using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Master
{
    public class CompanyModel
    {
        public int CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string CompanyCaption { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Address4 { get; set; }
        public int? Country { get; set; }
        public int? State { get; set; }
        public string Zipcode { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string BusinessType { get; set; }
        public string BackupPath { get; set; }
        public byte[] Logo { get; set; }
        public DateTime? FinYearFrom { get; set; }
        public DateTime? FinYearTo { get; set; }
        public DateTime? BookFrom { get; set; }
        public DateTime? BookTo { get; set; }
        public int? TaxSystem { get; set; }
        public string TaxNo { get; set; }
        public string LicenseNo { get; set; }
        public string DLNO1 { get; set; }
        public string DLNO2 { get; set; }
        public string FSSAINo { get; set; }
        public int? Currency { get; set; }
        public bool IsDelete { get; set; }
    }
    public class CompanyDDl
    {
        public int CompanyID { get; set; }
        public string CompanyName { get; set; }
    }
    public class CompanyDDlGrid
    {
        public IEnumerable<CompanyDDl> List { get; set; }
    }

    /// <summary>
    /// Static class to store application-wide session information
    /// </summary>
    public static class AppSession
    {
        // Company information
        public static int CompanyID { get; set; }
        public static string CompanyName { get; set; }
        public static string CompanyCaption { get; set; }
        public static byte[] CompanyLogo { get; set; }

        // User information
        public static int UserID { get; set; }
        public static string UserName { get; set; }
        public static int BranchID { get; set; }
        public static string BranchName { get; set; }

        // Financial year information
        public static DateTime? FinYearFrom { get; set; }
        public static DateTime? FinYearTo { get; set; }

        /// <summary>
        /// Load company information from CompanyModel into session
        /// </summary>
        /// <param name="company">Company model with data</param>
        public static void LoadCompanyInfo(CompanyModel company)
        {
            if (company != null)
            {
                CompanyID = company.CompanyID;
                CompanyName = company.CompanyName;
                CompanyCaption = company.CompanyCaption;
                CompanyLogo = company.Logo;
                FinYearFrom = company.FinYearFrom;
                FinYearTo = company.FinYearTo;
            }
        }

        /// <summary>
        /// Clear all session information
        /// </summary>
        public static void ClearSession()
        {
            CompanyID = 0;
            CompanyName = null;
            CompanyCaption = null;
            CompanyLogo = null;
            UserID = 0;
            UserName = null;
            BranchID = 0;
            BranchName = null;
            FinYearFrom = null;
            FinYearTo = null;
        }
    }
}
