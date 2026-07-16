using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Repository.ReportRepository
{
    /// <summary>
    /// Repository for Customer-wise Sales Summary Report
    /// </summary>
    public class CustomerwiseSalesSummaryRepo : BaseRepostitory
    {
        /// <summary>
        /// Retrieves the Customer-wise Sales Summary data from the stored procedure
        /// </summary>
        public List<CustomerwiseSalesSummaryItem> GetCustomerwiseSalesSummary(CustomerwiseSalesSummaryFilter filter)
        {
            List<CustomerwiseSalesSummaryItem> reportData = new List<CustomerwiseSalesSummaryItem>();
            
            try
            {
                if (DataConnection.State == ConnectionState.Closed)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_CustomerwiseSalesSummaryReport, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 180; // 3 minutes timeout

                    cmd.Parameters.AddWithValue("@CompanyId", filter.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", filter.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", filter.FinYearId);
                    cmd.Parameters.AddWithValue("@FromDate", filter.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", filter.ToDate);
                    
                    cmd.Parameters.AddWithValue("@CustomerId", filter.CustomerId.HasValue && filter.CustomerId.Value > 0 ? filter.CustomerId.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@GroupId", filter.GroupId.HasValue && filter.GroupId.Value > 0 ? filter.GroupId.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CategoryId", filter.CategoryId.HasValue && filter.CategoryId.Value > 0 ? filter.CategoryId.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SearchQuery", string.IsNullOrEmpty(filter.SearchQuery) ? (object)DBNull.Value : filter.SearchQuery);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                reportData.Add(new CustomerwiseSalesSummaryItem
                                {
                                    SlNo               = row["SlNo"] != DBNull.Value ? Convert.ToInt32(row["SlNo"]) : 0,
                                    BillDate           = row["BillDate"] != DBNull.Value ? Convert.ToDateTime(row["BillDate"]) : DateTime.MinValue,
                                    CustomerId         = row["CustomerId"] != DBNull.Value ? Convert.ToInt32(row["CustomerId"]) : 0,
                                    CustomerName       = row["CustomerName"]?.ToString() ?? "",
                                    Phone              = row["Phone"]?.ToString() ?? "",
                                    ItemId             = row["ItemId"] != DBNull.Value ? Convert.ToInt32(row["ItemId"]) : 0,
                                    Barcode            = row["Barcode"]?.ToString() ?? "",
                                    ItemName           = row["ItemName"]?.ToString() ?? "",
                                    GroupName          = row["GroupName"]?.ToString() ?? "",
                                    CategoryName       = row["CategoryName"]?.ToString() ?? "",
                                    BaseUnitName       = row["BaseUnitName"]?.ToString() ?? "",
                                    TotalQtySold       = row["TotalQtySold"] != DBNull.Value ? Convert.ToDecimal(row["TotalQtySold"]) : 0,
                                    TotalSalesAmount   = row["TotalSalesAmount"] != DBNull.Value ? Convert.ToDecimal(row["TotalSalesAmount"]) : 0
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving Customer-wise Sales Summary data: {ex.Message}", ex);
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
