using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass
{
    public  class DataBase
    {
        public static string Status { get; set; } 
        public static string Branch { get; set; }
        public static string BranchId { get; set; }
        public static string UserName { get; set; }
        public static string UserLevel { get; set; }
        public static string Message { get; set; }
        public static string CompanyId { get; set; }
        public static string EmailId { get; set; }
        public static string UserId { get; set; }
        public static string Operations { get; set; }
        public static string FinyearId { get; set; } 
        
        // Global Tax Settings - Malaysia Mode
        public static bool IsTaxEnabled { get; set; } = true; // Default to enabled for backward compatibility
        public static string TaxToggleMessage { get; set; } = "Tax calculations are enabled";
        
       // public string Local { get; set; } = "Local";
    }
}
