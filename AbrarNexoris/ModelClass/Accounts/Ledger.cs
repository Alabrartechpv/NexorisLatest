using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Accounts
{
    public class Ledger
    {
        public int LedgerID { get; set; }
        public string LedgerName { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public int GroupID { get; set; }
        public int BranchID { get; set; }
        public decimal OpnDebit { get; set; }
        public decimal OpnCredit { get; set; }
        public decimal Balance { get; set; }
        public int CompanyID { get; set; }
        public bool? ProvideBankDetails { get; set; }
        public bool? GstApplicable { get; set; }
        public bool? VatApplicable { get; set; }
        public bool? InventoryValuesAffected { get; set; }
        public bool? MaintainBillWiseDetails { get; set; }
        public bool? PriceLevelApplicable { get; set; }
        public bool? ActivateInterestCalculations { get; set; }
    }

    public class LedgerResult
    {
        public IEnumerable<Ledger> List { get; set; }
    }
}
