using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.TransactionModels
{
    class StockAdjustmentModel
    {
    }
    public class StockAdjustmentDetails
    {
        public int Id { get; set; }
        public int PhysicalStock { get; set; }
        public int QtyDifference { get; set; }
        public int SystemStock { get; set; }
        public StockAdjustmentDetailsList[] List { get; set; }
    }

    public class StockAdjustmentDetailsList
    {
        public IEnumerable<StockAdjustmentDetails> List { get; set; }
    }
    public class StockAdjMaster
    { 
        public int Id { get; set; }
        public int  FinYearId { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int StockAdjustmentNo { get; set; }
        public DateTime StockAdjustmentDate { get; set; }
        public string Comments { get; set; }
        public int AccountGroupId { get; set; }
        public int LedgerId { get; set; }
        public string LedgerName { get; set; }
        public int VoucherId { get; set; }
        public int UserId { get; set; }
        public int CancelFlag { get; set; }
        public string VoucherType { get; set; }
        public int CategoryId { get; set; }
        public string _Operation { get; set; }
    }
    public class StockAdjPriceDetails
    {
        public int FinYearId { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public string BarCode { get; set; }
        public string Description { get; set; }
        public string UOM { get; set; }
        public int QtyOnHand { get; set; }
        public int AdjQty { get; set; }
        public string Remarks { get; set; }
        public int StockAdjustmentMasterId { get; set; }
        public int LedgerId { get; set; }
        public int SlNo { get; set; }
        public int ItemId { get; set; }
        public int UnitId { get; set; }
        public float Packing { get; set; }
        public int IsBaseUnit { get; set; }
        public float Cost { get; set; }
        public float OriginalCost { get; set; }
        public float SystemStock { get; set; }
        public float PhysicalStock { get; set; }
        public float QtyDifference { get; set; }
        public int CancelFlag { get; set; }
        public int OrderedStock { get; set; }
        public string Reason { get; set; }
        public string _Operation { get; set; }
    }


    public class StockAdjMasterDialog
    {
        public int Id { get; set; }
        public int StockAdjustmentNo { get; set; }
        public DateTime StockAdjustmentDate { get; set; }
        public string Comments { get; set; }
        public string LedgerName { get; set; }
        public int LedgerID { get; set; }
        public int ItemId { get; set; }
        public string BarCode { get; set; }
        public string UnitName { get; set; }
        public string Description { get; set; }
        public double SystemStock { get; set; }
        public string Reason { get; set; }
        public int VoucherId { get; set; }
        public int CategoryId { get; set; }
    }

    public class StockGrid
    {
        public IEnumerable<StockAdjMasterDialog> ListMaster { get; set; }
        public IEnumerable<StockAdjPriceDetails> ListDetails { get; set; }
    }
}
