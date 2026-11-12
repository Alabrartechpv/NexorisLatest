using ModelClass;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Repository.ReportRepository
{
    public class DatewiseExpenseReportRepository : BaseRepostitory
    {
        public DatewiseExpenseReportRepository()
        {
        }

        /// <summary>
        /// Retrieves expense categories (Direct Expenses, Indirect Expenses, etc.) from AccountGroupMaster.
        /// </summary>
        public List<AccountGroupDDLItem> GetExpenseCategories()
        {
            List<AccountGroupDDLItem> list = new List<AccountGroupDDLItem>();
            try
            {
                if (DataConnection.State == ConnectionState.Closed)
                {
                    DataConnection.Open();
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT GroupID, GroupName, GroupType
                    FROM AccountGroupMaster
                    WHERE GroupID IN (10, 12) 
                       OR ParentGroupID IN (10, 12)
                       OR GroupType = 'EXPENSE'
                    ORDER BY 
                        CASE WHEN GroupID = 10 THEN 1 WHEN GroupID = 12 THEN 2 ELSE 3 END,
                        GroupName", (SqlConnection)DataConnection))
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new AccountGroupDDLItem
                        {
                            GroupID = Convert.ToInt32(r["GroupID"]),
                            GroupName = r["GroupName"].ToString(),
                            GroupType = r["GroupType"]?.ToString() ?? string.Empty
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DatewiseExpenseReportRepository.GetExpenseCategories: {ex.Message}");
            }
            return list;
        }

        /// <summary>
        /// Retrieves all active Expense Ledgers for dropdown and F11 dialog filtering.
        /// </summary>
        public List<ExpenseLedgerDDLItem> GetExpenseLedgers()
        {
            List<ExpenseLedgerDDLItem> list = new List<ExpenseLedgerDDLItem>();
            try
            {
                if (DataConnection.State == ConnectionState.Closed)
                {
                    DataConnection.Open();
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT lm.LedgerID, lm.LedgerName, lm.GroupID, ag.GroupName,
                           CASE 
                               WHEN ag.GroupID = 12 OR ag.GroupName LIKE '%Indirect%' THEN 'Indirect Expense'
                               WHEN ag.GroupID = 10 OR ag.GroupName LIKE '%Direct%' THEN 'Direct Expense'
                               ELSE 'Operating Expense'
                           END AS ExpenseCategory
                    FROM LedgerMaster lm
                    JOIN AccountGroupMaster ag ON lm.GroupID = ag.GroupID
                    WHERE (ag.GroupID IN (10, 12) 
                           OR ag.ParentGroupID IN (10, 12)
                           OR (ag.GroupType = 'EXPENSE' AND ag.GroupID <> 9)
                           OR ag.GroupName LIKE '%Expense%')
                      AND ISNULL(lm.IsDelete, 0) = 0
                    ORDER BY lm.LedgerName", (SqlConnection)DataConnection))
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new ExpenseLedgerDDLItem
                        {
                            LedgerID = Convert.ToInt32(r["LedgerID"]),
                            LedgerName = r["LedgerName"].ToString(),
                            GroupID = Convert.ToInt32(r["GroupID"]),
                            GroupName = r["GroupName"].ToString(),
                            ExpenseCategory = r["ExpenseCategory"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DatewiseExpenseReportRepository.GetExpenseLedgers: {ex.Message}");
            }
            return list;
        }

        /// <summary>
        /// Executes POS_DatewiseExpenseReport and returns mapped data and metrics.
        /// </summary>
        public DatewiseExpenseReportData GetReportData(DatewiseExpenseReportFilter filter)
        {
            DatewiseExpenseReportData result = new DatewiseExpenseReportData();
            DataSet ds = new DataSet("ExpenseReportDataSet");

            try
            {
                if (DataConnection.State == ConnectionState.Closed)
                {
                    DataConnection.Open();
                }

                using (SqlCommand cmd = new SqlCommand("dbo.POS_DatewiseExpenseReport", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;

                    cmd.Parameters.AddWithValue("@CompanyId", filter.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", filter.BranchId);
                    cmd.Parameters.AddWithValue("@PeriodMode", string.IsNullOrEmpty(filter.PeriodMode) ? "DATE_RANGE" : filter.PeriodMode);
                    cmd.Parameters.AddWithValue("@FromDate", filter.FromDate != DateTime.MinValue ? (object)filter.FromDate : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", filter.ToDate != DateTime.MinValue ? (object)filter.ToDate : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Month", filter.Month);
                    cmd.Parameters.AddWithValue("@Year", filter.Year);
                    cmd.Parameters.AddWithValue("@ExpenseCategoryId", filter.ExpenseCategoryId);
                    cmd.Parameters.AddWithValue("@LedgerId", filter.LedgerId);
                    cmd.Parameters.AddWithValue("@PaymentMode", string.IsNullOrEmpty(filter.PaymentMode) ? "ALL" : filter.PaymentMode);
                    cmd.Parameters.AddWithValue("@SearchText", string.IsNullOrEmpty(filter.SearchText) ? (object)DBNull.Value : filter.SearchText);
                    cmd.Parameters.AddWithValue("@ViewMode", string.IsNullOrEmpty(filter.ViewMode) ? "DATE_MATRIX" : filter.ViewMode);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(ds);
                    }
                }

                result.ReportDataSet = ds;

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    if (filter.ViewMode == "DATE_MATRIX")
                    {
                        result.ExpenseTransactions = MapDataTableToExpenseRows(ds.Tables[0]);
                    }
                }

                if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                {
                    result.VoucherDetails = MapDataTableToVoucherRows(ds.Tables[1]);
                }

                // Compute summary statistics
                if (result.ExpenseTransactions.Count > 0)
                {
                    result.GrandTotalExpense = result.ExpenseTransactions.Sum(x => x.ExpenseAmount);
                    result.TotalDirectExpense = result.ExpenseTransactions.Where(x => x.ExpenseCategory == "Direct Expense").Sum(x => x.ExpenseAmount);
                    result.TotalIndirectExpense = result.ExpenseTransactions.Where(x => x.ExpenseCategory == "Indirect Expense").Sum(x => x.ExpenseAmount);
                    result.TotalVouchers = result.ExpenseTransactions.Select(x => x.VoucherID).Distinct().Count();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DatewiseExpenseReportRepository.GetReportData: {ex.Message}");
                throw;
            }

            return result;
        }

        private static List<DatewiseExpenseReportRow> MapDataTableToExpenseRows(DataTable table)
        {
            List<DatewiseExpenseReportRow> rows = new List<DatewiseExpenseReportRow>();
            if (table == null || table.Rows.Count == 0) return rows;

            foreach (DataRow r in table.Rows)
            {
                rows.Add(new DatewiseExpenseReportRow
                {
                    VoucherID = r.Table.Columns.Contains("VoucherID") && r["VoucherID"] != DBNull.Value ? Convert.ToInt32(r["VoucherID"]) : 0,
                    VoucherNumber = r.Table.Columns.Contains("VoucherNumber") ? r["VoucherNumber"]?.ToString() : string.Empty,
                    VoucherDate = r.Table.Columns.Contains("VoucherDate") && r["VoucherDate"] != DBNull.Value ? Convert.ToDateTime(r["VoucherDate"]) : DateTime.MinValue,
                    VoucherType = r.Table.Columns.Contains("VoucherType") ? r["VoucherType"]?.ToString() : "GENPAY",
                    LedgerID = r.Table.Columns.Contains("LedgerID") && r["LedgerID"] != DBNull.Value ? Convert.ToInt32(r["LedgerID"]) : 0,
                    LedgerName = r.Table.Columns.Contains("LedgerName") ? r["LedgerName"]?.ToString() : string.Empty,
                    GroupID = r.Table.Columns.Contains("GroupID") && r["GroupID"] != DBNull.Value ? Convert.ToInt32(r["GroupID"]) : 0,
                    GroupName = r.Table.Columns.Contains("GroupName") ? r["GroupName"]?.ToString() : string.Empty,
                    ExpenseCategory = r.Table.Columns.Contains("ExpenseCategory") ? r["ExpenseCategory"]?.ToString() : "Operating Expense",
                    ExpenseAmount = r.Table.Columns.Contains("ExpenseAmount") && r["ExpenseAmount"] != DBNull.Value ? Convert.ToDecimal(r["ExpenseAmount"]) : 0m,
                    CreditLedgerID = r.Table.Columns.Contains("CreditLedgerID") && r["CreditLedgerID"] != DBNull.Value ? Convert.ToInt32(r["CreditLedgerID"]) : 0,
                    PaidThrough = r.Table.Columns.Contains("PaidThrough") ? r["PaidThrough"]?.ToString() : string.Empty,
                    PaymentMode = r.Table.Columns.Contains("PaymentMode") ? r["PaymentMode"]?.ToString() : "CASH",
                    Narration = r.Table.Columns.Contains("Narration") ? r["Narration"]?.ToString() : string.Empty,
                    UserName = r.Table.Columns.Contains("UserName") ? r["UserName"]?.ToString() : "Admin",
                    BranchID = r.Table.Columns.Contains("BranchID") && r["BranchID"] != DBNull.Value ? Convert.ToInt32(r["BranchID"]) : 0,
                    BranchName = r.Table.Columns.Contains("BranchName") ? r["BranchName"]?.ToString() : string.Empty,
                    CompanyID = r.Table.Columns.Contains("CompanyID") && r["CompanyID"] != DBNull.Value ? Convert.ToInt32(r["CompanyID"]) : 1
                });
            }

            return rows;
        }

        private static List<DatewiseExpenseVoucherRow> MapDataTableToVoucherRows(DataTable table)
        {
            List<DatewiseExpenseVoucherRow> rows = new List<DatewiseExpenseVoucherRow>();
            if (table == null || table.Rows.Count == 0) return rows;

            foreach (DataRow r in table.Rows)
            {
                rows.Add(new DatewiseExpenseVoucherRow
                {
                    LedgerID = r.Table.Columns.Contains("LedgerID") && r["LedgerID"] != DBNull.Value ? Convert.ToInt32(r["LedgerID"]) : 0,
                    VoucherID = r.Table.Columns.Contains("VoucherID") && r["VoucherID"] != DBNull.Value ? Convert.ToInt32(r["VoucherID"]) : 0,
                    VoucherNumber = r.Table.Columns.Contains("VoucherNumber") ? r["VoucherNumber"]?.ToString() : string.Empty,
                    VoucherDate = r.Table.Columns.Contains("VoucherDate") && r["VoucherDate"] != DBNull.Value ? Convert.ToDateTime(r["VoucherDate"]) : DateTime.MinValue,
                    VoucherType = r.Table.Columns.Contains("VoucherType") ? r["VoucherType"]?.ToString() : "GENPAY",
                    LedgerName = r.Table.Columns.Contains("LedgerName") ? r["LedgerName"]?.ToString() : string.Empty,
                    GroupName = r.Table.Columns.Contains("GroupName") ? r["GroupName"]?.ToString() : string.Empty,
                    ExpenseCategory = r.Table.Columns.Contains("ExpenseCategory") ? r["ExpenseCategory"]?.ToString() : "Operating Expense",
                    ExpenseAmount = r.Table.Columns.Contains("ExpenseAmount") && r["ExpenseAmount"] != DBNull.Value ? Convert.ToDecimal(r["ExpenseAmount"]) : 0m,
                    PaidThrough = r.Table.Columns.Contains("PaidThrough") ? r["PaidThrough"]?.ToString() : string.Empty,
                    PaymentMode = r.Table.Columns.Contains("PaymentMode") ? r["PaymentMode"]?.ToString() : "CASH",
                    Narration = r.Table.Columns.Contains("Narration") ? r["Narration"]?.ToString() : string.Empty,
                    UserName = r.Table.Columns.Contains("UserName") ? r["UserName"]?.ToString() : "Admin",
                    BranchName = r.Table.Columns.Contains("BranchName") ? r["BranchName"]?.ToString() : string.Empty
                });
            }

            return rows;
        }
    }

    public class AccountGroupDDLItem
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public string GroupType { get; set; }
    }

    public class ExpenseLedgerDDLItem
    {
        public int LedgerID { get; set; }
        public string LedgerName { get; set; }
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public string ExpenseCategory { get; set; }
    }
}
