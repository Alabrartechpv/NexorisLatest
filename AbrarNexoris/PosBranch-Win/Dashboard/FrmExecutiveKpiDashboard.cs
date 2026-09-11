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
            comboPeriod.Items.Add("Today", "Today");
            comboPeriod.Items.Add("This Week", "This Week");
            comboPeriod.Items.Add("This Month", "This Month");
            comboPeriod.Items.Add("This Quarter", "This Quarter");
            comboPeriod.Items.Add("This Year", "This Year");
            comboPeriod.Items.Add("Custom", "Custom");
            comboPeriod.SelectedIndex = 4; // Default: This Year

            comboPeriod.ValueChanged += (s, e) =>
            {
                string sel = comboPeriod.Text;
                DateTime now = DateTime.Today;
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
            dtFrom.Value = new DateTime(now.Year, 1, 1);
            dtTo.Value = now;
        }

        private void LoadDashboardData()
        {
            btnApply.Enabled = false;
            btnRefresh.Enabled = false;
            Cursor = Cursors.WaitCursor;

            DateTime from = dtFrom.Value != null ? Convert.ToDateTime(dtFrom.Value).Date : new DateTime(DateTime.Today.Year, 1, 1);
            DateTime to = dtTo.Value != null ? Convert.ToDateTime(dtTo.Value).Date : DateTime.Today;
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

        private void PopulateDashboardData()
        {
            pnlScrollableContent.SuspendLayout();
            pnlScrollableContent.Controls.Clear();

            int containerWidth = Math.Max(980, pnlScrollableContent.ClientSize.Width - 40);
            int currentY = 12;

            // ═══════════════════════════════════════════════════════════════════
            // EXECUTIVE SUMMARY HIGHLIGHT BANNER (Hero KPI Cards)
            // ═══════════════════════════════════════════════════════════════════
            var heroFlow = CreateHeroCardsFlow(containerWidth);
            decimal totalLiquidity = _model.CashInHand + _model.BankBalance;
            
            heroFlow.Controls.Add(CreateHeroCard(
                "Net Business Asset (NBA)",
                FormatCurr(_model.NetBusinessAsset),
                $"Total Assets: {FormatCurr(_model.TotalAssets)} | Liab: {FormatCurr(_model.TotalLiabilities)}",
                Color.FromArgb(16, 85, 154),
                () => DrillDown("BalanceSheet")));

            heroFlow.Controls.Add(CreateHeroCard(
                "Total Sales Revenue",
                FormatCurr(_model.TotalSalesRevenue),
                $"Purchases: {FormatCurr(_model.TotalPurchases)} | Bills: {_model.TotalSalesBillCount:N0}",
                Color.FromArgb(30, 136, 229),
                () => DrillDown("SalesAnalytics")));

            heroFlow.Controls.Add(CreateHeroCard(
                "Actual Net Profit",
                FormatCurr(_model.ActualNetProfit),
                $"Operating Margin: {_model.OperatingProfitMarginPercent:N2}% | Gross: {FormatCurr(_model.GrossProfit)}",
                _model.ActualNetProfit >= 0 ? Color.FromArgb(46, 125, 50) : Color.FromArgb(198, 40, 40),
                () => DrillDown("TradingPL")));

            heroFlow.Controls.Add(CreateHeroCard(
                "Total Cash & Bank Liquidity",
                FormatCurr(totalLiquidity),
                $"Cash: {FormatCurr(_model.CashInHand)} | Bank: {FormatCurr(_model.BankBalance)}",
                Color.FromArgb(0, 137, 123),
                () => DrillDown("CashBankBook")));

            heroFlow.Location = new Point(16, currentY);
            pnlScrollableContent.Controls.Add(heroFlow);
            currentY += heroFlow.PreferredSize.Height + 18;

            // ═══════════════════════════════════════════════════════════════════
            // SECTION 1: INVENTORY VALUATION & STOCK HEALTH (1, 2, 5, 6, 14, 15, 16, 20, 21, 22)
            // ═══════════════════════════════════════════════════════════════════
            var header1 = CreateSectionHeader("1. Inventory Valuation & Stock Health", "Real-time stock valuation, profit potential, damage discrepancies, and inventory alert controls", containerWidth);
            header1.Location = new Point(16, currentY);
            pnlScrollableContent.Controls.Add(header1);
            currentY += header1.Height + 6;

            var flow1 = CreateCardsFlow(containerWidth);
            flow1.Controls.Add(CreateKpiCard("1. Total Stock Value", "Total warehouse stock valued at purchase cost", FormatCurr(_model.TotalStockCostValue), $"Items: {_model.TotalStockItemCount:N0} | Units: {_model.TotalStockQuantity:N0}", Color.FromArgb(41, 128, 185), () => DrillDown("StockValuation")));
            flow1.Controls.Add(CreateKpiCard("2. Stock Profit Potential", "Estimated gross profit on 100% stock liquidation", FormatCurr(_model.StockProfitPotential), $"Retail Value: {FormatCurr(_model.TotalStockRetailValue)}", Color.FromArgb(39, 174, 96), () => DrillDown("StockValuation")));
            flow1.Controls.Add(CreateKpiCard("5. Negative Stock Impact", "Estimated financial distortion due to negative stock at cost", FormatCurr(_model.NegativeStockImpactValue), "Cost Value of Negative Items", Color.FromArgb(231, 76, 60), () => DrillDown("StockValuation")));
            flow1.Controls.Add(CreateKpiCard("6. Negative Stock Items", "Products currently running in negative inventory balance", $"{_model.NegativeStockItemCount} Items", $"Total Negative Qty: {_model.NegativeStockTotalQty:N0}", Color.FromArgb(192, 57, 43), () => DrillDown("StockAnalytics")));
            flow1.Controls.Add(CreateKpiCard("14. Loss Stock (Damaged / Out)", "Total stock written off or damaged (Stock OUT adjustments)", FormatCurr(_model.LossStockValue), $"Discrepancy Qty: {_model.LossStockQty:N2}", Color.FromArgb(211, 84, 0), () => DrillDown("StockAdjustment")));
            flow1.Controls.Add(CreateKpiCard("15. Extra Stock (Found / In)", "Total surplus stock found and added (Stock IN adjustments)", FormatCurr(_model.ExtraStockValue), $"Surplus Qty: {_model.ExtraStockQty:N2}", Color.FromArgb(22, 160, 133), () => DrillDown("StockAdjustment")));
            flow1.Controls.Add(CreateKpiCard("16. Net Stock Adjustment", "Net variance between stock additions and write-offs", FormatCurr(_model.NetStockAdjustmentValue), _model.NetStockAdjustmentValue >= 0 ? "Surplus Net Balance" : "Shortage Net Variance", Color.FromArgb(142, 68, 173), () => DrillDown("StockAdjustment")));
            flow1.Controls.Add(CreateKpiCard("20. Excess Stock Alert", "Products exceeding maximum inventory threshold", $"{_model.ExcessStockAlertCount} Items", "Over Maximum Limit", Color.FromArgb(243, 156, 18), () => DrillDown("SmartReorder")));
            flow1.Controls.Add(CreateKpiCard("21. Low Stock Alert", "Products below minimum critical inventory threshold", $"{_model.LowStockAlertCount} Items", "Below Minimum Limit", Color.FromArgb(230, 126, 34), () => DrillDown("LowStockAlert")));
            flow1.Controls.Add(CreateKpiCard("22. Reorder Alert", "Products at or below reorder level requiring purchase", $"{_model.ReorderAlertCount} Items", "Purchase Order Recommended", Color.FromArgb(192, 57, 43), () => DrillDown("SmartReorder")));
            flow1.Location = new Point(16, currentY);
            pnlScrollableContent.Controls.Add(flow1);
            currentY += flow1.PreferredSize.Height + 18;

            // ═══════════════════════════════════════════════════════════════════
            // SECTION 2: WORKING CAPITAL, LIQUIDITY & BALANCES (7, 8, 9, 10, 13, 29, 30)
            // ═══════════════════════════════════════════════════════════════════
            var header2 = CreateSectionHeader("2. Working Capital, Liquidity & Outstanding Balances", "Liquid funds, customer receivables, supplier payables, advance deposits, and net business valuation", containerWidth);
            header2.Location = new Point(16, currentY);
            pnlScrollableContent.Controls.Add(header2);
            currentY += header2.Height + 6;

            var flow2 = CreateCardsFlow(containerWidth);
            flow2.Controls.Add(CreateKpiCard("7. Cash in Hand", "Total liquid cash in cash drawers, counter tills, and main safe", FormatCurr(_model.CashInHand), "Available Counter Cash", Color.FromArgb(46, 204, 113), () => DrillDown("CashBankBook")));
            flow2.Controls.Add(CreateKpiCard("8. Bank Balance", "Total combined active bank account balances", FormatCurr(_model.BankBalance), "Active Commercial Bank Accounts", Color.FromArgb(52, 152, 219), () => DrillDown("BankStatement")));
            flow2.Controls.Add(CreateKpiCard("9. Supplier Payables", "Total outstanding liabilities owed to vendors and suppliers", FormatCurr(_model.SupplierPayables), $"{_model.SupplierPayablesCount} Pending Purchase Bills", Color.FromArgb(231, 76, 60), () => DrillDown("VendorOutstanding")));
            flow2.Controls.Add(CreateKpiCard("10. Customer Receivables", "Total outstanding dues to collect from customers", FormatCurr(_model.CustomerReceivables), $"{_model.CustomerReceivablesCount} Unpaid Sales Invoices", Color.FromArgb(241, 196, 15), () => DrillDown("CustomerOutstanding")));
            flow2.Controls.Add(CreateKpiCard("13. Net Business Asset (NBA)", "True net worth of the business (Total Assets minus Liabilities)", FormatCurr(_model.NetBusinessAsset), $"Assets: {FormatCurr(_model.TotalAssets)} | Liab: {FormatCurr(_model.TotalLiabilities)}", Color.FromArgb(31, 78, 121), () => DrillDown("BalanceSheet")));
            flow2.Controls.Add(CreateKpiCard("29. Supplier Advance / Overpayment", "Advance payments and debit balances with vendors", FormatCurr(_model.SupplierAdvanceBalance), "Advance Payments / Debit", Color.FromArgb(52, 73, 94), () => DrillDown("VendorPaymentReport")));
            flow2.Controls.Add(CreateKpiCard("30. Customer Advance / Deposits", "Customer advance payments and credit deposits received", FormatCurr(_model.CustomerAdvanceBalance), "Customer Deposits / Credit", Color.FromArgb(127, 140, 141), () => DrillDown("CustomerReceiptReport")));
            flow2.Location = new Point(16, currentY);
            pnlScrollableContent.Controls.Add(flow2);
            currentY += flow2.PreferredSize.Height + 18;

            // ═══════════════════════════════════════════════════════════════════
            // SECTION 3: REVENUE, EXPENSES & PROFITABILITY (3, 4, 11, 12, 19, 23, 24)
            // ═══════════════════════════════════════════════════════════════════
            var header3 = CreateSectionHeader("3. Sales Revenue, Expenses & Profitability", "Operating performance, direct/indirect expenses, profit margins, capital drawings, and write-offs", containerWidth);
            header3.Location = new Point(16, currentY);
            pnlScrollableContent.Controls.Add(header3);
            currentY += header3.Height + 6;

            var flow3 = CreateCardsFlow(containerWidth);
            flow3.Controls.Add(CreateKpiCard("3. Total Sales Revenue", "Total net billing revenue for the selected date period", FormatCurr(_model.TotalSalesRevenue), $"{_model.TotalSalesBillCount:N0} Invoices | Pur: {FormatCurr(_model.TotalPurchases)}", Color.FromArgb(41, 128, 185), () => DrillDown("SalesAnalytics")));
            flow3.Controls.Add(CreateKpiCard("4. Total Business Expenses", "Combined operational overheads, utilities, and running costs", FormatCurr(_model.TotalBusinessExpenses), $"Direct: {FormatCurr(_model.DirectExpenses)} | Indir: {FormatCurr(_model.IndirectExpenses)}", Color.FromArgb(192, 57, 43), () => DrillDown("ProfitLoss")));
            flow3.Controls.Add(CreateKpiCard("11. Actual Net Profit", "Bottom-line net earnings after deducting cost of sales and expenses", FormatCurr(_model.ActualNetProfit), $"Gross Profit: {FormatCurr(_model.GrossProfit)}", Color.FromArgb(39, 174, 96), () => DrillDown("TradingPL")));
            flow3.Controls.Add(CreateKpiCard("19. Operating Profit Margin %", "Ratio of net profit generated from sales turnover", $"{_model.OperatingProfitMarginPercent:N2}%", "Net Profit / Sales Revenue", Color.FromArgb(22, 160, 133), () => DrillDown("TradingPL")));
            flow3.Controls.Add(CreateKpiCard("12. Owner Drawings", "Capital withdrawals and personal drawings taken by proprietors", FormatCurr(_model.OwnerDrawings), "Capital Account Withdrawals", Color.FromArgb(155, 89, 182), () => DrillDown("DayBook")));
            flow3.Controls.Add(CreateKpiCard("23. Customer Bad Debts", "Uncollectible customer credit balances written off", FormatCurr(_model.CustomerBadDebts), "Credit Loss Write-Offs", Color.FromArgb(149, 165, 166), () => DrillDown("CustomerOutstanding")));
            flow3.Controls.Add(CreateKpiCard("24. Supplier Write-Offs", "Vendor invoice write-offs and negotiated settlement discounts", FormatCurr(_model.SupplierWriteOffs), "Discounts / Write-Off Settled", Color.FromArgb(127, 140, 141), () => DrillDown("VendorOutstanding")));
            flow3.Location = new Point(16, currentY);
            pnlScrollableContent.Controls.Add(flow3);
            currentY += flow3.PreferredSize.Height + 18;

            // ═══════════════════════════════════════════════════════════════════
            // SECTION 4: CREDIT RISK & AGING ANALYSIS (17, 18)
            // ═══════════════════════════════════════════════════════════════════
            var header4 = CreateSectionHeader("4. Credit Risk & Aging Analysis", "Delayed and overdue balances exceeding standard 30-day settlement window", containerWidth);
            header4.Location = new Point(16, currentY);
            pnlScrollableContent.Controls.Add(header4);
            currentY += header4.Height + 6;

            var flow4 = CreateCardsFlow(containerWidth);
            flow4.Controls.Add(CreateKpiCard("17. Delayed Customer Receivables (>30 Days)", "Overdue customer receivables past 30 days credit period", FormatCurr(_model.DelayedCustomerReceivables30Days), $"{_model.DelayedCustomerCount30Days} High-Risk Invoices", Color.FromArgb(231, 76, 60), () => DrillDown("CustomerOutstanding")));
            flow4.Controls.Add(CreateKpiCard("18. Delayed Supplier Payables (>30 Days)", "Overdue purchase invoices past 30 days credit period", FormatCurr(_model.DelayedSupplierPayables30Days), $"{_model.DelayedSupplierCount30Days} Overdue Vendor Bills", Color.FromArgb(230, 126, 34), () => DrillDown("VendorOutstanding")));
            flow4.Location = new Point(16, currentY);
            pnlScrollableContent.Controls.Add(flow4);
            currentY += flow4.PreferredSize.Height + 18;

            // ═══════════════════════════════════════════════════════════════════
            // SECTION 5: AUDIT TRAIL & INTERNAL CONTROLS (26, 27, 28)
            // ═══════════════════════════════════════════════════════════════════
            var header5 = CreateSectionHeader("5. Internal Control & Security Audit Trail", "Log of deletions, master price changes, and physical stock discrepancy adjustments", containerWidth);
            header5.Location = new Point(16, currentY);
            pnlScrollableContent.Controls.Add(header5);
            currentY += header5.Height + 6;

            var flow5Audit = CreateCardsFlow(containerWidth);
            flow5Audit.Controls.Add(CreateKpiCard("26. Deletion & Cancellation Logs", "Track of deleted sales bills, purchase bills, and account vouchers", $"{_model.DeletionCount} Events", "Cancelled / Deleted Documents", Color.FromArgb(192, 57, 43), () => DrillDown("AuditDeletions")));
            flow5Audit.Controls.Add(CreateKpiCard("27. Price Change Modifications", "Track of item selling and cost price edits in Item Master", $"{_model.PriceChangeCount} Events", "Item Master Price Modifications", Color.FromArgb(41, 128, 185), () => DrillDown("AuditPriceChanges")));
            flow5Audit.Controls.Add(CreateKpiCard("28. Physical Stock Discrepancy Logs", "Track of physical stock count reconciliations and adjustments", $"{_model.StockAdjustmentCount} Adjustments", "Stock Adjustment Entries", Color.FromArgb(142, 68, 173), () => DrillDown("StockAdjustment")));
            flow5Audit.Location = new Point(16, currentY);
            pnlScrollableContent.Controls.Add(flow5Audit);
            currentY += flow5Audit.PreferredSize.Height + 18;

            // ═══════════════════════════════════════════════════════════════════
            // SECTION 6: MONTHLY GROWTH & PERFORMANCE MATRIX (25)
            // ═══════════════════════════════════════════════════════════════════
            var header6 = CreateSectionHeader("6. Monthly Summary & Performance Growth Matrix (MoM %)", "Month-over-month sales trends, purchase volume, operational expenses, and net profit growth %", containerWidth);
            header6.Location = new Point(16, currentY);
            pnlScrollableContent.Controls.Add(header6);
            currentY += header6.Height + 8;

            var dgvGrowth = CreateGrowthMatrixGrid(containerWidth);
            dgvGrowth.Location = new Point(16, currentY);
            pnlScrollableContent.Controls.Add(dgvGrowth);
            currentY += dgvGrowth.Height + 24;

            var pnlSpacer = new Panel
            {
                Location = new Point(16, currentY),
                Size = new Size(containerWidth, 24),
                BackColor = Color.Transparent
            };
            pnlScrollableContent.Controls.Add(pnlSpacer);

            pnlScrollableContent.ResumeLayout(true);
        }

        private Panel CreateSectionHeader(string title, string subtitle, int width)
        {
            var pnl = new Panel
            {
                Width = width,
                Height = 36,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = SectionHeaderColor,
                Location = new Point(0, 0),
                AutoSize = true
            };
            pnl.Controls.Add(lblTitle);

            var lblSub = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 8F),
                ForeColor = SectionSubColor,
                Location = new Point(0, 20),
                AutoSize = true
            };
            pnl.Controls.Add(lblSub);

            return pnl;
        }

        private FlowLayoutPanel CreateHeroCardsFlow(int width)
        {
            return new FlowLayoutPanel
            {
                Width = width,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
        }

        private Panel CreateHeroCard(string title, string mainValue, string subtitle, Color accentColor, Action onClick)
        {
            int cardWidth = 236;
            var card = new Panel
            {
                Width = cardWidth,
                Height = 104,
                BackColor = CardBgColor,
                Margin = new Padding(0, 0, 10, 10),
                Cursor = Cursors.Hand
            };

            var leftBar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 5,
                BackColor = accentColor
            };
            card.Controls.Add(leftBar);

            var lblTitle = new Label
            {
                Text = title.ToUpperInvariant(),
                Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(70, 90, 120),
                Location = new Point(14, 10),
                Size = new Size(cardWidth - 24, 16),
                AutoEllipsis = true
            };
            card.Controls.Add(lblTitle);

            var lblValue = new Label
            {
                Text = mainValue,
                Font = new Font("Segoe UI", 13.5F, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new Point(13, 30),
                Size = new Size(cardWidth - 24, 30),
                AutoEllipsis = true
            };
            card.Controls.Add(lblValue);

            var lblSub = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = Color.FromArgb(100, 120, 145),
                Location = new Point(14, 66),
                Size = new Size(cardWidth - 24, 28),
                AutoEllipsis = true
            };
            card.Controls.Add(lblSub);

            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(CardBorderColor, 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            // Hover effect
            card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(245, 250, 255);
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

        private FlowLayoutPanel CreateCardsFlow(int width)
        {
            return new FlowLayoutPanel
            {
                Width = width,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
        }

        private Panel CreateKpiCard(string title, string description, string mainValue, string footerInfo, Color accentColor, Action onClick)
        {
            int cardWidth = 236;
            var card = new Panel
            {
                Width = cardWidth,
                Height = 112,
                BackColor = CardBgColor,
                Margin = new Padding(0, 0, 10, 10),
                Cursor = Cursors.Hand
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
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 48, 80),
                Location = new Point(10, 9),
                Size = new Size(cardWidth - 20, 18),
                AutoEllipsis = true
            };
            card.Controls.Add(lblTitle);

            var lblDesc = new Label
            {
                Text = description,
                Font = new Font("Segoe UI", 7F),
                ForeColor = Color.FromArgb(120, 140, 165),
                Location = new Point(10, 28),
                Size = new Size(cardWidth - 20, 14),
                AutoEllipsis = true
            };
            card.Controls.Add(lblDesc);

            var lblValue = new Label
            {
                Text = mainValue,
                Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new Point(9, 46),
                Size = new Size(cardWidth - 20, 28),
                AutoEllipsis = true
            };
            card.Controls.Add(lblValue);

            var pnlFooter = new Panel
            {
                Location = new Point(10, 78),
                Size = new Size(cardWidth - 20, 26),
                BackColor = Color.FromArgb(248, 250, 253)
            };
            card.Controls.Add(pnlFooter);

            var lblFooter = new Label
            {
                Text = footerInfo,
                Font = new Font("Segoe UI Semibold", 7.5F),
                ForeColor = Color.FromArgb(70, 95, 125),
                Location = new Point(4, 5),
                Size = new Size(pnlFooter.Width - 8, 16),
                AutoEllipsis = true
            };
            pnlFooter.Controls.Add(lblFooter);

            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(CardBorderColor, 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            // Hover effect
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

        private DataGridView CreateGrowthMatrixGrid(int width)
        {
            var grid = new DataGridView
            {
                Width = width,
                Height = 220,
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
                Margin = new Padding(0)
            };

            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(28, 70, 130);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            grid.ColumnHeadersHeight = 32;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            grid.DefaultCellStyle.Font = new Font("Segoe UI", 8.5F);
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(20, 40, 70);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 235, 255);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(10, 30, 60);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 251, 255);
            grid.RowTemplate.Height = 26;

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
                    sCell.Style.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);

                    var pCell = grid.Rows[rIdx].Cells["ProfitGrowth"];
                    pCell.Style.ForeColor = row.NetProfitGrowthPercent >= 0 ? Color.FromArgb(46, 125, 50) : Color.FromArgb(198, 40, 40);
                    pCell.Style.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);

                    var statusCell = grid.Rows[rIdx].Cells["Status"];
                    statusCell.Style.ForeColor = row.NetProfitAmount >= 0 ? Color.FromArgb(46, 125, 50) : Color.FromArgb(198, 40, 40);
                    statusCell.Style.Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold);
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
                    case "AuditPriceChanges":
                        formToOpen = new Reports.AuditReport.frmAuditReport();
                        title = "Audit Trail Report";
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
                    sfd.Filter = "HTML Document (*.html)|*.html|CSV Document (*.csv)|*.csv";
                    sfd.FileName = $"Executive_KPI_Summary_{DateTime.Now:yyyyMMdd_HHmm}.html";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        if (sfd.FilterIndex == 2)
                        {
                            ExportCsv(sfd.FileName);
                        }
                        else
                        {
                            ExportHtml(sfd.FileName);
                        }
                        MessageBox.Show("Executive Summary Report exported successfully!", "Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportHtml(string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'><title>Executive Business KPI Report</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: 'Segoe UI', -apple-system, BlinkMacSystemFont, Arial, sans-serif; background: #f8fafc; color: #0f172a; padding: 28px; line-height: 1.5; }");
            sb.AppendLine(".container { max-width: 1200px; margin: 0 auto; background: #ffffff; padding: 32px; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.06); }");
            sb.AppendLine("h1 { color: #0f274a; margin: 0 0 6px 0; font-size: 24px; font-weight: 700; }");
            sb.AppendLine(".meta { color: #64748b; font-size: 13px; margin-bottom: 24px; border-bottom: 1px solid #e2e8f0; padding-bottom: 12px; }");
            sb.AppendLine("h2 { color: #1e3a8a; font-size: 16px; margin-top: 28px; margin-bottom: 10px; border-bottom: 2px solid #3b82f6; padding-bottom: 6px; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 8px; margin-bottom: 20px; font-size: 13px; }");
            sb.AppendLine("th, td { border: 1px solid #e2e8f0; padding: 9px 12px; text-align: left; }");
            sb.AppendLine("th { background: #1e40af; color: #ffffff; font-weight: 600; font-size: 13px; }");
            sb.AppendLine("tr:nth-child(even) { background: #f8fafc; }");
            sb.AppendLine(".value { font-weight: 700; color: #0369a1; text-align: right; }");
            sb.AppendLine(".positive { color: #15803d; font-weight: 600; text-align: right; }");
            sb.AppendLine(".negative { color: #b91c1c; font-weight: 600; text-align: right; }");
            sb.AppendLine(".hero-box { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; margin-bottom: 24px; }");
            sb.AppendLine(".hero-card { background: #f0f7ff; border-left: 4px solid #0284c7; padding: 14px; border-radius: 4px; }");
            sb.AppendLine(".hero-title { font-size: 11px; text-transform: uppercase; color: #64748b; font-weight: 600; }");
            sb.AppendLine(".hero-val { font-size: 20px; font-weight: 700; color: #0369a1; margin: 4px 0; }");
            sb.AppendLine(".hero-note { font-size: 11px; color: #475569; }");
            sb.AppendLine("</style></head><body><div class='container'>");

            sb.AppendLine("<h1>Executive Business KPI Cockpit — 30 Key Metrics</h1>");
            sb.AppendLine($"<div class='meta'>Period: <b>{_model.FromDate:dd-MMM-yyyy}</b> to <b>{_model.ToDate:dd-MMM-yyyy}</b> | Generated: <b>{_model.GeneratedAt:dd-MMM-yyyy hh:mm tt}</b></div>");

            decimal totalLiquidity = _model.CashInHand + _model.BankBalance;
            sb.AppendLine("<div class='hero-box'>");
            sb.AppendLine($"<div class='hero-card'><div class='hero-title'>Net Business Asset</div><div class='hero-val'>{FormatCurr(_model.NetBusinessAsset)}</div><div class='hero-note'>Assets: {FormatCurr(_model.TotalAssets)}</div></div>");
            sb.AppendLine($"<div class='hero-card' style='border-left-color:#2563eb;'><div class='hero-title'>Sales Revenue</div><div class='hero-val'>{FormatCurr(_model.TotalSalesRevenue)}</div><div class='hero-note'>{_model.TotalSalesBillCount:N0} Invoices</div></div>");
            sb.AppendLine($"<div class='hero-card' style='border-left-color:#16a34a;'><div class='hero-title'>Actual Net Profit</div><div class='hero-val'>{FormatCurr(_model.ActualNetProfit)}</div><div class='hero-note'>Margin: {_model.OperatingProfitMarginPercent:N2}%</div></div>");
            sb.AppendLine($"<div class='hero-card' style='border-left-color:#0d9488;'><div class='hero-title'>Liquid Cash & Bank</div><div class='hero-val'>{FormatCurr(totalLiquidity)}</div><div class='hero-note'>Cash: {FormatCurr(_model.CashInHand)}</div></div>");
            sb.AppendLine("</div>");

            sb.AppendLine("<h2>1. Inventory Valuation & Stock Health</h2><table><tr><th style='width:50px;'>#</th><th>Metric Name</th><th style='text-align:right;'>Value</th><th>Business Context / Notes</th></tr>");
            sb.AppendLine($"<tr><td>1</td><td>Total Stock Value</td><td class='value'>{FormatCurr(_model.TotalStockCostValue)}</td><td>Valued at Cost Price ({_model.TotalStockItemCount:N0} Items, {_model.TotalStockQuantity:N0} Units)</td></tr>");
            sb.AppendLine($"<tr><td>2</td><td>Stock Profit Potential</td><td class='value'>{FormatCurr(_model.StockProfitPotential)}</td><td>Retail Value: {FormatCurr(_model.TotalStockRetailValue)}</td></tr>");
            sb.AppendLine($"<tr><td>5</td><td>Negative Stock Impact</td><td class='value'>{FormatCurr(_model.NegativeStockImpactValue)}</td><td>Cost value distortion from negative stock</td></tr>");
            sb.AppendLine($"<tr><td>6</td><td>Negative Stock Items</td><td class='value'>{_model.NegativeStockItemCount} Items</td><td>Total Negative Quantity: {_model.NegativeStockTotalQty:N0}</td></tr>");
            sb.AppendLine($"<tr><td>14</td><td>Loss Stock (Damaged / Out)</td><td class='value'>{FormatCurr(_model.LossStockValue)}</td><td>Damaged & Written Off ({_model.LossStockQty:N2} Qty)</td></tr>");
            sb.AppendLine($"<tr><td>15</td><td>Extra Stock (Found / In)</td><td class='value'>{FormatCurr(_model.ExtraStockValue)}</td><td>Surplus Stock Added ({_model.ExtraStockQty:N2} Qty)</td></tr>");
            sb.AppendLine($"<tr><td>16</td><td>Net Stock Adjustment</td><td class='value'>{FormatCurr(_model.NetStockAdjustmentValue)}</td><td>Net Discrepancy Balance</td></tr>");
            sb.AppendLine($"<tr><td>20</td><td>Excess Stock Alert</td><td class='value'>{_model.ExcessStockAlertCount} Items</td><td>Items above maximum inventory limits</td></tr>");
            sb.AppendLine($"<tr><td>21</td><td>Low Stock Alert</td><td class='value'>{_model.LowStockAlertCount} Items</td><td>Items below minimum safety stock</td></tr>");
            sb.AppendLine($"<tr><td>22</td><td>Reorder Alert</td><td class='value'>{_model.ReorderAlertCount} Items</td><td>Items at or below reorder level</td></tr>");
            sb.AppendLine("</table>");

            sb.AppendLine("<h2>2. Working Capital, Liquidity & Outstanding Balances</h2><table><tr><th style='width:50px;'>#</th><th>Metric Name</th><th style='text-align:right;'>Value</th><th>Business Context / Notes</th></tr>");
            sb.AppendLine($"<tr><td>7</td><td>Cash in Hand</td><td class='value'>{FormatCurr(_model.CashInHand)}</td><td>Liquid cash available in tills and safe</td></tr>");
            sb.AppendLine($"<tr><td>8</td><td>Bank Balance</td><td class='value'>{FormatCurr(_model.BankBalance)}</td><td>Active commercial bank accounts balance</td></tr>");
            sb.AppendLine($"<tr><td>9</td><td>Supplier Payables</td><td class='value'>{FormatCurr(_model.SupplierPayables)}</td><td>Owed to vendors across {_model.SupplierPayablesCount} bills</td></tr>");
            sb.AppendLine($"<tr><td>10</td><td>Customer Receivables</td><td class='value'>{FormatCurr(_model.CustomerReceivables)}</td><td>Outstanding dues from {_model.CustomerReceivablesCount} customer invoices</td></tr>");
            sb.AppendLine($"<tr><td>13</td><td>Net Business Asset (NBA)</td><td class='value'>{FormatCurr(_model.NetBusinessAsset)}</td><td>Total Assets: {FormatCurr(_model.TotalAssets)} | Liabilities: {FormatCurr(_model.TotalLiabilities)}</td></tr>");
            sb.AppendLine($"<tr><td>29</td><td>Supplier Advance / Overpayment</td><td class='value'>{FormatCurr(_model.SupplierAdvanceBalance)}</td><td>Advance payments & vendor debit balances</td></tr>");
            sb.AppendLine($"<tr><td>30</td><td>Customer Advance / Deposits</td><td class='value'>{FormatCurr(_model.CustomerAdvanceBalance)}</td><td>Advance deposits & customer credit balances</td></tr>");
            sb.AppendLine("</table>");

            sb.AppendLine("<h2>3. Sales Revenue, Expenses & Profitability</h2><table><tr><th style='width:50px;'>#</th><th>Metric Name</th><th style='text-align:right;'>Value</th><th>Business Context / Notes</th></tr>");
            sb.AppendLine($"<tr><td>3</td><td>Total Sales Revenue</td><td class='value'>{FormatCurr(_model.TotalSalesRevenue)}</td><td>{_model.TotalSalesBillCount:N0} Invoices (Purchases: {FormatCurr(_model.TotalPurchases)})</td></tr>");
            sb.AppendLine($"<tr><td>4</td><td>Total Business Expenses</td><td class='value'>{FormatCurr(_model.TotalBusinessExpenses)}</td><td>Direct: {FormatCurr(_model.DirectExpenses)} | Indirect: {FormatCurr(_model.IndirectExpenses)}</td></tr>");
            sb.AppendLine($"<tr><td>11</td><td>Actual Net Profit</td><td class='value'>{FormatCurr(_model.ActualNetProfit)}</td><td>Gross Profit: {FormatCurr(_model.GrossProfit)}</td></tr>");
            sb.AppendLine($"<tr><td>19</td><td>Operating Profit Margin %</td><td class='value'>{_model.OperatingProfitMarginPercent:N2}%</td><td>Net Profit / Sales Turnover</td></tr>");
            sb.AppendLine($"<tr><td>12</td><td>Owner Drawings</td><td class='value'>{FormatCurr(_model.OwnerDrawings)}</td><td>Proprietor capital withdrawals</td></tr>");
            sb.AppendLine($"<tr><td>23</td><td>Customer Bad Debts</td><td class='value'>{FormatCurr(_model.CustomerBadDebts)}</td><td>Uncollectible receivables written off</td></tr>");
            sb.AppendLine($"<tr><td>24</td><td>Supplier Write-Offs</td><td class='value'>{FormatCurr(_model.SupplierWriteOffs)}</td><td>Vendor settlement discounts / write-offs</td></tr>");
            sb.AppendLine("</table>");

            sb.AppendLine("<h2>4. Credit Risk & Aging Analysis</h2><table><tr><th style='width:50px;'>#</th><th>Metric Name</th><th style='text-align:right;'>Value</th><th>Business Context / Notes</th></tr>");
            sb.AppendLine($"<tr><td>17</td><td>Delayed Customer Receivables (>30 Days)</td><td class='value'>{FormatCurr(_model.DelayedCustomerReceivables30Days)}</td><td>{_model.DelayedCustomerCount30Days} High-Risk Invoices overdue</td></tr>");
            sb.AppendLine($"<tr><td>18</td><td>Delayed Supplier Payables (>30 Days)</td><td class='value'>{FormatCurr(_model.DelayedSupplierPayables30Days)}</td><td>{_model.DelayedSupplierCount30Days} Vendor Invoices overdue</td></tr>");
            sb.AppendLine("</table>");

            sb.AppendLine("<h2>5. Internal Control & Security Audit Trail</h2><table><tr><th style='width:50px;'>#</th><th>Metric Name</th><th style='text-align:right;'>Value</th><th>Business Context / Notes</th></tr>");
            sb.AppendLine($"<tr><td>26</td><td>Deletion & Cancellation Logs</td><td class='value'>{_model.DeletionCount} Events</td><td>Cancelled bills, deleted receipts & vouchers</td></tr>");
            sb.AppendLine($"<tr><td>27</td><td>Price Change Modifications</td><td class='value'>{_model.PriceChangeCount} Events</td><td>Item Master price updates</td></tr>");
            sb.AppendLine($"<tr><td>28</td><td>Physical Stock Discrepancy Logs</td><td class='value'>{_model.StockAdjustmentCount} Adjustments</td><td>Stock reconciliation entries</td></tr>");
            sb.AppendLine("</table>");

            sb.AppendLine("<h2>6. Monthly Summary & Performance Growth Matrix</h2><table><tr><th>Month</th><th style='text-align:right;'>Sales Turnover</th><th style='text-align:right;'>Sales MoM %</th><th style='text-align:right;'>Purchases</th><th style='text-align:right;'>Expenses</th><th style='text-align:right;'>Net Profit</th><th style='text-align:right;'>Profit MoM %</th><th>Status</th></tr>");
            if (_model.MonthlyGrowthMatrix != null && _model.MonthlyGrowthMatrix.Count > 0)
            {
                foreach (var m in _model.MonthlyGrowthMatrix)
                {
                    string sColor = m.SalesGrowthPercent >= 0 ? "positive" : "negative";
                    string pColor = m.NetProfitGrowthPercent >= 0 ? "positive" : "negative";
                    string status = m.NetProfitAmount >= 0 ? "<span style='color:#16a34a;font-weight:600;'>Profitable</span>" : "<span style='color:#dc2626;font-weight:600;'>Deficit</span>";
                    sb.AppendLine($"<tr><td><b>{m.MonthName}</b></td><td style='text-align:right;'>{FormatCurr(m.SalesAmount)}</td><td class='{sColor}'>{(m.SalesGrowthPercent >= 0 ? "+" : "")}{m.SalesGrowthPercent:N1}%</td><td style='text-align:right;'>{FormatCurr(m.PurchaseAmount)}</td><td style='text-align:right;'>{FormatCurr(m.ExpenseAmount)}</td><td style='text-align:right;'>{FormatCurr(m.NetProfitAmount)}</td><td class='{pColor}'>{(m.NetProfitGrowthPercent >= 0 ? "+" : "")}{m.NetProfitGrowthPercent:N1}%</td><td>{status}</td></tr>");
                }
            }
            sb.AppendLine("</table>");

            sb.AppendLine("</div></body></html>");
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private void ExportCsv(string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Metric Number,Category,Metric Name,Value,Notes");
            sb.AppendLine($"1,Inventory,Total Stock Value,\"{_model.TotalStockCostValue:N2}\",At Cost Price");
            sb.AppendLine($"2,Inventory,Stock Profit Potential,\"{_model.StockProfitPotential:N2}\",Retail Value: {_model.TotalStockRetailValue:N2}");
            sb.AppendLine($"5,Inventory,Negative Stock Impact,\"{_model.NegativeStockImpactValue:N2}\",Cost Value");
            sb.AppendLine($"6,Inventory,Negative Stock Items,\"{_model.NegativeStockItemCount}\",Total Qty: {_model.NegativeStockTotalQty:N0}");
            sb.AppendLine($"14,Inventory,Loss Stock (Damaged / Out),\"{_model.LossStockValue:N2}\",Discrepancy Qty: {_model.LossStockQty:N2}");
            sb.AppendLine($"15,Inventory,Extra Stock (Found / In),\"{_model.ExtraStockValue:N2}\",Surplus Qty: {_model.ExtraStockQty:N2}");
            sb.AppendLine($"16,Inventory,Net Stock Adjustment,\"{_model.NetStockAdjustmentValue:N2}\",Net Variance");
            sb.AppendLine($"20,Inventory,Excess Stock Alert,\"{_model.ExcessStockAlertCount}\",Over Max Limit");
            sb.AppendLine($"21,Inventory,Low Stock Alert,\"{_model.LowStockAlertCount}\",Below Min Limit");
            sb.AppendLine($"22,Inventory,Reorder Alert,\"{_model.ReorderAlertCount}\",Reorder Needed");
            sb.AppendLine($"7,Liquidity,Cash in Hand,\"{_model.CashInHand:N2}\",Counter & Safe Cash");
            sb.AppendLine($"8,Liquidity,Bank Balance,\"{_model.BankBalance:N2}\",Active Bank Accounts");
            sb.AppendLine($"9,Payables,Supplier Payables,\"{_model.SupplierPayables:N2}\",Bills Count: {_model.SupplierPayablesCount}");
            sb.AppendLine($"10,Receivables,Customer Receivables,\"{_model.CustomerReceivables:N2}\",Invoices Count: {_model.CustomerReceivablesCount}");
            sb.AppendLine($"13,Net Worth,Net Business Asset (NBA),\"{_model.NetBusinessAsset:N2}\",Assets: {_model.TotalAssets:N2} | Liab: {_model.TotalLiabilities:N2}");
            sb.AppendLine($"29,Advance,Supplier Advance / Overpayment,\"{_model.SupplierAdvanceBalance:N2}\",Debit Balances");
            sb.AppendLine($"30,Advance,Customer Advance / Deposits,\"{_model.CustomerAdvanceBalance:N2}\",Credit Balances");
            sb.AppendLine($"3,Revenue,Total Sales Revenue,\"{_model.TotalSalesRevenue:N2}\",Bills: {_model.TotalSalesBillCount}");
            sb.AppendLine($"4,Expenses,Total Business Expenses,\"{_model.TotalBusinessExpenses:N2}\",Direct: {_model.DirectExpenses:N2} | Indir: {_model.IndirectExpenses:N2}");
            sb.AppendLine($"11,Profitability,Actual Net Profit,\"{_model.ActualNetProfit:N2}\",Gross: {_model.GrossProfit:N2}");
            sb.AppendLine($"19,Profitability,Operating Profit Margin %,\"{_model.OperatingProfitMarginPercent:N2}%\",Net Margin");
            sb.AppendLine($"12,Drawings,Owner Drawings,\"{_model.OwnerDrawings:N2}\",Capital Withdrawals");
            sb.AppendLine($"23,Write-Offs,Customer Bad Debts,\"{_model.CustomerBadDebts:N2}\",Written Off");
            sb.AppendLine($"24,Write-Offs,Supplier Write-Offs,\"{_model.SupplierWriteOffs:N2}\",Discounts Settled");
            sb.AppendLine($"17,Aging,Delayed Customer Receivables (>30 Days),\"{_model.DelayedCustomerReceivables30Days:N2}\",Count: {_model.DelayedCustomerCount30Days}");
            sb.AppendLine($"18,Aging,Delayed Supplier Payables (>30 Days),\"{_model.DelayedSupplierPayables30Days:N2}\",Count: {_model.DelayedSupplierCount30Days}");
            sb.AppendLine($"26,Audit,Deletion & Cancellation Logs,\"{_model.DeletionCount}\",Deleted Bills/Vouchers");
            sb.AppendLine($"27,Audit,Price Change Modifications,\"{_model.PriceChangeCount}\",Item Master Edits");
            sb.AppendLine($"28,Audit,Physical Stock Discrepancy Logs,\"{_model.StockAdjustmentCount}\",Stock Adjustments");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private string FormatCurr(decimal val)
        {
            return "₹ " + val.ToString("N2", _culture);
        }
    }
}
