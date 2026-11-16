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
                                    TotalAssets = ToDec(r["TotalAssets"]),
                                    CreditorsOpening = ToDec(r["CreditorsOpening"]),
                                    CreditorsPurchases = ToDec(r["CreditorsPurchases"]),
                                    CreditorsPaid = ToDec(r["CreditorsPaid"]),
                                    CreditorsClosing = ToDec(r["CreditorsClosing"]),
                                    LoansOpening = ToDec(r["LoansOpening"]),
                                    LoansClosing = ToDec(r["LoansClosing"]),
                                    TotalLiabilities = ToDec(r["TotalLiabilities"]),
                                    NetStorePosition = ToDec(r["NetStorePosition"]),
                                    DailyNetChange = ToDec(r["DailyNetChange"])
                                });
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
                        }

                        // Result Set 3: Drilldown Rows
                        if (ds.Tables.Count > 2 && ds.Tables[2].Rows.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[2].Rows)
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
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
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

