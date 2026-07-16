using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.TransactionModels
{
  public class SalesRNItem
    {
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public int CounterId { get; set; }
        public Int64 BillNo { get; set; }
        public Int64 ItemId { get; set; }
        public string Description { get; set; }
        public int UnitId { get; set; }
        public string Unit { get; set; }
        public DateTime Expiry { get; set; }
        public double Qty { get; set; }
        public double Packing { get; set; }
        public double UnitPrice { get; set; }
        public double Amount { get; set; }
        public double DiscountPer { get; set; }
        public double DiscountAmount { get; set; }
        public double MarginPer { get; set; }
        public double MarginAmt { get; set; }
        public double TaxPer { get; set; }
        public double TaxAmt { get; set; }
        public double CessPer { get; set; }
        public double CessAmt { get; set; }
        public double KFCessPer { get; set; }
        public double KFCessAmt { get; set; }
        public double TotalAmount { get; set; }
        public bool CancelFlag { get; set; }
        public double OldQty { get; set; }
        public Int64 BatchId { get; set; }
        public string TaxType { get; set; }
        public double Cost { get; set; }
        public string BaseUnit { get; set; }
        public string Barcode { get; set; }
        public double MRP { get; set; }
        public string _Operation { get; set; }
    }
}
