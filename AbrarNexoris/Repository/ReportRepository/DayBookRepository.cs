using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ModelClass;
using ModelClass.Report;

namespace Repository.ReportRepository
{
    public class DayBookRepository : BaseRepostitory
    {
        public DayBookResponse GetDayBook(DateTime fromDate, DateTime toDate)
        {
            var response = new DayBookResponse();

            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                const string sql = @"
SELECT
    v.VoucherDate,
    v.VoucherID,
    ISNULL(v.VoucherNumber, '') AS VoucherNo,
    ISNULL(v.VoucherType, '') AS VoucherTypeName,
    ISNULL(NULLIF(v.LedgerName, ''), l.LedgerName) AS Particulars,
    ISNULL(v.Narration, '') AS Narration,
    ISNULL(v.Debit, 0) AS DebitAmount,
    ISNULL(v.Credit, 0) AS CreditAmount
FROM Vouchers v
LEFT JOIN LedgerMaster l
    ON l.LedgerID = v.LedgerID
   AND l.BranchID = v.BranchID
WHERE v.CompanyID = @CompanyId
  AND v.BranchID = @BranchId
  AND v.FinYearID = @FinYearId
  AND v.VoucherDate >= @FromDate
  AND v.VoucherDate <= @ToDate
  AND ISNULL(v.CancelFlag, 0) = 0
ORDER BY v.VoucherDate, v.VoucherID, v.SlNo;";

                using (SqlCommand command = new SqlCommand(sql, (SqlConnection)DataConnection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 60;
                    command.Parameters.AddWithValue("@CompanyId", GetContextValue(SessionContext.CompanyId, DataBase.CompanyId));
                    command.Parameters.AddWithValue("@BranchId", GetContextValue(SessionContext.BranchId, DataBase.BranchId));
                    command.Parameters.AddWithValue("@FinYearId", GetContextValue(SessionContext.FinYearId, DataBase.FinyearId));
                    command.Parameters.AddWithValue("@FromDate", fromDate);
                    command.Parameters.AddWithValue("@ToDate", GetEndOfDay(toDate));

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var t = new DayBookTransaction
                            {
                                VoucherDate = Convert.ToDateTime(reader["VoucherDate"]),
                                VoucherID = reader["VoucherID"] != DBNull.Value ? Convert.ToInt32(reader["VoucherID"]) : 0,
                                VoucherNo = reader["VoucherNo"]?.ToString(),
                                VoucherTypeName = reader["VoucherTypeName"]?.ToString(),
                                Particulars = reader["Particulars"]?.ToString(),
                                Narration = reader["Narration"]?.ToString(),
                                DebitAmount = Convert.ToDecimal(reader["DebitAmount"]),
                                CreditAmount = Convert.ToDecimal(reader["CreditAmount"])
                            };

                            response.Transactions.Add(t);
                            response.Summary.TotalDebits += t.DebitAmount;
                            response.Summary.TotalCredits += t.CreditAmount;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return response;
        }

        private DateTime GetEndOfDay(DateTime date)
        {
            return date.Date.AddDays(1).AddSeconds(-1);
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
    }
}
