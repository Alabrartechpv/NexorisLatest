using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Repository.ReportRepository
{
    public class LowStockAlertRepo : BaseRepostitory
    {
        public List<LowStockAlertItem> GetLowStockAlerts(LowStockAlertFilter filter)
        {
            List<LowStockAlertItem> reportData = new List<LowStockAlertItem>();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_LowStockAlertReport, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId",   filter.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId",    filter.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId",   filter.FinYearId);
                    cmd.Parameters.AddWithValue("@GroupId",     filter.GroupId.HasValue && filter.GroupId.Value > 0 ? filter.GroupId.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CategoryId",  filter.CategoryId.HasValue && filter.CategoryId.Value > 0 ? filter.CategoryId.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SearchQuery", string.IsNullOrEmpty(filter.SearchQuery) ? (object)DBNull.Value : filter.SearchQuery);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                reportData.Add(new LowStockAlertItem
                                {
                                    SlNo         = row["SlNo"] != DBNull.Value ? Convert.ToInt64(row["SlNo"]) : 0,
                                    ItemId       = row["ItemId"] != DBNull.Value ? Convert.ToInt32(row["ItemId"]) : 0,
                                    Barcode      = row["Barcode"]?.ToString() ?? "",
                                    ItemName     = row["ItemName"]?.ToString() ?? "",
                                    GroupName    = row["GroupName"]?.ToString() ?? "",
                                    CategoryName = row["CategoryName"]?.ToString() ?? "",
                                    BaseUnitName = row["BaseUnitName"]?.ToString() ?? "",
                                    CostPrice    = row["CostPrice"] != DBNull.Value ? Convert.ToDouble(row["CostPrice"]) : 0.0,
                                    RetailPrice  = row["RetailPrice"] != DBNull.Value ? Convert.ToDouble(row["RetailPrice"]) : 0.0,
                                    ReorderLevel = row["ReorderLevel"] != DBNull.Value ? Convert.ToDouble(row["ReorderLevel"]) : 0.0,
                                    CurrentStock = row["CurrentStock"] != DBNull.Value ? Convert.ToDouble(row["CurrentStock"]) : 0.0,
                                    ShortageQty  = row["ShortageQty"] != DBNull.Value ? Convert.ToDouble(row["ShortageQty"]) : 0.0
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving low stock alert report data: " + ex.Message, ex);
            }
            finally
            {
                DataConnection.Close();
            }

            return reportData;
        }
    }
}
