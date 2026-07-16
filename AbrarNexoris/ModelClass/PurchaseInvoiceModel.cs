using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass
{
   public class PurchaseInvoiceModel
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
        public Boolean CancelFlag { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public int TaxType { get; set; }
        public string Remarks { get; set; }
        public double RoundOff { get; set; }
        public double CessPer { get; set; }
        public double CessAmt { get; set; }
        public double CalAfterTax { get; set; }
        public int CurrencyID { get; set; }
        public string CurSymbol { get; set; }
        public int SeriesID { get; set; }
        public int VoucherID { get; set; }
        public Boolean IsSyncd { get; set; }
        public Boolean Paid { get; set; }
        public int Pid { get; set; }
        public int POrderMasterId { get; set; }
        public double PayedAmount { get; set; }
        public string BilledBy { get; set; }

    }
}
