using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using PosBranch_Win.Accounts;
using Repository.ReportRepository;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class FrmGeneralLedger : Form
    {
        #region Private Fields
        private readonly GeneralLedgerRepository _repository;
        private DataTable _allLedgersTable;
        private GeneralLedgerReport _currentReport;
        private List<GeneralLedgerTransaction> _filteredTransactions;
        private bool _isSelectionHidden = false;
        private bool _isLoadingControls = false;
        private int? _pendingLedgerId = null;

        private static readonly Color FormBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color FilterPanelBackColor = Color.FromArgb(235, 245, 252);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(225, 238, 248);
        private static readonly Color BorderBlue = Color.FromArgb(126, 170, 208);
        private static readonly Color ControlTextColor = Color.FromArgb(18, 49, 102);

        private static readonly Color GridHeaderBlue = Color.FromArgb(29, 78, 137);
        private static readonly Color GridHeaderBlueDark = Color.FromArgb(22, 62, 108);
        private static readonly Color ButtonTextBlue = Color.FromArgb(18, 49, 102);
        private static readonly Color RowAltColor = Color.FromArgb(246, 251, 255);
        private static readonly Color MutedZeroColor = Color.FromArgb(165, 175, 185);

        private static readonly Color DebitGreen = Color.FromArgb(27, 94, 32);
        private static readonly Color CreditRed = Color.FromArgb(183, 28, 28);
        private static readonly Color BalanceDrBlue = Color.FromArgb(18, 49, 102);
        #endregion

        #region Constructor & Lifecycle
        public FrmGeneralLedger()
        {
            InitializeComponent();

            _repository = new GeneralLedgerRepository();
            _currentReport = new GeneralLedgerReport();
            _filteredTransactions = new List<GeneralLedgerTransaction>();

            InitializePanels();

            this.Load += FrmGeneralLedger_Load;
            this.KeyPreview = true;
            this.KeyDown += FrmGeneralLedger_KeyDown;

            btnGenerate.Click += (s, e) => LoadData();
            btnPreviewGrid.Click += (s, e) => PreviewGrid();
            btnPrint.Click += (s, e) => PreviewGrid();
            btnExportCsv.Click += (s, e) => ExportCsv();
            btnClearFilters.Click += (s, e) => ResetFilters();
            btnToggleSelection.Click += (s, e) => ToggleSelectionPanel();

            txtSearch.ValueChanged += (s, e) => ApplySearchFilter();
            cmbGroupFilter.ValueChanged += CmbGroupFilter_ValueChanged;

            ultraGridTransactions.InitializeLayout += UltraGridTransactions_InitializeLayout;
            ultraGridTransactions.InitializeRow += UltraGridTransactions_InitializeRow;
            ultraGridTransactions.DoubleClickRow += UltraGridTransactions_DoubleClickRow;
        }

        private void FrmGeneralLedger_Load(object sender, EventArgs e)
        {
            _isLoadingControls = true;
            PopulatePresetCombo();
            LoadAllLedgers();

            if (!_pendingLedgerId.HasValue)
            {
                cmbDateQuickSelect.Value = "ALL";
                UpdateDateRangeForPreset("ALL");
            }

            cmbDateQuickSelect.ValueChanged += CmbDateQuickSelect_ValueChanged;
            _isLoadingControls = false;

            if (_pendingLedgerId.HasValue && _pendingLedgerId.Value > 0)
            {
                SelectLedgerById(_pendingLedgerId.Value);
                _pendingLedgerId = null;
                LoadData();
            }
            else if (ultraComboLedger.Value != null && int.TryParse(ultraComboLedger.Value.ToString(), out int id) && id > 0)
            {
                LoadData();
            }
        }
        #endregion

        #region Theme & UI Styling
        private void InitializePanels()
        {
            this.BackColor = FormBackColor;

            ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
            ultraPanelControls.Appearance.BorderColor = BorderBlue;
            ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelControls.Dock = DockStyle.Top;
            ultraPanelControls.Height = 78;

            lblSearch.Appearance.ForeColor = ControlTextColor;
            lblGroupFilter.Appearance.ForeColor = ControlTextColor;
            lblLedger.Appearance.ForeColor = ControlTextColor;
            lblPreset.Appearance.ForeColor = ControlTextColor;
            lblFromDate.Appearance.ForeColor = ControlTextColor;
            lblToDate.Appearance.ForeColor = ControlTextColor;
            lblRowCount.Appearance.ForeColor = ControlTextColor;
            lblRowCount.Appearance.FontData.SizeInPoints = 8.5f;

            ultraPanelAction.Appearance.BackColor = ActionPanelBackColor;
            ultraPanelAction.Appearance.BorderColor = BorderBlue;
            ultraPanelAction.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelAction.Dock = DockStyle.Top;
            ultraPanelAction.Height = 45;

            ultraPanelMaster.Appearance.BackColor = FormBackColor;
            ultraPanelMaster.Appearance.BorderColor = BorderBlue;
            ultraPanelMaster.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelMaster.Dock = DockStyle.Fill;

            ultraPanelGridFooter.Appearance.BackColor = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackColor2 = GridHeaderBlueDark;
            ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.Vertical;
            ultraPanelGridFooter.Appearance.BorderColor = BorderBlue;
            ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelGridFooter.Dock = DockStyle.Bottom;
            ultraPanelGridFooter.Height = 34;

            ultraGridTransactions.Dock = DockStyle.Fill;

            ultraPanelControls.SendToBack();
            ultraPanelAction.BringToFront();
            ultraPanelMaster.BringToFront();
            ultraPanelGridFooter.SendToBack();
            ultraGridTransactions.BringToFront();

            lblOpeningSummary.Appearance.ForeColor = Color.FromArgb(255, 255, 200);
            lblOpeningSummary.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblOpeningSummary.Appearance.FontData.SizeInPoints = 8.5f;

            lblDebitSummary.Appearance.ForeColor = Color.FromArgb(220, 255, 220);
            lblDebitSummary.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblDebitSummary.Appearance.FontData.SizeInPoints = 8.5f;

            lblCreditSummary.Appearance.ForeColor = Color.FromArgb(255, 220, 220);
            lblCreditSummary.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblCreditSummary.Appearance.FontData.SizeInPoints = 8.5f;

            SetClosingBadgeState(0);
            StyleButtons();
            SetupGridAppearance();
            UpdateSelectionToggleButtonText();
        }

        private void SetClosingBadgeState(decimal closingBalance)
        {
            if (closingBalance >= 0)
            {
                lblClosingBadge.Appearance.BackColor = Color.FromArgb(232, 245, 233);
                lblClosingBadge.Appearance.BackColor2 = Color.FromArgb(200, 230, 201);
                lblClosingBadge.Appearance.BackGradientStyle = GradientStyle.Vertical;
                lblClosingBadge.Appearance.BorderColor = Color.FromArgb(46, 125, 50);
                lblClosingBadge.Appearance.ForeColor = Color.FromArgb(27, 94, 32);
                lblClosingBadge.Text = $"CLOSING: ₹ {Math.Abs(closingBalance):N2} Dr";
            }
            else
            {
                lblClosingBadge.Appearance.BackColor = Color.FromArgb(255, 235, 238);
                lblClosingBadge.Appearance.BackColor2 = Color.FromArgb(255, 205, 210);
                lblClosingBadge.Appearance.BackGradientStyle = GradientStyle.Vertical;
                lblClosingBadge.Appearance.BorderColor = Color.FromArgb(198, 40, 40);
                lblClosingBadge.Appearance.ForeColor = Color.FromArgb(183, 28, 28);
                lblClosingBadge.Text = $"CLOSING: ₹ {Math.Abs(closingBalance):N2} Cr";
            }
        }

        private void StyleButtons()
        {
            StyleClassicButton(btnGenerate);
            StyleClassicButton(btnPreviewGrid);
            StyleClassicButton(btnPrint);
            StyleClassicButton(btnExportCsv);
            StyleClassicButton(btnClearFilters);
            StyleClassicButton(btnToggleSelection);
        }

        private void StyleClassicButton(UltraButton btn)
        {
            btn.UseAppStyling = false;
            btn.UseOsThemes = DefaultableBoolean.False;
            btn.ButtonStyle = UIElementButtonStyle.Flat;
            btn.Appearance.BackColor = Color.FromArgb(235, 244, 252);
            btn.Appearance.BackColor2 = Color.FromArgb(190, 214, 238);
            btn.Appearance.BackGradientStyle = GradientStyle.Vertical;
            btn.Appearance.BorderColor = BorderBlue;
            btn.Appearance.ForeColor = ButtonTextBlue;
            btn.Appearance.FontData.Bold = DefaultableBoolean.True;
            btn.Appearance.FontData.SizeInPoints = 8.5f;
        }

        private void SetupGridAppearance()
        {
            var grid = ultraGridTransactions;
            grid.UseOsThemes = DefaultableBoolean.False;
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

        #region Date Presets & All-Ledgers Loader
        private void PopulatePresetCombo()
        {
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
            UpdateDateRangeForPreset(cmbDateQuickSelect.Value.ToString());
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

        private void LoadAllLedgers()
        {
            try
            {
                int branchId = SessionContext.BranchId > 0
                    ? SessionContext.BranchId
                    : (int.TryParse(DataBase.BranchId, out int bid) ? bid : 0);

                _allLedgersTable = new Repository.Accounts.LedgerRepository().GetAllLedgers(branchId);

                PopulateGroupFilterCombo();
                BindLedgerDropdown(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading ledgers: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateGroupFilterCombo()
        {
            cmbGroupFilter.Items.Clear();
            cmbGroupFilter.Items.Add("ALL", "All Account Groups");

            if (_allLedgersTable != null && _allLedgersTable.Columns.Contains("GroupName"))
            {
                var groups = _allLedgersTable.AsEnumerable()
                    .Select(r => Convert.ToString(r["GroupName"])?.Trim() ?? string.Empty)
                    .Where(g => !string.IsNullOrWhiteSpace(g))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(g => g)
                    .ToList();

                foreach (string g in groups)
                {
                    cmbGroupFilter.Items.Add(g, g);
                }
            }

            cmbGroupFilter.Value = "ALL";
        }

        private void CmbGroupFilter_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoadingControls) return;
            string groupFilter = Convert.ToString(cmbGroupFilter.Value);
            if (string.Equals(groupFilter, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                groupFilter = null;
            }
            BindLedgerDropdown(groupFilter);
        }

        private void BindLedgerDropdown(string groupFilter)
        {
            if (_allLedgersTable == null) return;

            DataTable sourceTable;
            if (string.IsNullOrWhiteSpace(groupFilter) || !_allLedgersTable.Columns.Contains("GroupName"))
            {
                sourceTable = _allLedgersTable.Copy();
            }
            else
            {
                sourceTable = _allLedgersTable.Clone();
                foreach (DataRow row in _allLedgersTable.Rows)
                {
                    string gName = Convert.ToString(row["GroupName"])?.Trim() ?? string.Empty;
                    if (string.Equals(gName, groupFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        sourceTable.ImportRow(row);
                    }
                }
            }

            // Sort alphabetically by LedgerName for quick discovery
            if (sourceTable.Columns.Contains("LedgerName"))
            {
                sourceTable.DefaultView.Sort = "LedgerName ASC";
                sourceTable = sourceTable.DefaultView.ToTable();
            }

            ultraComboLedger.DataSource = sourceTable;
            ultraComboLedger.ValueMember = "LedgerID";
            ultraComboLedger.DisplayMember = "LedgerName";

            ConfigureLedgerComboLayout();

            if (ultraComboLedger.Rows.Count > 0)
            {
                ultraComboLedger.Value = ultraComboLedger.Rows[0].Cells["LedgerID"].Value;
            }
        }

        private void SelectLedgerById(int ledgerId)
        {
            if (ledgerId <= 0) return;

            _isLoadingControls = true;
            try
            {
                cmbGroupFilter.Value = "ALL";
                BindLedgerDropdown(null);
                ultraComboLedger.Value = ledgerId;
            }
            finally
            {
                _isLoadingControls = false;
            }
        }

        private void ConfigureLedgerComboLayout()
        {
            ultraComboLedger.DropDownWidth = 520;
            ultraComboLedger.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortMulti;
            ultraComboLedger.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;

            ultraComboLedger.DisplayLayout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            ultraComboLedger.DisplayLayout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            ultraComboLedger.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            ultraComboLedger.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            ultraComboLedger.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            ultraComboLedger.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

            if (ultraComboLedger.DisplayLayout.Bands.Count > 0)
            {
                var band = ultraComboLedger.DisplayLayout.Bands[0];
                foreach (UltraGridColumn col in band.Columns)
                {
                    col.Hidden = true;
                }

                if (band.Columns.Exists("LedgerID"))
                {
                    band.Columns["LedgerID"].Hidden = false;
                    band.Columns["LedgerID"].Header.Caption = "ID";
                    band.Columns["LedgerID"].Width = 55;
                    band.Columns["LedgerID"].Header.VisiblePosition = 0;
                    band.Columns["LedgerID"].CellAppearance.TextHAlign = HAlign.Center;
                }

                if (band.Columns.Exists("LedgerName"))
                {
                    band.Columns["LedgerName"].Hidden = false;
                    band.Columns["LedgerName"].Header.Caption = "Ledger Name";
                    band.Columns["LedgerName"].Width = 240;
                    band.Columns["LedgerName"].Header.VisiblePosition = 1;
                    band.Columns["LedgerName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["LedgerName"].CellAppearance.ForeColor = ButtonTextBlue;
                }

                if (band.Columns.Exists("GroupName"))
                {
                    band.Columns["GroupName"].Hidden = false;
                    band.Columns["GroupName"].Header.Caption = "Account Group";
                    band.Columns["GroupName"].Width = 175;
                    band.Columns["GroupName"].Header.VisiblePosition = 2;
                }
            }
        }
        #endregion

        #region Grid Layout & Formatting
        private void UltraGridTransactions_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            var band = e.Layout.Bands[0];

            foreach (UltraGridColumn col in band.Columns)
            {
                col.Hidden = true;
            }

            ConfigureColumn(band, "VoucherDate", "Date", 95, HAlign.Center, 0);
            if (band.Columns.Exists("VoucherDate"))
            {
                band.Columns["VoucherDate"].Format = "dd-MMM-yyyy";
            }

            ConfigureColumn(band, "VoucherNo", "Voucher No", 110, HAlign.Center, 1);
            ConfigureColumn(band, "VoucherID", "Voucher ID", 80, HAlign.Center, 2);

            ConfigureColumn(band, "VoucherTypeName", "Type", 95, HAlign.Left, 3);
            if (band.Columns.Exists("VoucherTypeName"))
            {
                band.Columns["VoucherTypeName"].CellAppearance.ForeColor = Color.FromArgb(69, 90, 100);
                band.Columns["VoucherTypeName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            ConfigureColumn(band, "Particulars", "Particulars / Contra Account", 210, HAlign.Left, 4);
            ConfigureColumn(band, "VoucherLedgerName", "Saved Ledger", 145, HAlign.Left, 5);
            ConfigureColumn(band, "Narration", "Narration", 220, HAlign.Left, 6);

            ConfigureColumn(band, "DebitAmount", "Debit (Dr) ₹", 115, HAlign.Right, 7);
            if (band.Columns.Exists("DebitAmount"))
            {
                band.Columns["DebitAmount"].Format = "N2";
                band.Columns["DebitAmount"].CellAppearance.ForeColor = DebitGreen;
                band.Columns["DebitAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            ConfigureColumn(band, "CreditAmount", "Credit (Cr) ₹", 115, HAlign.Right, 8);
            if (band.Columns.Exists("CreditAmount"))
            {
                band.Columns["CreditAmount"].Format = "N2";
                band.Columns["CreditAmount"].CellAppearance.ForeColor = CreditRed;
                band.Columns["CreditAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            ConfigureColumn(band, "FormattedBalance", "Running Balance", 130, HAlign.Right, 9);
            if (band.Columns.Exists("FormattedBalance"))
            {
                band.Columns["FormattedBalance"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            ConfigureColumn(band, "UserName", "Posted By", 90, HAlign.Center, 10);
            ConfigureColumn(band, "UserDate", "Posted Time", 135, HAlign.Center, 11);
            if (band.Columns.Exists("UserDate"))
            {
                band.Columns["UserDate"].Format = "dd-MMM-yyyy HH:mm";
            }

            band.Override.AllowColSizing = AllowColSizing.Free;
            e.Layout.AutoFitStyle = AutoFitStyle.ExtendLastColumn;
        }

        private void UltraGridTransactions_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (e.Row == null || !e.Row.IsDataRow) return;

            if (e.Row.Cells.Exists("RunningBalance") && e.Row.Cells.Exists("FormattedBalance"))
            {
                var val = e.Row.Cells["RunningBalance"].Value;
                if (val != null && val != DBNull.Value)
                {
                    decimal balance = Convert.ToDecimal(val);
                    e.Row.Cells["FormattedBalance"].Appearance.ForeColor = balance >= 0 ? BalanceDrBlue : CreditRed;
                }
            }

            if (e.Row.Cells.Exists("DebitAmount"))
            {
                var val = e.Row.Cells["DebitAmount"].Value;
                bool isZero = val == null || val == DBNull.Value || Convert.ToDecimal(val) == 0;
                e.Row.Cells["DebitAmount"].Appearance.ForeColor = isZero ? MutedZeroColor : DebitGreen;
            }

            if (e.Row.Cells.Exists("CreditAmount"))
            {
                var val = e.Row.Cells["CreditAmount"].Value;
                bool isZero = val == null || val == DBNull.Value || Convert.ToDecimal(val) == 0;
                e.Row.Cells["CreditAmount"].Appearance.ForeColor = isZero ? MutedZeroColor : CreditRed;
            }

            // Highlight when Vouchers.LedgerName differs from selected LedgerMaster.LedgerName
            if (_currentReport != null && !string.IsNullOrWhiteSpace(_currentReport.LedgerName) && e.Row.Cells.Exists("VoucherLedgerName"))
            {
                string savedName = Convert.ToString(e.Row.Cells["VoucherLedgerName"].Value)?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(savedName) &&
                    !string.Equals(savedName, _currentReport.LedgerName.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    e.Row.Cells["VoucherLedgerName"].Appearance.BackColor = Color.FromArgb(255, 243, 205);
                    e.Row.Cells["VoucherLedgerName"].Appearance.ForeColor = Color.FromArgb(146, 64, 14);
                    e.Row.Cells["VoucherLedgerName"].Appearance.FontData.Bold = DefaultableBoolean.True;
                }
            }
        }

        private void ConfigureColumn(UltraGridBand band, string key, string headerText, int width, HAlign align, int position)
        {
            if (band.Columns.Exists(key))
            {
                var col = band.Columns[key];
                col.Hidden = false;
                col.Header.Caption = headerText;
                col.Width = width;
                col.CellAppearance.TextHAlign = align;
                col.Header.VisiblePosition = position;
            }
        }
        #endregion

        #region Data Loading & Filtering
        private void LoadData()
        {
            try
            {
                if (ultraComboLedger.Value == null || !int.TryParse(ultraComboLedger.Value.ToString(), out int ledgerId) || ledgerId <= 0)
                {
                    MessageBox.Show("Please select a Ledger account.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                this.Cursor = Cursors.WaitCursor;

                DateTime from = Convert.ToDateTime(dtFromDate.DateTime).Date;
                DateTime to = Convert.ToDateTime(dtToDate.DateTime).Date;

                _currentReport = _repository.GetGeneralLedger(ledgerId, from, to) ?? new GeneralLedgerReport();
                ApplySearchFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading General Ledger report: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void ApplySearchFilter()
        {
            if (_currentReport == null || _currentReport.Transactions == null)
            {
                ultraGridTransactions.DataSource = null;
                lblRowCount.Text = "Total: 0 rows";
                ClearSummary();
                return;
            }

            string searchText = txtSearch.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                _filteredTransactions = new List<GeneralLedgerTransaction>(_currentReport.Transactions);
            }
            else
            {
                _filteredTransactions = _currentReport.Transactions
                    .Where(t => ContainsText(t.Particulars, searchText)
                             || ContainsText(t.VoucherLedgerName, searchText)
                             || ContainsText(t.Narration, searchText)
                             || ContainsText(t.VoucherTypeName, searchText)
                             || ContainsText(t.VoucherNo, searchText)
                             || ContainsText(t.UserName, searchText)
                             || t.VoucherID.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            ultraGridTransactions.DataSource = _filteredTransactions;

            string ledgerTitle = string.IsNullOrWhiteSpace(_currentReport.LedgerName)
                ? string.Empty
                : $" | {_currentReport.LedgerName}" + (string.IsNullOrWhiteSpace(_currentReport.GroupName) ? string.Empty : $" ({_currentReport.GroupName})");

            lblRowCount.Text = $"Total: {_filteredTransactions.Count} rows{ledgerTitle}";
            UpdateSummaryDisplay();
        }

        private static bool ContainsText(string value, string searchText)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void UpdateSummaryDisplay()
        {
            if (_currentReport == null || _currentReport.Summary == null)
            {
                ClearSummary();
                return;
            }

            var sum = _currentReport.Summary;
            lblOpeningSummary.Text = $"Opening: ₹ {Math.Abs(sum.OpeningBalance):N2} {(sum.OpeningBalance >= 0 ? "Dr" : "Cr")}";
            lblDebitSummary.Text = $"Total Debit (Dr): ₹ {sum.TotalDebit:N2}";
            lblCreditSummary.Text = $"Total Credit (Cr): ₹ {sum.TotalCredit:N2}";

            SetClosingBadgeState(sum.ClosingBalance);
        }

        private void ClearSummary()
        {
            lblOpeningSummary.Text = "Opening: ₹ 0.00 Dr";
            lblDebitSummary.Text = "Total Debit (Dr): ₹ 0.00";
            lblCreditSummary.Text = "Total Credit (Cr): ₹ 0.00";
            SetClosingBadgeState(0);
        }

        private void ResetFilters()
        {
            txtSearch.Text = string.Empty;
            _isLoadingControls = true;
            cmbGroupFilter.Value = "ALL";
            BindLedgerDropdown(null);
            cmbDateQuickSelect.Value = "ALL";
            UpdateDateRangeForPreset("ALL");
            _isLoadingControls = false;
            LoadData();
        }

        public void RibbonClear() => ResetFilters();
        public void Clear() => ResetFilters();

        public void LoadForLedger(int ledgerId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                _pendingLedgerId = ledgerId;
                if (dtFromDate != null) dtFromDate.DateTime = fromDate.Date;
                if (dtToDate != null) dtToDate.DateTime = toDate.Date;
                if (cmbDateQuickSelect != null) cmbDateQuickSelect.Value = "CUSTOM";

                if (_allLedgersTable != null)
                {
                    SelectLedgerById(ledgerId);
                    _pendingLedgerId = null;
                    LoadData();
                }
            }
            catch { }
        }
        #endregion

        #region Actions & Drill-Down
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
                printDoc.Header.TextLeft = "General Ledger Statement — " + (ultraComboLedger.Text ?? "");
                printDoc.Header.TextRight = $"Period: {Convert.ToDateTime(dtFromDate.DateTime):dd-MMM-yyyy} to {Convert.ToDateTime(dtToDate.DateTime):dd-MMM-yyyy}";
                printDoc.Footer.TextCenter = "Page [Page #]";

                using (var previewDialog = new PrintPreviewDialog())
                {
                    previewDialog.Document = printDoc;
                    previewDialog.WindowState = FormWindowState.Maximized;
                    previewDialog.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Preview failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportCsv()
        {
            if (ultraGridTransactions.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string ledgerPart = string.IsNullOrWhiteSpace(_currentReport?.LedgerName)
                ? "All"
                : string.Join("_", _currentReport.LedgerName.Split(Path.GetInvalidFileNameChars()));

            using (SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = ReportPdfExporter.CsvAndPdfFilter,
                FileName = $"GeneralLedger_{ledgerPart}_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (ReportPdfExporter.CheckAndExportPdf(
                            sfd,
                            ultraGridTransactions,
                            "General Ledger Statement",
                            $"Ledger: {ultraComboLedger.Text} | Period: {Convert.ToDateTime(dtFromDate.DateTime):dd/MM/yyyy} to {Convert.ToDateTime(dtToDate.DateTime):dd/MM/yyyy}"))
                        {
                            return;
                        }

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
                var item = e.Row.ListObject as GeneralLedgerTransaction;
                if (item == null || item.VoucherID <= 0) return;

                string vType = (item.VoucherTypeName ?? string.Empty).Trim();
                string vNo = string.IsNullOrWhiteSpace(item.VoucherNo) ? item.VoucherID.ToString() : item.VoucherNo.Trim();

                Form targetForm = null;
                string tabTitle = vNo;

                if (vType.Equals("GENPAY", StringComparison.OrdinalIgnoreCase))
                {
                    targetForm = new FrmGeneralPayment(vNo);
                    tabTitle = $"General Payment - {vNo}";
                }
                else if (vType.Equals("GENREC", StringComparison.OrdinalIgnoreCase))
                {
                    targetForm = new FrmGeneralReceipt(vNo);
                    tabTitle = $"General Receipt - {vNo}";
                }
                else if (vType.Equals("Contra", StringComparison.OrdinalIgnoreCase))
                {
                    targetForm = new FrmContra(vNo);
                    tabTitle = $"Contra - {vNo}";
                }
                else if (vType.Equals("VENDPAY", StringComparison.OrdinalIgnoreCase) || vType.Equals("Payment", StringComparison.OrdinalIgnoreCase))
                {
                    targetForm = new FrmPayment(item.VoucherID);
                    tabTitle = $"Payment - {vNo}";
                }
                else if (vType.Equals("CUSTRCPT", StringComparison.OrdinalIgnoreCase) || vType.Equals("Receipt", StringComparison.OrdinalIgnoreCase))
                {
                    targetForm = new FrmReceipt(item.VoucherID);
                    tabTitle = $"Receipt - {vNo}";
                }

                if (targetForm != null)
                {
                    var home = Application.OpenForms.OfType<Home>().FirstOrDefault();
                    if (home != null)
                    {
                        home.OpenFormInTabSafe(targetForm, tabTitle);
                    }
                    else
                    {
                        targetForm.Show(this);
                    }
                    return;
                }

                MessageBox.Show(
                    $"Voucher Type: {item.VoucherTypeName}\n" +
                    $"Voucher No / ID: {vNo} (#{item.VoucherID})\n" +
                    $"Date: {item.VoucherDate:dd-MMM-yyyy}\n" +
                    $"Contra Account: {item.Particulars}\n" +
                    $"Saved Ledger: {item.VoucherLedgerName}\n" +
                    $"Debit: ₹ {item.DebitAmount:N2} | Credit: ₹ {item.CreditAmount:N2}\n" +
                    $"Posted By: {item.UserName} ({item.UserDate?.ToString("dd-MMM-yyyy HH:mm") ?? "N/A"})\n" +
                    $"Narration: {item.Narration}",
                    "Voucher Details",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening voucher: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FrmGeneralLedger_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) this.Close();
            else if (e.KeyCode == Keys.F5) LoadData();
            else if (e.Control && e.KeyCode == Keys.E) ExportCsv();
            else if (e.Control && e.KeyCode == Keys.P) PreviewGrid();
        }
        #endregion
    }
}

