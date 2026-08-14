using ModelClass;
using ModelClass.Report;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.Dashboard
{
    public partial class FrmSalesAnalytics : Form
    {
        private static readonly Color PageBackColor = Color.FromArgb(230, 245, 253);
        private static readonly Color CardBackColor = Color.FromArgb(250, 253, 255);
        private static readonly Color CardBorderColor = Color.FromArgb(190, 213, 238);
        private static readonly Color TextBlue = Color.FromArgb(18, 49, 102);
        private static readonly Color MutedBlue = Color.FromArgb(68, 93, 132);
        private static readonly Color AccentBlue = Color.FromArgb(54, 126, 235);
        private static readonly Color AccentGreen = Color.FromArgb(64, 178, 72);
        private static readonly Color AccentPurple = Color.FromArgb(126, 75, 218);
        private static readonly Color AccentOrange = Color.FromArgb(245, 141, 35);
        private static readonly Color AccentTeal = Color.FromArgb(31, 163, 181);

        private readonly CultureInfo _culture = new CultureInfo("en-IN");
        private DateTime _fromDate = DateTime.Today.AddDays(-6);
        private DateTime _toDate = DateTime.Today;
        private SalesAnalyticsOverview _analytics = new SalesAnalyticsOverview();
        private bool _itemMapSortByAmount = true;
        private readonly VScrollBar _paymentLegendScroll = new VScrollBar();

        public FrmSalesAnalytics()
        {
            InitializeComponent();
            ConfigureRuntimeUi();
            if (IsDesignerHosted())
            {
                _analytics = CreateDesignerAnalytics();
                BindAnalytics();
                return;
            }

            Load += FrmSalesAnalytics_Load;
            Resize += (s, e) => InvalidateCharts();
        }

        private void FrmSalesAnalytics_Load(object sender, EventArgs e)
        {
            LoadAnalytics();
        }

        private bool IsDesignerHosted()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime
                || DesignMode
                || (Site != null && Site.DesignMode);
        }

        private SalesAnalyticsOverview CreateDesignerAnalytics()
        {
            DateTime toDate = DateTime.Today;
            DateTime fromDate = toDate.AddDays(-6);
            List<SalesItemMetric> items = SampleItemSales();

            return new SalesAnalyticsOverview
            {
                FromDate = fromDate,
                ToDate = toDate,
                Summary = new SalesAnalyticsSummary
                {
                    TotalSales = 58640M,
                    TotalOrders = 248,
                    AverageOrderValue = 236.45M,
                    TotalProfit = 15340M,
                    TotalItemsSold = 1248M,
                    SalesChangePercent = 32.45M,
                    OrdersChangePercent = 18.32M,
                    AverageOrderValueChangePercent = 12.05M,
                    ProfitChangePercent = 28.10M,
                    ItemsSoldChangePercent = 21.17M
                },
                SalesTrend = SampleSalesTrend(),
                TopByQuantity = items.OrderByDescending(x => x.QtySold).ToList(),
                TopByAmount = items.OrderByDescending(x => x.Amount).ToList(),
                ItemSales = items,
                PaymentMethods = SamplePayments(),
                Categories = SampleCategories()
            };
        }

        private void ConfigureApplyButton()
        {
            btnApply.UseVisualStyleBackColor = false;
            btnApply.FlatStyle = FlatStyle.Flat;
            btnApply.FlatAppearance.BorderSize = 0;
            btnApply.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 108, 211);
            btnApply.FlatAppearance.MouseDownBackColor = Color.FromArgb(31, 88, 181);
            btnApply.BackColor = AccentBlue;
            btnApply.ForeColor = Color.White;
            btnApply.Font = new Font("Segoe UI Semibold", 8.75F, FontStyle.Bold);
            btnApply.Paint -= ApplyButton_Paint;
            btnApply.Paint += ApplyButton_Paint;
            btnApply.Invalidate();
        }

        private void ApplyButton_Paint(object sender, PaintEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(AccentBlue))
                e.Graphics.FillRectangle(brush, btnApply.ClientRectangle);

            TextRenderer.DrawText(e.Graphics, btnApply.Text, btnApply.Font, btnApply.ClientRectangle,
                Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }

        private void ConfigureRuntimeUi()
        {
            BackColor = PageBackColor;
            dtFrom.Value = _fromDate;
            dtTo.Value = _toDate;
            ConfigureQuickDateCombo();
            ConfigureApplyButton();
            btnApply.Click += (s, e) =>
            {
                _fromDate = dtFrom.Value.Date;
                _toDate = dtTo.Value.Date;
                if (_toDate < _fromDate)
                {
                    DateTime swap = _fromDate;
                    _fromDate = _toDate;
                    _toDate = swap;
                }
                LoadAnalytics();
            };

            trendCanvas.Paint -= TrendCanvas_Paint;
            trendCanvas.Paint += TrendCanvas_Paint;
            itemMapCanvas.Paint -= ItemMapCanvas_Paint;
            itemMapCanvas.Paint += ItemMapCanvas_Paint;
            trendCanvas.Click -= TrendCanvas_Click;
            trendCanvas.Click += TrendCanvas_Click;
            itemMapCanvas.Click -= ItemMapCanvas_Click;
            itemMapCanvas.Click += ItemMapCanvas_Click;
            gridTopQty.CellClick -= GridTopQty_CellClick;
            gridTopQty.CellClick += GridTopQty_CellClick;
            gridTopAmount.CellClick -= GridTopAmount_CellClick;
            gridTopAmount.CellClick += GridTopAmount_CellClick;
            paymentCanvas.Paint -= PaymentCanvas_Paint;
            paymentCanvas.Paint += PaymentCanvas_Paint;
            categoryCanvas.Paint -= CategoryCanvas_Paint;
            categoryCanvas.Paint += CategoryCanvas_Paint;
            paymentCanvas.Click += PaymentCanvas_Click;
            categoryCanvas.Click += CategoryCanvas_Click;
            paymentCanvas.Cursor = Cursors.Hand;
            categoryCanvas.Cursor = Cursors.Hand;
            paymentCanvas.AutoScroll = false;
            _paymentLegendScroll.Dock = DockStyle.Right;
            _paymentLegendScroll.Visible = false;
            _paymentLegendScroll.Scroll += (s, e) => paymentCanvas.Invalidate();
            paymentCanvas.Controls.Add(_paymentLegendScroll);
            _paymentLegendScroll.BringToFront();
            paymentCanvas.MouseWheel += PaymentCanvas_MouseWheel;
            paymentCanvas.Resize += (s, e) => ConfigurePaymentCanvasScroll();
            trendCanvas.Cursor = Cursors.Hand;
            itemMapCanvas.Cursor = Cursors.Hand;
            gridTopQty.Cursor = Cursors.Hand;
            gridTopAmount.Cursor = Cursors.Hand;
            WireAverageOrderStockValueDrilldown();
            trendCanvas.AutoScroll = true;
            trendCanvas.Resize += (s, e) =>
            {
                ConfigureTrendCanvasScroll();
                trendCanvas.Invalidate();
            };

            RegisterCardPaint(cardTotalSales);
            RegisterCardPaint(cardOrders);
            RegisterCardPaint(cardAverageOrder);
            RegisterCardPaint(cardProfit);
            RegisterCardPaint(cardItemsSold);
            RegisterCardPaint(trendPanel);
            RegisterCardPaint(itemMapPanel);
            RegisterCardPaint(topQtyPanel);
            RegisterCardPaint(topAmountPanel);
            RegisterCardPaint(paymentPanel);
            RegisterCardPaint(categoryPanel);
           

            RegisterMetricIcon(iconSales, AccentBlue, MetricIconKind.Cart);
            RegisterMetricIcon(iconOrders, AccentGreen, MetricIconKind.Basket);
            RegisterMetricIcon(iconAverage, AccentPurple, MetricIconKind.Wallet);
            RegisterMetricIcon(iconProfit, AccentOrange, MetricIconKind.Profit);
            RegisterMetricIcon(iconItems, AccentTeal, MetricIconKind.Box);
            ConfigureItemMapControls();
            SetupHoldCard();
        }

        private void ConfigureQuickDateCombo()
        {
            cmbQuickDate.Items.Clear();
            cmbQuickDate.Items.AddRange(new object[]
            {
                "Today",
                "Yesterday",
                "This Month",
                "Previous Month",
                "This Year",
                "Previous Year"
            });
            cmbQuickDate.SelectedIndexChanged -= CmbQuickDate_SelectedIndexChanged;
            cmbQuickDate.SelectedIndexChanged += CmbQuickDate_SelectedIndexChanged;
            cmbQuickDate.SelectedIndex = 0;
        }

        private void CmbQuickDate_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected = Convert.ToString(cmbQuickDate.SelectedItem ?? cmbQuickDate.Text);
            DateTime today = DateTime.Today;
            DateTime fromDate;
            DateTime toDate;

            switch (selected)
            {
                case "Yesterday":
                    fromDate = today.AddDays(-1);
                    toDate = fromDate;
                    break;
                case "This Month":
                    fromDate = new DateTime(today.Year, today.Month, 1);
                    toDate = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
                    break;
                case "Previous Month":
                    DateTime previousMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                    fromDate = previousMonth;
                    toDate = new DateTime(previousMonth.Year, previousMonth.Month, DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month));
                    break;
                case "This Year":
                    fromDate = new DateTime(today.Year, 1, 1);
                    toDate = new DateTime(today.Year, 12, 31);
                    break;
                case "Previous Year":
                    fromDate = new DateTime(today.Year - 1, 1, 1);
                    toDate = new DateTime(today.Year - 1, 12, 31);
                    break;
                default:
                    fromDate = today;
                    toDate = today;
                    break;
            }

            _fromDate = fromDate;
            _toDate = toDate;
            dtFrom.Value = _fromDate;
            dtTo.Value = _toDate;
            if (IsHandleCreated && !IsDesignerHosted())
                LoadAnalytics();
        }

        private void ConfigureItemMapControls()
        {
            lblItemMapTitle.Text = "Item Sales (Brief)";
            lblItemMapTitle.Location = new Point(14, 8);

            cmbItemMapSort.Items.Clear();
            cmbItemMapSort.Items.AddRange(new object[] { "By Amount", "By Quantity" });
            cmbItemMapSort.SelectedIndexChanged -= CmbItemMapSort_SelectedIndexChanged;
            cmbItemMapSort.SelectedIndex = 0;
            cmbItemMapSort.SelectedIndexChanged += CmbItemMapSort_SelectedIndexChanged;

            itemMapPanel.Resize += (s, e) =>
            {
                cmbItemMapSort.Location = new Point(Math.Max(16, itemMapPanel.Width - 118), 6);
                itemMapCanvas.Location = new Point(10, 34);
                itemMapCanvas.Size = new Size(Math.Max(40, itemMapPanel.ClientSize.Width - 20), Math.Max(40, itemMapPanel.ClientSize.Height - 44));
                itemMapCanvas.Invalidate();
            };

            cmbItemMapSort.Location = new Point(Math.Max(16, itemMapPanel.Width - 118), 6);
            itemMapCanvas.Location = new Point(10, 34);
            itemMapCanvas.Size = new Size(Math.Max(40, itemMapPanel.ClientSize.Width - 20), Math.Max(40, itemMapPanel.ClientSize.Height - 44));
        }

        private void CmbItemMapSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            _itemMapSortByAmount = cmbItemMapSort.SelectedIndex == 0;
            itemMapCanvas.Invalidate();
        }

        private void RegisterCardPaint(Panel panel)
        {
            if (panel == null)
                return;

            panel.Paint -= Card_Paint;
            panel.Paint += Card_Paint;
        }

        private void RegisterMetricIcon(PictureBox icon, Color backColor, MetricIconKind kind)
        {
            if (icon == null)
                return;

            icon.BackColor = Color.Transparent;
            icon.SizeMode = PictureBoxSizeMode.CenterImage;
            icon.Image = CreateMetricIconImage(backColor, kind, icon.Width, icon.Height);
        }

        private void Card_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;
            if (panel == null)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = RoundedRect(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), 7))
            using (SolidBrush brush = new SolidBrush(CardBackColor))
            using (Pen pen = new Pen(CardBorderColor))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
            }
        }

        private void LoadAnalytics()
        {
            try
            {
                using (SalesAnalyticsRepository repository = new SalesAnalyticsRepository())
                {
                    _analytics = repository.GetAnalytics(_fromDate, _toDate);
                }

                BindAnalytics();
            }
            catch (Exception ex)
            {
                _analytics = new SalesAnalyticsOverview { FromDate = _fromDate, ToDate = _toDate };
                BindAnalytics();
                MessageBox.Show("Sales analytics could not be loaded.\n\n" + ex.Message, "Sales Analytics", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BindAnalytics()
        {
            SalesAnalyticsSummary summary = _analytics.Summary ?? new SalesAnalyticsSummary();
            lblTotalSales.Text = Money(summary.TotalSales);
            lblOrders.Text = summary.TotalOrders.ToString("N0", _culture);
            lblAverageOrder.Text = Money(summary.AverageOrderValue);
            lblProfit.Text = Money(summary.TotalProfit);
            lblItemsSold.Text = summary.TotalItemsSold.ToString("N0", _culture);
            lblSalesChange.Text = ChangeText(summary.SalesChangePercent);
            lblOrdersChange.Text = ChangeText(summary.OrdersChangePercent);
            lblAverageChange.Text = ChangeText(summary.AverageOrderValueChangePercent);
            lblProfitChange.Text = ChangeText(summary.ProfitChangePercent);
            lblItemsChange.Text = ChangeText(summary.ItemsSoldChangePercent);
            ApplyChangeColor(lblSalesChange, summary.SalesChangePercent);
            ApplyChangeColor(lblOrdersChange, summary.OrdersChangePercent);
            ApplyChangeColor(lblAverageChange, summary.AverageOrderValueChangePercent);
            ApplyChangeColor(lblProfitChange, summary.ProfitChangePercent);
            ApplyChangeColor(lblItemsChange, summary.ItemsSoldChangePercent);

            if (lblHoldItems != null && lblHoldAmount != null)
            {
                lblHoldItems.Text = summary.HoldItemsQty.ToString("N0", _culture);
                lblHoldAmount.Text = Money(summary.HoldAmount) + $" ({summary.HoldOrders} Bills)";
            }

            gridTopQty.DataSource = null;
            gridTopQty.Rows.Clear();
            gridTopQty.DataSource = new BindingList<TopQtyRow>(_analytics.TopByQuantity.Select((x, i) => new TopQtyRow
            {
                Rank = i + 1,
                ItemName = x.ItemName,
                QtySold = x.QtySold.ToString("N2", _culture),
                Amount = Money(x.Amount)
            }).ToList());

            gridTopAmount.DataSource = null;
            gridTopAmount.Rows.Clear();
            gridTopAmount.DataSource = new BindingList<TopAmountRow>(_analytics.TopByAmount.Select((x, i) => new TopAmountRow
            {
                Rank = i + 1,
                ItemName = x.ItemName,
                Amount = Money(x.Amount),
                Profit = Money(x.Profit)
            }).ToList());

            InvalidateCharts();
            ConfigureTrendCanvasScroll();
        }

        private void InvalidateCharts()
        {
            if (trendCanvas != null) trendCanvas.Invalidate();
            if (itemMapCanvas != null) itemMapCanvas.Invalidate();
            if (paymentCanvas != null) paymentCanvas.Invalidate();
            if (categoryCanvas != null) categoryCanvas.Invalidate();
            ConfigurePaymentCanvasScroll();
        }

        private void ConfigureTrendCanvasScroll()
        {
            if (trendCanvas == null)
                return;

            int pointCount = _analytics != null && _analytics.SalesTrend != null ? _analytics.SalesTrend.Count : 0;
            int canvasWidth = GetTrendCanvasWidth(pointCount, trendCanvas.ClientSize.Width);
            trendCanvas.AutoScrollMinSize = canvasWidth > trendCanvas.ClientSize.Width
                ? new Size(canvasWidth, 0)
                : Size.Empty;
        }

        private string Money(decimal value)
        {
            return "Rs " + value.ToString("N2", _culture);
        }

        private string ChangeText(decimal value)
        {
            string sign = value >= 0 ? "+" : string.Empty;
            return sign + value.ToString("0.##", _culture) + "% vs previous period";
        }

        private void ApplyChangeColor(Label label, decimal value)
        {
            label.ForeColor = value >= 0 ? AccentGreen : Color.FromArgb(220, 72, 72);
        }

        private void TrendCanvas_Paint(object sender, PaintEventArgs e)
        {
            DrawLineChart(e.Graphics, trendCanvas, GetSalesTrendForPaint(), x => x.Caption, x => x.Amount);
        }

        private void ItemMapCanvas_Paint(object sender, PaintEventArgs e)
        {
            IList<SalesItemMetric> items = GetItemSalesForPaint()
                .OrderByDescending(x => _itemMapSortByAmount ? x.Amount : x.QtySold)
                .ToList();
            DrawBarChart(e.Graphics, itemMapCanvas.ClientRectangle, items, x => x.ItemName, x => _itemMapSortByAmount ? x.Amount : x.QtySold, AccentBlue);
        }

        private void TrendCanvas_Click(object sender, EventArgs e)
        {
            IList<SalesTrendPoint> trend = GetSalesTrendForPaint();
            ShowSalesChartPopup(
                "Sales Trend - " + FormatDateRange(),
                new BindingList<SalesTrendDetailRow>(BuildTrendDetails(trend)),
                (g, chartPanel) => DrawLineChart(g, chartPanel, trend, x => x.Caption, x => x.Amount));
        }

        private void ItemMapCanvas_Click(object sender, EventArgs e)
        {
            IList<SalesItemMetric> items = GetItemSalesForPaint()
                .OrderByDescending(x => _itemMapSortByAmount ? x.Amount : x.QtySold)
                .ToList();
            ShowSalesChartPopup(
                "Item Sales - " + FormatDateRange(),
                new BindingList<SalesItemDetailRow>(BuildItemSalesRows(items)),
                (g, chartPanel) => DrawBarChart(g, chartPanel.ClientRectangle, items, x => x.ItemName, x => _itemMapSortByAmount ? x.Amount : x.QtySold, AccentBlue));
        }

        private void GridTopQty_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            IList<SalesItemMetric> items = GetAllSoldItems()
                .OrderByDescending(x => x.QtySold)
                .ToList();
            ShowSalesGridPopup("All Items Sold By Quantity - " + FormatDateRange(), BuildItemSalesRows(items));
        }

        private void GridTopAmount_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            IList<SalesItemMetric> items = GetAllSoldItems()
                .OrderByDescending(x => x.Amount)
                .ToList();
            ShowSalesGridPopup("All Items Sold By Amount - " + FormatDateRange(), BuildItemSalesRows(items));
        }

        private void WireAverageOrderStockValueDrilldown()
        {
            if (cardAverageOrder != null)
            {
                cardAverageOrder.Cursor = Cursors.Hand;
                cardAverageOrder.Click -= AverageOrderStockValue_Click;
                cardAverageOrder.Click += AverageOrderStockValue_Click;

                foreach (Control control in cardAverageOrder.Controls)
                {
                    control.Cursor = Cursors.Hand;
                    control.Click -= AverageOrderStockValue_Click;
                    control.Click += AverageOrderStockValue_Click;
                }
            }

            if (lblAverageOrder != null)
            {
                lblAverageOrder.Cursor = Cursors.Hand;
                lblAverageOrder.Click -= AverageOrderStockValue_Click;
                lblAverageOrder.Click += AverageOrderStockValue_Click;
            }
        }

        private void AverageOrderStockValue_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable rows;
                using (StockReportAdvanceRepo repository = new StockReportAdvanceRepo())
                {
                    rows = repository.GetStockTransactionValues(new StockReportFilter
                    {
                        FromDate = _fromDate,
                        ToDate = _toDate,
                        CompanyId = GetCompanyId(),
                        BranchId = GetBranchId(),
                        FinYearId = GetFinYearId()
                    });
                }

                ShowStockTransactionGridPopup("Stock Transaction Values - " + FormatDateRange(), rows);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Stock transaction values could not be loaded.\n\n" + ex.Message,
                    "Stock Transaction Values", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ShowStockTransactionGridPopup(string popupTitle, DataTable rows)
        {
            Form popup = CreatePopupForm(popupTitle, new Size(960, 560));
            Panel card = CreatePopupCard();
            Label title = CreatePopupTitle(popupTitle);
            Button close = CreatePopupCloseButton(popup);

            DataGridView detailGrid = CreatePopupGrid(rows ?? new DataTable());
            card.Controls.Add(detailGrid);
            card.Controls.Add(title);
            card.Controls.Add(close);
            close.BringToFront();
            popup.Controls.Add(card);
            popup.ShowDialog(this);
        }

        private Panel cardHoldItem;
        private PictureBox iconHold;
        private Label lblHoldTitle;
        private Label lblHoldItems;
        private Label lblHoldAmount;

        private void SetupHoldCard()
        {
            if (metricsLayout == null) return;

            metricsLayout.ColumnCount = 6;
            metricsLayout.ColumnStyles.Clear();
            for (int i = 0; i < 6; i++)
            {
                metricsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66F));
            }

            cardHoldItem = new Panel
            {
                BackColor = CardBackColor,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 12, 0),
                Name = "cardHoldItem",
                TabIndex = 5
            };

            iconHold = new PictureBox
            {
                BackColor = Color.Transparent,
                Location = new Point(16, 26),
                Size = new Size(46, 46),
                SizeMode = PictureBoxSizeMode.CenterImage
            };

            lblHoldTitle = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                ForeColor = TextBlue,
                Location = new Point(76, 18),
                Text = "Hold Item"
            };

            lblHoldItems = new Label
            {
                Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
                ForeColor = TextBlue,
                Location = new Point(76, 39),
                Size = new Size(130, 26),
                Text = "0"
            };

            lblHoldAmount = new Label
            {
                Font = new Font("Segoe UI", 8F),
                ForeColor = AccentOrange,
                Location = new Point(76, 68),
                Size = new Size(130, 20),
                Text = "Rs 0.00 (0 Bills)"
            };

            cardHoldItem.Controls.Add(iconHold);
            cardHoldItem.Controls.Add(lblHoldTitle);
            cardHoldItem.Controls.Add(lblHoldItems);
            cardHoldItem.Controls.Add(lblHoldAmount);

            metricsLayout.Controls.Add(cardHoldItem, 5, 0);

            RegisterCardPaint(cardHoldItem);
            RegisterMetricIcon(iconHold, AccentOrange, MetricIconKind.Wallet);

            WireHoldItemDrilldown();
        }

        private void WireHoldItemDrilldown()
        {
            if (cardHoldItem != null)
            {
                cardHoldItem.Cursor = Cursors.Hand;
                cardHoldItem.Click -= HoldItemCard_Click;
                cardHoldItem.Click += HoldItemCard_Click;

                foreach (Control control in cardHoldItem.Controls)
                {
                    control.Cursor = Cursors.Hand;
                    control.Click -= HoldItemCard_Click;
                    control.Click += HoldItemCard_Click;
                }
            }
        }

        private void HoldItemCard_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable rows;
                using (SalesAnalyticsRepository repository = new SalesAnalyticsRepository())
                {
                    rows = repository.GetHoldItemsDetails(_fromDate, _toDate);
                }

                ShowStockTransactionGridPopup("Held Items & Hold Bills - " + FormatDateRange(), rows);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hold items could not be loaded.\n\n" + ex.Message,
                    "Hold Items", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private DataGridView CreatePopupGrid(DataTable rows)
        {
            DataGridView grid = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoGenerateColumns = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = CardBackColor,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(220, 233, 246),
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                DataSource = rows
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(232, 241, 252);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = TextBlue;
            grid.ColumnHeadersHeight = 32;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(36, 64, 105);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 232, 250);
            grid.DefaultCellStyle.SelectionForeColor = TextBlue;
            grid.RowTemplate.Height = 28;

            foreach (DataGridViewColumn column in grid.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                if (column.Name == "ItemName")
                    column.FillWeight = 160;
                else if (column.Name == "TransactionDate")
                    column.FillWeight = 92;
                else if (column.Name == "Movement")
                    column.FillWeight = 98;
                else if (column.Name == "DocNumber")
                    column.FillWeight = 82;
                else
                    column.FillWeight = 78;

                if (column.ValueType == typeof(decimal) || column.Name == "Qty" || column.Name == "Cost" ||
                    column.Name == "SellingPrice" || column.Name == "StockValue")
                {
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    column.DefaultCellStyle.Format = "N2";
                }

                if (column.Name == "TransactionDate")
                    column.DefaultCellStyle.Format = "dd-MMM-yyyy";
            }

            return grid;
        }

        private void PaymentCanvas_Paint(object sender, PaintEventArgs e)
        {
            Rectangle bounds = paymentCanvas.ClientRectangle;
            if (_paymentLegendScroll.Visible)
                bounds.Width -= _paymentLegendScroll.Width;
            DrawDonutBreakdown(e.Graphics, bounds, GetPaymentMethodsForPaint(), _paymentLegendScroll.Value);
        }

        private void CategoryCanvas_Paint(object sender, PaintEventArgs e)
        {
            DrawDonutBreakdown(e.Graphics, categoryCanvas.ClientRectangle, GetCategoriesForPaint());
        }

        private void ConfigurePaymentCanvasScroll()
        {
            if (paymentCanvas == null) return;
            int count = GetPaymentMethodsForPaint().Count;
            int requiredHeight = 32 + count * 28;
            int visibleHeight = Math.Max(1, paymentCanvas.ClientSize.Height);
            _paymentLegendScroll.Visible = requiredHeight > visibleHeight;
            _paymentLegendScroll.Minimum = 0;
            _paymentLegendScroll.LargeChange = visibleHeight;
            _paymentLegendScroll.SmallChange = 28;
            _paymentLegendScroll.Maximum = Math.Max(0, requiredHeight - 1);
            int maxValue = Math.Max(0, _paymentLegendScroll.Maximum - _paymentLegendScroll.LargeChange + 1);
            if (_paymentLegendScroll.Value > maxValue)
                _paymentLegendScroll.Value = maxValue;
            paymentCanvas.Invalidate();
        }

        private void PaymentCanvas_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!_paymentLegendScroll.Visible) return;
            int maxValue = Math.Max(0, _paymentLegendScroll.Maximum - _paymentLegendScroll.LargeChange + 1);
            int next = _paymentLegendScroll.Value - Math.Sign(e.Delta) * _paymentLegendScroll.SmallChange;
            _paymentLegendScroll.Value = Math.Max(_paymentLegendScroll.Minimum, Math.Min(maxValue, next));
            paymentCanvas.Invalidate();
        }

        private void PaymentCanvas_Click(object sender, EventArgs e)
        {
            DashboardDrilldownPopup.ShowGrid(this,
                "Sales by Payment Method - " + FormatDateRange(),
                GetPaymentMethodsForPaint().OrderByDescending(x => x.Amount));
        }

        private void CategoryCanvas_Click(object sender, EventArgs e)
        {
            DashboardDrilldownPopup.ShowGrid(this,
                "Sold Item Categories - " + FormatDateRange(),
                _analytics != null && _analytics.ItemCategories != null
                    ? _analytics.ItemCategories
                    : Enumerable.Empty<SalesItemCategoryDetail>());
        }

        private IList<SalesTrendPoint> GetSalesTrendForPaint()
        {
            if (_analytics != null && _analytics.SalesTrend != null && _analytics.SalesTrend.Count > 0)
                return _analytics.SalesTrend;

            return IsInDesigner() ? SampleSalesTrend() : new List<SalesTrendPoint>();
        }

        private IList<SalesItemMetric> GetItemSalesForPaint()
        {
            if (_analytics != null && _analytics.ItemSales != null && _analytics.ItemSales.Count > 0)
                return _analytics.ItemSales;

            return IsInDesigner() ? SampleItemSales() : new List<SalesItemMetric>();
        }

        private IList<SalesItemMetric> GetAllSoldItems()
        {
            IList<SalesItemMetric> items = GetItemSalesForPaint();
            if (items != null && items.Count > 0)
                return items;

            if (_analytics != null && _analytics.TopByQuantity != null && _analytics.TopByQuantity.Count > 0)
                return _analytics.TopByQuantity;

            if (_analytics != null && _analytics.TopByAmount != null && _analytics.TopByAmount.Count > 0)
                return _analytics.TopByAmount;

            return new List<SalesItemMetric>();
        }

        private IList<SalesBreakdown> GetPaymentMethodsForPaint()
        {
            if (_analytics != null && _analytics.PaymentMethods != null && _analytics.PaymentMethods.Count > 0)
                return _analytics.PaymentMethods;

            return IsInDesigner() ? SamplePayments() : new List<SalesBreakdown>();
        }

        private IList<SalesBreakdown> GetCategoriesForPaint()
        {
            if (_analytics != null && _analytics.Categories != null && _analytics.Categories.Count > 0)
                return _analytics.Categories;

            return IsInDesigner() ? SampleCategories() : new List<SalesBreakdown>();
        }

        private bool IsInDesigner()
        {
            return IsDesignerHosted();
        }

        private List<SalesItemDetailRow> BuildItemSalesRows(IEnumerable<SalesItemMetric> items)
        {
            return (items ?? Enumerable.Empty<SalesItemMetric>()).Where(x => x != null).Select((x, i) => new SalesItemDetailRow
            {
                Rank = i + 1,
                ItemName = ShortLabel(x.ItemName, 40),
                QtySold = x.QtySold.ToString("N2", _culture),
                Amount = Money(x.Amount),
                Profit = Money(x.Profit),
                Range = FormatDateRange()
            }).ToList();
        }

        private List<SalesTrendDetailRow> BuildTrendDetails(IEnumerable<SalesTrendPoint> trend)
        {
            List<SalesTrendPoint> trendRows = (trend ?? Enumerable.Empty<SalesTrendPoint>()).Where(x => x != null).ToList();
            List<SalesItemMetric> itemRows = GetAllSoldItems().Where(x => x != null).ToList();
            List<SalesTrendDetailRow> rows = new List<SalesTrendDetailRow>();

            if (trendRows.Count == 0)
                return rows;

            if (itemRows.Count == 0)
            {
                for (int i = 0; i < trendRows.Count; i++)
                {
                    rows.Add(new SalesTrendDetailRow
                    {
                        Rank = i + 1,
                        Caption = trendRows[i].Caption,
                        SaleDate = trendRows[i].SaleDate.ToString("dd MMM yyyy", _culture),
                        Value = Money(trendRows[i].Amount),
                        ItemName = "No item",
                        QtySold = "0.00",
                        Amount = Money(0),
                        Profit = Money(0)
                    });
                }
                return rows;
            }

            int rank = 1;
            foreach (SalesTrendPoint point in trendRows)
            {
                foreach (SalesItemMetric item in itemRows)
                {
                    rows.Add(new SalesTrendDetailRow
                    {
                        Rank = rank++,
                        Caption = point.Caption,
                        SaleDate = point.SaleDate.ToString("dd MMM yyyy", _culture),
                        Value = Money(point.Amount),
                        ItemName = ShortLabel(item.ItemName, 40),
                        QtySold = item.QtySold.ToString("N2", _culture),
                        Amount = Money(item.Amount),
                        Profit = Money(item.Profit)
                    });
                }
            }

            return rows;
        }

        private string FormatDateRange()
        {
            return _fromDate.ToString("dd MMM yyyy", _culture) + " to " + _toDate.ToString("dd MMM yyyy", _culture);
        }

        private int GetCompanyId()
        {
            if (SessionContext.IsInitialized && SessionContext.CompanyId > 0)
                return SessionContext.CompanyId;
            int value;
            return int.TryParse(DataBase.CompanyId, out value) && value > 0 ? value : 1;
        }

        private int GetBranchId()
        {
            if (SessionContext.IsInitialized && SessionContext.BranchId > 0)
                return SessionContext.BranchId;
            int value;
            return int.TryParse(DataBase.BranchId, out value) && value > 0 ? value : 1;
        }

        private int GetFinYearId()
        {
            if (SessionContext.IsInitialized && SessionContext.FinYearId > 0)
                return SessionContext.FinYearId;
            int value;
            return int.TryParse(DataBase.FinyearId, out value) && value > 0 ? value : 1;
        }

        private List<SalesTrendPoint> SampleSalesTrend()
        {
            return new List<SalesTrendPoint>
            {
                new SalesTrendPoint { Caption = "14 May", Amount = 38000M },
                new SalesTrendPoint { Caption = "15 May", Amount = 59000M },
                new SalesTrendPoint { Caption = "16 May", Amount = 44000M },
                new SalesTrendPoint { Caption = "17 May", Amount = 56000M },
                new SalesTrendPoint { Caption = "18 May", Amount = 62000M },
                new SalesTrendPoint { Caption = "19 May", Amount = 85000M },
                new SalesTrendPoint { Caption = "20 May", Amount = 61000M }
            };
        }

        private List<SalesItemMetric> SampleItemSales()
        {
            return new List<SalesItemMetric>
            {
                new SalesItemMetric { ItemName = "Dell Laptop", QtySold = 95M, Amount = 23500M },
                new SalesItemMetric { ItemName = "HP Mouse", QtySold = 120M, Amount = 19000M },
                new SalesItemMetric { ItemName = "Keyboard", QtySold = 80M, Amount = 15500M },
                new SalesItemMetric { ItemName = "USB Pendrive", QtySold = 60M, Amount = 12500M },
                new SalesItemMetric { ItemName = "Power Adapter", QtySold = 45M, Amount = 11000M },
                new SalesItemMetric { ItemName = "Monitor", QtySold = 38M, Amount = 10000M },
                new SalesItemMetric { ItemName = "Printer", QtySold = 26M, Amount = 8000M },
                new SalesItemMetric { ItemName = "Speaker", QtySold = 22M, Amount = 6500M }
            };
        }

        private List<SalesBreakdown> SamplePayments()
        {
            return new List<SalesBreakdown>
            {
                new SalesBreakdown { Name = "Cash", Amount = 28140M, Count = 40 },
                new SalesBreakdown { Name = "UPI", Amount = 18500M, Count = 30 },
                new SalesBreakdown { Name = "Card", Amount = 8900M, Count = 14 },
                new SalesBreakdown { Name = "Bank Transfer", Amount = 2100M, Count = 5 },
                new SalesBreakdown { Name = "Other", Amount = 1000M, Count = 2 }
            };
        }

        private List<SalesBreakdown> SampleCategories()
        {
            return new List<SalesBreakdown>
            {
                new SalesBreakdown { Name = "Electronics", Amount = 32450M, Count = 25 },
                new SalesBreakdown { Name = "Accessories", Amount = 13200M, Count = 18 },
                new SalesBreakdown { Name = "Computer", Amount = 8750M, Count = 8 },
                new SalesBreakdown { Name = "Others", Amount = 4240M, Count = 5 }
            };
        }

        private void DrawLineChart(Graphics g, Panel canvas, IList<SalesTrendPoint> points, Func<SalesTrendPoint, string> label, Func<SalesTrendPoint, decimal> value)
        {
            PrepareGraphics(g);
            if (points == null || points.Count == 0)
            {
                DrawEmpty(g, canvas.ClientRectangle);
                return;
            }

            int canvasWidth = GetTrendCanvasWidth(points.Count, canvas.ClientSize.Width);
            int scrollX = canvas.AutoScrollPosition.X;
            int visibleHeight = canvas.ClientSize.Height - (canvasWidth > canvas.ClientSize.Width ? SystemInformation.HorizontalScrollBarHeight : 0);
            int topPadding = 18;
            int bottomPadding = 34;
            int leftPadding = 62;
            int rightPadding = 28;
            Rectangle plot = new Rectangle(scrollX + leftPadding, topPadding, Math.Max(10, canvasWidth - leftPadding - rightPadding), Math.Max(36, visibleHeight - topPadding - bottomPadding));

            decimal max = GetNiceScale(Math.Max(1, points.Max(value)));
            using (Pen gridPen = new Pen(CardBorderColor))
            using (Pen linePen = new Pen(AccentBlue, 2.4F))
            using (SolidBrush dotBrush = new SolidBrush(AccentBlue))
            using (SolidBrush textBrush = new SolidBrush(TextBlue))
            using (Font font = new Font("Segoe UI", 7.5F))
            using (Font scaleFont = new Font("Segoe UI", 7F))
            using (StringFormat scaleFormat = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center })
            using (StringFormat labelFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near })
            {
                for (int i = 0; i <= 4; i++)
                {
                    int y = plot.Bottom - (plot.Height * i / 4);
                    decimal axisValue = max * i / 4M;
                    g.DrawLine(gridPen, plot.Left, y, plot.Right, y);
                    RectangleF scaleBounds = new RectangleF(scrollX + 2, y - 8, leftPadding - 10, 16);
                    g.DrawString(CompactMoney(axisValue), scaleFont, textBrush, scaleBounds, scaleFormat);
                }

                PointF[] line = new PointF[points.Count];
                int labelStep = GetLabelStep(g, points.Select(label).ToList(), font, plot.Width);
                for (int i = 0; i < points.Count; i++)
                {
                    float x = points.Count == 1 ? plot.Left + plot.Width / 2F : plot.Left + (plot.Width * i / (float)(points.Count - 1));
                    float y = plot.Bottom - ((float)(value(points[i]) / max) * plot.Height);
                    line[i] = new PointF(x, y);

                    if (ShouldDrawTrendLabel(i, points.Count, labelStep))
                    {
                        RectangleF labelBounds = new RectangleF(x - 42, plot.Bottom + 8, 84, 22);
                        g.DrawString(label(points[i]), font, textBrush, labelBounds, labelFormat);
                    }
                }

                if (line.Length > 1)
                {
                    using (GraphicsPath fillPath = CreateTrendFillPath(line, plot.Bottom))
                    using (LinearGradientBrush fillBrush = new LinearGradientBrush(plot, Color.FromArgb(70, AccentBlue), Color.FromArgb(4, AccentBlue), LinearGradientMode.Vertical))
                    {
                        g.FillPath(fillBrush, fillPath);
                    }

                    g.DrawCurve(linePen, line, 0.35F);
                }
                else if (line.Length == 1)
                {
                    PointF point = line[0];
                    g.DrawLine(linePen, plot.Left, point.Y, plot.Right, point.Y);
                }

                foreach (PointF point in line)
                {
                    g.FillEllipse(dotBrush, point.X - 4, point.Y - 4, 8, 8);
                    g.DrawEllipse(Pens.White, point.X - 4, point.Y - 4, 8, 8);
                }
            }
        }

        private void DrawBarChart(Graphics g, Rectangle bounds, IList<SalesItemMetric> items, Func<SalesItemMetric, string> label, Func<SalesItemMetric, decimal> value, Color color)
        {
            PrepareGraphics(g);
            Rectangle plot = Rectangle.FromLTRB(bounds.Left + 46, bounds.Top + 8, bounds.Right - 12, bounds.Bottom - 30);
            if (items == null || items.Count == 0)
            {
                DrawEmpty(g, bounds);
                return;
            }

            decimal max = GetNiceScale(Math.Max(1, items.Max(value)));
            int count = Math.Min(items.Count, 8);
            int slot = Math.Max(18, plot.Width / count);
            int barWidth = Math.Max(14, Math.Min(30, slot - 12));
            using (SolidBrush barBrush = new SolidBrush(color))
            using (SolidBrush textBrush = new SolidBrush(MutedBlue))
            using (Pen gridPen = new Pen(Color.FromArgb(225, 235, 246)))
            using (Font font = new Font("Segoe UI", 7F))
            using (Font axisFont = new Font("Segoe UI", 7.5F))
            {
                for (int i = 0; i <= 4; i++)
                {
                    int y = plot.Bottom - (plot.Height * i / 4);
                    decimal axisValue = max * i / 4M;
                    g.DrawLine(gridPen, plot.Left, y, plot.Right, y);
                    g.DrawString(CompactMoney(axisValue), axisFont, textBrush, bounds.Left + 6, y - 7);
                }

                for (int i = 0; i < count; i++)
                {
                    int h = (int)((float)(value(items[i]) / max) * plot.Height);
                    int x = plot.Left + i * slot + (slot - barWidth) / 2;
                    Rectangle bar = new Rectangle(x, plot.Bottom - h, barWidth, h);
                    g.FillRectangle(barBrush, bar);
                    string text = ShortLabel(label(items[i]), 12);
                    SizeF textSize = g.MeasureString(text, font);
                    g.DrawString(text, font, textBrush, x + (barWidth / 2F) - (textSize.Width / 2F), plot.Bottom + 8);
                }
            }
        }

        private void DrawDonutBreakdown(Graphics g, Rectangle bounds, IList<SalesBreakdown> items, int legendOffset = 0)
        {
            PrepareGraphics(g);
            IList<SalesBreakdown> visibleItems = (items ?? new List<SalesBreakdown>()).Where(x => x != null).ToList();
            if (visibleItems.Count == 0 || visibleItems.Sum(x => x.Amount) <= 0)
            {
                DrawEmpty(g, bounds);
                return;
            }

            Color[] colors = { AccentBlue, AccentGreen, AccentPurple, AccentOrange, AccentTeal, Color.FromArgb(236, 99, 94) };
            decimal total = visibleItems.Sum(x => x.Amount);
            bool compact = bounds.Width < 330;
            int pieSize = compact
                ? Math.Min(88, Math.Max(70, bounds.Height - 68))
                : Math.Min(130, Math.Max(92, bounds.Height - 52));
            Rectangle pie = new Rectangle(bounds.Left + (compact ? 8 : 14), bounds.Top + ((bounds.Height - pieSize) / 2), pieSize, pieSize);
            float start = -90;
            using (SolidBrush white = new SolidBrush(CardBackColor))
            using (SolidBrush textBrush = new SolidBrush(TextBlue))
            using (Font font = new Font("Segoe UI", 7.5F))
            using (Font compactFont = new Font("Segoe UI", 6.8F))
            {
                for (int i = 0; i < visibleItems.Count; i++)
                {
                    float sweep = (float)(visibleItems[i].Amount / total) * 360F;
                    using (SolidBrush brush = new SolidBrush(colors[i % colors.Length]))
                        g.FillPie(brush, pie, start, sweep);
                    start += sweep;
                }
                Rectangle hole = Rectangle.Inflate(pie, -pie.Width / 4, -pie.Height / 4);
                g.FillEllipse(white, hole);

                int y = bounds.Top + (compact ? 16 : 24) - legendOffset;
                int legendX = pie.Right + (compact ? 8 : 16);
                int amountX = Math.Max(legendX + 118, bounds.Right - 112);
                int rowHeight = compact ? 28 : 22;
                GraphicsState legendState = g.Save();
                g.SetClip(new Rectangle(legendX, bounds.Top, Math.Max(1, bounds.Right - legendX), bounds.Height));
                for (int i = 0; i < visibleItems.Count; i++)
                {
                    if (y + rowHeight < bounds.Top)
                    {
                        y += rowHeight;
                        continue;
                    }
                    if (y >= bounds.Bottom)
                        break;

                    using (SolidBrush brush = new SolidBrush(colors[i % colors.Length]))
                        g.FillEllipse(brush, legendX, y + 4, 8, 8);
                    decimal percent = total > 0 ? visibleItems[i].Amount / total * 100M : 0;
                    string amountText = Money(visibleItems[i].Amount) + " (" + percent.ToString("0.#", _culture) + "%)";
                    if (compact)
                    {
                        g.DrawString(ShortLabel(visibleItems[i].Name, 14), compactFont, textBrush, legendX + 14, y - 1);
                        g.DrawString(amountText, compactFont, textBrush, legendX + 14, y + 11);
                    }
                    else
                    {
                        g.DrawString(ShortLabel(visibleItems[i].Name, 16), font, textBrush, legendX + 14, y);
                        g.DrawString(amountText, font, textBrush, amountX, y);
                    }
                    y += rowHeight;
                }
                g.Restore(legendState);
            }
        }

        private void ShowSalesChartPopup<T>(string popupTitle, BindingList<T> rows, Action<Graphics, Panel> drawChart)
        {
            Form popup = CreatePopupForm(popupTitle, new Size(930, 580));
            Panel card = CreatePopupCard();
            Label title = CreatePopupTitle(popupTitle);
            Button close = CreatePopupCloseButton(popup);

            TableLayoutPanel content = new TableLayoutPanel
            {
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                RowCount = 2,
                Padding = new Padding(0, 8, 0, 0)
            };
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));

            Panel chartPanel = new Panel
            {
                AutoScroll = true,
                BackColor = CardBackColor,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 10)
            };
            chartPanel.Paint += (s, e) => drawChart(e.Graphics, chartPanel);

            DataGridView detailGrid = CreatePopupGrid(rows);
            content.Controls.Add(chartPanel, 0, 0);
            content.Controls.Add(detailGrid, 0, 1);
            card.Controls.Add(content);
            card.Controls.Add(title);
            card.Controls.Add(close);
            close.BringToFront();
            popup.Controls.Add(card);
            popup.ShowDialog(this);
        }

        private void ShowSalesGridPopup<T>(string popupTitle, IEnumerable<T> rows)
        {
            Form popup = CreatePopupForm(popupTitle, new Size(820, 470));
            Panel card = CreatePopupCard();
            Label title = CreatePopupTitle(popupTitle);
            Button close = CreatePopupCloseButton(popup);
            DataGridView detailGrid = CreatePopupGrid(new BindingList<T>((rows ?? Enumerable.Empty<T>()).ToList()));

            card.Controls.Add(detailGrid);
            card.Controls.Add(title);
            card.Controls.Add(close);
            close.BringToFront();
            popup.Controls.Add(card);
            popup.ShowDialog(this);
        }

        private Form CreatePopupForm(string popupTitle, Size size)
        {
            return new Form
            {
                Text = popupTitle,
                StartPosition = FormStartPosition.CenterParent,
                Size = size,
                MinimizeBox = false,
                MaximizeBox = false,
                ShowIcon = false,
                BackColor = PageBackColor,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Padding = new Padding(14)
            };
        }

        private Panel CreatePopupCard()
        {
            Panel card = new Panel
            {
                BackColor = CardBackColor,
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 10, 12, 12)
            };
            card.Paint += Card_Paint;
            return card;
        }

        private Label CreatePopupTitle(string popupTitle)
        {
            return new Label
            {
                Dock = DockStyle.Top,
                Height = 32,
                Text = popupTitle,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = TextBlue,
                Padding = new Padding(3, 3, 0, 0)
            };
        }

        private Button CreatePopupCloseButton(Form popup)
        {
            Button close = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = AccentBlue,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                Location = new Point(popup.ClientSize.Width - 105, 16),
                Size = new Size(72, 28),
                Text = "Close"
            };
            close.FlatAppearance.BorderSize = 0;
            close.Click += (s, e) => popup.Close();
            return close;
        }

        private DataGridView CreatePopupGrid<T>(BindingList<T> rows)
        {
            DataGridView grid = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoGenerateColumns = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = CardBackColor,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(220, 233, 246),
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                DataSource = rows
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(232, 241, 252);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = TextBlue;
            grid.ColumnHeadersHeight = 32;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(36, 64, 105);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 232, 250);
            grid.DefaultCellStyle.SelectionForeColor = TextBlue;
            grid.RowTemplate.Height = 28;
            foreach (DataGridViewColumn column in grid.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                if (column.Name == "Rank")
                    column.FillWeight = 34;
                else if (column.Name == "ItemName")
                    column.FillWeight = 135;
                else
                    column.FillWeight = 78;
            }
            return grid;
        }

        private void PrepareGraphics(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(CardBackColor);
        }

        private Image CreateMetricIconImage(Color backColor, MetricIconKind kind, int width, int height)
        {
            int imageWidth = Math.Max(46, width);
            int imageHeight = Math.Max(46, height);
            Bitmap bitmap = new Bitmap(imageWidth, imageHeight);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                Rectangle badge = new Rectangle(1, 1, imageWidth - 3, imageHeight - 3);
                using (SolidBrush brush = new SolidBrush(backColor))
                    g.FillEllipse(brush, badge);

                using (Pen pen = new Pen(Color.White, 2.2F))
                using (SolidBrush brush = new SolidBrush(Color.White))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    float cx = badge.Left + badge.Width / 2F;
                    float cy = badge.Top + badge.Height / 2F;

                    switch (kind)
                    {
                        case MetricIconKind.Cart:
                            g.DrawLine(pen, cx - 13, cy - 9, cx - 8, cy - 9);
                            g.DrawLine(pen, cx - 8, cy - 9, cx - 4, cy + 5);
                            g.DrawRectangle(pen, cx - 3, cy - 5, 18, 10);
                            g.FillEllipse(brush, cx, cy + 9, 4, 4);
                            g.FillEllipse(brush, cx + 12, cy + 9, 4, 4);
                            break;
                        case MetricIconKind.Basket:
                            g.DrawArc(pen, cx - 10, cy - 13, 20, 17, 200, 140);
                            g.DrawRectangle(pen, cx - 14, cy - 4, 28, 14);
                            g.DrawLine(pen, cx - 8, cy - 1, cx - 6, cy + 8);
                            g.DrawLine(pen, cx, cy - 1, cx, cy + 8);
                            g.DrawLine(pen, cx + 8, cy - 1, cx + 6, cy + 8);
                            break;
                        case MetricIconKind.Wallet:
                            g.DrawRectangle(pen, cx - 13, cy - 9, 26, 18);
                            g.DrawLine(pen, cx - 10, cy - 4, cx + 13, cy - 4);
                            g.FillEllipse(brush, cx + 6, cy + 1, 4, 4);
                            break;
                        case MetricIconKind.Profit:
                            g.DrawLine(pen, cx - 13, cy + 10, cx - 13, cy + 3);
                            g.DrawLine(pen, cx - 5, cy + 10, cx - 5, cy - 4);
                            g.DrawLine(pen, cx + 3, cy + 10, cx + 3, cy - 9);
                            g.DrawLine(pen, cx + 11, cy + 10, cx + 11, cy - 14);
                            g.DrawLines(pen, new[] { new PointF(cx - 14, cy + 1), new PointF(cx - 6, cy - 5), new PointF(cx + 1, cy - 3), new PointF(cx + 12, cy - 15) });
                            g.DrawLine(pen, cx + 7, cy - 15, cx + 12, cy - 15);
                            g.DrawLine(pen, cx + 12, cy - 15, cx + 12, cy - 10);
                            break;
                        case MetricIconKind.Box:
                            PointF[] box = {
                                new PointF(cx, cy - 13), new PointF(cx + 13, cy - 6),
                                new PointF(cx + 13, cy + 8), new PointF(cx, cy + 15),
                                new PointF(cx - 13, cy + 8), new PointF(cx - 13, cy - 6)
                            };
                            g.DrawPolygon(pen, box);
                            g.DrawLine(pen, cx - 13, cy - 6, cx, cy + 1);
                            g.DrawLine(pen, cx + 13, cy - 6, cx, cy + 1);
                            g.DrawLine(pen, cx, cy + 1, cx, cy + 15);
                            break;
                    }
                }
            }

            return bitmap;
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

        private GraphicsPath CreateTrendFillPath(PointF[] line, int baseline)
        {
            GraphicsPath path = new GraphicsPath();
            if (line == null || line.Length == 0)
                return path;

            path.AddLine(line[0].X, baseline, line[0].X, line[0].Y);
            if (line.Length > 1)
                path.AddCurve(line, 0.35F);
            else
                path.AddLine(line[0].X, line[0].Y, line[0].X + 1, line[0].Y);

            PointF last = line[line.Length - 1];
            path.AddLine(last.X, last.Y, last.X, baseline);
            path.CloseFigure();
            return path;
        }

        private int GetLabelStep(Graphics graphics, IList<string> labels, Font font, int plotWidth)
        {
            if (labels == null || labels.Count <= 1)
                return 1;

            float widest = labels.Max(x => graphics.MeasureString(x ?? string.Empty, font).Width);
            int slots = Math.Max(2, (int)(plotWidth / Math.Max(46F, widest + 18F)));
            return Math.Max(1, (int)Math.Ceiling(labels.Count / (double)slots));
        }

        private bool ShouldDrawTrendLabel(int index, int count, int labelStep)
        {
            return index == 0 || index == count - 1 || index % labelStep == 0;
        }

        private int GetTrendCanvasWidth(int pointCount, int visibleWidth)
        {
            if (pointCount <= 1)
                return Math.Max(260, visibleWidth);

            int pointSpacing = 42;
            int requiredWidth = 90 + ((pointCount - 1) * pointSpacing);
            return Math.Max(Math.Max(260, visibleWidth), requiredWidth);
        }

        private decimal GetNiceScale(decimal max)
        {
            if (max <= 0)
                return 1;

            decimal magnitude = 1;
            while (magnitude * 10M < max)
                magnitude *= 10M;

            decimal normalized = max / magnitude;
            decimal nice = normalized <= 1M ? 1M : (normalized <= 2M ? 2M : (normalized <= 5M ? 5M : 10M));
            return nice * magnitude;
        }

        private string CompactMoney(decimal value)
        {
            decimal absolute = Math.Abs(value);
            if (absolute >= 10000000M)
                return "Rs " + (value / 10000000M).ToString("0.#", _culture) + "Cr";
            if (absolute >= 100000M)
                return "Rs " + (value / 100000M).ToString("0.#", _culture) + "L";
            if (absolute >= 1000M)
                return "Rs " + (value / 1000M).ToString("0.#", _culture) + "K";

            return "Rs " + value.ToString("0", _culture);
        }

        private void DrawEmpty(Graphics g, Rectangle bounds)
        {
            using (SolidBrush brush = new SolidBrush(MutedBlue))
            using (Font font = new Font("Segoe UI", 8F))
            {
                StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("No sales data for selected range", font, brush, bounds, format);
            }
        }

        private string ShortLabel(string text, int max)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "Unknown";
            return text.Length <= max ? text : text.Substring(0, max - 1) + ".";
        }

        private class TopQtyRow
        {
            public int Rank { get; set; }
            public string ItemName { get; set; }
            public string QtySold { get; set; }
            public string Amount { get; set; }
        }

        private class TopAmountRow
        {
            public int Rank { get; set; }
            public string ItemName { get; set; }
            public string Amount { get; set; }
            public string Profit { get; set; }
        }

        private class SalesItemDetailRow
        {
            public int Rank { get; set; }
            public string ItemName { get; set; }
            public string QtySold { get; set; }
            public string Amount { get; set; }
            public string Profit { get; set; }
            public string Range { get; set; }
        }

        private class SalesTrendDetailRow
        {
            public int Rank { get; set; }
            public string Caption { get; set; }
            public string SaleDate { get; set; }
            public string Value { get; set; }
            public string ItemName { get; set; }
            public string QtySold { get; set; }
            public string Amount { get; set; }
            public string Profit { get; set; }
        }

        private class CustomerSalesRow
        {
            public int Rank { get; set; }
            public string Customer { get; set; }
            public int Bills { get; set; }
            public string Amount { get; set; }
        }

        private enum MetricIconKind
        {
            Cart,
            Basket,
            Wallet,
            Profit,
            Box
        }
    }
}
