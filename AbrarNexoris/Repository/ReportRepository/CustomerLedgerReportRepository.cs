using System.Collections.Generic;
using System.Linq;
using ModelClass;

namespace Repository.ReportRepository
{
    public class CustomerLedgerReportRepository : LedgerReportRepository
    {
        public CustomerLedgerReportRepository() : base(STOREDPROCEDURE._POS_CustomerLedger)
        {
        }

        public List<CustomerGridList> GetCustomers()
        {
            CustomerRepositoty customerRepository = new CustomerRepositoty();
            CustomerDDLGrids data = customerRepository.GetCustomerDDL();

            if (data == null || data.List == null)
                return new List<CustomerGridList>();

            return data.List
                .Where(x => x != null && x.LedgerID > 0)
                .OrderBy(x => x.LedgerName)
                .ToList();
        }
    }
}
