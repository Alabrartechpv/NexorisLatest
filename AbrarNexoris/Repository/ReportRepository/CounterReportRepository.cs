using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ModelClass;

namespace Repository.ReportRepository
{
    /// <summary>
    /// Repository to fetch data for Counter Reports.
    /// </summary>
    public class CounterReportRepository : BaseRepostitory
    {
        /// <summary>
        /// Get Counter Report Data from ShiftClosing table
        /// </summary>
        public List<CounterReportModel> GetCounterReportData(DateTime fromDate, DateTime toDate, string counterName, int userId)
        {
            List<CounterReportModel> list = new List<CounterReportModel>();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_CounterReport, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETREPORT");
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
                    cmd.Parameters.AddWithValue("@Counter", string.IsNullOrEmpty(counterName) ? (object)DBNull.Value : counterName);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                list.Add(new CounterReportModel
                                {
                                    BillNo = Convert.ToInt64(row["BillNo"]),
                                    BillDate = Convert.ToDateTime(row["BillDate"]),
                                    Counter = row["Counter"]?.ToString() ?? "",
                                    UserName = row["UserName"]?.ToString() ?? "",
                                    CustomerName = row["CustomerName"]?.ToString() ?? "",
                                    PaymodeName = row["PaymodeName"]?.ToString() ?? "",
                                    CashMode = row["CashMode"]?.ToString() ?? "",
                                    SubTotal = row["SubTotal"] != DBNull.Value ? Convert.ToDecimal(row["SubTotal"]) : 0,
                                    DiscountAmt = row["DiscountAmt"] != DBNull.Value ? Convert.ToDecimal(row["DiscountAmt"]) : 0,
                                    TaxAmt = row["TaxAmt"] != DBNull.Value ? Convert.ToDecimal(row["TaxAmt"]) : 0,
                                    NetAmount = row["NetAmount"] != DBNull.Value ? Convert.ToDecimal(row["NetAmount"]) : 0,
                                    Status = row["Status"]?.ToString() ?? ""
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving counter report data. {ex.Message}", ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return list;
        }

        /// <summary>
        /// Get distinct counter names that have closing records
        /// </summary>
        public List<string> GetDistinctCounters()
        {
            List<string> counters = new List<string>();
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_CounterReport, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETDISTINCTCOUNTERS");
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            counters.Add(reader["Counter"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting distinct counters: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return counters;
        }
    }
}
