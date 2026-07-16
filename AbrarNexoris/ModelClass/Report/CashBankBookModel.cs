using System;
using System.Collections.Generic;

namespace ModelClass.Report
{
    public class CashBankBookModel
    {
        public List<CashBankTransaction> Transactions { get; set; }
        public CashBankSummary Summary { get; set; }

        public CashBankBookModel()
        {
            Transactions = new List<CashBankTransaction>();
            Summary = new CashBankSummary();
        }
    }

    public class CashBankTransaction
    {
        public int VoucherID { get; set; }
        public DateTime VoucherDate { get; set; }
        public string VoucherNo { get; set; }
        public string VoucherTypeName { get; set; }
        public string Particulars { get; set; }
        public string Narration { get; set; }
        public decimal ReceiptAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal RunningBalance { get; set; }
        
        // Formatted property for displaying in grid (e.g., "1,200.00 Dr")
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

    public class CashBankSummary
    {
        public decimal OpeningBalance { get; set; }
        public decimal TotalReceipts { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal ClosingBalance { get; set; }
    }
}
