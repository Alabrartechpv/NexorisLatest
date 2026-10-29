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
    /// Repository for Role-Based Access Control (RBAC) operations
    /// </summary>
    public class RolePermissionRepository : BaseRepostitory
    {
        /// <summary>
        /// Gets all permissions for a specific role
        /// </summary>
        /// <param name="roleId">Role ID to get permissions for</param>
        /// <returns>List of RolePermission objects</returns>
        /// <summary>
        /// Resolves all matching Role IDs across dbo.Roles and dbo.UserLevel for a given Role ID or Role Name
        /// </summary>
        private static bool _rolesSeeded = false;
        private static bool _formsSeeded = false;

        public List<int> GetTargetRoleIds(int roleId, string roleName = null)
        {
            var ids = new HashSet<int>();
            if (roleId > 0) ids.Add(roleId);

            try
            {
                var allRoles = GetAllRoles();
                var matched = allRoles.Where(r =>
                    (roleId > 0 && (r.RoleID == roleId || (r.UserLevelID.HasValue && r.UserLevelID.Value == roleId))) ||
                    (!string.IsNullOrWhiteSpace(roleName) && string.Equals(r.RoleName?.Trim(), roleName.Trim(), StringComparison.OrdinalIgnoreCase))
                ).ToList();

                string resolvedName = roleName;
                if (string.IsNullOrWhiteSpace(resolvedName) && matched.Count > 0)
                {
                    resolvedName = matched.FirstOrDefault(m => !string.IsNullOrWhiteSpace(m.RoleName))?.RoleName;
                }

                if (!string.IsNullOrWhiteSpace(resolvedName))
                {
                    var byName = allRoles.Where(r => string.Equals(r.RoleName?.Trim(), resolvedName.Trim(), StringComparison.OrdinalIgnoreCase));
                    foreach (var r in byName)
                    {
                        if (r.RoleID > 0) ids.Add(r.RoleID);
                        if (r.UserLevelID.HasValue && r.UserLevelID.Value > 0) ids.Add(r.UserLevelID.Value);
                    }
                }
                else
                {
                    foreach (var r in matched)
                    {
                        if (r.RoleID > 0) ids.Add(r.RoleID);
                        if (r.UserLevelID.HasValue && r.UserLevelID.Value > 0) ids.Add(r.UserLevelID.Value);
                    }
                }
            }
            catch { }

            try
            {
                if (DataConnection.State != ConnectionState.Open) DataConnection.Open();
                using (SqlCommand cmd = new SqlCommand(@"
DECLARE @Names TABLE (RoleName NVARCHAR(100));

IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL
    INSERT INTO @Names SELECT RoleName FROM dbo.Roles WHERE RoleID = @RId OR UserLevelID = @RId;

IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL AND OBJECT_ID('dbo.UserLevel', 'U') IS NOT NULL
BEGIN
    SELECT 
        u.UserLevelID AS RoleID,
        u.UserLevel AS RoleName,
        ISNULL(r.RoleID, u.UserLevelID) AS UserLevelID
    FROM dbo.UserLevel u
    LEFT JOIN dbo.Roles r ON UPPER(RTRIM(LTRIM(u.UserLevel))) = UPPER(RTRIM(LTRIM(r.RoleName)))
    WHERE ISNULL(u.UserLevel, '') <> ''
    ORDER BY u.UserLevelID;
END
ELSE IF OBJECT_ID('dbo.UserLevel', 'U') IS NOT NULL
    INSERT INTO @Names SELECT UserLevel FROM dbo.UserLevel WHERE UserLevelID = @RId;

IF ISNULL(@RName, '') <> ''
    INSERT INTO @Names VALUES (@RName);

IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL
BEGIN
    SELECT DISTINCT RoleID FROM dbo.Roles r INNER JOIN @Names n ON UPPER(RTRIM(LTRIM(r.RoleName))) = UPPER(RTRIM(LTRIM(n.RoleName)));
    SELECT DISTINCT UserLevelID FROM dbo.Roles r INNER JOIN @Names n ON UPPER(RTRIM(LTRIM(r.RoleName))) = UPPER(RTRIM(LTRIM(n.RoleName))) WHERE UserLevelID IS NOT NULL;
END

IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL AND OBJECT_ID('dbo.UserLevel', 'U') IS NOT NULL
BEGIN
    SELECT 
        u.UserLevelID AS RoleID,
        u.UserLevel AS RoleName,
        ISNULL(r.RoleID, u.UserLevelID) AS UserLevelID
    FROM dbo.UserLevel u
    LEFT JOIN dbo.Roles r ON UPPER(RTRIM(LTRIM(u.UserLevel))) = UPPER(RTRIM(LTRIM(r.RoleName)))
    WHERE ISNULL(u.UserLevel, '') <> ''
    ORDER BY u.UserLevelID;
END
ELSE IF OBJECT_ID('dbo.UserLevel', 'U') IS NOT NULL
    SELECT DISTINCT UserLevelID FROM dbo.UserLevel u INNER JOIN @Names n ON UPPER(RTRIM(LTRIM(u.UserLevel))) = UPPER(RTRIM(LTRIM(n.RoleName)));
", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@RId", roleId);
                    cmd.Parameters.AddWithValue("@RName", roleName ?? "");
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        do
                        {
                            while (reader.Read())
                            {
                                if (!reader.IsDBNull(0))
                                {
                                    int val = Convert.ToInt32(reader[0]);
                                    if (val > 0) ids.Add(val);
                                }
                            }
                        } while (reader.NextResult());
                    }
                }
            }
            catch { }

            return ids.Count > 0 ? ids.ToList() : new List<int> { roleId };
        }

        public List<RolePermission> GetPermissionsByRoleId(int roleId)
        {
            List<RolePermission> savedPermissions = new List<RolePermission>();

            // Resolve all matching role IDs (e.g., both RoleID=4 and UserLevelID=3 for Cashier)
            List<int> targetRoleIds = GetTargetRoleIds(roleId);
            try
            {
                var allRoles = GetAllRoles();
                var currentRole = allRoles.FirstOrDefault(r => r.RoleID == roleId || (r.UserLevelID.HasValue && r.UserLevelID.Value == roleId));
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

                // 1. Query saved rows from dbo.RolePermissions DB table matching any target role ID
                using (SqlCommand cmd = new SqlCommand($@"
IF OBJECT_ID('dbo.RolePermissions', 'U') IS NOT NULL
BEGIN
    DECLARE @HasFormKey BIT = CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RolePermissions') AND name = 'FormKey') THEN 1 ELSE 0 END;
    DECLARE @HasFormName BIT = CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RolePermissions') AND name = 'FormName') THEN 1 ELSE 0 END;

    IF @HasFormKey = 1 AND @HasFormName = 1
    BEGIN
        SELECT RoleID, FormID, FormKey, FormName, CanView, CanAdd, CanEdit, CanDelete 
        FROM dbo.RolePermissions 
        WHERE RoleID IN ({idListStr});
    END
    ELSE
    BEGIN
        SELECT RoleID, FormID, '' AS FormKey, '' AS FormName, CanView, CanAdd, CanEdit, CanDelete 
        FROM dbo.RolePermissions 
        WHERE RoleID IN ({idListStr});
    END
END", (SqlConnection)DataConnection))
                {
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

                // 2. If direct table query returned no rows, try stored procedure GETBYROLE
                if (savedPermissions == null || savedPermissions.Count == 0)
                {
                    foreach (int tid in targetRoleIds)
                    {
                        using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_RolePermission, (SqlConnection)DataConnection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@RoleID", tid);
                            cmd.Parameters.AddWithValue("@FormID", 0);
                            cmd.Parameters.AddWithValue("@CanView", 0);
                            cmd.Parameters.AddWithValue("@CanAdd", 0);
                            cmd.Parameters.AddWithValue("@CanEdit", 0);
                            cmd.Parameters.AddWithValue("@CanDelete", 0);
                            cmd.Parameters.AddWithValue("@_Operation", "GETBYROLE");

                            using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                            {
                                DataSet ds = new DataSet();
                                adapt.Fill(ds);
                                if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                                {
                                    savedPermissions = ds.Tables[0].ToListOfObject<RolePermission>();
                                    if (savedPermissions.Count > 0) break;
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
            string roleName = "";
            try
            {
                var roleObj = GetAllRoles().FirstOrDefault(r => r.RoleID == roleId);
                if (roleObj != null) roleName = roleObj.RoleName ?? "";
            }
            catch { }

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
                var ignoredSaved = savedPermissions?.FirstOrDefault(p => 
                    (p.FormID > 0 && p.FormID == sf.FormID) ||
                    (!string.IsNullOrEmpty(p.FormKey) && string.Equals(p.FormKey, sf.FormKey, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.FormName) && string.Equals(p.FormName, sf.FormName, StringComparison.OrdinalIgnoreCase)));

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

        private List<RolePermission> GenerateDefaultPermissionsForRole(int roleId)
        {
            var result = new List<RolePermission>();
            var allForms = GetFormsFromDatabase();

            string roleName = "";
            try
            {
                var roleObj = GetAllRoles().FirstOrDefault(r => r.RoleID == roleId);
                if (roleObj != null) roleName = roleObj.RoleName ?? "";
            }
            catch { }

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

            // Asynchronously save these initial default permissions to database so admin UI reflects them
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

                SavePermissions(roleId, gridList);
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
            DataConnection.Open();

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
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
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
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_RolePermission, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RoleID", userLevelId);  // Reuse RoleID param for UserLevelID lookup
                    cmd.Parameters.AddWithValue("@_Operation", "GETROLEBYLEVELID");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
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

            // 1. Query roles prioritizing dbo.UserLevel table matching exact database UserLevelID
            try
            {
                if (DataConnection.State != ConnectionState.Open) DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(@"
IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL AND OBJECT_ID('dbo.UserLevel', 'U') IS NOT NULL
BEGIN
    SELECT 
        u.UserLevelID AS RoleID,
        u.UserLevel AS RoleName,
        ISNULL(r.RoleID, u.UserLevelID) AS UserLevelID
    FROM dbo.UserLevel u
    LEFT JOIN dbo.Roles r ON UPPER(RTRIM(LTRIM(u.UserLevel))) = UPPER(RTRIM(LTRIM(r.RoleName)))
    WHERE ISNULL(u.UserLevel, '') <> ''
    ORDER BY u.UserLevelID;
END
ELSE IF OBJECT_ID('dbo.UserLevel', 'U') IS NOT NULL
BEGIN
    SELECT UserLevelID AS RoleID, UserLevel AS RoleName, UserLevelID 
    FROM dbo.UserLevel 
    WHERE ISNULL(UserLevel, '') <> ''
    ORDER BY UserLevelID;
END
ELSE IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL
BEGIN
    SELECT RoleID, RoleName, UserLevelID FROM dbo.Roles ORDER BY RoleID;
END", (SqlConnection)DataConnection))
                {
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
                    new Role { RoleID = 4, RoleName = "Administrator", UserLevelID = 4 },
                    new Role { RoleID = 5, RoleName = "Supervisor", UserLevelID = 5 },
                    new Role { RoleID = 6, RoleName = "Sales Man", UserLevelID = 6 },
                    new Role { RoleID = 7, RoleName = "Accountant", UserLevelID = 7 },
                    new Role { RoleID = 8, RoleName = "Inventory Manager", UserLevelID = 8 },
                    new Role { RoleID = 9, RoleName = "Standard User", UserLevelID = 9 }
                };
            }

            return roles;
        }

        /// <summary>
        /// Gets all forms with their permissions for a specific role (for admin UI)
        /// </summary>
        /// <param name="roleId">Role ID to get permissions for</param>
        /// <returns>List of FormPermissionGrid objects</returns>
        public List<FormPermissionGrid> GetFormsWithPermissions(int roleId)
        {
            var rolePerms = GetPermissionsByRoleId(roleId);
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

                using (SqlCommand cmd = new SqlCommand(@"
IF OBJECT_ID('dbo.Forms', 'U') IS NOT NULL
BEGIN
    SELECT FormID, 
           ISNULL(FormKey, '') AS FormKey, 
           ISNULL(FormName, '') AS FormName, 
           ISNULL(Category, 'General') AS Category 
    FROM dbo.Forms 
    WHERE ISNULL(IsActive, 1) = 1
    ORDER BY Category, FormName;
END", (SqlConnection)DataConnection))
                {
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

                using (SqlCommand cmd = new SqlCommand(@"
IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL
BEGIN
    CREATE TABLE #DefaultRoles (RoleName NVARCHAR(100));
    INSERT INTO #DefaultRoles (RoleName) VALUES 
    (N'Administrator'),
    (N'Manager'),
    (N'Supervisor'),
    (N'Cashier'),
    (N'Sales Man'),
    (N'Accountant'),
    (N'Inventory Manager'),
    (N'Standard User');

    IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL AND OBJECT_ID('dbo.UserLevel', 'U') IS NOT NULL
BEGIN
    SELECT 
        u.UserLevelID AS RoleID,
        u.UserLevel AS RoleName,
        ISNULL(r.RoleID, u.UserLevelID) AS UserLevelID
    FROM dbo.UserLevel u
    LEFT JOIN dbo.Roles r ON UPPER(RTRIM(LTRIM(u.UserLevel))) = UPPER(RTRIM(LTRIM(r.RoleName)))
    WHERE ISNULL(u.UserLevel, '') <> ''
    ORDER BY u.UserLevelID;
END
ELSE IF OBJECT_ID('dbo.UserLevel', 'U') IS NOT NULL
    BEGIN
        INSERT INTO #DefaultRoles (RoleName)
        SELECT DISTINCT UserLevel FROM dbo.UserLevel WHERE ISNULL(UserLevel, '') <> '';

        INSERT INTO dbo.UserLevel (UserLevel)
        SELECT d.RoleName 
        FROM #DefaultRoles d
        WHERE NOT EXISTS (
            SELECT 1 FROM dbo.UserLevel u WHERE UPPER(RTRIM(LTRIM(u.UserLevel))) = UPPER(RTRIM(LTRIM(d.RoleName)))
        );
    END;

    DECLARE @HasIsActive BIT = CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Roles') AND name = 'IsActive') THEN 1 ELSE 0 END;
    DECLARE @HasIsDelete BIT = CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Roles') AND name = 'IsDelete') THEN 1 ELSE 0 END;
    DECLARE @HasUserLevelID BIT = CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Roles') AND name = 'UserLevelID') THEN 1 ELSE 0 END;
    DECLARE @Sql NVARCHAR(MAX);

    SET @Sql = N'
    INSERT INTO dbo.Roles (RoleName' + 
        CASE WHEN @HasIsActive = 1 THEN N', IsActive' ELSE N'' END + 
        CASE WHEN @HasIsDelete = 1 THEN N', IsDelete' ELSE N'' END + N')
    SELECT DISTINCT d.RoleName' + 
        CASE WHEN @HasIsActive = 1 THEN N', 1' ELSE N'' END + 
        CASE WHEN @HasIsDelete = 1 THEN N', 0' ELSE N'' END + N'
    FROM #DefaultRoles d
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.Roles r WHERE UPPER(RTRIM(LTRIM(r.RoleName))) = UPPER(RTRIM(LTRIM(d.RoleName)))
    );';

    EXEC sp_executesql @Sql;

    IF @HasUserLevelID = 1 AND OBJECT_ID('dbo.UserLevel', 'U') IS NOT NULL
    BEGIN
        EXEC sp_executesql N'
        UPDATE r
        SET r.UserLevelID = u.UserLevelID
        FROM dbo.Roles r
        INNER JOIN dbo.UserLevel u ON UPPER(RTRIM(LTRIM(r.RoleName))) = UPPER(RTRIM(LTRIM(u.UserLevel)));';
    END;
    DROP TABLE #DefaultRoles;
END", (SqlConnection)DataConnection))
                {
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
                using (SqlCommand cmd = new SqlCommand(@"
DECLARE @SchemaName SYSNAME;
DECLARE @TableName SYSNAME;
DECLARE @FullName NVARCHAR(300);
DECLARE @Sql NVARCHAR(MAX);
DECLARE @ObjectId INT;
DECLARE @HasCategory BIT;
DECLARE @HasIsActive BIT;

SELECT TOP (1)
    @SchemaName = s.name,
    @TableName = t.name
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE EXISTS (
        SELECT 1
        FROM sys.columns c
        WHERE c.object_id = t.object_id
          AND c.name = 'FormKey'
    )
  AND EXISTS (
        SELECT 1
        FROM sys.columns c
        WHERE c.object_id = t.object_id
          AND c.name = 'FormName'
    )
ORDER BY CASE
    WHEN t.name IN ('Forms', 'FormMaster', 'RoleForms', 'POS_Forms', 'ApplicationForms') THEN 0
    ELSE 1
END, t.name;

IF @TableName IS NOT NULL
BEGIN
    SET @FullName = QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName);
    SET @ObjectId = OBJECT_ID(@FullName);

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = @ObjectId AND name = 'FormKey')
        EXEC(N'ALTER TABLE ' + @FullName + N' ADD FormKey NVARCHAR(100) NULL');
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = @ObjectId AND name = 'Category')
        EXEC(N'ALTER TABLE ' + @FullName + N' ADD Category NVARCHAR(100) NULL');
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = @ObjectId AND name = 'IsActive')
        EXEC(N'ALTER TABLE ' + @FullName + N' ADD IsActive BIT NULL DEFAULT 1');

    SET @HasCategory = 1;
    SET @HasIsActive = 1;

    CREATE TABLE #FormsToEnsure (
        FormKey NVARCHAR(100),
        FormName NVARCHAR(200),
        Category NVARCHAR(100)
    );

    INSERT INTO #FormsToEnsure (FormKey, FormName, Category) VALUES
    -- Master
    ('company', 'Company Setup', 'Master'),
    ('branch', 'Branch Setup', 'Master'),
    ('state', 'State Master', 'Master'),
    ('country', 'Country Master', 'Master'),
    ('currency', 'Currency Master', 'Master'),
    ('group', 'Group Master', 'Master'),
    ('category', 'Category Master', 'Master'),
    ('brand', 'Brand Master', 'Master'),
    ('ItemMaster', 'Item Master', 'Master'),
    ('itemtype', 'Item Type Master', 'Master'),
    ('unit', 'Unit Master', 'Master'),
    ('line', 'Line Master', 'Master'),
    ('rack', 'Rack Master', 'Master'),
    ('row', 'Row Master', 'Master'),
    ('reason', 'Reason Master', 'Master'),
    ('users', 'Users & Security Setup', 'Master'),
    ('paymode', 'General Paymode Setup', 'Master'),
    ('TaxManagement', 'Tax Management & Rates', 'Master'),
    ('counter', 'Counter Master', 'Master'),
    ('pricesetting', 'Price Level Settings', 'Master'),

    -- Transaction
    ('pos', 'POS Terminal', 'Transaction'),
    ('sales', 'Sales Invoice', 'Transaction'),
    ('salesreturn', 'Sales Return', 'Transaction'),
    ('purchase', 'Purchase Invoice', 'Transaction'),
    ('purchaseorder', 'Purchase Order', 'Transaction'),
    ('purchasereturn', 'Purchase Return', 'Transaction'),
    ('stockadjustment', 'Stock Adjustment', 'Transaction'),
    ('stocktransfer', 'Stock Transfer', 'Transaction'),
    ('grn', 'Goods Received Notes (GRN)', 'Transaction'),
    ('quotation', 'Sales Quotation / Estimate', 'Transaction'),
    ('deliverychallan', 'Delivery Challan', 'Transaction'),

    -- Accounts
    ('customer', 'Customer Management', 'Accounts'),
    ('vendor', 'Vendor Management', 'Accounts'),
    ('ledgers', 'Account Ledgers', 'Accounts'),
    ('accountgroup', 'Account Group Setup', 'Accounts'),
    ('chartofaccount', 'Chart of Accounts', 'Accounts'),
    ('receipt', 'Customer Receipt', 'Accounts'),
    ('payment', 'Vendor Payment', 'Accounts'),
    ('generalpayment', 'General Payment Voucher', 'Accounts'),
    ('generalreceipt', 'General Receipt Voucher', 'Accounts'),
    ('contra', 'Contra Voucher', 'Accounts'),
    ('journal', 'Journal Voucher', 'Accounts'),
    ('debitnote', 'Debit Note Voucher', 'Accounts'),
    ('creditnote', 'Credit Note Voucher', 'Accounts'),
    ('bankreconciliation', 'Bank Reconciliation', 'Accounts'),
    ('manualpartybalance', 'Manual Party Balance Entry', 'Accounts'),
    ('expenseentry', 'Expense Voucher Entry', 'Accounts'),

    -- Reports & Analytics
    ('dashboard', 'Executive Dashboard & KPIs', 'Reports'),
    ('salesanalytics', 'Sales Analytics Dashboard', 'Reports'),
    ('purchaseanalytics', 'Purchase Analytics Dashboard', 'Reports'),
    ('stockanalytics', 'Stock Analytics Dashboard', 'Reports'),
    ('smartreorder', 'Smart Reorder Dashboard', 'Reports'),
    ('itemreport', 'Item Report & Details', 'Reports'),
    ('stockreport', 'Stock Report', 'Reports'),
    ('salesreport', 'Sales Summary Report', 'Reports'),
    ('purchasereport', 'Purchase Summary Report', 'Reports'),
    ('vendorpurchasereport', 'Vendor Purchase Report', 'Reports'),
    ('tradingpl', 'Trading & Profit / Loss Account', 'Reports'),
    ('balancesheet', 'Balance Sheet', 'Reports'),
    ('trialbalance', 'Trial Balance', 'Reports'),
    ('cashbankbook', 'Cash & Bank Book', 'Reports'),
    ('daybook', 'Day Book', 'Reports'),
    ('bankstatement', 'Bank Statement Report', 'Reports'),
    ('shiftreconciliation', 'Shift Reconciliation Report', 'Reports'),
    ('vendoroutstanding', 'Vendor Outstanding Report', 'Reports'),
    ('customerledger', 'Customer Ledger Report', 'Reports'),
    ('customeroutstanding', 'Customer Outstanding Report', 'Reports'),
    ('customerreceiptreport', 'Customer Receipt Report', 'Reports'),
    ('salesprofit', 'Sales Profit Report', 'Reports'),
    ('counterreport', 'Counter Sales Report', 'Reports'),
    ('salesmanincentive', 'Salesman Incentive Report', 'Reports'),
    ('customerwisesalessummary', 'Customerwise Sales Summary Report', 'Reports'),
    ('salesmanwisesalessummary', 'Salesmanwise Sales Summary Report', 'Reports'),
    ('itemwisesalessummary', 'Itemwise Sales Summary Report', 'Reports'),
    ('stockvaluation', 'Stock Valuation Report', 'Reports'),
    ('lowstockalert', 'Low Stock Alert Report', 'Reports'),
    ('stockadjustmentreport', 'Stock Adjustment Report', 'Reports'),
    ('inactiveitemsreport', 'Inactive Items Report', 'Reports'),
    ('manualpartybalancereport', 'Manual Party Balance Report', 'Reports'),
    ('combinedpartybalancereport', 'Combined Party Balance Report', 'Reports'),
    ('stockreportadvanced', 'Advanced Stock Report', 'Reports'),
    ('stockaudit', 'Stock Audit Report', 'Reports'),
    ('salesdetails', 'Sales Details Report', 'Reports'),
    ('salesreturnreport', 'Sales Return Report', 'Reports'),
    ('salesreturndetails', 'Sales Return Details Report', 'Reports'),
    ('purchasedetails', 'Purchase Details Report', 'Reports'),
    ('purchasereturnreport', 'Purchase Return Report', 'Reports'),
    ('purchasereturndetails', 'Purchase Return Details Report', 'Reports'),
    ('grnreport', 'Goods Received (GRN) Report', 'Reports'),
    ('cashinout', 'Cash In / Out Report', 'Reports'),
    ('profitloss', 'Profit and Loss Account', 'Reports'),
    ('businessexpense', 'Business Expense Report', 'Reports'),
    ('inputgst', 'Input GST Report', 'Reports'),
    ('outputgst', 'Output GST Report', 'Reports'),
    ('govtgst', 'Government GST Return Report', 'Reports'),

    -- Settings & Utilities
    ('possettings', 'Sale Settings & Preferences', 'Settings'),
    ('excelimport', 'Excel Import / Export Utility', 'Utilities'),
    ('rolepermissions', 'Role Permissions Management', 'Settings'),
    ('ActivityLog', 'Activity Log (Master)', 'Settings'),
    ('itemhistory', 'Item History Log', 'Settings'),
    ('transactionactivity', 'Transaction Activity Log', 'Settings'),
    ('useractivity', 'User Activity Log', 'Settings'),
    ('itemstockactivity', 'Stock Activity Log', 'Settings'),
    ('yearclosing', 'Financial Year Closing', 'Settings'),
    ('applanguage', 'Application Language Settings', 'Settings'),
    ('nexorisai', 'Nexoris AI Copilot', 'Utilities'),
    ('barcode', 'Barcode Label Designer & Print', 'Utilities'),
    ('plu', 'PLU Weighing Scale Setup', 'Utilities'),
    ('openingstock', 'Opening Stock Entry Utility', 'Utilities'),
    ('counterclosing', 'Counter Shift Closing Utility', 'Utilities'),
    ('changeitemno', 'Change Item Number Utility', 'Utilities'),
    ('databasemaintenance', 'Database Backup & Maintenance', 'Utilities'),
    ('printsetup', 'Receipt & Invoice Print Setup', 'Settings'),
    ('systemsettings', 'System Preferences & Hardware', 'Settings'),
    ('dataaudit', 'Data Integrity Audit Utility', 'Utilities');

    DECLARE @IsIdent BIT = COLUMNPROPERTY(@ObjectId, 'FormID', 'IsIdentity');

    SET @Sql = N'
    DECLARE @MaxID INT = ISNULL((SELECT MAX(FormID) FROM ' + @FullName + N'), 0);
    INSERT INTO ' + @FullName + N' (' + CASE WHEN @IsIdent = 0 THEN N'FormID, ' ELSE N'' END + N'FormKey, FormName, Category, IsActive)
    SELECT ' + CASE WHEN @IsIdent = 0 THEN N'@MaxID + ROW_NUMBER() OVER (ORDER BY f.Category, f.FormName), ' ELSE N'' END + N'f.FormKey, f.FormName, f.Category, 1
    FROM #FormsToEnsure f
    WHERE NOT EXISTS (
        SELECT 1 FROM ' + @FullName + N' t 
        WHERE (ISNULL(t.FormKey, '''') <> '''' AND UPPER(RTRIM(LTRIM(t.FormKey))) = UPPER(RTRIM(LTRIM(f.FormKey))))
           OR (ISNULL(t.FormName, '''') <> '''' AND UPPER(RTRIM(LTRIM(t.FormName))) = UPPER(RTRIM(LTRIM(f.FormName))))
    );';

    EXEC sp_executesql @Sql;

    SET @Sql = N'
    UPDATE t
    SET t.Category = CASE WHEN ISNULL(t.Category, '''') = '''' THEN f.Category ELSE t.Category END,
        t.FormKey = CASE WHEN ISNULL(t.FormKey, '''') = '''' THEN f.FormKey ELSE t.FormKey END,
        t.IsActive = ISNULL(t.IsActive, 1)
    FROM ' + @FullName + N' t
    INNER JOIN #FormsToEnsure f ON UPPER(RTRIM(LTRIM(t.FormName))) = UPPER(RTRIM(LTRIM(f.FormName)))
                                OR UPPER(RTRIM(LTRIM(t.FormKey))) = UPPER(RTRIM(LTRIM(f.FormKey)));';

    EXEC sp_executesql @Sql;

    DROP TABLE #FormsToEnsure;
END", (SqlConnection)DataConnection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unable to ensure permission forms: {ex.Message}");
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

                using (SqlCommand cmd = new SqlCommand(@"
IF OBJECT_ID('dbo.RolePermissions', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RolePermissions') AND name = 'FormKey')
    BEGIN
        ALTER TABLE dbo.RolePermissions ADD FormKey NVARCHAR(100) NULL;
    END
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RolePermissions') AND name = 'FormName')
    BEGIN
        ALTER TABLE dbo.RolePermissions ADD FormName NVARCHAR(200) NULL;
    END
END", (SqlConnection)DataConnection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnsureRolePermissionColumnsExist warning: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves multiple permissions for a role (bulk save for admin UI)
        /// </summary>
        public string SavePermissions(int roleId, List<FormPermissionGrid> permissions)
        {
            EnsureRolesSeededInDatabase();
            EnsureAllFormsSeededInDatabase();
            EnsureRolePermissionColumnsExist();

            if (permissions == null || permissions.Count == 0) return "Success";

            List<int> targetRoleIds = GetTargetRoleIds(roleId);
            try
            {
                var allRoles = GetAllRoles();
                var currentRole = allRoles.FirstOrDefault(r => r.RoleID == roleId || (r.UserLevelID.HasValue && r.UserLevelID.Value == roleId));
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

                using (SqlCommand cmd = new SqlCommand(@"
DECLARE @RealFormID INT = @FormID;

IF OBJECT_ID('dbo.Forms', 'U') IS NOT NULL
BEGIN
    IF @RealFormID <= 0 OR NOT EXISTS (SELECT 1 FROM dbo.Forms WHERE FormID = @RealFormID)
    BEGIN
        SELECT TOP 1 @RealFormID = FormID 
        FROM dbo.Forms 
        WHERE (ISNULL(@FormKey, '') <> '' AND UPPER(RTRIM(LTRIM(FormKey))) = UPPER(RTRIM(LTRIM(@FormKey))))
           OR (ISNULL(@FormName, '') <> '' AND UPPER(RTRIM(LTRIM(FormName))) = UPPER(RTRIM(LTRIM(@FormName))));

        IF @RealFormID IS NULL OR @RealFormID <= 0 OR NOT EXISTS (SELECT 1 FROM dbo.Forms WHERE FormID = @RealFormID)
        BEGIN
            DECLARE @IsIdent BIT = COLUMNPROPERTY(OBJECT_ID('dbo.Forms'), 'FormID', 'IsIdentity');
            IF @IsIdent = 1
            BEGIN
                INSERT INTO dbo.Forms (FormKey, FormName, Category, IsActive)
                VALUES (@FormKey, @FormName, @Category, 1);
                SET @RealFormID = SCOPE_IDENTITY();
            END
            ELSE
            BEGIN
                SELECT @RealFormID = ISNULL(MAX(FormID), 0) + 1 FROM dbo.Forms;
                INSERT INTO dbo.Forms (FormID, FormKey, FormName, Category, IsActive)
                VALUES (@RealFormID, @FormKey, @FormName, @Category, 1);
            END
        END
    END
END

IF OBJECT_ID('dbo.RolePermissions', 'U') IS NOT NULL
BEGIN
    DECLARE @HasFK BIT = CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RolePermissions') AND name = 'FormKey') THEN 1 ELSE 0 END;
    DECLARE @HasFN BIT = CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RolePermissions') AND name = 'FormName') THEN 1 ELSE 0 END;

    IF @HasFK = 1 AND @HasFN = 1
    BEGIN
        IF EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleID = @RoleID AND FormID = @RealFormID)
        BEGIN
            UPDATE dbo.RolePermissions
            SET CanView = @CanView, CanAdd = @CanAdd, CanEdit = @CanEdit, CanDelete = @CanDelete,
                FormKey = @FormKey, FormName = @FormName
            WHERE RoleID = @RoleID AND FormID = @RealFormID;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.RolePermissions (RoleID, FormID, FormKey, FormName, CanView, CanAdd, CanEdit, CanDelete)
            VALUES (@RoleID, @RealFormID, @FormKey, @FormName, @CanView, @CanAdd, @CanEdit, @CanDelete);
        END
    END
    ELSE
    BEGIN
        IF EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleID = @RoleID AND FormID = @RealFormID)
        BEGIN
            UPDATE dbo.RolePermissions
            SET CanView = @CanView, CanAdd = @CanAdd, CanEdit = @CanEdit, CanDelete = @CanDelete
            WHERE RoleID = @RoleID AND FormID = @RealFormID;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.RolePermissions (RoleID, FormID, CanView, CanAdd, CanEdit, CanDelete)
            VALUES (@RoleID, @RealFormID, @CanView, @CanAdd, @CanEdit, @CanDelete);
        END
    END
END
ELSE IF OBJECT_ID('dbo.POS_RolePermissions', 'U') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.POS_RolePermissions WHERE RoleID = @RoleID AND FormID = @RealFormID)
    BEGIN
        UPDATE dbo.POS_RolePermissions
        SET CanView = @CanView, CanAdd = @CanAdd, CanEdit = @CanEdit, CanDelete = @CanDelete
        WHERE RoleID = @RoleID AND FormID = @RealFormID;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.POS_RolePermissions (RoleID, FormID, CanView, CanAdd, CanEdit, CanDelete)
        VALUES (@RoleID, @RealFormID, @CanView, @CanAdd, @CanEdit, @CanDelete);
    END
END", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.Add("@RoleID", SqlDbType.Int);
                    cmd.Parameters.Add("@FormID", SqlDbType.Int);
                    cmd.Parameters.Add("@FormKey", SqlDbType.NVarChar, 100);
                    cmd.Parameters.Add("@FormName", SqlDbType.NVarChar, 200);
                    cmd.Parameters.Add("@Category", SqlDbType.NVarChar, 100);
                    cmd.Parameters.Add("@CanView", SqlDbType.Bit);
                    cmd.Parameters.Add("@CanAdd", SqlDbType.Bit);
                    cmd.Parameters.Add("@CanEdit", SqlDbType.Bit);
                    cmd.Parameters.Add("@CanDelete", SqlDbType.Bit);

                    foreach (var tid in targetRoleIds)
                    {
                        foreach (var perm in permissions)
                        {
                            cmd.Parameters["@RoleID"].Value = tid;
                            cmd.Parameters["@FormID"].Value = perm.FormID;
                            cmd.Parameters["@FormKey"].Value = perm.FormKey ?? "";
                            cmd.Parameters["@FormName"].Value = perm.FormName ?? "";
                            cmd.Parameters["@Category"].Value = perm.Category ?? "General";
                            cmd.Parameters["@CanView"].Value = perm.CanView;
                            cmd.Parameters["@CanAdd"].Value = perm.CanAdd;
                            cmd.Parameters["@CanEdit"].Value = perm.CanEdit;
                            cmd.Parameters["@CanDelete"].Value = perm.CanDelete;

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                return "Success";
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

                using (SqlCommand cmd = new SqlCommand(@"
IF OBJECT_ID('dbo.Forms', 'U') IS NOT NULL
BEGIN
    DECLARE @RealFormID INT = 0;
    SELECT TOP 1 @RealFormID = FormID 
    FROM dbo.Forms 
    WHERE (ISNULL(@FormKey, '') <> '' AND UPPER(RTRIM(LTRIM(FormKey))) = UPPER(RTRIM(LTRIM(@FormKey))))
       OR (ISNULL(@FormName, '') <> '' AND UPPER(RTRIM(LTRIM(FormName))) = UPPER(RTRIM(LTRIM(@FormName))));

    IF @RealFormID IS NULL OR @RealFormID <= 0
    BEGIN
        DECLARE @IsIdent BIT = COLUMNPROPERTY(OBJECT_ID('dbo.Forms'), 'FormID', 'IsIdentity');
        IF @IsIdent = 1
        BEGIN
            INSERT INTO dbo.Forms (FormKey, FormName, Category, IsActive)
            VALUES (@FormKey, @FormName, @Category, 1);
            SET @RealFormID = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            SELECT @RealFormID = ISNULL(MAX(FormID), 0) + 1 FROM dbo.Forms;
            INSERT INTO dbo.Forms (FormID, FormKey, FormName, Category, IsActive)
            VALUES (@RealFormID, @FormKey, @FormName, @Category, 1);
        END
    END
    ELSE
    BEGIN
        UPDATE dbo.Forms
        SET FormKey = CASE WHEN ISNULL(FormKey, '') = '' THEN @FormKey ELSE FormKey END,
            FormName = CASE WHEN ISNULL(FormName, '') = '' THEN @FormName ELSE FormName END,
            Category = CASE WHEN ISNULL(Category, '') = '' THEN @Category ELSE Category END,
            IsActive = 1
        WHERE FormID = @RealFormID;
    END

    -- Ensure Admin role gets view/add/edit/delete permissions automatically
    IF OBJECT_ID('dbo.RolePermissions', 'U') IS NOT NULL
    BEGIN
        DECLARE @AdminRoleID INT = 0;
        SELECT TOP 1 @AdminRoleID = RoleID FROM dbo.Roles WHERE UPPER(RTRIM(LTRIM(RoleName))) IN ('ADMIN', 'ADMINISTRATOR');
        IF @AdminRoleID <= 0 AND OBJECT_ID('dbo.UserLevel', 'U') IS NOT NULL
            SELECT TOP 1 @AdminRoleID = UserLevelID FROM dbo.UserLevel WHERE UPPER(RTRIM(LTRIM(UserLevel))) IN ('ADMIN', 'ADMINISTRATOR');

        IF @AdminRoleID > 0
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleID = @AdminRoleID AND FormID = @RealFormID)
            BEGIN
                INSERT INTO dbo.RolePermissions (RoleID, FormID, CanView, CanAdd, CanEdit, CanDelete)
                VALUES (@AdminRoleID, @RealFormID, 1, 1, 1, 1);
            END
        END
    END
END", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@FormKey", formKey?.Trim() ?? "");
                    cmd.Parameters.AddWithValue("@FormName", formName?.Trim() ?? "");
                    cmd.Parameters.AddWithValue("@Category", string.IsNullOrWhiteSpace(category) ? "General" : category.Trim());
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
