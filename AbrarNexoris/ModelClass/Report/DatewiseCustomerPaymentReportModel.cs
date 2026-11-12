using System;
using System.Collections.Generic;

namespace ModelClass.Report
{
    /// <summary>
    /// Represents a single customer payment/receipt transaction or bill allocation row.
    /// </summary>
    public class DatewiseCustomerPaymentReportRow
    {
        public int ReceiptMasterId { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public string ReceiptType { get; set; } = "Customer Receipt"; // "Customer Receipt" or "Credit Note"
        public int VoucherId { get; set; }
        public string VoucherNo { get; set; }
        public DateTime VoucherDate { get; set; }
        public int CustomerLedgerId { get; set; }
        public string CustomerName { get; set; }
        public long BillNo { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime BillDate { get; set; }
        public decimal BillAmount { get; set; }
        public decimal TotalBillReceived { get; set; }
        public decimal ReturnedAmount { get; set; }
        public decimal BillBalance { get; set; }
        public decimal ReceiptAmount { get; set; }
        public decimal MasterReceiptAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public string PaymentMode { get; set; }
        public string Narration { get; set; }
        public string PaymentReference { get; set; }
        public string Status { get; set; } = "Active";
        public decimal CurrentCustomerOutstanding { get; set; }
        public decimal ReceivedInPeriod { get; set; }
        public string Receipts { get; set; }
    }

    /// <summary>
    /// Represents a Customer Invoice row matching FrmReceipt.cs schema.
    /// </summary>
    public class DatewiseCustomerInvoiceRow
    {
        public int CustomerLedgerId { get; set; }
        public string CustomerName { get; set; }
        public string BillKey { get; set; }
        public long BillNo { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime BillDate { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public decimal ReturnedAmount { get; set; }
        public decimal Balance { get; set; }
        public decimal ReceivedInPeriod { get; set; }
        public string Receipts { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; } = "Active";
        public string BranchName { get; set; }
    }

    /// <summary>
    /// Represents a single receipt voucher transaction for an invoice (matching FrmViewReceipt.cs).
    /// </summary>
    public class DatewiseCustomerVoucherRow
    {
        public string BillKey { get; set; }
        public int CustomerLedgerId { get; set; }
        public long BillNo { get; set; }
        public DateTime VoucherDate { get; set; }
        public int VoucherId { get; set; }
        public string VoucherNo { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal ReceiptAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public decimal RunningBalance { get; set; }
        public string PaymentMode { get; set; }
        public string Narration { get; set; }
        public string Status { get; set; } = "Active";
        public string BranchName { get; set; }
    }

    /// <summary>
    /// Composite data returned from the customer payment report query.
    /// </summary>
    public class DatewiseCustomerPaymentReportData
    {
        public List<DatewiseCustomerPaymentReportRow> Receipts { get; set; } = new List<DatewiseCustomerPaymentReportRow>();
        public List<DatewiseCustomerInvoiceRow> Invoices { get; set; } = new List<DatewiseCustomerInvoiceRow>();
        public List<DatewiseCustomerVoucherRow> VoucherDetails { get; set; } = new List<DatewiseCustomerVoucherRow>();
    }

    /// <summary>
    /// Filter parameters for Date-wise Customer Payment Report.
    /// </summary>
    public class DatewiseCustomerPaymentReportFilter
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string PeriodMode { get; set; } = "DATE_RANGE"; // "DATE_RANGE", "MONTH_WISE", "YEAR_WISE", "ALL"
        public int Month { get; set; } = 0;
        public int Year { get; set; } = 0;
        public int CompanyId { get; set; } = 0;
        public int BranchId { get; set; } = 0;
        public int FinYearId { get; set; } = 0;
        public int CustomerLedgerId { get; set; } = 0;
        public string ReceiptType { get; set; } = "ALL"; // "ALL", "Customer Receipt", "Credit Note"
        public string PaymentModeFilter { get; set; } = "ALL"; // "ALL", "CASH", "BANK TRANSFER", etc.
        public string Status { get; set; } = "Active"; // "Active", "Cancel", "ALL"
        public string SearchText { get; set; } = string.Empty;
        public string ViewMode { get; set; } = "DETAILED"; // "DETAILED", "DATE_CUSTOMER_SUMMARY", "CUSTOMER_SUMMARY", "DATE_MATRIX"
    }
}
