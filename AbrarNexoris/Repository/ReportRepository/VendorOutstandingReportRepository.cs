using ModelClass;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Repository.ReportRepository
{
    public class VendorOutstandingReportRepository : BaseRepostitory
    {
        public List<VendorOutstandingReportRow> GetReport(VendorOutstandingReportFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            List<VendorOutstandingReportRow> rows = new List<VendorOutstandingReportRow>();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_VendorOutstandingListing, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = filter.CompanyId > 0 ? (object)filter.CompanyId : DBNull.Value;
                    cmd.Parameters.Add("@BranchId", SqlDbType.Int).Value = filter.BranchId > 0 ? (object)filter.BranchId : DBNull.Value;
                    cmd.Parameters.Add("@FinYearId", SqlDbType.Int).Value = filter.FinYearId > 0 ? (object)filter.FinYearId : DBNull.Value;
                    cmd.Parameters.Add("@LedgerId", SqlDbType.Int).Value = filter.LedgerId > 0 ? (object)filter.LedgerId : DBNull.Value;
                    cmd.Parameters.Add("@FromLedgerId", SqlDbType.Int).Value = filter.FromLedgerId > 0 ? (object)filter.FromLedgerId : DBNull.Value;
                    cmd.Parameters.Add("@ToLedgerId", SqlDbType.Int).Value = filter.ToLedgerId > 0 ? (object)filter.ToLedgerId : DBNull.Value;
                    cmd.Parameters.Add("@DateFilterMode", SqlDbType.VarChar, 20).Value = string.IsNullOrWhiteSpace(filter.DateFilterMode) ? (object)DBNull.Value : filter.DateFilterMode;
                    cmd.Parameters.Add("@UseDateFilter", SqlDbType.Bit).Value = filter.UseDateFilter;
                    cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = filter.FromDate.Date;
                    cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = filter.ToDate.Date;
                    cmd.Parameters.Add("@PaymentDueOnly", SqlDbType.Bit).Value = filter.PaymentDueOnly;
                    cmd.Parameters.Add("@GetUnallocatedReturnsOnly", SqlDbType.Bit).Value = filter.GetUnallocatedReturnsOnly;

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);

                        foreach (DataRow row in table.Rows)
                        {
                            rows.Add(new VendorOutstandingReportRow
                            {
                                AcctCode = ToInt(row, "AcctCode"),
                                Company = ToString(row, "Company"),
                                Name = ToString(row, "Name"),
                                Phone = ToString(row, "Phone"),
                                PurchaseNo = ToLong(row, "PurchaseNo"),
                                Date = ToDateTime(row, "Date"),
                                Reference = ToString(row, "Reference"),
                                InvoiceDate = ToNullableDateTime(row, "InvoiceDate"),
                                PostDate = ToNullableDateTime(row, "PostDate"),
                                DocAmt = ToDecimal(row, "DocAmt"),
                                Balance = ToDecimal(row, "Balance"),
                                IsPR = ToInt(row, "IsPR")
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

        private static DateTime? ToNullableDateTime(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToDateTime(row[columnName])
                : (DateTime?)null;
        }

        private static string ToString(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToString(row[columnName])
                : string.Empty;
        }
    }
}
