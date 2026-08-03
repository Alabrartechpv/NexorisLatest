using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass
{
    public class BaseModels
    {
    }
    public class Requestfrm
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int BranchId { get; set; }
    }

    public class CustomerDDl
    {
        public int LedgerID { get; set; }
        public String LedgerName { get; set; }
    }
    public class CustomerDDlGrid
    {
        public IEnumerable<CustomerDDl> List { get; set; }
    }
    public class PaymodeDDl
    {
        public int PayModeID { get; set; }
        public string PayModeName { get; set; }
        public int LedgerID { get; set; }
    }
    public class PaymodeDDlGrid
    {
        public IEnumerable<PaymodeDDl> List { get; set; }
    }

    public class VendorDDLG
    {
        public int LedgerID { get; set; }
        public string LedgerName { get; set; }
    }

    public class VendorDDLGrids
    {
        public IEnumerable<VendorDDLG> List { get; set; }
    }


    public class PriceLevelDDl
    {
        public int PriceLevelId { get; set; }
        public string PriceLevel { get; set; }
    }
    public class PriceLevelDDlGrid
    {
        public IEnumerable<PriceLevelDDl> List { get; set; }
    }
    public class ItemDDl
    {
        //public string SLNO { get; set; }
        public int ItemId { get; set; }
        public string BarCode { get; set; }
        public string Description { get; set; }
        public Double Cost { get; set; }
        public int UnitId { get; set; }
        public string Unit { get; set; }
        public Double Packing { get; set; }
        //public Double Cost { get; set; }
        public Double MarginPer { get; set; }
        public Double MarginAmt { get; set; }
        public Double TaxPer { get; set; }
        public Double TaxAmt { get; set; }
        public string TaxType { get; set; } = "excl"; // Tax type: "incl" or "excl"
        public Double RetailPrice { get; set; }
        public Double WholeSalePrice { get; set; }
        public Double CreditPrice { get; set; }
        public Double CardPrice { get; set; }
        public Double MRP { get; set; }               // Maximum Retail Price
        public Double StaffPrice { get; set; }        // Staff Price
        public Double MinPrice { get; set; }          // Minimum Price
        public double Stock { get; set; }
        public string IsBaseUnit { get; set; }        // 'Y' or 'N' from PriceSettings
        public double OrderedStock { get; set; }      // OrderedStock from PriceSettings
        public string ItemStatus { get; set; } = "Active";
        public string StatusReason { get; set; }
        public DateTime? StatusDate { get; set; }
        public bool BlockSale { get; set; }
        public bool BlockPurchase { get; set; }
    }
    public class ItemDDlGrid
    {
        public IEnumerable<ItemDDl> List { get; set; }
    }
    public class ItemStatusRuleInfo
    {
        public int ItemId { get; set; }
        public string StatusName { get; set; } = "Active";
        public string StatusReason { get; set; }
        public DateTime? StatusDate { get; set; }
        public bool BlockSale { get; set; }
        public bool BlockPurchase { get; set; }
    }
    public class SalesPersonDDl
    {
        public string UserName { get; set; }
    }
    public class SalesPersonDDlGrid
    {
        public IEnumerable<SalesPersonDDl> List { get; set; }
    }
    public class Actioncls
    {
        public static string gNoOfDecimals { get; set; }
        public static string FormattedAmount(float mAmount, string mNoOfDecimals)
        {

            if (mNoOfDecimals == "2")
                return string.Format("{0:0.00}", mAmount);
            else
                return string.Format("{0:0.000}", mAmount);
        }
    }



    public class CustomerTypeDDL
    {
        public int Id { get; set; }
        public string PriceLevel { get; set; }
    }

    public class CustomerTypeDDlGrid
    {
        public IEnumerable<CustomerTypeDDL> List { get; set; }
    }
    public class GetHoldBill
    {
        public Int64 BillNo { get; set; }
        public string CustomerName { get; set; }
        public double NetAmount { get; set; }
        public string SaleType { get; set; }
        public int BranchId { get; set; }
        public int UserId { get; set; }
        public int CounterId { get; set; }
        public DateTime BillDate { get; set; }
    }
    public class GetHoldBillGrid
    {
        public IEnumerable<GetHoldBill> List { get; set; }
    }

    public class InvoicePrnt
    {
        public Int64 BillNo { get; set; }
        public int BranchId { get; set; }
        public DateTime BillDate { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public double Qty { get; set; }
        public double UnitPrice { get; set; }
        public double Amount { get; set; }
        public double DiscountAmount { get; set; }
        public double TaxAmt { get; set; }
        public double TaxPer { get; set; }
        public string Barcode { get; set; }
        public string _Operations { get; set; }
        public int CounterId { get; set; }
        public string CounterName { get; set; }
    }
    public class InvoicePrntGrid
    {
        public IEnumerable<InvoicePrnt> List { get; set; }
    }
    public class billPara
    {
        public Int64 BillNo { get; set; }
        public int BranchId { get; set; }
        public string _Operations { get; set; }
    }

    public class ItemPicture
    {
        public string ItemName { get; set; }
        public int ItemId { get; set; }
        public string Unit { get; set; }
        public byte[] Photo { get; set; } = null;
    }
    public class ItemPictureGrid
    {
        public IEnumerable<ItemPicture> list { get; set; }
    }




}
