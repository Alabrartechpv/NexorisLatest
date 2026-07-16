using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ModelClass;
using ModelClass.Report;

namespace Repository.ReportRepository
{
    public class CashBankBookRepository : BaseRepostitory
    {
        public CashBankBookModel GetCashBankBook(int ledgerId, DateTime fromDate, DateTime toDate)
        {
            var report = new CashBankBookModel();

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_CashBankBook, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@LedgerId", ledgerId);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapter.Fill(ds);

                        // Result Set 1: Transactions
                        if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                report.Transactions.Add(new CashBankTransaction
                                {
                                    VoucherID = row["VoucherID"] != DBNull.Value ? Convert.ToInt32(row["VoucherID"]) : 0,
                                    VoucherDate = row["VoucherDate"] != DBNull.Value ? Convert.ToDateTime(row["VoucherDate"]) : DateTime.MinValue,
                                    VoucherNo = row["VoucherNo"]?.ToString() ?? "",
                                    VoucherTypeName = row["VoucherTypeName"]?.ToString() ?? "",
                                    Particulars = row["Particulars"]?.ToString() ?? "",
                                    Narration = row["Narration"]?.ToString() ?? "",
                                    ReceiptAmount = row["ReceiptAmount"] != DBNull.Value ? Convert.ToDecimal(row["ReceiptAmount"]) : 0,
                                    PaymentAmount = row["PaymentAmount"] != DBNull.Value ? Convert.ToDecimal(row["PaymentAmount"]) : 0,
                                    RunningBalance = row["RunningBalance"] != DBNull.Value ? Convert.ToDecimal(row["RunningBalance"]) : 0
                                });
                            }
                        }

                        // Result Set 2: Summary
                        if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                        {
                            DataRow summaryRow = ds.Tables[1].Rows[0];
                            report.Summary = new CashBankSummary
                            {
                                OpeningBalance = summaryRow["OpeningBalance"] != DBNull.Value ? Convert.ToDecimal(summaryRow["OpeningBalance"]) : 0,
                                TotalReceipts = summaryRow["TotalReceipts"] != DBNull.Value ? Convert.ToDecimal(summaryRow["TotalReceipts"]) : 0,
                                TotalPayments = summaryRow["TotalPayments"] != DBNull.Value ? Convert.ToDecimal(summaryRow["TotalPayments"]) : 0,
                                ClosingBalance = summaryRow["ClosingBalance"] != DBNull.Value ? Convert.ToDecimal(summaryRow["ClosingBalance"]) : 0
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving Cash & Bank Book: {ex.Message}", ex);
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
