using System;

namespace ModelClass.Report
{
    /// <summary>
    /// Model representing a row of the Customer-wise Sales Summary Report
    /// </summary>
    public class CustomerwiseSalesSummaryItem
    {
        public int SlNo { get; set; }
        public DateTime BillDate { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public int ItemId { get; set; }
        public string Barcode { get; set; }
        public string ItemName { get; set; }
        public string GroupName { get; set; }
        public string CategoryName { get; set; }
        public string BaseUnitName { get; set; }
        public decimal TotalQtySold { get; set; }
        public decimal TotalSalesAmount { get; set; }
    }

    /// <summary>
    /// Filter parameters for the Customer-wise Sales Summary Report query
    /// </summary>
    public class CustomerwiseSalesSummaryFilter
    {
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? CustomerId { get; set; }
        public int? GroupId { get; set; }
        public int? CategoryId { get; set; }
        public string SearchQuery { get; set; }
    }
}
