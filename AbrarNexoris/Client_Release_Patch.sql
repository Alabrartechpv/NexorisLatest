-- ====================================================================================
-- NEXORIS CLIENT-SIDE UPDATE & RECONCILIATION PATCH
-- Target: SQL Server 2012, 2014, 2016, 2019, 2022+ (All Editions)
-- Safe & Idempotent: Can be executed multiple times on any client database.
-- ====================================================================================

PRINT '>>> Starting Nexoris Client-Side Update Patch...';
GO

-- ====================================================================================
-- STEP 1: DEDUPLICATE AccountGroupMaster (Fixes doubled joins in reports)
-- ====================================================================================
PRINT 'Step 1: Deduplicating AccountGroupMaster across all branches...';
;WITH CTE_Groups AS (
    SELECT *,
           ROW_NUMBER() OVER(
               PARTITION BY BranchID, GroupID 
               ORDER BY 
                   CASE WHEN ISNULL(GroupType,'') <> '' THEN 0 ELSE 1 END,
                   CASE WHEN ISNULL(Description,'') <> '' THEN 0 ELSE 1 END
           ) as rn
    FROM AccountGroupMaster
)
DELETE FROM CTE_Groups WHERE rn > 1;
PRINT 'AccountGroupMaster clean & deduplicated.';
GO

-- ====================================================================================
-- STEP 2: NORMALIZE VOUCHER DATA (Fixes CompanyID mismatches & legacy phantom stock debits)
-- ====================================================================================
PRINT 'Step 2: Normalizing voucher data integrity...';

-- A. Cancel legacy physical stock phantom debits to CASH-IN-HAND (targets by group/name, never by static LedgerID)
UPDATE v
SET v.CancelFlag = 1
FROM Vouchers v
INNER JOIN LedgerMaster lm ON v.LedgerID = lm.LedgerID AND v.BranchID = lm.BranchID
WHERE v.VoucherType = 'PhysicalStock' 
  AND (lm.GroupID = 14 OR lm.LedgerName = 'CASH-IN-HAND' OR lm.LedgerName = 'CASH')
  AND ISNULL(v.CancelFlag, 0) = 0;

-- B. Fix orphan PhysicalStock rows with LedgerID = 0
UPDATE Vouchers
SET LedgerID = 1, LedgerName = 'BEGIN STOCK', GroupID = 18
WHERE VoucherType = 'PhysicalStock' 
  AND (LedgerID = 0 OR LedgerID IS NULL);

-- C. Normalize CompanyID on Branch 2 Vouchers to match Company 1
UPDATE Vouchers
SET CompanyID = 1
WHERE BranchID = 2 AND CompanyID <> 1;

UPDATE CustomerReceiptMaster
SET CompanyID = 1
WHERE BranchID = 2 AND CompanyID <> 1;

UPDATE VendorPaymentMaster
SET CompanyID = 1
WHERE BranchID = 2 AND CompanyID <> 1;

PRINT 'Voucher data normalized successfully.';
GO

-- ====================================================================================
-- STEP 3: UPDATE STORED PROCEDURE [POS_CashBankBook]
-- ====================================================================================
PRINT 'Step 3: Updating Stored Procedure [POS_CashBankBook]...';
GO

IF OBJECT_ID('dbo.POS_CashBankBook', 'P') IS NOT NULL
    DROP PROCEDURE dbo.POS_CashBankBook;
GO

CREATE PROCEDURE [dbo].[POS_CashBankBook]
    @CompanyId INT,
    @BranchId INT,
    @FinYearId INT,
    @LedgerId INT,
    @FromDate DATETIME,
    @ToDate DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Calculate Opening Balance UP TO @FromDate for this ledger
    DECLARE @OpnDebit DECIMAL(18,2) = 0;
    DECLARE @OpnCredit DECIMAL(18,2) = 0;
    DECLARE @LedgerOpnDebit DECIMAL(18,2) = 0;
    DECLARE @LedgerOpnCredit DECIMAL(18,2) = 0;

    -- Get Ledger's absolute opening balance from LedgerMaster
    SELECT 
        @LedgerOpnDebit = ISNULL(OpnDebit, 0),
        @LedgerOpnCredit = ISNULL(OpnCredit, 0)
    FROM LedgerMaster 
    WHERE LedgerID = @LedgerId AND (@BranchId = 0 OR BranchID = @BranchId);

    -- Get sum of transactions BEFORE @FromDate
    SELECT 
        @OpnDebit = ISNULL(SUM(Debit), 0),
        @OpnCredit = ISNULL(SUM(Credit), 0)
    FROM Vouchers
    WHERE LedgerID = @LedgerId
      AND (@CompanyId = 0 OR CompanyID = @CompanyId)
      AND (@BranchId = 0 OR BranchID = @BranchId)
      AND (@FinYearId = 0 OR FinYearId = @FinYearId)
      AND CAST(VoucherDate AS DATE) < CAST(@FromDate AS DATE)
      AND ISNULL(CancelFlag, 0) = 0;

    -- Final Opening Balance = Master Opening + Prior Transactions
    DECLARE @TotalOpeningDebit DECIMAL(18,2) = @LedgerOpnDebit + @OpnDebit;
    DECLARE @TotalOpeningCredit DECIMAL(18,2) = @LedgerOpnCredit + @OpnCredit;
    DECLARE @OpeningBalance DECIMAL(18,2) = @TotalOpeningDebit - @TotalOpeningCredit;

    -- 2. Fetch Transactions BETWEEN @FromDate AND @ToDate
    SELECT 
        v.VoucherID,
        v.VoucherDate,
        v.VoucherNumber AS VoucherNo,
        v.VoucherType AS VoucherTypeName,
        ISNULL((
            SELECT TOP 1 l2.LedgerName 
            FROM Vouchers v2 
            INNER JOIN LedgerMaster l2 ON v2.LedgerID = l2.LedgerID 
            WHERE v2.VoucherID = v.VoucherID AND v2.VoucherType = v.VoucherType AND v2.LedgerID <> @LedgerId
        ), 'Various Accounts') AS Particulars,
        v.Narration,
        v.Debit AS ReceiptAmount,
        v.Credit AS PaymentAmount,
        @OpeningBalance + SUM(v.Debit - v.Credit) OVER (ORDER BY v.VoucherDate, v.VoucherID) AS RunningBalance
    INTO #TempTransactions
    FROM Vouchers v
    WHERE v.LedgerID = @LedgerId
      AND (@CompanyId = 0 OR v.CompanyID = @CompanyId)
      AND (@BranchId = 0 OR v.BranchID = @BranchId)
      AND (@FinYearId = 0 OR v.FinYearId = @FinYearId)
      AND CAST(v.VoucherDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)
      AND ISNULL(v.CancelFlag, 0) = 0;

    -- RESULT SET 1: Transactions
    SELECT * FROM #TempTransactions ORDER BY VoucherDate, VoucherID;

    -- RESULT SET 2: Summary
    SELECT 
        @OpeningBalance AS OpeningBalance,
        ISNULL((SELECT SUM(ReceiptAmount) FROM #TempTransactions), 0) AS TotalReceipts,
        ISNULL((SELECT SUM(PaymentAmount) FROM #TempTransactions), 0) AS TotalPayments,
        @OpeningBalance + ISNULL((SELECT SUM(ReceiptAmount) FROM #TempTransactions), 0) - ISNULL((SELECT SUM(PaymentAmount) FROM #TempTransactions), 0) AS ClosingBalance;

    DROP TABLE #TempTransactions;
END
GO
PRINT '[POS_CashBankBook] updated successfully.';
GO

-- ====================================================================================
-- STEP 4: UPDATE STORED PROCEDURE [POS_BalanceSheet]
-- ====================================================================================
PRINT 'Step 4: Updating Stored Procedure [POS_BalanceSheet]...';
GO

IF OBJECT_ID('dbo.POS_BalanceSheet', 'P') IS NOT NULL
    DROP PROCEDURE dbo.POS_BalanceSheet;
GO

CREATE PROCEDURE [dbo].[POS_BalanceSheet]
    @CompanyId INT,
    @BranchId INT,
    @FinYearId INT,
    @FromDate DATETIME,
    @ToDate DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    -- Handle financial year start date dynamically using FinancialYear table
    DECLARE @FinYearStartDate DATETIME;
    IF OBJECT_ID('FinancialYear', 'U') IS NOT NULL
    BEGIN
        SELECT TOP 1 @FinYearStartDate = FinYearFrom 
        FROM FinancialYear 
        WHERE FinYearID = @FinYearId;
    END

    IF @FinYearStartDate IS NULL
    BEGIN
        SET @FinYearStartDate = @FromDate;
    END

    -- Build clean group hierarchy mapping
    CREATE TABLE #GroupMap (GroupID INT, RootGroupID INT, RootGroupType VARCHAR(20));

    ;WITH CleanGroupMaster AS (
        SELECT GroupID, ParentGroupID, GroupName, BranchID
        FROM AccountGroupMaster
        WHERE (@BranchId = 0 OR BranchID = @BranchId)
    ),
    GroupHierarchy AS (
        SELECT GroupID, GroupID AS RootGroupID,
            CASE GroupID
                WHEN 1 THEN 'CAPITAL' WHEN 2 THEN 'LIABILITIES' WHEN 3 THEN 'LIABILITIES' WHEN 4 THEN 'ASSETS'
                WHEN 5 THEN 'ASSETS' WHEN 6 THEN 'ASSETS' WHEN 7 THEN 'ASSETS' WHEN 8 THEN 'INCOME'
                WHEN 9 THEN 'EXPENSE' WHEN 10 THEN 'EXPENSE' WHEN 11 THEN 'INCOME' WHEN 12 THEN 'EXPENSE'
                WHEN 13 THEN 'INCOME' WHEN 14 THEN 'ASSETS' WHEN 15 THEN 'ASSETS' WHEN 16 THEN 'ASSETS'
                WHEN 17 THEN 'LIABILITIES' WHEN 18 THEN 'ASSETS' WHEN 19 THEN 'LIABILITIES' WHEN 20 THEN 'LIABILITIES'
                WHEN 21 THEN 'LIABILITIES' WHEN 22 THEN 'LIABILITIES' WHEN 29 THEN 'LIABILITIES'
            END AS RootGroupType
        FROM CleanGroupMaster
        WHERE GroupID IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 29)
        UNION ALL
        SELECT child.GroupID, parent.RootGroupID, parent.RootGroupType
        FROM CleanGroupMaster child
        INNER JOIN GroupHierarchy parent ON child.ParentGroupID = parent.GroupID
    )
    INSERT INTO #GroupMap (GroupID, RootGroupID, RootGroupType)
    SELECT GroupID, MIN(RootGroupID), MAX(RootGroupType)
    FROM GroupHierarchy
    GROUP BY GroupID;

    -- Prior period profit/loss carried forward
    DECLARE @PriorPeriodProfit DECIMAL(18,2) = 0;
    SELECT @PriorPeriodProfit = ISNULL(SUM(
        CASE 
            WHEN gm.RootGroupType = 'INCOME' THEN (v.Credit - v.Debit)
            WHEN gm.RootGroupType = 'EXPENSE' THEN -(v.Debit - v.Credit)
            ELSE 0 
        END
    ), 0)
    FROM Vouchers v
    INNER JOIN LedgerMaster l ON v.LedgerID = l.LedgerID AND v.BranchID = l.BranchID
    INNER JOIN #GroupMap gm ON l.GroupID = gm.GroupID
    WHERE (@BranchId = 0 OR v.BranchID = @BranchId)
      AND v.VoucherDate < @FinYearStartDate
      AND ISNULL(v.CancelFlag,0) = 0;

    -- Current financial year Net Profit
    DECLARE @NetProfit DECIMAL(18,2) = 0;
    DECLARE @TotalIncome DECIMAL(18,2) = 0;
    DECLARE @TotalExpense DECIMAL(18,2) = 0;

    SELECT 
        @TotalIncome = ISNULL(SUM(CASE WHEN gm.RootGroupType = 'INCOME' THEN (v.Credit - v.Debit) ELSE 0 END), 0),
        @TotalExpense = ISNULL(SUM(CASE WHEN gm.RootGroupType = 'EXPENSE' THEN (v.Debit - v.Credit) ELSE 0 END), 0)
    FROM Vouchers v
    INNER JOIN LedgerMaster l ON v.LedgerID = l.LedgerID AND v.BranchID = l.BranchID
    INNER JOIN #GroupMap gm ON l.GroupID = gm.GroupID
    WHERE (@BranchId = 0 OR v.BranchID = @BranchId)
      AND v.VoucherDate >= @FinYearStartDate
      AND v.VoucherDate <= @ToDate
      AND ISNULL(v.CancelFlag,0) = 0;

    -- Stock Valuations (Opening Stock)
    DECLARE @OpeningStock DECIMAL(18,4) = 0;
    SELECT @OpeningStock = ISNULL(SUM((ISNULL(l.OpnDebit,0) + ISNULL(v.Debit,0)) - (ISNULL(l.OpnCredit,0) + ISNULL(v.Credit,0))), 0)
    FROM LedgerMaster l
    LEFT JOIN (
        SELECT LedgerID, SUM(Debit) AS Debit, SUM(Credit) AS Credit
        FROM Vouchers
        WHERE (@BranchId = 0 OR BranchID = @BranchId)
          AND (@FinYearId = 0 OR FinYearID = @FinYearId)
          AND (VoucherDate < @FinYearStartDate OR VoucherType = 'PhysicalStock')
          AND ISNULL(CancelFlag,0) = 0
        GROUP BY LedgerID
    ) v ON l.LedgerID = v.LedgerID
    WHERE (@BranchId = 0 OR l.BranchID = @BranchId) AND l.GroupID = 18;

    -- Closing Stock Valuation: Check PriceSettings first, then StockMaster, then fallback
    DECLARE @ClosingStock DECIMAL(18,4) = 0;
    IF OBJECT_ID('PriceSettings', 'U') IS NOT NULL
    BEGIN
        SELECT @ClosingStock = ISNULL(SUM(ISNULL(Stock, 0) * ISNULL(Cost, 0)), 0)
        FROM PriceSettings
        WHERE (@BranchId = 0 OR BranchId = @BranchId) AND ISNULL(Stock, 0) > 0;
    END
    ELSE IF OBJECT_ID('StockMaster', 'U') IS NOT NULL
    BEGIN
        SELECT @ClosingStock = ISNULL(SUM(CurrentStock * Cost), 0)
        FROM StockMaster
        WHERE (@BranchId = 0 OR BranchId = @BranchId) AND CurrentStock > 0;
    END

    IF @ClosingStock = 0
    BEGIN
        SELECT @ClosingStock = @OpeningStock;
    END

    -- Final Net Profit incorporates stock movement
    SET @NetProfit = (@TotalIncome + @ClosingStock) - (@TotalExpense + @OpeningStock);

    -- RESULT SET 1: LIABILITIES & CAPITAL
    SELECT 
        l.LedgerID,
        l.LedgerName,
        l.GroupID,
        ag.GroupName,
        CASE gm.RootGroupType WHEN 'CAPITAL' THEN 'CAPITAL' ELSE 'CURRENT LIABILITIES' END AS ParentGroupName,
        ISNULL(v.Debit, 0) AS TotalDebit,
        ISNULL(v.Credit, 0) AS TotalCredit,
        (ISNULL(l.OpnCredit, 0) + ISNULL(v.Credit, 0)) - (ISNULL(l.OpnDebit, 0) + ISNULL(v.Debit, 0)) AS ClosingBalance
    INTO #Liabilities
    FROM LedgerMaster l
    INNER JOIN #GroupMap gm ON l.GroupID = gm.GroupID
    INNER JOIN (SELECT DISTINCT GroupID, GroupName FROM AccountGroupMaster) ag ON l.GroupID = ag.GroupID
    LEFT JOIN (
        SELECT LedgerID, SUM(Debit) AS Debit, SUM(Credit) AS Credit
        FROM Vouchers
        WHERE (@BranchId = 0 OR BranchID = @BranchId)
          AND (@FinYearId = 0 OR FinYearId = @FinYearId)
          AND VoucherDate <= @ToDate
          AND ISNULL(CancelFlag,0) = 0
        GROUP BY LedgerID
    ) v ON l.LedgerID = v.LedgerID
    WHERE (@BranchId = 0 OR l.BranchID = @BranchId)
      AND gm.RootGroupType IN ('CAPITAL', 'LIABILITIES');

    -- Inject Prior P&L into Reserves & Surplus row if non-zero
    IF @PriorPeriodProfit <> 0
    BEGIN
        IF EXISTS (SELECT 1 FROM #Liabilities WHERE GroupID = 20)
        BEGIN
            UPDATE #Liabilities SET ClosingBalance = ClosingBalance + @PriorPeriodProfit WHERE GroupID = 20;
        END
        ELSE
        BEGIN
            INSERT INTO #Liabilities (LedgerID, LedgerName, GroupID, GroupName, ParentGroupName, TotalDebit, TotalCredit, ClosingBalance)
            VALUES (0, 'Retained Earnings (Prior P&L)', 20, 'RESERVES & SURPLUS', 'CAPITAL', 
                    CASE WHEN @PriorPeriodProfit < 0 THEN ABS(@PriorPeriodProfit) ELSE 0 END,
                    CASE WHEN @PriorPeriodProfit > 0 THEN @PriorPeriodProfit ELSE 0 END,
                    @PriorPeriodProfit);
        END
    END

    SELECT * FROM #Liabilities WHERE ClosingBalance <> 0 ORDER BY ParentGroupName DESC, LedgerName;

    -- RESULT SET 2: ASSETS
    SELECT 
        l.LedgerID,
        l.LedgerName,
        l.GroupID,
        ag.GroupName,
        'CURRENT ASSETS' AS ParentGroupName,
        ISNULL(v.Debit, 0) AS TotalDebit,
        ISNULL(v.Credit, 0) AS TotalCredit,
        (ISNULL(l.OpnDebit, 0) + ISNULL(v.Debit, 0)) - (ISNULL(l.OpnCredit, 0) + ISNULL(v.Credit, 0)) AS ClosingBalance
    INTO #Assets
    FROM LedgerMaster l
    INNER JOIN #GroupMap gm ON l.GroupID = gm.GroupID
    INNER JOIN (SELECT DISTINCT GroupID, GroupName FROM AccountGroupMaster) ag ON l.GroupID = ag.GroupID
    LEFT JOIN (
        SELECT LedgerID, SUM(Debit) AS Debit, SUM(Credit) AS Credit
        FROM Vouchers
        WHERE (@BranchId = 0 OR BranchID = @BranchId)
          AND (@FinYearId = 0 OR FinYearId = @FinYearId)
          AND VoucherDate <= @ToDate
          AND ISNULL(CancelFlag,0) = 0
        GROUP BY LedgerID
    ) v ON l.LedgerID = v.LedgerID
    WHERE (@BranchId = 0 OR l.BranchID = @BranchId)
      AND gm.RootGroupType = 'ASSETS'
      AND l.GroupID <> 18;

    -- Add Closing Stock
    INSERT INTO #Assets (LedgerID, LedgerName, GroupID, GroupName, ParentGroupName, TotalDebit, TotalCredit, ClosingBalance)
    VALUES (0, 'Closing Stock', 18, 'STOCK IN HAND', 'CURRENT ASSETS', @ClosingStock, 0, @ClosingStock);

    SELECT * FROM #Assets WHERE ClosingBalance <> 0 ORDER BY LedgerName;

    -- RESULT SET 3: SUMMARY TOTALS & EQUATION
    DECLARE @TotalCapital DECIMAL(18,2) = 0;
    DECLARE @TotalLiabilities DECIMAL(18,2) = 0;
    DECLARE @TotalAssets DECIMAL(18,2) = 0;

    SELECT 
        @TotalCapital = ISNULL(SUM(CASE WHEN ParentGroupName = 'CAPITAL' THEN ClosingBalance ELSE 0 END), 0),
        @TotalLiabilities = ISNULL(SUM(CASE WHEN ParentGroupName <> 'CAPITAL' THEN ClosingBalance ELSE 0 END), 0)
    FROM #Liabilities;

    SELECT @TotalAssets = ISNULL(SUM(ClosingBalance), 0) FROM #Assets;

    SELECT 
        @TotalCapital AS TotalCapital,
        @TotalLiabilities AS TotalLiabilities,
        @TotalAssets AS TotalAssets,
        @NetProfit AS NetProfit,
        @NetProfit AS NetProfitLoss,
        @OpeningStock AS OpeningStock,
        @ClosingStock AS ClosingStock,
        ABS(@TotalAssets - (@TotalCapital + @TotalLiabilities + @NetProfit)) AS Difference;

    DROP TABLE #GroupMap;
    DROP TABLE #Liabilities;
    DROP TABLE #Assets;
END
GO
PRINT '[POS_BalanceSheet] updated successfully.';
GO

-- ============================================================================
-- 3. PROCEDURE: _POS_ExecutiveDashboardKPIs
-- Dynamic multi-branch KPI calculations, LowStockAlert parity, cash reconciliation
-- ============================================================================
IF OBJECT_ID('dbo._POS_ExecutiveDashboardKPIs', 'P') IS NOT NULL
    DROP PROCEDURE dbo._POS_ExecutiveDashboardKPIs;
GO

CREATE PROCEDURE [dbo].[_POS_ExecutiveDashboardKPIs]
    @CompanyId INT = 0,
    @BranchId INT = 0,
    @FinYearId INT = 0,
    @FromDate DATETIME,
    @ToDate DATETIME,
    @GroupId INT = 0,
    @CategoryId INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NextDay DATETIME = DATEADD(DAY, 1, CAST(@ToDate AS DATE));
    DECLARE @RangeFrom DATETIME = CAST(@FromDate AS DATE);
    DECLARE @DelayedThresholdDate DATETIME = DATEADD(DAY, -30, @ToDate);
    DECLARE @DeadStockThresholdDate DATETIME = DATEADD(DAY, -90, @ToDate);

    ---------------------------------------------------------------------------
    -- 1. INVENTORY VALUATION & ALERTS (From PriceSettings & ItemMaster)
    ---------------------------------------------------------------------------
    DECLARE @TotalStockCostValue DECIMAL(18,2) = 0,
            @TotalStockRetailValue DECIMAL(18,2) = 0,
            @TotalStockItemCount INT = 0,
            @TotalStockQuantity DECIMAL(18,2) = 0,
            @NegativeStockImpactValue DECIMAL(18,2) = 0,
            @NegativeStockItemCount INT = 0,
            @NegativeStockTotalQty DECIMAL(18,2) = 0,
            @LowStockAlertCount INT = 0,
            @ExcessStockAlertCount INT = 0,
            @ReorderAlertCount INT = 0,
            @DeadStockValue DECIMAL(18,2) = 0,
            @DeadStockItemCount INT = 0;

    IF OBJECT_ID('PriceSettings', 'U') IS NOT NULL
    BEGIN
        SELECT
            @TotalStockCostValue = ISNULL(SUM(ISNULL(ps.Stock, 0) * ISNULL(ps.Cost, 0)), 0),
            @TotalStockRetailValue = ISNULL(SUM(ISNULL(ps.Stock, 0) * ISNULL(ps.RetailPrice, 0)), 0),
            @TotalStockItemCount = ISNULL(COUNT(1), 0),
            @TotalStockQuantity = ISNULL(SUM(ISNULL(ps.Stock, 0)), 0),
            @NegativeStockImpactValue = ISNULL(SUM(CASE WHEN ISNULL(ps.Stock, 0) < 0 THEN ABS(ISNULL(ps.Stock, 0)) * ISNULL(ps.Cost, 0) ELSE 0 END), 0),
            @NegativeStockItemCount = ISNULL(COUNT(CASE WHEN ISNULL(ps.Stock, 0) < 0 THEN 1 END), 0),
            @NegativeStockTotalQty = ISNULL(SUM(CASE WHEN ISNULL(ps.Stock, 0) < 0 THEN ISNULL(ps.Stock, 0) ELSE 0 END), 0),
            @LowStockAlertCount = ISNULL(COUNT(CASE WHEN ISNULL(ps.ReOrder, 0) > 0 AND ISNULL(ps.Stock, 0) <= ISNULL(ps.ReOrder, 0) THEN 1 END), 0),
            @ExcessStockAlertCount = ISNULL(COUNT(CASE WHEN ISNULL(ps.Stock, 0) > 100 THEN 1 END), 0),
            @ReorderAlertCount = ISNULL(COUNT(CASE 
                WHEN ISNULL(ps.Stock, 0) <= CASE 
                    WHEN (ISNULL(st.ADS, 0) * ISNULL(im.Order_Cycle_Days, 7) * 0.5) > ISNULL(ps.ReOrder, 0)
                        THEN (ISNULL(st.ADS, 0) * ISNULL(im.Order_Cycle_Days, 7) * 0.5)
                    ELSE ISNULL(ps.ReOrder, 0)
                END 
                AND (ISNULL(ps.ReOrder, 0) > 0 OR (ISNULL(st.ADS, 0) * ISNULL(im.Order_Cycle_Days, 7) * 0.5) > 0)
                AND sr.ItemId IS NULL
                THEN 1 
            END), 0)
        FROM PriceSettings ps
        INNER JOIN ItemMaster im ON ps.ItemId = im.ItemId
        LEFT JOIN POS_ItemReorderStats_Snapshot st ON ps.ItemId = st.ItemId AND ps.BranchId = st.BranchId AND ps.CompanyId = st.CompanyId
        LEFT JOIN POS_ItemMasterStatusRules sr ON ps.ItemId = sr.ItemId AND UPPER(LTRIM(RTRIM(ISNULL(sr.StatusName, '')))) = 'INACTIVE'
        WHERE (@BranchId = 0 OR ps.BranchId = @BranchId)
          AND ISNULL(ps.IsBaseUnit, 'N') = 'Y'
          AND (@GroupId = 0 OR im.GroupId = @GroupId)
          AND (@CategoryId = 0 OR im.CategoryId = @CategoryId);

        -- Dead Stock: Stock > 0 with no sales transactions in past 90 days
        IF OBJECT_ID('SDetails', 'U') IS NOT NULL AND OBJECT_ID('SMaster', 'U') IS NOT NULL
        BEGIN
            SELECT
                @DeadStockValue = ISNULL(SUM(CASE WHEN ISNULL(ps.Stock, 0) > 0 THEN ISNULL(ps.Stock, 0) * ISNULL(ps.Cost, 0) ELSE 0 END), 0),
                @DeadStockItemCount = ISNULL(COUNT(CASE WHEN ISNULL(ps.Stock, 0) > 0 THEN 1 END), 0)
            FROM PriceSettings ps
            INNER JOIN ItemMaster im ON ps.ItemId = im.ItemId
            WHERE (@BranchId = 0 OR ps.BranchId = @BranchId)
              AND ISNULL(ps.Stock, 0) > 0
              AND ISNULL(ps.IsBaseUnit, 'N') = 'Y'
              AND (@GroupId = 0 OR im.GroupId = @GroupId)
              AND (@CategoryId = 0 OR im.CategoryId = @CategoryId)
              AND ps.ItemId NOT IN (
                  SELECT DISTINCT sd.ItemId
                  FROM SDetails sd
                  INNER JOIN SMaster sm ON sm.BillNo = sd.BillNo AND sm.BranchId = sd.BranchId
                  WHERE sm.BillDate >= @DeadStockThresholdDate AND sm.BillDate < @NextDay
                    AND (@BranchId = 0 OR sm.BranchId = @BranchId)
                    AND ISNULL(sm.CancelFlag, 0) = 0
              );
        END
    END

    ---------------------------------------------------------------------------
    -- 2. STOCK ADJUSTMENTS (Loss / Extra Stock)
    ---------------------------------------------------------------------------
    DECLARE @LossStockValue DECIMAL(18,2) = 0,
            @LossStockQty DECIMAL(18,2) = 0,
            @LossStockItemCount INT = 0,
            @ExtraStockValue DECIMAL(18,2) = 0,
            @ExtraStockQty DECIMAL(18,2) = 0,
            @ExtraStockItemCount INT = 0;

    IF OBJECT_ID('StockAdjustmentMaster', 'U') IS NOT NULL AND OBJECT_ID('StockAdjustmentDetails', 'U') IS NOT NULL
    BEGIN
        SELECT
            @ExtraStockValue = ISNULL(SUM(CASE WHEN ISNULL(sad.QtyDifference, 0) > 0 THEN ISNULL(sad.QtyDifference, 0) * ISNULL(sad.Cost, 0) ELSE 0 END), 0),
            @ExtraStockQty = ISNULL(SUM(CASE WHEN ISNULL(sad.QtyDifference, 0) > 0 THEN ISNULL(sad.QtyDifference, 0) ELSE 0 END), 0),
            @ExtraStockItemCount = ISNULL(COUNT(DISTINCT CASE WHEN ISNULL(sad.QtyDifference, 0) > 0 THEN sad.ItemId END), 0),
            @LossStockValue = ISNULL(SUM(CASE WHEN ISNULL(sad.QtyDifference, 0) < 0 THEN ABS(ISNULL(sad.QtyDifference, 0)) * ISNULL(sad.Cost, 0) ELSE 0 END), 0),
            @LossStockQty = ISNULL(SUM(CASE WHEN ISNULL(sad.QtyDifference, 0) < 0 THEN ABS(ISNULL(sad.QtyDifference, 0)) ELSE 0 END), 0),
            @LossStockItemCount = ISNULL(COUNT(DISTINCT CASE WHEN ISNULL(sad.QtyDifference, 0) < 0 THEN sad.ItemId END), 0)
        FROM StockAdjustmentMaster sam
        INNER JOIN StockAdjustmentDetails sad ON sad.StockAdjustmentMasterId = sam.Id
        INNER JOIN ItemMaster im ON sad.ItemId = im.ItemId
        WHERE sam.StockAdjustmentDate >= @RangeFrom AND sam.StockAdjustmentDate < @NextDay
          AND (@BranchId = 0 OR sam.BranchId = @BranchId)
          AND ISNULL(sam.CancelFlag, 0) = 0
          AND ISNULL(sad.CancelFlag, 0) = 0
          AND (@GroupId = 0 OR im.GroupId = @GroupId)
          AND (@CategoryId = 0 OR im.CategoryId = @CategoryId);
    END

    ---------------------------------------------------------------------------
    -- 3. SALES, PURCHASES, RETURNS & HOLD BILLS
    ---------------------------------------------------------------------------
    DECLARE @TotalSalesRevenue DECIMAL(18,2) = 0,
            @TotalSalesBillCount INT = 0,
            @TotalSalesCost DECIMAL(18,2) = 0,
            @OutputGstAmount DECIMAL(18,2) = 0,
            @TotalPurchases DECIMAL(18,2) = 0,
            @TotalPurchasesBillCount INT = 0,
            @InputGstAmount DECIMAL(18,2) = 0,
            @TotalSalesReturn DECIMAL(18,2) = 0,
            @TotalSalesReturnCount INT = 0,
            @TotalPurchaseReturn DECIMAL(18,2) = 0,
            @TotalPurchaseReturnCount INT = 0,
            @HoldBillsValue DECIMAL(18,2) = 0,
            @HoldBillsCount INT = 0,
            @HoldItemsCount DECIMAL(18,2) = 0;

    IF OBJECT_ID('SMaster', 'U') IS NOT NULL
    BEGIN
        IF (@GroupId = 0 AND @CategoryId = 0)
        BEGIN
            SELECT
                @TotalSalesRevenue = ISNULL(SUM(ISNULL(NetAmount, 0)), 0),
                @TotalSalesBillCount = ISNULL(COUNT(1), 0),
                @TotalSalesCost = ISNULL(SUM(ISNULL(BillCost, 0)), 0),
                @OutputGstAmount = ISNULL(SUM(ISNULL(TaxAmt, 0)), 0)
            FROM SMaster
            WHERE BillDate >= @RangeFrom AND BillDate < @NextDay
              AND (@BranchId = 0 OR BranchId = @BranchId)
              AND ISNULL(CancelFlag, 0) = 0
              AND ISNULL(Status, '') <> 'Hold' AND ISNULL(Status, '') <> 'HOLD';

            SELECT 
                @HoldBillsValue = ISNULL(SUM(ISNULL(NetAmount, 0)), 0),
                @HoldBillsCount = ISNULL(COUNT(1), 0)
            FROM SMaster
            WHERE (@BranchId = 0 OR BranchId = @BranchId)
              AND ISNULL(CancelFlag, 0) = 0
              AND (Status = 'Hold' OR Status = 'HOLD');

            IF OBJECT_ID('SDetails', 'U') IS NOT NULL
            BEGIN
                SELECT @HoldItemsCount = ISNULL(SUM(ISNULL(sd.Qty, 0)), 0)
                FROM SDetails sd
                INNER JOIN SMaster sm ON sm.BillNo = sd.BillNo AND sm.BranchId = sd.BranchId
                WHERE (@BranchId = 0 OR sm.BranchId = @BranchId)
                  AND ISNULL(sm.CancelFlag, 0) = 0
                  AND (sm.Status = 'Hold' OR sm.Status = 'HOLD');
            END
        END
        ELSE
        BEGIN
            -- Filtered by Group / Category
            IF OBJECT_ID('SDetails', 'U') IS NOT NULL
            BEGIN
                SELECT
                    @TotalSalesRevenue = ISNULL(SUM(ISNULL(sd.TotalAmount, sd.Amount)), 0),
                    @TotalSalesBillCount = ISNULL(COUNT(DISTINCT sd.BillNo), 0),
                    @TotalSalesCost = ISNULL(SUM(ISNULL(sd.Cost, 0) * ISNULL(sd.Qty, 0)), 0),
                    @OutputGstAmount = ISNULL(SUM(ISNULL(sd.TaxAmt, 0)), 0)
                FROM SDetails sd
                INNER JOIN SMaster sm ON sm.BillNo = sd.BillNo AND sm.BranchId = sd.BranchId
                INNER JOIN ItemMaster im ON sd.ItemId = im.ItemId
                WHERE sm.BillDate >= @RangeFrom AND sm.BillDate < @NextDay
                  AND (@BranchId = 0 OR sm.BranchId = @BranchId)
                  AND ISNULL(sm.CancelFlag, 0) = 0
                  AND ISNULL(sm.Status, '') <> 'Hold' AND ISNULL(sm.Status, '') <> 'HOLD'
                  AND (@GroupId = 0 OR im.GroupId = @GroupId)
                  AND (@CategoryId = 0 OR im.CategoryId = @CategoryId);

                SELECT 
                    @HoldBillsValue = ISNULL(SUM(ISNULL(sd.TotalAmount, sd.Amount)), 0),
                    @HoldBillsCount = ISNULL(COUNT(DISTINCT sd.BillNo), 0),
                    @HoldItemsCount = ISNULL(SUM(ISNULL(sd.Qty, 0)), 0)
                FROM SDetails sd
                INNER JOIN SMaster sm ON sm.BillNo = sd.BillNo AND sm.BranchId = sd.BranchId
                INNER JOIN ItemMaster im ON sd.ItemId = im.ItemId
                WHERE (@BranchId = 0 OR sm.BranchId = @BranchId)
                  AND ISNULL(sm.CancelFlag, 0) = 0
                  AND (sm.Status = 'Hold' OR sm.Status = 'HOLD')
                  AND (@GroupId = 0 OR im.GroupId = @GroupId)
                  AND (@CategoryId = 0 OR im.CategoryId = @CategoryId);
            END
        END
    END

    -- Dedicated SMasterHold / SDetailsHold tables
    IF OBJECT_ID('SMasterHold', 'U') IS NOT NULL
    BEGIN
        DECLARE @HoldTableValue DECIMAL(18,2) = 0, @HoldTableCount INT = 0, @HoldTableItems DECIMAL(18,2) = 0;
        IF (@GroupId = 0 AND @CategoryId = 0)
        BEGIN
            SELECT 
                @HoldTableValue = ISNULL(SUM(ISNULL(NetAmount, 0)), 0),
                @HoldTableCount = ISNULL(COUNT(1), 0)
            FROM SMasterHold
            WHERE (@BranchId = 0 OR BranchId = @BranchId)
              AND ISNULL(CancelFlag, 0) = 0
              AND ISNULL(IsBilled, 0) = 0;

            IF OBJECT_ID('SDetailsHold', 'U') IS NOT NULL
            BEGIN
                SELECT @HoldTableItems = ISNULL(SUM(ISNULL(sdh.Qty, 0)), 0)
                FROM SDetailsHold sdh
                INNER JOIN SMasterHold smh ON smh.BillNo = sdh.BillNo AND smh.BranchId = sdh.BranchId
                WHERE (@BranchId = 0 OR smh.BranchId = @BranchId)
                  AND ISNULL(smh.CancelFlag, 0) = 0
                  AND ISNULL(smh.IsBilled, 0) = 0;
            END
        END
        ELSE
        BEGIN
            IF OBJECT_ID('SDetailsHold', 'U') IS NOT NULL
            BEGIN
                SELECT 
                    @HoldTableValue = ISNULL(SUM(ISNULL(sdh.TotalAmount, sdh.Amount)), 0),
                    @HoldTableCount = ISNULL(COUNT(DISTINCT sdh.BillNo), 0),
                    @HoldTableItems = ISNULL(SUM(ISNULL(sdh.Qty, 0)), 0)
                FROM SDetailsHold sdh
                INNER JOIN SMasterHold smh ON smh.BillNo = sdh.BillNo AND smh.BranchId = sdh.BranchId
                INNER JOIN ItemMaster im ON sdh.ItemId = im.ItemId
                WHERE (@BranchId = 0 OR smh.BranchId = @BranchId)
                  AND ISNULL(smh.CancelFlag, 0) = 0
                  AND ISNULL(smh.IsBilled, 0) = 0
                  AND (@GroupId = 0 OR im.GroupId = @GroupId)
                  AND (@CategoryId = 0 OR im.CategoryId = @CategoryId);
            END
        END

        SET @HoldBillsValue = @HoldBillsValue + @HoldTableValue;
        SET @HoldBillsCount = @HoldBillsCount + @HoldTableCount;
        SET @HoldItemsCount = @HoldItemsCount + @HoldTableItems;
    END

    -- Purchases
    IF OBJECT_ID('PMaster', 'U') IS NOT NULL
    BEGIN
        IF (@GroupId = 0 AND @CategoryId = 0)
        BEGIN
            SELECT 
                @TotalPurchases = ISNULL(SUM(ISNULL(GrandTotal, 0)), 0),
                @TotalPurchasesBillCount = ISNULL(COUNT(1), 0),
                @InputGstAmount = ISNULL(SUM(ISNULL(TaxAmt, 0)), 0)
            FROM PMaster
            WHERE PurchaseDate >= @RangeFrom AND PurchaseDate < @NextDay
              AND (@BranchId = 0 OR BranchId = @BranchId)
              AND ISNULL(CancelFlag, 0) = 0;
        END
        ELSE
        BEGIN
            IF OBJECT_ID('PDetails', 'U') IS NOT NULL
            BEGIN
                SELECT 
                    @TotalPurchases = ISNULL(SUM(ISNULL(pd.Cost, 0) * ISNULL(pd.Qty, 0) + ISNULL(pd.TaxAmt, 0)), 0),
                    @TotalPurchasesBillCount = ISNULL(COUNT(DISTINCT pd.PurchaseNo), 0),
                    @InputGstAmount = ISNULL(SUM(ISNULL(pd.TaxAmt, 0)), 0)
                FROM PDetails pd
                INNER JOIN PMaster pm ON pm.PurchaseNo = pd.PurchaseNo AND pm.BranchId = pd.BranchID
                INNER JOIN ItemMaster im ON pd.ItemID = im.ItemId
                WHERE pm.PurchaseDate >= @RangeFrom AND pm.PurchaseDate < @NextDay
                  AND (@BranchId = 0 OR pm.BranchId = @BranchId)
                  AND ISNULL(pm.CancelFlag, 0) = 0
                  AND (@GroupId = 0 OR im.GroupId = @GroupId)
                  AND (@CategoryId = 0 OR im.CategoryId = @CategoryId);
            END
        END
    END

    -- Sales Returns
    IF OBJECT_ID('SReturnMaster', 'U') IS NOT NULL
    BEGIN
        IF (@GroupId = 0 AND @CategoryId = 0)
        BEGIN
            SELECT
                @TotalSalesReturn = ISNULL(SUM(ISNULL(GrandTotal, 0)), 0),
                @TotalSalesReturnCount = ISNULL(COUNT(1), 0)
            FROM SReturnMaster
            WHERE SReturnDate >= @RangeFrom AND SReturnDate < @NextDay
              AND (@BranchId = 0 OR BranchId = @BranchId)
              AND ISNULL(CancelFlag, 0) = 0;
        END
        ELSE
        BEGIN
            IF OBJECT_ID('SReturnDetails', 'U') IS NOT NULL
            BEGIN
                SELECT
                    @TotalSalesReturn = ISNULL(SUM(ISNULL(srd.Amount, 0) + ISNULL(srd.TaxAmt, 0)), 0),
                    @TotalSalesReturnCount = ISNULL(COUNT(DISTINCT srd.SReturnNo), 0)
                FROM SReturnDetails srd
                INNER JOIN SReturnMaster srm ON srm.SReturnNo = srd.SReturnNo AND srm.BranchId = srd.BranchID
                INNER JOIN ItemMaster im ON srd.ItemID = im.ItemId
                WHERE srm.SReturnDate >= @RangeFrom AND srm.SReturnDate < @NextDay
                  AND (@BranchId = 0 OR srm.BranchId = @BranchId)
                  AND ISNULL(srm.CancelFlag, 0) = 0
                  AND (@GroupId = 0 OR im.GroupId = @GroupId)
                  AND (@CategoryId = 0 OR im.CategoryId = @CategoryId);
            END
        END
    END

    -- Purchase Returns
    IF OBJECT_ID('PReturnMaster', 'U') IS NOT NULL
    BEGIN
        IF (@GroupId = 0 AND @CategoryId = 0)
        BEGIN
            SELECT
                @TotalPurchaseReturn = ISNULL(SUM(ISNULL(GrandTotal, 0)), 0),
                @TotalPurchaseReturnCount = ISNULL(COUNT(1), 0)
            FROM PReturnMaster
            WHERE PReturnDate >= @RangeFrom AND PReturnDate < @NextDay
              AND (@BranchId = 0 OR BranchId = @BranchId)
              AND ISNULL(CancelFlag, 0) = 0;
        END
        ELSE
        BEGIN
            IF OBJECT_ID('PReturnDetails', 'U') IS NOT NULL
            BEGIN
                SELECT
                    @TotalPurchaseReturn = ISNULL(SUM(ISNULL(prd.Cost, 0) * ISNULL(prd.Returnqty, 0) + ISNULL(prd.TaxAmt, 0)), 0),
                    @TotalPurchaseReturnCount = ISNULL(COUNT(DISTINCT prd.PReturnNo), 0)
                FROM PReturnDetails prd
                INNER JOIN PReturnMaster prm ON prm.PReturnNo = prd.PReturnNo AND prm.BranchId = prd.BranchID
                INNER JOIN ItemMaster im ON prd.ItemID = im.ItemId
                WHERE prm.PReturnDate >= @RangeFrom AND prm.PReturnDate < @NextDay
                  AND (@BranchId = 0 OR prm.BranchId = @BranchId)
                  AND ISNULL(prm.CancelFlag, 0) = 0
                  AND (@GroupId = 0 OR im.GroupId = @GroupId)
                  AND (@CategoryId = 0 OR im.CategoryId = @CategoryId);
            END
        END
    END

    IF @TotalSalesCost = 0 AND @TotalSalesRevenue > 0
        SET @TotalSalesCost = @TotalSalesRevenue * 0.70;

    DECLARE @GrossProfit DECIMAL(18,2) = CASE WHEN @TotalSalesRevenue - @TotalSalesCost > 0 THEN @TotalSalesRevenue - @TotalSalesCost ELSE 0 END;
    DECLARE @GrossProfitMarginPercent DECIMAL(18,2) = CASE WHEN @TotalSalesRevenue > 0 THEN ROUND((@GrossProfit / @TotalSalesRevenue) * 100.0, 2) ELSE 0 END;
    DECLARE @NetTaxLiability DECIMAL(18,2) = @OutputGstAmount - @InputGstAmount;

    ---------------------------------------------------------------------------
    -- 4. EXPENSES, DRAWINGS & WRITE-OFFS (From Vouchers)
    ---------------------------------------------------------------------------
    DECLARE @DirectExpenses DECIMAL(18,2) = 0,
            @IndirectExpenses DECIMAL(18,2) = 0,
            @OwnerDrawings DECIMAL(18,2) = 0,
            @CustomerBadDebts DECIMAL(18,2) = 0,
            @SupplierWriteOffs DECIMAL(18,2) = 0;

    IF OBJECT_ID('Vouchers', 'U') IS NOT NULL AND OBJECT_ID('AccountGroupMaster', 'U') IS NOT NULL
    BEGIN
        SELECT
            @DirectExpenses = ISNULL(SUM(CASE WHEN ag.GroupName LIKE '%Direct Expense%' AND ag.GroupName NOT LIKE '%Indirect%' THEN ISNULL(v.Debit, 0) ELSE 0 END), 0),
            @IndirectExpenses = ISNULL(SUM(CASE WHEN ag.GroupName LIKE '%Indirect Expense%' OR (ag.GroupName LIKE '%Expense%' AND ag.GroupName NOT LIKE '%Direct%') THEN ISNULL(v.Debit, 0) ELSE 0 END), 0),
            @OwnerDrawings = ISNULL(SUM(CASE WHEN ag.GroupName LIKE '%Drawings%' OR v.LedgerName LIKE '%Drawings%' THEN ISNULL(v.Debit, 0) ELSE 0 END), 0),
            @CustomerBadDebts = ISNULL(SUM(CASE WHEN v.LedgerName LIKE '%Bad Debt%' THEN ISNULL(v.Debit, 0) ELSE 0 END), 0),
            @SupplierWriteOffs = ISNULL(SUM(CASE WHEN v.LedgerName LIKE '%Discount Received%' OR v.LedgerName LIKE '%Write Off%' THEN ISNULL(v.Credit, 0) ELSE 0 END), 0)
        FROM Vouchers v
        LEFT JOIN LedgerMaster lm ON lm.LedgerID = v.LedgerID
        LEFT JOIN (SELECT DISTINCT GroupID, GroupName FROM AccountGroupMaster) ag ON ag.GroupID = lm.GroupID
        WHERE v.VoucherDate >= @RangeFrom AND v.VoucherDate < @NextDay
          AND (@BranchId = 0 OR v.BranchID = @BranchId)
          AND ISNULL(v.CancelFlag, 0) = 0;
    END

    DECLARE @TotalExpenses DECIMAL(18,2) = @DirectExpenses + @IndirectExpenses;
    
    -- When filtered by Category, show Category Contribution (Gross Profit), else bottom-line Net Profit
    DECLARE @ActualNetProfit DECIMAL(18,2) = 0;
    DECLARE @OperatingMarginPercent DECIMAL(18,2) = 0;
    IF (@GroupId = 0 AND @CategoryId = 0)
    BEGIN
        SET @ActualNetProfit = @GrossProfit - @TotalExpenses;
        IF @TotalSalesRevenue > 0
            SET @OperatingMarginPercent = ROUND((@ActualNetProfit / @TotalSalesRevenue) * 100.0, 2);
    END
    ELSE
    BEGIN
        SET @ActualNetProfit = @GrossProfit;
        SET @OperatingMarginPercent = @GrossProfitMarginPercent;
    END

    ---------------------------------------------------------------------------
    -- 5. CASH, BANK, RECEIVABLES & PAYABLES (Store-Wide)
    ---------------------------------------------------------------------------
    DECLARE @CashInHand DECIMAL(18,2) = 0,
            @BankBalance DECIMAL(18,2) = 0,
            @SupplierPayables DECIMAL(18,2) = 0,
            @SupplierPayablesCount INT = 0,
            @CustomerReceivables DECIMAL(18,2) = 0,
            @CustomerReceivablesCount INT = 0,
            @SupplierAdvanceBalance DECIMAL(18,2) = 0,
            @CustomerAdvanceBalance DECIMAL(18,2) = 0;

    -- Cash & Bank Ledgers
    IF OBJECT_ID('Vouchers', 'U') IS NOT NULL AND OBJECT_ID('AccountGroupMaster', 'U') IS NOT NULL
    BEGIN
        SELECT
            @CashInHand = ISNULL(SUM(CASE WHEN ag.GroupName LIKE '%Cash%' THEN ISNULL(v.Debit, 0) - ISNULL(v.Credit, 0) ELSE 0 END), 0),
            @BankBalance = ISNULL(SUM(CASE WHEN ag.GroupName LIKE '%Bank%' THEN ISNULL(v.Debit, 0) - ISNULL(v.Credit, 0) ELSE 0 END), 0)
        FROM Vouchers v
        LEFT JOIN LedgerMaster lm ON lm.LedgerID = v.LedgerID
        LEFT JOIN (SELECT DISTINCT GroupID, GroupName FROM AccountGroupMaster) ag ON ag.GroupID = lm.GroupID
        WHERE (@BranchId = 0 OR v.BranchID = @BranchId)
          AND ISNULL(v.CancelFlag, 0) = 0
          AND v.VoucherDate < @NextDay;

        SELECT
            @CashInHand = @CashInHand + ISNULL(SUM(CASE WHEN ag.GroupName LIKE '%Cash%' THEN ISNULL(lm.OpnDebit, 0) - ISNULL(lm.OpnCredit, 0) ELSE 0 END), 0),
            @BankBalance = @BankBalance + ISNULL(SUM(CASE WHEN ag.GroupName LIKE '%Bank%' THEN ISNULL(lm.OpnDebit, 0) - ISNULL(lm.OpnCredit, 0) ELSE 0 END), 0)
        FROM LedgerMaster lm
        LEFT JOIN (SELECT DISTINCT GroupID, GroupName FROM AccountGroupMaster) ag ON ag.GroupID = lm.GroupID
        WHERE (@BranchId = 0 OR lm.BranchID = @BranchId);
    END

    -- Receivables & Payables
    IF OBJECT_ID('SMaster', 'U') IS NOT NULL
    BEGIN
        SELECT
            @CustomerReceivables = ISNULL(SUM(ISNULL(NetAmount, 0) - ISNULL(ReceivedAmount, 0)), 0),
            @CustomerReceivablesCount = ISNULL(COUNT(1), 0)
        FROM SMaster
        WHERE (@BranchId = 0 OR BranchId = @BranchId)
          AND ISNULL(CancelFlag, 0) = 0
          AND (ISNULL(NetAmount, 0) - ISNULL(ReceivedAmount, 0)) > 0
          AND BillDate < @NextDay;
    END

    IF OBJECT_ID('PMaster', 'U') IS NOT NULL
    BEGIN
        SELECT
            @SupplierPayables = ISNULL(SUM(ISNULL(GrandTotal, 0) - ISNULL(PayedAmount, 0)), 0),
            @SupplierPayablesCount = ISNULL(COUNT(1), 0)
        FROM PMaster
        WHERE (@BranchId = 0 OR BranchId = @BranchId)
          AND ISNULL(CancelFlag, 0) = 0
          AND (ISNULL(GrandTotal, 0) - ISNULL(PayedAmount, 0)) > 0
          AND PurchaseDate < @NextDay;
    END

    ---------------------------------------------------------------------------
    -- 6. BALANCE SHEET NET ASSETS (Total Assets vs Total Liabilities)
    ---------------------------------------------------------------------------
    DECLARE @TotalAssets DECIMAL(18,2) = 0,
            @TotalLiabilities DECIMAL(18,2) = 0,
            @NetBusinessAsset DECIMAL(18,2) = 0;

    DECLARE @LoanLiabilities DECIMAL(18,2) = 0;
    IF OBJECT_ID('LedgerMaster', 'U') IS NOT NULL AND OBJECT_ID('AccountGroupMaster', 'U') IS NOT NULL
    BEGIN
        SELECT @LoanLiabilities = ISNULL(SUM((ISNULL(lm.OpnCredit,0) + ISNULL(v.Credit,0)) - (ISNULL(lm.OpnDebit,0) + ISNULL(v.Debit,0))), 0)
        FROM LedgerMaster lm
        INNER JOIN (SELECT DISTINCT GroupID, GroupName, GroupType FROM AccountGroupMaster) ag ON lm.GroupID = ag.GroupID
        LEFT JOIN (
            SELECT LedgerID, SUM(ISNULL(Debit,0)) AS Debit, SUM(ISNULL(Credit,0)) AS Credit
            FROM Vouchers
            WHERE (@BranchId = 0 OR BranchID = @BranchId)
              AND VoucherDate < @NextDay
              AND ISNULL(CancelFlag,0) = 0
            GROUP BY LedgerID
        ) v ON lm.LedgerID = v.LedgerID
        WHERE (@BranchId = 0 OR lm.BranchID = @BranchId)
          AND (ag.GroupName LIKE '%Loan%' OR ag.GroupType = 'LIABILITIES')
          AND ag.GroupName NOT LIKE '%Capital%'
          AND ag.GroupID NOT IN (1, 17);
    END

    SET @TotalAssets = @TotalStockCostValue + @CashInHand + @BankBalance + @CustomerReceivables;
    SET @TotalLiabilities = @SupplierPayables + CASE WHEN @LoanLiabilities > 0 THEN @LoanLiabilities ELSE 0 END + CASE WHEN @NetTaxLiability > 0 THEN @NetTaxLiability ELSE 0 END;
    IF @TotalLiabilities < 0 SET @TotalLiabilities = 0;
    SET @NetBusinessAsset = @TotalAssets - @TotalLiabilities;

    DECLARE @DelayedCustomerReceivables30Days DECIMAL(18,2) = 0,
            @DelayedCustomerCount30Days INT = 0,
            @DelayedSupplierPayables30Days DECIMAL(18,2) = 0,
            @DelayedSupplierCount30Days INT = 0;

    IF OBJECT_ID('SMaster', 'U') IS NOT NULL
    BEGIN
        SELECT
            @DelayedCustomerReceivables30Days = ISNULL(SUM(ISNULL(NetAmount, 0) - ISNULL(ReceivedAmount, 0)), 0),
            @DelayedCustomerCount30Days = ISNULL(COUNT(1), 0)
        FROM SMaster
        WHERE BillDate <= @DelayedThresholdDate
          AND (@BranchId = 0 OR BranchId = @BranchId)
          AND ISNULL(CancelFlag, 0) = 0
          AND (ISNULL(NetAmount, 0) - ISNULL(ReceivedAmount, 0)) > 0;
    END

    IF OBJECT_ID('PMaster', 'U') IS NOT NULL
    BEGIN
        SELECT
            @DelayedSupplierPayables30Days = ISNULL(SUM(ISNULL(GrandTotal, 0) - ISNULL(PayedAmount, 0)), 0),
            @DelayedSupplierCount30Days = ISNULL(COUNT(1), 0)
        FROM PMaster
        WHERE PurchaseDate <= @DelayedThresholdDate
          AND (@BranchId = 0 OR BranchId = @BranchId)
          AND ISNULL(CancelFlag, 0) = 0
          AND (ISNULL(GrandTotal, 0) - ISNULL(PayedAmount, 0)) > 0;
    END

    ---------------------------------------------------------------------------
    -- 7. AUDIT COUNTS
    ---------------------------------------------------------------------------
    DECLARE @DeletionCount INT = 0,
            @PriceChangeCount INT = 0,
            @StockAdjustmentCount INT = 0;

    IF OBJECT_ID('ItemActivityLog', 'U') IS NOT NULL
    BEGIN
        SELECT @PriceChangeCount = ISNULL(COUNT(1), 0)
        FROM dbo.ItemActivityLog ial
        LEFT JOIN ItemMaster im ON ial.ItemId = im.ItemId
        WHERE ial.CreatedOn >= @RangeFrom AND ial.CreatedOn < @NextDay
          AND (@GroupId = 0 OR im.GroupId = @GroupId)
          AND (@CategoryId = 0 OR im.CategoryId = @CategoryId)
          AND (
              ial.ActivityType IN ('UPDATE', 'PRICE CHANGE', 'PRICE_CHANGE')
              OR ial.ActivityDetails LIKE '%Price%'
              OR ial.ActivityDetails LIKE '%Cost%'
              OR ial.ActivityDetails LIKE '%MRP%'
              OR ial.ActivityDetails LIKE '%Mark Up%'
          );
    END
    ELSE IF OBJECT_ID('tblAuditTrail', 'U') IS NOT NULL
    BEGIN
        SELECT @PriceChangeCount = ISNULL(COUNT(1), 0) 
        FROM tblAuditTrail 
        WHERE (ActionType LIKE '%Price%' OR ActionType LIKE '%Edit%') 
          AND ActionDate >= @RangeFrom AND ActionDate < @NextDay;
    END

    DECLARE @CancelledSales INT = 0,
            @CancelledPurchases INT = 0,
            @CancelledSReturn INT = 0,
            @CancelledPReturn INT = 0,
            @CancelledStockAdj INT = 0,
            @CancelledVouchers INT = 0,
            @ActivityLogDeletions INT = 0;

    IF OBJECT_ID('SMaster', 'U') IS NOT NULL
        SELECT @CancelledSales = COUNT(1) FROM SMaster WHERE CancelFlag = 1 AND BillDate >= @RangeFrom AND BillDate < @NextDay AND (@BranchId = 0 OR BranchId = @BranchId);

    IF OBJECT_ID('PMaster', 'U') IS NOT NULL
        SELECT @CancelledPurchases = COUNT(1) FROM PMaster WHERE CancelFlag = 1 AND PurchaseDate >= @RangeFrom AND PurchaseDate < @NextDay AND (@BranchId = 0 OR BranchId = @BranchId);

    IF OBJECT_ID('SReturnMaster', 'U') IS NOT NULL
        SELECT @CancelledSReturn = COUNT(1) FROM SReturnMaster WHERE CancelFlag = 1 AND SReturnDate >= @RangeFrom AND SReturnDate < @NextDay AND (@BranchId = 0 OR BranchId = @BranchId);

    IF OBJECT_ID('PReturnMaster', 'U') IS NOT NULL
        SELECT @CancelledPReturn = COUNT(1) FROM PReturnMaster WHERE CancelFlag = 1 AND PReturnDate >= @RangeFrom AND PReturnDate < @NextDay AND (@BranchId = 0 OR BranchId = @BranchId);

    IF OBJECT_ID('StockAdjustmentMaster', 'U') IS NOT NULL
        SELECT @CancelledStockAdj = COUNT(1) FROM StockAdjustmentMaster WHERE CancelFlag = 1 AND StockAdjustmentDate >= @RangeFrom AND StockAdjustmentDate < @NextDay AND (@BranchId = 0 OR BranchId = @BranchId);

    IF OBJECT_ID('Vouchers', 'U') IS NOT NULL
        SELECT @CancelledVouchers = COUNT(1) FROM Vouchers WHERE CancelFlag = 1 AND VoucherDate >= @RangeFrom AND VoucherDate < @NextDay AND (@BranchId = 0 OR BranchID = @BranchId);

    IF OBJECT_ID('UserActivityLog', 'U') IS NOT NULL
        SELECT @ActivityLogDeletions = COUNT(1) FROM UserActivityLog WHERE CreatedOn >= @RangeFrom AND CreatedOn < @NextDay AND (ActivityType LIKE '%Delete%' OR ActivityType LIKE '%Cancel%' OR ActivityDetails LIKE '%Delete%' OR ActivityDetails LIKE '%Cancel%');

    SET @DeletionCount = @CancelledSales + @CancelledPurchases + @CancelledSReturn + @CancelledPReturn + @CancelledStockAdj + @CancelledVouchers;
    IF @DeletionCount = 0 OR @ActivityLogDeletions > @DeletionCount
        SET @DeletionCount = @ActivityLogDeletions;

    IF @DeletionCount = 0 AND OBJECT_ID('tblAuditTrail', 'U') IS NOT NULL
        SELECT @DeletionCount = ISNULL(COUNT(1), 0) FROM tblAuditTrail WHERE (ActionType LIKE '%Delete%' OR ActionType LIKE '%Cancel%') AND ActionDate >= @RangeFrom AND ActionDate < @NextDay;

    IF OBJECT_ID('StockAdjustmentMaster', 'U') IS NOT NULL
        SELECT @StockAdjustmentCount = ISNULL(COUNT(1), 0) FROM StockAdjustmentMaster WHERE StockAdjustmentDate >= @RangeFrom AND StockAdjustmentDate < @NextDay AND ISNULL(CancelFlag, 0) = 0;

    ---------------------------------------------------------------------------
    -- 9. FULL CASH IN & CASH OUT (REAL CASH FLOW)
    ---------------------------------------------------------------------------
    DECLARE @CashInCounterCash DECIMAL(18,2) = 0,
            @CashInCounterDigital DECIMAL(18,2) = 0,
            @CashInCustomerReceipts DECIMAL(18,2) = 0,
            @CashInCustomerReceiptsCount INT = 0,
            @CashInGeneralReceipts DECIMAL(18,2) = 0,
            @CashInTotal DECIMAL(18,2) = 0;

    IF OBJECT_ID('SMaster', 'U') IS NOT NULL
    BEGIN
        SELECT 
            @CashInCounterCash = ISNULL(SUM(CASE WHEN PaymodeName LIKE '%Cash%' THEN NetAmount ELSE 0 END), 0),
            @CashInCounterDigital = ISNULL(SUM(CASE WHEN PaymodeName NOT LIKE '%Cash%' AND PaymodeName NOT LIKE '%Credit%' THEN NetAmount ELSE 0 END), 0)
        FROM SMaster
        WHERE BillDate >= @RangeFrom AND BillDate < @NextDay
          AND (@BranchId = 0 OR BranchId = @BranchId)
          AND ISNULL(CancelFlag, 0) = 0
          AND ISNULL(Status, '') NOT IN ('Hold', 'HOLD');
    END

    IF OBJECT_ID('CustomerReceiptMaster', 'U') IS NOT NULL
    BEGIN
        SELECT 
            @CashInCustomerReceipts = ISNULL(SUM(ReceiptAmount), 0),
            @CashInCustomerReceiptsCount = ISNULL(COUNT(1), 0)
        FROM CustomerReceiptMaster
        WHERE VoucherDate >= @RangeFrom AND VoucherDate < @NextDay
          AND (@BranchId = 0 OR BranchId = @BranchId)
          AND ISNULL(CancelFlag, 0) = 0;
    END

    IF OBJECT_ID('Vouchers', 'U') IS NOT NULL
    BEGIN
        SELECT 
            @CashInGeneralReceipts = ISNULL(SUM(Credit), 0)
        FROM Vouchers
        WHERE VoucherDate >= @RangeFrom AND VoucherDate < @NextDay
          AND (@BranchId = 0 OR BranchID = @BranchId)
          AND ISNULL(CancelFlag, 0) = 0
          AND VoucherType IN ('GENREC', 'Receipt', 'RECEIPT', 'Receipt Voucher', 'Cash Receipt');
    END

    SET @CashInTotal = @CashInCounterCash + @CashInCounterDigital + @CashInCustomerReceipts + @CashInGeneralReceipts;

    DECLARE @CashOutVendorPayments DECIMAL(18,2) = 0,
            @CashOutVendorPaymentsCount INT = 0,
            @CashOutCashPurchases DECIMAL(18,2) = 0,
            @CashOutExpenses DECIMAL(18,2) = 0,
            @CashOutCustomerRefunds DECIMAL(18,2) = 0,
            @CashOutTotal DECIMAL(18,2) = 0,
            @NetCashFlow DECIMAL(18,2) = 0;

    IF OBJECT_ID('VendorPaymentMaster', 'U') IS NOT NULL
    BEGIN
        SELECT 
            @CashOutVendorPayments = ISNULL(SUM(PaymentAmount), 0),
            @CashOutVendorPaymentsCount = ISNULL(COUNT(1), 0)
        FROM VendorPaymentMaster
        WHERE VoucherDate >= @RangeFrom AND VoucherDate < @NextDay
          AND (@BranchId = 0 OR BranchId = @BranchId)
          AND ISNULL(CancelFlag, 0) = 0;
    END

    IF OBJECT_ID('PMaster', 'U') IS NOT NULL
    BEGIN
        SELECT 
            @CashOutCashPurchases = ISNULL(SUM(GrandTotal), 0)
        FROM PMaster
        WHERE PurchaseDate >= @RangeFrom AND PurchaseDate < @NextDay
          AND (@BranchId = 0 OR BranchId = @BranchId)
          AND ISNULL(CancelFlag, 0) = 0
          AND (Paymode LIKE '%Cash%' OR Paymode LIKE '%CASH%');
    END

        IF OBJECT_ID('Vouchers', 'U') IS NOT NULL
    BEGIN
        SELECT 
            @CashOutExpenses = ISNULL(SUM(Debit), 0)
        FROM Vouchers
        WHERE VoucherDate >= @RangeFrom AND VoucherDate < @NextDay
          AND (@BranchId = 0 OR BranchID = @BranchId)
          AND ISNULL(CancelFlag, 0) = 0
          AND VoucherType IN ('GENPAY', 'PETTYC', 'EXPENSE', 'Payment', 'PAYMENT', 'Expense Payment');
    END
    ELSE
    BEGIN
        SET @CashOutExpenses = @TotalExpenses;
    END

    IF OBJECT_ID('SReturnMaster', 'U') IS NOT NULL
    BEGIN
        SELECT 
            @CashOutCustomerRefunds = ISNULL(SUM(GrandTotal), 0)
        FROM SReturnMaster
        WHERE SReturnDate >= @RangeFrom AND SReturnDate < @NextDay
          AND (@BranchId = 0 OR BranchId = @BranchId)
          AND ISNULL(CancelFlag, 0) = 0;
    END

    SET @CashOutTotal = @CashOutVendorPayments + @CashOutCashPurchases + @CashOutExpenses + @CashOutCustomerRefunds + @OwnerDrawings;
    SET @NetCashFlow = @CashInTotal - @CashOutTotal;

    DECLARE @OpeningCashBank DECIMAL(18,2) = 0,
            @ClosingCashBank DECIMAL(18,2) = 0;

    IF OBJECT_ID('Vouchers', 'U') IS NOT NULL AND OBJECT_ID('AccountGroupMaster', 'U') IS NOT NULL
    BEGIN
        ;WITH CashBankLedgers AS (
            SELECT lm.LedgerID, (ISNULL(lm.OpnDebit, 0) - ISNULL(lm.OpnCredit, 0)) AS MasterOpn
            FROM LedgerMaster lm
            LEFT JOIN (SELECT DISTINCT GroupID, GroupName FROM AccountGroupMaster) ag ON ag.GroupID = lm.GroupID
            WHERE (@BranchId = 0 OR lm.BranchID = @BranchId)
              AND (ag.GroupName LIKE '%Cash%' OR ag.GroupName LIKE '%Bank%')
        )
        SELECT
            @OpeningCashBank = ISNULL(SUM(cbl.MasterOpn), 0) + ISNULL(SUM(v_opn.NetAmt), 0),
            @ClosingCashBank = ISNULL(SUM(cbl.MasterOpn), 0) + ISNULL(SUM(v_cls.NetAmt), 0)
        FROM CashBankLedgers cbl
        LEFT JOIN (
            SELECT v.LedgerID, SUM(ISNULL(v.Debit, 0) - ISNULL(v.Credit, 0)) AS NetAmt
            FROM Vouchers v
            WHERE (@BranchId = 0 OR v.BranchID = @BranchId)
              AND ISNULL(v.CancelFlag, 0) = 0
              AND v.VoucherDate < @RangeFrom
            GROUP BY v.LedgerID
        ) v_opn ON v_opn.LedgerID = cbl.LedgerID
        LEFT JOIN (
            SELECT v.LedgerID, SUM(ISNULL(v.Debit, 0) - ISNULL(v.Credit, 0)) AS NetAmt
            FROM Vouchers v
            WHERE (@BranchId = 0 OR v.BranchID = @BranchId)
              AND ISNULL(v.CancelFlag, 0) = 0
              AND v.VoucherDate < @NextDay
            GROUP BY v.LedgerID
        ) v_cls ON v_cls.LedgerID = cbl.LedgerID;
    END

    ---------------------------------------------------------------------------
    -- RESULT SET 1: ALL KPI SCALARS
    ---------------------------------------------------------------------------
    SELECT
        @TotalStockCostValue AS TotalStockCostValue,
        @TotalStockRetailValue AS TotalStockRetailValue,
        @TotalStockItemCount AS TotalStockItemCount,
        @TotalStockQuantity AS TotalStockQuantity,
        @NegativeStockImpactValue AS NegativeStockImpactValue,
        @NegativeStockItemCount AS NegativeStockItemCount,
        @NegativeStockTotalQty AS NegativeStockTotalQty,
        @LowStockAlertCount AS LowStockAlertCount,
        @ExcessStockAlertCount AS ExcessStockAlertCount,
        @ReorderAlertCount AS ReorderAlertCount,
        @DeadStockValue AS DeadStockValue,
        @DeadStockItemCount AS DeadStockItemCount,
        @LossStockValue AS LossStockValue,
        @LossStockQty AS LossStockQty,
        @LossStockItemCount AS LossStockItemCount,
        @ExtraStockValue AS ExtraStockValue,
        @ExtraStockQty AS ExtraStockQty,
        @ExtraStockItemCount AS ExtraStockItemCount,
        (@ExtraStockValue - @LossStockValue) AS NetStockAdjustmentValue,
        @TotalSalesCost AS CostOfGoodsSold,
        @TotalSalesRevenue AS TotalSalesRevenue,
        @TotalSalesBillCount AS TotalSalesBillCount,
        @TotalSalesReturn AS TotalSalesReturn,
        @TotalSalesReturnCount AS TotalSalesReturnCount,
        @TotalPurchases AS TotalPurchases,
        @TotalPurchasesBillCount AS TotalPurchasesBillCount,
        @TotalPurchaseReturn AS TotalPurchaseReturn,
        @TotalPurchaseReturnCount AS TotalPurchaseReturnCount,
        @HoldBillsValue AS HoldBillsValue,
        @HoldBillsCount AS HoldBillsCount,
        @HoldItemsCount AS HoldItemsCount,
        @GrossProfit AS GrossProfit,
        @GrossProfitMarginPercent AS GrossProfitMarginPercent,
        @OutputGstAmount AS OutputGstAmount,
        @InputGstAmount AS InputGstAmount,
        @NetTaxLiability AS NetTaxLiability,
        @DirectExpenses AS DirectExpenses,
        @IndirectExpenses AS IndirectExpenses,
        @TotalExpenses AS TotalBusinessExpenses,
        @ActualNetProfit AS ActualNetProfit,
        @OperatingMarginPercent AS OperatingProfitMarginPercent,
        @OwnerDrawings AS OwnerDrawings,
        @CustomerBadDebts AS CustomerBadDebts,
        @SupplierWriteOffs AS SupplierWriteOffs,
        @CashInHand AS CashInHand,
        @BankBalance AS BankBalance,
        @SupplierPayables AS SupplierPayables,
        @SupplierPayablesCount AS SupplierPayablesCount,
        @CustomerReceivables AS CustomerReceivables,
        @CustomerReceivablesCount AS CustomerReceivablesCount,
        @SupplierAdvanceBalance AS SupplierAdvanceBalance,
        @CustomerAdvanceBalance AS CustomerAdvanceBalance,
        @TotalAssets AS TotalAssets,
        @TotalLiabilities AS TotalLiabilities,
        @NetBusinessAsset AS NetBusinessAsset,
        @DelayedCustomerReceivables30Days AS DelayedCustomerReceivables30Days,
        @DelayedCustomerCount30Days AS DelayedCustomerCount30Days,
        @DelayedSupplierPayables30Days AS DelayedSupplierPayables30Days,
        @DelayedSupplierCount30Days AS DelayedSupplierCount30Days,
        @DeletionCount AS DeletionCount,
        @PriceChangeCount AS PriceChangeCount,
        @StockAdjustmentCount AS StockAdjustmentCount,
        @CashInCounterCash AS CashInCounterCash,
        @CashInCounterDigital AS CashInCounterDigital,
        @CashInCustomerReceipts AS CashInCustomerReceipts,
        @CashInCustomerReceiptsCount AS CashInCustomerReceiptsCount,
        @CashInGeneralReceipts AS CashInGeneralReceipts,
        @CashInTotal AS CashInTotal,
        @CashOutVendorPayments AS CashOutVendorPayments,
        @CashOutVendorPaymentsCount AS CashOutVendorPaymentsCount,
        @CashOutCashPurchases AS CashOutCashPurchases,
        @CashOutExpenses AS CashOutExpenses,
        @CashOutCustomerRefunds AS CashOutCustomerRefunds,
        @OwnerDrawings AS CashOutOwnerDrawings,
        @CashOutTotal AS CashOutTotal,
        @NetCashFlow AS NetCashFlow,
        @OpeningCashBank AS OpeningCashBank,
        @ClosingCashBank AS ClosingCashBank;

    ---------------------------------------------------------------------------
    -- RESULT SET 2: MONTHLY GROWTH MATRIX
    ---------------------------------------------------------------------------
    IF OBJECT_ID('SMaster', 'U') IS NOT NULL
    BEGIN
        IF (@GroupId = 0 AND @CategoryId = 0)
        BEGIN
            SELECT
                DATENAME(MONTH, s.BillDate) + ' ' + CAST(YEAR(s.BillDate) AS VARCHAR(4)) AS MonthName,
                YEAR(s.BillDate) AS [Year],
                MONTH(s.BillDate) AS [Month],
                ISNULL(SUM(ISNULL(s.NetAmount, 0)), 0) AS SalesAmount,
                0.0 AS SalesGrowthPercent,
                ISNULL((SELECT SUM(ISNULL(p.GrandTotal,0)) FROM PMaster p WHERE YEAR(p.PurchaseDate) = YEAR(s.BillDate) AND MONTH(p.PurchaseDate) = MONTH(s.BillDate) AND ISNULL(p.CancelFlag,0)=0 AND (@BranchId = 0 OR p.BranchId = @BranchId)), 0) AS PurchaseAmount,
                ISNULL((
                    SELECT SUM(ISNULL(v.Debit, 0)) 
                    FROM Vouchers v
                    INNER JOIN LedgerMaster lm ON lm.LedgerID = v.LedgerID
                    LEFT JOIN (SELECT DISTINCT GroupID, GroupName FROM AccountGroupMaster) ag ON ag.GroupID = lm.GroupID
                    WHERE YEAR(v.VoucherDate) = YEAR(s.BillDate) AND MONTH(v.VoucherDate) = MONTH(s.BillDate)
                      AND ag.GroupName LIKE '%Expense%'
                      AND ISNULL(v.CancelFlag, 0) = 0
                      AND (@BranchId = 0 OR v.BranchID = @BranchId)
                ), 0) AS ExpenseAmount,
                ISNULL((SELECT SUM(ISNULL(sd.TotalAmount, sd.Amount) - (ISNULL(sd.Cost, 0) * ISNULL(sd.Qty, 0))) FROM SDetails sd INNER JOIN SMaster sm ON sm.BillNo = sd.BillNo AND sm.BranchId = sd.BranchId WHERE YEAR(sm.BillDate) = YEAR(s.BillDate) AND MONTH(sm.BillDate) = MONTH(s.BillDate) AND ISNULL(sm.CancelFlag, 0) = 0 AND (@BranchId = 0 OR sm.BranchId = @BranchId)), 0) - ISNULL((
                    SELECT SUM(ISNULL(v.Debit, 0)) 
                    FROM Vouchers v
                    INNER JOIN LedgerMaster lm ON lm.LedgerID = v.LedgerID
                    LEFT JOIN (SELECT DISTINCT GroupID, GroupName FROM AccountGroupMaster) ag ON ag.GroupID = lm.GroupID
                    WHERE YEAR(v.VoucherDate) = YEAR(s.BillDate) AND MONTH(v.VoucherDate) = MONTH(s.BillDate)
                      AND ag.GroupName LIKE '%Expense%'
                      AND ISNULL(v.CancelFlag, 0) = 0
                      AND (@BranchId = 0 OR v.BranchID = @BranchId)
                ), 0) AS NetProfitAmount,
                0.0 AS NetProfitGrowthPercent
            FROM SMaster s
            WHERE s.BillDate >= @RangeFrom AND s.BillDate < @NextDay
              AND (@BranchId = 0 OR s.BranchId = @BranchId)
              AND ISNULL(s.CancelFlag, 0) = 0
            GROUP BY YEAR(s.BillDate), MONTH(s.BillDate), DATENAME(MONTH, s.BillDate)
            ORDER BY [Year] ASC, [Month] ASC;
        END
        ELSE
        BEGIN
            -- Monthly matrix filtered by Group / Category
            SELECT
                DATENAME(MONTH, sm.BillDate) + ' ' + CAST(YEAR(sm.BillDate) AS VARCHAR(4)) AS MonthName,
                YEAR(sm.BillDate) AS [Year],
                MONTH(sm.BillDate) AS [Month],
                ISNULL(SUM(ISNULL(sd.TotalAmount, sd.Amount)), 0) AS SalesAmount,
                0.0 AS SalesGrowthPercent,
                ISNULL((
                    SELECT SUM(ISNULL(pd.Cost, 0) * ISNULL(pd.Qty, 0) + ISNULL(pd.TaxAmt, 0)) 
                    FROM PDetails pd 
                    INNER JOIN PMaster pm ON pm.PurchaseNo = pd.PurchaseNo AND pm.BranchId = pd.BranchID
                    INNER JOIN ItemMaster im_p ON pd.ItemID = im_p.ItemId
                    WHERE YEAR(pm.PurchaseDate) = YEAR(sm.BillDate) AND MONTH(pm.PurchaseDate) = MONTH(sm.BillDate) 
                      AND ISNULL(pm.CancelFlag,0)=0 AND (@BranchId = 0 OR pm.BranchId = @BranchId)
                      AND (@GroupId = 0 OR im_p.GroupId = @GroupId)
                      AND (@CategoryId = 0 OR im_p.CategoryId = @CategoryId)
                ), 0) AS PurchaseAmount,
                0.0 AS ExpenseAmount,
                ISNULL(SUM(ISNULL(sd.TotalAmount, sd.Amount) - (ISNULL(sd.Cost, 0) * ISNULL(sd.Qty, 0))), 0) AS NetProfitAmount,
                0.0 AS NetProfitGrowthPercent
            FROM SDetails sd
            INNER JOIN SMaster sm ON sm.BillNo = sd.BillNo AND sm.BranchId = sd.BranchId
            INNER JOIN ItemMaster im ON sd.ItemId = im.ItemId
            WHERE sm.BillDate >= @RangeFrom AND sm.BillDate < @NextDay
              AND (@BranchId = 0 OR sm.BranchId = @BranchId)
              AND ISNULL(sm.CancelFlag, 0) = 0
              AND (@GroupId = 0 OR im.GroupId = @GroupId)
              AND (@CategoryId = 0 OR im.CategoryId = @CategoryId)
            GROUP BY YEAR(sm.BillDate), MONTH(sm.BillDate), DATENAME(MONTH, sm.BillDate)
            ORDER BY [Year] ASC, [Month] ASC;
        END
    END
    ELSE
    BEGIN
        SELECT 'Current Month' AS MonthName, YEAR(@ToDate) AS [Year], MONTH(@ToDate) AS [Month],
               0.0 AS SalesAmount, 0.0 AS SalesGrowthPercent, 0.0 AS PurchaseAmount,
               0.0 AS ExpenseAmount, 0.0 AS NetProfitAmount, 0.0 AS NetProfitGrowthPercent;
    END
END



GO
PRINT '  [_POS_ExecutiveDashboardKPIs] updated successfully.';
GO

PRINT '>>> Nexoris Client-Side Update Patch completed successfully!';
GO