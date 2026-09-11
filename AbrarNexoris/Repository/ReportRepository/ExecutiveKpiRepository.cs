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
                model.GeneratedAt = DateTime.Now;

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
    }
}
