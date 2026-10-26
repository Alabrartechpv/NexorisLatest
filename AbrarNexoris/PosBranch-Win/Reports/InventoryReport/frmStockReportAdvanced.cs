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

        // ─── Column Header Filtering State ──────────────────────────────────────────
        private readonly Dictionary<string, HashSet<string>> _activeColumnFilters = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly Bitmap _filterIconNormal = CreateFilterIcon(false);
        private static readonly Bitmap _filterIconActive = CreateFilterIcon(true);
        private bool _suppressSorting = false;
        private ColumnFilterDropdownForm _activeDropdownForm = null;
        private DateTime _lastDropdownCloseTime = DateTime.MinValue;
        private string _lastClosedColumnKey = null;

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
                ReportGridLayoutHelper.LoadHiddenColumns("StockReportAdvanced", userHiddenColumnKeys);
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
                ultraGridStock.BeforeSortChange += (s, e) =>
                {
                    if (_suppressSorting)
                    {
                        e.Cancel = true;
                        _suppressSorting = false;
                    }
                };

                // Register footer cell sync and column drag-to-hide handlers matching frmPurchaseReturn / frmStockReport
                ultraGridStock.Resize += (s, e) => { CloseActiveFilterDropdown(); UpdateFooterCellPositions(); };
                ultraGridStock.AfterColPosChanged += (s, e) =>
                {
                    UpdateFooterCellPositions();
                    ReportGridLayoutHelper.SaveLayout("StockReportAdvanced", ultraGridStock, userHiddenColumnKeys);
                };
                ultraGridStock.AfterColRegionScroll += (s, e) => { CloseActiveFilterDropdown(); UpdateFooterCellPositions(); };
                ultraGridStock.AfterRowRegionScroll += (s, e) => { CloseActiveFilterDropdown(); UpdateFooterCellPositions(); };
                ultraGridStock.Paint += (s, e) => UpdateFooterCellPositions();

                SetupHeaderDragToHideAndColumnChooser();
                InitializeGridFooter();

                // Style Buttons & Summary Cards
                StyleButtons();

                FormClosing += (s, e) =>
                {
                    ReportGridLayoutHelper.SaveLayout("StockReportAdvanced", ultraGridStock, userHiddenColumnKeys);
                    CloseActiveFilterDropdown();
                    if (columnChooserForm != null && !columnChooserForm.IsDisposed)
                    {
                        columnChooserForm.Dispose();
                        columnChooserForm = null;
                    }
                };
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
            _activeColumnFilters.Clear();
            CloseActiveFilterDropdown();
            UpdateHeaderFilterIcons();
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
                    // Check if clicked the filter button (the image or rightmost 24 pixels of header)
                    ImageUIElement imgElem = element as ImageUIElement ?? element?.GetAncestor(typeof(ImageUIElement)) as ImageUIElement;
                    if (imgElem != null || e.X >= headerUI.Rect.Right - 24)
                    {
                        _suppressSorting = true;
                        isDraggingHeaderToHide = false;
                        columnBeingDragged = null;
                        ShowColumnFilterPopup(col, headerUI.Rect);
                        return;
                    }
                    columnBeingDragged = col;
                    headerDragStartPoint = new Point(e.X, e.Y);
                }
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDraggingHeaderToHide)
            {
                UIElement elem = ultraGridStock.DisplayLayout.UIElement?.ElementFromPoint(new Point(e.X, e.Y));
                HeaderUIElement hUI = elem as HeaderUIElement ?? elem?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;
                if (hUI != null && hUI.Header?.Column != null)
                {
                    if (e.X >= hUI.Rect.Right - 24)
                    {
                        ultraGridStock.Cursor = Cursors.Hand;
                        string colCaption = !string.IsNullOrEmpty(hUI.Header.Column.Header.Caption) ? hUI.Header.Column.Header.Caption : hUI.Header.Column.Key;
                        headerToolTip.SetToolTip(ultraGridStock, $"Filter by '{colCaption}'");
                        return;
                    }
                    else
                    {
                        ultraGridStock.Cursor = Cursors.Default;
                        headerToolTip.SetToolTip(ultraGridStock, string.Empty);
                    }
                }
            }

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
            ReportGridLayoutHelper.SaveLayout("StockReportAdvanced", ultraGridStock, userHiddenColumnKeys);
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

            ToolStripMenuItem filterItem = new ToolStripMenuItem($"🔍 Filter '{colName}'...", null, (s, e) => {
                var headerUI = col.Header.GetUIElement();
                Rectangle r = headerUI != null ? headerUI.Rect : new Rectangle(location.X, location.Y, 20, 20);
                ShowColumnFilterPopup(col, r);
            });
            filterItem.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            menu.Items.Add(filterItem);

            if (_activeColumnFilters.ContainsKey(col.Key))
            {
                ToolStripMenuItem clearColFilterItem = new ToolStripMenuItem($"❌ Clear Filter on '{colName}'", null, (s, e) => {
                    _activeColumnFilters.Remove(col.Key);
                    ApplyActiveColumnFilters();
                });
                menu.Items.Add(clearColFilterItem);
            }

            if (_activeColumnFilters.Count > 0)
            {
                ToolStripMenuItem clearAllFiltersItem = new ToolStripMenuItem("❌ Clear All Column Filters", null, (s, e) => {
                    _activeColumnFilters.Clear();
                    ApplyActiveColumnFilters();
                });
                menu.Items.Add(clearAllFiltersItem);
            }

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
                if (col.Hidden && !col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && col.ExcludeFromColumnChooser != ExcludeFromColumnChooser.True)
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
                ReportGridLayoutHelper.SaveLayout("StockReportAdvanced", ultraGridStock, userHiddenColumnKeys);
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
                if (!col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && col.ExcludeFromColumnChooser != ExcludeFromColumnChooser.True)
                {
                    col.Hidden = false;
                }
            }
            ReportGridLayoutHelper.SaveLayout("StockReportAdvanced", ultraGridStock, userHiddenColumnKeys);
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
                if (row != null && row.IsDataRow && !row.IsFilteredOut && !row.Hidden)
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
            {
                col.Hidden = true;
                if (col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                {
                    col.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                }
                else
                {
                    col.ExcludeFromColumnChooser = ExcludeFromColumnChooser.False;
                }
            }

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
            if (band.Columns.Exists("OpeningStock")) { band.Columns["OpeningStock"].Header.Caption = "Opening Stock"; band.Columns["OpeningStock"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.False; }
            if (band.Columns.Exists("Purchase")) { band.Columns["Purchase"].Header.Caption = "Purchase"; band.Columns["Purchase"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.False; }
            if (band.Columns.Exists("PurchaseReturn")) { band.Columns["PurchaseReturn"].Header.Caption = "Purchase Return"; band.Columns["PurchaseReturn"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.False; }
            if (band.Columns.Exists("StockAdjustmentIn")) { band.Columns["StockAdjustmentIn"].Header.Caption = "Adj In"; band.Columns["StockAdjustmentIn"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.False; }
            if (band.Columns.Exists("StockAdjustmentOut")) { band.Columns["StockAdjustmentOut"].Header.Caption = "Adj Out"; band.Columns["StockAdjustmentOut"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.False; }
            if (band.Columns.Exists("StockTransferIn")) { band.Columns["StockTransferIn"].Header.Caption = "Transfer In"; band.Columns["StockTransferIn"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.False; }
            if (band.Columns.Exists("StockTransferOut")) { band.Columns["StockTransferOut"].Header.Caption = "Transfer Out"; band.Columns["StockTransferOut"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.False; }
            if (band.Columns.Exists("Sales")) { band.Columns["Sales"].Header.Caption = "Sales"; band.Columns["Sales"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.False; }
            if (band.Columns.Exists("SalesReturn")) { band.Columns["SalesReturn"].Header.Caption = "Sales Return"; band.Columns["SalesReturn"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.False; }
            if (band.Columns.Exists("OrderedStock")) { band.Columns["OrderedStock"].Header.Caption = "Ordered Stock"; band.Columns["OrderedStock"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.False; }
            if (band.Columns.Exists("WholeSalePrice")) { band.Columns["WholeSalePrice"].Header.Caption = "Wholesale Price"; band.Columns["WholeSalePrice"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.False; }
            if (band.Columns.Exists("CreditPrice")) { band.Columns["CreditPrice"].Header.Caption = "Credit Price"; band.Columns["CreditPrice"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.False; }
            if (band.Columns.Exists("Profit")) { band.Columns["Profit"].Header.Caption = "Profit"; band.Columns["Profit"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.False; }
            if (band.Columns.Exists("SaleAmount")) { band.Columns["SaleAmount"].Header.Caption = "Sale Amount"; band.Columns["SaleAmount"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.False; }
            if (band.Columns.Exists("SubCategoryName")) { band.Columns["SubCategoryName"].Header.Caption = "Sub Category"; band.Columns["SubCategoryName"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.False; }

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

            ReportGridLayoutHelper.ApplySavedLayout("StockReportAdvanced", ultraGridStock, userHiddenColumnKeys);
            UpdateHeaderFilterIcons();
            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                PopulateColumnChooserListBox();
            }

            e.Layout.AutoFitStyle = AutoFitStyle.None;
            e.Layout.ScrollBounds = ScrollBounds.ScrollToFill;
            e.Layout.Scrollbars = Scrollbars.Both;
        }

        private void ConfigureColumn(UltraGridBand band, string key, string header,
            int width, string format, HAlign align, int visPos)
        {
            if (!band.Columns.Exists(key)) return;

            UltraGridColumn col = band.Columns[key];
            col.ExcludeFromColumnChooser = ExcludeFromColumnChooser.False;
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
                        _activeColumnFilters.Clear();
                        ultraGridStock.DataSource = data;
                        UpdateHeaderFilterIcons();
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
            _activeColumnFilters.Clear();
            CloseActiveFilterDropdown();
            UpdateHeaderFilterIcons();
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
                if (row.Hidden) continue;
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

        #region Column Header Dropdown Filtering & Popup Window
        private static Bitmap CreateFilterIcon(bool isActive)
        {
            Bitmap bmp = new Bitmap(18, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.Clear(Color.Transparent);

                Rectangle btnRect = new Rectangle(0, 0, 17, 15);

                if (isActive)
                {
                    // Active filter: Golden / amber highlight badge
                    using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(255, 179, 0)))
                    using (Pen borderPen = new Pen(Color.FromArgb(180, 120, 0), 1f))
                    using (GraphicsPath path = RoundedRect(btnRect, 3))
                    {
                        g.FillPath(bgBrush, path);
                        g.DrawPath(borderPen, path);
                    }

                    // Dark navy funnel
                    using (SolidBrush funnelBrush = new SolidBrush(Color.FromArgb(15, 35, 75)))
                    {
                        PointF[] funnel = new PointF[]
                        {
                            new PointF(3.5f, 3.5f),
                            new PointF(10.5f, 3.5f),
                            new PointF(7.5f, 7.5f),
                            new PointF(7.5f, 11.5f),
                            new PointF(6.5f, 10.5f),
                            new PointF(6.5f, 7.5f)
                        };
                        g.FillPolygon(funnelBrush, funnel);
                    }

                    // Small indicator dot
                    using (SolidBrush dotBrush = new SolidBrush(Color.FromArgb(15, 35, 75)))
                    {
                        g.FillEllipse(dotBrush, 11.5f, 8.5f, 3.5f, 3.5f);
                    }
                }
                else
                {
                    // Inactive filter: Subtle light button outline
                    using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(50, 255, 255, 255)))
                    using (Pen borderPen = new Pen(Color.FromArgb(120, 255, 255, 255), 1f))
                    using (GraphicsPath path = RoundedRect(btnRect, 3))
                    {
                        g.FillPath(bgBrush, path);
                        g.DrawPath(borderPen, path);
                    }

                    // Crisp white funnel
                    using (SolidBrush funnelBrush = new SolidBrush(Color.White))
                    {
                        PointF[] funnel = new PointF[]
                        {
                            new PointF(3.0f, 3.5f),
                            new PointF(10.0f, 3.5f),
                            new PointF(7.0f, 7.5f),
                            new PointF(7.0f, 11.5f),
                            new PointF(6.0f, 10.5f),
                            new PointF(6.0f, 7.5f)
                        };
                        g.FillPolygon(funnelBrush, funnel);

                        // Down arrow
                        PointF[] arrow = new PointF[]
                        {
                            new PointF(11.5f, 6.0f),
                            new PointF(15.5f, 6.0f),
                            new PointF(13.5f, 8.5f)
                        };
                        g.FillPolygon(funnelBrush, arrow);
                    }
                }
            }
            return bmp;
        }

        private void UpdateHeaderFilterIcons()
        {
            if (ultraGridStock.DisplayLayout == null || ultraGridStock.DisplayLayout.Bands.Count == 0) return;
            UltraGridBand band = ultraGridStock.DisplayLayout.Bands[0];

            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.Hidden || col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase) || col.Key == "SlNo") continue;

                bool isFiltered = _activeColumnFilters.ContainsKey(col.Key);
                col.Header.Appearance.Image = isFiltered ? _filterIconActive : _filterIconNormal;
                col.Header.Appearance.ImageHAlign = HAlign.Right;
                col.Header.Appearance.ImageVAlign = VAlign.Middle;
            }
        }

        private void CloseActiveFilterDropdown()
        {
            if (_activeDropdownForm != null && !_activeDropdownForm.IsDisposed)
            {
                _activeDropdownForm.Close();
                _activeDropdownForm = null;
            }
        }

        private void ShowColumnFilterPopup(UltraGridColumn col, Rectangle headerRect)
        {
            if (col == null || ultraGridStock.Rows == null || ultraGridStock.Rows.Count == 0)
            {
                MessageBox.Show("No data loaded in the grid to filter.", "Filter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_activeDropdownForm != null && !_activeDropdownForm.IsDisposed)
            {
                _activeDropdownForm.Close();
                _activeDropdownForm = null;
            }

            if (string.Equals(_lastClosedColumnKey, col.Key, StringComparison.OrdinalIgnoreCase) && 
                (DateTime.Now - _lastDropdownCloseTime).TotalMilliseconds < 250)
            {
                _lastClosedColumnKey = null;
                return;
            }

            // Collect distinct values for this column
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> allValues = new List<string>();

            foreach (UltraGridRow row in ultraGridStock.Rows)
            {
                if (!row.IsDataRow) continue;
                string text = row.Cells.Exists(col.Key) ? (row.Cells[col.Key].Text?.Trim() ?? "") : "";
                if (string.IsNullOrEmpty(text)) text = "(Blanks)";
                if (seen.Add(text))
                {
                    allValues.Add(text);
                }
            }

            // Sort values nicely
            allValues.Sort((a, b) =>
            {
                if (a == "(Blanks)") return 1;
                if (b == "(Blanks)") return -1;
                if (decimal.TryParse(a, out decimal da) && decimal.TryParse(b, out decimal db))
                    return da.CompareTo(db);
                return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
            });

            // Currently active selection
            HashSet<string> currentSelected = null;
            if (_activeColumnFilters.ContainsKey(col.Key))
            {
                currentSelected = _activeColumnFilters[col.Key];
            }

            string caption = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;

            int dropWidth = Math.Max(230, Math.Min(Math.Max(headerRect.Width, 230), 300));
            int itemsHeight = Math.Min(Math.Max(allValues.Count * 22, 44), 190);
            int dropHeight = 90 + itemsHeight;
            var popup = new ColumnFilterDropdownForm(col.Key, caption, allValues, currentSelected, dropWidth, dropHeight, (newSelected) =>
            {
                if (newSelected == null || newSelected.Count == allValues.Count)
                {
                    _activeColumnFilters.Remove(col.Key);
                }
                else
                {
                    _activeColumnFilters[col.Key] = new HashSet<string>(newSelected, StringComparer.OrdinalIgnoreCase);
                }

                ApplyActiveColumnFilters();
            });

            // Position popup right below header
            Point screenPt = ultraGridStock.PointToScreen(new Point(headerRect.Left, headerRect.Bottom));
            Rectangle screenArea = Screen.FromControl(ultraGridStock).WorkingArea;

            if (screenPt.X + dropWidth > screenArea.Right)
                screenPt.X = screenArea.Right - dropWidth - 6;
            if (screenPt.X < screenArea.Left)
                screenPt.X = screenArea.Left + 6;

            if (screenPt.Y + dropHeight > screenArea.Bottom)
                screenPt.Y = screenPt.Y - headerRect.Height - dropHeight;

            popup.Location = screenPt;
            _activeDropdownForm = popup;
            popup.FormClosed += (s, e) =>
            {
                _lastClosedColumnKey = col.Key;
                _lastDropdownCloseTime = DateTime.Now;
                if (_activeDropdownForm == popup)
                    _activeDropdownForm = null;
            };
            popup.Show(this);
        }

        private void ApplyActiveColumnFilters()
        {
            ultraGridStock.BeginUpdate();
            try
            {
                bool hasFilters = _activeColumnFilters.Count > 0;

                foreach (UltraGridRow row in ultraGridStock.Rows)
                {
                    if (!row.IsDataRow) continue;

                    if (!hasFilters)
                    {
                        row.Hidden = false;
                        continue;
                    }

                    bool visible = true;
                    foreach (var kvp in _activeColumnFilters)
                    {
                        string colKey = kvp.Key;
                        HashSet<string> allowed = kvp.Value;

                        string cellText = row.Cells.Exists(colKey) ? (row.Cells[colKey].Text?.Trim() ?? "") : "";
                        if (string.IsNullOrEmpty(cellText)) cellText = "(Blanks)";

                        if (!allowed.Contains(cellText))
                        {
                            visible = false;
                            break;
                        }
                    }

                    row.Hidden = !visible;
                }
            }
            finally
            {
                ultraGridStock.EndUpdate();
            }

            UpdateHeaderFilterIcons();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private sealed class ColumnFilterDropdownForm : Form
        {
            private readonly string _columnKey;
            private readonly string _columnTitle;
            private readonly List<string> _allValues;
            private readonly HashSet<string> _selectedValues;
            private readonly Action<HashSet<string>> _onApply;

            private Panel pnlHeader;
            private Label lblTitle;
            private Label btnCloseX;
            private Panel pnlSearch;
            private TextBox txtSearch;
            private Panel pnlSelectAll;
            private CheckBox chkSelectAll;
            private CheckedListBox clbItems;
            private Panel pnlBottom;
            private Button btnOK;
            private Button btnCancel;
            private Button btnClear;

            private List<string> _visibleValues = new List<string>();
            private bool _isUpdatingCheckState = false;

            protected override CreateParams CreateParams
            {
                get
                {
                    const int CS_DROPSHADOW = 0x00020000;
                    CreateParams cp = base.CreateParams;
                    cp.ClassStyle |= CS_DROPSHADOW;
                    return cp;
                }
            }

            public ColumnFilterDropdownForm(string columnKey, string columnTitle, List<string> allValues, HashSet<string> currentSelected, int width, int height, Action<HashSet<string>> onApply)
            {
                _columnKey = columnKey;
                _columnTitle = columnTitle;
                _allValues = allValues ?? new List<string>();
                _selectedValues = currentSelected != null 
                    ? new HashSet<string>(currentSelected, StringComparer.OrdinalIgnoreCase)
                    : new HashSet<string>(_allValues, StringComparer.OrdinalIgnoreCase);
                _onApply = onApply;

                InitializePopupUi(width, height);
                PopulateItems(string.Empty);
            }

            private void InitializePopupUi(int width, int height)
            {
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.Manual;
                ShowInTaskbar = false;
                TopMost = true;
                Size = new Size(width, height);
                BackColor = Color.White;
                KeyPreview = true;
                DoubleBuffered = true;

                // ── Search Panel ──
                pnlSearch = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 28,
                    BackColor = Color.FromArgb(240, 244, 248),
                    Padding = new Padding(4, 3, 4, 3)
                };

                txtSearch = new TextBox
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9F),
                    BorderStyle = BorderStyle.FixedSingle
                };
                txtSearch.TextChanged += (s, e) => PopulateItems(txtSearch.Text);

                Label lblSearchIcon = new Label
                {
                    Text = "🔍",
                    Dock = DockStyle.Left,
                    Width = 20,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.FromArgb(120, 140, 160),
                    Font = new Font("Segoe UI", 8F)
                };
                pnlSearch.Controls.Add(txtSearch);
                pnlSearch.Controls.Add(lblSearchIcon);

                txtSearch.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Escape) { Close(); e.Handled = true; }
                    else if (e.KeyCode == Keys.Enter) { ApplyAndClose(); e.Handled = true; }
                    else if (e.KeyCode == Keys.Down && clbItems.Items.Count > 0) { clbItems.Focus(); e.Handled = true; }
                };

                // ── Select All Panel ──
                pnlSelectAll = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 28,
                    BackColor = Color.FromArgb(248, 250, 252),
                    Padding = new Padding(6, 2, 6, 2)
                };

                chkSelectAll = new CheckBox
                {
                    Text = "(Select All)",
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 35, 75),
                    ThreeState = true
                };
                chkSelectAll.Click += ChkSelectAll_Click;
                pnlSelectAll.Controls.Add(chkSelectAll);

                // ── Bottom Buttons Panel ──
                pnlBottom = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 34,
                    BackColor = Color.FromArgb(246, 248, 252),
                    Padding = new Padding(4, 4, 4, 4)
                };

                btnOK = new Button
                {
                    Text = "OK",
                    Size = new Size(54, 24),
                    Location = new Point(5, 5),
                    BackColor = Color.FromArgb(67, 118, 184),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnOK.FlatAppearance.BorderSize = 0;
                btnOK.MouseEnter += (s, e) => { btnOK.BackColor = Color.FromArgb(50, 95, 155); };
                btnOK.MouseLeave += (s, e) => { btnOK.BackColor = Color.FromArgb(67, 118, 184); };
                btnOK.Click += (s, e) => ApplyAndClose();

                btnCancel = new Button
                {
                    Text = "Cancel",
                    Size = new Size(54, 24),
                    Location = new Point(64, 5),
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(30, 41, 59),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F),
                    Cursor = Cursors.Hand
                };
                btnCancel.FlatAppearance.BorderColor = Color.FromArgb(180, 200, 220);
                btnCancel.MouseEnter += (s, e) => { btnCancel.BackColor = Color.FromArgb(241, 245, 249); };
                btnCancel.MouseLeave += (s, e) => { btnCancel.BackColor = Color.White; };
                btnCancel.Click += (s, e) => Close();

                btnClear = new Button
                {
                    Text = "Clear",
                    Size = new Size(54, 24),
                    Location = new Point(124, 5),
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(180, 40, 40),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 8.5F),
                    Cursor = Cursors.Hand
                };
                btnClear.FlatAppearance.BorderColor = Color.FromArgb(210, 160, 160);
                btnClear.MouseEnter += (s, e) => { btnClear.BackColor = Color.FromArgb(254, 242, 242); };
                btnClear.MouseLeave += (s, e) => { btnClear.BackColor = Color.White; };
                btnClear.Click += (s, e) => ClearAndClose();

                pnlBottom.Controls.Add(btnOK);
                pnlBottom.Controls.Add(btnCancel);
                pnlBottom.Controls.Add(btnClear);

                // ── CheckedListBox ──
                clbItems = new CheckedListBox
                {
                    Dock = DockStyle.Fill,
                    CheckOnClick = true,
                    BorderStyle = BorderStyle.None,
                    Font = new Font("Segoe UI", 9F),
                    BackColor = Color.White,
                    IntegralHeight = false
                };
                clbItems.ItemCheck += ClbItems_ItemCheck;
                clbItems.DoubleClick += (s, e) =>
                {
                    int index = clbItems.SelectedIndex;
                    if (index >= 0 && index < _visibleValues.Count)
                    {
                        string val = _visibleValues[index];
                        _selectedValues.Clear();
                        _selectedValues.Add(val);
                        ApplyAndClose();
                    }
                };

                Controls.Add(clbItems);
                Controls.Add(pnlSelectAll);
                Controls.Add(pnlSearch);
                Controls.Add(pnlBottom);

                Deactivate += (s, e) => { if (!IsDisposed) Close(); };
                KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Escape) Close();
                    else if (e.KeyCode == Keys.Enter && !txtSearch.Focused) ApplyAndClose();
                };
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                using (Pen p = new Pen(Color.FromArgb(160, 178, 200), 1))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
                }
            }

            private void PopulateItems(string filterText)
            {
                _isUpdatingCheckState = true;
                clbItems.BeginUpdate();
                try
                {
                    clbItems.Items.Clear();
                    _visibleValues.Clear();

                    string query = (filterText ?? "").Trim();
                    bool hasQuery = !string.IsNullOrEmpty(query);

                    foreach (string val in _allValues)
                    {
                        if (!hasQuery || val.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            _visibleValues.Add(val);
                            bool isChecked = _selectedValues.Contains(val);
                            clbItems.Items.Add(val, isChecked);
                        }
                    }
                }
                finally
                {
                    clbItems.EndUpdate();
                    _isUpdatingCheckState = false;
                }

                UpdateSelectAllState();
            }

            private void ClbItems_ItemCheck(object sender, ItemCheckEventArgs e)
            {
                if (_isUpdatingCheckState) return;

                if (e.Index >= 0 && e.Index < _visibleValues.Count)
                {
                    string itemValue = _visibleValues[e.Index];
                    if (e.NewValue == CheckState.Checked)
                    {
                        _selectedValues.Add(itemValue);
                    }
                    else
                    {
                        _selectedValues.Remove(itemValue);
                    }
                }

                BeginInvoke(new Action(UpdateSelectAllState));
            }

            private void UpdateSelectAllState()
            {
                if (_visibleValues.Count == 0)
                {
                    chkSelectAll.CheckState = CheckState.Unchecked;
                    return;
                }

                int checkedCount = 0;
                foreach (string val in _visibleValues)
                {
                    if (_selectedValues.Contains(val))
                        checkedCount++;
                }

                _isUpdatingCheckState = true;
                try
                {
                    if (checkedCount == _visibleValues.Count)
                    {
                        chkSelectAll.CheckState = CheckState.Checked;
                    }
                    else if (checkedCount == 0)
                    {
                        chkSelectAll.CheckState = CheckState.Unchecked;
                    }
                    else
                    {
                        chkSelectAll.CheckState = CheckState.Indeterminate;
                    }
                }
                finally
                {
                    _isUpdatingCheckState = false;
                }
            }

            private void ChkSelectAll_Click(object sender, EventArgs e)
            {
                bool targetState = (chkSelectAll.CheckState == CheckState.Checked);

                _isUpdatingCheckState = true;
                clbItems.BeginUpdate();
                try
                {
                    for (int i = 0; i < _visibleValues.Count; i++)
                    {
                        string val = _visibleValues[i];
                        if (targetState)
                        {
                            _selectedValues.Add(val);
                            clbItems.SetItemChecked(i, true);
                        }
                        else
                        {
                            _selectedValues.Remove(val);
                            clbItems.SetItemChecked(i, false);
                        }
                    }
                }
                finally
                {
                    clbItems.EndUpdate();
                    _isUpdatingCheckState = false;
                }
            }

            private void ApplyAndClose()
            {
                _onApply?.Invoke(_selectedValues);
                Close();
            }

            private void ClearAndClose()
            {
                _onApply?.Invoke(null);
                Close();
            }
        }
        #endregion

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
