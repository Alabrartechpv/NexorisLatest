using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Report
{
    /// <summary>
    /// Complete Item Report Data containing all result sets from stored procedure
    /// </summary>
    public class ItemReportData
    {
        public List<ItemTransactionModel> Transactions { get; set; } = new List<ItemTransactionModel>();
        public List<MobileOrderModel> MobileOrders { get; set; } = new List<MobileOrderModel>();
        public ItemDetailsModel ItemDetails { get; set; }
        public List<PriceSettingsModel> PriceSettings { get; set; } = new List<PriceSettingsModel>();
        public List<VendorListModel> Vendors { get; set; } = new List<VendorListModel>();
        public List<StockSummaryModel> StockSummary { get; set; } = new List<StockSummaryModel>();
        public List<PendingOrderModel> PendingOrders { get; set; } = new List<PendingOrderModel>();
    }

    /// <summary>
    /// Item Transaction Model - Result Set 1 (Transaction history)
    /// </summary>
    public class ItemTransactionModel
    {
        public string Operation { get; set; }
        public int BranchId { get; set; }
        public DateTime DT { get; set; }
        public string RefNo { get; set; }
        public int UnitId { get; set; }
        public decimal Qty { get; set; }
        public decimal Packing { get; set; }
        public string IsBaseUnit { get; set; }
        public decimal Cost { get; set; }
        public string Way { get; set; }  // IN or OUT
        public decimal UnitPrice { get; set; }
        public decimal Balance { get; set; }
        public string BranchName { get; set; }
        public string UnitName { get; set; }
        public string Account { get; set; }
        public int RefId { get; set; }
    }

    /// <summary>
    /// Mobile Order Model - Result Set 2 (Mobile orders)
    /// </summary>
    public class MobileOrderModel
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderNo { get; set; }
        public string UnitName { get; set; }
        public decimal Qty { get; set; }
        public decimal Packing { get; set; }
        public string IsBaseUnit { get; set; }
        public string OrderStatus { get; set; }
    }

    /// <summary>
    /// Item Details Model - Result Set 3 (Item master info)
    /// </summary>
    public class ItemDetailsModel
    {
        public string ItemName { get; set; }
        public string BrandName { get; set; }
        public string NameInLocalLanguage { get; set; }
        public byte[] PhotoByteArray { get; set; }
        public string GroupName { get; set; }
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public string Line { get; set; }
        public string RackName { get; set; }
        public string Row { get; set; }
    }

    /// <summary>
    /// Price Settings Model - Result Set 4 (Price settings)
    /// </summary>
    public class PriceSettingsModel
    {
        public string BranchName { get; set; }
        public int ItemId { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public string IsBaseUnit { get; set; }
        public decimal Cost { get; set; }
        public decimal TaxP { get; set; }
        public decimal CessP { get; set; }
        public decimal KFCP { get; set; }
        public decimal MRP { get; set; }
        public decimal Retail { get; set; }
        public decimal WholeSale { get; set; }
        public decimal Credit { get; set; }
        public decimal Card { get; set; }
    }

    /// <summary>
    /// Vendor List Model - Result Set 5 (Vendor list)
    /// </summary>
    public class VendorListModel
    {
        public string BranchName { get; set; }
        public string VendorName { get; set; }
    }

    /// <summary>
    /// Stock Summary Model - Result Set 6 (Stock summary)
    /// </summary>
    public class StockSummaryModel
    {
        public string BranchName { get; set; }
        public decimal OrderedStock { get; set; }
        public decimal AvailableStock { get; set; }
        public decimal Stock { get; set; }
    }

    /// <summary>
    /// Pending Order Model - Result Set 7 (Pending orders)
    /// </summary>
    public class PendingOrderModel
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderNo { get; set; }
        public string UnitName { get; set; }
        public decimal Qty { get; set; }
        public decimal Packing { get; set; }
        public string IsBaseUnit { get; set; }
        public string OrderStatus { get; set; }
        public string PlatForm { get; set; }
        public string CustomerName { get; set; }
    }
}
