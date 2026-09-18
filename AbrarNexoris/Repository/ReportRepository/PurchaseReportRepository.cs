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
        private static int SafeInt(object value)
        {
            if (value == null || value == DBNull.Value) return 0;
            return int.TryParse(value.ToString(), out int result) ? result : 0;
        }

        private static double SafeDouble(object value)
        {
            if (value == null || value == DBNull.Value) return 0.0;
            return double.TryParse(value.ToString(), out double result) ? result : 0.0;
        }

        private static DateTime SafeDateTime(object value)
        {
            if (value == null || value == DBNull.Value) return DateTime.Today;
            return DateTime.TryParse(value.ToString(), out DateTime result) ? result : DateTime.Today;
        }

        private static string SafeString(object value)
        {
            if (value == null || value == DBNull.Value) return string.Empty;
            return value.ToString();
        }

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
                                PurchaseNo = SafeInt(masterRow["PurchaseNo"]),
                                PurchaseDate = SafeDateTime(masterRow["PurchaseDate"]),
                                InvoiceNo = SafeString(masterRow["InvoiceNo"]),
                                InvoiceDate = SafeDateTime(masterRow["InvoiceDate"]),
                                VendorName = SafeString(masterRow["VendorName"]),
                                Paymode = SafeString(masterRow["Paymode"]),
                                SubTotal = SafeDouble(masterRow["SubTotal"]),
                                GrandTotal = SafeDouble(masterRow["GrandTotal"]),
                                PayedAmount = SafeDouble(masterRow["PayedAmount"]),
                                BilledBy = SafeString(masterRow["BilledBy"])
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
                                    SlNo = SafeInt(detailRow["SlNo"]),
                                    ItemName = SafeString(detailRow["ItemName"]),
                                    BarCode = SafeString(detailRow["BarCode"]),
                                    Unit = SafeString(detailRow["Unit"]),
                                    Packing = SafeString(detailRow["Packing"]),
                                    Qty = SafeDouble(detailRow["qty"]),
                                    Cost = SafeDouble(detailRow["Cost"]),
                                    Amount = SafeDouble(detailRow["Amount"]),
                                    Free = SafeDouble(detailRow["Free"])
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
                                    PurchaseNo = SafeInt(row["PurchaseNo"]),
                                    PurchaseDate = SafeDateTime(row["PurchaseDate"]),
                                    InvoiceNo = SafeString(row["InvoiceNo"]),
                                    InvoiceDate = SafeDateTime(row["InvoiceDate"]),
                                    VendorName = SafeString(row["VendorName"]),
                                    Paymode = SafeString(row["Paymode"]),
                                    SubTotal = SafeDouble(row["SubTotal"]),
                                    GrandTotal = SafeDouble(row["GrandTotal"]),
                                    PayedAmount = SafeDouble(row["PayedAmount"]),
                                    BilledBy = SafeString(row["BilledBy"])
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
