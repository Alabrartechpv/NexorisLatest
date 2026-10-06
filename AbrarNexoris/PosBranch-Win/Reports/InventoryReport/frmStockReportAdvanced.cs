using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.InventoryReport
{
    public partial class frmStockReportAdvanced : Form
    {
        // ─── Theme Palette (matches frmItemReport / FrmSmartReorderDashboard) ────────
        private static readonly Color FormBackColor        = Color.FromArgb(232, 246, 255);
        private static readonly Color FilterPanelBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(206, 223, 238);
        private static readonly Color BorderBlue           = Color.FromArgb(118, 154, 198);
        private static readonly Color ControlBackColor     = Color.White;
        private static readonly Color ControlTextColor     = Color.FromArgb(18, 49, 102);
        private static readonly Color GridHeaderBlue       = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark   = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue     = Color.FromArgb(173, 216, 255);
        private static readonly Color GridRowLine          = Color.FromArgb(197, 217, 241);
        private static readonly Color GridAltRow           = Color.FromArgb(246, 250, 255);
        private static readonly Color GridFooterBorder     = Color.FromArgb(144, 181, 223);
        private static readonly Color SkyBlueOutline       = Color.FromArgb(160, 210, 255);

        private static readonly Color ButtonTopColor       = Color.FromArgb(234, 244, 255);
        private static readonly Color ButtonBottomColor    = Color.FromArgb(152, 188, 235);
        private static readonly Color ButtonBorderColor    = Color.FromArgb(73, 119, 184);
        private static readonly Color ButtonTextBlue       = Color.FromArgb(14, 47, 108);

        private static readonly Color PanelHoverTopColor   = Color.FromArgb(245, 250, 255);
        private static readonly Color PanelHoverBottomColor= Color.FromArgb(170, 206, 244);

        private static readonly Color PanelPressedTopColor = Color.FromArgb(205, 226, 248);
        private static readonly Color PanelPressedBottomColor = Color.FromArgb(128, 170, 224);

        private StockReportAdvanceRepo reportRepo;
        private Dropdowns dropdownRepo;
        private BackgroundWorker searchWorker;
        private bool isSearching = false;

        private readonly Dictionary<string, Label> _footerLabels = new Dictionary<string, Label>();
        private readonly Dictionary<string, string> _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // ─── Column Chooser & Drag-Down to Hide State ────────────────────────────────
        private ListBox columnChooserListBox;
        private Form columnChooserForm;
        private bool isDraggingHeaderToHide;
        private UltraGridColumn columnBeingDragged;
        private Point headerDragStartPoint;
        private readonly System.Windows.Forms.ToolTip headerToolTip = new System.Windows.Forms.ToolTip();
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Cursor blackXCursor = CreateBlackXCursor();

        public frmStockReportAdvanced()
        {
            InitializeComponent();
            InitializeForm();
            InitializeBackgroundWorker();
        }

        private void InitializeBackgroundWorker()
        {
            searchWorker = new BackgroundWorker();
            searchWorker.WorkerReportsProgress = true;
            searchWorker.WorkerSupportsCancellation = true;
            searchWorker.DoWork += SearchWorker_DoWork;
            searchWorker.RunWorkerCompleted += SearchWorker_RunWorkerCompleted;
        }

        private void InitializeForm()
        {
            try
            {
                reportRepo = new StockReportAdvanceRepo();
                dropdownRepo = new Dropdowns();

                // Apply unified theme appearance
                InitializeRuntimeAppearance();

                // Initialize Dates
                ultraDateTimeEditorFrom.Value = DateTime.Now.AddDays(-30);
                ultraDateTimeEditorTo.Value = DateTime.Now;
                InitializePresetDates();

                // Load Metadata
                LoadGroups();
                LoadCategories();
                LoadSubCategories();
                LoadLedgers();

                // Configure Grid
                ApplyGridStyling(ultraGridStock);
                ultraGridStock.InitializeLayout += UltraGridStock_InitializeLayout;
                ultraGridStock.InitializeRow += UltraGridStock_InitializeRow;

                // Register footer cell sync and column drag-to-hide handlers matching frmPurchaseReturn / frmStockReport
                ultraGridStock.Resize += (s, e) => UpdateFooterCellPositions();
                ultraGridStock.AfterColPosChanged += (s, e) => UpdateFooterCellPositions();
                ultraGridStock.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
                ultraGridStock.AfterRowRegionScroll += (s, e) => UpdateFooterCellPositions();
                ultraGridStock.Paint += (s, e) => UpdateFooterCellPositions();

                SetupHeaderDragToHideAndColumnChooser();
                InitializeGridFooter();

                // Style Buttons & Summary Cards
                StyleButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeRuntimeAppearance()
        {
            BackColor = FormBackColor;

            // Panels
            if (ultraPanelControls != null)
            {
                ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
                ultraPanelControls.Appearance.BorderColor = BorderBlue;
                ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;
            }

            if (ultraPanelFilters != null)
            {
                ultraPanelFilters.Appearance.BackColor = FilterPanelBackColor;
                ultraPanelFilters.Appearance.BorderColor = BorderBlue;
                ultraPanelFilters.BorderStyle = UIElementBorderStyle.Solid;
            }

            if (ultraPanelActionBar != null)
            {
                ultraPanelActionBar.Appearance.BackColor = ActionPanelBackColor;
                ultraPanelActionBar.Appearance.BorderColor = BorderBlue;
                ultraPanelActionBar.BorderStyle = UIElementBorderStyle.Solid;
                ultraPanelActionBar.Size = new Size(ultraPanelActionBar.Width, 38);
            }

            if (ultraPanelGrid != null)
            {
                ultraPanelGrid.Appearance.BackColor = FormBackColor;
                ultraPanelGrid.Appearance.BorderColor = BorderBlue;
                ultraPanelGrid.BorderStyle = UIElementBorderStyle.Solid;
            }

            if (gridFooterPanel != null)
            {
                gridFooterPanel.Appearance.BackColor = GridHeaderBlue;
                gridFooterPanel.Appearance.BackColor2 = GridHeaderBlue;
                gridFooterPanel.Appearance.BackGradientStyle = GradientStyle.None;
                gridFooterPanel.Appearance.BorderColor = GridFooterBorder;
                gridFooterPanel.BorderStyle = UIElementBorderStyle.Solid;
                gridFooterPanel.Height = 28;
            }

            // Labels
            StyleLabel(ultraLabelFromDate);
            StyleLabel(ultraLabelToDate);
            StyleLabel(ultraLabelPreset);
            StyleLabel(ultraLabelGroup);
            StyleLabel(ultraLabelCategory);
            StyleLabel(ultraLabelSubCategory);
            StyleLabel(ultraLabelBarcode);
            StyleLabel(ultraLabelLedger);

            // Controls
            StyleDateTimeEditor(ultraDateTimeEditorFrom);
            StyleDateTimeEditor(ultraDateTimeEditorTo);
            StyleFilterCombo(ultraComboPresetDates);
            StyleFilterCombo(ultraComboGroup);
            StyleFilterCombo(ultraComboCategory);
            StyleFilterCombo(ultraComboSubCategory);
            StyleFilterCombo(ultraComboLedger);
            StyleTextEditor(ultraTextEditorBarcode);

            // Summary Cards
            StyleSummaryCards();
        }

        private static void StyleLabel(Infragistics.Win.Misc.UltraLabel lbl)
        {
            if (lbl == null) return;
            lbl.Appearance.BackColor = Color.Transparent;
            lbl.Appearance.ForeColor = Color.FromArgb(18, 47, 95);
            lbl.Appearance.FontData.Bold = DefaultableBoolean.False;
            lbl.Appearance.FontData.Name = "Microsoft Sans Serif";
            lbl.Appearance.FontData.SizeInPoints = 9F;
        }

        private static void StyleFilterCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            if (combo == null) return;
            combo.UseAppStyling = false;
            combo.UseOsThemes = DefaultableBoolean.False;
            combo.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            combo.BorderStyle = UIElementBorderStyle.Solid;
            combo.Appearance.BackColor = ControlBackColor;
            combo.Appearance.BorderColor = SkyBlueOutline;
            combo.Appearance.ForeColor = ControlTextColor;
            combo.Appearance.FontData.Name = "Microsoft Sans Serif";
            combo.Appearance.FontData.SizeInPoints = 9F;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
        }

        private static void StyleTextEditor(Infragistics.Win.UltraWinEditors.UltraTextEditor editor)
        {
            if (editor == null) return;
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = ControlBackColor;
            editor.Appearance.BorderColor = SkyBlueOutline;
            editor.Appearance.ForeColor = ControlTextColor;
            editor.Appearance.FontData.Name = "Microsoft Sans Serif";
            editor.Appearance.FontData.SizeInPoints = 9F;
        }

        private static void StyleDateTimeEditor(Infragistics.Win.UltraWinEditors.UltraDateTimeEditor editor)
        {
            if (editor == null) return;
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = ControlBackColor;
            editor.Appearance.BorderColor = SkyBlueOutline;
            editor.Appearance.ForeColor = ControlTextColor;
            editor.Appearance.FontData.Name = "Microsoft Sans Serif";
            editor.Appearance.FontData.SizeInPoints = 9F;
            editor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
        }

        private void StyleSummaryCards()
        {
            UltraLabel[] captions = new UltraLabel[]
            {
                ultraLabelTotalItemsCaption, ultraLabelTotalValueCaption, ultraLabelTotalSalesCaption,
                ultraLabelTotalPurchaseCaption, ultraLabelTotalProfitCaption
            };

            UltraLabel[] values = new UltraLabel[]
            {
                ultraLabelTotalItemsValue, ultraLabelTotalValueValue, ultraLabelTotalSalesValue,
                ultraLabelTotalPurchaseValue, ultraLabelTotalProfitValue
            };

            foreach (var cap in captions)
            {
                if (cap == null) continue;
                cap.Appearance.BackColor = Color.Transparent;
                cap.Appearance.ForeColor = Color.FromArgb(14, 47, 108);
                cap.Appearance.FontData.Name = "Microsoft Sans Serif";
                cap.Appearance.FontData.SizeInPoints = 8.25F;
                cap.Appearance.FontData.Bold = DefaultableBoolean.True;
                cap.Appearance.TextHAlign = Infragistics.Win.HAlign.Center;
                cap.Appearance.TextVAlign = Infragistics.Win.VAlign.Middle;
            }

            foreach (var val in values)
            {
                if (val == null) continue;
                val.Appearance.BackColor = Color.White;
                val.Appearance.BorderColor = SkyBlueOutline;
                val.BorderStyleInner = UIElementBorderStyle.Solid;
                val.Appearance.ForeColor = ControlTextColor;
                val.Appearance.FontData.Name = "Microsoft Sans Serif";
                val.Appearance.FontData.SizeInPoints = 12F;
                val.Appearance.FontData.Bold = DefaultableBoolean.True;
                val.Appearance.TextHAlign = Infragistics.Win.HAlign.Center;
                val.Appearance.TextVAlign = Infragistics.Win.VAlign.Middle;
            }
        }

        private void InitializePresetDates()
        {
            ultraComboPresetDates.Items.Clear();
            ultraComboPresetDates.Items.Add("ALL", "ALL");
            ultraComboPresetDates.Items.Add("Today", "Today");
            ultraComboPresetDates.Items.Add("Yesterday", "Yesterday");
            ultraComboPresetDates.Items.Add("This Week", "This Week");
            ultraComboPresetDates.Items.Add("Last Week", "Last Week");
            ultraComboPresetDates.Items.Add("This Month", "This Month");
            ultraComboPresetDates.Items.Add("Last Month", "Last Month");
            ultraComboPresetDates.Value = "ALL";
        }

        private void LoadGroups()
        {
            try
            {
                var groups = dropdownRepo.getGroupDDl();
                if (groups != null && groups.List != null)
                {
                    var list = groups.List.ToList();
                    ultraComboGroup.DataSource = list;
                    ultraComboGroup.ValueMember = "Id";
                    ultraComboGroup.DisplayMember = "GroupName";
                }
            }
            catch { }
        }

        private void LoadCategories()
        {
            try
            {
                var categories = dropdownRepo.getCategoryDDl("");
                if (categories != null && categories.List != null)
                {
                    var list = categories.List.ToList();
                    ultraComboCategory.DataSource = list;
                    ultraComboCategory.ValueMember = "Id";
                    ultraComboCategory.DisplayMember = "CategoryName";
                }
            }
            catch { }
        }

        private void LoadSubCategories()
        {
        }

        private void LoadLedgers()
        {
            try
            {
                var vendors = dropdownRepo.VendorDDL();
                if (vendors != null && vendors.List != null)
                {
                    var list = vendors.List.ToList();
                    ultraComboLedger.DataSource = list;
                    ultraComboLedger.ValueMember = "LedgerID";
                    ultraComboLedger.DisplayMember = "LedgerName";
                }
            }
            catch { }
        }

        private void ApplyGridStyling(UltraGrid targetGrid)
        {
            if (targetGrid == null) return;

            targetGrid.UseAppStyling = false;
            targetGrid.UseOsThemes = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Appearance.BackColor = FormBackColor;
            targetGrid.DisplayLayout.AutoFitStyle = AutoFitStyle.None;
            targetGrid.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
            targetGrid.DisplayLayout.Scrollbars = Scrollbars.Both;
            targetGrid.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            targetGrid.DisplayLayout.GroupByBox.Hidden = true;
            targetGrid.DisplayLayout.GroupByBox.BorderStyle = UIElementBorderStyle.None;

            targetGrid.DisplayLayout.Override.HeaderStyle = HeaderStyle.Standard;
            targetGrid.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            targetGrid.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            targetGrid.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
            targetGrid.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
            targetGrid.DisplayLayout.Override.AllowRowFiltering = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;

            targetGrid.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.ColumnChooserButton;
            targetGrid.DisplayLayout.Override.RowSelectorWidth = 25;
            targetGrid.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.BackColor = GridHeaderBlueDark;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.BorderColor = BorderBlue;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.TextHAlign = Infragistics.Win.HAlign.Center;

            targetGrid.DisplayLayout.Override.MinRowHeight = 24;
            targetGrid.DisplayLayout.Override.DefaultRowHeight = 24;
            targetGrid.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            targetGrid.DisplayLayout.Override.RowAppearance.ForeColor = ControlTextColor;
            targetGrid.DisplayLayout.Override.RowAppearance.BorderColor = GridRowLine;
            targetGrid.DisplayLayout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            targetGrid.DisplayLayout.Override.RowAlternateAppearance.BorderColor = GridRowLine;
            targetGrid.DisplayLayout.Override.ActiveRowAppearance.BackColor = GridSelectedBlue;
            targetGrid.DisplayLayout.Override.ActiveRowAppearance.ForeColor = ControlTextColor;
            targetGrid.DisplayLayout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            targetGrid.DisplayLayout.Override.SelectedRowAppearance.ForeColor = ControlTextColor;

            targetGrid.DisplayLayout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            targetGrid.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BorderColor = BorderBlue;
            targetGrid.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
            targetGrid.DisplayLayout.Override.HeaderAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;
            targetGrid.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            targetGrid.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;
            targetGrid.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

            targetGrid.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.CellAppearance.BorderColor = GridRowLine;
            targetGrid.DisplayLayout.Override.CellAppearance.ForeColor = ControlTextColor;
            targetGrid.DisplayLayout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            targetGrid.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
            targetGrid.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;
        }

        private void StyleButtons()
        {
            StyleButton(btnSearch);
            StyleButton(btnClearFilters);
            StyleButton(btnExport);
            StyleButton(btnPrint);
            StyleButton(btnClose);
            StyleButton(btnHideSelection);

            SetupEnhancedSummaryPanel();
        }

        private static void StyleButton(Infragistics.Win.Misc.UltraButton button)
        {
            if (button == null) return;
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.ButtonStyle = UIElementButtonStyle.Office2013Button;
            button.Appearance.BackColor = ButtonTopColor;
            button.Appearance.BackColor2 = ButtonBottomColor;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.BorderColor = ButtonBorderColor;
            button.Appearance.ForeColor = ButtonTextBlue;
            button.Appearance.FontData.Name = "Microsoft Sans Serif";
            button.Appearance.FontData.SizeInPoints = 9F;
            button.Appearance.FontData.Bold = DefaultableBoolean.False;

            button.HotTrackAppearance.BackColor = PanelHoverTopColor;
            button.HotTrackAppearance.BackColor2 = PanelHoverBottomColor;
            button.HotTrackAppearance.BorderColor = ButtonBorderColor;
            button.HotTrackAppearance.ForeColor = ButtonTextBlue;

            button.PressedAppearance.BackColor = PanelPressedTopColor;
            button.PressedAppearance.BackColor2 = PanelPressedBottomColor;
            button.PressedAppearance.BorderColor = ButtonBorderColor;
            button.PressedAppearance.ForeColor = ButtonTextBlue;
        }

        private void SetupEnhancedSummaryPanel()
        {
            if (ultraPanelSummary == null) return;

            ultraPanelSummary.Dock = DockStyle.Bottom;
            ultraPanelSummary.Height = 72;
            ultraPanelSummary.Visible = true;

            gridFooterPanel.Dock = DockStyle.Bottom;
            gridFooterPanel.Height = 28;
            ultraGridStock.Dock = DockStyle.Fill;

            ultraPanelSummary.ClientArea.AutoScroll = false;
            ultraPanelSummary.Resize += (s, e) => AlignSummaryCards();
        }

        private void btnHideSelection_Click(object sender, EventArgs e)
        {
            bool show = !ultraPanelControls.Visible;
            ultraPanelControls.Visible = show;
            ultraPanelFilters.Visible = show;
            btnHideSelection.Text = show ? "Hide Selection" : "Show Selection";
            LayoutPanels();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutPanels();
        }

        private void frmStockReportAdvanced_Load(object sender, EventArgs e)
        {
            LayoutPanels();
        }

        private void LayoutPanels()
        {
            if (ultraPanelActionBar == null || ultraPanelGrid == null || ultraPanelSummary == null || ultraPanelControls == null || ultraPanelFilters == null) return;

            if (TopLevel == false)
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Normal;
                Dock = DockStyle.Fill;
            }

            SuspendLayout();

            ultraPanelControls.Dock = DockStyle.Top;
            ultraPanelFilters.Dock = DockStyle.Top;
            ultraPanelActionBar.Dock = DockStyle.Top;
            ultraPanelSummary.Dock = DockStyle.Bottom;
            ultraPanelSummary.Height = 72;
            ultraPanelSummary.Visible = true;
            ultraPanelGrid.Dock = DockStyle.Fill;

            if (!Controls.Contains(ultraPanelControls)) Controls.Add(ultraPanelControls);
            if (!Controls.Contains(ultraPanelFilters)) Controls.Add(ultraPanelFilters);
            if (!Controls.Contains(ultraPanelActionBar)) Controls.Add(ultraPanelActionBar);
            if (!Controls.Contains(ultraPanelSummary)) Controls.Add(ultraPanelSummary);
            if (!Controls.Contains(ultraPanelGrid)) Controls.Add(ultraPanelGrid);

            Controls.SetChildIndex(ultraPanelControls, 4);
            Controls.SetChildIndex(ultraPanelFilters, 3);
            Controls.SetChildIndex(ultraPanelActionBar, 2);
            Controls.SetChildIndex(ultraPanelSummary, 1);
            Controls.SetChildIndex(ultraPanelGrid, 0);

            if (ultraPanelGrid.ClientArea != null)
            {
                ultraPanelGrid.ClientArea.SuspendLayout();
                gridFooterPanel.Dock = DockStyle.Bottom;
                gridFooterPanel.Height = 28;
                ultraGridStock.Dock = DockStyle.Fill;

                if (!ultraPanelGrid.ClientArea.Controls.Contains(gridFooterPanel))
                    ultraPanelGrid.ClientArea.Controls.Add(gridFooterPanel);
                if (!ultraPanelGrid.ClientArea.Controls.Contains(ultraGridStock))
                    ultraPanelGrid.ClientArea.Controls.Add(ultraGridStock);

                ultraPanelGrid.ClientArea.Controls.SetChildIndex(gridFooterPanel, 1);
                ultraPanelGrid.ClientArea.Controls.SetChildIndex(ultraGridStock, 0);

                ultraPanelGrid.ClientArea.ResumeLayout(true);
                ultraPanelGrid.ClientArea.PerformLayout();
            }

            ResumeLayout(true);
            PerformLayout();

            AlignSummaryCards();
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private void AlignSummaryCards()
        {
            if (ultraPanelSummary == null || ultraPanelSummary.ClientArea == null) return;
            int totalWidth = ultraPanelSummary.ClientArea.Width;
            if (totalWidth <= 0) return;

            UltraLabel[] captions = new UltraLabel[]
            {
                ultraLabelTotalItemsCaption, ultraLabelTotalValueCaption, ultraLabelTotalSalesCaption,
                ultraLabelTotalPurchaseCaption, ultraLabelTotalProfitCaption
            };

            UltraLabel[] values = new UltraLabel[]
            {
                ultraLabelTotalItemsValue, ultraLabelTotalValueValue, ultraLabelTotalSalesValue,
                ultraLabelTotalPurchaseValue, ultraLabelTotalProfitValue
            };

            int count = 5;
            int padding = 16;
            int baseCardWidth = 160;

            int availableWidth = totalWidth - (padding * 2);
            if (availableWidth <= 0) return;

            int gap = 12;
            int computedWidth = (availableWidth - (gap * (count - 1))) / count;
            int cardWidth = Math.Max(baseCardWidth, Math.Min(260, computedWidth));

            int remainingForGaps = availableWidth - (count * cardWidth);
            if (count > 1)
            {
                gap = Math.Max(8, remainingForGaps / (count - 1));
            }

            int currentX = padding;
            for (int i = 0; i < count; i++)
            {
                if (captions[i] != null)
                {
                    captions[i].Location = new Point(currentX, 2);
                    captions[i].Size = new Size(cardWidth, 16);
                }
                if (values[i] != null)
                {
                    values[i].Location = new Point(currentX, 18);
                    values[i].Size = new Size(cardWidth, 48);
                }
                currentX += cardWidth + gap;
            }
        }

        #region Column Chooser & Drag-Down to Hide

        private static Cursor CreateBlackXCursor()
        {
            try
            {
                using (Bitmap bmp = new Bitmap(32, 32))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);

                    using (SolidBrush bgBrush = new SolidBrush(Color.Black))
                    {
                        g.FillEllipse(bgBrush, 4, 4, 24, 24);
                    }

                    using (Pen whitePen = new Pen(Color.White, 3.5f))
                    {
                        whitePen.StartCap = LineCap.Round;
                        whitePen.EndCap = LineCap.Round;
                        g.DrawLine(whitePen, 11, 11, 21, 21);
                        g.DrawLine(whitePen, 21, 11, 11, 21);
                    }

                    IntPtr hIcon = bmp.GetHicon();
                    return new Cursor(hIcon);
                }
            }
            catch
            {
                return Cursors.No;
            }
        }

        private void SetupHeaderDragToHideAndColumnChooser()
        {
            ultraGridStock.AllowDrop = true;
            ultraGridStock.MouseDown += Grid_MouseDown;
            ultraGridStock.MouseMove += Grid_MouseMove;
            ultraGridStock.MouseUp += Grid_MouseUp;
            ultraGridStock.DragOver += Grid_DragOver;
            ultraGridStock.DragDrop += Grid_DragDrop;

            ContextMenuStrip headerMenu = new ContextMenuStrip { Font = new Font("Segoe UI", 9F) };
            ToolStripMenuItem chooserItem = new ToolStripMenuItem("📋 Field / Column Chooser...", null, (s, e) => ShowColumnChooserForm());
            chooserItem.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            headerMenu.Items.Add(chooserItem);

            ToolStripMenuItem showAllItem = new ToolStripMenuItem("🔓 Show / Unhide All Columns", null, (s, e) => UnhideAllColumns());
            headerMenu.Items.Add(showAllItem);

            ultraGridStock.ContextMenuStrip = headerMenu;
        }

        private void Grid_MouseDown(object sender, MouseEventArgs e)
        {
            if (ultraGridStock.DisplayLayout == null || ultraGridStock.DisplayLayout.Bands.Count == 0)
                return;

            UIElement element = ultraGridStock.DisplayLayout.UIElement?.ElementFromPoint(new Point(e.X, e.Y));
            HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

            UltraGridColumn col = headerUI?.Header?.Column;
            if (headerUI != null && col != null)
            {
                if (e.Button == MouseButtons.Right)
                {
                    ShowHeaderContextMenu(col, e.Location);
                    return;
                }

                if (e.Button == MouseButtons.Left)
                {
                    isDraggingHeaderToHide = true;
                    columnBeingDragged = col;
                    headerDragStartPoint = new Point(e.X, e.Y);
                }
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDraggingHeaderToHide || columnBeingDragged == null || e.Button != MouseButtons.Left)
                return;

            int deltaX = Math.Abs(e.X - headerDragStartPoint.X);
            int deltaY = e.Y - headerDragStartPoint.Y;

            if (deltaY > 20 && deltaY > deltaX)
            {
                ultraGridStock.Cursor = blackXCursor;
                string colName = !string.IsNullOrEmpty(columnBeingDragged.Header.Caption) ? columnBeingDragged.Header.Caption : columnBeingDragged.Key;
                headerToolTip.SetToolTip(ultraGridStock, $"✖ Drag down to hide '{colName}' column");
            }
            else
            {
                ultraGridStock.Cursor = Cursors.Default;
                headerToolTip.SetToolTip(ultraGridStock, string.Empty);
            }
        }

        private void Grid_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDraggingHeaderToHide)
            {
                if (columnBeingDragged != null && (e.Y - headerDragStartPoint.Y) > 40)
                {
                    HideColumn(columnBeingDragged);
                }
                isDraggingHeaderToHide = false;
                columnBeingDragged = null;
                ultraGridStock.Cursor = Cursors.Default;
                headerToolTip.SetToolTip(ultraGridStock, string.Empty);
            }
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ColumnChooserItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void Grid_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(ColumnChooserItem)) is ColumnChooserItem item)
            {
                Point clientPt = ultraGridStock.PointToClient(new Point(e.X, e.Y));
                int dropPosition = GetTargetColumnPositionFromPoint(clientPt);
                UnhideColumn(item.ColumnKey, dropPosition);
            }
        }

        private int GetTargetColumnPositionFromPoint(Point pt)
        {
            if (ultraGridStock.DisplayLayout == null || ultraGridStock.DisplayLayout.Bands.Count == 0)
                return 0;

            UIElement element = ultraGridStock.DisplayLayout.UIElement?.ElementFromPoint(pt);
            HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

            if (headerUI != null && headerUI.Header?.Column != null)
            {
                return headerUI.Header.Column.Header.VisiblePosition;
            }

            UltraGridBand band = ultraGridStock.DisplayLayout.Bands[0];
            foreach (UltraGridColumn col in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (!col.Hidden)
                {
                    UIElement hUI = col.Header.GetUIElement();
                    if (hUI != null && pt.X >= hUI.Rect.Left && pt.X <= hUI.Rect.Right)
                    {
                        return col.Header.VisiblePosition;
                    }
                }
            }

            return band.Columns.Count;
        }

        private void HideColumn(UltraGridColumn col)
        {
            if (col == null) return;
            userHiddenColumnKeys.Add(col.Key);
            col.Hidden = true;
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                PopulateColumnChooserListBox();
            }
        }

        private void ShowHeaderContextMenu(UltraGridColumn col, Point location)
        {
            if (col == null) return;
            ContextMenuStrip menu = new ContextMenuStrip { Font = new Font("Segoe UI", 9F) };
            string colName = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;

            ToolStripMenuItem hideItem = new ToolStripMenuItem($"🙈 Hide Column '{colName}'", null, (s, e) => HideColumn(col));
            hideItem.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            menu.Items.Add(hideItem);

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem chooserItem = new ToolStripMenuItem("📋 Field / Column Chooser...", null, (s, e) => ShowColumnChooserForm());
            menu.Items.Add(chooserItem);

            ToolStripMenuItem showAllItem = new ToolStripMenuItem("🔓 Show / Unhide All Columns", null, (s, e) => UnhideAllColumns());
            menu.Items.Add(showAllItem);

            menu.Show(ultraGridStock, location);
        }

        private void ShowColumnChooserForm()
        {
            if (columnChooserForm == null || columnChooserForm.IsDisposed)
            {
                CreateColumnChooserForm();
            }

            PopulateColumnChooserListBox();
            columnChooserForm.Show(this);
            PositionColumnChooser();
        }

        private void CreateColumnChooserForm()
        {
            columnChooserForm = new Form
            {
                Text = "Customization (Field Chooser)",
                Size = new Size(240, 300),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(240, 244, 248),
                ShowIcon = false,
                ShowInTaskbar = false
            };

            columnChooserForm.FormClosing += (s, e) =>
            {
                e.Cancel = true;
                columnChooserForm.Hide();
            };

            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                AllowDrop = true,
                DrawMode = DrawMode.OwnerDrawFixed,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(240, 244, 248),
                ItemHeight = 34,
                IntegralHeight = false
            };

            columnChooserListBox.DrawItem += ColumnChooserListBox_DrawItem;
            columnChooserListBox.DoubleClick += ColumnChooserListBox_DoubleClick;
            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;

            columnChooserForm.Controls.Add(columnChooserListBox);
        }

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && columnChooserListBox != null)
            {
                int index = columnChooserListBox.IndexFromPoint(e.Location);
                if (index >= 0 && index < columnChooserListBox.Items.Count)
                {
                    if (columnChooserListBox.Items[index] is ColumnChooserItem item)
                    {
                        columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
                    }
                }
            }
        }

        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null || ultraGridStock.DisplayLayout.Bands.Count == 0)
                return;

            columnChooserListBox.Items.Clear();
            UltraGridBand band = ultraGridStock.DisplayLayout.Bands[0];

            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.Hidden && !col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                {
                    string caption = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;
                    columnChooserListBox.Items.Add(new ColumnChooserItem(col.Key, caption));
                }
            }
        }

        private void ColumnChooserListBox_DoubleClick(object sender, EventArgs e)
        {
            if (columnChooserListBox.SelectedItem is ColumnChooserItem item)
            {
                UnhideColumn(item.ColumnKey);
            }
        }

        private void UnhideColumn(string columnKey, int? targetVisiblePosition = null)
        {
            userHiddenColumnKeys.Remove(columnKey);
            if (ultraGridStock.DisplayLayout.Bands.Count > 0 && ultraGridStock.DisplayLayout.Bands[0].Columns.Exists(columnKey))
            {
                UltraGridColumn col = ultraGridStock.DisplayLayout.Bands[0].Columns[columnKey];
                col.Hidden = false;
                if (targetVisiblePosition.HasValue)
                {
                    col.Header.VisiblePosition = targetVisiblePosition.Value;
                }
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
                PopulateColumnChooserListBox();
            }
        }

        private void UnhideAllColumns()
        {
            userHiddenColumnKeys.Clear();
            if (ultraGridStock.DisplayLayout.Bands.Count == 0) return;
            UltraGridBand band = ultraGridStock.DisplayLayout.Bands[0];
            foreach (UltraGridColumn col in band.Columns)
            {
                if (!col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                {
                    col.Hidden = false;
                }
            }
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            PopulateColumnChooserListBox();
        }

        private void PositionColumnChooser()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed && columnChooserForm.Visible)
            {
                columnChooserForm.Location = new Point(
                    Right - columnChooserForm.Width - 30,
                    Bottom - columnChooserForm.Height - 30);
                columnChooserForm.BringToFront();
            }
        }

        private void ColumnChooserListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || columnChooserListBox == null || e.Index >= columnChooserListBox.Items.Count)
                return;

            if (!(columnChooserListBox.Items[e.Index] is ColumnChooserItem item))
                return;

            Rectangle rect = e.Bounds;
            rect.Inflate(-4, -3);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(0, 121, 211)))
            using (GraphicsPath path = RoundedRect(rect, 4))
            {
                e.Graphics.FillPath(bgBrush, path);
            }

            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                StringFormat sf = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Center
                };
                using (Font textFont = new Font("Segoe UI", 9F, FontStyle.Bold))
                {
                    e.Graphics.DrawString(item.DisplayText, textFont, textBrush, rect, sf);
                }
            }
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(arc, 180, 90);

            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        private sealed class ColumnChooserItem
        {
            public string ColumnKey { get; }
            public string DisplayText { get; }

            public ColumnChooserItem(string key, string text)
            {
                ColumnKey = key;
                DisplayText = text;
            }

            public override string ToString()
            {
                return DisplayText;
            }
        }

        #endregion

        #region GridFooterPanel Dynamic Alignment & Calculation

        private void InitializeGridFooter()
        {
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private void CreateFooterCells()
        {
            if (gridFooterPanel == null) return;
            gridFooterPanel.ClientArea.Controls.Clear();
            _footerLabels.Clear();

            if (ultraGridStock.DisplayLayout == null || ultraGridStock.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = ultraGridStock.DisplayLayout.Bands[0];
            int xOffset = ultraGridStock.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True
                ? ultraGridStock.DisplayLayout.Override.RowSelectorWidth
                : 0;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden)
                    continue;

                Label footerLabel = new Label
                {
                    Name = "footer_" + column.Key,
                    Text = string.Empty,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = GridHeaderBlue,
                    BorderStyle = BorderStyle.None,
                    AutoSize = false,
                    Width = column.Width,
                    Height = Math.Max(gridFooterPanel.Height - 2, 20),
                    Left = xOffset,
                    Top = 1,
                    Tag = Tuple.Create(column.Key, string.Empty),
                    ForeColor = Color.White,
                    Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0),
                    ContextMenuStrip = CreateFooterContextMenu(column.Key)
                };

                footerLabel.Paint += FooterLabel_Paint;
                gridFooterPanel.ClientArea.Controls.Add(footerLabel);
                _footerLabels[column.Key] = footerLabel;

                if (!_columnAggregations.ContainsKey(column.Key))
                {
                    _columnAggregations[column.Key] = "None";
                }

                xOffset += column.Width;
            }
        }

        private void FooterLabel_Paint(object sender, PaintEventArgs e)
        {
            Label lbl = sender as Label;
            if (lbl == null) return;

            using (Pen borderPen = new Pen(Color.FromArgb(118, 154, 198), 1))
            {
                e.Graphics.DrawLine(borderPen, lbl.Width - 1, 0, lbl.Width - 1, lbl.Height);
            }
        }

        private ContextMenuStrip CreateFooterContextMenu(string columnKey)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Tag = columnKey;

            bool isNumeric = ultraGridStock.DisplayLayout.Bands.Count > 0 &&
                             ultraGridStock.DisplayLayout.Bands[0].Columns.Exists(columnKey) &&
                             IsSummableColumn(ultraGridStock.DisplayLayout.Bands[0].Columns[columnKey]);

            ToolStripMenuItem itemSum = new ToolStripMenuItem("Sum");
            itemSum.Tag = "Sum";
            itemSum.Enabled = isNumeric;
            itemSum.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemMin = new ToolStripMenuItem("Min");
            itemMin.Tag = "Min";
            itemMin.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemMax = new ToolStripMenuItem("Max");
            itemMax.Tag = "Max";
            itemMax.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemCount = new ToolStripMenuItem("Count");
            itemCount.Tag = "Count";
            itemCount.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemAverage = new ToolStripMenuItem("Average");
            itemAverage.Tag = "Avg";
            itemAverage.Enabled = isNumeric;
            itemAverage.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemNone = new ToolStripMenuItem("None");
            itemNone.Tag = "None";
            itemNone.Click += FooterContextMenu_Click;

            menu.Items.Add(itemSum);
            menu.Items.Add(itemMin);
            menu.Items.Add(itemMax);
            menu.Items.Add(itemCount);
            menu.Items.Add(itemAverage);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(itemNone);

            menu.Opening += (sender, e) =>
            {
                string currentAggregation = _columnAggregations.ContainsKey(columnKey)
                    ? _columnAggregations[columnKey]
                    : "None";

                foreach (ToolStripItem menuItem in menu.Items)
                {
                    ToolStripMenuItem toolStripMenuItem = menuItem as ToolStripMenuItem;
                    if (toolStripMenuItem != null && toolStripMenuItem.Tag != null)
                    {
                        toolStripMenuItem.Checked = string.Equals(toolStripMenuItem.Tag.ToString(), currentAggregation, StringComparison.OrdinalIgnoreCase);
                    }
                }
            };

            return menu;
        }

        private bool IsSummableColumn(UltraGridColumn column)
        {
            if (column == null || column.DataType == null) return false;
            Type t = System.Nullable.GetUnderlyingType(column.DataType) ?? column.DataType;
            return t == typeof(decimal) || t == typeof(double) || t == typeof(float) ||
                   t == typeof(int) || t == typeof(long) || t == typeof(short);
        }

        private void FooterContextMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item == null)
                return;

            ContextMenuStrip menu = item.Owner as ContextMenuStrip;
            if (menu == null || menu.Tag == null || item.Tag == null)
                return;

            string columnKey = menu.Tag.ToString();
            string aggregation = item.Tag.ToString();

            _columnAggregations[columnKey] = aggregation;
            UpdateFooterValues();
        }

        private void UpdateFooterCellPositions()
        {
            if (ultraGridStock.DisplayLayout == null || ultraGridStock.DisplayLayout.Bands.Count == 0 || _footerLabels.Count == 0 || gridFooterPanel == null)
                return;

            UltraGridBand band = ultraGridStock.DisplayLayout.Bands[0];
            int rowSelectorWidth = ultraGridStock.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True
                ? ultraGridStock.DisplayLayout.Override.RowSelectorWidth
                : 0;
            int scrollOffset = 0;
            if (ultraGridStock.ActiveColScrollRegion != null)
            {
                scrollOffset = ultraGridStock.ActiveColScrollRegion.Position;
            }

            int calculatedX = rowSelectorWidth - scrollOffset;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden || !_footerLabels.ContainsKey(column.Key))
                    continue;

                Label footerLabel = _footerLabels[column.Key];
                var headerUI = column.Header.GetUIElement();
                int left, width;

                if (headerUI != null)
                {
                    left = headerUI.Rect.Left;
                    width = headerUI.Rect.Width;
                }
                else
                {
                    left = calculatedX;
                    width = column.Width;
                }

                calculatedX += column.Width;

                footerLabel.Left = left;
                footerLabel.Width = width;
                footerLabel.Top = 0;
                footerLabel.Height = gridFooterPanel.Height;
                footerLabel.Visible = (left + width > 0 && left < gridFooterPanel.Width);
                footerLabel.Invalidate();
            }
        }

        private void UpdateFooterValues()
        {
            if (_footerLabels.Count == 0)
                return;

            List<UltraGridRow> visibleRows = GetVisibleDataRows().ToList();
            foreach (KeyValuePair<string, Label> footerEntry in _footerLabels)
            {
                string columnKey = footerEntry.Key;
                Label footerLabel = footerEntry.Value;

                if (!_columnAggregations.ContainsKey(columnKey) ||
                    string.Equals(_columnAggregations[columnKey], "None", StringComparison.OrdinalIgnoreCase))
                {
                    footerLabel.Text = string.Empty;
                    footerLabel.Tag = Tuple.Create(columnKey, string.Empty);
                    footerLabel.Invalidate();
                    continue;
                }

                object result = CalculateAggregation(columnKey, _columnAggregations[columnKey], visibleRows);
                string displayValue = FormatAggregationResult(columnKey, _columnAggregations[columnKey], result);

                footerLabel.Text = displayValue;
                footerLabel.Tag = Tuple.Create(columnKey, displayValue);
                footerLabel.ForeColor = Color.White;
                footerLabel.Invalidate();
            }
        }

        private IEnumerable<UltraGridRow> GetVisibleDataRows()
        {
            if (ultraGridStock.Rows == null) yield break;
            foreach (UltraGridRow row in ultraGridStock.Rows)
            {
                if (row != null && row.IsDataRow && !row.IsFilteredOut)
                    yield return row;
            }
        }

        private object CalculateAggregation(string columnKey, string aggregation, List<UltraGridRow> visibleRows)
        {
            if (visibleRows == null || visibleRows.Count == 0)
                return null;

            switch (aggregation)
            {
                case "Sum":
                    return visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => GetNumericValue(row.Cells[columnKey].Value))
                        .Where(value => value.HasValue)
                        .Sum(value => value.Value);
                case "Min":
                    return visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => row.Cells[columnKey].Value)
                        .Where(HasCellValue)
                        .Cast<IComparable>()
                        .OrderBy(value => value)
                        .FirstOrDefault();
                case "Max":
                    return visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => row.Cells[columnKey].Value)
                        .Where(HasCellValue)
                        .Cast<IComparable>()
                        .OrderByDescending(value => value)
                        .FirstOrDefault();
                case "Count":
                    return visibleRows.Count(row => row.Cells.Exists(columnKey) && HasCellValue(row.Cells[columnKey].Value));
                case "Avg":
                    List<decimal> values = visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => GetNumericValue(row.Cells[columnKey].Value))
                        .Where(value => value.HasValue)
                        .Select(value => value.Value)
                        .ToList();
                    return values.Count == 0 ? 0m : values.Average();
                default:
                    return null;
            }
        }

        private string FormatAggregationResult(string columnKey, string aggregation, object result)
        {
            if (string.Equals(aggregation, "None", StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            if (result == null)
            {
                if (string.Equals(aggregation, "Count", StringComparison.OrdinalIgnoreCase))
                    return "0";
                if (string.Equals(aggregation, "Sum", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(aggregation, "Avg", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(aggregation, "Min", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(aggregation, "Max", StringComparison.OrdinalIgnoreCase))
                    return "0.00";
                return string.Empty;
            }

            if (aggregation == "Count")
                return Convert.ToString(result);

            if (ultraGridStock.DisplayLayout != null &&
                ultraGridStock.DisplayLayout.Bands.Count > 0 &&
                ultraGridStock.DisplayLayout.Bands[0].Columns.Exists(columnKey))
            {
                UltraGridColumn column = ultraGridStock.DisplayLayout.Bands[0].Columns[columnKey];
                decimal? numericValue = GetNumericValue(result);
                if (numericValue.HasValue)
                {
                    if (!string.IsNullOrWhiteSpace(column.Format))
                        return numericValue.Value.ToString(column.Format);

                    return numericValue.Value.ToString("N2");
                }
            }

            return Convert.ToString(result);
        }

        private static decimal? GetNumericValue(object rawValue)
        {
            if (rawValue == null || rawValue == DBNull.Value)
                return null;

            if (rawValue is decimal decVal) return decVal;
            if (rawValue is double dblVal) return Convert.ToDecimal(dblVal);
            if (rawValue is float fltVal) return Convert.ToDecimal(fltVal);
            if (rawValue is int intVal) return intVal;
            if (rawValue is long longVal) return longVal;
            if (rawValue is short shortVal) return shortVal;

            return decimal.TryParse(Convert.ToString(rawValue), out decimal parsed) ? parsed : (decimal?)null;
        }

        private static bool HasCellValue(object value)
        {
            return value != null && value != DBNull.Value && !string.IsNullOrWhiteSpace(Convert.ToString(value));
        }

        #endregion

        private void UltraGridStock_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0) return;

            UltraGridBand band = e.Layout.Bands[0];
            foreach (UltraGridColumn col in band.Columns)
                col.Hidden = true;

            // Columns: Category | Group | Barcode | Description | Cost | Selling Price | UOM | Stock | Hold | Available | Stock Value | Future Sales Value
            ConfigureColumn(band, "CategoryName", "Category", 110, null, HAlign.Left, 0);
            ConfigureColumn(band, "GroupName", "Group", 100, null, HAlign.Left, 1);
            ConfigureColumn(band, "Barcode", "Barcode", 120, null, HAlign.Left, 2);
            ConfigureColumn(band, "ItemName", "Description", 220, null, HAlign.Left, 3);
            ConfigureColumn(band, "Cost", "Cost", 90, "#,##0.00", HAlign.Right, 4);
            ConfigureColumn(band, "RetailPrice", "Selling Price", 90, "#,##0.00", HAlign.Right, 5);
            ConfigureColumn(band, "BaseUnitName", "UOM", 70, null, HAlign.Center, 6);
            ConfigureColumn(band, "ClosingStock", "Stock", 80, "#,##0.##", HAlign.Right, 7);
            ConfigureColumn(band, "HoldQty", "Hold", 70, "#,##0.##", HAlign.Right, 8);
            ConfigureColumn(band, "AvailableStock", "Available", 85, "#,##0.##", HAlign.Right, 9);
            ConfigureColumn(band, "StockValue", "Stock Value", 100, "#,##0.00", HAlign.Right, 10);
            ConfigureColumn(band, "FutureSalesValue", "Future Sales Value", 110, "#,##0.00", HAlign.Right, 11);

            // Configure friendly names for additional hidden fields in model for column chooser
            if (band.Columns.Exists("OpeningStock")) band.Columns["OpeningStock"].Header.Caption = "Opening Stock";
            if (band.Columns.Exists("Purchase")) band.Columns["Purchase"].Header.Caption = "Purchase";
            if (band.Columns.Exists("PurchaseReturn")) band.Columns["PurchaseReturn"].Header.Caption = "Purchase Return";
            if (band.Columns.Exists("StockAdjustmentIn")) band.Columns["StockAdjustmentIn"].Header.Caption = "Adj In";
            if (band.Columns.Exists("StockAdjustmentOut")) band.Columns["StockAdjustmentOut"].Header.Caption = "Adj Out";
            if (band.Columns.Exists("StockTransferIn")) band.Columns["StockTransferIn"].Header.Caption = "Transfer In";
            if (band.Columns.Exists("StockTransferOut")) band.Columns["StockTransferOut"].Header.Caption = "Transfer Out";
            if (band.Columns.Exists("Sales")) band.Columns["Sales"].Header.Caption = "Sales";
            if (band.Columns.Exists("SalesReturn")) band.Columns["SalesReturn"].Header.Caption = "Sales Return";
            if (band.Columns.Exists("OrderedStock")) band.Columns["OrderedStock"].Header.Caption = "Ordered Stock";
            if (band.Columns.Exists("WholeSalePrice")) band.Columns["WholeSalePrice"].Header.Caption = "Wholesale Price";
            if (band.Columns.Exists("CreditPrice")) band.Columns["CreditPrice"].Header.Caption = "Credit Price";
            if (band.Columns.Exists("Profit")) band.Columns["Profit"].Header.Caption = "Profit";
            if (band.Columns.Exists("SaleAmount")) band.Columns["SaleAmount"].Header.Caption = "Sale Amount";
            if (band.Columns.Exists("SubCategoryName")) band.Columns["SubCategoryName"].Header.Caption = "Sub Category";

            // Colour coding
            if (band.Columns.Exists("ClosingStock"))
                band.Columns["ClosingStock"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
            if (band.Columns.Exists("HoldQty"))
                band.Columns["HoldQty"].CellAppearance.ForeColor = Color.FromArgb(191, 54, 12);
            if (band.Columns.Exists("AvailableStock"))
                band.Columns["AvailableStock"].CellAppearance.ForeColor = Color.FromArgb(1, 87, 155);
            if (band.Columns.Exists("StockValue"))
                band.Columns["StockValue"].CellAppearance.ForeColor = Color.FromArgb(0, 102, 204);
            if (band.Columns.Exists("FutureSalesValue"))
                band.Columns["FutureSalesValue"].CellAppearance.ForeColor = Color.FromArgb(128, 0, 128);

            e.Layout.AutoFitStyle = AutoFitStyle.None;
            e.Layout.ScrollBounds = ScrollBounds.ScrollToFill;
            e.Layout.Scrollbars = Scrollbars.Both;
        }

        private void ConfigureColumn(UltraGridBand band, string key, string header,
            int width, string format, HAlign align, int visPos)
        {
            if (!band.Columns.Exists(key)) return;

            UltraGridColumn col = band.Columns[key];
            col.Header.Caption = header;
            col.Width = width;
            col.Header.VisiblePosition = visPos;
            col.Header.Appearance.BorderColor = Color.FromArgb(197, 217, 241);
            col.CellAppearance.BorderColor = Color.FromArgb(197, 217, 241);
            col.CellAppearance.TextHAlign = align;
            col.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            col.CellAppearance.FontData.SizeInPoints = 8.25F;
            if (!string.IsNullOrWhiteSpace(format))
                col.Format = format;

            col.Hidden = userHiddenColumnKeys.Contains(key);
        }

        private void UltraGridStock_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            try
            {
                if (e.Row.Cells.Exists("ClosingStock"))
                {
                    decimal stock = Convert.ToDecimal(e.Row.Cells["ClosingStock"].Value ?? 0);
                    if (stock < 0)
                    {
                        e.Row.Appearance.BackColor = Color.FromArgb(254, 226, 226);
                        e.Row.Appearance.ForeColor = Color.FromArgb(153, 27, 27);
                    }
                }
            }
            catch { }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (isSearching)
            {
                MessageBox.Show("Search is already in progress. Please wait.", "Busy", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool isAll = string.Equals(Convert.ToString(ultraComboPresetDates.Value ?? ultraComboPresetDates.Text), "ALL", StringComparison.OrdinalIgnoreCase);

            // Build filter from UI
            var filter = new ModelClass.Report.StockReportFilter
            {
                FromDate = isAll ? new DateTime(1753, 1, 1) : (DateTime)ultraDateTimeEditorFrom.Value,
                ToDate = isAll ? DateTime.Now : (DateTime)ultraDateTimeEditorTo.Value,
                CompanyId = !string.IsNullOrEmpty(DataBase.CompanyId) ? int.Parse(DataBase.CompanyId) : 1,
                BranchId = !string.IsNullOrEmpty(DataBase.BranchId) ? (int.TryParse(DataBase.BranchId, out int bid) ? bid : 0) : 1,
                FinYearId = !string.IsNullOrEmpty(DataBase.FinyearId) ? int.Parse(DataBase.FinyearId) : 1,
                BarcodeContains = ultraTextEditorBarcode.Text,
                GroupId = (ultraComboGroup.Value != null) ? (int)ultraComboGroup.Value : (int?)null,
                CategoryId = (ultraComboCategory.Value != null) ? (int)ultraComboCategory.Value : (int?)null,
                LedgerId = (ultraComboLedger.Value != null) ? (int)ultraComboLedger.Value : (int?)null
            };

            // Start async search
            StartAsyncSearch(filter);
        }

        private void StartAsyncSearch(ModelClass.Report.StockReportFilter filter)
        {
            isSearching = true;
            this.Cursor = Cursors.WaitCursor;
            btnSearch.Enabled = false;
            btnSearch.Text = "⏳ Loading...";

            // Clear previous results
            ultraGridStock.DataSource = null;
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            ultraLabelTotalItemsValue.Text = "Loading...";
            ultraLabelTotalValueValue.Text = "...";
            ultraLabelTotalSalesValue.Text = "...";
            ultraLabelTotalPurchaseValue.Text = "...";
            ultraLabelTotalProfitValue.Text = "...";

            Application.DoEvents(); // Allow UI to update

            // Run in background
            searchWorker.RunWorkerAsync(filter);
        }

        private void SearchWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var filter = (ModelClass.Report.StockReportFilter)e.Argument;
            try
            {
                // Execute search in background thread
                e.Result = reportRepo.GetStockReport(filter);
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void SearchWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    MessageBox.Show($"Error loading stock report: {e.Error.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ClearSummaries();
                    return;
                }

                if (e.Result is Exception ex)
                {
                    MessageBox.Show($"Error loading stock report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ClearSummaries();
                    return;
                }

                var data = e.Result as List<ModelClass.Report.StockReportItem>;

                if (data != null && data.Count > 0)
                {
                    // Suspend grid updates for faster binding
                    ultraGridStock.BeginUpdate();
                    try
                    {
                        ultraGridStock.DataSource = data;
                    }
                    finally
                    {
                        ultraGridStock.EndUpdate();
                    }

                    CreateFooterCells();
                    UpdateFooterCellPositions();
                    UpdateFooterValues();

                    // Update summaries
                    ultraLabelTotalItemsValue.Text = data.Count.ToString("N0");
                    ultraLabelTotalValueValue.Text = data.Sum(x => x.StockValue).ToString("C2");
                    ultraLabelTotalSalesValue.Text = data.Sum(x => x.SaleAmount).ToString("N2");
                    ultraLabelTotalPurchaseValue.Text = data.Sum(x => x.Purchase).ToString("N2");
                    ultraLabelTotalProfitValue.Text = data.Sum(x => x.Profit).ToString("C2");
                }
                else
                {
                    ultraGridStock.DataSource = null;
                    CreateFooterCells();
                    UpdateFooterCellPositions();
                    UpdateFooterValues();
                    ClearSummaries();
                    MessageBox.Show("No records found for the selected criteria.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            finally
            {
                // Reset UI
                isSearching = false;
                this.Cursor = Cursors.Default;
                btnSearch.Enabled = true;
                btnSearch.Text = "🔍 Search";
            }
        }

        private void ClearSummaries()
        {
            ultraLabelTotalItemsValue.Text = "0";
            ultraLabelTotalValueValue.Text = "₹ 0.00";
            ultraLabelTotalSalesValue.Text = "0";
            ultraLabelTotalPurchaseValue.Text = "0";
            ultraLabelTotalProfitValue.Text = "₹ 0.00";
        }

        public void RibbonClear() => btnClearFilters_Click(this, EventArgs.Empty);
        public void Clear() => btnClearFilters_Click(this, EventArgs.Empty);

        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            ultraDateTimeEditorFrom.Value = DateTime.Now.AddDays(-30);
            ultraDateTimeEditorTo.Value = DateTime.Now;
            ultraComboGroup.Value = null;
            ultraComboCategory.Value = null;
            ultraComboSubCategory.Value = null;
            ultraComboLedger.Value = null;
            ultraTextEditorBarcode.Text = "";
            ultraComboPresetDates.Value = "ALL";
            ultraGridStock.DataSource = null;
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            ClearSummaries();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV Files|*.csv",
                    Title = "Save Stock Report"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (ultraGridStock.Rows.Count > 0)
                    {
                        ExportToCSV(ultraGridStock, saveFileDialog.FileName);
                        MessageBox.Show("Export successful.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("No data to export.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCSV(UltraGrid grid, string fileName)
        {
            StringBuilder sb = new StringBuilder();

            // Header
            foreach (var col in grid.DisplayLayout.Bands[0].Columns)
            {
                if (!col.Hidden)
                    sb.Append(col.Header.Caption + ",");
            }
            if (sb.Length > 0) sb.Length--;
            sb.AppendLine();

            // Rows
            foreach (var row in grid.Rows)
            {
                foreach (var col in grid.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden)
                    {
                        string value = row.Cells[col].Value?.ToString() ?? "";
                        if (value.Contains(",")) value = "\"" + value + "\"";
                        sb.Append(value + ",");
                    }
                }
                if (sb.Length > 0) sb.Length--;
                sb.AppendLine();
            }

            File.WriteAllText(fileName, sb.ToString());
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            ultraGridStock.PrintPreview();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ultraComboPresetDates_ValueChanged(object sender, EventArgs e)
        {
            if (ultraComboPresetDates.Value == null) return;

            string val = ultraComboPresetDates.Value.ToString();
            DateTime now = DateTime.Now;

            switch (val)
            {
                case "ALL":
                case "All": ultraDateTimeEditorFrom.Value = new DateTime(1753, 1, 1); ultraDateTimeEditorTo.Value = now; break;
                case "Today": ultraDateTimeEditorFrom.Value = now.Date; ultraDateTimeEditorTo.Value = now.Date; break;
                case "Yesterday": ultraDateTimeEditorFrom.Value = now.AddDays(-1).Date; ultraDateTimeEditorTo.Value = now.AddDays(-1).Date; break;
                case "This Week": ultraDateTimeEditorFrom.Value = now.AddDays(-(int)now.DayOfWeek); ultraDateTimeEditorTo.Value = now; break;
                case "Last Week": ultraDateTimeEditorFrom.Value = now.AddDays(-(int)now.DayOfWeek - 7); ultraDateTimeEditorTo.Value = now.AddDays(-(int)now.DayOfWeek - 1); break;
                case "This Month": ultraDateTimeEditorFrom.Value = new DateTime(now.Year, now.Month, 1); ultraDateTimeEditorTo.Value = now; break;
                case "Last Month": ultraDateTimeEditorFrom.Value = new DateTime(now.Year, now.Month, 1).AddMonths(-1); ultraDateTimeEditorTo.Value = new DateTime(now.Year, now.Month, 1).AddDays(-1); break;
            }
        }

        private void ultraComboCategory_ValueChanged(object sender, EventArgs e)
        {
            // TODO: Load SubCategories if Category changes
        }

        private void ultraComboGroup_ValueChanged(object sender, EventArgs e)
        {
            // Optional: Filter Categories by Group if applicable
        }
    }
}
