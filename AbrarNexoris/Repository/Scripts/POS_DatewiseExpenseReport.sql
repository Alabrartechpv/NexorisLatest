-- ============================================================================
-- STORED PROCEDURE: POS_DatewiseExpenseReport
-- PURPOSE: High-performance reporting procedure for Date-wise Expense Report
--          Supports 4 Period Modes: DATE_RANGE, MONTH_WISE, YEAR_WISE, ALL
--          Supports 4 View Modes:
--            1. DATE_MATRIX: Multi-level hierarchy (Band 0: Ledger Matrix, Band 1: Vouchers)
--            2. DETAILED: Complete flat transaction voucher breakdown
--            3. DATE_SUMMARY: Summary grouped by Date
--            4. EXPENSE_SUMMARY: Summary grouped by Expense Category and Ledger
-- ============================================================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[POS_DatewiseExpenseReport]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[POS_DatewiseExpenseReport]
GO

CREATE PROCEDURE [dbo].[POS_DatewiseExpenseReport]
(
    @CompanyId          INT             = 1,
    @BranchId           INT             = 0,
    @PeriodMode         VARCHAR(20)     = 'DATE_RANGE', -- 'DATE_RANGE', 'MONTH_WISE', 'YEAR_WISE', 'ALL'
    @FromDate           DATETIME        = NULL,
    @ToDate             DATETIME        = NULL,
    @Month              INT             = NULL,         -- 1 to 12
    @Year               INT             = NULL,         -- e.g. 2026
    @ExpenseCategoryId  INT             = 0,            -- 0=ALL, 10=Direct, 12=Indirect, or specific GroupID
    @LedgerId           INT             = 0,            -- 0=ALL, or specific LedgerID
    @PaymentMode        VARCHAR(30)     = 'ALL',        -- 'ALL', 'CASH', 'BANK', 'CHEQUE', etc.
    @SearchText         VARCHAR(100)    = '',
    @ViewMode           VARCHAR(30)     = 'DATE_MATRIX' -- 'DATE_MATRIX', 'DETAILED', 'DATE_SUMMARY', 'EXPENSE_SUMMARY'
)
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Date normalization & defaults
    IF (@PeriodMode = 'MONTH_WISE')
    BEGIN
        IF (@Year IS NULL OR @Year < 2000) SET @Year = YEAR(GETDATE());
        IF (@Month IS NULL OR @Month < 1 OR @Month > 12) SET @Month = MONTH(GETDATE());
        SET @FromDate = DATEFROMPARTS(@Year, @Month, 1);
        SET @ToDate = DATEADD(SECOND, -1, DATEADD(MONTH, 1, @FromDate));
    END
    ELSE IF (@PeriodMode = 'YEAR_WISE')
    BEGIN
        IF (@Year IS NULL OR @Year < 2000) SET @Year = YEAR(GETDATE());
        SET @FromDate = DATEFROMPARTS(@Year, 1, 1);
        SET @ToDate = DATEADD(SECOND, -1, DATEADD(YEAR, 1, @FromDate));
    END
    ELSE IF (@PeriodMode = 'ALL')
    BEGIN
        SET @FromDate = '2000-01-01 00:00:00';
        SET @ToDate = '2099-12-31 23:59:59';
    END
    ELSE -- 'DATE_RANGE' or fallback
    BEGIN
        IF (@FromDate IS NULL OR @FromDate < '1900-01-01')
            SET @FromDate = DATEADD(DAY, 1 - DAY(GETDATE()), CAST(GETDATE() AS DATE)); -- 1st of current month
        IF (@ToDate IS NULL OR @ToDate < '1900-01-01')
            SET @ToDate = CAST(GETDATE() AS DATE);
        
        -- End of day coverage
        SET @ToDate = DATEADD(SECOND, -1, DATEADD(DAY, 1, CAST(@ToDate AS DATE)));
    END

    SET @SearchText = LTRIM(RTRIM(ISNULL(@SearchText, '')));

    -- 2. Extract Expense Vouchers into indexed temp table #ExpenseVouchers
    IF OBJECT_ID('tempdb..#ExpenseVouchers') IS NOT NULL DROP TABLE #ExpenseVouchers;
    CREATE TABLE #ExpenseVouchers
    (
        VoucherID           INT,
        VoucherNumber       VARCHAR(50),
        VoucherDate         DATETIME,
        VoucherType         VARCHAR(50),
        LedgerID            INT,
        LedgerName          VARCHAR(150),
        GroupID             INT,
        GroupName           VARCHAR(100),
        ExpenseCategory     VARCHAR(50),
        ExpenseAmount       DECIMAL(18,2),
        CreditLedgerID      INT,
        PaidThrough         VARCHAR(150),
        PaymentMode         VARCHAR(50),
        Narration           VARCHAR(500),
        UserName            VARCHAR(100),
        BranchID            INT,
        BranchName          VARCHAR(100),
        CompanyID           INT
    );

    INSERT INTO #ExpenseVouchers
    (
        VoucherID, VoucherNumber, VoucherDate, VoucherType,
        LedgerID, LedgerName, GroupID, GroupName, ExpenseCategory,
        ExpenseAmount, CreditLedgerID, PaidThrough, PaymentMode,
        Narration, UserName, BranchID, BranchName, CompanyID
    )
    SELECT 
        v.VoucherID,
        ISNULL(v.VoucherNumber, CAST(v.VoucherID AS VARCHAR(50))) AS VoucherNumber,
        v.VoucherDate,
        ISNULL(v.VoucherType, 'GENPAY') AS VoucherType,
        v.LedgerID,
        ISNULL(lm.LedgerName, v.LedgerName) AS LedgerName,
        ISNULL(lm.GroupID, v.GroupID) AS GroupID,
        ISNULL(ag.GroupName, 'OTHER EXPENSES') AS GroupName,
        CASE 
            WHEN ag.GroupID = 12 OR ag.GroupName LIKE '%Indirect%' THEN 'Indirect Expense'
            WHEN ag.GroupID = 10 OR ag.GroupName LIKE '%Direct%' THEN 'Direct Expense'
            ELSE 'Operating Expense'
        END AS ExpenseCategory,
        CAST(ISNULL(v.Debit, 0) - ISNULL(v.Credit, 0) AS DECIMAL(18,2)) AS ExpenseAmount,
        ISNULL(pay.CreditLedgerID, 0) AS CreditLedgerID,
        ISNULL(pay.PaidThrough, 'CASH-IN-HAND') AS PaidThrough,
        ISNULL(pay.PaymentMode, 'CASH') AS PaymentMode,
        ISNULL(v.Narration, '') AS Narration,
        ISNULL(v.UserName, 'Admin') AS UserName,
        v.BranchID,
        ISNULL(bm.BranchName, 'Main Branch') AS BranchName,
        v.CompanyID
    FROM Vouchers v
    LEFT JOIN LedgerMaster lm ON v.LedgerID = lm.LedgerID
    LEFT JOIN AccountGroupMaster ag ON lm.GroupID = ag.GroupID
    LEFT JOIN dbo.Branch bm ON v.BranchID = bm.Id
    OUTER APPLY (
        SELECT TOP 1 
            c.LedgerID AS CreditLedgerID,
            ISNULL(cl.LedgerName, c.LedgerName) AS PaidThrough,
            CASE 
                WHEN cg.GroupName LIKE '%CASH%' OR cl.LedgerName LIKE '%CASH%' THEN 'CASH'
                WHEN cg.GroupName LIKE '%BANK%' OR cl.LedgerName LIKE '%BANK%' THEN 'BANK'
                WHEN c.Mode IS NOT NULL AND c.Mode <> '' THEN c.Mode
                ELSE 'OTHER'
            END AS PaymentMode
        FROM Vouchers c
        LEFT JOIN LedgerMaster cl ON c.LedgerID = cl.LedgerID
        LEFT JOIN AccountGroupMaster cg ON cl.GroupID = cg.GroupID
        WHERE c.CompanyID = v.CompanyID
          AND c.BranchID = v.BranchID
          AND c.VoucherType = v.VoucherType
          AND c.VoucherID = v.VoucherID
          AND c.Credit > 0
          AND c.LedgerID <> v.LedgerID
        ORDER BY c.Credit DESC
    ) pay
    WHERE (@BranchId = 0 OR v.BranchID = @BranchId)
      AND (@CompanyId = 0 OR v.CompanyID = @CompanyId)
      AND v.VoucherDate >= @FromDate AND v.VoucherDate <= @ToDate
      AND ISNULL(v.CancelFlag, 0) = 0
      AND (ISNULL(v.Debit, 0) > 0)
      AND (
          ag.GroupID IN (10, 12) 
          OR ag.ParentGroupID IN (10, 12)
          OR (ag.GroupType = 'EXPENSE' AND ag.GroupID <> 9)
          OR ag.GroupName LIKE '%Expense%'
          OR v.VoucherType = 'GENPAY'
      )
      AND (@ExpenseCategoryId = 0 OR ag.GroupID = @ExpenseCategoryId OR ag.ParentGroupID = @ExpenseCategoryId)
      AND (@LedgerId = 0 OR v.LedgerID = @LedgerId)
      AND (@PaymentMode = 'ALL' OR pay.PaymentMode = @PaymentMode OR (@PaymentMode = 'CASH' AND pay.PaymentMode IS NULL))
      AND (
          @SearchText = ''
          OR lm.LedgerName LIKE '%' + @SearchText + '%'
          OR ag.GroupName LIKE '%' + @SearchText + '%'
          OR v.VoucherNumber LIKE '%' + @SearchText + '%'
          OR v.Narration LIKE '%' + @SearchText + '%'
      );

    CREATE NONCLUSTERED INDEX IX_Temp_Exp_Ledger ON #ExpenseVouchers(LedgerID);
    CREATE NONCLUSTERED INDEX IX_Temp_Exp_Date ON #ExpenseVouchers(VoucherDate);

    -- 3. Return datasets according to @ViewMode

    -- =========================================================================
    -- VIEW MODE 1: DATE_MATRIX
    -- =========================================================================
    IF (@ViewMode = 'DATE_MATRIX')
    BEGIN
        -- Result Set 0: Raw Expense Transactions within period (used by C# repository to pivot dynamic date matrix)
        SELECT 
            VoucherID,
            VoucherNumber,
            VoucherDate,
            VoucherType,
            LedgerID,
            LedgerName,
            GroupID,
            GroupName,
            ExpenseCategory,
            ExpenseAmount,
            CreditLedgerID,
            PaidThrough,
            PaymentMode,
            Narration,
            UserName,
            BranchID,
            BranchName,
            CompanyID
        FROM #ExpenseVouchers
        ORDER BY LedgerName, VoucherDate, VoucherID;

        -- Result Set 1: Child Band - Detailed Vouchers Breakdown for each Ledger
        SELECT 
            LedgerID,
            VoucherID,
            VoucherNumber,
            VoucherDate,
            VoucherType,
            LedgerName,
            GroupName,
            ExpenseCategory,
            ExpenseAmount,
            PaidThrough,
            PaymentMode,
            Narration,
            UserName,
            BranchName
        FROM #ExpenseVouchers
        ORDER BY LedgerID, VoucherDate, VoucherID;
    END

    -- =========================================================================
    -- VIEW MODE 2: DETAILED
    -- =========================================================================
    ELSE IF (@ViewMode = 'DETAILED')
    BEGIN
        SELECT 
            ROW_NUMBER() OVER(ORDER BY VoucherDate, VoucherID) AS SlNo,
            VoucherID,
            VoucherNumber,
            VoucherDate,
            LedgerID,
            LedgerName,
            GroupID,
            GroupName,
            ExpenseCategory,
            ExpenseAmount,
            CreditLedgerID,
            PaidThrough,
            PaymentMode,
            Narration,
            UserName,
            BranchID,
            BranchName
        FROM #ExpenseVouchers
        ORDER BY VoucherDate, VoucherID;
    END

    -- =========================================================================
    -- VIEW MODE 3: DATE_SUMMARY
    -- =========================================================================
    ELSE IF (@ViewMode = 'DATE_SUMMARY' OR @ViewMode = 'DATE_EXPENSE_SUMMARY')
    BEGIN
        SELECT 
            CAST(VoucherDate AS DATE) AS ExpenseDate,
            DATENAME(WEEKDAY, VoucherDate) AS DayName,
            SUM(CASE WHEN ExpenseCategory = 'Direct Expense' THEN ExpenseAmount ELSE 0 END) AS DirectExpense,
            SUM(CASE WHEN ExpenseCategory = 'Indirect Expense' THEN ExpenseAmount ELSE 0 END) AS IndirectExpense,
            SUM(ExpenseAmount) AS TotalExpense,
            COUNT(DISTINCT VoucherID) AS VoucherCount,
            ROUND(AVG(ExpenseAmount), 2) AS AverageVoucherAmount
        FROM #ExpenseVouchers
        GROUP BY CAST(VoucherDate AS DATE), DATENAME(WEEKDAY, VoucherDate)
        ORDER BY ExpenseDate;
    END

    -- =========================================================================
    -- VIEW MODE 4: EXPENSE_SUMMARY
    -- =========================================================================
    ELSE IF (@ViewMode = 'EXPENSE_SUMMARY' OR @ViewMode = 'CATEGORY_SUMMARY')
    BEGIN
        DECLARE @GrandTotalExpense DECIMAL(18,2);
        SELECT @GrandTotalExpense = ISNULL(SUM(ExpenseAmount), 0) FROM #ExpenseVouchers;

        SELECT 
            GroupID,
            GroupName,
            ExpenseCategory,
            LedgerID,
            LedgerName,
            SUM(ExpenseAmount) AS TotalExpense,
            COUNT(DISTINCT VoucherID) AS VoucherCount,
            MIN(VoucherDate) AS FirstExpenseDate,
            MAX(VoucherDate) AS LastExpenseDate,
            CASE 
                WHEN @GrandTotalExpense > 0 
                THEN ROUND((SUM(ExpenseAmount) / @GrandTotalExpense) * 100.0, 2)
                ELSE 0.00 
            END AS PercentageOfTotal
        FROM #ExpenseVouchers
        GROUP BY GroupID, GroupName, ExpenseCategory, LedgerID, LedgerName
        ORDER BY ExpenseCategory, TotalExpense DESC;
    END

    -- Cleanup
    IF OBJECT_ID('tempdb..#ExpenseVouchers') IS NOT NULL DROP TABLE #ExpenseVouchers;
END
GO
