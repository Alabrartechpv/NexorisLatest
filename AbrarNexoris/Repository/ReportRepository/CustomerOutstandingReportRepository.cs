using ModelClass;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Repository.ReportRepository
{
    public class CustomerOutstandingReportRepository : BaseRepostitory
    {
        /// <summary>
        /// Gets bill-wise outstanding invoices for a given customer via stored procedure.
        /// Only returns bills where NetAmount != ReceivedAmount (outstanding bills).
        /// </summary>
        public List<CustomerOutstandingReportRow> GetReport(CustomerOutstandingReportFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            List<CustomerOutstandingReportRow> rows = new List<CustomerOutstandingReportRow>();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CustomerOutstandingReport, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = filter.FromDate.Date;
                    cmd.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = filter.ToDate.Date;
                    cmd.Parameters.Add("@BranchId", SqlDbType.Int).Value = filter.BranchId > 0 ? filter.BranchId : SessionContext.BranchId;
                    cmd.Parameters.Add("@LedgerId", SqlDbType.Int).Value = filter.LedgerId;
                    cmd.Parameters.Add("@_Operation", SqlDbType.VarChar, 50).Value = "GETOUTSTANDING";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);

                        foreach (DataRow row in table.Rows)
                        {
                            rows.Add(new CustomerOutstandingReportRow
                            {
                                LedgerID = ToInt(row, "LedgerID"),
                                LedgerName = ToString(row, "LedgerName"),
                                BillNo = ToLong(row, "BillNo"),
                                BillDate = ToNullableDateTime(row, "BillDate"),
                                DueDate = ToNullableDateTime(row, "DueDate"),
                                InvoiceAmount = ToDecimal(row, "InvoiceAmount"),
                                ReceivedAmount = ToDecimal(row, "ReceivedAmount"),
                                Balance = ToDecimal(row, "Balance")
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

        private static long ToLong(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToInt64(row[columnName])
                : 0;
        }

        private static int ToInt(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToInt32(row[columnName])
                : 0;
        }

        private static decimal ToDecimal(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToDecimal(row[columnName])
                : 0m;
        }

        private static DateTime? ToNullableDateTime(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? (DateTime?)Convert.ToDateTime(row[columnName])
                : null;
        }

        private static string ToString(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToString(row[columnName])
                : string.Empty;
        }
    }
}
