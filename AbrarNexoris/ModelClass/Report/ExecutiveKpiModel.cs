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
        public int LossStockItemCount { get; set; }

        /// <summary>Metric 15: Extra Stock Value (Found in audit / Stock IN adjustments) (അധികമായി കണ്ടെത്തിയ സ്റ്റോക്ക്)</summary>
        public decimal ExtraStockValue { get; set; }
        public decimal ExtraStockQty { get; set; }
        public int ExtraStockItemCount { get; set; }

        /// <summary>Metric 16: Net Stock Adjustment Balance (Extra Stock - Loss Stock) (സ്റ്റോക്ക് അഡ്ജസ്റ്റ്മെന്റ് ബാലൻസ്)</summary>
        public decimal NetStockAdjustmentValue { get; set; }

        /// <summary>Metric 20: Excess Stock Alert Count (അധിക സ്റ്റോക്ക് അലർട്ട്)</summary>
        public int ExcessStockAlertCount { get; set; }

        /// <summary>Metric 21: Low Stock Alert Count (തീരാറായ സ്റ്റോക്ക് അലർട്ട്)</summary>
        public int LowStockAlertCount { get; set; }

        /// <summary>Metric 22: Reorder Alert Count (ഓർഡർ നൽകേണ്ട സാധനങ്ങളുടെ അലർട്ട്)</summary>
        public int ReorderAlertCount { get; set; }

        /// <summary>Non-Moving / Dead Stock Value (കഴിഞ്ഞ 90 ദിവസമായി വിൽക്കാത്ത സാധനങ്ങളുടെ സ്റ്റോക്ക് മൂല്യം)</summary>
        public decimal DeadStockValue { get; set; }
        public int DeadStockItemCount { get; set; }

        // ═══════════════════════════════════════════════════════════════════
        // SECTION 2: CASH, FINANCIAL ASSETS & LIABILITIES
        // ═══════════════════════════════════════════════════════════════════
        /// <summary>Cash in Hand (കൗണ്ടറിലും കൈയിലുമുള്ള പണം)</summary>
        public decimal CashInHand { get; set; }

        /// <summary>Bank Balance (ബാങ്ക് അക്കൗണ്ടിലെ തുക)</summary>
        public decimal BankBalance { get; set; }

        /// <summary>Supplier Payables (Vendor Outstanding) (സപ്ലൈയർമാർക്ക് കൊടുക്കാനുള്ള കടം)</summary>
        public decimal SupplierPayables { get; set; }
        public int SupplierPayablesCount { get; set; }

        /// <summary>Customer Receivables (Customer Outstanding) (കസ്റ്റമേഴ്സിൽ നിന്നും കിട്ടാനുള്ള കടം)</summary>
        public decimal CustomerReceivables { get; set; }
        public int CustomerReceivablesCount { get; set; }

        /// <summary>Net Business Asset - NBA (ബിസിനസ്സിന്റെ യഥാർത്ഥ ആസ്തി മൂല്യം = Total Assets - Total Liabilities)</summary>
        public decimal NetBusinessAsset { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }

        /// <summary>Supplier Overpayment / Advance (സപ്ലൈയർക്ക് അധികം നൽകിയ പണം / അഡ്വാൻസ്)</summary>
        public decimal SupplierAdvanceBalance { get; set; }

        /// <summary>Customer Overpayment / Advance (കസ്റ്റമർ അധികം തന്ന പണം / അഡ്വാൻസ്)</summary>
        public decimal CustomerAdvanceBalance { get; set; }

        /// <summary>Total Manual Party Balance (ബാക്കി നിൽക്കുന്ന ആകെ മാനുവൽ ബാലൻസ്)</summary>
        public decimal TotalManualBalance { get; set; }

        /// <summary>Manual Customer Balance (Receivables) (കസ്റ്റമർ മാനുവൽ ബാലൻസ്)</summary>
        public decimal ManualCustomerBalance { get; set; }

        /// <summary>Manual Vendor Balance (Payables) (വെണ്ടർ മാനുവൽ ബാലൻസ്)</summary>
        public decimal ManualVendorBalance { get; set; }

        /// <summary>Manual Balance Entries Count</summary>
        public int ManualBalanceCount { get; set; }

        // ═══════════════════════════════════════════════════════════════════
        // SECTION 3: REVENUE, PURCHASES, PROFITABILITY & TAX
        // ═══════════════════════════════════════════════════════════════════
        /// <summary>Total Sales Revenue (തിരഞ്ഞെടുത്ത കാലയളവിലെ സെയിൽസ്)</summary>
        public decimal TotalSalesRevenue { get; set; }
        public int TotalSalesBillCount { get; set; }

        /// <summary>Sales Return (Customer Returns / Credit Notes) (കസ്റ്റമർ റിട്ടേൺസ്)</summary>
        public decimal TotalSalesReturn { get; set; }
        public int TotalSalesReturnCount { get; set; }

        /// <summary>Total Purchases for period (പർച്ചേസ് തുക)</summary>
        public decimal TotalPurchases { get; set; }
        public int TotalPurchasesBillCount { get; set; }

        /// <summary>Purchase Return (Vendor Returns / Debit Notes) (പർച്ചേസ് റിട്ടേൺസ്)</summary>
        public decimal TotalPurchaseReturn { get; set; }
        public int TotalPurchaseReturnCount { get; set; }

        /// <summary>Holded Items / Hold Bills (ഹോൾഡ് ചെയ്ത ബില്ലുകളും ഉൽപ്പന്നങ്ങളും)</summary>
        public decimal HoldBillsValue { get; set; }
        public int HoldBillsCount { get; set; }
        public decimal HoldItemsCount { get; set; }

        /// <summary>Gross Profit (മൊത്തം ലാഭം = Sales - Cost)</summary>
        public decimal GrossProfit { get; set; }

        /// <summary>Gross Profit Margin % (മൊത്തം ലാഭ ശതമാനം = Gross Profit / Sales * 100)</summary>
        public decimal GrossProfitMarginPercent { get; set; }

        /// <summary>Business Expenses (Direct + Indirect) (സ്ഥാപന നടത്തിപ്പ് ചെലവുകൾ)</summary>
        public decimal TotalBusinessExpenses { get; set; }
        public decimal DirectExpenses { get; set; }
        public decimal IndirectExpenses { get; set; }

        /// <summary>Actual Net Profit (യഥാർത്ഥ അറ്റലാഭം = Gross Profit - Expenses + Incomes)</summary>
        public decimal ActualNetProfit { get; set; }

        /// <summary>Operating Profit Margin % (പ്രവർത്തന ലാഭ ശതമാനം = Net Profit / Sales * 100)</summary>
        public decimal OperatingProfitMarginPercent { get; set; }

        /// <summary>GST Output Tax collected on sales</summary>
        public decimal OutputGstAmount { get; set; }

        /// <summary>GST Input Tax credit paid on purchases</summary>
        public decimal InputGstAmount { get; set; }

        /// <summary>Net GST / Tax Liability (Output GST - Input GST)</summary>
        public decimal NetTaxLiability { get; set; }

        /// <summary>Owner Drawings (ഉടമ വ്യക്തിപരമായ ആവശ്യങ്ങൾക്ക് എടുത്തത്)</summary>
        public decimal OwnerDrawings { get; set; }

        /// <summary>Customer Bad Debts Written Off (കസ്റ്റമർ എഴുതി തള്ളിയ തുക)</summary>
        public decimal CustomerBadDebts { get; set; }

        /// <summary>Supplier Write-Offs / Discounts Received (വെണ്ടർ എഴുതി തള്ളിയ തുക)</summary>
        public decimal SupplierWriteOffs { get; set; }

        // ═══════════════════════════════════════════════════════════════════
        // SECTION 4: AGING & RISK CONTROL
        // ═══════════════════════════════════════════════════════════════════
        /// <summary>Delayed Customer Receivables > 30 Days Overdue (30 ദിവസത്തിലധികം പഴക്കമുള്ള കസ്റ്റമർ കടങ്ങൾ)</summary>
        public decimal DelayedCustomerReceivables30Days { get; set; }
        public int DelayedCustomerCount30Days { get; set; }

        /// <summary>Delayed Supplier Payables > 30 Days Overdue (30 ദിവസത്തിലധികം പഴക്കമുള്ള സപ്ലൈയർ കടങ്ങൾ)</summary>
        public decimal DelayedSupplierPayables30Days { get; set; }
        public int DelayedSupplierCount30Days { get; set; }

        // ═══════════════════════════════════════════════════════════════════
        // SECTION 5: GROWTH & AUDIT LOGS
        // ═══════════════════════════════════════════════════════════════════
        /// <summary>Monthly Summary & Growth % Matrix (പ്രധാന മാട്രിക്സ് ടോട്ടൽ & ഗ്രോത്ത് റിപ്പോർട്ട്)</summary>
        public List<MonthlyGrowthMatrixItem> MonthlyGrowthMatrix { get; set; } = new List<MonthlyGrowthMatrixItem>();

        /// <summary>Deletion Count in period (ബിൽ/സെയിൽസ് ഡിലീറ്റ് ചെയ്തതിന്റെ എണ്ണം)</summary>
        public int DeletionCount { get; set; }

        /// <summary>Price Change Count in period (വില മാറ്റിയതിന്റെ എണ്ണം)</summary>
        public int PriceChangeCount { get; set; }

        /// <summary>Stock Adjustment Count in period (സ്റ്റോക്ക് മാറ്റിയതിന്റെ എണ്ണം)</summary>
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
