using System;
using System.Collections.Generic;

namespace ModelClass.Report
{
    /// <summary>
    /// Represents a single line item in the Trial Balance report
    /// </summary>
    public class TrialBalanceLineItem
    {
        public int LedgerID { get; set; }
        public string LedgerName { get; set; }
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public string GroupType { get; set; }
        public decimal OpeningDebit { get; set; }
        public decimal OpeningCredit { get; set; }
        public decimal TransactionDebit { get; set; }
        public decimal TransactionCredit { get; set; }
        public decimal ClosingDebit { get; set; }
        public decimal ClosingCredit { get; set; }
    }

    /// <summary>
    /// Summary totals for the Trial Balance report
    /// </summary>
    public class TrialBalanceSummary
    {
        public decimal TotalOpeningDebit { get; set; }
        public decimal TotalOpeningCredit { get; set; }
        public decimal TotalTransactionDebit { get; set; }
        public decimal TotalTransactionCredit { get; set; }
        public decimal TotalClosingDebit { get; set; }
        public decimal TotalClosingCredit { get; set; }
        public decimal Difference { get; set; }
    }

    /// <summary>
    /// Complete Trial Balance report data
    /// </summary>
    public class TrialBalanceReport
    {
        public List<TrialBalanceLineItem> LineItems { get; set; }
        public TrialBalanceSummary Summary { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public TrialBalanceReport()
        {
            LineItems = new List<TrialBalanceLineItem>();
            Summary = new TrialBalanceSummary();
        }
    }
}
