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
        public List<RolePermission> GetPermissionsByRoleId(int roleId)
        {
            List<RolePermission> savedPermissions = new List<RolePermission>();

            // Resolve all matching role IDs (e.g., both RoleID=4 and UserLevelID=3 for Cashier)
            List<int> targetRoleIds = new List<int> { roleId };
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
                EnsureActivityLogPermissionForm();

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

            // Merge saved permissions with complete 88 static form definitions
            var resultPermissions = new List<RolePermission>();
            var staticForms = GetStaticFormPermissionsList();

            foreach (var sf in staticForms)
            {
                var saved = savedPermissions?.FirstOrDefault(p => p.FormID == sf.FormID ||
                    (!string.IsNullOrEmpty(p.FormKey) && string.Equals(p.FormKey, sf.FormKey, StringComparison.OrdinalIgnoreCase)));

                if (saved != null)
                {
                    resultPermissions.Add(new RolePermission
                    {
                        RoleID = roleId,
                        FormID = sf.FormID,
                        FormKey = sf.FormKey,
                        FormName = sf.FormName,
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
            var allForms = GetStaticFormPermissionsList();

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
IF OBJECT_ID('dbo.UserLevel', 'U') IS NOT NULL
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
            var staticList = GetStaticFormPermissionsList();
            var resultGrid = new List<FormPermissionGrid>();

            foreach (var p in rolePerms)
            {
                var sf = staticList.FirstOrDefault(s => s.FormID == p.FormID ||
                    (!string.IsNullOrEmpty(s.FormKey) && string.Equals(s.FormKey, p.FormKey, StringComparison.OrdinalIgnoreCase)));

                resultGrid.Add(new FormPermissionGrid
                {
                    FormID = p.FormID,
                    FormKey = p.FormKey,
                    FormName = p.FormName,
                    Category = sf != null && !string.IsNullOrWhiteSpace(sf.Category) ? sf.Category : "General",
                    CanView = p.CanView,
                    CanAdd = p.CanAdd,
                    CanEdit = p.CanEdit,
                    CanDelete = p.CanDelete
                });
            }

            return resultGrid;
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
            add("company", "Company", "Master");
            add("branch", "Branch", "Master");
            add("state", "State", "Master");
            add("country", "Country", "Master");
            add("currency", "Currency", "Master");
            add("group", "Group", "Master");
            add("category", "Category", "Master");
            add("brand", "Brand", "Master");
            add("ItemMaster", "Item Master", "Master");
            add("itemtype", "Item Type", "Master");
            add("unit", "Unit Master", "Master");
            add("line", "Line", "Master");
            add("rack", "Rack", "Master");
            add("row", "Row", "Master");
            add("reason", "Reason Master", "Master");
            add("users", "Users & Accounts", "Master");
            add("paymode", "General Paymode Setup", "Master");
            add("TaxManagement", "Tax Management", "Master");

            // Transaction
            add("pos", "POS Terminal", "Transaction");
            add("sales", "Sales Invoice", "Transaction");
            add("salesreturn", "Sales Return", "Transaction");
            add("purchase", "Purchase Invoice", "Transaction");
            add("purchaseorder", "Purchase Order", "Transaction");
            add("purchasereturn", "Purchase Return", "Transaction");
            add("stockadjustment", "Stock Adjustment", "Transaction");
            add("stocktransfer", "Stock Transfer", "Transaction");
            add("grn", "Good Received Notes (GRN)", "Transaction");

            // Accounts
            add("customer", "Customer Management", "Accounts");
            add("vendor", "Vendor Management", "Accounts");
            add("ledgers", "Account Ledgers", "Accounts");
            add("accountgroup", "Account Group", "Accounts");
            add("chartofaccount", "Chart of Accounts", "Accounts");
            add("receipt", "Customer Receipt", "Accounts");
            add("payment", "Vendor Payment", "Accounts");
            add("generalpayment", "General Payment Voucher", "Accounts");
            add("generalreceipt", "General Receipt Voucher", "Accounts");
            add("contra", "Contra Voucher", "Accounts");
            add("journal", "Journal Voucher", "Accounts");
            add("debitnote", "Debit Note", "Accounts");
            add("creditnote", "Credit Note", "Accounts");
            add("bankreconciliation", "Bank Reconciliation", "Accounts");
            add("manualpartybalance", "Manual Party Balance", "Accounts");

            // Reports & Analytics
            add("dashboard", "Dashboard Overview", "Reports");
            add("salesanalytics", "Sales Analytics", "Reports");
            add("purchaseanalytics", "Purchase Analytics", "Reports");
            add("stockanalytics", "Stock Analytics", "Reports");
            add("smartreorder", "Smart Reorder Dashboard", "Reports");
            add("itemreport", "Item Report", "Reports");
            add("stockreport", "Stock Report & Advanced Stock", "Reports");
            add("salesreport", "Sales Report & Details", "Reports");
            add("purchasereport", "Purchase Report & Details", "Reports");
            add("vendorpurchasereport", "Vendor Purchase Report", "Reports");
            add("tradingpl", "Trading & Profit/Loss Account", "Reports");
            add("balancesheet", "Balance Sheet", "Reports");
            add("trialbalance", "Trial Balance", "Reports");
            add("cashbankbook", "Cash & Bank Book", "Reports");
            add("daybook", "Day Book", "Reports");
            add("bankstatement", "Bank Statement Report", "Reports");
            add("shiftreconciliation", "Shift Reconciliation Report", "Reports");
            add("vendoroutstanding", "Vendor Outstanding Report", "Reports");
            add("customerledger", "Customer Ledger Report", "Reports");
            add("customeroutstanding", "Customer Outstanding Report", "Reports");
            add("customerreceiptreport", "Customer Receipt Report", "Reports");
            add("salesprofit", "Sales Profit Report", "Reports");
            add("counterreport", "Counter Report", "Reports");
            add("salesmanincentive", "Salesman Incentive Report", "Reports");
            add("customerwisesalessummary", "Customerwise Sales Summary", "Reports");
            add("salesmanwisesalessummary", "Salesmanwise Sales Summary", "Reports");
            add("itemwisesalessummary", "Itemwise Sales Summary", "Reports");
            add("stockvaluation", "Stock Valuation Report", "Reports");
            add("lowstockalert", "Low Stock Alert Report", "Reports");
            add("stockadjustmentreport", "Stock Adjustment Report", "Reports");

            // Settings & Utilities
            add("possettings", "Sale Settings", "Settings");
            add("excelimport", "Excel Import/Export", "Utilities");
            add("rolepermissions", "Role Permission Management", "Settings");
            add("ActivityLog", "Activity Log (Master)", "Settings");
            add("itemhistory", "Item History Log", "Settings");
            add("transactionactivity", "Transaction Activity Log", "Settings");
            add("useractivity", "User Activity Log", "Settings");
            add("itemstockactivity", "Stock Activity Log", "Settings");
            add("yearclosing", "Financial Year Closing", "Settings");
            add("applanguage", "App Language Settings", "Settings");
            add("nexorisai", "Nexoris AI Assistant", "Utilities");
            add("barcode", "Print Barcode Utility", "Utilities");
            add("plu", "PLU Weighing Setup", "Utilities");
            add("openingstock", "Opening Stock Setup", "Utilities");
            add("counterclosing", "Counter Closing Utility", "Utilities");
            add("databasemaintenance", "Database Maintenance Utility", "Utilities");

            return list;
        }

        private void EnsureRolesSeededInDatabase()
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

    IF OBJECT_ID('dbo.UserLevel', 'U') IS NOT NULL
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

        private void EnsureActivityLogPermissionForm()
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
    SET @HasCategory = CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = @ObjectId AND name = 'Category') THEN 1 ELSE 0 END;
    SET @HasIsActive = CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = @ObjectId AND name = 'IsActive') THEN 1 ELSE 0 END;

    CREATE TABLE #FormsToEnsure (
        FormKey NVARCHAR(100),
        FormName NVARCHAR(200),
        Category NVARCHAR(100)
    );

    INSERT INTO #FormsToEnsure (FormKey, FormName, Category) VALUES
    -- Master
    ('company', 'Company', 'Master'),
    ('branch', 'Branch', 'Master'),
    ('state', 'State', 'Master'),
    ('country', 'Country', 'Master'),
    ('currency', 'Currency', 'Master'),
    ('group', 'Group', 'Master'),
    ('category', 'Category', 'Master'),
    ('brand', 'Brand', 'Master'),
    ('ItemMaster', 'Item Master', 'Master'),
    ('itemtype', 'Item Type', 'Master'),
    ('unit', 'Unit Master', 'Master'),
    ('line', 'Line', 'Master'),
    ('rack', 'Rack', 'Master'),
    ('row', 'Row', 'Master'),
    ('reason', 'Reason Master', 'Master'),
    ('users', 'Users & Accounts', 'Master'),
    ('paymode', 'General Paymode Setup', 'Master'),
    ('TaxManagement', 'Tax Management', 'Master'),

    -- Transaction
    ('pos', 'POS Terminal', 'Transaction'),
    ('sales', 'Sales Invoice', 'Transaction'),
    ('salesreturn', 'Sales Return', 'Transaction'),
    ('purchase', 'Purchase Invoice', 'Transaction'),
    ('purchaseorder', 'Purchase Order', 'Transaction'),
    ('purchasereturn', 'Purchase Return', 'Transaction'),
    ('stockadjustment', 'Stock Adjustment', 'Transaction'),
    ('stocktransfer', 'Stock Transfer', 'Transaction'),
    ('grn', 'Good Received Notes (GRN)', 'Transaction'),

    -- Accounts
    ('customer', 'Customer Management', 'Accounts'),
    ('vendor', 'Vendor Management', 'Accounts'),
    ('ledgers', 'Account Ledgers', 'Accounts'),
    ('accountgroup', 'Account Group', 'Accounts'),
    ('chartofaccount', 'Chart of Accounts', 'Accounts'),
    ('receipt', 'Customer Receipt', 'Accounts'),
    ('payment', 'Vendor Payment', 'Accounts'),
    ('generalpayment', 'General Payment Voucher', 'Accounts'),
    ('generalreceipt', 'General Receipt Voucher', 'Accounts'),
    ('contra', 'Contra Voucher', 'Accounts'),
    ('journal', 'Journal Voucher', 'Accounts'),
    ('debitnote', 'Debit Note', 'Accounts'),
    ('creditnote', 'Credit Note', 'Accounts'),
    ('bankreconciliation', 'Bank Reconciliation', 'Accounts'),
    ('manualpartybalance', 'Manual Party Balance', 'Accounts'),

    -- Reports & Analytics
    ('dashboard', 'Dashboard Overview', 'Reports'),
    ('salesanalytics', 'Sales Analytics', 'Reports'),
    ('purchaseanalytics', 'Purchase Analytics', 'Reports'),
    ('stockanalytics', 'Stock Analytics', 'Reports'),
    ('smartreorder', 'Smart Reorder Dashboard', 'Reports'),
    ('itemreport', 'Item Report', 'Reports'),
    ('stockreport', 'Stock Report & Advanced Stock', 'Reports'),
    ('salesreport', 'Sales Report & Details', 'Reports'),
    ('purchasereport', 'Purchase Report & Details', 'Reports'),
    ('vendorpurchasereport', 'Vendor Purchase Report', 'Reports'),
    ('tradingpl', 'Trading & Profit/Loss Account', 'Reports'),
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
    ('counterreport', 'Counter Report', 'Reports'),
    ('salesmanincentive', 'Salesman Incentive Report', 'Reports'),
    ('customerwisesalessummary', 'Customerwise Sales Summary', 'Reports'),
    ('salesmanwisesalessummary', 'Salesmanwise Sales Summary', 'Reports'),
    ('itemwisesalessummary', 'Itemwise Sales Summary', 'Reports'),
    ('stockvaluation', 'Stock Valuation Report', 'Reports'),
    ('lowstockalert', 'Low Stock Alert Report', 'Reports'),
    ('stockadjustmentreport', 'Stock Adjustment Report', 'Reports'),

    -- Settings & Utilities
    ('possettings', 'Sale Settings', 'Settings'),
    ('excelimport', 'Excel Import/Export', 'Utilities'),
    ('rolepermissions', 'Role Permission Management', 'Settings'),
    ('ActivityLog', 'Activity Log (Master)', 'Settings'),
    ('itemhistory', 'Item History Log', 'Settings'),
    ('transactionactivity', 'Transaction Activity Log', 'Settings'),
    ('useractivity', 'User Activity Log', 'Settings'),
    ('itemstockactivity', 'Stock Activity Log', 'Settings'),
    ('yearclosing', 'Financial Year Closing', 'Settings'),
    ('applanguage', 'App Language Settings', 'Settings'),
    ('nexorisai', 'Nexoris AI Assistant', 'Utilities'),
    ('barcode', 'Print Barcode Utility', 'Utilities'),
    ('plu', 'PLU Weighing Setup', 'Utilities'),
    ('openingstock', 'Opening Stock Setup', 'Utilities'),
    ('counterclosing', 'Counter Closing Utility', 'Utilities'),
    ('databasemaintenance', 'Database Maintenance Utility', 'Utilities');

    SET @Sql = N'
    INSERT INTO ' + @FullName + N' (FormKey, FormName' + 
        CASE WHEN @HasCategory = 1 THEN N', Category' ELSE N'' END + 
        CASE WHEN @HasIsActive = 1 THEN N', IsActive' ELSE N'' END + N')
    SELECT f.FormKey, f.FormName' + 
        CASE WHEN @HasCategory = 1 THEN N', f.Category' ELSE N'' END + 
        CASE WHEN @HasIsActive = 1 THEN N', 1' ELSE N'' END + N'
    FROM #FormsToEnsure f
    WHERE NOT EXISTS (
        SELECT 1 FROM ' + @FullName + N' t WHERE t.FormKey = f.FormKey
    );';

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
            EnsureRolePermissionColumnsExist();

            if (permissions == null || permissions.Count == 0) return "Success";

            List<int> targetRoleIds = new List<int> { roleId };
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

                // Clean SQL UPSERT into dbo.RolePermissions (or dbo.POS_RolePermissions) table
                using (SqlCommand cmd = new SqlCommand(@"
IF OBJECT_ID('dbo.RolePermissions', 'U') IS NOT NULL
BEGIN
    DECLARE @HasFK BIT = CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RolePermissions') AND name = 'FormKey') THEN 1 ELSE 0 END;
    DECLARE @HasFN BIT = CASE WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.RolePermissions') AND name = 'FormName') THEN 1 ELSE 0 END;

    IF @HasFK = 1 AND @HasFN = 1
    BEGIN
        EXEC sp_executesql N'
        IF EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleID = @RoleID AND FormID = @FormID)
        BEGIN
            UPDATE dbo.RolePermissions
            SET CanView = @CanView, CanAdd = @CanAdd, CanEdit = @CanEdit, CanDelete = @CanDelete,
                FormKey = @FormKey, FormName = @FormName
            WHERE RoleID = @RoleID AND FormID = @FormID;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.RolePermissions (RoleID, FormID, FormKey, FormName, CanView, CanAdd, CanEdit, CanDelete)
            VALUES (@RoleID, @FormID, @FormKey, @FormName, @CanView, @CanAdd, @CanEdit, @CanDelete);
        END',
        N'@RoleID INT, @FormID INT, @FormKey NVARCHAR(100), @FormName NVARCHAR(200), @CanView BIT, @CanAdd BIT, @CanEdit BIT, @CanDelete BIT',
        @RoleID = @RoleID, @FormID = @FormID, @FormKey = @FormKey, @FormName = @FormName, @CanView = @CanView, @CanAdd = @CanAdd, @CanEdit = @CanEdit, @CanDelete = @CanDelete;
    END
    ELSE
    BEGIN
        EXEC sp_executesql N'
        IF EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleID = @RoleID AND FormID = @FormID)
        BEGIN
            UPDATE dbo.RolePermissions
            SET CanView = @CanView, CanAdd = @CanAdd, CanEdit = @CanEdit, CanDelete = @CanDelete
            WHERE RoleID = @RoleID AND FormID = @FormID;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.RolePermissions (RoleID, FormID, CanView, CanAdd, CanEdit, CanDelete)
            VALUES (@RoleID, @FormID, @CanView, @CanAdd, @CanEdit, @CanDelete);
        END',
        N'@RoleID INT, @FormID INT, @CanView BIT, @CanAdd BIT, @CanEdit BIT, @CanDelete BIT',
        @RoleID = @RoleID, @FormID = @FormID, @CanView = @CanView, @CanAdd = @CanAdd, @CanEdit = @CanEdit, @CanDelete = @CanDelete;
    END
END
ELSE IF OBJECT_ID('dbo.POS_RolePermissions', 'U') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.POS_RolePermissions WHERE RoleID = @RoleID AND FormID = @FormID)
    BEGIN
        UPDATE dbo.POS_RolePermissions
        SET CanView = @CanView, CanAdd = @CanAdd, CanEdit = @CanEdit, CanDelete = @CanDelete
        WHERE RoleID = @RoleID AND FormID = @FormID;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.POS_RolePermissions (RoleID, FormID, CanView, CanAdd, CanEdit, CanDelete)
        VALUES (@RoleID, @FormID, @CanView, @CanAdd, @CanEdit, @CanDelete);
    END
END", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.Add("@RoleID", SqlDbType.Int);
                    cmd.Parameters.Add("@FormID", SqlDbType.Int);
                    cmd.Parameters.Add("@FormKey", SqlDbType.NVarChar, 100);
                    cmd.Parameters.Add("@FormName", SqlDbType.NVarChar, 200);
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
    }
}
