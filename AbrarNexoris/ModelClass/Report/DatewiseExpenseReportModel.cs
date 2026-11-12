using System;
using System.Collections.Generic;
using System.Data;

namespace ModelClass.Report
{
    /// <summary>
    /// Represents a single expense transaction row returned by POS_DatewiseExpenseReport.
    /// </summary>
    public class DatewiseExpenseReportRow
    {
        public int VoucherID { get; set; }
        public string VoucherNumber { get; set; }
        public DateTime VoucherDate { get; set; }
        public string VoucherType { get; set; }
        public int LedgerID { get; set; }
        public string LedgerName { get; set; }
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public string ExpenseCategory { get; set; } // "Direct Expense" or "Indirect Expense"
        public decimal ExpenseAmount { get; set; }
        public int CreditLedgerID { get; set; }
        public string PaidThrough { get; set; }
        public string PaymentMode { get; set; }
        public string Narration { get; set; }
        public string UserName { get; set; }
        public int BranchID { get; set; }
        public string BranchName { get; set; }
        public int CompanyID { get; set; }
    }

    /// <summary>
    /// Represents an individual expense voucher row (for child band when expanding +).
    /// </summary>
    public class DatewiseExpenseVoucherRow
    {
        public int LedgerID { get; set; }
        public int VoucherID { get; set; }
        public string VoucherNumber { get; set; }
        public DateTime VoucherDate { get; set; }
        public string VoucherType { get; set; }
        public string LedgerName { get; set; }
        public string GroupName { get; set; }
        public string ExpenseCategory { get; set; }
        public decimal ExpenseAmount { get; set; }
        public string PaidThrough { get; set; }
        public string PaymentMode { get; set; }
        public string Narration { get; set; }
        public string UserName { get; set; }
        public string BranchName { get; set; }
    }

    /// <summary>
    /// Filter criteria for Date-wise Expense Report.
    /// </summary>
    public class DatewiseExpenseReportFilter
    {
        public string PeriodMode { get; set; } = "DATE_RANGE"; // "DATE_RANGE", "MONTH_WISE", "YEAR_WISE", "ALL"
        public DateTime FromDate { get; set; } = DateTime.Today;
        public DateTime ToDate { get; set; } = DateTime.Today;
        public int Month { get; set; } = DateTime.Today.Month;
        public int Year { get; set; } = DateTime.Today.Year;
        public string ViewMode { get; set; } = "DATE_MATRIX"; // "DATE_MATRIX", "DETAILED", "DATE_SUMMARY", "EXPENSE_SUMMARY"
        public int ExpenseCategoryId { get; set; } = 0; // 0 = ALL, 10 = Direct, 12 = Indirect
        public int LedgerId { get; set; } = 0; // 0 = ALL
        public string PaymentMode { get; set; } = "ALL"; // "ALL", "CASH", "BANK"
        public string SearchText { get; set; } = string.Empty;
        public int BranchId { get; set; } = 0;
        public int CompanyId { get; set; } = 1;
    }

    /// <summary>
    /// Holds the complete dataset and metrics returned by DatewiseExpenseReportRepository.
    /// </summary>
    public class DatewiseExpenseReportData
    {
        public DataSet ReportDataSet { get; set; }
        public List<DatewiseExpenseReportRow> ExpenseTransactions { get; set; } = new List<DatewiseExpenseReportRow>();
        public List<DatewiseExpenseVoucherRow> VoucherDetails { get; set; } = new List<DatewiseExpenseVoucherRow>();
        public decimal GrandTotalExpense { get; set; }
        public decimal TotalDirectExpense { get; set; }
        public decimal TotalIndirectExpense { get; set; }
        public int TotalVouchers { get; set; }
    }
}
