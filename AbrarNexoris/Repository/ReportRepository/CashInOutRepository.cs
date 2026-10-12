using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ModelClass;
using ModelClass.Report;

namespace Repository.ReportRepository
{
    public class CashInOutRepository : BaseRepostitory
    {
        public CashInOutReportResponse GetCashInOutReport(DateTime fromDate, DateTime toDate, int branchId = 0, int companyId = 0)
        {
            var response = new CashInOutReportResponse
            {
                FromDate = fromDate,
                ToDate = toDate,
                BranchId = branchId > 0 ? branchId : GetContextValue(SessionContext.BranchId, DataBase.BranchId),
                BranchName = !string.IsNullOrWhiteSpace(SessionContext.BranchName) ? SessionContext.BranchName : DataBase.Branch
            };

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_CashInOutReport, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;

                    int effectiveCompany = companyId > 0 ? companyId : GetContextValue(SessionContext.CompanyId, DataBase.CompanyId);
                    int effectiveBranch = response.BranchId;

                    cmd.Parameters.AddWithValue("@CompanyId", effectiveCompany);
                    cmd.Parameters.AddWithValue("@BranchId", effectiveBranch);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapter.Fill(ds);

                        // Result Set 1: Summary
                        if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            DataRow r = ds.Tables[0].Rows[0];
                            response.Summary = new CashInOutSummary
                            {
                                OpeningBalance = GetDecimal(r, "OpeningBalance"),
                                TotalCashIn = GetDecimal(r, "TotalCashIn"),
                                TotalCashOut = GetDecimal(r, "TotalCashOut"),
                                NetCashFlow = GetDecimal(r, "NetCashFlow"),
                                ClosingBalance = GetDecimal(r, "ClosingBalance"),
                                CounterCashSales = GetDecimal(r, "CounterCashSales"),
                                DigitalSales = GetDecimal(r, "DigitalSales"),
                                CustomerReceipts = GetDecimal(r, "CustomerReceipts"),
                                GeneralReceipts = GetDecimal(r, "GeneralReceipts"),
                                VendorPayments = GetDecimal(r, "VendorPayments"),
                                CashPurchases = GetDecimal(r, "CashPurchases"),
                                OperatingExpenses = GetDecimal(r, "OperatingExpenses"),
                                SalesReturnsRefund = GetDecimal(r, "SalesReturnsRefund"),
                                OwnerDrawings = GetDecimal(r, "OwnerDrawings"),
                                InflowCount = GetInt(r, "InflowCount"),
                                OutflowCount = GetInt(r, "OutflowCount")
                            };
                        }

                        // Result Set 2: Transactions
                        if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                        {
                            decimal currentRunning = response.Summary.OpeningBalance;

                            // Transactions in table are ordered DESC, but to compute running balance correctly,
                            // we can track or map from table
                            foreach (DataRow row in ds.Tables[1].Rows)
                            {
                                decimal inflow = GetDecimal(row, "InflowAmount");
                                decimal outflow = GetDecimal(row, "OutflowAmount");

                                response.Transactions.Add(new CashInOutTransaction
                                {
                                    SlNo = GetInt(row, "SlNo"),
                                    VoucherDate = row["VoucherDate"] != DBNull.Value ? Convert.ToDateTime(row["VoucherDate"]) : DateTime.MinValue,
                                    VoucherNo = row["VoucherNo"]?.ToString() ?? "",
                                    VoucherType = row["VoucherType"]?.ToString() ?? "",
                                    Category = row["Category"]?.ToString() ?? "",
                                    Direction = row["Direction"]?.ToString() ?? "",
                                    Particulars = row["Particulars"]?.ToString() ?? "",
                                    PaymentMode = row["PaymentMode"]?.ToString() ?? "",
                                    InflowAmount = inflow,
                                    OutflowAmount = outflow,
                                    Narration = row["Narration"]?.ToString() ?? ""
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving Cash In / Cash Out report: {ex.Message}", ex);
            }
            finally
            {
                DataConnection.Close();
            }

            return response;
        }

        private static int GetContextValue(int sessionVal, string dbVal)
        {
            if (sessionVal > 0) return sessionVal;
            if (int.TryParse(dbVal, out int val) && val > 0) return val;
            return 0;
        }

        private static decimal GetDecimal(DataRow row, string col)
        {
            if (row.Table.Columns.Contains(col) && row[col] != DBNull.Value)
            {
                decimal.TryParse(row[col].ToString(), out decimal val);
                return val;
            }
            return 0m;
        }

        private static int GetInt(DataRow row, string col)
        {
            if (row.Table.Columns.Contains(col) && row[col] != DBNull.Value)
            {
                int.TryParse(row[col].ToString(), out int val);
                return val;
            }
            return 0;
        }
    }
}
