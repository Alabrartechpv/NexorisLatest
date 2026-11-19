-- ====================================================================================
-- NEXORIS POS - DATE-WISE REPORTS COMPATIBILITY & BUG FIX PATCH
-- Fixes: "Invalid object name 'dbo.branch'" on client computers
-- Target: Any client database (Malaysia, Singapore, India, etc.)
-- Safe & Idempotent: Can be executed multiple times safely.
-- ====================================================================================

SET NOCOUNT ON;
GO

PRINT '>>> Step 1: Ensuring [dbo].[Branch] compatibility view exists on client database...';
GO

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Branch]') AND type IN (N'U', N'V', N'SN'))
BEGIN
    IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Branches]') AND type IN (N'U', N'V'))
    BEGIN
        EXEC('CREATE VIEW dbo.Branch AS SELECT * FROM dbo.Branches;');
        PRINT '    -> [dbo].[Branch] compatibility view created successfully pointing to [dbo].[Branches].';
    END
    ELSE
    BEGIN
        PRINT '    -> Note: Neither [dbo].[Branch] nor [dbo].[Branches] table found.';
    END
END
ELSE
BEGIN
    PRINT '    -> [dbo].[Branch] object already exists.';
END
GO

PRINT '>>> Step 2: Deploying POS_DatewiseCustomerPaymentReport...';
GO
-- =========================================================================================================
-- Stored Procedure : POS_DatewiseCustomerPaymentReport
-- Description      : Dedicated high-performance stored procedure for Date-wise Customer Payment / Receipt Report
--                    Supports Date Matrix, Detailed, Daily Customer Summary, and Customer Total Summary.
--                    Provides 3-level hierarchical datasets:
--                      Level 0: Customer Matrix (Customers vs Dates with totals & outstanding)
--                      Level 1: Customer Invoices & Receipt Status (matching FrmReceipt.cs)
--                      Level 2: Receipt History & Running Balance Breakdown (matching FrmViewReceipt.cs)
-- =========================================================================================================
IF OBJECT_ID('dbo.POS_DatewiseCustomerPaymentReport', 'P') IS NOT NULL
    DROP PROCEDURE dbo.POS_DatewiseCustomerPaymentReport;
GO

CREATE PROCEDURE dbo.POS_DatewiseCustomerPaymentReport
(
    @PeriodMode         VARCHAR(20)     = 'DATE_RANGE', -- 'DATE_RANGE', 'MONTH_WISE', 'YEAR_WISE', 'ALL'
    @FromDate           DATETIME        = NULL,
    @ToDate             DATETIME        = NULL,
    @Month              INT             = 0,            -- 1..12 (0 = ALL)
    @Year               INT             = 0,            -- e.g. 2026 (0 = current/all)
    @CompanyId          INT             = 0,
    @BranchId           INT             = 0,
    @FinYearId          INT             = 0,
    @CustomerLedgerId   INT             = 0,            -- 0 = ALL Customers
    @ReceiptType        VARCHAR(50)     = 'ALL',        -- 'ALL', 'Customer Receipt', 'Credit Note'
    @PaymentModeFilter  VARCHAR(50)     = 'ALL',        -- 'ALL', 'CASH', 'BANK TRANSFER', etc.
    @Status             VARCHAR(20)     = 'Active',     -- 'Active', 'Cancel', 'ALL'
    @SearchText         VARCHAR(100)    = '',
    @ViewMode           VARCHAR(30)     = 'DATE_MATRIX' -- 'DATE_MATRIX', 'DETAILED', 'DATE_CUSTOMER_SUMMARY', 'CUSTOMER_SUMMARY'
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Defaults
    IF (@FromDate IS NULL OR @FromDate < '1900-01-01')
        SET @FromDate = DATEADD(DAY, 1 - DAY(GETDATE()), CAST(GETDATE() AS DATE)); -- 1st of current month
    IF (@ToDate IS NULL OR @ToDate < '1900-01-01')
        SET @ToDate = CAST(GETDATE() AS DATE);

    SET @FromDate = CAST(@FromDate AS DATE);
    SET @ToDate = CAST(@ToDate AS DATE);
    SET @SearchText = LTRIM(RTRIM(ISNULL(@SearchText, '')));

    -- 1. Precalculate Customer Outstanding Balances from General Ledger (Debit - Credit + OpnDebit - OpnCredit)
    IF OBJECT_ID('tempdb..#CustomerBalances') IS NOT NULL DROP TABLE #CustomerBalances;
    CREATE TABLE #CustomerBalances
    (
        LedgerID            INT PRIMARY KEY,
        OutstandingBalance  DECIMAL(18,2)
    );

    INSERT INTO #CustomerBalances (LedgerID, OutstandingBalance)
    SELECT 
        LM.LedgerID,
        CAST(SUM(ISNULL(V.Debit, 0)) + ISNULL(LM.OpnDebit, 0) - SUM(ISNULL(V.Credit, 0)) - ISNULL(LM.OpnCredit, 0) AS DECIMAL(18,2)) AS OutstandingBalance
    FROM dbo.LedgerMaster LM
    LEFT JOIN dbo.Vouchers V 
        ON V.LedgerID = LM.LedgerID 
       AND (@BranchId = 0 OR V.BranchID = @BranchId)
       AND V.CancelFlag = 0
    WHERE (@CustomerLedgerId = 0 OR LM.LedgerID = @CustomerLedgerId)
    GROUP BY LM.LedgerID, LM.OpnDebit, LM.OpnCredit;

    -- 2. Build Raw Filtered Receipts Table
    IF OBJECT_ID('tempdb..#FilteredReceipts') IS NOT NULL DROP TABLE #FilteredReceipts;
    CREATE TABLE #FilteredReceipts
    (
        ReceiptMasterId             INT,
        BranchId                    INT,
        BranchName                  VARCHAR(150),
        ReceiptType                 VARCHAR(50),
        VoucherId                   INT,
        VoucherNo                   VARCHAR(50),
        VoucherDate                 DATETIME,
        CustomerLedgerId            INT,
        CustomerName                VARCHAR(250),
        BillNo                      BIGINT,
        InvoiceNo                   VARCHAR(50),
        BillDate                    DATETIME,
        BillAmount                  DECIMAL(18,2),
        TotalBillReceived           DECIMAL(18,2),
        ReturnedAmount              DECIMAL(18,2),
        BillBalance                 DECIMAL(18,2),
        ReceiptAmount               DECIMAL(18,2),
        MasterReceiptAmount         DECIMAL(18,2),
        BalanceAmount               DECIMAL(18,2),
        PaymentMode                 VARCHAR(50),
        Narration                   NVARCHAR(1000),
        PaymentReference            VARCHAR(100),
        Status                      VARCHAR(20),
        CurrentCustomerOutstanding  DECIMAL(18,2)
    );

    -- 2A. Insert allocated Customer Receipts
    INSERT INTO #FilteredReceipts
    (
        ReceiptMasterId,
        BranchId,
        BranchName,
        ReceiptType,
        VoucherId,
        VoucherNo,
        VoucherDate,
        CustomerLedgerId,
        CustomerName,
        BillNo,
        InvoiceNo,
        BillDate,
        BillAmount,
        TotalBillReceived,
        ReturnedAmount,
        BillBalance,
        ReceiptAmount,
        MasterReceiptAmount,
        BalanceAmount,
        PaymentMode,
        Narration,
        PaymentReference,
        Status,
        CurrentCustomerOutstanding
    )
    SELECT 
        CRM.Id AS ReceiptMasterId,
        CRM.BranchId,
        ISNULL(B.BranchName, '') AS BranchName,
        'Customer Receipt' AS ReceiptType,
        ISNULL(CRM.VoucherId, 0) AS VoucherId,
        CAST(ISNULL(CRM.VoucherId, 0) AS VARCHAR(50)) AS VoucherNo,
        CRM.VoucherDate,
        CRM.CustomerLedgerId,
        ISNULL(LM.LedgerName, 'Unknown Customer') AS CustomerName,
        ISNULL(CRD.BillNo, 0) AS BillNo,
        ISNULL(CAST(CRD.BillNo AS VARCHAR(50)), '-') AS InvoiceNo,
        ISNULL(CRD.BillDate, SM.BillDate) AS BillDate,
        CAST(ISNULL(CRD.BillAmount, ISNULL(SM.NetAmount, 0)) AS DECIMAL(18,2)) AS BillAmount,
        CAST(ISNULL(SM.ReceivedAmount, 0) AS DECIMAL(18,2)) AS TotalBillReceived,
        CAST(ISNULL(SR.ReturnedAmount, 0) AS DECIMAL(18,2)) AS ReturnedAmount,
        CAST(CASE 
            WHEN (ISNULL(SM.NetAmount, ISNULL(CRD.BillAmount, 0)) - ISNULL(SM.ReceivedAmount, 0) - ISNULL(SR.ReturnedAmount, 0)) < 0 THEN 0
            ELSE (ISNULL(SM.NetAmount, ISNULL(CRD.BillAmount, 0)) - ISNULL(SM.ReceivedAmount, 0) - ISNULL(SR.ReturnedAmount, 0))
        END AS DECIMAL(18,2)) AS BillBalance,
        CAST(ISNULL(CRD.ReceiptAmount, 0) AS DECIMAL(18,2)) AS ReceiptAmount,
        CAST(ISNULL(CRM.ReceiptAmount, 0) AS DECIMAL(18,2)) AS MasterReceiptAmount,
        CAST(ISNULL(CRD.BalanceAmount, 0) AS DECIMAL(18,2)) AS BalanceAmount,
        ISNULL(PayMode.PayModeName, 'CASH') AS PaymentMode,
        ISNULL(CRM.Narration, '') AS Narration,
        CAST(ISNULL(CRM.VoucherId, 0) AS VARCHAR(50)) AS PaymentReference,
        CASE WHEN ISNULL(CRM.CancelFlag, 0) = 1 THEN 'Cancel' ELSE 'Active' END AS Status,
        ISNULL(CB.OutstandingBalance, 0) AS CurrentCustomerOutstanding
    FROM dbo.CustomerReceiptDetails CRD
    INNER JOIN dbo.CustomerReceiptMaster CRM 
        ON CRM.Id = CRD.ReceiptMasterId
    LEFT JOIN dbo.SMaster SM 
        ON SM.BillNo = CRD.BillNo 
       AND (@BranchId = 0 OR SM.BranchId = @BranchId)
       AND SM.CancelFlag = 0
    OUTER APPLY (
        SELECT SUM(SRM.GrandTotal) AS ReturnedAmount
        FROM dbo.SReturnMaster SRM
        WHERE SRM.InvoiceNo = CAST(CRD.BillNo AS VARCHAR(50))
          AND (@BranchId = 0 OR SRM.BranchId = @BranchId)
          AND SRM.CancelFlag = 0
    ) SR
    LEFT JOIN dbo.LedgerMaster LM 
        ON LM.LedgerID = CRM.CustomerLedgerId
    LEFT JOIN dbo.Branches B WITH (NOLOCK)
        ON B.Id = CRM.BranchId
    LEFT JOIN dbo.PayMode PayMode 
        ON PayMode.PayModeID = CRM.PaymentMethodLedgerId
    LEFT JOIN #CustomerBalances CB 
        ON CB.LedgerID = CRM.CustomerLedgerId
    WHERE (@Status = 'ALL' OR ISNULL(CRM.CancelFlag, 0) = CASE WHEN @Status = 'Cancel' THEN 1 ELSE 0 END)
      AND (@CompanyId = 0 OR ISNULL(CRM.CompanyId, 0) = @CompanyId)
      AND (@BranchId = 0 OR ISNULL(CRM.BranchId, 0) = @BranchId)
      AND (@CustomerLedgerId = 0 OR ISNULL(CRM.CustomerLedgerId, 0) = @CustomerLedgerId)
      AND ISNULL(CRD.CancelFlag, 0) = 0;

    -- 2B. Insert unallocated Customer Receipts (receipts without details rows)
    INSERT INTO #FilteredReceipts
    (
        ReceiptMasterId,
        BranchId,
        BranchName,
        ReceiptType,
        VoucherId,
        VoucherNo,
        VoucherDate,
        CustomerLedgerId,
        CustomerName,
        BillNo,
        InvoiceNo,
        BillDate,
        BillAmount,
        TotalBillReceived,
        ReturnedAmount,
        BillBalance,
        ReceiptAmount,
        MasterReceiptAmount,
        BalanceAmount,
        PaymentMode,
        Narration,
        PaymentReference,
        Status,
        CurrentCustomerOutstanding
    )
    SELECT 
        CRM.Id AS ReceiptMasterId,
        CRM.BranchId,
        ISNULL(B.BranchName, '') AS BranchName,
        'Customer Receipt' AS ReceiptType,
        ISNULL(CRM.VoucherId, 0) AS VoucherId,
        CAST(ISNULL(CRM.VoucherId, 0) AS VARCHAR(50)) AS VoucherNo,
        CRM.VoucherDate,
        CRM.CustomerLedgerId,
        ISNULL(LM.LedgerName, 'Unknown Customer') AS CustomerName,
        0 AS BillNo,
        '-' AS InvoiceNo,
        CRM.VoucherDate AS BillDate,
        0 AS BillAmount,
        0 AS TotalBillReceived,
        0 AS ReturnedAmount,
        0 AS BillBalance,
        CAST(ISNULL(CRM.ReceiptAmount, 0) AS DECIMAL(18,2)) AS ReceiptAmount,
        CAST(ISNULL(CRM.ReceiptAmount, 0) AS DECIMAL(18,2)) AS MasterReceiptAmount,
        0 AS BalanceAmount,
        ISNULL(PayMode.PayModeName, 'CASH') AS PaymentMode,
        ISNULL(CRM.Narration, '') AS Narration,
        CAST(ISNULL(CRM.VoucherId, 0) AS VARCHAR(50)) AS PaymentReference,
        CASE WHEN ISNULL(CRM.CancelFlag, 0) = 1 THEN 'Cancel' ELSE 'Active' END AS Status,
        ISNULL(CB.OutstandingBalance, 0) AS CurrentCustomerOutstanding
    FROM dbo.CustomerReceiptMaster CRM
    LEFT JOIN dbo.LedgerMaster LM 
        ON LM.LedgerID = CRM.CustomerLedgerId
    LEFT JOIN dbo.Branches B WITH (NOLOCK)
        ON B.Id = CRM.BranchId
    LEFT JOIN dbo.PayMode PayMode 
        ON PayMode.PayModeID = CRM.PaymentMethodLedgerId
    LEFT JOIN #CustomerBalances CB 
        ON CB.LedgerID = CRM.CustomerLedgerId
    WHERE (@Status = 'ALL' OR ISNULL(CRM.CancelFlag, 0) = CASE WHEN @Status = 'Cancel' THEN 1 ELSE 0 END)
      AND (@CompanyId = 0 OR ISNULL(CRM.CompanyId, 0) = @CompanyId)
      AND (@BranchId = 0 OR ISNULL(CRM.BranchId, 0) = @BranchId)
      AND (@CustomerLedgerId = 0 OR ISNULL(CRM.CustomerLedgerId, 0) = @CustomerLedgerId)
      AND NOT EXISTS (
          SELECT 1 FROM dbo.CustomerReceiptDetails CRD2 
          WHERE CRD2.ReceiptMasterId = CRM.Id AND CRD2.CancelFlag = 0
      );

    -- 2C. Insert Credit Note allocations if any
    IF OBJECT_ID('dbo.CreditNoteDetails', 'U') IS NOT NULL AND OBJECT_ID('dbo.CreditNoteMaster', 'U') IS NOT NULL
    BEGIN
        INSERT INTO #FilteredReceipts
        (
            ReceiptMasterId,
            BranchId,
            BranchName,
            ReceiptType,
            VoucherId,
            VoucherNo,
            VoucherDate,
            CustomerLedgerId,
            CustomerName,
            BillNo,
            InvoiceNo,
            BillDate,
            BillAmount,
            TotalBillReceived,
            ReturnedAmount,
            BillBalance,
            ReceiptAmount,
            MasterReceiptAmount,
            BalanceAmount,
            PaymentMode,
            Narration,
            PaymentReference,
            Status,
            CurrentCustomerOutstanding
        )
        SELECT 
            CNM.Id AS ReceiptMasterId,
            CNM.BranchId,
            ISNULL(B.BranchName, '') AS BranchName,
            'Credit Note' AS ReceiptType,
            ISNULL(CNM.VoucherId, 0) AS VoucherId,
            'CN-' + CAST(ISNULL(CNM.VoucherId, 0) AS VARCHAR(50)) AS VoucherNo,
            CNM.VoucherDate,
            CNM.CustomerLedgerId,
            ISNULL(LM.LedgerName, 'Unknown Customer') AS CustomerName,
            ISNULL(CND.BillNo, 0) AS BillNo,
            ISNULL(CAST(CND.BillNo AS VARCHAR(50)), '-') AS InvoiceNo,
            ISNULL(CND.BillDate, SM.BillDate) AS BillDate,
            CAST(ISNULL(CND.BillAmount, ISNULL(SM.NetAmount, 0)) AS DECIMAL(18,2)) AS BillAmount,
            CAST(ISNULL(SM.ReceivedAmount, 0) AS DECIMAL(18,2)) AS TotalBillReceived,
            CAST(ISNULL(SR.ReturnedAmount, 0) AS DECIMAL(18,2)) AS ReturnedAmount,
            CAST(ISNULL(CND.BalanceAmount, 0) AS DECIMAL(18,2)) AS BillBalance,
            CAST(ISNULL(CND.CreditAmount, 0) AS DECIMAL(18,2)) AS ReceiptAmount,
            CAST(ISNULL(CNM.CreditAmount, 0) AS DECIMAL(18,2)) AS MasterReceiptAmount,
            CAST(ISNULL(CND.BalanceAmount, 0) AS DECIMAL(18,2)) AS BalanceAmount,
            'Credit Note' AS PaymentMode,
            ISNULL(CNM.Narration, '') AS Narration,
            'CN-' + CAST(ISNULL(CNM.VoucherId, 0) AS VARCHAR(50)) AS PaymentReference,
            CASE WHEN ISNULL(CNM.CancelFlag, 0) = 1 THEN 'Cancel' ELSE 'Active' END AS Status,
            ISNULL(CB.OutstandingBalance, 0) AS CurrentCustomerOutstanding
        FROM dbo.CreditNoteDetails CND
        INNER JOIN dbo.CreditNoteMaster CNM 
            ON CNM.Id = CND.CreditNoteMasterId
        LEFT JOIN dbo.SMaster SM 
            ON SM.BillNo = CND.BillNo 
           AND (@BranchId = 0 OR SM.BranchId = @BranchId)
           AND SM.CancelFlag = 0
        OUTER APPLY (
            SELECT SUM(SRM.GrandTotal) AS ReturnedAmount
            FROM dbo.SReturnMaster SRM
            WHERE SRM.InvoiceNo = CAST(CND.BillNo AS VARCHAR(50))
              AND (@BranchId = 0 OR SRM.BranchId = @BranchId)
              AND SRM.CancelFlag = 0
        ) SR
        LEFT JOIN dbo.LedgerMaster LM 
            ON LM.LedgerID = CNM.CustomerLedgerId
        LEFT JOIN dbo.Branches B WITH (NOLOCK)
            ON B.Id = CNM.BranchId
        LEFT JOIN #CustomerBalances CB 
            ON CB.LedgerID = CNM.CustomerLedgerId
        WHERE (@Status = 'ALL' OR ISNULL(CNM.CancelFlag, 0) = CASE WHEN @Status = 'Cancel' THEN 1 ELSE 0 END)
          AND (@CompanyId = 0 OR ISNULL(CNM.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(CNM.BranchId, 0) = @BranchId)
          AND (@CustomerLedgerId = 0 OR ISNULL(CNM.CustomerLedgerId, 0) = @CustomerLedgerId)
          AND ISNULL(CND.CancelFlag, 0) = 0;
    END

    -- Apply PeriodMode, Mode, and Search criteria in deletion/filtering
    DELETE FROM #FilteredReceipts
    WHERE NOT (
        -- Period Mode
        (
            (@PeriodMode = 'MONTH_WISE' AND 
                (@Year = 0 OR YEAR(VoucherDate) = @Year) AND 
                (@Month = 0 OR MONTH(VoucherDate) = @Month))
            OR
            (@PeriodMode = 'YEAR_WISE' AND 
                (@Year = 0 OR YEAR(VoucherDate) = @Year))
            OR
            (@PeriodMode = 'DATE_RANGE' AND 
                CAST(VoucherDate AS DATE) >= @FromDate AND CAST(VoucherDate AS DATE) <= @ToDate)
            OR
            (@PeriodMode = 'ALL' AND 
                (@FromDate = '1900-01-01' OR CAST(VoucherDate AS DATE) >= @FromDate) AND 
                (@ToDate = '2099-12-31' OR CAST(VoucherDate AS DATE) <= @ToDate))
        )
        -- Receipt Type
        AND (@ReceiptType = 'ALL' OR ReceiptType = @ReceiptType)
        -- Payment Mode
        AND (@PaymentModeFilter = 'ALL' OR PaymentMode = @PaymentModeFilter)
        -- Search Text
        AND (
            @SearchText = '' OR
            CustomerName LIKE '%' + @SearchText + '%' OR
            VoucherNo LIKE '%' + @SearchText + '%' OR
            CAST(BillNo AS VARCHAR(50)) LIKE '%' + @SearchText + '%' OR
            InvoiceNo LIKE '%' + @SearchText + '%' OR
            PaymentMode LIKE '%' + @SearchText + '%' OR
            Narration LIKE '%' + @SearchText + '%' OR
            PaymentReference LIKE '%' + @SearchText + '%'
        )
    );

    -- 3. Return results based on ViewMode
    IF (@ViewMode = 'DATE_CUSTOMER_SUMMARY')
    BEGIN
        SELECT 
            CAST(VoucherDate AS DATE) AS TransactionDate,
            CustomerLedgerId,
            CustomerName,
            COUNT(DISTINCT VoucherId) AS ReceiptCount,
            SUM(ReceiptAmount) AS TotalReceived,
            MAX(CurrentCustomerOutstanding) AS CurrentCustomerOutstanding
        FROM #FilteredReceipts
        GROUP BY CAST(VoucherDate AS DATE), CustomerLedgerId, CustomerName
        ORDER BY CAST(VoucherDate AS DATE) DESC, CustomerName ASC;
    END
    ELSE IF (@ViewMode = 'CUSTOMER_SUMMARY')
    BEGIN
        SELECT 
            CustomerLedgerId,
            CustomerName,
            COUNT(DISTINCT VoucherId) AS ReceiptCount,
            SUM(ReceiptAmount) AS TotalReceived,
            MAX(VoucherDate) AS LastReceiptDate,
            MAX(CurrentCustomerOutstanding) AS CurrentCustomerOutstanding
        FROM #FilteredReceipts
        GROUP BY CustomerLedgerId, CustomerName
        ORDER BY CustomerName ASC;
    END
    ELSE IF (@ViewMode = 'DATE_MATRIX')
    BEGIN
        -- Result Set 0: Receipt transactions within period (used for dynamic date matrix pivot)
        SELECT 
            ReceiptMasterId,
            BranchId,
            BranchName,
            ReceiptType,
            VoucherId,
            VoucherNo,
            VoucherDate,
            CustomerLedgerId,
            CustomerName,
            BillNo,
            InvoiceNo,
            BillDate,
            BillAmount,
            TotalBillReceived,
            ReturnedAmount,
            BillBalance,
            ReceiptAmount,
            MasterReceiptAmount,
            BalanceAmount,
            PaymentMode,
            Narration,
            PaymentReference,
            Status,
            CurrentCustomerOutstanding
        FROM #FilteredReceipts
        ORDER BY VoucherDate DESC, VoucherId DESC, BillNo ASC;

        -- Result Set 1: Customer Invoices matching FrmReceipt.cs (All bills for participating customers)
        SELECT 
            SM.LedgerID AS CustomerLedgerId,
            LM.LedgerName AS CustomerName,
            CAST(SM.LedgerID AS VARCHAR(20)) + '_' + CAST(SM.BillNo AS VARCHAR(50)) AS BillKey,
            SM.BillNo,
            ISNULL(SM.PaymentReference, CAST(SM.BillNo AS VARCHAR(50))) AS InvoiceNo,
            CAST(ISNULL(SM.BillDate, '1900-01-01') AS DATETIME) AS BillDate,
            CAST(ISNULL(SM.NetAmount, 0) AS DECIMAL(18,2)) AS InvoiceAmount,
            CAST(ISNULL(SM.ReceivedAmount, 0) AS DECIMAL(18,2)) AS ReceivedAmount,
            CAST(ISNULL(SR.ReturnedAmount, 0) AS DECIMAL(18,2)) AS ReturnedAmount,
            CAST(CASE 
                WHEN (ISNULL(SM.NetAmount, 0) - ISNULL(SM.ReceivedAmount, 0) - ISNULL(SR.ReturnedAmount, 0)) < 0 THEN 0
                ELSE (ISNULL(SM.NetAmount, 0) - ISNULL(SM.ReceivedAmount, 0) - ISNULL(SR.ReturnedAmount, 0))
            END AS DECIMAL(18,2)) AS Balance,
            CAST(ISNULL(PeriodRcp.ReceivedInPeriod, 0) AS DECIMAL(18,2)) AS ReceivedInPeriod,
            ISNULL(ReceiptList.Receipts, '') AS Receipts,
            ISNULL(PayMode.PayModeName, 'CASH') AS PaymentMethod,
            CASE 
                WHEN (ISNULL(SM.NetAmount, 0) - ISNULL(SM.ReceivedAmount, 0) - ISNULL(SR.ReturnedAmount, 0)) <= 0 THEN 'Fully Paid'
                WHEN ISNULL(SM.ReceivedAmount, 0) > 0 THEN 'Partially Paid'
                ELSE 'Unpaid'
            END AS Status,
            ISNULL(B.BranchName, '') AS BranchName
        FROM dbo.SMaster SM
        LEFT JOIN dbo.LedgerMaster LM ON LM.LedgerID = SM.LedgerID
        LEFT JOIN dbo.Branches B WITH (NOLOCK) ON B.Id = SM.BranchId
        OUTER APPLY (
            SELECT SUM(SRM.GrandTotal) AS ReturnedAmount
            FROM dbo.SReturnMaster SRM
            WHERE SRM.InvoiceNo = CAST(SM.BillNo AS VARCHAR(50))
              AND (@BranchId = 0 OR SRM.BranchId = @BranchId)
              AND SRM.CancelFlag = 0
        ) SR
        OUTER APPLY (
            SELECT SUM(CRD.ReceiptAmount) AS ReceivedInPeriod
            FROM dbo.CustomerReceiptDetails CRD
            INNER JOIN dbo.CustomerReceiptMaster CRM ON CRM.Id = CRD.ReceiptMasterId
            WHERE CRD.BillNo = SM.BillNo
              AND (@BranchId = 0 OR CRD.BranchId = @BranchId)
              AND CRD.CancelFlag = 0
              AND CRM.CancelFlag = 0
              AND CAST(CRM.VoucherDate AS DATE) >= @FromDate
              AND CAST(CRM.VoucherDate AS DATE) <= @ToDate
        ) PeriodRcp
        OUTER APPLY (
            SELECT STUFF((
                SELECT DISTINCT ', #' + CAST(CRM2.VoucherId AS VARCHAR(20))
                FROM dbo.CustomerReceiptDetails CRD2
                INNER JOIN dbo.CustomerReceiptMaster CRM2 ON CRM2.Id = CRD2.ReceiptMasterId
                WHERE CRD2.BillNo = SM.BillNo
                  AND (@BranchId = 0 OR CRD2.BranchId = @BranchId)
                  AND CRD2.CancelFlag = 0
                  AND CRM2.CancelFlag = 0
                FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS Receipts
        ) ReceiptList
        OUTER APPLY (
            SELECT TOP 1 PayMode.PayModeName
            FROM dbo.CustomerReceiptDetails CRD3
            INNER JOIN dbo.CustomerReceiptMaster CRM3 ON CRM3.Id = CRD3.ReceiptMasterId
            LEFT JOIN dbo.PayMode PayMode ON PayMode.PayModeID = CRM3.PaymentMethodLedgerId
            WHERE CRD3.BillNo = SM.BillNo
              AND CRD3.CancelFlag = 0
            ORDER BY CRM3.VoucherDate DESC, CRM3.Id DESC
        ) PayMode
        WHERE SM.LedgerID IN (SELECT DISTINCT CustomerLedgerId FROM #FilteredReceipts)
          AND (@BranchId = 0 OR SM.BranchId = @BranchId)
          AND SM.CancelFlag = 0
        ORDER BY SM.LedgerID ASC, SM.BillNo DESC;

        -- Result Set 2: Receipt payment breakdown per bill (matching FrmViewReceipt.cs)
        SELECT 
            CAST(CRM.CustomerLedgerId AS VARCHAR(20)) + '_' + CAST(CRD.BillNo AS VARCHAR(50)) AS BillKey,
            CRM.CustomerLedgerId,
            CRD.BillNo,
            CRM.VoucherDate,
            ISNULL(CRM.VoucherId, 0) AS VoucherId,
            CAST(ISNULL(CRM.VoucherId, 0) AS VARCHAR(20)) AS VoucherNo,
            CAST(ISNULL(CRD.ReceiptAmount, 0) AS DECIMAL(18,2)) AS ReceiptAmount,
            CAST(ISNULL(CRD.BalanceAmount, 0) AS DECIMAL(18,2)) AS BalanceAmount,
            ISNULL(PayMode.PayModeName, 'Cash') AS PaymentMode,
            ISNULL(CRM.Narration, '') AS Narration,
            CASE WHEN ISNULL(CRM.CancelFlag, 0) = 1 THEN 'Cancel' ELSE 'Active' END AS Status,
            ISNULL(B.BranchName, '') AS BranchName
        FROM dbo.CustomerReceiptDetails CRD
        INNER JOIN dbo.CustomerReceiptMaster CRM ON CRM.Id = CRD.ReceiptMasterId
        LEFT JOIN dbo.PayMode PayMode ON PayMode.PayModeID = CRM.PaymentMethodLedgerId
        LEFT JOIN dbo.Branches B WITH (NOLOCK) ON B.Id = CRM.BranchId
        WHERE CRM.CustomerLedgerId IN (SELECT DISTINCT CustomerLedgerId FROM #FilteredReceipts)
          AND (@BranchId = 0 OR CRD.BranchId = @BranchId)
          AND CRD.CancelFlag = 0
          AND CRM.CancelFlag = 0
        ORDER BY CRM.CustomerLedgerId ASC, CRD.BillNo DESC, CRM.VoucherDate ASC, CRM.VoucherId ASC;
    END
    ELSE -- 'DETAILED' or default
    BEGIN
        SELECT 
            ReceiptMasterId,
            BranchId,
            BranchName,
            ReceiptType,
            VoucherId,
            VoucherNo,
            VoucherDate,
            CustomerLedgerId,
            CustomerName,
            BillNo,
            InvoiceNo,
            BillDate,
            BillAmount,
            TotalBillReceived,
            ReturnedAmount,
            BillBalance,
            ReceiptAmount,
            MasterReceiptAmount,
            BalanceAmount,
            PaymentMode,
            Narration,
            PaymentReference,
            Status,
            CurrentCustomerOutstanding
        FROM #FilteredReceipts
        ORDER BY VoucherDate DESC, VoucherId DESC, BillNo ASC;
    END
END;
GO


PRINT '>>> Step 3: Deploying POS_DatewiseVendorPaymentReport...';
GO
-- Stored Procedure: dbo.POS_DatewiseVendorPaymentReport
GO

IF OBJECT_ID('dbo.POS_DatewiseVendorPaymentReport', 'P') IS NOT NULL
    DROP PROCEDURE dbo.POS_DatewiseVendorPaymentReport;
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ============================================================================
-- Author:      Antigravity
-- Create date: 2026-11-11
-- Description: Date-wise Vendor Payment Report matching FrmPayment data schema,
--              including VendorPaymentMaster, VendorPaymentDetails, PMaster,
--              accurate vendor outstanding balances, dynamic matrix aggregation,
--              and vendor invoices matching FrmPayment.cs.
-- ============================================================================
CREATE PROCEDURE dbo.POS_DatewiseVendorPaymentReport
(
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @PeriodMode VARCHAR(20) = 'ALL',        -- 'DATE_RANGE', 'MONTH_WISE', 'YEAR_WISE', 'ALL'
    @Month INT = 0,                         -- 1..12 or 0 for all
    @Year INT = 0,                          -- e.g. 2026 or 0
    @CompanyId INT = 0,
    @BranchId INT = 0,
    @FinYearId INT = 0,
    @VendorLedgerId INT = 0,
    @PaymentType VARCHAR(50) = 'ALL',       -- 'ALL', 'Vendor Payment', 'Debit Note'
    @PaymentModeFilter VARCHAR(50) = 'ALL', -- 'ALL', 'CASH', 'BANK TRANSFER', 'UPI', 'CHEQUE', etc.
    @Status VARCHAR(20) = 'Active',         -- 'Active', 'Cancel', 'ALL'
    @SearchText NVARCHAR(200) = NULL,
    @ViewMode VARCHAR(30) = 'DETAILED'      -- 'DETAILED', 'DATE_VENDOR_SUMMARY', 'VENDOR_SUMMARY', 'DATE_MATRIX'
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Normalize default dates
    IF (@FromDate IS NULL OR @FromDate = '0001-01-01')
        SET @FromDate = '1900-01-01';

    IF (@ToDate IS NULL OR @ToDate = '0001-01-01')
        SET @ToDate = '2099-12-31';

    SET @SearchText = LTRIM(RTRIM(ISNULL(@SearchText, '')));

    -- 1. Precompute Vendor Outstanding Balance EXACTLY as FrmPayment does (GetVendorOutstandingTotal / OUTSTANDINGTOTAL)
    -- Calculation: PMaster (CancelFlag=0, PaymodeID=1) GrandTotal - PayedAmount - PReturnMaster returns
    CREATE TABLE #VendorBalances (
        LedgerID INT PRIMARY KEY,
        TotalOutstanding DECIMAL(18,2)
    );

    INSERT INTO #VendorBalances (LedgerID, TotalOutstanding)
    SELECT 
        PM.LedgerID,
        CAST(SUM(
            CASE 
                WHEN (ISNULL(PM.GrandTotal, 0) - ISNULL(PM.PayedAmount, 0) - ISNULL(PR.ReturnedAmount, 0)) < 0 THEN 0
                ELSE (ISNULL(PM.GrandTotal, 0) - ISNULL(PM.PayedAmount, 0) - ISNULL(PR.ReturnedAmount, 0))
            END
        ) AS DECIMAL(18,2)) AS TotalOutstanding
    FROM dbo.PMaster PM
    OUTER APPLY (
        SELECT SUM(PRM.GrandTotal) AS ReturnedAmount
        FROM dbo.PReturnMaster PRM
        WHERE (PRM.InvoiceNo = CAST(PM.PurchaseNo AS VARCHAR(50)) OR PRM.PInvoice = CAST(PM.PurchaseNo AS VARCHAR(50)))
          AND (@BranchId = 0 OR PRM.BranchId = @BranchId)
          AND PRM.CancelFlag = 0
    ) PR
    WHERE PM.CancelFlag = 0
      AND PM.PaymodeID = 1 -- Credit purchases only, matching FrmPayment
      AND (@CompanyId = 0 OR ISNULL(PM.CompanyId, 0) = @CompanyId)
      AND (@BranchId = 0 OR ISNULL(PM.BranchId, 0) = @BranchId)
    GROUP BY PM.LedgerID;

    -- 2. Materialize filtered payments matching FrmPayment data
    CREATE TABLE #FilteredPayments (
        PaymentMasterId INT,
        BranchId INT,
        BranchName NVARCHAR(100),
        VoucherType NVARCHAR(50),
        VoucherId INT,
        VoucherNo NVARCHAR(50),
        VoucherDate DATETIME,
        VendorLedgerId INT,
        VendorName NVARCHAR(200),
        PurchaseNo BIGINT,
        InvoiceNo NVARCHAR(100),
        BillDate DATETIME,
        BillAmount DECIMAL(18,2),
        TotalBillPaid DECIMAL(18,2),
        ReturnedAmount DECIMAL(18,2),
        BillBalance DECIMAL(18,2),
        PaymentAmount DECIMAL(18,2),
        MasterPaymentAmount DECIMAL(18,2),
        BalanceAmount DECIMAL(18,2),
        PaymentMode NVARCHAR(100),
        Narration NVARCHAR(MAX),
        PaymentReference NVARCHAR(100),
        Status NVARCHAR(50),
        CurrentVendorOutstanding DECIMAL(18,2)
    );

    -- Insert Vendor Payments from VendorPaymentMaster + VendorPaymentDetails (FrmPayment core tables)
    INSERT INTO #FilteredPayments
    SELECT 
        VPM.Id AS PaymentMasterId,
        ISNULL(VPM.BranchId, 0) AS BranchId,
        ISNULL(B.BranchName, '') AS BranchName,
        'Vendor Payment' AS VoucherType,
        ISNULL(VPM.VoucherId, 0) AS VoucherId,
        CAST(ISNULL(VPM.VoucherId, 0) AS VARCHAR(20)) AS VoucherNo,
        CAST(ISNULL(VPM.VoucherDate, GETDATE()) AS DATETIME) AS VoucherDate,
        ISNULL(VPM.VendorLedgerId, 0) AS VendorLedgerId,
        ISNULL(LM.LedgerName, 'Unknown Vendor') AS VendorName,
        ISNULL(VPD.BillNo, ISNULL(VPM.BillNoUntil, 0)) AS PurchaseNo,
        ISNULL(PM.InvoiceNo, CAST(ISNULL(VPD.BillNo, ISNULL(VPM.BillNoUntil, 0)) AS VARCHAR(50))) AS InvoiceNo,
        CAST(ISNULL(PM.PurchaseDate, VPM.VoucherDate) AS DATETIME) AS BillDate,
        CAST(ISNULL(PM.GrandTotal, ISNULL(VPD.BillAmount, 0)) AS DECIMAL(18,2)) AS BillAmount,
        CAST(ISNULL(PM.PayedAmount, 0) AS DECIMAL(18,2)) AS TotalBillPaid,
        CAST(ISNULL(PR.ReturnedAmount, 0) AS DECIMAL(18,2)) AS ReturnedAmount,
        CAST(CASE 
            WHEN (ISNULL(PM.GrandTotal, 0) - ISNULL(PM.PayedAmount, 0) - ISNULL(PR.ReturnedAmount, 0)) < 0 THEN 0
            ELSE (ISNULL(PM.GrandTotal, 0) - ISNULL(PM.PayedAmount, 0) - ISNULL(PR.ReturnedAmount, 0))
        END AS DECIMAL(18,2)) AS BillBalance,
        CAST(ISNULL(VPD.PaymentAmount, ISNULL(VPM.PaymentAmount, 0)) AS DECIMAL(18,2)) AS PaymentAmount,
        CAST(ISNULL(VPM.PaymentAmount, 0) AS DECIMAL(18,2)) AS MasterPaymentAmount,
        CAST(ISNULL(VPD.BalanceAmount, 0) AS DECIMAL(18,2)) AS BalanceAmount,
        ISNULL(PayMode.PayModeName, 'Cash') AS PaymentMode,
        ISNULL(VPM.Narration, '') AS Narration,
        CAST(ISNULL(VPM.VoucherId, 0) AS NVARCHAR(50)) AS PaymentReference,
        CASE WHEN ISNULL(VPM.CancelFlag, 0) = 1 THEN 'Cancel' ELSE 'Active' END AS Status,
        CAST(ISNULL(VB.TotalOutstanding, 0) AS DECIMAL(18,2)) AS CurrentVendorOutstanding
    FROM dbo.VendorPaymentMaster VPM
    LEFT JOIN dbo.VendorPaymentDetails VPD 
        ON VPD.PaymentMasterId = VPM.Id 
       AND (@Status = 'ALL' OR VPD.CancelFlag = CASE WHEN @Status = 'Cancel' THEN 1 ELSE 0 END)
    LEFT JOIN dbo.PMaster PM 
        ON CAST(PM.PurchaseNo AS VARCHAR(50)) = CAST(VPD.BillNo AS VARCHAR(50)) 
       AND PM.CancelFlag = 0
       AND (@BranchId = 0 OR PM.BranchId = @BranchId)
    OUTER APPLY (
        SELECT SUM(PRM.GrandTotal) AS ReturnedAmount
        FROM dbo.PReturnMaster PRM
        WHERE (PRM.InvoiceNo = CAST(PM.PurchaseNo AS VARCHAR(50)) OR PRM.PInvoice = CAST(PM.PurchaseNo AS VARCHAR(50)))
          AND (@BranchId = 0 OR PRM.BranchId = @BranchId)
          AND PRM.CancelFlag = 0
    ) PR
    LEFT JOIN dbo.LedgerMaster LM 
        ON LM.LedgerID = VPM.VendorLedgerId
    LEFT JOIN dbo.Branches B WITH (NOLOCK)
        ON B.Id = VPM.BranchId
    LEFT JOIN dbo.PayMode PayMode 
        ON PayMode.PayModeID = VPM.PaymentMethodLedgerId
    LEFT JOIN #VendorBalances VB 
        ON VB.LedgerID = VPM.VendorLedgerId
    WHERE (@Status = 'ALL' OR ISNULL(VPM.CancelFlag, 0) = CASE WHEN @Status = 'Cancel' THEN 1 ELSE 0 END)
      AND (@CompanyId = 0 OR ISNULL(VPM.CompanyId, 0) = @CompanyId)
      AND (@BranchId = 0 OR ISNULL(VPM.BranchId, 0) = @BranchId)
      AND (@VendorLedgerId = 0 OR ISNULL(VPM.VendorLedgerId, 0) = @VendorLedgerId);

    -- Apply PeriodMode, Mode, and Search criteria in deletion/filtering
    DELETE FROM #FilteredPayments
    WHERE NOT (
        -- Period Mode
        (
            (@PeriodMode = 'MONTH_WISE' AND 
                (@Year = 0 OR YEAR(VoucherDate) = @Year) AND 
                (@Month = 0 OR MONTH(VoucherDate) = @Month))
            OR
            (@PeriodMode = 'YEAR_WISE' AND 
                (@Year = 0 OR YEAR(VoucherDate) = @Year))
            OR
            (@PeriodMode = 'DATE_RANGE' AND 
                CAST(VoucherDate AS DATE) >= @FromDate AND CAST(VoucherDate AS DATE) <= @ToDate)
            OR
            (@PeriodMode = 'ALL' AND 
                (@FromDate = '1900-01-01' OR CAST(VoucherDate AS DATE) >= @FromDate) AND 
                (@ToDate = '2099-12-31' OR CAST(VoucherDate AS DATE) <= @ToDate))
        )
        -- Voucher Type
        AND (@PaymentType = 'ALL' OR VoucherType = @PaymentType)
        -- Payment Mode
        AND (@PaymentModeFilter = 'ALL' OR PaymentMode = @PaymentModeFilter)
        -- Search Text
        AND (
            @SearchText = '' OR
            VendorName LIKE '%' + @SearchText + '%' OR
            VoucherNo LIKE '%' + @SearchText + '%' OR
            CAST(PurchaseNo AS VARCHAR(50)) LIKE '%' + @SearchText + '%' OR
            InvoiceNo LIKE '%' + @SearchText + '%' OR
            PaymentMode LIKE '%' + @SearchText + '%' OR
            Narration LIKE '%' + @SearchText + '%' OR
            PaymentReference LIKE '%' + @SearchText + '%'
        )
    );

    -- 3. Return results based on ViewMode
    IF (@ViewMode = 'DATE_VENDOR_SUMMARY')
    BEGIN
        SELECT 
            CAST(VoucherDate AS DATE) AS TransactionDate,
            VendorLedgerId,
            VendorName,
            COUNT(DISTINCT VoucherId) AS VoucherCount,
            SUM(PaymentAmount) AS TotalPaid,
            MAX(CurrentVendorOutstanding) AS CurrentVendorOutstanding
        FROM #FilteredPayments
        GROUP BY CAST(VoucherDate AS DATE), VendorLedgerId, VendorName
        ORDER BY CAST(VoucherDate AS DATE) DESC, VendorName ASC;
    END
    ELSE IF (@ViewMode = 'VENDOR_SUMMARY')
    BEGIN
        SELECT 
            VendorLedgerId,
            VendorName,
            COUNT(DISTINCT VoucherId) AS VoucherCount,
            SUM(PaymentAmount) AS TotalPaid,
            MAX(VoucherDate) AS LastPaymentDate,
            MAX(CurrentVendorOutstanding) AS CurrentVendorOutstanding
        FROM #FilteredPayments
        GROUP BY VendorLedgerId, VendorName
        ORDER BY VendorName ASC;
    END
    ELSE IF (@ViewMode = 'DATE_MATRIX')
    BEGIN
        -- Result Set 0: Payment transactions within period (used for dynamic date matrix pivot)
        SELECT 
            PaymentMasterId,
            BranchId,
            BranchName,
            VoucherType,
            VoucherId,
            VoucherNo,
            VoucherDate,
            VendorLedgerId,
            VendorName,
            PurchaseNo,
            InvoiceNo,
            BillDate,
            BillAmount,
            TotalBillPaid,
            ReturnedAmount,
            BillBalance,
            PaymentAmount,
            MasterPaymentAmount,
            BalanceAmount,
            PaymentMode,
            Narration,
            PaymentReference,
            Status,
            CurrentVendorOutstanding
        FROM #FilteredPayments
        ORDER BY VoucherDate DESC, VoucherId DESC, PurchaseNo ASC;

        -- Result Set 1: Vendor Invoices matching FrmPayment.cs (All bills for participating vendors)
        SELECT 
            PM.LedgerID AS VendorLedgerId,
            LM.LedgerName AS VendorName,
            CAST(PM.LedgerID AS VARCHAR(20)) + '_' + CAST(PM.PurchaseNo AS VARCHAR(50)) AS BillKey,
            PM.PurchaseNo,
            ISNULL(PM.InvoiceNo, CAST(PM.PurchaseNo AS VARCHAR(50))) AS InvoiceNo,
            CAST(ISNULL(PM.PurchaseDate, '1900-01-01') AS DATETIME) AS BillDate,
            CAST(ISNULL(PM.GrandTotal, 0) AS DECIMAL(18,2)) AS InvoiceAmount,
            CAST(ISNULL(PM.PayedAmount, 0) AS DECIMAL(18,2)) AS PaidAmount,
            CAST(ISNULL(PR.ReturnedAmount, 0) AS DECIMAL(18,2)) AS ReturnedAmount,
            CAST(CASE 
                WHEN (ISNULL(PM.GrandTotal, 0) - ISNULL(PM.PayedAmount, 0) - ISNULL(PR.ReturnedAmount, 0)) < 0 THEN 0
                ELSE (ISNULL(PM.GrandTotal, 0) - ISNULL(PM.PayedAmount, 0) - ISNULL(PR.ReturnedAmount, 0))
            END AS DECIMAL(18,2)) AS Balance,
            CAST(ISNULL(PeriodPay.PaidInPeriod, 0) AS DECIMAL(18,2)) AS PaidInPeriod,
            ISNULL(VoucherList.Vouchers, '') AS Vouchers,
            ISNULL(PayMode.PayModeName, 'CASH') AS PaymentMethod,
            CASE 
                WHEN (ISNULL(PM.GrandTotal, 0) - ISNULL(PM.PayedAmount, 0) - ISNULL(PR.ReturnedAmount, 0)) <= 0 THEN 'Fully Paid'
                WHEN ISNULL(PM.PayedAmount, 0) > 0 THEN 'Partially Paid'
                ELSE 'Unpaid'
            END AS Status,
            ISNULL(B.BranchName, '') AS BranchName
        FROM dbo.PMaster PM
        LEFT JOIN dbo.LedgerMaster LM ON LM.LedgerID = PM.LedgerID
        LEFT JOIN dbo.Branches B WITH (NOLOCK) ON B.Id = PM.BranchId
        OUTER APPLY (
            SELECT SUM(PRM.GrandTotal) AS ReturnedAmount
            FROM dbo.PReturnMaster PRM
            WHERE (PRM.InvoiceNo = CAST(PM.PurchaseNo AS VARCHAR(50)) OR PRM.PInvoice = CAST(PM.PurchaseNo AS VARCHAR(50)))
              AND (@BranchId = 0 OR PRM.BranchId = @BranchId)
              AND PRM.CancelFlag = 0
        ) PR
        OUTER APPLY (
            SELECT SUM(VPD.PaymentAmount) AS PaidInPeriod
            FROM dbo.VendorPaymentDetails VPD
            INNER JOIN dbo.VendorPaymentMaster VPM ON VPM.Id = VPD.PaymentMasterId
            WHERE VPD.BillNo = PM.PurchaseNo
              AND (@BranchId = 0 OR VPD.BranchId = @BranchId)
              AND VPD.CancelFlag = 0
              AND VPM.CancelFlag = 0
              AND CAST(VPM.VoucherDate AS DATE) >= @FromDate
              AND CAST(VPM.VoucherDate AS DATE) <= @ToDate
        ) PeriodPay
        OUTER APPLY (
            SELECT STUFF((
                SELECT DISTINCT ', #' + CAST(VPM2.VoucherId AS VARCHAR(20))
                FROM dbo.VendorPaymentDetails VPD2
                INNER JOIN dbo.VendorPaymentMaster VPM2 ON VPM2.Id = VPD2.PaymentMasterId
                WHERE VPD2.BillNo = PM.PurchaseNo
                  AND (@BranchId = 0 OR VPD2.BranchId = @BranchId)
                  AND VPD2.CancelFlag = 0
                  AND VPM2.CancelFlag = 0
                FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS Vouchers
        ) VoucherList
        OUTER APPLY (
            SELECT TOP 1 PayMode.PayModeName
            FROM dbo.VendorPaymentDetails VPD3
            INNER JOIN dbo.VendorPaymentMaster VPM3 ON VPM3.Id = VPD3.PaymentMasterId
            LEFT JOIN dbo.PayMode PayMode ON PayMode.PayModeID = VPM3.PaymentMethodLedgerId
            WHERE VPD3.BillNo = PM.PurchaseNo
              AND VPD3.CancelFlag = 0
            ORDER BY VPM3.VoucherDate DESC, VPM3.Id DESC
        ) PayMode
        WHERE PM.LedgerID IN (SELECT DISTINCT VendorLedgerId FROM #FilteredPayments)
          AND (@BranchId = 0 OR PM.BranchId = @BranchId)
          AND PM.CancelFlag = 0
        ORDER BY PM.LedgerID ASC, PM.PurchaseNo DESC;

        -- Result Set 2: Voucher payment breakdown per bill
        SELECT 
            CAST(VPM.VendorLedgerId AS VARCHAR(20)) + '_' + CAST(VPD.BillNo AS VARCHAR(50)) AS BillKey,
            VPM.VendorLedgerId,
            VPD.BillNo AS PurchaseNo,
            VPM.VoucherDate,
            ISNULL(VPM.VoucherId, 0) AS VoucherId,
            CAST(ISNULL(VPM.VoucherId, 0) AS VARCHAR(20)) AS VoucherNo,
            CAST(ISNULL(VPD.PaymentAmount, 0) AS DECIMAL(18,2)) AS PaymentAmount,
            CAST(ISNULL(VPD.BalanceAmount, 0) AS DECIMAL(18,2)) AS BalanceAmount,
            ISNULL(PayMode.PayModeName, 'Cash') AS PaymentMode,
            ISNULL(VPM.Narration, '') AS Narration,
            CASE WHEN ISNULL(VPM.CancelFlag, 0) = 1 THEN 'Cancel' ELSE 'Active' END AS Status,
            ISNULL(B.BranchName, '') AS BranchName
        FROM dbo.VendorPaymentDetails VPD
        INNER JOIN dbo.VendorPaymentMaster VPM ON VPM.Id = VPD.PaymentMasterId
        LEFT JOIN dbo.PayMode PayMode ON PayMode.PayModeID = VPM.PaymentMethodLedgerId
        LEFT JOIN dbo.Branches B WITH (NOLOCK) ON B.Id = VPM.BranchId
        WHERE VPM.VendorLedgerId IN (SELECT DISTINCT VendorLedgerId FROM #FilteredPayments)
          AND (@BranchId = 0 OR VPD.BranchId = @BranchId)
          AND VPD.CancelFlag = 0
          AND VPM.CancelFlag = 0
        ORDER BY VPM.VendorLedgerId ASC, VPD.BillNo DESC, VPM.VoucherDate ASC, VPM.VoucherId ASC;
    END
    ELSE -- 'DETAILED' or default
    BEGIN
        SELECT 
            PaymentMasterId,
            BranchId,
            BranchName,
            VoucherType,
            VoucherId,
            VoucherNo,
            VoucherDate,
            VendorLedgerId,
            VendorName,
            PurchaseNo,
            InvoiceNo,
            BillDate,
            BillAmount,
            TotalBillPaid,
            ReturnedAmount,
            BillBalance,
            PaymentAmount,
            MasterPaymentAmount,
            BalanceAmount,
            PaymentMode,
            Narration,
            PaymentReference,
            Status,
            CurrentVendorOutstanding
        FROM #FilteredPayments
        ORDER BY VoucherDate DESC, VoucherId DESC, PurchaseNo ASC;
    END

    DROP TABLE #FilteredPayments;
    DROP TABLE #VendorBalances;
END
GO


PRINT '>>> Step 4: Deploying POS_DatewiseExpenseReport...';
GO
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
        SET @FromDate = CAST(CONVERT(VARCHAR(4), @Year) + '-' + RIGHT('0' + CONVERT(VARCHAR(2), @Month), 2) + '-01 00:00:00' AS DATETIME);
        SET @ToDate = DATEADD(SECOND, -1, DATEADD(MONTH, 1, @FromDate));
    END
    ELSE IF (@PeriodMode = 'YEAR_WISE')
    BEGIN
        IF (@Year IS NULL OR @Year < 2000) SET @Year = YEAR(GETDATE());
        SET @FromDate = CAST(CONVERT(VARCHAR(4), @Year) + '-01-01 00:00:00' AS DATETIME);
        SET @ToDate = CAST(CONVERT(VARCHAR(4), @Year) + '-12-31 23:59:59' AS DATETIME);
    END
    ELSE IF (@PeriodMode = 'ALL')
    BEGIN
        SET @FromDate = '2000-01-01 00:00:00';
        SET @ToDate = '2099-12-31 23:59:59';
    END
    ELSE -- 'DATE_RANGE' or fallback
    BEGIN
        IF (@FromDate IS NULL OR @FromDate < '1900-01-01')
            SET @FromDate = CAST(CONVERT(VARCHAR(7), GETDATE(), 120) + '-01 00:00:00' AS DATETIME);
        ELSE
            SET @FromDate = CAST(CONVERT(VARCHAR(10), @FromDate, 120) + ' 00:00:00' AS DATETIME);

        IF (@ToDate IS NULL OR @ToDate < '1900-01-01')
            SET @ToDate = CAST(CONVERT(VARCHAR(10), GETDATE(), 120) + ' 23:59:59' AS DATETIME);
        ELSE
            SET @ToDate = CAST(CONVERT(VARCHAR(10), @ToDate, 120) + ' 23:59:59' AS DATETIME);
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
    LEFT JOIN dbo.Branches bm WITH (NOLOCK) ON v.BranchID = bm.Id
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


PRINT '>>> SUCCESS: All Date-wise procedures and Branch compatibility views installed successfully!';
GO
-- ====================================================================================
-- Step 5: dbo.POS_DatewisePurchaseReturnReport
-- ====================================================================================

-- ============================================================================
-- Stored Procedure: dbo.POS_DatewisePurchaseReturnReport
-- Description:      Retrieves purchase return (debit note) records filtered by
--                   date range, month, year, or all dates, with item name, quantity,
--                   cost, discounts, tax, net amount, return reason, and vendor.
-- Author:           Nexoris POS Engineering
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[POS_DatewisePurchaseReturnReport]') AND type IN (N'P', N'PC'))
BEGIN
    EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[POS_DatewisePurchaseReturnReport] AS RETURN 0;';
END
GO

ALTER PROCEDURE [dbo].[POS_DatewisePurchaseReturnReport]
    @FromDate        DATE          = NULL,
    @ToDate          DATE          = NULL,
    @PeriodMode      VARCHAR(20)   = 'ALL',       -- 'ALL', 'DATE_RANGE', 'MONTH_WISE', 'YEAR_WISE'
    @Month           INT           = 0,           -- 0 = ALL, 1..12
    @Year            INT           = 0,           -- 0 = ALL, 2026, ...
    @CompanyId       INT           = 0,
    @BranchId        INT           = 0,
    @FinYearId       INT           = 0,
    @VendorLedgerId  INT           = 0,
    @ItemSearch      NVARCHAR(200) = NULL,
    @ViewMode        VARCHAR(30)   = 'DETAILED'   -- 'DETAILED', 'DATE_ITEM_SUMMARY', 'ITEM_SUMMARY', 'VENDOR_SUMMARY'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Mode VARCHAR(20) = UPPER(LTRIM(RTRIM(ISNULL(@PeriodMode, 'ALL'))));
    DECLARE @View VARCHAR(30) = UPPER(LTRIM(RTRIM(ISNULL(@ViewMode, 'DETAILED'))));
    DECLARE @CleanSearch NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@ItemSearch)), '');

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

    -- ========================================================================
    -- VIEW MODE 1: DETAILED (Return Voucher & Item Line by Line)
    -- ========================================================================
    IF @View = 'DETAILED'
    BEGIN
        SELECT 
            PRM.PReturnDate AS ReturnDate,
            PRM.PReturnNo AS ReturnNo,
            ISNULL(PRM.InvoiceNo, '') AS InvoiceNo,
            ISNULL(PRM.InvoiceDate, PRM.PReturnDate) AS InvoiceDate,
            ISNULL(NULLIF(LM.LedgerName, ''), ISNULL(NULLIF(PRM.VendorName, ''), 'Unknown Vendor')) AS VendorName,
            ISNULL(PRM.LedgerID, 0) AS VendorLedgerId,
            ISNULL(PRD.SlNo, 1) AS SlNo,
            ISNULL(PRD.ItemID, 0) AS ItemID,
            ISNULL(PS.BarCode, ISNULL(IM.Barcode, '')) AS BarCode,
            ISNULL(PRD.ItemName, '') AS ItemName,
            ISNULL(PRD.Unit, '') AS Unit,
            CAST(ISNULL(PRD.Packing, 1) AS NVARCHAR(50)) AS Packing,
            CAST(ISNULL(PRD.Qty, 0) AS DECIMAL(18,2)) AS Qty,
            CAST(ISNULL(PRD.Free, 0) AS DECIMAL(18,2)) AS Free,
            CAST((ISNULL(PRD.Qty, 0) + ISNULL(PRD.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
            CAST(ISNULL(PRD.Cost, 0) AS DECIMAL(18,2)) AS Cost,
            CAST(ISNULL(PRD.Qty, 0) * ISNULL(PRD.Cost, 0) AS DECIMAL(18,2)) AS Amount,
            CAST(ISNULL(PRD.DisAmt, 0) AS DECIMAL(18,2)) AS DisAmt,
            CAST(ISNULL(PRD.TaxAmt, 0) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(ISNULL(PRD.TotalSP, (ISNULL(PRD.Qty, 0) * ISNULL(PRD.Cost, 0)) + ISNULL(PRD.TaxAmt, 0) - ISNULL(PRD.DisAmt, 0)) AS DECIMAL(18,2)) AS NetAmount,
            ISNULL(PRD.Reason, '') AS Reason,
            ISNULL(PRM.Paymode, 'CREDIT') AS Paymode,
            ISNULL(PRM.BranchId, 0) AS BranchId,
            ISNULL(B.BranchName, '') AS BranchName,
            ISNULL(PRM.UserName, '') AS BilledBy
        FROM dbo.PReturnMaster PRM WITH (NOLOCK)
        INNER JOIN dbo.PReturnDetails PRD WITH (NOLOCK) 
            ON PRM.PReturnNo = PRD.PReturnNo 
           AND PRM.BranchId = PRD.BranchID
        LEFT JOIN dbo.LedgerMaster LM WITH (NOLOCK) ON LM.LedgerID = PRM.LedgerID
        LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemID = PRD.ItemID
        LEFT JOIN dbo.Branches B WITH (NOLOCK) ON B.Id = PRM.BranchId
        OUTER APPLY (
            SELECT TOP 1 ps.BarCode
            FROM dbo.PriceSettings ps WITH (NOLOCK)
            WHERE ps.ItemId = PRD.ItemID
              AND (@BranchId = 0 OR ps.BranchId = @BranchId)
            ORDER BY CASE WHEN ps.UnitId = PRD.UnitId THEN 0 WHEN ps.IsBaseUnit = 'Y' THEN 1 ELSE 2 END
        ) PS
        WHERE ISNULL(PRM.CancelFlag, 0) = 0
          AND (@CompanyId = 0 OR ISNULL(PRM.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(PRM.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(PRM.FinYearId, 0) = @FinYearId)
          AND (@VendorLedgerId = 0 OR ISNULL(PRM.LedgerID, 0) = @VendorLedgerId)
          AND (
                @CleanSearch IS NULL 
                OR PRD.ItemName LIKE '%' + @CleanSearch + '%' 
                OR PS.BarCode LIKE '%' + @CleanSearch + '%' 
                OR PRM.InvoiceNo LIKE '%' + @CleanSearch + '%'
                OR CAST(PRM.PReturnNo AS VARCHAR(50)) LIKE '%' + @CleanSearch + '%'
              )
          AND (
                @Mode = 'ALL'
                OR (@Mode = 'DATE_RANGE' AND CAST(PRM.PReturnDate AS DATE) >= @StartDate AND CAST(PRM.PReturnDate AS DATE) <= @EndDate)
                OR (@Mode = 'MONTH_WISE' AND (
                    (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year) AND
                    (@Month = 0 OR MONTH(PRM.PReturnDate) = @Month)
                ))
                OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year))
              )
        ORDER BY PRM.PReturnDate DESC, PRM.PReturnNo DESC, PRD.SlNo ASC;
    END

    -- ========================================================================
    -- VIEW MODE 2: DATE_ITEM_SUMMARY / DATE_MATRIX (Aggregated by Date and Item)
    -- ========================================================================
    ELSE IF @View = 'DATE_ITEM_SUMMARY' OR @View = 'DATE_MATRIX'
    BEGIN
        SELECT 
            CAST(PRM.PReturnDate AS DATE) AS ReturnDate,
            PRD.ItemID,
            ISNULL(PS.BarCode, ISNULL(IM.Barcode, '')) AS BarCode,
            PRD.ItemName,
            PRD.Unit,
            COUNT(DISTINCT PRM.PReturnNo) AS ReturnCount,
            CAST(SUM(ISNULL(PRD.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
            CAST(SUM(ISNULL(PRD.Free, 0)) AS DECIMAL(18,2)) AS Free,
            CAST(SUM(ISNULL(PRD.Qty, 0) + ISNULL(PRD.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
            CAST(SUM(ISNULL(PRD.Qty, 0) * ISNULL(PRD.Cost, 0)) AS DECIMAL(18,2)) AS Amount,
            CAST(SUM(ISNULL(PRD.DisAmt, 0)) AS DECIMAL(18,2)) AS DisAmt,
            CAST(SUM(ISNULL(PRD.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(SUM(ISNULL(PRD.TotalSP, (ISNULL(PRD.Qty, 0) * ISNULL(PRD.Cost, 0)) + ISNULL(PRD.TaxAmt, 0) - ISNULL(PRD.DisAmt, 0))) AS DECIMAL(18,2)) AS NetAmount
        FROM dbo.PReturnMaster PRM WITH (NOLOCK)
        INNER JOIN dbo.PReturnDetails PRD WITH (NOLOCK) 
            ON PRM.PReturnNo = PRD.PReturnNo 
           AND PRM.BranchId = PRD.BranchID
        LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemID = PRD.ItemID
        OUTER APPLY (
            SELECT TOP 1 ps.BarCode
            FROM dbo.PriceSettings ps WITH (NOLOCK)
            WHERE ps.ItemId = PRD.ItemID
              AND (@BranchId = 0 OR ps.BranchId = @BranchId)
            ORDER BY CASE WHEN ps.UnitId = PRD.UnitId THEN 0 WHEN ps.IsBaseUnit = 'Y' THEN 1 ELSE 2 END
        ) PS
        WHERE ISNULL(PRM.CancelFlag, 0) = 0
          AND (@CompanyId = 0 OR ISNULL(PRM.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(PRM.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(PRM.FinYearId, 0) = @FinYearId)
          AND (@VendorLedgerId = 0 OR ISNULL(PRM.LedgerID, 0) = @VendorLedgerId)
          AND (
                @CleanSearch IS NULL 
                OR PRD.ItemName LIKE '%' + @CleanSearch + '%' 
                OR PS.BarCode LIKE '%' + @CleanSearch + '%' 
                OR PRM.InvoiceNo LIKE '%' + @CleanSearch + '%'
              )
          AND (
                @Mode = 'ALL'
                OR (@Mode = 'DATE_RANGE' AND CAST(PRM.PReturnDate AS DATE) >= @StartDate AND CAST(PRM.PReturnDate AS DATE) <= @EndDate)
                OR (@Mode = 'MONTH_WISE' AND (
                    (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year) AND
                    (@Month = 0 OR MONTH(PRM.PReturnDate) = @Month)
                ))
                OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year))
              )
        GROUP BY 
            CAST(PRM.PReturnDate AS DATE),
            PRD.ItemID,
            ISNULL(PS.BarCode, ISNULL(IM.Barcode, '')),
            PRD.ItemName,
            PRD.Unit
        ORDER BY ReturnDate DESC, NetAmount DESC;
    END

    -- ========================================================================
    -- VIEW MODE 3: ITEM_SUMMARY (Total aggregated by Item across entire period)
    -- ========================================================================
    ELSE IF @View = 'ITEM_SUMMARY'
    BEGIN
        SELECT 
            PRD.ItemID,
            ISNULL(PS.BarCode, ISNULL(IM.Barcode, '')) AS BarCode,
            PRD.ItemName,
            PRD.Unit,
            COUNT(DISTINCT PRM.PReturnNo) AS ReturnCount,
            CAST(SUM(ISNULL(PRD.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
            CAST(SUM(ISNULL(PRD.Free, 0)) AS DECIMAL(18,2)) AS Free,
            CAST(SUM(ISNULL(PRD.Qty, 0) + ISNULL(PRD.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
            CAST(SUM(ISNULL(PRD.Qty, 0) * ISNULL(PRD.Cost, 0)) AS DECIMAL(18,2)) AS Amount,
            CAST(SUM(ISNULL(PRD.DisAmt, 0)) AS DECIMAL(18,2)) AS DisAmt,
            CAST(SUM(ISNULL(PRD.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(SUM(ISNULL(PRD.TotalSP, (ISNULL(PRD.Qty, 0) * ISNULL(PRD.Cost, 0)) + ISNULL(PRD.TaxAmt, 0) - ISNULL(PRD.DisAmt, 0))) AS DECIMAL(18,2)) AS NetAmount
        FROM dbo.PReturnMaster PRM WITH (NOLOCK)
        INNER JOIN dbo.PReturnDetails PRD WITH (NOLOCK) 
            ON PRM.PReturnNo = PRD.PReturnNo 
           AND PRM.BranchId = PRD.BranchID
        LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemID = PRD.ItemID
        OUTER APPLY (
            SELECT TOP 1 ps.BarCode
            FROM dbo.PriceSettings ps WITH (NOLOCK)
            WHERE ps.ItemId = PRD.ItemID
              AND (@BranchId = 0 OR ps.BranchId = @BranchId)
            ORDER BY CASE WHEN ps.UnitId = PRD.UnitId THEN 0 WHEN ps.IsBaseUnit = 'Y' THEN 1 ELSE 2 END
        ) PS
        WHERE ISNULL(PRM.CancelFlag, 0) = 0
          AND (@CompanyId = 0 OR ISNULL(PRM.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(PRM.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(PRM.FinYearId, 0) = @FinYearId)
          AND (@VendorLedgerId = 0 OR ISNULL(PRM.LedgerID, 0) = @VendorLedgerId)
          AND (
                @CleanSearch IS NULL 
                OR PRD.ItemName LIKE '%' + @CleanSearch + '%' 
                OR PS.BarCode LIKE '%' + @CleanSearch + '%' 
                OR PRM.InvoiceNo LIKE '%' + @CleanSearch + '%'
              )
          AND (
                @Mode = 'ALL'
                OR (@Mode = 'DATE_RANGE' AND CAST(PRM.PReturnDate AS DATE) >= @StartDate AND CAST(PRM.PReturnDate AS DATE) <= @EndDate)
                OR (@Mode = 'MONTH_WISE' AND (
                    (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year) AND
                    (@Month = 0 OR MONTH(PRM.PReturnDate) = @Month)
                ))
                OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year))
              )
        GROUP BY 
            PRD.ItemID,
            ISNULL(PS.BarCode, ISNULL(IM.Barcode, '')),
            PRD.ItemName,
            PRD.Unit
        ORDER BY NetAmount DESC;
    END

    -- ========================================================================
    -- VIEW MODE 4: VENDOR_SUMMARY (Total aggregated by Vendor)
    -- ========================================================================
    ELSE IF @View = 'VENDOR_SUMMARY'
    BEGIN
        SELECT 
            ISNULL(PRM.LedgerID, 0) AS VendorLedgerId,
            ISNULL(NULLIF(LM.LedgerName, ''), ISNULL(NULLIF(PRM.VendorName, ''), 'Unknown Vendor')) AS VendorName,
            COUNT(DISTINCT PRM.PReturnNo) AS ReturnCount,
            CAST(SUM(ISNULL(PRD.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
            CAST(SUM(ISNULL(PRD.Free, 0)) AS DECIMAL(18,2)) AS Free,
            CAST(SUM(ISNULL(PRD.Qty, 0) + ISNULL(PRD.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
            CAST(SUM(ISNULL(PRD.Qty, 0) * ISNULL(PRD.Cost, 0)) AS DECIMAL(18,2)) AS Amount,
            CAST(SUM(ISNULL(PRD.DisAmt, 0)) AS DECIMAL(18,2)) AS DisAmt,
            CAST(SUM(ISNULL(PRD.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(SUM(ISNULL(PRD.TotalSP, (ISNULL(PRD.Qty, 0) * ISNULL(PRD.Cost, 0)) + ISNULL(PRD.TaxAmt, 0) - ISNULL(PRD.DisAmt, 0))) AS DECIMAL(18,2)) AS NetAmount
        FROM dbo.PReturnMaster PRM WITH (NOLOCK)
        INNER JOIN dbo.PReturnDetails PRD WITH (NOLOCK) 
            ON PRM.PReturnNo = PRD.PReturnNo 
           AND PRM.BranchId = PRD.BranchID
        LEFT JOIN dbo.LedgerMaster LM WITH (NOLOCK) ON LM.LedgerID = PRM.LedgerID
        WHERE ISNULL(PRM.CancelFlag, 0) = 0
          AND (@CompanyId = 0 OR ISNULL(PRM.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(PRM.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(PRM.FinYearId, 0) = @FinYearId)
          AND (@VendorLedgerId = 0 OR ISNULL(PRM.LedgerID, 0) = @VendorLedgerId)
          AND (
                @CleanSearch IS NULL 
                OR PRD.ItemName LIKE '%' + @CleanSearch + '%' 
                OR PRM.InvoiceNo LIKE '%' + @CleanSearch + '%'
              )
          AND (
                @Mode = 'ALL'
                OR (@Mode = 'DATE_RANGE' AND CAST(PRM.PReturnDate AS DATE) >= @StartDate AND CAST(PRM.PReturnDate AS DATE) <= @EndDate)
                OR (@Mode = 'MONTH_WISE' AND (
                    (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year) AND
                    (@Month = 0 OR MONTH(PRM.PReturnDate) = @Month)
                ))
                OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year))
              )
        GROUP BY 
            ISNULL(PRM.LedgerID, 0),
            ISNULL(NULLIF(LM.LedgerName, ''), ISNULL(NULLIF(PRM.VendorName, ''), 'Unknown Vendor'))
        ORDER BY NetAmount DESC;
    END
END
GO

