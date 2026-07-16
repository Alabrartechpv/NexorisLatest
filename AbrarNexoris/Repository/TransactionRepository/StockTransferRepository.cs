using Dapper;
using ModelClass;
using ModelClass.TransactionModels;
using Repository.MasterRepositry;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace Repository.TransactionRepository
{
    public class StockTransferRepository : BaseRepostitory
    {
        public int mstrId = 0;
        private LedgerRepository objLedgerRepository = new LedgerRepository();

        public int GenerateTransferNo(SqlTransaction trans = null)
        {
            int transferNo = 0;
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_StockTransfer, (SqlConnection)DataConnection, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@_Operation", "GENERATENUMBER");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            transferNo = Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return transferNo;
        }

        public string saveStockTransfer(StockTransferMaster master, List<StockTransferDetail> details)
        {
            if (master == null) return "Failed: Master record cannot be null";
            if (details == null || details.Count == 0) return "Failed: No details rows to transfer";

            SqlTransaction transaction = null;

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                transaction = (SqlTransaction)DataConnection.BeginTransaction();

                // 1. Generate voucher IDs for Source and Target branches
                Voucher sourceVoucher = new Voucher
                {
                    _Operation = "GENERATENUMBER",
                    CompanyID = SessionContext.CompanyId,
                    BranchID = master.SourceId,
                    FinYearID = SessionContext.FinYearId,
                    VoucherType = "Stock Transfer",
                    LedgerID = 0
                };

                List<Voucher> sourceVouchers = DataConnection.Query<Voucher>(
                    STOREDPROCEDURE.POS_Vouchers,
                    sourceVoucher,
                    transaction,
                    commandType: CommandType.StoredProcedure).ToList();

                if (sourceVouchers.Count > 0)
                {
                    master.SourceVoucherId = Convert.ToInt32(sourceVouchers[0].VoucherID);
                }

                Voucher targetVoucher = new Voucher
                {
                    _Operation = "GENERATENUMBER",
                    CompanyID = SessionContext.CompanyId,
                    BranchID = master.TargetId,
                    FinYearID = SessionContext.FinYearId,
                    VoucherType = "Stock Transfer",
                    LedgerID = 0
                };

                List<Voucher> targetVouchers = DataConnection.Query<Voucher>(
                    STOREDPROCEDURE.POS_Vouchers,
                    targetVoucher,
                    transaction,
                    commandType: CommandType.StoredProcedure).ToList();

                if (targetVouchers.Count > 0)
                {
                    master.TargetVoucherId = Convert.ToInt32(targetVouchers[0].VoucherID);
                }

                // 2. Insert Stock Transfer Master
                master._Operation = "CREATE";
                master.CancelFlag = false;

                var multi = DataConnection.QueryMultiple(
                    STOREDPROCEDURE.POS_StockTransfer,
                    master,
                    transaction,
                    commandType: CommandType.StoredProcedure);

                int stkTrNo = multi.Read<int>().FirstOrDefault();
                int targetBranchLedgerId = multi.Read<int>().FirstOrDefault();
                int sourceBranchLedgerId = multi.Read<int>().FirstOrDefault();

                master.StkTrNo = stkTrNo;

                // Retrieve the inserted Master Id
                int masterId = DataConnection.Query<int>(
                    "SELECT Id FROM StockTransferMaster WHERE StkTrNo = @StkTrNo AND CompanyId = @CompanyId AND CancelFlag = 0",
                    new { StkTrNo = stkTrNo, CompanyId = master.CompanyId },
                    transaction).FirstOrDefault();

                if (masterId == 0)
                {
                    transaction.Rollback();
                    return "Failed: Could not retrieve generated stock transfer master Id";
                }
                this.mstrId = masterId;

                // 3. Pre-fetch item cache to resolve packing values
                var itemCache = new Dictionary<string, List<ItemDDl>>();
                var dropdowns = new Dropdowns();
                foreach (var detail in details)
                {
                    string barcode = detail.BarCode;
                    if (string.IsNullOrWhiteSpace(barcode) || itemCache.ContainsKey(barcode)) continue;

                    // Query matching items
                    DataBase.Operations = "BARCODEPURCHASE";
                    ItemDDlGrid itemInfo = dropdowns.itemDDlGrid(barcode, null);
                    if (itemInfo != null && itemInfo.List != null && itemInfo.List.Any())
                    {
                        var matchingItems = itemInfo.List.Where(x => x.BarCode == barcode).ToList();
                        if (matchingItems.Any())
                        {
                            itemCache[barcode] = matchingItems;
                        }
                    }
                }

                // 4. Save stock transfer details & adjust stock
                for (int i = 0; i < details.Count; i++)
                {
                    var detail = details[i];
                    detail.StkTranMasterId = masterId;
                    detail.StkTrNo = stkTrNo;
                    detail.TransferDate = master.TransferDate;
                    detail.SourceId = master.SourceId;
                    detail.TargetId = master.TargetId;
                    detail.SlNo = i + 1;
                    detail.CompanyId = master.CompanyId;
                    detail.FinYearId = master.FinYearId;
                    detail.CancelFlag = false;
                    detail._Operation = "CREATE";

                    // Retrieve packing info from cache
                    if (itemCache.TryGetValue(detail.BarCode, out var cacheList) && cacheList.Any())
                    {
                        var item = cacheList.FirstOrDefault(x => x.UnitId == detail.UnitId) ?? cacheList.First();
                        detail.SourcePacking = Convert.ToSingle(item.Packing);
                        detail.SourceBaseUnit = item.IsBaseUnit ?? "N";
                        detail.TargetPacking = Convert.ToSingle(item.Packing);
                        detail.TargetBaseUnit = item.IsBaseUnit ?? "N";
                    }
                    else
                    {
                        detail.SourcePacking = 1.0f;
                        detail.SourceBaseUnit = "Y";
                        detail.TargetPacking = 1.0f;
                        detail.TargetBaseUnit = "Y";
                    }

                    // Execute details/stock adjustment SP
                    string detailsResult = DataConnection.Query<string>(
                        STOREDPROCEDURE.POS_StockTransferDetails,
                        detail,
                        transaction,
                        commandType: CommandType.StoredProcedure).FirstOrDefault();

                    if (detailsResult != "SUCCESS")
                    {
                        transaction.Rollback();
                        return $"Failed on item line {i + 1}: {detailsResult}";
                    }
                }

                // 5. Post accounting vouchers
                // Source Branch Voucher (Debit: Target Branch Ledger, Credit: Purchase/Inventory)
                int sourcePurchaseLedgerId = objLedgerRepository.GetLedgerId(DefaultLedgers.PURCHASE, (int)AccountGroup.PURCHASE_ACCOUNT, master.SourceId);
                
                if (sourceBranchLedgerId > 0 && sourcePurchaseLedgerId > 0)
                {
                    // Debit Entry
                    Voucher vDebit = new Voucher
                    {
                        CompanyID = master.CompanyId,
                        BranchID = master.SourceId,
                        VoucherID = master.SourceVoucherId,
                        VoucherSeriesID = 1,
                        VoucherDate = master.TransferDate,
                        VoucherNumber = master.StkTrNo.ToString(),
                        LedgerID = sourceBranchLedgerId,
                        VoucherType = "Stock Transfer",
                        Debit = Convert.ToDouble(master.TotalAmount),
                        Credit = 0,
                        Narration = master.Description ?? "Stock Transfer Out",
                        SlNo = 1,
                        UserDate = DateTime.Now,
                        UserID = master.UserId,
                        CancelFlag = false,
                        FinYearID = master.FinYearId,
                        _Operation = "CREATE"
                    };
                    DataConnection.Execute(STOREDPROCEDURE.POS_Vouchers, vDebit, transaction, commandType: CommandType.StoredProcedure);

                    // Credit Entry
                    Voucher vCredit = new Voucher
                    {
                        CompanyID = master.CompanyId,
                        BranchID = master.SourceId,
                        VoucherID = master.SourceVoucherId,
                        VoucherSeriesID = 1,
                        VoucherDate = master.TransferDate,
                        VoucherNumber = master.StkTrNo.ToString(),
                        LedgerID = sourcePurchaseLedgerId,
                        VoucherType = "Stock Transfer",
                        Debit = 0,
                        Credit = Convert.ToDouble(master.TotalAmount),
                        Narration = master.Description ?? "Stock Transfer Out",
                        SlNo = 2,
                        UserDate = DateTime.Now,
                        UserID = master.UserId,
                        CancelFlag = false,
                        FinYearID = master.FinYearId,
                        _Operation = "CREATE"
                    };
                    DataConnection.Execute(STOREDPROCEDURE.POS_Vouchers, vCredit, transaction, commandType: CommandType.StoredProcedure);
                }

                // Target Branch Voucher (Debit: Purchase/Inventory, Credit: Source Branch Ledger)
                int targetPurchaseLedgerId = objLedgerRepository.GetLedgerId(DefaultLedgers.PURCHASE, (int)AccountGroup.PURCHASE_ACCOUNT, master.TargetId);
                
                if (targetBranchLedgerId > 0 && targetPurchaseLedgerId > 0)
                {
                    // Debit Entry
                    Voucher vDebit = new Voucher
                    {
                        CompanyID = master.CompanyId,
                        BranchID = master.TargetId,
                        VoucherID = master.TargetVoucherId,
                        VoucherSeriesID = 1,
                        VoucherDate = master.TransferDate,
                        VoucherNumber = master.StkTrNo.ToString(),
                        LedgerID = targetPurchaseLedgerId,
                        VoucherType = "Stock Transfer",
                        Debit = Convert.ToDouble(master.TotalAmount),
                        Credit = 0,
                        Narration = master.Description ?? "Stock Transfer In",
                        SlNo = 1,
                        UserDate = DateTime.Now,
                        UserID = master.UserId,
                        CancelFlag = false,
                        FinYearID = master.FinYearId,
                        _Operation = "CREATE"
                    };
                    DataConnection.Execute(STOREDPROCEDURE.POS_Vouchers, vDebit, transaction, commandType: CommandType.StoredProcedure);

                    // Credit Entry
                    Voucher vCredit = new Voucher
                    {
                        CompanyID = master.CompanyId,
                        BranchID = master.TargetId,
                        VoucherID = master.TargetVoucherId,
                        VoucherSeriesID = 1,
                        VoucherDate = master.TransferDate,
                        VoucherNumber = master.StkTrNo.ToString(),
                        LedgerID = targetBranchLedgerId,
                        VoucherType = "Stock Transfer",
                        Debit = 0,
                        Credit = Convert.ToDouble(master.TotalAmount),
                        Narration = master.Description ?? "Stock Transfer In",
                        SlNo = 2,
                        UserDate = DateTime.Now,
                        UserID = master.UserId,
                        CancelFlag = false,
                        FinYearID = master.FinYearId,
                        _Operation = "CREATE"
                    };
                    DataConnection.Execute(STOREDPROCEDURE.POS_Vouchers, vCredit, transaction, commandType: CommandType.StoredProcedure);
                }

                transaction.Commit();
                return "success";
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                return $"Error: {ex.Message}";
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        public IEnumerable<dynamic> GetStockTransfers(int companyId, int? stkTrNo = null)
        {
            try
            {
                var p = new DynamicParameters();
                p.Add("@CompanyId", companyId);
                p.Add("@_Operation", "GETALL");
                p.Add("@PageIndex", 0);
                p.Add("@PageSize", 10000);
                if (stkTrNo.HasValue)
                {
                    p.Add("@STKTrNo", stkTrNo.Value);
                }

                return DataConnection.Query<dynamic>(
                    STOREDPROCEDURE.POS_StockTransfer,
                    p,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Tuple<StockTransferMaster, List<StockTransferDetail>> GetStockTransferById(int id)
        {
            try
            {
                var p = new DynamicParameters();
                p.Add("@Id", id);
                p.Add("@_Operation", "GETBYID");

                using (var multi = DataConnection.QueryMultiple(
                    STOREDPROCEDURE.POS_StockTransfer,
                    p,
                    commandType: CommandType.StoredProcedure))
                {
                    var master = multi.Read<StockTransferMaster>().FirstOrDefault();
                    var details = multi.Read<StockTransferDetail>().ToList();
                    return new Tuple<StockTransferMaster, List<StockTransferDetail>>(master, details);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
