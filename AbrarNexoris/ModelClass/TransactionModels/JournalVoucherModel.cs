using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelClass.TransactionModels
{
    public class JournalVoucher
    {
        public long VoucherID { get; set; }
        public string VoucherNumber { get; set; }
        public DateTime VoucherDate { get; set; }
        public string VoucherType { get; set; } = "Journal";
        public string Narration { get; set; }
        public int CompanyID { get; set; }
        public int BranchID { get; set; }
        public int FinYearID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public List<JournalVoucherLine> Lines { get; set; } = new List<JournalVoucherLine>();

        public decimal TotalDebit => Lines.Sum(line => line.Debit);
        public decimal TotalCredit => Lines.Sum(line => line.Credit);
    }

    public class JournalVoucherLine
    {
        public int SlNo { get; set; }
        public long LedgerID { get; set; }
        public string LedgerName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Narration { get; set; }
    }
}
