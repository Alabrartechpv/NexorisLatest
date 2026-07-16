using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Accounts
{
   public class CustomerReceiptInofo
    {
        public Int64  BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public double InvoiceAmount { get; set; }
        public double AdjustedAmount { get; set; }
        //public double ReceivedAmount { get; set; }
        public double Balance { get; set; }
       // public double ReceiptAmount { get; set; }
       // public double OldReceiptAmount { get; set; }
       // public string MobileOrderStatus { get; set; }
    }

    public class CustomerReceiptMaster
    {
        public int ReceiptId { get; set; }
        public string CustomerName { get; set; }
        public int CustomerLedgerId { get; set; }
        public string PaymentMethod { get; set; }
        public string SalesPerson { get; set; }
        public decimal TotalReceivableAmount { get; set; }
        public decimal TotalReceiptAmount { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string VoucherNo { get; set; }
        public int VoucherId { get; set; }
        public int BranchId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CustomerReceiptDetails
    {
        public int ReceiptDetailId { get; set; }
        public int ReceiptMasterId { get; set; }
        public string BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal AdjustedAmount { get; set; }
        public decimal Balance { get; set; }
    }




    public class CustomerSalesInfoGrid
    {
       public IEnumerable<CustomerReceiptInofo> CustomerSalesList { get; set; }
    }

    
}
