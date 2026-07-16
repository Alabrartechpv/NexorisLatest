using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass
{
    public class ClsVendors
    {
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int LedgerId { get; set; }
        public string LedgerName { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public string PriceLevel { get; set; }
        public decimal OpnDebit { get; set; }
        public decimal OpnCredit { get; set; }
        public string SSMNumber { get; set; }
        public string TINNumber { get; set; }
        public string CompanyName { get; set; }
        public string CompanyTIN { get; set; }
        public string CompanyMSICCode { get; set; }
        public string CompanyEmail { get; set; }
        public string _Operation { get; set; }
    }

    public class VendorAddress
    {
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int LedgerId { get; set; }
        public string LedgerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string SSMNumber { get; set; }
        public string TINNumber { get; set; }
        public string CompanyName { get; set; }
        public string CompanyTIN { get; set; }
        public string CompanyMSICCode { get; set; }
        public string CompanyEmail { get; set; }
        public string _Operation { get; set; }
    }

    public class VendorGridList
    {
        public int LedgerID { get; set; }
        public string LedgerName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int BranchID { get; set; }
    }

    public class VendorDDLGrid
    {
        public IEnumerable<VendorGridList> List { get; set; }
    }

    public class VendorAddressDDLGrid
    {
      public IEnumerable<ClsVendors> ListVendor { get; set; }
      public IEnumerable<VendorAddress>ListVendorAddress { get; set; }
    }

}
