using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using ModelClass.Accounts;
using ModelClass;


namespace Repository.Accounts
{
    public class VendorPaymentRepository : BaseRepostitory
    {
        private const string VendorPaymentVoucherType = "VENDPAY";

        public class PurchasePaymentCancellationSummary
        {
            public int PaymentVoucherCount { get; set; }
            public decimal PaymentAmount { get; set; }
            public DateTime LastPaymentDate { get; set; }
        }

        public VendorPurchasedInfoGrid getPurchasedInfoForPayment(int LedgerId)
        {
            VendorPurchasedInfoGrid objVendorPurchasedInfo = new VendorPurchasedInfoGrid();
            DataConnection.Open();
            try
            {
                SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_VendorPyamentInfo, (SqlConnection)DataConnection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@LedgerId", LedgerId);

                SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapt.Fill(ds);
                if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                {
                    objVendorPurchasedInfo.ListPurchasedInfo = ds.Tables[0].ToListOfObject<VendorPurchasedInfo>();
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return objVendorPurchasedInfo;
        }

        public int SaveVendorPayment(VendorPaymentMaster master, List<VendorPaymentDetails> details, List<VoucherEntry> vouchers)
        {
            if (master == null)
            {
                throw new ArgumentNullException(nameof(master));
            }

            if (details == null || !details.Any())
            {
                throw new ArgumentException("At least one payment detail is required.", nameof(details));
            }

            if (vouchers == null || vouchers.Count < 2)
            {
                throw new ArgumentException("Double-entry voucher rows are required.", nameof(vouchers));
            }

            ValidatePaymentAmounts(master, details, vouchers);

            DataConnection.Open();
            SqlTransaction transaction = null;

            try
            {
                transaction = ((SqlConnection)DataConnection).BeginTransaction();
                int paymentMasterId = 0;
                int companyId = master.CompanyId > 0 ? master.CompanyId : SessionContext.CompanyId;
                int branchId = master.BranchId > 0 ? master.BranchId : SessionContext.BranchId;
                int userId = master.CreatedBy > 0 ? master.CreatedBy : SessionContext.UserId;
                int paymentMethodId = GetPaymentMethodId(master);
                int paymentAccountLedgerId = ResolvePaymentAccountLedgerId(paymentMethodId, master.PaymentMethod, branchId);
                DateTime voucherDate = master.PaymentDate == default(DateTime)
                    ? DateTime.Now.Date
                    : master.PaymentDate.Date;

                int defaultFinYearId = SessionContext.FinYearId; // Default value or get from settings

                // 1. Generate Voucher Number
                int voucherId;
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, (SqlConnection)DataConnection, transaction))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchID", branchId);
                    cmd.Parameters.AddWithValue("@FinYearID", defaultFinYearId);
                    cmd.Parameters.AddWithValue("@VoucherType", VendorPaymentVoucherType);
                    cmd.Parameters.AddWithValue("@_Operation", "GENERATENUMBER");

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        voucherId = Convert.ToInt32(result);
                    }
                    else
                    {
                        throw new Exception("Failed to generate voucher number");
                    }
                }

                // 2. Save Payment Master
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentMaster, (SqlConnection)DataConnection, transaction))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@VoucherId", voucherId);
                    cmd.Parameters.AddWithValue("@VoucherDate", voucherDate);
                    cmd.Parameters.AddWithValue("@PaymentMethodLedgerId", paymentMethodId);
                    cmd.Parameters.AddWithValue("@VendorLedgerId", master.VendorLedgerId);
                    cmd.Parameters.AddWithValue("@PayableAmount", (float)master.TotalPaymentAmount);
                    cmd.Parameters.AddWithValue("@PaymentAmount", (float)master.TotalPaymentAmount);
                    cmd.Parameters.AddWithValue("@OldPaymentAmount", 0);
                    cmd.Parameters.AddWithValue("@Narration", master.Remarks ?? (object)DBNull.Value);

                    // Find the highest bill number among selected details
                    long highestBillNo = 0;
                    foreach (var detail in details)
                    {
                        if (detail.AdjustedAmount > 0 && !string.IsNullOrEmpty(detail.BillNo) && long.TryParse(detail.BillNo, out long billNo))
                        {
                            if (billNo > highestBillNo)
                            {
                                highestBillNo = billNo;
                            }
                        }
                    }
                    cmd.Parameters.AddWithValue("@BillNoUntil", highestBillNo);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var status = reader[0].ToString();
                            if (status == "SUCCESS")
                            {
                                paymentMasterId = Convert.ToInt32(reader[1]);
                            }
                            else
                            {
                                throw new Exception("Failed to create payment master");
                            }
                        }
                    }
                }

                // 3. Save Payment Details
                foreach (var detail in details)
                {
                    if (detail.AdjustedAmount > 0)
                    {
                        if (!int.TryParse(detail.BillNo, out int billNo))
                        {
                            throw new Exception($"Invalid BillNo format: {detail.BillNo}. BillNo must be a valid integer.");
                        }

                        using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentDetails, (SqlConnection)DataConnection, transaction))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@BranchId", branchId);
                            cmd.Parameters.AddWithValue("@VendorLedgerId", master.VendorLedgerId);
                            cmd.Parameters.AddWithValue("@CreditPaymodeId", paymentMethodId);
                            cmd.Parameters.AddWithValue("@PaymentMasterId", paymentMasterId);
                            cmd.Parameters.AddWithValue("@BillNo", billNo);
                            cmd.Parameters.AddWithValue("@BillDate", detail.BillDate != default(DateTime) ? detail.BillDate : voucherDate);
                            cmd.Parameters.AddWithValue("@BillAmount", (float)detail.InvoiceAmount);
                            cmd.Parameters.AddWithValue("@PayedAmount", 0); // Will be calculated in SP
                            cmd.Parameters.AddWithValue("@PaymentAmount", (float)detail.AdjustedAmount);
                            cmd.Parameters.AddWithValue("@BalanceAmount", (float)detail.Balance);
                            cmd.Parameters.AddWithValue("@OldBillAmount", (float)detail.InvoiceAmount);
                            cmd.Parameters.AddWithValue("@OldPaymentAmount", 0);
                            cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                            var result = cmd.ExecuteScalar();
                            if (result == null || !result.ToString().StartsWith("SUCCESS"))
                            {
                                throw new Exception($"Failed to save payment detail for bill {detail.BillNo}: {result}");
                            }
                        }
                    }
                }

                // 4. Create Voucher Entries (Double Entry System)
                int slNo = 1;
                foreach (VoucherEntry voucher in vouchers.Where(v => v != null && (v.DebitAmount > 0 || v.CreditAmount > 0)))
                {
                    int ledgerId = ResolveVoucherLedgerId(master, voucher, paymentMethodId, paymentAccountLedgerId);
                    if (ledgerId <= 0)
                    {
                        throw new Exception("Failed to resolve voucher ledger for vendor payment.");
                    }

                    DateTime currentVoucherDate = voucher.VoucherDate == default(DateTime)
                        ? voucherDate
                        : voucher.VoucherDate.Date;

                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, (SqlConnection)DataConnection, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CompanyID", companyId);
                        cmd.Parameters.AddWithValue("@BranchID", branchId);
                        cmd.Parameters.AddWithValue("@VoucherID", voucherId);
                        cmd.Parameters.AddWithValue("@VoucherSeriesID", 0);
                        cmd.Parameters.AddWithValue("@VoucherDate", currentVoucherDate);
                        cmd.Parameters.AddWithValue("@VoucherNumber", string.IsNullOrWhiteSpace(voucher.VoucherNo) ? master.VoucherNo ?? "" : voucher.VoucherNo);
                        cmd.Parameters.AddWithValue("@LedgerID", ledgerId);
                        cmd.Parameters.AddWithValue("@VoucherType", VendorPaymentVoucherType);
                        cmd.Parameters.AddWithValue("@Debit", (float)voucher.DebitAmount);
                        cmd.Parameters.AddWithValue("@Credit", (float)voucher.CreditAmount);
                        cmd.Parameters.AddWithValue("@Narration", string.IsNullOrWhiteSpace(voucher.Narration) ? $"Payment to {master.VendorName}" : voucher.Narration);
                        cmd.Parameters.AddWithValue("@SlNo", slNo++);
                        cmd.Parameters.AddWithValue("@Mode", "");
                        cmd.Parameters.AddWithValue("@ModeID", 0);
                        cmd.Parameters.AddWithValue("@UserDate", currentVoucherDate);
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@CancelFlag", false);
                        cmd.Parameters.AddWithValue("@FinYearID", defaultFinYearId);
                        cmd.Parameters.AddWithValue("@IsSyncd", false);
                        cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                        var result = cmd.ExecuteScalar();
                        if (result == null || !result.ToString().StartsWith("SUCCESS"))
                        {
                            throw new Exception("Failed to create vendor payment voucher entries.");
                        }
                    }
                }

                // 5. SyncQueue Integration - Enqueue Vendor Payment (via Stored Procedure POS_SyncQueue)
                try
                {
                    Guid paymentGuid = Guid.NewGuid();
                    SyncQueueRepository.SetTransactionGuid(
                        DataConnection,
                        transaction,
                        "VENDOR_PAYMENT",
                        paymentMasterId.ToString(),
                        paymentGuid);

                    SyncQueueRepository.EnqueueTransaction(
                        DataConnection,
                        transaction,
                        branchId > 0 ? branchId : SessionContext.BranchId,
                        "VENDOR_PAYMENT",
                        paymentMasterId.ToString(),
                        paymentGuid,
                        "CREATE");
                }
                catch (Exception syncEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[VendorPaymentRepository.SaveVendorPayment] SyncQueue error: {syncEx.Message}");
                }

                transaction.Commit();
                return paymentMasterId;
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        private int GetPaymentMethodId(VendorPaymentMaster master)
        {
            if (master.PaymentMethodLedgerId > 0)
            {
                return master.PaymentMethodLedgerId;
            }

            if (!string.IsNullOrWhiteSpace(master.PaymentMethod) && int.TryParse(master.PaymentMethod, out int paymentMethodId))
            {
                return paymentMethodId;
            }

            return 1;
        }

        private void ValidatePaymentAmounts(VendorPaymentMaster master, List<VendorPaymentDetails> details, List<VoucherEntry> vouchers)
        {
            if (master.TotalPaymentAmount <= 0)
            {
                throw new InvalidOperationException("Payment amount must be greater than zero.");
            }

            decimal detailTotal = Math.Round(details.Where(d => d != null).Sum(d => d.AdjustedAmount), 2);
            decimal paymentTotal = Math.Round(master.TotalPaymentAmount, 2);
            if (detailTotal != paymentTotal)
            {
                throw new InvalidOperationException("Payment detail total must equal payment amount.");
            }

            List<VoucherEntry> postingRows = vouchers
                .Where(v => v != null && (v.DebitAmount > 0 || v.CreditAmount > 0))
                .ToList();

            if (postingRows.Count < 2)
            {
                throw new InvalidOperationException("Payment voucher must contain at least two posting rows.");
            }

            foreach (VoucherEntry row in postingRows)
            {
                if (row.DebitAmount < 0 || row.CreditAmount < 0)
                {
                    throw new InvalidOperationException("Payment voucher debit and credit cannot be negative.");
                }

                if (row.DebitAmount > 0 && row.CreditAmount > 0)
                {
                    throw new InvalidOperationException("A payment voucher row cannot contain both debit and credit.");
                }
            }

            decimal totalDebit = Math.Round(postingRows.Sum(v => v.DebitAmount), 2);
            decimal totalCredit = Math.Round(postingRows.Sum(v => v.CreditAmount), 2);

            if (totalDebit != totalCredit)
            {
                throw new InvalidOperationException("Payment voucher is not balanced. Total debit must equal total credit.");
            }

            if (totalDebit != paymentTotal)
            {
                throw new InvalidOperationException("Payment voucher total must equal payment amount.");
            }
        }

        private int ResolveVoucherLedgerId(VendorPaymentMaster master, VoucherEntry voucher, int paymentMethodId, int paymentAccountLedgerId)
        {
            if (voucher.DebitAmount > 0)
            {
                return master.VendorLedgerId;
            }

            if (voucher.CreditAmount > 0)
            {
                if (voucher.LedgerId > 0 && voucher.LedgerId != paymentMethodId)
                {
                    return voucher.LedgerId;
                }

                return paymentAccountLedgerId;
            }

            return voucher.LedgerId;
        }

        private int ResolvePaymentAccountLedgerId(int paymentMethodId, string paymentMethod, int branchId)
        {
            try
            {
                // Resolve the actual Ledger ID for the selected payment mode from the PayMode SP or LedgerRepository
                if (paymentMethodId > 0)
                {
                    try
                    {
                        using (SqlCommand paymodeCmd = new SqlCommand(STOREDPROCEDURE.POS_PayMode, (SqlConnection)DataConnection))
                        {
                            paymodeCmd.CommandType = CommandType.StoredProcedure;
                            paymodeCmd.Parameters.AddWithValue("@PaymodeId", paymentMethodId);
                            paymodeCmd.Parameters.AddWithValue("@_Operation", "GETBYID");
                            using (SqlDataAdapter da = new SqlDataAdapter(paymodeCmd))
                            {
                                DataTable dtPaymode = new DataTable();
                                da.Fill(dtPaymode);
                                if (dtPaymode != null && dtPaymode.Rows.Count > 0 && dtPaymode.Columns.Contains("LedgerID") && dtPaymode.Rows[0]["LedgerID"] != DBNull.Value)
                                {
                                    int ledgerId = Convert.ToInt32(dtPaymode.Rows[0]["LedgerID"]);
                                    if (ledgerId > 0)
                                        return ledgerId;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error fetching LedgerID from PayMode SP for vendor payment: {ex.Message}");
                    }
                }

                var ledgerRepository = new Repository.MasterRepositry.LedgerRepository();
                string paymentMethodName = paymentMethod ?? string.Empty;
                string normalizedPaymentMethod = paymentMethodName.Replace(" ", string.Empty).Trim();

                if (!string.IsNullOrWhiteSpace(normalizedPaymentMethod) &&
                    !normalizedPaymentMethod.Equals("Cash", StringComparison.OrdinalIgnoreCase))
                {
                    string[] bankCandidates = new[]
                    {
                        paymentMethodName,
                        normalizedPaymentMethod,
                        normalizedPaymentMethod.Equals("Transfer", StringComparison.OrdinalIgnoreCase) ? "BankTransfer" : null,
                        normalizedPaymentMethod.Equals("Cheque", StringComparison.OrdinalIgnoreCase) ? "Cheque" : null,
                        normalizedPaymentMethod.Equals("Card", StringComparison.OrdinalIgnoreCase) ? "Card" : null,
                        normalizedPaymentMethod.Equals("UPI", StringComparison.OrdinalIgnoreCase) ? "UPI" : null
                    }
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                    foreach (string candidate in bankCandidates)
                    {
                        int bankLedgerId = ledgerRepository.GetLedgerId(candidate, (int)ModelClass.AccountGroup.BANK_ACCOUNTS, branchId);
                        if (bankLedgerId > 0)
                        {
                            return bankLedgerId;
                        }
                    }
                }

                int cashLedgerId = ledgerRepository.GetLedgerId(ModelClass.DefaultLedgers.CASH, (int)ModelClass.AccountGroup.CASH_IN_HAND, branchId);
                if (cashLedgerId > 0)
                {
                    return cashLedgerId;
                }

                return GetCashLedgerIdFromDatabase(branchId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting payment account ledger ID for paymode {paymentMethodId}: {ex.Message}");
                return GetCashLedgerIdFromDatabase(branchId);
            }
        }

        /// <summary>
        /// Fallback method to get cash ledger ID using stored procedure
        /// </summary>
        private int GetCashLedgerIdFromDatabase(int branchId)
        {
            try
            {
                // Use the same stored procedure pattern as CustomerReceiptInfoRepository
                DataConnection.Open();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._4GetLedgerIdByLedgerNameAndGroupId, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@LedgerName", ModelClass.DefaultLedgers.CASH);
                        cmd.Parameters.AddWithValue("@GroupId", (int)ModelClass.AccountGroup.CASH_IN_HAND);
                        cmd.Parameters.AddWithValue("@BranchId", branchId);

                        using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                        {
                            DataSet ds = new DataSet();
                            adapt.Fill(ds);
                            if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                            {
                                int ledgerId = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                                System.Diagnostics.Debug.WriteLine($"Found cash ledger ID via stored procedure: {ledgerId}");
                                return ledgerId;
                            }
                        }
                    }
                }
                finally
                {
                    if (DataConnection.State == ConnectionState.Open)
                        DataConnection.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetCashLedgerIdFromDatabase: {ex.Message}");
            }

            // Ultimate fallback - return 1 (should be updated based on your system)
            System.Diagnostics.Debug.WriteLine("Using fallback cash ledger ID: 1");
            return 1;
        }

        public DataTable GetOutstandingInvoices(int vendorLedgerId)
        {
            DataTable dt = new DataTable();
            try
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();

                DataConnection.Open();
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETOUTSTANDING");

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calling SP _VendorPaymentMaster GETOUTSTANDING: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            // Fallback 1: Try with @LedgerId if first attempt returned 0 rows
            if (dt == null || dt.Rows.Count == 0)
            {
                try
                {
                    DataConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentMaster, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@LedgerId", vendorLedgerId);
                        cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                        cmd.Parameters.AddWithValue("@_Operation", "GETOUTSTANDING");

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calling SP _VendorPaymentMaster with @LedgerId: {ex.Message}");
                }
                finally
                {
                    if (DataConnection.State == ConnectionState.Open)
                        DataConnection.Close();
                }
            }

            // Fallback 2: Stored Procedure POS_VendorPyamentInfo
            if (dt == null || dt.Rows.Count == 0)
            {
                try
                {
                    DataConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_VendorPyamentInfo, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@LedgerId", vendorLedgerId);

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable spDt = new DataTable();
                            da.Fill(spDt);
                            if (spDt != null && spDt.Rows.Count > 0)
                            {
                                dt = new DataTable();
                                dt.Columns.Add("BillNo", typeof(string));
                                dt.Columns.Add("InvoiceNo", typeof(string));
                                dt.Columns.Add("InvoiceAmount", typeof(decimal));
                                dt.Columns.Add("PayedAmount", typeof(decimal));
                                dt.Columns.Add("ReturnedAmount", typeof(decimal));
                                dt.Columns.Add("Balance", typeof(decimal));
                                dt.Columns.Add("BillDate", typeof(DateTime));
                                dt.Columns.Add("Paymode", typeof(string));
                                dt.Columns.Add("PaymodeID", typeof(int));

                                foreach (DataRow r in spDt.Rows)
                                {
                                    DataRow newR = dt.NewRow();
                                    newR["BillNo"] = spDt.Columns.Contains("BillNo") ? r["BillNo"]?.ToString() : "0";
                                    newR["InvoiceNo"] = spDt.Columns.Contains("InvoiceNo") && r["InvoiceNo"] != DBNull.Value ? r["InvoiceNo"].ToString() : newR["BillNo"];
                                    decimal inv = spDt.Columns.Contains("InvoiceAmount") && r["InvoiceAmount"] != DBNull.Value ? Convert.ToDecimal(r["InvoiceAmount"]) : 0m;
                                    decimal bal = spDt.Columns.Contains("Balance") && r["Balance"] != DBNull.Value ? Convert.ToDecimal(r["Balance"]) : inv;
                                    decimal paid = spDt.Columns.Contains("PayedAmount") && r["PayedAmount"] != DBNull.Value ? Convert.ToDecimal(r["PayedAmount"]) :
                                                   spDt.Columns.Contains("PaidAmount") && r["PaidAmount"] != DBNull.Value ? Convert.ToDecimal(r["PaidAmount"]) :
                                                   Math.Max(0m, inv - bal);

                                    newR["InvoiceAmount"] = inv;
                                    newR["PayedAmount"] = paid;
                                    newR["ReturnedAmount"] = 0m;
                                    newR["Balance"] = bal;
                                    newR["BillDate"] = spDt.Columns.Contains("BillDate") && r["BillDate"] != DBNull.Value ? Convert.ToDateTime(r["BillDate"]) : DateTime.Now;
                                    newR["Paymode"] = "Credit";
                                    newR["PaymodeID"] = 1;
                                    dt.Rows.Add(newR);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calling SP POS_VendorPyamentInfo: {ex.Message}");
                }
                finally
                {
                    if (DataConnection.State == ConnectionState.Open)
                        DataConnection.Close();
                }
            }

            // Fallback 3: Stored Procedure POS_VendorOutstandingListing
            if (dt == null || dt.Rows.Count == 0)
            {
                try
                {
                    DataConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_VendorOutstandingListing, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = SessionContext.CompanyId > 0 ? (object)SessionContext.CompanyId : DBNull.Value;
                        cmd.Parameters.Add("@BranchId", SqlDbType.Int).Value = SessionContext.BranchId > 0 ? (object)SessionContext.BranchId : DBNull.Value;
                        cmd.Parameters.Add("@FinYearId", SqlDbType.Int).Value = SessionContext.FinYearId > 0 ? (object)SessionContext.FinYearId : DBNull.Value;
                        cmd.Parameters.Add("@LedgerId", SqlDbType.Int).Value = vendorLedgerId > 0 ? (object)vendorLedgerId : DBNull.Value;
                        cmd.Parameters.Add("@FromLedgerId", SqlDbType.Int).Value = DBNull.Value;
                        cmd.Parameters.Add("@ToLedgerId", SqlDbType.Int).Value = DBNull.Value;
                        cmd.Parameters.Add("@DateFilterMode", SqlDbType.VarChar, 20).Value = DBNull.Value;
                        cmd.Parameters.Add("@UseDateFilter", SqlDbType.Bit).Value = false;
                        cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = DateTime.Today;
                        cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = DateTime.Today;
                        cmd.Parameters.Add("@PaymentDueOnly", SqlDbType.Bit).Value = false;
                        cmd.Parameters.Add("@GetUnallocatedReturnsOnly", SqlDbType.Bit).Value = false;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable spDt = new DataTable();
                            da.Fill(spDt);
                            if (spDt != null && spDt.Rows.Count > 0)
                            {
                                dt = new DataTable();
                                dt.Columns.Add("BillNo", typeof(string));
                                dt.Columns.Add("InvoiceNo", typeof(string));
                                dt.Columns.Add("InvoiceAmount", typeof(decimal));
                                dt.Columns.Add("PayedAmount", typeof(decimal));
                                dt.Columns.Add("ReturnedAmount", typeof(decimal));
                                dt.Columns.Add("Balance", typeof(decimal));
                                dt.Columns.Add("BillDate", typeof(DateTime));
                                dt.Columns.Add("Paymode", typeof(string));
                                dt.Columns.Add("PaymodeID", typeof(int));

                                foreach (DataRow r in spDt.Rows)
                                {
                                    DataRow newR = dt.NewRow();
                                    newR["BillNo"] = spDt.Columns.Contains("PurchaseNo") ? r["PurchaseNo"]?.ToString() : "0";
                                    newR["InvoiceNo"] = spDt.Columns.Contains("Reference") && r["Reference"] != DBNull.Value && !string.IsNullOrEmpty(r["Reference"].ToString()) 
                                                        ? r["Reference"].ToString() 
                                                        : newR["BillNo"];
                                    decimal docAmt = spDt.Columns.Contains("DocAmt") && r["DocAmt"] != DBNull.Value ? Convert.ToDecimal(r["DocAmt"]) : 0m;
                                    decimal balAmt = spDt.Columns.Contains("Balance") && r["Balance"] != DBNull.Value ? Convert.ToDecimal(r["Balance"]) : docAmt;
                                    decimal paidAmt = spDt.Columns.Contains("PaidAmount") && r["PaidAmount"] != DBNull.Value ? Convert.ToDecimal(r["PaidAmount"]) :
                                                      spDt.Columns.Contains("PayedAmount") && r["PayedAmount"] != DBNull.Value ? Convert.ToDecimal(r["PayedAmount"]) :
                                                      Math.Max(0m, docAmt - balAmt);

                                    newR["InvoiceAmount"] = docAmt;
                                    newR["PayedAmount"] = paidAmt;
                                    newR["ReturnedAmount"] = 0m;
                                    newR["Balance"] = balAmt;
                                    newR["BillDate"] = spDt.Columns.Contains("Date") && r["Date"] != DBNull.Value ? Convert.ToDateTime(r["Date"]) : DateTime.Now;
                                    newR["Paymode"] = "Credit";
                                    newR["PaymodeID"] = 1;
                                    dt.Rows.Add(newR);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calling SP POS_VendorOutstandingListing: {ex.Message}");
                }
                finally
                {
                    if (DataConnection.State == ConnectionState.Open)
                        DataConnection.Close();
                }
            }

            SanitizeInvoiceTable(dt);
            EnhanceInvoiceTableWithCashPaymode(dt);
            NormalizeInvoiceBalances(dt);

            // Filter out invoices with Balance <= 0 for outstanding invoices
            if (dt != null && dt.Columns.Contains("Balance"))
            {
                var rows = dt.AsEnumerable()
                    .Where(row => {
                        var val = row["Balance"];
                        decimal balance = 0;
                        if (val != DBNull.Value && val != null)
                        {
                            decimal.TryParse(val.ToString(), out balance);
                        }
                        return balance > 0;
                    })
                    .ToList();

                if (rows.Count > 0)
                    dt = rows.CopyToDataTable();
                else
                    dt = dt.Clone();
            }

            return dt;
        }

        public DataTable GetAllInvoices(int vendorLedgerId)
        {
            DataTable dt = new DataTable();
            try
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();

                DataConnection.Open();
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALLINVOICES");

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calling SP _VendorPaymentMaster GETALLINVOICES: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            // Fallback 1: Try with @LedgerId
            if (dt == null || dt.Rows.Count == 0)
            {
                try
                {
                    DataConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentMaster, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@LedgerId", vendorLedgerId);
                        cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                        cmd.Parameters.AddWithValue("@_Operation", "GETALLINVOICES");

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calling SP _VendorPaymentMaster GETALLINVOICES with @LedgerId: {ex.Message}");
                }
                finally
                {
                    if (DataConnection.State == ConnectionState.Open)
                        DataConnection.Close();
                }
            }

            // Fallback 2: Fallback Stored Procedure: POS_Purchase GETALL
            if (dt == null || dt.Rows.Count == 0)
            {
                try
                {
                    DataConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Purchase, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                        cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                        cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                        cmd.Parameters.AddWithValue("@_Operation", "GETALL");

                        DataTable purDt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(purDt);
                        }

                        if (purDt != null && purDt.Rows.Count > 0)
                        {
                            string ledgerCol = purDt.Columns.Contains("LedgerID") ? "LedgerID" :
                                               purDt.Columns.Contains("LedgerId") ? "LedgerId" :
                                               purDt.Columns.Contains("VendorLedgerId") ? "VendorLedgerId" :
                                               purDt.Columns.Contains("VendorLedgerID") ? "VendorLedgerID" :
                                               purDt.Columns.Contains("VendorID") ? "VendorID" :
                                               purDt.Columns.Contains("VendorId") ? "VendorId" : null;

                            if (ledgerCol != null)
                            {
                                var vendorRows = purDt.AsEnumerable()
                                    .Where(r => r[ledgerCol] != DBNull.Value && Convert.ToInt32(r[ledgerCol]) == vendorLedgerId);

                                if (vendorRows.Any())
                                {
                                    DataTable filteredPur = vendorRows.CopyToDataTable();
                                    dt = MapPurchaseTableToInvoiceTable(filteredPur);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calling SP POS_Purchase GETALL: {ex.Message}");
                }
                finally
                {
                    if (DataConnection.State == ConnectionState.Open)
                        DataConnection.Close();
                }
            }

            // Fallback 3: Outstanding invoices SP
            if (dt == null || dt.Rows.Count == 0)
            {
                dt = GetOutstandingInvoices(vendorLedgerId);
            }

            SanitizeInvoiceTable(dt);
            EnhanceInvoiceTableWithCashPaymode(dt);
            NormalizeInvoiceBalances(dt);
            return dt;
        }

        private static decimal GetRowDecimal(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value || row[columnName] == null)
            {
                return 0m;
            }
            if (decimal.TryParse(row[columnName].ToString(), out decimal result))
            {
                return result;
            }
            return 0m;
        }

        private void NormalizeInvoiceBalances(DataTable invoices)
        {
            if (invoices == null)
            {
                return;
            }

            foreach (DataRow row in invoices.Rows)
            {
                decimal invoiceAmount = GetRowDecimal(row, "InvoiceAmount");
                if (invoiceAmount == 0m)
                    invoiceAmount = GetRowDecimal(row, "GrandTotal");
                if (invoiceAmount == 0m)
                    invoiceAmount = GetRowDecimal(row, "DocAmt");
                if (invoiceAmount == 0m)
                    invoiceAmount = GetRowDecimal(row, "TotalAmount");

                decimal payedAmount = GetRowDecimal(row, "PayedAmount");
                if (payedAmount == 0m)
                    payedAmount = GetRowDecimal(row, "PaidAmount");
                if (payedAmount == 0m)
                    payedAmount = GetRowDecimal(row, "PaymentAmount");
                if (payedAmount == 0m)
                    payedAmount = GetRowDecimal(row, "ReceivedAmount");

                decimal returnedAmount = invoices.Columns.Contains("ReturnedAmount") ? GetRowDecimal(row, "ReturnedAmount") : 0m;

                decimal balance = invoices.Columns.Contains("Balance") && row["Balance"] != DBNull.Value ? GetRowDecimal(row, "Balance") : (invoiceAmount - payedAmount - returnedAmount);

                if (payedAmount == 0m && balance > 0m && balance < invoiceAmount && (invoiceAmount - balance - returnedAmount) > 0m)
                {
                    payedAmount = invoiceAmount - balance - returnedAmount;
                }

                // Detect cash purchases where full payment was settled at purchase time (Cash = PaymodeID 2)
                bool isCashPurchase = false;
                if (invoices.Columns.Contains("Paymode") && row["Paymode"] != DBNull.Value)
                {
                    string pm = row["Paymode"].ToString().Trim().ToLower();
                    if (pm == "cash" || pm == "2") isCashPurchase = true;
                }
                if (invoices.Columns.Contains("PaymodeID") && row["PaymodeID"] != DBNull.Value)
                {
                    if (Convert.ToInt32(row["PaymodeID"]) == 2) isCashPurchase = true;
                }
                if (invoices.Columns.Contains("PayModeID") && row["PayModeID"] != DBNull.Value)
                {
                    if (Convert.ToInt32(row["PayModeID"]) == 2) isCashPurchase = true;
                }

                // Credit purchases (PaymodeID = 1 / "credit") must NEVER be treated as cash purchases
                if (invoices.Columns.Contains("Paymode") && row["Paymode"] != DBNull.Value)
                {
                    string pm = row["Paymode"].ToString().Trim().ToLower();
                    if (pm == "credit" || pm == "1") isCashPurchase = false;
                }
                if (invoices.Columns.Contains("PaymodeID") && row["PaymodeID"] != DBNull.Value)
                {
                    if (Convert.ToInt32(row["PaymodeID"]) == 1) isCashPurchase = false;
                }
                if (invoices.Columns.Contains("PayModeID") && row["PayModeID"] != DBNull.Value)
                {
                    if (Convert.ToInt32(row["PayModeID"]) == 1) isCashPurchase = false;
                }

                if (isCashPurchase)
                {
                    payedAmount = invoiceAmount;
                }

                if (invoiceAmount < 0m)
                {
                    invoiceAmount = 0m;
                }

                if (payedAmount < 0m)
                {
                    payedAmount = 0m;
                }

                if (returnedAmount < 0m)
                {
                    returnedAmount = 0m;
                }

                if (invoiceAmount > 0m && payedAmount > invoiceAmount)
                {
                    payedAmount = invoiceAmount;
                }

                balance = invoiceAmount - payedAmount - returnedAmount;
                if (balance < 0m)
                {
                    balance = 0m;
                }

                if (invoices.Columns.Contains("InvoiceAmount"))
                {
                    row["InvoiceAmount"] = invoiceAmount;
                }

                if (invoices.Columns.Contains("PayedAmount"))
                {
                    row["PayedAmount"] = payedAmount;
                }

                if (invoices.Columns.Contains("ReturnedAmount"))
                {
                    row["ReturnedAmount"] = returnedAmount;
                }

                if (invoices.Columns.Contains("Balance"))
                {
                    row["Balance"] = balance;
                }
            }
        }

        private DataTable MapPurchaseTableToInvoiceTable(DataTable purDt)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("BillNo", typeof(string));
            dt.Columns.Add("InvoiceNo", typeof(string));
            dt.Columns.Add("InvoiceAmount", typeof(decimal));
            dt.Columns.Add("PayedAmount", typeof(decimal));
            dt.Columns.Add("ReturnedAmount", typeof(decimal));
            dt.Columns.Add("Balance", typeof(decimal));
            dt.Columns.Add("BillDate", typeof(DateTime));
            dt.Columns.Add("Paymode", typeof(string));
            dt.Columns.Add("PaymodeID", typeof(int));

            if (purDt != null && purDt.Rows.Count > 0)
            {
                foreach (DataRow r in purDt.Rows)
                {
                    DataRow newR = dt.NewRow();
                    string billNo = purDt.Columns.Contains("PurchaseNo") ? r["PurchaseNo"]?.ToString() :
                                   purDt.Columns.Contains("BillNo") ? r["BillNo"]?.ToString() : "0";
                    newR["BillNo"] = billNo;
                    newR["InvoiceNo"] = purDt.Columns.Contains("InvoiceNo") && r["InvoiceNo"] != DBNull.Value && !string.IsNullOrEmpty(r["InvoiceNo"].ToString()) 
                                        ? r["InvoiceNo"].ToString() 
                                        : billNo;

                    decimal grandTotal = purDt.Columns.Contains("GrandTotal") && r["GrandTotal"] != DBNull.Value ? Convert.ToDecimal(r["GrandTotal"]) :
                                         purDt.Columns.Contains("TotalAmount") && r["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(r["TotalAmount"]) : 0m;
                    decimal payedAmount = purDt.Columns.Contains("PayedAmount") && r["PayedAmount"] != DBNull.Value ? Convert.ToDecimal(r["PayedAmount"]) : 0m;
                    decimal returnedAmount = purDt.Columns.Contains("ReturnedAmount") && r["ReturnedAmount"] != DBNull.Value ? Convert.ToDecimal(r["ReturnedAmount"]) : 0m;

                    newR["InvoiceAmount"] = grandTotal;
                    newR["PayedAmount"] = payedAmount;
                    newR["ReturnedAmount"] = returnedAmount;
                    newR["Balance"] = grandTotal - payedAmount - returnedAmount;
                    newR["BillDate"] = purDt.Columns.Contains("PurchaseDate") && r["PurchaseDate"] != DBNull.Value ? Convert.ToDateTime(r["PurchaseDate"]) : DateTime.Now;
                    newR["Paymode"] = purDt.Columns.Contains("Paymode") ? r["Paymode"]?.ToString() : "";
                    newR["PaymodeID"] = purDt.Columns.Contains("PaymodeID") && r["PaymodeID"] != DBNull.Value ? Convert.ToInt32(r["PaymodeID"]) : 0;

                    dt.Rows.Add(newR);
                }
            }
            return dt;
        }

        private static void SanitizeInvoiceTable(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0) return;

            foreach (DataRow row in dt.Rows)
            {
                if (dt.Columns.Contains("BillNo") && (row["BillNo"] == DBNull.Value || row["BillNo"] == null))
                    row["BillNo"] = "0";

                if (dt.Columns.Contains("InvoiceNo") && (row["InvoiceNo"] == DBNull.Value || row["InvoiceNo"] == null))
                    row["InvoiceNo"] = row["BillNo"]?.ToString() ?? "";

                if (dt.Columns.Contains("InvoiceAmount") && (row["InvoiceAmount"] == DBNull.Value || row["InvoiceAmount"] == null))
                    row["InvoiceAmount"] = 0m;

                if (dt.Columns.Contains("PayedAmount") && (row["PayedAmount"] == DBNull.Value || row["PayedAmount"] == null))
                    row["PayedAmount"] = 0m;

                if (dt.Columns.Contains("ReturnedAmount") && (row["ReturnedAmount"] == DBNull.Value || row["ReturnedAmount"] == null))
                    row["ReturnedAmount"] = 0m;

                if (dt.Columns.Contains("Balance") && (row["Balance"] == DBNull.Value || row["Balance"] == null))
                {
                    decimal inv = decimal.TryParse(row["InvoiceAmount"]?.ToString(), out decimal iVal) ? iVal : 0m;
                    decimal paid = decimal.TryParse(row["PayedAmount"]?.ToString(), out decimal pVal) ? pVal : 0m;
                    decimal ret = dt.Columns.Contains("ReturnedAmount") && decimal.TryParse(row["ReturnedAmount"]?.ToString(), out decimal rVal) ? rVal : 0m;
                    row["Balance"] = inv - paid - ret;
                }

                if (dt.Columns.Contains("BillDate") && (row["BillDate"] == DBNull.Value || row["BillDate"] == null))
                    row["BillDate"] = DateTime.Now;
            }
        }

        private void EnhanceInvoiceTableWithCashPaymode(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0) return;

            if (!dt.Columns.Contains("Paymode"))
                dt.Columns.Add("Paymode", typeof(string));
            if (!dt.Columns.Contains("PaymodeID"))
                dt.Columns.Add("PaymodeID", typeof(int));
            if (!dt.Columns.Contains("PayedAmount"))
                dt.Columns.Add("PayedAmount", typeof(decimal));
            if (!dt.Columns.Contains("ReturnedAmount"))
                dt.Columns.Add("ReturnedAmount", typeof(decimal));

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Purchase, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");

                    DataTable pmDt = new DataTable();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(pmDt);
                    }

                    if (pmDt != null && pmDt.Rows.Count > 0)
                    {
                        string paymodeCol = pmDt.Columns.Contains("Paymode") ? "Paymode" :
                                           pmDt.Columns.Contains("PayMode") ? "PayMode" :
                                           pmDt.Columns.Contains("PaymodeName") ? "PaymodeName" : null;

                        string paymodeIdCol = pmDt.Columns.Contains("PaymodeID") ? "PaymodeID" :
                                             pmDt.Columns.Contains("PayModeId") ? "PayModeId" :
                                             pmDt.Columns.Contains("PayModeID") ? "PayModeID" : null;

                        var pmIdMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                        var pmNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        var pmPaidMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                        var pmRetMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

                        foreach (DataRow r in pmDt.Rows)
                        {
                            if (r.Table.Columns.Contains("PurchaseNo") && r["PurchaseNo"] != DBNull.Value)
                            {
                                string pNo = r["PurchaseNo"].ToString().Trim();
                                string pm = paymodeCol != null && r[paymodeCol] != DBNull.Value ? r[paymodeCol].ToString().Trim() : "";
                                int pmId = paymodeIdCol != null && r[paymodeIdCol] != DBNull.Value ? Convert.ToInt32(r[paymodeIdCol]) : 0;
                                decimal pPaid = r.Table.Columns.Contains("PayedAmount") && r["PayedAmount"] != DBNull.Value ? Convert.ToDecimal(r["PayedAmount"]) : 0m;
                                decimal pRet = r.Table.Columns.Contains("ReturnedAmount") && r["ReturnedAmount"] != DBNull.Value ? Convert.ToDecimal(r["ReturnedAmount"]) : 0m;

                                pmNameMap[pNo] = pm;
                                pmIdMap[pNo] = pmId;
                                pmPaidMap[pNo] = pPaid;
                                pmRetMap[pNo] = pRet;
                            }
                        }

                        foreach (DataRow row in dt.Rows)
                        {
                            string billNo = row["BillNo"]?.ToString()?.Trim();
                            if (!string.IsNullOrEmpty(billNo))
                            {
                                if (pmNameMap.TryGetValue(billNo, out string pmName))
                                    row["Paymode"] = pmName;
                                if (pmIdMap.TryGetValue(billNo, out int pmId))
                                    row["PaymodeID"] = pmId;

                                if (pmPaidMap.TryGetValue(billNo, out decimal pPaid) && pPaid > 0)
                                {
                                    decimal currentPaid = dt.Columns.Contains("PayedAmount") && row["PayedAmount"] != DBNull.Value ? Convert.ToDecimal(row["PayedAmount"]) : 0m;
                                    if (pPaid > currentPaid)
                                        row["PayedAmount"] = pPaid;
                                }

                                if (pmRetMap.TryGetValue(billNo, out decimal pRet) && pRet > 0)
                                {
                                    decimal currentRet = dt.Columns.Contains("ReturnedAmount") && row["ReturnedAmount"] != DBNull.Value ? Convert.ToDecimal(row["ReturnedAmount"]) : 0m;
                                    if (pRet > currentRet)
                                        row["ReturnedAmount"] = pRet;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error enhancing invoice table with paymode via stored procedure: {ex.Message}");
            }
        }

        public decimal GetVendorOutstandingTotal(int vendorLedgerId)
        {
            decimal outstandingTotal = 0;
            try
            {
                DataTable dtOut = GetOutstandingInvoices(vendorLedgerId);
                if (dtOut != null && dtOut.Rows.Count > 0 && dtOut.Columns.Contains("Balance"))
                {
                    foreach (DataRow row in dtOut.Rows)
                    {
                        if (row["Balance"] != DBNull.Value)
                        {
                            if (decimal.TryParse(row["Balance"].ToString(), out decimal bal))
                            {
                                outstandingTotal += bal;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting vendor outstanding total: {ex.Message}");
            }

            return outstandingTotal;
        }

        public DataSet GetPaymentDataByVoucherId(long voucherId, int branchId)
        {
            DataSet ds = new DataSet();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VoucherId", voucherId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(ds);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return ds;
        }

        public DataTable GetPaymentHistory(int vendorLedgerId, long billNo)
        {
            DataTable dt = new DataTable();
            DataConnection.Open();
            try
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentMaster, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
                        cmd.Parameters.AddWithValue("@BillNoUntil", billNo);
                        cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
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

                if (dt == null || dt.Rows.Count == 0)
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentDetails, (SqlConnection)DataConnection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
                            cmd.Parameters.AddWithValue("@BillNo", billNo);
                            cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
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

                return dt;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        public PurchasePaymentCancellationSummary GetActivePaymentSummaryForPurchase(int purchaseNo, int branchId, int vendorLedgerId)
        {
            DataConnection.Open();
            try
            {
                DataTable dt = new DataTable();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentDetails, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@BillNo", purchaseNo);
                        cmd.Parameters.AddWithValue("@BranchId", branchId);
                        cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
                        cmd.Parameters.AddWithValue("@_Operation", "GETSUMMARY");

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calling SP _VendorPaymentDetails GETSUMMARY: {ex.Message}");
                }

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    return new PurchasePaymentCancellationSummary
                    {
                        PaymentVoucherCount = dt.Columns.Contains("PaymentVoucherCount") && r["PaymentVoucherCount"] != DBNull.Value ? Convert.ToInt32(r["PaymentVoucherCount"]) : 0,
                        PaymentAmount = dt.Columns.Contains("PaymentAmount") && r["PaymentAmount"] != DBNull.Value ? Convert.ToDecimal(r["PaymentAmount"]) : 0m,
                        LastPaymentDate = dt.Columns.Contains("LastPaymentDate") && r["LastPaymentDate"] != DBNull.Value ? Convert.ToDateTime(r["LastPaymentDate"]) : DateTime.MinValue
                    };
                }

                return new PurchasePaymentCancellationSummary();
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        public PurchasePaymentCancellationSummary CancelVendorPayment(int paymentMasterId, int branchId, int userId, string reason)
        {
            DataConnection.Open();
            SqlTransaction transaction = null;

            try
            {
                transaction = ((SqlConnection)DataConnection).BeginTransaction();

                DataTable activeDetails = new DataTable();
                int voucherId = 0;
                DateTime voucherDate = DateTime.MinValue;

                // 1. Fetch master info via Stored Procedure STOREDPROCEDURE._VendorPaymentMaster
                try
                {
                    using (SqlCommand masterCmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentMaster, (SqlConnection)DataConnection, transaction))
                    {
                        masterCmd.CommandType = CommandType.StoredProcedure;
                        masterCmd.Parameters.AddWithValue("@VoucherId", paymentMasterId);
                        masterCmd.Parameters.AddWithValue("@BranchId", branchId);
                        masterCmd.Parameters.AddWithValue("@_Operation", "GETBYID");

                        using (SqlDataAdapter adapter = new SqlDataAdapter(masterCmd))
                        {
                            DataSet dsMaster = new DataSet();
                            adapter.Fill(dsMaster);
                            if (dsMaster != null && dsMaster.Tables.Count > 0 && dsMaster.Tables[0].Rows.Count > 0)
                            {
                                DataRow masterRow = dsMaster.Tables[0].Rows[0];
                                voucherId = masterRow.Table.Columns.Contains("VoucherId") && masterRow["VoucherId"] != DBNull.Value ? Convert.ToInt32(masterRow["VoucherId"]) : 0;
                                voucherDate = masterRow.Table.Columns.Contains("VoucherDate") && masterRow["VoucherDate"] != DBNull.Value ? Convert.ToDateTime(masterRow["VoucherDate"]) : DateTime.MinValue;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calling SP _VendorPaymentMaster GETBYID in cancel: {ex.Message}");
                }

                // 2. Fetch details via Stored Procedure STOREDPROCEDURE._VendorPaymentDetails
                try
                {
                    using (SqlCommand detailsCmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentDetails, (SqlConnection)DataConnection, transaction))
                    {
                        detailsCmd.CommandType = CommandType.StoredProcedure;
                        detailsCmd.Parameters.AddWithValue("@PaymentMasterId", paymentMasterId);
                        detailsCmd.Parameters.AddWithValue("@BranchId", branchId);
                        detailsCmd.Parameters.AddWithValue("@_Operation", "GETBYMASTERID");

                        using (SqlDataAdapter adapter = new SqlDataAdapter(detailsCmd))
                        {
                            adapter.Fill(activeDetails);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calling SP _VendorPaymentDetails GETBYMASTERID: {ex.Message}");
                }

                decimal totalReversed = 0m;
                if (activeDetails != null && activeDetails.Rows.Count > 0)
                {
                    foreach (DataRow detailRow in activeDetails.Rows)
                    {
                        decimal paymentAmount = detailRow.Table.Columns.Contains("PaymentAmount") && detailRow["PaymentAmount"] != DBNull.Value ? Convert.ToDecimal(detailRow["PaymentAmount"]) : 0m;
                        totalReversed += paymentAmount;
                    }
                }

                // 3. Cancel details using Stored Procedure STOREDPROCEDURE._VendorPaymentDetails
                try
                {
                    using (SqlCommand cancelDetailsSp = new SqlCommand(STOREDPROCEDURE._VendorPaymentDetails, (SqlConnection)DataConnection, transaction))
                    {
                        cancelDetailsSp.CommandType = CommandType.StoredProcedure;
                        cancelDetailsSp.Parameters.AddWithValue("@PaymentMasterId", paymentMasterId);
                        cancelDetailsSp.Parameters.AddWithValue("@_Operation", "CANCEL");
                        cancelDetailsSp.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calling SP _VendorPaymentDetails CANCEL: {ex.Message}");
                }

                // 4. Cancel Master using Stored Procedure STOREDPROCEDURE._VendorPaymentMaster
                string cancelNote = " | Cancelled";
                if (!string.IsNullOrWhiteSpace(reason))
                    cancelNote += ": " + reason.Trim();

                try
                {
                    using (SqlCommand cancelMasterSp = new SqlCommand(STOREDPROCEDURE._VendorPaymentMaster, (SqlConnection)DataConnection, transaction))
                    {
                        cancelMasterSp.CommandType = CommandType.StoredProcedure;
                        cancelMasterSp.Parameters.AddWithValue("@Id", paymentMasterId);
                        cancelMasterSp.Parameters.AddWithValue("@Narration", cancelNote);
                        cancelMasterSp.Parameters.AddWithValue("@_Operation", "CANCEL");
                        cancelMasterSp.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calling SP _VendorPaymentMaster CANCEL: {ex.Message}");
                }

                // 5. Cancel Vouchers using Stored Procedure STOREDPROCEDURE.POS_Vouchers
                if (voucherId > 0)
                {
                    try
                    {
                        using (SqlCommand cancelVoucherSp = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, (SqlConnection)DataConnection, transaction))
                        {
                            cancelVoucherSp.CommandType = CommandType.StoredProcedure;
                            cancelVoucherSp.Parameters.AddWithValue("@BranchID", branchId);
                            cancelVoucherSp.Parameters.AddWithValue("@VoucherID", voucherId);
                            cancelVoucherSp.Parameters.AddWithValue("@VoucherType", VendorPaymentVoucherType);
                            cancelVoucherSp.Parameters.AddWithValue("@_Operation", "CANCEL");
                            cancelVoucherSp.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error calling SP POS_Vouchers CANCEL: {ex.Message}");
                    }
                }

                // SyncQueue Integration - Enqueue Vendor Payment Cancellation
                try
                {
                    Guid? existingGuid = SyncQueueRepository.GetExistingGuid(
                        DataConnection,
                        transaction,
                        branchId > 0 ? branchId : SessionContext.BranchId,
                        "VENDOR_PAYMENT",
                        paymentMasterId.ToString()) ?? Guid.NewGuid();

                    SyncQueueRepository.EnqueueTransaction(
                        DataConnection,
                        transaction,
                        branchId > 0 ? branchId : SessionContext.BranchId,
                        "VENDOR_PAYMENT",
                        paymentMasterId.ToString(),
                        existingGuid.Value,
                        "CANCEL");
                }
                catch (Exception syncEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[VendorPaymentRepository.CancelVendorPayment] SyncQueue error: {syncEx.Message}");
                }

                transaction.Commit();
                return new PurchasePaymentCancellationSummary
                {
                    PaymentVoucherCount = 1,
                    PaymentAmount = totalReversed,
                    LastPaymentDate = voucherDate
                };
            }
            catch
            {
                if (transaction != null)
                    transaction.Rollback();
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        public PurchasePaymentCancellationSummary CancelPaymentsForPurchaseEdit(int purchaseNo, int branchId, int vendorLedgerId, int userId, string reason)
        {
            DataConnection.Open();
            SqlTransaction transaction = null;

            try
            {
                transaction = ((SqlConnection)DataConnection).BeginTransaction();

                DataTable affectedMasters = new DataTable();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentDetails, (SqlConnection)DataConnection, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@BillNo", purchaseNo);
                        cmd.Parameters.AddWithValue("@BranchId", branchId);
                        cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
                        cmd.Parameters.AddWithValue("@_Operation", "GETMASTERSBYBILL");

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(affectedMasters);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calling SP _VendorPaymentDetails GETMASTERSBYBILL: {ex.Message}");
                }

                if (affectedMasters == null || affectedMasters.Rows.Count == 0)
                {
                    transaction.Commit();
                    return new PurchasePaymentCancellationSummary();
                }

                decimal totalReversedForPurchase = 0m;
                DateTime lastPaymentDate = DateTime.MinValue;

                foreach (DataRow masterRow in affectedMasters.Rows)
                {
                    int paymentMasterId = masterRow.Table.Columns.Contains("Id") ? Convert.ToInt32(masterRow["Id"]) :
                                          masterRow.Table.Columns.Contains("PaymentMasterId") ? Convert.ToInt32(masterRow["PaymentMasterId"]) : 0;
                    int paymentBranchId = masterRow.Table.Columns.Contains("BranchId") ? Convert.ToInt32(masterRow["BranchId"]) : branchId;
                    int voucherId = masterRow.Table.Columns.Contains("VoucherId") ? Convert.ToInt32(masterRow["VoucherId"]) : 0;
                    if (masterRow.Table.Columns.Contains("VoucherDate") && masterRow["VoucherDate"] != DBNull.Value)
                    {
                        DateTime voucherDate = Convert.ToDateTime(masterRow["VoucherDate"]);
                        if (voucherDate > lastPaymentDate)
                            lastPaymentDate = voucherDate;
                    }

                    // Accumulate the payment amount from details before cancelling
                    try
                    {
                        using (SqlCommand sumCmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentDetails, (SqlConnection)DataConnection, transaction))
                        {
                            sumCmd.CommandType = CommandType.StoredProcedure;
                            sumCmd.Parameters.AddWithValue("@PaymentMasterId", paymentMasterId);
                            sumCmd.Parameters.AddWithValue("@BranchId", paymentBranchId);
                            sumCmd.Parameters.AddWithValue("@_Operation", "GETBYMASTERID");
                            DataTable detailsDt = new DataTable();
                            using (SqlDataAdapter da = new SqlDataAdapter(sumCmd))
                            {
                                da.Fill(detailsDt);
                            }
                            if (detailsDt != null && detailsDt.Rows.Count > 0)
                            {
                                foreach (DataRow dRow in detailsDt.Rows)
                                {
                                    if (dRow.Table.Columns.Contains("PaymentAmount") && dRow["PaymentAmount"] != DBNull.Value)
                                        totalReversedForPurchase += Convert.ToDecimal(dRow["PaymentAmount"]);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error summing detail amounts for paymentMasterId {paymentMasterId}: {ex.Message}");
                    }

                    // Cancel details using SP
                    try
                    {
                        using (SqlCommand cancelDetailsSp = new SqlCommand(STOREDPROCEDURE._VendorPaymentDetails, (SqlConnection)DataConnection, transaction))
                        {
                            cancelDetailsSp.CommandType = CommandType.StoredProcedure;
                            cancelDetailsSp.Parameters.AddWithValue("@PaymentMasterId", paymentMasterId);
                            cancelDetailsSp.Parameters.AddWithValue("@_Operation", "CANCEL");
                            cancelDetailsSp.ExecuteNonQuery();
                        }
                    }
                    catch { }

                    // Cancel master using SP
                    string cancelNote = " | Cancelled for purchase edit GRN-" + purchaseNo;
                    if (!string.IsNullOrWhiteSpace(reason))
                        cancelNote += ": " + reason.Trim();

                    try
                    {
                        using (SqlCommand cancelMasterSp = new SqlCommand(STOREDPROCEDURE._VendorPaymentMaster, (SqlConnection)DataConnection, transaction))
                        {
                            cancelMasterSp.CommandType = CommandType.StoredProcedure;
                            cancelMasterSp.Parameters.AddWithValue("@Id", paymentMasterId);
                            cancelMasterSp.Parameters.AddWithValue("@Narration", cancelNote);
                            cancelMasterSp.Parameters.AddWithValue("@_Operation", "CANCEL");
                            cancelMasterSp.ExecuteNonQuery();
                        }
                    }
                    catch { }

                    // Cancel voucher using SP
                    if (voucherId > 0)
                    {
                        try
                        {
                            using (SqlCommand cancelVoucherSp = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, (SqlConnection)DataConnection, transaction))
                            {
                                cancelVoucherSp.CommandType = CommandType.StoredProcedure;
                                cancelVoucherSp.Parameters.AddWithValue("@BranchID", paymentBranchId);
                                cancelVoucherSp.Parameters.AddWithValue("@VoucherID", voucherId);
                                cancelVoucherSp.Parameters.AddWithValue("@VoucherType", VendorPaymentVoucherType);
                                cancelVoucherSp.Parameters.AddWithValue("@_Operation", "CANCEL");
                                cancelVoucherSp.ExecuteNonQuery();
                            }
                        }
                        catch { }
                    }
                }

                transaction.Commit();
                return new PurchasePaymentCancellationSummary
                {
                    PaymentVoucherCount = affectedMasters.Rows.Count,
                    PaymentAmount = totalReversedForPurchase,
                    LastPaymentDate = lastPaymentDate
                };
            }
            catch
            {
                if (transaction != null)
                    transaction.Rollback();
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        public DataTable GetAllPayments(int branchId)
        {
            DataTable dt = new DataTable();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@VoucherType", VendorPaymentVoucherType);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    cmd.Parameters.AddWithValue("@PageIndex", 0);
                    cmd.Parameters.AddWithValue("@PageSize", 1000);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return dt;
        }

        /// <summary>
        /// Returns all active (non-cancelled) payment vouchers for a given vendor,
        /// grouped by GRN (BillNo).  Used by the GRN Payment Cancellation popup.
        /// Columns: PaymentMasterId, GrnNo, VoucherNo, VoucherDate, PaymentAmount, VendorLedgerId
        /// </summary>
        public DataTable GetActiveVouchersByVendor(int vendorLedgerId, int branchId)
        {
            DataTable dt = new DataTable();
            DataConnection.Open();
            try
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentMaster, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@BranchId", branchId);
                        cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
                        cmd.Parameters.AddWithValue("@_Operation", "GETACTIVEVOUCHERS");

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calling SP _VendorPaymentMaster GETACTIVEVOUCHERS: {ex.Message}");
                }

                if (dt == null || dt.Rows.Count == 0)
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentDetails, (SqlConnection)DataConnection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@BranchId", branchId);
                            cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
                            cmd.Parameters.AddWithValue("@_Operation", "GETACTIVEVOUCHERS");

                            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            {
                                da.Fill(dt);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error calling SP _VendorPaymentDetails GETACTIVEVOUCHERS: {ex.Message}");
                    }
                }

                return dt;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }
    }
}
