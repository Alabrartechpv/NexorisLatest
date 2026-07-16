using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Master
{
   public class GroupModel
    {
    }
    public class Group
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string GroupName { get; set; }
        public Byte[] Photo { get; set; }
        /*public Byte[] PhotoByteArray { get; set; } = null;*/ // USES ONLY FOR STORED PROCEDURE WITH _OPERATION GETBYID
        public string _Operation { get; set; }

    }
    public class GroupDDL
    {
        public int Id { get; set; }
        public string GroupName { get; set; }

    }

    public class GroupDDlGrid
    {
        public IEnumerable<GroupDDL> List { get; set; }
    }

}
