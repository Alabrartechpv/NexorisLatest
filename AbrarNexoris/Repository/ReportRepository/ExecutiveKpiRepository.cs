using Dapper;
using ModelClass;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace Repository.ReportRepository
{
    public class ExecutiveKpiRepository : BaseRepostitory
    {
        public ExecutiveKpiModel GetExecutiveKpiData(DateTime fromDate, DateTime toDate, int branchId = 0, int companyId = 0, int finYearId = 0)
        {
            var model = new ExecutiveKpiModel();

            try
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
                DataConnection.Open();

                DateTime rangeFrom = fromDate.Date;
                DateTime rangeTo = toDate.Date;
                if (rangeTo < rangeFrom)
                {
                    DateTime swap = rangeFrom;
                    rangeFrom = rangeTo;
                    rangeTo = swap;
                }

                int effectiveBranch = branchId > 0 ? branchId : SessionContext.BranchId;
                int effectiveCompany = companyId > 0 ? companyId : SessionContext.CompanyId;
                int effectiveFinYear = finYearId > 0 ? finYearId : SessionContext.FinYearId;

                if (effectiveBranch <= 0) int.TryParse(DataBase.BranchId, out effectiveBranch);
                if (effectiveCompany <= 0) int.TryParse(DataBase.CompanyId, out effectiveCompany);
                if (effectiveFinYear <= 0) int.TryParse(DataBase.FinyearId, out effectiveFinYear);

                model.FromDate = rangeFrom;
                model.ToDate = rangeTo;
                model.BranchId = effectiveBranch;
                model.CompanyId = effectiveCompany;
                model.FinYearId = effectiveFinYear;
                model.BranchName = !string.IsNullOrWhiteSpace(SessionContext.BranchName) ? SessionContext.BranchName : DataBase.Branch;
                EnsureExecutiveDashboardKPIsStoredProcedure();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_ExecutiveDashboardKPIs, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 90;
                    cmd.Parameters.AddWithValue("@CompanyId", effectiveCompany);
                    cmd.Parameters.AddWithValue("@BranchId", effectiveBranch);
                    cmd.Parameters.AddWithValue("@FinYearId", effectiveFinYear);
                    cmd.Parameters.AddWithValue("@FromDate", rangeFrom);
                    cmd.Parameters.AddWithValue("@ToDate", rangeTo);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapter.Fill(ds);

                        // Result Set 1: Scalar KPIs
                        if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            DataRow r = ds.Tables[0].Rows[0];

                            // 1. Inventory & Stock
                            model.TotalStockCostValue = GetDecimal(r, "TotalStockCostValue");
                            model.TotalStockRetailValue = GetDecimal(r, "TotalStockRetailValue");
                            model.StockProfitPotential = Math.Max(0, model.TotalStockRetailValue - model.TotalStockCostValue);
                            model.TotalStockItemCount = GetInt(r, "TotalStockItemCount");
                            model.TotalStockQuantity = GetDecimal(r, "TotalStockQuantity");

                            model.NegativeStockImpactValue = GetDecimal(r, "NegativeStockImpactValue");
                            model.NegativeStockItemCount = GetInt(r, "NegativeStockItemCount");
                            model.NegativeStockTotalQty = GetDecimal(r, "NegativeStockTotalQty");

                            model.LowStockAlertCount = GetInt(r, "LowStockAlertCount");
                            model.ExcessStockAlertCount = GetInt(r, "ExcessStockAlertCount");
                            model.ReorderAlertCount = GetInt(r, "ReorderAlertCount");

                            model.LossStockValue = GetDecimal(r, "LossStockValue");
                            model.LossStockQty = GetDecimal(r, "LossStockQty");
                            model.ExtraStockValue = GetDecimal(r, "ExtraStockValue");
                            model.ExtraStockQty = GetDecimal(r, "ExtraStockQty");
                            model.NetStockAdjustmentValue = GetDecimal(r, "NetStockAdjustmentValue");

                            // 2. Sales & Purchases
                            model.TotalSalesRevenue = GetDecimal(r, "TotalSalesRevenue");
                            model.TotalSalesBillCount = GetInt(r, "TotalSalesBillCount");
                            model.TotalPurchases = GetDecimal(r, "TotalPurchases");
                            model.GrossProfit = GetDecimal(r, "GrossProfit");

                            // 3. Expenses & Profitability
                            model.DirectExpenses = GetDecimal(r, "DirectExpenses");
                            model.IndirectExpenses = GetDecimal(r, "IndirectExpenses");
                            model.TotalBusinessExpenses = GetDecimal(r, "TotalBusinessExpenses");
                            model.ActualNetProfit = GetDecimal(r, "ActualNetProfit");
                            model.OperatingProfitMarginPercent = GetDecimal(r, "OperatingProfitMarginPercent");
                            model.OwnerDrawings = GetDecimal(r, "OwnerDrawings");
                            model.CustomerBadDebts = GetDecimal(r, "CustomerBadDebts");
                            model.SupplierWriteOffs = GetDecimal(r, "SupplierWriteOffs");

                            // 4. Working Capital & Balances
                            model.CashInHand = GetDecimal(r, "CashInHand");
                            model.BankBalance = GetDecimal(r, "BankBalance");
                            model.SupplierPayables = GetDecimal(r, "SupplierPayables");
                            model.SupplierPayablesCount = GetInt(r, "SupplierPayablesCount");
                            model.CustomerReceivables = GetDecimal(r, "CustomerReceivables");
                            model.CustomerReceivablesCount = GetInt(r, "CustomerReceivablesCount");
                            model.SupplierAdvanceBalance = GetDecimal(r, "SupplierAdvanceBalance");
                            model.CustomerAdvanceBalance = GetDecimal(r, "CustomerAdvanceBalance");
                            model.TotalAssets = GetDecimal(r, "TotalAssets");
                            model.TotalLiabilities = GetDecimal(r, "TotalLiabilities");
                            model.NetBusinessAsset = GetDecimal(r, "NetBusinessAsset");

                            // 5. Aging
                            model.DelayedCustomerReceivables30Days = GetDecimal(r, "DelayedCustomerReceivables30Days");
                            model.DelayedCustomerCount30Days = GetInt(r, "DelayedCustomerCount30Days");
                            model.DelayedSupplierPayables30Days = GetDecimal(r, "DelayedSupplierPayables30Days");
                            model.DelayedSupplierCount30Days = GetInt(r, "DelayedSupplierCount30Days");

                            // 6. Audit
                            model.DeletionCount = GetInt(r, "DeletionCount");
                            model.PriceChangeCount = GetInt(r, "PriceChangeCount");
                            model.StockAdjustmentCount = GetInt(r, "StockAdjustmentCount");
                        }

                        // Result Set 2: Monthly Growth Matrix
                        model.MonthlyGrowthMatrix = new List<MonthlyGrowthMatrixItem>();
                        if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                        {
                            decimal prevSales = 0;
                            decimal prevProfit = 0;
                            bool isFirst = true;

                            foreach (DataRow row in ds.Tables[1].Rows)
                            {
                                decimal curSales = GetDecimal(row, "SalesAmount");
                                decimal curPurchases = GetDecimal(row, "PurchaseAmount");
                                decimal curExpenses = GetDecimal(row, "ExpenseAmount");
                                decimal curProfit = GetDecimal(row, "NetProfitAmount");

                                decimal salesGrowth = 0;
                                decimal profitGrowth = 0;

                                if (!isFirst && prevSales > 0)
                                {
                                    salesGrowth = Math.Round(((curSales - prevSales) / prevSales) * 100m, 1);
                                }
                                if (!isFirst && prevProfit != 0)
                                {
                                    profitGrowth = Math.Round(((curProfit - prevProfit) / Math.Abs(prevProfit)) * 100m, 1);
                                }

                                model.MonthlyGrowthMatrix.Add(new MonthlyGrowthMatrixItem
                                {
                                    MonthName = row["MonthName"]?.ToString() ?? "",
                                    Year = GetInt(row, "Year"),
                                    Month = GetInt(row, "Month"),
                                    SalesAmount = curSales,
                                    SalesGrowthPercent = salesGrowth,
                                    PurchaseAmount = curPurchases,
                                    ExpenseAmount = curExpenses,
                                    NetProfitAmount = curProfit,
                                    NetProfitGrowthPercent = profitGrowth
                                });

                                prevSales = curSales;
                                prevProfit = curProfit;
                                isFirst = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading Executive KPI Data: {ex.Message}");
                throw;
            }
            finally
            {
                if (DataConnection != null && DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return model;
        }

        private static decimal GetDecimal(DataRow row, string colName)
        {
            if (row.Table.Columns.Contains(colName) && row[colName] != DBNull.Value)
            {
                decimal.TryParse(row[colName].ToString(), out decimal val);
                return val;
            }
            return 0;
        }

        private static int GetInt(DataRow row, string colName)
        {
            if (row.Table.Columns.Contains(colName) && row[colName] != DBNull.Value)
            {
                int.TryParse(row[colName].ToString(), out int val);
                return val;
            }
            return 0;
        }

        private static bool _schemaEnsured = false;
        private void EnsureExecutiveDashboardKPIsStoredProcedure()
        {
            if (_schemaEnsured) return;
            try
            {
                using (SqlCommand cmdEnsure = new SqlCommand(@"
IF OBJECT_ID(N'dbo._POS_ExecutiveDashboardKPIs', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo._POS_ExecutiveDashboardKPIs AS BEGIN SET NOCOUNT ON; END');", (SqlConnection)DataConnection))
                {
                    cmdEnsure.ExecuteNonQuery();
                }

                using (SqlCommand cmdAlter = new SqlCommand(@"
ALTER PROCEDURE dbo._POS_ExecutiveDashboardKPIs
    @CompanyId INT = 0,
    @BranchId INT = 0,
    @FinYearId INT = 0,
    @FromDate DATETIME,
    @ToDate DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NextDay DATETIME = DATEADD(DAY, 1, CAST(@ToDate AS DATE));
    DECLARE @RangeFrom DATETIME = CAST(@FromDate AS DATE);
    DECLARE @DelayedThresholdDate DATETIME = DATEADD(DAY, -30, @ToDate);

    ---------------------------------------------------------------------------
    -- 1. INVENTORY VALUATION & ALERTS (From PriceSettings & ItemMaster)
    ---------------------------------------------------------------------------
    DECLARE @TotalStockCostValue DECIMAL(18,2) = 0,
            @TotalStockRetailValue DECIMAL(18,2) = 0,
            @TotalStockItemCount INT = 0,
            @TotalStockQuantity DECIMAL(18,2) = 0,
            @NegativeStockImpactValue DECIMAL(18,2) = 0,
            @NegativeStockItemCount INT = 0,
            @NegativeStockTotalQty DECIMAL(18,2) = 0,
            @LowStockAlertCount INT = 0,
            @ExcessStockAlertCount INT = 0,
            @ReorderAlertCount INT = 0;

    IF OBJECT_ID('PriceSettings', 'U') IS NOT NULL
    BEGIN
        SELECT
            @TotalStockCostValue = ISNULL(SUM(CASE WHEN ISNULL(ps.Stock, 0) > 0 THEN ISNULL(ps.Stock, 0) * ISNULL(ps.Cost, 0) ELSE 0 END), 0),
            @TotalStockRetailValue = ISNULL(SUM(CASE WHEN ISNULL(ps.Stock, 0) > 0 THEN ISNULL(ps.Stock, 0) * ISNULL(ps.RetailPrice, 0) ELSE 0 END), 0),
            @TotalStockItemCount = ISNULL(COUNT(CASE WHEN ISNULL(ps.Stock, 0) > 0 THEN 1 END), 0),
            @TotalStockQuantity = ISNULL(SUM(CASE WHEN ISNULL(ps.Stock, 0) > 0 THEN ISNULL(ps.Stock, 0) ELSE 0 END), 0),
            @NegativeStockImpactValue = ISNULL(SUM(CASE WHEN ISNULL(ps.Stock, 0) < 0 THEN ABS(ISNULL(ps.Stock, 0)) * ISNULL(ps.Cost, 0) ELSE 0 END), 0),
            @NegativeStockItemCount = ISNULL(COUNT(CASE WHEN ISNULL(ps.Stock, 0) < 0 THEN 1 END), 0),
            @NegativeStockTotalQty = ISNULL(SUM(CASE WHEN ISNULL(ps.Stock, 0) < 0 THEN ISNULL(ps.Stock, 0) ELSE 0 END), 0),
            @LowStockAlertCount = ISNULL(COUNT(CASE WHEN ISNULL(ps.Stock, 0) > 0 AND ISNULL(ps.ReOrder, 0) > 0 AND ISNULL(ps.Stock, 0) <= ISNULL(ps.ReOrder, 0) THEN 1 END), 0),
            @ExcessStockAlertCount = ISNULL(COUNT(CASE WHEN ISNULL(ps.Stock, 0) > 100 THEN 1 END), 0),
            @ReorderAlertCount = ISNULL(COUNT(CASE WHEN ISNULL(ps.ReOrder, 0) > 0 AND ISNULL(ps.Stock, 0) <= ISNULL(ps.ReOrder, 0) THEN 1 END), 0)
        FROM PriceSettings ps
        WHERE (@BranchId = 0 OR ps.BranchId = @BranchId);
    END

    ---------------------------------------------------------------------------
    -- 2. STOCK ADJUSTMENTS (Loss / Extra Stock)
    ---------------------------------------------------------------------------
    DECLARE @LossStockValue DECIMAL(18,2) = 0,
            @LossStockQty DECIMAL(18,2) = 0,
            @ExtraStockValue DECIMAL(18,2) = 0,
            @ExtraStockQty DECIMAL(18,2) = 0;

    IF OBJECT_ID('StockAdjustmentMaster', 'U') IS NOT NULL AND OBJECT_ID('StockAdjustmentDetails', 'U') IS NOT NULL
    BEGIN
        SELECT
            @ExtraStockValue = ISNULL(SUM(CASE WHEN ISNULL(sad.QtyDifference, 0) > 0 THEN ISNULL(sad.QtyDifference, 0) * ISNULL(sad.Cost, 0) ELSE 0 END), 0),
            @ExtraStockQty = ISNULL(SUM(CASE WHEN ISNULL(sad.QtyDifference, 0) > 0 THEN ISNULL(sad.QtyDifference, 0) ELSE 0 END), 0),
            @LossStockValue = ISNULL(SUM(CASE WHEN ISNULL(sad.QtyDifference, 0) < 0 THEN ABS(ISNULL(sad.QtyDifference, 0)) * ISNULL(sad.Cost, 0) ELSE 0 END), 0),
            @LossStockQty = ISNULL(SUM(CASE WHEN ISNULL(sad.QtyDifference, 0) < 0 THEN ABS(ISNULL(sad.QtyDifference, 0)) ELSE 0 END), 0)
        FROM StockAdjustmentMaster sam
        INNER JOIN StockAdjustmentDetails sad ON sad.StockAdjustmentMasterId = sam.Id
        WHERE sam.StockAdjustmentDate >= @RangeFrom AND sam.StockAdjustmentDate < @NextDay
          AND (@BranchId = 0 OR sam.BranchId = @BranchId)
          AND ISNULL(sam.CancelFlag, 0) = 0
          AND ISNULL(sad.CancelFlag, 0) = 0;
    END

    ---------------------------------------------------------------------------
    -- 3. SALES & PURCHASES
    ---------------------------------------------------------------------------
    DECLARE @TotalSalesRevenue DECIMAL(18,2) = 0,
            @TotalSalesBillCount INT = 0,
            @TotalPurchases DECIMAL(18,2) = 0,
            @TotalSalesCost DECIMAL(18,2) = 0;

    IF OBJECT_ID('SMaster', 'U') IS NOT NULL
    BEGIN
        SELECT
            @TotalSalesRevenue = ISNULL(SUM(ISNULL(NetAmount, 0)), 0),
            @TotalSalesBillCount = ISNULL(COUNT(1), 0),
            @TotalSalesCost = ISNULL(SUM(ISNULL(BillCost, 0)), 0)
        FROM SMaster
        WHERE BillDate >= @RangeFrom AND BillDate < @NextDay
          AND (@BranchId = 0 OR BranchId = @BranchId)
          AND ISNULL(CancelFlag, 0) = 0;
    END

    IF OBJECT_ID('PMaster', 'U') IS NOT NULL
    BEGIN
        SELECT @TotalPurchases = ISNULL(SUM(ISNULL(GrandTotal, 0)), 0)
        FROM PMaster
        WHERE PurchaseDate >= @RangeFrom AND PurchaseDate < @NextDay
          AND (@BranchId = 0 OR BranchId = @BranchId)
          AND ISNULL(CancelFlag, 0) = 0;
    END

    IF @TotalSalesCost = 0 AND @TotalSalesRevenue > 0
        SET @TotalSalesCost = @TotalSalesRevenue * 0.70;

    DECLARE @GrossProfit DECIMAL(18,2) = CASE WHEN @TotalSalesRevenue - @TotalSalesCost > 0 THEN @TotalSalesRevenue - @TotalSalesCost ELSE 0 END;

    ---------------------------------------------------------------------------
    -- 4. EXPENSES, DRAWINGS & WRITE-OFFS (From Vouchers)
    ---------------------------------------------------------------------------
    DECLARE @DirectExpenses DECIMAL(18,2) = 0,
            @IndirectExpenses DECIMAL(18,2) = 0,
            @OwnerDrawings DECIMAL(18,2) = 0,
            @CustomerBadDebts DECIMAL(18,2) = 0,
            @SupplierWriteOffs DECIMAL(18,2) = 0;

    IF OBJECT_ID('Vouchers', 'U') IS NOT NULL AND OBJECT_ID('AccountGroupMaster', 'U') IS NOT NULL
    BEGIN
        SELECT
            @DirectExpenses = ISNULL(SUM(CASE WHEN ag.GroupName LIKE '%Direct Expense%' THEN ISNULL(v.Debit, 0) ELSE 0 END), 0),
            @IndirectExpenses = ISNULL(SUM(CASE WHEN ag.GroupName LIKE '%Indirect Expense%' OR (ag.GroupName LIKE '%Expense%' AND ag.GroupName NOT LIKE '%Direct%') THEN ISNULL(v.Debit, 0) ELSE 0 END), 0),
            @OwnerDrawings = ISNULL(SUM(CASE WHEN ag.GroupName LIKE '%Drawings%' OR v.LedgerName LIKE '%Drawings%' THEN ISNULL(v.Debit, 0) ELSE 0 END), 0),
            @CustomerBadDebts = ISNULL(SUM(CASE WHEN v.LedgerName LIKE '%Bad Debt%' THEN ISNULL(v.Debit, 0) ELSE 0 END), 0),
            @SupplierWriteOffs = ISNULL(SUM(CASE WHEN v.LedgerName LIKE '%Discount Received%' OR v.LedgerName LIKE '%Write Off%' THEN ISNULL(v.Credit, 0) ELSE 0 END), 0)
        FROM Vouchers v
        LEFT JOIN LedgerMaster lm ON lm.LedgerID = v.LedgerID
        LEFT JOIN AccountGroupMaster ag ON ag.GroupID = lm.GroupID
        WHERE v.VoucherDate >= @RangeFrom AND v.VoucherDate < @NextDay
          AND (@BranchId = 0 OR v.BranchID = @BranchId)
          AND ISNULL(v.CancelFlag, 0) = 0;
    END

    DECLARE @TotalExpenses DECIMAL(18,2) = @DirectExpenses + @IndirectExpenses;
    DECLARE @ActualNetProfit DECIMAL(18,2) = @GrossProfit - @TotalExpenses;
    DECLARE @OperatingMarginPercent DECIMAL(18,2) = 0;
    IF @TotalSalesRevenue > 0
        SET @OperatingMarginPercent = ROUND((@ActualNetProfit / @TotalSalesRevenue) * 100.0, 2);

    ---------------------------------------------------------------------------
    -- 5. CASH, BANK, RECEIVABLES & PAYABLES
    ---------------------------------------------------------------------------
    DECLARE @CashInHand DECIMAL(18,2) = 0,
            @BankBalance DECIMAL(18,2) = 0,
            @SupplierPayables DECIMAL(18,2) = 0,
            @SupplierPayablesCount INT = 0,
            @CustomerReceivables DECIMAL(18,2) = 0,
            @CustomerReceivablesCount INT = 0,
            @SupplierAdvanceBalance DECIMAL(18,2) = 0,
            @CustomerAdvanceBalance DECIMAL(18,2) = 0;

    -- Cash & Bank: JOIN on LedgerMaster.GroupID (reliable) instead of Vouchers.GroupID
    IF OBJECT_ID('Vouchers', 'U') IS NOT NULL AND OBJECT_ID('AccountGroupMaster', 'U') IS NOT NULL
    BEGIN
        SELECT
            @CashInHand = ISNULL(SUM(CASE WHEN ag.GroupName LIKE '%Cash%' THEN ISNULL(v.Debit, 0) - ISNULL(v.Credit, 0) ELSE 0 END), 0),
            @BankBalance = ISNULL(SUM(CASE WHEN ag.GroupName LIKE '%Bank%' THEN ISNULL(v.Debit, 0) - ISNULL(v.Credit, 0) ELSE 0 END), 0)
        FROM Vouchers v
        INNER JOIN LedgerMaster lm ON lm.LedgerID = v.LedgerID
        LEFT JOIN AccountGroupMaster ag ON ag.GroupID = lm.GroupID
        WHERE (@BranchId = 0 OR v.BranchID = @BranchId)
          AND ISNULL(v.CancelFlag, 0) = 0;
    END

    -- Payables (Net of Unallocated Purchase Returns to match Vendor Outstanding Report)
    IF OBJECT_ID('PMaster', 'U') IS NOT NULL
    BEGIN
        DECLARE @RawSupplierPayables DECIMAL(18,2) = 0;
        DECLARE @UnallocatedReturns DECIMAL(18,2) = 0;

        SELECT
            @RawSupplierPayables = ISNULL(SUM(ISNULL(GrandTotal, 0) - ISNULL(PayedAmount, 0)), 0),
            @SupplierPayablesCount = ISNULL(COUNT(1), 0)
        FROM PMaster
        WHERE (@BranchId = 0 OR BranchId = @BranchId)
          AND ISNULL(CancelFlag, 0) = 0
          AND (ISNULL(GrandTotal, 0) - ISNULL(PayedAmount, 0)) > 0;

        IF OBJECT_ID('PReturnMaster', 'U') IS NOT NULL
        BEGIN
            SELECT @UnallocatedReturns = ISNULL(SUM(t.UnallocatedAmt), 0)
            FROM (
                SELECT ROUND(ISNULL(PR.GrandTotal, 0) - ISNULL((SELECT SUM(DNM.DebitAmount) FROM DebitNoteMaster DNM WHERE DNM.PReturnNo = PR.PReturnNo AND ISNULL(DNM.CancelFlag, 0) = 0), 0), 2) AS UnallocatedAmt
                FROM dbo.PReturnMaster PR
                WHERE ISNULL(PR.CancelFlag, 0) = 0
                  AND (@BranchId = 0 OR PR.BranchId = @BranchId)
            ) t
            WHERE t.UnallocatedAmt > 0;
        END

        SET @SupplierPayables = @RawSupplierPayables - @UnallocatedReturns;
    END

    -- Receivables (Net of Advances/Overpayments to match Customer Outstanding Report)
    IF OBJECT_ID('SMaster', 'U') IS NOT NULL
    BEGIN
        SELECT
            @CustomerReceivables = ISNULL(SUM(ISNULL(NetAmount, 0) - ISNULL(ReceivedAmount, 0)), 0),
            @CustomerReceivablesCount = ISNULL(COUNT(1), 0)
        FROM SMaster
        WHERE (@BranchId = 0 OR BranchId = @BranchId)
          AND ISNULL(CancelFlag, 0) = 0
          AND (ISNULL(NetAmount, 0) - ISNULL(ReceivedAmount, 0)) <> 0;
    END

    -- Advances
    IF OBJECT_ID('LedgerMaster', 'U') IS NOT NULL AND OBJECT_ID('AccountGroupMaster', 'U') IS NOT NULL
    BEGIN
        SELECT
            @CustomerAdvanceBalance = ISNULL(SUM(CASE WHEN ag.GroupName LIKE '%Sundry Creditor%' AND (ISNULL(lm.OpnCredit,0) - ISNULL(lm.OpnDebit,0)) < 0 THEN ABS(ISNULL(lm.OpnCredit,0) - ISNULL(lm.OpnDebit,0)) ELSE 0 END), 0),
            @SupplierAdvanceBalance = ISNULL(SUM(CASE WHEN ag.GroupName LIKE '%Sundry Debtor%' AND (ISNULL(lm.OpnDebit,0) - ISNULL(lm.OpnCredit,0)) < 0 THEN ABS(ISNULL(lm.OpnDebit,0) - ISNULL(lm.OpnCredit,0)) ELSE 0 END), 0)
        FROM LedgerMaster lm
        LEFT JOIN AccountGroupMaster ag ON ag.GroupID = lm.GroupID
        WHERE (@BranchId = 0 OR lm.BranchID = @BranchId);
    END

    -- Total Assets & Liabilities for Net Business Asset (NBA)
    DECLARE @TotalAssets DECIMAL(18,2) = @TotalStockCostValue + @CashInHand + @BankBalance + @CustomerReceivables + @SupplierAdvanceBalance;
    DECLARE @TotalLiabilities DECIMAL(18,2) = @SupplierPayables + @CustomerAdvanceBalance;
    DECLARE @NetBusinessAsset DECIMAL(18,2) = @TotalAssets - @TotalLiabilities;

    ---------------------------------------------------------------------------
    -- 6. AGING (>30 Days)
    ---------------------------------------------------------------------------
    DECLARE @DelayedCustomerReceivables30Days DECIMAL(18,2) = 0,
            @DelayedCustomerCount30Days INT = 0,
            @DelayedSupplierPayables30Days DECIMAL(18,2) = 0,
            @DelayedSupplierCount30Days INT = 0;

    IF OBJECT_ID('SMaster', 'U') IS NOT NULL
    BEGIN
        SELECT
            @DelayedCustomerReceivables30Days = ISNULL(SUM(ISNULL(NetAmount, 0) - ISNULL(ReceivedAmount, 0)), 0),
            @DelayedCustomerCount30Days = ISNULL(COUNT(1), 0)
        FROM SMaster
        WHERE BillDate <= @DelayedThresholdDate
          AND (@BranchId = 0 OR BranchId = @BranchId)
          AND ISNULL(CancelFlag, 0) = 0
          AND (ISNULL(NetAmount, 0) - ISNULL(ReceivedAmount, 0)) > 0;
    END

    IF OBJECT_ID('PMaster', 'U') IS NOT NULL
    BEGIN
        SELECT
            @DelayedSupplierPayables30Days = ISNULL(SUM(ISNULL(GrandTotal, 0) - ISNULL(PayedAmount, 0)), 0),
            @DelayedSupplierCount30Days = ISNULL(COUNT(1), 0)
        FROM PMaster
        WHERE PurchaseDate <= @DelayedThresholdDate
          AND (@BranchId = 0 OR BranchId = @BranchId)
          AND ISNULL(CancelFlag, 0) = 0
          AND (ISNULL(GrandTotal, 0) - ISNULL(PayedAmount, 0)) > 0;
    END

    ---------------------------------------------------------------------------
    -- 7. AUDIT COUNTS (Deletions, Price Changes, Stock Adjustments)
    ---------------------------------------------------------------------------
    DECLARE @DeletionCount INT = 0,
            @PriceChangeCount INT = 0,
            @StockAdjustmentCount INT = 0;

    -- 7a. Price Changes from ItemActivityLog (and fallback to tblAuditTrail)
    IF OBJECT_ID('ItemActivityLog', 'U') IS NOT NULL
    BEGIN
        SELECT @PriceChangeCount = ISNULL(COUNT(1), 0)
        FROM dbo.ItemActivityLog
        WHERE CreatedOn >= @RangeFrom AND CreatedOn < @NextDay
          AND (
              ActivityType IN ('UPDATE', 'PRICE CHANGE', 'PRICE_CHANGE')
              OR ActivityDetails LIKE '%Price%'
              OR ActivityDetails LIKE '%Cost%'
              OR ActivityDetails LIKE '%MRP%'
              OR ActivityDetails LIKE '%Mark Up%'
          );
    END
    ELSE IF OBJECT_ID('tblAuditTrail', 'U') IS NOT NULL
    BEGIN
        SELECT @PriceChangeCount = ISNULL(COUNT(1), 0) 
        FROM tblAuditTrail 
        WHERE (ActionType LIKE '%Price%' OR ActionType LIKE '%Edit%') 
          AND ActionDate >= @RangeFrom AND ActionDate < @NextDay;
    END

    -- 7b. Deletions & Cancellations from Master Tables and Activity Logs
    DECLARE @CancelledSales INT = 0,
            @CancelledPurchases INT = 0,
            @CancelledSReturn INT = 0,
            @CancelledPReturn INT = 0,
            @CancelledStockAdj INT = 0,
            @CancelledVouchers INT = 0,
            @ActivityLogDeletions INT = 0;

    IF OBJECT_ID('SMaster', 'U') IS NOT NULL
        SELECT @CancelledSales = COUNT(1) FROM SMaster WHERE CancelFlag = 1 AND BillDate >= @RangeFrom AND BillDate < @NextDay AND (@BranchId = 0 OR BranchId = @BranchId);

    IF OBJECT_ID('PMaster', 'U') IS NOT NULL
        SELECT @CancelledPurchases = COUNT(1) FROM PMaster WHERE CancelFlag = 1 AND PurchaseDate >= @RangeFrom AND PurchaseDate < @NextDay AND (@BranchId = 0 OR BranchId = @BranchId);

    IF OBJECT_ID('SReturnMaster', 'U') IS NOT NULL
        SELECT @CancelledSReturn = COUNT(1) FROM SReturnMaster WHERE CancelFlag = 1 AND SReturnDate >= @RangeFrom AND SReturnDate < @NextDay AND (@BranchId = 0 OR BranchId = @BranchId);

    IF OBJECT_ID('PReturnMaster', 'U') IS NOT NULL
        SELECT @CancelledPReturn = COUNT(1) FROM PReturnMaster WHERE CancelFlag = 1 AND PReturnDate >= @RangeFrom AND PReturnDate < @NextDay AND (@BranchId = 0 OR BranchId = @BranchId);

    IF OBJECT_ID('StockAdjustmentMaster', 'U') IS NOT NULL
        SELECT @CancelledStockAdj = COUNT(1) FROM StockAdjustmentMaster WHERE CancelFlag = 1 AND StockAdjustmentDate >= @RangeFrom AND StockAdjustmentDate < @NextDay AND (@BranchId = 0 OR BranchId = @BranchId);

    IF OBJECT_ID('Vouchers', 'U') IS NOT NULL
        SELECT @CancelledVouchers = COUNT(1) FROM Vouchers WHERE CancelFlag = 1 AND VoucherDate >= @RangeFrom AND VoucherDate < @NextDay AND (@BranchId = 0 OR BranchID = @BranchId);

    IF OBJECT_ID('UserActivityLog', 'U') IS NOT NULL
        SELECT @ActivityLogDeletions = COUNT(1) FROM UserActivityLog WHERE CreatedOn >= @RangeFrom AND CreatedOn < @NextDay AND (ActivityType LIKE '%Delete%' OR ActivityType LIKE '%Cancel%' OR ActivityDetails LIKE '%Delete%' OR ActivityDetails LIKE '%Cancel%');

    SET @DeletionCount = @CancelledSales + @CancelledPurchases + @CancelledSReturn + @CancelledPReturn + @CancelledStockAdj + @CancelledVouchers;
    IF @DeletionCount = 0 OR @ActivityLogDeletions > @DeletionCount
        SET @DeletionCount = @ActivityLogDeletions;

    IF @DeletionCount = 0 AND OBJECT_ID('tblAuditTrail', 'U') IS NOT NULL
        SELECT @DeletionCount = ISNULL(COUNT(1), 0) FROM tblAuditTrail WHERE (ActionType LIKE '%Delete%' OR ActionType LIKE '%Cancel%') AND ActionDate >= @RangeFrom AND ActionDate < @NextDay;

    -- 7c. Stock Adjustments Count
    IF OBJECT_ID('StockAdjustmentMaster', 'U') IS NOT NULL
        SELECT @StockAdjustmentCount = ISNULL(COUNT(1), 0) FROM StockAdjustmentMaster WHERE StockAdjustmentDate >= @RangeFrom AND StockAdjustmentDate < @NextDay AND ISNULL(CancelFlag, 0) = 0;

    ---------------------------------------------------------------------------
    -- RESULT SET 1: ALL 30 KPI SCALARS
    ---------------------------------------------------------------------------
    SELECT
        @TotalStockCostValue AS TotalStockCostValue,
        @TotalStockRetailValue AS TotalStockRetailValue,
        @TotalStockItemCount AS TotalStockItemCount,
        @TotalStockQuantity AS TotalStockQuantity,
        @NegativeStockImpactValue AS NegativeStockImpactValue,
        @NegativeStockItemCount AS NegativeStockItemCount,
        @NegativeStockTotalQty AS NegativeStockTotalQty,
        @LowStockAlertCount AS LowStockAlertCount,
        @ExcessStockAlertCount AS ExcessStockAlertCount,
        @ReorderAlertCount AS ReorderAlertCount,
        @LossStockValue AS LossStockValue,
        @LossStockQty AS LossStockQty,
        @ExtraStockValue AS ExtraStockValue,
        @ExtraStockQty AS ExtraStockQty,
        (@ExtraStockValue - @LossStockValue) AS NetStockAdjustmentValue,
        @TotalSalesRevenue AS TotalSalesRevenue,
        @TotalSalesBillCount AS TotalSalesBillCount,
        @TotalPurchases AS TotalPurchases,
        @GrossProfit AS GrossProfit,
        @DirectExpenses AS DirectExpenses,
        @IndirectExpenses AS IndirectExpenses,
        @TotalExpenses AS TotalBusinessExpenses,
        @ActualNetProfit AS ActualNetProfit,
        @OperatingMarginPercent AS OperatingProfitMarginPercent,
        @OwnerDrawings AS OwnerDrawings,
        @CustomerBadDebts AS CustomerBadDebts,
        @SupplierWriteOffs AS SupplierWriteOffs,
        @CashInHand AS CashInHand,
        @BankBalance AS BankBalance,
        @SupplierPayables AS SupplierPayables,
        @SupplierPayablesCount AS SupplierPayablesCount,
        @CustomerReceivables AS CustomerReceivables,
        @CustomerReceivablesCount AS CustomerReceivablesCount,
        @SupplierAdvanceBalance AS SupplierAdvanceBalance,
        @CustomerAdvanceBalance AS CustomerAdvanceBalance,
        @TotalAssets AS TotalAssets,
        @TotalLiabilities AS TotalLiabilities,
        @NetBusinessAsset AS NetBusinessAsset,
        @DelayedCustomerReceivables30Days AS DelayedCustomerReceivables30Days,
        @DelayedCustomerCount30Days AS DelayedCustomerCount30Days,
        @DelayedSupplierPayables30Days AS DelayedSupplierPayables30Days,
        @DelayedSupplierCount30Days AS DelayedSupplierCount30Days,
        @DeletionCount AS DeletionCount,
        @PriceChangeCount AS PriceChangeCount,
        @StockAdjustmentCount AS StockAdjustmentCount;

    ---------------------------------------------------------------------------
    -- RESULT SET 2: MONTHLY GROWTH MATRIX
    ---------------------------------------------------------------------------
    IF OBJECT_ID('SMaster', 'U') IS NOT NULL
    BEGIN
        SELECT
            DATENAME(MONTH, s.BillDate) + ' ' + CAST(YEAR(s.BillDate) AS VARCHAR(4)) AS MonthName,
            YEAR(s.BillDate) AS [Year],
            MONTH(s.BillDate) AS [Month],
            ISNULL(SUM(ISNULL(s.NetAmount, 0)), 0) AS SalesAmount,
            0.0 AS SalesGrowthPercent,
            ISNULL((SELECT SUM(ISNULL(p.GrandTotal,0)) FROM PMaster p WHERE YEAR(p.PurchaseDate) = YEAR(s.BillDate) AND MONTH(p.PurchaseDate) = MONTH(s.BillDate) AND ISNULL(p.CancelFlag,0)=0 AND (@BranchId = 0 OR p.BranchId = @BranchId)), 0) AS PurchaseAmount,
            0.0 AS ExpenseAmount,
            ISNULL(SUM(ISNULL(s.NetAmount, 0)), 0) * 0.30 AS NetProfitAmount,
            0.0 AS NetProfitGrowthPercent
        FROM SMaster s
        WHERE s.BillDate >= @RangeFrom AND s.BillDate < @NextDay
          AND (@BranchId = 0 OR s.BranchId = @BranchId)
          AND ISNULL(s.CancelFlag, 0) = 0
        GROUP BY YEAR(s.BillDate), MONTH(s.BillDate), DATENAME(MONTH, s.BillDate)
        ORDER BY [Year] ASC, [Month] ASC;
    END
    ELSE
    BEGIN
        SELECT 'Current Month' AS MonthName, YEAR(@ToDate) AS [Year], MONTH(@ToDate) AS [Month],
               0.0 AS SalesAmount, 0.0 AS SalesGrowthPercent, 0.0 AS PurchaseAmount,
               0.0 AS ExpenseAmount, 0.0 AS NetProfitAmount, 0.0 AS NetProfitGrowthPercent;
    END
END", (SqlConnection)DataConnection))
                {
                    cmdAlter.ExecuteNonQuery();
                }
                _schemaEnsured = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnsureExecutiveDashboardKPIsStoredProcedure error: {ex.Message}");
            }
        }
    }
}
