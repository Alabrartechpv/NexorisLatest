using System;

namespace ModelClass.Report
{
    public class VendorOutstandingReportRow
    {
        public int AcctCode { get; set; }
        public string Company { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public long PurchaseNo { get; set; }
        public DateTime Date { get; set; }
        public string Reference { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? PostDate { get; set; }
        public decimal DocAmt { get; set; }
        public decimal Balance { get; set; }
        public int IsPR { get; set; }

        public string DocNo
        {
            get 
            {
                if (IsPR == 1) return "PR-" + PurchaseNo;
                return PurchaseNo > 0 ? "GRN-" + PurchaseNo : string.Empty;
            }
        }

        public int LedgerID
        {
            get { return AcctCode; }
            set { AcctCode = value; }
        }

        public string LedgerName
        {
            get { return Company; }
            set { Company = value; }
        }

        public DateTime? VoucherDate
        {
            get { return Date == DateTime.MinValue ? (DateTime?)null : Date; }
            set { Date = value ?? DateTime.MinValue; }
        }

        public decimal TotalOutstanding
        {
            get { return DocAmt; }
            set { DocAmt = value; }
        }

        public decimal TotalPaid
        {
            get { return DocAmt - Balance; }
            set { Balance = DocAmt - value; }
        }
    }

    public class VendorOutstandingReportFilter
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public int LedgerId { get; set; }
        public int FromLedgerId { get; set; }
        public int ToLedgerId { get; set; }
        public string DateFilterMode { get; set; }
        public bool UseDateFilter { get; set; }
        public bool PaymentDueOnly { get; set; }
        public bool IncludePaymentNotWithinSelectionDate { get; set; }
        public bool GetUnallocatedReturnsOnly { get; set; }
    }
}
