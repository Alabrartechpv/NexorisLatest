using System;

namespace ModelClass.Report
{
    /// <summary>
    /// Item-wise Sales and Profit Summary item model
    /// </summary>
    public class ItemwiseSalesSummaryItem
    {
        public int ItemId { get; set; }
        public string Barcode { get; set; }
        public string ItemName { get; set; }
        public string GroupName { get; set; }
        public string CategoryName { get; set; }
        public string BaseUnitName { get; set; }
        
        public decimal TotalQtySold { get; set; }
        public decimal AvgUnitPrice { get; set; }
        public decimal TotalSalesAmount { get; set; }
        public decimal TotalCostValue { get; set; }
        public decimal TotalMarginProfit { get; set; }

        // Calculated properties
        public decimal MarginPercent => TotalSalesAmount > 0 ? (TotalMarginProfit / TotalSalesAmount) * 100 : 0;
    }

    /// <summary>
    /// Item-wise Sales and Profit Summary filter parameters
    /// </summary>
    public class ItemwiseSalesSummaryFilter
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public string BarcodeContains { get; set; }
        public int? GroupId { get; set; }
        public int? CategoryId { get; set; }
    }
}
