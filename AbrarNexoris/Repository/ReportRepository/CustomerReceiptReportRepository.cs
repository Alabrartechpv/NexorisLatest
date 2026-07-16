using ModelClass;
using ModelClass.Report;
using Repository.Accounts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Repository.ReportRepository
{
    public class CustomerReceiptReportRepository : BaseRepostitory
    {
        private readonly Dropdowns _dropdowns;
        private readonly CustomerReceiptInfoRepository _receiptRepository;

        public CustomerReceiptReportRepository()
        {
            _dropdowns = new Dropdowns();
            _receiptRepository = new CustomerReceiptInfoRepository();
        }

        public List<CustomerReceiptReportRow> GetReport(CustomerReceiptReportFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            DataTable receiptTable = new DataTable();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CustomerReceiptReport, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BranchId", SqlDbType.Int).Value = filter.BranchId;
                    cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = filter.FromDate.Date;
                    cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = filter.ToDate.Date;
                    cmd.Parameters.Add("@CustomerLedgerId", SqlDbType.Int).Value = filter.CustomerLedgerId;

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(receiptTable);
                    }
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            List<CustomerReceiptReportRow> rows = MapRows(receiptTable)
                .OrderByDescending(x => x.VoucherDate)
                .ThenByDescending(x => x.VoucherId)
                .ToList();

            EnrichRows(rows, filter.BranchId);
            return rows;
        }

        public List<CustomerDDl> GetCustomers()
        {
            CustomerDDlGrid customers = _dropdowns.CustomerDDl();
            if (customers == null || customers.List == null)
                return new List<CustomerDDl>();

            return customers.List
                .Where(x => x != null && x.LedgerID > 0)
                .OrderBy(x => x.LedgerName)
                .ToList();
        }

        private static IEnumerable<CustomerReceiptReportRow> MapRows(DataTable table)
        {
            if (table == null || table.Rows.Count == 0)
                return Enumerable.Empty<CustomerReceiptReportRow>();

            List<CustomerReceiptReportRow> rows = new List<CustomerReceiptReportRow>();
            foreach (DataRow row in table.Rows)
            {
                rows.Add(new CustomerReceiptReportRow
                {
                    VoucherId = ToInt(row, "VoucherID"),
                    VoucherDate = ToDateTime(row, "VoucherDate"),
                    BillNo = ToLong(row, "BillNo"),
                    CustomerLedgerId = ToInt(row, "CustomerLedgerId"),
                    CustomerName = ToString(row, "LedgerName"),
                    TotalAmount = ToDecimal(row, "TotalAmount"),
                    ReceiptAmount = ToDecimal(row, "ReceiptAmount"),
                    Balance = ToDecimal(row, "Balance")
                });
            }

            return rows;
        }

        private void EnrichRows(List<CustomerReceiptReportRow> rows, int branchId)
        {
            if (rows == null || rows.Count == 0)
                return;

            Dictionary<int, string> customerLookup = GetCustomers()
                .Where(x => x != null && x.LedgerID > 0)
                .GroupBy(x => x.LedgerID)
                .ToDictionary(x => x.Key, x => x.First().LedgerName ?? string.Empty);

            foreach (CustomerReceiptReportRow row in rows)
            {
                if (row == null || row.VoucherId <= 0)
                    continue;

                try
                {
                    DataSet receiptData = _receiptRepository.GetReceiptDataByVoucherId(row.VoucherId, branchId);
                    if (receiptData == null || receiptData.Tables.Count == 0 || receiptData.Tables[0].Rows.Count == 0)
                    {
                        ApplyCustomerFallback(row, customerLookup);
                        continue;
                    }

                    if (receiptData.Tables.Count > 2 && receiptData.Tables[2].Rows.Count > 0)
                    {
                        row.CustomerName = Convert.ToString(receiptData.Tables[2].Rows[0]["LedgerName"]);
                    }
                    else
                    {
                        ApplyCustomerFallback(row, customerLookup);
                    }

                    if (receiptData.Tables.Count > 1 && receiptData.Tables[1].Rows.Count > 0)
                    {
                        row.Balance = receiptData.Tables[1].AsEnumerable()
                            .Where(x => x != null && x.Table.Columns.Contains("BalanceAmount") && x["BalanceAmount"] != DBNull.Value)
                            .Sum(x => Convert.ToDecimal(x["BalanceAmount"]));
                    }
                }
                catch
                {
                    ApplyCustomerFallback(row, customerLookup);
                }
            }
        }

        private static void ApplyCustomerFallback(CustomerReceiptReportRow row, Dictionary<int, string> customerLookup)
        {
            if (row == null)
                return;

            if (string.IsNullOrWhiteSpace(row.CustomerName) &&
                customerLookup != null &&
                customerLookup.TryGetValue(row.CustomerLedgerId, out string customerName))
            {
                row.CustomerName = customerName ?? string.Empty;
            }
        }

        private static int ToInt(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToInt32(row[columnName])
                : 0;
        }

        private static long ToLong(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToInt64(row[columnName])
                : 0L;
        }

        private static decimal ToDecimal(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToDecimal(row[columnName])
                : 0m;
        }

        private static DateTime ToDateTime(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToDateTime(row[columnName])
                : DateTime.MinValue;
        }

        private static string ToString(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToString(row[columnName])
                : string.Empty;
        }
    }
}
