using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ModelClass;
using ModelClass.Report;

namespace Repository.ReportRepository
{
    public class BalanceSheetRepository : BaseRepostitory
    {
        /// <summary>
        /// Gets the complete Balance Sheet report
        /// </summary>
        /// <param name="fromDate">Start date of the reporting period</param>
        /// <param name="toDate">End date of the reporting period</param>
        /// <returns>BalanceSheetReport with Liabilities, Assets, and Summary</returns>
        public BalanceSheetReport GetBalanceSheetReport(DateTime fromDate, DateTime toDate)
        {
            BalanceSheetReport report = new BalanceSheetReport();
            report.FromDate = fromDate;
            report.ToDate = toDate;
            int companyId = GetContextValue(SessionContext.CompanyId, DataBase.CompanyId);
            int branchId = GetContextValue(SessionContext.BranchId, DataBase.BranchId);
            int finYearId = GetContextValue(SessionContext.FinYearId, DataBase.FinyearId);

            if (companyId <= 0 || branchId <= 0 || finYearId <= 0)
            {
                throw new InvalidOperationException(
                    $"Balance Sheet cannot be loaded because session values are missing. CompanyId={companyId}, BranchId={branchId}, FinYearId={finYearId}.");
            }

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_BalanceSheet, (SqlConnection)DataConnection))
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

                        // Result Set 1: Liabilities & Equity Line Items
                        if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                report.LiabilitiesItems.Add(new BalanceSheetLineItem
                                {
                                    LedgerID = Convert.ToInt32(row["LedgerID"]),
                                    LedgerName = row["LedgerName"].ToString(),
                                    GroupID = Convert.ToInt32(row["GroupID"]),
                                    GroupName = row["GroupName"].ToString(),
                                    ParentGroupName = row["ParentGroupName"].ToString(),
                                    TotalDebit = row["TotalDebit"] != DBNull.Value ? Convert.ToDecimal(row["TotalDebit"]) : 0,
                                    TotalCredit = row["TotalCredit"] != DBNull.Value ? Convert.ToDecimal(row["TotalCredit"]) : 0,
                                    ClosingBalance = row["ClosingBalance"] != DBNull.Value ? Convert.ToDecimal(row["ClosingBalance"]) : 0
                                });
                            }
                        }

                        // Result Set 2: Assets Line Items
                        if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                        {
                            foreach (DataRow row in ds.Tables[1].Rows)
                            {
                                report.AssetsItems.Add(new BalanceSheetLineItem
                                {
                                    LedgerID = Convert.ToInt32(row["LedgerID"]),
                                    LedgerName = row["LedgerName"].ToString(),
                                    GroupID = Convert.ToInt32(row["GroupID"]),
                                    GroupName = row["GroupName"].ToString(),
                                    ParentGroupName = row["ParentGroupName"].ToString(),
                                    TotalDebit = row["TotalDebit"] != DBNull.Value ? Convert.ToDecimal(row["TotalDebit"]) : 0,
                                    TotalCredit = row["TotalCredit"] != DBNull.Value ? Convert.ToDecimal(row["TotalCredit"]) : 0,
                                    ClosingBalance = row["ClosingBalance"] != DBNull.Value ? Convert.ToDecimal(row["ClosingBalance"]) : 0
                                });
                            }
                        }

                        // Result Set 3: Summary Totals
                        if (ds.Tables.Count > 2 && ds.Tables[2].Rows.Count > 0)
                        {
                            DataRow row = ds.Tables[2].Rows[0];
                            report.Summary.TotalAssets = row["TotalAssets"] != DBNull.Value ? Convert.ToDecimal(row["TotalAssets"]) : 0;
                            report.Summary.TotalLiabilities = row["TotalLiabilities"] != DBNull.Value ? Convert.ToDecimal(row["TotalLiabilities"]) : 0;
                            report.Summary.TotalCapital = row["TotalCapital"] != DBNull.Value ? Convert.ToDecimal(row["TotalCapital"]) : 0;
                            report.Summary.NetProfitLoss = row["NetProfitLoss"] != DBNull.Value ? Convert.ToDecimal(row["NetProfitLoss"]) : 0;
                            report.Summary.Difference = row["Difference"] != DBNull.Value ? Convert.ToDecimal(row["Difference"]) : 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving Balance Sheet report. {ex.Message}", ex);
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
