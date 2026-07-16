using System;

namespace ModelClass.Report
{
    public class SmartReorderItemModel
    {
        public int ItemId { get; set; }
        public int UnitId { get; set; }
        public string ItemName { get; set; }
        public string Barcode { get; set; }
        public string Unit { get; set; }
        public int Order_Cycle_Days { get; set; }
        public int Box_Quantity { get; set; }
        public bool Is_Perishable { get; set; }
        public bool IsSelected { get; set; }
        public string Category { get; set; } = "ALL";
        public string Group { get; set; } = "ALL";
        public decimal CurrentStock { get; set; }
        public decimal AverageDailySales { get; set; }
        public decimal TargetStock { get; set; }
        public decimal ReorderLevel { get; set; }
        public decimal RequiredQuantity { get; set; }
        public decimal SuggestedQuantity { get; set; }
        public decimal FinalQuantity { get; set; }
        public DateTime? NearestExpiryDate { get; set; }
        public DateTime? LastSaleDate { get; set; }
        public string Alert { get; set; }
        public string Reason { get; set; }

        public decimal? DaysOfStockLeft
        {
            get
            {
                if (AverageDailySales <= 0)
                {
                    return null;
                }

                return CurrentStock / AverageDailySales;
            }
        }
    }
}
