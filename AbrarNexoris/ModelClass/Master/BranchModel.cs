using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Master
{
    public class BranchModel
    {
    }
    public class BranchDDl
    {
        public int Id { get; set; }
        public string BranchName { get; set; }
    }
    public class BranchDDlGrid
    {
        public IEnumerable<BranchDDl> List { get; set; }
    }
    public class Branch
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string BranchName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public int FinYearId { get; set; }
        public string _Operation { get; set; }

    }
}
