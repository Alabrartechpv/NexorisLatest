using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Accounts
{
    public class AccountHead
    {
        public string AcHeadName { get; set; }
        public int AcHeadId { get; set; }

    }
    public class AccoundHeadDDLGrid
    {
        public IEnumerable<AccountHead> List { get; set; }
    }

    public class AccountGroupHead
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public int BranchID { get; set; }
        public int GroupCategoryID { get; set; }
        public int ParentGroupId { get; set; }
        public string GroupType { get; set; }
        public string GroupUnder { get; set; }
    }

    public class AccountGroupHeadDDL
    {
        public IEnumerable<AccountGroupHead> List { get; set; }
    }

}
