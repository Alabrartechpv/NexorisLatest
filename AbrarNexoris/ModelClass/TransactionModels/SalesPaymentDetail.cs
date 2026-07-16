using System;

namespace ModelClass.TransactionModels
{
    /// <summary>
    /// Represents a single payment entry in a split payment transaction.
    /// Multiple SalesPaymentDetail records can exist for one sale to support split payments.
    /// </summary>
    public class SalesPaymentDetail
    {
        public int SPaymentId { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public long BillNo { get; set; }
        public int PaymodeId { get; set; }
        public string PaymodeName { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public DateTime CreatedDate { get; set; }
        public string _Operation { get; set; }
    }
}
