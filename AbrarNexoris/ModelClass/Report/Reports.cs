using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Report
{
    public class Reports
    {
    }
    public class Sales_Daily
    {

        public Int64 BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public string CustomerName { get; set; }
        public string PaymodeName { get; set; }
        public double SubTotal { get; set; }
        public double NetAmount { get; set; }
        public double ReceivedAmount { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// Sales Report Master - Header information for each bill
    /// </summary>
    public class SalesReportMaster
    {
        public int BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public string CustomerName { get; set; }
        public string PaymodeName { get; set; }
        public string CashMode { get; set; }
        public double TaxPer { get; set; }
        public double TaxAmt { get; set; }
        public double SubTotal { get; set; }
        public double NetAmount { get; set; }
        public double Profit { get; set; }

        // Navigation property for details
        public List<SalesReportDetail> Details { get; set; } = new List<SalesReportDetail>();
    }

    /// <summary>
    /// Sales Report Detail - Line items for each bill
    /// </summary>
    public class SalesReportDetail
    {
        public int SlNo { get; set; }
        public string ItemName { get; set; }
        public string Barcode { get; set; }
        public string Unit { get; set; }
        public string Packing { get; set; }
        public double Qty { get; set; }
        public double UnitPrice { get; set; }
        public double Amount { get; set; }
        public double MarginPer { get; set; }
        public double Profit { get; set; }  // Renamed from MarginAmt
        public double TaxPer { get; set; }
        public double TaxAmt { get; set; }
        public double TotalAmount { get; set; }

        // Foreign key reference
        public int BillNo { get; set; }
    }

    /// <summary>
    /// Complete Sales Report Data Structure
    /// </summary>
    public class SalesReportData
    {
        public SalesReportMaster Master { get; set; }
        public List<SalesReportDetail> Details { get; set; } = new List<SalesReportDetail>();
    }

    /// <summary>
    /// Sales Profit Report - Bill-wise profit summary
    /// </summary>
    public class SalesProfitReport
    {
        public int BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public double BillAmount { get; set; }
        public double Profit { get; set; }
        public string PayMode { get; set; }
        public string CashMode { get; set; }
    }
}
