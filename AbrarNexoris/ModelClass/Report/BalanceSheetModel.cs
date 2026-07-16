using System;
using System.Collections.Generic;

namespace ModelClass.Report
{
    /// <summary>
    /// Represents a single line item in the Balance Sheet
    /// </summary>
    public class BalanceSheetLineItem
    {
        public int LedgerID { get; set; }
        public string LedgerName { get; set; }
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public string ParentGroupName { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal ClosingBalance { get; set; }
    }

    /// <summary>
    /// Summary totals for the Balance Sheet Report
    /// </summary>
    public class BalanceSheetSummary
    {
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal TotalCapital { get; set; }
        public decimal NetProfitLoss { get; set; }
        public decimal Difference { get; set; }
    }

    /// <summary>
    /// Complete Balance Sheet report data
    /// </summary>
    public class BalanceSheetReport
    {
        public List<BalanceSheetLineItem> LiabilitiesItems { get; set; }
        public List<BalanceSheetLineItem> AssetsItems { get; set; }
        public BalanceSheetSummary Summary { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public BalanceSheetReport()
        {
            LiabilitiesItems = new List<BalanceSheetLineItem>();
            AssetsItems = new List<BalanceSheetLineItem>();
            Summary = new BalanceSheetSummary();
        }
    }
}
