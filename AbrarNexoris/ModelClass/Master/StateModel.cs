using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Master
{
   public class StateModel
    {   
    }
    public class State
    {
        public int StateID { get; set; }
        public int CountryID { get; set; }
        public string StateName { get; set; }
        public int StateCode { get; set; }
        public string _Operation { get; set; }
        public bool IsDelete { get; set; }

    }
    public class StateDDL
    {
        public int StateID { get; set; }
        public string StateName { get; set; }

    }
    public class StateDDlGrid
    {
        public IEnumerable<StateDDL> List { get; set; }
    }
}
