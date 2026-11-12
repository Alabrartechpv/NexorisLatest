using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using PosBranch_Win.Accounts;
using PosBranch_Win.DialogBox;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class frmdatewiseexpensereport : Form
    {
        #region Colour Palette (Parity with frmVendorOutstandingReport)
        private static readonly Color FormBackColor        = Color.FromArgb(232, 246, 255);
        private static readonly Color FilterPanelBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(206, 223, 238);
        private static readonly Color BorderBlue           = Color.FromArgb(118, 154, 198);
        private static readonly Color ControlBackColor     = Color.White;
        private static readonly Color ControlTextColor     = Color.FromArgb(18, 49, 102);
        private static readonly Color GridHeaderBlue       = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark   = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue     = Color.FromArgb(126, 126, 245);
        private static readonly Color GridRowLine          = Color.FromArgb(197, 217, 241);
        private static readonly Color GridAltRow           = Color.FromArgb(246, 250, 255);
        private static readonly Color GridFooterBorder     = Color.FromArgb(144, 181, 223);
        private static readonly Color ButtonBlueTop        = Color.FromArgb(232, 241, 252);
        private static readonly Color ButtonBlueBottom     = Color.FromArgb(145, 181, 224);
        private static readonly Color ButtonBlueBorder     = Color.FromArgb(62, 104, 166);
        private static readonly Color ButtonLightOutline   = Color.FromArgb(166, 183, 202);
        private static readonly Color SkyBlueOutline       = Color.FromArgb(160, 210, 255);
        private static readonly Color ButtonTextBlue       = Color.FromArgb(14, 47, 108);

        // Expense Accent Colors
        private static readonly Color DirectExpenseColor   = Color.FromArgb(230, 81, 0);   // Dark Amber/Orange
        private static readonly Color IndirectExpenseColor = Color.FromArgb(13, 71, 161);  // Deep Navy
        private static readonly Color TotalExpenseColor    = Color.FromArgb(183, 28, 28);  // Crimson
        private static readonly Color ZeroMutedColor       = Color.FromArgb(170, 170, 170);
        #endregion

        #region Inner Helper Types
        private sealed class PivotDateColumnInfo
        {
            public string ColumnKey { get; set; }
            public string HeaderCaption { get; set; }
            public string Tooltip { get; set; }
            public DateTime? Date { get; set; }
            public int DayNumber { get; set; }
            public int MonthNumber { get; set; }
        }

        private sealed class ExpenseLedgerSummary
        {
            public int LedgerID { get; set; }
            public string LedgerName { get; set; }
            public string GroupName { get; set; }
            public string ExpenseCategory { get; set; }
            public decimal TotalExpense { get; set; }
            public int VoucherCount { get; set; }
            public DateTime LastExpenseDate { get; set; }
            public string PrimaryPaymentMode { get; set; }
            public string BranchName { get; set; }
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

            public override string ToString() => DisplayText;
        }
        #endregion

        #region Private Fields
        private readonly DatewiseExpenseReportRepository _repository;
        private DatewiseExpenseReportData _reportData;
        private List<DatewiseExpenseReportRow> _reportRows;
        private List<AccountGroupDDLItem> _categories;
        private List<ExpenseLedgerDDLItem> _ledgers;
        private bool _isLoading;

        private readonly List<PivotDateColumnInfo> _currentMatrixDateColumns = new List<PivotDateColumnInfo>();
        private DataSet _matrixDataSet;
        private DataTable _matrixTable;

        // Custom Dynamic Footer
        private readonly Dictionary<string, Label> _footerLabels;
        private readonly Dictionary<string, string> _columnAggregations;
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> summaryDefaultNumericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Total",
            "TotalExpense",
            "ExpenseAmount",
            "AverageAmount",
            "AverageVoucher",
            "DirectExpense",
            "IndirectExpense",
            "Debit",
            "Credit"
        };

        // Header drag-to-hide & Column Chooser
        private Form columnChooserForm;
        private ListBox columnChooserListBox;
        private TextBox txtColumnSearch;
        private bool isDraggingHeaderToHide;
        private UltraGridColumn columnBeingDragged;
        private Point headerDragStartPoint;
        private ReportColumnFilterManager _filterManager;
        private static readonly Cursor blackXCursor = CreateBlackXCursor();
        #endregion

        #region Constructor & Form Lifecycle
        public frmdatewiseexpensereport()
        {
            _repository = new DatewiseExpenseReportRepository();
            _reportData = new DatewiseExpenseReportData();
            _reportRows = new List<DatewiseExpenseReportRow>();
            _categories = new List<AccountGroupDDLItem>();
            _ledgers = new List<ExpenseLedgerDDLItem>();
            _footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
            _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            InitializeComponent();
            ApplyTheme();
            WireEvents();
        }

        private void frmdatewiseexpensereport_Load(object sender, EventArgs e)
        {
            _isLoading = true;
            try
            {
                PopulateDropdowns();
                SetDefaultFilterDates();
            }
            finally
            {
                _isLoading = false;
            }

            LoadReport();
        }

        private void WireEvents()
        {
            this.Load += frmdatewiseexpensereport_Load;
            this.KeyDown += frmdatewiseexpensereport_KeyDown;
            this.KeyPreview = true;

            ultraComboPeriodMode.ValueChanged += UltraComboPeriodMode_ValueChanged;
            ultraComboViewMode.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboExpenseCategory.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboExpenseLedger.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboPaymentMode.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };

            btnExpenseLedgerPicker.Click += BtnExpenseLedgerPicker_Click;
            btnViewGrid.Click += (s, e) => LoadReport();
            btnPreviewGrid.Click += (s, e) => PreviewGrid();
            btnPreviewReport.Click += (s, e) => PreviewReport();
            btnExportGrid.Click += (s, e) => ExportGridData();
            btnClearFilters.Click += (s, e) => ResetFilters();
            btnColumnChooser.Click += (s, e) => ShowColumnChooser();
            btnExpandAll.Click += (s, e) => ExpandAllBands();
            btnCollapseAll.Click += (s, e) => CollapseAllBands();
            btnToggleSelection.Click += (s, e) => ToggleSelectionPanel();

            txtSearch.ValueChanged += (s, e) => ApplySearchFilter();

            // Grid events
            gridReport.InitializeLayout += GridReport_InitializeLayout;
            gridReport.InitializeRow += GridReport_InitializeRow;
            gridReport.DoubleClickRow += GridReport_DoubleClickRow;
            gridReport.AfterColPosChanged += GridReport_AfterColPosChanged;
            gridReport.SizeChanged += (s, e) => SyncFooterPositions();
            gridReport.Paint += (s, e) => SyncFooterPositions();

            // Drag-to-hide events
            gridReport.MouseDown += GridReport_MouseDown;
            gridReport.MouseMove += GridReport_MouseMove;
            gridReport.MouseUp += GridReport_MouseUp;
        }

        private void frmdatewiseexpensereport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                e.Handled = true;
                BtnExpenseLedgerPicker_Click(btnExpenseLedgerPicker, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.F5)
            {
                e.Handled = true;
                LoadReport();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (columnChooserForm != null && columnChooserForm.Visible)
                {
                    columnChooserForm.Close();
                    e.Handled = true;
                }
            }
        }
        #endregion

        #region Theme & UI Styling (Matching frmVendorOutstandingReport)
        private void ApplyTheme()
        {
            this.BackColor = FormBackColor;

            // Filter Panel
            ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
            ultraPanelControls.Appearance.BorderColor = BorderBlue;
            ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;

            // Action Panel
            ultraPanelAction.Appearance.BackColor = ActionPanelBackColor;
            ultraPanelAction.Appearance.BorderColor = BorderBlue;
            ultraPanelAction.BorderStyle = UIElementBorderStyle.Solid;

            // Master & Grid
            ultraPanelMaster.Appearance.BackColor = FormBackColor;
            ultraPanelMaster.BorderStyle = UIElementBorderStyle.None;

            // Custom Footer Panel
            ultraPanelGridFooter.Appearance.BackColor = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackColor2 = GridHeaderBlueDark;
            ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.Vertical;
            ultraPanelGridFooter.Appearance.BorderColor = GridFooterBorder;
            ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelGridFooter.Height = 38;

            // Apply modern styles to buttons
            StyleActionButton(btnViewGrid, "View Grid");
            StyleActionButton(btnPreviewGrid, "Preview Grid");
            StyleActionButton(btnPreviewReport, "Preview Report");
            StyleActionButton(btnExportGrid, "Export Grid");
            StyleActionButton(btnClearFilters, "Clear Filters");
            StyleActionButton(btnColumnChooser, "Column Chooser");
            StyleActionButton(btnExpandAll, "Expand All");
            StyleActionButton(btnCollapseAll, "Collapse All");
            StyleActionButton(btnToggleSelection, "Hide Selection");

            // F11 picker button
            btnExpenseLedgerPicker.FlatStyle = FlatStyle.Flat;
            btnExpenseLedgerPicker.BackColor = ButtonBlueTop;
            btnExpenseLedgerPicker.ForeColor = ButtonTextBlue;
            btnExpenseLedgerPicker.FlatAppearance.BorderColor = ButtonBlueBorder;
            btnExpenseLedgerPicker.Cursor = Cursors.Hand;
            btnExpenseLedgerPicker.Font = new Font("Segoe UI", 8F, FontStyle.Bold);

            // Labels styling
            lblPeriodMode.Appearance.ForeColor = ControlTextColor;
            lblFromDate.Appearance.ForeColor = ControlTextColor;
            lblToDate.Appearance.ForeColor = ControlTextColor;
            lblMonth.Appearance.ForeColor = ControlTextColor;
            lblYear.Appearance.ForeColor = ControlTextColor;
            lblViewMode.Appearance.ForeColor = ControlTextColor;
            lblExpenseCategory.Appearance.ForeColor = ControlTextColor;
            lblExpenseLedger.Appearance.ForeColor = ControlTextColor;
            lblPaymentMode.Appearance.ForeColor = ControlTextColor;
            lblSearch.Appearance.ForeColor = ControlTextColor;

            // Base Grid Setup
            SetupGridBaseAppearance();
        }

        private static void StyleActionButton(UltraButton btn, string caption)
        {
            btn.Text = caption;
            btn.UseOsThemes = DefaultableBoolean.False;
            btn.ButtonStyle = UIElementButtonStyle.Flat;
            btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            btn.Appearance.BackColor = ButtonBlueTop;
            btn.Appearance.BackColor2 = ButtonBlueBottom;
            btn.Appearance.BackGradientStyle = GradientStyle.Vertical;
            btn.Appearance.BorderColor = ButtonBlueBorder;
            btn.Appearance.ForeColor = ButtonTextBlue;

            btn.HotTrackAppearance.BackColor = Color.FromArgb(240, 248, 255);
            btn.HotTrackAppearance.BackColor2 = Color.FromArgb(173, 209, 240);
            btn.HotTrackAppearance.BackGradientStyle = GradientStyle.Vertical;
            btn.HotTrackAppearance.BorderColor = Color.FromArgb(43, 107, 168);
            btn.HotTrackAppearance.ForeColor = Color.FromArgb(10, 35, 75);

            btn.PressedAppearance.BackColor = Color.FromArgb(180, 213, 242);
            btn.PressedAppearance.BackColor2 = Color.FromArgb(145, 185, 225);
            btn.PressedAppearance.BackGradientStyle = GradientStyle.Vertical;
            btn.PressedAppearance.BorderColor = Color.FromArgb(29, 78, 137);
        }

        private void SetupGridBaseAppearance()
        {
            var layout = gridReport.DisplayLayout;
            layout.Reset();

            layout.ViewStyle = ViewStyle.MultiBand;
            layout.ViewStyleBand = ViewStyleBand.Vertical;
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.AllowColMoving = AllowColMoving.WithinBand;
            layout.Override.AllowColSizing = AllowColSizing.Free;

            // Headers
            layout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            layout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 9.25f;
            layout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;

            // Row styles
            layout.Override.RowAppearance.BackColor = Color.White;
            layout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            layout.Override.RowAppearance.BorderColor = GridRowLine;
            layout.Override.RowAppearance.FontData.SizeInPoints = 9.0f;
            layout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;

            // GroupBy
            layout.GroupByBox.Hidden = false;
            layout.GroupByBox.Prompt = "Drag an expense category or group header here to group transactions";
            layout.GroupByBox.Appearance.BackColor = ActionPanelBackColor;
            layout.GroupByBox.Appearance.ForeColor = ControlTextColor;
            layout.GroupByBox.PromptAppearance.ForeColor = Color.FromArgb(90, 115, 150);

            // Expansion & scroll
            layout.Override.ExpansionIndicator = ShowExpansionIndicator.CheckOnDisplay;
            layout.Scrollbars = Scrollbars.Both;
            layout.ScrollBounds = ScrollBounds.ScrollToFill;
        }
        #endregion

        #region Dropdown & Filter Population
        private void PopulateDropdowns()
        {
            // Period Modes
            ultraComboPeriodMode.Items.Clear();
            ultraComboPeriodMode.Items.Add("DATE_RANGE", "Date Range");
            ultraComboPeriodMode.Items.Add("MONTH_WISE", "Month Wise");
            ultraComboPeriodMode.Items.Add("YEAR_WISE", "Year Wise");
            ultraComboPeriodMode.Items.Add("ALL", "All Dates");
            ultraComboPeriodMode.Value = "DATE_RANGE";

            // Months
            ultraComboMonth.Items.Clear();
            string[] monthNames = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;
            for (int i = 1; i <= 12; i++)
            {
                ultraComboMonth.Items.Add(i, monthNames[i - 1]);
            }
            ultraComboMonth.Value = DateTime.Today.Month;

            // Years
            ultraComboYear.Items.Clear();
            int currentYear = DateTime.Today.Year;
            for (int y = currentYear - 5; y <= currentYear + 2; y++)
            {
                ultraComboYear.Items.Add(y, y.ToString());
            }
            ultraComboYear.Value = currentYear;

            // View Modes
            ultraComboViewMode.Items.Clear();
            ultraComboViewMode.Items.Add("DATE_MATRIX", "Date Matrix (Expense vs Dates)");
            ultraComboViewMode.Items.Add("DETAILED", "Detailed (Voucher Level Breakdown)");
            ultraComboViewMode.Items.Add("DATE_SUMMARY", "Date-wise Summary (Date vs Expense)");
            ultraComboViewMode.Items.Add("EXPENSE_SUMMARY", "Category Summary (Category vs Expense)");
            ultraComboViewMode.Value = "DATE_MATRIX";

            // Payment Modes
            ultraComboPaymentMode.Items.Clear();
            ultraComboPaymentMode.Items.Add("ALL", "ALL Modes");
            ultraComboPaymentMode.Items.Add("CASH", "Cash");
            ultraComboPaymentMode.Items.Add("BANK", "Bank Transfer / Online");
            ultraComboPaymentMode.Items.Add("CHEQUE", "Cheque");
            ultraComboPaymentMode.Value = "ALL";

            // Expense Categories
            PopulateCategories();

            // Expense Ledgers
            PopulateLedgers();
        }

        private void PopulateCategories()
        {
            ultraComboExpenseCategory.Items.Clear();
            ultraComboExpenseCategory.Items.Add(0, "ALL Categories");
            ultraComboExpenseCategory.Items.Add(10, "Direct Expenses");
            ultraComboExpenseCategory.Items.Add(12, "Indirect Expenses");

            try
            {
                _categories = _repository.GetExpenseCategories();
                foreach (var cat in _categories)
                {
                    if (cat.GroupID != 10 && cat.GroupID != 12)
                    {
                        ultraComboExpenseCategory.Items.Add(cat.GroupID, cat.GroupName);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading categories: {ex.Message}");
            }

            ultraComboExpenseCategory.Value = 0;
        }

        private void PopulateLedgers()
        {
            ultraComboExpenseLedger.Items.Clear();
            ultraComboExpenseLedger.Items.Add(0, "ALL Expense Ledgers");

            try
            {
                _ledgers = _repository.GetExpenseLedgers();
                foreach (var led in _ledgers)
                {
                    ultraComboExpenseLedger.Items.Add(led.LedgerID, led.LedgerName);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading ledgers: {ex.Message}");
            }

            ultraComboExpenseLedger.Value = 0;
        }

        private void SetDefaultFilterDates()
        {
            DateTime today = DateTime.Today;
            DateTime firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            dtFrom.DateTime = firstDayOfMonth;
            dtTo.DateTime = today;
        }

        private void UltraComboPeriodMode_ValueChanged(object sender, EventArgs e)
        {
            string mode = ultraComboPeriodMode.Value?.ToString() ?? "DATE_RANGE";

            bool isMonthWise = string.Equals(mode, "MONTH_WISE", StringComparison.OrdinalIgnoreCase);
            bool isYearWise  = string.Equals(mode, "YEAR_WISE", StringComparison.OrdinalIgnoreCase);
            bool isAll       = string.Equals(mode, "ALL", StringComparison.OrdinalIgnoreCase);
            bool isDateRange = string.Equals(mode, "DATE_RANGE", StringComparison.OrdinalIgnoreCase);

            lblFromDate.Visible = isDateRange;
            dtFrom.Visible      = isDateRange;
            lblToDate.Visible   = isDateRange;
            dtTo.Visible        = isDateRange;

            lblMonth.Visible    = isMonthWise;
            ultraComboMonth.Visible = isMonthWise;

            lblYear.Visible     = isMonthWise || isYearWise;
            ultraComboYear.Visible = isMonthWise || isYearWise;

            if (!_isLoading)
            {
                LoadReport();
            }
        }
        #endregion

        #region Report Data Loading & Binding
        private void LoadReport()
        {
            if (_isLoading) return;

            Cursor prevCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            try
            {
                DatewiseExpenseReportFilter filter = BuildFilterFromControls();
                _reportData = _repository.GetReportData(filter);
                _reportRows = _reportData.ExpenseTransactions ?? new List<DatewiseExpenseReportRow>();

                if (filter.ViewMode == "DATE_MATRIX")
                {
                    BindMatrixView(filter);
                }
                else
                {
                    BindDirectTable(filter);
                }

                UpdateFooterSummaries();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating Date-wise Expense Report:\n{ex.Message}", "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = prevCursor;
            }
        }

        private DatewiseExpenseReportFilter BuildFilterFromControls()
        {
            DatewiseExpenseReportFilter f = new DatewiseExpenseReportFilter();
            f.PeriodMode = ultraComboPeriodMode.Value?.ToString() ?? "DATE_RANGE";
            f.ViewMode   = ultraComboViewMode.Value?.ToString() ?? "DATE_MATRIX";

            if (f.PeriodMode == "MONTH_WISE")
            {
                f.Month = ultraComboMonth.Value != null ? Convert.ToInt32(ultraComboMonth.Value) : DateTime.Today.Month;
                f.Year  = ultraComboYear.Value != null ? Convert.ToInt32(ultraComboYear.Value) : DateTime.Today.Year;
                f.FromDate = new DateTime(f.Year, f.Month, 1);
                f.ToDate   = f.FromDate.AddMonths(1).AddSeconds(-1);
            }
            else if (f.PeriodMode == "YEAR_WISE")
            {
                f.Year  = ultraComboYear.Value != null ? Convert.ToInt32(ultraComboYear.Value) : DateTime.Today.Year;
                f.FromDate = new DateTime(f.Year, 1, 1);
                f.ToDate   = f.FromDate.AddYears(1).AddSeconds(-1);
            }
            else if (f.PeriodMode == "ALL")
            {
                f.FromDate = new DateTime(2000, 1, 1);
                f.ToDate   = new DateTime(2099, 12, 31, 23, 59, 59);
            }
            else
            {
                f.FromDate = dtFrom.DateTime != DateTime.MinValue ? dtFrom.DateTime.Date : DateTime.Today;
                f.ToDate   = dtTo.DateTime != DateTime.MinValue ? dtTo.DateTime.Date.AddDays(1).AddSeconds(-1) : DateTime.Today.AddDays(1).AddSeconds(-1);
            }

            f.ExpenseCategoryId = ultraComboExpenseCategory.Value != null ? Convert.ToInt32(ultraComboExpenseCategory.Value) : 0;
            f.LedgerId          = ultraComboExpenseLedger.Value != null ? Convert.ToInt32(ultraComboExpenseLedger.Value) : 0;
            f.PaymentMode       = ultraComboPaymentMode.Value?.ToString() ?? "ALL";
            f.SearchText        = (txtSearch.Text ?? string.Empty).Trim();
            f.CompanyId         = 1;
            f.BranchId          = 0;

            return f;
        }

        private void BindMatrixView(DatewiseExpenseReportFilter filter)
        {
            _currentMatrixDateColumns.Clear();
            _matrixDataSet = BuildMatrixDataSet(_reportData, _reportRows, filter, out List<PivotDateColumnInfo> dateColumns);
            _currentMatrixDateColumns.AddRange(dateColumns);

            gridReport.DataSource = null;
            gridReport.DataSource = _matrixDataSet;
            gridReport.DataMember = "ExpenseMatrix";

            if (_matrixDataSet.Tables.Count > 0)
            {
                _matrixTable = _matrixDataSet.Tables["ExpenseMatrix"];
            }

            ConfigureMatrixBands();
        }

        private void BindDirectTable(DatewiseExpenseReportFilter filter)
        {
            gridReport.DataSource = null;

            if (_reportData.ReportDataSet != null && _reportData.ReportDataSet.Tables.Count > 0)
            {
                DataTable table = _reportData.ReportDataSet.Tables[0];
                gridReport.DataSource = table;

                if (gridReport.DisplayLayout.Bands.Count > 0)
                {
                    var band = gridReport.DisplayLayout.Bands[0];
                    FormatDirectTableBand(band, filter.ViewMode);
                }
            }
        }
        #endregion

        #region Matrix Building (Band 0 & Band 1)
        private DataSet BuildMatrixDataSet(DatewiseExpenseReportData reportData, List<DatewiseExpenseReportRow> rawRows, DatewiseExpenseReportFilter filter, out List<PivotDateColumnInfo> dateColumns)
        {
            DataTable matrixTable = BuildMatrixDataTable(rawRows, filter, out dateColumns);
            DataTable vouchersTable = BuildVouchersDataTable(reportData, rawRows);

            DataSet ds = new DataSet("DatewiseExpenseDataSet");
            ds.Tables.Add(matrixTable);
            ds.Tables.Add(vouchersTable);

            // Add relationship between LedgerKey for expandable child rows
            DataRelation relation = new DataRelation("ExpenseVoucherBreakdown", matrixTable.Columns["LedgerKey"], vouchersTable.Columns["LedgerKey"], false);
            ds.Relations.Add(relation);

            return ds;
        }

        private DataTable BuildMatrixDataTable(List<DatewiseExpenseReportRow> rawRows, DatewiseExpenseReportFilter filter, out List<PivotDateColumnInfo> dateColumns)
        {
            dateColumns = new List<PivotDateColumnInfo>();
            string periodMode = filter.PeriodMode ?? "DATE_RANGE";

            if (periodMode == "YEAR_WISE")
            {
                string[] monthShortNames = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames;
                for (int m = 1; m <= 12; m++)
                {
                    dateColumns.Add(new PivotDateColumnInfo
                    {
                        ColumnKey = monthShortNames[m - 1],
                        HeaderCaption = monthShortNames[m - 1],
                        Tooltip = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[m - 1],
                        MonthNumber = m
                    });
                }
            }
            else if (periodMode == "MONTH_WISE")
            {
                int year = filter.Year > 0 ? filter.Year : DateTime.Today.Year;
                int month = filter.Month >= 1 && filter.Month <= 12 ? filter.Month : DateTime.Today.Month;
                int daysInMonth = DateTime.DaysInMonth(year, month);

                for (int day = 1; day <= daysInMonth; day++)
                {
                    DateTime colDate = new DateTime(year, month, day);
                    dateColumns.Add(new PivotDateColumnInfo
                    {
                        ColumnKey = day.ToString(),
                        HeaderCaption = day.ToString(),
                        Tooltip = $"{colDate:dd-MMM-yyyy (dddd)}",
                        Date = colDate,
                        DayNumber = day
                    });
                }
            }
            else // DATE_RANGE or ALL
            {
                DateTime start = filter.FromDate.Date;
                DateTime end = filter.ToDate.Date;
                int totalDays = (int)(end - start).TotalDays + 1;

                if (totalDays > 0 && totalDays <= 31)
                {
                    for (int i = 0; i < totalDays; i++)
                    {
                        DateTime d = start.AddDays(i);
                        dateColumns.Add(new PivotDateColumnInfo
                        {
                            ColumnKey = $"D_{d:yyyyMMdd}",
                            HeaderCaption = d.ToString("dd-MMM"),
                            Tooltip = $"{d:dd-MMM-yyyy (dddd)}",
                            Date = d,
                            DayNumber = d.Day
                        });
                    }
                }
                else
                {
                    var distinctDates = (rawRows ?? new List<DatewiseExpenseReportRow>())
                        .Select(r => r.VoucherDate.Date)
                        .Distinct()
                        .OrderBy(d => d)
                        .ToList();

                    if (distinctDates.Count == 0)
                    {
                        distinctDates.Add(start);
                    }

                    foreach (var d in distinctDates)
                    {
                        dateColumns.Add(new PivotDateColumnInfo
                        {
                            ColumnKey = $"D_{d:yyyyMMdd}",
                            HeaderCaption = d.ToString("dd-MMM"),
                            Tooltip = $"{d:dd-MMM-yyyy (dddd)}",
                            Date = d,
                            DayNumber = d.Day
                        });
                    }
                }
            }

            // Create DataTable Schema
            DataTable table = new DataTable("ExpenseMatrix");
            table.Columns.Add("LedgerKey", typeof(string));
            table.Columns.Add("ExpenseCategory", typeof(string));
            table.Columns.Add("GroupName", typeof(string));
            table.Columns.Add("LedgerName", typeof(string));

            foreach (var col in dateColumns)
            {
                var dc = table.Columns.Add(col.ColumnKey, typeof(decimal));
                dc.DefaultValue = 0m;
            }

            var cTotal = table.Columns.Add("TotalExpense", typeof(decimal));
            cTotal.DefaultValue = 0m;

            var cCount = table.Columns.Add("VoucherCount", typeof(int));
            cCount.DefaultValue = 0;

            var cAvg = table.Columns.Add("AverageVoucher", typeof(decimal));
            cAvg.DefaultValue = 0m;

            var cLastDate = table.Columns.Add("LastExpenseDate", typeof(DateTime));

            var cMode = table.Columns.Add("PrimaryPaymentMode", typeof(string));
            cMode.DefaultValue = string.Empty;

            var cBranch = table.Columns.Add("BranchName", typeof(string));
            cBranch.DefaultValue = string.Empty;

            if (rawRows == null || rawRows.Count == 0)
            {
                return table;
            }

            // Aggregation by ledger and date
            Dictionary<string, decimal> dateExpenseMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, ExpenseLedgerSummary> ledgerSummaryMap = new Dictionary<string, ExpenseLedgerSummary>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, HashSet<int>> ledgerVouchersMap = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);

            foreach (DatewiseExpenseReportRow r in rawRows)
            {
                string ledgerKey = r.LedgerID > 0 ? r.LedgerID.ToString() : (r.LedgerName ?? string.Empty);

                ExpenseLedgerSummary summary;
                if (!ledgerSummaryMap.TryGetValue(ledgerKey, out summary))
                {
                    summary = new ExpenseLedgerSummary
                    {
                        LedgerID = r.LedgerID,
                        LedgerName = !string.IsNullOrEmpty(r.LedgerName) ? r.LedgerName : "Unknown Expense",
                        GroupName = r.GroupName ?? "EXPENSES",
                        ExpenseCategory = r.ExpenseCategory ?? "Operating Expense",
                        TotalExpense = 0m,
                        VoucherCount = 0,
                        LastExpenseDate = r.VoucherDate,
                        PrimaryPaymentMode = r.PaymentMode ?? "CASH",
                        BranchName = r.BranchName ?? string.Empty
                    };
                    ledgerSummaryMap[ledgerKey] = summary;
                    ledgerVouchersMap[ledgerKey] = new HashSet<int>();
                }
                else
                {
                    if (r.VoucherDate > summary.LastExpenseDate)
                    {
                        summary.LastExpenseDate = r.VoucherDate;
                        if (!string.IsNullOrEmpty(r.PaymentMode))
                            summary.PrimaryPaymentMode = r.PaymentMode;
                    }
                }

                summary.TotalExpense += r.ExpenseAmount;
                if (r.VoucherID > 0)
                {
                    ledgerVouchersMap[ledgerKey].Add(r.VoucherID);
                }

                foreach (PivotDateColumnInfo dateCol in dateColumns)
                {
                    bool isMatch = false;
                    if (periodMode == "YEAR_WISE")
                    {
                        isMatch = (r.VoucherDate.Month == dateCol.MonthNumber);
                    }
                    else if (periodMode == "MONTH_WISE")
                    {
                        isMatch = (r.VoucherDate.Day == dateCol.DayNumber);
                    }
                    else
                    {
                        if (dateCol.Date.HasValue)
                        {
                            isMatch = (r.VoucherDate.Date == dateCol.Date.Value.Date);
                        }
                    }

                    if (isMatch)
                    {
                        string cellKey = $"{ledgerKey}_{dateCol.ColumnKey}";
                        if (!dateExpenseMap.ContainsKey(cellKey))
                            dateExpenseMap[cellKey] = 0m;
                        dateExpenseMap[cellKey] += r.ExpenseAmount;
                    }
                }
            }

            // Populate rows
            var sortedSummaries = ledgerSummaryMap.Values
                .OrderBy(s => s.ExpenseCategory == "Direct Expense" ? 0 : 1)
                .ThenBy(s => s.GroupName)
                .ThenBy(s => s.LedgerName)
                .ToList();

            foreach (var s in sortedSummaries)
            {
                string ledgerKey = s.LedgerID > 0 ? s.LedgerID.ToString() : s.LedgerName;
                DataRow dr = table.NewRow();
                dr["LedgerKey"] = ledgerKey;
                dr["ExpenseCategory"] = s.ExpenseCategory;
                dr["GroupName"] = s.GroupName;
                dr["LedgerName"] = s.LedgerName;

                decimal rowTotal = 0m;
                foreach (PivotDateColumnInfo col in dateColumns)
                {
                    string cellKey = $"{ledgerKey}_{col.ColumnKey}";
                    decimal val = dateExpenseMap.ContainsKey(cellKey) ? dateExpenseMap[cellKey] : 0m;
                    dr[col.ColumnKey] = val;
                    rowTotal += val;
                }

                dr["TotalExpense"] = s.TotalExpense;
                int vCount = ledgerVouchersMap.ContainsKey(ledgerKey) ? ledgerVouchersMap[ledgerKey].Count : 0;
                dr["VoucherCount"] = vCount;
                dr["AverageVoucher"] = vCount > 0 ? Math.Round(s.TotalExpense / vCount, 2) : 0m;
                if (s.LastExpenseDate != DateTime.MinValue)
                {
                    dr["LastExpenseDate"] = s.LastExpenseDate.Date;
                }
                dr["PrimaryPaymentMode"] = s.PrimaryPaymentMode;
                dr["BranchName"] = s.BranchName;

                table.Rows.Add(dr);
            }

            return table;
        }

        private DataTable BuildVouchersDataTable(DatewiseExpenseReportData reportData, List<DatewiseExpenseReportRow> rawRows)
        {
            DataTable table = new DataTable("ExpenseVoucherBreakdown");
            table.Columns.Add("LedgerKey", typeof(string));
            table.Columns.Add("VoucherDate", typeof(DateTime));
            table.Columns.Add("VoucherNumber", typeof(string));
            table.Columns.Add("VoucherType", typeof(string));
            table.Columns.Add("ExpenseCategory", typeof(string));
            table.Columns.Add("ExpenseAmount", typeof(decimal));
            table.Columns.Add("PaidThrough", typeof(string));
            table.Columns.Add("PaymentMode", typeof(string));
            table.Columns.Add("Narration", typeof(string));
            table.Columns.Add("UserName", typeof(string));
            table.Columns.Add("BranchName", typeof(string));

            var sourceRows = reportData.VoucherDetails != null && reportData.VoucherDetails.Count > 0
                ? reportData.VoucherDetails.Select(v => new DatewiseExpenseReportRow
                {
                    LedgerID = v.LedgerID,
                    VoucherID = v.VoucherID,
                    VoucherNumber = v.VoucherNumber,
                    VoucherDate = v.VoucherDate,
                    VoucherType = v.VoucherType,
                    ExpenseCategory = v.ExpenseCategory,
                    ExpenseAmount = v.ExpenseAmount,
                    PaidThrough = v.PaidThrough,
                    PaymentMode = v.PaymentMode,
                    Narration = v.Narration,
                    UserName = v.UserName,
                    BranchName = v.BranchName
                }).ToList()
                : (rawRows ?? new List<DatewiseExpenseReportRow>());

            foreach (var r in sourceRows.OrderBy(x => x.LedgerName).ThenBy(x => x.VoucherDate).ThenBy(x => x.VoucherID))
            {
                DataRow dr = table.NewRow();
                dr["LedgerKey"] = r.LedgerID > 0 ? r.LedgerID.ToString() : (r.LedgerName ?? string.Empty);
                if (r.VoucherDate != DateTime.MinValue)
                    dr["VoucherDate"] = r.VoucherDate.Date;
                dr["VoucherNumber"] = !string.IsNullOrEmpty(r.VoucherNumber) ? r.VoucherNumber : r.VoucherID.ToString();
                dr["VoucherType"] = r.VoucherType ?? "GENPAY";
                dr["ExpenseCategory"] = r.ExpenseCategory ?? "Operating Expense";
                dr["ExpenseAmount"] = r.ExpenseAmount;
                dr["PaidThrough"] = !string.IsNullOrEmpty(r.PaidThrough) ? r.PaidThrough : "CASH-IN-HAND";
                dr["PaymentMode"] = !string.IsNullOrEmpty(r.PaymentMode) ? r.PaymentMode : "CASH";
                dr["Narration"] = !string.IsNullOrEmpty(r.Narration) ? r.Narration : "-";
                dr["UserName"] = r.UserName ?? "Admin";
                dr["BranchName"] = r.BranchName ?? string.Empty;
                table.Rows.Add(dr);
            }

            return table;
        }
        #endregion

        #region Band Configuration & Formatting
        private void ConfigureMatrixBands()
        {
            if (gridReport.DisplayLayout.Bands.Count == 0) return;

            // Band 0 (Expense Matrix)
            var band0 = gridReport.DisplayLayout.Bands[0];
            band0.ColHeaderLines = 1;

            if (band0.Columns.Exists("LedgerKey")) band0.Columns["LedgerKey"].Hidden = true;

            if (band0.Columns.Exists("ExpenseCategory"))
            {
                band0.Columns["ExpenseCategory"].Header.Caption = "Category";
                band0.Columns["ExpenseCategory"].Width = 120;
            }

            if (band0.Columns.Exists("GroupName"))
            {
                band0.Columns["GroupName"].Header.Caption = "Group";
                band0.Columns["GroupName"].Width = 140;
            }

            if (band0.Columns.Exists("LedgerName"))
            {
                band0.Columns["LedgerName"].Header.Caption = "Expense Ledger";
                band0.Columns["LedgerName"].Width = 230;
                band0.Columns["LedgerName"].Header.Appearance.FontData.Bold = DefaultableBoolean.True;
            }

            // Date Columns
            foreach (var dateCol in _currentMatrixDateColumns)
            {
                if (band0.Columns.Exists(dateCol.ColumnKey))
                {
                    var col = band0.Columns[dateCol.ColumnKey];
                    col.Header.Caption = dateCol.HeaderCaption;
                    col.Header.ToolTipText = dateCol.Tooltip;
                    col.Format = "#,##0.00";
                    col.CellAppearance.TextHAlign = HAlign.Right;
                    col.Width = 65;
                }
            }

            if (band0.Columns.Exists("TotalExpense"))
            {
                band0.Columns["TotalExpense"].Header.Caption = "Total Expense";
                band0.Columns["TotalExpense"].Format = "#,##0.00";
                band0.Columns["TotalExpense"].CellAppearance.TextHAlign = HAlign.Right;
                band0.Columns["TotalExpense"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                band0.Columns["TotalExpense"].Width = 110;
            }

            if (band0.Columns.Exists("VoucherCount"))
            {
                band0.Columns["VoucherCount"].Header.Caption = "Vouchers";
                band0.Columns["VoucherCount"].CellAppearance.TextHAlign = HAlign.Center;
                band0.Columns["VoucherCount"].Width = 65;
            }

            if (band0.Columns.Exists("AverageVoucher"))
            {
                band0.Columns["AverageVoucher"].Header.Caption = "Avg Voucher";
                band0.Columns["AverageVoucher"].Format = "#,##0.00";
                band0.Columns["AverageVoucher"].CellAppearance.TextHAlign = HAlign.Right;
                band0.Columns["AverageVoucher"].Width = 90;
            }

            if (band0.Columns.Exists("LastExpenseDate"))
            {
                band0.Columns["LastExpenseDate"].Header.Caption = "Last Date";
                band0.Columns["LastExpenseDate"].Format = "dd-MMM-yyyy";
                band0.Columns["LastExpenseDate"].CellAppearance.TextHAlign = HAlign.Center;
                band0.Columns["LastExpenseDate"].Width = 85;
            }

            if (band0.Columns.Exists("PrimaryPaymentMode"))
            {
                band0.Columns["PrimaryPaymentMode"].Header.Caption = "Primary Mode";
                band0.Columns["PrimaryPaymentMode"].Width = 85;
            }

            if (band0.Columns.Exists("BranchName"))
            {
                band0.Columns["BranchName"].Header.Caption = "Branch";
                band0.Columns["BranchName"].Width = 110;
            }

            // Band 1 (Child: Expense Vouchers Breakdown)
            if (gridReport.DisplayLayout.Bands.Count > 1)
            {
                var band1 = gridReport.DisplayLayout.Bands[1];
                band1.Header.Caption = "Expense Vouchers Breakdown (Double-click row to open Voucher)";
                band1.Header.Appearance.BackColor = GridHeaderBlueDark;
                band1.Header.Appearance.ForeColor = Color.White;
                band1.Header.Appearance.FontData.Bold = DefaultableBoolean.True;

                if (band1.Columns.Exists("LedgerKey")) band1.Columns["LedgerKey"].Hidden = true;

                if (band1.Columns.Exists("VoucherDate"))
                {
                    band1.Columns["VoucherDate"].Header.Caption = "Date";
                    band1.Columns["VoucherDate"].Format = "dd-MMM-yyyy";
                    band1.Columns["VoucherDate"].Width = 90;
                }

                if (band1.Columns.Exists("VoucherNumber"))
                {
                    band1.Columns["VoucherNumber"].Header.Caption = "Voucher #";
                    band1.Columns["VoucherNumber"].Width = 100;
                }

                if (band1.Columns.Exists("VoucherType"))
                {
                    band1.Columns["VoucherType"].Header.Caption = "Type";
                    band1.Columns["VoucherType"].Width = 80;
                }

                if (band1.Columns.Exists("ExpenseCategory"))
                {
                    band1.Columns["ExpenseCategory"].Header.Caption = "Category";
                    band1.Columns["ExpenseCategory"].Width = 110;
                }

                if (band1.Columns.Exists("ExpenseAmount"))
                {
                    band1.Columns["ExpenseAmount"].Header.Caption = "Amount";
                    band1.Columns["ExpenseAmount"].Format = "#,##0.00";
                    band1.Columns["ExpenseAmount"].CellAppearance.TextHAlign = HAlign.Right;
                    band1.Columns["ExpenseAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band1.Columns["ExpenseAmount"].Width = 100;
                }

                if (band1.Columns.Exists("PaidThrough"))
                {
                    band1.Columns["PaidThrough"].Header.Caption = "Paid Through";
                    band1.Columns["PaidThrough"].Width = 140;
                }

                if (band1.Columns.Exists("PaymentMode"))
                {
                    band1.Columns["PaymentMode"].Header.Caption = "Mode";
                    band1.Columns["PaymentMode"].Width = 70;
                }

                if (band1.Columns.Exists("Narration"))
                {
                    band1.Columns["Narration"].Header.Caption = "Narration / Remarks";
                    band1.Columns["Narration"].Width = 220;
                }

                if (band1.Columns.Exists("UserName"))
                {
                    band1.Columns["UserName"].Header.Caption = "Created By";
                    band1.Columns["UserName"].Width = 90;
                }

                if (band1.Columns.Exists("BranchName"))
                {
                    band1.Columns["BranchName"].Header.Caption = "Branch";
                    band1.Columns["BranchName"].Width = 100;
                }
            }
        }

        private static void FormatDirectTableBand(UltraGridBand band, string viewMode)
        {
            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.DataType == typeof(decimal) || col.DataType == typeof(double) || col.DataType == typeof(float))
                {
                    col.Format = "#,##0.00";
                    col.CellAppearance.TextHAlign = HAlign.Right;
                }
                else if (col.DataType == typeof(DateTime))
                {
                    col.Format = "dd-MMM-yyyy";
                    col.CellAppearance.TextHAlign = HAlign.Center;
                }
            }

            if (band.Columns.Exists("DirectExpense"))
            {
                band.Columns["DirectExpense"].CellAppearance.ForeColor = DirectExpenseColor;
                band.Columns["DirectExpense"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (band.Columns.Exists("IndirectExpense"))
            {
                band.Columns["IndirectExpense"].CellAppearance.ForeColor = IndirectExpenseColor;
                band.Columns["IndirectExpense"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (band.Columns.Exists("TotalExpense"))
            {
                band.Columns["TotalExpense"].CellAppearance.ForeColor = TotalExpenseColor;
                band.Columns["TotalExpense"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (band.Columns.Exists("PercentageOfTotal"))
            {
                band.Columns["PercentageOfTotal"].Header.Caption = "% of Total";
                band.Columns["PercentageOfTotal"].Format = "0.00'%'";
            }
        }
        #endregion

        #region Grid Row Initialization & Interactions
        private void GridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            _filterManager = new ReportColumnFilterManager(gridReport);
        }

        private void GridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (e.Row.Band.Index == 0)
            {
                // Mute zero values across date columns
                foreach (var dateCol in _currentMatrixDateColumns)
                {
                    if (e.Row.Band.Columns.Exists(dateCol.ColumnKey))
                    {
                        var cell = e.Row.Cells[dateCol.ColumnKey];
                        if (cell.Value != null && cell.Value != DBNull.Value)
                        {
                            decimal val = Convert.ToDecimal(cell.Value);
                            if (val == 0m)
                            {
                                cell.Appearance.ForeColor = ZeroMutedColor;
                            }
                            else
                            {
                                cell.Appearance.ForeColor = TotalExpenseColor;
                                cell.Appearance.FontData.Bold = DefaultableBoolean.True;
                            }
                        }
                    }
                }

                // Category badge colors
                if (e.Row.Band.Columns.Exists("ExpenseCategory"))
                {
                    string cat = e.Row.Cells["ExpenseCategory"].Value?.ToString() ?? string.Empty;
                    if (string.Equals(cat, "Direct Expense", StringComparison.OrdinalIgnoreCase))
                    {
                        e.Row.Cells["ExpenseCategory"].Appearance.ForeColor = DirectExpenseColor;
                        e.Row.Cells["ExpenseCategory"].Appearance.FontData.Bold = DefaultableBoolean.True;
                    }
                    else if (string.Equals(cat, "Indirect Expense", StringComparison.OrdinalIgnoreCase))
                    {
                        e.Row.Cells["ExpenseCategory"].Appearance.ForeColor = IndirectExpenseColor;
                        e.Row.Cells["ExpenseCategory"].Appearance.FontData.Bold = DefaultableBoolean.True;
                    }
                }
            }
        }

        private void GridReport_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            if (e.Row == null) return;

            // Double clicking Band 1 (Child Voucher): open FrmGeneralPayment
            if (e.Row.Band.Index == 1)
            {
                string vNo = e.Row.Cells["VoucherNumber"].Value?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(vNo))
                {
                    try
                    {
                        FrmGeneralPayment paymentForm = new FrmGeneralPayment(vNo);
                        paymentForm.ShowDialog(this);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Could not open Voucher #{vNo}: {ex.Message}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            else if (e.Row.Band.Index == 0)
            {
                // Toggle row expansion
                e.Row.Expanded = !e.Row.Expanded;
            }
        }
        #endregion

        #region Custom Dynamic Footer (Matching frmVendorOutstandingReport)
        private void UpdateFooterSummaries()
        {
            ultraPanelGridFooter.ClientArea.Controls.Clear();
            _footerLabels.Clear();

            int totalLedgers = _reportRows.Select(x => x.LedgerID).Distinct().Count();
            int totalVouchers = _reportRows.Select(x => x.VoucherID).Distinct().Count();
            decimal totalExpense = _reportRows.Sum(x => x.ExpenseAmount);
            decimal directExpense = _reportRows.Where(x => x.ExpenseCategory == "Direct Expense").Sum(x => x.ExpenseAmount);
            decimal indirectExpense = _reportRows.Where(x => x.ExpenseCategory == "Indirect Expense").Sum(x => x.ExpenseAmount);

            // Left badge: Record counter
            Label lblCount = new Label
            {
                Text = $"Total: {totalLedgers} Ledgers | {totalVouchers} Vouchers",
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(12, 10)
            };
            ultraPanelGridFooter.ClientArea.Controls.Add(lblCount);

            // Badges on right side
            int rightX = ultraPanelGridFooter.Width - 16;

            // 1. Total Expense Badge
            Label lblTotalBadge = CreateBadgeLabel($"Total: ₹ {totalExpense:N2}", TotalExpenseColor, Color.White, ref rightX);
            ultraPanelGridFooter.ClientArea.Controls.Add(lblTotalBadge);

            // 2. Indirect Expense Badge
            Label lblIndirectBadge = CreateBadgeLabel($"Indirect: ₹ {indirectExpense:N2}", Color.FromArgb(220, 240, 255), IndirectExpenseColor, ref rightX);
            ultraPanelGridFooter.ClientArea.Controls.Add(lblIndirectBadge);

            // 3. Direct Expense Badge
            Label lblDirectBadge = CreateBadgeLabel($"Direct: ₹ {directExpense:N2}", Color.FromArgb(255, 238, 187), DirectExpenseColor, ref rightX);
            ultraPanelGridFooter.ClientArea.Controls.Add(lblDirectBadge);

            SyncFooterPositions();
        }

        private static Label CreateBadgeLabel(string text, Color backColor, Color foreColor, ref int rightX)
        {
            Label lbl = new Label
            {
                Text = text,
                AutoSize = true,
                BackColor = backColor,
                ForeColor = foreColor,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(6, 2, 6, 2),
                BorderStyle = BorderStyle.FixedSingle
            };
            rightX -= (lbl.PreferredWidth + 10);
            lbl.Location = new Point(rightX, 6);
            return lbl;
        }

        private void SyncFooterPositions()
        {
            if (gridReport.DisplayLayout.Bands.Count == 0) return;
            // Layout is kept cleanly formatted and aligned with bottom panel
        }

        private void GridReport_AfterColPosChanged(object sender, AfterColPosChangedEventArgs e)
        {
            SyncFooterPositions();
        }
        #endregion

        #region Card Column Chooser & Drag-to-Hide
        private void ShowColumnChooser()
        {
            if (gridReport.DisplayLayout.Bands.Count == 0) return;

            var band = gridReport.DisplayLayout.Bands[0];

            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.BringToFront();
                return;
            }

            columnChooserForm = new Form
            {
                Text = "Customization (Field Chooser)",
                Size = new Size(240, 380),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.Manual,
                ShowInTaskbar = false,
                BackColor = FormBackColor
            };

            Point pt = btnColumnChooser.PointToScreen(new Point(0, btnColumnChooser.Height));
            columnChooserForm.Location = pt;

            txtColumnSearch = new TextBox
            {
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 9F),
                Height = 24
            };
            txtColumnSearch.TextChanged += (s, e) => FilterColumnChooserList(band);

            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 28,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White
            };
            columnChooserListBox.DrawItem += ColumnChooserListBox_DrawItem;
            columnChooserListBox.DoubleClick += ColumnChooserListBox_DoubleClick;

            Button btnResetColumns = new Button
            {
                Text = "Reset Columns",
                Dock = DockStyle.Bottom,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = ButtonBlueTop,
                ForeColor = ButtonTextBlue
            };
            btnResetColumns.Click += (s, e) => ResetColumnVisibility(band);

            columnChooserForm.Controls.Add(columnChooserListBox);
            columnChooserForm.Controls.Add(txtColumnSearch);
            columnChooserForm.Controls.Add(btnResetColumns);

            PopulateColumnChooserList(band, string.Empty);
            columnChooserForm.Show(this);
        }

        private void PopulateColumnChooserList(UltraGridBand band, string search)
        {
            columnChooserListBox.Items.Clear();
            string query = (search ?? string.Empty).Trim().ToLowerInvariant();

            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.Key == "LedgerKey") continue;

                if (col.Hidden)
                {
                    string caption = col.Header.Caption;
                    if (string.IsNullOrEmpty(caption)) caption = col.Key;

                    if (string.IsNullOrEmpty(query) || caption.ToLowerInvariant().Contains(query))
                    {
                        columnChooserListBox.Items.Add(new ColumnChooserItem(col.Key, caption));
                    }
                }
            }
        }

        private void FilterColumnChooserList(UltraGridBand band)
        {
            PopulateColumnChooserList(band, txtColumnSearch.Text);
        }

        private void ColumnChooserListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= columnChooserListBox.Items.Count) return;

            e.DrawBackground();
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(e.Bounds.X + 4, e.Bounds.Y + 2, e.Bounds.Width - 8, e.Bounds.Height - 4);
            using (GraphicsPath path = GetRoundedRect(rect, 4))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(245, 248, 252)))
            using (Pen borderPen = new Pen(BorderBlue))
            {
                g.FillPath(brush, path);
                g.DrawPath(borderPen, path);
            }

            ColumnChooserItem item = columnChooserListBox.Items[e.Index] as ColumnChooserItem;
            if (item != null)
            {
                using (SolidBrush textBrush = new SolidBrush(ControlTextColor))
                {
                    StringFormat sf = new StringFormat { LineAlignment = StringAlignment.Center };
                    Rectangle textRect = new Rectangle(rect.X + 8, rect.Y, rect.Width - 16, rect.Height);
                    g.DrawString(item.DisplayText, e.Font, textBrush, textRect, sf);
                }
            }
        }

        private void ColumnChooserListBox_DoubleClick(object sender, EventArgs e)
        {
            if (columnChooserListBox.SelectedItem is ColumnChooserItem item)
            {
                var band = gridReport.DisplayLayout.Bands[0];
                if (band.Columns.Exists(item.ColumnKey))
                {
                    band.Columns[item.ColumnKey].Hidden = false;
                    userHiddenColumnKeys.Remove(item.ColumnKey);
                    FilterColumnChooserList(band);
                }
            }
        }

        private void ResetColumnVisibility(UltraGridBand band)
        {
            userHiddenColumnKeys.Clear();
            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.Key != "LedgerKey")
                {
                    col.Hidden = false;
                }
            }
            FilterColumnChooserList(band);
        }

        private static GraphicsPath GetRoundedRect(Rectangle r, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static Cursor CreateBlackXCursor()
        {
            try
            {
                Bitmap bmp = new Bitmap(16, 16);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using (Pen p = new Pen(Color.Red, 2.5f))
                    {
                        g.DrawLine(p, 2, 2, 14, 14);
                        g.DrawLine(p, 14, 2, 2, 14);
                    }
                }
                return new Cursor(bmp.GetHicon());
            }
            catch
            {
                return Cursors.No;
            }
        }

        private void GridReport_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                UIElement elem = gridReport.DisplayLayout.UIElement.ElementFromPoint(e.Location);
                HeaderUIElement headerElem = elem?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;
                if (headerElem?.Header is Infragistics.Win.UltraWinGrid.ColumnHeader hdr && hdr.Column != null)
                {
                    columnBeingDragged = hdr.Column;
                    headerDragStartPoint = e.Location;
                    isDraggingHeaderToHide = false;
                }
            }
        }

        private void GridReport_MouseMove(object sender, MouseEventArgs e)
        {
            if (columnBeingDragged != null && e.Button == MouseButtons.Left)
            {
                int dy = Math.Abs(e.Y - headerDragStartPoint.Y);
                if (dy > 30)
                {
                    isDraggingHeaderToHide = true;
                    Cursor.Current = blackXCursor;
                }
                else
                {
                    isDraggingHeaderToHide = false;
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void GridReport_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDraggingHeaderToHide && columnBeingDragged != null)
            {
                columnBeingDragged.Hidden = true;
                userHiddenColumnKeys.Add(columnBeingDragged.Key);
                if (columnChooserForm != null && columnChooserForm.Visible)
                {
                    FilterColumnChooserList(gridReport.DisplayLayout.Bands[0]);
                }
            }

            columnBeingDragged = null;
            isDraggingHeaderToHide = false;
            Cursor.Current = Cursors.Default;
        }
        #endregion

        #region Actions & Drill Downs
        private void BtnExpenseLedgerPicker_Click(object sender, EventArgs e)
        {
            try
            {
                using (Form pickerForm = new Form())
                {
                    pickerForm.Text = "Select Expense Ledger";
                    pickerForm.Size = new Size(500, 450);
                    pickerForm.StartPosition = FormStartPosition.CenterParent;
                    pickerForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    pickerForm.MaximizeBox = false;
                    pickerForm.MinimizeBox = false;
                    pickerForm.BackColor = FormBackColor;

                    TextBox txtFilter = new TextBox
                    {
                        Dock = DockStyle.Top,
                        Font = new Font("Segoe UI", 10F),
                        Height = 28
                    };

                    ListBox lst = new ListBox
                    {
                        Dock = DockStyle.Fill,
                        Font = new Font("Segoe UI", 9.5F),
                        IntegralHeight = false
                    };

                    Action populateList = () =>
                    {
                        lst.Items.Clear();
                        string q = txtFilter.Text.Trim().ToLowerInvariant();
                        lst.Items.Add(new ExpenseLedgerDDLItem { LedgerID = 0, LedgerName = "ALL Expense Ledgers" });
                        foreach (var l in _ledgers)
                        {
                            if (string.IsNullOrEmpty(q) || l.LedgerName.ToLowerInvariant().Contains(q) || l.GroupName.ToLowerInvariant().Contains(q))
                            {
                                lst.Items.Add(l);
                            }
                        }
                        if (lst.Items.Count > 0) lst.SelectedIndex = 0;
                    };

                    txtFilter.TextChanged += (s, ev) => populateList();
                    lst.Format += (s, ev) =>
                    {
                        if (ev.ListItem is ExpenseLedgerDDLItem itm)
                        {
                            ev.Value = itm.LedgerID > 0 ? $"{itm.LedgerName} ({itm.ExpenseCategory})" : itm.LedgerName;
                        }
                    };

                    Button btnSelect = new Button
                    {
                        Text = "Select",
                        Dock = DockStyle.Bottom,
                        Height = 36,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = ButtonBlueTop,
                        ForeColor = ButtonTextBlue,
                        Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                    };

                    Action doSelect = () =>
                    {
                        if (lst.SelectedItem is ExpenseLedgerDDLItem sel)
                        {
                            ultraComboExpenseLedger.Value = sel.LedgerID;
                            pickerForm.DialogResult = DialogResult.OK;
                            pickerForm.Close();
                            LoadReport();
                        }
                    };

                    btnSelect.Click += (s, ev) => doSelect();
                    lst.DoubleClick += (s, ev) => doSelect();
                    txtFilter.KeyDown += (s, ev) =>
                    {
                        if (ev.KeyCode == Keys.Enter) doSelect();
                        else if (ev.KeyCode == Keys.Down && lst.Items.Count > 0)
                        {
                            lst.Focus();
                            if (lst.SelectedIndex < lst.Items.Count - 1) lst.SelectedIndex++;
                        }
                    };
                    lst.KeyDown += (s, ev) =>
                    {
                        if (ev.KeyCode == Keys.Enter) doSelect();
                    };

                    Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 34, Padding = new Padding(4) };
                    pnlTop.Controls.Add(txtFilter);

                    pickerForm.Controls.Add(lst);
                    pickerForm.Controls.Add(pnlTop);
                    pickerForm.Controls.Add(btnSelect);

                    populateList();
                    txtFilter.Focus();
                    pickerForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in Expense Ledger picker: {ex.Message}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void PreviewGrid()
        {
            try
            {
                gridReport.PrintPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not preview grid: {ex.Message}", "Print Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void PreviewReport()
        {
            if (_matrixDataSet == null || _matrixDataSet.Tables.Count == 0)
            {
                MessageBox.Show("No data available to preview.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Form previewForm = new Form
                {
                    Text = "Report Preview - Date-wise Expense Report",
                    Size = new Size(1100, 700),
                    StartPosition = FormStartPosition.CenterParent,
                    ShowInTaskbar = false
                };

                UltraGrid previewGrid = new UltraGrid { Dock = DockStyle.Fill };
                previewForm.Controls.Add(previewGrid);
                previewGrid.DataSource = _matrixDataSet.Copy();
                previewForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open report preview: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportGridData()
        {
            if (gridReport.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV File (*.csv)|*.csv|All Files (*.*)|*.*";
                sfd.FileName = $"DatewiseExpenseReport_{DateTime.Now:yyyyMMdd_HHmm}.csv";

                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        ExportToCsv(sfd.FileName);
                        MessageBox.Show($"Successfully exported to:\n{sfd.FileName}", "Export Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Export failed: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ExportToCsv(string filePath)
        {
            StringBuilder sb = new StringBuilder();
            var band = gridReport.DisplayLayout.Bands[0];

            // Headers
            var visibleCols = band.Columns.Cast<UltraGridColumn>().Where(c => !c.Hidden).ToList();
            sb.AppendLine(string.Join(",", visibleCols.Select(c => $"\"{c.Header.Caption.Replace("\"", "\"\"")}\"")));

            // Rows
            foreach (var row in gridReport.Rows)
            {
                if (row.IsFilteredOut) continue;

                var values = visibleCols.Select(c =>
                {
                    object val = row.GetCellValue(c);
                    if (val == null || val == DBNull.Value) return "\"\"";
                    if (val is decimal || val is double || val is float)
                        return ((decimal)Convert.ToDecimal(val)).ToString("F2");
                    if (val is DateTime dt)
                        return dt.ToString("yyyy-MM-dd");
                    return $"\"{val.ToString().Replace("\"", "\"\"")}\"";
                });
                sb.AppendLine(string.Join(",", values));
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private void ResetFilters()
        {
            _isLoading = true;
            try
            {
                ultraComboPeriodMode.Value = "DATE_RANGE";
                ultraComboViewMode.Value = "DATE_MATRIX";
                ultraComboExpenseCategory.Value = 0;
                ultraComboExpenseLedger.Value = 0;
                ultraComboPaymentMode.Value = "ALL";
                txtSearch.Text = string.Empty;
                SetDefaultFilterDates();
            }
            finally
            {
                _isLoading = false;
            }

            LoadReport();
        }

        private void ExpandAllBands()
        {
            gridReport.Rows.ExpandAll(true);
        }

        private void CollapseAllBands()
        {
            gridReport.Rows.CollapseAll(true);
        }

        private void ToggleSelectionPanel()
        {
            ultraPanelControls.Visible = !ultraPanelControls.Visible;
            btnToggleSelection.Text = ultraPanelControls.Visible ? "Hide Selection" : "Show Selection";
        }

        private void ApplySearchFilter()
        {
            string search = (txtSearch.Text ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(search))
            {
                foreach (var row in gridReport.Rows)
                {
                    row.Hidden = false;
                }
                return;
            }

            foreach (var row in gridReport.Rows)
            {
                bool match = false;
                foreach (UltraGridCell cell in row.Cells)
                {
                    if (cell.Value != null && cell.Value.ToString().ToLowerInvariant().Contains(search))
                    {
                        match = true;
                        break;
                    }
                }
                row.Hidden = !match;
            }
        }
        #endregion
    }
}
