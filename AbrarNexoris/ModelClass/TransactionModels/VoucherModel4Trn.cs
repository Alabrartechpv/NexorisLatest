using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.TransactionModels
{
   public class VoucherModel4Trn
    {
    }

    public class Voucher
    {
        public int CompanyID { get; set; }
        public int BranchID { get; set; }
        public Int64 VoucherID { get; set; }
        public int VoucherSeriesID { get; set; }
        public DateTime? VoucherDate { get; set; }
        public string VoucherNumber { get; set; }
        public Int64 LedgerID { get; set; }
        public string LedgerName { get; set; }
        public string VoucherType { get; set; }
        public int GroupID { get; set; }
        public double Debit { get; set; }
        public double Credit { get; set; }
        public string Narration { get; set; }
        public int SlNo { get; set; }
        public string Mode { get; set; }
        public int ModeID { get; set; }
        public DateTime? UserDate { get; set; }
        public string UserName { get; set; }
        public int UserID { get; set; }
        public bool CancelFlag { get; set; }
        public int FinYearID { get; set; }
        public bool IsSyncd { get; set; }
        public string _Operation { get; set; }
    }


}
