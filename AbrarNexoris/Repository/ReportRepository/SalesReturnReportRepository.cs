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
    public class SalesReturnReportRepository : BaseRepostitory
    {
        /// <summary>
        /// Get Sales Return Report Details for a specific Return Number using the stored procedure
        /// </summary>
        /// <param name="sReturnNo">Sales Return Number to get details for</param>
        /// <returns>SalesReturnReportData containing master and detail information</returns>
        public SalesReturnReportData GetSalesReturnReportDetails(int sReturnNo)
        {
            SalesReturnReportData reportData = new SalesReturnReportData();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_SalesReturn_Details_for_Report, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SReturnNo", sReturnNo);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);

                        // First table contains master data (SReturnMaster table)
                        if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            var masterRow = ds.Tables[0].Rows[0];
                            reportData.Master = new SalesReturnReportMaster
                            {
                                SReturnNo = Convert.ToInt32(masterRow["SReturnNo"]),
                                SReturnDate = Convert.ToDateTime(masterRow["SReturnDate"]),
                                InvoiceNo = masterRow["InvoiceNo"]?.ToString(),
                                InvoiceDate = Convert.ToDateTime(masterRow["InvoiceDate"]),
                                CustomerName = masterRow["CustomerName"]?.ToString(),
                                Paymode = masterRow["Paymode"]?.ToString(),
                                SubTotal = Convert.ToDouble(masterRow["SubTotal"]),
                                GrandTotal = Convert.ToDouble(masterRow["GrandTotal"])
                            };
                        }

                        // Second table contains detail data (SReturnDetails table)
                        if (ds.Tables.Count > 1 && ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                        {
                            reportData.Details = new List<SalesReturnReportDetail>();

                            foreach (DataRow detailRow in ds.Tables[1].Rows)
                            {
                                var detail = new SalesReturnReportDetail
                                {
                                    SReturnNo = sReturnNo,
                                    SlNo = Convert.ToInt32(detailRow["SlNo"]),
                                    ItemName = detailRow["ItemName"]?.ToString(),
                                    Unit = detailRow["Unit"]?.ToString(),
                                    Packing = detailRow["Packing"]?.ToString(),
                                    Qty = Convert.ToDouble(detailRow["Qty"]),
                                    SalesPrice = Convert.ToDouble(detailRow["SalesPrice"]),
                                    // Database stores percentage values as basis points (multiplied by 100)
                                    TaxPer = Convert.ToDouble(detailRow["TaxPer"]) / 100,
                                    TaxAmt = Convert.ToDouble(detailRow["TaxAmt"]),
                                    Amount = Convert.ToDouble(detailRow["Amount"]),
                                    Reason = detailRow["Reason"]?.ToString()
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
                throw new Exception($"Error retrieving sales return report details for Return No: {sReturnNo}. {ex.Message}", ex);
            }
            finally
            {
                DataConnection.Close();
            }

            return reportData;
        }

        /// <summary>
        /// Get all sales return records for a date range (for master grid) using stored procedure
        /// </summary>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <param name="branchId">Branch ID</param>
        /// <returns>List of sales return records</returns>
        public List<SalesReturnReportMaster> GetSalesReturnRecords(DateTime fromDate, DateTime toDate, int branchId = 0)
        {
            List<SalesReturnReportMaster> returns = new List<SalesReturnReportMaster>();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_SalesReturn_Master_for_Report, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    cmd.Parameters.AddWithValue("@BranchId", branchId); // Always pass BranchId

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            // Manual mapping to handle case-sensitive column names
                            foreach (DataRow row in dt.Rows)
                            {
                                returns.Add(new SalesReturnReportMaster
                                {
                                    SReturnNo = Convert.ToInt32(row["SReturnNo"]),
                                    SReturnDate = Convert.ToDateTime(row["SReturnDate"]),
                                    InvoiceNo = row["InvoiceNo"]?.ToString() ?? "",
                                    InvoiceDate = Convert.ToDateTime(row["InvoiceDate"]),
                                    CustomerName = row["CustomerName"]?.ToString() ?? "",
                                    Paymode = row["Paymode"]?.ToString() ?? "",
                                    SubTotal = Convert.ToDouble(row["SubTotal"]),
                                    GrandTotal = Convert.ToDouble(row["GrandTotal"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving sales return records. {ex.Message}", ex);
            }
            finally
            {
                DataConnection.Close();
            }

            return returns;
        }
    }
}
