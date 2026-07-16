using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Master
{
    public class HoldItemDetails
    {
        public string BillNo { get; set; }
        public int LedgerID { get; set; }
        public string CustomerName { get; set; }
        public double HoldQty { get; set; }
        public string Unit { get; set; }
    }

    public class HoldItemDetailsGrid
    {
        public IEnumerable<HoldItemDetails> List { get; set; }
    }
}