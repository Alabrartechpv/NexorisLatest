using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Report
{
    /// <summary>
    /// Purchase Report Master - Header information for each purchase bill
    /// </summary>
    public class PurchaseReportMaster
    {
        public int PurchaseNo { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string VendorName { get; set; }
        public string Paymode { get; set; }
        public double SubTotal { get; set; }
        public double GrandTotal { get; set; }
        public double PayedAmount { get; set; }
        public string BilledBy { get; set; }

        // Navigation property for details
        public List<PurchaseReportDetail> Details { get; set; } = new List<PurchaseReportDetail>();
    }

    /// <summary>
    /// Purchase Report Detail - Line items for each purchase bill
    /// </summary>
    public class PurchaseReportDetail
    {
        public int SlNo { get; set; }
        public string ItemName { get; set; }
        public string BarCode { get; set; }
        public string Unit { get; set; }
        public string Packing { get; set; }
        public double Qty { get; set; }
        public double Cost { get; set; }
        public double Amount { get; set; }
        public double Free { get; set; }

        // Foreign key reference
        public int PurchaseNo { get; set; }
    }

    /// <summary>
    /// Complete Purchase Report Data Structure
    /// </summary>
    public class PurchaseReportData
    {
        public PurchaseReportMaster Master { get; set; }
        public List<PurchaseReportDetail> Details { get; set; } = new List<PurchaseReportDetail>();
    }
}
