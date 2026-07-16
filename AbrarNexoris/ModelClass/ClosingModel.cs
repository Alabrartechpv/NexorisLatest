using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelClass
{
    /// <summary>
    /// Enhanced Closing Model with all fields for ShiftClosing
    /// </summary>
    public class ClosingModel
    {
        // Basic Info
        public int ShiftClosingId { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public string Counter { get; set; }
        public long CounterSessionId { get; set; }
        public int UserId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ReportSelection { get; set; }
        public string DocNo { get; set; }

        // Sales Summary
        public decimal TotalGrossSales { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalReturn { get; set; }
        public decimal NetSales { get; set; }

        // Payment Collection Summary
        public decimal CashSale { get; set; }
        public decimal CardSale { get; set; }
        public decimal UpiSale { get; set; }
        public decimal CreditSale { get; set; }
        public decimal CustomerReceipt { get; set; }
        public decimal TotalCollection { get; set; }

        // Cash Drawer Summary
        public decimal CashRefundAdjusted { get; set; }
        public decimal MidDayCashSkim { get; set; }
        public decimal SystemExpectedCash { get; set; }

        // Physical Cash Count
        public decimal PhysicalCashCounted { get; set; }
        public decimal CashDifference { get; set; }
        public string DifferenceReason { get; set; }

        // Status & Voucher
        public string Status { get; set; }
        public int? VoucherId { get; set; }

        // Cash Details (Denominations)
        public List<CashDetail> CashDetails { get; set; }

        // Additional Calculated Properties
        public decimal TotalAmount
        {
            get { return CashDetails?.Sum(x => x.Amount) ?? 0; }
        }

        public int TotalBills { get; set; }
        public int CashBills { get; set; }
        public int CardBills { get; set; }
        public int UpiBills { get; set; }

        public ClosingModel()
        {
            CashDetails = new List<CashDetail>();
            TransactionDate = DateTime.Now;
            Status = "Open";
        }
    }

    /// <summary>
    /// Cash Detail (Denomination) Model
    /// </summary>
    public class CashDetail
    {
        public int DenominationId { get; set; }
        public int ShiftClosingId { get; set; }
        public int No { get; set; }
        public decimal Denomination { get; set; }
        public int Quantity { get; set; }

        public decimal Amount
        {
            get { return Denomination * Quantity; }
        }

        public string DisplayText
        {
            get
            {
                if (Denomination >= 1)
                    return $"₹{Denomination:0}";
                else
                    return $"₹{Denomination:0.00}";
            }
        }
    }

    /// <summary>
    /// Sales Data Summary from POS
    /// </summary>
    public class SalesDataSummary
    {
        public decimal TotalGrossSales { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalReturn { get; set; }
        public decimal NetSales { get; set; }
        public decimal CashSale { get; set; }
        public decimal CardSale { get; set; }
        public decimal UpiSale { get; set; }
        public decimal CreditSale { get; set; }
        public decimal TotalCollection { get; set; }
        public int TotalBills { get; set; }
        public int CashBills { get; set; }
        public int CardBills { get; set; }
        public int UpiBills { get; set; }
    }

    /// <summary>
    /// Customer Receipt Summary
    /// </summary>
    public class CustomerReceiptSummary
    {
        public decimal CashReceipt { get; set; }
        public decimal CardReceipt { get; set; }
        public decimal UpiReceipt { get; set; }
        public decimal TotalReceipt { get; set; }
    }
}
