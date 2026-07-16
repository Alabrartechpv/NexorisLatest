IF OBJECT_ID(N'dbo.POS_ItemStockActivityLog', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo.POS_ItemStockActivityLog AS BEGIN SET NOCOUNT ON; END');
GO

IF OBJECT_ID(N'dbo.StockAdjustmentActivityLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockAdjustmentActivityLog
    (
        ActivityLogId INT IDENTITY(1,1) PRIMARY KEY,
        TransactionNo BIGINT NOT NULL,
        InvoiceNo NVARCHAR(100) NULL,
        PartyName NVARCHAR(250) NULL,
        PaymentMode NVARCHAR(100) NULL,
        NetAmount DECIMAL(18,4) NOT NULL DEFAULT(0),
        ActivityType NVARCHAR(50) NOT NULL,
        ActivityDetails NVARCHAR(MAX) NULL,
        Qty DECIMAL(18,4) NULL,
        Cost DECIMAL(18,4) NULL,
        Unit NVARCHAR(50) NULL,
        Barcode NVARCHAR(100) NULL,
        SPrice DECIMAL(18,4) NULL,
        TaxAmt DECIMAL(18,4) NULL,
        TaxPer DECIMAL(18,4) NULL,
        BaseAmount DECIMAL(18,4) NULL,
        Packing DECIMAL(18,4) NULL,
        RetailPrice DECIMAL(18,4) NULL,
        Free DECIMAL(18,4) NULL,
        UnitSP DECIMAL(18,4) NULL,
        TaxType NVARCHAR(50) NULL,
        Gross DECIMAL(18,4) NULL,
        CompanyId INT NOT NULL DEFAULT(0),
        BranchId INT NOT NULL DEFAULT(0),
        FinYearId INT NOT NULL DEFAULT(0),
        UserId INT NOT NULL DEFAULT(0),
        UserName NVARCHAR(150) NULL,
        CounterId INT NOT NULL DEFAULT(0),
        CounterName NVARCHAR(150) NULL,
        CounterSessionId BIGINT NOT NULL DEFAULT(0),
        CreatedOn DATETIME NOT NULL DEFAULT(GETDATE())
    );

    CREATE INDEX IX_StockAdjustmentActivityLog_TransactionNo ON dbo.StockAdjustmentActivityLog(TransactionNo, CreatedOn);
    CREATE INDEX IX_StockAdjustmentActivityLog_UserCounter ON dbo.StockAdjustmentActivityLog(UserId, CounterId, CreatedOn);
END

IF COL_LENGTH('dbo.StockAdjustmentActivityLog', 'ActivityLogId') IS NULL
    ALTER TABLE dbo.StockAdjustmentActivityLog ADD ActivityLogId INT IDENTITY(1,1) NOT NULL;

IF COL_LENGTH('dbo.StockAdjustmentActivityLog', 'TransactionNo') IS NULL
    ALTER TABLE dbo.StockAdjustmentActivityLog ADD TransactionNo BIGINT NOT NULL CONSTRAINT DF_StockAdjustmentActivityLog_TransactionNo DEFAULT(0);

IF COL_LENGTH('dbo.StockAdjustmentActivityLog', 'InvoiceNo') IS NULL
    ALTER TABLE dbo.StockAdjustmentActivityLog ADD InvoiceNo NVARCHAR(100) NULL;

IF COL_LENGTH('dbo.StockAdjustmentActivityLog', 'PartyName') IS NULL
    ALTER TABLE dbo.StockAdjustmentActivityLog ADD PartyName NVARCHAR(250) NULL;

IF COL_LENGTH('dbo.StockAdjustmentActivityLog', 'PaymentMode') IS NULL
    ALTER TABLE dbo.StockAdjustmentActivityLog ADD PaymentMode NVARCHAR(100) NULL;

IF COL_LENGTH('dbo.StockAdjustmentActivityLog', 'NetAmount') IS NULL
    ALTER TABLE dbo.StockAdjustmentActivityLog ADD NetAmount DECIMAL(18,4) NOT NULL CONSTRAINT DF_StockAdjustmentActivityLog_NetAmount DEFAULT(0);

IF COL_LENGTH('dbo.StockAdjustmentActivityLog', 'ActivityType') IS NULL
    ALTER TABLE dbo.StockAdjustmentActivityLog ADD ActivityType NVARCHAR(50) NOT NULL CONSTRAINT DF_StockAdjustmentActivityLog_ActivityType DEFAULT('');

IF COL_LENGTH('dbo.StockAdjustmentActivityLog', 'ActivityDetails') IS NULL
    ALTER TABLE dbo.StockAdjustmentActivityLog ADD ActivityDetails NVARCHAR(MAX) NULL;

IF COL_LENGTH('dbo.StockAdjustmentActivityLog', 'CompanyId') IS NULL
    ALTER TABLE dbo.StockAdjustmentActivityLog ADD CompanyId INT NOT NULL CONSTRAINT DF_StockAdjustmentActivityLog_CompanyId DEFAULT(0);

IF COL_LENGTH('dbo.StockAdjustmentActivityLog', 'BranchId') IS NULL
    ALTER TABLE dbo.StockAdjustmentActivityLog ADD BranchId INT NOT NULL CONSTRAINT DF_StockAdjustmentActivityLog_BranchId DEFAULT(0);

IF COL_LENGTH('dbo.StockAdjustmentActivityLog', 'FinYearId') IS NULL
    ALTER TABLE dbo.StockAdjustmentActivityLog ADD FinYearId INT NOT NULL CONSTRAINT DF_StockAdjustmentActivityLog_FinYearId DEFAULT(0);

IF COL_LENGTH('dbo.StockAdjustmentActivityLog', 'UserId') IS NULL
    ALTER TABLE dbo.StockAdjustmentActivityLog ADD UserId INT NOT NULL CONSTRAINT DF_StockAdjustmentActivityLog_UserId DEFAULT(0);

IF COL_LENGTH('dbo.StockAdjustmentActivityLog', 'UserName') IS NULL
    ALTER TABLE dbo.StockAdjustmentActivityLog ADD UserName NVARCHAR(150) NULL;

IF COL_LENGTH('dbo.StockAdjustmentActivityLog', 'CounterId') IS NULL
    ALTER TABLE dbo.StockAdjustmentActivityLog ADD CounterId INT NOT NULL CONSTRAINT DF_StockAdjustmentActivityLog_CounterId DEFAULT(0);

IF COL_LENGTH('dbo.StockAdjustmentActivityLog', 'CounterName') IS NULL
    ALTER TABLE dbo.StockAdjustmentActivityLog ADD CounterName NVARCHAR(150) NULL;

IF COL_LENGTH('dbo.StockAdjustmentActivityLog', 'CounterSessionId') IS NULL
    ALTER TABLE dbo.StockAdjustmentActivityLog ADD CounterSessionId BIGINT NOT NULL CONSTRAINT DF_StockAdjustmentActivityLog_CounterSessionId DEFAULT(0);

IF COL_LENGTH('dbo.StockAdjustmentActivityLog', 'CreatedOn') IS NULL
    ALTER TABLE dbo.StockAdjustmentActivityLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_StockAdjustmentActivityLog_CreatedOn DEFAULT(GETDATE());
GO

ALTER PROCEDURE dbo.POS_ItemStockActivityLog
(
    @FromDate date = NULL,
    @ToDate date = NULL,
    @UserName nvarchar(150) = NULL,
    @Action nvarchar(50) = NULL,
    @ItemSearch nvarchar(250) = NULL,
    @CompanyId int = NULL,
    @BranchId int = NULL,
    @FinYearId int = NULL,
    @_Operation varchar(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SET @UserName = ISNULL(@UserName, N'');
    SET @Action = ISNULL(@Action, N'');
    SET @ItemSearch = ISNULL(@ItemSearch, N'');

    IF (@_Operation = 'GETACTIONS')
    BEGIN
        SELECT Value
        FROM (VALUES
            (N'Sales'),
            (N'Purchase'),
            (N'Sales Return'),
            (N'Purchase Return'),
            (N'Stock IN'),
            (N'Stock OUT')
        ) AS Actions(Value)
        ORDER BY Value;
        RETURN;
    END

    IF (@_Operation = 'GETUSERS')
    BEGIN
        CREATE TABLE #Users(Value nvarchar(150) NULL);

        INSERT INTO #Users(Value)
        SELECT DISTINCT COALESCE(NULLIF(u.UserName, N''), NULLIF(CONVERT(nvarchar(150), sm.UserId), N'0'))
        FROM dbo.SMaster sm
        LEFT JOIN dbo.Users u ON u.UserID = sm.UserId
        WHERE ISNULL(sm.UserId, 0) <> 0
          AND (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(sm.CompanyId, 0) = @CompanyId)
          AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(sm.BranchId, 0) = @BranchId)
          AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(sm.FinYearId, 0) = @FinYearId);

        INSERT INTO #Users(Value)
        SELECT DISTINCT NULLIF(pm.UserName, N'')
        FROM dbo.PMaster pm
        WHERE ISNULL(pm.UserName, N'') <> N''
          AND (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(pm.CompanyId, 0) = @CompanyId)
          AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(pm.BranchId, 0) = @BranchId)
          AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(pm.FinYearId, 0) = @FinYearId);

        INSERT INTO #Users(Value)
        SELECT DISTINCT NULLIF(srm.UserName, N'')
        FROM dbo.SReturnMaster srm
        WHERE ISNULL(srm.UserName, N'') <> N''
          AND (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(srm.CompanyId, 0) = @CompanyId)
          AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(srm.BranchId, 0) = @BranchId)
          AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(srm.FinYearId, 0) = @FinYearId);

        INSERT INTO #Users(Value)
        SELECT DISTINCT NULLIF(prm.UserName, N'')
        FROM dbo.PReturnMaster prm
        WHERE ISNULL(prm.UserName, N'') <> N''
          AND (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(prm.CompanyId, 0) = @CompanyId)
          AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(prm.BranchId, 0) = @BranchId)
          AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(prm.FinYearId, 0) = @FinYearId);

        INSERT INTO #Users(Value)
        SELECT DISTINCT NULLIF(sal.UserName, N'')
        FROM dbo.SalesActivityLog sal
        WHERE ISNULL(sal.UserName, N'') <> N''
          AND (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(sal.CompanyId, 0) = @CompanyId)
          AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(sal.BranchId, 0) = @BranchId)
          AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(sal.FinYearId, 0) = @FinYearId);

        INSERT INTO #Users(Value)
        SELECT DISTINCT NULLIF(pal.UserName, N'')
        FROM dbo.PurchaseActivityLog pal
        WHERE ISNULL(pal.UserName, N'') <> N''
          AND (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(pal.CompanyId, 0) = @CompanyId)
          AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(pal.BranchId, 0) = @BranchId)
          AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(pal.FinYearId, 0) = @FinYearId);

        INSERT INTO #Users(Value)
        SELECT DISTINCT NULLIF(sral.UserName, N'')
        FROM dbo.SalesReturnActivityLog sral
        WHERE ISNULL(sral.UserName, N'') <> N''
          AND (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(sral.CompanyId, 0) = @CompanyId)
          AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(sral.BranchId, 0) = @BranchId)
          AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(sral.FinYearId, 0) = @FinYearId);

        INSERT INTO #Users(Value)
        SELECT DISTINCT NULLIF(pral.UserName, N'')
        FROM dbo.PurchaseReturnActivityLog pral
        WHERE ISNULL(pral.UserName, N'') <> N''
          AND (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(pral.CompanyId, 0) = @CompanyId)
          AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(pral.BranchId, 0) = @BranchId)
          AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(pral.FinYearId, 0) = @FinYearId);

        INSERT INTO #Users(Value)
        SELECT DISTINCT NULLIF(saal.UserName, N'')
        FROM dbo.StockAdjustmentActivityLog saal
        WHERE ISNULL(saal.UserName, N'') <> N''
          AND (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(saal.CompanyId, 0) = @CompanyId)
          AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(saal.BranchId, 0) = @BranchId)
          AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(saal.FinYearId, 0) = @FinYearId);

        INSERT INTO #Users(Value)
        SELECT DISTINCT COALESCE(NULLIF(u.UserName, N''), NULLIF(CONVERT(nvarchar(150), sam.UserId), N'0'))
        FROM dbo.StockAdjustmentMaster sam
        LEFT JOIN dbo.Users u ON u.UserID = sam.UserId
        WHERE ISNULL(sam.UserId, 0) <> 0
          AND (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(sam.CompanyId, 0) = @CompanyId)
          AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(sam.BranchId, 0) = @BranchId)
          AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(sam.FinYearId, 0) = @FinYearId);

        SELECT DISTINCT Value
        FROM #Users
        WHERE ISNULL(Value, N'') <> N''
        ORDER BY Value;
        RETURN;
    END

    IF (@_Operation = 'LATESTSTAMP')
    BEGIN
        DECLARE @Latest datetime = NULL;

        SELECT @Latest = MAX(sm.BillDate)
        FROM dbo.SMaster sm
        WHERE ISNULL(sm.CancelFlag, 0) = 0
          AND (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(sm.CompanyId, 0) = @CompanyId)
          AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(sm.BranchId, 0) = @BranchId)
          AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(sm.FinYearId, 0) = @FinYearId);

        SELECT @Latest = CASE WHEN @Latest IS NULL OR MAX(sal.CreatedOn) > @Latest THEN MAX(sal.CreatedOn) ELSE @Latest END
        FROM dbo.SalesActivityLog sal
        WHERE (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(sal.CompanyId, 0) = @CompanyId)
          AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(sal.BranchId, 0) = @BranchId)
          AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(sal.FinYearId, 0) = @FinYearId);

        SELECT @Latest = CASE WHEN @Latest IS NULL OR MAX(CAST(CAST(pm.PurchaseDate AS date) AS datetime)) > @Latest THEN MAX(CAST(CAST(pm.PurchaseDate AS date) AS datetime)) ELSE @Latest END
        FROM dbo.PMaster pm
        WHERE ISNULL(pm.CancelFlag, 0) = 0
          AND (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(pm.CompanyId, 0) = @CompanyId)
          AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(pm.BranchId, 0) = @BranchId)
          AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(pm.FinYearId, 0) = @FinYearId);

        SELECT @Latest = CASE WHEN @Latest IS NULL OR MAX(pal.CreatedOn) > @Latest THEN MAX(pal.CreatedOn) ELSE @Latest END
        FROM dbo.PurchaseActivityLog pal
        WHERE (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(pal.CompanyId, 0) = @CompanyId)
          AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(pal.BranchId, 0) = @BranchId)
          AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(pal.FinYearId, 0) = @FinYearId);

        SELECT @Latest = CASE WHEN @Latest IS NULL OR MAX(srm.SReturnDate) > @Latest THEN MAX(srm.SReturnDate) ELSE @Latest END
        FROM dbo.SReturnMaster srm
        WHERE ISNULL(srm.CancelFlag, 0) = 0
          AND (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(srm.CompanyId, 0) = @CompanyId)
          AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(srm.BranchId, 0) = @BranchId)
          AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(srm.FinYearId, 0) = @FinYearId);

        SELECT @Latest = CASE WHEN @Latest IS NULL OR MAX(sral.CreatedOn) > @Latest THEN MAX(sral.CreatedOn) ELSE @Latest END
        FROM dbo.SalesReturnActivityLog sral
        WHERE (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(sral.CompanyId, 0) = @CompanyId)
          AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(sral.BranchId, 0) = @BranchId)
          AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(sral.FinYearId, 0) = @FinYearId);

        SELECT @Latest = CASE WHEN @Latest IS NULL OR MAX(prm.PReturnDate) > @Latest THEN MAX(prm.PReturnDate) ELSE @Latest END
        FROM dbo.PReturnMaster prm
        WHERE ISNULL(prm.CancelFlag, 0) = 0
          AND (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(prm.CompanyId, 0) = @CompanyId)
          AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(prm.BranchId, 0) = @BranchId)
          AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(prm.FinYearId, 0) = @FinYearId);

        SELECT @Latest = CASE WHEN @Latest IS NULL OR MAX(pral.CreatedOn) > @Latest THEN MAX(pral.CreatedOn) ELSE @Latest END
        FROM dbo.PurchaseReturnActivityLog pral
        WHERE (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(pral.CompanyId, 0) = @CompanyId)
          AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(pral.BranchId, 0) = @BranchId)
          AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(pral.FinYearId, 0) = @FinYearId);

        SELECT @Latest = CASE WHEN @Latest IS NULL OR MAX(saal.CreatedOn) > @Latest THEN MAX(saal.CreatedOn) ELSE @Latest END
        FROM dbo.StockAdjustmentActivityLog saal
        WHERE (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(saal.CompanyId, 0) = @CompanyId)
          AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(saal.BranchId, 0) = @BranchId)
          AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(saal.FinYearId, 0) = @FinYearId);

        SELECT @Latest = CASE WHEN @Latest IS NULL OR MAX(COALESCE(v.UserDate, sam.StockAdjustmentDate)) > @Latest THEN MAX(COALESCE(v.UserDate, sam.StockAdjustmentDate)) ELSE @Latest END
        FROM dbo.StockAdjustmentMaster sam
        OUTER APPLY
        (
            SELECT MAX(v.UserDate) AS UserDate
            FROM dbo.Vouchers v
            WHERE v.VoucherID = sam.VoucherId
              AND ISNULL(v.CompanyID, 0) = ISNULL(sam.CompanyId, 0)
              AND ISNULL(v.BranchID, 0) = ISNULL(sam.BranchId, 0)
              AND ISNULL(v.FinYearID, 0) = ISNULL(sam.FinYearId, 0)
              AND ISNULL(v.VoucherType, N'') = N'PhysicalStock'
        ) v
        WHERE ISNULL(sam.CancelFlag, 0) = 0
          AND (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(sam.CompanyId, 0) = @CompanyId)
          AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(sam.BranchId, 0) = @BranchId)
          AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(sam.FinYearId, 0) = @FinYearId);

        SELECT ISNULL(@Latest, CONVERT(datetime, '19000101', 112));
        RETURN;
    END

    IF (@_Operation NOT IN ('GET', 'COUNT'))
    BEGIN
        RAISERROR('Invalid POS_ItemStockActivityLog operation.', 16, 1);
        RETURN;
    END

    CREATE TABLE #ItemStockActivity
    (
        CreatedOn datetime NOT NULL,
        UserName nvarchar(150) NULL,
        Action nvarchar(50) NOT NULL,
        ActionSort int NOT NULL,
        TransactionNo bigint NOT NULL DEFAULT(0),
        InvoiceNo nvarchar(100) NULL,
        SalesBillNo nvarchar(100) NULL,
        PurchaseNo nvarchar(100) NULL,
        ItemName nvarchar(250) NULL,
        Barcode nvarchar(100) NULL,
        UOM nvarchar(50) NULL,
        Qty decimal(18,4) NULL,
        MovementQty decimal(18,4) NULL,
        UnitPrice decimal(18,4) NULL,
        SellingPrice decimal(18,4) NULL,
        Stock decimal(18,4) NULL,
        StockIn decimal(18,4) NULL,
        StockOut decimal(18,4) NULL,
        AdjustmentQty decimal(18,4) NULL,
        NewBalance decimal(18,4) NULL,
        QtyDifference decimal(18,4) NULL,
        Reason nvarchar(500) NULL,
        Available decimal(18,4) NULL,
        Hold decimal(18,4) NULL,
        Cycle int NULL,
        BoxQty int NULL,
        ActivityDetails nvarchar(max) NULL,
        CompanyId int NOT NULL DEFAULT(0),
        BranchId int NOT NULL DEFAULT(0),
        FinYearId int NOT NULL DEFAULT(0),
        UserId int NOT NULL DEFAULT(0),
        CounterName nvarchar(150) NULL,
        CounterId int NOT NULL DEFAULT(0),
        CounterSessionId bigint NOT NULL DEFAULT(0),
        ActivityLogId bigint NOT NULL DEFAULT(0),
        SlNo int NOT NULL DEFAULT(0),
        ItemId bigint NOT NULL DEFAULT(0),
        UnitId int NOT NULL DEFAULT(0),
        MatchesFilter bit NOT NULL DEFAULT(0)
    );

    INSERT INTO #ItemStockActivity
    (
        CreatedOn, UserName, Action, ActionSort, TransactionNo, InvoiceNo, SalesBillNo, PurchaseNo,
        ItemName, Barcode, UOM, Qty, MovementQty, UnitPrice, SellingPrice, Stock, StockIn, StockOut,
        AdjustmentQty, NewBalance, QtyDifference, Reason, Available, Hold, Cycle, BoxQty, ActivityDetails,
        CompanyId, BranchId, FinYearId, UserId, CounterName, CounterId, CounterSessionId, ActivityLogId,
        SlNo, ItemId, UnitId, MatchesFilter
    )
    SELECT
        COALESCE(sal.CreatedOn, sm.BillDate),
        COALESCE(NULLIF(sal.UserName, N''), NULLIF(u.UserName, N''), NULLIF(CONVERT(nvarchar(150), sm.UserId), N'0')),
        N'Sales',
        1,
        sm.BillNo,
        CONVERT(nvarchar(100), sm.BillNo),
        CONVERT(nvarchar(100), sm.BillNo),
        NULL,
        COALESCE(NULLIF(sd.ItemName, N''), im.Description),
        COALESCE(ps.BarCode, im.BarCode),
        sd.Unit,
        CAST(ISNULL(sd.Qty, 0) AS decimal(18,4)),
        CAST(CASE WHEN ISNULL(sm.Status, N'') = N'Hold' THEN 0 ELSE 0 - ISNULL(sd.Qty, 0) END AS decimal(18,4)),
        CAST(ISNULL(ps.Cost, 0) AS decimal(18,4)),
        CAST(ISNULL(ps.RetailPrice, 0) AS decimal(18,4)),
        CAST(ISNULL(ps.Stock, 0) AS decimal(18,4)),
        CAST(0 AS decimal(18,4)),
        CAST(CASE WHEN ISNULL(sm.Status, N'') = N'Hold' THEN 0 ELSE ISNULL(sd.Qty, 0) END AS decimal(18,4)),
        NULL,
        NULL,
        CAST(CASE WHEN ISNULL(sm.Status, N'') = N'Hold' THEN 0 ELSE 0 - ISNULL(sd.Qty, 0) END AS decimal(18,4)),
        NULL,
        CAST(ISNULL(ps.Stock, 0) - CASE WHEN ISNULL(sm.Status, N'') = N'Hold' THEN ISNULL(sd.Qty, 0) ELSE 0 END AS decimal(18,4)),
        CAST(CASE WHEN ISNULL(sm.Status, N'') = N'Hold' THEN ISNULL(sd.Qty, 0) ELSE 0 END AS decimal(18,4)),
        ISNULL(im.Order_Cycle_Days, 0),
        ISNULL(im.Box_Quantity, 0),
        N'Sales Bill No: ' + CONVERT(nvarchar(50), sm.BillNo) + N', Customer: ' + ISNULL(sm.CustomerName, N''),
        ISNULL(sm.CompanyId, 0),
        ISNULL(sm.BranchId, 0),
        ISNULL(sm.FinYearId, 0),
        COALESCE(NULLIF(sal.UserId, 0), ISNULL(sm.UserId, 0)),
        COALESCE(NULLIF(sal.CounterName, N''), CASE WHEN ISNULL(sm.CounterId, 0) > 0 THEN N'Counter ' + CONVERT(nvarchar(20), sm.CounterId) ELSE NULL END),
        COALESCE(NULLIF(sal.CounterId, 0), ISNULL(sm.CounterId, 0)),
        COALESCE(NULLIF(sal.CounterSessionId, 0), ISNULL(sm.CounterSessionId, 0)),
        ISNULL(sal.ActivityLogId, 0),
        ISNULL(sd.SlNO, 0),
        ISNULL(sd.ItemId, 0),
        ISNULL(sd.UnitId, 0),
        1
    FROM dbo.SMaster sm
    INNER JOIN dbo.SDetails sd ON sd.BillNo = sm.BillNo AND sd.BranchID = sm.BranchId AND sd.CompanyId = sm.CompanyId AND sd.FinYearId = sm.FinYearId
    LEFT JOIN dbo.ItemMaster im ON im.ItemId = sd.ItemId
    LEFT JOIN dbo.Users u ON u.UserID = sm.UserId
    LEFT JOIN dbo.SalesActivityLog sal ON sal.TransactionNo = sm.BillNo
        AND ISNULL(sal.CompanyId, 0) = ISNULL(sm.CompanyId, 0)
        AND ISNULL(sal.BranchId, 0) = ISNULL(sm.BranchId, 0)
        AND ISNULL(sal.FinYearId, 0) = ISNULL(sm.FinYearId, 0)
        AND ISNULL(sal.ActivityType, N'') IN (N'SAVE', N'UPDATE', N'COMPLETE HOLD')
    OUTER APPLY
    (
        SELECT TOP 1 ps.*
        FROM dbo.PriceSettings ps
        WHERE ps.ItemId = sd.ItemId
        ORDER BY
            CASE WHEN ISNULL(ps.BranchId, 0) = ISNULL(sm.BranchId, 0) THEN 0 ELSE 1 END,
            CASE WHEN ISNULL(ps.UnitId, 0) = ISNULL(sd.UnitId, 0) THEN 0 ELSE 1 END,
            ps.UnitId
    ) ps
    WHERE ISNULL(sm.CancelFlag, 0) = 0
      AND COALESCE(sal.CreatedOn, sm.BillDate) >= @FromDate AND COALESCE(sal.CreatedOn, sm.BillDate) < DATEADD(DAY, 1, @ToDate)
      AND (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(sm.CompanyId, 0) = @CompanyId)
      AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(sm.BranchId, 0) = @BranchId)
      AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(sm.FinYearId, 0) = @FinYearId)
      AND (@UserName = N'' OR COALESCE(NULLIF(sal.UserName, N''), NULLIF(u.UserName, N''), NULLIF(CONVERT(nvarchar(150), sm.UserId), N'0')) = @UserName)
      AND (@Action = N'' OR @Action = N'Sales')
      AND (@ItemSearch = N'' OR COALESCE(NULLIF(sd.ItemName, N''), im.Description, N'') LIKE N'%' + @ItemSearch + N'%' OR COALESCE(ps.BarCode, im.BarCode, N'') LIKE N'%' + @ItemSearch + N'%');

    INSERT INTO #ItemStockActivity
    (
        CreatedOn, UserName, Action, ActionSort, TransactionNo, InvoiceNo, SalesBillNo, PurchaseNo,
        ItemName, Barcode, UOM, Qty, MovementQty, UnitPrice, SellingPrice, Stock, StockIn, StockOut,
        AdjustmentQty, NewBalance, QtyDifference, Reason, Available, Hold, Cycle, BoxQty, ActivityDetails,
        CompanyId, BranchId, FinYearId, UserId, CounterName, CounterId, CounterSessionId, ActivityLogId,
        SlNo, ItemId, UnitId, MatchesFilter
    )
    SELECT
        COALESCE(pal.CreatedOn, CAST(CAST(pm.PurchaseDate AS date) AS datetime)),
        COALESCE(NULLIF(pal.UserName, N''), pm.UserName),
        N'Purchase',
        2,
        pm.PurchaseNo,
        pm.InvoiceNo,
        NULL,
        CONVERT(nvarchar(100), pm.PurchaseNo),
        COALESCE(NULLIF(pd.ItemName, N''), im.Description),
        COALESCE(ps.BarCode, im.BarCode),
        pd.Unit,
        CAST(ISNULL(pd.Qty, 0) AS decimal(18,4)),
        CAST(ISNULL(pd.Qty, 0) AS decimal(18,4)),
        CAST(ISNULL(pd.Cost, 0) AS decimal(18,4)),
        CAST(ISNULL(pd.SalesPrice, 0) AS decimal(18,4)),
        CAST(ISNULL(ps.Stock, 0) AS decimal(18,4)),
        CAST(ISNULL(pd.Qty, 0) AS decimal(18,4)),
        CAST(0 AS decimal(18,4)),
        NULL,
        NULL,
        CAST(ISNULL(pd.Qty, 0) AS decimal(18,4)),
        NULL,
        CAST(ISNULL(ps.Stock, 0) AS decimal(18,4)),
        CAST(0 AS decimal(18,4)),
        ISNULL(im.Order_Cycle_Days, 0),
        ISNULL(im.Box_Quantity, 0),
        N'Purchase No: ' + CONVERT(nvarchar(50), pm.PurchaseNo) + N', Vendor: ' + ISNULL(pm.VendorName, N''),
        ISNULL(pm.CompanyId, 0),
        ISNULL(pm.BranchId, 0),
        ISNULL(pm.FinYearId, 0),
        COALESCE(NULLIF(pal.UserId, 0), ISNULL(pm.UserID, 0)),
        NULLIF(pal.CounterName, N''),
        ISNULL(pal.CounterId, 0),
        ISNULL(pal.CounterSessionId, 0),
        ISNULL(pal.ActivityLogId, 0),
        ISNULL(pd.SlNo, 0),
        ISNULL(pd.ItemID, 0),
        ISNULL(pd.UnitId, 0),
        1
    FROM dbo.PMaster pm
    INNER JOIN dbo.PDetails pd ON pd.PurchaseNo = pm.PurchaseNo
        AND (ISNULL(pd.CompanyId, 0) = 0 OR ISNULL(pd.CompanyId, 0) = ISNULL(pm.CompanyId, 0))
        AND (ISNULL(pd.BranchID, 0) = 0 OR ISNULL(pd.BranchID, 0) = ISNULL(pm.BranchId, 0))
        AND (ISNULL(pd.FinYearId, 0) = 0 OR ISNULL(pd.FinYearId, 0) = ISNULL(pm.FinYearId, 0))
    LEFT JOIN dbo.ItemMaster im ON im.ItemId = pd.ItemID
    LEFT JOIN dbo.PurchaseActivityLog pal ON pal.TransactionNo = pm.PurchaseNo
        AND (ISNULL(pal.CompanyId, 0) = 0 OR ISNULL(pal.CompanyId, 0) = ISNULL(pm.CompanyId, 0))
        AND (ISNULL(pal.BranchId, 0) = 0 OR ISNULL(pal.BranchId, 0) = ISNULL(pm.BranchId, 0))
        AND (ISNULL(pal.FinYearId, 0) = 0 OR ISNULL(pal.FinYearId, 0) = ISNULL(pm.FinYearId, 0))
        AND ISNULL(pal.ActivityType, N'') IN (N'SAVE', N'UPDATE')
    OUTER APPLY
    (
        SELECT TOP 1 ps.*
        FROM dbo.PriceSettings ps
        WHERE ps.ItemId = pd.ItemID
        ORDER BY
            CASE WHEN ISNULL(ps.BranchId, 0) = ISNULL(pm.BranchId, 0) THEN 0 ELSE 1 END,
            CASE WHEN ISNULL(ps.UnitId, 0) = ISNULL(pd.UnitId, 0) THEN 0 ELSE 1 END,
            ps.UnitId
    ) ps
    WHERE ISNULL(pm.CancelFlag, 0) = 0
      AND COALESCE(pal.CreatedOn, CAST(CAST(pm.PurchaseDate AS date) AS datetime)) >= @FromDate AND COALESCE(pal.CreatedOn, CAST(CAST(pm.PurchaseDate AS date) AS datetime)) < DATEADD(DAY, 1, @ToDate)
      AND (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(pm.CompanyId, 0) = @CompanyId)
      AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(pm.BranchId, 0) = @BranchId)
      AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(pm.FinYearId, 0) = @FinYearId)
      AND (@UserName = N'' OR COALESCE(NULLIF(pal.UserName, N''), pm.UserName, N'') = @UserName)
      AND (@Action = N'' OR @Action = N'Purchase')
      AND (@ItemSearch = N'' OR COALESCE(NULLIF(pd.ItemName, N''), im.Description, N'') LIKE N'%' + @ItemSearch + N'%' OR COALESCE(ps.BarCode, im.BarCode, N'') LIKE N'%' + @ItemSearch + N'%');

    INSERT INTO #ItemStockActivity
    (
        CreatedOn, UserName, Action, ActionSort, TransactionNo, InvoiceNo, SalesBillNo, PurchaseNo,
        ItemName, Barcode, UOM, Qty, MovementQty, UnitPrice, SellingPrice, Stock, StockIn, StockOut,
        AdjustmentQty, NewBalance, QtyDifference, Reason, Available, Hold, Cycle, BoxQty, ActivityDetails,
        CompanyId, BranchId, FinYearId, UserId, CounterName, CounterId, CounterSessionId, ActivityLogId,
        SlNo, ItemId, UnitId, MatchesFilter
    )
    SELECT
        COALESCE(sral.CreatedOn, srm.SReturnDate),
        COALESCE(NULLIF(sral.UserName, N''), srm.UserName),
        N'Sales Return',
        3,
        srm.SReturnNo,
        srm.InvoiceNo,
        srm.InvoiceNo,
        NULL,
        COALESCE(NULLIF(srd.ItemName, N''), im.Description),
        COALESCE(ps.BarCode, im.BarCode),
        srd.Unit,
        CAST(ISNULL(NULLIF(srd.ReturnQty, 0), srd.Qty) AS decimal(18,4)),
        CAST(ISNULL(NULLIF(srd.ReturnQty, 0), srd.Qty) AS decimal(18,4)),
        CAST(ISNULL(srd.SalesPrice, 0) AS decimal(18,4)),
        CAST(ISNULL(srd.SalesPrice, 0) AS decimal(18,4)),
        CAST(ISNULL(ps.Stock, 0) AS decimal(18,4)),
        CAST(ISNULL(NULLIF(srd.ReturnQty, 0), srd.Qty) AS decimal(18,4)),
        CAST(0 AS decimal(18,4)),
        NULL,
        NULL,
        CAST(ISNULL(NULLIF(srd.ReturnQty, 0), srd.Qty) AS decimal(18,4)),
        NULL,
        CAST(ISNULL(ps.Stock, 0) AS decimal(18,4)),
        CAST(0 AS decimal(18,4)),
        ISNULL(im.Order_Cycle_Days, 0),
        ISNULL(im.Box_Quantity, 0),
        N'Sales Return No: ' + CONVERT(nvarchar(50), srm.SReturnNo) + N', Customer: ' + ISNULL(srm.CustomerName, N''),
        ISNULL(srm.CompanyId, 0),
        ISNULL(srm.BranchId, 0),
        ISNULL(srm.FinYearId, 0),
        COALESCE(NULLIF(sral.UserId, 0), ISNULL(srm.UserID, 0)),
        NULLIF(sral.CounterName, N''),
        ISNULL(sral.CounterId, 0),
        ISNULL(sral.CounterSessionId, 0),
        ISNULL(sral.ActivityLogId, 0),
        ISNULL(srd.SlNo, 0),
        ISNULL(srd.ItemId, 0),
        ISNULL(srd.UnitId, 0),
        1
    FROM dbo.SReturnMaster srm
    INNER JOIN dbo.SReturnDetails srd ON srd.SReturnNo = srm.SReturnNo AND srd.BranchID = srm.BranchId AND srd.CompanyId = srm.CompanyId AND srd.FinYearId = srm.FinYearId
    LEFT JOIN dbo.ItemMaster im ON im.ItemId = srd.ItemId
    LEFT JOIN dbo.SalesReturnActivityLog sral ON sral.TransactionNo = srm.SReturnNo
        AND ISNULL(sral.CompanyId, 0) = ISNULL(srm.CompanyId, 0)
        AND ISNULL(sral.BranchId, 0) = ISNULL(srm.BranchId, 0)
        AND ISNULL(sral.FinYearId, 0) = ISNULL(srm.FinYearId, 0)
        AND ISNULL(sral.ActivityType, N'') IN (N'SAVE', N'UPDATE')
    OUTER APPLY
    (
        SELECT TOP 1 ps.*
        FROM dbo.PriceSettings ps
        WHERE ps.ItemId = srd.ItemId
        ORDER BY
            CASE WHEN ISNULL(ps.BranchId, 0) = ISNULL(srm.BranchId, 0) THEN 0 ELSE 1 END,
            CASE WHEN ISNULL(ps.UnitId, 0) = ISNULL(srd.UnitId, 0) THEN 0 ELSE 1 END,
            ps.UnitId
    ) ps
    WHERE ISNULL(srm.CancelFlag, 0) = 0
      AND COALESCE(sral.CreatedOn, srm.SReturnDate) >= @FromDate AND COALESCE(sral.CreatedOn, srm.SReturnDate) < DATEADD(DAY, 1, @ToDate)
      AND (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(srm.CompanyId, 0) = @CompanyId)
      AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(srm.BranchId, 0) = @BranchId)
      AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(srm.FinYearId, 0) = @FinYearId)
      AND (@UserName = N'' OR COALESCE(NULLIF(sral.UserName, N''), srm.UserName, N'') = @UserName)
      AND (@Action = N'' OR @Action = N'Sales Return')
      AND (@ItemSearch = N'' OR COALESCE(NULLIF(srd.ItemName, N''), im.Description, N'') LIKE N'%' + @ItemSearch + N'%' OR COALESCE(ps.BarCode, im.BarCode, N'') LIKE N'%' + @ItemSearch + N'%');

    INSERT INTO #ItemStockActivity
    (
        CreatedOn, UserName, Action, ActionSort, TransactionNo, InvoiceNo, SalesBillNo, PurchaseNo,
        ItemName, Barcode, UOM, Qty, MovementQty, UnitPrice, SellingPrice, Stock, StockIn, StockOut,
        AdjustmentQty, NewBalance, QtyDifference, Reason, Available, Hold, Cycle, BoxQty, ActivityDetails,
        CompanyId, BranchId, FinYearId, UserId, CounterName, CounterId, CounterSessionId, ActivityLogId,
        SlNo, ItemId, UnitId, MatchesFilter
    )
    SELECT
        COALESCE(pral.CreatedOn, prm.PReturnDate),
        COALESCE(NULLIF(pral.UserName, N''), prm.UserName),
        N'Purchase Return',
        4,
        prm.PReturnNo,
        prm.InvoiceNo,
        NULL,
        prm.InvoiceNo,
        im.Description,
        COALESCE(ps.BarCode, im.BarCode),
        COALESCE(ps.Unit, N''),
        CAST(ISNULL(prd.Qty, 0) AS decimal(18,4)),
        CAST(0 - ISNULL(prd.Qty, 0) AS decimal(18,4)),
        CAST(ISNULL(prd.Cost, 0) AS decimal(18,4)),
        CAST(ISNULL(prd.SalesPrice, 0) AS decimal(18,4)),
        CAST(ISNULL(ps.Stock, 0) AS decimal(18,4)),
        CAST(0 AS decimal(18,4)),
        CAST(ISNULL(prd.Qty, 0) AS decimal(18,4)),
        NULL,
        NULL,
        CAST(0 - ISNULL(prd.Qty, 0) AS decimal(18,4)),
        NULL,
        CAST(ISNULL(ps.Stock, 0) AS decimal(18,4)),
        CAST(0 AS decimal(18,4)),
        ISNULL(im.Order_Cycle_Days, 0),
        ISNULL(im.Box_Quantity, 0),
        N'Purchase Return No: ' + CONVERT(nvarchar(50), prm.PReturnNo) + N', Vendor: ' + ISNULL(prm.VendorName, N''),
        ISNULL(prm.CompanyId, 0),
        ISNULL(prm.BranchId, 0),
        ISNULL(prm.FinYearId, 0),
        COALESCE(NULLIF(pral.UserId, 0), ISNULL(prm.UserID, 0)),
        NULLIF(pral.CounterName, N''),
        ISNULL(pral.CounterId, 0),
        ISNULL(pral.CounterSessionId, 0),
        ISNULL(pral.ActivityLogId, 0),
        ISNULL(prd.SlNo, 0),
        ISNULL(prd.ItemID, 0),
        ISNULL(prd.UnitId, 0),
        1
    FROM dbo.PReturnMaster prm
    INNER JOIN dbo.PReturnDetails prd ON prd.PReturnNo = prm.PReturnNo AND prd.BranchID = prm.BranchId AND prd.CompanyId = prm.CompanyId AND prd.FinYearId = prm.FinYearId
    LEFT JOIN dbo.ItemMaster im ON im.ItemId = prd.ItemID
    LEFT JOIN dbo.PurchaseReturnActivityLog pral ON pral.TransactionNo = prm.PReturnNo
        AND ISNULL(pral.CompanyId, 0) = ISNULL(prm.CompanyId, 0)
        AND ISNULL(pral.BranchId, 0) = ISNULL(prm.BranchId, 0)
        AND ISNULL(pral.FinYearId, 0) = ISNULL(prm.FinYearId, 0)
        AND ISNULL(pral.ActivityType, N'') IN (N'SAVE', N'UPDATE')
    OUTER APPLY
    (
        SELECT TOP 1 ps.*
        FROM dbo.PriceSettings ps
        WHERE ps.ItemId = prd.ItemID
        ORDER BY
            CASE WHEN ISNULL(ps.BranchId, 0) = ISNULL(prm.BranchId, 0) THEN 0 ELSE 1 END,
            CASE WHEN ISNULL(ps.UnitId, 0) = ISNULL(prd.UnitId, 0) THEN 0 ELSE 1 END,
            ps.UnitId
    ) ps
    WHERE ISNULL(prm.CancelFlag, 0) = 0
      AND COALESCE(pral.CreatedOn, prm.PReturnDate) >= @FromDate AND COALESCE(pral.CreatedOn, prm.PReturnDate) < DATEADD(DAY, 1, @ToDate)
      AND (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(prm.CompanyId, 0) = @CompanyId)
      AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(prm.BranchId, 0) = @BranchId)
      AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(prm.FinYearId, 0) = @FinYearId)
      AND (@UserName = N'' OR COALESCE(NULLIF(pral.UserName, N''), prm.UserName, N'') = @UserName)
      AND (@Action = N'' OR @Action = N'Purchase Return')
      AND (@ItemSearch = N'' OR ISNULL(im.Description, N'') LIKE N'%' + @ItemSearch + N'%' OR COALESCE(ps.BarCode, im.BarCode, N'') LIKE N'%' + @ItemSearch + N'%');

    INSERT INTO #ItemStockActivity
    (
        CreatedOn, UserName, Action, ActionSort, TransactionNo, InvoiceNo, SalesBillNo, PurchaseNo,
        ItemName, Barcode, UOM, Qty, MovementQty, UnitPrice, SellingPrice, Stock, StockIn, StockOut,
        AdjustmentQty, NewBalance, QtyDifference, Reason, Available, Hold, Cycle, BoxQty, ActivityDetails,
        CompanyId, BranchId, FinYearId, UserId, CounterName, CounterId, CounterSessionId, ActivityLogId,
        SlNo, ItemId, UnitId, MatchesFilter
    )
    SELECT
        COALESCE(saal.CreatedOn, v.UserDate, sam.StockAdjustmentDate),
        COALESCE(NULLIF(saal.UserName, N''), NULLIF(u.UserName, N''), NULLIF(CONVERT(nvarchar(150), sam.UserId), N'0')),
        CASE WHEN ISNULL(sad.QtyDifference, 0) >= 0 THEN N'Stock IN' ELSE N'Stock OUT' END,
        5,
        sam.StockAdjustmentNo,
        CONVERT(nvarchar(100), sam.StockAdjustmentNo),
        NULL,
        NULL,
        im.Description,
        im.BarCode,
        um.UnitName,
        CAST(ABS(ISNULL(sad.QtyDifference, 0)) AS decimal(18,4)),
        CAST(ISNULL(sad.QtyDifference, 0) AS decimal(18,4)),
        CAST(ISNULL(sad.Cost, 0) AS decimal(18,4)),
        CAST(0 AS decimal(18,4)),
        CAST(ISNULL(sad.PhysicalStock, 0) AS decimal(18,4)),
        CAST(CASE WHEN ISNULL(sad.QtyDifference, 0) > 0 THEN sad.QtyDifference ELSE 0 END AS decimal(18,4)),
        CAST(CASE WHEN ISNULL(sad.QtyDifference, 0) < 0 THEN ABS(sad.QtyDifference) ELSE 0 END AS decimal(18,4)),
        CAST(ISNULL(sad.QtyDifference, 0) AS decimal(18,4)),
        CAST(ISNULL(sad.PhysicalStock, 0) AS decimal(18,4)),
        CAST(ISNULL(sad.QtyDifference, 0) AS decimal(18,4)),
        COALESCE(NULLIF(sad.Reason, N''), NULLIF(sam.Comments, N''), lm.LedgerName),
        CAST(ISNULL(sad.PhysicalStock, 0) AS decimal(18,4)),
        CAST(0 AS decimal(18,4)),
        ISNULL(im.Order_Cycle_Days, 0),
        ISNULL(im.Box_Quantity, 0),
        N'Stock Adjustment No: ' + CONVERT(nvarchar(50), sam.StockAdjustmentNo) + N', Reason: ' + ISNULL(COALESCE(NULLIF(sad.Reason, N''), NULLIF(sam.Comments, N''), lm.LedgerName), N''),
        ISNULL(sam.CompanyId, 0),
        ISNULL(sam.BranchId, 0),
        ISNULL(sam.FinYearId, 0),
        COALESCE(NULLIF(saal.UserId, 0), ISNULL(sam.UserId, 0)),
        NULLIF(saal.CounterName, N''),
        ISNULL(saal.CounterId, 0),
        ISNULL(saal.CounterSessionId, 0),
        ISNULL(saal.ActivityLogId, ISNULL(sam.Id, 0)),
        ISNULL(sad.SlNo, 0),
        ISNULL(sad.ItemId, 0),
        ISNULL(sad.UnitId, 0),
        1
    FROM dbo.StockAdjustmentMaster sam
    INNER JOIN dbo.StockAdjustmentDetails sad ON sad.StockAdjustmentMasterId = sam.Id
    LEFT JOIN dbo.ItemMaster im ON im.ItemId = sad.ItemId
    LEFT JOIN dbo.UnitMaster um ON um.UnitID = sad.UnitId
    LEFT JOIN dbo.LedgerMaster lm ON lm.LedgerID = sam.LedgerId
    LEFT JOIN dbo.Users u ON u.UserID = sam.UserId
    LEFT JOIN dbo.StockAdjustmentActivityLog saal ON saal.TransactionNo = sam.StockAdjustmentNo
        AND ISNULL(saal.CompanyId, 0) = ISNULL(sam.CompanyId, 0)
        AND ISNULL(saal.BranchId, 0) = ISNULL(sam.BranchId, 0)
        AND ISNULL(saal.FinYearId, 0) = ISNULL(sam.FinYearId, 0)
        AND ISNULL(saal.ActivityType, N'') IN (N'SAVE', N'UPDATE')
    OUTER APPLY
    (
        SELECT MAX(v.UserDate) AS UserDate
        FROM dbo.Vouchers v
        WHERE v.VoucherID = sam.VoucherId
          AND ISNULL(v.CompanyID, 0) = ISNULL(sam.CompanyId, 0)
          AND ISNULL(v.BranchID, 0) = ISNULL(sam.BranchId, 0)
          AND ISNULL(v.FinYearID, 0) = ISNULL(sam.FinYearId, 0)
          AND ISNULL(v.VoucherType, N'') = N'PhysicalStock'
    ) v
    WHERE ISNULL(sam.CancelFlag, 0) = 0
      AND ISNULL(sad.CancelFlag, 0) = 0
      AND COALESCE(saal.CreatedOn, v.UserDate, sam.StockAdjustmentDate) >= @FromDate
      AND COALESCE(saal.CreatedOn, v.UserDate, sam.StockAdjustmentDate) < DATEADD(DAY, 1, @ToDate)
      AND (@CompanyId IS NULL OR @CompanyId = 0 OR ISNULL(sam.CompanyId, 0) = @CompanyId)
      AND (@BranchId IS NULL OR @BranchId = 0 OR ISNULL(sam.BranchId, 0) = @BranchId)
      AND (@FinYearId IS NULL OR @FinYearId = 0 OR ISNULL(sam.FinYearId, 0) = @FinYearId)
      AND (@UserName = N'' OR COALESCE(NULLIF(saal.UserName, N''), NULLIF(u.UserName, N''), NULLIF(CONVERT(nvarchar(150), sam.UserId), N'0')) = @UserName)
      AND (@Action = N'' OR @Action = CASE WHEN ISNULL(sad.QtyDifference, 0) >= 0 THEN N'Stock IN' ELSE N'Stock OUT' END)
      AND (@ItemSearch = N''
           OR ISNULL(im.Description, N'') LIKE N'%' + @ItemSearch + N'%'
           OR ISNULL(im.BarCode, N'') LIKE N'%' + @ItemSearch + N'%');

    IF (@_Operation = 'COUNT')
    BEGIN
        SELECT COUNT(1) FROM #ItemStockActivity WHERE MatchesFilter = 1;
        RETURN;
    END

    ;WITH StockTimeline AS
    (
        SELECT
            a.*,
            ISNULL(a.Stock, 0) - ISNULL(
                SUM(ISNULL(a.MovementQty, 0)) OVER (
                    PARTITION BY a.ItemId, a.BranchId
                    ORDER BY a.CreatedOn DESC, a.ActivityLogId DESC, a.ActionSort, a.TransactionNo DESC, a.SlNo DESC
                    ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING
                ), 0) AS TimelineStock
        FROM #ItemStockActivity a
    )
    SELECT
        COUNT(*) OVER ()
            - ROW_NUMBER() OVER (ORDER BY CreatedOn DESC, ActivityLogId DESC, ActionSort, TransactionNo DESC, SlNo DESC)
            + 1 AS DisplayLogNo,
        CreatedOn,
        UserName,
        Action,
        TransactionNo,
        InvoiceNo,
        SalesBillNo,
        PurchaseNo,
        ItemName,
        Barcode,
        UOM,
        Qty,
        UnitPrice,
        SellingPrice,
        TimelineStock AS Stock,
        StockIn,
        StockOut,
        AdjustmentQty,
        NewBalance,
        QtyDifference,
        Reason,
        TimelineStock - ISNULL((
            SELECT SUM(ISNULL(hd.Qty, 0))
            FROM dbo.SMaster hm
            INNER JOIN dbo.SDetails hd ON hd.BillNo = hm.BillNo
            WHERE ISNULL(hm.Status, N'') = N'Hold'
              AND hd.ItemId = StockTimeline.ItemId
              AND ISNULL(hm.CompanyId, 0) = ISNULL(StockTimeline.CompanyId, 0)
              AND ISNULL(hm.BranchId, 0) = ISNULL(StockTimeline.BranchId, 0)
              AND ISNULL(hm.FinYearId, 0) = ISNULL(StockTimeline.FinYearId, 0)
        ), 0) AS Available,
        Hold,
        Cycle,
        BoxQty,
        ActivityDetails,
        CompanyId,
        BranchId,
        FinYearId,
        UserId,
        CounterName,
        CounterId,
        CounterSessionId,
        ActivityLogId,
        SlNo,
        ItemId,
        UnitId
    FROM StockTimeline
    WHERE MatchesFilter = 1
    ORDER BY CreatedOn DESC, ActivityLogId DESC, ActionSort, TransactionNo DESC, SlNo DESC;
END
GO
