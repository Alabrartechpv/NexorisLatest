using Dapper;
using ModelClass.Master;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Repository.MasterRepositry
{
    public class UnitMasterRepository : BaseRepostitory
    {
        private UnitMaster MapDataRowToUnit(DataRow row, DataColumnCollection columns)
        {
            var unit = new UnitMaster();

            // Only access columns that exist
            if (columns.Contains("UnitID"))
                unit.UnitID = Convert.ToInt32(row["UnitID"]);

            if (columns.Contains("UnitName"))
                unit.UnitName = row["UnitName"].ToString();

            if (columns.Contains("UnitSymbol"))
                unit.UnitSymbol = row["UnitSymbol"] != DBNull.Value ? row["UnitSymbol"].ToString() : "";
            else
                unit.UnitSymbol = "";

            if (columns.Contains("UnitQuantityCode"))
                unit.UnitQuantityCode = row["UnitQuantityCode"] != DBNull.Value ? Convert.ToInt32(row["UnitQuantityCode"]) : 1;
            else
                unit.UnitQuantityCode = 1;

            if (columns.Contains("Packing"))
                unit.Packing = row["Packing"] != DBNull.Value ? Convert.ToDouble(row["Packing"]) : 1;
            else
                unit.Packing = 1;

            if (columns.Contains("NoOfDecimalPlaces"))
                unit.NoOfDecimalPlaces = row["NoOfDecimalPlaces"] != DBNull.Value ? Convert.ToInt32(row["NoOfDecimalPlaces"]) : 0;
            else
                unit.NoOfDecimalPlaces = 0;

            if (columns.Contains("UnitNameInBill"))
                unit.UnitNameInBill = row["UnitNameInBill"] != DBNull.Value ? row["UnitNameInBill"].ToString() : "";
            else
                unit.UnitNameInBill = "";

            if (columns.Contains("IsDelete"))
                unit.IsDelete = row["IsDelete"] != DBNull.Value ? Convert.ToBoolean(row["IsDelete"]) : false;
            else
                unit.IsDelete = false;

            return unit;
        }

        public string SaveUnit(UnitMaster unit)
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_UnitMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UnitID", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitName", unit.UnitName);
                    // Only save UnitName - pass null/defaults for other fields since they are managed in ItemMaster UOM tab
                    cmd.Parameters.AddWithValue("@UnitSymbol", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitQuantityCode", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Packing", DBNull.Value);
                    cmd.Parameters.AddWithValue("@NoOfDecimalPlaces", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitNameInBill", DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsDelete", 0);
                    cmd.Parameters.AddWithValue("@_Operation", "Create");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            var result = ds.Tables[0].Rows[0][0]?.ToString();
                            if (result == "Is Exists")
                            {
                                return "Unit already exists with this name!";
                            }
                        }
                    }
                }
                return "Success";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving unit: {ex.Message}", ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        public List<UnitMaster> GetAllUnits()
        {
            List<UnitMaster> units = new List<UnitMaster>();

            try
            {
                // First, get the list of unit IDs
                List<int> unitIds = new List<int>();

                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_UnitMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UnitID", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitName", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitSymbol", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitQuantityCode", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Packing", DBNull.Value);
                    cmd.Parameters.AddWithValue("@NoOfDecimalPlaces", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitNameInBill", DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsDelete", DBNull.Value);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                unitIds.Add(Convert.ToInt32(row["UnitID"]));
                            }
                        }
                    }
                }

                // Close the connection before making individual calls
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();

                // Now get full details for each unit
                foreach (int unitId in unitIds)
                {
                    try
                    {
                        var fullUnit = GetByIdUnit(unitId);
                        if (fullUnit != null)
                        {
                            units.Add(fullUnit);
                        }
                    }
                    catch (Exception ex)
                    {
                        // If individual unit fetch fails, create a basic unit
                        System.Diagnostics.Debug.WriteLine($"Failed to get unit {unitId}: {ex.Message}");
                        // Continue with next unit rather than failing completely
                    }
                }

                // If no units were loaded, provide fallback data
                if (units.Count == 0 && unitIds.Count == 0)
                {
                    // Fallback to sample data if no units found
                    units = new List<UnitMaster>
                    {
                        new UnitMaster { UnitID = 1, UnitName = "Pieces", UnitSymbol = "PCS", Packing = 1, NoOfDecimalPlaces = 0, UnitNameInBill = "Pieces", IsDelete = false },
                        new UnitMaster { UnitID = 2, UnitName = "Kilograms", UnitSymbol = "KG", Packing = 1, NoOfDecimalPlaces = 2, UnitNameInBill = "Kgs", IsDelete = false },
                        new UnitMaster { UnitID = 3, UnitName = "Liters", UnitSymbol = "L", Packing = 1, NoOfDecimalPlaces = 2, UnitNameInBill = "Ltrs", IsDelete = false },
                        new UnitMaster { UnitID = 4, UnitName = "Meters", UnitSymbol = "M", Packing = 1, NoOfDecimalPlaces = 2, UnitNameInBill = "Mtrs", IsDelete = false }
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving units: {ex.Message}", ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return units;
        }

        /// <summary>
        /// Gets units for display in the grid (only UnitID and UnitName)
        /// </summary>
        public List<ModelClass.Master.UnitMasterDisplay> GetUnitsForDisplay()
        {
            var fullUnits = GetAllUnits();
            var displayUnits = new List<ModelClass.Master.UnitMasterDisplay>();

            foreach (var unit in fullUnits)
            {
                displayUnits.Add(new ModelClass.Master.UnitMasterDisplay
                {
                    UnitID = unit.UnitID,
                    UnitName = unit.UnitName
                });
            }

            return displayUnits;
        }

        public UnitMaster GetByIdUnit(int selectedId)
        {
            UnitMaster item = new UnitMaster();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_UnitMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UnitID", selectedId);
                    cmd.Parameters.AddWithValue("@UnitName", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitSymbol", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitQuantityCode", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Packing", DBNull.Value);
                    cmd.Parameters.AddWithValue("@NoOfDecimalPlaces", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitNameInBill", DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsDelete", DBNull.Value);
                    cmd.Parameters.AddWithValue("@_Operation", "GetById");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            var columns = ds.Tables[0].Columns;
                            var row = ds.Tables[0].Rows[0];
                            item = MapDataRowToUnit(row, columns);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving unit by ID: {ex.Message}", ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return item;
        }

        public UnitMaster UpdateUnit(UnitMaster utm)
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_UnitMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UnitID", utm.UnitID);
                    cmd.Parameters.AddWithValue("@UnitName", utm.UnitName);
                    // Only update UnitName - pass null for other fields since they are managed in ItemMaster UOM tab
                    cmd.Parameters.AddWithValue("@UnitSymbol", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitQuantityCode", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Packing", DBNull.Value);
                    cmd.Parameters.AddWithValue("@NoOfDecimalPlaces", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitNameInBill", DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsDelete", 0);
                    cmd.Parameters.AddWithValue("@_Operation", "Update");

                    // Execute the update command
                    object result = cmd.ExecuteScalar();

                    // Close connection before making another call
                    if (DataConnection.State == ConnectionState.Open)
                        DataConnection.Close();

                    // Check if update was successful
                    if (result != null && result.ToString() == "SUCESS")
                    {
                        // Return the updated unit by getting it from database
                        return GetByIdUnit(utm.UnitID);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating unit: {ex.Message}", ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return null;
        }

        public UnitMaster DeleteUnit(int selectedId)
        {
            UnitMaster unitToDelete = null;

            try
            {
                // First get the unit details before deleting
                unitToDelete = GetByIdUnit(selectedId);

                // Now perform the delete operation
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_UnitMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UnitID", selectedId);
                    cmd.Parameters.AddWithValue("@UnitName", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitSymbol", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitQuantityCode", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Packing", DBNull.Value);
                    cmd.Parameters.AddWithValue("@NoOfDecimalPlaces", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitNameInBill", DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsDelete", DBNull.Value);
                    cmd.Parameters.AddWithValue("@_Operation", "Delete");

                    // Execute the delete command
                    object result = cmd.ExecuteScalar();

                    // Check if delete was successful
                    if (result != null && result.ToString() == "SUCESS")
                    {
                        // Return the deleted unit details
                        return unitToDelete;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting unit: {ex.Message}", ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return null;
        }

        public int GetByIdPacking(int selectedId)
        {
            UnitMaster item = new UnitMaster();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_UnitMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UnitID", selectedId);
                    cmd.Parameters.AddWithValue("@Packing", 0);
                    cmd.Parameters.AddWithValue("@_Operation", "GetById");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            item = ds.Tables[0].Rows[0].ToNullableObject<UnitMaster>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving unit packing: {ex.Message}", ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return Convert.ToInt32(item.Packing);
        }

        public List<UnitMaster> SearchUnits(string searchText)
        {
            List<UnitMaster> units = new List<UnitMaster>();

            try
            {
                // First, get the list of unit IDs matching the search
                List<int> unitIds = new List<int>();

                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_UnitMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UnitID", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitName", searchText);
                    cmd.Parameters.AddWithValue("@UnitSymbol", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitQuantityCode", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Packing", DBNull.Value);
                    cmd.Parameters.AddWithValue("@NoOfDecimalPlaces", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitNameInBill", DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsDelete", DBNull.Value);
                    cmd.Parameters.AddWithValue("@_Operation", "Search");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                unitIds.Add(Convert.ToInt32(row["UnitID"]));
                            }
                        }
                    }
                }

                // Close the connection before making individual calls
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();

                // Now get full details for each unit
                foreach (int unitId in unitIds)
                {
                    try
                    {
                        var fullUnit = GetByIdUnit(unitId);
                        if (fullUnit != null)
                        {
                            units.Add(fullUnit);
                        }
                    }
                    catch (Exception ex)
                    {
                        // If individual unit fetch fails, log and continue
                        System.Diagnostics.Debug.WriteLine($"Failed to get unit {unitId}: {ex.Message}");
                        // Continue with next unit rather than failing completely
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching units: {ex.Message}", ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return units;
        }

        /// <summary>
        /// Gets search results for display in the grid (only UnitID and UnitName)
        /// </summary>
        public List<ModelClass.Master.UnitMasterDisplay> GetSearchResultsForDisplay(string searchText)
        {
            var fullUnits = SearchUnits(searchText);
            var displayUnits = new List<ModelClass.Master.UnitMasterDisplay>();

            foreach (var unit in fullUnits)
            {
                displayUnits.Add(new ModelClass.Master.UnitMasterDisplay
                {
                    UnitID = unit.UnitID,
                    UnitName = unit.UnitName
                });
            }

            return displayUnits;
        }

        public int GetUnitIdByName(string unitName, SqlTransaction transaction = null)
        {
            int unitId = 0;
            bool localTransaction = false;

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                    localTransaction = true;
                }

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_UnitMaster, (SqlConnection)DataConnection, transaction))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UnitName", unitName);
                    cmd.Parameters.AddWithValue("@_Operation", "GetByName");

                    // Add other required parameters with default values
                    cmd.Parameters.AddWithValue("@UnitID", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitSymbol", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitQuantityCode", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Packing", DBNull.Value);
                    cmd.Parameters.AddWithValue("@NoOfDecimalPlaces", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitNameInBill", DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsDelete", DBNull.Value);

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        unitId = Convert.ToInt32(result);
                        System.Diagnostics.Debug.WriteLine($"Found UnitId {unitId} for unit {unitName} using stored procedure");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting UnitId by name: {ex.Message}");
                throw;
            }
            finally
            {
                if (localTransaction && DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return unitId;
        }

        // GetUnitQuantityCodes method removed - no longer needed since Unit Quantity Code field was removed from the form
    }
}
