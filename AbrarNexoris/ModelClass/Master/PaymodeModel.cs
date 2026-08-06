using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Master
{
    public class PaymodeModel
    {
        public int PayModeID { get; set; }
        public string PayModeName { get; set; }
        public string Description { get; set; }
        public string FunctionKey { get; set; }
        public string PaymodeType { get; set; }
        public string Category { get; set; }
        public string FileName { get; set; }
        public byte[] Photo { get; set; }
        public bool RequireFillInReference { get; set; }
        public bool IsHide { get; set; }
        public bool DontOpenDrawer { get; set; }
        public int LedgerID { get; set; }
        public string LedgerName { get; set; }
    }
}
