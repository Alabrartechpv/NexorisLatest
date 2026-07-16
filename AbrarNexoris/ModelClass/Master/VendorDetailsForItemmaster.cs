using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Master
{
    public class VendorDetailsForItemmaster
    {
        public int LedgerID { get; set; }
        public string VendorName { get; set; }
        public double Cost { get; set; }
        public string Unit { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int PurchaseNo { get; set; }
        public string InvoiceNo { get; set; }
        public int Pid { get; set; }
    }
    public class VendorDetilsForItemGrid
    {
        public IEnumerable<VendorDetailsForItemmaster> ListVendorDetItem { get; set; }
    }


}
