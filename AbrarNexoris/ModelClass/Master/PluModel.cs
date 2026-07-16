using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Master
{
    public class PluModel
    {
        public bool Selected { get; set; }
        public int ItemId { get; set; }
        public string Description { get; set; }
        public string BarCode { get; set; }
        public int ItemTypeId { get; set; }
        public decimal RetailPrice { get; set; }

        public PluModel()
        {
            Selected = true; // Default to selected
        }
    }
}
