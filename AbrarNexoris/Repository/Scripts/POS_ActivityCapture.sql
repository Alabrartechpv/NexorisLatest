-- Install before POS_ActivityReport.sql. Audit entries commit or roll back with the business write.
IF OBJECT_ID(N'dbo.POS_ActivityChange', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.POS_ActivityChange
    (
        EventId uniqueidentifier NOT NULL,
        WriteTransactionId bigint NULL,
        WriteStatementId uniqueidentifier NULL,
        DirectVoucherEditId uniqueidentifier NULL,
        ActivityDate datetime2(3) NOT NULL,
        CompanyId int NULL,
        BranchId int NULL,
        FinYearId int NULL,
        UserId int NULL,
        CounterId int NULL,
        Category nvarchar(24) NOT NULL,
        Module nvarchar(80) NOT NULL,
        SourceTable sysname NOT NULL,
        DocumentNo nvarchar(100) NULL,
        ActionType varchar(10) NOT NULL,
        RowVersion varchar(6) NOT NULL,
        Amount decimal(28,4) NULL,
        Details nvarchar(2000) NOT NULL,
        CONSTRAINT PK_POS_ActivityChange PRIMARY KEY CLUSTERED (ActivityDate, EventId)
    );
END;
GO

IF COL_LENGTH(N'dbo.POS_ActivityChange', N'WriteTransactionId') IS NULL
    ALTER TABLE dbo.POS_ActivityChange ADD WriteTransactionId bigint NULL;
IF COL_LENGTH(N'dbo.POS_ActivityChange', N'WriteStatementId') IS NULL
    ALTER TABLE dbo.POS_ActivityChange ADD WriteStatementId uniqueidentifier NULL;
IF COL_LENGTH(N'dbo.POS_ActivityChange', N'DirectVoucherEditId') IS NULL
    ALTER TABLE dbo.POS_ActivityChange ADD DirectVoucherEditId uniqueidentifier NULL;
GO

IF OBJECT_ID(N'dbo.POS_ActivityCaptureInstallation', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.POS_ActivityCaptureInstallation
    (
        Id tinyint NOT NULL PRIMARY KEY CHECK (Id = 1),
        InstalledOn datetime2(3) NOT NULL
    );
    INSERT dbo.POS_ActivityCaptureInstallation VALUES (1, SYSDATETIME());
END;
GO

IF OBJECT_ID(N'dbo.POS_SetActivityContext', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo.POS_SetActivityContext AS RETURN 0;');
GO
ALTER PROCEDURE dbo.POS_SetActivityContext
    @CompanyId int, @BranchId int, @FinYearId int, @UserId int, @CounterId int,
    @DirectVoucherEdit bit = 0
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Context varbinary(128) = 0x504F5341 + CONVERT(binary(4), @CompanyId)
        + CONVERT(binary(4), @BranchId) + CONVERT(binary(4), @FinYearId)
        + CONVERT(binary(4), @UserId) + CONVERT(binary(4), @CounterId);
    IF @DirectVoucherEdit = 1 AND @UserId > 0
        SET @Context += 0x01 + CONVERT(binary(16), NEWID());
    SET CONTEXT_INFO @Context;
END;
GO

-- Each entry supplies the business document number, including for tables without a primary key.
-- Keep the original table name in the audit event so the changed detail can be located.
DECLARE @Sources TABLE (TableName sysname, Module nvarchar(80), DocumentColumn sysname, Category nvarchar(24));
INSERT @Sources VALUES
    (N'SMaster', N'Sales Invoice', N'BillNo', N'Transaction'),
    (N'SMasterHold', N'Held Bill', N'BillNo', N'Transaction'),
    (N'PMaster', N'Purchase Invoice', N'PurchaseNo', N'Transaction'),
    (N'SReturnMaster', N'Sales Return', N'SReturnNo', N'Transaction'),
    (N'PReturnMaster', N'Purchase Return', N'PReturnNo', N'Transaction'),
    (N'POrderMaster', N'Purchase Order', N'PONo', N'Transaction'),
    (N'SOrderMaster', N'Sales Order', N'OrderNo', N'Transaction'),
    (N'QuotationMaster', N'Quotation', N'QuotationNo', N'Transaction'),
    (N'DeliveryOrderMaster', N'Delivery Order', N'DeliveryNo', N'Transaction'),
    (N'StockAdjustmentMaster', N'Stock Adjustment', N'StockAdjustmentNo', N'Transaction'),
    (N'StockTransferMaster', N'Stock Transfer', N'StkTrNo', N'Transaction'),
    (N'OfficeSuppliesMaster', N'Office Supplies', N'OfficeSupplyNo', N'Transaction'),
    (N'OfficeSuppliesWithdrawMaster', N'Office Supplies Withdrawal', N'OfficeSupplyNo', N'Transaction'),
    (N'VanSaleStockMaster', N'Van Stock', N'Id', N'Transaction'),
    (N'MobileOrderMaster', N'Mobile Order', N'MobileOrderNumber', N'Transaction'),
    (N'MobileOrderDetails', N'Mobile Order', N'MobileOrderMasterId', N'Transaction'),
    (N'Vouchers', N'Voucher Posting', N'VoucherID', N'Payment'),
    (N'SPaymentDetails', N'Sales Payment', N'BillNo', N'Payment'),
    (N'VendorPaymentMaster', N'Vendor Payment', N'VoucherId', N'Payment'),
    (N'CustomerReceiptMaster', N'Customer Receipt', N'VoucherId', N'Payment'),
    (N'CreditNoteMaster', N'Credit Note', N'VoucherId', N'Payment'),
    (N'DebitNoteMaster', N'Debit Note', N'VoucherId', N'Payment'),
    (N'VendorPaymentDetails', N'Vendor Payment Detail', N'PaymentMasterId', N'Payment'),
    (N'CustomerReceiptDetails', N'Customer Receipt Detail', N'ReceiptMasterId', N'Payment'),
    (N'CreditNoteDetails', N'Credit Note Detail', N'CreditNoteMasterId', N'Payment'),
    (N'DebitNoteDetails', N'Debit Note Detail', N'DebitNoteMasterId', N'Payment'),
    (N'TransporterReceiptMaster', N'Transporter Receipt', N'Id', N'Payment'),
    (N'TransporterReceiptDetails', N'Transporter Receipt Detail', N'ReceiptMasterId', N'Payment'),
    (N'ManualPartyBalance', N'Manual Party Balance', N'Id', N'Payment'),
    (N'ManualPartyBalanceSettlement', N'Party Balance Settlement', N'ManualPartyBalanceId', N'Payment'),
    (N'BankReconciliation', N'Bank Reconciliation', N'VoucherID', N'Payment'),
    (N'SDetails', N'Sales Invoice Detail', N'BillNo', N'Detail'),
    (N'SDetailsHold', N'Held Bill Detail', N'BillNo', N'Detail'),
    (N'PDetails', N'Purchase Invoice Detail', N'PurchaseNo', N'Detail'),
    (N'SReturnDetails', N'Sales Return Detail', N'SReturnNo', N'Detail'),
    (N'PReturnDetails', N'Purchase Return Detail', N'PReturnNo', N'Detail'),
    (N'POrderDetails', N'Purchase Order Detail', N'PONo', N'Detail'),
    (N'SOrderDetails', N'Sales Order Detail', N'OrderNo', N'Detail'),
    (N'QuotationDetails', N'Quotation Detail', N'QuotationNo', N'Detail'),
    (N'DeliveryOrderDetails', N'Delivery Order Detail', N'DeliveryNo', N'Detail'),
    (N'StockAdjustmentDetails', N'Stock Adjustment Detail', N'StockAdjustmentMasterId', N'Detail'),
    (N'StockTransferDetail', N'Stock Transfer Detail', N'StkTrNo', N'Detail'),
    (N'OfficeSuppliesDetails', N'Office Supplies Detail', N'OfficeSuppliesMasterId', N'Detail'),
    (N'OfficeSuppliesWithdrawDetails', N'Withdrawal Detail', N'MasterId', N'Detail'),
    (N'VanSaleStockDetails', N'Van Stock Detail', N'MasterId', N'Detail'),
    (N'ItemAlternativeBarcode', N'Alternative Barcode', N'ItemId', N'Detail'),
    (N'ItemBatch', N'Item Batch', N'ItemId', N'Detail'),
    (N'ItemDimension', N'Item Dimension', N'ItemId', N'Detail'),
    (N'ContactDetails', N'Ledger Contact Detail', N'LedgerID', N'Detail'),
    (N'CardDetails', N'Ledger Card Detail', N'LedgerID', N'Detail'),
    (N'LedgerDetails', N'Ledger Terms', N'LedgerID', N'Detail'),
    (N'QuickPurchasePresetItem', N'Quick Purchase Preset Item', N'PresetId', N'Detail'),
    (N'BranchDetails', N'Branch Detail', N'BranchId', N'Detail'),
    (N'ItemMaster', N'Item Master', N'ItemId', N'Master'),
    (N'LedgerMaster', N'Ledger Master', N'LedgerId', N'Master'),
    (N'VendorMaster', N'Vendor Master', N'Id', N'Master'),
    (N'AccountGroupMaster', N'Account Group', N'GroupID', N'Master'),
    (N'AccountHead', N'Account Head', N'AcHeadId', N'Master'),
    (N'AllocationMethod', N'Allocation Method', N'Id', N'Master'),
    (N'PriceSettings', N'Item Price and Stock', N'ItemId', N'Master'),
    (N'POS_ItemMasterStatusRules', N'Item Status Rule', N'ItemId', N'Master'),
    (N'Category', N'Category', N'Id', N'Master'),
    (N'SubCategory', N'Subcategory', N'Id', N'Master'),
    (N'Group', N'Item Group', N'Id', N'Master'),
    (N'Brands', N'Brand', N'Id', N'Master'),
    (N'ItemTypes', N'Item Type', N'Id', N'Master'),
    (N'ItemLine', N'Item Line', N'Id', N'Master'),
    (N'ItemRack', N'Item Rack', N'Id', N'Master'),
    (N'ItemRow', N'Item Row', N'Id', N'Master'),
    (N'UnitMaster', N'Unit Master', N'UnitID', N'Master'),
    (N'Units', N'Unit', N'Id', N'Master'),
    (N'UnitQuantityCode', N'Unit Quantity Code', N'ID', N'Master'),
    (N'TaxSystem', N'Tax Rate', N'Id', N'Master'),
    (N'TaxType', N'Tax Type', N'Id', N'Master'),
    (N'Currency', N'Currency', N'CurrencyID', N'Master'),
    (N'Country', N'Country', N'CountryID', N'Master'),
    (N'State', N'State', N'StateID', N'Master'),
    (N'Company', N'Company', N'CompanyID', N'Master'),
    (N'CompanyInfo', N'Company Details', N'CompanyID', N'Master'),
    (N'Branch', N'Branch', N'Id', N'Master'),
    (N'Branches', N'Branch', N'Id', N'Master'),
    (N'FinancialYear', N'Financial Year', N'FinYearID', N'Master'),
    (N'BARCODE', N'Barcode Design', N'BarcodeID', N'Master'),
    (N'PayMode', N'Payment Mode', N'PayModeID', N'Master'),
    (N'PartyType', N'Party Type', N'Id', N'Master'),
    (N'PriceLevels', N'Price Level', N'Id', N'Master'),
    (N'PromoCode', N'Promo Code', N'Id', N'Master'),
    (N'OfferType', N'Offer', N'OfferId', N'Master'),
    (N'ItemOfferDiscount', N'Item Offer Discount', N'Id', N'Master'),
    (N'QuickPurchasePreset', N'Quick Purchase Preset', N'PresetId', N'Master'),
    (N'StockAdjustmentReasonMaster', N'Stock Adjustment Reason', N'Id', N'Master'),
    (N'VoucherTypeMaster', N'Voucher Type', N'VoucherID', N'Master'),
    (N'POS_Settings', N'POS Setting', N'SettingId', N'Master'),
    (N'PrintSettings', N'Print Setting', N'Id', N'Master'),
    (N'BillFormats', N'Bill Format', N'Id', N'Master'),
    (N'PurchasePrintBranding', N'Purchase Print Branding', N'BranchId', N'Master'),
    (N'Utilities', N'Utility Setting', N'Id', N'Master'),
    (N'WeighMachineSettings', N'Weighing Scale Setting', N'Id', N'Master'),
    (N'KeyBoardShortCut', N'Keyboard Shortcut', N'ID', N'Master'),
    (N'KeyBoardShortCutMenu', N'Shortcut Menu', N'ID', N'Master'),
    (N'KeyBoardShortCutSubMenu', N'Shortcut Submenu', N'ID', N'Master'),
    (N'TransactionType', N'Transaction Type', N'Id', N'Master'),
    (N'Users', N'User Account', N'UserID', N'User'),
    (N'MobileUsers', N'Mobile User Account', N'Id', N'User'),
    (N'TransporterLogin', N'Transporter Account', N'UserID', N'User'),
    (N'UserLevel', N'User Level', N'UserLevelID', N'User'),
    (N'Userlevels', N'User Level', N'UserLevelID', N'User'),
    (N'UserRights', N'User Rights', N'ID', N'User'),
    (N'Roles', N'Role', N'RoleID', N'User'),
    (N'RolePermissions', N'Role Permission', N'RolePermissionID', N'User'),
    (N'UserGroups', N'User Group', N'UserGroupID', N'User'),
    (N'Forms', N'Form Permission Definition', N'FormID', N'User'),
    (N'CounterMaster', N'Counter', N'CounterID', N'Counter'),
    (N'CounterSessions', N'Counter Session', N'SessionId', N'Counter'),
    (N'DayOpenings', N'Day Opening', N'Id', N'Counter'),
    (N'DayClosing', N'Day Closing', N'ID', N'Counter'),
    (N'DayCloseDenominations', N'Day Closing Denomination', N'DayOpeningId', N'Counter'),
    (N'ShiftClosing', N'Shift Closing', N'ShiftClosingId', N'Counter'),
    (N'ShiftClosingDenominations', N'Shift Denomination', N'ShiftClosingId', N'Counter'),
    (N'ShiftClosingPaymentDetails', N'Shift Payment Detail', N'ShiftClosingId', N'Counter');

-- Only these fields can be copied out of a business table. Passwords, tokens, card/account
-- numbers, contact data, configuration values, and arbitrary free text are excluded.
DECLARE @Fields TABLE (ColumnName sysname PRIMARY KEY);
INSERT @Fields VALUES
    ('Id'), ('SlNo'), ('ItemId'), ('ItemNo'), ('ItemName'), ('Description'), ('HSNCode'), ('Barcode'), ('BarCodeId'),
    ('VoucherID'), ('LedgerId'), ('LedgerName'), ('GroupId'), ('GroupName'), ('VendorLedgerId'), ('CustomerLedgerId'),
    ('UnitId'), ('Unit'), ('UnitName'), ('Packing'), ('Qty'), ('Quantity'), ('Free'),
    ('Cost'), ('UnitCost'), ('Rate'), ('RetailPrice'), ('WalkinPrice'), ('SPrice'),
    ('OpeningStock'), ('Stock'), ('BalanceStock'), ('Available'), ('OpeningQty'), ('ClosingQty'),
    ('DiscountAmt'), ('DiscountAmount'), ('DiscountPer'), ('DiscountPercentage'), ('TaxAmt'), ('TaxPer'), ('TaxType'),
    ('SubTotal'), ('NetAmount'), ('TotalAmount'), ('TotalPrice'), ('GrandTotal'), ('Gross'),
    ('Amount'), ('BillAmount'), ('BalanceAmount'), ('PaymentAmount'), ('ReceiptAmount'), ('SettlementAmount'),
    ('DebitAmount'), ('CreditAmount'), ('Debit'), ('Credit'), ('RoundAmount'), ('RoundOff'),
    ('OpeningCash'), ('ClosingCash'), ('CashDifference'), ('Denomination'), ('Count'),
    ('PaymentType'), ('FormName'), ('PayModeId'), ('PayModeName'), ('PaymentMethodLedgerId'), ('VoucherType'), ('Mode'),
    ('CancelFlag'), ('IsDelete'), ('IsDeleted'), ('IsActive'), ('IsBilled'), ('Status'), ('ItemStatus'),
    ('OrderStatus'), ('OfficeSupplyStatus'), ('ApprovedStatus'), ('BlockSale'), ('BlockPurchase'), ('StatusName'), ('StatusReason'), ('StatusDate'),
    ('OfferId'), ('Name'), ('CategoryName'), ('SubCategoryName'), ('BrandName'), ('ItemType'),
    ('CurrencyName'), ('CountryName'), ('StateName'), ('BranchName'), ('CompanyName'), ('CounterName'),
    ('UserName'), ('UserLevelId'), ('RoleId'), ('RoleName'), ('FormKey'), ('CanView'), ('CanAdd'),
    ('CanEdit'), ('CanDelete'), ('IsAdd'), ('IsEdit'), ('SettingKey'), ('DocumentNo'), ('InvoiceNo'), ('BillNo'),
    ('SessionId'), ('BillDate'), ('PODate'), ('OrderDate'), ('PurchaseDate'), ('VoucherDate'), ('CloseTime');

DECLARE @Table sysname, @Document sysname, @Module nvarchar(80), @Category nvarchar(24);
DECLARE sources CURSOR LOCAL FAST_FORWARD FOR
    SELECT s.TableName, c.name, s.Module, s.Category
    FROM @Sources s
    JOIN sys.tables t ON t.name = s.TableName AND t.schema_id = SCHEMA_ID('dbo')
    JOIN sys.columns c ON c.object_id = t.object_id AND LOWER(c.name) = LOWER(s.DocumentColumn);
OPEN sources;
FETCH NEXT FROM sources INTO @Table, @Document, @Module, @Category;
WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @ObjectId int = OBJECT_ID(N'dbo.' + QUOTENAME(@Table));
    DECLARE @Trigger sysname = N'POS_Activity_' + @Table;
    DECLARE @Company nvarchar(300) = N'@CompanyId', @Branch nvarchar(300) = N'@BranchId',
            @Year nvarchar(300) = N'@FinYearId', @Counter nvarchar(300) = N'@CounterId',
            @InsertUser nvarchar(300) = N'CONVERT(int, NULL)';
    SELECT @Company = COALESCE(MAX(CASE WHEN LOWER(name) = 'companyid'
                THEN N'COALESCE(NULLIF(TRY_CONVERT(int, r.' + QUOTENAME(name) + N'),0), @CompanyId)' END), @Company),
           @Branch = COALESCE(MAX(CASE WHEN LOWER(name) IN ('branchid','requestedbybranchid')
                THEN N'COALESCE(NULLIF(TRY_CONVERT(int, r.' + QUOTENAME(name) + N'),0), @BranchId)' END), @Branch),
           @Year = COALESCE(MAX(CASE WHEN LOWER(name) = 'finyearid'
                THEN N'COALESCE(NULLIF(TRY_CONVERT(int, r.' + QUOTENAME(name) + N'),0), @FinYearId)' END), @Year),
           @Counter = COALESCE(MAX(CASE WHEN LOWER(name) = 'counterid'
                THEN N'CASE WHEN @Action = ''SAVE'' THEN COALESCE(@CounterId, NULLIF(TRY_CONVERT(int, r.'
                     + QUOTENAME(name) + N'),0)) ELSE @CounterId END' END), @Counter),
           @InsertUser = CASE WHEN @Category = N'User' THEN N'CONVERT(int, NULL)'
                ELSE COALESCE(MAX(CASE WHEN LOWER(name) = 'userid'
                     THEN N'NULLIF(TRY_CONVERT(int, r.' + QUOTENAME(name) + N'),0)' END), N'CONVERT(int, NULL)') END
    FROM sys.columns WHERE object_id = @ObjectId;

    DECLARE @Display nvarchar(max);
    SELECT @Display = (
        SELECT N' + N'' | ' + REPLACE(c.name, '''', '''''') + N'='' + COALESCE(CONVERT(nvarchar(150), r.'
               + QUOTENAME(c.name) + N', 120), N''(null)'')'
        FROM sys.columns c
        WHERE c.object_id = @ObjectId
          AND EXISTS (SELECT 1 FROM @Fields f WHERE LOWER(f.ColumnName) = LOWER(c.name))
          AND c.name <> @Document
          AND (LOWER(c.name) NOT IN ('description','name') OR @Table IN ('ItemMaster','VendorMaster','ItemLine','ItemRack','ItemRow'))
        ORDER BY CASE WHEN LOWER(c.name) IN ('slno','id','itemid','ledgerid','barCode','name','description') THEN 0 ELSE 1 END, c.column_id
        FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)');

    DECLARE @Amount nvarchar(300) = N'CONVERT(decimal(28,4), NULL)';
    SELECT TOP (1) @Amount = N'TRY_CONVERT(decimal(28,4), r.' + QUOTENAME(c.name) + N')'
    FROM sys.columns c
    JOIN (VALUES ('netamount',1), ('paymentamount',2), ('receiptamount',3), ('creditamount',4),
                 ('debitamount',5), ('settlementamount',6), ('grandtotal',7), ('totalamount',8),
                 ('totalprice',9), ('amount',10)) a(Name, Priority) ON LOWER(c.name) = a.Name
    WHERE c.object_id = @ObjectId AND @Category IN (N'Payment', N'Transaction', N'Counter')
    ORDER BY a.Priority;

    -- Row images use UNION ALL to preserve duplicate line items. Changes rejected by the
    -- business transaction are rejected here too. An update writes separate before/after rows.
    DECLARE @Projection nvarchar(max) = @Company + N', ' + @Branch + N', ' + @Year
        + N', COALESCE(@UserId, CASE WHEN @Action = ''SAVE'' THEN ' + @InsertUser + N' END), ' + @Counter
        + N', CONVERT(nvarchar(100), r.' + QUOTENAME(@Document) + N'), ' + @Amount
        + N', COALESCE(CONVERT(nvarchar(2000), STUFF(CONVERT(nvarchar(max), N''   '')'
        + COALESCE(@Display, N'') + N', 1, 6, N'''')), N'''')';
    DECLARE @ModuleSql nvarchar(max) = N'N''' + REPLACE(@Module, '''', '''''') + N'''';
    IF @Table = N'Vouchers'
        SET @ModuleSql = N'CASE UPPER(LTRIM(RTRIM(r.VoucherType)))
            WHEN ''GENPAY'' THEN N''General Payment'' WHEN ''GERPAY'' THEN N''General Payment''
            WHEN ''GENREC'' THEN N''General Receipt'' WHEN ''GENRECEIPT'' THEN N''General Receipt''
            WHEN ''GENRECIEPT'' THEN N''General Receipt'' WHEN ''VENDPAY'' THEN N''Vendor Payment Posting''
            WHEN ''CUSTRCPT'' THEN N''Customer Receipt Posting''
            ELSE COALESCE(NULLIF(CONVERT(nvarchar(65),r.VoucherType),N''''), N''Voucher'') + N'' Posting'' END';

    DECLARE @Sql nvarchar(max) =
        CASE WHEN OBJECT_ID(N'dbo.' + QUOTENAME(@Trigger), N'TR') IS NULL THEN N'CREATE' ELSE N'ALTER' END
        + N' TRIGGER dbo.' + QUOTENAME(@Trigger) + N' ON dbo.' + QUOTENAME(@Table) + N'
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    IF ROWCOUNT_BIG() = 0 RETURN;
    SET NOCOUNT ON;
    DECLARE @Action varchar(10) = CASE WHEN EXISTS (SELECT 1 FROM inserted)
        THEN CASE WHEN EXISTS (SELECT 1 FROM deleted) THEN ''UPDATE'' ELSE ''SAVE'' END ELSE ''DELETE'' END;
    DECLARE @CompanyId int, @BranchId int, @FinYearId int, @UserId int, @CounterId int;
    DECLARE @Context varbinary(128) = CONTEXT_INFO(), @Stamp datetime2(3) = SYSDATETIME();
    IF SUBSTRING(@Context, 1, 4) = 0x504F5341
        SELECT @CompanyId = NULLIF(CONVERT(int, SUBSTRING(@Context, 5, 4)),0),
               @BranchId = NULLIF(CONVERT(int, SUBSTRING(@Context, 9, 4)),0),
               @FinYearId = NULLIF(CONVERT(int, SUBSTRING(@Context, 13, 4)),0),
               @UserId = NULLIF(CONVERT(int, SUBSTRING(@Context, 17, 4)),0),
               @CounterId = NULLIF(CONVERT(int, SUBSTRING(@Context, 21, 4)),0);
    DECLARE @DirectVoucherEditId uniqueidentifier;
    IF N''' + @Table + N''' = N''Vouchers'' AND @UserId IS NOT NULL AND SUBSTRING(@Context,25,1)=0x01
        SET @DirectVoucherEditId = CONVERT(uniqueidentifier, SUBSTRING(@Context,26,16));
    IF @DirectVoucherEditId IS NOT NULL SET @Action = ''UPDATE'';

    DECLARE @WriteTransactionId bigint, @WriteStatementId uniqueidentifier = NEWID();
    IF HAS_PERMS_BY_NAME(NULL, NULL, ''VIEW SERVER STATE'') = 1
        SELECT @WriteTransactionId = transaction_id FROM sys.dm_tran_current_transaction;

    INSERT dbo.POS_ActivityChange
        (EventId, WriteTransactionId, WriteStatementId, DirectVoucherEditId, ActivityDate, Category, Module, SourceTable, ActionType, RowVersion,
         CompanyId, BranchId, FinYearId, UserId, CounterId, DocumentNo, Amount, Details)
    SELECT NEWID(), @WriteTransactionId, @WriteStatementId, @DirectVoucherEditId, @Stamp, N''' + @Category + N''', ' + @ModuleSql + N', N''' + @Table + N''', @Action, ''Before'',
           ' + @Projection + N'
    FROM deleted r
    UNION ALL
    SELECT NEWID(), @WriteTransactionId, @WriteStatementId, @DirectVoucherEditId, @Stamp, N''' + @Category + N''', ' + @ModuleSql + N', N''' + @Table + N''', @Action, ''After'',
           ' + @Projection + N'
    FROM inserted r;
END;';
    EXEC sys.sp_executesql @Sql;
    FETCH NEXT FROM sources INTO @Table, @Document, @Module, @Category;
END;
CLOSE sources;
DEALLOCATE sources;
GO
