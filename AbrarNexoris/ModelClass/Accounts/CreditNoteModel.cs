using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Accounts
{
    /// <summary>
    /// Model for Credit Note invoice information (for grid display)
    /// </summary>
    public class CreditNoteInfo
    {
        public Int64 BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public double InvoiceAmount { get; set; }
        public double CreditAmount { get; set; }
        public double Balance { get; set; }
    }

    /// <summary>
    /// Lifecycle status for a credit note.
    /// Open     = no credit applied yet (full amount still available).
    /// Partial  = some credit applied, remainder still available.
    /// Closed   = all credit has been applied or refunded.
    /// </summary>
    public enum CreditNoteStatus
    {
        Open,
        Partial,
        Closed
    }

    /// <summary>
    /// Credit Note Master model - stores header information
    /// </summary>
    public class CreditNoteMaster
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public int VoucherId { get; set; }
        public DateTime VoucherDate { get; set; }
        public int CustomerLedgerId { get; set; }
        public string CustomerName { get; set; }
        public int SReturnNo { get; set; }
        public string InvoiceNo { get; set; }

        // ── Core amount ────────────────────────────────────────────────────────
        public double CreditAmount { get; set; }

        // ── Phase 2: Credit lifecycle tracking ────────────────────────────────
        /// <summary>Total credit already applied to invoices or refunded.</summary>
        public double AppliedAmount { get; set; }

        /// <summary>Credit still available for the customer (CreditAmount - AppliedAmount).</summary>
        public double RemainingAmount => CreditAmount - AppliedAmount;

        /// <summary>
        /// Lifecycle status derived from the amounts.
        /// Stored as a string in the DB ("Open" / "Partial" / "Closed").
        /// </summary>
        public string Status
        {
            get => ComputeStatus().ToString();
            set { /* setter kept for model-binding / ORM compatibility */ }
        }

        /// <summary>Strongly-typed status — use this in business logic.</summary>
        public CreditNoteStatus CreditStatus => ComputeStatus();

        private CreditNoteStatus ComputeStatus()
        {
            if (AppliedAmount <= 0)
                return CreditNoteStatus.Open;
            if (AppliedAmount >= CreditAmount)
                return CreditNoteStatus.Closed;
            return CreditNoteStatus.Partial;
        }
        // ──────────────────────────────────────────────────────────────────────

        public int PaymentMethodLedgerId { get; set; }
        public string PaymentMethod { get; set; }
        public string Narration { get; set; }
        public int UserId { get; set; }
        public bool CancelFlag { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Credit Note Details model - stores bill-wise credit adjustments
    /// </summary>
    public class CreditNoteDetails
    {
        public int Id { get; set; }
        public int CreditNoteMasterId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public int BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public double BillAmount { get; set; }
        public double OldBillAmount { get; set; }
        public double CreditAmount { get; set; }
        public double OldCreditAmount { get; set; }
        public double BalanceAmount { get; set; }
        public bool CancelFlag { get; set; }
    }

    /// <summary>
    /// Grid container for credit note invoices
    /// </summary>
    public class CreditNoteInfoGrid
    {
        public IEnumerable<CreditNoteInfo> InvoiceList { get; set; }
    }
}
