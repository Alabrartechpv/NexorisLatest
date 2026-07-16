using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Master
{
   public class Users
    {
        public int UserID { get; set; }
        public int CompanyID { get; set; }
        public int BranchID { get; set; }
        public int UserLevelID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public bool IsDelete { get; set; }
        public string _Operation { get; set; }
    }
    public class UsersDDl
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
    }
    public class UserDDlGrid
    {
        public IEnumerable<UsersDDl> List { get; set; }
    }
    public class UserLevelDDl
    {
        public int UserLevelID { get; set; }
        public string UserLevel { get; set; }
    }
    public class UserLevelDDlGrid
    {
        public IEnumerable<UserLevelDDl> List { get; set; }
    }

}
