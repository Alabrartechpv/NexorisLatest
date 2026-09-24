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
        private List<ModelClass.Report.StockReportItem> searchResults;
        private bool isSearching = false;

        // Grid layout persistence for column chooser
        private const string GRID_LAYOUT_FILE = "StockReportAdvancedGridLayout.xml";
        private string GridLayoutPath => Path.Combine(Application.StartupPath, GRID_LAYOUT_FILE);
        private bool gridLayoutLoaded = false;

        // Column chooser and drag state fields
        private Form columnChooserForm = null;
        private ListBox columnChooserListBox = null;
        private Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();
        private Point startPoint;
        private Infragistics.Win.UltraWinGrid.UltraGridColumn columnToMove = null;
        private bool isDraggingColumn = false;
        private System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();

        // --- Grid Footer Summary Panel ---
        private Dictionary<string, Label> summaryLabels = new Dictionary<string, Label>();
        private string currentSummaryType = "None";
        private readonly string[] summaryTypes = new[] { "Sum", "Min", "Max", "Average", "Count", "None" };
        private Dictionary<string, string> columnAggregations = new Dictionary<string, string>();

        public frmStockReportAdvanced()
        {
            InitializeComponent();
            InitializeForm();
            InitializeBackgroundWorker();
            InitializeSummaryFooterPanel();
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

                // Setup column chooser
                SetupColumnChooserMenu();
                LoadGridLayout();

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
                gridFooterPanel.Height = 26;
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
            targetGrid.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            targetGrid.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            targetGrid.DisplayLayout.GroupByBox.Hidden = true;
            targetGrid.DisplayLayout.GroupByBox.BorderStyle = UIElementBorderStyle.None;

            targetGrid.DisplayLayout.Override.HeaderStyle = HeaderStyle.Standard;
            targetGrid.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortMulti;
            targetGrid.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            targetGrid.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
            targetGrid.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
            targetGrid.DisplayLayout.Override.AllowRowFiltering = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.FilterUIType = FilterUIType.FilterRow;
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
            gridFooterPanel.Height = 26;
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
                gridFooterPanel.Height = 26;
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
            AlignSummaryLabels();
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

        // --- BEGIN: Grid Footer Summary Panel Logic ---
        private void InitializeSummaryFooterPanel()
        {
            if (gridFooterPanel == null) return;
            gridFooterPanel.Paint += (s, e) => { AlignSummaryLabels(); };
            gridFooterPanel.Resize += (s, e) => { AlignSummaryLabels(); };
            ultraGridStock.AfterColPosChanged += (s, e) => AlignSummaryLabels();
            ultraGridStock.AfterSortChange += (s, e) => AlignSummaryLabels();
            ultraGridStock.AfterRowFilterChanged += (s, e) => AlignSummaryLabels();
            ultraGridStock.InitializeLayout += (s, e) => AlignSummaryLabels();
            ultraGridStock.SizeChanged += (s, e) => AlignSummaryLabels();

            // Panel-wide right-click context menu (sets same aggregation for all numeric columns)
            var panelMenu = new ContextMenuStrip();
            foreach (var type in summaryTypes)
            {
                var item = new ToolStripMenuItem(type, null, OnPanelSummaryTypeSelected) { Tag = type };
                panelMenu.Items.Add(item);
            }
            gridFooterPanel.ClientArea.ContextMenuStrip = panelMenu;
            gridFooterPanel.ClientArea.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var ctrl = gridFooterPanel.ClientArea.GetChildAtPoint(e.Location);
                    if (ctrl == null || !(ctrl is Label))
                        panelMenu.Show(gridFooterPanel.ClientArea, e.Location);
                }
            };
        }

        private void OnPanelSummaryTypeSelected(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is string type)
            {
                currentSummaryType = type;
                if (ultraGridStock.DisplayLayout.Bands.Count > 0)
                {
                    foreach (var col in ultraGridStock.DisplayLayout.Bands[0].Columns.Cast<Infragistics.Win.UltraWinGrid.UltraGridColumn>())
                    {
                        if (!col.Hidden && IsNumericColumn(col))
                            columnAggregations[col.Key] = type;
                    }
                }
                UpdateSummaryFooter();
            }
        }

        private ContextMenuStrip CreateFooterLabelMenu(string columnKey)
        {
            var menu = new ContextMenuStrip();
            foreach (var type in summaryTypes)
            {
                var menuItem = new ToolStripMenuItem(type) { Tag = type };
                menuItem.Click += (s, e) =>
                {
                    columnAggregations[columnKey] = type;
                    UpdateFooterValues();
                };
                menu.Items.Add(menuItem);
            }
            menu.Opening += (s, e) =>
            {
                foreach (ToolStripMenuItem mi in menu.Items)
                    mi.Checked = columnAggregations.ContainsKey(columnKey) && columnAggregations[columnKey] == (string)mi.Tag;
            };
            return menu;
        }

        private void UpdateSummaryFooter()
        {
            if (gridFooterPanel == null || gridFooterPanel.ClientArea == null
                || ultraGridStock == null || ultraGridStock.DisplayLayout == null
                || ultraGridStock.DisplayLayout.Bands.Count == 0) return;

            gridFooterPanel.ClientArea.SuspendLayout();
            gridFooterPanel.ClientArea.Controls.Clear();
            summaryLabels.Clear();

            var band = ultraGridStock.DisplayLayout.Bands[0];
            foreach (var col in band.Columns.Cast<Infragistics.Win.UltraWinGrid.UltraGridColumn>())
            {
                if (col.Hidden) continue;
                if (!IsNumericColumn(col)) continue;
                if (!columnAggregations.ContainsKey(col.Key) || columnAggregations[col.Key] == "None") continue;

                var lbl = new Label
                {
                    Name = $"lblSummary_{col.Key}",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleRight,
                    ForeColor = Color.FromArgb(17, 52, 102),
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Height = gridFooterPanel.Height - 4,
                    ContextMenuStrip = CreateFooterLabelMenu(col.Key)
                };
                gridFooterPanel.ClientArea.Controls.Add(lbl);
                summaryLabels[col.Key] = lbl;
            }

            UpdateFooterValues();
            AlignSummaryLabels();
            gridFooterPanel.ClientArea.ResumeLayout();
        }

        private void UpdateFooterValues()
        {
            if (ultraGridStock == null || ultraGridStock.DataSource == null) return;

            var data = ultraGridStock.DataSource as List<ModelClass.Report.StockReportItem>;
            if (data == null) return;

            foreach (var kvp in summaryLabels)
            {
                string colKey = kvp.Key;
                Label lbl = kvp.Value;
                string agg = columnAggregations.ContainsKey(colKey) ? columnAggregations[colKey] : "None";

                // Use reflection to get values by property name
                var prop = typeof(ModelClass.Report.StockReportItem).GetProperty(colKey);
                if (prop == null) { lbl.Text = ""; continue; }

                var values = data
                    .Select(r => prop.GetValue(r))
                    .Where(v => v != null)
                    .Select(v => Convert.ToDouble(v))
                    .ToList();

                string text = "";
                switch (agg)
                {
                    case "Sum":     text = values.Count > 0 ? values.Sum().ToString("N2") : "0.00"; break;
                    case "Min":     text = values.Count > 0 ? values.Min().ToString("N2") : "-"; break;
                    case "Max":     text = values.Count > 0 ? values.Max().ToString("N2") : "-"; break;
                    case "Average": text = values.Count > 0 ? values.Average().ToString("N2") : "-"; break;
                    case "Count":   text = values.Count.ToString(); break;
                }
                lbl.Text = text;
            }
        }

        private void AlignSummaryLabels()
        {
            if (gridFooterPanel == null || gridFooterPanel.ClientArea == null
                || ultraGridStock == null || ultraGridStock.DisplayLayout == null
                || ultraGridStock.DisplayLayout.Bands.Count == 0) return;

            var band = ultraGridStock.DisplayLayout.Bands[0];
            foreach (var col in band.Columns.Cast<Infragistics.Win.UltraWinGrid.UltraGridColumn>())
            {
                if (col.Hidden) continue;
                if (!summaryLabels.TryGetValue(col.Key, out var lbl)) continue;

                var headerUI = ultraGridStock.DisplayLayout.Bands[0].Columns[col.Key].Header?.GetUIElement();
                if (headerUI != null)
                {
                    var headerPoint = headerUI.Control.PointToScreen(headerUI.Rect.Location);
                    int colLeft = headerPoint.X - gridFooterPanel.PointToScreen(Point.Empty).X;
                    int colWidth = headerUI.Rect.Width;
                    lbl.Left = colLeft;
                    lbl.Width = colWidth;
                    lbl.Top = 2;
                    lbl.Height = gridFooterPanel.Height - 4;
                }
            }
        }

        private bool IsNumericColumn(Infragistics.Win.UltraWinGrid.UltraGridColumn col)
        {
            var t = col.DataType;
            return t == typeof(int) || t == typeof(float) || t == typeof(double)
                || t == typeof(decimal) || t == typeof(long) || t == typeof(short);
        }

        private void RefreshSummaryFooter()
        {
            UpdateFooterValues();
            AlignSummaryLabels();
        }
        // --- END: Grid Footer Summary Panel Logic ---

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

                    // Apply saved layout AFTER columns are created
                    ApplySavedLayoutIfAvailable();

                    // Customize new columns in the grid
                    if (ultraGridStock.DisplayLayout.Bands.Count > 0)
                    {
                        var band = ultraGridStock.DisplayLayout.Bands[0];
                        if (band.Columns.Exists("HoldQty"))
                        {
                            var col = band.Columns["HoldQty"];
                            col.Header.Caption = "Hold Qty";
                            col.Format = "N2";
                            col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                        }
                        if (band.Columns.Exists("AvailableStock"))
                        {
                            var col = band.Columns["AvailableStock"];
                            col.Header.Caption = "Available Stock";
                            col.Format = "N2";
                            col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                        }
                    }

                    // Update summaries
                    ultraLabelTotalItemsValue.Text = data.Count.ToString("N0");
                    ultraLabelTotalValueValue.Text = data.Sum(x => x.StockValue).ToString("C2");
                    ultraLabelTotalSalesValue.Text = data.Sum(x => x.SaleAmount).ToString("N2");
                    ultraLabelTotalPurchaseValue.Text = data.Sum(x => x.Purchase).ToString("N2");
                    ultraLabelTotalProfitValue.Text = data.Sum(x => x.Profit).ToString("C2");

                    // Refresh grid footer summary panel
                    UpdateSummaryFooter();
                }
                else
                {
                    ultraGridStock.DataSource = null;
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
            sb.Length--;
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
                sb.Length--;
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

        // --- Column Chooser and Drag-to-Hide Logic ---

        private void SetupColumnChooserMenu()
        {
            ContextMenuStrip gridContextMenu = new ContextMenuStrip();
            ToolStripMenuItem columnChooserMenuItem = new ToolStripMenuItem("Field/Column Chooser");
            columnChooserMenuItem.Click += ColumnChooserMenuItem_Click;
            gridContextMenu.Items.Add(columnChooserMenuItem);
            ultraGridStock.ContextMenuStrip = gridContextMenu;
            SetupDirectHeaderDragDrop();
        }

        private void SetupDirectHeaderDragDrop()
        {
            ultraGridStock.AllowDrop = true;
            ultraGridStock.MouseDown += UltraGridStock_MouseDown;
            ultraGridStock.MouseMove += UltraGridStock_MouseMove;
            ultraGridStock.MouseUp += UltraGridStock_MouseUp;
            ultraGridStock.DragOver += UltraGridStock_DragOver;
            ultraGridStock.DragDrop += UltraGridStock_DragDrop;
            CreateColumnChooserForm();
        }

        private void CreateColumnChooserForm()
        {
            columnChooserForm = new Form
            {
                Text = "Customization",
                Size = new Size(220, 280),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(240, 240, 240),
                ShowIcon = false,
                ShowInTaskbar = false
            };
            columnChooserForm.FormClosing += (s, e) => { columnChooserListBox = null; };
            columnChooserForm.Shown += (s, e) => PositionColumnChooserAtBottomRight();

            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                AllowDrop = true,
                DrawMode = DrawMode.OwnerDrawFixed,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(240, 240, 240),
                ItemHeight = 30,
                IntegralHeight = false
            };

            columnChooserListBox.DrawItem += (s, evt) =>
            {
                if (evt.Index < 0) return;
                ColumnItem item = columnChooserListBox.Items[evt.Index] as ColumnItem;
                if (item == null) return;

                Rectangle rect = evt.Bounds;
                rect.Inflate(-3, -3);
                Color bgColor = Color.FromArgb(33, 150, 243);
                using (SolidBrush bgBrush = new SolidBrush(bgColor))
                {
                    using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                    {
                        int radius = 4;
                        int diameter = radius * 2;
                        Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));
                        path.AddArc(arcRect, 180, 90);
                        arcRect.X = rect.Right - diameter;
                        path.AddArc(arcRect, 270, 90);
                        arcRect.Y = rect.Bottom - diameter;
                        path.AddArc(arcRect, 0, 90);
                        arcRect.X = rect.Left;
                        path.AddArc(arcRect, 90, 90);
                        path.CloseFigure();
                        evt.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        evt.Graphics.FillPath(bgBrush, path);
                    }
                }

                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Alignment = StringAlignment.Center
                    };
                    evt.Graphics.DrawString(item.DisplayText, evt.Font, textBrush, rect, sf);
                }

                if ((evt.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    using (Pen focusPen = new Pen(Color.White, 1.5f))
                    {
                        focusPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                        Rectangle focusRect = rect;
                        focusRect.Inflate(-2, -2);
                        evt.Graphics.DrawRectangle(focusPen, focusRect);
                    }
                }
            };

            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            columnChooserListBox.DragOver += ColumnChooserListBox_DragOver;
            columnChooserListBox.DragDrop += ColumnChooserListBox_DragDrop;
            columnChooserForm.Controls.Add(columnChooserListBox);
            PopulateColumnChooserListBox();
        }

        private void PositionColumnChooserAtBottomRight()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed && columnChooserForm.Visible)
            {
                columnChooserForm.Location = new Point(
                    this.Right - columnChooserForm.Width - 20,
                    this.Bottom - columnChooserForm.Height - 20);
                columnChooserForm.TopMost = true;
                columnChooserForm.BringToFront();
            }
        }

        private void ColumnChooserMenuItem_Click(object sender, EventArgs e)
        {
            ShowColumnChooser();
        }

        private void ShowColumnChooser()
        {
            PopulateColumnChooserListBox();
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Show();
                PositionColumnChooserAtBottomRight();
                return;
            }
            CreateColumnChooserForm();
            columnChooserForm.Show(this);
            PositionColumnChooserAtBottomRight();
        }

        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null) return;
            columnChooserListBox.Items.Clear();
            if (ultraGridStock.DisplayLayout.Bands.Count > 0)
            {
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGridStock.DisplayLayout.Bands[0].Columns)
                {
                    if (col.Hidden)
                    {
                        string displayText = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;
                        columnChooserListBox.Items.Add(new ColumnItem(col.Key, displayText));
                    }
                }
            }
        }

        private class ColumnItem
        {
            public string ColumnKey { get; set; }
            public string DisplayText { get; set; }
            public ColumnItem(string key, string text) { ColumnKey = key; DisplayText = text; }
            public override string ToString() => DisplayText;
        }

        private void UltraGridStock_MouseDown(object sender, MouseEventArgs e)
        {
            isDraggingColumn = false;
            columnToMove = null;
            startPoint = new Point(e.X, e.Y);
            if (e.Y < 40 && ultraGridStock.DisplayLayout.Bands.Count > 0)
            {
                int xPos = 0;
                if (ultraGridStock.DisplayLayout.Override.RowSelectors == Infragistics.Win.DefaultableBoolean.True)
                    xPos += ultraGridStock.DisplayLayout.Override.RowSelectorWidth;

                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGridStock.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden)
                    {
                        if (e.X >= xPos && e.X < xPos + col.Width)
                        {
                            columnToMove = col;
                            isDraggingColumn = true;
                            break;
                        }
                        xPos += col.Width;
                    }
                }
            }
        }

        private void UltraGridStock_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingColumn && columnToMove != null && e.Button == MouseButtons.Left)
            {
                int deltaX = Math.Abs(e.X - startPoint.X);
                int deltaY = Math.Abs(e.Y - startPoint.Y);
                if (deltaX > SystemInformation.DragSize.Width || deltaY > SystemInformation.DragSize.Height)
                {
                    bool isDraggingDown = (e.Y > startPoint.Y && deltaY > deltaX);
                    if (isDraggingDown)
                    {
                        ultraGridStock.Cursor = Cursors.No;
                        string columnName = !string.IsNullOrEmpty(columnToMove.Header.Caption) ? columnToMove.Header.Caption : columnToMove.Key;
                        toolTip.SetToolTip(ultraGridStock, $"Drag down to hide '{columnName}' column");
                        if (e.Y - startPoint.Y > 50)
                        {
                            HideColumn(columnToMove);
                            columnToMove = null;
                            isDraggingColumn = false;
                            ultraGridStock.Cursor = Cursors.Default;
                            toolTip.SetToolTip(ultraGridStock, "");
                        }
                    }
                }
            }
        }

        private void UltraGridStock_MouseUp(object sender, MouseEventArgs e)
        {
            ultraGridStock.Cursor = Cursors.Default;
            toolTip.SetToolTip(ultraGridStock, "");
            isDraggingColumn = false;
            columnToMove = null;
        }

        private void HideColumn(Infragistics.Win.UltraWinGrid.UltraGridColumn column)
        {
            if (column != null && !column.Hidden)
            {
                savedColumnWidths[column.Key] = column.Width;
                ultraGridStock.SuspendLayout();
                column.Hidden = true;
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGridStock.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden && savedColumnWidths.ContainsKey(col.Key))
                    {
                        col.Width = savedColumnWidths[col.Key];
                    }
                }
                ultraGridStock.ResumeLayout();

                if (columnChooserListBox != null)
                {
                    bool alreadyExists = false;
                    foreach (object item in columnChooserListBox.Items)
                    {
                        if (item is ColumnItem columnItem && columnItem.ColumnKey == column.Key)
                        {
                            alreadyExists = true;
                            break;
                        }
                    }
                    if (!alreadyExists)
                    {
                        string columnName = !string.IsNullOrEmpty(column.Header.Caption) ? column.Header.Caption : column.Key;
                        columnChooserListBox.Items.Add(new ColumnItem(column.Key, columnName));
                    }
                }
                PopulateColumnChooserListBox();
                SaveGridLayout();
            }
        }

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            int index = columnChooserListBox.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                if (columnChooserListBox.Items[index] is ColumnItem item)
                {
                    columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
                }
            }
        }

        private void ColumnChooserListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn)) ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void ColumnChooserListBox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn)) is Infragistics.Win.UltraWinGrid.UltraGridColumn column && !column.Hidden)
            {
                string name = !string.IsNullOrEmpty(column.Header.Caption) ? column.Header.Caption : column.Key;
                column.Hidden = true;
                columnChooserListBox.Items.Add(new ColumnItem(column.Key, name));
                PopulateColumnChooserListBox();
                SaveGridLayout();
            }
        }

        private void UltraGridStock_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(ColumnItem)) ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void UltraGridStock_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(ColumnItem)) is ColumnItem item)
            {
                if (ultraGridStock.DisplayLayout.Bands.Count > 0 && ultraGridStock.DisplayLayout.Bands[0].Columns.Exists(item.ColumnKey))
                {
                    var column = ultraGridStock.DisplayLayout.Bands[0].Columns[item.ColumnKey];
                    column.Hidden = false;
                    columnChooserListBox.Items.Remove(item);
                    toolTip.Show($"'{item.DisplayText}' restored", ultraGridStock, ultraGridStock.PointToClient(MousePosition), 1500);
                    SaveGridLayout();
                }
            }
        }

        // Layout Persistence Methods

        private void LoadGridLayout()
        {
            try
            {
                if (File.Exists(GridLayoutPath))
                {
                    // Only load when columns exist; otherwise defer until data bind
                    if (ultraGridStock.DisplayLayout.Bands.Count > 0 &&
                        ultraGridStock.DisplayLayout.Bands[0].Columns.Count > 0)
                    {
                        ultraGridStock.DisplayLayout.LoadFromXml(GridLayoutPath);
                        gridLayoutLoaded = true;
                    }
                    else
                    {
                        gridLayoutLoaded = false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading grid layout: {ex.Message}");
            }
        }

        private void ApplySavedLayoutIfAvailable()
        {
            if (!File.Exists(GridLayoutPath)) return;
            if (ultraGridStock.DisplayLayout.Bands.Count == 0) return;
            if (ultraGridStock.DisplayLayout.Bands[0].Columns.Count == 0) return;

            try
            {
                // Delete old grid layout file if it doesn't contain HoldQty, so new columns are visible by default
                string xmlContent = File.ReadAllText(GridLayoutPath);
                if (!xmlContent.Contains("HoldQty"))
                {
                    File.Delete(GridLayoutPath);
                    return;
                }

                ultraGridStock.DisplayLayout.LoadFromXml(GridLayoutPath);
                gridLayoutLoaded = true;
                PopulateColumnChooserListBox();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying grid layout: {ex.Message}");
            }
        }

        private void SaveGridLayout()
        {
            try
            {
                ultraGridStock.DisplayLayout.SaveAsXml(GridLayoutPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving grid layout: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveGridLayout();
            base.OnFormClosing(e);
        }
    }
}
