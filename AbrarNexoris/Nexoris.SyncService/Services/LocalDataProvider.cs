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

                    var parameters = new DynamicParameters();
                    parameters.Add("@_Operation", "GETPENDING");
                    parameters.Add("@TopN", batchSize);

                    var items = await conn.QueryAsync<SyncQueueItem>(
                        "dbo.POS_SyncQueue",
                        parameters,
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
                            // Fetch PMaster
                            const string pMasterSql = @"
                                SELECT TOP 1 
                                    PurchaseNo, PurchaseDate, InvoiceNo, InvoiceDate, 
                                    LedgerID, VendorName, PaymodeID, Paymode, CreditPeriod, 
                                    SubTotal, SpDisPer, SpDsiAmt, BillDiscountPer, BillDiscountAmt, 
                                    TaxPer, TaxAmt, Frieght, ExpenseAmt, OtherExpAmt, GrandTotal, 
                                    CancelFlag, UserID, UserName, TaxType, Remarks, RoundOff, 
                                    CessPer, CessAmt, CalAfterTax, CurrencyID, CurSymbol, SeriesID, 
                                    NetTotal, VoucherID
                                FROM dbo.PMaster 
                                WHERE TransactionGuid = @TransactionGuid";

                            var pMaster = await conn.QueryFirstOrDefaultAsync<dynamic>(
                                pMasterSql, new { item.TransactionGuid });

                            if (pMaster != null)
                            {
                                tx.PMaster = new PMasterSyncDto
                                {
                                    PurchaseNo = (int)pMaster.PurchaseNo,
                                    PurchaseDate = (DateTime)pMaster.PurchaseDate,
                                    InvoiceNo = pMaster.InvoiceNo != null ? (string)pMaster.InvoiceNo : string.Empty,
                                    InvoiceDate = pMaster.InvoiceDate != null ? (DateTime?)pMaster.InvoiceDate : null,
                                    LedgerID = pMaster.LedgerID != null ? (int?)pMaster.LedgerID : null,
                                    VendorName = pMaster.VendorName != null ? (string)pMaster.VendorName : string.Empty,
                                    PaymodeID = pMaster.PaymodeID != null ? (int?)pMaster.PaymodeID : null,
                                    Paymode = pMaster.Paymode != null ? (string)pMaster.Paymode : "Cash",
                                    CreditPeriod = pMaster.CreditPeriod != null ? (int)pMaster.CreditPeriod : 0,
                                    SubTotal = pMaster.SubTotal != null ? Convert.ToDecimal(pMaster.SubTotal) : 0m,
                                    SpDisPer = pMaster.SpDisPer != null ? Convert.ToDecimal(pMaster.SpDisPer) : 0m,
                                    SpDsiAmt = pMaster.SpDsiAmt != null ? Convert.ToDecimal(pMaster.SpDsiAmt) : 0m,
                                    BillDiscountPer = pMaster.BillDiscountPer != null ? Convert.ToDecimal(pMaster.BillDiscountPer) : 0m,
                                    BillDiscountAmt = pMaster.BillDiscountAmt != null ? Convert.ToDecimal(pMaster.BillDiscountAmt) : 0m,
                                    TaxPer = pMaster.TaxPer != null ? Convert.ToDecimal(pMaster.TaxPer) : 0m,
                                    TaxAmt = pMaster.TaxAmt != null ? Convert.ToDecimal(pMaster.TaxAmt) : 0m,
                                    Frieght = pMaster.Frieght != null ? Convert.ToDecimal(pMaster.Frieght) : 0m,
                                    ExpenseAmt = pMaster.ExpenseAmt != null ? Convert.ToDecimal(pMaster.ExpenseAmt) : 0m,
                                    OtherExpAmt = pMaster.OtherExpAmt != null ? Convert.ToDecimal(pMaster.OtherExpAmt) : 0m,
                                    GrandTotal = pMaster.GrandTotal != null ? Convert.ToDecimal(pMaster.GrandTotal) : 0m,
                                    CancelFlag = pMaster.CancelFlag != null ? (bool)pMaster.CancelFlag : false,
                                    UserID = pMaster.UserID != null ? (int?)pMaster.UserID : null,
                                    UserName = pMaster.UserName != null ? (string)pMaster.UserName : string.Empty,
                                    TaxType = pMaster.TaxType != null ? (string)pMaster.TaxType : "I",
                                    Remarks = pMaster.Remarks != null ? (string)pMaster.Remarks : string.Empty,
                                    RoundOff = pMaster.RoundOff != null ? Convert.ToDecimal(pMaster.RoundOff) : 0m,
                                    CessPer = pMaster.CessPer != null ? Convert.ToDecimal(pMaster.CessPer) : 0m,
                                    CessAmt = pMaster.CessAmt != null ? Convert.ToDecimal(pMaster.CessAmt) : 0m,
                                    CalAfterTax = pMaster.CalAfterTax != null ? Convert.ToDecimal(pMaster.CalAfterTax) : 0m,
                                    CurrencyID = pMaster.CurrencyID != null ? (int?)pMaster.CurrencyID : 1,
                                    CurSymbol = pMaster.CurSymbol != null ? (string)pMaster.CurSymbol : "RM",
                                    SeriesID = pMaster.SeriesID != null ? (int)pMaster.SeriesID : 0,
                                    NetTotal = pMaster.NetTotal != null ? Convert.ToDecimal(pMaster.NetTotal) : 0m
                                };

                                // Fetch PDetails
                                const string pDetailsSql = @"
                                    SELECT 
                                        SlNo, ItemID, ItemName, UnitId, Unit, BaseUnit, 
                                        Packing, Qty, Free, Cost, DisPer, DisAmt, SalesPrice, 
                                        TaxPer, TaxAmt, TotalSP, OriginalCost, OriginalSP, TaxType, 
                                        SeriesID, CessAmt, CessPer
                                    FROM dbo.PDetails 
                                    WHERE PurchaseNo = @PurchaseNo 
                                    ORDER BY SlNo";

                                var details = await conn.QueryAsync<dynamic>(pDetailsSql, new { PurchaseNo = (int)pMaster.PurchaseNo });
                                foreach (var d in details)
                                {
                                    tx.PDetails.Add(new PDetailsSyncDto
                                    {
                                        SlNo = d.SlNo != null ? (int)d.SlNo : 1,
                                        ItemID = d.ItemID != null ? (int)d.ItemID : 0,
                                        Barcode = string.Empty,
                                        ItemName = d.ItemName != null ? (string)d.ItemName : string.Empty,
                                        UnitId = d.UnitId != null ? (int?)d.UnitId : null,
                                        Unit = d.Unit != null ? (string)d.Unit : string.Empty,
                                        BaseUnit = d.BaseUnit != null ? (string)d.BaseUnit : string.Empty,
                                        Packing = d.Packing != null ? Convert.ToDecimal(d.Packing) : 1.0m,
                                        Qty = d.Qty != null ? Convert.ToDecimal(d.Qty) : 0m,
                                        Free = d.Free != null ? Convert.ToDecimal(d.Free) : 0m,
                                        Cost = d.Cost != null ? Convert.ToDecimal(d.Cost) : 0m,
                                        DisPer = d.DisPer != null ? Convert.ToDecimal(d.DisPer) : 0m,
                                        DisAmt = d.DisAmt != null ? Convert.ToDecimal(d.DisAmt) : 0m,
                                        SalesPrice = d.SalesPrice != null ? Convert.ToDecimal(d.SalesPrice) : 0m,
                                        TaxPer = d.TaxPer != null ? Convert.ToDecimal(d.TaxPer) : 0m,
                                        TaxAmt = d.TaxAmt != null ? Convert.ToDecimal(d.TaxAmt) : 0m,
                                        TotalSP = d.TotalSP != null ? Convert.ToDecimal(d.TotalSP) : 0m,
                                        OriginalCost = d.OriginalCost != null ? (decimal?)Convert.ToDecimal(d.OriginalCost) : null,
                                        OriginalSP = d.OriginalSP != null ? (decimal?)Convert.ToDecimal(d.OriginalSP) : null,
                                        TaxType = d.TaxType != null ? (string)d.TaxType : "I",
                                        SeriesID = d.SeriesID != null ? (int)d.SeriesID : 0,
                                        CessAmt = d.CessAmt != null ? Convert.ToDecimal(d.CessAmt) : 0m,
                                        CessPer = d.CessPer != null ? Convert.ToDecimal(d.CessPer) : 0m
                                    });
                                }

                                // Fetch Vouchers for Purchase
                                if (pMaster.VoucherID != null)
                                {
                                    const string voucherSql = @"
                                        SELECT 
                                            VoucherID AS BranchVoucherId, LedgerID, LedgerName, 
                                            Debit, Credit, Narration 
                                        FROM dbo.Vouchers 
                                        WHERE VoucherID = @VoucherID AND VoucherType = 'Purchase'";

                                    var vouchers = await conn.QueryAsync<dynamic>(voucherSql, new { VoucherID = (int)pMaster.VoucherID });
                                    foreach (var v in vouchers)
                                    {
                                        tx.Vouchers.Add(new VoucherSyncDto
                                        {
                                            BranchVoucherId = (long)v.BranchVoucherId,
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
                            // Fetch SMaster
                            const string masterSql = @"
                                SELECT TOP 1 
                                    BillNo, BillDate, CompanyId, FinYearId, CounterId, 
                                    CustomerName, LedgerID, PaymodeId, PaymodeName, 
                                    SubTotal, DiscountAmt, TaxAmt, NetAmount, UserId, Status,
                                    VoucherID
                                FROM dbo.SMaster 
                                WHERE TransactionGuid = @TransactionGuid";

                            var master = await conn.QueryFirstOrDefaultAsync<dynamic>(
                                masterSql, new { item.TransactionGuid });

                            if (master != null)
                            {
                                tx.SMaster = new SMasterSyncDto
                                {
                                    BillNo = (long)master.BillNo,
                                    BillDate = (DateTime)master.BillDate,
                                    CompanyId = master.CompanyId != null ? (int)master.CompanyId : 1,
                                    FinYearId = master.FinYearId != null ? (int)master.FinYearId : 1,
                                    CounterId = master.CounterId != null ? (int)master.CounterId : 1,
                                    CustomerName = master.CustomerName != null ? (string)master.CustomerName : string.Empty,
                                    LedgerID = master.LedgerID != null ? (int?)master.LedgerID : null,
                                    PaymodeId = master.PaymodeId != null ? (int?)master.PaymodeId : null,
                                    PaymodeName = master.PaymodeName != null ? (string)master.PaymodeName : string.Empty,
                                    SubTotal = master.SubTotal != null ? (decimal)master.SubTotal : 0m,
                                    DiscountAmt = master.DiscountAmt != null ? (decimal)master.DiscountAmt : 0m,
                                    TaxAmt = master.TaxAmt != null ? (decimal)master.TaxAmt : 0m,
                                    NetAmount = master.NetAmount != null ? (decimal)master.NetAmount : 0m,
                                    UserId = master.UserId != null ? (int?)master.UserId : null,
                                    Status = master.Status != null ? (string)master.Status : "PAID"
                                };

                                // Fetch SDetails
                                const string detailsSql = @"
                                    SELECT 
                                        SlNO, ItemId, Barcode, ItemName, Qty, Packing, UnitPrice, 
                                        Amount, DiscountAmount, TaxAmt, TotalAmount, UnitId
                                    FROM dbo.SDetails 
                                    WHERE BillNo = @BillNo 
                                    ORDER BY SlNO";

                                var details = await conn.QueryAsync<dynamic>(detailsSql, new { BillNo = (long)master.BillNo });
                                foreach (var d in details)
                                {
                                    tx.SDetails.Add(new SDetailsSyncDto
                                    {
                                        SlNO = (int)d.SlNO,
                                        ItemId = (long)d.ItemId,
                                        Barcode = d.Barcode != null ? (string)d.Barcode : string.Empty,
                                        ItemName = d.ItemName != null ? (string)d.ItemName : string.Empty,
                                        Qty = (decimal)d.Qty,
                                        Packing = d.Packing != null ? (decimal)d.Packing : 1.0m,
                                        UnitPrice = (decimal)d.UnitPrice,
                                        Amount = (decimal)d.Amount,
                                        DiscountAmount = d.DiscountAmount != null ? (decimal?)d.DiscountAmount : null,
                                        TaxAmt = d.TaxAmt != null ? (decimal?)d.TaxAmt : null,
                                        TotalAmount = (decimal)d.TotalAmount,
                                        UnitId = d.UnitId != null ? (int?)d.UnitId : null
                                    });
                                }

                                // Fetch Vouchers for Sales
                                if (master.VoucherID != null)
                                {
                                    const string voucherSql = @"
                                        SELECT 
                                            VoucherID AS BranchVoucherId, LedgerID, LedgerName, 
                                            Debit, Credit, Narration 
                                        FROM dbo.Vouchers 
                                        WHERE VoucherID = @VoucherID AND VoucherType = 'Sales'";

                                    var vouchers = await conn.QueryAsync<dynamic>(voucherSql, new { VoucherID = (long)master.VoucherID });
                                    foreach (var v in vouchers)
                                    {
                                        tx.Vouchers.Add(new VoucherSyncDto
                                        {
                                            BranchVoucherId = (long)v.BranchVoucherId,
                                            LedgerID = v.LedgerID != null ? (int?)v.LedgerID : null,
                                            LedgerName = v.LedgerName != null ? (string)v.LedgerName : string.Empty,
                                            Debit = (decimal)v.Debit,
                                            Credit = (decimal)v.Credit,
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

                    var parameters = new DynamicParameters();
                    parameters.Add("@_Operation", "UPDATESTATUS");
                    parameters.Add("@TransactionGuid", transactionGuid);
                    parameters.Add("@Status", status);
                    parameters.Add("@ErrorMessage", string.IsNullOrEmpty(errorMessage) ? null : errorMessage);

                    await conn.ExecuteAsync(
                        "dbo.POS_SyncQueue",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("[ERROR] Failed to update SyncQueue for {0}: {1}", transactionGuid, ex.Message));
            }
        }

        public async Task ProcessResultsAsync(List<SyncItemResult> results)
        {
            if (results == null || results.Count == 0) return;

            foreach (var r in results)
            {
                string syncStatus = (r.Status == "Synced" || r.Status == "AlreadySynced") ? "SYNCED" : "FAILED";
                await UpdateQueueStatusAsync(r.TransactionGuid, syncStatus, r.ErrorMessage);
            }
        }
    }
}
