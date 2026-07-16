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
    public class PurchaseReportRepository : BaseRepostitory
    {
        /// <summary>
        /// Get Purchase Report Details for a specific Purchase Number using the stored procedure
        /// </summary>
        /// <param name="purchaseNo">Purchase Number to get details for</param>
        /// <returns>PurchaseReportData containing master and detail information</returns>
        public PurchaseReportData GetPurchaseReportDetails(int purchaseNo)
        {
            PurchaseReportData reportData = new PurchaseReportData();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_Purchase_Details_for_Report, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PurchaseNo", purchaseNo);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);

                        // First table contains master data (PMaster table)
                        if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            var masterRow = ds.Tables[0].Rows[0];
                            reportData.Master = new PurchaseReportMaster
                            {
                                PurchaseNo = Convert.ToInt32(masterRow["PurchaseNo"]),
                                PurchaseDate = Convert.ToDateTime(masterRow["PurchaseDate"]),
                                InvoiceNo = masterRow["InvoiceNo"]?.ToString(),
                                InvoiceDate = Convert.ToDateTime(masterRow["InvoiceDate"]),
                                VendorName = masterRow["VendorName"]?.ToString(),
                                Paymode = masterRow["Paymode"]?.ToString(),
                                SubTotal = Convert.ToDouble(masterRow["SubTotal"]),
                                GrandTotal = Convert.ToDouble(masterRow["GrandTotal"]),
                                PayedAmount = Convert.ToDouble(masterRow["PayedAmount"]),
                                BilledBy = masterRow["BilledBy"]?.ToString()
                            };
                        }

                        // Second table contains detail data (PDetails table)
                        if (ds.Tables.Count > 1 && ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                        {
                            reportData.Details = new List<PurchaseReportDetail>();

                            foreach (DataRow detailRow in ds.Tables[1].Rows)
                            {
                                var detail = new PurchaseReportDetail
                                {
                                    PurchaseNo = purchaseNo,
                                    SlNo = Convert.ToInt32(detailRow["SlNo"]),
                                    ItemName = detailRow["ItemName"]?.ToString(),
                                    BarCode = detailRow["BarCode"]?.ToString(),
                                    Unit = detailRow["Unit"]?.ToString(),
                                    Packing = detailRow["Packing"]?.ToString(),
                                    Qty = Convert.ToDouble(detailRow["qty"]),
                                    Cost = Convert.ToDouble(detailRow["Cost"]),
                                    Amount = Convert.ToDouble(detailRow["Amount"]),
                                    Free = Convert.ToDouble(detailRow["Free"])
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
                throw new Exception($"Error retrieving purchase report details for Purchase No: {purchaseNo}. {ex.Message}", ex);
            }
            finally
            {
                DataConnection.Close();
            }

            return reportData;
        }

        /// <summary>
        /// Get all purchase bills for a date range (for master grid) using stored procedure
        /// </summary>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <param name="branchId">Branch ID</param>
        /// <returns>List of purchase bills</returns>
        public List<PurchaseReportMaster> GetPurchaseBills(DateTime fromDate, DateTime toDate, int branchId = 0)
        {
            List<PurchaseReportMaster> bills = new List<PurchaseReportMaster>();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_Purchase_Master_for_Report, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    cmd.Parameters.AddWithValue("@BranchId", branchId); // Always pass BranchId (even if 0)

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            // Manual mapping to handle case-sensitive column names
                            foreach (DataRow row in dt.Rows)
                            {
                                bills.Add(new PurchaseReportMaster
                                {
                                    PurchaseNo = Convert.ToInt32(row["PurchaseNo"]),
                                    PurchaseDate = Convert.ToDateTime(row["PurchaseDate"]),
                                    InvoiceNo = row["InvoiceNo"]?.ToString() ?? "",
                                    InvoiceDate = Convert.ToDateTime(row["InvoiceDate"]),
                                    VendorName = row["VendorName"]?.ToString() ?? "",
                                    Paymode = row["Paymode"]?.ToString() ?? "",
                                    SubTotal = Convert.ToDouble(row["SubTotal"]),
                                    GrandTotal = Convert.ToDouble(row["GrandTotal"]),
                                    PayedAmount = Convert.ToDouble(row["PayedAmount"]),
                                    BilledBy = row["BilledBy"]?.ToString() ?? ""
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving purchase bills. {ex.Message}", ex);
            }
            finally
            {
                DataConnection.Close();
            }

            return bills;
        }
    }
}
