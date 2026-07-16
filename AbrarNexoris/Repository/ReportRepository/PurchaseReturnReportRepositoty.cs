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
    public class PurchaseReturnReportRepository : BaseRepostitory
    {
        /// <summary>
        /// Get Purchase Return Report Details for a specific Return Number using the stored procedure
        /// </summary>
        /// <param name="pReturnNo">Purchase Return Number to get details for</param>
        /// <returns>PurchaseReturnReportData containing master and detail information</returns>
        public PurchaseReturnReportData GetPurchaseReturnReportDetails(int pReturnNo)
        {
            PurchaseReturnReportData reportData = new PurchaseReturnReportData();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_PurchaseReturn_Details_for_Report, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PReturnNo", pReturnNo);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);

                        // First table contains master data (PReturnMaster table)
                        if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            var masterRow = ds.Tables[0].Rows[0];
                            reportData.Master = new PurchaseReturnReportMaster
                            {
                                PReturnNo = masterRow["PReturnNo"] != DBNull.Value ? Convert.ToInt32(masterRow["PReturnNo"]) : 0,
                                PReturnDate = masterRow["PReturnDate"] != DBNull.Value ? Convert.ToDateTime(masterRow["PReturnDate"]) : DateTime.MinValue,
                                InvoiceNo = masterRow["InvoiceNo"] != DBNull.Value ? masterRow["InvoiceNo"].ToString() : "",
                                InvoiceDate = masterRow["InvoiceDate"] != DBNull.Value ? Convert.ToDateTime(masterRow["InvoiceDate"]) : DateTime.MinValue,
                                VendorName = masterRow["VendorName"] != DBNull.Value ? masterRow["VendorName"].ToString() : "",
                                Paymode = masterRow["Paymode"] != DBNull.Value ? masterRow["Paymode"].ToString() : "",
                                SubTotal = masterRow["SubTotal"] != DBNull.Value ? Convert.ToDouble(masterRow["SubTotal"]) : 0.0,
                                GrandTotal = masterRow["GrandTotal"] != DBNull.Value ? Convert.ToDouble(masterRow["GrandTotal"]) : 0.0
                            };
                        }

                        // Second table contains detail data (PReturnDetails table)
                        if (ds.Tables.Count > 1 && ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                        {
                            reportData.Details = new List<PurchaseReturnReportDetail>();

                            foreach (DataRow detailRow in ds.Tables[1].Rows)
                            {
                                var detail = new PurchaseReturnReportDetail
                                {
                                    PReturnNo = pReturnNo,
                                    SlNo = detailRow["SlNo"] != DBNull.Value ? Convert.ToInt32(detailRow["SlNo"]) : 0,
                                    ItemName = detailRow["ItemName"] != DBNull.Value ? detailRow["ItemName"].ToString() : "",
                                    Unit = detailRow["Unit"] != DBNull.Value ? detailRow["Unit"].ToString() : "",
                                    Packing = detailRow["Packing"] != DBNull.Value ? detailRow["Packing"].ToString() : "",
                                    Qty = detailRow["Qty"] != DBNull.Value ? Convert.ToDouble(detailRow["Qty"]) : 0.0,
                                    Cost = detailRow["Cost"] != DBNull.Value ? Convert.ToDouble(detailRow["Cost"]) : 0.0,
                                    // Database stores percentage values as basis points (multiplied by 100)
                                    TaxPer = detailRow["TaxPer"] != DBNull.Value ? Convert.ToDouble(detailRow["TaxPer"]) / 100 : 0.0,
                                    TaxAmt = detailRow["TaxAmt"] != DBNull.Value ? Convert.ToDouble(detailRow["TaxAmt"]) : 0.0,
                                    Amount = detailRow["Amount"] != DBNull.Value ? Convert.ToDouble(detailRow["Amount"]) : 0.0,
                                    Reason = detailRow["Reason"] != DBNull.Value ? detailRow["Reason"].ToString() : ""
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
                throw new Exception($"Error retrieving purchase return report details for Return No: {pReturnNo}. {ex.Message}", ex);
            }
            finally
            {
                DataConnection.Close();
            }

            return reportData;
        }

        /// <summary>
        /// Get all purchase return records for a date range (for master grid) using stored procedure
        /// </summary>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <param name="branchId">Branch ID</param>
        /// <returns>List of purchase return records</returns>
        public List<PurchaseReturnReportMaster> GetPurchaseReturnRecords(DateTime fromDate, DateTime toDate, int branchId = 0)
        {
            List<PurchaseReturnReportMaster> returns = new List<PurchaseReturnReportMaster>();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_PurchaseReturn_Master_for_Report, (SqlConnection)DataConnection))
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
                            // Manual mapping to handle case-sensitive column names and DBNull values
                            foreach (DataRow row in dt.Rows)
                            {
                                returns.Add(new PurchaseReturnReportMaster
                                {
                                    PReturnNo = row["PReturnNo"] != DBNull.Value ? Convert.ToInt32(row["PReturnNo"]) : 0,
                                    PReturnDate = row["PReturnDate"] != DBNull.Value ? Convert.ToDateTime(row["PReturnDate"]) : DateTime.MinValue,
                                    InvoiceNo = row["InvoiceNo"] != DBNull.Value ? row["InvoiceNo"].ToString() : "",
                                    InvoiceDate = row["InvoiceDate"] != DBNull.Value ? Convert.ToDateTime(row["InvoiceDate"]) : DateTime.MinValue,
                                    VendorName = row["VendorName"] != DBNull.Value ? row["VendorName"].ToString() : "",
                                    Paymode = row["Paymode"] != DBNull.Value ? row["Paymode"].ToString() : "",
                                    SubTotal = row["SubTotal"] != DBNull.Value ? Convert.ToDouble(row["SubTotal"]) : 0.0,
                                    GrandTotal = row["GrandTotal"] != DBNull.Value ? Convert.ToDouble(row["GrandTotal"]) : 0.0
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving purchase return records. {ex.Message}", ex);
            }
            finally
            {
                DataConnection.Close();
            }

            return returns;
        }
    }
}
