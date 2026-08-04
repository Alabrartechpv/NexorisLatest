using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Master
{
    public class ItemTypeModel
    {
    }

    public class ItemType
    {
        public int Id { get; set; }
        public string ItemTypeName { get; set; }
        public bool IsDelete { get; set; }
        public bool IsDefault { get; set; }
        public string _Operation { get; set; }
    }
}
