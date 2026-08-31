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
        public PMasterSyncDto PMaster { get; set; }
        public List<PDetailsSyncDto> PDetails { get; set; }
        public CustomerReceiptSyncDto Receipt { get; set; }
        public List<CustomerReceiptDetailsSyncDto> ReceiptDetails { get; set; }
        public VendorPaymentSyncDto Payment { get; set; }
        public List<VendorPaymentDetailsSyncDto> PaymentDetails { get; set; }
        public SalesReturnSyncDto SalesReturn { get; set; }
        public List<SalesReturnDetailsSyncDto> SalesReturnDetails { get; set; }
        public CreditNoteSyncDto CreditNote { get; set; }
        public List<CreditNoteDetailsSyncDto> CreditNoteDetails { get; set; }
        public PurchaseReturnSyncDto PurchaseReturn { get; set; }
        public List<PurchaseReturnDetailsSyncDto> PurchaseReturnDetails { get; set; }
        public DebitNoteSyncDto DebitNote { get; set; }
        public List<DebitNoteDetailsSyncDto> DebitNoteDetails { get; set; }
        public StockAdjustmentSyncDto StockAdjustment { get; set; }
        public List<StockAdjustmentDetailsSyncDto> StockAdjustmentDetails { get; set; }
        public ShiftClosingSyncDto ShiftClosing { get; set; }
        public List<ShiftClosingDenominationSyncDto> ShiftClosingDenominations { get; set; }
        public CounterSessionSyncDto CounterSession { get; set; }
        public List<VoucherSyncDto> Vouchers { get; set; }

        public TransactionSyncDto()
        {
            EntityType = "SALES";
            Operation = "CREATE";
            OccurredUtc = DateTime.UtcNow;
            SDetails = new List<SDetailsSyncDto>();
            PDetails = new List<PDetailsSyncDto>();
            ReceiptDetails = new List<CustomerReceiptDetailsSyncDto>();
            PaymentDetails = new List<VendorPaymentDetailsSyncDto>();
            SalesReturnDetails = new List<SalesReturnDetailsSyncDto>();
            CreditNoteDetails = new List<CreditNoteDetailsSyncDto>();
            PurchaseReturnDetails = new List<PurchaseReturnDetailsSyncDto>();
            DebitNoteDetails = new List<DebitNoteDetailsSyncDto>();
            StockAdjustmentDetails = new List<StockAdjustmentDetailsSyncDto>();
            ShiftClosingDenominations = new List<ShiftClosingDenominationSyncDto>();
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

        public PMasterSyncDto()
        {
            VendorName = string.Empty;
            Paymode = "Cash";
            TaxType = "I";
            Remarks = string.Empty;
            CurSymbol = "RM";
        }
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
        public decimal Packing { get; set; }
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

        public PDetailsSyncDto()
        {
            Barcode = string.Empty;
            ItemName = string.Empty;
            Unit = string.Empty;
            Packing = 1.0m;
            TaxType = "I";
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

    public class BranchStatusResponse
    {
        public int BranchId { get; set; }
        public bool IsActive { get; set; }
        public bool InitialSyncRequired { get; set; }
        public int ExistingItemCount { get; set; }
        public DateTime ServerUtc { get; set; }
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
        public bool Active { get; set; }
        public bool Hide { get; set; }
        public int? BaseUnitId { get; set; }
        public string HSNCode { get; set; }

        public ItemMasterSyncDto()
        {
            Active = true;
            Hide = false;
        }
    }

    public class MasterDataSyncRequest
    {
        public int BranchId { get; set; }
        public ItemMasterSyncDto Item { get; set; }
        public List<PriceSettingsSyncDto> PriceSettings { get; set; }

        public MasterDataSyncRequest()
        {
            PriceSettings = new List<PriceSettingsSyncDto>();
        }
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
        public decimal Packing { get; set; }
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
        public string IsBaseUnit { get; set; }
        public decimal MRP { get; set; }

        public PriceSettingsSyncDto()
        {
            Packing = 1.0m;
            IsBaseUnit = "Y";
        }
    }

    public class CustomerReceiptSyncDto
    {
        public int BranchReceiptId { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public long VoucherId { get; set; }
        public DateTime VoucherDate { get; set; }
        public int PaymentMethodLedgerId { get; set; }
        public string PaymentMethodName { get; set; }
        public int CustomerLedgerId { get; set; }
        public string CustomerName { get; set; }
        public decimal ReceivableAmount { get; set; }
        public decimal ReceiptAmount { get; set; }
        public decimal OldReceiptAmount { get; set; }
        public string Narration { get; set; }
        public long BillNoUntil { get; set; }
        public bool CancelFlag { get; set; }
        public int UserId { get; set; }
        public int? TransporterLedgerId { get; set; }

        public CustomerReceiptSyncDto()
        {
            VoucherDate = DateTime.Now;
            PaymentMethodName = string.Empty;
            CustomerName = string.Empty;
            Narration = string.Empty;
        }
    }

    public class CustomerReceiptDetailsSyncDto
    {
        public int BranchId { get; set; }
        public int BranchReceiptId { get; set; }
        public int BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public decimal BillAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public decimal ReceiptAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public bool CancelFlag { get; set; }

        public CustomerReceiptDetailsSyncDto()
        {
            BillDate = DateTime.Now;
        }
    }

    public class VendorPaymentSyncDto
    {
        public int BranchPaymentId { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public long VoucherId { get; set; }
        public DateTime VoucherDate { get; set; }
        public int PaymentMethodLedgerId { get; set; }
        public string PaymentMethodName { get; set; }
        public int VendorLedgerId { get; set; }
        public string VendorName { get; set; }
        public decimal PayableAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal OldPaymentAmount { get; set; }
        public string Narration { get; set; }
        public long BillNoUntil { get; set; }
        public bool CancelFlag { get; set; }
        public int UserId { get; set; }

        public VendorPaymentSyncDto()
        {
            VoucherDate = DateTime.Now;
            PaymentMethodName = string.Empty;
            VendorName = string.Empty;
            Narration = string.Empty;
        }
    }

    public class VendorPaymentDetailsSyncDto
    {
        public int BranchId { get; set; }
        public int BranchPaymentId { get; set; }
        public int BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public decimal BillAmount { get; set; }
        public decimal PayedAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public bool CancelFlag { get; set; }

        public VendorPaymentDetailsSyncDto()
        {
            BillDate = DateTime.Now;
        }
    }

    public class SalesReturnSyncDto
    {
        public int BranchSReturnNo { get; set; }
        public DateTime SReturnDate { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }
        public int BranchId { get; set; }
        public int LedgerID { get; set; }
        public string CustomerName { get; set; }
        public string Paymode { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal GrandTotal { get; set; }
        public long? VoucherID { get; set; }
        public string Remarks { get; set; }
        public bool CancelFlag { get; set; }
        public int UserId { get; set; }

        public SalesReturnSyncDto()
        {
            SReturnDate = DateTime.Now;
            CustomerName = string.Empty;
            Paymode = string.Empty;
            Remarks = string.Empty;
        }
    }

    public class SalesReturnDetailsSyncDto
    {
        public int BranchId { get; set; }
        public int BranchSReturnNo { get; set; }
        public int SlNo { get; set; }
        public long ItemID { get; set; }
        public string ItemName { get; set; }
        public decimal Qty { get; set; }
        public decimal Packing { get; set; }
        public decimal SalesPrice { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal TotalSP { get; set; }
        public int? UnitId { get; set; }
        public string Unit { get; set; }
        public bool CancelFlag { get; set; }

        public SalesReturnDetailsSyncDto()
        {
            Packing = 1.0m;
            ItemName = string.Empty;
            Unit = string.Empty;
        }
    }

    public class CreditNoteSyncDto
    {
        public int BranchCreditNoteId { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public long? VoucherId { get; set; }
        public DateTime VoucherDate { get; set; }
        public int CustomerLedgerId { get; set; }
        public string CustomerName { get; set; }
        public int? SReturnNo { get; set; }
        public string InvoiceNo { get; set; }
        public decimal CreditAmount { get; set; }
        public string Narration { get; set; }
        public bool CancelFlag { get; set; }
        public int UserId { get; set; }

        public CreditNoteSyncDto()
        {
            VoucherDate = DateTime.Now;
            CustomerName = string.Empty;
            Narration = string.Empty;
        }
    }

    public class CreditNoteDetailsSyncDto
    {
        public int BranchId { get; set; }
        public int BranchCreditNoteId { get; set; }
        public int BillNo { get; set; }
        public DateTime? BillDate { get; set; }
        public decimal BillAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public bool CancelFlag { get; set; }
    }

    public class PurchaseReturnSyncDto
    {
        public int BranchPReturnNo { get; set; }
        public DateTime PReturnDate { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }
        public int BranchId { get; set; }
        public int LedgerID { get; set; }
        public string VendorName { get; set; }
        public string Paymode { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal GrandTotal { get; set; }
        public long? VoucherID { get; set; }
        public string Remarks { get; set; }
        public bool CancelFlag { get; set; }
        public int UserId { get; set; }

        public PurchaseReturnSyncDto()
        {
            PReturnDate = DateTime.Now;
            VendorName = string.Empty;
            Paymode = string.Empty;
            Remarks = string.Empty;
        }
    }

    public class PurchaseReturnDetailsSyncDto
    {
        public int BranchId { get; set; }
        public int BranchPReturnNo { get; set; }
        public int SlNo { get; set; }
        public long ItemID { get; set; }
        public string ItemName { get; set; }
        public decimal Qty { get; set; }
        public decimal Packing { get; set; }
        public decimal Cost { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal TotalSP { get; set; }
        public int? UnitId { get; set; }
        public string Unit { get; set; }
        public bool CancelFlag { get; set; }

        public PurchaseReturnDetailsSyncDto()
        {
            Packing = 1.0m;
            ItemName = string.Empty;
            Unit = string.Empty;
        }
    }

    public class DebitNoteSyncDto
    {
        public int BranchDebitNoteId { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public long? VoucherId { get; set; }
        public DateTime VoucherDate { get; set; }
        public int VendorLedgerId { get; set; }
        public string VendorName { get; set; }
        public int? PReturnNo { get; set; }
        public string InvoiceNo { get; set; }
        public decimal DebitAmount { get; set; }
        public string Narration { get; set; }
        public bool CancelFlag { get; set; }
        public int UserId { get; set; }

        public DebitNoteSyncDto()
        {
            VoucherDate = DateTime.Now;
            VendorName = string.Empty;
            Narration = string.Empty;
        }
    }

    public class DebitNoteDetailsSyncDto
    {
        public int BranchId { get; set; }
        public int BranchDebitNoteId { get; set; }
        public int BillNo { get; set; }
        public DateTime? BillDate { get; set; }
        public decimal BillAmount { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public bool CancelFlag { get; set; }
    }

    public class StockAdjustmentSyncDto
    {
        public int BranchStockAdjustmentId { get; set; }
        public int FinYearId { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int StockAdjustmentNo { get; set; }
        public DateTime StockAdjustmentDate { get; set; } = DateTime.Now;
        public string Comments { get; set; }
        public int? LedgerId { get; set; }
        public int? VoucherId { get; set; }
        public int UserId { get; set; }
        public bool CancelFlag { get; set; }
        public int? CategoryId { get; set; }
    }

    public class StockAdjustmentDetailsSyncDto
    {
        public int FinYearId { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int BranchStockAdjustmentNo { get; set; }
        public int SlNo { get; set; }
        public int ItemId { get; set; }
        public int? UnitId { get; set; }
        public decimal Packing { get; set; } = 1.0m;
        public bool IsBaseUnit { get; set; } = true;
        public decimal Cost { get; set; }
        public decimal OriginalCost { get; set; }
        public decimal SystemStock { get; set; }
        public decimal PhysicalStock { get; set; }
        public decimal QtyDifference { get; set; }
        public string Reason { get; set; }
        public bool CancelFlag { get; set; }
    }

    public class ShiftClosingSyncDto
    {
        public int BranchShiftClosingId { get; set; }
        public int CompanyId { get; set; } = 1;
        public int BranchId { get; set; }
        public int FinYearId { get; set; } = 1;
        public string Counter { get; set; }
        public int UserId { get; set; } = 1;
        public DateTime ClosingDate { get; set; } = DateTime.Now;
        public string ReportSelection { get; set; }
        public string DocNo { get; set; }
        public decimal TotalGrossSales { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalReturn { get; set; }
        public decimal NetSales { get; set; }
        public decimal CashSale { get; set; }
        public decimal CardSale { get; set; }
        public decimal UpiSale { get; set; }
        public decimal CreditSale { get; set; }
        public decimal CustomerReceipt { get; set; }
        public decimal TotalCollection { get; set; }
        public decimal CashRefundAdjusted { get; set; }
        public decimal MidDayCashSkim { get; set; }
        public decimal SystemExpectedCash { get; set; }
        public decimal PhysicalCashCounted { get; set; }
        public decimal CashDifference { get; set; }
        public string DifferenceReason { get; set; }
        public string Status { get; set; } = "Closed";
        public long? VoucherId { get; set; }
        public long? CounterSessionId { get; set; }
    }

    public class ShiftClosingDenominationSyncDto
    {
        public int DenominationId { get; set; }
        public int BranchShiftClosingId { get; set; }
        public int No { get; set; }
        public decimal Denomination { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
    }

    public class CounterSessionSyncDto
    {
        public long BranchSessionId { get; set; }
        public int CompanyId { get; set; } = 1;
        public int BranchId { get; set; }
        public int FinYearId { get; set; } = 1;
        public int CounterId { get; set; } = 1;
        public string CounterName { get; set; }
        public int UserId { get; set; } = 1;
        public DateTime LoginTime { get; set; }
        public DateTime? CloseTime { get; set; }
        public int? ShiftClosingId { get; set; }
        public string Status { get; set; } = "Closed";
        public string SystemName { get; set; }
    }

    public class MasterDataSyncResponse
    {
        public int BranchId { get; set; }
        public bool Success { get; set; }
        public int SyncedItemCount { get; set; }
        public string Message { get; set; }
        public DateTime SyncedUtc { get; set; }
    }
}
