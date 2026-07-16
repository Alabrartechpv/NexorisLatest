using System;
using System.Collections.Generic;

namespace ModelClass.TransactionModels
{
    /// <summary>
    /// Represents a single transaction line in the Bank Reconciliation grid.
    /// </summary>
    public class BankReconciliationItem
    {
        public long VoucherID { get; set; }
        public int SlNo { get; set; }
        public DateTime VoucherDate { get; set; }
        public string VoucherNumber { get; set; }
        public string VoucherType { get; set; }
        public string Particulars { get; set; }
        public string Narration { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public DateTime? ReconciliationDate { get; set; }
        public bool IsReconciled { get; set; }
    }

    /// <summary>
    /// Summary statistics for the Bank Reconciliation view.
    /// </summary>
    public class BankReconciliationSummary
    {
        public decimal BooksBalance { get; set; }
        public decimal UnclearedReceipts { get; set; }
        public decimal UnclearedPayments { get; set; }
        public decimal BankBalance { get; set; }
    }

    /// <summary>
    /// Combined result returned by the BankReconciliationRepository.
    /// </summary>
    public class BankReconciliationResult
    {
        public List<BankReconciliationItem> Items { get; set; } = new List<BankReconciliationItem>();
        public BankReconciliationSummary Summary { get; set; } = new BankReconciliationSummary();
    }
}
