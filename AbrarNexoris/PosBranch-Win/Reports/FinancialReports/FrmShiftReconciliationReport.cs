using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using Repository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.FinancialReports
{
    /// <summary>
    /// Modern Shift Cashier Reconciliation & Z-Report Audit Form
    /// </summary>
    public partial class FrmShiftReconciliationReport : Form
    {
        private readonly ClosingRepo _repository;
        private List<ClosingModel> _closingRecords;
        private bool _isLoading;

        public FrmShiftReconciliationReport()
        {
            _repository = new ClosingRepo();
            _closingRecords = new List<ClosingModel>();

            InitializeComponent();

            Load += FrmShiftReconciliationReport_Load;
            btnSearch.Click += BtnSearch_Click;
            btnClearFilters.Click += BtnClearFilters_Click;
            btnExport.Click += BtnExport_Click;
            btnPrint.Click += BtnPrint_Click;
            btnClose.Click += BtnClose_Click;
            ultraComboPreset.ValueChanged += UltraComboPreset_ValueChanged;
            txtSearch.ValueChanged += TxtSearch_ValueChanged;
            txtCounterFilter.ValueChanged += TxtCounterFilter_ValueChanged;
            gridReport.InitializeLayout += GridReport_InitializeLayout;
            gridReport.InitializeRow += GridReport_InitializeRow;
            gridReport.DoubleClickRow += GridReport_DoubleClickRow;

            KeyPreview = true;
            KeyDown += FrmShiftReconciliationReport_KeyDown;
        }

        private void FrmShiftReconciliationReport_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            _isLoading = true;

            try
            {
                this.Text = "Shift Reconciliation & Z-Report Audit";
                this.WindowState = FormWindowState.Maximized;
                this.StartPosition = FormStartPosition.CenterScreen;

                InitializeDateControls();
                InitializeSearchControls();
                InitializePanels();
                StyleButtons();
                SetupGrid();
                
                // Load data initially
                LoadReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            dtTo.Value = today.AddDays(1).AddTicks(-1); // End of today

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
            ultraComboPreset.Items.Add("Custom", "Custom Range");
            ultraComboPreset.Value = "ThisMonth";

            lblSearch.Visible = false;
            txtSearch.Visible = false;

            lblPreset.Location = new Point(12, 52);
            ultraComboPreset.Location = new Point(86, 49);

            txtCounterFilter.NullText = "Search Counter...";
        }

        private void InitializePanels()
        {
            ultraPanelMaster.BackColor = Color.FromArgb(250, 251, 252);
            ultraPanelMaster.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelControls.BackColor = Color.FromArgb(236, 240, 245);

            StyleSummaryLabel(lblSalesCaption, Color.FromArgb(25, 118, 210));
            StyleSummaryLabel(lblExpectedCaption, Color.FromArgb(13, 71, 161));
            StyleSummaryLabel(lblCountedCaption, Color.FromArgb(56, 142, 60));
            StyleSummaryLabel(lblVarianceCaption, Color.FromArgb(191, 54, 12));

            StyleSummaryValueLabel(lblSales, Color.FromArgb(25, 118, 210), 14);
            StyleSummaryValueLabel(lblExpected, Color.FromArgb(13, 71, 161), 14);
            StyleSummaryValueLabel(lblCounted, Color.FromArgb(27, 94, 32), 14);
            StyleSummaryValueLabel(lblVariance, Color.FromArgb(191, 54, 12), 16);

            StyleSummaryLabel(lblTotalCashSaleCaption, Color.FromArgb(70, 70, 70));
            StyleSummaryLabel(lblTotalCardSaleCaption, Color.FromArgb(70, 70, 70));
            StyleSummaryLabel(lblTotalUpiSaleCaption, Color.FromArgb(70, 70, 70));
            StyleSummaryLabel(lblTotalCreditSaleCaption, Color.FromArgb(70, 70, 70));
            StyleSummaryLabel(lblTotalCustReceiptCaption, Color.FromArgb(70, 70, 70));

            StyleSummaryValueLabel(lblTotalCashSale, Color.FromArgb(40, 40, 40), 12);
            StyleSummaryValueLabel(lblTotalCardSale, Color.FromArgb(40, 40, 40), 12);
            StyleSummaryValueLabel(lblTotalUpiSale, Color.FromArgb(40, 40, 40), 12);
            StyleSummaryValueLabel(lblTotalCreditSale, Color.FromArgb(40, 40, 40), 12);
            StyleSummaryValueLabel(lblTotalCustReceipt, Color.FromArgb(40, 40, 40), 12);
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
            layout.Override.MinRowHeight = 28;
            layout.Override.DefaultRowHeight = 28;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            
            layout.ScrollBounds = ScrollBounds.ScrollToFill;
            layout.ScrollStyle = ScrollStyle.Immediate;
        }

        private void LoadReport()
        {
            if (!ValidateDateRange())
                return;

            Cursor previousCursor = Cursor;
            Cursor = Cursors.WaitCursor;

            try
            {
                // Retrieve all shift closing history
                _closingRecords = _repository.GetShiftHistory() ?? new List<ClosingModel>();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load shift reconciliation report.\n{ex.Message}", "Report Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = previousCursor;
            }
        }

        private void ApplyFilters()
        {
            if (_closingRecords == null) return;

            DateTime fromDate = Convert.ToDateTime(dtFrom.Value).Date;
            DateTime toDate = Convert.ToDateTime(dtTo.Value).Date.AddDays(1).AddTicks(-1);

            IEnumerable<ClosingModel> filtered = _closingRecords;

            // Date filtering
            filtered = filtered.Where(x => x.TransactionDate >= fromDate && x.TransactionDate <= toDate);

            // Counter filter
            string counterText = txtCounterFilter.Text.Trim();
            if (!string.IsNullOrWhiteSpace(counterText))
            {
                filtered = filtered.Where(x => x.Counter != null && x.Counter.IndexOf(counterText, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            List<ClosingModel> boundRows = filtered.OrderByDescending(x => x.TransactionDate).ToList();
            gridReport.DataSource = boundRows;
            UpdateSummary(boundRows);
        }

        private void UpdateSummary(IList<ClosingModel> rows)
        {
            decimal totalNetSales = rows.Sum(x => x.NetSales);
            decimal totalExpected = rows.Sum(x => x.SystemExpectedCash);
            decimal totalPhysical = rows.Sum(x => x.PhysicalCashCounted);
            decimal totalVariance = rows.Sum(x => x.CashDifference);

            decimal totalCashSale = rows.Sum(x => x.CashSale);
            decimal totalCardSale = rows.Sum(x => x.CardSale);
            decimal totalUpiSale = rows.Sum(x => x.UpiSale);
            decimal totalCreditSale = rows.Sum(x => x.CreditSale);
            decimal totalCustReceipt = rows.Sum(x => x.CustomerReceipt);

            lblSales.Text = $"₹ {totalNetSales:N2}";
            lblExpected.Text = $"₹ {totalExpected:N2}";
            lblCounted.Text = $"₹ {totalPhysical:N2}";

            lblTotalCashSale.Text = $"₹ {totalCashSale:N2}";
            lblTotalCardSale.Text = $"₹ {totalCardSale:N2}";
            lblTotalUpiSale.Text = $"₹ {totalUpiSale:N2}";
            lblTotalCreditSale.Text = $"₹ {totalCreditSale:N2}";
            lblTotalCustReceipt.Text = $"₹ {totalCustReceipt:N2}";
            
            // Variance formatting (shortages in parenthesis or negative, color highlights)
            lblVariance.Text = $"₹ {totalVariance:N2}";
            if (totalVariance > 0)
            {
                lblVariance.Appearance.ForeColor = Color.Green;
            }
            else if (totalVariance < 0)
            {
                lblVariance.Appearance.ForeColor = Color.Red;
            }
            else
            {
                lblVariance.Appearance.ForeColor = Color.FromArgb(191, 54, 12);
            }
        }

        private bool ValidateDateRange()
        {
            if (dtFrom.Value == null || dtTo.Value == null)
            {
                MessageBox.Show("Please select valid dates.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

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

        private void ExportReport()
        {
            if (gridReport.Rows.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv";
                dialog.FileName = $"ShiftReconciliation_{DateTime.Now:yyyyMMdd_HHmmss}";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                Cursor previousCursor = Cursor;
                Cursor = Cursors.WaitCursor;

                try
                {
                    ExportCsv(dialog.FileName);
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

        private void ExportCsv(string filePath)
        {
            List<ClosingModel> rows = gridReport.DataSource as List<ClosingModel>;
            if (rows == null) return;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Closing Date,Counter,Gross Sales,Returns,Net Sales,Expected Cash,Physical Cash,Difference,Status");

            foreach (var row in rows)
            {
                builder.AppendLine(string.Join(",",
                    EscapeCsv(row.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss")),
                    EscapeCsv(row.Counter),
                    row.TotalGrossSales.ToString("F2"),
                    row.TotalReturn.ToString("F2"),
                    row.NetSales.ToString("F2"),
                    row.SystemExpectedCash.ToString("F2"),
                    row.PhysicalCashCounted.ToString("F2"),
                    row.CashDifference.ToString("F2"),
                    EscapeCsv(row.Status)
                ));
            }

            File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);
            MessageBox.Show("Report exported successfully.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static string EscapeCsv(string value)
        {
            string safeValue = value ?? string.Empty;
            if (!safeValue.Contains(",") && !safeValue.Contains("\"") && !safeValue.Contains("\n"))
                return safeValue;

            return $"\"{safeValue.Replace("\"", "\"\"")}\"";
        }

        private void PrintZReport()
        {
            if (gridReport.ActiveRow == null)
            {
                MessageBox.Show("Please select a closing record from the grid to print.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var selectedModel = gridReport.ActiveRow.ListObject as ClosingModel;
                if (selectedModel == null)
                {
                    MessageBox.Show("Selected row is not a valid closing record.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Cursor = Cursors.WaitCursor;

                // Load denominations detailed counted quantities
                var denominations = _repository.GetClosingDenominations(selectedModel.ShiftClosingId);
                selectedModel.CashDetails = denominations;

                // Set up and trigger printed report document
                PrintDocument printDoc = new PrintDocument();
                printDoc.DocumentName = $"Z-Report - Session {selectedModel.CounterSessionId}";

                printDoc.PrintPage += (s, ev) =>
                {
                    try
                    {
                        Font titleFont = new Font("Arial", 14, FontStyle.Bold);
                        Font subtitleFont = new Font("Arial", 11, FontStyle.Bold);
                        Font headerFont = new Font("Arial", 9.5f, FontStyle.Bold);
                        Font dataFont = new Font("Arial", 9);
                        Font boldFont = new Font("Arial", 9, FontStyle.Bold);

                        float y = 40;
                        float margin = 40;
                        float width = ev.PageBounds.Width - 80;

                        // Header
                        string title = "SHIFT CLOSING AUDIT REPORT (Z-REPORT)";
                        SizeF titleSize = ev.Graphics.MeasureString(title, titleFont);
                        ev.Graphics.DrawString(title, titleFont, Brushes.Black, (width - titleSize.Width) / 2 + margin, y);
                        y += 30;

                        ev.Graphics.DrawLine(Pens.Black, margin, y, width + margin, y);
                        y += 10;

                        // Info metadata
                        float colWidth = width / 2;
                        ev.Graphics.DrawString($"Session ID: {selectedModel.CounterSessionId}", boldFont, Brushes.Black, margin, y);
                        ev.Graphics.DrawString($"Counter: {selectedModel.Counter}", dataFont, Brushes.Black, margin + colWidth, y);
                        y += 18;

                        ev.Graphics.DrawString($"Closing Date: {selectedModel.TransactionDate:dd-MMM-yyyy HH:mm}", dataFont, Brushes.Black, margin, y);
                        ev.Graphics.DrawString($"Status: {selectedModel.Status}", dataFont, Brushes.Black, margin + colWidth, y);
                        y += 18;

                        ev.Graphics.DrawString($"Type: {selectedModel.ReportSelection}", dataFont, Brushes.Black, margin, y);
                        y += 15;

                        ev.Graphics.DrawLine(Pens.Gray, margin, y, width + margin, y);
                        y += 15;

                        // Sales section
                        ev.Graphics.DrawString("FINANCIAL SUMMARY", subtitleFont, Brushes.Black, margin, y);
                        y += 20;

                        float labelX = margin + 10;
                        float valX = margin + 280;

                        ev.Graphics.DrawString("Total Gross Sales:", dataFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.TotalGrossSales:N2}", dataFont, Brushes.Black, valX, y);
                        y += 18;

                        ev.Graphics.DrawString("Total Discount Given:", dataFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.TotalDiscount:N2}", dataFont, Brushes.Black, valX, y);
                        y += 18;

                        ev.Graphics.DrawString("Total Sales Returns:", dataFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.TotalReturn:N2}", dataFont, Brushes.Black, valX, y);
                        y += 18;

                        ev.Graphics.DrawString("Net Sales:", boldFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.NetSales:N2}", boldFont, Brushes.Black, valX, y);
                        y += 22;

                        ev.Graphics.DrawLine(Pens.LightGray, margin, y, width + margin, y);
                        y += 10;

                        // Collection breakdown
                        ev.Graphics.DrawString("PAYMENT COLLECTION BREAKDOWN", subtitleFont, Brushes.Black, margin, y);
                        y += 20;

                        ev.Graphics.DrawString("Cash Sales Collection:", dataFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.CashSale:N2}", dataFont, Brushes.Black, valX, y);
                        y += 18;

                        ev.Graphics.DrawString("Card Sales Collection:", dataFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.CardSale:N2}", dataFont, Brushes.Black, valX, y);
                        y += 18;

                        ev.Graphics.DrawString("UPI Sales Collection:", dataFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.UpiSale:N2}", dataFont, Brushes.Black, valX, y);
                        y += 18;

                        ev.Graphics.DrawString("Credit Sales (Unpaid):", dataFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.CreditSale:N2}", dataFont, Brushes.Black, valX, y);
                        y += 18;

                        ev.Graphics.DrawString("Customer Cash Receipts:", dataFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.CustomerReceipt:N2}", dataFont, Brushes.Black, valX, y);
                        y += 18;

                        ev.Graphics.DrawString("Total Collections Summary:", boldFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.TotalCollection:N2}", boldFont, Brushes.Black, valX, y);
                        y += 22;

                        ev.Graphics.DrawLine(Pens.LightGray, margin, y, width + margin, y);
                        y += 10;

                        // Reconciliation
                        ev.Graphics.DrawString("CASH RECONCILIATION", subtitleFont, Brushes.Black, margin, y);
                        y += 20;

                        ev.Graphics.DrawString("System Expected Cash:", dataFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.SystemExpectedCash:N2}", dataFont, Brushes.Black, valX, y);
                        y += 18;

                        ev.Graphics.DrawString("Physical Cash Counted:", boldFont, Brushes.Black, labelX, y);
                        ev.Graphics.DrawString($"₹ {selectedModel.PhysicalCashCounted:N2}", boldFont, Brushes.Black, valX, y);
                        y += 18;

                        string diffText = selectedModel.CashDifference >= 0 ? "Cash Excess (Surplus):" : "Cash Shortage (Deficit):";
                        Color diffColor = selectedModel.CashDifference >= 0 ? Color.Green : Color.Red;
                        using (Brush diffBrush = new SolidBrush(diffColor))
                        {
                            ev.Graphics.DrawString(diffText, boldFont, diffBrush, labelX, y);
                            ev.Graphics.DrawString($"₹ {Math.Abs(selectedModel.CashDifference):N2}", boldFont, diffBrush, valX, y);
                        }
                        y += 18;

                        if (!string.IsNullOrWhiteSpace(selectedModel.DifferenceReason))
                        {
                            ev.Graphics.DrawString($"Difference Reason: {selectedModel.DifferenceReason}", dataFont, Brushes.Black, labelX, y);
                            y += 18;
                        }
                        y += 22;

                        ev.Graphics.DrawLine(Pens.Black, margin, y, width + margin, y);
                        y += 15;

                        // Denominations Table
                        if (selectedModel.CashDetails != null && selectedModel.CashDetails.Any())
                        {
                            ev.Graphics.DrawString("COUNTED DENOMINATIONS DETAILS", subtitleFont, Brushes.Black, margin, y);
                            y += 25;

                            float dCol1 = margin;
                            float dCol2 = margin + 120;
                            float dCol3 = margin + 220;
                            float dCol4 = margin + 320;

                            ev.Graphics.DrawString("#", headerFont, Brushes.Black, dCol1, y);
                            ev.Graphics.DrawString("Denomination", headerFont, Brushes.Black, dCol2, y);
                            ev.Graphics.DrawString("Quantity", headerFont, Brushes.Black, dCol3, y);
                            ev.Graphics.DrawString("Amount", headerFont, Brushes.Black, dCol4, y);
                            y += 20;

                            ev.Graphics.DrawLine(Pens.Gray, margin, y, width + margin, y);
                            y += 10;

                            int rowNum = 1;
                            foreach (var detail in selectedModel.CashDetails)
                            {
                                ev.Graphics.DrawString(rowNum.ToString(), dataFont, Brushes.Black, dCol1, y);
                                ev.Graphics.DrawString($"₹{detail.Denomination:N2}", dataFont, Brushes.Black, dCol2, y);
                                ev.Graphics.DrawString(detail.Quantity.ToString(), dataFont, Brushes.Black, dCol3, y);
                                ev.Graphics.DrawString($"₹{detail.Amount:N2}", dataFont, Brushes.Black, dCol4, y);
                                y += 18;
                                rowNum++;
                            }

                            y += 10;
                            ev.Graphics.DrawLine(Pens.Black, margin, y, width + margin, y);
                            y += 15;
                        }

                        // Footer info
                        ev.Graphics.DrawString($"Printed on: {DateTime.Now:dd-MMM-yyyy HH:mm:ss} | Auditor Report", dataFont, Brushes.Gray, margin, y);
                    }
                    catch (Exception ex)
                    {
                        ev.Graphics.DrawString($"Error drawing report page: {ex.Message}", new Font("Arial", 9), Brushes.Red, 50, 50);
                    }
                };

                PrintDialog printDialog = new PrintDialog();
                printDialog.Document = printDoc;

                if (printDialog.ShowDialog(this) == DialogResult.OK)
                {
                    printDoc.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing report: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
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

        // ===== Events =====

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void BtnClearFilters_Click(object sender, EventArgs e)
        {
            _isLoading = true;

            try
            {
                DateTime today = DateTime.Today;
                dtFrom.Value = new DateTime(today.Year, today.Month, 1);
                dtTo.Value = today.AddDays(1).AddTicks(-1);
                ultraComboPreset.Value = "ThisMonth";
                txtSearch.Text = string.Empty;
                txtCounterFilter.Text = string.Empty;
            }
            finally
            {
                _isLoading = false;
            }

            _closingRecords = new List<ClosingModel>();
            gridReport.DataSource = null;
            UpdateSummary(new List<ClosingModel>());
            
            // Reload all
            LoadReport();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            ExportReport();
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            PrintZReport();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void UltraComboPreset_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading || ultraComboPreset.Value == null)
                return;

            string preset = ultraComboPreset.Value.ToString();
            DateTime today = DateTime.Today;

            switch (preset)
            {
                case "Today":
                    dtFrom.Value = today;
                    dtTo.Value = today.AddDays(1).AddTicks(-1);
                    break;
                case "Yesterday":
                    dtFrom.Value = today.AddDays(-1);
                    dtTo.Value = today.AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);
                    break;
                case "ThisWeek":
                    int daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
                    dtFrom.Value = today.AddDays(-daysSinceMonday);
                    dtTo.Value = today.AddDays(1).AddTicks(-1);
                    break;
                case "ThisMonth":
                    dtFrom.Value = new DateTime(today.Year, today.Month, 1);
                    dtTo.Value = today.AddDays(1).AddTicks(-1);
                    break;
                case "Last30Days":
                    dtFrom.Value = today.AddDays(-29);
                    dtTo.Value = today.AddDays(1).AddTicks(-1);
                    break;
            }

            LoadReport();
        }

        private void TxtSearch_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            ApplyFilters();
        }

        private void TxtCounterFilter_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            ApplyFilters();
        }

        private void GridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0)
                return;

            UltraGridBand band = e.Layout.Bands[0];
            foreach (UltraGridColumn column in band.Columns)
            {
                column.Hidden = true;
            }

            ConfigureGridColumn(band, "TransactionDate", "Closing Date & Time", 150, "dd-MMM-yyyy hh:mm tt", HAlign.Left);
            ConfigureGridColumn(band, "Counter", "Counter Name", 120, null, HAlign.Left);
            ConfigureGridColumn(band, "TotalGrossSales", "Gross Sales", 110, "#,##0.00", HAlign.Right);
            ConfigureGridColumn(band, "TotalReturn", "Returns", 100, "#,##0.00", HAlign.Right);
            ConfigureGridColumn(band, "NetSales", "Net Sales", 110, "#,##0.00", HAlign.Right);
            ConfigureGridColumn(band, "SystemExpectedCash", "Expected Cash", 115, "#,##0.00", HAlign.Right);
            ConfigureGridColumn(band, "PhysicalCashCounted", "Counted Cash", 115, "#,##0.00", HAlign.Right);
            ConfigureGridColumn(band, "CashDifference", "Cash Variance", 115, "#,##0.00", HAlign.Right);
            ConfigureGridColumn(band, "Status", "Status", 90, null, HAlign.Center);

            band.Columns["NetSales"].CellAppearance.ForeColor = Color.FromArgb(13, 71, 161);
            band.Columns["PhysicalCashCounted"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);

            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private void GridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (!e.Row.Cells.Exists("CashDifference"))
                return;

            decimal diff = 0m;
            if (e.Row.Cells["CashDifference"].Value != null)
            {
                decimal.TryParse(e.Row.Cells["CashDifference"].Value.ToString(), out diff);
            }

            e.Row.Cells["CashDifference"].Appearance.FontData.Bold = DefaultableBoolean.True;

            if (diff > 0)
            {
                e.Row.Cells["CashDifference"].Appearance.ForeColor = Color.Green;
            }
            else if (diff < 0)
            {
                e.Row.Cells["CashDifference"].Appearance.ForeColor = Color.Red;
            }
            else
            {
                e.Row.Cells["CashDifference"].Appearance.ForeColor = Color.FromArgb(96, 125, 139);
            }
        }

        private void GridReport_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            PrintZReport();
        }

        private void FrmShiftReconciliationReport_KeyDown(object sender, KeyEventArgs e)
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
