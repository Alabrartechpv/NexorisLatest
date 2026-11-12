using Dapper;
using ModelClass.Master;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Repository.MasterRepositry
{
    /// <summary>
    /// Repository for Role-Based Access Control (RBAC) operations.
    /// All database operations are executed via clean stored procedures (POS_RolePermission).
    /// </summary>
    public class RolePermissionRepository : BaseRepostitory
    {
        private static bool _rolesSeeded = false;
        private static bool _formsSeeded = false;

        /// <summary>
        /// Resolves all matching Role IDs across dbo.Roles and dbo.UserLevel for a given Role ID or Role Name.
        /// </summary>
        public List<int> GetTargetRoleIds(int roleId, string roleName = null)
        {
            var ids = new HashSet<int>();
            if (roleId > 0) ids.Add(roleId);

            try
            {
                var allRoles = GetAllRoles();
                string resolvedName = roleName?.Trim();

                if (string.IsNullOrWhiteSpace(resolvedName) && roleId > 0)
                {
                    var matchedRole = allRoles.FirstOrDefault(r => r.RoleID == roleId);
                    if (matchedRole != null && !string.IsNullOrWhiteSpace(matchedRole.RoleName))
                    {
                        resolvedName = matchedRole.RoleName.Trim();
                    }
                }

                if (!string.IsNullOrWhiteSpace(resolvedName))
                {
                    bool isTargetAdmin = string.Equals(resolvedName, "Admin", StringComparison.OrdinalIgnoreCase) ||
                                         string.Equals(resolvedName, "Administrator", StringComparison.OrdinalIgnoreCase);

                    var byName = allRoles.Where(r => !string.IsNullOrWhiteSpace(r.RoleName) &&
                        (isTargetAdmin
                            ? (string.Equals(r.RoleName.Trim(), "Admin", StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(r.RoleName.Trim(), "Administrator", StringComparison.OrdinalIgnoreCase))
                            : string.Equals(r.RoleName.Trim(), resolvedName, StringComparison.OrdinalIgnoreCase)));

                    foreach (var r in byName)
                    {
                        if (r.RoleID > 0) ids.Add(r.RoleID);
                        if (r.UserLevelID.HasValue && r.UserLevelID.Value > 0) ids.Add(r.UserLevelID.Value);
                    }
                }
                else
                {
                    var matched = allRoles.Where(r => roleId > 0 && r.RoleID == roleId).ToList();
                    foreach (var r in matched)
                    {
                        if (r.RoleID > 0) ids.Add(r.RoleID);
                        if (r.UserLevelID.HasValue && r.UserLevelID.Value > 0) ids.Add(r.UserLevelID.Value);
                    }
                }
            }
            catch { }

            return ids.Count > 0 ? ids.ToList() : (roleId > 0 ? new List<int> { roleId } : new List<int>());
        }

        /// <summary>
        /// Gets all permissions for a specific role.
        /// </summary>
        /// <param name="roleId">Role ID to get permissions for</param>
        /// <param name="roleName">Optional Role Name</param>
        /// <returns>List of RolePermission objects</returns>
        public List<RolePermission> GetPermissionsByRoleId(int roleId, string roleName = null)
        {
            List<RolePermission> savedPermissions = new List<RolePermission>();

            // Resolve all matching role IDs (e.g., both RoleID=4 and UserLevelID=3 for Cashier)
            List<int> targetRoleIds = GetTargetRoleIds(roleId, roleName);
            try
            {
                var allRoles = GetAllRoles();
                var currentRole = allRoles.FirstOrDefault(r => r.RoleID == roleId);
                if (currentRole != null && !string.IsNullOrWhiteSpace(currentRole.RoleName))
                {
                    var matchingRoles = allRoles.Where(r => string.Equals(r.RoleName, currentRole.RoleName, StringComparison.OrdinalIgnoreCase));
                    foreach (var mr in matchingRoles)
                    {
                        if (mr.RoleID > 0 && !targetRoleIds.Contains(mr.RoleID)) targetRoleIds.Add(mr.RoleID);
                        if (mr.UserLevelID.HasValue && mr.UserLevelID.Value > 0 && !targetRoleIds.Contains(mr.UserLevelID.Value)) targetRoleIds.Add(mr.UserLevelID.Value);
                    }
                }
            }
            catch { }

            string idListStr = string.Join(",", targetRoleIds);

            try
            {
                EnsureAllFormsSeededInDatabase();

                if (DataConnection.State != ConnectionState.Open) DataConnection.Open();

                // 1. Query saved rows via stored procedure POS_RolePermission with GETBYROLE
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_RolePermission, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RoleID", roleId);
                    cmd.Parameters.AddWithValue("@RoleIds", idListStr);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYROLE");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            savedPermissions = ds.Tables[0].ToListOfObject<RolePermission>();
                        }
                    }
                }

                // 2. Fallback: if multi-role query returned empty, query for each target role ID individually
                if ((savedPermissions == null || savedPermissions.Count == 0) && targetRoleIds.Count > 0)
                {
                    foreach (int tid in targetRoleIds)
                    {
                        using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_RolePermission, (SqlConnection)DataConnection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@RoleID", tid);
                            cmd.Parameters.AddWithValue("@_Operation", "GETBYROLE");

                            using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                            {
                                DataSet ds = new DataSet();
                                adapt.Fill(ds);
                                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                                {
                                    var list = ds.Tables[0].ToListOfObject<RolePermission>();
                                    if (list != null && list.Count > 0)
                                    {
                                        if (savedPermissions == null) savedPermissions = new List<RolePermission>();
                                        savedPermissions.AddRange(list);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting permissions: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            // Check if role is Admin
            if (string.IsNullOrWhiteSpace(roleName) && roleId > 0)
            {
                try
                {
                    var roleObj = GetAllRoles().FirstOrDefault(r => r.RoleID == roleId);
                    if (roleObj != null) roleName = roleObj.RoleName ?? "";
                }
                catch { }
            }

            bool isAdmin = string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(roleName, "Administrator", StringComparison.OrdinalIgnoreCase);

            // Merge saved permissions with all database forms (and comprehensive fallbacks)
            var resultPermissions = new List<RolePermission>();
            var staticForms = GetFormsFromDatabase();

            foreach (var sf in staticForms)
            {
                var matchingList = savedPermissions?.Where(p =>
                    (p.FormID > 0 && p.FormID == sf.FormID) ||
                    (!string.IsNullOrEmpty(p.FormKey) && string.Equals(p.FormKey, sf.FormKey, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.FormName) && string.Equals(p.FormName, sf.FormName, StringComparison.OrdinalIgnoreCase))).ToList();

                var saved = (matchingList != null && matchingList.Count > 0)
                    ? new RolePermission
                    {
                        RoleID = roleId,
                        FormID = sf.FormID,
                        FormKey = sf.FormKey,
                        FormName = sf.FormName,
                        Category = sf.Category,
                        CanView = matchingList.Any(m => m.CanView),
                        CanAdd = matchingList.Any(m => m.CanAdd),
                        CanEdit = matchingList.Any(m => m.CanEdit),
                        CanDelete = matchingList.Any(m => m.CanDelete)
                    }
                    : null;

                if (saved != null)
                {
                    resultPermissions.Add(new RolePermission
                    {
                        RoleID = roleId,
                        FormID = sf.FormID,
                        FormKey = sf.FormKey,
                        FormName = sf.FormName,
                        Category = sf.Category,
                        CanView = saved.CanView,
                        CanAdd = saved.CanAdd,
                        CanEdit = saved.CanEdit,
                        CanDelete = saved.CanDelete
                    });
                }
                else
                {
                    // Default to FALSE for non-admin, TRUE for admin
                    resultPermissions.Add(new RolePermission
                    {
                        RoleID = roleId,
                        FormID = sf.FormID,
                        FormKey = sf.FormKey,
                        FormName = sf.FormName,
                        Category = sf.Category,
                        CanView = isAdmin,
                        CanAdd = isAdmin,
                        CanEdit = isAdmin,
                        CanDelete = isAdmin
                    });
                }
            }

            return resultPermissions;
        }

        private List<RolePermission> GenerateDefaultPermissionsForRole(int roleId, string roleName = null)
        {
            var result = new List<RolePermission>();
            var allForms = GetFormsFromDatabase();

            if (string.IsNullOrWhiteSpace(roleName) && roleId > 0)
            {
                try
                {
                    var roleObj = GetAllRoles().FirstOrDefault(r => r.RoleID == roleId);
                    if (roleObj != null) roleName = roleObj.RoleName ?? "";
                }
                catch { }
            }

            bool isAdminRole = string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(roleName, "Administrator", StringComparison.OrdinalIgnoreCase);

            foreach (var f in allForms)
            {
                bool canView = isAdminRole;
                bool canAdd = isAdminRole;
                bool canEdit = isAdminRole;
                bool canDelete = isAdminRole;

                result.Add(new RolePermission
                {
                    RoleID = roleId,
                    FormID = f.FormID,
                    FormKey = f.FormKey,
                    FormName = f.FormName,
                    Category = f.Category,
                    CanView = canView,
                    CanAdd = canAdd,
                    CanEdit = canEdit,
                    CanDelete = canDelete
                });
            }

            // Save these initial default permissions to database so admin UI reflects them
            try
            {
                var gridList = result.Select(r => new FormPermissionGrid
                {
                    FormID = r.FormID,
                    FormKey = r.FormKey,
                    FormName = r.FormName,
                    Category = r.Category,
                    CanView = r.CanView,
                    CanAdd = r.CanAdd,
                    CanEdit = r.CanEdit,
                    CanDelete = r.CanDelete
                }).ToList();

                SavePermissions(roleId, gridList, roleName);
            }
            catch { }

            return result;
        }

        /// <summary>
        /// Gets Role ID by role name
        /// </summary>
        /// <param name="roleName">Role name (UserLevel)</param>
        /// <returns>Role ID or 0 if not found</returns>
        public int GetRoleIdByName(string roleName)
        {
            int roleId = 0;
            if (DataConnection.State != ConnectionState.Open) DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_RolePermission, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RoleName", roleName ?? "");
                    cmd.Parameters.AddWithValue("@_Operation", "GETROLEBYNAME");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            roleId = Convert.ToInt32(ds.Tables[0].Rows[0]["RoleID"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting role ID: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            if (roleId <= 0 && !string.IsNullOrWhiteSpace(roleName))
            {
                try
                {
                    string clean = roleName.Trim();
                    var matchingRole = GetAllRoles().FirstOrDefault(r =>
                        string.Equals(r.RoleName, clean, StringComparison.OrdinalIgnoreCase) ||
                        r.RoleID.ToString() == clean ||
                        (r.UserLevelID.HasValue && r.UserLevelID.Value.ToString() == clean));

                    if (matchingRole != null) roleId = matchingRole.RoleID;
                }
                catch { }
            }

            return roleId;
        }

        /// <summary>
        /// Gets Role ID by UserLevelID (from Users table)
        /// </summary>
        /// <param name="userLevelId">UserLevelID from Users table</param>
        /// <returns>Role ID or 0 if not found</returns>
        public int GetRoleIdByUserLevelId(int userLevelId)
        {
            int roleId = 0;
            if (DataConnection.State != ConnectionState.Open) DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_RolePermission, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RoleID", userLevelId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETROLEBYLEVELID");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            roleId = Convert.ToInt32(ds.Tables[0].Rows[0]["RoleID"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting role ID by UserLevelID: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return roleId;
        }

        /// <summary>
        /// Gets all active roles
        /// </summary>
        /// <returns>List of Role objects</returns>
        public List<Role> GetAllRoles()
        {
            List<Role> roles = new List<Role>();

            // 0. Ensure default roles and IDs exist in database
            EnsureRolesSeededInDatabase();

            // 1. Query roles via stored procedure
            try
            {
                if (DataConnection.State != ConnectionState.Open) DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_RolePermission, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETALLROLES");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            roles = ds.Tables[0].ToListOfObject<Role>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error querying roles: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            // Fallback: If list is empty, supply default roles
            if (roles == null || roles.Count == 0)
            {
                roles = new List<Role>
                {
                    new Role { RoleID = 1, RoleName = "Admin", UserLevelID = 1 },
                    new Role { RoleID = 2, RoleName = "Manager", UserLevelID = 2 },
                    new Role { RoleID = 3, RoleName = "Cashier", UserLevelID = 3 },
                    new Role { RoleID = 5, RoleName = "Supervisor", UserLevelID = 5 },
                    new Role { RoleID = 6, RoleName = "Sales Man", UserLevelID = 6 },
                    new Role { RoleID = 7, RoleName = "Accountant", UserLevelID = 7 },
                    new Role { RoleID = 8, RoleName = "Inventory Manager", UserLevelID = 8 },
                    new Role { RoleID = 9, RoleName = "Standard User", UserLevelID = 9 }
                };
            }

            if (roles != null && roles.Count > 0)
            {
                var clean = new List<Role>();
                bool adminAdded = false;
                foreach (var r in roles)
                {
                    if (string.IsNullOrWhiteSpace(r.RoleName)) continue;
                    string name = r.RoleName.Trim();
                    bool isAdminName = string.Equals(name, "Admin", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(name, "Administrator", StringComparison.OrdinalIgnoreCase);
                    if (isAdminName)
                    {
                        if (!adminAdded)
                        {
                            adminAdded = true;
                            clean.Add(new Role { RoleID = r.RoleID, RoleName = "Admin", UserLevelID = r.UserLevelID });
                        }
                    }
                    else if (!clean.Any(c => string.Equals(c.RoleName, name, StringComparison.OrdinalIgnoreCase)))
                    {
                        clean.Add(r);
                    }
                }
                roles = clean;
            }

            return roles;
        }

        /// <summary>
        /// Gets all forms with their permissions for a specific role (for admin UI)
        /// </summary>
        /// <param name="roleId">Role ID to get permissions for</param>
        /// <param name="roleName">Optional Role Name</param>
        /// <returns>List of FormPermissionGrid objects</returns>
        public List<FormPermissionGrid> GetFormsWithPermissions(int roleId, string roleName = null)
        {
            var rolePerms = GetPermissionsByRoleId(roleId, roleName);
            var staticList = GetFormsFromDatabase();
            var resultGrid = new List<FormPermissionGrid>();

            foreach (var p in rolePerms)
            {
                var sf = staticList.FirstOrDefault(s => (s.FormID > 0 && s.FormID == p.FormID) ||
                    (!string.IsNullOrEmpty(s.FormKey) && string.Equals(s.FormKey, p.FormKey, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(s.FormName) && string.Equals(s.FormName, p.FormName, StringComparison.OrdinalIgnoreCase)));

                resultGrid.Add(new FormPermissionGrid
                {
                    FormID = p.FormID,
                    FormKey = p.FormKey,
                    FormName = p.FormName,
                    Category = sf != null && !string.IsNullOrWhiteSpace(sf.Category) ? sf.Category : (!string.IsNullOrWhiteSpace(p.Category) ? p.Category : "General"),
                    CanView = p.CanView,
                    CanAdd = p.CanAdd,
                    CanEdit = p.CanEdit,
                    CanDelete = p.CanDelete
                });
            }

            return resultGrid;
        }

        private List<FormPermissionGrid> GetFormsFromDatabase()
        {
            var list = new List<FormPermissionGrid>();
            try
            {
                EnsureAllFormsSeededInDatabase();

                if (DataConnection.State != ConnectionState.Open) DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_RolePermission, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETFORMS");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);
                        foreach (DataRow row in dt.Rows)
                        {
                            list.Add(new FormPermissionGrid
                            {
                                FormID = Convert.ToInt32(row["FormID"]),
                                FormKey = row["FormKey"]?.ToString() ?? "",
                                FormName = row["FormName"]?.ToString() ?? "",
                                Category = row["Category"]?.ToString() ?? "General",
                                CanView = false,
                                CanAdd = false,
                                CanEdit = false,
                                CanDelete = false
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error querying dbo.Forms: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            if (list == null || list.Count == 0)
            {
                list = GetStaticFormPermissionsList();
            }

            return list;
        }

        private static List<FormPermissionGrid> GetStaticFormPermissionsList()
        {
            var list = new List<FormPermissionGrid>();
            int id = 1;
            Action<string, string, string> add = (key, name, cat) =>
            {
                list.Add(new FormPermissionGrid
                {
                    FormID = id++,
                    FormKey = key,
                    FormName = name,
                    Category = cat,
                    CanView = true,
                    CanAdd = true,
                    CanEdit = true,
                    CanDelete = true
                });
            };

            // Master
            add("company", "Company Setup", "Master");
            add("branch", "Branch Setup", "Master");
            add("state", "State Master", "Master");
            add("country", "Country Master", "Master");
            add("currency", "Currency Master", "Master");
            add("group", "Group Master", "Master");
            add("category", "Category Master", "Master");
            add("brand", "Brand Master", "Master");
            add("ItemMaster", "Item Master", "Master");
            add("itemtype", "Item Type Master", "Master");
            add("unit", "Unit Master", "Master");
            add("line", "Line Master", "Master");
            add("rack", "Rack Master", "Master");
            add("row", "Row Master", "Master");
            add("reason", "Reason Master", "Master");
            add("users", "Users & Security Setup", "Master");
            add("paymode", "General Paymode Setup", "Master");
            add("TaxManagement", "Tax Management & Rates", "Master");
            add("counter", "Counter Master", "Master");
            add("pricesetting", "Price Level Settings", "Master");

            // Transaction
            add("pos", "POS Terminal", "Transaction");
            add("sales", "Sales Invoice", "Transaction");
            add("salesreturn", "Sales Return", "Transaction");
            add("purchase", "Purchase Invoice", "Transaction");
            add("purchaseorder", "Purchase Order", "Transaction");
            add("purchasereturn", "Purchase Return", "Transaction");
            add("stockadjustment", "Stock Adjustment", "Transaction");
            add("stocktransfer", "Stock Transfer", "Transaction");
            add("grn", "Goods Received Notes (GRN)", "Transaction");
            add("quotation", "Sales Quotation / Estimate", "Transaction");
            add("deliverychallan", "Delivery Challan", "Transaction");

            // Accounts
            add("customer", "Customer Management", "Accounts");
            add("vendor", "Vendor Management", "Accounts");
            add("ledgers", "Account Ledgers", "Accounts");
            add("accountgroup", "Account Group Setup", "Accounts");
            add("chartofaccount", "Chart of Accounts", "Accounts");
            add("receipt", "Customer Receipt", "Accounts");
            add("payment", "Vendor Payment", "Accounts");
            add("generalpayment", "General Payment Voucher", "Accounts");
            add("generalreceipt", "General Receipt Voucher", "Accounts");
            add("contra", "Contra Voucher", "Accounts");
            add("journal", "Journal Voucher", "Accounts");
            add("debitnote", "Debit Note Voucher", "Accounts");
            add("creditnote", "Credit Note Voucher", "Accounts");
            add("bankreconciliation", "Bank Reconciliation", "Accounts");
            add("manualpartybalance", "Manual Party Balance Entry", "Accounts");
            add("expenseentry", "Expense Voucher Entry", "Accounts");

            // Reports & Analytics - ALL REPORTS
            add("dashboard", "Executive Dashboard & KPIs", "Reports");
            add("smartreorder", "Smart Reorder Dashboard", "Reports");
            add("salesanalytics", "Sales Analytics Dashboard", "Reports");
            add("purchaseanalytics", "Purchase Analytics Dashboard", "Reports");
            add("stockanalytics", "Stock Analytics Dashboard", "Reports");
            add("salesreport", "Sales Summary Report", "Reports");
            add("salesdetails", "Sales Details Report", "Reports");
            add("salesreturnreport", "Sales Return Report", "Reports");
            add("salesreturndetails", "Sales Return Details Report", "Reports");
            add("salesprofit", "Sales Profit Report", "Reports");
            add("salesmanincentive", "Salesman Incentive Report", "Reports");
            add("salesmanwisesalessummary", "Salesmanwise Sales Summary Report", "Reports");
            add("customerwisesalessummary", "Customerwise Sales Summary Report", "Reports");
            add("counterreport", "Counter Sales Report", "Reports");
            add("purchasereport", "Purchase Summary Report", "Reports");
            add("purchasedetails", "Purchase Details Report", "Reports");
            add("purchasereturnreport", "Purchase Return Report", "Reports");
            add("purchasereturndetails", "Purchase Return Details Report", "Reports");
            add("vendorpurchasereport", "Vendor Purchase Report", "Reports");
            add("grnreport", "Goods Received (GRN) Report", "Reports");
            add("stockreport", "Stock Report", "Reports");
            add("stockreportadvanced", "Advanced Stock Report", "Reports");
            add("stockadjustmentreport", "Stock Adjustment Report", "Reports");
            add("stockvaluation", "Stock Valuation Report", "Reports");
            add("stockaudit", "Stock Audit Report", "Reports");
            add("lowstockalert", "Low Stock Alert Report", "Reports");
            add("inactiveitemsreport", "Inactive Items Report", "Reports");
            add("itemreport", "Item Report & Details", "Reports");
            add("itemwisesalessummary", "Itemwise Sales Summary Report", "Reports");
            add("cashinout", "Cash In / Out Report", "Reports");
            add("daybook", "Day Book", "Reports");
            add("cashbankbook", "Cash & Bank Book", "Reports");
            add("generalledger", "General Ledger Statement", "Reports");
            add("bankstatement", "Bank Statement Report", "Reports");
            add("shiftreconciliation", "Shift Reconciliation Report", "Reports");
            add("customeroutstanding", "Customer Outstanding Report", "Reports");
            add("customerreceiptreport", "Customer Receipt Report", "Reports");
            add("customerledger", "Customer Ledger Report", "Reports");
            add("vendoroutstanding", "Vendor Outstanding Report", "Reports");
            add("vendorpaymentreport", "Vendor Payment Report", "Reports");
            add("combinedpartybalancereport", "Combined Party Balance Report", "Reports");
            add("manualpartybalancereport", "Manual Party Balance Report", "Reports");
            add("tradingpl", "Trading & Profit / Loss Account", "Reports");
            add("profitloss", "Profit and Loss Account", "Reports");
            add("balancesheet", "Balance Sheet", "Reports");
            add("trialbalance", "Trial Balance", "Reports");
            add("businessexpense", "Business Expense Report", "Reports");
            add("inputgst", "Input GST Report", "Reports");

            add("outputgst", "Output GST Report", "Reports");
            add("govtgst", "Government GST Return Report", "Reports");

            // Settings
            add("possettings", "Sale Settings & Preferences", "Settings");
            add("rolepermissions", "Role Permissions Management", "Settings");
            add("ActivityLog", "Activity Log (Master)", "Settings");
            add("itemhistory", "Item History Log", "Settings");
            add("transactionactivity", "Transaction Activity Log", "Settings");
            add("useractivity", "User Activity Log", "Settings");
            add("itemstockactivity", "Stock Activity Log", "Settings");
            add("yearclosing", "Financial Year Closing", "Settings");
            add("applanguage", "Application Language Settings", "Settings");
            add("printsetup", "Receipt & Invoice Print Setup", "Settings");
            add("systemsettings", "System Preferences & Hardware", "Settings");

            // Utilities
            add("excelimport", "Excel Import / Export Utility", "Utilities");
            add("barcode", "Barcode Label Designer & Print", "Utilities");
            add("plu", "PLU Weighing Scale Setup", "Utilities");
            add("openingstock", "Opening Stock Entry Utility", "Utilities");
            add("counterclosing", "Counter Shift Closing Utility", "Utilities");
            add("changeitemno", "Change Item Number Utility", "Utilities");
            add("databasemaintenance", "Database Backup & Maintenance", "Utilities");
            add("nexorisai", "Nexoris AI Copilot", "Utilities");
            add("dataaudit", "Data Integrity Audit Utility", "Utilities");

            return list;
        }

        private void EnsureRolesSeededInDatabase()
        {
            if (_rolesSeeded) return;
            _rolesSeeded = true;
            EnsureRolesSeededInDatabase_Impl();
        }

        private void EnsureRolesSeededInDatabase_Impl()
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open) DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_RolePermission, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "SEEDROLES");
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnsureRolesSeededInDatabase warning: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        private void EnsureActivityLogPermissionForm() => EnsureAllFormsSeededInDatabase();

        private void EnsureAllFormsSeededInDatabase()
        {
            if (_formsSeeded) return;
            _formsSeeded = true;
            EnsureAllFormsSeededInDatabase_Impl();
        }

        private void EnsureAllFormsSeededInDatabase_Impl()
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open) DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_RolePermission, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "SEEDFORMS");
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unable to ensure permission forms: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        /// <summary>
        /// Saves a single permission
        /// </summary>
        public string SavePermission(int roleId, int formId, bool canView, bool canAdd, bool canEdit, bool canDelete)
        {
            EnsureRolesSeededInDatabase();
            EnsureAllFormsSeededInDatabase();

            if (DataConnection.State != ConnectionState.Open) DataConnection.Open();
            var trans = DataConnection.BeginTransaction();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_RolePermission, (SqlConnection)DataConnection, (SqlTransaction)trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RoleID", roleId);
                    cmd.Parameters.AddWithValue("@FormID", formId);
                    cmd.Parameters.AddWithValue("@CanView", canView);
                    cmd.Parameters.AddWithValue("@CanAdd", canAdd);
                    cmd.Parameters.AddWithValue("@CanEdit", canEdit);
                    cmd.Parameters.AddWithValue("@CanDelete", canDelete);
                    cmd.Parameters.AddWithValue("@_Operation", "SAVE");
                    cmd.ExecuteNonQuery();
                }

                trans.Commit();
                return "Success";
            }
            catch (Exception ex)
            {
                trans.Rollback();
                System.Diagnostics.Debug.WriteLine($"Error saving permission: {ex.Message}");
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        private void EnsureRolePermissionColumnsExist()
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open) DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_RolePermission, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "ENSURESCHEMA");
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnsureRolePermissionColumnsExist warning: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        /// <summary>
        /// Saves multiple permissions for a role (bulk save for admin UI)
        /// </summary>
        public string SavePermissions(int roleId, List<FormPermissionGrid> permissions, string roleName = null)
        {
            EnsureRolesSeededInDatabase();
            EnsureAllFormsSeededInDatabase();
            EnsureRolePermissionColumnsExist();

            if (permissions == null || permissions.Count == 0) return "Success";

            List<int> targetRoleIds = GetTargetRoleIds(roleId, roleName);
            try
            {
                var allRoles = GetAllRoles();
                var currentRole = allRoles.FirstOrDefault(r => r.RoleID == roleId);
                if (currentRole != null && !string.IsNullOrWhiteSpace(currentRole.RoleName))
                {
                    var matchingRoles = allRoles.Where(r => string.Equals(r.RoleName, currentRole.RoleName, StringComparison.OrdinalIgnoreCase));
                    foreach (var mr in matchingRoles)
                    {
                        if (mr.RoleID > 0 && !targetRoleIds.Contains(mr.RoleID)) targetRoleIds.Add(mr.RoleID);
                        if (mr.UserLevelID.HasValue && mr.UserLevelID.Value > 0 && !targetRoleIds.Contains(mr.UserLevelID.Value)) targetRoleIds.Add(mr.UserLevelID.Value);
                    }
                }
            }
            catch { }

            try
            {
                if (DataConnection.State != ConnectionState.Open) DataConnection.Open();
                var trans = DataConnection.BeginTransaction();

                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_RolePermission, (SqlConnection)DataConnection, (SqlTransaction)trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@RoleID", SqlDbType.Int);
                        cmd.Parameters.Add("@FormID", SqlDbType.Int);
                        cmd.Parameters.Add("@FormKey", SqlDbType.NVarChar, 100);
                        cmd.Parameters.Add("@FormName", SqlDbType.NVarChar, 200);
                        cmd.Parameters.Add("@Category", SqlDbType.NVarChar, 100);
                        cmd.Parameters.Add("@CanView", SqlDbType.Bit);
                        cmd.Parameters.Add("@CanAdd", SqlDbType.Bit);
                        cmd.Parameters.Add("@CanEdit", SqlDbType.Bit);
                        cmd.Parameters.Add("@CanDelete", SqlDbType.Bit);
                        cmd.Parameters.Add("@_Operation", SqlDbType.NVarChar, 50).Value = "SAVE";

                        foreach (var tid in targetRoleIds)
                        {
                            foreach (var perm in permissions)
                            {
                                cmd.Parameters["@RoleID"].Value = tid;
                                cmd.Parameters["@FormID"].Value = perm.FormID;
                                cmd.Parameters["@FormKey"].Value = (object)(perm.FormKey ?? "") ?? DBNull.Value;
                                cmd.Parameters["@FormName"].Value = (object)(perm.FormName ?? "") ?? DBNull.Value;
                                cmd.Parameters["@Category"].Value = (object)(perm.Category ?? "General") ?? DBNull.Value;
                                cmd.Parameters["@CanView"].Value = perm.CanView;
                                cmd.Parameters["@CanAdd"].Value = perm.CanAdd;
                                cmd.Parameters["@CanEdit"].Value = perm.CanEdit;
                                cmd.Parameters["@CanDelete"].Value = perm.CanDelete;

                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    trans.Commit();
                    return "Success";
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving permissions: {ex.Message}");
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        /// <summary>
        /// Registers a new form or module into dbo.Forms and grants Admin default permissions
        /// </summary>
        public bool RegisterNewForm(string formKey, string formName, string category)
        {
            if (string.IsNullOrWhiteSpace(formKey) && string.IsNullOrWhiteSpace(formName)) return false;

            EnsureAllFormsSeededInDatabase();

            try
            {
                if (DataConnection.State != ConnectionState.Open) DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_RolePermission, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FormKey", formKey?.Trim() ?? "");
                    cmd.Parameters.AddWithValue("@FormName", formName?.Trim() ?? "");
                    cmd.Parameters.AddWithValue("@Category", string.IsNullOrWhiteSpace(category) ? "General" : category.Trim());
                    cmd.Parameters.AddWithValue("@_Operation", "REGISTERFORM");
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RegisterNewForm error: {ex.Message}");
                return false;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        /// <summary>
        /// Registers a list of discovered forms into dbo.Forms
        /// </summary>
        public int SyncAllDiscoveredForms(List<FormPermissionGrid> forms)
        {
            if (forms == null || forms.Count == 0) return 0;
            int count = 0;
            foreach (var f in forms)
            {
                if (RegisterNewForm(f.FormKey, f.FormName, f.Category)) count++;
            }
            return count;
        }
    }
}
