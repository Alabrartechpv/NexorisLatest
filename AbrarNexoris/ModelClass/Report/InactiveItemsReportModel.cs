using System;

namespace ModelClass.Report
{
    public class InactiveItemsReportFilter
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public int CategoryId { get; set; }
        public int GroupId { get; set; }
        public string DateFilterMode { get; set; } = "ALL"; // "ALL" or "DATE_RANGE"
        public string SearchText { get; set; } = string.Empty;
    }

    public class InactiveItemsReportRow
    {
        public int ItemId { get; set; }
        public string ItemNo { get; set; }
        public string Barcode { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public string CategoryName { get; set; }
        public string GroupName { get; set; }
        public string BrandName { get; set; }
        public string HSNCode { get; set; }
        public decimal Stock { get; set; }
        public decimal UnitCost { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal WalkinPrice { get; set; }
        public string ItemStatus { get; set; } = "Inactive";
        public string StatusReason { get; set; }
        public DateTime? StatusDate { get; set; }
        public string InactivatedByUser { get; set; }
        public int UserId { get; set; }
        public string CounterName { get; set; }
        public int CounterId { get; set; }
        public string BranchName { get; set; }
        public int BranchId { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
