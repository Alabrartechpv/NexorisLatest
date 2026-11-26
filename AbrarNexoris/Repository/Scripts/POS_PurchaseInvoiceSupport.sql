-- Install before deploying the corresponding PurchaseInvoiceRepository build.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- Apply the schema changes that previously ran from the repository constructor
-- and from the stock-cost update path.
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.PMaster') AND name = N'CurSymbol' AND is_nullable = 0
)
BEGIN
    DECLARE @DefaultName sysname;
    SELECT @DefaultName = name FROM sys.default_constraints
    WHERE parent_object_id = OBJECT_ID(N'dbo.PMaster')
      AND parent_column_id = COLUMNPROPERTY(OBJECT_ID(N'dbo.PMaster'), N'CurSymbol', 'ColumnId');

    IF @DefaultName IS NOT NULL
    BEGIN
        DECLARE @DropDefault nvarchar(400) = N'ALTER TABLE dbo.PMaster DROP CONSTRAINT ' + QUOTENAME(@DefaultName);
        EXEC sys.sp_executesql @DropDefault;
    END;

    ALTER TABLE dbo.PMaster ALTER COLUMN CurSymbol nvarchar(50) NULL;
END;

IF OBJECT_ID(N'dbo.PMaster', N'U') IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM sys.default_constraints
    WHERE parent_object_id = OBJECT_ID(N'dbo.PMaster')
      AND parent_column_id = COLUMNPROPERTY(OBJECT_ID(N'dbo.PMaster'), N'CurSymbol', 'ColumnId')
)
    ALTER TABLE dbo.PMaster ADD CONSTRAINT DF_PMaster_CurSymbol DEFAULT ('RM') FOR CurSymbol;

IF OBJECT_ID(N'dbo.PriceSettings', N'U') IS NOT NULL
    AND COL_LENGTH(N'dbo.PriceSettings', N'SingleItemCost') IS NULL
    ALTER TABLE dbo.PriceSettings ADD SingleItemCost float NULL;
GO

IF OBJECT_ID(N'dbo.POS_PurchaseInvoiceSupport', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo.POS_PurchaseInvoiceSupport AS RETURN 0;');
GO

ALTER PROCEDURE dbo.POS_PurchaseInvoiceSupport
    @_Operation varchar(30),
    @CompanyId int = NULL,
    @BranchId int = NULL,
    @FinYearId int = NULL,
    @PurchaseNo int = NULL,
    @VoucherId bigint = NULL,
    @ItemId int = NULL,
    @UnitId int = NULL,
    @LedgerId int = NULL,
    @CashGroupId int = NULL,
    @BaseUnitCost float = NULL,
    @AverageBaseCost float = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @_Operation = 'GET_TRANSACTION_GUID'
    BEGIN
        SELECT TransactionGuid
        FROM dbo.PMaster
        WHERE PurchaseNo = @PurchaseNo AND BranchId = @BranchId
          AND CompanyId = @CompanyId AND FinYearId = @FinYearId;
        RETURN;
    END;

    IF @_Operation = 'GET_ACTIVE_PAYMENTS'
    BEGIN
        SELECT DISTINCT p.VoucherId
        FROM dbo.VendorPaymentDetails d
        JOIN dbo.VendorPaymentMaster p ON p.Id = d.PaymentMasterId AND p.BranchId = d.BranchId
        JOIN dbo.PMaster m ON m.PurchaseNo = d.BillNo AND m.BranchId = d.BranchId
                         AND m.LedgerId = p.VendorLedgerId
        WHERE d.BillNo = @PurchaseNo
          AND (ISNULL(@BranchId, 0) = 0 OR d.BranchId = @BranchId)
          AND (ISNULL(@FinYearId, 0) = 0 OR m.FinYearId = @FinYearId)
          AND ISNULL(m.CancelFlag, 0) = 0 AND ISNULL(p.CancelFlag, 0) = 0 AND ISNULL(d.CancelFlag, 0) = 0;
        RETURN;
    END;

    IF @_Operation = 'GET_ACTIVE_RETURNS'
    BEGIN
        SELECT DISTINCT r.PReturnNo
        FROM dbo.PReturnMaster r
        WHERE (r.InvoiceNo = CONVERT(varchar(50), @PurchaseNo)
            OR r.PInvoice = CONVERT(varchar(50), @PurchaseNo)
            OR EXISTS (
                SELECT 1 FROM dbo.PMaster m
                WHERE m.PurchaseNo = @PurchaseNo AND (ISNULL(@BranchId, 0) = 0 OR m.BranchId = @BranchId)
                  AND m.InvoiceNo = r.InvoiceNo AND m.LedgerId = r.LedgerId
            ))
          AND (ISNULL(@BranchId, 0) = 0 OR r.BranchId = @BranchId)
          AND (ISNULL(@FinYearId, 0) = 0 OR r.FinYearId = @FinYearId)
          AND ISNULL(r.CancelFlag, 0) = 0;
        RETURN;
    END;

    IF @_Operation = 'GET_OLD_DETAILS'
    BEGIN
        SELECT ItemId, UnitId, ISNULL(Qty, 0) AS Qty, ISNULL(Cost, 0) AS Cost,
               ISNULL(Free, 0) AS Free, ISNULL(Packing, 1) AS Packing
        FROM dbo.PDetails
        WHERE PurchaseNo = @PurchaseNo AND FinYearId = @FinYearId
          AND BranchID = @BranchId AND CompanyId = @CompanyId;
        RETURN;
    END;

    IF @_Operation = 'GET_ITEM_PRICES'
    BEGIN
        SELECT ISNULL(Cost, 0) AS Cost, ISNULL(Stock, 0) AS Stock,
               ISNULL(RetailPrice, 0) AS RetailPrice, ISNULL(WholeSalePrice, 0) AS WholeSalePrice,
               ISNULL(CreditPrice, 0) AS CreditPrice, ISNULL(MDRetailPrice, 0) AS MDRetailPrice,
               ISNULL(MDWalkinPrice, 0) AS MDWalkinPrice, ISNULL(MDCreditPrice, 0) AS MDCreditPrice,
               ISNULL(MDMrpPrice, 0) AS MDMrpPrice, ISNULL(MDCardPrice, 0) AS MDCardPrice,
               ISNULL(MDStaffPrice, 0) AS MDStaffPrice, ISNULL(MDMinPrice, 0) AS MDMinPrice
        FROM dbo.PriceSettings
        WHERE BranchId = @BranchId AND ItemId = @ItemId AND UnitId = @UnitId;
        RETURN;
    END;

    IF @_Operation = 'GET_BASE_UNIT_PRICES'
    BEGIN
        SELECT TOP (1) ISNULL(Cost, 0) AS Cost, ISNULL(Stock, 0) AS Stock, ISNULL(Packing, 1) AS Packing
        FROM dbo.PriceSettings
        WHERE BranchId = @BranchId AND ItemId = @ItemId
        ORDER BY CASE WHEN IsBaseUnit = 'Y' THEN 0 WHEN Packing = 1 THEN 1 ELSE 2 END, UnitId;
        RETURN;
    END;

    IF @_Operation = 'SET_ITEM_COST'
    BEGIN
        UPDATE dbo.PriceSettings
        SET Cost = @BaseUnitCost * CASE WHEN Packing > 0 THEN Packing ELSE 1 END,
            SingleItemCost = @AverageBaseCost
        WHERE BranchId = @BranchId AND ItemId = @ItemId;
        RETURN;
    END;

    IF @_Operation = 'GET_CASH_BALANCE'
    BEGIN
        SELECT ISNULL(SUM(ISNULL(l.OpnDebit, 0) - ISNULL(l.OpnCredit, 0) +
                          ISNULL(v.Debit, 0) - ISNULL(v.Credit, 0)), 0)
        FROM dbo.LedgerMaster l
        LEFT JOIN (
            SELECT LedgerId, SUM(ISNULL(Debit, 0)) AS Debit, SUM(ISNULL(Credit, 0)) AS Credit
            FROM dbo.Vouchers
            WHERE (@BranchId = 0 OR BranchId = @BranchId) AND ISNULL(CancelFlag, 0) = 0
            GROUP BY LedgerId
        ) v ON v.LedgerId = l.LedgerId
        WHERE (@BranchId = 0 OR l.BranchId = @BranchId)
          AND (l.GroupId = @CashGroupId OR l.LedgerId = @LedgerId OR UPPER(l.LedgerName) LIKE '%CASH%');
        RETURN;
    END;

    IF @_Operation = 'GET_OLD_CASH_CREDIT'
    BEGIN
        SELECT ISNULL(SUM(ISNULL(Credit, 0)), 0)
        FROM dbo.Vouchers
        WHERE VoucherId = @VoucherId AND BranchId = @BranchId AND VoucherType = 'Purchase'
          AND ISNULL(CancelFlag, 0) = 0
          AND (GroupId = @CashGroupId OR LedgerName IN ('CASH-IN-HAND', 'CASH'));
        RETURN;
    END;

    THROW 50001, 'Unknown purchase invoice support operation.', 1;
END;
GO
