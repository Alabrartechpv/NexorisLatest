using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelClass.Accounts;
using System.Data;
using System.Data.SqlClient;
using ModelClass.TransactionModels;
using ModelClass;


namespace Repository.Accounts
{
    public class CustomerReceiptInfoRepository : BaseRepostitory
    {
        Voucher vochPosCustReceipt = new Voucher();

        public int GenerateVoucherId(int branchId, int finYearId)
        {
            int voucherId = 0;

            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchID", branchId);
                    cmd.Parameters.AddWithValue("@FinYearID", finYearId);
                    cmd.Parameters.AddWithValue("@VoucherType", "CUSTRCPT");
                    cmd.Parameters.AddWithValue("@_Operation", "GENERATENUMBER");

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        voucherId = Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating voucher ID: {ex.Message}", ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return voucherId;
        }

        public CustomerSalesInfoGrid getCustomerSalesReceiptInfo(int LedgerId)
        {
            CustomerSalesInfoGrid objCustomerSalesInfoGrid = new CustomerSalesInfoGrid();



            DataConnection.Open();
            try
            {
                SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_CustomerReceiptInfo, (SqlConnection)DataConnection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@LedgerId", LedgerId);
                SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapt.Fill(ds);
                if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                {
                    objCustomerSalesInfoGrid.CustomerSalesList = ds.Tables[0].ToListOfObject<CustomerReceiptInofo>();
                }

            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();

            }
            return objCustomerSalesInfoGrid;
        }


        public DataTable GetOutstandingInvoices(int customerId, int branchId)
        {

            DataConnection.Open();

            try
            {

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CustomerReceiptMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerLedgerId", customerId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETOUTSTANDING");

                    DataTable dt = new DataTable();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                    EnhanceInvoiceTableWithCashPaymode(dt);
                    NormalizeInvoiceBalances(dt);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

        }


        public DataTable GetAllInvoices(int customerId, int branchId)
        {
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CustomerReceiptMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerLedgerId", customerId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALLINVOICES");

                    DataTable dt = new DataTable();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                    EnhanceInvoiceTableWithCashPaymode(dt);
                    NormalizeInvoiceBalances(dt);
                    return dt;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

        }


        public bool SaveCustomerReceipt(CustomerReceiptMaster master, List<CustomerReceiptDetails> details, List<VoucherEntry> voucherEntries)
        {
            if (master == null || details == null || !details.Any() || voucherEntries == null || voucherEntries.Count < 2)
            {
                return false; // Validate input parameters
            }

            ValidateReceiptAmounts(master, details, voucherEntries);

            SqlConnection conn = DataConnection as SqlConnection;
            if (conn == null)
            {
                throw new Exception("Database connection is not initialized.");
            }

            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Convert master.CreatedBy from string to int for the UserID parameter
                        int userId = 1; // Default value
                        if (!string.IsNullOrEmpty(master.CreatedBy))
                        {
                            if (!int.TryParse(master.CreatedBy, out userId))
                            {
                                userId = 1; // Use default if parsing fails
                            }
                        }

                            // Initialize LedgerRepository to get proper ledger IDs
                            var ledgerRepository = new Repository.MasterRepositry.LedgerRepository();

                            // 1. Generate Voucher Number if not provided
                            int defaultFinYearId = SessionContext.FinYearId; // Default value or get from settings

                            if (master.VoucherId <= 0)
                            {
                                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, conn, transaction))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("@BranchID", master.BranchId);
                                    cmd.Parameters.AddWithValue("@FinYearID", defaultFinYearId);
                                    cmd.Parameters.AddWithValue("@VoucherType", "CUSTRCPT");
                                    cmd.Parameters.AddWithValue("@_Operation", "GENERATENUMBER");

                                    object result = cmd.ExecuteScalar();
                                    if (result != null && result != DBNull.Value)
                                    {
                                        master.VoucherId = Convert.ToInt32(result);
                                     }
                                     if (master.VoucherId <= 0)
                                     {
                                         throw new Exception("SP returned an invalid VoucherId (<= 0) for CUSTRCPT.");
                                     }
                                }
                            }

                            // Convert payment method from string to int
                            int paymentMethodId = 0;
                            if (!string.IsNullOrEmpty(master.PaymentMethod))
                            {
                                if (!int.TryParse(master.PaymentMethod, out paymentMethodId))
                                {
                                    transaction.Rollback();
                                    return false; // Invalid payment method
                                }
                            }
                            else
                            {
                                transaction.Rollback();
                                return false; // Missing payment method
                            }

                             // Resolve the actual Ledger ID for the selected payment mode via POS_PayMode SP
                             int cashLedgerId = 0;
                             try
                             {
                                 using (SqlCommand paymodeCmd = new SqlCommand(STOREDPROCEDURE.POS_PayMode, conn, transaction))
                                 {
                                     paymodeCmd.CommandType = CommandType.StoredProcedure;
                                     paymodeCmd.Parameters.AddWithValue("@PaymodeId", paymentMethodId);
                                     paymodeCmd.Parameters.AddWithValue("@_Operation", "GETBYID");
                                     using (SqlDataAdapter da = new SqlDataAdapter(paymodeCmd))
                                     {
                                         DataTable dtPaymode = new DataTable();
                                         da.Fill(dtPaymode);
                                         if (dtPaymode != null && dtPaymode.Rows.Count > 0 &&
                                             dtPaymode.Columns.Contains("LedgerID") &&
                                             dtPaymode.Rows[0]["LedgerID"] != DBNull.Value)
                                         {
                                             cashLedgerId = Convert.ToInt32(dtPaymode.Rows[0]["LedgerID"]);
                                         }
                                     }
                                 }
                             }
                             catch (Exception ex)
                             {
                                 System.Diagnostics.Debug.WriteLine($"Error fetching LedgerID from POS_PayMode SP: {ex.Message}");
                             }

                             // Fallback to cash ledger if not resolved from PayMode SP
                             if (cashLedgerId <= 0)
                             {
                                 cashLedgerId = GetCashLedgerId(paymentMethodId, master.BranchId, ledgerRepository);
                             }
                             System.Diagnostics.Debug.WriteLine($"PaymentMethodId: {paymentMethodId}, Resolved LedgerID: {cashLedgerId}");

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

                            // Make sure we have a valid highestBillNo
                            if (highestBillNo <= 0)
                            {
                                // Try to find any valid bill number
                                foreach (var detail in details)
                                {
                                    if (!string.IsNullOrEmpty(detail.BillNo))
                                    {
                                        highestBillNo = 1; // Set to a safe default
                                        break;
                                    }
                                }

                                // If still no valid bill number, use a safe default
                                if (highestBillNo <= 0)
                                {
                                    highestBillNo = 1;
                                }
                            }

                            int legacyCompanyId;
                            int companyId = SessionContext.CompanyId > 0
                                ? SessionContext.CompanyId
                                : int.TryParse(DataBase.CompanyId, out legacyCompanyId) && legacyCompanyId > 0
                                    ? legacyCompanyId
                                    : master.BranchId;

                            // 2. Insert into CustomerReceiptMaster
                            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CustomerReceiptMaster, conn, transaction))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@CompanyId", companyId);
                                cmd.Parameters.AddWithValue("@BranchId", master.BranchId);
                                cmd.Parameters.AddWithValue("@VoucherId", master.VoucherId);
                                cmd.Parameters.AddWithValue("@VoucherDate", master.ReceiptDate);
                                cmd.Parameters.AddWithValue("@PaymentMethodLedgerId", paymentMethodId);
                                cmd.Parameters.AddWithValue("@CustomerLedgerId", master.CustomerLedgerId);
                                cmd.Parameters.AddWithValue("@ReceivableAmount", master.TotalReceivableAmount);
                                cmd.Parameters.AddWithValue("@ReceiptAmount", master.TotalReceiptAmount);
                                cmd.Parameters.AddWithValue("@OldReceiptAmount", 0);
                                string narration = !string.IsNullOrEmpty(master.SalesPerson) ? master.SalesPerson : "";
                                cmd.Parameters.AddWithValue("@Narration", narration);
                                cmd.Parameters.AddWithValue("@BillNoUntil", highestBillNo);
                                cmd.Parameters.AddWithValue("@UserId", userId); // Use parsed integer
                                cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                                using (var reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read() && reader.FieldCount >= 2)
                                    {
                                        var status = reader[0].ToString();
                                        if (status == "SUCCESS")
                                        {
                                            master.ReceiptId = Convert.ToInt32(reader[1]);
                                        }
                                        else
                                        {
                                            transaction.Rollback();
                                            return false; // SP did not return SUCCESS
                                        }
                                    }
                                    else
                                    {
                                        transaction.Rollback();
                                        return false; // Failed to create master record or get receipt ID
                                    }
                                }
                            }

                            // 3. Insert into CustomerReceiptDetails for each selected invoice
                            foreach (var detail in details)
                            {
                                if (detail.AdjustedAmount > 0)
                                {
                                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CustomerReceiptDetails, conn, transaction))
                                    {
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.Parameters.AddWithValue("@BranchId", master.BranchId);
                                        cmd.Parameters.AddWithValue("@CustomerLedgerId", master.CustomerLedgerId);
                                        cmd.Parameters.AddWithValue("@CreditPaymodeId", paymentMethodId);
                                        cmd.Parameters.AddWithValue("@ReceiptMasterId", master.ReceiptId);

                                        if (!int.TryParse(detail.BillNo, out int billNo))
                                        {
                                            transaction.Rollback();
                                            throw new Exception($"Invalid BillNo format: {detail.BillNo}. BillNo must be a valid integer.");
                                        }
                                        cmd.Parameters.AddWithValue("@BillNo", billNo);
                                        cmd.Parameters.AddWithValue("@BillDate", detail.BillDate);
                                        cmd.Parameters.AddWithValue("@BillAmount", detail.InvoiceAmount);
                                        cmd.Parameters.AddWithValue("@ReceivedAmount", 0); // Will be calculated in SP
                                        cmd.Parameters.AddWithValue("@ReceiptAmount", detail.AdjustedAmount);

                                        // Don't pass balance - let stored procedure calculate it
                                        cmd.Parameters.AddWithValue("@BalanceAmount", detail.Balance); // Pass the running balance value
                                        cmd.Parameters.AddWithValue("@OldBillAmount", detail.InvoiceAmount);
                                        cmd.Parameters.AddWithValue("@OldReceiptAmount", 0);
                                        cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                                        var detailResult = cmd.ExecuteScalar();
                                        if (detailResult == null || !detailResult.ToString().StartsWith("SUCCESS"))
                                        {
                                            transaction.Rollback();
                                            return false;
                                        }
                                    }
                                }
                            }

                            // 4. Create Voucher Entries - Double entry system
                            DateTime userDate = DateTime.Now;
                            string voucherNarration = !string.IsNullOrEmpty(master.SalesPerson) ? master.SalesPerson : "";

                            // Create debit entry (Cash or Bank account)
                            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, conn, transaction))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@CompanyID", companyId);
                                cmd.Parameters.AddWithValue("@BranchID", master.BranchId);
                                cmd.Parameters.AddWithValue("@VoucherID", master.VoucherId);
                                cmd.Parameters.AddWithValue("@VoucherSeriesID", 0);
                                cmd.Parameters.AddWithValue("@VoucherDate", master.ReceiptDate);
                                cmd.Parameters.AddWithValue("@VoucherNumber", "");
                                cmd.Parameters.AddWithValue("@LedgerID", cashLedgerId); // Use actual cash ledger ID instead of payment method ID
                                cmd.Parameters.AddWithValue("@VoucherType", "CUSTRCPT");
                                cmd.Parameters.AddWithValue("@Debit", (float)master.TotalReceiptAmount); // Convert to float as required by SP
                                cmd.Parameters.AddWithValue("@Credit", 0);
                                cmd.Parameters.AddWithValue("@Narration", voucherNarration);
                                cmd.Parameters.AddWithValue("@SlNo", 1);
                                cmd.Parameters.AddWithValue("@Mode", "");
                                cmd.Parameters.AddWithValue("@ModeID", 0);
                                cmd.Parameters.AddWithValue("@UserDate", userDate);
                                cmd.Parameters.AddWithValue("@UserID", userId); // Use parsed integer
                                cmd.Parameters.AddWithValue("@CancelFlag", false);
                                cmd.Parameters.AddWithValue("@FinYearID", defaultFinYearId);
                                cmd.Parameters.AddWithValue("@IsSyncd", false);
                                cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                                System.Diagnostics.Debug.WriteLine($"Creating debit voucher entry: VoucherID={master.VoucherId}, LedgerID={cashLedgerId}, Debit={master.TotalReceiptAmount}");

                                var voucherResult = cmd.ExecuteScalar();
                                if (voucherResult == null || !voucherResult.ToString().StartsWith("SUCCESS"))
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                            }

                            // Create credit entry (Customer account)
                            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, conn, transaction))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@CompanyID", companyId);
                                cmd.Parameters.AddWithValue("@BranchID", master.BranchId);
                                cmd.Parameters.AddWithValue("@VoucherID", master.VoucherId);
                                cmd.Parameters.AddWithValue("@VoucherSeriesID", 0);
                                cmd.Parameters.AddWithValue("@VoucherDate", master.ReceiptDate);
                                cmd.Parameters.AddWithValue("@VoucherNumber", "");
                                cmd.Parameters.AddWithValue("@LedgerID", master.CustomerLedgerId);
                                cmd.Parameters.AddWithValue("@VoucherType", "CUSTRCPT");
                                cmd.Parameters.AddWithValue("@Debit", 0);
                                cmd.Parameters.AddWithValue("@Credit", (float)master.TotalReceiptAmount); // Convert to float as required by SP
                                cmd.Parameters.AddWithValue("@Narration", voucherNarration);
                                cmd.Parameters.AddWithValue("@SlNo", 2);
                                cmd.Parameters.AddWithValue("@Mode", "");
                                cmd.Parameters.AddWithValue("@ModeID", 0);
                                cmd.Parameters.AddWithValue("@UserDate", userDate);
                                cmd.Parameters.AddWithValue("@UserID", userId); // Use parsed integer
                                cmd.Parameters.AddWithValue("@CancelFlag", false);
                                cmd.Parameters.AddWithValue("@FinYearID", defaultFinYearId);
                                cmd.Parameters.AddWithValue("@IsSyncd", false);
                                cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                                System.Diagnostics.Debug.WriteLine($"Creating credit voucher entry: VoucherID={master.VoucherId}, LedgerID={master.CustomerLedgerId}, Credit={master.TotalReceiptAmount}");

                                var voucherResult = cmd.ExecuteScalar();
                                if (voucherResult == null || !voucherResult.ToString().StartsWith("SUCCESS"))
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                            }

                            // 6. SyncQueue Integration - Enqueue Customer Receipt (via Stored Procedure POS_SyncQueue)
                            try
                            {
                                Guid receiptGuid = Guid.NewGuid();
                                SyncQueueRepository.SetTransactionGuid(
                                    conn,
                                    transaction,
                                    "CUSTOMER_RECEIPT",
                                    master.ReceiptId.ToString(),
                                    receiptGuid);

                                SyncQueueRepository.EnqueueTransaction(
                                    conn,
                                    transaction,
                                    master.BranchId > 0 ? master.BranchId : SessionContext.BranchId,
                                    "CUSTOMER_RECEIPT",
                                    master.ReceiptId.ToString(),
                                    receiptGuid,
                                    "CREATE");
                            }
                            catch (Exception syncEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"[CustomerReceiptInfoRepository.SaveCustomerReceipt] SyncQueue error: {syncEx.Message}");
                            }

                        transaction.Commit();
                        return true;
                    }
                    catch (SqlException sqlEx)
                    {
                        transaction.Rollback();
                        throw new Exception($"SQL error while saving receipt: {sqlEx.Message}, SQL error code: {sqlEx.Number}", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Error while saving receipt: {ex.Message}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Connection error: {ex.Message}", ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        public DataTable GetReceiptHistory(int customerLedgerId, long billNo)
        {
            if (DataConnection.State == ConnectionState.Open)
                DataConnection.Close();

            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CustomerReceiptMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerLedgerId", customerLedgerId);
                    cmd.Parameters.AddWithValue("@BillNoUntil", billNo);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@_Operation", "VIEWRECEIPT");

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                    return dt;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        public decimal GetCustomerOutstandingTotal(int customerLedgerId)
        {
            decimal outstandingTotal = 0;
            if (DataConnection.State == ConnectionState.Open)
                DataConnection.Close();

            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CustomerReceiptMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerLedgerId", customerLedgerId);
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
                System.Diagnostics.Debug.WriteLine($"Error getting customer outstanding total: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return outstandingTotal;
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
                decimal receivedAmount = GetRowDecimal(row, "ReceivedAmount");
                decimal returnedAmount = invoices.Columns.Contains("ReturnedAmount") ? GetRowDecimal(row, "ReturnedAmount") : 0m;

                // Detect cash sales where full payment was settled at sale time (Cash = PaymodeID 2)
                bool isCashSale = false;
                if (invoices.Columns.Contains("Paymode") && row["Paymode"] != DBNull.Value)
                {
                    string pm = row["Paymode"].ToString().Trim().ToLower();
                    if (pm == "cash" || pm == "2") isCashSale = true;
                }
                if (invoices.Columns.Contains("PaymodeID") && row["PaymodeID"] != DBNull.Value)
                {
                    if (Convert.ToInt32(row["PaymodeID"]) == 2) isCashSale = true;
                }
                if (invoices.Columns.Contains("PayModeID") && row["PayModeID"] != DBNull.Value)
                {
                    if (Convert.ToInt32(row["PayModeID"]) == 2) isCashSale = true;
                }

                // Credit sales (PaymodeID = 1 / "credit") must NEVER be treated as cash sales
                if (invoices.Columns.Contains("Paymode") && row["Paymode"] != DBNull.Value)
                {
                    string pm = row["Paymode"].ToString().Trim().ToLower();
                    if (pm == "credit" || pm == "1") isCashSale = false;
                }
                if (invoices.Columns.Contains("PaymodeID") && row["PaymodeID"] != DBNull.Value)
                {
                    if (Convert.ToInt32(row["PaymodeID"]) == 1) isCashSale = false;
                }
                if (invoices.Columns.Contains("PayModeID") && row["PayModeID"] != DBNull.Value)
                {
                    if (Convert.ToInt32(row["PayModeID"]) == 1) isCashSale = false;
                }

                if (isCashSale)
                {
                    receivedAmount = invoiceAmount;
                }

                if (invoiceAmount < 0m)
                {
                    invoiceAmount = 0m;
                }

                if (receivedAmount < 0m)
                {
                    receivedAmount = 0m;
                }

                if (returnedAmount < 0m)
                {
                    returnedAmount = 0m;
                }

                if (invoiceAmount > 0m && receivedAmount > invoiceAmount)
                {
                    receivedAmount = invoiceAmount;
                }

                decimal balance = invoiceAmount - receivedAmount - returnedAmount;
                if (balance < 0m)
                {
                    balance = 0m;
                }

                if (invoices.Columns.Contains("InvoiceAmount"))
                {
                    row["InvoiceAmount"] = invoiceAmount;
                }

                if (invoices.Columns.Contains("ReceivedAmount"))
                {
                    row["ReceivedAmount"] = receivedAmount;
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

        private void EnhanceInvoiceTableWithCashPaymode(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0) return;

            if (!dt.Columns.Contains("Paymode"))
                dt.Columns.Add("Paymode", typeof(string));
            if (!dt.Columns.Contains("PaymodeID"))
                dt.Columns.Add("PaymodeID", typeof(int));

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_Sales_Win, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");

                    DataTable salesDt = new DataTable();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(salesDt);
                    }

                    if (salesDt != null && salesDt.Rows.Count > 0)
                    {
                        string paymodeCol = salesDt.Columns.Contains("Paymode") ? "Paymode" :
                                           salesDt.Columns.Contains("PayMode") ? "PayMode" :
                                           salesDt.Columns.Contains("PaymodeName") ? "PaymodeName" : null;

                        string paymodeIdCol = salesDt.Columns.Contains("PaymodeID") ? "PaymodeID" :
                                             salesDt.Columns.Contains("PayModeId") ? "PayModeId" :
                                             salesDt.Columns.Contains("PayModeID") ? "PayModeID" : null;

                        var pmIdMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                        var pmNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                        foreach (DataRow r in salesDt.Rows)
                        {
                            if (r.Table.Columns.Contains("BillNo") && r["BillNo"] != DBNull.Value)
                            {
                                string billNo = r["BillNo"].ToString().Trim();
                                string pm = paymodeCol != null && r[paymodeCol] != DBNull.Value ? r[paymodeCol].ToString().Trim() : "";
                                int pmId = paymodeIdCol != null && r[paymodeIdCol] != DBNull.Value ? Convert.ToInt32(r[paymodeIdCol]) : 0;
                                pmNameMap[billNo] = pm;
                                pmIdMap[billNo] = pmId;
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
                System.Diagnostics.Debug.WriteLine($"Error enhancing sales invoice table with paymode via stored procedure: {ex.Message}");
            }
        }

        private decimal GetRowDecimal(DataRow row, string columnName)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
            {
                return 0m;
            }

            try
            {
                return Convert.ToDecimal(row[columnName]);
            }
            catch
            {
                return 0m;
            }
        }

        public DataTable GetAllReceipts(int branchId)
        {
            DataTable dt = new DataTable();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CustomerReceiptMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@VoucherType", "CUSTRCPT");
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    cmd.Parameters.AddWithValue("@PageIndex", 0);
                    cmd.Parameters.AddWithValue("@PageSize", 1000); // Adjust as needed
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

        public long GetNavigationVoucherId(long currentVoucherId, int branchId, string operation)
        {
            long voucherId = 0;
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CustomerReceiptMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VoucherId", currentVoucherId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@_Operation", operation);

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        voucherId = Convert.ToInt64(result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return voucherId;
        }

        public DataSet GetReceiptDataByVoucherId(long voucherId, int branchId)
        {
            DataSet ds = new DataSet();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CustomerReceiptMaster, (SqlConnection)DataConnection))
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
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return ds;
        }



        /// <summary>
        /// Gets the actual cash ledger ID for the given payment method
        /// </summary>
        private int GetCashLedgerId(int paymentMethodId, int branchId, Repository.MasterRepositry.LedgerRepository ledgerRepository)
        {
            try
            {
                // For cash payments, get the actual CASH-IN-HAND ledger ID
                // This follows the same pattern as SalesRepository
                int cashLedgerId = ledgerRepository.GetLedgerId(ModelClass.DefaultLedgers.CASH, (int)ModelClass.AccountGroup.CASH_IN_HAND, branchId);
                
                if (cashLedgerId > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Found cash ledger ID: {cashLedgerId} for branch {branchId}");
                    return cashLedgerId;
                }
                else
                {
                    // Fallback: try to find cash ledger directly from database
                    System.Diagnostics.Debug.WriteLine("Could not find cash ledger via LedgerRepository, trying direct query");
                    return GetCashLedgerIdFromDatabase(branchId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting cash ledger ID: {ex.Message}");
                return GetCashLedgerIdFromDatabase(branchId);
            }
        }

        private void ValidateReceiptAmounts(CustomerReceiptMaster master, List<CustomerReceiptDetails> details, List<VoucherEntry> voucherEntries)
        {
            if (master.TotalReceiptAmount <= 0)
            {
                throw new InvalidOperationException("Receipt amount must be greater than zero.");
            }

            decimal detailTotal = Math.Round(details.Where(d => d != null).Sum(d => d.AdjustedAmount), 2);
            decimal receiptTotal = Math.Round(master.TotalReceiptAmount, 2);
            if (detailTotal != receiptTotal)
            {
                throw new InvalidOperationException("Receipt detail total must equal receipt amount.");
            }

            List<VoucherEntry> postingRows = voucherEntries
                .Where(v => v != null && (v.DebitAmount > 0 || v.CreditAmount > 0))
                .ToList();

            if (postingRows.Count < 2)
            {
                throw new InvalidOperationException("Receipt voucher must contain at least two posting rows.");
            }

            foreach (VoucherEntry row in postingRows)
            {
                if (row.DebitAmount < 0 || row.CreditAmount < 0)
                {
                    throw new InvalidOperationException("Receipt voucher debit and credit cannot be negative.");
                }

                if (row.DebitAmount > 0 && row.CreditAmount > 0)
                {
                    throw new InvalidOperationException("A receipt voucher row cannot contain both debit and credit.");
                }
            }

            decimal totalDebit = Math.Round(postingRows.Sum(v => v.DebitAmount), 2);
            decimal totalCredit = Math.Round(postingRows.Sum(v => v.CreditAmount), 2);

            if (totalDebit != totalCredit)
            {
                throw new InvalidOperationException("Receipt voucher is not balanced. Total debit must equal total credit.");
            }

            if (totalDebit != receiptTotal)
            {
                throw new InvalidOperationException("Receipt voucher total must equal receipt amount.");
            }
        }

        /// <summary>
        /// Fallback method to get cash ledger ID using stored procedure
        /// </summary>
        private int GetCashLedgerIdFromDatabase(int branchId)
        {
            try
            {
                // Use the same stored procedure pattern as other methods in this repository
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

        /// <summary>
        /// Delete a customer receipt (soft delete)
        /// </summary>
        public bool DeleteCustomerReceipt(int receiptId)
        {
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CustomerReceiptMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", receiptId);
                    cmd.Parameters.AddWithValue("@_Operation", "DELETE");

                    cmd.ExecuteNonQuery();
                    return true;
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

        /// <summary>
        /// Summary result returned upon customer receipt cancellation
        /// </summary>
        public class CustomerReceiptCancellationSummary
        {
            public int ReceiptVoucherCount { get; set; }
            public decimal ReceiptAmount { get; set; }
            public DateTime LastReceiptDate { get; set; }
        }

        /// <summary>
        /// Cancels/reverses a loaded customer receipt voucher in a transaction using stored procedures,
        /// restoring SMaster invoice balances and soft-deleting receipt & voucher records.
        /// </summary>
        public CustomerReceiptCancellationSummary CancelCustomerReceipt(int receiptMasterId, int branchId, int userId, string reason)
        {
            DataConnection.Open();
            SqlTransaction transaction = null;

            try
            {
                transaction = ((SqlConnection)DataConnection).BeginTransaction();

                DataTable activeDetails = new DataTable();
                long voucherId = 0;
                DateTime voucherDate = DateTime.MinValue;

                // 1. Fetch master info via Stored Procedure STOREDPROCEDURE._CustomerReceiptMaster
                try
                {
                    using (SqlCommand masterCmd = new SqlCommand(STOREDPROCEDURE._CustomerReceiptMaster, (SqlConnection)DataConnection, transaction))
                    {
                        masterCmd.CommandType = CommandType.StoredProcedure;
                        masterCmd.Parameters.AddWithValue("@VoucherId", receiptMasterId);
                        masterCmd.Parameters.AddWithValue("@BranchId", branchId);
                        masterCmd.Parameters.AddWithValue("@_Operation", "GETBYID");

                        using (SqlDataAdapter adapter = new SqlDataAdapter(masterCmd))
                        {
                            DataSet dsMaster = new DataSet();
                            adapter.Fill(dsMaster);
                            if (dsMaster != null && dsMaster.Tables.Count > 0 && dsMaster.Tables[0].Rows.Count > 0)
                            {
                                DataRow masterRow = dsMaster.Tables[0].Rows[0];
                                voucherId = masterRow.Table.Columns.Contains("VoucherId") && masterRow["VoucherId"] != DBNull.Value ? Convert.ToInt64(masterRow["VoucherId"]) : 0;
                                voucherDate = masterRow.Table.Columns.Contains("VoucherDate") && masterRow["VoucherDate"] != DBNull.Value ? Convert.ToDateTime(masterRow["VoucherDate"]) : DateTime.MinValue;
                            }
                        }
                    }
                }
                catch
                {
                    // Fallback query if SP parameter differs
                }

                if (voucherId <= 0)
                {
                    throw new Exception("This customer receipt is already cancelled or could not be found.");
                }

                // 2. Fetch details via Stored Procedure STOREDPROCEDURE._CustomerReceiptDetails
                using (SqlCommand detailsCmd = new SqlCommand(STOREDPROCEDURE._CustomerReceiptDetails, (SqlConnection)DataConnection, transaction))
                {
                    detailsCmd.CommandType = CommandType.StoredProcedure;
                    detailsCmd.Parameters.AddWithValue("@ReceiptMasterId", receiptMasterId);
                    detailsCmd.Parameters.AddWithValue("@BranchId", branchId);
                    detailsCmd.Parameters.AddWithValue("@_Operation", "GETBYMASTERID");

                    using (SqlDataAdapter adapter = new SqlDataAdapter(detailsCmd))
                    {
                        adapter.Fill(activeDetails);
                    }
                }

                if (activeDetails.Rows.Count == 0)
                    throw new Exception("No active receipt allocations were found for this voucher.");

                decimal totalReversed = 0m;
                foreach (DataRow detailRow in activeDetails.Rows)
                {
                    if (detailRow.Table.Columns.Contains("ReceiptAmount") && detailRow["ReceiptAmount"] != DBNull.Value)
                    {
                        totalReversed += Convert.ToDecimal(detailRow["ReceiptAmount"]);
                    }
                }

                // 3. Cancel details using Stored Procedure STOREDPROCEDURE._CustomerReceiptDetails
                using (SqlCommand cancelDetailsSp = new SqlCommand(STOREDPROCEDURE._CustomerReceiptDetails, (SqlConnection)DataConnection, transaction))
                {
                    cancelDetailsSp.CommandType = CommandType.StoredProcedure;
                    cancelDetailsSp.Parameters.AddWithValue("@ReceiptMasterId", receiptMasterId);
                    cancelDetailsSp.Parameters.AddWithValue("@_Operation", "CANCEL");
                    cancelDetailsSp.ExecuteNonQuery();
                }

                // 4. Cancel Master using Stored Procedure STOREDPROCEDURE._CustomerReceiptMaster
                string cancelNote = " | Cancelled";
                if (!string.IsNullOrWhiteSpace(reason))
                    cancelNote += ": " + reason.Trim();

                using (SqlCommand cancelMasterSp = new SqlCommand(STOREDPROCEDURE._CustomerReceiptMaster, (SqlConnection)DataConnection, transaction))
                {
                    cancelMasterSp.CommandType = CommandType.StoredProcedure;
                    cancelMasterSp.Parameters.AddWithValue("@Id", receiptMasterId);
                    cancelMasterSp.Parameters.AddWithValue("@Narration", cancelNote);
                    cancelMasterSp.Parameters.AddWithValue("@_Operation", "CANCEL");
                    cancelMasterSp.ExecuteNonQuery();
                }

                // 5. Cancel Vouchers using Stored Procedure STOREDPROCEDURE.POS_Vouchers
                using (SqlCommand cancelVoucherSp = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, (SqlConnection)DataConnection, transaction))
                {
                    cancelVoucherSp.CommandType = CommandType.StoredProcedure;
                    cancelVoucherSp.Parameters.AddWithValue("@BranchID", branchId);
                    cancelVoucherSp.Parameters.AddWithValue("@VoucherID", voucherId);
                    cancelVoucherSp.Parameters.AddWithValue("@VoucherType", "CUSTRCPT");
                    cancelVoucherSp.Parameters.AddWithValue("@_Operation", "CANCEL");
                    cancelVoucherSp.ExecuteNonQuery();
                }

                // SyncQueue Integration - Enqueue Customer Receipt Cancellation
                try
                {
                    Guid? existingGuid = SyncQueueRepository.GetExistingGuid(
                        (SqlConnection)DataConnection,
                        transaction,
                        branchId > 0 ? branchId : SessionContext.BranchId,
                        "CUSTOMER_RECEIPT",
                        receiptMasterId.ToString()) ?? Guid.NewGuid();

                    SyncQueueRepository.EnqueueTransaction(
                        (SqlConnection)DataConnection,
                        transaction,
                        branchId > 0 ? branchId : SessionContext.BranchId,
                        "CUSTOMER_RECEIPT",
                        receiptMasterId.ToString(),
                        existingGuid.Value,
                        "CANCEL");
                }
                catch (Exception syncEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[CustomerReceiptInfoRepository.CancelCustomerReceipt] SyncQueue error: {syncEx.Message}");
                }

                transaction.Commit();
                return new CustomerReceiptCancellationSummary
                {
                    ReceiptVoucherCount = 1,
                    ReceiptAmount = totalReversed,
                    LastReceiptDate = voucherDate
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
    }
}

