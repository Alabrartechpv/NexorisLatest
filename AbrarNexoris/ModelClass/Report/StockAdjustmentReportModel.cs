using System;

namespace ModelClass.Report
{
    public class StockAdjustmentReportRow
    {
        public long SlNo { get; set; }
        public int StockAdjustmentId { get; set; }
        public int StockAdjustmentNo { get; set; }
        public DateTime StockAdjustmentDate { get; set; }
        public string AdjustmentType { get; set; }
        public string Barcode { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string UnitName { get; set; }
        public decimal SystemStock { get; set; }
        public decimal PhysicalStock { get; set; }
        public decimal QtyDifference { get; set; }
        public decimal StockInQty { get; set; }
        public decimal StockOutQty { get; set; }
        public decimal Cost { get; set; }
        public decimal AdjustmentValue { get; set; }
        public string Reason { get; set; }
        public string LedgerName { get; set; }
        public string UserName { get; set; }
        public string Comments { get; set; }
    }

    public class StockAdjustmentReportFilter
    {
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string AdjustmentType { get; set; }
        public string SearchQuery { get; set; }
    }
}
