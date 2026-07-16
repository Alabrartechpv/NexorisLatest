using System;

namespace ModelClass.Report
{
    public class VendorPaymentReportRow
    {
        public int PaymentMasterId { get; set; }
        public int BranchId { get; set; }
        public int VoucherId { get; set; }
        public DateTime VoucherDate { get; set; }
        public int VendorLedgerId { get; set; }
        public int PaymentMethodId { get; set; }
        public string VendorName { get; set; }
        public long PurchaseNo { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public string PaymentMode { get; set; }
        public string Remark { get; set; }
        public string Status { get; set; }
        public string PaymentReference { get; set; }

        public string DocumentNo
        {
            get { return PurchaseNo > 0 ? "DN" + PurchaseNo : string.Empty; }
        }
    }

    public class VendorPaymentReportFilter
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int VendorLedgerId { get; set; }
    }
}
