using ModelClass;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Repository.ReportRepository
{
    public class SalesHoldReportRepository : BaseRepostitory
    {
        public List<SalesHoldSummaryItem> GetSalesHoldSummary(SalesHoldFilter filter)
        {
            if (filter == null)
            {
                filter = new SalesHoldFilter();
            }

            filter.InitializeFromSessionIfNotSet();
            List<SalesHoldSummaryItem> list = new List<SalesHoldSummaryItem>();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_Rpt_SalesHold, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Operation", "Summary");
                    cmd.Parameters.AddWithValue("@FromDate", filter.IsAllDates ? (object)DBNull.Value : filter.FromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", filter.IsAllDates ? (object)DBNull.Value : filter.ToDate.Date.AddDays(1).AddSeconds(-1));
                    cmd.Parameters.AddWithValue("@CompanyId", filter.CompanyId > 0 ? (object)filter.CompanyId : DBNull.Value);
                    cmd.Parameters.AddWithValue("@BranchId", filter.BranchId > 0 ? (object)filter.BranchId : DBNull.Value);
                    cmd.Parameters.AddWithValue("@LedgerId", filter.LedgerId.HasValue && filter.LedgerId.Value > 0 ? (object)filter.LedgerId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@CustomerName", !string.IsNullOrWhiteSpace(filter.CustomerName) ? (object)filter.CustomerName.Trim() : DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserId", filter.UserId.HasValue && filter.UserId.Value > 0 ? (object)filter.UserId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ItemId", filter.ItemId.HasValue && filter.ItemId.Value > 0 ? (object)filter.ItemId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@CategoryId", filter.CategoryId.HasValue && filter.CategoryId.Value > 0 ? (object)filter.CategoryId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@GroupId", filter.GroupId.HasValue && filter.GroupId.Value > 0 ? (object)filter.GroupId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@BrandId", filter.BrandId.HasValue && filter.BrandId.Value > 0 ? (object)filter.BrandId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", !string.IsNullOrWhiteSpace(filter.Status) && !string.Equals(filter.Status, "ALL", StringComparison.OrdinalIgnoreCase) ? (object)filter.Status.Trim() : DBNull.Value);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);
                        foreach (DataRow row in table.Rows)
                        {
                            list.Add(new SalesHoldSummaryItem
                            {
                                BillNo = row["BillNo"] != DBNull.Value ? Convert.ToInt64(row["BillNo"]) : 0,
                                BillDate = row["BillDate"] != DBNull.Value ? Convert.ToDateTime(row["BillDate"]) : DateTime.MinValue,
                                CustomerName = row["CustomerName"] != DBNull.Value ? row["CustomerName"].ToString() : "",
                                UserName = row["UserName"] != DBNull.Value ? row["UserName"].ToString() : "",
                                PaymodeName = row["PaymodeName"] != DBNull.Value ? row["PaymodeName"].ToString() : "",
                                ItemCount = row["ItemCount"] != DBNull.Value ? Convert.ToInt32(row["ItemCount"]) : 0,
                                TotalQty = row["TotalQty"] != DBNull.Value ? Convert.ToDecimal(row["TotalQty"]) : 0,
                                SubTotal = row["SubTotal"] != DBNull.Value ? Convert.ToDecimal(row["SubTotal"]) : 0,
                                DiscountAmt = row["DiscountAmt"] != DBNull.Value ? Convert.ToDecimal(row["DiscountAmt"]) : 0,
                                TaxAmt = row["TaxAmt"] != DBNull.Value ? Convert.ToDecimal(row["TaxAmt"]) : 0,
                                NetAmount = row["NetAmount"] != DBNull.Value ? Convert.ToDecimal(row["NetAmount"]) : 0,
                                Status = row["Status"] != DBNull.Value ? row["Status"].ToString() : "Hold",
                                BranchId = row["BranchId"] != DBNull.Value ? Convert.ToInt32(row["BranchId"]) : 0,
                                CompanyId = row["CompanyId"] != DBNull.Value ? Convert.ToInt32(row["CompanyId"]) : 0,
                                CounterId = row["CounterId"] != DBNull.Value ? Convert.ToInt32(row["CounterId"]) : 0
                            });
                        }
                    }
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return list;
        }

        public List<SalesHoldDetailItem> GetSalesHoldDetail(SalesHoldFilter filter)
        {
            if (filter == null)
            {
                filter = new SalesHoldFilter();
            }

            filter.InitializeFromSessionIfNotSet();
            List<SalesHoldDetailItem> list = new List<SalesHoldDetailItem>();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_Rpt_SalesHold, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Operation", "Detail");
                    cmd.Parameters.AddWithValue("@FromDate", filter.IsAllDates ? (object)DBNull.Value : filter.FromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", filter.IsAllDates ? (object)DBNull.Value : filter.ToDate.Date.AddDays(1).AddSeconds(-1));
                    cmd.Parameters.AddWithValue("@CompanyId", filter.CompanyId > 0 ? (object)filter.CompanyId : DBNull.Value);
                    cmd.Parameters.AddWithValue("@BranchId", filter.BranchId > 0 ? (object)filter.BranchId : DBNull.Value);
                    cmd.Parameters.AddWithValue("@LedgerId", filter.LedgerId.HasValue && filter.LedgerId.Value > 0 ? (object)filter.LedgerId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@CustomerName", !string.IsNullOrWhiteSpace(filter.CustomerName) ? (object)filter.CustomerName.Trim() : DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserId", filter.UserId.HasValue && filter.UserId.Value > 0 ? (object)filter.UserId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ItemId", filter.ItemId.HasValue && filter.ItemId.Value > 0 ? (object)filter.ItemId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@CategoryId", filter.CategoryId.HasValue && filter.CategoryId.Value > 0 ? (object)filter.CategoryId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@GroupId", filter.GroupId.HasValue && filter.GroupId.Value > 0 ? (object)filter.GroupId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@BrandId", filter.BrandId.HasValue && filter.BrandId.Value > 0 ? (object)filter.BrandId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", !string.IsNullOrWhiteSpace(filter.Status) && !string.Equals(filter.Status, "ALL", StringComparison.OrdinalIgnoreCase) ? (object)filter.Status.Trim() : DBNull.Value);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);
                        foreach (DataRow row in table.Rows)
                        {
                            list.Add(new SalesHoldDetailItem
                            {
                                BillNo = row["BillNo"] != DBNull.Value ? Convert.ToInt64(row["BillNo"]) : 0,
                                BillDate = row["BillDate"] != DBNull.Value ? Convert.ToDateTime(row["BillDate"]) : DateTime.MinValue,
                                CustomerName = row["CustomerName"] != DBNull.Value ? row["CustomerName"].ToString() : "",
                                UserName = row["UserName"] != DBNull.Value ? row["UserName"].ToString() : "",
                                PaymodeName = row["PaymodeName"] != DBNull.Value ? row["PaymodeName"].ToString() : "",
                                SlNo = row["SlNo"] != DBNull.Value ? Convert.ToInt32(row["SlNo"]) : 0,
                                ItemId = row["ItemId"] != DBNull.Value ? Convert.ToInt64(row["ItemId"]) : 0,
                                ItemName = row["ItemName"] != DBNull.Value ? row["ItemName"].ToString() : "",
                                Barcode = row["Barcode"] != DBNull.Value ? row["Barcode"].ToString() : "",
                                Unit = row["Unit"] != DBNull.Value ? row["Unit"].ToString() : "UNIT",
                                Qty = row["Qty"] != DBNull.Value ? Convert.ToDecimal(row["Qty"]) : 0,
                                UnitPrice = row["UnitPrice"] != DBNull.Value ? Convert.ToDecimal(row["UnitPrice"]) : 0,
                                Amount = row["Amount"] != DBNull.Value ? Convert.ToDecimal(row["Amount"]) : 0,
                                DiscountAmount = row["DiscountAmount"] != DBNull.Value ? Convert.ToDecimal(row["DiscountAmount"]) : 0,
                                TaxAmt = row["TaxAmt"] != DBNull.Value ? Convert.ToDecimal(row["TaxAmt"]) : 0,
                                TotalAmount = row["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(row["TotalAmount"]) : 0,
                                Status = row["Status"] != DBNull.Value ? row["Status"].ToString() : "Hold",
                                GroupName = row["GroupName"] != DBNull.Value ? row["GroupName"].ToString() : "",
                                CategoryName = row["CategoryName"] != DBNull.Value ? row["CategoryName"].ToString() : "",
                                BrandName = row["BrandName"] != DBNull.Value ? row["BrandName"].ToString() : "",
                                BranchId = row["BranchId"] != DBNull.Value ? Convert.ToInt32(row["BranchId"]) : 0,
                                CompanyId = row["CompanyId"] != DBNull.Value ? Convert.ToInt32(row["CompanyId"]) : 0,
                                CounterId = row["CounterId"] != DBNull.Value ? Convert.ToInt32(row["CounterId"]) : 0
                            });
                        }
                    }
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return list;
        }
    }
}
