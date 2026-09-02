using System;

namespace ModelClass.Report
{
    public class GovtGSTReturnFilter
    {
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public DateTime FromDate { get; set; } = DateTime.Today;
        public DateTime ToDate { get; set; } = DateTime.Today;
        public int TaxMonth { get; set; } = DateTime.Today.Month;
        public int TaxYear { get; set; } = DateTime.Today.Year;
    }

    public class GSTR1WorkingRow
    {
        public string ReturnSection { get; set; } = string.Empty; // B2B Invoices, B2C Supplies, Credit/Debit Notes, HSN Summary, Exempt/Nil
        public int InvoiceCount { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal CGSTAmt { get; set; }
        public decimal SGSTAmt { get; set; }
        public decimal IGSTAmt { get; set; }
        public decimal CessAmt { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public string FilingStatus { get; set; } = "Prepared";
    }

    public class GSTR3BWorkingRow
    {
        public string SectionCode { get; set; } = string.Empty; // 3.1 Outward Taxable, 4.A Eligible ITC, 4.B ITC Reversed, 5. Exempt
        public string Description { get; set; } = string.Empty;
        public decimal TaxableValue { get; set; }
        public decimal IGSTAmt { get; set; }
        public decimal CGSTAmt { get; set; }
        public decimal SGSTAmt { get; set; }
        public decimal CessAmt { get; set; }
    }

    public class GSTLiabilityUtilizationRow
    {
        public string Particulars { get; set; } = string.Empty; // Output Tax Liability, Eligible ITC Available, ITC Utilized, Net Cash Payable
        public decimal IGSTAmt { get; set; }
        public decimal CGSTAmt { get; set; }
        public decimal SGSTAmt { get; set; }
        public decimal CessAmt { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class MonthlyGSTExecutiveSummary
    {
        public string CompanyName { get; set; } = "ABC Supermarket";
        public string GSTIN { get; set; } = string.Empty;
        public string TaxPeriod { get; set; } = string.Empty; // e.g. September 2026

        // Purchase / Input GST Metrics
        public decimal PurchaseTaxableValue { get; set; }
        public decimal InputCGST { get; set; }
        public decimal InputSGST { get; set; }
        public decimal InputIGST { get; set; }
        public decimal EligibleITC { get; set; }
        public decimal IneligibleITC { get; set; }
        public decimal GSTR2BMatched { get; set; }
        public decimal GSTR2BDifference { get; set; }

        // Sales / Output GST Metrics
        public decimal SalesTaxableValue { get; set; }
        public decimal B2BSalesValue { get; set; }
        public decimal B2CSalesValue { get; set; }
        public decimal OutputCGST { get; set; }
        public decimal OutputSGST { get; set; }
        public decimal OutputIGST { get; set; }
        public decimal TotalOutputGST { get; set; }

        // Government Return & Payment Metrics
        public string GSTR1Status { get; set; } = "Prepared";
        public string GSTR2BStatus { get; set; } = "Reconciled";
        public string GSTR3BStatus { get; set; } = "Prepared";
        public decimal NetGSTLiability { get; set; }
        public decimal ITCUtilized { get; set; }
        public decimal GSTCashPayment { get; set; }
        public string ReconciliationStatus { get; set; } = "MATCHED";
    }
}
