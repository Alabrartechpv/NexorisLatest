using System;
using System.Collections.Generic;

namespace ModelClass.Report
{
    public class CustomerOutstandingReportRow
    {
        public int LedgerID { get; set; }
        public string LedgerName { get; set; }
        public long BillNo { get; set; }
        public string DocNo { get; set; }
        public DateTime? BillDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public decimal Balance { get; set; }
        public string SalesPerson { get; set; }
        public int? SalesmanId { get; set; }
        public string CategoryName { get; set; }
        public int? CategoryId { get; set; }

        public List<CustomerBillReceiptDetail> ReceiptDetails { get; set; } = new List<CustomerBillReceiptDetail>();
        public bool HasLoadedReceipts { get; set; }
    }

    public class CustomerBillReceiptDetail
    {
        public long BillNo { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string VoucherNo { get; set; }
        public string PaymentMode { get; set; }
        public decimal BillAmount { get; set; }
        public decimal ReceiptAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public string Remarks { get; set; }
    }

    public class CustomerOutstandingReportFilter
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public int LedgerId { get; set; }
        public int FromLedgerId { get; set; }
        public int ToLedgerId { get; set; }
        public bool UseDateFilter { get; set; }
        public string DateFilterMode { get; set; }
        public int? CategoryId { get; set; }
        public int? FromCategoryId { get; set; }
        public int? ToCategoryId { get; set; }
        public string CategoryName { get; set; }
        public int? SalesmanId { get; set; }
        public string SalesmanName { get; set; }
        public bool PaymentDueOnly { get; set; }
        public bool IncludePaymentNotWithinSelectionDate { get; set; }
    }
}

