using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Master
{
   public class BrandModel
    {
    }
    public class Brand
    {
        public int Id { get; set; }
        public string BrandName { get; set; }
        public Byte[] Photo { get; set; }
        public string _Operation { get; set; }
    }

    public class BrandDDL
    {
        public int Id { get; set; }
        public string BrandName { get; set; }
        public Byte[] Photo { get; set; }
    }
    public class BrandDDLGrid
    {
        public IEnumerable<BrandDDL> List { get; set; }
    }
}
