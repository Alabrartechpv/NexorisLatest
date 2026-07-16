using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Accounts
{
    public class VendorPayment
    {
        

    }
    public class VendorPurchasedInfo
    {
        //public int CompanyId { get; set; }
        //public int BranchId { get; set; }
        //public int FinYearId { get; set; }
        public int BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public double InvoiceAmount { get; set; }
        public double AdjustedAmount { get; set; }
        public double Balance { get; set; }
       // public double ReceiptAmount { get; set; }

    }

    public class VendorPaymentMaster
    {
        public int PaymentMasterId { get; set; }
        public int CompanyId { get; set; }
        public int VendorLedgerId { get; set; }
        public string VendorName { get; set; }
        public string PaymentMethod { get; set; }
        public int PaymentMethodLedgerId { get; set; }
        public string SalesPerson { get; set; }
        public decimal TotalPaymentAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string VoucherNo { get; set; }
        public int VoucherId { get; set; }
        public string Remarks { get; set; }
        public int BranchId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }

    }
    public class VendorPaymentDetails
    {
        public int PaymentDetailsId { get; set; }
        public int PaymentMasterId { get; set; }
        public string BillNo { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal AdjustedAmount { get; set; }
        public decimal Balance { get; set; }
        public DateTime BillDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
    }

    public class VoucherEntry
    {
        public int VoucherId { get; set; }
        public string VoucherNo { get; set; }
        public int LedgerId { get; set; }
        public string LedgerName { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public DateTime VoucherDate { get; set; }
        public string Narration { get; set; }
        public int BranchId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
    }

    public class VendorPurchasedInfoGrid
    {
       public IEnumerable<VendorPurchasedInfo> ListPurchasedInfo { get; set; }
    }

}
