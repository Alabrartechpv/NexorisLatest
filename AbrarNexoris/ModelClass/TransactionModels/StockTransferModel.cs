using System;
using System.Collections.Generic;

namespace ModelClass.TransactionModels
{
    public class StockTransferMaster
    {
        public int Id { get; set; }
        public int StkTrNo { get; set; }
        public DateTime TransferDate { get; set; }
        public int SourceId { get; set; }
        public int TargetId { get; set; }
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Description { get; set; }
        public string TransferType { get; set; }
        public int SourceVoucherId { get; set; }
        public int TargetVoucherId { get; set; }
        public bool CancelFlag { get; set; }
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }
        public string VoucherType { get; set; }
        public string _Operation { get; set; }
    }

    public class StockTransferDetail
    {
        public int Id { get; set; }
        public int StkTranMasterId { get; set; }
        public int StkTrNo { get; set; }
        public DateTime TransferDate { get; set; }
        public int SourceId { get; set; }
        public int TargetId { get; set; }
        public int SlNo { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } // Derived field for grid
        public string BarCode { get; set; }
        public int Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal Amt { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; } // Derived field for grid
        public float SourcePacking { get; set; }
        public string SourceBaseUnit { get; set; }
        public float TargetPacking { get; set; }
        public string TargetBaseUnit { get; set; }
        public bool CancelFlag { get; set; }
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string _Operation { get; set; }
    }
}
