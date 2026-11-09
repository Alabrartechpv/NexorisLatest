using ModelClass;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Repository.ReportRepository
{
    public class DatewiseStockAdjustmentReportRepository : BaseRepostitory
    {
        private static bool _procedureEnsured = false;
        private static readonly object _procedureLock = new object();

        public DatewiseStockAdjustmentReportRepository()
        {
            EnsureStoredProcedure();
        }

        /// <summary>
        /// Automatically ensures that the POS_DatewiseStockAdjustmentReport stored procedure exists and is up to date in the database.
        /// </summary>
        public void EnsureStoredProcedure()
        {
            if (_procedureEnsured) return;

            lock (_procedureLock)
            {
                if (_procedureEnsured) return;

                try
                {
                    bool wasClosed = DataConnection.State == ConnectionState.Closed;
                    if (wasClosed) DataConnection.Open();

                    string ddl = @"
IF OBJECT_ID(N'[dbo].[POS_DatewiseStockAdjustmentReport]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[POS_DatewiseStockAdjustmentReport];

EXEC dbo.sp_executesql @statement = N'
CREATE PROCEDURE [dbo].[POS_DatewiseStockAdjustmentReport]
    @FromDate        DATE          = NULL,
    @ToDate          DATE          = NULL,
    @PeriodMode      VARCHAR(20)   = ''ALL'',
    @Month           INT           = 0,
    @Year            INT           = 0,
    @CompanyId       INT           = 0,
    @BranchId        INT           = 0,
    @FinYearId       INT           = 0,
    @AdjustmentType  VARCHAR(30)   = ''ALL'',
    @ItemSearch      NVARCHAR(200) = NULL,
    @ViewMode        VARCHAR(30)   = ''DETAILED''
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Mode VARCHAR(20) = UPPER(LTRIM(RTRIM(ISNULL(@PeriodMode, ''ALL''))));
    DECLARE @View VARCHAR(30) = UPPER(LTRIM(RTRIM(ISNULL(@ViewMode, ''DETAILED''))));
    DECLARE @AdjType VARCHAR(30) = UPPER(LTRIM(RTRIM(ISNULL(@AdjustmentType, ''ALL''))));
    DECLARE @CleanSearch NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@ItemSearch)), '''');

    DECLARE @StartDate DATE = @FromDate;
    DECLARE @EndDate DATE = @ToDate;

    IF @Mode = ''MONTH_WISE''
    BEGIN
        IF @Year > 0 AND @Month > 0
        BEGIN
            SET @StartDate = DATEFROMPARTS(@Year, @Month, 1);
            SET @EndDate = EOMONTH(@StartDate);
        END
        ELSE IF @Year > 0 AND (@Month = 0 OR @Month IS NULL)
        BEGIN
            SET @StartDate = DATEFROMPARTS(@Year, 1, 1);
            SET @EndDate = DATEFROMPARTS(@Year, 12, 31);
        END
    END
    ELSE IF @Mode = ''YEAR_WISE''
    BEGIN
        IF @Year > 0
        BEGIN
            SET @StartDate = DATEFROMPARTS(@Year, 1, 1);
            SET @EndDate = DATEFROMPARTS(@Year, 12, 31);
        END
    END

    IF @View = ''DETAILED''
    BEGIN
        SELECT 
            ISNULL(sam.StockAdjustmentDate, GETDATE()) AS AdjustmentDate,
            ISNULL(sam.StockAdjustmentNo, 0) AS AdjustmentNo,
            sam.Id AS StockAdjustmentMasterId,
            ISNULL(sad.SlNo, 1) AS SlNo,
            ISNULL(sad.ItemId, 0) AS ItemID,
            ISNULL(im.BarCode, '''') AS BarCode,
            ISNULL(im.Description, ''Unknown Item'') AS ItemName,
            ISNULL(um.UnitName, '''') AS Unit,
            CASE WHEN ISNULL(sad.QtyDifference, 0) >= 0 THEN ''Stock IN'' ELSE ''Stock OUT'' END AS AdjustmentType,
            CAST(ISNULL(sad.SystemStock, 0) AS DECIMAL(18,2)) AS SystemStock,
            CAST(ISNULL(sad.PhysicalStock, 0) AS DECIMAL(18,2)) AS PhysicalStock,
            CAST(ISNULL(sad.QtyDifference, 0) AS DECIMAL(18,2)) AS QtyDifference,
            CAST(ABS(ISNULL(sad.QtyDifference, 0)) AS DECIMAL(18,2)) AS Qty,
            CAST(CASE WHEN ISNULL(sad.QtyDifference, 0) > 0 THEN sad.QtyDifference ELSE 0 END AS DECIMAL(18,2)) AS StockInQty,
            CAST(CASE WHEN ISNULL(sad.QtyDifference, 0) < 0 THEN ABS(sad.QtyDifference) ELSE 0 END AS DECIMAL(18,2)) AS StockOutQty,
            CAST(ISNULL(sad.Cost, 0) AS DECIMAL(18,2)) AS Cost,
            CAST(ISNULL(sad.QtyDifference, 0) * ISNULL(sad.Cost, 0) AS DECIMAL(18,2)) AS Amount,
            CAST(ABS(ISNULL(sad.QtyDifference, 0)) * ISNULL(sad.Cost, 0) AS DECIMAL(18,2)) AS NetAmount,
            ISNULL(sad.Reason, '''') AS Reason,
            ISNULL(sam.Comments, '''') AS Comments,
            ISNULL(lm.LedgerName, '''') AS LedgerName,
            COALESCE(NULLIF(u.UserName, ''''), NULLIF(CONVERT(NVARCHAR(50), sam.UserId), ''0''), ''System'') AS UserName
        FROM dbo.StockAdjustmentMaster sam WITH (NOLOCK)
        INNER JOIN dbo.StockAdjustmentDetails sad WITH (NOLOCK) ON sad.StockAdjustmentMasterId = sam.Id
        LEFT JOIN dbo.ItemMaster im WITH (NOLOCK) ON im.ItemId = sad.ItemId
        LEFT JOIN dbo.UnitMaster um WITH (NOLOCK) ON um.UnitID = sad.UnitId
        LEFT JOIN dbo.LedgerMaster lm WITH (NOLOCK) ON lm.LedgerID = sam.LedgerId
        LEFT JOIN dbo.Users u WITH (NOLOCK) ON u.UserID = sam.UserId
        WHERE ISNULL(sam.CancelFlag, 0) = 0
          AND ISNULL(sad.CancelFlag, 0) = 0
          AND (@CompanyId = 0 OR ISNULL(sam.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(sam.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(sam.FinYearId, 0) = @FinYearId)
          AND (
                @AdjType = ''ALL''
                OR (@AdjType = ''STOCK IN'' AND ISNULL(sad.QtyDifference, 0) >= 0)
                OR (@AdjType = ''STOCK OUT'' AND ISNULL(sad.QtyDifference, 0) < 0)
              )
          AND (
                @CleanSearch IS NULL
                OR im.Description LIKE ''%'' + @CleanSearch + ''%''
                OR im.BarCode LIKE ''%'' + @CleanSearch + ''%''
                OR CONVERT(NVARCHAR(50), sam.StockAdjustmentNo) LIKE ''%'' + @CleanSearch + ''%''
                OR sad.Reason LIKE ''%'' + @CleanSearch + ''%''
                OR sam.Comments LIKE ''%'' + @CleanSearch + ''%''
              )
          AND (
                @Mode = ''ALL''
                OR (@Mode = ''DATE_RANGE'' AND CAST(sam.StockAdjustmentDate AS DATE) >= @StartDate AND CAST(sam.StockAdjustmentDate AS DATE) <= @EndDate)
                OR (@Mode = ''MONTH_WISE'' AND (
                    (@Year = 0 OR YEAR(sam.StockAdjustmentDate) = @Year) AND
                    (@Month = 0 OR MONTH(sam.StockAdjustmentDate) = @Month)
                ))
                OR (@Mode = ''YEAR_WISE'' AND (@Year = 0 OR YEAR(sam.StockAdjustmentDate) = @Year))
              )
        ORDER BY sam.StockAdjustmentDate DESC, sam.StockAdjustmentNo DESC, ISNULL(sad.SlNo, 0) ASC;
    END
    ELSE IF @View = ''DATE_ITEM_SUMMARY'' OR @View = ''DATE_MATRIX''
    BEGIN
        SELECT 
            CAST(sam.StockAdjustmentDate AS DATE) AS AdjustmentDate,
            0 AS AdjustmentNo,
            0 AS StockAdjustmentMasterId,
            ROW_NUMBER() OVER(PARTITION BY CAST(sam.StockAdjustmentDate AS DATE) ORDER BY ISNULL(im.Description, ''Unknown Item'')) AS SlNo,
            ISNULL(sad.ItemId, 0) AS ItemID,
            ISNULL(MAX(im.BarCode), '''') AS BarCode,
            ISNULL(im.Description, ''Unknown Item'') AS ItemName,
            ISNULL(um.UnitName, '''') AS Unit,
            CASE WHEN SUM(ISNULL(sad.QtyDifference, 0)) >= 0 THEN ''Stock IN'' ELSE ''Stock OUT'' END AS AdjustmentType,
            CAST(AVG(ISNULL(sad.SystemStock, 0)) AS DECIMAL(18,2)) AS SystemStock,
            CAST(AVG(ISNULL(sad.PhysicalStock, 0)) AS DECIMAL(18,2)) AS PhysicalStock,
            CAST(SUM(ISNULL(sad.QtyDifference, 0)) AS DECIMAL(18,2)) AS QtyDifference,
            CAST(SUM(ABS(ISNULL(sad.QtyDifference, 0))) AS DECIMAL(18,2)) AS Qty,
            CAST(SUM(CASE WHEN ISNULL(sad.QtyDifference, 0) > 0 THEN sad.QtyDifference ELSE 0 END) AS DECIMAL(18,2)) AS StockInQty,
            CAST(SUM(CASE WHEN ISNULL(sad.QtyDifference, 0) < 0 THEN ABS(sad.QtyDifference) ELSE 0 END) AS DECIMAL(18,2)) AS StockOutQty,
            CAST(CASE WHEN SUM(ABS(ISNULL(sad.QtyDifference, 0))) > 0
                      THEN SUM(ABS(ISNULL(sad.QtyDifference, 0)) * ISNULL(sad.Cost, 0)) / SUM(ABS(ISNULL(sad.QtyDifference, 0)))
                      ELSE MAX(ISNULL(sad.Cost, 0))
                 END AS DECIMAL(18,2)) AS Cost,
            CAST(SUM(ISNULL(sad.QtyDifference, 0) * ISNULL(sad.Cost, 0)) AS DECIMAL(18,2)) AS Amount,
            CAST(SUM(ABS(ISNULL(sad.QtyDifference, 0)) * ISNULL(sad.Cost, 0)) AS DECIMAL(18,2)) AS NetAmount,
            '''' AS Reason,
            ''Daily Summary'' AS Comments,
            '''' AS LedgerName,
            '''' AS UserName
        FROM dbo.StockAdjustmentMaster sam WITH (NOLOCK)
        INNER JOIN dbo.StockAdjustmentDetails sad WITH (NOLOCK) ON sad.StockAdjustmentMasterId = sam.Id
        LEFT JOIN dbo.ItemMaster im WITH (NOLOCK) ON im.ItemId = sad.ItemId
        LEFT JOIN dbo.UnitMaster um WITH (NOLOCK) ON um.UnitID = sad.UnitId
        WHERE ISNULL(sam.CancelFlag, 0) = 0
          AND ISNULL(sad.CancelFlag, 0) = 0
          AND (@CompanyId = 0 OR ISNULL(sam.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(sam.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(sam.FinYearId, 0) = @FinYearId)
          AND (
                @AdjType = ''ALL''
                OR (@AdjType = ''STOCK IN'' AND ISNULL(sad.QtyDifference, 0) >= 0)
                OR (@AdjType = ''STOCK OUT'' AND ISNULL(sad.QtyDifference, 0) < 0)
              )
          AND (
                @CleanSearch IS NULL
                OR im.Description LIKE ''%'' + @CleanSearch + ''%''
                OR im.BarCode LIKE ''%'' + @CleanSearch + ''%''
                OR CONVERT(NVARCHAR(50), sam.StockAdjustmentNo) LIKE ''%'' + @CleanSearch + ''%''
                OR sad.Reason LIKE ''%'' + @CleanSearch + ''%''
                OR sam.Comments LIKE ''%'' + @CleanSearch + ''%''
              )
          AND (
                @Mode = ''ALL''
                OR (@Mode = ''DATE_RANGE'' AND CAST(sam.StockAdjustmentDate AS DATE) >= @StartDate AND CAST(sam.StockAdjustmentDate AS DATE) <= @EndDate)
                OR (@Mode = ''MONTH_WISE'' AND (
                    (@Year = 0 OR YEAR(sam.StockAdjustmentDate) = @Year) AND
                    (@Month = 0 OR MONTH(sam.StockAdjustmentDate) = @Month)
                ))
                OR (@Mode = ''YEAR_WISE'' AND (@Year = 0 OR YEAR(sam.StockAdjustmentDate) = @Year))
              )
        GROUP BY CAST(sam.StockAdjustmentDate AS DATE), sad.ItemId, im.Description, um.UnitName
        ORDER BY CAST(sam.StockAdjustmentDate AS DATE) DESC, ISNULL(im.Description, ''Unknown Item'') ASC;
    END
    ELSE IF @View = ''ITEM_SUMMARY''
    BEGIN
        SELECT 
            MAX(sam.StockAdjustmentDate) AS AdjustmentDate,
            0 AS AdjustmentNo,
            0 AS StockAdjustmentMasterId,
            ROW_NUMBER() OVER(ORDER BY ISNULL(im.Description, ''Unknown Item'')) AS SlNo,
            ISNULL(sad.ItemId, 0) AS ItemID,
            ISNULL(MAX(im.BarCode), '''') AS BarCode,
            ISNULL(im.Description, ''Unknown Item'') AS ItemName,
            ISNULL(um.UnitName, '''') AS Unit,
            CASE WHEN SUM(ISNULL(sad.QtyDifference, 0)) >= 0 THEN ''Stock IN'' ELSE ''Stock OUT'' END AS AdjustmentType,
            CAST(AVG(ISNULL(sad.SystemStock, 0)) AS DECIMAL(18,2)) AS SystemStock,
            CAST(AVG(ISNULL(sad.PhysicalStock, 0)) AS DECIMAL(18,2)) AS PhysicalStock,
            CAST(SUM(ISNULL(sad.QtyDifference, 0)) AS DECIMAL(18,2)) AS QtyDifference,
            CAST(SUM(ABS(ISNULL(sad.QtyDifference, 0))) AS DECIMAL(18,2)) AS Qty,
            CAST(SUM(CASE WHEN ISNULL(sad.QtyDifference, 0) > 0 THEN sad.QtyDifference ELSE 0 END) AS DECIMAL(18,2)) AS StockInQty,
            CAST(SUM(CASE WHEN ISNULL(sad.QtyDifference, 0) < 0 THEN ABS(sad.QtyDifference) ELSE 0 END) AS DECIMAL(18,2)) AS StockOutQty,
            CAST(CASE WHEN SUM(ABS(ISNULL(sad.QtyDifference, 0))) > 0
                      THEN SUM(ABS(ISNULL(sad.QtyDifference, 0)) * ISNULL(sad.Cost, 0)) / SUM(ABS(ISNULL(sad.QtyDifference, 0)))
                      ELSE MAX(ISNULL(sad.Cost, 0))
                 END AS DECIMAL(18,2)) AS Cost,
            CAST(SUM(ISNULL(sad.QtyDifference, 0) * ISNULL(sad.Cost, 0)) AS DECIMAL(18,2)) AS Amount,
            CAST(SUM(ABS(ISNULL(sad.QtyDifference, 0)) * ISNULL(sad.Cost, 0)) AS DECIMAL(18,2)) AS NetAmount,
            '''' AS Reason,
            ''Item Summary'' AS Comments,
            '''' AS LedgerName,
            '''' AS UserName
        FROM dbo.StockAdjustmentMaster sam WITH (NOLOCK)
        INNER JOIN dbo.StockAdjustmentDetails sad WITH (NOLOCK) ON sad.StockAdjustmentMasterId = sam.Id
        LEFT JOIN dbo.ItemMaster im WITH (NOLOCK) ON im.ItemId = sad.ItemId
        LEFT JOIN dbo.UnitMaster um WITH (NOLOCK) ON um.UnitID = sad.UnitId
        WHERE ISNULL(sam.CancelFlag, 0) = 0
          AND ISNULL(sad.CancelFlag, 0) = 0
          AND (@CompanyId = 0 OR ISNULL(sam.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(sam.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(sam.FinYearId, 0) = @FinYearId)
          AND (
                @AdjType = ''ALL''
                OR (@AdjType = ''STOCK IN'' AND ISNULL(sad.QtyDifference, 0) >= 0)
                OR (@AdjType = ''STOCK OUT'' AND ISNULL(sad.QtyDifference, 0) < 0)
              )
          AND (
                @CleanSearch IS NULL
                OR im.Description LIKE ''%'' + @CleanSearch + ''%''
                OR im.BarCode LIKE ''%'' + @CleanSearch + ''%''
                OR CONVERT(NVARCHAR(50), sam.StockAdjustmentNo) LIKE ''%'' + @CleanSearch + ''%''
                OR sad.Reason LIKE ''%'' + @CleanSearch + ''%''
                OR sam.Comments LIKE ''%'' + @CleanSearch + ''%''
              )
          AND (
                @Mode = ''ALL''
                OR (@Mode = ''DATE_RANGE'' AND CAST(sam.StockAdjustmentDate AS DATE) >= @StartDate AND CAST(sam.StockAdjustmentDate AS DATE) <= @EndDate)
                OR (@Mode = ''MONTH_WISE'' AND (
                    (@Year = 0 OR YEAR(sam.StockAdjustmentDate) = @Year) AND
                    (@Month = 0 OR MONTH(sam.StockAdjustmentDate) = @Month)
                ))
                OR (@Mode = ''YEAR_WISE'' AND (@Year = 0 OR YEAR(sam.StockAdjustmentDate) = @Year))
              )
        GROUP BY sad.ItemId, im.Description, um.UnitName
        ORDER BY ISNULL(im.Description, ''Unknown Item'') ASC;
    END
END
';
";
                    using (SqlCommand cmd = new SqlCommand(ddl, (SqlConnection)DataConnection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    if (wasClosed && DataConnection.State == ConnectionState.Open)
                    {
                        DataConnection.Close();
                    }

                    _procedureEnsured = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"EnsureStoredProcedure (StockAdjustment) error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Retrieves the Date-wise stock adjustment report rows based on user filters.
        /// </summary>
        public List<DatewiseStockAdjustmentReportRow> GetReport(DatewiseStockAdjustmentReportFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            List<DatewiseStockAdjustmentReportRow> list = new List<DatewiseStockAdjustmentReportRow>();
            bool wasClosed = DataConnection.State == ConnectionState.Closed;

            try
            {
                if (wasClosed) DataConnection.Open();

                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_DatewiseStockAdjustmentReport, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 180;

                        cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = (object)filter.FromDate.Date ?? DBNull.Value;
                        cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = (object)filter.ToDate.Date ?? DBNull.Value;
                        cmd.Parameters.Add("@PeriodMode", SqlDbType.VarChar, 20).Value = string.IsNullOrWhiteSpace(filter.PeriodMode) ? "ALL" : filter.PeriodMode;
                        cmd.Parameters.Add("@Month", SqlDbType.Int).Value = filter.Month;
                        cmd.Parameters.Add("@Year", SqlDbType.Int).Value = filter.Year;
                        cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = filter.CompanyId > 0 ? filter.CompanyId : SessionContext.CompanyId;
                        cmd.Parameters.Add("@BranchId", SqlDbType.Int).Value = filter.BranchId > 0 ? filter.BranchId : SessionContext.BranchId;
                        cmd.Parameters.Add("@FinYearId", SqlDbType.Int).Value = filter.FinYearId > 0 ? filter.FinYearId : SessionContext.FinYearId;
                        cmd.Parameters.Add("@AdjustmentType", SqlDbType.VarChar, 30).Value = string.IsNullOrWhiteSpace(filter.AdjustmentType) ? "ALL" : filter.AdjustmentType;
                        cmd.Parameters.Add("@ItemSearch", SqlDbType.NVarChar, 200).Value = string.IsNullOrWhiteSpace(filter.ItemSearch) ? (object)DBNull.Value : filter.ItemSearch.Trim();
                        cmd.Parameters.Add("@ViewMode", SqlDbType.VarChar, 30).Value = string.IsNullOrWhiteSpace(filter.ViewMode) ? "DETAILED" : filter.ViewMode;

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable table = new DataTable();
                            adapter.Fill(table);
                            list = MapDataTableToReportRows(table);
                            return list;
                        }
                    }
                }
                catch (SqlException)
                {
                    // Fallback to inline query if stored procedure execution fails
                    list = ExecuteFallbackQuery(filter);
                }
            }
            finally
            {
                if (wasClosed && DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return list;
        }

        private List<DatewiseStockAdjustmentReportRow> ExecuteFallbackQuery(DatewiseStockAdjustmentReportFilter filter)
        {
            List<DatewiseStockAdjustmentReportRow> list = new List<DatewiseStockAdjustmentReportRow>();
            try
            {
                string sql = @"
SELECT 
    ISNULL(sam.StockAdjustmentDate, GETDATE()) AS AdjustmentDate,
    ISNULL(sam.StockAdjustmentNo, 0) AS AdjustmentNo,
    sam.Id AS StockAdjustmentMasterId,
    ISNULL(sad.SlNo, 1) AS SlNo,
    ISNULL(sad.ItemId, 0) AS ItemID,
    ISNULL(im.BarCode, '') AS BarCode,
    ISNULL(im.Description, 'Unknown Item') AS ItemName,
    ISNULL(um.UnitName, '') AS Unit,
    CASE WHEN ISNULL(sad.QtyDifference, 0) >= 0 THEN 'Stock IN' ELSE 'Stock OUT' END AS AdjustmentType,
    CAST(ISNULL(sad.SystemStock, 0) AS DECIMAL(18,2)) AS SystemStock,
    CAST(ISNULL(sad.PhysicalStock, 0) AS DECIMAL(18,2)) AS PhysicalStock,
    CAST(ISNULL(sad.QtyDifference, 0) AS DECIMAL(18,2)) AS QtyDifference,
    CAST(ABS(ISNULL(sad.QtyDifference, 0)) AS DECIMAL(18,2)) AS Qty,
    CAST(CASE WHEN ISNULL(sad.QtyDifference, 0) > 0 THEN sad.QtyDifference ELSE 0 END AS DECIMAL(18,2)) AS StockInQty,
    CAST(CASE WHEN ISNULL(sad.QtyDifference, 0) < 0 THEN ABS(sad.QtyDifference) ELSE 0 END AS DECIMAL(18,2)) AS StockOutQty,
    CAST(ISNULL(sad.Cost, 0) AS DECIMAL(18,2)) AS Cost,
    CAST(ISNULL(sad.QtyDifference, 0) * ISNULL(sad.Cost, 0) AS DECIMAL(18,2)) AS Amount,
    CAST(ABS(ISNULL(sad.QtyDifference, 0)) * ISNULL(sad.Cost, 0) AS DECIMAL(18,2)) AS NetAmount,
    ISNULL(sad.Reason, '') AS Reason,
    ISNULL(sam.Comments, '') AS Comments,
    ISNULL(lm.LedgerName, '') AS LedgerName,
    COALESCE(NULLIF(u.UserName, ''), NULLIF(CONVERT(NVARCHAR(50), sam.UserId), '0'), 'System') AS UserName
FROM dbo.StockAdjustmentMaster sam WITH (NOLOCK)
INNER JOIN dbo.StockAdjustmentDetails sad WITH (NOLOCK) ON sad.StockAdjustmentMasterId = sam.Id
LEFT JOIN dbo.ItemMaster im WITH (NOLOCK) ON im.ItemId = sad.ItemId
LEFT JOIN dbo.UnitMaster um WITH (NOLOCK) ON um.UnitID = sad.UnitId
LEFT JOIN dbo.LedgerMaster lm WITH (NOLOCK) ON lm.LedgerID = sam.LedgerId
LEFT JOIN dbo.Users u WITH (NOLOCK) ON u.UserID = sam.UserId
WHERE ISNULL(sam.CancelFlag, 0) = 0
  AND ISNULL(sad.CancelFlag, 0) = 0
  AND (@CompanyId = 0 OR ISNULL(sam.CompanyId, 0) = @CompanyId)
  AND (@BranchId = 0 OR ISNULL(sam.BranchId, 0) = @BranchId)
  AND (@FinYearId = 0 OR ISNULL(sam.FinYearId, 0) = @FinYearId)
  AND (
        @AdjustmentType = 'ALL'
        OR (@AdjustmentType = 'STOCK IN' AND ISNULL(sad.QtyDifference, 0) >= 0)
        OR (@AdjustmentType = 'STOCK OUT' AND ISNULL(sad.QtyDifference, 0) < 0)
      )
  AND (
        @ItemSearch IS NULL
        OR im.Description LIKE '%' + @ItemSearch + '%'
        OR im.BarCode LIKE '%' + @ItemSearch + '%'
      )
  AND (
        @PeriodMode = 'ALL'
        OR (@PeriodMode = 'DATE_RANGE' AND CAST(sam.StockAdjustmentDate AS DATE) >= @FromDate AND CAST(sam.StockAdjustmentDate AS DATE) <= @ToDate)
        OR (@PeriodMode = 'MONTH_WISE' AND (@Year = 0 OR YEAR(sam.StockAdjustmentDate) = @Year) AND (@Month = 0 OR MONTH(sam.StockAdjustmentDate) = @Month))
        OR (@PeriodMode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(sam.StockAdjustmentDate) = @Year))
      )
ORDER BY sam.StockAdjustmentDate DESC, sam.StockAdjustmentNo DESC, ISNULL(sad.SlNo, 0) ASC;
";

                using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = 180;

                    cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = (object)filter.FromDate.Date ?? DBNull.Value;
                    cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = (object)filter.ToDate.Date ?? DBNull.Value;
                    cmd.Parameters.Add("@PeriodMode", SqlDbType.VarChar, 20).Value = string.IsNullOrWhiteSpace(filter.PeriodMode) ? "ALL" : filter.PeriodMode;
                    cmd.Parameters.Add("@Month", SqlDbType.Int).Value = filter.Month;
                    cmd.Parameters.Add("@Year", SqlDbType.Int).Value = filter.Year;
                    cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = filter.CompanyId > 0 ? filter.CompanyId : SessionContext.CompanyId;
                    cmd.Parameters.Add("@BranchId", SqlDbType.Int).Value = filter.BranchId > 0 ? filter.BranchId : SessionContext.BranchId;
                    cmd.Parameters.Add("@FinYearId", SqlDbType.Int).Value = filter.FinYearId > 0 ? filter.FinYearId : SessionContext.FinYearId;
                    cmd.Parameters.Add("@AdjustmentType", SqlDbType.VarChar, 30).Value = string.IsNullOrWhiteSpace(filter.AdjustmentType) ? "ALL" : filter.AdjustmentType.ToUpper();
                    cmd.Parameters.Add("@ItemSearch", SqlDbType.NVarChar, 200).Value = string.IsNullOrWhiteSpace(filter.ItemSearch) ? (object)DBNull.Value : filter.ItemSearch.Trim();

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);
                        list = MapDataTableToReportRows(table);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ExecuteFallbackQuery (StockAdjustment) error: {ex.Message}");
            }

            return list;
        }

        private List<DatewiseStockAdjustmentReportRow> MapDataTableToReportRows(DataTable table)
        {
            List<DatewiseStockAdjustmentReportRow> list = new List<DatewiseStockAdjustmentReportRow>();
            if (table == null || table.Rows.Count == 0) return list;

            foreach (DataRow row in table.Rows)
            {
                DatewiseStockAdjustmentReportRow item = new DatewiseStockAdjustmentReportRow();

                if (table.Columns.Contains("AdjustmentDate") && row["AdjustmentDate"] != DBNull.Value)
                    item.AdjustmentDate = Convert.ToDateTime(row["AdjustmentDate"]);

                if (table.Columns.Contains("AdjustmentNo") && row["AdjustmentNo"] != DBNull.Value)
                    item.AdjustmentNo = Convert.ToInt32(row["AdjustmentNo"]);

                if (table.Columns.Contains("StockAdjustmentMasterId") && row["StockAdjustmentMasterId"] != DBNull.Value)
                    item.StockAdjustmentMasterId = Convert.ToInt32(row["StockAdjustmentMasterId"]);

                if (table.Columns.Contains("SlNo") && row["SlNo"] != DBNull.Value)
                    item.SlNo = Convert.ToInt32(row["SlNo"]);

                if (table.Columns.Contains("ItemID") && row["ItemID"] != DBNull.Value)
                    item.ItemID = Convert.ToInt32(row["ItemID"]);

                if (table.Columns.Contains("BarCode") && row["BarCode"] != DBNull.Value)
                    item.BarCode = row["BarCode"].ToString();

                if (table.Columns.Contains("ItemName") && row["ItemName"] != DBNull.Value)
                    item.ItemName = row["ItemName"].ToString();

                if (table.Columns.Contains("Unit") && row["Unit"] != DBNull.Value)
                    item.Unit = row["Unit"].ToString();

                if (table.Columns.Contains("AdjustmentType") && row["AdjustmentType"] != DBNull.Value)
                    item.AdjustmentType = row["AdjustmentType"].ToString();

                if (table.Columns.Contains("SystemStock") && row["SystemStock"] != DBNull.Value)
                    item.SystemStock = Convert.ToDecimal(row["SystemStock"]);

                if (table.Columns.Contains("PhysicalStock") && row["PhysicalStock"] != DBNull.Value)
                    item.PhysicalStock = Convert.ToDecimal(row["PhysicalStock"]);

                if (table.Columns.Contains("QtyDifference") && row["QtyDifference"] != DBNull.Value)
                    item.QtyDifference = Convert.ToDecimal(row["QtyDifference"]);

                if (table.Columns.Contains("Qty") && row["Qty"] != DBNull.Value)
                    item.Qty = Convert.ToDecimal(row["Qty"]);

                if (table.Columns.Contains("StockInQty") && row["StockInQty"] != DBNull.Value)
                    item.StockInQty = Convert.ToDecimal(row["StockInQty"]);

                if (table.Columns.Contains("StockOutQty") && row["StockOutQty"] != DBNull.Value)
                    item.StockOutQty = Convert.ToDecimal(row["StockOutQty"]);

                if (table.Columns.Contains("Cost") && row["Cost"] != DBNull.Value)
                    item.Cost = Convert.ToDecimal(row["Cost"]);

                if (table.Columns.Contains("Amount") && row["Amount"] != DBNull.Value)
                    item.Amount = Convert.ToDecimal(row["Amount"]);

                if (table.Columns.Contains("NetAmount") && row["NetAmount"] != DBNull.Value)
                    item.NetAmount = Convert.ToDecimal(row["NetAmount"]);

                if (table.Columns.Contains("Reason") && row["Reason"] != DBNull.Value)
                    item.Reason = row["Reason"].ToString();

                if (table.Columns.Contains("Comments") && row["Comments"] != DBNull.Value)
                    item.Comments = row["Comments"].ToString();

                if (table.Columns.Contains("LedgerName") && row["LedgerName"] != DBNull.Value)
                    item.LedgerName = row["LedgerName"].ToString();

                if (table.Columns.Contains("UserName") && row["UserName"] != DBNull.Value)
                    item.UserName = row["UserName"].ToString();

                list.Add(item);
            }

            return list;
        }

        /// <summary>
        /// Populates drop down options for Stock Adjustment Types.
        /// </summary>
        public DataTable GetAdjustmentTypeDDL()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Code", typeof(string));
            dt.Columns.Add("Name", typeof(string));

            dt.Rows.Add("ALL", "ALL Types");
            dt.Rows.Add("Stock IN", "Stock IN (Excess)");
            dt.Rows.Add("Stock OUT", "Stock OUT (Shortage)");

            return dt;
        }
    }
}
