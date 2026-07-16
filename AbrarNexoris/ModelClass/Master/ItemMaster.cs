using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Master
{
    public class ItemMaster
    {
    }
    public class ItemAlternativeBarcode
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string Barcode { get; set; }
        public string _Operation { get; set; }
    }
    public class ItemMasterPriceSettings
    {
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public int FinYearId { get; set; }
        public int ItemId { get; set; }
        public int UnitId { get; set; }
        public string Unit { get; set; }
        public double Packing { get; set; }
        public double Cost { get; set; }
        public double MarginPer { get; set; }
        public double MarginAmt { get; set; }
        public double RetailPrice { get; set; }
        public double WholeSalePrice { get; set; }
        public double CreditPrice { get; set; }
        public double CardPrice { get; set; }
        public double StaffPrice { get; set; }
        public double MinPrice { get; set; }
        public string AliasBarcode { get; set; }
        public double MRP { get; set; }
        public double Stock { get; set; }
        public double OrderedStock { get; set; }
        public double StockValue { get; set; }
        public double ReOrder { get; set; }
        public string BarCode { get; set; }
        public double OpnStk { get; set; }
        public double OpeningCost { get; set; }
        public double OpnValue { get; set; }
        public DateTime? OpnDate { get; set; }
        public string TaxType { get; set; }
        public double TaxPer { get; set; }
        public double TaxAmt { get; set; }
        public string IsBaseUnit { get; set; }
        public string Costing { get; set; } = "AVERAGE";
        public Byte[] Photo { get; set; }
        public Byte[] PhotoByteArray { get; set; } = null;
        public string _Operation { get; set; }

        // Markdown fields for each price type
        public double MDRetailPrice { get; set; }
        public double MDWalkinPrice { get; set; }
        public double MDCreditPrice { get; set; }
        public double MDMrpPrice { get; set; }
        public double MDCardPrice { get; set; }
        public double MDStaffPrice { get; set; }
        public double MDMinPrice { get; set; }
    }

    public class ItemMasterPriceSettingsDDL
    {
        public string Unit { get; set; }
        public float Packing { get; set; }
        public string BarCode { get; set; }
        public float ReOrder { get; set; }
        public float OpnStk { get; set; }
        public float Cost { get; set; }
        public float MarginAmt { get; set; }
        public float MRP { get; set; }
        public float RetailPrice { get; set; }
        public float WholeSalePrice { get; set; }
        public float CreditPrice { get; set; }
        public float CardPrice { get; set; }
        public string Costing { get; set; } = "AVERAGE";
        public Byte[] Photo { get; set; }
        public Byte[] PhotoByteArray { get; set; } = null;
        public string _Operation { get; set; }
    }

    public class ItemMasterPriceSettingsDDLGrid
    {
        public IEnumerable<ItemMasterPriceSettingsDDL> List { get; set; }
    }

    public class Item
    {
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public int ItemId { get; set; }
        public int ItemNo { get; set; }
        public string Description { get; set; }
        public string Barcode { get; set; }
        public int ItemTypeId { get; set; }
        public int VendorId { get; set; }
        public int BrandId { get; set; }
        public int GroupId { get; set; }
        public int CategoryId { get; set; }
        public int BaseUnitId { get; set; }
        public string ForCustomerType { get; set; }
        public string NameInLocalLanguage { get; set; }
        public string HSNCode { get; set; }
        public int Order_Cycle_Days { get; set; } = 30;
        public int Box_Quantity { get; set; } = 1;
        public bool Is_Perishable { get; set; }
        public string _Operation { get; set; }
        //public List<ItemMasterPriceSettingsDDLGrid> itmpricesettings { get; set; }
    }
    public class ItemTypeDDL
    {
        public int Id { get; set; }
        public string ItemType { get; set; }
    }

    public class ItemTypeDDlGrid
    {
        public IEnumerable<ItemTypeDDL> List { get; set; }
    }


    public class ItemlistDDl
    {
        public int ItemId { get; set; }
        public string Description { get; set; }
        public string GroupName { get; set; }
        public string CategoryName { get; set; }
        public string Barcode { get; set; }
    }
    public class ItemlistDDlGrid
    {
        public IEnumerable<ItemlistDDl> List { get; set; }
    }

    public class ItemDialogDDL
    {
        public int ItemId { get; set; }
        public string Description { get; set; }
        public string Barcode { get; set; }
        public string BarCode { get; set; }
        public string NameInLocalLanguage { get; set; }

    }
    public class ItemDialogDDLGrid
    {
        public IEnumerable<ItemDialogDDL> List { get; set; }
    }

    public class ItemGet
    {
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public int ItemId { get; set; }
        public string ItemNo { get; set; }
        public string Description { get; set; }
        public string Barcode { get; set; }
        public int ItemTypeId { get; set; }
        public int VendorId { get; set; }
        public int BrandId { get; set; }
        public int GroupId { get; set; }
        public int CategoryId { get; set; }
        public int BaseUnitId { get; set; }
        public string ForCustomerType { get; set; }
        public string NameInLocalLanguage { get; set; }
        public string HSNCode { get; set; }

        public string ItemType { get; set; }
        public string BrandName { get; set; }
        public string UnitName { get; set; }
        public string GroupName { get; set; }
        public string CategoryName { get; set; }
        public string PriceLevel { get; set; }
        public double HoldQty { get; set; }
        public int Order_Cycle_Days { get; set; } = 30;
        public int Box_Quantity { get; set; } = 1;
        public bool Is_Perishable { get; set; }
        public ItemMasterPriceSettings[] List { get; set; }
        public ItemAlternativeBarcode[] ListAlternativeBarcodes { get; set; }
        public VendorDetailsForItemmaster[] ListVendor { get; set; }
        public string _Operation { get; set; }
        //public int Stock { get; set; }
        //public List<ItemMasterPriceSettingsDDLGrid> itmpricesettings { get; set; }
    }

}
