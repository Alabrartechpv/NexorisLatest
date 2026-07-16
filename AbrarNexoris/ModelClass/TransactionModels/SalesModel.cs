using System;
using System.Collections.Generic;

namespace ModelClass.TransactionModels
{
    public class SalesModel
    {
    }
    public class SalesMaster
    {
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public int CounterId { get; set; }
        public long CounterSessionId { get; set; }
        public Int64 BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public Int64 LedgerID { get; set; }
        public string CustomerName { get; set; }
        // public int EmpID { get; set; }
        public Int64 VoucherID { get; set; }
        public int StateId { get; set; }
        public double Freight { get; set; }
        public int PaymodeId { get; set; }
        public string PaymodeName { get; set; }
        public int PaymodeLedgerId { get; set; }
        public int CreditDays { get; set; } = 0;
        public double TaxPer { get; set; } = 0;
        public double TaxAmt { get; set; } = 0;
        public double CessPer { get; set; } = 0;
        public double CessAmt { get; set; } = 0;
        public double KFCessPer { get; set; } = 0;
        public double KFCessAmt { get; set; } = 0;
        public double SubTotal { get; set; }
        public double DiscountPer { get; set; }
        public double DiscountAmt { get; set; }
        public bool RoundOffFlag { get; set; }
        public double RoundOff { get; set; }
        public double NetAmount { get; set; }
        public double TenderedAmount { get; set; }
        public double Balance { get; set; }
        public Int64 CurrencyId { get; set; }
        public string CurrencySymbol { get; set; }
        public bool CancelFlag { get; set; }
        //public SalesDetails[] Items { get; set; } = null;
        public long OrderNo { get; set; } = 0; // ADDED BY M FOR MOBILE ORDER FUNCTIONALITY
                                               //  public string BranchName { get; set; } // ADDED BY M FOR MOBILE ORDER FUNCTIONALITY
                                               // public int? TransporterLedgerId { get; set; } // ADDED BY M FOR MOBILE ORDER FUNCTIONALITY
        public double FreightProfit { get; set; } // ADDED BY M FOR MOBILE ORDER FUNCTIONALITY
                                                  // public bool PaidFreight { get; set; } = false; // ADDED BY M FOR MOBILE ORDER FUNCTIONALITY
                                                  // public string OrderDevice { get; set; } = "Windows";// ADDED BY M FOR MOBILE ORDER FUNCTIONALITY

        //  public double BilledNetAmount { get; set; } = 0; // ADDED BY M FOR MOBILE ORDER FUNCTIONALITY
        // public object LedgerName { get; set; } = null;
        public string SavedVia { get; set; } = null;
        public double ReceivedAmount { get; set; } = 0;
        public int UserId { get; set; } = 0;
        public int EmpID { get; set; } = 0;
        public string Status { get; set; }
        public string _Operation { get; set; }
        public DateTime DueDate { get; set; } // Added DueDate property to track payment due date
        public string PaymentReference { get; set; } // Added PaymentReference property for payment tracking

        // Missing properties that are showing as 0/NULL in database
        public double BillCost { get; set; } = 0; // Bill cost/production cost
        public bool IsPaid { get; set; } = false; // Payment status
        public bool IsSyncd { get; set; } = false; // Sync status for mobile/cloud
    }
    public class SalesDetails
    {
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }
        public int CounterId { get; set; }
        public int BranchID { get; set; }
        public Int64 BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public int SlNO { get; set; }
        public Int64 ItemId { get; set; }
        public string ItemName { get; set; }
        public int UnitId { get; set; }
        public string Unit { get; set; }
        public DateTime Expiry { get; set; }
        public double Qty { get; set; }
        public double Packing { get; set; }
        public double UnitPrice { get; set; }
        public double Amount { get; set; }
        public double DiscountPer { get; set; }
        public double DiscountAmount { get; set; }
        public double MarginPer { get; set; }
        public double MarginAmt { get; set; }
        public double TaxPer { get; set; } = 0; // Tax percentage
        public double TaxAmt { get; set; } = 0; // Tax amount
        public string TaxType { get; set; } = "incl"; // Tax type: "incl" or "excl"
        public float BaseAmount { get; set; } = 0; // Taxable value before tax for GST compliance
        public double TotalAmount { get; set; }
        public double Cost { get; set; }
        public string BaseUnit { get; set; }
        public int VoucherId { get; set; }
        public double MRP { get; set; }
        public string Barcode { get; set; }
        public string _Operation { get; set; }


        // public double RetailPrice { get; set; }
    }

    public class salesGrid
    {
        public IEnumerable<SalesMaster> ListSales { get; set; }
        public IEnumerable<SalesDetails> ListSDetails { get; set; }
    }

    /// <summary>
    /// Data structure to hold GST Summary information for invoice printing
    /// Groups items by tax percentage and calculates CGST/SGST breakdown
    /// </summary>
    public class GSTSummaryItem
    {
        public double TaxPercentage { get; set; }
        public double BaseAmount { get; set; }
        public double CGSTAmount { get; set; }
        public double SGSTAmount { get; set; }
        public double TotalWithGST { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for payment processing results
    /// Used to communicate payment status between payment dialogs and transaction forms
    /// </summary>
    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
        public string BillNumber { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string TenderedAmount { get; set; } = string.Empty;
        public string ChangeAmount { get; set; } = string.Empty;
        public string PaymentMode { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;

        /// <summary>
        /// List of payment details for split payment support.
        /// Contains multiple payment entries when customer pays using different methods.
        /// </summary>
        public List<SalesPaymentDetail> PaymentDetails { get; set; } = new List<SalesPaymentDetail>();
    }

}
