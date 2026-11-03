using System;

namespace ModelClass.Report
{
    /// <summary>
    /// Model representing a single row in the Date-wise Purchase Report.
    /// Captures purchase date, bill & vendor details, item details, quantities, rates, and amounts.
    /// </summary>
    public class DatewisePurchaseReportRow
    {
        public DateTime PurchaseDate { get; set; }
        public long PurchaseNo { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string VendorName { get; set; }
        public int VendorLedgerId { get; set; }
        public int SlNo { get; set; }
        public int ItemID { get; set; }
        public string BarCode { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public string Packing { get; set; }
        public decimal Qty { get; set; }
        public decimal Free { get; set; }
        public decimal TotalQty { get; set; }
        public decimal Cost { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal NetAmount { get; set; }
        public string Paymode { get; set; }
        public string BilledBy { get; set; }
    }

    /// <summary>
    /// Filter criteria for querying the Date-wise Purchase Report.
    /// Supports Date Range, Month Wise, Year Wise, and All Dates modes,
    /// along with Item search, Vendor selection, and View aggregation mode.
    /// </summary>
    public class DatewisePurchaseReportFilter
    {
        public string PeriodMode { get; set; } = "DATE_RANGE"; // 'DATE_RANGE', 'MONTH_WISE', 'YEAR_WISE', 'ALL'
        public DateTime FromDate { get; set; } = DateTime.Today;
        public DateTime ToDate { get; set; } = DateTime.Today;
        public int Month { get; set; } = 0; // 0 = ALL, 1..12
        public int Year { get; set; } = 0; // 0 = ALL, 2026, ...
        public int CompanyId { get; set; } = 0;
        public int BranchId { get; set; } = 0;
        public int FinYearId { get; set; } = 0;
        public int VendorLedgerId { get; set; } = 0;
        public string ItemSearch { get; set; } = string.Empty;
        public string ViewMode { get; set; } = "DETAILED"; // 'DETAILED', 'DATE_ITEM_SUMMARY', 'ITEM_SUMMARY'
    }
}
