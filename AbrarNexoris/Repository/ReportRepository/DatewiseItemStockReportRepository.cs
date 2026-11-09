using ModelClass;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Repository.ReportRepository
{
    public class DatewiseItemStockReportRepository : BaseRepostitory
    {
        public List<DatewiseItemStockReportRow> GetReport(DatewiseItemStockReportFilter filter)
        {
            DataTable table = new DataTable();

            try
            {
                if (DataConnection.State == ConnectionState.Closed)
                {
                    DataConnection.Open();
                }

                using (SqlCommand cmd = new SqlCommand("dbo.POS_DatewiseItemStockReport", (SqlConnection)DataConnection))
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
                    cmd.Parameters.AddWithValue("@WayFilter", string.IsNullOrEmpty(filter.WayFilter) ? "ALL" : filter.WayFilter);
                    cmd.Parameters.AddWithValue("@OperationFilter", string.IsNullOrEmpty(filter.OperationFilter) ? "ALL" : filter.OperationFilter);
                    cmd.Parameters.AddWithValue("@ItemSearch", string.IsNullOrEmpty(filter.ItemSearch) ? (object)DBNull.Value : filter.ItemSearch);
                    cmd.Parameters.AddWithValue("@ViewMode", string.IsNullOrEmpty(filter.ViewMode) ? "DETAILED" : filter.ViewMode);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(table);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DatewiseItemStockReportRepository.GetReport: {ex.Message}");
                throw;
            }

            return MapDataTableToRows(table);
        }

        private static List<DatewiseItemStockReportRow> MapDataTableToRows(DataTable table)
        {
            List<DatewiseItemStockReportRow> rows = new List<DatewiseItemStockReportRow>();
            if (table == null || table.Rows.Count == 0) return rows;

            foreach (DataRow r in table.Rows)
            {
                rows.Add(new DatewiseItemStockReportRow
                {
                    TransactionDate = ToDateTime(r, "TransactionDate"),
                    Operation = ToString(r, "Operation"),
                    Way = ToString(r, "Way"),
                    RefNo = ToString(r, "RefNo"),
                    SlNo = ToInt(r, "SlNo"),
                    ItemID = ToInt(r, "ItemID"),
                    BarCode = ToString(r, "BarCode"),
                    ItemName = ToString(r, "ItemName"),
                    Unit = ToString(r, "Unit"),
                    Qty = ToDecimal(r, "Qty"),
                    InQty = ToDecimal(r, "InQty"),
                    OutQty = ToDecimal(r, "OutQty"),
                    Rate = ToDecimal(r, "Rate"),
                    Cost = ToDecimal(r, "Cost"),
                    Amount = ToDecimal(r, "Amount"),
                    Account = ToString(r, "Account"),
                    Remarks = ToString(r, "Remarks"),
                    BranchName = ToString(r, "BranchName"),
                    CurrentStock = ToDecimal(r, "CurrentStock")
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

        private static decimal ToDecimal(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value || row[column] == null)
                return 0m;
            return decimal.TryParse(row[column].ToString(), out decimal val) ? val : 0m;
        }

        private static DateTime ToDateTime(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value || row[column] == null)
                return DateTime.Today;
            return DateTime.TryParse(row[column].ToString(), out DateTime val) ? val : DateTime.Today;
        }
    }
}
