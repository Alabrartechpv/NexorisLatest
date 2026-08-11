using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.TransactionModels
{
    public class PurchaseModel
    {
    }

    public class PurchaseMaster
    {
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public int PurchaseNo { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int LedgerID { get; set; }
        public string VendorName { get; set; }
        public int PaymodeID { get; set; }
        public string Paymode { get; set; }
        public int PaymodeLedgerID { get; set; }
        public int CreditPeriod { get; set; }
        public double SubTotal { get; set; }
        public double SpDisPer { get; set; }
        public double SpDsiAmt { get; set; }
        public double BillDiscountPer { get; set; }
        public double BillDiscountAmt { get; set; }
        public double TaxPer { get; set; }
        public double TaxAmt { get; set; }
        public double Frieght { get; set; }
        public double ExpenseAmt { get; set; }
        public double OtherExpAmt { get; set; }
        public double GrandTotal { get; set; }
        public bool CancelFlag { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string TaxType { get; set; }
        public string Remarks { get; set; }
        public double RoundOff { get; set; }
        public double CessPer { get; set; }
        public double CessAmt { get; set; }
        public double CalAfterTax { get; set; }
        public int CurrencyID { get; set; } = 1;
        public string CurSymbol { get; set; } = "RM";
        public int SeriesID { get; set; }
        public int VoucherID { get; set; }
        public bool IsSyncd { get; set; }
        public bool Paid { get; set; }
        public int Pid { get; set; }
        public int POrderMasterId { get; set; }
        public double PayedAmount { get; set; }
        public string BilledBy { get; set; }
        public string TrnsType { get; set; }
        public double NetTotal { get; set; }
        public string _Operation { get; set; }
    }
    public class PurchaseDetails
    {
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }
        public int BranchID { get; set; }
        public string BranchName { get; set; }
        public int PurchaseNo { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string InvoiceNo { get; set; }
        public int SlNo { get; set; }
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public int UnitId { get; set; }
        public string Unit { get; set; }
        public string BaseUnit { get; set; }
        public double Packing { get; set; }
        public double Qty { get; set; }
        public double Free { get; set; }
        public double Cost { get; set; }
        public double DisPer { get; set; }
        public double DisAmt { get; set; }
        public double SalesPrice { get; set; }
        public double TaxPer { get; set; }
        public double TaxAmt { get; set; }
        public double TotalSP { get; set; }
        public double OriginalCost { get; set; }
        public double OriginalSP { get; set; }
        public bool IsExpiry { get; set; }
        public string TaxType { get; set; }
        public int SeriesID { get; set; }
        public double CessAmt { get; set; }
        public double CessPer { get; set; }
        public bool IsSyncd { get; set; }

        public double OldQty { get; set; }
        public double RetailPrice { get; set; }
        public double WholeSalePrice { get; set; }
        public double CreditPrice { get; set; }

        public string Barcode { get; set; }
        public double SingleItemCost { get; set; }
        public string TrnsType { get; set; }
        public string _Operation { get; set; }

    }

    public class PurchaseInvoiceGrid
    {

        public IEnumerable<PurchaseMaster> Listpmaster { get; set; }
        public IEnumerable<PurchaseDetails> Listpdetails { get; set; }

    }

    public class PurchaseGrid
    {
        public IEnumerable<PurchaseMaster> ListPurchase { get; set; }
        public IEnumerable<PurchaseDetails> ListPurchaseDetails { get; set; }
    }

    public class PurchaseStockUpdateOnPricesettings
    {
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }
        public int BranchID { get; set; }
        public int ItemID { get; set; }
        public int UnitId { get; set; }
        public double Qty { get; set; }
        public double Free { get; set; }
        public double Packing { get; set; }
        public double OldQty { get; set; }
        public double RetailPrice { get; set; }
        public double WholeSalePrice { get; set; }
        public double CreditPrice { get; set; }
        public double SingleItemCost { get; set; }
        public string _Operation { get; set; }

        // Markdown fields to preserve during purchase operations
        public double MDRetailPrice { get; set; }
        public double MDWalkinPrice { get; set; }
        public double MDCreditPrice { get; set; }
        public double MDMrpPrice { get; set; }
        public double MDCardPrice { get; set; }
        public double MDStaffPrice { get; set; }
        public double MDMinPrice { get; set; }
    }
}
