using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Master
{
    public class UnitMasterModel
    {

    }
    public class UnitMaster
    {
        public int UnitID { get; set; }
        public string UnitName { get; set; }
        public string UnitSymbol { get; set; }
        public int UnitQuantityCode { get; set; }
        public double Packing { get; set; }
        public int NoOfDecimalPlaces { get; set; }
        public string UnitNameInBill { get; set; }
        public bool IsDelete { get; set; }
        public string _Operation { get; set; }

    }
    public class UnitDDL
    {
        public int UnitID { get; set; }
        public string UnitName { get; set; }
        public double Packing { get; set; }//31
    }
    public class UnitDDLGrid
    {
        public IEnumerable<UnitDDL> List { get; set; }
    }
    public class UnitQtyCodeDDL
    {
        public int ID { get; set; }
        public string UnitQuantityCode { get; set; }
    }
    public class UnitQtyCodeDDLGrid
    {
        public IEnumerable<UnitQtyCodeDDL> List { get; set; }
    }
    public class Unit
    {
        public int UnitID { get; set; }
        public string UnitName { get; set; }
        public int UnitSymbol { set; get; }
        public int UnitQuantityCode { get; set; }
        public float Packing { get; set; }
        public int NoOfDecimalPlaces { set; get; }
        public string UnitNameInBill { get; set; }
        //public string IsDelete { get; set; }
    }

    public class UnitDDlGrid
    {
        public IEnumerable<UnitDDL> List { get; set; }
    }

    /// <summary>
    /// Display-only model for the grid - only shows UnitID and UnitName
    /// </summary>
    public class UnitMasterDisplay
    {
        public int UnitID { get; set; }
        public string UnitName { get; set; }
    }
}
