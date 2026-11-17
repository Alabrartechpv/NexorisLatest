-- ============================================================================
-- Stored Procedure: dbo.POS_DailyFinancialMovementReport
-- Description:      Computes daily 12:00 AM automatic closing movements for all
--                   Assets, Liabilities, and Net Store Position (Cash, Bank,
--                   Debtors, Stock, Creditors, Loans, Fixed Assets, Capital)
--                   across any selected date range [FromDate, ToDate].
--                   Provides day-wise asset progression and Balance Sheet grouping.
-- Author:           Nexoris POS Engineering
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[POS_DailyFinancialMovementReport]') AND type IN (N'P', N'PC'))
BEGIN
    EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[POS_DailyFinancialMovementReport] AS RETURN 0;';
END
GO

ALTER PROCEDURE [dbo].[POS_DailyFinancialMovementReport]
    @CompanyId INT = 0,
    @BranchId  INT = 0,
    @FinYearId INT = 0,
    @FromDate  DATETIME,
    @ToDate    DATETIME,
    @DrillDownCategory VARCHAR(30) = 'ALL',  -- 'ALL', 'CASH', 'BANK', 'DEBTORS', 'CREDITORS', 'STOCK', 'LOANS', 'ASSETS'
    @DrillDownDate     DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartDate DATE = CAST(@FromDate AS DATE);
    DECLARE @EndDate   DATE = CAST(@ToDate AS DATE);

    -- Ensure valid dates
    IF @StartDate IS NULL SET @StartDate = CAST(GETDATE() AS DATE);
    IF @EndDate IS NULL SET @EndDate = CAST(GETDATE() AS DATE);
    IF @StartDate > @EndDate
    BEGIN
        DECLARE @TempDate DATE = @StartDate;
        SET @StartDate = @EndDate;
        SET @EndDate = @TempDate;
    END

    -- Financial Year Start Date
    DECLARE @FinYearStartDate DATE;
    IF OBJECT_ID('FinancialYear', 'U') IS NOT NULL AND @FinYearId > 0
    BEGIN
        SELECT TOP 1 @FinYearStartDate = CAST(FinYearFrom AS DATE)
        FROM FinancialYear 
        WHERE FinYearID = @FinYearId;
    END

    IF @FinYearStartDate IS NULL AND OBJECT_ID('CompanyInfo', 'U') IS NOT NULL
    BEGIN
        SELECT TOP 1 @FinYearStartDate = CAST(FinYearFrom AS DATE)
        FROM CompanyInfo 
        WHERE (@CompanyId = 0 OR CompanyID = @CompanyId);
    END

    IF @FinYearStartDate IS NULL
    BEGIN
        SET @FinYearStartDate = DATEFROMPARTS(YEAR(@StartDate), 1, 1);
    END

    ----------------------------------------------------------------------------
    -- 1. CALENDAR DATE TABLE (12:00 AM CYCLES)
    ----------------------------------------------------------------------------
    CREATE TABLE #CalendarDates (
        TxDate DATE PRIMARY KEY,
        DayName VARCHAR(15)
    );

    DECLARE @CurrDate DATE = @StartDate;
    WHILE @CurrDate <= @EndDate
    BEGIN
        INSERT INTO #CalendarDates (TxDate, DayName)
        VALUES (@CurrDate, DATENAME(WEEKDAY, @CurrDate));
        SET @CurrDate = DATEADD(DAY, 1, @CurrDate);
    END;

    ----------------------------------------------------------------------------
    -- 2. FULL BALANCE SHEET GROUP HIERARCHY MAPPING
    ----------------------------------------------------------------------------
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
                ELSE 'OTHER'
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

    -- Guarantee key standard balance sheet groups exist
    IF NOT EXISTS (SELECT 1 FROM #GroupMap WHERE GroupID = 14) INSERT INTO #GroupMap VALUES (14, 14, 'ASSETS');
    IF NOT EXISTS (SELECT 1 FROM #GroupMap WHERE GroupID = 15) INSERT INTO #GroupMap VALUES (15, 15, 'ASSETS');
    IF NOT EXISTS (SELECT 1 FROM #GroupMap WHERE GroupID = 16) INSERT INTO #GroupMap VALUES (16, 16, 'ASSETS');
    IF NOT EXISTS (SELECT 1 FROM #GroupMap WHERE GroupID = 17) INSERT INTO #GroupMap VALUES (17, 17, 'LIABILITIES');
    IF NOT EXISTS (SELECT 1 FROM #GroupMap WHERE GroupID = 18) INSERT INTO #GroupMap VALUES (18, 18, 'ASSETS');
    IF NOT EXISTS (SELECT 1 FROM #GroupMap WHERE GroupID = 25) INSERT INTO #GroupMap VALUES (25, 25, 'LIABILITIES');
    IF NOT EXISTS (SELECT 1 FROM #GroupMap WHERE GroupID = 26) INSERT INTO #GroupMap VALUES (26, 26, 'LIABILITIES');
    IF NOT EXISTS (SELECT 1 FROM #GroupMap WHERE GroupID = 27) INSERT INTO #GroupMap VALUES (27, 27, 'LIABILITIES');

    ----------------------------------------------------------------------------
    -- 3. CURRENT STOCK VALUATION (BASE FOR STOCK BACKTRACKING)
    ----------------------------------------------------------------------------
    DECLARE @CurrentLiveStockValuation DECIMAL(18,2) = 0;
    SELECT @CurrentLiveStockValuation = ISNULL(SUM(ISNULL(Stock, 0) * ISNULL(Cost, 0)), 0)
    FROM dbo.PriceSettings
    WHERE (@BranchId = 0 OR BranchId = @BranchId)
      AND ISNULL(IsBaseUnit, 'N') = 'Y';

    ----------------------------------------------------------------------------
    -- 4. INITIAL 12:00 AM BALANCES AS OF @StartDate
    ----------------------------------------------------------------------------
    CREATE TABLE #InitialBalances (
        GroupID INT PRIMARY KEY,
        GroupName VARCHAR(150),
        RootGroupType VARCHAR(20),
        InitialBal DECIMAL(18,2)
    );

    INSERT INTO #InitialBalances (GroupID, GroupName, RootGroupType, InitialBal)
    SELECT 
        gm.GroupID,
        MAX(ag.GroupName),
        gm.RootGroupType,
        ISNULL(SUM(
            CASE 
                -- Assets: Debit is positive
                WHEN gm.RootGroupType = 'ASSETS' THEN (ISNULL(l.OpnDebit, 0) - ISNULL(l.OpnCredit, 0))
                -- Liabilities & Capital: Credit is positive
                WHEN gm.RootGroupType IN ('LIABILITIES', 'CAPITAL') THEN (ISNULL(l.OpnCredit, 0) - ISNULL(l.OpnDebit, 0))
                ELSE 0 
            END
        ), 0) +
        ISNULL(SUM(
            CASE 
                -- Vouchers strictly before @StartDate 12:00 AM
                WHEN gm.RootGroupType = 'ASSETS' AND v.VoucherDate < @StartDate THEN (ISNULL(v.Debit, 0) - ISNULL(v.Credit, 0))
                WHEN gm.RootGroupType IN ('LIABILITIES', 'CAPITAL') AND v.VoucherDate < @StartDate THEN (ISNULL(v.Credit, 0) - ISNULL(v.Debit, 0))
                ELSE 0 
            END
        ), 0)
    FROM #GroupMap gm
    INNER JOIN dbo.AccountGroupMaster ag WITH (NOLOCK) ON gm.GroupID = ag.GroupID AND (@BranchId = 0 OR ag.BranchID = @BranchId)
    INNER JOIN dbo.LedgerMaster l WITH (NOLOCK) ON ag.GroupID = l.GroupID AND ag.BranchID = l.BranchID
    LEFT JOIN dbo.Vouchers v WITH (NOLOCK) ON l.LedgerID = v.LedgerID AND l.BranchID = v.BranchID AND ISNULL(v.CancelFlag, 0) = 0
    WHERE (@BranchId = 0 OR ag.BranchID = @BranchId)
      AND gm.RootGroupType IN ('ASSETS', 'LIABILITIES', 'CAPITAL')
      AND gm.GroupID <> 18 -- Stock handled separately
    GROUP BY gm.GroupID, gm.RootGroupType;

    ----------------------------------------------------------------------------
    -- 5. DAILY VOUCHER MOVEMENTS WITHIN [@StartDate, @EndDate]
    ----------------------------------------------------------------------------
    CREATE TABLE #DailyVoucherMovements (
        TxDate DATE,
        GroupID INT,
        RootGroupType VARCHAR(20),
        DayDebit DECIMAL(18,2),
        DayCredit DECIMAL(18,2),
        PRIMARY KEY (TxDate, GroupID)
    );

    INSERT INTO #DailyVoucherMovements (TxDate, GroupID, RootGroupType, DayDebit, DayCredit)
    SELECT 
        CAST(v.VoucherDate AS DATE) AS TxDate,
        gm.GroupID,
        gm.RootGroupType,
        ISNULL(SUM(v.Debit), 0),
        ISNULL(SUM(v.Credit), 0)
    FROM dbo.Vouchers v WITH (NOLOCK)
    INNER JOIN dbo.LedgerMaster l WITH (NOLOCK) ON v.LedgerID = l.LedgerID AND v.BranchID = l.BranchID
    INNER JOIN #GroupMap gm ON l.GroupID = gm.GroupID
    WHERE (@BranchId = 0 OR v.BranchID = @BranchId)
      AND ISNULL(v.CancelFlag, 0) = 0
      AND CAST(v.VoucherDate AS DATE) >= @StartDate
      AND CAST(v.VoucherDate AS DATE) <= @EndDate
      AND gm.RootGroupType IN ('ASSETS', 'LIABILITIES', 'CAPITAL')
      AND gm.GroupID <> 18
    GROUP BY CAST(v.VoucherDate AS DATE), gm.GroupID, gm.RootGroupType;

    ----------------------------------------------------------------------------
    -- 6. DAILY STOCK MOVEMENTS (PURCHASE INWARD vs SALES OUTWARD COST)
    ----------------------------------------------------------------------------
    CREATE TABLE #DailyStockMovements (
        TxDate DATE PRIMARY KEY,
        StockInward DECIMAL(18,2),
        StockOutward DECIMAL(18,2),
        StockNetDelta DECIMAL(18,2)
    );

    INSERT INTO #DailyStockMovements (TxDate, StockInward, StockOutward, StockNetDelta)
    SELECT 
        c.TxDate,
        ISNULL(pur.PurchaseCost, 0) + ISNULL(sret.SalesReturnCost, 0) + ISNULL(adj.AdjInCost, 0) AS StockInward,
        ISNULL(sal.SalesCost, 0) + ISNULL(pret.PurchaseReturnCost, 0) + ISNULL(adj.AdjOutCost, 0) AS StockOutward,
        (ISNULL(pur.PurchaseCost, 0) + ISNULL(sret.SalesReturnCost, 0) + ISNULL(adj.AdjInCost, 0)) -
        (ISNULL(sal.SalesCost, 0) + ISNULL(pret.PurchaseReturnCost, 0) + ISNULL(adj.AdjOutCost, 0)) AS StockNetDelta
    FROM #CalendarDates c
    LEFT JOIN (
        SELECT CAST(pd.PurchaseDate AS DATE) AS pDate, 
               SUM((ISNULL(pd.Qty,0) + ISNULL(pd.Free,0)) * ISNULL(pd.Cost,0)) AS PurchaseCost
        FROM dbo.PDetails pd WITH (NOLOCK)
        INNER JOIN dbo.PMaster pm WITH (NOLOCK) ON pd.BranchID = pm.BranchId AND pd.FinYearId = pm.FinYearId AND pd.PurchaseNo = pm.PurchaseNo
        WHERE (@BranchId = 0 OR pd.BranchID = @BranchId) AND ISNULL(pm.CancelFlag, 0) = 0
        GROUP BY CAST(pd.PurchaseDate AS DATE)
    ) pur ON c.TxDate = pur.pDate
    LEFT JOIN (
        SELECT CAST(sd.BillDate AS DATE) AS sDate, 
               SUM(ISNULL(sd.Qty,0) * ISNULL(sd.Cost,0)) AS SalesCost
        FROM dbo.SDetails sd WITH (NOLOCK)
        INNER JOIN dbo.SMaster sm WITH (NOLOCK) ON sd.BranchID = sm.BranchId AND sd.FinYearId = sm.FinYearId AND sd.BillNo = sm.BillNo
        WHERE (@BranchId = 0 OR sd.BranchID = @BranchId) AND ISNULL(sm.CancelFlag, 0) = 0 AND ISNULL(sd.CancelFlag, 0) = 0
        GROUP BY CAST(sd.BillDate AS DATE)
    ) sal ON c.TxDate = sal.sDate
    LEFT JOIN (
        SELECT CAST(srd.SReturnDate AS DATE) AS srDate,
               SUM(CAST(COALESCE(NULLIF(srd.ReturnedQty, 0), NULLIF(srd.ReturnQty, 0), srd.Qty, 0) AS DECIMAL(18,2)) * ISNULL(srd.Cost, 0)) AS SalesReturnCost
        FROM dbo.SReturnDetails srd WITH (NOLOCK)
        INNER JOIN dbo.SReturnMaster srm WITH (NOLOCK) ON srd.BranchID = srm.BranchId AND srd.FinYearId = srm.FinYearId AND srd.SReturnNo = srm.SReturnNo
        WHERE (@BranchId = 0 OR srd.BranchID = @BranchId) AND ISNULL(srm.CancelFlag, 0) = 0
        GROUP BY CAST(srd.SReturnDate AS DATE)
    ) sret ON c.TxDate = sret.srDate
    LEFT JOIN (
        SELECT CAST(prd.PReturnDate AS DATE) AS prDate,
               SUM(ISNULL(prd.Qty,0) * ISNULL(prd.Cost,0)) AS PurchaseReturnCost
        FROM dbo.PReturnDetails prd WITH (NOLOCK)
        INNER JOIN dbo.PReturnMaster prm WITH (NOLOCK) ON prd.BranchID = prm.BranchId AND prd.FinYearId = prm.FinYearId AND prd.PReturnNo = prm.PReturnNo
        WHERE (@BranchId = 0 OR prd.BranchID = @BranchId) AND ISNULL(prm.CancelFlag, 0) = 0
        GROUP BY CAST(prd.PReturnDate AS DATE)
    ) pret ON c.TxDate = pret.prDate
    LEFT JOIN (
        SELECT CAST(sam.StockAdjustmentDate AS DATE) AS aDate,
               SUM(CASE WHEN ISNULL(sad.QtyDifference, 0) > 0 THEN ISNULL(sad.QtyDifference, 0) * ISNULL(sad.Cost, 0) ELSE 0 END) AS AdjInCost,
               SUM(CASE WHEN ISNULL(sad.QtyDifference, 0) < 0 THEN ABS(ISNULL(sad.QtyDifference, 0)) * ISNULL(sad.Cost, 0) ELSE 0 END) AS AdjOutCost
        FROM dbo.StockAdjustmentDetails sad WITH (NOLOCK)
        INNER JOIN dbo.StockAdjustmentMaster sam WITH (NOLOCK) ON sad.StockAdjustmentMasterId = sam.Id
        WHERE (@BranchId = 0 OR sad.BranchID = @BranchId)
          AND ISNULL(sam.CancelFlag, 0) = 0
          AND ISNULL(sad.CancelFlag, 0) = 0
        GROUP BY CAST(sam.StockAdjustmentDate AS DATE)
    ) adj ON c.TxDate = adj.aDate;

    -- Stock value as of @EndDate
    DECLARE @StockDeltasAfterEndDate DECIMAL(18,2) = 0;
    SELECT @StockDeltasAfterEndDate = ISNULL(SUM(
        ISNULL(pur.PurchaseCost, 0) - ISNULL(sal.SalesCost, 0)
    ), 0)
    FROM (
        SELECT CAST(pd.PurchaseDate AS DATE) AS pDate, 
               SUM((ISNULL(pd.Qty,0) + ISNULL(pd.Free,0)) * ISNULL(pd.Cost,0)) AS PurchaseCost
        FROM dbo.PDetails pd WITH (NOLOCK)
        INNER JOIN dbo.PMaster pm WITH (NOLOCK) ON pd.BranchID = pm.BranchId AND pd.FinYearId = pm.FinYearId AND pd.PurchaseNo = pm.PurchaseNo
        WHERE (@BranchId = 0 OR pd.BranchID = @BranchId) AND ISNULL(pm.CancelFlag, 0) = 0 AND CAST(pd.PurchaseDate AS DATE) > @EndDate
        GROUP BY CAST(pd.PurchaseDate AS DATE)
    ) pur
    FULL OUTER JOIN (
        SELECT CAST(sd.BillDate AS DATE) AS sDate, 
               SUM(ISNULL(sd.Qty,0) * ISNULL(sd.Cost,0)) AS SalesCost
        FROM dbo.SDetails sd WITH (NOLOCK)
        INNER JOIN dbo.SMaster sm WITH (NOLOCK) ON sd.BranchID = sm.BranchId AND sd.FinYearId = sm.FinYearId AND sd.BillNo = sm.BillNo
        WHERE (@BranchId = 0 OR sd.BranchID = @BranchId) AND ISNULL(sm.CancelFlag, 0) = 0 AND ISNULL(sd.CancelFlag, 0) = 0 AND CAST(sd.BillDate AS DATE) > @EndDate
        GROUP BY CAST(sd.BillDate AS DATE)
    ) sal ON pur.pDate = sal.sDate;

    DECLARE @StockValuationAtEndDate DECIMAL(18,2) = @CurrentLiveStockValuation - @StockDeltasAfterEndDate;
    DECLARE @TotalPeriodStockDelta DECIMAL(18,2) = 0;
    SELECT @TotalPeriodStockDelta = ISNULL(SUM(StockNetDelta), 0) FROM #DailyStockMovements;
    DECLARE @InitialStockValuation DECIMAL(18,2) = @StockValuationAtEndDate - @TotalPeriodStockDelta;

    ----------------------------------------------------------------------------
    -- 7. ASSEMBLE DAY-WISE REPORT TABLE (12:00 AM CYCLES)
    ----------------------------------------------------------------------------
    CREATE TABLE #DailyReport (
        TxDate              DATE PRIMARY KEY,
        DayName             VARCHAR(15),
        
        -- Cash
        CashOpening         DECIMAL(18,2),
        CashInflow          DECIMAL(18,2),
        CashOutflow         DECIMAL(18,2),
        CashClosing         DECIMAL(18,2),
        
        -- Bank
        BankOpening         DECIMAL(18,2),
        BankInflow          DECIMAL(18,2),
        BankOutflow         DECIMAL(18,2),
        BankClosing         DECIMAL(18,2),
        
        -- Debtors (Customer Udhaar)
        DebtorsOpening      DECIMAL(18,2),
        DebtorsCreditSales  DECIMAL(18,2),
        DebtorsCollected    DECIMAL(18,2),
        DebtorsClosing      DECIMAL(18,2),
        
        -- Stock
        StockOpening        DECIMAL(18,2),
        StockInflow         DECIMAL(18,2),
        StockOutflow        DECIMAL(18,2),
        StockClosing        DECIMAL(18,2),
        
        -- Total Assets 12:00 AM Progression
        AssetOpening        DECIMAL(18,2),
        AssetAdditions      DECIMAL(18,2),
        AssetReductions     DECIMAL(18,2),
        TotalAssets         DECIMAL(18,2),
        DailyAssetChange    DECIMAL(18,2),
        DailyAssetChangePct DECIMAL(18,2),
        
        -- Creditors (Vendor Udhaar)
        CreditorsOpening    DECIMAL(18,2),
        CreditorsPurchases  DECIMAL(18,2),
        CreditorsPaid       DECIMAL(18,2),
        CreditorsClosing    DECIMAL(18,2),
        
        -- Loans
        LoansOpening        DECIMAL(18,2),
        LoansClosing        DECIMAL(18,2),
        
        -- Total Liabilities 12:00 AM Progression
        LiabOpening         DECIMAL(18,2),
        LiabAdditions       DECIMAL(18,2),
        LiabReductions      DECIMAL(18,2),
        TotalLiabilities    DECIMAL(18,2),
        DailyLiabChange     DECIMAL(18,2),
        
        -- Net Store Position (Worth Tonight)
        NetStorePosition    DECIMAL(18,2),
        DailyNetChange      DECIMAL(18,2)
    );

    -- Running variables
    DECLARE @rCash     DECIMAL(18,2) = ISNULL((SELECT InitialBal FROM #InitialBalances WHERE GroupID = 14), 0);
    DECLARE @rBank     DECIMAL(18,2) = ISNULL((SELECT InitialBal FROM #InitialBalances WHERE GroupID = 15), 0);
    DECLARE @rDebtors  DECIMAL(18,2) = ISNULL((SELECT InitialBal FROM #InitialBalances WHERE GroupID = 16), 0);
    DECLARE @rStock    DECIMAL(18,2) = @InitialStockValuation;
    DECLARE @rOtherAssets DECIMAL(18,2) = ISNULL((SELECT SUM(InitialBal) FROM #InitialBalances WHERE RootGroupType = 'ASSETS' AND GroupID NOT IN (14, 15, 16, 18)), 0);

    DECLARE @rCred     DECIMAL(18,2) = ISNULL((SELECT InitialBal FROM #InitialBalances WHERE GroupID = 17), 0);
    DECLARE @rLoans    DECIMAL(18,2) = ISNULL((SELECT SUM(InitialBal) FROM #InitialBalances WHERE RootGroupType IN ('LIABILITIES', 'CAPITAL') AND GroupID <> 17), 0);

    DECLARE @PrevNetPosition DECIMAL(18,2) = (@rCash + @rBank + @rDebtors + @rStock + @rOtherAssets) - (@rCred + @rLoans);

    -- Iterate day by day in chronological order
    DECLARE curDays CURSOR LOCAL FAST_FORWARD FOR
        SELECT TxDate, DayName FROM #CalendarDates ORDER BY TxDate;

    DECLARE @dDate DATE, @dName VARCHAR(15);
    OPEN curDays;
    FETCH NEXT FROM curDays INTO @dDate, @dName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Day Movements
        DECLARE @cIn  DECIMAL(18,2) = ISNULL((SELECT DayDebit  FROM #DailyVoucherMovements WHERE TxDate = @dDate AND GroupID = 14), 0);
        DECLARE @cOut DECIMAL(18,2) = ISNULL((SELECT DayCredit FROM #DailyVoucherMovements WHERE TxDate = @dDate AND GroupID = 14), 0);
        DECLARE @cClose DECIMAL(18,2) = @rCash + @cIn - @cOut;

        DECLARE @bIn  DECIMAL(18,2) = ISNULL((SELECT DayDebit  FROM #DailyVoucherMovements WHERE TxDate = @dDate AND GroupID = 15), 0);
        DECLARE @bOut DECIMAL(18,2) = ISNULL((SELECT DayCredit FROM #DailyVoucherMovements WHERE TxDate = @dDate AND GroupID = 15), 0);
        DECLARE @bClose DECIMAL(18,2) = @rBank + @bIn - @bOut;

        DECLARE @dIn  DECIMAL(18,2) = ISNULL((SELECT DayDebit  FROM #DailyVoucherMovements WHERE TxDate = @dDate AND GroupID = 16), 0);
        DECLARE @dOut DECIMAL(18,2) = ISNULL((SELECT DayCredit FROM #DailyVoucherMovements WHERE TxDate = @dDate AND GroupID = 16), 0);
        DECLARE @dClose DECIMAL(18,2) = @rDebtors + @dIn - @dOut;

        DECLARE @sIn  DECIMAL(18,2) = ISNULL((SELECT StockInward  FROM #DailyStockMovements WHERE TxDate = @dDate), 0);
        DECLARE @sOut DECIMAL(18,2) = ISNULL((SELECT StockOutward FROM #DailyStockMovements WHERE TxDate = @dDate), 0);
        DECLARE @sClose DECIMAL(18,2) = @rStock + @sIn - @sOut;

        DECLARE @oaIn  DECIMAL(18,2) = ISNULL((SELECT SUM(DayDebit)  FROM #DailyVoucherMovements WHERE TxDate = @dDate AND RootGroupType = 'ASSETS' AND GroupID NOT IN (14, 15, 16, 18)), 0);
        DECLARE @oaOut DECIMAL(18,2) = ISNULL((SELECT SUM(DayCredit) FROM #DailyVoucherMovements WHERE TxDate = @dDate AND RootGroupType = 'ASSETS' AND GroupID NOT IN (14, 15, 16, 18)), 0);
        DECLARE @oaClose DECIMAL(18,2) = @rOtherAssets + @oaIn - @oaOut;

        -- Asset Aggregation for Day
        DECLARE @dayAssetOpn DECIMAL(18,2) = @rCash + @rBank + @rDebtors + @rStock + @rOtherAssets;
        DECLARE @dayAssetIn  DECIMAL(18,2) = @cIn + @bIn + @dIn + @sIn + @oaIn;
        DECLARE @dayAssetOut DECIMAL(18,2) = @cOut + @bOut + @dOut + @sOut + @oaOut;
        DECLARE @totAssets   DECIMAL(18,2) = @cClose + @bClose + @dClose + @sClose + @oaClose;
        DECLARE @dayAssetChange DECIMAL(18,2) = @totAssets - @dayAssetOpn;
        DECLARE @dayAssetChangePct DECIMAL(18,2) = CASE WHEN @dayAssetOpn <> 0 THEN ROUND((@dayAssetChange / @dayAssetOpn) * 100, 2) ELSE 0 END;

        -- Liabilities Movements
        DECLARE @crPurch DECIMAL(18,2) = ISNULL((SELECT DayCredit FROM #DailyVoucherMovements WHERE TxDate = @dDate AND GroupID = 17), 0);
        DECLARE @crPaid  DECIMAL(18,2) = ISNULL((SELECT DayDebit  FROM #DailyVoucherMovements WHERE TxDate = @dDate AND GroupID = 17), 0);
        DECLARE @crClose DECIMAL(18,2) = @rCred + @crPurch - @crPaid;

        DECLARE @lIn  DECIMAL(18,2) = ISNULL((SELECT SUM(DayCredit) FROM #DailyVoucherMovements WHERE TxDate = @dDate AND RootGroupType IN ('LIABILITIES', 'CAPITAL') AND GroupID <> 17), 0);
        DECLARE @lOut DECIMAL(18,2) = ISNULL((SELECT SUM(DayDebit)  FROM #DailyVoucherMovements WHERE TxDate = @dDate AND RootGroupType IN ('LIABILITIES', 'CAPITAL') AND GroupID <> 17), 0);
        DECLARE @lClose DECIMAL(18,2) = @rLoans + @lIn - @lOut;

        -- Liability Aggregation for Day
        DECLARE @dayLiabOpn DECIMAL(18,2) = @rCred + @rLoans;
        DECLARE @dayLiabIn  DECIMAL(18,2) = @crPurch + @lIn;
        DECLARE @dayLiabOut DECIMAL(18,2) = @crPaid + @lOut;
        DECLARE @totLiab    DECIMAL(18,2) = @crClose + @lClose;
        DECLARE @dayLiabChange DECIMAL(18,2) = @totLiab - @dayLiabOpn;

        -- Net Position & Daily Change
        DECLARE @currNetPos DECIMAL(18,2) = @totAssets - @totLiab;
        DECLARE @dailyNetChange DECIMAL(18,2) = @currNetPos - @PrevNetPosition;

        INSERT INTO #DailyReport (
            TxDate, DayName,
            CashOpening, CashInflow, CashOutflow, CashClosing,
            BankOpening, BankInflow, BankOutflow, BankClosing,
            DebtorsOpening, DebtorsCreditSales, DebtorsCollected, DebtorsClosing,
            StockOpening, StockInflow, StockOutflow, StockClosing,
            AssetOpening, AssetAdditions, AssetReductions, TotalAssets, DailyAssetChange, DailyAssetChangePct,
            CreditorsOpening, CreditorsPurchases, CreditorsPaid, CreditorsClosing,
            LoansOpening, LoansClosing,
            LiabOpening, LiabAdditions, LiabReductions, TotalLiabilities, DailyLiabChange,
            NetStorePosition, DailyNetChange
        )
        VALUES (
            @dDate, @dName,
            @rCash, @cIn, @cOut, @cClose,
            @rBank, @bIn, @bOut, @bClose,
            @rDebtors, @dIn, @dOut, @dClose,
            @rStock, @sIn, @sOut, @sClose,
            @dayAssetOpn, @dayAssetIn, @dayAssetOut, @totAssets, @dayAssetChange, @dayAssetChangePct,
            @rCred, @crPurch, @crPaid, @crClose,
            @rLoans, @lClose,
            @dayLiabOpn, @dayLiabIn, @dayLiabOut, @totLiab, @dayLiabChange,
            @currNetPos, @dailyNetChange
        );

        -- Advance running balances to tomorrow morning 12:00 AM
        SET @rCash = @cClose;
        SET @rBank = @bClose;
        SET @rDebtors = @dClose;
        SET @rStock = @sClose;
        SET @rOtherAssets = @oaClose;
        SET @rCred = @crClose;
        SET @rLoans = @lClose;
        SET @PrevNetPosition = @currNetPos;

        FETCH NEXT FROM curDays INTO @dDate, @dName;
    END;

    CLOSE curDays;
    DEALLOCATE curDays;

    ----------------------------------------------------------------------------
    -- RESULT SET 1: DAILY REPORT ROWS (12:00 AM SNAPSHOTS & ASSET PROGRESSION)
    ----------------------------------------------------------------------------
    SELECT * FROM #DailyReport ORDER BY TxDate;

    ----------------------------------------------------------------------------
    -- RESULT SET 2: EXECUTIVE SUMMARY & PERIOD PROGRESSION TOTALS
    ----------------------------------------------------------------------------
    DECLARE @PeriodAssetOpening DECIMAL(18,2) = ISNULL((SELECT TOP 1 AssetOpening FROM #DailyReport ORDER BY TxDate ASC), 0);
    DECLARE @PeriodAssetClosing DECIMAL(18,2) = ISNULL((SELECT TOP 1 TotalAssets FROM #DailyReport ORDER BY TxDate DESC), 0);
    DECLARE @PeriodAssetGrowth  DECIMAL(18,2) = @PeriodAssetClosing - @PeriodAssetOpening;
    DECLARE @PeriodAssetGrowthPct DECIMAL(18,2) = CASE WHEN @PeriodAssetOpening <> 0 THEN ROUND((@PeriodAssetGrowth / @PeriodAssetOpening) * 100, 2) ELSE 0 END;

    DECLARE @PeriodLiabOpening DECIMAL(18,2) = ISNULL((SELECT TOP 1 LiabOpening FROM #DailyReport ORDER BY TxDate ASC), 0);
    DECLARE @PeriodLiabClosing DECIMAL(18,2) = ISNULL((SELECT TOP 1 TotalLiabilities FROM #DailyReport ORDER BY TxDate DESC), 0);
    DECLARE @PeriodLiabGrowth  DECIMAL(18,2) = @PeriodLiabClosing - @PeriodLiabOpening;

    DECLARE @PeriodNetWorthOpening DECIMAL(18,2) = @PeriodAssetOpening - @PeriodLiabOpening;
    DECLARE @PeriodNetWorthClosing DECIMAL(18,2) = @PeriodAssetClosing - @PeriodLiabClosing;

    SELECT 
        @PeriodAssetClosing AS LatestTotalAssets,
        @PeriodLiabClosing  AS LatestTotalLiabilities,
        @PeriodNetWorthClosing AS LatestNetStorePosition,
        (SELECT TOP 1 CashClosing FROM #DailyReport ORDER BY TxDate DESC) AS LatestCashBalance,
        (SELECT TOP 1 BankClosing FROM #DailyReport ORDER BY TxDate DESC) AS LatestBankBalance,
        (SELECT TOP 1 StockClosing FROM #DailyReport ORDER BY TxDate DESC) AS LatestStockValuation,
        (SELECT TOP 1 DebtorsClosing FROM #DailyReport ORDER BY TxDate DESC) AS LatestDebtorsBalance,
        (SELECT TOP 1 CreditorsClosing FROM #DailyReport ORDER BY TxDate DESC) AS LatestCreditorsBalance,
        ISNULL(SUM(CashInflow), 0) AS PeriodCashInflow,
        ISNULL(SUM(CashOutflow), 0) AS PeriodCashOutflow,
        ISNULL(SUM(DebtorsCreditSales), 0) AS PeriodCreditSales,
        ISNULL(SUM(DebtorsCollected), 0) AS PeriodDebtorsCollected,
        ISNULL(SUM(CreditorsPurchases), 0) AS PeriodCreditPurchases,
        ISNULL(SUM(CreditorsPaid), 0) AS PeriodCreditorsPaid,
        ISNULL(SUM(StockInflow), 0) AS PeriodStockInwardCost,
        ISNULL(SUM(StockOutflow), 0) AS PeriodStockOutwardCost,
        ISNULL(SUM(DailyNetChange), 0) AS PeriodNetWorthGrowth,
        @PeriodAssetOpening AS PeriodAssetOpening,
        @PeriodAssetClosing AS PeriodAssetClosing,
        @PeriodAssetGrowth  AS PeriodAssetGrowth,
        @PeriodAssetGrowthPct AS PeriodAssetGrowthPct,
        @PeriodLiabOpening  AS PeriodLiabOpening,
        @PeriodLiabClosing  AS PeriodLiabClosing,
        @PeriodLiabGrowth   AS PeriodLiabGrowth,
        @PeriodNetWorthOpening AS PeriodNetWorthOpening,
        @PeriodNetWorthClosing AS PeriodNetWorthClosing
    FROM #DailyReport;

    ----------------------------------------------------------------------------
    -- RESULT SET 3: BALANCE SHEET GROUP-WISE MOVEMENT SUMMARY (From Date to To Date)
    ----------------------------------------------------------------------------
    SELECT 
        'ASSETS' AS Side,
        ag.GroupID,
        ag.GroupName,
        'CURRENT ASSETS' AS ParentGroupName,
        ISNULL(ib.InitialBal, 0) AS OpeningBal,
        ISNULL(dvm.TotalDebit, 0) AS DebitTotal,
        ISNULL(dvm.TotalCredit, 0) AS CreditTotal,
        ISNULL(ib.InitialBal, 0) + ISNULL(dvm.TotalDebit, 0) - ISNULL(dvm.TotalCredit, 0) AS ClosingBal,
        (ISNULL(dvm.TotalDebit, 0) - ISNULL(dvm.TotalCredit, 0)) AS NetChange
    FROM dbo.AccountGroupMaster ag WITH (NOLOCK)
    INNER JOIN #GroupMap gm ON ag.GroupID = gm.GroupID
    LEFT JOIN #InitialBalances ib ON ag.GroupID = ib.GroupID
    LEFT JOIN (
        SELECT GroupID, SUM(DayDebit) AS TotalDebit, SUM(DayCredit) AS TotalCredit
        FROM #DailyVoucherMovements
        GROUP BY GroupID
    ) dvm ON ag.GroupID = dvm.GroupID
    WHERE gm.RootGroupType = 'ASSETS'
      AND ag.GroupID <> 18
      AND (@BranchId = 0 OR ag.BranchID = @BranchId)
      AND (ISNULL(ib.InitialBal, 0) <> 0 OR ISNULL(dvm.TotalDebit, 0) <> 0 OR ISNULL(dvm.TotalCredit, 0) <> 0)
    UNION ALL
    SELECT 
        'ASSETS' AS Side,
        18 AS GroupID,
        'STOCK IN HAND' AS GroupName,
        'CURRENT ASSETS' AS ParentGroupName,
        @InitialStockValuation AS OpeningBal,
        ISNULL(SUM(StockInward), 0) AS DebitTotal,
        ISNULL(SUM(StockOutward), 0) AS CreditTotal,
        @StockValuationAtEndDate AS ClosingBal,
        @StockValuationAtEndDate - @InitialStockValuation AS NetChange
    FROM #DailyStockMovements
    UNION ALL
    SELECT 
        'LIABILITIES' AS Side,
        ag.GroupID,
        ag.GroupName,
        CASE WHEN gm.RootGroupType = 'CAPITAL' THEN 'CAPITAL' ELSE 'CURRENT LIABILITIES' END AS ParentGroupName,
        ISNULL(ib.InitialBal, 0) AS OpeningBal,
        ISNULL(dvm.TotalDebit, 0) AS DebitTotal,
        ISNULL(dvm.TotalCredit, 0) AS CreditTotal,
        ISNULL(ib.InitialBal, 0) + ISNULL(dvm.TotalCredit, 0) - ISNULL(dvm.TotalDebit, 0) AS ClosingBal,
        (ISNULL(dvm.TotalCredit, 0) - ISNULL(dvm.TotalDebit, 0)) AS NetChange
    FROM dbo.AccountGroupMaster ag WITH (NOLOCK)
    INNER JOIN #GroupMap gm ON ag.GroupID = gm.GroupID
    LEFT JOIN #InitialBalances ib ON ag.GroupID = ib.GroupID
    LEFT JOIN (
        SELECT GroupID, SUM(DayDebit) AS TotalDebit, SUM(DayCredit) AS TotalCredit
        FROM #DailyVoucherMovements
        GROUP BY GroupID
    ) dvm ON ag.GroupID = dvm.GroupID
    WHERE gm.RootGroupType IN ('LIABILITIES', 'CAPITAL')
      AND (@BranchId = 0 OR ag.BranchID = @BranchId)
      AND (ISNULL(ib.InitialBal, 0) <> 0 OR ISNULL(dvm.TotalDebit, 0) <> 0 OR ISNULL(dvm.TotalCredit, 0) <> 0)
    ORDER BY Side, ParentGroupName DESC, GroupName;

    ----------------------------------------------------------------------------
    -- RESULT SET 4: DRILL-DOWN TRANSACTIONS (WHEN A CATEGORY AND DATE ARE CHOSEN)
    ----------------------------------------------------------------------------
    IF @DrillDownCategory <> 'ALL' AND @DrillDownDate IS NOT NULL
    BEGIN
        DECLARE @ddDate DATE = CAST(@DrillDownDate AS DATE);

        IF @DrillDownCategory IN ('CASH', 'BANK', 'DEBTORS', 'CREDITORS', 'LOANS', 'ASSETS')
        BEGIN
            DECLARE @targetGroupId INT = CASE @DrillDownCategory
                WHEN 'CASH' THEN 14
                WHEN 'BANK' THEN 15
                WHEN 'DEBTORS' THEN 16
                WHEN 'CREDITORS' THEN 17
                WHEN 'LOANS' THEN 25
                ELSE 0
            END;

            SELECT 
                v.VoucherID,
                v.VoucherDate,
                v.VoucherNumber,
                v.VoucherType,
                l.LedgerID,
                l.LedgerName,
                ag.GroupName,
                ISNULL(v.Debit, 0) AS Debit,
                ISNULL(v.Credit, 0) AS Credit,
                ISNULL(v.Narration, '') AS Narration,
                ISNULL(v.UserName, '') AS UserName
            FROM dbo.Vouchers v WITH (NOLOCK)
            INNER JOIN dbo.LedgerMaster l WITH (NOLOCK) ON v.LedgerID = l.LedgerID AND v.BranchID = l.BranchID
            INNER JOIN dbo.AccountGroupMaster ag WITH (NOLOCK) ON l.GroupID = ag.GroupID AND l.BranchID = ag.BranchID
            INNER JOIN #GroupMap gm ON l.GroupID = gm.GroupID
            WHERE (@BranchId = 0 OR v.BranchID = @BranchId)
              AND ISNULL(v.CancelFlag, 0) = 0
              AND CAST(v.VoucherDate AS DATE) = @ddDate
              AND (
                    (@targetGroupId = 14 AND ag.GroupID = 14)
                    OR (@targetGroupId = 15 AND ag.GroupID = 15)
                    OR (@targetGroupId = 16 AND ag.GroupID = 16)
                    OR (@targetGroupId = 17 AND ag.GroupID = 17)
                    OR (@targetGroupId = 25 AND ag.GroupID IN (25, 26, 27))
                    OR (@targetGroupId = 0 AND gm.RootGroupType = 'ASSETS')
                  )
            ORDER BY v.VoucherDate, v.VoucherID;
        END
        ELSE IF @DrillDownCategory = 'STOCK'
        BEGIN
            SELECT 
                'Purchase' AS MovementType,
                pd.PurchaseDate AS TxDate,
                'PUR-' + CAST(pd.PurchaseNo AS VARCHAR(50)) AS RefNo,
                pd.ItemID,
                im.Description AS ItemName,
                im.BarCode,
                CAST(ISNULL(pd.Qty,0) + ISNULL(pd.Free,0) AS DECIMAL(18,2)) AS InQty,
                0.00 AS OutQty,
                CAST(pd.Cost AS DECIMAL(18,2)) AS Cost,
                CAST((ISNULL(pd.Qty,0) + ISNULL(pd.Free,0)) * ISNULL(pd.Cost,0) AS DECIMAL(18,2)) AS TotalCost,
                ISNULL(lm.LedgerName, 'Supplier') AS PartyName
            FROM dbo.PDetails pd WITH (NOLOCK)
            INNER JOIN dbo.PMaster pm WITH (NOLOCK) ON pd.BranchID = pm.BranchId AND pd.FinYearId = pm.FinYearId AND pd.PurchaseNo = pm.PurchaseNo
            LEFT JOIN dbo.ItemMaster im WITH (NOLOCK) ON pd.ItemID = im.ItemId
            LEFT JOIN dbo.LedgerMaster lm WITH (NOLOCK) ON pm.LedgerID = lm.LedgerID
            WHERE (@BranchId = 0 OR pd.BranchID = @BranchId) AND ISNULL(pm.CancelFlag, 0) = 0
              AND CAST(pd.PurchaseDate AS DATE) = @ddDate
            UNION ALL
            SELECT 
                'Sales' AS MovementType,
                sd.BillDate AS TxDate,
                'INV-' + CAST(sd.BillNo AS VARCHAR(50)) AS RefNo,
                sd.ItemId,
                im.Description AS ItemName,
                im.BarCode,
                0.00 AS InQty,
                CAST(sd.Qty AS DECIMAL(18,2)) AS OutQty,
                CAST(sd.Cost AS DECIMAL(18,2)) AS Cost,
                CAST(ISNULL(sd.Qty,0) * ISNULL(sd.Cost,0) AS DECIMAL(18,2)) AS TotalCost,
                COALESCE(NULLIF(sm.CustomerName,''), 'Cash Customer') AS PartyName
            FROM dbo.SDetails sd WITH (NOLOCK)
            INNER JOIN dbo.SMaster sm WITH (NOLOCK) ON sd.BranchID = sm.BranchId AND sd.FinYearId = sm.FinYearId AND sd.BillNo = sm.BillNo
            LEFT JOIN dbo.ItemMaster im WITH (NOLOCK) ON sd.ItemId = im.ItemId
            WHERE (@BranchId = 0 OR sd.BranchID = @BranchId) AND ISNULL(sm.CancelFlag, 0) = 0 AND ISNULL(sd.CancelFlag, 0) = 0
              AND CAST(sd.BillDate AS DATE) = @ddDate
            ORDER BY TxDate;
        END
    END
    ELSE
    BEGIN
        SELECT CAST(0 AS INT) AS VoucherID, GETDATE() AS VoucherDate, '' AS VoucherNumber, '' AS VoucherType, 0 AS LedgerID, '' AS LedgerName, '' AS GroupName, CAST(0 AS DECIMAL(18,2)) AS Debit, CAST(0 AS DECIMAL(18,2)) AS Credit, '' AS Narration, '' AS UserName
        WHERE 1 = 0;
    END

    -- Cleanup temp tables
    DROP TABLE #CalendarDates;
    DROP TABLE #GroupMap;
    DROP TABLE #InitialBalances;
    DROP TABLE #DailyVoucherMovements;
    DROP TABLE #DailyStockMovements;
    DROP TABLE #DailyReport;
END
GO
