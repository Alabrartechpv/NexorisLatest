using Dapper;
using ModelClass;
using ModelClass.Master;
using Repository.SettingsRepo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Repository.MasterRepositry
{
    public class ItemMasterRepository : BaseRepostitory
    {
        public string saveItem(Item i, ItemMasterPriceSettings itmprice)
        {
            DataConnection.Open();
            var trans = DataConnection.BeginTransaction();
            try
            {
                List<Item> listitem = DataConnection.Query<Item>(STOREDPROCEDURE.POS_ItemMaster, i, trans,
                    commandType: CommandType.StoredProcedure).ToList<Item>();
                if (listitem.Count > 0)
                {
                    foreach (Item master in listitem)
                    {
                        itmprice.ItemId = master.ItemId;
                    }
                }
                List<ItemMasterPriceSettings> listitemprice = DataConnection.Query<ItemMasterPriceSettings>(STOREDPROCEDURE.POS_ItemMasterPriceSettings, itmprice, trans,
                    commandType: CommandType.StoredProcedure).ToList<ItemMasterPriceSettings>();
                if (listitem.Count > 0)
                {
                    trans.Commit();
                }
            }
            catch (Exception)
            {
                trans.Rollback();
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
            return "success";
        }

        public string savePriceSettings(ItemMasterPriceSettings iprice)
        {
            DataConnection.Open();
            var trans = DataConnection.BeginTransaction();
            try
            {
                List<ItemMasterPriceSettings> listitem = DataConnection.Query<ItemMasterPriceSettings>(STOREDPROCEDURE.POS_ItemMasterPriceSettings, iprice, trans,
                    commandType: CommandType.StoredProcedure).ToList<ItemMasterPriceSettings>();
                if (listitem.Count > 0)
                {
                    trans.Commit();
                }
            }
            catch (Exception)
            {
                trans.Rollback();
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
            return "success";
        }

        public ItemGet GetByIdItem(int selectedId)
        {
            ItemGet item = new ItemGet();
            ItemMasterPriceSettings itemp = new ItemMasterPriceSettings();
            VendorDetailsForItemmaster VendorItemForGrid = new VendorDetailsForItemmaster();
            DataConnection.Open();

            try
            {
                // Use the required stored procedure name to ensure HSNCode and other fields are returned
                using (SqlCommand cmd = new SqlCommand("_POS_ItemMaster", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ItemId", selectedId);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@ItemNo", 0);
                    cmd.Parameters.AddWithValue("@Description", "");
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@ItemTypeId", 0);
                    cmd.Parameters.AddWithValue("@VendorId", 0);
                    cmd.Parameters.AddWithValue("@BrandId", 0);
                    cmd.Parameters.AddWithValue("@GroupId", 0);
                    cmd.Parameters.AddWithValue("@CategoryId", 0);
                    cmd.Parameters.AddWithValue("@BaseUnitId", 0);
                    cmd.Parameters.AddWithValue("@ForCustomerType", "");
                    cmd.Parameters.AddWithValue("@NameInLocalLanguage", "");
                    cmd.Parameters.AddWithValue("@HSNCode", "");
                    //cmd.Parameters.AddWithValue("@Stock","");
                    cmd.Parameters.AddWithValue("@_Operation", "GETITEM");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            item = ds.Tables[0].Rows[0].ToNullableObject<ItemGet>();

                            // Fetch fields directly from ItemMaster so the UI keeps working
                            // even if GETITEM does not return every column we need.
                            try
                            {
                                using (SqlCommand hsnCmd = new SqlCommand(
                                    "SELECT Barcode, HSNCode, Order_Cycle_Days, Box_Quantity, Is_Perishable FROM ItemMaster WHERE ItemId = @ItemId",
                                    (SqlConnection)DataConnection))
                                {
                                    hsnCmd.Parameters.AddWithValue("@ItemId", selectedId);
                                    using (SqlDataReader reader = hsnCmd.ExecuteReader())
                                    {
                                        if (reader.Read())
                                        {
                                            item.Barcode = reader["Barcode"] == DBNull.Value ? item.Barcode : Convert.ToString(reader["Barcode"]);
                                            item.HSNCode = reader["HSNCode"] == DBNull.Value ? item.HSNCode : Convert.ToString(reader["HSNCode"]);
                                            item.Order_Cycle_Days = reader["Order_Cycle_Days"] == DBNull.Value ? item.Order_Cycle_Days : Convert.ToInt32(reader["Order_Cycle_Days"]);
                                            item.Box_Quantity = reader["Box_Quantity"] == DBNull.Value ? item.Box_Quantity : Convert.ToInt32(reader["Box_Quantity"]);
                                            item.Is_Perishable = reader["Is_Perishable"] != DBNull.Value && Convert.ToBoolean(reader["Is_Perishable"]);
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                        if ((ds != null) && (ds.Tables.Count > 1) && (ds.Tables[1] != null) && (ds.Tables[1].Rows.Count > 0))
                        {
                            item.List = ds.Tables[1].ToArrayOfObject<ItemMasterPriceSettings>();
                            System.Diagnostics.Debug.WriteLine($"GetByIdItem: Loaded {item.List?.Length ?? 0} price settings for ItemId {selectedId}");
                        }
                        if ((ds != null) && (ds.Tables.Count > 2) && (ds.Tables[2] != null) && (ds.Tables[2].Rows.Count > 0))
                        {
                            item.ListVendor = ds.Tables[2].ToArrayOfObject<VendorDetailsForItemmaster>();
                        }

                        // Fetch Alternative Barcodes natively via Stored Procedure (4th result set)
                        try
                        {
                            if ((ds != null) && (ds.Tables.Count > 3) && (ds.Tables[3] != null) && (ds.Tables[3].Rows.Count > 0))
                            {
                                item.ListAlternativeBarcodes = ds.Tables[3].ToArrayOfObject<ItemAlternativeBarcode>();
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to load alternative barcodes: {ex.Message}");
                        }
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return item;
        }

        public ItemMasterPriceSettings GetByIdItemPrice(int selectedId)
        {
            ItemMasterPriceSettings itemp = new ItemMasterPriceSettings();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemMasterPriceSettings, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ItemId", selectedId);
                    cmd.Parameters.AddWithValue("@Cost", "");
                    cmd.Parameters.AddWithValue("@OrderedStock", "");
                    cmd.Parameters.AddWithValue("@StockValue", "");
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            itemp = ds.Tables[0].Rows[0].ToNullableObject<ItemMasterPriceSettings>();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return itemp;
        }

        // Fetch first available non-null photo for an item using the price settings stored procedure
        public byte[] GetItemPhoto(int itemId)
        {
            byte[] photoBytes = null;
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemMasterPriceSettings, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ItemId", itemId);
                    // The stored procedure signature expects these (even if unused for GETBYID)
                    cmd.Parameters.AddWithValue("@Cost", "");
                    cmd.Parameters.AddWithValue("@OrderedStock", "");
                    cmd.Parameters.AddWithValue("@StockValue", "");
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                if (ds.Tables[0].Columns.Contains("Photo") && row["Photo"] != DBNull.Value)
                                {
                                    photoBytes = row["Photo"] as byte[];
                                    if (photoBytes != null && photoBytes.Length > 0) break;
                                }
                                if (ds.Tables[0].Columns.Contains("PhotoByteArray") && row["PhotoByteArray"] != DBNull.Value)
                                {
                                    photoBytes = row["PhotoByteArray"] as byte[];
                                    if (photoBytes != null && photoBytes.Length > 0) break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return photoBytes;
        }

        public Item UpdateItem(Item itmm)
        {
            Item objitem = new Item();

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ItemId", itmm.ItemId);
                    cmd.Parameters.AddWithValue("@Description", itmm.Description);
                    cmd.Parameters.AddWithValue("@NameInLocalLanguage", itmm.NameInLocalLanguage);
                    cmd.Parameters.AddWithValue("@ItemTypeId", itmm.ItemTypeId);
                    cmd.Parameters.AddWithValue("@ForCustomerType", itmm.ForCustomerType);
                    cmd.Parameters.AddWithValue("@BaseUnitId", itmm.BaseUnitId);
                    cmd.Parameters.AddWithValue("@GroupId", itmm.GroupId);
                    cmd.Parameters.AddWithValue("@CategoryId", itmm.CategoryId);
                    cmd.Parameters.AddWithValue("@BrandId", itmm.BrandId);
                    //cmd.Parameters.AddWithValue("@GroupId", itmm.GroupId);
                    //cmd.Parameters.AddWithValue("@CategoryId", itmm.CategoryId);
                    //cmd.Parameters.AddWithValue("@BrandId", itmm.BrandId);
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            objitem = ds.Tables[0].Rows[0].ToNullableObject<Item>();

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
            return objitem;
        }


        public ItemDialogDDLGrid getItemGetAll(string search)
        {
            ItemDialogDDLGrid ItemGrid = new ItemDialogDDLGrid();
            int branchId = SessionContext.BranchId > 0 ? SessionContext.BranchId : ParseIntValue(DataBase.BranchId);
            int companyId = SessionContext.CompanyId > 0 ? SessionContext.CompanyId : ParseIntValue(DataBase.CompanyId);
            int finYearId = SessionContext.FinYearId > 0 ? SessionContext.FinYearId : ParseIntValue(DataBase.FinyearId);
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@FinyearId", finYearId);
                    cmd.Parameters.AddWithValue("@Barcode", search);
                    cmd.Parameters.AddWithValue("@Operation", "GETITEM");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            ItemGrid.List = ds.Tables[0].ToListOfObject<ItemDialogDDL>();
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
            return ItemGrid;
        }

        private static int ParseIntValue(string value)
        {
            int parsed;
            return int.TryParse(value, out parsed) ? parsed : 0;
        }



        #region Clean Save and Update Methods

        /// <summary>
        /// Saves a new item with its price settings
        /// </summary>
        public string SaveItemMaster(Item item, ItemMasterPriceSettings priceSettings, DataGridView dgv_Uom, DataGridView dgv_Price, DataGridView dgv_AlternativeBarcodes = null)
        {
            DataConnection.Open();
            var transaction = DataConnection.BeginTransaction();

            try
            {
                // Set operation for item creation
                item._Operation = "CREATE";
                item.CompanyId = Convert.ToInt32(DataBase.CompanyId);
                item.BranchId = Convert.ToInt32(DataBase.BranchId);
                item.FinYearId = SessionContext.FinYearId;

                // Save item master
                List<Item> savedItems = DataConnection.Query<Item>(
                    STOREDPROCEDURE.POS_ItemMaster,
                    item,
                    transaction,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                if (savedItems.Count == 0)
                {
                    throw new Exception("Failed to save item master");
                }

                // Get the newly created item ID
                int newItemId = savedItems[0].ItemId;
                if (newItemId <= 0)
                {
                    throw new Exception("Failed to create item master. Description may already exist; please use a unique item name for cloned items.");
                }
                item.ItemId = newItemId; // FIX: Update the item object with the new ID so the UI can use it
                PersistSmartReorderFields(item, transaction);

                // Save price settings for each unit
                for (int i = 0; i < dgv_Uom.Rows.Count; i++)
                {
                    var uomRow = dgv_Uom.Rows[i];
                    if (uomRow.IsNewRow) continue;

                    var unitValue = uomRow.Cells["Unit"]?.Value?.ToString();
                    if (string.IsNullOrWhiteSpace(unitValue)) continue;

                    // Create new price settings object for this unit
                    var unitPriceSettings = new ItemMasterPriceSettings
                    {
                        _Operation = "CREATE",
                        CompanyId = Convert.ToInt32(DataBase.CompanyId),
                        BranchId = Convert.ToInt32(DataBase.BranchId),
                        FinYearId = SessionContext.FinYearId,
                        ItemId = newItemId,
                        UnitId = SafeConvertToInt32(uomRow.Cells["UnitId"]?.Value, 0),
                        Unit = unitValue,
                        Packing = SafeParseDouble(uomRow.Cells["Packing"]?.Value),
                        BarCode = item.Barcode ?? "",  // Save barcode from txt_barcode (item.Barcode) to PriceSettings
                        AliasBarcode = uomRow.Cells["AliasBarcode"]?.Value?.ToString() ?? "",  // Save alias barcode from grid
                        ReOrder = SafeParseDouble(uomRow.Cells["Reorder"]?.Value),
                        OpnStk = SafeParseDouble(uomRow.Cells["OpnStk"]?.Value),
                        Cost = 0,
                        MarginAmt = 0,
                        MarginPer = 0,
                        MRP = 0,
                        RetailPrice = 0,
                        WholeSalePrice = 0,
                        CreditPrice = 0,
                        CardPrice = 0,
                        TaxAmt = 0,
                        TaxPer = priceSettings?.TaxPer ?? 0,
                        TaxType = priceSettings?.TaxType ?? string.Empty,
                        Costing = "AVERAGE",
                        Stock = 0,
                        StockValue = 0,
                        OrderedStock = 0,
                        Photo = null,
                        PhotoByteArray = priceSettings?.PhotoByteArray,
                        BranchName = DataBase.Branch
                    };

                    // Mark base unit explicitly
                    try
                    {
                        bool isBase = false;
                        if (unitPriceSettings.Packing == 1d)
                        {
                            isBase = true;
                        }
                        else
                        {
                            var unitText = (unitPriceSettings.Unit ?? string.Empty).Trim().ToLowerInvariant();
                            // Also treat textual base like "1 UNIT" or "UNIT 1" as base
                            if ((unitText.Contains("1") && unitText.Contains("unit")) || unitText == "1 unit")
                            {
                                isBase = true;
                            }
                        }
                        unitPriceSettings.IsBaseUnit = isBase ? "Y" : "N";
                    }
                    catch { unitPriceSettings.IsBaseUnit = "N"; }

                    // Find corresponding price row
                    var priceRow = FindPriceRowByUnit(dgv_Price, unitValue);
                    System.Diagnostics.Debug.WriteLine($"SaveItemMaster: Looking for unit '{unitValue}' in dgv_Price (rows={dgv_Price.Rows.Count})");
                    if (priceRow != null)
                    {
                        var costValue = priceRow.Cells["Cost"]?.Value;
                        System.Diagnostics.Debug.WriteLine($"SaveItemMaster: Found priceRow for '{unitValue}', Cost cell value = {costValue} (type={costValue?.GetType()?.Name})");
                        unitPriceSettings.Cost = SafeParseDouble(priceRow.Cells["Cost"]?.Value);
                        System.Diagnostics.Debug.WriteLine($"SaveItemMaster: After SafeParseDouble, unitPriceSettings.Cost = {unitPriceSettings.Cost}");
                        unitPriceSettings.MarginAmt = SafeParseDouble(priceRow.Cells["MarginAmt"]?.Value);
                        unitPriceSettings.MarginPer = SafeParseDouble(priceRow.Cells["MarginPer"]?.Value);
                        unitPriceSettings.MRP = SafeParseDouble(priceRow.Cells["MRP"]?.Value);
                        unitPriceSettings.RetailPrice = SafeParseDouble(priceRow.Cells["RetailPrice"]?.Value);
                        unitPriceSettings.WholeSalePrice = SafeParseDouble(priceRow.Cells["WholeSalePrice"]?.Value);
                        unitPriceSettings.CreditPrice = SafeParseDouble(priceRow.Cells["CreditPrice"]?.Value);
                        unitPriceSettings.CardPrice = SafeParseDouble(priceRow.Cells["CardPrice"]?.Value);
                        unitPriceSettings.TaxAmt = SafeParseDouble(priceRow.Cells["TaxAmt"]?.Value);
                        if (priceRow.DataGridView.Columns.Contains("StaffPrice"))
                            unitPriceSettings.StaffPrice = SafeParseDouble(priceRow.Cells["StaffPrice"]?.Value);
                        if (priceRow.DataGridView.Columns.Contains("MinPrice"))
                            unitPriceSettings.MinPrice = SafeParseDouble(priceRow.Cells["MinPrice"]?.Value);
                        // Prefer per-row TaxPer if present
                        if (priceRow.DataGridView != null && priceRow.DataGridView.Columns.Contains("TaxPer"))
                        {
                            var taxPerValue = priceRow.Cells["TaxPer"]?.Value;
                            if (taxPerValue != null)
                                unitPriceSettings.TaxPer = SafeParseDouble(taxPerValue);
                        }
                    }

                    // Set markdown values from the main price settings
                    unitPriceSettings.MDRetailPrice = priceSettings?.MDRetailPrice ?? 0;
                    unitPriceSettings.MDWalkinPrice = priceSettings?.MDWalkinPrice ?? 0;
                    unitPriceSettings.MDCreditPrice = priceSettings?.MDCreditPrice ?? 0;
                    unitPriceSettings.MDMrpPrice = priceSettings?.MDMrpPrice ?? 0;
                    unitPriceSettings.MDCardPrice = priceSettings?.MDCardPrice ?? 0;
                    unitPriceSettings.MDStaffPrice = priceSettings?.MDStaffPrice ?? 0;
                    unitPriceSettings.MDMinPrice = priceSettings?.MDMinPrice ?? 0;


                    // Save price settings (only pass parameters that exist in the procedure)
                    var dp = BuildFilteredParameters(STOREDPROCEDURE.POS_ItemMasterPriceSettings, unitPriceSettings, transaction);
                    List<ItemMasterPriceSettings> savedPriceSettings = DataConnection.Query<ItemMasterPriceSettings>(
                        STOREDPROCEDURE.POS_ItemMasterPriceSettings,
                        dp,
                        transaction,
                        commandType: CommandType.StoredProcedure
                    ).ToList();

                    if (savedPriceSettings.Count == 0)
                    {
                        throw new Exception($"Failed to save price settings for unit: {unitValue}");
                    }
                }

                // --- SAVE ALTERNATIVE BARCODES ---
                if (dgv_AlternativeBarcodes != null)
                {
                    try
                    {
                        foreach (DataGridViewRow row in dgv_AlternativeBarcodes.Rows)
                        {
                            if (row.IsNewRow) continue;

                            var bcValue = row.Cells["Barcode"]?.Value?.ToString()?.Trim();
                            if (!string.IsNullOrWhiteSpace(bcValue))
                            {
                                DataConnection.Execute(
                                    "_POS_ItemAlternativeBarcode",
                                    new { ItemId = newItemId, Barcode = bcValue, _Operation = "CREATE" },
                                    transaction,
                                    commandType: CommandType.StoredProcedure
                                );
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to save alternative barcodes: {ex.Message}");
                    }
                }
                // ------------------------------------

                transaction.Commit();
                return $"Success|{newItemId}";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return $"Failed: {ex.Message}";
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        /// <summary>
        /// Updates an existing item and its price settings
        /// </summary>
        public string UpdateItemMaster(Item item, ItemMasterPriceSettings priceSettings, DataGridView dgv_Uom, DataGridView dgv_Price, DataGridView dgv_AlternativeBarcodes = null)
        {
            DataConnection.Open();
            var transaction = DataConnection.BeginTransaction();

            try
            {
                // Set operation for item update
                item._Operation = "UPDATE";
                item.CompanyId = Convert.ToInt32(DataBase.CompanyId);
                item.BranchId = Convert.ToInt32(DataBase.BranchId);
                item.FinYearId = SessionContext.FinYearId;

                // Update item master
                List<Item> updatedItems = DataConnection.Query<Item>(
                    STOREDPROCEDURE.POS_ItemMaster,
                    item,
                    transaction,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                if (updatedItems.Count == 0)
                {
                    throw new Exception("Failed to update item master");
                }
                PersistSmartReorderFields(item, transaction);

                // Get existing stock values BEFORE deleting to preserve them
                // Use the GetItemPriceSettings method or query the existing price settings
                var existingPriceSettingsParam = new ItemMasterPriceSettings
                {
                    _Operation = "GETBYID",
                    ItemId = item.ItemId,
                    CompanyId = Convert.ToInt32(DataBase.CompanyId),
                    BranchId = Convert.ToInt32(DataBase.BranchId),
                    FinYearId = SessionContext.FinYearId
                };

                var existingPriceList = DataConnection.Query<ItemMasterPriceSettings>(
                    STOREDPROCEDURE.POS_ItemMasterPriceSettings,
                    existingPriceSettingsParam,
                    transaction,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                // Build dictionary from existing prices, handling potential duplicates by taking the first occurrence
                var existingStocks = new Dictionary<int, dynamic>();
                foreach (var price in existingPriceList)
                {
                    if (!existingStocks.ContainsKey(price.UnitId))
                    {
                        existingStocks[price.UnitId] = new
                        {
                            Stock = price.Stock,
                            StockValue = price.StockValue,
                            OrderedStock = price.OrderedStock,
                            OpeningCost = price.OpeningCost,
                            OpnValue = price.OpnValue,
                            OpnDate = price.OpnDate
                        };
                    }
                }

                // Delete existing price settings for this item
                var deletePriceSettings = new ItemMasterPriceSettings
                {
                    _Operation = "DELETE",
                    ItemId = item.ItemId,
                    CompanyId = Convert.ToInt32(DataBase.CompanyId),
                    BranchId = Convert.ToInt32(DataBase.BranchId),
                    FinYearId = SessionContext.FinYearId
                };

                var deleteParams = BuildFilteredParameters(STOREDPROCEDURE.POS_ItemMasterPriceSettings, deletePriceSettings, transaction);
                List<ItemMasterPriceSettings> deletedPriceSettings = DataConnection.Query<ItemMasterPriceSettings>(
                    STOREDPROCEDURE.POS_ItemMasterPriceSettings,
                    deleteParams,
                    transaction,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                // Create new price settings for each unit
                for (int i = 0; i < dgv_Uom.Rows.Count; i++)
                {
                    var uomRow = dgv_Uom.Rows[i];
                    if (uomRow.IsNewRow) continue;

                    var unitValue = uomRow.Cells["Unit"]?.Value?.ToString();
                    if (string.IsNullOrWhiteSpace(unitValue)) continue;

                    // Create new price settings object for this unit
                    // Use safe parsing to handle string values from DataGridView
                    int currentUnitId = 0;
                    var unitIdValue = uomRow.Cells["UnitId"]?.Value;
                    if (unitIdValue != null)
                    {
                        if (unitIdValue is int)
                            currentUnitId = (int)unitIdValue;
                        else if (!int.TryParse(unitIdValue.ToString(), out currentUnitId))
                            currentUnitId = 0;
                    }

                    // Preserve existing stock values if they exist
                    double existingStock = 0;
                    double existingStockValue = 0;
                    double existingOrderedStock = 0;
                    double existingOpeningCost = 0;
                    double existingOpnValue = 0;
                    DateTime? existingOpnDate = null;

                    if (existingStocks.ContainsKey(currentUnitId))
                    {
                        existingStock = existingStocks[currentUnitId].Stock;
                        existingStockValue = existingStocks[currentUnitId].StockValue;
                        existingOrderedStock = existingStocks[currentUnitId].OrderedStock;
                        existingOpeningCost = existingStocks[currentUnitId].OpeningCost;
                        existingOpnValue = existingStocks[currentUnitId].OpnValue;
                        existingOpnDate = existingStocks[currentUnitId].OpnDate;
                    }

                    var unitPriceSettings = new ItemMasterPriceSettings
                    {
                        _Operation = "CREATE",
                        CompanyId = Convert.ToInt32(DataBase.CompanyId),
                        BranchId = Convert.ToInt32(DataBase.BranchId),
                        FinYearId = SessionContext.FinYearId,
                        ItemId = item.ItemId,
                        UnitId = currentUnitId,
                        Unit = unitValue,
                        Packing = SafeParseDouble(uomRow.Cells["Packing"]?.Value),
                        BarCode = item.Barcode ?? "",  // Save barcode from txt_barcode (item.Barcode) to PriceSettings
                        AliasBarcode = uomRow.Cells["AliasBarcode"]?.Value?.ToString() ?? "",  // Save alias barcode from grid
                        ReOrder = SafeParseDouble(uomRow.Cells["Reorder"]?.Value),
                        OpnStk = SafeParseDouble(uomRow.Cells["OpnStk"]?.Value),
                        OpeningCost = existingOpeningCost,
                        OpnValue = existingOpnValue,
                        OpnDate = existingOpnDate,
                        Cost = 0,
                        MarginAmt = 0,
                        MarginPer = 0,
                        MRP = 0,
                        RetailPrice = 0,
                        WholeSalePrice = 0,
                        CreditPrice = 0,
                        CardPrice = 0,
                        TaxAmt = 0,
                        TaxPer = priceSettings?.TaxPer ?? 0,
                        TaxType = priceSettings?.TaxType ?? string.Empty,
                        Costing = "AVERAGE",
                        Stock = existingStock,          // Preserve existing stock
                        StockValue = existingStockValue,  // Preserve existing stock value
                        OrderedStock = existingOrderedStock, // Preserve existing ordered stock
                        Photo = null,
                        PhotoByteArray = priceSettings?.PhotoByteArray,
                        BranchName = DataBase.Branch
                    };

                    // Mark base unit explicitly
                    try
                    {
                        bool isBase = false;
                        if (unitPriceSettings.Packing == 1d)
                        {
                            isBase = true;
                        }
                        else
                        {
                            var unitText = (unitPriceSettings.Unit ?? string.Empty).Trim().ToLowerInvariant();
                            if ((unitText.Contains("1") && unitText.Contains("unit")) || unitText == "1 unit")
                            {
                                isBase = true;
                            }
                        }
                        unitPriceSettings.IsBaseUnit = isBase ? "Y" : "N";
                    }
                    catch { unitPriceSettings.IsBaseUnit = "N"; }

                    // Find corresponding price row
                    var priceRow = FindPriceRowByUnit(dgv_Price, unitValue);
                    if (priceRow != null)
                    {
                        unitPriceSettings.Cost = SafeParseDouble(priceRow.Cells["Cost"]?.Value);
                        unitPriceSettings.MarginAmt = SafeParseDouble(priceRow.Cells["MarginAmt"]?.Value);
                        unitPriceSettings.MarginPer = SafeParseDouble(priceRow.Cells["MarginPer"]?.Value);
                        unitPriceSettings.MRP = SafeParseDouble(priceRow.Cells["MRP"]?.Value);
                        unitPriceSettings.RetailPrice = SafeParseDouble(priceRow.Cells["RetailPrice"]?.Value);
                        unitPriceSettings.WholeSalePrice = SafeParseDouble(priceRow.Cells["WholeSalePrice"]?.Value);
                        unitPriceSettings.CreditPrice = SafeParseDouble(priceRow.Cells["CreditPrice"]?.Value);
                        unitPriceSettings.CardPrice = SafeParseDouble(priceRow.Cells["CardPrice"]?.Value);
                        unitPriceSettings.TaxAmt = SafeParseDouble(priceRow.Cells["TaxAmt"]?.Value);
                        if (priceRow.DataGridView != null && priceRow.DataGridView.Columns.Contains("TaxPer"))
                        {
                            var taxPerValue = priceRow.Cells["TaxPer"]?.Value;
                            if (taxPerValue != null)
                                unitPriceSettings.TaxPer = SafeParseDouble(taxPerValue);
                        }
                        if (priceRow.DataGridView.Columns.Contains("StaffPrice"))
                            unitPriceSettings.StaffPrice = SafeParseDouble(priceRow.Cells["StaffPrice"]?.Value);
                        if (priceRow.DataGridView.Columns.Contains("MinPrice"))
                            unitPriceSettings.MinPrice = SafeParseDouble(priceRow.Cells["MinPrice"]?.Value);
                    }

                    // Set markdown values from the main price settings
                    unitPriceSettings.MDRetailPrice = priceSettings?.MDRetailPrice ?? 0;
                    unitPriceSettings.MDWalkinPrice = priceSettings?.MDWalkinPrice ?? 0;
                    unitPriceSettings.MDCreditPrice = priceSettings?.MDCreditPrice ?? 0;
                    unitPriceSettings.MDMrpPrice = priceSettings?.MDMrpPrice ?? 0;
                    unitPriceSettings.MDCardPrice = priceSettings?.MDCardPrice ?? 0;
                    unitPriceSettings.MDStaffPrice = priceSettings?.MDStaffPrice ?? 0;
                    unitPriceSettings.MDMinPrice = priceSettings?.MDMinPrice ?? 0;

                    // Save price settings (only pass parameters that exist in the procedure)
                    var createParams = BuildFilteredParameters(STOREDPROCEDURE.POS_ItemMasterPriceSettings, unitPriceSettings, transaction);
                    List<ItemMasterPriceSettings> savedPriceSettings = DataConnection.Query<ItemMasterPriceSettings>(
                        STOREDPROCEDURE.POS_ItemMasterPriceSettings,
                        createParams,
                        transaction,
                        commandType: CommandType.StoredProcedure
                    ).ToList();

                    if (savedPriceSettings.Count == 0)
                    {
                        throw new Exception($"Failed to save price settings for unit: {unitValue}");
                    }
                }

                // --- UPDATE ALTERNATIVE BARCODES ---
                try
                {
                    // Always clear existing alternative barcodes for this item first using SP
                    DataConnection.Execute(
                        "_POS_ItemAlternativeBarcode",
                        new { ItemId = item.ItemId, Barcode = "", _Operation = "DELETE" },
                        transaction,
                        commandType: CommandType.StoredProcedure
                    );

                    if (dgv_AlternativeBarcodes != null)
                    {
                        foreach (DataGridViewRow row in dgv_AlternativeBarcodes.Rows)
                        {
                            if (row.IsNewRow) continue;

                            var bcValue = row.Cells["Barcode"]?.Value?.ToString()?.Trim();
                            if (!string.IsNullOrWhiteSpace(bcValue))
                            {
                                DataConnection.Execute(
                                    "_POS_ItemAlternativeBarcode",
                                    new { ItemId = item.ItemId, Barcode = bcValue, _Operation = "CREATE" },
                                    transaction,
                                    commandType: CommandType.StoredProcedure
                                );
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to update alternative barcodes: {ex.Message}");
                }
                // ------------------------------------

                transaction.Commit();
                return $"Success|{item.ItemId}";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return $"Failed: {ex.Message}";
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        /// <summary>
        /// Helper method to find price row by unit
        /// </summary>
        private DataGridViewRow FindPriceRowByUnit(DataGridView dgv_Price, string unitValue)
        {
            foreach (DataGridViewRow row in dgv_Price.Rows)
            {
                if (row.IsNewRow) continue;

                var rowUnit = row.Cells["Unit"]?.Value?.ToString();
                if (string.Equals(rowUnit, unitValue, StringComparison.OrdinalIgnoreCase))
                {
                    return row;
                }
            }
            return null;
        }

        /// <summary>
        /// Safely parses a value to double, handling string, numeric, and null values
        /// </summary>
        private double SafeParseDouble(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0.0;

            if (value is double)
                return (double)value;

            if (value is float)
                return (double)(float)value;

            if (value is int)
                return (double)(int)value;

            if (value is decimal)
                return (double)(decimal)value;

            string stringValue = value.ToString().Trim();
            if (string.IsNullOrWhiteSpace(stringValue))
                return 0.0;

            double result;
            if (double.TryParse(stringValue, out result))
                return result;

            return 0.0;
        }

        #endregion

        /// <summary>
        /// Build Dapper DynamicParameters limited to the parameters that the stored procedure actually accepts.
        /// Prevents "too many arguments specified" errors when POCO has extra properties.
        /// </summary>
        private DynamicParameters BuildFilteredParameters(string storedProcName, ItemMasterPriceSettings src, IDbTransaction transaction)
        {
            var dyn = new DynamicParameters();
            try
            {
                using (var cmd = new SqlCommand(storedProcName, (SqlConnection)DataConnection, (SqlTransaction)transaction))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlCommandBuilder.DeriveParameters(cmd);
                    foreach (SqlParameter sqlParam in cmd.Parameters)
                    {
                        if (sqlParam.Direction == ParameterDirection.ReturnValue) continue;
                        string pName = sqlParam.ParameterName?.TrimStart('@');
                        if (string.IsNullOrEmpty(pName)) continue;

                        // Try case-insensitive property match on src
                        var prop = typeof(ItemMasterPriceSettings).GetProperties()
                            .FirstOrDefault(pi => string.Equals(pi.Name, pName, StringComparison.OrdinalIgnoreCase));

                        object val = null;
                        if (prop != null)
                        {
                            val = prop.GetValue(src, null);
                        }
                        else
                        {
                            // Common alternate names mapping if needed
                            if (string.Equals(pName, "BranchID", StringComparison.OrdinalIgnoreCase))
                                val = src.BranchId;
                            else if (string.Equals(pName, "CompanyID", StringComparison.OrdinalIgnoreCase))
                                val = src.CompanyId;
                            else if (string.Equals(pName, "FinYearID", StringComparison.OrdinalIgnoreCase))
                                val = src.FinYearId;
                            else if (string.Equals(pName, "Barcode", StringComparison.OrdinalIgnoreCase))
                                val = src.BarCode;
                            else if (string.Equals(pName, "PhotoByteArray", StringComparison.OrdinalIgnoreCase))
                                val = src.PhotoByteArray;
                            else if (string.Equals(pName, "Photo", StringComparison.OrdinalIgnoreCase))
                                val = src.Photo;
                            else if (string.Equals(pName, "Operation", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(pName, "_Operation", StringComparison.OrdinalIgnoreCase))
                                val = src._Operation;
                        }

                        // Ensure BranchName is not sent as DBNull (some SPs reject DBNull for NVARCHAR)
                        if (string.Equals(pName, "BranchName", StringComparison.OrdinalIgnoreCase))
                        {
                            var s = val as string;
                            if (string.IsNullOrWhiteSpace(s))
                            {
                                try { val = Convert.ToString(DataBase.Branch) ?? string.Empty; }
                                catch { val = string.Empty; }
                            }
                        }

                        // Ensure Unit is not sent as DBNull
                        if (string.Equals(pName, "Unit", StringComparison.OrdinalIgnoreCase))
                        {
                            var s = val as string;
                            if (string.IsNullOrWhiteSpace(s))
                            {
                                val = string.Empty;
                            }
                        }

                        // Ensure BarCode is not sent as DBNull
                        if (string.Equals(pName, "BarCode", StringComparison.OrdinalIgnoreCase))
                        {
                            var s = val as string;
                            if (string.IsNullOrWhiteSpace(s))
                            {
                                val = string.Empty;
                            }
                        }

                        // Ensure TaxType is not sent as DBNull
                        if (string.Equals(pName, "TaxType", StringComparison.OrdinalIgnoreCase))
                        {
                            var s = val as string;
                            if (string.IsNullOrWhiteSpace(s))
                            {
                                val = string.Empty;
                            }
                        }

                        // Ensure IsBaseUnit always has a value if the SP expects it
                        if (string.Equals(pName, "IsBaseUnit", StringComparison.OrdinalIgnoreCase))
                        {
                            var s = Convert.ToString(val);
                            if (string.IsNullOrWhiteSpace(s)) val = "N";
                        }

                        // Ensure Costing always has a value if the SP expects it
                        if (string.Equals(pName, "Costing", StringComparison.OrdinalIgnoreCase))
                        {
                            var s = Convert.ToString(val);
                            if (string.IsNullOrWhiteSpace(s)) val = "AVERAGE";
                        }

                        // Avoid passing DBNull for blob parameters (some SPs balk). If no image, skip these params.
                        if ((string.Equals(pName, "Photo", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(pName, "PhotoByteArray", StringComparison.OrdinalIgnoreCase)))
                        {
                            var bytes = val as byte[];
                            if (bytes == null || bytes.Length == 0)
                                continue; // do not send photo params when empty
                        }

                        // If the SP param is a string type and value is null/DBNull, coerce to empty string
                        if (val == null || val == DBNull.Value)
                        {
                            if (sqlParam.SqlDbType == SqlDbType.VarChar ||
                                sqlParam.SqlDbType == SqlDbType.NVarChar ||
                                sqlParam.SqlDbType == SqlDbType.Char ||
                                sqlParam.SqlDbType == SqlDbType.NChar ||
                                sqlParam.SqlDbType == SqlDbType.Text ||
                                sqlParam.SqlDbType == SqlDbType.NText)
                            {
                                val = string.Empty;
                            }
                            else
                            {
                                val = null;
                            }
                        }

                        dyn.Add("@" + pName, val);
                    }
                }
            }
            catch
            {
                // Fallback: add a minimal safe set if DeriveParameters fails
                dyn.Add("@ItemId", src.ItemId);
                dyn.Add("@CompanyId", src.CompanyId);
                dyn.Add("@BranchId", src.BranchId);
                if (!string.IsNullOrWhiteSpace(src.BranchName)) dyn.Add("@BranchName", src.BranchName);
                dyn.Add("@FinYearId", src.FinYearId);
                dyn.Add("@UnitId", src.UnitId);
                dyn.Add("@Unit", src.Unit);
                dyn.Add("@Packing", src.Packing);
                dyn.Add("@BarCode", src.BarCode);
                dyn.Add("@ReOrder", src.ReOrder);
                dyn.Add("@OpnStk", src.OpnStk);
                dyn.Add("@Cost", src.Cost);
                dyn.Add("@MarginAmt", src.MarginAmt);
                dyn.Add("@MarginPer", src.MarginPer);
                dyn.Add("@MRP", src.MRP);
                dyn.Add("@RetailPrice", src.RetailPrice);
                dyn.Add("@WholeSalePrice", src.WholeSalePrice);
                dyn.Add("@CreditPrice", src.CreditPrice);
                dyn.Add("@CardPrice", src.CardPrice);
                dyn.Add("@StaffPrice", src.StaffPrice);
                dyn.Add("@MinPrice", src.MinPrice);
                // Markdown fields (if SP doesn't accept, DeriveParameters path would have been used)
                dyn.Add("@MDRetailPrice", src.MDRetailPrice);
                dyn.Add("@MDWalkinPrice", src.MDWalkinPrice);
                dyn.Add("@MDCreditPrice", src.MDCreditPrice);
                dyn.Add("@MDMrpPricen", src.MDMrpPrice);
                dyn.Add("@MDCardPrice", src.MDCreditPrice);
                dyn.Add("@MDStaffPrice", src.MDStaffPrice);
                dyn.Add("@MDMinPrice", src.MDMinPrice);
                dyn.Add("@StaffPrice", src.StaffPrice);
                dyn.Add("@MinPrice", src.MinPrice);
                dyn.Add("@TaxPer", src.TaxPer);
                dyn.Add("@TaxAmt", src.TaxAmt);
                dyn.Add("@TaxType", src.TaxType);
                dyn.Add("@IsBaseUnit", (object)(string.IsNullOrWhiteSpace(src.IsBaseUnit) ? "N" : src.IsBaseUnit));
                dyn.Add("@Costing", (object)(string.IsNullOrWhiteSpace(src.Costing) ? "AVERAGE" : src.Costing));
                if (src.Photo != null && src.Photo.Length > 0) dyn.Add("@Photo", src.Photo);
                if (src.PhotoByteArray != null && src.PhotoByteArray.Length > 0) dyn.Add("@PhotoByteArray", src.PhotoByteArray);
                dyn.Add("@_Operation", src._Operation);
            }

            return dyn;
        }

        /// <summary>
        /// Persists smart reorder fields explicitly because the item master stored procedure
        /// does not reliably save these columns during create flows.
        /// </summary>
        private void PersistSmartReorderFields(Item item, IDbTransaction transaction)
        {
            if (item == null || item.ItemId <= 0)
            {
                return;
            }

            const string sql = @"
UPDATE ItemMaster
SET Order_Cycle_Days = @Order_Cycle_Days,
    Box_Quantity = @Box_Quantity,
    Is_Perishable = @Is_Perishable
WHERE ItemId = @ItemId";

            DataConnection.Execute(
                sql,
                new
                {
                    ItemId = item.ItemId,
                    Order_Cycle_Days = item.Order_Cycle_Days > 0 ? item.Order_Cycle_Days : 0,
                    Box_Quantity = item.Box_Quantity > 0 ? item.Box_Quantity : 0,
                    Is_Perishable = item.Is_Perishable
                },
                transaction,
                commandType: CommandType.Text
            );
        }

        #region Additional Utility Methods

        /// <summary>
        /// Gets all items with basic information
        /// </summary>
        public List<Item> GetAllItems()
        {
            List<Item> items = new List<Item>();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    cmd.Parameters.AddWithValue("@CompanyId", DataBase.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", DataBase.FinyearId);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            items = ds.Tables[0].ToListOfObject<Item>();
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
            return items;
        }

        /// <summary>
        /// Gets price settings for a specific item
        /// </summary>
        public List<ItemMasterPriceSettings> GetItemPriceSettings(int itemId)
        {
            List<ItemMasterPriceSettings> priceSettings = new List<ItemMasterPriceSettings>();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemMasterPriceSettings, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYITEMID");
                    cmd.Parameters.AddWithValue("@ItemId", itemId);
                    cmd.Parameters.AddWithValue("@CompanyId", DataBase.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", DataBase.FinyearId);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            priceSettings = ds.Tables[0].ToListOfObject<ItemMasterPriceSettings>();
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
            return priceSettings;
        }

        /// <summary>
        /// Deletes an item and all its price settings
        /// </summary>
        public string DeleteItem(int itemId)
        {
            DataConnection.Open();
            var transaction = DataConnection.BeginTransaction();

            try
            {
                var deletedItemInfo = DataConnection.QueryFirstOrDefault<dynamic>(
                    "SELECT TOP 1 CAST(ItemNo AS NVARCHAR(50)) AS ItemNo, Description AS ItemName, Barcode FROM ItemMaster WHERE ItemId = @ItemId",
                    new { ItemId = itemId },
                    transaction
                );

                // Delete price settings first
                var deletePriceSettings = new ItemMasterPriceSettings
                {
                    _Operation = "DELETE",
                    ItemId = itemId,
                    CompanyId = Convert.ToInt32(DataBase.CompanyId),
                    BranchId = Convert.ToInt32(DataBase.BranchId),
                    FinYearId = Convert.ToInt32(DataBase.FinyearId)
                };

                List<ItemMasterPriceSettings> deletedPriceSettings = DataConnection.Query<ItemMasterPriceSettings>(
                    STOREDPROCEDURE.POS_ItemMasterPriceSettings,
                    deletePriceSettings,
                    transaction,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                // Delete item master
                var deleteItem = new Item
                {
                    _Operation = "DELETE",
                    ItemId = itemId,
                    CompanyId = Convert.ToInt32(DataBase.CompanyId),
                    BranchId = Convert.ToInt32(DataBase.BranchId),
                    FinYearId = Convert.ToInt32(DataBase.FinyearId)
                };

                List<Item> deletedItems = DataConnection.Query<Item>(
                    STOREDPROCEDURE.POS_ItemMaster,
                    deleteItem,
                    transaction,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                transaction.Commit();
                try
                {
                    using (var activityLog = new ItemActivityLogRepository())
                    {
                        activityLog.SaveItemActivity(
                            itemId,
                            deletedItemInfo != null ? Convert.ToString(deletedItemInfo.ItemNo) : string.Empty,
                            deletedItemInfo != null ? Convert.ToString(deletedItemInfo.ItemName) : string.Empty,
                            deletedItemInfo != null ? Convert.ToString(deletedItemInfo.Barcode) : string.Empty,
                            "DELETE",
                            "Item deleted from item master.");
                    }
                }
                catch (Exception logEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Item delete activity log failed: {logEx.Message}");
                }
                return "Success";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return $"Failed: {ex.Message}";
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        #endregion

        // Method to get detailed item information for Purchase Return using the POS_ItemDetalisDDL stored procedure
        public DataTable getItemDetailsForPurchaseReturn(string searchText)
        {
            DataTable dt = new DataTable();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_ItemDetalisDDL", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", DataBase.CompanyId);
                    cmd.Parameters.AddWithValue("@Barcode", searchText);
                    cmd.Parameters.AddWithValue("@ItemName", searchText);
                    cmd.Parameters.AddWithValue("@Operation", "GETALL"); // Use GETALL to search by description

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        adapt.Fill(dt);
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
            return dt;
        }

        // Method to get hold item details for a specific item
        public List<HoldItemDetails> GetHoldItemDetails(int itemId)
        {
            List<HoldItemDetails> holdDetails = new List<HoldItemDetails>();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand("_POS_ItemMaster", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "HoldItem");
                    cmd.Parameters.AddWithValue("@ItemId", itemId);
                    cmd.Parameters.AddWithValue("@CompanyId", DataBase.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId > 0 ? SessionContext.FinYearId : (int.TryParse(DataBase.FinyearId, out var id) ? id : 1));



                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            HoldItemDetails detail = new HoldItemDetails
                            {
                                BillNo = reader["BillNo"].ToString(),
                                LedgerID = SafeConvertToInt32(reader["LedgerID"], 0),
                                CustomerName = reader["CustomerName"].ToString(),
                                HoldQty = SafeParseDouble(reader["HoldQty"]),
                                Unit = reader["Unit"].ToString()
                            };
                            holdDetails.Add(detail);
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
            return holdDetails;
        }

        /// <summary>
        /// Checks if a barcode already exists in the database for a different item
        /// </summary>
        /// <param name="barcode">The barcode to check</param>
        /// <param name="excludeItemId">Item ID to exclude from check (for updates)</param>
        /// <returns>True if barcode exists, false otherwise</returns>
        public bool CheckBarcodeExists(string barcode, int excludeItemId = 0)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                return false;
            }

            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand("_POS_ItemMaster", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Barcode", barcode);
                    cmd.Parameters.AddWithValue("@CompanyId", DataBase.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", DataBase.FinyearId);
                    cmd.Parameters.AddWithValue("@ItemId", 0);
                    cmd.Parameters.AddWithValue("@ItemNo", 0);
                    cmd.Parameters.AddWithValue("@Description", "");
                    cmd.Parameters.AddWithValue("@ItemTypeId", 0);
                    cmd.Parameters.AddWithValue("@VendorId", 0);
                    cmd.Parameters.AddWithValue("@BrandId", 0);
                    cmd.Parameters.AddWithValue("@GroupId", 0);
                    cmd.Parameters.AddWithValue("@CategoryId", 0);
                    cmd.Parameters.AddWithValue("@BaseUnitId", 0);
                    cmd.Parameters.AddWithValue("@ForCustomerType", "");
                    cmd.Parameters.AddWithValue("@NameInLocalLanguage", "");
                    cmd.Parameters.AddWithValue("@HSNCode", "");
                    cmd.Parameters.AddWithValue("@_Operation", "CHECKBARCODE");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                int existingItemId = SafeConvertToInt32(row["ItemId"], 0);
                                // If excludeItemId is provided and matches, skip this item (it's the item being updated)
                                if (excludeItemId > 0 && existingItemId == excludeItemId)
                                {
                                    continue;
                                }
                                // Barcode exists for a different item
                                return true;
                            }
                        }
                    }
                }
            }
            catch
            {
                // If stored procedure doesn't support CHECKBARCODE, use direct SQL query
                try
                {
                    string query = @"SELECT TOP 1 ItemId FROM ItemMaster 
                                    WHERE Barcode = @Barcode 
                                    AND CompanyId = @CompanyId 
                                    AND BranchId = @BranchId";

                    if (excludeItemId > 0)
                    {
                        query += " AND ItemId != @ExcludeItemId";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, (SqlConnection)DataConnection))
                    {
                        cmd.Parameters.AddWithValue("@Barcode", barcode);
                        cmd.Parameters.AddWithValue("@CompanyId", DataBase.CompanyId);
                        cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                        if (excludeItemId > 0)
                        {
                            cmd.Parameters.AddWithValue("@ExcludeItemId", excludeItemId);
                        }

                        object result = cmd.ExecuteScalar();
                        return result != null && result != DBNull.Value;
                    }
                }
                catch
                {
                    return false; // Return false on error to avoid blocking saves
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if any AliasBarcode in the provided list already exists in PriceSettings.
        /// </summary>
        /// <param name="aliasBarcodes">List of alias barcodes to check</param>
        /// <param name="excludeItemId">ItemId to exclude (for updates)</param>
        /// <returns>First duplicate AliasBarcode found, or null if none</returns>
        public string CheckAliasBarcodesExist(List<string> aliasBarcodes, int excludeItemId = 0)
        {
            // Filter out empty values
            var validBarcodes = aliasBarcodes?
                .Where(b => !string.IsNullOrWhiteSpace(b))
                .Select(b => b.Trim())
                .ToList();

            if (validBarcodes == null || !validBarcodes.Any())
                return null;

            DataConnection.Open();
            try
            {
                // Build parameterized query to check for existing alias barcodes
                string query = @"SELECT TOP 1 AliasBarcode, ItemId FROM PriceSettings 
                                    WHERE AliasBarcode IN ({0}) 
                                    AND BranchId = @BranchId 
                                    AND CompanyId = @CompanyId
                                    AND AliasBarcode IS NOT NULL 
                                    AND AliasBarcode != ''";

                if (excludeItemId > 0)
                {
                    query += " AND ItemId != @ExcludeItemId";
                }

                // Build IN clause with parameters
                var parameters = new List<string>();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = (SqlConnection)DataConnection;

                    for (int i = 0; i < validBarcodes.Count; i++)
                    {
                        string paramName = $"@alias{i}";
                        parameters.Add(paramName);
                        cmd.Parameters.AddWithValue(paramName, validBarcodes[i]);
                    }

                    cmd.CommandText = string.Format(query, string.Join(", ", parameters));
                    cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", DataBase.CompanyId);

                    if (excludeItemId > 0)
                    {
                        cmd.Parameters.AddWithValue("@ExcludeItemId", excludeItemId);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader["AliasBarcode"]?.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CheckAliasBarcodesExist: {ex.Message}");
                return null; // Return null on error to avoid blocking saves
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
            return null;
        }

        // Helper methods for safe conversion
        private static int SafeConvertToInt32(object value, int defaultValue = 0)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;

            if (value is int)
                return (int)value;

            if (int.TryParse(value.ToString(), out int result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// Gets ItemId from PriceSettings table by barcode
        /// </summary>
        /// <param name="barcode">The barcode to search for</param>
        /// <returns>ItemId if found, otherwise 0</returns>
        public int GetItemIdByBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return 0;

            try
            {
                DataConnection.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT TOP 1 ItemId FROM PriceSettings WHERE BarCode = @Barcode AND BranchId = @BranchId",
                    (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@Barcode", barcode.Trim());
                    cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetItemIdByBarcode: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return 0;
        }

        /// <summary>
        /// Gets ItemId from PriceSettings table by alias barcode
        /// </summary>
        /// <param name="aliasBarcode">The alias barcode to search for</param>
        /// <returns>ItemId if found, otherwise 0</returns>
        public int GetItemIdByAliasBarcode(string aliasBarcode)
        {
            if (string.IsNullOrWhiteSpace(aliasBarcode))
                return 0;

            try
            {
                DataConnection.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT TOP 1 ItemId FROM PriceSettings WHERE AliasBarcode = @AliasBarcode AND BranchId = @BranchId AND AliasBarcode IS NOT NULL AND AliasBarcode != ''",
                    (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@AliasBarcode", aliasBarcode.Trim());
                    cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetItemIdByAliasBarcode: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return 0;
        }

        /// <summary>
        /// Gets ItemId from ItemAlternativeBarcode table by alternative barcode
        /// </summary>
        /// <param name="alternativeBarcode">The alternative barcode to search for</param>
        /// <returns>ItemId if found, otherwise 0</returns>
        public int GetItemIdByAlternativeBarcode(string alternativeBarcode)
        {
            if (string.IsNullOrWhiteSpace(alternativeBarcode))
                return 0;

            try
            {
                DataConnection.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT TOP 1 ItemId FROM ItemAlternativeBarcode WHERE Barcode = @Barcode",
                    (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@Barcode", alternativeBarcode.Trim());

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetItemIdByAlternativeBarcode: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return 0;
        }

        /// <summary>
        /// Looks up stock information by barcode or item name (description).
        /// Returns a list of matching items with their stock details.
        /// </summary>
        public List<StockLookupResult> GetStockByBarcodeOrName(string searchTerm)
        {
            var results = new List<StockLookupResult>();
            if (string.IsNullOrWhiteSpace(searchTerm))
                return results;

            try
            {
                DataConnection.Open();
                string sql = @"
                    SELECT TOP 10
                        im.ItemId,
                        im.Description AS ItemName,
                        ps.BarCode,
                        ps.Stock,
                        ps.Unit,
                        ps.RetailPrice,
                        ps.Cost,
                        ps.ReOrder
                    FROM PriceSettings ps
                    INNER JOIN ItemMaster im ON ps.ItemId = im.ItemId
                    WHERE ps.BranchId = @BranchId
                      AND (
                            ps.BarCode = @SearchExact
                            OR ps.AliasBarcode = @SearchExact
                            OR im.Description LIKE @SearchLike
                          )
                    ORDER BY im.Description";

                using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                    cmd.Parameters.AddWithValue("@SearchExact", searchTerm.Trim());
                    cmd.Parameters.AddWithValue("@SearchLike", "%" + searchTerm.Trim() + "%");

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new StockLookupResult
                            {
                                ItemId = reader["ItemId"] != DBNull.Value ? Convert.ToInt32(reader["ItemId"]) : 0,
                                ItemName = reader["ItemName"]?.ToString() ?? "",
                                Barcode = reader["BarCode"]?.ToString() ?? "",
                                Stock = reader["Stock"] != DBNull.Value ? Convert.ToDouble(reader["Stock"]) : 0,
                                Unit = reader["Unit"]?.ToString() ?? "",
                                RetailPrice = reader["RetailPrice"] != DBNull.Value ? Convert.ToDouble(reader["RetailPrice"]) : 0,
                                Cost = reader["Cost"] != DBNull.Value ? Convert.ToDouble(reader["Cost"]) : 0,
                                ReorderLevel = reader["ReOrder"] != DBNull.Value ? Convert.ToDouble(reader["ReOrder"]) : 0
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetStockByBarcodeOrName: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return results;
        }

        /// <summary>
        /// Returns the next available item number for the current company/branch.
        /// </summary>
        public int GetNextItemNumber()
        {
            int nextItemNoFromProcedure = GetNextItemNumberFromProcedure();
            int nextItemNoFromLastItem = GetNextItemNumberFromLastItem();
            int nextItemNoFromAllItems = GetNextItemNumberFromAllItems();
            int nextItemNo = Math.Max(nextItemNoFromProcedure, Math.Max(nextItemNoFromLastItem, nextItemNoFromAllItems));

            return nextItemNo > 0 ? nextItemNo : 1;
        }

        private int GetNextItemNumberFromProcedure()
        {
            int branchId = SessionContext.BranchId > 0 ? SessionContext.BranchId : ParseIntValue(DataBase.BranchId);
            int companyId = SessionContext.CompanyId > 0 ? SessionContext.CompanyId : ParseIntValue(DataBase.CompanyId);
            int finYearId = SessionContext.FinYearId > 0 ? SessionContext.FinYearId : ParseIntValue(DataBase.FinyearId);

            bool wasClosed = DataConnection.State != ConnectionState.Open;

            try
            {
                if (wasClosed)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETNEXTITEMNO");
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        int nextItemNo = Convert.ToInt32(result);
                        if (nextItemNo > 0)
                            return nextItemNo;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetNextItemNumber stored procedure lookup: {ex.Message}");
            }
            finally
            {
                if (wasClosed && DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return 0;
        }

        private int GetNextItemNumberFromLastItem()
        {
            try
            {
                int lastItemId = NavigateItem("LAST", 0);
                if (lastItemId <= 0)
                    return 0;

                ItemGet lastItem = GetByIdItem(lastItemId);
                int lastItemNo;
                if (lastItem != null && int.TryParse(Convert.ToString(lastItem.ItemNo), out lastItemNo) && lastItemNo > 0)
                    return lastItemNo + 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting next item number from last item: {ex.Message}");
            }

            return 0;
        }

        private int GetNextItemNumberFromAllItems()
        {
            try
            {
                List<Item> items = GetAllItems();
                if (items == null || items.Count == 0)
                    return 0;

                int lastItemNo = items
                    .Where(item => item != null && item.ItemNo > 0)
                    .Select(item => item.ItemNo)
                    .DefaultIfEmpty(0)
                    .Max();

                return lastItemNo > 0 ? lastItemNo + 1 : 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting next item number from all items: {ex.Message}");
            }

            return 0;
        }

        /// <summary>
        /// Navigates to another item (FIRST, PREVIOUS, NEXT, LAST) and returns its ItemId, or 0 if none.
        /// </summary>
        public int NavigateItem(string navigationType, int currentItemNo)
        {
            int branchId = SessionContext.BranchId > 0 ? SessionContext.BranchId : ParseIntValue(DataBase.BranchId);
            int companyId = SessionContext.CompanyId > 0 ? SessionContext.CompanyId : ParseIntValue(DataBase.CompanyId);
            int finYearId = SessionContext.FinYearId > 0 ? SessionContext.FinYearId : ParseIntValue(DataBase.FinyearId);

            bool wasClosed = DataConnection.State != ConnectionState.Open;

            try
            {
                if (wasClosed)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "NAVIGATE");
                    cmd.Parameters.AddWithValue("@NavigationType", navigationType ?? string.Empty);
                    cmd.Parameters.AddWithValue("@CurrentItemNo", currentItemNo);
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        return Convert.ToInt32(result);
                }
            }
            finally
            {
                if (wasClosed && DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return 0;
        }

    }

    /// <summary>
    /// Lightweight result class for stock lookups via the AI chatbot.
    /// </summary>
    public class StockLookupResult
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string Barcode { get; set; }
        public double Stock { get; set; }
        public string Unit { get; set; }
        public double RetailPrice { get; set; }
        public double Cost { get; set; }
        public double ReorderLevel { get; set; }
    }
}
