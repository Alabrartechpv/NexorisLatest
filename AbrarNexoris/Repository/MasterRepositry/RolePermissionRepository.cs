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
            DataConnection.Open();

            try
            {
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
                            roles = ds.Tables[0].ToListOfObject<Role>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting roles: {ex.Message}");
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
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
            List<FormPermissionGrid> forms = new List<FormPermissionGrid>();
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
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return forms;
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

    SET @Sql = N'
IF NOT EXISTS (SELECT 1 FROM ' + @FullName + N' WHERE FormKey = @FormKey)
BEGIN
    INSERT INTO ' + @FullName + N'
    (
        FormKey,
        FormName' +
        CASE WHEN @HasCategory = 1 THEN N',
        Category' ELSE N'' END +
        CASE WHEN @HasIsActive = 1 THEN N',
        IsActive' ELSE N'' END +
    N'
    )
    VALUES
    (
        @FormKey,
        @FormName' +
        CASE WHEN @HasCategory = 1 THEN N',
        @Category' ELSE N'' END +
        CASE WHEN @HasIsActive = 1 THEN N',
        1' ELSE N'' END +
    N'
    );
END';

    EXEC sp_executesql
        @Sql,
        N'@FormKey NVARCHAR(100), @FormName NVARCHAR(200), @Category NVARCHAR(100)',
        @FormKey = N'ActivityLog',
        @FormName = N'Activity Log',
        @Category = N'Settings';

    EXEC sp_executesql
        @Sql,
        N'@FormKey NVARCHAR(100), @FormName NVARCHAR(200), @Category NVARCHAR(100)',
        @FormKey = N'stocktransfer',
        @FormName = N'Stock Transfer',
        @Category = N'Transaction';
END", (SqlConnection)DataConnection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unable to ensure ActivityLog permission form: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves a single permission
        /// </summary>
        public string SavePermission(int roleId, int formId, bool canView, bool canAdd, bool canEdit, bool canDelete)
        {
            DataConnection.Open();
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
            DataConnection.Open();
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
