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
    LEFT JOIN dbo.Branch B 
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
    LEFT JOIN dbo.Branch B 
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
        LEFT JOIN dbo.Branch B 
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
        LEFT JOIN dbo.Branch B ON B.Id = SM.BranchId
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
        LEFT JOIN dbo.Branch B ON B.Id = CRM.BranchId
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
