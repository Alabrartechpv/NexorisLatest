using ModelClass;
using ModelClass.Master;
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
                    cmd.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = (filter.UseDateFilter && filter.FromDate.HasValue) ? (object)filter.FromDate.Value.Date : DBNull.Value;
                    cmd.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = (filter.UseDateFilter && filter.ToDate.HasValue) ? (object)filter.ToDate.Value.Date : DBNull.Value;
                    cmd.Parameters.Add("@BranchId", SqlDbType.Int).Value = filter.BranchId > 0 ? filter.BranchId : SessionContext.BranchId;
                    cmd.Parameters.Add("@LedgerId", SqlDbType.Int).Value = filter.LedgerId;
                    cmd.Parameters.Add("@_Operation", SqlDbType.VarChar, 50).Value = "GETOUTSTANDING";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);

                        foreach (DataRow row in table.Rows)
                        {
                            long billNo = ToLong(row, "BillNo");
                            string docNo = ToString(row, "DocNo");
                            if (string.IsNullOrWhiteSpace(docNo))
                            {
                                docNo = ToString(row, "InvoiceNo");
                            }
                            if (string.IsNullOrWhiteSpace(docNo) && billNo > 0)
                            {
                                docNo = billNo.ToString();
                            }

                            string salesPerson = ToString(row, "SalesPerson");
                            if (string.IsNullOrWhiteSpace(salesPerson))
                            {
                                salesPerson = ToString(row, "SalesmanName");
                            }
                            if (string.IsNullOrWhiteSpace(salesPerson))
                            {
                                salesPerson = ToString(row, "Salesman");
                            }
                            if (string.IsNullOrWhiteSpace(salesPerson))
                            {
                                salesPerson = ToString(row, "UserName");
                            }

                            rows.Add(new CustomerOutstandingReportRow
                            {
                                LedgerID = ToInt(row, "LedgerID"),
                                LedgerName = ToString(row, "LedgerName"),
                                BillNo = billNo,
                                DocNo = docNo,
                                BillDate = ToNullableDateTime(row, "BillDate"),
                                DueDate = ToNullableDateTime(row, "DueDate"),
                                InvoiceAmount = ToDecimal(row, "InvoiceAmount"),
                                ReceivedAmount = ToDecimal(row, "ReceivedAmount"),
                                Balance = ToDecimal(row, "Balance"),
                                SalesPerson = salesPerson,
                                SalesmanId = ToNullableInt(row, "SalesmanId") ?? ToNullableInt(row, "UserId") ?? ToNullableInt(row, "EmpID"),
                                CategoryName = ToString(row, "CategoryName"),
                                CategoryId = ToNullableInt(row, "CategoryId")
                            });
                        }
                    }
                }

                EnrichSalesPerson(rows);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return rows;
        }

        private void EnrichSalesPerson(List<CustomerOutstandingReportRow> rows)
        {
            if (rows == null || rows.Count == 0) return;

            var missingRows = rows.Where(r => r.BillNo > 0 && string.IsNullOrWhiteSpace(r.SalesPerson)).ToList();
            if (missingRows.Count == 0) return;

            try
            {
                string masterTable = TableExists("SMaster") ? "SMaster" : (TableExists("SalesMaster") ? "SalesMaster" : "");
                if (string.IsNullOrEmpty(masterTable)) return;

                bool hasUserId = ColumnExists(masterTable, "UserId");
                bool hasEmpId = ColumnExists(masterTable, "EmpID") || ColumnExists(masterTable, "EmpId");
                bool hasSalesmanId = ColumnExists(masterTable, "SalesmanId") || ColumnExists(masterTable, "SalesManId");
                bool hasSalesman = ColumnExists(masterTable, "Salesman") || ColumnExists(masterTable, "SalesPerson") || ColumnExists(masterTable, "SalesmanName");

                string empCol = ColumnExists(masterTable, "EmpID") ? "EmpID" : (ColumnExists(masterTable, "EmpId") ? "EmpId" : null);
                string salesmanIdCol = ColumnExists(masterTable, "SalesmanId") ? "SalesmanId" : (ColumnExists(masterTable, "SalesManId") ? "SalesManId" : null);
                string salesmanTextCol = ColumnExists(masterTable, "Salesman") ? "Salesman" : (ColumnExists(masterTable, "SalesPerson") ? "SalesPerson" : (ColumnExists(masterTable, "SalesmanName") ? "SalesmanName" : null));

                bool hasUsers = TableExists("Users");
                bool hasLedger = TableExists("LedgerMaster");

                List<string> nameExpressions = new List<string>();
                if (salesmanTextCol != null)
                {
                    nameExpressions.Add($"NULLIF(sm.[{salesmanTextCol}], '')");
                }
                if (hasUsers && hasUserId)
                {
                    nameExpressions.Add("NULLIF(u.UserName, '')");
                }
                if (hasUsers && empCol != null)
                {
                    nameExpressions.Add("NULLIF(uEmp.UserName, '')");
                }
                if (hasUsers && salesmanIdCol != null)
                {
                    nameExpressions.Add("NULLIF(uSm.UserName, '')");
                }
                if (hasLedger && empCol != null)
                {
                    nameExpressions.Add("NULLIF(lEmp.LedgerName, '')");
                }
                if (hasLedger && salesmanIdCol != null)
                {
                    nameExpressions.Add("NULLIF(lSm.LedgerName, '')");
                }
                if (hasUserId)
                {
                    nameExpressions.Add("CASE WHEN ISNULL(sm.UserId, 0) > 0 THEN 'User ' + CAST(sm.UserId AS varchar(20)) ELSE NULL END");
                }
                nameExpressions.Add("''");

                string coalesceName = $"COALESCE({string.Join(", ", nameExpressions)})";

                List<string> idExpressions = new List<string>();
                if (salesmanIdCol != null) idExpressions.Add($"NULLIF(sm.[{salesmanIdCol}], 0)");
                if (empCol != null) idExpressions.Add($"NULLIF(sm.[{empCol}], 0)");
                if (hasUserId) idExpressions.Add($"NULLIF(sm.UserId, 0)");
                idExpressions.Add("0");
                string coalesceId = $"COALESCE({string.Join(", ", idExpressions)})";

                List<string> joins = new List<string>();
                if (hasUsers && hasUserId)
                    joins.Add("LEFT JOIN Users u ON u.UserID = sm.UserId");
                if (hasUsers && empCol != null)
                    joins.Add($"LEFT JOIN Users uEmp ON uEmp.UserID = sm.[{empCol}]");
                if (hasUsers && salesmanIdCol != null)
                    joins.Add($"LEFT JOIN Users uSm ON uSm.UserID = sm.[{salesmanIdCol}]");
                if (hasLedger && empCol != null)
                    joins.Add($"LEFT JOIN LedgerMaster lEmp ON lEmp.LedgerID = sm.[{empCol}]");
                if (hasLedger && salesmanIdCol != null)
                    joins.Add($"LEFT JOIN LedgerMaster lSm ON lSm.LedgerID = sm.[{salesmanIdCol}]");

                var billNos = missingRows.Select(r => r.BillNo).Distinct().ToList();

                for (int i = 0; i < billNos.Count; i += 500)
                {
                    var chunk = billNos.Skip(i).Take(500).ToList();
                    string inClause = string.Join(",", chunk);

                    string query = $@"
                        SELECT sm.BillNo,
                               CAST({coalesceName} AS nvarchar(150)) AS SalesPerson,
                               CAST({coalesceId} AS int) AS SalesmanId
                        FROM {masterTable} sm
                        {string.Join(Environment.NewLine, joins)}
                        WHERE sm.BillNo IN ({inClause})
                    ";

                    using (SqlCommand cmd = new SqlCommand(query, (SqlConnection)DataConnection))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var map = new Dictionary<long, Tuple<string, int?>>();
                            while (reader.Read())
                            {
                                long bNo = reader.GetInt64(0);
                                string sp = !reader.IsDBNull(1) ? reader.GetString(1)?.Trim() : "";
                                int? smId = !reader.IsDBNull(2) ? reader.GetInt32(2) : (int?)null;
                                if (!map.ContainsKey(bNo))
                                {
                                    map[bNo] = Tuple.Create(sp, smId);
                                }
                            }
                            reader.Close();

                            foreach (var row in missingRows)
                            {
                                if (map.TryGetValue(row.BillNo, out var info))
                                {
                                    if (!string.IsNullOrWhiteSpace(info.Item1) && string.IsNullOrWhiteSpace(row.SalesPerson))
                                        row.SalesPerson = info.Item1;
                                    if (info.Item2.HasValue && info.Item2.Value > 0 && !row.SalesmanId.HasValue)
                                        row.SalesmanId = info.Item2;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error enriching SalesPerson: " + ex.Message);
            }
        }

        private bool TableExists(string tableName)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@TableName", tableName);
                    object res = cmd.ExecuteScalar();
                    return res != null && res != DBNull.Value;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool ColumnExists(string tableName, string columnName)
        {
            try
            {
                string cleanTable = tableName.StartsWith("dbo.") ? tableName.Substring(4) : tableName;
                using (SqlCommand cmd = new SqlCommand("SELECT CASE WHEN COL_LENGTH(@TableName, @ColumnName) IS NULL THEN 0 ELSE 1 END;", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@TableName", "dbo." + cleanTable);
                    cmd.Parameters.AddWithValue("@ColumnName", columnName);
                    object res = cmd.ExecuteScalar();
                    return res != null && res != DBNull.Value && Convert.ToInt32(res) == 1;
                }
            }
            catch
            {
                return false;
            }
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

        public List<CategoryDDL> GetCategories()
        {
            try
            {
                CategoryDDlGrid data = new Dropdowns().getCategoryDDl(string.Empty);
                if (data == null || data.List == null)
                    return new List<CategoryDDL>();

                return data.List.Where(x => x != null && x.Id > 0).OrderBy(x => x.CategoryName).ToList();
            }
            catch
            {
                return new List<CategoryDDL>();
            }
        }

        public List<UsersDDl> GetSalesPersons()
        {
            try
            {
                UserDDlGrid data = new Dropdowns().getUsersDDl();
                if (data == null || data.List == null)
                    return new List<UsersDDl>();

                return data.List.Where(x => x != null && x.UserID > 0).OrderBy(x => x.UserName).ToList();
            }
            catch
            {
                return new List<UsersDDl>();
            }
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

        private static int? ToNullableInt(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return null;

            int val;
            return int.TryParse(row[columnName].ToString(), out val) ? (int?)val : null;
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
