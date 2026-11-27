-- Install after POS_ActivityReport.sql. Current records are labelled separately from historical snapshots.
IF OBJECT_ID(N'dbo.POS_ActivityReportDetails', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo.POS_ActivityReportDetails AS RETURN 0;');
GO
ALTER PROCEDURE dbo.POS_ActivityReportDetails
    @CompanyId int,
    @BranchId int = NULL,
    @FinYearId int = NULL,
    @CounterId int = NULL,
    @Module nvarchar(100),
    @DocumentNo nvarchar(100),
    @Source nvarchar(128),
    @ActionType nvarchar(100),
    @ActivityDate datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Access int;
    EXEC @Access = dbo.POS_RequireActivityAdministrator @CompanyId;
    IF @Access <> 0 RETURN;

    CREATE TABLE #Details
    (
        SortOrder int NOT NULL, Line bigint NULL, Section nvarchar(40) NOT NULL,
        Record nvarchar(200) NULL, Details nvarchar(max) NOT NULL
    );

    IF @ActionType IN (N'DELETE', N'DELETED', N'CANCEL', N'CANCELLED')
    BEGIN
        IF @Source = N'Ledger deletion archive'
        BEGIN
            INSERT #Details
            SELECT 10, NULL, N'Archived ledger', a.LedgerName,
                CONCAT(CONVERT(nvarchar(max),N'Ledger ID: '), a.LedgerID,
                    CHAR(13), CHAR(10), N'Alias: ', x.n.value('(Alias/text())[1]', 'nvarchar(200)'),
                    CHAR(13), CHAR(10), N'Description: ', x.n.value('(Description/text())[1]', 'nvarchar(1000)'),
                    CHAR(13), CHAR(10), N'Group: ', x.n.value('(GroupID/text())[1]', 'nvarchar(20)'),
                    CHAR(13), CHAR(10), N'Opening debit: ', x.n.value('(OpnDebit/text())[1]', 'nvarchar(50)'),
                    CHAR(13), CHAR(10), N'Opening credit: ', x.n.value('(OpnCredit/text())[1]', 'nvarchar(50)'),
                    CHAR(13), CHAR(10), N'Price level: ', x.n.value('(PriceLevel/text())[1]', 'nvarchar(100)'),
                    CHAR(13), CHAR(10), N'Bill-wise details: ', x.n.value('(MaintainBillWiseDetails/text())[1]', 'nvarchar(20)'))
            FROM dbo.LedgerDeletionArchive a
            CROSS APPLY a.LedgerSnapshot.nodes('/Ledger') x(n)
            WHERE a.CompanyId=@CompanyId AND a.BranchId=@BranchId AND a.LedgerID=TRY_CONVERT(int,@DocumentNo)
              AND CONVERT(datetime2(3),DATEADD(minute,DATEDIFF(minute,GETUTCDATE(),GETDATE()),a.DeletedAtUtc))=@ActivityDate;
        END;

        IF @Source = N'Voucher deletion audit'
        BEGIN
            INSERT #Details
            SELECT 10, NULL, N'Archived voucher', a.PartyName,
                CONCAT(CONVERT(nvarchar(max),N'Voucher ID: '), a.VoucherId,
                    CHAR(13), CHAR(10), N'Voucher type: ', a.VoucherType,
                    CHAR(13), CHAR(10), N'Document date: ', CONVERT(nvarchar(24),a.DocDate,120),
                    CHAR(13), CHAR(10), N'Reference: ', a.RefNo,
                    CHAR(13), CHAR(10), N'Amount: ', a.Amount, CHAR(13), CHAR(10), N'Narration: ', a.Narration)
            FROM dbo.VoucherDeletionAudit a
            WHERE a.CompanyId=@CompanyId AND a.BranchId=@BranchId AND a.FinYearId=@FinYearId
              AND a.VoucherId=TRY_CONVERT(bigint,@DocumentNo) AND CONVERT(datetime2(3),a.DeletedDate)=@ActivityDate
              AND @Module = CASE UPPER(LTRIM(RTRIM(a.VoucherType)))
                  WHEN N'GENPAY' THEN N'General Payment' WHEN N'GERPAY' THEN N'General Payment'
                  WHEN N'GENREC' THEN N'General Receipt' WHEN N'GENRECEIPT' THEN N'General Receipt'
                  WHEN N'GENRECIEPT' THEN N'General Receipt' WHEN N'VENDPAY' THEN N'Vendor Payment'
                  WHEN N'CUSTRCPT' THEN N'Customer Receipt' ELSE a.VoucherType END;
        END;

        IF @Source = N'Vouchers'
        BEGIN
            INSERT #Details
            SELECT 10, NULL, N'Archived posting', a.Module, a.Details
            FROM dbo.POS_ActivityChange a
            WHERE a.CompanyId=@CompanyId AND a.BranchId=@BranchId AND a.FinYearId=@FinYearId
              AND a.ActivityDate=@ActivityDate AND a.DocumentNo=@DocumentNo AND a.Module=@Module
              AND a.SourceTable=N'Vouchers' AND a.ActionType=N'DELETE' AND a.RowVersion=N'Before'
              AND (a.CounterId=@CounterId OR (a.CounterId IS NULL AND @CounterId IS NULL));
        END;

        SELECT Section, Line, Record, Details FROM #Details ORDER BY SortOrder, Line, Record;
        RETURN;
    END;

    IF NULLIF(@DocumentNo, N'') IS NULL
    BEGIN
        SELECT Section, Line, Record, Details FROM #Details;
        RETURN;
    END;

    DECLARE @Documents TABLE
    (
        Module nvarchar(80) PRIMARY KEY, HeaderTable sysname, HeaderKey sysname,
        DetailTable sysname NULL, DetailKey sysname NULL, HeaderLink sysname NULL
    );
    INSERT @Documents VALUES
    (N'Sales Invoice', N'SMaster', N'BillNo', N'SDetails', N'BillNo', N'BillNo'),
    (N'Held Bill', N'SMasterHold', N'BillNo', N'SDetailsHold', N'BillNo', N'BillNo'),
    (N'Purchase Invoice', N'PMaster', N'PurchaseNo', N'PDetails', N'PurchaseNo', N'PurchaseNo'),
    (N'Sales Return', N'SReturnMaster', N'SReturnNo', N'SReturnDetails', N'SReturnNo', N'SReturnNo'),
    (N'Purchase Return', N'PReturnMaster', N'PReturnNo', N'PReturnDetails', N'PReturnNo', N'PReturnNo'),
    (N'Purchase Order', N'POrderMaster', N'PONo', N'POrderDetails', N'PONo', N'PONo'),
    (N'Sales Order', N'SOrderMaster', N'OrderNo', N'SOrderDetails', N'OrderNo', N'OrderNo'),
    (N'Quotation', N'QuotationMaster', N'QuotationNo', N'QuotationDetails', N'QuotationNo', N'QuotationNo'),
    (N'Delivery Order', N'DeliveryOrderMaster', N'DeliveryNo', N'DeliveryOrderDetails', N'DeliveryNo', N'DeliveryNo'),
    (N'Stock Adjustment', N'StockAdjustmentMaster', N'StockAdjustmentNo', N'StockAdjustmentDetails', N'StockAdjustmentMasterId', N'Id'),
    (N'Stock Transfer', N'StockTransferMaster', N'StkTrNo', N'StockTransferDetail', N'StkTranMasterId', N'Id'),
    (N'Office Supplies', N'OfficeSuppliesMaster', N'OfficeSupplyNo', N'OfficeSuppliesDetails', N'OfficeSuppliesMasterId', N'Id'),
    (N'Office Supplies Withdrawal', N'OfficeSuppliesWithdrawMaster', N'OfficeSupplyNo', N'OfficeSuppliesWithdrawDetails', N'MasterId', N'Id'),
    (N'Van Stock', N'VanSaleStockMaster', N'Id', N'VanSaleStockDetails', N'MasterId', N'Id'),
    (N'Mobile Order', N'MobileOrderMaster', N'MobileOrderNumber', N'MobileOrderDetails', N'MobileOrderMasterId', N'Id'),
    (N'Vendor Payment', N'VendorPaymentMaster', N'VoucherId', N'VendorPaymentDetails', N'PaymentMasterId', N'Id'),
    (N'Vendor Payment Detail', N'VendorPaymentMaster', N'Id', N'VendorPaymentDetails', N'PaymentMasterId', N'Id'),
    (N'Customer Receipt', N'CustomerReceiptMaster', N'VoucherId', N'CustomerReceiptDetails', N'ReceiptMasterId', N'Id'),
    (N'Customer Receipt Detail', N'CustomerReceiptMaster', N'Id', N'CustomerReceiptDetails', N'ReceiptMasterId', N'Id'),
    (N'Credit Note', N'CreditNoteMaster', N'VoucherId', N'CreditNoteDetails', N'CreditNoteMasterId', N'Id'),
    (N'Credit Note Detail', N'CreditNoteMaster', N'Id', N'CreditNoteDetails', N'CreditNoteMasterId', N'Id'),
    (N'Debit Note', N'DebitNoteMaster', N'VoucherId', N'DebitNoteDetails', N'DebitNoteMasterId', N'Id'),
    (N'Debit Note Detail', N'DebitNoteMaster', N'Id', N'DebitNoteDetails', N'DebitNoteMasterId', N'Id'),
    (N'Transporter Receipt', N'TransporterReceiptMaster', N'Id', N'TransporterReceiptDetails', N'ReceiptMasterId', N'Id'),
    (N'Transporter Receipt Detail', N'TransporterReceiptMaster', N'Id', N'TransporterReceiptDetails', N'ReceiptMasterId', N'Id'),
    (N'Manual Party Balance', N'ManualPartyBalance', N'Id', N'ManualPartyBalanceSettlement', N'ManualPartyBalanceId', N'Id'),
    (N'Party Balance Settlement', N'ManualPartyBalance', N'Id', N'ManualPartyBalanceSettlement', N'ManualPartyBalanceId', N'Id'),
    (N'Bank Reconciliation', N'BankReconciliation', N'VoucherID', NULL, NULL, NULL),
    (N'Sales Payment', N'SMaster', N'BillNo', N'SDetails', N'BillNo', N'BillNo'),
    (N'Vendor Payment Posting', N'VendorPaymentMaster', N'VoucherId', N'VendorPaymentDetails', N'PaymentMasterId', N'Id'),
    (N'Customer Receipt Posting', N'CustomerReceiptMaster', N'VoucherId', N'CustomerReceiptDetails', N'ReceiptMasterId', N'Id');

    DECLARE @Masters TABLE (TableName sysname PRIMARY KEY, KeyColumn sysname);
    INSERT @Masters VALUES
    (N'ItemMaster', N'ItemId'),
    (N'LedgerMaster', N'LedgerId'),
    (N'VendorMaster', N'Id'),
    (N'AccountGroupMaster', N'GroupID'),
    (N'AccountHead', N'AcHeadId'),
    (N'AllocationMethod', N'Id'),
    (N'PriceSettings', N'ItemId'),
    (N'POS_ItemMasterStatusRules', N'ItemId'),
    (N'Category', N'Id'),
    (N'SubCategory', N'Id'),
    (N'Group', N'Id'),
    (N'Brands', N'Id'),
    (N'ItemTypes', N'Id'),
    (N'ItemLine', N'Id'),
    (N'ItemRack', N'Id'),
    (N'ItemRow', N'Id'),
    (N'UnitMaster', N'UnitID'),
    (N'Units', N'Id'),
    (N'UnitQuantityCode', N'ID'),
    (N'TaxSystem', N'Id'),
    (N'TaxType', N'Id'),
    (N'Currency', N'CurrencyID'),
    (N'Country', N'CountryID'),
    (N'State', N'StateID'),
    (N'Company', N'CompanyID'),
    (N'CompanyInfo', N'CompanyID'),
    (N'Branch', N'Id'),
    (N'Branches', N'Id'),
    (N'FinancialYear', N'FinYearID'),
    (N'BARCODE', N'BarcodeID'),
    (N'PayMode', N'PayModeID'),
    (N'PartyType', N'Id'),
    (N'PriceLevels', N'Id'),
    (N'PromoCode', N'Id'),
    (N'OfferType', N'OfferId'),
    (N'ItemOfferDiscount', N'Id'),
    (N'QuickPurchasePreset', N'PresetId'),
    (N'StockAdjustmentReasonMaster', N'Id'),
    (N'VoucherTypeMaster', N'VoucherID'),
    (N'POS_Settings', N'SettingId'),
    (N'PrintSettings', N'Id'),
    (N'BillFormats', N'Id'),
    (N'PurchasePrintBranding', N'BranchId'),
    (N'Utilities', N'Id'),
    (N'WeighMachineSettings', N'Id'),
    (N'KeyBoardShortCut', N'ID'),
    (N'KeyBoardShortCutMenu', N'ID'),
    (N'KeyBoardShortCutSubMenu', N'ID'),
    (N'TransactionType', N'Id');

    DECLARE @Header sysname, @Key sysname, @Detail sysname, @Link sysname, @ParentLink sysname;
    SELECT @Header = HeaderTable, @Key = HeaderKey, @Detail = DetailTable, @Link = DetailKey, @ParentLink = HeaderLink
    FROM @Documents WHERE Module = @Module;

    IF @Source = N'Item activity log'
    BEGIN
        DECLARE @ItemId int;
        SELECT @ItemId = MIN(ItemId)
        FROM dbo.ItemActivityLog
        WHERE CompanyId = @CompanyId AND BranchId = @BranchId AND FinYearId = @FinYearId
          AND ItemNo = @DocumentNo AND CreatedOn = CONVERT(datetime, @ActivityDate)
          AND UPPER(LTRIM(RTRIM(ActivityType))) = @ActionType
        HAVING MIN(ItemId) = MAX(ItemId);
        SELECT @DocumentNo = CONVERT(nvarchar(100), @ItemId), @Header = N'ItemMaster', @Key = N'ItemId';
    END;

    IF @Source=N'POS_ItemMasterStatusRules'
        SELECT @Header=N'ItemMaster', @Key=N'ItemId';

    IF @Header IS NULL
        SELECT @Header = TableName, @Key = KeyColumn FROM @Masters WHERE TableName = @Source;

    IF @Source = N'MobileOrderDetails'
        SELECT @Header = N'MobileOrderMaster', @Key = N'Id';
    IF @Source = N'Activity log' AND @Module = N'Sales Invoice' AND @ActionType = N'HOLD'
        SELECT @Header = N'SMasterHold', @Key = N'BillNo',
               @Detail = N'SDetailsHold', @Link = N'BillNo', @ParentLink = N'BillNo';

    DECLARE @VoucherKind nvarchar(80) = CASE @Module
        WHEN N'General Payment' THEN N'GENPAY' WHEN N'General Receipt' THEN N'GENREC'
        WHEN N'Vendor Payment' THEN N'VENDPAY' WHEN N'Vendor Payment Posting' THEN N'VENDPAY'
        WHEN N'Customer Receipt' THEN N'CUSTRCPT' WHEN N'Customer Receipt Posting' THEN N'CUSTRCPT'
        WHEN N'Credit Note' THEN N'CREDITNOTE' WHEN N'Debit Note' THEN N'DEBITNOTE'
        WHEN N'Sales Invoice' THEN N'SALES' WHEN N'Sales Payment' THEN N'SALES'
        WHEN N'Purchase Invoice' THEN N'PURCHASE' WHEN N'Stock Adjustment' THEN N'PHYSICALSTOCK'
        ELSE UPPER(REPLACE(LTRIM(RTRIM(@Module)), N' ', N'')) END;
    DECLARE @VoucherTypeSql nvarchar(max) = N'CASE UPPER(REPLACE(LTRIM(RTRIM(v.VoucherType)), N'' '', N''''))
        WHEN N''GERPAY'' THEN N''GENPAY'' WHEN N''GENERALPAYMENT'' THEN N''GENPAY''
        WHEN N''GENRECEIPT'' THEN N''GENREC'' WHEN N''GENRECIEPT'' THEN N''GENREC''
        WHEN N''GENERALRECEIPT'' THEN N''GENREC'' WHEN N''VENDORPAYMENT'' THEN N''VENDPAY''
        WHEN N''CUSTOMERRECEIPT'' THEN N''CUSTRCPT'' WHEN N''CUSTREC'' THEN N''CUSTRCPT''
        ELSE UPPER(REPLACE(LTRIM(RTRIM(v.VoucherType)), N'' '', N'''')) END = @VoucherKind';
    DECLARE @ReadVouchers bit = CASE WHEN @Source IN (N'Vouchers', N'Voucher posting record') THEN 1 ELSE 0 END;
    DECLARE @VoucherDate date;
    DECLARE @Sql nvarchar(max);

    IF @ReadVouchers = 1 AND @BranchId IS NOT NULL AND @FinYearId IS NOT NULL
    BEGIN
        SET @Sql = N'SELECT @VoucherDate = CONVERT(date, MAX(v.VoucherDate)) FROM dbo.Vouchers v
            WHERE v.CompanyId=@CompanyId AND v.BranchId=@BranchId AND v.FinYearId=@FinYearId
              AND v.VoucherId=TRY_CONVERT(int,@DocumentNo) AND ' + @VoucherTypeSql;
        EXEC sys.sp_executesql @Sql,
            N'@CompanyId int, @BranchId int, @FinYearId int, @DocumentNo nvarchar(100), @VoucherKind nvarchar(80), @VoucherDate date OUTPUT',
            @CompanyId, @BranchId, @FinYearId, @DocumentNo, @VoucherKind, @VoucherDate OUTPUT;
    END;

    DECLARE @Plan TABLE
    (
        Position int NOT NULL, Section nvarchar(40) NOT NULL, TableName sysname NOT NULL,
        JoinColumn sysname NULL, ParentColumn sysname NULL
    );
    IF @Header IS NOT NULL
        INSERT @Plan VALUES (10, N'Current record', @Header, NULL, NULL);
    IF @Detail IS NOT NULL
        INSERT @Plan VALUES (20, N'Current lines', @Detail, @Link, @ParentLink);
    IF @Header = N'SMaster'
        INSERT @Plan VALUES (30, N'Current payments', N'SPaymentDetails', N'BillNo', N'BillNo');
    IF @Header = N'ItemMaster'
        INSERT @Plan VALUES (30, N'Current units and prices', N'PriceSettings', N'ItemId', N'ItemId'),
                            (40, N'Current barcodes', N'ItemAlternativeBarcode', N'ItemId', N'ItemId'),
                            (50, N'Current item batches', N'ItemBatch', N'ItemId', N'ItemId'),
                            (60, N'Current item dimensions', N'ItemDimension', N'ItemId', N'ItemId'),
                            (65, N'Current item status', N'POS_ItemMasterStatusRules', N'ItemId', N'ItemId');
    IF @Header = N'LedgerMaster'
        INSERT @Plan VALUES (30, N'Current ledger terms', N'LedgerDetails', N'LedgerID', N'LedgerID');
    IF @Header = N'QuickPurchasePreset'
        INSERT @Plan VALUES (30, N'Current preset items', N'QuickPurchasePresetItem', N'PresetId', N'PresetId');
    IF @ReadVouchers = 1 AND @VoucherDate IS NOT NULL
        INSERT @Plan VALUES (40, N'Current postings', N'Vouchers', NULL, NULL);
    ELSE IF @Header IN (N'SMaster', N'PMaster', N'StockAdjustmentMaster')
        INSERT @Plan VALUES (40, N'Current postings', N'Vouchers', N'VoucherId', N'VoucherId');

    -- Explicit allowlist keeps credentials, addresses, payment references, bank/card numbers,
    -- internal settings values and binary blobs out of the report.
    DECLARE @Fields TABLE (ColumnName sysname PRIMARY KEY);
    INSERT @Fields VALUES
        ('id'), ('slno'), ('itemid'), ('itemno'), ('itemname'), ('description'), ('barcode'), ('aliasbarcode'),
        ('unitid'), ('unit'), ('unitname'), ('baseunit'), ('isbaseunit'), ('packing'), ('batchid'), ('batchno'),
        ('expiry'), ('expirydate'), ('qty'), ('quantity'), ('free'), ('oldqty'), ('returnqty'), ('returnedqty'),
        ('returned'), ('loadedqty'), ('unloadedqty'), ('systemstock'), ('physicalstock'), ('qtydifference'), ('stock'), ('available'),
        ('openingstock'), ('closingstock'), ('opnstk'), ('opnvalue'), ('opndate'), ('orderedstock'), ('reorder'), ('reorderlevel'),
        ('stockvalue'), ('cost'), ('originalcost'), ('openingcost'), ('singleitemcost'), ('costing'), ('unitcost'), ('rate'),
        ('unitprice'), ('sellingprice'), ('salesprice'), ('salesrate'), ('salesvalue'), ('originalsp'), ('sprice'), ('retailprice'),
        ('wholesaleprice'), ('creditprice'), ('cardprice'), ('staffprice'), ('minprice'), ('mrp'), ('totalsp'), ('marginper'),
        ('marginamt'), ('taxtype'), ('taxper'), ('taxamt'), ('cessamt'), ('cessper'), ('kfcessper'), ('kfcessamt'),
        ('disper'), ('disamt'), ('discountper'), ('discountamount'), ('discountamt'), ('billdiscountper'), ('billdiscountamt'), ('orderdiscountper'),
        ('orderdiscountamt'), ('spdisper'), ('spdsiamt'), ('baseamount'), ('gross'), ('subtotal'), ('netamount'), ('nettotal'),
        ('grandtotal'), ('totalamount'), ('totalprice'), ('amount'), ('amt'), ('billamount'), ('oldbillamount'), ('oldreceiptamount'),
        ('oldpaymentamount'), ('receivedamount'), ('receiptamount'), ('paymentamount'), ('payedamount'), ('payableamount'), ('receivableamount'), ('settlementamount'),
        ('balanceamount'), ('tenderedamount'), ('balance'), ('creditamount'), ('debitamount'), ('debit'), ('credit'), ('roundamount'),
        ('roundoff'), ('roundoffflag'), ('freight'), ('frieght'), ('expenseamt'), ('otherexpamt'), ('creditperiod'), ('duedate'),
        ('creditdays'), ('purchaseno'), ('purchasedate'), ('invoiceno'), ('invoicedate'), ('billno'), ('billdate'), ('pono'),
        ('podate'), ('orderno'), ('orderdate'), ('sreturnno'), ('sreturndate'), ('preturnno'), ('preturndate'), ('quotationno'),
        ('quotationdate'), ('deliveryno'), ('deliverydate'), ('stockadjustmentno'), ('stockadjustmentdate'), ('officesupplyno'), ('stktrno'), ('transferdate'),
        ('voucherid'), ('vouchernumber'), ('voucherdate'), ('vouchertype'), ('mode'), ('paymode'), ('paymodeid'), ('paymodename'),
        ('paymentmethodledgerid'), ('paymodeledgerid'), ('ledgerid'), ('ledgername'), ('customerledgerid'), ('customername'), ('vendorledgerid'), ('vendorname'),
        ('vendorid'), ('partyname'), ('partytype'), ('groupid'), ('groupname'), ('categoryid'), ('categoryname'), ('subcategoryid'),
        ('subcategoryname'), ('brandid'), ('brandname'), ('itemtype'), ('itemtypeid'), ('itemstatus'), ('active'), ('status'),
        ('isactive'), ('isdelete'), ('isdeleted'), ('hide'), ('cancelflag'), ('ispaid'), ('paid'), ('isbilled'),
        ('isexpiry'), ('is_perishable'), ('baseunitid'), ('lineid'), ('line'), ('rackid'), ('rack'), ('rowid'),
        ('row'), ('hsncode'), ('order_cycle_days'), ('box_quantity'), ('opndebit'), ('opncredit'), ('creditlimit'), ('pricelevel'),
        ('alias'), ('currencyname'), ('currencysymbol'), ('statename'), ('branchname'), ('companyname'), ('countryname'), ('transfertype'),
        ('officesupplystatus'), ('orderstatus'), ('requestedondate'), ('suppliedondate'), ('sourceid'), ('targetid'), ('remarks'),
        ('narration'), ('comments'), ('reason'), ('entrydate'), ('balancetype'), ('approvedstatus'), ('name'), ('date'),
        ('methodforappropriationinpurchaseinvoice'), ('blocksale'), ('blockpurchase'), ('statusname'), ('statusreason'), ('statusdate'), ('offername'), ('discountpercentage'),
        ('counterid'), ('seriesid'), ('registrationtype');

    DECLARE @HeaderId int = OBJECT_ID(N'dbo.' + QUOTENAME(@Header), N'U');
    DECLARE @HeaderFilter nvarchar(max), @HeaderBranch sysname, @HeaderYear sysname,
            @HeaderCompany sysname, @HeaderCounter sysname;
    IF @HeaderId IS NOT NULL AND COL_LENGTH(N'dbo.' + QUOTENAME(@Header), @Key) IS NOT NULL
    BEGIN
        SELECT @HeaderCompany = NULLIF(MAX(CASE WHEN LOWER(name) = N'companyid' THEN name ELSE N'' END), N''),
               @HeaderBranch = NULLIF(MAX(CASE WHEN LOWER(name) IN (N'branchid', N'sourceid', N'requestedbybranchid') THEN name ELSE N'' END), N''),
               @HeaderYear = NULLIF(MAX(CASE WHEN LOWER(name) = N'finyearid' THEN name ELSE N'' END), N''),
               @HeaderCounter = NULLIF(MAX(CASE WHEN LOWER(name) = N'counterid' THEN name ELSE N'' END), N'')
        FROM sys.columns WHERE object_id = @HeaderId;

        SET @HeaderFilter = N'CONVERT(nvarchar(100), r.' + QUOTENAME(@Key) + N') = @DocumentNo';
        IF @Source = N'Document record' AND @Key = N'VoucherId'
           AND COL_LENGTH(N'dbo.' + QUOTENAME(@Header), N'Id') IS NOT NULL
            SET @HeaderFilter = N'CONVERT(nvarchar(100), COALESCE(NULLIF(r.' + QUOTENAME(@Key) + N',0), r.Id)) = @DocumentNo';
        IF @HeaderCompany IS NOT NULL
            SET @HeaderFilter += N' AND r.' + QUOTENAME(@HeaderCompany) + N' = @CompanyId';
        IF @HeaderBranch IS NOT NULL
        BEGIN
            SET @HeaderFilter += N' AND r.' + QUOTENAME(@HeaderBranch) + N' = @BranchId';
            IF @HeaderCompany IS NULL
                SET @HeaderFilter += N' AND EXISTS (SELECT 1 FROM dbo.Branches b WHERE b.Id=@BranchId AND b.CompanyId=@CompanyId)';
        END;
        IF @HeaderYear IS NOT NULL
            SET @HeaderFilter += N' AND (@FinYearId IS NULL OR r.' + QUOTENAME(@HeaderYear) + N' = @FinYearId)';
        IF @HeaderCounter IS NOT NULL AND @Module IN (N'Sales Order', N'Quotation', N'Delivery Order')
            SET @HeaderFilter += N' AND (@CounterId IS NULL OR r.' + QUOTENAME(@HeaderCounter) + N' = @CounterId)';
        IF @ReadVouchers = 1 AND @Key = N'VoucherId'
            SET @HeaderFilter += N' AND CONVERT(date, r.VoucherDate) = @VoucherDate';

        -- A reused document number must never return lines from a different record.
        IF @Header NOT IN (N'PriceSettings', N'BankReconciliation')
        BEGIN
            DECLARE @Matches int;
            SET @Sql = N'SELECT @Matches=COUNT(*) FROM dbo.' + QUOTENAME(@Header) + N' r WHERE ' + @HeaderFilter;
            EXEC sys.sp_executesql @Sql,
                N'@CompanyId int,@BranchId int,@FinYearId int,@CounterId int,@DocumentNo nvarchar(100),@VoucherDate date,@Matches int OUTPUT',
                @CompanyId,@BranchId,@FinYearId,@CounterId,@DocumentNo,@VoucherDate,@Matches OUTPUT;
            IF @Matches <> 1 SET @HeaderFilter = NULL;
        END;
    END;

    DECLARE @Position int, @Section nvarchar(40), @Table sysname, @Join sysname, @Parent sysname;
    DECLARE sources CURSOR LOCAL FAST_FORWARD FOR
        SELECT Position, Section, TableName, JoinColumn, ParentColumn FROM @Plan ORDER BY Position;
    OPEN sources;
    FETCH NEXT FROM sources INTO @Position, @Section, @Table, @Join, @Parent;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        DECLARE @ObjectId int = OBJECT_ID(N'dbo.' + QUOTENAME(@Table), N'U');
        IF @ObjectId IS NOT NULL AND (@HeaderFilter IS NOT NULL OR (@Section = N'Current postings' AND @Join IS NULL))
        BEGIN
            DECLARE @Payload nvarchar(max), @RowNumber nvarchar(200), @Record nvarchar(max) = NULL, @From nvarchar(max);
            DECLARE @HasItem bit = 0;
            SELECT @Payload = (
                SELECT N' + COALESCE(NCHAR(13)+NCHAR(10)+N''' + REPLACE(c.name, N'''', N'''''') + N': '' + '
                    + N'NULLIF(CONVERT(nvarchar(max), d.' + QUOTENAME(c.name) + N',120),N''''),N'''')'
                FROM sys.columns c
                WHERE c.object_id = @ObjectId AND EXISTS (SELECT 1 FROM @Fields f WHERE f.ColumnName = LOWER(c.name))
                  AND c.system_type_id NOT IN (34, 165, 173, 240, 241)
                ORDER BY c.column_id
                FOR XML PATH(''),TYPE).value('.', 'nvarchar(max)');

            SELECT @RowNumber = COALESCE(NULLIF(MAX(CASE WHEN LOWER(name)=N'slno'
                    THEN N'TRY_CONVERT(bigint,d.' + QUOTENAME(name) + N')' ELSE N'' END), N''), N'CONVERT(bigint,NULL)'),
                   @HasItem = CONVERT(bit, MAX(CASE WHEN LOWER(name)=N'itemid' THEN 1 ELSE 0 END))
            FROM sys.columns WHERE object_id=@ObjectId;

            SELECT TOP (1) @Record = N'NULLIF(CONVERT(nvarchar(200),d.' + QUOTENAME(name) + N'), N'''')'
            FROM sys.columns
            WHERE object_id=@ObjectId AND LOWER(name) IN (N'itemname',N'ledgername',N'customername',N'vendorname',N'partyname',N'name',N'description',N'billno')
            ORDER BY CASE LOWER(name) WHEN N'itemname' THEN 0 WHEN N'ledgername' THEN 1
                     WHEN N'customername' THEN 2 WHEN N'vendorname' THEN 3 WHEN N'partyname' THEN 4
                     WHEN N'name' THEN 5 WHEN N'description' THEN 6 ELSE 7 END;
            IF @HasItem = 1 AND @Join IS NOT NULL
                SET @Record = N'COALESCE(' + COALESCE(@Record, N'CONVERT(nvarchar(200),NULL)') + N',
                    (SELECT TOP (1) i.Description FROM dbo.ItemMaster i
                     WHERE i.CompanyId=@CompanyId AND i.ItemId=d.ItemId ORDER BY i.ItemId))';

            IF @Section = N'Current postings' AND @Join IS NULL
            BEGIN
                SET @From = N' FROM dbo.Vouchers d
                    WHERE d.CompanyId=@CompanyId AND d.BranchId=@BranchId AND d.FinYearId=@FinYearId
                      AND d.VoucherId=TRY_CONVERT(int,@DocumentNo) AND ' + REPLACE(@VoucherTypeSql, N'v.VoucherType', N'd.VoucherType');
            END
            ELSE IF @Join IS NULL
                SET @From = N' FROM dbo.' + QUOTENAME(@Header) + N' d WHERE ' + REPLACE(@HeaderFilter, N'r.', N'd.');
            ELSE
            BEGIN
                SET @From = N' FROM dbo.' + QUOTENAME(@Table) + N' d
                    JOIN dbo.' + QUOTENAME(@Header) + N' r ON d.' + QUOTENAME(@Join) + N'=r.' + QUOTENAME(@Parent);
                -- Match scope fields carried by both sides of an item or document relationship.
                SELECT @From += N' AND d.' + QUOTENAME(c.name) + N'=r.' + QUOTENAME(h.name)
                FROM sys.columns c
                JOIN sys.columns h ON h.object_id=@HeaderId AND LOWER(c.name)=LOWER(h.name)
                WHERE c.object_id=@ObjectId AND LOWER(c.name) IN (N'companyid',N'branchid',N'finyearid',N'counterid',N'sourceid');
                SET @From += N' WHERE ' + @HeaderFilter;
                IF @Section = N'Current postings'
                    SET @From += N' AND ' + REPLACE(@VoucherTypeSql, N'v.VoucherType', N'd.VoucherType');
            END;

            SET @Sql = N'INSERT #Details
                SELECT @Position, ' + @RowNumber + N', @Section, COALESCE('
                + COALESCE(@Record, N'CONVERT(nvarchar(200), NULL)') + N', @Module, @DocumentNo),
                COALESCE(STUFF(CONVERT(nvarchar(max),N'''')' + COALESCE(@Payload,N'')
                + N',1,2,N''''), N'''')' + @From;
            EXEC sys.sp_executesql @Sql,
                N'@CompanyId int,@BranchId int,@FinYearId int,@CounterId int,@DocumentNo nvarchar(100),
                  @VoucherKind nvarchar(80),@VoucherDate date,@Position int,@Section nvarchar(40),@Module nvarchar(100)',
                @CompanyId,@BranchId,@FinYearId,@CounterId,@DocumentNo,@VoucherKind,@VoucherDate,@Position,@Section,@Module;
        END;
        FETCH NEXT FROM sources INTO @Position, @Section, @Table, @Join, @Parent;
    END;
    CLOSE sources;
    DEALLOCATE sources;

    SELECT Section, Line, Record, Details FROM #Details ORDER BY SortOrder, Line, Record;
END;
GO
