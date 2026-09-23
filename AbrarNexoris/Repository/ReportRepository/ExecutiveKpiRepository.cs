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
                // RECONCILE: Override stock metrics using the same SP as the
                // Stock Valuation Report (_Test16 / _POS_StockReportAdvanced)
                // so that the dashboard and the report always match exactly.
                // ═══════════════════════════════════════════════════════════════════
                try
                {
                    if (DataConnection.State != ConnectionState.Open)
                        DataConnection.Open();

                    using (SqlCommand cmdStock = new SqlCommand(STOREDPROCEDURE._POS_StockReportAdvanced, (SqlConnection)DataConnection))
                    {
                        cmdStock.CommandType = CommandType.StoredProcedure;
                        cmdStock.CommandTimeout = 180;
                        cmdStock.Parameters.AddWithValue("@FromDate", new DateTime(1753, 1, 1));
                        cmdStock.Parameters.AddWithValue("@ToDate", DateTime.Today.AddDays(1).AddTicks(-1));
                        cmdStock.Parameters.AddWithValue("@CompanyId", effectiveCompany);
                        cmdStock.Parameters.AddWithValue("@BranchId", effectiveBranch);
                        cmdStock.Parameters.AddWithValue("@FinYearId", effectiveFinYear);
                        cmdStock.Parameters.AddWithValue("@BarcodeContains", DBNull.Value);
                        cmdStock.Parameters.AddWithValue("@GroupId", DBNull.Value);
                        cmdStock.Parameters.AddWithValue("@CategoryId", DBNull.Value);
                        cmdStock.Parameters.AddWithValue("@SubCategoryId", DBNull.Value);
                        cmdStock.Parameters.AddWithValue("@LedgerId", DBNull.Value);

                        using (SqlDataAdapter adaptStock = new SqlDataAdapter(cmdStock))
                        {
                            DataTable dtStock = new DataTable();
                            adaptStock.Fill(dtStock);

                            if (dtStock != null && dtStock.Rows.Count > 0)
                            {
                                decimal totalCostValueAll = 0;
                                decimal totalRetailValueAll = 0;
                                decimal totalQtyAll = 0;
                                int itemCountAll = 0;

                                decimal negImpactValue = 0;
                                decimal negTotalQty = 0;
                                int negItemCount = 0;

                                foreach (DataRow sr in dtStock.Rows)
                                {
                                    decimal closingStock = sr["ClosingStock"] != DBNull.Value ? Convert.ToDecimal(sr["ClosingStock"]) : 0;
                                    decimal cost = sr["Cost"] != DBNull.Value ? Convert.ToDecimal(sr["Cost"]) : 0;
                                    decimal retail = sr["RetailPrice"] != DBNull.Value ? Convert.ToDecimal(sr["RetailPrice"]) : 0;

                                    totalCostValueAll += closingStock * cost;
                                    totalRetailValueAll += closingStock * retail;
                                    totalQtyAll += closingStock;
                                    itemCountAll++;

                                    if (closingStock < 0)
                                    {
                                        negImpactValue += Math.Abs(closingStock) * cost;
                                        negTotalQty += closingStock; // negative
                                        negItemCount++;
                                    }
                                }

                                // Override with accurate net values matching the Stock Report
                                model.TotalStockCostValue = totalCostValueAll;
                                model.TotalStockRetailValue = totalRetailValueAll;
                                model.StockProfitPotential = totalRetailValueAll - totalCostValueAll;
                                model.TotalStockItemCount = itemCountAll;
                                model.TotalStockQuantity = totalQtyAll;

                                model.NegativeStockImpactValue = negImpactValue;
                                model.NegativeStockItemCount = negItemCount;
                                model.NegativeStockTotalQty = negTotalQty;
                            }
                        }
                    }
                }
                catch (Exception exStock)
                {
                    // If the reconciliation query fails, keep the original dashboard values
                    System.Diagnostics.Debug.WriteLine($"Stock reconciliation fallback: {exStock.Message}");
                }

                // ═══════════════════════════════════════════════════════════════════
                // MANUAL PARTY BALANCE INTEGRATION
                // Fetch active manual customer and vendor balances
                // ═══════════════════════════════════════════════════════════════════
                try
                {
                    if (DataConnection.State != ConnectionState.Open)
                        DataConnection.Open();

                    const string sqlManual = @"
IF OBJECT_ID('dbo.ManualPartyBalance', 'U') IS NOT NULL
BEGIN
    SELECT 
        b.Id,
        b.PartyType,
        b.BalanceType,
        b.Amount,
        ISNULL(s.SettledAmount, 0) AS SettledAmount,
        (b.Amount - ISNULL(s.SettledAmount, 0)) AS RemainingAmount
    FROM dbo.ManualPartyBalance b
    OUTER APPLY (
        SELECT SUM(SettlementAmount) AS SettledAmount
        FROM dbo.ManualPartyBalanceSettlement
        WHERE ManualPartyBalanceId = b.Id AND IsDeleted = 0
    ) s
    WHERE b.IsDeleted = 0
      AND b.Status <> 'Settled'
      AND (@CompanyId = 0 OR b.CompanyId = @CompanyId)
      AND (@BranchId = 0 OR b.BranchId = @BranchId)
      AND (b.EntryDate <= @ToDate);
END";

                    using (SqlCommand cmdManual = new SqlCommand(sqlManual, (SqlConnection)DataConnection))
                    {
                        cmdManual.CommandType = CommandType.Text;
                        cmdManual.CommandTimeout = 60;
                        cmdManual.Parameters.AddWithValue("@CompanyId", effectiveCompany);
                        cmdManual.Parameters.AddWithValue("@BranchId", effectiveBranch);
                        cmdManual.Parameters.AddWithValue("@ToDate", rangeTo.AddDays(1).AddTicks(-1));

                        using (SqlDataAdapter adaptManual = new SqlDataAdapter(cmdManual))
                        {
                            DataTable dtManual = new DataTable();
                            adaptManual.Fill(dtManual);

                            if (dtManual != null && dtManual.Rows.Count > 0)
                            {
                                decimal totalManual = 0;
                                decimal manualCust = 0;
                                decimal manualVend = 0;
                                int manualCount = 0;

                                foreach (DataRow mr in dtManual.Rows)
                                {
                                    decimal rem = mr["RemainingAmount"] != DBNull.Value ? Convert.ToDecimal(mr["RemainingAmount"]) : 0;
                                    if (rem <= 0) continue;

                                    string pType = mr["PartyType"]?.ToString() ?? "";
                                    string bType = mr["BalanceType"]?.ToString() ?? "";

                                    totalManual += rem;
                                    manualCount++;

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
                    }
                }
                catch (Exception exManual)
                {
                    System.Diagnostics.Debug.WriteLine($"Manual balance calculation fallback: {exManual.Message}");
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

