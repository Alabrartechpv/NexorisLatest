using Infragistics.Win;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using PosBranch_Win.Reports.FinancialReports;
using Repository;
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
    public partial class frmCounterReport : Form
    {
        private static readonly Color FormBackColor        = Color.FromArgb(232, 246, 255);
        private static readonly Color FilterPanelBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(206, 223, 238);
        private static readonly Color BorderBlue           = Color.FromArgb(118, 154, 198);
        private static readonly Color ControlBackColor     = Color.White;
        private static readonly Color ControlTextColor     = Color.FromArgb(18, 49, 102);
        private static readonly Color GridHeaderBlue       = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark   = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue     = Color.FromArgb(126, 126, 245);
        private static readonly Color GridRowLine          = Color.FromArgb(197, 217, 241);
        private static readonly Color GridAltRow           = Color.FromArgb(246, 250, 255);
        private static readonly Color GridFooterBorder     = Color.FromArgb(144, 181, 223);
        private static readonly Color ButtonBlueTop        = Color.FromArgb(232, 241, 252);
        private static readonly Color ButtonBlueBottom     = Color.FromArgb(145, 181, 224);
        private static readonly Color ButtonBlueBorder     = Color.FromArgb(62, 104, 166);
        private static readonly Color ButtonLightOutline   = Color.FromArgb(166, 183, 202);
        private static readonly Color SkyBlueOutline       = Color.FromArgb(160, 210, 255);
        private static readonly Color ButtonTextBlue       = Color.FromArgb(14, 47, 108);

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

                InitializePanels();
                StyleFilterControls();
                StyleButtons();
                SetupGrid();

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
            ultraGridCounterReport.UseAppStyling = false;
            ultraGridCounterReport.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = ultraGridCounterReport.DisplayLayout;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;
            layout.GroupByBox.Hidden = false;
            layout.GroupByBox.BandLabelAppearance.BackColor = GridHeaderBlueDark;
            layout.GroupByBox.BandLabelAppearance.ForeColor = Color.White;
            layout.GroupByBox.BandLabelAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.GroupByBox.PromptAppearance.BackColor = GridHeaderBlue;
            layout.GroupByBox.PromptAppearance.BackColor2 = GridHeaderBlueDark;
            layout.GroupByBox.PromptAppearance.BackGradientStyle = GradientStyle.Horizontal;
            layout.GroupByBox.PromptAppearance.ForeColor = Color.White;
            layout.GroupByBox.Prompt = "Drag a column header here to group by that column";
            layout.GroupByBox.Appearance.BackColor = Color.FromArgb(109, 167, 226);
            layout.GroupByBox.Appearance.BackColor2 = Color.FromArgb(69, 125, 190);
            layout.GroupByBox.Appearance.BackGradientStyle = GradientStyle.Vertical;

            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.AllowColMoving = AllowColMoving.WithinBand;
            layout.Override.AllowColSizing = AllowColSizing.Free;
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 25;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;

            layout.Appearance.BackColor = FormBackColor;
            layout.Appearance.BorderColor = BorderBlue;
            layout.Appearance.BackColor2 = FormBackColor;
            layout.Appearance.BackGradientStyle = GradientStyle.None;
            layout.Override.RowSelectorAppearance.BackColor = GridHeaderBlueDark;
            layout.Override.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
            layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.RowSelectorAppearance.BorderColor = BorderBlue;
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            layout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            layout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.BorderColor = BorderBlue;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;

            layout.Override.RowAppearance.BackColor = Color.White;
            layout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            layout.Override.RowAppearance.BorderColor = GridRowLine;
            layout.Override.RowAlternateAppearance.BorderColor = GridRowLine;
            layout.Override.ActiveRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.ActiveRowAppearance.ForeColor = Color.White;
            layout.Override.ActiveRowAppearance.BorderColor = BorderBlue;
            layout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;
            layout.Override.CellAppearance.BorderColor = GridRowLine;
            layout.Override.CellAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.Override.MinRowHeight = 20;
            layout.Override.DefaultRowHeight = 20;
            layout.RowConnectorStyle = RowConnectorStyle.Solid;
            layout.RowConnectorColor = GridRowLine;
            layout.ScrollBarLook.Appearance.BackColor = ActionPanelBackColor;
            layout.ScrollBarLook.Appearance.BorderColor = BorderBlue;
            layout.ScrollBarLook.TrackAppearance.BackColor = Color.FromArgb(225, 236, 246);
            layout.ScrollBarLook.ButtonAppearance.BackColor = GridHeaderBlue;
            layout.ScrollBarLook.ButtonAppearance.BackColor2 = GridHeaderBlueDark;
            layout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.ScrollBarLook.ButtonAppearance.BorderColor = BorderBlue;

            ultraGridCounterReport.BackColor = FormBackColor;
            ultraGridCounterReport.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            
            ultraGridCounterReport.InitializeLayout += ultraGridCounterReport_InitializeLayout;
        }

        private void InitializePanels()
        {
            BackColor = FormBackColor;
            ultraPanelMain.Appearance.BackColor = FormBackColor;

            ultraGroupBoxFilters.Appearance.BackColor = FilterPanelBackColor;
            ultraGroupBoxFilters.Appearance.BorderColor = BorderBlue;
            ultraGroupBoxFilters.BorderStyle = Infragistics.Win.Misc.GroupBoxBorderStyle.RectangularSolid;

            ultraGroupBoxGrid.Appearance.BackColor = FormBackColor;
            ultraGroupBoxGrid.BorderStyle = Infragistics.Win.Misc.GroupBoxBorderStyle.RectangularSolid;
            ultraGroupBoxGrid.HeaderAppearance.BackColor = GridHeaderBlueDark;
            ultraGroupBoxGrid.HeaderAppearance.ForeColor = Color.White;
            ultraGroupBoxGrid.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;

            ultraPanelSummary.Appearance.BackColor = GridHeaderBlue;
            ultraPanelSummary.Appearance.BorderColor = GridFooterBorder;
            ultraPanelSummary.BorderStyle = UIElementBorderStyle.Solid;
            
            lblTotalSessionsCaption.Text = "Total Bills:";
            lblTotalNetSalesCaption.Text = "Total Net Sales:";
            lblTotalCollectionCaption.Text = "Total Tax:";
            lblDifferenceCaption.Text = "Total Discount:";

            StyleSummaryLabel(lblTotalSessionsCaption);
            StyleSummaryLabel(lblTotalNetSalesCaption);
            StyleSummaryLabel(lblTotalCollectionCaption);
            StyleSummaryLabel(lblDifferenceCaption);
            
            StyleSummaryValueLabel(lblTotalSessionsValue);
            StyleSummaryValueLabel(lblTotalNetSalesValue);
            StyleSummaryValueLabel(lblTotalCollectionValue);
            StyleSummaryValueLabel(lblDifferenceValue);
        }

        private static void StyleSummaryLabel(Infragistics.Win.Misc.UltraLabel label)
        {
            if (label == null) return;
            label.Appearance.BackColor = Color.Transparent;
            label.Appearance.ForeColor = Color.White;
            label.Appearance.FontData.Bold = DefaultableBoolean.True;
            label.Appearance.FontData.Name = "Tahoma";
            label.Appearance.FontData.SizeInPoints = 9.5F;
            label.Appearance.TextHAlign = HAlign.Left;
        }

        private static void StyleSummaryValueLabel(Infragistics.Win.Misc.UltraLabel label)
        {
            if (label == null) return;
            label.Appearance.BackColor = Color.Transparent;
            label.Appearance.ForeColor = Color.White;
            label.Appearance.FontData.Bold = DefaultableBoolean.True;
            label.Appearance.FontData.Name = "Tahoma";
            label.Appearance.FontData.SizeInPoints = 12F;
            label.Appearance.TextHAlign = HAlign.Left;
        }

        private void StyleLabels()
        {
            StyleFilterLabel(lblFromDate);
            StyleFilterLabel(lblToDate);
            StyleFilterLabel(lblCounter);
            StyleFilterLabel(lblUser);
            StyleFilterLabel(lblQuickDate);
        }

        private static void StyleFilterLabel(Label label)
        {
            if (label == null) return;
            label.BackColor = Color.Transparent;
            label.ForeColor = Color.FromArgb(18, 47, 95);
            label.Font = new Font("Tahoma", 9.5F, FontStyle.Regular, GraphicsUnit.Point, 0);
        }

        private void StyleFilterControls()
        {
            StyleLabels();
            StyleFilterCombo(ultraComboPresetDates);
            StyleFilterCombo(ultraComboCounter);
            StyleFilterCombo(ultraComboUser);
            StyleDateEditor(ultraDateTimeFrom);
            StyleDateEditor(ultraDateTimeTo);
        }

        private static void StyleFilterCombo(UltraComboEditor combo)
        {
            if (combo == null) return;
            combo.UseAppStyling = false;
            combo.UseOsThemes = DefaultableBoolean.False;
            combo.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            combo.BorderStyle = UIElementBorderStyle.Solid;
            combo.Appearance.BackColor = ControlBackColor;
            combo.Appearance.BorderColor = SkyBlueOutline;
            combo.Appearance.ForeColor = ControlTextColor;
            combo.Appearance.FontData.Name = "Tahoma";
            combo.Appearance.FontData.SizeInPoints = 9.5F;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            combo.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;
        }

        private static void StyleDateEditor(UltraDateTimeEditor editor)
        {
            if (editor == null) return;
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = ControlBackColor;
            editor.Appearance.BorderColor = SkyBlueOutline;
            editor.Appearance.ForeColor = ControlTextColor;
            editor.Appearance.FontData.Name = "Tahoma";
            editor.Appearance.FontData.SizeInPoints = 9.5F;
            editor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            editor.MaskInput = "{date}";
            editor.FormatString = "dd/MM/yyyy";
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
            StyleClassicButton(btnSearch);
            StyleClassicButton(btnClear);
            StyleClassicButton(btnExport);
            StyleClassicButton(btnPrint);
            StyleClassicButton(btnClose);
        }

        private static void StyleClassicButton(Infragistics.Win.Misc.UltraButton button)
        {
            if (button == null) return;
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.ButtonStyle = UIElementButtonStyle.Flat;
            button.UseFlatMode = DefaultableBoolean.False;
            button.Appearance.BackColor = ButtonBlueTop;
            button.Appearance.BackColor2 = ButtonBlueBottom;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.ForeColor = ButtonTextBlue;
            button.Appearance.BorderColor = ButtonLightOutline;
            button.Appearance.TextHAlign = HAlign.Center;
            button.Appearance.TextVAlign = VAlign.Middle;
            button.Appearance.FontData.Bold = DefaultableBoolean.False;
            button.Appearance.FontData.SizeInPoints = 9;
            button.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button.HotTrackAppearance.BackColor = Color.FromArgb(241, 247, 254);
            button.HotTrackAppearance.BackColor2 = Color.FromArgb(166, 195, 231);
            button.HotTrackAppearance.BackGradientStyle = GradientStyle.Vertical;
            button.HotTrackAppearance.BorderColor = ButtonLightOutline;
            button.HotTrackAppearance.ForeColor = ButtonTextBlue;
            button.PressedAppearance.BackColor = Color.FromArgb(118, 161, 214);
            button.PressedAppearance.BackColor2 = Color.FromArgb(217, 231, 247);
            button.PressedAppearance.BackGradientStyle = GradientStyle.Vertical;
            button.PressedAppearance.BorderColor = Color.FromArgb(148, 163, 182);
            button.PressedAppearance.ForeColor = ButtonTextBlue;
        }

        private void ultraGridCounterReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count > 0)
            {
                UltraGridBand band = e.Layout.Bands[0];
                ConfigureColumn(band, "BillNo", "Bill No", 90);
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

                if (band.Columns.Exists("NetAmount"))
                {
                    band.Columns["NetAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["NetAmount"].CellAppearance.ForeColor = Color.FromArgb(46, 125, 50);
                }
            }

            e.Layout.AutoFitStyle = AutoFitStyle.None;
        }

        private static void ConfigureColumn(UltraGridBand band, string key, string caption, int width)
        {
            if (!band.Columns.Exists(key)) return;
            band.Columns[key].Header.Caption = caption;
            band.Columns[key].Width = width;
            band.Columns[key].Header.Appearance.TextHAlign = HAlign.Left;
            band.Columns[key].CellAppearance.FontData.Name = "Microsoft Sans Serif";
            band.Columns[key].CellAppearance.FontData.SizeInPoints = 8.25F;
        }

        private static void ConfigureDateColumn(UltraGridBand band, string key, string caption, int width)
        {
            if (!band.Columns.Exists(key)) return;
            band.Columns[key].Header.Caption = caption;
            band.Columns[key].Format = "dd-MMM-yyyy HH:mm";
            band.Columns[key].Width = width;
            band.Columns[key].CellAppearance.TextHAlign = HAlign.Left;
            band.Columns[key].Header.Appearance.TextHAlign = HAlign.Left;
            band.Columns[key].CellAppearance.FontData.Name = "Microsoft Sans Serif";
            band.Columns[key].CellAppearance.FontData.SizeInPoints = 8.25F;
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
            band.Columns[key].Header.Appearance.TextHAlign = HAlign.Right;
            band.Columns[key].CellAppearance.FontData.Name = "Microsoft Sans Serif";
            band.Columns[key].CellAppearance.FontData.SizeInPoints = 8.25F;
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
        }

        private void ClearSummary()
        {
            lblTotalSessionsValue.Text = "0";
            lblTotalNetSalesValue.Text = "₹ 0.00";
            lblTotalCollectionValue.Text = "₹ 0.00";
            lblDifferenceValue.Text = "₹ 0.00";
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

        public void RibbonClear() => btnClear_Click(this, EventArgs.Empty);
        public void Clear() => btnClear_Click(this, EventArgs.Empty);

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
            ShowReportFormatDialog(
                "COUNTER SESSION CLOSING REPORT",
                new[]
                {
                    "COUNTER SESSION CLOSING REPORT",
                    "COUNTER SESSION CLOSING - SUMMARY BY COUNTER",
                    "COUNTER SESSION CLOSING - CASHIER SUMMARY"
                });
        }

        private void ShowReportFormatDialog(string reportCaption, IEnumerable<string> formatDescriptions)
        {
            using (frmReportFormatDialog dialog = new frmReportFormatDialog(reportCaption, formatDescriptions))
            {
                dialog.ShowDialog(this);
            }
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
