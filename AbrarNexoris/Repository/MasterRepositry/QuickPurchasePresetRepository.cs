using ModelClass.Master;
using Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Repository.MasterRepositry
{
    /// <summary>
    /// Handles all DB operations for Quick Purchase Presets.
    /// Tables are auto-created on first use via EnsureStorage().
    /// All PKs are non-IDENTITY — uses MAX(id)+1 pattern.
    /// </summary>
    public class QuickPurchasePresetRepository : BaseRepostitory
    {
        public QuickPurchasePresetRepository()
        {
            EnsureStorage();
        }

        private void EnsureStorage()
        {
            SqlConnection conn = DataConnection as SqlConnection;
            if (conn == null) return;

            bool opened = false;
            try
            {
                if (conn.State != ConnectionState.Open) { conn.Open(); opened = true; }

                const string createPreset = @"
                    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='QuickPurchasePreset')
                    CREATE TABLE QuickPurchasePreset (
                        PresetId    INT         NOT NULL PRIMARY KEY,
                        PresetName  NVARCHAR(200) NOT NULL,
                        VendorId    INT         NOT NULL DEFAULT 0,
                        VendorName  NVARCHAR(300) NULL,
                        IsDelete    BIT         NOT NULL DEFAULT 0
                    )";

                const string createItem = @"
                    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='QuickPurchasePresetItem')
                    CREATE TABLE QuickPurchasePresetItem (
                        PresetItemId INT          NOT NULL PRIMARY KEY,
                        PresetId     INT          NOT NULL,
                        ItemId       INT          NOT NULL,
                        ItemName     NVARCHAR(300) NULL,
                        Barcode      NVARCHAR(100) NULL,
                        Unit         NVARCHAR(100) NULL,
                        UnitId       INT          NOT NULL DEFAULT 0,
                        UnitPrice    FLOAT        NOT NULL DEFAULT 0,
                        Cost         FLOAT        NOT NULL DEFAULT 0,
                        Quantity     INT          NOT NULL DEFAULT 1,
                        IsDelete     BIT          NOT NULL DEFAULT 0
                    )";

                using (var cmd = new SqlCommand(createPreset, conn)) cmd.ExecuteNonQuery();
                using (var cmd = new SqlCommand(createItem, conn)) cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"QuickPurchasePresetRepository.EnsureStorage error: {ex.Message}");
            }
            finally
            {
                if (opened && conn.State == ConnectionState.Open) conn.Close();
            }
        }

        // ── Presets ────────────────────────────────────────────────────────────────

        public List<QuickPurchasePreset> GetAllPresets()
        {
            var list = new List<QuickPurchasePreset>();
            SqlConnection conn = DataConnection as SqlConnection;
            if (conn == null) return list;

            bool opened = false;
            try
            {
                if (conn.State != ConnectionState.Open) { conn.Open(); opened = true; }

                const string sql = "SELECT PresetId, PresetName, VendorId, VendorName FROM QuickPurchasePreset WHERE IsDelete = 0 ORDER BY PresetId";
                using (var cmd = new SqlCommand(sql, conn))
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new QuickPurchasePreset
                        {
                            PresetId = Convert.ToInt32(rdr["PresetId"]),
                            PresetName = rdr["PresetName"]?.ToString() ?? string.Empty,
                            VendorId = Convert.ToInt32(rdr["VendorId"]),
                            VendorName = rdr["VendorName"]?.ToString() ?? string.Empty
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllPresets error: {ex.Message}");
            }
            finally
            {
                if (opened && conn.State == ConnectionState.Open) conn.Close();
            }
            return list;
        }

        public int SavePreset(QuickPurchasePreset preset)
        {
            SqlConnection conn = DataConnection as SqlConnection;
            if (conn == null) return 0;

            bool opened = false;
            try
            {
                if (conn.State != ConnectionState.Open) { conn.Open(); opened = true; }

                if (preset.PresetId <= 0)
                {
                    int nextId;
                    using (var idCmd = new SqlCommand("SELECT ISNULL(MAX(PresetId), 0) + 1 FROM QuickPurchasePreset", conn))
                    {
                        nextId = Convert.ToInt32(idCmd.ExecuteScalar());
                    }
                    preset.PresetId = nextId;

                    const string insert = @"
                        INSERT INTO QuickPurchasePreset (PresetId, PresetName, VendorId, VendorName, IsDelete)
                        VALUES (@PresetId, @PresetName, @VendorId, @VendorName, 0)";
                    using (var cmd = new SqlCommand(insert, conn))
                    {
                        cmd.Parameters.Add("@PresetId", SqlDbType.Int).Value = nextId;
                        cmd.Parameters.Add("@PresetName", SqlDbType.NVarChar, 200).Value = (object)preset.PresetName ?? DBNull.Value;
                        cmd.Parameters.Add("@VendorId", SqlDbType.Int).Value = preset.VendorId;
                        cmd.Parameters.Add("@VendorName", SqlDbType.NVarChar, 300).Value = (object)preset.VendorName ?? DBNull.Value;
                        cmd.ExecuteNonQuery();
                    }
                    return nextId;
                }
                else
                {
                    const string update = @"
                        UPDATE QuickPurchasePreset
                        SET PresetName = @PresetName, VendorId = @VendorId, VendorName = @VendorName
                        WHERE PresetId = @PresetId";
                    using (var cmd = new SqlCommand(update, conn))
                    {
                        cmd.Parameters.Add("@PresetId", SqlDbType.Int).Value = preset.PresetId;
                        cmd.Parameters.Add("@PresetName", SqlDbType.NVarChar, 200).Value = (object)preset.PresetName ?? DBNull.Value;
                        cmd.Parameters.Add("@VendorId", SqlDbType.Int).Value = preset.VendorId;
                        cmd.Parameters.Add("@VendorName", SqlDbType.NVarChar, 300).Value = (object)preset.VendorName ?? DBNull.Value;
                        cmd.ExecuteNonQuery();
                    }
                    return preset.PresetId;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SavePreset error: {ex.Message}");
                return 0;
            }
            finally
            {
                if (opened && conn.State == ConnectionState.Open) conn.Close();
            }
        }

        public bool DeletePreset(int presetId)
        {
            SqlConnection conn = DataConnection as SqlConnection;
            if (conn == null || presetId <= 0) return false;

            bool opened = false;
            try
            {
                if (conn.State != ConnectionState.Open) { conn.Open(); opened = true; }

                using (var cmd = new SqlCommand("UPDATE QuickPurchasePreset SET IsDelete = 1 WHERE PresetId = @PresetId", conn))
                {
                    cmd.Parameters.Add("@PresetId", SqlDbType.Int).Value = presetId;
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new SqlCommand("UPDATE QuickPurchasePresetItem SET IsDelete = 1 WHERE PresetId = @PresetId", conn))
                {
                    cmd.Parameters.Add("@PresetId", SqlDbType.Int).Value = presetId;
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeletePreset error: {ex.Message}");
                return false;
            }
            finally
            {
                if (opened && conn.State == ConnectionState.Open) conn.Close();
            }
        }

        // ── Preset Items ───────────────────────────────────────────────────────────

        public List<QuickPurchasePresetItem> GetPresetItems(int presetId)
        {
            var list = new List<QuickPurchasePresetItem>();
            SqlConnection conn = DataConnection as SqlConnection;
            if (conn == null) return list;

            bool opened = false;
            try
            {
                if (conn.State != ConnectionState.Open) { conn.Open(); opened = true; }

                const string sql = @"
                    SELECT PresetItemId, PresetId, ItemId, ItemName, Barcode, Unit, UnitId, UnitPrice, Cost, Quantity
                    FROM QuickPurchasePresetItem
                    WHERE PresetId = @PresetId AND IsDelete = 0
                    ORDER BY PresetItemId";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@PresetId", SqlDbType.Int).Value = presetId;
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            list.Add(new QuickPurchasePresetItem
                            {
                                PresetItemId = Convert.ToInt32(rdr["PresetItemId"]),
                                PresetId = Convert.ToInt32(rdr["PresetId"]),
                                ItemId = Convert.ToInt32(rdr["ItemId"]),
                                ItemName = rdr["ItemName"]?.ToString() ?? string.Empty,
                                Barcode = rdr["Barcode"]?.ToString() ?? string.Empty,
                                Unit = rdr["Unit"]?.ToString() ?? string.Empty,
                                UnitId = Convert.ToInt32(rdr["UnitId"]),
                                UnitPrice = Convert.ToDouble(rdr["UnitPrice"]),
                                Cost = Convert.ToDouble(rdr["Cost"]),
                                Quantity = Convert.ToInt32(rdr["Quantity"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetPresetItems error: {ex.Message}");
            }
            finally
            {
                if (opened && conn.State == ConnectionState.Open) conn.Close();
            }
            return list;
        }

        public int AddItemToPreset(QuickPurchasePresetItem item)
        {
            SqlConnection conn = DataConnection as SqlConnection;
            if (conn == null) return 0;

            bool opened = false;
            try
            {
                if (conn.State != ConnectionState.Open) { conn.Open(); opened = true; }

                int nextId;
                using (var idCmd = new SqlCommand("SELECT ISNULL(MAX(PresetItemId), 0) + 1 FROM QuickPurchasePresetItem", conn))
                {
                    nextId = Convert.ToInt32(idCmd.ExecuteScalar());
                }
                item.PresetItemId = nextId;

                const string sql = @"
                    INSERT INTO QuickPurchasePresetItem
                        (PresetItemId, PresetId, ItemId, ItemName, Barcode, Unit, UnitId, UnitPrice, Cost, Quantity, IsDelete)
                    VALUES
                        (@PresetItemId, @PresetId, @ItemId, @ItemName, @Barcode, @Unit, @UnitId, @UnitPrice, @Cost, @Quantity, 0)";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@PresetItemId", SqlDbType.Int).Value = nextId;
                    cmd.Parameters.Add("@PresetId", SqlDbType.Int).Value = item.PresetId;
                    cmd.Parameters.Add("@ItemId", SqlDbType.Int).Value = item.ItemId;
                    cmd.Parameters.Add("@ItemName", SqlDbType.NVarChar, 300).Value = (object)item.ItemName ?? DBNull.Value;
                    cmd.Parameters.Add("@Barcode", SqlDbType.NVarChar, 100).Value = (object)item.Barcode ?? DBNull.Value;
                    cmd.Parameters.Add("@Unit", SqlDbType.NVarChar, 100).Value = (object)item.Unit ?? DBNull.Value;
                    cmd.Parameters.Add("@UnitId", SqlDbType.Int).Value = item.UnitId;
                    cmd.Parameters.Add("@UnitPrice", SqlDbType.Float).Value = item.UnitPrice;
                    cmd.Parameters.Add("@Cost", SqlDbType.Float).Value = item.Cost;
                    cmd.Parameters.Add("@Quantity", SqlDbType.Int).Value = item.Quantity;
                    cmd.ExecuteNonQuery();
                }
                return nextId;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddItemToPreset error: {ex.Message}");
                return 0;
            }
            finally
            {
                if (opened && conn.State == ConnectionState.Open) conn.Close();
            }
        }

        public bool UpdateItemQuantity(int presetItemId, int quantity)
        {
            SqlConnection conn = DataConnection as SqlConnection;
            if (conn == null) return false;

            bool opened = false;
            try
            {
                if (conn.State != ConnectionState.Open) { conn.Open(); opened = true; }
                using (var cmd = new SqlCommand("UPDATE QuickPurchasePresetItem SET Quantity = @Qty WHERE PresetItemId = @Id", conn))
                {
                    cmd.Parameters.Add("@Qty", SqlDbType.Int).Value = quantity;
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = presetItemId;
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateItemQuantity error: {ex.Message}");
                return false;
            }
            finally
            {
                if (opened && conn.State == ConnectionState.Open) conn.Close();
            }
        }

        public bool UpdateItemCost(int presetItemId, double cost)
        {
            SqlConnection conn = DataConnection as SqlConnection;
            if (conn == null) return false;

            bool opened = false;
            try
            {
                if (conn.State != ConnectionState.Open) { conn.Open(); opened = true; }
                using (var cmd = new SqlCommand("UPDATE QuickPurchasePresetItem SET Cost = @Cost WHERE PresetItemId = @Id", conn))
                {
                    cmd.Parameters.Add("@Cost", SqlDbType.Float).Value = cost;
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = presetItemId;
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateItemCost error: {ex.Message}");
                return false;
            }
            finally
            {
                if (opened && conn.State == ConnectionState.Open) conn.Close();
            }
        }

        public bool RemoveItemFromPreset(int presetItemId)
        {
            SqlConnection conn = DataConnection as SqlConnection;
            if (conn == null) return false;

            bool opened = false;
            try
            {
                if (conn.State != ConnectionState.Open) { conn.Open(); opened = true; }
                using (var cmd = new SqlCommand("UPDATE QuickPurchasePresetItem SET IsDelete = 1 WHERE PresetItemId = @Id", conn))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = presetItemId;
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RemoveItemFromPreset error: {ex.Message}");
                return false;
            }
            finally
            {
                if (opened && conn.State == ConnectionState.Open) conn.Close();
            }
        }
    }
}
