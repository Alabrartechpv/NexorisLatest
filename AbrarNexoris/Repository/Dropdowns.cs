using ModelClass;
using ModelClass.Master;
using ModelClass.TransactionModels;
using Repository.MasterRepositry;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class Dropdowns : BaseRepostitory
    {
        private const string ItemStatusActive = "Active";
        private const string ItemStatusInactive = "Inactive";
        private const string ItemStatusBlockedForSale = "Blocked for Sale";
        private const string ItemStatusBlockedForPurchase = "Blocked for Purchase";
        private const string ItemStatusDiscontinued = "Discontinued";
        private const string ItemStatusTableName = "POS_ItemMasterStatusRules";

        private static readonly string[] knownItemStatuses = new[]
        {
            ItemStatusActive,
            ItemStatusInactive,
            ItemStatusBlockedForSale,
            ItemStatusBlockedForPurchase,
            ItemStatusDiscontinued
        };

        private static bool itemStatusStorageEnsured;

        public static string NormalizeItemStatusName(string statusName)
        {
            if (!string.IsNullOrWhiteSpace(statusName))
            {
                string normalized = knownItemStatuses
                    .FirstOrDefault(status => string.Equals(status, statusName.Trim(), StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    return normalized;
                }
            }

            return ItemStatusActive;
        }

        public static bool DoesStatusBlockSale(string statusName)
        {
            switch (NormalizeItemStatusName(statusName))
            {
                case ItemStatusInactive:
                case ItemStatusBlockedForSale:
                case ItemStatusDiscontinued:
                    return true;
                default:
                    return false;
            }
        }

        public static bool DoesStatusBlockPurchase(string statusName)
        {
            switch (NormalizeItemStatusName(statusName))
            {
                case ItemStatusInactive:
                case ItemStatusBlockedForPurchase:
                case ItemStatusDiscontinued:
                    return true;
                default:
                    return false;
            }
        }

        private static ItemStatusRuleInfo CreateDefaultItemStatus(int itemId = 0)
        {
            return new ItemStatusRuleInfo
            {
                ItemId = itemId,
                StatusName = ItemStatusActive,
                StatusReason = string.Empty,
                StatusDate = DateTime.Today,
                BlockSale = false,
                BlockPurchase = false
            };
        }

        private static void EnsureStatusColumns(DataTable table)
        {
            if (table == null)
            {
                return;
            }

            if (!table.Columns.Contains("ItemStatus"))
            {
                table.Columns.Add("ItemStatus", typeof(string));
            }

            if (!table.Columns.Contains("StatusReason"))
            {
                table.Columns.Add("StatusReason", typeof(string));
            }

            if (!table.Columns.Contains("StatusDate"))
            {
                table.Columns.Add("StatusDate", typeof(DateTime));
            }

            if (!table.Columns.Contains("BlockSale"))
            {
                table.Columns.Add("BlockSale", typeof(bool));
            }

            if (!table.Columns.Contains("BlockPurchase"))
            {
                table.Columns.Add("BlockPurchase", typeof(bool));
            }
        }

        private static void ApplyItemStatus(ItemDDl item, ItemStatusRuleInfo status)
        {
            if (item == null)
            {
                return;
            }

            ItemStatusRuleInfo effectiveStatus = status ?? CreateDefaultItemStatus(item.ItemId);
            item.ItemStatus = NormalizeItemStatusName(effectiveStatus.StatusName);
            item.StatusReason = effectiveStatus.StatusReason ?? string.Empty;
            item.StatusDate = effectiveStatus.StatusDate;
            item.BlockSale = effectiveStatus.BlockSale;
            item.BlockPurchase = effectiveStatus.BlockPurchase;
        }

        public bool EnsureItemStatusStorage()
        {
            if (itemStatusStorageEnsured)
            {
                return true;
            }

            SqlConnection connection = DataConnection as SqlConnection;
            if (connection == null)
            {
                return false;
            }

            bool openedHere = false;
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                    openedHere = true;
                }

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemMasterStatusRules, connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "INITIALIZE");
                    cmd.ExecuteNonQuery();
                }

                itemStatusStorageEnsured = true;
                return true;
            }
            catch
            {
                itemStatusStorageEnsured = true;
                return true;
            }
            finally
            {
                if (openedHere && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public ItemStatusRuleInfo GetItemStatus(int itemId)
        {
            if (itemId <= 0)
            {
                return CreateDefaultItemStatus(itemId);
            }

            Dictionary<int, ItemStatusRuleInfo> statuses = GetItemStatuses(new[] { itemId });
            if (statuses.TryGetValue(itemId, out ItemStatusRuleInfo status))
            {
                return status;
            }

            return CreateDefaultItemStatus(itemId);
        }

        public Dictionary<int, ItemStatusRuleInfo> GetItemStatuses(IEnumerable<int> itemIds)
        {
            Dictionary<int, ItemStatusRuleInfo> statusMap = new Dictionary<int, ItemStatusRuleInfo>();
            List<int> ids = itemIds?
                .Where(itemId => itemId > 0)
                .Distinct()
                .ToList() ?? new List<int>();

            foreach (int itemId in ids)
            {
                statusMap[itemId] = CreateDefaultItemStatus(itemId);
            }

            if (ids.Count == 0 || !EnsureItemStatusStorage())
            {
                return statusMap;
            }

            SqlConnection connection = DataConnection as SqlConnection;
            if (connection == null)
            {
                return statusMap;
            }

            bool openedHere = false;
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                    openedHere = true;
                }

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemMasterStatusRules, connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int itemId = reader["ItemId"] != DBNull.Value ? Convert.ToInt32(reader["ItemId"]) : 0;
                            if (itemId <= 0 || !statusMap.ContainsKey(itemId))
                            {
                                continue;
                            }

                            statusMap[itemId] = new ItemStatusRuleInfo
                            {
                                ItemId = itemId,
                                StatusName = NormalizeItemStatusName(reader["StatusName"]?.ToString()),
                                StatusReason = reader["StatusReason"] == DBNull.Value ? string.Empty : reader["StatusReason"].ToString(),
                                StatusDate = reader["StatusDate"] == DBNull.Value ? (DateTime?)DateTime.Today : Convert.ToDateTime(reader["StatusDate"]),
                                BlockSale = reader["BlockSale"] != DBNull.Value && Convert.ToBoolean(reader["BlockSale"]),
                                BlockPurchase = reader["BlockPurchase"] != DBNull.Value && Convert.ToBoolean(reader["BlockPurchase"])
                            };
                        }
                    }
                }
            }
            catch
            {
                // Default status map is already prepared for fallback behavior.
            }
            finally
            {
                if (openedHere && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return statusMap;
        }

        public void ApplyItemStatuses(IEnumerable<ItemDDl> items)
        {
            List<ItemDDl> itemList = items?.Where(item => item != null).ToList() ?? new List<ItemDDl>();
            if (itemList.Count == 0)
            {
                return;
            }

            Dictionary<int, ItemStatusRuleInfo> statuses = GetItemStatuses(itemList.Select(item => item.ItemId));
            foreach (ItemDDl item in itemList)
            {
                statuses.TryGetValue(item.ItemId, out ItemStatusRuleInfo status);
                ApplyItemStatus(item, status);
            }
        }

        public void ApplyItemStatuses(DataTable table, string itemIdColumn = "ItemId")
        {
            if (table == null || string.IsNullOrWhiteSpace(itemIdColumn) || !table.Columns.Contains(itemIdColumn))
            {
                return;
            }

            EnsureStatusColumns(table);
            List<int> itemIds = new List<int>();
            foreach (DataRow row in table.Rows)
            {
                if (row == null)
                {
                    continue;
                }

                int itemId;
                if (int.TryParse(row[itemIdColumn]?.ToString(), out itemId) && itemId > 0)
                {
                    itemIds.Add(itemId);
                }
            }

            Dictionary<int, ItemStatusRuleInfo> statuses = GetItemStatuses(itemIds);
            foreach (DataRow row in table.Rows)
            {
                int itemId;
                int.TryParse(row[itemIdColumn]?.ToString(), out itemId);
                ItemStatusRuleInfo status;
                if (!statuses.TryGetValue(itemId, out status))
                {
                    status = CreateDefaultItemStatus(itemId);
                }

                row["ItemStatus"] = NormalizeItemStatusName(status.StatusName);
                row["StatusReason"] = status.StatusReason ?? string.Empty;
                row["StatusDate"] = status.StatusDate.HasValue ? (object)status.StatusDate.Value.Date : DBNull.Value;
                row["BlockSale"] = status.BlockSale;
                row["BlockPurchase"] = status.BlockPurchase;
            }
        }

        //here geting all branch deteails from database
        public BranchDDlGrid getBanchDDl()
        {
            BranchDDlGrid grid = new BranchDDlGrid();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Branch, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<BranchDDl>();
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
            return grid;
        }

        //here getting all paymode details from paymode
        public PaymodeDDlGrid GetPaymode()
        {
            PaymodeDDlGrid grid = new PaymodeDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_PayMode, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<PaymodeDDl>();
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
            return grid;
        }

        /// <summary>
        /// here we getting all customer details from database
        /// </summary>
        /// <returns></returns>
        public CustomerDDlGrid CustomerDDl()
        {
            CustomerDDlGrid grid = new CustomerDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_CustomerDDl, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@Operation", "GETCUSTOMER");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<CustomerDDl>();
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
            return grid;
        }

        public VendorDDLGrids VendorDDL()
        {
            VendorDDLGrids ObjVendorDDLGrid = new VendorDDLGrids();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vendor, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@_Operation", "DDLVendor");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            ObjVendorDDLGrid.List = ds.Tables[0].ToListOfObject<VendorDDLG>();
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
            return ObjVendorDDLGrid;

        }


        public PriceLevelDDlGrid GetPriceLevel()
        {
            PriceLevelDDlGrid grid = new PriceLevelDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@Operation", "PriceLevel");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<PriceLevelDDl>();
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
            return grid;
        }




        /// <summary>
        /// here we are going to get all 
        /// </summary>
        /// <returns></returns>
        public PaymodeDDlGrid PaymodeDDl()
        {
            PaymodeDDlGrid grid = new PaymodeDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@FinyearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@Operation", "PAYMODE");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<PaymodeDDl>();
                        }
                        else
                        {
                            // Self-healing: if PayMode is empty, seed default payment modes automatically
                            AutoSeedPayModes((SqlConnection)DataConnection);

                            using (SqlCommand reCmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                            {
                                reCmd.CommandType = CommandType.StoredProcedure;
                                reCmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                                reCmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                                reCmd.Parameters.AddWithValue("@FinyearId", SessionContext.FinYearId);
                                reCmd.Parameters.AddWithValue("@Barcode", "");
                                reCmd.Parameters.AddWithValue("@Operation", "PAYMODE");
                                using (SqlDataAdapter adapt2 = new SqlDataAdapter(reCmd))
                                {
                                    DataSet ds2 = new DataSet();
                                    adapt2.Fill(ds2);
                                    if ((ds2 != null) && (ds2.Tables.Count > 0) && (ds2.Tables[0] != null) && (ds2.Tables[0].Rows.Count > 0))
                                    {
                                        grid.List = ds2.Tables[0].ToListOfObject<PaymodeDDl>();
                                    }
                                }
                            }
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
            return grid;
        }

        private void AutoSeedPayModes(SqlConnection conn)
        {
            try
            {
                PaymodeRepository.EnsurePaymodeSeedData(conn);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AutoSeedPayModes error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all units for a specific item using ItemId.
        /// Useful when different units have different barcodes.
        /// </summary>
        public ItemDDlGrid GetItemUnits(int itemId)
        {
            ItemDDlGrid grid = new ItemDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@FinyearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@ItemId", itemId);
                    cmd.Parameters.AddWithValue("@Barcode", DBNull.Value);
                    cmd.Parameters.AddWithValue("@description", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Operation", "ItemUnit");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            List<ItemDDl> items = ds.Tables[0].ToListOfObject<ItemDDl>()?.ToList() ?? new List<ItemDDl>();
                            ApplyItemStatuses(items);
                            grid.List = items;
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
            return grid;
        }

        /// <summary>
        /// here getting Item Details
        /// </summary>
        /// <param name="Barcode"></param>
        /// <param name="ItemName"></param>
        /// <returns></returns>
        public ItemDDlGrid itemDDlGrid(string Barcode = null, string ItemName = null)
        {
            ItemDDlGrid grid = new ItemDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemDetalisDDL, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    //  cmd.Parameters.AddWithValue("@FinyearId", Convert.ToInt64(DataBase.));
                    cmd.Parameters.AddWithValue("@Barcode", Barcode);
                    cmd.Parameters.AddWithValue("@ItemName", ItemName);

                    cmd.Parameters.AddWithValue("@Operation", DataBase.Operations);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            List<ItemDDl> items = ds.Tables[0].ToListOfObject<ItemDDl>()?.ToList() ?? new List<ItemDDl>();
                            ApplyItemStatuses(items);
                            grid.List = items;
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
            return grid;
        }


        public StateDDlGrid getStateDDl()
        {
            StateDDlGrid grid = new StateDDlGrid();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_State, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<StateDDL>();
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
            return grid;
        }

        /// <summary>
        /// here getting users details
        /// </summary>
        /// <param name="Barcode"></param>
        /// <param name="ItemName"></param>
        /// <returns></returns>
        public SalesPersonDDlGrid GetSalesPerson(string Barcode = null, string ItemName = null)
        {
            SalesPersonDDlGrid grid = new SalesPersonDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@FinyearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@Barcode", Barcode);

                    cmd.Parameters.AddWithValue("@Operation", "SALESPERSON");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<SalesPersonDDl>();
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
            return grid;
        }

        public GroupDDlGrid getGroupDDl()
        {
            GroupDDlGrid grid = new GroupDDlGrid();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Group, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<GroupDDL>();
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
            return grid;
        }

        public GroupDDlGrid GroupDDl()
        {
            GroupDDlGrid grid = new GroupDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@Operation", "Group");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<GroupDDL>();
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
            return grid;
        }

        public CompanyDDlGrid CompanyDDl()
        {
            CompanyDDlGrid grid = new CompanyDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");

                    cmd.Parameters.AddWithValue("@Operation", "Company");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<CompanyDDl>();
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
            return grid;
        }

        public CategoryDDlGrid getCategoryDDl(string search)
        {
            CategoryDDlGrid grid = new CategoryDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", search);

                    cmd.Parameters.AddWithValue("@Operation", "Category");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<CategoryDDL>();
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
            return grid;
        }

        public BrandDDLGrid getBrandDDl()
        {
            BrandDDLGrid grid = new BrandDDLGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@Operation", "Brand");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<BrandDDL>();
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
            return grid;
        }

        public ItemTypeDDlGrid getItemTypeDDl()
        {
            ItemTypeDDlGrid itemtypeddl = new ItemTypeDDlGrid();
            try
            {
                var repo = new MasterRepositry.ItemTypeRepository();
                var items = repo.GetAllItemTypes();
                if (items != null && items.Count > 0)
                {
                    itemtypeddl.List = items.Select(x => new ItemTypeDDL
                    {
                        Id = x.Id,
                        ItemType = x.ItemTypeName
                    }).ToList();
                    return itemtypeddl;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ItemTypeRepository DDL fallback: {ex.Message}");
            }

            if (DataConnection.State != ConnectionState.Open) DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@Operation", "ItemType");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            itemtypeddl.List = ds.Tables[0].ToListOfObject<ItemTypeDDL>();
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
            return itemtypeddl;
        }

        public CustomerTypeDDlGrid getCustomerTypeDDl()
        {
            CustomerTypeDDlGrid custtypeddl = new CustomerTypeDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@Operation", "PriceLevel");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            custtypeddl.List = ds.Tables[0].ToListOfObject<CustomerTypeDDL>();
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
            return custtypeddl;
        }

        public UnitDDlGrid getUnitDDl()
        {
            UnitDDlGrid unitgrid = new UnitDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@Operation", "Unit");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            unitgrid.List = ds.Tables[0].ToListOfObject<UnitDDL>();
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
            return unitgrid;
        }

        public ItemDDlGrid ListItemDDl()
        {
            ItemDDlGrid igrid = new ItemDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@Operation", "ITEM");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            List<ItemDDl> items = ds.Tables[0].ToListOfObject<ItemDDl>()?.ToList() ?? new List<ItemDDl>();
                            ApplyItemStatuses(items);
                            igrid.List = items;
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
            return igrid;
        }

        public ItemlistDDlGrid ItemlistgridDDl()
        {
            ItemlistDDlGrid ilistgrid = new ItemlistDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@ItemId", 0);
                    cmd.Parameters.AddWithValue("@description", "");
                    cmd.Parameters.AddWithValue("@Operation", "ITEMLIST");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            ilistgrid.List = ds.Tables[0].ToListOfObject<ItemlistDDl>();
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
            return ilistgrid;
        }

        public ItemlistDDlGrid SearchItem(ItemlistDDl itemddl)
        {
            ItemlistDDlGrid searchdesc = new ItemlistDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", itemddl.Barcode);
                    cmd.Parameters.AddWithValue("@description", itemddl.Description);
                    cmd.Parameters.AddWithValue("@Operation", "ITEMLIST");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            searchdesc.List = ds.Tables[0].ToListOfObject<ItemlistDDl>();
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
            return searchdesc;
        }

        public BrandDDLGrid getBrandDDL()
        {
            BrandDDLGrid grid = new BrandDDLGrid();
            Brand brd = new Brand();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Brand, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", brd.Id);
                    cmd.Parameters.AddWithValue("@BrandName", brd.BrandName);
                    cmd.Parameters.AddWithValue("@Photo", brd.Photo);
                    cmd.Parameters.AddWithValue("@IsDelete", 0);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds.Tables.Count > 0) && (ds != null) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {

                            grid.List = ds.Tables[0].ToListOfObject<BrandDDL>();
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
            return grid;
        }
        public UserLevelDDlGrid UserLevelDDl()
        {
            UserLevelDDlGrid usergrid = new UserLevelDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@Operation", "UserLevel");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            usergrid.List = ds.Tables[0].ToListOfObject<UserLevelDDl>();
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
            return usergrid;
        }
        public UserDDlGrid getUsersDDl()
        {
            UserDDlGrid grid = new UserDDlGrid();
            Users us = new Users();
            int companyId = SessionContext.CompanyId;
            int branchId = SessionContext.BranchId;
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_User, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", us.UserID);
                    cmd.Parameters.AddWithValue("@CompanyID", companyId);
                    cmd.Parameters.AddWithValue("@BranchID", branchId);
                    cmd.Parameters.AddWithValue("@UserLevelID", us.UserLevelID);
                    cmd.Parameters.AddWithValue("@UserName", us.UserName);
                    cmd.Parameters.AddWithValue("@Password", us.Password);
                    cmd.Parameters.AddWithValue("@Email", us.Email);
                    cmd.Parameters.AddWithValue("@IsDelete", 0);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds.Tables.Count > 0) && (ds != null) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {

                            grid.List = ds.Tables[0].ToListOfObject<UsersDDl>();
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
            return grid;
        }
        public SalesDDlGrid SalesDDl()
        {
            SalesDDlGrid usergrid = new SalesDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@Operation", "SalesInvoice");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            usergrid.List = ds.Tables[0].ToListOfObject<SalesDDl>();
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
            return usergrid;
        }

        public TaxTypeDDLGrid GetTaxType()
        {
            TaxTypeDDLGrid TaxType = new TaxTypeDDLGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@Operation", "TaxType");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            TaxType.List = ds.Tables[0].ToListOfObject<TaxTypeDDL>();
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
            return TaxType;
        }

        public static void EnsureTaxSeedData(SqlConnection conn)
        {
            if (conn == null) return;

            bool openedHere = false;
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                    openedHere = true;
                }

                // Call POS_dropdown Stored Procedure with operation SEEDTAXPER
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@Operation", "SEEDTAXPER");
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnsureTaxSeedData SP warning: {ex.Message}");
            }
            finally
            {
                if (openedHere && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        public TaxPerDDlGrid GetTaxPer()
        {
            TaxPerDDlGrid taxper = new TaxPerDDlGrid();
            if (DataConnection.State == ConnectionState.Open)
                DataConnection.Close();

            DataConnection.Open();
            try
            {
                EnsureTaxSeedData((SqlConnection)DataConnection);

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@Operation", "GETTAXPER");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            taxper.List = ds.Tables[0].ToListOfObject<TaxPerDDl>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTaxPer error: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            // Fallback: If DB table is still empty or SP returned nothing, return default tax percentages (0%, 5%, 12%, 18%, 28%)
            if (taxper.List == null || !taxper.List.Any())
            {
                taxper.List = new List<TaxPerDDl>
                {
                    new TaxPerDDl { Id = 1, TaxPer = 0.00m },
                    new TaxPerDDl { Id = 2, TaxPer = 5.00m },
                    new TaxPerDDl { Id = 3, TaxPer = 12.00m },
                    new TaxPerDDl { Id = 4, TaxPer = 18.00m },
                    new TaxPerDDl { Id = 5, TaxPer = 28.00m }
                };
            }

            return taxper;
        }

        public CountryDDLGrid CountryDDl()
        {
            CountryDDLGrid grid = new CountryDDLGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");

                    cmd.Parameters.AddWithValue("@Operation", "Country");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<CountryDDL>();
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
            return grid;
        }

        public TaxTypeDDLGrid TaxTypeDDL()
        {
            TaxTypeDDLGrid grid = new TaxTypeDDLGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");

                    cmd.Parameters.AddWithValue("@Operation", "TaxType");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<TaxTypeDDL>();
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
            return grid;
        }


        public CountryDDLGrid getCountryDDl()
        {
            CountryDDLGrid grid = new CountryDDLGrid();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Country, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<CountryDDL>();
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
            return grid;
        }

        public CurrencyDDLGRID getCurrency()
        {
            CurrencyDDLGRID GridCur = new CurrencyDDLGRID();
            try
            {
                CurrencyRepository repo = new CurrencyRepository();
                GridCur.List = repo.GetAllCurrencies();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("getCurrency error: " + ex.Message);
            }
            return GridCur;
        }

        public CountryDDLGrid SearchCountry(string searchTerm)
        {
            CountryDDLGrid records = new CountryDDLGrid();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Country, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CountryName", searchTerm);
                    cmd.Parameters.AddWithValue("@_Operation", "SEARCH");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            records.List = ds.Tables[0].ToListOfObject<CountryDDL>();
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
            return records;
        }
        //*****************
        public ReasonDDLGrid getReasonDDl()
        {
            ReasonDDLGrid grid = new ReasonDDLGrid();
            List<Reason> reasonList = new List<Reason>();
            HashSet<int> addedLedgerIds = new HashSet<int>();

            bool wasClosed = DataConnection.State == ConnectionState.Closed;
            if (wasClosed) DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_StockAdjustmentReasonMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Operations", "GET_REASONS");
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int ledgerId = Convert.ToInt32(reader["LedgerId"]);
                            string reasonName = reader["ReasonName"].ToString();
                            if (addedLedgerIds.Add(ledgerId))
                            {
                                reasonList.Add(new Reason { LedgerID = ledgerId, ReasonName = reasonName });
                            }
                        }
                    }
                }

                grid.List = reasonList.OrderBy(r => r.ReasonName).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("getReasonDDl Error: " + ex.Message);
            }
            finally
            {
                if (wasClosed && DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return grid;
        }
        //******************

        public PurchaseGrid getAllPurchaseMaster()
        {
            PurchaseGrid PurGrid = new PurchaseGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Purchase, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            PurGrid.ListPurchase = ds.Tables[0].ToListOfObject<PurchaseMaster>();
                        }
                    }


                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return PurGrid;

        }

        public StockGrid getAllDocNo()
        {
            StockGrid stkGrid = new StockGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_StockAdjustemnt, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            stkGrid.ListMaster = ds.Tables[0].ToListOfObject<StockAdjMasterDialog>();
                        }
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[1] != null) && (ds.Tables[1].Rows.Count > 0))
                        {
                            stkGrid.ListDetails = ds.Tables[1].ToListOfObject<StockAdjPriceDetails>();
                        }
                    }


                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return stkGrid;

        }

        public StockGrid getStockAdjustmentById(int id)
        {
            StockGrid stkGrid = new StockGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_StockAdjustemnt, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);

                        // First table contains master record
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            // Map the master record to a list with single item
                            var masterRecord = ds.Tables[0].Rows[0].ToNullableObject<StockAdjMasterDialog>();
                            stkGrid.ListMaster = new List<StockAdjMasterDialog> { masterRecord };
                        }

                        // Second table contains detail records
                        if ((ds != null) && (ds.Tables.Count > 1) && (ds.Tables[1] != null) && (ds.Tables[1].Rows.Count > 0))
                        {
                            stkGrid.ListDetails = ds.Tables[1].ToListOfObject<StockAdjPriceDetails>();
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return stkGrid;
        }
    }
}
