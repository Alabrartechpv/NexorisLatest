using System;
using System.Collections.Generic;

namespace ModelClass.Report
{
    /// <summary>
    /// Represents a single vendor payment transaction or bill allocation row.
    /// </summary>
    public class DatewiseVendorPaymentReportRow
    {
        public int PaymentMasterId { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public string VoucherType { get; set; } // "Vendor Payment" or "Debit Note"
        public int VoucherId { get; set; }
        public string VoucherNo { get; set; }
        public DateTime VoucherDate { get; set; }
        public int VendorLedgerId { get; set; }
        public string VendorName { get; set; }
        public long PurchaseNo { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime BillDate { get; set; }
        public decimal BillAmount { get; set; }
        public decimal TotalBillPaid { get; set; }
        public decimal ReturnedAmount { get; set; }
        public decimal BillBalance { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal MasterPaymentAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public string PaymentMode { get; set; }
        public string Narration { get; set; }
        public string PaymentReference { get; set; }
        public string Status { get; set; } = "Active";
        public decimal CurrentVendorOutstanding { get; set; }
        public decimal PaidInPeriod { get; set; }
        public string Vouchers { get; set; }
    }

    /// <summary>
    /// Represents a Vendor Invoice row matching FrmPayment.cs schema.
    /// </summary>
    public class DatewiseVendorInvoiceRow
    {
        public int VendorLedgerId { get; set; }
        public string VendorName { get; set; }
        public string BillKey { get; set; }
        public long PurchaseNo { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime BillDate { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal ReturnedAmount { get; set; }
        public decimal Balance { get; set; }
        public decimal PaidInPeriod { get; set; }
        public string Vouchers { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; } = "Active";
        public string BranchName { get; set; }
    }

    /// <summary>
    /// Represents a single voucher payment transaction for an invoice.
    /// </summary>
    public class DatewiseVendorVoucherRow
    {
        public string BillKey { get; set; }
        public int VendorLedgerId { get; set; }
        public long PurchaseNo { get; set; }
        public DateTime VoucherDate { get; set; }
        public int VoucherId { get; set; }
        public string VoucherNo { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public decimal RunningBalance { get; set; }
        public string PaymentMode { get; set; }
        public string Narration { get; set; }
        public string Status { get; set; } = "Active";
        public string BranchName { get; set; }
    }

    /// <summary>
    /// Composite data returned from the report query.
    /// </summary>
    public class DatewiseVendorPaymentReportData
    {
        public List<DatewiseVendorPaymentReportRow> Payments { get; set; } = new List<DatewiseVendorPaymentReportRow>();
        public List<DatewiseVendorInvoiceRow> Invoices { get; set; } = new List<DatewiseVendorInvoiceRow>();
        public List<DatewiseVendorVoucherRow> VoucherDetails { get; set; } = new List<DatewiseVendorVoucherRow>();
    }

    /// <summary>
    /// Filter parameters for Date-wise Vendor Payment Report.
    /// </summary>
    public class DatewiseVendorPaymentReportFilter
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string PeriodMode { get; set; } = "DATE_RANGE"; // "DATE_RANGE", "MONTH_WISE", "YEAR_WISE", "ALL"
        public int Month { get; set; } = 0;
        public int Year { get; set; } = 0;
        public int CompanyId { get; set; } = 0;
        public int BranchId { get; set; } = 0;
        public int FinYearId { get; set; } = 0;
        public int VendorLedgerId { get; set; } = 0;
        public string PaymentType { get; set; } = "ALL"; // "ALL", "Vendor Payment", "Debit Note"
        public string PaymentModeFilter { get; set; } = "ALL"; // "ALL", "CASH", "BANK TRANSFER", etc.
        public string Status { get; set; } = "Active"; // "Active", "Cancel", "ALL"
        public string SearchText { get; set; } = string.Empty;
        public string ViewMode { get; set; } = "DETAILED"; // "DETAILED", "DATE_VENDOR_SUMMARY", "VENDOR_SUMMARY", "DATE_MATRIX"
    }
}
