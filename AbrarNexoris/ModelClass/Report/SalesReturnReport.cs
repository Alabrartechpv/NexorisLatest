using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Report
{
    /// <summary>
    /// Sales Return Report Master - Header information for each sales return
    /// </summary>
    public class SalesReturnReportMaster
    {
        public int SReturnNo { get; set; }
        public DateTime SReturnDate { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string CustomerName { get; set; }
        public string Paymode { get; set; }
        public double SubTotal { get; set; }
        public double GrandTotal { get; set; }

        // Navigation property for details
        public List<SalesReturnReportDetail> Details { get; set; } = new List<SalesReturnReportDetail>();
    }

    /// <summary>
    /// Sales Return Report Detail - Line items for each sales return
    /// </summary>
    public class SalesReturnReportDetail
    {
        public int SlNo { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public string Packing { get; set; }
        public double Qty { get; set; }
        public double SalesPrice { get; set; }
        public double TaxPer { get; set; }
        public double TaxAmt { get; set; }
        public double Amount { get; set; }
        public string Reason { get; set; }

        // Foreign key reference
        public int SReturnNo { get; set; }
    }

    /// <summary>
    /// Complete Sales Return Report Data Structure
    /// </summary>
    public class SalesReturnReportData
    {
        public SalesReturnReportMaster Master { get; set; }
        public List<SalesReturnReportDetail> Details { get; set; } = new List<SalesReturnReportDetail>();
    }
}
