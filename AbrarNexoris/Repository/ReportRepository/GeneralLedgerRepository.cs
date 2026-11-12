using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ModelClass;
using ModelClass.Report;

namespace Repository.ReportRepository
{
    public class GeneralLedgerRepository : BaseRepostitory
    {
        public GeneralLedgerReport GetGeneralLedger(int ledgerId, DateTime fromDate, DateTime toDate, int branchIdOverride = 0)
        {
            var report = new GeneralLedgerReport
            {
                LedgerId = ledgerId,
                FromDate = fromDate.Date,
                ToDate = toDate.Date
            };

            if (ledgerId <= 0)
            {
                return report;
            }

            int companyId = GetContextValue(SessionContext.CompanyId, DataBase.CompanyId);
            int branchId = branchIdOverride > 0 ? branchIdOverride : GetContextValue(SessionContext.BranchId, DataBase.BranchId);
            int finYearId = GetContextValue(SessionContext.FinYearId, DataBase.FinyearId);

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                // 1. Fetch ledger metadata and master opening balance
                decimal masterOpnDebit = 0m;
                decimal masterOpnCredit = 0m;

                const string ledgerSql = @"
SELECT TOP 1
    lm.LedgerID,
    ISNULL(lm.LedgerName, '') AS LedgerName,
    ISNULL(lm.GroupID, 0) AS GroupID,
    ISNULL(ag.GroupName, '') AS GroupName,
    ISNULL(lm.OpnDebit, 0) AS OpnDebit,
    ISNULL(lm.OpnCredit, 0) AS OpnCredit
FROM LedgerMaster lm
LEFT JOIN AccountGroupMaster ag
    ON ag.GroupID = lm.GroupID
   AND ag.BranchID = lm.BranchID
WHERE lm.LedgerID = @LedgerId
  AND (@BranchId <= 0 OR lm.BranchID = @BranchId);";

                using (var cmd = new SqlCommand(ledgerSql, (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@LedgerId", ledgerId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            report.LedgerName = Convert.ToString(reader["LedgerName"]) ?? string.Empty;
                            report.GroupId = reader["GroupID"] != DBNull.Value ? Convert.ToInt32(reader["GroupID"]) : 0;
                            report.GroupName = Convert.ToString(reader["GroupName"]) ?? string.Empty;
                            masterOpnDebit = reader["OpnDebit"] != DBNull.Value ? Convert.ToDecimal(reader["OpnDebit"]) : 0m;
                            masterOpnCredit = reader["OpnCredit"] != DBNull.Value ? Convert.ToDecimal(reader["OpnCredit"]) : 0m;
                        }
                    }
                }

                // 2. Calculate prior transactions opening balance before @FromDate
                decimal priorNet = 0m;
                const string priorSql = @"
SELECT ISNULL(SUM(ISNULL(v.Debit, 0) - ISNULL(v.Credit, 0)), 0) AS PriorNet
FROM Vouchers v
WHERE v.LedgerID = @LedgerId
  AND (@CompanyId <= 0 OR v.CompanyID = @CompanyId)
  AND (@BranchId <= 0 OR v.BranchID = @BranchId)
  AND (@FinYearId <= 0 OR v.FinYearID = @FinYearId)
  AND CAST(v.VoucherDate AS DATE) < CAST(@FromDate AS DATE)
  AND ISNULL(v.CancelFlag, 0) = 0;";

                using (var cmd = new SqlCommand(priorSql, (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@LedgerId", ledgerId);
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    object val = cmd.ExecuteScalar();
                    if (val != null && val != DBNull.Value)
                    {
                        priorNet = Convert.ToDecimal(val);
                    }
                }

                decimal openingBalance = (masterOpnDebit - masterOpnCredit) + priorNet;
                decimal runningBalance = openingBalance;
                decimal totalDebit = 0m;
                decimal totalCredit = 0m;

                // 3. Fetch period transactions with contra account(s) and audit fields
                const string txnSql = @"
SELECT
    v.VoucherID,
    v.VoucherDate,
    ISNULL(NULLIF(v.VoucherNumber, ''), CAST(v.VoucherID AS varchar(30))) AS VoucherNo,
    ISNULL(v.VoucherType, '') AS VoucherTypeName,
    ISNULL(NULLIF(STUFF((
        SELECT DISTINCT ', ' + ISNULL(NULLIF(l2.LedgerName, ''), ISNULL(v2.LedgerName, ''))
        FROM Vouchers v2
        LEFT JOIN LedgerMaster l2
            ON l2.LedgerID = v2.LedgerID
           AND l2.BranchID = v2.BranchID
        WHERE v2.VoucherID = v.VoucherID
          AND v2.VoucherType = v.VoucherType
          AND v2.BranchID = v.BranchID
          AND v2.LedgerID <> @LedgerId
          AND ISNULL(v2.CancelFlag, 0) = 0
        FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, ''), ''), 'Various Accounts') AS Particulars,
    ISNULL(v.LedgerName, '') AS VoucherLedgerName,
    ISNULL(v.Narration, '') AS Narration,
    ISNULL(v.Debit, 0) AS DebitAmount,
    ISNULL(v.Credit, 0) AS CreditAmount,
    ISNULL(v.UserID, 0) AS UserID,
    ISNULL(v.UserName, '') AS UserName,
    v.UserDate
FROM Vouchers v
WHERE v.LedgerID = @LedgerId
  AND (@CompanyId <= 0 OR v.CompanyID = @CompanyId)
  AND (@BranchId <= 0 OR v.BranchID = @BranchId)
  AND (@FinYearId <= 0 OR v.FinYearID = @FinYearId)
  AND CAST(v.VoucherDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)
  AND ISNULL(v.CancelFlag, 0) = 0
ORDER BY v.VoucherDate, v.VoucherID, v.SlNo;";

                using (var cmd = new SqlCommand(txnSql, (SqlConnection)DataConnection))
                {
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.AddWithValue("@LedgerId", ledgerId);
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            decimal debit = reader["DebitAmount"] != DBNull.Value ? Convert.ToDecimal(reader["DebitAmount"]) : 0m;
                            decimal credit = reader["CreditAmount"] != DBNull.Value ? Convert.ToDecimal(reader["CreditAmount"]) : 0m;
                            runningBalance += debit - credit;
                            totalDebit += debit;
                            totalCredit += credit;

                            report.Transactions.Add(new GeneralLedgerTransaction
                            {
                                VoucherID = reader["VoucherID"] != DBNull.Value ? Convert.ToInt32(reader["VoucherID"]) : 0,
                                VoucherDate = reader["VoucherDate"] != DBNull.Value ? Convert.ToDateTime(reader["VoucherDate"]) : DateTime.MinValue,
                                VoucherNo = Convert.ToString(reader["VoucherNo"]) ?? string.Empty,
                                VoucherTypeName = Convert.ToString(reader["VoucherTypeName"]) ?? string.Empty,
                                Particulars = Convert.ToString(reader["Particulars"]) ?? string.Empty,
                                VoucherLedgerName = Convert.ToString(reader["VoucherLedgerName"]) ?? string.Empty,
                                Narration = Convert.ToString(reader["Narration"]) ?? string.Empty,
                                DebitAmount = debit,
                                CreditAmount = credit,
                                RunningBalance = runningBalance,
                                UserID = reader["UserID"] != DBNull.Value ? Convert.ToInt32(reader["UserID"]) : 0,
                                UserName = Convert.ToString(reader["UserName"]) ?? string.Empty,
                                UserDate = reader["UserDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["UserDate"]) : null
                            });
                        }
                    }
                }

                report.Summary = new GeneralLedgerSummary
                {
                    OpeningBalance = openingBalance,
                    TotalDebit = totalDebit,
                    TotalCredit = totalCredit,
                    ClosingBalance = runningBalance
                };
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return report;
        }
    }
}

