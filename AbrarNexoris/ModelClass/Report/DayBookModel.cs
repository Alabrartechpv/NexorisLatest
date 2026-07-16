using System;
using System.Collections.Generic;

namespace ModelClass.Report
{
    public class DayBookTransaction
    {
        public DateTime VoucherDate { get; set; }
        public int VoucherID { get; set; }
        public string VoucherNo { get; set; }
        public string VoucherTypeName { get; set; }
        public string Particulars { get; set; }
        public string Narration { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
    }

    public class DayBookSummary
    {
        public decimal TotalDebits { get; set; }
        public decimal TotalCredits { get; set; }
    }

    public class DayBookResponse
    {
        public List<DayBookTransaction> Transactions { get; set; }
        public DayBookSummary Summary { get; set; }

        public DayBookResponse()
        {
            Transactions = new List<DayBookTransaction>();
            Summary = new DayBookSummary();
        }
    }
}
