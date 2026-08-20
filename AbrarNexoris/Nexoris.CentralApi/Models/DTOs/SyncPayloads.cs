using System;
using System.Collections.Generic;

namespace Nexoris.CentralApi.Models.DTOs
{
    public class BatchSyncRequest
    {
        public int BranchId { get; set; }
        public string BatchId { get; set; } = Guid.NewGuid().ToString();
        public List<TransactionSyncDto> Transactions { get; set; } = new List<TransactionSyncDto>();
    }

    public class TransactionSyncDto
    {
        public Guid TransactionGuid { get; set; }
        public string EntityType { get; set; } = "SALES"; // 'SALES', 'SALES_RETURN', etc.
        public string Operation { get; set; } = "CREATE";  // 'CREATE', 'UPDATE', 'CANCEL'
        public int Version { get; set; } = 1;
        public SMasterSyncDto SMaster { get; set; }
        public List<SDetailsSyncDto> SDetails { get; set; } = new List<SDetailsSyncDto>();
        public List<VoucherSyncDto> Vouchers { get; set; } = new List<VoucherSyncDto>();
    }

    public class SMasterSyncDto
    {
        public long BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public int? CompanyId { get; set; }
        public int? FinYearId { get; set; }
        public int? CounterId { get; set; }
        public string CustomerName { get; set; }
        public int? LedgerID { get; set; }
        public int? PaymodeId { get; set; }
        public string PaymodeName { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? DiscountAmt { get; set; }
        public decimal? TaxAmt { get; set; }
        public decimal NetAmount { get; set; }
        public int? UserId { get; set; }
        public string Status { get; set; }
    }

    public class SDetailsSyncDto
    {
        public int SlNO { get; set; }
        public long ItemId { get; set; }
        public string Barcode { get; set; }
        public string ItemName { get; set; }
        public decimal Qty { get; set; }
        public decimal Packing { get; set; } = 1.0m;
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? TaxAmt { get; set; }
        public decimal TotalAmount { get; set; }
        public int? UnitId { get; set; }
    }

    public class VoucherSyncDto
    {
        public long? BranchVoucherId { get; set; }
        public long? LedgerID { get; set; }
        public string LedgerName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Narration { get; set; }
    }

    public class BatchSyncResponse
    {
        public string BatchId { get; set; }
        public DateTime ProcessedUtc { get; set; } = DateTime.UtcNow;
        public List<SyncItemResult> Results { get; set; } = new List<SyncItemResult>();
    }

    public class SyncItemResult
    {
        public Guid TransactionGuid { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public string Status { get; set; } // 'Synced', 'AlreadySynced', 'Failed'
        public long? CentralTransactionId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
