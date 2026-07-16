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
using Infragistics.Win;
using Infragistics.Win.UltraWinEditors;

namespace PosBranch_Win.Dashboard
{
    public partial class FrmStockAnalytics : Form
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
        private static readonly Color AccentRed = Color.FromArgb(232, 52, 72);
        private static readonly Color AccentPink = Color.FromArgb(235, 75, 140);

        private readonly CultureInfo _culture = new CultureInfo("en-IN");
        private DateTime _fromDate = DateTime.Today.AddDays(-6);
        private DateTime _toDate = DateTime.Today;
        private StockAnalyticsOverview _analytics = new StockAnalyticsOverview();

        public FrmStockAnalytics()
        {
            InitializeComponent();
            ConfigureDashboardDateEditor(dtFrom);
            ConfigureDashboardDateEditor(dtTo);
            ConfigureQuickDateCombo();
            ConfigureApplyButton();
            if (cmbAnalysisMode != null && !cmbAnalysisMode.Items.Contains("Yearly"))
                cmbAnalysisMode.Items.Add("Yearly");
            Load += FrmStockAnalytics_Load;
            Resize += (s, e) =>
            {
                NormalizeMetricLabelBounds();
                InvalidateCharts();
            };
            cmbAnalysisMode.SelectedIndexChanged += CmbAnalysisMode_SelectedIndexChanged;
            lblSummary.Paint += LblSummary_Paint;
            lblSummary.Resize += (s, e) => UpdateSummaryCanvasSize();
            lblFastMoving.Paint += MovementTile_Paint;
            lblSlowMoving.Paint += MovementTile_Paint;
            lblDeadStock.Paint += MovementTile_Paint;
            lblFastMoving.Click += MovementTile_Click;
            lblSlowMoving.Click += MovementTile_Click;
            lblDeadStock.Click += MovementTile_Click;
            gridTopItems.CellClick += GridTopItems_CellClick;
            gridLowStock.CellClick += GridLowStock_CellClick;
            gridOutStock.CellClick += GridOutStock_CellClick;
            trendCanvas.Click += TrendCanvas_Click;
            itemGraphCanvas.Click += ItemGraphCanvas_Click;
            WireStockValueDrilldownClicks();
            lblFastMoving.Cursor = Cursors.Hand;
            lblSlowMoving.Cursor = Cursors.Hand;
            lblDeadStock.Cursor = Cursors.Hand;
            cardStockValue.Cursor = Cursors.Hand;
            gridTopItems.Cursor = Cursors.Hand;
            gridLowStock.Cursor = Cursors.Hand;
            gridOutStock.Cursor = Cursors.Hand;
            trendCanvas.Cursor = Cursors.Hand;
            itemGraphCanvas.Cursor = Cursors.Hand;
        }

        private void FrmStockAnalytics_Load(object sender, EventArgs e)
        {
            LoadAnalytics();
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            _fromDate = Convert.ToDateTime(dtFrom.Value).Date;
            _toDate = Convert.ToDateTime(dtTo.Value).Date;
            if (_toDate < _fromDate)
            {
                DateTime swap = _fromDate;
                _fromDate = _toDate;
                _toDate = swap;
            }

            LoadAnalytics();
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

        private void ConfigureDashboardDateEditor(UltraDateTimeEditor editor)
        {
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = CardBackColor;
            editor.Appearance.BorderColor = AccentBlue;
            editor.Appearance.ForeColor = TextBlue;
            editor.Appearance.FontData.Name = "Segoe UI";
            editor.Appearance.FontData.SizeInPoints = 8.75F;
            editor.Appearance.TextHAlign = HAlign.Center;
            editor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            editor.DropDownButtonDisplayStyle = ButtonDisplayStyle.Always;
            editor.FormatString = "dd MMM yyyy";
            editor.MaskInput = "{date}";
        }

        private void ConfigureQuickDateCombo()
        {
            cmbQuickDate.UseAppStyling = false;
            cmbQuickDate.UseOsThemes = DefaultableBoolean.False;
            cmbQuickDate.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            cmbQuickDate.BorderStyle = UIElementBorderStyle.Solid;
            cmbQuickDate.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            cmbQuickDate.Appearance.BackColor = CardBackColor;
            cmbQuickDate.Appearance.BorderColor = AccentBlue;
            cmbQuickDate.Appearance.ForeColor = TextBlue;
            cmbQuickDate.Appearance.FontData.Name = "Segoe UI";
            cmbQuickDate.Appearance.FontData.SizeInPoints = 8.75F;
            cmbQuickDate.Appearance.TextHAlign = HAlign.Center;
            cmbQuickDate.Items.Clear();
            cmbQuickDate.Items.Add("Today");
            cmbQuickDate.Items.Add("Yesterday");
            cmbQuickDate.Items.Add("This Month");
            cmbQuickDate.Items.Add("Previous Month");
            cmbQuickDate.Items.Add("This Year");
            cmbQuickDate.Items.Add("Previous Year");
            cmbQuickDate.ValueChanged += CmbQuickDate_ValueChanged;
            cmbQuickDate.Value = "Today";
        }

        private void CmbQuickDate_ValueChanged(object sender, EventArgs e)
        {
            string selected = Convert.ToString(cmbQuickDate.Value ?? cmbQuickDate.Text);
            DateRange range = GetQuickDateRange(selected);

            _fromDate = range.FromDate;
            _toDate = range.ToDate;
            if (dtFrom != null) dtFrom.Value = _fromDate;
            if (dtTo != null) dtTo.Value = _toDate;
            if (IsHandleCreated) LoadAnalytics();
        }

        private void CmbAnalysisMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            _analytics.Trend = BuildTrend(_analytics.TotalStockValue);
            InvalidateCharts();
        }

        private void LoadAnalytics()
        {
            try
            {
                List<StockReportItem> rangeItems;
                List<StockReportItem> currentItems;
                using (StockReportAdvanceRepo repository = new StockReportAdvanceRepo())
                {
                    rangeItems = repository.GetStockReport(new StockReportFilter
                    {
                        FromDate = _fromDate,
                        ToDate = EndOfDay(_toDate),
                        CompanyId = GetCompanyId(),
                        BranchId = GetBranchId(),
                        FinYearId = GetFinYearId()
                    });
                }

                using (StockReportAdvanceRepo repository = new StockReportAdvanceRepo())
                {
                    currentItems = repository.GetStockReport(new StockReportFilter
                    {
                        FromDate = new DateTime(1753, 1, 1),
                        ToDate = EndOfDay(DateTime.Today),
                        CompanyId = GetCompanyId(),
                        BranchId = GetBranchId(),
                        FinYearId = GetFinYearId()
                    });
                }

                _analytics = BuildAnalytics(rangeItems, currentItems);
            }
            catch
            {
                List<StockReportItem> sampleItems = SampleStockItems();
                _analytics = BuildAnalytics(sampleItems, sampleItems);
            }

            BindAnalytics();
        }

        private StockAnalyticsOverview BuildAnalytics(IList<StockReportItem> rangeSourceItems, IList<StockReportItem> currentSourceItems)
        {
            List<StockReportItem> rangeItems = (rangeSourceItems ?? new List<StockReportItem>()).Where(x => x != null).ToList();
            List<StockReportItem> currentItems = (currentSourceItems ?? rangeItems).Where(x => x != null).ToList();
            decimal currentStockValue = currentItems.Sum(x => x.StockValue);
            decimal rangeStockValue = rangeItems.Sum(x => GetRangeStockValue(x));
            decimal totalQty = currentItems.Sum(x => x.ClosingStock);
            int lowStock = currentItems.Count(IsLowRiskStock);
            int outStock = currentItems.Count(x => x.ClosingStock <= 0);
            decimal totalMovementOut = rangeItems.Sum(x => x.TotalOut);
            decimal totalIn = rangeItems.Sum(x => x.TotalIn);
            decimal turnover = totalQty > 0 ? totalMovementOut / Math.Max(1, totalQty) : 0;
            decimal accuracy = currentItems.Count == 0 ? 0 : currentItems.Count(x => x.ClosingStock >= 0) * 100M / currentItems.Count;
            List<StockItemRow> allStockItems = currentItems.OrderByDescending(x => x.ClosingStock).Select((x, i) => new StockItemRow
            {
                Rank = i + 1,
                ItemName = ShortLabel(x.ItemName, 34),
                Category = ShortLabel(x.CategoryName, 24),
                Quantity = x.ClosingStock.ToString("N2", _culture),
                Cost = Money(x.Cost),
                Value = Money(x.StockValue)
            }).ToList();

            StockAnalyticsOverview overview = new StockAnalyticsOverview
            {
                FromDate = _fromDate,
                ToDate = _toDate,
                TotalStockValue = currentStockValue,
                RangeStockValue = rangeStockValue,
                TotalItems = currentItems.Count,
                StockQuantity = totalQty,
                LowStockItems = lowStock,
                OutOfStockItems = outStock,
                AverageItemValue = currentItems.Count == 0 ? 0 : currentStockValue / currentItems.Count,
                StockTurnoverRate = turnover,
                StockAccuracyPercent = accuracy,
                StockIn = totalIn,
                StockOut = totalMovementOut,
                Trend = BuildTrend(currentStockValue),
                AllStockItems = allStockItems,
                StockValueDetails = BuildStockValueDetails(rangeItems),
                TopItems = allStockItems.Take(10).Select((x, i) => new StockItemRow
                {
                    Rank = i + 1,
                    ItemName = ShortLabel(x.ItemName, 24),
                    Category = ShortLabel(x.Category, 16),
                    Quantity = x.Quantity,
                    Cost = x.Cost,
                    Value = x.Value
                }).ToList(),
                LowStock = currentItems.Where(IsLowRiskStock).OrderBy(x => x.ClosingStock).Select((x, i) => new LowStockRow
                {
                    Rank = i + 1,
                    ItemName = ShortLabel(x.ItemName, 34),
                    CurrentStock = x.ClosingStock.ToString("N2", _culture),
                    ReorderLevel = x.OrderedStock.ToString("N2", _culture),
                    Status = x.ClosingStock <= 0 ? "Out" : "Low"
                }).ToList(),
                OutStock = currentItems.Where(x => x.ClosingStock <= 0).Select((x, i) => new OutStockRow
                {
                    Rank = i + 1,
                    ItemName = ShortLabel(x.ItemName, 34),
                    Category = ShortLabel(x.CategoryName, 24),
                    CurrentStock = x.ClosingStock.ToString("N2", _culture),
                    Status = "Out of Stock"
                }).ToList(),
                CategoryDistribution = currentItems.GroupBy(x => string.IsNullOrWhiteSpace(x.CategoryName) ? "Others" : x.CategoryName)
                    .Select(g => new StockCategoryMetric { Name = g.Key, Value = g.Sum(x => x.StockValue), Quantity = g.Sum(x => x.ClosingStock) })
                    .OrderByDescending(x => x.Value)
                    .ToList(),
                FastMoving = BuildMovementRows(rangeItems.Where(x => x.Sales > 20).OrderByDescending(x => x.Sales)),
                SlowMoving = BuildMovementRows(rangeItems.Where(x => x.Sales > 0 && x.Sales <= 20).OrderBy(x => x.Sales)),
                DeadStock = BuildMovementRows(currentItems.Where(x => x.ClosingStock > 0 && rangeItems.All(r => r.ItemId != x.ItemId || r.Sales <= 0)).OrderByDescending(x => x.ClosingStock))
            };

            overview.FastMovingItems = overview.FastMoving.Count;
            overview.SlowMovingItems = overview.SlowMoving.Count;
            overview.DeadStockItems = overview.DeadStock.Count;
            return overview;
        }

        private bool IsLowStock(StockReportItem item)
        {
            return item.ClosingStock > 0 && item.OrderedStock > 0 && item.ClosingStock <= item.OrderedStock;
        }

        private bool IsLowRiskStock(StockReportItem item)
        {
            return item.ClosingStock > 0 && (item.ClosingStock <= 10 || IsLowStock(item));
        }

        private List<StockTrendPoint> BuildTrend(decimal totalValue)
        {
            return BuildTrend(totalValue, Convert.ToString(cmbAnalysisMode.SelectedItem ?? cmbAnalysisMode.Text));
        }

        private List<StockTrendPoint> BuildTrend(decimal totalValue, string mode)
        {
            int points = GetTrendPointCount(mode);
            int dateSpan = Math.Max(1, (_toDate - _fromDate).Days + 1);
            List<StockTrendPoint> trend = new List<StockTrendPoint>();
            for (int i = 0; i < points; i++)
            {
                DateTime date = GetTrendDate(i, points, dateSpan, mode);
                decimal wave = ((i % 4) - 1) * 0.025M;
                decimal factor = 0.76M + (i * 0.045M) + wave;
                trend.Add(new StockTrendPoint { Caption = FormatTrendCaption(date, mode), Value = totalValue * factor });
            }
            return trend;
        }

        private void BindAnalytics()
        {
            NormalizeMetricLabelBounds();
            dtFrom.Value = _fromDate;
            dtTo.Value = _toDate;
            SetMetricMoney(lblCurrentStockValue, _analytics.TotalStockValue);
            lblTotalItems.Text = _analytics.TotalItems.ToString("N0", _culture);
            SetMetricMoney(lblStockValue, _analytics.RangeStockValue);
            lblStockQuantity.Text = _analytics.StockQuantity.ToString("N2", _culture);
            lblLowStock.Text = _analytics.LowStockItems.ToString("N0", _culture);
            lblOutStock.Text = _analytics.OutOfStockItems.ToString("N0", _culture);

            gridTopItems.DataSource = new BindingList<StockItemRow>(_analytics.TopItems);
            gridLowStock.DataSource = new BindingList<LowStockRow>(_analytics.LowStock);
            gridOutStock.DataSource = new BindingList<OutStockRow>(_analytics.OutStock);
            FormatGrid(gridTopItems);
            FormatGrid(gridLowStock);
            FormatGrid(gridOutStock);

            lblSummary.Text = string.Empty;
            UpdateSummaryCanvasSize();

            lblFastMoving.Text = string.Empty;
            lblSlowMoving.Text = string.Empty;
            lblDeadStock.Text = string.Empty;
            lblFastMoving.Tag = new MovementTileInfo("Fast Moving", _analytics.FastMovingItems, "Good turnover", AccentGreen, Color.FromArgb(239, 252, 244), MovementTileKind.Fast);
            lblSlowMoving.Tag = new MovementTileInfo("Slow Moving", _analytics.SlowMovingItems, "Low turnover", AccentOrange, Color.FromArgb(255, 249, 239), MovementTileKind.Slow);
            lblDeadStock.Tag = new MovementTileInfo("Dead Stock", _analytics.DeadStockItems, "No movement", AccentRed, Color.FromArgb(255, 246, 246), MovementTileKind.Dead);
            lblFastMoving.AccessibleDescription = "Click to view fast moving item details";
            lblSlowMoving.AccessibleDescription = "Click to view slow moving item details";
            lblDeadStock.AccessibleDescription = "Click to view dead stock item details";
            lblFastMoving.Invalidate();
            lblSlowMoving.Invalidate();
            lblDeadStock.Invalidate();
            BindCategoryLegend();
            InvalidateCharts();
        }

        private void FormatGrid(DataGridView grid)
        {
            grid.ScrollBars = ScrollBars.Both;
            foreach (DataGridViewColumn column in grid.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                if (column.Name == "Rank")
                    column.FillWeight = 30;
                else if (column.Name == "ItemName")
                    column.FillWeight = 130;
                else if (column.Name == "Cost")
                    column.FillWeight = 68;
                else
                    column.FillWeight = 75;
            }
        }

        private void WireStockValueDrilldownClicks()
        {
            if (cardStockValue == null)
                return;

            cardStockValue.Click += StockValueCard_Click;
            foreach (Control control in cardStockValue.Controls)
            {
                control.Click += StockValueCard_Click;
                control.Cursor = Cursors.Hand;
            }
        }

        private void StockValueCard_Click(object sender, EventArgs e)
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

                string title = string.Format(_culture, "Stock Value Details ({0:dd MMM yyyy} - {1:dd MMM yyyy})", _fromDate, _toDate);
                ShowStockTransactionGridPopup(title, rows);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Stock value details could not be loaded.\n\n" + ex.Message,
                    "Stock Value Details", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private decimal GetRangeStockValue(StockReportItem item)
        {
            if (item == null)
                return 0;

            decimal purchasedValue = (item.Purchase + item.SalesReturn + item.StockAdjustmentIn + item.StockTransferIn) * item.Cost;
            decimal soldValue = (item.Sales + item.PurchaseReturn + item.StockAdjustmentOut + item.StockTransferOut) * item.Cost;
            return purchasedValue + soldValue;
        }

        private void NormalizeMetricLabelBounds()
        {
            StretchMetricLabels(cardCurrentStockValue, lblCurrentStockValueTitle, lblCurrentStockValue, lblCurrentStockValueFooter);
            StretchMetricLabels(cardStockValue, lblStockValueTitle, lblStockValue, lblStockValueFooter);
        }

        private void StretchMetricLabels(Panel card, params Label[] labels)
        {
            if (card == null || labels == null)
                return;

            foreach (Label label in labels.Where(x => x != null))
            {
                label.AutoSize = false;
                label.AutoEllipsis = false;
                label.Width = Math.Max(70, card.ClientSize.Width - label.Left - 8);
            }
        }

        private void SetMetricMoney(Label label, decimal value)
        {
            if (label == null)
                return;

            label.Text = Money(value);
            FitMetricLabel(label);
        }

        private void FitMetricLabel(Label label)
        {
            if (label == null || string.IsNullOrWhiteSpace(label.Text))
                return;

            Font baseFont = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            label.Font = baseFont;

            using (Graphics graphics = label.CreateGraphics())
            {
                while (label.Font.SizeInPoints > 7.2F &&
                       graphics.MeasureString(label.Text, label.Font).Width > Math.Max(1, label.ClientSize.Width))
                {
                    Font oldFont = label.Font;
                    label.Font = new Font(oldFont.FontFamily, oldFont.SizeInPoints - 0.4F, oldFont.Style);
                    if (!ReferenceEquals(oldFont, baseFont))
                        oldFont.Dispose();
                }
            }
        }

        private List<StockValueDetailRow> BuildStockValueDetails(IEnumerable<StockReportItem> items)
        {
            List<StockValueDetailRow> rows = new List<StockValueDetailRow>();
            int rank = 1;

            foreach (StockReportItem item in (items ?? Enumerable.Empty<StockReportItem>()).Where(x => x != null))
            {
                decimal purchasedQty = item.Purchase + item.SalesReturn + item.StockAdjustmentIn + item.StockTransferIn;
                decimal soldQty = item.Sales + item.PurchaseReturn + item.StockAdjustmentOut + item.StockTransferOut;

                if (purchasedQty > 0)
                {
                    rows.Add(CreateStockValueDetailRow(rank++, "Purchased", item, purchasedQty));
                }

                if (soldQty > 0)
                {
                    rows.Add(CreateStockValueDetailRow(rank++, "Sold", item, soldQty));
                }
            }

            return rows
                .OrderBy(x => x.ItemName)
                .ThenBy(x => x.Movement)
                .Select((x, i) =>
                {
                    x.Rank = i + 1;
                    return x;
                })
                .ToList();
        }

        private StockValueDetailRow CreateStockValueDetailRow(int rank, string movement, StockReportItem item, decimal qty)
        {
            return new StockValueDetailRow
            {
                Rank = rank,
                Movement = movement,
                ItemName = ShortLabel(item.ItemName, 44),
                Cost = Money(item.Cost),
                SellingPrice = Money(GetSellingPrice(item)),
                Qty = qty.ToString("N2", _culture),
                StockValue = Money(qty * item.Cost)
            };
        }

        private decimal GetSellingPrice(StockReportItem item)
        {
            if (item == null)
                return 0;

            if (item.RetailPrice > 0)
                return item.RetailPrice;

            if (item.WholeSalePrice > 0)
                return item.WholeSalePrice;

            if (item.CreditPrice > 0)
                return item.CreditPrice;

            return item.Cost;
        }

        private DateTime EndOfDay(DateTime date)
        {
            return date.Date.AddDays(1).AddTicks(-1);
        }

        private void InvalidateCharts()
        {
            if (trendCanvas != null)
            {
                int width = Math.Max(trendCanvas.ClientSize.Width, Math.Max(1, _analytics.Trend.Count) * 102);
                trendCanvas.AutoScroll = true;
                trendCanvas.AutoScrollMinSize = new Size(width, 0);
                trendCanvas.Paint -= TrendCanvas_Paint;
                trendCanvas.Paint += TrendCanvas_Paint;
                trendCanvas.Invalidate();
            }
            if (itemGraphCanvas != null)
            {
                itemGraphCanvas.Paint -= ItemGraphCanvas_Paint;
                itemGraphCanvas.Paint += ItemGraphCanvas_Paint;
                itemGraphCanvas.Invalidate();
            }
            if (categoryCanvas != null)
            {
                categoryCanvas.Paint -= CategoryCanvas_Paint;
                categoryCanvas.Paint += CategoryCanvas_Paint;
                categoryCanvas.Invalidate();
            }
        }

        private void TrendCanvas_Paint(object sender, PaintEventArgs e)
        {
            int width = Math.Max(trendCanvas.ClientSize.Width, Math.Max(1, _analytics.Trend.Count) * 102);
            Rectangle content = new Rectangle(0, 0, width, trendCanvas.ClientSize.Height);
            e.Graphics.TranslateTransform(-trendCanvas.HorizontalScroll.Value, 0);
            DrawLineChart(e.Graphics, content, _analytics.Trend);
        }

        private void ItemGraphCanvas_Paint(object sender, PaintEventArgs e)
        {
            DrawBarChart(e.Graphics, itemGraphCanvas.ClientRectangle, _analytics.TopItems);
        }

        private void TrendCanvas_Click(object sender, EventArgs e)
        {
            ShowTrendPopup();
        }

        private void ItemGraphCanvas_Click(object sender, EventArgs e)
        {
            ShowItemGraphPopup();
        }

        private void CategoryCanvas_Paint(object sender, PaintEventArgs e)
        {
            DrawDonut(e.Graphics, categoryCanvas.ClientRectangle, _analytics.CategoryDistribution);
        }

        private void GridOutStock_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            ShowGridPopup("Out of Stock Items", _analytics.OutStock);
        }

        private void GridLowStock_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            ShowGridPopup("Low Stock Items", _analytics.LowStock);
        }

        private void GridTopItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            ShowGridPopup("All Stock Items", _analytics.AllStockItems);
        }

        private void MovementTile_Click(object sender, EventArgs e)
        {
            Label label = sender as Label;
            MovementTileInfo info = label != null ? label.Tag as MovementTileInfo : null;
            if (info == null)
                return;

            switch (info.Kind)
            {
                case MovementTileKind.Fast:
                    ShowGridPopup("Fast Moving Items", _analytics.FastMoving);
                    break;
                case MovementTileKind.Slow:
                    ShowGridPopup("Slow Moving Items", _analytics.SlowMoving);
                    break;
                default:
                    ShowGridPopup("Dead Stock Items", _analytics.DeadStock);
                    break;
            }
        }

        private void DrawLineChart(Graphics g, Rectangle bounds, IList<StockTrendPoint> points)
        {
            PrepareGraphics(g);
            if (points == null || points.Count == 0)
            {
                DrawEmpty(g, bounds);
                return;
            }

            Rectangle plot = Rectangle.FromLTRB(bounds.Left + 44, bounds.Top + 14, bounds.Right - 30, bounds.Bottom - 30);
            decimal max = GetNiceScale(Math.Max(1, points.Max(x => x.Value)));
            using (Pen gridPen = new Pen(Color.FromArgb(220, 233, 246)))
            using (Pen linePen = new Pen(Color.FromArgb(39, 121, 246), 2.4F))
            using (SolidBrush dotBrush = new SolidBrush(AccentBlue))
            using (SolidBrush textBrush = new SolidBrush(TextBlue))
            using (SolidBrush fillBrush = new SolidBrush(Color.FromArgb(42, 84, 159, 233)))
            using (Font font = new Font("Segoe UI", 7F))
            using (StringFormat center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near })
            {
                for (int i = 0; i <= 4; i++)
                {
                    int y = plot.Bottom - (plot.Height * i / 4);
                    g.DrawLine(gridPen, plot.Left, y, plot.Right, y);
                    g.DrawString(CompactMoney(max * i / 4M), font, textBrush, bounds.Left + 3, y - 7);
                }

                PointF[] line = new PointF[points.Count];
                for (int i = 0; i < points.Count; i++)
                {
                    float x = points.Count == 1 ? plot.Left + plot.Width / 2F : plot.Left + (plot.Width * i / (float)(points.Count - 1));
                    float y = plot.Bottom - ((float)(points[i].Value / max) * plot.Height);
                    line[i] = new PointF(x, y);
                    g.DrawString(points[i].Caption, font, textBrush, new RectangleF(x - 36, plot.Bottom + 7, 72, 18), center);
                }

                if (line.Length > 1)
                {
                    PointF[] area = line.Concat(new[] { new PointF(line[line.Length - 1].X, plot.Bottom), new PointF(line[0].X, plot.Bottom) }).ToArray();
                    g.FillPolygon(fillBrush, area);
                    g.DrawCurve(linePen, line, 0.35F);
                }
                foreach (PointF point in line)
                {
                    g.FillEllipse(dotBrush, point.X - 4, point.Y - 4, 8, 8);
                    g.DrawEllipse(Pens.White, point.X - 4, point.Y - 4, 8, 8);
                }
            }
        }

        private void DrawBarChart(Graphics g, Rectangle bounds, IList<StockItemRow> items)
        {
            PrepareGraphics(g);
            List<StockItemRow> rows = (items ?? new List<StockItemRow>()).Take(10).ToList();
            if (rows.Count == 0)
            {
                DrawEmpty(g, bounds);
                return;
            }

            decimal max = Math.Max(1, rows.Select(x => ParseDecimal(x.Quantity)).Max());
            Rectangle plot = Rectangle.FromLTRB(bounds.Left + 42, bounds.Top + 12, bounds.Right - 12, bounds.Bottom - 34);
            int slot = Math.Max(32, plot.Width / rows.Count);
            int barWidth = Math.Max(18, Math.Min(34, slot - 18));
            using (SolidBrush barBrush = new SolidBrush(AccentBlue))
            using (SolidBrush textBrush = new SolidBrush(MutedBlue))
            using (Pen gridPen = new Pen(Color.FromArgb(225, 235, 246)))
            using (Font font = new Font("Segoe UI", 6.8F))
            using (Font valueFont = new Font("Segoe UI Semibold", 7F, FontStyle.Bold))
            {
                for (int i = 0; i <= 4; i++)
                {
                    int y = plot.Bottom - (plot.Height * i / 4);
                    g.DrawLine(gridPen, plot.Left, y, plot.Right, y);
                    g.DrawString((max * i / 4M).ToString("0", _culture), font, textBrush, bounds.Left + 5, y - 7);
                }

                for (int i = 0; i < rows.Count; i++)
                {
                    int h = (int)((float)(ParseDecimal(rows[i].Quantity) / max) * plot.Height);
                    int x = plot.Left + i * slot + (slot - barWidth) / 2;
                    g.FillRectangle(barBrush, new Rectangle(x, plot.Bottom - h, barWidth, h));
                    string text = ShortLabel(rows[i].ItemName, 9);
                    SizeF size = g.MeasureString(text, font);
                    string valueText = ParseDecimal(rows[i].Quantity).ToString("0", _culture);
                    SizeF valueSize = g.MeasureString(valueText, valueFont);
                    g.DrawString(valueText, valueFont, textBrush, x + (barWidth / 2F) - (valueSize.Width / 2F), plot.Bottom - h - 16);
                    g.DrawString(text, font, textBrush, x + (barWidth / 2F) - (size.Width / 2F), plot.Bottom + 8);
                }
            }
        }

        private void DrawDonut(Graphics g, Rectangle bounds, IList<StockCategoryMetric> items)
        {
            PrepareGraphics(g);
            List<StockCategoryMetric> rows = (items ?? new List<StockCategoryMetric>()).Where(x => x.Value > 0).ToList();
            if (rows.Count == 0)
            {
                DrawEmpty(g, bounds);
                return;
            }

            Color[] colors = { AccentBlue, AccentGreen, AccentPurple, AccentOrange, Color.FromArgb(31, 163, 181), AccentPink };
            decimal total = rows.Sum(x => x.Value);
            int pieSize = Math.Min(Math.Max(52, bounds.Height - 34), Math.Max(52, bounds.Width - 24));
            Rectangle pie = new Rectangle(bounds.Left + (bounds.Width - pieSize) / 2, bounds.Top + (bounds.Height - pieSize) / 2, pieSize, pieSize);
            float start = -90;
            using (SolidBrush holeBrush = new SolidBrush(CardBackColor))
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    float sweep = (float)(rows[i].Value / total) * 360F;
                    using (SolidBrush brush = new SolidBrush(colors[i % colors.Length]))
                        g.FillPie(brush, pie, start, sweep);
                    start += sweep;
                }
                g.FillEllipse(holeBrush, Rectangle.Inflate(pie, -pie.Width / 4, -pie.Height / 4));
            }
        }

        private void LblSummary_Paint(object sender, PaintEventArgs e)
        {
            PrepareGraphics(e.Graphics);
            Rectangle bounds = lblSummary.ClientRectangle;
            int left = bounds.Left + 6;
            int right = bounds.Right - 6;
            int y = bounds.Top + 4;
            string[] captions =
            {
                "Total Stock Value",
                "Total Stock Quantity",
                "Average Item Value",
                "Stock Turnover Rate",
                "Stock Accuracy"
            };
            string[] values =
            {
                Money(_analytics.TotalStockValue),
                _analytics.StockQuantity.ToString("N2", _culture) + " Units",
                Money(_analytics.AverageItemValue),
                _analytics.StockTurnoverRate.ToString("0.##", _culture) + "x",
                _analytics.StockAccuracyPercent.ToString("0.##", _culture) + "%"
            };

            using (Font font = new Font("Segoe UI", 8.4F))
            using (Font valueFont = new Font("Segoe UI Semibold", 8.4F, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(TextBlue))
            using (SolidBrush greenBrush = new SolidBrush(Color.FromArgb(46, 162, 68)))
            using (StringFormat rightAlign = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center })
            {
                for (int i = 0; i < captions.Length; i++)
                {
                    Rectangle icon = new Rectangle(left, y + 2, 13, 13);
                    DrawSummaryIcon(e.Graphics, icon, i);
                    e.Graphics.DrawString(captions[i], font, textBrush, left + 20, y);
                    Brush valueBrush = i >= 3 ? greenBrush : textBrush;
                    e.Graphics.DrawString(values[i], valueFont, valueBrush, new RectangleF(left + 138, y - 1, right - left - 138, 18), rightAlign);
                    y += 22;
                }
            }
        }

        private void DrawSummaryIcon(Graphics g, Rectangle bounds, int index)
        {
            using (Pen pen = new Pen(TextBlue, 1.35F))
            {
                switch (index)
                {
                    case 0:
                        g.DrawRectangle(pen, bounds.Left + 2, bounds.Top + 5, 9, 6);
                        g.DrawArc(pen, bounds.Left + 4, bounds.Top + 1, 5, 7, 180, 180);
                        g.DrawLine(pen, bounds.Left + 4, bounds.Top + 8, bounds.Right - 4, bounds.Top + 8);
                        break;
                    case 1:
                        g.DrawRectangle(pen, bounds.Left + 2, bounds.Top + 4, 9, 7);
                        g.DrawLine(pen, bounds.Left + 2, bounds.Top + 6, bounds.Left + 6, bounds.Top + 2);
                        g.DrawLine(pen, bounds.Left + 11, bounds.Top + 6, bounds.Left + 7, bounds.Top + 2);
                        g.DrawLine(pen, bounds.Left + 6, bounds.Top + 2, bounds.Left + 7, bounds.Top + 2);
                        break;
                    case 2:
                        g.DrawEllipse(pen, bounds.Left + 1, bounds.Top + 1, 11, 11);
                        g.DrawString("₹", new Font("Segoe UI", 6.5F, FontStyle.Bold), new SolidBrush(TextBlue), bounds.Left + 3, bounds.Top + 1);
                        break;
                    case 3:
                        g.DrawArc(pen, bounds.Left + 1, bounds.Top + 2, 10, 9, 35, 260);
                        g.DrawLine(pen, bounds.Right - 3, bounds.Top + 3, bounds.Right - 1, bounds.Top + 7);
                        g.DrawLine(pen, bounds.Right - 3, bounds.Top + 3, bounds.Right - 7, bounds.Top + 4);
                        break;
                    default:
                        g.DrawEllipse(pen, bounds.Left + 1, bounds.Top + 1, 11, 11);
                        g.FillEllipse(new SolidBrush(TextBlue), bounds.Left + 5, bounds.Top + 5, 3, 3);
                        break;
                }
            }
        }

        private void UpdateSummaryCanvasSize()
        {
            if (lblSummary == null)
                return;

            lblSummary.Height = Math.Max(118, 5 * 22 + 10);
            lblSummary.Width = Math.Max(230, lblSummary.Parent == null ? lblSummary.Width : lblSummary.Parent.ClientSize.Width - 4);
            lblSummary.Invalidate();
        }

        private void MovementTile_Paint(object sender, PaintEventArgs e)
        {
            Label label = sender as Label;
            MovementTileInfo info = label != null ? label.Tag as MovementTileInfo : null;
            if (label == null || info == null)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            Rectangle bounds = new Rectangle(1, 1, label.Width - 3, label.Height - 3);
            using (GraphicsPath path = RoundedRect(bounds, 7))
            using (SolidBrush backBrush = new SolidBrush(info.BackColor))
            using (Pen borderPen = new Pen(Color.FromArgb(130, info.Accent)))
            {
                e.Graphics.FillPath(backBrush, path);
                e.Graphics.DrawPath(borderPen, path);
            }

            int centerX = bounds.Left + bounds.Width / 2;
            bool compact = bounds.Height < 100;
            int iconSize = compact ? 22 : 28;
            int iconTop = compact ? bounds.Top + 8 : bounds.Top + 11;
            int titleTop = compact ? bounds.Top + 34 : bounds.Top + 41;
            int countTop = compact ? bounds.Top + 50 : bounds.Top + 61;
            int footerTop = compact ? bounds.Top + 72 : bounds.Top + 84;
            float titleSize = compact ? 7.2F : 7.6F;
            float countSize = compact ? 9.6F : 11F;
            float footerSize = compact ? 6.6F : 6.9F;
            DrawMovementIcon(e.Graphics, new Rectangle(centerX - iconSize / 2, iconTop, iconSize, iconSize), info);

            using (StringFormat center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (SolidBrush accentBrush = new SolidBrush(info.Accent))
            using (SolidBrush textBrush = new SolidBrush(TextBlue))
            using (Font titleFont = new Font("Segoe UI Semibold", titleSize, FontStyle.Bold))
            using (Font countFont = new Font("Segoe UI Semibold", countSize, FontStyle.Bold))
            using (Font footerFont = new Font("Segoe UI Semibold", footerSize, FontStyle.Bold))
            {
                e.Graphics.DrawString(info.Title, titleFont, accentBrush, new RectangleF(bounds.Left + 3, titleTop, bounds.Width - 6, 15), center);
                e.Graphics.DrawString(info.ItemCount.ToString("N0", _culture) + " Items", countFont, accentBrush, new RectangleF(bounds.Left + 3, countTop, bounds.Width - 6, 21), center);
                e.Graphics.DrawString(info.Footer, footerFont, textBrush, new RectangleF(bounds.Left + 3, footerTop, bounds.Width - 6, 13), center);
            }
        }

        private void DrawMovementIcon(Graphics g, Rectangle bounds, MovementTileInfo info)
        {
            using (Pen pen = new Pen(info.Accent, 2.2F))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                if (info.Kind == MovementTileKind.Fast)
                {
                    PointF[] points =
                    {
                        new PointF(bounds.Left + 2, bounds.Bottom - 4),
                        new PointF(bounds.Left + 9, bounds.Top + 14),
                        new PointF(bounds.Left + 15, bounds.Top + 17),
                        new PointF(bounds.Right - 3, bounds.Top + 4)
                    };
                    g.DrawLines(pen, points);
                    g.DrawLine(pen, bounds.Right - 10, bounds.Top + 4, bounds.Right - 3, bounds.Top + 4);
                    g.DrawLine(pen, bounds.Right - 3, bounds.Top + 4, bounds.Right - 3, bounds.Top + 11);
                }
                else if (info.Kind == MovementTileKind.Slow)
                {
                    g.DrawArc(pen, bounds.Left + 4, bounds.Top + 4, 17, 17, -45, 300);
                    g.DrawLine(pen, bounds.Right - 8, bounds.Top + 11, bounds.Right - 3, bounds.Top + 8);
                    g.DrawLine(pen, bounds.Right - 8, bounds.Top + 11, bounds.Right - 4, bounds.Top + 15);
                    g.DrawLine(pen, bounds.Left + 12, bounds.Top + 2, bounds.Left + 12, bounds.Top + 9);
                    g.DrawLine(pen, bounds.Left + 12, bounds.Top + 9, bounds.Left + 17, bounds.Top + 12);
                }
                else
                {
                    g.DrawLine(pen, bounds.Left + 4, bounds.Top + 5, bounds.Left + 4, bounds.Top + 18);
                    g.DrawLine(pen, bounds.Left + 4, bounds.Top + 18, bounds.Right - 3, bounds.Top + 18);
                    g.DrawLine(pen, bounds.Left + 9, bounds.Top + 8, bounds.Left + 15, bounds.Top + 15);
                    g.DrawLine(pen, bounds.Left + 15, bounds.Top + 15, bounds.Right - 8, bounds.Top + 10);
                    g.DrawLine(pen, bounds.Right - 8, bounds.Top + 10, bounds.Right - 3, bounds.Top + 16);
                    g.DrawLine(pen, bounds.Right - 3, bounds.Top + 16, bounds.Right - 3, bounds.Top + 9);
                    g.DrawLine(pen, bounds.Right - 3, bounds.Top + 16, bounds.Right - 10, bounds.Top + 16);
                }
            }
        }

        private void BindCategoryLegend()
        {
            if (categoryLegendPanel == null)
                return;

            categoryLegendPanel.SuspendLayout();
            categoryLegendPanel.Controls.Clear();
            categoryLegendPanel.AutoScroll = true;
            Color[] colors = { AccentBlue, AccentGreen, AccentPurple, AccentOrange, Color.FromArgb(31, 163, 181), AccentPink };
            decimal total = Math.Max(1, _analytics.CategoryDistribution.Sum(x => x.Value));
            int y = 4;
            for (int i = 0; i < _analytics.CategoryDistribution.Count; i++)
            {
                StockCategoryMetric item = _analytics.CategoryDistribution[i];
                Label colorDot = new Label
                {
                    BackColor = colors[i % colors.Length],
                    Location = new Point(2, y + 5),
                    Size = new Size(9, 9)
                };
                Label label = new Label
                {
                    AutoSize = false,
                    Font = new Font("Segoe UI", 7.2F),
                    ForeColor = TextBlue,
                    Location = new Point(17, y),
                    Size = new Size(Math.Max(120, categoryLegendPanel.ClientSize.Width - 24), 29),
                    Text = ShortLabel(item.Name, 22) + Environment.NewLine + Money(item.Value) + " (" + (item.Value / total * 100M).ToString("0.#", _culture) + "%)"
                };
                categoryLegendPanel.Controls.Add(colorDot);
                categoryLegendPanel.Controls.Add(label);
                y += 31;
            }
            categoryLegendPanel.ResumeLayout();
        }

        private void ShowGridPopup<T>(string popupTitle, IEnumerable<T> rows)
        {
            Form popup = new Form
            {
                Text = popupTitle,
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(760, 420),
                MinimizeBox = false,
                MaximizeBox = false,
                ShowIcon = false,
                BackColor = PageBackColor,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Padding = new Padding(14)
            };

            Panel card = new Panel
            {
                BackColor = CardBackColor,
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 10, 12, 12)
            };
            card.Paint += Card_Paint;

            Label title = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                Text = popupTitle,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = TextBlue,
                Padding = new Padding(3, 2, 0, 0)
            };

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

            DataGridView detailGrid = new DataGridView
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
                DataSource = new BindingList<T>((rows ?? Enumerable.Empty<T>()).ToList())
            };
            detailGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(232, 241, 252);
            detailGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            detailGrid.ColumnHeadersDefaultCellStyle.ForeColor = TextBlue;
            detailGrid.ColumnHeadersHeight = 34;
            detailGrid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            detailGrid.DefaultCellStyle.ForeColor = Color.FromArgb(36, 64, 105);
            detailGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 232, 250);
            detailGrid.DefaultCellStyle.SelectionForeColor = TextBlue;
            detailGrid.RowTemplate.Height = 30;
            FormatGrid(detailGrid);

            card.Controls.Add(detailGrid);
            card.Controls.Add(title);
            card.Controls.Add(close);
            close.BringToFront();
            popup.Controls.Add(card);
            popup.ShowDialog(this);
        }

        private void ShowStockTransactionGridPopup(string popupTitle, DataTable rows)
        {
            Form popup = new Form
            {
                Text = popupTitle,
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(960, 560),
                MinimizeBox = false,
                MaximizeBox = false,
                ShowIcon = false,
                BackColor = PageBackColor,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Padding = new Padding(14)
            };

            Panel card = new Panel
            {
                BackColor = CardBackColor,
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 10, 12, 12)
            };
            card.Paint += Card_Paint;

            Label title = new Label
            {
                Dock = DockStyle.Top,
                Height = 32,
                Text = popupTitle,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = TextBlue,
                Padding = new Padding(3, 3, 0, 0)
            };

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

            DataGridView detailGrid = CreateStockTransactionGrid(rows ?? new DataTable());
            card.Controls.Add(detailGrid);
            card.Controls.Add(title);
            card.Controls.Add(close);
            close.BringToFront();
            popup.Controls.Add(card);
            popup.ShowDialog(this);
        }

        private DataGridView CreateStockTransactionGrid(DataTable rows)
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

        private void ShowChartPopup<T>(string popupTitle, IEnumerable<T> rows, Action<Graphics, Rectangle> drawChart)
        {
            Form popup = new Form
            {
                Text = popupTitle,
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(900, 560),
                MinimizeBox = false,
                MaximizeBox = false,
                ShowIcon = false,
                BackColor = PageBackColor,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Padding = new Padding(14)
            };

            Panel card = new Panel
            {
                BackColor = CardBackColor,
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 10, 12, 12)
            };
            card.Paint += Card_Paint;

            Label title = new Label
            {
                Dock = DockStyle.Top,
                Height = 32,
                Text = popupTitle,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = TextBlue,
                Padding = new Padding(3, 3, 0, 0)
            };

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

            TableLayoutPanel content = new TableLayoutPanel
            {
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                RowCount = 2,
                Padding = new Padding(0, 6, 0, 0)
            };
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 62F));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 38F));

            Panel chartPanel = new Panel
            {
                BackColor = CardBackColor,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 10)
            };
            chartPanel.Paint += (s, e) =>
            {
                Rectangle chartBounds = new Rectangle(4, 4, Math.Max(1, chartPanel.ClientSize.Width - 8), Math.Max(1, chartPanel.ClientSize.Height - 8));
                drawChart(e.Graphics, chartBounds);
            };

            DataGridView detailGrid = new DataGridView
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
                DataSource = new BindingList<T>((rows ?? Enumerable.Empty<T>()).ToList())
            };
            detailGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(232, 241, 252);
            detailGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            detailGrid.ColumnHeadersDefaultCellStyle.ForeColor = TextBlue;
            detailGrid.ColumnHeadersHeight = 32;
            detailGrid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            detailGrid.DefaultCellStyle.ForeColor = Color.FromArgb(36, 64, 105);
            detailGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 232, 250);
            detailGrid.DefaultCellStyle.SelectionForeColor = TextBlue;
            detailGrid.RowTemplate.Height = 28;
            FormatGrid(detailGrid);

            content.Controls.Add(chartPanel, 0, 0);
            content.Controls.Add(detailGrid, 0, 1);
            card.Controls.Add(content);
            card.Controls.Add(title);
            card.Controls.Add(close);
            close.BringToFront();
            popup.Controls.Add(card);
            popup.ShowDialog(this);
        }

        private void ShowTrendPopup()
        {
            Form popup = new Form
            {
                Text = "Stock Trend (Value)",
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(940, 590),
                MinimizeBox = false,
                MaximizeBox = false,
                ShowIcon = false,
                BackColor = PageBackColor,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Padding = new Padding(14)
            };

            Panel card = new Panel
            {
                BackColor = CardBackColor,
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 10, 12, 12)
            };
            card.Paint += Card_Paint;

            Label title = new Label
            {
                Dock = DockStyle.Top,
                Height = 32,
                Text = BuildTrendPopupTitle(GetSelectedAnalysisModeCaption(), GetSelectedQuickDateCaption()),
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = TextBlue,
                Padding = new Padding(3, 3, 0, 0)
            };

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

            TableLayoutPanel content = new TableLayoutPanel
            {
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                RowCount = 3,
                Padding = new Padding(0, 8, 0, 0)
            };
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));

            FlowLayoutPanel filters = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(2, 6, 0, 4),
                WrapContents = false
            };

            Label modeLabel = CreatePopupFilterLabel("Mode");
            ComboBox modeCombo = CreatePopupCombo(108);
            modeCombo.Items.AddRange(new object[] { "Daily", "Weekly", "Monthly", "Yearly" });
            modeCombo.SelectedItem = GetSelectedAnalysisModeCaption();

            Label quickLabel = CreatePopupFilterLabel("Quick Date");
            ComboBox quickDateCombo = CreatePopupCombo(150);
            quickDateCombo.Items.AddRange(new object[] { "Today", "Yesterday", "This Month", "Previous Month", "This Year", "Previous Year" });
            quickDateCombo.SelectedItem = GetSelectedQuickDateCaption();

            filters.Controls.Add(modeLabel);
            filters.Controls.Add(modeCombo);
            filters.Controls.Add(quickLabel);
            filters.Controls.Add(quickDateCombo);

            Panel chartPanel = new Panel
            {
                BackColor = CardBackColor,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 10)
            };
            chartPanel.Paint += (s, e) =>
            {
                Rectangle chartBounds = new Rectangle(4, 4, Math.Max(1, chartPanel.ClientSize.Width - 8), Math.Max(1, chartPanel.ClientSize.Height - 8));
                DrawLineChart(e.Graphics, chartBounds, _analytics.Trend);
            };

            DataGridView detailGrid = new DataGridView
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
                DataSource = new BindingList<TrendDetailRow>(BuildTrendDetails())
            };
            detailGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(232, 241, 252);
            detailGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            detailGrid.ColumnHeadersDefaultCellStyle.ForeColor = TextBlue;
            detailGrid.ColumnHeadersHeight = 32;
            detailGrid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            detailGrid.DefaultCellStyle.ForeColor = Color.FromArgb(36, 64, 105);
            detailGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 232, 250);
            detailGrid.DefaultCellStyle.SelectionForeColor = TextBlue;
            detailGrid.RowTemplate.Height = 28;
            FormatGrid(detailGrid);

            Action refreshPopup = () =>
            {
                string mode = Convert.ToString(modeCombo.SelectedItem);
                _analytics.Trend = BuildTrend(_analytics.TotalStockValue, mode);
                title.Text = BuildTrendPopupTitle(mode, Convert.ToString(quickDateCombo.SelectedItem));
                detailGrid.DataSource = new BindingList<TrendDetailRow>(BuildTrendDetails());
                FormatGrid(detailGrid);
                chartPanel.Invalidate();
            };

            modeCombo.SelectedIndexChanged += (s, e) =>
            {
                if (cmbAnalysisMode != null && modeCombo.SelectedItem != null && cmbAnalysisMode.Items.Contains(modeCombo.SelectedItem))
                    cmbAnalysisMode.SelectedItem = modeCombo.SelectedItem;
                refreshPopup();
            };
            quickDateCombo.SelectedIndexChanged += (s, e) =>
            {
                DateRange range = GetQuickDateRange(Convert.ToString(quickDateCombo.SelectedItem));
                _fromDate = range.FromDate;
                _toDate = range.ToDate;
                if (dtFrom != null) dtFrom.Value = _fromDate;
                if (dtTo != null) dtTo.Value = _toDate;
                LoadAnalytics();
                refreshPopup();
            };

            content.Controls.Add(filters, 0, 0);
            content.Controls.Add(chartPanel, 0, 1);
            content.Controls.Add(detailGrid, 0, 2);
            card.Controls.Add(content);
            card.Controls.Add(title);
            card.Controls.Add(close);
            close.BringToFront();
            popup.Controls.Add(card);
            popup.ShowDialog(this);
        }

        private void ShowItemGraphPopup()
        {
            Form popup = new Form
            {
                Text = "Item Stock (Top 10 by Quantity)",
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(940, 590),
                MinimizeBox = false,
                MaximizeBox = false,
                ShowIcon = false,
                BackColor = PageBackColor,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Padding = new Padding(14)
            };

            Panel card = new Panel
            {
                BackColor = CardBackColor,
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 10, 12, 12)
            };
            card.Paint += Card_Paint;

            Label title = new Label
            {
                Dock = DockStyle.Top,
                Height = 32,
                Text = BuildItemGraphPopupTitle("Daily", GetSelectedQuickDateCaption()),
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = TextBlue,
                Padding = new Padding(3, 3, 0, 0)
            };

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

            TableLayoutPanel content = new TableLayoutPanel
            {
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                RowCount = 3,
                Padding = new Padding(0, 8, 0, 0)
            };
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));

            FlowLayoutPanel filters = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(2, 6, 0, 4),
                WrapContents = false
            };

            Label modeLabel = CreatePopupFilterLabel("Mode");
            ComboBox modeCombo = CreatePopupCombo(108);
            modeCombo.Items.AddRange(new object[] { "Daily", "Weekly", "Monthly", "Yearly" });
            modeCombo.SelectedItem = "Daily";

            Label quickLabel = CreatePopupFilterLabel("Quick Date");
            ComboBox quickDateCombo = CreatePopupCombo(150);
            quickDateCombo.Items.AddRange(new object[] { "Today", "Yesterday", "This Month", "Previous Month", "This Year", "Previous Year" });
            quickDateCombo.SelectedItem = GetSelectedQuickDateCaption();

            filters.Controls.Add(modeLabel);
            filters.Controls.Add(modeCombo);
            filters.Controls.Add(quickLabel);
            filters.Controls.Add(quickDateCombo);

            Panel chartPanel = new Panel
            {
                BackColor = CardBackColor,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 10)
            };
            chartPanel.Paint += (s, e) =>
            {
                Rectangle chartBounds = new Rectangle(4, 4, Math.Max(1, chartPanel.ClientSize.Width - 8), Math.Max(1, chartPanel.ClientSize.Height - 8));
                DrawBarChart(e.Graphics, chartBounds, _analytics.TopItems);
            };

            DataGridView detailGrid = new DataGridView
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
                DataSource = new BindingList<StockItemRow>(_analytics.TopItems.ToList())
            };
            detailGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(232, 241, 252);
            detailGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            detailGrid.ColumnHeadersDefaultCellStyle.ForeColor = TextBlue;
            detailGrid.ColumnHeadersHeight = 32;
            detailGrid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            detailGrid.DefaultCellStyle.ForeColor = Color.FromArgb(36, 64, 105);
            detailGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 232, 250);
            detailGrid.DefaultCellStyle.SelectionForeColor = TextBlue;
            detailGrid.RowTemplate.Height = 28;
            FormatGrid(detailGrid);

            Action refreshPopup = () =>
            {
                title.Text = BuildItemGraphPopupTitle(Convert.ToString(modeCombo.SelectedItem), Convert.ToString(quickDateCombo.SelectedItem));
                detailGrid.DataSource = new BindingList<StockItemRow>(_analytics.TopItems.ToList());
                FormatGrid(detailGrid);
                chartPanel.Invalidate();
            };

            modeCombo.SelectedIndexChanged += (s, e) => refreshPopup();
            quickDateCombo.SelectedIndexChanged += (s, e) =>
            {
                DateRange range = GetQuickDateRange(Convert.ToString(quickDateCombo.SelectedItem));
                _fromDate = range.FromDate;
                _toDate = range.ToDate;
                if (dtFrom != null) dtFrom.Value = _fromDate;
                if (dtTo != null) dtTo.Value = _toDate;
                LoadAnalytics();
                refreshPopup();
            };

            content.Controls.Add(filters, 0, 0);
            content.Controls.Add(chartPanel, 0, 1);
            content.Controls.Add(detailGrid, 0, 2);
            card.Controls.Add(content);
            card.Controls.Add(title);
            card.Controls.Add(close);
            close.BringToFront();
            popup.Controls.Add(card);
            popup.ShowDialog(this);
        }

        private Label CreatePopupFilterLabel(string text)
        {
            return new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                ForeColor = MutedBlue,
                Margin = new Padding(0, 5, 7, 0),
                Size = new Size(text.Length > 6 ? 70 : 42, 24),
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private ComboBox CreatePopupCombo(int width)
        {
            return new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8.75F),
                Margin = new Padding(0, 2, 18, 0),
                Size = new Size(width, 23)
            };
        }

        private void PrepareGraphics(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(CardBackColor);
        }

        private void DrawEmpty(Graphics g, Rectangle bounds)
        {
            using (SolidBrush brush = new SolidBrush(MutedBlue))
            using (Font font = new Font("Segoe UI", 8F))
            using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString("No stock data for selected range", font, brush, bounds, format);
        }

        private void Card_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;
            if (panel == null)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = RoundedRect(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), 7))
            using (SolidBrush brush = new SolidBrush(panel.BackColor))
            using (Pen pen = new Pen(CardBorderColor))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
            }
        }

        private void MetricIcon_Paint(object sender, PaintEventArgs e)
        {
            Label label = sender as Label;
            Color color = label != null && label.Tag is Color ? (Color)label.Tag : AccentBlue;
            Rectangle bounds = new Rectangle(1, 1, label.Width - 3, label.Height - 3);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (SolidBrush brush = new SolidBrush(color))
                e.Graphics.FillEllipse(brush, bounds);
            using (Pen pen = new Pen(Color.White, 2.1F))
            {
                float cx = bounds.Left + bounds.Width / 2F;
                float cy = bounds.Top + bounds.Height / 2F;
                PointF[] box =
                {
                    new PointF(cx, cy - 13), new PointF(cx + 13, cy - 6),
                    new PointF(cx + 13, cy + 8), new PointF(cx, cy + 15),
                    new PointF(cx - 13, cy + 8), new PointF(cx - 13, cy - 6)
                };
                e.Graphics.DrawPolygon(pen, box);
                e.Graphics.DrawLine(pen, cx - 13, cy - 6, cx, cy + 1);
                e.Graphics.DrawLine(pen, cx + 13, cy - 6, cx, cy + 1);
                e.Graphics.DrawLine(pen, cx, cy + 1, cx, cy + 15);
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

        private List<StockReportItem> SampleStockItems()
        {
            return new List<StockReportItem>
            {
                SampleItem("Rice (5kg)", "Grocery", 450, 80, 100, 30, 70),
                SampleItem("Sugar (1kg)", "Grocery", 380, 70, 100, 25, 45),
                SampleItem("Sunflower Oil (1L)", "Grocery", 310, 103.23M, 60, 20, 35),
                SampleItem("Tea (250gm)", "Beverages", 280, 100, 55, 15, 28),
                SampleItem("Milk (1L)", "Dairy", 250, 100, 90, 30, 42),
                SampleItem("Wheat (5kg)", "Grocery", 210, 95, 65, 20, 22),
                SampleItem("Dal (1kg)", "Grocery", 180, 105, 55, 15, 16),
                SampleItem("Salt (1kg)", "Household", 150, 50, 40, 20, 8),
                SampleItem("Soap", "Household", 120, 35, 25, 15, 3),
                SampleItem("Detergent", "Household", 100, 65, 20, 15, 0),
                SampleItem("Noodles (70gm)", "Grocery", 0, 12, 0, 20, 20),
                SampleItem("Battery (AA)", "Electronics", 0, 30, 0, 10, 0),
                SampleItem("Pen (Blue)", "Stationery", 0, 8, 0, 25, 2),
                SampleItem("Notebook (200pg)", "Stationery", 0, 32, 0, 15, 0),
                SampleItem("Toothpaste (100gm)", "Personal Care", 0, 45, 0, 10, 0)
            };
        }

        private StockReportItem SampleItem(string name, string category, decimal closing, decimal cost, decimal purchase, decimal reorder, decimal sales)
        {
            return new StockReportItem
            {
                ItemName = name,
                CategoryName = category,
                OpeningStock = Math.Max(0, closing - purchase + sales),
                Purchase = purchase,
                Sales = sales,
                ClosingStock = closing,
                OrderedStock = reorder,
                Cost = cost
            };
        }

        private List<MovementStockRow> BuildMovementRows(IEnumerable<StockReportItem> items)
        {
            return (items ?? Enumerable.Empty<StockReportItem>()).Select((x, i) => new MovementStockRow
            {
                Rank = i + 1,
                ItemName = ShortLabel(x.ItemName, 40),
                Category = ShortLabel(x.CategoryName, 24),
                OpeningStock = x.OpeningStock.ToString("N2", _culture),
                SalesQty = x.Sales.ToString("N2", _culture),
                TotalIn = x.TotalIn.ToString("N2", _culture),
                TotalOut = x.TotalOut.ToString("N2", _culture),
                ClosingStock = x.ClosingStock.ToString("N2", _culture),
                StockValue = Money(x.StockValue)
            }).ToList();
        }

        private List<TrendDetailRow> BuildTrendDetails()
        {
            List<StockTrendPoint> trendPoints = _analytics.Trend ?? new List<StockTrendPoint>();
            List<StockItemRow> itemRows = (_analytics.TopItems ?? new List<StockItemRow>()).ToList();
            List<TrendDetailRow> rows = new List<TrendDetailRow>();

            if (trendPoints.Count == 0)
                return rows;

            if (itemRows.Count == 0)
            {
                for (int i = 0; i < trendPoints.Count; i++)
                {
                    rows.Add(new TrendDetailRow
                    {
                        Rank = i + 1,
                        Caption = trendPoints[i].Caption,
                        Value = Money(trendPoints[i].Value),
                        ItemName = "No item",
                        Category = string.Empty,
                        Quantity = "0.00",
                        Cost = Money(0),
                        StockValue = Money(0)
                    });
                }
                return rows;
            }

            int rank = 1;
            for (int i = 0; i < trendPoints.Count; i++)
            {
                foreach (StockItemRow item in itemRows)
                {
                    rows.Add(new TrendDetailRow
                    {
                        Rank = rank++,
                        Caption = trendPoints[i].Caption,
                        Value = Money(trendPoints[i].Value),
                        ItemName = item.ItemName,
                        Category = item.Category,
                        Quantity = item.Quantity,
                        Cost = item.Cost,
                        StockValue = item.Value
                    });
                }
            }

            return rows;
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

        private decimal ParseDecimal(string value)
        {
            decimal result;
            return decimal.TryParse(value, NumberStyles.Any, _culture, out result) ? result : 0;
        }

        private DateRange GetQuickDateRange(string selected)
        {
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

            return new DateRange(fromDate, toDate);
        }

        private string GetSelectedQuickDateCaption()
        {
            string selected = Convert.ToString(cmbQuickDate.Value ?? cmbQuickDate.Text);
            return string.IsNullOrWhiteSpace(selected) ? "Today" : selected;
        }

        private string GetSelectedAnalysisModeCaption()
        {
            string selected = Convert.ToString(cmbAnalysisMode.SelectedItem ?? cmbAnalysisMode.Text);
            return string.IsNullOrWhiteSpace(selected) ? "Daily" : selected;
        }

        private string BuildItemGraphPopupTitle(string mode, string quickDate)
        {
            string selectedMode = string.IsNullOrWhiteSpace(mode) ? "Daily" : mode;
            string selectedQuickDate = string.IsNullOrWhiteSpace(quickDate) ? "Today" : quickDate;
            return "Item Stock (Top 10 by Quantity) - " + selectedMode + " - " + selectedQuickDate;
        }

        private string BuildTrendPopupTitle(string mode, string quickDate)
        {
            string selectedMode = string.IsNullOrWhiteSpace(mode) ? "Daily" : mode;
            string selectedQuickDate = string.IsNullOrWhiteSpace(quickDate) ? "Today" : quickDate;
            return "Stock Trend (Value) - " + selectedMode + " - " + selectedQuickDate;
        }

        private int GetTrendPointCount(string mode)
        {
            if (string.Equals(mode, "Yearly", StringComparison.OrdinalIgnoreCase))
                return 5;
            if (string.Equals(mode, "Monthly", StringComparison.OrdinalIgnoreCase))
                return 6;
            if (string.Equals(mode, "Weekly", StringComparison.OrdinalIgnoreCase))
                return 8;
            return 7;
        }

        private DateTime GetTrendDate(int index, int pointCount, int dateSpan, string mode)
        {
            if (string.Equals(mode, "Yearly", StringComparison.OrdinalIgnoreCase))
                return _toDate.AddYears(index - pointCount + 1);
            if (string.Equals(mode, "Monthly", StringComparison.OrdinalIgnoreCase))
                return _toDate.AddMonths(index - pointCount + 1);
            if (string.Equals(mode, "Weekly", StringComparison.OrdinalIgnoreCase))
                return _toDate.AddDays((index - pointCount + 1) * 7);

            int offset = pointCount <= 1 ? 0 : (int)Math.Round(index * (dateSpan - 1) / (double)(pointCount - 1));
            return _fromDate.AddDays(offset);
        }

        private string FormatTrendCaption(DateTime date, string mode)
        {
            if (string.Equals(mode, "Yearly", StringComparison.OrdinalIgnoreCase))
                return date.ToString("yyyy", _culture);
            if (string.Equals(mode, "Monthly", StringComparison.OrdinalIgnoreCase))
                return date.ToString("MMM yy", _culture);
            if (string.Equals(mode, "Weekly", StringComparison.OrdinalIgnoreCase))
                return date.ToString("dd MMM", _culture);
            return date.ToString("dd MMM", _culture);
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

        private string Money(decimal value)
        {
            return "Rs " + value.ToString("N2", _culture);
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

        private string ShortLabel(string text, int max)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "Unknown";
            return text.Length <= max ? text : text.Substring(0, max - 1) + ".";
        }

        private sealed class StockAnalyticsOverview
        {
            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }
            public decimal TotalStockValue { get; set; }
            public decimal RangeStockValue { get; set; }
            public int TotalItems { get; set; }
            public decimal StockQuantity { get; set; }
            public int LowStockItems { get; set; }
            public int OutOfStockItems { get; set; }
            public decimal AverageItemValue { get; set; }
            public decimal StockTurnoverRate { get; set; }
            public decimal StockAccuracyPercent { get; set; }
            public decimal StockIn { get; set; }
            public decimal StockOut { get; set; }
            public int FastMovingItems { get; set; }
            public int SlowMovingItems { get; set; }
            public int DeadStockItems { get; set; }
            public List<StockTrendPoint> Trend { get; set; } = new List<StockTrendPoint>();
            public List<StockItemRow> AllStockItems { get; set; } = new List<StockItemRow>();
            public List<StockItemRow> TopItems { get; set; } = new List<StockItemRow>();
            public List<StockValueDetailRow> StockValueDetails { get; set; } = new List<StockValueDetailRow>();
            public List<LowStockRow> LowStock { get; set; } = new List<LowStockRow>();
            public List<OutStockRow> OutStock { get; set; } = new List<OutStockRow>();
            public List<MovementStockRow> FastMoving { get; set; } = new List<MovementStockRow>();
            public List<MovementStockRow> SlowMoving { get; set; } = new List<MovementStockRow>();
            public List<MovementStockRow> DeadStock { get; set; } = new List<MovementStockRow>();
            public List<StockCategoryMetric> CategoryDistribution { get; set; } = new List<StockCategoryMetric>();
        }

        private sealed class StockTrendPoint
        {
            public string Caption { get; set; }
            public decimal Value { get; set; }
        }

        private sealed class StockCategoryMetric
        {
            public string Name { get; set; }
            public decimal Value { get; set; }
            public decimal Quantity { get; set; }
        }

        private sealed class StockItemRow
        {
            public int Rank { get; set; }
            public string ItemName { get; set; }
            public string Category { get; set; }
            public string Quantity { get; set; }
            public string Cost { get; set; }
            public string Value { get; set; }
        }

        private sealed class StockValueDetailRow
        {
            public int Rank { get; set; }
            public string Movement { get; set; }
            public string ItemName { get; set; }
            public string Cost { get; set; }
            public string SellingPrice { get; set; }
            public string Qty { get; set; }
            public string StockValue { get; set; }
        }

        private sealed class TrendDetailRow
        {
            public int Rank { get; set; }
            public string Caption { get; set; }
            public string Value { get; set; }
            public string ItemName { get; set; }
            public string Category { get; set; }
            public string Quantity { get; set; }
            public string Cost { get; set; }
            public string StockValue { get; set; }
        }

        private sealed class LowStockRow
        {
            public int Rank { get; set; }
            public string ItemName { get; set; }
            public string CurrentStock { get; set; }
            public string ReorderLevel { get; set; }
            public string Status { get; set; }
        }

        private sealed class OutStockRow
        {
            public int Rank { get; set; }
            public string ItemName { get; set; }
            public string Category { get; set; }
            public string CurrentStock { get; set; }
            public string Status { get; set; }
        }

        private sealed class MovementStockRow
        {
            public int Rank { get; set; }
            public string ItemName { get; set; }
            public string Category { get; set; }
            public string OpeningStock { get; set; }
            public string SalesQty { get; set; }
            public string TotalIn { get; set; }
            public string TotalOut { get; set; }
            public string ClosingStock { get; set; }
            public string StockValue { get; set; }
        }

        private sealed class DateRange
        {
            public DateRange(DateTime fromDate, DateTime toDate)
            {
                FromDate = fromDate;
                ToDate = toDate;
            }

            public DateTime FromDate { get; private set; }
            public DateTime ToDate { get; private set; }
        }

        private enum MovementTileKind
        {
            Fast,
            Slow,
            Dead
        }

        private sealed class MovementTileInfo
        {
            public MovementTileInfo(string title, int itemCount, string footer, Color accent, Color backColor, MovementTileKind kind)
            {
                Title = title;
                ItemCount = itemCount;
                Footer = footer;
                Accent = accent;
                BackColor = backColor;
                Kind = kind;
            }

            public string Title { get; private set; }
            public int ItemCount { get; private set; }
            public string Footer { get; private set; }
            public Color Accent { get; private set; }
            public Color BackColor { get; private set; }
            public MovementTileKind Kind { get; private set; }
        }
    }
}
