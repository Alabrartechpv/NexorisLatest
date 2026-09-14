using ModelClass;
using ModelClass.Report;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Dashboard
{
    public partial class FrmExecutiveKpiDashboard : Form
    {
        private static readonly Color BgColor = Color.FromArgb(243, 247, 252);
        private static readonly Color CardBgColor = Color.White;
        private static readonly Color CardBorderColor = Color.FromArgb(215, 228, 242);
        private static readonly Color SectionHeaderColor = Color.FromArgb(15, 34, 64);
        private static readonly Color SectionSubColor = Color.FromArgb(85, 105, 135);

        private readonly ExecutiveKpiRepository _repository = new ExecutiveKpiRepository();
        private readonly Action<Form, string> _openFormInTab;
        private readonly CultureInfo _culture = new CultureInfo("en-IN");
        private ExecutiveKpiModel _model = new ExecutiveKpiModel();
        private BackgroundWorker _worker;

        public FrmExecutiveKpiDashboard() : this(null)
        {
        }

        public FrmExecutiveKpiDashboard(Action<Form, string> openFormInTab)
        {
            _openFormInTab = openFormInTab;
            InitializeComponent();
            InitializeBackgroundWorker();
            Load += FrmExecutiveKpiDashboard_Load;
            Resize += FrmExecutiveKpiDashboard_Resize;
        }

        private void InitializeBackgroundWorker()
        {
            _worker = new BackgroundWorker();
            _worker.DoWork += (s, e) =>
            {
                var args = (Tuple<DateTime, DateTime>)e.Argument;
                try
                {
                    e.Result = _repository.GetExecutiveKpiData(args.Item1, args.Item2);
                }
                catch (Exception ex)
                {
                    e.Result = ex;
                }
            };
            _worker.RunWorkerCompleted += (s, e) =>
            {
                btnApply.Enabled = true;
                btnRefresh.Enabled = true;
                Cursor = Cursors.Default;
                if (e.Error != null)
                {
                    MessageBox.Show("Error loading dashboard data: " + e.Error.Message, "Executive Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (e.Result is Exception ex)
                {
                    MessageBox.Show("Error loading dashboard data: " + ex.Message, "Executive Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (e.Result is ExecutiveKpiModel data)
                {
                    _model = data;
                }
                PopulateDashboardData();
            };
        }

        private void FrmExecutiveKpiDashboard_Load(object sender, EventArgs e)
        {
            InitPeriodCombo();
            SetDefaultDates();
            LoadDashboardData();
        }

        private void FrmExecutiveKpiDashboard_Resize(object sender, EventArgs e)
        {
            if (_model != null)
            {
                PopulateDashboardData();
            }
        }

        private void InitPeriodCombo()
        {
            comboPeriod.Items.Clear();
            comboPeriod.Items.Add("All", "All");
            comboPeriod.Items.Add("Today", "Today");
            comboPeriod.Items.Add("This Week", "This Week");
            comboPeriod.Items.Add("This Month", "This Month");
            comboPeriod.Items.Add("This Quarter", "This Quarter");
            comboPeriod.Items.Add("This Year", "This Year");
            comboPeriod.Items.Add("Custom", "Custom");
            comboPeriod.SelectedIndex = 0; // Default: All

            comboPeriod.ValueChanged += (s, e) =>
            {
                string sel = comboPeriod.Text;
                DateTime now = DateTime.Today;

                if (sel == "All")
                {
                    dtFrom.Enabled = false;
                    dtTo.Enabled = false;
                    dtFrom.Value = new DateTime(1990, 1, 1);
                    dtTo.Value = now;
                    return;
                }

                dtFrom.Enabled = true;
                dtTo.Enabled = true;

                switch (sel)
                {
                    case "Today":
                        dtFrom.Value = now;
                        dtTo.Value = now;
                        break;
                    case "This Week":
                        dtFrom.Value = now.AddDays(-(int)now.DayOfWeek);
                        dtTo.Value = now;
                        break;
                    case "This Month":
                        dtFrom.Value = new DateTime(now.Year, now.Month, 1);
                        dtTo.Value = now;
                        break;
                    case "This Quarter":
                        int qMonth = ((now.Month - 1) / 3) * 3 + 1;
                        dtFrom.Value = new DateTime(now.Year, qMonth, 1);
                        dtTo.Value = now;
                        break;
                    case "This Year":
                        dtFrom.Value = new DateTime(now.Year, 1, 1);
                        dtTo.Value = now;
                        break;
                }
            };

            btnApply.Click += (s, e) => LoadDashboardData();
            btnRefresh.Click += (s, e) => LoadDashboardData();
            btnPrint.Click += (s, e) => ExportExecutiveSummary();
        }

        private void SetDefaultDates()
        {
            DateTime now = DateTime.Today;
            dtFrom.Enabled = false;
            dtTo.Enabled = false;
            dtFrom.Value = new DateTime(1990, 1, 1);
            dtTo.Value = now;
        }

        private void LoadDashboardData()
        {
            btnApply.Enabled = false;
            btnRefresh.Enabled = false;
            Cursor = Cursors.WaitCursor;

            DateTime from;
            DateTime to;

            if (comboPeriod.Text == "All")
            {
                from = new DateTime(1990, 1, 1);
                to = DateTime.Today;
            }
            else
            {
                from = dtFrom.Value != null ? Convert.ToDateTime(dtFrom.Value).Date : new DateTime(1990, 1, 1);
                to = dtTo.Value != null ? Convert.ToDateTime(dtTo.Value).Date : DateTime.Today;
            }

            if (to < from)
            {
                DateTime swap = from;
                from = to;
                to = swap;
            }

            if (_worker != null && !_worker.IsBusy)
            {
                _worker.RunWorkerAsync(Tuple.Create(from, to));
            }
            else
            {
                try
                {
                    _model = _repository.GetExecutiveKpiData(from, to);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading dashboard data: " + ex.Message, "Executive Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                btnApply.Enabled = true;
                btnRefresh.Enabled = true;
                Cursor = Cursors.Default;
                PopulateDashboardData();
            }
        }

        private ToolTip _toolTip = new ToolTip
        {
            AutoPopDelay = 8000,
            InitialDelay = 300,
            ReshowDelay = 100,
            ShowAlways = true
        };

        private void PopulateDashboardData()
        {
            pnlScrollableContent.SuspendLayout();
            pnlScrollableContent.Controls.Clear();

            int leftMargin = 16;
            int containerWidth = Math.Max(960, pnlScrollableContent.ClientSize.Width - (leftMargin * 2));
            int currentY = 8;

            // ═══════════════════════════════════════════════════════════════════
            // ALL 30 METRICS IN EXACT NUMERICAL ORDER (1 to 30)
            // ═══════════════════════════════════════════════════════════════════
            var allCards = new List<Control>
            {
                // 1. Total Stock Value
                CreateKpiCard("1. Total Stock Value", "Total warehouse stock valued at purchase cost", FormatCurr(_model.TotalStockCostValue), $"Items: {_model.TotalStockItemCount:N0} | Units: {_model.TotalStockQuantity:N0}", Color.FromArgb(41, 128, 185), () => DrillDown("StockValuation")),
                
                // 2. Stock Profit Potential
                CreateKpiCard("2. Stock Profit Potential", "Estimated gross profit on 100% stock liquidation", FormatCurr(_model.StockProfitPotential), $"Retail Value: {FormatCurr(_model.TotalStockRetailValue)}", Color.FromArgb(39, 174, 96), () => DrillDown("StockValuation")),
                
                // 3. Total Sales Revenue
                CreateKpiCard("3. Total Sales Revenue", "Total net billing revenue for the selected date period", FormatCurr(_model.TotalSalesRevenue), $"{_model.TotalSalesBillCount:N0} Bills | Pur: {FormatCurr(_model.TotalPurchases)}", Color.FromArgb(30, 136, 229), () => DrillDown("SalesAnalytics")),
                
                // 4. Total Business Expenses
                CreateKpiCard("4. Total Business Expenses", "Combined operational overheads, utilities, and running costs", FormatCurr(_model.TotalBusinessExpenses), $"Direct: {FormatCurr(_model.DirectExpenses)} | Indir: {FormatCurr(_model.IndirectExpenses)}", Color.FromArgb(192, 57, 43), () => DrillDown("ProfitLoss")),
                
                // 5. Negative Stock Impact
                CreateKpiCard("5. Negative Stock Impact", "Estimated financial distortion due to negative stock at cost", FormatCurr(_model.NegativeStockImpactValue), "Cost Value of Negative Items", Color.FromArgb(231, 76, 60), () => DrillDown("StockValuation")),
                
                // 6. Negative Stock Items
                CreateKpiCard("6. Negative Stock Items", "Products currently running in negative inventory balance", $"{_model.NegativeStockItemCount} Items", $"Total Negative Qty: {_model.NegativeStockTotalQty:N0}", Color.FromArgb(192, 57, 43), () => DrillDown("StockAnalytics")),
                
                // 7. Cash in Hand
                CreateKpiCard("7. Cash in Hand", "Total liquid cash in cash drawers, counter tills, and main safe", FormatCurr(_model.CashInHand), "Available Counter Cash", Color.FromArgb(46, 204, 113), () => DrillDown("CashBankBook")),
                
                // 8. Bank Balance
                CreateKpiCard("8. Bank Balance", "Total combined active bank account balances", FormatCurr(_model.BankBalance), "Active Commercial Bank Accounts", Color.FromArgb(52, 152, 219), () => DrillDown("BankStatement")),
                
                // 9. Supplier Payables
                CreateKpiCard("9. Supplier Payables", "Total outstanding liabilities owed to vendors and suppliers", FormatCurr(_model.SupplierPayables), $"{_model.SupplierPayablesCount} Pending Purchase Bills", Color.FromArgb(231, 76, 60), () => DrillDown("VendorOutstanding")),
                
                // 10. Customer Receivables
                CreateKpiCard("10. Customer Receivables", "Total outstanding dues to collect from customers", FormatCurr(_model.CustomerReceivables), $"{_model.CustomerReceivablesCount} Unpaid Sales Invoices", Color.FromArgb(241, 196, 15), () => DrillDown("CustomerOutstanding")),
                
                // 11. Actual Net Profit
                CreateKpiCard("11. Actual Net Profit", "Bottom-line net earnings after deducting cost of sales and expenses", FormatCurr(_model.ActualNetProfit), $"Operating Margin: {_model.OperatingProfitMarginPercent:N2}%", _model.ActualNetProfit >= 0 ? Color.FromArgb(39, 174, 96) : Color.FromArgb(192, 57, 43), () => DrillDown("ProfitLoss")),
                
                // 12. Owner Drawings
                CreateKpiCard("12. Owner Drawings", "Capital withdrawals and personal drawings taken by proprietors", FormatCurr(_model.OwnerDrawings), "Capital Account Withdrawals", Color.FromArgb(155, 89, 182), () => DrillDown("DayBook")),
                
                // 13. Net Business Asset (NBA)
                CreateKpiCard("13. Net Business Asset (NBA)", "True net worth of the business (Total Assets minus Liabilities)", FormatCurr(_model.NetBusinessAsset), $"Assets: {FormatCurr(_model.TotalAssets)} | Liab: {FormatCurr(_model.TotalLiabilities)}", Color.FromArgb(16, 85, 154), () => DrillDown("BalanceSheet")),
                
                // 14. Loss Stock (Damaged / Out)
                CreateKpiCard("14. Loss Stock (Damaged / Out)", "Total stock written off or damaged (Stock OUT adjustments)", FormatCurr(_model.LossStockValue), $"Discrepancy Qty: {_model.LossStockQty:N2}", Color.FromArgb(211, 84, 0), () => DrillDown("StockAdjustment")),
                
                // 15. Extra Stock (Found / In)
                CreateKpiCard("15. Extra Stock (Found / In)", "Total surplus stock found and added (Stock IN adjustments)", FormatCurr(_model.ExtraStockValue), $"Surplus Qty: {_model.ExtraStockQty:N2}", Color.FromArgb(22, 160, 133), () => DrillDown("StockAdjustment")),
                
                // 16. Net Stock Adjustment
                CreateKpiCard("16. Net Stock Adjustment", "Net variance between stock additions and write-offs", FormatCurr(_model.NetStockAdjustmentValue), _model.NetStockAdjustmentValue >= 0 ? "Surplus Net Balance" : "Shortage Net Variance", Color.FromArgb(142, 68, 173), () => DrillDown("StockAdjustment")),
                
                // 17. Delayed Customer Receivables (>30D)
                CreateKpiCard("17. Delayed Customer Receivables (>30D)", "Overdue customer receivables past 30 days credit period", FormatCurr(_model.DelayedCustomerReceivables30Days), $"{_model.DelayedCustomerCount30Days} High-Risk Invoices", Color.FromArgb(231, 76, 60), () => DrillDown("CustomerOutstanding")),
                
                // 18. Delayed Supplier Payables (>30D)
                CreateKpiCard("18. Delayed Supplier Payables (>30D)", "Overdue purchase invoices past 30 days credit period", FormatCurr(_model.DelayedSupplierPayables30Days), $"{_model.DelayedSupplierCount30Days} Overdue Vendor Bills", Color.FromArgb(230, 126, 34), () => DrillDown("VendorOutstanding")),
                
                // 19. Operating Profit Margin %
                CreateKpiCard("19. Operating Profit Margin %", "Ratio of net profit generated from sales turnover", $"{_model.OperatingProfitMarginPercent:N2}%", "Net Profit / Sales Revenue", Color.FromArgb(22, 160, 133), () => DrillDown("ProfitLoss")),
                
                // 20. Excess Stock Alert
                CreateKpiCard("20. Excess Stock Alert", "Products exceeding maximum inventory threshold", $"{_model.ExcessStockAlertCount} Items", "Over Maximum Limit", Color.FromArgb(243, 156, 18), () => DrillDown("SmartReorder")),
                
                // 21. Low Stock Alert
                CreateKpiCard("21. Low Stock Alert", "Products below minimum critical inventory threshold", $"{_model.LowStockAlertCount} Items", "Below Minimum Limit", Color.FromArgb(230, 126, 34), () => DrillDown("LowStockAlert")),
                
                // 22. Reorder Alert
                CreateKpiCard("22. Reorder Alert", "Products at or below reorder level requiring purchase", $"{_model.ReorderAlertCount} Items", "Purchase Order Recommended", Color.FromArgb(192, 57, 43), () => DrillDown("SmartReorder")),
                
                // 23. Customer Bad Debts
                CreateKpiCard("23. Customer Bad Debts", "Uncollectible customer credit balances written off", FormatCurr(_model.CustomerBadDebts), "Credit Loss Write-Offs", Color.FromArgb(149, 165, 166), () => DrillDown("CustomerOutstanding")),
                
                // 24. Supplier Write-Offs
                CreateKpiCard("24. Supplier Write-Offs", "Vendor invoice write-offs and negotiated settlement discounts", FormatCurr(_model.SupplierWriteOffs), "Discounts / Write-Off Settled", Color.FromArgb(127, 140, 141), () => DrillDown("VendorOutstanding")),
                
                // 26. Deletion & Cancellation Logs
                CreateKpiCard("26. Deletion & Cancellation Logs", "Track of deleted sales bills, purchase bills, and account vouchers", $"{_model.DeletionCount} Events", "Cancelled / Deleted Documents", Color.FromArgb(192, 57, 43), () => DrillDown("AuditDeletions")),
                
                // 27. Price Change Modifications
                CreateKpiCard("27. Price Change Modifications", "Track of item selling and cost price edits in Item Master", $"{_model.PriceChangeCount} Events", "Item Master Price Modifications", Color.FromArgb(41, 128, 185), () => DrillDown("AuditPriceChanges")),
                
                // 28. Physical Stock Discrepancy Logs
                CreateKpiCard("28. Physical Stock Discrepancy Logs", "Track of physical stock count reconciliations and adjustments", $"{_model.StockAdjustmentCount} Adjustments", "Stock Adjustment Entries", Color.FromArgb(142, 68, 173), () => DrillDown("StockAdjustment")),
                
                // 29. Supplier Advance / Overpayment
                CreateKpiCard("29. Supplier Advance / Overpayment", "Advance payments and debit balances with vendors", FormatCurr(_model.SupplierAdvanceBalance), "Advance Payments / Debit", Color.FromArgb(52, 73, 94), () => DrillDown("VendorPaymentReport")),
                
                // 30. Customer Advance / Deposits
                CreateKpiCard("30. Customer Advance / Deposits", "Customer advance payments and credit deposits received", FormatCurr(_model.CustomerAdvanceBalance), "Customer Deposits / Credit", Color.FromArgb(127, 140, 141), () => DrillDown("CustomerReceiptReport"))
            };

            // Display in clean full-width responsive grid:
            // Rows 1-4: 6 cards per row, Row 5: 5 cards evenly stretched
            int clientH = pnlScrollableContent.ClientSize.Height;
            int gap = 5;
            int overhead = 36;
            int usableH = Math.Max(380, clientH - overhead);
            int targetCardH = (int)Math.Floor(((usableH * 0.58) - (4 * gap)) / 5);
            int cardHeight = Math.Max(50, Math.Min(74, targetCardH));

            var cardsGrid = CreateCardGridPanel(allCards, containerWidth, cardHeight, gap);
            cardsGrid.Location = new Point(leftMargin, currentY);
            pnlScrollableContent.Controls.Add(cardsGrid);
            currentY += cardsGrid.Height + 6;

            // ═══════════════════════════════════════════════════════════════════
            // 25. MONTHLY GROWTH & PERFORMANCE MATRIX (MoM %)
            // ═══════════════════════════════════════════════════════════════════
            var headerGrowth = CreateSectionHeader("25. Monthly Growth & Performance Matrix (MoM %)", 0, containerWidth);
            headerGrowth.Location = new Point(leftMargin, currentY);
            pnlScrollableContent.Controls.Add(headerGrowth);
            currentY += headerGrowth.Height + 3;

            int tableHeight = Math.Max(80, clientH - currentY - 6);
            var dgvGrowth = CreateGrowthMatrixGrid(containerWidth, tableHeight);
            dgvGrowth.Location = new Point(leftMargin, currentY);
            pnlScrollableContent.Controls.Add(dgvGrowth);
            currentY += dgvGrowth.Height + 6;

            pnlScrollableContent.ResumeLayout(true);
        }

        private Panel CreateSectionHeader(string title, int kpiCount, int width)
        {
            var pnl = new Panel
            {
                Size = new Size(width, 18),
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = SectionHeaderColor,
                Location = new Point(0, 0),
                AutoSize = true
            };
            pnl.Controls.Add(lblTitle);

            pnl.Paint += (s, e) =>
            {
                int titleRight = lblTitle.Right + 8;
                if (titleRight < width)
                {
                    using (var pen = new Pen(Color.FromArgb(215, 228, 242), 1))
                    {
                        e.Graphics.DrawLine(pen, titleRight, 9, width - 4, 9);
                    }
                }
            };

            return pnl;
        }

        private Panel CreateCardGridPanel(List<Control> cards, int containerWidth, int cardHeight, int gap = 5)
        {
            int totalRows = 5;
            int totalHeight = totalRows * cardHeight + (totalRows - 1) * gap;

            var pnl = new Panel
            {
                Size = new Size(containerWidth, totalHeight),
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            // First 24 cards: 4 rows of 6 cards each
            int cardWidth6 = (containerWidth - (5 * gap)) / 6;
            for (int i = 0; i < Math.Min(24, cards.Count); i++)
            {
                int row = i / 6;
                int col = i % 6;
                int x = col * (cardWidth6 + gap);
                int y = row * (cardHeight + gap);

                cards[i].Location = new Point(x, y);
                cards[i].Size = new Size(cardWidth6, cardHeight);
                pnl.Controls.Add(cards[i]);
            }

            // Last 5 cards (cards 24 to 28): 1 row of 5 cards stretched across 100% width
            if (cards.Count > 24)
            {
                int cardWidth5 = (containerWidth - (4 * gap)) / 5;
                for (int i = 24; i < cards.Count; i++)
                {
                    int col = i - 24;
                    int x = col * (cardWidth5 + gap);
                    int y = 4 * (cardHeight + gap);

                    cards[i].Location = new Point(x, y);
                    cards[i].Size = new Size(cardWidth5, cardHeight);
                    pnl.Controls.Add(cards[i]);
                }
            }

            return pnl;
        }

        private Panel CreateKpiCard(string title, string description, string mainValue, string footerInfo, Color accentColor, Action onClick)
        {
            var card = new Panel
            {
                Size = new Size(200, 60),
                BackColor = CardBgColor,
                Cursor = Cursors.Hand,
                Margin = new Padding(0)
            };

            var topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 3,
                BackColor = accentColor
            };
            card.Controls.Add(topBar);

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 48, 80),
                Location = new Point(6, 4),
                Size = new Size(185, 14),
                AutoEllipsis = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Controls.Add(lblTitle);

            var lblValue = new Label
            {
                Text = mainValue,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new Point(5, 19),
                Size = new Size(185, 18),
                AutoEllipsis = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Controls.Add(lblValue);

            var lblFooter = new Label
            {
                Text = footerInfo,
                Font = new Font("Segoe UI", 7F),
                ForeColor = Color.FromArgb(100, 120, 145),
                Location = new Point(6, 38),
                Size = new Size(185, 13),
                AutoEllipsis = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Controls.Add(lblFooter);

            card.Resize += (s, e) =>
            {
                int w = card.Width;
                int h = card.Height;
                lblTitle.Size = new Size(Math.Max(10, w - 12), 14);

                int vTop = h >= 64 ? 21 : (h >= 56 ? 19 : 17);
                int vH = h >= 64 ? 22 : 18;
                lblValue.Location = new Point(5, vTop);
                lblValue.Size = new Size(Math.Max(10, w - 10), vH);
                lblValue.Font = new Font("Segoe UI", h >= 64 ? 12F : (h >= 56 ? 11F : 10F), FontStyle.Bold);

                int fTop = Math.Max(vTop + vH, h - 16);
                lblFooter.Location = new Point(6, fTop);
                lblFooter.Size = new Size(Math.Max(10, w - 12), 13);
            };

            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(CardBorderColor, 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            if (!string.IsNullOrEmpty(description))
            {
                string fullTip = $"{title}\n{description}\nValue: {mainValue}\n{footerInfo}";
                _toolTip.SetToolTip(card, fullTip);
                _toolTip.SetToolTip(lblTitle, fullTip);
                _toolTip.SetToolTip(lblValue, fullTip);
                _toolTip.SetToolTip(lblFooter, fullTip);
            }

            card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(242, 248, 255);
            card.MouseLeave += (s, e) => card.BackColor = CardBgColor;

            if (onClick != null)
            {
                void WireClick(Control ctrl)
                {
                    ctrl.Click += (s, e) => onClick();
                    ctrl.Cursor = Cursors.Hand;
                    foreach (Control child in ctrl.Controls)
                    {
                        WireClick(child);
                    }
                }
                WireClick(card);
            }

            return card;
        }

        private DataGridView CreateGrowthMatrixGrid(int width, int height)
        {
            var grid = new DataGridView
            {
                Width = width,
                Height = height,
                BackgroundColor = Color.White,
                GridColor = Color.FromArgb(225, 235, 245),
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                Margin = new Padding(0),
                ScrollBars = ScrollBars.Both
            };

            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(28, 70, 130);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold);
            grid.ColumnHeadersHeight = height >= 140 ? 25 : 22;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            grid.DefaultCellStyle.Font = new Font("Segoe UI", 7.5F);
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(20, 40, 70);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 235, 255);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(10, 30, 60);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 251, 255);
            grid.RowTemplate.Height = height >= 160 ? 24 : 20;

            grid.Columns.Add("Month", "Month Period");
            grid.Columns.Add("Sales", "Sales Turnover");
            grid.Columns.Add("SalesGrowth", "Sales MoM %");
            grid.Columns.Add("Purchases", "Total Purchases");
            grid.Columns.Add("Expenses", "Total Expenses");
            grid.Columns.Add("NetProfit", "Actual Net Profit");
            grid.Columns.Add("ProfitGrowth", "Profit MoM %");
            grid.Columns.Add("Status", "Performance");

            grid.Columns["Month"].FillWeight = 110;
            grid.Columns["Sales"].FillWeight = 120;
            grid.Columns["SalesGrowth"].FillWeight = 95;
            grid.Columns["Purchases"].FillWeight = 120;
            grid.Columns["Expenses"].FillWeight = 110;
            grid.Columns["NetProfit"].FillWeight = 120;
            grid.Columns["ProfitGrowth"].FillWeight = 95;
            grid.Columns["Status"].FillWeight = 90;

            grid.Columns["Sales"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grid.Columns["SalesGrowth"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.Columns["Purchases"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grid.Columns["Expenses"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grid.Columns["NetProfit"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grid.Columns["ProfitGrowth"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.Columns["Status"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            if (_model.MonthlyGrowthMatrix != null && _model.MonthlyGrowthMatrix.Count > 0)
            {
                foreach (var row in _model.MonthlyGrowthMatrix)
                {
                    string status = row.NetProfitAmount >= 0 ? "Profitable" : "Deficit";
                    int rIdx = grid.Rows.Add(
                        row.MonthName,
                        FormatCurr(row.SalesAmount),
                        (row.SalesGrowthPercent >= 0 ? "+" : "") + row.SalesGrowthPercent.ToString("N1") + "%",
                        FormatCurr(row.PurchaseAmount),
                        FormatCurr(row.ExpenseAmount),
                        FormatCurr(row.NetProfitAmount),
                        (row.NetProfitGrowthPercent >= 0 ? "+" : "") + row.NetProfitGrowthPercent.ToString("N1") + "%",
                        status
                    );

                    var sCell = grid.Rows[rIdx].Cells["SalesGrowth"];
                    sCell.Style.ForeColor = row.SalesGrowthPercent >= 0 ? Color.FromArgb(46, 125, 50) : Color.FromArgb(198, 40, 40);
                    sCell.Style.Font = new Font("Segoe UI Semibold", 7.5F, FontStyle.Bold);

                    var pCell = grid.Rows[rIdx].Cells["ProfitGrowth"];
                    pCell.Style.ForeColor = row.NetProfitGrowthPercent >= 0 ? Color.FromArgb(46, 125, 50) : Color.FromArgb(198, 40, 40);
                    pCell.Style.Font = new Font("Segoe UI Semibold", 7.5F, FontStyle.Bold);

                    var statusCell = grid.Rows[rIdx].Cells["Status"];
                    statusCell.Style.ForeColor = row.NetProfitAmount >= 0 ? Color.FromArgb(46, 125, 50) : Color.FromArgb(198, 40, 40);
                    statusCell.Style.Font = new Font("Segoe UI Semibold", 7.5F, FontStyle.Bold);
                }
            }

            return grid;
        }

        private void DrillDown(string target)
        {
            try
            {
                Form formToOpen = null;
                string title = "";

                switch (target)
                {
                    case "StockValuation":
                        formToOpen = new Reports.InventoryReport.frmStockValuationReport();
                        title = "Stock Valuation Report";
                        break;
                    case "StockAnalytics":
                        formToOpen = new FrmStockAnalytics();
                        title = "Stock Analytics";
                        break;
                    case "StockAdjustment":
                        formToOpen = new Reports.InventoryReport.frmStockAdjustmentReport();
                        title = "Stock Adjustment Report";
                        break;
                    case "SmartReorder":
                        formToOpen = new Reports.InventoryReport.FrmSmartReorderDashboard();
                        title = "Smart Reorder Dashboard";
                        break;
                    case "LowStockAlert":
                        formToOpen = new Reports.InventoryReport.frmLowStockAlertReport();
                        title = "Low Stock Alert Report";
                        break;
                    case "CashBankBook":
                        formToOpen = new Reports.FinancialReports.FrmCashBankBook();
                        title = "Cash & Bank Book";
                        break;
                    case "BankStatement":
                        formToOpen = new Reports.FinancialReports.FrmBankStatementReport();
                        title = "Bank Statement Report";
                        break;
                    case "VendorOutstanding":
                        formToOpen = new Reports.FinancialReports.frmVendorOutstandingReport();
                        title = "Vendor Outstanding Report";
                        break;
                    case "CustomerOutstanding":
                        formToOpen = new Reports.FinancialReports.frmCustomerOutstandingReport();
                        title = "Customer Outstanding Report";
                        break;
                    case "BalanceSheet":
                        formToOpen = new Reports.FinancialReports.FrmBalanceSheet();
                        title = "Balance Sheet";
                        break;
                    case "VendorPaymentReport":
                        formToOpen = new Reports.FinancialReports.frmVendorPaymentReport();
                        title = "Vendor Payment Report";
                        break;
                    case "CustomerReceiptReport":
                        formToOpen = new Reports.FinancialReports.frmCustomerReceiptReport();
                        title = "Customer Receipt Report";
                        break;
                    case "SalesAnalytics":
                        formToOpen = new FrmSalesAnalytics();
                        title = "Sales Analytics";
                        break;
                    case "ProfitLoss":
                        formToOpen = new Reports.FinancialReports.FrmProfitLossAccount();
                        title = "Profit & Loss Account";
                        break;
                    case "TradingPL":
                        formToOpen = new Reports.FinancialReports.FrmTradingPLAccount();
                        title = "Trading & P/L Account";
                        break;
                    case "DayBook":
                        formToOpen = new Reports.FinancialReports.FrmDayBook();
                        title = "Day Book";
                        break;
                    case "AuditDeletions":
                        formToOpen = new Settings.UserActivityLog();
                        title = "User Activity Log";
                        break;
                    case "AuditPriceChanges":
                        formToOpen = new Settings.ItemHistoryLog();
                        title = "Item History Log";
                        break;
                }

                if (formToOpen != null)
                {
                    if (_openFormInTab != null)
                    {
                        _openFormInTab(formToOpen, title);
                    }
                    else
                    {
                        formToOpen.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open drilldown report: {ex.Message}", "Drilldown Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ExportExecutiveSummary()
        {
            try
            {
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Excel Workbook (*.xls)|*.xls|PDF / Printable Report (*.html)|*.html|CSV Document (*.csv)|*.csv";
                    sfd.FileName = $"Executive_KPI_Report_{DateTime.Now:yyyyMMdd_HHmm}.xls";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        string ext = Path.GetExtension(sfd.FileName).ToLowerInvariant();
                        if (ext == ".csv")
                        {
                            ExportCsv(sfd.FileName);
                        }
                        else if (ext == ".xls" || ext == ".xlsx")
                        {
                            ExportExcel(sfd.FileName);
                        }
                        else
                        {
                            ExportHtml(sfd.FileName);
                        }

                        var res = MessageBox.Show("Executive Summary Report exported successfully!\n\nDo you want to open the exported file now?", "Export Success", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (res == DialogResult.Yes)
                        {
                            try
                            {
                                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportExcel(string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
            sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            sb.AppendLine(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
            sb.AppendLine(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"");
            sb.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            sb.AppendLine(" xmlns:html=\"http://www.w3.org/TR/REC-html40\">");
            sb.AppendLine(" <DocumentProperties xmlns=\"urn:schemas-microsoft-com:office:office\">");
            sb.AppendLine("  <Author>Nexoris ERP</Author>");
            sb.AppendLine($"  <Created>{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}</Created>");
            sb.AppendLine("  <Title>Executive Business KPI Dashboard</Title>");
            sb.AppendLine(" </DocumentProperties>");
            sb.AppendLine(" <Styles>");
            sb.AppendLine("  <Style ss:ID=\"Default\" ss:Name=\"Normal\"><Alignment ss:Vertical=\"Center\"/><Font ss:FontName=\"Segoe UI\" ss:Size=\"10\"/></Style>");
            sb.AppendLine("  <Style ss:ID=\"Title\"><Font ss:FontName=\"Segoe UI\" ss:Size=\"14\" ss:Bold=\"1\" ss:Color=\"#1F4E78\"/><Alignment ss:Horizontal=\"Left\" ss:Vertical=\"Center\"/></Style>");
            sb.AppendLine("  <Style ss:ID=\"Meta\"><Font ss:FontName=\"Segoe UI\" ss:Size=\"9\" ss:Italic=\"1\" ss:Color=\"#595959\"/><Alignment ss:Horizontal=\"Left\" ss:Vertical=\"Center\"/></Style>");
            sb.AppendLine("  <Style ss:ID=\"Header\"><Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Bold=\"1\" ss:Color=\"#FFFFFF\"/><Interior ss:Color=\"#1F4E78\" ss:Pattern=\"Solid\"/><Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\" ss:WrapText=\"1\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#B0C4DE\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#B0C4DE\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#B0C4DE\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#B0C4DE\"/></Borders></Style>");
            sb.AppendLine("  <Style ss:ID=\"HeaderSummary\"><Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Bold=\"1\" ss:Color=\"#FFFFFF\"/><Interior ss:Color=\"#203864\" ss:Pattern=\"Solid\"/><Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#B0C4DE\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#B0C4DE\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#B0C4DE\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#B0C4DE\"/></Borders></Style>");
            sb.AppendLine("  <Style ss:ID=\"CellText\"><Alignment ss:Horizontal=\"Left\" ss:Vertical=\"Center\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/></Borders></Style>");
            sb.AppendLine("  <Style ss:ID=\"CellTextBold\"><Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Bold=\"1\"/><Alignment ss:Horizontal=\"Left\" ss:Vertical=\"Center\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/></Borders></Style>");
            sb.AppendLine("  <Style ss:ID=\"CellCenter\"><Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/></Borders></Style>");
            sb.AppendLine("  <Style ss:ID=\"CellCurrency\"><Alignment ss:Horizontal=\"Right\" ss:Vertical=\"Center\"/><NumberFormat ss:Format=\"#,##0.00\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/></Borders></Style>");
            sb.AppendLine("  <Style ss:ID=\"CellCurrencyBold\"><Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Bold=\"1\"/><Alignment ss:Horizontal=\"Right\" ss:Vertical=\"Center\"/><NumberFormat ss:Format=\"#,##0.00\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/></Borders></Style>");
            sb.AppendLine("  <Style ss:ID=\"CellInteger\"><Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/><NumberFormat ss:Format=\"#,##0\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/></Borders></Style>");
            sb.AppendLine("  <Style ss:ID=\"CellPercent\"><Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/></Borders></Style>");
            sb.AppendLine("  <Style ss:ID=\"CellTotal\"><Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Bold=\"1\" ss:Color=\"#0F274A\"/><Interior ss:Color=\"#EDF2F8\" ss:Pattern=\"Solid\"/><Alignment ss:Horizontal=\"Right\" ss:Vertical=\"Center\"/><NumberFormat ss:Format=\"#,##0.00\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"2\" ss:Color=\"#1F4E78\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#B0C4DE\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#B0C4DE\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"2\" ss:Color=\"#1F4E78\"/></Borders></Style>");
            sb.AppendLine("  <Style ss:ID=\"CellTotalLabel\"><Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Bold=\"1\" ss:Color=\"#0F274A\"/><Interior ss:Color=\"#EDF2F8\" ss:Pattern=\"Solid\"/><Alignment ss:Horizontal=\"Left\" ss:Vertical=\"Center\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"2\" ss:Color=\"#1F4E78\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#B0C4DE\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#B0C4DE\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"2\" ss:Color=\"#1F4E78\"/></Borders></Style>");
            sb.AppendLine("  <Style ss:ID=\"CellTotalCenter\"><Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Bold=\"1\" ss:Color=\"#0F274A\"/><Interior ss:Color=\"#EDF2F8\" ss:Pattern=\"Solid\"/><Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"2\" ss:Color=\"#1F4E78\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#B0C4DE\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#B0C4DE\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"2\" ss:Color=\"#1F4E78\"/></Borders></Style>");
            sb.AppendLine("  <Style ss:ID=\"PositiveGrowth\"><Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Bold=\"1\" ss:Color=\"#1B5E20\"/><Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/></Borders></Style>");
            sb.AppendLine("  <Style ss:ID=\"NegativeGrowth\"><Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Bold=\"1\" ss:Color=\"#B71C1C\"/><Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E0E0E0\"/></Borders></Style>");
            sb.AppendLine(" </Styles>");

            string periodLabel = _model.FromDate <= new DateTime(1990, 1, 1) ? $"All Time (Up to {_model.ToDate:dd-MMM-yyyy})" : $"{_model.FromDate:dd-MMM-yyyy} to {_model.ToDate:dd-MMM-yyyy}";

            // ═══════════════════════════════════════════════════════════════════
            // WORKSHEET 1: MONTHLY MATRIX
            // ═══════════════════════════════════════════════════════════════════
            sb.AppendLine(" <Worksheet ss:Name=\"Monthly Matrix\">");
            sb.AppendLine("  <Table>");
            sb.AppendLine("   <Column ss:Width=\"90\"/>"); // Month
            for (int i = 0; i < 29; i++)
            {
                sb.AppendLine("   <Column ss:Width=\"80\"/>");
            }
            sb.AppendLine("   <Column ss:Width=\"85\"/>"); // Sales MoM%
            sb.AppendLine("   <Column ss:Width=\"85\"/>"); // Profit MoM%

            // Title Rows
            sb.AppendLine("   <Row ss:Height=\"24\">");
            sb.AppendLine("    <Cell ss:StyleID=\"Title\"><Data ss:Type=\"String\">Executive Business KPI Dashboard — Monthly Performance &amp; Growth Matrix</Data></Cell>");
            sb.AppendLine("   </Row>");
            sb.AppendLine("   <Row ss:Height=\"18\">");
            sb.AppendLine($"    <Cell ss:StyleID=\"Meta\"><Data ss:Type=\"String\">Selected Period: {EscapeXml(periodLabel)} | Generated: {_model.GeneratedAt:dd-MMM-yyyy hh:mm tt}</Data></Cell>");
            sb.AppendLine("   </Row>");
            sb.AppendLine("   <Row ss:Height=\"6\"/>"); // Spacer

            // Header Row
            sb.AppendLine("   <Row ss:Height=\"26\">");
            string[] headers = new string[] {
                "Month", "1.Stock", "2.StkProfit", "3.Sales", "4.Expenses", "5.NegStk",
                "6.NegItems", "7.Cash", "8.Bank", "9.SupPay", "10.CustRec",
                "11.NetProfit", "12.Drawings", "13.NBA", "14.Loss", "15.Extra",
                "16.AdjBal", "17.DelayCust", "18.DelaySupp", "19.Profit%", "20.ExcAlert",
                "21.LowAlert", "22.Reorder", "23.BadDebt", "24.WriteOff", "26.Deletions",
                "27.PriceChange", "28.StockAdj", "29.SupAdvance", "30.CustAdvance",
                "Sales MoM%", "Profit MoM%"
            };
            foreach (var h in headers)
            {
                sb.AppendLine($"    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">{EscapeXml(h)}</Data></Cell>");
            }
            sb.AppendLine("   </Row>");

            if (_model.MonthlyGrowthMatrix != null && _model.MonthlyGrowthMatrix.Count > 0)
            {
                foreach (var m in _model.MonthlyGrowthMatrix)
                {
                    decimal profitMargin = m.SalesAmount > 0 ? (m.NetProfitAmount / m.SalesAmount) * 100m : 0m;
                    string sStyle = m.SalesGrowthPercent >= 0 ? "PositiveGrowth" : "NegativeGrowth";
                    string pStyle = m.NetProfitGrowthPercent >= 0 ? "PositiveGrowth" : "NegativeGrowth";

                    sb.AppendLine("   <Row ss:Height=\"20\">");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellTextBold\"><Data ss:Type=\"String\">{EscapeXml(m.MonthName)}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellCurrency\"><Data ss:Type=\"Number\">{_model.TotalStockCostValue}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellCurrency\"><Data ss:Type=\"Number\">{_model.StockProfitPotential}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellCurrency\"><Data ss:Type=\"Number\">{m.SalesAmount}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellCurrency\"><Data ss:Type=\"Number\">{m.ExpenseAmount}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellCurrency\"><Data ss:Type=\"Number\">{_model.NegativeStockImpactValue}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellInteger\"><Data ss:Type=\"Number\">{_model.NegativeStockItemCount}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellCurrency\"><Data ss:Type=\"Number\">{_model.CashInHand}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellCurrency\"><Data ss:Type=\"Number\">{_model.BankBalance}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellCurrency\"><Data ss:Type=\"Number\">{_model.SupplierPayables}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellCurrency\"><Data ss:Type=\"Number\">{_model.CustomerReceivables}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellCurrencyBold\"><Data ss:Type=\"Number\">{m.NetProfitAmount}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellCurrency\"><Data ss:Type=\"Number\">{_model.OwnerDrawings}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellCurrency\"><Data ss:Type=\"Number\">{_model.NetBusinessAsset}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellCurrency\"><Data ss:Type=\"Number\">{_model.LossStockValue}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellCurrency\"><Data ss:Type=\"Number\">{_model.ExtraStockValue}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellCurrency\"><Data ss:Type=\"Number\">{_model.NetStockAdjustmentValue}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellCurrency\"><Data ss:Type=\"Number\">{_model.DelayedCustomerReceivables30Days}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellCurrency\"><Data ss:Type=\"Number\">{_model.DelayedSupplierPayables30Days}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellPercent\"><Data ss:Type=\"String\">{profitMargin:N1}%</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellInteger\"><Data ss:Type=\"Number\">{_model.ExcessStockAlertCount}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellInteger\"><Data ss:Type=\"Number\">{_model.LowStockAlertCount}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellInteger\"><Data ss:Type=\"Number\">{_model.ReorderAlertCount}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellCurrency\"><Data ss:Type=\"Number\">{_model.CustomerBadDebts}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellCurrency\"><Data ss:Type=\"Number\">{_model.SupplierWriteOffs}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellInteger\"><Data ss:Type=\"Number\">{_model.DeletionCount}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellInteger\"><Data ss:Type=\"Number\">{_model.PriceChangeCount}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellInteger\"><Data ss:Type=\"Number\">{_model.StockAdjustmentCount}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellCurrency\"><Data ss:Type=\"Number\">{_model.SupplierAdvanceBalance}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"CellCurrency\"><Data ss:Type=\"Number\">{_model.CustomerAdvanceBalance}</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"{sStyle}\"><Data ss:Type=\"String\">{(m.SalesGrowthPercent >= 0 ? "+" : "")}{m.SalesGrowthPercent:N1}%</Data></Cell>");
                    sb.AppendLine($"    <Cell ss:StyleID=\"{pStyle}\"><Data ss:Type=\"String\">{(m.NetProfitGrowthPercent >= 0 ? "+" : "")}{m.NetProfitGrowthPercent:N1}%</Data></Cell>");
                    sb.AppendLine("   </Row>");
                }
            }

            // Period Total Row
            sb.AppendLine("   <Row ss:Height=\"22\">");
            sb.AppendLine("    <Cell ss:StyleID=\"CellTotalLabel\"><Data ss:Type=\"String\">Selected Period Total</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotal\"><Data ss:Type=\"Number\">{_model.TotalStockCostValue}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotal\"><Data ss:Type=\"Number\">{_model.StockProfitPotential}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotal\"><Data ss:Type=\"Number\">{_model.TotalSalesRevenue}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotal\"><Data ss:Type=\"Number\">{_model.TotalBusinessExpenses}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotal\"><Data ss:Type=\"Number\">{_model.NegativeStockImpactValue}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotalCenter\"><Data ss:Type=\"Number\">{_model.NegativeStockItemCount}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotal\"><Data ss:Type=\"Number\">{_model.CashInHand}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotal\"><Data ss:Type=\"Number\">{_model.BankBalance}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotal\"><Data ss:Type=\"Number\">{_model.SupplierPayables}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotal\"><Data ss:Type=\"Number\">{_model.CustomerReceivables}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotal\"><Data ss:Type=\"Number\">{_model.ActualNetProfit}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotal\"><Data ss:Type=\"Number\">{_model.OwnerDrawings}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotal\"><Data ss:Type=\"Number\">{_model.NetBusinessAsset}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotal\"><Data ss:Type=\"Number\">{_model.LossStockValue}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotal\"><Data ss:Type=\"Number\">{_model.ExtraStockValue}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotal\"><Data ss:Type=\"Number\">{_model.NetStockAdjustmentValue}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotal\"><Data ss:Type=\"Number\">{_model.DelayedCustomerReceivables30Days}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotal\"><Data ss:Type=\"Number\">{_model.DelayedSupplierPayables30Days}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotalCenter\"><Data ss:Type=\"String\">{_model.OperatingProfitMarginPercent:N1}%</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotalCenter\"><Data ss:Type=\"Number\">{_model.ExcessStockAlertCount}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotalCenter\"><Data ss:Type=\"Number\">{_model.LowStockAlertCount}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotalCenter\"><Data ss:Type=\"Number\">{_model.ReorderAlertCount}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotal\"><Data ss:Type=\"Number\">{_model.CustomerBadDebts}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotal\"><Data ss:Type=\"Number\">{_model.SupplierWriteOffs}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotalCenter\"><Data ss:Type=\"Number\">{_model.DeletionCount}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotalCenter\"><Data ss:Type=\"Number\">{_model.PriceChangeCount}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotalCenter\"><Data ss:Type=\"Number\">{_model.StockAdjustmentCount}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotal\"><Data ss:Type=\"Number\">{_model.SupplierAdvanceBalance}</Data></Cell>");
            sb.AppendLine($"    <Cell ss:StyleID=\"CellTotal\"><Data ss:Type=\"Number\">{_model.CustomerAdvanceBalance}</Data></Cell>");
            sb.AppendLine("    <Cell ss:StyleID=\"CellTotalCenter\"><Data ss:Type=\"String\">-</Data></Cell>");
            sb.AppendLine("    <Cell ss:StyleID=\"CellTotalCenter\"><Data ss:Type=\"String\">-</Data></Cell>");
            sb.AppendLine("   </Row>");
            sb.AppendLine("  </Table>");
            sb.AppendLine(" </Worksheet>");

            // ═══════════════════════════════════════════════════════════════════
            // WORKSHEET 2: COMPLETE 30 KPI SUMMARY
            // ═══════════════════════════════════════════════════════════════════
            sb.AppendLine(" <Worksheet ss:Name=\"30 KPI Summary\">");
            sb.AppendLine("  <Table>");
            sb.AppendLine("   <Column ss:Width=\"55\"/>");  // SI No
            sb.AppendLine("   <Column ss:Width=\"240\"/>"); // Metric Name
            sb.AppendLine("   <Column ss:Width=\"130\"/>"); // Value
            sb.AppendLine("   <Column ss:Width=\"380\"/>"); // Business Notes

            sb.AppendLine("   <Row ss:Height=\"24\">");
            sb.AppendLine("    <Cell ss:StyleID=\"Title\"><Data ss:Type=\"String\">Executive Business KPI Dashboard — Complete 30 Metrics Summary</Data></Cell>");
            sb.AppendLine("   </Row>");
            sb.AppendLine("   <Row ss:Height=\"18\">");
            sb.AppendLine($"    <Cell ss:StyleID=\"Meta\"><Data ss:Type=\"String\">Period: {EscapeXml(periodLabel)} | Generated: {_model.GeneratedAt:dd-MMM-yyyy hh:mm tt}</Data></Cell>");
            sb.AppendLine("   </Row>");
            sb.AppendLine("   <Row ss:Height=\"6\"/>");

            sb.AppendLine("   <Row ss:Height=\"24\">");
            sb.AppendLine("    <Cell ss:StyleID=\"HeaderSummary\"><Data ss:Type=\"String\">SI No</Data></Cell>");
            sb.AppendLine("    <Cell ss:StyleID=\"HeaderSummary\"><Data ss:Type=\"String\">Metric Name</Data></Cell>");
            sb.AppendLine("    <Cell ss:StyleID=\"HeaderSummary\"><Data ss:Type=\"String\">Value</Data></Cell>");
            sb.AppendLine("    <Cell ss:StyleID=\"HeaderSummary\"><Data ss:Type=\"String\">Business Context &amp; Notes</Data></Cell>");
            sb.AppendLine("   </Row>");

            var summaryList = new List<Tuple<int, string, string, string>>()
            {
                Tuple.Create(1, "Total Stock Value", FormatCurr(_model.TotalStockCostValue), $"Valued at Cost Price ({_model.TotalStockItemCount:N0} Items, {_model.TotalStockQuantity:N0} Units)"),
                Tuple.Create(2, "Stock Profit Potential", FormatCurr(_model.StockProfitPotential), $"Retail Value: {FormatCurr(_model.TotalStockRetailValue)}"),
                Tuple.Create(3, "Total Sales Revenue", FormatCurr(_model.TotalSalesRevenue), $"{_model.TotalSalesBillCount:N0} Bills | Purchases: {FormatCurr(_model.TotalPurchases)}"),
                Tuple.Create(4, "Total Business Expenses", FormatCurr(_model.TotalBusinessExpenses), $"Direct: {FormatCurr(_model.DirectExpenses)} | Indirect: {FormatCurr(_model.IndirectExpenses)}"),
                Tuple.Create(5, "Negative Stock Impact", FormatCurr(_model.NegativeStockImpactValue), "Cost value distortion from negative stock"),
                Tuple.Create(6, "Negative Stock Items", $"{_model.NegativeStockItemCount} Items", $"Total Negative Quantity: {_model.NegativeStockTotalQty:N0}"),
                Tuple.Create(7, "Cash in Hand", FormatCurr(_model.CashInHand), "Liquid cash available in tills and safe"),
                Tuple.Create(8, "Bank Balance", FormatCurr(_model.BankBalance), "Active commercial bank accounts balance"),
                Tuple.Create(9, "Supplier Payables", FormatCurr(_model.SupplierPayables), $"Owed to vendors across {_model.SupplierPayablesCount} bills"),
                Tuple.Create(10, "Customer Receivables", FormatCurr(_model.CustomerReceivables), $"Outstanding dues from {_model.CustomerReceivablesCount} customer invoices"),
                Tuple.Create(11, "Actual Net Profit", FormatCurr(_model.ActualNetProfit), $"Operating Margin: {_model.OperatingProfitMarginPercent:N2}%"),
                Tuple.Create(12, "Owner Drawings", FormatCurr(_model.OwnerDrawings), "Proprietor capital withdrawals"),
                Tuple.Create(13, "Net Business Asset (NBA)", FormatCurr(_model.NetBusinessAsset), $"Total Assets: {FormatCurr(_model.TotalAssets)} | Liabilities: {FormatCurr(_model.TotalLiabilities)}"),
                Tuple.Create(14, "Loss Stock (Damaged / Out)", FormatCurr(_model.LossStockValue), $"Damaged & Written Off ({_model.LossStockQty:N2} Qty)"),
                Tuple.Create(15, "Extra Stock (Found / In)", FormatCurr(_model.ExtraStockValue), $"Surplus Stock Added ({_model.ExtraStockQty:N2} Qty)"),
                Tuple.Create(16, "Net Stock Adjustment", FormatCurr(_model.NetStockAdjustmentValue), "Net Discrepancy Balance"),
                Tuple.Create(17, "Delayed Customer Receivables (>30 Days)", FormatCurr(_model.DelayedCustomerReceivables30Days), $"{_model.DelayedCustomerCount30Days} High-Risk Invoices overdue"),
                Tuple.Create(18, "Delayed Supplier Payables (>30 Days)", FormatCurr(_model.DelayedSupplierPayables30Days), $"{_model.DelayedSupplierCount30Days} Vendor Invoices overdue"),
                Tuple.Create(19, "Operating Profit Margin %", $"{_model.OperatingProfitMarginPercent:N2}%", "Net Profit / Sales Turnover"),
                Tuple.Create(20, "Excess Stock Alert", $"{_model.ExcessStockAlertCount} Items", "Items above maximum inventory limits"),
                Tuple.Create(21, "Low Stock Alert", $"{_model.LowStockAlertCount} Items", "Items below minimum safety stock"),
                Tuple.Create(22, "Reorder Alert", $"{_model.ReorderAlertCount} Items", "Items at or below reorder level"),
                Tuple.Create(23, "Customer Bad Debts", FormatCurr(_model.CustomerBadDebts), "Uncollectible receivables written off"),
                Tuple.Create(24, "Supplier Write-Offs", FormatCurr(_model.SupplierWriteOffs), "Vendor settlement discounts / write-offs"),
                Tuple.Create(26, "Deletion & Cancellation Logs", $"{_model.DeletionCount} Events", "Cancelled bills, deleted receipts & vouchers"),
                Tuple.Create(27, "Price Change Modifications", $"{_model.PriceChangeCount} Events", "Item Master price updates"),
                Tuple.Create(28, "Physical Stock Discrepancy Logs", $"{_model.StockAdjustmentCount} Adjustments", "Stock reconciliation entries"),
                Tuple.Create(29, "Supplier Advance / Overpayment", FormatCurr(_model.SupplierAdvanceBalance), "Advance payments & vendor debit balances"),
                Tuple.Create(30, "Customer Advance / Deposits", FormatCurr(_model.CustomerAdvanceBalance), "Advance deposits & customer credit balances")
            };

            foreach (var item in summaryList)
            {
                sb.AppendLine("   <Row ss:Height=\"20\">");
                sb.AppendLine($"    <Cell ss:StyleID=\"CellCenter\"><Data ss:Type=\"Number\">{item.Item1}</Data></Cell>");
                sb.AppendLine($"    <Cell ss:StyleID=\"CellText\"><Data ss:Type=\"String\">{EscapeXml(item.Item2)}</Data></Cell>");
                sb.AppendLine($"    <Cell ss:StyleID=\"CellCurrencyBold\"><Data ss:Type=\"String\">{EscapeXml(item.Item3)}</Data></Cell>");
                sb.AppendLine($"    <Cell ss:StyleID=\"CellText\"><Data ss:Type=\"String\">{EscapeXml(item.Item4)}</Data></Cell>");
                sb.AppendLine("   </Row>");
            }

            sb.AppendLine("  </Table>");
            sb.AppendLine(" </Worksheet>");
            sb.AppendLine("</Workbook>");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private static string EscapeXml(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        private void ExportHtml(string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'><title>Executive Business KPI Report</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("@page { size: landscape; margin: 10mm; }");
            sb.AppendLine("body { font-family: 'Segoe UI', -apple-system, BlinkMacSystemFont, Arial, sans-serif; background: #f8fafc; color: #0f172a; padding: 20px; line-height: 1.4; }");
            sb.AppendLine(".container { max-width: 100%; margin: 0 auto; background: #ffffff; padding: 24px; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.06); }");
            sb.AppendLine("h1 { color: #0f274a; margin: 0 0 6px 0; font-size: 22px; font-weight: 700; }");
            sb.AppendLine(".meta { color: #64748b; font-size: 12px; margin-bottom: 20px; border-bottom: 1px solid #e2e8f0; padding-bottom: 8px; }");
            sb.AppendLine("h2 { color: #1e3a8a; font-size: 15px; margin-top: 20px; margin-bottom: 8px; border-bottom: 2px solid #3b82f6; padding-bottom: 4px; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 6px; margin-bottom: 16px; font-size: 11px; }");
            sb.AppendLine("th, td { border: 1px solid #e2e8f0; padding: 6px 8px; text-align: left; }");
            sb.AppendLine("th { background: #1e40af; color: #ffffff; font-weight: 600; font-size: 11px; white-space: nowrap; }");
            sb.AppendLine("tr:nth-child(even) { background: #f8fafc; }");
            sb.AppendLine(".value { font-weight: 700; color: #0369a1; text-align: right; }");
            sb.AppendLine(".positive { color: #15803d; font-weight: 600; text-align: center; }");
            sb.AppendLine(".negative { color: #b91c1c; font-weight: 600; text-align: center; }");
            sb.AppendLine(".center { text-align: center; }");
            sb.AppendLine(".right { text-align: right; }");
            sb.AppendLine("</style></head><body><div class='container'>");

            string periodLabel = _model.FromDate <= new DateTime(1990, 1, 1) ? $"All Time (Up to {_model.ToDate:dd-MMM-yyyy})" : $"{_model.FromDate:dd-MMM-yyyy} to {_model.ToDate:dd-MMM-yyyy}";
            sb.AppendLine("<h1>Executive Business KPI Cockpit — 30 Key Metrics</h1>");
            sb.AppendLine($"<div class='meta'>Period: <b>{periodLabel}</b> | Generated: <b>{_model.GeneratedAt:dd-MMM-yyyy hh:mm tt}</b></div>");

            // 1. Monthly Comparison Matrix
            sb.AppendLine("<h2>1. Monthly Executive Performance & Growth Matrix</h2>");
            sb.AppendLine("<div style='overflow-x:auto;'><table><tr>");
            sb.AppendLine("<th>Month</th><th>1.Stock</th><th>2.StkProfit</th><th>3.Sales</th><th>4.Expenses</th><th>5.NegStk</th><th>6.NegItems</th><th>7.Cash</th><th>8.Bank</th><th>9.SupPay</th><th>10.CustRec</th><th>11.NetProfit</th><th>12.Drawings</th><th>13.NBA</th><th>14.Loss</th><th>15.Extra</th><th>16.AdjBal</th><th>17.DelayCust</th><th>18.DelaySupp</th><th>19.Profit%</th><th>20.ExcAlert</th><th>21.LowAlert</th><th>22.Reorder</th><th>23.BadDebt</th><th>24.WriteOff</th><th>26.Deletions</th><th>27.PriceChange</th><th>28.StockAdj</th><th>29.SupAdvance</th><th>30.CustAdvance</th><th>Sales MoM%</th><th>Profit MoM%</th></tr>");

            if (_model.MonthlyGrowthMatrix != null && _model.MonthlyGrowthMatrix.Count > 0)
            {
                foreach (var m in _model.MonthlyGrowthMatrix)
                {
                    decimal profitMargin = m.SalesAmount > 0 ? (m.NetProfitAmount / m.SalesAmount) * 100m : 0m;
                    string sColor = m.SalesGrowthPercent >= 0 ? "positive" : "negative";
                    string pColor = m.NetProfitGrowthPercent >= 0 ? "positive" : "negative";

                    sb.AppendLine($"<tr><td><b>{m.MonthName}</b></td><td class='right'>{FormatCurr(_model.TotalStockCostValue)}</td><td class='right'>{FormatCurr(_model.StockProfitPotential)}</td><td class='right'>{FormatCurr(m.SalesAmount)}</td><td class='right'>{FormatCurr(m.ExpenseAmount)}</td><td class='right'>{FormatCurr(_model.NegativeStockImpactValue)}</td><td class='center'>{_model.NegativeStockItemCount}</td><td class='right'>{FormatCurr(_model.CashInHand)}</td><td class='right'>{FormatCurr(_model.BankBalance)}</td><td class='right'>{FormatCurr(_model.SupplierPayables)}</td><td class='right'>{FormatCurr(_model.CustomerReceivables)}</td><td class='right' style='font-weight:700;'>{FormatCurr(m.NetProfitAmount)}</td><td class='right'>{FormatCurr(_model.OwnerDrawings)}</td><td class='right'>{FormatCurr(_model.NetBusinessAsset)}</td><td class='right'>{FormatCurr(_model.LossStockValue)}</td><td class='right'>{FormatCurr(_model.ExtraStockValue)}</td><td class='right'>{FormatCurr(_model.NetStockAdjustmentValue)}</td><td class='right'>{FormatCurr(_model.DelayedCustomerReceivables30Days)}</td><td class='right'>{FormatCurr(_model.DelayedSupplierPayables30Days)}</td><td class='center'>{profitMargin:N1}%</td><td class='center'>{_model.ExcessStockAlertCount}</td><td class='center'>{_model.LowStockAlertCount}</td><td class='center'>{_model.ReorderAlertCount}</td><td class='right'>{FormatCurr(_model.CustomerBadDebts)}</td><td class='right'>{FormatCurr(_model.SupplierWriteOffs)}</td><td class='center'>{_model.DeletionCount}</td><td class='center'>{_model.PriceChangeCount}</td><td class='center'>{_model.StockAdjustmentCount}</td><td class='right'>{FormatCurr(_model.SupplierAdvanceBalance)}</td><td class='right'>{FormatCurr(_model.CustomerAdvanceBalance)}</td><td class='{sColor}'>{(m.SalesGrowthPercent >= 0 ? "+" : "")}{m.SalesGrowthPercent:N1}%</td><td class='{pColor}'>{(m.NetProfitGrowthPercent >= 0 ? "+" : "")}{m.NetProfitGrowthPercent:N1}%</td></tr>");
                }
            }
            sb.AppendLine("</table></div>");

            // 2. Complete 30 KPI Summary Table
            sb.AppendLine("<h2>2. Complete 30 Business KPI Summary</h2>");
            sb.AppendLine("<table><tr><th style='width:50px;'>#</th><th>Metric Name</th><th style='text-align:right;'>Value</th><th>Business Context & Notes</th></tr>");
            sb.AppendLine($"<tr><td>1</td><td>Total Stock Value</td><td class='value'>{FormatCurr(_model.TotalStockCostValue)}</td><td>Valued at Cost Price ({_model.TotalStockItemCount:N0} Items, {_model.TotalStockQuantity:N0} Units)</td></tr>");
            sb.AppendLine($"<tr><td>2</td><td>Stock Profit Potential</td><td class='value'>{FormatCurr(_model.StockProfitPotential)}</td><td>Retail Value: {FormatCurr(_model.TotalStockRetailValue)}</td></tr>");
            sb.AppendLine($"<tr><td>3</td><td>Total Sales Revenue</td><td class='value'>{FormatCurr(_model.TotalSalesRevenue)}</td><td>{_model.TotalSalesBillCount:N0} Bills | Purchases: {FormatCurr(_model.TotalPurchases)}</td></tr>");
            sb.AppendLine($"<tr><td>4</td><td>Total Business Expenses</td><td class='value'>{FormatCurr(_model.TotalBusinessExpenses)}</td><td>Direct: {FormatCurr(_model.DirectExpenses)} | Indirect: {FormatCurr(_model.IndirectExpenses)}</td></tr>");
            sb.AppendLine($"<tr><td>5</td><td>Negative Stock Impact</td><td class='value'>{FormatCurr(_model.NegativeStockImpactValue)}</td><td>Cost value distortion from negative stock</td></tr>");
            sb.AppendLine($"<tr><td>6</td><td>Negative Stock Items</td><td class='value'>{_model.NegativeStockItemCount} Items</td><td>Total Negative Quantity: {_model.NegativeStockTotalQty:N0}</td></tr>");
            sb.AppendLine($"<tr><td>7</td><td>Cash in Hand</td><td class='value'>{FormatCurr(_model.CashInHand)}</td><td>Liquid cash available in tills and safe</td></tr>");
            sb.AppendLine($"<tr><td>8</td><td>Bank Balance</td><td class='value'>{FormatCurr(_model.BankBalance)}</td><td>Active commercial bank accounts balance</td></tr>");
            sb.AppendLine($"<tr><td>9</td><td>Supplier Payables</td><td class='value'>{FormatCurr(_model.SupplierPayables)}</td><td>Owed to vendors across {_model.SupplierPayablesCount} bills</td></tr>");
            sb.AppendLine($"<tr><td>10</td><td>Customer Receivables</td><td class='value'>{FormatCurr(_model.CustomerReceivables)}</td><td>Outstanding dues from {_model.CustomerReceivablesCount} customer invoices</td></tr>");
            sb.AppendLine($"<tr><td>11</td><td>Actual Net Profit</td><td class='value'>{FormatCurr(_model.ActualNetProfit)}</td><td>Operating Margin: {_model.OperatingProfitMarginPercent:N2}%</td></tr>");
            sb.AppendLine($"<tr><td>12</td><td>Owner Drawings</td><td class='value'>{FormatCurr(_model.OwnerDrawings)}</td><td>Proprietor capital withdrawals</td></tr>");
            sb.AppendLine($"<tr><td>13</td><td>Net Business Asset (NBA)</td><td class='value'>{FormatCurr(_model.NetBusinessAsset)}</td><td>Total Assets: {FormatCurr(_model.TotalAssets)} | Liabilities: {FormatCurr(_model.TotalLiabilities)}</td></tr>");
            sb.AppendLine($"<tr><td>14</td><td>Loss Stock (Damaged / Out)</td><td class='value'>{FormatCurr(_model.LossStockValue)}</td><td>Damaged & Written Off ({_model.LossStockQty:N2} Qty)</td></tr>");
            sb.AppendLine($"<tr><td>15</td><td>Extra Stock (Found / In)</td><td class='value'>{FormatCurr(_model.ExtraStockValue)}</td><td>Surplus Stock Added ({_model.ExtraStockQty:N2} Qty)</td></tr>");
            sb.AppendLine($"<tr><td>16</td><td>Net Stock Adjustment</td><td class='value'>{FormatCurr(_model.NetStockAdjustmentValue)}</td><td>Net Discrepancy Balance</td></tr>");
            sb.AppendLine($"<tr><td>17</td><td>Delayed Customer Receivables (>30 Days)</td><td class='value'>{FormatCurr(_model.DelayedCustomerReceivables30Days)}</td><td>{_model.DelayedCustomerCount30Days} High-Risk Invoices overdue</td></tr>");
            sb.AppendLine($"<tr><td>18</td><td>Delayed Supplier Payables (>30 Days)</td><td class='value'>{FormatCurr(_model.DelayedSupplierPayables30Days)}</td><td>{_model.DelayedSupplierCount30Days} Vendor Invoices overdue</td></tr>");
            sb.AppendLine($"<tr><td>19</td><td>Operating Profit Margin %</td><td class='value'>{_model.OperatingProfitMarginPercent:N2}%</td><td>Net Profit / Sales Turnover</td></tr>");
            sb.AppendLine($"<tr><td>20</td><td>Excess Stock Alert</td><td class='value'>{_model.ExcessStockAlertCount} Items</td><td>Items above maximum inventory limits</td></tr>");
            sb.AppendLine($"<tr><td>21</td><td>Low Stock Alert</td><td class='value'>{_model.LowStockAlertCount} Items</td><td>Items below minimum safety stock</td></tr>");
            sb.AppendLine($"<tr><td>22</td><td>Reorder Alert</td><td class='value'>{_model.ReorderAlertCount} Items</td><td>Items at or below reorder level</td></tr>");
            sb.AppendLine($"<tr><td>23</td><td>Customer Bad Debts</td><td class='value'>{FormatCurr(_model.CustomerBadDebts)}</td><td>Uncollectible receivables written off</td></tr>");
            sb.AppendLine($"<tr><td>24</td><td>Supplier Write-Offs</td><td class='value'>{FormatCurr(_model.SupplierWriteOffs)}</td><td>Vendor settlement discounts / write-offs</td></tr>");
            sb.AppendLine($"<tr><td>26</td><td>Deletion & Cancellation Logs</td><td class='value'>{_model.DeletionCount} Events</td><td>Cancelled bills, deleted receipts & vouchers</td></tr>");
            sb.AppendLine($"<tr><td>27</td><td>Price Change Modifications</td><td class='value'>{_model.PriceChangeCount} Events</td><td>Item Master price updates</td></tr>");
            sb.AppendLine($"<tr><td>28</td><td>Physical Stock Discrepancy Logs</td><td class='value'>{_model.StockAdjustmentCount} Adjustments</td><td>Stock reconciliation entries</td></tr>");
            sb.AppendLine($"<tr><td>29</td><td>Supplier Advance / Overpayment</td><td class='value'>{FormatCurr(_model.SupplierAdvanceBalance)}</td><td>Advance payments & vendor debit balances</td></tr>");
            sb.AppendLine($"<tr><td>30</td><td>Customer Advance / Deposits</td><td class='value'>{FormatCurr(_model.CustomerAdvanceBalance)}</td><td>Advance deposits & customer credit balances</td></tr>");
            sb.AppendLine("</table>");

            sb.AppendLine("</div></body></html>");
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private void ExportCsv(string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SI No,Metric Name,Value,Notes");
            sb.AppendLine($"1,Total Stock Value,\"{_model.TotalStockCostValue:N2}\",At Cost Price ({_model.TotalStockItemCount} Items, {_model.TotalStockQuantity} Units)");
            sb.AppendLine($"2,Stock Profit Potential,\"{_model.StockProfitPotential:N2}\",Retail Value: {_model.TotalStockRetailValue:N2}");
            sb.AppendLine($"3,Total Sales Revenue,\"{_model.TotalSalesRevenue:N2}\",Bills: {_model.TotalSalesBillCount}, Purchases: {_model.TotalPurchases:N2}");
            sb.AppendLine($"4,Total Business Expenses,\"{_model.TotalBusinessExpenses:N2}\",Direct: {_model.DirectExpenses:N2}, Indir: {_model.IndirectExpenses:N2}");
            sb.AppendLine($"5,Negative Stock Impact,\"{_model.NegativeStockImpactValue:N2}\",Cost Value Distortion");
            sb.AppendLine($"6,Negative Stock Items,\"{_model.NegativeStockItemCount}\",Total Qty: {_model.NegativeStockTotalQty:N0}");
            sb.AppendLine($"7,Cash in Hand,\"{_model.CashInHand:N2}\",Counter & Safe Cash");
            sb.AppendLine($"8,Bank Balance,\"{_model.BankBalance:N2}\",Active Bank Accounts");
            sb.AppendLine($"9,Supplier Payables,\"{_model.SupplierPayables:N2}\",Bills Count: {_model.SupplierPayablesCount}");
            sb.AppendLine($"10,Customer Receivables,\"{_model.CustomerReceivables:N2}\",Invoices Count: {_model.CustomerReceivablesCount}");
            sb.AppendLine($"11,Actual Net Profit,\"{_model.ActualNetProfit:N2}\",Margin: {_model.OperatingProfitMarginPercent:N2}%");
            sb.AppendLine($"12,Owner Drawings,\"{_model.OwnerDrawings:N2}\",Capital Withdrawals");
            sb.AppendLine($"13,Net Business Asset (NBA),\"{_model.NetBusinessAsset:N2}\",Assets: {_model.TotalAssets:N2} | Liab: {_model.TotalLiabilities:N2}");
            sb.AppendLine($"14,Loss Stock (Damaged / Out),\"{_model.LossStockValue:N2}\",Discrepancy Qty: {_model.LossStockQty:N2}");
            sb.AppendLine($"15,Extra Stock (Found / In),\"{_model.ExtraStockValue:N2}\",Surplus Qty: {_model.ExtraStockQty:N2}");
            sb.AppendLine($"16,Net Stock Adjustment,\"{_model.NetStockAdjustmentValue:N2}\",Net Variance");
            sb.AppendLine($"17,Delayed Customer Receivables (>30 Days),\"{_model.DelayedCustomerReceivables30Days:N2}\",Count: {_model.DelayedCustomerCount30Days}");
            sb.AppendLine($"18,Delayed Supplier Payables (>30 Days),\"{_model.DelayedSupplierPayables30Days:N2}\",Count: {_model.DelayedSupplierCount30Days}");
            sb.AppendLine($"19,Operating Profit Margin %,\"{_model.OperatingProfitMarginPercent:N2}%\",Net Margin");
            sb.AppendLine($"20,Excess Stock Alert,\"{_model.ExcessStockAlertCount}\",Over Max Limit");
            sb.AppendLine($"21,Low Stock Alert,\"{_model.LowStockAlertCount}\",Below Min Limit");
            sb.AppendLine($"22,Reorder Alert,\"{_model.ReorderAlertCount}\",Reorder Needed");
            sb.AppendLine($"23,Customer Bad Debts,\"{_model.CustomerBadDebts:N2}\",Written Off");
            sb.AppendLine($"24,Supplier Write-Offs,\"{_model.SupplierWriteOffs:N2}\",Discounts Settled");
            sb.AppendLine($"26,Deletion & Cancellation Logs,\"{_model.DeletionCount}\",Deleted Bills/Vouchers");
            sb.AppendLine($"27,Price Change Modifications,\"{_model.PriceChangeCount}\",Item Master Edits");
            sb.AppendLine($"28,Physical Stock Discrepancy Logs,\"{_model.StockAdjustmentCount}\",Stock Adjustments");
            sb.AppendLine($"29,Supplier Advance / Overpayment,\"{_model.SupplierAdvanceBalance:N2}\",Debit Balances");
            sb.AppendLine($"30,Customer Advance / Deposits,\"{_model.CustomerAdvanceBalance:N2}\",Credit Balances");

            // Monthly Matrix in CSV
            sb.AppendLine();
            sb.AppendLine("Monthly Performance & Growth Matrix");
            sb.AppendLine("Month,1.Stock,2.StkProfit,3.Sales,4.Expenses,5.NegStk,6.NegItems,7.Cash,8.Bank,9.SupPay,10.CustRec,11.NetProfit,12.Drawings,13.NBA,14.Loss,15.Extra,16.AdjBal,17.DelayCust,18.DelaySupp,19.Profit%,20.ExcAlert,21.LowAlert,22.Reorder,23.BadDebt,24.WriteOff,26.Deletions,27.PriceChange,28.StockAdj,29.SupAdvance,30.CustAdvance,Sales Growth %,Profit Growth %");

            if (_model.MonthlyGrowthMatrix != null && _model.MonthlyGrowthMatrix.Count > 0)
            {
                foreach (var m in _model.MonthlyGrowthMatrix)
                {
                    decimal profitMargin = m.SalesAmount > 0 ? (m.NetProfitAmount / m.SalesAmount) * 100m : 0m;
                    sb.AppendLine($"\"{m.MonthName}\",\"{_model.TotalStockCostValue:N2}\",\"{_model.StockProfitPotential:N2}\",\"{m.SalesAmount:N2}\",\"{m.ExpenseAmount:N2}\",\"{_model.NegativeStockImpactValue:N2}\",{_model.NegativeStockItemCount},\"{_model.CashInHand:N2}\",\"{_model.BankBalance:N2}\",\"{_model.SupplierPayables:N2}\",\"{_model.CustomerReceivables:N2}\",\"{m.NetProfitAmount:N2}\",\"{_model.OwnerDrawings:N2}\",\"{_model.NetBusinessAsset:N2}\",\"{_model.LossStockValue:N2}\",\"{_model.ExtraStockValue:N2}\",\"{_model.NetStockAdjustmentValue:N2}\",\"{_model.DelayedCustomerReceivables30Days:N2}\",\"{_model.DelayedSupplierPayables30Days:N2}\",\"{profitMargin:N1}%\",{_model.ExcessStockAlertCount},{_model.LowStockAlertCount},{_model.ReorderAlertCount},\"{_model.CustomerBadDebts:N2}\",\"{_model.SupplierWriteOffs:N2}\",{_model.DeletionCount},{_model.PriceChangeCount},{_model.StockAdjustmentCount},\"{_model.SupplierAdvanceBalance:N2}\",\"{_model.CustomerAdvanceBalance:N2}\",\"{m.SalesGrowthPercent:N1}%\",\"{m.NetProfitGrowthPercent:N1}%\"");
                }
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private string FormatCurr(decimal val)
        {
            return "₹ " + val.ToString("N2", _culture);
        }
    }
}
