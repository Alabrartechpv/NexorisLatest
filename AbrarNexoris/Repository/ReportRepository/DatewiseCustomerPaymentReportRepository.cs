using ModelClass;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Repository.ReportRepository
{
    public class DatewiseCustomerPaymentReportRepository : BaseRepostitory
    {
        public DatewiseCustomerPaymentReportRepository()
        {
        }

        public List<CustomerDDl> GetCustomers()
        {
            try
            {
                Dropdowns dropdowns = new Dropdowns();
                CustomerDDlGrid data = dropdowns.CustomerDDl();
                if (data?.List != null)
                {
                    return data.List.Where(x => x != null && x.LedgerID > 0).OrderBy(x => x.LedgerName).ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DatewiseCustomerPaymentReportRepository.GetCustomers: {ex.Message}");
            }
            return new List<CustomerDDl>();
        }

        public List<DatewiseCustomerPaymentReportRow> GetReport(DatewiseCustomerPaymentReportFilter filter)
        {
            return GetReportData(filter).Receipts;
        }

        public DatewiseCustomerPaymentReportData GetReportData(DatewiseCustomerPaymentReportFilter filter)
        {
            DatewiseCustomerPaymentReportData result = new DatewiseCustomerPaymentReportData();
            DataSet ds = new DataSet("CustomerPaymentReportDataSet");

            try
            {
                if (DataConnection.State == ConnectionState.Closed)
                {
                    DataConnection.Open();
                }

                using (SqlCommand cmd = new SqlCommand("dbo.POS_DatewiseCustomerPaymentReport", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;

                    cmd.Parameters.AddWithValue("@FromDate", filter.FromDate != DateTime.MinValue ? (object)filter.FromDate.Date : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", filter.ToDate != DateTime.MinValue ? (object)filter.ToDate.Date : DBNull.Value);
                    cmd.Parameters.AddWithValue("@PeriodMode", string.IsNullOrEmpty(filter.PeriodMode) ? "ALL" : filter.PeriodMode);
                    cmd.Parameters.AddWithValue("@Month", filter.Month);
                    cmd.Parameters.AddWithValue("@Year", filter.Year);
                    cmd.Parameters.AddWithValue("@CompanyId", filter.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", filter.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", filter.FinYearId);
                    cmd.Parameters.AddWithValue("@CustomerLedgerId", filter.CustomerLedgerId);
                    cmd.Parameters.AddWithValue("@ReceiptType", string.IsNullOrEmpty(filter.ReceiptType) ? "ALL" : filter.ReceiptType);
                    cmd.Parameters.AddWithValue("@PaymentModeFilter", string.IsNullOrEmpty(filter.PaymentModeFilter) ? "ALL" : filter.PaymentModeFilter);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(filter.Status) ? "Active" : filter.Status);
                    cmd.Parameters.AddWithValue("@SearchText", string.IsNullOrEmpty(filter.SearchText) ? (object)DBNull.Value : filter.SearchText);
                    cmd.Parameters.AddWithValue("@ViewMode", string.IsNullOrEmpty(filter.ViewMode) ? "DETAILED" : filter.ViewMode);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(ds);
                    }
                }

                if (ds.Tables.Count > 0)
                {
                    result.Receipts = MapDataTableToPaymentRows(ds.Tables[0]);
                }

                if (ds.Tables.Count > 1)
                {
                    result.Invoices = MapDataTableToInvoiceRows(ds.Tables[1]);
                }

                if (ds.Tables.Count > 2)
                {
                    result.VoucherDetails = MapDataTableToVoucherRows(ds.Tables[2]);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DatewiseCustomerPaymentReportRepository.GetReportData: {ex.Message}");
                throw;
            }

            return result;
        }

        private static List<DatewiseCustomerPaymentReportRow> MapDataTableToPaymentRows(DataTable table)
        {
            List<DatewiseCustomerPaymentReportRow> rows = new List<DatewiseCustomerPaymentReportRow>();
            if (table == null || table.Rows.Count == 0) return rows;

            foreach (DataRow r in table.Rows)
            {
                rows.Add(new DatewiseCustomerPaymentReportRow
                {
                    ReceiptMasterId = ToInt(r, "ReceiptMasterId"),
                    BranchId = ToInt(r, "BranchId"),
                    BranchName = ToString(r, "BranchName"),
                    ReceiptType = ToString(r, "ReceiptType"),
                    VoucherId = ToInt(r, "VoucherId"),
                    VoucherNo = ToString(r, "VoucherNo"),
                    VoucherDate = ToDateTime(r, "VoucherDate"),
                    CustomerLedgerId = ToInt(r, "CustomerLedgerId"),
                    CustomerName = ToString(r, "CustomerName"),
                    BillNo = ToLong(r, "BillNo"),
                    InvoiceNo = ToString(r, "InvoiceNo"),
                    BillDate = ToDateTime(r, "BillDate"),
                    BillAmount = ToDecimal(r, "BillAmount"),
                    TotalBillReceived = ToDecimal(r, "TotalBillReceived"),
                    ReturnedAmount = ToDecimal(r, "ReturnedAmount"),
                    BillBalance = ToDecimal(r, "BillBalance"),
                    ReceiptAmount = ToDecimal(r, "ReceiptAmount"),
                    MasterReceiptAmount = ToDecimal(r, "MasterReceiptAmount"),
                    BalanceAmount = ToDecimal(r, "BalanceAmount"),
                    PaymentMode = ToString(r, "PaymentMode"),
                    Narration = ToString(r, "Narration"),
                    PaymentReference = ToString(r, "PaymentReference"),
                    Status = ToString(r, "Status"),
                    CurrentCustomerOutstanding = ToDecimal(r, "CurrentCustomerOutstanding")
                });
            }

            return rows;
        }

        private static List<DatewiseCustomerInvoiceRow> MapDataTableToInvoiceRows(DataTable table)
        {
            List<DatewiseCustomerInvoiceRow> rows = new List<DatewiseCustomerInvoiceRow>();
            if (table == null || table.Rows.Count == 0) return rows;

            foreach (DataRow r in table.Rows)
            {
                rows.Add(new DatewiseCustomerInvoiceRow
                {
                    CustomerLedgerId = ToInt(r, "CustomerLedgerId"),
                    CustomerName = ToString(r, "CustomerName"),
                    BillKey = ToString(r, "BillKey"),
                    BillNo = ToLong(r, "BillNo"),
                    InvoiceNo = ToString(r, "InvoiceNo"),
                    BillDate = ToDateTime(r, "BillDate"),
                    InvoiceAmount = ToDecimal(r, "InvoiceAmount"),
                    ReceivedAmount = ToDecimal(r, "ReceivedAmount"),
                    ReturnedAmount = ToDecimal(r, "ReturnedAmount"),
                    Balance = ToDecimal(r, "Balance"),
                    ReceivedInPeriod = ToDecimal(r, "ReceivedInPeriod"),
                    Receipts = ToString(r, "Receipts"),
                    PaymentMethod = ToString(r, "PaymentMethod"),
                    Status = ToString(r, "Status"),
                    BranchName = ToString(r, "BranchName")
                });
            }

            return rows;
        }

        private static List<DatewiseCustomerVoucherRow> MapDataTableToVoucherRows(DataTable table)
        {
            List<DatewiseCustomerVoucherRow> rows = new List<DatewiseCustomerVoucherRow>();
            if (table == null || table.Rows.Count == 0) return rows;

            foreach (DataRow r in table.Rows)
            {
                rows.Add(new DatewiseCustomerVoucherRow
                {
                    BillKey = ToString(r, "BillKey"),
                    CustomerLedgerId = ToInt(r, "CustomerLedgerId"),
                    BillNo = ToLong(r, "BillNo"),
                    VoucherDate = ToDateTime(r, "VoucherDate"),
                    VoucherId = ToInt(r, "VoucherId"),
                    VoucherNo = ToString(r, "VoucherNo"),
                    ReceiptAmount = ToDecimal(r, "ReceiptAmount"),
                    BalanceAmount = ToDecimal(r, "BalanceAmount"),
                    PaymentMode = ToString(r, "PaymentMode"),
                    Narration = ToString(r, "Narration"),
                    Status = ToString(r, "Status"),
                    BranchName = ToString(r, "BranchName")
                });
            }

            return rows;
        }

        private static string ToString(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value || row[column] == null)
                return string.Empty;
            return row[column].ToString();
        }

        private static int ToInt(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value || row[column] == null)
                return 0;
            return int.TryParse(row[column].ToString(), out int val) ? val : 0;
        }

        private static long ToLong(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value || row[column] == null)
                return 0;
            return long.TryParse(row[column].ToString(), out long val) ? val : 0;
        }

        private static decimal ToDecimal(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value || row[column] == null)
                return 0m;
            return decimal.TryParse(row[column].ToString(), out decimal val) ? val : 0m;
        }

        private static DateTime ToDateTime(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value || row[column] == null)
                return DateTime.MinValue;
            return DateTime.TryParse(row[column].ToString(), out DateTime val) ? val : DateTime.MinValue;
        }
    }
}
