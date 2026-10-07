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
        public TradingPLReport GetTradingPLReport(DateTime fromDate, DateTime toDate, int branchId = 0, int companyId = 0, int finYearId = 0)
        {
            TradingPLReport report = new TradingPLReport();
            report.FromDate = fromDate;
            report.ToDate = toDate;
            int effCompanyId = companyId > 0 ? companyId : GetContextValue(SessionContext.CompanyId, DataBase.CompanyId);
            int effBranchId = branchId > 0 ? branchId : GetContextValue(SessionContext.BranchId, DataBase.BranchId);
            int effFinYearId = finYearId > 0 ? finYearId : GetContextValue(SessionContext.FinYearId, DataBase.FinyearId);

            if (effCompanyId <= 0) int.TryParse(DataBase.CompanyId, out effCompanyId);
            if (effBranchId <= 0) int.TryParse(DataBase.BranchId, out effBranchId);
            if (effFinYearId <= 0) int.TryParse(DataBase.FinyearId, out effFinYearId);

            if (effCompanyId <= 0) effCompanyId = 1;
            if (effBranchId <= 0) effBranchId = 1;
            if (effFinYearId <= 0) effFinYearId = 1;

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
                    cmd.Parameters.AddWithValue("@CompanyId", effCompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", effBranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", effFinYearId);
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
