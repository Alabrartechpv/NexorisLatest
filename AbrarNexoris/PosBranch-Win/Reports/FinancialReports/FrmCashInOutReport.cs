using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using PosBranch_Win.Accounts;
using PosBranch_Win.Transaction;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class FrmCashInOutReport : Form
    {
        #region Colour Palette
        private static readonly Color FormBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color FilterPanelBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color ActionBarBackColor = Color.FromArgb(206, 223, 238);
        private static readonly Color BorderBlue = Color.FromArgb(118, 154, 198);
        private static readonly Color ControlTextColor = Color.FromArgb(18, 49, 102);

        private static readonly Color ButtonBlueTop = Color.FromArgb(241, 248, 254);
        private static readonly Color ButtonBlueBottom = Color.FromArgb(196, 218, 243);
        private static readonly Color ButtonTextBlue = Color.FromArgb(18, 49, 102);
        private static readonly Color ButtonLightOutline = Color.FromArgb(118, 154, 198);

        private static readonly Color GridHeaderBlue = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue = Color.FromArgb(173, 216, 255);
        private static readonly Color GridRowLine = Color.FromArgb(197, 217, 241);
        private static readonly Color GridAltRow = Color.FromArgb(245, 250, 255);
        private static readonly Color CashInGreen = Color.FromArgb(46, 125, 50);
        private static readonly Color CashOutRed = Color.FromArgb(198, 40, 40);
        #endregion

        #region Fields & Member Variables
        private readonly CashInOutRepository _repository;
        private CashInOutReportResponse _currentData;
        private List<CashInOutTransaction> _filteredTransactions;
        private bool _isLoading;

        // Footer Dynamic Aggregation Panel under Grid Columns
        private readonly Dictionary<string, Label> _footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> _summaryDefaultNumericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "InflowAmount",
            "OutflowAmount"
        };

        // Drag & Drop Column Chooser
        private ListBox _columnChooserListBox;
        private Form _columnChooserForm;
        private TextBox _txtColumnSearch;
        private bool _isDraggingHeaderToHide;
        private UltraGridColumn _columnBeingDragged;
        private Point _headerDragStartPoint;
        private readonly System.Windows.Forms.ToolTip _headerToolTip = new System.Windows.Forms.ToolTip();
        private static readonly Cursor _blackXCursor = CreateBlackXCursor();
        #endregion

        #region Helper Classes
        private sealed class ColumnChooserItem
        {
            public string ColumnKey { get; }
            public string DisplayText { get; }

            public ColumnChooserItem(string key, string text)
            {
                ColumnKey = key;
                DisplayText = text;
            }

            public override string ToString() => DisplayText;
        }
        #endregion

        #region Constructor & Lifecycle
        public FrmCashInOutReport() : this(DateTime.Today, DateTime.Today)
        {
        }

        public FrmCashInOutReport(DateTime fromDate, DateTime toDate)
        {
            _repository = new CashInOutRepository();
            _currentData = new CashInOutReportResponse();
            _filteredTransactions = new List<CashInOutTransaction>();

            InitializeComponent();

            Load += FrmCashInOutReport_Load;
            KeyDown += FrmCashInOutReport_KeyDown;
            FormClosed += FrmCashInOutReport_FormClosed;
            KeyPreview = true;

            // Action Buttons
            btnViewGrid.Click += (s, e) => LoadData();
            btnPreviewGrid.Click += (s, e) => ShowGridPreview("Cash In / Cash Out Flow - Grid Preview");
            btnPreviewReport.Click += (s, e) => ShowReportPreview();
            btnExportExcel.Click += (s, e) => ExportToExcel();
            btnClearFilters.Click += (s, e) => ResetFilters();
            btnColumnChooser.Click += (s, e) => ShowColumnChooserForm();
            btnHideSelection.Click += (s, e) => ToggleSelection();

            // Filters
            cmbDateQuickSelect.ValueChanged += CmbDateQuickSelect_ValueChanged;
            cmbCategory.ValueChanged += (s, e) => ApplyFilter();
            txtSearch.TextChanged += (s, e) => ApplyFilter();

            // Grid Events
            gridReport.InitializeLayout += GridReport_InitializeLayout;
            gridReport.InitializeRow += GridReport_InitializeRow;
            gridReport.DoubleClickRow += GridReport_DoubleClickRow;

            // Header Drag-to-Hide & Drag-and-Drop Column Chooser
            SetupHeaderDragToHideAndColumnChooser();

            // Responsive Summary Cards & Interactivity
            ultraPanelSummary.Resize += (s, e) => LayoutSummaryCards();
            SetupSummaryCardInteractivity();

            // Set Initial Dates
            dtFrom.DateTime = fromDate;
            dtTo.DateTime = toDate;
        }

        private void FrmCashInOutReport_Load(object sender, EventArgs e)
        {
            InitializeStyling();
            PopulateCombos();
            UpdateDateControlVisibility();
            LayoutSummaryCards();
            LoadData();
        }

        private void FrmCashInOutReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                e.Handled = true;
                LoadData();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        private void FrmCashInOutReport_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_columnChooserForm != null && !_columnChooserForm.IsDisposed)
            {
                _columnChooserForm.Dispose();
                _columnChooserForm = null;
            }
            _headerToolTip?.Dispose();
        }
        #endregion

        #region UI Styling & Grid Appearance
        private void InitializeStyling()
        {
            BackColor = FormBackColor;

            // Filter Panel
            ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
            ultraPanelControls.Appearance.BorderColor = BorderBlue;
            ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;

            // Action Bar Panel
            ultraPanelActionBar.Appearance.BackColor = ActionBarBackColor;
            ultraPanelActionBar.Appearance.BorderColor = BorderBlue;
            ultraPanelActionBar.BorderStyle = UIElementBorderStyle.Solid;

            // Summary Panel
            ultraPanelSummary.Appearance.BackColor = FormBackColor;
            ultraPanelSummary.Appearance.BorderColor = BorderBlue;

            // Style All Action Buttons (Standard Nexoris Gradient Buttons)
            StyleClassicButton(btnViewGrid);
            StyleClassicButton(btnPreviewGrid);
            StyleClassicButton(btnPreviewReport);
            StyleClassicButton(btnExportExcel);
            StyleClassicButton(btnClearFilters);
            StyleClassicButton(btnColumnChooser);
            StyleClassicButton(btnHideSelection);

            SetupGridAppearance(gridReport);
            InitializeGridFooter();
        }

        private static void StyleClassicButton(UltraButton button)
        {
            if (button == null) return;
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.ButtonStyle = UIElementButtonStyle.Office2013Button;
            button.UseFlatMode = DefaultableBoolean.False;
            button.Appearance.BackColor = ButtonBlueTop;
            button.Appearance.BackColor2 = ButtonBlueBottom;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.ForeColor = ButtonTextBlue;
            button.Appearance.BorderColor = ButtonLightOutline;
            button.Appearance.TextHAlign = HAlign.Center;
            button.Appearance.TextVAlign = VAlign.Middle;
            button.Appearance.FontData.Bold = DefaultableBoolean.False;
            button.Appearance.FontData.Name = "Segoe UI";
            button.Appearance.FontData.SizeInPoints = 9;
            button.HotTrackAppearance.BackColor = Color.FromArgb(241, 247, 254);
            button.HotTrackAppearance.BackColor2 = Color.FromArgb(166, 195, 231);
            button.HotTrackAppearance.BackGradientStyle = GradientStyle.Vertical;
            button.HotTrackAppearance.BorderColor = ButtonLightOutline;
        }

        private void SetupGridAppearance(UltraGrid grid)
        {
            if (grid == null) return;

            grid.ResetDisplayLayout();
            grid.UseAppStyling = false;
            grid.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = grid.DisplayLayout;
            layout.Appearance.BackColor = FormBackColor;
            layout.Appearance.BorderColor = BorderBlue;
            layout.BorderStyle = UIElementBorderStyle.Solid;

            layout.CaptionVisible = DefaultableBoolean.False;
            layout.GroupByBox.Hidden = true;

            // Override settings
            UltraGridOverride ov = layout.Override;
            ov.HeaderAppearance.BackColor = GridHeaderBlue;
            ov.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            ov.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            ov.HeaderAppearance.ForeColor = Color.White;
            ov.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            ov.HeaderAppearance.FontData.Name = "Segoe UI";
            ov.HeaderAppearance.FontData.SizeInPoints = 9;
            ov.HeaderAppearance.TextHAlign = HAlign.Center;
            ov.HeaderAppearance.BorderColor = BorderBlue;
            ov.HeaderStyle = HeaderStyle.Standard;

            ov.RowAppearance.BackColor = Color.White;
            ov.RowAppearance.ForeColor = Color.FromArgb(30, 30, 30);
            ov.RowAppearance.BorderColor = GridRowLine;
            ov.RowAppearance.FontData.Name = "Segoe UI";
            ov.RowAppearance.FontData.SizeInPoints = 9;

            ov.RowAlternateAppearance.BackColor = GridAltRow;
            ov.RowAlternateAppearance.ForeColor = Color.FromArgb(30, 30, 30);
            ov.RowAlternateAppearance.BorderColor = GridRowLine;

            ov.SelectedRowAppearance.BackColor = GridSelectedBlue;
            ov.SelectedRowAppearance.ForeColor = Color.Black;

            ov.ActiveRowAppearance.BackColor = GridSelectedBlue;
            ov.ActiveRowAppearance.ForeColor = Color.Black;

            ov.BorderStyleRow = UIElementBorderStyle.Solid;
            ov.BorderStyleCell = UIElementBorderStyle.Solid;
            ov.CellPadding = 2;

            ov.AllowAddNew = AllowAddNew.No;
            ov.AllowDelete = DefaultableBoolean.False;
            ov.AllowUpdate = DefaultableBoolean.False;
            ov.CellClickAction = CellClickAction.RowSelect;
            ov.SelectTypeRow = SelectType.Single;

            ov.AllowColSizing = AllowColSizing.Free;
            ov.AllowColMoving = AllowColMoving.WithinBand;
            ov.AllowColSwapping = AllowColSwapping.WithinBand;

            // Row Selectors & Native Column Chooser Button in Top-Left
            ov.RowSelectors = DefaultableBoolean.True;
            ov.RowSelectorWidth = 35;
            ov.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            ov.RowSelectorHeaderStyle = RowSelectorHeaderStyle.ColumnChooserButton;
            ov.RowSelectorAppearance.BackColor = GridHeaderBlueDark;
            ov.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
            ov.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            ov.RowSelectorAppearance.ForeColor = Color.White;
            ov.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            ov.RowSelectorAppearance.TextHAlign = HAlign.Center;

            layout.ScrollBounds = ScrollBounds.ScrollToFill;
            layout.ScrollStyle = ScrollStyle.Immediate;

            // Sync Footer Panel with Grid Resize, Scrolling, Sorting, Filtering, and Column Moving
            grid.Resize += (s, e) => UpdateFooterCellPositions();
            grid.AfterColPosChanged += (s, e) =>
            {
                UpdateFooterCellPositions();
                ApplyUserHiddenColumns();
            };
            grid.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
            grid.AfterRowRegionScroll += (s, e) => UpdateFooterCellPositions();
            grid.Paint += (s, e) => UpdateFooterCellPositions();
            grid.AfterRowFilterChanged += (s, e) =>
            {
                UpdateFooterValues();
                UpdateFooterCellPositions();
            };
            grid.AfterSortChange += (s, e) => UpdateFooterValues();
        }
        #endregion

        #region Filters & Data Loading
        private void PopulateCombos()
        {
            // Date Quick Select
            cmbDateQuickSelect.Items.Clear();
            cmbDateQuickSelect.Items.Add("TODAY", "Today");
            cmbDateQuickSelect.Items.Add("YESTERDAY", "Yesterday");
            cmbDateQuickSelect.Items.Add("THIS_WEEK", "This Week");
            cmbDateQuickSelect.Items.Add("THIS_MONTH", "This Month");
            cmbDateQuickSelect.Items.Add("THIS_YEAR", "This Year");
            cmbDateQuickSelect.Items.Add("ALL", "All Dates");
            cmbDateQuickSelect.SelectedIndex = 0;

            // Category Filter
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add("ALL", "All Categories");
            cmbCategory.Items.Add("IN", "All Inflows [IN]");
            cmbCategory.Items.Add("OUT", "All Outflows [OUT]");
            cmbCategory.Items.Add("VENDOR_PAYMENT", "Vendor Payments");
            cmbCategory.Items.Add("CUSTOMER_RECEIPT", "Customer Receipts");
            cmbCategory.Items.Add("COUNTER_CASH_SALES", "Counter Cash Sales");
            cmbCategory.Items.Add("DIGITAL_SALES", "Digital Sales");
            cmbCategory.Items.Add("DIRECT_PURCHASE", "Direct Purchases");
            cmbCategory.Items.Add("EXPENSE", "Operating Expenses");
            cmbCategory.Items.Add("GENERAL_RECEIPT", "General Receipts");
            cmbCategory.Items.Add("DRAWINGS", "Drawings / Other");
            cmbCategory.SelectedIndex = 0;
        }

        private void CmbDateQuickSelect_ValueChanged(object sender, EventArgs e)
        {
            if (cmbDateQuickSelect.Value == null) return;
            string key = cmbDateQuickSelect.Value.ToString();
            DateTime today = DateTime.Today;

            switch (key)
            {
                case "TODAY":
                    dtFrom.DateTime = today;
                    dtTo.DateTime = today;
                    break;
                case "YESTERDAY":
                    dtFrom.DateTime = today.AddDays(-1);
                    dtTo.DateTime = today.AddDays(-1);
                    break;
                case "THIS_WEEK":
                    int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                    dtFrom.DateTime = today.AddDays(-diff);
                    dtTo.DateTime = today;
                    break;
                case "THIS_MONTH":
                    dtFrom.DateTime = new DateTime(today.Year, today.Month, 1);
                    dtTo.DateTime = today;
                    break;
                case "THIS_YEAR":
                    dtFrom.DateTime = new DateTime(today.Year, 1, 1);
                    dtTo.DateTime = today;
                    break;
                case "ALL":
                    dtFrom.DateTime = new DateTime(1990, 1, 1);
                    dtTo.DateTime = today;
                    break;
            }

            UpdateDateControlVisibility();
        }

        private void UpdateDateControlVisibility()
        {
            string key = cmbDateQuickSelect.Value?.ToString() ?? "";
            bool isAll = string.Equals(key, "ALL", StringComparison.OrdinalIgnoreCase);

            lblFromDate.Visible = !isAll;
            dtFrom.Visible = !isAll;
            lblToDate.Visible = !isAll;
            dtTo.Visible = !isAll;

            int offset = isAll ? 324 : 0;

            lblCategory.Location = new Point(522 - offset, 14);
            cmbCategory.Location = new Point(565 - offset, 11);
            lblSearch.Location = new Point(732 - offset, 14);
            txtSearch.Location = new Point(782 - offset, 11);
        }

        public void LoadData()
        {
            if (_isLoading) return;

            try
            {
                _isLoading = true;
                Cursor = Cursors.WaitCursor;

                DateTime fromDate = dtFrom.DateTime.Date;
                DateTime toDate = dtTo.DateTime.Date.AddDays(1).AddMilliseconds(-3);

                _currentData = _repository.GetCashInOutReport(fromDate, toDate, SessionContext.BranchId, SessionContext.CompanyId);

                UpdateSummaryCards();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Cash In / Cash Out report: {ex.Message}", "Report Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isLoading = false;
                Cursor = Cursors.Default;
            }
        }

        private void UpdateSummaryCards()
        {
            if (_currentData == null || _currentData.Summary == null) return;

            var s = _currentData.Summary;
            lblCardInValue.Text = $"₹ {s.TotalCashIn:N2}";
            lblCardInTitle.Text = $"TOTAL CASH IN ({s.InflowCount:N0} Txns)";

            lblCardOutValue.Text = $"₹ {s.TotalCashOut:N2}";
            lblCardOutTitle.Text = $"TOTAL CASH OUT ({s.OutflowCount:N0} Txns)";

            lblCardNetValue.Text = $"₹ {s.NetCashFlow:N2}";
            lblCardNetValue.ForeColor = s.NetCashFlow >= 0 ? CashInGreen : CashOutRed;

            lblCardBalanceValue.Text = $"₹ {s.ClosingBalance:N2}";
            lblCardBalanceTitle.Text = $"CLOSING (Opn: ₹ {s.OpeningBalance:N0})";
        }

        private void LayoutSummaryCards()
        {
            try
            {
                if (ultraPanelSummary == null || pnlCardIn == null || pnlCardOut == null || pnlCardNet == null || pnlCardBalance == null) return;
                int containerW = ultraPanelSummary.ClientArea.ClientSize.Width;
                if (containerW <= 100) return;
                int margin = 10;
                int gap = 10;
                int cardCount = 4;
                int availableW = containerW - (margin * 2) - (gap * (cardCount - 1));
                int cardW = Math.Max(200, availableW / cardCount);

                pnlCardIn.Location = new Point(margin, 6);
                pnlCardIn.Size = new Size(cardW, 54);

                pnlCardOut.Location = new Point(margin + cardW + gap, 6);
                pnlCardOut.Size = new Size(cardW, 54);

                pnlCardNet.Location = new Point(margin + (cardW + gap) * 2, 6);
                pnlCardNet.Size = new Size(cardW, 54);

                pnlCardBalance.Location = new Point(margin + (cardW + gap) * 3, 6);
                pnlCardBalance.Size = new Size(cardW, 54);
            }
            catch
            {
                // Ignore sizing errors
            }
        }

        private void ApplyFilter()
        {
            if (_currentData == null || _currentData.Transactions == null)
            {
                gridReport.DataSource = null;
                CreateFooterCells();
                UpdateFooterValues();
                return;
            }

            var query = _currentData.Transactions.AsEnumerable();

            // Category / Direction Filter
            string catFilter = cmbCategory.Value?.ToString() ?? "ALL";
            if (catFilter == "IN")
            {
                query = query.Where(t => t.Direction == "IN");
            }
            else if (catFilter == "OUT")
            {
                query = query.Where(t => t.Direction == "OUT");
            }
            else if (catFilter != "ALL")
            {
                query = query.Where(t => string.Equals(t.Category, catFilter, StringComparison.OrdinalIgnoreCase));
            }

            // Search Text
            string search = txtSearch.Text?.Trim() ?? "";
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t =>
                    (t.VoucherNo ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (t.Particulars ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (t.Narration ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (t.PaymentMode ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (t.VoucherType ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            _filteredTransactions = query.ToList();

            // Bind to grid
            gridReport.DataSource = null;
            gridReport.DataSource = _filteredTransactions;

            ApplyUserHiddenColumns();
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private void ResetFilters()
        {
            cmbDateQuickSelect.SelectedIndex = 0;
            UpdateDateControlVisibility();
            cmbCategory.SelectedIndex = 0;
            txtSearch.Text = "";
            LoadData();
        }

        private void ToggleSelection()
        {
            ultraPanelControls.Visible = !ultraPanelControls.Visible;
            btnHideSelection.Text = ultraPanelControls.Visible ? "&Hide Selection" : "&Show Selection";
        }
        #endregion

        #region Grid Column Layout & Row Formatting
        private void GridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                if (e.Layout.Bands.Count == 0) return;
                var band = e.Layout.Bands[0];

                // Configure Columns
                int pos = 0;
                SetCol(band, "SlNo", "S.No", 50, HAlign.Center, pos++);
                SetCol(band, "VoucherDate", "Date & Time", 125, HAlign.Center, pos++, "dd-MM-yyyy HH:mm");
                SetCol(band, "VoucherNo", "Voucher / Bill No", 110, HAlign.Center, pos++);
                SetCol(band, "VoucherType", "Voucher Type", 100, HAlign.Left, pos++);
                SetCol(band, "Category", "Category", 130, HAlign.Left, pos++);
                SetCol(band, "Direction", "Direction", 70, HAlign.Center, pos++);
                SetCol(band, "Particulars", "Particulars / Party / Ledger", 230, HAlign.Left, pos++);
                SetCol(band, "PaymentMode", "Mode", 80, HAlign.Center, pos++);
                SetCol(band, "InflowAmount", "Cash In (₹)", 120, HAlign.Right, pos++, "#,##0.00");
                SetCol(band, "OutflowAmount", "Cash Out (₹)", 120, HAlign.Right, pos++, "#,##0.00");
                SetCol(band, "Narration", "Remarks / Narration", 200, HAlign.Left, pos++);

                if (band.Columns.Exists("Direction"))
                {
                    band.Columns["Direction"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
                if (band.Columns.Exists("InflowAmount"))
                {
                    band.Columns["InflowAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["InflowAmount"].CellAppearance.ForeColor = CashInGreen;
                }
                if (band.Columns.Exists("OutflowAmount"))
                {
                    band.Columns["OutflowAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["OutflowAmount"].CellAppearance.ForeColor = CashOutRed;
                }

                ApplyUserHiddenColumns();
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Grid layout error: {ex.Message}");
            }
        }

        private static void SetCol(UltraGridBand band, string key, string caption, int width, HAlign align, int visiblePosition, string format = null)
        {
            if (!band.Columns.Exists(key)) return;
            var col = band.Columns[key];
            col.Header.Caption = caption;
            col.Width = width;
            col.CellAppearance.TextHAlign = align;
            col.Header.VisiblePosition = visiblePosition;
            if (!string.IsNullOrEmpty(format))
            {
                col.Format = format;
            }
        }

        private void GridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            try
            {
                if (e.Row == null || !e.Row.Band.Columns.Exists("Direction")) return;

                string dir = e.Row.Cells["Direction"].Value?.ToString() ?? "";
                if (dir == "IN")
                {
                    e.Row.Cells["Direction"].Appearance.ForeColor = CashInGreen;
                    e.Row.Cells["Direction"].Appearance.BackColor = Color.FromArgb(232, 245, 233);
                }
                else if (dir == "OUT")
                {
                    e.Row.Cells["Direction"].Appearance.ForeColor = CashOutRed;
                    e.Row.Cells["Direction"].Appearance.BackColor = Color.FromArgb(255, 235, 238);
                }
            }
            catch
            {
                // Ignore row styling error
            }
        }
        #endregion

        #region Summary Panel under Grid Columns (Right-Click Context Menu)
        private void InitializeGridFooter()
        {
            if (ultraPanelGridFooter == null) return;

            ultraPanelGridFooter.Appearance.BackColor = GridHeaderBlueDark;
            ultraPanelGridFooter.Appearance.BackColor2 = GridHeaderBlueDark;
            ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.None;
            ultraPanelGridFooter.Appearance.BorderColor = BorderBlue;
            ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelGridFooter.Height = 28;

            // Set default summaries for key financial columns
            if (!_columnAggregations.ContainsKey("InflowAmount"))
                _columnAggregations["InflowAmount"] = "Sum";
            if (!_columnAggregations.ContainsKey("OutflowAmount"))
                _columnAggregations["OutflowAmount"] = "Sum";
            if (!_columnAggregations.ContainsKey("SlNo"))
                _columnAggregations["SlNo"] = "Count";

            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private void CreateFooterCells()
        {
            if (ultraPanelGridFooter == null) return;
            ultraPanelGridFooter.ClientArea.Controls.Clear();
            _footerLabels.Clear();

            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            int xOffset = (gridReport.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True)
                ? gridReport.DisplayLayout.Override.RowSelectorWidth
                : 0;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden)
                    continue;

                ContentAlignment align = ContentAlignment.MiddleCenter;
                if (column.CellAppearance.TextHAlign == HAlign.Right || IsNumericColumn(column.Key))
                    align = ContentAlignment.MiddleRight;
                else if (column.CellAppearance.TextHAlign == HAlign.Left)
                    align = ContentAlignment.MiddleLeft;

                Label footerLabel = new Label
                {
                    Name = "footer_" + column.Key,
                    Text = string.Empty,
                    TextAlign = align,
                    BackColor = GridHeaderBlueDark,
                    BorderStyle = BorderStyle.None,
                    AutoSize = false,
                    Width = column.Width,
                    Height = Math.Max(ultraPanelGridFooter.Height - 2, 20),
                    Left = xOffset,
                    Top = 1,
                    Tag = column.Key,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ContextMenuStrip = CreateFooterContextMenu(column.Key)
                };

                footerLabel.Paint += FooterLabel_Paint;
                ultraPanelGridFooter.ClientArea.Controls.Add(footerLabel);
                _footerLabels[column.Key] = footerLabel;

                if (!_columnAggregations.ContainsKey(column.Key))
                {
                    _columnAggregations[column.Key] = _summaryDefaultNumericColumns.Contains(column.Key) ? "Sum" : "None";
                }

                xOffset += column.Width;
            }
        }

        private void FooterLabel_Paint(object sender, PaintEventArgs e)
        {
            if (sender is Label lbl)
            {
                using (Pen borderPen = new Pen(BorderBlue, 1))
                {
                    e.Graphics.DrawLine(borderPen, lbl.Width - 1, 0, lbl.Width - 1, lbl.Height);
                }
            }
        }

        private ContextMenuStrip CreateFooterContextMenu(string columnKey)
        {
            ContextMenuStrip menu = new ContextMenuStrip { Font = new Font("Segoe UI", 9F) };
            menu.Tag = columnKey;
            PopulateSummaryMenuItems(menu.Items, columnKey);
            return menu;
        }

        private void PopulateSummaryMenuItems(ToolStripItemCollection items, string columnKey)
        {
            bool isNumeric = IsNumericColumn(columnKey);

            string currentAgg = _columnAggregations.ContainsKey(columnKey)
                ? _columnAggregations[columnKey]
                : "None";

            AddSummaryMenuItem(items, columnKey, "Sum", "Sum (∑)", isNumeric, currentAgg);
            AddSummaryMenuItem(items, columnKey, "Avg", "Average (x̄)", isNumeric, currentAgg);
            AddSummaryMenuItem(items, columnKey, "Min", "Minimum (Min)", true, currentAgg);
            AddSummaryMenuItem(items, columnKey, "Max", "Maximum (Max)", true, currentAgg);
            AddSummaryMenuItem(items, columnKey, "Count", "Count (N)", true, currentAgg);
            items.Add(new ToolStripSeparator());
            AddSummaryMenuItem(items, columnKey, "None", "None (Clear)", true, currentAgg);
        }

        private void AddSummaryMenuItem(ToolStripItemCollection items, string columnKey, string aggKey, string text, bool enabled, string currentAgg)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text)
            {
                Tag = aggKey,
                Enabled = enabled,
                Checked = string.Equals(aggKey, currentAgg, StringComparison.OrdinalIgnoreCase)
            };
            item.Click += (s, e) =>
            {
                _columnAggregations[columnKey] = aggKey;
                UpdateFooterValues();
            };
            items.Add(item);
        }

        private bool IsNumericColumn(string columnKey)
        {
            if (string.IsNullOrEmpty(columnKey)) return false;
            if (_summaryDefaultNumericColumns.Contains(columnKey)) return true;
            if (gridReport.DisplayLayout.Bands.Count > 0 && gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey))
            {
                var col = gridReport.DisplayLayout.Bands[0].Columns[columnKey];
                if (col.DataType == typeof(decimal) || col.DataType == typeof(double) ||
                    col.DataType == typeof(float) || col.DataType == typeof(int) ||
                    col.DataType == typeof(long) || col.DataType == typeof(short))
                    return true;
            }
            return false;
        }

        private void UpdateFooterCellPositions()
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0 || _footerLabels.Count == 0 || ultraPanelGridFooter == null)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            int rowSelectorWidth = (gridReport.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True)
                ? gridReport.DisplayLayout.Override.RowSelectorWidth
                : 0;

            int scrollOffset = 0;
            if (gridReport.ActiveColScrollRegion != null)
            {
                scrollOffset = gridReport.ActiveColScrollRegion.Position;
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
                footerLabel.Top = 1;
                footerLabel.Height = Math.Max(ultraPanelGridFooter.Height - 2, 20);
                footerLabel.Visible = (left + width > 0 && left < ultraPanelGridFooter.Width);
                footerLabel.Invalidate();
            }
        }

        private void UpdateFooterValues()
        {
            if (_footerLabels.Count == 0) return;

            List<UltraGridRow> visibleRows = GetVisibleDataRows().ToList();
            foreach (KeyValuePair<string, Label> footerEntry in _footerLabels)
            {
                string columnKey = footerEntry.Key;
                Label footerLabel = footerEntry.Value;

                if (!_columnAggregations.ContainsKey(columnKey) ||
                    string.Equals(_columnAggregations[columnKey], "None", StringComparison.OrdinalIgnoreCase))
                {
                    footerLabel.Text = string.Empty;
                    footerLabel.Invalidate();
                    continue;
                }

                string agg = _columnAggregations[columnKey];
                object result = CalculateAggregation(columnKey, agg, visibleRows);
                string displayValue = FormatAggregationResult(columnKey, agg, result);

                footerLabel.Text = displayValue;
                footerLabel.ForeColor = Color.White;
                footerLabel.Invalidate();
            }
        }

        private IEnumerable<UltraGridRow> GetVisibleDataRows()
        {
            if (gridReport.Rows == null) yield break;
            foreach (UltraGridRow row in gridReport.Rows)
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
                    return visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Count(row => HasCellValue(row.Cells[columnKey].Value));
                case "Avg":
                case "Average":
                    var values = visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => GetNumericValue(row.Cells[columnKey].Value))
                        .Where(value => value.HasValue)
                        .Select(value => value.Value)
                        .ToList();
                    return values.Count > 0 ? (decimal?)values.Average() : null;
                default:
                    return null;
            }
        }

        private static bool HasCellValue(object value)
        {
            return value != null && value != DBNull.Value && !string.IsNullOrWhiteSpace(value.ToString());
        }

        private static decimal? GetNumericValue(object value)
        {
            if (!HasCellValue(value))
                return null;

            decimal result;
            if (decimal.TryParse(value.ToString(), out result))
                return result;

            return null;
        }

        private string FormatAggregationResult(string columnKey, string aggregation, object result)
        {
            if (result == null)
                return string.Empty;

            bool isCurrency = string.Equals(columnKey, "InflowAmount", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(columnKey, "OutflowAmount", StringComparison.OrdinalIgnoreCase);

            if (aggregation == "Count")
                return $"{result}";

            if (result is decimal decValue)
            {
                string numStr = isCurrency ? ("₹ " + decValue.ToString("N2")) : decValue.ToString("N2");
                if (aggregation == "Avg" || aggregation == "Average") return $"Avg: {numStr}";
                if (aggregation == "Min") return $"Min: {numStr}";
                if (aggregation == "Max") return $"Max: {numStr}";
                return numStr;
            }

            if (result is DateTime dtValue)
            {
                return dtValue.ToString("dd-MM-yyyy");
            }

            if (aggregation == "Min") return $"Min: {result}";
            if (aggregation == "Max") return $"Max: {result}";

            return result.ToString();
        }
        #endregion

        #region Drag & Drop Column Chooser Implementation
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
            gridReport.AllowDrop = true;
            gridReport.MouseDown += GridReport_MouseDown;
            gridReport.MouseMove += GridReport_MouseMove;
            gridReport.MouseUp += GridReport_MouseUp;
            gridReport.DragOver += GridReport_DragOver;
            gridReport.DragDrop += GridReport_DragDrop;
            gridReport.BeforeColumnChooserDisplayed += (s, e) =>
            {
                e.Cancel = true;
                ShowColumnChooserForm();
            };
        }

        private void GridReport_MouseDown(object sender, MouseEventArgs e)
        {
            Point pt = new Point(e.X, e.Y);
            UIElement element = gridReport.DisplayLayout.UIElement?.ElementFromPoint(pt);
            HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

            if (e.Button == MouseButtons.Right)
            {
                if (headerUI != null && headerUI.Header?.Column != null)
                {
                    ShowHeaderContextMenu(headerUI.Header.Column, pt);
                    return;
                }
            }

            if (e.Button == MouseButtons.Left)
            {
                if (headerUI != null && headerUI.Header?.Column != null)
                {
                    _columnBeingDragged = headerUI.Header.Column;
                    _headerDragStartPoint = pt;
                    _isDraggingHeaderToHide = false;
                }
                else
                {
                    _columnBeingDragged = null;
                }
            }
        }

        private void GridReport_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _columnBeingDragged != null)
            {
                int deltaY = e.Y - _headerDragStartPoint.Y;
                int deltaX = Math.Abs(e.X - _headerDragStartPoint.X);

                if (deltaY > 12 && deltaY > deltaX)
                {
                    _isDraggingHeaderToHide = true;
                    Cursor.Current = _blackXCursor;
                    _headerToolTip.Show("Drag down to hide column", gridReport, e.X + 15, e.Y + 15, 500);
                }
                else
                {
                    _isDraggingHeaderToHide = false;
                    Cursor.Current = Cursors.Default;
                    _headerToolTip.Hide(gridReport);
                }
            }
        }

        private void GridReport_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _columnBeingDragged != null)
            {
                if (_isDraggingHeaderToHide)
                {
                    HideColumn(_columnBeingDragged);
                    _headerToolTip.Hide(gridReport);
                }
                _columnBeingDragged = null;
                _isDraggingHeaderToHide = false;
                Cursor.Current = Cursors.Default;
            }
        }

        private void GridReport_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ColumnChooserItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void GridReport_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(ColumnChooserItem)) is ColumnChooserItem item)
            {
                Point clientPt = gridReport.PointToClient(new Point(e.X, e.Y));
                int dropPosition = GetTargetColumnPositionFromPoint(clientPt);
                UnhideColumn(item.ColumnKey, dropPosition);
            }
        }

        private int GetTargetColumnPositionFromPoint(Point pt)
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return 0;

            UIElement element = gridReport.DisplayLayout.UIElement?.ElementFromPoint(pt);
            HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

            if (headerUI != null && headerUI.Header?.Column != null)
            {
                return headerUI.Header.Column.Header.VisiblePosition;
            }

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
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
            _userHiddenColumnKeys.Add(col.Key);
            col.Hidden = true;
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            if (_columnChooserForm != null && _columnChooserForm.Visible)
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

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem summaryMenu = new ToolStripMenuItem($"📊 Summary for '{colName}'");
            PopulateSummaryMenuItems(summaryMenu.DropDownItems, col.Key);
            menu.Items.Add(summaryMenu);

            menu.Show(gridReport, location);
        }

        public void ShowColumnChooserForm()
        {
            if (_columnChooserForm == null || _columnChooserForm.IsDisposed)
            {
                CreateColumnChooserForm();
            }

            PopulateColumnChooserListBox();
            _columnChooserForm.Show(this);
            PositionColumnChooser();
        }

        private void CreateColumnChooserForm()
        {
            _columnChooserForm = new Form
            {
                Text = "Customization (Column Chooser)",
                Size = new Size(260, 360),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(240, 244, 248),
                ShowIcon = false,
                ShowInTaskbar = false
            };

            _columnChooserForm.FormClosing += (s, e) =>
            {
                e.Cancel = true;
                _columnChooserForm.Hide();
            };

            _txtColumnSearch = new TextBox
            {
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 9F),
                BorderStyle = BorderStyle.FixedSingle
            };
            _txtColumnSearch.TextChanged += (s, e) => PopulateColumnChooserListBox();

            _columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                AllowDrop = true,
                DrawMode = DrawMode.OwnerDrawFixed,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(240, 244, 248),
                ItemHeight = 32,
                IntegralHeight = false
            };

            _columnChooserListBox.DrawItem += ColumnChooserListBox_DrawItem;
            _columnChooserListBox.DoubleClick += ColumnChooserListBox_DoubleClick;
            _columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;

            _columnChooserForm.Controls.Add(_columnChooserListBox);
            _columnChooserForm.Controls.Add(_txtColumnSearch);
        }

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _columnChooserListBox != null)
            {
                int index = _columnChooserListBox.IndexFromPoint(e.Location);
                if (index >= 0 && index < _columnChooserListBox.Items.Count)
                {
                    if (_columnChooserListBox.Items[index] is ColumnChooserItem item)
                    {
                        _columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
                    }
                }
            }
        }

        private void PopulateColumnChooserListBox()
        {
            if (_columnChooserListBox == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            _columnChooserListBox.Items.Clear();
            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            string filterText = _txtColumnSearch?.Text?.Trim() ?? "";

            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.Hidden)
                {
                    string caption = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;
                    if (string.IsNullOrEmpty(filterText) || caption.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        _columnChooserListBox.Items.Add(new ColumnChooserItem(col.Key, caption));
                    }
                }
            }
        }

        private void ColumnChooserListBox_DoubleClick(object sender, EventArgs e)
        {
            if (_columnChooserListBox.SelectedItem is ColumnChooserItem item)
            {
                UnhideColumn(item.ColumnKey);
            }
        }

        private void UnhideColumn(string columnKey, int? targetVisiblePosition = null)
        {
            _userHiddenColumnKeys.Remove(columnKey);
            if (gridReport.DisplayLayout.Bands.Count > 0 && gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey))
            {
                UltraGridColumn col = gridReport.DisplayLayout.Bands[0].Columns[columnKey];
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
            _userHiddenColumnKeys.Clear();
            if (gridReport.DisplayLayout.Bands.Count == 0) return;
            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            foreach (UltraGridColumn col in band.Columns)
            {
                col.Hidden = false;
            }
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            PopulateColumnChooserListBox();
        }

        private void ApplyUserHiddenColumns()
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0) return;
            var band = gridReport.DisplayLayout.Bands[0];
            foreach (var key in _userHiddenColumnKeys)
            {
                if (band.Columns.Exists(key))
                {
                    band.Columns[key].Hidden = true;
                }
            }
        }

        private void PositionColumnChooser()
        {
            if (_columnChooserForm != null && !_columnChooserForm.IsDisposed && _columnChooserForm.Visible)
            {
                _columnChooserForm.Location = new Point(
                    Right - _columnChooserForm.Width - 30,
                    Bottom - _columnChooserForm.Height - 30);
                _columnChooserForm.BringToFront();
            }
        }

        private void ColumnChooserListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || _columnChooserListBox == null || e.Index >= _columnChooserListBox.Items.Count)
                return;

            if (!(_columnChooserListBox.Items[e.Index] is ColumnChooserItem item))
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
        #endregion

        #region Previews & Export
        private void ShowGridPreview(string title)
        {
            try
            {
                if (_filteredTransactions == null || _filteredTransactions.Count == 0)
                {
                    MessageBox.Show("There is no data to preview. Click View Grid first.", "Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                gridReport.PrintPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open print preview: {ex.Message}", "Print", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ShowReportPreview()
        {
            try
            {
                if (_filteredTransactions == null || _filteredTransactions.Count == 0)
                {
                    MessageBox.Show("There is no data to preview. Click View Grid first.", "Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (Form preview = new Form())
                using (Panel header = new Panel())
                using (UltraGrid previewGrid = new UltraGrid())
                {
                    preview.Text = "Cash In / Cash Out Flow - Report Preview";
                    preview.StartPosition = FormStartPosition.CenterParent;
                    preview.WindowState = FormWindowState.Maximized;
                    preview.MinimumSize = new Size(1024, 600);
                    preview.BackColor = FormBackColor;
                    preview.Padding = new Padding(10);

                    // Header banner
                    header.Dock = DockStyle.Top;
                    header.Height = 70;
                    header.BackColor = GridHeaderBlueDark;
                    header.Padding = new Padding(16, 8, 16, 8);

                    Label titleLabel = new Label
                    {
                        Dock = DockStyle.Top,
                        Height = 28,
                        Text = "CASH IN / CASH OUT FLOW REPORT",
                        ForeColor = Color.White,
                        BackColor = Color.Transparent,
                        Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                        TextAlign = ContentAlignment.MiddleLeft
                    };

                    decimal totalIn = _filteredTransactions.Sum(t => t.InflowAmount);
                    decimal totalOut = _filteredTransactions.Sum(t => t.OutflowAmount);
                    decimal net = totalIn - totalOut;

                    Label subtitleLabel = new Label
                    {
                        Dock = DockStyle.Fill,
                        Text = $"Period: {dtFrom.DateTime:dd-MMM-yyyy} to {dtTo.DateTime:dd-MMM-yyyy}   |   Total Cash In: ₹ {totalIn:N2}   |   Total Cash Out: ₹ {totalOut:N2}   |   Net Flow: ₹ {net:N2}",
                        ForeColor = Color.FromArgb(224, 238, 252),
                        BackColor = Color.Transparent,
                        Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                        TextAlign = ContentAlignment.MiddleLeft
                    };

                    header.Controls.Add(subtitleLabel);
                    header.Controls.Add(titleLabel);

                    // Grid
                    previewGrid.Dock = DockStyle.Fill;
                    previewGrid.BackColor = Color.White;
                    SetupGridAppearance(previewGrid);
                    previewGrid.InitializeLayout += GridReport_InitializeLayout;
                    previewGrid.InitializeRow += GridReport_InitializeRow;
                    previewGrid.DataSource = _filteredTransactions.ToList();

                    preview.Controls.Add(previewGrid);
                    preview.Controls.Add(header);

                    preview.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to display report preview: {ex.Message}", "Preview Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToExcel()
        {
            try
            {
                if (_filteredTransactions == null || _filteredTransactions.Count == 0)
                {
                    MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "CSV File (*.csv)|*.csv",
                    FileName = $"CashInOutReport_{DateTime.Now:yyyyMMdd_HHmm}.csv"
                })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("SlNo,Date,VoucherNo,VoucherType,Category,Direction,Particulars,Mode,InflowAmount,OutflowAmount,Narration");

                        foreach (var t in _filteredTransactions)
                        {
                            sb.AppendLine($"{t.SlNo},\"{t.VoucherDate:dd-MM-yyyy HH:mm}\",\"{t.VoucherNo}\",\"{t.VoucherType}\",\"{t.Category}\",\"{t.Direction}\",\"{t.Particulars?.Replace("\"", "\"\"")}\",\"{t.PaymentMode}\",{t.InflowAmount:F2},{t.OutflowAmount:F2},\"{t.Narration?.Replace("\"", "\"\"")}\"");
                        }

                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show("Report exported successfully!", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting data: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Drill-Down & Transaction Navigation
        private void SetupSummaryCardInteractivity()
        {
            var inControls = new Control[] { pnlCardIn, lblCardInTitle, lblCardInValue };
            var outControls = new Control[] { pnlCardOut, lblCardOutTitle, lblCardOutValue };
            var netControls = new Control[] { pnlCardNet, lblCardNetTitle, lblCardNetValue };
            var balanceControls = new Control[] { pnlCardBalance, lblCardBalanceTitle, lblCardBalanceValue };

            foreach (var ctrl in inControls)
            {
                ctrl.Cursor = Cursors.Hand;
                ctrl.Click += (s, e) => FilterByDirection("IN");
            }
            foreach (var ctrl in outControls)
            {
                ctrl.Cursor = Cursors.Hand;
                ctrl.Click += (s, e) => FilterByDirection("OUT");
            }
            foreach (var ctrl in netControls)
            {
                ctrl.Cursor = Cursors.Hand;
                ctrl.Click += (s, e) => FilterByDirection("ALL");
            }
            foreach (var ctrl in balanceControls)
            {
                ctrl.Cursor = Cursors.Hand;
                ctrl.Click += (s, e) => FilterByDirection("ALL");
            }
        }

        private void FilterByDirection(string direction)
        {
            if (cmbCategory.Items.Count > 0)
            {
                if (direction == "IN")
                    cmbCategory.Value = "IN";
                else if (direction == "OUT")
                    cmbCategory.Value = "OUT";
                else
                    cmbCategory.Value = "ALL";
            }
        }

        private void GridReport_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            try
            {
                if (e.Row == null || !e.Row.IsDataRow) return;

                var txn = e.Row.ListObject as CashInOutTransaction;
                if (txn == null) return;

                string voucherType = txn.VoucherType?.Trim() ?? "";
                string voucherNo = txn.VoucherNo?.Trim() ?? "";

                if (string.IsNullOrEmpty(voucherNo))
                {
                    MessageBox.Show("No voucher or bill number found for this transaction.",
                        "Transaction Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Cursor = Cursors.WaitCursor;

                switch (voucherType)
                {
                    case "Counter Sales":
                        if (long.TryParse(voucherNo, out long billNo))
                        {
                            var frmSales = new frmSalesInvoice(billNo);
                            OpenDrillDownForm(frmSales, $"Sales Invoice #{billNo}");
                        }
                        else
                        {
                            MessageBox.Show($"Could not parse bill number: {voucherNo}", "Drill-Down", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        break;

                    case "Cash Purchase":
                        if (int.TryParse(voucherNo, out int purchaseNo))
                        {
                            var frmPur = new FrmPurchase(purchaseNo);
                            OpenDrillDownForm(frmPur, $"Purchase Bill #{purchaseNo}");
                        }
                        else
                        {
                            MessageBox.Show($"Could not parse purchase number: {voucherNo}", "Drill-Down", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        break;

                    case "Customer Receipt":
                        if (long.TryParse(voucherNo, out long receiptVoucherId))
                        {
                            var frmRcp = new FrmReceipt(receiptVoucherId);
                            OpenDrillDownForm(frmRcp, $"Customer Receipt #{receiptVoucherId}");
                        }
                        else
                        {
                            MessageBox.Show($"Could not parse receipt voucher ID: {voucherNo}", "Drill-Down", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        break;

                    case "Vendor Payment":
                        if (long.TryParse(voucherNo, out long paymentVoucherId))
                        {
                            var frmPay = new FrmPayment(paymentVoucherId);
                            OpenDrillDownForm(frmPay, $"Vendor Payment #{paymentVoucherId}");
                        }
                        else
                        {
                            MessageBox.Show($"Could not parse payment voucher ID: {voucherNo}", "Drill-Down", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        break;

                    case "General Receipt":
                        var frmGenRec = new FrmGeneralReceipt(voucherNo);
                        OpenDrillDownForm(frmGenRec, $"General Receipt #{voucherNo}");
                        break;

                    case "Expense Payment":
                        var frmGenPay = new FrmGeneralPayment(voucherNo);
                        OpenDrillDownForm(frmGenPay, $"Expense Payment #{voucherNo}");
                        break;

                    case "Contra / Transfer":
                        var frmContra = new FrmContra(voucherNo);
                        OpenDrillDownForm(frmContra, $"Contra Voucher #{voucherNo}");
                        break;

                    default:
                        MessageBox.Show($"Transaction '{voucherType}' (Voucher #{voucherNo}) does not have an associated editor.",
                            "Drill-Down", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening transaction details: {ex.Message}", "Drill-Down Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void OpenDrillDownForm(Form targetForm, string tabTitle)
        {
            try
            {
                var homeForm = Application.OpenForms.OfType<Home>().FirstOrDefault();
                if (homeForm != null)
                {
                    var openTabMethod = homeForm.GetType().GetMethod("OpenFormInTabSafe",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                        ?? homeForm.GetType().GetMethod("OpenFormInTab",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                    if (openTabMethod != null)
                    {
                        openTabMethod.Invoke(homeForm, new object[] { targetForm, tabTitle });
                        return;
                    }
                }

                targetForm.Show();
                targetForm.BringToFront();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open {tabTitle}: {ex.Message}", "Drill-Down Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
    }
}
