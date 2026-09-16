using System;
using System.Collections.Generic;

namespace ModelClass.Report
{
    public class SalesHoldSummaryItem
    {
        public long BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public string CustomerName { get; set; }
        public string UserName { get; set; }
        public string PaymodeName { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalQty { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmt { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal NetAmount { get; set; }
        public string Status { get; set; }
        public int BranchId { get; set; }
        public int CompanyId { get; set; }
        public int CounterId { get; set; }
    }

    public class SalesHoldDetailItem
    {
        public long BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public string CustomerName { get; set; }
        public string UserName { get; set; }
        public string PaymodeName { get; set; }
        public int SlNo { get; set; }
        public long ItemId { get; set; }
        public string ItemName { get; set; }
        public string Barcode { get; set; }
        public string Unit { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string GroupName { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public int BranchId { get; set; }
        public int CompanyId { get; set; }
        public int CounterId { get; set; }
    }

    public class SalesHoldFilter : SessionAwareModel
    {
        public string DateMode { get; set; } = "ALL"; // "ALL" or "RANGE"
        public bool IsAllDates => string.Equals(DateMode, "ALL", StringComparison.OrdinalIgnoreCase);
        public DateTime FromDate { get; set; } = DateTime.Today;
        public DateTime ToDate { get; set; } = DateTime.Today;
        public string ViewMode { get; set; } = "Summary"; // "Summary" or "Detail"
        public long? LedgerId { get; set; }
        public string CustomerName { get; set; }
        public int? UserId { get; set; }
        public long? ItemId { get; set; }
        public int? CategoryId { get; set; }
        public int? GroupId { get; set; }
        public int? BrandId { get; set; }
        public string Status { get; set; }
    }
}
