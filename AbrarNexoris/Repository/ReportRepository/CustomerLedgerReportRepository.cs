using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ModelClass;
using ModelClass.Report;

namespace Repository.ReportRepository
{
    public class CustomerLedgerReportRepository : BaseRepostitory
    {
        public List<CustomerLedgerReportRow> GetReport(CustomerLedgerReportFilter filter, out decimal openingBalance, out decimal totalDebit, out decimal totalCredit, out decimal closingBalance)
        {
            openingBalance = 0;
            totalDebit = 0;
            totalCredit = 0;
            closingBalance = 0;

            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            List<CustomerLedgerReportRow> rows = new List<CustomerLedgerReportRow>();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_CustomerLedger, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@CompanyId", filter.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", filter.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", filter.FinYearId);
                    cmd.Parameters.AddWithValue("@LedgerId", filter.LedgerId);
                    cmd.Parameters.AddWithValue("@FromDate", filter.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", filter.ToDate);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapter.Fill(ds);

                        // Result Set 1: Transactions
                        if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                rows.Add(new CustomerLedgerReportRow
                                {
                                    VoucherID = row["VoucherID"] != DBNull.Value ? Convert.ToInt64(row["VoucherID"]) : 0,
                                    VoucherDate = row["VoucherDate"] != DBNull.Value ? Convert.ToDateTime(row["VoucherDate"]) : DateTime.MinValue,
                                    VoucherNo = row["VoucherNo"]?.ToString() ?? "",
                                    VoucherTypeName = row["VoucherTypeName"]?.ToString() ?? "",
                                    Particulars = row["Particulars"]?.ToString() ?? "",
                                    Narration = row["Narration"]?.ToString() ?? "",
                                    ReceiptAmount = row["ReceiptAmount"] != DBNull.Value ? Convert.ToDecimal(row["ReceiptAmount"]) : 0m,
                                    PaymentAmount = row["PaymentAmount"] != DBNull.Value ? Convert.ToDecimal(row["PaymentAmount"]) : 0m,
                                    RunningBalance = row["RunningBalance"] != DBNull.Value ? Convert.ToDecimal(row["RunningBalance"]) : 0m
                                });
                            }
                        }

                        // Result Set 2: Summary
                        if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                        {
                            DataRow summaryRow = ds.Tables[1].Rows[0];
                            openingBalance = summaryRow["OpeningBalance"] != DBNull.Value ? Convert.ToDecimal(summaryRow["OpeningBalance"]) : 0m;
                            totalDebit = summaryRow["TotalReceipts"] != DBNull.Value ? Convert.ToDecimal(summaryRow["TotalReceipts"]) : 0m;
                            totalCredit = summaryRow["TotalPayments"] != DBNull.Value ? Convert.ToDecimal(summaryRow["TotalPayments"]) : 0m;
                            closingBalance = summaryRow["ClosingBalance"] != DBNull.Value ? Convert.ToDecimal(summaryRow["ClosingBalance"]) : 0m;
                        }
                    }
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return rows;
        }

        public List<CustomerGridList> GetCustomers()
        {
            CustomerRepositoty customerRepository = new CustomerRepositoty();
            CustomerDDLGrids data = customerRepository.GetCustomerDDL();

            if (data == null || data.List == null)
                return new List<CustomerGridList>();

            return data.List
                .Where(x => x != null && x.LedgerID > 0)
                .OrderBy(x => x.LedgerName)
                .ToList();
        }
    }
}
