using System;

namespace ModelClass.TransactionModels
{
    /// <summary>
    /// Represents a queued transaction record awaiting synchronization with Head Office.
    /// </summary>
    public class SyncQueueModel
    {
        public long SyncId { get; set; }
        public int BranchId { get; set; }
        public string EntityType { get; set; } // 'SALES', 'SALES_RETURN', 'PURCHASE', 'VOUCHER'
        public string EntityId { get; set; }   // BillNo or Document No
        public Guid TransactionGuid { get; set; }
        public string Operation { get; set; }  // 'CREATE', 'UPDATE', 'CANCEL'
        public int Version { get; set; } = 1;
        public string Status { get; set; } = "PENDING"; // 'PENDING', 'IN_FLIGHT', 'SYNCED', 'FAILED'
        public int RetryCount { get; set; } = 0;
        public int MaxRetries { get; set; } = 10;
        public string ErrorMessage { get; set; }
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? LastAttemptUtc { get; set; }
        public DateTime? SyncedUtc { get; set; }
    }
}
