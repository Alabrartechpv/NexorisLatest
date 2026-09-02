using System;

namespace ModelClass.Report
{
    public class InputGSTReportFilter
    {
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public DateTime FromDate { get; set; } = DateTime.Today;
        public DateTime ToDate { get; set; } = DateTime.Today;
        public int SupplierLedgerId { get; set; }
        public string HSNCode { get; set; } = string.Empty;
        public string ITCStatus { get; set; } = "ALL"; // ALL, Eligible, Ineligible
        public string SearchText { get; set; } = string.Empty;
    }

    public class PurchaseGSTRegisterRow
    {
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime DocDate { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string SupplierGSTIN { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string HSNCode { get; set; } = string.Empty;
        public double Qty { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal TaxableValue { get; set; }
        public double CGSTPer { get; set; }
        public decimal CGSTAmt { get; set; }
        public double SGSTPer { get; set; }
        public decimal SGSTAmt { get; set; }
        public double IGSTPer { get; set; }
        public decimal IGSTAmt { get; set; }
        public double CessPer { get; set; }
        public decimal CessAmt { get; set; }
        public decimal TotalInputGST { get; set; }
        public decimal TotalInvoiceAmount { get; set; }
        public string TaxType { get; set; } = string.Empty;
    }

    public class InputGSTSummaryRow
    {
        public string Particulars { get; set; } = string.Empty; // Taxable, Exempt, Nil Rated, Non-GST
        public decimal TaxableValue { get; set; }
        public decimal CGSTAmt { get; set; }
        public decimal SGSTAmt { get; set; }
        public decimal IGSTAmt { get; set; }
        public decimal CessAmt { get; set; }
        public decimal TotalInputGST { get; set; }
    }

    public class InputGSTRateWiseRow
    {
        public string GSTRate { get; set; } = string.Empty; // 0%, 5%, 12%, 18%, 28%
        public decimal TaxableValue { get; set; }
        public decimal CGSTAmt { get; set; }
        public decimal SGSTAmt { get; set; }
        public decimal IGSTAmt { get; set; }
        public decimal CessAmt { get; set; }
        public decimal TotalGST { get; set; }
    }

    public class ITCReportRow
    {
        public string SupplierName { get; set; } = string.Empty;
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public decimal PurchaseGST { get; set; }
        public decimal EligibleITC { get; set; }
        public decimal IneligibleITC { get; set; }
        public string Status { get; set; } = "Eligible"; // Eligible / Ineligible
        public string Reason { get; set; } = string.Empty;
    }

    public class GSTR2BReconcileRow
    {
        public string SupplierName { get; set; } = string.Empty;
        public string InvoiceNo { get; set; } = string.Empty;
        public decimal ERPTaxable { get; set; }
        public decimal ERPGST { get; set; }
        public decimal GSTR2BTaxable { get; set; }
        public decimal GSTR2BGST { get; set; }
        public decimal TaxDiff { get; set; }
        public string Status { get; set; } = "MATCHED"; // MATCHED, NOT IN 2B, DIFFERENCE, DUPLICATE
    }
}
