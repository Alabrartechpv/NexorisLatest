using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using ModelClass;
using ModelClass.Accounts;
using Ledger = ModelClass.Accounts.Ledger;

namespace Repository.Accounts
{
    public class LedgerRepository : BaseRepostitory
    {
        // Method to get all ledgers from the database
        public DataTable GetAllLedgers(int branchId = 0)
        {
            DataTable dtResult = new DataTable();

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_Ledger", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    // Always pass @BranchID so the SP always filters by branch.
                    // Pass 0 only if the caller explicitly wants all branches (admin use).
                    cmd.Parameters.AddWithValue("@BranchID", branchId == 0 ? (object)DBNull.Value : branchId);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        adapt.Fill(dtResult);
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

            return dtResult;
        }

        public Dictionary<int, decimal> GetLedgerBalances(int companyId, int branchId, int finYearId, DateTime toDate)
        {
            Dictionary<int, decimal> balances = new Dictionary<int, decimal>();

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Ledger, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETBALANCES");
                    cmd.Parameters.AddWithValue("@BranchID", branchId <= 0 ? (object)DBNull.Value : branchId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int ledgerId = Convert.ToInt32(reader["LedgerID"]);
                            decimal balance = reader["Balance"] != DBNull.Value ? Convert.ToDecimal(reader["Balance"]) : 0;
                            balances[ledgerId] = balance;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetLedgerBalances SP error: " + ex.Message);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return balances;
        }

        /// <summary>
        /// Gets live inventory stock valuation (Sum of ClosingStock * Cost) for Stock In Hand (Group 18)
        /// Uses the same SQL-side SUM pattern as POS_BalanceSheet and POS_TradingPLAccount SPs
        /// to guarantee identical precision across all reports.
        /// </summary>
        public decimal GetLiveStockValuation(int companyId, int branchId, int finYearId)
        {
            decimal stockValuation = 0;
            bool wasClosed = DataConnection.State == ConnectionState.Closed;
            if (wasClosed) DataConnection.Open();

            try
            {
                int effCompanyId = companyId > 0 ? companyId : (DataBase.CompanyId != null ? Convert.ToInt32(DataBase.CompanyId) : 1);
                int effBranchId = branchId > 0 ? branchId : (DataBase.BranchId != null ? Convert.ToInt32(DataBase.BranchId) : 1);
                int effFinYearId = finYearId > 0 ? finYearId : (DataBase.FinyearId != null ? Convert.ToInt32(DataBase.FinyearId) : 1);

                // Use SQL-side SUM to match Balance Sheet / Trading P&L SP precision exactly
                string sql = @"
                    CREATE TABLE #StockVal (
                        ItemId INT, GroupName NVARCHAR(500), CategoryName NVARCHAR(500), SubCategoryName NVARCHAR(500),
                        Barcode NVARCHAR(100), ItemName NVARCHAR(500), OpeningStock DECIMAL(18,5), Purchase DECIMAL(18,5),
                        PurchaseReturn DECIMAL(18,5), StockAdjustmentIn DECIMAL(18,5), StockAdjustmentOut DECIMAL(18,5),
                        StockTransferIn DECIMAL(18,5), StockTransferOut DECIMAL(18,5), Sales DECIMAL(18,5), Profit DECIMAL(18,5),
                        SaleAmount DECIMAL(18,5), SalesReturn DECIMAL(18,5), ClosingStock DECIMAL(18,5), OrderedStock DECIMAL(18,5),
                        HoldQty DECIMAL(18,5), Cost DECIMAL(18,5), RetailPrice DECIMAL(18,5), WholeSalePrice DECIMAL(18,5),
                        CreditPrice DECIMAL(18,5), BaseUnitName NVARCHAR(100)
                    );
                    INSERT INTO #StockVal
                    EXEC [dbo].[_Test16] 
                        @FromDate = '1753-01-01', @ToDate = @p_ToDate, 
                        @CompanyId = @p_CompanyId, @BranchId = @p_BranchId, @FinYearId = @p_FinYearId;
                    SELECT ISNULL(SUM(ClosingStock * Cost), 0) AS StockValuation FROM #StockVal;
                    DROP TABLE #StockVal;";

                using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                {
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@p_ToDate", DateTime.Today.AddDays(1).AddSeconds(-1));
                    cmd.Parameters.AddWithValue("@p_CompanyId", effCompanyId);
                    cmd.Parameters.AddWithValue("@p_BranchId", effBranchId);
                    cmd.Parameters.AddWithValue("@p_FinYearId", effFinYearId);

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        stockValuation = Convert.ToDecimal(result);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetLiveStockValuation error: " + ex.Message);
            }
            finally
            {
                if (wasClosed && DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return stockValuation;
        }

        // Method to create a new ledger
        public bool CreateLedger(Ledger ledger)
        {
            bool result = false;

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_Ledger", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "CREATE");
                    cmd.Parameters.AddWithValue("@CompanyID", ledger.CompanyID);
                    cmd.Parameters.AddWithValue("@BranchID", ledger.BranchID);
                    cmd.Parameters.AddWithValue("@LedgerID", ledger.LedgerID);
                    cmd.Parameters.AddWithValue("@LedgerName", ledger.LedgerName);
                    cmd.Parameters.AddWithValue("@Alias", string.IsNullOrEmpty(ledger.Alias) ? DBNull.Value : (object)ledger.Alias);
                    cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(ledger.Description) ? DBNull.Value : (object)ledger.Description);
                    cmd.Parameters.AddWithValue("@Notes", string.IsNullOrEmpty(ledger.Notes) ? DBNull.Value : (object)ledger.Notes);
                    cmd.Parameters.AddWithValue("@GroupID", ledger.GroupID);
                    cmd.Parameters.AddWithValue("@OpnDebit", ledger.OpnDebit);
                    cmd.Parameters.AddWithValue("@OpnCredit", ledger.OpnCredit);
                    cmd.Parameters.AddWithValue("@ProvideBankDetails", ledger.ProvideBankDetails ?? false);
                    cmd.Parameters.AddWithValue("@GstApplicable", ledger.GstApplicable ?? false);
                    cmd.Parameters.AddWithValue("@VatApplicable", ledger.VatApplicable ?? false);
                    cmd.Parameters.AddWithValue("@InventoryValuesAffected", ledger.InventoryValuesAffected ?? false);
                    cmd.Parameters.AddWithValue("@MaintainBillWiseDetails", ledger.MaintainBillWiseDetails ?? false);
                    cmd.Parameters.AddWithValue("@PriceLevelApplicable", ledger.PriceLevelApplicable ?? false);

                    object scalar = cmd.ExecuteScalar();
                    result = scalar != null && Convert.ToInt32(scalar) > 0;
                }
            }
            catch (SqlException sqlEx) when (sqlEx.Number == 2627 || sqlEx.Number == 2601)
            {
                // Primary key / Duplicate Key violation fallback:
                // Auto-generate next free LedgerID and retry insertion
                try
                {
                    int freshId = GetNextLedgerID();
                    ledger.LedgerID = freshId;

                    if (DataConnection.State != ConnectionState.Open)
                        DataConnection.Open();

                    using (SqlCommand cmd = new SqlCommand("POS_Ledger", (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@_Operation", "CREATE");
                        cmd.Parameters.AddWithValue("@CompanyID", ledger.CompanyID);
                        cmd.Parameters.AddWithValue("@BranchID", ledger.BranchID);
                        cmd.Parameters.AddWithValue("@LedgerID", ledger.LedgerID);
                        cmd.Parameters.AddWithValue("@LedgerName", ledger.LedgerName);
                        cmd.Parameters.AddWithValue("@Alias", string.IsNullOrEmpty(ledger.Alias) ? DBNull.Value : (object)ledger.Alias);
                        cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(ledger.Description) ? DBNull.Value : (object)ledger.Description);
                        cmd.Parameters.AddWithValue("@Notes", string.IsNullOrEmpty(ledger.Notes) ? DBNull.Value : (object)ledger.Notes);
                        cmd.Parameters.AddWithValue("@GroupID", ledger.GroupID);
                        cmd.Parameters.AddWithValue("@OpnDebit", ledger.OpnDebit);
                        cmd.Parameters.AddWithValue("@OpnCredit", ledger.OpnCredit);
                        cmd.Parameters.AddWithValue("@ProvideBankDetails", ledger.ProvideBankDetails ?? false);
                        cmd.Parameters.AddWithValue("@GstApplicable", ledger.GstApplicable ?? false);
                        cmd.Parameters.AddWithValue("@VatApplicable", ledger.VatApplicable ?? false);
                        cmd.Parameters.AddWithValue("@InventoryValuesAffected", ledger.InventoryValuesAffected ?? false);
                        cmd.Parameters.AddWithValue("@MaintainBillWiseDetails", ledger.MaintainBillWiseDetails ?? false);
                        cmd.Parameters.AddWithValue("@PriceLevelApplicable", ledger.PriceLevelApplicable ?? false);

                        object scalar = cmd.ExecuteScalar();
                        result = scalar != null && Convert.ToInt32(scalar) > 0;
                    }
                }
                catch
                {
                    throw sqlEx;
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

            return result;
        }

        // Method to update an existing ledger
        public bool UpdateLedger(Ledger ledger)
        {
            bool result = false;

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_Ledger", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "UPDATE");
                    cmd.Parameters.AddWithValue("@CompanyID", ledger.CompanyID);
                    cmd.Parameters.AddWithValue("@BranchID", ledger.BranchID);
                    cmd.Parameters.AddWithValue("@LedgerID", ledger.LedgerID);
                    cmd.Parameters.AddWithValue("@LedgerName", ledger.LedgerName);
                    cmd.Parameters.AddWithValue("@Alias", string.IsNullOrEmpty(ledger.Alias) ? DBNull.Value : (object)ledger.Alias);
                    cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(ledger.Description) ? DBNull.Value : (object)ledger.Description);
                    cmd.Parameters.AddWithValue("@Notes", string.IsNullOrEmpty(ledger.Notes) ? DBNull.Value : (object)ledger.Notes);
                    cmd.Parameters.AddWithValue("@GroupID", ledger.GroupID);
                    cmd.Parameters.AddWithValue("@OpnDebit", ledger.OpnDebit);
                    cmd.Parameters.AddWithValue("@OpnCredit", ledger.OpnCredit);
                    cmd.Parameters.AddWithValue("@ProvideBankDetails", ledger.ProvideBankDetails ?? false);
                    cmd.Parameters.AddWithValue("@GstApplicable", ledger.GstApplicable ?? false);
                    cmd.Parameters.AddWithValue("@VatApplicable", ledger.VatApplicable ?? false);
                    cmd.Parameters.AddWithValue("@InventoryValuesAffected", ledger.InventoryValuesAffected ?? false);
                    cmd.Parameters.AddWithValue("@MaintainBillWiseDetails", ledger.MaintainBillWiseDetails ?? false);
                    cmd.Parameters.AddWithValue("@PriceLevelApplicable", ledger.PriceLevelApplicable ?? false);

                    object scalar = cmd.ExecuteScalar();
                    result = scalar != null && Convert.ToInt32(scalar) > 0;
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

            return result;
        }

        // Method to delete a ledger
        public bool DeleteLedger(int ledgerId)
        {
            bool result = false;

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_Ledger", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "DELETE");
                    cmd.Parameters.AddWithValue("@LedgerID", ledgerId);

                    object scalar = cmd.ExecuteScalar();
                    result = scalar != null && Convert.ToInt32(scalar) > 0;
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

            return result;
        }

        // Method to get the next available LedgerID
        public int GetNextLedgerID()
        {
            int nextId = 1; // Default starting ID

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Ledger, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETNEXTID");

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read() && reader[0] != DBNull.Value)
                        {
                            nextId = Convert.ToInt32(reader[0]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetNextLedgerID: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return nextId;
        }

        // Method to get a ledger by ID
        public DataRow GetLedgerById(int ledgerId)
        {
            DataTable dtResult = new DataTable();

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_Ledger", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");
                    cmd.Parameters.AddWithValue("@LedgerID", ledgerId);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        adapt.Fill(dtResult);
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

            return dtResult.Rows.Count > 0 ? dtResult.Rows[0] : null;
        }

        // Method to check if a ledger name already exists
        public bool IsLedgerNameExists(string ledgerName, int branchId, int excludeLedgerId = 0)
        {
            bool exists = false;

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Ledger, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "CHECKNAMEEXISTS");
                    cmd.Parameters.AddWithValue("@LedgerName", ledgerName);
                    cmd.Parameters.AddWithValue("@BranchID", branchId);
                    cmd.Parameters.AddWithValue("@ExcludeLedgerID", excludeLedgerId);

                    object result = cmd.ExecuteScalar();
                    exists = (result != null && Convert.ToInt32(result) > 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in IsLedgerNameExists: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return exists;
        }

        // Method to check if an alias already exists (ignoring empty aliases)
        public bool IsLedgerAliasExists(string alias, int branchId, int excludeLedgerId = 0)
        {
            if (string.IsNullOrWhiteSpace(alias))
                return false;

            bool exists = false;

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Ledger, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "CHECKALIASEXISTS");
                    cmd.Parameters.AddWithValue("@Alias", alias);
                    cmd.Parameters.AddWithValue("@BranchID", branchId);
                    cmd.Parameters.AddWithValue("@ExcludeLedgerID", excludeLedgerId);

                    object result = cmd.ExecuteScalar();
                    exists = (result != null && Convert.ToInt32(result) > 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in IsLedgerAliasExists: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return exists;
        }

        // Method to recursively get the account type (CUSTOMER, SUPPLIER, or OTHER) using stored procedure
        public string GetLedgerAccountType(long ledgerId)
        {
            string accountType = "OTHER";

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Ledger, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETACCOUNTTYPE");
                    cmd.Parameters.AddWithValue("@LedgerID", ledgerId);

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        accountType = result.ToString().Trim().ToUpper();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in GetLedgerAccountType via stored procedure: " + ex.Message);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return accountType;
        }
    }
}

