using System;

namespace ModelClass.Report
{
    /// <summary>
    /// Model representing a single invoice/bill in sales history by counter.
    /// </summary>
    public class CounterReportModel
    {
        public long BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public string Counter { get; set; }
        public string UserName { get; set; }
        public string CustomerName { get; set; }
        public string PaymodeName { get; set; }
        public string CashMode { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmt { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal NetAmount { get; set; }
        public string Status { get; set; }
    }
}
