CREATE PROC [dbo].[_Test16]
(
@FromDate datetime = NULL,
@ToDate datetime = NULL,
@CompanyId int = NULL,
@BranchId int = NULL,
@FinYearId int = NULL,
@BarcodeContains varchar(50) = NULL,
@GroupId int = NULL,
@CategoryId int = NULL,
@SubCategoryId int = NULL,
@LedgerId int = NULL
)
AS
SET NOCOUNT ON
BEGIN

-- 1. Insert all transaction data into a temporary table to avoid executing the 11-table union multiple times
SELECT * INTO #data FROM (
		SELECT PD.BranchId AS BranchId, PD.PurchaseDate AS [Date], ItemID, UnitId, 
				0 AS Opening, ((Packing * Qty)+Free) AS Purchase,  0 AS PurchaseReturn, 0 AS StockAdjustmentIn, 0 AS StockAdjustmentOut,
				0 AS StockTransferIn, 0 AS StockTransferOut, 0 AS Sales, 0 AS SalesReturn, 0 AS Closing, 0 AS OrderedStock,
				0 AS HoldQty,
				CASE WHEN PD.TaxType = 'I' THEN (PD.Cost * PD.Qty) - PD.TaxAmt - PD.CessAmt
										   ELSE PD.Cost * PD.Qty END AS InwardCost, 0 AS OutwardCost,0 as Profit,0 as SaleAmount
			FROM PDetails AS PD 
				LEFT JOIN PMaster AS PM ON (PD.BranchID = PM.BranchId AND PD.FinYearId = PM.FinYearId AND PD.PurchaseNo = PM.PurchaseNo)
			WHERE PM.CancelFlag = 0 AND PD.FinYearId = @FinYearId AND 
					CAST(PD.PurchaseDate AS date) <= CAST(@ToDate AS date) AND PD.CompanyId = @CompanyId AND 
					PD.BranchId = CASE WHEN @BranchId IS NOT NULL AND @BranchId <> 0 THEN @BranchId ELSE PD.BranchId END 
		UNION ALL
		SELECT SRD.BranchId AS BranchId, SRD.SReturnDate AS [Date], ItemID, UnitId, 
				0 AS Opening, 0 AS Purchase, 0 AS PurchaseReturn, 0 AS StockAdjustmentIn, 0 AS StockAdjustmentOut, 
				0 AS StockTransferIn, 0 AS StockTransferOut, 0 AS Sales, Packing * ReturnedQty AS SalesReturn, 0 AS Closing, 
				0 AS OrderedStock, 
				0 AS HoldQty,
				SRD.Cost * SRD.ReturnedQty AS InwardCost, 0 AS OutwardCost,0 as Profit,0 as SaleAmount
			FROM SReturnDetails AS SRD
				LEFT JOIN SReturnMaster AS SRM ON (SRD.BranchID = SRM.BranchId AND SRD.FinYearId = SRM.FinYearId AND SRD.SReturnNo = SRM.SReturnNo)
			WHERE SRM.CancelFlag = 0 AND SRD.FinYearId = @FinYearId AND 
					CAST(SRD.SReturnDate AS date) <= CAST(@ToDate AS date) AND SRD.CompanyId = @CompanyId AND 
					SRD.BranchId = CASE WHEN @BranchId IS NOT NULL AND @BranchId <> 0 THEN @BranchId ELSE SRD.BranchId END 
		UNION ALL
		SELECT STD.TargetId AS BranchId, STD.TransferDate AS [Date], ItemID, STD.UnitId, 
				0 AS Opening, 0 AS Purchase, 0 AS PurchaseReturn, 0 AS StockAdjustmentIn, 0 AS StockAdjustmentOut, 
				UM.Packing * Qty AS StockTransferIn, 0 AS StockTransferOut, 0 AS Sales, 0 AS SalesReturn, 0 AS Closing,
				0 AS OrderedStock, 
				0 AS HoldQty,
				Rate * Qty AS InwardCost, 0 AS OutwardCost,0 as Profit,0 as SaleAmount
			FROM StockTransferDetail AS STD
				LEFT JOIN StockTransferMaster AS STM ON (STD.TargetId = STM.TargetId AND STD.FinYearId = STM.FinYearId AND STD.StkTrNo = STM.StkTrNo)
				LEFT JOIN UnitMaster AS UM ON UM.UnitId = STD.UnitId
			WHERE @BranchId IS NOT NULL AND @BranchId <> 0 AND STM.CancelFlag = 0 AND STD.FinYearId = @FinYearId AND 
					CAST(STD.TransferDate AS date) <= CAST(@ToDate AS date) AND STD.CompanyId = @CompanyId AND STD.TargetId = @BranchId
		UNION ALL
		SELECT PS.BranchId AS BranchId, ISNULL(PS.OpnDate, '1900-01-01') AS [Date], PS.ItemId AS ItemID, PS.UnitId AS UnitId, 
				OpnStk AS Opening, 0 AS Purchase, 0 AS PurchaseReturn, 0 AS StockAdjustmentIn, 0 AS StockAdjustmentOut, 
				0 AS StockTransferIn, 0 AS StockTransferOut, 0 AS Sales, 0 AS SalesReturn, 0 AS Closing, 0 AS OrderedStock,
				0 AS HoldQty,
				ISNULL(PS.OpnValue, PS.OpnStk * ISNULL(PS.OpeningCost, PS.Cost)) AS InwardCost, 0 AS OutwardCost,0 as Profit,0 as SaleAmount
			FROM PriceSettings AS PS
					LEFT JOIN ItemMaster AS IM ON (PS.ItemId = IM.ItemId)
			WHERE CAST(ISNULL(PS.OpnDate, '1900-01-01') AS date) <= CAST(@ToDate AS date) AND PS.CompanyId = @CompanyId AND
					PS.BranchId = CASE WHEN @BranchId IS NOT NULL AND @BranchId <> 0 THEN @BranchId ELSE PS.BranchId END 
		UNION ALL
		SELECT SAD.BranchId AS BranchId, SAM.StockAdjustmentDate AS [Date], ItemId, SAD.UnitId, 
				0 AS Opening, 0 AS Purchase, 0 AS PurchaseReturn, 
				CASE WHEN QtyDifference > 0 THEN UM.Packing * QtyDifference ELSE 0 END AS StockAdjustmentIn, 
				0 AS StockAdjustmentOut, 0 AS StockTransferIn, 0 AS StockTransferOut, 0 AS Sales, 0 AS SalesReturn, 
				0 AS Closing, 0 AS OrderedStock, 
				0 AS HoldQty,
				CASE WHEN QtyDifference > 0 THEN Cost * QtyDifference ELSE 0 END AS InwardCost, 0 AS OutwardCost,0 as Profit,0 as SaleAmount
			FROM StockAdjustmentDetails AS SAD
				LEFT JOIN StockAdjustmentMaster AS SAM ON SAM.Id = SAD.StockAdjustmentMasterId
				LEFT JOIN UnitMaster AS UM ON UM.UnitId = SAD.UnitId
			WHERE SAM.CancelFlag = 0 AND SAD.FinYearId = @FinYearId AND 
					CAST(SAM.StockAdjustmentDate AS date) <= CAST(@ToDate AS date) AND SAD.CompanyId = @CompanyId AND 
					SAD.BranchId = CASE WHEN @BranchId IS NOT NULL AND @BranchId <> 0 THEN @BranchId ELSE SAD.BranchId END AND
					SAD.QtyDifference > 0 
		UNION ALL
		SELECT PRD.BranchID AS BranchId, PRD.PReturnDate AS [Date], ItemID, UnitId, 
				0 AS Opening, 0 AS Purchase, Packing * Returned AS PurchaseReturn, 0 AS StockAdjustmentIn, 0 AS StockAdjustmentOut, 
				0 AS StockTransferIn, 0 AS StockTransferOut, 0 AS Sales, 0 AS SalesReturn, 0 AS Closing, 0 AS OrderedStock,
				0 AS HoldQty,
				0 AS InwardCost,
				CASE WHEN PRD.TaxType = 'I' THEN (PRD.Cost * PRD.Returned) - PRD.TaxAmt - PRD.CessAmt
										    ELSE PRD.Cost * PRD.Returned END AS OutwardCost,0 as Profit,0 as SaleAmount
			FROM PReturnDetails AS PRD 
				LEFT JOIN PReturnMaster AS PRM ON (PRD.BranchID = PRM.BranchId AND PRD.FinYearId = PRM.FinYearId AND PRD.PReturnNo = PRM.PReturnNo)
			WHERE PRM.CancelFlag = 0 AND PRD.FinYearId = @FinYearId AND CAST(PRD.PReturnDate AS date) <= CAST(@ToDate AS date) AND 
					PRD.CompanyId = @CompanyId AND PRD.BranchId = CASE WHEN @BranchId IS NOT NULL AND @BranchId <> 0 THEN @BranchId ELSE PRD.BranchId END 
		UNION ALL
		SELECT SOD.BranchId AS BranchId, SOD.OrderDate AS [Date], ItemID, UnitId, 
				0 AS Opening, 0 AS Purchase, 0 AS PurchaseReturn, 0 AS StockAdjustmentIn, 0 AS StockAdjustmentOut, 
				0 AS StockTransferIn, 0 AS StockTransferOut, 0 AS Sales, 0 AS SalesReturn, 0 AS Closing, 
				Packing * Qty AS OrderedStock, 
				0 AS HoldQty,
				0 AS InwardCost, SOD.Cost * SOD.Qty AS OutwardCost,0 as Profit,0 as SaleAmount
			FROM SOrderDetails AS SOD 
				LEFT JOIN SOrderMaster AS SOM ON (SOD.BranchID = SOM.BranchId AND SOD.FinYearId = SOM.FinYearId AND SOD.OrderNo = SOM.OrderNo)
			WHERE SOM.CancelFlag = 0 AND SOD.FinYearId = @FinYearId AND CAST(SOD.OrderDate AS date) <= CAST(@ToDate AS date) AND 
					SOD.CompanyId = @CompanyId AND SOD.BranchId = CASE WHEN @BranchId IS NOT NULL AND @BranchId <> 0 THEN @BranchId ELSE SOD.BranchId END 
					AND (SOM.MobileOrderStatus = 'Pending' OR SOM.MobileOrderStatus = 'Waiting')
		UNION ALL
		SELECT SD.BranchId AS BranchId, SD.BillDate AS [Date], ItemID, UnitId,
				0 AS Opening, 0 AS Purchase, 0 AS PurchaseReturn, 0 AS StockAdjustmentIn, 0 AS StockAdjustmentOut, 
				0 AS StockTransferIn, 0 AS StockTransferOut, 
				CASE WHEN ISNULL(SM.Status, '') <> 'Hold' THEN Packing * Qty ELSE 0 END AS Sales, 
				0 AS SalesReturn, 0 AS Closing,
				CASE WHEN SM.Status = 'Hold' THEN Packing * Qty ELSE 0 END AS OrderedStock, 
				CASE WHEN SM.Status = 'Hold' THEN Packing * Qty ELSE 0 END AS HoldQty, 
				0 AS InwardCost, 
				CASE WHEN ISNULL(SM.Status, '') <> 'Hold' THEN SD.Cost * SD.Qty ELSE 0 END AS OutwardCost,
				CASE WHEN ISNULL(SM.Status, '') <> 'Hold' THEN SD.MarginAmt ELSE 0 END as Profit,
				CASE WHEN ISNULL(SM.Status, '') <> 'Hold' THEN SD.TotalAmount ELSE 0 END as SaleAmount
			FROM SDetails AS SD 
				LEFT JOIN SMaster AS SM ON (SD.BranchID = SM.BranchId AND SD.FinYearId = SM.FinYearId AND SD.BillNo = SM.BillNo)
			WHERE SD.CancelFlag = 0 AND SD.FinYearId = @FinYearId AND CAST(SD.BillDate AS date) <= CAST(@ToDate AS date) AND 
					SD.CompanyId = @CompanyId AND SD.BranchId = CASE WHEN @BranchId IS NOT NULL AND @BranchId <> 0 THEN @BranchId ELSE SD.BranchId END 
		UNION ALL
		SELECT STD.SourceId AS BranchId, STD.TransferDate AS [Date], ItemID, STD.UnitId, 
				0 AS Opening, 0 AS Purchase, 0 AS PurchaseReturn, 0 AS StockAdjustmentIn, 0 AS StockAdjustmentOut,
				0 AS StockTransferIn, UM.Packing * Qty AS StockTransferOut, 0 AS Sales, 0 AS SalesReturn, 0 AS Closing,
				0 AS OrderedStock, 
				0 AS HoldQty,
				0 AS InwardCost, Rate * Qty AS OutwardCost,0 as Profit,0 as SaleAmount
			FROM StockTransferDetail AS STD
				LEFT JOIN StockTransferMaster AS STM ON (STD.SourceId = STM.SourceId AND STD.FinYearId = STM.FinYearId AND STD.StkTrNo = STM.StkTrNo)
				LEFT JOIN UnitMaster AS UM ON UM.UnitId = STD.UnitId
			WHERE @BranchId IS NOT NULL AND @BranchId <> 0 AND STM.CancelFlag = 0 AND STD.FinYearId = @FinYearId AND 
					CAST(STD.TransferDate AS date) <= CAST(@ToDate AS date) AND STD.CompanyId = @CompanyId AND STD.SourceId = @BranchId
		UNION ALL
		SELECT SAD.BranchId AS BranchId, SAM.StockAdjustmentDate AS [Date], ItemId, SAD.UnitId,
				0 AS Opening, 0 AS Purchase, 0 AS PurchaseReturn, 0 AS StockAdjustmentIn, 
				CASE WHEN QtyDifference < 0 THEN UM.Packing * (SAD.QtyDifference * -1) ELSE 0 END AS StockAdjustmentOut,
				0 AS StockTransferIn, 0 AS StockTransferOut, 0 AS Sales, 0 AS SalesReturn, 0 AS Closing,
				0 AS OrderedStock, 
				0 AS HoldQty,
				0 AS InwardCost, 
				CASE WHEN QtyDifference < 0 THEN Cost * (QtyDifference * -1) ELSE 0 END AS OutwardCost,0 as Profit,0 as SaleAmount
			FROM StockAdjustmentDetails AS SAD
				LEFT JOIN StockAdjustmentMaster AS SAM ON SAM.Id = SAD.StockAdjustmentMasterId
				LEFT JOIN UnitMaster AS UM ON UM.UnitId = SAD.UnitId
			WHERE SAM.CancelFlag = 0 AND SAD.FinYearId = @FinYearId AND 
					CAST(SAM.StockAdjustmentDate AS date) <= CAST(@ToDate AS date) AND SAD.CompanyId = @CompanyId AND 
					SAD.BranchId = CASE WHEN @BranchId IS NOT NULL AND @BranchId <> 0 THEN @BranchId ELSE SAD.BranchId END AND
					SAD.QtyDifference < 0
		UNION ALL
		SELECT OSWM.BranchId AS BranchId, OSWM.Date AS [Date], OSWD.ItemId AS ItemID, UnitId,
				0 AS Opening, 0 AS Purchase, 0 AS PurchaseReturn, 0 AS StockAdjustmentIn, Packing * Qty AS StockAdjustmentOut, 
				0 AS StockTransferIn, 0 AS StockTransferOut, 0 AS Sales, 0 AS SalesReturn, 0 AS Closing,
				0 AS OrderedStock, 
				0 AS HoldQty,
				0 AS InwardCost, OSWD.Rate * OSWD.Qty AS OutwardCost,0 as Profit,0 as SaleAmount
			FROM OfficeSuppliesWithdrawDetails AS OSWD
				LEFT JOIN OfficeSuppliesWithdrawMaster AS OSWM ON (OSWM.Id = OSWD.MasterId)
			WHERE OSWM.CancelFlag = 0 AND OSWM.FinYearId = @FinYearId AND CAST(OSWM.Date AS date) <= CAST(@ToDate AS date) AND 
					OSWM.CompanyId = @CompanyId AND OSWM.BranchId = CASE WHEN @BranchId IS NOT NULL AND @BranchId <> 0 THEN @BranchId ELSE OSWM.BranchId END 
) AS T_CTE

-- 2. Create clustered index on the temp table to make subsequent queries fast
CREATE CLUSTERED INDEX IX_temp_data_ItemId_Date ON #data(ItemId, [Date])

-- 3. Execute main select query referencing the temp table
SELECT * FROM (
	SELECT T.ItemId, G.GroupName AS GroupName, C.CategoryName AS CategoryName, SC.SubCategoryName AS SubCategoryName,
			PS.BarCode AS Barcode, IM.Description AS ItemName,
			SUM(Opening) AS OpeningStock, 
			SUM(Purchase) AS Purchase, 
			SUM(PurchaseReturn) AS PurchaseReturn, 
			SUM(StockAdjustmentIn) AS StockAdjustmentIn, 
			SUM(StockAdjustmentOut) AS StockAdjustmentOut, 
			SUM(StockTransferIn) AS StockTransferIn, 
			SUM(StockTransferOut) AS StockTransferOut, 
			SUM(Sales) AS Sales,
			SUM(Profit) as Profit,
			SUM(SaleAmount) as SaleAmount, 
			SUM(SalesReturn) AS SalesReturn, 
			round(CAST(SUM(Closing) AS decimal(18,5)),2) AS ClosingStock, 
			SUM(T.OrderedStock) AS OrderedStock,
			SUM(T.HoldQty) AS HoldQty,
			CASE SUM(Closing) WHEN 0 THEN MIN(PS.Cost) ELSE ROUND(SUM(T.StockValue) / SUM(Closing), 5) END AS Cost,
			PS.RetailPrice AS RetailPrice,
			PS.WholeSalePrice AS WholeSalePrice,
			PS.CreditPrice AS CreditPrice,
			UM.UnitName AS BaseUnitName
		FROM (
				SELECT ItemId, ((SUM(Opening) + SUM(Purchase) + SUM(StockAdjustmentIn) + SUM(StockTransferIn) + SUM(SalesReturn)) -
					   (SUM(PurchaseReturn) + SUM(StockAdjustmentOut) + SUM(StockTransferOut) + SUM(Sales))) AS Opening,
					   0 AS Purchase, 0 AS PurchaseReturn, 0 AS StockAdjustmentIn, 0 AS StockAdjustmentOut, 0 AS StockTransferIn, 
					   0 AS StockTransferOut, 0 AS Sales, 0 AS SalesReturn, 0 AS OrderedStock, 0 AS Closing, 0 AS StockValue,0 as Profit,0 as SaleAmount,
					   0 AS HoldQty
					FROM #data
					WHERE CAST([Date] AS date) < CAST(@FromDate AS date)
					GROUP BY ItemId
				UNION
				SELECT ItemId, 0 AS Opening, 0 AS Purchase, 0 AS PurchaseReturn, 0 AS StockAdjustmentIn, 0 AS StockAdjustmentOut,
					   0 AS StockTransferIn, 0 AS StockTransferOut, 0 AS Sales, 0 AS SalesReturn, 0 AS OrderedStock,
					   round(CAST(((SUM(Opening) + SUM(Purchase) + SUM(StockAdjustmentIn) + SUM(StockTransferIn) + SUM(SalesReturn)) -
					   (SUM(PurchaseReturn) + SUM(StockAdjustmentOut) + SUM(StockTransferOut) + SUM(Sales))) AS decimal(18,5)),2) AS Closing,
	   				   SUM(InwardCost) - SUM(OutwardCost) AS StockValue,0 as Profit,0 as SaleAmount,
					   SUM(HoldQty) AS HoldQty
					FROM #data
					WHERE CAST([Date] AS date) <= CAST(@ToDate AS date)
					GROUP BY ItemId
				UNION
				SELECT ItemId, 0 AS Opening, SUM(Purchase) AS Purchase, SUM(PurchaseReturn) AS PurchaseReturn,
					   SUM(StockAdjustmentIn) AS StockAdjustmentIn, SUM(StockAdjustmentOut) AS StockAdjustmentOut, 
					   SUM(StockTransferIn) AS StockTransferIn, SUM(StockTransferOut) AS StockTransferOut, SUM(Sales) AS Sales, 
					   SUM(SalesReturn) AS SalesReturn, SUM(OrderedStock) AS OrderedStock, 0 AS Closing, 0 AS StockValue,sum(Profit) as Profit,sum(SaleAmount) as SaleAmount,
					   0 AS HoldQty
					FROM #data
					WHERE CAST([Date] AS date) BETWEEN CAST(@FromDate AS date) AND CAST(@ToDate AS date)
					GROUP BY ItemId
			) T 
			LEFT JOIN ItemMaster AS IM ON IM.ItemId = T.ItemId
			LEFT JOIN PriceSettings AS PS ON PS.BranchId = CASE WHEN @BranchId IS NOT NULL AND @BranchId <> 0 THEN @BranchId ELSE PS.BranchId END
												AND PS.ItemId = T.ItemId AND PS.IsBaseUnit = 'Y'
			LEFT JOIN [Group] AS G ON G.Id = IM.GroupId
			LEFT JOIN Category AS C ON C.Id = IM.CategoryId
			LEFT JOIN SubCategory AS SC ON SC.Id = IM.SubCategoryId
			LEFT JOIN UnitMaster AS UM ON UM.UnitId = PS.UnitId
		WHERE IM.Active = 0 AND PS.BarCode LIKE '%'+ ISNULL(@BarcodeContains, '') +'%' AND
			((ISNULL(@GroupId, 0) > 0 AND IM.GroupId = @GroupId) OR (ISNULL(@GroupId, 0) = 0 AND IM.GroupId = IM.GroupId OR IM.GroupId IS NULL)) AND 
			((ISNULL(@CategoryId, 0) > 0 AND IM.CategoryId = @CategoryId) OR (ISNULL(@CategoryId, 0) = 0 AND IM.CategoryId = IM.CategoryId OR IM.CategoryId IS NULL)) AND 
			((ISNULL(@SubCategoryId, 0) > 0 AND IM.SubCategoryId = @SubCategoryId) OR (ISNULL(@SubCategoryId, 0) = 0 AND IM.SubCategoryId = IM.SubCategoryId OR IM.SubCategoryId IS NULL)) AND
			((ISNULL(@LedgerId, 0) > 0) AND T.ItemId IN (SELECT ItemId FROM PDetails WHERE PurchaseNo IN (SELECT PurchaseNo FROM PMaster WHERE LedgerId = @LedgerId AND CancelFlag = 0))
				OR (ISNULL(@LedgerId, 0) = 0))
		GROUP BY T.ItemId, PS.BarCode, IM.Description, G.GroupName, C.CategoryName, SC.SubCategoryName, PS.RetailPrice, PS.WholeSalePrice, PS.CreditPrice, UM.UnitName
	UNION
	SELECT IM.ItemId, G.GroupName AS GroupName, C.CategoryName AS CategoryName, SC.SubCategoryName AS SubCategoryName,
			PS.BarCode AS Barcode, IM.Description AS ItemName, PS.Stock AS OpeningStock, 0 AS Purchase, 0 AS PurchaseReturn, 
			0 AS StockAdjustmentIn, 0 AS StockAdjustmentOut, 0 AS StockTransferIn, 0 AS StockTransferOut, 0 AS Sales,0 as Profit,0 as SaleAmount, 0 AS SalesReturn, 
			ROUND(CAST(PS.Stock AS decimal(18,5)),2) AS ClosingStock, PS.OrderedStock AS OrderedStock, 
			0 AS HoldQty,
			PS.Cost AS Cost, PS.RetailPrice AS RetailPrice, 
			PS.WholeSalePrice AS WholeSalePrice, PS.CreditPrice AS CreditPrice, UM.UnitName AS BaseUnitName
		FROM ItemMaster AS IM
			LEFT JOIN PriceSettings AS PS ON PS.BranchId = CASE WHEN @BranchId IS NOT NULL AND @BranchId <> 0 THEN @BranchId ELSE PS.BranchId END
												AND PS.ItemId = IM.ItemId AND PS.IsBaseUnit = 'Y'
			LEFT JOIN [Group] AS G ON G.Id = IM.GroupId
			LEFT JOIN Category AS C ON C.Id = IM.CategoryId
			LEFT JOIN SubCategory AS SC ON SC.Id = IM.SubCategoryId
			LEFT JOIN UnitMaster AS UM ON UM.UnitId = PS.UnitId
		WHERE IM.Active = 0 AND PS.BarCode LIKE '%'+ ISNULL(@BarcodeContains, '') +'%' AND
			((ISNULL(@GroupId, 0) > 0 AND IM.GroupId = @GroupId) OR (ISNULL(@GroupId, 0) = 0)) AND 
			((ISNULL(@CategoryId, 0) > 0 AND IM.CategoryId = @CategoryId) OR (ISNULL(@CategoryId, 0) = 0)) AND 
			((ISNULL(@SubCategoryId, 0) > 0 AND IM.SubCategoryId = @SubCategoryId) OR (ISNULL(@SubCategoryId, 0) = 0)) AND
			((ISNULL(@LedgerId, 0) > 0) AND IM.ItemId IN (SELECT ItemId FROM PDetails WHERE PurchaseNo IN (SELECT PurchaseNo FROM PMaster WHERE LedgerId = @LedgerId AND CancelFlag = 0))
				OR (ISNULL(@LedgerId, 0) = 0)) AND
			IM.ItemId NOT IN (SELECT DISTINCT ItemId FROM #data)
		GROUP BY IM.ItemId, PS.Stock, PS.Cost, PS.OrderedStock, PS.BarCode, IM.Description, G.GroupName, C.CategoryName, SC.SubCategoryName, PS.RetailPrice, PS.WholeSalePrice, PS.CreditPrice, UM.UnitName
	
	) FinalData
	ORDER BY ItemName

DROP TABLE #data
END

