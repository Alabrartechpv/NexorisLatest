-- Internal helper called by POS_ActivityReport. Its input has already been scoped to the report company.
IF OBJECT_ID(N'dbo.POS_GroupActivityReportChanges', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo.POS_GroupActivityReportChanges AS RETURN 0;');
GO
ALTER PROCEDURE dbo.POS_GroupActivityReportChanges
AS
BEGIN
    SET NOCOUNT ON;

    -- A trigger writes one before/after image per affected row. Start with one entry
    -- per statement, or one entry per voucher or item master action in the same database transaction.
    ;WITH Images AS
    (
        SELECT EventId, FIRST_VALUE(EventId) OVER
        (
            PARTITION BY CompanyId, BranchId, FinYearId, UserId, CounterId, SourceTable, Module, DocumentNo, ActionType,
                DirectVoucherEditId,
                CASE WHEN SourceTable IN (N'Vouchers',N'ItemMaster') THEN WriteTransactionId END,
                CASE WHEN DirectVoucherEditId IS NULL AND
                    (SourceTable NOT IN (N'Vouchers',N'ItemMaster') OR WriteTransactionId IS NULL) THEN WriteStatementId END,
                CASE WHEN DirectVoucherEditId IS NULL AND WriteStatementId IS NULL
                    AND (SourceTable NOT IN (N'Vouchers',N'ItemMaster') OR WriteTransactionId IS NULL)
                    THEN ActivityDate END
            ORDER BY CASE WHEN RowVersion = 'After' THEN 0 ELSE 1 END,
                CASE WHEN Amount IS NOT NULL THEN 0 ELSE 1 END, ActivityDate, EventId
        ) AS AnchorId
        FROM #ActivityImages
    )
    UPDATE a SET AnchorId = i.AnchorId
    FROM #ActivityImages a JOIN Images i ON i.EventId = a.EventId;

    SELECT a.* INTO #ActivityStatements
    FROM #ActivityImages a WHERE a.EventId = a.AnchorId;
    CREATE UNIQUE CLUSTERED INDEX IX_ActivityStatements_Event ON #ActivityStatements(EventId);
    CREATE INDEX IX_ActivityStatements_Document ON #ActivityStatements(SourceTable, DocumentNo, ActivityDate);

    DECLARE @DocumentTables TABLE (HeaderTable sysname, DetailTable sysname, PRIMARY KEY(HeaderTable, DetailTable));
    INSERT @DocumentTables VALUES
        (N'SMaster',N'SDetails'), (N'SMasterHold',N'SDetailsHold'), (N'SMaster',N'SPaymentDetails'),
        (N'PMaster',N'PDetails'), (N'SReturnMaster',N'SReturnDetails'), (N'PReturnMaster',N'PReturnDetails'),
        (N'POrderMaster',N'POrderDetails'), (N'SOrderMaster',N'SOrderDetails'),
        (N'QuotationMaster',N'QuotationDetails'), (N'DeliveryOrderMaster',N'DeliveryOrderDetails'),
        (N'StockAdjustmentMaster',N'StockAdjustmentDetails'), (N'StockTransferMaster',N'StockTransferDetail'),
        (N'VendorPaymentMaster',N'VendorPaymentDetails'), (N'CustomerReceiptMaster',N'CustomerReceiptDetails'),
        (N'CreditNoteMaster',N'CreditNoteDetails'), (N'DebitNoteMaster',N'DebitNoteDetails'),
        (N'TransporterReceiptMaster',N'TransporterReceiptDetails'),
        (N'OfficeSuppliesMaster',N'OfficeSuppliesDetails'), (N'OfficeSuppliesWithdrawMaster',N'OfficeSuppliesWithdrawDetails'),
        (N'VanSaleStockMaster',N'VanSaleStockDetails'), (N'MobileOrderMaster',N'MobileOrderDetails'),
        (N'ManualPartyBalance',N'ManualPartyBalanceSettlement');

    DECLARE @OperationHeaders TABLE (TableName sysname PRIMARY KEY);
    INSERT @OperationHeaders
        SELECT HeaderTable FROM @DocumentTables GROUP BY HeaderTable;
    INSERT @OperationHeaders VALUES (N'ItemMaster'), (N'LedgerMaster'), (N'VendorMaster'),
        (N'Vouchers'), (N'BankReconciliation');

    -- Match a related statement to one unambiguous header. Older audit rows have
    -- no transaction ID. Item status rules are also written by a separate repository.
    -- For those cases, require the same document, operator, counter and a narrow time window.
    ;WITH Candidates AS
    (
        SELECT d.EventId, h.EventId AS HeaderId,
            CASE WHEN d.WriteTransactionId = h.WriteTransactionId THEN 0 ELSE 3 END +
            CASE WHEN h.ActionType='SAVE' AND h.WriteTransactionId=d.WriteTransactionId
                      AND h.ActivityDate<=d.ActivityDate THEN 0
                 WHEN d.ActionType=h.ActionType THEN 1 ELSE 2 END AS Priority
        FROM #ActivityStatements d
        JOIN #ActivityStatements h ON h.EventId <> d.EventId
          AND (h.BranchId = d.BranchId OR (h.BranchId IS NULL AND d.BranchId IS NULL))
          AND (h.FinYearId = d.FinYearId OR h.FinYearId IS NULL OR d.FinYearId IS NULL)
          AND (h.UserId = d.UserId OR (h.UserId IS NULL AND d.UserId IS NULL))
          AND (h.CounterId = d.CounterId OR (h.CounterId IS NULL AND d.CounterId IS NULL))
          AND (h.WriteTransactionId = d.WriteTransactionId OR
               ((h.WriteTransactionId IS NULL OR d.WriteTransactionId IS NULL
                 OR (d.SourceTable=N'POS_ItemMasterStatusRules' AND h.SourceTable=N'ItemMaster'
                     AND d.DocumentNo=h.DocumentNo))
                AND h.ActivityDate BETWEEN DATEADD(millisecond,-1000,d.ActivityDate) AND DATEADD(millisecond,1000,d.ActivityDate)))
        WHERE
            (d.ActionType='UPDATE' AND d.DirectVoucherEditId IS NULL
             AND h.ActionType='SAVE' AND d.SourceTable=h.SourceTable
             AND d.Module=h.Module AND d.DocumentNo=h.DocumentNo AND h.ActivityDate<=d.ActivityDate
             AND d.WriteTransactionId=h.WriteTransactionId
             AND EXISTS (SELECT 1 FROM @OperationHeaders m WHERE m.TableName=h.SourceTable))
         OR (EXISTS (SELECT 1 FROM @DocumentTables m WHERE m.HeaderTable=h.SourceTable AND m.DetailTable=d.SourceTable)
             AND (h.DocumentNo=d.DocumentNo OR
                  (h.WriteTransactionId=d.WriteTransactionId AND h.SourceTable IN
                      (N'StockAdjustmentMaster',N'StockTransferMaster',N'VendorPaymentMaster',N'CustomerReceiptMaster',
                       N'CreditNoteMaster',N'DebitNoteMaster',N'TransporterReceiptMaster',N'OfficeSuppliesMaster',
                       N'OfficeSuppliesWithdrawMaster',N'VanSaleStockMaster',N'MobileOrderMaster',N'ManualPartyBalance'))))
         OR (d.SourceTable=N'Vouchers' AND
               (d.DocumentNo=h.DocumentNo OR
                CHARINDEX(N' | voucherid='+d.DocumentNo+N' | ', LOWER(N' | '+h.Details+N' | '))>0) AND
               ((h.SourceTable=N'PMaster' AND d.Module=N'Purchase Posting')
             OR (h.SourceTable=N'SMaster' AND d.Module=N'Sales Posting')
             OR (h.SourceTable=N'VendorPaymentMaster' AND d.Module=N'Vendor Payment Posting')
             OR (h.SourceTable=N'CustomerReceiptMaster' AND d.Module=N'Customer Receipt Posting')
             OR (h.SourceTable=N'CreditNoteMaster' AND d.Module=N'Credit Note Posting')
             OR (h.SourceTable=N'DebitNoteMaster' AND d.Module=N'Debit Note Posting')))
         OR (h.SourceTable=N'ItemMaster' AND d.DocumentNo=h.DocumentNo AND
              (d.SourceTable IN (N'PriceSettings',N'POS_ItemMasterStatusRules',N'ItemBatch',N'ItemAlternativeBarcode',N'ItemDimension')
               OR (d.SourceTable=N'ItemMaster' AND h.ActionType='SAVE' AND d.ActionType='UPDATE'
                   AND (h.WriteTransactionId IS NULL OR d.WriteTransactionId IS NULL)
                   AND NOT EXISTS
                   (
                       SELECT a.Details FROM #ActivityImages a WHERE a.AnchorId=d.EventId AND a.RowVersion='Before'
                       EXCEPT
                       SELECT a.Details FROM #ActivityImages a WHERE a.AnchorId=d.EventId AND a.RowVersion='After'
                   ) AND h.ActivityDate<=d.ActivityDate)))
    ),
    Ranked AS
    (
        SELECT *, DENSE_RANK() OVER (PARTITION BY EventId ORDER BY Priority) AS MatchRank
        FROM Candidates
    ),
    UniqueMatches AS
    (
        SELECT EventId, MIN(CONVERT(char(36),HeaderId)) AS HeaderId
        FROM Ranked WHERE MatchRank=1 GROUP BY EventId HAVING COUNT(*)=1
    )
    UPDATE a SET AnchorId=CONVERT(uniqueidentifier,m.HeaderId)
    FROM #ActivityImages a JOIN UniqueMatches m ON m.EventId=a.AnchorId;

    -- Inventory writes for an invoice belong under that invoice only when a
    -- captured line identifies the item and exactly one invoice owns the change.
    ;WITH StockOwners AS
    (
        SELECT s.EventId, MIN(CONVERT(char(36),a.AnchorId)) AS OwnerId
        FROM #ActivityStatements s
        JOIN #ActivityImages original ON original.EventId=s.EventId AND original.AnchorId=s.EventId
        JOIN #ActivityImages a ON a.SourceTable IN (N'PDetails',N'SDetails',N'PReturnDetails',N'SReturnDetails',N'StockAdjustmentDetails')
          AND a.EventId<>a.AnchorId AND a.BranchId=s.BranchId AND a.FinYearId=s.FinYearId
          AND a.UserId=s.UserId AND (a.CounterId=s.CounterId OR (a.CounterId IS NULL AND s.CounterId IS NULL))
          AND (a.WriteTransactionId=s.WriteTransactionId OR
               ((a.WriteTransactionId IS NULL OR s.WriteTransactionId IS NULL)
                AND a.ActivityDate BETWEEN DATEADD(millisecond,-500,s.ActivityDate) AND DATEADD(millisecond,500,s.ActivityDate)))
        CROSS APPLY (VALUES (LOWER(N' | '+a.Details))) image(Text)
        WHERE s.SourceTable IN (N'PriceSettings',N'ItemMaster') AND s.ActionType='UPDATE'
          AND CHARINDEX(N' | itemid='+LOWER(s.DocumentNo)+N' | ',image.Text+N' | ')>0
        GROUP BY s.EventId
        HAVING COUNT(DISTINCT a.AnchorId)=1
    )
    UPDATE a SET AnchorId=CONVERT(uniqueidentifier,o.OwnerId)
    FROM #ActivityImages a JOIN StockOwners o ON o.EventId=a.AnchorId;

    -- Resolve item-side changes attached to an item update that was itself
    -- folded into an item save or an invoice.
    UPDATE a SET AnchorId=p.AnchorId
    FROM #ActivityImages a JOIN #ActivityImages p ON p.EventId=a.AnchorId
    WHERE a.AnchorId<>p.AnchorId;
END;
GO
