using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Repository.ReportRepository
{
    public class SalesmanwiseSalesSummaryRepo : BaseRepostitory
    {
        public List<SalesmanwiseSalesSummaryItem> GetSalesmanwiseSalesSummary(SalesmanwiseSalesSummaryFilter filter)
        {
            List<SalesmanwiseSalesSummaryItem> reportData = new List<SalesmanwiseSalesSummaryItem>();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_SalesmanwiseSalesSummaryReport, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId",   filter.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId",    filter.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId",   filter.FinYearId);
                    cmd.Parameters.AddWithValue("@FromDate",    filter.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate",      filter.ToDate);
                    cmd.Parameters.AddWithValue("@SalesmanId",  filter.SalesmanId.HasValue && filter.SalesmanId.Value > 0 ? (object)filter.SalesmanId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@SearchQuery", string.IsNullOrEmpty(filter.SearchQuery) ? (object)DBNull.Value : filter.SearchQuery);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                reportData.Add(new SalesmanwiseSalesSummaryItem
                                {
                                    SlNo               = row["SlNo"] != DBNull.Value ? Convert.ToInt64(row["SlNo"]) : 0,
                                    SalesmanId         = row["SalesmanId"] != DBNull.Value ? Convert.ToInt32(row["SalesmanId"]) : (int?)null,
                                    SalesmanName       = row["SalesmanName"]?.ToString() ?? "",
                                    Email              = row["Email"]?.ToString() ?? "",
                                    InvoiceCount       = row["InvoiceCount"] != DBNull.Value ? Convert.ToInt32(row["InvoiceCount"]) : 0,
                                    TotalQtySold       = row["TotalQtySold"] != DBNull.Value ? Convert.ToDouble(row["TotalQtySold"]) : 0.0,
                                    TotalSalesAmount   = row["TotalSalesAmount"] != DBNull.Value ? Convert.ToDouble(row["TotalSalesAmount"]) : 0.0,
                                    CommissionPercent  = 0.0, // Calculated dynamically by C# or UI input
                                    CommissionAmount   = 0.0  // Calculated dynamically by C# or UI input
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving salesman-wise sales summary report: " + ex.Message, ex);
            }
            finally
            {
                DataConnection.Close();
            }

            return reportData;
        }
    }
}
