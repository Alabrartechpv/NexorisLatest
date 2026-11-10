using System;

namespace ModelClass.Report
{
    /// <summary>
    /// Represents a single stock movement row (Stock Came / IN & Stock Gone / OUT)
    /// across purchases, sales, returns, adjustments, transfers, and opening stock.
    /// </summary>
    public class DatewiseItemStockReportRow
    {
        public DateTime TransactionDate { get; set; }
        public string Operation { get; set; }
        public string Way { get; set; } // "IN" or "OUT"
        public string RefNo { get; set; }
        public int SlNo { get; set; }
        public int ItemID { get; set; }
        public string BarCode { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public decimal Qty { get; set; }
        public decimal InQty { get; set; }
        public decimal OutQty { get; set; }
        public decimal NetMovement => InQty - OutQty;
        public decimal Rate { get; set; }
        public decimal Cost { get; set; }
        public decimal Amount { get; set; }
        public string Account { get; set; }
        public string Remarks { get; set; }
        public string BranchName { get; set; }
        public decimal CurrentStock { get; set; }
    }

    /// <summary>
    /// Filter parameters for Date-wise Item Stock (Movement) Report.
    /// </summary>
    public class DatewiseItemStockReportFilter
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string PeriodMode { get; set; } = "DATE_RANGE"; // "DATE_RANGE", "MONTH_WISE", "YEAR_WISE", "ALL"
        public int Month { get; set; } = 0;
        public int Year { get; set; } = 0;
        public int CompanyId { get; set; } = 0;
        public int BranchId { get; set; } = 0;
        public int FinYearId { get; set; } = 0;
        public string WayFilter { get; set; } = "ALL"; // "ALL", "IN", "OUT"
        public string OperationFilter { get; set; } = "ALL"; // "ALL", "Purchase", "Sales", "Sales Return", "Purchase Return", "Stock Adjustment", "Stock Transfer", "Opening Stock"
        public string ItemSearch { get; set; } = string.Empty;
        public string ViewMode { get; set; } = "DETAILED"; // "DETAILED", "DATE_ITEM_SUMMARY", "ITEM_SUMMARY"
    }
}
