using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.Dashboard
{
    public partial class FrmPurchaseAnalytics : Form
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
        private PurchaseAnalyticsOverview _analytics = new PurchaseAnalyticsOverview();
        private bool _itemMapSortByAmount = true;

        public FrmPurchaseAnalytics()
        {
            InitializeComponent();
            if (IsDesignerHosted())
            {
                return;
            }

            ConfigureRuntimeUi();
            Load += FrmPurchaseAnalytics_Load;
            Resize += (s, e) => InvalidateCharts();
        }

        private void FrmPurchaseAnalytics_Load(object sender, EventArgs e)
        {
            LoadAnalytics();
        }

        private bool IsDesignerHosted()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime
                || DesignMode
                || (Site != null && Site.DesignMode);
        }

        private PurchaseAnalyticsOverview CreateDesignerAnalytics()
        {
            DateTime toDate = DateTime.Today;
            DateTime fromDate = toDate.AddDays(-6);
            List<PurchaseItemMetric> items = SamplePurchaseItems();

            return new PurchaseAnalyticsOverview
            {
                FromDate = fromDate,
                ToDate = toDate,
                Summary = new PurchaseAnalyticsSummary
                {
                    TotalPurchase = 28750M,
                    TotalVendors = 24,
                    AveragePurchaseValue = 1197.92M,
                    TotalItemsPurchased = 431M,
                    PurchaseChangePercent = 25.30M,
                    VendorsChangePercent = 9.09M,
                    AveragePurchaseValueChangePercent = 14.82M,
                    ItemsPurchasedChangePercent = 18.31M
                },
                PurchaseTrend = SamplePurchaseTrend(),
                TopByQuantity = items.OrderByDescending(x => x.QtyPurchased).ToList(),
                TopByAmount = items.OrderByDescending(x => x.Amount).ToList(),
                PaymentMethods = SamplePayments(),
                Categories = SampleCategories(),
                TopVendors = SampleVendors(),
                Brief = SampleBrief(),
                ItemPurchases = items
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
            ConfigureApplyButton();
            btnApply.Click += (s, e) =>
            {
                _fromDate = dtFrom.Value.Date;
                _toDate = dtTo.Value.Date;
                LoadAnalytics();
            };

            trendCanvas.Paint -= TrendCanvas_Paint;
            trendCanvas.Paint += TrendCanvas_Paint;
            paymentCanvas.Paint -= PaymentCanvas_Paint;
            paymentCanvas.Paint += PaymentCanvas_Paint;
            categoryCanvas.Paint -= CategoryCanvas_Paint;
            categoryCanvas.Paint += CategoryCanvas_Paint;
            gridTopQty.CellClick += GridTopQty_CellClick;
            gridTopAmount.CellClick += GridTopAmount_CellClick;
            paymentCanvas.Click += PaymentCanvas_Click;
            categoryCanvas.Click += CategoryCanvas_Click;
            gridTopQty.Cursor = Cursors.Hand;
            gridTopAmount.Cursor = Cursors.Hand;
            paymentCanvas.Cursor = Cursors.Hand;
            categoryCanvas.Cursor = Cursors.Hand;
            briefCanvas.Paint -= BriefCanvas_Paint;
            briefCanvas.Paint += BriefCanvas_Paint;
            trendCanvas.AutoScroll = true;
            trendCanvas.Resize += (s, e) =>
            {
                ConfigureTrendCanvasScroll();
                trendCanvas.Invalidate();
            };

            RegisterCardPaint(cardTotalPurchase);
            RegisterCardPaint(cardTotalVendors);
            RegisterCardPaint(cardAveragePurchase);
            RegisterCardPaint(cardItemsPurchased);
            RegisterCardPaint(trendPanel);
            RegisterCardPaint(briefPanel);
            RegisterCardPaint(topQtyPanel);
            RegisterCardPaint(topAmountPanel);
            RegisterCardPaint(paymentPanel);
            RegisterCardPaint(categoryPanel);
       

            RegisterMetricIcon(iconPurchase, AccentBlue, MetricIconKind.Cart);
            RegisterMetricIcon(iconVendors, AccentGreen, MetricIconKind.Truck);
            RegisterMetricIcon(iconAveragePurchase, AccentPurple, MetricIconKind.Wallet);
            RegisterMetricIcon(iconItemsPurchased, AccentTeal, MetricIconKind.Box);

            ConfigureBriefPanelControls();
            lblTrendTitle.Text = "Purchase Trend (Daily)";
            lblTopQtyTitle.Text = "Top Purchased Items (By Quantity)";
            lblTopAmountTitle.Text = "Top Purchased Items (By Amount)";
            lblPaymentTitle.Text = "Purchase by Payment Method";
            lblCategoryTitle.Text = "Purchase by Category";
            lblTotalPurchaseTitle.Text = "Total Purchase";
            lblVendorsTitle.Text = "Total Vendors";
            lblAveragePurchaseTitle.Text = "Average Purchase Value";
            lblItemsPurchasedTitle.Text = "Total Items Purchased";
            lblVendorsFooter.Text = "Active Vendors";

            gridTopQty.AutoGenerateColumns = true;
            gridTopAmount.AutoGenerateColumns = true;
        }

        private void ConfigureBriefPanelControls()
        {
            lblBriefTitle.Text = "Purchase Brief";
            lblBriefTitle.Location = new Point(14, 8);

            cmbItemMapSort.Items.Clear();
            cmbItemMapSort.Items.AddRange(new object[] { "By Amount", "By Quantity" });
            cmbItemMapSort.SelectedIndexChanged -= CmbItemMapSort_SelectedIndexChanged;
            cmbItemMapSort.SelectedIndex = 0;
            cmbItemMapSort.SelectedIndexChanged += CmbItemMapSort_SelectedIndexChanged;

            briefPanel.Resize += (s, e) =>
            {
                cmbItemMapSort.Location = new Point(Math.Max(16, briefPanel.Width - 118), 6);
                briefCanvas.Location = new Point(10, 34);
                briefCanvas.Size = new Size(Math.Max(40, briefPanel.ClientSize.Width - 20), Math.Max(40, briefPanel.ClientSize.Height - 44));
                briefCanvas.Invalidate();
            };

            cmbItemMapSort.Location = new Point(Math.Max(16, briefPanel.Width - 118), 6);
            briefCanvas.Location = new Point(10, 34);
            briefCanvas.Size = new Size(Math.Max(40, briefPanel.ClientSize.Width - 20), Math.Max(40, briefPanel.ClientSize.Height - 44));
        }

        private void CmbItemMapSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            _itemMapSortByAmount = cmbItemMapSort.SelectedIndex == 0;
            briefCanvas.Invalidate();
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
                using (PurchaseAnalyticsRepository repository = new PurchaseAnalyticsRepository())
                {
                    _analytics = repository.GetAnalytics(_fromDate, _toDate);
                }

                BindAnalytics();
            }
            catch (Exception ex)
            {
                _analytics = new PurchaseAnalyticsOverview { FromDate = _fromDate, ToDate = _toDate };
                BindAnalytics();
                MessageBox.Show("Purchase analytics could not be loaded.\n\n" + ex.Message, "Purchase Analytics", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BindAnalytics()
        {
            PurchaseAnalyticsSummary summary = _analytics.Summary ?? new PurchaseAnalyticsSummary();
        
            lblTotalPurchase.Text = Money(summary.TotalPurchase);
            lblVendors.Text = summary.TotalVendors.ToString("N0", _culture);
            lblAveragePurchase.Text = Money(summary.AveragePurchaseValue);
            lblItemsPurchased.Text = summary.TotalItemsPurchased.ToString("N0", _culture);
            lblPurchaseChange.Text = ChangeText(summary.PurchaseChangePercent);
            lblVendorsChange.Text = ChangeText(summary.VendorsChangePercent);
            lblAveragePurchaseChange.Text = ChangeText(summary.AveragePurchaseValueChangePercent);
            lblItemsChange.Text = ChangeText(summary.ItemsPurchasedChangePercent);
            ApplyChangeColor(lblPurchaseChange, summary.PurchaseChangePercent);
            ApplyChangeColor(lblVendorsChange, summary.VendorsChangePercent);
            ApplyChangeColor(lblAveragePurchaseChange, summary.AveragePurchaseValueChangePercent);
            ApplyChangeColor(lblItemsChange, summary.ItemsPurchasedChangePercent);

            gridTopQty.DataSource = null;
            gridTopQty.Rows.Clear();
            gridTopQty.DataSource = new BindingList<TopQtyRow>(_analytics.TopByQuantity.Select((x, i) => new TopQtyRow
            {
                Rank = i + 1,
                ItemName = x.ItemName,
                QtyPurchased = x.QtyPurchased.ToString("N2", _culture)
            }).ToList());

            gridTopAmount.DataSource = null;
            gridTopAmount.Rows.Clear();
            gridTopAmount.DataSource = new BindingList<TopAmountRow>(_analytics.TopByAmount.Select((x, i) => new TopAmountRow
            {
                Rank = i + 1,
                ItemName = x.ItemName,
                Amount = Money(x.Amount)
            }).ToList());

            
            InvalidateCharts();
            ConfigureTrendCanvasScroll();
        }

        private void InvalidateCharts()
        {
            if (trendCanvas != null) trendCanvas.Invalidate();
            if (briefCanvas != null) briefCanvas.Invalidate();
            if (paymentCanvas != null) paymentCanvas.Invalidate();
            if (categoryCanvas != null) categoryCanvas.Invalidate();
        }

        private void ConfigureTrendCanvasScroll()
        {
            if (trendCanvas == null)
                return;

            int pointCount = _analytics != null && _analytics.PurchaseTrend != null ? _analytics.PurchaseTrend.Count : 0;
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
            DrawLineChart(e.Graphics, trendCanvas, GetPurchaseTrendForPaint(), x => x.Caption, x => x.Amount);
        }

        private void PaymentCanvas_Paint(object sender, PaintEventArgs e)
        {
            DrawDonutBreakdown(e.Graphics, paymentCanvas.ClientRectangle, GetPaymentMethodsForPaint());
        }

        private void CategoryCanvas_Paint(object sender, PaintEventArgs e)
        {
            DrawDonutBreakdown(e.Graphics, categoryCanvas.ClientRectangle, GetCategoriesForPaint());
        }

        private void GridTopQty_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DashboardDrilldownPopup.ShowGrid(this,
                "All Purchased Items by Quantity - " + FormatDateRange(),
                BuildPurchaseDetailRows(GetItemPurchaseDetails().OrderByDescending(x => x.Qty)));
        }

        private void GridTopAmount_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DashboardDrilldownPopup.ShowGrid(this,
                "All Purchased Items by Amount - " + FormatDateRange(),
                BuildPurchaseDetailRows(GetItemPurchaseDetails().OrderByDescending(x => x.Amount)));
        }

        private void PaymentCanvas_Click(object sender, EventArgs e)
        {
            DashboardDrilldownPopup.ShowGrid(this,
                "Purchase Payment Methods - " + FormatDateRange(),
                GetPaymentMethodsForPaint());
        }

        private void CategoryCanvas_Click(object sender, EventArgs e)
        {
            DashboardDrilldownPopup.ShowGrid(this,
                "Purchased Item Categories - " + FormatDateRange(),
                GetCategoriesForPaint().OrderByDescending(x => x.Amount));
        }

        private string FormatDateRange()
        {
            return _fromDate.ToString("dd MMM yyyy", _culture) + " to " + _toDate.ToString("dd MMM yyyy", _culture);
        }

        private IList<PurchaseItemDetail> GetItemPurchaseDetails()
        {
            return _analytics != null && _analytics.ItemPurchaseDetails != null
                ? _analytics.ItemPurchaseDetails
                : new List<PurchaseItemDetail>();
        }

        private List<PurchaseItemPopupRow> BuildPurchaseDetailRows(IEnumerable<PurchaseItemDetail> details)
        {
            return (details ?? Enumerable.Empty<PurchaseItemDetail>())
                .Where(x => x != null)
                .Select((x, index) => new PurchaseItemPopupRow
                {
                    Rank = index + 1,
                    PurchaseNo = x.PurchaseNo.ToString(_culture),
                    ItemName = x.ItemName,
                    Vendor = x.Vendor,
                    Qty = x.Qty.ToString("N2", _culture),
                    Cost = Money(x.Cost),
                    Amount = Money(x.Amount),
                    Range = FormatDateRange()
                })
                .ToList();
        }

        private void BriefCanvas_Paint(object sender, PaintEventArgs e)
        {
            IList<PurchaseItemMetric> items = GetItemPurchasesForPaint()
                .OrderByDescending(x => _itemMapSortByAmount ? x.Amount : x.QtyPurchased)
                .ToList();
            DrawBarChart(e.Graphics, briefCanvas.ClientRectangle, items, x => x.ItemName, x => _itemMapSortByAmount ? x.Amount : x.QtyPurchased, AccentBlue);
        }

        private IList<PurchaseTrendPoint> GetPurchaseTrendForPaint()
        {
            if (_analytics != null && _analytics.PurchaseTrend != null && _analytics.PurchaseTrend.Count > 0)
                return _analytics.PurchaseTrend;

            return IsInDesigner() ? SamplePurchaseTrend() : new List<PurchaseTrendPoint>();
        }

        private IList<PurchaseBreakdown> GetPaymentMethodsForPaint()
        {
            if (_analytics != null && _analytics.PaymentMethods != null && _analytics.PaymentMethods.Count > 0)
                return _analytics.PaymentMethods;

            return IsInDesigner() ? SamplePayments() : new List<PurchaseBreakdown>();
        }

        private IList<PurchaseBreakdown> GetCategoriesForPaint()
        {
            if (_analytics != null && _analytics.Categories != null && _analytics.Categories.Count > 0)
                return _analytics.Categories;

            return IsInDesigner() ? SampleCategories() : new List<PurchaseBreakdown>();
        }

        private IList<PurchaseItemMetric> GetItemPurchasesForPaint()
        {
            if (_analytics != null && _analytics.ItemPurchases != null && _analytics.ItemPurchases.Count > 0)
                return _analytics.ItemPurchases;

            return IsInDesigner() ? SamplePurchaseItems() : new List<PurchaseItemMetric>();
        }

        private bool IsInDesigner()
        {
            return IsDesignerHosted();
        }

        private List<PurchaseTrendPoint> SamplePurchaseTrend()
        {
            return new List<PurchaseTrendPoint>
            {
                new PurchaseTrendPoint { Caption = "14 May", Amount = 3200M },
                new PurchaseTrendPoint { Caption = "15 May", Amount = 4100M },
                new PurchaseTrendPoint { Caption = "16 May", Amount = 3800M },
                new PurchaseTrendPoint { Caption = "17 May", Amount = 5200M },
                new PurchaseTrendPoint { Caption = "18 May", Amount = 4600M },
                new PurchaseTrendPoint { Caption = "19 May", Amount = 6100M },
                new PurchaseTrendPoint { Caption = "20 May", Amount = 5350M }
            };
        }

        private List<PurchaseItemMetric> SamplePurchaseItems()
        {
            return new List<PurchaseItemMetric>
            {
                new PurchaseItemMetric { ItemName = "Keyboard", QtyPurchased = 120M, Amount = 2250M },
                new PurchaseItemMetric { ItemName = "Mouse", QtyPurchased = 95M, Amount = 1900M },
                new PurchaseItemMetric { ItemName = "USB Pendrive", QtyPurchased = 80M, Amount = 1600M },
                new PurchaseItemMetric { ItemName = "Power Adapter", QtyPurchased = 60M, Amount = 1500M },
                new PurchaseItemMetric { ItemName = "HDMI Cable", QtyPurchased = 45M, Amount = 900M },
                new PurchaseItemMetric { ItemName = "Dell Laptop", QtyPurchased = 12M, Amount = 22500M },
                new PurchaseItemMetric { ItemName = "HP Laptop", QtyPurchased = 8M, Amount = 9500M },
                new PurchaseItemMetric { ItemName = "Monitor", QtyPurchased = 10M, Amount = 4800M }
            };
        }

        private List<PurchaseBreakdown> SamplePayments()
        {
            return new List<PurchaseBreakdown>
            {
                new PurchaseBreakdown { Name = "Cash", Amount = 12540M, Count = 12 },
                new PurchaseBreakdown { Name = "UPI", Amount = 7850M, Count = 8 },
                new PurchaseBreakdown { Name = "Card", Amount = 4890M, Count = 5 },
                new PurchaseBreakdown { Name = "Bank Transfer", Amount = 2790M, Count = 2 },
                new PurchaseBreakdown { Name = "Other", Amount = 680M, Count = 1 }
            };
        }

        private List<PurchaseBreakdown> SampleCategories()
        {
            return new List<PurchaseBreakdown>
            {
                new PurchaseBreakdown { Name = "Electronics", Amount = 15840M, Count = 10 },
                new PurchaseBreakdown { Name = "Accessories", Amount = 6240M, Count = 8 },
                new PurchaseBreakdown { Name = "Computer", Amount = 4650M, Count = 4 },
                new PurchaseBreakdown { Name = "Others", Amount = 2020M, Count = 2 }
            };
        }

        private List<PurchaseVendorMetric> SampleVendors()
        {
            return new List<PurchaseVendorMetric>
            {
                new PurchaseVendorMetric { VendorName = "Tech Source Pvt Ltd", Amount = 12500M },
                new PurchaseVendorMetric { VendorName = "Global Infotech", Amount = 6750M },
                new PurchaseVendorMetric { VendorName = "Smart Traders", Amount = 4850M },
                new PurchaseVendorMetric { VendorName = "Digital World", Amount = 2950M },
                new PurchaseVendorMetric { VendorName = "Office Essentials", Amount = 1700M }
            };
        }

        private PurchaseBriefSummary SampleBrief()
        {
            return new PurchaseBriefSummary
            {
                TotalPurchase = 28750M,
                TotalItemsPurchased = 431M,
                PurchaseReturn = 1250M,
                NetPurchase = 27500M,
                LowStockItems = 23,
                OutOfStockItems = 7
            };
        }

        private void DrawLineChart(Graphics g, Panel canvas, IList<PurchaseTrendPoint> points, Func<PurchaseTrendPoint, string> label, Func<PurchaseTrendPoint, decimal> value)
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

        private void DrawBarChart(Graphics g, Rectangle bounds, IList<PurchaseItemMetric> items, Func<PurchaseItemMetric, string> label, Func<PurchaseItemMetric, decimal> value, Color color)
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

        private void DrawDonutBreakdown(Graphics g, Rectangle bounds, IList<PurchaseBreakdown> items)
        {
            PrepareGraphics(g);
            IList<PurchaseBreakdown> visibleItems = (items ?? new List<PurchaseBreakdown>()).Where(x => x != null).ToList();
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

                int y = bounds.Top + (compact ? 16 : 24);
                int legendX = pie.Right + (compact ? 8 : 16);
                int amountX = Math.Max(legendX + 118, bounds.Right - 112);
                int rowHeight = compact ? 28 : 22;
                for (int i = 0; i < visibleItems.Count && y < bounds.Bottom - 14; i++)
                {
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
            }
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
                        case MetricIconKind.Truck:
                            g.DrawRectangle(pen, cx - 14, cy - 7, 17, 13);
                            g.DrawRectangle(pen, cx + 3, cy - 3, 10, 9);
                            g.FillEllipse(brush, cx - 11, cy + 8, 5, 5);
                            g.FillEllipse(brush, cx + 6, cy + 8, 5, 5);
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
                g.DrawString("No purchase data for selected range", font, brush, bounds, format);
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
            public string QtyPurchased { get; set; }
        }

        private class TopAmountRow
        {
            public int Rank { get; set; }
            public string ItemName { get; set; }
            public string Amount { get; set; }
        }

        private class TopVendorRow
        {
            public int Rank { get; set; }
            public string VendorName { get; set; }
            public string Amount { get; set; }
        }

        private enum MetricIconKind
        {
            Cart,
            Basket,
            Wallet,
            Truck,
            Box
        }

        private class PurchaseItemPopupRow
        {
            public int Rank { get; set; }
            public string PurchaseNo { get; set; }
            public string ItemName { get; set; }
            public string Vendor { get; set; }
            public string Qty { get; set; }
            public string Cost { get; set; }
            public string Amount { get; set; }
            public string Range { get; set; }
        }
    }

    internal static class DashboardDrilldownPopup
    {
        public static void ShowGrid<T>(IWin32Window owner, string title, IEnumerable<T> rows)
        {
            Form popup = new Form
            {
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(820, 470),
                MinimizeBox = false,
                MaximizeBox = false,
                ShowIcon = false,
                BackColor = Color.FromArgb(230, 245, 253),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Padding = new Padding(14)
            };

            Panel card = new Panel
            {
                BackColor = Color.FromArgb(250, 253, 255),
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 10, 12, 12)
            };
            Label heading = new Label
            {
                Dock = DockStyle.Top,
                Height = 32,
                Text = title,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(18, 49, 102),
                Padding = new Padding(3, 3, 0, 0)
            };
            DataGridView grid = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToResizeRows = false,
                BackgroundColor = Color.FromArgb(250, 253, 255),
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(220, 233, 246),
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                DataSource = new BindingList<T>((rows ?? Enumerable.Empty<T>()).ToList())
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(232, 241, 252);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(18, 49, 102);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            grid.ColumnHeadersHeight = 32;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(36, 64, 105);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 232, 250);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(18, 49, 102);
            grid.RowTemplate.Height = 28;
            foreach (DataGridViewColumn column in grid.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                if (column.Name == "Rank")
                    column.FillWeight = 34;
                else if (column.Name == "ItemName" || column.Name == "Vendor")
                    column.FillWeight = 125;
                else if (column.Name == "Range")
                    column.FillWeight = 115;
                else
                    column.FillWeight = 72;
            }

            Button close = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(54, 126, 235),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                Location = new Point(popup.ClientSize.Width - 105, 16),
                Size = new Size(72, 28),
                Text = "Close"
            };
            close.FlatAppearance.BorderSize = 0;
            close.Click += (s, e) => popup.Close();

            card.Controls.Add(grid);
            card.Controls.Add(heading);
            card.Controls.Add(close);
            close.BringToFront();
            popup.Controls.Add(card);
            popup.ShowDialog(owner);
        }
    }
}
