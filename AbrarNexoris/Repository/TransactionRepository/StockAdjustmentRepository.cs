using Dapper;
using ModelClass;
using ModelClass.TransactionModels;
using Repository.MasterRepositry;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Repository.TransactionRepository
{
    public class StockAdjustmentRepository : BaseRepostitory
    {
        public int mstrId = 0;
        LedgerRepository objLedgerRepository = new LedgerRepository();
        public StockAdjustmentDetails GetByIdItem(int selectedId)
        {
            StockAdjustmentDetails stockdet = new StockAdjustmentDetails();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_StockAdjustemnt, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", selectedId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            stockdet = ds.Tables[0].Rows[0].ToNullableObject<StockAdjustmentDetails>();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return stockdet;
        }
        public int GenerateAdjustNo(SqlTransaction trans = null)
        {
            int AdjNo = 0;
            // DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_StockAdjustemnt, (SqlConnection)DataConnection, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@_Operation", "GENERATENUMBER");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            AdjNo = Convert.ToInt32(ds.Tables[0].Rows[0].ItemArray[0].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    // DataConnection.Close();
                }
            }
            return AdjNo;
        }

        /// <summary>
        /// Saves a new stock adjustment record and its details to the database
        /// </summary>
        /// <param name="stockMaster">The stock adjustment master record</param>
        /// <param name="stockDetail">The stock adjustment detail record template</param>
        /// <param name="itemsGrid">DataGridView containing the items to adjust</param>
        /// <returns>Status message: "success" or error message</returns>
        public string saveStock(StockAdjMaster stockMaster, StockAdjPriceDetails stockDetail, DataGridView itemsGrid)
        {
            if (stockMaster == null) return "Failed: Stock master cannot be null";
            if (stockDetail == null) return "Failed: Stock detail cannot be null";
            if (itemsGrid == null || itemsGrid.Rows.Count == 0) return "Failed: No items to adjust";

            int validationStockInHandLedgerId = objLedgerRepository.GetLedgerId(DefaultLedgers.BEGINSTOCK, (int)AccountGroup.STOCK_IN_HAND, SessionContext.BranchId);
            if (stockMaster.LedgerId == validationStockInHandLedgerId)
            {
                return "Failed: The primary STOCK IN HAND ledger cannot be selected as the adjustment reason.";
            }

            Voucher voucher = new Voucher();
            SqlTransaction transaction = null;

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                transaction = (SqlTransaction)DataConnection.BeginTransaction();

                // 1. Generate voucher number for the adjustment
                voucher._Operation = "GENERATENUMBER";
                voucher.CompanyID = SessionContext.CompanyId;
                voucher.BranchID = SessionContext.BranchId;
                voucher.FinYearID = SessionContext.FinYearId;
                voucher.VoucherType = "PhysicalStock";
                voucher.LedgerID = stockMaster.LedgerId;

                List<Voucher> vouchersList = DataConnection.Query<Voucher>(
                    STOREDPROCEDURE.POS_Vouchers,
                    voucher,
                    transaction,
                    commandType: CommandType.StoredProcedure).ToList<Voucher>();

                if (vouchersList.Count > 0)
                {
                    voucher.VoucherID = vouchersList[0].VoucherID;
                    stockMaster.VoucherId = Convert.ToInt32(voucher.VoucherID);
                }
                else
                {
                    transaction.Rollback();
                    return "Failed: Could not generate voucher number";
                }

                // 2. Save stock adjustment master record
                stockMaster._Operation = "CREATE";
                stockMaster.Id = 0;
                stockMaster.CompanyId = SessionContext.CompanyId;
                stockMaster.FinYearId = SessionContext.FinYearId;
                stockMaster.BranchId = SessionContext.BranchId;
                stockMaster.AccountGroupId = voucher.GroupID;
                stockMaster.UserId = SessionContext.UserId;
                stockMaster.CancelFlag = 0;
                stockMaster.VoucherType = "PhysicalStock";

                List<StockAdjMaster> stockMasterResult = DataConnection.Query<StockAdjMaster>(
                    STOREDPROCEDURE.POS_StockAdjustemnt,
                    stockMaster,
                    transaction,
                    commandType: CommandType.StoredProcedure).ToList<StockAdjMaster>();

                // Store the master ID for detail records
                int masterId = 0;
                if (stockMasterResult.Count > 0)
                {
                    masterId = stockMasterResult[0].Id;
                    this.mstrId = masterId;
                }
                else
                {
                    transaction.Rollback();
                    return "Failed: Could not create stock adjustment master record";
                }

                // 3. Pre-fetch all item details BEFORE processing the loop
                // This avoids calling itemDDlGrid inside the transaction which causes connection issues
                var itemCache = new Dictionary<string, List<ItemDDl>>();
                for (int i = 0; i < itemsGrid.RowCount; i++)
                {
                    DataGridViewRow row = itemsGrid.Rows[i];
                    if (row.Cells["BarCode"].Value == null) continue;

                    string barcode = row.Cells["BarCode"].Value?.ToString() ?? "";
                    if (string.IsNullOrWhiteSpace(barcode) || itemCache.ContainsKey(barcode)) continue;

                    // Fetch item details outside the transaction
                    DataBase.Operations = "BARCODEPURCHASE";
                    ItemDDlGrid itemInfo = new Dropdowns().itemDDlGrid(barcode, null);
                    if (itemInfo != null && itemInfo.List != null && itemInfo.List.Any())
                    {
                        var matchingItems = itemInfo.List.Where(x => x.BarCode == barcode).ToList();
                        if (matchingItems.Any())
                        {
                            itemCache[barcode] = matchingItems;
                        }
                    }
                }

                // 4. Process all items and calculate adjustment values
                double totalAdjustmentValue = 0;
                int totalQtyDifference = 0;
                int successfulDetailSaves = 0;

                for (int i = 0; i < itemsGrid.RowCount; i++)
                {
                    DataGridViewRow row = itemsGrid.Rows[i];
                    try
                    {
                        // Skip rows with incomplete data
                        if (row.Cells["ItemNo"].Value == null ||
                            row.Cells["BarCode"].Value == null ||
                            row.Cells["No"].Value == null)
                        {
                            continue;
                        }

                        // Get item details from grid
                        int itemId = Convert.ToInt32(row.Cells["ItemNo"].Value?.ToString() ?? "0");
                        string barcode = row.Cells["BarCode"].Value?.ToString() ?? "";

                        if (string.IsNullOrWhiteSpace(barcode) || itemId == 0)
                        {
                            continue;
                        }

                        // Get detailed item information including UOM
                        string uom = row.Cells["UOM"].Value?.ToString() ?? "";

                        // Use cached item data instead of calling database
                        if (!itemCache.TryGetValue(barcode, out var matchingItems) || !matchingItems.Any())
                        {
                            transaction.Rollback();
                            return $"Failed: Item with barcode {barcode} not found in cache";
                        }

                        // If UOM is specified, try to find matching unit first
                        ItemDDl item = null;
                        if (!string.IsNullOrWhiteSpace(uom))
                        {
                            item = matchingItems.FirstOrDefault(x => x.Unit != null && x.Unit.Equals(uom, StringComparison.OrdinalIgnoreCase));
                        }

                        // If no UOM match, get the base unit (IsBaseUnit = 'Y')
                        if (item == null)
                        {
                            item = matchingItems.FirstOrDefault(x => !string.IsNullOrEmpty(x.IsBaseUnit) && x.IsBaseUnit.ToUpper() == "Y");
                        }

                        // If no base unit found, take the first one with Packing = 1
                        if (item == null)
                        {
                            item = matchingItems.FirstOrDefault(x => x.Packing == 1);
                        }

                        // If still null, just take the first one
                        if (item == null)
                        {
                            item = matchingItems.FirstOrDefault();
                        }

                        if (item == null)
                        {
                            transaction.Rollback();
                            return $"Failed: No item details found for barcode {barcode}";
                        }

                        // Convert IsBaseUnit from string ('Y'/'N') to integer (1/0)
                        int isBaseUnit = (!string.IsNullOrEmpty(item.IsBaseUnit) && item.IsBaseUnit.ToUpper() == "Y") ? 1 :
                                        (string.IsNullOrEmpty(item.IsBaseUnit) && item.Packing == 1) ? 1 : 0;

                        int orderedStock = item.OrderedStock > 0 ? Convert.ToInt32(item.OrderedStock) : 0;

                        // Set up detail record for this item
                        StockAdjPriceDetails detailRecord = new StockAdjPriceDetails
                        {
                            FinYearId = SessionContext.FinYearId,
                            CompanyId = SessionContext.CompanyId,
                            BranchId = SessionContext.BranchId,
                            StockAdjustmentMasterId = masterId,
                            LedgerId = stockMaster.LedgerId,
                            SlNo = Convert.ToInt32(row.Cells["No"].Value?.ToString() ?? "0"),
                            ItemId = itemId,
                            UnitId = item.UnitId,
                            UOM = uom,
                            Remarks = stockMaster.Comments ?? "",  // Use master-level comments
                            Description = row.Cells["Description"].Value?.ToString() ?? "",
                            BarCode = barcode,
                            Packing = Convert.ToSingle(item.Packing),
                            IsBaseUnit = isBaseUnit,
                            Cost = Convert.ToSingle(item.Cost),
                            OriginalCost = Convert.ToSingle(item.Cost),
                            OrderedStock = orderedStock,
                            _Operation = "CREATE",
                            Reason = stockMaster.Comments ?? "",  // Use master-level comments
                            CancelFlag = 0
                        };

                        // Get adjustment quantity (amount to add/subtract)
                        // Try "Adjustment Qty" first (new way), then "Physical Qty" (old way), then "Adj Qty" (legacy)
                        float adjustmentQty = 0;
                        if (itemsGrid.Columns.Contains("Adjustment Qty"))
                        {
                            adjustmentQty = Convert.ToSingle(row.Cells["Adjustment Qty"].Value?.ToString() ?? "0");
                        }
                        else if (itemsGrid.Columns.Contains("Physical Qty"))
                        {
                            adjustmentQty = Convert.ToSingle(row.Cells["Physical Qty"].Value?.ToString() ?? "0");
                        }
                        else if (itemsGrid.Columns.Contains("Adj Qty"))
                        {
                            adjustmentQty = Convert.ToSingle(row.Cells["Adj Qty"].Value?.ToString() ?? "0");
                        }

                        // Calculate physical stock and quantity difference
                        detailRecord.SystemStock = Convert.ToSingle(row.Cells["Qty On Hand"].Value?.ToString() ?? "0");
                        // Physical Stock = System Stock + Adjustment Qty (new way)
                        // If using old "Physical Qty" column, it's already the physical count
                        if (itemsGrid.Columns.Contains("Adjustment Qty"))
                        {
                            detailRecord.PhysicalStock = detailRecord.SystemStock + adjustmentQty;
                            detailRecord.QtyDifference = adjustmentQty;
                        }
                        else
                        {
                            // Old way: Physical Qty is the actual physical count
                            detailRecord.PhysicalStock = adjustmentQty;
                            detailRecord.QtyDifference = adjustmentQty - detailRecord.SystemStock;
                        }

                        // Update running totals (use base unit quantities for accurate calculations)
                        float baseUnitQtyDiff = detailRecord.QtyDifference * detailRecord.Packing;
                        totalQtyDifference += (int)baseUnitQtyDiff;
                        totalAdjustmentValue += baseUnitQtyDiff * Convert.ToDouble(detailRecord.Cost);

                        // Use DynamicParameters to explicitly map each parameter
                        var detailParams = new Dapper.DynamicParameters();
                        detailParams.Add("@FinYearId", detailRecord.FinYearId);
                        detailParams.Add("@CompanyId", detailRecord.CompanyId);
                        detailParams.Add("@BranchId", detailRecord.BranchId);
                        detailParams.Add("@StockAdjustmentMasterId", detailRecord.StockAdjustmentMasterId);
                        detailParams.Add("@LedgerId", detailRecord.LedgerId);
                        detailParams.Add("@SlNo", detailRecord.SlNo);
                        detailParams.Add("@ItemId", detailRecord.ItemId);
                        detailParams.Add("@UOM", detailRecord.UOM);
                        detailParams.Add("@Remarks", detailRecord.Remarks);
                        detailParams.Add("@Description", detailRecord.Description);
                        detailParams.Add("@Barcode", detailRecord.BarCode);
                        detailParams.Add("@UnitId", detailRecord.UnitId);
                        detailParams.Add("@Packing", detailRecord.Packing);
                        detailParams.Add("@IsBaseUnit", detailRecord.IsBaseUnit);
                        detailParams.Add("@Cost", detailRecord.Cost);
                        detailParams.Add("@OriginalCost", detailRecord.OriginalCost);
                        detailParams.Add("@SystemStock", detailRecord.SystemStock);
                        detailParams.Add("@PhysicalStock", detailRecord.PhysicalStock);
                        detailParams.Add("@QtyDifference", detailRecord.QtyDifference);
                        detailParams.Add("@CancelFlag", detailRecord.CancelFlag);
                        detailParams.Add("@OrderedStock", detailRecord.OrderedStock);
                        detailParams.Add("@Reason", detailRecord.Reason);
                        detailParams.Add("@_Operation", detailRecord._Operation);

                        string result = null;
                        try
                        {
                            result = DataConnection.Query<string>(
                                STOREDPROCEDURE.POS_StockAdjustemntDetails,
                                detailParams,
                                transaction,
                                commandType: CommandType.StoredProcedure).FirstOrDefault();
                        }
                        catch (SqlException sqlEx)
                        {
                            string errorMsg = $"SQL Error saving detail: {sqlEx.Message}, Number: {sqlEx.Number}, Line: {sqlEx.LineNumber}";
                            transaction.Rollback();
                            return errorMsg;
                        }
                        catch (Exception ex)
                        {
                            string errorMsg = $"Exception saving detail: {ex.Message}";
                            transaction.Rollback();
                            return errorMsg;
                        }

                        if (result != null && result.Contains("CANCELLED"))
                        {
                            transaction.Rollback();
                            return result;
                        }

                        if (result != null && !result.Contains("SUCCESS") && result.Trim() != "")
                        {
                            string errorMsg = $"Failed to save detail for item {itemId} (barcode: {barcode}): {result}";
                            transaction.Rollback();
                            return errorMsg;
                        }

                        successfulDetailSaves++;
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = $"Error processing row {i} (ItemId: {row.Cells["ItemNo"].Value}): {ex.Message}";
                        transaction.Rollback();
                        return $"Failed: {errorMsg}";
                    }
                }

                if (successfulDetailSaves == 0)
                {
                    transaction.Rollback();
                    return "Failed: No detail records were saved";
                }

                // 4. Create accounting voucher entries
                voucher._Operation = "CREATE";
                voucher.VoucherID = voucher.VoucherID;
                voucher.VoucherSeriesID = 0;
                voucher.VoucherDate = DateTime.Now;
                voucher.Mode = "";
                voucher.ModeID = 0;
                voucher.UserDate = DateTime.Now;
                voucher.UserName = DataBase.UserName;
                voucher.UserID = SessionContext.UserId;
                voucher.CancelFlag = false;
                voucher.IsSyncd = false;

                // Get values for voucher entries
                double absAdjustmentValue = Math.Abs(totalAdjustmentValue);
                string adjustmentType = totalQtyDifference >= 0 ? "Stock In" : "Stock Out";
                string narration = $"PhysicalStock: #{stockMaster.StockAdjustmentNo}| {adjustmentType} WORTH:{absAdjustmentValue}| REMARKS: {stockMaster.Comments}";

                // Determine the contra ledger details
                int contraGroupId;
                int contraLedgerId;
                string contraLedgerName;

                if (stockMaster.LedgerId > 0)
                {
                    contraGroupId = Convert.ToInt32(AccountGroup.SUNDRY_CREDITORS); // Default fallback group
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Ledger, (SqlConnection)DataConnection, (SqlTransaction)transaction))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@_Operation", "GETBYID");
                            cmd.Parameters.AddWithValue("@LedgerID", stockMaster.LedgerId);
                            object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                contraGroupId = Convert.ToInt32(result);
                            }
                        }
                    }
                    catch { }

                    contraLedgerId = stockMaster.LedgerId;
                    contraLedgerName = stockMaster.LedgerName;
                }
                else
                {
                    contraGroupId = Convert.ToInt32(AccountGroup.PURCHASE_ACCOUNT);
                    contraLedgerId = objLedgerRepository.GetLedgerId(DefaultLedgers.PURCHASE, (int)AccountGroup.PURCHASE_ACCOUNT, SessionContext.BranchId);
                    contraLedgerName = DefaultLedgers.PURCHASE;
                }

                // --- Resolve STOCK IN HAND ledger (Group 18) for Leg 1 ---
                // Correct journal: Dr STOCK IN HAND / Cr Reason (Stock In)
                //                  Dr Reason / Cr STOCK IN HAND  (Stock Out)
                int stockInHandGroupId = Convert.ToInt32(AccountGroup.STOCK_IN_HAND);
                int stockInHandLedgerId = objLedgerRepository.GetLedgerId(DefaultLedgers.BEGINSTOCK, (int)AccountGroup.STOCK_IN_HAND, SessionContext.BranchId);
                string stockInHandLedgerName = DefaultLedgers.BEGINSTOCK;
                if (stockInHandLedgerId == 0)
                {
                    // Fallback: use stored procedure _4GetAccountLedgerDDL to get stock-in-hand ledgers for this branch
                    try
                    {
                        var ddlRequest = new AccountLedgerDDLRequest { BranchId = SessionContext.BranchId, For = "Stock" };
                        var ddlResult = objLedgerRepository.getAccountLedgerDDL(ddlRequest);
                        if (ddlResult != null && ddlResult.List != null && ddlResult.List.Any())
                        {
                            var firstLedger = ddlResult.List.FirstOrDefault();
                            if (firstLedger != null)
                            {
                                stockInHandLedgerId = firstLedger.Id;
                                stockInHandLedgerName = firstLedger.Name;
                            }
                        }
                    }
                    catch { }
                }

                // Create appropriate voucher entries based on adjustment type
                if (totalQtyDifference >= 0) // Stock In (Positive Adjustment)
                {
                    // Entry 1: Debit STOCK IN HAND (Group 18 — increases stock asset)
                    voucher.GroupID = stockInHandGroupId;
                    voucher.LedgerID = stockInHandLedgerId;
                    voucher.LedgerName = stockInHandLedgerName;
                    voucher.Debit = absAdjustmentValue;
                    voucher.Credit = 0;
                    voucher.Narration = narration;
                    voucher.SlNo = 1;

                    DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, voucher, transaction, commandType: CommandType.StoredProcedure);

                    // Entry 2: Credit Reason/Contra Ledger
                    voucher.GroupID = contraGroupId;
                    voucher.LedgerID = contraLedgerId;
                    voucher.LedgerName = contraLedgerName;
                    voucher.Debit = 0;
                    voucher.Credit = absAdjustmentValue;
                    voucher.Narration = narration;
                    voucher.SlNo = 2;

                    DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, voucher, transaction, commandType: CommandType.StoredProcedure);
                }
                else // Stock Out (Negative Adjustment)
                {
                    // Entry 1: Credit STOCK IN HAND (Group 18 — decreases stock asset)
                    voucher.GroupID = stockInHandGroupId;
                    voucher.LedgerID = stockInHandLedgerId;
                    voucher.LedgerName = stockInHandLedgerName;
                    voucher.Debit = 0;
                    voucher.Credit = absAdjustmentValue;
                    voucher.Narration = narration;
                    voucher.SlNo = 1;

                    DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, voucher, transaction, commandType: CommandType.StoredProcedure);

                    // Entry 2: Debit Reason/Contra Ledger
                    voucher.GroupID = contraGroupId;
                    voucher.LedgerID = contraLedgerId;
                    voucher.LedgerName = contraLedgerName;
                    voucher.Debit = absAdjustmentValue;
                    voucher.Credit = 0;
                    voucher.Narration = narration;
                    voucher.SlNo = 2;

                    DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, voucher, transaction, commandType: CommandType.StoredProcedure);
                }

                // 5. Commit the transaction - ALL operations successful
                transaction.Commit();
                return "success";
            }
            catch (Exception ex)
            {
                try
                {
                    if (transaction != null)
                    {
                        transaction.Rollback();
                    }
                }
                catch (Exception rollbackEx)
                {
                    // Rollback failed
                }

                return $"Failed: {ex.Message}";
            }
            finally
            {
                if (DataConnection != null && DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        public string updateStock(StockAdjMaster sk, StockAdjPriceDetails stkdet, DataGridView dgv_stockadjustment)
        {
            int validationStockInHandLedgerId = objLedgerRepository.GetLedgerId(DefaultLedgers.BEGINSTOCK, (int)AccountGroup.STOCK_IN_HAND, SessionContext.BranchId);
            if (sk.LedgerId == validationStockInHandLedgerId)
            {
                return "Failed: The primary STOCK IN HAND ledger cannot be selected as the adjustment reason.";
            }

            Voucher objVoucher = new Voucher();
            SqlTransaction trans = null;

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                trans = (SqlTransaction)DataConnection.BeginTransaction();
                sk._Operation = "UPDATE";
                sk.CompanyId = SessionContext.CompanyId;
                sk.FinYearId = SessionContext.FinYearId;
                sk.BranchId = SessionContext.BranchId;
                sk.AccountGroupId = objVoucher.GroupID;
                sk.UserId = SessionContext.UserId;
                sk.CancelFlag = 0;
                sk.VoucherType = "PhysicalStock";

                List<StockAdjMaster> liststock = DataConnection.Query<StockAdjMaster>(STOREDPROCEDURE.POS_StockAdjustemnt, sk, trans,
                    commandType: CommandType.StoredProcedure).ToList<StockAdjMaster>();
                if (liststock.Count > 0)
                {
                    foreach (StockAdjMaster stmaster in liststock)
                    {
                        stkdet.StockAdjustmentMasterId = sk.Id;
                    }
                }

                // Pre-fetch all item details BEFORE processing the loop
                // This avoids calling itemDDlGrid inside the transaction which causes connection issues
                var itemCache = new Dictionary<string, List<ItemDDl>>();
                for (int i = 0; i < dgv_stockadjustment.RowCount; i++)
                {
                    if (dgv_stockadjustment.Rows[i].Cells["BarCode"].Value == null) continue;

                    string barcode = dgv_stockadjustment.Rows[i].Cells["BarCode"].Value?.ToString() ?? "";
                    if (string.IsNullOrWhiteSpace(barcode) || itemCache.ContainsKey(barcode)) continue;

                    DataBase.Operations = "BARCODEPURCHASE";
                    ItemDDlGrid itemInfo = new Dropdowns().itemDDlGrid(barcode, null);
                    if (itemInfo != null && itemInfo.List != null && itemInfo.List.Any())
                    {
                        var matchingItems = itemInfo.List.Where(x => x.BarCode == barcode).ToList();
                        if (matchingItems.Any())
                        {
                            itemCache[barcode] = matchingItems;
                        }
                    }
                }

                // Calculate total adjustment value and determine if it's a stock in or stock out
                double totalAdjustmentValue = 0;
                int totalQtyDifference = 0;

                for (int i = 0; i < dgv_stockadjustment.RowCount; i++)
                {
                    try
                    {
                        // Check if the row has valid data before processing
                        if (dgv_stockadjustment.Rows[i].Cells["ItemNo"].Value == null ||
                            dgv_stockadjustment.Rows[i].Cells["BarCode"].Value == null ||
                            dgv_stockadjustment.Rows[i].Cells["No"].Value == null)
                        {
                            continue; // Skip this row if essential data is missing
                        }

                        // Get item details from the grid with null checking
                        int itemId = Convert.ToInt32(dgv_stockadjustment.Rows[i].Cells["ItemNo"].Value?.ToString() ?? "0");
                        string barcode = dgv_stockadjustment.Rows[i].Cells["BarCode"].Value?.ToString() ?? "";
                        string uom = dgv_stockadjustment.Rows[i].Cells["UOM"].Value?.ToString() ?? "";

                        if (string.IsNullOrWhiteSpace(barcode) || itemId == 0)
                        {
                            continue; // Skip invalid rows
                        }

                        // Use cached item data instead of calling database
                        if (!itemCache.TryGetValue(barcode, out var matchingItems) || !matchingItems.Any())
                        {
                            trans.Rollback();
                            return $"Failed: Item with barcode {barcode} not found in cache";
                        }

                        // If UOM is specified, try to find matching unit first
                        ItemDDl item = null;
                        if (!string.IsNullOrWhiteSpace(uom))
                        {
                            item = matchingItems.FirstOrDefault(x => x.Unit != null && x.Unit.Equals(uom, StringComparison.OrdinalIgnoreCase));
                        }

                        // If no UOM match, get the base unit (IsBaseUnit = 'Y')
                        if (item == null)
                        {
                            item = matchingItems.FirstOrDefault(x => !string.IsNullOrEmpty(x.IsBaseUnit) && x.IsBaseUnit.ToUpper() == "Y");
                        }

                        // If no base unit found, take the first one with Packing = 1
                        if (item == null)
                        {
                            item = matchingItems.FirstOrDefault(x => x.Packing == 1);
                        }

                        // If still null, just take the first one
                        if (item == null)
                        {
                            item = matchingItems.FirstOrDefault();
                        }

                        if (item == null)
                        {
                            trans.Rollback();
                            return $"Failed: No item details found for barcode {barcode}";
                        }

                        // Convert IsBaseUnit from string ('Y'/'N') to integer (1/0)
                        int isBaseUnit = (!string.IsNullOrEmpty(item.IsBaseUnit) && item.IsBaseUnit.ToUpper() == "Y") ? 1 :
                                        (string.IsNullOrEmpty(item.IsBaseUnit) && item.Packing == 1) ? 1 : 0;

                        int orderedStock = item.OrderedStock > 0 ? Convert.ToInt32(item.OrderedStock) : 0;

                        // Set stock adjustment details from grid and fetched data
                        stkdet.FinYearId = SessionContext.FinYearId; // Hardcoded as 1
                        stkdet.CompanyId = SessionContext.CompanyId;
                        stkdet.BranchId = SessionContext.BranchId;
                        stkdet.StockAdjustmentMasterId = sk.Id;
                        stkdet.LedgerId = sk.LedgerId;
                        stkdet.SlNo = Convert.ToInt32(dgv_stockadjustment.Rows[i].Cells["No"].Value?.ToString() ?? "0");
                        stkdet.ItemId = itemId;
                        stkdet.UnitId = item.UnitId;
                        stkdet.UOM = uom;
                        stkdet.Remarks = sk.Comments ?? "";  // Use master-level comments
                        stkdet.Description = dgv_stockadjustment.Rows[i].Cells["Description"].Value?.ToString() ?? "";
                        stkdet.BarCode = barcode;
                        stkdet.Packing = Convert.ToSingle(item.Packing);
                        stkdet.IsBaseUnit = isBaseUnit;
                        stkdet.Cost = Convert.ToSingle(item.Cost);
                        stkdet.OriginalCost = Convert.ToSingle(item.Cost);
                        stkdet.OrderedStock = orderedStock;

                        // Get adjustment quantity (amount to add/subtract)
                        // Try "Adjustment Qty" first (new way), then "Physical Qty" (old way), then "Adj Qty" (legacy)
                        float adjustmentQty = 0;
                        if (dgv_stockadjustment.Columns.Contains("Adjustment Qty"))
                        {
                            adjustmentQty = Convert.ToSingle(dgv_stockadjustment.Rows[i].Cells["Adjustment Qty"].Value?.ToString() ?? "0");
                        }
                        else if (dgv_stockadjustment.Columns.Contains("Physical Qty"))
                        {
                            adjustmentQty = Convert.ToSingle(dgv_stockadjustment.Rows[i].Cells["Physical Qty"].Value?.ToString() ?? "0");
                        }
                        else if (dgv_stockadjustment.Columns.Contains("Adj Qty"))
                        {
                            adjustmentQty = Convert.ToSingle(dgv_stockadjustment.Rows[i].Cells["Adj Qty"].Value?.ToString() ?? "0");
                        }

                        stkdet.SystemStock = Convert.ToSingle(dgv_stockadjustment.Rows[i].Cells["Qty On Hand"].Value?.ToString() ?? "0");
                        // Physical Stock = System Stock + Adjustment Qty (new way)
                        // If using old "Physical Qty" column, it's already the physical count
                        if (dgv_stockadjustment.Columns.Contains("Adjustment Qty"))
                        {
                            stkdet.PhysicalStock = stkdet.SystemStock + adjustmentQty;
                            stkdet.QtyDifference = adjustmentQty;
                        }
                        else
                        {
                            // Old way: Physical Qty is the actual physical count
                            stkdet.PhysicalStock = adjustmentQty;
                            stkdet.QtyDifference = adjustmentQty - stkdet.SystemStock;
                        }

                        // Add to total adjustment value (use base unit quantities for accurate calculations)
                        float baseUnitQtyDiff = stkdet.QtyDifference * stkdet.Packing;
                        totalQtyDifference += (int)baseUnitQtyDiff;
                        totalAdjustmentValue += baseUnitQtyDiff * Convert.ToDouble(stkdet.Cost);

                        stkdet.CancelFlag = 0;
                        stkdet.Reason = sk.Comments ?? "";  // Use master-level comments
                        stkdet._Operation = "CREATE";

                        // Use DynamicParameters to explicitly map each parameter
                        var detailParams = new Dapper.DynamicParameters();
                        detailParams.Add("@FinYearId", stkdet.FinYearId);
                        detailParams.Add("@CompanyId", stkdet.CompanyId);
                        detailParams.Add("@BranchId", stkdet.BranchId);
                        detailParams.Add("@StockAdjustmentMasterId", stkdet.StockAdjustmentMasterId);
                        detailParams.Add("@LedgerId", stkdet.LedgerId);
                        detailParams.Add("@SlNo", stkdet.SlNo);
                        detailParams.Add("@ItemId", stkdet.ItemId);
                        detailParams.Add("@UOM", stkdet.UOM);
                        detailParams.Add("@Remarks", stkdet.Remarks);
                        detailParams.Add("@Description", stkdet.Description);
                        detailParams.Add("@Barcode", stkdet.BarCode);
                        detailParams.Add("@UnitId", stkdet.UnitId);
                        detailParams.Add("@Packing", stkdet.Packing);
                        detailParams.Add("@IsBaseUnit", stkdet.IsBaseUnit);
                        detailParams.Add("@Cost", stkdet.Cost);
                        detailParams.Add("@OriginalCost", stkdet.OriginalCost);
                        detailParams.Add("@SystemStock", stkdet.SystemStock);
                        detailParams.Add("@PhysicalStock", stkdet.PhysicalStock);
                        detailParams.Add("@QtyDifference", stkdet.QtyDifference);
                        detailParams.Add("@CancelFlag", stkdet.CancelFlag);
                        detailParams.Add("@OrderedStock", stkdet.OrderedStock);
                        detailParams.Add("@Reason", stkdet.Reason);
                        detailParams.Add("@_Operation", stkdet._Operation);

                        var result = DataConnection.Query<string>(STOREDPROCEDURE.POS_StockAdjustemntDetails, detailParams, trans,
                        commandType: CommandType.StoredProcedure).FirstOrDefault();

                        if (result != null && result.Contains("CANCELLED"))
                        {
                            trans.Rollback();
                            return result;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error processing row {i} in updateStock: {ex.Message}", ex);
                    }
                }

                // First, delete existing vouchers for this adjustment
                objVoucher._Operation = "UPDATE";
                objVoucher.CompanyID = SessionContext.CompanyId;
                objVoucher.BranchID = SessionContext.BranchId;
                objVoucher.FinYearID = SessionContext.FinYearId;
                objVoucher.VoucherID = sk.VoucherId;
                objVoucher.VoucherType = "PhysicalStock";

                // This will delete existing vouchers as per the stored procedure
                List<Voucher> deleteResult = DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, objVoucher, trans, commandType: CommandType.StoredProcedure).ToList<Voucher>();

                // Now create new voucher entries
                objVoucher._Operation = "CREATE";
                objVoucher.CompanyID = SessionContext.CompanyId;
                objVoucher.BranchID = SessionContext.BranchId;
                objVoucher.VoucherID = sk.VoucherId; // Use the existing voucher ID
                objVoucher.VoucherSeriesID = 0;
                objVoucher.VoucherDate = DateTime.Now;
                objVoucher.VoucherType = "PhysicalStock";
                objVoucher.Mode = "";
                objVoucher.ModeID = 0;
                objVoucher.UserDate = DateTime.Now;
                objVoucher.UserName = DataBase.UserName;
                objVoucher.UserID = SessionContext.UserId;
                objVoucher.CancelFlag = false;
                objVoucher.FinYearID = SessionContext.FinYearId;
                objVoucher.IsSyncd = false;

                // Get the absolute value for the voucher entries
                double absAdjustmentValue = Math.Abs(totalAdjustmentValue);
                string adjustmentType = totalQtyDifference >= 0 ? "Stock In" : "Stock Out";

                // Determine the contra ledger details
                int contraGroupId;
                int contraLedgerId;
                string contraLedgerName;

                if (sk.LedgerId > 0)
                {
                    contraGroupId = Convert.ToInt32(AccountGroup.SUNDRY_CREDITORS); // Default fallback group
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Ledger, (SqlConnection)DataConnection, (SqlTransaction)trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@_Operation", "GETBYID");
                            cmd.Parameters.AddWithValue("@LedgerID", sk.LedgerId);
                            object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                contraGroupId = Convert.ToInt32(result);
                            }
                        }
                    }
                    catch { }

                    contraLedgerId = sk.LedgerId;
                    contraLedgerName = sk.LedgerName;
                }
                else
                {
                    contraGroupId = Convert.ToInt32(AccountGroup.PURCHASE_ACCOUNT);
                    contraLedgerId = objLedgerRepository.GetLedgerId(DefaultLedgers.PURCHASE, (int)AccountGroup.PURCHASE_ACCOUNT, SessionContext.BranchId);
                    contraLedgerName = DefaultLedgers.PURCHASE;
                }

                // --- Resolve STOCK IN HAND ledger (Group 18) for Leg 1 ---
                // Correct journal: Dr STOCK IN HAND / Cr Reason (Stock In)
                //                  Dr Reason / Cr STOCK IN HAND  (Stock Out)
                int stkInHandGroupId = Convert.ToInt32(AccountGroup.STOCK_IN_HAND);
                int stkInHandLedgerId = objLedgerRepository.GetLedgerId(DefaultLedgers.BEGINSTOCK, (int)AccountGroup.STOCK_IN_HAND, SessionContext.BranchId);
                string stkInHandLedgerName = DefaultLedgers.BEGINSTOCK;
                if (stkInHandLedgerId == 0)
                {
                    // Fallback: use stored procedure _4GetAccountLedgerDDL to get stock-in-hand ledgers for this branch
                    try
                    {
                        var ddlRequest = new AccountLedgerDDLRequest { BranchId = SessionContext.BranchId, For = "Stock" };
                        var ddlResult = objLedgerRepository.getAccountLedgerDDL(ddlRequest);
                        if (ddlResult != null && ddlResult.List != null && ddlResult.List.Any())
                        {
                            var firstLedger = ddlResult.List.FirstOrDefault();
                            if (firstLedger != null)
                            {
                                stkInHandLedgerId = firstLedger.Id;
                                stkInHandLedgerName = firstLedger.Name;
                            }
                        }
                    }
                    catch { }
                }

                // Correct double-entry: Dr STOCK IN HAND / Cr Reason (Stock In) | Dr Reason / Cr STOCK IN HAND (Stock Out)
                if (totalQtyDifference >= 0) // Stock In (Positive Adjustment)
                {
                    // Entry 1: Debit STOCK IN HAND (Group 18 — increases stock asset)
                    objVoucher.GroupID = stkInHandGroupId;
                    objVoucher.LedgerID = stkInHandLedgerId;
                    objVoucher.LedgerName = stkInHandLedgerName;
                    objVoucher.Debit = absAdjustmentValue;
                    objVoucher.Credit = 0;
                    objVoucher.Narration = "PhysicalStock: #" + Convert.ToString(sk.StockAdjustmentNo) + "| " + adjustmentType + " WORTH:" + Convert.ToString(absAdjustmentValue) + "| REMARKS: " + sk.Comments;
                    objVoucher.SlNo = 1;

                    List<Voucher> ObjSaveDebitVoucher = DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, objVoucher, trans, commandType: CommandType.StoredProcedure).ToList<Voucher>();

                    // Entry 2: Credit Reason/Contra Ledger
                    objVoucher.GroupID = contraGroupId;
                    objVoucher.LedgerID = contraLedgerId;
                    objVoucher.LedgerName = contraLedgerName;
                    objVoucher.Debit = 0;
                    objVoucher.Credit = absAdjustmentValue;
                    objVoucher.Narration = "PhysicalStock: #" + Convert.ToString(sk.StockAdjustmentNo) + "| " + adjustmentType + " WORTH:" + Convert.ToString(absAdjustmentValue) + "| REMARKS: " + sk.Comments;
                    objVoucher.SlNo = 2;

                    List<Voucher> ObjSaveCreditVoucher = DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, objVoucher, trans, commandType: CommandType.StoredProcedure).ToList<Voucher>();
                }
                else // Stock Out (Negative Adjustment)
                {
                    // Entry 1: Credit STOCK IN HAND (Group 18 — decreases stock asset)
                    objVoucher.GroupID = stkInHandGroupId;
                    objVoucher.LedgerID = stkInHandLedgerId;
                    objVoucher.LedgerName = stkInHandLedgerName;
                    objVoucher.Debit = 0;
                    objVoucher.Credit = absAdjustmentValue;
                    objVoucher.Narration = "PhysicalStock: #" + Convert.ToString(sk.StockAdjustmentNo) + "| " + adjustmentType + " WORTH:" + Convert.ToString(absAdjustmentValue) + "| REMARKS: " + sk.Comments;
                    objVoucher.SlNo = 1;

                    List<Voucher> ObjSaveCreditVoucher = DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, objVoucher, trans, commandType: CommandType.StoredProcedure).ToList<Voucher>();

                    // Entry 2: Debit Reason/Contra Ledger
                    objVoucher.GroupID = contraGroupId;
                    objVoucher.LedgerID = contraLedgerId;
                    objVoucher.LedgerName = contraLedgerName;
                    objVoucher.Debit = absAdjustmentValue;
                    objVoucher.Credit = 0;
                    objVoucher.Narration = "PhysicalStock: #" + Convert.ToString(sk.StockAdjustmentNo) + "| " + adjustmentType + " WORTH:" + Convert.ToString(absAdjustmentValue) + "| REMARKS: " + sk.Comments;
                    objVoucher.SlNo = 2;

                    List<Voucher> ObjSaveDebitVoucher = DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, objVoucher, trans, commandType: CommandType.StoredProcedure).ToList<Voucher>();
                }

                if (liststock.Count > 0)
                {
                    trans.Commit();
                    return "success";
                }
                else
                {
                    trans.Rollback();
                    return "Failed: No master record was updated";
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (trans != null)
                    {
                        trans.Rollback();
                    }
                }
                catch (Exception rollbackEx)
                {
                    // Rollback failed
                }

                return $"Failed: {ex.Message}";
            }
            finally
            {
                if (DataConnection != null && DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        /// <summary>
        /// Gets the available units for a specific item
        /// </summary>
        /// <param name="itemId">The ID of the item</param>
        /// <returns>DataTable containing available units for the item</returns>
        public DataTable GetItemUnits(int itemId)
        {
            DataTable dtUnits = new DataTable();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@FinyearId", SessionContext.FinYearId);  // Default value as used elsewhere
                    cmd.Parameters.AddWithValue("@Operation", "ItemUnit");
                    cmd.Parameters.AddWithValue("@ItemId", itemId);
                    cmd.Parameters.AddWithValue("@description", DBNull.Value);
                    cmd.Parameters.AddWithValue("@LedgerId", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Barcode", DBNull.Value);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dtUnits);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return dtUnits;
        }

        /// <summary>
        /// Gets a specific unit for an item
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <param name="unitId">Optional Unit ID (if null, returns default unit)</param>
        /// <returns>Unit name or empty string if not found</returns>
        public string GetItemUnit(int itemId, int? unitId = null)
        {
            string unitName = string.Empty;
            DataTable dtUnits = GetItemUnits(itemId);

            if (dtUnits != null && dtUnits.Rows.Count > 0)
            {
                if (unitId.HasValue)
                {
                    // Try to find the specified unit
                    foreach (DataRow row in dtUnits.Rows)
                    {
                        if (row.Table.Columns.Contains("UnitId") &&
                            row["UnitId"] != DBNull.Value &&
                            Convert.ToInt32(row["UnitId"]) == unitId.Value)
                        {
                            unitName = row["Unit"].ToString();
                            break;
                        }
                    }
                }

                // If we couldn't find the specified unit or no unit was specified, return the first one
                if (string.IsNullOrEmpty(unitName) && dtUnits.Rows.Count > 0)
                {
                    unitName = dtUnits.Rows[0]["Unit"].ToString();
                }
            }

            return unitName;
        }

    }
}
