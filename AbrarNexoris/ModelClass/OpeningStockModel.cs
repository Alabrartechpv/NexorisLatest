using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass
{
    public class OpeningStockModel
    {
        public int BranchId { get; set; }
        public int ItemId { get; set; }
        public string Description { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public int UnitId { get; set; }
        public string Unit { get; set; }
        public double Packing { get; set; }
        public double OpnStk { get; set; }  // Opening Stock Quantity
        public double OpeningCost { get; set; }  // Opening Cost
        public DateTime? OpnDate { get; set; }  // Opening Date
        public double OpnValue { get; set; }  // Opening Stock Value (calculated: OpnStk * OpeningCost)
        public string EffectOnlyCost { get; set; }  // From ItemMaster
    }
}
