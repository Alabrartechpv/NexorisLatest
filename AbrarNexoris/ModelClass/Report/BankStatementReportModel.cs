using System;
using System.Collections.Generic;

namespace ModelClass.Report
{
    /// <summary>
    /// Represents a single row in the Bank Statement Report.
    /// Each row is a bank-related transaction from Sales, Purchase, Vendor Payment, or Customer Receipt.
    /// </summary>
    public class BankStatementTransaction
    {
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public string PartyName { get; set; }
        public string BillVoucherNo { get; set; }
        public decimal MoneyIn { get; set; }
        public decimal MoneyOut { get; set; }
        public string PaymentMethod { get; set; }
        public string Reference { get; set; }
    }

    /// <summary>
    /// Summary totals for the Bank Statement Report.
    /// </summary>
    public class BankStatementSummary
    {
        public decimal TotalMoneyIn { get; set; }
        public decimal TotalMoneyOut { get; set; }
        public decimal NetAmount { get; set; }
    }

    /// <summary>
    /// Complete Bank Statement Report result containing transactions and summary.
    /// </summary>
    public class BankStatementReportModel
    {
        public List<BankStatementTransaction> Transactions { get; set; }
        public BankStatementSummary Summary { get; set; }

        public BankStatementReportModel()
        {
            Transactions = new List<BankStatementTransaction>();
            Summary = new BankStatementSummary();
        }
    }
}
