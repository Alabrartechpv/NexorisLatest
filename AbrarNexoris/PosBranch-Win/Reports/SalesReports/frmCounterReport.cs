using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass.Report;
using Repository;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ModelClass;

namespace PosBranch_Win.Reports.SalesReports
{
    public partial class frmCounterReport : Form
    {
        private CounterReportRepository _reportRepository;
        private Dropdowns _dropdowns;
        private List<CounterReportModel> _currentData;

        public frmCounterReport()
        {
            InitializeComponent();
            _reportRepository = new CounterReportRepository();
            _dropdowns = new Dropdowns();
            InitializeForm();
        }

        private void InitializeForm()
        {
            try
            {
                Text = "Counter Session Closing Report";
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.CenterScreen;

                // Configure preset date combo
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
                MessageBox.Show("Error initializing report: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool HasPrivilege()
        {
            string level = SessionContext.UserLevel ?? string.Empty;
            return level.IndexOf("admin", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   level.IndexOf("manager", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   level.IndexOf("supervisor", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void SetupGrid()
        {
            ultraGridCounterReport.DisplayLayout.Reset();
            ultraGridCounterReport.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            ultraGridCounterReport.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            ultraGridCounterReport.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
            ultraGridCounterReport.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            ultraGridCounterReport.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            ultraGridCounterReport.DisplayLayout.Override.RowSelectorWidth = 32;
            ultraGridCounterReport.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            ultraGridCounterReport.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortMulti;
            ultraGridCounterReport.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
            ultraGridCounterReport.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
            ultraGridCounterReport.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
            ultraGridCounterReport.DisplayLayout.Override.AllowRowFiltering = DefaultableBoolean.True;
            ultraGridCounterReport.DisplayLayout.Override.FilterUIType = FilterUIType.HeaderIcons;
            ultraGridCounterReport.DisplayLayout.Override.FilterOperatorLocation = FilterOperatorLocation.Hidden;
            ultraGridCounterReport.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            ultraGridCounterReport.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
            
            ultraGridCounterReport.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            ultraGridCounterReport.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            ultraGridCounterReport.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            ultraGridCounterReport.DisplayLayout.GroupByBox.Hidden = true;
            ultraGridCounterReport.DisplayLayout.Override.MinRowHeight = 26;
            ultraGridCounterReport.DisplayLayout.Override.DefaultRowHeight = 26;
            
            ultraGridCounterReport.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            ultraGridCounterReport.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(247, 250, 255);
            
            ultraGridCounterReport.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(120, 116, 235);
            ultraGridCounterReport.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;
            ultraGridCounterReport.DisplayLayout.Override.ActiveRowAppearance.BorderColor = Color.FromArgb(103, 142, 196);
            
            ultraGridCounterReport.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(120, 116, 235);
            ultraGridCounterReport.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
            ultraGridCounterReport.DisplayLayout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.True;
            
            ultraGridCounterReport.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(145, 179, 222);
            ultraGridCounterReport.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(118, 157, 209);
            ultraGridCounterReport.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            ultraGridCounterReport.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.FromArgb(17, 52, 102);
            ultraGridCounterReport.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            ultraGridCounterReport.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9.5F;
            ultraGridCounterReport.DisplayLayout.Override.HeaderAppearance.BorderColor = Color.FromArgb(103, 142, 196);
            
            ultraGridCounterReport.DisplayLayout.Override.CellAppearance.BorderColor = Color.FromArgb(210, 220, 235);
            ultraGridCounterReport.DisplayLayout.Override.FilterCellAppearance.BackColor = Color.White;
            ultraGridCounterReport.DisplayLayout.Override.FilterCellAppearance.BorderColor = Color.FromArgb(180, 198, 220);
            ultraGridCounterReport.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;
            ultraGridCounterReport.DisplayLayout.Override.WrapHeaderText = DefaultableBoolean.True;
            
            ultraGridCounterReport.InitializeLayout += ultraGridCounterReport_InitializeLayout;
        }

        private void InitializePanels()
        {
            ultraPanelSummary.BackColor = Color.FromArgb(240, 246, 252);
            
            lblTotalSessionsCaption.Text = "Total Bills:";
            lblTotalNetSalesCaption.Text = "Total Net Sales:";
            lblTotalCollectionCaption.Text = "Total Tax:";
            lblDifferenceCaption.Text = "Total Discount:";

            StyleSummaryLabel(lblTotalSessionsCaption, Color.FromArgb(17, 52, 102), true);
            StyleSummaryLabel(lblTotalNetSalesCaption, Color.FromArgb(17, 52, 102), true);
            StyleSummaryLabel(lblTotalCollectionCaption, Color.FromArgb(17, 52, 102), true);
            StyleSummaryLabel(lblDifferenceCaption, Color.FromArgb(17, 52, 102), true);
            
            StyleSummaryValueLabel(lblTotalSessionsValue, Color.FromArgb(72, 122, 214), 14);
            StyleSummaryValueLabel(lblTotalNetSalesValue, Color.FromArgb(72, 122, 214), 14);
            StyleSummaryValueLabel(lblTotalCollectionValue, Color.FromArgb(72, 122, 214), 14);
            StyleSummaryValueLabel(lblDifferenceValue, Color.FromArgb(72, 122, 214), 14);
        }

        private static void StyleSummaryLabel(Infragistics.Win.Misc.UltraLabel label, Color foreColor, bool isBold)
        {
            label.Appearance.ForeColor = foreColor;
            label.Appearance.FontData.Bold = isBold ? DefaultableBoolean.True : DefaultableBoolean.False;
            label.Appearance.FontData.SizeInPoints = 9.5F;
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
            toolTip.SetToolTip(ultraComboCounter, "Filter by specific Counter Name");
            toolTip.SetToolTip(ultraComboUser, "Filter by specific Cashier");
            toolTip.SetToolTip(btnSearch, "Search with current filters (F5)");
            toolTip.SetToolTip(btnClear, "Clear all filters");
            toolTip.SetToolTip(btnExport, "Export to CSV (Ctrl+E)");
            toolTip.SetToolTip(btnPrint, "Print report (Ctrl+P)");
            toolTip.SetToolTip(btnClose, "Close form (Escape)");
        }

        private void StyleButtons()
        {
            StyleButton(btnSearch, Color.FromArgb(72, 122, 214), Color.FromArgb(95, 145, 230), Color.FromArgb(72, 122, 214));
            StyleButton(btnClear, Color.FromArgb(84, 120, 190), Color.FromArgb(112, 148, 214), Color.FromArgb(84, 120, 190));
            StyleButton(btnExport, Color.FromArgb(94, 116, 202), Color.FromArgb(121, 141, 222), Color.FromArgb(94, 116, 202));
            StyleButton(btnPrint, Color.FromArgb(74, 130, 176), Color.FromArgb(104, 155, 196), Color.FromArgb(74, 130, 176));
            StyleButton(btnClose, Color.FromArgb(198, 40, 40), Color.FromArgb(229, 57, 53), Color.FromArgb(198, 40, 40));
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
            button.Appearance.FontData.SizeInPoints = 9.5F;
            button.Appearance.BorderColor = border;
        }

        private void SetButtonHoverEffects()
        {
            btnSearch.HotTrackAppearance.BackColor = Color.FromArgb(95, 145, 230);
            btnSearch.HotTrackAppearance.ForeColor = Color.White;
            btnClear.HotTrackAppearance.BackColor = Color.FromArgb(112, 148, 214);
            btnClear.HotTrackAppearance.ForeColor = Color.White;
            btnExport.HotTrackAppearance.BackColor = Color.FromArgb(121, 141, 222);
            btnExport.HotTrackAppearance.ForeColor = Color.White;
            btnPrint.HotTrackAppearance.BackColor = Color.FromArgb(104, 155, 196);
            btnPrint.HotTrackAppearance.ForeColor = Color.White;
            btnClose.HotTrackAppearance.BackColor = Color.FromArgb(229, 57, 53);
            btnClose.HotTrackAppearance.ForeColor = Color.White;
        }

        private void ultraGridCounterReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count > 0)
            {
                UltraGridBand band = e.Layout.Bands[0];
                ConfigureColumn(band, "BillNo", "Bill No", 80);
                ConfigureDateColumn(band, "BillDate", "Bill Date & Time", 145);
                ConfigureColumn(band, "Counter", "Counter", 100);
                ConfigureColumn(band, "UserName", "Cashier", 110);
                ConfigureColumn(band, "CustomerName", "Customer", 180);
                ConfigureColumn(band, "PaymodeName", "Bill Type", 90);
                ConfigureColumn(band, "CashMode", "Payment Mode", 120);

                ConfigureMoneyColumn(band, "SubTotal", "Sub Total", 110, Color.FromArgb(15, 23, 42));
                ConfigureMoneyColumn(band, "DiscountAmt", "Discount", 90, Color.FromArgb(198, 40, 40));
                ConfigureMoneyColumn(band, "TaxAmt", "Tax Amount", 100, Color.FromArgb(211, 84, 0));
                ConfigureMoneyColumn(band, "NetAmount", "Net Amount", 120, Color.FromArgb(56, 142, 60));

                ConfigureColumn(band, "Status", "Status", 90);
            }

            e.Layout.AutoFitStyle = AutoFitStyle.None;
        }

        private static void ConfigureColumn(UltraGridBand band, string key, string caption, int width)
        {
            if (!band.Columns.Exists(key)) return;
            band.Columns[key].Header.Caption = caption;
            band.Columns[key].Width = width;
            band.Columns[key].Header.Appearance.TextHAlign = HAlign.Center;
        }

        private static void ConfigureDateColumn(UltraGridBand band, string key, string caption, int width)
        {
            if (!band.Columns.Exists(key)) return;
            band.Columns[key].Header.Caption = caption;
            band.Columns[key].Format = "dd-MM-yyyy HH:mm";
            band.Columns[key].Width = width;
            band.Columns[key].CellAppearance.TextHAlign = HAlign.Center;
            band.Columns[key].Header.Appearance.TextHAlign = HAlign.Center;
        }

        private static void ConfigureMoneyColumn(UltraGridBand band, string key, string caption, int width, Color foreColor)
        {
            if (!band.Columns.Exists(key)) return;
            band.Columns[key].Header.Caption = caption;
            band.Columns[key].Format = "#,##0.00";
            band.Columns[key].Width = width;
            band.Columns[key].CellAppearance.TextHAlign = HAlign.Right;
            band.Columns[key].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            band.Columns[key].CellAppearance.ForeColor = foreColor;
            band.Columns[key].Header.Appearance.TextHAlign = HAlign.Center;
        }

        private void LoadFilters()
        {
            try
            {
                // Load Counter names
                var counters = _reportRepository.GetDistinctCounters();
                ultraComboCounter.Items.Clear();
                ultraComboCounter.Items.Add("", "--- All Counters ---");
                foreach (var counter in counters)
                {
                    ultraComboCounter.Items.Add(counter, counter);
                }
                ultraComboCounter.SelectedIndex = 0;

                // Load Users/Cashiers
                var usersResult = _dropdowns.getUsersDDl();
                ultraComboUser.Items.Clear();
                ultraComboUser.Items.Add(0, "--- All Cashiers ---");
                if (usersResult != null && usersResult.List != null)
                {
                    foreach (var user in usersResult.List)
                    {
                        ultraComboUser.Items.Add(user.UserID, user.UserName);
                    }
                }
                ultraComboUser.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dropdown filters: {ex.Message}");
            }
        }

        private void LoadData()
        {
            if (!HasPrivilege())
            {
                MessageBox.Show("Access Denied. You do not have supervisor or administrator permissions to view the Counter Report.",
                    "Security Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.BeginInvoke(new Action(this.Close));
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                DateTime fromDate = Convert.ToDateTime(ultraDateTimeFrom.Value);
                DateTime toDate = Convert.ToDateTime(ultraDateTimeTo.Value);
                
                string selectedCounter = ultraComboCounter.Value?.ToString() ?? "";
                int selectedUserId = Convert.ToInt32(ultraComboUser.Value ?? 0);

                _currentData = _reportRepository.GetCounterReportData(fromDate, toDate, selectedCounter, selectedUserId);

                if (_currentData != null && _currentData.Count > 0)
                {
                    ultraGridCounterReport.DataSource = _currentData;
                    UpdateSummary();
                }
                else
                {
                    ultraGridCounterReport.DataSource = null;
                    ClearSummary();
                    MessageBox.Show("No records found for the selected criteria.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading counter data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void UpdateSummary()
        {
            if (_currentData == null || _currentData.Count == 0)
            {
                ClearSummary();
                return;
            }

            int totalBills = _currentData.Count;
            decimal totalNetSales = _currentData.Sum(x => x.NetAmount);
            decimal totalTax = _currentData.Sum(x => x.TaxAmt);
            decimal totalDiscount = _currentData.Sum(x => x.DiscountAmt);

            lblTotalSessionsValue.Text = totalBills.ToString("N0");
            lblTotalNetSalesValue.Text = "₹ " + totalNetSales.ToString("N2");
            lblTotalCollectionValue.Text = "₹ " + totalTax.ToString("N2");
            lblDifferenceValue.Text = "₹ " + totalDiscount.ToString("N2");
            
            lblDifferenceValue.Appearance.ForeColor = Color.FromArgb(72, 122, 214);
        }

        private void ClearSummary()
        {
            lblTotalSessionsValue.Text = "0";
            lblTotalNetSalesValue.Text = "₹ 0.00";
            lblTotalCollectionValue.Text = "₹ 0.00";
            lblDifferenceValue.Text = "₹ 0.00";
            lblDifferenceValue.Appearance.ForeColor = Color.FromArgb(72, 122, 214);
        }

        private void ultraComboPresetDates_ValueChanged(object sender, EventArgs e)
        {
            if (ultraComboPresetDates.Value == null) return;

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
            ultraComboCounter.SelectedIndex = 0;
            ultraComboUser.SelectedIndex = 0;
            ultraGridCounterReport.DataSource = null;
            ClearSummary();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (_currentData == null || _currentData.Count == 0)
            {
                MessageBox.Show("No data to export.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV Files (*.csv)|*.csv";
                sfd.FileName = "CounterReport_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Bill No,Date & Time,Counter,Cashier,Customer,Bill Type,Payment Mode,Sub Total,Discount,Tax Amount,Net Amount,Status");

                    foreach (var item in _currentData)
                    {
                        sb.AppendLine(string.Format("\"{0}\",{1:dd/MM/yyyy HH:mm},\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",{7:F2},{8:F2},{9:F2},{10:F2},\"{11}\"",
                            item.BillNo, item.BillDate, item.Counter, item.UserName, item.CustomerName, item.PaymodeName, item.CashMode,
                            item.SubTotal, item.DiscountAmt, item.TaxAmt, item.NetAmount, item.Status));
                    }

                    sb.AppendLine();
                    sb.AppendLine("Total Bills:," + _currentData.Count);
                    sb.AppendLine("Total Net Sales:," + _currentData.Sum(x => x.NetAmount).ToString("F2"));
                    sb.AppendLine("Total Tax:," + _currentData.Sum(x => x.TaxAmt).ToString("F2"));
                    sb.AppendLine("Total Discount:," + _currentData.Sum(x => x.DiscountAmt).ToString("F2"));

                    File.WriteAllText(sfd.FileName, sb.ToString());
                    MessageBox.Show("Report exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (_currentData == null || _currentData.Count == 0)
            {
                MessageBox.Show("No data to print.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ultraGridCounterReport.Print();
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

        private void frmCounterReport_Load(object sender, EventArgs e)
        {
            if (!HasPrivilege())
            {
                MessageBox.Show("Access Denied. You do not have supervisor or administrator permissions to view the Counter Report.",
                    "Security Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
                return;
            }

            LoadFilters();
            LoadData();
        }
    }
}
