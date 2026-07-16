using ModelClass.Report;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Repository.ReportRepository
{
    public class SalesmanIncentiveReportRepository : BaseRepostitory
    {
        public SalesmanIncentiveReportData GetSalesmanIncentiveReport(SalesmanIncentiveReportFilter filter)
        {
            SalesmanIncentiveReportData reportData = new SalesmanIncentiveReportData();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_Salesman_Incentive_Report, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", filter.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", filter.ToDate);
                    cmd.Parameters.AddWithValue("@CompanyId", filter.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", filter.BranchId);
                    cmd.Parameters.AddWithValue("@SalesmanId", filter.SalesmanId);
                    cmd.Parameters.AddWithValue("@UserId", filter.UserId);
                    cmd.Parameters.AddWithValue("@GroupId", filter.GroupId);
                    cmd.Parameters.AddWithValue("@CategoryId", filter.CategoryId);
                    cmd.Parameters.AddWithValue("@BrandId", filter.BrandId);
                    cmd.Parameters.AddWithValue("@VendorId", filter.VendorId);
                    cmd.Parameters.AddWithValue("@IncentivePercent", filter.IncentivePercent);
                    cmd.Parameters.AddWithValue("@IncludeDetails", filter.IncludeDetails);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);

                        if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            reportData.Summary = ds.Tables[0].ToListOfObject<SalesmanIncentiveSummary>();
                        }

                        if (ds.Tables.Count > 1 && ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                        {
                            reportData.Details = ds.Tables[1].ToListOfObject<SalesmanIncentiveDetail>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving salesman incentive report. " + ex.Message, ex);
            }
            finally
            {
                DataConnection.Close();
            }

            return reportData;
        }
    }
}
