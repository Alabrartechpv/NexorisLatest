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
                        // First check if the bill exists in PMaster
                        bool billExists = false;
                        if (int.TryParse(detail.BillNo, out int billNo))
                        {
                            using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM PMaster WHERE PurchaseNo = @BillNo AND BranchId = @BranchId AND LedgerID = @VendorLedgerId AND CancelFlag = 0", (SqlConnection)DataConnection, transaction))
                            {
                                checkCmd.Parameters.AddWithValue("@BillNo", billNo);
                                checkCmd.Parameters.AddWithValue("@BranchId", branchId);
                                checkCmd.Parameters.AddWithValue("@VendorLedgerId", master.VendorLedgerId);
                                billExists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
                            }

                            if (!billExists)
                            {
                                throw new Exception($"Bill #{billNo} doesn't exist in the system for this vendor. Cannot process payment.");
                            }

                            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentDetails, (SqlConnection)DataConnection, transaction))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@BranchId", branchId);
                                cmd.Parameters.AddWithValue("@VendorLedgerId", master.VendorLedgerId);
                                cmd.Parameters.AddWithValue("@CreditPaymodeId", paymentMethodId);
                                cmd.Parameters.AddWithValue("@PaymentMasterId", paymentMasterId);
                                cmd.Parameters.AddWithValue("@BIllNo", billNo);
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
                        else
                        {
                            throw new Exception($"Invalid BillNo format: {detail.BillNo}. BillNo must be a valid integer.");
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
                // Resolve the actual Ledger ID for the selected payment mode from the PayMode table first
                if (paymentMethodId > 0)
                {
                    try
                    {
                        string paymodeQuery = "SELECT TOP 1 LedgerID FROM PayMode WHERE PayModeID = @PayModeID";
                        using (SqlCommand paymodeCmd = new SqlCommand(paymodeQuery, (SqlConnection)DataConnection))
                        {
                            paymodeCmd.Parameters.AddWithValue("@PayModeID", paymentMethodId);
                            // Connection is already open from SaveVendorPayment
                            object paymodeResult = paymodeCmd.ExecuteScalar();
                            if (paymodeResult != null && paymodeResult != DBNull.Value)
                            {
                                int ledgerId = Convert.ToInt32(paymodeResult);
                                if (ledgerId > 0)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Found LedgerID {ledgerId} for PayModeID {paymentMethodId} in PayMode table.");
                                    return ledgerId;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error fetching LedgerID from PayMode for vendor payment: {ex.Message}");
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

            // Direct SQL fallback if SP returned 0 rows or failed
            if (dt == null || dt.Rows.Count == 0)
            {
                try
                {
                    DataConnection.Open();
                    string sql = @"
SELECT 
    P.PurchaseNo AS BillNo,
    ISNULL(P.InvoiceNo, CAST(P.PurchaseNo AS VARCHAR(50))) AS InvoiceNo,
    ISNULL(P.GrandTotal, ISNULL(P.TotalAmount, 0)) AS InvoiceAmount,
    ISNULL(P.PayedAmount, 0) AS PayedAmount,
    ISNULL(P.ReturnedAmount, 0) AS ReturnedAmount,
    (ISNULL(P.GrandTotal, ISNULL(P.TotalAmount, 0)) - ISNULL(P.PayedAmount, 0) - ISNULL(P.ReturnedAmount, 0)) AS Balance,
    ISNULL(P.PurchaseDate, GETDATE()) AS BillDate,
    ISNULL(P.Paymode, '') AS Paymode,
    ISNULL(P.PaymodeID, 0) AS PaymodeID
FROM PMaster P
WHERE P.LedgerID = @VendorLedgerId
  AND ISNULL(P.CancelFlag, 0) = 0
  AND (ISNULL(P.GrandTotal, ISNULL(P.TotalAmount, 0)) - ISNULL(P.PayedAmount, 0) - ISNULL(P.ReturnedAmount, 0)) > 0
ORDER BY P.PurchaseDate ASC, P.PurchaseNo ASC";

                    using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                    {
                        cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            dt = new DataTable();
                            da.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error running SQL fallback for GETOUTSTANDING: {ex.Message}");
                }
                finally
                {
                    if (DataConnection.State == ConnectionState.Open)
                        DataConnection.Close();
                }
            }

            SanitizeInvoiceTable(dt);
            EnhanceInvoiceTableWithCashPaymode(dt);
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

            // Direct SQL fallback if SP returned 0 rows or failed
            if (dt == null || dt.Rows.Count == 0)
            {
                try
                {
                    DataConnection.Open();
                    string sql = @"
SELECT 
    P.PurchaseNo AS BillNo,
    ISNULL(P.InvoiceNo, CAST(P.PurchaseNo AS VARCHAR(50))) AS InvoiceNo,
    ISNULL(P.GrandTotal, ISNULL(P.TotalAmount, 0)) AS InvoiceAmount,
    ISNULL(P.PayedAmount, 0) AS PayedAmount,
    ISNULL(P.ReturnedAmount, 0) AS ReturnedAmount,
    (ISNULL(P.GrandTotal, ISNULL(P.TotalAmount, 0)) - ISNULL(P.PayedAmount, 0) - ISNULL(P.ReturnedAmount, 0)) AS Balance,
    ISNULL(P.PurchaseDate, GETDATE()) AS BillDate,
    ISNULL(P.Paymode, '') AS Paymode,
    ISNULL(P.PaymodeID, 0) AS PaymodeID
FROM PMaster P
WHERE P.LedgerID = @VendorLedgerId
  AND ISNULL(P.CancelFlag, 0) = 0
ORDER BY P.PurchaseDate DESC, P.PurchaseNo DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                    {
                        cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            dt = new DataTable();
                            da.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error running SQL fallback for GETALLINVOICES: {ex.Message}");
                }
                finally
                {
                    if (DataConnection.State == ConnectionState.Open)
                        DataConnection.Close();
                }
            }

            SanitizeInvoiceTable(dt);
            EnhanceInvoiceTableWithCashPaymode(dt);
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

                        foreach (DataRow r in pmDt.Rows)
                        {
                            if (r.Table.Columns.Contains("PurchaseNo") && r["PurchaseNo"] != DBNull.Value)
                            {
                                string pNo = r["PurchaseNo"].ToString().Trim();
                                string pm = paymodeCol != null && r[paymodeCol] != DBNull.Value ? r[paymodeCol].ToString().Trim() : "";
                                int pmId = paymodeIdCol != null && r[paymodeIdCol] != DBNull.Value ? Convert.ToInt32(r[paymodeIdCol]) : 0;
                                pmNameMap[pNo] = pm;
                                pmIdMap[pNo] = pmId;
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
            if (DataConnection.State == ConnectionState.Open)
                DataConnection.Close();

            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._VendorPaymentMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@_Operation", "OUTSTANDINGTOTAL");

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            object result = reader["TotalOutStanding"];
                            if (result != null && result != DBNull.Value)
                            {
                                outstandingTotal = Convert.ToDecimal(result);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting vendor outstanding total: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            if (outstandingTotal <= 0)
            {
                try
                {
                    DataConnection.Open();
                    string sql = @"
SELECT ISNULL(SUM(ISNULL(P.GrandTotal, ISNULL(P.TotalAmount, 0)) - ISNULL(P.PayedAmount, 0) - ISNULL(P.ReturnedAmount, 0)), 0)
FROM PMaster P
WHERE P.LedgerID = @VendorLedgerId
  AND ISNULL(P.CancelFlag, 0) = 0
  AND (ISNULL(P.GrandTotal, ISNULL(P.TotalAmount, 0)) - ISNULL(P.PayedAmount, 0) - ISNULL(P.ReturnedAmount, 0)) > 0";

                    using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                    {
                        cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
                        object res = cmd.ExecuteScalar();
                        if (res != null && res != DBNull.Value)
                        {
                            outstandingTotal = Convert.ToDecimal(res);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting vendor outstanding total via SQL: {ex.Message}");
                }
                finally
                {
                    if (DataConnection.State == ConnectionState.Open)
                        DataConnection.Close();
                }
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
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(@"
SELECT BillNo, BillDate, BillAmount, PaymentAmount, BalanceAmount
FROM VendorPaymentDetails
WHERE VendorLedgerId = @VendorLedgerId
  AND BillNo = @BillNo
  AND ISNULL(CancelFlag, 0) = 0
ORDER BY BillDate DESC, PaymentMasterId DESC", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
                    cmd.Parameters.AddWithValue("@BillNo", billNo);

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                    return dt;
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
        }

        public PurchasePaymentCancellationSummary GetActivePaymentSummaryForPurchase(int purchaseNo, int branchId, int vendorLedgerId)
        {
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(@"
SELECT
    COUNT(DISTINCT VPM.Id) AS PaymentVoucherCount,
    ISNULL(SUM(ISNULL(VPD.PaymentAmount, 0)), 0) AS PaymentAmount,
    MAX(VPM.VoucherDate) AS LastPaymentDate
FROM VendorPaymentMaster VPM
INNER JOIN VendorPaymentDetails VPD ON VPD.PaymentMasterId = VPM.Id
WHERE ISNULL(VPM.CancelFlag, 0) = 0
  AND ISNULL(VPD.CancelFlag, 0) = 0
  AND VPM.BranchId = @BranchId
  AND VPD.BillNo = @PurchaseNo
  AND VPD.VendorLedgerId = @VendorLedgerId", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@PurchaseNo", purchaseNo);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return new PurchasePaymentCancellationSummary();

                        return new PurchasePaymentCancellationSummary
                        {
                            PaymentVoucherCount = reader["PaymentVoucherCount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PaymentVoucherCount"]),
                            PaymentAmount = reader["PaymentAmount"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["PaymentAmount"]),
                            LastPaymentDate = reader["LastPaymentDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["LastPaymentDate"])
                        };
                    }
                }
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
                int voucherId;
                DateTime voucherDate;

                using (SqlCommand masterCmd = new SqlCommand(@"
SELECT Id, BranchId, VoucherId, VoucherDate
FROM VendorPaymentMaster
WHERE Id = @PaymentMasterId
  AND BranchId = @BranchId
  AND ISNULL(CancelFlag, 0) = 0", (SqlConnection)DataConnection, transaction))
                {
                    masterCmd.Parameters.AddWithValue("@PaymentMasterId", paymentMasterId);
                    masterCmd.Parameters.AddWithValue("@BranchId", branchId);

                    using (SqlDataReader reader = masterCmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            throw new Exception("This vendor payment is already cancelled or could not be found.");

                        voucherId = Convert.ToInt32(reader["VoucherId"]);
                        voucherDate = reader["VoucherDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["VoucherDate"]);
                    }
                }

                using (SqlCommand detailsCmd = new SqlCommand(@"
SELECT BranchId, BillNo, VendorLedgerId, ISNULL(PaymentAmount, 0) AS PaymentAmount
FROM VendorPaymentDetails
WHERE PaymentMasterId = @PaymentMasterId
  AND ISNULL(CancelFlag, 0) = 0", (SqlConnection)DataConnection, transaction))
                {
                    detailsCmd.Parameters.AddWithValue("@PaymentMasterId", paymentMasterId);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(detailsCmd))
                    {
                        adapter.Fill(activeDetails);
                    }
                }

                if (activeDetails.Rows.Count == 0)
                    throw new Exception("No active payment allocations were found for this voucher.");

                decimal totalReversed = 0m;
                foreach (DataRow detailRow in activeDetails.Rows)
                {
                    int detailBranchId = Convert.ToInt32(detailRow["BranchId"]);
                    int billNo = Convert.ToInt32(detailRow["BillNo"]);
                    int vendorLedgerId = Convert.ToInt32(Convert.ToDecimal(detailRow["VendorLedgerId"]));
                    decimal paymentAmount = Convert.ToDecimal(detailRow["PaymentAmount"]);

                    using (SqlCommand reverseCmd = new SqlCommand(@"
UPDATE PMaster
SET PayedAmount = CASE
        WHEN ROUND(ISNULL(PayedAmount, 0) - @PaymentAmount, 2) < 0 THEN 0
        ELSE ROUND(ISNULL(PayedAmount, 0) - @PaymentAmount, 2)
    END,
    Paid = 0
WHERE CancelFlag = 0
  AND PurchaseNo = @BillNo
  AND BranchId = @BranchId
  AND LedgerID = @VendorLedgerId", (SqlConnection)DataConnection, transaction))
                    {
                        reverseCmd.Parameters.AddWithValue("@PaymentAmount", (float)paymentAmount);
                        reverseCmd.Parameters.AddWithValue("@BillNo", billNo);
                        reverseCmd.Parameters.AddWithValue("@BranchId", detailBranchId);
                        reverseCmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
                        reverseCmd.ExecuteNonQuery();
                    }

                    totalReversed += paymentAmount;
                }

                using (SqlCommand cancelDetailsCmd = new SqlCommand(@"
UPDATE VendorPaymentDetails
SET CancelFlag = 1
WHERE PaymentMasterId = @PaymentMasterId
  AND ISNULL(CancelFlag, 0) = 0", (SqlConnection)DataConnection, transaction))
                {
                    cancelDetailsCmd.Parameters.AddWithValue("@PaymentMasterId", paymentMasterId);
                    cancelDetailsCmd.ExecuteNonQuery();
                }

                using (SqlCommand cancelMasterCmd = new SqlCommand(@"
UPDATE VendorPaymentMaster
SET CancelFlag = 1,
    Narration = LEFT(ISNULL(Narration, '') + @CancelNote, 4000)
WHERE Id = @PaymentMasterId
  AND ISNULL(CancelFlag, 0) = 0", (SqlConnection)DataConnection, transaction))
                {
                    string cancelNote = " | Cancelled";
                    if (!string.IsNullOrWhiteSpace(reason))
                        cancelNote += ": " + reason.Trim();

                    cancelMasterCmd.Parameters.AddWithValue("@CancelNote", cancelNote);
                    cancelMasterCmd.Parameters.AddWithValue("@PaymentMasterId", paymentMasterId);
                    cancelMasterCmd.ExecuteNonQuery();
                }

                using (SqlCommand cancelVoucherCmd = new SqlCommand(@"
UPDATE Vouchers
SET CancelFlag = 1
WHERE BranchID = @BranchId
  AND VoucherID = @VoucherId
  AND VoucherType = @VoucherType
  AND ISNULL(CancelFlag, 0) = 0", (SqlConnection)DataConnection, transaction))
                {
                    cancelVoucherCmd.Parameters.AddWithValue("@BranchId", branchId);
                    cancelVoucherCmd.Parameters.AddWithValue("@VoucherId", voucherId);
                    cancelVoucherCmd.Parameters.AddWithValue("@VoucherType", VendorPaymentVoucherType);
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
                using (SqlCommand cmd = new SqlCommand(@"
SELECT DISTINCT VPM.Id, VPM.BranchId, VPM.VoucherId, VPM.VoucherDate
FROM VendorPaymentMaster VPM
INNER JOIN VendorPaymentDetails VPD ON VPD.PaymentMasterId = VPM.Id
WHERE ISNULL(VPM.CancelFlag, 0) = 0
  AND ISNULL(VPD.CancelFlag, 0) = 0
  AND VPM.BranchId = @BranchId
  AND VPD.BillNo = @PurchaseNo
  AND VPD.VendorLedgerId = @VendorLedgerId", (SqlConnection)DataConnection, transaction))
                {
                    cmd.Parameters.AddWithValue("@PurchaseNo", purchaseNo);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(affectedMasters);
                    }
                }

                if (affectedMasters.Rows.Count == 0)
                {
                    transaction.Commit();
                    return new PurchasePaymentCancellationSummary();
                }

                decimal totalReversedForPurchase = 0m;
                DateTime lastPaymentDate = DateTime.MinValue;

                foreach (DataRow masterRow in affectedMasters.Rows)
                {
                    int paymentMasterId = Convert.ToInt32(masterRow["Id"]);
                    int paymentBranchId = Convert.ToInt32(masterRow["BranchId"]);
                    int voucherId = Convert.ToInt32(masterRow["VoucherId"]);
                    if (masterRow["VoucherDate"] != DBNull.Value)
                    {
                        DateTime voucherDate = Convert.ToDateTime(masterRow["VoucherDate"]);
                        if (voucherDate > lastPaymentDate)
                            lastPaymentDate = voucherDate;
                    }

                    DataTable activeDetails = new DataTable();
                    using (SqlCommand detailsCmd = new SqlCommand(@"
SELECT BranchId, BillNo, VendorLedgerId, ISNULL(PaymentAmount, 0) AS PaymentAmount
FROM VendorPaymentDetails
WHERE PaymentMasterId = @PaymentMasterId
  AND ISNULL(CancelFlag, 0) = 0", (SqlConnection)DataConnection, transaction))
                    {
                        detailsCmd.Parameters.AddWithValue("@PaymentMasterId", paymentMasterId);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(detailsCmd))
                        {
                            adapter.Fill(activeDetails);
                        }
                    }

                    foreach (DataRow detailRow in activeDetails.Rows)
                    {
                        int detailBranchId = Convert.ToInt32(detailRow["BranchId"]);
                        int detailBillNo = Convert.ToInt32(detailRow["BillNo"]);
                        int detailVendorLedgerId = Convert.ToInt32(Convert.ToDecimal(detailRow["VendorLedgerId"]));
                        decimal paymentAmount = Convert.ToDecimal(detailRow["PaymentAmount"]);

                        using (SqlCommand reverseCmd = new SqlCommand(@"
UPDATE PMaster
SET PayedAmount = CASE
        WHEN ROUND(ISNULL(PayedAmount, 0) - @PaymentAmount, 2) < 0 THEN 0
        ELSE ROUND(ISNULL(PayedAmount, 0) - @PaymentAmount, 2)
    END,
    Paid = 0
WHERE CancelFlag = 0
  AND PurchaseNo = @BillNo
  AND BranchId = @BranchId
  AND LedgerID = @VendorLedgerId", (SqlConnection)DataConnection, transaction))
                        {
                            reverseCmd.Parameters.AddWithValue("@PaymentAmount", (float)paymentAmount);
                            reverseCmd.Parameters.AddWithValue("@BillNo", detailBillNo);
                            reverseCmd.Parameters.AddWithValue("@BranchId", detailBranchId);
                            reverseCmd.Parameters.AddWithValue("@VendorLedgerId", detailVendorLedgerId);
                            reverseCmd.ExecuteNonQuery();
                        }

                        if (detailBillNo == purchaseNo && detailBranchId == branchId && detailVendorLedgerId == vendorLedgerId)
                            totalReversedForPurchase += paymentAmount;
                    }

                    using (SqlCommand cancelDetailsCmd = new SqlCommand(@"
UPDATE VendorPaymentDetails
SET CancelFlag = 1
WHERE PaymentMasterId = @PaymentMasterId
  AND ISNULL(CancelFlag, 0) = 0", (SqlConnection)DataConnection, transaction))
                    {
                        cancelDetailsCmd.Parameters.AddWithValue("@PaymentMasterId", paymentMasterId);
                        cancelDetailsCmd.ExecuteNonQuery();
                    }

                    using (SqlCommand cancelMasterCmd = new SqlCommand(@"
UPDATE VendorPaymentMaster
SET CancelFlag = 1,
    Narration = LEFT(ISNULL(Narration, '') + @CancelNote, 4000)
WHERE Id = @PaymentMasterId
  AND ISNULL(CancelFlag, 0) = 0", (SqlConnection)DataConnection, transaction))
                    {
                        string cancelNote = " | Cancelled for purchase edit GRN-" + purchaseNo;
                        if (!string.IsNullOrWhiteSpace(reason))
                            cancelNote += ": " + reason.Trim();
                        cancelMasterCmd.Parameters.AddWithValue("@CancelNote", cancelNote);
                        cancelMasterCmd.Parameters.AddWithValue("@PaymentMasterId", paymentMasterId);
                        cancelMasterCmd.ExecuteNonQuery();
                    }

                    using (SqlCommand cancelVoucherCmd = new SqlCommand(@"
UPDATE Vouchers
SET CancelFlag = 1
WHERE BranchID = @BranchId
  AND VoucherID = @VoucherId
  AND VoucherType = @VoucherType
  AND ISNULL(CancelFlag, 0) = 0", (SqlConnection)DataConnection, transaction))
                    {
                        cancelVoucherCmd.Parameters.AddWithValue("@BranchId", paymentBranchId);
                        cancelVoucherCmd.Parameters.AddWithValue("@VoucherId", voucherId);
                        cancelVoucherCmd.Parameters.AddWithValue("@VoucherType", VendorPaymentVoucherType);
                        cancelVoucherCmd.ExecuteNonQuery();
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
                using (SqlCommand cmd = new SqlCommand(@"
SELECT
    VPM.Id           AS PaymentMasterId,
    VPD.BillNo       AS GrnNo,
    VPM.VoucherId    AS VoucherNo,
    VPM.VoucherDate,
    ISNULL(VPD.PaymentAmount, 0) AS PaymentAmount,
    VPD.VendorLedgerId
FROM VendorPaymentMaster VPM
INNER JOIN VendorPaymentDetails VPD ON VPD.PaymentMasterId = VPM.Id
WHERE VPM.BranchId          = @BranchId
  AND VPD.VendorLedgerId    = @VendorLedgerId
  AND ISNULL(VPM.CancelFlag, 0) = 0
  AND ISNULL(VPD.CancelFlag, 0) = 0
ORDER BY VPD.BillNo, VPM.VoucherDate DESC", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
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
            return dt;
        }
    }
}
