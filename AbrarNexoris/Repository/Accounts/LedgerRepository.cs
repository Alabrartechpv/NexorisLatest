using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using ModelClass.Accounts;

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
                string query = @"
SELECT
    l.LedgerID,
    CASE
        WHEN ag.GroupType IN ('LIABILITIES', 'INCOME')
            OR ag.GroupID IN (1, 2, 3, 8, 11, 13, 17, 20, 23, 24, 25, 26, 27, 28, 29)
            THEN
                (ISNULL(l.OpnCredit, 0) + ISNULL(SUM(vd.Credit), 0))
                - (ISNULL(l.OpnDebit, 0) + ISNULL(SUM(vd.Debit), 0))
        ELSE
                (ISNULL(l.OpnDebit, 0) + ISNULL(SUM(vd.Debit), 0))
                - (ISNULL(l.OpnCredit, 0) + ISNULL(SUM(vd.Credit), 0))
    END AS Balance
FROM LedgerMaster l
INNER JOIN AccountGroupMaster ag
    ON l.GroupID = ag.GroupID
    AND ag.BranchID = l.BranchID
LEFT JOIN Vouchers vd
    ON l.LedgerID = vd.LedgerID
    AND vd.CompanyID = @CompanyId
    AND vd.BranchID = @BranchId
    AND vd.FinYearID = @FinYearId
    AND vd.VoucherDate <= @ToDate
    AND ISNULL(vd.CancelFlag, 0) = 0
WHERE l.BranchID = @BranchId
GROUP BY l.LedgerID, ag.GroupType, ag.GroupID, l.OpnDebit, l.OpnCredit;";

                using (SqlCommand cmd = new SqlCommand(query, (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

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
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return balances;
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
                // Try stored procedure first
                try
                {
                    using (SqlCommand cmd = new SqlCommand("POS_Ledger", (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@_Operation", "GETNEXTID");

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read() && reader["NextID"] != DBNull.Value)
                            {
                                nextId = Convert.ToInt32(reader["NextID"]);
                                return nextId;
                            }
                        }
                    }
                }
                catch
                {
                    // Procedure failed, try direct SQL as fallback
                    Console.WriteLine("Procedure call failed, using direct SQL as fallback");
                }

                // Fallback to direct SQL query if procedure fails
                string query = "SELECT ISNULL(MAX(CAST(LedgerID AS INT)), 0) + 1 FROM LedgerMaster";

                using (SqlCommand cmd = new SqlCommand(query, (SqlConnection)DataConnection))
                {
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        nextId = Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetNextLedgerID: {ex.Message}");
                throw ex;
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
                string query = "SELECT COUNT(1) FROM LedgerMaster WHERE LedgerName = @LedgerName AND BranchID = @BranchID AND LedgerID != @ExcludeLedgerID";
                using (SqlCommand cmd = new SqlCommand(query, (SqlConnection)DataConnection))
                {
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
                throw ex;
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
                string query = "SELECT COUNT(1) FROM LedgerMaster WHERE Alias = @Alias AND BranchID = @BranchID AND LedgerID != @ExcludeLedgerID";
                using (SqlCommand cmd = new SqlCommand(query, (SqlConnection)DataConnection))
                {
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
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return exists;
        }

        // Method to recursively get the account type (CUSTOMER, SUPPLIER, or OTHER) using CTE
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
                string query = @"
WITH GroupHierarchy AS (
    SELECT GroupID, ParentGroupID
    FROM AccountGroupMaster
    WHERE GroupID = (SELECT GroupID FROM LedgerMaster WHERE LedgerID = @LedgerID)
    UNION ALL
    SELECT p.GroupID, p.ParentGroupID
    FROM GroupHierarchy h
    INNER JOIN AccountGroupMaster p ON h.ParentGroupID = p.GroupID
)
SELECT 
    CASE 
        WHEN EXISTS (SELECT 1 FROM GroupHierarchy WHERE GroupID = 16) THEN 'CUSTOMER'
        WHEN EXISTS (SELECT 1 FROM GroupHierarchy WHERE GroupID = 17) THEN 'SUPPLIER'
        ELSE 'OTHER'
    END AS AccountType;";

                using (SqlCommand cmd = new SqlCommand(query, (SqlConnection)DataConnection))
                {
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
                throw ex;
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

