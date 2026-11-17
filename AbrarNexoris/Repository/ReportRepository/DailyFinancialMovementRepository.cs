using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ModelClass;
using ModelClass.Report;

namespace Repository.ReportRepository
{
    public class DailyFinancialMovementRepository : BaseRepostitory
    {
        public DailyFinancialMovementReport GetReport(DailyFinancialMovementFilter filter)
        {
            DailyFinancialMovementReport report = new DailyFinancialMovementReport
            {
                FromDate = filter.FromDate,
                ToDate = filter.ToDate
            };

            int companyId = filter.CompanyId > 0 ? filter.CompanyId : GetContextValue(SessionContext.CompanyId, DataBase.CompanyId);
            int branchId = filter.BranchId > 0 ? filter.BranchId : GetContextValue(SessionContext.BranchId, DataBase.BranchId);
            int finYearId = filter.FinYearId > 0 ? filter.FinYearId : GetContextValue(SessionContext.FinYearId, DataBase.FinyearId);

            if (branchId <= 0) branchId = 1;

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand("dbo.POS_DailyFinancialMovementReport", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                    cmd.Parameters.AddWithValue("@FromDate", filter.FromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", filter.ToDate.Date);
                    cmd.Parameters.AddWithValue("@DrillDownCategory", string.IsNullOrEmpty(filter.DrillDownCategory) ? "ALL" : filter.DrillDownCategory);
                    cmd.Parameters.AddWithValue("@DrillDownDate", filter.DrillDownDate.HasValue ? (object)filter.DrillDownDate.Value.Date : DBNull.Value);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        da.Fill(ds);

                        // Result Set 1: Day Rows
                        if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow r in ds.Tables[0].Rows)
                            {
                                report.DayRows.Add(new DailyFinancialMovementDayRow
                                {
                                    TxDate = Convert.ToDateTime(r["TxDate"]),
                                    DayName = r["DayName"].ToString(),
                                    CashOpening = ToDec(r["CashOpening"]),
                                    CashInflow = ToDec(r["CashInflow"]),
                                    CashOutflow = ToDec(r["CashOutflow"]),
                                    CashClosing = ToDec(r["CashClosing"]),
                                    BankOpening = ToDec(r["BankOpening"]),
                                    BankInflow = ToDec(r["BankInflow"]),
                                    BankOutflow = ToDec(r["BankOutflow"]),
                                    BankClosing = ToDec(r["BankClosing"]),
                                    DebtorsOpening = ToDec(r["DebtorsOpening"]),
                                    DebtorsCreditSales = ToDec(r["DebtorsCreditSales"]),
                                    DebtorsCollected = ToDec(r["DebtorsCollected"]),
                                    DebtorsClosing = ToDec(r["DebtorsClosing"]),
                                    StockOpening = ToDec(r["StockOpening"]),
                                    StockInflow = ToDec(r["StockInflow"]),
                                    StockOutflow = ToDec(r["StockOutflow"]),
                                    StockClosing = ToDec(r["StockClosing"]),
                                    AssetOpening = r.Table.Columns.Contains("AssetOpening") ? ToDec(r["AssetOpening"]) : 0,
                                    AssetAdditions = r.Table.Columns.Contains("AssetAdditions") ? ToDec(r["AssetAdditions"]) : 0,
                                    AssetReductions = r.Table.Columns.Contains("AssetReductions") ? ToDec(r["AssetReductions"]) : 0,
                                    TotalAssets = ToDec(r["TotalAssets"]),
                                    DailyAssetChange = r.Table.Columns.Contains("DailyAssetChange") ? ToDec(r["DailyAssetChange"]) : 0,
                                    DailyAssetChangePct = r.Table.Columns.Contains("DailyAssetChangePct") ? ToDec(r["DailyAssetChangePct"]) : 0,
                                    CreditorsOpening = ToDec(r["CreditorsOpening"]),
                                    CreditorsPurchases = ToDec(r["CreditorsPurchases"]),
                                    CreditorsPaid = ToDec(r["CreditorsPaid"]),
                                    CreditorsClosing = ToDec(r["CreditorsClosing"]),
                                    LoansOpening = ToDec(r["LoansOpening"]),
                                    LoansClosing = ToDec(r["LoansClosing"]),
                                    LiabOpening = r.Table.Columns.Contains("LiabOpening") ? ToDec(r["LiabOpening"]) : 0,
                                    LiabAdditions = r.Table.Columns.Contains("LiabAdditions") ? ToDec(r["LiabAdditions"]) : 0,
                                    LiabReductions = r.Table.Columns.Contains("LiabReductions") ? ToDec(r["LiabReductions"]) : 0,
                                    TotalLiabilities = ToDec(r["TotalLiabilities"]),
                                    DailyLiabChange = r.Table.Columns.Contains("DailyLiabChange") ? ToDec(r["DailyLiabChange"]) : 0,
                                    NetStorePosition = ToDec(r["NetStorePosition"]),
                                    DailyNetChange = ToDec(r["DailyNetChange"])
                                });
                            }

                            // Safeguard: Cascade and compute any missing 12:00 AM daily progression metrics
                            for (int i = 0; i < report.DayRows.Count; i++)
                            {
                                var day = report.DayRows[i];

                                // 1. Asset Inflow & Outflow
                                if (day.AssetAdditions == 0m)
                                {
                                    day.AssetAdditions = day.CashInflow + day.BankInflow + day.DebtorsCreditSales + day.StockInflow;
                                }
                                if (day.AssetReductions == 0m)
                                {
                                    day.AssetReductions = day.CashOutflow + day.BankOutflow + day.DebtorsCollected + day.StockOutflow;
                                }

                                // 2. Asset Opening
                                if (day.AssetOpening == 0m)
                                {
                                    if (i == 0)
                                    {
                                        day.AssetOpening = (day.CashOpening + day.BankOpening + day.DebtorsOpening + day.StockOpening);
                                        if (day.AssetOpening == 0m && day.TotalAssets != 0m)
                                        {
                                            day.AssetOpening = day.TotalAssets - day.AssetAdditions + day.AssetReductions;
                                        }
                                    }
                                    else
                                    {
                                        day.AssetOpening = report.DayRows[i - 1].TotalAssets;
                                    }
                                }

                                // 3. Total Assets Closing
                                if (day.TotalAssets == 0m)
                                {
                                    day.TotalAssets = day.AssetOpening + day.AssetAdditions - day.AssetReductions;
                                }

                                // 4. Daily Asset Change & %
                                if (day.DailyAssetChange == 0m && (day.TotalAssets != day.AssetOpening))
                                {
                                    day.DailyAssetChange = day.TotalAssets - day.AssetOpening;
                                }
                                if (day.DailyAssetChangePct == 0m && day.AssetOpening != 0m)
                                {
                                    day.DailyAssetChangePct = Math.Round((day.DailyAssetChange / day.AssetOpening) * 100m, 2);
                                }

                                // 5. Liability Opening, Additions, Reductions, and Change
                                if (day.LiabAdditions == 0m) day.LiabAdditions = day.CreditorsPurchases;
                                if (day.LiabReductions == 0m) day.LiabReductions = day.CreditorsPaid;

                                if (day.LiabOpening == 0m)
                                {
                                    if (i == 0)
                                    {
                                        day.LiabOpening = day.CreditorsOpening + day.LoansOpening;
                                        if (day.LiabOpening == 0m && day.TotalLiabilities != 0m)
                                        {
                                            day.LiabOpening = day.TotalLiabilities - day.LiabAdditions + day.LiabReductions;
                                        }
                                    }
                                    else
                                    {
                                        day.LiabOpening = report.DayRows[i - 1].TotalLiabilities;
                                    }
                                }

                                if (day.DailyLiabChange == 0m && (day.TotalLiabilities != day.LiabOpening))
                                {
                                    day.DailyLiabChange = day.TotalLiabilities - day.LiabOpening;
                                }

                                // 6. Net Store Position & Daily Net Change
                                if (day.NetStorePosition == 0m)
                                {
                                    day.NetStorePosition = day.TotalAssets - day.TotalLiabilities;
                                }
                                if (day.DailyNetChange == 0m && i > 0)
                                {
                                    day.DailyNetChange = day.NetStorePosition - report.DayRows[i - 1].NetStorePosition;
                                }
                            }
                        }

                        // Result Set 2: Summary Totals
                        if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                        {
                            DataRow sr = ds.Tables[1].Rows[0];
                            report.Summary.LatestTotalAssets = ToDec(sr["LatestTotalAssets"]);
                            report.Summary.LatestTotalLiabilities = ToDec(sr["LatestTotalLiabilities"]);
                            report.Summary.LatestNetStorePosition = ToDec(sr["LatestNetStorePosition"]);
                            report.Summary.LatestCashBalance = ToDec(sr["LatestCashBalance"]);
                            report.Summary.LatestBankBalance = ToDec(sr["LatestBankBalance"]);
                            report.Summary.LatestStockValuation = ToDec(sr["LatestStockValuation"]);
                            report.Summary.LatestDebtorsBalance = ToDec(sr["LatestDebtorsBalance"]);
                            report.Summary.LatestCreditorsBalance = ToDec(sr["LatestCreditorsBalance"]);
                            report.Summary.PeriodCashInflow = ToDec(sr["PeriodCashInflow"]);
                            report.Summary.PeriodCashOutflow = ToDec(sr["PeriodCashOutflow"]);
                            report.Summary.PeriodCreditSales = ToDec(sr["PeriodCreditSales"]);
                            report.Summary.PeriodDebtorsCollected = ToDec(sr["PeriodDebtorsCollected"]);
                            report.Summary.PeriodCreditPurchases = ToDec(sr["PeriodCreditPurchases"]);
                            report.Summary.PeriodCreditorsPaid = ToDec(sr["PeriodCreditorsPaid"]);
                            report.Summary.PeriodStockInwardCost = ToDec(sr["PeriodStockInwardCost"]);
                            report.Summary.PeriodStockOutwardCost = ToDec(sr["PeriodStockOutwardCost"]);
                            report.Summary.PeriodNetWorthGrowth = ToDec(sr["PeriodNetWorthGrowth"]);

                            if (sr.Table.Columns.Contains("PeriodAssetOpening")) report.Summary.PeriodAssetOpening = ToDec(sr["PeriodAssetOpening"]);
                            if (sr.Table.Columns.Contains("PeriodAssetClosing")) report.Summary.PeriodAssetClosing = ToDec(sr["PeriodAssetClosing"]);
                            if (sr.Table.Columns.Contains("PeriodAssetGrowth")) report.Summary.PeriodAssetGrowth = ToDec(sr["PeriodAssetGrowth"]);
                            if (sr.Table.Columns.Contains("PeriodAssetGrowthPct")) report.Summary.PeriodAssetGrowthPct = ToDec(sr["PeriodAssetGrowthPct"]);
                            if (sr.Table.Columns.Contains("PeriodLiabOpening")) report.Summary.PeriodLiabOpening = ToDec(sr["PeriodLiabOpening"]);
                            if (sr.Table.Columns.Contains("PeriodLiabClosing")) report.Summary.PeriodLiabClosing = ToDec(sr["PeriodLiabClosing"]);
                            if (sr.Table.Columns.Contains("PeriodLiabGrowth")) report.Summary.PeriodLiabGrowth = ToDec(sr["PeriodLiabGrowth"]);
                            if (sr.Table.Columns.Contains("PeriodNetWorthOpening")) report.Summary.PeriodNetWorthOpening = ToDec(sr["PeriodNetWorthOpening"]);
                            if (sr.Table.Columns.Contains("PeriodNetWorthClosing")) report.Summary.PeriodNetWorthClosing = ToDec(sr["PeriodNetWorthClosing"]);
                        }

                        // Inspect additional tables for Group Rows or Drilldown Rows
                        for (int tIdx = 2; tIdx < ds.Tables.Count; tIdx++)
                        {
                            DataTable table = ds.Tables[tIdx];
                            if (table.Rows.Count == 0) continue;

                            // Check if table contains Group Rows
                            if (table.Columns.Contains("Side") && table.Columns.Contains("GroupName"))
                            {
                                foreach (DataRow gr in table.Rows)
                                {
                                    report.GroupRows.Add(new BalanceSheetGroupMovementRow
                                    {
                                        Side = gr["Side"].ToString(),
                                        GroupID = gr.Table.Columns.Contains("GroupID") && gr["GroupID"] != DBNull.Value ? Convert.ToInt32(gr["GroupID"]) : 0,
                                        GroupName = gr["GroupName"].ToString(),
                                        ParentGroupName = gr.Table.Columns.Contains("ParentGroupName") ? gr["ParentGroupName"].ToString() : string.Empty,
                                        OpeningBal = ToDec(gr["OpeningBal"]),
                                        DebitTotal = ToDec(gr["DebitTotal"]),
                                        CreditTotal = ToDec(gr["CreditTotal"]),
                                        ClosingBal = ToDec(gr["ClosingBal"]),
                                        NetChange = ToDec(gr["NetChange"])
                                    });
                                }
                            }
                            // Check if table contains Drilldown Rows
                            else if (table.Columns.Contains("VoucherID") || table.Columns.Contains("RefNo") || table.Columns.Contains("MovementType"))
                            {
                                foreach (DataRow dr in table.Rows)
                                {
                                    report.DrilldownRows.Add(new DailyFinancialMovementDrilldownRow
                                    {
                                        VoucherID = dr.Table.Columns.Contains("VoucherID") && dr["VoucherID"] != DBNull.Value ? Convert.ToInt32(dr["VoucherID"]) : 0,
                                        TxDate = dr.Table.Columns.Contains("VoucherDate") && dr["VoucherDate"] != DBNull.Value ? Convert.ToDateTime(dr["VoucherDate"]) : (dr.Table.Columns.Contains("TxDate") && dr["TxDate"] != DBNull.Value ? Convert.ToDateTime(dr["TxDate"]) : DateTime.MinValue),
                                        RefNo = dr.Table.Columns.Contains("VoucherNumber") ? dr["VoucherNumber"].ToString() : (dr.Table.Columns.Contains("RefNo") ? dr["RefNo"].ToString() : string.Empty),
                                        MovementType = dr.Table.Columns.Contains("VoucherType") ? dr["VoucherType"].ToString() : (dr.Table.Columns.Contains("MovementType") ? dr["MovementType"].ToString() : string.Empty),
                                        LedgerID = dr.Table.Columns.Contains("LedgerID") && dr["LedgerID"] != DBNull.Value ? Convert.ToInt32(dr["LedgerID"]) : (dr.Table.Columns.Contains("ItemID") && dr["ItemID"] != DBNull.Value ? Convert.ToInt32(dr["ItemID"]) : 0),
                                        LedgerName = dr.Table.Columns.Contains("LedgerName") ? dr["LedgerName"].ToString() : (dr.Table.Columns.Contains("ItemName") ? dr["ItemName"].ToString() : (dr.Table.Columns.Contains("PartyName") ? dr["PartyName"].ToString() : string.Empty)),
                                        GroupName = dr.Table.Columns.Contains("GroupName") ? dr["GroupName"].ToString() : (dr.Table.Columns.Contains("BarCode") ? dr["BarCode"].ToString() : string.Empty),
                                        InAmount = dr.Table.Columns.Contains("Debit") ? ToDec(dr["Debit"]) : (dr.Table.Columns.Contains("InQty") ? ToDec(dr["InQty"]) : 0),
                                        OutAmount = dr.Table.Columns.Contains("Credit") ? ToDec(dr["Credit"]) : (dr.Table.Columns.Contains("OutQty") ? ToDec(dr["OutQty"]) : 0),
                                        Narration = dr.Table.Columns.Contains("Narration") ? dr["Narration"].ToString() : (dr.Table.Columns.Contains("TotalCost") ? "Cost: " + ToDec(dr["TotalCost"]).ToString("N2") : string.Empty),
                                        UserName = dr.Table.Columns.Contains("UserName") ? dr["UserName"].ToString() : string.Empty
                                    });
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            // Guarantee summary progression values are populated
            if (report.DayRows.Count > 0)
            {
                var first = report.DayRows[0];
                var last = report.DayRows[report.DayRows.Count - 1];

                if (report.Summary.PeriodAssetOpening == 0m) report.Summary.PeriodAssetOpening = first.AssetOpening;
                if (report.Summary.PeriodAssetClosing == 0m) report.Summary.PeriodAssetClosing = last.TotalAssets;
                if (report.Summary.PeriodAssetGrowth == 0m) report.Summary.PeriodAssetGrowth = report.Summary.PeriodAssetClosing - report.Summary.PeriodAssetOpening;
                if (report.Summary.PeriodAssetGrowthPct == 0m && report.Summary.PeriodAssetOpening != 0m)
                    report.Summary.PeriodAssetGrowthPct = Math.Round((report.Summary.PeriodAssetGrowth / report.Summary.PeriodAssetOpening) * 100m, 2);

                if (report.Summary.PeriodLiabOpening == 0m) report.Summary.PeriodLiabOpening = first.LiabOpening;
                if (report.Summary.PeriodLiabClosing == 0m) report.Summary.PeriodLiabClosing = last.TotalLiabilities;
                if (report.Summary.PeriodLiabGrowth == 0m) report.Summary.PeriodLiabGrowth = report.Summary.PeriodLiabClosing - report.Summary.PeriodLiabOpening;

                if (report.Summary.PeriodNetWorthOpening == 0m) report.Summary.PeriodNetWorthOpening = report.Summary.PeriodAssetOpening - report.Summary.PeriodLiabOpening;
                if (report.Summary.PeriodNetWorthClosing == 0m) report.Summary.PeriodNetWorthClosing = report.Summary.PeriodAssetClosing - report.Summary.PeriodLiabClosing;
                if (report.Summary.PeriodNetWorthGrowth == 0m) report.Summary.PeriodNetWorthGrowth = report.Summary.PeriodNetWorthClosing - report.Summary.PeriodNetWorthOpening;
            }

            return report;
        }

        private static decimal ToDec(object val)
        {
            if (val == null || val == DBNull.Value) return 0m;
            decimal.TryParse(val.ToString(), out decimal res);
            return res;
        }
    }
}

