using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Accounts
{
    /// <summary>
    /// Model for Debit Note invoice information (for grid display)
    /// </summary>
    public class DebitNoteInfo
    {
        public Int64 BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public double InvoiceAmount { get; set; }
        public double DebitAmount { get; set; }
        public double Balance { get; set; }
    }

    /// <summary>
    /// Debit Note Master model - stores header information
    /// </summary>
    public class DebitNoteMaster
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public int VoucherId { get; set; }
        public DateTime VoucherDate { get; set; }
        public int VendorLedgerId { get; set; }
        public string VendorName { get; set; }
        public int PReturnNo { get; set; }
        public string InvoiceNo { get; set; }
        public double DebitAmount { get; set; }
        public int PaymentMethodLedgerId { get; set; }
        public string PaymentMethod { get; set; }
        public string Narration { get; set; }
        public int UserId { get; set; }
        public bool CancelFlag { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Debit Note Details model - stores bill-wise debit adjustments
    /// </summary>
    public class DebitNoteDetails
    {
        public int Id { get; set; }
        public int DebitNoteMasterId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public int BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public double BillAmount { get; set; }
        public double OldBillAmount { get; set; }
        public double DebitAmount { get; set; }
        public double OldDebitAmount { get; set; }
        public double BalanceAmount { get; set; }
        public bool CancelFlag { get; set; }
    }

    /// <summary>
    /// Grid container for debit note invoices
    /// </summary>
    public class DebitNoteInfoGrid
    {
        public IEnumerable<DebitNoteInfo> InvoiceList { get; set; }
    }
}
