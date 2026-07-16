using System;

namespace ModelClass.Report
{
    public class CustomerOutstandingReportRow
    {
        public int LedgerID { get; set; }
        public string LedgerName { get; set; }
        public long BillNo { get; set; }
        public DateTime? BillDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public decimal Balance { get; set; }
    }

    public class CustomerOutstandingReportFilter
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public int LedgerId { get; set; }
    }
}
