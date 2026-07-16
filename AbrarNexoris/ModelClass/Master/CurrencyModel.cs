using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ModelClass.Master
{
    public class CurrencyModel
    {
       public int CurrencyID { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencySymbol { get; set; }
        public bool AmntInMillions { get; set; }
        public int DecimalPlace { get; set; }
        public string CurrencyUnit { get; set; }
        public int CountryID { get; set; }
    }

    public class CurrencyDDLGRID
    {
        public IEnumerable<CurrencyModel> List { get; set; }
    }

}
