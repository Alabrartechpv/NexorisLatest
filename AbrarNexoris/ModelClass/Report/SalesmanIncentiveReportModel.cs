using System;
using System.Collections.Generic;

namespace ModelClass.Report
{
    public class SalesmanIncentiveReportFilter
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int SalesmanId { get; set; }
        public int UserId { get; set; }
        public int GroupId { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public int VendorId { get; set; }
        public decimal IncentivePercent { get; set; }
        public bool IncludeDetails { get; set; }
    }

    public class SalesmanIncentiveSummary
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public int SalesmanId { get; set; }
        public string SalesmanName { get; set; }
        public int SalesBillCount { get; set; }
        public int SalesReturnDocCount { get; set; }
        public decimal SalesQty { get; set; }
        public decimal SalesAmount { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal SalesReturnLoss { get; set; }
        public decimal NetProfit { get; set; }
        public decimal IncentivePercent { get; set; }
        public decimal IncentiveAmount { get; set; }
    }

    public class SalesmanIncentiveDetail
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public int SalesmanId { get; set; }
        public string SalesmanName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string SourceType { get; set; }
        public long BillNo { get; set; }
        public string ReferenceInvoiceNo { get; set; }
        public DateTime TransactionDate { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int VendorId { get; set; }
        public decimal Qty { get; set; }
        public decimal CostPerUnit { get; set; }
        public decimal SalesPricePerUnit { get; set; }
        public decimal SalesValue { get; set; }
        public decimal CostValue { get; set; }
        public decimal ProfitValue { get; set; }
        public string Reason { get; set; }
        public string AttributionStatus { get; set; }
    }

    public class SalesmanIncentiveReportData
    {
        public List<SalesmanIncentiveSummary> Summary { get; set; } = new List<SalesmanIncentiveSummary>();
        public List<SalesmanIncentiveDetail> Details { get; set; } = new List<SalesmanIncentiveDetail>();
    }
}
