using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.TransactionModels
{
    public class PReturnMaster
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public int PReturnNo { get; set; }
        public string PInvoice { get; set; }
        public DateTime PReturnDate { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public Int64 LedgerID { get; set; }
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
        public Int64 CurrencyID { get; set; }
        public string CurSymbol { get; set; }
        public int SeriesID { get; set; }
        public Int64 VoucherID { get; set; }
        public string TrnsType { get; set; }
        public string VoucherType { get; set; }
        public string _Operation { get; set; }
    }

    public class PReturnDetails
    {
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }
        public int BranchID { get; set; }
        public string BranchName { get; set; }
        public int PReturnNo { get; set; }
        public DateTime PReturnDate { get; set; }
        public string InvoiceNo { get; set; }
        public int SlNo { get; set; }
        public Int64 ItemID { get; set; }
        public string Description { get; set; }
        public int UnitId { get; set; }
        public bool BaseUnit { get; set; }
        public double Packing { get; set; }
        public bool IsExpiry { get; set; }
        public string BatchNo { get; set; }
        public DateTime? Expiry { get; set; }
        public double Qty { get; set; }
        public double TaxPer { get; set; }
        public double TaxAmt { get; set; }
        public string Reason { get; set; }
        public double Free { get; set; }
        public double Cost { get; set; }
        public double DisPer { get; set; }
        public double DisAmt { get; set; }
        public double SalesPrice { get; set; }
        public double OriginalCost { get; set; }
        public double TotalSP { get; set; }
        public double TotalAmount { get; set; }
        public double CessAmt { get; set; }
        public double CessPer { get; set; }
        public string _Operation { get; set; }
    }

    public class PReturnVoucher
    {
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }
        public int BranchID { get; set; }
        public Int64 VoucherID { get; set; }
        public string VoucherType { get; set; }
        public DateTime VoucherDate { get; set; }
        public string ReferenceNo { get; set; }
        public Int64 LedgerID { get; set; }
        public double Debit { get; set; }
        public double Credit { get; set; }
        public string Narration { get; set; }
        public bool CancelFlag { get; set; }
        public string _Operation { get; set; }
    }

    public class PReturnDetailsGrid
    {
        public IEnumerable<PReturnDetails> List { get; set; }
    }

    public class PReturnDDl
    {
        public int PReturnNo { get; set; }
        public DateTime PReturnDate { get; set; }
        public string VendorName { get; set; }
        public Int64 LedgerID { get; set; }
        public int PaymodeId { get; set; }
        public double NetAmount { get; set; }
        public string PaymodeName { get; set; }
    }

    public class PReturnDDlGrid
    {
        public IEnumerable<PReturnDDl> List { get; set; }
    }

    public class PReturnGetAll
    {
        public int PReturnNo { get; set; }
        public DateTime PReturnDate { get; set; }
        public string InvoiceNo { get; set; }
        public string VendorName { get; set; }
        public double GrandTotal { get; set; }
    }
}