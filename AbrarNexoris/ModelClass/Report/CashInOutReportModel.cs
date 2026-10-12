using System;
using System.Collections.Generic;

namespace ModelClass.Report
{
    public class CashInOutSummary
    {
        public decimal OpeningBalance { get; set; }
        public decimal TotalCashIn { get; set; }
        public decimal TotalCashOut { get; set; }
        public decimal NetCashFlow { get; set; }
        public decimal ClosingBalance { get; set; }

        // Inflow breakdown
        public decimal CounterCashSales { get; set; }
        public decimal DigitalSales { get; set; }
        public decimal CustomerReceipts { get; set; }
        public decimal GeneralReceipts { get; set; }

        // Outflow breakdown
        public decimal VendorPayments { get; set; }
        public decimal CashPurchases { get; set; }
        public decimal OperatingExpenses { get; set; }
        public decimal SalesReturnsRefund { get; set; }
        public decimal OwnerDrawings { get; set; }

        public int InflowCount { get; set; }
        public int OutflowCount { get; set; }
    }

    public class CashInOutTransaction
    {
        public int SlNo { get; set; }
        public DateTime VoucherDate { get; set; }
        public string VoucherNo { get; set; }
        public string VoucherType { get; set; }
        public string Category { get; set; }
        public string Direction { get; set; } // "IN" or "OUT"
        public string Particulars { get; set; }
        public string PaymentMode { get; set; }
        public decimal InflowAmount { get; set; }
        public decimal OutflowAmount { get; set; }
        public decimal RunningBalance { get; set; }
        public string Narration { get; set; }
    }

    public class CashInOutReportResponse
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public CashInOutSummary Summary { get; set; } = new CashInOutSummary();
        public List<CashInOutTransaction> Transactions { get; set; } = new List<CashInOutTransaction>();
    }
}
