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
            List<RolePermission> permissions = new List<RolePermission>();
            DataConnection.Open();

            try
            {
                EnsureActivityLogPermissionForm();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_RolePermission, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RoleID", roleId);
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
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null))
                        {
                            permissions = ds.Tables[0].ToListOfObject<RolePermission>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting permissions: {ex.Message}");
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return permissions;
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

            // 0. Ensure default roles exist in dbo.Roles DB table to satisfy FK_RolePermissions_Roles constraint
            EnsureRolesSeededInDatabase();

            // 1. Query dbo.Roles table directly if present
            try
            {
                if (DataConnection.State != ConnectionState.Open) DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(@"
IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL
BEGIN
    SELECT RoleID, RoleName FROM dbo.Roles 
    WHERE (EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Roles') AND name = 'IsDelete') AND IsDelete = 0)
       OR NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Roles') AND name = 'IsDelete')
    ORDER BY RoleID;
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
                System.Diagnostics.Debug.WriteLine($"Error querying dbo.Roles directly: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            // 2. Try stored procedure GETALLROLES
            if (roles == null || roles.Count == 0)
            {
                try
                {
                    if (DataConnection.State != ConnectionState.Open) DataConnection.Open();

                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_RolePermission, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@RoleID", 0);
                        cmd.Parameters.AddWithValue("@FormID", 0);
                        cmd.Parameters.AddWithValue("@CanView", 0);
                        cmd.Parameters.AddWithValue("@CanAdd", 0);
                        cmd.Parameters.AddWithValue("@CanEdit", 0);
                        cmd.Parameters.AddWithValue("@CanDelete", 0);
                        cmd.Parameters.AddWithValue("@_Operation", "GETALLROLES");

                        using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                        {
                            DataSet ds = new DataSet();
                            adapt.Fill(ds);
                            if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null))
                            {
                                var fetched = ds.Tables[0].ToListOfObject<Role>();
                                if (fetched != null && fetched.Count > 0)
                                {
                                    roles.AddRange(fetched);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting roles from SP: {ex.Message}");
                }
                finally
                {
                    if (DataConnection.State == ConnectionState.Open)
                        DataConnection.Close();
                }
            }

            // 3. Query Dropdowns UserLevel
            try
            {
                var userLevels = new Dropdowns().UserLevelDDl();
                if (userLevels != null && userLevels.List != null)
                {
                    foreach (var u in userLevels.List)
                    {
                        if (u != null && !string.IsNullOrWhiteSpace(u.UserLevel))
                        {
                            roles.Add(new Role { RoleID = u.UserLevelID, RoleName = u.UserLevel.Trim() });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fallback roles from UserLevel error: {ex.Message}");
            }

            // 4. Always include standard system roles
            var defaultRoles = new List<Role>
            {
                new Role { RoleID = 1, RoleName = "Administrator" },
                new Role { RoleID = 2, RoleName = "Manager" },
                new Role { RoleID = 3, RoleName = "Supervisor" },
                new Role { RoleID = 4, RoleName = "Cashier" },
                new Role { RoleID = 5, RoleName = "Sales Man" },
                new Role { RoleID = 6, RoleName = "Accountant" },
                new Role { RoleID = 7, RoleName = "Inventory Manager" },
                new Role { RoleID = 8, RoleName = "Standard User" }
            };
            roles.AddRange(defaultRoles);

            // 5. Deduplicate roles by RoleName and assign UserLevelID
            var uniqueRoles = new List<Role>();
            var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            int nextId = 100;
            foreach (var r in roles)
            {
                if (r == null || string.IsNullOrWhiteSpace(r.RoleName)) continue;
                string cleanName = r.RoleName.Trim();
                if (seenNames.Add(cleanName))
                {
                    r.RoleName = cleanName;
                    if (r.RoleID <= 0) r.RoleID = nextId++;
                    if (!r.UserLevelID.HasValue || r.UserLevelID.Value <= 0)
                        r.UserLevelID = r.RoleID;
                    uniqueRoles.Add(r);
                }
            }

            return uniqueRoles;
        }

        /// <summary>
        /// Gets all forms with their permissions for a specific role (for admin UI)
        /// </summary>
        /// <param name="roleId">Role ID to get permissions for</param>
        /// <returns>List of FormPermissionGrid objects</returns>
        public List<FormPermissionGrid> GetFormsWithPermissions(int roleId)
        {
            List<FormPermissionGrid> forms = new List<FormPermissionGrid>();

            try
            {
                if (DataConnection.State != ConnectionState.Open) DataConnection.Open();
                EnsureActivityLogPermissionForm();

                if (DataConnection.State != ConnectionState.Open) DataConnection.Open();
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_RolePermission, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RoleID", roleId);
                    cmd.Parameters.AddWithValue("@FormID", 0);
                    cmd.Parameters.AddWithValue("@CanView", 0);
                    cmd.Parameters.AddWithValue("@CanAdd", 0);
                    cmd.Parameters.AddWithValue("@CanEdit", 0);
                    cmd.Parameters.AddWithValue("@CanDelete", 0);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALLFORMS");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null))
                        {
                            forms = ds.Tables[0].ToListOfObject<FormPermissionGrid>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting forms with permissions: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            // Fallback: Query Forms table directly
            if (forms == null || forms.Count == 0)
            {
                try
                {
                    if (DataConnection.State != ConnectionState.Open) DataConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(@"
SELECT FormID, FormKey, FormName, ISNULL(Category, 'General') AS Category, 
       CAST(1 AS BIT) AS CanView, CAST(1 AS BIT) AS CanAdd, CAST(1 AS BIT) AS CanEdit, CAST(1 AS BIT) AS CanDelete
FROM Forms", (SqlConnection)DataConnection))
                    {
                        using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                        {
                            DataSet ds = new DataSet();
                            adapt.Fill(ds);
                            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                            {
                                forms = ds.Tables[0].ToListOfObject<FormPermissionGrid>();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Fallback forms query error: {ex.Message}");
                }
                finally
                {
                    if (DataConnection.State == ConnectionState.Open)
                        DataConnection.Close();
                }
            }

            // Fallback 2: Return static list of all 88 forms
            if (forms == null || forms.Count == 0)
            {
                forms = GetStaticFormPermissionsList();
            }

            // Ensure no null Category properties
            foreach (var f in forms)
            {
                if (string.IsNullOrWhiteSpace(f.Category)) f.Category = "General";
            }

            return forms;
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

    IF @HasUserLevelID = 1
    BEGIN
        EXEC sp_executesql N'UPDATE dbo.Roles SET UserLevelID = RoleID WHERE UserLevelID IS NULL OR UserLevelID = 0;';
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

        /// <summary>
        /// Saves multiple permissions for a role (bulk save for admin UI)
        /// </summary>
        public string SavePermissions(int roleId, List<FormPermissionGrid> permissions)
        {
            EnsureRolesSeededInDatabase();

            if (DataConnection.State != ConnectionState.Open) DataConnection.Open();
            var trans = DataConnection.BeginTransaction();

            try
            {
                foreach (var perm in permissions)
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_RolePermission, (SqlConnection)DataConnection, (SqlTransaction)trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@RoleID", roleId);
                        cmd.Parameters.AddWithValue("@FormID", perm.FormID);
                        cmd.Parameters.AddWithValue("@CanView", perm.CanView);
                        cmd.Parameters.AddWithValue("@CanAdd", perm.CanAdd);
                        cmd.Parameters.AddWithValue("@CanEdit", perm.CanEdit);
                        cmd.Parameters.AddWithValue("@CanDelete", perm.CanDelete);
                        cmd.Parameters.AddWithValue("@_Operation", "SAVE");
                        cmd.ExecuteNonQuery();
                    }
                }

                trans.Commit();
                return "Success";
            }
            catch (Exception ex)
            {
                trans.Rollback();
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
