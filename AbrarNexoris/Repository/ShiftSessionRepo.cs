using ModelClass;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Repository
{
    public class ShiftSessionRepo : BaseRepostitory
    {
        public bool EnsureSchema()
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                string sql = @"
IF OBJECT_ID('dbo.CounterSessions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CounterSessions
    (
        SessionId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CounterSessions PRIMARY KEY,
        CompanyId INT NOT NULL,
        BranchId INT NOT NULL,
        FinYearId INT NOT NULL,
        CounterId INT NOT NULL,
        CounterName VARCHAR(50) NULL,
        UserId INT NOT NULL,
        LoginTime DATETIME NOT NULL,
        CloseTime DATETIME NULL,
        ShiftClosingId INT NULL,
        Status VARCHAR(20) NOT NULL CONSTRAINT DF_CounterSessions_Status DEFAULT('Open'),
        SystemName VARCHAR(100) NULL,
        CreatedDate DATETIME NOT NULL CONSTRAINT DF_CounterSessions_CreatedDate DEFAULT(GETDATE()),
        ModifiedDate DATETIME NULL
    );

    CREATE INDEX IX_CounterSessions_Open
        ON dbo.CounterSessions(BranchId, CounterId, Status, LoginTime);
END";

                using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                {
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                throw new InvalidOperationException("This counter or user already has an open session. Please close the existing counter session first.", ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        public bool StartOrResumeSession()
        {
            EnsureSchema();

            if (SessionContext.CounterId <= 0)
                return false;

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand("dbo.POS_CounterSession", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@CounterId", SessionContext.CounterId);
                    cmd.Parameters.AddWithValue("@CounterName", (object)SessionContext.CounterName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserId", SessionContext.UserId);
                    cmd.Parameters.AddWithValue("@SystemName", Environment.MachineName);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return false;

                        SessionContext.CounterSessionId = Convert.ToInt64(reader["SessionId"]);
                        SessionContext.LoginTime = Convert.ToDateTime(reader["LoginTime"]);
                        string status = reader["Status"]?.ToString() ?? "Open";
                        int sessionUserId = Convert.ToInt32(reader["UserId"]);
                        SessionContext.RequiresClosing = !status.Equals("Open", StringComparison.OrdinalIgnoreCase)
                            || DateTime.Now.Date > SessionContext.LoginTime.Date
                            || SessionContext.UserId != sessionUserId;
                    }
                }

                return true;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        public bool IsCurrentSessionOpen(out string errorMessage)
        {
            errorMessage = null;

            if (SessionContext.UserLevel?.Equals("Administrator", StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }

            if (SessionContext.CounterSessionId <= 0)
            {
                errorMessage = "Counter session is not started. Please login again.";
                return false;
            }

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(@"
SELECT LoginTime, Status
FROM dbo.CounterSessions
WHERE SessionId = @SessionId
  AND BranchId = @BranchId
  AND CounterId = @CounterId
  AND UserId = @UserId;", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@SessionId", SessionContext.CounterSessionId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@CounterId", SessionContext.CounterId);
                    cmd.Parameters.AddWithValue("@UserId", SessionContext.UserId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            errorMessage = "Counter session was not found. Please login again.";
                            return false;
                        }

                        SessionContext.LoginTime = Convert.ToDateTime(reader["LoginTime"]);
                        string status = reader["Status"]?.ToString() ?? "";
                        if (!status.Equals("Open", StringComparison.OrdinalIgnoreCase))
                        {
                            errorMessage = "Please complete shift closing before continuing transactions.";
                            SessionContext.RequiresClosing = true;
                            return false;
                        }
                    }
                }

                if (DateTime.Now.Date > SessionContext.LoginTime.Date)
                {
                    errorMessage = "Please complete shift closing before continuing transactions.";
                    SessionContext.RequiresClosing = true;
                    return false;
                }

                return true;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        public void CloseCurrentSession(int shiftClosingId, DateTime closeTime, SqlTransaction transaction)
        {
            using (SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.CounterSessions
SET Status = 'Closed',
    CloseTime = GETDATE(),
    ShiftClosingId = @ShiftClosingId,
    ModifiedDate = GETDATE()
WHERE SessionId = @SessionId
  AND Status = 'Open';", (SqlConnection)DataConnection, transaction))
            {
                cmd.Parameters.AddWithValue("@ShiftClosingId", shiftClosingId);
                cmd.Parameters.AddWithValue("@SessionId", SessionContext.CounterSessionId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
