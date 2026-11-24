using System;

namespace ModelClass.Report
{
    public class DeletionAuditReportFilter
    {
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public DateTime FromDate { get; set; } = DateTime.Today;
        public DateTime ToDate { get; set; } = DateTime.Today;
        public string TransactionType { get; set; } = "ALL";
        public string UserFilter { get; set; } = "ALL";
        public string SearchText { get; set; } = string.Empty;
    }

    public class DeletionAuditReportRow
    {
        public int SlNo { get; set; }
        public string ModuleKey { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public string DocNo { get; set; } = string.Empty;
        public string RefNo { get; set; } = string.Empty;
        public DateTime DocDate { get; set; }
        public DateTime DeletedDate { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Qty { get; set; }
        public string DeletedBy { get; set; } = string.Empty;
        public string Counter { get; set; } = string.Empty;
        public string DeletionReason { get; set; } = string.Empty;
        public string SourceDetails { get; set; } = string.Empty;
    }
}
