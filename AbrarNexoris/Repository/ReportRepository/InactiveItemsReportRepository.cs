using Dapper;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Repository.ReportRepository
{
    public class InactiveItemsReportRepository : BaseRepostitory
    {
        public List<InactiveItemsReportRow> GetInactiveItemsReport(InactiveItemsReportFilter filter)
        {
            if (filter == null)
            {
                filter = new InactiveItemsReportFilter();
            }

            List<InactiveItemsReportRow> list = new List<InactiveItemsReportRow>();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                // Ensure POS_ItemMasterStatusRules table exists before we run the SP
                EnsureStatusRulesTable((SqlConnection)DataConnection);

                // Always re-deploy the latest SP version
                EnsureInactiveItemsReportProcedure();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@CompanyId", filter.CompanyId);
                parameters.Add("@BranchId", filter.BranchId);
                parameters.Add("@CategoryId", filter.CategoryId);
                parameters.Add("@GroupId", filter.GroupId);
                parameters.Add("@DateFilterMode", filter.DateFilterMode ?? "ALL");
                parameters.Add("@FromDate", filter.FromDate?.Date);
                parameters.Add("@ToDate", filter.ToDate?.Date);

                list = DataConnection.Query<InactiveItemsReportRow>(
                    STOREDPROCEDURE.POS_InactiveItemsReport,
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting inactive items report: {ex.Message}");
                throw; // Re-throw so the UI MessageBox shows the real error
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return list;
        }

        /// <summary>
        /// Creates POS_ItemMasterStatusRules table if it doesn't exist.
        /// Called before the SP so the SP can safely reference it.
        /// </summary>
        private static void EnsureStatusRulesTable(SqlConnection conn)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(@"
IF OBJECT_ID(N'dbo.POS_ItemMasterStatusRules', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.POS_ItemMasterStatusRules
    (
        RuleId        INT IDENTITY(1,1) PRIMARY KEY,
        ItemId        INT NOT NULL UNIQUE,
        CompanyId     INT NULL DEFAULT 0,
        BranchId      INT NULL DEFAULT 0,
        StatusName    NVARCHAR(100) NOT NULL DEFAULT N'Active',
        StatusReason  NVARCHAR(500) NULL,
        StatusDate    DATETIME NULL DEFAULT GETDATE(),
        BlockSale     BIT NOT NULL DEFAULT 0,
        BlockPurchase BIT NOT NULL DEFAULT 0,
        CreatedOn     DATETIME NULL DEFAULT GETDATE(),
        UpdatedOn     DATETIME NULL DEFAULT GETDATE()
    );
END", conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnsureStatusRulesTable warning: {ex.Message}");
            }
        }

        private class MasterCandidate
        {
            public string Table { get; set; }
            public string Key { get; set; }
            public string Name { get; set; }

            public MasterCandidate(string table, string key, string name)
            {
                Table = table;
                Key = key;
                Name = name;
            }
        }

        private struct TableResolveResult
        {
            public string TableName;
            public string KeyColumn;
            public string NameColumn;
            public bool Exists;
        }

        private static TableResolveResult ResolveMasterTable(
            SqlConnection conn,
            params MasterCandidate[] candidates)
        {
            foreach (var c in candidates)
            {
                string rawName = c.Table.Replace("dbo.", "").Trim('[', ']');

                using (SqlCommand cmd = new SqlCommand($"SELECT OBJECT_ID(N'dbo.[{rawName}]', N'U')", conn))
                {
                    object res = cmd.ExecuteScalar();
                    if (res != null && res != DBNull.Value)
                    {
                        string actualKey = c.Key;
                        using (SqlCommand kCmd = new SqlCommand($"SELECT COL_LENGTH(N'dbo.[{rawName}]', N'{c.Key}')", conn))
                        {
                            object kRes = kCmd.ExecuteScalar();
                            if (kRes == null || kRes == DBNull.Value)
                            {
                                string altKey = c.Key.Equals("Id", StringComparison.OrdinalIgnoreCase) ? rawName + "Id" : "Id";
                                using (SqlCommand altCmd = new SqlCommand($"SELECT COL_LENGTH(N'dbo.[{rawName}]', N'{altKey}')", conn))
                                {
                                    if (altCmd.ExecuteScalar() != null && altCmd.ExecuteScalar() != DBNull.Value)
                                    {
                                        actualKey = altKey;
                                    }
                                }
                            }
                        }

                        string actualName = c.Name;
                        using (SqlCommand nCmd = new SqlCommand($"SELECT COL_LENGTH(N'dbo.[{rawName}]', N'{c.Name}')", conn))
                        {
                            object nRes = nCmd.ExecuteScalar();
                            if (nRes == null || nRes == DBNull.Value)
                            {
                                string altName = c.Name.Equals("Name", StringComparison.OrdinalIgnoreCase) ? rawName + "Name" : "Name";
                                using (SqlCommand altCmd = new SqlCommand($"SELECT COL_LENGTH(N'dbo.[{rawName}]', N'{altName}')", conn))
                                {
                                    if (altCmd.ExecuteScalar() != null && altCmd.ExecuteScalar() != DBNull.Value)
                                    {
                                        actualName = altName;
                                    }
                                }
                            }
                        }

                        return new TableResolveResult
                        {
                            TableName = $"dbo.[{rawName}]",
                            KeyColumn = actualKey,
                            NameColumn = actualName,
                            Exists = true
                        };
                    }
                }
            }

            return new TableResolveResult { Exists = false };
        }

        private void EnsureInactiveItemsReportProcedure()
        {
            SqlConnection conn = (SqlConnection)DataConnection;

            // Step 1: Create stub SP if not exists
            using (SqlCommand cmd = new SqlCommand(@"
IF OBJECT_ID(N'dbo.POS_InactiveItemsReport', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo.POS_InactiveItemsReport AS BEGIN SET NOCOUNT ON; END');", conn))
            {
                cmd.ExecuteNonQuery();
            }

            // Resolve Master Tables dynamically to support all database schema variants
            var catRes = ResolveMasterTable(conn,
                new MasterCandidate("Category", "Id", "CategoryName"),
                new MasterCandidate("Category", "CategoryId", "CategoryName"),
                new MasterCandidate("CategoryMaster", "CategoryId", "CategoryName"),
                new MasterCandidate("CategoryMaster", "Id", "CategoryName"),
                new MasterCandidate("Categories", "Id", "CategoryName"));

            var grpRes = ResolveMasterTable(conn,
                new MasterCandidate("Group", "Id", "GroupName"),
                new MasterCandidate("Group", "GroupId", "GroupName"),
                new MasterCandidate("GroupMaster", "GroupId", "GroupName"),
                new MasterCandidate("GroupMaster", "Id", "GroupName"),
                new MasterCandidate("Groups", "Id", "GroupName"));

            var brdRes = ResolveMasterTable(conn,
                new MasterCandidate("Brand", "Id", "BrandName"),
                new MasterCandidate("Brands", "Id", "BrandName"),
                new MasterCandidate("BrandMaster", "BrandId", "BrandName"),
                new MasterCandidate("BrandMaster", "Id", "BrandName"));

            var untRes = ResolveMasterTable(conn,
                new MasterCandidate("UnitMaster", "UnitId", "UnitName"),
                new MasterCandidate("UnitMaster", "UnitID", "UnitName"),
                new MasterCandidate("UnitMaster", "Id", "UnitName"),
                new MasterCandidate("Unit", "Id", "UnitName"),
                new MasterCandidate("Units", "Id", "UnitName"));

            var brnRes = ResolveMasterTable(conn,
                new MasterCandidate("Branches", "Id", "BranchName"),
                new MasterCandidate("Branches", "BranchId", "BranchName"),
                new MasterCandidate("BranchMaster", "BranchId", "BranchName"),
                new MasterCandidate("BranchMaster", "Id", "BranchName"),
                new MasterCandidate("Branch", "Id", "BranchName"));

            string catJoin = catRes.Exists
                ? $"LEFT JOIN {catRes.TableName} c WITH (NOLOCK) ON im.CategoryId = c.[{catRes.KeyColumn}]"
                : "LEFT JOIN (SELECT CAST(NULL AS INT) AS CatId, CAST('' AS NVARCHAR(100)) AS CategoryName) c ON 1=0";
            string catSelect = catRes.Exists
                ? $"ISNULL(c.[{catRes.NameColumn}], '')"
                : "ISNULL(c.CategoryName, '')";

            string grpJoin = grpRes.Exists
                ? $"LEFT JOIN {grpRes.TableName} g WITH (NOLOCK) ON im.GroupId = g.[{grpRes.KeyColumn}]"
                : "LEFT JOIN (SELECT CAST(NULL AS INT) AS GrpId, CAST('' AS NVARCHAR(100)) AS GroupName) g ON 1=0";
            string grpSelect = grpRes.Exists
                ? $"ISNULL(g.[{grpRes.NameColumn}], '')"
                : "ISNULL(g.GroupName, '')";

            string brdJoin = brdRes.Exists
                ? $"LEFT JOIN {brdRes.TableName} b WITH (NOLOCK) ON im.BrandId = b.[{brdRes.KeyColumn}]"
                : "LEFT JOIN (SELECT CAST(NULL AS INT) AS BrdId, CAST('' AS NVARCHAR(100)) AS BrandName) b ON 1=0";
            string brdSelect = brdRes.Exists
                ? $"ISNULL(b.[{brdRes.NameColumn}], '')"
                : "ISNULL(b.BrandName, '')";

            string untJoin = untRes.Exists
                ? $"LEFT JOIN {untRes.TableName} u WITH (NOLOCK) ON im.BaseUnitId = u.[{untRes.KeyColumn}]"
                : "LEFT JOIN (SELECT CAST(NULL AS INT) AS UntId, CAST('' AS NVARCHAR(100)) AS UnitName) u ON 1=0";
            string untSelect = untRes.Exists
                ? $"ISNULL(u.[{untRes.NameColumn}], '')"
                : "ISNULL(u.UnitName, '')";

            string brnJoin = brnRes.Exists
                ? $"LEFT JOIN {brnRes.TableName} br WITH (NOLOCK) ON im.BranchId = br.[{brnRes.KeyColumn}]"
                : "LEFT JOIN (SELECT CAST(NULL AS INT) AS BrnId, CAST('' AS NVARCHAR(100)) AS BranchName) br ON 1=0";
            string brnSelect = brnRes.Exists
                ? $"ISNULL(br.[{brnRes.NameColumn}], '')"
                : "ISNULL(br.BranchName, '')";

            string barcodeSelect = "ISNULL(im.Barcode, '')";
            using (SqlCommand colCmd = new SqlCommand("SELECT COL_LENGTH(N'dbo.ItemMaster', N'Barcode')", conn))
            {
                if (colCmd.ExecuteScalar() == DBNull.Value || colCmd.ExecuteScalar() == null)
                {
                    using (SqlCommand altCmd = new SqlCommand("SELECT COL_LENGTH(N'dbo.ItemMaster', N'BarCode')", conn))
                    {
                        if (altCmd.ExecuteScalar() != null && altCmd.ExecuteScalar() != DBNull.Value)
                        {
                            barcodeSelect = "ISNULL(im.BarCode, '')";
                        }
                        else
                        {
                            barcodeSelect = "''";
                        }
                    }
                }
            }

            bool hasStatus = false;
            using (SqlCommand colCmd = new SqlCommand("SELECT COL_LENGTH(N'dbo.ItemMaster', N'Status')", conn))
            {
                object res = colCmd.ExecuteScalar();
                hasStatus = (res != null && res != DBNull.Value);
            }

            bool hasIsActive = false;
            using (SqlCommand colCmd = new SqlCommand("SELECT COL_LENGTH(N'dbo.ItemMaster', N'IsActive')", conn))
            {
                object res = colCmd.ExecuteScalar();
                hasIsActive = (res != null && res != DBNull.Value);
            }

            string itemStatusWhere = "";
            if (hasStatus && hasIsActive)
            {
                itemStatusWhere = @"(
          UPPER(LTRIM(RTRIM(ISNULL(sr.StatusName, '')))) = 'INACTIVE'
       OR UPPER(LTRIM(RTRIM(ISNULL(im.Status, ''))))    = 'INACTIVE'
       OR ISNULL(im.IsActive, 1) = 0
    )";
            }
            else if (hasStatus)
            {
                itemStatusWhere = @"(
          UPPER(LTRIM(RTRIM(ISNULL(sr.StatusName, '')))) = 'INACTIVE'
       OR UPPER(LTRIM(RTRIM(ISNULL(im.Status, ''))))    = 'INACTIVE'
    )";
            }
            else if (hasIsActive)
            {
                itemStatusWhere = @"(
          UPPER(LTRIM(RTRIM(ISNULL(sr.StatusName, '')))) = 'INACTIVE'
       OR ISNULL(im.IsActive, 1) = 0
    )";
            }
            else
            {
                itemStatusWhere = @"UPPER(LTRIM(RTRIM(ISNULL(sr.StatusName, '')))) = 'INACTIVE'";
            }

            string spSql = $@"
ALTER PROCEDURE dbo.POS_InactiveItemsReport
    @CompanyId      INT          = 0,
    @BranchId       INT          = 0,
    @CategoryId     INT          = 0,
    @GroupId        INT          = 0,
    @DateFilterMode NVARCHAR(50) = N'ALL',
    @FromDate       DATE         = NULL,
    @ToDate         DATE         = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- ----------------------------------------------------------------
    -- Step 1: Pull all inactive items into a temp table
    -- ----------------------------------------------------------------
    SELECT
        im.ItemId,
        ISNULL(im.ItemNo, '')          AS ItemNo,
        {barcodeSelect}                AS Barcode,
        ISNULL(im.Description, '')     AS ItemName,
        {untSelect}                    AS Unit,
        {catSelect}                    AS CategoryName,
        {grpSelect}                    AS GroupName,
        {brdSelect}                    AS BrandName,
        ISNULL(im.HSNCode,   '')       AS HSNCode,
        ISNULL(ps.Stock,    0)         AS Stock,
        ISNULL(ps.UnitCost, 0)         AS UnitCost,
        ISNULL(ps.RetailPrice, 0)      AS RetailPrice,
        ISNULL(ps.WalkinPrice, 0)      AS WalkinPrice,
        -- StatusName: prefer POS_ItemMasterStatusRules, fall back to ItemMaster flags
        ISNULL(sr.StatusName, 'Inactive') AS ItemStatus,
        ISNULL(sr.StatusReason, '')    AS StatusReason,
        sr.StatusDate                  AS StatusDate,
        {brnSelect}                    AS BranchName,
        im.BranchId,
        -- Log columns filled in Step 2
        CAST(NULL AS NVARCHAR(150))    AS InactivatedByUser,
        CAST(NULL AS INT)              AS UserId,
        CAST(NULL AS NVARCHAR(150))    AS CounterName,
        CAST(NULL AS INT)              AS CounterId,
        CAST(NULL AS DATETIME)         AS CreatedOn
    INTO #InactiveItems
    FROM dbo.ItemMaster im WITH (NOLOCK)
    LEFT JOIN dbo.POS_ItemMasterStatusRules sr WITH (NOLOCK)
           ON im.ItemId = sr.ItemId
    {catJoin}
    {grpJoin}
    {brdJoin}
    {untJoin}
    {brnJoin}
    LEFT JOIN (
        -- PriceSettings: aggregate per item (handles both base-unit and non-base-unit DBs)
        SELECT
            ItemId,
            SUM(Stock)          AS Stock,
            MAX(Cost)           AS UnitCost,
            MAX(WholeSalePrice) AS RetailPrice,
            MAX(RetailPrice)    AS WalkinPrice
        FROM dbo.PriceSettings WITH (NOLOCK)
        GROUP BY ItemId
    ) ps ON im.ItemId = ps.ItemId
    WHERE {itemStatusWhere}
      AND (@BranchId   <= 0 OR im.BranchId  IS NULL OR im.BranchId  = 0 OR im.BranchId  = @BranchId)
      AND (@CompanyId  <= 0 OR im.CompanyId IS NULL OR im.CompanyId = 0 OR im.CompanyId = @CompanyId)
      AND (@CategoryId <= 0 OR im.CategoryId = @CategoryId)
      AND (@GroupId    <= 0 OR im.GroupId    = @GroupId);

    -- ----------------------------------------------------------------
    -- Step 2: Enrich with ItemActivityLog data (safe: table may not exist)
    -- ----------------------------------------------------------------
    IF OBJECT_ID(N'dbo.ItemActivityLog', N'U') IS NOT NULL
    BEGIN
        IF COL_LENGTH('dbo.ItemActivityLog', 'ItemStatus') IS NOT NULL
        BEGIN
            -- ItemStatus column exists: filter by it
            UPDATE ii
            SET ii.InactivatedByUser = lg.UserName,
                ii.UserId            = lg.UserId,
                ii.CounterName       = lg.CounterName,
                ii.CounterId         = lg.CounterId,
                ii.CreatedOn         = lg.CreatedOn
            FROM #InactiveItems ii
            INNER JOIN (
                SELECT ItemId, UserName, UserId, CounterName, CounterId, CreatedOn
                FROM (
                    SELECT ItemId, UserName, UserId, CounterName, CounterId, CreatedOn,
                           ROW_NUMBER() OVER (PARTITION BY ItemId ORDER BY ItemActivityLogId DESC) AS rn
                    FROM dbo.ItemActivityLog WITH (NOLOCK)
                    WHERE UPPER(ISNULL(ItemStatus, '')) = 'INACTIVE'
                       OR ActivityDetails LIKE '%Inactive%'
                       OR ActivityDetails LIKE '%Status%'
                ) sub WHERE rn = 1
            ) lg ON ii.ItemId = lg.ItemId;
        END
        ELSE
        BEGIN
            -- ItemStatus column missing: filter by ActivityDetails only
            UPDATE ii
            SET ii.InactivatedByUser = lg.UserName,
                ii.UserId            = lg.UserId,
                ii.CounterName       = lg.CounterName,
                ii.CounterId         = lg.CounterId,
                ii.CreatedOn         = lg.CreatedOn
            FROM #InactiveItems ii
            INNER JOIN (
                SELECT ItemId, UserName, UserId, CounterName, CounterId, CreatedOn
                FROM (
                    SELECT ItemId, UserName, UserId, CounterName, CounterId, CreatedOn,
                           ROW_NUMBER() OVER (PARTITION BY ItemId ORDER BY ItemActivityLogId DESC) AS rn
                    FROM dbo.ItemActivityLog WITH (NOLOCK)
                    WHERE ActivityDetails LIKE '%Inactive%'
                       OR ActivityDetails LIKE '%Status%'
                ) sub WHERE rn = 1
            ) lg ON ii.ItemId = lg.ItemId;
        END
    END

    -- ----------------------------------------------------------------
    -- Step 3: Apply date-range filter (only when RANGE mode selected)
    -- ----------------------------------------------------------------
    IF @DateFilterMode = 'RANGE' AND @FromDate IS NOT NULL AND @ToDate IS NOT NULL
    BEGIN
        DELETE FROM #InactiveItems
        WHERE NOT (
            (StatusDate >= @FromDate AND StatusDate < DATEADD(DAY, 1, @ToDate))
            OR (CreatedOn  >= @FromDate AND CreatedOn  < DATEADD(DAY, 1, @ToDate))
        );
    END

    -- ----------------------------------------------------------------
    -- Step 4: Return results
    -- ----------------------------------------------------------------
    SELECT
        ItemId,
        ItemNo,
        Barcode,
        ItemName,
        Unit,
        CategoryName,
        GroupName,
        BrandName,
        HSNCode,
        Stock,
        UnitCost,
        RetailPrice,
        WalkinPrice,
        ItemStatus,
        StatusReason,
        StatusDate,
        ISNULL(InactivatedByUser, 'System') AS InactivatedByUser,
        ISNULL(UserId, 0)                   AS UserId,
        ISNULL(CounterName, '')             AS CounterName,
        ISNULL(CounterId, 0)                AS CounterId,
        BranchName,
        BranchId,
        CreatedOn
    FROM #InactiveItems
    ORDER BY ISNULL(StatusDate, ISNULL(CreatedOn, GETDATE())) DESC, ItemName ASC;

    IF OBJECT_ID(N'tempdb..#InactiveItems') IS NOT NULL
        DROP TABLE #InactiveItems;
END";

            using (SqlCommand cmd = new SqlCommand(spSql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}

