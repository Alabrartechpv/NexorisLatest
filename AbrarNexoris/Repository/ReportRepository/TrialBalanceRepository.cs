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

                        // Result Set 1: Individual Ledger Line Items (deduplicated by LedgerID)
                        var seenLedgerIds = new HashSet<int>();
                        if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                int ledgerId = row["LedgerID"] != DBNull.Value ? Convert.ToInt32(row["LedgerID"]) : 0;
                                if (ledgerId > 0 && seenLedgerIds.Contains(ledgerId))
                                {
                                    continue; // Skip duplicate records returned by multi-branch SP join
                                }
                                if (ledgerId > 0)
                                {
                                    seenLedgerIds.Add(ledgerId);
                                }

                                report.LineItems.Add(new TrialBalanceLineItem
                                {
                                    LedgerID = ledgerId,
                                    LedgerName = row["LedgerName"] != DBNull.Value ? row["LedgerName"].ToString() : string.Empty,
                                    GroupID = row["GroupID"] != DBNull.Value ? Convert.ToInt32(row["GroupID"]) : 0,
                                    GroupName = row["GroupName"] != DBNull.Value ? row["GroupName"].ToString() : string.Empty,
                                    GroupType = row["GroupType"] != DBNull.Value ? row["GroupType"].ToString() : string.Empty,
                                    OpeningDebit = row["OpeningDebit"] != DBNull.Value ? Convert.ToDecimal(row["OpeningDebit"]) : 0,
                                    OpeningCredit = row["OpeningCredit"] != DBNull.Value ? Convert.ToDecimal(row["OpeningCredit"]) : 0,
                                    TransactionDebit = row["TransactionDebit"] != DBNull.Value ? Convert.ToDecimal(row["TransactionDebit"]) : 0,
                                    TransactionCredit = row["TransactionCredit"] != DBNull.Value ? Convert.ToDecimal(row["TransactionCredit"]) : 0,
                                    ClosingDebit = row["ClosingDebit"] != DBNull.Value ? Convert.ToDecimal(row["ClosingDebit"]) : 0,
                                    ClosingCredit = row["ClosingCredit"] != DBNull.Value ? Convert.ToDecimal(row["ClosingCredit"]) : 0
                                });
                            }
                        }

                        // Summary Totals: Calculate from clean, deduplicated line items
                        decimal totalOpDr = 0, totalOpCr = 0, totalTxnDr = 0, totalTxnCr = 0, totalClDr = 0, totalClCr = 0;
                        foreach (var item in report.LineItems)
                        {
                            totalOpDr += item.OpeningDebit;
                            totalOpCr += item.OpeningCredit;
                            totalTxnDr += item.TransactionDebit;
                            totalTxnCr += item.TransactionCredit;
                            totalClDr += item.ClosingDebit;
                            totalClCr += item.ClosingCredit;
                        }

                        report.Summary.TotalOpeningDebit = totalOpDr;
                        report.Summary.TotalOpeningCredit = totalOpCr;
                        report.Summary.TotalTransactionDebit = totalTxnDr;
                        report.Summary.TotalTransactionCredit = totalTxnCr;
                        report.Summary.TotalClosingDebit = totalClDr;
                        report.Summary.TotalClosingCredit = totalClCr;
                        report.Summary.Difference = totalClDr - totalClCr;
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
