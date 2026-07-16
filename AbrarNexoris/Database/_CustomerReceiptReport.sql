USE [RambaiTest]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROC [dbo].[_CustomerReceiptReport]
(
    @BranchId INT,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @CustomerLedgerId INT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @FromDate IS NULL
        SET @FromDate = '19000101';

    IF @ToDate IS NULL
        SET @ToDate = '99991231';

    SELECT
        CRM.VoucherId,
        CRM.VoucherDate,
        CRM.BillNoUntil AS BillNo,
        CRM.CustomerLedgerId,
        ISNULL(LM.LedgerName, '') AS CustomerName,
        CAST(ISNULL(CRM.ReceivableAmount, 0) AS DECIMAL(18, 2)) AS TotalAmount,
        CAST(ISNULL(CRM.ReceiptAmount, 0) AS DECIMAL(18, 2)) AS ReceiptAmount,
        CAST(ISNULL(CRM.ReceivableAmount, 0) - ISNULL(CRM.ReceiptAmount, 0) AS DECIMAL(18, 2)) AS Balance
    FROM CustomerReceiptMaster AS CRM
    LEFT JOIN LedgerMaster AS LM
        ON LM.LedgerID = CRM.CustomerLedgerId
    WHERE CRM.CancelFlag = 0
      AND CRM.BranchId = @BranchId
      AND CAST(CRM.VoucherDate AS DATE) >= @FromDate
      AND CAST(CRM.VoucherDate AS DATE) <= @ToDate
      AND (@CustomerLedgerId = 0 OR CRM.CustomerLedgerId = @CustomerLedgerId)
    ORDER BY CRM.VoucherDate DESC, CRM.VoucherId DESC;
END
GO
