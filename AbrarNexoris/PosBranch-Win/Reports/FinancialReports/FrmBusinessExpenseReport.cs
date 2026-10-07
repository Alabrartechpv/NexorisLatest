using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository;
using Repository.ReportRepository;
using PosBranch_Win.Accounts;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class FrmBusinessExpenseReport : Form
    {
        #region Private Fields
        private readonly ExecutiveKpiRepository _repository;
        private List<ExpenseLedgerSummaryItem> _allLedgerSummaries;
        private List<ExpenseLedgerSummaryItem> _filteredLedgerSummaries;
        private List<ExpenseVoucherTransactionItem> _allVoucherTransactions;
        private List<ExpenseVoucherTransactionItem> _filteredVoucherTransactions;

        private bool _isSelectionHidden = false;
        private DateTime? _initialFromDate;
        private DateTime? _initialToDate;
        private int? _initialBranchId;

        // Unified IRS POS Design System Palette
        private static readonly Color FormBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color FilterPanelBackColor = Color.FromArgb(235, 245, 252);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(225, 238, 248);
        private static readonly Color BorderBlue = Color.FromArgb(126, 170, 208);
        private static readonly Color ControlTextColor = Color.FromArgb(18, 49, 102);

        private static readonly Color GridHeaderBlue = Color.FromArgb(29, 78, 137);
        private static readonly Color GridHeaderBlueDark = Color.FromArgb(22, 62, 108);
        private static readonly Color ButtonTextBlue = Color.FromArgb(18, 49, 102);
        private static readonly Color RowAltColor = Color.FromArgb(246, 251, 255);

        private static readonly Color DirectExpenseColor = Color.FromArgb(230, 81, 0);   // Dark Amber/Orange
        private static readonly Color IndirectExpenseColor = Color.FromArgb(13, 71, 161); // Deep Navy
        private static readonly Color NetAmountColor = Color.FromArgb(183, 28, 28);       // Crimson Red for expense
        #endregion

        #region Constructors & Lifecycle
        public FrmBusinessExpenseReport()
        {
            InitializeComponent();

            _repository = new ExecutiveKpiRepository();
            _allLedgerSummaries = new List<ExpenseLedgerSummaryItem>();
            _filteredLedgerSummaries = new List<ExpenseLedgerSummaryItem>();
            _allVoucherTransactions = new List<ExpenseVoucherTransactionItem>();
            _filteredVoucherTransactions = new List<ExpenseVoucherTransactionItem>();

            InitializePanels();

            this.Load += FrmBusinessExpenseReport_Load;
            this.KeyPreview = true;
            this.KeyDown += FrmBusinessExpenseReport_KeyDown;

            // Wire Actions
            btnGenerate.Click += (s, e) => LoadData();
            btnPreviewGrid.Click += (s, e) => PreviewGrid();
            btnPrint.Click += (s, e) => PrintReport();
            btnExportCsv.Click += (s, e) => ExportCsv();
            btnExportExcel.Click += (s, e) => ExportExcel();
            btnClearFilters.Click += (s, e) => ResetFilters();
            btnToggleSelection.Click += (s, e) => ToggleSelectionPanel();

            txtSearch.ValueChanged += (s, e) => ApplyClientFilters();
            cmbViewMode.ValueChanged += (s, e) => OnViewModeChanged();
            cmbCategory.ValueChanged += (s, e) => ApplyClientFilters();

            // Grid Events
            ultraGridExpenses.InitializeLayout += UltraGridExpenses_InitializeLayout;
            ultraGridExpenses.InitializeRow += UltraGridExpenses_InitializeRow;
            ultraGridExpenses.DoubleClickRow += UltraGridExpenses_DoubleClickRow;
        }

        public FrmBusinessExpenseReport(DateTime fromDate, DateTime toDate, int branchId = 0) : this()
        {
            _initialFromDate = fromDate;
            _initialToDate = toDate;
            if (branchId > 0)
            {
                _initialBranchId = branchId;
            }
        }

        private void FrmBusinessExpenseReport_Load(object sender, EventArgs e)
        {
            PopulateDropdowns();

            if (_initialFromDate.HasValue && _initialToDate.HasValue)
            {
                cmbDateQuickSelect.Value = "CUSTOM";
                dtFromDate.DateTime = _initialFromDate.Value;
                dtToDate.DateTime = _initialToDate.Value;
            }
            else
            {
                cmbDateQuickSelect.Value = "THIS_FIN_YEAR";
                UpdateDateRangeForPreset("THIS_FIN_YEAR");
            }

            cmbDateQuickSelect.ValueChanged += CmbDateQuickSelect_ValueChanged;

            // Auto-load data on startup
            LoadData();
        }
        #endregion

        #region Theme & UI Styling
        private void InitializePanels()
        {
            this.BackColor = FormBackColor;

            // Filter Panel
            ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
            ultraPanelControls.Appearance.BorderColor = BorderBlue;
            ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelControls.Dock = DockStyle.Top;
            ultraPanelControls.Height = 78;

            // Labels
            lblSearch.Appearance.ForeColor = ControlTextColor;
            lblViewMode.Appearance.ForeColor = ControlTextColor;
            lblCategory.Appearance.ForeColor = ControlTextColor;
            lblPreset.Appearance.ForeColor = ControlTextColor;
            lblFromDate.Appearance.ForeColor = ControlTextColor;
            lblToDate.Appearance.ForeColor = ControlTextColor;
            lblRowCount.Appearance.ForeColor = ControlTextColor;
            lblRowCount.Appearance.FontData.SizeInPoints = 8.5f;

            // Action Panel
            ultraPanelAction.Appearance.BackColor = ActionPanelBackColor;
            ultraPanelAction.Appearance.BorderColor = BorderBlue;
            ultraPanelAction.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelAction.Dock = DockStyle.Top;
            ultraPanelAction.Height = 45;

            // Master Panel
            ultraPanelMaster.Appearance.BackColor = FormBackColor;
            ultraPanelMaster.Appearance.BorderColor = BorderBlue;
            ultraPanelMaster.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelMaster.Dock = DockStyle.Fill;

            // Footer Panel
            ultraPanelGridFooter.Appearance.BackColor = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackColor2 = GridHeaderBlueDark;
            ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.Vertical;
            ultraPanelGridFooter.Appearance.BorderColor = BorderBlue;
            ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelGridFooter.Dock = DockStyle.Bottom;
            ultraPanelGridFooter.Height = 36;

            // Grid
            ultraGridExpenses.Dock = DockStyle.Fill;

            // Z-Order
            ultraPanelControls.SendToBack();
            ultraPanelAction.BringToFront();
            ultraPanelMaster.BringToFront();
            ultraPanelGridFooter.SendToBack();
            ultraGridExpenses.BringToFront();

            // Footer Labels
            lblDirectExpensesSummary.Appearance.ForeColor = Color.FromArgb(255, 238, 187); // Warm Yellow/Orange
            lblDirectExpensesSummary.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblDirectExpensesSummary.Appearance.FontData.SizeInPoints = 8.5f;

            lblIndirectExpensesSummary.Appearance.ForeColor = Color.FromArgb(220, 240, 255); // Soft Cyan/Blue
            lblIndirectExpensesSummary.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblIndirectExpensesSummary.Appearance.FontData.SizeInPoints = 8.5f;

            lblTotalVouchersSummary.Appearance.ForeColor = Color.FromArgb(220, 255, 220); // Soft Green
            lblTotalVouchersSummary.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblTotalVouchersSummary.Appearance.FontData.SizeInPoints = 8.5f;

            // Total Badge
            lblTotalExpensesSummary.Appearance.BackColor = Color.FromArgb(255, 235, 238);
            lblTotalExpensesSummary.Appearance.BackColor2 = Color.FromArgb(255, 205, 210);
            lblTotalExpensesSummary.Appearance.BackGradientStyle = GradientStyle.Vertical;
            lblTotalExpensesSummary.Appearance.BorderColor = Color.FromArgb(198, 40, 40);
            lblTotalExpensesSummary.Appearance.ForeColor = Color.FromArgb(183, 28, 28);
            lblTotalExpensesSummary.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblTotalExpensesSummary.Appearance.FontData.SizeInPoints = 9.5f;

            StyleButtons();
            SetupGridAppearance();
        }

        private void StyleButtons()
        {
            btnGenerate.Text = "View Grid";
            btnPreviewGrid.Text = "Preview Grid";
            btnPrint.Text = "Preview Report";
            btnExportCsv.Text = "Export CSV";
            btnExportExcel.Text = "Export Excel";
            btnClearFilters.Text = "Reset Filters";
            btnToggleSelection.Text = "Hide Selection";

            StyleClassicButton(btnGenerate);
            StyleClassicButton(btnPreviewGrid);
            StyleClassicButton(btnPrint);
            StyleClassicButton(btnExportCsv);
            StyleClassicButton(btnExportExcel);
            StyleClassicButton(btnClearFilters);
            StyleClassicButton(btnToggleSelection);
        }

        private static void StyleClassicButton(UltraButton button)
        {
            button.UseOsThemes = DefaultableBoolean.False;
            button.ButtonStyle = UIElementButtonStyle.Flat;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            button.Appearance.BackColor = Color.FromArgb(240, 248, 255);
            button.Appearance.BackColor2 = Color.FromArgb(196, 222, 245);
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.BorderColor = BorderBlue;
            button.Appearance.ForeColor = ButtonTextBlue;

            button.HotTrackAppearance.BackColor = Color.FromArgb(223, 240, 255);
            button.HotTrackAppearance.BackColor2 = Color.FromArgb(173, 209, 240);
            button.HotTrackAppearance.BackGradientStyle = GradientStyle.Vertical;
            button.HotTrackAppearance.BorderColor = Color.FromArgb(43, 107, 168);
            button.HotTrackAppearance.ForeColor = Color.FromArgb(10, 35, 75);

            button.PressedAppearance.BackColor = Color.FromArgb(180, 213, 242);
            button.PressedAppearance.BackColor2 = Color.FromArgb(150, 192, 228);
            button.PressedAppearance.BackGradientStyle = GradientStyle.Vertical;
            button.PressedAppearance.BorderColor = Color.FromArgb(29, 78, 137);
            button.PressedAppearance.ForeColor = Color.FromArgb(6, 23, 50);
        }

        private void SetupGridAppearance()
        {
            var grid = ultraGridExpenses;
            grid.DisplayLayout.Reset();

            grid.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            grid.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            grid.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;

            grid.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            grid.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            grid.DisplayLayout.Override.RowSelectorWidth = 40;
            grid.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            grid.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;

            grid.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            grid.DisplayLayout.GroupByBox.Hidden = true;

            grid.DisplayLayout.Override.MinRowHeight = 26;
            grid.DisplayLayout.Override.DefaultRowHeight = 26;

            grid.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            grid.DisplayLayout.Override.RowAlternateAppearance.BackColor = RowAltColor;

            grid.DisplayLayout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            grid.DisplayLayout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            grid.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            grid.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            grid.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            grid.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9F;
            grid.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
            grid.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

            grid.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(218, 236, 252);
            grid.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.Black;

            grid.DisplayLayout.Override.CellAppearance.BorderColor = Color.FromArgb(220, 230, 240);
            grid.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
        }
        #endregion

        #region Dropdowns & Presets
        private void PopulateDropdowns()
        {
            // View Modes
            cmbViewMode.Items.Clear();
            cmbViewMode.Items.Add("SUMMARY", "Summary by Ledger");
            cmbViewMode.Items.Add("DETAIL", "Voucher Transactions Detail");
            cmbViewMode.Value = "SUMMARY";

            // Categories
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add("ALL", "All Categories");
            cmbCategory.Items.Add("DIRECT", "Direct Expenses");
            cmbCategory.Items.Add("INDIRECT", "Indirect Expenses");
            cmbCategory.Value = "ALL";

            // Date Presets
            cmbDateQuickSelect.Items.Clear();
            cmbDateQuickSelect.Items.Add("ALL", "ALL (Full History)");
            cmbDateQuickSelect.Items.Add("TODAY", "Today");
            cmbDateQuickSelect.Items.Add("THIS_MONTH", "This Month");
            cmbDateQuickSelect.Items.Add("LAST_MONTH", "Last Month");
            cmbDateQuickSelect.Items.Add("THIS_FIN_YEAR", "This Financial Year");
            cmbDateQuickSelect.Items.Add("CUSTOM", "Custom Range");
        }

        private void CmbDateQuickSelect_ValueChanged(object sender, EventArgs e)
        {
            if (cmbDateQuickSelect.Value == null) return;
            string key = cmbDateQuickSelect.Value.ToString();
            UpdateDateRangeForPreset(key);
        }

        private void UpdateDateRangeForPreset(string presetKey)
        {
            DateTime today = DateTime.Today;

            switch (presetKey)
            {
                case "ALL":
                    dtFromDate.DateTime = new DateTime(1990, 1, 1);
                    dtToDate.DateTime = today;
                    break;
                case "TODAY":
                    dtFromDate.DateTime = today;
                    dtToDate.DateTime = today;
                    break;
                case "THIS_MONTH":
                    dtFromDate.DateTime = new DateTime(today.Year, today.Month, 1);
                    dtToDate.DateTime = today;
                    break;
                case "LAST_MONTH":
                    var firstDayLastMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                    dtFromDate.DateTime = firstDayLastMonth;
                    dtToDate.DateTime = firstDayLastMonth.AddMonths(1).AddDays(-1);
                    break;
                case "THIS_FIN_YEAR":
                    int startYear = today.Month >= 4 ? today.Year : today.Year - 1;
                    dtFromDate.DateTime = new DateTime(startYear, 4, 1);
                    dtToDate.DateTime = today;
                    break;
                case "CUSTOM":
                    break;
            }
        }
        #endregion

        #region Data Loading & Filtering
        private void LoadData()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                DateTime fromDate = dtFromDate.DateTime.Date;
                DateTime toDate = dtToDate.DateTime.Date;
                int branchId = _initialBranchId.HasValue && _initialBranchId.Value > 0
                    ? _initialBranchId.Value
                    : (SessionContext.BranchId > 0 ? SessionContext.BranchId : (int.TryParse(DataBase.BranchId, out int b) ? b : 0));
                int companyId = SessionContext.CompanyId > 0 ? SessionContext.CompanyId : (int.TryParse(DataBase.CompanyId, out int c) ? c : 0);

                _allLedgerSummaries = _repository.GetExpenseLedgerSummary(fromDate, toDate, branchId, companyId) ?? new List<ExpenseLedgerSummaryItem>();
                _allVoucherTransactions = _repository.GetExpenseVoucherTransactions(fromDate, toDate, branchId, companyId) ?? new List<ExpenseVoucherTransactionItem>();

                ApplyClientFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Business Expenses: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void OnViewModeChanged()
        {
            ApplyClientFilters();
        }

        private void ApplyClientFilters()
        {
            string search = (txtSearch.Text ?? "").Trim().ToLowerInvariant();
            string categoryFilter = cmbCategory.Value?.ToString() ?? "ALL";
            string viewMode = cmbViewMode.Value?.ToString() ?? "SUMMARY";

            if (viewMode == "SUMMARY")
            {
                var query = _allLedgerSummaries.AsEnumerable();

                if (categoryFilter == "DIRECT")
                {
                    query = query.Where(x => string.Equals(x.ExpenseType, "Direct Expense", StringComparison.OrdinalIgnoreCase) ||
                                            (x.ExpenseType != null && x.ExpenseType.IndexOf("Direct", StringComparison.OrdinalIgnoreCase) >= 0 && x.ExpenseType.IndexOf("Indirect", StringComparison.OrdinalIgnoreCase) < 0));
                }
                else if (categoryFilter == "INDIRECT")
                {
                    query = query.Where(x => string.Equals(x.ExpenseType, "Indirect Expense", StringComparison.OrdinalIgnoreCase) ||
                                            (x.ExpenseType != null && x.ExpenseType.IndexOf("Indirect", StringComparison.OrdinalIgnoreCase) >= 0));
                }

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x =>
                        (x.LedgerName ?? "").ToLowerInvariant().Contains(search) ||
                        (x.GroupName ?? "").ToLowerInvariant().Contains(search) ||
                        (x.ExpenseType ?? "").ToLowerInvariant().Contains(search)
                    );
                }

                _filteredLedgerSummaries = query.ToList();

                // Re-calculate percentages for the filtered view
                decimal filteredTotal = _filteredLedgerSummaries.Sum(x => x.NetAmount);
                int slNo = 1;
                foreach (var item in _filteredLedgerSummaries)
                {
                    item.SlNo = slNo++;
                    item.PercentageOfTotal = filteredTotal > 0 ? Math.Round((item.NetAmount / filteredTotal) * 100, 1) : 0;
                }

                ultraGridExpenses.DataSource = null;
                ultraGridExpenses.DataSource = _filteredLedgerSummaries;

                lblRowCount.Text = $"Total: {_filteredLedgerSummaries.Count} ledger accounts";
                UpdateFootersSummaryMode();
            }
            else
            {
                var query = _allVoucherTransactions.AsEnumerable();

                if (categoryFilter == "DIRECT")
                {
                    query = query.Where(x => string.Equals(x.ExpenseType, "Direct Expense", StringComparison.OrdinalIgnoreCase) ||
                                            (x.ExpenseType != null && x.ExpenseType.IndexOf("Direct", StringComparison.OrdinalIgnoreCase) >= 0 && x.ExpenseType.IndexOf("Indirect", StringComparison.OrdinalIgnoreCase) < 0));
                }
                else if (categoryFilter == "INDIRECT")
                {
                    query = query.Where(x => string.Equals(x.ExpenseType, "Indirect Expense", StringComparison.OrdinalIgnoreCase) ||
                                            (x.ExpenseType != null && x.ExpenseType.IndexOf("Indirect", StringComparison.OrdinalIgnoreCase) >= 0));
                }

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x =>
                        (x.LedgerName ?? "").ToLowerInvariant().Contains(search) ||
                        (x.GroupName ?? "").ToLowerInvariant().Contains(search) ||
                        (x.VoucherNumber ?? "").ToLowerInvariant().Contains(search) ||
                        (x.VoucherType ?? "").ToLowerInvariant().Contains(search) ||
                        (x.Narration ?? "").ToLowerInvariant().Contains(search) ||
                        (x.ExpenseType ?? "").ToLowerInvariant().Contains(search)
                    );
                }

                _filteredVoucherTransactions = query.ToList();

                ultraGridExpenses.DataSource = null;
                ultraGridExpenses.DataSource = _filteredVoucherTransactions;

                lblRowCount.Text = $"Total: {_filteredVoucherTransactions.Count} voucher entries";
                UpdateFootersDetailMode();
            }
        }

        private void UpdateFootersSummaryMode()
        {
            decimal directTotal = _filteredLedgerSummaries
                .Where(x => string.Equals(x.ExpenseType, "Direct Expense", StringComparison.OrdinalIgnoreCase) ||
                            (x.ExpenseType != null && x.ExpenseType.IndexOf("Direct", StringComparison.OrdinalIgnoreCase) >= 0 && x.ExpenseType.IndexOf("Indirect", StringComparison.OrdinalIgnoreCase) < 0))
                .Sum(x => x.NetAmount);

            decimal indirectTotal = _filteredLedgerSummaries
                .Where(x => string.Equals(x.ExpenseType, "Indirect Expense", StringComparison.OrdinalIgnoreCase) ||
                            (x.ExpenseType != null && x.ExpenseType.IndexOf("Indirect", StringComparison.OrdinalIgnoreCase) >= 0))
                .Sum(x => x.NetAmount);

            decimal total = _filteredLedgerSummaries.Sum(x => x.NetAmount);
            int vouchers = _filteredLedgerSummaries.Sum(x => x.VoucherCount);

            lblDirectExpensesSummary.Text = $"Direct Expenses: ₹ {directTotal:N2}";
            lblIndirectExpensesSummary.Text = $"Indirect Expenses: ₹ {indirectTotal:N2}";
            lblTotalVouchersSummary.Text = $"Vouchers: {vouchers:N0}";
            lblTotalExpensesSummary.Text = $"TOTAL EXPENSES: ₹ {total:N2}";
        }

        private void UpdateFootersDetailMode()
        {
            decimal directTotal = _filteredVoucherTransactions
                .Where(x => string.Equals(x.ExpenseType, "Direct Expense", StringComparison.OrdinalIgnoreCase) ||
                            (x.ExpenseType != null && x.ExpenseType.IndexOf("Direct", StringComparison.OrdinalIgnoreCase) >= 0 && x.ExpenseType.IndexOf("Indirect", StringComparison.OrdinalIgnoreCase) < 0))
                .Sum(x => x.NetAmount);

            decimal indirectTotal = _filteredVoucherTransactions
                .Where(x => string.Equals(x.ExpenseType, "Indirect Expense", StringComparison.OrdinalIgnoreCase) ||
                            (x.ExpenseType != null && x.ExpenseType.IndexOf("Indirect", StringComparison.OrdinalIgnoreCase) >= 0))
                .Sum(x => x.NetAmount);

            decimal total = _filteredVoucherTransactions.Sum(x => x.NetAmount);
            int vouchers = _filteredVoucherTransactions.Count;

            lblDirectExpensesSummary.Text = $"Direct Expenses: ₹ {directTotal:N2}";
            lblIndirectExpensesSummary.Text = $"Indirect Expenses: ₹ {indirectTotal:N2}";
            lblTotalVouchersSummary.Text = $"Vouchers: {vouchers:N0}";
            lblTotalExpensesSummary.Text = $"TOTAL EXPENSES: ₹ {total:N2}";
        }
        #endregion

        #region Grid Events & Custom Layout
        private void UltraGridExpenses_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0) return;

            var band = e.Layout.Bands[0];
            string viewMode = cmbViewMode.Value?.ToString() ?? "SUMMARY";

            // Configure Pinned Summary Footer
            e.Layout.Override.SummaryDisplayArea = SummaryDisplayAreas.BottomFixed | SummaryDisplayAreas.InGroupByRows;
            e.Layout.Override.SummaryFooterAppearance.BackColor = Color.FromArgb(240, 248, 255);
            e.Layout.Override.SummaryFooterAppearance.ForeColor = Color.FromArgb(18, 49, 102);
            e.Layout.Override.SummaryFooterAppearance.FontData.Bold = DefaultableBoolean.True;
            e.Layout.Override.SummaryFooterCaptionVisible = DefaultableBoolean.True;
            band.SummaryFooterCaption = "Totals:";
            band.Summaries.Clear();

            if (viewMode == "SUMMARY")
            {
                // Summary Mode Columns: SlNo, ExpenseType, GroupName, LedgerName, VoucherCount, TotalDebit, TotalCredit, NetAmount, PercentageOfTotal
                if (band.Columns.Exists("LedgerID")) band.Columns["LedgerID"].Hidden = true;

                if (band.Columns.Exists("SlNo"))
                {
                    band.Columns["SlNo"].Header.Caption = "S.No";
                    band.Columns["SlNo"].Width = 50;
                    band.Columns["SlNo"].CellAppearance.TextHAlign = HAlign.Center;
                    band.Columns["SlNo"].Header.VisiblePosition = 0;
                }

                if (band.Columns.Exists("ExpenseType"))
                {
                    band.Columns["ExpenseType"].Header.Caption = "Category";
                    band.Columns["ExpenseType"].Width = 140;
                    band.Columns["ExpenseType"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["ExpenseType"].Header.VisiblePosition = 1;
                }

                if (band.Columns.Exists("GroupName"))
                {
                    band.Columns["GroupName"].Header.Caption = "Account Group";
                    band.Columns["GroupName"].Width = 190;
                    band.Columns["GroupName"].Header.VisiblePosition = 2;
                }

                if (band.Columns.Exists("LedgerName"))
                {
                    band.Columns["LedgerName"].Header.Caption = "Expense Ledger Name";
                    band.Columns["LedgerName"].Width = 280;
                    band.Columns["LedgerName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["LedgerName"].Header.VisiblePosition = 3;
                }

                if (band.Columns.Exists("VoucherCount"))
                {
                    band.Columns["VoucherCount"].Header.Caption = "Vouchers";
                    band.Columns["VoucherCount"].Width = 85;
                    band.Columns["VoucherCount"].CellAppearance.TextHAlign = HAlign.Right;
                    band.Columns["VoucherCount"].Format = "#,##0";
                    band.Columns["VoucherCount"].Header.VisiblePosition = 4;

                    var sumVouchers = band.Summaries.Add("SumVouchers", SummaryType.Sum, band.Columns["VoucherCount"], SummaryPosition.UseSummaryPositionColumn);
                    sumVouchers.DisplayFormat = "{0:N0}";
                    sumVouchers.Appearance.TextHAlign = HAlign.Right;
                    sumVouchers.Appearance.FontData.Bold = DefaultableBoolean.True;
                }

                if (band.Columns.Exists("TotalDebit"))
                {
                    band.Columns["TotalDebit"].Header.Caption = "Debit (₹)";
                    band.Columns["TotalDebit"].Width = 125;
                    band.Columns["TotalDebit"].CellAppearance.TextHAlign = HAlign.Right;
                    band.Columns["TotalDebit"].Format = "N2";
                    band.Columns["TotalDebit"].Header.VisiblePosition = 5;

                    var sumDebit = band.Summaries.Add("SumTotalDebit", SummaryType.Sum, band.Columns["TotalDebit"], SummaryPosition.UseSummaryPositionColumn);
                    sumDebit.DisplayFormat = "₹ {0:N2}";
                    sumDebit.Appearance.TextHAlign = HAlign.Right;
                    sumDebit.Appearance.FontData.Bold = DefaultableBoolean.True;
                }

                if (band.Columns.Exists("TotalCredit"))
                {
                    band.Columns["TotalCredit"].Header.Caption = "Credit (₹)";
                    band.Columns["TotalCredit"].Width = 125;
                    band.Columns["TotalCredit"].CellAppearance.TextHAlign = HAlign.Right;
                    band.Columns["TotalCredit"].Format = "N2";
                    band.Columns["TotalCredit"].Header.VisiblePosition = 6;

                    var sumCredit = band.Summaries.Add("SumTotalCredit", SummaryType.Sum, band.Columns["TotalCredit"], SummaryPosition.UseSummaryPositionColumn);
                    sumCredit.DisplayFormat = "₹ {0:N2}";
                    sumCredit.Appearance.TextHAlign = HAlign.Right;
                    sumCredit.Appearance.FontData.Bold = DefaultableBoolean.True;
                }

                if (band.Columns.Exists("NetAmount"))
                {
                    band.Columns["NetAmount"].Header.Caption = "Net Expense (₹)";
                    band.Columns["NetAmount"].Width = 150;
                    band.Columns["NetAmount"].CellAppearance.TextHAlign = HAlign.Right;
                    band.Columns["NetAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["NetAmount"].CellAppearance.ForeColor = NetAmountColor;
                    band.Columns["NetAmount"].Format = "N2";
                    band.Columns["NetAmount"].Header.VisiblePosition = 7;

                    var sumNet = band.Summaries.Add("SumNetAmount", SummaryType.Sum, band.Columns["NetAmount"], SummaryPosition.UseSummaryPositionColumn);
                    sumNet.DisplayFormat = "₹ {0:N2}";
                    sumNet.Appearance.TextHAlign = HAlign.Right;
                    sumNet.Appearance.FontData.Bold = DefaultableBoolean.True;
                    sumNet.Appearance.ForeColor = NetAmountColor;
                }

                if (band.Columns.Exists("PercentageOfTotal"))
                {
                    band.Columns["PercentageOfTotal"].Header.Caption = "Share %";
                    band.Columns["PercentageOfTotal"].Width = 95;
                    band.Columns["PercentageOfTotal"].CellAppearance.TextHAlign = HAlign.Right;
                    band.Columns["PercentageOfTotal"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["PercentageOfTotal"].Format = "0.0'%'";
                    band.Columns["PercentageOfTotal"].Header.VisiblePosition = 8;
                }
            }
            else
            {
                // Detail Mode Columns: VoucherID, VoucherDate, VoucherNumber, VoucherType, LedgerName, GroupName, ExpenseType, Debit, Credit, NetAmount, Narration
                if (band.Columns.Exists("VoucherID")) band.Columns["VoucherID"].Hidden = true;

                if (band.Columns.Exists("VoucherDate"))
                {
                    band.Columns["VoucherDate"].Header.Caption = "Date";
                    band.Columns["VoucherDate"].Width = 95;
                    band.Columns["VoucherDate"].CellAppearance.TextHAlign = HAlign.Center;
                    band.Columns["VoucherDate"].Format = "dd-MM-yyyy";
                    band.Columns["VoucherDate"].Header.VisiblePosition = 0;
                }

                if (band.Columns.Exists("VoucherNumber"))
                {
                    band.Columns["VoucherNumber"].Header.Caption = "Voucher No";
                    band.Columns["VoucherNumber"].Width = 120;
                    band.Columns["VoucherNumber"].Header.VisiblePosition = 1;
                }

                if (band.Columns.Exists("VoucherType"))
                {
                    band.Columns["VoucherType"].Header.Caption = "Type";
                    band.Columns["VoucherType"].Width = 110;
                    band.Columns["VoucherType"].Header.VisiblePosition = 2;
                }

                if (band.Columns.Exists("ExpenseType"))
                {
                    band.Columns["ExpenseType"].Header.Caption = "Category";
                    band.Columns["ExpenseType"].Width = 130;
                    band.Columns["ExpenseType"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["ExpenseType"].Header.VisiblePosition = 3;
                }

                if (band.Columns.Exists("LedgerName"))
                {
                    band.Columns["LedgerName"].Header.Caption = "Expense Ledger";
                    band.Columns["LedgerName"].Width = 230;
                    band.Columns["LedgerName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["LedgerName"].Header.VisiblePosition = 4;
                }

                if (band.Columns.Exists("GroupName"))
                {
                    band.Columns["GroupName"].Header.Caption = "Account Group";
                    band.Columns["GroupName"].Width = 160;
                    band.Columns["GroupName"].Header.VisiblePosition = 5;
                }

                if (band.Columns.Exists("Debit"))
                {
                    band.Columns["Debit"].Header.Caption = "Debit (₹)";
                    band.Columns["Debit"].Width = 115;
                    band.Columns["Debit"].CellAppearance.TextHAlign = HAlign.Right;
                    band.Columns["Debit"].Format = "N2";
                    band.Columns["Debit"].Header.VisiblePosition = 6;

                    var sumDebit = band.Summaries.Add("SumDebit", SummaryType.Sum, band.Columns["Debit"], SummaryPosition.UseSummaryPositionColumn);
                    sumDebit.DisplayFormat = "₹ {0:N2}";
                    sumDebit.Appearance.TextHAlign = HAlign.Right;
                    sumDebit.Appearance.FontData.Bold = DefaultableBoolean.True;
                }

                if (band.Columns.Exists("Credit"))
                {
                    band.Columns["Credit"].Header.Caption = "Credit (₹)";
                    band.Columns["Credit"].Width = 115;
                    band.Columns["Credit"].CellAppearance.TextHAlign = HAlign.Right;
                    band.Columns["Credit"].Format = "N2";
                    band.Columns["Credit"].Header.VisiblePosition = 7;

                    var sumCredit = band.Summaries.Add("SumCredit", SummaryType.Sum, band.Columns["Credit"], SummaryPosition.UseSummaryPositionColumn);
                    sumCredit.DisplayFormat = "₹ {0:N2}";
                    sumCredit.Appearance.TextHAlign = HAlign.Right;
                    sumCredit.Appearance.FontData.Bold = DefaultableBoolean.True;
                }

                if (band.Columns.Exists("NetAmount"))
                {
                    band.Columns["NetAmount"].Header.Caption = "Net (₹)";
                    band.Columns["NetAmount"].Width = 125;
                    band.Columns["NetAmount"].CellAppearance.TextHAlign = HAlign.Right;
                    band.Columns["NetAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["NetAmount"].CellAppearance.ForeColor = NetAmountColor;
                    band.Columns["NetAmount"].Format = "N2";
                    band.Columns["NetAmount"].Header.VisiblePosition = 8;

                    var sumNet = band.Summaries.Add("SumDetailNet", SummaryType.Sum, band.Columns["NetAmount"], SummaryPosition.UseSummaryPositionColumn);
                    sumNet.DisplayFormat = "₹ {0:N2}";
                    sumNet.Appearance.TextHAlign = HAlign.Right;
                    sumNet.Appearance.FontData.Bold = DefaultableBoolean.True;
                    sumNet.Appearance.ForeColor = NetAmountColor;
                }

                if (band.Columns.Exists("Narration"))
                {
                    band.Columns["Narration"].Header.Caption = "Narration / Description";
                    band.Columns["Narration"].Width = 320;
                    band.Columns["Narration"].Header.VisiblePosition = 9;
                }
            }
        }

        private void UltraGridExpenses_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (e.Row.Cells.Exists("ExpenseType"))
            {
                string expType = e.Row.Cells["ExpenseType"].Value?.ToString() ?? "";
                if (expType.IndexOf("Direct", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    e.Row.Cells["ExpenseType"].Appearance.ForeColor = DirectExpenseColor;
                }
                else
                {
                    e.Row.Cells["ExpenseType"].Appearance.ForeColor = IndirectExpenseColor;
                }
            }

            // Visual Alert for Credit Balances / Refunds / Reversals
            if (e.Row.Cells.Exists("NetAmount"))
            {
                object netVal = e.Row.Cells["NetAmount"].Value;
                if (netVal != null && decimal.TryParse(netVal.ToString(), out decimal net) && net < 0)
                {
                    e.Row.Appearance.BackColor = Color.FromArgb(254, 243, 199); // Soft amber alert
                    e.Row.Cells["NetAmount"].Appearance.ForeColor = Color.FromArgb(180, 83, 9);
                    e.Row.ToolTipText = "Credit Balance / Refund / Reversal entry";
                }
            }
        }

        private void UltraGridExpenses_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            if (e.Row == null || !e.Row.IsDataRow) return;

            string viewMode = cmbViewMode.Value?.ToString() ?? "SUMMARY";
            if (viewMode == "SUMMARY")
            {
                // If user double clicks on a ledger row in summary mode, drill into detail mode for that specific ledger!
                string ledgerName = e.Row.Cells["LedgerName"].Value?.ToString() ?? "";
                if (!string.IsNullOrEmpty(ledgerName))
                {
                    cmbViewMode.Value = "DETAIL";
                    txtSearch.Text = ledgerName;
                }
            }
            else
            {
                // Detail Mode: Drill down to open the voucher document
                DrillDownToVoucher(e.Row);
            }
        }

        private void DrillDownToVoucher(UltraGridRow row)
        {
            try
            {
                string voucherNo = row.Cells.Exists("VoucherNumber") ? row.Cells["VoucherNumber"].Value?.ToString() ?? "" : "";
                string voucherType = row.Cells.Exists("VoucherType") ? row.Cells["VoucherType"].Value?.ToString() ?? "" : "";
                long voucherId = row.Cells.Exists("VoucherID") ? Convert.ToInt64(row.Cells["VoucherID"].Value ?? 0) : 0;

                if (string.IsNullOrWhiteSpace(voucherNo) && voucherId <= 0)
                {
                    return;
                }

                // Identify voucher category and open appropriate entry form
                if (voucherType.IndexOf("Payment", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    voucherType.IndexOf("GENPAY", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var paymentForm = new FrmGeneralPayment();
                    paymentForm.LoadVoucherByNumber(voucherNo);
                    OpenFormInTabOrModal(paymentForm, $"Payment #{voucherNo}");
                }
                else if (voucherType.IndexOf("Journal", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var journalForm = new FrmJournal();
                    journalForm.LoadVoucherByNumber(voucherNo);
                    OpenFormInTabOrModal(journalForm, $"Journal #{voucherNo}");
                }
                else if (voucherType.IndexOf("Receipt", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         voucherType.IndexOf("GENREC", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var receiptForm = new FrmGeneralReceipt();
                    receiptForm.LoadVoucherByNumber(voucherNo);
                    OpenFormInTabOrModal(receiptForm, $"Receipt #{voucherNo}");
                }
                else
                {
                    // For Contra, Sales, or custom system vouchers, show voucher info
                    MessageBox.Show(
                        $"Voucher Type: {voucherType}\nVoucher No: {voucherNo}\n\nThis voucher was generated by the '{voucherType}' module.",
                        "Voucher Information",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening voucher: {ex.Message}", "Drill-Down Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenFormInTabOrModal(Form form, string title)
        {
            try
            {
                var homeForm = Application.OpenForms.OfType<Home>().FirstOrDefault();
                if (homeForm != null)
                {
                    var openFormInTabMethod = homeForm.GetType().GetMethod("OpenFormInTabSafe",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?? homeForm.GetType().GetMethod("OpenFormInTab",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (openFormInTabMethod != null)
                    {
                        openFormInTabMethod.Invoke(homeForm, new object[] { form, title });
                        return;
                    }
                }
                form.Show();
                form.BringToFront();
            }
            catch
            {
                form.Show();
                form.BringToFront();
            }
        }
        #endregion

        #region Actions (Export, Print, Reset, Toggle)
        private void ExportCsv()
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv",
                    FileName = $"Business_Expenses_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    StringBuilder sb = new StringBuilder();
                    string viewMode = cmbViewMode.Value?.ToString() ?? "SUMMARY";

                    sb.AppendLine($"Business Expenses Report ({viewMode})");
                    sb.AppendLine($"Period: {dtFromDate.DateTime:dd-MM-yyyy} to {dtToDate.DateTime:dd-MM-yyyy}");
                    sb.AppendLine();

                    if (viewMode == "SUMMARY")
                    {
                        sb.AppendLine("S.No,Category,Account Group,Expense Ledger Name,Vouchers,Debit,Credit,Net Expense,Share %");
                        foreach (var item in _filteredLedgerSummaries)
                        {
                            sb.AppendLine($"{item.SlNo},\"{EscapeCsv(item.ExpenseType)}\",\"{EscapeCsv(item.GroupName)}\",\"{EscapeCsv(item.LedgerName)}\",{item.VoucherCount},{item.TotalDebit:F2},{item.TotalCredit:F2},{item.NetAmount:F2},{item.PercentageOfTotal:F1}%");
                        }
                        sb.AppendLine();
                        sb.AppendLine($"TOTAL,,,,{_filteredLedgerSummaries.Sum(x => x.VoucherCount)},{_filteredLedgerSummaries.Sum(x => x.TotalDebit):F2},{_filteredLedgerSummaries.Sum(x => x.TotalCredit):F2},{_filteredLedgerSummaries.Sum(x => x.NetAmount):F2},100%");
                    }
                    else
                    {
                        sb.AppendLine("Date,Voucher No,Type,Category,Expense Ledger,Account Group,Debit,Credit,Net,Narration");
                        foreach (var item in _filteredVoucherTransactions)
                        {
                            sb.AppendLine($"{item.VoucherDate:dd-MM-yyyy},\"{EscapeCsv(item.VoucherNumber)}\",\"{EscapeCsv(item.VoucherType)}\",\"{EscapeCsv(item.ExpenseType)}\",\"{EscapeCsv(item.LedgerName)}\",\"{EscapeCsv(item.GroupName)}\",{item.Debit:F2},{item.Credit:F2},{item.NetAmount:F2},\"{EscapeCsv(item.Narration)}\"");
                        }
                        sb.AppendLine();
                        sb.AppendLine($"TOTAL,,,,,,{_filteredVoucherTransactions.Sum(x => x.Debit):F2},{_filteredVoucherTransactions.Sum(x => x.Credit):F2},{_filteredVoucherTransactions.Sum(x => x.NetAmount):F2},");
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Business expenses report exported successfully to CSV.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting CSV: " + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportExcel()
        {
            // For fast and standard Excel compatibility, export structured CSV/TSV format that opens natively in Excel
            ExportCsv();
        }

        private static string EscapeCsv(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Replace("\"", "\"\"");
        }

        private void PreviewGrid()
        {
            try
            {
                using (var printDoc = new Infragistics.Win.UltraWinGrid.UltraGridPrintDocument())
                {
                    printDoc.Grid = ultraGridExpenses;
                    printDoc.Header.TextLeft = "Business Expenses Report";
                    printDoc.Header.TextRight = $"Period: {dtFromDate.DateTime:dd-MM-yyyy} to {dtToDate.DateTime:dd-MM-yyyy}";
                    printDoc.Footer.TextCenter = "Page [Page #] of [Total Pages]";

                    using (var previewDialog = new PrintPreviewDialog())
                    {
                        previewDialog.Document = printDoc;
                        previewDialog.WindowState = FormWindowState.Maximized;
                        previewDialog.ShowDialog(this);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Print preview error: " + ex.Message, "Preview Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintReport()
        {
            PreviewGrid();
        }

        private void ResetFilters()
        {
            txtSearch.Text = string.Empty;
            cmbCategory.Value = "ALL";
            cmbViewMode.Value = "SUMMARY";
            cmbDateQuickSelect.Value = "THIS_FIN_YEAR";
            UpdateDateRangeForPreset("THIS_FIN_YEAR");
            LoadData();
        }

        private void ToggleSelectionPanel()
        {
            _isSelectionHidden = !_isSelectionHidden;
            ultraPanelControls.Visible = !_isSelectionHidden;
            btnToggleSelection.Text = _isSelectionHidden ? "Show Selection" : "Hide Selection";
        }

        private void FrmBusinessExpenseReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                LoadData();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
                e.Handled = true;
            }
        }
        #endregion
    }
}
