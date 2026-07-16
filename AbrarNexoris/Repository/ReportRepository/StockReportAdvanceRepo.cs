using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ReportRepository
{
    /// <summary>
    /// Repository for Stock Report Advanced operations
    /// </summary>
    public class StockReportAdvanceRepo : BaseRepostitory
    {
        /// <summary>
        /// Get Stock Report data with applied filters
        /// </summary>
        /// <param name="filter">Stock Report Filter parameters</param>
        /// <returns>List of StockReportItem</returns>
        /// <summary>
        /// Get Stock Report data with applied filters
        /// </summary>
        /// <param name="filter">Stock Report Filter parameters</param>
        /// <returns>List of StockReportItem</returns>
        public List<ModelClass.Report.StockReportItem> GetStockReport(ModelClass.Report.StockReportFilter filter)
        {
            List<ModelClass.Report.StockReportItem> reportData = new List<ModelClass.Report.StockReportItem>();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_StockReportAdvanced, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 180; // 3 minutes timeout for large data

                    cmd.Parameters.AddWithValue("@FromDate", filter.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", filter.ToDate);
                    cmd.Parameters.AddWithValue("@CompanyId", filter.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", filter.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", filter.FinYearId);
                    cmd.Parameters.AddWithValue("@BarcodeContains", string.IsNullOrEmpty(filter.BarcodeContains) ? (object)DBNull.Value : filter.BarcodeContains);
                    cmd.Parameters.AddWithValue("@GroupId", filter.GroupId.HasValue && filter.GroupId.Value > 0 ? filter.GroupId.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CategoryId", filter.CategoryId.HasValue && filter.CategoryId.Value > 0 ? filter.CategoryId.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SubCategoryId", filter.SubCategoryId.HasValue && filter.SubCategoryId.Value > 0 ? filter.SubCategoryId.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LedgerId", filter.LedgerId.HasValue && filter.LedgerId.Value > 0 ? filter.LedgerId.Value : (object)DBNull.Value);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                reportData.Add(new ModelClass.Report.StockReportItem
                                {
                                    ItemId = row["ItemId"] != DBNull.Value ? Convert.ToInt32(row["ItemId"]) : 0,
                                    GroupName = row["GroupName"]?.ToString() ?? "",
                                    CategoryName = row["CategoryName"]?.ToString() ?? "",
                                    SubCategoryName = row["SubCategoryName"]?.ToString() ?? "",
                                    Barcode = row["Barcode"]?.ToString() ?? "",
                                    ItemName = row["ItemName"]?.ToString() ?? "",
                                    OpeningStock = row["OpeningStock"] != DBNull.Value ? Convert.ToDecimal(row["OpeningStock"]) : 0,
                                    Purchase = row["Purchase"] != DBNull.Value ? Convert.ToDecimal(row["Purchase"]) : 0,
                                    PurchaseReturn = row["PurchaseReturn"] != DBNull.Value ? Convert.ToDecimal(row["PurchaseReturn"]) : 0,
                                    StockAdjustmentIn = row["StockAdjustmentIn"] != DBNull.Value ? Convert.ToDecimal(row["StockAdjustmentIn"]) : 0,
                                    StockAdjustmentOut = row["StockAdjustmentOut"] != DBNull.Value ? Convert.ToDecimal(row["StockAdjustmentOut"]) : 0,
                                    StockTransferIn = row["StockTransferIn"] != DBNull.Value ? Convert.ToDecimal(row["StockTransferIn"]) : 0,
                                    StockTransferOut = row["StockTransferOut"] != DBNull.Value ? Convert.ToDecimal(row["StockTransferOut"]) : 0,
                                    Sales = row["Sales"] != DBNull.Value ? Convert.ToDecimal(row["Sales"]) : 0,
                                    SalesReturn = row["SalesReturn"] != DBNull.Value ? Convert.ToDecimal(row["SalesReturn"]) : 0,
                                    ClosingStock = row["ClosingStock"] != DBNull.Value ? Convert.ToDecimal(row["ClosingStock"]) : 0,
                                    OrderedStock = row["OrderedStock"] != DBNull.Value ? Convert.ToDecimal(row["OrderedStock"]) : 0,
                                    HoldQty = row.Table.Columns.Contains("HoldQty") && row["HoldQty"] != DBNull.Value ? Convert.ToDecimal(row["HoldQty"]) : 0,
                                    Cost = row["Cost"] != DBNull.Value ? Convert.ToDecimal(row["Cost"]) : 0,
                                    RetailPrice = row["RetailPrice"] != DBNull.Value ? Convert.ToDecimal(row["RetailPrice"]) : 0,
                                    WholeSalePrice = row["WholeSalePrice"] != DBNull.Value ? Convert.ToDecimal(row["WholeSalePrice"]) : 0,
                                    CreditPrice = row["CreditPrice"] != DBNull.Value ? Convert.ToDecimal(row["CreditPrice"]) : 0,
                                    BaseUnitName = row["BaseUnitName"]?.ToString() ?? "",
                                    Profit = row["Profit"] != DBNull.Value ? Convert.ToDecimal(row["Profit"]) : 0,
                                    SaleAmount = row["SaleAmount"] != DBNull.Value ? Convert.ToDecimal(row["SaleAmount"]) : 0
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving Stock Report data. {ex.Message}", ex);
            }
            finally
            {
                DataConnection.Close();
            }

            return reportData;
        }

        /// <summary>
        /// Get Stock Report with simple parameters
        /// </summary>
        public List<ModelClass.Report.StockReportItem> GetStockReport(DateTime fromDate, DateTime toDate, int companyId, int branchId, int finYearId,
            string barcodeContains = null, int? groupId = null, int? categoryId = null, int? subCategoryId = null, int? ledgerId = null)
        {
            var filter = new ModelClass.Report.StockReportFilter
            {
                FromDate = fromDate,
                ToDate = toDate,
                CompanyId = companyId,
                BranchId = branchId,
                FinYearId = finYearId,
                BarcodeContains = barcodeContains,
                GroupId = groupId,
                CategoryId = categoryId,
                SubCategoryId = subCategoryId,
                LedgerId = ledgerId
            };

            return GetStockReport(filter);
        }

        public DataTable GetStockTransactionValues(ModelClass.Report.StockReportFilter filter)
        {
            DataTable result = new DataTable();
            DataConnection.Open();

            try
            {
                string sql = @"
DECLARE @ExclusiveToDate datetime = DATEADD(DAY, 1, CAST(@ToDate AS date));

SELECT
    COUNT(*) OVER ()
        - ROW_NUMBER() OVER (ORDER BY SortDate DESC, SortLogId DESC, SortDocNumber DESC, SortLineNo DESC, ItemName)
        + 1 AS Rank,
    Movement,
    DocNumber,
    TransactionDate,
    ItemName,
    Qty,
    Cost,
    SellingPrice,
    StockValue
FROM
(
    SELECT
        'Purchase' AS Movement,
        CAST(PM.PurchaseNo AS nvarchar(50)) AS DocNumber,
        COALESCE(PAL.CreatedOn, CAST(CAST(PD.PurchaseDate AS date) AS datetime)) AS TransactionDate,
        ISNULL(NULLIF(IM.[Description], ''), 'Unknown Item') AS ItemName,
        CAST(((ISNULL(PD.Packing, 0) * ISNULL(PD.Qty, 0)) + ISNULL(PD.Free, 0)) AS decimal(18, 2)) AS Qty,
        CAST(ISNULL(PD.Cost, 0) AS decimal(18, 2)) AS Cost,
        CAST(ISNULL(PS.RetailPrice, 0) AS decimal(18, 2)) AS SellingPrice,
        CAST(CASE WHEN PD.TaxType = 'I'
            THEN (ISNULL(PD.Cost, 0) * ISNULL(PD.Qty, 0)) - ISNULL(PD.TaxAmt, 0) - ISNULL(PD.CessAmt, 0)
            ELSE ISNULL(PD.Cost, 0) * ISNULL(PD.Qty, 0)
        END AS decimal(18, 2)) AS StockValue,
        COALESCE(PAL.CreatedOn, CAST(CAST(PD.PurchaseDate AS date) AS datetime)) AS SortDate,
        ISNULL(PAL.ActivityLogId, 0) AS SortLogId,
        CAST(ISNULL(PM.PurchaseNo, 0) AS bigint) AS SortDocNumber,
        CAST(ISNULL(PD.SlNo, 0) AS bigint) AS SortLineNo
    FROM PDetails PD
    INNER JOIN PMaster PM ON PD.BranchID = PM.BranchId AND PD.FinYearId = PM.FinYearId AND PD.PurchaseNo = PM.PurchaseNo
    LEFT JOIN ItemMaster IM ON IM.ItemId = PD.ItemID
    LEFT JOIN PriceSettings PS ON PS.BranchId = PD.BranchId AND PS.ItemId = PD.ItemID AND PS.IsBaseUnit = 'Y'
    OUTER APPLY
    (
        SELECT TOP 1 PAL.ActivityLogId, PAL.CreatedOn
        FROM PurchaseActivityLog PAL
        WHERE PAL.TransactionNo = PM.PurchaseNo
          AND (ISNULL(PAL.CompanyId, 0) = 0 OR ISNULL(PAL.CompanyId, 0) = ISNULL(PM.CompanyId, 0))
          AND (ISNULL(PAL.BranchId, 0) = 0 OR ISNULL(PAL.BranchId, 0) = ISNULL(PM.BranchId, 0))
          AND (ISNULL(PAL.FinYearId, 0) = 0 OR ISNULL(PAL.FinYearId, 0) = ISNULL(PM.FinYearId, 0))
          AND ISNULL(PAL.ActivityType, '') IN ('SAVE', 'UPDATE')
        ORDER BY PAL.CreatedOn DESC, PAL.ActivityLogId DESC
    ) PAL
    WHERE ISNULL(PM.CancelFlag, 0) = 0
      AND PD.FinYearId = @FinYearId
      AND PD.CompanyId = @CompanyId
      AND PD.BranchId = CASE WHEN @BranchId IS NOT NULL AND @BranchId <> 0 THEN @BranchId ELSE PD.BranchId END
      AND COALESCE(PAL.CreatedOn, CAST(CAST(PD.PurchaseDate AS date) AS datetime)) >= CAST(@FromDate AS date)
      AND COALESCE(PAL.CreatedOn, CAST(CAST(PD.PurchaseDate AS date) AS datetime)) < @ExclusiveToDate

    UNION ALL

    SELECT
        'Sold' AS Movement,
        CAST(SM.BillNo AS nvarchar(50)) AS DocNumber,
        COALESCE(SAL.CreatedOn, SD.BillDate) AS TransactionDate,
        ISNULL(NULLIF(IM.[Description], ''), 'Unknown Item') AS ItemName,
        CAST(ISNULL(SD.Packing, 0) * ISNULL(SD.Qty, 0) AS decimal(18, 2)) AS Qty,
        CAST(ISNULL(SD.Cost, 0) AS decimal(18, 2)) AS Cost,
        CAST(CASE WHEN ISNULL(SD.Qty, 0) = 0 THEN ISNULL(PS.RetailPrice, 0) ELSE ISNULL(SD.TotalAmount, 0) / NULLIF(SD.Qty, 0) END AS decimal(18, 2)) AS SellingPrice,
        CAST(CASE WHEN ISNULL(SM.Status, '') <> 'Hold' THEN ISNULL(SD.Cost, 0) * ISNULL(SD.Qty, 0) ELSE 0 END AS decimal(18, 2)) AS StockValue,
        COALESCE(SAL.CreatedOn, SD.BillDate) AS SortDate,
        ISNULL(SAL.ActivityLogId, 0) AS SortLogId,
        CAST(ISNULL(SM.BillNo, 0) AS bigint) AS SortDocNumber,
        CAST(ISNULL(SD.SlNo, 0) AS bigint) AS SortLineNo
    FROM SDetails SD
    INNER JOIN SMaster SM ON SD.BranchID = SM.BranchId AND SD.FinYearId = SM.FinYearId AND SD.BillNo = SM.BillNo
    LEFT JOIN ItemMaster IM ON IM.ItemId = SD.ItemID
    LEFT JOIN PriceSettings PS ON PS.BranchId = SD.BranchId AND PS.ItemId = SD.ItemID AND PS.IsBaseUnit = 'Y'
    OUTER APPLY
    (
        SELECT TOP 1 SAL.ActivityLogId, SAL.CreatedOn
        FROM SalesActivityLog SAL
        WHERE SAL.TransactionNo = SM.BillNo
          AND (ISNULL(SAL.CompanyId, 0) = 0 OR ISNULL(SAL.CompanyId, 0) = ISNULL(SM.CompanyId, 0))
          AND (ISNULL(SAL.BranchId, 0) = 0 OR ISNULL(SAL.BranchId, 0) = ISNULL(SM.BranchId, 0))
          AND (ISNULL(SAL.FinYearId, 0) = 0 OR ISNULL(SAL.FinYearId, 0) = ISNULL(SM.FinYearId, 0))
          AND ISNULL(SAL.ActivityType, '') IN ('SAVE', 'UPDATE', 'HOLD')
        ORDER BY SAL.CreatedOn DESC, SAL.ActivityLogId DESC
    ) SAL
    WHERE ISNULL(SD.CancelFlag, 0) = 0
      AND ISNULL(SM.CancelFlag, 0) = 0
      AND SD.FinYearId = @FinYearId
      AND SD.CompanyId = @CompanyId
      AND SD.BranchId = CASE WHEN @BranchId IS NOT NULL AND @BranchId <> 0 THEN @BranchId ELSE SD.BranchId END
      AND COALESCE(SAL.CreatedOn, SD.BillDate) >= CAST(@FromDate AS date)
      AND COALESCE(SAL.CreatedOn, SD.BillDate) < @ExclusiveToDate

    UNION ALL

    SELECT
        'Purchase Return' AS Movement,
        CAST(PRM.PReturnNo AS nvarchar(50)) AS DocNumber,
        COALESCE(PRAL.CreatedOn, PRD.PReturnDate) AS TransactionDate,
        ISNULL(NULLIF(IM.[Description], ''), 'Unknown Item') AS ItemName,
        CAST(ISNULL(PRD.Packing, 0) * ISNULL(PRD.Returned, 0) AS decimal(18, 2)) AS Qty,
        CAST(ISNULL(PRD.Cost, 0) AS decimal(18, 2)) AS Cost,
        CAST(ISNULL(PS.RetailPrice, 0) AS decimal(18, 2)) AS SellingPrice,
        CAST(CASE WHEN PRD.TaxType = 'I'
            THEN (ISNULL(PRD.Cost, 0) * ISNULL(PRD.Returned, 0)) - ISNULL(PRD.TaxAmt, 0) - ISNULL(PRD.CessAmt, 0)
            ELSE ISNULL(PRD.Cost, 0) * ISNULL(PRD.Returned, 0)
        END AS decimal(18, 2)) AS StockValue,
        COALESCE(PRAL.CreatedOn, PRD.PReturnDate) AS SortDate,
        ISNULL(PRAL.ActivityLogId, 0) AS SortLogId,
        CAST(ISNULL(PRM.PReturnNo, 0) AS bigint) AS SortDocNumber,
        CAST(ISNULL(PRD.SlNo, 0) AS bigint) AS SortLineNo
    FROM PReturnDetails PRD
    INNER JOIN PReturnMaster PRM ON PRD.BranchID = PRM.BranchId AND PRD.FinYearId = PRM.FinYearId AND PRD.PReturnNo = PRM.PReturnNo
    LEFT JOIN ItemMaster IM ON IM.ItemId = PRD.ItemID
    LEFT JOIN PriceSettings PS ON PS.BranchId = PRD.BranchId AND PS.ItemId = PRD.ItemID AND PS.IsBaseUnit = 'Y'
    OUTER APPLY
    (
        SELECT TOP 1 PRAL.ActivityLogId, PRAL.CreatedOn
        FROM PurchaseReturnActivityLog PRAL
        WHERE PRAL.TransactionNo = PRM.PReturnNo
          AND (ISNULL(PRAL.CompanyId, 0) = 0 OR ISNULL(PRAL.CompanyId, 0) = ISNULL(PRM.CompanyId, 0))
          AND (ISNULL(PRAL.BranchId, 0) = 0 OR ISNULL(PRAL.BranchId, 0) = ISNULL(PRM.BranchId, 0))
          AND (ISNULL(PRAL.FinYearId, 0) = 0 OR ISNULL(PRAL.FinYearId, 0) = ISNULL(PRM.FinYearId, 0))
          AND ISNULL(PRAL.ActivityType, '') IN ('SAVE', 'UPDATE')
        ORDER BY PRAL.CreatedOn DESC, PRAL.ActivityLogId DESC
    ) PRAL
    WHERE ISNULL(PRM.CancelFlag, 0) = 0
      AND PRD.FinYearId = @FinYearId
      AND PRD.CompanyId = @CompanyId
      AND PRD.BranchId = CASE WHEN @BranchId IS NOT NULL AND @BranchId <> 0 THEN @BranchId ELSE PRD.BranchId END
      AND COALESCE(PRAL.CreatedOn, PRD.PReturnDate) >= CAST(@FromDate AS date)
      AND COALESCE(PRAL.CreatedOn, PRD.PReturnDate) < @ExclusiveToDate

    UNION ALL

    SELECT
        'Sales Return' AS Movement,
        CAST(SRM.SReturnNo AS nvarchar(50)) AS DocNumber,
        COALESCE(SRAL.CreatedOn, SRD.SReturnDate) AS TransactionDate,
        ISNULL(NULLIF(IM.[Description], ''), 'Unknown Item') AS ItemName,
        CAST(ISNULL(SRD.Packing, 0) * ISNULL(SRD.ReturnedQty, 0) AS decimal(18, 2)) AS Qty,
        CAST(ISNULL(SRD.Cost, 0) AS decimal(18, 2)) AS Cost,
        CAST(ISNULL(PS.RetailPrice, 0) AS decimal(18, 2)) AS SellingPrice,
        CAST(ISNULL(SRD.Cost, 0) * ISNULL(SRD.ReturnedQty, 0) AS decimal(18, 2)) AS StockValue,
        COALESCE(SRAL.CreatedOn, SRD.SReturnDate) AS SortDate,
        ISNULL(SRAL.ActivityLogId, 0) AS SortLogId,
        CAST(ISNULL(SRM.SReturnNo, 0) AS bigint) AS SortDocNumber,
        CAST(ISNULL(SRD.SlNo, 0) AS bigint) AS SortLineNo
    FROM SReturnDetails SRD
    INNER JOIN SReturnMaster SRM ON SRD.BranchID = SRM.BranchId AND SRD.FinYearId = SRM.FinYearId AND SRD.SReturnNo = SRM.SReturnNo
    LEFT JOIN ItemMaster IM ON IM.ItemId = SRD.ItemID
    LEFT JOIN PriceSettings PS ON PS.BranchId = SRD.BranchId AND PS.ItemId = SRD.ItemID AND PS.IsBaseUnit = 'Y'
    OUTER APPLY
    (
        SELECT TOP 1 SRAL.ActivityLogId, SRAL.CreatedOn
        FROM SalesReturnActivityLog SRAL
        WHERE SRAL.TransactionNo = SRM.SReturnNo
          AND (ISNULL(SRAL.CompanyId, 0) = 0 OR ISNULL(SRAL.CompanyId, 0) = ISNULL(SRM.CompanyId, 0))
          AND (ISNULL(SRAL.BranchId, 0) = 0 OR ISNULL(SRAL.BranchId, 0) = ISNULL(SRM.BranchId, 0))
          AND (ISNULL(SRAL.FinYearId, 0) = 0 OR ISNULL(SRAL.FinYearId, 0) = ISNULL(SRM.FinYearId, 0))
          AND ISNULL(SRAL.ActivityType, '') IN ('SAVE', 'UPDATE')
        ORDER BY SRAL.CreatedOn DESC, SRAL.ActivityLogId DESC
    ) SRAL
    WHERE ISNULL(SRM.CancelFlag, 0) = 0
      AND SRD.FinYearId = @FinYearId
      AND SRD.CompanyId = @CompanyId
      AND SRD.BranchId = CASE WHEN @BranchId IS NOT NULL AND @BranchId <> 0 THEN @BranchId ELSE SRD.BranchId END
      AND COALESCE(SRAL.CreatedOn, SRD.SReturnDate) >= CAST(@FromDate AS date)
      AND COALESCE(SRAL.CreatedOn, SRD.SReturnDate) < @ExclusiveToDate

    UNION ALL

    SELECT
        'Stock In' AS Movement,
        CAST(SAM.StockAdjustmentNo AS nvarchar(50)) AS DocNumber,
        COALESCE(SAAL.CreatedOn, SAM.StockAdjustmentDate) AS TransactionDate,
        ISNULL(NULLIF(IM.[Description], ''), 'Unknown Item') AS ItemName,
        CAST(ISNULL(UM.Packing, 1) * ISNULL(SAD.QtyDifference, 0) AS decimal(18, 2)) AS Qty,
        CAST(ISNULL(SAD.Cost, 0) AS decimal(18, 2)) AS Cost,
        CAST(ISNULL(PS.RetailPrice, 0) AS decimal(18, 2)) AS SellingPrice,
        CAST(ISNULL(SAD.Cost, 0) * ISNULL(SAD.QtyDifference, 0) AS decimal(18, 2)) AS StockValue,
        COALESCE(SAAL.CreatedOn, SAM.StockAdjustmentDate) AS SortDate,
        ISNULL(SAAL.ActivityLogId, 0) AS SortLogId,
        CAST(ISNULL(SAM.StockAdjustmentNo, 0) AS bigint) AS SortDocNumber,
        CAST(ISNULL(SAD.SlNo, 0) AS bigint) AS SortLineNo
    FROM StockAdjustmentDetails SAD
    INNER JOIN StockAdjustmentMaster SAM ON SAM.Id = SAD.StockAdjustmentMasterId
    LEFT JOIN ItemMaster IM ON IM.ItemId = SAD.ItemId
    LEFT JOIN UnitMaster UM ON UM.UnitId = SAD.UnitId
    LEFT JOIN PriceSettings PS ON PS.BranchId = SAD.BranchId AND PS.ItemId = SAD.ItemId AND PS.IsBaseUnit = 'Y'
    OUTER APPLY
    (
        SELECT TOP 1 SAAL.ActivityLogId, SAAL.CreatedOn
        FROM StockAdjustmentActivityLog SAAL
        WHERE SAAL.TransactionNo = SAM.StockAdjustmentNo
          AND (ISNULL(SAAL.CompanyId, 0) = 0 OR ISNULL(SAAL.CompanyId, 0) = ISNULL(SAM.CompanyId, 0))
          AND (ISNULL(SAAL.BranchId, 0) = 0 OR ISNULL(SAAL.BranchId, 0) = ISNULL(SAM.BranchId, 0))
          AND (ISNULL(SAAL.FinYearId, 0) = 0 OR ISNULL(SAAL.FinYearId, 0) = ISNULL(SAM.FinYearId, 0))
          AND ISNULL(SAAL.ActivityType, '') IN ('SAVE', 'UPDATE')
        ORDER BY SAAL.CreatedOn DESC, SAAL.ActivityLogId DESC
    ) SAAL
    WHERE ISNULL(SAM.CancelFlag, 0) = 0
      AND SAD.FinYearId = @FinYearId
      AND SAD.CompanyId = @CompanyId
      AND SAD.BranchId = CASE WHEN @BranchId IS NOT NULL AND @BranchId <> 0 THEN @BranchId ELSE SAD.BranchId END
      AND SAD.QtyDifference > 0
      AND COALESCE(SAAL.CreatedOn, SAM.StockAdjustmentDate) >= CAST(@FromDate AS date)
      AND COALESCE(SAAL.CreatedOn, SAM.StockAdjustmentDate) < @ExclusiveToDate

    UNION ALL

    SELECT
        'Stock Out' AS Movement,
        CAST(SAM.StockAdjustmentNo AS nvarchar(50)) AS DocNumber,
        COALESCE(SAAL.CreatedOn, SAM.StockAdjustmentDate) AS TransactionDate,
        ISNULL(NULLIF(IM.[Description], ''), 'Unknown Item') AS ItemName,
        CAST(ISNULL(UM.Packing, 1) * (ISNULL(SAD.QtyDifference, 0) * -1) AS decimal(18, 2)) AS Qty,
        CAST(ISNULL(SAD.Cost, 0) AS decimal(18, 2)) AS Cost,
        CAST(ISNULL(PS.RetailPrice, 0) AS decimal(18, 2)) AS SellingPrice,
        CAST(ISNULL(SAD.Cost, 0) * (ISNULL(SAD.QtyDifference, 0) * -1) AS decimal(18, 2)) AS StockValue,
        COALESCE(SAAL.CreatedOn, SAM.StockAdjustmentDate) AS SortDate,
        ISNULL(SAAL.ActivityLogId, 0) AS SortLogId,
        CAST(ISNULL(SAM.StockAdjustmentNo, 0) AS bigint) AS SortDocNumber,
        CAST(ISNULL(SAD.SlNo, 0) AS bigint) AS SortLineNo
    FROM StockAdjustmentDetails SAD
    INNER JOIN StockAdjustmentMaster SAM ON SAM.Id = SAD.StockAdjustmentMasterId
    LEFT JOIN ItemMaster IM ON IM.ItemId = SAD.ItemId
    LEFT JOIN UnitMaster UM ON UM.UnitId = SAD.UnitId
    LEFT JOIN PriceSettings PS ON PS.BranchId = SAD.BranchId AND PS.ItemId = SAD.ItemId AND PS.IsBaseUnit = 'Y'
    OUTER APPLY
    (
        SELECT TOP 1 SAAL.ActivityLogId, SAAL.CreatedOn
        FROM StockAdjustmentActivityLog SAAL
        WHERE SAAL.TransactionNo = SAM.StockAdjustmentNo
          AND (ISNULL(SAAL.CompanyId, 0) = 0 OR ISNULL(SAAL.CompanyId, 0) = ISNULL(SAM.CompanyId, 0))
          AND (ISNULL(SAAL.BranchId, 0) = 0 OR ISNULL(SAAL.BranchId, 0) = ISNULL(SAM.BranchId, 0))
          AND (ISNULL(SAAL.FinYearId, 0) = 0 OR ISNULL(SAAL.FinYearId, 0) = ISNULL(SAM.FinYearId, 0))
          AND ISNULL(SAAL.ActivityType, '') IN ('SAVE', 'UPDATE')
        ORDER BY SAAL.CreatedOn DESC, SAAL.ActivityLogId DESC
    ) SAAL
    WHERE ISNULL(SAM.CancelFlag, 0) = 0
      AND SAD.FinYearId = @FinYearId
      AND SAD.CompanyId = @CompanyId
      AND SAD.BranchId = CASE WHEN @BranchId IS NOT NULL AND @BranchId <> 0 THEN @BranchId ELSE SAD.BranchId END
      AND SAD.QtyDifference < 0
      AND COALESCE(SAAL.CreatedOn, SAM.StockAdjustmentDate) >= CAST(@FromDate AS date)
      AND COALESCE(SAAL.CreatedOn, SAM.StockAdjustmentDate) < @ExclusiveToDate
) rows
ORDER BY SortDate DESC, SortLogId DESC, SortDocNumber DESC, SortLineNo DESC, ItemName;";

                using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = 180;
                    cmd.Parameters.AddWithValue("@FromDate", filter.FromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", filter.ToDate.Date);
                    cmd.Parameters.AddWithValue("@CompanyId", filter.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", filter.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", filter.FinYearId);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving stock transaction values. {ex.Message}", ex);
            }
            finally
            {
                DataConnection.Close();
            }

            return result;
        }
    }
}
