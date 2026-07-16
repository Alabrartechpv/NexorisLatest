using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ModelClass;

namespace Repository
{
    /// <summary>
    /// Enhanced Closing Repository with ShiftClosing integration
    /// </summary>
    public class ClosingRepo : BaseRepostitory
    {
        /// <summary>
        /// Get default denominations (Indian Rupee)
        /// </summary>
        public List<CashDetail> GetDefaultDenominations()
        {
            return new List<CashDetail>
            {
                new CashDetail { No = 1, Denomination = 500.00m },
                new CashDetail { No = 2, Denomination = 200.00m },
                new CashDetail { No = 3, Denomination = 100.00m },
                new CashDetail { No = 4, Denomination = 50.00m },
                new CashDetail { No = 5, Denomination = 20.00m },
                new CashDetail { No = 6, Denomination = 10.00m },
                new CashDetail { No = 7, Denomination = 5.00m },
                new CashDetail { No = 8, Denomination = 2.00m },
                new CashDetail { No = 9, Denomination = 1.00m },
                new CashDetail { No = 10, Denomination = 0.50m },
                new CashDetail { No = 11, Denomination = 0.25m },
                new CashDetail { No = 12, Denomination = 0.10m }
            };
        }

        /// <summary>
        /// Get Sales Data Summary from POS
        /// </summary>
        public SalesDataSummary GetSalesDataSummary(DateTime closingDate, string counter)
        {
            SalesDataSummary summary = new SalesDataSummary();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ShiftClosing, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETSALESDATASUMMARY");
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@ClosingDate", closingDate.Date);
                    cmd.Parameters.AddWithValue("@UserId", SessionContext.UserId);
                    cmd.Parameters.AddWithValue("@Counter", counter);
                    cmd.Parameters.AddWithValue("@CounterSessionId", SessionContext.CounterSessionId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            summary.TotalGrossSales = GetDecimal(reader, "TotalGrossSales");
                            summary.TotalDiscount = GetDecimal(reader, "TotalDiscount");
                            summary.TotalReturn = GetDecimal(reader, "TotalReturn");
                            summary.NetSales = GetDecimal(reader, "NetSales");
                            summary.CashSale = GetDecimal(reader, "CashSale");
                            summary.CardSale = GetDecimal(reader, "CardSale");
                            summary.UpiSale = GetDecimal(reader, "UpiSale");
                            summary.CreditSale = GetDecimal(reader, "CreditSale");
                            summary.TotalCollection = GetDecimal(reader, "TotalCollection");
                            summary.TotalBills = GetInt(reader, "TotalBills");
                            summary.CashBills = GetInt(reader, "CashBills");
                            summary.CardBills = GetInt(reader, "CardBills");
                            summary.UpiBills = GetInt(reader, "UpiBills");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting sales summary: {ex.Message}");
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return summary;
        }

        /// <summary>
        /// Get Customer Receipt Summary
        /// </summary>
        public CustomerReceiptSummary GetCustomerReceiptSummary(DateTime closingDate)
        {
            CustomerReceiptSummary summary = new CustomerReceiptSummary();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ShiftClosing, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETCUSTOMERRECEIPTS");
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@ClosingDate", closingDate.Date);
                    cmd.Parameters.AddWithValue("@UserId", SessionContext.UserId);
                    cmd.Parameters.AddWithValue("@CounterSessionId", SessionContext.CounterSessionId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            summary.CashReceipt = GetDecimal(reader, "CashReceipt");
                            summary.CardReceipt = GetDecimal(reader, "CardReceipt");
                            summary.UpiReceipt = GetDecimal(reader, "UpiReceipt");
                            summary.TotalReceipt = GetDecimal(reader, "TotalReceipt");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting receipt summary: {ex.Message}");
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return summary;
        }

        private decimal GetDecimal(IDataRecord record, string columnName)
        {
            int ordinal = GetOrdinal(record, columnName);
            if (ordinal < 0 || record.IsDBNull(ordinal))
            {
                return 0;
            }

            decimal value;
            return decimal.TryParse(record.GetValue(ordinal).ToString(), out value) ? value : 0;
        }

        private int GetInt(IDataRecord record, string columnName)
        {
            int ordinal = GetOrdinal(record, columnName);
            if (ordinal < 0 || record.IsDBNull(ordinal))
            {
                return 0;
            }

            int value;
            return int.TryParse(record.GetValue(ordinal).ToString(), out value) ? value : 0;
        }

        private int GetOrdinal(IDataRecord record, string columnName)
        {
            for (int i = 0; i < record.FieldCount; i++)
            {
                if (string.Equals(record.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Save Closing with Transaction Safety
        /// </summary>
        public bool SaveClosing(ClosingModel model)
        {
            if (model == null)
                return false;

            SqlTransaction transaction = null;

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                transaction = ((SqlConnection)DataConnection).BeginTransaction();

                // 1. Generate VoucherId FIRST (like other forms)
                int voucherId = GenerateVoucherNumber(transaction);
                if (voucherId <= 0)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to generate VoucherID for closing");
                    transaction.Rollback();
                    throw new Exception("Failed to generate VoucherID. Cannot save closing.");
                }

                model.VoucherId = voucherId;
                System.Diagnostics.Debug.WriteLine($"Generated VoucherID for closing: {voucherId}");

                // 2. Save Closing Master (with VoucherId)
                int shiftClosingId = SaveClosingMaster(model, transaction);
                if (shiftClosingId <= 0)
                {
                    transaction.Rollback();
                    return false;
                }

                model.ShiftClosingId = shiftClosingId;
                System.Diagnostics.Debug.WriteLine($"Saved closing master with ID: {shiftClosingId}");

                // 3. Save Cash Denominations
                bool denominationsSaved = SaveClosingDetails(shiftClosingId, model.CashDetails, transaction);
                if (!denominationsSaved)
                {
                    transaction.Rollback();
                    return false;
                }

                // 4. Create Voucher Entries (if closed)
                if (model.Status == "Closed")
                {
                    CreateVoucherEntries(voucherId, model, transaction);
                }

                if (SessionContext.CounterSessionId > 0)
                {
                    CloseCounterSession(shiftClosingId, transaction);
                }

                transaction.Commit();
                System.Diagnostics.Debug.WriteLine($"Closing saved successfully - ID: {shiftClosingId}, VoucherID: {voucherId}");
                return true;
            }
            catch (Exception ex)
            {
                if (transaction != null)
                    transaction.Rollback();

                System.Diagnostics.Debug.WriteLine($"Error saving closing: {ex.Message}");
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        private int SaveClosingMaster(ClosingModel model, SqlTransaction transaction)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ShiftClosing, (SqlConnection)DataConnection, transaction))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "CREATE");
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@Counter", model.Counter ?? "");
                    cmd.Parameters.AddWithValue("@CounterSessionId", SessionContext.CounterSessionId);
                    cmd.Parameters.AddWithValue("@UserId", SessionContext.UserId);
                    cmd.Parameters.AddWithValue("@ClosingDate", model.TransactionDate);
                    cmd.Parameters.AddWithValue("@ReportSelection", model.ReportSelection ?? "");
                    cmd.Parameters.AddWithValue("@DocNo", model.DocNo ?? "");

                    // Sales Summary
                    cmd.Parameters.AddWithValue("@TotalGrossSales", model.TotalGrossSales);
                    cmd.Parameters.AddWithValue("@TotalDiscount", model.TotalDiscount);
                    cmd.Parameters.AddWithValue("@TotalReturn", model.TotalReturn);
                    cmd.Parameters.AddWithValue("@NetSales", model.NetSales);

                    // Payment Collection
                    cmd.Parameters.AddWithValue("@CashSale", model.CashSale);
                    cmd.Parameters.AddWithValue("@CardSale", model.CardSale);
                    cmd.Parameters.AddWithValue("@UpiSale", model.UpiSale);
                    cmd.Parameters.AddWithValue("@CreditSale", model.CreditSale);
                    cmd.Parameters.AddWithValue("@CustomerReceipt", model.CustomerReceipt);
                    cmd.Parameters.AddWithValue("@TotalCollection", model.TotalCollection);

                    // Cash Drawer
                    cmd.Parameters.AddWithValue("@CashRefundAdjusted", model.CashRefundAdjusted);
                    cmd.Parameters.AddWithValue("@MidDayCashSkim", model.MidDayCashSkim);
                    cmd.Parameters.AddWithValue("@SystemExpectedCash", model.SystemExpectedCash);

                    // Physical Count
                    cmd.Parameters.AddWithValue("@PhysicalCashCounted", model.PhysicalCashCounted);
                    cmd.Parameters.AddWithValue("@CashDifference", model.CashDifference);
                    cmd.Parameters.AddWithValue("@DifferenceReason", model.DifferenceReason ?? "");

                    cmd.Parameters.AddWithValue("@Status", model.Status ?? "Closed");
                    cmd.Parameters.AddWithValue("@VoucherId", model.VoucherId);
                    cmd.Parameters.AddWithValue("@CreatedBy", SessionContext.UserId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string result = reader["Result"].ToString();
                            if (result == "SUCCESS")
                            {
                                return Convert.ToInt32(reader["ShiftClosingId"]);
                            }
                            else if (result == "ALREADY_CLOSED")
                            {
                                throw new Exception("Closing already done for today. Cannot close again.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving closing master: {ex.Message}");
                throw;
            }

            return 0;
        }

        private void CloseCounterSession(int shiftClosingId, SqlTransaction transaction)
        {
            using (SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.CounterSessions
SET Status = 'Closed',
    CloseTime = GETDATE(),
    ShiftClosingId = @ShiftClosingId,
    ModifiedDate = GETDATE()
WHERE SessionId = @SessionId
  AND Status = 'Open';", (SqlConnection)DataConnection, transaction))
            {
                cmd.Parameters.AddWithValue("@ShiftClosingId", shiftClosingId);
                cmd.Parameters.AddWithValue("@SessionId", SessionContext.CounterSessionId);
                cmd.ExecuteNonQuery();
            }
        }

        private bool SaveClosingDetails(int shiftClosingId, List<CashDetail> cashDetails, SqlTransaction transaction)
        {
            try
            {
                // Delete existing denominations first
                using (SqlCommand cmdDelete = new SqlCommand(STOREDPROCEDURE.POS_ShiftClosingDenominations, (SqlConnection)DataConnection, transaction))
                {
                    cmdDelete.CommandType = CommandType.StoredProcedure;
                    cmdDelete.Parameters.AddWithValue("@_Operation", "DELETEALL");
                    cmdDelete.Parameters.AddWithValue("@ShiftClosingId", shiftClosingId);
                    cmdDelete.ExecuteNonQuery();
                }

                // Save new denominations (only non-zero quantities)
                foreach (var detail in cashDetails.Where(d => d.Quantity > 0))
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ShiftClosingDenominations, (SqlConnection)DataConnection, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@_Operation", "CREATE");
                        cmd.Parameters.AddWithValue("@ShiftClosingId", shiftClosingId);
                        cmd.Parameters.AddWithValue("@No", detail.No);
                        cmd.Parameters.AddWithValue("@Denomination", detail.Denomination);
                        cmd.Parameters.AddWithValue("@Quantity", detail.Quantity);
                        cmd.Parameters.AddWithValue("@Amount", detail.Amount);

                        cmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving closing details: {ex.Message}");
                return false;
            }
        }

        private int GenerateVoucherNumber(SqlTransaction transaction)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, (SqlConnection)DataConnection, transaction))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GENERATENUMBER");
                    cmd.Parameters.AddWithValue("@CompanyID", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchID", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearID", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@VoucherType", "ShiftClosing");

                    System.Diagnostics.Debug.WriteLine($"Generating VoucherID - CompanyID: {SessionContext.CompanyId}, BranchID: {SessionContext.BranchId}, FinYearID: {SessionContext.FinYearId}, VoucherType: ShiftClosing");

                    // Use ExecuteScalar to get the result (like SalesReturnRepository pattern)
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value && int.TryParse(result.ToString(), out int voucherId))
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ Generated VoucherID: {voucherId} for Shift Closing");
                        return voucherId;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("❌ POS_Vouchers returned null or invalid result");
                        throw new Exception("Failed to generate VoucherID for Shift Closing");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error generating voucher number: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        private bool CreateVoucherEntries(int voucherId, ClosingModel model, SqlTransaction transaction)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== CreateVoucherEntries START - VoucherID: {voucherId} ===");

                // Get CASH-IN-HAND ledger (LedgerID=258, GroupID=14) for credit
                int cashInHandLedgerId = GetCashLedgerId();
                System.Diagnostics.Debug.WriteLine($"CASH-IN-HAND Ledger ID: {cashInHandLedgerId}");

                // Get CASH EXCESS OR SHORT ledger (LedgerID=500, GroupID=13) for debit
                int cashExcessShortLedgerId = GetClosingLedgerId();
                System.Diagnostics.Debug.WriteLine($"CASH EXCESS OR SHORT Ledger ID: {cashExcessShortLedgerId}");

                if (cashInHandLedgerId <= 0 || cashExcessShortLedgerId <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ ERROR: Invalid ledger IDs - CASH-IN-HAND: {cashInHandLedgerId}, CASH EXCESS OR SHORT: {cashExcessShortLedgerId}");
                    System.Diagnostics.Debug.WriteLine("⚠️ SKIPPING voucher entry creation - ledgers not configured");
                    // Don't fail the entire closing if ledgers aren't configured
                    return true;
                }

                decimal difference = model.CashDifference;

                if (difference == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No cash difference. Skipping voucher entries.");
                    return true;
                }

                string narration = $"Shift Closing - {model.Counter} - {model.TransactionDate:dd-MMM-yyyy} - Variance: {difference}";

                if (difference > 0)
                {
                    // Excess Cash: Debit CASH-IN-HAND, Credit CASH EXCESS OR SHORT
                    System.Diagnostics.Debug.WriteLine($"Creating Debit Entry - CASH-IN-HAND - LedgerID: {cashInHandLedgerId}, Debit: {difference}");
                    CreateVoucherEntry(voucherId, cashInHandLedgerId, difference, 0, 1, narration, transaction);

                    System.Diagnostics.Debug.WriteLine($"Creating Credit Entry - CASH EXCESS OR SHORT - LedgerID: {cashExcessShortLedgerId}, Credit: {difference}");
                    CreateVoucherEntry(voucherId, cashExcessShortLedgerId, 0, difference, 2, narration, transaction);
                }
                else
                {
                    // Short Cash: Debit CASH EXCESS OR SHORT, Credit CASH-IN-HAND
                    decimal absDifference = Math.Abs(difference);
                    System.Diagnostics.Debug.WriteLine($"Creating Debit Entry - CASH EXCESS OR SHORT - LedgerID: {cashExcessShortLedgerId}, Debit: {absDifference}");
                    CreateVoucherEntry(voucherId, cashExcessShortLedgerId, absDifference, 0, 1, narration, transaction);

                    System.Diagnostics.Debug.WriteLine($"Creating Credit Entry - CASH-IN-HAND - LedgerID: {cashInHandLedgerId}, Credit: {absDifference}");
                    CreateVoucherEntry(voucherId, cashInHandLedgerId, 0, absDifference, 2, narration, transaction);
                }

                System.Diagnostics.Debug.WriteLine($"✅ Voucher entries created successfully - VoucherID: {voucherId}");
                System.Diagnostics.Debug.WriteLine($"=== CreateVoucherEntries END ===");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR creating voucher entries: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                // Don't fail the entire closing if voucher entries fail
                return true;
            }
        }

        private void CreateVoucherEntry(int voucherId, int ledgerId, decimal debit, decimal credit,
            int slNo, string narration, SqlTransaction transaction)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"  >> CreateVoucherEntry - VoucherID: {voucherId}, LedgerID: {ledgerId}, SlNo: {slNo}");

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, (SqlConnection)DataConnection, transaction))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Use CREATE operation like Purchase/Sales repositories
                    cmd.Parameters.AddWithValue("@_Operation", "CREATE");
                    cmd.Parameters.AddWithValue("@CompanyID", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchID", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@VoucherID", voucherId);
                    cmd.Parameters.AddWithValue("@VoucherSeriesID", 0);
                    cmd.Parameters.AddWithValue("@VoucherDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@VoucherNumber", DBNull.Value);
                    cmd.Parameters.AddWithValue("@LedgerID", ledgerId);
                    cmd.Parameters.AddWithValue("@LedgerName", ""); // Will be filled by SP
                    cmd.Parameters.AddWithValue("@VoucherType", "ShiftClosing");
                    cmd.Parameters.AddWithValue("@GroupID", 0); // Will be filled by SP based on LedgerID
                    cmd.Parameters.AddWithValue("@Debit", Convert.ToDouble(debit));
                    cmd.Parameters.AddWithValue("@Credit", Convert.ToDouble(credit));
                    cmd.Parameters.AddWithValue("@Narration", narration);
                    cmd.Parameters.AddWithValue("@SlNo", slNo);
                    cmd.Parameters.AddWithValue("@Mode", "");
                    cmd.Parameters.AddWithValue("@ModeID", 0);
                    cmd.Parameters.AddWithValue("@UserDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@UserName", SessionContext.UserName ?? "");
                    cmd.Parameters.AddWithValue("@UserID", SessionContext.UserId);
                    cmd.Parameters.AddWithValue("@CancelFlag", false);
                    cmd.Parameters.AddWithValue("@FinYearID", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@IsSyncd", false);

                    System.Diagnostics.Debug.WriteLine($"  >> Executing POS_Vouchers CREATE...");
                    int rowsAffected = cmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"  >> ✅ Rows affected: {rowsAffected}");

                    if (rowsAffected == 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"  >> ⚠️ WARNING: No rows affected - voucher entry may not have been created!");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"  >> ❌ ERROR in CreateVoucherEntry: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"  >> Stack: {ex.StackTrace}");
                throw; // Re-throw to be caught by CreateVoucherEntries
            }
        }

        private int GetCashLedgerId()
        {
            try
            {
                // Get CASH-IN-HAND ledger (LedgerID=258, GroupID=14)
                // First try the specific ledger ID and group ID combination
                var ledgerRepo = new MasterRepositry.LedgerRepository();

                // Try to get CASH-IN-HAND with GroupID=14 first
                int ledgerId = ledgerRepo.GetLedgerId("CASH-IN-HAND", 14, SessionContext.BranchId);
                if (ledgerId > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Found CASH-IN-HAND Ledger: ID={ledgerId}, GroupID=14");
                    return ledgerId;
                }

                // Fallback: Try other common cash ledger names with various group IDs
                string[] cashLedgerNames = { "CASH-IN-HAND", "CASH", "CASH ACCOUNT" };

                foreach (var ledgerName in cashLedgerNames)
                {
                    // Try with different group IDs (14 = typical for cash accounts)
                    for (int groupId = 1; groupId <= 20; groupId++)
                    {
                        ledgerId = ledgerRepo.GetLedgerId(ledgerName, groupId, SessionContext.BranchId);
                        if (ledgerId > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"Found Cash Ledger: {ledgerName}, ID: {ledgerId}, GroupID: {groupId}");
                            return ledgerId;
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine("CASH-IN-HAND ledger not found - voucher entries will be skipped");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting cash ledger ID: {ex.Message}");
            }

            // Return 0 if not found (voucher entries will be skipped)
            return 0;
        }

        /// <summary>
        /// Get Shift Closing History
        /// </summary>
        public List<ClosingModel> GetShiftHistory()
        {
            List<ClosingModel> historyList = new List<ClosingModel>();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ShiftClosing, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "SHIFTHISTORY");

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var model = new ClosingModel
                            {
                                ShiftClosingId = reader["ShiftClosingId"] != DBNull.Value ? Convert.ToInt32(reader["ShiftClosingId"]) : 0,
                                TransactionDate = reader["ClosingDate"] != DBNull.Value ? Convert.ToDateTime(reader["ClosingDate"]) : DateTime.MinValue,
                                DocNo = reader["DocNo"]?.ToString() ?? "",
                                TotalCollection = reader["TotalCollection"] != DBNull.Value ? Convert.ToDecimal(reader["TotalCollection"]) : 0,
                                TotalGrossSales = reader["TotalGrossSales"] != DBNull.Value ? Convert.ToDecimal(reader["TotalGrossSales"]) : 0,
                                TotalReturn = reader["TotalReturn"] != DBNull.Value ? Convert.ToDecimal(reader["TotalReturn"]) : 0,
                                NetSales = reader["NetSales"] != DBNull.Value ? Convert.ToDecimal(reader["NetSales"]) : 0,
                                CashSale = reader["CashSale"] != DBNull.Value ? Convert.ToDecimal(reader["CashSale"]) : 0,
                                CardSale = reader["CardSale"] != DBNull.Value ? Convert.ToDecimal(reader["CardSale"]) : 0,
                                UpiSale = reader["UpiSale"] != DBNull.Value ? Convert.ToDecimal(reader["UpiSale"]) : 0,
                                CreditSale = reader["CreditSale"] != DBNull.Value ? Convert.ToDecimal(reader["CreditSale"]) : 0,
                                CustomerReceipt = reader["CustomerReceipt"] != DBNull.Value ? Convert.ToDecimal(reader["CustomerReceipt"]) : 0,
                                SystemExpectedCash = reader["SystemExpectedCash"] != DBNull.Value ? Convert.ToDecimal(reader["SystemExpectedCash"]) : 0,
                                PhysicalCashCounted = reader["PhysicalCashCounted"] != DBNull.Value ? Convert.ToDecimal(reader["PhysicalCashCounted"]) : 0,
                                CashDifference = reader["CashDifference"] != DBNull.Value ? Convert.ToDecimal(reader["CashDifference"]) : 0,
                                Status = reader["Status"]?.ToString() ?? "",
                                Counter = reader["Counter"]?.ToString() ?? ""
                            };

                            historyList.Add(model);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting shift history: {ex.Message}");
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return historyList;
        }

        /// <summary>
        /// Get shift closing denominations by shift closing ID
        /// </summary>
        public List<CashDetail> GetClosingDenominations(int shiftClosingId)
        {
            List<CashDetail> details = new List<CashDetail>();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ShiftClosing, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");
                    cmd.Parameters.AddWithValue("@ShiftClosingId", shiftClosingId);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapter.Fill(ds);

                        if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                        {
                            foreach (DataRow row in ds.Tables[1].Rows)
                            {
                                details.Add(new CashDetail
                                {
                                    ShiftClosingId = Convert.ToInt32(row["ShiftClosingId"]),
                                    No = Convert.ToInt32(row["No"]),
                                    Denomination = Convert.ToDecimal(row["Denomination"]),
                                    Quantity = Convert.ToInt32(row["Quantity"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting closing denominations: {ex.Message}");
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return details;
        }


        private int GetClosingLedgerId()
        {
            try
            {
                // Get CASH EXCESS OR SHORT ledger (LedgerID=500, GroupID=13)
                // First try the specific ledger ID and group ID combination
                var ledgerRepo = new MasterRepositry.LedgerRepository();

                // Try to get CASH EXCESS OR SHORT with GroupID=13 first
                int ledgerId = ledgerRepo.GetLedgerId("CASH EXCESS OR SHORT", 13, SessionContext.BranchId);
                if (ledgerId > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Found CASH EXCESS OR SHORT Ledger: ID={ledgerId}, GroupID=13");
                    return ledgerId;
                }

                // Fallback: Try variations of the ledger name
                string[] closingLedgerNames = { "CASH EXCESS OR SHORT", "CASH EXCESS/SHORT", "CASH EXCESS SHORT", "CASH SHORT" };

                foreach (var ledgerName in closingLedgerNames)
                {
                    // Try with different group IDs (13 is typical for indirect expenses)
                    for (int groupId = 1; groupId <= 20; groupId++)
                    {
                        ledgerId = ledgerRepo.GetLedgerId(ledgerName, groupId, SessionContext.BranchId);
                        if (ledgerId > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"Found Closing Ledger: {ledgerName}, ID: {ledgerId}, GroupID: {groupId}");
                            return ledgerId;
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine("CASH EXCESS OR SHORT ledger not found - voucher entries will be skipped");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting CASH EXCESS OR SHORT ledger ID: {ex.Message}");
            }

            // Return 0 if not found (voucher entries will be skipped)
            return 0;
        }
    }
}
