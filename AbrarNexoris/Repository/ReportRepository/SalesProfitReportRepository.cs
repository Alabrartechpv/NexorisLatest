using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Repository.ReportRepository
{
    public class SalesProfitReportRepository : BaseRepostitory
    {
        /// <summary>
        /// Get Sales Profit Report using the SalesProfitReport stored procedure
        /// </summary>
        /// <param name="billNo">Bill Number (0 for all bills)</param>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <returns>List of SalesProfitReport</returns>
        public List<SalesProfitReport> GetSalesProfitReport(int billNo, DateTime fromDate, DateTime toDate)
        {
            List<SalesProfitReport> list = new List<SalesProfitReport>();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._SalesProfitReport, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Billno", billNo);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                list.Add(new SalesProfitReport
                                {
                                    BillNo = Convert.ToInt32(row["BillNo"]),
                                    BillDate = Convert.ToDateTime(row["BillDate"]),
                                    BillAmount = Convert.ToDouble(row["BillAmount"]),
                                    Profit = Convert.ToDouble(row["Profit"]),
                                    PayMode = row["PayMode"]?.ToString() ?? "",
                                    CashMode = row["CashMode"]?.ToString() ?? ""
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving sales profit report. {ex.Message}", ex);
            }
            finally
            {
                DataConnection.Close();
            }

            return list;
        }
    }
}
