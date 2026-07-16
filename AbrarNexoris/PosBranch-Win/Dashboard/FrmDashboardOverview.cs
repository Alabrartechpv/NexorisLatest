using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinEditors;

namespace PosBranch_Win.Dashboard
{
    public partial class FrmDashboardOverview : Form
    {
        private static readonly Color DashboardBackColor = Color.FromArgb(230, 245, 253);
        private static readonly Color CardBackColor = Color.FromArgb(250, 253, 255);
        private static readonly Color CardHoverBackColor = Color.FromArgb(238, 247, 255);
        private static readonly Color CardBorderColor = Color.FromArgb(144, 181, 223);
        private static readonly Color CardHoverBorderColor = Color.FromArgb(93, 151, 214);
        private static readonly Color GridLineColor = Color.FromArgb(197, 217, 241);
        private static readonly Color TextBlue = Color.FromArgb(18, 49, 102);
        private static readonly Color StockTotalGreen = Color.FromArgb(48, 160, 70);
        private static readonly Color StockLowOrange = Color.FromArgb(238, 126, 22);
        private static readonly Color StockOutRed = Color.FromArgb(216, 48, 48);

        private DashboardOverview _overview;
        private DateTime _businessDate = DateTime.Today;
        private DateTime _rangeFromDate = DateTime.Today;
        private DateTime _rangeToDate = DateTime.Today;
        private DashboardOverviewRangeKind _rangeKind = DashboardOverviewRangeKind.Day;
        private readonly Action<Form, string> _openFormInTab;
        private readonly Timer _refreshTimer = new Timer();
        private readonly CultureInfo _culture = new CultureInfo("en-IN");
        private readonly HashSet<Panel> _cardPanels = new HashSet<Panel>();
        private readonly HashSet<Panel> _hoveredCards = new HashSet<Panel>();

        public FrmDashboardOverview()
            : this(null)
        {
        }

        public FrmDashboardOverview(Action<Form, string> openFormInTab)
        {
            _openFormInTab = openFormInTab;
            InitializeComponent();
            ConfigureDashboard();
            Load += FrmDashboardOverview_Load;
            Resize += FrmDashboardOverview_Resize;
            FormClosed += FrmDashboardOverview_FormClosed;
        }

        private void FrmDashboardOverview_Load(object sender, EventArgs e)
        {
            LoadDashboard();
            _refreshTimer.Interval = 60000;
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
        }

        private void ConfigureDashboard()
        {
            BackColor = DashboardBackColor;
            mainLayout.BackColor = DashboardBackColor;
            headerPanel.BackColor = DashboardBackColor;
            cardsLayout.BackColor = DashboardBackColor;
            middleLayout.BackColor = DashboardBackColor;
            bottomLayout.BackColor = DashboardBackColor;

            ConfigureMetricCard(cardSales, lblSalesIcon, lblSalesCardTitle, lblSalesValue, lblSalesFooter, Color.FromArgb(54, 126, 235), DashboardBadgeKind.Cart);
            ConfigureMetricCard(cardPurchase, lblPurchaseIcon, lblPurchaseCardTitle, lblPurchaseValue, lblPurchaseFooter, Color.FromArgb(86, 176, 69), DashboardBadgeKind.Basket);
            ConfigureMetricCard(cardSalesReturn, lblSalesReturnIcon, lblSalesReturnCardTitle, lblSalesReturnValue, lblSalesReturnFooter, Color.FromArgb(236, 99, 94), DashboardBadgeKind.Return);
            ConfigureMetricCard(cardPurchaseReturn, lblPurchaseReturnIcon, lblPurchaseReturnCardTitle, lblPurchaseReturnValue, lblPurchaseReturnFooter, Color.FromArgb(245, 141, 35), DashboardBadgeKind.Return);
            ConfigureMetricCard(cardReceipts, lblReceiptsIcon, lblReceiptsCardTitle, lblReceiptsValue, lblReceiptsFooter, Color.FromArgb(124, 78, 218), DashboardBadgeKind.Wallet);
            ConfigureMetricCard(cardPayments, lblPaymentsIcon, lblPaymentsCardTitle, lblPaymentsValue, lblPaymentsFooter, Color.FromArgb(31, 163, 181), DashboardBadgeKind.Payment);
            RegisterCardClick(cardSales, CardSales_Click);
            RegisterCardClick(cardPurchase, CardPurchase_Click);

            StylePanel(chartPanelWrapper);
            StylePanel(topItemsPanel);
            StylePanel(stockPanel);
            StylePanel(customerPanel);
            StylePanel(vendorPanel);
            StylePanel(duePanel);
            ConfigureBottomCard(stockPanel, lblStockIcon, lblStockTitle, Color.FromArgb(86, 126, 188), DashboardBadgeKind.Box);
            RegisterCardClick(stockPanel, CardStock_Click);
            ConfigureBottomCard(customerPanel, lblCustomerIcon, lblCustomerTitle, Color.FromArgb(77, 202, 72), DashboardBadgeKind.People);
            ConfigureBottomCard(vendorPanel, lblVendorIcon, lblVendorTitle, Color.FromArgb(64, 133, 238), DashboardBadgeKind.Truck);
            RegisterCardClick(customerPanel, CustomerPanel_Click);
            RegisterCardClick(vendorPanel, VendorPanel_Click);
            ConfigureBottomCard(duePanel, lblDueIcon, lblDueTitle, Color.FromArgb(245, 141, 35), DashboardBadgeKind.Wallet);

            StyleSmallTitle(lblStockTitle);
            StyleSmallTitle(lblCustomerTitle);
            StyleSmallTitle(lblVendorTitle);
            StyleSmallTitle(lblDueTitle);
            StyleBottomValueLabel(lblCustomerValue);
            StyleBottomValueLabel(lblVendorValue);
            StyleBottomCaption(lblCustomerCaption);
            StyleBottomCaption(lblVendorCaption);
            lblStockSummary.Visible = false;
            StyleStockSummaryLabel(lblStockTotalCaption);
            StyleStockSummaryLabel(lblStockLowCaption);
            StyleStockSummaryLabel(lblStockOutCaption);
            StyleStockValueLabel(lblStockTotalValue, StockTotalGreen);
            StyleStockValueLabel(lblStockLowValue, StockLowOrange);
            StyleStockValueLabel(lblStockOutValue, StockOutRed);
            ConfigureStockRowIcon(lblStockTotalIcon, StockTotalGreen, StockRowIconKind.Box);
            ConfigureStockRowIcon(lblStockLowIcon, StockLowOrange, StockRowIconKind.Alert);
            ConfigureStockRowIcon(lblStockOutIcon, StockOutRed, StockRowIconKind.Box);
            StyleBodyLabel(lblDueSummary);

            pnlSalesTrend.AutoScroll = true;
            pnlSalesTrend.Paint += PnlSalesTrend_Paint;
            pnlSalesTrend.Resize += (s, e) =>
            {
                ConfigureSalesTrendScroll();
                pnlSalesTrend.Invalidate();
            };
            dgvTopItems.AutoGenerateColumns = false;
            dgvTopItems.Columns.Clear();
            dgvTopItems.Columns.Add(CreateGridColumn("SlNo", "#", 42));
            dgvTopItems.Columns.Add(CreateGridColumn("ItemName", "Item Name", 170));
            dgvTopItems.Columns.Add(CreateGridColumn("Qty", "Qty", 70));
            dgvTopItems.Columns.Add(CreateGridColumn("AmountText", "Amount", 112));
            dgvTopItems.EnableHeadersVisualStyles = false;
            dgvTopItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(187, 214, 243);
            dgvTopItems.ColumnHeadersDefaultCellStyle.ForeColor = TextBlue;
            dgvTopItems.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
            dgvTopItems.DefaultCellStyle.Font = new Font("Segoe UI", 8.5F);
            dgvTopItems.DefaultCellStyle.ForeColor = Color.FromArgb(36, 64, 105);
            dgvTopItems.GridColor = GridLineColor;
            dgvTopItems.RowTemplate.Height = 25;
            dgvTopItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            ConfigureDashboardDateEditor(dtFrom, false);
            ConfigureDashboardDateEditor(dtTo, false);
            ConfigureQuickDateCombo();
            ConfigureApplyButton();
            btnApply.Click += BtnApply_Click;
            SetHeaderDateValue();
        }

        private void ConfigureApplyButton()
        {
            btnApply.UseVisualStyleBackColor = false;
            btnApply.FlatStyle = FlatStyle.Flat;
            btnApply.FlatAppearance.BorderSize = 0;
            btnApply.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 108, 211);
            btnApply.FlatAppearance.MouseDownBackColor = Color.FromArgb(31, 88, 181);
            btnApply.BackColor = Color.FromArgb(54, 126, 235);
            btnApply.ForeColor = Color.White;
            btnApply.Font = new Font("Segoe UI Semibold", 8.75F, FontStyle.Bold);
            btnApply.Paint -= ApplyButton_Paint;
            btnApply.Paint += ApplyButton_Paint;
            btnApply.Invalidate();
        }

        private void ApplyButton_Paint(object sender, PaintEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(54, 126, 235)))
                e.Graphics.FillRectangle(brush, btnApply.ClientRectangle);

            TextRenderer.DrawText(e.Graphics, btnApply.Text, btnApply.Font, btnApply.ClientRectangle,
                Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }

        private void CardSales_Click(object sender, EventArgs e)
        {
            OpenSalesAnalytics();
        }

        private void CardPurchase_Click(object sender, EventArgs e)
        {
            OpenPurchaseAnalytics();
        }

        private void CardStock_Click(object sender, EventArgs e)
        {
            OpenStockAnalytics();
        }

        private void CustomerPanel_Click(object sender, EventArgs e)
        {
            DashboardDrilldownPopup.ShowGrid(this, "All Customers",
                _overview != null
                    ? _overview.Customers.Select((x, index) => new PartyPopupRow { No = index + 1, Name = x.Name })
                    : Enumerable.Empty<PartyPopupRow>());
        }

        private void VendorPanel_Click(object sender, EventArgs e)
        {
            DashboardDrilldownPopup.ShowGrid(this, "All Vendors",
                _overview != null
                    ? _overview.Vendors.Select((x, index) => new PartyPopupRow { No = index + 1, Name = x.Name })
                    : Enumerable.Empty<PartyPopupRow>());
        }

        private class PartyPopupRow
        {
            public int No { get; set; }
            public string Name { get; set; }
        }

        private void OpenSalesAnalytics()
        {
            FrmSalesAnalytics analytics = new FrmSalesAnalytics();
            if (_openFormInTab != null)
            {
                _openFormInTab(analytics, "Sales Analytics");
                return;
            }

            analytics.StartPosition = FormStartPosition.CenterScreen;
            analytics.Show();
        }

        private void OpenPurchaseAnalytics()
        {
            FrmPurchaseAnalytics analytics = new FrmPurchaseAnalytics();
            if (_openFormInTab != null)
            {
                _openFormInTab(analytics, "Purchase Analytics");
                return;
            }

            analytics.StartPosition = FormStartPosition.CenterScreen;
            analytics.Show();
        }

        private void OpenStockAnalytics()
        {
            FrmStockAnalytics analytics = new FrmStockAnalytics();
            if (_openFormInTab != null)
            {
                _openFormInTab(analytics, "Stock Analytics");
                return;
            }

            analytics.StartPosition = FormStartPosition.CenterScreen;
            analytics.Show();
        }

        private DataGridViewTextBoxColumn CreateGridColumn(string propertyName, string headerText, int width)
        {
            return new DataGridViewTextBoxColumn
            {
                DataPropertyName = propertyName,
                HeaderText = headerText,
                Name = propertyName,
                Width = width,
                ReadOnly = true
            };
        }

        private void LoadDashboard()
        {
            try
            {
                using (DashboardOverviewRepository repository = new DashboardOverviewRepository())
                {
                    _overview = repository.GetOverview(_rangeFromDate, _rangeToDate, _rangeKind);
                }

                BindDashboard();
            }
            catch (Exception ex)
            {
                _overview = new DashboardOverview { BusinessDate = _rangeFromDate, FromDate = _rangeFromDate, ToDate = _rangeToDate, RangeKind = _rangeKind, GeneratedAt = DateTime.Now };
                BindDashboard();
                MessageBox.Show("Dashboard data could not be loaded.\n\n" + ex.Message, "Overview", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BindDashboard()
        {
            _rangeFromDate = _overview.FromDate == DateTime.MinValue ? _rangeFromDate : _overview.FromDate;
            _rangeToDate = _overview.ToDate == DateTime.MinValue ? _rangeToDate : _overview.ToDate;
            _rangeKind = _overview.RangeKind;
            _businessDate = _rangeFromDate;

            SetHeaderDateValue();
            lblSubTitle.Text = string.IsNullOrWhiteSpace(_overview.BranchName)
                ? "Business snapshot for " + FormatRangeLabel(_rangeFromDate, _rangeToDate, _rangeKind).ToLowerInvariant() + "."
                : _overview.BranchName + " business snapshot generated at " + _overview.GeneratedAt.ToString("hh:mm tt");

            lblSalesCardTitle.Text = MetricTitle("Total Sales", _overview.SalesCount);
            lblPurchaseCardTitle.Text = MetricTitle("Total Purchase", _overview.PurchaseCount);
            lblSalesReturnCardTitle.Text = MetricTitle("Sales Return", _overview.SalesReturnCount);
            lblPurchaseReturnCardTitle.Text = MetricTitle("Purchase Return", _overview.PurchaseReturnCount);
            lblReceiptsCardTitle.Text = MetricTitle("Total Receipts", _overview.ReceiptsCount);
            lblPaymentsCardTitle.Text = MetricTitle("Total Payments", _overview.PaymentsCount);
            lblSalesValue.Text = Money(_overview.TotalSales);
            lblSalesFooter.Text = PreviousPeriodCaption() + " - " + Money(_overview.YesterdaySales);
            lblPurchaseValue.Text = Money(_overview.TotalPurchase);
            lblPurchaseFooter.Text = PreviousPeriodCaption() + " - " + Money(_overview.YesterdayPurchase);
            lblSalesReturnValue.Text = Money(_overview.TotalSalesReturn);
            lblSalesReturnFooter.Text = "Selected range returns";
            lblPurchaseReturnValue.Text = Money(_overview.TotalPurchaseReturn);
            lblPurchaseReturnFooter.Text = "Selected range returns";
            lblReceiptsValue.Text = Money(_overview.TotalReceipts);
            lblReceiptsFooter.Text = "Voucher receipts";
            lblPaymentsValue.Text = Money(_overview.TotalPayments);
            lblPaymentsFooter.Text = "Voucher payments";
            lblChartTitle.Text = "Sales Trend (" + RangeKindCaption() + ")";
            lblTopItemsTitle.Text = "Top Selling Items (" + FormatRangeLabel(_rangeFromDate, _rangeToDate, _rangeKind) + ")";
            chartPanelWrapper.Visible = true;
            pnlSalesTrend.Visible = true;
            ConfigureSalesTrendScroll();

            lblStockTotalCaption.Text = "Total Items";
            lblStockLowCaption.Text = "Low Stock Items";
            lblStockOutCaption.Text = "Out of Stock Items";
            lblStockTotalValue.Text = _overview.TotalItems.ToString("N0", _culture);
            lblStockLowValue.Text = _overview.LowStockItems.ToString("N0", _culture);
            lblStockOutValue.Text = _overview.OutOfStockItems.ToString("N0", _culture);
            lblCustomerValue.Text = _overview.TotalCustomers.ToString("N0");
            lblCustomerCaption.Text = "Active Customers";
            lblVendorValue.Text = _overview.TotalVendors.ToString("N0");
            lblVendorCaption.Text = "Active Vendors";
            lblDueSummary.Text = "Receivables  " + Money(_overview.DueReceivables) + "\r\nPayables     " + Money(_overview.DuePayables);

            dgvTopItems.DataSource = new BindingList<TopItemRow>(_overview.TopSellingItems
                .Select((item, index) => new TopItemRow
                {
                    SlNo = index + 1,
                    ItemName = item.ItemName,
                    Qty = item.Qty.ToString("N2", _culture),
                    AmountText = Money(item.Amount)
                }).ToList());

            pnlSalesTrend.Invalidate();
        }

        private void ConfigureSalesTrendScroll()
        {
            if (pnlSalesTrend == null)
                return;

            int pointCount = _overview != null && _overview.SalesTrend != null ? _overview.SalesTrend.Count : 0;
            int chartWidth = GetSalesTrendCanvasWidth(pointCount);
            pnlSalesTrend.AutoScrollMinSize = chartWidth > pnlSalesTrend.ClientSize.Width
                ? new Size(chartWidth, 0)
                : Size.Empty;
        }

        private string MetricTitle(string title, int count)
        {
            return title + " (" + count.ToString("N0", _culture) + ")";
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            LoadDashboard();
        }

        private void FrmDashboardOverview_FormClosed(object sender, FormClosedEventArgs e)
        {
            _refreshTimer.Stop();
            _refreshTimer.Tick -= RefreshTimer_Tick;
            _refreshTimer.Dispose();
        }

        private void ConfigureDashboardDateEditor(UltraDateTimeEditor editor, bool headerEditor)
        {
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = CardBackColor;
            editor.Appearance.BorderColor = CardBorderColor;
            editor.Appearance.ForeColor = TextBlue;
            editor.Appearance.FontData.Name = "Segoe UI Semibold";
            editor.Appearance.FontData.SizeInPoints = headerEditor ? 9 : 8.75F;
            editor.Appearance.TextHAlign = HAlign.Center;
            editor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            editor.DropDownButtonDisplayStyle = headerEditor ? ButtonDisplayStyle.Never : ButtonDisplayStyle.Always;
            editor.FormatString = "dd MMM yyyy";
            editor.MaskInput = "{date}";
            if (headerEditor)
                editor.ReadOnly = true;
        }

        private void ConfigureQuickDateCombo()
        {
            cmbQuickDate.UseAppStyling = false;
            cmbQuickDate.UseOsThemes = DefaultableBoolean.False;
            cmbQuickDate.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            cmbQuickDate.BorderStyle = UIElementBorderStyle.Solid;
            cmbQuickDate.DropDownStyle = DropDownStyle.DropDownList;
            cmbQuickDate.Appearance.BackColor = CardBackColor;
            cmbQuickDate.Appearance.BorderColor = CardBorderColor;
            cmbQuickDate.Appearance.ForeColor = TextBlue;
            cmbQuickDate.Appearance.FontData.Name = "Segoe UI";
            cmbQuickDate.Appearance.FontData.SizeInPoints = 8.75F;
            cmbQuickDate.Items.Clear();
            cmbQuickDate.Items.Add("Today");
            cmbQuickDate.Items.Add("Yesterday");
            cmbQuickDate.Items.Add("This Month");
            cmbQuickDate.Items.Add("Previous Month");
            cmbQuickDate.Items.Add("This Year");
            cmbQuickDate.Items.Add("Previous Year");
            cmbQuickDate.ValueChanged += CmbQuickDate_ValueChanged;
        }

        private void CmbQuickDate_ValueChanged(object sender, EventArgs e)
        {
            string selected = Convert.ToString(cmbQuickDate.Value ?? cmbQuickDate.Text);
            DateTime today = DateTime.Today;
            DateTime fromDate;
            DateTime toDate;
            DashboardOverviewRangeKind rangeKind;

            switch (selected)
            {
                case "Yesterday":
                    fromDate = today.AddDays(-1);
                    toDate = fromDate;
                    rangeKind = DashboardOverviewRangeKind.Day;
                    break;
                case "This Month":
                    fromDate = new DateTime(today.Year, today.Month, 1);
                    toDate = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
                    rangeKind = DashboardOverviewRangeKind.Month;
                    break;
                case "Previous Month":
                    DateTime previousMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                    fromDate = previousMonth;
                    toDate = new DateTime(previousMonth.Year, previousMonth.Month, DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month));
                    rangeKind = DashboardOverviewRangeKind.Month;
                    break;
                case "This Year":
                    fromDate = new DateTime(today.Year, 1, 1);
                    toDate = new DateTime(today.Year, 12, 31);
                    rangeKind = DashboardOverviewRangeKind.Year;
                    break;
                case "Previous Year":
                    fromDate = new DateTime(today.Year - 1, 1, 1);
                    toDate = new DateTime(today.Year - 1, 12, 31);
                    rangeKind = DashboardOverviewRangeKind.Year;
                    break;
                default:
                    fromDate = today;
                    toDate = today;
                    rangeKind = DashboardOverviewRangeKind.Day;
                    break;
            }

            ApplySelectedRange(fromDate, toDate, rangeKind);
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            DateTime fromDate;
            DateTime toDate;
            NormalizeSelectedRange(Convert.ToDateTime(dtFrom.Value), Convert.ToDateTime(dtTo.Value), DashboardOverviewRangeKind.Day, out fromDate, out toDate);
            ApplySelectedRange(fromDate, toDate, DashboardOverviewRangeKind.Day);
        }

        private void ApplySelectedRange(DateTime fromDate, DateTime toDate, DashboardOverviewRangeKind rangeKind)
        {
            _rangeKind = rangeKind;
            _rangeFromDate = fromDate.Date;
            _rangeToDate = toDate.Date;
            _businessDate = _rangeFromDate;
            SetHeaderDateValue();
            LoadDashboard();
        }

        private void SetHeaderDateValue()
        {
            dtFrom.Value = _rangeFromDate;
            dtTo.Value = _rangeToDate;
        }

        private void NormalizeSelectedRange(DateTime fromValue, DateTime toValue, DashboardOverviewRangeKind rangeKind, out DateTime fromDate, out DateTime toDate)
        {
            if (toValue.Date < fromValue.Date)
            {
                DateTime swapValue = fromValue;
                fromValue = toValue;
                toValue = swapValue;
            }

            switch (rangeKind)
            {
                case DashboardOverviewRangeKind.Month:
                    fromDate = new DateTime(fromValue.Year, fromValue.Month, 1);
                    toDate = new DateTime(toValue.Year, toValue.Month, DateTime.DaysInMonth(toValue.Year, toValue.Month));
                    break;
                case DashboardOverviewRangeKind.Year:
                    fromDate = new DateTime(fromValue.Year, 1, 1);
                    toDate = new DateTime(toValue.Year, 12, 31);
                    break;
                default:
                    fromDate = fromValue.Date;
                    toDate = toValue.Date;
                    break;
            }
        }

        private string FormatRangeLabel(DateTime fromDate, DateTime toDate, DashboardOverviewRangeKind rangeKind)
        {
            string fromText;
            string toText;

            switch (rangeKind)
            {
                case DashboardOverviewRangeKind.Month:
                    fromText = fromDate.ToString("MMM yyyy", CultureInfo.InvariantCulture);
                    toText = toDate.ToString("MMM yyyy", CultureInfo.InvariantCulture);
                    break;
                case DashboardOverviewRangeKind.Year:
                    fromText = fromDate.ToString("yyyy", CultureInfo.InvariantCulture);
                    toText = toDate.ToString("yyyy", CultureInfo.InvariantCulture);
                    break;
                default:
                    fromText = fromDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
                    toText = toDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
                    break;
            }

            return fromText == toText ? fromText : fromText + " - " + toText;
        }

        private string RangeKindCaption()
        {
            switch (_rangeKind)
            {
                case DashboardOverviewRangeKind.Month:
                    return "Months";
                case DashboardOverviewRangeKind.Year:
                    return "Years";
                default:
                    return "Days";
            }
        }

        private string PreviousPeriodCaption()
        {
            switch (_rangeKind)
            {
                case DashboardOverviewRangeKind.Month:
                    return new DateTime(_rangeFromDate.Year, _rangeFromDate.Month, 1).AddMonths(-1).ToString("MMM yyyy", CultureInfo.InvariantCulture);
                case DashboardOverviewRangeKind.Year:
                    return (_rangeFromDate.Year - 1).ToString(CultureInfo.InvariantCulture);
                default:
                    return "Yesterday";
            }
        }

        private void ConfigureMetricCard(Panel card, Label icon, Label titleLabel, Label valueLabel, Label footerLabel, Color badgeColor, DashboardBadgeKind badgeKind)
        {
            StylePanel(card);
            SetCardMargin(card);

            ConfigureBadge(icon, badgeColor, badgeKind);

            titleLabel.Font = new Font("Segoe UI Semibold", 8.75F, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(31, 58, 103);
            valueLabel.Font = new Font("Segoe UI Semibold", 12.5F, FontStyle.Bold);
            valueLabel.ForeColor = Color.FromArgb(20, 48, 92);
            footerLabel.Font = new Font("Segoe UI", 7.8F);
            footerLabel.ForeColor = Color.FromArgb(60, 91, 145);

            ResizeMetricLabels(card, titleLabel, valueLabel, footerLabel);
            card.Resize += (s, e) =>
            {
                ResizeMetricLabels(card, titleLabel, valueLabel, footerLabel);
            };
        }

        private void ConfigureBottomCard(Panel card, Label icon, Label titleLabel, Color badgeColor, DashboardBadgeKind badgeKind)
        {
            ConfigureBadge(icon, badgeColor, badgeKind);
            titleLabel.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(28, 60, 112);
        }

        private void ConfigureBadge(Label icon, Color badgeColor, DashboardBadgeKind badgeKind)
        {
            icon.Text = string.Empty;
            icon.BackColor = Color.Transparent;
            icon.Tag = new DashboardBadgeInfo(badgeColor, badgeKind);
            icon.Paint -= BadgeLabel_Paint;
            icon.Paint += BadgeLabel_Paint;
            icon.BringToFront();
            icon.Invalidate();
        }

        private void ResizeMetricLabels(Panel card, Label titleLabel, Label valueLabel, Label footerLabel)
        {
            int width = Math.Max(80, card.Width - 34);
            titleLabel.Width = width;
            valueLabel.Width = width;
            footerLabel.Width = width;
        }

        private void PnlSalesTrend_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            List<DashboardTrendPoint> points = _overview != null && _overview.SalesTrend != null ? _overview.SalesTrend : new List<DashboardTrendPoint>();
            if (points.Count == 0)
                return;

            int pointCount = _overview != null && _overview.SalesTrend != null ? _overview.SalesTrend.Count : 0;
            int canvasWidth = GetSalesTrendCanvasWidth(pointCount);
            int scrollX = pnlSalesTrend.AutoScrollPosition.X;
            int visibleHeight = pnlSalesTrend.ClientSize.Height - (canvasWidth > pnlSalesTrend.ClientSize.Width ? SystemInformation.HorizontalScrollBarHeight : 0);
            int topPadding = 18;
            int bottomPadding = 34;
            int leftPadding = 62;
            int rightPadding = 28;
            Rectangle plot = new Rectangle(scrollX + leftPadding, topPadding, Math.Max(10, canvasWidth - leftPadding - rightPadding), Math.Max(36, visibleHeight - topPadding - bottomPadding));
            using (Pen gridPen = new Pen(GridLineColor))
            using (Pen linePen = new Pen(Color.FromArgb(54, 126, 235), 2.4F))
            using (SolidBrush pointBrush = new SolidBrush(Color.FromArgb(54, 126, 235)))
            using (SolidBrush textBrush = new SolidBrush(TextBlue))
            using (Font font = new Font("Segoe UI", 7.5F))
            using (Font scaleFont = new Font("Segoe UI", 7F))
            using (StringFormat scaleFormat = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center })
            using (StringFormat labelFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near })
            {
                decimal max = Math.Max(1, points.Max(x => x.Amount));
                decimal scaleMax = GetNiceTrendScale(max);
                for (int i = 0; i <= 4; i++)
                {
                    int y = plot.Bottom - (plot.Height * i / 4);
                    e.Graphics.DrawLine(gridPen, plot.Left, y, plot.Right, y);
                    decimal scaleValue = scaleMax * i / 4M;
                    RectangleF scaleBounds = new RectangleF(scrollX + 2, y - 8, leftPadding - 10, 16);
                    e.Graphics.DrawString(CompactMoney(scaleValue), scaleFont, textBrush, scaleBounds, scaleFormat);
                }

                PointF[] chartPoints = new PointF[points.Count];
                int labelStep = GetTrendLabelStep(e.Graphics, points, font, plot.Width);
                for (int i = 0; i < points.Count; i++)
                {
                    float x = plot.Left + (points.Count == 1 ? plot.Width / 2F : (plot.Width * i / (float)(points.Count - 1)));
                    float y = plot.Bottom - (float)(plot.Height * (points[i].Amount / scaleMax));
                    chartPoints[i] = new PointF(x, y);

                    if (ShouldDrawTrendLabel(i, points.Count, labelStep))
                    {
                        RectangleF labelBounds = new RectangleF(x - 42, plot.Bottom + 8, 84, 22);
                        e.Graphics.DrawString(points[i].Caption, font, textBrush, labelBounds, labelFormat);
                    }
                }

                if (chartPoints.Length > 1)
                {
                    using (GraphicsPath fillPath = CreateTrendFillPath(chartPoints, plot.Bottom))
                    using (LinearGradientBrush fillBrush = new LinearGradientBrush(
                        new Rectangle(plot.Left, plot.Top, plot.Width, plot.Height),
                        Color.FromArgb(72, 54, 126, 235),
                        Color.FromArgb(5, 54, 126, 235),
                        LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillPath(fillBrush, fillPath);
                    }

                    e.Graphics.DrawCurve(linePen, chartPoints, 0.35F);
                }
                else if (chartPoints.Length == 1)
                {
                    PointF point = chartPoints[0];
                    e.Graphics.DrawLine(linePen, plot.Left, point.Y, plot.Right, point.Y);
                }

                foreach (PointF point in chartPoints)
                {
                    e.Graphics.FillEllipse(pointBrush, point.X - 4, point.Y - 4, 8, 8);
                    e.Graphics.DrawEllipse(Pens.White, point.X - 4, point.Y - 4, 8, 8);
                }
            }
        }

        private int GetSalesTrendCanvasWidth(int pointCount)
        {
            int visibleWidth = pnlSalesTrend != null ? pnlSalesTrend.ClientSize.Width : 0;
            if (pointCount <= 1)
                return Math.Max(260, visibleWidth);

            int pointSpacing = _rangeKind == DashboardOverviewRangeKind.Day ? 42 : 70;
            int requiredWidth = 90 + ((pointCount - 1) * pointSpacing);
            return Math.Max(Math.Max(260, visibleWidth), requiredWidth);
        }

        private GraphicsPath CreateTrendFillPath(PointF[] chartPoints, int baselineY)
        {
            GraphicsPath path = new GraphicsPath();
            if (chartPoints.Length == 0)
                return path;

            path.AddLine(chartPoints[0].X, baselineY, chartPoints[0].X, chartPoints[0].Y);
            if (chartPoints.Length > 1)
                path.AddCurve(chartPoints, 0.35F);
            else
                path.AddLine(chartPoints[0].X, chartPoints[0].Y, chartPoints[0].X + 1, chartPoints[0].Y);

            PointF lastPoint = chartPoints[chartPoints.Length - 1];
            path.AddLine(lastPoint.X, lastPoint.Y, lastPoint.X, baselineY);
            path.CloseFigure();
            return path;
        }

        private int GetTrendLabelStep(Graphics graphics, List<DashboardTrendPoint> points, Font font, int plotWidth)
        {
            if (points.Count <= 1)
                return 1;

            float widestLabel = points.Max(point => graphics.MeasureString(point.Caption, font).Width);
            int availableSlots = Math.Max(2, (int)(plotWidth / Math.Max(42F, widestLabel + 18F)));
            return Math.Max(1, (int)Math.Ceiling(points.Count / (double)availableSlots));
        }

        private bool ShouldDrawTrendLabel(int index, int count, int labelStep)
        {
            return index == 0 || index == count - 1 || index % labelStep == 0;
        }

        private decimal GetNiceTrendScale(decimal max)
        {
            if (max <= 0)
                return 1;

            decimal magnitude = 1;
            while (magnitude * 10M < max)
                magnitude *= 10M;

            decimal normalized = max / magnitude;
            decimal niceNormalized = normalized <= 1M ? 1M : (normalized <= 2M ? 2M : (normalized <= 5M ? 5M : 10M));
            return niceNormalized * magnitude;
        }

        private string CompactMoney(decimal value)
        {
            decimal absoluteValue = Math.Abs(value);
            if (absoluteValue >= 10000000M)
                return "Rs " + (value / 10000000M).ToString("0.#", _culture) + "Cr";

            if (absoluteValue >= 100000M)
                return "Rs " + (value / 100000M).ToString("0.#", _culture) + "L";

            if (absoluteValue >= 1000M)
                return "Rs " + (value / 1000M).ToString("0.#", _culture) + "K";

            return "Rs " + value.ToString("0", _culture);
        }

        private void FrmDashboardOverview_Resize(object sender, EventArgs e)
        {
            if (Width < 950)
            {
                cardsLayout.ColumnCount = 3;
                cardsLayout.RowCount = 2;
                mainLayout.RowStyles[1].Height = 292;
                MoveCard(cardSales, 0, 0);
                MoveCard(cardPurchase, 1, 0);
                MoveCard(cardSalesReturn, 2, 0);
                MoveCard(cardPurchaseReturn, 0, 1);
                MoveCard(cardReceipts, 1, 1);
                MoveCard(cardPayments, 2, 1);
            }
            else
            {
                cardsLayout.ColumnCount = 6;
                cardsLayout.RowCount = 1;
                mainLayout.RowStyles[1].Height = 146;
                MoveCard(cardSales, 0, 0);
                MoveCard(cardPurchase, 1, 0);
                MoveCard(cardSalesReturn, 2, 0);
                MoveCard(cardPurchaseReturn, 3, 0);
                MoveCard(cardReceipts, 4, 0);
                MoveCard(cardPayments, 5, 0);
            }
        }

        private void MoveCard(Control control, int column, int row)
        {
            cardsLayout.SetColumn(control, column);
            cardsLayout.SetRow(control, row);
        }

        private string Money(decimal value)
        {
            return "Rs " + value.ToString("N2", _culture);
        }

        private void SetCardMargin(Control control)
        {
            control.Margin = new Padding(7, 6, 7, 8);
        }

        private void StylePanel(Panel panel)
        {
            panel.BackColor = CardBackColor;
            panel.Margin = new Padding(8, 6, 8, 8);
            panel.Paint -= RoundedPanel_Paint;
            panel.Paint += RoundedPanel_Paint;
            _cardPanels.Add(panel);
            RegisterCardHover(panel);
        }

        private void RegisterCardHover(Panel panel)
        {
            panel.MouseEnter -= Card_MouseEnter;
            panel.MouseLeave -= Card_MouseLeave;
            panel.MouseEnter += Card_MouseEnter;
            panel.MouseLeave += Card_MouseLeave;

            foreach (Control child in panel.Controls)
                RegisterCardChildHover(panel, child);
        }

        private void RegisterCardChildHover(Panel panel, Control control)
        {
            control.MouseEnter -= Card_MouseEnter;
            control.MouseLeave -= Card_MouseLeave;
            control.MouseEnter += Card_MouseEnter;
            control.MouseLeave += Card_MouseLeave;

            foreach (Control child in control.Controls)
                RegisterCardChildHover(panel, child);
        }

        private void RegisterCardClick(Control control, EventHandler handler)
        {
            control.Cursor = Cursors.Hand;
            control.Click -= handler;
            control.Click += handler;

            foreach (Control child in control.Controls)
                RegisterCardClick(child, handler);
        }

        private void Card_MouseEnter(object sender, EventArgs e)
        {
            Panel panel = FindCardPanel(sender as Control);
            if (panel == null)
                return;

            _hoveredCards.Add(panel);
            panel.BackColor = CardHoverBackColor;
            panel.Invalidate();
        }

        private void Card_MouseLeave(object sender, EventArgs e)
        {
            Panel panel = FindCardPanel(sender as Control);
            if (panel == null || panel.ClientRectangle.Contains(panel.PointToClient(Cursor.Position)))
                return;

            _hoveredCards.Remove(panel);
            panel.BackColor = CardBackColor;
            panel.Invalidate();
        }

        private Panel FindCardPanel(Control control)
        {
            while (control != null)
            {
                Panel panel = control as Panel;
                if (panel != null && _cardPanels.Contains(panel))
                    return panel;

                control = control.Parent;
            }

            return null;
        }

        private void StyleSmallTitle(Label label)
        {
            label.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            label.ForeColor = Color.FromArgb(28, 60, 112);
        }

        private void StyleValueLabel(Label label)
        {
            label.Font = new Font("Segoe UI Semibold", 17F, FontStyle.Bold);
            label.ForeColor = Color.FromArgb(20, 48, 92);
        }

        private void StyleBottomValueLabel(Label label)
        {
            label.BackColor = Color.Transparent;
            label.Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold);
            label.ForeColor = Color.FromArgb(20, 48, 92);
        }

        private void StyleBottomCaption(Label label)
        {
            label.BackColor = Color.Transparent;
            label.Font = new Font("Segoe UI", 8F);
            label.ForeColor = Color.FromArgb(39, 73, 126);
        }

        private void StyleBodyLabel(Label label)
        {
            label.Font = new Font("Consolas", 9F);
            label.ForeColor = Color.FromArgb(39, 73, 126);
        }

        private void StyleStockSummaryLabel(Label label)
        {
            label.BackColor = Color.Transparent;
            label.Font = new Font("Segoe UI Semibold", 8.25F, FontStyle.Bold);
            label.ForeColor = Color.FromArgb(39, 73, 126);
        }

        private void StyleStockValueLabel(Label label, Color color)
        {
            label.BackColor = Color.Transparent;
            label.Font = new Font("Segoe UI Semibold", 8.25F, FontStyle.Bold);
            label.ForeColor = color;
        }

        private void ConfigureStockRowIcon(Label icon, Color color, StockRowIconKind kind)
        {
            icon.BackColor = Color.Transparent;
            icon.Tag = new StockRowIconInfo(color, kind);
            icon.Paint -= StockRowIcon_Paint;
            icon.Paint += StockRowIcon_Paint;
            icon.Invalidate();
        }

        private void RoundedPanel_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;
            if (panel == null) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
            using (GraphicsPath path = RoundedRect(rect, 8))
            using (SolidBrush brush = new SolidBrush(panel.BackColor))
            using (Pen pen = new Pen(_hoveredCards.Contains(panel) ? CardHoverBorderColor : CardBorderColor, _hoveredCards.Contains(panel) ? 1.6F : 1F))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
            }
        }

        private void BadgeLabel_Paint(object sender, PaintEventArgs e)
        {
            Label label = sender as Label;
            if (label == null) return;
            DashboardBadgeInfo badge = label.Tag as DashboardBadgeInfo;
            if (badge == null) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle bounds = new Rectangle(1, 1, label.Width - 3, label.Height - 3);
            using (GraphicsPath path = RoundedRect(bounds, bounds.Width / 2))
            using (SolidBrush brush = new SolidBrush(badge.BackColor))
            {
                e.Graphics.FillPath(brush, path);
            }

            DrawBadgeGlyph(e.Graphics, bounds, badge.Kind);
        }

        private void StockRowIcon_Paint(object sender, PaintEventArgs e)
        {
            Label label = sender as Label;
            if (label == null) return;
            StockRowIconInfo icon = label.Tag as StockRowIconInfo;
            if (icon == null) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle bounds = new Rectangle(1, 1, label.Width - 3, label.Height - 3);
            using (GraphicsPath path = RoundedRect(bounds, 3))
            using (SolidBrush brush = new SolidBrush(icon.BackColor))
            {
                e.Graphics.FillPath(brush, path);
            }

            using (Pen pen = new Pen(Color.White, 1.4F))
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                float cx = bounds.Left + bounds.Width / 2F;
                float cy = bounds.Top + bounds.Height / 2F;

                if (icon.Kind == StockRowIconKind.Alert)
                {
                    PointF[] triangle = {
                        new PointF(cx, bounds.Top + 3),
                        new PointF(bounds.Right - 3, bounds.Bottom - 3),
                        new PointF(bounds.Left + 3, bounds.Bottom - 3)
                    };
                    e.Graphics.DrawPolygon(pen, triangle);
                    e.Graphics.DrawLine(pen, cx, cy - 1, cx, cy + 3);
                    e.Graphics.FillEllipse(brush, cx - 0.8F, cy + 5F, 1.6F, 1.6F);
                    return;
                }

                PointF[] box = {
                    new PointF(cx, bounds.Top + 3),
                    new PointF(bounds.Right - 3, cy - 1),
                    new PointF(bounds.Right - 3, bounds.Bottom - 4),
                    new PointF(cx, bounds.Bottom - 1),
                    new PointF(bounds.Left + 3, bounds.Bottom - 4),
                    new PointF(bounds.Left + 3, cy - 1)
                };
                e.Graphics.DrawPolygon(pen, box);
                e.Graphics.DrawLine(pen, bounds.Left + 3, cy - 1, cx, cy + 2);
                e.Graphics.DrawLine(pen, bounds.Right - 3, cy - 1, cx, cy + 2);
                e.Graphics.DrawLine(pen, cx, cy + 2, cx, bounds.Bottom - 1);
            }
        }

        private void DrawBadgeGlyph(Graphics g, Rectangle bounds, DashboardBadgeKind kind)
        {
            using (Pen pen = new Pen(Color.White, 2.0F))
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                float cx = bounds.Left + bounds.Width / 2F;
                float cy = bounds.Top + bounds.Height / 2F;

                switch (kind)
                {
                    case DashboardBadgeKind.Cart:
                        g.DrawLine(pen, cx - 12, cy - 9, cx - 7, cy - 9);
                        g.DrawLine(pen, cx - 7, cy - 9, cx - 3, cy + 5);
                        g.DrawRectangle(pen, cx - 2, cy - 5, 17, 10);
                        g.FillEllipse(brush, cx + 1, cy + 9, 4, 4);
                        g.FillEllipse(brush, cx + 12, cy + 9, 4, 4);
                        break;
                    case DashboardBadgeKind.Basket:
                        g.DrawArc(pen, cx - 9, cy - 12, 18, 16, 200, 140);
                        g.DrawRectangle(pen, cx - 13, cy - 3, 26, 13);
                        g.DrawLine(pen, cx - 8, cy - 1, cx - 6, cy + 8);
                        g.DrawLine(pen, cx, cy - 1, cx, cy + 8);
                        g.DrawLine(pen, cx + 8, cy - 1, cx + 6, cy + 8);
                        break;
                    case DashboardBadgeKind.Return:
                        g.DrawArc(pen, cx - 11, cy - 11, 22, 22, 35, 275);
                        g.DrawLine(pen, cx - 12, cy - 4, cx - 12, cy - 12);
                        g.DrawLine(pen, cx - 12, cy - 12, cx - 4, cy - 12);
                        break;
                    case DashboardBadgeKind.Wallet:
                        g.DrawRectangle(pen, cx - 13, cy - 8, 26, 17);
                        g.DrawLine(pen, cx - 11, cy - 3, cx + 13, cy - 3);
                        g.FillEllipse(brush, cx + 7, cy + 1, 4, 4);
                        break;
                    case DashboardBadgeKind.Payment:
                        g.DrawRectangle(pen, cx - 13, cy - 9, 26, 18);
                        g.DrawLine(pen, cx - 9, cy - 3, cx + 9, cy - 3);
                        g.DrawLine(pen, cx - 8, cy + 4, cx + 1, cy + 4);
                        break;
                    case DashboardBadgeKind.Box:
                        PointF[] box = {
                            new PointF(cx, cy - 12), new PointF(cx + 12, cy - 5),
                            new PointF(cx + 12, cy + 8), new PointF(cx, cy + 15),
                            new PointF(cx - 12, cy + 8), new PointF(cx - 12, cy - 5)
                        };
                        g.DrawPolygon(pen, box);
                        g.DrawLine(pen, cx - 12, cy - 5, cx, cy + 2);
                        g.DrawLine(pen, cx + 12, cy - 5, cx, cy + 2);
                        g.DrawLine(pen, cx, cy + 2, cx, cy + 15);
                        break;
                    case DashboardBadgeKind.People:
                        g.FillEllipse(brush, cx - 6, cy - 11, 12, 12);
                        g.FillEllipse(brush, cx - 16, cy - 5, 10, 10);
                        g.FillEllipse(brush, cx + 6, cy - 5, 10, 10);
                        g.FillEllipse(brush, cx - 12, cy + 3, 24, 13);
                        break;
                    case DashboardBadgeKind.Truck:
                        g.DrawRectangle(pen, cx - 14, cy - 7, 17, 13);
                        g.DrawRectangle(pen, cx + 3, cy - 3, 10, 9);
                        g.FillEllipse(brush, cx - 11, cy + 8, 5, 5);
                        g.FillEllipse(brush, cx + 6, cy + 8, 5, 5);
                        break;
                }
            }
        }

        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private sealed class TopItemRow
        {
            public int SlNo { get; set; }
            public string ItemName { get; set; }
            public string Qty { get; set; }
            public string AmountText { get; set; }
        }

        private sealed class DashboardBadgeInfo
        {
            public DashboardBadgeInfo(Color backColor, DashboardBadgeKind kind)
            {
                BackColor = backColor;
                Kind = kind;
            }

            public Color BackColor { get; private set; }
            public DashboardBadgeKind Kind { get; private set; }
        }

        private sealed class StockRowIconInfo
        {
            public StockRowIconInfo(Color backColor, StockRowIconKind kind)
            {
                BackColor = backColor;
                Kind = kind;
            }

            public Color BackColor { get; private set; }
            public StockRowIconKind Kind { get; private set; }
        }

        private enum DashboardBadgeKind
        {
            Cart,
            Basket,
            Return,
            Wallet,
            Payment,
            Box,
            People,
            Truck
        }

        private enum StockRowIconKind
        {
            Box,
            Alert
        }
    }
}
