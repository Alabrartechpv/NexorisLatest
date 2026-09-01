using System;

namespace ModelClass.Report
{
    public class GSTAndTaxReportFilter
    {
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public DateTime FromDate { get; set; } = DateTime.Today;
        public DateTime ToDate { get; set; } = DateTime.Today;
        public string TrnsType { get; set; } = "ALL";
        public string TaxType { get; set; } = "ALL";
        public string TaxPer { get; set; } = "ALL";
        public string SearchText { get; set; } = string.Empty;
    }

    public class GSTAndTaxReportRow
    {
        public string TrnsType { get; set; } = string.Empty;
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime DocDate { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public string PartyGSTIN { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string HSNCode { get; set; } = string.Empty;
        public double Qty { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal TaxableAmt { get; set; }
        public double TaxPer { get; set; }
        public double CGSTPer { get; set; }
        public decimal CGSTAmt { get; set; }
        public double SGSTPer { get; set; }
        public decimal SGSTAmt { get; set; }
        public double IGSTPer { get; set; }
        public decimal IGSTAmt { get; set; }
        public double CessPer { get; set; }
        public decimal CessAmt { get; set; }
        public decimal TotalTaxAmt { get; set; }
        public decimal GrandTotal { get; set; }
        public string TaxType { get; set; } = string.Empty;
        public string TaxCategory { get; set; } = string.Empty; // Output Tax / Input Tax (ITC)
        public decimal OutputTaxAmt { get; set; }
        public decimal InputTaxAmt { get; set; }
    }
}
