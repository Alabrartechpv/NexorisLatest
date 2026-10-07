using Dapper;
using ModelClass;
using ModelClass.Report;
using Repository.Accounts;
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
        public ExecutiveKpiModel GetExecutiveKpiData(DateTime fromDate, DateTime toDate, int branchId = 0, int companyId = 0, int finYearId = 0, int? groupId = null, int? categoryId = null)
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
                model.GroupId = groupId;
                model.CategoryId = categoryId;
                model.BranchName = !string.IsNullOrWhiteSpace(SessionContext.BranchName) ? SessionContext.BranchName : DataBase.Branch;

                // ═══════════════════════════════════════════════════════════════════
                // EXECUTE STORED PROCEDURE: _POS_ExecutiveDashboardKPIs
                // ═══════════════════════════════════════════════════════════════════
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_ExecutiveDashboardKPIs, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
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

                            model.DeadStockValue = GetDecimal(r, "DeadStockValue");
                            model.DeadStockItemCount = GetInt(r, "DeadStockItemCount");

                            model.LossStockValue = GetDecimal(r, "LossStockValue");
                            model.LossStockQty = GetDecimal(r, "LossStockQty");
                            model.LossStockItemCount = GetInt(r, "LossStockItemCount");
                            model.ExtraStockValue = GetDecimal(r, "ExtraStockValue");
                            model.ExtraStockQty = GetDecimal(r, "ExtraStockQty");
                            model.ExtraStockItemCount = GetInt(r, "ExtraStockItemCount");
                            model.NetStockAdjustmentValue = GetDecimal(r, "NetStockAdjustmentValue");

                            // 2. Sales, Purchases, Returns & Discounts
                            model.TotalSalesRevenue = GetDecimal(r, "TotalSalesRevenue");
                            model.TotalSalesBillCount = GetInt(r, "TotalSalesBillCount");
                            model.TotalSalesReturn = GetDecimal(r, "TotalSalesReturn");
                            model.TotalSalesReturnCount = GetInt(r, "TotalSalesReturnCount");

                            model.TotalPurchases = GetDecimal(r, "TotalPurchases");
                            model.TotalPurchasesBillCount = GetInt(r, "TotalPurchasesBillCount");
                            model.TotalPurchaseReturn = GetDecimal(r, "TotalPurchaseReturn");
                            model.TotalPurchaseReturnCount = GetInt(r, "TotalPurchaseReturnCount");

                            model.HoldBillsValue = GetDecimal(r, "HoldBillsValue");
                            model.HoldBillsCount = GetInt(r, "HoldBillsCount");
                            model.HoldItemsCount = GetDecimal(r, "HoldItemsCount");
                            model.GrossProfit = GetDecimal(r, "GrossProfit");
                            model.GrossProfitMarginPercent = GetDecimal(r, "GrossProfitMarginPercent");
                            model.CostOfGoodsSold = r.Table.Columns.Contains("CostOfGoodsSold") && r["CostOfGoodsSold"] != DBNull.Value
                                ? GetDecimal(r, "CostOfGoodsSold")
                                : Math.Max(0, model.TotalSalesRevenue - model.GrossProfit);

                            // 3. Tax / GST Liabilities
                            model.OutputGstAmount = GetDecimal(r, "OutputGstAmount");
                            model.InputGstAmount = GetDecimal(r, "InputGstAmount");
                            model.NetTaxLiability = GetDecimal(r, "NetTaxLiability");

                            // 4. Expenses & Profitability
                            model.DirectExpenses = GetDecimal(r, "DirectExpenses");
                            model.IndirectExpenses = GetDecimal(r, "IndirectExpenses");
                            model.TotalBusinessExpenses = GetDecimal(r, "TotalBusinessExpenses");
                            model.ActualNetProfit = GetDecimal(r, "ActualNetProfit");
                            model.OperatingProfitMarginPercent = GetDecimal(r, "OperatingProfitMarginPercent");
                            model.OwnerDrawings = GetDecimal(r, "OwnerDrawings");
                            model.CustomerBadDebts = GetDecimal(r, "CustomerBadDebts");
                            model.SupplierWriteOffs = GetDecimal(r, "SupplierWriteOffs");

                            // 5. Working Capital & Balances
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

                            // 6. Aging
                            model.DelayedCustomerReceivables30Days = GetDecimal(r, "DelayedCustomerReceivables30Days");
                            model.DelayedCustomerCount30Days = GetInt(r, "DelayedCustomerCount30Days");
                            model.DelayedSupplierPayables30Days = GetDecimal(r, "DelayedSupplierPayables30Days");
                            model.DelayedSupplierCount30Days = GetInt(r, "DelayedSupplierCount30Days");

                            // 7. Audit
                            model.DeletionCount = GetInt(r, "DeletionCount");
                            model.PriceChangeCount = GetInt(r, "PriceChangeCount");
                            model.StockAdjustmentCount = GetInt(r, "StockAdjustmentCount");

                            // 8. Manual Party Balances
                            model.TotalManualBalance = GetDecimal(r, "TotalManualBalance");
                            model.ManualCustomerBalance = GetDecimal(r, "ManualCustomerBalance");
                            model.ManualVendorBalance = GetDecimal(r, "ManualVendorBalance");
                            model.ManualBalanceCount = GetInt(r, "ManualBalanceCount");
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

                // ═══════════════════════════════════════════════════════════════════
                // GROUP / CATEGORY STOCK RECONCILIATION VIA STORED PROCEDURE
                // Execute _POS_StockReportAdvanced (_Test16) stored procedure directly
                // ═══════════════════════════════════════════════════════════════════
                if ((groupId.HasValue && groupId.Value > 0) || (categoryId.HasValue && categoryId.Value > 0))
                {
                    try
                    {
                        if (DataConnection.State != ConnectionState.Open)
                            DataConnection.Open();

                        using (SqlCommand cmdStock = new SqlCommand(STOREDPROCEDURE._POS_StockReportAdvanced, (SqlConnection)DataConnection))
                        {
                            cmdStock.CommandType = CommandType.StoredProcedure;
                            cmdStock.CommandTimeout = 180;
                            cmdStock.Parameters.AddWithValue("@FromDate", rangeFrom);
                            cmdStock.Parameters.AddWithValue("@ToDate", rangeTo.AddDays(1).AddSeconds(-1));
                            cmdStock.Parameters.AddWithValue("@CompanyId", effectiveCompany);
                            cmdStock.Parameters.AddWithValue("@BranchId", effectiveBranch);
                            cmdStock.Parameters.AddWithValue("@FinYearId", effectiveFinYear);
                            cmdStock.Parameters.AddWithValue("@BarcodeContains", DBNull.Value);
                            cmdStock.Parameters.AddWithValue("@GroupId", (groupId.HasValue && groupId.Value > 0) ? (object)groupId.Value : DBNull.Value);
                            cmdStock.Parameters.AddWithValue("@CategoryId", (categoryId.HasValue && categoryId.Value > 0) ? (object)categoryId.Value : DBNull.Value);
                            cmdStock.Parameters.AddWithValue("@SubCategoryId", DBNull.Value);
                            cmdStock.Parameters.AddWithValue("@LedgerId", DBNull.Value);

                            using (SqlDataAdapter adaptStock = new SqlDataAdapter(cmdStock))
                            {
                                DataTable dtStock = new DataTable();
                                adaptStock.Fill(dtStock);

                                if (dtStock != null && dtStock.Rows.Count > 0)
                                {
                                    decimal totalCostAll = 0;
                                    decimal totalRetailValueAll = 0;
                                    decimal totalQtyAll = 0;
                                    int itemCountAll = 0;

                                    decimal negImpactValue = 0;
                                    decimal negTotalQty = 0;
                                    int negItemCount = 0;

                                    decimal filteredSales = 0;
                                    decimal filteredProfit = 0;
                                    decimal filteredPurchases = 0;
                                    decimal filteredSalesReturn = 0;
                                    decimal filteredPurchaseReturn = 0;
                                    decimal filteredLossStockVal = 0;
                                    decimal filteredLossStockQty = 0;
                                    int filteredLossItemCount = 0;
                                    decimal filteredExtraStockVal = 0;
                                    decimal filteredExtraStockQty = 0;
                                    int filteredExtraItemCount = 0;
                                    decimal filteredHoldVal = 0;
                                    decimal filteredHoldQty = 0;
                                    int filteredDeadItemCount = 0;
                                    decimal filteredDeadStockVal = 0;

                                    foreach (DataRow sr in dtStock.Rows)
                                    {
                                        decimal closingStock = sr["ClosingStock"] != DBNull.Value ? Convert.ToDecimal(sr["ClosingStock"]) : 0;
                                        decimal cost = sr["Cost"] != DBNull.Value ? Convert.ToDecimal(sr["Cost"]) : 0;
                                        decimal retail = sr["RetailPrice"] != DBNull.Value ? Convert.ToDecimal(sr["RetailPrice"]) : 0;
                                        decimal saleAmt = sr["SaleAmount"] != DBNull.Value ? Convert.ToDecimal(sr["SaleAmount"]) : 0;
                                        decimal profit = sr["Profit"] != DBNull.Value ? Convert.ToDecimal(sr["Profit"]) : 0;
                                        decimal purchQty = sr["Purchase"] != DBNull.Value ? Convert.ToDecimal(sr["Purchase"]) : 0;
                                        decimal sRetQty = sr["SalesReturn"] != DBNull.Value ? Convert.ToDecimal(sr["SalesReturn"]) : 0;
                                        decimal pRetQty = sr["PurchaseReturn"] != DBNull.Value ? Convert.ToDecimal(sr["PurchaseReturn"]) : 0;
                                        decimal adjOut = sr["StockAdjustmentOut"] != DBNull.Value ? Convert.ToDecimal(sr["StockAdjustmentOut"]) : 0;
                                        decimal adjIn = sr["StockAdjustmentIn"] != DBNull.Value ? Convert.ToDecimal(sr["StockAdjustmentIn"]) : 0;
                                        decimal hold = sr.Table.Columns.Contains("HoldQty") && sr["HoldQty"] != DBNull.Value ? Convert.ToDecimal(sr["HoldQty"]) : 0;
                                        decimal salesQty = sr["Sales"] != DBNull.Value ? Convert.ToDecimal(sr["Sales"]) : 0;

                                        totalCostAll += Math.Round(closingStock * cost, 2);
                                        totalRetailValueAll += Math.Round(closingStock * retail, 2);
                                        totalQtyAll += closingStock;
                                        itemCountAll++;

                                        if (closingStock < 0)
                                        {
                                            negImpactValue += Math.Round(Math.Abs(closingStock) * cost, 2);
                                            negTotalQty += closingStock;
                                            negItemCount++;
                                        }

                                        filteredSales += saleAmt;
                                        filteredProfit += profit;
                                        filteredPurchases += Math.Round(purchQty * cost, 2);
                                        filteredSalesReturn += Math.Round(sRetQty * retail, 2);
                                        filteredPurchaseReturn += Math.Round(pRetQty * cost, 2);

                                        if (adjOut > 0)
                                        {
                                            filteredLossStockVal += Math.Round(adjOut * cost, 2);
                                            filteredLossStockQty += adjOut;
                                            filteredLossItemCount++;
                                        }
                                        if (adjIn > 0)
                                        {
                                            filteredExtraStockVal += Math.Round(adjIn * cost, 2);
                                            filteredExtraStockQty += adjIn;
                                            filteredExtraItemCount++;
                                        }
                                        if (hold > 0)
                                        {
                                            filteredHoldVal += Math.Round(hold * retail, 2);
                                            filteredHoldQty += hold;
                                        }
                                        if (salesQty == 0 && closingStock > 0)
                                        {
                                            filteredDeadStockVal += Math.Round(closingStock * cost, 2);
                                            filteredDeadItemCount++;
                                        }
                                    }

                                    model.TotalStockCostValue = totalCostAll;
                                    model.TotalStockRetailValue = totalRetailValueAll;
                                    model.TotalStockItemCount = itemCountAll;
                                    model.TotalStockQuantity = totalQtyAll;
                                    model.StockProfitPotential = Math.Max(0, totalRetailValueAll - totalCostAll);

                                    model.NegativeStockImpactValue = negImpactValue;
                                    model.NegativeStockItemCount = negItemCount;
                                    model.NegativeStockTotalQty = negTotalQty;

                                    model.TotalSalesRevenue = filteredSales;
                                    model.GrossProfit = filteredProfit;
                                    model.CostOfGoodsSold = Math.Max(0, filteredSales - filteredProfit);
                                    model.GrossProfitMarginPercent = filteredSales > 0 ? Math.Round((filteredProfit / filteredSales) * 100m, 1) : 0;
                                    model.TotalPurchases = filteredPurchases;
                                    model.TotalSalesReturn = filteredSalesReturn;
                                    model.TotalPurchaseReturn = filteredPurchaseReturn;
                                    model.LossStockValue = filteredLossStockVal;
                                    model.LossStockQty = filteredLossStockQty;
                                    model.LossStockItemCount = filteredLossItemCount;
                                    model.ExtraStockValue = filteredExtraStockVal;
                                    model.ExtraStockQty = filteredExtraStockQty;
                                    model.ExtraStockItemCount = filteredExtraItemCount;
                                    model.NetStockAdjustmentValue = filteredExtraStockVal - filteredLossStockVal;
                                    model.HoldBillsValue = filteredHoldVal;
                                    model.HoldItemsCount = filteredHoldQty;
                                    model.DeadStockValue = filteredDeadStockVal;
                                    model.DeadStockItemCount = filteredDeadItemCount;

                                    model.ActualNetProfit = filteredProfit;
                                    model.OperatingProfitMarginPercent = filteredSales > 0 ? Math.Round((filteredProfit / filteredSales) * 100m, 2) : 0;
                                }
                            }
                        }
                    }
                    catch (Exception exStock)
                    {
                        System.Diagnostics.Debug.WriteLine($"Stock filter SP execution info: {exStock.Message}");
                    }
                }

                // ═══════════════════════════════════════════════════════════════════
                // MANUAL PARTY BALANCE INTEGRATION VIA STORED PROCEDURE
                // If not populated by dashboard SP, query via ManualPartyBalanceRepository
                // ═══════════════════════════════════════════════════════════════════
                if (model.TotalManualBalance == 0 && model.ManualBalanceCount == 0)
                {
                    try
                    {
                        var manualRepo = new ManualPartyBalanceRepository();
                        var entries = manualRepo.GetEntries(openOnly: true, toDate: rangeTo);
                        if (entries != null && entries.Count > 0)
                        {
                            decimal totalManual = 0;
                            decimal manualCust = 0;
                            decimal manualVend = 0;
                            int manualCount = 0;

                            foreach (var item in entries)
                            {
                                decimal rem = item.RemainingAmount;
                                totalManual += rem;
                                manualCount++;

                                string pType = item.PartyType ?? "";
                                string bType = item.BalanceType ?? "";
                                bool isCustomer = pType.IndexOf("Customer", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                  pType.IndexOf("Debtor", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                  bType.IndexOf("Receivable", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                  bType.IndexOf("Debit", StringComparison.OrdinalIgnoreCase) >= 0;
                                bool isVendor = pType.IndexOf("Vendor", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                pType.IndexOf("Supplier", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                pType.IndexOf("Creditor", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                bType.IndexOf("Payable", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                bType.IndexOf("Credit", StringComparison.OrdinalIgnoreCase) >= 0;

                                if (isCustomer && !isVendor)
                                {
                                    manualCust += rem;
                                }
                                else if (isVendor)
                                {
                                    manualVend += rem;
                                }
                                else
                                {
                                    manualCust += rem;
                                }
                            }

                            model.TotalManualBalance = totalManual;
                            model.ManualCustomerBalance = manualCust;
                            model.ManualVendorBalance = manualVend;
                            model.ManualBalanceCount = manualCount;
                        }
                    }
                    catch (Exception exManual)
                    {
                        System.Diagnostics.Debug.WriteLine($"Manual balance SP integration info: {exManual.Message}");
                    }
                }

                // ═══════════════════════════════════════════════════════════════════
                // 9. EXPENSES & NET PROFIT RECONCILIATION
                // Guarantee exact parity with Business Expenses Report (Net = Debit - Credit)
                // ═══════════════════════════════════════════════════════════════════
                try
                {
                    var expenseSummaries = GetExpenseLedgerSummary(rangeFrom, rangeTo, effectiveBranch, effectiveCompany);
                    if (expenseSummaries != null)
                    {
                        decimal direct = expenseSummaries
                            .Where(x => string.Equals(x.ExpenseType, "Direct Expense", StringComparison.OrdinalIgnoreCase) ||
                                        (x.ExpenseType != null && x.ExpenseType.IndexOf("Direct", StringComparison.OrdinalIgnoreCase) >= 0 && x.ExpenseType.IndexOf("Indirect", StringComparison.OrdinalIgnoreCase) < 0))
                            .Sum(x => x.NetAmount);

                        decimal indirect = expenseSummaries
                            .Where(x => string.Equals(x.ExpenseType, "Indirect Expense", StringComparison.OrdinalIgnoreCase) ||
                                        (x.ExpenseType != null && x.ExpenseType.IndexOf("Indirect", StringComparison.OrdinalIgnoreCase) >= 0))
                            .Sum(x => x.NetAmount);

                        model.DirectExpenses = direct;
                        model.IndirectExpenses = indirect;
                        model.TotalBusinessExpenses = direct + indirect;

                        // Recompute Actual Net Profit & Operating Margin using net expenses
                        model.ActualNetProfit = model.GrossProfit - model.TotalBusinessExpenses;
                        model.OperatingProfitMarginPercent = model.TotalSalesRevenue > 0
                            ? Math.Round((model.ActualNetProfit / model.TotalSalesRevenue) * 100m, 2)
                            : 0;
                    }
                }
                catch (Exception exExp)
                {
                    System.Diagnostics.Debug.WriteLine($"Expense reconciliation info: {exExp.Message}");
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

        public List<ExpenseLedgerSummaryItem> GetExpenseLedgerSummary(DateTime fromDate, DateTime toDate, int branchId, int companyId = 0)
        {
            var list = new List<ExpenseLedgerSummaryItem>();
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                string sql = @"
WITH CleanGroup AS (
    SELECT GroupID, MIN(GroupName) AS GroupName
    FROM AccountGroupMaster
    GROUP BY GroupID
)
SELECT 
    lm.LedgerID,
    lm.LedgerName,
    ag.GroupName,
    CASE 
        WHEN ag.GroupName LIKE '%Indirect%' THEN 'Indirect Expense'
        WHEN ag.GroupName LIKE '%Direct%' THEN 'Direct Expense'
        ELSE 'Indirect Expense'
    END AS ExpenseType,
    ISNULL(SUM(v.Debit), 0) AS TotalDebit,
    ISNULL(SUM(v.Credit), 0) AS TotalCredit,
    ISNULL(SUM(v.Debit), 0) - ISNULL(SUM(v.Credit), 0) AS NetAmount,
    COUNT(DISTINCT v.VoucherID) AS VoucherCount
FROM Vouchers v
INNER JOIN LedgerMaster lm ON lm.LedgerID = v.LedgerID
INNER JOIN CleanGroup ag ON ag.GroupID = lm.GroupID
WHERE (ag.GroupID IN (10, 12) OR ag.GroupName LIKE '%Expense%')
  AND v.VoucherDate >= @FromDate AND v.VoucherDate <= @ToDate
  AND (@BranchId = 0 OR v.BranchID = @BranchId)
  AND (@CompanyId = 0 OR v.CompanyID = @CompanyId)
  AND ISNULL(v.CancelFlag, 0) = 0
GROUP BY lm.LedgerID, lm.LedgerName, ag.GroupID, ag.GroupName
HAVING (ISNULL(SUM(v.Debit), 0) - ISNULL(SUM(v.Credit), 0)) <> 0
ORDER BY ExpenseType, NetAmount DESC, lm.LedgerName;";

                using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date.AddDays(1).AddSeconds(-1));
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        int slNo = 1;
                        while (reader.Read())
                        {
                            list.Add(new ExpenseLedgerSummaryItem
                            {
                                SlNo = slNo++,
                                LedgerID = reader["LedgerID"] != DBNull.Value ? Convert.ToInt32(reader["LedgerID"]) : 0,
                                LedgerName = reader["LedgerName"]?.ToString() ?? "",
                                GroupName = reader["GroupName"]?.ToString() ?? "",
                                ExpenseType = reader["ExpenseType"]?.ToString() ?? "Indirect Expense",
                                TotalDebit = reader["TotalDebit"] != DBNull.Value ? Convert.ToDecimal(reader["TotalDebit"]) : 0,
                                TotalCredit = reader["TotalCredit"] != DBNull.Value ? Convert.ToDecimal(reader["TotalCredit"]) : 0,
                                NetAmount = reader["NetAmount"] != DBNull.Value ? Convert.ToDecimal(reader["NetAmount"]) : 0,
                                VoucherCount = reader["VoucherCount"] != DBNull.Value ? Convert.ToInt32(reader["VoucherCount"]) : 0
                            });
                        }
                    }
                }

                decimal total = list.Sum(x => x.NetAmount);
                if (total > 0)
                {
                    foreach (var item in list)
                    {
                        item.PercentageOfTotal = Math.Round((item.NetAmount / total) * 100, 1);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetExpenseLedgerSummary error: " + ex.Message);
            }
            finally
            {
                if (DataConnection != null && DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return list;
        }

        public List<ExpenseVoucherTransactionItem> GetExpenseVoucherTransactions(DateTime fromDate, DateTime toDate, int branchId, int companyId = 0)
        {
            var list = new List<ExpenseVoucherTransactionItem>();
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                string sql = @"
WITH CleanGroup AS (
    SELECT GroupID, MIN(GroupName) AS GroupName
    FROM AccountGroupMaster
    GROUP BY GroupID
)
SELECT 
    v.VoucherID,
    v.VoucherDate,
    v.VoucherNumber,
    v.VoucherType,
    lm.LedgerName,
    ag.GroupName,
    CASE 
        WHEN ag.GroupName LIKE '%Indirect%' THEN 'Indirect Expense'
        WHEN ag.GroupName LIKE '%Direct%' THEN 'Direct Expense'
        ELSE 'Indirect Expense'
    END AS ExpenseType,
    ISNULL(v.Debit, 0) AS Debit,
    ISNULL(v.Credit, 0) AS Credit,
    ISNULL(v.Debit, 0) - ISNULL(v.Credit, 0) AS NetAmount,
    ISNULL(v.Narration, '') AS Narration
FROM Vouchers v
INNER JOIN LedgerMaster lm ON lm.LedgerID = v.LedgerID
INNER JOIN CleanGroup ag ON ag.GroupID = lm.GroupID
WHERE (ag.GroupID IN (10, 12) OR ag.GroupName LIKE '%Expense%')
  AND v.VoucherDate >= @FromDate AND v.VoucherDate <= @ToDate
  AND (@BranchId = 0 OR v.BranchID = @BranchId)
  AND (@CompanyId = 0 OR v.CompanyID = @CompanyId)
  AND ISNULL(v.CancelFlag, 0) = 0
ORDER BY v.VoucherDate DESC, v.VoucherID DESC;";

                using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date.AddDays(1).AddSeconds(-1));
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new ExpenseVoucherTransactionItem
                            {
                                VoucherID = reader["VoucherID"] != DBNull.Value ? Convert.ToInt64(reader["VoucherID"]) : 0,
                                VoucherDate = reader["VoucherDate"] != DBNull.Value ? Convert.ToDateTime(reader["VoucherDate"]) : DateTime.MinValue,
                                VoucherNumber = reader["VoucherNumber"]?.ToString() ?? "",
                                VoucherType = reader["VoucherType"]?.ToString() ?? "",
                                LedgerName = reader["LedgerName"]?.ToString() ?? "",
                                GroupName = reader["GroupName"]?.ToString() ?? "",
                                ExpenseType = reader["ExpenseType"]?.ToString() ?? "Indirect Expense",
                                Debit = reader["Debit"] != DBNull.Value ? Convert.ToDecimal(reader["Debit"]) : 0,
                                Credit = reader["Credit"] != DBNull.Value ? Convert.ToDecimal(reader["Credit"]) : 0,
                                NetAmount = reader["NetAmount"] != DBNull.Value ? Convert.ToDecimal(reader["NetAmount"]) : 0,
                                Narration = reader["Narration"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetExpenseVoucherTransactions error: " + ex.Message);
            }
            finally
            {
                if (DataConnection != null && DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return list;
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
    }
}
