using System;

namespace ModelClass.Report
{
    public class OutputGSTReportFilter
    {
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public DateTime FromDate { get; set; } = DateTime.Today;
        public DateTime ToDate { get; set; } = DateTime.Today;
        public int CustomerLedgerId { get; set; }
        public string SaleType { get; set; } = "ALL"; // ALL, B2B, B2C, Interstate
        public string HSNCode { get; set; } = string.Empty;
        public string SearchText { get; set; } = string.Empty;
    }

    public class SalesGSTRegisterRow
    {
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime DocDate { get; set; }
        public DateTime InvoiceDate => DocDate;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerGSTIN { get; set; } = string.Empty;
        public string SaleType { get; set; } = "B2C"; // B2B / B2C
        public string CustomerType => SaleType;
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
        public decimal TotalOutputGST { get; set; }
        public decimal TotalGST => TotalOutputGST;
        public decimal TotalInvoiceAmount { get; set; }
        public decimal InvoiceValue => TotalInvoiceAmount;
        public string PlaceOfSupply { get; set; } = string.Empty;
        public string SupplyType { get; set; } = "INTRA-STATE"; // INTRA-STATE / INTER-STATE
        public string TaxType { get; set; } = string.Empty;
    }

    public class OutputGSTSummaryRow
    {
        public string SalesCategory { get; set; } = string.Empty; // Taxable Sales, Exempt Sales, Nil Rated Sales, Non-GST Sales
        public string Particulars => SalesCategory;
        public decimal TaxableValue { get; set; }
        public decimal CGSTAmt { get; set; }
        public decimal SGSTAmt { get; set; }
        public decimal IGSTAmt { get; set; }
        public decimal CessAmt { get; set; }
        public decimal TotalOutputGST { get; set; }
    }

    public class OutputGSTRateWiseRow
    {
        public string GSTRate { get; set; } = string.Empty; // 0%, 5%, 12%, 18%, 28%
        public decimal TaxableValue { get; set; }
        public decimal CGSTAmt { get; set; }
        public decimal SGSTAmt { get; set; }
        public decimal IGSTAmt { get; set; }
        public decimal CessAmt { get; set; }
        public decimal TotalGST { get; set; }
    }

    public class B2BSalesRow
    {
        public string CustomerGSTIN { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime DocDate { get; set; }
        public DateTime InvoiceDate => DocDate;
        public decimal TotalInvoiceAmount { get; set; }
        public decimal InvoiceValue => TotalInvoiceAmount;
        public string PlaceOfSupply { get; set; } = string.Empty;
        public string ReverseCharge { get; set; } = "N";
        public decimal TaxableValue { get; set; }
        public string GSTRate { get; set; } = string.Empty;
        public decimal CGSTAmt { get; set; }
        public decimal SGSTAmt { get; set; }
        public decimal IGSTAmt { get; set; }
        public decimal CessAmt { get; set; }
    }

    public class HSNOutputGSTRow
    {
        public string HSNCode { get; set; } = string.Empty;
        public string ItemDescription { get; set; } = string.Empty;
        public string Description => ItemDescription;
        public string Unit { get; set; } = "PCS";
        public string UQC => Unit;
        public double TotalQty { get; set; }
        public decimal TotalValue { get; set; }
        public decimal TaxableValue { get; set; }
        public string GSTRate { get; set; } = string.Empty;
        public decimal CGSTAmt { get; set; }
        public decimal SGSTAmt { get; set; }
        public decimal IGSTAmt { get; set; }
        public decimal CessAmt { get; set; }
        public decimal TotalGST { get; set; }
        public decimal TotalTax => TotalGST;
    }

    public class CreditDebitNoteGSTRow
    {
        public string DocumentType { get; set; } = "Credit Note"; // Credit Note / Debit Note / Sales Return
        public string NoteType => DocumentType;
        public string NoteNo { get; set; } = string.Empty;
        public DateTime NoteDate { get; set; }
        public string RefInvoiceNo { get; set; } = string.Empty;
        public string OriginalInvoiceNo => RefInvoiceNo;
        public DateTime? OriginalInvoiceDate { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public string CustomerName => PartyName;
        public string CustomerGSTIN { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public decimal TaxableAdjustment { get; set; }
        public decimal TaxableValue => TaxableAdjustment;
        public decimal CGSTAdjustment { get; set; }
        public decimal CGSTAmt => CGSTAdjustment;
        public decimal SGSTAdjustment { get; set; }
        public decimal SGSTAmt => SGSTAdjustment;
        public decimal IGSTAdjustment { get; set; }
        public decimal IGSTAmt => IGSTAdjustment;
        public decimal CessAmt { get; set; }
        public decimal NetTotalAdjustment { get; set; }
        public decimal TotalAdjustment => NetTotalAdjustment;
    }
}
