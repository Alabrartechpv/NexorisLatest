using Infragistics.Win;
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
                // Remove ALL SetChildIndex calls - they broke the layout

                reportRepo = new StockReportAdvanceRepo();
                dropdownRepo = new Dropdowns();

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
                ConfigureGrid();

                // Setup column chooser
                SetupColumnChooserMenu();
                LoadGridLayout();

                // Style Buttons
                StyleButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializePresetDates()
        {
            ultraComboPresetDates.Items.Clear();
            ultraComboPresetDates.Items.Add("Today", "Today");
            ultraComboPresetDates.Items.Add("Yesterday", "Yesterday");
            ultraComboPresetDates.Items.Add("This Week", "This Week");
            ultraComboPresetDates.Items.Add("Last Week", "Last Week");
            ultraComboPresetDates.Items.Add("This Month", "This Month");
            ultraComboPresetDates.Items.Add("Last Month", "Last Month");
        }

        private void LoadGroups()
        {
            try
            {
                // Use Dropdowns repository
                var groups = dropdownRepo.getGroupDDl();
                if (groups != null && groups.List != null)
                {
                    // Add "All" option manually or handle null in Value
                    // UltraCombo doesn't easily support inserting into IEnumerable, so we might need a List
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
            // Placeholder: Need to know SP for SubCategory
            // Can be implemented once SubCategory SP is identified
        }

        private void LoadLedgers()
        {
            try
            {
                // Using Vendor DDL for Ledger filter (assuming Supplier/Vendor context for stock)
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

        private void ConfigureGrid()
        {
            // Sorting & Filtering
            ultraGridStock.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortMulti;
            ultraGridStock.DisplayLayout.Override.AllowRowFiltering = DefaultableBoolean.True;
            ultraGridStock.DisplayLayout.Override.FilterUIType = FilterUIType.FilterRow;
            ultraGridStock.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;

            // Row selectors - Modern look
            ultraGridStock.DisplayLayout.Override.RowSelectorAppearance.BackColor = Color.FromArgb(69, 90, 100);
            ultraGridStock.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
            ultraGridStock.DisplayLayout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            ultraGridStock.DisplayLayout.Override.RowSelectorAppearance.TextHAlign = Infragistics.Win.HAlign.Center;

            // Modern header styling - Deep Blue-Grey gradient
            ultraGridStock.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(55, 71, 79);
            ultraGridStock.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(69, 90, 100);
            ultraGridStock.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            ultraGridStock.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            ultraGridStock.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            ultraGridStock.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;

            // Row height
            ultraGridStock.DisplayLayout.Override.MinRowHeight = 25;
            ultraGridStock.DisplayLayout.Override.DefaultRowHeight = 25;

            // Alternating row colors - Soft gradient
            ultraGridStock.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            ultraGridStock.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(250, 250, 252);

            // Selection colors - Material Design Blue
            ultraGridStock.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(66, 165, 245);
            ultraGridStock.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
            ultraGridStock.DisplayLayout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.True;

            // Active row - Light blue hover
            ultraGridStock.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(227, 242, 253);
            ultraGridStock.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.FromArgb(33, 33, 33);
            ultraGridStock.DisplayLayout.Override.ActiveRowAppearance.BorderColor = Color.FromArgb(66, 165, 245);

            // Border styles
            ultraGridStock.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            ultraGridStock.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            ultraGridStock.DisplayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
            ultraGridStock.DisplayLayout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
            ultraGridStock.DisplayLayout.GroupByBox.Hidden = true;

            // Column interactions
            ultraGridStock.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
            ultraGridStock.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
            ultraGridStock.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
        }

        private void StyleButtons()
        {
            // Style search button - Primary Blue with gradient
            btnSearch.UseAppStyling = false;
            btnSearch.UseOsThemes = DefaultableBoolean.False;
            btnSearch.Appearance.BackColor = Color.FromArgb(25, 118, 210);
            btnSearch.Appearance.BackColor2 = Color.FromArgb(33, 150, 243);
            btnSearch.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            btnSearch.Appearance.ForeColor = Color.White;
            btnSearch.Appearance.FontData.Bold = DefaultableBoolean.True;
            btnSearch.Appearance.FontData.SizeInPoints = 10;
            btnSearch.Appearance.BorderColor = Color.FromArgb(21, 101, 192);
            btnSearch.HotTrackAppearance.BackColor = Color.FromArgb(66, 165, 245);
            btnSearch.HotTrackAppearance.ForeColor = Color.White;

            // Style export button - Teal with gradient
            btnExport.UseAppStyling = false;
            btnExport.UseOsThemes = DefaultableBoolean.False;
            btnExport.Appearance.BackColor = Color.FromArgb(0, 121, 107);
            btnExport.Appearance.BackColor2 = Color.FromArgb(0, 150, 136);
            btnExport.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            btnExport.Appearance.ForeColor = Color.White;
            btnExport.Appearance.FontData.Bold = DefaultableBoolean.True;
            btnExport.Appearance.FontData.SizeInPoints = 10;
            btnExport.Appearance.BorderColor = Color.FromArgb(0, 105, 92);
            btnExport.HotTrackAppearance.BackColor = Color.FromArgb(38, 166, 154);
            btnExport.HotTrackAppearance.ForeColor = Color.White;

            // Style clear filters button - Orange with gradient
            btnClearFilters.UseAppStyling = false;
            btnClearFilters.UseOsThemes = DefaultableBoolean.False;
            btnClearFilters.Appearance.BackColor = Color.FromArgb(245, 124, 0);
            btnClearFilters.Appearance.BackColor2 = Color.FromArgb(255, 152, 0);
            btnClearFilters.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            btnClearFilters.Appearance.ForeColor = Color.White;
            btnClearFilters.Appearance.FontData.Bold = DefaultableBoolean.True;
            btnClearFilters.Appearance.FontData.SizeInPoints = 10;
            btnClearFilters.Appearance.BorderColor = Color.FromArgb(230, 81, 0);
            btnClearFilters.HotTrackAppearance.BackColor = Color.FromArgb(255, 167, 38);
            btnClearFilters.HotTrackAppearance.ForeColor = Color.White;

            // Style print button - Deep Purple with gradient
            btnPrint.UseAppStyling = false;
            btnPrint.UseOsThemes = DefaultableBoolean.False;
            btnPrint.Appearance.BackColor = Color.FromArgb(81, 45, 168);
            btnPrint.Appearance.BackColor2 = Color.FromArgb(103, 58, 183);
            btnPrint.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            btnPrint.Appearance.ForeColor = Color.White;
            btnPrint.Appearance.FontData.Bold = DefaultableBoolean.True;
            btnPrint.Appearance.FontData.SizeInPoints = 10;
            btnPrint.Appearance.BorderColor = Color.FromArgb(69, 39, 160);
            btnPrint.HotTrackAppearance.BackColor = Color.FromArgb(126, 87, 194);
            btnPrint.HotTrackAppearance.ForeColor = Color.White;

            // Style close button - Red with gradient
            btnClose.UseAppStyling = false;
            btnClose.UseOsThemes = DefaultableBoolean.False;
            btnClose.Appearance.BackColor = Color.FromArgb(211, 47, 47);
            btnClose.Appearance.BackColor2 = Color.FromArgb(244, 67, 54);
            btnClose.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            btnClose.Appearance.ForeColor = Color.White;
            btnClose.Appearance.FontData.Bold = DefaultableBoolean.True;
            btnClose.Appearance.FontData.SizeInPoints = 10;
            btnClose.Appearance.BorderColor = Color.FromArgb(183, 28, 28);
            btnClose.HotTrackAppearance.BackColor = Color.FromArgb(229, 115, 115);
            btnClose.HotTrackAppearance.ForeColor = Color.White;

            // Style summary labels
            StyleSummaryLabels();
        }

        /// <summary>
        /// Style summary labels with colors and bold text
        /// </summary>
        private void StyleSummaryLabels()
        {
            // Caption labels - bold with accent colors
            ultraLabelTotalItemsCaption.Appearance.ForeColor = Color.FromArgb(25, 118, 210); // Blue
            ultraLabelTotalItemsCaption.Appearance.FontData.Bold = DefaultableBoolean.True;
            ultraLabelTotalItemsCaption.Appearance.FontData.SizeInPoints = 10;

            ultraLabelTotalValueCaption.Appearance.ForeColor = Color.FromArgb(123, 31, 162); // Purple
            ultraLabelTotalValueCaption.Appearance.FontData.Bold = DefaultableBoolean.True;
            ultraLabelTotalValueCaption.Appearance.FontData.SizeInPoints = 10;

            ultraLabelTotalSalesCaption.Appearance.ForeColor = Color.FromArgb(211, 84, 0); // Orange
            ultraLabelTotalSalesCaption.Appearance.FontData.Bold = DefaultableBoolean.True;
            ultraLabelTotalSalesCaption.Appearance.FontData.SizeInPoints = 10;

            ultraLabelTotalPurchaseCaption.Appearance.ForeColor = Color.FromArgb(56, 142, 60); // Green
            ultraLabelTotalPurchaseCaption.Appearance.FontData.Bold = DefaultableBoolean.True;
            ultraLabelTotalPurchaseCaption.Appearance.FontData.SizeInPoints = 10;

            ultraLabelTotalProfitCaption.Appearance.ForeColor = Color.FromArgb(22, 160, 133); // Teal
            ultraLabelTotalProfitCaption.Appearance.FontData.Bold = DefaultableBoolean.True;
            ultraLabelTotalProfitCaption.Appearance.FontData.SizeInPoints = 10;

            // Value labels - larger, bold
            ultraLabelTotalItemsValue.Appearance.ForeColor = Color.FromArgb(13, 71, 161);
            ultraLabelTotalItemsValue.Appearance.FontData.Bold = DefaultableBoolean.True;
            ultraLabelTotalItemsValue.Appearance.FontData.SizeInPoints = 14;

            ultraLabelTotalValueValue.Appearance.ForeColor = Color.FromArgb(74, 20, 140);
            ultraLabelTotalValueValue.Appearance.FontData.Bold = DefaultableBoolean.True;
            ultraLabelTotalValueValue.Appearance.FontData.SizeInPoints = 16;

            ultraLabelTotalSalesValue.Appearance.ForeColor = Color.FromArgb(191, 54, 12);
            ultraLabelTotalSalesValue.Appearance.FontData.Bold = DefaultableBoolean.True;
            ultraLabelTotalSalesValue.Appearance.FontData.SizeInPoints = 14;

            ultraLabelTotalPurchaseValue.Appearance.ForeColor = Color.FromArgb(27, 94, 32);
            ultraLabelTotalPurchaseValue.Appearance.FontData.Bold = DefaultableBoolean.True;
            ultraLabelTotalPurchaseValue.Appearance.FontData.SizeInPoints = 14;

            ultraLabelTotalProfitValue.Appearance.ForeColor = Color.FromArgb(0, 121, 107);
            ultraLabelTotalProfitValue.Appearance.FontData.Bold = DefaultableBoolean.True;
            ultraLabelTotalProfitValue.Appearance.FontData.SizeInPoints = 16;
        }

        private void frmStockReportAdvanced_Load(object sender, EventArgs e)
        {
            LayoutPanels();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutPanels();
        }

        private void LayoutPanels()
        {
            int topUsed = ultraPanelControls.Height + ultraPanelFilters.Height;
            int summaryHeight = 85;
            int clientH = this.ClientSize.Height;

            // Summary panel: pinned to bottom
            ultraPanelSummary.SetBounds(0, clientH - summaryHeight, this.ClientSize.Width, summaryHeight);

            // Grid panel: fills remaining middle space
            int gridTop = topUsed;
            int gridHeight = clientH - topUsed - summaryHeight;
            if (gridHeight < 50) gridHeight = 50;
            ultraPanelGrid.SetBounds(0, gridTop, this.ClientSize.Width, gridHeight);

            ultraPanelSummary.Visible = true;
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
                    ForeColor = Color.White,
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

            // Build filter from UI
            var filter = new ModelClass.Report.StockReportFilter
            {
                FromDate = (DateTime)ultraDateTimeEditorFrom.Value,
                ToDate = (DateTime)ultraDateTimeEditorTo.Value,
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

        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            ultraDateTimeEditorFrom.Value = DateTime.Now.AddDays(-30);
            ultraDateTimeEditorTo.Value = DateTime.Now;
            ultraComboGroup.Value = null;
            ultraComboCategory.Value = null;
            ultraComboSubCategory.Value = null;
            ultraComboLedger.Value = null;
            ultraTextEditorBarcode.Text = "";
            ultraComboPresetDates.Value = null;
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
