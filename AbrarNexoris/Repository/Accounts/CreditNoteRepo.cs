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
    public class CreditNoteRepository : BaseRepostitory
    {
        /// <summary>
        /// Generate a new voucher ID for Credit Note
        /// </summary>
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
                    cmd.Parameters.AddWithValue("@VoucherType", "Credit Note");
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

        /// <summary>
        /// Get outstanding invoices for customer that can be credited
        /// </summary>
        public DataTable GetOutstandingInvoices(int customerId, int branchId)
        {
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CreditNoteMaster, (SqlConnection)DataConnection))
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

        public DataRow GetInvoiceByBillNo(string billNo, int customerId, int branchId)
        {
            if (DataConnection.State == ConnectionState.Open)
                DataConnection.Close();

            DataConnection.Open();
            try
            {
                string query = "SELECT BillNo, BillDate, DueDate, NetAmount AS InvoiceAmount, ISNULL(ReceivedAmount, 0) AS ReceivedAmount, " +
                               "ISNULL((SELECT SUM(GrandTotal) FROM SReturnMaster WHERE InvoiceNo = CAST(SMaster.BillNo AS varchar(50)) AND BranchId = @BranchId AND CompanyId = SMaster.CompanyId AND FinYearId = SMaster.FinYearId AND CancelFlag = 0), 0) AS ReturnedAmount, " +
                               "CASE WHEN (NetAmount - ISNULL(ReceivedAmount, 0)) < 0 THEN 0 " +
                               "ELSE (NetAmount - ISNULL(ReceivedAmount, 0)) END AS Balance " +
                               "FROM SMaster WHERE BillNo = @BillNo AND LedgerID = @LedgerID AND BranchId = @BranchId AND CancelFlag = 0";
                using (SqlCommand cmd = new SqlCommand(query, (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@BillNo", billNo);
                    cmd.Parameters.AddWithValue("@LedgerID", customerId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                    if (dt.Rows.Count > 0)
                        return dt.Rows[0];
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return null;
        }

        /// <summary>
        /// Get all invoices for customer
        /// </summary>
        public DataTable GetAllInvoices(int customerId, int branchId)
        {
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CreditNoteMaster, (SqlConnection)DataConnection))
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

        /// <summary>
        /// Get customer outstanding total
        /// </summary>
        public decimal GetCustomerOutstandingTotal(int customerLedgerId, int branchId)
        {
            decimal outstandingTotal = 0;
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CreditNoteMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerLedgerId", customerLedgerId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
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
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return outstandingTotal;
        }

        /// <summary>
        /// Save Credit Note - creates master, details, and voucher entries
        /// </summary>
        /// <param name="master">Credit Note master record</param>
        /// <param name="details">Credit Note detail records</param>
        /// <param name="skipVoucherCreation">If true, skip voucher creation (used when coming from Sales Return which already created vouchers)</param>
        public bool SaveCreditNote(CreditNoteMaster master, List<CreditNoteDetails> details, bool skipVoucherCreation = false)
        {
            if (master == null)
                return false;

            // details may be null or empty for fully-paid invoice returns.
            // In that case the credit amount is saved on the master only and
            // remains available as store credit for the customer.
            if (details == null)
                details = new List<CreditNoteDetails>();

            using (SqlConnection conn = (SqlConnection)DataConnection)
            {
                try
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            int finYearId = SessionContext.FinYearId;

                            // 1. Generate Voucher Number if not provided
                            if (master.VoucherId <= 0)
                            {
                                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, conn, transaction))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("@BranchID", master.BranchId);
                                    cmd.Parameters.AddWithValue("@FinYearID", finYearId);
                                    cmd.Parameters.AddWithValue("@VoucherType", "Credit Note");
                                    cmd.Parameters.AddWithValue("@_Operation", "GENERATENUMBER");

                                    object result = cmd.ExecuteScalar();
                                    if (result != null && result != DBNull.Value)
                                    {
                                        master.VoucherId = Convert.ToInt32(result);
                                    }
                                    else
                                    {
                                        transaction.Rollback();
                                        return false;
                                    }
                                }
                            }

                             // 2. Insert or Update into CreditNoteMaster
                             bool exists = false;
                             int existingId = 0;
                             if (master.SReturnNo > 0)
                             {
                                 using (SqlCommand checkCmd = new SqlCommand(STOREDPROCEDURE._CreditNoteMaster, conn, transaction))
                                 {
                                     checkCmd.CommandType = CommandType.StoredProcedure;
                                     checkCmd.Parameters.AddWithValue("@SReturnNo", master.SReturnNo);
                                     checkCmd.Parameters.AddWithValue("@BranchId", master.BranchId);
                                     checkCmd.Parameters.AddWithValue("@FinYearId", finYearId);
                                     checkCmd.Parameters.AddWithValue("@_Operation", "GETMASTERBYSR");
                                     using (var checkReader = checkCmd.ExecuteReader())
                                     {
                                         if (checkReader.Read())
                                         {
                                             exists = true;
                                             existingId = Convert.ToInt32(checkReader["Id"]);
                                             master.VoucherId = Convert.ToInt32(checkReader["VoucherId"]);
                                         }
                                     }
                                 }
                             }

                            if (exists)
                            {
                                master.Id = existingId;
                                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CreditNoteMaster, conn, transaction))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("@Id", master.Id);
                                    cmd.Parameters.AddWithValue("@CompanyId", master.CompanyId);
                                    cmd.Parameters.AddWithValue("@BranchId", master.BranchId);
                                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                                    cmd.Parameters.AddWithValue("@VoucherId", master.VoucherId);
                                    cmd.Parameters.AddWithValue("@CustomerLedgerId", master.CustomerLedgerId);
                                    cmd.Parameters.AddWithValue("@CreditAmount", master.CreditAmount);
                                    cmd.Parameters.AddWithValue("@PaymentMethodLedgerId", master.PaymentMethodLedgerId);
                                    cmd.Parameters.AddWithValue("@Narration", master.Narration ?? "");
                                    cmd.Parameters.AddWithValue("@_Operation", "UPDATE");

                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CreditNoteMaster, conn, transaction))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("@CompanyId", master.CompanyId);
                                    cmd.Parameters.AddWithValue("@BranchId", master.BranchId);
                                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                                    cmd.Parameters.AddWithValue("@VoucherId", master.VoucherId);
                                    cmd.Parameters.AddWithValue("@VoucherDate", master.VoucherDate);
                                    cmd.Parameters.AddWithValue("@CustomerLedgerId", master.CustomerLedgerId);
                                    cmd.Parameters.AddWithValue("@SReturnNo", master.SReturnNo);
                                    cmd.Parameters.AddWithValue("@InvoiceNo", master.InvoiceNo ?? "");
                                    cmd.Parameters.AddWithValue("@CreditAmount", master.CreditAmount);
                                    cmd.Parameters.AddWithValue("@PaymentMethodLedgerId", master.PaymentMethodLedgerId);
                                    cmd.Parameters.AddWithValue("@Narration", master.Narration ?? "");
                                    cmd.Parameters.AddWithValue("@UserId", master.UserId);
                                    cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                                    using (var reader = cmd.ExecuteReader())
                                    {
                                        if (reader.Read() && reader.FieldCount >= 2)
                                        {
                                            var status = reader[0].ToString();
                                            if (status == "SUCCESS")
                                            {
                                                master.Id = Convert.ToInt32(reader[1]);
                                            }
                                            else
                                            {
                                                transaction.Rollback();
                                                return false;
                                            }
                                        }
                                        else
                                        {
                                            transaction.Rollback();
                                            return false;
                                        }
                                    }
                                }
                            }

                            // 3. Insert detail rows for each invoice that had credit applied.
                            // Rows with CreditAmount = 0 (fully-paid invoice audit rows)
                            // are skipped for now and will be handled in Phase 2.
                            foreach (var detail in details)
                            {
                                if (detail.CreditAmount <= 0)
                                    continue;

                                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CreditNoteDetails, conn, transaction))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("@BranchId", master.BranchId);
                                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                                    cmd.Parameters.AddWithValue("@CustomerLedgerId", master.CustomerLedgerId);
                                    cmd.Parameters.AddWithValue("@CreditNoteMasterId", master.Id);
                                    cmd.Parameters.AddWithValue("@BillNo", detail.BillNo);
                                    cmd.Parameters.AddWithValue("@BillDate", detail.BillDate);
                                    cmd.Parameters.AddWithValue("@BillAmount", detail.BillAmount);
                                    cmd.Parameters.AddWithValue("@CreditAmount", detail.CreditAmount);
                                    cmd.Parameters.AddWithValue("@BalanceAmount", detail.BalanceAmount);
                                    cmd.Parameters.AddWithValue("@OldBillAmount", detail.OldBillAmount);
                                    cmd.Parameters.AddWithValue("@OldCreditAmount", 0);
                                    cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                                    var detailResult = cmd.ExecuteScalar();
                                    if (detailResult == null || !detailResult.ToString().StartsWith("SUCCESS"))
                                    {
                                        transaction.Rollback();
                                        return false;
                                    }
                                }
                            }

                            // Phase 2: Stamp the lifecycle fields on the master object.
                            // AppliedAmount = sum of all detail rows that were inserted (CreditAmount > 0).
                            // RemainingAmount and Status derive automatically from the model's
                            // computed properties — no DB schema change required at this stage.
                            master.AppliedAmount = details
                                .Where(d => d.CreditAmount > 0)
                                .Sum(d => d.CreditAmount);
                            // master.Status  → "Open" / "Partial" / "Closed"  (computed property)
                            // master.RemainingAmount → CreditAmount - AppliedAmount (computed property)

                            // 4. Create Voucher Entries - Double entry system
                            // Skip voucher creation if coming from Sales Return (vouchers already created)
                            if (!skipVoucherCreation)
                            {
                                // Credit entry (Customer account - reduce receivable)
                                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, conn, transaction))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("@CompanyID", master.CompanyId);
                                    cmd.Parameters.AddWithValue("@BranchID", master.BranchId);
                                    cmd.Parameters.AddWithValue("@VoucherID", master.VoucherId);
                                    cmd.Parameters.AddWithValue("@VoucherSeriesID", 0);
                                    cmd.Parameters.AddWithValue("@VoucherDate", master.VoucherDate);
                                    cmd.Parameters.AddWithValue("@VoucherNumber", "");
                                    cmd.Parameters.AddWithValue("@LedgerID", master.CustomerLedgerId);
                                    cmd.Parameters.AddWithValue("@VoucherType", "Credit Note");
                                    cmd.Parameters.AddWithValue("@Debit", 0);
                                    cmd.Parameters.AddWithValue("@Credit", (float)master.CreditAmount);
                                    cmd.Parameters.AddWithValue("@Narration", master.Narration ?? "");
                                    cmd.Parameters.AddWithValue("@SlNo", 1);
                                    cmd.Parameters.AddWithValue("@Mode", "");
                                    cmd.Parameters.AddWithValue("@ModeID", 0);
                                    cmd.Parameters.AddWithValue("@UserDate", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@UserID", master.UserId);
                                    cmd.Parameters.AddWithValue("@CancelFlag", false);
                                    cmd.Parameters.AddWithValue("@FinYearID", finYearId);
                                    cmd.Parameters.AddWithValue("@IsSyncd", false);
                                    cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                                    var voucherResult = cmd.ExecuteScalar();
                                    if (voucherResult == null || !voucherResult.ToString().StartsWith("SUCCESS"))
                                    {
                                        transaction.Rollback();
                                        return false;
                                    }
                                }

                                // Debit entry (Sales Return or Credit Note Issued account)
                                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, conn, transaction))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("@CompanyID", master.CompanyId);
                                    cmd.Parameters.AddWithValue("@BranchID", master.BranchId);
                                    cmd.Parameters.AddWithValue("@VoucherID", master.VoucherId);
                                    cmd.Parameters.AddWithValue("@VoucherSeriesID", 0);
                                    cmd.Parameters.AddWithValue("@VoucherDate", master.VoucherDate);
                                    cmd.Parameters.AddWithValue("@VoucherNumber", "");
                                    cmd.Parameters.AddWithValue("@LedgerID", master.PaymentMethodLedgerId);
                                    cmd.Parameters.AddWithValue("@VoucherType", "Credit Note");
                                    cmd.Parameters.AddWithValue("@Debit", (float)master.CreditAmount);
                                    cmd.Parameters.AddWithValue("@Credit", 0);
                                    cmd.Parameters.AddWithValue("@Narration", master.Narration ?? "");
                                    cmd.Parameters.AddWithValue("@SlNo", 2);
                                    cmd.Parameters.AddWithValue("@Mode", "");
                                    cmd.Parameters.AddWithValue("@ModeID", 0);
                                    cmd.Parameters.AddWithValue("@UserDate", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@UserID", master.UserId);
                                    cmd.Parameters.AddWithValue("@CancelFlag", false);
                                    cmd.Parameters.AddWithValue("@FinYearID", finYearId);
                                    cmd.Parameters.AddWithValue("@IsSyncd", false);
                                    cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                                    var voucherResult = cmd.ExecuteScalar();
                                    if (voucherResult == null || !voucherResult.ToString().StartsWith("SUCCESS"))
                                    {
                                        transaction.Rollback();
                                        return false;
                                    }
                                }
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new Exception($"Error while saving credit note: {ex.Message}", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Connection error: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// Get all credit notes for a branch
        /// </summary>
        public DataTable GetAllCreditNotes(int branchId, int finYearId, int pageIndex = 0, int pageSize = 100)
        {
            DataTable dt = new DataTable();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CreditNoteMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                    cmd.Parameters.AddWithValue("@PageIndex", pageIndex);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");

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
        /// Get credit note by ID
        /// </summary>
        public DataSet GetCreditNoteById(int voucherId, int branchId)
        {
            DataSet ds = new DataSet();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CreditNoteMaster, (SqlConnection)DataConnection))
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
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return ds;
        }

        /// <summary>
        /// Get credit note by Sales Return number
        /// </summary>
        public DataTable GetCreditNoteBySReturnNo(int sReturnNo, int branchId, int finYearId)
        {
            DataTable dt = new DataTable();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CreditNoteMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SReturnNo", sReturnNo);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYSRETURNNO");

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
        /// Delete a credit note (soft delete)
        /// </summary>
        public bool DeleteCreditNote(int creditNoteId)
        {
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CreditNoteMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", creditNoteId);
                    cmd.Parameters.AddWithValue("@_Operation", "DELETE");

                    var result = cmd.ExecuteScalar();
                    return result != null && result.ToString() == "SUCCESS";
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }
    }
}
