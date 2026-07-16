using System;

namespace ModelClass.Report
{
    public class CombinedPartyBalanceReportRow
    {
        public string PartyType { get; set; }
        public string PartyName { get; set; }
        public decimal ManualBalance { get; set; }
        public decimal OutstandingBalance { get; set; }
        public decimal TotalBalance { get; set; }
        public int ManualEntryCount { get; set; }
        public int OutstandingEntryCount { get; set; }
        public DateTime? ManualLastDate { get; set; }
        public DateTime? OutstandingLastDate { get; set; }
        public string MatchStatus { get; set; }
    }

    public class CombinedPartyBalanceReportFilter
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string PartyType { get; set; }
        public string SearchText { get; set; }
        public bool OpenOnly { get; set; }
        public bool MatchedOnly { get; set; }
    }
}
