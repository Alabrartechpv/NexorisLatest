using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Accounts
{
    public class ChartOfAccount
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // "Group" or "Ledger"
        public int? ParentId { get; set; }
        public int GroupId { get; set; }
        public int? LedgerId { get; set; }
        public string Description { get; set; }
        public decimal Balance { get; set; }
        public int BranchId { get; set; }
        public string GroupUnder { get; set; }
        public string NodePath { get; set; }
        public int Level { get; set; }
    }

    public class ChartOfAccountNode
    {
        public ChartOfAccountNode()
        {
            Children = new List<ChartOfAccountNode>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // "Group" or "Ledger"
        public int? ParentId { get; set; }
        public int GroupId { get; set; }
        public int? LedgerId { get; set; }
        public string Description { get; set; }
        public decimal Balance { get; set; }
        public List<ChartOfAccountNode> Children { get; set; }
        public string NodePath { get; set; }
        public int Level { get; set; }
    }

    public class ChartOfAccountResult
    {
        public IEnumerable<ChartOfAccount> List { get; set; }
    }
}