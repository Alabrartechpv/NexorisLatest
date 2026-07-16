using System;
using System.Collections.Generic;

namespace ModelClass.TransactionModels
{
    class SalesReturnModel
    {
    }
    public class SalesReturn
    {

        public int CompanyId { get; set; }
        public int FinYearId { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public int SReturnNo { get; set; }
        public DateTime SReturnDate { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public Int64 LedgerID { get; set; }
        public string CustomerName { get; set; }
        public int PaymodeID { get; set; }
        public string Paymode { get; set; }
        public int PaymodeLedgerID { get; set; }
        public double SubTotal { get; set; }
        public double SpDisPer { get; set; }
        public double SpDsiAmt { get; set; }
        public double BillDiscountPer { get; set; }
        public double BillDiscountAmt { get; set; }
        public double TaxPer { get; set; }
        public double TaxAmt { get; set; }
        public double Frieght { get; set; }
        public double GrandTotal { get; set; }
        public bool CancelFlag { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string TaxType { get; set; }
        public double RoundOff { get; set; }
        public double CessPer { get; set; }
        public double CessAmt { get; set; }
        public Int64 VoucherID { get; set; }
        public Int64 BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public int EmpID { get; set; }
        public int StateId { get; set; }
        public double Freight { get; set; }
        public double FreightProfit { get; set; }
        public int PaymodeId { get; set; }
        public string PaymodeName { get; set; }
        public int PaymodeLedgerId { get; set; }
        public double KFCessPer { get; set; }
        public double KFCessAmt { get; set; }
        public double DiscountPer { get; set; }
        public double DiscountAmt { get; set; }
        public bool RoundOffFlag { get; set; }
        public double RoundAmount { get; set; }
        public double NetAmount { get; set; }
        public double TenderedAmount { get; set; }
        public double Balance { get; set; }
        public Int64 CurrencyId { get; set; }
        public string CurrencySymbol { get; set; }
        public Int64 OrderNo { get; set; }
        public int TransporterLedgerId { get; set; }
        public double ReceivedAmount { get; set; }
        public float CalAfterTax { get; set; }
        public string CurSymbol { get; set; }
        public int SeriesId { get; set; }
        public string _Operation { get; set; }
        public string Status { get; set; }
    }
    public class SalesReturnDetails
    {
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }
        public int BranchID { get; set; }
        public string BranchName { get; set; }
        public int SReturnNo { get; set; }
        public DateTime SReturnDate { get; set; }
        public string InvoiceNo { get; set; }
        public int SlNo { get; set; }
        //public int ItemID { get; set; }
        public string Description { get; set; }
        public string Barcode { get; set; }
        public string Unit { get; set; }
        public double Qty { get; set; }

        public string ItemName { get; set; }
        public int UnitId { get; set; }
        public string BaseUnit { get; set; }
        public double Packing { get; set; }
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
        public float BaseAmount { get; set; } = 0; // Taxable value before tax for GST compliance
        public int SeriesID { get; set; }
        public string Reason { get; set; }
        public double Amount { get; set; }
        public string _Operation { get; set; }
        public int CounterId { get; set; }
        public Int64 BillNo { get; set; }
        public Int64 ItemId { get; set; }
        public DateTime Expiry { get; set; }
        public double UnitPrice { get; set; }
        public double DiscountPer { get; set; }
        public double DiscountAmount { get; set; }
        public double MarginPer { get; set; }
        public double MarginAmt { get; set; }
        public double CessPer { get; set; }
        public double CessAmt { get; set; }
        public double KFCessPer { get; set; }
        public double KFCessAmt { get; set; }
        public double TotalAmount { get; set; }
        public bool CancelFlag { get; set; }
        public double MRP { get; set; }
        public double ReturnQty { get; set; }
        public double ReturnedQty { get; set; }
    }

    public class SalesReturnDetailsGrid
    {
        public IEnumerable<SalesReturnDetails> List { get; set; }
    }
    public class SalesDDl
    {
        public Int64 BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public string CustomerName { get; set; }
        public Int64 LedgerID { get; set; }
        public int PaymodeId { get; set; }
        public double NetAmount { get; set; }
        public string PaymodeName { get; set; }

    }
    public class SalesDDlGrid
    {
        public IEnumerable<SalesDDl> List { get; set; }
    }

    public class SRgetAll
    {
        public int SReturnNo { get; set; }
        public DateTime SReturnDate { get; set; }
        public string InvoiceNo { get; set; }
        public string CustomerName { get; set; }
        public Int64 LedgerID { get; set; }
        public double GrandTotal { get; set; }
        public int PaymodeID { get; set; }
        public string Paymode { get; set; }
    }
    //public class SRgetAllGrid
    //{
    //    public IEnumerable<SRgetAll> list
    //}

}
