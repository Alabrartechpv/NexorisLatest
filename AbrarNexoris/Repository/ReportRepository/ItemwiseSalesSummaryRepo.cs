using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Repository.ReportRepository
{
    /// <summary>
    /// Repository for Item-wise Sales & Profit Summary Report
    /// </summary>
    public class ItemwiseSalesSummaryRepo : BaseRepostitory
    {
        /// <summary>
        /// Retrieves the Item-wise Sales and Profit Summary data from the stored procedure
        /// </summary>
        public List<ItemwiseSalesSummaryItem> GetItemwiseSalesSummary(ItemwiseSalesSummaryFilter filter)
        {
            List<ItemwiseSalesSummaryItem> reportData = new List<ItemwiseSalesSummaryItem>();
            
            try
            {
                if (DataConnection.State == ConnectionState.Closed)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemwiseSalesSummaryReport, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 180; // 3 minutes timeout

                    cmd.Parameters.AddWithValue("@CompanyId", filter.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", filter.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", filter.FinYearId);
                    cmd.Parameters.AddWithValue("@FromDate", filter.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", filter.ToDate);
                    
                    cmd.Parameters.AddWithValue("@GroupId", filter.GroupId.HasValue && filter.GroupId.Value > 0 ? filter.GroupId.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CategoryId", filter.CategoryId.HasValue && filter.CategoryId.Value > 0 ? filter.CategoryId.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BarcodeContains", string.IsNullOrEmpty(filter.BarcodeContains) ? (object)DBNull.Value : filter.BarcodeContains);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                reportData.Add(new ItemwiseSalesSummaryItem
                                {
                                    ItemId            = row["ItemId"] != DBNull.Value ? Convert.ToInt32(row["ItemId"]) : 0,
                                    Barcode           = row["Barcode"]?.ToString() ?? "",
                                    ItemName          = row["ItemName"]?.ToString() ?? "",
                                    GroupName         = row["GroupName"]?.ToString() ?? "",
                                    CategoryName      = row["CategoryName"]?.ToString() ?? "",
                                    BaseUnitName      = row["BaseUnitName"]?.ToString() ?? "",
                                    TotalQtySold      = row["TotalQtySold"] != DBNull.Value ? Convert.ToDecimal(row["TotalQtySold"]) : 0,
                                    AvgUnitPrice      = row["AvgUnitPrice"] != DBNull.Value ? Convert.ToDecimal(row["AvgUnitPrice"]) : 0,
                                    TotalSalesAmount  = row["TotalSalesAmount"] != DBNull.Value ? Convert.ToDecimal(row["TotalSalesAmount"]) : 0,
                                    TotalCostValue    = row["TotalCostValue"] != DBNull.Value ? Convert.ToDecimal(row["TotalCostValue"]) : 0,
                                    TotalMarginProfit = row["TotalMarginProfit"] != DBNull.Value ? Convert.ToDecimal(row["TotalMarginProfit"]) : 0
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving Item-wise Sales Summary data: {ex.Message}", ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return reportData;
        }
    }
}
