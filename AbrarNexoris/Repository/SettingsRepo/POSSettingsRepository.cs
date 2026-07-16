using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ModelClass.Settings;

namespace Repository.SettingsRepo
{
    /// <summary>
    /// Repository for managing POS_Settings table operations
    /// </summary>
    public class POSSettingsRepository : BaseRepostitory
    {
        #region Get Settings

        /// <summary>
        /// Gets all settings for a specific company and branch
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <param name="branchId">Branch ID</param>
        /// <returns>List of POSSetting objects</returns>
        public List<POSSetting> GetSettings(int companyId, int branchId)
        {
            var settings = new List<POSSetting>();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Setting, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GET");
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);

                    if (DataConnection.State != ConnectionState.Open)
                        DataConnection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            settings.Add(MapReaderToSetting(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting settings: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return settings;
        }

        /// <summary>
        /// Gets a single setting value
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <param name="branchId">Branch ID</param>
        /// <param name="settingKey">Setting key</param>
        /// <returns>Setting value or null if not found</returns>
        public string GetSettingValue(int companyId, int branchId, string settingKey)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Setting, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GET");
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@SettingKey", settingKey);

                    if (DataConnection.State != ConnectionState.Open)
                        DataConnection.Open();

                    object result = cmd.ExecuteScalar();
                    return result?.ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting setting value: {ex.Message}");
                return null;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        /// <summary>
        /// Gets a POSSetting object by key
        /// </summary>
        public POSSetting GetSetting(int companyId, int branchId, string settingKey)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Setting, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GET_BY_KEY");
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@SettingKey", settingKey);

                    if (DataConnection.State != ConnectionState.Open)
                        DataConnection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapReaderToSetting(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting setting: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return null;
        }

        #endregion

        #region Save Settings

        /// <summary>
        /// Saves or updates a setting value
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <param name="branchId">Branch ID</param>
        /// <param name="settingKey">Setting key</param>
        /// <param name="settingValue">Setting value</param>
        /// <param name="settingType">Setting type (Boolean, String, Integer)</param>
        /// <param name="description">Optional description</param>
        /// <param name="category">Optional category</param>
        /// <returns>True if successful</returns>
        public bool SaveSetting(int companyId, int branchId, string settingKey, string settingValue,
            string settingType = "String", string description = null, string category = null)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Setting, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "SAVE");
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@SettingKey", settingKey);
                    cmd.Parameters.AddWithValue("@SettingValue", settingValue ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SettingType", settingType);
                    cmd.Parameters.AddWithValue("@Description", description ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Category", category ?? (object)DBNull.Value);

                    if (DataConnection.State != ConnectionState.Open)
                        DataConnection.Open();

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving setting: {ex.Message}");
                return false;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        /// <summary>
        /// Saves multiple settings at once
        /// </summary>
        public bool SaveSettings(int companyId, int branchId, Dictionary<string, string> settings)
        {
            bool allSuccess = true;

            foreach (var setting in settings)
            {
                if (!SaveSetting(companyId, branchId, setting.Key, setting.Value))
                {
                    allSuccess = false;
                }
            }

            return allSuccess;
        }

        #endregion

        #region Delete Settings

        /// <summary>
        /// Deletes a specific setting
        /// </summary>
        public bool DeleteSetting(int companyId, int branchId, string settingKey)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Setting, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "DELETE");
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@SettingKey", settingKey);

                    if (DataConnection.State != ConnectionState.Open)
                        DataConnection.Open();

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting setting: {ex.Message}");
                return false;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        #endregion

        #region Initialize Default Settings

        /// <summary>
        /// Initializes default settings for a new company/branch if they don't exist
        /// </summary>
        public void InitializeDefaultSettings(int companyId, int branchId)
        {
            // Initialize each default setting if it doesn't already exist
            InitializeSettingIfNotExists(companyId, branchId, SettingKeys.DuplicateItemBehavior,
                "MergeQuantity", "String", "How to handle scanning same item: MergeQuantity or SeparateRows", "Sales");

            InitializeSettingIfNotExists(companyId, branchId, SettingKeys.AllowNegativeStock,
                "false", "Boolean", "Allow selling items with negative stock", "Inventory");

            InitializeSettingIfNotExists(companyId, branchId, SettingKeys.AutoPrintAfterSave,
                "true", "Boolean", "Automatically print receipt after saving sale", "Printing");

            InitializeSettingIfNotExists(companyId, branchId, SettingKeys.DefaultSaleType,
                "Cash", "String", "Default sale mode for new sales: Cash or Credit", "Sales");

            InitializeSettingIfNotExists(companyId, branchId, SettingKeys.DefaultCreditDays,
                "30", "Integer", "Default credit term (days) for credit sales", "Sales");

            InitializeSettingIfNotExists(companyId, branchId, SettingKeys.DefaultPriceLevel,
                "RetailPrice", "String", "Default price level for new sales", "Sales");

            InitializeSettingIfNotExists(companyId, branchId, SettingKeys.RoundingMethod,
                "None", "String", "Rounding: None, Nearest5, Nearest10, Up, Down", "Sales");

            InitializeSettingIfNotExists(companyId, branchId, SettingKeys.MaxDiscountPercent,
                "100", "Integer", "Maximum allowed discount percentage", "Sales");

            InitializeSettingIfNotExists(companyId, branchId, SettingKeys.AllowSaleBelowCost,
                "false", "Boolean", "Allow selling items below cost price", "Sales");

            InitializeSettingIfNotExists(companyId, branchId, SettingKeys.ShowCostToUser,
                "false", "Boolean", "Show cost column to regular users", "Display");

            InitializeSettingIfNotExists(companyId, branchId, SettingKeys.ShowMarginColumn,
                "false", "Boolean", "Show margin percentage column", "Display");

            InitializeSettingIfNotExists(companyId, branchId, SettingKeys.PrintCopies,
                "1", "Integer", "Number of receipt copies to print", "Printing");
        }

        /// <summary>
        /// Initializes a single setting if it doesn't already exist
        /// </summary>
        private void InitializeSettingIfNotExists(int companyId, int branchId, string key,
            string value, string type, string description, string category)
        {
            var existing = GetSettingValue(companyId, branchId, key);
            if (existing == null)
            {
                SaveSetting(companyId, branchId, key, value, type, description, category);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Maps a SqlDataReader row to POSSetting object
        /// </summary>
        private POSSetting MapReaderToSetting(SqlDataReader reader)
        {
            return new POSSetting
            {
                SettingId = reader["SettingId"] != DBNull.Value ? Convert.ToInt32(reader["SettingId"]) : 0,
                CompanyId = reader["CompanyId"] != DBNull.Value ? Convert.ToInt32(reader["CompanyId"]) : 0,
                BranchId = reader["BranchId"] != DBNull.Value ? Convert.ToInt32(reader["BranchId"]) : 0,
                SettingKey = reader["SettingKey"]?.ToString(),
                SettingValue = reader["SettingValue"]?.ToString(),
                SettingType = reader["SettingType"]?.ToString(),
                Description = reader["Description"]?.ToString(),
                Category = reader["Category"]?.ToString(),
                CreatedDate = reader["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedDate"]) : DateTime.MinValue,
                ModifiedDate = reader["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ModifiedDate"]) : DateTime.MinValue
            };
        }

        #endregion
    }
}
