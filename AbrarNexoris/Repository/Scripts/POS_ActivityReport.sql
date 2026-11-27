-- Report data stays company-scoped. Install POS_ActivityCapture.sql first.
IF OBJECT_ID(N'dbo.POS_RequireActivityAdministrator', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo.POS_RequireActivityAdministrator AS RETURN 0;');
GO
ALTER PROCEDURE dbo.POS_RequireActivityAdministrator @CompanyId int
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Context varbinary(128) = CONTEXT_INFO();
    IF SUBSTRING(@Context, 1, 4) = 0x504F5341
       AND CONVERT(int, SUBSTRING(@Context, 5, 4)) = @CompanyId
       AND EXISTS
       (
           SELECT 1 FROM dbo.Users u
           WHERE u.UserId = CONVERT(int, SUBSTRING(@Context, 17, 4))
             AND u.CompanyId = @CompanyId AND ISNULL(u.IsDelete, 0) = 0
             AND (EXISTS (SELECT 1 FROM dbo.Roles r WHERE r.UserLevelId = u.UserLevelId
                            AND r.IsActive = 1 AND r.RoleName IN (N'Admin', N'Administrator'))
                  OR EXISTS (SELECT 1 FROM dbo.UserLevel l WHERE l.UserLevelId = u.UserLevelId
                               AND l.UserLevel IN (N'Admin', N'Administrator')))
       )
        RETURN 0;
    RAISERROR('The Activity Report requires an administrator for the current company.', 16, 1);
    RETURN 1;
END;
GO

IF OBJECT_ID(N'dbo.POS_ActivityReport', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo.POS_ActivityReport AS RETURN 0;');
GO
ALTER PROCEDURE dbo.POS_ActivityReport
    @CompanyId int,
    @FromDate date,
    @ToDate date,
    @BranchId int = NULL,
    @UserName nvarchar(128) = NULL,
    @CounterName nvarchar(128) = NULL,
    @Category nvarchar(24) = NULL,
    @Search nvarchar(200) = NULL,
    @ActionType nvarchar(100) = NULL,
    @AsOfNow bit = 0,
    @IncludeDetailKeys bit = 0
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Access int;
    EXEC @Access = dbo.POS_RequireActivityAdministrator @CompanyId;
    IF @Access <> 0 RETURN;
    IF ISNULL(@CompanyId, 0) <= 0 OR @FromDate IS NULL OR @ToDate IS NULL OR @ToDate < @FromDate
    BEGIN
        RAISERROR('Select a valid company and date range.',16,1);
        RETURN;
    END;

    DECLARE @ToExclusive datetime2(3) = CASE WHEN @ToDate < '99991231'
        THEN DATEADD(day, 1, CONVERT(datetime2(3), @ToDate)) ELSE CONVERT(datetime2(3), '9999-12-31T23:59:59.999') END;
    DECLARE @Now datetime2(3) = SYSDATETIME();
    IF @AsOfNow = 1 AND @Now < @ToExclusive
        SET @ToExclusive = @Now;

    DECLARE @LegacyTo datetime2(3) = @ToExclusive;
    SELECT @LegacyTo = CASE WHEN InstalledOn < @ToExclusive THEN InstalledOn ELSE @ToExclusive END
        FROM dbo.POS_ActivityCaptureInstallation;
    DECLARE @Filter nvarchar(410) = N'%' + REPLACE(REPLACE(REPLACE(REPLACE(@Search, N'~', N'~~'),
        N'%', N'~%'), N'_', N'~_'), N'[', N'~[') + N'%';

    ;WITH Events (ActivityDate, CompanyId, BranchId, FinYearId, UserId, UserName, CounterId, CounterName,
        Category, Module, ActionType, DocumentNo, RowVersion, Amount, Source, Details) AS
    (
    SELECT a.ActivityDate, a.CompanyId, a.BranchId, a.FinYearId, a.UserId, NULL, a.CounterId, NULL,
        a.Category, a.Module, a.ActionType, a.DocumentNo, a.RowVersion, a.Amount, a.SourceTable, a.Details
    FROM dbo.POS_ActivityChange a
    WHERE a.ActivityDate >= @FromDate AND a.ActivityDate < @ToExclusive
      AND (a.CompanyId = @CompanyId OR (a.CompanyId IS NULL AND
            (a.BranchId IS NULL OR EXISTS (SELECT 1 FROM dbo.Branches b WHERE b.Id = a.BranchId AND b.CompanyId = @CompanyId))))
    UNION ALL
    SELECT CreatedOn, CompanyId, BranchId, FinYearId, UserId, UserName, CounterId, CounterName,
        N'Transaction', N'Sales Invoice', ActivityType, CONVERT(nvarchar(100), TransactionNo), NULL,
        NetAmount, N'Activity log', CONCAT(NULLIF(InvoiceNo, ''), CASE WHEN NULLIF(PartyName, '') IS NOT NULL THEN N' | ' + PartyName END,
            CASE WHEN NULLIF(PaymentMode, '') IS NOT NULL THEN N' | ' + PaymentMode END,
            CASE WHEN NULLIF(ActivityDetails, '') IS NOT NULL THEN N' | ' + CONVERT(nvarchar(max), ActivityDetails) END)
    FROM dbo.SalesActivityLog WHERE CompanyId = @CompanyId AND CreatedOn >= @FromDate AND CreatedOn < @ToExclusive
    UNION ALL
    SELECT CreatedOn, CompanyId, BranchId, FinYearId, UserId, UserName, CounterId, CounterName,
        N'Transaction', N'Purchase Invoice', ActivityType, CONVERT(nvarchar(100), TransactionNo), NULL,
        NetAmount, N'Activity log', CONCAT(NULLIF(InvoiceNo, ''), CASE WHEN NULLIF(PartyName, '') IS NOT NULL THEN N' | ' + PartyName END,
            CASE WHEN NULLIF(PaymentMode, '') IS NOT NULL THEN N' | ' + PaymentMode END,
            CASE WHEN NULLIF(ActivityDetails, '') IS NOT NULL THEN N' | ' + CONVERT(nvarchar(max), ActivityDetails) END)
    FROM dbo.PurchaseActivityLog WHERE CompanyId = @CompanyId AND CreatedOn >= @FromDate AND CreatedOn < @ToExclusive
    UNION ALL
    SELECT CreatedOn, CompanyId, BranchId, FinYearId, UserId, UserName, CounterId, CounterName,
        N'Transaction', N'Sales Return', ActivityType, CONVERT(nvarchar(100), TransactionNo), NULL,
        NetAmount, N'Activity log', CONCAT(NULLIF(InvoiceNo, ''), CASE WHEN NULLIF(PartyName, '') IS NOT NULL THEN N' | ' + PartyName END,
            CASE WHEN NULLIF(PaymentMode, '') IS NOT NULL THEN N' | ' + PaymentMode END,
            CASE WHEN NULLIF(ActivityDetails, '') IS NOT NULL THEN N' | ' + CONVERT(nvarchar(max), ActivityDetails) END)
    FROM dbo.SalesReturnActivityLog WHERE CompanyId = @CompanyId AND CreatedOn >= @FromDate AND CreatedOn < @ToExclusive
    UNION ALL
    SELECT CreatedOn, CompanyId, BranchId, FinYearId, UserId, UserName, CounterId, CounterName,
        N'Transaction', N'Purchase Return', ActivityType, CONVERT(nvarchar(100), TransactionNo), NULL,
        NetAmount, N'Activity log', CONCAT(NULLIF(InvoiceNo, ''), CASE WHEN NULLIF(PartyName, '') IS NOT NULL THEN N' | ' + PartyName END,
            CASE WHEN NULLIF(PaymentMode, '') IS NOT NULL THEN N' | ' + PaymentMode END,
            CASE WHEN NULLIF(ActivityDetails, '') IS NOT NULL THEN N' | ' + CONVERT(nvarchar(max), ActivityDetails) END)
    FROM dbo.PurchaseReturnActivityLog WHERE CompanyId = @CompanyId AND CreatedOn >= @FromDate AND CreatedOn < @ToExclusive
    UNION ALL
    SELECT CreatedOn, CompanyId, BranchId, FinYearId, UserId, UserName, CounterId, CounterName,
        N'Transaction', N'Stock Adjustment', ActivityType, CONVERT(nvarchar(100), TransactionNo), NULL,
        NetAmount, N'Activity log', CONCAT(NULLIF(InvoiceNo, ''), CASE WHEN NULLIF(PartyName, '') IS NOT NULL THEN N' | ' + PartyName END,
            CASE WHEN NULLIF(PaymentMode, '') IS NOT NULL THEN N' | ' + PaymentMode END,
            CASE WHEN NULLIF(ActivityDetails, '') IS NOT NULL THEN N' | ' + CONVERT(nvarchar(max), ActivityDetails) END)
    FROM dbo.StockAdjustmentActivityLog WHERE CompanyId = @CompanyId AND CreatedOn >= @FromDate AND CreatedOn < @ToExclusive
    UNION ALL
    SELECT CreatedOn, CompanyId, BranchId, FinYearId, UserId, UserName, CounterId, CounterName,
        N'Master', N'Item Master', ActivityType, CONVERT(nvarchar(100), ItemNo), NULL, NULL,
        N'Item activity log', CONCAT(ItemName, CASE WHEN NULLIF(Barcode, '') IS NOT NULL THEN N' | ' + Barcode END,
            CASE WHEN NULLIF(ActivityDetails, '') IS NOT NULL THEN N' | ' + CONVERT(nvarchar(max), ActivityDetails) END)
    FROM dbo.ItemActivityLog WHERE CompanyId = @CompanyId AND CreatedOn >= @FromDate AND CreatedOn < @ToExclusive
    UNION ALL
    SELECT CreatedOn, CompanyId, BranchId, FinYearId, UserId, UserName, CounterId, CounterName,
        N'User', COALESCE(NULLIF(FormName, ''), N'User Session'), ActivityType,
        CONVERT(nvarchar(100), SessionId), NULL, NULL, N'User activity log', LEFT(ActivityDetails, 4000)
    FROM dbo.UserActivityLog WHERE CompanyId = @CompanyId AND CreatedOn >= @FromDate AND CreatedOn < @ToExclusive
    UNION ALL
    SELECT ActionDate, u.CompanyID, u.BranchID, NULL, u.UserID, u.UserName, NULL, NULL,
        N'User', FormName, ActionType, DocumentNo, NULL, NULL, N'Audit trail', LEFT(Remarks, 4000)
    FROM dbo.tblAuditTrail t JOIN dbo.Users u ON u.UserID = t.UserID
    WHERE u.CompanyID = @CompanyId AND ActionDate >= @FromDate AND ActionDate < @ToExclusive
    UNION ALL
    SELECT DeletedDate, CompanyId, BranchId, FinYearId, NULL, DeletedBy, NULL, Counter,
        N'Payment', CASE UPPER(LTRIM(RTRIM(VoucherType)))
            WHEN 'GENPAY' THEN N'General Payment' WHEN 'GERPAY' THEN N'General Payment'
            WHEN 'GENREC' THEN N'General Receipt' WHEN 'GENRECEIPT' THEN N'General Receipt'
            WHEN 'GENRECIEPT' THEN N'General Receipt' WHEN 'VENDPAY' THEN N'Vendor Payment'
            WHEN 'CUSTRCPT' THEN N'Customer Receipt' ELSE VoucherType END,
        N'DELETE', CONVERT(nvarchar(100), VoucherId), NULL, Amount, N'Voucher deletion audit',
        CONCAT(RefNo, CASE WHEN NULLIF(PartyName, '') IS NOT NULL THEN N' | ' + PartyName END,
            CASE WHEN DocDate IS NOT NULL THEN N' | Document date: ' + CONVERT(nvarchar(24), DocDate, 120) END,
            CASE WHEN NULLIF(Narration, '') IS NOT NULL THEN N' | ' + CONVERT(nvarchar(max), Narration) END)
    FROM dbo.VoucherDeletionAudit WHERE CompanyId = @CompanyId AND DeletedDate >= @FromDate AND DeletedDate < @ToExclusive
    UNION ALL
    SELECT d.ActivityDate, CompanyID, BranchID, NULL, NULL, DeletedBy, NULL, NULL,
        N'Master', N'Ledger Master', N'DELETE', CONVERT(nvarchar(100), LedgerID), NULL, NULL,
        N'Ledger deletion archive', LedgerName
    FROM dbo.LedgerDeletionArchive a
    CROSS APPLY (VALUES (DATEADD(minute, DATEDIFF(minute, GETUTCDATE(), GETDATE()), a.DeletedAtUtc))) d(ActivityDate)
    WHERE CompanyID = @CompanyId AND d.ActivityDate >= @FromDate AND d.ActivityDate < @ToExclusive
    UNION ALL
    SELECT h.ActivityDate, s.CompanyId, s.BranchId, s.FinYearId, s.UserId, NULL,
        s.CounterId, s.CounterName, N'Counter', N'Counter Session', h.ActionType,
        CONVERT(nvarchar(100), s.SessionId), NULL, NULL, N'Counter session record', NULL
    FROM dbo.CounterSessions s
    CROSS APPLY (VALUES (s.LoginTime, N'OPEN'), (s.CloseTime, N'CLOSE')) h(ActivityDate, ActionType)
    WHERE s.CompanyId = @CompanyId AND h.ActivityDate >= @FromDate AND h.ActivityDate < @LegacyTo
    UNION ALL
    SELECT MAX(COALESCE(v.UserDate, v.VoucherDate)), v.CompanyId, v.BranchId, v.FinYearId,
        MAX(v.UserId), MAX(v.UserName), MAX(v.CounterId), NULL, N'Payment', m.Module,
        N'ON FILE', CONVERT(nvarchar(100), v.VoucherId), NULL,
        CASE WHEN SUM(COALESCE(v.Debit,0)) <> 0 THEN SUM(COALESCE(v.Debit,0)) ELSE SUM(COALESCE(v.Credit,0)) END,
        N'Voucher posting record', CONCAT(MAX(v.VoucherNumber), ' | ', MAX(v.Mode),
            CASE WHEN MAX(CONVERT(int,v.CancelFlag)) = 1 THEN ' | Currently cancelled' ELSE '' END)
    FROM dbo.Vouchers v
    CROSS APPLY (VALUES (CASE UPPER(LTRIM(RTRIM(v.VoucherType)))
        WHEN 'GENPAY' THEN N'General Payment' WHEN 'GERPAY' THEN N'General Payment'
        WHEN 'GENREC' THEN N'General Receipt' WHEN 'GENRECEIPT' THEN N'General Receipt'
        WHEN 'GENRECIEPT' THEN N'General Receipt' WHEN 'VENDPAY' THEN N'Vendor Payment'
        WHEN 'CUSTRCPT' THEN N'Customer Receipt' WHEN 'CUSTREC' THEN N'Customer Receipt'
        WHEN 'PURCHASE' THEN NULL WHEN 'SALES' THEN NULL WHEN 'PHYSICALSTOCK' THEN NULL
        ELSE NULLIF(v.VoucherType, '') END)) m(Module)
    WHERE v.CompanyId = @CompanyId AND m.Module IS NOT NULL
    GROUP BY v.CompanyId, v.BranchId, v.FinYearId, v.VoucherId, m.Module
    HAVING MAX(COALESCE(v.UserDate, v.VoucherDate)) >= @FromDate
       AND MAX(COALESCE(v.UserDate, v.VoucherDate)) < @LegacyTo
    UNION ALL
    SELECT BillDate, CompanyId, BranchId, FinYearId, UserId, NULL, CounterId, NULL,
        N'Transaction', N'Sales Invoice', N'ON FILE', CONVERT(nvarchar(100), BillNo), NULL,
        NetAmount, N'Document record', CONCAT(N'Bill date: ', CONVERT(nvarchar(24), BillDate, 120),
            CASE WHEN NULLIF(CustomerName, '') IS NOT NULL THEN N' | Customer: ' + CustomerName END,
            CASE WHEN NULLIF(PaymodeName, '') IS NOT NULL THEN N' | Payment: ' + PaymodeName END,
            CASE WHEN CancelFlag = 1 THEN N' | Currently cancelled' END)
    FROM dbo.SMaster m WHERE CompanyId = @CompanyId AND BillDate >= @FromDate AND BillDate < @LegacyTo
      AND NOT EXISTS (SELECT 1 FROM dbo.SalesActivityLog a
          WHERE a.CompanyId = m.CompanyId AND a.BranchId = m.BranchId AND a.FinYearId = m.FinYearId
            AND a.TransactionNo = m.BillNo AND a.ActivityType = 'SAVE')
    UNION ALL
    SELECT PurchaseDate, CompanyId, BranchId, FinYearId, UserId, UserName, NULL, NULL,
        N'Transaction', N'Purchase Invoice', N'ON FILE', CONVERT(nvarchar(100), PurchaseNo), NULL,
        GrandTotal, N'Document record', CONCAT(N'Purchase date: ', CONVERT(nvarchar(24), PurchaseDate, 120),
            CASE WHEN NULLIF(InvoiceNo, '') IS NOT NULL THEN N' | Invoice: ' + CONVERT(nvarchar(100), InvoiceNo) END,
            CASE WHEN NULLIF(VendorName, '') IS NOT NULL THEN N' | Vendor: ' + VendorName END,
            CASE WHEN CancelFlag = 1 THEN N' | Currently cancelled' END)
    FROM dbo.PMaster m WHERE CompanyId = @CompanyId AND PurchaseDate >= @FromDate AND PurchaseDate < @LegacyTo
      AND NOT EXISTS (SELECT 1 FROM dbo.PurchaseActivityLog a
          WHERE a.CompanyId = m.CompanyId AND a.BranchId = m.BranchId AND a.FinYearId = m.FinYearId
            AND a.TransactionNo = m.PurchaseNo AND a.ActivityType = 'SAVE')
    UNION ALL
    SELECT SReturnDate, CompanyId, BranchId, FinYearId, UserId, UserName, NULL, NULL,
        N'Transaction', N'Sales Return', N'ON FILE', CONVERT(nvarchar(100), SReturnNo), NULL,
        GrandTotal, N'Document record', CASE WHEN CancelFlag = 1 THEN N'Currently cancelled' END
    FROM dbo.SReturnMaster m WHERE CompanyId = @CompanyId AND SReturnDate >= @FromDate AND SReturnDate < @LegacyTo
      AND NOT EXISTS (SELECT 1 FROM dbo.SalesReturnActivityLog a
          WHERE a.CompanyId = m.CompanyId AND a.BranchId = m.BranchId AND a.FinYearId = m.FinYearId
            AND a.TransactionNo = m.SReturnNo AND a.ActivityType = 'SAVE')
    UNION ALL
    SELECT PReturnDate, CompanyId, BranchId, FinYearId, UserId, UserName, NULL, NULL,
        N'Transaction', N'Purchase Return', N'ON FILE', CONVERT(nvarchar(100), PReturnNo), NULL,
        GrandTotal, N'Document record', CASE WHEN CancelFlag = 1 THEN N'Currently cancelled' END
    FROM dbo.PReturnMaster m WHERE CompanyId = @CompanyId AND PReturnDate >= @FromDate AND PReturnDate < @LegacyTo
      AND NOT EXISTS (SELECT 1 FROM dbo.PurchaseReturnActivityLog a
          WHERE a.CompanyId = m.CompanyId AND a.BranchId = m.BranchId AND a.FinYearId = m.FinYearId
            AND a.TransactionNo = m.PReturnNo AND a.ActivityType = 'SAVE')
    UNION ALL
    SELECT StockAdjustmentDate, CompanyId, BranchId, FinYearId, UserId, NULL, NULL, NULL,
        N'Transaction', N'Stock Adjustment', N'ON FILE', CONVERT(nvarchar(100), StockAdjustmentNo), NULL,
        NULL, N'Document record', CASE WHEN CancelFlag = 1 THEN N'Currently cancelled' END
    FROM dbo.StockAdjustmentMaster m WHERE CompanyId = @CompanyId AND StockAdjustmentDate >= @FromDate AND StockAdjustmentDate < @LegacyTo
      AND NOT EXISTS (SELECT 1 FROM dbo.StockAdjustmentActivityLog a
          WHERE a.CompanyId = m.CompanyId AND a.BranchId = m.BranchId AND a.FinYearId = m.FinYearId
            AND a.TransactionNo = m.StockAdjustmentNo AND a.ActivityType = 'SAVE')
    UNION ALL
    SELECT VoucherDate, CompanyId, BranchId, NULL, UserId, NULL, NULL, NULL, N'Payment',
        N'Vendor Payment', N'ON FILE', CONVERT(nvarchar(100), COALESCE(NULLIF(VoucherId, 0), Id)),
        NULL, PaymentAmount, N'Document record', CASE WHEN CancelFlag = 1 THEN N'Currently cancelled' END
    FROM dbo.VendorPaymentMaster m WHERE CompanyId = @CompanyId AND VoucherDate >= @FromDate AND VoucherDate < @LegacyTo
      AND NOT EXISTS (SELECT 1 FROM dbo.Vouchers v
          WHERE v.CompanyId = m.CompanyId AND v.BranchId = m.BranchId AND v.VoucherId = m.VoucherId
            AND UPPER(LTRIM(RTRIM(v.VoucherType))) IN ('VENDPAY', 'VENDOR PAYMENT'))
    UNION ALL
    SELECT VoucherDate, CompanyId, BranchId, NULL, UserId, NULL, NULL, NULL, N'Payment',
        N'Customer Receipt', N'ON FILE', CONVERT(nvarchar(100), COALESCE(NULLIF(VoucherId, 0), Id)),
        NULL, ReceiptAmount, N'Document record', CASE WHEN CancelFlag = 1 THEN N'Currently cancelled' END
    FROM dbo.CustomerReceiptMaster m WHERE CompanyId = @CompanyId AND VoucherDate >= @FromDate AND VoucherDate < @LegacyTo
      AND NOT EXISTS (SELECT 1 FROM dbo.Vouchers v
          WHERE v.CompanyId = m.CompanyId AND v.BranchId = m.BranchId AND v.VoucherId = m.VoucherId
            AND UPPER(LTRIM(RTRIM(v.VoucherType))) IN ('CUSTRCPT', 'CUSTREC', 'CUSTOMER RECEIPT'))
    UNION ALL
    SELECT VoucherDate, CompanyId, BranchId, NULL, UserId, NULL, NULL, NULL, N'Payment',
        N'Credit Note', N'ON FILE', CONVERT(nvarchar(100), COALESCE(NULLIF(VoucherId, 0), Id)),
        NULL, CreditAmount, N'Document record', CASE WHEN CancelFlag = 1 THEN N'Currently cancelled' END
    FROM dbo.CreditNoteMaster m WHERE CompanyId = @CompanyId AND VoucherDate >= @FromDate AND VoucherDate < @LegacyTo
      AND NOT EXISTS (SELECT 1 FROM dbo.Vouchers v
          WHERE v.CompanyId = m.CompanyId AND v.BranchId = m.BranchId AND v.VoucherId = m.VoucherId
            AND UPPER(LTRIM(RTRIM(v.VoucherType))) IN ('CREDIT NOTE', 'CREDITNOTE'))
    UNION ALL
    SELECT VoucherDate, CompanyId, BranchId, NULL, UserId, NULL, NULL, NULL, N'Payment',
        N'Debit Note', N'ON FILE', CONVERT(nvarchar(100), COALESCE(NULLIF(VoucherId, 0), Id)),
        NULL, DebitAmount, N'Document record', CASE WHEN CancelFlag = 1 THEN N'Currently cancelled' END
    FROM dbo.DebitNoteMaster m WHERE CompanyId = @CompanyId AND VoucherDate >= @FromDate AND VoucherDate < @LegacyTo
      AND NOT EXISTS (SELECT 1 FROM dbo.Vouchers v
          WHERE v.CompanyId = m.CompanyId AND v.BranchId = m.BranchId AND v.VoucherId = m.VoucherId
            AND UPPER(LTRIM(RTRIM(v.VoucherType))) IN ('DEBIT NOTE', 'DEBITNOTE'))
    UNION ALL
    SELECT VoucherDate, CompanyId, BranchId, NULL, UserId, NULL, NULL, NULL, N'Payment',
        N'Transporter Receipt', N'ON FILE', CONVERT(nvarchar(100), Id), NULL, ReceiptAmount,
        N'Document record', CASE WHEN CancelFlag = 1 THEN N'Currently cancelled' END
    FROM dbo.TransporterReceiptMaster WHERE CompanyId = @CompanyId AND VoucherDate >= @FromDate AND VoucherDate < @LegacyTo
    UNION ALL
    SELECT ulog.DateAndTime, u.CompanyId, ulog.BranchId, NULL, ulog.UserId, u.UserName, NULL, NULL,
        N'User', N'User Log', ulog.Title, NULL, NULL, NULL, N'User log', LEFT(ulog.Message, 4000)
    FROM dbo.UserLog ulog JOIN dbo.Users u ON u.UserId = ulog.UserId
    WHERE u.CompanyId = @CompanyId AND ulog.DateAndTime >= @FromDate AND ulog.DateAndTime < @ToExclusive
    UNION ALL
    SELECT PODate, CompanyId, BranchId, FinYearId, TRY_CONVERT(int, UserId), UserName,
        NULL, NULL, N'Transaction', N'Purchase Order', N'ON FILE', CONVERT(nvarchar(100), PONo), NULL,
        GrandTotal, N'Document record', CASE WHEN CancelFlag = 1 THEN N'Currently cancelled or deleted' END
    FROM dbo.POrderMaster WHERE CompanyId = @CompanyId AND PODate >= @FromDate AND PODate < @LegacyTo
    UNION ALL
    SELECT OrderDate, CompanyId, BranchId, FinYearId, TRY_CONVERT(int, UserId), NULL,
        CounterId, NULL, N'Transaction', N'Sales Order', N'ON FILE', CONVERT(nvarchar(100), OrderNo), NULL,
        NetAmount, N'Document record', CASE WHEN CancelFlag = 1 THEN N'Currently cancelled or deleted' END
    FROM dbo.SOrderMaster WHERE CompanyId = @CompanyId AND OrderDate >= @FromDate AND OrderDate < @LegacyTo
    UNION ALL
    SELECT QuotationDate, CompanyId, BranchId, FinYearId, TRY_CONVERT(int, NULL), NULL,
        CounterId, NULL, N'Transaction', N'Quotation', N'ON FILE', CONVERT(nvarchar(100), QuotationNo), NULL,
        NetAmount, N'Document record', CASE WHEN CancelFlag = 1 THEN N'Currently cancelled or deleted' END
    FROM dbo.QuotationMaster WHERE CompanyId = @CompanyId AND QuotationDate >= @FromDate AND QuotationDate < @LegacyTo
    UNION ALL
    SELECT DeliveryDate, CompanyId, BranchId, FinYearId, TRY_CONVERT(int, NULL), NULL,
        CounterId, NULL, N'Transaction', N'Delivery Order', N'ON FILE', CONVERT(nvarchar(100), DeliveryNo), NULL,
        NetAmount, N'Document record', CASE WHEN CancelFlag = 1 THEN N'Currently cancelled or deleted' END
    FROM dbo.DeliveryOrderMaster WHERE CompanyId = @CompanyId AND DeliveryDate >= @FromDate AND DeliveryDate < @LegacyTo
    UNION ALL
    SELECT TransferDate, CompanyId, SourceId, FinYearId, TRY_CONVERT(int, UserId), NULL,
        NULL, NULL, N'Transaction', N'Stock Transfer', N'ON FILE', CONVERT(nvarchar(100), StkTrNo), NULL,
        TotalAmount, N'Document record', CASE WHEN CancelFlag = 1 THEN N'Currently cancelled or deleted' END
    FROM dbo.StockTransferMaster WHERE CompanyId = @CompanyId AND TransferDate >= @FromDate AND TransferDate < @LegacyTo
    UNION ALL
    SELECT BillDate, CompanyId, BranchId, FinYearId, TRY_CONVERT(int, NULL), NULL,
        CounterId, NULL, N'Transaction', N'Held Bill', N'ON FILE', CONVERT(nvarchar(100), BillNo), NULL,
        NetAmount, N'Document record', CASE WHEN CancelFlag = 1 THEN N'Currently cancelled or deleted' END
    FROM dbo.SMasterHold WHERE CompanyId = @CompanyId AND BillDate >= @FromDate AND BillDate < @LegacyTo
    UNION ALL
    SELECT RequestedOnDate, CompanyId, RequestedByBranchId, FinYearId, TRY_CONVERT(int, RequestedByUserId), NULL,
        NULL, NULL, N'Transaction', N'Office Supplies', N'ON FILE', CONVERT(nvarchar(100), OfficeSupplyNo), NULL,
        TotalAmount, N'Document record', CASE WHEN CancelFlag = 1 THEN N'Currently cancelled or deleted' END
    FROM dbo.OfficeSuppliesMaster WHERE CompanyId = @CompanyId AND RequestedOnDate >= @FromDate AND RequestedOnDate < @LegacyTo
    UNION ALL
    SELECT [Date], CompanyId, BranchId, FinYearId, TRY_CONVERT(int, UserId), NULL,
        NULL, NULL, N'Transaction', N'Office Supplies Withdrawal', N'ON FILE', CONVERT(nvarchar(100), OfficeSupplyNo), NULL,
        TotalAmount, N'Document record', CASE WHEN CancelFlag = 1 THEN N'Currently cancelled or deleted' END
    FROM dbo.OfficeSuppliesWithdrawMaster WHERE CompanyId = @CompanyId AND [Date] >= @FromDate AND [Date] < @LegacyTo
    UNION ALL
    SELECT CreatedDate, CompanyId, BranchId, FinYearId, TRY_CONVERT(int, CreatedBy), NULL,
        NULL, NULL, N'Payment', N'Manual Party Balance', N'ON FILE', CONVERT(nvarchar(100), Id), NULL,
        Amount, N'Document record', CASE WHEN IsDeleted = 1 THEN N'Currently cancelled or deleted' END
    FROM dbo.ManualPartyBalance WHERE CompanyId = @CompanyId AND CreatedDate >= @FromDate AND CreatedDate < @LegacyTo
    UNION ALL
    SELECT CreatedDate, CompanyId, BranchId, FinYearId, TRY_CONVERT(int, NULL), NULL,
        NULL, NULL, N'Payment', N'Bank Reconciliation', N'ON FILE', CONVERT(nvarchar(100), VoucherId), NULL,
        NULL, N'Document record', CASE WHEN 0 = 1 THEN N'Currently cancelled or deleted' END
    FROM dbo.BankReconciliation WHERE CompanyId = @CompanyId AND CreatedDate >= @FromDate AND CreatedDate < @LegacyTo
    UNION ALL
    SELECT p.CreatedDate, p.CompanyId, p.BranchId, p.FinYearId, NULL, NULL, NULL, NULL,
        N'Payment', N'Sales Payment', N'ON FILE', CONVERT(nvarchar(100), p.BillNo), NULL,
        p.Amount, N'Document record', CONCAT(N'Bill: ', p.BillNo,
            CASE WHEN NULLIF(m.PayModeName, '') IS NOT NULL THEN N' | Payment: ' + m.PayModeName END,
            CASE WHEN p.Amount IS NOT NULL THEN N' | Amount: ' + CONVERT(nvarchar(60), TRY_CONVERT(decimal(28,4),p.Amount)) END)
    FROM dbo.SPaymentDetails p LEFT JOIN dbo.PayMode m ON m.PayModeID = p.PaymodeId
    WHERE p.CompanyId = @CompanyId AND p.CreatedDate >= @FromDate AND p.CreatedDate < @LegacyTo
    UNION ALL
    SELECT CreatedDate, CompanyId, BranchId, FinYearId, TRY_CONVERT(int, UserId), NULL,
        NULL, NULL, N'Counter', N'Shift Closing', N'ON FILE', CONVERT(nvarchar(100), DocNo), NULL,
        TotalCollection, N'Document record', CASE WHEN IsDelete = 1 THEN N'Currently cancelled or deleted' END
    FROM dbo.ShiftClosing WHERE CompanyId = @CompanyId AND CreatedDate >= @FromDate AND CreatedDate < @LegacyTo
    UNION ALL
    SELECT DateTimeFrom, CompanyId, BranchId, FinYearId, TRY_CONVERT(int, OpenedByUserId), NULL,
        NULL, NULL, N'Counter', N'Day Opening', N'ON FILE', CONVERT(nvarchar(100), Id), NULL,
        OpeningCash, N'Document record', CASE WHEN IsDelete = 1 THEN N'Currently cancelled or deleted' END
    FROM dbo.DayOpenings WHERE CompanyId = @CompanyId AND DateTimeFrom >= @FromDate AND DateTimeFrom < @LegacyTo
    )
    SELECT e.ActivityDate, COALESCE(NULLIF(b.BranchName, ''), CONVERT(nvarchar(20), e.BranchId), N'Shared / Unassigned') AS BranchName,
        e.Category, e.Module, activity.ActionType, e.DocumentNo,
        actor.UserName, actor.CounterName, e.RowVersion, TRY_CONVERT(decimal(28,4), e.Amount) AS Amount, e.Source, e.Details,
        e.BranchId AS EventBranchId, e.FinYearId AS EventFinYearId, e.CounterId AS EventCounterId
    INTO #ReportRows
    FROM Events e
    LEFT JOIN dbo.Branches b ON b.CompanyId = @CompanyId AND b.Id = e.BranchId
    LEFT JOIN dbo.Users u ON u.CompanyId = @CompanyId AND u.UserId = e.UserId
    LEFT JOIN dbo.CounterMaster c ON c.CounterId = e.CounterId
    CROSS APPLY (VALUES (COALESCE(NULLIF(e.UserName,''), NULLIF(u.UserName,''), CONVERT(nvarchar(20),e.UserId), N'Unrecorded'),
        COALESCE(NULLIF(e.CounterName,''), NULLIF(c.CounterName,''), CONVERT(nvarchar(20),e.CounterId), N'Unrecorded'))) actor(UserName, CounterName)
    CROSS APPLY (VALUES (UPPER(LTRIM(RTRIM(e.ActionType))))) activity(ActionType)
    WHERE e.Category IN (N'Transaction', N'Payment', N'Master')
      AND (@BranchId IS NULL OR e.BranchId = @BranchId)
      AND (@UserName IS NULL OR actor.UserName = @UserName)
      AND (@CounterName IS NULL OR actor.CounterName = @CounterName)
      AND (@Category IS NULL OR e.Category = @Category)
      AND (@ActionType IS NULL OR activity.ActionType = UPPER(LTRIM(RTRIM(@ActionType))))
      AND (@Filter IS NULL OR e.Module LIKE @Filter ESCAPE N'~' OR e.DocumentNo LIKE @Filter ESCAPE N'~'
           OR e.ActionType LIKE @Filter ESCAPE N'~' OR e.Details LIKE @Filter ESCAPE N'~'
           OR e.Source LIKE @Filter ESCAPE N'~' OR actor.UserName LIKE @Filter ESCAPE N'~'
           OR actor.CounterName LIKE @Filter ESCAPE N'~')
    OPTION (RECOMPILE);

    IF @IncludeDetailKeys = 1
        SELECT ActivityDate, BranchName, Category, Module, ActionType, DocumentNo, UserName, CounterName,
            RowVersion, Amount, Source, Details, EventBranchId, EventFinYearId, EventCounterId
        FROM #ReportRows ORDER BY ActivityDate DESC, Category, Module, DocumentNo, RowVersion DESC, Source;
    ELSE
        SELECT ActivityDate, BranchName, Category, Module, ActionType, DocumentNo, UserName, CounterName,
            RowVersion, Amount, Source, Details
        FROM #ReportRows ORDER BY ActivityDate DESC, Category, Module, DocumentNo, RowVersion DESC, Source;
END;
GO

IF OBJECT_ID(N'dbo.POS_ActivityReportFilters', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo.POS_ActivityReportFilters AS RETURN 0;');
GO
ALTER PROCEDURE dbo.POS_ActivityReportFilters @CompanyId int
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Access int;
    EXEC @Access = dbo.POS_RequireActivityAdministrator @CompanyId;
    IF @Access <> 0 RETURN;

    SELECT Id, BranchName FROM dbo.Branches WHERE CompanyId = @CompanyId ORDER BY BranchName, Id;

    SELECT UserName FROM
    (
        SELECT UserName FROM dbo.Users WHERE CompanyId = @CompanyId
        UNION SELECT UserName FROM dbo.UserActivityLog WHERE CompanyId = @CompanyId
        UNION SELECT DeletedBy FROM dbo.VoucherDeletionAudit WHERE CompanyId = @CompanyId
        UNION SELECT DeletedBy FROM dbo.LedgerDeletionArchive WHERE CompanyId = @CompanyId
        UNION SELECT N'Unrecorded'
    ) u WHERE NULLIF(UserName, '') IS NOT NULL ORDER BY UserName;

    SELECT CounterName FROM
    (
        SELECT c.CounterName FROM dbo.CounterMaster c
        WHERE EXISTS (SELECT 1 FROM dbo.Branches b WHERE b.CompanyId = @CompanyId AND b.Id = c.BranchId)
        UNION SELECT CounterName FROM dbo.UserActivityLog WHERE CompanyId = @CompanyId
        UNION SELECT Counter FROM dbo.VoucherDeletionAudit WHERE CompanyId = @CompanyId
        UNION SELECT N'Unrecorded'
    ) c WHERE NULLIF(CounterName, '') IS NOT NULL ORDER BY CounterName;

    SELECT DISTINCT UPPER(LTRIM(RTRIM(a.ActionType))) AS ActionType
    FROM
    (
        SELECT ActionType FROM (VALUES (N'SAVE'), (N'UPDATE'), (N'DELETE'), (N'ON FILE')) standard(ActionType)
        UNION ALL SELECT ActionType FROM dbo.POS_ActivityChange
            WHERE CompanyId = @CompanyId AND Category IN (N'Transaction', N'Payment', N'Master')
        UNION ALL SELECT ActivityType FROM dbo.SalesActivityLog WHERE CompanyId = @CompanyId
        UNION ALL SELECT ActivityType FROM dbo.PurchaseActivityLog WHERE CompanyId = @CompanyId
        UNION ALL SELECT ActivityType FROM dbo.SalesReturnActivityLog WHERE CompanyId = @CompanyId
        UNION ALL SELECT ActivityType FROM dbo.PurchaseReturnActivityLog WHERE CompanyId = @CompanyId
        UNION ALL SELECT ActivityType FROM dbo.StockAdjustmentActivityLog WHERE CompanyId = @CompanyId
        UNION ALL SELECT ActivityType FROM dbo.ItemActivityLog WHERE CompanyId = @CompanyId
    ) a
    WHERE NULLIF(LTRIM(RTRIM(a.ActionType)), N'') IS NOT NULL
    ORDER BY ActionType;
END;
GO
