using System;

namespace ModelClass.Accounts
{
    public class ManualPartyBalanceEntry
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public string PartyType { get; set; }
        public string PartyName { get; set; }
        public string BalanceType { get; set; }
        public decimal Amount { get; set; }
        public decimal SettledAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public DateTime EntryDate { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class ManualPartyBalanceSettlement
    {
        public int Id { get; set; }
        public int ManualPartyBalanceId { get; set; }
        public decimal SettlementAmount { get; set; }
        public DateTime SettlementDate { get; set; }
        public string Remarks { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}