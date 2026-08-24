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
    public class DebitNoteRepository : BaseRepostitory
    {
        /// <summary>
        /// Generate a new voucher ID for Debit Note
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
                    cmd.Parameters.AddWithValue("@VoucherType", "Debit Note");
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
        /// Get outstanding invoices for vendor that can be debited (from PMaster)
        /// </summary>
        public DataTable GetOutstandingInvoices(int vendorId, int branchId)
        {
            if (DataConnection.State == ConnectionState.Open)
                DataConnection.Close();

            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._DebitNoteMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VendorLedgerId", vendorId);
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

        public DataRow GetInvoiceByPurchaseNo(string purchaseNo, int vendorId, int branchId)
        {
            if (string.IsNullOrEmpty(purchaseNo))
                return null;

            if (DataConnection.State == ConnectionState.Open)
                DataConnection.Close();

            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._DebitNoteMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@InvoiceNo", purchaseNo);
                    cmd.Parameters.AddWithValue("@VendorLedgerId", vendorId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYPURCHASENO");

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

        public int GetVendorLedgerIdByInvoiceNo(string purchaseNo, int branchId)
        {
            if (string.IsNullOrWhiteSpace(purchaseNo))
                return 0;

            if (DataConnection.State == ConnectionState.Open)
                DataConnection.Close();

            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._DebitNoteMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@InvoiceNo", purchaseNo);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETVENDORLEDGERBYINVOICE");
                    object res = cmd.ExecuteScalar();
                    if (res != null && res != DBNull.Value)
                    {
                        return Convert.ToInt32(res);
                    }
                }
            }
            catch { }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return 0;
        }

        public string GetVendorNameByLedgerId(int ledgerId)
        {
            if (ledgerId <= 0)
                return string.Empty;

            if (DataConnection.State == ConnectionState.Open)
                DataConnection.Close();

            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._DebitNoteMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VendorLedgerId", ledgerId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETVENDORNAME");
                    object res = cmd.ExecuteScalar();
                    if (res != null && res != DBNull.Value)
                    {
                        return res.ToString();
                    }
                }
            }
            catch { }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return string.Empty;
        }

        /// <summary>
        /// Get all invoices for vendor
        /// </summary>
        public DataTable GetAllInvoices(int vendorId, int branchId)
        {
            if (DataConnection.State == ConnectionState.Open)
                DataConnection.Close();

            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._DebitNoteMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VendorLedgerId", vendorId);
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
        /// Get vendor outstanding total via stored procedure
        /// </summary>
        public decimal GetVendorOutstandingTotal(int vendorLedgerId, int branchId)
        {
            decimal outstandingTotal = 0;
            if (DataConnection.State == ConnectionState.Open)
                DataConnection.Close();

            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._DebitNoteMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
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
                System.Diagnostics.Debug.WriteLine($"Error getting vendor outstanding: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return outstandingTotal;
        }

        /// <summary>
        /// Save Debit Note - creates master, details, and voucher entries
        /// </summary>
        /// <param name="master">Debit Note master record</param>
        /// <param name="details">Debit Note detail records</param>
        /// <param name="skipVoucherCreation">If true, skip voucher creation for non-accounting saves</param>
        public bool SaveDebitNote(DebitNoteMaster master, List<DebitNoteDetails> details, bool skipVoucherCreation = false)
        {
            if (master == null)
            {
                return false;
            }

            if (details == null)
            {
                details = new List<DebitNoteDetails>();
            }

            bool isPurchaseReturnDebitNote = master.PReturnNo > 0;
            if (!details.Any() && !isPurchaseReturnDebitNote)
            {
                return false;
            }
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
                                    cmd.Parameters.AddWithValue("@VoucherType", "Debit Note");
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

                            // 2. Insert into DebitNoteMaster
                            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._DebitNoteMaster, conn, transaction))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@CompanyId", master.CompanyId);
                                cmd.Parameters.AddWithValue("@BranchId", master.BranchId);
                                cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                                cmd.Parameters.AddWithValue("@VoucherId", master.VoucherId);
                                cmd.Parameters.AddWithValue("@VoucherDate", master.VoucherDate);
                                cmd.Parameters.AddWithValue("@VendorLedgerId", master.VendorLedgerId);
                                cmd.Parameters.AddWithValue("@PReturnNo", master.PReturnNo);
                                cmd.Parameters.AddWithValue("@InvoiceNo", master.InvoiceNo ?? "");
                                cmd.Parameters.AddWithValue("@DebitAmount", master.DebitAmount);
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

                            // 3. Insert into DebitNoteDetails for each selected invoice
                            foreach (var detail in details)
                            {
                                if (detail.DebitAmount > 0)
                                {
                                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._DebitNoteDetails, conn, transaction))
                                    {
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.Parameters.AddWithValue("@BranchId", master.BranchId);
                                        cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                                        cmd.Parameters.AddWithValue("@VendorLedgerId", master.VendorLedgerId);
                                        cmd.Parameters.AddWithValue("@DebitNoteMasterId", master.Id);
                                        cmd.Parameters.AddWithValue("@BillNo", detail.BillNo);
                                        cmd.Parameters.AddWithValue("@BillDate", detail.BillDate);
                                        cmd.Parameters.AddWithValue("@BillAmount", detail.BillAmount);
                                        cmd.Parameters.AddWithValue("@DebitAmount", detail.DebitAmount);
                                        cmd.Parameters.AddWithValue("@BalanceAmount", detail.BalanceAmount);
                                        cmd.Parameters.AddWithValue("@OldBillAmount", detail.OldBillAmount);
                                        cmd.Parameters.AddWithValue("@OldDebitAmount", 0);
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

                            // 4. Create Voucher Entries - Double entry system.
                            // Purchase Return is stock/pending only; linked Debit Note is the accounting posting point.

                            if (!skipVoucherCreation)
                            {
                                // Debit entry (Vendor account - reduce payable)
                                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, conn, transaction))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("@CompanyID", master.CompanyId);
                                    cmd.Parameters.AddWithValue("@BranchID", master.BranchId);
                                    cmd.Parameters.AddWithValue("@VoucherID", master.VoucherId);
                                    cmd.Parameters.AddWithValue("@VoucherSeriesID", 0);
                                    cmd.Parameters.AddWithValue("@VoucherDate", master.VoucherDate);
                                    cmd.Parameters.AddWithValue("@VoucherNumber", "DN" + master.VoucherId);
                                    cmd.Parameters.AddWithValue("@LedgerID", master.VendorLedgerId);
                                    cmd.Parameters.AddWithValue("@VoucherType", "Debit Note");
                                    cmd.Parameters.AddWithValue("@Debit", (float)master.DebitAmount);
                                    cmd.Parameters.AddWithValue("@Credit", 0);
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

                                // Credit entry (Purchase Return or Debit Note Issued account)
                                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, conn, transaction))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("@CompanyID", master.CompanyId);
                                    cmd.Parameters.AddWithValue("@BranchID", master.BranchId);
                                    cmd.Parameters.AddWithValue("@VoucherID", master.VoucherId);
                                    cmd.Parameters.AddWithValue("@VoucherSeriesID", 0);
                                    cmd.Parameters.AddWithValue("@VoucherDate", master.VoucherDate);
                                    cmd.Parameters.AddWithValue("@VoucherNumber", "DN" + master.VoucherId);
                                    cmd.Parameters.AddWithValue("@LedgerID", master.PaymentMethodLedgerId);
                                    cmd.Parameters.AddWithValue("@VoucherType", "Debit Note");
                                    cmd.Parameters.AddWithValue("@Debit", 0);
                                    cmd.Parameters.AddWithValue("@Credit", (float)master.DebitAmount);
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

                            // SyncQueue Integration - Enqueue Debit Note
                            try
                            {
                                Guid debitNoteGuid = Guid.NewGuid();
                                SyncQueueRepository.SetTransactionGuid(
                                    conn,
                                    transaction,
                                    "DEBIT_NOTE",
                                    master.Id.ToString(),
                                    debitNoteGuid);

                                SyncQueueRepository.EnqueueTransaction(
                                    conn,
                                    transaction,
                                    master.BranchId > 0 ? master.BranchId : SessionContext.BranchId,
                                    "DEBIT_NOTE",
                                    master.Id.ToString(),
                                    debitNoteGuid,
                                    "CREATE");
                            }
                            catch (Exception syncEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"[DebitNoteRepo.SaveDebitNote] SyncQueue error: {syncEx.Message}");
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new Exception($"Error while saving debit note: {ex.Message}", ex);
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
        /// Get all debit notes for a branch
        /// </summary>
        public DataTable GetAllDebitNotes(int branchId, int finYearId, int pageIndex = 0, int pageSize = 100)
        {
            DataTable dt = new DataTable();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._DebitNoteMaster, (SqlConnection)DataConnection))
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
        /// Get debit note by ID
        /// </summary>
        public DataSet GetDebitNoteById(int voucherId, int branchId)
        {
            DataSet ds = new DataSet();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._DebitNoteMaster, (SqlConnection)DataConnection))
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
        /// Get debit note by Purchase Return number
        /// </summary>
        public DataTable GetDebitNoteByPReturnNo(int pReturnNo, int branchId, int finYearId)
        {
            DataTable dt = new DataTable();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._DebitNoteMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PReturnNo", pReturnNo);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYPRETURNNO");

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
        /// Delete a debit note (soft delete)
        /// </summary>
        public bool DeleteDebitNote(int debitNoteId)
        {
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._DebitNoteMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", debitNoteId);
                    cmd.Parameters.AddWithValue("@_Operation", "DELETE");

                    var result = cmd.ExecuteScalar();
                    bool success = result != null && result.ToString() == "SUCCESS";

                    if (success)
                    {
                        try
                        {
                            Guid? existingGuid = SyncQueueRepository.GetExistingGuid(
                                DataConnection,
                                null,
                                SessionContext.BranchId,
                                "DEBIT_NOTE",
                                debitNoteId.ToString()) ?? Guid.NewGuid();

                            SyncQueueRepository.EnqueueTransaction(
                                DataConnection,
                                null,
                                SessionContext.BranchId,
                                "DEBIT_NOTE",
                                debitNoteId.ToString(),
                                existingGuid.Value,
                                "CANCEL");
                        }
                        catch (Exception syncEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"[DebitNoteRepo.DeleteDebitNote] SyncQueue error: {syncEx.Message}");
                        }
                    }

                    return success;
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        /// <summary>
        /// Gets all purchase returns available for a vendor, adjusting for partial payments/debits.
        /// </summary>
        public List<PReturnGetAll> GetAvailablePurchaseReturns(int vendorLedgerId, int branchId, int excludeDebitNoteId)
        {
            List<PReturnGetAll> list = new List<PReturnGetAll>();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._DebitNoteMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VendorLedgerId", vendorLedgerId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@Id", excludeDebitNoteId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETAVAILABLERETURNS");

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            list = dt.ToListOfObject<PReturnGetAll>();
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
            return list;
        }
    }
}
