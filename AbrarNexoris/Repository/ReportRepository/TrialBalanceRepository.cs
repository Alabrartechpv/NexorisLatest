using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ModelClass;
using ModelClass.Report;

namespace Repository.ReportRepository
{
    public class TrialBalanceRepository : BaseRepostitory
    {
        /// <summary>
        /// Gets the complete Trial Balance report
        /// </summary>
        /// <param name="fromDate">Start date of the reporting period</param>
        /// <param name="toDate">End date of the reporting period</param>
        /// <returns>TrialBalanceReport with LineItems and Summary</returns>
        public TrialBalanceReport GetTrialBalanceReport(DateTime fromDate, DateTime toDate)
        {
            TrialBalanceReport report = new TrialBalanceReport();
            report.FromDate = fromDate;
            report.ToDate = toDate;
            int companyId = GetContextValue(SessionContext.CompanyId, DataBase.CompanyId);
            int branchId = GetContextValue(SessionContext.BranchId, DataBase.BranchId);
            int finYearId = GetContextValue(SessionContext.FinYearId, DataBase.FinyearId);

            if (companyId <= 0 || branchId <= 0 || finYearId <= 0)
            {
                throw new InvalidOperationException(
                    $"Trial Balance cannot be loaded because session values are missing. CompanyId={companyId}, BranchId={branchId}, FinYearId={finYearId}.");
            }

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_TrialBalance, (SqlConnection)DataConnection))
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

                        // Result Set 1: Individual Ledger Line Items
                        if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                report.LineItems.Add(new TrialBalanceLineItem
                                {
                                    LedgerID = Convert.ToInt32(row["LedgerID"]),
                                    LedgerName = row["LedgerName"].ToString(),
                                    GroupID = Convert.ToInt32(row["GroupID"]),
                                    GroupName = row["GroupName"].ToString(),
                                    GroupType = row["GroupType"].ToString(),
                                    OpeningDebit = row["OpeningDebit"] != DBNull.Value ? Convert.ToDecimal(row["OpeningDebit"]) : 0,
                                    OpeningCredit = row["OpeningCredit"] != DBNull.Value ? Convert.ToDecimal(row["OpeningCredit"]) : 0,
                                    TransactionDebit = row["TransactionDebit"] != DBNull.Value ? Convert.ToDecimal(row["TransactionDebit"]) : 0,
                                    TransactionCredit = row["TransactionCredit"] != DBNull.Value ? Convert.ToDecimal(row["TransactionCredit"]) : 0,
                                    ClosingDebit = row["ClosingDebit"] != DBNull.Value ? Convert.ToDecimal(row["ClosingDebit"]) : 0,
                                    ClosingCredit = row["ClosingCredit"] != DBNull.Value ? Convert.ToDecimal(row["ClosingCredit"]) : 0
                                });
                            }
                        }

                        // Result Set 2: Summary Totals
                        if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                        {
                            DataRow row = ds.Tables[1].Rows[0];
                            report.Summary.TotalOpeningDebit = row["TotalOpeningDebit"] != DBNull.Value ? Convert.ToDecimal(row["TotalOpeningDebit"]) : 0;
                            report.Summary.TotalOpeningCredit = row["TotalOpeningCredit"] != DBNull.Value ? Convert.ToDecimal(row["TotalOpeningCredit"]) : 0;
                            report.Summary.TotalTransactionDebit = row["TotalTransactionDebit"] != DBNull.Value ? Convert.ToDecimal(row["TotalTransactionDebit"]) : 0;
                            report.Summary.TotalTransactionCredit = row["TotalTransactionCredit"] != DBNull.Value ? Convert.ToDecimal(row["TotalTransactionCredit"]) : 0;
                            report.Summary.TotalClosingDebit = row["TotalClosingDebit"] != DBNull.Value ? Convert.ToDecimal(row["TotalClosingDebit"]) : 0;
                            report.Summary.TotalClosingCredit = row["TotalClosingCredit"] != DBNull.Value ? Convert.ToDecimal(row["TotalClosingCredit"]) : 0;
                            report.Summary.Difference = row["Difference"] != DBNull.Value ? Convert.ToDecimal(row["Difference"]) : 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving Trial Balance report. {ex.Message}", ex);
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
