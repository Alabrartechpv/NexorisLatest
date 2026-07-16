using ModelClass;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Repository.ReportRepository
{
    public class VendorPaymentReportRepository : BaseRepostitory
    {
        private readonly Dropdowns _dropdowns;

        public VendorPaymentReportRepository()
        {
            _dropdowns = new Dropdowns();
        }

        public List<VendorPaymentReportRow> GetReport(VendorPaymentReportFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            List<VendorPaymentReportRow> rows = new List<VendorPaymentReportRow>();
            Dictionary<int, string> paymodeLookup = GetPaymodeLookup();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentReport, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = filter.CompanyId;
                    cmd.Parameters.Add("@BranchId", SqlDbType.Int).Value = filter.BranchId;
                    cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = filter.FromDate.Date;
                    cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = filter.ToDate.Date;
                    cmd.Parameters.Add("@VendorLedgerId", SqlDbType.Int).Value = filter.VendorLedgerId;

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);

                        foreach (DataRow row in table.Rows)
                        {
                            rows.Add(new VendorPaymentReportRow
                            {
                                PaymentMasterId = ToInt(row, "PaymentMasterId"),
                                BranchId = ToInt(row, "BranchId"),
                                VoucherId = ToInt(row, "VoucherId"),
                                VoucherDate = ToDateTime(row, "VoucherDate"),
                                VendorLedgerId = ToInt(row, "VendorLedgerId"),
                                PaymentMethodId = ToInt(row, "PaymentMethodId"),
                                VendorName = ToString(row, "VendorName"),
                                PurchaseNo = ToLong(row, "PurchaseNo"),
                                Amount = ToDecimal(row, "Amount"),
                                Balance = ToDecimal(row, "Balance"),
                                PaymentMode = ResolvePaymentMode(ToString(row, "PaymentMode"), ToInt(row, "PaymentMethodId"), paymodeLookup),
                                Remark = ToString(row, "Remark"),
                                Status = ToString(row, "Status"),
                                PaymentReference = ToString(row, "PaymentReference")
                            });
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

        private Dictionary<int, string> GetPaymodeLookup()
        {
            try
            {
                PaymodeDDlGrid paymodes = _dropdowns.GetPaymode();
                return paymodes.List?
                    .Where(x => x != null && x.PayModeID > 0)
                    .GroupBy(x => x.PayModeID)
                    .ToDictionary(x => x.Key, x => x.First().PayModeName ?? string.Empty)
                    ?? new Dictionary<int, string>();
            }
            catch
            {
                return new Dictionary<int, string>();
            }
        }

        private static string ResolvePaymentMode(string paymentMode, int paymentMethodId, Dictionary<int, string> paymodeLookup)
        {
            if (!string.IsNullOrWhiteSpace(paymentMode))
                return paymentMode;

            if (paymentMethodId > 0 && paymodeLookup != null && paymodeLookup.TryGetValue(paymentMethodId, out string paymodeName))
                return paymodeName ?? string.Empty;

            return string.Empty;
        }

        public List<VendorGridList> GetVendors()
        {
            VendorRepository vendorRepository = new VendorRepository();
            VendorDDLGrid data = vendorRepository.GetVendorDDL();

            if (data == null || data.List == null)
                return new List<VendorGridList>();

            return data.List
                .Where(x => x != null && x.LedgerID > 0)
                .OrderBy(x => x.LedgerName)
                .ToList();
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
