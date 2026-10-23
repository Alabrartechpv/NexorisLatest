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

PRINT '>>> Nexoris Client-Side Update Patch completed successfully!';
GO
