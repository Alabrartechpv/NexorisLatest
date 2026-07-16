using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Master
{
   public class CountryModel
    {
    }
    public class Country
    {
        public int CountryID { get; set; }
        public string CountryName { get; set; }
        public bool IsDelete { get; set; }
        public DateTime? FinYearFrom { get; set; }
        public DateTime? FinYearTo { get; set; }
        public DateTime? BookFrom { get; set; }
        public DateTime? BookTo { get; set; }
        public int TaxTypeId { get; set; }
        public string _Operation { get; set; }

    }
    public class CountryDDL
    {
        public int CountryID { get; set; }
        public string CountryName { get; set; }

    }
    public class CountryDDLGrid
    {
        public IEnumerable<CountryDDL> List { get; set; }
    }
}
