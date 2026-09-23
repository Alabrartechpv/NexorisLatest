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
using Repository.MasterRepositry;
using Repository.ReportRepository;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class FrmCashBankBook : Form
    {
        #region Private Fields
        private readonly CashBankBookRepository _repository;
        private readonly LedgerRepository _ledgerRepository;
        private CashBankBookModel _currentReport;
        private List<CashBankTransaction> _filteredTransactions;
        private bool _isSelectionHidden = false;

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
        private static readonly Color MutedZeroColor = Color.FromArgb(165, 175, 185);

        private static readonly Color ReceiptGreen = Color.FromArgb(27, 94, 32);
        private static readonly Color PaymentRed = Color.FromArgb(183, 28, 28);
        private static readonly Color BalanceDrBlue = Color.FromArgb(18, 49, 102);
        #endregion

        #region Constructor & Lifecycle
        public FrmCashBankBook()
        {
            InitializeComponent();

            _repository = new CashBankBookRepository();
            _ledgerRepository = new LedgerRepository();
            _currentReport = new CashBankBookModel();
            _filteredTransactions = new List<CashBankTransaction>();

            // Setup Panels, Docking and Z-Order immediately in constructor
            InitializePanels();

            this.Load += FrmCashBankBook_Load;
            this.KeyPreview = true;
            this.KeyDown += FrmCashBankBook_KeyDown;

            // Wire Actions
            btnGenerate.Click += (s, e) => LoadData();
            btnPreviewGrid.Click += (s, e) => PreviewGrid();
            btnPrint.Click += (s, e) => PrintReport();
            btnExportCsv.Click += (s, e) => ExportCsv();
            btnClearFilters.Click += (s, e) => ResetFilters();
            btnToggleSelection.Click += (s, e) => ToggleSelectionPanel();

            txtSearch.ValueChanged += (s, e) => ApplySearchFilter();

            // Grid Events
            ultraGridTransactions.InitializeLayout += UltraGridTransactions_InitializeLayout;
            ultraGridTransactions.InitializeRow += UltraGridTransactions_InitializeRow;
            ultraGridTransactions.DoubleClickRow += UltraGridTransactions_DoubleClickRow;
        }

        private void FrmCashBankBook_Load(object sender, EventArgs e)
        {
            PopulatePresetCombo();
            LoadLedgers();

            // Default preset is ALL
            cmbDateQuickSelect.Value = "ALL";
            UpdateDateRangeForPreset("ALL");
            cmbDateQuickSelect.ValueChanged += CmbDateQuickSelect_ValueChanged;

            // Auto-load if ledger exists
            if (ultraComboLedger.Value != null && Convert.ToInt32(ultraComboLedger.Value) > 0)
            {
                LoadData();
            }
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
            lblLedger.Appearance.ForeColor = ControlTextColor;
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

            // Dock & Z-Order (Identical to working FrmTrialBalance and FrmTradingPLAccount)
            ultraPanelControls.SendToBack();
            ultraPanelAction.BringToFront();
            ultraPanelMaster.BringToFront();
            ultraPanelGridFooter.SendToBack();
            ultraGridTransactions.BringToFront();

            // Footer Labels
            lblOpeningSummary.Appearance.ForeColor = Color.FromArgb(255, 255, 200);
            lblOpeningSummary.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblOpeningSummary.Appearance.FontData.SizeInPoints = 8.5f;

            lblReceiptsSummary.Appearance.ForeColor = Color.FromArgb(220, 255, 220);
            lblReceiptsSummary.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblReceiptsSummary.Appearance.FontData.SizeInPoints = 8.5f;

            lblPaymentsSummary.Appearance.ForeColor = Color.FromArgb(255, 220, 220);
            lblPaymentsSummary.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblPaymentsSummary.Appearance.FontData.SizeInPoints = 8.5f;

            SetClosingBadgeState(0);
            StyleButtons();
            SetupGridAppearance();

            UpdateSelectionToggleButtonText();
        }

        private void SetClosingBadgeState(decimal closingBalance)
        {
            if (closingBalance >= 0)
            {
                // Positive Closing Balance (Debit)
                lblClosingBadge.Appearance.BackColor = Color.FromArgb(232, 245, 233);
                lblClosingBadge.Appearance.BackColor2 = Color.FromArgb(200, 230, 201);
                lblClosingBadge.Appearance.BackGradientStyle = GradientStyle.Vertical;
                lblClosingBadge.Appearance.BorderColor = Color.FromArgb(46, 125, 50);
                lblClosingBadge.Appearance.ForeColor = Color.FromArgb(27, 94, 32);
                lblClosingBadge.Appearance.FontData.Bold = DefaultableBoolean.True;
                lblClosingBadge.Appearance.FontData.SizeInPoints = 9.5f;
                lblClosingBadge.Text = $"CLOSING: ₹ {Math.Abs(closingBalance):N2} Dr";
            }
            else
            {
                // Negative / Overdrawn (Credit)
                lblClosingBadge.Appearance.BackColor = Color.FromArgb(255, 235, 238);
                lblClosingBadge.Appearance.BackColor2 = Color.FromArgb(255, 205, 210);
                lblClosingBadge.Appearance.BackGradientStyle = GradientStyle.Vertical;
                lblClosingBadge.Appearance.BorderColor = Color.FromArgb(198, 40, 40);
                lblClosingBadge.Appearance.ForeColor = Color.FromArgb(183, 28, 28);
                lblClosingBadge.Appearance.FontData.Bold = DefaultableBoolean.True;
                lblClosingBadge.Appearance.FontData.SizeInPoints = 9.5f;
                lblClosingBadge.Text = $"CLOSING: ₹ {Math.Abs(closingBalance):N2} Cr (Overdrawn)";
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

        #region Date Presets & Ledgers
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

        private void LoadLedgers()
        {
            try
            {
                int branchId = SessionContext.BranchId > 0 ? SessionContext.BranchId : Convert.ToInt32(DataBase.BranchId);
                DataTable allLedgers = new Repository.Accounts.LedgerRepository().GetAllLedgers(branchId);

                if (allLedgers != null && allLedgers.Rows.Count > 0)
                {
                    DataTable filteredTable = allLedgers.Clone();
                    foreach (DataRow row in allLedgers.Rows)
                    {
                        string groupName = Convert.ToString(row["GroupName"]) ?? string.Empty;
                        string ledgerName = Convert.ToString(row["LedgerName"]) ?? string.Empty;
                        string combined = $"{groupName} {ledgerName}".ToUpperInvariant();

                        if (combined.Contains("CASH") || combined.Contains("BANK"))
                        {
                            filteredTable.ImportRow(row);
                        }
                    }

                    if (filteredTable.Rows.Count == 0)
                    {
                        filteredTable = allLedgers;
                    }

                    ultraComboLedger.DataSource = filteredTable;
                    ultraComboLedger.ValueMember = "LedgerID";
                    ultraComboLedger.DisplayMember = "LedgerName";
                }
                else
                {
                    var request = new AccountLedgerDDLRequest
                    {
                        BranchId = SessionContext.BranchId,
                        For = "All"
                    };
                    var result = _ledgerRepository.getAccountLedgerDDL(request);
                    if (result != null && result.List != null)
                    {
                        var list = result.List.Where(l => (l.Name ?? "").ToUpperInvariant().Contains("CASH") || (l.Name ?? "").ToUpperInvariant().Contains("BANK")).ToList();
                        if (list.Count == 0) list = result.List.ToList();
                        ultraComboLedger.DataSource = list;
                        ultraComboLedger.ValueMember = "Id";
                        ultraComboLedger.DisplayMember = "Name";
                    }
                }

                ConfigureLedgerComboLayout();

                // Select CASH-IN-HAND by default if present
                if (ultraComboLedger.Rows.Count > 0)
                {
                    UltraGridRow defaultRow = null;
                    string displayCol = ultraComboLedger.DisplayMember;
                    string valueCol = ultraComboLedger.ValueMember;

                    foreach (UltraGridRow row in ultraComboLedger.Rows)
                    {
                        string text = row.Cells[displayCol].Value?.ToString() ?? "";
                        if (text.IndexOf("CASH", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            defaultRow = row;
                            break;
                        }
                    }

                    if (defaultRow == null)
                    {
                        defaultRow = ultraComboLedger.Rows[0];
                    }

                    ultraComboLedger.Value = defaultRow.Cells[valueCol].Value;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading ledgers: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureLedgerComboLayout()
        {
            ultraComboLedger.DropDownWidth = 460;
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

                if (band.Columns.Exists("LedgerID")) band.Columns["LedgerID"].Hidden = true;
                if (band.Columns.Exists("Id")) band.Columns["Id"].Hidden = true;
                if (band.Columns.Exists("GroupID")) band.Columns["GroupID"].Hidden = true;
                if (band.Columns.Exists("GroupId")) band.Columns["GroupId"].Hidden = true;

                string nameCol = band.Columns.Exists("LedgerName") ? "LedgerName" : (band.Columns.Exists("Name") ? "Name" : "");
                if (!string.IsNullOrEmpty(nameCol))
                {
                    band.Columns[nameCol].Hidden = false;
                    band.Columns[nameCol].Header.Caption = "Account Name";
                    band.Columns[nameCol].Width = 220;
                    band.Columns[nameCol].Header.VisiblePosition = 0;
                    band.Columns[nameCol].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns[nameCol].CellAppearance.ForeColor = ButtonTextBlue;
                }

                if (band.Columns.Exists("GroupName"))
                {
                    band.Columns["GroupName"].Hidden = false;
                    band.Columns["GroupName"].Header.Caption = "Group";
                    band.Columns["GroupName"].Width = 130;
                    band.Columns["GroupName"].Header.VisiblePosition = 1;
                }

                if (band.Columns.Exists("Balance"))
                {
                    band.Columns["Balance"].Hidden = false;
                    band.Columns["Balance"].Header.Caption = "Balance";
                    band.Columns["Balance"].Width = 100;
                    band.Columns["Balance"].CellAppearance.TextHAlign = HAlign.Right;
                    band.Columns["Balance"].Header.VisiblePosition = 2;
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

            ConfigureColumn(band, "VoucherDate", "Date", 95, HAlign.Center);
            if (band.Columns.Exists("VoucherDate"))
            {
                band.Columns["VoucherDate"].Header.VisiblePosition = 0;
                band.Columns["VoucherDate"].Format = "dd-MMM-yyyy";
            }

            ConfigureColumn(band, "VoucherID", "Voucher ID", 85, HAlign.Center);
            if (band.Columns.Exists("VoucherID"))
            {
                band.Columns["VoucherID"].Header.VisiblePosition = 1;
            }

            ConfigureColumn(band, "VoucherTypeName", "Type", 100, HAlign.Left);
            if (band.Columns.Exists("VoucherTypeName"))
            {
                band.Columns["VoucherTypeName"].Header.VisiblePosition = 2;
                band.Columns["VoucherTypeName"].CellAppearance.ForeColor = Color.FromArgb(69, 90, 100);
            }

            ConfigureColumn(band, "Particulars", "Particulars / Account", 220, HAlign.Left);
            if (band.Columns.Exists("Particulars"))
            {
                band.Columns["Particulars"].Header.VisiblePosition = 3;
            }

            ConfigureColumn(band, "Narration", "Narration", 200, HAlign.Left);
            if (band.Columns.Exists("Narration"))
            {
                band.Columns["Narration"].Header.VisiblePosition = 4;
            }

            ConfigureColumn(band, "ReceiptAmount", "Receipts (Dr) ₹", 120, HAlign.Right);
            if (band.Columns.Exists("ReceiptAmount"))
            {
                band.Columns["ReceiptAmount"].Header.VisiblePosition = 5;
                band.Columns["ReceiptAmount"].Format = "N2";
                band.Columns["ReceiptAmount"].CellAppearance.ForeColor = ReceiptGreen;
                band.Columns["ReceiptAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            ConfigureColumn(band, "PaymentAmount", "Payments (Cr) ₹", 120, HAlign.Right);
            if (band.Columns.Exists("PaymentAmount"))
            {
                band.Columns["PaymentAmount"].Header.VisiblePosition = 6;
                band.Columns["PaymentAmount"].Format = "N2";
                band.Columns["PaymentAmount"].CellAppearance.ForeColor = PaymentRed;
                band.Columns["PaymentAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            ConfigureColumn(band, "FormattedBalance", "Running Balance", 130, HAlign.Right);
            if (band.Columns.Exists("FormattedBalance"))
            {
                band.Columns["FormattedBalance"].Header.VisiblePosition = 7;
                band.Columns["FormattedBalance"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            band.Override.AllowColSizing = AllowColSizing.Free;
            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private void UltraGridTransactions_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (e.Row == null || !e.Row.IsDataRow) return;

            // Running Balance Color
            if (e.Row.Cells.Exists("RunningBalance") && e.Row.Cells.Exists("FormattedBalance"))
            {
                var val = e.Row.Cells["RunningBalance"].Value;
                if (val != null && val != DBNull.Value)
                {
                    decimal balance = Convert.ToDecimal(val);
                    e.Row.Cells["FormattedBalance"].Appearance.ForeColor = balance >= 0 ? BalanceDrBlue : PaymentRed;
                }
            }

            // Mute zero amounts in soft gray
            if (e.Row.Cells.Exists("ReceiptAmount"))
            {
                var val = e.Row.Cells["ReceiptAmount"].Value;
                if (val != null && val != DBNull.Value && Convert.ToDecimal(val) == 0)
                {
                    e.Row.Cells["ReceiptAmount"].Appearance.ForeColor = MutedZeroColor;
                }
                else
                {
                    e.Row.Cells["ReceiptAmount"].Appearance.ForeColor = ReceiptGreen;
                }
            }

            if (e.Row.Cells.Exists("PaymentAmount"))
            {
                var val = e.Row.Cells["PaymentAmount"].Value;
                if (val != null && val != DBNull.Value && Convert.ToDecimal(val) == 0)
                {
                    e.Row.Cells["PaymentAmount"].Appearance.ForeColor = MutedZeroColor;
                }
                else
                {
                    e.Row.Cells["PaymentAmount"].Appearance.ForeColor = PaymentRed;
                }
            }
        }

        private void ConfigureColumn(UltraGridBand band, string key, string headerText, int width, HAlign align)
        {
            if (band.Columns.Exists(key))
            {
                var col = band.Columns[key];
                col.Hidden = false;
                col.Header.Caption = headerText;
                col.Width = width;
                col.CellAppearance.TextHAlign = align;
            }
        }
        #endregion

        #region Data Loading & Filtering
        private void LoadData()
        {
            try
            {
                if (ultraComboLedger.Value == null || string.IsNullOrEmpty(ultraComboLedger.Value.ToString()))
                {
                    MessageBox.Show("Please select a Ledger Account (Cash or Bank).", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                this.Cursor = Cursors.WaitCursor;

                int ledgerId = Convert.ToInt32(ultraComboLedger.Value);
                DateTime from = Convert.ToDateTime(dtFromDate.DateTime).Date;
                DateTime to = Convert.ToDateTime(dtToDate.DateTime).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

                _currentReport = _repository.GetCashBankBook(ledgerId, from, to) ?? new CashBankBookModel();
                ApplySearchFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading report data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                _filteredTransactions = new List<CashBankTransaction>(_currentReport.Transactions);
            }
            else
            {
                _filteredTransactions = _currentReport.Transactions
                    .Where(t => (t.Particulars != null && t.Particulars.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                             || (t.Narration != null && t.Narration.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                             || (t.VoucherTypeName != null && t.VoucherTypeName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                             || (t.VoucherNo != null && t.VoucherNo.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                             || (t.VoucherID.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0))
                    .ToList();
            }

            ultraGridTransactions.DataSource = _filteredTransactions;
            lblRowCount.Text = $"Total: {_filteredTransactions.Count} rows";

            UpdateSummaryDisplay();
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
            lblReceiptsSummary.Text = $"Receipts (Dr): ₹ {sum.TotalReceipts:N2}";
            lblPaymentsSummary.Text = $"Payments (Cr): ₹ {sum.TotalPayments:N2}";

            SetClosingBadgeState(sum.ClosingBalance);
        }

        private void ClearSummary()
        {
            lblOpeningSummary.Text = "Opening: ₹ 0.00 Dr";
            lblReceiptsSummary.Text = "Receipts (Dr): ₹ 0.00";
            lblPaymentsSummary.Text = "Payments (Cr): ₹ 0.00";
            SetClosingBadgeState(0);
        }

        private void ResetFilters()
        {
            txtSearch.Text = string.Empty;
            cmbDateQuickSelect.Value = "ALL";
            UpdateDateRangeForPreset("ALL");
            LoadData();
        }

        public void RibbonClear() => ResetFilters();
        public void Clear() => ResetFilters();
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
                printDoc.Header.TextLeft = "Cash & Bank Book — " + (ultraComboLedger.Text ?? "");
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

            using (SaveFileDialog sfd = new SaveFileDialog { Filter = "CSV Files (*.csv)|*.csv", FileName = $"CashBankBook_{DateTime.Now:yyyyMMdd_HHmm}.csv" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var csv = new StringBuilder();

                        // Header
                        var visibleCols = ultraGridTransactions.DisplayLayout.Bands[0].Columns
                            .Cast<UltraGridColumn>()
                            .Where(c => !c.Hidden)
                            .OrderBy(c => c.Header.VisiblePosition)
                            .ToList();

                        csv.AppendLine(string.Join(",", visibleCols.Select(c => $"\"{c.Header.Caption}\"")));

                        // Rows
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
                string voucherType = e.Row.Cells["VoucherTypeName"].Value?.ToString() ?? "";
                int voucherId = Convert.ToInt32(e.Row.Cells["VoucherID"].Value ?? 0);

                if (voucherId == 0) return;

                if (voucherType.Equals("Sales", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"[Drill-Down]\nOpening Sales Voucher #{voucherId}", "Drill-Down", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (voucherType.Equals("Payment", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"[Drill-Down]\nOpening Payment Voucher #{voucherId}", "Drill-Down", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (voucherType.Equals("Receipt", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"[Drill-Down]\nOpening Receipt Voucher #{voucherId}", "Drill-Down", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Voucher Type '{voucherType}' (ID: {voucherId}) cannot be opened from here.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening voucher: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FrmCashBankBook_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) this.Close();
            else if (e.KeyCode == Keys.F5) LoadData();
            else if (e.Control && e.KeyCode == Keys.E) ExportCsv();
            else if (e.Control && e.KeyCode == Keys.P) PreviewGrid();
        }
        #endregion
    }
}
