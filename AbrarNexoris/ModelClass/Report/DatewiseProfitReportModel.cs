using System;

namespace ModelClass.Report
{
    /// <summary>
    /// Model representing a single row in the Date-wise Profit Report.
    /// Captures bill date, bill & customer details, item details, quantities, rates, costs, sales amounts, profit, and margin %.
    /// </summary>
    public class DatewiseProfitReportRow
    {
        public DateTime BillDate { get; set; }
        public long BillNo { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string CustomerName { get; set; }
        public int CustomerLedgerId { get; set; }
        public int SlNo { get; set; }
        public int ItemID { get; set; }
        public string BarCode { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public string Packing { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal CostRate { get; set; }
        public decimal SalesAmount { get; set; }
        public decimal DiscountAmt { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal NetAmount { get; set; }
        public decimal CostAmount { get; set; }
        public decimal Profit { get; set; }
        public decimal MarginPercent { get; set; }
        public string Paymode { get; set; }
        public string BilledBy { get; set; }
    }

    /// <summary>
    /// Filter criteria for querying the Date-wise Profit Report.
    /// Supports Date Range, Month Wise, Year Wise, and All Dates modes,
    /// along with Item search, Customer selection, and View aggregation mode.
    /// </summary>
    public class DatewiseProfitReportFilter
    {
        public string PeriodMode { get; set; } = "DATE_RANGE"; // 'DATE_RANGE', 'MONTH_WISE', 'YEAR_WISE', 'ALL'
        public DateTime FromDate { get; set; } = DateTime.Today;
        public DateTime ToDate { get; set; } = DateTime.Today;
        public int Month { get; set; } = 0; // 0 = ALL, 1..12
        public int Year { get; set; } = 0; // 0 = ALL, 2026, ...
        public int CompanyId { get; set; } = 0;
        public int BranchId { get; set; } = 0;
        public int FinYearId { get; set; } = 0;
        public int CustomerLedgerId { get; set; } = 0;
        public string ItemSearch { get; set; } = string.Empty;
        public string ViewMode { get; set; } = "DATE_MATRIX"; // 'DATE_MATRIX', 'DETAILED', 'DATE_ITEM_SUMMARY', 'ITEM_SUMMARY'
    }
}
