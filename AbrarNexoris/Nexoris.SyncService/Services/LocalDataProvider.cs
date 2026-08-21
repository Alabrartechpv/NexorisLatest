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
