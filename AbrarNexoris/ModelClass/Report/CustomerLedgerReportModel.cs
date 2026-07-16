using System;

namespace ModelClass.Report
{
    public class CustomerLedgerReportRow
    {
        public long VoucherID { get; set; }
        public DateTime VoucherDate { get; set; }
        public string VoucherNo { get; set; }
        public string VoucherTypeName { get; set; }
        public string Particulars { get; set; }
        public string Narration { get; set; }
        public decimal ReceiptAmount { get; set; } // Dr (debit amount increases debtor balance)
        public decimal PaymentAmount { get; set; } // Cr (credit amount decreases debtor balance)
        public decimal RunningBalance { get; set; }
    }

    public class CustomerLedgerReportFilter
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public int LedgerId { get; set; }
    }
}
