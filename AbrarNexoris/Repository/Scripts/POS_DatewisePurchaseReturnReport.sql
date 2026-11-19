-- ============================================================================
-- Stored Procedure: dbo.POS_DatewisePurchaseReturnReport
-- Description:      Retrieves purchase return (debit note) records filtered by
--                   date range, month, year, or all dates, with item name, quantity,
--                   cost, discounts, tax, net amount, return reason, and vendor.
-- Author:           Nexoris POS Engineering
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[POS_DatewisePurchaseReturnReport]') AND type IN (N'P', N'PC'))
BEGIN
    EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[POS_DatewisePurchaseReturnReport] AS RETURN 0;';
END
GO

ALTER PROCEDURE [dbo].[POS_DatewisePurchaseReturnReport]
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
    @ViewMode        VARCHAR(30)   = 'DETAILED'   -- 'DETAILED', 'DATE_ITEM_SUMMARY', 'ITEM_SUMMARY', 'VENDOR_SUMMARY'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Mode VARCHAR(20) = UPPER(LTRIM(RTRIM(ISNULL(@PeriodMode, 'ALL'))));
    DECLARE @View VARCHAR(30) = UPPER(LTRIM(RTRIM(ISNULL(@ViewMode, 'DETAILED'))));
    DECLARE @CleanSearch NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@ItemSearch)), '');

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
    -- VIEW MODE 1: DETAILED (Return Voucher & Item Line by Line)
    -- ========================================================================
    IF @View = 'DETAILED'
    BEGIN
        SELECT 
            PRM.PReturnDate AS ReturnDate,
            PRM.PReturnNo AS ReturnNo,
            ISNULL(PRM.InvoiceNo, '') AS InvoiceNo,
            ISNULL(PRM.InvoiceDate, PRM.PReturnDate) AS InvoiceDate,
            ISNULL(NULLIF(LM.LedgerName, ''), ISNULL(NULLIF(PRM.VendorName, ''), 'Unknown Vendor')) AS VendorName,
            ISNULL(PRM.LedgerID, 0) AS VendorLedgerId,
            ISNULL(PRD.SlNo, 1) AS SlNo,
            ISNULL(PRD.ItemID, 0) AS ItemID,
            ISNULL(PS.BarCode, ISNULL(IM.Barcode, '')) AS BarCode,
            ISNULL(PRD.ItemName, '') AS ItemName,
            ISNULL(PRD.Unit, '') AS Unit,
            CAST(ISNULL(PRD.Packing, 1) AS NVARCHAR(50)) AS Packing,
            CAST(ISNULL(PRD.Qty, 0) AS DECIMAL(18,2)) AS Qty,
            CAST(ISNULL(PRD.Free, 0) AS DECIMAL(18,2)) AS Free,
            CAST((ISNULL(PRD.Qty, 0) + ISNULL(PRD.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
            CAST(ISNULL(PRD.Cost, 0) AS DECIMAL(18,2)) AS Cost,
            CAST(ISNULL(PRD.Qty, 0) * ISNULL(PRD.Cost, 0) AS DECIMAL(18,2)) AS Amount,
            CAST(ISNULL(PRD.DisAmt, 0) AS DECIMAL(18,2)) AS DisAmt,
            CAST(ISNULL(PRD.TaxAmt, 0) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(ISNULL(PRD.TotalSP, (ISNULL(PRD.Qty, 0) * ISNULL(PRD.Cost, 0)) + ISNULL(PRD.TaxAmt, 0) - ISNULL(PRD.DisAmt, 0)) AS DECIMAL(18,2)) AS NetAmount,
            ISNULL(PRD.Reason, '') AS Reason,
            ISNULL(PRM.Paymode, 'CREDIT') AS Paymode,
            ISNULL(PRM.BranchId, 0) AS BranchId,
            ISNULL(B.BranchName, '') AS BranchName,
            ISNULL(PRM.UserName, '') AS BilledBy
        FROM dbo.PReturnMaster PRM WITH (NOLOCK)
        INNER JOIN dbo.PReturnDetails PRD WITH (NOLOCK) 
            ON PRM.PReturnNo = PRD.PReturnNo 
           AND PRM.BranchId = PRD.BranchID
        LEFT JOIN dbo.LedgerMaster LM WITH (NOLOCK) ON LM.LedgerID = PRM.LedgerID
        LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemID = PRD.ItemID
        LEFT JOIN dbo.Branches B WITH (NOLOCK) ON B.Id = PRM.BranchId
        OUTER APPLY (
            SELECT TOP 1 ps.BarCode
            FROM dbo.PriceSettings ps WITH (NOLOCK)
            WHERE ps.ItemId = PRD.ItemID
              AND (@BranchId = 0 OR ps.BranchId = @BranchId)
            ORDER BY CASE WHEN ps.UnitId = PRD.UnitId THEN 0 WHEN ps.IsBaseUnit = 'Y' THEN 1 ELSE 2 END
        ) PS
        WHERE ISNULL(PRM.CancelFlag, 0) = 0
          AND (@CompanyId = 0 OR ISNULL(PRM.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(PRM.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(PRM.FinYearId, 0) = @FinYearId)
          AND (@VendorLedgerId = 0 OR ISNULL(PRM.LedgerID, 0) = @VendorLedgerId)
          AND (
                @CleanSearch IS NULL 
                OR PRD.ItemName LIKE '%' + @CleanSearch + '%' 
                OR PS.BarCode LIKE '%' + @CleanSearch + '%' 
                OR PRM.InvoiceNo LIKE '%' + @CleanSearch + '%'
                OR CAST(PRM.PReturnNo AS VARCHAR(50)) LIKE '%' + @CleanSearch + '%'
              )
          AND (
                @Mode = 'ALL'
                OR (@Mode = 'DATE_RANGE' AND CAST(PRM.PReturnDate AS DATE) >= @StartDate AND CAST(PRM.PReturnDate AS DATE) <= @EndDate)
                OR (@Mode = 'MONTH_WISE' AND (
                    (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year) AND
                    (@Month = 0 OR MONTH(PRM.PReturnDate) = @Month)
                ))
                OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year))
              )
        ORDER BY PRM.PReturnDate DESC, PRM.PReturnNo DESC, PRD.SlNo ASC;
    END

    -- ========================================================================
    -- VIEW MODE 2: DATE_ITEM_SUMMARY / DATE_MATRIX (Aggregated by Date and Item)
    -- ========================================================================
    ELSE IF @View = 'DATE_ITEM_SUMMARY' OR @View = 'DATE_MATRIX'
    BEGIN
        SELECT 
            CAST(PRM.PReturnDate AS DATE) AS ReturnDate,
            PRD.ItemID,
            ISNULL(PS.BarCode, ISNULL(IM.Barcode, '')) AS BarCode,
            PRD.ItemName,
            PRD.Unit,
            COUNT(DISTINCT PRM.PReturnNo) AS ReturnCount,
            CAST(SUM(ISNULL(PRD.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
            CAST(SUM(ISNULL(PRD.Free, 0)) AS DECIMAL(18,2)) AS Free,
            CAST(SUM(ISNULL(PRD.Qty, 0) + ISNULL(PRD.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
            CAST(SUM(ISNULL(PRD.Qty, 0) * ISNULL(PRD.Cost, 0)) AS DECIMAL(18,2)) AS Amount,
            CAST(SUM(ISNULL(PRD.DisAmt, 0)) AS DECIMAL(18,2)) AS DisAmt,
            CAST(SUM(ISNULL(PRD.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(SUM(ISNULL(PRD.TotalSP, (ISNULL(PRD.Qty, 0) * ISNULL(PRD.Cost, 0)) + ISNULL(PRD.TaxAmt, 0) - ISNULL(PRD.DisAmt, 0))) AS DECIMAL(18,2)) AS NetAmount
        FROM dbo.PReturnMaster PRM WITH (NOLOCK)
        INNER JOIN dbo.PReturnDetails PRD WITH (NOLOCK) 
            ON PRM.PReturnNo = PRD.PReturnNo 
           AND PRM.BranchId = PRD.BranchID
        LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemID = PRD.ItemID
        OUTER APPLY (
            SELECT TOP 1 ps.BarCode
            FROM dbo.PriceSettings ps WITH (NOLOCK)
            WHERE ps.ItemId = PRD.ItemID
              AND (@BranchId = 0 OR ps.BranchId = @BranchId)
            ORDER BY CASE WHEN ps.UnitId = PRD.UnitId THEN 0 WHEN ps.IsBaseUnit = 'Y' THEN 1 ELSE 2 END
        ) PS
        WHERE ISNULL(PRM.CancelFlag, 0) = 0
          AND (@CompanyId = 0 OR ISNULL(PRM.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(PRM.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(PRM.FinYearId, 0) = @FinYearId)
          AND (@VendorLedgerId = 0 OR ISNULL(PRM.LedgerID, 0) = @VendorLedgerId)
          AND (
                @CleanSearch IS NULL 
                OR PRD.ItemName LIKE '%' + @CleanSearch + '%' 
                OR PS.BarCode LIKE '%' + @CleanSearch + '%' 
                OR PRM.InvoiceNo LIKE '%' + @CleanSearch + '%'
              )
          AND (
                @Mode = 'ALL'
                OR (@Mode = 'DATE_RANGE' AND CAST(PRM.PReturnDate AS DATE) >= @StartDate AND CAST(PRM.PReturnDate AS DATE) <= @EndDate)
                OR (@Mode = 'MONTH_WISE' AND (
                    (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year) AND
                    (@Month = 0 OR MONTH(PRM.PReturnDate) = @Month)
                ))
                OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year))
              )
        GROUP BY 
            CAST(PRM.PReturnDate AS DATE),
            PRD.ItemID,
            ISNULL(PS.BarCode, ISNULL(IM.Barcode, '')),
            PRD.ItemName,
            PRD.Unit
        ORDER BY ReturnDate DESC, NetAmount DESC;
    END

    -- ========================================================================
    -- VIEW MODE 3: ITEM_SUMMARY (Total aggregated by Item across entire period)
    -- ========================================================================
    ELSE IF @View = 'ITEM_SUMMARY'
    BEGIN
        SELECT 
            PRD.ItemID,
            ISNULL(PS.BarCode, ISNULL(IM.Barcode, '')) AS BarCode,
            PRD.ItemName,
            PRD.Unit,
            COUNT(DISTINCT PRM.PReturnNo) AS ReturnCount,
            CAST(SUM(ISNULL(PRD.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
            CAST(SUM(ISNULL(PRD.Free, 0)) AS DECIMAL(18,2)) AS Free,
            CAST(SUM(ISNULL(PRD.Qty, 0) + ISNULL(PRD.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
            CAST(SUM(ISNULL(PRD.Qty, 0) * ISNULL(PRD.Cost, 0)) AS DECIMAL(18,2)) AS Amount,
            CAST(SUM(ISNULL(PRD.DisAmt, 0)) AS DECIMAL(18,2)) AS DisAmt,
            CAST(SUM(ISNULL(PRD.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(SUM(ISNULL(PRD.TotalSP, (ISNULL(PRD.Qty, 0) * ISNULL(PRD.Cost, 0)) + ISNULL(PRD.TaxAmt, 0) - ISNULL(PRD.DisAmt, 0))) AS DECIMAL(18,2)) AS NetAmount
        FROM dbo.PReturnMaster PRM WITH (NOLOCK)
        INNER JOIN dbo.PReturnDetails PRD WITH (NOLOCK) 
            ON PRM.PReturnNo = PRD.PReturnNo 
           AND PRM.BranchId = PRD.BranchID
        LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemID = PRD.ItemID
        OUTER APPLY (
            SELECT TOP 1 ps.BarCode
            FROM dbo.PriceSettings ps WITH (NOLOCK)
            WHERE ps.ItemId = PRD.ItemID
              AND (@BranchId = 0 OR ps.BranchId = @BranchId)
            ORDER BY CASE WHEN ps.UnitId = PRD.UnitId THEN 0 WHEN ps.IsBaseUnit = 'Y' THEN 1 ELSE 2 END
        ) PS
        WHERE ISNULL(PRM.CancelFlag, 0) = 0
          AND (@CompanyId = 0 OR ISNULL(PRM.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(PRM.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(PRM.FinYearId, 0) = @FinYearId)
          AND (@VendorLedgerId = 0 OR ISNULL(PRM.LedgerID, 0) = @VendorLedgerId)
          AND (
                @CleanSearch IS NULL 
                OR PRD.ItemName LIKE '%' + @CleanSearch + '%' 
                OR PS.BarCode LIKE '%' + @CleanSearch + '%' 
                OR PRM.InvoiceNo LIKE '%' + @CleanSearch + '%'
              )
          AND (
                @Mode = 'ALL'
                OR (@Mode = 'DATE_RANGE' AND CAST(PRM.PReturnDate AS DATE) >= @StartDate AND CAST(PRM.PReturnDate AS DATE) <= @EndDate)
                OR (@Mode = 'MONTH_WISE' AND (
                    (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year) AND
                    (@Month = 0 OR MONTH(PRM.PReturnDate) = @Month)
                ))
                OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year))
              )
        GROUP BY 
            PRD.ItemID,
            ISNULL(PS.BarCode, ISNULL(IM.Barcode, '')),
            PRD.ItemName,
            PRD.Unit
        ORDER BY NetAmount DESC;
    END

    -- ========================================================================
    -- VIEW MODE 4: VENDOR_SUMMARY (Total aggregated by Vendor)
    -- ========================================================================
    ELSE IF @View = 'VENDOR_SUMMARY'
    BEGIN
        SELECT 
            ISNULL(PRM.LedgerID, 0) AS VendorLedgerId,
            ISNULL(NULLIF(LM.LedgerName, ''), ISNULL(NULLIF(PRM.VendorName, ''), 'Unknown Vendor')) AS VendorName,
            COUNT(DISTINCT PRM.PReturnNo) AS ReturnCount,
            CAST(SUM(ISNULL(PRD.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
            CAST(SUM(ISNULL(PRD.Free, 0)) AS DECIMAL(18,2)) AS Free,
            CAST(SUM(ISNULL(PRD.Qty, 0) + ISNULL(PRD.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
            CAST(SUM(ISNULL(PRD.Qty, 0) * ISNULL(PRD.Cost, 0)) AS DECIMAL(18,2)) AS Amount,
            CAST(SUM(ISNULL(PRD.DisAmt, 0)) AS DECIMAL(18,2)) AS DisAmt,
            CAST(SUM(ISNULL(PRD.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(SUM(ISNULL(PRD.TotalSP, (ISNULL(PRD.Qty, 0) * ISNULL(PRD.Cost, 0)) + ISNULL(PRD.TaxAmt, 0) - ISNULL(PRD.DisAmt, 0))) AS DECIMAL(18,2)) AS NetAmount
        FROM dbo.PReturnMaster PRM WITH (NOLOCK)
        INNER JOIN dbo.PReturnDetails PRD WITH (NOLOCK) 
            ON PRM.PReturnNo = PRD.PReturnNo 
           AND PRM.BranchId = PRD.BranchID
        LEFT JOIN dbo.LedgerMaster LM WITH (NOLOCK) ON LM.LedgerID = PRM.LedgerID
        WHERE ISNULL(PRM.CancelFlag, 0) = 0
          AND (@CompanyId = 0 OR ISNULL(PRM.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(PRM.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(PRM.FinYearId, 0) = @FinYearId)
          AND (@VendorLedgerId = 0 OR ISNULL(PRM.LedgerID, 0) = @VendorLedgerId)
          AND (
                @CleanSearch IS NULL 
                OR PRD.ItemName LIKE '%' + @CleanSearch + '%' 
                OR PRM.InvoiceNo LIKE '%' + @CleanSearch + '%'
              )
          AND (
                @Mode = 'ALL'
                OR (@Mode = 'DATE_RANGE' AND CAST(PRM.PReturnDate AS DATE) >= @StartDate AND CAST(PRM.PReturnDate AS DATE) <= @EndDate)
                OR (@Mode = 'MONTH_WISE' AND (
                    (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year) AND
                    (@Month = 0 OR MONTH(PRM.PReturnDate) = @Month)
                ))
                OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year))
              )
        GROUP BY 
            ISNULL(PRM.LedgerID, 0),
            ISNULL(NULLIF(LM.LedgerName, ''), ISNULL(NULLIF(PRM.VendorName, ''), 'Unknown Vendor'))
        ORDER BY NetAmount DESC;
    END
END
GO
