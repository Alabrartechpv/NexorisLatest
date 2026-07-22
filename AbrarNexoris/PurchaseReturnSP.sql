
ALTER PROCEDURE [dbo].[_POS_PurchaseReturn]
(
    @Id int = NULL,
    @CompanyId int = NULL,
    @FinYearId int = NULL,
    @BranchId int = NULL,
    @BranchName varchar(50) = NULL,
    @PReturnNo int = NULL,
    @PInvoice varchar(50) = NULL,
    @InvoiceNo varchar(50) = NULL,
    @InvoiceDate datetime = NULL,
    @PReturnDate datetime = NULL,
    @LedgerID int = NULL,
    @VendorName varchar(MAX) = NULL,
    @PaymodeID int = NULL,
    @Paymode varchar(50) = NULL,
    @PaymodeLedgerID int = NULL,
    @CreditPeriod int = NULL,
    @SubTotal float = NULL,
    @SpDisPer float = NULL,
    @SpDsiAmt float = NULL,
    @BillDiscountPer float = NULL,
    @BillDiscountAmt float = NULL,
    @TaxPer float = NULL,
    @TaxAmt float = NULL,
    @Frieght float = NULL,
    @ExpenseAmt float = NULL,
    @OtherExpAmt float = NULL,
    @GrandTotal float = NULL,
    @CancelFlag bit = NULL,
    @UserID int = NULL,
    @UserName varchar(50) = NULL,
    @TaxType varchar(10) = NULL,
    @Remarks varchar(MAX) = NULL,
    @RoundOff float = NULL,
    @CessPer float = NULL,
    @CessAmt float = NULL,
    @CalAfterTax float = NULL,
    @CurrencyID int = NULL,
    @CurSymbol varchar(10) = NULL,
    @SeriesID int = NULL,
    @VoucherID int = NULL,
    @TrnsType varchar(50) = NULL,
    @VoucherType varchar(50) = NULL,
    @PurchaseNo int = NULL,
    @barcode varchar(50) = NULL,
    @_Operation varchar(50) = NULL
)
AS
SET NOCOUNT ON
BEGIN
    IF(@_Operation = 'GENERATENUMBER')
    BEGIN
        SET @PReturnNo = (SELECT ISNULL(PRBillNo + 1, 1) FROM TrackTrans WHERE BranchID = @BranchId AND FinYearID = @FinYearId)
        SELECT @PReturnNo
    END
    ELSE IF(@_Operation = 'CREATE')
    BEGIN
        SET @BranchName = (SELECT BranchName FROM Branches WHERE Id = @BranchId)
        SET @VendorName = (SELECT LedgerName FROM LedgerMaster WHERE LedgerID = @LedgerID)
        SET @Paymode = (SELECT PayModeName FROM PayMode WHERE PayModeID = @PaymodeID)
        SET @UserName = (SELECT UserName FROM Users WHERE UserID = @UserID)
        SET @CurrencyID = (SELECT Currency FROM CompanyInfo WHERE CompanyID = @CompanyId)
        SET @CurSymbol = (SELECT CurrencySymbol FROM Currency WHERE CurrencyID = @CurrencyID)
        
        INSERT INTO PReturnMaster(
            CompanyId, FinYearId, BranchId, BranchName, PReturnNo, PReturnDate, PInvoice, InvoiceNo, InvoiceDate,
            LedgerID, VendorName, PaymodeID, Paymode, PaymodeLedgerID, CreditPeriod, SubTotal, SpDisPer,
            SpDsiAmt, BillDiscountPer, BillDiscountAmt, TaxPer, TaxAmt, Frieght, ExpenseAmt, OtherExpAmt,
            GrandTotal, CancelFlag, UserID, UserName, TaxType, Remarks, RoundOff, CessPer, CessAmt, CalAfterTax, 
            CurrencyID, CurSymbol, SeriesID, VoucherID, TrnsType, VoucherType
        )
        VALUES (
            @CompanyId, @FinYearId, @BranchId, @BranchName, @PReturnNo, @PReturnDate, @PInvoice, @InvoiceNo, @InvoiceDate,
            @LedgerID, @VendorName, @PaymodeID, @Paymode, @PaymodeLedgerID, @CreditPeriod, @SubTotal, @SpDisPer,
            @SpDsiAmt, @BillDiscountPer, @BillDiscountAmt, @TaxPer, @TaxAmt, @Frieght, @ExpenseAmt, @OtherExpAmt,
            @GrandTotal, @CancelFlag, @UserID, @UserName, @TaxType, @Remarks, @RoundOff, @CessPer, @CessAmt, @CalAfterTax, 
            @CurrencyID, @CurSymbol, @SeriesID, @VoucherID, @TrnsType, @VoucherType
        )
        
        UPDATE TrackTrans SET PRBillNo = @PReturnNo WHERE BranchID = @BranchId AND FinYearID = @FinYearId
        SELECT @PReturnNo
    END
    ELSE IF(@_Operation = 'GETALL')
BEGIN
    SELECT 
        Id,
        BranchName, 
        PReturnNo, 
        PReturnDate, 
        InvoiceNo,
        LedgerID,
        VendorName,
        GrandTotal 
    FROM PReturnMaster
    WHERE CancelFlag = 0 
        AND PReturnNo LIKE '%'+ ISNULL(CAST(@PReturnNo AS nvarchar(20)), '') +'%' 
        AND BranchId = CASE WHEN @BranchId IS NOT NULL AND @BranchId <> 0 THEN @BranchId ELSE BranchId END 
        AND CompanyId = CASE WHEN @CompanyId IS NOT NULL AND @CompanyId <> 0 THEN @CompanyId ELSE CompanyId END
    ORDER BY PReturnNo DESC
END
    ELSE IF(@_Operation = 'GETBYID')
    BEGIN
        SET @CompanyId = (SELECT CompanyId FROM PReturnMaster WHERE Id = @Id)
        SET @FinYearId = (SELECT FinYearId FROM PReturnMaster WHERE Id = @Id)
        SET @BranchId = (SELECT BranchId FROM PReturnMaster WHERE Id = @Id)
        SET @PReturnNo = (SELECT PReturnNo FROM PReturnMaster WHERE Id = @Id)
        
        -- FIXED: Only try to get Pid if InvoiceNo is numeric, otherwise keep original value
        DECLARE @OriginalInvoiceNo VARCHAR(50)
        SET @OriginalInvoiceNo = (SELECT InvoiceNo FROM PReturnMaster WHERE Id = @Id)
        
        IF (ISNUMERIC(@OriginalInvoiceNo) = 1)
        BEGIN
            SET @InvoiceNo = (SELECT Pid FROM PMaster WHERE BranchId = @BranchId AND PurchaseNo = CAST(@OriginalInvoiceNo AS INT))
        END
        ELSE
        BEGIN
            SET @InvoiceNo = @OriginalInvoiceNo
        END
        
        SELECT Id, BranchId, PReturnNo, PReturnDate, @InvoiceNo AS InvoiceNo, PInvoice, InvoiceDate, LedgerID, 
               PaymodeID, Paymode, SubTotal, SpDisPer, SpDsiAmt, BillDiscountPer, BillDiscountAmt, TaxAmt, 
               ExpenseAmt, Frieght, OtherExpAmt, GrandTotal, Remarks, 0 AS InvoiceAmount, 0 AS AmtInDifference, 
               CompanyId, FinYearId, VoucherId, TaxType, CessAmt,
               SeriesID, VoucherID, TrnsType, VoucherType
        FROM PReturnMaster 
        WHERE Id = @Id
        
        SELECT PReturnDetails.ItemID, ItemMaster.Description, PReturnDetails.UnitId, 
               CONVERT(BIT, CASE PReturnDetails.BaseUnit WHEN 'Y' THEN 1 WHEN 'N' THEN 0 END) AS BaseUnit, 
               PReturnDetails.Packing, PReturnDetails.IsExpiry, '' AS BatchNo, NULL AS Expiry,
               PReturnDetails.Qty, PReturnDetails.TaxPer, PReturnDetails.TaxAmt, PReturnDetails.Reason,
               PReturnDetails.Free, PReturnDetails.Cost, PReturnDetails.DisPer, PReturnDetails.DisAmt, 
               PReturnDetails.SalesPrice, PReturnDetails.OriginalCost, PReturnDetails.TotalSP, 
               PReturnDetails.Cost - PReturnDetails.DisAmt + PReturnDetails.TaxAmt AS TotalAmount,
               PReturnDetails.CessAmt, PReturnDetails.CessPer, PReturnDetails.OriginalCost
        FROM PReturnDetails 
        LEFT JOIN ItemMaster ON PReturnDetails.ItemID = ItemMaster.ItemId
        WHERE PReturnDetails.PReturnNo = @PReturnNo 
            AND PReturnDetails.CompanyId = @CompanyId 
            AND PReturnDetails.BranchID = @BranchId 
            AND PReturnDetails.FinYearId = @FinYearId
        
        SELECT LedgerName FROM LedgerMaster WHERE LedgerID = (SELECT LedgerID FROM PReturnMaster WHERE Id = @Id)
        SELECT PaymodeID, Paymode FROM PReturnMaster WHERE Id = @Id
    END
    ELSE IF(@_Operation = 'UPDATE')
    BEGIN
        -- Get the lookup values from the database based on IDs
        SET @BranchName = (SELECT BranchName FROM Branches WHERE Id = @BranchId)
        SET @VendorName = (SELECT LedgerName FROM LedgerMaster WHERE LedgerID = @LedgerID)
        SET @Paymode = (SELECT PayModeName FROM PayMode WHERE PayModeID = @PaymodeID)
        SET @UserName = (SELECT UserName FROM Users WHERE UserID = @UserID)
        
        -- Handle CurrencyID safely
        IF @CompanyId IS NOT NULL AND @CompanyId > 0
        BEGIN
            SET @CurrencyID = (SELECT Currency FROM CompanyInfo WHERE CompanyID = @CompanyId)
            IF @CurrencyID IS NOT NULL AND @CurrencyID > 0
            BEGIN
                SET @CurSymbol = (SELECT CurrencySymbol FROM Currency WHERE CurrencyID = @CurrencyID)
            END
        END
        
        -- CRITICAL FIX: Update with ALL fields including the missing ones
        UPDATE PReturnMaster 
        SET BranchId = @BranchId,
            BranchName = @BranchName,
            PReturnDate = @PReturnDate,
            InvoiceNo = @InvoiceNo,
            InvoiceDate = @InvoiceDate,
            LedgerID = @LedgerID,
            VendorName = @VendorName,
            PaymodeID = @PaymodeID,
            Paymode = @Paymode,
            PaymodeLedgerID = @PaymodeLedgerID,
            CreditPeriod = @CreditPeriod,
            SubTotal = @SubTotal,
            SpDisPer = @SpDisPer,
            SpDsiAmt = @SpDsiAmt,
            BillDiscountPer = @BillDiscountPer,
            BillDiscountAmt = @BillDiscountAmt,
            TaxPer = @TaxPer,
            TaxAmt = @TaxAmt,
            Frieght = @Frieght,
            ExpenseAmt = @ExpenseAmt,
            OtherExpAmt = @OtherExpAmt,
            GrandTotal = @GrandTotal,
            CancelFlag = @CancelFlag,
            UserID = @UserID,
            UserName = @UserName,
            TaxType = @TaxType,
            Remarks = @Remarks,
            RoundOff = @RoundOff,
            CessPer = @CessPer,
            CessAmt = @CessAmt,
            CalAfterTax = @CalAfterTax,
            CurrencyID = ISNULL(@CurrencyID, CurrencyID),
            CurSymbol = ISNULL(@CurSymbol, CurSymbol),
            SeriesID = ISNULL(@SeriesID, SeriesID),
            VoucherID = ISNULL(@VoucherID, VoucherID),
            TrnsType = ISNULL(@TrnsType, TrnsType),
            VoucherType = ISNULL(@VoucherType, VoucherType)
        WHERE Id = @Id
        
        SELECT 'SUCCESS'
    END
    ELSE IF(@_Operation = 'DELETE')
    BEGIN
        UPDATE PReturnMaster SET CancelFlag = 1 WHERE Id = @Id
        
        DELETE FROM PReturnDetails 
        WHERE CompanyId = @CompanyId 
            AND FinYearId = @FinYearId 
            AND BranchID = @BranchId 
            AND PReturnNo = @PReturnNo
        
        UPDATE Vouchers SET CancelFlag = 1 
        WHERE CompanyId = @CompanyId 
            AND FinYearId = @FinYearId 
            AND BranchID = @BranchId 
            AND VoucherID = @VoucherId 
            AND VoucherType = @VoucherType
        
        SELECT 'SUCCESS'
    END
    ELSE IF(@_Operation = 'DDlVendor')
    BEGIN
        SELECT PurchaseNo, PurchaseDate, InvoiceNo, InvoiceDate, LedgerID, VendorName, PaymodeID, PayMode, GrandTotal, PayedAmount 
        FROM PMaster 
        WHERE LedgerID = @LedgerID
    END
    ELSE IF(@_Operation = 'GetAllPurchaseItems')
    BEGIN
        SELECT 
            Pd.SlNo, 
            Pd.ItemID, 
            ISNULL(Im.[Description], '') as ItemName,
            ISNULL(Ps.BarCode, '') as BarCode, 
            Pd.UnitId, 
            Pd.Unit, 
            Pd.Packing, 
            Pd.Qty, 
            Pd.Cost,
            ISNULL(Pd.TaxType, 'I') as TaxType,
            ISNULL(Pd.TaxPer, 0) as TaxPer,
            ISNULL(Pd.TaxAmt, 0) as TaxAmt,
            ISNULL((Pd.Cost * Pd.Packing * Pd.Qty), 0) as Amount,
            '' as Reason,
            ISNULL(SUM(prd.Returned), 0) as Returned
        FROM PDetails as Pd 
        LEFT JOIN ItemMaster as Im ON(Pd.ItemID = Im.ItemId)
        LEFT JOIN PriceSettings as Ps ON(Pd.ItemID = Ps.ItemId 
            AND (Ps.UnitId IS NULL OR Pd.UnitId = Ps.UnitId))
        LEFT JOIN PReturnDetails prd ON prd.ItemID = Pd.ItemID
            AND prd.CompanyId = @CompanyId
            AND prd.BranchID = @BranchId
            AND prd.FinYearId = @FinYearId
        LEFT JOIN PReturnMaster prm ON prd.PReturnNo = prm.PReturnNo 
            AND prd.CompanyId = prm.CompanyId 
            AND prd.BranchID = prm.BranchId
            AND prd.FinYearId = prm.FinYearId
            AND (
                prm.PInvoice = CAST(@PurchaseNo AS VARCHAR(50))
                OR TRY_CAST(prm.PInvoice AS INT) = @PurchaseNo
                OR prm.InvoiceNo = CAST(@PurchaseNo AS VARCHAR(50))
                OR TRY_CAST(prm.InvoiceNo AS INT) = @PurchaseNo
            )
        WHERE Pd.PurchaseNo = @PurchaseNo
        GROUP BY 
            Pd.SlNo, Pd.ItemID, Im.[Description], Ps.BarCode, Pd.UnitId, Pd.Unit, 
            Pd.Packing, Pd.Qty, Pd.Cost, Pd.TaxType, Pd.TaxPer, Pd.TaxAmt
        ORDER BY Pd.SlNo
    END
    ELSE IF(@_Operation = 'BARCODEPURCHASE')
    BEGIN
        SELECT isnull(IM.ItemId, 0) as ItemId, isnull(PS.BarCode, '') as BarCode, isnull(IM.[Description], '') as Description,
               isnull(PS.Cost, 0) as Cost, 0 as Free, isnull(PS.UnitId, 0) as UnitId, isnull(PS.Unit, '') as Unit,
               isnull(PS.Packing, 0) as Packing, isnull(PS.MarginPer, 0) as MarginPer, isnull(Ps.MarginAmt, 0),
               isnull(PS.TaxPer, 0) as TaxPer, isnull(PS.TaxAmt, 0) as TaxAmt, isnull(PS.RetailPrice, 0) as RetailPrice,
               isnull(PS.WholeSalePrice, 0) as WholeSalePrice, isnull(PS.CreditPrice, 0) as CreditPrice,
               isnull(PS.CardPrice, 0) as CardPrice, PS.Stock as Stock, PS.TaxType
        FROM PriceSettings AS PS
        LEFT JOIN ItemMaster AS IM ON PS.ItemId = IM.ItemId
        WHERE (@barcode IS NULL OR (PS.BarCode LIKE + ISNULL(@barcode, '') + '%'))
        
        SELECT 'SUCCESS'
    END
    ELSE IF(@_Operation = 'GETAllPurchaseReturn')
    BEGIN
        SELECT PReturnNo, PReturnDate, InvoiceNo, InvoiceDate, VendorName, Paymode, SubTotal, GrandTotal 
        FROM PReturnMaster 
        WHERE CancelFlag = 0
            AND BranchId = CASE WHEN @BranchId IS NOT NULL AND @BranchId <> 0 THEN @BranchId ELSE BranchId END
            AND CompanyId = CASE WHEN @CompanyId IS NOT NULL AND @CompanyId <> 0 THEN @CompanyId ELSE CompanyId END
    END
    ELSE IF(@_Operation = 'GETAllPurchaseReturnDetails')
    BEGIN
        SELECT SlNo, PrtD.ItemID, ItemName, Ps.BarCode, PrtD.Unit, PrtD.Packing, PrtD.Cost,
               (PrtD.Cost * PrtD.Packing) as Amount, PrtD.Reason, PReturnNo 
        FROM PReturnDetails as PrtD
        LEFT JOIN PriceSettings as Ps ON(PrtD.ItemID = Ps.ItemId)
        WHERE PrtD.UnitId = Ps.UnitId 
            AND PrtD.PReturnNo = @PReturnNo
            AND PrtD.BranchID = CASE WHEN @BranchId IS NOT NULL AND @BranchId <> 0 THEN @BranchId ELSE PrtD.BranchID END
            AND PrtD.CompanyId = CASE WHEN @CompanyId IS NOT NULL AND @CompanyId <> 0 THEN @CompanyId ELSE PrtD.CompanyId END
    END
    ELSE IF(@_Operation = 'GETPAYMENTINFO')
    BEGIN
        SELECT PaymodeID, Paymode 
        FROM PReturnMaster
        WHERE PReturnNo = @PReturnNo AND BranchId = @BranchId AND FinYearId = @FinYearId
    END
END
