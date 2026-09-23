using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Repository.ReportRepository
{
    public class SalesProfitReportRepository : BaseRepostitory
    {
        private class TaxInfo
        {
            public double SubTotal { get; set; }
            public double TaxAmt { get; set; }
        }

        /// <summary>
        /// Get Sales Profit Report using the SalesProfitReport stored procedure and SMaster data
        /// </summary>
        /// <param name="billNo">Bill Number (0 for all bills)</param>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <returns>List of SalesProfitReport</returns>
        public List<SalesProfitReport> GetSalesProfitReport(int billNo, DateTime fromDate, DateTime toDate)
        {
            List<SalesProfitReport> list = new List<SalesProfitReport>();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._SalesProfitReport, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Billno", billNo);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            bool hasSubTotal = dt.Columns.Contains("SubTotal");
                            bool hasTaxAmt = dt.Columns.Contains("TaxAmt") || dt.Columns.Contains("TaxAmount");
                            string taxCol = dt.Columns.Contains("TaxAmt") ? "TaxAmt" : (dt.Columns.Contains("TaxAmount") ? "TaxAmount" : null);
                            bool hasBillAmount = dt.Columns.Contains("BillAmount");
                            bool hasNetAmount = dt.Columns.Contains("NetAmount");
                            bool hasProfit = dt.Columns.Contains("Profit");

                            // Check if we need to enrich SubTotal and TaxAmt from SMaster
                            Dictionary<int, TaxInfo> taxLookup = null;
                            if (!hasSubTotal || !hasTaxAmt)
                            {
                                taxLookup = LoadTaxLookup(fromDate, toDate, billNo);
                            }

                            foreach (DataRow row in dt.Rows)
                            {
                                int bNo = Convert.ToInt32(row["BillNo"]);
                                double billAmount = hasBillAmount && row["BillAmount"] != DBNull.Value
                                    ? Convert.ToDouble(row["BillAmount"])
                                    : (hasNetAmount && row["NetAmount"] != DBNull.Value ? Convert.ToDouble(row["NetAmount"]) : 0);

                                double profit = hasProfit && row["Profit"] != DBNull.Value
                                    ? Convert.ToDouble(row["Profit"])
                                    : 0;

                                double subTotal = 0;
                                double taxAmt = 0;

                                if (hasSubTotal && row["SubTotal"] != DBNull.Value)
                                {
                                    subTotal = Convert.ToDouble(row["SubTotal"]);
                                }
                                if (hasTaxAmt && row[taxCol] != DBNull.Value)
                                {
                                    taxAmt = Convert.ToDouble(row[taxCol]);
                                }

                                if ((subTotal == 0 && taxAmt == 0) && taxLookup != null && taxLookup.TryGetValue(bNo, out var taxInfo))
                                {
                                    subTotal = taxInfo.SubTotal;
                                    taxAmt = taxInfo.TaxAmt;
                                }

                                if (subTotal == 0 && billAmount > 0)
                                {
                                    subTotal = billAmount - taxAmt;
                                }

                                double profitExclGst = profit - taxAmt;

                                list.Add(new SalesProfitReport
                                {
                                    BillNo = bNo,
                                    BillDate = Convert.ToDateTime(row["BillDate"]),
                                    SubTotal = subTotal,
                                    TaxAmt = taxAmt,
                                    BillAmount = billAmount,
                                    Profit = profit,
                                    ProfitExclGst = profitExclGst,
                                    PayMode = row.Table.Columns.Contains("PayMode") ? (row["PayMode"]?.ToString() ?? "") : (row.Table.Columns.Contains("paymodename") ? (row["paymodename"]?.ToString() ?? "") : ""),
                                    CashMode = row.Table.Columns.Contains("CashMode") ? (row["CashMode"]?.ToString() ?? "") : ""
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving sales profit report. {ex.Message}", ex);
            }
            finally
            {
                DataConnection.Close();
            }

            return list;
        }

        private Dictionary<int, TaxInfo> LoadTaxLookup(DateTime fromDate, DateTime toDate, int billNo)
        {
            var lookup = new Dictionary<int, TaxInfo>();
            try
            {
                string sql = @"
IF OBJECT_ID('SMaster', 'U') IS NOT NULL
BEGIN
    SELECT BillNo, ISNULL(SubTotal, 0) AS SubTotal, ISNULL(TaxAmt, 0) AS TaxAmt
    FROM SMaster
    WHERE BillDate >= @FromDate AND BillDate <= @ToDate
      AND (@BillNo = 0 OR BillNo = @BillNo);
END";
                using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    cmd.Parameters.AddWithValue("@BillNo", billNo);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int bNo = Convert.ToInt32(reader["BillNo"]);
                            double subTotal = Convert.ToDouble(reader["SubTotal"]);
                            double taxAmt = Convert.ToDouble(reader["TaxAmt"]);
                            lookup[bNo] = new TaxInfo { SubTotal = subTotal, TaxAmt = taxAmt };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadTaxLookup error: " + ex.Message);
            }
            return lookup;
        }
    }
}
