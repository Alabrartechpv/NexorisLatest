using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass.Report;
using Repository.ReportRepository;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class FrmBankStatementReport : Form
    {
        #region Private Fields
        private readonly BankStatementReportRepository _repository;
        private BankStatementReportModel _currentReport;
        private List<BankStatementTransaction> _filteredTransactions;
        private bool _isSelectionHidden = false;
        private bool _updatingPaymentMethods = false;

        // Unified IRS POS Financial Design System Palette
        private static readonly Color FormBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color FilterPanelBackColor = Color.FromArgb(235, 245, 252);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(225, 238, 248);
        private static readonly Color BorderBlue = Color.FromArgb(126, 170, 208);
        private static readonly Color ControlTextColor = Color.FromArgb(18, 49, 102);

        private static readonly Color GridHeaderBlue = Color.FromArgb(29, 78, 137);
        private static readonly Color GridHeaderBlueDark = Color.FromArgb(22, 62, 108);
        private static readonly Color ButtonTextBlue = Color.FromArgb(18, 49, 102);
        private static readonly Color RowAltColor = Color.FromArgb(246, 251, 255);
        private static readonly Color SelectedRowColor = Color.FromArgb(227, 242, 253);

        private static readonly Color MoneyInColor = Color.FromArgb(27, 94, 32);
        private static readonly Color MoneyOutColor = Color.FromArgb(183, 28, 28);
        private static readonly Color SalesColor = Color.FromArgb(46, 125, 50);
        private static readonly Color PurchaseColor = Color.FromArgb(211, 47, 47);
        private static readonly Color VendorPaymentColor = Color.FromArgb(245, 124, 0);
        private static readonly Color CustomerReceiptColor = Color.FromArgb(25, 118, 210);
        private static readonly Color ContraColor = Color.FromArgb(94, 53, 177);
        #endregion

        #region Constructor & Lifecycle
        public FrmBankStatementReport()
        {
            InitializeComponent();

            _repository = new BankStatementReportRepository();
            _currentReport = new BankStatementReportModel();
            _filteredTransactions = new List<BankStatementTransaction>();

            InitializePanels();

            this.Load += FrmBankStatementReport_Load;
            this.KeyPreview = true;
            this.KeyDown += FrmBankStatementReport_KeyDown;

            // Wire Action buttons
            btnGenerate.Click += (s, e) => LoadData();
            btnPreviewGrid.Click += (s, e) => PreviewGrid();
            btnPrint.Click += (s, e) => PrintReport();
            btnExportCsv.Click += (s, e) => ExportCsv();
            btnClearFilters.Click += (s, e) => ResetFilters();
            btnToggleSelection.Click += (s, e) => ToggleSelectionPanel();

            txtSearch.ValueChanged += (s, e) => ApplyFilters();
            cmbPaymentMethod.ValueChanged += CmbPaymentMethod_ValueChanged;

            // Grid Events
            ultraGridTransactions.InitializeLayout += UltraGridTransactions_InitializeLayout;
            ultraGridTransactions.InitializeRow += UltraGridTransactions_InitializeRow;
            ultraGridTransactions.DoubleClickRow += UltraGridTransactions_DoubleClickRow;
        }

        private void FrmBankStatementReport_Load(object sender, EventArgs e)
        {
            PopulatePresetCombo();
            PopulateInitialPaymentModes();

            // Default preset is This Month
            cmbDateQuickSelect.Value = "THIS_MONTH";
            UpdateDateRangeForPreset("THIS_MONTH");
            cmbDateQuickSelect.ValueChanged += CmbDateQuickSelect_ValueChanged;

            // Auto-load report
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
            lblPaymentMethod.Appearance.ForeColor = ControlTextColor;
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

            // Footer Panel (Dock: Bottom, Height: 34)
            ultraPanelGridFooter.Appearance.BackColor = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackColor2 = GridHeaderBlueDark;
            ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.Vertical;
            ultraPanelGridFooter.Appearance.BorderColor = BorderBlue;
            ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelGridFooter.Dock = DockStyle.Bottom;
            ultraPanelGridFooter.Height = 34;

            // Grid (Dock: Fill)
            ultraGridTransactions.Dock = DockStyle.Fill;

            // Z-Order
            ultraPanelControls.SendToBack();
            ultraPanelAction.BringToFront();
            ultraPanelMaster.BringToFront();
            ultraPanelGridFooter.SendToBack();
            ultraGridTransactions.BringToFront();

            // Footer Labels
            lblMoneyInSummary.Appearance.ForeColor = Color.FromArgb(220, 255, 220);
            lblMoneyInSummary.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblMoneyInSummary.Appearance.FontData.SizeInPoints = 8.5f;

            lblMoneyOutSummary.Appearance.ForeColor = Color.FromArgb(255, 220, 220);
            lblMoneyOutSummary.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblMoneyOutSummary.Appearance.FontData.SizeInPoints = 8.5f;

            lblBreakdownSummary.Appearance.ForeColor = Color.FromArgb(255, 255, 200);
            lblBreakdownSummary.Appearance.FontData.SizeInPoints = 8.25f;

            SetNetBadgeState(0);
            StyleButtons();
            SetupGridAppearance();
            UpdateSelectionToggleButtonText();
        }

        private void SetNetBadgeState(decimal netAmount)
        {
            if (netAmount >= 0)
            {
                // Net Inflow
                lblNetBadge.Appearance.BackColor = Color.FromArgb(232, 245, 233);
                lblNetBadge.Appearance.BackColor2 = Color.FromArgb(200, 230, 201);
                lblNetBadge.Appearance.BackGradientStyle = GradientStyle.Vertical;
                lblNetBadge.Appearance.BorderColor = Color.FromArgb(46, 125, 50);
                lblNetBadge.Appearance.ForeColor = Color.FromArgb(27, 94, 32);
                lblNetBadge.Appearance.FontData.Bold = DefaultableBoolean.True;
                lblNetBadge.Appearance.FontData.SizeInPoints = 9.5f;
                lblNetBadge.Text = $"NET INFLOW: ₹ {netAmount:N2}";
            }
            else
            {
                // Net Outflow
                lblNetBadge.Appearance.BackColor = Color.FromArgb(255, 235, 238);
                lblNetBadge.Appearance.BackColor2 = Color.FromArgb(255, 205, 210);
                lblNetBadge.Appearance.BackGradientStyle = GradientStyle.Vertical;
                lblNetBadge.Appearance.BorderColor = Color.FromArgb(198, 40, 40);
                lblNetBadge.Appearance.ForeColor = Color.FromArgb(183, 28, 28);
                lblNetBadge.Appearance.FontData.Bold = DefaultableBoolean.True;
                lblNetBadge.Appearance.FontData.SizeInPoints = 9.5f;
                lblNetBadge.Text = $"NET OUTFLOW: ₹ {Math.Abs(netAmount):N2}";
            }
        }

        private void StyleButtons()
        {
            btnGenerate.Text = "View Grid";
            btnPreviewGrid.Text = "Preview Grid";
            btnPrint.Text = "Preview Report";
            btnExportCsv.Text = "Export Grid";
            btnClearFilters.Text = "Reset Filters";
            btnToggleSelection.Text = "Hide Selection";

            StyleClassicButton(btnGenerate);
            StyleClassicButton(btnPreviewGrid);
            StyleClassicButton(btnPrint);
            StyleClassicButton(btnExportCsv);
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
            var grid = ultraGridTransactions;
            grid.DisplayLayout.Reset();

            grid.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            grid.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            grid.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
            grid.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
            grid.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            grid.DisplayLayout.Override.RowSelectors = DefaultableBoolean.False;

            grid.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            grid.DisplayLayout.Override.AllowRowFiltering = DefaultableBoolean.True;
            grid.DisplayLayout.Override.FilterUIType = FilterUIType.FilterRow;
            grid.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;

            // Headers
            grid.DisplayLayout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            grid.DisplayLayout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            grid.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            grid.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            grid.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            grid.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9.5f;
            grid.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

            // Rows
            grid.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            grid.DisplayLayout.Override.RowAlternateAppearance.BackColor = RowAltColor;
            grid.DisplayLayout.Override.SelectedRowAppearance.BackColor = SelectedRowColor;
            grid.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.Black;
            grid.DisplayLayout.Override.CellAppearance.BorderColor = Color.FromArgb(224, 224, 224);
            grid.DisplayLayout.Override.CellPadding = 3;
            grid.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 9f;
        }
        #endregion

        #region Filters & Presets
        private void PopulatePresetCombo()
        {
            cmbDateQuickSelect.Items.Clear();
            cmbDateQuickSelect.Items.Add("ALL", "All Time");
            cmbDateQuickSelect.Items.Add("TODAY", "Today");
            cmbDateQuickSelect.Items.Add("THIS_MONTH", "This Month");
            cmbDateQuickSelect.Items.Add("LAST_MONTH", "Last Month");
            cmbDateQuickSelect.Items.Add("THIS_FY", "This Financial Year");
        }

        private void PopulateInitialPaymentModes()
        {
            _updatingPaymentMethods = true;
            try
            {
                cmbPaymentMethod.Items.Clear();
                cmbPaymentMethod.Items.Add("ALL", "All payment modes");
                cmbPaymentMethod.Value = "ALL";
            }
            finally
            {
                _updatingPaymentMethods = false;
            }
        }

        private void CmbDateQuickSelect_ValueChanged(object sender, EventArgs e)
        {
            string preset = cmbDateQuickSelect.Value?.ToString() ?? "THIS_MONTH";
            UpdateDateRangeForPreset(preset);
        }

        private void UpdateDateRangeForPreset(string preset)
        {
            DateTime now = DateTime.Now;

            switch (preset)
            {
                case "TODAY":
                    dtFromDate.DateTime = now.Date;
                    dtToDate.DateTime = now.Date;
                    break;
                case "THIS_MONTH":
                    dtFromDate.DateTime = new DateTime(now.Year, now.Month, 1);
                    dtToDate.DateTime = now.Date;
                    break;
                case "LAST_MONTH":
                    var lastMonth = now.AddMonths(-1);
                    dtFromDate.DateTime = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                    dtToDate.DateTime = new DateTime(lastMonth.Year, lastMonth.Month, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month));
                    break;
                case "THIS_FY":
                    int fyStartYear = now.Month >= 4 ? now.Year : now.Year - 1;
                    dtFromDate.DateTime = new DateTime(fyStartYear, 4, 1);
                    dtToDate.DateTime = now.Date;
                    break;
                case "ALL":
                default:
                    dtFromDate.DateTime = new DateTime(2000, 1, 1);
                    dtToDate.DateTime = now.Date;
                    break;
            }
        }

        private void CmbPaymentMethod_ValueChanged(object sender, EventArgs e)
        {
            if (!_updatingPaymentMethods)
                ApplyFilters();
        }

        private void PopulatePaymentMethodDropdown()
        {
            string currentSelected = cmbPaymentMethod.Value?.ToString() ?? "ALL";

            var distinctMethods = (_currentReport?.Transactions ?? new List<BankStatementTransaction>())
                .Select(t => (t.PaymentMethod ?? "").Trim())
                .Where(m => !string.IsNullOrEmpty(m))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(m => m)
                .ToList();

            _updatingPaymentMethods = true;
            try
            {
                cmbPaymentMethod.Items.Clear();
                cmbPaymentMethod.Items.Add("ALL", "All payment modes");

                foreach (var method in distinctMethods)
                {
                    cmbPaymentMethod.Items.Add(method, method);
                }

                if (distinctMethods.Any(m => m.Equals(currentSelected, StringComparison.OrdinalIgnoreCase)))
                {
                    cmbPaymentMethod.Value = currentSelected;
                }
                else
                {
                    cmbPaymentMethod.Value = "ALL";
                }
            }
            finally
            {
                _updatingPaymentMethods = false;
            }
        }
        #endregion

        #region Data Retrieval & Filtering
        public void LoadData()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                DateTime fromDate = Convert.ToDateTime(dtFromDate.DateTime).Date;
                DateTime toDate = Convert.ToDateTime(dtToDate.DateTime).Date;

                if (fromDate > toDate)
                {
                    MessageBox.Show("From Date cannot be later than To Date.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dtFromDate.Focus();
                    return;
                }

                _currentReport = _repository.GetBankStatementReport(fromDate, toDate) ?? new BankStatementReportModel();

                PopulatePaymentMethodDropdown();
                ApplyFilters();

                this.Text = $"Bank Statement Report - {_filteredTransactions.Count:N0} transactions";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving Bank Statement: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void ApplyFilters()
        {
            if (_currentReport == null || _currentReport.Transactions == null)
            {
                _filteredTransactions = new List<BankStatementTransaction>();
                ultraGridTransactions.DataSource = null;
                lblRowCount.Text = "Total: 0 rows";
                ClearSummary();
                return;
            }

            string searchText = (txtSearch.Text ?? "").Trim();
            string selectedPayMode = cmbPaymentMethod.Value?.ToString() ?? "ALL";

            IEnumerable<BankStatementTransaction> query = _currentReport.Transactions;

            // Pay mode filter
            if (!string.IsNullOrEmpty(selectedPayMode) && !selectedPayMode.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(t => string.Equals((t.PaymentMethod ?? "").Trim(), selectedPayMode.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            // Search text filter
            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(t =>
                    (t.PartyName != null && t.PartyName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (t.BillVoucherNo != null && t.BillVoucherNo.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (t.TransactionType != null && t.TransactionType.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (t.PaymentMethod != null && t.PaymentMethod.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (t.Reference != null && t.Reference.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    t.MoneyIn.ToString("0.##", CultureInfo.InvariantCulture).Contains(searchText) ||
                    t.MoneyOut.ToString("0.##", CultureInfo.InvariantCulture).Contains(searchText));
            }

            _filteredTransactions = query.OrderBy(t => t.TransactionDate).ThenBy(t => t.BillVoucherNo).ToList();
            ultraGridTransactions.DataSource = new BindingList<BankStatementTransaction>(_filteredTransactions);
            lblRowCount.Text = $"Total: {_filteredTransactions.Count:N0} rows";

            UpdateSummaryDisplay();
        }

        private void UpdateSummaryDisplay()
        {
            decimal totalMoneyIn = 0;
            decimal totalMoneyOut = 0;
            decimal upiIn = 0, upiOut = 0;
            decimal bankIn = 0, bankOut = 0;
            decimal cardIn = 0, cardOut = 0;
            decimal chequeIn = 0, chequeOut = 0;
            decimal otherIn = 0, otherOut = 0;

            foreach (var txn in _filteredTransactions)
            {
                totalMoneyIn += txn.MoneyIn;
                totalMoneyOut += txn.MoneyOut;

                string method = (txn.PaymentMethod ?? "").ToLowerInvariant();

                if (method.Contains("upi"))
                {
                    upiIn += txn.MoneyIn;
                    upiOut += txn.MoneyOut;
                }
                else if (method.Contains("bank") || method.Contains("transfer") || method.Contains("neft") || method.Contains("rtgs") || method.Contains("imps"))
                {
                    bankIn += txn.MoneyIn;
                    bankOut += txn.MoneyOut;
                }
                else if (method.Contains("card"))
                {
                    cardIn += txn.MoneyIn;
                    cardOut += txn.MoneyOut;
                }
                else if (method.Contains("cheque"))
                {
                    chequeIn += txn.MoneyIn;
                    chequeOut += txn.MoneyOut;
                }
                else
                {
                    otherIn += txn.MoneyIn;
                    otherOut += txn.MoneyOut;
                }
            }

            decimal netAmount = totalMoneyIn - totalMoneyOut;

            lblMoneyInSummary.Text = $"Money In (Dr): ₹ {totalMoneyIn:N2}";
            lblMoneyOutSummary.Text = $"Money Out (Cr): ₹ {totalMoneyOut:N2}";

            lblBreakdownSummary.Text = $"UPI: ₹ {(upiIn - upiOut):N2}  |  Transfer: ₹ {(bankIn - bankOut):N2}  |  Card: ₹ {(cardIn - cardOut):N2}  |  Cheque: ₹ {(chequeIn - chequeOut):N2}";

            SetNetBadgeState(netAmount);
        }

        private void ClearSummary()
        {
            lblMoneyInSummary.Text = "Money In (Dr): ₹ 0.00";
            lblMoneyOutSummary.Text = "Money Out (Cr): ₹ 0.00";
            lblBreakdownSummary.Text = "UPI: ₹ 0.00  |  Transfer: ₹ 0.00  |  Card: ₹ 0.00  |  Cheque: ₹ 0.00";
            SetNetBadgeState(0);
        }

        private void ResetFilters()
        {
            txtSearch.Text = string.Empty;
            cmbDateQuickSelect.Value = "THIS_MONTH";
            UpdateDateRangeForPreset("THIS_MONTH");
            cmbPaymentMethod.Value = "ALL";
            LoadData();
        }

        public void RibbonClear() => ResetFilters();
        public void Clear() => ResetFilters();
        #endregion

        #region Grid Formatting & Layout
        private void UltraGridTransactions_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            UltraGridBand band = e.Layout.Bands[0];

            if (!band.Columns.Exists("BankEffect"))
            {
                UltraGridColumn bankEffectColumn = band.Columns.Add("BankEffect", "Bank Effect");
                bankEffectColumn.DataType = typeof(decimal);
            }

            if (band.Columns.Exists("TransactionDate"))
            {
                band.Columns["TransactionDate"].Header.Caption = "Date";
                band.Columns["TransactionDate"].Format = "dd-MMM-yyyy";
                band.Columns["TransactionDate"].Width = 100;
            }
            if (band.Columns.Exists("TransactionType"))
            {
                band.Columns["TransactionType"].Header.Caption = "Type";
                band.Columns["TransactionType"].Width = 130;
            }
            if (band.Columns.Exists("PartyName"))
            {
                band.Columns["PartyName"].Header.Caption = "Particulars / Party";
                band.Columns["PartyName"].Width = 190;
            }
            if (band.Columns.Exists("BillVoucherNo"))
            {
                band.Columns["BillVoucherNo"].Header.Caption = "Voucher / Bill No";
                band.Columns["BillVoucherNo"].Width = 120;
            }
            if (band.Columns.Exists("MoneyIn"))
            {
                band.Columns["MoneyIn"].Header.Caption = "Money In (Dr)";
                band.Columns["MoneyIn"].Format = "N2";
                band.Columns["MoneyIn"].Width = 115;
                band.Columns["MoneyIn"].CellAppearance.TextHAlign = HAlign.Right;
            }
            if (band.Columns.Exists("MoneyOut"))
            {
                band.Columns["MoneyOut"].Header.Caption = "Money Out (Cr)";
                band.Columns["MoneyOut"].Format = "N2";
                band.Columns["MoneyOut"].Width = 115;
                band.Columns["MoneyOut"].CellAppearance.TextHAlign = HAlign.Right;
            }
            if (band.Columns.Exists("BankEffect"))
            {
                band.Columns["BankEffect"].Header.Caption = "Net Movement";
                band.Columns["BankEffect"].Format = "N2";
                band.Columns["BankEffect"].Width = 115;
                band.Columns["BankEffect"].CellAppearance.TextHAlign = HAlign.Right;
            }
            if (band.Columns.Exists("PaymentMethod"))
            {
                band.Columns["PaymentMethod"].Header.Caption = "Pay Mode";
                band.Columns["PaymentMethod"].Width = 120;
            }
            if (band.Columns.Exists("Reference"))
            {
                band.Columns["Reference"].Header.Caption = "Narration / Reference";
                band.Columns["Reference"].Width = 220;
            }
        }

        private void UltraGridTransactions_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            BankStatementTransaction txn = e.Row.ListObject as BankStatementTransaction;

            if (e.Row.Cells.Exists("MoneyIn"))
            {
                decimal moneyIn = Convert.ToDecimal(e.Row.Cells["MoneyIn"].Value ?? 0);
                if (moneyIn > 0)
                {
                    e.Row.Cells["MoneyIn"].Appearance.ForeColor = MoneyInColor;
                    e.Row.Cells["MoneyIn"].Appearance.FontData.Bold = DefaultableBoolean.True;
                }
            }

            if (e.Row.Cells.Exists("MoneyOut"))
            {
                decimal moneyOut = Convert.ToDecimal(e.Row.Cells["MoneyOut"].Value ?? 0);
                if (moneyOut > 0)
                {
                    e.Row.Cells["MoneyOut"].Appearance.ForeColor = MoneyOutColor;
                    e.Row.Cells["MoneyOut"].Appearance.FontData.Bold = DefaultableBoolean.True;
                }
            }

            if (txn != null && e.Row.Cells.Exists("BankEffect"))
            {
                decimal bankEffect = txn.MoneyIn - txn.MoneyOut;
                e.Row.Cells["BankEffect"].Value = bankEffect;
                e.Row.Cells["BankEffect"].Appearance.ForeColor = bankEffect >= 0 ? MoneyInColor : MoneyOutColor;
                e.Row.Cells["BankEffect"].Appearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (e.Row.Cells.Exists("TransactionType"))
            {
                string type = e.Row.Cells["TransactionType"].Value?.ToString() ?? "";
                Color typeColor;
                switch (type)
                {
                    case "Sales":
                        typeColor = SalesColor;
                        break;
                    case "Purchase":
                        typeColor = PurchaseColor;
                        break;
                    case "Vendor Payment":
                    case "General Payment":
                    case "Payment":
                        typeColor = VendorPaymentColor;
                        break;
                    case "Customer Receipt":
                    case "General Receipt":
                    case "Receipt":
                        typeColor = CustomerReceiptColor;
                        break;
                    case "Bank Contra":
                        typeColor = ContraColor;
                        break;
                    default:
                        typeColor = Color.Black;
                        break;
                }
                e.Row.Cells["TransactionType"].Appearance.ForeColor = typeColor;
                e.Row.Cells["TransactionType"].Appearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (e.Row.Cells.Exists("Reference"))
            {
                string reference = e.Row.Cells["Reference"].Value?.ToString() ?? "";
                if (!string.IsNullOrWhiteSpace(reference))
                {
                    e.Row.Cells["Reference"].Appearance.ForeColor = Color.FromArgb(106, 27, 154);
                }
            }
        }
        #endregion

        #region Actions, Print & Export
        private void PreviewGrid()
        {
            if (ultraGridTransactions.Rows.Count == 0)
            {
                MessageBox.Show("No data available to preview.", "Preview Grid", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var printDoc = new UltraGridPrintDocument();
                printDoc.Grid = ultraGridTransactions;
                printDoc.Header.TextLeft = "Bank Statement Report";
                printDoc.Header.TextRight = $"Period: {Convert.ToDateTime(dtFromDate.DateTime):dd-MMM-yyyy} to {Convert.ToDateTime(dtToDate.DateTime):dd-MMM-yyyy}";
                printDoc.Footer.TextCenter = "Page [Page #]";

                var previewDialog = new PrintPreviewDialog();
                previewDialog.Document = printDoc;
                previewDialog.WindowState = FormWindowState.Maximized;
                previewDialog.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Preview failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintReport()
        {
            PreviewGrid();
        }

        private void ExportCsv()
        {
            if (ultraGridTransactions.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog { Filter = "CSV Files (*.csv)|*.csv", FileName = $"BankStatement_{DateTime.Now:yyyyMMdd_HHmm}.csv" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var csv = new StringBuilder();

                        var visibleCols = ultraGridTransactions.DisplayLayout.Bands[0].Columns
                            .Cast<UltraGridColumn>()
                            .Where(c => !c.Hidden)
                            .OrderBy(c => c.Header.VisiblePosition)
                            .ToList();

                        csv.AppendLine(string.Join(",", visibleCols.Select(c => $"\"{c.Header.Caption}\"")));

                        foreach (UltraGridRow row in ultraGridTransactions.Rows)
                        {
                            var values = visibleCols.Select(col =>
                            {
                                object val = row.Cells[col.Key].Value;
                                string text = val != null ? val.ToString().Replace("\"", "\"\"") : string.Empty;
                                return $"\"{text}\"";
                            });
                            csv.AppendLine(string.Join(",", values));
                        }

                        File.WriteAllText(sfd.FileName, csv.ToString(), Encoding.UTF8);
                        MessageBox.Show("Data exported successfully.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ToggleSelectionPanel()
        {
            _isSelectionHidden = !_isSelectionHidden;
            ultraPanelControls.Visible = !_isSelectionHidden;
            UpdateSelectionToggleButtonText();
        }

        private void UpdateSelectionToggleButtonText()
        {
            btnToggleSelection.Text = _isSelectionHidden ? "Show Selection" : "Hide Selection";
        }

        private void UltraGridTransactions_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            if (e.Row == null || !e.Row.IsDataRow) return;

            try
            {
                string voucherType = e.Row.Cells["TransactionType"].Value?.ToString() ?? "";
                string voucherNo = e.Row.Cells["BillVoucherNo"].Value?.ToString() ?? "";

                if (string.IsNullOrEmpty(voucherNo)) return;

                MessageBox.Show($"Transaction Type: {voucherType}\nVoucher/Bill No: {voucherNo}\nParty: {e.Row.Cells["PartyName"].Value}\nAmount: In ₹ {e.Row.Cells["MoneyIn"].Value} / Out ₹ {e.Row.Cells["MoneyOut"].Value}",
                    "Transaction Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening details: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FrmBankStatementReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) this.Close();
            else if (e.KeyCode == Keys.F5) LoadData();
            else if (e.Control && e.KeyCode == Keys.E) ExportCsv();
            else if (e.Control && e.KeyCode == Keys.P) PreviewGrid();
            else if (e.Control && e.KeyCode == Keys.F)
            {
                txtSearch.Focus();
                txtSearch.SelectAll();
            }
        }
        #endregion
    }
}
