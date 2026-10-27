using ModelClass;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Repository.ReportRepository
{
    public class VendorOutstandingReportRepository : BaseRepostitory
    {
        public List<VendorOutstandingReportRow> GetReport(VendorOutstandingReportFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            List<VendorOutstandingReportRow> rows = new List<VendorOutstandingReportRow>();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_VendorOutstandingListing, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    int companyId = filter.CompanyId > 0 ? filter.CompanyId : GetContextValue(SessionContext.CompanyId, DataBase.CompanyId);
                    int branchId = filter.BranchId > 0 ? filter.BranchId : GetContextValue(SessionContext.BranchId, DataBase.BranchId);
                    int finYearId = filter.FinYearId > 0 ? filter.FinYearId : GetContextValue(SessionContext.FinYearId, DataBase.FinyearId);

                    cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = companyId > 0 ? (object)companyId : DBNull.Value;
                    cmd.Parameters.Add("@BranchId", SqlDbType.Int).Value = branchId > 0 ? (object)branchId : DBNull.Value;
                    cmd.Parameters.Add("@FinYearId", SqlDbType.Int).Value = finYearId > 0 ? (object)finYearId : DBNull.Value;
                    cmd.Parameters.Add("@LedgerId", SqlDbType.Int).Value = filter.LedgerId > 0 ? (object)filter.LedgerId : DBNull.Value;
                    cmd.Parameters.Add("@FromLedgerId", SqlDbType.Int).Value = filter.FromLedgerId > 0 ? (object)filter.FromLedgerId : DBNull.Value;
                    cmd.Parameters.Add("@ToLedgerId", SqlDbType.Int).Value = filter.ToLedgerId > 0 ? (object)filter.ToLedgerId : DBNull.Value;
                    cmd.Parameters.Add("@DateFilterMode", SqlDbType.VarChar, 20).Value = string.IsNullOrWhiteSpace(filter.DateFilterMode) ? (object)DBNull.Value : filter.DateFilterMode;
                    cmd.Parameters.Add("@UseDateFilter", SqlDbType.Bit).Value = filter.UseDateFilter;
                    cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = filter.FromDate.Date;
                    cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = filter.ToDate.Date;
                    cmd.Parameters.Add("@PaymentDueOnly", SqlDbType.Bit).Value = filter.PaymentDueOnly;
                    cmd.Parameters.Add("@GetUnallocatedReturnsOnly", SqlDbType.Bit).Value = filter.GetUnallocatedReturnsOnly;

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);

                        foreach (DataRow row in table.Rows)
                        {
                            decimal docAmt = ToDecimal(row, "DocAmt");
                            decimal balance = ToDecimal(row, "Balance");
                            decimal paidAmt = row.Table.Columns.Contains("PaidAmount") && row["PaidAmount"] != DBNull.Value
                                ? Convert.ToDecimal(row["PaidAmount"])
                                : (row.Table.Columns.Contains("PayedAmount") && row["PayedAmount"] != DBNull.Value
                                    ? Convert.ToDecimal(row["PayedAmount"])
                                    : Math.Max(0m, docAmt - balance));

                            rows.Add(new VendorOutstandingReportRow
                            {
                                AcctCode = ToInt(row, "AcctCode"),
                                Company = ToString(row, "Company"),
                                Name = ToString(row, "Name"),
                                Phone = ToString(row, "Phone"),
                                PurchaseNo = ToLong(row, "PurchaseNo"),
                                Date = ToDateTime(row, "Date"),
                                Reference = ToString(row, "Reference"),
                                InvoiceDate = ToNullableDateTime(row, "InvoiceDate"),
                                PostDate = ToNullableDateTime(row, "PostDate"),
                                DocAmt = docAmt,
                                PaidAmount = paidAmt,
                                Balance = balance,
                                IsPR = ToInt(row, "IsPR")
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

        public List<VendorGridList> GetVendors()
        {
            VendorRepository vendorRepository = new VendorRepository();
            VendorDDLGrid data = vendorRepository.GetVendorDDL();

            if (data == null || data.List == null)
                return new List<VendorGridList>();

            return data.List
                .Where(x => x != null && x.LedgerID > 0)
                .OrderBy(x => x.LedgerName)
                .ToList();
        }

        public List<VendorBillPaymentDetail> GetBillPaymentHistory(int vendorLedgerId, long purchaseNo, int branchId, decimal docAmt, decimal balance)
        {
            List<VendorBillPaymentDetail> list = new List<VendorBillPaymentDetail>();

            int effectiveBranchId = branchId > 0 ? branchId : GetContextValue(SessionContext.BranchId, DataBase.BranchId);

            // 1. Fetch Payment History using Stored Procedure STOREDPROCEDURE._VendorPaymentMaster (Operation: VIEWPAYMENT)
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                DataTable dt = new DataTable();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentMaster, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
                        cmd.Parameters.AddWithValue("@BillNoUntil", purchaseNo);
                        cmd.Parameters.AddWithValue("@BranchId", effectiveBranchId);
                        cmd.Parameters.AddWithValue("@_Operation", "VIEWPAYMENT");

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calling SP _VendorPaymentMaster VIEWPAYMENT: {ex.Message}");
                }

                // 2. If no records returned, fetch via Stored Procedure STOREDPROCEDURE._VendorPaymentDetails (Operation: GETBYBILLNO)
                if (dt == null || dt.Rows.Count == 0)
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentDetails, (SqlConnection)DataConnection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
                            cmd.Parameters.AddWithValue("@BillNo", purchaseNo);
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
                        System.Diagnostics.Debug.WriteLine($"Error calling SP _VendorPaymentDetails GETBYBILLNO: {ex.Message}");
                    }
                }

                if (dt != null && dt.Rows.Count > 0)
                {
                    decimal runningBillAmt = docAmt > 0 ? docAmt : 0m;
                    decimal runningBal = docAmt;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow r = dt.Rows[i];
                        decimal payAmt = r.Table.Columns.Contains("PaymentAmount") && r["PaymentAmount"] != DBNull.Value
                            ? Convert.ToDecimal(r["PaymentAmount"])
                            : (r.Table.Columns.Contains("PayedAmount") && r["PayedAmount"] != DBNull.Value
                                ? Convert.ToDecimal(r["PayedAmount"])
                                : 0m);

                        if (payAmt <= 0) continue;

                        decimal billAmt = r.Table.Columns.Contains("BillAmount") && r["BillAmount"] != DBNull.Value
                            ? Convert.ToDecimal(r["BillAmount"])
                            : (r.Table.Columns.Contains("InvoiceAmount") && r["InvoiceAmount"] != DBNull.Value
                                ? Convert.ToDecimal(r["InvoiceAmount"])
                                : runningBillAmt);

                        if (i == 0 && runningBillAmt == 0) runningBillAmt = billAmt;
                        runningBal = Math.Max(0m, runningBal - payAmt);

                        decimal balAmt = r.Table.Columns.Contains("BalanceAmount") && r["BalanceAmount"] != DBNull.Value
                            ? Convert.ToDecimal(r["BalanceAmount"])
                            : runningBal;

                        DateTime payDate = r.Table.Columns.Contains("PaymentDate") && r["PaymentDate"] != DBNull.Value
                            ? Convert.ToDateTime(r["PaymentDate"])
                            : (r.Table.Columns.Contains("BillDate") && r["BillDate"] != DBNull.Value
                                ? Convert.ToDateTime(r["BillDate"])
                                : (r.Table.Columns.Contains("VoucherDate") && r["VoucherDate"] != DBNull.Value
                                    ? Convert.ToDateTime(r["VoucherDate"])
                                    : DateTime.Today));

                        string voucherNo = r.Table.Columns.Contains("VoucherNo") && r["VoucherNo"] != DBNull.Value
                            ? Convert.ToString(r["VoucherNo"])
                            : (r.Table.Columns.Contains("VoucherId") && r["VoucherId"] != DBNull.Value
                                ? "VCH-" + Convert.ToString(r["VoucherId"])
                                : (r.Table.Columns.Contains("PaymentMasterId") && r["PaymentMasterId"] != DBNull.Value
                                    ? "VP-" + Convert.ToString(r["PaymentMasterId"])
                                    : "PAYMENT"));

                        string payMode = r.Table.Columns.Contains("Type") && r["Type"] != DBNull.Value
                            ? Convert.ToString(r["Type"])
                            : (r.Table.Columns.Contains("PaymentMode") && r["PaymentMode"] != DBNull.Value
                                ? Convert.ToString(r["PaymentMode"])
                                : (r.Table.Columns.Contains("PayModeName") && r["PayModeName"] != DBNull.Value
                                    ? Convert.ToString(r["PayModeName"])
                                    : "Cash / Bank"));

                        string remarks = r.Table.Columns.Contains("Remarks") && r["Remarks"] != DBNull.Value
                            ? Convert.ToString(r["Remarks"])
                            : (r.Table.Columns.Contains("Narration") && r["Narration"] != DBNull.Value
                                ? Convert.ToString(r["Narration"])
                                : "");

                        list.Add(new VendorBillPaymentDetail
                        {
                            PurchaseNo = purchaseNo,
                            PaymentDate = payDate,
                            VoucherNo = voucherNo,
                            PaymentMode = payMode,
                            BillAmount = billAmt,
                            PaymentAmount = payAmt,
                            BalanceAmount = balAmt,
                            Remarks = remarks
                        });

                        runningBillAmt = balAmt;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error querying SP vendor payment: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            // 3. Check for Debit Notes applied to this invoice via Stored Procedure STOREDPROCEDURE._DebitNoteMaster (Operation: GETBYPURCHASENO)
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._DebitNoteMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@InvoiceNo", purchaseNo.ToString());
                    cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
                    cmd.Parameters.AddWithValue("@BranchId", effectiveBranchId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYPURCHASENO");

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dtDn = new DataTable();
                        da.Fill(dtDn);

                        foreach (DataRow r in dtDn.Rows)
                        {
                            decimal retAmt = r.Table.Columns.Contains("ReturnedAmount") && r["ReturnedAmount"] != DBNull.Value
                                ? Convert.ToDecimal(r["ReturnedAmount"])
                                : (r.Table.Columns.Contains("DebitAmount") && r["DebitAmount"] != DBNull.Value
                                    ? Convert.ToDecimal(r["DebitAmount"])
                                    : 0m);

                            if (retAmt <= 0) continue;

                            DateTime payDate = r.Table.Columns.Contains("BillDate") && r["BillDate"] != DBNull.Value
                                ? Convert.ToDateTime(r["BillDate"])
                                : DateTime.Today;

                            list.Add(new VendorBillPaymentDetail
                            {
                                PurchaseNo = purchaseNo,
                                PaymentDate = payDate,
                                VoucherNo = "DEBIT-NOTE",
                                PaymentMode = "Debit Note / PR",
                                BillAmount = r.Table.Columns.Contains("InvoiceAmount") && r["InvoiceAmount"] != DBNull.Value
                                    ? Convert.ToDecimal(r["InvoiceAmount"])
                                    : docAmt,
                                PaymentAmount = retAmt,
                                BalanceAmount = r.Table.Columns.Contains("Balance") && r["Balance"] != DBNull.Value
                                    ? Convert.ToDecimal(r["Balance"])
                                    : balance,
                                Remarks = "Debit Note Adjustment"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error querying SP _DebitNoteMaster: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return list.OrderBy(p => p.PaymentDate).ToList();
        }

        private static int ToInt(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToInt32(row[columnName])
                : 0;
        }

        private static long ToLong(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToInt64(row[columnName])
                : 0L;
        }

        private static decimal ToDecimal(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToDecimal(row[columnName])
                : 0m;
        }

        private static DateTime ToDateTime(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToDateTime(row[columnName])
                : DateTime.MinValue;
        }

        private static DateTime? ToNullableDateTime(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToDateTime(row[columnName])
                : (DateTime?)null;
        }

        private static string ToString(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToString(row[columnName])
                : string.Empty;
        }
    }
}
