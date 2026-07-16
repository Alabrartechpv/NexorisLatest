using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository.Accounts;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class frmCustomerOutstandingReport : Form
    {
        private readonly CustomerOutstandingReportRepository _repository;
        private List<CustomerOutstandingReportRow> _reportRows;
        private bool _isLoading;

        public frmCustomerOutstandingReport()
        {
            _repository = new CustomerOutstandingReportRepository();
            _reportRows = new List<CustomerOutstandingReportRow>();

            InitializeComponent();

            Load += frmCustomerOutstandingReport_Load;
            btnSearch.Click += btnSearch_Click;
            btnClearFilters.Click += btnClearFilters_Click;
            btnExport.Click += btnExport_Click;
            btnPrint.Click += btnPrint_Click;
            btnClose.Click += btnClose_Click;
            ultraComboPreset.ValueChanged += ultraComboPreset_ValueChanged;
            txtSearch.ValueChanged += txtSearch_ValueChanged;
            chkPositiveBalance.CheckedChanged += chkPositiveBalance_CheckedChanged;
            ultraComboCustomer.ValueChanged += ultraComboCustomer_ValueChanged;
            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.InitializeRow += gridReport_InitializeRow;
            gridReport.DoubleClickRow += gridReport_DoubleClickRow;

            KeyPreview = true;
            KeyDown += frmCustomerOutstandingReport_KeyDown;
        }

        private void frmCustomerOutstandingReport_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            _isLoading = true;

            try
            {
                Text = "Customer Outstanding Report";
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.CenterScreen;

                InitializeDateControls();
                InitializeSearchControls();
                InitializePanels();
                StyleButtons();
                SetupGrid();
                LoadCustomers();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void InitializeDateControls()
        {
            DateTime today = DateTime.Today;
            dtFrom.Value = new DateTime(today.Year, today.Month, 1);
            dtTo.Value = today;

            dtFrom.MaskInput = "{date}";
            dtTo.MaskInput = "{date}";
            dtFrom.FormatString = "dd/MM/yyyy";
            dtTo.FormatString = "dd/MM/yyyy";
        }

        private void InitializeSearchControls()
        {
            ultraComboPreset.Items.Clear();
            ultraComboPreset.Items.Add("Today", "Today");
            ultraComboPreset.Items.Add("Yesterday", "Yesterday");
            ultraComboPreset.Items.Add("ThisWeek", "This Week");
            ultraComboPreset.Items.Add("ThisMonth", "This Month");
            ultraComboPreset.Items.Add("Last30Days", "Last 30 Days");
            ultraComboPreset.Items.Add("Custom", "Custom");
            ultraComboPreset.Value = "ThisMonth";

            txtSearch.NullText = "Search bill no...";
            chkPositiveBalance.Checked = true;
        }

        private void InitializePanels()
        {
            ultraPanelMaster.BackColor = Color.FromArgb(250, 251, 252);
            ultraPanelMaster.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelControls.BackColor = Color.FromArgb(236, 240, 245);

            StyleSummaryLabel(lblCustomerCountCaption, Color.FromArgb(25, 118, 210));
            StyleSummaryLabel(lblOutstandingCaption, Color.FromArgb(13, 71, 161));
            StyleSummaryLabel(lblReceivedCaption, Color.FromArgb(56, 142, 60));
            StyleSummaryLabel(lblBalanceCaption, Color.FromArgb(191, 54, 12));

            StyleSummaryValueLabel(lblCustomerCount, Color.FromArgb(25, 118, 210), 14);
            StyleSummaryValueLabel(lblOutstanding, Color.FromArgb(13, 71, 161), 14);
            StyleSummaryValueLabel(lblReceived, Color.FromArgb(27, 94, 32), 14);
            StyleSummaryValueLabel(lblBalance, Color.FromArgb(191, 54, 12), 16);
        }

        private static void StyleSummaryLabel(Infragistics.Win.Misc.UltraLabel label, Color foreColor)
        {
            label.Appearance.ForeColor = foreColor;
            label.Appearance.FontData.Bold = DefaultableBoolean.True;
            label.Appearance.FontData.SizeInPoints = 10;
            label.Appearance.TextHAlign = HAlign.Left;
        }

        private static void StyleSummaryValueLabel(Infragistics.Win.Misc.UltraLabel label, Color foreColor, float fontSize)
        {
            label.Appearance.ForeColor = foreColor;
            label.Appearance.FontData.Bold = DefaultableBoolean.True;
            label.Appearance.FontData.SizeInPoints = fontSize;
            label.Appearance.TextHAlign = HAlign.Left;
        }

        private void StyleButtons()
        {
            StyleButton(btnSearch, Color.FromArgb(25, 118, 210), Color.FromArgb(33, 150, 243), Color.FromArgb(21, 101, 192));
            StyleButton(btnClearFilters, Color.FromArgb(245, 124, 0), Color.FromArgb(255, 152, 0), Color.FromArgb(230, 81, 0));
            StyleButton(btnPrint, Color.FromArgb(106, 27, 154), Color.FromArgb(142, 36, 170), Color.FromArgb(74, 20, 140));
            StyleButton(btnExport, Color.FromArgb(0, 121, 107), Color.FromArgb(0, 150, 136), Color.FromArgb(0, 105, 92));
            StyleButton(btnClose, Color.FromArgb(96, 125, 139), Color.FromArgb(120, 144, 156), Color.FromArgb(69, 90, 100));

            btnSearch.HotTrackAppearance.BackColor = Color.FromArgb(66, 165, 245);
            btnClearFilters.HotTrackAppearance.BackColor = Color.FromArgb(255, 167, 38);
            btnPrint.HotTrackAppearance.BackColor = Color.FromArgb(171, 71, 188);
            btnExport.HotTrackAppearance.BackColor = Color.FromArgb(38, 166, 154);
            btnClose.HotTrackAppearance.BackColor = Color.FromArgb(144, 164, 174);
        }

        private static void StyleButton(Infragistics.Win.Misc.UltraButton button, Color backColor1, Color backColor2, Color borderColor)
        {
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.Appearance.BackColor = backColor1;
            button.Appearance.BackColor2 = backColor2;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.ForeColor = Color.White;
            button.Appearance.FontData.Bold = DefaultableBoolean.True;
            button.Appearance.FontData.SizeInPoints = 10;
            button.Appearance.BorderColor = borderColor;
            button.HotTrackAppearance.ForeColor = Color.White;
        }

        private void SetupGrid()
        {
            gridReport.DisplayLayout.Reset();
            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = gridReport.DisplayLayout;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;
            layout.GroupByBox.Hidden = true;

            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 40;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;

            layout.Override.RowSelectorAppearance.BackColor = Color.FromArgb(69, 90, 100);
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            layout.Override.HeaderAppearance.BackColor = Color.FromArgb(55, 71, 79);
            layout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(69, 90, 100);
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 9;

            layout.Override.RowAppearance.BackColor = Color.White;
            layout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(248, 250, 252);
            layout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(227, 242, 253);
            layout.Override.ActiveRowAppearance.ForeColor = Color.FromArgb(33, 33, 33);
            layout.Override.ActiveRowAppearance.BorderColor = Color.FromArgb(66, 165, 245);
            layout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(66, 165, 245);
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;
            layout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.MinRowHeight = 25;
            layout.Override.DefaultRowHeight = 25;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
        }

        private void LoadCustomers()
        {
            List<CustomerGridList> customers = _repository.GetCustomers();

            ultraComboCustomer.Items.Clear();
            ultraComboCustomer.Items.Add(0, "-- Select Customer --");

            foreach (CustomerGridList customer in customers)
            {
                string displayText = string.IsNullOrWhiteSpace(customer.LedgerName)
                    ? customer.LedgerID.ToString()
                    : customer.LedgerName;

                ultraComboCustomer.Items.Add(customer.LedgerID, displayText);
            }

            ultraComboCustomer.Value = 0;
        }

        private void LoadReport()
        {
            int ledgerId = GetSelectedLedgerId();

            if (!ValidateDateRange())
                return;

            Cursor previousCursor = Cursor;
            Cursor = Cursors.WaitCursor;

            try
            {
                CustomerOutstandingReportFilter filter = new CustomerOutstandingReportFilter
                {
                    FromDate = Convert.ToDateTime(dtFrom.Value).Date,
                    ToDate = Convert.ToDateTime(dtTo.Value).Date,
                    CompanyId = SessionContext.CompanyId,
                    BranchId = SessionContext.BranchId,
                    FinYearId = SessionContext.FinYearId,
                    LedgerId = ledgerId
                };

                _reportRows = _repository.GetReport(filter);
                ApplyClientFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load customer outstanding report.\n{ex.Message}", "Report Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = previousCursor;
            }
        }

        private void ApplyClientFilters()
        {
            IEnumerable<CustomerOutstandingReportRow> filteredRows = _reportRows ?? Enumerable.Empty<CustomerOutstandingReportRow>();

            // Filter by bill no or ledger name search
            string searchText = GetSearchText();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredRows = filteredRows.Where(x =>
                    x.BillNo.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (x.LedgerName != null && x.LedgerName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                );
            }

            // Filter only positive balance
            if (chkPositiveBalance.Checked)
            {
                filteredRows = filteredRows.Where(x => x.Balance > 0);
            }

            List<CustomerOutstandingReportRow> boundRows = filteredRows
                .OrderBy(x => x.BillNo)
                .ToList();

            gridReport.DataSource = boundRows;
            UpdateSummary(boundRows);
        }

        private void UpdateSummary(IList<CustomerOutstandingReportRow> rows)
        {
            IList<CustomerOutstandingReportRow> safeRows = rows ?? new List<CustomerOutstandingReportRow>();

            lblCustomerCount.Text = safeRows.Count.ToString();
            lblOutstanding.Text = $"Rs. {safeRows.Sum(x => x.InvoiceAmount):N2}";
            lblReceived.Text = $"Rs. {safeRows.Sum(x => x.ReceivedAmount):N2}";
            lblBalance.Text = $"Rs. {safeRows.Sum(x => x.Balance):N2}";
        }

        private bool ValidateDateRange()
        {
            DateTime fromDate = Convert.ToDateTime(dtFrom.Value).Date;
            DateTime toDate = Convert.ToDateTime(dtTo.Value).Date;

            if (fromDate > toDate)
            {
                MessageBox.Show("From date cannot be greater than to date.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtFrom.Focus();
                return false;
            }

            return true;
        }

        private int GetSelectedLedgerId()
        {
            if (ultraComboCustomer.Value == null)
                return 0;

            int ledgerId;
            return int.TryParse(ultraComboCustomer.Value.ToString(), out ledgerId) ? ledgerId : 0;
        }

        private string GetSearchText()
        {
            string value = txtSearch.Value != null ? txtSearch.Value.ToString() : txtSearch.Text;
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private long GetSelectedBillNo()
        {
            if (gridReport.ActiveRow == null)
                return 0;

            if (!gridReport.ActiveRow.Cells.Exists("BillNo"))
                return 0;

            object billNoValue = gridReport.ActiveRow.Cells["BillNo"].Value;
            if (billNoValue == null || billNoValue == System.DBNull.Value)
                return 0;

            long billNo;
            return long.TryParse(billNoValue.ToString(), out billNo) ? billNo : 0;
        }

        private void PrintSelectedBill()
        {
            if (gridReport.Rows.Count == 0)
            {
                MessageBox.Show("There is no data to print.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Cursor previousCursor = Cursor;
                Cursor = Cursors.WaitCursor;

                try
                {
                    UltraGridPrintDocument printDocument = new UltraGridPrintDocument();
                    ConfigurePrintDocument(printDocument);

                    using (PrintPreviewDialog previewDialog = new PrintPreviewDialog())
                    {
                        previewDialog.Document = printDocument;
                        previewDialog.WindowState = FormWindowState.Maximized;
                        previewDialog.ShowDialog(this);
                    }
                }
                finally
                {
                    Cursor = previousCursor;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating PDF: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportReport()
        {
            if (gridReport.Rows.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv|PDF files (*.pdf)|*.pdf";
                dialog.FilterIndex = 1;
                dialog.FileName = $"CustomerOutstanding_{DateTime.Now:yyyyMMdd_HHmmss}";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                Cursor previousCursor = Cursor;
                Cursor = Cursors.WaitCursor;

                try
                {
                    if (dialog.FilterIndex == 1) // CSV
                    {
                        ExportCsvRaw(dialog.FileName);
                    }
                    else if (dialog.FilterIndex == 2) // PDF
                    {
                        ExportPdfRaw(dialog.FileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting report: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor = previousCursor;
                }
            }
        }

        private void ExportCsvRaw(string filePath)
        {
            List<CustomerOutstandingReportRow> rows = gridReport.DataSource as List<CustomerOutstandingReportRow>;
            if (rows == null) return;

            StringBuilder builder = new StringBuilder();
            
            // Check if LedgerName column should be included
            bool includeLedgerName = gridReport.DisplayLayout.Bands[0].Columns.Exists("LedgerName") && 
                                     !gridReport.DisplayLayout.Bands[0].Columns["LedgerName"].Hidden;

            if (includeLedgerName)
            {
                builder.AppendLine("Customer,Bill No,Bill Date,Due Date,Invoice Amount,Received Amount,Balance");
            }
            else
            {
                builder.AppendLine("Bill No,Bill Date,Due Date,Invoice Amount,Received Amount,Balance");
            }

            foreach (CustomerOutstandingReportRow row in rows)
            {
                if (includeLedgerName)
                {
                    builder.AppendLine(string.Join(",",
                        EscapeCsv(row.LedgerName),
                        row.BillNo.ToString(),
                        EscapeCsv(row.BillDate.HasValue ? row.BillDate.Value.ToString("yyyy-MM-dd") : string.Empty),
                        EscapeCsv(row.DueDate.HasValue ? row.DueDate.Value.ToString("yyyy-MM-dd") : string.Empty),
                        row.InvoiceAmount.ToString("F2"),
                        row.ReceivedAmount.ToString("F2"),
                        row.Balance.ToString("F2")));
                }
                else
                {
                    builder.AppendLine(string.Join(",",
                        row.BillNo.ToString(),
                        EscapeCsv(row.BillDate.HasValue ? row.BillDate.Value.ToString("yyyy-MM-dd") : string.Empty),
                        EscapeCsv(row.DueDate.HasValue ? row.DueDate.Value.ToString("yyyy-MM-dd") : string.Empty),
                        row.InvoiceAmount.ToString("F2"),
                        row.ReceivedAmount.ToString("F2"),
                        row.Balance.ToString("F2")));
                }
            }

            File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);
            MessageBox.Show("Report exported to CSV successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportPdfRaw(string filePath)
        {
            UltraGridPrintDocument printDocument = new UltraGridPrintDocument();
            ConfigurePrintDocument(printDocument);

            printDocument.PrinterSettings.PrinterName = "Microsoft Print to PDF";
            printDocument.PrinterSettings.PrintToFile = true;
            printDocument.PrinterSettings.PrintFileName = filePath;

            printDocument.Print();

            MessageBox.Show("Report exported to PDF successfully.", "Export PDF", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ConfigurePrintDocument(UltraGridPrintDocument printDocument)
        {
            printDocument.Grid = gridReport;
            
            printDocument.DefaultPageSettings.Landscape = true;
            printDocument.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(40, 40, 60, 60);

            string customerName = ultraComboCustomer.Value == null || (int)ultraComboCustomer.Value <= 0
                ? "All Customers"
                : ultraComboCustomer.Text;

            string headerText = $"Customer Outstanding Report{Environment.NewLine}" +
                                $"Customer: {customerName}";

            printDocument.Header.TextCenter = headerText;
            printDocument.Header.TextRight = $"Print Date: {DateTime.Now:dd-MMM-yyyy hh:mm tt}";

            printDocument.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
            printDocument.Header.Appearance.FontData.SizeInPoints = 12;
            printDocument.Header.Appearance.TextHAlign = HAlign.Center;
            printDocument.Header.Appearance.TextVAlign = VAlign.Middle;
            printDocument.Footer.TextCenter = "Page [Page #]";
            
            printDocument.FitWidthToPages = 1;
        }

        private static string EscapeCsv(string value)
        {
            string safeValue = value ?? string.Empty;
            if (!safeValue.Contains(",") && !safeValue.Contains("\"") && !safeValue.Contains("\n"))
                return safeValue;

            return $"\"{safeValue.Replace("\"", "\"\"")}\"";
        }

        private void ConfigureGridColumn(UltraGridBand band, string key, string header, int width, string format, HAlign align)
        {
            if (!band.Columns.Exists(key))
                return;

            UltraGridColumn column = band.Columns[key];
            column.Hidden = false;
            column.Header.Caption = header;
            column.Width = width;
            column.CellAppearance.TextHAlign = align;

            if (!string.IsNullOrWhiteSpace(format))
            {
                column.Format = format;
            }
        }

        // ===== Event Handlers =====

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            _isLoading = true;

            try
            {
                DateTime today = DateTime.Today;
                dtFrom.Value = new DateTime(today.Year, today.Month, 1);
                dtTo.Value = today;
                ultraComboPreset.Value = "ThisMonth";
                ultraComboCustomer.Value = 0;
                txtSearch.Text = string.Empty;
                chkPositiveBalance.Checked = true;
            }
            finally
            {
                _isLoading = false;
            }

            _reportRows = new List<CustomerOutstandingReportRow>();
            gridReport.DataSource = null;
            UpdateSummary(new List<CustomerOutstandingReportRow>());
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportReport();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            PrintSelectedBill();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ultraComboPreset_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading || ultraComboPreset.Value == null)
                return;

            string preset = ultraComboPreset.Value.ToString();
            DateTime today = DateTime.Today;

            switch (preset)
            {
                case "Today":
                    dtFrom.Value = today;
                    dtTo.Value = today;
                    break;
                case "Yesterday":
                    dtFrom.Value = today.AddDays(-1);
                    dtTo.Value = today.AddDays(-1);
                    break;
                case "ThisWeek":
                    int daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
                    dtFrom.Value = today.AddDays(-daysSinceMonday);
                    dtTo.Value = today;
                    break;
                case "ThisMonth":
                    dtFrom.Value = new DateTime(today.Year, today.Month, 1);
                    dtTo.Value = today;
                    break;
                case "Last30Days":
                    dtFrom.Value = today.AddDays(-29);
                    dtTo.Value = today;
                    break;
            }
        }

        private void txtSearch_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            ApplyClientFilters();
        }

        private void chkPositiveBalance_CheckedChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            ApplyClientFilters();
        }

        private void ultraComboCustomer_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            LoadReport();
        }

        private void gridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0)
                return;

            UltraGridBand band = e.Layout.Bands[0];
            foreach (UltraGridColumn column in band.Columns)
            {
                column.Hidden = true;
            }

            // Determine if we should show LedgerName (Only show when "All Customers" is selected)
            int selectedLedgerId = GetSelectedLedgerId();
            if (selectedLedgerId <= 0)
            {
                ConfigureGridColumn(band, "LedgerName", "Customer", 180, null, HAlign.Left);
            }

            ConfigureGridColumn(band, "BillNo", "Bill No", 100, null, HAlign.Left);
            ConfigureGridColumn(band, "BillDate", "Bill Date", 100, "dd-MMM-yyyy", HAlign.Left);
            ConfigureGridColumn(band, "DueDate", "Due Date", 100, "dd-MMM-yyyy", HAlign.Left);
            ConfigureGridColumn(band, "InvoiceAmount", "Invoice Amount", 120, "#,##0.00", HAlign.Right);
            ConfigureGridColumn(band, "ReceivedAmount", "Received Amount", 120, "#,##0.00", HAlign.Right);
            ConfigureGridColumn(band, "Balance", "Balance", 120, "#,##0.00", HAlign.Right);

            if (band.Columns.Exists("LedgerName"))
            {
                band.Columns["LedgerName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (band.Columns.Exists("BillNo"))
            {
                band.Columns["BillNo"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (band.Columns.Exists("InvoiceAmount"))
            {
                band.Columns["InvoiceAmount"].CellAppearance.ForeColor = Color.FromArgb(13, 71, 161);
            }

            if (band.Columns.Exists("ReceivedAmount"))
            {
                band.Columns["ReceivedAmount"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
            }

            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private void gridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (!e.Row.Cells.Exists("Balance"))
                return;

            decimal balance = 0m;
            if (e.Row.Cells["Balance"].Value != null)
            {
                decimal.TryParse(e.Row.Cells["Balance"].Value.ToString(), out balance);
            }

            e.Row.Cells["Balance"].Appearance.FontData.Bold = DefaultableBoolean.True;

            if (balance > 0)
            {
                e.Row.Cells["Balance"].Appearance.ForeColor = Color.FromArgb(191, 54, 12);
            }
            else if (balance < 0)
            {
                e.Row.Cells["Balance"].Appearance.ForeColor = Color.FromArgb(46, 125, 50);
            }
            else
            {
                e.Row.Cells["Balance"].Appearance.ForeColor = Color.FromArgb(96, 125, 139);
            }
        }

        private void gridReport_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            PrintSelectedBill();
        }

        private void frmCustomerOutstandingReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.E)
            {
                btnExport.PerformClick();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.P)
            {
                btnPrint.PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                btnSearch.PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F6)
            {
                btnClearFilters.PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                btnClose.PerformClick();
                e.Handled = true;
            }
        }
    }
}
