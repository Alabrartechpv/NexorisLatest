using ModelClass;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Repository.SettingsRepo
{
    public class TransactionActivityLogRepository : BaseRepostitory
    {
        public void SavePurchaseActivity(
            long transactionNo,
            string invoiceNo,
            string partyName,
            string paymentMode,
            decimal netAmount,
            string activityType,
            string activityDetails,
            decimal? qty = null,
            decimal? cost = null,
            string unit = null,
            string barcode = null,
            decimal? sPrice = null,
            decimal? taxAmt = null,
            decimal? taxPer = null,
            decimal? baseAmount = null,
            decimal? packing = null,
            decimal? retailPrice = null,
            decimal? free = null,
            decimal? unitSP = null,
            string taxType = null,
            decimal? gross = null)
        {
            SaveActivity("Purchase", transactionNo, invoiceNo, partyName, paymentMode, netAmount, activityType, activityDetails, qty, cost, unit, barcode, sPrice, taxAmt, taxPer, baseAmount, packing, retailPrice, free, unitSP, taxType, gross);
        }

        public void SaveSalesActivity(
            long transactionNo,
            string invoiceNo,
            string partyName,
            string paymentMode,
            decimal netAmount,
            string activityType,
            string activityDetails,
            decimal? qty = null,
            decimal? cost = null,
            string unit = null,
            string barcode = null,
            decimal? sPrice = null,
            decimal? taxAmt = null,
            decimal? taxPer = null,
            decimal? baseAmount = null,
            decimal? packing = null,
            decimal? retailPrice = null,
            decimal? free = null,
            decimal? unitSP = null,
            string taxType = null,
            decimal? gross = null)
        {
            SaveActivity("Sales", transactionNo, invoiceNo, partyName, paymentMode, netAmount, activityType, activityDetails, qty, cost, unit, barcode, sPrice, taxAmt, taxPer, baseAmount, packing, retailPrice, free, unitSP, taxType, gross);
        }

        public void SavePurchaseReturnActivity(
            long transactionNo,
            string invoiceNo,
            string partyName,
            string paymentMode,
            decimal netAmount,
            string activityType,
            string activityDetails,
            decimal? qty = null,
            decimal? cost = null,
            string unit = null,
            string barcode = null,
            decimal? sPrice = null,
            decimal? taxAmt = null,
            decimal? taxPer = null,
            decimal? baseAmount = null,
            decimal? packing = null,
            decimal? retailPrice = null,
            decimal? free = null,
            decimal? unitSP = null,
            string taxType = null,
            decimal? gross = null)
        {
            SaveActivity("Purchase Return", transactionNo, invoiceNo, partyName, paymentMode, netAmount, activityType, activityDetails, qty, cost, unit, barcode, sPrice, taxAmt, taxPer, baseAmount, packing, retailPrice, free, unitSP, taxType, gross);
        }

        public void SaveSalesReturnActivity(
            long transactionNo,
            string invoiceNo,
            string partyName,
            string paymentMode,
            decimal netAmount,
            string activityType,
            string activityDetails,
            decimal? qty = null,
            decimal? cost = null,
            string unit = null,
            string barcode = null,
            decimal? sPrice = null,
            decimal? taxAmt = null,
            decimal? taxPer = null,
            decimal? baseAmount = null,
            decimal? packing = null,
            decimal? retailPrice = null,
            decimal? free = null,
            decimal? unitSP = null,
            string taxType = null,
            decimal? gross = null)
        {
            SaveActivity("Sales Return", transactionNo, invoiceNo, partyName, paymentMode, netAmount, activityType, activityDetails, qty, cost, unit, barcode, sPrice, taxAmt, taxPer, baseAmount, packing, retailPrice, free, unitSP, taxType, gross);
        }

        public void SaveStockAdjustmentActivity(
            long transactionNo,
            string invoiceNo,
            string partyName,
            string paymentMode,
            decimal netAmount,
            string activityType,
            string activityDetails,
            decimal? qty = null,
            decimal? cost = null,
            string unit = null,
            string barcode = null,
            decimal? sPrice = null,
            decimal? taxAmt = null,
            decimal? taxPer = null,
            decimal? baseAmount = null,
            decimal? packing = null,
            decimal? retailPrice = null,
            decimal? free = null,
            decimal? unitSP = null,
            string taxType = null,
            decimal? gross = null)
        {
            SaveActivity("Stock Adjustment", transactionNo, invoiceNo, partyName, paymentMode, netAmount, activityType, activityDetails, qty, cost, unit, barcode, sPrice, taxAmt, taxPer, baseAmount, packing, retailPrice, free, unitSP, taxType, gross);
        }

        public DataTable GetActivityLog(string logType, DateTime fromDate, DateTime toDate, string userName, string activityType, string searchText)
        {
            DataTable result = new DataTable();
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                string tableName = GetTableName(logType);
                EnsureActivityLogTable(tableName);

                using (SqlCommand cmd = new SqlCommand($@"
SELECT
    ActivityLogId,
    CreatedOn,
    UserName,
    ActivityType,
    TransactionNo,
    InvoiceNo,
    PartyName,
    PaymentMode,
    NetAmount,
    Qty,
    Cost,
    Unit,
    Barcode,
    SPrice,
    TaxAmt,
    TaxPer,
    BaseAmount,
    Packing,
    RetailPrice,
    Free,
    UnitSP,
    TaxType,
    Gross,
    ActivityDetails,
    CompanyId,
    BranchId,
    FinYearId,
    UserId,
    CounterName,
    CounterId,
    CounterSessionId
FROM dbo.{tableName}
WHERE CreatedOn >= @FromDate
  AND CreatedOn < DATEADD(DAY, 1, @ToDate)
ORDER BY CreatedOn DESC, ActivityLogId DESC;", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(result);
                    }
                }

                AppendRecoveredTransactionRows(result, logType, fromDate, toDate, string.Empty, string.Empty, string.Empty);
                ApplyPersistentDisplayLogNumbers(result);
                result = FilterActivityRows(result, userName, activityType, searchText);
                result.DefaultView.Sort = result.Columns.Contains("DisplayLogNo")
                    ? "DisplayLogNo DESC"
                    : "CreatedOn DESC, ActivityLogId DESC";
                result = result.DefaultView.ToTable();
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return result;
        }

        public DataTable GetActivityUsers(string logType)
        {
            return GetDistinctColumnValues(logType, "UserName");
        }

        public DataTable GetActivityTypes(string logType)
        {
            return GetDistinctColumnValues(logType, "ActivityType");
        }

        public int CountActivity(string logType, DateTime fromDate, DateTime toDate)
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                string tableName = GetTableName(logType);
                EnsureActivityLogTable(tableName);

                using (SqlCommand cmd = new SqlCommand($@"
SELECT COUNT(1)
FROM dbo.{tableName}
WHERE CreatedOn >= @FromDate
  AND CreatedOn < DATEADD(DAY, 1, @ToDate);", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date);
                    return Convert.ToInt32(cmd.ExecuteScalar()) + CountRecoveredTransactions(logType, fromDate, toDate);
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        private void SaveActivity(
            string logType,
            long transactionNo,
            string invoiceNo,
            string partyName,
            string paymentMode,
            decimal netAmount,
            string activityType,
            string activityDetails,
            decimal? qty,
            decimal? cost,
            string unit,
            string barcode,
            decimal? sPrice = null,
            decimal? taxAmt = null,
            decimal? taxPer = null,
            decimal? baseAmount = null,
            decimal? packing = null,
            decimal? retailPrice = null,
            decimal? free = null,
            decimal? unitSP = null,
            string taxType = null,
            decimal? gross = null)
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                string tableName = GetTableName(logType);
                EnsureActivityLogTable(tableName);
                EnsureTransactionActivityLogStoredProcedure();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_TransactionActivityLog, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "SAVE");
                    cmd.Parameters.AddWithValue("@LogType", logType);
                    cmd.Parameters.AddWithValue("@TransactionNo", transactionNo);
                    cmd.Parameters.AddWithValue("@InvoiceNo", (object)invoiceNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PartyName", (object)partyName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PaymentMode", (object)paymentMode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NetAmount", netAmount);
                    cmd.Parameters.AddWithValue("@Qty", (object)qty ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Cost", (object)cost ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Unit", (object)unit ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Barcode", (object)barcode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SPrice", (object)sPrice ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TaxAmt", (object)taxAmt ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TaxPer", (object)taxPer ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BaseAmount", (object)baseAmount ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Packing", (object)packing ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RetailPrice", (object)retailPrice ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Free", (object)free ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@UnitSP", (object)unitSP ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TaxType", (object)taxType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Gross", (object)gross ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActivityType", (object)activityType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActivityDetails", (object)activityDetails ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CompanyId", GetCompanyId());
                    cmd.Parameters.AddWithValue("@BranchId", GetBranchId());
                    cmd.Parameters.AddWithValue("@FinYearId", GetFinYearId());
                    cmd.Parameters.AddWithValue("@UserId", GetUserId());
                    cmd.Parameters.AddWithValue("@UserName", GetUserName());
                    cmd.Parameters.AddWithValue("@CounterId", SessionContext.CounterId);
                    cmd.Parameters.AddWithValue("@CounterName", (object)(SessionContext.CounterName ?? string.Empty));
                    cmd.Parameters.AddWithValue("@CounterSessionId", SessionContext.CounterSessionId);
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        private DataTable GetDistinctColumnValues(string logType, string columnName)
        {
            DataTable result = new DataTable();
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                string tableName = GetTableName(logType);
                EnsureActivityLogTable(tableName);

                using (SqlCommand cmd = new SqlCommand($@"
SELECT DISTINCT ISNULL({columnName}, '') AS Value
FROM dbo.{tableName}
WHERE ISNULL({columnName}, '') <> ''
ORDER BY Value;", (SqlConnection)DataConnection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(result);
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return result;
        }

        private static void ApplyPersistentDisplayLogNumbers(DataTable table)
        {
            if (table == null)
            {
                return;
            }

            const string displayColumn = "DisplayLogNo";
            if (!table.Columns.Contains(displayColumn))
            {
                table.Columns.Add(displayColumn, typeof(int));
            }

            int maxRealLogId = 0;
            foreach (DataRow row in table.Rows)
            {
                int activityLogId = row["ActivityLogId"] == DBNull.Value ? 0 : Convert.ToInt32(row["ActivityLogId"]);
                if (activityLogId > maxRealLogId)
                {
                    maxRealLogId = activityLogId;
                }
            }

            DataView chronologicalView = new DataView(table)
            {
                Sort = "CreatedOn ASC, ActivityLogId ASC, TransactionNo ASC"
            };

            int syntheticLogNo = maxRealLogId + 1;
            foreach (DataRowView rowView in chronologicalView)
            {
                int activityLogId = rowView.Row["ActivityLogId"] == DBNull.Value ? 0 : Convert.ToInt32(rowView.Row["ActivityLogId"]);
                rowView.Row[displayColumn] = activityLogId > 0 ? activityLogId : syntheticLogNo++;
            }
        }

        private static DataTable FilterActivityRows(DataTable source, string userName, string activityType, string searchText)
        {
            if (source == null)
            {
                return new DataTable();
            }

            DataTable filtered = source.Clone();
            string userFilter = userName ?? string.Empty;
            string activityFilter = activityType ?? string.Empty;
            string textFilter = searchText ?? string.Empty;

            foreach (DataRow row in source.Rows)
            {
                if (!string.IsNullOrWhiteSpace(userFilter) &&
                    !string.Equals(Convert.ToString(row["UserName"]), userFilter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string rowActivity = Convert.ToString(row["ActivityType"]);
                if (!string.IsNullOrWhiteSpace(activityFilter) &&
                    !string.Equals(rowActivity, activityFilter, StringComparison.OrdinalIgnoreCase) &&
                    !(IsHoldActivityFilter(activityFilter) && IsHoldActivityRow(rowActivity)))
                {
                    continue;
                }

                if (!MatchesActivitySearch(row, textFilter))
                {
                    continue;
                }

                filtered.ImportRow(row);
            }

            return filtered;
        }

        private static bool IsHoldActivityRow(string activityType)
        {
            return string.Equals(activityType, "HOLD UPDATE", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(activityType, "HOLD COMPLETED", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(activityType, "Hold bill updated", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(activityType, "Hold bill saved", StringComparison.OrdinalIgnoreCase);
        }

        private static bool MatchesActivitySearch(DataRow row, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return true;
            }

            return ContainsText(Convert.ToString(row["TransactionNo"]), searchText) ||
                   ContainsText(Convert.ToString(row["InvoiceNo"]), searchText) ||
                   ContainsText(Convert.ToString(row["PartyName"]), searchText);
        }

        private static bool ContainsText(string value, string searchText)
        {
            return !string.IsNullOrEmpty(value) &&
                   value.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void AppendRecoveredTransactionRows(DataTable result, string logType, DateTime fromDate, DateTime toDate, string userName, string activityType, string searchText)
        {
            bool isSalesLog = string.Equals(logType, "Sales", StringComparison.OrdinalIgnoreCase);
            bool isPurchaseLog = string.Equals(logType, "Purchase", StringComparison.OrdinalIgnoreCase);
            if (!isSalesLog && !isPurchaseLog)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(activityType) &&
                !string.Equals(activityType, "SAVE", StringComparison.OrdinalIgnoreCase) &&
                !(isSalesLog && IsHoldActivityFilter(activityType)))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(userName))
            {
                return;
            }

            string sql = string.Equals(logType, "Purchase", StringComparison.OrdinalIgnoreCase)
                ? BuildRecoveredPurchaseSql()
                : BuildRecoveredSalesSql();

            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
            {
                cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                cmd.Parameters.AddWithValue("@ToDate", toDate.Date);
                cmd.Parameters.AddWithValue("@ActivityType", activityType ?? string.Empty);
                cmd.Parameters.AddWithValue("@SearchText", searchText ?? string.Empty);

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(result);
                }
            }
        }

        private static bool IsHoldActivityFilter(string activityType)
        {
            return string.Equals(activityType, "HOLD", StringComparison.OrdinalIgnoreCase);
        }

        private int CountRecoveredTransactions(string logType, DateTime fromDate, DateTime toDate)
        {
            if (!string.Equals(logType, "Sales", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(logType, "Purchase", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            string sql = string.Equals(logType, "Purchase", StringComparison.OrdinalIgnoreCase)
                ? BuildRecoveredPurchaseCountSql()
                : BuildRecoveredSalesCountSql();

            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
            {
                cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                cmd.Parameters.AddWithValue("@ToDate", toDate.Date);
                object value = cmd.ExecuteScalar();
                return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
            }
        }

        private static string BuildRecoveredSalesSql()
        {
            return @"
IF OBJECT_ID('dbo.SMaster', 'U') IS NULL
    SELECT TOP 0
        CAST(0 AS int) AS ActivityLogId,
        CAST(GETDATE() AS datetime) AS CreatedOn,
        CAST(NULL AS nvarchar(150)) AS UserName,
        CAST('SAVE' AS nvarchar(50)) AS ActivityType,
        CAST(0 AS bigint) AS TransactionNo,
        CAST(NULL AS nvarchar(100)) AS InvoiceNo,
        CAST(NULL AS nvarchar(250)) AS PartyName,
        CAST(NULL AS nvarchar(100)) AS PaymentMode,
        CAST(0 AS decimal(18,4)) AS NetAmount,
        CAST(NULL AS decimal(18,4)) AS Qty,
        CAST(NULL AS decimal(18,4)) AS Cost,
        CAST(NULL AS nvarchar(50)) AS Unit,
        CAST(NULL AS nvarchar(100)) AS Barcode,
        CAST(NULL AS decimal(18,4)) AS SPrice,
        CAST(NULL AS decimal(18,4)) AS TaxAmt,
        CAST(NULL AS decimal(18,4)) AS TaxPer,
        CAST(NULL AS decimal(18,4)) AS BaseAmount,
        CAST(NULL AS decimal(18,4)) AS Packing,
        CAST(NULL AS decimal(18,4)) AS RetailPrice,
        CAST(NULL AS decimal(18,4)) AS Free,
        CAST(NULL AS decimal(18,4)) AS UnitSP,
        CAST(NULL AS nvarchar(50)) AS TaxType,
        CAST(NULL AS decimal(18,4)) AS Gross,
        CAST(NULL AS nvarchar(MAX)) AS ActivityDetails,
        CAST(0 AS int) AS CompanyId,
        CAST(0 AS int) AS BranchId,
        CAST(0 AS int) AS FinYearId,
        CAST(0 AS int) AS UserId,
        CAST(NULL AS nvarchar(150)) AS CounterName,
        CAST(0 AS int) AS CounterId,
        CAST(0 AS bigint) AS CounterSessionId
ELSE
    SELECT
        CAST(-sm.BillNo AS int) AS ActivityLogId,
        sm.BillDate AS CreatedOn,
        CAST(COALESCE(NULLIF(u.UserName, ''), CASE WHEN ISNULL(sm.UserId, 0) = 0 THEN NULL ELSE 'User ' + CONVERT(nvarchar(20), sm.UserId) END) AS nvarchar(150)) AS UserName,
        CAST(CASE WHEN ISNULL(sm.Status, '') = 'Hold' THEN 'HOLD' ELSE 'SAVE' END AS nvarchar(50)) AS ActivityType,
        CAST(sm.BillNo AS bigint) AS TransactionNo,
        CONVERT(nvarchar(100), sm.BillNo) AS InvoiceNo,
        CAST(sm.CustomerName AS nvarchar(250)) AS PartyName,
        CAST(CASE WHEN ISNULL(sm.PaymodeName, '') = 'Credit' AND ISNULL(sm.CreditDays, 0) > 0 THEN CONVERT(nvarchar(20), sm.CreditDays) ELSE sm.PaymodeName END AS nvarchar(100)) AS PaymentMode,
        CAST(ISNULL(sm.NetAmount, 0) AS decimal(18,4)) AS NetAmount,
        d.Qty,
        d.Cost,
        d.Unit,
        d.Barcode,
        d.SPrice,
        d.TaxAmt,
        d.TaxPer,
        d.BaseAmount,
        CAST(NULL AS decimal(18,4)) AS Packing,
        CAST(NULL AS decimal(18,4)) AS RetailPrice,
        CAST(NULL AS decimal(18,4)) AS Free,
        CAST(NULL AS decimal(18,4)) AS UnitSP,
        CAST(NULL AS nvarchar(50)) AS TaxType,
        CAST(NULL AS decimal(18,4)) AS Gross,
        CAST(
            CASE WHEN ISNULL(sm.Status, '') = 'Hold'
                THEN 'Sales invoice #' + CONVERT(nvarchar(50), sm.BillNo) + ' hold processed.' + CHAR(13) + CHAR(10) + 'Holded items:' + ISNULL(d.ItemLines, '')
                ELSE 'Sales invoice #' + CONVERT(nvarchar(50), sm.BillNo) + ' saved.' + CHAR(13) + CHAR(10) + 'Items:' + ISNULL(d.ItemLines, '')
            END AS nvarchar(MAX)) AS ActivityDetails,
        CAST(ISNULL(sm.CompanyId, 0) AS int) AS CompanyId,
        CAST(ISNULL(sm.BranchId, 0) AS int) AS BranchId,
        CAST(ISNULL(sm.FinYearId, 0) AS int) AS FinYearId,
        CAST(ISNULL(sm.UserId, 0) AS int) AS UserId,
        CAST(NULL AS nvarchar(150)) AS CounterName,
        CAST(ISNULL(sm.CounterId, 0) AS int) AS CounterId,
        CAST(ISNULL(sm.CounterSessionId, 0) AS bigint) AS CounterSessionId
    FROM dbo.SMaster sm
    LEFT JOIN dbo.Users u
      ON u.UserID = sm.UserId
    OUTER APPLY
    (
        SELECT
            CAST(SUM(ISNULL(sd.Qty, 0)) AS decimal(18,4)) AS Qty,
            CAST(SUM(ISNULL(sd.Cost, 0) * ISNULL(sd.Qty, 0)) AS decimal(18,4)) AS Cost,
            CAST(MAX(sd.Unit) AS nvarchar(50)) AS Unit,
            CAST(MAX(sd.Barcode) AS nvarchar(100)) AS Barcode,
            CAST(MAX(sd.UnitPrice) AS decimal(18,4)) AS SPrice,
            CAST(SUM(ISNULL(sd.TaxAmt, 0)) AS decimal(18,4)) AS TaxAmt,
            CAST(MAX(sd.TaxPer) AS decimal(18,4)) AS TaxPer,
            CAST(SUM(ISNULL(sd.BaseAmount, 0)) AS decimal(18,4)) AS BaseAmount,
            CAST(
                (
                    SELECT CHAR(13) + CHAR(10)
                        + '- ""' + ISNULL(sd2.ItemName, 'Item') + '""'
                        + ', Qty: ' + CONVERT(nvarchar(50), CAST(ISNULL(sd2.Qty, 0) AS decimal(18,4)))
                        + ', S/Price: ' + CONVERT(nvarchar(50), CAST(ISNULL(sd2.UnitPrice, 0) AS decimal(18,4)))
                        + ', TaxAmt: ' + CONVERT(nvarchar(50), CAST(ISNULL(sd2.TaxAmt, 0) AS decimal(18,4)))
                        + ', TotalAmount: ' + CONVERT(nvarchar(50), CAST(ISNULL(sd2.TotalAmount, 0) AS decimal(18,4)))
                        + CASE WHEN ISNULL(sd2.Unit, '') <> '' THEN ', Unit: ' + sd2.Unit ELSE '' END
                        + CASE WHEN ISNULL(sd2.Barcode, '') <> '' THEN ', Barcode: ' + sd2.Barcode ELSE '' END
                    FROM dbo.SDetails sd2
                    WHERE sd2.BillNo = sm.BillNo
                      AND sd2.BranchId = sm.BranchId
                      AND sd2.CompanyId = sm.CompanyId
                      AND sd2.FinYearId = sm.FinYearId
                    ORDER BY sd2.SlNO
                    FOR XML PATH(''), TYPE
                ).value('.', 'nvarchar(max)') AS nvarchar(MAX)) AS ItemLines
        FROM dbo.SDetails sd
        WHERE sd.BillNo = sm.BillNo
          AND sd.BranchId = sm.BranchId
          AND sd.CompanyId = sm.CompanyId
          AND sd.FinYearId = sm.FinYearId
    ) d
    WHERE ISNULL(sm.CancelFlag, 0) = 0
      AND sm.BillDate >= @FromDate
      AND sm.BillDate < DATEADD(DAY, 1, @ToDate)
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.SalesActivityLog sal
          WHERE sal.TransactionNo = sm.BillNo
      )
      AND (
            @ActivityType = ''
            OR CASE WHEN ISNULL(sm.Status, '') = 'Hold' THEN 'HOLD' ELSE 'SAVE' END = @ActivityType
          )
      AND (
            @SearchText = ''
            OR CONVERT(nvarchar(50), sm.BillNo) LIKE '%' + @SearchText + '%'
            OR ISNULL(sm.CustomerName, '') LIKE '%' + @SearchText + '%'
          )
    ORDER BY sm.BillDate DESC, sm.BillNo DESC;";
        }

        private static string BuildRecoveredPurchaseSql()
        {
            return @"
IF OBJECT_ID('dbo.PMaster', 'U') IS NULL
    SELECT TOP 0
        CAST(0 AS int) AS ActivityLogId,
        CAST(GETDATE() AS datetime) AS CreatedOn,
        CAST(NULL AS nvarchar(150)) AS UserName,
        CAST('SAVE' AS nvarchar(50)) AS ActivityType,
        CAST(0 AS bigint) AS TransactionNo,
        CAST(NULL AS nvarchar(100)) AS InvoiceNo,
        CAST(NULL AS nvarchar(250)) AS PartyName,
        CAST(NULL AS nvarchar(100)) AS PaymentMode,
        CAST(0 AS decimal(18,4)) AS NetAmount,
        CAST(NULL AS decimal(18,4)) AS Qty,
        CAST(NULL AS decimal(18,4)) AS Cost,
        CAST(NULL AS nvarchar(50)) AS Unit,
        CAST(NULL AS nvarchar(100)) AS Barcode,
        CAST(NULL AS decimal(18,4)) AS SPrice,
        CAST(NULL AS decimal(18,4)) AS TaxAmt,
        CAST(NULL AS decimal(18,4)) AS TaxPer,
        CAST(NULL AS decimal(18,4)) AS BaseAmount,
        CAST(NULL AS decimal(18,4)) AS Packing,
        CAST(NULL AS decimal(18,4)) AS RetailPrice,
        CAST(NULL AS decimal(18,4)) AS Free,
        CAST(NULL AS decimal(18,4)) AS UnitSP,
        CAST(NULL AS nvarchar(50)) AS TaxType,
        CAST(NULL AS decimal(18,4)) AS Gross,
        CAST(NULL AS nvarchar(MAX)) AS ActivityDetails,
        CAST(0 AS int) AS CompanyId,
        CAST(0 AS int) AS BranchId,
        CAST(0 AS int) AS FinYearId,
        CAST(0 AS int) AS UserId,
        CAST(NULL AS nvarchar(150)) AS CounterName,
        CAST(0 AS int) AS CounterId,
        CAST(0 AS bigint) AS CounterSessionId
ELSE
    SELECT
        CAST(0 AS int) AS ActivityLogId,
        pm.PurchaseDate AS CreatedOn,
        CAST(pm.UserName AS nvarchar(150)) AS UserName,
        CAST('SAVE' AS nvarchar(50)) AS ActivityType,
        CAST(pm.PurchaseNo AS bigint) AS TransactionNo,
        CAST(pm.InvoiceNo AS nvarchar(100)) AS InvoiceNo,
        CAST(pm.VendorName AS nvarchar(250)) AS PartyName,
        CAST(pm.Paymode AS nvarchar(100)) AS PaymentMode,
        CAST(ISNULL(NULLIF(pm.NetTotal, 0), pm.GrandTotal) AS decimal(18,4)) AS NetAmount,
        d.Qty,
        d.Cost,
        d.Unit,
        CAST(NULL AS nvarchar(100)) AS Barcode,
        d.SPrice,
        d.TaxAmt,
        d.TaxPer,
        d.BaseAmount,
        d.Packing,
        CAST(NULL AS decimal(18,4)) AS RetailPrice,
        d.Free,
        CAST(NULL AS decimal(18,4)) AS UnitSP,
        d.TaxType,
        d.Gross,
        CAST('Recovered from saved purchase invoice.' AS nvarchar(MAX)) AS ActivityDetails,
        CAST(ISNULL(pm.CompanyId, 0) AS int) AS CompanyId,
        CAST(ISNULL(pm.BranchId, 0) AS int) AS BranchId,
        CAST(ISNULL(pm.FinYearId, 0) AS int) AS FinYearId,
        CAST(ISNULL(pm.UserID, 0) AS int) AS UserId,
        CAST(NULL AS nvarchar(150)) AS CounterName,
        CAST(0 AS int) AS CounterId,
        CAST(0 AS bigint) AS CounterSessionId
    FROM dbo.PMaster pm
    OUTER APPLY
    (
        SELECT
            CAST(SUM(ISNULL(pd.Qty, 0)) AS decimal(18,4)) AS Qty,
            CAST(SUM(ISNULL(pd.Cost, 0) * ISNULL(pd.Qty, 0)) AS decimal(18,4)) AS Cost,
            CAST(MAX(pd.Unit) AS nvarchar(50)) AS Unit,
            CAST(MAX(pd.SalesPrice) AS decimal(18,4)) AS SPrice,
            CAST(SUM(ISNULL(pd.TaxAmt, 0)) AS decimal(18,4)) AS TaxAmt,
            CAST(MAX(pd.TaxPer) AS decimal(18,4)) AS TaxPer,
            CAST(SUM(ISNULL(pd.Cost, 0) * ISNULL(pd.Qty, 0)) AS decimal(18,4)) AS BaseAmount,
            CAST(MAX(pd.Packing) AS decimal(18,4)) AS Packing,
            CAST(SUM(ISNULL(pd.Free, 0)) AS decimal(18,4)) AS Free,
            CAST(MAX(pd.TaxType) AS nvarchar(50)) AS TaxType,
            CAST(SUM(ISNULL(pd.TotalSP, 0)) AS decimal(18,4)) AS Gross
        FROM dbo.PDetails pd
        WHERE pd.PurchaseNo = pm.PurchaseNo
          AND pd.BranchID = pm.BranchID
          AND pd.CompanyId = pm.CompanyId
          AND pd.FinYearId = pm.FinYearId
    ) d
    WHERE ISNULL(pm.CancelFlag, 0) = 0
      AND pm.PurchaseDate >= @FromDate
      AND pm.PurchaseDate < DATEADD(DAY, 1, @ToDate)
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.PurchaseActivityLog pal
          WHERE pal.TransactionNo = pm.PurchaseNo
            AND ISNULL(pal.ActivityType, '') = 'SAVE'
      )
      AND (
            @SearchText = ''
            OR CONVERT(nvarchar(50), pm.PurchaseNo) LIKE '%' + @SearchText + '%'
            OR ISNULL(pm.InvoiceNo, '') LIKE '%' + @SearchText + '%'
            OR ISNULL(pm.VendorName, '') LIKE '%' + @SearchText + '%'
          )
    ORDER BY pm.PurchaseDate DESC, pm.PurchaseNo DESC;";
        }

        private static string BuildRecoveredSalesCountSql()
        {
            return @"
IF OBJECT_ID('dbo.SMaster', 'U') IS NULL
    SELECT 0
ELSE
    SELECT COUNT(1)
    FROM dbo.SMaster sm
    WHERE ISNULL(sm.CancelFlag, 0) = 0
      AND sm.BillDate >= @FromDate
      AND sm.BillDate < DATEADD(DAY, 1, @ToDate)
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.SalesActivityLog sal
          WHERE sal.TransactionNo = sm.BillNo
      );";
        }

        private static string BuildRecoveredPurchaseCountSql()
        {
            return @"
IF OBJECT_ID('dbo.PMaster', 'U') IS NULL
    SELECT 0
ELSE
    SELECT COUNT(1)
    FROM dbo.PMaster pm
    WHERE ISNULL(pm.CancelFlag, 0) = 0
      AND pm.PurchaseDate >= @FromDate
      AND pm.PurchaseDate < DATEADD(DAY, 1, @ToDate)
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.PurchaseActivityLog pal
          WHERE pal.TransactionNo = pm.PurchaseNo
            AND ISNULL(pal.ActivityType, '') = 'SAVE'
      );";
        }

        private void EnsureActivityLogTable(string tableName)
        {
            using (SqlCommand cmd = new SqlCommand($@"
IF OBJECT_ID('dbo.{tableName}', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.{tableName}
    (
        ActivityLogId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TransactionNo BIGINT NOT NULL DEFAULT(0),
        InvoiceNo NVARCHAR(100) NULL,
        PartyName NVARCHAR(250) NULL,
        PaymentMode NVARCHAR(100) NULL,
        NetAmount DECIMAL(18,4) NOT NULL DEFAULT(0),
        ActivityType NVARCHAR(50) NOT NULL,
        ActivityDetails NVARCHAR(MAX) NULL,
        Qty DECIMAL(18,4) NULL,
        Cost DECIMAL(18,4) NULL,
        Unit NVARCHAR(50) NULL,
        Barcode NVARCHAR(100) NULL,
        SPrice DECIMAL(18,4) NULL,
        TaxAmt DECIMAL(18,4) NULL,
        TaxPer DECIMAL(18,4) NULL,
        BaseAmount DECIMAL(18,4) NULL,
        Packing DECIMAL(18,4) NULL,
        RetailPrice DECIMAL(18,4) NULL,
        Free DECIMAL(18,4) NULL,
        UnitSP DECIMAL(18,4) NULL,
        TaxType NVARCHAR(50) NULL,
        Gross DECIMAL(18,4) NULL,
        CompanyId INT NOT NULL DEFAULT(0),
        BranchId INT NOT NULL DEFAULT(0),
        FinYearId INT NOT NULL DEFAULT(0),
        UserId INT NOT NULL DEFAULT(0),
        UserName NVARCHAR(150) NULL,
        CounterId INT NOT NULL DEFAULT(0),
        CounterName NVARCHAR(150) NULL,
        CounterSessionId BIGINT NOT NULL DEFAULT(0),
        CreatedOn DATETIME NOT NULL DEFAULT(GETDATE())
    );

    CREATE INDEX IX_{tableName}_TransactionNo ON dbo.{tableName}(TransactionNo, CreatedOn);
    CREATE INDEX IX_{tableName}_UserCounter ON dbo.{tableName}(UserId, CounterId, CreatedOn);
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.{tableName}', 'ActivityLogId') IS NULL
        ALTER TABLE dbo.{tableName} ADD ActivityLogId INT IDENTITY(1,1) NOT NULL;

    IF COL_LENGTH('dbo.{tableName}', 'TransactionNo') IS NULL
        ALTER TABLE dbo.{tableName} ADD TransactionNo BIGINT NOT NULL CONSTRAINT DF_{tableName}_TransactionNo DEFAULT(0);

    IF COL_LENGTH('dbo.{tableName}', 'InvoiceNo') IS NULL
        ALTER TABLE dbo.{tableName} ADD InvoiceNo NVARCHAR(100) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'PartyName') IS NULL
        ALTER TABLE dbo.{tableName} ADD PartyName NVARCHAR(250) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'PaymentMode') IS NULL
        ALTER TABLE dbo.{tableName} ADD PaymentMode NVARCHAR(100) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'NetAmount') IS NULL
        ALTER TABLE dbo.{tableName} ADD NetAmount DECIMAL(18,4) NOT NULL CONSTRAINT DF_{tableName}_NetAmount DEFAULT(0);

    IF COL_LENGTH('dbo.{tableName}', 'ActivityType') IS NULL
        ALTER TABLE dbo.{tableName} ADD ActivityType NVARCHAR(50) NOT NULL CONSTRAINT DF_{tableName}_ActivityType DEFAULT('');

    IF COL_LENGTH('dbo.{tableName}', 'ActivityDetails') IS NULL
        ALTER TABLE dbo.{tableName} ADD ActivityDetails NVARCHAR(MAX) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'Qty') IS NULL
        ALTER TABLE dbo.{tableName} ADD Qty DECIMAL(18,4) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'Cost') IS NULL
        ALTER TABLE dbo.{tableName} ADD Cost DECIMAL(18,4) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'Unit') IS NULL
        ALTER TABLE dbo.{tableName} ADD Unit NVARCHAR(50) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'Barcode') IS NULL
        ALTER TABLE dbo.{tableName} ADD Barcode NVARCHAR(100) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'SPrice') IS NULL
        ALTER TABLE dbo.{tableName} ADD SPrice DECIMAL(18,4) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'TaxAmt') IS NULL
        ALTER TABLE dbo.{tableName} ADD TaxAmt DECIMAL(18,4) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'TaxPer') IS NULL
        ALTER TABLE dbo.{tableName} ADD TaxPer DECIMAL(18,4) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'BaseAmount') IS NULL
        ALTER TABLE dbo.{tableName} ADD BaseAmount DECIMAL(18,4) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'Packing') IS NULL
        ALTER TABLE dbo.{tableName} ADD Packing DECIMAL(18,4) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'RetailPrice') IS NULL
        ALTER TABLE dbo.{tableName} ADD RetailPrice DECIMAL(18,4) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'Free') IS NULL
        ALTER TABLE dbo.{tableName} ADD Free DECIMAL(18,4) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'UnitSP') IS NULL
        ALTER TABLE dbo.{tableName} ADD UnitSP DECIMAL(18,4) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'TaxType') IS NULL
        ALTER TABLE dbo.{tableName} ADD TaxType NVARCHAR(50) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'Gross') IS NULL
        ALTER TABLE dbo.{tableName} ADD Gross DECIMAL(18,4) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'ActivityDetails') IS NOT NULL
        ALTER TABLE dbo.{tableName} ALTER COLUMN ActivityDetails NVARCHAR(MAX) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'CompanyId') IS NULL
        ALTER TABLE dbo.{tableName} ADD CompanyId INT NOT NULL CONSTRAINT DF_{tableName}_CompanyId DEFAULT(0);

    IF COL_LENGTH('dbo.{tableName}', 'BranchId') IS NULL
        ALTER TABLE dbo.{tableName} ADD BranchId INT NOT NULL CONSTRAINT DF_{tableName}_BranchId DEFAULT(0);

    IF COL_LENGTH('dbo.{tableName}', 'FinYearId') IS NULL
        ALTER TABLE dbo.{tableName} ADD FinYearId INT NOT NULL CONSTRAINT DF_{tableName}_FinYearId DEFAULT(0);

    IF COL_LENGTH('dbo.{tableName}', 'UserId') IS NULL
        ALTER TABLE dbo.{tableName} ADD UserId INT NOT NULL CONSTRAINT DF_{tableName}_UserId DEFAULT(0);

    IF COL_LENGTH('dbo.{tableName}', 'UserName') IS NULL
        ALTER TABLE dbo.{tableName} ADD UserName NVARCHAR(150) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'CounterId') IS NULL
        ALTER TABLE dbo.{tableName} ADD CounterId INT NOT NULL CONSTRAINT DF_{tableName}_CounterId DEFAULT(0);

    IF COL_LENGTH('dbo.{tableName}', 'CounterName') IS NULL
        ALTER TABLE dbo.{tableName} ADD CounterName NVARCHAR(150) NULL;

    IF COL_LENGTH('dbo.{tableName}', 'CounterSessionId') IS NULL
        ALTER TABLE dbo.{tableName} ADD CounterSessionId BIGINT NOT NULL CONSTRAINT DF_{tableName}_CounterSessionId DEFAULT(0);

    IF COL_LENGTH('dbo.{tableName}', 'CreatedOn') IS NULL
        ALTER TABLE dbo.{tableName} ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_{tableName}_CreatedOn DEFAULT(GETDATE());
END;", (SqlConnection)DataConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private void EnsureTransactionActivityLogStoredProcedure()
        {
            using (SqlCommand cmd = new SqlCommand(@"
IF OBJECT_ID(N'dbo.POS_TransactionActivityLog', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo.POS_TransactionActivityLog AS BEGIN SET NOCOUNT ON; END');", (SqlConnection)DataConnection))
            {
                cmd.ExecuteNonQuery();
            }

            using (SqlCommand cmd = new SqlCommand(@"
ALTER PROCEDURE dbo.POS_TransactionActivityLog
    @_Operation NVARCHAR(30),
    @LogType NVARCHAR(30),
    @TransactionNo BIGINT = 0,
    @InvoiceNo NVARCHAR(100) = NULL,
    @PartyName NVARCHAR(250) = NULL,
    @PaymentMode NVARCHAR(100) = NULL,
    @NetAmount DECIMAL(18,4) = 0,
    @Qty DECIMAL(18,4) = NULL,
    @Cost DECIMAL(18,4) = NULL,
    @Unit NVARCHAR(50) = NULL,
    @Barcode NVARCHAR(100) = NULL,
    @SPrice DECIMAL(18,4) = NULL,
    @TaxAmt DECIMAL(18,4) = NULL,
    @TaxPer DECIMAL(18,4) = NULL,
    @BaseAmount DECIMAL(18,4) = NULL,
    @Packing DECIMAL(18,4) = NULL,
    @RetailPrice DECIMAL(18,4) = NULL,
    @Free DECIMAL(18,4) = NULL,
    @UnitSP DECIMAL(18,4) = NULL,
    @TaxType NVARCHAR(50) = NULL,
    @Gross DECIMAL(18,4) = NULL,
    @ActivityType NVARCHAR(50) = NULL,
    @ActivityDetails NVARCHAR(MAX) = NULL,
    @CompanyId INT = 0,
    @BranchId INT = 0,
    @FinYearId INT = 0,
    @UserId INT = 0,
    @UserName NVARCHAR(150) = NULL,
    @CounterId INT = 0,
    @CounterName NVARCHAR(150) = NULL,
    @CounterSessionId BIGINT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TableName SYSNAME;

    IF @LogType = N'Purchase'
        SET @TableName = N'PurchaseActivityLog';
    ELSE IF @LogType = N'Sales'
        SET @TableName = N'SalesActivityLog';
    ELSE IF @LogType = N'Purchase Return'
        SET @TableName = N'PurchaseReturnActivityLog';
    ELSE IF @LogType = N'Sales Return'
        SET @TableName = N'SalesReturnActivityLog';
    ELSE IF @LogType = N'Stock Adjustment'
        SET @TableName = N'StockAdjustmentActivityLog';
    ELSE
    BEGIN
        RAISERROR('Unsupported transaction activity log type.', 16, 1);
        RETURN;
    END

    IF @_Operation = N'SAVE'
    BEGIN
        DECLARE @Sql NVARCHAR(MAX) = N'
INSERT INTO dbo.' + QUOTENAME(@TableName) + N'
(
    TransactionNo, InvoiceNo, PartyName, PaymentMode, NetAmount,
    Qty, Cost, Unit, Barcode,
    SPrice, TaxAmt, TaxPer, BaseAmount,
    Packing, RetailPrice, Free, UnitSP, TaxType, Gross,
    ActivityType, ActivityDetails,
    CompanyId, BranchId, FinYearId, UserId, UserName,
    CounterId, CounterName, CounterSessionId, CreatedOn
)
VALUES
(
    @TransactionNo, @InvoiceNo, @PartyName, @PaymentMode, @NetAmount,
    @Qty, @Cost, @Unit, @Barcode,
    @SPrice, @TaxAmt, @TaxPer, @BaseAmount,
    @Packing, @RetailPrice, @Free, @UnitSP, @TaxType, @Gross,
    @ActivityType, @ActivityDetails,
    @CompanyId, @BranchId, @FinYearId, @UserId, @UserName,
    @CounterId, @CounterName, @CounterSessionId, GETDATE()
);';

        EXEC sp_executesql
            @Sql,
            N'@TransactionNo BIGINT, @InvoiceNo NVARCHAR(100), @PartyName NVARCHAR(250), @PaymentMode NVARCHAR(100), @NetAmount DECIMAL(18,4), @Qty DECIMAL(18,4), @Cost DECIMAL(18,4), @Unit NVARCHAR(50), @Barcode NVARCHAR(100), @SPrice DECIMAL(18,4), @TaxAmt DECIMAL(18,4), @TaxPer DECIMAL(18,4), @BaseAmount DECIMAL(18,4), @Packing DECIMAL(18,4), @RetailPrice DECIMAL(18,4), @Free DECIMAL(18,4), @UnitSP DECIMAL(18,4), @TaxType NVARCHAR(50), @Gross DECIMAL(18,4), @ActivityType NVARCHAR(50), @ActivityDetails NVARCHAR(MAX), @CompanyId INT, @BranchId INT, @FinYearId INT, @UserId INT, @UserName NVARCHAR(150), @CounterId INT, @CounterName NVARCHAR(150), @CounterSessionId BIGINT',
            @TransactionNo, @InvoiceNo, @PartyName, @PaymentMode, @NetAmount, @Qty, @Cost, @Unit, @Barcode,
            @SPrice, @TaxAmt, @TaxPer, @BaseAmount,
            @Packing, @RetailPrice, @Free, @UnitSP, @TaxType, @Gross,
            @ActivityType, @ActivityDetails,
            @CompanyId, @BranchId, @FinYearId, @UserId, @UserName,
            @CounterId, @CounterName, @CounterSessionId;
    END
END;", (SqlConnection)DataConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private static string GetTableName(string logType)
        {
            if (string.Equals(logType, "Purchase", StringComparison.OrdinalIgnoreCase))
            {
                return "PurchaseActivityLog";
            }

            if (string.Equals(logType, "Sales", StringComparison.OrdinalIgnoreCase))
            {
                return "SalesActivityLog";
            }

            if (string.Equals(logType, "Purchase Return", StringComparison.OrdinalIgnoreCase))
            {
                return "PurchaseReturnActivityLog";
            }

            if (string.Equals(logType, "Sales Return", StringComparison.OrdinalIgnoreCase))
            {
                return "SalesReturnActivityLog";
            }

            if (string.Equals(logType, "Stock Adjustment", StringComparison.OrdinalIgnoreCase))
            {
                return "StockAdjustmentActivityLog";
            }

            throw new ArgumentException("Unsupported activity log type.", nameof(logType));
        }

        private static int GetCompanyId()
        {
            return SessionContext.CompanyId > 0 ? SessionContext.CompanyId : ParseInt(DataBase.CompanyId);
        }

        private static int GetBranchId()
        {
            return SessionContext.BranchId > 0 ? SessionContext.BranchId : ParseInt(DataBase.BranchId);
        }

        private static int GetFinYearId()
        {
            return SessionContext.FinYearId > 0 ? SessionContext.FinYearId : ParseInt(DataBase.FinyearId);
        }

        private static int GetUserId()
        {
            return SessionContext.UserId > 0 ? SessionContext.UserId : ParseInt(DataBase.UserId);
        }

        private static string GetUserName()
        {
            return !string.IsNullOrWhiteSpace(SessionContext.UserName) ? SessionContext.UserName : DataBase.UserName;
        }

        private static int ParseInt(string value)
        {
            int parsed;
            return int.TryParse(value, out parsed) ? parsed : 0;
        }
    }
}
