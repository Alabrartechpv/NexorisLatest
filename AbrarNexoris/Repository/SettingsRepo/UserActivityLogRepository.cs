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

                using (SqlCommand cmd = new SqlCommand($@"
SELECT DISTINCT ISNULL({columnName}, '') AS Value
FROM dbo.UserActivityLog
WHERE ISNULL({columnName}, '') <> ''
ORDER BY Value;", (SqlConnection)DataConnection))
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
