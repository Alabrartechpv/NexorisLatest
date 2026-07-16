using System;
using System.Data;
using System.Data.SqlClient;
using ModelClass.Settings;

namespace Repository.SettingsRepo
{
    public class FinancialYearRepository : BaseRepostitory
    {
        /// <summary>
        /// Gets the current active financial year for a company from the FinancialYear table.
        /// Returns null if no row with CurFinYear = 1 exists. Never fabricates a fallback ID.
        /// Exceptions propagate to the caller so the UI can block the closing flow.
        /// </summary>
        public FinancialYearModel GetCurrentFinancialYear(int companyId)
        {
            FinancialYearModel model = null;

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                string sql = "SELECT CompanyID, FinYearFrom, FinYearTo, FinYearID, CurFinYear FROM FinancialYear WHERE CompanyID = @CompanyID AND CurFinYear = 1";
                using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", companyId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = new FinancialYearModel
                            {
                                CompanyID = Convert.ToInt32(reader["CompanyID"]),
                                FinYearFrom = Convert.ToDateTime(reader["FinYearFrom"]),
                                FinYearTo = Convert.ToDateTime(reader["FinYearTo"]),
                                FinYearID = Convert.ToInt32(reader["FinYearID"]),
                                CurFinYear = Convert.ToInt32(reader["CurFinYear"])
                            };
                        }
                    }
                }
                // No fallback to CompanyInfo — closing requires an actual FinancialYear row.
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return model;
        }

        /// <summary>
        /// Checks if there are any active/open counter sessions in this company (all branches)
        /// </summary>
        public bool HasOpenSessions(int companyId)
        {
            bool hasOpen = false;

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                string sql = "SELECT COUNT(*) FROM CounterSessions WHERE CompanyId = @CompanyID AND Status = 'Open'";
                using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", companyId);
                    int count = (int)cmd.ExecuteScalar();
                    hasOpen = count > 0;
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return hasOpen;
        }

        /// <summary>
        /// Calls the stored procedure _POS_FinancialYearClosing to perform the company-wide rollover.
        /// BranchId is not passed — the SP processes every branch recorded in TrackTrans.
        /// </summary>
        public string PerformFinancialYearClosing(
            int companyId, int oldYearId, int newYearId,
            DateTime newFrom, DateTime newTo,
            string username, int userId, int counterId)
        {
            string result = "Failed to run rollover.";

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand("dbo._POS_FinancialYearClosing", (SqlConnection)DataConnection))
                {
                    cmd.CommandType    = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300; // 5-minute timeout for large year-end operations
                    cmd.Parameters.AddWithValue("@CompanyId",    companyId);
                    cmd.Parameters.AddWithValue("@OldFinYearId", oldYearId);
                    cmd.Parameters.AddWithValue("@NewFinYearId", newYearId);
                    cmd.Parameters.AddWithValue("@NewYearFrom",  newFrom.Date);
                    cmd.Parameters.AddWithValue("@NewYearTo",    newTo.Date);
                    cmd.Parameters.AddWithValue("@UserName",     username);
                    cmd.Parameters.AddWithValue("@UserID",       userId);
                    cmd.Parameters.AddWithValue("@CounterID",    counterId);

                    object spResult = cmd.ExecuteScalar();
                    result = spResult?.ToString() ?? "No result returned.";
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return result;
        }
    }
}
