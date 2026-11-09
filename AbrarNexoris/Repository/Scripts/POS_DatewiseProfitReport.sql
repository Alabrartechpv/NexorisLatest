-- ============================================================================
-- Stored Procedure: dbo.POS_DatewiseProfitReport
-- Description:      Retrieves sales & profit records filtered by date range,
--                   month, year, or all dates, with item, quantities, costs,
--                   selling rates, net sales amount, cost amount, profit, and margin %.
-- Author:           Nexoris POS Engineering
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[POS_DatewiseProfitReport]') AND type IN (N'P', N'PC'))
BEGIN
    EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[POS_DatewiseProfitReport] AS RETURN 0;';
END
GO

ALTER PROCEDURE [dbo].[POS_DatewiseProfitReport]
    @FromDate          DATE          = NULL,
    @ToDate            DATE          = NULL,
    @PeriodMode        VARCHAR(20)   = 'ALL',         -- 'ALL', 'DATE_RANGE', 'MONTH_WISE', 'YEAR_WISE'
    @Month             INT           = 0,             -- 0 = ALL, 1..12
    @Year              INT           = 0,             -- 0 = ALL, 2026, ...
    @CompanyId         INT           = 0,
    @BranchId          INT           = 0,
    @FinYearId         INT           = 0,
    @CustomerLedgerId  INT           = 0,
    @ItemSearch        NVARCHAR(200) = NULL,
    @ViewMode          VARCHAR(30)   = 'DATE_MATRIX'  -- 'DATE_MATRIX', 'DETAILED', 'DATE_ITEM_SUMMARY', 'ITEM_SUMMARY'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Mode VARCHAR(20) = UPPER(LTRIM(RTRIM(ISNULL(@PeriodMode, 'ALL'))));
    DECLARE @View VARCHAR(30) = UPPER(LTRIM(RTRIM(ISNULL(@ViewMode, 'DATE_MATRIX'))));
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
    -- VIEW MODE 1: DETAILED (Bill and Item Wise)
    -- ========================================================================
    IF @View = 'DETAILED'
    BEGIN
        SELECT 
            CAST(M.BillDate AS DATE) AS BillDate,
            M.BillNo,
            CAST(M.BillNo AS NVARCHAR(50)) AS InvoiceNo,
            CAST(M.BillDate AS DATE) AS InvoiceDate,
            ISNULL(NULLIF(M.CustomerName, ''), 'Cash Customer') AS CustomerName,
            ISNULL(M.LedgerID, 0) AS CustomerLedgerId,
            ISNULL(D.SlNO, 1) AS SlNo,
            ISNULL(D.ItemId, 0) AS ItemID,
            ISNULL(NULLIF(D.Barcode, ''), ISNULL(PS.BarCode, '')) AS BarCode,
            ISNULL(D.ItemName, '') AS ItemName,
            ISNULL(D.Unit, '') AS Unit,
            CAST(ISNULL(D.Packing, '1') AS NVARCHAR(50)) AS Packing,
            CAST(ISNULL(D.Qty, 0) AS DECIMAL(18,2)) AS Qty,
            CAST(ISNULL(D.UnitPrice, 0) AS DECIMAL(18,2)) AS Rate,
            CAST(ISNULL(D.Cost, 0) AS DECIMAL(18,2)) AS CostRate,
            CAST(ISNULL(D.Amount, ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0)) AS DECIMAL(18,2)) AS SalesAmount,
            CAST(ISNULL(D.DiscountAmount, 0) AS DECIMAL(18,2)) AS DiscountAmt,
            CAST(ISNULL(D.TaxAmt, 0) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(ISNULL(D.TotalAmount, (ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0)) + ISNULL(D.TaxAmt, 0)) AS DECIMAL(18,2)) AS NetAmount,
            CAST(ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0) AS DECIMAL(18,2)) AS CostAmount,
            CAST(ISNULL(D.MarginAmt, ISNULL(D.TotalAmount, (ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0))) - (ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0))) AS DECIMAL(18,2)) AS Profit,
            CAST(CASE WHEN ISNULL(D.TotalAmount, 0) <> 0 
                      THEN (ISNULL(D.MarginAmt, ISNULL(D.TotalAmount, 0) - (ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0))) / D.TotalAmount) * 100 
                      ELSE 0 
                 END AS DECIMAL(18,2)) AS MarginPercent,
            ISNULL(M.PaymodeName, '') AS Paymode,
            ISNULL(CAST(M.UserId AS NVARCHAR(50)), '') AS BilledBy
        FROM dbo.SMaster M WITH (NOLOCK)
        INNER JOIN dbo.SDetails D WITH (NOLOCK) ON M.BillNo = D.BillNo
        OUTER APPLY (
            SELECT TOP 1 ps.BarCode
            FROM dbo.PriceSettings ps WITH (NOLOCK)
            WHERE ps.ItemId = D.ItemId
              AND (@BranchId = 0 OR ps.BranchId = @BranchId)
            ORDER BY CASE WHEN ps.UnitId = D.UnitId THEN 0 WHEN ps.IsBaseUnit = 'Y' THEN 1 ELSE 2 END
        ) PS
        WHERE ISNULL(M.CancelFlag, 0) = 0
          AND ISNULL(M.Status, '') NOT IN ('Hold', 'HOLD')
          AND (@CompanyId = 0 OR ISNULL(M.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(M.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(M.FinYearId, 0) = @FinYearId)
          AND (@CustomerLedgerId = 0 OR ISNULL(M.LedgerID, 0) = @CustomerLedgerId)
          AND (@CleanSearch IS NULL OR D.ItemName LIKE '%' + @CleanSearch + '%' OR D.Barcode LIKE '%' + @CleanSearch + '%' OR PS.BarCode LIKE '%' + @CleanSearch + '%')
          AND (
                @Mode = 'ALL'
                OR (@Mode = 'DATE_RANGE' AND CAST(M.BillDate AS DATE) >= @StartDate AND CAST(M.BillDate AS DATE) <= @EndDate)
                OR (@Mode = 'MONTH_WISE' AND (
                    (@Year = 0 OR YEAR(M.BillDate) = @Year) AND
                    (@Month = 0 OR MONTH(M.BillDate) = @Month)
                ))
                OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(M.BillDate) = @Year))
              )
        ORDER BY M.BillDate DESC, M.BillNo DESC, D.SlNO ASC;
    END

    -- ========================================================================
    -- VIEW MODE 2: DATE_ITEM_SUMMARY / DATE_MATRIX (Aggregated by Date and Item)
    -- ========================================================================
    ELSE IF @View = 'DATE_ITEM_SUMMARY' OR @View = 'DATE_MATRIX'
    BEGIN
        SELECT 
            CAST(M.BillDate AS DATE) AS BillDate,
            CAST(0 AS BIGINT) AS BillNo,
            'Daily Summary' AS InvoiceNo,
            CAST(M.BillDate AS DATE) AS InvoiceDate,
            'Multiple Customers' AS CustomerName,
            0 AS CustomerLedgerId,
            ROW_NUMBER() OVER(PARTITION BY CAST(M.BillDate AS DATE) ORDER BY D.ItemName) AS SlNo,
            ISNULL(D.ItemId, 0) AS ItemID,
            ISNULL(MAX(NULLIF(D.Barcode, '')), ISNULL(MAX(PS.BarCode), '')) AS BarCode,
            ISNULL(D.ItemName, '') AS ItemName,
            ISNULL(D.Unit, '') AS Unit,
            '1' AS Packing,
            CAST(SUM(ISNULL(D.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
            CAST(CASE WHEN SUM(ISNULL(D.Qty, 0)) > 0 
                      THEN SUM(ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0)) / SUM(ISNULL(D.Qty, 0)) 
                      ELSE MAX(ISNULL(D.UnitPrice, 0)) 
                 END AS DECIMAL(18,2)) AS Rate,
            CAST(CASE WHEN SUM(ISNULL(D.Qty, 0)) > 0 
                      THEN SUM(ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)) / SUM(ISNULL(D.Qty, 0)) 
                      ELSE MAX(ISNULL(D.Cost, 0)) 
                 END AS DECIMAL(18,2)) AS CostRate,
            CAST(SUM(ISNULL(D.Amount, ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0))) AS DECIMAL(18,2)) AS SalesAmount,
            CAST(SUM(ISNULL(D.DiscountAmount, 0)) AS DECIMAL(18,2)) AS DiscountAmt,
            CAST(SUM(ISNULL(D.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(SUM(ISNULL(D.TotalAmount, (ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0)) + ISNULL(D.TaxAmt, 0))) AS DECIMAL(18,2)) AS NetAmount,
            CAST(SUM(ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)) AS DECIMAL(18,2)) AS CostAmount,
            CAST(SUM(ISNULL(D.MarginAmt, ISNULL(D.TotalAmount, 0) - (ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)))) AS DECIMAL(18,2)) AS Profit,
            CAST(CASE WHEN SUM(ISNULL(D.TotalAmount, 0)) > 0 
                      THEN (SUM(ISNULL(D.MarginAmt, ISNULL(D.TotalAmount, 0) - (ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)))) / SUM(D.TotalAmount)) * 100 
                      ELSE 0 
                 END AS DECIMAL(18,2)) AS MarginPercent,
            '' AS Paymode,
            '' AS BilledBy
        FROM dbo.SMaster M WITH (NOLOCK)
        INNER JOIN dbo.SDetails D WITH (NOLOCK) ON M.BillNo = D.BillNo
        OUTER APPLY (
            SELECT TOP 1 ps.BarCode
            FROM dbo.PriceSettings ps WITH (NOLOCK)
            WHERE ps.ItemId = D.ItemId
              AND (@BranchId = 0 OR ps.BranchId = @BranchId)
            ORDER BY CASE WHEN ps.UnitId = D.UnitId THEN 0 WHEN ps.IsBaseUnit = 'Y' THEN 1 ELSE 2 END
        ) PS
        WHERE ISNULL(M.CancelFlag, 0) = 0
          AND ISNULL(M.Status, '') NOT IN ('Hold', 'HOLD')
          AND (@CompanyId = 0 OR ISNULL(M.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(M.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(M.FinYearId, 0) = @FinYearId)
          AND (@CustomerLedgerId = 0 OR ISNULL(M.LedgerID, 0) = @CustomerLedgerId)
          AND (@CleanSearch IS NULL OR D.ItemName LIKE '%' + @CleanSearch + '%' OR D.Barcode LIKE '%' + @CleanSearch + '%' OR PS.BarCode LIKE '%' + @CleanSearch + '%')
          AND (
                @Mode = 'ALL'
                OR (@Mode = 'DATE_RANGE' AND CAST(M.BillDate AS DATE) >= @StartDate AND CAST(M.BillDate AS DATE) <= @EndDate)
                OR (@Mode = 'MONTH_WISE' AND (
                    (@Year = 0 OR YEAR(M.BillDate) = @Year) AND
                    (@Month = 0 OR MONTH(M.BillDate) = @Month)
                ))
                OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(M.BillDate) = @Year))
              )
        GROUP BY CAST(M.BillDate AS DATE), D.ItemId, D.ItemName, D.Unit
        ORDER BY CAST(M.BillDate AS DATE) DESC, D.ItemName ASC;
    END

    -- ========================================================================
    -- VIEW MODE 3: ITEM_SUMMARY (Aggregated Across Entire Period by Item)
    -- ========================================================================
    ELSE
    BEGIN
        SELECT 
            MAX(M.BillDate) AS BillDate,
            CAST(0 AS BIGINT) AS BillNo,
            'Item Summary' AS InvoiceNo,
            MAX(M.BillDate) AS InvoiceDate,
            'Multiple Customers' AS CustomerName,
            0 AS CustomerLedgerId,
            ROW_NUMBER() OVER(ORDER BY D.ItemName) AS SlNo,
            ISNULL(D.ItemId, 0) AS ItemID,
            ISNULL(MAX(NULLIF(D.Barcode, '')), ISNULL(MAX(PS.BarCode), '')) AS BarCode,
            ISNULL(D.ItemName, '') AS ItemName,
            ISNULL(D.Unit, '') AS Unit,
            '1' AS Packing,
            CAST(SUM(ISNULL(D.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
            CAST(CASE WHEN SUM(ISNULL(D.Qty, 0)) > 0 
                      THEN SUM(ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0)) / SUM(ISNULL(D.Qty, 0)) 
                      ELSE MAX(ISNULL(D.UnitPrice, 0)) 
                 END AS DECIMAL(18,2)) AS Rate,
            CAST(CASE WHEN SUM(ISNULL(D.Qty, 0)) > 0 
                      THEN SUM(ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)) / SUM(ISNULL(D.Qty, 0)) 
                      ELSE MAX(ISNULL(D.Cost, 0)) 
                 END AS DECIMAL(18,2)) AS CostRate,
            CAST(SUM(ISNULL(D.Amount, ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0))) AS DECIMAL(18,2)) AS SalesAmount,
            CAST(SUM(ISNULL(D.DiscountAmount, 0)) AS DECIMAL(18,2)) AS DiscountAmt,
            CAST(SUM(ISNULL(D.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(SUM(ISNULL(D.TotalAmount, (ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0)) + ISNULL(D.TaxAmt, 0))) AS DECIMAL(18,2)) AS NetAmount,
            CAST(SUM(ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)) AS DECIMAL(18,2)) AS CostAmount,
            CAST(SUM(ISNULL(D.MarginAmt, ISNULL(D.TotalAmount, 0) - (ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)))) AS DECIMAL(18,2)) AS Profit,
            CAST(CASE WHEN SUM(ISNULL(D.TotalAmount, 0)) > 0 
                      THEN (SUM(ISNULL(D.MarginAmt, ISNULL(D.TotalAmount, 0) - (ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)))) / SUM(D.TotalAmount)) * 100 
                      ELSE 0 
                 END AS DECIMAL(18,2)) AS MarginPercent,
            '' AS Paymode,
            '' AS BilledBy
        FROM dbo.SMaster M WITH (NOLOCK)
        INNER JOIN dbo.SDetails D WITH (NOLOCK) ON M.BillNo = D.BillNo
        OUTER APPLY (
            SELECT TOP 1 ps.BarCode
            FROM dbo.PriceSettings ps WITH (NOLOCK)
            WHERE ps.ItemId = D.ItemId
              AND (@BranchId = 0 OR ps.BranchId = @BranchId)
            ORDER BY CASE WHEN ps.UnitId = D.UnitId THEN 0 WHEN ps.IsBaseUnit = 'Y' THEN 1 ELSE 2 END
        ) PS
        WHERE ISNULL(M.CancelFlag, 0) = 0
          AND ISNULL(M.Status, '') NOT IN ('Hold', 'HOLD')
          AND (@CompanyId = 0 OR ISNULL(M.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(M.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(M.FinYearId, 0) = @FinYearId)
          AND (@CustomerLedgerId = 0 OR ISNULL(M.LedgerID, 0) = @CustomerLedgerId)
          AND (@CleanSearch IS NULL OR D.ItemName LIKE '%' + @CleanSearch + '%' OR D.Barcode LIKE '%' + @CleanSearch + '%' OR PS.BarCode LIKE '%' + @CleanSearch + '%')
          AND (
                @Mode = 'ALL'
                OR (@Mode = 'DATE_RANGE' AND CAST(M.BillDate AS DATE) >= @StartDate AND CAST(M.BillDate AS DATE) <= @EndDate)
                OR (@Mode = 'MONTH_WISE' AND (
                    (@Year = 0 OR YEAR(M.BillDate) = @Year) AND
                    (@Month = 0 OR MONTH(M.BillDate) = @Month)
                ))
                OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(M.BillDate) = @Year))
              )
        GROUP BY D.ItemId, D.ItemName, D.Unit
        ORDER BY D.ItemName ASC;
    END
END
GO
