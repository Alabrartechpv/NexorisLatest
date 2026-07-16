using System;

namespace ModelClass.Report
{
    /// <summary>
    /// Represents a row in the Low Stock Alert report
    /// </summary>
    public class LowStockAlertItem
    {
        public long SlNo { get; set; }
        public int ItemId { get; set; }
        public string Barcode { get; set; }
        public string ItemName { get; set; }
        public string GroupName { get; set; }
        public string CategoryName { get; set; }
        public string BaseUnitName { get; set; }
        public double CostPrice { get; set; }
        public double RetailPrice { get; set; }
        public double ReorderLevel { get; set; }
        public double CurrentStock { get; set; }
        public double ShortageQty { get; set; }
    }

    /// <summary>
    /// Filter parameters for loading the Low Stock Alert report
    /// </summary>
    public class LowStockAlertFilter
    {
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public int? GroupId { get; set; }
        public int? CategoryId { get; set; }
        public string SearchQuery { get; set; }
    }
}
