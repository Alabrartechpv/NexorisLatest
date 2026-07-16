using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Master
{
   public class TaxTypeModel
    {
    }
    public class TaxTypeDDL
    {
        public int ID { get; set; }
        public string TaxType { get; set; }
    }
    public class TaxTypeDDLGrid
    {
        public IEnumerable<TaxTypeDDL> List { get; set; }
    }

    public class TaxPerDDl
    {
        public int Id { get; set; }
        public decimal TaxPer { get; set; }
        
    }
    public class TaxPerDDlGrid
    {
        public IEnumerable<TaxPerDDl> List { get; set; }
    }
}
