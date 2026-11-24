-- =========================================================================================================
-- Stored Procedure : POS_DeletionAuditReport
-- Description      : Dedicated comprehensive audit procedure for ALL deleted/cancelled transactions and masters.
--                    Covers: Sales, Purchase, Sales Return, Purchase Return, Customer Receipt, Vendor Payment,
--                            General Payment, General Receipt, Journal, Contra, Debit Note, Credit Note,
--                            Stock Adjustment, Hold Bills, Item Deletions, and Ledger/Account Deletions.
--                    RESTRICTED: Administrator only.
-- =========================================================================================================
IF OBJECT_ID('dbo.POS_DeletionAuditReport', 'P') IS NOT NULL
    DROP PROCEDURE dbo.POS_DeletionAuditReport;
GO

CREATE PROCEDURE dbo.POS_DeletionAuditReport
(
    @CompanyId          INT             = 0,
    @BranchId           INT             = 0,
    @FinYearId          INT             = 0,
    @FromDate           DATETIME        = NULL,
    @ToDate             DATETIME        = NULL,
    @TransactionType    VARCHAR(50)     = 'ALL', -- 'ALL', 'SALES', 'PURCHASE', 'SALES_RETURN', 'PURCHASE_RETURN', 'CUSTOMER_RECEIPT', 'VENDOR_PAYMENT', 'GENERAL_PAYMENT', 'GENERAL_RECEIPT', 'JOURNAL_VOUCHER', 'CONTRA_VOUCHER', 'CREDIT_NOTE', 'DEBIT_NOTE', 'STOCK_ADJUSTMENT', 'HOLD_BILL', 'ITEM', 'LEDGER'
    @UserFilter         VARCHAR(100)    = 'ALL', -- 'ALL' or specific username
    @SearchText         VARCHAR(100)    = ''
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Defaults
    IF (@FromDate IS NULL OR @FromDate < '1900-01-01')
        SET @FromDate = '1900-01-01';
    IF (@ToDate IS NULL OR @ToDate < '1900-01-01')
        SET @ToDate = CAST(GETDATE() AS DATE);

    DECLARE @RangeFrom DATETIME = CAST(@FromDate AS DATE);
    DECLARE @RangeToExclusive DATETIME = DATEADD(DAY, 1, CAST(@ToDate AS DATE));
    SET @TransactionType = UPPER(LTRIM(RTRIM(ISNULL(@TransactionType, 'ALL'))));
    SET @UserFilter = LTRIM(RTRIM(ISNULL(@UserFilter, 'ALL')));
    SET @SearchText = LTRIM(RTRIM(ISNULL(@SearchText, '')));

    WITH AllDeletions AS (
        -- 1. Sales Master (CancelFlag = 1) - Differentiates Sales Invoices vs Hold Bills
        SELECT 
            CAST(CASE 
                WHEN m.Status LIKE '%Hold%' OR EXISTS (SELECT 1 FROM dbo.SalesActivityLog salH WHERE salH.TransactionNo = m.BillNo AND salH.ActivityType = 'HOLD') THEN 'HOLD_BILL'
                ELSE 'SALES'
            END AS NVARCHAR(50)) AS ModuleKey,
            CAST(CASE 
                WHEN m.Status LIKE '%Hold%' OR EXISTS (SELECT 1 FROM dbo.SalesActivityLog salH WHERE salH.TransactionNo = m.BillNo AND salH.ActivityType = 'HOLD') THEN 'Hold Bill'
                ELSE 'Sales Invoice'
            END AS NVARCHAR(100)) AS TransactionType,
            CAST(CASE 
                WHEN m.Status LIKE '%Hold%' OR EXISTS (SELECT 1 FROM dbo.SalesActivityLog salH WHERE salH.TransactionNo = m.BillNo AND salH.ActivityType = 'HOLD') THEN 'HOLD-' + CAST(m.BillNo AS NVARCHAR(50))
                ELSE 'Bill #' + CAST(m.BillNo AS NVARCHAR(50))
            END AS NVARCHAR(100)) AS DocNo,
            CAST(m.BillNo AS NVARCHAR(100)) AS RefNo,
            CAST(m.BillDate AS DATETIME) AS DocDate,
            CAST(ISNULL(
                (SELECT TOP 1 sal.CreatedOn 
                 FROM dbo.SalesActivityLog sal 
                 WHERE sal.TransactionNo = m.BillNo AND sal.ActivityType = 'DELETE' 
                 ORDER BY sal.CreatedOn DESC),
                ISNULL(
                    (SELECT TOP 1 DATEADD(minute, DATEDIFF(minute, GETUTCDATE(), GETDATE()), sq.CreatedUtc) 
                     FROM dbo.SyncQueue sq 
                     WHERE sq.EntityType = 'SALES' AND sq.EntityId = CAST(m.BillNo AS VARCHAR(50)) AND sq.Operation = 'CANCEL' 
                     ORDER BY sq.SyncId DESC),
                    m.BillDate
                )
            ) AS DATETIME) AS DeletedDate,
            CAST(ISNULL(NULLIF(m.CustomerName, ''), 'Walk-in Customer') AS NVARCHAR(200)) AS PartyName,
            CAST(ISNULL(m.NetAmount, 0) AS DECIMAL(18,2)) AS Amount,
            CAST(0 AS DECIMAL(18,2)) AS Qty,
            CAST(ISNULL(
                (SELECT TOP 1 sal.UserName 
                 FROM dbo.SalesActivityLog sal 
                 WHERE sal.TransactionNo = m.BillNo AND sal.ActivityType = 'DELETE' 
                 ORDER BY sal.CreatedOn DESC),
                ISNULL(u.UserName, 'admin')
            ) AS NVARCHAR(100)) AS DeletedBy,
            CAST(ISNULL(c.CounterName, ISNULL((SELECT TOP 1 sal.CounterName FROM dbo.SalesActivityLog sal WHERE sal.TransactionNo = m.BillNo AND sal.ActivityType = 'DELETE' ORDER BY sal.CreatedOn DESC), '')) AS NVARCHAR(100)) AS Counter,
            CAST(ISNULL(
                (SELECT TOP 1 sal.ActivityDetails 
                 FROM dbo.SalesActivityLog sal 
                 WHERE sal.TransactionNo = m.BillNo AND sal.ActivityType = 'DELETE' 
                 ORDER BY sal.CreatedOn DESC),
                CASE 
                    WHEN m.Status LIKE '%Hold%' THEN 'Hold Bill Cancelled / Deleted'
                    ELSE 'Sales Invoice Cancelled / Deleted'
                END
            ) AS NVARCHAR(500)) AS DeletionReason,
            CAST(CASE 
                WHEN m.Status LIKE '%Hold%' THEN 'Hold Bill #' + CAST(m.BillNo AS NVARCHAR(50)) + ' Cancelled'
                ELSE 'Bill #' + CAST(m.BillNo AS NVARCHAR(50)) + ' Cancelled'
            END AS NVARCHAR(MAX)) AS SourceDetails,
            m.CompanyId, m.BranchId, m.FinYearId
        FROM dbo.SMaster m
        LEFT JOIN dbo.Users u ON m.UserId = u.UserId
        LEFT JOIN dbo.CounterMaster c ON m.CounterId = c.CounterId
        WHERE m.CancelFlag = 1

        UNION ALL

        -- 2. Sales Activity Log Deletions (hard deleted or line-level) - Also checks for Hold Bill
        SELECT 
            CAST(CASE 
                WHEN sal.ActivityDetails LIKE '%Hold%' OR EXISTS (SELECT 1 FROM dbo.SalesActivityLog salH WHERE salH.TransactionNo = sal.TransactionNo AND salH.ActivityType = 'HOLD') OR EXISTS (SELECT 1 FROM dbo.SMaster sm WHERE sm.BillNo = sal.TransactionNo AND sm.Status LIKE '%Hold%') THEN 'HOLD_BILL'
                ELSE 'SALES'
            END AS NVARCHAR(50)) AS ModuleKey,
            CAST(CASE 
                WHEN sal.ActivityDetails LIKE '%Hold%' OR EXISTS (SELECT 1 FROM dbo.SalesActivityLog salH WHERE salH.TransactionNo = sal.TransactionNo AND salH.ActivityType = 'HOLD') OR EXISTS (SELECT 1 FROM dbo.SMaster sm WHERE sm.BillNo = sal.TransactionNo AND sm.Status LIKE '%Hold%') THEN 'Hold Bill'
                ELSE 'Sales Invoice'
            END AS NVARCHAR(100)) AS TransactionType,
            CAST(CASE 
                WHEN sal.ActivityDetails LIKE '%Hold%' OR EXISTS (SELECT 1 FROM dbo.SalesActivityLog salH WHERE salH.TransactionNo = sal.TransactionNo AND salH.ActivityType = 'HOLD') OR EXISTS (SELECT 1 FROM dbo.SMaster sm WHERE sm.BillNo = sal.TransactionNo AND sm.Status LIKE '%Hold%') THEN 'HOLD-' + CAST(sal.TransactionNo AS NVARCHAR(50))
                ELSE 'Bill #' + CAST(sal.TransactionNo AS NVARCHAR(50))
            END AS NVARCHAR(100)) AS DocNo,
            CAST(ISNULL(sal.InvoiceNo, '') AS NVARCHAR(100)) AS RefNo,
            CAST(ISNULL((SELECT TOP 1 sm.BillDate FROM dbo.SMaster sm WHERE sm.BillNo = sal.TransactionNo), sal.CreatedOn) AS DATETIME) AS DocDate,
            CAST(sal.CreatedOn AS DATETIME) AS DeletedDate,
            CAST(ISNULL(NULLIF(sal.PartyName, ''), 'Walk-in Customer') AS NVARCHAR(200)) AS PartyName,
            CAST(ISNULL(sal.NetAmount, 0) AS DECIMAL(18,2)) AS Amount,
            CAST(ISNULL(sal.Qty, 0) AS DECIMAL(18,2)) AS Qty,
            CAST(ISNULL(sal.UserName, 'admin') AS NVARCHAR(100)) AS DeletedBy,
            CAST(ISNULL(sal.CounterName, '') AS NVARCHAR(100)) AS Counter,
            CAST(CASE 
                WHEN sal.ActivityDetails LIKE '%Hold%' OR EXISTS (SELECT 1 FROM dbo.SalesActivityLog salH WHERE salH.TransactionNo = sal.TransactionNo AND salH.ActivityType = 'HOLD') THEN 'Deleted via Hold Bills screen'
                ELSE 'Deleted via Sales screen'
            END AS NVARCHAR(500)) AS DeletionReason,
            CAST(ISNULL(sal.ActivityDetails, '') AS NVARCHAR(MAX)) AS SourceDetails,
            sal.CompanyId, sal.BranchId, sal.FinYearId
        FROM dbo.SalesActivityLog sal
        WHERE sal.ActivityType = 'DELETE'
          AND NOT EXISTS (SELECT 1 FROM dbo.SMaster sm WHERE sm.BillNo = sal.TransactionNo AND sm.CancelFlag = 1)

        UNION ALL

        -- 3. Purchase Master (CancelFlag = 1)
        SELECT 
            CAST('PURCHASE' AS NVARCHAR(50)) AS ModuleKey,
            CAST('Purchase Invoice' AS NVARCHAR(100)) AS TransactionType,
            CAST('GRN-' + CAST(m.PurchaseNo AS NVARCHAR(50)) AS NVARCHAR(100)) AS DocNo,
            CAST(ISNULL(m.InvoiceNo, '') AS NVARCHAR(100)) AS RefNo,
            CAST(m.PurchaseDate AS DATETIME) AS DocDate,
            CAST(ISNULL(
                (SELECT TOP 1 pal.CreatedOn 
                 FROM dbo.PurchaseActivityLog pal 
                 WHERE pal.TransactionNo = m.PurchaseNo AND pal.ActivityType = 'DELETE' 
                 ORDER BY pal.CreatedOn DESC),
                ISNULL(
                    (SELECT TOP 1 DATEADD(minute, DATEDIFF(minute, GETUTCDATE(), GETDATE()), sq.CreatedUtc) 
                     FROM dbo.SyncQueue sq 
                     WHERE sq.EntityType = 'PURCHASE' AND sq.EntityId = CAST(m.PurchaseNo AS VARCHAR(50)) AND sq.Operation = 'CANCEL' 
                     ORDER BY sq.SyncId DESC),
                    ISNULL(m.InvoiceDate, m.PurchaseDate)
                )
            ) AS DATETIME) AS DeletedDate,
            CAST(ISNULL(NULLIF(m.VendorName, ''), 'General Supplier') AS NVARCHAR(200)) AS PartyName,
            CAST(ISNULL(m.GrandTotal, ISNULL(m.SubTotal, 0)) AS DECIMAL(18,2)) AS Amount,
            CAST(ISNULL((SELECT SUM(ISNULL(pal.Qty, 0)) FROM dbo.PurchaseActivityLog pal WHERE pal.TransactionNo = m.PurchaseNo AND pal.ActivityType = 'DELETE'), 0) AS DECIMAL(18,2)) AS Qty,
            CAST(ISNULL(
                (SELECT TOP 1 pal.UserName 
                 FROM dbo.PurchaseActivityLog pal 
                 WHERE pal.TransactionNo = m.PurchaseNo AND pal.ActivityType = 'DELETE' 
                 ORDER BY pal.CreatedOn DESC),
                ISNULL(m.UserName, 'admin')
            ) AS NVARCHAR(100)) AS DeletedBy,
            CAST(ISNULL(
                (SELECT TOP 1 pal.CounterName 
                 FROM dbo.PurchaseActivityLog pal 
                 WHERE pal.TransactionNo = m.PurchaseNo AND pal.ActivityType = 'DELETE' 
                 ORDER BY pal.CreatedOn DESC),
                ISNULL(
                    (SELECT TOP 1 c.CounterName 
                     FROM dbo.PurchaseActivityLog pal 
                     LEFT JOIN dbo.CounterMaster c ON pal.CounterId = c.CounterId 
                     WHERE pal.TransactionNo = m.PurchaseNo 
                     ORDER BY pal.CreatedOn DESC),
                    ''
                )
            ) AS NVARCHAR(100)) AS Counter,
            CAST(ISNULL(
                (SELECT TOP 1 pal.ActivityDetails 
                 FROM dbo.PurchaseActivityLog pal 
                 WHERE pal.TransactionNo = m.PurchaseNo AND pal.ActivityType = 'DELETE' 
                 ORDER BY pal.CreatedOn DESC),
                ISNULL(NULLIF(m.Remarks, ''), 'Purchase Invoice Cancelled / Deleted')
            ) AS NVARCHAR(500)) AS DeletionReason,
            CAST('GRN-' + CAST(m.PurchaseNo AS NVARCHAR(50)) + ' Cancelled' AS NVARCHAR(MAX)) AS SourceDetails,
            m.CompanyId, m.BranchId, m.FinYearId
        FROM dbo.PMaster m
        WHERE m.CancelFlag = 1

        UNION ALL

        -- 4. Purchase Activity Log Deletions
        SELECT 
            CAST('PURCHASE' AS NVARCHAR(50)) AS ModuleKey,
            CAST('Purchase Invoice' AS NVARCHAR(100)) AS TransactionType,
            CAST('GRN-' + CAST(pal.TransactionNo AS NVARCHAR(50)) AS NVARCHAR(100)) AS DocNo,
            CAST(ISNULL(pal.InvoiceNo, '') AS NVARCHAR(100)) AS RefNo,
            CAST(ISNULL((SELECT TOP 1 pm.PurchaseDate FROM dbo.PMaster pm WHERE pm.PurchaseNo = pal.TransactionNo), pal.CreatedOn) AS DATETIME) AS DocDate,
            CAST(pal.CreatedOn AS DATETIME) AS DeletedDate,
            CAST(ISNULL(NULLIF(pal.PartyName, ''), 'General Supplier') AS NVARCHAR(200)) AS PartyName,
            CAST(ISNULL((SELECT TOP 1 pm.GrandTotal FROM dbo.PMaster pm WHERE pm.PurchaseNo = pal.TransactionNo), ISNULL(pal.NetAmount, 0)) AS DECIMAL(18,2)) AS Amount,
            CAST(ISNULL(pal.Qty, 0) AS DECIMAL(18,2)) AS Qty,
            CAST(ISNULL(pal.UserName, 'admin') AS NVARCHAR(100)) AS DeletedBy,
            CAST(ISNULL(pal.CounterName, '') AS NVARCHAR(100)) AS Counter,
            CAST('Deleted via Purchase screen' AS NVARCHAR(500)) AS DeletionReason,
            CAST(ISNULL(pal.ActivityDetails, '') AS NVARCHAR(MAX)) AS SourceDetails,
            pal.CompanyId, pal.BranchId, pal.FinYearId
        FROM dbo.PurchaseActivityLog pal
        WHERE pal.ActivityType = 'DELETE'
          AND NOT EXISTS (SELECT 1 FROM dbo.PMaster pm WHERE pm.PurchaseNo = pal.TransactionNo AND pm.CancelFlag = 1)

        UNION ALL

        -- 5. Sales Return Master
        SELECT 
            CAST('SALES_RETURN' AS NVARCHAR(50)) AS ModuleKey,
            CAST('Sales Return' AS NVARCHAR(100)) AS TransactionType,
            CAST('SR-' + CAST(sr.SReturnNo AS NVARCHAR(50)) AS NVARCHAR(100)) AS DocNo,
            CAST(ISNULL(sr.InvoiceNo, '') AS NVARCHAR(100)) AS RefNo,
            CAST(sr.SReturnDate AS DATETIME) AS DocDate,
            CAST(ISNULL(
                (SELECT TOP 1 sral.CreatedOn 
                 FROM dbo.SalesReturnActivityLog sral 
                 WHERE sral.TransactionNo = sr.SReturnNo AND sral.ActivityType = 'DELETE' 
                 ORDER BY sral.CreatedOn DESC),
                sr.SReturnDate
            ) AS DATETIME) AS DeletedDate,
            CAST(ISNULL(NULLIF(sr.CustomerName, ''), 'Customer') AS NVARCHAR(200)) AS PartyName,
            CAST(ISNULL(sr.GrandTotal, 0) AS DECIMAL(18,2)) AS Amount,
            CAST(0 AS DECIMAL(18,2)) AS Qty,
            CAST(ISNULL(
                (SELECT TOP 1 sral.UserName 
                 FROM dbo.SalesReturnActivityLog sral 
                 WHERE sral.TransactionNo = sr.SReturnNo AND sral.ActivityType = 'DELETE' 
                 ORDER BY sral.CreatedOn DESC),
                ISNULL(sr.UserName, 'admin')
            ) AS NVARCHAR(100)) AS DeletedBy,
            CAST(ISNULL(
                (SELECT TOP 1 sral.CounterName 
                 FROM dbo.SalesReturnActivityLog sral 
                 WHERE sral.TransactionNo = sr.SReturnNo AND sral.ActivityType = 'DELETE' 
                 ORDER BY sral.CreatedOn DESC),
                ISNULL(
                    (SELECT TOP 1 c.CounterName 
                     FROM dbo.SalesReturnActivityLog sral 
                     LEFT JOIN dbo.CounterMaster c ON sral.CounterId = c.CounterId 
                     WHERE sral.TransactionNo = sr.SReturnNo 
                     ORDER BY sral.CreatedOn DESC),
                    ''
                )
            ) AS NVARCHAR(100)) AS Counter,
            CAST(ISNULL(
                (SELECT TOP 1 sral.ActivityDetails 
                 FROM dbo.SalesReturnActivityLog sral 
                 WHERE sral.TransactionNo = sr.SReturnNo AND sral.ActivityType = 'DELETE' 
                 ORDER BY sral.CreatedOn DESC),
                ISNULL(NULLIF(sr.Remarks, ''), 'Sales Return Cancelled / Deleted')
            ) AS NVARCHAR(500)) AS DeletionReason,
            CAST('SR-' + CAST(sr.SReturnNo AS NVARCHAR(50)) + ' Cancelled' AS NVARCHAR(MAX)) AS SourceDetails,
            sr.CompanyId, sr.BranchId, sr.FinYearId
        FROM dbo.SReturnMaster sr
        WHERE sr.CancelFlag = 1

        UNION ALL

        -- 6. Sales Return Activity Log Deletions
        SELECT 
            CAST('SALES_RETURN' AS NVARCHAR(50)) AS ModuleKey,
            CAST('Sales Return' AS NVARCHAR(100)) AS TransactionType,
            CAST('SR-' + CAST(sral.TransactionNo AS NVARCHAR(50)) AS NVARCHAR(100)) AS DocNo,
            CAST(ISNULL(sral.InvoiceNo, '') AS NVARCHAR(100)) AS RefNo,
            CAST(sral.CreatedOn AS DATETIME) AS DocDate,
            CAST(sral.CreatedOn AS DATETIME) AS DeletedDate,
            CAST(ISNULL(NULLIF(sral.PartyName, ''), 'Customer') AS NVARCHAR(200)) AS PartyName,
            CAST(ISNULL(sral.NetAmount, 0) AS DECIMAL(18,2)) AS Amount,
            CAST(ISNULL(sral.Qty, 0) AS DECIMAL(18,2)) AS Qty,
            CAST(ISNULL(sral.UserName, 'admin') AS NVARCHAR(100)) AS DeletedBy,
            CAST(ISNULL(sral.CounterName, '') AS NVARCHAR(100)) AS Counter,
            CAST('Deleted via Sales Return screen' AS NVARCHAR(500)) AS DeletionReason,
            CAST(ISNULL(sral.ActivityDetails, '') AS NVARCHAR(MAX)) AS SourceDetails,
            sral.CompanyId, sral.BranchId, sral.FinYearId
        FROM dbo.SalesReturnActivityLog sral
        WHERE sral.ActivityType = 'DELETE'
          AND NOT EXISTS (SELECT 1 FROM dbo.SReturnMaster srm WHERE srm.SReturnNo = sral.TransactionNo AND srm.CancelFlag = 1)

        UNION ALL

        -- 7. Purchase Return Master
        SELECT 
            CAST('PURCHASE_RETURN' AS NVARCHAR(50)) AS ModuleKey,
            CAST('Purchase Return' AS NVARCHAR(100)) AS TransactionType,
            CAST('PR-' + CAST(pr.PReturnNo AS NVARCHAR(50)) AS NVARCHAR(100)) AS DocNo,
            CAST(ISNULL(pr.InvoiceNo, '') AS NVARCHAR(100)) AS RefNo,
            CAST(pr.PReturnDate AS DATETIME) AS DocDate,
            CAST(ISNULL(
                (SELECT TOP 1 pral.CreatedOn 
                 FROM dbo.PurchaseReturnActivityLog pral 
                 WHERE pral.TransactionNo = pr.PReturnNo AND pral.ActivityType = 'DELETE' 
                 ORDER BY pral.CreatedOn DESC),
                pr.PReturnDate
            ) AS DATETIME) AS DeletedDate,
            CAST(ISNULL(NULLIF(pr.VendorName, ''), 'Vendor') AS NVARCHAR(200)) AS PartyName,
            CAST(ISNULL(pr.GrandTotal, 0) AS DECIMAL(18,2)) AS Amount,
            CAST(0 AS DECIMAL(18,2)) AS Qty,
            CAST(ISNULL(
                (SELECT TOP 1 pral.UserName 
                 FROM dbo.PurchaseReturnActivityLog pral 
                 WHERE pral.TransactionNo = pr.PReturnNo AND pral.ActivityType = 'DELETE' 
                 ORDER BY pral.CreatedOn DESC),
                ISNULL(pr.UserName, 'admin')
            ) AS NVARCHAR(100)) AS DeletedBy,
            CAST(ISNULL(
                (SELECT TOP 1 pral.CounterName 
                 FROM dbo.PurchaseReturnActivityLog pral 
                 WHERE pral.TransactionNo = pr.PReturnNo AND pral.ActivityType = 'DELETE' 
                 ORDER BY pral.CreatedOn DESC),
                ISNULL(
                    (SELECT TOP 1 c.CounterName 
                     FROM dbo.PurchaseReturnActivityLog pral 
                     LEFT JOIN dbo.CounterMaster c ON pral.CounterId = c.CounterId 
                     WHERE pral.TransactionNo = pr.PReturnNo 
                     ORDER BY pral.CreatedOn DESC),
                    ''
                )
            ) AS NVARCHAR(100)) AS Counter,
            CAST(ISNULL(
                (SELECT TOP 1 pral.ActivityDetails 
                 FROM dbo.PurchaseReturnActivityLog pral 
                 WHERE pral.TransactionNo = pr.PReturnNo AND pral.ActivityType = 'DELETE' 
                 ORDER BY pral.CreatedOn DESC),
                ISNULL(NULLIF(pr.Remarks, ''), 'Purchase Return Cancelled / Deleted')
            ) AS NVARCHAR(500)) AS DeletionReason,
            CAST('PR-' + CAST(pr.PReturnNo AS NVARCHAR(50)) + ' Cancelled' AS NVARCHAR(MAX)) AS SourceDetails,
            pr.CompanyId, pr.BranchId, pr.FinYearId
        FROM dbo.PReturnMaster pr
        WHERE pr.CancelFlag = 1

        UNION ALL

        -- 8. Purchase Return Activity Log Deletions
        SELECT 
            CAST('PURCHASE_RETURN' AS NVARCHAR(50)) AS ModuleKey,
            CAST('Purchase Return' AS NVARCHAR(100)) AS TransactionType,
            CAST('PR-' + CAST(pral.TransactionNo AS NVARCHAR(50)) AS NVARCHAR(100)) AS DocNo,
            CAST(ISNULL(pral.InvoiceNo, '') AS NVARCHAR(100)) AS RefNo,
            CAST(pral.CreatedOn AS DATETIME) AS DocDate,
            CAST(pral.CreatedOn AS DATETIME) AS DeletedDate,
            CAST(ISNULL(NULLIF(pral.PartyName, ''), 'Vendor') AS NVARCHAR(200)) AS PartyName,
            CAST(ISNULL(pral.NetAmount, 0) AS DECIMAL(18,2)) AS Amount,
            CAST(ISNULL(pral.Qty, 0) AS DECIMAL(18,2)) AS Qty,
            CAST(ISNULL(pral.UserName, 'admin') AS NVARCHAR(100)) AS DeletedBy,
            CAST(ISNULL(pral.CounterName, '') AS NVARCHAR(100)) AS Counter,
            CAST('Deleted via Purchase Return screen' AS NVARCHAR(500)) AS DeletionReason,
            CAST(ISNULL(pral.ActivityDetails, '') AS NVARCHAR(MAX)) AS SourceDetails,
            pral.CompanyId, pral.BranchId, pral.FinYearId
        FROM dbo.PurchaseReturnActivityLog pral
        WHERE pral.ActivityType = 'DELETE'
          AND NOT EXISTS (SELECT 1 FROM dbo.PReturnMaster prm WHERE prm.PReturnNo = pral.TransactionNo AND prm.CancelFlag = 1)

        UNION ALL

        -- 9. Customer Receipt Master
        SELECT 
            CAST('CUSTOMER_RECEIPT' AS NVARCHAR(50)) AS ModuleKey,
            CAST('Customer Receipt' AS NVARCHAR(100)) AS TransactionType,
            CAST('CR-' + CAST(r.Id AS NVARCHAR(50)) AS NVARCHAR(100)) AS DocNo,
            CAST(ISNULL(r.BillNoUntil, '') AS NVARCHAR(100)) AS RefNo,
            CAST(r.VoucherDate AS DATETIME) AS DocDate,
            CAST(ISNULL(
                (SELECT TOP 1 DATEADD(minute, DATEDIFF(minute, GETUTCDATE(), GETDATE()), sq.CreatedUtc) 
                 FROM dbo.SyncQueue sq 
                 WHERE sq.EntityType = 'CUSTOMER_RECEIPT' AND sq.EntityId = CAST(r.Id AS VARCHAR(50)) AND sq.Operation = 'CANCEL' 
                 ORDER BY sq.SyncId DESC),
                r.VoucherDate
            ) AS DATETIME) AS DeletedDate,
            CAST(ISNULL(l.LedgerName, 'Customer #' + CAST(r.CustomerLedgerId AS NVARCHAR(50))) AS NVARCHAR(200)) AS PartyName,
            CAST(ISNULL(r.ReceiptAmount, 0) AS DECIMAL(18,2)) AS Amount,
            CAST(0 AS DECIMAL(18,2)) AS Qty,
            CAST(ISNULL(u.UserName, 'admin') AS NVARCHAR(100)) AS DeletedBy,
            CAST('' AS NVARCHAR(100)) AS Counter,
            CAST(ISNULL(r.Narration, 'Customer Receipt Cancelled') AS NVARCHAR(500)) AS DeletionReason,
            CAST(ISNULL(r.Narration, '') AS NVARCHAR(MAX)) AS SourceDetails,
            r.CompanyId, r.BranchId, 1 AS FinYearId
        FROM dbo.CustomerReceiptMaster r
        LEFT JOIN dbo.LedgerMaster l ON r.CustomerLedgerId = l.LedgerID
        LEFT JOIN dbo.Users u ON r.UserId = u.UserId
        WHERE r.CancelFlag = 1

        UNION ALL

        -- 10. Vendor Payment Master
        SELECT 
            CAST('VENDOR_PAYMENT' AS NVARCHAR(50)) AS ModuleKey,
            CAST('Vendor Payment' AS NVARCHAR(100)) AS TransactionType,
            CAST('VP-' + CAST(p.Id AS NVARCHAR(50)) AS NVARCHAR(100)) AS DocNo,
            CAST(ISNULL(p.BillNoUntil, '') AS NVARCHAR(100)) AS RefNo,
            CAST(p.VoucherDate AS DATETIME) AS DocDate,
            CAST(ISNULL(
                (SELECT TOP 1 DATEADD(minute, DATEDIFF(minute, GETUTCDATE(), GETDATE()), sq.CreatedUtc) 
                 FROM dbo.SyncQueue sq 
                 WHERE sq.EntityType = 'VENDOR_PAYMENT' AND sq.EntityId = CAST(p.Id AS VARCHAR(50)) AND sq.Operation = 'CANCEL' 
                 ORDER BY sq.SyncId DESC),
                p.VoucherDate
            ) AS DATETIME) AS DeletedDate,
            CAST(ISNULL(l.LedgerName, 'Vendor #' + CAST(p.VendorLedgerId AS NVARCHAR(50))) AS NVARCHAR(200)) AS PartyName,
            CAST(ISNULL(p.PaymentAmount, 0) AS DECIMAL(18,2)) AS Amount,
            CAST(0 AS DECIMAL(18,2)) AS Qty,
            CAST(ISNULL(u.UserName, 'admin') AS NVARCHAR(100)) AS DeletedBy,
            CAST('' AS NVARCHAR(100)) AS Counter,
            CAST(ISNULL(p.Narration, 'Vendor Payment Cancelled') AS NVARCHAR(500)) AS DeletionReason,
            CAST(ISNULL(p.Narration, '') AS NVARCHAR(MAX)) AS SourceDetails,
            p.CompanyId, p.BranchId, 1 AS FinYearId
        FROM dbo.VendorPaymentMaster p
        LEFT JOIN dbo.LedgerMaster l ON p.VendorLedgerId = l.LedgerID
        LEFT JOIN dbo.Users u ON p.UserId = u.UserId
        WHERE p.CancelFlag = 1

        UNION ALL

        -- 11. Credit Note Master
        SELECT 
            CAST('CREDIT_NOTE' AS NVARCHAR(50)) AS ModuleKey,
            CAST('Credit Note' AS NVARCHAR(100)) AS TransactionType,
            CAST('CN-' + CAST(cn.Id AS NVARCHAR(50)) AS NVARCHAR(100)) AS DocNo,
            CAST(ISNULL(cn.InvoiceNo, '') AS NVARCHAR(100)) AS RefNo,
            CAST(cn.VoucherDate AS DATETIME) AS DocDate,
            CAST(ISNULL(
                (SELECT TOP 1 DATEADD(minute, DATEDIFF(minute, GETUTCDATE(), GETDATE()), sq.CreatedUtc) 
                 FROM dbo.SyncQueue sq 
                 WHERE sq.EntityType = 'CREDIT_NOTE' AND sq.EntityId = CAST(cn.Id AS VARCHAR(50)) AND sq.Operation = 'CANCEL' 
                 ORDER BY sq.SyncId DESC),
                ISNULL(cn.CreatedDate, cn.VoucherDate)
            ) AS DATETIME) AS DeletedDate,
            CAST(ISNULL(l.LedgerName, 'Customer #' + CAST(cn.CustomerLedgerId AS NVARCHAR(50))) AS NVARCHAR(200)) AS PartyName,
            CAST(ISNULL(cn.CreditAmount, 0) AS DECIMAL(18,2)) AS Amount,
            CAST(0 AS DECIMAL(18,2)) AS Qty,
            CAST(ISNULL(u.UserName, 'admin') AS NVARCHAR(100)) AS DeletedBy,
            CAST('' AS NVARCHAR(100)) AS Counter,
            CAST(ISNULL(cn.Narration, 'Credit Note Cancelled') AS NVARCHAR(500)) AS DeletionReason,
            CAST(ISNULL(cn.Narration, '') AS NVARCHAR(MAX)) AS SourceDetails,
            cn.CompanyId, cn.BranchId, cn.FinYearId
        FROM dbo.CreditNoteMaster cn
        LEFT JOIN dbo.LedgerMaster l ON cn.CustomerLedgerId = l.LedgerID
        LEFT JOIN dbo.Users u ON cn.UserId = u.UserId
        WHERE cn.CancelFlag = 1

        UNION ALL

        -- 12. Debit Note Master
        SELECT 
            CAST('DEBIT_NOTE' AS NVARCHAR(50)) AS ModuleKey,
            CAST('Debit Note' AS NVARCHAR(100)) AS TransactionType,
            CAST('DN-' + CAST(dn.Id AS NVARCHAR(50)) AS NVARCHAR(100)) AS DocNo,
            CAST(ISNULL(dn.InvoiceNo, '') AS NVARCHAR(100)) AS RefNo,
            CAST(dn.VoucherDate AS DATETIME) AS DocDate,
            CAST(ISNULL(
                (SELECT TOP 1 DATEADD(minute, DATEDIFF(minute, GETUTCDATE(), GETDATE()), sq.CreatedUtc) 
                 FROM dbo.SyncQueue sq 
                 WHERE sq.EntityType = 'DEBIT_NOTE' AND sq.EntityId = CAST(dn.Id AS VARCHAR(50)) AND sq.Operation = 'CANCEL' 
                 ORDER BY sq.SyncId DESC),
                dn.VoucherDate
            ) AS DATETIME) AS DeletedDate,
            CAST(ISNULL(l.LedgerName, 'Vendor #' + CAST(dn.VendorLedgerId AS NVARCHAR(50))) AS NVARCHAR(200)) AS PartyName,
            CAST(ISNULL(dn.DebitAmount, 0) AS DECIMAL(18,2)) AS Amount,
            CAST(0 AS DECIMAL(18,2)) AS Qty,
            CAST(ISNULL(u.UserName, 'admin') AS NVARCHAR(100)) AS DeletedBy,
            CAST('' AS NVARCHAR(100)) AS Counter,
            CAST(ISNULL(dn.Narration, 'Debit Note Cancelled') AS NVARCHAR(500)) AS DeletionReason,
            CAST(ISNULL(dn.Narration, '') AS NVARCHAR(MAX)) AS SourceDetails,
            dn.CompanyId, dn.BranchId, dn.FinYearId
        FROM dbo.DebitNoteMaster dn
        LEFT JOIN dbo.LedgerMaster l ON dn.VendorLedgerId = l.LedgerID
        LEFT JOIN dbo.Users u ON dn.UserId = u.UserId
        WHERE dn.CancelFlag = 1

        UNION ALL

        -- 13. Stock Adjustment Master
        SELECT 
            CAST('STOCK_ADJUSTMENT' AS NVARCHAR(50)) AS ModuleKey,
            CAST('Stock Adjustment' AS NVARCHAR(100)) AS TransactionType,
            CAST('ADJ-' + CAST(sa.StockAdjustmentNo AS NVARCHAR(50)) AS NVARCHAR(100)) AS DocNo,
            CAST('' AS NVARCHAR(100)) AS RefNo,
            CAST(sa.StockAdjustmentDate AS DATETIME) AS DocDate,
            CAST(ISNULL(
                (SELECT TOP 1 saal.CreatedOn 
                 FROM dbo.StockAdjustmentActivityLog saal 
                 WHERE saal.TransactionNo = sa.StockAdjustmentNo AND saal.ActivityType = 'DELETE' 
                 ORDER BY saal.CreatedOn DESC),
                sa.StockAdjustmentDate
            ) AS DATETIME) AS DeletedDate,
            CAST(ISNULL(l.LedgerName, 'Reason #' + CAST(sa.LedgerId AS NVARCHAR(50))) AS NVARCHAR(200)) AS PartyName,
            CAST(0 AS DECIMAL(18,2)) AS Amount,
            CAST(0 AS DECIMAL(18,2)) AS Qty,
            CAST(ISNULL(
                (SELECT TOP 1 saal.UserName 
                 FROM dbo.StockAdjustmentActivityLog saal 
                 WHERE saal.TransactionNo = sa.StockAdjustmentNo AND saal.ActivityType = 'DELETE' 
                 ORDER BY saal.CreatedOn DESC),
                ISNULL(u.UserName, 'admin')
            ) AS NVARCHAR(100)) AS DeletedBy,
            CAST(ISNULL(
                (SELECT TOP 1 saal.CounterName 
                 FROM dbo.StockAdjustmentActivityLog saal 
                 WHERE saal.TransactionNo = sa.StockAdjustmentNo AND saal.ActivityType = 'DELETE' 
                 ORDER BY saal.CreatedOn DESC),
                ''
            ) AS NVARCHAR(100)) AS Counter,
            CAST(ISNULL(sa.Comments, 'Stock Adjustment Cancelled') AS NVARCHAR(500)) AS DeletionReason,
            CAST(ISNULL(sa.Comments, '') AS NVARCHAR(MAX)) AS SourceDetails,
            sa.CompanyId, sa.BranchId, sa.FinYearId
        FROM dbo.StockAdjustmentMaster sa
        LEFT JOIN dbo.LedgerMaster l ON sa.LedgerId = l.LedgerID
        LEFT JOIN dbo.Users u ON sa.UserId = u.UserId
        WHERE sa.CancelFlag = 1

        UNION ALL

        -- 14. Stock Adjustment Activity Log Deletions
        SELECT 
            CAST('STOCK_ADJUSTMENT' AS NVARCHAR(50)) AS ModuleKey,
            CAST('Stock Adjustment' AS NVARCHAR(100)) AS TransactionType,
            CAST('ADJ-' + CAST(saal.TransactionNo AS NVARCHAR(50)) AS NVARCHAR(100)) AS DocNo,
            CAST(ISNULL(saal.InvoiceNo, '') AS NVARCHAR(100)) AS RefNo,
            CAST(saal.CreatedOn AS DATETIME) AS DocDate,
            CAST(saal.CreatedOn AS DATETIME) AS DeletedDate,
            CAST(ISNULL(NULLIF(saal.PartyName, ''), 'Adjustment') AS NVARCHAR(200)) AS PartyName,
            CAST(ISNULL(saal.NetAmount, 0) AS DECIMAL(18,2)) AS Amount,
            CAST(ISNULL(saal.Qty, 0) AS DECIMAL(18,2)) AS Qty,
            CAST(ISNULL(saal.UserName, 'admin') AS NVARCHAR(100)) AS DeletedBy,
            CAST(ISNULL(saal.CounterName, '') AS NVARCHAR(100)) AS Counter,
            CAST('Deleted via Stock Adjustment screen' AS NVARCHAR(500)) AS DeletionReason,
            CAST(ISNULL(saal.ActivityDetails, '') AS NVARCHAR(MAX)) AS SourceDetails,
            saal.CompanyId, saal.BranchId, saal.FinYearId
        FROM dbo.StockAdjustmentActivityLog saal
        WHERE saal.ActivityType = 'DELETE'
          AND NOT EXISTS (SELECT 1 FROM dbo.StockAdjustmentMaster sam WHERE sam.StockAdjustmentNo = saal.TransactionNo AND sam.CancelFlag = 1)

        UNION ALL

        -- 15. Hold Bills Master (SMasterHold)
        SELECT 
            CAST('HOLD_BILL' AS NVARCHAR(50)) AS ModuleKey,
            CAST('Hold Bill' AS NVARCHAR(100)) AS TransactionType,
            CAST('HOLD-' + CAST(h.BillNo AS NVARCHAR(50)) AS NVARCHAR(100)) AS DocNo,
            CAST('' AS NVARCHAR(100)) AS RefNo,
            CAST(h.BillDate AS DATETIME) AS DocDate,
            CAST(ISNULL(
                (SELECT TOP 1 sal.CreatedOn 
                 FROM dbo.SalesActivityLog sal 
                 WHERE sal.TransactionNo = h.BillNo AND (sal.ActivityType = 'DELETE' OR sal.ActivityDetails LIKE '%Hold%') 
                 ORDER BY sal.CreatedOn DESC),
                h.BillDate
            ) AS DATETIME) AS DeletedDate,
            CAST(ISNULL(NULLIF(h.CustomerName, ''), 'Customer') AS NVARCHAR(200)) AS PartyName,
            CAST(ISNULL(h.NetAmount, 0) AS DECIMAL(18,2)) AS Amount,
            CAST(0 AS DECIMAL(18,2)) AS Qty,
            CAST('admin' AS NVARCHAR(100)) AS DeletedBy,
            CAST('COUNTER' + CAST(h.CounterId AS NVARCHAR(50)) AS NVARCHAR(100)) AS Counter,
            CAST('Hold Bill Cancelled / Deleted' AS NVARCHAR(500)) AS DeletionReason,
            CAST('Hold Bill #' + CAST(h.BillNo AS NVARCHAR(50)) AS NVARCHAR(MAX)) AS SourceDetails,
            h.CompanyId, h.BranchId, h.FinYearId
        FROM dbo.SMasterHold h
        WHERE h.CancelFlag = 1

        UNION ALL

        -- 16. General Vouchers (Payment, Receipt, Journal, Contra)
        SELECT 
            CAST(CASE 
                WHEN v.VoucherType LIKE '%Payment%' THEN 'GENERAL_PAYMENT'
                WHEN v.VoucherType LIKE '%Receipt%' THEN 'GENERAL_RECEIPT'
                WHEN v.VoucherType LIKE '%Journal%' THEN 'JOURNAL_VOUCHER'
                WHEN v.VoucherType LIKE '%Contra%' THEN 'CONTRA_VOUCHER'
                ELSE 'ACCOUNTS_VOUCHER'
            END AS NVARCHAR(50)) AS ModuleKey,
            CAST(v.VoucherType AS NVARCHAR(100)) AS TransactionType,
            CAST(v.VoucherType + ' #' + CAST(v.VoucherID AS NVARCHAR(50)) AS NVARCHAR(100)) AS DocNo,
            CAST(ISNULL(v.VoucherNumber, '') AS NVARCHAR(100)) AS RefNo,
            CAST(v.VoucherDate AS DATETIME) AS DocDate,
            CAST(ISNULL(v.UserDate, v.VoucherDate) AS DATETIME) AS DeletedDate,
            CAST(ISNULL(v.LedgerName, '') AS NVARCHAR(200)) AS PartyName,
            CAST(CASE WHEN ISNULL(v.Debit, 0) > 0 THEN v.Debit ELSE ISNULL(v.Credit, 0) END AS DECIMAL(18,2)) AS Amount,
            CAST(0 AS DECIMAL(18,2)) AS Qty,
            CAST(ISNULL(v.UserName, 'admin') AS NVARCHAR(100)) AS DeletedBy,
            CAST(ISNULL(CAST(v.CounterID AS NVARCHAR(50)), '') AS NVARCHAR(100)) AS Counter,
            CAST(ISNULL(v.Narration, 'Voucher Cancelled') AS NVARCHAR(500)) AS DeletionReason,
            CAST(ISNULL(v.Narration, '') AS NVARCHAR(MAX)) AS SourceDetails,
            v.CompanyID AS CompanyId, v.BranchID AS BranchId, v.FinYearID AS FinYearId
        FROM dbo.Vouchers v
        WHERE v.CancelFlag = 1
          AND v.VoucherType NOT IN ('Sales', 'Purchase', 'Customer Receipt', 'Vendor Payment')

        UNION ALL

        -- 17. Item Deletions from ItemActivityLog
        SELECT 
            CAST('ITEM' AS NVARCHAR(50)) AS ModuleKey,
            CAST('Item Master' AS NVARCHAR(100)) AS TransactionType,
            CAST('ITEM-' + CAST(ial.ItemId AS NVARCHAR(50)) AS NVARCHAR(100)) AS DocNo,
            CAST(ISNULL(ial.Barcode, '') AS NVARCHAR(100)) AS RefNo,
            CAST(ial.CreatedOn AS DATETIME) AS DocDate,
            CAST(ial.CreatedOn AS DATETIME) AS DeletedDate,
            CAST(ISNULL(ial.ItemName, '') AS NVARCHAR(200)) AS PartyName,
            CAST(ISNULL(ial.UnitCost, ISNULL(ial.RetailPrice, 0)) AS DECIMAL(18,2)) AS Amount,
            CAST(ISNULL(ial.Quantity, 0) AS DECIMAL(18,2)) AS Qty,
            CAST(ISNULL(ial.UserName, 'admin') AS NVARCHAR(100)) AS DeletedBy,
            CAST(ISNULL(ial.CounterName, '') AS NVARCHAR(100)) AS Counter,
            CAST('Item deleted from item master' AS NVARCHAR(500)) AS DeletionReason,
            CAST(ISNULL(ial.ActivityDetails, '') AS NVARCHAR(MAX)) AS SourceDetails,
            ial.CompanyId, ial.BranchId, ial.FinYearId
        FROM dbo.ItemActivityLog ial
        WHERE ial.ActivityType = 'DELETE'

        UNION ALL

        -- 18. Other System & Master Deletions from UserActivityLog (Ledgers, Accounts, etc.)
        SELECT 
            CAST(CASE 
                WHEN ual.FormName LIKE '%Ledger%' OR ual.ActivityDetails LIKE '%Ledger%' THEN 'LEDGER'
                WHEN ual.FormName LIKE '%Account%' OR ual.ActivityDetails LIKE '%Account%' THEN 'ACCOUNTS'
                WHEN ual.FormName LIKE '%Item%' OR ual.ActivityDetails LIKE '%Item%' THEN 'ITEM'
                WHEN ual.FormName LIKE '%Hold%' OR ual.ActivityDetails LIKE '%Hold%' THEN 'HOLD_BILL'
                ELSE 'MASTER_SYSTEM'
            END AS NVARCHAR(50)) AS ModuleKey,
            CAST(CASE 
                WHEN ual.FormName LIKE '%Ledger%' OR ual.ActivityDetails LIKE '%Ledger%' THEN 'Ledger Master'
                WHEN ual.FormName LIKE '%Account%' OR ual.ActivityDetails LIKE '%Account%' THEN 'Account Group'
                WHEN ual.FormName LIKE '%Item%' OR ual.ActivityDetails LIKE '%Item%' THEN 'Item Master'
                WHEN ual.FormName LIKE '%Hold%' OR ual.ActivityDetails LIKE '%Hold%' THEN 'Hold Bill'
                ELSE 'System Audit'
            END AS NVARCHAR(100)) AS TransactionType,
            CAST('LOG-' + CAST(ual.UserActivityLogId AS NVARCHAR(50)) AS NVARCHAR(100)) AS DocNo,
            CAST(ISNULL(ual.FormName, '') AS NVARCHAR(100)) AS RefNo,
            CAST(ual.CreatedOn AS DATETIME) AS DocDate,
            CAST(ual.CreatedOn AS DATETIME) AS DeletedDate,
            CAST(ISNULL(ual.FormName, 'System') AS NVARCHAR(200)) AS PartyName,
            CAST(0 AS DECIMAL(18,2)) AS Amount,
            CAST(0 AS DECIMAL(18,2)) AS Qty,
            CAST(ISNULL(ual.UserName, 'admin') AS NVARCHAR(100)) AS DeletedBy,
            CAST(ISNULL(ual.CounterName, '') AS NVARCHAR(100)) AS Counter,
            CAST(ISNULL(ual.ActivityDetails, 'Deleted') AS NVARCHAR(500)) AS DeletionReason,
            CAST(ISNULL(ual.ActivityDetails, '') AS NVARCHAR(MAX)) AS SourceDetails,
            ual.CompanyId, ual.BranchId, ual.FinYearId
        FROM dbo.UserActivityLog ual
        WHERE ual.ActivityType = 'DELETE'
          AND ual.FormName NOT IN ('frmSalesInvoice', 'FrmPurchase', 'frmSalesReturn', 'frmPurchaseReturn', 'frmStockAdjustment')
    )
    SELECT 
        ROW_NUMBER() OVER (ORDER BY DeletedDate DESC, DocNo DESC) AS SlNo,
        ModuleKey,
        TransactionType,
        DocNo,
        RefNo,
        DocDate,
        DeletedDate,
        PartyName,
        Amount,
        Qty,
        DeletedBy,
        Counter,
        DeletionReason,
        SourceDetails
    FROM AllDeletions
    WHERE (@CompanyId = 0 OR CompanyId = 0 OR CompanyId = @CompanyId)
      AND (@BranchId = 0 OR BranchId = 0 OR BranchId = @BranchId)
      AND (@FinYearId = 0 OR FinYearId = 0 OR FinYearId = @FinYearId)
      AND (DeletedDate >= @RangeFrom AND DeletedDate < @RangeToExclusive)
      AND (@TransactionType = 'ALL' OR ModuleKey = @TransactionType OR UPPER(TransactionType) = @TransactionType)
      AND (@UserFilter = 'ALL' OR UPPER(DeletedBy) = UPPER(@UserFilter))
      AND (
          @SearchText = '' OR 
          DocNo LIKE '%' + @SearchText + '%' OR 
          RefNo LIKE '%' + @SearchText + '%' OR 
          PartyName LIKE '%' + @SearchText + '%' OR 
          Counter LIKE '%' + @SearchText + '%' OR 
          DeletedBy LIKE '%' + @SearchText + '%' OR 
          DeletionReason LIKE '%' + @SearchText + '%' OR 
          SourceDetails LIKE '%' + @SearchText + '%'
      )
    ORDER BY DeletedDate DESC, DocNo DESC;
END
GO
