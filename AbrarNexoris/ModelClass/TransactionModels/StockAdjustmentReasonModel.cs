using System;

namespace ModelClass.TransactionModels
{
    public class StockAdjustmentReasonMaster
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public string ReasonName { get; set; }
        public string ReasonType { get; set; } // 'Loss' (Group 12), 'Gain' (Group 13), 'DirectLoss' (Group 10)
        public int LedgerId { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
