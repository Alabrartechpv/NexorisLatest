using System;

namespace ModelClass.Report
{
    /// <summary>
    /// Model representing a single row in the Date-wise Stock Adjustment Report.
    /// Supports transaction details, item info, system stock, physical stock, qty differences, cost, and amount.
    /// </summary>
    public class DatewiseStockAdjustmentReportRow
    {
        public DateTime AdjustmentDate { get; set; }
        public int AdjustmentNo { get; set; }
        public int StockAdjustmentMasterId { get; set; }
        public int SlNo { get; set; }
        public int ItemID { get; set; }
        public string BarCode { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public string AdjustmentType { get; set; } // 'Stock IN' or 'Stock OUT'
        public decimal SystemStock { get; set; }
        public decimal PhysicalStock { get; set; }
        public decimal QtyDifference { get; set; }  // Net adjustment (signed)
        public decimal Qty { get; set; }            // Magnitude / Absolute Qty for matrix & summary
        public decimal StockInQty { get; set; }
        public decimal StockOutQty { get; set; }
        public decimal Cost { get; set; }
        public decimal Amount { get; set; }         // Net Value (signed)
        public decimal NetAmount { get; set; }      // Magnitude Value (ABS(QtyDifference) * Cost)
        public string Reason { get; set; }
        public string Comments { get; set; }
        public string LedgerName { get; set; }
        public string UserName { get; set; }
    }

    /// <summary>
    /// Filter criteria for querying Date-wise Stock Adjustment Report.
    /// Supports Date Range, Month Wise, Year Wise, ALL dates, Adjustment Type, Item search, and View mode.
    /// </summary>
    public class DatewiseStockAdjustmentReportFilter
    {
        public string PeriodMode { get; set; } = "DATE_RANGE"; // 'DATE_RANGE', 'MONTH_WISE', 'YEAR_WISE', 'ALL'
        public DateTime FromDate { get; set; } = DateTime.Today;
        public DateTime ToDate { get; set; } = DateTime.Today;
        public int Month { get; set; } = 0; // 0 = ALL, 1..12
        public int Year { get; set; } = 0;  // 0 = ALL, 2026, ...
        public int CompanyId { get; set; } = 0;
        public int BranchId { get; set; } = 0;
        public int FinYearId { get; set; } = 0;
        public string AdjustmentType { get; set; } = "ALL"; // 'ALL', 'Stock IN', 'Stock OUT'
        public string ItemSearch { get; set; } = string.Empty;
        public string ViewMode { get; set; } = "DATE_MATRIX"; // 'DATE_MATRIX', 'DETAILED', 'DATE_ITEM_SUMMARY', 'ITEM_SUMMARY'
    }
}
