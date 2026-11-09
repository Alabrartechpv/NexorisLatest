-- ============================================================================
-- Stored Procedure: dbo.POS_DatewiseItemStockReport
-- Description:      Retrieves date-wise stock movement (Stock Came & Gone / IN & OUT)
--                   across all transactions (Purchases, Sales, Returns, Adjustments,
--                   Transfers, Opening Stock) with matrix, detailed, and summary views.
-- Author:           Nexoris POS Engineering
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[POS_DatewiseItemStockReport]') AND type IN (N'P', N'PC'))
BEGIN
    EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[POS_DatewiseItemStockReport] AS RETURN 0;';
END
GO

ALTER PROCEDURE [dbo].[POS_DatewiseItemStockReport]
    @FromDate        DATE          = NULL,
    @ToDate          DATE          = NULL,
    @PeriodMode      VARCHAR(20)   = 'ALL',       -- 'ALL', 'DATE_RANGE', 'MONTH_WISE', 'YEAR_WISE'
    @Month           INT           = 0,           -- 0 = ALL, 1..12
    @Year            INT           = 0,           -- 0 = ALL, 2026, ...
    @CompanyId       INT           = 0,
    @BranchId        INT           = 0,
    @FinYearId       INT           = 0,
    @WayFilter       VARCHAR(20)   = 'ALL',       -- 'ALL', 'IN', 'OUT'
    @OperationFilter VARCHAR(50)   = 'ALL',       -- 'ALL', 'Purchase', 'Sales', 'Sales Return', 'Purchase Return', 'Stock Adjustment', 'Stock Transfer', 'Opening Stock'
    @ItemSearch      NVARCHAR(200) = NULL,
    @ViewMode        VARCHAR(30)   = 'DETAILED'   -- 'DETAILED', 'DATE_ITEM_SUMMARY', 'ITEM_SUMMARY'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Mode VARCHAR(20) = UPPER(LTRIM(RTRIM(ISNULL(@PeriodMode, 'ALL'))));
    DECLARE @View VARCHAR(30) = UPPER(LTRIM(RTRIM(ISNULL(@ViewMode, 'DETAILED'))));
    DECLARE @Way VARCHAR(20)  = UPPER(LTRIM(RTRIM(ISNULL(@WayFilter, 'ALL'))));
    DECLARE @Op VARCHAR(50)   = UPPER(LTRIM(RTRIM(ISNULL(@OperationFilter, 'ALL'))));
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

    -- Temporary table to hold all combined movements
    CREATE TABLE #StockMovements
    (
        TransactionDate DATE,
        Operation       VARCHAR(50),
        Way             VARCHAR(10),   -- 'IN' or 'OUT'
        RefNo           VARCHAR(100),
        SlNo            INT,
        ItemID          INT,
        BarCode         VARCHAR(100),
        ItemName        NVARCHAR(250),
        Unit            VARCHAR(50),
        Qty             DECIMAL(18,2),
        InQty           DECIMAL(18,2),
        OutQty          DECIMAL(18,2),
        Rate            DECIMAL(18,2),
        Cost            DECIMAL(18,2),
        Amount          DECIMAL(18,2),
        Account         NVARCHAR(200),
        Remarks         NVARCHAR(300),
        BranchName      NVARCHAR(150),
        CurrentStock    DECIMAL(18,2)
    );

    -- 1. Purchases (Stock Came / IN)
    INSERT INTO #StockMovements
    SELECT 
        CAST(ISNULL(PD.PurchaseDate, GETDATE()) AS DATE),
        'Purchase',
        'IN',
        COALESCE(NULLIF(PD.InvoiceNo, ''), 'PUR-' + CAST(ISNULL(PD.PurchaseNo, 0) AS VARCHAR(50))),
        ISNULL(PD.SlNo, 1),
        ISNULL(PD.ItemID, 0),
        ISNULL(IM.BarCode, ''),
        ISNULL(IM.Description, 'Unknown Item'),
        ISNULL(UM.UnitName, ISNULL(PD.Unit, '')),
        CAST(ISNULL(PD.Qty, 0) + ISNULL(PD.Free, 0) AS DECIMAL(18,2)),
        CAST(ISNULL(PD.Qty, 0) + ISNULL(PD.Free, 0) AS DECIMAL(18,2)),
        0.00,
        CAST(ISNULL(PD.Cost, 0) AS DECIMAL(18,2)),
        CAST(ISNULL(PD.Cost, 0) AS DECIMAL(18,2)),
        CAST((ISNULL(PD.Qty, 0) + ISNULL(PD.Free, 0)) * ISNULL(PD.Cost, 0) AS DECIMAL(18,2)),
        ISNULL(LM.LedgerName, 'Vendor Purchase'),
        'Purchase Inward',
        ISNULL(B.BranchName, ''),
        CAST(ISNULL(PS.Stock, 0) AS DECIMAL(18,2))
    FROM dbo.PDetails PD WITH (NOLOCK)
    LEFT JOIN dbo.PMaster PM WITH (NOLOCK) ON (PD.BranchID = PM.BranchId AND PD.FinYearId = PM.FinYearId AND PD.PurchaseNo = PM.PurchaseNo)
    LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemId = PD.ItemID
    LEFT JOIN dbo.UnitMaster UM WITH (NOLOCK) ON UM.UnitID = PD.UnitId
    LEFT JOIN dbo.LedgerMaster LM WITH (NOLOCK) ON LM.LedgerID = PM.LedgerID
    LEFT JOIN dbo.Branches B WITH (NOLOCK) ON B.Id = PD.BranchID
    OUTER APPLY (
        SELECT TOP 1 Stock FROM dbo.PriceSettings ps 
        WHERE ps.ItemId = PD.ItemID AND ps.IsBaseUnit = 'Y' 
          AND (@BranchId = 0 OR ps.BranchId = @BranchId)
    ) PS
    WHERE ISNULL(PM.CancelFlag, 0) = 0
      AND (@CompanyId = 0 OR ISNULL(PD.CompanyId, 0) = @CompanyId)
      AND (@BranchId = 0 OR ISNULL(PD.BranchID, 0) = @BranchId)
      AND (@FinYearId = 0 OR ISNULL(PD.FinYearId, 0) = @FinYearId)
      AND (
            @Mode = 'ALL'
            OR (@Mode = 'DATE_RANGE' AND CAST(PD.PurchaseDate AS DATE) >= @StartDate AND CAST(PD.PurchaseDate AS DATE) <= @EndDate)
            OR (@Mode = 'MONTH_WISE' AND ((@Year = 0 OR YEAR(PD.PurchaseDate) = @Year) AND (@Month = 0 OR MONTH(PD.PurchaseDate) = @Month)))
            OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(PD.PurchaseDate) = @Year))
          );

    -- 2. Sales Returns (Stock Came / IN)
    INSERT INTO #StockMovements
    SELECT 
        CAST(ISNULL(SRD.SReturnDate, GETDATE()) AS DATE),
        'Sales Return',
        'IN',
        COALESCE(NULLIF(SRD.InvoiceNo, ''), 'SR-' + CAST(ISNULL(SRD.SReturnNo, 0) AS VARCHAR(50))),
        ISNULL(SRD.SlNo, 1),
        ISNULL(SRD.ItemID, 0),
        ISNULL(IM.BarCode, ''),
        ISNULL(IM.Description, 'Unknown Item'),
        ISNULL(UM.UnitName, ISNULL(SRD.Unit, '')),
        CAST(COALESCE(NULLIF(SRD.ReturnedQty, 0), NULLIF(SRD.ReturnQty, 0), SRD.Qty, 0) AS DECIMAL(18,2)),
        CAST(COALESCE(NULLIF(SRD.ReturnedQty, 0), NULLIF(SRD.ReturnQty, 0), SRD.Qty, 0) AS DECIMAL(18,2)),
        0.00,
        CAST(ISNULL(SRD.Cost, 0) AS DECIMAL(18,2)),
        CAST(ISNULL(SRD.Cost, 0) AS DECIMAL(18,2)),
        CAST(COALESCE(NULLIF(SRD.ReturnedQty, 0), NULLIF(SRD.ReturnQty, 0), SRD.Qty, 0) * ISNULL(SRD.Cost, 0) AS DECIMAL(18,2)),
        ISNULL(LM.LedgerName, 'Customer Return'),
        ISNULL(SRD.Reason, 'Sales Return Inward'),
        ISNULL(B.BranchName, ''),
        CAST(ISNULL(PS.Stock, 0) AS DECIMAL(18,2))
    FROM dbo.SReturnDetails SRD WITH (NOLOCK)
    LEFT JOIN dbo.SReturnMaster SRM WITH (NOLOCK) ON (SRD.BranchID = SRM.BranchId AND SRD.FinYearId = SRM.FinYearId AND SRD.SReturnNo = SRM.SReturnNo)
    LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemId = SRD.ItemID
    LEFT JOIN dbo.UnitMaster UM WITH (NOLOCK) ON UM.UnitID = SRD.UnitId
    LEFT JOIN dbo.LedgerMaster LM WITH (NOLOCK) ON LM.LedgerID = SRM.LedgerID
    LEFT JOIN dbo.Branches B WITH (NOLOCK) ON B.Id = SRD.BranchID
    OUTER APPLY (
        SELECT TOP 1 Stock FROM dbo.PriceSettings ps 
        WHERE ps.ItemId = SRD.ItemID AND ps.IsBaseUnit = 'Y' 
          AND (@BranchId = 0 OR ps.BranchId = @BranchId)
    ) PS
    WHERE ISNULL(SRM.CancelFlag, 0) = 0
      AND (@CompanyId = 0 OR ISNULL(SRD.CompanyId, 0) = @CompanyId)
      AND (@BranchId = 0 OR ISNULL(SRD.BranchID, 0) = @BranchId)
      AND (@FinYearId = 0 OR ISNULL(SRD.FinYearId, 0) = @FinYearId)
      AND (
            @Mode = 'ALL'
            OR (@Mode = 'DATE_RANGE' AND CAST(SRD.SReturnDate AS DATE) >= @StartDate AND CAST(SRD.SReturnDate AS DATE) <= @EndDate)
            OR (@Mode = 'MONTH_WISE' AND ((@Year = 0 OR YEAR(SRD.SReturnDate) = @Year) AND (@Month = 0 OR MONTH(SRD.SReturnDate) = @Month)))
            OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(SRD.SReturnDate) = @Year))
          );

    -- 3. Stock Adjustment In (Stock Came / IN)
    INSERT INTO #StockMovements
    SELECT 
        CAST(ISNULL(SAM.StockAdjustmentDate, GETDATE()) AS DATE),
        'Stock Adjustment In',
        'IN',
        'ADJ-' + CAST(ISNULL(SAM.StockAdjustmentNo, 0) AS VARCHAR(50)),
        ISNULL(SAD.SlNo, 1),
        ISNULL(SAD.ItemId, 0),
        ISNULL(IM.BarCode, ''),
        ISNULL(IM.Description, 'Unknown Item'),
        ISNULL(UM.UnitName, ''),
        CAST(ISNULL(SAD.QtyDifference, 0) AS DECIMAL(18,2)),
        CAST(ISNULL(SAD.QtyDifference, 0) AS DECIMAL(18,2)),
        0.00,
        CAST(ISNULL(SAD.Cost, 0) AS DECIMAL(18,2)),
        CAST(ISNULL(SAD.Cost, 0) AS DECIMAL(18,2)),
        CAST(ISNULL(SAD.QtyDifference, 0) * ISNULL(SAD.Cost, 0) AS DECIMAL(18,2)),
        ISNULL(LM.LedgerName, 'Stock Adjustment'),
        COALESCE(NULLIF(SAD.Reason, ''), NULLIF(SAM.Comments, ''), 'Stock Adjustment IN'),
        ISNULL(B.BranchName, ''),
        CAST(ISNULL(PS.Stock, 0) AS DECIMAL(18,2))
    FROM dbo.StockAdjustmentDetails SAD WITH (NOLOCK)
    INNER JOIN dbo.StockAdjustmentMaster SAM WITH (NOLOCK) ON SAM.Id = SAD.StockAdjustmentMasterId
    LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemId = SAD.ItemId
    LEFT JOIN dbo.UnitMaster UM WITH (NOLOCK) ON UM.UnitID = SAD.UnitId
    LEFT JOIN dbo.LedgerMaster LM WITH (NOLOCK) ON LM.LedgerID = SAM.LedgerId
    LEFT JOIN dbo.Branches B WITH (NOLOCK) ON B.Id = SAD.BranchId
    OUTER APPLY (
        SELECT TOP 1 Stock FROM dbo.PriceSettings ps 
        WHERE ps.ItemId = SAD.ItemId AND ps.IsBaseUnit = 'Y' 
          AND (@BranchId = 0 OR ps.BranchId = @BranchId)
    ) PS
    WHERE ISNULL(SAM.CancelFlag, 0) = 0
      AND ISNULL(SAD.CancelFlag, 0) = 0
      AND ISNULL(SAD.QtyDifference, 0) > 0
      AND (@CompanyId = 0 OR ISNULL(SAD.CompanyId, 0) = @CompanyId)
      AND (@BranchId = 0 OR ISNULL(SAD.BranchId, 0) = @BranchId)
      AND (@FinYearId = 0 OR ISNULL(SAD.FinYearId, 0) = @FinYearId)
      AND (
            @Mode = 'ALL'
            OR (@Mode = 'DATE_RANGE' AND CAST(SAM.StockAdjustmentDate AS DATE) >= @StartDate AND CAST(SAM.StockAdjustmentDate AS DATE) <= @EndDate)
            OR (@Mode = 'MONTH_WISE' AND ((@Year = 0 OR YEAR(SAM.StockAdjustmentDate) = @Year) AND (@Month = 0 OR MONTH(SAM.StockAdjustmentDate) = @Month)))
            OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(SAM.StockAdjustmentDate) = @Year))
          );

    -- 4. Stock Transfer In / Local Purchase (Stock Came / IN)
    INSERT INTO #StockMovements
    SELECT 
        CAST(ISNULL(STD.TransferDate, GETDATE()) AS DATE),
        'Stock Transfer In',
        'IN',
        'TR-' + CAST(ISNULL(STD.StkTrNo, 0) AS VARCHAR(50)),
        ISNULL(STD.SlNo, 1),
        ISNULL(STD.ItemId, 0),
        ISNULL(IM.BarCode, ''),
        ISNULL(IM.Description, 'Unknown Item'),
        ISNULL(UM.UnitName, ''),
        CAST(ISNULL(STD.Qty, 0) AS DECIMAL(18,2)),
        CAST(ISNULL(STD.Qty, 0) AS DECIMAL(18,2)),
        0.00,
        CAST(ISNULL(STD.Rate, 0) AS DECIMAL(18,2)),
        CAST(ISNULL(STD.Rate, 0) AS DECIMAL(18,2)),
        CAST(ISNULL(STD.Amt, 0) AS DECIMAL(18,2)),
        'Transfer From: ' + ISNULL(BSrc.BranchName, 'Source Branch'),
        'Stock Transfer Inward',
        ISNULL(BTgt.BranchName, ''),
        CAST(ISNULL(PS.Stock, 0) AS DECIMAL(18,2))
    FROM dbo.StockTransferDetail STD WITH (NOLOCK)
    LEFT JOIN dbo.StockTransferMaster STM WITH (NOLOCK) ON (STD.StkTranMasterId = STM.Id OR (STD.TargetId = STM.TargetId AND STD.FinYearId = STM.FinYearId AND STD.StkTrNo = STM.StkTrNo))
    LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemId = STD.ItemId
    LEFT JOIN dbo.UnitMaster UM WITH (NOLOCK) ON UM.UnitID = STD.UnitId
    LEFT JOIN dbo.Branches BSrc WITH (NOLOCK) ON BSrc.Id = STD.SourceId
    LEFT JOIN dbo.Branches BTgt WITH (NOLOCK) ON BTgt.Id = STD.TargetId
    OUTER APPLY (
        SELECT TOP 1 Stock FROM dbo.PriceSettings ps 
        WHERE ps.ItemId = STD.ItemId AND ps.IsBaseUnit = 'Y' 
          AND (@BranchId = 0 OR ps.BranchId = @BranchId)
    ) PS
    WHERE ISNULL(STM.CancelFlag, 0) = 0
      AND ISNULL(STD.CancelFlag, 0) = 0
      AND (@CompanyId = 0 OR ISNULL(STD.CompanyId, 0) = @CompanyId)
      AND (@BranchId = 0 OR ISNULL(STD.TargetId, 0) = @BranchId)
      AND (@FinYearId = 0 OR ISNULL(STD.FinYearId, 0) = @FinYearId)
      AND (
            @Mode = 'ALL'
            OR (@Mode = 'DATE_RANGE' AND CAST(STD.TransferDate AS DATE) >= @StartDate AND CAST(STD.TransferDate AS DATE) <= @EndDate)
            OR (@Mode = 'MONTH_WISE' AND ((@Year = 0 OR YEAR(STD.TransferDate) = @Year) AND (@Month = 0 OR MONTH(STD.TransferDate) = @Month)))
            OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(STD.TransferDate) = @Year))
          );

    -- 5. Opening Stock (Stock Came / IN)
    INSERT INTO #StockMovements
    SELECT 
        CAST(COALESCE(PS.OpnDate, '2000-01-01') AS DATE),
        'Opening Stock',
        'IN',
        'OPN',
        1,
        ISNULL(PS.ItemId, 0),
        ISNULL(IM.BarCode, ''),
        ISNULL(IM.Description, 'Unknown Item'),
        ISNULL(UM.UnitName, ''),
        CAST(ISNULL(PS.OpnStk, 0) AS DECIMAL(18,2)),
        CAST(ISNULL(PS.OpnStk, 0) AS DECIMAL(18,2)),
        0.00,
        CAST(CASE WHEN ISNULL(PS.OpnStk, 0) > 0 THEN ISNULL(PS.OpnValue, 0) / PS.OpnStk ELSE ISNULL(PS.Cost, 0) END AS DECIMAL(18,2)),
        CAST(CASE WHEN ISNULL(PS.OpnStk, 0) > 0 THEN ISNULL(PS.OpnValue, 0) / PS.OpnStk ELSE ISNULL(PS.Cost, 0) END AS DECIMAL(18,2)),
        CAST(ISNULL(PS.OpnValue, 0) AS DECIMAL(18,2)),
        'Opening Balance',
        'Initial Stock Balance',
        ISNULL(B.BranchName, ''),
        CAST(ISNULL(PS.Stock, 0) AS DECIMAL(18,2))
    FROM dbo.PriceSettings PS WITH (NOLOCK)
    LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemId = PS.ItemId
    LEFT JOIN dbo.UnitMaster UM WITH (NOLOCK) ON UM.UnitID = PS.UnitId
    LEFT JOIN dbo.Branches B WITH (NOLOCK) ON B.Id = PS.BranchId
    WHERE ISNULL(PS.OpnStk, 0) > 0
      AND ISNULL(PS.IsBaseUnit, 'Y') = 'Y'
      AND (@CompanyId = 0 OR ISNULL(PS.CompanyId, 0) = @CompanyId)
      AND (@BranchId = 0 OR ISNULL(PS.BranchId, 0) = @BranchId)
      AND (
            @Mode = 'ALL'
            OR (@Mode = 'DATE_RANGE' AND CAST(COALESCE(PS.OpnDate, '2000-01-01') AS DATE) >= @StartDate AND CAST(COALESCE(PS.OpnDate, '2000-01-01') AS DATE) <= @EndDate)
            OR (@Mode = 'MONTH_WISE' AND ((@Year = 0 OR YEAR(COALESCE(PS.OpnDate, '2000-01-01')) = @Year) AND (@Month = 0 OR MONTH(COALESCE(PS.OpnDate, '2000-01-01')) = @Month)))
            OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(COALESCE(PS.OpnDate, '2000-01-01')) = @Year))
          );

    -- 6. Sales (Stock Gone / OUT)
    INSERT INTO #StockMovements
    SELECT 
        CAST(ISNULL(SD.BillDate, GETDATE()) AS DATE),
        'Sales',
        'OUT',
        'INV-' + CAST(ISNULL(SD.BillNo, 0) AS VARCHAR(50)),
        ISNULL(SD.SlNo, 1),
        ISNULL(SD.ItemId, 0),
        ISNULL(IM.BarCode, ISNULL(SD.Barcode, '')),
        ISNULL(IM.Description, ISNULL(SD.ItemName, 'Unknown Item')),
        ISNULL(UM.UnitName, ISNULL(SD.Unit, '')),
        CAST(ISNULL(SD.Qty, 0) AS DECIMAL(18,2)),
        0.00,
        CAST(ISNULL(SD.Qty, 0) AS DECIMAL(18,2)),
        CAST(ISNULL(SD.UnitPrice, 0) AS DECIMAL(18,2)),
        CAST(ISNULL(SD.Cost, 0) AS DECIMAL(18,2)),
        CAST(ISNULL(SD.TotalAmount, ISNULL(SD.Amount, 0)) AS DECIMAL(18,2)),
        COALESCE(NULLIF(SM.CustomerName, ''), NULLIF(LM.LedgerName, ''), 'Cash Customer'),
        'Sales Outward',
        ISNULL(B.BranchName, ''),
        CAST(ISNULL(PS.Stock, 0) AS DECIMAL(18,2))
    FROM dbo.SDetails SD WITH (NOLOCK)
    LEFT JOIN dbo.SMaster SM WITH (NOLOCK) ON (SD.BranchID = SM.BranchId AND SD.FinYearId = SM.FinYearId AND SD.BillNo = SM.BillNo)
    LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemId = SD.ItemId
    LEFT JOIN dbo.UnitMaster UM WITH (NOLOCK) ON UM.UnitID = SD.UnitId
    LEFT JOIN dbo.LedgerMaster LM WITH (NOLOCK) ON LM.LedgerID = SM.LedgerID
    LEFT JOIN dbo.Branches B WITH (NOLOCK) ON B.Id = SD.BranchId
    OUTER APPLY (
        SELECT TOP 1 Stock FROM dbo.PriceSettings ps 
        WHERE ps.ItemId = SD.ItemId AND ps.IsBaseUnit = 'Y' 
          AND (@BranchId = 0 OR ps.BranchId = @BranchId)
    ) PS
    WHERE ISNULL(SM.CancelFlag, 0) = 0
      AND ISNULL(SD.CancelFlag, 0) = 0
      AND ISNULL(SM.Status, '') <> 'Hold'
      AND (@CompanyId = 0 OR ISNULL(SD.CompanyId, 0) = @CompanyId)
      AND (@BranchId = 0 OR ISNULL(SD.BranchId, 0) = @BranchId)
      AND (@FinYearId = 0 OR ISNULL(SD.FinYearId, 0) = @FinYearId)
      AND (
            @Mode = 'ALL'
            OR (@Mode = 'DATE_RANGE' AND CAST(SD.BillDate AS DATE) >= @StartDate AND CAST(SD.BillDate AS DATE) <= @EndDate)
            OR (@Mode = 'MONTH_WISE' AND ((@Year = 0 OR YEAR(SD.BillDate) = @Year) AND (@Month = 0 OR MONTH(SD.BillDate) = @Month)))
            OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(SD.BillDate) = @Year))
          );

    -- 7. Purchase Returns (Stock Gone / OUT)
    INSERT INTO #StockMovements
    SELECT 
        CAST(ISNULL(PRD.PReturnDate, GETDATE()) AS DATE),
        'Purchase Return',
        'OUT',
        COALESCE(NULLIF(PRD.InvoiceNo, ''), 'PR-' + CAST(ISNULL(PRD.PReturnNo, 0) AS VARCHAR(50))),
        ISNULL(PRD.SlNo, 1),
        ISNULL(PRD.ItemID, 0),
        ISNULL(IM.BarCode, ''),
        ISNULL(IM.Description, 'Unknown Item'),
        ISNULL(UM.UnitName, ISNULL(PRD.Unit, '')),
        CAST(COALESCE(NULLIF(PRD.Returned, 0), NULLIF(PRD.Returnqty, 0), PRD.Qty, 0) AS DECIMAL(18,2)),
        0.00,
        CAST(COALESCE(NULLIF(PRD.Returned, 0), NULLIF(PRD.Returnqty, 0), PRD.Qty, 0) AS DECIMAL(18,2)),
        CAST(ISNULL(PRD.Cost, 0) AS DECIMAL(18,2)),
        CAST(ISNULL(PRD.Cost, 0) AS DECIMAL(18,2)),
        CAST(COALESCE(NULLIF(PRD.Returned, 0), NULLIF(PRD.Returnqty, 0), PRD.Qty, 0) * ISNULL(PRD.Cost, 0) AS DECIMAL(18,2)),
        ISNULL(LM.LedgerName, 'Vendor Return'),
        ISNULL(PRD.Reason, 'Purchase Return Outward'),
        ISNULL(B.BranchName, ''),
        CAST(ISNULL(PS.Stock, 0) AS DECIMAL(18,2))
    FROM dbo.PReturnDetails PRD WITH (NOLOCK)
    LEFT JOIN dbo.PReturnMaster PRM WITH (NOLOCK) ON (PRD.BranchID = PRM.BranchId AND PRD.FinYearId = PRM.FinYearId AND PRD.PReturnNo = PRM.PReturnNo)
    LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemId = PRD.ItemID
    LEFT JOIN dbo.UnitMaster UM WITH (NOLOCK) ON UM.UnitID = PRD.UnitId
    LEFT JOIN dbo.LedgerMaster LM WITH (NOLOCK) ON LM.LedgerID = PRM.LedgerID
    LEFT JOIN dbo.Branches B WITH (NOLOCK) ON B.Id = PRD.BranchID
    OUTER APPLY (
        SELECT TOP 1 Stock FROM dbo.PriceSettings ps 
        WHERE ps.ItemId = PRD.ItemID AND ps.IsBaseUnit = 'Y' 
          AND (@BranchId = 0 OR ps.BranchId = @BranchId)
    ) PS
    WHERE ISNULL(PRM.CancelFlag, 0) = 0
      AND (@CompanyId = 0 OR ISNULL(PRD.CompanyId, 0) = @CompanyId)
      AND (@BranchId = 0 OR ISNULL(PRD.BranchID, 0) = @BranchId)
      AND (@FinYearId = 0 OR ISNULL(PRD.FinYearId, 0) = @FinYearId)
      AND (
            @Mode = 'ALL'
            OR (@Mode = 'DATE_RANGE' AND CAST(PRD.PReturnDate AS DATE) >= @StartDate AND CAST(PRD.PReturnDate AS DATE) <= @EndDate)
            OR (@Mode = 'MONTH_WISE' AND ((@Year = 0 OR YEAR(PRD.PReturnDate) = @Year) AND (@Month = 0 OR MONTH(PRD.PReturnDate) = @Month)))
            OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(PRD.PReturnDate) = @Year))
          );

    -- 8. Stock Adjustment Out (Stock Gone / OUT)
    INSERT INTO #StockMovements
    SELECT 
        CAST(ISNULL(SAM.StockAdjustmentDate, GETDATE()) AS DATE),
        'Stock Adjustment Out',
        'OUT',
        'ADJ-' + CAST(ISNULL(SAM.StockAdjustmentNo, 0) AS VARCHAR(50)),
        ISNULL(SAD.SlNo, 1),
        ISNULL(SAD.ItemId, 0),
        ISNULL(IM.BarCode, ''),
        ISNULL(IM.Description, 'Unknown Item'),
        ISNULL(UM.UnitName, ''),
        CAST(ABS(ISNULL(SAD.QtyDifference, 0)) AS DECIMAL(18,2)),
        0.00,
        CAST(ABS(ISNULL(SAD.QtyDifference, 0)) AS DECIMAL(18,2)),
        CAST(ISNULL(SAD.Cost, 0) AS DECIMAL(18,2)),
        CAST(ISNULL(SAD.Cost, 0) AS DECIMAL(18,2)),
        CAST(ABS(ISNULL(SAD.QtyDifference, 0)) * ISNULL(SAD.Cost, 0) AS DECIMAL(18,2)),
        ISNULL(LM.LedgerName, 'Stock Adjustment'),
        COALESCE(NULLIF(SAD.Reason, ''), NULLIF(SAM.Comments, ''), 'Stock Adjustment OUT'),
        ISNULL(B.BranchName, ''),
        CAST(ISNULL(PS.Stock, 0) AS DECIMAL(18,2))
    FROM dbo.StockAdjustmentDetails SAD WITH (NOLOCK)
    INNER JOIN dbo.StockAdjustmentMaster SAM WITH (NOLOCK) ON SAM.Id = SAD.StockAdjustmentMasterId
    LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemId = SAD.ItemId
    LEFT JOIN dbo.UnitMaster UM WITH (NOLOCK) ON UM.UnitID = SAD.UnitId
    LEFT JOIN dbo.LedgerMaster LM WITH (NOLOCK) ON LM.LedgerID = SAM.LedgerId
    LEFT JOIN dbo.Branches B WITH (NOLOCK) ON B.Id = SAD.BranchId
    OUTER APPLY (
        SELECT TOP 1 Stock FROM dbo.PriceSettings ps 
        WHERE ps.ItemId = SAD.ItemId AND ps.IsBaseUnit = 'Y' 
          AND (@BranchId = 0 OR ps.BranchId = @BranchId)
    ) PS
    WHERE ISNULL(SAM.CancelFlag, 0) = 0
      AND ISNULL(SAD.CancelFlag, 0) = 0
      AND ISNULL(SAD.QtyDifference, 0) < 0
      AND (@CompanyId = 0 OR ISNULL(SAD.CompanyId, 0) = @CompanyId)
      AND (@BranchId = 0 OR ISNULL(SAD.BranchId, 0) = @BranchId)
      AND (@FinYearId = 0 OR ISNULL(SAD.FinYearId, 0) = @FinYearId)
      AND (
            @Mode = 'ALL'
            OR (@Mode = 'DATE_RANGE' AND CAST(SAM.StockAdjustmentDate AS DATE) >= @StartDate AND CAST(SAM.StockAdjustmentDate AS DATE) <= @EndDate)
            OR (@Mode = 'MONTH_WISE' AND ((@Year = 0 OR YEAR(SAM.StockAdjustmentDate) = @Year) AND (@Month = 0 OR MONTH(SAM.StockAdjustmentDate) = @Month)))
            OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(SAM.StockAdjustmentDate) = @Year))
          );

    -- 9. Stock Transfer Out / Local Sales (Stock Gone / OUT)
    INSERT INTO #StockMovements
    SELECT 
        CAST(ISNULL(STD.TransferDate, GETDATE()) AS DATE),
        'Stock Transfer Out',
        'OUT',
        'TR-' + CAST(ISNULL(STD.StkTrNo, 0) AS VARCHAR(50)),
        ISNULL(STD.SlNo, 1),
        ISNULL(STD.ItemId, 0),
        ISNULL(IM.BarCode, ''),
        ISNULL(IM.Description, 'Unknown Item'),
        ISNULL(UM.UnitName, ''),
        CAST(ISNULL(STD.Qty, 0) AS DECIMAL(18,2)),
        0.00,
        CAST(ISNULL(STD.Qty, 0) AS DECIMAL(18,2)),
        CAST(ISNULL(STD.Rate, 0) AS DECIMAL(18,2)),
        CAST(ISNULL(STD.Rate, 0) AS DECIMAL(18,2)),
        CAST(ISNULL(STD.Amt, 0) AS DECIMAL(18,2)),
        'Transfer To: ' + ISNULL(BTgt.BranchName, 'Target Branch'),
        'Stock Transfer Outward',
        ISNULL(BSrc.BranchName, ''),
        CAST(ISNULL(PS.Stock, 0) AS DECIMAL(18,2))
    FROM dbo.StockTransferDetail STD WITH (NOLOCK)
    LEFT JOIN dbo.StockTransferMaster STM WITH (NOLOCK) ON (STD.StkTranMasterId = STM.Id OR (STD.SourceId = STM.SourceId AND STD.FinYearId = STM.FinYearId AND STD.StkTrNo = STM.StkTrNo))
    LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemId = STD.ItemId
    LEFT JOIN dbo.UnitMaster UM WITH (NOLOCK) ON UM.UnitID = STD.UnitId
    LEFT JOIN dbo.Branches BSrc WITH (NOLOCK) ON BSrc.Id = STD.SourceId
    LEFT JOIN dbo.Branches BTgt WITH (NOLOCK) ON BTgt.Id = STD.TargetId
    OUTER APPLY (
        SELECT TOP 1 Stock FROM dbo.PriceSettings ps 
        WHERE ps.ItemId = STD.ItemId AND ps.IsBaseUnit = 'Y' 
          AND (@BranchId = 0 OR ps.BranchId = @BranchId)
    ) PS
    WHERE ISNULL(STM.CancelFlag, 0) = 0
      AND ISNULL(STD.CancelFlag, 0) = 0
      AND (@CompanyId = 0 OR ISNULL(STD.CompanyId, 0) = @CompanyId)
      AND (@BranchId = 0 OR ISNULL(STD.SourceId, 0) = @BranchId)
      AND (@FinYearId = 0 OR ISNULL(STD.FinYearId, 0) = @FinYearId)
      AND (
            @Mode = 'ALL'
            OR (@Mode = 'DATE_RANGE' AND CAST(STD.TransferDate AS DATE) >= @StartDate AND CAST(STD.TransferDate AS DATE) <= @EndDate)
            OR (@Mode = 'MONTH_WISE' AND ((@Year = 0 OR YEAR(STD.TransferDate) = @Year) AND (@Month = 0 OR MONTH(STD.TransferDate) = @Month)))
            OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(STD.TransferDate) = @Year))
          );

    -- 10. Office Supplies Withdrawal (Stock Gone / OUT)
    INSERT INTO #StockMovements
    SELECT 
        CAST(ISNULL(OSWM.Date, GETDATE()) AS DATE),
        'Office Supplies Withdrawal',
        'OUT',
        'OSW-' + CAST(ISNULL(OSWM.OfficeSupplyNo, 0) AS VARCHAR(50)),
        ISNULL(OSWD.SlNo, 1),
        ISNULL(OSWD.ItemId, 0),
        ISNULL(IM.BarCode, ''),
        ISNULL(IM.Description, 'Unknown Item'),
        ISNULL(UM.UnitName, ''),
        CAST(ISNULL(OSWD.Qty, 0) AS DECIMAL(18,2)),
        0.00,
        CAST(ISNULL(OSWD.Qty, 0) AS DECIMAL(18,2)),
        CAST(ISNULL(OSWD.Rate, 0) AS DECIMAL(18,2)),
        CAST(ISNULL(OSWD.Rate, 0) AS DECIMAL(18,2)),
        CAST(ISNULL(OSWD.Amt, 0) AS DECIMAL(18,2)),
        'Internal Expense',
        'Office Supplies Withdrawal',
        ISNULL(B.BranchName, ''),
        CAST(ISNULL(PS.Stock, 0) AS DECIMAL(18,2))
    FROM dbo.OfficeSuppliesWithdrawDetails OSWD WITH (NOLOCK)
    LEFT JOIN dbo.OfficeSuppliesWithdrawMaster OSWM WITH (NOLOCK) ON (OSWM.Id = OSWD.MasterId)
    LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemId = OSWD.ItemId
    LEFT JOIN dbo.UnitMaster UM WITH (NOLOCK) ON UM.UnitID = OSWD.UnitId
    LEFT JOIN dbo.Branches B WITH (NOLOCK) ON B.Id = OSWM.BranchId
    OUTER APPLY (
        SELECT TOP 1 Stock FROM dbo.PriceSettings ps 
        WHERE ps.ItemId = OSWD.ItemId AND ps.IsBaseUnit = 'Y' 
          AND (@BranchId = 0 OR ps.BranchId = @BranchId)
    ) PS
    WHERE ISNULL(OSWM.CancelFlag, 0) = 0
      AND (@CompanyId = 0 OR ISNULL(OSWM.CompanyId, 0) = @CompanyId)
      AND (@BranchId = 0 OR ISNULL(OSWM.BranchId, 0) = @BranchId)
      AND (@FinYearId = 0 OR ISNULL(OSWM.FinYearId, 0) = @FinYearId)
      AND (
            @Mode = 'ALL'
            OR (@Mode = 'DATE_RANGE' AND CAST(OSWM.Date AS DATE) >= @StartDate AND CAST(OSWM.Date AS DATE) <= @EndDate)
            OR (@Mode = 'MONTH_WISE' AND ((@Year = 0 OR YEAR(OSWM.Date) = @Year) AND (@Month = 0 OR MONTH(OSWM.Date) = @Month)))
            OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(OSWM.Date) = @Year))
          );

    -- Apply Way filter, Operation filter, and Item search filter
    DELETE FROM #StockMovements
    WHERE (
            @Way <> 'ALL' AND Way <> @Way
          )
       OR (
            @Op <> 'ALL' AND Operation NOT LIKE '%' + @Op + '%'
          )
       OR (
            @CleanSearch IS NOT NULL
            AND ItemName NOT LIKE '%' + @CleanSearch + '%'
            AND BarCode NOT LIKE '%' + @CleanSearch + '%'
            AND RefNo NOT LIKE '%' + @CleanSearch + '%'
            AND Account NOT LIKE '%' + @CleanSearch + '%'
            AND Remarks NOT LIKE '%' + @CleanSearch + '%'
          );

    -- ========================================================================
    -- VIEW MODE 1: DETAILED (Every Stock In/Out Transaction)
    -- ========================================================================
    IF @View = 'DETAILED'
    BEGIN
        SELECT 
            TransactionDate,
            Operation,
            Way,
            RefNo,
            SlNo,
            ItemID,
            BarCode,
            ItemName,
            Unit,
            Qty,
            InQty,
            OutQty,
            Rate,
            Cost,
            Amount,
            Account,
            Remarks,
            BranchName,
            CurrentStock
        FROM #StockMovements
        ORDER BY TransactionDate DESC, ItemName ASC, RefNo DESC, SlNo ASC;
        RETURN;
    END

    -- ========================================================================
    -- VIEW MODE 2: DATE_ITEM_SUMMARY (Daily In / Out / Net per Item)
    -- ========================================================================
    IF @View = 'DATE_ITEM_SUMMARY'
    BEGIN
        SELECT 
            TransactionDate,
            'Daily Movement' AS Operation,
            CASE 
                WHEN SUM(InQty) > SUM(OutQty) THEN 'IN'
                WHEN SUM(OutQty) > SUM(InQty) THEN 'OUT'
                ELSE 'BAL'
            END AS Way,
            '' AS RefNo,
            1 AS SlNo,
            ItemID,
            BarCode,
            ItemName,
            Unit,
            SUM(InQty) + SUM(OutQty) AS Qty,
            SUM(InQty) AS InQty,
            SUM(OutQty) AS OutQty,
            AVG(Rate) AS Rate,
            AVG(Cost) AS Cost,
            SUM(Amount) AS Amount,
            'Daily Aggregated' AS Account,
            'Net: ' + CAST(SUM(InQty) - SUM(OutQty) AS VARCHAR(30)) AS Remarks,
            MAX(BranchName) AS BranchName,
            MAX(CurrentStock) AS CurrentStock
        FROM #StockMovements
        GROUP BY TransactionDate, ItemID, BarCode, ItemName, Unit
        ORDER BY TransactionDate DESC, ItemName ASC;
        RETURN;
    END

    -- ========================================================================
    -- VIEW MODE 3: ITEM_SUMMARY (Totals by Item across whole period)
    -- ========================================================================
    IF @View = 'ITEM_SUMMARY'
    BEGIN
        SELECT 
            COALESCE(@StartDate, MIN(TransactionDate), GETDATE()) AS TransactionDate,
            'Item Summary' AS Operation,
            CASE 
                WHEN SUM(InQty) > SUM(OutQty) THEN 'IN'
                WHEN SUM(OutQty) > SUM(InQty) THEN 'OUT'
                ELSE 'BAL'
            END AS Way,
            '' AS RefNo,
            1 AS SlNo,
            ItemID,
            BarCode,
            ItemName,
            Unit,
            SUM(InQty) + SUM(OutQty) AS Qty,
            SUM(InQty) AS InQty,
            SUM(OutQty) AS OutQty,
            AVG(Rate) AS Rate,
            AVG(Cost) AS Cost,
            SUM(Amount) AS Amount,
            'Summary' AS Account,
            'Net: ' + CAST(SUM(InQty) - SUM(OutQty) AS VARCHAR(30)) AS Remarks,
            MAX(BranchName) AS BranchName,
            MAX(CurrentStock) AS CurrentStock
        FROM #StockMovements
        GROUP BY ItemID, BarCode, ItemName, Unit
        ORDER BY ItemName ASC;
        RETURN;
    END

    DROP TABLE #StockMovements;
END
GO
