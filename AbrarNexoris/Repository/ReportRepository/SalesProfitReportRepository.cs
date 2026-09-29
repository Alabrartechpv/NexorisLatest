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

                                double profitExclGst = row.Table.Columns.Contains("ProfitExclGst") && row["ProfitExclGst"] != DBNull.Value
                                    ? Convert.ToDouble(row["ProfitExclGst"])
                                    : (profit > taxAmt && billAmount > subTotal ? profit - taxAmt : profit);

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

        public class ExpenseLedgerItem
        {
            public string LedgerName { get; set; }
            public string GroupName { get; set; }
            public double Amount { get; set; }
        }

        public List<ExpenseLedgerItem> GetExpenseBreakdown(DateTime fromDate, DateTime toDate, int branchId)
        {
            var list = new List<ExpenseLedgerItem>();
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                string sql = @"
SELECT 
    lm.LedgerName,
    ag.GroupName,
    ISNULL(SUM(v.Debit), 0) AS Amount
FROM Vouchers v
INNER JOIN LedgerMaster lm ON lm.LedgerID = v.LedgerID
INNER JOIN AccountGroupMaster ag ON ag.GroupID = lm.GroupID
WHERE (ag.GroupID IN (10, 12) OR ag.GroupName LIKE '%Expense%')
  AND v.VoucherDate >= @FromDate AND v.VoucherDate <= @ToDate
  AND (@BranchId = 0 OR v.BranchID = @BranchId)
  AND ISNULL(v.CancelFlag, 0) = 0
GROUP BY lm.LedgerName, ag.GroupName
HAVING ISNULL(SUM(v.Debit), 0) <> 0
ORDER BY ag.GroupName, lm.LedgerName;";

                using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date.AddDays(1).AddSeconds(-1));
                    cmd.Parameters.AddWithValue("@BranchId", branchId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new ExpenseLedgerItem
                            {
                                LedgerName = reader["LedgerName"]?.ToString() ?? "",
                                GroupName = reader["GroupName"]?.ToString() ?? "",
                                Amount = reader["Amount"] != DBNull.Value ? Convert.ToDouble(reader["Amount"]) : 0
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetExpenseBreakdown error: " + ex.Message);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return list;
        }

        public double GetTotalExpenses(DateTime fromDate, DateTime toDate, int branchId)
        {
            var items = GetExpenseBreakdown(fromDate, toDate, branchId);
            return items.Sum(x => x.Amount);
        }
    }
}
