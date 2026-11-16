ALTER PROC [dbo].[POS_Purchase]
(
@CompanyId int = NULL,
@FinYearId int = NULL,
@BranchId int = NULL,
@BranchName varchar(50) = NULL,
@PurchaseNo int = NULL,
@PurchaseDate datetime = NULL,
@InvoiceNo varchar(50) = NULL,
@InvoiceDate datetime = NULL,
@LedgerID int = NULL,
@VendorName varchar(100) = NULL,
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
@IsSyncd bit = NULL,
@Paid bit = NULL,
@Pid int=null,
@POrderMasterId int = NULL,
@TrnsType varchar(50) = NULL,
@PayedAmount float = NULL,
@BilledBy varchar(50) =NULL,
@NetTotal float = NULL,
@TransactionGuid uniqueidentifier = NULL,
@_Operation varchar(25)=null
)
AS
SET NOCOUNT ON
BEGIN
IF(@_Operation='GENERATEPURCHASENO')
BEGIN
	SELECT ISNULL(MAX(PurchaseNo)+1,1) AS PurchaseNo 
	FROM PMaster 
	WHERE CompanyId=@CompanyId AND BranchId=@BranchId
END

ELSE IF(@_Operation='CREATE')
BEGIN
	SET @BranchName=(SELECT BranchName FROM Branches WHERE id=@BranchId)
	SET @PurchaseNo=(SELECT ISNULL(PBillNo+1,1) FROM TrackTrans WHERE BranchID=@BranchId AND FinYearID=@FinYearId)
	SET @VendorName=(SELECT LedgerName FROM LedgerMaster WHERE LedgerID=@LedgerID)
	SET @Paymode =(SELECT PayModeName FROM PayMode WHERE PayModeID=@PaymodeID)
	SET @UserName=(SELECT UserName FROM Users WHERE UserID=@UserID)
	SET @CurrencyID=(SELECT Currency FROM CompanyInfo WHERE CompanyID=@CompanyId)
	SET @CurSymbol =(SELECT CurrencySymbol FROM Currency WHERE CurrencyID=@CurrencyID)

	INSERT INTO PMaster
	(CompanyId,FinYearId,BranchId,BranchName,PurchaseNo,PurchaseDate,InvoiceNo,InvoiceDate,LedgerID,VendorName,PaymodeID,Paymode,PaymodeLedgerID,CreditPeriod,SubTotal,SpDisPer,
	SpDsiAmt,BillDiscountPer, BillDiscountAmt, TaxPer, TaxAmt, Frieght, ExpenseAmt, OtherExpAmt,
	GrandTotal, CancelFlag, UserID, UserName, TaxType, Remarks, RoundOff, CessPer, CessAmt, 
	CalAfterTax, CurrencyID, CurSymbol, SeriesID, VoucherID, IsSyncd, Paid, POrderMasterId,PayedAmount,BilledBy,NetTotal,TransactionGuid
	) 
	VALUES
	(@CompanyId,@FinYearId,@BranchId,@BranchName,@PurchaseNo,@PurchaseDate,@InvoiceNo,@InvoiceDate,@LedgerID,@VendorName,@PaymodeID,@Paymode,@PaymodeLedgerID,@CreditPeriod,@SubTotal,@SpDisPer,
	@SpDsiAmt,@BillDiscountPer, @BillDiscountAmt, @TaxPer, @TaxAmt, @Frieght, @ExpenseAmt, @OtherExpAmt,
	@GrandTotal, @CancelFlag, @UserID, @UserName, @TaxType, @Remarks, @RoundOff, @CessPer, @CessAmt, 
	@CalAfterTax, @CurrencyID, @CurSymbol, @SeriesID, @VoucherID, @IsSyncd, @Paid, @POrderMasterId,@PayedAmount,@BilledBy,@NetTotal,@TransactionGuid
	)

	UPDATE TrackTrans SET PBillNo=@PurchaseNo WHERE BranchID=@BranchId AND FinYearID=@FinYearId
	SELECT @PurchaseNo
END

ELSE IF(@_Operation='GETALL')
BEGIN
	SELECT 
		Pm.Pid, 
		Pm.CompanyId, 
		Pm.BranchId, 
		Pm.BranchName,
		Pm.PurchaseNo,
		Pm.PurchaseDate,
		Lm.LedgerName AS VendorName,
		Pm.GrandTotal,
		Pm.Paymode,
		Pm.UserName
	FROM PMaster AS Pm
	LEFT JOIN LedgerMaster AS Lm ON (Pm.LedgerID=Lm.LedgerID)
	WHERE Pm.CancelFlag=0 
		AND (@CompanyId IS NULL OR Pm.CompanyId = @CompanyId)
		AND (@BranchId IS NULL OR Pm.BranchId = @BranchId)
	ORDER BY Pm.PurchaseNo DESC
END

ELSE IF(@_Operation='GETBYID')
BEGIN
	SET @CompanyId = (SELECT CompanyId FROM PMaster WHERE Pid = @Pid)
	SET @FinYearId = (SELECT FinYearId FROM PMaster WHERE Pid = @Pid)
	SET @BranchId = (SELECT BranchId FROM PMaster WHERE Pid = @Pid)
	SET @PurchaseNo = (SELECT PurchaseNo FROM PMaster WHERE Pid = @Pid)

	SELECT Pid, CompanyId, FinYearId, VoucherID, PurchaseNo, BranchId, PurchaseDate, LedgerID, VendorName,PaymodeID,Paymode,
	InvoiceNo, InvoiceDate, 0 AS InvoiceAmt, ExpenseAmt, Frieght, OtherExpAmt, SpDisPer, SpDsiAmt, 
	BillDiscountPer, BillDiscountAmt, Remarks, SubTotal, GrandTotal, 0 AS AmtInDifference, TaxAmt,
	POrderMasterId, RoundOff, TaxType, CessAmt,PayedAmount,BilledBy,NetTotal,TransactionGuid
	FROM PMaster 
	WHERE Pid = @Pid

	SELECT PDetails.ItemID, ItemMaster.Description AS ItemName, PDetails.UnitId, CONVERT(varchar, CASE PDetails.BaseUnit WHEN 'Y' THEN 1
	WHEN 'N' THEN 0
	END) AS BaseUnit, 
	PDetails.Packing, PDetails.IsExpiry, ISNULL(ItemBatch.BatchNo,'') AS BatchNo, 
	ISNULL(ItemBatch.Expiry ,NULL) AS Expiry, PDetails.Qty, PDetails.TaxPer, PDetails.TaxAmt, 
	PDetails.TaxType,
	PDetails.Free, PDetails.Cost, PDetails.DisPer, PDetails.DisAmt, PDetails.SalesPrice, PDetails.OriginalCost,
	PDetails.TotalSP, PDetails.Cost - PDetails.DisAmt + PDetails.TaxAmt AS TotalAmount,
	PriceSettings.RetailPrice, PriceSettings.WholeSalePrice, PriceSettings.CreditPrice, 
	ISNULL(PriceSettings.BarCode,'') AS Barcode, PDetails.CessPer, PDetails.CessAmt, UM.UnitName AS Unit,
	PDetails.TransactionGuid
	FROM PDetails 
	LEFT JOIN ItemBatch	ON PDetails.PurchaseNo = ItemBatch.TrnsNo AND 
	ItemBatch.TrnsType = @TrnsType AND
	PDetails.SlNo = ItemBatch.SlNo AND 
	PDetails.ItemID = ItemBatch.ItemId AND 
	PDetails.FinYearId = ItemBatch.FinYearId AND
	PDetails.BranchID = ItemBatch.BranchId AND
	PDetails.CompanyId = ItemBatch.CompanyId AND
	PDetails.UnitId = ItemBatch.UnitId
	LEFT JOIN ItemMaster ON PDetails.ItemID = ItemMaster.ItemId
	LEFT JOIN PriceSettings ON PDetails.ItemID = PriceSettings.ItemId AND 
	PDetails.UnitId = PriceSettings.UnitId AND 
	PDetails.BranchID = PriceSettings.BranchId
	LEFT JOIN UnitMaster UM ON UM.UnitId = PDetails.UnitId
	WHERE PDetails.PurchaseNo = @PurchaseNo AND
	PDetails.CompanyId = @CompanyId AND
	PDetails.BranchID = @BranchId AND
	PDetails.FinYearId = @FinYearId
	ORDER BY PDetails.SlNo 
			
	SELECT LedgerName FROM LedgerMaster WHERE LedgerID = (SELECT LedgerID FROM PMaster WHERE Pid = @Pid)
END

ELSE IF(@_Operation='UPDATE')
BEGIN
	IF (EXISTS(SELECT * FROM PReturnMaster WHERE InvoiceNo=CAST(@PurchaseNo AS varchar(50)) AND BranchId=@BranchId))
	BEGIN
		SELECT 'CANNOT EDIT SINCE RETURN REFERENCE EXISTS.'
	END
	ELSE
	BEGIN
		SET @BranchName=(SELECT BranchName FROM Branches WHERE Id=@BranchId);
		SET @VendorName=(SELECT Ledgername FROM LedgerMaster WHERE LedgerID=@LedgerID)
		SET @Paymode=(SELECT PayModeName FROM PayMode WHERE PayModeID=@PaymodeID)
		SET @UserName=(SELECT Username FROM Users WHERE UserID=@UserID)
		SET @CurrencyID=(SELECT Currency FROM CompanyInfo WHERE CompanyID=@CompanyId)
		SET @CurSymbol=(SELECT CurrencySymbol FROM Currency WHERE CurrencyID=@CurrencyID)

		UPDATE PMaster SET 
			BranchId = @BranchId,
			BranchName = @BranchName,
			PurchaseDate = @PurchaseDate,
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
			CurrencyID = @CurrencyID,
			CurSymbol = @CurSymbol,
			BilledBy=@BilledBy,
			NetTotal=@NetTotal,
			POrderMasterId = @POrderMasterId,
			PayedAmount = @PayedAmount,
			TransactionGuid = ISNULL(@TransactionGuid, TransactionGuid)
		WHERE Pid = @Pid

		DELETE FROM PDetails WHERE CompanyId = @CompanyId AND
			FinYearId = @FinYearId AND
			BranchID = @BranchId AND
			PurchaseNo = @PurchaseNo

		SELECT 'SUCCESS'
	END
END

ELSE IF(@_Operation='DELETE')
BEGIN
	-- 1. Strict Accounting Rule: Block deleting if active Vendor Payment(s) exist for this purchase bill
	DECLARE @DeletePaymentIds VARCHAR(500);
	SELECT @DeletePaymentIds = STUFF((
		SELECT DISTINCT ', #' + CAST(VPM.VoucherId AS VARCHAR(20))
		FROM VendorPaymentDetails VPD
		INNER JOIN VendorPaymentMaster VPM
			ON VPM.Id = VPD.PaymentMasterId AND VPM.BranchId = VPD.BranchId
		INNER JOIN PMaster PM
			ON PM.PurchaseNo = VPD.BillNo AND PM.BranchId = VPD.BranchId
		   AND PM.LedgerID = VPM.VendorLedgerId
		WHERE VPD.BillNo = @PurchaseNo
		  AND (@BranchId IS NULL OR VPD.BranchId = @BranchId)
		  AND (@FinYearId IS NULL OR PM.FinYearId = @FinYearId)
		  AND ISNULL(PM.CancelFlag, 0) = 0
		  AND ISNULL(VPD.CancelFlag, 0) = 0
		  AND ISNULL(VPM.CancelFlag, 0) = 0
		ORDER BY 1
		FOR XML PATH('')
	), 1, 2, '');

	IF @DeletePaymentIds IS NOT NULL
	BEGIN
		RAISERROR('Cannot delete Purchase Bill #%d because active Vendor Payment(s) [%s] exist. Cancel the payment(s) first.', 16, 1, @PurchaseNo, @DeletePaymentIds);
		RETURN;
	END

	-- 2. Strict Accounting Rule: Block deleting if active Purchase Return(s) exist for this purchase bill
	DECLARE @DeletePReturnIds VARCHAR(500);
	SELECT @DeletePReturnIds = STUFF((
		SELECT DISTINCT ', #' + CAST(PRM.PReturnNo AS VARCHAR(20))
		FROM PReturnMaster PRM
		WHERE (PRM.InvoiceNo = CAST(@PurchaseNo AS VARCHAR(50)) 
			OR PRM.PInvoice = CAST(@PurchaseNo AS VARCHAR(50))
			OR (EXISTS (SELECT 1 FROM PMaster PM WHERE PM.PurchaseNo = @PurchaseNo AND (@BranchId IS NULL OR PM.BranchId = @BranchId) AND PM.InvoiceNo = PRM.InvoiceNo AND PRM.LedgerID = PM.LedgerID)))
		  AND (@BranchId IS NULL OR PRM.BranchId = @BranchId)
		  AND (@FinYearId IS NULL OR PRM.FinYearId = @FinYearId)
		  AND ISNULL(PRM.CancelFlag, 0) = 0
		ORDER BY 1
		FOR XML PATH('')
	), 1, 2, '');

	IF @DeletePReturnIds IS NOT NULL
	BEGIN
		RAISERROR('Cannot delete Purchase Bill #%d because active Purchase Return(s) [%s] exist. Cancel the purchase return first.', 16, 1, @PurchaseNo, @DeletePReturnIds);
		RETURN;
	END

	-- 3. Soft delete PMaster
	UPDATE PMaster 
	SET CancelFlag = 1 
	WHERE PurchaseNo = @PurchaseNo 
		AND BranchId = @BranchId 
		AND FinYearId = @FinYearId;

	-- 4. Delete PDetails line items
	DELETE FROM PDetails 
	WHERE PurchaseNo = @PurchaseNo 
		AND BranchId = @BranchId 
		AND FinYearId = @FinYearId;
	
	-- 5. Cancel Vouchers
	UPDATE Vouchers 
	SET CancelFlag = 1 
	WHERE VoucherID = @VoucherID 
		AND BranchID = @BranchId 
		AND FinYearId = @FinYearId 
		AND VoucherType = 'Purchase';

	SELECT 'SUCCESS';
END

END
