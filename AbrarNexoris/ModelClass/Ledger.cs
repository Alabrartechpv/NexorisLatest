using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass
{
   public  class Ledger
    {
    }
    public class AccountLedgerDDL
    {
        public int Id { get; set; }
        public  string Name { get; set; }
        public string Balance { get; set; }
        public int GroupId { get; set; }
    }

    public class AccountLedgerDDLGrid
    {
     public  IEnumerable<AccountLedgerDDL> List { get; set; }
    }

    public class AccountLedgerDDLRequest
    {
        public int BranchId { get; set; }
        public string For { get; set; }
    }
}
