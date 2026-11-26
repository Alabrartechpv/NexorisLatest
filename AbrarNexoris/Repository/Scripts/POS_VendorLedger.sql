SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.POS_VendorLedger', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo.POS_VendorLedger AS RETURN 0;');
GO

ALTER PROCEDURE dbo.POS_VendorLedger
    @CompanyId INT,
    @BranchId  INT,
    @FinYearId INT,
    @LedgerId  INT,
    @FromDate  DATE,
    @ToDate    DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF @FromDate IS NULL OR @ToDate IS NULL OR @FromDate > @ToDate
        OR @FromDate < '17530101' OR @ToDate = '99991231'
        THROW 50001, 'Enter a valid date range for the Vendor Ledger Statement.', 1;

    DECLARE @LedgerOpening DECIMAL(28, 4);
    SELECT @LedgerOpening = ISNULL(OpnDebit, 0) - ISNULL(OpnCredit, 0)
    FROM dbo.LedgerMaster
    WHERE LedgerID = @LedgerId
      AND CompanyID = @CompanyId
      AND BranchID = @BranchId
      AND GroupID = 17; -- Sundry Creditors, the group used by the vendor picker.

    IF @LedgerOpening IS NULL
        THROW 50002, 'The selected ledger is not a vendor for the current company and branch.', 1;

    DECLARE @FromStart DATETIME = CONVERT(DATETIME, @FromDate);
    DECLARE @ToEnd DATETIME = DATEADD(DAY, 1, CONVERT(DATETIME, @ToDate));
    DECLARE @OpeningBalance DECIMAL(28, 2);

    SELECT @OpeningBalance = @LedgerOpening + ISNULL(SUM(
        CONVERT(DECIMAL(28, 4), ISNULL(Debit, 0))
        - CONVERT(DECIMAL(28, 4), ISNULL(Credit, 0))), 0)
    FROM dbo.Vouchers
    WHERE CompanyID = @CompanyId
      AND BranchID = @BranchId
      AND FinYearID = @FinYearId
      AND LedgerID = @LedgerId
      AND VoucherDate < @FromStart
      AND ISNULL(CancelFlag, 0) = 0;

    SELECT VoucherID, VoucherDate, SlNo,
           ISNULL(VoucherNumber, N'') AS VoucherNo,
           ISNULL(VoucherType, N'') AS VoucherTypeName,
           ISNULL(Narration, N'') AS Narration,
           CONVERT(DECIMAL(28, 2), ISNULL(Debit, 0)) AS ReceiptAmount,
           CONVERT(DECIMAL(28, 2), ISNULL(Credit, 0)) AS PaymentAmount
    INTO #Period
    FROM dbo.Vouchers
    WHERE CompanyID = @CompanyId
      AND BranchID = @BranchId
      AND FinYearID = @FinYearId
      AND LedgerID = @LedgerId
      AND VoucherDate >= @FromStart
      AND VoucherDate < @ToEnd
      AND ISNULL(CancelFlag, 0) = 0;

    -- Resolve particulars from the opposing postings in the same accounting scope.
    SELECT p.VoucherID, p.VoucherTypeName,
           MIN(COALESCE(NULLIF(l.LedgerName, N''), NULLIF(v.LedgerName, N''))) AS LedgerName
    INTO #Opposing
    FROM (SELECT DISTINCT VoucherID, VoucherTypeName FROM #Period) p
    JOIN dbo.Vouchers v
      ON v.VoucherID = p.VoucherID AND ISNULL(v.VoucherType, N'') = p.VoucherTypeName
     AND v.CompanyID = @CompanyId AND v.BranchID = @BranchId AND v.FinYearID = @FinYearId
     AND v.LedgerID <> @LedgerId AND ISNULL(v.CancelFlag, 0) = 0
    LEFT JOIN dbo.LedgerMaster l
      ON l.LedgerID = v.LedgerID AND l.CompanyID = v.CompanyID AND l.BranchID = v.BranchID
    GROUP BY p.VoucherID, p.VoucherTypeName;

    -- Same debit-minus-credit display used by the Customer Ledger Statement.
    SELECT p.VoucherID, p.VoucherDate, p.VoucherNo, p.VoucherTypeName,
           ISNULL(o.LedgerName, N'Various Accounts') AS Particulars,
           p.Narration, p.ReceiptAmount, p.PaymentAmount,
           @OpeningBalance + CONVERT(DECIMAL(28, 2), SUM(p.ReceiptAmount - p.PaymentAmount) OVER (
               ORDER BY p.VoucherDate, p.VoucherID, p.VoucherTypeName, p.SlNo, p.ReceiptAmount, p.PaymentAmount
               ROWS UNBOUNDED PRECEDING)) AS RunningBalance
    FROM #Period p
    LEFT JOIN #Opposing o ON o.VoucherID = p.VoucherID AND o.VoucherTypeName = p.VoucherTypeName
    ORDER BY p.VoucherDate, p.VoucherID, p.VoucherTypeName, p.SlNo, p.ReceiptAmount, p.PaymentAmount;

    DECLARE @TotalDebit DECIMAL(28, 2), @TotalCredit DECIMAL(28, 2);
    SELECT @TotalDebit = ISNULL(SUM(ReceiptAmount), 0), @TotalCredit = ISNULL(SUM(PaymentAmount), 0)
    FROM #Period;

    SELECT @OpeningBalance AS OpeningBalance,
           @TotalDebit AS TotalReceipts,
           @TotalCredit AS TotalPayments,
           @OpeningBalance + @TotalDebit - @TotalCredit AS ClosingBalance;

    ;WITH PostedDocuments AS
    (
        SELECT DISTINCT VoucherID, VoucherTypeName, VoucherDate FROM #Period
    ),
    Allocations AS
    (
        SELECT p.VoucherID, p.VoucherTypeName, p.VoucherDate,
               N'Vendor Payment' AS Document, d.BillNo, d.BillDate,
               d.BillAmount AS Amount, d.PaymentAmount AS Applied, d.BalanceAmount AS BillBalance
        FROM PostedDocuments p
        JOIN dbo.VendorPaymentMaster m
          ON p.VoucherTypeName IN (N'VENDPAY', N'Vendor Payment')
         AND m.VoucherId = p.VoucherID AND CONVERT(date, m.VoucherDate) = CONVERT(date, p.VoucherDate)
         AND m.CompanyId = @CompanyId AND m.BranchId = @BranchId AND m.VendorLedgerId = @LedgerId
         AND ISNULL(m.CancelFlag, 0) = 0
        JOIN dbo.VendorPaymentDetails d
          ON d.PaymentMasterId = m.Id AND d.BranchId = m.BranchId
         AND d.BillNo > 0 AND ISNULL(d.CancelFlag, 0) = 0

        UNION ALL

        SELECT p.VoucherID, p.VoucherTypeName, p.VoucherDate,
               N'Debit Note', d.BillNo, d.BillDate, d.BillAmount, d.DebitAmount, d.BalanceAmount
        FROM PostedDocuments p
        JOIN dbo.DebitNoteMaster m
          ON p.VoucherTypeName = N'Debit Note'
         AND m.VoucherId = p.VoucherID AND m.VendorLedgerId = @LedgerId
         AND m.CompanyId = @CompanyId AND m.BranchId = @BranchId AND m.FinYearId = @FinYearId
         AND ISNULL(m.CancelFlag, 0) = 0
        JOIN dbo.DebitNoteDetails d
          ON d.DebitNoteMasterId = m.Id AND d.BranchId = m.BranchId AND d.FinYearId = m.FinYearId
         AND d.BillNo > 0 AND ISNULL(d.CancelFlag, 0) = 0
    ),
    BillRows AS
    (
        SELECT p.VoucherID, p.VoucherTypeName, p.VoucherDate,
               N'Purchase' AS Document, m.PurchaseNo AS BillNo, m.InvoiceNo AS InvoiceNumber,
               COALESCE(m.InvoiceDate, m.PurchaseDate) AS BillDate,
               CASE WHEN d.PurchaseNo IS NULL THEN N'Purchase bill'
                    ELSE CONCAT(d.ItemName, CASE WHEN NULLIF(d.Unit, '') IS NOT NULL THEN CONCAT(N' (', d.Unit, N')') END)
               END AS Detail,
               d.Qty AS Quantity,
               CASE WHEN d.PurchaseNo IS NULL THEN m.GrandTotal
                    ELSE ISNULL(d.Qty, 0) * ISNULL(d.Cost, 0) + ISNULL(d.TaxAmt, 0)
               END AS Amount,
               CAST(NULL AS float) AS Applied, CAST(NULL AS float) AS BillBalance,
               d.SlNo AS LineNumber
        FROM PostedDocuments p
        JOIN dbo.PMaster m
          ON p.VoucherTypeName = N'Purchase' AND m.VoucherID = p.VoucherID AND m.LedgerID = @LedgerId
         AND m.CompanyId = @CompanyId AND m.BranchId = @BranchId AND m.FinYearId = @FinYearId
         AND ISNULL(m.CancelFlag, 0) = 0
        LEFT JOIN dbo.PDetails d
          ON d.PurchaseNo = m.PurchaseNo AND d.CompanyId = m.CompanyId
         AND d.BranchID = m.BranchId AND d.FinYearId = m.FinYearId
         AND ISNULL(d.SeriesID, 0) = ISNULL(m.SeriesID, 0)

        UNION ALL

        SELECT a.VoucherID, a.VoucherTypeName, a.VoucherDate,
               a.Document, a.BillNo, original.InvoiceNo, COALESCE(a.BillDate, original.PurchaseDate),
               N'Applied to purchase bill', NULL, a.Amount, a.Applied, a.BillBalance, NULL
        FROM Allocations a
        OUTER APPLY
        (
            SELECT TOP (1) m.InvoiceNo, m.PurchaseDate
            FROM dbo.PMaster m
            WHERE m.PurchaseNo = a.BillNo AND m.LedgerID = @LedgerId
              AND m.CompanyId = @CompanyId AND m.BranchId = @BranchId
              AND CONVERT(date, m.PurchaseDate) <= CONVERT(date, a.VoucherDate)
              AND ISNULL(m.CancelFlag, 0) = 0
            ORDER BY CASE WHEN CONVERT(date, m.PurchaseDate) = CONVERT(date, a.BillDate) THEN 0 ELSE 1 END,
                     m.PurchaseDate DESC, m.FinYearId DESC, m.Pid DESC
        ) original

        UNION ALL

        SELECT p.VoucherID, p.VoucherTypeName, p.VoucherDate,
               N'Purchase Return', m.PReturnNo, m.InvoiceNo, m.PReturnDate,
               N'Purchase return document', NULL, m.GrandTotal, NULL, NULL, NULL
        FROM PostedDocuments p
        JOIN dbo.PReturnMaster m
          ON p.VoucherTypeName IN (N'PR', N'Purchase Return') AND m.VoucherID = p.VoucherID
         AND m.LedgerID = @LedgerId AND m.CompanyId = @CompanyId AND m.BranchId = @BranchId AND m.FinYearId = @FinYearId
         AND ISNULL(m.CancelFlag, 0) = 0

        UNION ALL

        SELECT p.VoucherID, p.VoucherTypeName, p.VoucherDate,
               N'Purchase Return', r.PReturnNo, r.InvoiceNo, r.PReturnDate,
               N'Purchase return linked to the debit note', NULL, r.GrandTotal, m.DebitAmount, NULL, NULL
        FROM PostedDocuments p
        JOIN dbo.DebitNoteMaster m
          ON p.VoucherTypeName = N'Debit Note'
         AND m.VoucherId = p.VoucherID AND m.VendorLedgerId = @LedgerId
         AND m.CompanyId = @CompanyId AND m.BranchId = @BranchId AND m.FinYearId = @FinYearId
         AND ISNULL(m.CancelFlag, 0) = 0
        JOIN dbo.PReturnMaster r
          ON r.PReturnNo = m.PReturnNo AND r.LedgerID = m.VendorLedgerId
         AND r.CompanyId = m.CompanyId AND r.BranchId = m.BranchId AND r.FinYearId = m.FinYearId
         AND ISNULL(r.CancelFlag, 0) = 0
    )
    SELECT VoucherID, VoucherTypeName, VoucherDate, Document,
           CONVERT(nvarchar(30), BillNo) AS BillNumber, InvoiceNumber, BillDate, Detail,
           CONVERT(decimal(28, 3), Quantity) AS Quantity,
           CONVERT(decimal(28, 2), Amount) AS Amount,
           CONVERT(decimal(28, 2), Applied) AS Applied,
           CONVERT(decimal(28, 2), BillBalance) AS BillBalance
    FROM BillRows
    ORDER BY VoucherDate, VoucherID, VoucherTypeName, BillNo, LineNumber;
END;
GO

IF OBJECT_ID(N'dbo.Forms', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.Forms WHERE FormKey = N'vendorledger')
BEGIN
    INSERT dbo.Forms (FormKey, FormName, Category, IsActive)
    VALUES (N'vendorledger', N'Vendor Ledger Statement', N'Reports', 1);
END;
GO
