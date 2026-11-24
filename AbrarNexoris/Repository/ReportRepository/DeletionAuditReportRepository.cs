using ModelClass;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Repository.ReportRepository
{
    public class DeletionAuditReportRepository : BaseRepostitory
    {
        public List<DeletionAuditReportRow> GetDeletionAuditReport(DeletionAuditReportFilter filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            List<DeletionAuditReportRow> list = new List<DeletionAuditReportRow>();

            int companyId = filter.CompanyId > 0 ? filter.CompanyId : GetContextValue(SessionContext.CompanyId, DataBase.CompanyId);
            int branchId = filter.BranchId > 0 ? filter.BranchId : GetContextValue(SessionContext.BranchId, DataBase.BranchId);
            int finYearId = filter.FinYearId > 0 ? filter.FinYearId : GetContextValue(SessionContext.FinYearId, DataBase.FinyearId);

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_DeletionAuditReport, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                    cmd.Parameters.AddWithValue("@FromDate", filter.FromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", filter.ToDate.Date);
                    cmd.Parameters.AddWithValue("@TransactionType", string.IsNullOrWhiteSpace(filter.TransactionType) ? "ALL" : filter.TransactionType);
                    cmd.Parameters.AddWithValue("@UserFilter", string.IsNullOrWhiteSpace(filter.UserFilter) ? "ALL" : filter.UserFilter);
                    cmd.Parameters.AddWithValue("@SearchText", filter.SearchText ?? string.Empty);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        int sl = 1;
                        foreach (DataRow row in dt.Rows)
                        {
                            list.Add(new DeletionAuditReportRow
                            {
                                SlNo = row.Table.Columns.Contains("SlNo") && row["SlNo"] != DBNull.Value ? Convert.ToInt32(row["SlNo"]) : sl++,
                                ModuleKey = ToString(row, "ModuleKey"),
                                TransactionType = ToString(row, "TransactionType"),
                                DocNo = ToString(row, "DocNo"),
                                RefNo = ToString(row, "RefNo"),
                                DocDate = ToDateTime(row, "DocDate"),
                                DeletedDate = ToDateTime(row, "DeletedDate"),
                                PartyName = ToString(row, "PartyName"),
                                Amount = ToDecimal(row, "Amount"),
                                Qty = ToDecimal(row, "Qty"),
                                DeletedBy = ToString(row, "DeletedBy"),
                                Counter = ToString(row, "Counter"),
                                DeletionReason = ToString(row, "DeletionReason"),
                                SourceDetails = ToString(row, "SourceDetails")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetDeletionAuditReport error: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return list;
        }

        public List<string> GetDistinctUsers()
        {
            List<string> users = new List<string> { "ALL" };
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT DISTINCT UserName FROM (
                        SELECT UserName FROM dbo.SMaster WHERE CancelFlag = 1 AND UserName IS NOT NULL AND UserName <> ''
                        UNION
                        SELECT UserName FROM dbo.PMaster WHERE CancelFlag = 1 AND UserName IS NOT NULL AND UserName <> ''
                        UNION
                        SELECT UserName FROM dbo.SReturnMaster WHERE CancelFlag = 1 AND UserName IS NOT NULL AND UserName <> ''
                        UNION
                        SELECT UserName FROM dbo.PReturnMaster WHERE CancelFlag = 1 AND UserName IS NOT NULL AND UserName <> ''
                        UNION
                        SELECT UserName FROM dbo.Vouchers WHERE CancelFlag = 1 AND UserName IS NOT NULL AND UserName <> ''
                        UNION
                        SELECT UserName FROM dbo.SalesActivityLog WHERE ActivityType = 'DELETE' AND UserName IS NOT NULL AND UserName <> ''
                        UNION
                        SELECT UserName FROM dbo.PurchaseActivityLog WHERE ActivityType = 'DELETE' AND UserName IS NOT NULL AND UserName <> ''
                        UNION
                        SELECT UserName FROM dbo.ItemActivityLog WHERE ActivityType = 'DELETE' AND UserName IS NOT NULL AND UserName <> ''
                        UNION
                        SELECT UserName FROM dbo.UserActivityLog WHERE ActivityType = 'DELETE' AND UserName IS NOT NULL AND UserName <> ''
                    ) u ORDER BY UserName;", (SqlConnection)DataConnection))
                {
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            string uname = Convert.ToString(r["UserName"]);
                            if (!string.IsNullOrWhiteSpace(uname) && !users.Contains(uname))
                                users.Add(uname);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetDistinctUsers error: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return users;
        }

        private static string ToString(DataRow row, string col) => row.Table.Columns.Contains(col) && row[col] != DBNull.Value ? Convert.ToString(row[col]) : string.Empty;
        private static decimal ToDecimal(DataRow row, string col) => row.Table.Columns.Contains(col) && row[col] != DBNull.Value ? Convert.ToDecimal(row[col]) : 0m;
        private static DateTime ToDateTime(DataRow row, string col) => row.Table.Columns.Contains(col) && row[col] != DBNull.Value ? Convert.ToDateTime(row[col]) : DateTime.MinValue;
    }
}
