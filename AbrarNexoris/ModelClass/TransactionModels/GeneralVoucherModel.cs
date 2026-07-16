using System;

namespace ModelClass.TransactionModels
{
    public class GeneralVoucher
    {
        public long VoucherID { get; set; }
        public string VoucherNumber { get; set; }
        public DateTime VoucherDate { get; set; }
        public string VoucherType { get; set; } // "GENPAY" or "GENREC"
        public string Narration { get; set; }
        public int CompanyID { get; set; }
        public int BranchID { get; set; }
        public int FinYearID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }

        // General Payment/Receipt specific fields
        public int LedgerID { get; set; }         // Target ledger (e.g., Rent Expense)
        public string LedgerName { get; set; }
        public int CashBankLedgerID { get; set; } // Cash/Bank account ledger
        public string CashBankLedgerName { get; set; }
        public decimal Amount { get; set; }
        public string ReferenceNo { get; set; }   // Reference/Cheque/Transaction No
    }
}
