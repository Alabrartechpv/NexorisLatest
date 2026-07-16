using System;
using System.Collections.Generic;

namespace ModelClass.TransactionModels
{
    public class PurchaseOrderMaster
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
        public int CurrencyID { get; set; }
        public string CurSymbol { get; set; }
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

        public string ReferenceNo { get; set; }
        public string ShipTo1 { get; set; }
        public string ShipTo2 { get; set; }
        public string ShipTo3 { get; set; }
        public string ShipTo4 { get; set; }
        public string Telephone { get; set; }
        public string OrderBy { get; set; }
        public DateTime ExpectedDate { get; set; }
        public string CreditPeriodTerm { get; set; }
        public string ApprovedBy { get; set; }
        public string CreatedBy { get; set; }
        public string ShipVia { get; set; }
        public string FobPoints { get; set; }
    }

    public class PurchaseOrderDetail
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
        public string RowRemark { get; set; }
        public double BaseQty { get; set; }
        public double BaseQtyReceived { get; set; }
        public double TotalSst { get; set; }
    }

    public class PurchaseOrderGrid
    {
        public IEnumerable<PurchaseOrderMaster> ListPurchaseOrder { get; set; }
        public IEnumerable<PurchaseOrderDetail> ListPurchaseOrderDetails { get; set; }
    }

    public class PurchaseOrderLookupItem
    {
        public int Poid { get; set; }
        public string BranchName { get; set; }
        public int PONo { get; set; }
        public DateTime PODate { get; set; }
        public double GrandTotal { get; set; }
    }

    public class PurchaseOrderLoadResult
    {
        public PurchaseOrderMaster Master { get; set; }
        public List<PurchaseOrderDetail> Details { get; set; }
    }
}
