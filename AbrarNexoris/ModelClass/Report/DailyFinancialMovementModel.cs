using System;
using System.Collections.Generic;

namespace ModelClass.Report
{
    public class DailyFinancialMovementFilter
    {
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string DrillDownCategory { get; set; } = "ALL";
        public DateTime? DrillDownDate { get; set; }
    }

    public class DailyFinancialMovementDayRow
    {
        public DateTime TxDate { get; set; }
        public string DayName { get; set; }

        // Cash
        public decimal CashOpening { get; set; }
        public decimal CashInflow { get; set; }
        public decimal CashOutflow { get; set; }
        public decimal CashClosing { get; set; }

        // Bank / Digital
        public decimal BankOpening { get; set; }
        public decimal BankInflow { get; set; }
        public decimal BankOutflow { get; set; }
        public decimal BankClosing { get; set; }

        // Debtors (Customer Udhaar)
        public decimal DebtorsOpening { get; set; }
        public decimal DebtorsCreditSales { get; set; }
        public decimal DebtorsCollected { get; set; }
        public decimal DebtorsClosing { get; set; }

        // Stock Valuation
        public decimal StockOpening { get; set; }
        public decimal StockInflow { get; set; }
        public decimal StockOutflow { get; set; }
        public decimal StockClosing { get; set; }

        // Total Assets Tonight
        public decimal TotalAssets { get; set; }

        // Creditors (Supplier Udhaar)
        public decimal CreditorsOpening { get; set; }
        public decimal CreditorsPurchases { get; set; }
        public decimal CreditorsPaid { get; set; }
        public decimal CreditorsClosing { get; set; }

        // Loans & Bank OD
        public decimal LoansOpening { get; set; }
        public decimal LoansClosing { get; set; }

        // Total Liabilities Tonight
        public decimal TotalLiabilities { get; set; }

        // Net Store Position (Worth) Tonight
        public decimal NetStorePosition { get; set; }
        public decimal DailyNetChange { get; set; }
    }

    public class DailyFinancialMovementSummary
    {
        public decimal LatestTotalAssets { get; set; }
        public decimal LatestTotalLiabilities { get; set; }
        public decimal LatestNetStorePosition { get; set; }
        public decimal LatestCashBalance { get; set; }
        public decimal LatestBankBalance { get; set; }
        public decimal LatestStockValuation { get; set; }
        public decimal LatestDebtorsBalance { get; set; }
        public decimal LatestCreditorsBalance { get; set; }
        public decimal PeriodCashInflow { get; set; }
        public decimal PeriodCashOutflow { get; set; }
        public decimal PeriodCreditSales { get; set; }
        public decimal PeriodDebtorsCollected { get; set; }
        public decimal PeriodCreditPurchases { get; set; }
        public decimal PeriodCreditorsPaid { get; set; }
        public decimal PeriodStockInwardCost { get; set; }
        public decimal PeriodStockOutwardCost { get; set; }
        public decimal PeriodNetWorthGrowth { get; set; }
    }

    public class DailyFinancialMovementDrilldownRow
    {
        public int VoucherID { get; set; }
        public DateTime TxDate { get; set; }
        public string RefNo { get; set; }
        public string MovementType { get; set; }
        public int LedgerID { get; set; }
        public string LedgerName { get; set; }
        public string GroupName { get; set; }
        public decimal InAmount { get; set; }
        public decimal OutAmount { get; set; }
        public string Narration { get; set; }
        public string UserName { get; set; }
    }

    public class DailyFinancialMovementReport
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<DailyFinancialMovementDayRow> DayRows { get; set; } = new List<DailyFinancialMovementDayRow>();
        public DailyFinancialMovementSummary Summary { get; set; } = new DailyFinancialMovementSummary();
        public List<DailyFinancialMovementDrilldownRow> DrilldownRows { get; set; } = new List<DailyFinancialMovementDrilldownRow>();
    }
}

