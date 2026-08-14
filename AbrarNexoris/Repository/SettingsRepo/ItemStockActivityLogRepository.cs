using ModelClass;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace Repository.SettingsRepo
{
    public class ItemStockActivityLogRepository : BaseRepostitory
    {
        public DataTable GetItemStockActivityLog(DateTime fromDate, DateTime toDate, string userName, string action, string itemSearch)
        {
            DataTable result = ExecuteTable("GET", fromDate, toDate, userName, action, itemSearch);
            AppendRecoveredPurchaseRows(result, fromDate, toDate, userName, action, itemSearch);
            ApplyActivityLogMetadata(result, fromDate, toDate);
            ResolveMissingStockPartyNames(result);
            ApplyActivityQuantitySnapshots(result);
            NormalizePurchaseReturnMovements(result);
            result = SortActivityTable(result);
            ApplyStableStockTimeline(result);
            return result;
        }

        public DataTable GetItemStockHistoryLog(string searchText)
        {
            return GetItemStockActivityLog(new DateTime(2000, 1, 1), DateTime.Today.AddYears(5), string.Empty, string.Empty, searchText);
        }

        public DataTable GetItemStockActivityUsers()
        {
            DataTable users = ExecuteTable("GETUSERS", DateTime.MinValue, DateTime.MinValue, string.Empty, string.Empty, string.Empty);
            AppendRecoveredPurchaseUsers(users);
            return SortDistinctValues(users);
        }

        public DataTable GetItemStockActivityActions()
        {
            DataTable table = ExecuteTable("GETACTIONS", DateTime.MinValue, DateTime.MinValue, string.Empty, string.Empty, string.Empty);
            if (table.Rows.Count > 0)
            {
                return table;
            }

            table.Columns.Add("Value", typeof(string));
            table.Rows.Add("Sales");
            table.Rows.Add("Purchase");
            table.Rows.Add("Sales Return");
            table.Rows.Add("Purchase Return");
            table.Rows.Add("Stock IN");
            table.Rows.Add("Stock OUT");
            return table;
        }

        public int CountItemStockActivity(DateTime fromDate, DateTime toDate)
        {
            return GetItemStockActivityLog(fromDate, toDate, string.Empty, string.Empty, string.Empty).Rows.Count;
        }

        public DateTime GetLatestActivityStamp()
        {
            object value = ExecuteScalar("LATESTSTAMP", DateTime.MinValue, DateTime.MinValue, string.Empty, string.Empty, string.Empty);
            DateTime latest = value == null || value == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(value);
            DateTime purchaseLatest = GetRecoveredPurchaseLatestStamp();
            DateTime activityLatest = GetLatestTransactionActivityStamp();
            latest = purchaseLatest > latest ? purchaseLatest : latest;
            return activityLatest > latest ? activityLatest : latest;
        }

        private DataTable ExecuteTable(string operation, DateTime fromDate, DateTime toDate, string userName, string action, string itemSearch)
        {
            DataTable result = new DataTable();
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                using (SqlCommand cmd = CreateCommand(operation, fromDate, toDate, userName, action, itemSearch))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(result);
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("Item stock activity procedure failed: " + ex.Message);
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

        private object ExecuteScalar(string operation, DateTime fromDate, DateTime toDate, string userName, string action, string itemSearch)
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                using (SqlCommand cmd = CreateCommand(operation, fromDate, toDate, userName, action, itemSearch))
                {
                    return cmd.ExecuteScalar();
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("Item stock activity scalar procedure failed: " + ex.Message);
                return null;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        private SqlCommand CreateCommand(string operation, DateTime fromDate, DateTime toDate, string userName, string action, string itemSearch)
        {
            SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemStockActivityLog, (SqlConnection)DataConnection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@_Operation", operation);
            cmd.Parameters.AddWithValue("@FromDate", fromDate == DateTime.MinValue ? (object)DBNull.Value : fromDate.Date);
            cmd.Parameters.AddWithValue("@ToDate", toDate == DateTime.MinValue ? (object)DBNull.Value : toDate.Date);
            cmd.Parameters.AddWithValue("@UserName", userName ?? string.Empty);
            cmd.Parameters.AddWithValue("@Action", action ?? string.Empty);
            cmd.Parameters.AddWithValue("@ItemSearch", itemSearch ?? string.Empty);
            cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
            cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
            cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
            return cmd;
        }

        private void AppendRecoveredPurchaseRows(DataTable target, DateTime fromDate, DateTime toDate, string userName, string action, string itemSearch)
        {
            if (target == null ||
                (!string.IsNullOrWhiteSpace(action) && !string.Equals(action, "Purchase", StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            DataTable purchaseRows = GetRecoveredPurchaseRows(fromDate, toDate, userName, itemSearch);
            if (purchaseRows == null || purchaseRows.Rows.Count == 0)
            {
                return;
            }

            AddMissingColumns(target, purchaseRows);

            foreach (DataRow purchaseRow in purchaseRows.Rows)
            {
                DataRow existing = FindExistingPurchaseRow(target, purchaseRow);
                if (existing == null)
                {
                    target.ImportRow(purchaseRow);
                    continue;
                }

                FillPurchaseRowGaps(existing, purchaseRow);
            }
        }

        private DataTable GetRecoveredPurchaseRows(DateTime fromDate, DateTime toDate, string userName, string itemSearch)
        {
            DataTable result = new DataTable();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                if (!TableExists("PMaster") || !TableExists("PDetails"))
                {
                    return result;
                }

                string purchaseActivityApply = TableExists("PurchaseActivityLog")
                    ? @"
    OUTER APPLY
    (
        SELECT TOP 1
            pal.ActivityLogId,
            pal.CreatedOn,
            pal.UserName,
            pal.UserId,
            pal.CounterName,
            pal.CounterId,
            pal.CounterSessionId
        FROM dbo.PurchaseActivityLog pal
        WHERE pal.TransactionNo = pm.PurchaseNo
          AND (ISNULL(pal.CompanyId, 0) = 0 OR ISNULL(pal.CompanyId, 0) = ISNULL(pm.CompanyId, 0))
          AND (ISNULL(pal.BranchId, 0) = 0 OR ISNULL(pal.BranchId, 0) = ISNULL(pm.BranchId, 0))
          AND (ISNULL(pal.FinYearId, 0) = 0 OR ISNULL(pal.FinYearId, 0) = ISNULL(pm.FinYearId, 0))
          AND ISNULL(pal.ActivityType, N'') IN (N'SAVE', N'UPDATE')
        ORDER BY pal.CreatedOn DESC, pal.ActivityLogId DESC
    ) pal"
                    : @"
    OUTER APPLY
    (
        SELECT
            CAST(0 AS bigint) AS ActivityLogId,
            CAST(NULL AS datetime) AS CreatedOn,
            CAST(NULL AS nvarchar(150)) AS UserName,
            CAST(0 AS int) AS UserId,
            CAST(NULL AS nvarchar(150)) AS CounterName,
            CAST(0 AS int) AS CounterId,
            CAST(0 AS bigint) AS CounterSessionId
    ) pal";

                using (SqlCommand cmd = new SqlCommand(@"
SELECT
    COALESCE(pal.CreatedOn, CAST(CAST(pm.PurchaseDate AS date) AS datetime)) AS CreatedOn,
    COALESCE(NULLIF(pal.UserName, N''), NULLIF(pm.UserName, N'')) AS UserName,
    N'Purchase' AS Action,
    CAST(2 AS int) AS ActionSort,
    CAST(pm.PurchaseNo AS bigint) AS TransactionNo,
    CAST(pm.InvoiceNo AS nvarchar(100)) AS InvoiceNo,
    CAST(NULL AS nvarchar(100)) AS SalesBillNo,
    CONVERT(nvarchar(100), pm.PurchaseNo) AS PurchaseNo,
    COALESCE(NULLIF(pd.ItemName, N''), im.Description) AS ItemName,
    COALESCE(ps.BarCode, im.BarCode) AS Barcode,
    CAST(pd.Unit AS nvarchar(50)) AS UOM,
    CAST(ISNULL(pd.Qty, 0) AS decimal(18,4)) AS Qty,
    CAST(ISNULL(pd.Qty, 0) AS decimal(18,4)) AS MovementQty,
    CAST(ISNULL(pd.Cost, 0) AS decimal(18,4)) AS UnitPrice,
    CAST(ISNULL(pd.SalesPrice, 0) AS decimal(18,4)) AS SellingPrice,
    CAST(ISNULL(ps.Stock, 0) AS decimal(18,4)) AS Stock,
    CAST(ISNULL(pd.Qty, 0) AS decimal(18,4)) AS StockIn,
    CAST(0 AS decimal(18,4)) AS StockOut,
    CAST(NULL AS decimal(18,4)) AS AdjustmentQty,
    CAST(NULL AS decimal(18,4)) AS NewBalance,
    CAST(ISNULL(pd.Qty, 0) AS decimal(18,4)) AS QtyDifference,
    CAST(NULL AS nvarchar(500)) AS Reason,
    CAST(ISNULL(ps.Stock, 0) AS decimal(18,4)) AS Available,
    CAST(0 AS decimal(18,4)) AS Hold,
    ISNULL(im.Order_Cycle_Days, 0) AS Cycle,
    ISNULL(im.Box_Quantity, 0) AS BoxQty,
    N'Purchase No: ' + CONVERT(nvarchar(50), pm.PurchaseNo) + N', Vendor: ' + ISNULL(pm.VendorName, N'') AS ActivityDetails,
    ISNULL(pm.CompanyId, 0) AS CompanyId,
    ISNULL(pm.BranchId, 0) AS BranchId,
    ISNULL(pm.FinYearId, 0) AS FinYearId,
    COALESCE(NULLIF(pal.UserId, 0), ISNULL(pm.UserID, 0)) AS UserId,
    COALESCE(NULLIF(pal.CounterName, N''), CASE WHEN ISNULL(pal.CounterId, 0) > 0 THEN N'Counter ' + CONVERT(nvarchar(20), pal.CounterId) ELSE NULL END) AS CounterName,
    ISNULL(pal.CounterId, 0) AS CounterId,
    ISNULL(pal.CounterSessionId, 0) AS CounterSessionId,
    ISNULL(pal.ActivityLogId, 0) AS ActivityLogId,
    ISNULL(pd.SlNo, 0) AS SlNo,
    ISNULL(pd.ItemID, 0) AS ItemId,
    ISNULL(pd.UnitId, 0) AS UnitId
FROM dbo.PMaster pm
INNER JOIN dbo.PDetails pd ON pd.PurchaseNo = pm.PurchaseNo
    AND (ISNULL(pd.CompanyId, 0) = 0 OR ISNULL(pd.CompanyId, 0) = ISNULL(pm.CompanyId, 0))
    AND (ISNULL(pd.BranchID, 0) = 0 OR ISNULL(pd.BranchID, 0) = ISNULL(pm.BranchId, 0))
    AND (ISNULL(pd.FinYearId, 0) = 0 OR ISNULL(pd.FinYearId, 0) = ISNULL(pm.FinYearId, 0))
LEFT JOIN dbo.ItemMaster im ON im.ItemId = pd.ItemID
" + purchaseActivityApply + @"
OUTER APPLY
(
    SELECT TOP 1 ps.*
    FROM dbo.PriceSettings ps
    WHERE ps.ItemId = pd.ItemID
    ORDER BY
        CASE WHEN ISNULL(ps.BranchId, 0) = ISNULL(pm.BranchId, 0) THEN 0 ELSE 1 END,
        CASE WHEN ISNULL(ps.UnitId, 0) = ISNULL(pd.UnitId, 0) THEN 0 ELSE 1 END,
        ps.UnitId
) ps
WHERE ISNULL(pm.CancelFlag, 0) = 0
  AND COALESCE(pal.CreatedOn, CAST(CAST(pm.PurchaseDate AS date) AS datetime)) >= @FromDate
  AND COALESCE(pal.CreatedOn, CAST(CAST(pm.PurchaseDate AS date) AS datetime)) < DATEADD(DAY, 1, @ToDate)
  AND (@CompanyId = 0 OR ISNULL(pm.CompanyId, 0) = @CompanyId)
  AND (@BranchId = 0 OR ISNULL(pm.BranchId, 0) = @BranchId)
  AND (@FinYearId = 0 OR ISNULL(pm.FinYearId, 0) = @FinYearId)
  AND (@UserName = N'' OR COALESCE(NULLIF(pal.UserName, N''), pm.UserName, N'') = @UserName)
  AND (@ItemSearch = N'' OR COALESCE(NULLIF(pd.ItemName, N''), im.Description, N'') LIKE N'%' + @ItemSearch + N'%' OR COALESCE(ps.BarCode, im.BarCode, N'') LIKE N'%' + @ItemSearch + N'%');", (SqlConnection)DataConnection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date);
                    cmd.Parameters.AddWithValue("@UserName", userName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@ItemSearch", itemSearch ?? string.Empty);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
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

        private void AppendRecoveredPurchaseUsers(DataTable users)
        {
            if (users == null)
            {
                return;
            }

            if (!users.Columns.Contains("Value"))
            {
                users.Columns.Add("Value", typeof(string));
            }

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                if (!TableExists("PMaster"))
                {
                    return;
                }

                using (SqlCommand cmd = new SqlCommand(@"
SELECT DISTINCT NULLIF(pm.UserName, N'') AS Value
FROM dbo.PMaster pm
WHERE ISNULL(pm.UserName, N'') <> N''
  AND (@CompanyId = 0 OR ISNULL(pm.CompanyId, 0) = @CompanyId)
  AND (@BranchId = 0 OR ISNULL(pm.BranchId, 0) = @BranchId)
  AND (@FinYearId = 0 OR ISNULL(pm.FinYearId, 0) = @FinYearId);", (SqlConnection)DataConnection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    DataTable purchaseUsers = new DataTable();
                    adapter.Fill(purchaseUsers);

                    foreach (DataRow row in purchaseUsers.Rows)
                    {
                        string value = Convert.ToString(row["Value"]);
                        if (!string.IsNullOrWhiteSpace(value) && !ContainsValue(users, value))
                        {
                            users.Rows.Add(value);
                        }
                    }
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

        private static void ApplyActivityQuantitySnapshots(DataTable rows)
        {
            if (rows == null ||
                !rows.Columns.Contains("ActivityQty") ||
                !rows.Columns.Contains("ActivityType") ||
                !rows.Columns.Contains("ActivityLogId"))
            {
                return;
            }

            EnsureColumn(rows, "StockIn", typeof(decimal));
            EnsureColumn(rows, "StockOut", typeof(decimal));
            EnsureColumn(rows, "QtyDifference", typeof(decimal));

            var groups = new Dictionary<string, List<DataRow>>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row in rows.Rows)
            {
                string activityType = Convert.ToString(row["ActivityType"]);
                if ((!string.Equals(activityType, "SAVE", StringComparison.OrdinalIgnoreCase) &&
                     !string.Equals(activityType, "UPDATE", StringComparison.OrdinalIgnoreCase)) ||
                    row["ActivityQty"] == DBNull.Value ||
                    !CanApplyActivitySnapshot(rows, row))
                {
                    continue;
                }

                string key = string.Join("|",
                    Convert.ToString(row["Action"]),
                    ToLong(row, "TransactionNo"),
                    ToLong(row, "ItemId"),
                    ToLong(row, "UnitId"));

                List<DataRow> group;
                if (!groups.TryGetValue(key, out group))
                {
                    group = new List<DataRow>();
                    groups[key] = group;
                }

                group.Add(row);
            }

            foreach (List<DataRow> group in groups.Values)
            {
                group.Sort((left, right) =>
                {
                    int dateCompare = ToDateTime(left, "CreatedOn").CompareTo(ToDateTime(right, "CreatedOn"));
                    return dateCompare != 0
                        ? dateCompare
                        : ToLong(left, "ActivityLogId").CompareTo(ToLong(right, "ActivityLogId"));
                });

                decimal? previousSignedSnapshot = null;
                foreach (DataRow row in group)
                {
                    decimal snapshotQty = Math.Abs(ToDecimal(row, "ActivityQty"));
                    decimal signedSnapshot = IsNaturallyStockOut(Convert.ToString(row["Action"]))
                        ? 0m - snapshotQty
                        : snapshotQty;
                    bool isUpdate = string.Equals(Convert.ToString(row["ActivityType"]), "UPDATE", StringComparison.OrdinalIgnoreCase);
                    decimal movement = isUpdate && previousSignedSnapshot.HasValue
                        ? signedSnapshot - previousSignedSnapshot.Value
                        : signedSnapshot;

                    if (rows.Columns.Contains("Qty"))
                    {
                        // Qty is the value entered on this transaction snapshot.
                        // StockIn/StockOut and QtyDifference represent only the
                        // movement caused by an update.
                        row["Qty"] = snapshotQty;
                    }
                    row["StockIn"] = movement > 0m ? movement : 0m;
                    row["StockOut"] = movement < 0m ? Math.Abs(movement) : 0m;
                    row["QtyDifference"] = movement;
                    previousSignedSnapshot = signedSnapshot;
                }
            }
        }

        private static bool CanApplyActivitySnapshot(DataTable rows, DataRow current)
        {
            string activityBarcode = Convert.ToString(current["ActivityBarcode"]);
            string rowBarcode = current.Table.Columns.Contains("Barcode")
                ? Convert.ToString(current["Barcode"])
                : string.Empty;

            if (!string.IsNullOrWhiteSpace(activityBarcode) &&
                !string.Equals(activityBarcode, "Multiple", StringComparison.OrdinalIgnoreCase))
            {
                return string.Equals(activityBarcode.Trim(), rowBarcode.Trim(), StringComparison.OrdinalIgnoreCase);
            }

            long itemId = 0;
            int distinctItems = 0;
            foreach (DataRow row in rows.Rows)
            {
                if (!string.Equals(Convert.ToString(row["Action"]), Convert.ToString(current["Action"]), StringComparison.OrdinalIgnoreCase) ||
                    ToLong(row, "TransactionNo") != ToLong(current, "TransactionNo"))
                {
                    continue;
                }

                long candidateItemId = ToLong(row, "ItemId");
                if (candidateItemId > 0 && candidateItemId != itemId)
                {
                    itemId = candidateItemId;
                    distinctItems++;
                    if (distinctItems > 1)
                    {
                        return false;
                    }
                }
            }

            return distinctItems == 1;
        }

        private static bool IsNaturallyStockOut(string action)
        {
            return string.Equals(action, "Sales", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(action, "Purchase Return", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(action, "Stock OUT", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(action, "Stock Out", StringComparison.OrdinalIgnoreCase);
        }

        private void NormalizePurchaseReturnMovements(DataTable rows)
        {
            if (rows == null || rows.Rows.Count == 0 || !rows.Columns.Contains("Action"))
            {
                return;
            }

            EnsureColumn(rows, "StockIn", typeof(decimal));
            EnsureColumn(rows, "StockOut", typeof(decimal));
            EnsureColumn(rows, "MovementQty", typeof(decimal));
            EnsureColumn(rows, "QtyDifference", typeof(decimal));

            foreach (DataRow row in rows.Rows)
            {
                if (!string.Equals(Convert.ToString(row["Action"]), "Purchase Return", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (rows.Columns.Contains("ActivityQty") &&
                    rows.Columns.Contains("ActivityType") &&
                    row["ActivityQty"] != DBNull.Value &&
                    !string.IsNullOrWhiteSpace(Convert.ToString(row["ActivityType"])))
                {
                    continue;
                }

                decimal returnQty = GetPurchaseReturnQty(row);
                if (returnQty <= 0m)
                {
                    continue;
                }

                if (rows.Columns.Contains("Qty"))
                {
                    row["Qty"] = returnQty;
                }
                row["StockIn"] = 0m;
                row["StockOut"] = returnQty;
                row["MovementQty"] = 0m - returnQty;
                row["QtyDifference"] = 0m - returnQty;
            }
        }

        private decimal GetPurchaseReturnQty(DataRow row)
        {
            decimal returnQty = FirstNonZeroDecimal(row, "Returned", "ReturnedQty", "ReturnQty", "Returnqty", "Returned qty");
            if (returnQty > 0m)
            {
                return returnQty;
            }

            return LookupPurchaseReturnQty(row);
        }

        private decimal LookupPurchaseReturnQty(DataRow row)
        {
            long purchaseReturnNo = ToLong(row, "TransactionNo");
            long itemId = ToLong(row, "ItemId");
            long slNo = ToLong(row, "SlNo");
            if (purchaseReturnNo <= 0)
            {
                return 0m;
            }

            ConnectionState originalState = DataConnection.State;
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                if (!TableExists("PReturnDetails"))
                {
                    return 0m;
                }

                string itemFilter = itemId > 0 ? " AND ISNULL(ItemID, 0) = @ItemId" : string.Empty;
                string slNoFilter = slNo > 0 ? " AND ISNULL(SlNo, 0) = @SlNo" : string.Empty;
                using (SqlCommand cmd = new SqlCommand(@"
SELECT ISNULL(SUM(ISNULL(Returned, 0)), 0)
FROM dbo.PReturnDetails
WHERE PReturnNo = @PReturnNo
  AND (@CompanyId = 0 OR ISNULL(CompanyId, 0) = @CompanyId)
  AND (@BranchId = 0 OR ISNULL(BranchID, 0) = @BranchId)
  AND (@FinYearId = 0 OR ISNULL(FinYearId, 0) = @FinYearId)" + itemFilter + slNoFilter + ";", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@PReturnNo", purchaseReturnNo);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    if (itemId > 0)
                    {
                        cmd.Parameters.AddWithValue("@ItemId", itemId);
                    }
                    if (slNo > 0)
                    {
                        cmd.Parameters.AddWithValue("@SlNo", slNo);
                    }

                    object value = cmd.ExecuteScalar();
                    decimal parsed;
                    return value == null || value == DBNull.Value || !decimal.TryParse(Convert.ToString(value), out parsed)
                        ? 0m
                        : Math.Abs(parsed);
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("Unable to normalize purchase return qty: " + ex.Message);
                return 0m;
            }
            finally
            {
                if (originalState != ConnectionState.Open && DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        private static decimal FirstNonZeroDecimal(DataRow row, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                decimal value = ToDecimal(row, columnName);
                if (value != 0m)
                {
                    return Math.Abs(value);
                }
            }

            return 0m;
        }

        private void ApplyActivityLogMetadata(DataTable rows, DateTime fromDate, DateTime toDate)
        {
            if (rows == null || rows.Rows.Count == 0 || !rows.Columns.Contains("Action") || !rows.Columns.Contains("TransactionNo"))
            {
                return;
            }

            AddActivityMetadataColumns(rows);

            DataTable activityRows = GetActivityLogMetadata(fromDate, toDate);
            if (activityRows.Rows.Count == 0)
            {
                return;
            }

            foreach (DataRow row in rows.Rows)
            {
                string action = GetMetadataAction(Convert.ToString(row["Action"]));
                long transactionNo = ToLong(row, "TransactionNo");
                if (string.IsNullOrWhiteSpace(action) || transactionNo <= 0)
                {
                    continue;
                }

                DataRow metadata = FindActivityMetadata(activityRows, action, transactionNo, ToLong(row, "ActivityLogId"));
                if (metadata == null)
                {
                    continue;
                }

                CopyIfColumnExists(row, metadata, "CreatedOn");
                CopyIfColumnExists(row, metadata, "UserName");
                CopyIfColumnExists(row, metadata, "UserId");
                CopyIfColumnExists(row, metadata, "CounterName");
                CopyIfColumnExists(row, metadata, "CounterId");
                CopyIfColumnExists(row, metadata, "CounterSessionId");
                CopyIfColumnExists(row, metadata, "ActivityLogId");
                CopyIfColumnExists(row, metadata, "ActivityType");
                CopyIfColumnExists(row, metadata, "ActivityQty");
                CopyIfColumnExists(row, metadata, "ActivityBarcode");
                CopyIfColumnExists(row, metadata, "PartyName");
            }
        }

        private DataTable GetActivityLogMetadata(DateTime fromDate, DateTime toDate)
        {
            DataTable result = CreateActivityMetadataTable();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                string sql = BuildActivityMetadataSql();
                if (string.IsNullOrWhiteSpace(sql))
                {
                    return result;
                }

                using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    adapter.Fill(result);
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("Unable to load activity log metadata: " + ex.Message);
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

        private string BuildActivityMetadataSql()
        {
            string unionSql = string.Empty;
            AppendActivityMetadataSelect(ref unionSql, "PurchaseActivityLog", "Purchase");
            AppendActivityMetadataSelect(ref unionSql, "SalesActivityLog", "Sales");
            AppendActivityMetadataSelect(ref unionSql, "SalesReturnActivityLog", "Sales Return");
            AppendActivityMetadataSelect(ref unionSql, "PurchaseReturnActivityLog", "Purchase Return");
            AppendActivityMetadataSelect(ref unionSql, "StockAdjustmentActivityLog", "Stock Adjustment");

            if (string.IsNullOrWhiteSpace(unionSql))
            {
                return string.Empty;
            }

            return @"
WITH ActivityRows AS
(
" + unionSql + @"
)
SELECT
    Action,
    TransactionNo,
    ActivityLogId,
    ActivityType,
    ActivityQty,
    ActivityBarcode,
    CreatedOn,
    UserName,
    UserId,
    CounterName,
    CounterId,
    CounterSessionId,
    PartyName
FROM ActivityRows;";
        }

        private void AppendActivityMetadataSelect(ref string unionSql, string tableName, string action)
        {
            if (!TableExists(tableName))
            {
                return;
            }

            string activityTypeExpression = ColumnExists(tableName, "ActivityType")
                ? "CAST(ActivityType AS nvarchar(50))"
                : "CAST(NULL AS nvarchar(50))";
            string qtyExpression = ColumnExists(tableName, "Qty")
                ? "CAST(Qty AS decimal(18,4))"
                : "CAST(NULL AS decimal(18,4))";
            string barcodeExpression = ColumnExists(tableName, "Barcode")
                ? "CAST(Barcode AS nvarchar(100))"
                : "CAST(NULL AS nvarchar(100))";

            string partyNameExpression = ColumnExists(tableName, "PartyName")
                ? "CAST(PartyName AS nvarchar(250))"
                : "CAST(NULL AS nvarchar(250))";

            if (!string.IsNullOrWhiteSpace(unionSql))
            {
                unionSql += @"
UNION ALL
";
            }

            unionSql += @"
    SELECT
        CAST(N'" + action.Replace("'", "''") + @"' AS nvarchar(50)) AS Action,
        CAST(TransactionNo AS bigint) AS TransactionNo,
        CAST(ActivityLogId AS int) AS ActivityLogId,
        " + activityTypeExpression + @" AS ActivityType,
        " + qtyExpression + @" AS ActivityQty,
        " + barcodeExpression + @" AS ActivityBarcode,
        CreatedOn,
        CAST(UserName AS nvarchar(150)) AS UserName,
        CAST(UserId AS int) AS UserId,
        CAST(CounterName AS nvarchar(150)) AS CounterName,
        CAST(CounterId AS int) AS CounterId,
        CAST(CounterSessionId AS bigint) AS CounterSessionId,
        " + partyNameExpression + @" AS PartyName
    FROM dbo." + tableName + @"
    WHERE CreatedOn >= @FromDate
      AND CreatedOn < DATEADD(DAY, 1, @ToDate)
      AND (@CompanyId = 0 OR ISNULL(CompanyId, 0) = @CompanyId)
      AND (@BranchId = 0 OR ISNULL(BranchId, 0) = @BranchId)
      AND (@FinYearId = 0 OR ISNULL(FinYearId, 0) = @FinYearId)";
        }

        private DateTime GetLatestTransactionActivityStamp()
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                string sql = BuildLatestTransactionActivitySql();
                if (string.IsNullOrWhiteSpace(sql))
                {
                    return DateTime.MinValue;
                }

                using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    object value = cmd.ExecuteScalar();
                    return value == null || value == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(value);
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("Unable to load latest transaction activity stamp: " + ex.Message);
                return DateTime.MinValue;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        private string BuildLatestTransactionActivitySql()
        {
            string unionSql = string.Empty;
            AppendLatestActivitySelect(ref unionSql, "PurchaseActivityLog");
            AppendLatestActivitySelect(ref unionSql, "SalesActivityLog");
            AppendLatestActivitySelect(ref unionSql, "SalesReturnActivityLog");
            AppendLatestActivitySelect(ref unionSql, "PurchaseReturnActivityLog");
            AppendLatestActivitySelect(ref unionSql, "StockAdjustmentActivityLog");

            if (string.IsNullOrWhiteSpace(unionSql))
            {
                return string.Empty;
            }

            return "SELECT ISNULL(MAX(CreatedOn), CONVERT(datetime, '19000101', 112)) FROM (" + unionSql + ") ActivityStamps;";
        }

        private void AppendLatestActivitySelect(ref string unionSql, string tableName)
        {
            if (!TableExists(tableName))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(unionSql))
            {
                unionSql += " UNION ALL ";
            }

            unionSql += "SELECT CreatedOn FROM dbo." + tableName + " WHERE (@CompanyId = 0 OR ISNULL(CompanyId, 0) = @CompanyId) AND (@BranchId = 0 OR ISNULL(BranchId, 0) = @BranchId) AND (@FinYearId = 0 OR ISNULL(FinYearId, 0) = @FinYearId)";
        }

        private DateTime GetRecoveredPurchaseLatestStamp()
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                if (!TableExists("PMaster"))
                {
                    return DateTime.MinValue;
                }

                string activitySelect = TableExists("PurchaseActivityLog")
                    ? @"SELECT @Latest = CASE WHEN @Latest IS NULL OR MAX(pal.CreatedOn) > @Latest THEN MAX(pal.CreatedOn) ELSE @Latest END
FROM dbo.PurchaseActivityLog pal
WHERE (@CompanyId = 0 OR ISNULL(pal.CompanyId, 0) = @CompanyId)
  AND (@BranchId = 0 OR ISNULL(pal.BranchId, 0) = @BranchId)
  AND (@FinYearId = 0 OR ISNULL(pal.FinYearId, 0) = @FinYearId);"
                    : string.Empty;

                using (SqlCommand cmd = new SqlCommand(@"
DECLARE @Latest datetime = NULL;

SELECT @Latest = MAX(CAST(CAST(pm.PurchaseDate AS date) AS datetime))
FROM dbo.PMaster pm
WHERE ISNULL(pm.CancelFlag, 0) = 0
  AND (@CompanyId = 0 OR ISNULL(pm.CompanyId, 0) = @CompanyId)
  AND (@BranchId = 0 OR ISNULL(pm.BranchId, 0) = @BranchId)
  AND (@FinYearId = 0 OR ISNULL(pm.FinYearId, 0) = @FinYearId);

" + activitySelect + @"

SELECT ISNULL(@Latest, CONVERT(datetime, '19000101', 112));", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    object value = cmd.ExecuteScalar();
                    return value == null || value == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(value);
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

        private bool TableExists(string tableName)
        {
            using (SqlCommand cmd = new SqlCommand("SELECT CASE WHEN OBJECT_ID(@TableName, 'U') IS NULL THEN 0 ELSE 1 END;", (SqlConnection)DataConnection))
            {
                cmd.Parameters.AddWithValue("@TableName", "dbo." + tableName);
                return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
            }
        }

        private static void AddMissingColumns(DataTable target, DataTable source)
        {
            foreach (DataColumn column in source.Columns)
            {
                if (!target.Columns.Contains(column.ColumnName))
                {
                    target.Columns.Add(column.ColumnName, column.DataType);
                }
            }
        }

        private static DataRow FindExistingPurchaseRow(DataTable target, DataRow purchaseRow)
        {
            foreach (DataRow row in target.Rows)
            {
                if (string.Equals(Convert.ToString(row["Action"]), "Purchase", StringComparison.OrdinalIgnoreCase) &&
                    ToLong(row, "TransactionNo") == ToLong(purchaseRow, "TransactionNo") &&
                    ToLong(row, "SlNo") == ToLong(purchaseRow, "SlNo") &&
                    ToLong(row, "ItemId") == ToLong(purchaseRow, "ItemId"))
                {
                    return row;
                }
            }

            return null;
        }

        private static void FillPurchaseRowGaps(DataRow target, DataRow source)
        {
            foreach (DataColumn column in source.Table.Columns)
            {
                if (!target.Table.Columns.Contains(column.ColumnName))
                {
                    continue;
                }

                bool sourceHasActivityLog = ToLong(source, "ActivityLogId") > 0;
                bool shouldPreferActivityLogValue =
                    sourceHasActivityLog &&
                    (string.Equals(column.ColumnName, "CreatedOn", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(column.ColumnName, "UserName", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(column.ColumnName, "UserId", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(column.ColumnName, "CounterName", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(column.ColumnName, "CounterId", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(column.ColumnName, "CounterSessionId", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(column.ColumnName, "ActivityLogId", StringComparison.OrdinalIgnoreCase));

                bool targetBlank = target[column.ColumnName] == DBNull.Value ||
                                   string.IsNullOrWhiteSpace(Convert.ToString(target[column.ColumnName])) ||
                                   IsZeroNumber(target[column.ColumnName]);
                if (shouldPreferActivityLogValue || targetBlank)
                {
                    target[column.ColumnName] = source[column.ColumnName];
                }
            }
        }

        private static void AddActivityMetadataColumns(DataTable table)
        {
            EnsureColumn(table, "CreatedOn", typeof(DateTime));
            EnsureColumn(table, "UserName", typeof(string));
            EnsureColumn(table, "UserId", typeof(int));
            EnsureColumn(table, "CounterName", typeof(string));
            EnsureColumn(table, "CounterId", typeof(int));
            EnsureColumn(table, "CounterSessionId", typeof(long));
            EnsureColumn(table, "ActivityLogId", typeof(int));
            EnsureColumn(table, "ActivityType", typeof(string));
            EnsureColumn(table, "ActivityQty", typeof(decimal));
            EnsureColumn(table, "ActivityBarcode", typeof(string));
            EnsureColumn(table, "PartyName", typeof(string));
        }

        private static void EnsureColumn(DataTable table, string columnName, Type dataType)
        {
            if (table != null && !table.Columns.Contains(columnName))
            {
                table.Columns.Add(columnName, dataType);
            }
        }

        private static DataTable CreateActivityMetadataTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Action", typeof(string));
            table.Columns.Add("TransactionNo", typeof(long));
            table.Columns.Add("ActivityLogId", typeof(int));
            table.Columns.Add("ActivityType", typeof(string));
            table.Columns.Add("ActivityQty", typeof(decimal));
            table.Columns.Add("ActivityBarcode", typeof(string));
            table.Columns.Add("CreatedOn", typeof(DateTime));
            table.Columns.Add("UserName", typeof(string));
            table.Columns.Add("UserId", typeof(int));
            table.Columns.Add("CounterName", typeof(string));
            table.Columns.Add("CounterId", typeof(int));
            table.Columns.Add("CounterSessionId", typeof(long));
            table.Columns.Add("PartyName", typeof(string));
            return table;
        }

        private void ResolveMissingStockPartyNames(DataTable table)
        {
            if (table == null || table.Rows.Count == 0) return;
            EnsureColumn(table, "PartyName", typeof(string));

            List<DataRow> missingRows = new List<DataRow>();
            foreach (DataRow row in table.Rows)
            {
                string party = Convert.ToString(row["PartyName"]);
                if (string.IsNullOrWhiteSpace(party))
                {
                    missingRows.Add(row);
                }
            }

            if (missingRows.Count == 0) return;

            ConnectionState originalState = DataConnection.State;
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                foreach (DataRow row in missingRows)
                {
                    string action = Convert.ToString(row["Action"]) ?? string.Empty;
                    long transNo = ToLong(row, "TransactionNo");
                    if (transNo <= 0) continue;

                    string party = string.Empty;

                    if (action.IndexOf("Sales Return", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (TableExists("SReturnMaster"))
                        {
                            using (SqlCommand cmd = new SqlCommand(@"
SELECT TOP 1 ISNULL(NULLIF(CustomerName, ''), NULLIF(LedgerName, ''))
FROM dbo.SReturnMaster
WHERE SReturnNo = @TransNo
  AND (@CompanyId = 0 OR ISNULL(CompanyId, 0) = @CompanyId)
  AND (@BranchId = 0 OR ISNULL(BranchID, 0) = @BranchId)
  AND (@FinYearId = 0 OR ISNULL(FinYearId, 0) = @FinYearId);", (SqlConnection)DataConnection))
                            {
                                cmd.Parameters.AddWithValue("@TransNo", transNo);
                                cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                                cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                                cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                                object val = cmd.ExecuteScalar();
                                if (val != null && val != DBNull.Value) party = Convert.ToString(val);
                            }
                        }
                    }
                    else if (action.IndexOf("Sales", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (TableExists("SMaster"))
                        {
                            using (SqlCommand cmd = new SqlCommand(@"
SELECT TOP 1 ISNULL(CustomerName, '')
FROM dbo.SMaster
WHERE BillNo = @TransNo
  AND (@CompanyId = 0 OR ISNULL(CompanyId, 0) = @CompanyId)
  AND (@BranchId = 0 OR ISNULL(BranchId, 0) = @BranchId)
  AND (@FinYearId = 0 OR ISNULL(FinYearId, 0) = @FinYearId);", (SqlConnection)DataConnection))
                            {
                                cmd.Parameters.AddWithValue("@TransNo", transNo);
                                cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                                cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                                cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                                object val = cmd.ExecuteScalar();
                                if (val != null && val != DBNull.Value) party = Convert.ToString(val);
                            }
                        }
                    }
                    else if (action.IndexOf("Purchase Return", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (TableExists("PReturnMaster"))
                        {
                            using (SqlCommand cmd = new SqlCommand(@"
SELECT TOP 1 ISNULL(NULLIF(VendorName, ''), NULLIF(SupplierName, ''))
FROM dbo.PReturnMaster
WHERE PReturnNo = @TransNo
  AND (@CompanyId = 0 OR ISNULL(CompanyId, 0) = @CompanyId)
  AND (@BranchId = 0 OR ISNULL(BranchID, 0) = @BranchId)
  AND (@FinYearId = 0 OR ISNULL(FinYearId, 0) = @FinYearId);", (SqlConnection)DataConnection))
                            {
                                cmd.Parameters.AddWithValue("@TransNo", transNo);
                                cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                                cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                                cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                                object val = cmd.ExecuteScalar();
                                if (val != null && val != DBNull.Value) party = Convert.ToString(val);
                            }
                        }
                    }
                    else if (action.IndexOf("Purchase", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (TableExists("PMaster"))
                        {
                            using (SqlCommand cmd = new SqlCommand(@"
SELECT TOP 1 ISNULL(VendorName, '')
FROM dbo.PMaster
WHERE PurchaseNo = @TransNo
  AND (@CompanyId = 0 OR ISNULL(CompanyId, 0) = @CompanyId)
  AND (@BranchId = 0 OR ISNULL(BranchId, 0) = @BranchId)
  AND (@FinYearId = 0 OR ISNULL(FinYearId, 0) = @FinYearId);", (SqlConnection)DataConnection))
                            {
                                cmd.Parameters.AddWithValue("@TransNo", transNo);
                                cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                                cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                                cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                                object val = cmd.ExecuteScalar();
                                if (val != null && val != DBNull.Value) party = Convert.ToString(val);
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(party))
                    {
                        row["PartyName"] = party;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error resolving missing stock party names: " + ex.Message);
            }
            finally
            {
                if (originalState != ConnectionState.Open && DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        private static string GetMetadataAction(string action)
        {
            if (string.Equals(action, "Stock IN", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "Stock In", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "Stock OUT", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "Stock Out", StringComparison.OrdinalIgnoreCase))
            {
                return "Stock Adjustment";
            }

            return action ?? string.Empty;
        }

        private static DataRow FindActivityMetadata(DataTable metadataRows, string action, long transactionNo, long activityLogId)
        {
            DataRow latestMatch = null;
            long latestActivityLogId = 0;

            foreach (DataRow metadata in metadataRows.Rows)
            {
                if (string.Equals(Convert.ToString(metadata["Action"]), action, StringComparison.OrdinalIgnoreCase) &&
                    ToLong(metadata, "TransactionNo") == transactionNo)
                {
                    long metadataActivityLogId = ToLong(metadata, "ActivityLogId");
                    if (activityLogId > 0 && metadataActivityLogId == activityLogId)
                    {
                        return metadata;
                    }

                    if (metadataActivityLogId > latestActivityLogId)
                    {
                        latestMatch = metadata;
                        latestActivityLogId = metadataActivityLogId;
                    }
                }
            }

            return latestMatch;
        }

        private static void CopyIfColumnExists(DataRow target, DataRow source, string columnName)
        {
            if (target.Table.Columns.Contains(columnName) &&
                source.Table.Columns.Contains(columnName) &&
                source[columnName] != DBNull.Value)
            {
                target[columnName] = source[columnName];
            }
        }

        private static DataTable SortDistinctValues(DataTable table)
        {
            if (table == null || !table.Columns.Contains("Value"))
            {
                return table;
            }

            DataView view = table.DefaultView;
            view.Sort = "Value ASC";
            return view.ToTable(true, "Value");
        }

        private static bool ContainsValue(DataTable table, string value)
        {
            foreach (DataRow row in table.Rows)
            {
                if (string.Equals(Convert.ToString(row["Value"]), value, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static long ToLong(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
            {
                return 0;
            }

            long value;
            return long.TryParse(Convert.ToString(row[columnName]), out value) ? value : 0;
        }

        private static bool IsZeroNumber(object value)
        {
            decimal number;
            return value != null && decimal.TryParse(Convert.ToString(value), out number) && number == 0m;
        }

        private static DataTable SortActivityTable(DataTable table)
        {
            if (table == null || table.Rows.Count == 0 || !table.Columns.Contains("CreatedOn"))
            {
                return table;
            }

            string sort = "CreatedOn DESC";
            if (table.Columns.Contains("ActivityLogId"))
            {
                sort += ", ActivityLogId DESC";
            }
            if (table.Columns.Contains("TransactionNo"))
            {
                sort += ", TransactionNo DESC";
            }
            if (table.Columns.Contains("SlNo"))
            {
                sort += ", SlNo DESC";
            }

            DataView view = table.DefaultView;
            view.Sort = sort;
            DataTable sorted = view.ToTable();
            AssignDisplayLogNumbers(sorted);
            return sorted;
        }

        private static void AssignDisplayLogNumbers(DataTable table)
        {
            if (table == null)
            {
                return;
            }

            if (!table.Columns.Contains("DisplayLogNo"))
            {
                table.Columns.Add("DisplayLogNo", typeof(int));
            }

            for (int index = 0; index < table.Rows.Count; index++)
            {
                // The grid is sorted newest first, but log numbers must increase
                // chronologically so the latest activity has the highest number.
                table.Rows[index]["DisplayLogNo"] = table.Rows.Count - index;
            }
        }

        private void ApplyStableStockTimeline(DataTable table)
        {
            if (table == null ||
                table.Rows.Count == 0 ||
                !table.Columns.Contains("ItemId"))
            {
                return;
            }

            EnsureColumn(table, "Stock", typeof(decimal));
            EnsureColumn(table, "Available", typeof(decimal));

            Dictionary<string, decimal> runningStockByItem = new Dictionary<string, decimal>();
            List<DataRow> rows = new List<DataRow>();
            foreach (DataRow row in table.Rows)
            {
                rows.Add(row);
            }

            rows.Sort(CompareRowsByLedgerOrder);

            foreach (DataRow row in rows)
            {
                long itemId = ToLong(row, "ItemId");
                if (itemId <= 0)
                {
                    continue;
                }

                string key = BuildStockTimelineKey(row);
                decimal runningStock;
                runningStockByItem.TryGetValue(key, out runningStock);

                runningStock += GetSignedMovement(row);
                runningStockByItem[key] = runningStock;

                row["Stock"] = runningStock;
                row["Available"] = runningStock - ToDecimal(row, "Hold");
            }
        }

        private static int CompareRowsByLedgerOrder(DataRow left, DataRow right)
        {
            int dateCompare = ToDateTime(left, "CreatedOn").CompareTo(ToDateTime(right, "CreatedOn"));
            if (dateCompare != 0)
            {
                return dateCompare;
            }

            int logCompare = ToLong(left, "ActivityLogId").CompareTo(ToLong(right, "ActivityLogId"));
            if (logCompare != 0)
            {
                return logCompare;
            }

            int transactionCompare = ToLong(left, "TransactionNo").CompareTo(ToLong(right, "TransactionNo"));
            if (transactionCompare != 0)
            {
                return transactionCompare;
            }

            return ToLong(left, "SlNo").CompareTo(ToLong(right, "SlNo"));
        }

        private static string BuildStockTimelineKey(DataRow row)
        {
            return ToLong(row, "ItemId") + "|" + ToLong(row, "BranchId");
        }
        private DataTable GetCurrentStockSnapshots()
        {
            DataTable result = new DataTable();
            result.Columns.Add("ItemId", typeof(long));
            result.Columns.Add("UnitId", typeof(long));
            result.Columns.Add("BranchId", typeof(long));
            result.Columns.Add("Stock", typeof(decimal));
            result.Columns.Add("Hold", typeof(decimal));

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                if (!TableExists("PriceSettings"))
                {
                    return result;
                }

                string companyFilter = ColumnExists("PriceSettings", "CompanyId")
                    ? "AND (@CompanyId = 0 OR ISNULL(ps.CompanyId, 0) = @CompanyId)"
                    : string.Empty;
                string branchColumn = ColumnExists("PriceSettings", "BranchId") ? "ps.BranchId" : "0";
                string branchFilter = ColumnExists("PriceSettings", "BranchId")
                    ? "AND (@BranchId = 0 OR ISNULL(ps.BranchId, 0) = @BranchId)"
                    : string.Empty;
                string unitColumn = ColumnExists("PriceSettings", "UnitId") ? "ps.UnitId" : "0";

                string identityOrder = ColumnExists("PriceSettings", "PriceSettingsId")
                    ? "ps.PriceSettingsId DESC"
                    : (ColumnExists("PriceSettings", "Id") ? "ps.Id DESC" : "ps.ItemId");

                using (SqlCommand cmd = new SqlCommand(@"
SELECT
    CAST(ps.ItemId AS bigint) AS ItemId,
    CAST(" + unitColumn + @" AS bigint) AS UnitId,
    CAST(" + branchColumn + @" AS bigint) AS BranchId,
    CAST(ISNULL(ps.Stock, 0) AS decimal(18,4)) AS Stock,
    CAST(ISNULL(hold.HoldQty, 0) AS decimal(18,4)) AS Hold
FROM dbo.PriceSettings ps
OUTER APPLY
(
    SELECT SUM(ISNULL(sd.Qty, 0)) AS HoldQty
    FROM dbo.SMaster sm
    INNER JOIN dbo.SDetails sd ON sd.BillNo = sm.BillNo
    WHERE ISNULL(sm.Status, N'') = N'Hold'
      AND ISNULL(sm.CancelFlag, 0) = 0
      AND sd.ItemId = ps.ItemId
      AND (@CompanyId = 0 OR ISNULL(sm.CompanyId, 0) = @CompanyId)
      AND (@BranchId = 0 OR ISNULL(sm.BranchId, 0) = @BranchId)
      AND (@FinYearId = 0 OR ISNULL(sm.FinYearId, 0) = @FinYearId)
) hold
WHERE ps.ItemId IS NOT NULL
  " + companyFilter + @"
  " + branchFilter + @"
ORDER BY ps.ItemId, " + branchColumn + @", " + unitColumn + @", " + identityOrder + @";", (SqlConnection)DataConnection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    adapter.Fill(result);
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("Unable to load current stock snapshots: " + ex.Message);
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

        private bool ColumnExists(string tableName, string columnName)
        {
            using (SqlCommand cmd = new SqlCommand("SELECT CASE WHEN COL_LENGTH(@TableName, @ColumnName) IS NULL THEN 0 ELSE 1 END;", (SqlConnection)DataConnection))
            {
                cmd.Parameters.AddWithValue("@TableName", "dbo." + tableName);
                cmd.Parameters.AddWithValue("@ColumnName", columnName);
                return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
            }
        }

        private static bool TryGetCurrentStock(DataTable currentStock, long itemId, long unitId, long branchId, out decimal stock, out decimal hold)
        {
            stock = 0m;
            hold = 0m;
            DataRow fallback = null;
            DataRow itemUnitFallback = null;

            foreach (DataRow row in currentStock.Rows)
            {
                if (ToLong(row, "ItemId") != itemId)
                {
                    continue;
                }

                if (fallback == null)
                {
                    fallback = row;
                }

                bool exactUnit = unitId <= 0 || ToLong(row, "UnitId") == unitId;
                bool exactBranch = branchId <= 0 || ToLong(row, "BranchId") == branchId;
                if (exactUnit && itemUnitFallback == null)
                {
                    itemUnitFallback = row;
                }

                bool unitMatches = exactUnit || ToLong(row, "UnitId") == 0;
                bool branchMatches = exactBranch || ToLong(row, "BranchId") == 0;
                if (unitMatches && branchMatches)
                {
                    stock = ToDecimal(row, "Stock");
                    hold = ToDecimal(row, "Hold");
                    return true;
                }
            }

            if (itemUnitFallback != null)
            {
                stock = ToDecimal(itemUnitFallback, "Stock");
                hold = ToDecimal(itemUnitFallback, "Hold");
                return true;
            }

            if (fallback != null)
            {
                stock = ToDecimal(fallback, "Stock");
                hold = ToDecimal(fallback, "Hold");
                return true;
            }

            return false;
        }

        private static decimal GetNewerMovementTotal(DataTable table, DataRow currentRow, long itemId, long unitId, long branchId)
        {
            decimal total = 0m;
            DateTime currentCreatedOn = ToDateTime(currentRow, "CreatedOn");
            long currentActivityLogId = ToLong(currentRow, "ActivityLogId");
            long currentTransactionNo = ToLong(currentRow, "TransactionNo");
            long currentSlNo = ToLong(currentRow, "SlNo");

            foreach (DataRow row in table.Rows)
            {
                if (ReferenceEquals(row, currentRow) ||
                    ToLong(row, "ItemId") != itemId ||
                    (unitId > 0 && ToLong(row, "UnitId") != unitId) ||
                    (branchId > 0 && ToLong(row, "BranchId") != branchId))
                {
                    continue;
                }

                if (IsRowNewer(row, currentCreatedOn, currentActivityLogId, currentTransactionNo, currentSlNo))
                {
                    total += GetSignedMovement(row);
                }
            }

            return total;
        }

        private static decimal GetSignedMovement(DataRow row)
        {
            decimal stockIn = ToDecimal(row, "StockIn");
            decimal stockOut = ToDecimal(row, "StockOut");
            if (stockIn != 0m || stockOut != 0m)
            {
                return stockIn - stockOut;
            }

            decimal qtyDifference = ToDecimal(row, "QtyDifference");
            if (qtyDifference != 0m)
            {
                return qtyDifference;
            }

            decimal movementQty = ToDecimal(row, "MovementQty");
            if (movementQty != 0m)
            {
                return movementQty;
            }

            decimal qty = ToDecimal(row, "Qty");
            string action = row.Table.Columns.Contains("Action") ? Convert.ToString(row["Action"]) : string.Empty;
            if (string.Equals(action, "Sales", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "Purchase Return", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "Stock OUT", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(action, "Stock Out", StringComparison.OrdinalIgnoreCase))
            {
                return 0m - qty;
            }

            return qty;
        }

        private static bool IsRowNewer(DataRow row, DateTime createdOn, long activityLogId, long transactionNo, long slNo)
        {
            DateTime rowCreatedOn = ToDateTime(row, "CreatedOn");
            if (rowCreatedOn != createdOn)
            {
                return rowCreatedOn > createdOn;
            }

            long rowActivityLogId = ToLong(row, "ActivityLogId");
            if (rowActivityLogId != activityLogId)
            {
                return rowActivityLogId > activityLogId;
            }

            long rowTransactionNo = ToLong(row, "TransactionNo");
            if (rowTransactionNo != transactionNo)
            {
                return rowTransactionNo > transactionNo;
            }

            return ToLong(row, "SlNo") > slNo;
        }

        private static decimal ToDecimal(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
            {
                return 0m;
            }

            decimal value;
            return decimal.TryParse(Convert.ToString(row[columnName]), out value) ? value : 0m;
        }

        private static DateTime ToDateTime(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
            {
                return DateTime.MinValue;
            }

            DateTime value;
            return DateTime.TryParse(Convert.ToString(row[columnName]), out value) ? value : DateTime.MinValue;
        }
    }
}
