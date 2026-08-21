using Dapper;
using Nexoris.CentralApi.Models.DTOs;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Nexoris.CentralApi.Services
{
    public interface ICentralSyncService
    {
        Task<BatchSyncResponse> ProcessBatchAsync(BatchSyncRequest request);
        Task<bool> ValidateBranchKeyAsync(int branchId, string apiKey);
        Task<bool> CheckDatabaseHealthAsync();
        Task<BranchStatusResponse> GetBranchStatusAsync(int branchId);
        Task<MasterDataSyncResponse> IngestMasterDataAsync(MasterDataSyncRequest request);
    }

    public class CentralSyncService : ICentralSyncService
    {
        private readonly string _connectionString;

        public CentralSyncService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["CentralDbConnection"]?.ConnectionString
                ?? "Server=192.168.1.232\\SQLEXPRESS;Database=NexorisCentralDB;User Id=sa;Password=Abrar@123;Connect Timeout=30;";
        }

        public async Task<BranchStatusResponse> GetBranchStatusAsync(int branchId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var status = await conn.QueryFirstOrDefaultAsync<dynamic>(
                    "dbo.sp_Central_GetBranchStatus",
                    new { BranchId = branchId },
                    commandType: CommandType.StoredProcedure
                );

                if (status != null)
                {
                    int itemCount = Convert.ToInt32(status.ExistingItemCount);
                    bool isActive = Convert.ToBoolean(status.IsActive);
                    return new BranchStatusResponse
                    {
                        BranchId = branchId,
                        IsActive = isActive,
                        InitialSyncRequired = itemCount == 0,
                        ExistingItemCount = itemCount,
                        ServerUtc = DateTime.UtcNow
                    };
                }

                return new BranchStatusResponse
                {
                    BranchId = branchId,
                    IsActive = false,
                    InitialSyncRequired = true,
                    ExistingItemCount = 0,
                    ServerUtc = DateTime.UtcNow
                };
            }
        }

        public async Task<MasterDataSyncResponse> IngestMasterDataAsync(MasterDataSyncRequest request)
        {
            if (request == null || request.PriceSettings == null || request.PriceSettings.Count == 0)
            {
                return new MasterDataSyncResponse
                {
                    BranchId = request != null ? request.BranchId : 0,
                    Success = false,
                    SyncedItemCount = 0,
                    Message = "No PriceSettings records provided in payload."
                };
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        if (request.Item != null)
                        {
                            await conn.ExecuteAsync(
                                "dbo.sp_Central_UpsertItemMaster",
                                new
                                {
                                    CompanyId = request.Item.CompanyId ?? 1,
                                    BranchId = request.BranchId,
                                    FinYearId = request.Item.FinYearId ?? 1,
                                    request.Item.ItemId,
                                    request.Item.ItemNo,
                                    request.Item.Description,
                                    request.Item.BarCode,
                                    request.Item.ItemTypeId,
                                    request.Item.VendorId,
                                    request.Item.BrandId,
                                    request.Item.GroupId,
                                    request.Item.CategoryId,
                                    request.Item.SubCategoryId,
                                    Active = request.Item.Active,
                                    Hide = request.Item.Hide,
                                    request.Item.BaseUnitId,
                                    request.Item.HSNCode
                                },
                                transaction: trans,
                                commandType: CommandType.StoredProcedure
                            );
                        }

                        foreach (var item in request.PriceSettings)
                        {
                            await conn.ExecuteAsync(
                                "dbo.sp_Central_UpsertPriceSetting",
                                new
                                {
                                    CompanyId = item.CompanyId ?? 1,
                                    FinYearId = item.FinYearId ?? 1,
                                    BranchId = request.BranchId,
                                    item.BranchName,
                                    item.ItemId,
                                    UnitId = item.UnitId,
                                    Unit = item.Unit ?? "UNIT",
                                    Packing = item.Packing > 0 ? item.Packing : 1.0m,
                                    Cost = item.Cost,
                                    MarginPer = item.MarginPer,
                                    MarginAmt = item.MarginAmt,
                                    TaxPer = item.TaxPer,
                                    TaxAmt = item.TaxAmt,
                                    RetailPrice = item.RetailPrice,
                                    WholeSalePrice = item.WholeSalePrice,
                                    CreditPrice = item.CreditPrice,
                                    CardPrice = item.CardPrice,
                                    Stock = item.Stock,
                                    StockValue = item.StockValue,
                                    ReOrder = item.ReOrder,
                                    BarCode = item.BarCode,
                                    TaxType = item.TaxType,
                                    OpnStk = item.OpnStk,
                                    OpnValue = item.OpnValue,
                                    IsBaseUnit = string.IsNullOrEmpty(item.IsBaseUnit) ? "Y" : item.IsBaseUnit,
                                    MRP = item.MRP
                                },
                                transaction: trans,
                                commandType: CommandType.StoredProcedure
                            );
                        }

                        trans.Commit();
                        return new MasterDataSyncResponse
                        {
                            BranchId = request.BranchId,
                            Success = true,
                            SyncedItemCount = request.PriceSettings.Count,
                            Message = $"Successfully synced {request.PriceSettings.Count} PriceSettings item(s)."
                        };
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return new MasterDataSyncResponse
                        {
                            BranchId = request.BranchId,
                            Success = false,
                            SyncedItemCount = 0,
                            Message = "Error ingesting Master Data: " + ex.Message
                        };
                    }
                }
            }
        }

        public async Task<bool> CheckDatabaseHealthAsync()
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    var result = await conn.ExecuteScalarAsync<int>("SELECT 1");
                    return result == 1;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] Health check failed for Central Database: " + ex.Message);
                Console.ResetColor();
                return false;
            }
        }

        public async Task<bool> ValidateBranchKeyAsync(int branchId, string apiKey)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    int count = await conn.ExecuteScalarAsync<int>(
                        "dbo.sp_Central_ValidateApiKey",
                        new { BranchId = branchId, ApiKey = apiKey },
                        commandType: CommandType.StoredProcedure
                    );
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(string.Format("[ERROR] Error validating branch API key for BranchId={0}: {1}", branchId, ex.Message));
                Console.ResetColor();
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
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(string.Format("[ERROR] Failed to ingest transaction {0}: {1}", tx.TransactionGuid, ex.Message));
                    Console.ResetColor();

                    string entityId = "Unknown";
                    if (tx.EntityType.Equals("PURCHASE", StringComparison.OrdinalIgnoreCase) && tx.PMaster != null)
                        entityId = tx.PMaster.PurchaseNo.ToString();
                    else if (tx.SMaster != null)
                        entityId = tx.SMaster.BillNo.ToString();

                    response.Results.Add(new SyncItemResult
                    {
                        TransactionGuid = tx.TransactionGuid,
                        EntityType = tx.EntityType,
                        EntityId = entityId,
                        Status = "Failed",
                        ErrorMessage = ex.Message
                    });
                }
            }

            return response;
        }

        private async Task<SyncItemResult> IngestTransactionAsync(int branchId, TransactionSyncDto tx)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                if (tx.EntityType.Equals("PURCHASE", StringComparison.OrdinalIgnoreCase))
                {
                    return await IngestPurchaseTransactionAsync(conn, branchId, tx);
                }
                else
                {
                    return await IngestSalesTransactionAsync(conn, branchId, tx);
                }
            }
        }

        #region Sales Transaction Ingest
        private async Task<SyncItemResult> IngestSalesTransactionAsync(SqlConnection conn, int branchId, TransactionSyncDto tx)
        {
            string entityId = tx.SMaster != null ? tx.SMaster.BillNo.ToString() : "Unknown";

            if (tx.SMaster == null && !tx.Operation.Equals("CANCEL", StringComparison.OrdinalIgnoreCase))
            {
                return new SyncItemResult
                {
                    TransactionGuid = tx.TransactionGuid,
                    EntityType = tx.EntityType,
                    EntityId = entityId,
                    Status = "Failed",
                    ErrorMessage = "Payload missing SMaster header record."
                };
            }

            // 1. DEDUPLICATION / IDEMPOTENCY CHECK
            const string findGuidSql = @"
                SELECT TOP 1 CentralTransactionID, CancelFlag 
                FROM dbo.SMaster 
                WHERE TransactionGuid = @TransactionGuid";

            var existing = await conn.QueryFirstOrDefaultAsync<dynamic>(findGuidSql, new { tx.TransactionGuid });

            // HANDLE ALREADY SYNCED 'CREATE'
            if (existing != null && tx.Operation.Equals("CREATE", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(string.Format("[INFO] Sales Transaction {0} already exists in Central DB. Returning AlreadySynced.", tx.TransactionGuid));
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
                using (var cancelTrans = conn.BeginTransaction())
                {
                    try
                    {
                        const string cancelSql = @"
                            UPDATE dbo.SMaster 
                            SET CancelFlag = 1 
                            WHERE TransactionGuid = @TransactionGuid;

                            UPDATE dbo.Vouchers 
                            SET CancelFlag = 1 
                            WHERE TransactionGuid = @TransactionGuid;

                            UPDATE PS 
                            SET PS.Stock = PS.Stock + (SD.Qty * ISNULL(SD.Packing, 1)), PS.LastSyncUtc = GETUTCDATE()
                            FROM dbo.PriceSettings PS
                            INNER JOIN dbo.SDetails SD ON PS.BranchId = SD.BranchId AND PS.ItemId = SD.ItemId
                            WHERE SD.TransactionGuid = @TransactionGuid AND (PS.IsBaseUnit = 'Y' OR PS.IsBaseUnit IS NULL);";

                        await conn.ExecuteAsync(cancelSql, new { tx.TransactionGuid }, transaction: cancelTrans);
                        cancelTrans.Commit();

                        return new SyncItemResult
                        {
                            TransactionGuid = tx.TransactionGuid,
                            EntityType = tx.EntityType,
                            EntityId = entityId,
                            Status = "Synced",
                            CentralTransactionId = existing != null ? (long)existing.CentralTransactionID : (long?)null
                        };
                    }
                    catch
                    {
                        cancelTrans.Rollback();
                        throw;
                    }
                }
            }

            // HANDLE 'UPDATE' (Overwrite details and update header)
            if (existing != null && tx.Operation.Equals("UPDATE", StringComparison.OrdinalIgnoreCase))
            {
                long centralId = (long)existing.CentralTransactionID;
                using (var updateTrans = conn.BeginTransaction())
                {
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

                        // Restore previous details stock before replacing (targeting BaseUnit)
                        const string restoreOldStockSql = @"
                            UPDATE PS 
                            SET PS.Stock = PS.Stock + (SD.Qty * ISNULL(SD.Packing, 1)), PS.LastSyncUtc = GETUTCDATE()
                            FROM dbo.PriceSettings PS
                            INNER JOIN dbo.SDetails SD ON PS.BranchId = SD.BranchId AND PS.ItemId = SD.ItemId
                            WHERE SD.CentralTransactionID = @CentralTransactionID AND (PS.IsBaseUnit = 'Y' OR PS.IsBaseUnit IS NULL);";

                        await conn.ExecuteAsync(restoreOldStockSql, new { CentralTransactionID = centralId }, transaction: updateTrans);

                        // Replace SDetails
                        await conn.ExecuteAsync(
                            "DELETE FROM dbo.SDetails WHERE CentralTransactionID = @CentralTransactionID",
                            new { CentralTransactionID = centralId }, transaction: updateTrans);

                        const string insertDetailsSql = @"
                            INSERT INTO dbo.SDetails (
                                CentralTransactionID, BranchId, TransactionGuid, BillNo, SlNO, 
                                ItemId, Barcode, ItemName, Qty, Packing, UnitPrice, Amount, 
                                DiscountAmount, TaxAmt, TotalAmount, UnitId, SyncReceivedUtc
                            )
                            VALUES (
                                @CentralTransactionID, @BranchId, @TransactionGuid, @BillNo, @SlNO, 
                                @ItemId, @Barcode, @ItemName, @Qty, @Packing, @UnitPrice, @Amount, 
                                @DiscountAmount, @TaxAmt, @TotalAmount, @UnitId, GETUTCDATE()
                            );";

                        const string deductStockSql = @"
                            IF EXISTS (SELECT 1 FROM dbo.PriceSettings WHERE BranchId = @BranchId AND ItemId = @ItemId)
                            BEGIN
                                UPDATE dbo.PriceSettings 
                                SET Stock = Stock - (@Qty * ISNULL(@Packing, 1)), LastSyncUtc = GETUTCDATE() 
                                WHERE BranchId = @BranchId AND ItemId = @ItemId AND (IsBaseUnit = 'Y' OR IsBaseUnit IS NULL);
                            END
                            ELSE
                            BEGIN
                                INSERT INTO dbo.PriceSettings (BranchId, ItemId, UnitId, Packing, Stock, RetailPrice, IsBaseUnit, LastSyncUtc)
                                VALUES (@BranchId, @ItemId, @UnitId, ISNULL(@Packing, 1), -(@Qty * ISNULL(@Packing, 1)), @UnitPrice, 'Y', GETUTCDATE());
                            END";

                        foreach (var d in tx.SDetails)
                        {
                            decimal packingVal = d.Packing > 0 ? d.Packing : 1.0m;
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
                                Packing = packingVal,
                                d.UnitPrice,
                                d.Amount,
                                d.DiscountAmount,
                                d.TaxAmt,
                                d.TotalAmount,
                                d.UnitId
                            }, transaction: updateTrans);

                            await conn.ExecuteAsync(deductStockSql, new
                            {
                                BranchId = branchId,
                                d.ItemId,
                                d.UnitId,
                                d.Qty,
                                Packing = packingVal,
                                d.UnitPrice
                            }, transaction: updateTrans);
                        }

                        // Replace Vouchers if provided
                        if (tx.Vouchers != null && tx.Vouchers.Count > 0)
                        {
                            await conn.ExecuteAsync(
                                "DELETE FROM dbo.Vouchers WHERE CentralTransactionID = @CentralTransactionID",
                                new { CentralTransactionID = centralId }, transaction: updateTrans);

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
                                }, transaction: updateTrans);
                            }
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
                            ItemId, Barcode, ItemName, Qty, Packing, UnitPrice, Amount, 
                            DiscountAmount, TaxAmt, TotalAmount, UnitId, SyncReceivedUtc
                        )
                        VALUES (
                            @CentralTransactionID, @BranchId, @TransactionGuid, @BillNo, @SlNO, 
                            @ItemId, @Barcode, @ItemName, @Qty, @Packing, @UnitPrice, @Amount, 
                            @DiscountAmount, @TaxAmt, @TotalAmount, @UnitId, GETUTCDATE()
                        );";

                    const string deductStockCreateSql = @"
                        IF EXISTS (SELECT 1 FROM dbo.PriceSettings WHERE BranchId = @BranchId AND ItemId = @ItemId)
                        BEGIN
                            UPDATE dbo.PriceSettings 
                            SET Stock = Stock - (@Qty * ISNULL(@Packing, 1)), LastSyncUtc = GETUTCDATE() 
                            WHERE BranchId = @BranchId AND ItemId = @ItemId AND (IsBaseUnit = 'Y' OR IsBaseUnit IS NULL);
                        END
                        ELSE
                        BEGIN
                            INSERT INTO dbo.PriceSettings (BranchId, ItemId, UnitId, Packing, Stock, RetailPrice, IsBaseUnit, LastSyncUtc)
                            VALUES (@BranchId, @ItemId, @UnitId, ISNULL(@Packing, 1), -(@Qty * ISNULL(@Packing, 1)), @UnitPrice, 'Y', GETUTCDATE());
                        END";

                    foreach (var d in tx.SDetails)
                    {
                        decimal packingVal = d.Packing > 0 ? d.Packing : 1.0m;
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
                            Packing = packingVal,
                            d.UnitPrice,
                            d.Amount,
                            d.DiscountAmount,
                            d.TaxAmt,
                            d.TotalAmount,
                            d.UnitId
                        }, transaction: insertTrans);

                        await conn.ExecuteAsync(deductStockCreateSql, new
                        {
                            BranchId = branchId,
                            d.ItemId,
                            d.UnitId,
                            d.Qty,
                            Packing = packingVal,
                            d.UnitPrice
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
        #endregion

        #region Purchase Transaction Ingest
        private async Task<SyncItemResult> IngestPurchaseTransactionAsync(SqlConnection conn, int branchId, TransactionSyncDto tx)
        {
            string entityId = tx.PMaster != null ? tx.PMaster.PurchaseNo.ToString() : "Unknown";

            if (tx.PMaster == null && !tx.Operation.Equals("CANCEL", StringComparison.OrdinalIgnoreCase))
            {
                return new SyncItemResult
                {
                    TransactionGuid = tx.TransactionGuid,
                    EntityType = tx.EntityType,
                    EntityId = entityId,
                    Status = "Failed",
                    ErrorMessage = "Payload missing PMaster header record."
                };
            }

            // 1. DEDUPLICATION / IDEMPOTENCY CHECK
            const string findGuidSql = @"
                SELECT TOP 1 CentralPurchaseID, CancelFlag 
                FROM dbo.PMaster 
                WHERE TransactionGuid = @TransactionGuid";

            var existing = await conn.QueryFirstOrDefaultAsync<dynamic>(findGuidSql, new { tx.TransactionGuid });

            // HANDLE ALREADY SYNCED 'CREATE'
            if (existing != null && tx.Operation.Equals("CREATE", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(string.Format("[INFO] Purchase Transaction {0} already exists in Central DB. Returning AlreadySynced.", tx.TransactionGuid));
                return new SyncItemResult
                {
                    TransactionGuid = tx.TransactionGuid,
                    EntityType = tx.EntityType,
                    EntityId = entityId,
                    Status = "AlreadySynced",
                    CentralTransactionId = (long)existing.CentralPurchaseID
                };
            }

            // HANDLE 'CANCEL' / VOID
            if (tx.Operation.Equals("CANCEL", StringComparison.OrdinalIgnoreCase))
            {
                using (var cancelTrans = conn.BeginTransaction())
                {
                    try
                    {
                        const string cancelSql = @"
                            UPDATE dbo.PMaster 
                            SET CancelFlag = 1 
                            WHERE TransactionGuid = @TransactionGuid;

                            UPDATE dbo.Vouchers 
                            SET CancelFlag = 1 
                            WHERE TransactionGuid = @TransactionGuid;

                            -- Reverse stock added by purchase (target BaseUnit)
                            UPDATE PS 
                            SET PS.Stock = PS.Stock - ((PD.Qty + ISNULL(PD.Free, 0)) * ISNULL(PD.Packing, 1)), PS.LastSyncUtc = GETUTCDATE()
                            FROM dbo.PriceSettings PS
                            INNER JOIN dbo.PDetails PD ON PS.BranchId = PD.BranchId AND PS.ItemId = PD.ItemID
                            WHERE PD.TransactionGuid = @TransactionGuid AND (PS.IsBaseUnit = 'Y' OR PS.IsBaseUnit IS NULL);";

                        await conn.ExecuteAsync(cancelSql, new { tx.TransactionGuid }, transaction: cancelTrans);
                        cancelTrans.Commit();

                        return new SyncItemResult
                        {
                            TransactionGuid = tx.TransactionGuid,
                            EntityType = tx.EntityType,
                            EntityId = entityId,
                            Status = "Synced",
                            CentralTransactionId = existing != null ? (long)existing.CentralPurchaseID : (long?)null
                        };
                    }
                    catch
                    {
                        cancelTrans.Rollback();
                        throw;
                    }
                }
            }

            // HANDLE 'UPDATE' (Overwrite details, adjust stock, and update header)
            if (existing != null && tx.Operation.Equals("UPDATE", StringComparison.OrdinalIgnoreCase))
            {
                long centralId = (long)existing.CentralPurchaseID;
                using (var updateTrans = conn.BeginTransaction())
                {
                    try
                    {
                        const string updateMasterSql = @"
                            UPDATE dbo.PMaster SET
                                PurchaseDate = @PurchaseDate,
                                InvoiceNo = @InvoiceNo,
                                InvoiceDate = @InvoiceDate,
                                LedgerID = @LedgerID,
                                VendorName = @VendorName,
                                PaymodeID = @PaymodeID,
                                Paymode = @Paymode,
                                CreditPeriod = @CreditPeriod,
                                SubTotal = @SubTotal,
                                SpDisPer = @SpDisPer,
                                SpDsiAmt = @SpDsiAmt,
                                BillDiscountPer = @BillDiscountPer,
                                BillDiscountAmt = @BillDiscountAmt,
                                TaxPer = @TaxPer,
                                TaxAmt = @TaxAmt,
                                Frieght = @Frieght,
                                ExpenseAmt = @ExpenseAmt,
                                OtherExpAmt = @OtherExpAmt,
                                GrandTotal = @GrandTotal,
                                UserID = @UserID,
                                UserName = @UserName,
                                TaxType = @TaxType,
                                Remarks = @Remarks,
                                RoundOff = @RoundOff,
                                CessPer = @CessPer,
                                CessAmt = @CessAmt,
                                CalAfterTax = @CalAfterTax,
                                CurrencyID = @CurrencyID,
                                CurSymbol = @CurSymbol,
                                SeriesID = @SeriesID,
                                NetTotal = @NetTotal,
                                SyncReceivedUtc = GETUTCDATE()
                            WHERE CentralPurchaseID = @CentralPurchaseID;";

                        await conn.ExecuteAsync(updateMasterSql, new
                        {
                            CentralPurchaseID = centralId,
                            PurchaseDate = SafeSqlDate(tx.PMaster.PurchaseDate),
                            tx.PMaster.InvoiceNo,
                            InvoiceDate = SafeSqlDate(tx.PMaster.InvoiceDate),
                            tx.PMaster.LedgerID,
                            tx.PMaster.VendorName,
                            tx.PMaster.PaymodeID,
                            tx.PMaster.Paymode,
                            tx.PMaster.CreditPeriod,
                            tx.PMaster.SubTotal,
                            tx.PMaster.SpDisPer,
                            tx.PMaster.SpDsiAmt,
                            tx.PMaster.BillDiscountPer,
                            tx.PMaster.BillDiscountAmt,
                            tx.PMaster.TaxPer,
                            tx.PMaster.TaxAmt,
                            tx.PMaster.Frieght,
                            tx.PMaster.ExpenseAmt,
                            tx.PMaster.OtherExpAmt,
                            tx.PMaster.GrandTotal,
                            tx.PMaster.UserID,
                            tx.PMaster.UserName,
                            tx.PMaster.TaxType,
                            tx.PMaster.Remarks,
                            tx.PMaster.RoundOff,
                            tx.PMaster.CessPer,
                            tx.PMaster.CessAmt,
                            tx.PMaster.CalAfterTax,
                            tx.PMaster.CurrencyID,
                            tx.PMaster.CurSymbol,
                            tx.PMaster.SeriesID,
                            tx.PMaster.NetTotal
                        }, transaction: updateTrans);

                        // Reverse previous purchase stock before replacing (targeting BaseUnit)
                        const string reverseOldStockSql = @"
                            UPDATE PS 
                            SET PS.Stock = PS.Stock - ((PD.Qty + ISNULL(PD.Free, 0)) * ISNULL(PD.Packing, 1)), PS.LastSyncUtc = GETUTCDATE()
                            FROM dbo.PriceSettings PS
                            INNER JOIN dbo.PDetails PD ON PS.BranchId = PD.BranchId AND PS.ItemId = PD.ItemID
                            WHERE PD.CentralPurchaseID = @CentralPurchaseID AND (PS.IsBaseUnit = 'Y' OR PS.IsBaseUnit IS NULL);";

                        await conn.ExecuteAsync(reverseOldStockSql, new { CentralPurchaseID = centralId }, transaction: updateTrans);

                        // Delete old PDetails
                        await conn.ExecuteAsync(
                            "DELETE FROM dbo.PDetails WHERE CentralPurchaseID = @CentralPurchaseID",
                            new { CentralPurchaseID = centralId }, transaction: updateTrans);

                        const string insertDetailsSql = @"
                            INSERT INTO dbo.PDetails (
                                CentralPurchaseID, BranchId, TransactionGuid, PurchaseNo, SlNo, 
                                ItemID, Barcode, ItemName, UnitId, Unit, BaseUnit, Packing, 
                                Qty, Free, Cost, DisPer, DisAmt, SalesPrice, TaxPer, TaxAmt, 
                                TotalSP, OriginalCost, OriginalSP, TaxType, SeriesID, CessAmt, CessPer, SyncReceivedUtc
                            )
                            VALUES (
                                @CentralPurchaseID, @BranchId, @TransactionGuid, @PurchaseNo, @SlNo, 
                                @ItemID, @Barcode, @ItemName, @UnitId, @Unit, @BaseUnit, @Packing, 
                                @Qty, @Free, @Cost, @DisPer, @DisAmt, @SalesPrice, @TaxPer, @TaxAmt, 
                                @TotalSP, @OriginalCost, @OriginalSP, @TaxType, @SeriesID, @CessAmt, @CessPer, GETUTCDATE()
                            );";

                        const string addStockSql = @"
                            IF EXISTS (SELECT 1 FROM dbo.PriceSettings WHERE BranchId = @BranchId AND ItemId = @ItemId)
                            BEGIN
                                UPDATE dbo.PriceSettings 
                                SET Stock = Stock + ((@Qty + @Free) * ISNULL(@Packing, 1)), LastSyncUtc = GETUTCDATE() 
                                WHERE BranchId = @BranchId AND ItemId = @ItemId AND (IsBaseUnit = 'Y' OR IsBaseUnit IS NULL);
                            END
                            ELSE
                            BEGIN
                                INSERT INTO dbo.PriceSettings (BranchId, ItemId, UnitId, Unit, Packing, Stock, Cost, IsBaseUnit, LastSyncUtc)
                                VALUES (@BranchId, @ItemId, @UnitId, @Unit, ISNULL(@Packing, 1), ((@Qty + @Free) * ISNULL(@Packing, 1)), @Cost, 'Y', GETUTCDATE());
                            END";

                        foreach (var d in tx.PDetails)
                        {
                            decimal packingVal = d.Packing > 0 ? d.Packing : 1.0m;
                            await conn.ExecuteAsync(insertDetailsSql, new
                            {
                                CentralPurchaseID = centralId,
                                BranchId = branchId,
                                tx.TransactionGuid,
                                tx.PMaster.PurchaseNo,
                                d.SlNo,
                                d.ItemID,
                                d.Barcode,
                                d.ItemName,
                                d.UnitId,
                                d.Unit,
                                d.BaseUnit,
                                Packing = packingVal,
                                d.Qty,
                                d.Free,
                                d.Cost,
                                d.DisPer,
                                d.DisAmt,
                                d.SalesPrice,
                                d.TaxPer,
                                d.TaxAmt,
                                d.TotalSP,
                                d.OriginalCost,
                                d.OriginalSP,
                                d.TaxType,
                                d.SeriesID,
                                d.CessAmt,
                                d.CessPer
                            }, transaction: updateTrans);

                            await conn.ExecuteAsync(addStockSql, new
                            {
                                BranchId = branchId,
                                ItemId = d.ItemID,
                                UnitId = d.UnitId,
                                Unit = d.Unit,
                                d.Qty,
                                d.Free,
                                Packing = packingVal,
                                d.Cost
                            }, transaction: updateTrans);
                        }

                        // Replace Vouchers if provided
                        if (tx.Vouchers != null && tx.Vouchers.Count > 0)
                        {
                            await conn.ExecuteAsync(
                                "DELETE FROM dbo.Vouchers WHERE TransactionGuid = @TransactionGuid",
                                new { tx.TransactionGuid }, transaction: updateTrans);

                            const string insertVoucherSql = @"
                                INSERT INTO dbo.Vouchers (
                                    CentralTransactionID, BranchId, TransactionGuid, BranchVoucherID, 
                                    LedgerID, LedgerName, Debit, Credit, Narration, CancelFlag, SyncReceivedUtc
                                )
                                VALUES (
                                    @CentralPurchaseID, @BranchId, @TransactionGuid, @BranchVoucherID, 
                                    @LedgerID, @LedgerName, @Debit, @Credit, @Narration, 0, GETUTCDATE()
                                );";

                            foreach (var v in tx.Vouchers)
                            {
                                await conn.ExecuteAsync(insertVoucherSql, new
                                {
                                    CentralPurchaseID = centralId,
                                    BranchId = branchId,
                                    tx.TransactionGuid,
                                    v.BranchVoucherId,
                                    v.LedgerID,
                                    v.LedgerName,
                                    v.Debit,
                                    v.Credit,
                                    v.Narration
                                }, transaction: updateTrans);
                            }
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
            }

            // HANDLE NEW 'CREATE'
            using (var insertTrans = conn.BeginTransaction())
            {
                try
                {
                    const string insertMasterSql = @"
                        INSERT INTO dbo.PMaster (
                            BranchId, TransactionGuid, PurchaseNo, PurchaseDate, InvoiceNo, InvoiceDate, 
                            LedgerID, VendorName, PaymodeID, Paymode, CreditPeriod, SubTotal, 
                            SpDisPer, SpDsiAmt, BillDiscountPer, BillDiscountAmt, TaxPer, TaxAmt, 
                            Frieght, ExpenseAmt, OtherExpAmt, GrandTotal, CancelFlag, UserID, 
                            UserName, TaxType, Remarks, RoundOff, CessPer, CessAmt, CalAfterTax, 
                            CurrencyID, CurSymbol, SeriesID, NetTotal, SyncReceivedUtc
                        )
                        OUTPUT INSERTED.CentralPurchaseID
                        VALUES (
                            @BranchId, @TransactionGuid, @PurchaseNo, @PurchaseDate, @InvoiceNo, @InvoiceDate, 
                            @LedgerID, @VendorName, @PaymodeID, @Paymode, @CreditPeriod, @SubTotal, 
                            @SpDisPer, @SpDsiAmt, @BillDiscountPer, @BillDiscountAmt, @TaxPer, @TaxAmt, 
                            @Frieght, @ExpenseAmt, @OtherExpAmt, @GrandTotal, 0, @UserID, 
                            @UserName, @TaxType, @Remarks, @RoundOff, @CessPer, @CessAmt, @CalAfterTax, 
                            @CurrencyID, @CurSymbol, @SeriesID, @NetTotal, GETUTCDATE()
                        );";

                    long centralId = await conn.ExecuteScalarAsync<long>(insertMasterSql, new
                    {
                        BranchId = branchId,
                        tx.TransactionGuid,
                        tx.PMaster.PurchaseNo,
                        PurchaseDate = SafeSqlDate(tx.PMaster.PurchaseDate),
                        tx.PMaster.InvoiceNo,
                        InvoiceDate = SafeSqlDate(tx.PMaster.InvoiceDate),
                        tx.PMaster.LedgerID,
                        tx.PMaster.VendorName,
                        tx.PMaster.PaymodeID,
                        tx.PMaster.Paymode,
                        tx.PMaster.CreditPeriod,
                        tx.PMaster.SubTotal,
                        tx.PMaster.SpDisPer,
                        tx.PMaster.SpDsiAmt,
                        tx.PMaster.BillDiscountPer,
                        tx.PMaster.BillDiscountAmt,
                        tx.PMaster.TaxPer,
                        tx.PMaster.TaxAmt,
                        tx.PMaster.Frieght,
                        tx.PMaster.ExpenseAmt,
                        tx.PMaster.OtherExpAmt,
                        tx.PMaster.GrandTotal,
                        tx.PMaster.UserID,
                        tx.PMaster.UserName,
                        tx.PMaster.TaxType,
                        tx.PMaster.Remarks,
                        tx.PMaster.RoundOff,
                        tx.PMaster.CessPer,
                        tx.PMaster.CessAmt,
                        tx.PMaster.CalAfterTax,
                        tx.PMaster.CurrencyID,
                        tx.PMaster.CurSymbol,
                        tx.PMaster.SeriesID,
                        tx.PMaster.NetTotal
                    }, transaction: insertTrans);

                    // Insert PDetails line items
                    const string insertDetailsSql = @"
                        INSERT INTO dbo.PDetails (
                            CentralPurchaseID, BranchId, TransactionGuid, PurchaseNo, SlNo, 
                            ItemID, Barcode, ItemName, UnitId, Unit, BaseUnit, Packing, 
                            Qty, Free, Cost, DisPer, DisAmt, SalesPrice, TaxPer, TaxAmt, 
                            TotalSP, OriginalCost, OriginalSP, TaxType, SeriesID, CessAmt, CessPer, SyncReceivedUtc
                        )
                        VALUES (
                            @CentralPurchaseID, @BranchId, @TransactionGuid, @PurchaseNo, @SlNo, 
                            @ItemID, @Barcode, @ItemName, @UnitId, @Unit, @BaseUnit, @Packing, 
                            @Qty, @Free, @Cost, @DisPer, @DisAmt, @SalesPrice, @TaxPer, @TaxAmt, 
                            @TotalSP, @OriginalCost, @OriginalSP, @TaxType, @SeriesID, @CessAmt, @CessPer, GETUTCDATE()
                        );";

                    const string addStockCreateSql = @"
                        IF EXISTS (SELECT 1 FROM dbo.PriceSettings WHERE BranchId = @BranchId AND ItemId = @ItemId)
                        BEGIN
                            UPDATE dbo.PriceSettings 
                            SET Stock = Stock + ((@Qty + @Free) * ISNULL(@Packing, 1)), LastSyncUtc = GETUTCDATE() 
                            WHERE BranchId = @BranchId AND ItemId = @ItemId AND (IsBaseUnit = 'Y' OR IsBaseUnit IS NULL);
                        END
                        ELSE
                        BEGIN
                            INSERT INTO dbo.PriceSettings (BranchId, ItemId, UnitId, Unit, Packing, Stock, Cost, IsBaseUnit, LastSyncUtc)
                            VALUES (@BranchId, @ItemId, @UnitId, @Unit, ISNULL(@Packing, 1), ((@Qty + @Free) * ISNULL(@Packing, 1)), @Cost, 'Y', GETUTCDATE());
                        END";

                    foreach (var d in tx.PDetails)
                    {
                        decimal packingVal = d.Packing > 0 ? d.Packing : 1.0m;
                        await conn.ExecuteAsync(insertDetailsSql, new
                        {
                            CentralPurchaseID = centralId,
                            BranchId = branchId,
                            tx.TransactionGuid,
                            tx.PMaster.PurchaseNo,
                            d.SlNo,
                            d.ItemID,
                            d.Barcode,
                            d.ItemName,
                            d.UnitId,
                            d.Unit,
                            d.BaseUnit,
                            Packing = packingVal,
                            d.Qty,
                            d.Free,
                            d.Cost,
                            d.DisPer,
                            d.DisAmt,
                            d.SalesPrice,
                            d.TaxPer,
                            d.TaxAmt,
                            d.TotalSP,
                            d.OriginalCost,
                            d.OriginalSP,
                            d.TaxType,
                            d.SeriesID,
                            d.CessAmt,
                            d.CessPer
                        }, transaction: insertTrans);

                        await conn.ExecuteAsync(addStockCreateSql, new
                        {
                            BranchId = branchId,
                            ItemId = d.ItemID,
                            UnitId = d.UnitId,
                            Unit = d.Unit,
                            d.Qty,
                            d.Free,
                            Packing = packingVal,
                            d.Cost
                        }, transaction: insertTrans);
                    }

                    // Insert Vouchers accounting entries
                    const string insertVoucherSql = @"
                        INSERT INTO dbo.Vouchers (
                            CentralTransactionID, BranchId, TransactionGuid, BranchVoucherID, 
                            LedgerID, LedgerName, Debit, Credit, Narration, CancelFlag, SyncReceivedUtc
                        )
                        VALUES (
                            @CentralPurchaseID, @BranchId, @TransactionGuid, @BranchVoucherID, 
                            @LedgerID, @LedgerName, @Debit, @Credit, @Narration, 0, GETUTCDATE()
                        );";

                    foreach (var v in tx.Vouchers)
                    {
                        await conn.ExecuteAsync(insertVoucherSql, new
                        {
                            CentralPurchaseID = centralId,
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
        private static DateTime SafeSqlDate(DateTime dt)
        {
            if (dt < new DateTime(1753, 1, 1)) return DateTime.UtcNow;
            if (dt > new DateTime(9999, 12, 31)) return DateTime.UtcNow;
            return dt;
        }

        private static DateTime? SafeSqlDate(DateTime? dt)
        {
            if (!dt.HasValue) return null;
            if (dt.Value < new DateTime(1753, 1, 1)) return null;
            if (dt.Value > new DateTime(9999, 12, 31)) return null;
            return dt.Value;
        }
        #endregion
    }
}
