
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using ModelClass.Accounts;

namespace Repository.Accounts
{
    public class ChartOfAccountRepository : BaseRepostitory
    {
        // Get the chart of accounts data for tree view
        public ChartOfAccountResult GetChartOfAccounts(int branchId = 0)
        {
            ChartOfAccountResult result = new ChartOfAccountResult();
            List<ChartOfAccount> chartItems = new List<ChartOfAccount>();

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                // First get all account groups
                using (SqlCommand cmd = new SqlCommand("POS_AccountGroups", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");

                    if (branchId > 0)
                        cmd.Parameters.AddWithValue("@_BranchID", branchId);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dtGroups = new DataTable();
                        adapter.Fill(dtGroups);

                        if (branchId > 0 && dtGroups.Columns.Contains("BranchID"))
                        {
                            DataRow[] branchRows = dtGroups.Select($"BranchID = {branchId}");
                            dtGroups = branchRows.Length > 0 ? branchRows.CopyToDataTable() : dtGroups.Clone();
                        }

                        foreach (DataRow row in dtGroups.Rows)
                        {
                            ChartOfAccount item = new ChartOfAccount
                            {
                                Id = Convert.ToInt32(row["GroupID"]),
                                Name = row["GroupName"].ToString(),
                                Type = "Group",
                                GroupId = Convert.ToInt32(row["GroupID"]),
                                Description = row["Description"] != DBNull.Value ? row["Description"].ToString() : string.Empty,
                                BranchId = GetInt(row, "BranchID"),
                                ParentId = row.Table.Columns.Contains("ParentGroupId") && row["ParentGroupId"] != DBNull.Value ? (int?)Convert.ToInt32(row["ParentGroupId"]) : null,
                                GroupUnder = row["GroupUnder"] != DBNull.Value ? row["GroupUnder"].ToString() : string.Empty,
                                Balance = 0 // Will be calculated if needed
                            };

                            // Set the level based on parent-child relationship
                            if (item.ParentId == null || item.ParentId == 0)
                            {
                                item.Level = 0; // Root level
                                item.NodePath = item.Id.ToString();
                            }
                            else
                            {
                                // Find parent to determine level and path
                                var parent = chartItems.FirstOrDefault(p => p.Id == item.ParentId && p.Type == "Group");
                                if (parent != null)
                                {
                                    item.Level = parent.Level + 1;
                                    item.NodePath = parent.NodePath + "/" + item.Id;
                                }
                                else
                                {
                                    item.Level = 1; // Default to level 1 if parent not found
                                    item.NodePath = item.Id.ToString();
                                }
                            }

                            chartItems.Add(item);
                        }
                    }
                }

                // Then get all ledgers
                using (SqlCommand cmd = new SqlCommand("POS_Ledger", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");

                    if (branchId > 0)
                        cmd.Parameters.AddWithValue("@BranchID", branchId);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dtLedgers = new DataTable();
                        adapter.Fill(dtLedgers);

                        foreach (DataRow row in dtLedgers.Rows)
                        {
                            int groupId = Convert.ToInt32(row["GroupID"]);
                            int ledgerId = Convert.ToInt32(row["LedgerID"]);

                            ChartOfAccount item = new ChartOfAccount
                            {
                                Id = ledgerId + 100000, // Add offset to ensure unique IDs
                                Name = row["LedgerName"].ToString(),
                                Type = "Ledger",
                                GroupId = groupId,
                                LedgerId = ledgerId,
                                ParentId = groupId,
                                Description = row["Description"] != DBNull.Value ? row["Description"].ToString() : string.Empty,
                                BranchId = GetInt(row, "BranchID"),
                                Balance = row["Balance"] != DBNull.Value ? Convert.ToDecimal(row["Balance"]) : 0
                            };

                            // Find parent group to determine level and path
                            var parentGroup = chartItems.FirstOrDefault(p => p.Id == item.ParentId && p.Type == "Group");
                            if (parentGroup != null)
                            {
                                item.Level = parentGroup.Level + 1;
                                item.NodePath = parentGroup.NodePath + "/" + item.Id;
                            }
                            else
                            {
                                item.Level = 1; // Default to level 1 if parent not found
                                item.NodePath = item.Id.ToString();
                            }

                            chartItems.Add(item);
                        }
                    }
                }

                result.List = chartItems;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return result;
        }

        // Build a hierarchical tree structure from flat data
        public List<ChartOfAccountNode> BuildTreeStructure(IEnumerable<ChartOfAccount> flatItems)
        {
            var lookup = new Dictionary<int, ChartOfAccountNode>();
            var rootNodes = new List<ChartOfAccountNode>();

            // First create all nodes
            foreach (var item in flatItems)
            {
                var node = new ChartOfAccountNode
                {
                    Id = item.Id,
                    Name = item.Name,
                    Type = item.Type,
                    ParentId = item.ParentId,
                    GroupId = item.GroupId,
                    LedgerId = item.LedgerId,
                    Description = item.Description,
                    Balance = item.Balance,
                    NodePath = item.NodePath,
                    Level = item.Level
                };

                lookup[item.Id] = node;

                // If this is a root node, add it to our root collection
                if (item.ParentId == null || item.ParentId == 0 || !flatItems.Any(f => f.Id == item.ParentId))
                {
                    rootNodes.Add(node);
                }
            }

            // Then build parent-child relationships
            foreach (var item in flatItems.Where(f => f.ParentId != null && f.ParentId > 0))
            {
                if (lookup.ContainsKey(item.Id) && lookup.ContainsKey(item.ParentId.Value))
                {
                    var childNode = lookup[item.Id];
                    var parentNode = lookup[item.ParentId.Value];

                    if (!parentNode.Children.Contains(childNode))
                    {
                        parentNode.Children.Add(childNode);
                    }
                }
            }

            return rootNodes;
        }

        // Get account groups by parent ID
        public List<AccountGroupHead> GetAccountGroupsByParentId(int? parentId)
        {
            List<AccountGroupHead> groups = new List<AccountGroupHead>();

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_AccountGroups", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYPARENT");

                    if (parentId.HasValue)
                        cmd.Parameters.AddWithValue("@_ParentGroupId", parentId.Value);
                    else
                        cmd.Parameters.AddWithValue("@_ParentGroupId", DBNull.Value);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            AccountGroupHead group = new AccountGroupHead
                            {
                                GroupID = Convert.ToInt32(row["GroupID"]),
                                GroupName = row["GroupName"].ToString(),
                                Description = row["Description"] != DBNull.Value ? row["Description"].ToString() : string.Empty,
                                BranchID = GetInt(row, "BranchID"),
                                GroupCategoryID = GetInt(row, "GroupCategoryID"),
                                ParentGroupId = GetInt(row, "ParentGroupId"),
                                GroupType = row["GroupType"] != DBNull.Value ? row["GroupType"].ToString() : string.Empty,
                                GroupUnder = row["GroupUnder"] != DBNull.Value ? row["GroupUnder"].ToString() : string.Empty
                            };

                            groups.Add(group);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return groups;
        }

        // Get ledgers by group ID
        public List<Ledger> GetLedgersByGroupId(int groupId)
        {
            List<Ledger> ledgers = new List<Ledger>();

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_Ledger", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYGROUP");
                    cmd.Parameters.AddWithValue("@GroupID", groupId);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            Ledger ledger = new Ledger
                            {
                                LedgerID = Convert.ToInt32(row["LedgerID"]),
                                LedgerName = row["LedgerName"].ToString(),
                                Alias = row["Alias"] != DBNull.Value ? row["Alias"].ToString() : string.Empty,
                                Description = row["Description"] != DBNull.Value ? row["Description"].ToString() : string.Empty,
                                Notes = row["Notes"] != DBNull.Value ? row["Notes"].ToString() : string.Empty,
                                GroupID = groupId,
                                BranchID = row["BranchID"] != DBNull.Value ? Convert.ToInt32(row["BranchID"]) : 0,
                                OpnDebit = row["OpnDebit"] != DBNull.Value ? Convert.ToDecimal(row["OpnDebit"]) : 0,
                                OpnCredit = row["OpnCredit"] != DBNull.Value ? Convert.ToDecimal(row["OpnCredit"]) : 0,
                                Balance = row["Balance"] != DBNull.Value ? Convert.ToDecimal(row["Balance"]) : 0
                            };

                            ledgers.Add(ledger);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return ledgers;
        }

        private int GetInt(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToInt32(row[columnName])
                : 0;
        }
    }
}
