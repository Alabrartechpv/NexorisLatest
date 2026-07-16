using System;
using System.Drawing;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Repository;
using Repository.ReportRepository;
using Repository.MasterRepositry;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ModelClass;
using ModelClass.Report;
using System.Text;
using System.IO;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class FrmCashBankBook : Form
    {
        private readonly CashBankBookRepository _repository;
        private readonly LedgerRepository _ledgerRepository;
        private BindingList<CashBankTransaction> _transactionsList;

        // Color constants for consistent theme
        private static readonly Color HeaderBackColor = Color.FromArgb(38, 50, 56);
        private static readonly Color HeaderBackColor2 = Color.FromArgb(55, 71, 79);
        private static readonly Color RowAltColor = Color.FromArgb(250, 250, 252);
        private static readonly Color ReceiptColor = Color.FromArgb(27, 94, 32);
        private static readonly Color PaymentColor = Color.FromArgb(198, 40, 40);
        private static readonly Color BalanceDrColor = Color.FromArgb(21, 101, 192);
        private static readonly Color BalanceCrColor = Color.FromArgb(198, 40, 40);
        private static readonly Color SelectedRowColor = Color.FromArgb(227, 242, 253);
        private Infragistics.Win.UltraWinEditors.UltraTextEditor txtSearch;
        private Infragistics.Win.Misc.UltraPanel panelChart;
        // private Infragistics.Win.UltraWinChart.UltraChart ultraChart; // Temporarily removed to fix assembly error

        public FrmCashBankBook()
        {
            InitializeComponent();
            _repository = new CashBankBookRepository();
            _ledgerRepository = new LedgerRepository();
            _transactionsList = new BindingList<CashBankTransaction>();

            // Event Handlers
            this.Load += FrmCashBankBook_Load;
            btnGenerate.Click += BtnGenerate_Click;
            btnExportCsv.Click += btnExportCsv_Click;
            btnPrint.Click += btnPrint_Click;
            btnClose.Click += (s, e) => this.Close();

            // UltraGrid events
            ultraGridTransactions.InitializeLayout += UltraGridTransactions_InitializeLayout;
            ultraGridTransactions.InitializeRow += UltraGridTransactions_InitializeRow;
            ultraGridTransactions.DoubleClickRow += UltraGridTransactions_DoubleClickRow;

            // Keyboard Shortcuts
            this.KeyPreview = true;
            this.KeyDown += FrmCashBankBook_KeyDown;
        }

        private void FrmCashBankBook_Load(object sender, EventArgs e)
        {
            // Apply professional theme
            SetupGrid();
            StyleButtons();
            StyleSummaryPanels();

            // Default date range
            dtFromDate.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dtToDate.Value = DateTime.Now.Date;

            // Load ledgers
            LoadLedgers();

            // Default quick date
            cmbDateQuickSelect.SelectedIndex = 1;
            cmbDateQuickSelect.ValueChanged += cmbDateQuickSelect_ValueChanged;
        }

        #region Grid Setup & Styling

        private void SetupGrid()
        {
            var grid = ultraGridTransactions;
            grid.DisplayLayout.Reset();

            // Read-only
            grid.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            grid.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            grid.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;

            // Selection
            grid.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            grid.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            grid.DisplayLayout.Override.RowSelectorWidth = 40;
            grid.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            grid.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;

            // Hide group-by box and caption
            grid.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            grid.DisplayLayout.GroupByBox.Hidden = true;

            // Row height
            grid.DisplayLayout.Override.MinRowHeight = 28;
            grid.DisplayLayout.Override.DefaultRowHeight = 28;

            // Row colors
            grid.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            grid.DisplayLayout.Override.RowAlternateAppearance.BackColor = RowAltColor;

            // Header (gradient)
            grid.DisplayLayout.Override.HeaderAppearance.BackColor = HeaderBackColor;
            grid.DisplayLayout.Override.HeaderAppearance.BackColor2 = HeaderBackColor2;
            grid.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            grid.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            grid.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            grid.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9.5f;
            grid.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
            grid.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

            // Selected row highlight
            grid.DisplayLayout.Override.SelectedRowAppearance.BackColor = SelectedRowColor;
            grid.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.Black;

            // Cell border
            grid.DisplayLayout.Override.CellAppearance.BorderColor = Color.FromArgb(230, 230, 230);
            grid.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
        }

        private void UltraGridTransactions_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            var band = e.Layout.Bands[0];

            // First hide all, then show only what we need
            foreach (UltraGridColumn col in band.Columns)
            {
                col.Hidden = true;
            }

            // ADD UNBOUND COLUMN FOR ICONS
            if (!band.Columns.Exists("IconCol"))
            {
                var iconCol = band.Columns.Add("IconCol", "");
                iconCol.DataType = typeof(string);
                iconCol.Header.VisiblePosition = 0;
                iconCol.Width = 30;
                iconCol.CellAppearance.TextHAlign = HAlign.Center;
            }

            // Configure visible columns in order
            ConfigureColumn(band, "VoucherDate", "Date", 100, HAlign.Center);
            if (band.Columns.Exists("VoucherDate"))
                band.Columns["VoucherDate"].Format = "dd-MMM-yyyy";

            ConfigureColumn(band, "VoucherID", "Voucher ID", 90, HAlign.Center);

            ConfigureColumn(band, "VoucherTypeName", "Type", 110, HAlign.Left);
            if (band.Columns.Exists("VoucherTypeName"))
                band.Columns["VoucherTypeName"].CellAppearance.ForeColor = Color.FromArgb(69, 90, 100);

            ConfigureColumn(band, "Particulars", "Particulars", 200, HAlign.Left);

            ConfigureColumn(band, "Narration", "Narration", 250, HAlign.Left);

            ConfigureColumn(band, "ReceiptAmount", "Receipts (Dr) ₹", 130, HAlign.Right);
            if (band.Columns.Exists("ReceiptAmount"))
            {
                band.Columns["ReceiptAmount"].Format = "N2";
                band.Columns["ReceiptAmount"].CellAppearance.ForeColor = ReceiptColor;
                band.Columns["ReceiptAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            ConfigureColumn(band, "PaymentAmount", "Payments (Cr) ₹", 130, HAlign.Right);
            if (band.Columns.Exists("PaymentAmount"))
            {
                band.Columns["PaymentAmount"].Format = "N2";
                band.Columns["PaymentAmount"].CellAppearance.ForeColor = PaymentColor;
                band.Columns["PaymentAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            ConfigureColumn(band, "FormattedBalance", "Running Balance", 150, HAlign.Right);
            if (band.Columns.Exists("FormattedBalance"))
            {
                band.Columns["FormattedBalance"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                band.Columns["FormattedBalance"].CellAppearance.FontData.SizeInPoints = 9f;
            }

            // Allow column resizing & auto-fit last column
            band.Override.AllowColSizing = AllowColSizing.Free;
            e.Layout.AutoFitStyle = AutoFitStyle.ExtendLastColumn;

            // Footer summaries
            band.Override.SummaryDisplayArea = SummaryDisplayAreas.BottomFixed;
            band.Override.SummaryFooterCaptionVisible = DefaultableBoolean.False;

            if (band.Columns.Exists("ReceiptAmount") && !band.Summaries.Exists("TotalReceipts"))
            {
                var s = band.Summaries.Add("TotalReceipts", SummaryType.Sum, band.Columns["ReceiptAmount"]);
                s.DisplayFormat = "₹ {0:N2}";
                s.Appearance.TextHAlign = HAlign.Right;
                s.Appearance.FontData.Bold = DefaultableBoolean.True;
                s.Appearance.ForeColor = ReceiptColor;
            }
            if (band.Columns.Exists("PaymentAmount") && !band.Summaries.Exists("TotalPayments"))
            {
                var s = band.Summaries.Add("TotalPayments", SummaryType.Sum, band.Columns["PaymentAmount"]);
                s.DisplayFormat = "₹ {0:N2}";
                s.Appearance.TextHAlign = HAlign.Right;
                s.Appearance.FontData.Bold = DefaultableBoolean.True;
                s.Appearance.ForeColor = PaymentColor;
            }
        }

        private void UltraGridTransactions_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            // Indicator Icons
            if (e.Row.Cells.Exists("IconCol"))
            {
                 e.Row.Cells["IconCol"].Value = (Convert.ToDecimal(e.Row.Cells["ReceiptAmount"].Value ?? 0) > 0) ? "💰" : "💸";
            }

            // Color the Running Balance (Dr = blue, Cr = red)
            if (e.Row.Cells.Exists("RunningBalance") && e.Row.Cells.Exists("FormattedBalance"))
            {
                var val = e.Row.Cells["RunningBalance"].Value;
                if (val != null && val != DBNull.Value)
                {
                    decimal balance = Convert.ToDecimal(val);
                    e.Row.Cells["FormattedBalance"].Appearance.ForeColor = balance >= 0 ? BalanceDrColor : BalanceCrColor;
                }
            }

            // Highlight zero amounts as light gray
            if (e.Row.Cells.Exists("ReceiptAmount"))
            {
                var val = e.Row.Cells["ReceiptAmount"].Value;
                if (val != null && val != DBNull.Value && Convert.ToDecimal(val) == 0)
                    e.Row.Cells["ReceiptAmount"].Appearance.ForeColor = Color.LightGray;
            }
            if (e.Row.Cells.Exists("PaymentAmount"))
            {
                var val = e.Row.Cells["PaymentAmount"].Value;
                if (val != null && val != DBNull.Value && Convert.ToDecimal(val) == 0)
                    e.Row.Cells["PaymentAmount"].Appearance.ForeColor = Color.LightGray;
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

        #region Extra Features (Search, Drill-Down, Chart)

        private void TxtSearch_ValueChanged(object sender, EventArgs e)
        {
            string filterText = txtSearch.Text.Trim().ToLower();
            
            if (string.IsNullOrEmpty(filterText))
            {
                ultraGridTransactions.DisplayLayout.Bands[0].ColumnFilters.ClearAllFilters();
                return;
            }

            var band = ultraGridTransactions.DisplayLayout.Bands[0];
            band.ColumnFilters.ClearAllFilters();
            band.ColumnFilters.LogicalOperator = FilterLogicalOperator.Or;
            
            if (band.Columns.Exists("Particulars")) band.ColumnFilters["Particulars"].FilterConditions.Add(FilterComparisionOperator.Contains, filterText);
            if (band.Columns.Exists("Narration")) band.ColumnFilters["Narration"].FilterConditions.Add(FilterComparisionOperator.Contains, filterText);
            if (band.Columns.Exists("VoucherTypeName")) band.ColumnFilters["VoucherTypeName"].FilterConditions.Add(FilterComparisionOperator.Contains, filterText);
            if (band.Columns.Exists("VoucherID")) band.ColumnFilters["VoucherID"].FilterConditions.Add(FilterComparisionOperator.Contains, filterText);
        }

        private void UltraGridTransactions_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            if (e.Row == null || !e.Row.IsDataRow) return;

            try
            {
                string voucherType = e.Row.Cells["VoucherTypeName"].Value?.ToString() ?? "";
                int voucherId = Convert.ToInt32(e.Row.Cells["VoucherID"].Value ?? 0);

                if (voucherId == 0) return;

                Form targetForm = null;

                // Open appropriate form based on type
                if (voucherType.Equals("Sales", StringComparison.OrdinalIgnoreCase))
                {
                    targetForm = new Transaction.frmSalesInvoice();
                    // Assumes Id assignment if property exists, or pass via constructor if available.
                    // For now, we will notify user drill down triggered.
                    MessageBox.Show($"[Drill-Down Triggered]\nOpening {voucherType} Voucher #{voucherId}\n(Routing to frmSalesInvoice...)", "Drill-Down", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (voucherType.Equals("Payment", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"[Drill-Down Triggered]\nOpening Payment Voucher #{voucherId}", "Drill-Down", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (voucherType.Equals("Receipt", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"[Drill-Down Triggered]\nOpening Receipt Voucher #{voucherId}", "Drill-Down", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void BuildChart(List<CashBankTransaction> transactions)
        {
            // Chart feature temporarily disabled due to missing assembly references.
            // Requires adding Infragistics.Win.UltraWinChart references to the project.
        }

        #endregion

        #region Summary Panel & Button Styling

        private void StyleSummaryPanels()
        {
            // Opening Balance — soft blue
            StyleSinglePanel(panelOpening, lblOpeningBalanceTitle, lblOpeningBalanceValue,
                Color.FromArgb(227, 242, 253), Color.FromArgb(21, 101, 192));

            // Total Receipts — green
            StyleSinglePanel(panelReceipts, lblTotalReceiptsTitle, lblTotalReceiptsValue,
                Color.FromArgb(232, 245, 233), ReceiptColor);

            // Total Payments — red
            StyleSinglePanel(panelPayments, lblTotalPaymentsTitle, lblTotalPaymentsValue,
                Color.FromArgb(255, 235, 238), PaymentColor);

            // Closing Balance — teal
            StyleSinglePanel(panelClosing, lblClosingBalanceTitle, lblClosingBalanceValue,
                Color.FromArgb(224, 242, 241), Color.FromArgb(0, 105, 92));
        }

        private void StyleSinglePanel(UltraPanel panel, UltraLabel titleLbl, UltraLabel valueLbl, Color bgColor, Color valueColor)
        {
            panel.Appearance.BackColor = bgColor;
            panel.Appearance.BorderColor = Color.FromArgb(200, 200, 200);
            panel.BorderStyle = UIElementBorderStyle.Solid;

            titleLbl.Appearance.ForeColor = Color.FromArgb(80, 80, 80);
            valueLbl.Appearance.ForeColor = valueColor;
        }

        private void StyleButtons()
        {
            StyleSingleButton(btnGenerate, Color.FromArgb(21, 101, 192), "▶ Generate");
            StyleSingleButton(btnExportCsv, Color.FromArgb(27, 94, 32), "⬇ CSV");
            StyleSingleButton(btnPrint, Color.FromArgb(74, 20, 140), "🖨 Print");
            StyleSingleButton(btnClose, Color.FromArgb(183, 28, 28), "✕ Close");
        }

        private void StyleSingleButton(UltraButton btn, Color backColor, string text)
        {
            btn.Text = text;
            btn.UseOsThemes = DefaultableBoolean.False;
            btn.Appearance.BackColor = backColor;
            btn.Appearance.ForeColor = Color.White;
            btn.Appearance.FontData.Bold = DefaultableBoolean.True;
            btn.Appearance.FontData.SizeInPoints = 9f;
            btn.Appearance.BorderColor = Color.FromArgb(
                Math.Max(0, backColor.R - 30),
                Math.Max(0, backColor.G - 30),
                Math.Max(0, backColor.B - 30));
            btn.ButtonStyle = UIElementButtonStyle.Flat;

            var hoverColor = Color.FromArgb(
                Math.Min(255, backColor.R + 25),
                Math.Min(255, backColor.G + 25),
                Math.Min(255, backColor.B + 25));

            btn.MouseEnterElement += (s, e) => btn.Appearance.BackColor = hoverColor;
            btn.MouseLeaveElement += (s, e) => btn.Appearance.BackColor = backColor;
        }

        #endregion

        #region Data Loading

        private void LoadLedgers()
        {
            try
            {
                var request = new AccountLedgerDDLRequest
                {
                    BranchId = SessionContext.BranchId,
                    For = "All"
                };
                var result = _ledgerRepository.getAccountLedgerDDL(request);

                if (result != null && result.List != null)
                {
                    var ledgerList = result.List.ToList();
                    ultraComboLedger.DataSource = ledgerList;
                    ultraComboLedger.ValueMember = "Id";
                    ultraComboLedger.DisplayMember = "Name";

                    ultraComboLedger.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortMulti;
                    ultraComboLedger.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading ledgers: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbDateQuickSelect_ValueChanged(object sender, EventArgs e)
        {
            DateTime today = DateTime.Now.Date;
            switch (cmbDateQuickSelect.Value?.ToString())
            {
                case "Today":
                    dtFromDate.Value = today;
                    dtToDate.Value = today;
                    break;
                case "This Month":
                    dtFromDate.Value = new DateTime(today.Year, today.Month, 1);
                    dtToDate.Value = today;
                    break;
                case "Last Month":
                    var firstDayLastMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                    dtFromDate.Value = firstDayLastMonth;
                    dtToDate.Value = firstDayLastMonth.AddMonths(1).AddDays(-1);
                    break;
                case "This Financial Year":
                    int startMonth = 4;
                    int startYear = today.Month >= startMonth ? today.Year : today.Year - 1;
                    dtFromDate.Value = new DateTime(startYear, startMonth, 1);
                    dtToDate.Value = today;
                    break;
            }
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            LoadData();
        }

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
                DateTime from = Convert.ToDateTime(dtFromDate.Value).Date;
                DateTime to = Convert.ToDateTime(dtToDate.Value).Date;

                var reportData = _repository.GetCashBankBook(ledgerId, from, to);

                // Bind Grid
                _transactionsList = new BindingList<CashBankTransaction>(reportData.Transactions);
                ultraGridTransactions.DataSource = _transactionsList;
                ultraGridTransactions.DataBind();

                // Bind Summary
                lblOpeningBalanceValue.Text = reportData.Summary.OpeningBalance.ToString("N2");
                lblTotalReceiptsValue.Text = reportData.Summary.TotalReceipts.ToString("N2");
                lblTotalPaymentsValue.Text = reportData.Summary.TotalPayments.ToString("N2");
                lblClosingBalanceValue.Text = reportData.Summary.ClosingBalance.ToString("N2");

                // Dynamic color for closing balance
                lblClosingBalanceValue.Appearance.ForeColor = reportData.Summary.ClosingBalance >= 0
                    ? Color.FromArgb(0, 105, 92)
                    : PaymentColor;

                // Build Chart
                BuildChart(reportData.Transactions);
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

        #endregion

        #region Actions (Export, Print, Shortcuts)

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            if (ultraGridTransactions.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "CSV|*.csv", FileName = "CashBank_Report.csv" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var csv = new StringBuilder();

                        // Headers
                        foreach (UltraGridColumn col in ultraGridTransactions.DisplayLayout.Bands[0].Columns)
                        {
                            if (!col.Hidden)
                                csv.Append($"\"{col.Header.Caption}\",");
                        }
                        csv.AppendLine();

                        // Data rows
                        foreach (UltraGridRow row in ultraGridTransactions.Rows)
                        {
                            foreach (UltraGridCell cell in row.Cells)
                            {
                                if (!cell.Column.Hidden)
                                {
                                    string text = (cell.Value?.ToString() ?? "").Replace("\"", "\"\"");
                                    csv.Append($"\"{text}\",");
                                }
                            }
                            csv.AppendLine();
                        }

                        File.WriteAllText(sfd.FileName, csv.ToString());
                        MessageBox.Show("Data exported successfully.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (ultraGridTransactions.Rows.Count == 0)
            {
                MessageBox.Show("No data to print.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var printDoc = new Infragistics.Win.UltraWinGrid.UltraGridPrintDocument();
                printDoc.Grid = ultraGridTransactions;
                printDoc.Header.TextLeft = "Cash & Bank Book — " + (ultraComboLedger.Text ?? "");
                printDoc.Header.TextRight = $"Period: {Convert.ToDateTime(dtFromDate.Value):dd-MMM-yyyy} to {Convert.ToDateTime(dtToDate.Value):dd-MMM-yyyy}";
                printDoc.Footer.TextCenter = "Page [Page #]";

                var previewDialog = new System.Windows.Forms.PrintPreviewDialog();
                previewDialog.Document = printDoc;
                previewDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Print failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FrmCashBankBook_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) this.Close();
            else if (e.KeyCode == Keys.F5) btnGenerate.PerformClick();
            else if (e.Control && e.KeyCode == Keys.E) btnExportCsv.PerformClick();
            else if (e.Control && e.KeyCode == Keys.P) btnPrint.PerformClick();
        }

        #endregion
    }
}
