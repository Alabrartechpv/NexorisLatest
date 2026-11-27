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
    public class JournalVoucherRepository : BaseRepostitory
    {
        protected virtual string VoucherType => "Journal";
        protected virtual string VoucherNumberPrefix => "JV";
        private bool IsGeneralVoucher => VoucherType == "GENPAY" || VoucherType == "GENREC";

        public JournalVoucher Save(JournalVoucher journal)
        {
            ValidateJournal(journal);
            ApplyContext(journal);

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();
            var transaction = DataConnection.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                if (journal.VoucherID <= 0)
                {
                    GenerateVoucherId(journal, transaction);
                }
                else
                {
                    ActivityConnectionContext.MarkDirectVoucherEdit(transaction);
                    DeleteVoucherLines(journal, transaction);
                }

                int slNo = 1;
                foreach (var line in journal.Lines)
                {
                    var voucher = CreateVoucherEntry(journal, line, slNo++);
                    DataConnection.Query<Voucher>(
                        STOREDPROCEDURE.POS_Vouchers,
                        voucher,
                        transaction,
                        commandType: CommandType.StoredProcedure).ToList();
                }

                transaction.Commit();
                journal.VoucherNumber = BuildDisplayVoucherNumber(journal.VoucherID);
                return journal;
            }
            catch
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

        public JournalVoucher GetJournalVoucher(string voucherText)
        {
            long voucherId = ParseVoucherId(voucherText);
            ApplyOpenConnection();

            try
            {
                string sql = @"
SELECT v.VoucherID,
       v.VoucherNumber,
       v.VoucherDate,
       v.Narration AS HeaderNarration,
       v.CompanyID,
       v.BranchID,
       v.FinYearID,
       v.UserID,
       v.SlNo,
       v.LedgerID,
       ISNULL(l.LedgerName, v.LedgerName) AS LedgerName,
       ISNULL(v.Debit, 0) AS Debit,
       ISNULL(v.Credit, 0) AS Credit,
       v.Narration AS LineNarration
FROM Vouchers v
LEFT JOIN LedgerMaster l
    ON l.LedgerID = v.LedgerID
   AND l.BranchID = v.BranchID
   AND l.CompanyID = v.CompanyID
WHERE v.VoucherType = @VoucherType
  AND ISNULL(v.CancelFlag, 0) = 0
  AND (@UseSessionScope = 0 OR
       (v.CompanyID = @CompanyID AND v.BranchID = @BranchID AND v.FinYearID = @FinYearID))
  AND ((@VoucherID > 0 AND v.VoucherID = @VoucherID)
       OR (@VoucherNumber <> '' AND v.VoucherNumber = @VoucherNumber))
ORDER BY v.SlNo;";

                DataTable table = new DataTable();
                using (var command = new SqlCommand(sql, (SqlConnection)DataConnection))
                {
                    command.Parameters.AddWithValue("@VoucherType", VoucherType);
                    command.Parameters.AddWithValue("@VoucherID", voucherId);
                    command.Parameters.AddWithValue("@VoucherNumber", voucherText ?? string.Empty);
                    command.Parameters.AddWithValue("@UseSessionScope", IsGeneralVoucher);
                    command.Parameters.AddWithValue("@CompanyID", GetContextValue(SessionContext.CompanyId, DataBase.CompanyId));
                    command.Parameters.AddWithValue("@BranchID", GetContextValue(SessionContext.BranchId, DataBase.BranchId));
                    command.Parameters.AddWithValue("@FinYearID", GetContextValue(SessionContext.FinYearId, DataBase.FinyearId));
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(table);
                    }
                }

                if (table.Rows.Count == 0)
                {
                    return null;
                }

                DataRow first = table.Rows[0];
                var journal = new JournalVoucher
                {
                    VoucherID = Convert.ToInt64(first["VoucherID"]),
                    VoucherNumber = first["VoucherNumber"] == DBNull.Value || string.IsNullOrWhiteSpace(first["VoucherNumber"].ToString())
                        ? BuildDisplayVoucherNumber(Convert.ToInt64(first["VoucherID"]))
                        : first["VoucherNumber"].ToString(),
                    VoucherDate = first["VoucherDate"] == DBNull.Value ? DateTime.Today : Convert.ToDateTime(first["VoucherDate"]),
                    Narration = first["HeaderNarration"] == DBNull.Value ? string.Empty : first["HeaderNarration"].ToString(),
                    CompanyID = first["CompanyID"] == DBNull.Value ? 0 : Convert.ToInt32(first["CompanyID"]),
                    BranchID = first["BranchID"] == DBNull.Value ? 0 : Convert.ToInt32(first["BranchID"]),
                    FinYearID = first["FinYearID"] == DBNull.Value ? 0 : Convert.ToInt32(first["FinYearID"]),
                    UserID = first["UserID"] == DBNull.Value ? 0 : Convert.ToInt32(first["UserID"])
                };

                foreach (DataRow row in table.Rows)
                {
                    journal.Lines.Add(new JournalVoucherLine
                    {
                        SlNo = row["SlNo"] == DBNull.Value ? journal.Lines.Count + 1 : Convert.ToInt32(row["SlNo"]),
                        LedgerID = row["LedgerID"] == DBNull.Value ? 0 : Convert.ToInt64(row["LedgerID"]),
                        LedgerName = row["LedgerName"] == DBNull.Value ? string.Empty : row["LedgerName"].ToString(),
                        Debit = row["Debit"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Debit"]),
                        Credit = row["Credit"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Credit"]),
                        Narration = row["LineNarration"] == DBNull.Value ? string.Empty : row["LineNarration"].ToString()
                    });
                }

                return journal;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        public DataTable GetVoucherHistory(int branchId, int maxRows = 100)
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
                string sql = @"
SELECT TOP (@MaxRows)
       v.VoucherID,
       ISNULL(NULLIF(MAX(v.VoucherNumber), ''), CAST(v.VoucherID AS varchar(30))) AS VoucherNumber,
       CAST(MAX(v.VoucherDate) AS date) AS VoucherDate,
       ISNULL(MAX(v.Narration), '') AS Narration,
       SUM(ISNULL(v.Debit, 0)) AS TotalDebit,
       SUM(ISNULL(v.Credit, 0)) AS TotalCredit
FROM Vouchers v
WHERE v.VoucherType = @VoucherType
  AND v.CompanyID = @CompanyID
  AND v.BranchID = @BranchID
  AND v.FinYearID = @FinYearID
  AND ISNULL(v.CancelFlag, 0) = 0
GROUP BY v.VoucherID
ORDER BY MAX(v.VoucherDate) DESC, v.VoucherID DESC;";

                DataTable table = new DataTable();
                using (var command = new SqlCommand(sql, (SqlConnection)DataConnection))
                {
                    command.Parameters.AddWithValue("@MaxRows", maxRows);
                    command.Parameters.AddWithValue("@VoucherType", VoucherType);
                    command.Parameters.AddWithValue("@CompanyID", companyId);
                    command.Parameters.AddWithValue("@BranchID", branchId);
                    command.Parameters.AddWithValue("@FinYearID", finYearId);
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

        public void Delete(long voucherId)
        {
            if (voucherId <= 0)
            {
                throw new ArgumentException("Voucher ID is required for delete.", nameof(voucherId));
            }

            var journal = new JournalVoucher { VoucherID = voucherId };
            ApplyContext(journal);

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();
            var transaction = DataConnection.BeginTransaction();

            try
            {
                if (IsGeneralVoucher)
                    DeleteGeneralVoucherWithAudit(journal, transaction);
                else
                    DeleteVoucherLines(journal, transaction);
                transaction.Commit();
            }
            catch
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

        private void DeleteGeneralVoucherWithAudit(JournalVoucher journal, IDbTransaction transaction)
        {
            const string sql = @"
                DECLARE @DeletedLines TABLE (
                    DocDate datetime, RefNo nvarchar(100), LedgerId int, PartyName nvarchar(200),
                    Narration nvarchar(max), Debit decimal(28, 4), Credit decimal(28, 4));

                DELETE dbo.Vouchers
                OUTPUT deleted.VoucherDate, deleted.VoucherNumber, deleted.LedgerID, deleted.LedgerName,
                       deleted.Narration, ISNULL(deleted.Debit, 0), ISNULL(deleted.Credit, 0)
                INTO @DeletedLines (DocDate, RefNo, LedgerId, PartyName, Narration, Debit, Credit)
                WHERE CompanyID = @CompanyID AND BranchID = @BranchID AND FinYearID = @FinYearID
                  AND VoucherType = @VoucherType AND VoucherID = @VoucherID AND ISNULL(CancelFlag, 0) = 0;

                IF NOT EXISTS (SELECT 1 FROM @DeletedLines)
                    THROW 50001, 'The voucher could not be found or was already deleted.', 1;

                INSERT dbo.VoucherDeletionAudit
                    (CompanyId, BranchId, FinYearId, VoucherType, VoucherId, DocDate, RefNo,
                     PartyName, Amount, Narration, DeletedBy, Counter)
                SELECT @CompanyID, @BranchID, @FinYearID, @VoucherType, @VoucherID,
                       MIN(DocDate), MAX(NULLIF(RefNo, N'')),
                       CASE WHEN COUNT(DISTINCT d.LedgerId) > 1 THEN N'Multiple ledgers'
                            ELSE MIN(COALESCE(NULLIF(d.PartyName, N''), l.LedgerName)) END,
                       SUM(CASE WHEN @VoucherType = 'GENPAY' THEN Debit ELSE Credit END),
                       MAX(NULLIF(d.Narration, N'')), @DeletedBy, @Counter
                FROM @DeletedLines d
                LEFT JOIN dbo.LedgerMaster l
                  ON l.LedgerId = d.LedgerId AND l.CompanyId = @CompanyID AND l.BranchId = @BranchID
                WHERE (@VoucherType = 'GENPAY' AND Debit > 0) OR (@VoucherType = 'GENREC' AND Credit > 0)
                HAVING COUNT(*) > 0;

                IF @@ROWCOUNT = 0
                    THROW 50002, 'The voucher has no payment or receipt entries to audit.', 1;";

            DataConnection.Execute(sql, new
            {
                journal.CompanyID,
                journal.BranchID,
                journal.FinYearID,
                journal.VoucherID,
                VoucherType,
                DeletedBy = string.IsNullOrWhiteSpace(journal.UserName) ? "Unknown" : journal.UserName,
                Counter = SessionContext.CounterName
            }, transaction);
        }

        private void ValidateJournal(JournalVoucher journal)
        {
            if (journal == null)
            {
                throw new ArgumentNullException(nameof(journal));
            }

            journal.Lines = journal.Lines
                .Where(line => line != null && line.LedgerID > 0 && (line.Debit > 0 || line.Credit > 0))
                .ToList();

            if (journal.Lines.Count < 2)
            {
                throw new InvalidOperationException("Journal voucher must contain at least two posting lines.");
            }

            foreach (var line in journal.Lines)
            {
                if (line.Debit < 0 || line.Credit < 0)
                {
                    throw new InvalidOperationException("Debit and Credit cannot be negative.");
                }

                if (line.Debit > 0 && line.Credit > 0)
                {
                    throw new InvalidOperationException("A journal line cannot have both Debit and Credit.");
                }
            }

            if (Math.Round(journal.TotalDebit, 2) != Math.Round(journal.TotalCredit, 2))
            {
                throw new InvalidOperationException("Journal voucher is not balanced. Total Debit must equal Total Credit.");
            }
        }

        private void ApplyContext(JournalVoucher journal)
        {
            journal.CompanyID = GetContextValue(SessionContext.CompanyId, DataBase.CompanyId);
            if (journal.BranchID <= 0)
            {
                journal.BranchID = GetContextValue(SessionContext.BranchId, DataBase.BranchId);
            }
            journal.FinYearID = GetContextValue(SessionContext.FinYearId, DataBase.FinyearId);
            journal.UserID = GetContextValue(SessionContext.UserId, DataBase.UserId);
            journal.UserName = SessionContext.UserName ?? DataBase.UserName ?? string.Empty;
            journal.VoucherType = VoucherType;

            if (journal.CompanyID <= 0 || journal.BranchID <= 0 || journal.FinYearID <= 0)
            {
                throw new InvalidOperationException(
                    $"Journal cannot be saved because session values are missing. CompanyId={journal.CompanyID}, BranchId={journal.BranchID}, FinYearId={journal.FinYearID}.");
            }
        }

        private void GenerateVoucherId(JournalVoucher journal, IDbTransaction transaction)
        {
            long nextId = 0;
            try
            {
                var voucher = new Voucher
                {
                    _Operation = "GENERATENUMBER",
                    CompanyID = journal.CompanyID,
                    BranchID = journal.BranchID,
                    FinYearID = journal.FinYearID,
                    VoucherType = VoucherType
                };

                var generated = DataConnection.Query<Voucher>(
                    STOREDPROCEDURE.POS_Vouchers,
                    voucher,
                    transaction,
                    commandType: CommandType.StoredProcedure).FirstOrDefault();

                if (generated != null && generated.VoucherID > 0)
                {
                    nextId = generated.VoucherID;
                }
            }
            catch { }

            try
            {
                string maxSql = "SELECT ISNULL(MAX(VoucherID), 0) FROM Vouchers WHERE VoucherType = @VoucherType AND BranchID = @BranchID;";
                long currentMax = DataConnection.ExecuteScalar<long>(maxSql, new { VoucherType = VoucherType, BranchID = journal.BranchID }, transaction);

                if (nextId <= currentMax)
                {
                    nextId = currentMax + 1;
                }
            }
            catch { }

            if (nextId <= 0)
            {
                nextId = 1;
            }

            journal.VoucherID = nextId;
            journal.VoucherNumber = BuildDisplayVoucherNumber(nextId);
        }

        private Voucher CreateVoucherEntry(JournalVoucher journal, JournalVoucherLine line, int slNo)
        {
            string narration = string.IsNullOrWhiteSpace(line.Narration) ? journal.Narration : line.Narration;

            return new Voucher
            {
                CompanyID = journal.CompanyID,
                BranchID = journal.BranchID,
                VoucherID = journal.VoucherID,
                VoucherSeriesID = 0,
                VoucherDate = journal.VoucherDate,
                VoucherNumber = journal.VoucherNumber ?? BuildDisplayVoucherNumber(journal.VoucherID),
                LedgerID = line.LedgerID,
                LedgerName = line.LedgerName,
                VoucherType = VoucherType,
                Debit = Convert.ToDouble(line.Debit),
                Credit = Convert.ToDouble(line.Credit),
                Narration = narration ?? string.Empty,
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

        private void DeleteVoucherLines(JournalVoucher journal, IDbTransaction transaction)
        {
            try
            {
                var voucher = new Voucher
                {
                    CompanyID = journal.CompanyID,
                    BranchID = journal.BranchID,
                    VoucherID = journal.VoucherID,
                    VoucherType = VoucherType,
                    FinYearID = journal.FinYearID,
                    _Operation = "UPDATE"
                };

                DataConnection.Query<Voucher>(
                    STOREDPROCEDURE.POS_Vouchers,
                    voucher,
                    transaction,
                    commandType: CommandType.StoredProcedure).ToList();
            }
            catch { }

            try
            {
                string sql = "DELETE FROM Vouchers WHERE VoucherType = @VoucherType AND VoucherID = @VoucherID AND BranchID = @BranchID;";
                DataConnection.Execute(sql, new { VoucherType = VoucherType, VoucherID = journal.VoucherID, BranchID = journal.BranchID }, transaction);
            }
            catch { }
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
            {
                return sessionValue;
            }

            int parsedValue;
            return int.TryParse(legacyValue, out parsedValue) ? parsedValue : 0;
        }

        private long ParseVoucherId(string voucherText)
        {
            if (string.IsNullOrWhiteSpace(voucherText))
            {
                return 0;
            }

            string digits = new string(voucherText.Where(char.IsDigit).ToArray());
            long voucherId;
            return long.TryParse(digits, out voucherId) ? voucherId : 0;
        }

        private string BuildDisplayVoucherNumber(long voucherId)
        {
            return $"{VoucherNumberPrefix}{voucherId:000000}";
        }
    }

    public class GeneralPaymentRepository : JournalVoucherRepository
    {
        protected override string VoucherType => "GENPAY";
        protected override string VoucherNumberPrefix => "GP";
    }

    public class GeneralReceiptRepository : JournalVoucherRepository
    {
        protected override string VoucherType => "GENREC";
        protected override string VoucherNumberPrefix => "GR";
    }
}

