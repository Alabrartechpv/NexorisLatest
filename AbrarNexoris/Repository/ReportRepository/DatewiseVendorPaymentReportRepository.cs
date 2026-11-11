using ModelClass;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Repository.ReportRepository
{
    public class DatewiseVendorPaymentReportRepository : BaseRepostitory
    {
        public DatewiseVendorPaymentReportRepository()
        {
        }

        public List<VendorGridList> GetVendors()
        {
            try
            {
                VendorRepository vendorRepository = new VendorRepository();
                VendorDDLGrid data = vendorRepository.GetVendorDDL();
                if (data?.List != null)
                {
                    return data.List.Where(x => x != null && x.LedgerID > 0).ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DatewiseVendorPaymentReportRepository.GetVendors: {ex.Message}");
            }
            return new List<VendorGridList>();
        }

        public List<DatewiseVendorPaymentReportRow> GetReport(DatewiseVendorPaymentReportFilter filter)
        {
            return GetReportData(filter).Payments;
        }

        public DatewiseVendorPaymentReportData GetReportData(DatewiseVendorPaymentReportFilter filter)
        {
            DatewiseVendorPaymentReportData result = new DatewiseVendorPaymentReportData();
            DataSet ds = new DataSet("VendorPaymentReportDataSet");

            try
            {
                if (DataConnection.State == ConnectionState.Closed)
                {
                    DataConnection.Open();
                }

                using (SqlCommand cmd = new SqlCommand("dbo.POS_DatewiseVendorPaymentReport", (SqlConnection)DataConnection))
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
                    cmd.Parameters.AddWithValue("@VendorLedgerId", filter.VendorLedgerId);
                    cmd.Parameters.AddWithValue("@PaymentType", string.IsNullOrEmpty(filter.PaymentType) ? "ALL" : filter.PaymentType);
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
                    result.Payments = MapDataTableToPaymentRows(ds.Tables[0]);
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
                System.Diagnostics.Debug.WriteLine($"Error in DatewiseVendorPaymentReportRepository.GetReportData: {ex.Message}");
                throw;
            }

            return result;
        }

        private static List<DatewiseVendorPaymentReportRow> MapDataTableToPaymentRows(DataTable table)
        {
            List<DatewiseVendorPaymentReportRow> rows = new List<DatewiseVendorPaymentReportRow>();
            if (table == null || table.Rows.Count == 0) return rows;

            foreach (DataRow r in table.Rows)
            {
                rows.Add(new DatewiseVendorPaymentReportRow
                {
                    PaymentMasterId = ToInt(r, "PaymentMasterId"),
                    BranchId = ToInt(r, "BranchId"),
                    BranchName = ToString(r, "BranchName"),
                    VoucherType = ToString(r, "VoucherType"),
                    VoucherId = ToInt(r, "VoucherId"),
                    VoucherNo = ToString(r, "VoucherNo"),
                    VoucherDate = ToDateTime(r, "VoucherDate"),
                    VendorLedgerId = ToInt(r, "VendorLedgerId"),
                    VendorName = ToString(r, "VendorName"),
                    PurchaseNo = ToLong(r, "PurchaseNo"),
                    InvoiceNo = ToString(r, "InvoiceNo"),
                    BillDate = ToDateTime(r, "BillDate"),
                    BillAmount = ToDecimal(r, "BillAmount"),
                    TotalBillPaid = ToDecimal(r, "TotalBillPaid"),
                    ReturnedAmount = ToDecimal(r, "ReturnedAmount"),
                    BillBalance = ToDecimal(r, "BillBalance"),
                    PaymentAmount = ToDecimal(r, "PaymentAmount"),
                    MasterPaymentAmount = ToDecimal(r, "MasterPaymentAmount"),
                    BalanceAmount = ToDecimal(r, "BalanceAmount"),
                    PaymentMode = ToString(r, "PaymentMode"),
                    Narration = ToString(r, "Narration"),
                    PaymentReference = ToString(r, "PaymentReference"),
                    Status = ToString(r, "Status"),
                    CurrentVendorOutstanding = ToDecimal(r, "CurrentVendorOutstanding"),
                    PaidInPeriod = ToDecimal(r, "PaidInPeriod"),
                    Vouchers = ToString(r, "Vouchers")
                });
            }

            return rows;
        }

        private static List<DatewiseVendorInvoiceRow> MapDataTableToInvoiceRows(DataTable table)
        {
            List<DatewiseVendorInvoiceRow> rows = new List<DatewiseVendorInvoiceRow>();
            if (table == null || table.Rows.Count == 0) return rows;

            foreach (DataRow r in table.Rows)
            {
                rows.Add(new DatewiseVendorInvoiceRow
                {
                    VendorLedgerId = ToInt(r, "VendorLedgerId"),
                    VendorName = ToString(r, "VendorName"),
                    BillKey = ToString(r, "BillKey"),
                    PurchaseNo = ToLong(r, "PurchaseNo"),
                    InvoiceNo = ToString(r, "InvoiceNo"),
                    BillDate = ToDateTime(r, "BillDate"),
                    InvoiceAmount = ToDecimal(r, "InvoiceAmount"),
                    PaidAmount = ToDecimal(r, "PaidAmount"),
                    ReturnedAmount = ToDecimal(r, "ReturnedAmount"),
                    Balance = ToDecimal(r, "Balance"),
                    PaidInPeriod = ToDecimal(r, "PaidInPeriod"),
                    Vouchers = ToString(r, "Vouchers"),
                    PaymentMethod = ToString(r, "PaymentMethod"),
                    Status = ToString(r, "Status"),
                    BranchName = ToString(r, "BranchName")
                });
            }

            return rows;
        }

        private static List<DatewiseVendorVoucherRow> MapDataTableToVoucherRows(DataTable table)
        {
            List<DatewiseVendorVoucherRow> rows = new List<DatewiseVendorVoucherRow>();
            if (table == null || table.Rows.Count == 0) return rows;

            foreach (DataRow r in table.Rows)
            {
                rows.Add(new DatewiseVendorVoucherRow
                {
                    BillKey = ToString(r, "BillKey"),
                    VendorLedgerId = ToInt(r, "VendorLedgerId"),
                    PurchaseNo = ToLong(r, "PurchaseNo"),
                    VoucherDate = ToDateTime(r, "VoucherDate"),
                    VoucherId = ToInt(r, "VoucherId"),
                    VoucherNo = ToString(r, "VoucherNo"),
                    PaymentAmount = ToDecimal(r, "PaymentAmount"),
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
