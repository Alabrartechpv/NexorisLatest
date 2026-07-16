using System;

namespace ModelClass.Settings
{
    /// <summary>
    /// Model representing a financial year range and status
    /// </summary>
    public class FinancialYearModel
    {
        public int CompanyID { get; set; }
        public DateTime FinYearFrom { get; set; }
        public DateTime FinYearTo { get; set; }
        public int FinYearID { get; set; }
        public int CurFinYear { get; set; }
    }
}
