using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Report
{
    /// <summary>
    /// Purchase Return Report Master - Header information for each purchase return
    /// </summary>
    public class PurchaseReturnReportMaster
    {
        public int PReturnNo { get; set; }
        public DateTime PReturnDate { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string VendorName { get; set; }
        public string Paymode { get; set; }
        public double SubTotal { get; set; }
        public double GrandTotal { get; set; }

        // Navigation property for details
        public List<PurchaseReturnReportDetail> Details { get; set; } = new List<PurchaseReturnReportDetail>();
    }

    /// <summary>
    /// Purchase Return Report Detail - Line items for each purchase return
    /// </summary>
    public class PurchaseReturnReportDetail
    {
        public int SlNo { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public string Packing { get; set; }
        public double Qty { get; set; }
        public double Cost { get; set; }
        public double TaxPer { get; set; }
        public double TaxAmt { get; set; }
        public double Amount { get; set; }
        public string Reason { get; set; }

        // Foreign key reference
        public int PReturnNo { get; set; }
    }

    /// <summary>
    /// Complete Purchase Return Report Data Structure
    /// </summary>
    public class PurchaseReturnReportData
    {
        public PurchaseReturnReportMaster Master { get; set; }
        public List<PurchaseReturnReportDetail> Details { get; set; } = new List<PurchaseReturnReportDetail>();
    }
}
