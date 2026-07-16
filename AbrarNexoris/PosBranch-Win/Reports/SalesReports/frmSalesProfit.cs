using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass.Report;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.SalesReports
{
    public partial class frmSalesProfit : Form
    {
        private SalesProfitReportRepository reportRepository;
        private List<SalesProfitReport> currentData;

        public frmSalesProfit()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            try
            {
                reportRepository = new SalesProfitReportRepository();
                Text = "Sales Profit Report";
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.CenterScreen;

                ultraComboPresetDates.Items.Clear();
                ultraComboPresetDates.Items.Add("Today", "Today");
                ultraComboPresetDates.Items.Add("Yesterday", "Yesterday");
                ultraComboPresetDates.Items.Add("ThisWeek", "This Week");
                ultraComboPresetDates.Items.Add("LastWeek", "Last Week");
                ultraComboPresetDates.Items.Add("ThisMonth", "This Month");
                ultraComboPresetDates.Items.Add("LastMonth", "Last Month");
                ultraComboPresetDates.Items.Add("ThisQuarter", "This Quarter");
                ultraComboPresetDates.Items.Add("LastQuarter", "Last Quarter");
                ultraComboPresetDates.Items.Add("ThisYear", "This Year");
                ultraComboPresetDates.Items.Add("LastYear", "Last Year");
                ultraComboPresetDates.Items.Add("Custom", "Custom Range");

                ultraComboPresetDates.Value = "ThisMonth";
                ultraDateTimeFrom.FormatString = "dd-MM-yyyy";
                ultraDateTimeTo.FormatString = "dd-MM-yyyy";
                ultraNumericBillNo.FormatString = "0";
                ultraNumericBillNo.Value = 0;

                SetupGrid();
                InitializePanels();
                StyleButtons();
                SetButtonHoverEffects();

                KeyPreview = true;
                KeyDown += Form_KeyDown;
                InitializeTooltips();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing form: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupGrid()
        {
            ultraGridProfit.DisplayLayout.Reset();
            ultraGridProfit.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            ultraGridProfit.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            ultraGridProfit.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
            ultraGridProfit.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            ultraGridProfit.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            ultraGridProfit.DisplayLayout.Override.RowSelectorWidth = 40;
            ultraGridProfit.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            ultraGridProfit.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            ultraGridProfit.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
            ultraGridProfit.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
            ultraGridProfit.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
            ultraGridProfit.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            ultraGridProfit.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
            ultraGridProfit.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            ultraGridProfit.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            ultraGridProfit.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            ultraGridProfit.DisplayLayout.GroupByBox.Hidden = true;
            ultraGridProfit.DisplayLayout.Override.MinRowHeight = 25;
            ultraGridProfit.DisplayLayout.Override.DefaultRowHeight = 25;
            ultraGridProfit.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(66, 165, 245);
            ultraGridProfit.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
            ultraGridProfit.DisplayLayout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.True;
            ultraGridProfit.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(55, 71, 79);
            ultraGridProfit.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(69, 90, 100);
            ultraGridProfit.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            ultraGridProfit.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            ultraGridProfit.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            ultraGridProfit.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
            ultraGridProfit.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            ultraGridProfit.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(250, 250, 252);
            ultraGridProfit.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(227, 242, 253);
            ultraGridProfit.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.FromArgb(33, 33, 33);
            ultraGridProfit.DisplayLayout.Override.ActiveRowAppearance.BorderColor = Color.FromArgb(66, 165, 245);
            ultraGridProfit.InitializeLayout += ultraGridProfit_InitializeLayout;
        }

        private void InitializePanels()
        {
            ultraPanelSummary.BackColor = Color.FromArgb(250, 251, 252);
            StyleSummaryLabel(lblTotalBillsCaption, Color.FromArgb(25, 118, 210), true);
            StyleSummaryLabel(lblTotalAmountCaption, Color.FromArgb(56, 142, 60), true);
            StyleSummaryLabel(lblTotalProfitCaption, Color.FromArgb(22, 160, 133), true);
            StyleSummaryLabel(lblProfitPercentCaption, Color.FromArgb(123, 31, 162), true);
            StyleSummaryValueLabel(lblTotalBillsValue, Color.FromArgb(13, 71, 161), 14);
            StyleSummaryValueLabel(lblTotalAmountValue, Color.FromArgb(27, 94, 32), 14);
            StyleSummaryValueLabel(lblTotalProfitValue, Color.FromArgb(22, 160, 133), 14);
            StyleSummaryValueLabel(lblProfitPercentValue, Color.FromArgb(74, 20, 140), 16);
        }

        private static void StyleSummaryLabel(Infragistics.Win.Misc.UltraLabel label, Color foreColor, bool isBold)
        {
            label.Appearance.ForeColor = foreColor;
            label.Appearance.FontData.Bold = isBold ? DefaultableBoolean.True : DefaultableBoolean.False;
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

        private void InitializeTooltips()
        {
            System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();
            toolTip.SetToolTip(ultraComboPresetDates, "Quick date range selection");
            toolTip.SetToolTip(ultraDateTimeFrom, "Select start date for the report");
            toolTip.SetToolTip(ultraDateTimeTo, "Select end date for the report");
            toolTip.SetToolTip(ultraNumericBillNo, "Enter specific bill number (0 = All)");
            toolTip.SetToolTip(btnSearch, "Search with current filters (F5)");
            toolTip.SetToolTip(btnClear, "Clear all filters");
            toolTip.SetToolTip(btnExport, "Export to CSV (Ctrl+E)");
            toolTip.SetToolTip(btnPrint, "Print report (Ctrl+P)");
            toolTip.SetToolTip(btnClose, "Close form (Escape)");
        }

        private void StyleButtons()
        {
            StyleButton(btnSearch, Color.FromArgb(25, 118, 210), Color.FromArgb(33, 150, 243), Color.FromArgb(21, 101, 192));
            StyleButton(btnClear, Color.FromArgb(245, 124, 0), Color.FromArgb(255, 152, 0), Color.FromArgb(230, 81, 0));
            StyleButton(btnExport, Color.FromArgb(0, 121, 107), Color.FromArgb(0, 150, 136), Color.FromArgb(0, 105, 92));
            StyleButton(btnPrint, Color.FromArgb(81, 45, 168), Color.FromArgb(103, 58, 183), Color.FromArgb(69, 39, 160));
            StyleButton(btnClose, Color.FromArgb(198, 40, 40), Color.FromArgb(229, 57, 53), Color.FromArgb(183, 28, 28));
        }

        private static void StyleButton(Infragistics.Win.Misc.UltraButton button, Color c1, Color c2, Color border)
        {
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.Appearance.BackColor = c1;
            button.Appearance.BackColor2 = c2;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.ForeColor = Color.White;
            button.Appearance.FontData.Bold = DefaultableBoolean.True;
            button.Appearance.FontData.SizeInPoints = 10;
            button.Appearance.BorderColor = border;
        }

        private void SetButtonHoverEffects()
        {
            btnSearch.HotTrackAppearance.BackColor = Color.FromArgb(66, 165, 245);
            btnSearch.HotTrackAppearance.ForeColor = Color.White;
            btnClear.HotTrackAppearance.BackColor = Color.FromArgb(255, 167, 38);
            btnClear.HotTrackAppearance.ForeColor = Color.White;
            btnExport.HotTrackAppearance.BackColor = Color.FromArgb(38, 166, 154);
            btnExport.HotTrackAppearance.ForeColor = Color.White;
            btnPrint.HotTrackAppearance.BackColor = Color.FromArgb(126, 87, 194);
            btnPrint.HotTrackAppearance.ForeColor = Color.White;
            btnClose.HotTrackAppearance.BackColor = Color.FromArgb(239, 83, 80);
            btnClose.HotTrackAppearance.ForeColor = Color.White;
        }

        private void ultraGridProfit_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count > 0)
            {
                UltraGridBand band = e.Layout.Bands[0];
                ConfigureColumn(band, "BillNo", "Bill No", 90);
                ConfigureDateColumn(band, "BillDate", "Bill Date", 110);
                ConfigureMoneyColumn(band, "BillAmount", "Bill Amount", 130, Color.FromArgb(27, 94, 32));
                ConfigureMoneyColumn(band, "Profit", "Profit", 130, Color.FromArgb(22, 160, 133));
                ConfigureColumn(band, "PayMode", "Pay Mode", 120);
                ConfigureColumn(band, "CashMode", "Cash Mode", 120);
            }

            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private static void ConfigureColumn(UltraGridBand band, string key, string caption, int width)
        {
            if (!band.Columns.Exists(key))
            {
                return;
            }

            band.Columns[key].Header.Caption = caption;
            band.Columns[key].Width = width;
        }

        private static void ConfigureDateColumn(UltraGridBand band, string key, string caption, int width)
        {
            if (!band.Columns.Exists(key))
            {
                return;
            }

            band.Columns[key].Header.Caption = caption;
            band.Columns[key].Format = "dd-MM-yyyy";
            band.Columns[key].Width = width;
        }

        private static void ConfigureMoneyColumn(UltraGridBand band, string key, string caption, int width, Color foreColor)
        {
            if (!band.Columns.Exists(key))
            {
                return;
            }

            band.Columns[key].Header.Caption = caption;
            band.Columns[key].Format = "#,##0.00";
            band.Columns[key].Width = width;
            band.Columns[key].CellAppearance.TextHAlign = HAlign.Right;
            band.Columns[key].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            band.Columns[key].CellAppearance.ForeColor = foreColor;
        }

        private void LoadData()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                DateTime fromDate = Convert.ToDateTime(ultraDateTimeFrom.Value);
                DateTime toDate = Convert.ToDateTime(ultraDateTimeTo.Value);
                int billNo = Convert.ToInt32(ultraNumericBillNo.Value ?? 0);

                currentData = reportRepository.GetSalesProfitReport(billNo, fromDate, toDate);

                if (currentData != null && currentData.Count > 0)
                {
                    ultraGridProfit.DataSource = currentData;
                    UpdateSummary();
                }
                else
                {
                    ultraGridProfit.DataSource = null;
                    ClearSummary();
                    MessageBox.Show("No records found for the selected criteria.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void UpdateSummary()
        {
            if (currentData == null || currentData.Count == 0)
            {
                ClearSummary();
                return;
            }

            int totalBills = currentData.Count;
            double totalAmount = currentData.Sum(x => x.BillAmount);
            double totalProfit = currentData.Sum(x => x.Profit);
            double profitPercent = totalAmount > 0 ? (totalProfit / totalAmount) * 100 : 0;

            lblTotalBillsValue.Text = totalBills.ToString("N0");
            lblTotalAmountValue.Text = totalAmount.ToString("N2");
            lblTotalProfitValue.Text = totalProfit.ToString("N2");
            lblProfitPercentValue.Text = profitPercent.ToString("N2") + " %";
        }

        private void ClearSummary()
        {
            lblTotalBillsValue.Text = "0";
            lblTotalAmountValue.Text = "0.00";
            lblTotalProfitValue.Text = "0.00";
            lblProfitPercentValue.Text = "0.00 %";
        }

        private void ultraComboPresetDates_ValueChanged(object sender, EventArgs e)
        {
            if (ultraComboPresetDates.Value == null)
            {
                return;
            }

            string preset = ultraComboPresetDates.Value.ToString();
            DateTime fromDate;
            DateTime toDate;

            switch (preset)
            {
                case "Today":
                    fromDate = DateTime.Now.Date;
                    toDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    break;
                case "Yesterday":
                    fromDate = DateTime.Now.AddDays(-1).Date;
                    toDate = DateTime.Now.AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    break;
                case "ThisWeek":
                    fromDate = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek).Date;
                    toDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    break;
                case "LastWeek":
                    fromDate = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek - 7).Date;
                    toDate = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek - 1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    break;
                case "ThisMonth":
                    fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    toDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    break;
                case "LastMonth":
                    fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);
                    toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);
                    break;
                case "ThisQuarter":
                    int quarter = (DateTime.Now.Month - 1) / 3 + 1;
                    fromDate = new DateTime(DateTime.Now.Year, (quarter - 1) * 3 + 1, 1);
                    toDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    break;
                case "LastQuarter":
                    int lastQuarter = (DateTime.Now.Month - 1) / 3;
                    if (lastQuarter == 0)
                    {
                        lastQuarter = 4;
                        fromDate = new DateTime(DateTime.Now.Year - 1, 10, 1);
                    }
                    else
                    {
                        fromDate = new DateTime(DateTime.Now.Year, (lastQuarter - 1) * 3 + 1, 1);
                    }
                    toDate = fromDate.AddMonths(3).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);
                    break;
                case "ThisYear":
                    fromDate = new DateTime(DateTime.Now.Year, 1, 1);
                    toDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    break;
                case "LastYear":
                    fromDate = new DateTime(DateTime.Now.Year - 1, 1, 1);
                    toDate = new DateTime(DateTime.Now.Year - 1, 12, 31).AddHours(23).AddMinutes(59).AddSeconds(59);
                    break;
                case "Custom":
                    return;
                default:
                    return;
            }

            ultraDateTimeFrom.Value = fromDate;
            ultraDateTimeTo.Value = toDate;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ultraDateTimeFrom.Value = DateTime.Now.AddDays(-30);
            ultraDateTimeTo.Value = DateTime.Now;
            ultraNumericBillNo.Value = 0;
            ultraGridProfit.DataSource = null;
            ClearSummary();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (currentData == null || currentData.Count == 0)
            {
                MessageBox.Show("No data to export.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV Files (*.csv)|*.csv";
                sfd.FileName = "SalesProfitReport_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Bill No,Bill Date,Bill Amount,Profit,Pay Mode,Cash Mode");

                    foreach (SalesProfitReport item in currentData)
                    {
                        sb.AppendLine(string.Format("{0},{1:dd/MM/yyyy},{2:N2},{3:N2},\"{4}\",\"{5}\"", item.BillNo, item.BillDate, item.BillAmount, item.Profit, item.PayMode, item.CashMode));
                    }

                    sb.AppendLine();
                    sb.AppendLine("Total Bills:," + currentData.Count);
                    sb.AppendLine("Total Amount:," + currentData.Sum(x => x.BillAmount).ToString("N2"));
                    sb.AppendLine("Total Profit:," + currentData.Sum(x => x.Profit).ToString("N2"));
                    File.WriteAllText(sfd.FileName, sb.ToString());
                    MessageBox.Show("Report exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (currentData == null || currentData.Count == 0)
            {
                MessageBox.Show("No data to print.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ultraGridProfit.Print();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                btnSearch_Click(sender, e);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                btnClose_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.E)
            {
                btnExport_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.P)
            {
                btnPrint_Click(sender, e);
                e.Handled = true;
            }
        }

        private void frmSalesProfit_Load(object sender, EventArgs e)
        {
            LoadData();
        }
    }
}
