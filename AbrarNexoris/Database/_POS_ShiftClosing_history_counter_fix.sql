USE [RambaiTest]
GO
/****** Object:  StoredProcedure [dbo].[POS_ShiftClosing]    Script Date: 16-06-2026 11:22:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/* Shift closing: exact CounterSessionId + split-payment aware summary. */
ALTER PROCEDURE [dbo].[POS_ShiftClosing]
(
    @ShiftClosingId INT = NULL,
    @CompanyId INT = NULL,
    @BranchId INT = NULL,
    @FinYearId INT = NULL,
    @Counter VARCHAR(50) = NULL,
    @UserId INT = NULL,
    @CounterSessionId BIGINT = NULL,
    @ClosingDate DATETIME = NULL,
    @ReportSelection VARCHAR(50) = NULL,
    @DocNo VARCHAR(50) = NULL,

    @TotalGrossSales DECIMAL(18,2) = 0,
    @TotalDiscount DECIMAL(18,2) = 0,
    @TotalReturn DECIMAL(18,2) = 0,
    @NetSales DECIMAL(18,2) = 0,

    @CashSale DECIMAL(18,2) = 0,
    @CardSale DECIMAL(18,2) = 0,
    @UpiSale DECIMAL(18,2) = 0,
    @CreditSale DECIMAL(18,2) = 0,
    @CustomerReceipt DECIMAL(18,2) = 0,
    @TotalCollection DECIMAL(18,2) = 0,

    @CashRefundAdjusted DECIMAL(18,2) = 0,
    @MidDayCashSkim DECIMAL(18,2) = 0,
    @SystemExpectedCash DECIMAL(18,2) = 0,

    @PhysicalCashCounted DECIMAL(18,2) = 0,
    @CashDifference DECIMAL(18,2) = 0,
    @DifferenceReason VARCHAR(500) = NULL,

    @Status VARCHAR(20) = NULL,
    @VoucherId INT = NULL,
    @CreatedBy INT = NULL,
    @ModifiedBy INT = NULL,

    @PageIndex INT = 0,
    @PageSize INT = 50,
    @SortBy VARCHAR(50) = 'ShiftClosingId',
    @SortByDirection VARCHAR(10) = 'DESC',

    @_Operation VARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF(@_Operation = 'CREATE')
        BEGIN
            DECLARE @ActualClosingTime DATETIME = GETDATE();
            DECLARE @ShiftNumber INT;

            IF @CounterSessionId IS NULL OR @CounterSessionId <= 0
            BEGIN
                RAISERROR('CounterSessionId is required for shift closing.', 16, 1);
                RETURN;
            END

            IF NOT EXISTS (
                SELECT 1
                FROM dbo.CounterSessions WITH (UPDLOCK, HOLDLOCK)
                WHERE SessionId = @CounterSessionId
                  AND BranchId = @BranchId
                  AND UserId = @UserId
                  AND Status = 'Open'
            )
            BEGIN
                RAISERROR('Counter session is not open or already closed.', 16, 1);
                RETURN;
            END

            SELECT @ShiftNumber = ISNULL(COUNT(*), 0) + 1
            FROM ShiftClosing
            WHERE UserId = @UserId
              AND BranchId = @BranchId
              AND CAST(ClosingDate AS DATE) = CAST(@ActualClosingTime AS DATE)
              AND IsDelete = 0;

            INSERT INTO ShiftClosing (
                CompanyId, BranchId, FinYearId, CounterSessionId, Counter, UserId, ClosingDate,
                ReportSelection, DocNo,
                TotalGrossSales, TotalDiscount, TotalReturn, NetSales,
                CashSale, CardSale, UpiSale, CreditSale, CustomerReceipt, TotalCollection,
                CashRefundAdjusted, MidDayCashSkim, SystemExpectedCash,
                PhysicalCashCounted, CashDifference, DifferenceReason,
                Status, VoucherId, CreatedBy, CreatedDate
            )
            VALUES (
                @CompanyId, @BranchId, @FinYearId, @CounterSessionId, @Counter, @UserId, @ActualClosingTime,
                ISNULL(@ReportSelection, 'Shift Collection') + ' (Shift #' + CAST(@ShiftNumber AS VARCHAR) + ')',
                @DocNo,
                @TotalGrossSales, @TotalDiscount, @TotalReturn, @NetSales,
                @CashSale, @CardSale, @UpiSale, @CreditSale, @CustomerReceipt, @TotalCollection,
                @CashRefundAdjusted, @MidDayCashSkim, @SystemExpectedCash,
                @PhysicalCashCounted, @CashDifference, @DifferenceReason,
                ISNULL(@Status, 'Closed'), @VoucherId, @CreatedBy, GETDATE()
            );

            SET @ShiftClosingId = SCOPE_IDENTITY();
            SELECT 'SUCCESS' AS Result, @ShiftClosingId AS ShiftClosingId, @ShiftNumber AS ShiftNumber;
        END

        ELSE IF(@_Operation = 'GETSALESDATASUMMARY')
        BEGIN
            DECLARE @StartDate DATETIME;
            DECLARE @EndDate DATETIME = GETDATE();

            IF @CounterSessionId IS NULL OR @CounterSessionId <= 0
            BEGIN
                RAISERROR('CounterSessionId is required for sales summary.', 16, 1);
                RETURN;
            END

            SELECT @StartDate = LoginTime
            FROM CounterSessions
            WHERE SessionId = @CounterSessionId
              AND BranchId = @BranchId
              AND UserId = @UserId;

            IF @StartDate IS NULL
            BEGIN
                RAISERROR('Counter session was not found.', 16, 1);
                RETURN;
            END

            ;WITH SessionSales AS
            (
                SELECT SM.*
                FROM SMaster SM
                WHERE SM.BranchId = @BranchId
                  AND SM.CompanyId = @CompanyId
                  AND SM.FinYearId = @FinYearId
                  AND SM.CounterSessionId = @CounterSessionId
                  AND (SM.CancelFlag = 0 OR SM.CancelFlag IS NULL)
                  AND SM.Status = 'Complete'
            ),
            PaymentRows AS
            (
                SELECT
                    SPD.BillNo,
                    SPD.Amount,
                    UPPER(REPLACE(ISNULL(PM.PayModeName, ''), ' ', '')) AS PayModeName
                FROM SPaymentDetails SPD
                INNER JOIN SessionSales SM
                    ON SM.BillNo = SPD.BillNo
                   AND SM.BranchId = SPD.BranchId
                   AND SM.CompanyId = SPD.CompanyId
                   AND SM.FinYearId = SPD.FinYearId
                LEFT JOIN PayMode PM
                    ON PM.PayModeID = SPD.PaymodeId
            ),
            PaymentSummary AS
            (
                SELECT
                    ISNULL(SUM(CASE WHEN PayModeName = 'CASH' THEN Amount ELSE 0 END), 0) AS CashSale,
                    ISNULL(SUM(CASE WHEN PayModeName = 'CARD' THEN Amount ELSE 0 END), 0) AS CardSale,
                    ISNULL(SUM(CASE WHEN PayModeName = 'UPI' THEN Amount ELSE 0 END), 0) AS UpiSale,
                    ISNULL(SUM(CASE WHEN PayModeName IN ('BANKTRANSFER', 'TRANSFER') THEN Amount ELSE 0 END), 0) AS BankTransferSale,
                    COUNT(DISTINCT CASE WHEN PayModeName = 'CASH' THEN BillNo END) AS CashBills,
                    COUNT(DISTINCT CASE WHEN PayModeName = 'CARD' THEN BillNo END) AS CardBills,
                    COUNT(DISTINCT CASE WHEN PayModeName = 'UPI' THEN BillNo END) AS UpiBills
                FROM PaymentRows
            ),
            FallbackSummary AS
            (
                SELECT
                    ISNULL(SUM(CASE WHEN UPPER(REPLACE(ISNULL(SM.PaymodeName, ''), ' ', '')) = 'CASH'
                                    THEN ISNULL(NULLIF(SM.ReceivedAmount, 0), SM.NetAmount) ELSE 0 END), 0) AS CashSale,
                    ISNULL(SUM(CASE WHEN UPPER(REPLACE(ISNULL(SM.PaymodeName, ''), ' ', '')) = 'CARD'
                                    THEN ISNULL(NULLIF(SM.ReceivedAmount, 0), SM.NetAmount) ELSE 0 END), 0) AS CardSale,
                    ISNULL(SUM(CASE WHEN UPPER(REPLACE(ISNULL(SM.PaymodeName, ''), ' ', '')) = 'UPI'
                                    THEN ISNULL(NULLIF(SM.ReceivedAmount, 0), SM.NetAmount) ELSE 0 END), 0) AS UpiSale,
                    ISNULL(SUM(CASE WHEN UPPER(REPLACE(ISNULL(SM.PaymodeName, ''), ' ', '')) = 'CREDIT'
                                    THEN SM.NetAmount ELSE 0 END), 0) AS CreditSale,
                    COUNT(DISTINCT CASE WHEN UPPER(REPLACE(ISNULL(SM.PaymodeName, ''), ' ', '')) = 'CASH' THEN SM.BillNo END) AS CashBills,
                    COUNT(DISTINCT CASE WHEN UPPER(REPLACE(ISNULL(SM.PaymodeName, ''), ' ', '')) = 'CARD' THEN SM.BillNo END) AS CardBills,
                    COUNT(DISTINCT CASE WHEN UPPER(REPLACE(ISNULL(SM.PaymodeName, ''), ' ', '')) = 'UPI' THEN SM.BillNo END) AS UpiBills
                FROM SessionSales SM
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM SPaymentDetails SPD
                    WHERE SPD.BillNo = SM.BillNo
                      AND SPD.BranchId = SM.BranchId
                      AND SPD.CompanyId = SM.CompanyId
                      AND SPD.FinYearId = SM.FinYearId
                )
            )
            SELECT
                ISNULL(SUM(SM.SubTotal), 0) AS TotalGrossSales,
                ISNULL(SUM(SM.DiscountAmt), 0) AS TotalDiscount,
                0.00 AS TotalReturn,
                ISNULL(SUM(SM.NetAmount), 0) AS NetSales,
                ISNULL(MAX(PS.CashSale), 0) + ISNULL(MAX(FS.CashSale), 0) AS CashSale,
                ISNULL(MAX(PS.CardSale), 0) + ISNULL(MAX(FS.CardSale), 0) AS CardSale,
                ISNULL(MAX(PS.UpiSale), 0) + ISNULL(MAX(FS.UpiSale), 0) AS UpiSale,
                ISNULL(MAX(FS.CreditSale), 0) AS CreditSale,
                ISNULL(MAX(PS.CashSale), 0) + ISNULL(MAX(FS.CashSale), 0)
                    + ISNULL(MAX(PS.CardSale), 0) + ISNULL(MAX(FS.CardSale), 0)
                    + ISNULL(MAX(PS.UpiSale), 0) + ISNULL(MAX(FS.UpiSale), 0)
                    + ISNULL(MAX(PS.BankTransferSale), 0) AS TotalCollection,
                COUNT(DISTINCT SM.BillNo) AS TotalBills,
                ISNULL(MAX(PS.CashBills), 0) + ISNULL(MAX(FS.CashBills), 0) AS CashBills,
                ISNULL(MAX(PS.CardBills), 0) + ISNULL(MAX(FS.CardBills), 0) AS CardBills,
                ISNULL(MAX(PS.UpiBills), 0) + ISNULL(MAX(FS.UpiBills), 0) AS UpiBills,
                @StartDate AS ShiftStartTime,
                @EndDate AS ShiftEndTime
            FROM SessionSales SM
            CROSS JOIN PaymentSummary PS
            CROSS JOIN FallbackSummary FS;
        END

        ELSE IF(@_Operation = 'GETCUSTOMERRECEIPTS')
        BEGIN
            DECLARE @ReceiptStartDate DATETIME;
            DECLARE @ReceiptEndDate DATETIME = GETDATE();

            IF @CounterSessionId IS NULL OR @CounterSessionId <= 0
            BEGIN
                RAISERROR('CounterSessionId is required for customer receipts summary.', 16, 1);
                RETURN;
            END

            SELECT @ReceiptStartDate = LoginTime
            FROM CounterSessions
            WHERE SessionId = @CounterSessionId
              AND BranchId = @BranchId
              AND UserId = @UserId;

            IF @ReceiptStartDate IS NULL
            BEGIN
                RAISERROR('Counter session was not found.', 16, 1);
                RETURN;
            END

            -- Add a 1-hour buffer on start and end times to accommodate potential client/server clock drifts
            DECLARE @BufferedStartDate DATETIME = DATEADD(hour, -1, @ReceiptStartDate);
            DECLARE @BufferedEndDate DATETIME = DATEADD(hour, 1, @ReceiptEndDate);

            SELECT 
                ISNULL(SUM(CASE WHEN L.GroupID = 14 OR UPPER(REPLACE(L.LedgerName, ' ', '')) = 'CASH' THEN CRM.ReceiptAmount ELSE 0 END), 0) AS CashReceipt,
                ISNULL(SUM(CASE WHEN UPPER(REPLACE(L.LedgerName, ' ', '')) LIKE '%CARD%' THEN CRM.ReceiptAmount ELSE 0 END), 0) AS CardReceipt,
                ISNULL(SUM(CASE WHEN UPPER(REPLACE(L.LedgerName, ' ', '')) LIKE '%UPI%' OR UPPER(REPLACE(L.LedgerName, ' ', '')) LIKE '%GPAY%' OR UPPER(REPLACE(L.LedgerName, ' ', '')) LIKE '%PHONEPE%' THEN CRM.ReceiptAmount ELSE 0 END), 0) AS UpiReceipt,
                ISNULL(SUM(CRM.ReceiptAmount), 0) AS TotalReceipt
            FROM CustomerReceiptMaster CRM
            LEFT JOIN LedgerMaster L ON L.LedgerID = CRM.PaymentMethodLedgerId AND L.BranchID = CRM.BranchId AND L.CompanyID = CRM.CompanyId
            WHERE CRM.BranchId = @BranchId
              AND CRM.CompanyId = @CompanyId
              AND CRM.UserId = @UserId
              AND CRM.VoucherDate >= @BufferedStartDate
              AND CRM.VoucherDate < @BufferedEndDate
              AND (CRM.CancelFlag = 0 OR CRM.CancelFlag IS NULL);
        END

        ELSE IF(@_Operation = 'GETALL')
        BEGIN
            IF @SortBy NOT IN ('ShiftClosingId', 'ClosingDate', 'Counter', 'NetSales', 'Status')
                SET @SortBy = 'ShiftClosingId';
            IF @SortByDirection NOT IN ('ASC', 'DESC')
                SET @SortByDirection = 'DESC';

            WITH ClosingCTE AS (
                SELECT SC.ShiftClosingId, SC.ClosingDate, SC.Counter, B.BranchName, U.UserName,
                       SC.NetSales, SC.TotalCollection, SC.PhysicalCashCounted, SC.CashDifference,
                       SC.Status, SC.ReportSelection,
                       ROW_NUMBER() OVER (
                           ORDER BY
                               CASE WHEN @SortByDirection = 'ASC' AND @SortBy = 'ShiftClosingId' THEN SC.ShiftClosingId END ASC,
                               CASE WHEN @SortByDirection = 'DESC' AND @SortBy = 'ShiftClosingId' THEN SC.ShiftClosingId END DESC,
                               CASE WHEN @SortByDirection = 'ASC' AND @SortBy = 'ClosingDate' THEN SC.ClosingDate END ASC,
                               CASE WHEN @SortByDirection = 'DESC' AND @SortBy = 'ClosingDate' THEN SC.ClosingDate END DESC
                       ) AS RowNum
                FROM ShiftClosing SC
                LEFT JOIN Branches B ON B.Id = SC.BranchId
                LEFT JOIN Users U ON U.UserID = SC.UserId
                WHERE SC.IsDelete = 0
                  AND SC.BranchId = ISNULL(@BranchId, SC.BranchId)
                  AND SC.Status = ISNULL(@Status, SC.Status)
            )
            SELECT * FROM ClosingCTE
            WHERE RowNum BETWEEN (@PageIndex * @PageSize + 1) AND ((@PageIndex + 1) * @PageSize);

            SELECT COUNT(*) AS TotalRecords
            FROM ShiftClosing
            WHERE IsDelete = 0
              AND BranchId = ISNULL(@BranchId, BranchId)
              AND Status = ISNULL(@Status, Status);
        END

        ELSE IF(@_Operation = 'GETBYID')
        BEGIN
            SELECT * FROM ShiftClosing WHERE ShiftClosingId = @ShiftClosingId;
            SELECT * FROM ShiftClosingDenominations WHERE ShiftClosingId = @ShiftClosingId ORDER BY No;
        END

        ELSE IF(@_Operation = 'UPDATE')
        BEGIN
            IF NOT EXISTS(SELECT 1 FROM ShiftClosing WHERE ShiftClosingId = @ShiftClosingId)
            BEGIN
                SELECT 'NOT_FOUND' AS Result;
                RETURN;
            END

            IF EXISTS(SELECT 1 FROM ShiftClosing WHERE ShiftClosingId = @ShiftClosingId AND Status = 'Closed')
            BEGIN
                SELECT 'ALREADY_CLOSED' AS Result;
                RETURN;
            END

            UPDATE ShiftClosing
            SET Counter = ISNULL(@Counter, Counter),
                CounterSessionId = ISNULL(@CounterSessionId, CounterSessionId),
                ClosingDate = ISNULL(@ClosingDate, ClosingDate),
                PhysicalCashCounted = @PhysicalCashCounted,
                CashDifference = @CashDifference,
                DifferenceReason = @DifferenceReason,
                Status = ISNULL(@Status, Status),
                ModifiedBy = @ModifiedBy,
                ModifiedDate = GETDATE()
            WHERE ShiftClosingId = @ShiftClosingId;

            SELECT 'SUCCESS' AS Result;
        END

        ELSE IF(@_Operation = 'DELETE')
        BEGIN
            IF EXISTS(SELECT 1 FROM ShiftClosing WHERE ShiftClosingId = @ShiftClosingId AND Status = 'Closed')
            BEGIN
                SELECT 'CANNOT_DELETE_CLOSED' AS Result;
                RETURN;
            END

            UPDATE ShiftClosing
            SET IsDelete = 1,
                ModifiedBy = @ModifiedBy,
                ModifiedDate = GETDATE()
            WHERE ShiftClosingId = @ShiftClosingId;

            SELECT 'SUCCESS' AS Result;
        END

        ELSE IF(@_Operation = 'SHIFTHISTORY')
        BEGIN
            SELECT ShiftClosingId, ClosingDate, DocNo, TotalCollection, TotalGrossSales, TotalReturn,
                   NetSales, CashSale, CardSale, UpiSale, CreditSale, CustomerReceipt, SystemExpectedCash,
                   PhysicalCashCounted, CashDifference, Status, ReportSelection, Counter
            FROM ShiftClosing
            WHERE IsDelete = 0
            ORDER BY ClosingDate DESC;
        END
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
