using System;
using System.Data;
using System.Data.SqlClient;

namespace Repository.ReportRepository
{
    public class VendorPurchaseReportRepository : BaseRepostitory
    {
        public DataTable GetVendorPurchases(DateTime fromDate, DateTime toDate, int vendorLedgerId, int itemId, int companyId, int branchId, int finYearId)
        {
            return GetReport("VENDOR", fromDate, toDate, vendorLedgerId, itemId, companyId, branchId, finYearId);
        }

        public DataTable GetItemVendorPurchases(DateTime fromDate, DateTime toDate, int itemId, int companyId, int branchId, int finYearId)
        {
            return GetReport("ITEM", fromDate, toDate, 0, itemId, companyId, branchId, finYearId);
        }

        public DataTable GetVendorItemPurchases(DateTime fromDate, DateTime toDate, int vendorLedgerId, int itemId, int companyId, int branchId, int finYearId)
        {
            return GetReport("BOTH", fromDate, toDate, vendorLedgerId, itemId, companyId, branchId, finYearId);
        }

        private DataTable GetReport(string mode, DateTime fromDate, DateTime toDate, int vendorLedgerId, int itemId, int companyId, int branchId, int finYearId)
        {
            DataTable result = new DataTable();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_VendorPurchaseReport, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 180;
                    cmd.Parameters.AddWithValue("@Mode", mode);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date);
                    cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
                    cmd.Parameters.AddWithValue("@ItemId", itemId);
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading vendor purchase report. " + ex.Message, ex);
            }
            finally
            {
                DataConnection.Close();
            }

            return result;
        }
    }
}
