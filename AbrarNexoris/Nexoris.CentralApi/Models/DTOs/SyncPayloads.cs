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
        public string EntityType { get; set; } = "SALES"; // 'SALES', 'PURCHASE', etc.
        public string Operation { get; set; } = "CREATE";  // 'CREATE', 'UPDATE', 'CANCEL'
        public int Version { get; set; } = 1;
        public SMasterSyncDto SMaster { get; set; }
        public List<SDetailsSyncDto> SDetails { get; set; } = new List<SDetailsSyncDto>();
        public PMasterSyncDto PMaster { get; set; }
        public List<PDetailsSyncDto> PDetails { get; set; } = new List<PDetailsSyncDto>();
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

    public class PMasterSyncDto
    {
        public int PurchaseNo { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public int? LedgerID { get; set; }
        public string VendorName { get; set; }
        public int? PaymodeID { get; set; }
        public string Paymode { get; set; }
        public int CreditPeriod { get; set; }
        public decimal SubTotal { get; set; }
        public decimal SpDisPer { get; set; }
        public decimal SpDsiAmt { get; set; }
        public decimal BillDiscountPer { get; set; }
        public decimal BillDiscountAmt { get; set; }
        public decimal TaxPer { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal Frieght { get; set; }
        public decimal ExpenseAmt { get; set; }
        public decimal OtherExpAmt { get; set; }
        public decimal GrandTotal { get; set; }
        public bool CancelFlag { get; set; }
        public int? UserID { get; set; }
        public string UserName { get; set; }
        public string TaxType { get; set; }
        public string Remarks { get; set; }
        public decimal RoundOff { get; set; }
        public decimal CessPer { get; set; }
        public decimal CessAmt { get; set; }
        public decimal CalAfterTax { get; set; }
        public int? CurrencyID { get; set; }
        public string CurSymbol { get; set; }
        public int SeriesID { get; set; }
        public decimal NetTotal { get; set; }
    }

    public class PDetailsSyncDto
    {
        public int SlNo { get; set; }
        public int ItemID { get; set; }
        public string Barcode { get; set; }
        public string ItemName { get; set; }
        public int? UnitId { get; set; }
        public string Unit { get; set; }
        public string BaseUnit { get; set; }
        public decimal Packing { get; set; } = 1.0m;
        public decimal Qty { get; set; }
        public decimal Free { get; set; }
        public decimal Cost { get; set; }
        public decimal DisPer { get; set; }
        public decimal DisAmt { get; set; }
        public decimal SalesPrice { get; set; }
        public decimal TaxPer { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal TotalSP { get; set; }
        public decimal? OriginalCost { get; set; }
        public decimal? OriginalSP { get; set; }
        public string TaxType { get; set; }
        public int SeriesID { get; set; }
        public decimal CessAmt { get; set; }
        public decimal CessPer { get; set; }
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

    public class BranchStatusResponse
    {
        public int BranchId { get; set; }
        public bool IsActive { get; set; }
        public bool InitialSyncRequired { get; set; }
        public int ExistingItemCount { get; set; }
        public DateTime ServerUtc { get; set; } = DateTime.UtcNow;
    }

    public class ItemMasterSyncDto
    {
        public int? CompanyId { get; set; }
        public int? BranchId { get; set; }
        public int? FinYearId { get; set; }
        public int ItemId { get; set; }
        public string ItemNo { get; set; }
        public string Description { get; set; }
        public string BarCode { get; set; }
        public int? ItemTypeId { get; set; }
        public int? VendorId { get; set; }
        public int? BrandId { get; set; }
        public int? GroupId { get; set; }
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public bool Active { get; set; } = true;
        public bool Hide { get; set; }
        public int? BaseUnitId { get; set; }
        public string HSNCode { get; set; }
    }

    public class MasterDataSyncRequest
    {
        public int BranchId { get; set; }
        public ItemMasterSyncDto Item { get; set; }
        public List<PriceSettingsSyncDto> PriceSettings { get; set; } = new List<PriceSettingsSyncDto>();
    }

    public class PriceSettingsSyncDto
    {
        public int? CompanyId { get; set; }
        public int? FinYearId { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public int ItemId { get; set; }
        public int UnitId { get; set; }
        public string Unit { get; set; }
        public decimal Packing { get; set; } = 1.0m;
        public decimal Cost { get; set; }
        public decimal MarginPer { get; set; }
        public decimal MarginAmt { get; set; }
        public decimal TaxPer { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal WholeSalePrice { get; set; }
        public decimal CreditPrice { get; set; }
        public decimal CardPrice { get; set; }
        public decimal Stock { get; set; }
        public decimal StockValue { get; set; }
        public decimal ReOrder { get; set; }
        public string BarCode { get; set; }
        public string TaxType { get; set; }
        public decimal OpnStk { get; set; }
        public decimal OpnValue { get; set; }
        public string IsBaseUnit { get; set; } = "Y";
        public decimal MRP { get; set; }
    }

    public class MasterDataSyncResponse
    {
        public int BranchId { get; set; }
        public bool Success { get; set; }
        public int SyncedItemCount { get; set; }
        public string Message { get; set; }
        public DateTime SyncedUtc { get; set; } = DateTime.UtcNow;
    }
}
