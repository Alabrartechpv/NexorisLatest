using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ModelClass
{
   public class ClsCustomers
    {
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int LedgerId { get; set; }
        public string LedgerName { get; set; }
        public string AliasName { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public string PriceLevel { get; set; }
        public decimal OpenDebit { get; set; }
        public decimal OpenCredit { get; set; }
        public string SSMNumber { get; set; }
        public string TINNumber { get; set; }
        public string CompanyName { get; set; }
        public string CompanyTIN { get; set; }
        public string CompanyMSICCode { get; set; }
        public string CompanyEmail { get; set; }

        public string _Operation { get; set; }
    }
     
    public class CustomerAddress
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

    public class CustomerGridList
    {
        public int LedgerID { get; set; }
        public string LedgerName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int BranchID { get; set; }
    }


    public class CustomerDDLGrids
    {
        public IEnumerable<CustomerGridList> List { get; set; }
    }

    public class CustAddressDDLGrids
    {
        public IEnumerable<ClsCustomers> ListCustomer { get; set; }
        public IEnumerable<CustomerAddress> ListCustAddress { get; set; }
    }


}
