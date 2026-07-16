using System;

namespace ModelClass.Report
{
    /// <summary>
    /// Represents a row in the Salesman-wise Sales Summary report
    /// </summary>
    public class SalesmanwiseSalesSummaryItem
    {
        public long SlNo { get; set; }
        public int? SalesmanId { get; set; }
        public string SalesmanName { get; set; }
        public string Email { get; set; }
        public int InvoiceCount { get; set; }
        public double TotalQtySold { get; set; }
        public double TotalSalesAmount { get; set; }
        public double CommissionPercent { get; set; }
        public double CommissionAmount { get; set; }
    }

    /// <summary>
    /// Filter parameters for loading the Salesman-wise Sales Summary report
    /// </summary>
    public class SalesmanwiseSalesSummaryFilter
    {
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? SalesmanId { get; set; }
        public string SearchQuery { get; set; }
    }
}
