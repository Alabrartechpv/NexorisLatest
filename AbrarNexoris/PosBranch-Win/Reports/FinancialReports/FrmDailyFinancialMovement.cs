using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository.ReportRepository;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class FrmDailyFinancialMovement : Form
    {
        #region Styling Constants (Identical to FrmBalanceSheet)
        private static readonly Color FormBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color FilterPanelBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color BorderBlue = Color.FromArgb(118, 154, 198);
        private static readonly Color ControlTextColor = Color.FromArgb(18, 49, 102);
        private static readonly Color GridHeaderBlue = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue = Color.FromArgb(126, 126, 245);
        private static readonly Color GridRowLine = Color.FromArgb(197, 217, 241);
        private static readonly Color GridAltRow = Color.FromArgb(246, 250, 255);
        private static readonly Color ButtonTextBlue = Color.FromArgb(14, 47, 108);

        private static readonly Color DebitGreen = Color.FromArgb(27, 94, 32);
        private static readonly Color CreditRed = Color.FromArgb(198, 40, 40);
        #endregion

        private readonly DailyFinancialMovementRepository _repository;
        private DailyFinancialMovementReport _currentReport;
        private bool _isPopulatingCombo = false;
        private bool _isDaywiseView = false;

        public FrmDailyFinancialMovement()
        {
            InitializeComponent();

            _repository = new DailyFinancialMovementRepository();
            _currentReport = new DailyFinancialMovementReport();

            SetupGridTheme(ultraGridLiabilities);
            SetupGridTheme(ultraGridAssets);
            SetupGridTheme(ultraGridDaywise);

            this.Load += FrmDailyFinancialMovement_Load;
            this.KeyPreview = true;
            this.KeyDown += FrmDailyFinancialMovement_KeyDown;

            btnGenerate.Click += (s, e) => LoadData();
            btnToggleView.Click += (s, e) => ToggleViewMode();
            btnToday.Click += (s, e) => SetPreset(DateTime.Today, DateTime.Today);
            btnYesterday.Click += (s, e) => SetPreset(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(-1));
            btnLast7Days.Click += (s, e) => SetPreset(DateTime.Today.AddDays(-6), DateTime.Today);
            btnThisMonth.Click += (s, e) => SetPreset(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), DateTime.Today);

            cmbDaySelect.SelectedIndexChanged += CmbDaySelect_SelectedIndexChanged;

            btnExportCsv.Click += (s, e) => ExportCsv();
            btnPrint.Click += (s, e) => PrintGrids();

            ultraGridLiabilities.DoubleClickRow += UltraGrid_DoubleClickRow;
            ultraGridAssets.DoubleClickRow += UltraGrid_DoubleClickRow;
            ultraGridDaywise.DoubleClickRow += UltraGridDaywise_DoubleClickRow;
            ultraGridDaywise.InitializeRow += UltraGridDaywise_InitializeRow;
        }

        private void SetupGridTheme(UltraGrid grid)
        {
            grid.UseOsThemes = DefaultableBoolean.False;
            grid.DisplayLayout.Reset();

            grid.DisplayLayout.Override.HeaderPlacement = HeaderPlacement.OncePerRowIsland;
            grid.DisplayLayout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            grid.DisplayLayout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            grid.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            grid.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            grid.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            grid.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9f;
            grid.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;

            grid.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            grid.DisplayLayout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            grid.DisplayLayout.Override.CellAppearance.BorderColor = GridRowLine;
            grid.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            grid.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;

            grid.DisplayLayout.Override.MinRowHeight = 26;
            grid.DisplayLayout.Override.DefaultRowHeight = 26;

            grid.DisplayLayout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            grid.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
            grid.DisplayLayout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.True;

            grid.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            grid.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            grid.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
            grid.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            grid.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
            grid.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            grid.DisplayLayout.GroupByBox.Hidden = true;
        }

        private void FrmDailyFinancialMovement_Load(object sender, EventArgs e)
        {
            dtFromDate.DateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtToDate.DateTime = DateTime.Today;

            LoadData();
        }

        private void FrmDailyFinancialMovement_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                LoadData();
                e.Handled = true;
            }
        }

        private void SetPreset(DateTime from, DateTime to)
        {
            dtFromDate.DateTime = from;
            dtToDate.DateTime = to;
            LoadData();
        }

        public void LoadData()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                Application.DoEvents();

                int branchId = 1;
                try
                {
                    int sessBranch = SessionContext.BranchId;
                    branchId = sessBranch > 0 ? sessBranch : Convert.ToInt32(DataBase.BranchId);
                }
                catch { branchId = 1; }
                if (branchId <= 0) branchId = 1;

                var filter = new DailyFinancialMovementFilter
                {
                    CompanyId = 1,
                    BranchId = branchId,
                    FinYearId = 1,
                    FromDate = dtFromDate.DateTime.Date,
                    ToDate = dtToDate.DateTime.Date,
                    DrillDownCategory = "ALL"
                };

                _currentReport = _repository.GetReport(filter);

                PopulateDayCombo();
                DisplayCurrentSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Balance Sheet Movement:\n" + ex.Message,
                    "Balance Sheet Movement Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void PopulateDayCombo()
        {
            _isPopulatingCombo = true;
            cmbDaySelect.Items.Clear();

            cmbDaySelect.Items.Add("Whole Period (Summary)");

            if (_currentReport != null && _currentReport.DayRows.Count > 0)
            {
                for (int i = _currentReport.DayRows.Count - 1; i >= 0; i--)
                {
                    var r = _currentReport.DayRows[i];
                    cmbDaySelect.Items.Add($"{r.TxDate:dd-MM-yyyy} ({r.DayName})");
                }
            }

            cmbDaySelect.SelectedIndex = 0;
            _isPopulatingCombo = false;
        }

        private void CmbDaySelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isPopulatingCombo) return;
            DisplayCurrentSelection();
        }

        private void DisplayCurrentSelection()
        {
            if (_currentReport == null) return;

            if (cmbDaySelect.SelectedIndex <= 0 || _currentReport.DayRows.Count == 0)
            {
                DisplayPeriodSummary();
            }
            else
            {
                int dayIndex = (_currentReport.DayRows.Count - 1) - (cmbDaySelect.SelectedIndex - 1);
                if (dayIndex >= 0 && dayIndex < _currentReport.DayRows.Count)
                {
                    DisplaySingleDay(_currentReport.DayRows[dayIndex]);
                }
            }
        }

        private void DisplayPeriodSummary()
        {
            if (_currentReport.DayRows.Count == 0) return;

            var firstDay = _currentReport.DayRows.First();
            var lastDay = _currentReport.DayRows.Last();

            decimal totalCashIn = _currentReport.Summary.PeriodCashInflow;
            decimal totalCashOut = _currentReport.Summary.PeriodCashOutflow;

            decimal totalBankIn = _currentReport.DayRows.Sum(d => d.BankInflow);
            decimal totalBankOut = _currentReport.DayRows.Sum(d => d.BankOutflow);

            decimal totalStockIn = _currentReport.Summary.PeriodStockInwardCost;
            decimal totalStockOut = _currentReport.Summary.PeriodStockOutwardCost;

            decimal totalCreditSales = _currentReport.Summary.PeriodCreditSales;
            decimal totalDebtorsColl = _currentReport.Summary.PeriodDebtorsCollected;

            decimal totalCreditPurchases = _currentReport.Summary.PeriodCreditPurchases;
            decimal totalCreditorsPaid = _currentReport.Summary.PeriodCreditorsPaid;

            decimal totalLoansDrawn = _currentReport.DayRows.Sum(d => d.LoansClosing - d.LoansOpening > 0 ? d.LoansClosing - d.LoansOpening : 0);
            decimal totalLoansRepaid = _currentReport.DayRows.Sum(d => d.LoansOpening - d.LoansClosing > 0 ? d.LoansOpening - d.LoansClosing : 0);

            // Bind Liabilities Table (from full GroupRows if available, or fallback)
            DataTable dtLiab = CreateBalanceSheetTable();
            var liabGroups = _currentReport.GroupRows?.Where(g => g.Side == "LIABILITIES").ToList();
            if (liabGroups != null && liabGroups.Count > 0)
            {
                foreach (var g in liabGroups)
                {
                    string categoryCode = g.GroupID == 17 ? "CREDITORS" : (g.GroupID >= 25 && g.GroupID <= 27 ? "LOANS" : "LIAB_" + g.GroupID);
                    AddBalanceSheetRow(dtLiab, g.GroupName, g.OpeningBal, g.CreditTotal, g.DebitTotal, g.ClosingBal, categoryCode);
                }
            }
            else
            {
                AddBalanceSheetRow(dtLiab, "Sundry Creditors (Supplier / Vendor Udhaar)", firstDay.CreditorsOpening, totalCreditPurchases, totalCreditorsPaid, lastDay.CreditorsClosing, "CREDITORS");
                AddBalanceSheetRow(dtLiab, "Bank Overdraft & Loans", firstDay.LoansOpening, totalLoansDrawn, totalLoansRepaid, lastDay.LoansClosing, "LOANS");
            }
            BindSideGrid(ultraGridLiabilities, dtLiab, isAsset: false);

            // Bind Assets Table (from full GroupRows if available, or fallback)
            DataTable dtAssets = CreateBalanceSheetTable();
            var assetGroups = _currentReport.GroupRows?.Where(g => g.Side == "ASSETS").ToList();
            if (assetGroups != null && assetGroups.Count > 0)
            {
                foreach (var g in assetGroups)
                {
                    string categoryCode = g.GroupID == 14 ? "CASH" : (g.GroupID == 15 ? "BANK" : (g.GroupID == 16 ? "DEBTORS" : (g.GroupID == 18 ? "STOCK" : "ASSET_" + g.GroupID)));
                    AddBalanceSheetRow(dtAssets, g.GroupName, g.OpeningBal, g.DebitTotal, g.CreditTotal, g.ClosingBal, categoryCode);
                }
            }
            else
            {
                AddBalanceSheetRow(dtAssets, "Cash in Hand (Counter Drawers & Safe)", firstDay.CashOpening, totalCashIn, totalCashOut, lastDay.CashClosing, "CASH");
                AddBalanceSheetRow(dtAssets, "Bank Accounts & UPI / Card Machines", firstDay.BankOpening, totalBankIn, totalBankOut, lastDay.BankClosing, "BANK");
                AddBalanceSheetRow(dtAssets, "Stock on Hand (Shelf & Godown Valuation)", firstDay.StockOpening, totalStockIn, totalStockOut, lastDay.StockClosing, "STOCK");
                AddBalanceSheetRow(dtAssets, "Sundry Debtors (Customer Udhaar Book)", firstDay.DebtorsOpening, totalCreditSales, totalDebtorsColl, lastDay.DebtorsClosing, "DEBTORS");
            }
            BindSideGrid(ultraGridAssets, dtAssets, isAsset: true);

            // Sub-Totals inside containers
            lblLiabTotalValue.Text = "₹ " + lastDay.TotalLiabilities.ToString("N2");
            lblAssetsTotalValue.Text = "₹ " + lastDay.TotalAssets.ToString("N2");

            // Bottom Footer
            lblTotalLiabilities.Text = "Total Liabilities: ₹ " + lastDay.TotalLiabilities.ToString("N2");
            lblTotalAssets.Text = "Total Assets: ₹ " + lastDay.TotalAssets.ToString("N2");

            string sign = _currentReport.Summary.PeriodNetWorthGrowth >= 0 ? "+" : "";
            string assetSign = _currentReport.Summary.PeriodAssetGrowth >= 0 ? "+" : "";
            lblNetPositionBadge.Text = $"NET STORE WORTH: ₹ {lastDay.NetStorePosition:N2} (GAIN: {sign}₹ {_currentReport.Summary.PeriodNetWorthGrowth:N2}) | ASSET GROWTH: {assetSign}₹ {_currentReport.Summary.PeriodAssetGrowth:N2} ({assetSign}{_currentReport.Summary.PeriodAssetGrowthPct:N2}%)";

            // Update Day-wise Grid data
            BindDaywiseGrid();
        }

        private void DisplaySingleDay(DailyFinancialMovementDayRow day)
        {
            if (day == null) return;

            // Bind Liabilities Table
            DataTable dtLiab = CreateBalanceSheetTable();
            AddBalanceSheetRow(dtLiab, "Sundry Creditors (Supplier / Vendor Udhaar)", day.CreditorsOpening, day.CreditorsPurchases, day.CreditorsPaid, day.CreditorsClosing, "CREDITORS");
            AddBalanceSheetRow(dtLiab, "Bank Overdraft & Loans", day.LoansOpening, 0, 0, day.LoansClosing, "LOANS");
            BindSideGrid(ultraGridLiabilities, dtLiab, isAsset: false);

            // Bind Assets Table
            DataTable dtAssets = CreateBalanceSheetTable();
            AddBalanceSheetRow(dtAssets, "Cash in Hand (Counter Drawers & Safe)", day.CashOpening, day.CashInflow, day.CashOutflow, day.CashClosing, "CASH");
            AddBalanceSheetRow(dtAssets, "Bank Accounts & UPI / Card Machines", day.BankOpening, day.BankInflow, day.BankOutflow, day.BankClosing, "BANK");
            AddBalanceSheetRow(dtAssets, "Stock on Hand (Shelf & Godown Valuation)", day.StockOpening, day.StockInflow, day.StockOutflow, day.StockClosing, "STOCK");
            AddBalanceSheetRow(dtAssets, "Sundry Debtors (Customer Udhaar Book)", day.DebtorsOpening, day.DebtorsCreditSales, day.DebtorsCollected, day.DebtorsClosing, "DEBTORS");
            BindSideGrid(ultraGridAssets, dtAssets, isAsset: true);

            // Sub-Totals inside containers
            lblLiabTotalValue.Text = "₹ " + day.TotalLiabilities.ToString("N2");
            lblAssetsTotalValue.Text = "₹ " + day.TotalAssets.ToString("N2");

            // Bottom Footer
            lblTotalLiabilities.Text = "Total Liabilities: ₹ " + day.TotalLiabilities.ToString("N2");
            lblTotalAssets.Text = "Total Assets: ₹ " + day.TotalAssets.ToString("N2");

            string sign = day.DailyNetChange >= 0 ? "+" : "";
            string assetSign = day.DailyAssetChange >= 0 ? "+" : "";
            lblNetPositionBadge.Text = $"NET STORE WORTH: ₹ {day.NetStorePosition:N2} (DAY GAIN: {sign}₹ {day.DailyNetChange:N2}) | ASSET PROGRESSION: {assetSign}₹ {day.DailyAssetChange:N2} ({assetSign}{day.DailyAssetChangePct:N2}%)";
        }

        private void ToggleViewMode()
        {
            _isDaywiseView = !_isDaywiseView;

            if (_isDaywiseView)
            {
                ultraGridDaywise.Visible = true;
                ultraGridDaywise.BringToFront();
                splitContainerMain.Visible = false;
                btnToggleView.Text = "Balance Sheet View";
                lblDaySelect.Visible = false;
                cmbDaySelect.Visible = false;
                BindDaywiseGrid();
            }
            else
            {
                ultraGridDaywise.Visible = false;
                splitContainerMain.Visible = true;
                splitContainerMain.BringToFront();
                btnToggleView.Text = "Day-wise Timeline";
                lblDaySelect.Visible = true;
                cmbDaySelect.Visible = true;
                DisplayCurrentSelection();
            }
        }

        private void BindDaywiseGrid()
        {
            if (_currentReport == null || _currentReport.DayRows == null || _currentReport.DayRows.Count == 0)
            {
                ultraGridDaywise.DataSource = null;
                return;
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("TxDate", typeof(DateTime));
            dt.Columns.Add("DayName", typeof(string));
            dt.Columns.Add("AssetOpening", typeof(decimal));
            dt.Columns.Add("AssetAdditions", typeof(decimal));
            dt.Columns.Add("AssetReductions", typeof(decimal));
            dt.Columns.Add("TotalAssets", typeof(decimal));
            dt.Columns.Add("DailyAssetChange", typeof(decimal));
            dt.Columns.Add("DailyAssetChangePct", typeof(decimal));
            dt.Columns.Add("LiabOpening", typeof(decimal));
            dt.Columns.Add("TotalLiabilities", typeof(decimal));
            dt.Columns.Add("DailyLiabChange", typeof(decimal));
            dt.Columns.Add("NetStorePosition", typeof(decimal));
            dt.Columns.Add("DailyNetChange", typeof(decimal));

            foreach (var r in _currentReport.DayRows)
            {
                dt.Rows.Add(
                    r.TxDate,
                    r.DayName,
                    r.AssetOpening,
                    r.AssetAdditions,
                    r.AssetReductions,
                    r.TotalAssets,
                    r.DailyAssetChange,
                    r.DailyAssetChangePct,
                    r.LiabOpening,
                    r.TotalLiabilities,
                    r.DailyLiabChange,
                    r.NetStorePosition,
                    r.DailyNetChange
                );
            }

            ultraGridDaywise.DataSource = dt;
            if (ultraGridDaywise.DisplayLayout.Bands.Count > 0)
            {
                var b = ultraGridDaywise.DisplayLayout.Bands[0];
                b.ColHeadersVisible = true;
                ultraGridDaywise.DisplayLayout.Override.HeaderPlacement = HeaderPlacement.OncePerRowIsland;

                FormatDaywiseCol(b, "TxDate", "Date", 90, isDate: true);
                FormatDaywiseCol(b, "DayName", "Day", 70);
                FormatDaywiseCol(b, "AssetOpening", "Asset Opn (12 AM)", 125);
                FormatDaywiseCol(b, "AssetAdditions", "Asset In (+)", 105);
                FormatDaywiseCol(b, "AssetReductions", "Asset Out (-)", 105);
                FormatDaywiseCol(b, "TotalAssets", "Asset Close (12 AM)", 135, isBold: true, foreColor: DebitGreen);
                FormatDaywiseCol(b, "DailyAssetChange", "Asset Growth (₹)", 115, isBold: true, format: "+#,##0.00;-#,##0.00;0.00");
                FormatDaywiseCol(b, "DailyAssetChangePct", "Asset Growth %", 95, isBold: true, format: "+0.00%;-0.00%;0.00%");
                FormatDaywiseCol(b, "LiabOpening", "Liab Opn (12 AM)", 125);
                FormatDaywiseCol(b, "TotalLiabilities", "Liab Close (12 AM)", 135, isBold: true, foreColor: CreditRed);
                FormatDaywiseCol(b, "DailyLiabChange", "Liab Change", 105, isBold: true, format: "+#,##0.00;-#,##0.00;0.00");
                FormatDaywiseCol(b, "NetStorePosition", "Net Store Worth", 125, isBold: true);
                FormatDaywiseCol(b, "DailyNetChange", "Day Net Gain", 110, isBold: true, format: "+#,##0.00;-#,##0.00;0.00");
            }
        }

        private void FormatDaywiseCol(UltraGridBand band, string colKey, string caption, int width, bool isBold = false, Color? foreColor = null, bool isDate = false, string format = "#,##0.00")
        {
            if (band.Columns.Exists(colKey))
            {
                var col = band.Columns[colKey];
                col.Header.Caption = caption;
                col.Width = width;
                col.Header.Appearance.BackColor = GridHeaderBlue;
                col.Header.Appearance.BackColor2 = GridHeaderBlueDark;
                col.Header.Appearance.BackGradientStyle = GradientStyle.Vertical;
                col.Header.Appearance.ForeColor = Color.White;
                col.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
                col.Header.Appearance.TextHAlign = HAlign.Center;

                if (isDate)
                {
                    col.Format = "dd-MM-yyyy";
                    col.CellAppearance.TextHAlign = HAlign.Center;
                }
                else if (col.DataType == typeof(decimal))
                {
                    col.Format = format;
                    col.CellAppearance.TextHAlign = HAlign.Right;
                }

                if (isBold) col.CellAppearance.FontData.Bold = DefaultableBoolean.True;
                if (foreColor.HasValue) col.CellAppearance.ForeColor = foreColor.Value;
            }
        }

        private void UltraGridDaywise_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (e.Row == null || !e.Row.IsDataRow) return;

            if (e.Row.Cells.Exists("DailyAssetChange"))
            {
                decimal change = Convert.ToDecimal(e.Row.Cells["DailyAssetChange"].Value ?? 0);
                if (change > 0)
                {
                    e.Row.Cells["DailyAssetChange"].Appearance.ForeColor = DebitGreen;
                    if (e.Row.Cells.Exists("DailyAssetChangePct")) e.Row.Cells["DailyAssetChangePct"].Appearance.ForeColor = DebitGreen;
                }
                else if (change < 0)
                {
                    e.Row.Cells["DailyAssetChange"].Appearance.ForeColor = CreditRed;
                    if (e.Row.Cells.Exists("DailyAssetChangePct")) e.Row.Cells["DailyAssetChangePct"].Appearance.ForeColor = CreditRed;
                }
            }

            if (e.Row.Cells.Exists("DailyNetChange"))
            {
                decimal netChange = Convert.ToDecimal(e.Row.Cells["DailyNetChange"].Value ?? 0);
                e.Row.Cells["DailyNetChange"].Appearance.ForeColor = netChange >= 0 ? DebitGreen : CreditRed;
            }
        }

        private void UltraGridDaywise_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            if (e.Row == null || !e.Row.IsDataRow) return;
            DateTime targetDate = Convert.ToDateTime(e.Row.Cells["TxDate"].Value);

            // Switch to single-day Balance Sheet view
            for (int i = 1; i < cmbDaySelect.Items.Count; i++)
            {
                if (cmbDaySelect.Items[i].ToString().StartsWith(targetDate.ToString("dd-MM-yyyy")))
                {
                    ToggleViewMode();
                    cmbDaySelect.SelectedIndex = i;
                    return;
                }
            }
        }

        private DataTable CreateBalanceSheetTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CategoryCode", typeof(string));
            dt.Columns.Add("Particulars", typeof(string));
            dt.Columns.Add("OpeningBal", typeof(decimal));
            dt.Columns.Add("Inflow", typeof(decimal));
            dt.Columns.Add("Outflow", typeof(decimal));
            dt.Columns.Add("ClosingBal", typeof(decimal));
            dt.Columns.Add("NetChange", typeof(decimal));
            return dt;
        }

        private void AddBalanceSheetRow(DataTable dt, string name, decimal opn, decimal inAmt, decimal outAmt, decimal close, string code)
        {
            decimal change = close - opn;
            dt.Rows.Add(code, name, opn, inAmt, outAmt, close, change);
        }

        private void BindSideGrid(UltraGrid grid, DataTable dt, bool isAsset)
        {
            grid.DataSource = dt;
            if (grid.DisplayLayout.Bands.Count > 0)
            {
                var band = grid.DisplayLayout.Bands[0];
                band.ColHeadersVisible = true;
                grid.DisplayLayout.Override.HeaderPlacement = HeaderPlacement.OncePerRowIsland;

                if (band.Columns.Exists("CategoryCode")) band.Columns["CategoryCode"].Hidden = true;

                if (band.Columns.Exists("Particulars"))
                {
                    band.Columns["Particulars"].Header.Caption = "Particulars";
                    band.Columns["Particulars"].Width = 230;
                    band.Columns["Particulars"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["Particulars"].CellAppearance.ForeColor = ButtonTextBlue;
                }
                FormatCol(band, "OpeningBal", "Opening (12 AM)", 100);
                FormatCol(band, "Inflow", "Additions (+)", 95);
                FormatCol(band, "Outflow", "Reductions (-)", 95);
                FormatCol(band, "ClosingBal", "Closing (12 AM)", 110, isBold: true, foreColor: isAsset ? DebitGreen : CreditRed);
                FormatCol(band, "NetChange", "Net Change", 95, isBold: true);
            }
        }

        private void FormatCol(UltraGridBand band, string colKey, string caption, int width, bool isBold = false, Color? foreColor = null)
        {
            if (band.Columns.Exists(colKey))
            {
                var col = band.Columns[colKey];
                col.Header.Caption = caption;
                col.Format = "#,##0.00";
                col.Width = width;
                col.CellAppearance.TextHAlign = HAlign.Right;
                col.Header.Appearance.BackColor = GridHeaderBlue;
                col.Header.Appearance.BackColor2 = GridHeaderBlueDark;
                col.Header.Appearance.BackGradientStyle = GradientStyle.Vertical;
                col.Header.Appearance.ForeColor = Color.White;
                col.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
                if (isBold) col.CellAppearance.FontData.Bold = DefaultableBoolean.True;
                if (foreColor.HasValue) col.CellAppearance.ForeColor = foreColor.Value;
            }
        }

        private void UltraGrid_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            if (e.Row == null || !e.Row.IsDataRow) return;

            string categoryCode = e.Row.Cells.Exists("CategoryCode") ? e.Row.Cells["CategoryCode"].Value?.ToString() : "ALL";
            string particularName = e.Row.Cells.Exists("Particulars") ? e.Row.Cells["Particulars"].Value?.ToString() : "";

            DateTime targetFrom = dtFromDate.DateTime.Date;
            DateTime targetTo = dtToDate.DateTime.Date;

            if (cmbDaySelect.SelectedIndex > 0 && _currentReport != null && _currentReport.DayRows.Count > 0)
            {
                int dayIndex = (_currentReport.DayRows.Count - 1) - (cmbDaySelect.SelectedIndex - 1);
                if (dayIndex >= 0 && dayIndex < _currentReport.DayRows.Count)
                {
                    targetFrom = _currentReport.DayRows[dayIndex].TxDate;
                    targetTo = targetFrom;
                }
            }

            ShowCategoryDrilldown(categoryCode, particularName, targetFrom, targetTo);
        }

        private void ShowCategoryDrilldown(string categoryCode, string title, DateTime fromDate, DateTime toDate)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                int branchId = 1;
                try
                {
                    int sessBranch = SessionContext.BranchId;
                    branchId = sessBranch > 0 ? sessBranch : Convert.ToInt32(DataBase.BranchId);
                }
                catch { branchId = 1; }

                var filter = new DailyFinancialMovementFilter
                {
                    CompanyId = 1,
                    BranchId = branchId,
                    FinYearId = 1,
                    FromDate = fromDate,
                    ToDate = toDate,
                    DrillDownCategory = categoryCode,
                    DrillDownDate = fromDate == toDate ? (DateTime?)fromDate : null
                };

                var rpt = _repository.GetReport(filter);

                Form dlg = new Form
                {
                    Text = $"{title} - Detailed Movement ({fromDate:dd-MM-yyyy} to {toDate:dd-MM-yyyy})",
                    Size = new Size(950, 520),
                    StartPosition = FormStartPosition.CenterParent,
                    BackColor = FormBackColor,
                    Font = new Font("Segoe UI", 9F)
                };

                UltraGrid grid = new UltraGrid
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9F)
                };

                DataTable dt = new DataTable();
                dt.Columns.Add("TxDate", typeof(DateTime));
                dt.Columns.Add("RefNo", typeof(string));
                dt.Columns.Add("MovementType", typeof(string));
                dt.Columns.Add("LedgerName", typeof(string));
                dt.Columns.Add("InAmount", typeof(decimal));
                dt.Columns.Add("OutAmount", typeof(decimal));
                dt.Columns.Add("Narration", typeof(string));
                dt.Columns.Add("UserName", typeof(string));

                if (rpt.DrilldownRows != null)
                {
                    foreach (var item in rpt.DrilldownRows)
                    {
                        dt.Rows.Add(
                            item.TxDate != DateTime.MinValue ? (object)item.TxDate : DBNull.Value,
                            item.RefNo,
                            item.MovementType,
                            item.LedgerName,
                            item.InAmount,
                            item.OutAmount,
                            item.Narration,
                            item.UserName
                        );
                    }
                }

                grid.DataSource = dt;
                grid.UseOsThemes = DefaultableBoolean.False;
                grid.InitializeLayout += (s, ev) =>
                {
                    var b = ev.Layout.Bands[0];
                    b.ColHeadersVisible = true;
                    ev.Layout.Override.HeaderPlacement = HeaderPlacement.OncePerRowIsland;
                    ev.Layout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
                    ev.Layout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
                    ev.Layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
                    ev.Layout.Override.HeaderAppearance.ForeColor = Color.White;
                    ev.Layout.Override.RowAlternateAppearance.BackColor = GridAltRow;
                    ev.Layout.Override.CellClickAction = CellClickAction.RowSelect;
                    ev.Layout.Override.AllowUpdate = DefaultableBoolean.False;

                    if (b.Columns.Exists("TxDate")) { b.Columns["TxDate"].Header.Caption = "Date/Time"; b.Columns["TxDate"].Format = "dd-MM-yyyy HH:mm"; b.Columns["TxDate"].Width = 110; }
                    if (b.Columns.Exists("RefNo")) { b.Columns["RefNo"].Header.Caption = "Voucher / Bill #"; b.Columns["RefNo"].Width = 110; }
                    if (b.Columns.Exists("MovementType")) { b.Columns["MovementType"].Header.Caption = "Type"; b.Columns["MovementType"].Width = 90; }
                    if (b.Columns.Exists("LedgerName")) { b.Columns["LedgerName"].Header.Caption = "Particulars / Account"; b.Columns["LedgerName"].Width = 200; }
                    if (b.Columns.Exists("InAmount"))
                    {
                        b.Columns["InAmount"].Header.Caption = "Inflow / Add (+) ₹";
                        b.Columns["InAmount"].Format = "#,##0.00";
                        b.Columns["InAmount"].Width = 100;
                        b.Columns["InAmount"].CellAppearance.TextHAlign = HAlign.Right;
                    }
                    if (b.Columns.Exists("OutAmount"))
                    {
                        b.Columns["OutAmount"].Header.Caption = "Outflow / Less (-) ₹";
                        b.Columns["OutAmount"].Format = "#,##0.00";
                        b.Columns["OutAmount"].Width = 100;
                        b.Columns["OutAmount"].CellAppearance.TextHAlign = HAlign.Right;
                    }
                    if (b.Columns.Exists("Narration")) { b.Columns["Narration"].Header.Caption = "Narration / Details"; b.Columns["Narration"].Width = 180; }
                    if (b.Columns.Exists("UserName")) { b.Columns["UserName"].Header.Caption = "User"; b.Columns["UserName"].Width = 80; }
                };

                Panel pnlClose = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.FromArgb(232, 246, 255) };
                Button btnClose = new Button
                {
                    Text = "Close (Esc)",
                    DialogResult = DialogResult.OK,
                    Anchor = AnchorStyles.Right,
                    Location = new Point(830, 8),
                    Size = new Size(95, 26),
                    BackColor = Color.FromArgb(232, 241, 252),
                    ForeColor = ButtonTextBlue
                };
                pnlClose.Controls.Add(btnClose);

                dlg.Controls.Add(grid);
                dlg.Controls.Add(pnlClose);
                dlg.AcceptButton = btnClose;
                dlg.CancelButton = btnClose;

                dlg.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening drill-down:\n" + ex.Message, "Drilldown Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void ExportCsv()
        {
            if (_currentReport == null || _currentReport.DayRows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                sfd.FileName = $"AssetProgression_BalanceSheet_{dtFromDate.DateTime:yyyyMMdd}_{dtToDate.DateTime:yyyyMMdd}.csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("=== ASSET PROGRESSION & BALANCE SHEET MOVEMENT REPORT ===");
                    sb.AppendLine($"Reporting Period: {dtFromDate.DateTime:dd-MM-yyyy} to {dtToDate.DateTime:dd-MM-yyyy}");
                    sb.AppendLine($"Starting Assets (12 AM): ₹ {_currentReport.Summary.PeriodAssetOpening:F2}");
                    sb.AppendLine($"Ending Assets (12 AM): ₹ {_currentReport.Summary.PeriodAssetClosing:F2}");
                    sb.AppendLine($"Period Asset Growth: ₹ {_currentReport.Summary.PeriodAssetGrowth:F2} ({_currentReport.Summary.PeriodAssetGrowthPct:F2}%)");
                    sb.AppendLine($"Ending Net Store Worth: ₹ {_currentReport.Summary.LatestNetStorePosition:F2} (Net Worth Gain: ₹ {_currentReport.Summary.PeriodNetWorthGrowth:F2})");
                    sb.AppendLine();
                    sb.AppendLine("Date,Day,Asset Opening (12 AM),Asset Inflow (+),Asset Outflow (-),Asset Closing (12 AM),Asset Growth (₹),Asset Growth %,Liab Opening (12 AM),Liab Closing (12 AM),Liab Growth (₹),Net Store Worth,Day Net Gain,Cash Close,Bank Close,Stock Close,Debtors Close,Creditors Close");

                    foreach (var r in _currentReport.DayRows)
                    {
                        sb.AppendLine(string.Format("{0:dd-MM-yyyy},{1},{2:F2},{3:F2},{4:F2},{5:F2},{6:F2},{7:F2}%,{8:F2},{9:F2},{10:F2},{11:F2},{12:F2},{13:F2},{14:F2},{15:F2},{16:F2},{17:F2}",
                            r.TxDate, r.DayName, r.AssetOpening, r.AssetAdditions, r.AssetReductions, r.TotalAssets, r.DailyAssetChange, r.DailyAssetChangePct,
                            r.LiabOpening, r.TotalLiabilities, r.DailyLiabChange, r.NetStorePosition, r.DailyNetChange,
                            r.CashClosing, r.BankClosing, r.StockClosing, r.DebtorsClosing, r.CreditorsClosing));
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("CSV exported successfully to:\n" + sfd.FileName, "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void PrintGrids()
        {
            try
            {
                if (_isDaywiseView && ultraGridDaywise != null)
                {
                    ultraGridDaywise.PrintPreview();
                }
                else
                {
                    ultraGridAssets.PrintPreview();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error displaying print preview:\n" + ex.Message, "Print Preview", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
