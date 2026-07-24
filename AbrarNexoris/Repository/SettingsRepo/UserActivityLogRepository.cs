using ModelClass;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Repository.SettingsRepo
{
    public class UserActivityLogRepository : BaseRepostitory
    {
        public void SaveUserActivity(
            int userId,
            string userName,
            string userRole,
            int counterId,
            string counterName,
            string activityType,
            string activityDetails,
            string formName = null,
            long sessionId = 0)
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                EnsureUserActivityLogTable();
                EnsureUserActivityLogStoredProcedure();

                using (SqlCommand cmd = new SqlCommand("dbo.POS_UserActivityLog", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "SAVE");
                    cmd.Parameters.AddWithValue("@CompanyId", GetCompanyId());
                    cmd.Parameters.AddWithValue("@BranchId", GetBranchId());
                    cmd.Parameters.AddWithValue("@FinYearId", GetFinYearId());
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@UserName", (object)userName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserRole", (object)userRole ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CounterId", counterId);
                    cmd.Parameters.AddWithValue("@CounterName", (object)counterName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActivityType", (object)activityType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActivityDetails", (object)activityDetails ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FormName", (object)formName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SessionId", sessionId);

                    if (activityType == "Login")
                    {
                        cmd.Parameters.AddWithValue("@LoginTime", DateTime.Now);
                        cmd.Parameters.AddWithValue("@LogoutTime", DBNull.Value);
                    }
                    else if (activityType == "Logout")
                    {
                        cmd.Parameters.AddWithValue("@LoginTime", DBNull.Value);
                        cmd.Parameters.AddWithValue("@LogoutTime", DateTime.Now);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@LoginTime", DBNull.Value);
                        cmd.Parameters.AddWithValue("@LogoutTime", DBNull.Value);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving user activity: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        public DataTable GetUserActivityLog(DateTime fromDate, DateTime toDate, string userName, string activityType, string searchText)
        {
            DataTable result = new DataTable();
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                EnsureUserActivityLogTable();
                EnsureUserActivityLogStoredProcedure();

                using (SqlCommand cmd = new SqlCommand("dbo.POS_UserActivityLog", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GET");
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date);
                    cmd.Parameters.AddWithValue("@UserName", userName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@ActivityType", activityType ?? string.Empty);
                    cmd.Parameters.AddWithValue("@SearchText", searchText ?? string.Empty);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(result);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading user activity: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return result;
        }

        public int GetLatestActivityLogId()
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                EnsureUserActivityLogTable();

                using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(MAX(UserActivityLogId), 0) FROM dbo.UserActivityLog", (SqlConnection)DataConnection))
                {
                    object result = cmd.ExecuteScalar();
                    return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
                }
            }
            catch
            {
                return 0;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        public DataTable GetUserActivityUsers()
        {
            return GetDistinctColumnValues("UserName");
        }

        public DataTable GetUserActivityTypes()
        {
            return GetDistinctColumnValues("ActivityType");
        }

        public int CountUserActivity(DateTime fromDate, DateTime toDate, string activityType = null)
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                EnsureUserActivityLogTable();

                string sql = @"
SELECT COUNT(1)
FROM dbo.UserActivityLog
WHERE CreatedOn >= @FromDate
  AND CreatedOn < DATEADD(DAY, 1, @ToDate)";
                
                if (!string.IsNullOrEmpty(activityType))
                {
                    sql += " AND ActivityType = @ActivityType";
                }

                using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date);
                    if (!string.IsNullOrEmpty(activityType))
                    {
                        cmd.Parameters.AddWithValue("@ActivityType", activityType);
                    }
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error counting user activities: {ex.Message}");
                return 0;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        private DataTable GetDistinctColumnValues(string columnName)
        {
            DataTable result = new DataTable();
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                EnsureUserActivityLogTable();

                string unionQuery = $@"
SELECT DISTINCT ISNULL({columnName}, '') AS Value FROM dbo.UserActivityLog WHERE ISNULL({columnName}, '') <> ''
UNION
SELECT DISTINCT ISNULL({columnName}, '') AS Value FROM dbo.PurchaseActivityLog WHERE OBJECT_ID('dbo.PurchaseActivityLog', 'U') IS NOT NULL AND ISNULL({columnName}, '') <> ''
UNION
SELECT DISTINCT ISNULL({columnName}, '') AS Value FROM dbo.SalesActivityLog WHERE OBJECT_ID('dbo.SalesActivityLog', 'U') IS NOT NULL AND ISNULL({columnName}, '') <> ''
UNION
SELECT DISTINCT ISNULL({columnName}, '') AS Value FROM dbo.PurchaseReturnActivityLog WHERE OBJECT_ID('dbo.PurchaseReturnActivityLog', 'U') IS NOT NULL AND ISNULL({columnName}, '') <> ''
UNION
SELECT DISTINCT ISNULL({columnName}, '') AS Value FROM dbo.SalesReturnActivityLog WHERE OBJECT_ID('dbo.SalesReturnActivityLog', 'U') IS NOT NULL AND ISNULL({columnName}, '') <> ''
ORDER BY Value;";

                using (SqlCommand cmd = new SqlCommand(unionQuery, (SqlConnection)DataConnection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(result);
                }
            }
            catch
            {
                // return empty table
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return result;
        }

        private void EnsureUserActivityLogTable()
        {
            using (SqlCommand cmd = new SqlCommand(@"
IF OBJECT_ID('dbo.UserActivityLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserActivityLog
    (
        UserActivityLogId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CompanyId INT NOT NULL DEFAULT(0),
        BranchId INT NOT NULL DEFAULT(0),
        FinYearId INT NOT NULL DEFAULT(0),
        UserId INT NOT NULL DEFAULT(0),
        UserName NVARCHAR(150) NULL,
        UserRole NVARCHAR(100) NULL,
        CounterId INT NOT NULL DEFAULT(0),
        CounterName NVARCHAR(150) NULL,
        ActivityType NVARCHAR(50) NOT NULL,
        ActivityDetails NVARCHAR(1000) NULL,
        FormName NVARCHAR(250) NULL,
        LoginTime DATETIME NULL,
        LogoutTime DATETIME NULL,
        SessionId BIGINT NOT NULL DEFAULT(0),
        CreatedOn DATETIME NOT NULL DEFAULT(GETDATE())
    );

    CREATE INDEX IX_UserActivityLog_UserId ON dbo.UserActivityLog(UserId, CreatedOn);
    CREATE INDEX IX_UserActivityLog_CreatedOn ON dbo.UserActivityLog(CreatedOn);
END;", (SqlConnection)DataConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private void EnsureUserActivityLogStoredProcedure()
        {
            using (SqlCommand cmd = new SqlCommand(@"
IF OBJECT_ID(N'dbo.POS_UserActivityLog', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo.POS_UserActivityLog AS BEGIN SET NOCOUNT ON; END');", (SqlConnection)DataConnection))
            {
                cmd.ExecuteNonQuery();
            }

            using (SqlCommand cmd = new SqlCommand(@"
ALTER PROCEDURE dbo.POS_UserActivityLog
    @_Operation NVARCHAR(30),
    @UserActivityLogId INT = 0,
    @CompanyId INT = 0,
    @BranchId INT = 0,
    @FinYearId INT = 0,
    @UserId INT = 0,
    @UserName NVARCHAR(150) = NULL,
    @UserRole NVARCHAR(100) = NULL,
    @CounterId INT = 0,
    @CounterName NVARCHAR(150) = NULL,
    @ActivityType NVARCHAR(50) = NULL,
    @ActivityDetails NVARCHAR(1000) = NULL,
    @FormName NVARCHAR(250) = NULL,
    @LoginTime DATETIME = NULL,
    @LogoutTime DATETIME = NULL,
    @SessionId BIGINT = 0,
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL,
    @SearchText NVARCHAR(250) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @_Operation = N'SAVE'
    BEGIN
        INSERT INTO dbo.UserActivityLog
        (
            CompanyId, BranchId, FinYearId, UserId, UserName, UserRole,
            CounterId, CounterName, ActivityType, ActivityDetails, FormName,
            LoginTime, LogoutTime, SessionId, CreatedOn
        )
        VALUES
        (
            @CompanyId, @BranchId, @FinYearId, @UserId, @UserName, @UserRole,
            @CounterId, @CounterName, @ActivityType, @ActivityDetails, @FormName,
            @LoginTime, @LogoutTime, @SessionId, GETDATE()
        );
    END
    ELSE IF @_Operation = N'GET'
    BEGIN
        WITH CombinedLogs AS
        (
            SELECT
                UserActivityLogId,
                CompanyId,
                BranchId,
                FinYearId,
                UserId,
                UserName,
                UserRole,
                CounterId,
                CounterName,
                ActivityType,
                ActivityDetails,
                FormName,
                LoginTime,
                LogoutTime,
                SessionId,
                CreatedOn
            FROM dbo.UserActivityLog

            UNION ALL

            -- Purchase Activity Logs (FrmPurchase)
            SELECT
                ISNULL(pal.ActivityLogId, 0) AS UserActivityLogId,
                ISNULL(pal.CompanyId, 0) AS CompanyId,
                ISNULL(pal.BranchId, 0) AS BranchId,
                ISNULL(pal.FinYearId, 0) AS FinYearId,
                ISNULL(pal.UserId, 0) AS UserId,
                ISNULL(pal.UserName, N'') AS UserName,
                N'User' AS UserRole,
                ISNULL(pal.CounterId, 0) AS CounterId,
                ISNULL(pal.CounterName, N'') AS CounterName,
                ISNULL(pal.ActivityType, N'SAVE') AS ActivityType,
                N'Purchase GRN-' + CONVERT(nvarchar(50), pal.TransactionNo) 
                    + CASE WHEN ISNULL(pal.InvoiceNo, N'') <> N'' THEN N' (Inv: ' + pal.InvoiceNo + N')' ELSE N'' END
                    + N', Amount: ' + CONVERT(nvarchar(50), CAST(ISNULL(pal.NetAmount, 0) AS decimal(18,2)))
                    + CASE WHEN ISNULL(pal.PartyName, N'') <> N'' THEN N', Vendor: ' + pal.PartyName ELSE N'' END AS ActivityDetails,
                N'FrmPurchase' AS FormName,
                NULL AS LoginTime,
                NULL AS LogoutTime,
                ISNULL(pal.CounterSessionId, 0) AS SessionId,
                pal.CreatedOn
            FROM dbo.PurchaseActivityLog pal
            WHERE OBJECT_ID('dbo.PurchaseActivityLog', 'U') IS NOT NULL
              AND NOT EXISTS (
                SELECT 1 FROM dbo.UserActivityLog ual 
                WHERE ual.FormName = N'FrmPurchase' 
                  AND ual.ActivityDetails LIKE N'%' + CONVERT(nvarchar(50), pal.TransactionNo) + N'%'
                  AND ual.ActivityType = pal.ActivityType
                  AND DATEDIFF(SECOND, ual.CreatedOn, pal.CreatedOn) BETWEEN -5 AND 5
            )

            UNION ALL

            -- Sales Invoice Activity Logs (frmSalesInvoice)
            SELECT
                ISNULL(sal.ActivityLogId, 0) AS UserActivityLogId,
                ISNULL(sal.CompanyId, 0) AS CompanyId,
                ISNULL(sal.BranchId, 0) AS BranchId,
                ISNULL(sal.FinYearId, 0) AS FinYearId,
                ISNULL(sal.UserId, 0) AS UserId,
                ISNULL(sal.UserName, N'') AS UserName,
                N'User' AS UserRole,
                ISNULL(sal.CounterId, 0) AS CounterId,
                ISNULL(sal.CounterName, N'') AS CounterName,
                ISNULL(sal.ActivityType, N'SAVE') AS ActivityType,
                N'Sales Invoice Bill #' + CONVERT(nvarchar(50), sal.TransactionNo) 
                    + N', Amount: ' + CONVERT(nvarchar(50), CAST(ISNULL(sal.NetAmount, 0) AS decimal(18,2)))
                    + CASE WHEN ISNULL(sal.PartyName, N'') <> N'' THEN N', Customer: ' + sal.PartyName ELSE N'' END AS ActivityDetails,
                N'frmSalesInvoice' AS FormName,
                NULL AS LoginTime,
                NULL AS LogoutTime,
                ISNULL(sal.CounterSessionId, 0) AS SessionId,
                sal.CreatedOn
            FROM dbo.SalesActivityLog sal
            WHERE OBJECT_ID('dbo.SalesActivityLog', 'U') IS NOT NULL
              AND NOT EXISTS (
                SELECT 1 FROM dbo.UserActivityLog ual 
                WHERE ual.FormName = N'frmSalesInvoice' 
                  AND ual.ActivityDetails LIKE N'%' + CONVERT(nvarchar(50), sal.TransactionNo) + N'%'
                  AND ual.ActivityType = sal.ActivityType
                  AND DATEDIFF(SECOND, ual.CreatedOn, sal.CreatedOn) BETWEEN -5 AND 5
            )

            UNION ALL

            -- Purchase Return Activity Logs (frmPurchaseReturn)
            SELECT
                ISNULL(pral.ActivityLogId, 0) AS UserActivityLogId,
                ISNULL(pral.CompanyId, 0) AS CompanyId,
                ISNULL(pral.BranchId, 0) AS BranchId,
                ISNULL(pral.FinYearId, 0) AS FinYearId,
                ISNULL(pral.UserId, 0) AS UserId,
                ISNULL(pral.UserName, N'') AS UserName,
                N'User' AS UserRole,
                ISNULL(pral.CounterId, 0) AS CounterId,
                ISNULL(pral.CounterName, N'') AS CounterName,
                ISNULL(pral.ActivityType, N'SAVE') AS ActivityType,
                N'Purchase Return GRN-' + CONVERT(nvarchar(50), pral.TransactionNo) 
                    + N', Amount: ' + CONVERT(nvarchar(50), CAST(ISNULL(pral.NetAmount, 0) AS decimal(18,2)))
                    + CASE WHEN ISNULL(pral.PartyName, N'') <> N'' THEN N', Vendor: ' + pral.PartyName ELSE N'' END AS ActivityDetails,
                N'frmPurchaseReturn' AS FormName,
                NULL AS LoginTime,
                NULL AS LogoutTime,
                ISNULL(pral.CounterSessionId, 0) AS SessionId,
                pral.CreatedOn
            FROM dbo.PurchaseReturnActivityLog pral
            WHERE OBJECT_ID('dbo.PurchaseReturnActivityLog', 'U') IS NOT NULL
              AND NOT EXISTS (
                SELECT 1 FROM dbo.UserActivityLog ual 
                WHERE ual.FormName = N'frmPurchaseReturn' 
                  AND ual.ActivityDetails LIKE N'%' + CONVERT(nvarchar(50), pral.TransactionNo) + N'%'
                  AND ual.ActivityType = pral.ActivityType
                  AND DATEDIFF(SECOND, ual.CreatedOn, pral.CreatedOn) BETWEEN -5 AND 5
            )

            UNION ALL

            -- Sales Return Activity Logs (frmSalesReturn)
            SELECT
                ISNULL(sral.ActivityLogId, 0) AS UserActivityLogId,
                ISNULL(sral.CompanyId, 0) AS CompanyId,
                ISNULL(sral.BranchId, 0) AS BranchId,
                ISNULL(sral.FinYearId, 0) AS FinYearId,
                ISNULL(sral.UserId, 0) AS UserId,
                ISNULL(sral.UserName, N'') AS UserName,
                N'User' AS UserRole,
                ISNULL(sral.CounterId, 0) AS CounterId,
                ISNULL(sral.CounterName, N'') AS CounterName,
                ISNULL(sral.ActivityType, N'SAVE') AS ActivityType,
                N'Sales Return Bill #' + CONVERT(nvarchar(50), sral.TransactionNo) 
                    + N', Amount: ' + CONVERT(nvarchar(50), CAST(ISNULL(sral.NetAmount, 0) AS decimal(18,2)))
                    + CASE WHEN ISNULL(sral.PartyName, N'') <> N'' THEN N', Customer: ' + sral.PartyName ELSE N'' END AS ActivityDetails,
                N'frmSalesReturn' AS FormName,
                NULL AS LoginTime,
                NULL AS LogoutTime,
                ISNULL(sral.CounterSessionId, 0) AS SessionId,
                sral.CreatedOn
            FROM dbo.SalesReturnActivityLog sral
            WHERE OBJECT_ID('dbo.SalesReturnActivityLog', 'U') IS NOT NULL
              AND NOT EXISTS (
                SELECT 1 FROM dbo.UserActivityLog ual 
                WHERE ual.FormName = N'frmSalesReturn' 
                  AND ual.ActivityDetails LIKE N'%' + CONVERT(nvarchar(50), sral.TransactionNo) + N'%'
                  AND ual.ActivityType = sral.ActivityType
                  AND DATEDIFF(SECOND, ual.CreatedOn, sral.CreatedOn) BETWEEN -5 AND 5
            )
        )
        SELECT
            UserActivityLogId,
            CompanyId,
            BranchId,
            FinYearId,
            UserId,
            UserName,
            UserRole,
            CounterId,
            CounterName,
            ActivityType,
            ActivityDetails,
            FormName,
            LoginTime,
            LogoutTime,
            SessionId,
            CreatedOn
        FROM CombinedLogs
        WHERE CreatedOn >= @FromDate
          AND CreatedOn < DATEADD(DAY, 1, @ToDate)
          AND (@UserName = '' OR ISNULL(UserName, '') = @UserName)
          AND (@ActivityType = '' OR ISNULL(ActivityType, '') = @ActivityType)
          AND (
                @SearchText = ''
                OR ISNULL(ActivityDetails, '') LIKE '%' + @SearchText + '%'
                OR ISNULL(FormName, '') LIKE '%' + @SearchText + '%'
                OR ISNULL(UserRole, '') LIKE '%' + @SearchText + '%'
                OR ISNULL(UserName, '') LIKE '%' + @SearchText + '%'
              )
        ORDER BY CreatedOn DESC, UserActivityLogId DESC;
    END
END;", (SqlConnection)DataConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private static int GetCompanyId()
        {
            return SessionContext.CompanyId > 0 ? SessionContext.CompanyId : ParseInt(DataBase.CompanyId);
        }

        private static int GetBranchId()
        {
            return SessionContext.BranchId > 0 ? SessionContext.BranchId : ParseInt(DataBase.BranchId);
        }

        private static int GetFinYearId()
        {
            return SessionContext.FinYearId > 0 ? SessionContext.FinYearId : ParseInt(DataBase.FinyearId);
        }

        private static int ParseInt(string value)
        {
            int parsed;
            return int.TryParse(value, out parsed) ? parsed : 0;
        }
    }
}
