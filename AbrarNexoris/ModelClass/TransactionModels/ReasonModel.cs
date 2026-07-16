using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.TransactionModels
{
    class ReasonModel
    {

    }
    public class Reason 
    {
        public int LedgerID { get; set; }
        public string ReasonName { get; set; }
    }
    
    public class ReasonDDLGrid
    {
        public IEnumerable<Reason> List { get; set; }
    }
}
