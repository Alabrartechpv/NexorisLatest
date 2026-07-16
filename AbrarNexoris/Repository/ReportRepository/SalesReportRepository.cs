using ModelClass;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ReportRepository
{
    public class SalesReportRepository : BaseRepostitory
    {
        public List<Sales_Daily> getSales(Requestfrm request)
        {
            List<Sales_Daily> list = new List<Sales_Daily>();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_SALES_REPORT, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                    cmd.Parameters.AddWithValue("@BranchId", request.BranchId);
                    cmd.Parameters.AddWithValue("@_Operation", "DAILYSALES");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            list = ds.Tables[0].ToListOfObject<Sales_Daily>();
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

            }
            return list;
        }

        /// <summary>
        /// Get Sales Report Details for a specific Bill Number using the stored procedure
        /// </summary>
        /// <param name="billNo">Bill Number to get details for</param>
        /// <param name="fromDate">Start date for filtering</param>
        /// <param name="toDate">End date for filtering</param>
        /// <returns>SalesReportData containing master and detail information</returns>
        public SalesReportData GetSalesReportDetails(int billNo, DateTime fromDate, DateTime toDate)
        {
            SalesReportData reportData = new SalesReportData();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_Sales_Details_for_Report, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    cmd.Parameters.AddWithValue("@BillNo", billNo);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);

                        // First table contains master data (SMaster table)
                        if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            var masterRow = ds.Tables[0].Rows[0];
                            reportData.Master = new SalesReportMaster
                            {
                                BillNo = Convert.ToInt32(masterRow["BillNo"]),
                                BillDate = Convert.ToDateTime(masterRow["BillDate"]),
                                CustomerName = masterRow["customername"]?.ToString(),
                                PaymodeName = masterRow["paymodename"]?.ToString(),
                                // TaxPer removed from master table in stored procedure
                                TaxPer = 0, // Not available in master
                                TaxAmt = Convert.ToDouble(masterRow["TaxAmt"]),
                                SubTotal = Convert.ToDouble(masterRow["SubTotal"]),
                                NetAmount = Convert.ToDouble(masterRow["NetAmount"]),
                                Profit = Convert.ToDouble(masterRow["Profit"])
                            };
                        }

                        // Second table contains detail data (SDetails table)
                        if (ds.Tables.Count > 1 && ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                        {
                            reportData.Details = new List<SalesReportDetail>();

                            foreach (DataRow detailRow in ds.Tables[1].Rows)
                            {
                                var detail = new SalesReportDetail
                                {
                                    BillNo = billNo, // Use the parameter value
                                    SlNo = Convert.ToInt32(detailRow["slno"]),
                                    ItemName = detailRow["ItemName"]?.ToString(),
                                    Barcode = detailRow["barcode"]?.ToString(),
                                    Unit = detailRow["Unit"]?.ToString(),
                                    Packing = detailRow["Packing"]?.ToString(),
                                    Qty = Convert.ToDouble(detailRow["qty"]),
                                    UnitPrice = Convert.ToDouble(detailRow["UnitPrice"]),
                                    Amount = Convert.ToDouble(detailRow["amount"]),
                                    // Database stores percentage values as basis points (multiplied by 100)
                                    // e.g., 3000 = 30%, 1200 = 12%, 3750 = 37.5%
                                    // So we need to divide by 100 to get actual percentage
                                    MarginPer = Convert.ToDouble(detailRow["MarginPer"]) / 100,
                                    Profit = Convert.ToDouble(detailRow["MarginAmt"]),  // Renamed from MarginAmt to Profit
                                    TaxPer = Convert.ToDouble(detailRow["TaxPer"]) / 100,
                                    TaxAmt = Convert.ToDouble(detailRow["TaxAmt"]),
                                    TotalAmount = Convert.ToDouble(detailRow["TotalAmount"])
                                };
                                reportData.Details.Add(detail);
                            }

                            // Link details to master
                            if (reportData.Master != null)
                            {
                                reportData.Master.Details = reportData.Details;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving sales report details for Bill No: {billNo}. {ex.Message}", ex);
            }
            finally
            {
                DataConnection.Close();
            }

            return reportData;
        }

        /// <summary>
        /// Get all sales bills for a date range (for master grid) using stored procedure
        /// </summary>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <param name="branchId">Branch ID</param>
        /// <returns>List of sales bills</returns>
        public List<SalesReportMaster> GetSalesBills(DateTime fromDate, DateTime toDate, int branchId = 0)
        {
            List<SalesReportMaster> bills = new List<SalesReportMaster>();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_Sales_Master_for_Report, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    if (branchId > 0)
                    {
                        cmd.Parameters.AddWithValue("@BranchId", branchId);
                    }

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            // Manual mapping to handle case-sensitive column names
                            foreach (DataRow row in dt.Rows)
                            {
                                bills.Add(new SalesReportMaster
                                {
                                    BillNo = Convert.ToInt32(row["BillNo"]),
                                    BillDate = Convert.ToDateTime(row["BillDate"]),
                                    CustomerName = row["customername"]?.ToString() ?? "",
                                    PaymodeName = row["paymodename"]?.ToString() ?? "",
                            CashMode = row.Table.Columns.Contains("CashMode") ? (row["CashMode"]?.ToString() ?? "") : "",
                                    TaxAmt = Convert.ToDouble(row["TaxAmt"]),
                                    SubTotal = Convert.ToDouble(row["SubTotal"]),
                                    NetAmount = Convert.ToDouble(row["NetAmount"]),
                                    Profit = Convert.ToDouble(row["Profit"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving sales bills. {ex.Message}", ex);
            }
            finally
            {
                DataConnection.Close();
            }

            return bills;
        }
    }
}
