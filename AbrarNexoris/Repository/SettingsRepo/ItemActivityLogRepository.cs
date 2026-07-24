using ModelClass;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Repository.SettingsRepo
{
    public class ItemActivityLogRepository : BaseRepostitory
    {
        public void SaveItemActivity(
            int itemId,
            string itemNo,
            string itemName,
            string barcode,
            string activityType,
            string activityDetails,
            decimal? unitCost = null,
            decimal? retailPrice = null,
            decimal? walkinPrice = null,
            decimal? quantity = null,
            decimal? available = null,
            decimal? onHold = null,
            decimal? reorder = null,
            int? orderCycleDays = null,
            int? boxQty = null,
            string itemType = null,
            string category = null,
            string itemGroup = null,
            string hsn = null,
            string itemStatus = null)
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                EnsureItemActivityLogTable();
                EnsureItemActivityLogStoredProcedure();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemActivityLog, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "SAVE");
                    cmd.Parameters.AddWithValue("@ItemId", itemId);
                    cmd.Parameters.AddWithValue("@ItemNo", (object)itemNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ItemName", (object)itemName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Barcode", (object)barcode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActivityType", (object)activityType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActivityDetails", (object)activityDetails ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitCost", (object)unitCost ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RetailPrice", (object)retailPrice ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@WalkinPrice", (object)walkinPrice ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Quantity", (object)quantity ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Available", (object)available ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@OnHold", (object)onHold ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Reorder", (object)reorder ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@OrderCycleDays", (object)orderCycleDays ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BoxQty", (object)boxQty ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ItemType", (object)itemType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Category", (object)category ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ItemGroup", (object)itemGroup ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@HSN", (object)hsn ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ItemStatus", (object)itemStatus ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CompanyId", GetCompanyId());
                    cmd.Parameters.AddWithValue("@BranchId", GetBranchId());
                    cmd.Parameters.AddWithValue("@FinYearId", GetFinYearId());
                    cmd.Parameters.AddWithValue("@UserId", GetUserId());
                    cmd.Parameters.AddWithValue("@UserName", GetUserName());
                    cmd.Parameters.AddWithValue("@CounterId", SessionContext.CounterId);
                    cmd.Parameters.AddWithValue("@CounterName", (object)(SessionContext.CounterName ?? string.Empty));
                    cmd.Parameters.AddWithValue("@CounterSessionId", SessionContext.CounterSessionId);
                    cmd.ExecuteNonQuery();
                }

                // Mirror to central UserActivityLog for audit trail
                try
                {
                    string mirrorDetails;
                    if (string.Equals(activityType, "SAVE", StringComparison.OrdinalIgnoreCase))
                    {
                        mirrorDetails = $"New Item Added: '{itemName}'" +
                            (unitCost.HasValue   ? $", Unit Cost: {unitCost.Value:N4}"    : "") +
                            (retailPrice.HasValue ? $", Retail Price: {retailPrice.Value:N4}" : "") +
                            (walkinPrice.HasValue ? $", Walkin Price: {walkinPrice.Value:N4}" : "") +
                            (!string.IsNullOrWhiteSpace(barcode) ? $", Barcode: {barcode}" : "");
                    }
                    else if (string.Equals(activityType, "UPDATE", StringComparison.OrdinalIgnoreCase))
                    {
                        mirrorDetails = $"Item Updated: '{itemName}'" +
                            (unitCost.HasValue   ? $", Unit Cost: {unitCost.Value:N4}"    : "") +
                            (retailPrice.HasValue ? $", Retail Price: {retailPrice.Value:N4}" : "") +
                            (!string.IsNullOrWhiteSpace(barcode) ? $", Barcode: {barcode}" : "");
                    }
                    else
                    {
                        mirrorDetails = $"Item '{itemName}' — {activityType}";
                    }

                    using (var userRepo = new UserActivityLogRepository())
                    {
                        userRepo.SaveUserActivity(
                            GetUserId(),
                            GetUserName(),
                            SessionContext.UserLevel,
                            SessionContext.CounterId,
                            SessionContext.CounterName,
                            activityType,
                            mirrorDetails,
                            "frmItemMasterNew",
                            SessionContext.CounterSessionId);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error mirroring item activity to UserActivityLog: {ex.Message}");
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        public DataTable GetItemActivityLog(DateTime fromDate, DateTime toDate, string userName, string activityType, string itemSearch)
        {
            DataTable result = new DataTable();
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                EnsureItemActivityLogTable();

                using (SqlCommand cmd = new SqlCommand(@"
SELECT
    ItemActivityLogId,
    CreatedOn,
    UserName,
    ActivityType,
    ItemNo,
    ItemName,
    Barcode,
    ActivityDetails,
    UnitCost,
    RetailPrice,
    WalkinPrice,
    ISNULL(Quantity, 0) AS Quantity,
    ISNULL(Available, 0) AS Available,
    ISNULL(OnHold, 0) AS OnHold,
    ISNULL(Reorder, 0) AS Reorder,
    ISNULL(OrderCycleDays, 0) AS OrderCycleDays,
    ISNULL(BoxQty, 0) AS BoxQty,
    ISNULL(ItemType, '') AS ItemType,
    ISNULL(Category, '') AS Category,
    ISNULL(ItemGroup, '') AS ItemGroup,
    ISNULL(HSN, '') AS HSN,
    ISNULL(ItemStatus, '') AS ItemStatus,
    CompanyId,
    BranchId,
    FinYearId,
    UserId,
    CounterName,
    CounterId,
    CounterSessionId
FROM dbo.ItemActivityLog
WHERE CreatedOn >= @FromDate
  AND CreatedOn < DATEADD(DAY, 1, @ToDate)
  AND (@UserName = '' OR ISNULL(UserName, '') = @UserName)
  AND (@ActivityType = '' OR ISNULL(ActivityType, '') = @ActivityType)
  AND (
        @ItemSearch = ''
        OR ISNULL(ItemName, '') LIKE '%' + @ItemSearch + '%'
        OR ISNULL(ItemNo, '') LIKE '%' + @ItemSearch + '%'
        OR ISNULL(Barcode, '') LIKE '%' + @ItemSearch + '%'
      )
ORDER BY CreatedOn DESC, ItemActivityLogId DESC;", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date);
                    cmd.Parameters.AddWithValue("@UserName", userName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@ActivityType", activityType ?? string.Empty);
                    cmd.Parameters.AddWithValue("@ItemSearch", itemSearch ?? string.Empty);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(result);
                    }
                }
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

        public DataTable GetItemHistoryLog(string searchText)
        {
            DataTable result = new DataTable();
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                EnsureItemActivityLogTable();

                using (SqlCommand cmd = new SqlCommand(@"
WITH MatchingItemIds AS (
    SELECT ItemId FROM dbo.ItemMaster WHERE Barcode = @SearchText OR Description LIKE '%' + @SearchText + '%' OR ItemNo = @SearchText
    UNION
    SELECT ItemId FROM dbo.PriceSettings WHERE BarCode = @SearchText OR AliasBarcode = @SearchText
    UNION
    SELECT ItemId FROM dbo.ItemAlternativeBarcode WHERE Barcode = @SearchText
    UNION
    SELECT ItemId FROM dbo.ItemActivityLog WHERE Barcode = @SearchText OR ItemName LIKE '%' + @SearchText + '%' OR ItemNo = @SearchText
)
SELECT
    ItemActivityLogId,
    CreatedOn,
    UserName,
    ActivityType,
    ItemNo,
    ItemName,
    Barcode,
    ActivityDetails,
    UnitCost,
    RetailPrice,
    WalkinPrice,
    ISNULL(Quantity, 0) AS Quantity,
    ISNULL(Available, 0) AS Available,
    ISNULL(OnHold, 0) AS OnHold,
    ISNULL(Reorder, 0) AS Reorder,
    ISNULL(OrderCycleDays, 0) AS OrderCycleDays,
    ISNULL(BoxQty, 0) AS BoxQty,
    ISNULL(ItemType, '') AS ItemType,
    ISNULL(Category, '') AS Category,
    ISNULL(ItemGroup, '') AS ItemGroup,
    ISNULL(HSN, '') AS HSN,
    ISNULL(ItemStatus, '') AS ItemStatus,
    CompanyId,
    BranchId,
    FinYearId,
    UserId,
    CounterName,
    CounterId,
    CounterSessionId
FROM dbo.ItemActivityLog
WHERE ItemId IN (SELECT ItemId FROM MatchingItemIds)
ORDER BY CreatedOn DESC, ItemActivityLogId DESC;", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@SearchText", searchText ?? string.Empty);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(result);
                    }
                }
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

                EnsureItemActivityLogTable();

                using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(MAX(ItemActivityLogId), 0) FROM dbo.ItemActivityLog", (SqlConnection)DataConnection))
                {
                    object result = cmd.ExecuteScalar();
                    return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        public DataTable GetItemActivityUsers()
        {
            return GetDistinctColumnValues("UserName");
        }

        public DataTable GetItemActivityTypes()
        {
            return GetDistinctColumnValues("ActivityType");
        }

        public int CountItemActivity(DateTime fromDate, DateTime toDate)
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                EnsureItemActivityLogTable();

                using (SqlCommand cmd = new SqlCommand(@"
SELECT COUNT(1)
FROM dbo.ItemActivityLog
WHERE CreatedOn >= @FromDate
  AND CreatedOn < DATEADD(DAY, 1, @ToDate);", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
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

                EnsureItemActivityLogTable();

                using (SqlCommand cmd = new SqlCommand($@"
SELECT DISTINCT ISNULL({columnName}, '') AS Value
FROM dbo.ItemActivityLog
WHERE ISNULL({columnName}, '') <> ''
ORDER BY Value;", (SqlConnection)DataConnection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(result);
                }
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

        private void EnsureItemActivityLogTable()
        {
            using (SqlCommand cmd = new SqlCommand(@"
IF OBJECT_ID('dbo.ItemActivityLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ItemActivityLog
    (
        ItemActivityLogId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ItemId INT NOT NULL DEFAULT(0),
        ItemNo NVARCHAR(50) NULL,
        ItemName NVARCHAR(250) NULL,
        Barcode NVARCHAR(100) NULL,
        ActivityType NVARCHAR(50) NOT NULL,
        ActivityDetails NVARCHAR(2000) NULL,
        UnitCost DECIMAL(18,4) NULL,
        RetailPrice DECIMAL(18,4) NULL,
        WalkinPrice DECIMAL(18,4) NULL,
        CompanyId INT NOT NULL DEFAULT(0),
        BranchId INT NOT NULL DEFAULT(0),
        FinYearId INT NOT NULL DEFAULT(0),
        UserId INT NOT NULL DEFAULT(0),
        UserName NVARCHAR(150) NULL,
        CounterId INT NOT NULL DEFAULT(0),
        CounterName NVARCHAR(150) NULL,
        CounterSessionId BIGINT NOT NULL DEFAULT(0),
        CreatedOn DATETIME NOT NULL DEFAULT(GETDATE())
    );

    CREATE INDEX IX_ItemActivityLog_ItemId ON dbo.ItemActivityLog(ItemId, CreatedOn);
    CREATE INDEX IX_ItemActivityLog_UserCounter ON dbo.ItemActivityLog(UserId, CounterId, CreatedOn);
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.ItemActivityLog', 'ActivityDetails') IS NOT NULL
        ALTER TABLE dbo.ItemActivityLog ALTER COLUMN ActivityDetails NVARCHAR(2000) NULL;

    IF COL_LENGTH('dbo.ItemActivityLog', 'UnitCost') IS NULL
        ALTER TABLE dbo.ItemActivityLog ADD UnitCost DECIMAL(18,4) NULL;

    IF COL_LENGTH('dbo.ItemActivityLog', 'RetailPrice') IS NULL
        ALTER TABLE dbo.ItemActivityLog ADD RetailPrice DECIMAL(18,4) NULL;

    IF COL_LENGTH('dbo.ItemActivityLog', 'WalkinPrice') IS NULL
        ALTER TABLE dbo.ItemActivityLog ADD WalkinPrice DECIMAL(18,4) NULL;

    IF COL_LENGTH('dbo.ItemActivityLog', 'CompanyId') IS NULL
        ALTER TABLE dbo.ItemActivityLog ADD CompanyId INT NOT NULL CONSTRAINT DF_ItemActivityLog_CompanyId DEFAULT(0);

    IF COL_LENGTH('dbo.ItemActivityLog', 'BranchId') IS NULL
        ALTER TABLE dbo.ItemActivityLog ADD BranchId INT NOT NULL CONSTRAINT DF_ItemActivityLog_BranchId DEFAULT(0);

    IF COL_LENGTH('dbo.ItemActivityLog', 'FinYearId') IS NULL
        ALTER TABLE dbo.ItemActivityLog ADD FinYearId INT NOT NULL CONSTRAINT DF_ItemActivityLog_FinYearId DEFAULT(0);

    IF COL_LENGTH('dbo.ItemActivityLog', 'UserId') IS NULL
        ALTER TABLE dbo.ItemActivityLog ADD UserId INT NOT NULL CONSTRAINT DF_ItemActivityLog_UserId DEFAULT(0);

    IF COL_LENGTH('dbo.ItemActivityLog', 'UserName') IS NULL
        ALTER TABLE dbo.ItemActivityLog ADD UserName NVARCHAR(150) NULL;

    IF COL_LENGTH('dbo.ItemActivityLog', 'CounterId') IS NULL
        ALTER TABLE dbo.ItemActivityLog ADD CounterId INT NOT NULL CONSTRAINT DF_ItemActivityLog_CounterId DEFAULT(0);

    IF COL_LENGTH('dbo.ItemActivityLog', 'CounterName') IS NULL
        ALTER TABLE dbo.ItemActivityLog ADD CounterName NVARCHAR(150) NULL;

    IF COL_LENGTH('dbo.ItemActivityLog', 'CounterSessionId') IS NULL
        ALTER TABLE dbo.ItemActivityLog ADD CounterSessionId BIGINT NOT NULL CONSTRAINT DF_ItemActivityLog_CounterSessionId DEFAULT(0);

    IF COL_LENGTH('dbo.ItemActivityLog', 'Quantity') IS NULL
        ALTER TABLE dbo.ItemActivityLog ADD Quantity DECIMAL(18,4) NULL;

    IF COL_LENGTH('dbo.ItemActivityLog', 'Available') IS NULL
        ALTER TABLE dbo.ItemActivityLog ADD Available DECIMAL(18,4) NULL;

    IF COL_LENGTH('dbo.ItemActivityLog', 'OnHold') IS NULL
        ALTER TABLE dbo.ItemActivityLog ADD OnHold DECIMAL(18,4) NULL;

    IF COL_LENGTH('dbo.ItemActivityLog', 'Reorder') IS NULL
        ALTER TABLE dbo.ItemActivityLog ADD Reorder DECIMAL(18,4) NULL;

    IF COL_LENGTH('dbo.ItemActivityLog', 'OrderCycleDays') IS NULL
        ALTER TABLE dbo.ItemActivityLog ADD OrderCycleDays INT NULL;

    IF COL_LENGTH('dbo.ItemActivityLog', 'BoxQty') IS NULL
        ALTER TABLE dbo.ItemActivityLog ADD BoxQty INT NULL;

    IF COL_LENGTH('dbo.ItemActivityLog', 'ItemType') IS NULL
        ALTER TABLE dbo.ItemActivityLog ADD ItemType NVARCHAR(100) NULL;

    IF COL_LENGTH('dbo.ItemActivityLog', 'Category') IS NULL
        ALTER TABLE dbo.ItemActivityLog ADD Category NVARCHAR(100) NULL;

    IF COL_LENGTH('dbo.ItemActivityLog', 'ItemGroup') IS NULL
        ALTER TABLE dbo.ItemActivityLog ADD ItemGroup NVARCHAR(100) NULL;

    IF COL_LENGTH('dbo.ItemActivityLog', 'HSN') IS NULL
        ALTER TABLE dbo.ItemActivityLog ADD HSN NVARCHAR(50) NULL;

    IF COL_LENGTH('dbo.ItemActivityLog', 'ItemStatus') IS NULL
        ALTER TABLE dbo.ItemActivityLog ADD ItemStatus NVARCHAR(50) NULL;
END;", (SqlConnection)DataConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private void EnsureItemActivityLogStoredProcedure()
        {
            using (SqlCommand cmd = new SqlCommand(@"
IF OBJECT_ID(N'dbo.POS_ItemActivityLog', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo.POS_ItemActivityLog AS BEGIN SET NOCOUNT ON; END');", (SqlConnection)DataConnection))
            {
                cmd.ExecuteNonQuery();
            }

            using (SqlCommand cmd = new SqlCommand(@"
ALTER PROCEDURE dbo.POS_ItemActivityLog
    @_Operation NVARCHAR(30),
    @ItemId INT = 0,
    @ItemNo NVARCHAR(50) = NULL,
    @ItemName NVARCHAR(250) = NULL,
    @Barcode NVARCHAR(100) = NULL,
    @ActivityType NVARCHAR(50) = NULL,
    @ActivityDetails NVARCHAR(2000) = NULL,
    @UnitCost DECIMAL(18,4) = NULL,
    @RetailPrice DECIMAL(18,4) = NULL,
    @WalkinPrice DECIMAL(18,4) = NULL,
    @Quantity DECIMAL(18,4) = NULL,
    @Available DECIMAL(18,4) = NULL,
    @OnHold DECIMAL(18,4) = NULL,
    @Reorder DECIMAL(18,4) = NULL,
    @OrderCycleDays INT = NULL,
    @BoxQty INT = NULL,
    @ItemType NVARCHAR(100) = NULL,
    @Category NVARCHAR(100) = NULL,
    @ItemGroup NVARCHAR(100) = NULL,
    @HSN NVARCHAR(50) = NULL,
    @ItemStatus NVARCHAR(50) = NULL,
    @CompanyId INT = 0,
    @BranchId INT = 0,
    @FinYearId INT = 0,
    @UserId INT = 0,
    @UserName NVARCHAR(150) = NULL,
    @CounterId INT = 0,
    @CounterName NVARCHAR(150) = NULL,
    @CounterSessionId BIGINT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF @_Operation = N'SAVE'
    BEGIN
        INSERT INTO dbo.ItemActivityLog
        (
            ItemId, ItemNo, ItemName, Barcode, ActivityType, ActivityDetails,
            UnitCost, RetailPrice, WalkinPrice,
            Quantity, Available, OnHold, Reorder, OrderCycleDays, BoxQty,
            ItemType, Category, ItemGroup, HSN, ItemStatus,
            CompanyId, BranchId, FinYearId, UserId, UserName,
            CounterId, CounterName, CounterSessionId, CreatedOn
        )
        VALUES
        (
            @ItemId, @ItemNo, @ItemName, @Barcode, @ActivityType, @ActivityDetails,
            @UnitCost, @RetailPrice, @WalkinPrice,
            @Quantity, @Available, @OnHold, @Reorder, @OrderCycleDays, @BoxQty,
            @ItemType, @Category, @ItemGroup, @HSN, @ItemStatus,
            @CompanyId, @BranchId, @FinYearId, @UserId, @UserName,
            @CounterId, @CounterName, @CounterSessionId, GETDATE()
        );
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

        private static int GetUserId()
        {
            return SessionContext.UserId > 0 ? SessionContext.UserId : ParseInt(DataBase.UserId);
        }

        private static string GetUserName()
        {
            return !string.IsNullOrWhiteSpace(SessionContext.UserName) ? SessionContext.UserName : DataBase.UserName;
        }

        private static int ParseInt(string value)
        {
            int parsed;
            return int.TryParse(value, out parsed) ? parsed : 0;
        }
    }
}
