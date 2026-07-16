using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ModelClass;
using ModelClass.Report;

namespace Repository.ReportRepository
{
    public class BankStatementReportRepository : BaseRepostitory
    {
        /// <summary>
        /// Retrieves all bank-related transactions (Sales, Purchase, Vendor Payment, Customer Receipt)
        /// for the specified date range.
        /// </summary>
        public BankStatementReportModel GetBankStatementReport(DateTime fromDate, DateTime toDate)
        {
            var report = new BankStatementReportModel();

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_BankStatementReport, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        decimal totalIn = 0, totalOut = 0;

                        foreach (DataRow row in dt.Rows)
                        {
                            decimal moneyIn = row["MoneyIn"] != DBNull.Value ? Convert.ToDecimal(row["MoneyIn"]) : 0;
                            decimal moneyOut = row["MoneyOut"] != DBNull.Value ? Convert.ToDecimal(row["MoneyOut"]) : 0;

                            report.Transactions.Add(new BankStatementTransaction
                            {
                                TransactionDate = row["TransactionDate"] != DBNull.Value ? Convert.ToDateTime(row["TransactionDate"]) : DateTime.MinValue,
                                TransactionType = row["TransactionType"]?.ToString() ?? "",
                                PartyName = row["PartyName"]?.ToString() ?? "",
                                BillVoucherNo = row["BillVoucherNo"]?.ToString() ?? "",
                                MoneyIn = moneyIn,
                                MoneyOut = moneyOut,
                                PaymentMethod = row["PaymentMethod"]?.ToString() ?? "",
                                Reference = row["Reference"]?.ToString() ?? ""
                            });

                            totalIn += moneyIn;
                            totalOut += moneyOut;
                        }

                        report.Summary = new BankStatementSummary
                        {
                            TotalMoneyIn = totalIn,
                            TotalMoneyOut = totalOut,
                            NetAmount = totalIn - totalOut
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving Bank Statement Report: {ex.Message}", ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return report;
        }
    }
}
