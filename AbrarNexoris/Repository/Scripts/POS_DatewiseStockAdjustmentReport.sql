-- ============================================================================
-- Stored Procedure: dbo.POS_DatewiseStockAdjustmentReport
-- Description:      Retrieves stock adjustment transactions filtered by date range,
--                   month, year, or all dates, with item name, adjustment quantities,
--                   cost, and adjustment values.
-- Author:           Nexoris POS Engineering
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[POS_DatewiseStockAdjustmentReport]') AND type IN (N'P', N'PC'))
BEGIN
    EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[POS_DatewiseStockAdjustmentReport] AS RETURN 0;';
END
GO

ALTER PROCEDURE [dbo].[POS_DatewiseStockAdjustmentReport]
    @FromDate        DATE          = NULL,
    @ToDate          DATE          = NULL,
    @PeriodMode      VARCHAR(20)   = 'ALL',       -- 'ALL', 'DATE_RANGE', 'MONTH_WISE', 'YEAR_WISE'
    @Month           INT           = 0,           -- 0 = ALL, 1..12
    @Year            INT           = 0,           -- 0 = ALL, 2026, ...
    @CompanyId       INT           = 0,
    @BranchId        INT           = 0,
    @FinYearId       INT           = 0,
    @AdjustmentType  VARCHAR(30)   = 'ALL',       -- 'ALL', 'Stock IN', 'Stock OUT'
    @ItemSearch      NVARCHAR(200) = NULL,
    @ViewMode        VARCHAR(30)   = 'DETAILED'   -- 'DETAILED', 'DATE_ITEM_SUMMARY', 'ITEM_SUMMARY'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Mode VARCHAR(20) = UPPER(LTRIM(RTRIM(ISNULL(@PeriodMode, 'ALL'))));
    DECLARE @View VARCHAR(30) = UPPER(LTRIM(RTRIM(ISNULL(@ViewMode, 'DETAILED'))));
    DECLARE @AdjType VARCHAR(30) = UPPER(LTRIM(RTRIM(ISNULL(@AdjustmentType, 'ALL'))));
    DECLARE @CleanSearch NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@ItemSearch)), '');

    -- Period date boundaries
    DECLARE @StartDate DATE = @FromDate;
    DECLARE @EndDate DATE = @ToDate;

    IF @Mode = 'MONTH_WISE'
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
    ELSE IF @Mode = 'YEAR_WISE'
    BEGIN
        IF @Year > 0
        BEGIN
            SET @StartDate = DATEFROMPARTS(@Year, 1, 1);
            SET @EndDate = DATEFROMPARTS(@Year, 12, 31);
        END
    END

    -- ========================================================================
    -- VIEW MODE 1: DETAILED (Voucher & Item Line by Line)
    -- ========================================================================
    IF @View = 'DETAILED'
    BEGIN
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
                @AdjType = 'ALL'
                OR (@AdjType = 'STOCK IN' AND ISNULL(sad.QtyDifference, 0) >= 0)
                OR (@AdjType = 'STOCK OUT' AND ISNULL(sad.QtyDifference, 0) < 0)
              )
          AND (
                @CleanSearch IS NULL
                OR im.Description LIKE '%' + @CleanSearch + '%'
                OR im.BarCode LIKE '%' + @CleanSearch + '%'
                OR CONVERT(NVARCHAR(50), sam.StockAdjustmentNo) LIKE '%' + @CleanSearch + '%'
                OR sad.Reason LIKE '%' + @CleanSearch + '%'
                OR sam.Comments LIKE '%' + @CleanSearch + '%'
              )
          AND (
                @Mode = 'ALL'
                OR (@Mode = 'DATE_RANGE' AND CAST(sam.StockAdjustmentDate AS DATE) >= @StartDate AND CAST(sam.StockAdjustmentDate AS DATE) <= @EndDate)
                OR (@Mode = 'MONTH_WISE' AND (
                    (@Year = 0 OR YEAR(sam.StockAdjustmentDate) = @Year) AND
                    (@Month = 0 OR MONTH(sam.StockAdjustmentDate) = @Month)
                ))
                OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(sam.StockAdjustmentDate) = @Year))
              )
        ORDER BY sam.StockAdjustmentDate DESC, sam.StockAdjustmentNo DESC, ISNULL(sad.SlNo, 0) ASC;
    END

    -- ========================================================================
    -- VIEW MODE 2: DATE_ITEM_SUMMARY / DATE_MATRIX (Aggregated by Date and Item)
    -- ========================================================================
    ELSE IF @View = 'DATE_ITEM_SUMMARY' OR @View = 'DATE_MATRIX'
    BEGIN
        SELECT 
            CAST(sam.StockAdjustmentDate AS DATE) AS AdjustmentDate,
            0 AS AdjustmentNo,
            0 AS StockAdjustmentMasterId,
            ROW_NUMBER() OVER(PARTITION BY CAST(sam.StockAdjustmentDate AS DATE) ORDER BY ISNULL(im.Description, 'Unknown Item')) AS SlNo,
            ISNULL(sad.ItemId, 0) AS ItemID,
            ISNULL(MAX(im.BarCode), '') AS BarCode,
            ISNULL(im.Description, 'Unknown Item') AS ItemName,
            ISNULL(um.UnitName, '') AS Unit,
            CASE WHEN SUM(ISNULL(sad.QtyDifference, 0)) >= 0 THEN 'Stock IN' ELSE 'Stock OUT' END AS AdjustmentType,
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
            '' AS Reason,
            'Daily Summary' AS Comments,
            '' AS LedgerName,
            '' AS UserName
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
                @AdjType = 'ALL'
                OR (@AdjType = 'STOCK IN' AND ISNULL(sad.QtyDifference, 0) >= 0)
                OR (@AdjType = 'STOCK OUT' AND ISNULL(sad.QtyDifference, 0) < 0)
              )
          AND (
                @CleanSearch IS NULL
                OR im.Description LIKE '%' + @CleanSearch + '%'
                OR im.BarCode LIKE '%' + @CleanSearch + '%'
                OR CONVERT(NVARCHAR(50), sam.StockAdjustmentNo) LIKE '%' + @CleanSearch + '%'
                OR sad.Reason LIKE '%' + @CleanSearch + '%'
                OR sam.Comments LIKE '%' + @CleanSearch + '%'
              )
          AND (
                @Mode = 'ALL'
                OR (@Mode = 'DATE_RANGE' AND CAST(sam.StockAdjustmentDate AS DATE) >= @StartDate AND CAST(sam.StockAdjustmentDate AS DATE) <= @EndDate)
                OR (@Mode = 'MONTH_WISE' AND (
                    (@Year = 0 OR YEAR(sam.StockAdjustmentDate) = @Year) AND
                    (@Month = 0 OR MONTH(sam.StockAdjustmentDate) = @Month)
                ))
                OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(sam.StockAdjustmentDate) = @Year))
              )
        GROUP BY CAST(sam.StockAdjustmentDate AS DATE), sad.ItemId, im.Description, um.UnitName
        ORDER BY CAST(sam.StockAdjustmentDate AS DATE) DESC, ISNULL(im.Description, 'Unknown Item') ASC;
    END

    -- ========================================================================
    -- VIEW MODE 3: ITEM_SUMMARY (Aggregated across the entire period)
    -- ========================================================================
    ELSE IF @View = 'ITEM_SUMMARY'
    BEGIN
        SELECT 
            MAX(sam.StockAdjustmentDate) AS AdjustmentDate,
            0 AS AdjustmentNo,
            0 AS StockAdjustmentMasterId,
            ROW_NUMBER() OVER(ORDER BY ISNULL(im.Description, 'Unknown Item')) AS SlNo,
            ISNULL(sad.ItemId, 0) AS ItemID,
            ISNULL(MAX(im.BarCode), '') AS BarCode,
            ISNULL(im.Description, 'Unknown Item') AS ItemName,
            ISNULL(um.UnitName, '') AS Unit,
            CASE WHEN SUM(ISNULL(sad.QtyDifference, 0)) >= 0 THEN 'Stock IN' ELSE 'Stock OUT' END AS AdjustmentType,
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
            '' AS Reason,
            'Item Summary' AS Comments,
            '' AS LedgerName,
            '' AS UserName
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
                @AdjType = 'ALL'
                OR (@AdjType = 'STOCK IN' AND ISNULL(sad.QtyDifference, 0) >= 0)
                OR (@AdjType = 'STOCK OUT' AND ISNULL(sad.QtyDifference, 0) < 0)
              )
          AND (
                @CleanSearch IS NULL
                OR im.Description LIKE '%' + @CleanSearch + '%'
                OR im.BarCode LIKE '%' + @CleanSearch + '%'
                OR CONVERT(NVARCHAR(50), sam.StockAdjustmentNo) LIKE '%' + @CleanSearch + '%'
                OR sad.Reason LIKE '%' + @CleanSearch + '%'
                OR sam.Comments LIKE '%' + @CleanSearch + '%'
              )
          AND (
                @Mode = 'ALL'
                OR (@Mode = 'DATE_RANGE' AND CAST(sam.StockAdjustmentDate AS DATE) >= @StartDate AND CAST(sam.StockAdjustmentDate AS DATE) <= @EndDate)
                OR (@Mode = 'MONTH_WISE' AND (
                    (@Year = 0 OR YEAR(sam.StockAdjustmentDate) = @Year) AND
                    (@Month = 0 OR MONTH(sam.StockAdjustmentDate) = @Month)
                ))
                OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(sam.StockAdjustmentDate) = @Year))
              )
        GROUP BY sad.ItemId, im.Description, um.UnitName
        ORDER BY ISNULL(im.Description, 'Unknown Item') ASC;
    END
END
GO
