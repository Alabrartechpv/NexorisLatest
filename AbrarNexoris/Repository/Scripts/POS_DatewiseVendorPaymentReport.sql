USE [RambaiTest]
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
    LEFT JOIN dbo.Branch B 
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
        LEFT JOIN dbo.Branch B ON B.Id = PM.BranchId
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
        LEFT JOIN dbo.Branch B ON B.Id = VPM.BranchId
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
