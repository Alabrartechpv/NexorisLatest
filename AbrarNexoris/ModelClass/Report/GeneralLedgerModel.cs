using System;
using System.Collections.Generic;

namespace ModelClass.Report
{
    public class GeneralLedgerReport
    {
        public int LedgerId { get; set; }
        public string LedgerName { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<GeneralLedgerTransaction> Transactions { get; set; }
        public GeneralLedgerSummary Summary { get; set; }

        public GeneralLedgerReport()
        {
            Transactions = new List<GeneralLedgerTransaction>();
            Summary = new GeneralLedgerSummary();
        }
    }

    public class GeneralLedgerTransaction
    {
        public int VoucherID { get; set; }
        public DateTime VoucherDate { get; set; }
        public string VoucherNo { get; set; }
        public string VoucherTypeName { get; set; }
        public string Particulars { get; set; }
        public string VoucherLedgerName { get; set; }
        public string Narration { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal RunningBalance { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public DateTime? UserDate { get; set; }

        public string FormattedBalance
        {
            get
            {
                return RunningBalance >= 0
                    ? $"{Math.Abs(RunningBalance):N2} Dr"
                    : $"{Math.Abs(RunningBalance):N2} Cr";
            }
        }
    }

    public class GeneralLedgerSummary
    {
        public decimal OpeningBalance { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal ClosingBalance { get; set; }
    }
}

