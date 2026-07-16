using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ModelClass;
using ModelClass.Report;

namespace Repository.ReportRepository
{
    public class TradingPLRepository : BaseRepostitory
    {
        /// <summary>
        /// Gets the complete Trading & Profit/Loss Account report
        /// </summary>
        /// <param name="fromDate">Start date of the reporting period</param>
        /// <param name="toDate">End date of the reporting period</param>
        /// <returns>TradingPLReport with all line items and summary</returns>
        public TradingPLReport GetTradingPLReport(DateTime fromDate, DateTime toDate)
        {
            TradingPLReport report = new TradingPLReport();
            report.FromDate = fromDate;
            report.ToDate = toDate;
            int companyId = GetContextValue(SessionContext.CompanyId, DataBase.CompanyId);
            int branchId = GetContextValue(SessionContext.BranchId, DataBase.BranchId);
            int finYearId = GetContextValue(SessionContext.FinYearId, DataBase.FinyearId);

            if (companyId <= 0 || branchId <= 0 || finYearId <= 0)
            {
                throw new InvalidOperationException(
                    $"Trading & P/L cannot be loaded because session values are missing. CompanyId={companyId}, BranchId={branchId}, FinYearId={finYearId}.");
            }

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_TradingPLAccount, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapter.Fill(ds);

                        // Result Set 1: Trading Account line items
                        if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                report.TradingItems.Add(new TradingPLLineItem
                                {
                                    LedgerID = Convert.ToInt32(row["LedgerID"]),
                                    LedgerName = row["LedgerName"].ToString(),
                                    GroupID = Convert.ToInt32(row["GroupID"]),
                                    GroupName = row["GroupName"].ToString(),
                                    Category = row["Category"].ToString(),
                                    NormalBalance = row["NormalBalance"].ToString(),
                                    TotalDebit = row["TotalDebit"] != DBNull.Value ? Convert.ToDecimal(row["TotalDebit"]) : 0,
                                    TotalCredit = row["TotalCredit"] != DBNull.Value ? Convert.ToDecimal(row["TotalCredit"]) : 0,
                                    NetBalance = row["NetBalance"] != DBNull.Value ? Convert.ToDecimal(row["NetBalance"]) : 0
                                });
                            }
                        }

                        // Result Set 2: Profit & Loss Account line items
                        if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                        {
                            foreach (DataRow row in ds.Tables[1].Rows)
                            {
                                report.ProfitLossItems.Add(new TradingPLLineItem
                                {
                                    LedgerID = Convert.ToInt32(row["LedgerID"]),
                                    LedgerName = row["LedgerName"].ToString(),
                                    GroupID = Convert.ToInt32(row["GroupID"]),
                                    GroupName = row["GroupName"].ToString(),
                                    Category = row["Category"].ToString(),
                                    NormalBalance = row["NormalBalance"].ToString(),
                                    TotalDebit = row["TotalDebit"] != DBNull.Value ? Convert.ToDecimal(row["TotalDebit"]) : 0,
                                    TotalCredit = row["TotalCredit"] != DBNull.Value ? Convert.ToDecimal(row["TotalCredit"]) : 0,
                                    NetBalance = row["NetBalance"] != DBNull.Value ? Convert.ToDecimal(row["NetBalance"]) : 0
                                });
                            }
                        }

                        // Result Set 3: Summary totals
                        if (ds.Tables.Count > 2 && ds.Tables[2].Rows.Count > 0)
                        {
                            DataRow summaryRow = ds.Tables[2].Rows[0];
                            report.Summary = new TradingPLSummary
                            {
                                TotalSales = summaryRow["TotalSales"] != DBNull.Value ? Convert.ToDecimal(summaryRow["TotalSales"]) : 0,
                                TotalPurchases = summaryRow["TotalPurchases"] != DBNull.Value ? Convert.ToDecimal(summaryRow["TotalPurchases"]) : 0,
                                TotalDirectExpenses = summaryRow["TotalDirectExpenses"] != DBNull.Value ? Convert.ToDecimal(summaryRow["TotalDirectExpenses"]) : 0,
                                TotalDirectIncomes = summaryRow["TotalDirectIncomes"] != DBNull.Value ? Convert.ToDecimal(summaryRow["TotalDirectIncomes"]) : 0,
                                TotalStockInHand = summaryRow["TotalStockInHand"] != DBNull.Value ? Convert.ToDecimal(summaryRow["TotalStockInHand"]) : 0,
                                OpeningStock = summaryRow["OpeningStock"] != DBNull.Value ? Convert.ToDecimal(summaryRow["OpeningStock"]) : 0,
                                ClosingStock = summaryRow["ClosingStock"] != DBNull.Value ? Convert.ToDecimal(summaryRow["ClosingStock"]) : 0,
                                GrossProfit = summaryRow["GrossProfit"] != DBNull.Value ? Convert.ToDecimal(summaryRow["GrossProfit"]) : 0,
                                TotalIndirectExpenses = summaryRow["TotalIndirectExpenses"] != DBNull.Value ? Convert.ToDecimal(summaryRow["TotalIndirectExpenses"]) : 0,
                                TotalIndirectIncomes = summaryRow["TotalIndirectIncomes"] != DBNull.Value ? Convert.ToDecimal(summaryRow["TotalIndirectIncomes"]) : 0,
                                NetProfit = summaryRow["NetProfit"] != DBNull.Value ? Convert.ToDecimal(summaryRow["NetProfit"]) : 0
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving Trading & P/L Account report. {ex.Message}", ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return report;
        }

        private int GetContextValue(int sessionValue, string legacyValue)
        {
            if (sessionValue > 0)
            {
                return sessionValue;
            }

            int parsedValue;
            return int.TryParse(legacyValue, out parsedValue) ? parsedValue : 0;
        }
    }
}
