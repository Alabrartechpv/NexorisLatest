using System;

namespace PosBranch_Win.Reports.FinancialReports
{
    public sealed class frmVendorLedgerReport : frmCustomerLedgerReport
    {
        public const string Title = "Vendor Ledger Statement";

        public frmVendorLedgerReport() : base(true)
        {
        }

        public void LoadForVendor(int vendorId, string vendorName, DateTime fromDate, DateTime toDate) =>
            LoadForLedger(vendorId, vendorName, fromDate, toDate);
    }
}
