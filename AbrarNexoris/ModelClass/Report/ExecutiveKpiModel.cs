using System;
using System.Collections.Generic;

namespace ModelClass.Report
{
    public class ExecutiveKpiModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.Now;

        // ═══════════════════════════════════════════════════════════════════
        // SECTION 1: INVENTORY & VALUATION (Metrics 1, 2, 5, 6, 14, 15, 16, 20, 21, 22)
        // ═══════════════════════════════════════════════════════════════════
        /// <summary>Metric 1: Total Stock Value at Cost (ആകെ സ്റ്റോക്കിന്റെ വാങ്ങൽ വില)</summary>
        public decimal TotalStockCostValue { get; set; }

        /// <summary>Total Stock Value at Retail Price</summary>
        public decimal TotalStockRetailValue { get; set; }

        /// <summary>Metric 2: Stock Profit Potential (മുഴുവൻ വിറ്റഴിച്ചാൽ കിട്ടുന്ന ലാഭം = Retail - Cost)</summary>
        public decimal StockProfitPotential { get; set; }

        /// <summary>Total items count in stock</summary>
        public int TotalStockItemCount { get; set; }

        /// <summary>Total stock quantity across all items</summary>
        public decimal TotalStockQuantity { get; set; }

        /// <summary>Metric 5: Negative Stock Financial Impact (ബില്ലിംഗ് തെറ്റുകൾ മൂലം വരുന്ന സ്റ്റോക്ക് വ്യത്യാസം)</summary>
        public decimal NegativeStockImpactValue { get; set; }

        /// <summary>Metric 6: Negative Stock Items Count (നെഗറ്റീവ് സ്റ്റോക്കിലുള്ള ഉൽപ്പന്നങ്ങളുടെ എണ്ണം)</summary>
        public int NegativeStockItemCount { get; set; }

        /// <summary>Metric 6: Negative Stock Total Negative Qty</summary>
        public decimal NegativeStockTotalQty { get; set; }

        /// <summary>Metric 14: Loss Stock Value (Damaged, Expired, Stock OUT adjustments) (നഷ്ടപ്പെട്ടതോ കേടായതോ ആയ സ്റ്റോക്ക്)</summary>
        public decimal LossStockValue { get; set; }
        public decimal LossStockQty { get; set; }

        /// <summary>Metric 15: Extra Stock Value (Found in audit / Stock IN adjustments) (അധികമായി കണ്ടെത്തിയ സ്റ്റോക്ക്)</summary>
        public decimal ExtraStockValue { get; set; }
        public decimal ExtraStockQty { get; set; }

        /// <summary>Metric 16: Net Stock Adjustment Balance (Extra Stock - Loss Stock) (സ്റ്റോക്ക് അഡ്ജസ്റ്റ്മെന്റ് ബാലൻസ്)</summary>
        public decimal NetStockAdjustmentValue { get; set; }

        /// <summary>Metric 20: Excess Stock Alert Count (അധിക സ്റ്റോക്ക് അലർട്ട്)</summary>
        public int ExcessStockAlertCount { get; set; }

        /// <summary>Metric 21: Low Stock Alert Count (തീരാറായ സ്റ്റോക്ക് അലർട്ട്)</summary>
        public int LowStockAlertCount { get; set; }

        /// <summary>Metric 22: Reorder Alert Count (ഓർഡർ നൽകേണ്ട സാധനങ്ങളുടെ അലർട്ട്)</summary>
        public int ReorderAlertCount { get; set; }

        // ═══════════════════════════════════════════════════════════════════
        // SECTION 2: CASH, FINANCIAL ASSETS & LIABILITIES (Metrics 7, 8, 9, 10, 13, 29, 30)
        // ═══════════════════════════════════════════════════════════════════
        /// <summary>Metric 7: Cash in Hand (കൗണ്ടറിലും കൈയിലുമുള്ള പണം)</summary>
        public decimal CashInHand { get; set; }

        /// <summary>Metric 8: Bank Balance (ബാങ്ക് അക്കൗണ്ടിലെ തുക)</summary>
        public decimal BankBalance { get; set; }

        /// <summary>Metric 9: Supplier Payables (Total Outstanding) (സപ്ലൈയർമാർക്ക് കൊടുക്കാനുള്ള കടം)</summary>
        public decimal SupplierPayables { get; set; }
        public int SupplierPayablesCount { get; set; }

        /// <summary>Metric 10: Customer Receivables (Total Outstanding) (കസ്റ്റമേഴ്സിൽ നിന്നും കിട്ടാനുള്ള കടം)</summary>
        public decimal CustomerReceivables { get; set; }
        public int CustomerReceivablesCount { get; set; }

        /// <summary>Metric 13: Net Business Asset - NBA (ബിസിനസ്സിന്റെ യഥാർത്ഥ ആസ്തി മൂല്യം = Total Assets - Total Liabilities)</summary>
        public decimal NetBusinessAsset { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }

        /// <summary>Metric 29: Supplier Overpayment / Advance (സപ്ലൈയർക്ക് അധികം നൽകിയ പണം / അഡ്വാൻസ്)</summary>
        public decimal SupplierAdvanceBalance { get; set; }

        /// <summary>Metric 30: Customer Overpayment / Advance (കസ്റ്റമർ അധികം തന്ന പണം / അഡ്വാൻസ്)</summary>
        public decimal CustomerAdvanceBalance { get; set; }

        // ═══════════════════════════════════════════════════════════════════
        // SECTION 3: REVENUE, PROFITABILITY & EXPENSES (Metrics 3, 4, 11, 12, 19, 23, 24)
        // ═══════════════════════════════════════════════════════════════════
        /// <summary>Metric 3: Total Sales Revenue (തിരഞ്ഞെടുത്ത കാലയളവിലെ സെയിൽസ്)</summary>
        public decimal TotalSalesRevenue { get; set; }
        public int TotalSalesBillCount { get; set; }

        /// <summary>Total Purchases for period</summary>
        public decimal TotalPurchases { get; set; }

        /// <summary>Gross Profit</summary>
        public decimal GrossProfit { get; set; }

        /// <summary>Metric 4: Business Expenses (Direct + Indirect) (സ്ഥാപന നടത്തിപ്പ് ചെലവുകൾ)</summary>
        public decimal TotalBusinessExpenses { get; set; }
        public decimal DirectExpenses { get; set; }
        public decimal IndirectExpenses { get; set; }

        /// <summary>Metric 11: Actual Net Profit (യഥാർത്ഥ അറ്റലാഭം = Gross Profit - Expenses + Incomes)</summary>
        public decimal ActualNetProfit { get; set; }

        /// <summary>Metric 12: Owner Drawings (ഉടമ വ്യക്തിപരമായ ആവശ്യങ്ങൾക്ക് എടുത്തത്)</summary>
        public decimal OwnerDrawings { get; set; }

        /// <summary>Metric 19: Operating Profit Margin % (പ്രവർത്തന ലാഭ ശതമാനം = Net Profit / Sales * 100)</summary>
        public decimal OperatingProfitMarginPercent { get; set; }

        /// <summary>Metric 23: Customer Bad Debts Written Off (കസ്റ്റമർ എഴുതി തള്ളിയ തുക)</summary>
        public decimal CustomerBadDebts { get; set; }

        /// <summary>Metric 24: Supplier Write-Offs / Discounts Received (വെണ്ടർ എഴുതി തള്ളിയ തുക)</summary>
        public decimal SupplierWriteOffs { get; set; }

        // ═══════════════════════════════════════════════════════════════════
        // SECTION 4: AGING & RISK CONTROL (Metrics 17, 18)
        // ═══════════════════════════════════════════════════════════════════
        /// <summary>Metric 17: Delayed Customer Receivables > 30 Days Overdue (30 ദിവസത്തിലധികം പഴക്കമുള്ള കസ്റ്റമർ കടങ്ങൾ)</summary>
        public decimal DelayedCustomerReceivables30Days { get; set; }
        public int DelayedCustomerCount30Days { get; set; }

        /// <summary>Metric 18: Delayed Supplier Payables > 30 Days Overdue (30 ദിവസത്തിലധികം പഴക്കമുള്ള സപ്ലൈയർ കടങ്ങൾ)</summary>
        public decimal DelayedSupplierPayables30Days { get; set; }
        public int DelayedSupplierCount30Days { get; set; }

        // ═══════════════════════════════════════════════════════════════════
        // SECTION 5: GROWTH & AUDIT LOGS (Metrics 25, 26, 27, 28)
        // ═══════════════════════════════════════════════════════════════════
        /// <summary>Metric 25: Monthly Summary & Growth % Matrix (പ്രധാന മാട്രിക്സ് ടോട്ടൽ & ഗ്രോത്ത് റിപ്പോർട്ട്)</summary>
        public List<MonthlyGrowthMatrixItem> MonthlyGrowthMatrix { get; set; } = new List<MonthlyGrowthMatrixItem>();

        /// <summary>Metric 26: Deletion Count in period (ബിൽ/സെയിൽസ് ഡിലീറ്റ് ചെയ്തതിന്റെ എണ്ണം)</summary>
        public int DeletionCount { get; set; }

        /// <summary>Metric 27: Price Change Count in period (വില മാറ്റിയതിന്റെ എണ്ണം)</summary>
        public int PriceChangeCount { get; set; }

        /// <summary>Metric 28: Stock Adjustment Count in period (സ്റ്റോക്ക് മാറ്റിയതിന്റെ എണ്ണം)</summary>
        public int StockAdjustmentCount { get; set; }
    }

    public class MonthlyGrowthMatrixItem
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public decimal SalesAmount { get; set; }
        public decimal SalesGrowthPercent { get; set; }
        public decimal PurchaseAmount { get; set; }
        public decimal ExpenseAmount { get; set; }
        public decimal NetProfitAmount { get; set; }
        public decimal NetProfitGrowthPercent { get; set; }
    }
}
