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
                            model.CostOfGoodsSold = r.Table.Columns.Contains("CostOfGoodsSold") && r["CostOfGoodsSold"] != DBNull.Value
                                ? GetDecimal(r, "CostOfGoodsSold")
                                : Math.Max(0, model.TotalSalesRevenue - model.GrossProfit);
                            model.GrossProfit = GetDecimal(r, "GrossProfit");
                            model.GrossProfitMarginPercent = GetDecimal(r, "GrossProfitMarginPercent");

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
                                if (rem <= 0) continue;

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
    }
}
