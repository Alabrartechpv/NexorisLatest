using Dapper;
using Nexoris.SyncService.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Nexoris.SyncService.Services
{
    public class LocalDataProvider : ILocalDataProvider
    {
        private const string SyncStoredProcedure = "dbo.POS_SyncQueue";
        private readonly string _connectionString;

        public LocalDataProvider()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["LocalDbConnection"]?.ConnectionString
                ?? "Server=192.168.1.232\\SQLEXPRESS;Database=RambaiTest;User Id=sa;Password=Abrar@123;Connect Timeout=30;";
        }

        public async Task<List<SyncQueueItem>> GetPendingQueueItemsAsync(int batchSize)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var items = await conn.QueryAsync<SyncQueueItem>(
                        SyncStoredProcedure,
                        new { _Operation = "GETPENDING", TopN = batchSize },
                        commandType: CommandType.StoredProcedure
                    );

                    return items.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] Error retrieving pending items from SyncQueue: " + ex.Message);
                return new List<SyncQueueItem>();
            }
        }

        public async Task<BatchSyncRequest> AssembleBatchAsync(List<SyncQueueItem> queueItems, int branchId)
        {
            var batch = new BatchSyncRequest
            {
                BatchId = Guid.NewGuid().ToString(),
                BranchId = branchId,
                SentUtc = DateTime.UtcNow
            };

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                foreach (var item in queueItems)
                {
                    try
                    {
                        var tx = new TransactionSyncDto
                        {
                            TransactionGuid = item.TransactionGuid,
                            EntityType = item.EntityType,
                            Operation = item.Operation,
                            OccurredUtc = item.CreatedDate
                        };

                        if (item.EntityType.Equals("PURCHASE", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var multi = await conn.QueryMultipleAsync(
                                SyncStoredProcedure,
                                new { _Operation = "GETPURCHASE", item.TransactionGuid, EntityId = item.EntityID },
                                commandType: CommandType.StoredProcedure))
                            {
                                var pMaster = await multi.ReadFirstOrDefaultAsync<dynamic>();
                                if (pMaster != null)
                                {
                                    tx.PMaster = new PMasterSyncDto
                                    {
                                        PurchaseNo = Convert.ToInt32(pMaster.PurchaseNo),
                                        PurchaseDate = pMaster.PurchaseDate != null ? (DateTime)pMaster.PurchaseDate : DateTime.Now,
                                        InvoiceNo = pMaster.InvoiceNo != null ? (string)pMaster.InvoiceNo : string.Empty,
                                        InvoiceDate = pMaster.InvoiceDate != null ? (DateTime?)pMaster.InvoiceDate : null,
                                        LedgerID = pMaster.LedgerID != null ? (int?)pMaster.LedgerID : null,
                                        VendorName = pMaster.VendorName != null ? (string)pMaster.VendorName : string.Empty,
                                        Paymode = pMaster.Paymode != null ? (string)pMaster.Paymode : "Cash",
                                        SubTotal = pMaster.SubTotal != null ? Convert.ToDecimal(pMaster.SubTotal) : 0m,
                                        BillDiscountAmt = pMaster.BillDiscountAmt != null ? Convert.ToDecimal(pMaster.BillDiscountAmt) : 0m,
                                        TaxAmt = pMaster.TaxAmt != null ? Convert.ToDecimal(pMaster.TaxAmt) : 0m,
                                        GrandTotal = pMaster.GrandTotal != null ? Convert.ToDecimal(pMaster.GrandTotal) : 0m,
                                        UserID = pMaster.UserID != null ? (int?)pMaster.UserID : null,
                                        Remarks = pMaster.Remarks != null ? (string)pMaster.Remarks : string.Empty
                                    };

                                    var pDetails = await multi.ReadAsync<dynamic>();
                                    foreach (var d in pDetails)
                                    {
                                        tx.PDetails.Add(new PDetailsSyncDto
                                        {
                                            SlNo = Convert.ToInt32(d.SlNo),
                                            ItemID = Convert.ToInt32(d.ItemID),
                                            ItemName = d.ItemName != null ? (string)d.ItemName : string.Empty,
                                            UnitId = d.UnitId != null ? (int?)d.UnitId : null,
                                            Unit = d.Unit != null ? (string)d.Unit : string.Empty,
                                            Packing = d.Packing != null ? Convert.ToDecimal(d.Packing) : 1.0m,
                                            Qty = Convert.ToDecimal(d.Qty),
                                            Free = d.Free != null ? Convert.ToDecimal(d.Free) : 0m,
                                            Cost = Convert.ToDecimal(d.Cost),
                                            DisAmt = d.DisAmt != null ? Convert.ToDecimal(d.DisAmt) : 0m,
                                            TaxAmt = d.TaxAmt != null ? Convert.ToDecimal(d.TaxAmt) : 0m,
                                            TotalSP = d.TotalSP != null ? Convert.ToDecimal(d.TotalSP) : 0m,
                                            SalesPrice = d.SalesPrice != null ? Convert.ToDecimal(d.SalesPrice) : 0m
                                        });
                                    }

                                    var vouchers = await multi.ReadAsync<dynamic>();
                                    foreach (var v in vouchers)
                                    {
                                        tx.Vouchers.Add(new VoucherSyncDto
                                        {
                                            BranchVoucherId = Convert.ToInt64(v.BranchVoucherId),
                                            LedgerID = v.LedgerID != null ? (int?)v.LedgerID : null,
                                            LedgerName = v.LedgerName != null ? (string)v.LedgerName : string.Empty,
                                            Debit = v.Debit != null ? Convert.ToDecimal(v.Debit) : 0m,
                                            Credit = v.Credit != null ? Convert.ToDecimal(v.Credit) : 0m,
                                            Narration = v.Narration != null ? (string)v.Narration : string.Empty
                                        });
                                    }
                                }
                            }
                        }
                        else if (item.EntityType.Equals("CUSTOMER_RECEIPT", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var multi = await conn.QueryMultipleAsync(
                                SyncStoredProcedure,
                                new { _Operation = "GETRECEIPT", item.TransactionGuid, EntityId = item.EntityID },
                                commandType: CommandType.StoredProcedure))
                            {
                                var rMaster = await multi.ReadFirstOrDefaultAsync<dynamic>();
                                if (rMaster != null)
                                {
                                    tx.Receipt = new CustomerReceiptSyncDto
                                    {
                                        BranchReceiptId = Convert.ToInt32(rMaster.BranchReceiptId),
                                        CompanyId = rMaster.CompanyId != null ? Convert.ToInt32(rMaster.CompanyId) : 1,
                                        BranchId = rMaster.BranchId != null ? Convert.ToInt32(rMaster.BranchId) : 1,
                                        VoucherId = Convert.ToInt64(rMaster.VoucherId),
                                        VoucherDate = rMaster.VoucherDate != null ? (DateTime)rMaster.VoucherDate : DateTime.Now,
                                        PaymentMethodLedgerId = Convert.ToInt32(rMaster.PaymentMethodLedgerId),
                                        PaymentMethodName = string.Empty,
                                        CustomerLedgerId = Convert.ToInt32(rMaster.CustomerLedgerId),
                                        CustomerName = string.Empty,
                                        ReceivableAmount = rMaster.ReceivableAmount != null ? Convert.ToDecimal(rMaster.ReceivableAmount) : 0m,
                                        ReceiptAmount = rMaster.ReceiptAmount != null ? Convert.ToDecimal(rMaster.ReceiptAmount) : 0m,
                                        OldReceiptAmount = rMaster.OldReceiptAmount != null ? Convert.ToDecimal(rMaster.OldReceiptAmount) : 0m,
                                        Narration = rMaster.Narration != null ? (string)rMaster.Narration : string.Empty,
                                        BillNoUntil = rMaster.BillNoUntil != null ? Convert.ToInt64(rMaster.BillNoUntil) : 0,
                                        CancelFlag = rMaster.CancelFlag != null && Convert.ToBoolean(rMaster.CancelFlag),
                                        UserId = rMaster.UserId != null ? Convert.ToInt32(rMaster.UserId) : 1,
                                        TransporterLedgerId = rMaster.TransporterLedgerId != null ? (int?)rMaster.TransporterLedgerId : null
                                    };

                                    var rDetails = await multi.ReadAsync<dynamic>();
                                    foreach (var d in rDetails)
                                    {
                                        tx.ReceiptDetails.Add(new CustomerReceiptDetailsSyncDto
                                        {
                                            BranchId = Convert.ToInt32(d.BranchId),
                                            BranchReceiptId = Convert.ToInt32(d.BranchReceiptId),
                                            BillNo = Convert.ToInt32(d.BillNo),
                                            BillDate = d.BillDate != null ? (DateTime)d.BillDate : DateTime.Now,
                                            BillAmount = d.BillAmount != null ? Convert.ToDecimal(d.BillAmount) : 0m,
                                            ReceivedAmount = d.ReceivedAmount != null ? Convert.ToDecimal(d.ReceivedAmount) : 0m,
                                            ReceiptAmount = d.ReceiptAmount != null ? Convert.ToDecimal(d.ReceiptAmount) : 0m,
                                            BalanceAmount = d.BalanceAmount != null ? Convert.ToDecimal(d.BalanceAmount) : 0m,
                                            CancelFlag = d.CancelFlag != null && Convert.ToBoolean(d.CancelFlag)
                                        });
                                    }

                                    var vouchers = await multi.ReadAsync<dynamic>();
                                    foreach (var v in vouchers)
                                    {
                                        tx.Vouchers.Add(new VoucherSyncDto
                                        {
                                            BranchVoucherId = Convert.ToInt64(v.BranchVoucherId),
                                            LedgerID = v.LedgerID != null ? (int?)v.LedgerID : null,
                                            LedgerName = v.LedgerName != null ? (string)v.LedgerName : string.Empty,
                                            Debit = v.Debit != null ? Convert.ToDecimal(v.Debit) : 0m,
                                            Credit = v.Credit != null ? Convert.ToDecimal(v.Credit) : 0m,
                                            Narration = v.Narration != null ? (string)v.Narration : string.Empty
                                        });
                                    }
                                }
                            }
                        }
                        else if (item.EntityType.Equals("VENDOR_PAYMENT", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var multi = await conn.QueryMultipleAsync(
                                SyncStoredProcedure,
                                new { _Operation = "GETPAYMENT", item.TransactionGuid, EntityId = item.EntityID },
                                commandType: CommandType.StoredProcedure))
                            {
                                var pMaster = await multi.ReadFirstOrDefaultAsync<dynamic>();
                                if (pMaster != null)
                                {
                                    tx.Payment = new VendorPaymentSyncDto
                                    {
                                        BranchPaymentId = Convert.ToInt32(pMaster.BranchPaymentId),
                                        CompanyId = pMaster.CompanyId != null ? Convert.ToInt32(pMaster.CompanyId) : 1,
                                        BranchId = pMaster.BranchId != null ? Convert.ToInt32(pMaster.BranchId) : 1,
                                        VoucherId = Convert.ToInt64(pMaster.VoucherId),
                                        VoucherDate = pMaster.VoucherDate != null ? (DateTime)pMaster.VoucherDate : DateTime.Now,
                                        PaymentMethodLedgerId = Convert.ToInt32(pMaster.PaymentMethodLedgerId),
                                        PaymentMethodName = string.Empty,
                                        VendorLedgerId = Convert.ToInt32(pMaster.VendorLedgerId),
                                        VendorName = string.Empty,
                                        PayableAmount = pMaster.PayableAmount != null ? Convert.ToDecimal(pMaster.PayableAmount) : 0m,
                                        PaymentAmount = pMaster.PaymentAmount != null ? Convert.ToDecimal(pMaster.PaymentAmount) : 0m,
                                        OldPaymentAmount = pMaster.OldPaymentAmount != null ? Convert.ToDecimal(pMaster.OldPaymentAmount) : 0m,
                                        Narration = pMaster.Narration != null ? (string)pMaster.Narration : string.Empty,
                                        BillNoUntil = pMaster.BillNoUntil != null ? Convert.ToInt64(pMaster.BillNoUntil) : 0,
                                        CancelFlag = pMaster.CancelFlag != null && Convert.ToBoolean(pMaster.CancelFlag),
                                        UserId = pMaster.UserId != null ? Convert.ToInt32(pMaster.UserId) : 1
                                    };

                                    var pDetails = await multi.ReadAsync<dynamic>();
                                    foreach (var d in pDetails)
                                    {
                                        tx.PaymentDetails.Add(new VendorPaymentDetailsSyncDto
                                        {
                                            BranchId = Convert.ToInt32(d.BranchId),
                                            BranchPaymentId = Convert.ToInt32(d.BranchPaymentId),
                                            BillNo = Convert.ToInt32(d.BillNo),
                                            BillDate = d.BillDate != null ? (DateTime)d.BillDate : DateTime.Now,
                                            BillAmount = d.BillAmount != null ? Convert.ToDecimal(d.BillAmount) : 0m,
                                            PayedAmount = d.PayedAmount != null ? Convert.ToDecimal(d.PayedAmount) : 0m,
                                            PaymentAmount = d.PaymentAmount != null ? Convert.ToDecimal(d.PaymentAmount) : 0m,
                                            BalanceAmount = d.BalanceAmount != null ? Convert.ToDecimal(d.BalanceAmount) : 0m,
                                            CancelFlag = d.CancelFlag != null && Convert.ToBoolean(d.CancelFlag)
                                        });
                                    }

                                    var vouchers = await multi.ReadAsync<dynamic>();
                                    foreach (var v in vouchers)
                                    {
                                        tx.Vouchers.Add(new VoucherSyncDto
                                        {
                                            BranchVoucherId = Convert.ToInt64(v.BranchVoucherId),
                                            LedgerID = v.LedgerID != null ? (int?)v.LedgerID : null,
                                            LedgerName = v.LedgerName != null ? (string)v.LedgerName : string.Empty,
                                            Debit = v.Debit != null ? Convert.ToDecimal(v.Debit) : 0m,
                                            Credit = v.Credit != null ? Convert.ToDecimal(v.Credit) : 0m,
                                            Narration = v.Narration != null ? (string)v.Narration : string.Empty
                                        });
                                    }
                                }
                            }
                        }
                        else if (item.EntityType.Equals("SALES_RETURN", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var multi = await conn.QueryMultipleAsync(
                                SyncStoredProcedure,
                                new { _Operation = "GETSALESRETURN", item.TransactionGuid, EntityId = item.EntityID },
                                commandType: CommandType.StoredProcedure))
                            {
                                var srMaster = await multi.ReadFirstOrDefaultAsync<dynamic>();
                                if (srMaster != null)
                                {
                                    tx.SalesReturn = new SalesReturnSyncDto
                                    {
                                        BranchSReturnNo = Convert.ToInt32(srMaster.BranchSReturnNo),
                                        SReturnDate = srMaster.SReturnDate != null ? (DateTime)srMaster.SReturnDate : DateTime.Now,
                                        InvoiceNo = srMaster.InvoiceNo != null ? (string)srMaster.InvoiceNo : string.Empty,
                                        InvoiceDate = srMaster.InvoiceDate != null ? (DateTime?)srMaster.InvoiceDate : null,
                                        CompanyId = srMaster.CompanyId != null ? Convert.ToInt32(srMaster.CompanyId) : 1,
                                        FinYearId = srMaster.FinYearId != null ? Convert.ToInt32(srMaster.FinYearId) : 1,
                                        BranchId = srMaster.BranchId != null ? Convert.ToInt32(srMaster.BranchId) : branchId,
                                        LedgerID = srMaster.LedgerID != null ? Convert.ToInt32(srMaster.LedgerID) : 0,
                                        CustomerName = srMaster.CustomerName != null ? (string)srMaster.CustomerName : string.Empty,
                                        Paymode = srMaster.Paymode != null ? (string)srMaster.Paymode : string.Empty,
                                        SubTotal = srMaster.SubTotal != null ? Convert.ToDecimal(srMaster.SubTotal) : 0m,
                                        TaxAmt = srMaster.TaxAmt != null ? Convert.ToDecimal(srMaster.TaxAmt) : 0m,
                                        GrandTotal = srMaster.GrandTotal != null ? Convert.ToDecimal(srMaster.GrandTotal) : 0m,
                                        VoucherID = srMaster.VoucherID != null ? (long?)Convert.ToInt64(srMaster.VoucherID) : null,
                                        Remarks = srMaster.Remarks != null ? (string)srMaster.Remarks : string.Empty,
                                        CancelFlag = srMaster.CancelFlag != null && Convert.ToBoolean(srMaster.CancelFlag),
                                        UserId = srMaster.UserID != null ? Convert.ToInt32(srMaster.UserID) : 1
                                    };

                                    var srDetails = await multi.ReadAsync<dynamic>();
                                    foreach (var d in srDetails)
                                    {
                                        tx.SalesReturnDetails.Add(new SalesReturnDetailsSyncDto
                                        {
                                            BranchId = d.BranchID != null ? Convert.ToInt32(d.BranchID) : branchId,
                                            BranchSReturnNo = Convert.ToInt32(d.BranchSReturnNo),
                                            SlNo = Convert.ToInt32(d.SlNo),
                                            ItemID = Convert.ToInt64(d.ItemID),
                                            ItemName = d.ItemName != null ? (string)d.ItemName : string.Empty,
                                            Qty = Convert.ToDecimal(d.Qty),
                                            Packing = d.Packing != null ? Convert.ToDecimal(d.Packing) : 1.0m,
                                            SalesPrice = d.SalesPrice != null ? Convert.ToDecimal(d.SalesPrice) : 0m,
                                            TaxAmt = d.TaxAmt != null ? Convert.ToDecimal(d.TaxAmt) : 0m,
                                            TotalSP = d.TotalSP != null ? Convert.ToDecimal(d.TotalSP) : 0m,
                                            UnitId = d.UnitId != null ? (int?)d.UnitId : null,
                                            Unit = d.Unit != null ? (string)d.Unit : string.Empty,
                                            CancelFlag = d.CancelFlag != null && Convert.ToBoolean(d.CancelFlag)
                                        });
                                    }

                                    var vouchers = await multi.ReadAsync<dynamic>();
                                    foreach (var v in vouchers)
                                    {
                                        tx.Vouchers.Add(new VoucherSyncDto
                                        {
                                            BranchVoucherId = Convert.ToInt64(v.BranchVoucherId),
                                            LedgerID = v.LedgerID != null ? (int?)v.LedgerID : null,
                                            LedgerName = v.LedgerName != null ? (string)v.LedgerName : string.Empty,
                                            Debit = v.Debit != null ? Convert.ToDecimal(v.Debit) : 0m,
                                            Credit = v.Credit != null ? Convert.ToDecimal(v.Credit) : 0m,
                                            Narration = v.Narration != null ? (string)v.Narration : string.Empty
                                        });
                                    }
                                }
                            }
                        }
                        else if (item.EntityType.Equals("CREDIT_NOTE", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var multi = await conn.QueryMultipleAsync(
                                SyncStoredProcedure,
                                new { _Operation = "GETCREDITNOTE", item.TransactionGuid, EntityId = item.EntityID },
                                commandType: CommandType.StoredProcedure))
                            {
                                var cnMaster = await multi.ReadFirstOrDefaultAsync<dynamic>();
                                if (cnMaster != null)
                                {
                                    tx.CreditNote = new CreditNoteSyncDto
                                    {
                                        BranchCreditNoteId = Convert.ToInt32(cnMaster.BranchCreditNoteId),
                                        CompanyId = cnMaster.CompanyId != null ? Convert.ToInt32(cnMaster.CompanyId) : 1,
                                        BranchId = cnMaster.BranchId != null ? Convert.ToInt32(cnMaster.BranchId) : branchId,
                                        FinYearId = cnMaster.FinYearId != null ? Convert.ToInt32(cnMaster.FinYearId) : 1,
                                        VoucherId = cnMaster.VoucherId != null ? (long?)Convert.ToInt64(cnMaster.VoucherId) : null,
                                        VoucherDate = cnMaster.VoucherDate != null ? (DateTime)cnMaster.VoucherDate : DateTime.Now,
                                        CustomerLedgerId = cnMaster.CustomerLedgerId != null ? Convert.ToInt32(cnMaster.CustomerLedgerId) : 0,
                                        CustomerName = cnMaster.CustomerName != null ? (string)cnMaster.CustomerName : string.Empty,
                                        SReturnNo = cnMaster.SReturnNo != null ? (int?)Convert.ToInt32(cnMaster.SReturnNo) : null,
                                        InvoiceNo = cnMaster.InvoiceNo != null ? (string)cnMaster.InvoiceNo : string.Empty,
                                        CreditAmount = cnMaster.CreditAmount != null ? Convert.ToDecimal(cnMaster.CreditAmount) : 0m,
                                        Narration = cnMaster.Narration != null ? (string)cnMaster.Narration : string.Empty,
                                        CancelFlag = cnMaster.CancelFlag != null && Convert.ToBoolean(cnMaster.CancelFlag),
                                        UserId = cnMaster.UserId != null ? Convert.ToInt32(cnMaster.UserId) : 1
                                    };

                                    var cnDetails = await multi.ReadAsync<dynamic>();
                                    foreach (var d in cnDetails)
                                    {
                                        tx.CreditNoteDetails.Add(new CreditNoteDetailsSyncDto
                                        {
                                            BranchId = d.BranchId != null ? Convert.ToInt32(d.BranchId) : branchId,
                                            BranchCreditNoteId = Convert.ToInt32(d.BranchCreditNoteId),
                                            BillNo = Convert.ToInt32(d.BillNo),
                                            BillDate = d.BillDate != null ? (DateTime?)d.BillDate : null,
                                            BillAmount = d.BillAmount != null ? Convert.ToDecimal(d.BillAmount) : 0m,
                                            CreditAmount = d.CreditAmount != null ? Convert.ToDecimal(d.CreditAmount) : 0m,
                                            BalanceAmount = d.BalanceAmount != null ? Convert.ToDecimal(d.BalanceAmount) : 0m,
                                            CancelFlag = d.CancelFlag != null && Convert.ToBoolean(d.CancelFlag)
                                        });
                                    }

                                    var vouchers = await multi.ReadAsync<dynamic>();
                                    foreach (var v in vouchers)
                                    {
                                        tx.Vouchers.Add(new VoucherSyncDto
                                        {
                                            BranchVoucherId = Convert.ToInt64(v.BranchVoucherId),
                                            LedgerID = v.LedgerID != null ? (int?)v.LedgerID : null,
                                            LedgerName = v.LedgerName != null ? (string)v.LedgerName : string.Empty,
                                            Debit = v.Debit != null ? Convert.ToDecimal(v.Debit) : 0m,
                                            Credit = v.Credit != null ? Convert.ToDecimal(v.Credit) : 0m,
                                            Narration = v.Narration != null ? (string)v.Narration : string.Empty
                                        });
                                    }
                                }
                            }
                        }
                        else if (item.EntityType.Equals("PURCHASE_RETURN", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var multi = await conn.QueryMultipleAsync(
                                SyncStoredProcedure,
                                new { _Operation = "GETPURCHASERETURN", item.TransactionGuid, EntityId = item.EntityID },
                                commandType: CommandType.StoredProcedure))
                            {
                                var prMaster = await multi.ReadFirstOrDefaultAsync<dynamic>();
                                if (prMaster != null)
                                {
                                    tx.PurchaseReturn = new PurchaseReturnSyncDto
                                    {
                                        BranchPReturnNo = Convert.ToInt32(prMaster.BranchPReturnNo),
                                        PReturnDate = prMaster.PReturnDate != null ? (DateTime)prMaster.PReturnDate : DateTime.Now,
                                        InvoiceNo = prMaster.InvoiceNo != null ? (string)prMaster.InvoiceNo : string.Empty,
                                        InvoiceDate = prMaster.InvoiceDate != null ? (DateTime?)prMaster.InvoiceDate : null,
                                        CompanyId = prMaster.CompanyId != null ? Convert.ToInt32(prMaster.CompanyId) : 1,
                                        FinYearId = prMaster.FinYearId != null ? Convert.ToInt32(prMaster.FinYearId) : 1,
                                        BranchId = prMaster.BranchId != null ? Convert.ToInt32(prMaster.BranchId) : branchId,
                                        LedgerID = prMaster.LedgerID != null ? Convert.ToInt32(prMaster.LedgerID) : 0,
                                        VendorName = prMaster.VendorName != null ? (string)prMaster.VendorName : string.Empty,
                                        Paymode = prMaster.Paymode != null ? (string)prMaster.Paymode : string.Empty,
                                        SubTotal = prMaster.SubTotal != null ? Convert.ToDecimal(prMaster.SubTotal) : 0m,
                                        TaxAmt = prMaster.TaxAmt != null ? Convert.ToDecimal(prMaster.TaxAmt) : 0m,
                                        GrandTotal = prMaster.GrandTotal != null ? Convert.ToDecimal(prMaster.GrandTotal) : 0m,
                                        VoucherID = prMaster.VoucherID != null ? (long?)Convert.ToInt64(prMaster.VoucherID) : null,
                                        Remarks = prMaster.Remarks != null ? (string)prMaster.Remarks : string.Empty,
                                        CancelFlag = prMaster.CancelFlag != null && Convert.ToBoolean(prMaster.CancelFlag),
                                        UserId = prMaster.UserID != null ? Convert.ToInt32(prMaster.UserID) : 1
                                    };

                                    var prDetails = await multi.ReadAsync<dynamic>();
                                    foreach (var d in prDetails)
                                    {
                                        tx.PurchaseReturnDetails.Add(new PurchaseReturnDetailsSyncDto
                                        {
                                            BranchId = d.BranchID != null ? Convert.ToInt32(d.BranchID) : branchId,
                                            BranchPReturnNo = Convert.ToInt32(d.BranchPReturnNo),
                                            SlNo = Convert.ToInt32(d.SlNo),
                                            ItemID = Convert.ToInt64(d.ItemID),
                                            ItemName = d.ItemName != null ? (string)d.ItemName : string.Empty,
                                            Qty = Convert.ToDecimal(d.Qty),
                                            Packing = d.Packing != null ? Convert.ToDecimal(d.Packing) : 1.0m,
                                            Cost = d.Cost != null ? Convert.ToDecimal(d.Cost) : 0m,
                                            TaxAmt = d.TaxAmt != null ? Convert.ToDecimal(d.TaxAmt) : 0m,
                                            TotalSP = d.TotalSP != null ? Convert.ToDecimal(d.TotalSP) : 0m,
                                            UnitId = d.UnitId != null ? (int?)d.UnitId : null,
                                            Unit = d.Unit != null ? (string)d.Unit : string.Empty,
                                            CancelFlag = d.CancelFlag != null && Convert.ToBoolean(d.CancelFlag)
                                        });
                                    }

                                    var vouchers = await multi.ReadAsync<dynamic>();
                                    foreach (var v in vouchers)
                                    {
                                        tx.Vouchers.Add(new VoucherSyncDto
                                        {
                                            BranchVoucherId = Convert.ToInt64(v.BranchVoucherId),
                                            LedgerID = v.LedgerID != null ? (int?)v.LedgerID : null,
                                            LedgerName = v.LedgerName != null ? (string)v.LedgerName : string.Empty,
                                            Debit = v.Debit != null ? Convert.ToDecimal(v.Debit) : 0m,
                                            Credit = v.Credit != null ? Convert.ToDecimal(v.Credit) : 0m,
                                            Narration = v.Narration != null ? (string)v.Narration : string.Empty
                                        });
                                    }
                                }
                            }
                        }
                        else if (item.EntityType.Equals("DEBIT_NOTE", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var multi = await conn.QueryMultipleAsync(
                                SyncStoredProcedure,
                                new { _Operation = "GETDEBITNOTE", item.TransactionGuid, EntityId = item.EntityID },
                                commandType: CommandType.StoredProcedure))
                            {
                                var dnMaster = await multi.ReadFirstOrDefaultAsync<dynamic>();
                                if (dnMaster != null)
                                {
                                    tx.DebitNote = new DebitNoteSyncDto
                                    {
                                        BranchDebitNoteId = Convert.ToInt32(dnMaster.BranchDebitNoteId),
                                        CompanyId = dnMaster.CompanyId != null ? Convert.ToInt32(dnMaster.CompanyId) : 1,
                                        BranchId = dnMaster.BranchId != null ? Convert.ToInt32(dnMaster.BranchId) : branchId,
                                        FinYearId = dnMaster.FinYearId != null ? Convert.ToInt32(dnMaster.FinYearId) : 1,
                                        VoucherId = dnMaster.VoucherId != null ? (long?)Convert.ToInt64(dnMaster.VoucherId) : null,
                                        VoucherDate = dnMaster.VoucherDate != null ? (DateTime)dnMaster.VoucherDate : DateTime.Now,
                                        VendorLedgerId = dnMaster.VendorLedgerId != null ? Convert.ToInt32(dnMaster.VendorLedgerId) : 0,
                                        VendorName = dnMaster.VendorName != null ? (string)dnMaster.VendorName : string.Empty,
                                        PReturnNo = dnMaster.PReturnNo != null ? (int?)Convert.ToInt32(dnMaster.PReturnNo) : null,
                                        InvoiceNo = dnMaster.InvoiceNo != null ? (string)dnMaster.InvoiceNo : string.Empty,
                                        DebitAmount = dnMaster.DebitAmount != null ? Convert.ToDecimal(dnMaster.DebitAmount) : 0m,
                                        Narration = dnMaster.Narration != null ? (string)dnMaster.Narration : string.Empty,
                                        CancelFlag = dnMaster.CancelFlag != null && Convert.ToBoolean(dnMaster.CancelFlag),
                                        UserId = dnMaster.UserId != null ? Convert.ToInt32(dnMaster.UserId) : 1
                                    };

                                    var dnDetails = await multi.ReadAsync<dynamic>();
                                    foreach (var d in dnDetails)
                                    {
                                        tx.DebitNoteDetails.Add(new DebitNoteDetailsSyncDto
                                        {
                                            BranchId = d.BranchId != null ? Convert.ToInt32(d.BranchId) : branchId,
                                            BranchDebitNoteId = Convert.ToInt32(d.BranchDebitNoteId),
                                            BillNo = Convert.ToInt32(d.BillNo),
                                            BillDate = d.BillDate != null ? (DateTime?)d.BillDate : null,
                                            BillAmount = d.BillAmount != null ? Convert.ToDecimal(d.BillAmount) : 0m,
                                            DebitAmount = d.DebitAmount != null ? Convert.ToDecimal(d.DebitAmount) : 0m,
                                            BalanceAmount = d.BalanceAmount != null ? Convert.ToDecimal(d.BalanceAmount) : 0m,
                                            CancelFlag = d.CancelFlag != null && Convert.ToBoolean(d.CancelFlag)
                                        });
                                    }

                                    var vouchers = await multi.ReadAsync<dynamic>();
                                    foreach (var v in vouchers)
                                    {
                                        tx.Vouchers.Add(new VoucherSyncDto
                                        {
                                            BranchVoucherId = Convert.ToInt64(v.BranchVoucherId),
                                            LedgerID = v.LedgerID != null ? (int?)v.LedgerID : null,
                                            LedgerName = v.LedgerName != null ? (string)v.LedgerName : string.Empty,
                                            Debit = v.Debit != null ? Convert.ToDecimal(v.Debit) : 0m,
                                            Credit = v.Credit != null ? Convert.ToDecimal(v.Credit) : 0m,
                                            Narration = v.Narration != null ? (string)v.Narration : string.Empty
                                        });
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Default: SALES
                            using (var multi = await conn.QueryMultipleAsync(
                                SyncStoredProcedure,
                                new { _Operation = "GETSALES", item.TransactionGuid },
                                commandType: CommandType.StoredProcedure))
                            {
                                var master = await multi.ReadFirstOrDefaultAsync<dynamic>();
                                if (master != null)
                                {
                                    tx.SMaster = new SMasterSyncDto
                                    {
                                        BillNo = Convert.ToInt64(master.BillNo),
                                        BillDate = (DateTime)master.BillDate,
                                        CompanyId = master.CompanyId != null ? Convert.ToInt32(master.CompanyId) : 1,
                                        FinYearId = master.FinYearId != null ? Convert.ToInt32(master.FinYearId) : 1,
                                        CounterId = master.CounterId != null ? Convert.ToInt32(master.CounterId) : 1,
                                        CustomerName = master.CustomerName != null ? (string)master.CustomerName : string.Empty,
                                        LedgerID = master.LedgerID != null ? (int?)master.LedgerID : null,
                                        PaymodeId = master.PaymodeId != null ? (int?)master.PaymodeId : null,
                                        PaymodeName = master.PaymodeName != null ? (string)master.PaymodeName : string.Empty,
                                        SubTotal = master.SubTotal != null ? Convert.ToDecimal(master.SubTotal) : 0m,
                                        DiscountAmt = master.DiscountAmt != null ? Convert.ToDecimal(master.DiscountAmt) : 0m,
                                        TaxAmt = master.TaxAmt != null ? Convert.ToDecimal(master.TaxAmt) : 0m,
                                        NetAmount = master.NetAmount != null ? Convert.ToDecimal(master.NetAmount) : 0m,
                                        UserId = master.UserId != null ? (int?)master.UserId : null,
                                        Status = master.Status != null ? (string)master.Status : "PAID"
                                    };

                                    var details = await multi.ReadAsync<dynamic>();
                                    foreach (var d in details)
                                    {
                                        tx.SDetails.Add(new SDetailsSyncDto
                                        {
                                            SlNO = Convert.ToInt32(d.SlNO),
                                            ItemId = Convert.ToInt64(d.ItemId),
                                            Barcode = d.Barcode != null ? (string)d.Barcode : string.Empty,
                                            ItemName = d.ItemName != null ? (string)d.ItemName : string.Empty,
                                            Qty = Convert.ToDecimal(d.Qty),
                                            Packing = d.Packing != null ? Convert.ToDecimal(d.Packing) : 1.0m,
                                            UnitPrice = Convert.ToDecimal(d.UnitPrice),
                                            Amount = Convert.ToDecimal(d.Amount),
                                            DiscountAmount = d.DiscountAmount != null ? Convert.ToDecimal(d.DiscountAmount) : (decimal?)null,
                                            TaxAmt = d.TaxAmt != null ? Convert.ToDecimal(d.TaxAmt) : (decimal?)null,
                                            TotalAmount = Convert.ToDecimal(d.TotalAmount),
                                            UnitId = d.UnitId != null ? (int?)d.UnitId : null
                                        });
                                    }

                                    var vouchers = await multi.ReadAsync<dynamic>();
                                    foreach (var v in vouchers)
                                    {
                                        tx.Vouchers.Add(new VoucherSyncDto
                                        {
                                            BranchVoucherId = Convert.ToInt64(v.BranchVoucherId),
                                            LedgerID = v.LedgerID != null ? (int?)v.LedgerID : null,
                                            LedgerName = v.LedgerName != null ? (string)v.LedgerName : string.Empty,
                                            Debit = Convert.ToDecimal(v.Debit),
                                            Credit = Convert.ToDecimal(v.Credit),
                                            Narration = v.Narration != null ? (string)v.Narration : string.Empty
                                        });
                                    }
                                }
                            }
                        }

                        batch.Transactions.Add(tx);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(string.Format("[ERROR] Failed assembling transaction for Guid {0}: {1}", item.TransactionGuid, ex.Message));
                    }
                }
            }

            return batch;
        }

        public async Task UpdateQueueStatusAsync(Guid transactionGuid, string status, string errorMessage = null)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    await conn.ExecuteAsync(
                        SyncStoredProcedure,
                        new
                        {
                            _Operation = "UPDATESTATUS",
                            TransactionGuid = transactionGuid,
                            Status = status,
                            ErrorMessage = string.IsNullOrEmpty(errorMessage) ? null : errorMessage
                        },
                        commandType: CommandType.StoredProcedure
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("[ERROR] Failed to update queue status for {0}: {1}", transactionGuid, ex.Message));
            }
        }

        public async Task ProcessResultsAsync(List<SyncItemResult> results)
        {
            if (results == null || !results.Any()) return;

            foreach (var r in results)
            {
                await UpdateQueueStatusAsync(r.TransactionGuid, r.Status, r.ErrorMessage);
            }
        }

        public async Task<List<PriceSettingsSyncDto>> GetLocalPriceSettingsAsync(int branchId)
        {
            var list = new List<PriceSettingsSyncDto>();
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var rows = await conn.QueryAsync<PriceSettingsSyncDto>(
                        SyncStoredProcedure,
                        new { _Operation = "GETPRICESETTINGS", BranchId = branchId },
                        commandType: CommandType.StoredProcedure
                    );

                    list = rows.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("[ERROR] Failed to read local PriceSettings for Branch {0}: {1}", branchId, ex.Message));
            }
            return list;
        }

        public async Task<MasterDataSyncRequest> AssembleMasterDataAsync(int itemId, int branchId)
        {
            var req = new MasterDataSyncRequest { BranchId = branchId };
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    using (var multi = await conn.QueryMultipleAsync(
                        SyncStoredProcedure,
                        new { _Operation = "GETITEM", ItemId = itemId, BranchId = branchId },
                        commandType: CommandType.StoredProcedure))
                    {
                        var it = await multi.ReadFirstOrDefaultAsync<dynamic>();
                        if (it != null)
                        {
                            req.Item = new ItemMasterSyncDto
                            {
                                CompanyId = it.CompanyId != null ? (int?)it.CompanyId : null,
                                BranchId = it.BranchId != null ? (int?)it.BranchId : branchId,
                                FinYearId = it.FinYearId != null ? (int?)it.FinYearId : null,
                                ItemId = (int)it.ItemId,
                                ItemNo = it.ItemNo != null ? (string)it.ItemNo : string.Empty,
                                Description = it.Description != null ? (string)it.Description : string.Empty,
                                BarCode = it.BarCode != null ? (string)it.BarCode : string.Empty,
                                ItemTypeId = it.ItemTypeId != null ? (int?)it.ItemTypeId : null,
                                VendorId = it.VendorId != null ? (int?)it.VendorId : null,
                                BrandId = it.BrandId != null ? (int?)it.BrandId : null,
                                GroupId = it.GroupId != null ? (int?)it.GroupId : null,
                                CategoryId = it.CategoryId != null ? (int?)it.CategoryId : null,
                                SubCategoryId = it.SubCategoryId != null ? (int?)it.SubCategoryId : null,
                                Active = it.Active != null ? (bool)it.Active : true,
                                Hide = it.Hide != null ? (bool)it.Hide : false,
                                BaseUnitId = it.BaseUnitId != null ? (int?)it.BaseUnitId : null,
                                HSNCode = it.HSNCode != null ? (string)it.HSNCode : string.Empty
                            };
                        }

                        var psRows = await multi.ReadAsync<PriceSettingsSyncDto>();
                        req.PriceSettings = psRows.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("[ERROR] Failed assembling master data for ItemId {0}: {1}", itemId, ex.Message));
            }
            return req;
        }
    }
}
