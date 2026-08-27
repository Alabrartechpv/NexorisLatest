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
                    else if (tx.EntityType.Equals("SHIFT_CLOSING", StringComparison.OrdinalIgnoreCase) && tx.ShiftClosing != null)
                        entityId = tx.ShiftClosing.BranchShiftClosingId.ToString();
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
                else if (tx.EntityType.Equals("CUSTOMER_RECEIPT", StringComparison.OrdinalIgnoreCase))
                {
                    return await IngestCustomerReceiptTransactionAsync(conn, branchId, tx);
                }
                else if (tx.EntityType.Equals("VENDOR_PAYMENT", StringComparison.OrdinalIgnoreCase))
                {
                    return await IngestVendorPaymentTransactionAsync(conn, branchId, tx);
                }
                else if (tx.EntityType.Equals("SALES_RETURN", StringComparison.OrdinalIgnoreCase))
                {
                    return await IngestSalesReturnTransactionAsync(conn, branchId, tx);
                }
                else if (tx.EntityType.Equals("CREDIT_NOTE", StringComparison.OrdinalIgnoreCase))
                {
                    return await IngestCreditNoteTransactionAsync(conn, branchId, tx);
                }
                else if (tx.EntityType.Equals("PURCHASE_RETURN", StringComparison.OrdinalIgnoreCase))
                {
                    return await IngestPurchaseReturnTransactionAsync(conn, branchId, tx);
                }
                else if (tx.EntityType.Equals("DEBIT_NOTE", StringComparison.OrdinalIgnoreCase))
                {
                    return await IngestDebitNoteTransactionAsync(conn, branchId, tx);
                }
                else if (tx.EntityType.Equals("STOCK_ADJUSTMENT", StringComparison.OrdinalIgnoreCase))
                {
                    return await IngestStockAdjustmentTransactionAsync(conn, branchId, tx);
                }
                else if (tx.EntityType.Equals("SHIFT_CLOSING", StringComparison.OrdinalIgnoreCase))
                {
                    return await IngestShiftClosingTransactionAsync(conn, branchId, tx);
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

            // 1. Deduplication / Idempotency check via Stored Procedure
            var existing = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "dbo.sp_Central_CheckTransactionGuid",
                new { BranchId = branchId, EntityType = "SALES", tx.TransactionGuid },
                commandType: CommandType.StoredProcedure
            );

            if (existing != null && tx.Operation.Equals("CREATE", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(string.Format("[INFO] Sales Transaction {0} already exists in Central DB. Returning AlreadySynced.", tx.TransactionGuid));
                return new SyncItemResult
                {
                    TransactionGuid = tx.TransactionGuid,
                    EntityType = tx.EntityType,
                    EntityId = entityId,
                    Status = "AlreadySynced",
                    CentralTransactionId = (long)existing.CentralId
                };
            }

            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    if (tx.Operation.Equals("CANCEL", StringComparison.OrdinalIgnoreCase))
                    {
                        await conn.ExecuteAsync(
                            "dbo.sp_Central_CancelSalesTransaction",
                            new { BranchId = branchId, tx.TransactionGuid },
                            transaction: trans,
                            commandType: CommandType.StoredProcedure
                        );

                        trans.Commit();

                        return new SyncItemResult
                        {
                            TransactionGuid = tx.TransactionGuid,
                            EntityType = tx.EntityType,
                            EntityId = entityId,
                            Status = "Cancelled",
                            CentralTransactionId = existing != null ? (long)existing.CentralId : (long?)null
                        };
                    }

                    var sm = tx.SMaster;
                    var masterParams = new DynamicParameters();
                    masterParams.Add("@BranchId", branchId);
                    masterParams.Add("@TransactionGuid", tx.TransactionGuid);
                    masterParams.Add("@BillNo", sm.BillNo);
                    masterParams.Add("@BillDate", SafeSqlDate(sm.BillDate));
                    masterParams.Add("@CompanyId", sm.CompanyId > 0 ? sm.CompanyId : 1);
                    masterParams.Add("@FinYearId", sm.FinYearId > 0 ? sm.FinYearId : 1);
                    masterParams.Add("@CounterId", sm.CounterId > 0 ? sm.CounterId : 1);
                    masterParams.Add("@CustomerName", sm.CustomerName);
                    masterParams.Add("@LedgerID", sm.LedgerID);
                    masterParams.Add("@PaymodeId", sm.PaymodeId);
                    masterParams.Add("@PaymodeName", sm.PaymodeName);
                    masterParams.Add("@SubTotal", sm.SubTotal);
                    masterParams.Add("@DiscountAmt", sm.DiscountAmt);
                    masterParams.Add("@TaxAmt", sm.TaxAmt);
                    masterParams.Add("@NetAmount", sm.NetAmount);
                    masterParams.Add("@UserId", sm.UserId > 0 ? sm.UserId : 1);
                    masterParams.Add("@Status", sm.Status ?? "Completed");
                    masterParams.Add("@CancelFlag", 0);

                    long centralId = await conn.ExecuteScalarAsync<long>(
                        "dbo.sp_Central_UpsertSalesMaster",
                        masterParams,
                        transaction: trans,
                        commandType: CommandType.StoredProcedure
                    );

                    if (tx.SDetails != null)
                    {
                        foreach (var d in tx.SDetails)
                        {
                            var detailParams = new DynamicParameters();
                            detailParams.Add("@CentralTransactionID", centralId);
                            detailParams.Add("@BranchId", branchId);
                            detailParams.Add("@TransactionGuid", tx.TransactionGuid);
                            detailParams.Add("@BillNo", sm.BillNo);
                            detailParams.Add("@SlNO", d.SlNO);
                            detailParams.Add("@ItemId", d.ItemId);
                            detailParams.Add("@Barcode", d.Barcode);
                            detailParams.Add("@ItemName", d.ItemName);
                            detailParams.Add("@Qty", d.Qty);
                            detailParams.Add("@Packing", d.Packing > 0 ? d.Packing : 1.0m);
                            detailParams.Add("@UnitPrice", d.UnitPrice);
                            detailParams.Add("@Amount", d.Amount);
                            detailParams.Add("@DiscountAmount", d.DiscountAmount);
                            detailParams.Add("@TaxAmt", d.TaxAmt);
                            detailParams.Add("@TotalAmount", d.TotalAmount);
                            detailParams.Add("@UnitId", d.UnitId > 0 ? d.UnitId : 1);

                            await conn.ExecuteAsync(
                                "dbo.sp_Central_UpsertSalesDetail",
                                detailParams,
                                transaction: trans,
                                commandType: CommandType.StoredProcedure
                            );
                        }
                    }

                    if (tx.Vouchers != null)
                    {
                        foreach (var v in tx.Vouchers)
                        {
                            var vParams = new DynamicParameters();
                            vParams.Add("@CentralTransactionID", centralId);
                            vParams.Add("@BranchId", branchId);
                            vParams.Add("@TransactionGuid", tx.TransactionGuid);
                            vParams.Add("@BranchVoucherID", v.BranchVoucherId);
                            vParams.Add("@LedgerID", v.LedgerID ?? 0);
                            vParams.Add("@LedgerName", v.LedgerName);
                            vParams.Add("@Debit", v.Debit);
                            vParams.Add("@Credit", v.Credit);
                            vParams.Add("@Narration", v.Narration);
                            vParams.Add("@VoucherType", "SALES");
                            vParams.Add("@VoucherDate", SafeSqlDate(sm.BillDate));

                            await conn.ExecuteAsync(
                                "dbo.sp_Central_UpsertVoucher",
                                vParams,
                                transaction: trans,
                                commandType: CommandType.StoredProcedure
                            );
                        }
                    }

                    trans.Commit();

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
                    trans.Rollback();
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

            // 1. Deduplication / Idempotency check via Stored Procedure
            var existing = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "dbo.sp_Central_CheckTransactionGuid",
                new { BranchId = branchId, EntityType = "PURCHASE", tx.TransactionGuid },
                commandType: CommandType.StoredProcedure
            );

            if (existing != null && tx.Operation.Equals("CREATE", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(string.Format("[INFO] Purchase Transaction {0} already exists in Central DB. Returning AlreadySynced.", tx.TransactionGuid));
                return new SyncItemResult
                {
                    TransactionGuid = tx.TransactionGuid,
                    EntityType = tx.EntityType,
                    EntityId = entityId,
                    Status = "AlreadySynced",
                    CentralTransactionId = (long)existing.CentralId
                };
            }

            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    if (tx.Operation.Equals("CANCEL", StringComparison.OrdinalIgnoreCase))
                    {
                        await conn.ExecuteAsync(
                            "dbo.sp_Central_CancelPurchaseTransaction",
                            new { BranchId = branchId, tx.TransactionGuid },
                            transaction: trans,
                            commandType: CommandType.StoredProcedure
                        );

                        trans.Commit();

                        return new SyncItemResult
                        {
                            TransactionGuid = tx.TransactionGuid,
                            EntityType = tx.EntityType,
                            EntityId = entityId,
                            Status = "Cancelled",
                            CentralTransactionId = existing != null ? (long)existing.CentralId : (long?)null
                        };
                    }

                    var pm = tx.PMaster;
                    var masterParams = new DynamicParameters();
                    masterParams.Add("@BranchId", branchId);
                    masterParams.Add("@TransactionGuid", tx.TransactionGuid);
                    masterParams.Add("@PurchaseNo", pm.PurchaseNo);
                    masterParams.Add("@PurchaseDate", SafeSqlDate(pm.PurchaseDate));
                    masterParams.Add("@InvoiceNo", pm.InvoiceNo);
                    masterParams.Add("@InvoiceDate", SafeSqlDate(pm.InvoiceDate));
                    masterParams.Add("@LedgerID", pm.LedgerID);
                    masterParams.Add("@VendorName", pm.VendorName);
                    masterParams.Add("@PaymodeID", pm.PaymodeID);
                    masterParams.Add("@Paymode", pm.Paymode);
                    masterParams.Add("@CreditPeriod", pm.CreditPeriod);
                    masterParams.Add("@SubTotal", pm.SubTotal);
                    masterParams.Add("@SpDisPer", pm.SpDisPer);
                    masterParams.Add("@SpDsiAmt", pm.SpDsiAmt);
                    masterParams.Add("@BillDiscountPer", pm.BillDiscountPer);
                    masterParams.Add("@BillDiscountAmt", pm.BillDiscountAmt);
                    masterParams.Add("@TaxPer", pm.TaxPer);
                    masterParams.Add("@TaxAmt", pm.TaxAmt);
                    masterParams.Add("@Frieght", pm.Frieght);
                    masterParams.Add("@ExpenseAmt", pm.ExpenseAmt);
                    masterParams.Add("@OtherExpAmt", pm.OtherExpAmt);
                    masterParams.Add("@GrandTotal", pm.GrandTotal);
                    masterParams.Add("@UserID", pm.UserID > 0 ? pm.UserID : 1);
                    masterParams.Add("@UserName", pm.UserName);
                    masterParams.Add("@TaxType", pm.TaxType);
                    masterParams.Add("@Remarks", pm.Remarks);
                    masterParams.Add("@RoundOff", pm.RoundOff);
                    masterParams.Add("@CessPer", pm.CessPer);
                    masterParams.Add("@CessAmt", pm.CessAmt);
                    masterParams.Add("@CalAfterTax", pm.CalAfterTax);
                    masterParams.Add("@CurrencyID", pm.CurrencyID > 0 ? pm.CurrencyID : 1);
                    masterParams.Add("@CurSymbol", pm.CurSymbol);
                    masterParams.Add("@SeriesID", pm.SeriesID > 0 ? pm.SeriesID : 1);
                    masterParams.Add("@NetTotal", pm.NetTotal);
                    masterParams.Add("@CancelFlag", 0);

                    long centralId = await conn.ExecuteScalarAsync<long>(
                        "dbo.sp_Central_UpsertPurchaseMaster",
                        masterParams,
                        transaction: trans,
                        commandType: CommandType.StoredProcedure
                    );

                    if (tx.PDetails != null)
                    {
                        foreach (var d in tx.PDetails)
                        {
                            var detailParams = new DynamicParameters();
                            detailParams.Add("@CentralPurchaseID", centralId);
                            detailParams.Add("@BranchId", branchId);
                            detailParams.Add("@TransactionGuid", tx.TransactionGuid);
                            detailParams.Add("@PurchaseNo", pm.PurchaseNo);
                            detailParams.Add("@SlNo", d.SlNo);
                            detailParams.Add("@ItemID", d.ItemID);
                            detailParams.Add("@Barcode", d.Barcode);
                            detailParams.Add("@ItemName", d.ItemName);
                            detailParams.Add("@UnitId", d.UnitId > 0 ? d.UnitId : 1);
                            detailParams.Add("@Unit", d.Unit ?? "UNIT");
                            detailParams.Add("@BaseUnit", d.BaseUnit ?? "Y");
                            detailParams.Add("@Packing", d.Packing > 0 ? d.Packing : 1.0m);
                            detailParams.Add("@Qty", d.Qty);
                            detailParams.Add("@Free", d.Free);
                            detailParams.Add("@Cost", d.Cost);
                            detailParams.Add("@DisPer", d.DisPer);
                            detailParams.Add("@DisAmt", d.DisAmt);
                            detailParams.Add("@SalesPrice", d.SalesPrice);
                            detailParams.Add("@TaxPer", d.TaxPer);
                            detailParams.Add("@TaxAmt", d.TaxAmt);
                            detailParams.Add("@TotalSP", d.TotalSP);
                            detailParams.Add("@OriginalCost", d.OriginalCost);
                            detailParams.Add("@OriginalSP", d.OriginalSP);
                            detailParams.Add("@TaxType", d.TaxType);
                            detailParams.Add("@SeriesID", d.SeriesID > 0 ? d.SeriesID : 1);
                            detailParams.Add("@CessAmt", d.CessAmt);
                            detailParams.Add("@CessPer", d.CessPer);

                            await conn.ExecuteAsync(
                                "dbo.sp_Central_UpsertPurchaseDetail",
                                detailParams,
                                transaction: trans,
                                commandType: CommandType.StoredProcedure
                            );
                        }
                    }

                    if (tx.Vouchers != null)
                    {
                        foreach (var v in tx.Vouchers)
                        {
                            var vParams = new DynamicParameters();
                            vParams.Add("@CentralTransactionID", centralId);
                            vParams.Add("@BranchId", branchId);
                            vParams.Add("@TransactionGuid", tx.TransactionGuid);
                            vParams.Add("@BranchVoucherID", v.BranchVoucherId);
                            vParams.Add("@LedgerID", v.LedgerID ?? 0);
                            vParams.Add("@LedgerName", v.LedgerName);
                            vParams.Add("@Debit", v.Debit);
                            vParams.Add("@Credit", v.Credit);
                            vParams.Add("@Narration", v.Narration);
                            vParams.Add("@VoucherType", "PURCHASE");
                            vParams.Add("@VoucherDate", SafeSqlDate(pm.PurchaseDate));

                            await conn.ExecuteAsync(
                                "dbo.sp_Central_UpsertVoucher",
                                vParams,
                                transaction: trans,
                                commandType: CommandType.StoredProcedure
                            );
                        }
                    }

                    trans.Commit();

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
                    trans.Rollback();
                    throw;
                }
            }
        }
        #endregion

        #region Customer Receipt Ingest
        private async Task<SyncItemResult> IngestCustomerReceiptTransactionAsync(SqlConnection conn, int branchId, TransactionSyncDto tx)
        {
            string entityId = tx.Receipt != null ? tx.Receipt.BranchReceiptId.ToString() : "Unknown";

            if (tx.Operation.Equals("CANCEL", StringComparison.OrdinalIgnoreCase))
            {
                await conn.ExecuteAsync(
                    "dbo.sp_Central_CancelTransaction",
                    new { BranchId = branchId, EntityType = "CUSTOMER_RECEIPT", tx.TransactionGuid },
                    commandType: CommandType.StoredProcedure
                );

                return new SyncItemResult
                {
                    TransactionGuid = tx.TransactionGuid,
                    EntityType = tx.EntityType,
                    EntityId = entityId,
                    Status = "Synced"
                };
            }

            if (tx.Receipt == null)
            {
                throw new InvalidOperationException($"Payload for Customer Receipt {tx.TransactionGuid} is empty.");
            }

            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    var p = new DynamicParameters();
                    p.Add("@BranchId", branchId);
                    p.Add("@TransactionGuid", tx.TransactionGuid);
                    p.Add("@BranchReceiptId", tx.Receipt.BranchReceiptId);
                    p.Add("@CompanyId", tx.Receipt.CompanyId > 0 ? tx.Receipt.CompanyId : 1);
                    p.Add("@VoucherId", tx.Receipt.VoucherId);
                    p.Add("@VoucherDate", SafeSqlDate(tx.Receipt.VoucherDate));
                    p.Add("@PaymentMethodLedgerId", tx.Receipt.PaymentMethodLedgerId);
                    p.Add("@PaymentMethodName", tx.Receipt.PaymentMethodName ?? "");
                    p.Add("@CustomerLedgerId", tx.Receipt.CustomerLedgerId);
                    p.Add("@CustomerName", tx.Receipt.CustomerName ?? "");
                    p.Add("@ReceivableAmount", tx.Receipt.ReceivableAmount);
                    p.Add("@ReceiptAmount", tx.Receipt.ReceiptAmount);
                    p.Add("@OldReceiptAmount", tx.Receipt.OldReceiptAmount);
                    p.Add("@Narration", tx.Receipt.Narration ?? "");
                    p.Add("@BillNoUntil", tx.Receipt.BillNoUntil);
                    p.Add("@CancelFlag", tx.Receipt.CancelFlag);
                    p.Add("@UserId", tx.Receipt.UserId > 0 ? tx.Receipt.UserId : 1);
                    p.Add("@TransporterLedgerId", tx.Receipt.TransporterLedgerId);

                    long centralReceiptId = await conn.ExecuteScalarAsync<long>(
                        "dbo.sp_Central_UpsertCustomerReceipt",
                        p,
                        transaction: trans,
                        commandType: CommandType.StoredProcedure
                    );

                    // Insert Details via Stored Procedure
                    foreach (var d in tx.ReceiptDetails)
                    {
                        await conn.ExecuteAsync(
                            "dbo.sp_Central_UpsertCustomerReceiptDetail",
                            new
                            {
                                CentralReceiptId = centralReceiptId,
                                BranchId = branchId,
                                tx.TransactionGuid,
                                BranchReceiptId = tx.Receipt.BranchReceiptId,
                                d.BillNo,
                                BillDate = SafeSqlDate(d.BillDate),
                                d.BillAmount,
                                d.ReceivedAmount,
                                d.ReceiptAmount,
                                d.BalanceAmount,
                                d.CancelFlag
                            },
                            transaction: trans,
                            commandType: CommandType.StoredProcedure
                        );
                    }

                    // Insert Vouchers via Stored Procedure
                    foreach (var v in tx.Vouchers)
                    {
                        await conn.ExecuteAsync(
                            "dbo.sp_Central_UpsertVoucher",
                            new
                            {
                                CentralTransactionID = centralReceiptId,
                                BranchId = branchId,
                                tx.TransactionGuid,
                                BranchVoucherID = v.BranchVoucherId,
                                v.LedgerID,
                                v.LedgerName,
                                v.Debit,
                                v.Credit,
                                v.Narration,
                                VoucherType = "CUSTRCPT"
                            },
                            transaction: trans,
                            commandType: CommandType.StoredProcedure
                        );
                    }

                    trans.Commit();

                    return new SyncItemResult
                    {
                        TransactionGuid = tx.TransactionGuid,
                        EntityType = tx.EntityType,
                        EntityId = entityId,
                        Status = "Synced",
                        CentralTransactionId = centralReceiptId
                    };
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }
        #endregion

        #region Vendor Payment Ingest
        private async Task<SyncItemResult> IngestVendorPaymentTransactionAsync(SqlConnection conn, int branchId, TransactionSyncDto tx)
        {
            string entityId = tx.Payment != null ? tx.Payment.BranchPaymentId.ToString() : "Unknown";

            if (tx.Operation.Equals("CANCEL", StringComparison.OrdinalIgnoreCase))
            {
                await conn.ExecuteAsync(
                    "dbo.sp_Central_CancelTransaction",
                    new { BranchId = branchId, EntityType = "VENDOR_PAYMENT", tx.TransactionGuid },
                    commandType: CommandType.StoredProcedure
                );

                return new SyncItemResult
                {
                    TransactionGuid = tx.TransactionGuid,
                    EntityType = tx.EntityType,
                    EntityId = entityId,
                    Status = "Synced"
                };
            }

            if (tx.Payment == null)
            {
                throw new InvalidOperationException($"Payload for Vendor Payment {tx.TransactionGuid} is empty.");
            }

            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    var p = new DynamicParameters();
                    p.Add("@BranchId", branchId);
                    p.Add("@TransactionGuid", tx.TransactionGuid);
                    p.Add("@BranchPaymentId", tx.Payment.BranchPaymentId);
                    p.Add("@CompanyId", tx.Payment.CompanyId > 0 ? tx.Payment.CompanyId : 1);
                    p.Add("@VoucherId", tx.Payment.VoucherId);
                    p.Add("@VoucherDate", SafeSqlDate(tx.Payment.VoucherDate));
                    p.Add("@PaymentMethodLedgerId", tx.Payment.PaymentMethodLedgerId);
                    p.Add("@PaymentMethodName", tx.Payment.PaymentMethodName ?? "");
                    p.Add("@VendorLedgerId", tx.Payment.VendorLedgerId);
                    p.Add("@VendorName", tx.Payment.VendorName ?? "");
                    p.Add("@PayableAmount", tx.Payment.PayableAmount);
                    p.Add("@PaymentAmount", tx.Payment.PaymentAmount);
                    p.Add("@OldPaymentAmount", tx.Payment.OldPaymentAmount);
                    p.Add("@Narration", tx.Payment.Narration ?? "");
                    p.Add("@BillNoUntil", tx.Payment.BillNoUntil);
                    p.Add("@CancelFlag", tx.Payment.CancelFlag);
                    p.Add("@UserId", tx.Payment.UserId > 0 ? tx.Payment.UserId : 1);

                    long centralPaymentId = await conn.ExecuteScalarAsync<long>(
                        "dbo.sp_Central_UpsertVendorPayment",
                        p,
                        transaction: trans,
                        commandType: CommandType.StoredProcedure
                    );

                    // Insert Details via Stored Procedure
                    foreach (var d in tx.PaymentDetails)
                    {
                        await conn.ExecuteAsync(
                            "dbo.sp_Central_UpsertVendorPaymentDetail",
                            new
                            {
                                CentralPaymentId = centralPaymentId,
                                BranchId = branchId,
                                tx.TransactionGuid,
                                BranchPaymentId = tx.Payment.BranchPaymentId,
                                d.BillNo,
                                BillDate = SafeSqlDate(d.BillDate),
                                d.BillAmount,
                                d.PayedAmount,
                                d.PaymentAmount,
                                d.BalanceAmount,
                                d.CancelFlag
                            },
                            transaction: trans,
                            commandType: CommandType.StoredProcedure
                        );
                    }

                    // Insert Vouchers via Stored Procedure
                    foreach (var v in tx.Vouchers)
                    {
                        await conn.ExecuteAsync(
                            "dbo.sp_Central_UpsertVoucher",
                            new
                            {
                                CentralTransactionID = centralPaymentId,
                                BranchId = branchId,
                                tx.TransactionGuid,
                                BranchVoucherID = v.BranchVoucherId,
                                v.LedgerID,
                                v.LedgerName,
                                v.Debit,
                                v.Credit,
                                v.Narration,
                                VoucherType = "VENDPAY"
                            },
                            transaction: trans,
                            commandType: CommandType.StoredProcedure
                        );
                    }

                    trans.Commit();

                    return new SyncItemResult
                    {
                        TransactionGuid = tx.TransactionGuid,
                        EntityType = tx.EntityType,
                        EntityId = entityId,
                        Status = "Synced",
                        CentralTransactionId = centralPaymentId
                    };
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }
        #endregion

        #region Sales Return Transaction Ingest
        private async Task<SyncItemResult> IngestSalesReturnTransactionAsync(SqlConnection conn, int branchId, TransactionSyncDto tx)
        {
            string entityId = tx.SalesReturn != null ? tx.SalesReturn.BranchSReturnNo.ToString() : "Unknown";

            if (tx.SalesReturn == null && !tx.Operation.Equals("CANCEL", StringComparison.OrdinalIgnoreCase))
            {
                return new SyncItemResult
                {
                    TransactionGuid = tx.TransactionGuid,
                    EntityType = tx.EntityType,
                    EntityId = entityId,
                    Status = "Failed",
                    ErrorMessage = "Payload missing SalesReturn header record."
                };
            }

            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    if (tx.Operation.Equals("CANCEL", StringComparison.OrdinalIgnoreCase))
                    {
                        await conn.ExecuteAsync(
                            "dbo.sp_Central_CancelTransaction",
                            new
                            {
                                BranchId = branchId,
                                EntityType = "SALES_RETURN",
                                TransactionGuid = tx.TransactionGuid
                            },
                            transaction: trans,
                            commandType: CommandType.StoredProcedure
                        );

                        trans.Commit();
                        return new SyncItemResult
                        {
                            TransactionGuid = tx.TransactionGuid,
                            EntityType = tx.EntityType,
                            EntityId = entityId,
                            Status = "Cancelled"
                        };
                    }

                    var sr = tx.SalesReturn;

                    var upsertParams = new DynamicParameters();
                    upsertParams.Add("@BranchId", branchId);
                    upsertParams.Add("@TransactionGuid", tx.TransactionGuid);
                    upsertParams.Add("@BranchSReturnNo", sr.BranchSReturnNo);
                    upsertParams.Add("@SReturnDate", SafeSqlDate(sr.SReturnDate));
                    upsertParams.Add("@InvoiceNo", sr.InvoiceNo);
                    upsertParams.Add("@InvoiceDate", SafeSqlDate(sr.InvoiceDate));
                    upsertParams.Add("@CompanyId", sr.CompanyId > 0 ? sr.CompanyId : 1);
                    upsertParams.Add("@FinYearId", sr.FinYearId > 0 ? sr.FinYearId : 1);
                    upsertParams.Add("@LedgerID", sr.LedgerID);
                    upsertParams.Add("@CustomerName", sr.CustomerName);
                    upsertParams.Add("@Paymode", sr.Paymode);
                    upsertParams.Add("@SubTotal", sr.SubTotal);
                    upsertParams.Add("@TaxAmt", sr.TaxAmt);
                    upsertParams.Add("@GrandTotal", sr.GrandTotal);
                    upsertParams.Add("@VoucherID", sr.VoucherID);
                    upsertParams.Add("@Remarks", sr.Remarks);
                    upsertParams.Add("@CancelFlag", sr.CancelFlag);
                    upsertParams.Add("@UserId", sr.UserId > 0 ? sr.UserId : 1);

                    long centralSReturnId = await conn.ExecuteScalarAsync<long>(
                        "dbo.sp_Central_UpsertSalesReturn",
                        upsertParams,
                        transaction: trans,
                        commandType: CommandType.StoredProcedure
                    );

                    if (tx.SalesReturnDetails != null)
                    {
                        foreach (var d in tx.SalesReturnDetails)
                        {
                            var detailParams = new DynamicParameters();
                            detailParams.Add("@CentralSReturnId", centralSReturnId);
                            detailParams.Add("@BranchId", branchId);
                            detailParams.Add("@TransactionGuid", tx.TransactionGuid);
                            detailParams.Add("@BranchSReturnNo", sr.BranchSReturnNo);
                            detailParams.Add("@SlNo", d.SlNo);
                            detailParams.Add("@ItemID", d.ItemID);
                            detailParams.Add("@ItemName", d.ItemName);
                            detailParams.Add("@Qty", d.Qty);
                            detailParams.Add("@Packing", d.Packing);
                            detailParams.Add("@SalesPrice", d.SalesPrice);
                            detailParams.Add("@TaxAmt", d.TaxAmt);
                            detailParams.Add("@TotalSP", d.TotalSP);
                            detailParams.Add("@UnitId", d.UnitId);
                            detailParams.Add("@Unit", d.Unit);
                            detailParams.Add("@CancelFlag", d.CancelFlag);

                            await conn.ExecuteAsync(
                                "dbo.sp_Central_UpsertSalesReturnDetail",
                                detailParams,
                                transaction: trans,
                                commandType: CommandType.StoredProcedure
                            );
                        }
                    }

                    if (tx.Vouchers != null)
                    {
                        foreach (var v in tx.Vouchers)
                        {
                            var vParams = new DynamicParameters();
                            vParams.Add("@BranchId", branchId);
                            vParams.Add("@TransactionGuid", tx.TransactionGuid);
                            vParams.Add("@BranchVoucherId", v.BranchVoucherId);
                            vParams.Add("@CentralTransactionID", centralSReturnId);
                            vParams.Add("@VoucherType", "SalesReturn");
                            vParams.Add("@LedgerID", v.LedgerID ?? 0);
                            vParams.Add("@LedgerName", v.LedgerName);
                            vParams.Add("@Debit", v.Debit);
                            vParams.Add("@Credit", v.Credit);
                            vParams.Add("@Narration", v.Narration);
                            vParams.Add("@VoucherDate", SafeSqlDate(sr.SReturnDate));

                            await conn.ExecuteAsync(
                                "dbo.sp_Central_UpsertVoucher",
                                vParams,
                                transaction: trans,
                                commandType: CommandType.StoredProcedure
                            );
                        }
                    }

                    trans.Commit();

                    return new SyncItemResult
                    {
                        TransactionGuid = tx.TransactionGuid,
                        EntityType = tx.EntityType,
                        EntityId = entityId,
                        Status = "Synced",
                        CentralTransactionId = centralSReturnId
                    };
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }
        #endregion

        #region Credit Note Transaction Ingest
        private async Task<SyncItemResult> IngestCreditNoteTransactionAsync(SqlConnection conn, int branchId, TransactionSyncDto tx)
        {
            string entityId = tx.CreditNote != null ? tx.CreditNote.BranchCreditNoteId.ToString() : "Unknown";

            if (tx.CreditNote == null && !tx.Operation.Equals("CANCEL", StringComparison.OrdinalIgnoreCase))
            {
                return new SyncItemResult
                {
                    TransactionGuid = tx.TransactionGuid,
                    EntityType = tx.EntityType,
                    EntityId = entityId,
                    Status = "Failed",
                    ErrorMessage = "Payload missing CreditNote header record."
                };
            }

            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    if (tx.Operation.Equals("CANCEL", StringComparison.OrdinalIgnoreCase))
                    {
                        await conn.ExecuteAsync(
                            "dbo.sp_Central_CancelTransaction",
                            new
                            {
                                BranchId = branchId,
                                EntityType = "CREDIT_NOTE",
                                TransactionGuid = tx.TransactionGuid
                            },
                            transaction: trans,
                            commandType: CommandType.StoredProcedure
                        );

                        trans.Commit();
                        return new SyncItemResult
                        {
                            TransactionGuid = tx.TransactionGuid,
                            EntityType = tx.EntityType,
                            EntityId = entityId,
                            Status = "Cancelled"
                        };
                    }

                    var cn = tx.CreditNote;

                    var upsertParams = new DynamicParameters();
                    upsertParams.Add("@BranchId", branchId);
                    upsertParams.Add("@TransactionGuid", tx.TransactionGuid);
                    upsertParams.Add("@BranchCreditNoteId", cn.BranchCreditNoteId);
                    upsertParams.Add("@CompanyId", cn.CompanyId > 0 ? cn.CompanyId : 1);
                    upsertParams.Add("@FinYearId", cn.FinYearId > 0 ? cn.FinYearId : 1);
                    upsertParams.Add("@VoucherId", cn.VoucherId);
                    upsertParams.Add("@VoucherDate", SafeSqlDate(cn.VoucherDate));
                    upsertParams.Add("@CustomerLedgerId", cn.CustomerLedgerId);
                    upsertParams.Add("@CustomerName", cn.CustomerName);
                    upsertParams.Add("@SReturnNo", cn.SReturnNo);
                    upsertParams.Add("@InvoiceNo", cn.InvoiceNo);
                    upsertParams.Add("@CreditAmount", cn.CreditAmount);
                    upsertParams.Add("@Narration", cn.Narration);
                    upsertParams.Add("@CancelFlag", cn.CancelFlag);
                    upsertParams.Add("@UserId", cn.UserId > 0 ? cn.UserId : 1);

                    long centralCreditNoteId = await conn.ExecuteScalarAsync<long>(
                        "dbo.sp_Central_UpsertCreditNote",
                        upsertParams,
                        transaction: trans,
                        commandType: CommandType.StoredProcedure
                    );

                    if (tx.CreditNoteDetails != null)
                    {
                        foreach (var d in tx.CreditNoteDetails)
                        {
                            var detailParams = new DynamicParameters();
                            detailParams.Add("@CentralCreditNoteId", centralCreditNoteId);
                            detailParams.Add("@BranchId", branchId);
                            detailParams.Add("@TransactionGuid", tx.TransactionGuid);
                            detailParams.Add("@BranchCreditNoteId", cn.BranchCreditNoteId);
                            detailParams.Add("@BillNo", d.BillNo);
                            detailParams.Add("@BillDate", SafeSqlDate(d.BillDate));
                            detailParams.Add("@BillAmount", d.BillAmount);
                            detailParams.Add("@CreditAmount", d.CreditAmount);
                            detailParams.Add("@BalanceAmount", d.BalanceAmount);
                            detailParams.Add("@CancelFlag", d.CancelFlag);

                            await conn.ExecuteAsync(
                                "dbo.sp_Central_UpsertCreditNoteDetail",
                                detailParams,
                                transaction: trans,
                                commandType: CommandType.StoredProcedure
                            );
                        }
                    }

                    if (tx.Vouchers != null)
                    {
                        foreach (var v in tx.Vouchers)
                        {
                            var vParams = new DynamicParameters();
                            vParams.Add("@BranchId", branchId);
                            vParams.Add("@TransactionGuid", tx.TransactionGuid);
                            vParams.Add("@BranchVoucherId", v.BranchVoucherId);
                            vParams.Add("@CentralTransactionID", centralCreditNoteId);
                            vParams.Add("@VoucherType", "CRNOTE");
                            vParams.Add("@LedgerID", v.LedgerID ?? 0);
                            vParams.Add("@LedgerName", v.LedgerName);
                            vParams.Add("@Debit", v.Debit);
                            vParams.Add("@Credit", v.Credit);
                            vParams.Add("@Narration", v.Narration);
                            vParams.Add("@VoucherDate", SafeSqlDate(cn.VoucherDate));

                            await conn.ExecuteAsync(
                                "dbo.sp_Central_UpsertVoucher",
                                vParams,
                                transaction: trans,
                                commandType: CommandType.StoredProcedure
                            );
                        }
                    }

                    trans.Commit();

                    return new SyncItemResult
                    {
                        TransactionGuid = tx.TransactionGuid,
                        EntityType = tx.EntityType,
                        EntityId = entityId,
                        Status = "Synced",
                        CentralTransactionId = centralCreditNoteId
                    };
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }
        #endregion

        #region Purchase Return Transaction Ingest
        private async Task<SyncItemResult> IngestPurchaseReturnTransactionAsync(SqlConnection conn, int branchId, TransactionSyncDto tx)
        {
            string entityId = tx.PurchaseReturn != null ? tx.PurchaseReturn.BranchPReturnNo.ToString() : "Unknown";

            if (tx.PurchaseReturn == null && !tx.Operation.Equals("CANCEL", StringComparison.OrdinalIgnoreCase))
            {
                return new SyncItemResult
                {
                    TransactionGuid = tx.TransactionGuid,
                    EntityType = tx.EntityType,
                    EntityId = entityId,
                    Status = "Failed",
                    ErrorMessage = "Payload missing PurchaseReturn header record."
                };
            }

            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    if (tx.Operation.Equals("CANCEL", StringComparison.OrdinalIgnoreCase))
                    {
                        await conn.ExecuteAsync(
                            "dbo.sp_Central_CancelTransaction",
                            new
                            {
                                BranchId = branchId,
                                EntityType = "PURCHASE_RETURN",
                                TransactionGuid = tx.TransactionGuid
                            },
                            transaction: trans,
                            commandType: CommandType.StoredProcedure
                        );

                        trans.Commit();
                        return new SyncItemResult
                        {
                            TransactionGuid = tx.TransactionGuid,
                            EntityType = tx.EntityType,
                            EntityId = entityId,
                            Status = "Cancelled"
                        };
                    }

                    var pr = tx.PurchaseReturn;

                    var upsertParams = new DynamicParameters();
                    upsertParams.Add("@BranchId", branchId);
                    upsertParams.Add("@TransactionGuid", tx.TransactionGuid);
                    upsertParams.Add("@BranchPReturnNo", pr.BranchPReturnNo);
                    upsertParams.Add("@PReturnDate", SafeSqlDate(pr.PReturnDate));
                    upsertParams.Add("@InvoiceNo", pr.InvoiceNo);
                    upsertParams.Add("@InvoiceDate", SafeSqlDate(pr.InvoiceDate));
                    upsertParams.Add("@CompanyId", pr.CompanyId > 0 ? pr.CompanyId : 1);
                    upsertParams.Add("@FinYearId", pr.FinYearId > 0 ? pr.FinYearId : 1);
                    upsertParams.Add("@LedgerID", pr.LedgerID);
                    upsertParams.Add("@VendorName", pr.VendorName);
                    upsertParams.Add("@Paymode", pr.Paymode);
                    upsertParams.Add("@SubTotal", pr.SubTotal);
                    upsertParams.Add("@TaxAmt", pr.TaxAmt);
                    upsertParams.Add("@GrandTotal", pr.GrandTotal);
                    upsertParams.Add("@VoucherID", pr.VoucherID);
                    upsertParams.Add("@Remarks", pr.Remarks);
                    upsertParams.Add("@CancelFlag", pr.CancelFlag);
                    upsertParams.Add("@UserId", pr.UserId > 0 ? pr.UserId : 1);

                    long centralPReturnId = await conn.ExecuteScalarAsync<long>(
                        "dbo.sp_Central_UpsertPurchaseReturn",
                        upsertParams,
                        transaction: trans,
                        commandType: CommandType.StoredProcedure
                    );

                    if (tx.PurchaseReturnDetails != null)
                    {
                        foreach (var d in tx.PurchaseReturnDetails)
                        {
                            var detailParams = new DynamicParameters();
                            detailParams.Add("@CentralPReturnId", centralPReturnId);
                            detailParams.Add("@BranchId", branchId);
                            detailParams.Add("@TransactionGuid", tx.TransactionGuid);
                            detailParams.Add("@BranchPReturnNo", pr.BranchPReturnNo);
                            detailParams.Add("@SlNo", d.SlNo);
                            detailParams.Add("@ItemID", d.ItemID);
                            detailParams.Add("@ItemName", d.ItemName);
                            detailParams.Add("@Qty", d.Qty);
                            detailParams.Add("@Packing", d.Packing);
                            detailParams.Add("@Cost", d.Cost);
                            detailParams.Add("@TaxAmt", d.TaxAmt);
                            detailParams.Add("@TotalSP", d.TotalSP);
                            detailParams.Add("@UnitId", d.UnitId);
                            detailParams.Add("@Unit", d.Unit);
                            detailParams.Add("@CancelFlag", d.CancelFlag);

                            await conn.ExecuteAsync(
                                "dbo.sp_Central_UpsertPurchaseReturnDetail",
                                detailParams,
                                transaction: trans,
                                commandType: CommandType.StoredProcedure
                            );
                        }
                    }

                    if (tx.Vouchers != null)
                    {
                        foreach (var v in tx.Vouchers)
                        {
                            var vParams = new DynamicParameters();
                            vParams.Add("@BranchId", branchId);
                            vParams.Add("@TransactionGuid", tx.TransactionGuid);
                            vParams.Add("@BranchVoucherId", v.BranchVoucherId);
                            vParams.Add("@CentralTransactionID", centralPReturnId);
                            vParams.Add("@VoucherType", "PurchaseReturn");
                            vParams.Add("@LedgerID", v.LedgerID ?? 0);
                            vParams.Add("@LedgerName", v.LedgerName);
                            vParams.Add("@Debit", v.Debit);
                            vParams.Add("@Credit", v.Credit);
                            vParams.Add("@Narration", v.Narration);
                            vParams.Add("@VoucherDate", SafeSqlDate(pr.PReturnDate));

                            await conn.ExecuteAsync(
                                "dbo.sp_Central_UpsertVoucher",
                                vParams,
                                transaction: trans,
                                commandType: CommandType.StoredProcedure
                            );
                        }
                    }

                    trans.Commit();

                    return new SyncItemResult
                    {
                        TransactionGuid = tx.TransactionGuid,
                        EntityType = tx.EntityType,
                        EntityId = entityId,
                        Status = "Synced",
                        CentralTransactionId = centralPReturnId
                    };
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }
        #endregion

        #region Debit Note Transaction Ingest
        private async Task<SyncItemResult> IngestDebitNoteTransactionAsync(SqlConnection conn, int branchId, TransactionSyncDto tx)
        {
            string entityId = tx.DebitNote != null ? tx.DebitNote.BranchDebitNoteId.ToString() : "Unknown";

            if (tx.DebitNote == null && !tx.Operation.Equals("CANCEL", StringComparison.OrdinalIgnoreCase))
            {
                return new SyncItemResult
                {
                    TransactionGuid = tx.TransactionGuid,
                    EntityType = tx.EntityType,
                    EntityId = entityId,
                    Status = "Failed",
                    ErrorMessage = "Payload missing DebitNote header record."
                };
            }

            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    if (tx.Operation.Equals("CANCEL", StringComparison.OrdinalIgnoreCase))
                    {
                        await conn.ExecuteAsync(
                            "dbo.sp_Central_CancelTransaction",
                            new
                            {
                                BranchId = branchId,
                                EntityType = "DEBIT_NOTE",
                                TransactionGuid = tx.TransactionGuid
                            },
                            transaction: trans,
                            commandType: CommandType.StoredProcedure
                        );

                        trans.Commit();
                        return new SyncItemResult
                        {
                            TransactionGuid = tx.TransactionGuid,
                            EntityType = tx.EntityType,
                            EntityId = entityId,
                            Status = "Cancelled"
                        };
                    }

                    var dn = tx.DebitNote;

                    var upsertParams = new DynamicParameters();
                    upsertParams.Add("@BranchId", branchId);
                    upsertParams.Add("@TransactionGuid", tx.TransactionGuid);
                    upsertParams.Add("@BranchDebitNoteId", dn.BranchDebitNoteId);
                    upsertParams.Add("@CompanyId", dn.CompanyId > 0 ? dn.CompanyId : 1);
                    upsertParams.Add("@FinYearId", dn.FinYearId > 0 ? dn.FinYearId : 1);
                    upsertParams.Add("@VoucherId", dn.VoucherId);
                    upsertParams.Add("@VoucherDate", SafeSqlDate(dn.VoucherDate));
                    upsertParams.Add("@VendorLedgerId", dn.VendorLedgerId);
                    upsertParams.Add("@VendorName", dn.VendorName);
                    upsertParams.Add("@PReturnNo", dn.PReturnNo);
                    upsertParams.Add("@InvoiceNo", dn.InvoiceNo);
                    upsertParams.Add("@DebitAmount", dn.DebitAmount);
                    upsertParams.Add("@Narration", dn.Narration);
                    upsertParams.Add("@CancelFlag", dn.CancelFlag);
                    upsertParams.Add("@UserId", dn.UserId > 0 ? dn.UserId : 1);

                    long centralDebitNoteId = await conn.ExecuteScalarAsync<long>(
                        "dbo.sp_Central_UpsertDebitNote",
                        upsertParams,
                        transaction: trans,
                        commandType: CommandType.StoredProcedure
                    );

                    if (tx.DebitNoteDetails != null)
                    {
                        foreach (var d in tx.DebitNoteDetails)
                        {
                            var detailParams = new DynamicParameters();
                            detailParams.Add("@CentralDebitNoteId", centralDebitNoteId);
                            detailParams.Add("@BranchId", branchId);
                            detailParams.Add("@TransactionGuid", tx.TransactionGuid);
                            detailParams.Add("@BranchDebitNoteId", dn.BranchDebitNoteId);
                            detailParams.Add("@BillNo", d.BillNo);
                            detailParams.Add("@BillDate", SafeSqlDate(d.BillDate));
                            detailParams.Add("@BillAmount", d.BillAmount);
                            detailParams.Add("@DebitAmount", d.DebitAmount);
                            detailParams.Add("@BalanceAmount", d.BalanceAmount);
                            detailParams.Add("@CancelFlag", d.CancelFlag);

                            await conn.ExecuteAsync(
                                "dbo.sp_Central_UpsertDebitNoteDetail",
                                detailParams,
                                transaction: trans,
                                commandType: CommandType.StoredProcedure
                            );
                        }
                    }

                    if (tx.Vouchers != null)
                    {
                        foreach (var v in tx.Vouchers)
                        {
                            var vParams = new DynamicParameters();
                            vParams.Add("@BranchId", branchId);
                            vParams.Add("@TransactionGuid", tx.TransactionGuid);
                            vParams.Add("@BranchVoucherId", v.BranchVoucherId);
                            vParams.Add("@CentralTransactionID", centralDebitNoteId);
                            vParams.Add("@VoucherType", "DRNOTE");
                            vParams.Add("@LedgerID", v.LedgerID ?? 0);
                            vParams.Add("@LedgerName", v.LedgerName);
                            vParams.Add("@Debit", v.Debit);
                            vParams.Add("@Credit", v.Credit);
                            vParams.Add("@Narration", v.Narration);
                            vParams.Add("@VoucherDate", SafeSqlDate(dn.VoucherDate));

                            await conn.ExecuteAsync(
                                "dbo.sp_Central_UpsertVoucher",
                                vParams,
                                transaction: trans,
                                commandType: CommandType.StoredProcedure
                            );
                        }
                    }

                    trans.Commit();

                    return new SyncItemResult
                    {
                        TransactionGuid = tx.TransactionGuid,
                        EntityType = tx.EntityType,
                        EntityId = entityId,
                        Status = "Synced",
                        CentralTransactionId = centralDebitNoteId
                    };
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }
        #endregion

        #region Stock Adjustment Transaction Ingest
        private async Task<SyncItemResult> IngestStockAdjustmentTransactionAsync(SqlConnection conn, int branchId, TransactionSyncDto tx)
        {
            string entityId = tx.StockAdjustment != null ? tx.StockAdjustment.StockAdjustmentNo.ToString() : "Unknown";

            if (tx.StockAdjustment == null && !tx.Operation.Equals("CANCEL", StringComparison.OrdinalIgnoreCase))
            {
                return new SyncItemResult
                {
                    TransactionGuid = tx.TransactionGuid,
                    EntityType = tx.EntityType,
                    EntityId = entityId,
                    Status = "Failed",
                    ErrorMessage = "Payload missing StockAdjustment header record."
                };
            }

            // Check if already synced
            var existing = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT CentralStockAdjustmentID FROM dbo.StockAdjustmentMaster WHERE BranchId = @BranchId AND TransactionGuid = @TransactionGuid",
                new { BranchId = branchId, tx.TransactionGuid }
            );

            if (existing != null && tx.Operation.Equals("CREATE", StringComparison.OrdinalIgnoreCase))
            {
                return new SyncItemResult
                {
                    TransactionGuid = tx.TransactionGuid,
                    EntityType = tx.EntityType,
                    EntityId = entityId,
                    Status = "AlreadySynced",
                    CentralTransactionId = (long)existing.CentralStockAdjustmentID
                };
            }

            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    if (tx.Operation.Equals("CANCEL", StringComparison.OrdinalIgnoreCase))
                    {
                        await conn.ExecuteAsync(
                            "dbo.sp_Central_CancelTransaction",
                            new
                            {
                                BranchId = branchId,
                                EntityType = "STOCK_ADJUSTMENT",
                                TransactionGuid = tx.TransactionGuid
                            },
                            transaction: trans,
                            commandType: CommandType.StoredProcedure
                        );

                        trans.Commit();
                        return new SyncItemResult
                        {
                            TransactionGuid = tx.TransactionGuid,
                            EntityType = tx.EntityType,
                            EntityId = entityId,
                            Status = "Cancelled"
                        };
                    }

                    var sa = tx.StockAdjustment;

                    var upsertParams = new DynamicParameters();
                    upsertParams.Add("@BranchId", branchId);
                    upsertParams.Add("@TransactionGuid", tx.TransactionGuid);
                    upsertParams.Add("@BranchStockAdjustmentId", sa.BranchStockAdjustmentId);
                    upsertParams.Add("@StockAdjustmentNo", sa.StockAdjustmentNo);
                    upsertParams.Add("@StockAdjustmentDate", SafeSqlDate(sa.StockAdjustmentDate));
                    upsertParams.Add("@Comments", sa.Comments);
                    upsertParams.Add("@LedgerId", sa.LedgerId);
                    upsertParams.Add("@VoucherId", sa.VoucherId);
                    upsertParams.Add("@UserId", sa.UserId > 0 ? sa.UserId : 1);
                    upsertParams.Add("@CancelFlag", sa.CancelFlag);
                    upsertParams.Add("@CategoryId", sa.CategoryId);

                    long centralStockAdjId = await conn.ExecuteScalarAsync<long>(
                        "dbo.sp_Central_UpsertStockAdjustment",
                        upsertParams,
                        transaction: trans,
                        commandType: CommandType.StoredProcedure
                    );

                    if (tx.StockAdjustmentDetails != null)
                    {
                        foreach (var d in tx.StockAdjustmentDetails)
                        {
                            var detailParams = new DynamicParameters();
                            detailParams.Add("@CentralStockAdjustmentID", centralStockAdjId);
                            detailParams.Add("@BranchId", branchId);
                            detailParams.Add("@TransactionGuid", tx.TransactionGuid);
                            detailParams.Add("@BranchStockAdjustmentNo", sa.StockAdjustmentNo);
                            detailParams.Add("@SlNo", d.SlNo);
                            detailParams.Add("@ItemId", d.ItemId);
                            detailParams.Add("@UnitId", d.UnitId);
                            detailParams.Add("@Packing", d.Packing);
                            detailParams.Add("@IsBaseUnit", d.IsBaseUnit);
                            detailParams.Add("@Cost", d.Cost);
                            detailParams.Add("@OriginalCost", d.OriginalCost);
                            detailParams.Add("@SystemStock", d.SystemStock);
                            detailParams.Add("@PhysicalStock", d.PhysicalStock);
                            detailParams.Add("@QtyDifference", d.QtyDifference);
                            detailParams.Add("@Reason", d.Reason);
                            detailParams.Add("@CancelFlag", d.CancelFlag);

                            await conn.ExecuteAsync(
                                "dbo.sp_Central_UpsertStockAdjustmentDetail",
                                detailParams,
                                transaction: trans,
                                commandType: CommandType.StoredProcedure
                            );
                        }
                    }

                    if (tx.Vouchers != null)
                    {
                        foreach (var v in tx.Vouchers)
                        {
                            var vParams = new DynamicParameters();
                            vParams.Add("@BranchId", branchId);
                            vParams.Add("@TransactionGuid", tx.TransactionGuid);
                            vParams.Add("@BranchVoucherId", v.BranchVoucherId);
                            vParams.Add("@CentralTransactionID", centralStockAdjId);
                            vParams.Add("@VoucherType", "PhysicalStock");
                            vParams.Add("@LedgerID", v.LedgerID ?? 0);
                            vParams.Add("@LedgerName", v.LedgerName);
                            vParams.Add("@Debit", v.Debit);
                            vParams.Add("@Credit", v.Credit);
                            vParams.Add("@Narration", v.Narration);
                            vParams.Add("@VoucherDate", SafeSqlDate(sa.StockAdjustmentDate));

                            await conn.ExecuteAsync(
                                "dbo.sp_Central_UpsertVoucher",
                                vParams,
                                transaction: trans,
                                commandType: CommandType.StoredProcedure
                            );
                        }
                    }

                    trans.Commit();

                    return new SyncItemResult
                    {
                        TransactionGuid = tx.TransactionGuid,
                        EntityType = tx.EntityType,
                        EntityId = entityId,
                        Status = "Synced",
                        CentralTransactionId = centralStockAdjId
                    };
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }
        #endregion

        #region Shift & Counter Day-End Closing
        private async Task<SyncItemResult> IngestShiftClosingTransactionAsync(SqlConnection conn, int branchId, TransactionSyncDto tx)
        {
            string entityId = tx.ShiftClosing != null ? tx.ShiftClosing.BranchShiftClosingId.ToString() : "0";

            if (tx.ShiftClosing == null)
            {
                return new SyncItemResult
                {
                    TransactionGuid = tx.TransactionGuid,
                    EntityType = tx.EntityType,
                    EntityId = entityId,
                    Status = "Failed",
                    ErrorMessage = "ShiftClosing payload is missing in TransactionSyncDto"
                };
            }

            var checkParams = new DynamicParameters();
            checkParams.Add("@BranchId", branchId);
            checkParams.Add("@EntityType", "SHIFT_CLOSING");
            checkParams.Add("@TransactionGuid", tx.TransactionGuid);

            var existingTx = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "dbo.sp_Central_CheckTransactionGuid",
                checkParams,
                commandType: CommandType.StoredProcedure
            );

            if (existingTx != null && tx.Operation.Equals("CREATE", StringComparison.OrdinalIgnoreCase))
            {
                return new SyncItemResult
                {
                    TransactionGuid = tx.TransactionGuid,
                    EntityType = tx.EntityType,
                    EntityId = entityId,
                    Status = "AlreadySynced",
                    CentralTransactionId = Convert.ToInt64(existingTx.CentralId)
                };
            }

            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    var sc = tx.ShiftClosing;

                    var upsertParams = new DynamicParameters();
                    upsertParams.Add("@BranchId", branchId);
                    upsertParams.Add("@TransactionGuid", tx.TransactionGuid);
                    upsertParams.Add("@BranchShiftClosingId", sc.BranchShiftClosingId);
                    upsertParams.Add("@CompanyId", sc.CompanyId > 0 ? sc.CompanyId : 1);
                    upsertParams.Add("@FinYearId", sc.FinYearId > 0 ? sc.FinYearId : 1);
                    upsertParams.Add("@Counter", sc.Counter ?? string.Empty);
                    upsertParams.Add("@UserId", sc.UserId > 0 ? sc.UserId : 1);
                    upsertParams.Add("@ClosingDate", SafeSqlDate(sc.ClosingDate));
                    upsertParams.Add("@ReportSelection", sc.ReportSelection ?? string.Empty);
                    upsertParams.Add("@DocNo", sc.DocNo ?? string.Empty);
                    upsertParams.Add("@TotalGrossSales", sc.TotalGrossSales);
                    upsertParams.Add("@TotalDiscount", sc.TotalDiscount);
                    upsertParams.Add("@TotalReturn", sc.TotalReturn);
                    upsertParams.Add("@NetSales", sc.NetSales);
                    upsertParams.Add("@CashSale", sc.CashSale);
                    upsertParams.Add("@CardSale", sc.CardSale);
                    upsertParams.Add("@UpiSale", sc.UpiSale);
                    upsertParams.Add("@CreditSale", sc.CreditSale);
                    upsertParams.Add("@CustomerReceipt", sc.CustomerReceipt);
                    upsertParams.Add("@TotalCollection", sc.TotalCollection);
                    upsertParams.Add("@CashRefundAdjusted", sc.CashRefundAdjusted);
                    upsertParams.Add("@MidDayCashSkim", sc.MidDayCashSkim);
                    upsertParams.Add("@SystemExpectedCash", sc.SystemExpectedCash);
                    upsertParams.Add("@PhysicalCashCounted", sc.PhysicalCashCounted);
                    upsertParams.Add("@CashDifference", sc.CashDifference);
                    upsertParams.Add("@DifferenceReason", sc.DifferenceReason ?? string.Empty);
                    upsertParams.Add("@Status", sc.Status ?? "Closed");
                    upsertParams.Add("@VoucherId", sc.VoucherId);
                    upsertParams.Add("@CounterSessionId", sc.CounterSessionId);

                    long centralShiftClosingId = await conn.ExecuteScalarAsync<long>(
                        "dbo.sp_Central_UpsertShiftClosing",
                        upsertParams,
                        transaction: trans,
                        commandType: CommandType.StoredProcedure
                    );

                    if (tx.ShiftClosingDenominations != null)
                    {
                        foreach (var d in tx.ShiftClosingDenominations)
                        {
                            var denomParams = new DynamicParameters();
                            denomParams.Add("@CentralShiftClosingID", centralShiftClosingId);
                            denomParams.Add("@BranchId", branchId);
                            denomParams.Add("@TransactionGuid", tx.TransactionGuid);
                            denomParams.Add("@BranchShiftClosingId", sc.BranchShiftClosingId);
                            denomParams.Add("@No", d.No);
                            denomParams.Add("@Denomination", d.Denomination);
                            denomParams.Add("@Quantity", d.Quantity);
                            denomParams.Add("@Amount", d.Amount);

                            await conn.ExecuteAsync(
                                "dbo.sp_Central_UpsertShiftClosingDenomination",
                                denomParams,
                                transaction: trans,
                                commandType: CommandType.StoredProcedure
                            );
                        }
                    }

                    if (tx.CounterSession != null)
                    {
                        var cs = tx.CounterSession;
                        var sessionParams = new DynamicParameters();
                        sessionParams.Add("@BranchId", branchId);
                        sessionParams.Add("@BranchSessionID", cs.BranchSessionId);
                        sessionParams.Add("@CompanyId", cs.CompanyId > 0 ? cs.CompanyId : 1);
                        sessionParams.Add("@FinYearId", cs.FinYearId > 0 ? cs.FinYearId : 1);
                        sessionParams.Add("@CounterId", cs.CounterId > 0 ? cs.CounterId : 1);
                        sessionParams.Add("@CounterName", cs.CounterName ?? string.Empty);
                        sessionParams.Add("@UserId", cs.UserId > 0 ? cs.UserId : 1);
                        sessionParams.Add("@LoginTime", SafeSqlDate(cs.LoginTime));
                        sessionParams.Add("@CloseTime", SafeSqlDate(cs.CloseTime));
                        sessionParams.Add("@ShiftClosingId", sc.BranchShiftClosingId);
                        sessionParams.Add("@Status", cs.Status ?? "Closed");
                        sessionParams.Add("@SystemName", cs.SystemName ?? string.Empty);

                        await conn.ExecuteAsync(
                            "dbo.sp_Central_UpsertCounterSession",
                            sessionParams,
                            transaction: trans,
                            commandType: CommandType.StoredProcedure
                        );
                    }

                    if (tx.Vouchers != null)
                    {
                        foreach (var v in tx.Vouchers)
                        {
                            var vParams = new DynamicParameters();
                            vParams.Add("@BranchId", branchId);
                            vParams.Add("@TransactionGuid", tx.TransactionGuid);
                            vParams.Add("@BranchVoucherId", v.BranchVoucherId);
                            vParams.Add("@CentralTransactionID", centralShiftClosingId);
                            vParams.Add("@VoucherType", "ShiftClosing");
                            vParams.Add("@LedgerID", v.LedgerID ?? 0);
                            vParams.Add("@LedgerName", v.LedgerName);
                            vParams.Add("@Debit", v.Debit);
                            vParams.Add("@Credit", v.Credit);
                            vParams.Add("@Narration", v.Narration);
                            vParams.Add("@VoucherDate", SafeSqlDate(sc.ClosingDate));

                            await conn.ExecuteAsync(
                                "dbo.sp_Central_UpsertVoucher",
                                vParams,
                                transaction: trans,
                                commandType: CommandType.StoredProcedure
                            );
                        }
                    }

                    trans.Commit();

                    return new SyncItemResult
                    {
                        TransactionGuid = tx.TransactionGuid,
                        EntityType = tx.EntityType,
                        EntityId = entityId,
                        Status = "Synced",
                        CentralTransactionId = centralShiftClosingId
                    };
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }
        #endregion

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
    }
}
