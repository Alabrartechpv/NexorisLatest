using System;

namespace ModelClass.Report
{
    public class CustomerReceiptReportRow
    {
        public int VoucherId { get; set; }
        public DateTime VoucherDate { get; set; }
        public long BillNo { get; set; }
        public int CustomerLedgerId { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ReceiptAmount { get; set; }
        public decimal Balance { get; set; }

        public string Status
        {
            get { return Balance <= 0 ? "Closed" : "Open"; }
        }
    }

    public class CustomerReceiptReportFilter
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int BranchId { get; set; }
        public int CustomerLedgerId { get; set; }
    }
}
