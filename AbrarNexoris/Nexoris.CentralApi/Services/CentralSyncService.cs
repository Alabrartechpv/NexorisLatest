using Dapper;
using Microsoft.Data.SqlClient;
using Nexoris.CentralApi.Models.DTOs;
using System.Data;

namespace Nexoris.CentralApi.Services
{
    public interface ICentralSyncService
    {
        Task<BatchSyncResponse> ProcessBatchAsync(BatchSyncRequest request);
        Task<bool> ValidateBranchKeyAsync(int branchId, string apiKey);
        Task<bool> CheckDatabaseHealthAsync();
    }

    public class CentralSyncService : ICentralSyncService
    {
        private readonly string _connectionString;
        private readonly ILogger<CentralSyncService> _logger;

        public CentralSyncService(IConfiguration config, ILogger<CentralSyncService> logger)
        {
            _connectionString = config.GetConnectionString("CentralDbConnection")
                ?? throw new InvalidOperationException("CentralDbConnection connection string is missing.");
            _logger = logger;
        }

        public async Task<bool> CheckDatabaseHealthAsync()
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();
                var result = await conn.ExecuteScalarAsync<int>("SELECT 1");
                return result == 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed for Central Database.");
                return false;
            }
        }

        public async Task<bool> ValidateBranchKeyAsync(int branchId, string apiKey)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();
                const string sql = @"
                    SELECT COUNT(1) 
                    FROM dbo.BranchApiKeys 
                    WHERE BranchId = @BranchId 
                      AND ApiKey = @ApiKey 
                      AND IsActive = 1";

                int count = await conn.ExecuteScalarAsync<int>(sql, new { BranchId = branchId, ApiKey = apiKey });
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating branch API key for BranchId={BranchId}", branchId);
                return false;
            }
        }

        public async Task<BatchSyncResponse> ProcessBatchAsync(BatchSyncRequest request)
        {
            var response = new BatchSyncResponse
            {
                BatchId = request.BatchId,
                ProcessedUtc = DateTime.UtcNow
            };

            foreach (var tx in request.Transactions)
            {
                try
                {
                    var itemResult = await IngestTransactionAsync(request.BranchId, tx);
                    response.Results.Add(itemResult);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to ingest transaction {Guid}", tx.TransactionGuid);
                    response.Results.Add(new SyncItemResult
                    {
                        TransactionGuid = tx.TransactionGuid,
                        EntityType = tx.EntityType,
                        EntityId = tx.SMaster?.BillNo.ToString() ?? "Unknown",
                        Status = "Failed",
                        ErrorMessage = ex.Message
                    });
                }
            }

            return response;
        }

        private async Task<SyncItemResult> IngestTransactionAsync(int branchId, TransactionSyncDto tx)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string entityId = tx.SMaster?.BillNo.ToString() ?? "Unknown";

            // 1. DEDUPLICATION / IDEMPOTENCY CHECK
            const string findGuidSql = @"
                SELECT TOP 1 CentralTransactionID, CancelFlag 
                FROM dbo.SMaster 
                WHERE TransactionGuid = @TransactionGuid";

            var existing = await conn.QueryFirstOrDefaultAsync<dynamic>(findGuidSql, new { tx.TransactionGuid });

            // HANDLE ALREADY SYNCED 'CREATE'
            if (existing != null && tx.Operation.Equals("CREATE", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Transaction {Guid} already exists in Central DB. Returning AlreadySynced.", tx.TransactionGuid);
                return new SyncItemResult
                {
                    TransactionGuid = tx.TransactionGuid,
                    EntityType = tx.EntityType,
                    EntityId = entityId,
                    Status = "AlreadySynced",
                    CentralTransactionId = (long)existing.CentralTransactionID
                };
            }

            // HANDLE 'CANCEL' / VOID
            if (tx.Operation.Equals("CANCEL", StringComparison.OrdinalIgnoreCase))
            {
                using var cancelTrans = conn.BeginTransaction();
                try
                {
                    const string cancelSql = @"
                        UPDATE dbo.SMaster 
                        SET CancelFlag = 1 
                        WHERE TransactionGuid = @TransactionGuid;

                        UPDATE dbo.Vouchers 
                        SET CancelFlag = 1 
                        WHERE TransactionGuid = @TransactionGuid;";

                    await conn.ExecuteAsync(cancelSql, new { tx.TransactionGuid }, transaction: cancelTrans);
                    cancelTrans.Commit();

                    return new SyncItemResult
                    {
                        TransactionGuid = tx.TransactionGuid,
                        EntityType = tx.EntityType,
                        EntityId = entityId,
                        Status = "Synced",
                        CentralTransactionId = existing != null ? (long)existing.CentralTransactionID : null
                    };
                }
                catch
                {
                    cancelTrans.Rollback();
                    throw;
                }
            }

            // HANDLE 'UPDATE' (Overwrite details and update header)
            if (existing != null && tx.Operation.Equals("UPDATE", StringComparison.OrdinalIgnoreCase))
            {
                long centralId = (long)existing.CentralTransactionID;
                using var updateTrans = conn.BeginTransaction();
                try
                {
                    const string updateMasterSql = @"
                        UPDATE dbo.SMaster SET
                            BillDate = @BillDate,
                            CustomerName = @CustomerName,
                            LedgerID = @LedgerID,
                            PaymodeId = @PaymodeId,
                            PaymodeName = @PaymodeName,
                            SubTotal = @SubTotal,
                            DiscountAmt = @DiscountAmt,
                            TaxAmt = @TaxAmt,
                            NetAmount = @NetAmount,
                            UserId = @UserId,
                            Status = @Status,
                            SyncReceivedUtc = GETUTCDATE()
                        WHERE CentralTransactionID = @CentralTransactionID;";

                    await conn.ExecuteAsync(updateMasterSql, new
                    {
                        CentralTransactionID = centralId,
                        tx.SMaster.BillDate,
                        tx.SMaster.CustomerName,
                        tx.SMaster.LedgerID,
                        tx.SMaster.PaymodeId,
                        tx.SMaster.PaymodeName,
                        tx.SMaster.SubTotal,
                        tx.SMaster.DiscountAmt,
                        tx.SMaster.TaxAmt,
                        tx.SMaster.NetAmount,
                        tx.SMaster.UserId,
                        tx.SMaster.Status
                    }, transaction: updateTrans);

                    // Replace SDetails
                    await conn.ExecuteAsync(
                        "DELETE FROM dbo.SDetails WHERE CentralTransactionID = @CentralTransactionID",
                        new { CentralTransactionID = centralId }, transaction: updateTrans);

                    const string insertDetailsSql = @"
                        INSERT INTO dbo.SDetails (
                            CentralTransactionID, BranchId, TransactionGuid, BillNo, SlNO, 
                            ItemId, Barcode, ItemName, Qty, UnitPrice, Amount, 
                            DiscountAmount, TaxAmt, TotalAmount, UnitId, SyncReceivedUtc
                        )
                        VALUES (
                            @CentralTransactionID, @BranchId, @TransactionGuid, @BillNo, @SlNO, 
                            @ItemId, @Barcode, @ItemName, @Qty, @UnitPrice, @Amount, 
                            @DiscountAmount, @TaxAmt, @TotalAmount, @UnitId, GETUTCDATE()
                        );";

                    foreach (var d in tx.SDetails)
                    {
                        await conn.ExecuteAsync(insertDetailsSql, new
                        {
                            CentralTransactionID = centralId,
                            BranchId = branchId,
                            tx.TransactionGuid,
                            tx.SMaster.BillNo,
                            d.SlNO,
                            d.ItemId,
                            d.Barcode,
                            d.ItemName,
                            d.Qty,
                            d.UnitPrice,
                            d.Amount,
                            d.DiscountAmount,
                            d.TaxAmt,
                            d.TotalAmount,
                            d.UnitId
                        }, transaction: updateTrans);
                    }

                    updateTrans.Commit();

                    return new SyncItemResult
                    {
                        TransactionGuid = tx.TransactionGuid,
                        EntityType = tx.EntityType,
                        EntityId = entityId,
                        Status = "Synced",
                        CentralTransactionId = centralId
                    };
                }
                catch
                {
                    updateTrans.Rollback();
                    throw;
                }
            }

            // HANDLE NEW 'CREATE'
            using (var insertTrans = conn.BeginTransaction())
            {
                try
                {
                    const string insertMasterSql = @"
                        INSERT INTO dbo.SMaster (
                            BranchId, TransactionGuid, BillNo, BillDate, CompanyId, FinYearId, 
                            CounterId, CustomerName, LedgerID, PaymodeId, PaymodeName, 
                            SubTotal, DiscountAmt, TaxAmt, NetAmount, UserId, CancelFlag, Status, SyncReceivedUtc
                        )
                        OUTPUT INSERTED.CentralTransactionID
                        VALUES (
                            @BranchId, @TransactionGuid, @BillNo, @BillDate, @CompanyId, @FinYearId, 
                            @CounterId, @CustomerName, @LedgerID, @PaymodeId, @PaymodeName, 
                            @SubTotal, @DiscountAmt, @TaxAmt, @NetAmount, @UserId, 0, @Status, GETUTCDATE()
                        );";

                    long centralId = await conn.ExecuteScalarAsync<long>(insertMasterSql, new
                    {
                        BranchId = branchId,
                        tx.TransactionGuid,
                        tx.SMaster.BillNo,
                        tx.SMaster.BillDate,
                        tx.SMaster.CompanyId,
                        tx.SMaster.FinYearId,
                        tx.SMaster.CounterId,
                        tx.SMaster.CustomerName,
                        tx.SMaster.LedgerID,
                        tx.SMaster.PaymodeId,
                        tx.SMaster.PaymodeName,
                        tx.SMaster.SubTotal,
                        tx.SMaster.DiscountAmt,
                        tx.SMaster.TaxAmt,
                        tx.SMaster.NetAmount,
                        tx.SMaster.UserId,
                        tx.SMaster.Status
                    }, transaction: insertTrans);

                    // Insert SDetails line items
                    const string insertDetailsSql = @"
                        INSERT INTO dbo.SDetails (
                            CentralTransactionID, BranchId, TransactionGuid, BillNo, SlNO, 
                            ItemId, Barcode, ItemName, Qty, UnitPrice, Amount, 
                            DiscountAmount, TaxAmt, TotalAmount, UnitId, SyncReceivedUtc
                        )
                        VALUES (
                            @CentralTransactionID, @BranchId, @TransactionGuid, @BillNo, @SlNO, 
                            @ItemId, @Barcode, @ItemName, @Qty, @UnitPrice, @Amount, 
                            @DiscountAmount, @TaxAmt, @TotalAmount, @UnitId, GETUTCDATE()
                        );";

                    foreach (var d in tx.SDetails)
                    {
                        await conn.ExecuteAsync(insertDetailsSql, new
                        {
                            CentralTransactionID = centralId,
                            BranchId = branchId,
                            tx.TransactionGuid,
                            tx.SMaster.BillNo,
                            d.SlNO,
                            d.ItemId,
                            d.Barcode,
                            d.ItemName,
                            d.Qty,
                            d.UnitPrice,
                            d.Amount,
                            d.DiscountAmount,
                            d.TaxAmt,
                            d.TotalAmount,
                            d.UnitId
                        }, transaction: insertTrans);
                    }

                    // Insert Vouchers accounting entries
                    const string insertVoucherSql = @"
                        INSERT INTO dbo.Vouchers (
                            CentralTransactionID, BranchId, TransactionGuid, BranchVoucherID, 
                            LedgerID, LedgerName, Debit, Credit, Narration, CancelFlag, SyncReceivedUtc
                        )
                        VALUES (
                            @CentralTransactionID, @BranchId, @TransactionGuid, @BranchVoucherID, 
                            @LedgerID, @LedgerName, @Debit, @Credit, @Narration, 0, GETUTCDATE()
                        );";

                    foreach (var v in tx.Vouchers)
                    {
                        await conn.ExecuteAsync(insertVoucherSql, new
                        {
                            CentralTransactionID = centralId,
                            BranchId = branchId,
                            tx.TransactionGuid,
                            v.BranchVoucherId,
                            v.LedgerID,
                            v.LedgerName,
                            v.Debit,
                            v.Credit,
                            v.Narration
                        }, transaction: insertTrans);
                    }

                    insertTrans.Commit();

                    return new SyncItemResult
                    {
                        TransactionGuid = tx.TransactionGuid,
                        EntityType = tx.EntityType,
                        EntityId = entityId,
                        Status = "Synced",
                        CentralTransactionId = centralId
                    };
                }
                catch
                {
                    insertTrans.Rollback();
                    throw;
                }
            }
        }
    }
}
