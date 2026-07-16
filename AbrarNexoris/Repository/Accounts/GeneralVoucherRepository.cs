using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using ModelClass;
using ModelClass.TransactionModels;

namespace Repository.Accounts
{
    public class GeneralVoucherRepository : BaseRepostitory
    {
        public GeneralVoucher Save(GeneralVoucher voucher)
        {
            ValidateVoucher(voucher);
            ApplyContext(voucher);

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();
            var transaction = DataConnection.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                // Generate a new Voucher ID if it is a new record
                if (voucher.VoucherID <= 0)
                {
                    GenerateVoucherId(voucher, transaction);
                }
                else
                {
                    // For updates, the SP operation 'UPDATE' deletes the existing lines first
                    DeleteVoucherLinesForUpdate(voucher, transaction);
                }

                // Prepare double entry lines
                List<VoucherEntryLine> lines = PrepareVoucherEntries(voucher);

                // Insert the lines using STOREDPROCEDURE.POS_Vouchers
                int slNo = 1;
                foreach (var line in lines)
                {
                    var voucherData = CreateVoucherParam(voucher, line, slNo++);
                    DataConnection.Query<Voucher>(
                        STOREDPROCEDURE.POS_Vouchers,
                        voucherData,
                        transaction,
                        commandType: CommandType.StoredProcedure).ToList();
                }

                transaction.Commit();
                return voucher;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        public GeneralVoucher GetGeneralVoucher(long voucherId, int branchId, string voucherType)
        {
            ApplyOpenConnection();

            try
            {
                var parameters = new Voucher
                {
                    VoucherID = voucherId,
                    BranchID = branchId,
                    VoucherType = voucherType,
                    _Operation = "GETBYID"
                };

                var lines = DataConnection.Query<Voucher>(
                    STOREDPROCEDURE.POS_Vouchers,
                    parameters,
                    commandType: CommandType.StoredProcedure).ToList();

                if (lines == null || lines.Count == 0)
                {
                    return null;
                }

                var first = lines[0];
                var voucher = new GeneralVoucher
                {
                    VoucherID = first.VoucherID,
                    VoucherNumber = first.VoucherNumber,
                    VoucherDate = first.VoucherDate.GetValueOrDefault(DateTime.Today),
                    VoucherType = first.VoucherType,
                    Narration = first.Narration,
                    CompanyID = first.CompanyID,
                    BranchID = first.BranchID,
                    FinYearID = first.FinYearID,
                    UserID = first.UserID,
                    UserName = first.UserName
                };

                // Decode target ledger and cash/bank ledger from double entries
                if (voucherType == "GENPAY")
                {
                    // GENPAY: Target ledger is Debited, Cash/Bank ledger is Credited
                    var debitLine = lines.FirstOrDefault(l => l.Debit > 0);
                    var creditLine = lines.FirstOrDefault(l => l.Credit > 0);

                    if (debitLine != null)
                    {
                        voucher.LedgerID = (int)debitLine.LedgerID;
                        voucher.LedgerName = debitLine.LedgerName;
                        voucher.Amount = (decimal)debitLine.Debit;
                    }

                    if (creditLine != null)
                    {
                        voucher.CashBankLedgerID = (int)creditLine.LedgerID;
                        voucher.CashBankLedgerName = creditLine.LedgerName;
                    }
                }
                else if (voucherType == "GENREC")
                {
                    // GENREC: Cash/Bank ledger is Debited, Target ledger is Credited
                    var debitLine = lines.FirstOrDefault(l => l.Debit > 0);
                    var creditLine = lines.FirstOrDefault(l => l.Credit > 0);

                    if (debitLine != null)
                    {
                        voucher.CashBankLedgerID = (int)debitLine.LedgerID;
                        voucher.CashBankLedgerName = debitLine.LedgerName;
                        voucher.Amount = (decimal)debitLine.Debit;
                    }

                    if (creditLine != null)
                    {
                        voucher.LedgerID = (int)creditLine.LedgerID;
                        voucher.LedgerName = creditLine.LedgerName;
                    }
                }

                // Extract ReferenceNo from narration if we appended it (pattern: " [Ref: {ReferenceNo}]")
                if (!string.IsNullOrEmpty(voucher.Narration) && voucher.Narration.Contains(" [Ref: "))
                {
                    int index = voucher.Narration.LastIndexOf(" [Ref: ");
                    if (index > 0)
                    {
                        string refPart = voucher.Narration.Substring(index + 7);
                        voucher.ReferenceNo = refPart.TrimEnd(']');
                        voucher.Narration = voucher.Narration.Substring(0, index);
                    }
                }

                return voucher;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        public DataTable GetVoucherHistory(string voucherType, int branchId, int maxRows = 100)
        {
            int companyId = GetContextValue(SessionContext.CompanyId, DataBase.CompanyId);
            int finYearId = GetContextValue(SessionContext.FinYearId, DataBase.FinyearId);
            if (branchId <= 0)
            {
                branchId = GetContextValue(SessionContext.BranchId, DataBase.BranchId);
            }

            ApplyOpenConnection();

            try
            {
                DataTable table = new DataTable();
                using (var command = new SqlCommand(STOREDPROCEDURE.POS_GetGeneralVoucherHistory, (SqlConnection)DataConnection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@VoucherType", voucherType);
                    command.Parameters.AddWithValue("@CompanyID", companyId);
                    command.Parameters.AddWithValue("@BranchID", branchId);
                    command.Parameters.AddWithValue("@FinYearID", finYearId);
                    command.Parameters.AddWithValue("@MaxRows", maxRows);
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(table);
                    }
                }

                return table;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        public void Delete(long voucherId, int branchId, string voucherType)
        {
            if (voucherId <= 0)
            {
                throw new ArgumentException("Voucher ID is required for delete.", nameof(voucherId));
            }

            ApplyOpenConnection();

            try
            {
                var voucher = new Voucher
                {
                    VoucherID = voucherId,
                    BranchID = branchId,
                    VoucherType = voucherType,
                    _Operation = "DELETE"
                };

                DataConnection.Query<Voucher>(
                    STOREDPROCEDURE.POS_Vouchers,
                    voucher,
                    commandType: CommandType.StoredProcedure);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        private void ValidateVoucher(GeneralVoucher voucher)
        {
            if (voucher == null)
                throw new ArgumentNullException(nameof(voucher));

            if (voucher.Amount <= 0)
                throw new InvalidOperationException("Transaction Amount must be greater than zero.");

            if (voucher.LedgerID <= 0)
                throw new InvalidOperationException("Target Account Ledger must be selected.");

            if (voucher.CashBankLedgerID <= 0)
                throw new InvalidOperationException("Cash/Bank Account Ledger must be selected.");

            if (voucher.LedgerID == voucher.CashBankLedgerID)
                throw new InvalidOperationException("Target Ledger and Cash/Bank Ledger cannot be the same account.");
        }

        private void ApplyContext(GeneralVoucher voucher)
        {
            voucher.CompanyID = GetContextValue(SessionContext.CompanyId, DataBase.CompanyId);
            if (voucher.BranchID <= 0)
            {
                voucher.BranchID = GetContextValue(SessionContext.BranchId, DataBase.BranchId);
            }
            voucher.FinYearID = GetContextValue(SessionContext.FinYearId, DataBase.FinyearId);
            voucher.UserID = GetContextValue(SessionContext.UserId, DataBase.UserId);
            voucher.UserName = SessionContext.UserName ?? DataBase.UserName ?? "Admin";

            if (voucher.CompanyID <= 0 || voucher.BranchID <= 0 || voucher.FinYearID <= 0)
            {
                throw new InvalidOperationException(
                    $"Session values are missing. CompanyId={voucher.CompanyID}, BranchId={voucher.BranchID}, FinYearId={voucher.FinYearID}.");
            }
        }

        private void GenerateVoucherId(GeneralVoucher voucher, IDbTransaction transaction)
        {
            var p = new Voucher
            {
                _Operation = "GENERATENUMBER",
                CompanyID = voucher.CompanyID,
                BranchID = voucher.BranchID,
                FinYearID = voucher.FinYearID,
                VoucherType = voucher.VoucherType
            };

            var generated = DataConnection.Query<Voucher>(
                STOREDPROCEDURE.POS_Vouchers,
                p,
                transaction,
                commandType: CommandType.StoredProcedure).FirstOrDefault();

            if (generated == null || generated.VoucherID <= 0)
            {
                throw new InvalidOperationException("Failed to generate General Voucher ID.");
            }

            string prefix = voucher.VoucherType == "GENPAY" ? "GP" : "GR";
            voucher.VoucherID = generated.VoucherID;
            voucher.VoucherNumber = string.IsNullOrWhiteSpace(generated.VoucherNumber)
                ? $"{prefix}{generated.VoucherID:000000}"
                : generated.VoucherNumber;
        }

        private void DeleteVoucherLinesForUpdate(GeneralVoucher voucher, IDbTransaction transaction)
        {
            var p = new Voucher
            {
                CompanyID = voucher.CompanyID,
                BranchID = voucher.BranchID,
                VoucherID = voucher.VoucherID,
                VoucherType = voucher.VoucherType,
                FinYearID = voucher.FinYearID,
                _Operation = "UPDATE" // SP's UPDATE operation deletes existing rows for this voucher ID
            };

            DataConnection.Query<Voucher>(
                STOREDPROCEDURE.POS_Vouchers,
                p,
                transaction,
                commandType: CommandType.StoredProcedure);
        }

        private List<VoucherEntryLine> PrepareVoucherEntries(GeneralVoucher voucher)
        {
            var lines = new List<VoucherEntryLine>();
            decimal amt = voucher.Amount;

            if (voucher.VoucherType == "GENPAY")
            {
                // General Payment: Debit Target Ledger, Credit Cash/Bank
                lines.Add(new VoucherEntryLine { LedgerID = voucher.LedgerID, Debit = amt, Credit = 0 });
                lines.Add(new VoucherEntryLine { LedgerID = voucher.CashBankLedgerID, Debit = 0, Credit = amt });
            }
            else if (voucher.VoucherType == "GENREC")
            {
                // General Receipt: Debit Cash/Bank, Credit Target Ledger
                lines.Add(new VoucherEntryLine { LedgerID = voucher.CashBankLedgerID, Debit = amt, Credit = 0 });
                lines.Add(new VoucherEntryLine { LedgerID = voucher.LedgerID, Debit = 0, Credit = amt });
            }

            return lines;
        }

        private Voucher CreateVoucherParam(GeneralVoucher journal, VoucherEntryLine line, int slNo)
        {
            // Append reference/cheque number to narration if provided
            string fullNarration = journal.Narration ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(journal.ReferenceNo))
            {
                fullNarration += $" [Ref: {journal.ReferenceNo}]";
            }

            return new Voucher
            {
                CompanyID = journal.CompanyID,
                BranchID = journal.BranchID,
                VoucherID = journal.VoucherID,
                VoucherSeriesID = 0,
                VoucherDate = journal.VoucherDate,
                VoucherNumber = journal.VoucherNumber,
                LedgerID = line.LedgerID,
                VoucherType = journal.VoucherType,
                Debit = Convert.ToDouble(line.Debit),
                Credit = Convert.ToDouble(line.Credit),
                Narration = fullNarration,
                SlNo = slNo,
                Mode = string.Empty,
                ModeID = 0,
                UserDate = DateTime.Now,
                UserName = journal.UserName,
                UserID = journal.UserID,
                CancelFlag = false,
                FinYearID = journal.FinYearID,
                IsSyncd = false,
                _Operation = "CREATE"
            };
        }

        private void ApplyOpenConnection()
        {
            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }
            DataConnection.Open();
        }

        private int GetContextValue(int sessionValue, string legacyValue)
        {
            if (sessionValue > 0)
                return sessionValue;

            int val;
            return int.TryParse(legacyValue, out val) ? val : 0;
        }

        private class VoucherEntryLine
        {
            public int LedgerID { get; set; }
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
        }
    }
}
