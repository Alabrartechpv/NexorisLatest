using ModelClass;
using ModelClass.Master;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Repository.ReportRepository
{
    public class CustomerOutstandingReportRepository : BaseRepostitory
    {
        /// <summary>
        /// Gets bill-wise outstanding invoices for a given customer via stored procedure.
        /// Only returns bills where NetAmount != ReceivedAmount (outstanding bills).
        /// </summary>
        public List<CustomerOutstandingReportRow> GetReport(CustomerOutstandingReportFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            List<CustomerOutstandingReportRow> rows = new List<CustomerOutstandingReportRow>();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CustomerOutstandingReport, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = (filter.UseDateFilter && filter.FromDate.HasValue) ? (object)filter.FromDate.Value.Date : DBNull.Value;
                    cmd.Parameters.Add("@ToDate", SqlDbType.DateTime).Value = (filter.UseDateFilter && filter.ToDate.HasValue) ? (object)filter.ToDate.Value.Date : DBNull.Value;
                    int branchId = filter.BranchId > 0 ? filter.BranchId : GetContextValue(SessionContext.BranchId, DataBase.BranchId);
                    cmd.Parameters.Add("@BranchId", SqlDbType.Int).Value = branchId;
                    cmd.Parameters.Add("@LedgerId", SqlDbType.Int).Value = filter.LedgerId;
                    cmd.Parameters.Add("@_Operation", SqlDbType.VarChar, 50).Value = "GETOUTSTANDING";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);

                        foreach (DataRow row in table.Rows)
                        {
                            long billNo = ToLong(row, "BillNo");
                            string docNo = ToString(row, "DocNo");
                            if (string.IsNullOrWhiteSpace(docNo))
                            {
                                docNo = ToString(row, "InvoiceNo");
                            }
                            if (string.IsNullOrWhiteSpace(docNo) && billNo > 0)
                            {
                                docNo = billNo.ToString();
                            }

                            string salesPerson = ToString(row, "SalesPerson");
                            if (string.IsNullOrWhiteSpace(salesPerson))
                            {
                                salesPerson = ToString(row, "SalesmanName");
                            }
                            if (string.IsNullOrWhiteSpace(salesPerson))
                            {
                                salesPerson = ToString(row, "Salesman");
                            }
                            if (string.IsNullOrWhiteSpace(salesPerson))
                            {
                                salesPerson = ToString(row, "UserName");
                            }

                            rows.Add(new CustomerOutstandingReportRow
                            {
                                LedgerID = ToInt(row, "LedgerID"),
                                LedgerName = ToString(row, "LedgerName"),
                                BillNo = billNo,
                                DocNo = docNo,
                                BillDate = ToNullableDateTime(row, "BillDate"),
                                DueDate = ToNullableDateTime(row, "DueDate"),
                                InvoiceAmount = ToDecimal(row, "InvoiceAmount"),
                                ReceivedAmount = ToDecimal(row, "ReceivedAmount"),
                                Balance = ToDecimal(row, "Balance"),
                                SalesPerson = salesPerson,
                                SalesmanId = ToNullableInt(row, "SalesmanId") ?? ToNullableInt(row, "UserId") ?? ToNullableInt(row, "EmpID"),
                                CategoryName = ToString(row, "CategoryName"),
                                CategoryId = ToNullableInt(row, "CategoryId")
                            });
                        }
                    }
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return rows;
        }

        public List<CustomerBillReceiptDetail> GetBillReceiptHistory(int customerLedgerId, long billNo, int branchId, decimal invoiceAmt, decimal balance)
        {
            List<CustomerBillReceiptDetail> list = new List<CustomerBillReceiptDetail>();
            int effectiveBranchId = branchId > 0 ? branchId : GetContextValue(SessionContext.BranchId, DataBase.BranchId);

            // 1. Fetch Receipt History using Stored Procedure STOREDPROCEDURE._CustomerReceiptMaster (Operation: VIEWRECEIPT)
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                DataTable dt = new DataTable();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CustomerReceiptMaster, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CustomerLedgerId", customerLedgerId);
                        cmd.Parameters.AddWithValue("@BillNoUntil", billNo);
                        cmd.Parameters.AddWithValue("@BranchId", effectiveBranchId);
                        cmd.Parameters.AddWithValue("@_Operation", "VIEWRECEIPT");

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calling SP _CustomerReceiptMaster VIEWRECEIPT: {ex.Message}");
                }

                // 2. Fallback to Stored Procedure STOREDPROCEDURE._CustomerReceiptDetails (Operation: GETBYBILLNO)
                if (dt == null || dt.Rows.Count == 0)
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CustomerReceiptDetails, (SqlConnection)DataConnection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@CustomerLedgerId", customerLedgerId);
                            cmd.Parameters.AddWithValue("@BillNo", billNo);
                            cmd.Parameters.AddWithValue("@BranchId", effectiveBranchId);
                            cmd.Parameters.AddWithValue("@_Operation", "GETBYBILLNO");

                            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            {
                                da.Fill(dt);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error calling SP _CustomerReceiptDetails GETBYBILLNO: {ex.Message}");
                    }
                }

                if (dt != null && dt.Rows.Count > 0)
                {
                    decimal runningBillAmt = invoiceAmt > 0 ? invoiceAmt : 0m;
                    decimal runningBal = invoiceAmt;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow r = dt.Rows[i];
                        decimal rcptAmt = r.Table.Columns.Contains("ReceiptAmount") && r["ReceiptAmount"] != DBNull.Value
                            ? Convert.ToDecimal(r["ReceiptAmount"])
                            : (r.Table.Columns.Contains("ReceivedAmount") && r["ReceivedAmount"] != DBNull.Value
                                ? Convert.ToDecimal(r["ReceivedAmount"])
                                : (r.Table.Columns.Contains("Amount") && r["Amount"] != DBNull.Value
                                    ? Convert.ToDecimal(r["Amount"])
                                    : 0m));

                        if (rcptAmt <= 0) continue;

                        decimal billAmt = r.Table.Columns.Contains("BillAmount") && r["BillAmount"] != DBNull.Value
                            ? Convert.ToDecimal(r["BillAmount"])
                            : (r.Table.Columns.Contains("InvoiceAmount") && r["InvoiceAmount"] != DBNull.Value
                                ? Convert.ToDecimal(r["InvoiceAmount"])
                                : runningBillAmt);

                        if (i == 0 && runningBillAmt == 0) runningBillAmt = billAmt;
                        runningBal = Math.Max(0m, runningBal - rcptAmt);

                        decimal balAmt = r.Table.Columns.Contains("BalanceAmount") && r["BalanceAmount"] != DBNull.Value
                            ? Convert.ToDecimal(r["BalanceAmount"])
                            : (r.Table.Columns.Contains("Balance") && r["Balance"] != DBNull.Value
                                ? Convert.ToDecimal(r["Balance"])
                                : runningBal);

                        DateTime rcptDate = r.Table.Columns.Contains("ReceiptDate") && r["ReceiptDate"] != DBNull.Value
                            ? Convert.ToDateTime(r["ReceiptDate"])
                            : (r.Table.Columns.Contains("VoucherDate") && r["VoucherDate"] != DBNull.Value
                                ? Convert.ToDateTime(r["VoucherDate"])
                                : (r.Table.Columns.Contains("BillDate") && r["BillDate"] != DBNull.Value
                                    ? Convert.ToDateTime(r["BillDate"])
                                    : DateTime.Today));

                        string voucherNo = r.Table.Columns.Contains("VoucherNo") && r["VoucherNo"] != DBNull.Value
                            ? Convert.ToString(r["VoucherNo"])
                            : (r.Table.Columns.Contains("VoucherId") && r["VoucherId"] != DBNull.Value
                                ? "VCH-" + Convert.ToString(r["VoucherId"])
                                : (r.Table.Columns.Contains("ReceiptMasterId") && r["ReceiptMasterId"] != DBNull.Value
                                    ? "CR-" + Convert.ToString(r["ReceiptMasterId"])
                                    : "RECEIPT"));

                        string payMode = r.Table.Columns.Contains("PaymentMode") && r["PaymentMode"] != DBNull.Value
                            ? Convert.ToString(r["PaymentMode"])
                            : (r.Table.Columns.Contains("PayModeName") && r["PayModeName"] != DBNull.Value
                                ? Convert.ToString(r["PayModeName"])
                                : (r.Table.Columns.Contains("Type") && r["Type"] != DBNull.Value
                                    ? Convert.ToString(r["Type"])
                                    : "Cash / Bank"));

                        string remarks = r.Table.Columns.Contains("Remarks") && r["Remarks"] != DBNull.Value
                            ? Convert.ToString(r["Remarks"])
                            : (r.Table.Columns.Contains("Narration") && r["Narration"] != DBNull.Value
                                ? Convert.ToString(r["Narration"])
                                : "");

                        list.Add(new CustomerBillReceiptDetail
                        {
                            BillNo = billNo,
                            ReceiptDate = rcptDate,
                            VoucherNo = voucherNo,
                            PaymentMode = payMode,
                            BillAmount = billAmt,
                            ReceiptAmount = rcptAmt,
                            BalanceAmount = balAmt,
                            Remarks = remarks
                        });

                        runningBillAmt = balAmt;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error querying SP customer receipt: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            // 3. Check for Credit Notes applied to this invoice via Stored Procedure STOREDPROCEDURE._CreditNoteMaster (Operation: GETBYBILLNO)
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CreditNoteMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BillNo", billNo.ToString());
                    if (customerLedgerId > 0)
                        cmd.Parameters.AddWithValue("@CustomerLedgerId", customerLedgerId);
                    cmd.Parameters.AddWithValue("@BranchId", effectiveBranchId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYBILLNO");

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dtCn = new DataTable();
                        da.Fill(dtCn);

                        foreach (DataRow r in dtCn.Rows)
                        {
                            decimal credAmt = r.Table.Columns.Contains("CreditAmount") && r["CreditAmount"] != DBNull.Value
                                ? Convert.ToDecimal(r["CreditAmount"])
                                : (r.Table.Columns.Contains("ReturnedAmount") && r["ReturnedAmount"] != DBNull.Value
                                    ? Convert.ToDecimal(r["ReturnedAmount"])
                                    : 0m);

                            if (credAmt <= 0) continue;

                            DateTime payDate = r.Table.Columns.Contains("BillDate") && r["BillDate"] != DBNull.Value
                                ? Convert.ToDateTime(r["BillDate"])
                                : DateTime.Today;

                            list.Add(new CustomerBillReceiptDetail
                            {
                                BillNo = billNo,
                                ReceiptDate = payDate,
                                VoucherNo = "CREDIT-NOTE",
                                PaymentMode = "Credit Note / SR",
                                BillAmount = r.Table.Columns.Contains("InvoiceAmount") && r["InvoiceAmount"] != DBNull.Value
                                    ? Convert.ToDecimal(r["InvoiceAmount"])
                                    : invoiceAmt,
                                ReceiptAmount = credAmt,
                                BalanceAmount = r.Table.Columns.Contains("Balance") && r["Balance"] != DBNull.Value
                                    ? Convert.ToDecimal(r["Balance"])
                                    : balance,
                                Remarks = "Credit Note Adjustment"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error querying SP _CreditNoteMaster: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return list.OrderBy(p => p.ReceiptDate).ToList();
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

        public List<CategoryDDL> GetCategories()
        {
            try
            {
                CategoryDDlGrid data = new Dropdowns().getCategoryDDl(string.Empty);
                if (data == null || data.List == null)
                    return new List<CategoryDDL>();

                return data.List.Where(x => x != null && x.Id > 0).OrderBy(x => x.CategoryName).ToList();
            }
            catch
            {
                return new List<CategoryDDL>();
            }
        }

        public List<UsersDDl> GetSalesPersons()
        {
            try
            {
                UserDDlGrid data = new Dropdowns().getUsersDDl();
                if (data == null || data.List == null)
                    return new List<UsersDDl>();

                return data.List.Where(x => x != null && x.UserID > 0).OrderBy(x => x.UserName).ToList();
            }
            catch
            {
                return new List<UsersDDl>();
            }
        }

        private static long ToLong(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToInt64(row[columnName])
                : 0;
        }

        private static int ToInt(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToInt32(row[columnName])
                : 0;
        }

        private static int? ToNullableInt(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return null;

            int val;
            return int.TryParse(row[columnName].ToString(), out val) ? (int?)val : null;
        }

        private static decimal ToDecimal(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToDecimal(row[columnName])
                : 0m;
        }

        private static DateTime? ToNullableDateTime(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? (DateTime?)Convert.ToDateTime(row[columnName])
                : null;
        }

        private static string ToString(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToString(row[columnName])
                : string.Empty;
        }
    }
}
