-- ====================================================================================
-- NEXORIS VENDOR RECONCILIATION & BALANCE SHEET HARMONIZATION SCRIPT
-- Target: Client Live Database & Test Databases (SQL Server 2012+)
--
-- PURPOSE:
--   1. Clears negative Sundry Creditor balances (vendor overpayments) to exactly ₹ 0.00.
--   2. Preserves physical CASH-IN-HAND strictly at its actual drawer balance.
--   3. Eliminates phantom legacy Stock Adjustment cash debits (e.g., 58,689 glitch).
--   4. Balances the General Ledger and Balance Sheet with DIFFERENCE = ₹ 0.00.
--   5. Idempotent & Safe: Can be previewed in DRY RUN mode or run multiple times.
-- ====================================================================================

SET NOCOUNT ON;

-- ------------------------------------------------------------------------------------
-- CONFIGURATION:
-- Set @DryRun = 1 to test and PREVIEW the changes without saving.
-- Set @DryRun = 0 to APPLY and COMMIT the changes to the live database.
-- ------------------------------------------------------------------------------------
DECLARE @DryRun BIT = 0; 

PRINT '====================================================================================';
PRINT '>>> Starting Nexoris Vendor Reconciliation & Balance Sheet Harmonization...';
PRINT '>>> Mode: ' + CASE WHEN @DryRun = 1 THEN 'DRY RUN (Preview Only - No Changes Saved)' ELSE 'LIVE EXECUTION (Changes will be committed)' END;
PRINT '====================================================================================';

BEGIN TRANSACTION;

BEGIN TRY
    -- 1. Identify distinct branches from Vouchers
    DECLARE @Branches TABLE (BranchID INT);
    INSERT INTO @Branches (BranchID)
    SELECT DISTINCT BranchID FROM Vouchers WHERE BranchID > 0;

    IF NOT EXISTS (SELECT 1 FROM @Branches)
        INSERT INTO @Branches (BranchID) VALUES (1);

    DECLARE @CurBranchId INT;
    DECLARE branch_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT BranchID FROM @Branches;
    OPEN branch_cursor;
    FETCH NEXT FROM branch_cursor INTO @CurBranchId;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        PRINT '';
        PRINT '------------------------------------------------------------------------------------';
        PRINT '>>> Processing Branch ID: ' + CAST(@CurBranchId AS VARCHAR);
        PRINT '------------------------------------------------------------------------------------';

        -- A. Resolve CompanyID and FinYearID for this branch
        DECLARE @CompanyId INT = 1;
        DECLARE @FinYearId INT = 1;
        SELECT TOP 1 @CompanyId = CompanyID, @FinYearId = FinYearID 
        FROM Vouchers 
        WHERE BranchID = @CurBranchId AND CompanyID > 0;

        -- B. Resolve Capital Ledger (GroupID = 1)
        DECLARE @CapitalLedgerId INT = 0;
        DECLARE @CapitalLedgerName NVARCHAR(100) = '';
        SELECT TOP 1 
            @CapitalLedgerId = LedgerID, 
            @CapitalLedgerName = LedgerName 
        FROM LedgerMaster 
        WHERE GroupID = 1 AND BranchID = @CurBranchId
        ORDER BY CASE WHEN LedgerName LIKE '%OWNER%' OR LedgerName LIKE '%CAPITAL%' THEN 0 ELSE 1 END, LedgerID;

        IF @CapitalLedgerId = 0
        BEGIN
            PRINT 'WARNING: No Capital Ledger found for Branch ' + CAST(@CurBranchId AS VARCHAR) + '. Skipping branch.';
            FETCH NEXT FROM branch_cursor INTO @CurBranchId;
            CONTINUE;
        END

        PRINT 'Resolved Capital Ledger: ' + @CapitalLedgerName + ' (LedgerID: ' + CAST(@CapitalLedgerId AS VARCHAR) + ')';

        -- C. Resolve Cash-in-Hand Ledger (GroupID = 14)
        DECLARE @CashLedgerId INT = 0;
        DECLARE @CashLedgerName NVARCHAR(100) = '';
        SELECT TOP 1 
            @CashLedgerId = LedgerID, 
            @CashLedgerName = LedgerName 
        FROM LedgerMaster 
        WHERE GroupID = 14 AND BranchID = @CurBranchId
        ORDER BY CASE WHEN LedgerName = 'CASH-IN-HAND' THEN 0 ELSE 1 END, LedgerID;

        DECLARE @CashBefore DECIMAL(18,2) = 0;
        IF @CashLedgerId > 0
        BEGIN
            SELECT @CashBefore = ISNULL(SUM(Debit - Credit), 0) 
            FROM Vouchers 
            WHERE LedgerID = @CashLedgerId AND BranchID = @CurBranchId AND ISNULL(CancelFlag, 0) = 0;
            PRINT 'Actual Cash-In-Hand before reconciliation: ' + CAST(@CashBefore AS VARCHAR);
        END

        -- D. Get Next VoucherID
        DECLARE @MaxVoucherId INT = 0;
        SELECT @MaxVoucherId = ISNULL(MAX(VoucherID), 0) + 1 FROM Vouchers;

        -- E. Reconcile Negative Sundry Creditors (Overpaid vendors)
        DECLARE @TotalVendorAdj DECIMAL(18,2) = 0;
        DECLARE @VendorCount INT = 0;
        DECLARE @SlNo INT = 1;
        DECLARE @VendorId INT, @VendorName NVARCHAR(100), @NegAmount DECIMAL(18,2);

        DECLARE vendor_cur CURSOR LOCAL FAST_FORWARD FOR
        SELECT 
            l.LedgerID,
            l.LedgerName,
            ABS((ISNULL(l.OpnCredit,0) + ISNULL(v.Credit,0)) - (ISNULL(l.OpnDebit,0) + ISNULL(v.Debit,0))) AS NegAmount
        FROM LedgerMaster l
        LEFT JOIN (
            SELECT LedgerID, SUM(Debit) AS Debit, SUM(Credit) AS Credit
            FROM Vouchers 
            WHERE BranchID = @CurBranchId AND ISNULL(CancelFlag, 0) = 0
            GROUP BY LedgerID
        ) v ON l.LedgerID = v.LedgerID
        WHERE l.GroupID = 17 AND l.BranchID = @CurBranchId
          AND ((ISNULL(l.OpnCredit,0) + ISNULL(v.Credit,0)) - (ISNULL(l.OpnDebit,0) + ISNULL(v.Debit,0))) < -0.01;

        OPEN vendor_cur;
        FETCH NEXT FROM vendor_cur INTO @VendorId, @VendorName, @NegAmount;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            PRINT '  * Overpaid Vendor: ' + @VendorName + ' | Current Balance: -' + CAST(@NegAmount AS VARCHAR) + ' -> Adjusting to 0.00';

            INSERT INTO Vouchers (
                CompanyID, BranchID, VoucherID, VoucherSeriesID, VoucherDate, VoucherNumber, 
                LedgerID, LedgerName, VoucherType, GroupID, Debit, Credit, Narration, 
                SlNo, Mode, ModeID, UserDate, UserName, UserID, CancelFlag, FinYearID, IsSyncd, CounterID
            )
            VALUES (
                @CompanyId, @CurBranchId, @MaxVoucherId, 0, CAST(GETDATE() AS DATE), 'VEND-RECON', 
                @VendorId, @VendorName, 'Journal', 17, 0, @NegAmount, 'Reconcile vendor overpayment to zero', 
                @SlNo, '', 0, GETDATE(), 'SYSTEM_RECON', 1, 0, @FinYearId, 0, 1
            );

            SET @SlNo = @SlNo + 1;
            SET @TotalVendorAdj = @TotalVendorAdj + @NegAmount;
            SET @VendorCount = @VendorCount + 1;

            FETCH NEXT FROM vendor_cur INTO @VendorId, @VendorName, @NegAmount;
        END

        CLOSE vendor_cur;
        DEALLOCATE vendor_cur;

        IF @TotalVendorAdj > 0
        BEGIN
            -- Offset overpaid vendors against Equity/Capital
            INSERT INTO Vouchers (
                CompanyID, BranchID, VoucherID, VoucherSeriesID, VoucherDate, VoucherNumber, 
                LedgerID, LedgerName, VoucherType, GroupID, Debit, Credit, Narration, 
                SlNo, Mode, ModeID, UserDate, UserName, UserID, CancelFlag, FinYearID, IsSyncd, CounterID
            )
            VALUES (
                @CompanyId, @CurBranchId, @MaxVoucherId, 0, CAST(GETDATE() AS DATE), 'VEND-RECON', 
                @CapitalLedgerId, @CapitalLedgerName, 'Journal', 1, @TotalVendorAdj, 0, 'Offset vendor overpayments against equity', 
                @SlNo, '', 0, GETDATE(), 'SYSTEM_RECON', 1, 0, @FinYearId, 0, 1
            );

            PRINT '  -> Successfully posted Journal Voucher #' + CAST(@MaxVoucherId AS VARCHAR) + ' clearing ' + CAST(@VendorCount AS VARCHAR) + ' overpaid vendors (Total: ' + CAST(@TotalVendorAdj AS VARCHAR) + ').';
        END
        ELSE
        BEGIN
            PRINT '  -> No negative creditor balances found for Branch ' + CAST(@CurBranchId AS VARCHAR) + '.';
        END

        -- F. Cancel Phantom Stock Adjustment Cash Debits while strictly preserving Cash-in-Hand
        DECLARE @PhantomCashAmount DECIMAL(18,2) = 0;
        SELECT @PhantomCashAmount = ISNULL(SUM(Debit - Credit), 0)
        FROM Vouchers v
        INNER JOIN LedgerMaster lm ON v.LedgerID = lm.LedgerID AND v.BranchID = lm.BranchID
        WHERE v.VoucherType = 'PhysicalStock' 
          AND (lm.GroupID = 14 OR lm.LedgerName = 'CASH-IN-HAND' OR lm.LedgerName = 'CASH')
          AND ISNULL(v.CancelFlag, 0) = 0
          AND v.BranchID = @CurBranchId;

        IF @PhantomCashAmount <> 0
        BEGIN
            PRINT '  * Found Phantom Stock Adjustment Cash Debits: ' + CAST(@PhantomCashAmount AS VARCHAR);

            -- 1. Cancel the faulty legacy rows
            UPDATE v
            SET v.CancelFlag = 1
            FROM Vouchers v
            INNER JOIN LedgerMaster lm ON v.LedgerID = lm.LedgerID AND v.BranchID = lm.BranchID
            WHERE v.VoucherType = 'PhysicalStock' 
              AND (lm.GroupID = 14 OR lm.LedgerName = 'CASH-IN-HAND' OR lm.LedgerName = 'CASH')
              AND ISNULL(v.CancelFlag, 0) = 0
              AND v.BranchID = @CurBranchId;

            -- 2. Preserve EXACT physical Cash In Hand by posting a balanced replacement against Capital
            SET @MaxVoucherId = @MaxVoucherId + 1;

            INSERT INTO Vouchers (
                CompanyID, BranchID, VoucherID, VoucherSeriesID, VoucherDate, VoucherNumber, 
                LedgerID, LedgerName, VoucherType, GroupID, Debit, Credit, Narration, 
                SlNo, Mode, ModeID, UserDate, UserName, UserID, CancelFlag, FinYearID, IsSyncd, CounterID
            )
            VALUES (
                @CompanyId, @CurBranchId, @MaxVoucherId, 0, CAST(GETDATE() AS DATE), 'CASH-PRESERVE', 
                @CashLedgerId, @CashLedgerName, 'Journal', 14, @PhantomCashAmount, 0, 'Preserve actual physical cash balance', 
                1, '', 0, GETDATE(), 'SYSTEM_RECON', 1, 0, @FinYearId, 0, 1
            );

            INSERT INTO Vouchers (
                CompanyID, BranchID, VoucherID, VoucherSeriesID, VoucherDate, VoucherNumber, 
                LedgerID, LedgerName, VoucherType, GroupID, Debit, Credit, Narration, 
                SlNo, Mode, ModeID, UserDate, UserName, UserID, CancelFlag, FinYearID, IsSyncd, CounterID
            )
            VALUES (
                @CompanyId, @CurBranchId, @MaxVoucherId, 0, CAST(GETDATE() AS DATE), 'CASH-PRESERVE', 
                @CapitalLedgerId, @CapitalLedgerName, 'Journal', 1, 0, @PhantomCashAmount, 'Preserve actual physical cash balance', 
                2, '', 0, GETDATE(), 'SYSTEM_RECON', 1, 0, @FinYearId, 0, 1
            );

            PRINT '  -> Cancelled phantom stock cash entries and preserved physical Cash-In-Hand at exact drawer value.';
        END

        -- G. Verify Cash-in-Hand After Fix
        IF @CashLedgerId > 0
        BEGIN
            DECLARE @CashAfter DECIMAL(18,2) = 0;
            SELECT @CashAfter = ISNULL(SUM(Debit - Credit), 0) 
            FROM Vouchers 
            WHERE LedgerID = @CashLedgerId AND BranchID = @CurBranchId AND ISNULL(CancelFlag, 0) = 0;
            
            PRINT '  -> Cash-In-Hand AFTER reconciliation: ' + CAST(@CashAfter AS VARCHAR) + ' (Difference: ' + CAST((@CashAfter - @CashBefore) AS VARCHAR) + ')';
        END

        FETCH NEXT FROM branch_cursor INTO @CurBranchId;
    END

    CLOSE branch_cursor;
    DEALLOCATE branch_cursor;

    -- Final Validation Check: Total Debits vs Total Credits across all Vouchers
    DECLARE @TotalDebit DECIMAL(18,2) = 0, @TotalCredit DECIMAL(18,2) = 0, @Diff DECIMAL(18,2) = 0;
    SELECT 
        @TotalDebit = ISNULL(SUM(Debit), 0),
        @TotalCredit = ISNULL(SUM(Credit), 0),
        @Diff = ISNULL(SUM(Debit), 0) - ISNULL(SUM(Credit), 0)
    FROM Vouchers 
    WHERE ISNULL(CancelFlag, 0) = 0;

    PRINT '';
    PRINT '====================================================================================';
    PRINT '>>> FINAL GENERAL LEDGER AUDIT CHECK:';
    PRINT '    Total Debits:  ' + CAST(@TotalDebit AS VARCHAR);
    PRINT '    Total Credits: ' + CAST(@TotalCredit AS VARCHAR);
    PRINT '    Difference:    ' + CAST(@Diff AS VARCHAR);
    PRINT '====================================================================================';

    IF ABS(@Diff) < 0.01
    BEGIN
        PRINT '>>> STATUS: SUCCESS! All accounts are 100% mathematically balanced.';
    END
    ELSE
    BEGIN
        PRINT '>>> STATUS: WARNING - General ledger has a difference of: ' + CAST(@Diff AS VARCHAR);
    END

    IF @DryRun = 1
    BEGIN
        ROLLBACK TRANSACTION;
        PRINT '>>> DRY RUN COMPLETE: All changes have been rolled back safely. Set @DryRun = 0 to execute.';
    END
    ELSE
    BEGIN
        COMMIT TRANSACTION;
        PRINT '>>> LIVE EXECUTION COMPLETE: All reconciliation adjustments successfully committed to the database.';
    END
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT '!!! ERROR OCCURRED: ' + ERROR_MESSAGE() + ' (Line: ' + CAST(ERROR_LINE() AS VARCHAR) + ')';
    PRINT 'Transaction rolled back safely.';
END CATCH;
GO
