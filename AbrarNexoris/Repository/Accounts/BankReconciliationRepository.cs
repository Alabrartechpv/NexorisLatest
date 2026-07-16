using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ModelClass;
using ModelClass.TransactionModels;

namespace Repository.Accounts
{
    public class BankReconciliationRepository : BaseRepostitory
    {
        /// <summary>
        /// Retrieves bank reconciliation data (grid items + summary) for a given bank ledger and date range.
        /// </summary>
        public BankReconciliationResult GetReconciliationData(int ledgerId, DateTime fromDate, DateTime toDate)
        {
            var result = new BankReconciliationResult();

            int companyId = SessionContext.CompanyId > 0 ? SessionContext.CompanyId : Convert.ToInt32(DataBase.CompanyId);
            int branchId = SessionContext.BranchId > 0 ? SessionContext.BranchId : Convert.ToInt32(DataBase.BranchId);
            int finYearId = SessionContext.FinYearId > 0 ? SessionContext.FinYearId : Convert.ToInt32(DataBase.FinyearId);

            if (DataConnection.State == ConnectionState.Open)
                DataConnection.Close();

            DataConnection.Open();

            try
            {
                using (var command = new SqlCommand(STOREDPROCEDURE.POS_BankReconciliation, (SqlConnection)DataConnection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 60;
                    command.Parameters.AddWithValue("@_Operation", "GETDATA");
                    command.Parameters.AddWithValue("@CompanyID", companyId);
                    command.Parameters.AddWithValue("@BranchID", branchId);
                    command.Parameters.AddWithValue("@FinYearID", finYearId);
                    command.Parameters.AddWithValue("@LedgerID", ledgerId);
                    command.Parameters.AddWithValue("@FromDate", fromDate);
                    command.Parameters.AddWithValue("@ToDate", toDate);

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        DataSet ds = new DataSet();
                        adapter.Fill(ds);

                        // Result Set 1: Transaction items
                        if (ds.Tables.Count > 0)
                        {
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                result.Items.Add(new BankReconciliationItem
                                {
                                    VoucherID = row["VoucherID"] != DBNull.Value ? Convert.ToInt64(row["VoucherID"]) : 0,
                                    SlNo = row["SlNo"] != DBNull.Value ? Convert.ToInt32(row["SlNo"]) : 0,
                                    VoucherDate = row["VoucherDate"] != DBNull.Value ? Convert.ToDateTime(row["VoucherDate"]) : DateTime.MinValue,
                                    VoucherNumber = row["VoucherNumber"] != DBNull.Value ? row["VoucherNumber"].ToString() : "",
                                    VoucherType = row["VoucherType"] != DBNull.Value ? row["VoucherType"].ToString() : "",
                                    Particulars = row["Particulars"] != DBNull.Value ? row["Particulars"].ToString() : "",
                                    Narration = row["Narration"] != DBNull.Value ? row["Narration"].ToString() : "",
                                    Debit = row["Debit"] != DBNull.Value ? Convert.ToDecimal(row["Debit"]) : 0,
                                    Credit = row["Credit"] != DBNull.Value ? Convert.ToDecimal(row["Credit"]) : 0,
                                    ReconciliationDate = row["ReconciliationDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["ReconciliationDate"]) : null,
                                    IsReconciled = row["IsReconciled"] != DBNull.Value && Convert.ToInt32(row["IsReconciled"]) == 1
                                });
                            }
                        }

                        // Result Set 2: Summary
                        if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                        {
                            DataRow summaryRow = ds.Tables[1].Rows[0];
                            result.Summary = new BankReconciliationSummary
                            {
                                BooksBalance = summaryRow["BooksBalance"] != DBNull.Value ? Convert.ToDecimal(summaryRow["BooksBalance"]) : 0,
                                UnclearedReceipts = summaryRow["UnclearedReceipts"] != DBNull.Value ? Convert.ToDecimal(summaryRow["UnclearedReceipts"]) : 0,
                                UnclearedPayments = summaryRow["UnclearedPayments"] != DBNull.Value ? Convert.ToDecimal(summaryRow["UnclearedPayments"]) : 0,
                                BankBalance = summaryRow["BankBalance"] != DBNull.Value ? Convert.ToDecimal(summaryRow["BankBalance"]) : 0
                            };
                        }
                    }
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return result;
        }

        /// <summary>
        /// Batch-saves reconciliation dates for multiple voucher lines in a single transaction.
        /// </summary>
        public int ReconcileBatch(List<BankReconciliationItem> items, int ledgerId)
        {
            int companyId = SessionContext.CompanyId > 0 ? SessionContext.CompanyId : Convert.ToInt32(DataBase.CompanyId);
            int branchId = SessionContext.BranchId > 0 ? SessionContext.BranchId : Convert.ToInt32(DataBase.BranchId);
            int finYearId = SessionContext.FinYearId > 0 ? SessionContext.FinYearId : Convert.ToInt32(DataBase.FinyearId);

            if (DataConnection.State == ConnectionState.Open)
                DataConnection.Close();

            DataConnection.Open();
            var transaction = DataConnection.BeginTransaction(IsolationLevel.Serializable);
            int totalUpdated = 0;

            try
            {
                foreach (var item in items)
                {
                    using (var command = new SqlCommand(STOREDPROCEDURE.POS_BankReconciliation, (SqlConnection)DataConnection, (SqlTransaction)transaction))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@_Operation", "RECONCILE");
                        command.Parameters.AddWithValue("@CompanyID", companyId);
                        command.Parameters.AddWithValue("@BranchID", branchId);
                        command.Parameters.AddWithValue("@FinYearID", finYearId);
                        command.Parameters.AddWithValue("@VoucherID", item.VoucherID);
                        command.Parameters.AddWithValue("@LedgerID", ledgerId);
                        command.Parameters.AddWithValue("@SlNo", item.SlNo);
                        command.Parameters.AddWithValue("@ReconciliationDate", (object)item.ReconciliationDate ?? DBNull.Value);

                        object result = command.ExecuteScalar();
                        if (result != null)
                            totalUpdated += Convert.ToInt32(result);
                    }
                }

                transaction.Commit();
                return totalUpdated;
            }
            catch
            {
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
