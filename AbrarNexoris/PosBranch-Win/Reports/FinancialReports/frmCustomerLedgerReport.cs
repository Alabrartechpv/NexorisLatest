using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class frmCustomerLedgerReport : Form
    {
        // ── Repository ─────────────────────────────────────────────
        private readonly CustomerLedgerReportRepository _repository;

        // ── Data ────────────────────────────────────────────────────
        private List<CustomerLedgerReportRow> _allFetchedRows;   // raw DB result
        private List<CustomerLedgerReportRow> _reportRows;        // after text filter

        // ── Selected customer (set via dialog) ───────────────────────
        private int    _selectedCustomerId;
        private string _selectedCustomerName;

        // ── Summary ─────────────────────────────────────────────────
        private decimal _openingBalance;
        private decimal _totalDebit;
        private decimal _totalCredit;
        private decimal _closingBalance;

        // ── State ───────────────────────────────────────────────────
        private bool     _isLoading;
        private DateTime _lastRefreshed;

        // ── Theme palette ────────────────────────────────────────────
        private static readonly Color ThemeHeaderDark  = Color.FromArgb(15,  23,  42);
        private static readonly Color ThemeSky         = Color.FromArgb(2,  132, 199);
        private static readonly Color ThemeGreen       = Color.FromArgb(22, 163,  74);
        private static readonly Color ThemeRed         = Color.FromArgb(220,  38,  38);
        private static readonly Color ThemeSlate       = Color.FromArgb(51,  65,  85);
        private static readonly Color ThemeLightBg     = Color.FromArgb(241, 245, 249);
        private static readonly Color ThemeBorder      = Color.FromArgb(203, 213, 225);
        private static readonly Color ThemeGreenDark   = Color.FromArgb(21, 128,  61);
        private static readonly Color ThemeRedDark     = Color.FromArgb(185,  28,  28);

        // ════════════════════════════════════════════════════════════
        public frmCustomerLedgerReport()
        {
            _repository     = new CustomerLedgerReportRepository();
            _allFetchedRows = new List<CustomerLedgerReportRow>();
            _reportRows     = new List<CustomerLedgerReportRow>();

            InitializeComponent();

            Load                         += frmCustomerLedgerReport_Load;
            btnSearch.Click              += btnSearch_Click;
            btnReset.Click               += btnReset_Click;
            btnExport.Click              += btnExport_Click;
            btnPrint.Click               += btnPrint_Click;
            btnClose.Click               += btnClose_Click;
            btnSelectCustomer.Click      += btnSelectCustomer_Click;
            ultraComboPreset.ValueChanged    += ultraComboPreset_ValueChanged;
            dtFrom.ValueChanged              += dtDate_ValueChanged;
            dtTo.ValueChanged                += dtDate_ValueChanged;
            txtSearch.TextChanged            += txtSearch_TextChanged;
            gridReport.InitializeLayout      += gridReport_InitializeLayout;
            gridReport.InitializeRow         += gridReport_InitializeRow;

            KeyPreview = true;
            KeyDown    += frmCustomerLedgerReport_KeyDown;
        }

        // ════════════════════════════════════════════════════════════
        //  Initialization
        // ════════════════════════════════════════════════════════════

        private void frmCustomerLedgerReport_Load(object sender, EventArgs e)
        {
            _isLoading = true;
            try
            {
                WindowState = FormWindowState.Maximized;
                Text        = "Customer Ledger Statement";

                ApplyTheme();
                InitializeDateControls();
                InitializePresetCombo();
                ApplyButtonStyles();
                ApplyGridStyles();
                LoadCustomers();
                SetStatus("Ready  |  Select a customer and press Search (F5)  |  Ctrl+E = Export  |  Ctrl+P = Print  |  Esc = Close");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ApplyTheme()
        {
            // Header panel is styled via Designer; just ensure labels OK
            // Controls panel
            ultraPanelControls.Appearance.BackColor  = ThemeLightBg;
            ultraPanelControls.Appearance.BorderColor = ThemeBorder;

            // Grid container
            ultraPanelMaster.Appearance.BackColor = Color.White;

            // Summary panel
            ultraPanelSummary.Appearance.BackColor  = ThemeLightBg;
            ultraPanelSummary.Appearance.BorderColor = ThemeBorder;
        }

        private void InitializeDateControls()
        {
            DateTime today     = DateTime.Today;
            dtFrom.Value       = new DateTime(today.Year, today.Month, 1);
            dtTo.Value         = today;
            dtFrom.MaskInput   = "{date}";
            dtTo.MaskInput     = "{date}";
            dtFrom.FormatString = "dd/MM/yyyy";
            dtTo.FormatString   = "dd/MM/yyyy";
        }

        private void InitializePresetCombo()
        {
            ultraComboPreset.Items.Clear();
            ultraComboPreset.Items.Add("Today",        "Today");
            ultraComboPreset.Items.Add("Yesterday",    "Yesterday");
            ultraComboPreset.Items.Add("ThisWeek",     "This Week");
            ultraComboPreset.Items.Add("ThisMonth",    "This Month");
            ultraComboPreset.Items.Add("Last30Days",   "Last 30 Days");
            ultraComboPreset.Items.Add("Last3Months",  "Last 3 Months");
            ultraComboPreset.Items.Add("ThisYear",     "This Year");
            ultraComboPreset.Items.Add("Custom",       "Custom");
            ultraComboPreset.Value = "ThisMonth";
        }

        private void ApplyButtonStyles()
        {
            SetBtnStyle(btnSelectCustomer, ThemeSky,  Color.White, ThemeSky);
            SetBtnStyle(btnSearch,  Color.FromArgb(37, 99, 235), Color.White, Color.FromArgb(37, 99, 235));
            SetBtnStyle(btnReset,   Color.White, ThemeSlate, ThemeBorder);
            SetBtnStyle(btnExport,  ThemeGreen, Color.White, ThemeGreen);
            SetBtnStyle(btnPrint,   Color.White, ThemeSlate, ThemeBorder);
            SetBtnStyle(btnClose,   ThemeRed,   Color.White, ThemeRed);
        }

        private void SetBtnStyle(Infragistics.Win.Misc.UltraButton btn,
                                  Color back, Color fore, Color border)
        {
            btn.ButtonStyle                              = UIElementButtonStyle.Flat;
            btn.UseOsThemes                              = DefaultableBoolean.False;
            btn.Appearance.BackColor                     = back;
            btn.Appearance.ForeColor                     = fore;
            btn.Appearance.BorderColor                   = border;
            btn.Appearance.FontData.Name                 = "Segoe UI";
            btn.Appearance.FontData.SizeInPoints          = 9f;
            btn.Appearance.FontData.Bold                 = DefaultableBoolean.True;

            Color hover = (back == Color.White)
                ? ThemeLightBg
                : Color.FromArgb(back.A, Math.Max(0, back.R - 20), Math.Max(0, back.G - 20), Math.Max(0, back.B - 20));

            btn.HotTrackAppearance.BackColor   = hover;
            btn.HotTrackAppearance.ForeColor   = fore;
            btn.HotTrackAppearance.BorderColor = border;
        }

        private void ApplyGridStyles()
        {
            gridReport.Font = new Font("Segoe UI", 9f);
            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes = DefaultableBoolean.False;

            var lo = gridReport.DisplayLayout;

            lo.Override.CellClickAction   = CellClickAction.RowSelect;
            lo.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            lo.Override.RowSelectors      = DefaultableBoolean.False;
            lo.Override.DefaultRowHeight  = 26;

            // Cell borders
            lo.Override.CellAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            lo.Override.RowAppearance.BorderColor  = Color.FromArgb(226, 232, 240);

            // Alternating rows
            lo.Override.RowAlternateAppearance.BackColor = Color.FromArgb(248, 250, 252);

            // Selection
            lo.Override.SelectedRowAppearance.BackColor = Color.FromArgb(219, 234, 254);
            lo.Override.SelectedRowAppearance.ForeColor = Color.FromArgb(29, 78, 216);

            // Headers
            lo.Override.HeaderAppearance.BackColor   = Color.FromArgb(30, 41, 59);
            lo.Override.HeaderAppearance.ForeColor   = Color.White;
            lo.Override.HeaderAppearance.BorderColor = Color.FromArgb(15, 23, 42);
            lo.Override.HeaderAppearance.FontData.Name         = "Segoe UI";
            lo.Override.HeaderAppearance.FontData.SizeInPoints = 9.5f;
            lo.Override.HeaderAppearance.FontData.Bold         = DefaultableBoolean.True;
            lo.Override.HeaderStyle = Infragistics.Win.HeaderStyle.Standard;

            // Group-by box
            lo.ViewStyleBand = ViewStyleBand.OutlookGroupBy;
            lo.GroupByBox.Appearance.BackColor = ThemeLightBg;
            lo.GroupByBox.Appearance.ForeColor = ThemeSlate;
        }

        private void LoadCustomers() { /* No longer needed – dialog handles search */ }

        // ════════════════════════════════════════════════════════════
        //  Data access
        // ════════════════════════════════════════════════════════════

        /// <summary>
        /// Hits the database. Called only when customer or date range changes.
        /// </summary>
        private void FetchFromDatabase()
        {
            if (!ValidateDateRange()) return;

            if (_selectedCustomerId <= 0)
            {
                MessageBox.Show("Please select a Customer first (click \"Select Customer\" or press F3).",
                                 "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Cursor = Cursors.WaitCursor;
            SetStatus("Loading data from database…");
            Application.DoEvents();             // let UI refresh the status msg

            try
            {
                int customerId = _selectedCustomerId;
                var filter = new CustomerLedgerReportFilter
                {
                    FromDate  = Convert.ToDateTime(dtFrom.Value).Date,
                    ToDate    = Convert.ToDateTime(dtTo.Value).Date,
                    CompanyId = SessionContext.CompanyId,
                    BranchId  = SessionContext.BranchId,
                    FinYearId = SessionContext.FinYearId,
                    LedgerId  = customerId
                };

                _allFetchedRows = _repository.GetReport(
                    filter,
                    out _openingBalance,
                    out _totalDebit,
                    out _totalCredit,
                    out _closingBalance);

                _lastRefreshed = DateTime.Now;

              

                ApplyLocalFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching report: {ex.Message}", "Error",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("Error occurred. Please check your filters and try again.");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Filters _allFetchedRows in memory (no DB round-trip).
        /// Called whenever the search text changes.
        /// </summary>
        private void ApplyLocalFilter()
        {
            string search = (txtSearch.Text ?? "").Trim();
            List<CustomerLedgerReportRow> filteredList;

            if (string.IsNullOrEmpty(search))
            {
                filteredList = _allFetchedRows;
            }
            else
            {
                string lower = search.ToLowerInvariant();
                filteredList = _allFetchedRows.Where(r =>
                    ContainsIgnoreCase(r.VoucherNo,       lower) ||
                    ContainsIgnoreCase(r.VoucherTypeName, lower) ||
                    ContainsIgnoreCase(r.Particulars,     lower) ||
                    ContainsIgnoreCase(r.Narration,       lower)
                ).ToList();
            }

            // Create list for binding (always prepend virtual Opening Balance row)
            var bindingList = new List<CustomerLedgerReportRow>();
            bindingList.Add(new CustomerLedgerReportRow
            {
                VoucherID       = 0,
                VoucherDate     = Convert.ToDateTime(dtFrom.Value).Date,
                VoucherNo       = "-",
                VoucherTypeName = "Opening Balance",
                Particulars     = "Balance Brought Forward",
                Narration       = "Opening Balance",
                ReceiptAmount   = 0,
                PaymentAmount   = 0,
                RunningBalance  = _openingBalance
            });
            bindingList.AddRange(filteredList);

            _reportRows = bindingList;
            gridReport.DataSource = _reportRows;
            UpdateSummaryCards();

            string statusSuffix = _lastRefreshed == default
                ? ""
                : $"  |  Last refreshed: {_lastRefreshed:HH:mm:ss}";

            int shown = filteredList.Count;
            int total = _allFetchedRows.Count;

            SetStatus(shown == total
                ? $"Showing {total} record(s){statusSuffix}  |  F5 = Refresh  |  Ctrl+E = Export  |  Ctrl+P = Print  |  Esc = Close"
                : $"Showing {shown} of {total} record(s)  (filtered){statusSuffix}  |  F5 = Refresh  |  Esc = Close");
        }

        // ════════════════════════════════════════════════════════════
        //  UI helpers
        // ════════════════════════════════════════════════════════════

        private bool ValidateDateRange()
        {
            if (dtFrom.Value == null || dtTo.Value == null)
            {
                MessageBox.Show("Please enter both From and To dates.", "Validation",
                                 MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            DateTime from = Convert.ToDateTime(dtFrom.Value).Date;
            DateTime to   = Convert.ToDateTime(dtTo.Value).Date;

            if (from > to)
            {
                MessageBox.Show("'From Date' cannot be after 'To Date'.", "Invalid Date Range",
                                 MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtFrom.Focus();
                return false;
            }
            return true;
        }

        private void UpdateSummaryCards()
        {
            lblOpeningVal.Text = _openingBalance.ToString("N2");
            lblDebitVal.Text   = _totalDebit.ToString("N2");
            lblCreditVal.Text  = _totalCredit.ToString("N2");
            lblClosingVal.Text = _closingBalance.ToString("N2");

            SetBalanceColor(lblOpeningVal, _openingBalance);
            SetBalanceColor(lblClosingVal, _closingBalance);
            lblDebitVal.Appearance.ForeColor  = ThemeGreenDark;
            lblCreditVal.Appearance.ForeColor = ThemeRedDark;
        }

        private static void SetBalanceColor(Infragistics.Win.Misc.UltraLabel lbl, decimal value)
        {
            lbl.Appearance.ForeColor = value >= 0 ? Color.FromArgb(21, 128, 61) : Color.FromArgb(185, 28, 28);
        }

        private void ResetSummaryCards()
        {
            Color neutral = Color.FromArgb(100, 116, 139);
            lblOpeningVal.Text = "–"; lblOpeningVal.Appearance.ForeColor = neutral;
            lblDebitVal.Text   = "–"; lblDebitVal.Appearance.ForeColor   = neutral;
            lblCreditVal.Text  = "–"; lblCreditVal.Appearance.ForeColor  = neutral;
            lblClosingVal.Text = "–"; lblClosingVal.Appearance.ForeColor = neutral;
        }

        private void SetStatus(string message)
        {
            if (lblStatus != null)
                lblStatus.Text = message;
        }

        // ════════════════════════════════════════════════════════════
        //  Event handlers
        // ════════════════════════════════════════════════════════════

        private void btnSearch_Click(object sender, EventArgs e)   => FetchFromDatabase();

        private void btnReset_Click(object sender, EventArgs e)
        {
            _isLoading = true;
            try
            {
                _selectedCustomerId   = 0;
                _selectedCustomerName = string.Empty;
                txtCustomerName.Text  = string.Empty;
                txtSearch.Text        = string.Empty;
                _allFetchedRows       = new List<CustomerLedgerReportRow>();
                _reportRows           = new List<CustomerLedgerReportRow>();
                gridReport.DataSource = null;

                _openingBalance = 0; _totalDebit = 0; _totalCredit = 0; _closingBalance = 0;
                InitializeDateControls();
                ultraComboPreset.Value     = "ThisMonth";

                ResetSummaryCards();
                SetStatus("Ready  |  Select a customer and press Search (F5)");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (_reportRows == null || _reportRows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export",
                                 MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                string safeCustomer = SanitizeFileName(_selectedCustomerName);
                using (var dlg = new SaveFileDialog())
                {
                    dlg.Filter   = "CSV Files (*.csv)|*.csv";
                    dlg.FileName = $"CustomerLedger_{safeCustomer}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                    if (dlg.ShowDialog() != DialogResult.OK) return;

                    var sb = new StringBuilder();
                    sb.AppendLine("Date,Voucher No,Voucher Type,Particulars,Narration,Debit (Dr),Credit (Cr),Running Balance");

                    foreach (var row in _reportRows)
                    {
                        sb.AppendLine(string.Join(",",
                            CsvCell(row.VoucherDate.ToString("yyyy-MM-dd")),
                            CsvCell(row.VoucherNo),
                            CsvCell(row.VoucherTypeName),
                            CsvCell(row.Particulars),
                            CsvCell(row.Narration),
                            row.ReceiptAmount.ToString("F2"),
                            row.PaymentAmount.ToString("F2"),
                            row.RunningBalance.ToString("F2")
                        ));
                    }

                    File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Report exported successfully!", "Export",
                                     MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (_reportRows == null || _reportRows.Count == 0)
            {
                MessageBox.Show("No data to print.", "Print",
                                 MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            gridReport.PrintPreview();
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();

        /// <summary>Opens frmCustomerDialog and stores the selection.</summary>
        private void btnSelectCustomer_Click(object sender, EventArgs e)
        {
            OpenCustomerDialog();
        }

        private void OpenCustomerDialog()
        {
            using (var dlg = new PosBranch_Win.DialogBox.frmCustomerDialog())
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                if (dlg.SelectedCustomerId <= 0) return;

                _selectedCustomerId   = dlg.SelectedCustomerId;
                _selectedCustomerName = dlg.SelectedCustomerName ?? string.Empty;
                txtCustomerName.Text  = _selectedCustomerName;

                // Auto-search once a customer is selected
                FetchFromDatabase();
            }
        }

        private void ultraComboPreset_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading || ultraComboPreset.Value == null) return;

            DateTime today = DateTime.Today;
            _isLoading = true;
            try
            {
                switch (ultraComboPreset.Value.ToString())
                {
                    case "Today":        dtFrom.Value = today;               dtTo.Value = today;               break;
                    case "Yesterday":    dtFrom.Value = today.AddDays(-1);   dtTo.Value = today.AddDays(-1);   break;
                    case "ThisWeek":     dtFrom.Value = today.AddDays(-(int)today.DayOfWeek); dtTo.Value = today; break;
                    case "ThisMonth":    dtFrom.Value = new DateTime(today.Year, today.Month, 1); dtTo.Value = today; break;
                    case "Last30Days":   dtFrom.Value = today.AddDays(-30);  dtTo.Value = today;               break;
                    case "Last3Months":  dtFrom.Value = today.AddMonths(-3); dtTo.Value = today;               break;
                    case "ThisYear":     dtFrom.Value = new DateTime(today.Year, 1, 1); dtTo.Value = today;    break;
                }
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void dtDate_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            _isLoading = true;
            try
            {
                ultraComboPreset.Value = "Custom";
            }
            finally
            {
                _isLoading = false;
            }
        }

        /// <summary>Text search → local in-memory filter only. No DB call.</summary>
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (_isLoading || _allFetchedRows.Count == 0) return;
            ApplyLocalFilter();
        }

        private void gridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            var band = e.Layout.Bands[0];

            // Hide internal ID
            if (band.Columns.Exists("VoucherID")) band.Columns["VoucherID"].Hidden = true;

            // Column order, widths, alignment
            SetCol(band, "VoucherDate",    "Date",            100, "dd/MM/yyyy", HAlign.Center, 0);
            SetCol(band, "VoucherNo",      "Voucher No",      115, null,         HAlign.Left,   1);
            SetCol(band, "VoucherTypeName","Type",            110, null,         HAlign.Left,   2);
            SetCol(band, "Particulars",    "Particulars",     200, null,         HAlign.Left,   3);
            SetCol(band, "Narration",      "Narration",       280, null,         HAlign.Left,   4);
            SetCol(band, "ReceiptAmount",  "Debit (Dr)",      115, "N2",         HAlign.Right,  5);
            SetCol(band, "PaymentAmount",  "Credit (Cr)",     115, "N2",         HAlign.Right,  6);
            SetCol(band, "RunningBalance", "Running Balance", 135, "N2",         HAlign.Right,  7);
        }

        private static void SetCol(UltraGridBand band, string key, string caption,
                                    int width, string fmt, HAlign align, int pos)
        {
            if (!band.Columns.Exists(key)) return;
            var col = band.Columns[key];
            col.Header.Caption            = caption;
            col.Header.VisiblePosition    = pos;
            col.Width                     = width;
            col.CellAppearance.TextHAlign = align;
            if (!string.IsNullOrEmpty(fmt))
                col.Format = fmt;
        }

        private void gridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (!e.Row.Cells.Exists("RunningBalance")) return;
            object val = e.Row.Cells["RunningBalance"].Value;
            if (val == null || val == DBNull.Value) return;

            decimal bal = Convert.ToDecimal(val);
            e.Row.Cells["RunningBalance"].Appearance.ForeColor =
                bal < 0 ? ThemeRedDark :
                bal > 0 ? ThemeGreenDark :
                          ThemeSlate;
        }

        private void frmCustomerLedgerReport_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:                              Close();             break;
                case Keys.F5:                                  FetchFromDatabase(); break;
                case Keys.F3:                                  OpenCustomerDialog(); break;
                case Keys.E when e.Control:                    btnExport_Click(this, e); break;
                case Keys.P when e.Control:                    btnPrint_Click(this, e);  break;
            }
        }

        // ════════════════════════════════════════════════════════════
        //  Static helpers
        // ════════════════════════════════════════════════════════════

        private static bool ContainsIgnoreCase(string source, string lower)
            => source != null && source.ToLowerInvariant().Contains(lower);

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Customer";
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c.ToString(), "");
            return name.Trim();
        }

        private static string CsvCell(string value)
        {
            string s = value ?? string.Empty;
            if (!s.Contains(",") && !s.Contains("\"") && !s.Contains("\n"))
                return s;
            return $"\"{s.Replace("\"", "\"\"")}\"";
        }
    }
}
