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

                    var p = new DynamicParameters();
                    p.Add("@_Operation", "GETPENDING");
                    p.Add("@BatchSize", batchSize);

                    var items = await conn.QueryAsync<SyncQueueItem>(
                        "dbo.POS_SyncQueue",
                        p,
                        commandType: CommandType.StoredProcedure);

                    return items.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] Error retrieving pending items from SyncQueue: " + ex.Message);
                Console.ResetColor();
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

                        if (item.EntityType.Equals("SALES", StringComparison.OrdinalIgnoreCase))
                        {
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

                                var details = await conn.QueryAsync<dynamic>(
                                    detailsSql, new { master.BillNo });

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

                                // Fetch Vouchers for accounting
                                if (master.VoucherID != null && (long)master.VoucherID > 0)
                                {
                                    const string voucherSql = @"
                                        SELECT 
                                            VoucherID, LedgerID, LedgerName, Debit, Credit, Narration
                                        FROM dbo.Vouchers 
                                        WHERE VoucherID = @VoucherID";

                                    var vouchers = await conn.QueryAsync<dynamic>(
                                        voucherSql, new { master.VoucherID });

                                    foreach (var v in vouchers)
                                    {
                                        tx.Vouchers.Add(new VoucherSyncDto
                                        {
                                            BranchVoucherId = (long)v.VoucherID,
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
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(string.Format("[ERROR] Failed to assemble payload for TransactionGuid {0}: {1}", item.TransactionGuid, ex.Message));
                        Console.ResetColor();
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

                    var p = new DynamicParameters();
                    p.Add("@_Operation", "UPDATESTATUS");
                    p.Add("@TransactionGuid", transactionGuid);
                    p.Add("@Status", status.ToUpperInvariant());
                    p.Add("@ErrorMessage", errorMessage);

                    await conn.ExecuteAsync(
                        "dbo.POS_SyncQueue",
                        p,
                        commandType: CommandType.StoredProcedure);

                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine(string.Format("[OK]   Queue updated -> GUID: {0} | Status: {1}", transactionGuid, status));
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(string.Format("[ERROR] Failed to update SyncQueue for {0}: {1}", transactionGuid, ex.Message));
                Console.ResetColor();
            }
        }

        public async Task ProcessResultsAsync(List<SyncItemResult> results)
        {
            if (results == null) return;
            foreach (var r in results)
            {
                await UpdateQueueStatusAsync(r.TransactionGuid, r.Status, r.ErrorMessage);
            }
        }
    }
}
