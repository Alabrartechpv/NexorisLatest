using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static PosBranch_Win.Reports.FinancialReports.FinancialReportGrid;

namespace PosBranch_Win.Reports.FinancialReports
{
    public class datewisecashinhandreport : Form
    {
        internal const string Title = "Date-wise Cash in Hand Report";
        private const string LayoutKey = "DatewiseCashInHandMatrix";
        private const string MatrixTable = "CashMatrix";
        private const string VoucherType = "VoucherType";
        private const string CashLedger = "CashLedger";
        private const string DatePrefix = "Date_";
        private const string Total = "Total";
        private const int AllCashLedgers = 0;
        private const string AllCashLedgerCaption = "All Cash in Hand Ledgers";
        private const string AmountFormat = "#,##0.00";
        private const string BalanceFormat = "#,##0.00' Dr';#,##0.00' Cr';0.00";
        private static readonly Color ReceiptGreen = Color.FromArgb(27, 94, 32);

        private readonly UltraComboEditor ledgerSelector = new UltraComboEditor { Width = 230 };
        private readonly UltraComboEditor amountSelector = new UltraComboEditor { Width = 120 };
        private readonly UltraDateTimeEditor fromDate = new UltraDateTimeEditor();
        private readonly UltraDateTimeEditor toDate = new UltraDateTimeEditor();
        private readonly UltraGrid gridReport = new UltraGrid { Name = "gridReport", Dock = DockStyle.Fill };
        private readonly UltraPanel ultraPanelGridFooter = new UltraPanel { Name = "ultraPanelGridFooter", Dock = DockStyle.Bottom };
        private readonly FinancialReportGrid reportGrid;
        private readonly Label periodTotals = new Label
        {
            Dock = DockStyle.Top,
            Height = 30,
            Padding = new Padding(10, 0, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = ControlTextColor
        };
        private string loadedPeriod;
        private DateTime loadedFrom;
        private DateTime loadedTo;
        private List<CashVoucher> loadedVouchers;
        private bool isLoadingReport;

        public datewisecashinhandreport()
        {
            Name = nameof(datewisecashinhandreport);
            Text = Title;
            BackColor = FormBackColor;
            Font = new Font("Microsoft Sans Serif", 8.25F);
            ClientSize = new Size(1120, 650);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            KeyPreview = true;

            var filters = CreatePanel(DockStyle.Top, FilterPanelBackColor, 43);
            var filterFlow = CreateFlow(filters);
            AddFilter(filterFlow, "Ledger", ledgerSelector);
            AddFilter(filterFlow, "From", fromDate);
            AddFilter(filterFlow, "To", toDate);
            AddFilter(filterFlow, "Show", amountSelector);
            StyleFilterCombo(ledgerSelector, true);
            StyleFilterCombo(amountSelector, true);
            amountSelector.Items.Add(CashAmount.Net, "Net Movement");
            amountSelector.Items.Add(CashAmount.Receipts, "Receipts");
            amountSelector.Items.Add(CashAmount.Payments, "Payments");
            amountSelector.Value = CashAmount.Net;
            amountSelector.ValueChanged += (s, e) =>
            {
                if (!isLoadingReport && loadedVouchers != null)
                    BindMatrix();
            };
            ledgerSelector.Items.Add(AllCashLedgers, AllCashLedgerCaption);
            ledgerSelector.Value = AllCashLedgers;
            foreach (UltraDateTimeEditor editor in new[] { fromDate, toDate })
            {
                editor.Width = 125;
                editor.MaskInput = "{date}";
                editor.FormatString = "dd/MM/yyyy";
                StyleDateEditor(editor);
            }

            var actions = CreatePanel(DockStyle.Top, ActionPanelBackColor, 47);
            var actionFlow = CreateFlow(actions);
            AddAction(actionFlow, "View Grid", LoadReport);
            AddAction(actionFlow, "Preview Grid", () => Preview(false));
            AddAction(actionFlow, "Preview Report", () => Preview(true));
            AddAction(actionFlow, "Export Grid", ExportGrid);
            AddAction(actionFlow, "Expand All", () => SetVoucherExpansion(true));
            AddAction(actionFlow, "Collapse All", () => SetVoucherExpansion(false));
            AddAction(actionFlow, "Reset Filters", RibbonClear);
            var toggle = AddAction(actionFlow, "Hide Selection", null);
            toggle.Click += (s, e) =>
            {
                filters.Visible = !filters.Visible;
                toggle.Text = filters.Visible ? "Hide Selection" : "Show Selection";
            };

            var master = CreatePanel(DockStyle.Fill, FormBackColor, 0);
            master.ClientArea.Controls.Add(gridReport);
            master.ClientArea.Controls.Add(ultraPanelGridFooter);
            Controls.Add(master);
            Controls.Add(periodTotals);
            Controls.Add(actions);
            Controls.Add(filters);

            reportGrid = new FinancialReportGrid(this, gridReport, ultraPanelGridFooter, () => LayoutKey, Total);
            reportGrid.UseMatrixFooter(VoucherType, count => $"Total ({count} Types)", IsDateColumn);
            gridReport.InitializeLayout += InitializeGridLayout;
            gridReport.InitializeRow += InitializeGridRow;
            gridReport.DoubleClickRow += (s, e) =>
            {
                if (e.Row != null && e.Row.IsDataRow && e.Row.Band.Index == 0)
                    e.Row.Expanded = !e.Row.Expanded;
            };
            Load += (s, e) =>
            {
                reportGrid.Initialize();
                RibbonClear();
            };
            ledgerSelector.ValueChanged += (s, e) =>
            {
                if (ledgerSelector.Value is int)
                    LoadReport();
            };
            FormClosing += (s, e) => reportGrid.SaveLayout();
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape) Close();
                else if (e.KeyCode == Keys.F5) LoadReport();
                else if (e.Control && e.KeyCode == Keys.E) ExportGrid();
                else if (e.Control && e.KeyCode == Keys.P) Preview(false);
                else return;
                e.Handled = true;
                e.SuppressKeyPress = true;
            };
        }

        public void RibbonClear() => LoadReport(resetFilters: true);

        private void LoadReport() => LoadReport(resetFilters: false);

        private void LoadReport(bool resetFilters)
        {
            if (isLoadingReport)
                return;

            isLoadingReport = true;
            try
            {
                if (resetFilters)
                {
                    DateTime today = DateTime.Today;
                    fromDate.Value = new DateTime(today.Year, today.Month, 1);
                    toDate.Value = today;
                    ledgerSelector.Value = AllCashLedgers;
                    amountSelector.Value = CashAmount.Net;
                }

                if (!(fromDate.Value is DateTime from) || !(toDate.Value is DateTime to) || from.Date > to.Date)
                {
                    ClearReport();
                    MessageBox.Show(this, "Select a valid From and To date.", Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (from.Date < System.Data.SqlTypes.SqlDateTime.MinValue.Value)
                {
                    ClearReport();
                    MessageBox.Show(this, "The From date must be on or after 1 January 1753.", Title,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                UseWaitCursor = true;
                using (var repository = new CashBankBookRepository())
                using (DataTable ledgers = repository.GetBranchLedgers())
                {
                    int selected = PopulateLedgerSelector(ledgers);
                    DataRow[] selectedLedgers = ledgers.AsEnumerable().Where(ledger => selected == AllCashLedgers
                        ? !ledger.IsNull("GroupID") && Convert.ToInt32(ledger["GroupID"]) == (int)AccountGroup.CASH_IN_HAND
                        : Convert.ToInt32(ledger["LedgerID"]) == selected).ToArray();
                    decimal opening = 0, receipts = 0, payments = 0, closing = 0;
                    var vouchers = new List<CashVoucher>();
                    DateTime lastVoucherTime = to.Date.Add(new TimeSpan(0, 23, 59, 59, 997));
                    foreach (DataRow ledger in selectedLedgers)
                    {
                        int ledgerId = Convert.ToInt32(ledger["LedgerID"]);
                        var book = repository.GetCashBankBook(ledgerId, from.Date, lastVoucherTime);
                        opening += book.Summary.OpeningBalance;
                        receipts += book.Summary.TotalReceipts;
                        payments += book.Summary.TotalPayments;
                        closing += book.Summary.ClosingBalance;

                        string ledgerName = Convert.ToString(ledger["LedgerName"]);
                        vouchers.AddRange(book.Transactions.Select(transaction => new CashVoucher
                        {
                            LedgerName = ledgerName,
                            Transaction = transaction
                        }));
                    }

                    loadedFrom = from.Date;
                    loadedTo = to.Date;
                    loadedVouchers = vouchers;
                    string ledgerCaption = selected == AllCashLedgers
                        ? AllCashLedgerCaption
                        : Convert.ToString(selectedLedgers[0]["LedgerName"]);
                    loadedPeriod = $"{from:dd-MMM-yyyy} to {to:dd-MMM-yyyy}   |   {ledgerCaption}";
                    BindMatrix();
                    periodTotals.Text = selectedLedgers.Length == 0
                        ? "No Cash in Hand ledgers found for this branch."
                        : $"Period totals   |   Opening: {opening.ToString(BalanceFormat)}   |   Receipts: {receipts:N2}"
                          + $"   |   Payments: {payments:N2}   |   Closing: {closing.ToString(BalanceFormat)}";
                }
            }
            catch (Exception ex)
            {
                ClearReport();
                MessageBox.Show(this, "Unable to load the report. " + ex.Message, Title,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
                isLoadingReport = false;
            }
        }

        private int PopulateLedgerSelector(DataTable ledgers)
        {
            int selected = Convert.ToInt32(ledgerSelector.Value);
            ledgerSelector.Items.Clear();
            ledgerSelector.Items.Add(AllCashLedgers, AllCashLedgerCaption);
            bool selectionExists = selected == AllCashLedgers;
            foreach (DataRow ledger in ledgers.AsEnumerable().OrderBy(row => row.Field<string>("LedgerName")))
            {
                int ledgerId = Convert.ToInt32(ledger["LedgerID"]);
                ledgerSelector.Items.Add(ledgerId, Convert.ToString(ledger["LedgerName"]));
                selectionExists |= ledgerId == selected;
            }
            selected = selectionExists ? selected : AllCashLedgers;
            ledgerSelector.Value = selected;
            return selected;
        }

        private void ClearReport()
        {
            loadedVouchers = null;
            loadedPeriod = null;
            periodTotals.Text = string.Empty;
            gridReport.DataSource = null;
            reportGrid.DataBound();
        }

        private void BindMatrix()
        {
            var data = BuildMatrix(loadedVouchers, loadedFrom, loadedTo, (CashAmount)amountSelector.Value);
            reportGrid.ClearFilters();
            gridReport.SetDataBinding(data, MatrixTable);
            reportGrid.DataBound();
        }

        private static DataSet BuildMatrix(List<CashVoucher> vouchers, DateTime from, DateTime to, CashAmount amount)
        {
            var matrix = new DataTable(MatrixTable);
            matrix.Columns.Add(VoucherType, typeof(string));
            int dayCount = (to - from).Days + 1;
            // Match the purchase matrix's date-range behavior for long periods.
            var dates = dayCount <= 93
                ? Enumerable.Range(0, dayCount).Select(offset => from.AddDays(offset))
                : vouchers.Select(entry => entry.Transaction.VoucherDate.Date).Distinct().OrderBy(date => date);
            bool sameMonth = from.Year == to.Year && from.Month == to.Month;
            var dateColumns = new Dictionary<DateTime, DataColumn>();
            foreach (DateTime date in dates)
            {
                string key = DatePrefix + date.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
                var column = matrix.Columns.Add(key, typeof(decimal));
                column.DefaultValue = 0m;
                column.Caption = sameMonth ? date.Day.ToString() : date.ToString(from.Year == to.Year ? "dd-MMM" : "dd-MMM-yy");
                dateColumns.Add(date, column);
            }
            matrix.Columns.Add(Total, typeof(decimal));

            var details = new DataTable("Vouchers");
            details.Columns.Add(VoucherType, typeof(string));
            details.Columns.Add(nameof(CashBankTransaction.VoucherDate), typeof(DateTime));
            details.Columns.Add(CashLedger, typeof(string));
            details.Columns.Add(nameof(CashBankTransaction.VoucherID), typeof(int));
            details.Columns.Add(nameof(CashBankTransaction.VoucherNo), typeof(string));
            details.Columns.Add(nameof(CashBankTransaction.Particulars), typeof(string));
            details.Columns.Add(nameof(CashBankTransaction.Narration), typeof(string));
            details.Columns.Add(nameof(CashBankTransaction.ReceiptAmount), typeof(decimal));
            details.Columns.Add(nameof(CashBankTransaction.PaymentAmount), typeof(decimal));
            details.Columns.Add(nameof(CashBankTransaction.RunningBalance), typeof(decimal));

            var voucherTypes = vouchers.GroupBy(entry =>
            {
                string type = entry.Transaction.VoucherTypeName?.Trim();
                return string.IsNullOrEmpty(type) ? "(Unspecified)" : type;
            }, StringComparer.OrdinalIgnoreCase).OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase);

            foreach (var group in voucherTypes)
            {
                DataRow row = matrix.NewRow();
                row[VoucherType] = group.Key;
                decimal total = 0m;
                foreach (var entry in group.OrderBy(item => item.Transaction.VoucherDate).ThenBy(item => item.Transaction.VoucherID))
                {
                    CashBankTransaction transaction = entry.Transaction;
                    decimal value = transaction.ReceiptAmount;
                    if (amount == CashAmount.Payments)
                        value = transaction.PaymentAmount;
                    else if (amount == CashAmount.Net)
                        value -= transaction.PaymentAmount;

                    DataColumn dateColumn = dateColumns[transaction.VoucherDate.Date];
                    row[dateColumn] = (decimal)row[dateColumn] + value;
                    total += value;
                    details.Rows.Add(group.Key, transaction.VoucherDate, entry.LedgerName, transaction.VoucherID,
                        transaction.VoucherNo, transaction.Particulars, transaction.Narration, transaction.ReceiptAmount,
                        transaction.PaymentAmount, transaction.RunningBalance);
                }
                row[Total] = total;
                matrix.Rows.Add(row);
            }

            var data = new DataSet();
            data.Tables.Add(matrix);
            data.Tables.Add(details);
            data.Relations.Add("VoucherDetails", matrix.Columns[VoucherType], details.Columns[VoucherType], false);
            return data;
        }

        private void InitializeGridLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0 || !(gridReport.DataSource is DataSet source))
                return;

            e.Layout.AutoFitStyle = AutoFitStyle.None;
            foreach (UltraGridBand band in e.Layout.Bands)
            {
                foreach (UltraGridColumn column in band.Columns)
                {
                    if (column.IsChaptered)
                        continue;
                    column.Hidden = true;
                    column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                }
            }

            var matrix = e.Layout.Bands[0];
            reportGrid.ConfigureGridColumn(matrix, VoucherType, "Voucher Type", 240, null, HAlign.Left, 0);
            matrix.Columns[VoucherType].CellAppearance.FontData.Bold = DefaultableBoolean.True;

            int position = 1;
            foreach (DataColumn dateColumn in source.Tables[MatrixTable].Columns)
            {
                if (!IsDateColumn(dateColumn.ColumnName))
                    continue;

                int width = dateColumn.Caption.Length <= 2 ? 65 : 85;
                reportGrid.ConfigureGridColumn(matrix, dateColumn.ColumnName, dateColumn.Caption, width, AmountFormat, HAlign.Right, position++);
                DateTime date = DateTime.ParseExact(dateColumn.ColumnName.Substring(DatePrefix.Length), "yyyyMMdd", CultureInfo.InvariantCulture);
                matrix.Columns[dateColumn.ColumnName].Header.ToolTipText = $"{date:dd-MMM-yyyy (dddd)} ({amountSelector.Text})";
            }

            reportGrid.ConfigureGridColumn(matrix, Total, "Total", 105, AmountFormat, HAlign.Right, position);
            matrix.Columns[Total].Header.ToolTipText = "Total across dates";

            if (e.Layout.Bands.Count > 1)
            {
                var vouchers = e.Layout.Bands[1];
                ConfigureVoucherColumn(vouchers, nameof(CashBankTransaction.VoucherDate), "Date", 100, HAlign.Center, "dd-MMM-yyyy");
                ConfigureVoucherColumn(vouchers, CashLedger, "Ledger", 160, HAlign.Left);
                ConfigureVoucherColumn(vouchers, nameof(CashBankTransaction.VoucherID), "Voucher ID", 78, HAlign.Center);
                ConfigureVoucherColumn(vouchers, nameof(CashBankTransaction.VoucherNo), "Voucher No.", 105, HAlign.Left);
                ConfigureVoucherColumn(vouchers, nameof(CashBankTransaction.Particulars), "Particulars", 185, HAlign.Left);
                ConfigureVoucherColumn(vouchers, nameof(CashBankTransaction.Narration), "Narration", 190, HAlign.Left);
                ConfigureVoucherColumn(vouchers, nameof(CashBankTransaction.ReceiptAmount), "Receipts (Dr)", 105, HAlign.Right, "N2");
                ConfigureVoucherColumn(vouchers, nameof(CashBankTransaction.PaymentAmount), "Payments (Cr)", 105, HAlign.Right, "N2");
                ConfigureVoucherColumn(vouchers, nameof(CashBankTransaction.RunningBalance), "Running Balance", 115, HAlign.Right, BalanceFormat);
            }
            reportGrid.ApplySavedLayout();
        }

        private static bool IsDateColumn(string key) => key.StartsWith(DatePrefix, StringComparison.Ordinal);

        private void InitializeGridRow(object sender, InitializeRowEventArgs e)
        {
            if (e.Row.Band.Index != 0)
            {
                var receipt = e.Row.Cells[nameof(CashBankTransaction.ReceiptAmount)];
                if (receipt.Value is decimal received && received > 0m)
                    receipt.Appearance.ForeColor = ReceiptGreen;
                return;
            }

            bool showsCashIn = !Equals(amountSelector.Value, CashAmount.Payments);

            foreach (UltraGridCell cell in e.Row.Cells)
            {
                bool total = cell.Column.Key == Total;
                if ((!total && !IsDateColumn(cell.Column.Key)) || !(cell.Value is decimal amount))
                    continue;

                if (amount == 0m && !total)
                {
                    cell.Appearance.ForeColor = Color.FromArgb(170, 185, 205);
                    continue;
                }

                cell.Appearance.FontData.Bold = DefaultableBoolean.True;
                if (amount < 0m)
                    cell.Appearance.ForeColor = Color.FromArgb(191, 54, 12);
                else
                    cell.Appearance.ForeColor = showsCashIn && amount > 0m ? ReceiptGreen : Color.FromArgb(10, 45, 120);
                cell.Appearance.BackColor = total ? Color.FromArgb(238, 245, 255) : Color.FromArgb(242, 248, 255);
            }
        }

        private static void ConfigureVoucherColumn(UltraGridBand band, string key, string caption, int width,
            HAlign align, string format = null)
        {
            var column = band.Columns[key];
            column.Hidden = false;
            column.Header.Caption = caption;
            column.Header.VisiblePosition = band.Columns.Cast<UltraGridColumn>().Count(c => !c.Hidden) - 1;
            column.Width = width;
            column.Format = format ?? string.Empty;
            column.CellAppearance.TextHAlign = align;
        }

        private void SetVoucherExpansion(bool expanded)
        {
            foreach (UltraGridRow row in reportGrid.GetVisibleDataRows())
                row.Expanded = expanded;
        }

        private void Preview(bool includePeriodTotals)
        {
            if (!HasVisibleRows())
                return;

            try
            {
                using (var document = new UltraGridPrintDocument { Grid = gridReport })
                using (var dialog = new PrintPreviewDialog { Document = document, WindowState = FormWindowState.Maximized })
                {
                    document.DefaultPageSettings.Landscape = true;
                    document.Header.TextLeft = Title;
                    document.Header.TextRight = $"{loadedPeriod}   |   {amountSelector.Text}";
                    if (includePeriodTotals)
                        document.Footer.TextLeft = periodTotals.Text;
                    document.Footer.TextRight = "Page [Page #]";
                    dialog.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Preview failed. " + ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportGrid()
        {
            if (!HasVisibleRows())
                return;

            using (var dialog = new SaveFileDialog
            {
                Filter = ReportPdfExporter.CsvAndPdfFilter,
                FileName = $"DatewiseCashInHand_{DateTime.Today:yyyyMMdd}.csv"
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    if (ReportPdfExporter.CheckAndExportPdf(dialog, gridReport, Title, $"{loadedPeriod}   |   {amountSelector.Text}"))
                        return;

                    var columns = gridReport.DisplayLayout.Bands[0].Columns.Cast<UltraGridColumn>()
                        .Where(c => !c.Hidden && !c.IsChaptered).OrderBy(c => c.Header.VisiblePosition).ToList();
                    if (columns.Count == 0)
                        throw new InvalidOperationException("Show at least one column before exporting.");

                    using (var writer = new StreamWriter(dialog.FileName, false, new UTF8Encoding(true)))
                    {
                        writer.WriteLine(string.Join(",", columns.Select(c => QuoteCsv(c.Header.Caption))));
                        foreach (var row in reportGrid.GetVisibleDataRows())
                            writer.WriteLine(string.Join(",", columns.Select(c => QuoteCsv(FormatCsv(row.Cells[c.Key].Value)))));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Export failed. " + ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool HasVisibleRows()
        {
            if (reportGrid.GetVisibleDataRows().Any()) return true;
            MessageBox.Show(this, "No vouchers match the current selection.", Title,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        private static string FormatCsv(object value) => value is DateTime date
            ? date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : Convert.ToString(value, CultureInfo.InvariantCulture);

        private static string QuoteCsv(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";

        private static UltraPanel CreatePanel(DockStyle dock, Color color, int height)
        {
            var panel = new UltraPanel { Dock = dock, BorderStyle = UIElementBorderStyle.Solid };
            if (height > 0) panel.Height = height;
            panel.Appearance.BackColor = color;
            panel.Appearance.BorderColor = BorderBlue;
            return panel;
        }

        private static FlowLayoutPanel CreateFlow(UltraPanel parent)
        {
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8, 5, 0, 0), WrapContents = false };
            parent.ClientArea.Controls.Add(flow);
            return flow;
        }

        private static void AddFilter(FlowLayoutPanel panel, string caption, Control editor)
        {
            var label = new UltraLabel { Text = caption, AutoSize = true, Margin = new Padding(3, 7, 2, 0) };
            StyleLabel(label);
            panel.Controls.Add(label);
            panel.Controls.Add(editor);
        }

        private static UltraButton AddAction(FlowLayoutPanel panel, string caption, Action action)
        {
            var button = new UltraButton { Text = caption, Width = 110, Height = 29 };
            StyleClassicButton(button);
            if (action != null) button.Click += (s, e) => action();
            panel.Controls.Add(button);
            return button;
        }

        private enum CashAmount
        {
            Net,
            Receipts,
            Payments
        }

        private sealed class CashVoucher
        {
            public string LedgerName { get; set; }
            public CashBankTransaction Transaction { get; set; }
        }
    }
}
