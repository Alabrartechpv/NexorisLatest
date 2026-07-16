using System;
using System.Collections.Generic;

namespace ModelClass.Report
{
    /// <summary>
    /// Represents a single line item in the Trading or Profit & Loss Account
    /// </summary>
    public class TradingPLLineItem
    {
        public int LedgerID { get; set; }
        public string LedgerName { get; set; }
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public string Category { get; set; }
        public string NormalBalance { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal NetBalance { get; set; }

        /// <summary>
        /// Returns the effective amount based on the normal balance direction
        /// </summary>
        public decimal EffectiveAmount
        {
            get
            {
                if (NormalBalance == "DEBIT")
                    return TotalDebit - TotalCredit;
                else
                    return TotalCredit - TotalDebit;
            }
        }
    }

    /// <summary>
    /// Summary totals for Trading & Profit/Loss Account
    /// </summary>
    public class TradingPLSummary
    {
        public decimal TotalSales { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal TotalDirectExpenses { get; set; }
        public decimal TotalDirectIncomes { get; set; }
        public decimal TotalStockInHand { get; set; }
        public decimal OpeningStock { get; set; }
        public decimal ClosingStock { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal TotalIndirectExpenses { get; set; }
        public decimal TotalIndirectIncomes { get; set; }
        public decimal NetProfit { get; set; }
    }

    /// <summary>
    /// Complete Trading & Profit/Loss Account report data
    /// </summary>
    public class TradingPLReport
    {
        public List<TradingPLLineItem> TradingItems { get; set; }
        public List<TradingPLLineItem> ProfitLossItems { get; set; }
        public TradingPLSummary Summary { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public TradingPLReport()
        {
            TradingItems = new List<TradingPLLineItem>();
            ProfitLossItems = new List<TradingPLLineItem>();
            Summary = new TradingPLSummary();
        }
    }
}
