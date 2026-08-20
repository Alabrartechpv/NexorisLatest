using System;
using System.Collections.Generic;

namespace Nexoris.SyncService.Models
{
    public class BatchSyncRequest
    {
        public string BatchId { get; set; }
        public int BranchId { get; set; }
        public DateTime SentUtc { get; set; }
        public List<TransactionSyncDto> Transactions { get; set; }

        public BatchSyncRequest()
        {
            BatchId = Guid.NewGuid().ToString();
            SentUtc = DateTime.UtcNow;
            Transactions = new List<TransactionSyncDto>();
        }
    }

    public class TransactionSyncDto
    {
        public Guid TransactionGuid { get; set; }
        public string EntityType { get; set; }
        public string Operation { get; set; } // CREATE, UPDATE, CANCEL
        public DateTime OccurredUtc { get; set; }

        public SMasterSyncDto SMaster { get; set; }
        public List<SDetailsSyncDto> SDetails { get; set; }
        public List<VoucherSyncDto> Vouchers { get; set; }

        public TransactionSyncDto()
        {
            EntityType = "SALES";
            Operation = "CREATE";
            OccurredUtc = DateTime.UtcNow;
            SDetails = new List<SDetailsSyncDto>();
            Vouchers = new List<VoucherSyncDto>();
        }
    }

    public class SMasterSyncDto
    {
        public long BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }
        public int CounterId { get; set; }
        public string CustomerName { get; set; }
        public int? LedgerID { get; set; }
        public int? PaymodeId { get; set; }
        public string PaymodeName { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmt { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal NetAmount { get; set; }
        public int? UserId { get; set; }
        public string Status { get; set; }

        public SMasterSyncDto()
        {
            CustomerName = string.Empty;
            PaymodeName = string.Empty;
            Status = "PAID";
        }
    }

    public class SDetailsSyncDto
    {
        public int SlNO { get; set; }
        public long ItemId { get; set; }
        public string Barcode { get; set; }
        public string ItemName { get; set; }
        public decimal Qty { get; set; }
        public decimal Packing { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? TaxAmt { get; set; }
        public decimal TotalAmount { get; set; }
        public int? UnitId { get; set; }

        public SDetailsSyncDto()
        {
            Barcode = string.Empty;
            ItemName = string.Empty;
            Packing = 1.0m;
        }
    }

    public class VoucherSyncDto
    {
        public long? BranchVoucherId { get; set; }
        public int? LedgerID { get; set; }
        public string LedgerName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Narration { get; set; }

        public VoucherSyncDto()
        {
            LedgerName = string.Empty;
            Narration = string.Empty;
        }
    }

    public class BatchSyncResponse
    {
        public string BatchId { get; set; }
        public DateTime ProcessedUtc { get; set; }
        public List<SyncItemResult> Results { get; set; }

        public BatchSyncResponse()
        {
            BatchId = string.Empty;
            Results = new List<SyncItemResult>();
        }
    }

    public class SyncItemResult
    {
        public Guid TransactionGuid { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public string Status { get; set; } // Synced, AlreadySynced, Failed
        public long? CentralTransactionId { get; set; }
        public string ErrorMessage { get; set; }

        public SyncItemResult()
        {
            EntityType = string.Empty;
            EntityId = string.Empty;
            Status = string.Empty;
        }
    }

    public class SyncQueueItem
    {
        public long SyncID { get; set; }
        public int BranchId { get; set; }
        public string EntityType { get; set; }
        public string EntityID { get; set; }
        public string Operation { get; set; }
        public Guid TransactionGuid { get; set; }
        public string Status { get; set; }
        public int RetryCount { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime CreatedDate { get; set; }

        public SyncQueueItem()
        {
            EntityType = string.Empty;
            EntityID = string.Empty;
            Operation = string.Empty;
            Status = string.Empty;
        }
    }
}
