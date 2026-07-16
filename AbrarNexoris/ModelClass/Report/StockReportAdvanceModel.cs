using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Report
{
    /// <summary>
    /// Stock Report Advanced Item - Complete stock movement data
    /// </summary>
    public class StockReportItem
    {
        public int ItemId { get; set; }
        public string GroupName { get; set; }
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public string Barcode { get; set; }
        public string ItemName { get; set; }
        public decimal OpeningStock { get; set; }
        public decimal Purchase { get; set; }
        public decimal PurchaseReturn { get; set; }
        public decimal StockAdjustmentIn { get; set; }
        public decimal StockAdjustmentOut { get; set; }
        public decimal StockTransferIn { get; set; }
        public decimal StockTransferOut { get; set; }
        public decimal Sales { get; set; }
        public decimal SalesReturn { get; set; }
        public decimal ClosingStock { get; set; }
        public decimal OrderedStock { get; set; }
        public decimal Cost { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal WholeSalePrice { get; set; }
        public decimal CreditPrice { get; set; }
        public string BaseUnitName { get; set; }
        public decimal Profit { get; set; }
        public decimal SaleAmount { get; set; }

        public decimal HoldQty { get; set; }
        public decimal AvailableStock => ClosingStock - HoldQty;

        // Calculated properties
        public decimal TotalIn => OpeningStock + Purchase + StockAdjustmentIn + StockTransferIn + SalesReturn;
        public decimal TotalOut => PurchaseReturn + StockAdjustmentOut + StockTransferOut + Sales;
        public decimal StockValue => ClosingStock * Cost;
    }

    /// <summary>
    /// Stock Report Filter Parameters
    /// </summary>
    public class StockReportFilter
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public string BarcodeContains { get; set; }
        public int? GroupId { get; set; }
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public int? LedgerId { get; set; }
    }
}
