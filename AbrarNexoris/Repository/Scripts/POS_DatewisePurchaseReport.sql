-- ============================================================================
-- Stored Procedure: dbo.POS_DatewisePurchaseReport
-- Description:      Retrieves purchase records filtered by date range, month,
--                   year, or all dates, with item name, quantity, rates, and amounts.
-- Author:           Nexoris POS Engineering
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[POS_DatewisePurchaseReport]') AND type IN (N'P', N'PC'))
BEGIN
    EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[POS_DatewisePurchaseReport] AS RETURN 0;';
END
GO

ALTER PROCEDURE [dbo].[POS_DatewisePurchaseReport]
    @FromDate        DATE          = NULL,
    @ToDate          DATE          = NULL,
    @PeriodMode      VARCHAR(20)   = 'ALL',       -- 'ALL', 'DATE_RANGE', 'MONTH_WISE', 'YEAR_WISE'
    @Month           INT           = 0,           -- 0 = ALL, 1..12
    @Year            INT           = 0,           -- 0 = ALL, 2026, ...
    @CompanyId       INT           = 0,
    @BranchId        INT           = 0,
    @FinYearId       INT           = 0,
    @VendorLedgerId  INT           = 0,
    @ItemSearch      NVARCHAR(200) = NULL,
    @ViewMode        VARCHAR(30)   = 'DETAILED'   -- 'DETAILED', 'DATE_ITEM_SUMMARY', 'ITEM_SUMMARY'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Mode VARCHAR(20) = UPPER(LTRIM(RTRIM(ISNULL(@PeriodMode, 'ALL'))));
    DECLARE @View VARCHAR(30) = UPPER(LTRIM(RTRIM(ISNULL(@ViewMode, 'DETAILED'))));
    DECLARE @CleanSearch NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@ItemSearch)), '');

    -- Format dates
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
    -- VIEW MODE 1: DETAILED (Bill & Item Line by Line)
    -- ========================================================================
    IF @View = 'DETAILED'
    BEGIN
        SELECT 
            M.PurchaseDate,
            M.PurchaseNo,
            ISNULL(M.InvoiceNo, '') AS InvoiceNo,
            ISNULL(M.InvoiceDate, M.PurchaseDate) AS InvoiceDate,
            ISNULL(NULLIF(M.VendorName, ''), 'Unknown Vendor') AS VendorName,
            ISNULL(M.LedgerID, 0) AS VendorLedgerId,
            ISNULL(D.SlNo, 1) AS SlNo,
            ISNULL(D.ItemID, 0) AS ItemID,
            ISNULL(PS.BarCode, '') AS BarCode,
            ISNULL(D.ItemName, '') AS ItemName,
            ISNULL(D.Unit, '') AS Unit,
            CAST(ISNULL(D.Packing, '1') AS NVARCHAR(50)) AS Packing,
            CAST(ISNULL(D.Qty, 0) AS DECIMAL(18,2)) AS Qty,
            CAST(ISNULL(D.Free, 0) AS DECIMAL(18,2)) AS Free,
            CAST((ISNULL(D.Qty, 0) + ISNULL(D.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
            CAST(ISNULL(D.Cost, 0) AS DECIMAL(18,2)) AS Cost,
            CAST(ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0) AS DECIMAL(18,2)) AS Amount,
            CAST(ISNULL(D.TaxAmt, 0) AS DECIMAL(18,2)) AS TaxAmt,
            CAST((ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)) + ISNULL(D.TaxAmt, 0) AS DECIMAL(18,2)) AS NetAmount,
            ISNULL(M.Paymode, '') AS Paymode,
            ISNULL(M.BilledBy, '') AS BilledBy
        FROM dbo.PMaster M WITH (NOLOCK)
        INNER JOIN dbo.PDetails D WITH (NOLOCK) ON M.PurchaseNo = D.PurchaseNo
        OUTER APPLY (
            SELECT TOP 1 ps.BarCode
            FROM dbo.PriceSettings ps WITH (NOLOCK)
            WHERE ps.ItemId = D.ItemID
              AND (@BranchId = 0 OR ps.BranchId = @BranchId)
            ORDER BY CASE WHEN ps.UnitId = D.UnitId THEN 0 WHEN ps.IsBaseUnit = 'Y' THEN 1 ELSE 2 END
        ) PS
        WHERE ISNULL(M.CancelFlag, 0) = 0
          AND (@CompanyId = 0 OR ISNULL(M.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(M.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(M.FinYearId, 0) = @FinYearId)
          AND (@VendorLedgerId = 0 OR ISNULL(M.LedgerID, 0) = @VendorLedgerId)
          AND (@CleanSearch IS NULL OR D.ItemName LIKE '%' + @CleanSearch + '%' OR PS.BarCode LIKE '%' + @CleanSearch + '%')
          AND (
                @Mode = 'ALL'
                OR (@Mode = 'DATE_RANGE' AND CAST(M.PurchaseDate AS DATE) >= @StartDate AND CAST(M.PurchaseDate AS DATE) <= @EndDate)
                OR (@Mode = 'MONTH_WISE' AND (
                    (@Year = 0 OR YEAR(M.PurchaseDate) = @Year) AND
                    (@Month = 0 OR MONTH(M.PurchaseDate) = @Month)
                ))
                OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(M.PurchaseDate) = @Year))
              )
        ORDER BY M.PurchaseDate DESC, M.PurchaseNo DESC, D.SlNo ASC;
    END

    -- ========================================================================
    -- VIEW MODE 2: DATE_ITEM_SUMMARY / DATE_MATRIX (Aggregated by Date and Item)
    -- ========================================================================
    ELSE IF @View = 'DATE_ITEM_SUMMARY' OR @View = 'DATE_MATRIX'
    BEGIN
        SELECT 
            CAST(M.PurchaseDate AS DATE) AS PurchaseDate,
            CAST(0 AS BIGINT) AS PurchaseNo,
            'Daily Summary' AS InvoiceNo,
            CAST(M.PurchaseDate AS DATE) AS InvoiceDate,
            'Multiple Vendors' AS VendorName,
            0 AS VendorLedgerId,
            ROW_NUMBER() OVER(PARTITION BY CAST(M.PurchaseDate AS DATE) ORDER BY D.ItemName) AS SlNo,
            ISNULL(D.ItemID, 0) AS ItemID,
            ISNULL(MAX(PS.BarCode), '') AS BarCode,
            ISNULL(D.ItemName, '') AS ItemName,
            ISNULL(D.Unit, '') AS Unit,
            '1' AS Packing,
            CAST(SUM(ISNULL(D.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
            CAST(SUM(ISNULL(D.Free, 0)) AS DECIMAL(18,2)) AS Free,
            CAST(SUM(ISNULL(D.Qty, 0) + ISNULL(D.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
            CAST(CASE WHEN SUM(ISNULL(D.Qty, 0)) > 0 
                      THEN SUM(ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)) / SUM(ISNULL(D.Qty, 0)) 
                      ELSE MAX(ISNULL(D.Cost, 0)) 
                 END AS DECIMAL(18,2)) AS Cost,
            CAST(SUM(ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)) AS DECIMAL(18,2)) AS Amount,
            CAST(SUM(ISNULL(D.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(SUM((ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)) + ISNULL(D.TaxAmt, 0)) AS DECIMAL(18,2)) AS NetAmount,
            '' AS Paymode,
            '' AS BilledBy
        FROM dbo.PMaster M WITH (NOLOCK)
        INNER JOIN dbo.PDetails D WITH (NOLOCK) ON M.PurchaseNo = D.PurchaseNo
        OUTER APPLY (
            SELECT TOP 1 ps.BarCode
            FROM dbo.PriceSettings ps WITH (NOLOCK)
            WHERE ps.ItemId = D.ItemID
              AND (@BranchId = 0 OR ps.BranchId = @BranchId)
            ORDER BY CASE WHEN ps.UnitId = D.UnitId THEN 0 WHEN ps.IsBaseUnit = 'Y' THEN 1 ELSE 2 END
        ) PS
        WHERE ISNULL(M.CancelFlag, 0) = 0
          AND (@CompanyId = 0 OR ISNULL(M.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(M.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(M.FinYearId, 0) = @FinYearId)
          AND (@VendorLedgerId = 0 OR ISNULL(M.LedgerID, 0) = @VendorLedgerId)
          AND (@CleanSearch IS NULL OR D.ItemName LIKE '%' + @CleanSearch + '%' OR PS.BarCode LIKE '%' + @CleanSearch + '%')
          AND (
                @Mode = 'ALL'
                OR (@Mode = 'DATE_RANGE' AND CAST(M.PurchaseDate AS DATE) >= @StartDate AND CAST(M.PurchaseDate AS DATE) <= @EndDate)
                OR (@Mode = 'MONTH_WISE' AND (
                    (@Year = 0 OR YEAR(M.PurchaseDate) = @Year) AND
                    (@Month = 0 OR MONTH(M.PurchaseDate) = @Month)
                ))
                OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(M.PurchaseDate) = @Year))
              )
        GROUP BY CAST(M.PurchaseDate AS DATE), D.ItemID, D.ItemName, D.Unit
        ORDER BY CAST(M.PurchaseDate AS DATE) DESC, D.ItemName ASC;
    END

    -- ========================================================================
    -- VIEW MODE 3: ITEM_SUMMARY (Aggregated by Item across the entire period)
    -- ========================================================================
    ELSE IF @View = 'ITEM_SUMMARY'
    BEGIN
        SELECT 
            MAX(M.PurchaseDate) AS PurchaseDate,
            CAST(0 AS BIGINT) AS PurchaseNo,
            'Item Summary' AS InvoiceNo,
            MAX(M.PurchaseDate) AS InvoiceDate,
            'Multiple Vendors' AS VendorName,
            0 AS VendorLedgerId,
            ROW_NUMBER() OVER(ORDER BY D.ItemName) AS SlNo,
            ISNULL(D.ItemID, 0) AS ItemID,
            ISNULL(MAX(PS.BarCode), '') AS BarCode,
            ISNULL(D.ItemName, '') AS ItemName,
            ISNULL(D.Unit, '') AS Unit,
            '1' AS Packing,
            CAST(SUM(ISNULL(D.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
            CAST(SUM(ISNULL(D.Free, 0)) AS DECIMAL(18,2)) AS Free,
            CAST(SUM(ISNULL(D.Qty, 0) + ISNULL(D.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
            CAST(CASE WHEN SUM(ISNULL(D.Qty, 0)) > 0 
                      THEN SUM(ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)) / SUM(ISNULL(D.Qty, 0)) 
                      ELSE MAX(ISNULL(D.Cost, 0)) 
                 END AS DECIMAL(18,2)) AS Cost,
            CAST(SUM(ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)) AS DECIMAL(18,2)) AS Amount,
            CAST(SUM(ISNULL(D.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(SUM((ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)) + ISNULL(D.TaxAmt, 0)) AS DECIMAL(18,2)) AS NetAmount,
            '' AS Paymode,
            '' AS BilledBy
        FROM dbo.PMaster M WITH (NOLOCK)
        INNER JOIN dbo.PDetails D WITH (NOLOCK) ON M.PurchaseNo = D.PurchaseNo
        OUTER APPLY (
            SELECT TOP 1 ps.BarCode
            FROM dbo.PriceSettings ps WITH (NOLOCK)
            WHERE ps.ItemId = D.ItemID
              AND (@BranchId = 0 OR ps.BranchId = @BranchId)
            ORDER BY CASE WHEN ps.UnitId = D.UnitId THEN 0 WHEN ps.IsBaseUnit = 'Y' THEN 1 ELSE 2 END
        ) PS
        WHERE ISNULL(M.CancelFlag, 0) = 0
          AND (@CompanyId = 0 OR ISNULL(M.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(M.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(M.FinYearId, 0) = @FinYearId)
          AND (@VendorLedgerId = 0 OR ISNULL(M.LedgerID, 0) = @VendorLedgerId)
          AND (@CleanSearch IS NULL OR D.ItemName LIKE '%' + @CleanSearch + '%' OR PS.BarCode LIKE '%' + @CleanSearch + '%')
          AND (
                @Mode = 'ALL'
                OR (@Mode = 'DATE_RANGE' AND CAST(M.PurchaseDate AS DATE) >= @StartDate AND CAST(M.PurchaseDate AS DATE) <= @EndDate)
                OR (@Mode = 'MONTH_WISE' AND (
                    (@Year = 0 OR YEAR(M.PurchaseDate) = @Year) AND
                    (@Month = 0 OR MONTH(M.PurchaseDate) = @Month)
                ))
                OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(M.PurchaseDate) = @Year))
              )
        GROUP BY D.ItemID, D.ItemName, D.Unit
        ORDER BY D.ItemName ASC;
    END
END;
GO
