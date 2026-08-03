using ModelClass;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Repository.SettingsRepo
{
    public class ItemHistoryLogRepository : BaseRepostitory
    {
        public DataTable GetItemHistoryLog(DateTime fromDate, DateTime toDate, string userName, string actionFilter, string itemSearch)
        {
            DataTable combined = CreateCombinedTable();
            DataTable itemRows = ExecuteItemHistoryProcedure("GET", fromDate, toDate, userName, string.Empty, itemSearch);
            foreach (DataRow row in itemRows.Rows)
            {
                combined.ImportRow(row);
            }

            using (var stockRepo = new ItemStockActivityLogRepository())
            {
                DataTable stockRows = stockRepo.GetItemStockActivityLog(fromDate, toDate, userName ?? string.Empty, GetStockActionFilter(actionFilter), itemSearch ?? string.Empty);
                foreach (DataRow row in stockRows.Rows)
                {
                    AddStockRow(combined, row);
                }
            }

            ApplyActionFilter(combined, actionFilter);
            DataView view = combined.DefaultView;
            view.Sort = "CreatedOn DESC, SortNo DESC";
            return view.ToTable();
        }

        public DataTable GetItemDedicatedHistory(string searchText)
        {
            return GetItemHistoryLog(new DateTime(2000, 1, 1), DateTime.Today.AddYears(5), string.Empty, string.Empty, searchText);
        }

        public DataTable GetItemHistoryUsers()
        {
            var users = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            AddUserRows(users, ExecuteItemHistoryProcedure("GETUSERS", DateTime.MinValue, DateTime.MinValue, string.Empty, string.Empty, string.Empty));
            using (var stockRepo = new ItemStockActivityLogRepository())
            {
                AddUserRows(users, stockRepo.GetItemStockActivityUsers());
            }
            DataTable table = new DataTable();
            table.Columns.Add("Value", typeof(string));
            foreach (string user in users)
            {
                table.Rows.Add(user);
            }
            return table;
        }

        public DataTable GetItemHistoryActions()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Value", typeof(string));
            foreach (string action in GetActionFilterValues())
            {
                table.Rows.Add(action);
            }
            return table;
        }

        public int CountItemHistory(DateTime fromDate, DateTime toDate, string userName, string itemSearch)
        {
            return GetItemHistoryLog(fromDate, toDate, userName, string.Empty, itemSearch).Rows.Count;
        }

        public DataTable GetItemHistorySummary(string itemSearch)
        {
            DataTable result = new DataTable();
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                using (SqlCommand cmd = new SqlCommand(@"
IF OBJECT_ID(N'dbo.ItemMaster', N'U') IS NULL
BEGIN
    SELECT
        CAST(N'' AS NVARCHAR(250)) AS ItemName,
        CAST(N'' AS NVARCHAR(100)) AS Barcode,
        CAST(0 AS DECIMAL(18,4)) AS CurrentStock,
        CAST(NULL AS DATETIME) AS CreatedOn
    WHERE 1 = 0;
    RETURN;
END

;WITH MatchingItemIds AS
(
    SELECT ItemId FROM dbo.ItemMaster
    WHERE @ItemSearch <> N'' AND (ISNULL(Description, N'') LIKE N'%' + @ItemSearch + N'%' OR ISNULL(ItemNo, N'') = @ItemSearch OR ISNULL(Barcode, N'') = @ItemSearch)
    UNION
    SELECT ItemId FROM dbo.PriceSettings
    WHERE @ItemSearch <> N'' AND (ISNULL(BarCode, N'') = @ItemSearch OR ISNULL(AliasBarcode, N'') = @ItemSearch)
    UNION
    SELECT ItemId FROM dbo.ItemAlternativeBarcode
    WHERE @ItemSearch <> N'' AND ISNULL(Barcode, N'') = @ItemSearch
    UNION
    SELECT ItemId FROM dbo.ItemActivityLog
    WHERE @ItemSearch <> N'' AND (ISNULL(ItemName, N'') LIKE N'%' + @ItemSearch + N'%' OR ISNULL(ItemNo, N'') = @ItemSearch OR ISNULL(Barcode, N'') = @ItemSearch)
),
PickedItem AS
(
    SELECT TOP 1 im.ItemId, im.Description, im.Barcode
    FROM dbo.ItemMaster im
    WHERE im.ItemId IN (SELECT ItemId FROM MatchingItemIds)
    ORDER BY im.ItemId
)
SELECT
    CAST(ISNULL(pi.Description, N'') AS NVARCHAR(250)) AS ItemName,
    CAST(ISNULL(pi.Barcode, N'') AS NVARCHAR(100)) AS Barcode,
    CAST(ISNULL(stock.CurrentStock, 0) AS DECIMAL(18,4)) AS CurrentStock,
    created.CreatedOn
FROM PickedItem pi
OUTER APPLY
(
    SELECT SUM(ISNULL(ps.Stock, 0)) AS CurrentStock
    FROM dbo.PriceSettings ps
    WHERE ps.ItemId = pi.ItemId
      AND (@CompanyId = 0 OR ISNULL(ps.CompanyId, 0) = @CompanyId)
      AND (@BranchId = 0 OR ISNULL(ps.BranchId, 0) = @BranchId)
) stock
OUTER APPLY
(
    SELECT MIN(ial.CreatedOn) AS CreatedOn
    FROM dbo.ItemActivityLog ial
    WHERE ial.ItemId = pi.ItemId
      AND UPPER(ISNULL(ial.ActivityType, N'')) IN (N'SAVE', N'ADD', N'CREATE', N'CREATED')
) created;", (SqlConnection)DataConnection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.Parameters.AddWithValue("@ItemSearch", itemSearch ?? string.Empty);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    adapter.Fill(result);
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("Item history summary failed: " + ex.Message);
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

        public DateTime GetLatestActivityStamp()
        {
            DateTime latest = DateTime.MinValue;
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                using (SqlCommand cmd = new SqlCommand(@"
DECLARE @Latest DATETIME = NULL;

IF OBJECT_ID(N'dbo.ItemActivityLog', N'U') IS NOT NULL
    SELECT @Latest = MAX(CreatedOn) FROM dbo.ItemActivityLog;

SELECT ISNULL(@Latest, CONVERT(DATETIME, '19000101', 112));", (SqlConnection)DataConnection))
                {
                    object value = cmd.ExecuteScalar();
                    latest = value == null || value == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(value);
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("Item history latest stamp failed: " + ex.Message);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            using (var stockRepo = new ItemStockActivityLogRepository())
            {
                DateTime stockLatest = stockRepo.GetLatestActivityStamp();
                return stockLatest > latest ? stockLatest : latest;
            }
        }

        private DataTable ExecuteItemHistoryProcedure(string operation, DateTime fromDate, DateTime toDate, string userName, string actionFilter, string itemSearch)
        {
            DataTable result = CreateCombinedTable();
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                EnsureItemHistoryLogStoredProcedure();
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemHistoryLog, (SqlConnection)DataConnection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", operation ?? "GET");
                    cmd.Parameters.AddWithValue("@FromDate", fromDate == DateTime.MinValue ? (object)DBNull.Value : fromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", toDate == DateTime.MinValue ? (object)DBNull.Value : toDate.Date);
                    cmd.Parameters.AddWithValue("@UserName", userName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Action", actionFilter ?? string.Empty);
                    cmd.Parameters.AddWithValue("@ItemSearch", itemSearch ?? string.Empty);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    result.Rows.Clear();
                    adapter.Fill(result);
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("Item history log procedure failed: " + ex.Message);
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

        private void EnsureItemHistoryLogStoredProcedure()
        {
            using (SqlCommand cmd = new SqlCommand(@"
IF OBJECT_ID(N'dbo.POS_ItemHistoryLog', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo.POS_ItemHistoryLog AS BEGIN SET NOCOUNT ON; END');", (SqlConnection)DataConnection))
            {
                cmd.ExecuteNonQuery();
            }

            using (SqlCommand cmd = new SqlCommand(@"
ALTER PROCEDURE dbo.POS_ItemHistoryLog
    @_Operation NVARCHAR(30) = N'GET',
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @UserName NVARCHAR(150) = N'',
    @Action NVARCHAR(150) = N'',
    @ItemSearch NVARCHAR(250) = N'',
    @CompanyId INT = 0,
    @BranchId INT = 0,
    @FinYearId INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF OBJECT_ID(N'dbo.ItemActivityLog', N'U') IS NULL
    BEGIN
        SELECT
            CAST(0 AS BIGINT) AS SortNo,
            CAST(NULL AS DATETIME) AS CreatedOn,
            CAST(N'' AS NVARCHAR(150)) AS Action,
            CAST(N'' AS NVARCHAR(150)) AS Source,
            CAST(N'' AS NVARCHAR(50)) AS ActivityType,
            CAST(N'' AS NVARCHAR(150)) AS UserName,
            CAST(N'' AS NVARCHAR(100)) AS ItemNo,
            CAST(N'' AS NVARCHAR(250)) AS ItemName,
            CAST(N'' AS NVARCHAR(150)) AS Barcode,
            CAST(N'' AS NVARCHAR(50)) AS UOM,
            CAST(0 AS DECIMAL(18,4)) AS Qty,
            CAST(0 AS DECIMAL(18,4)) AS StockIn,
            CAST(0 AS DECIMAL(18,4)) AS StockOut,
            CAST(0 AS DECIMAL(18,4)) AS QtyDifference,
            CAST(0 AS DECIMAL(18,4)) AS UnitCost,
            CAST(0 AS DECIMAL(18,4)) AS RetailPrice,
            CAST(0 AS DECIMAL(18,4)) AS WalkinPrice,
            CAST(N'' AS NVARCHAR(100)) AS TransactionNo,
            CAST(N'' AS NVARCHAR(100)) AS InvoiceNo,
            CAST(N'' AS NVARCHAR(250)) AS PartyName,
            CAST(N'' AS NVARCHAR(MAX)) AS ActivityDetails,
            CAST(N'' AS NVARCHAR(150)) AS CounterName,
            CAST(N'' AS NVARCHAR(100)) AS CounterSessionId
        WHERE 1 = 0;
        RETURN;
    END

    IF @_Operation = N'GETUSERS'
    BEGIN
        SELECT DISTINCT ISNULL(UserName, N'') AS Value
        FROM dbo.ItemActivityLog
        WHERE ISNULL(UserName, N'') <> N''
        ORDER BY Value;
        RETURN;
    END

    IF @_Operation = N'GETACTIONS'
    BEGIN
        SELECT Value
        FROM (VALUES
            (N'Item Created'), (N'Item Updated'), (N'Purchase'), (N'Purchase Updated'),
            (N'Sales'), (N'Sales Updated'), (N'Purchase Return'), (N'Purchase Return Updated'),
            (N'Sales Return'), (N'Sales Return Updated'), (N'Stock Adjustment'), (N'Stock Adjustment Updated')
        ) v(Value);
        RETURN;
    END

    ;WITH MatchingItemIds AS
    (
        SELECT ItemId FROM dbo.ItemActivityLog
        WHERE @ItemSearch = N'' OR ISNULL(ItemName, N'') LIKE N'%' + @ItemSearch + N'%' OR ISNULL(ItemNo, N'') = @ItemSearch OR ISNULL(Barcode, N'') = @ItemSearch
        UNION
        SELECT ItemId FROM dbo.ItemMaster
        WHERE @ItemSearch <> N'' AND (ISNULL(Description, N'') LIKE N'%' + @ItemSearch + N'%' OR ISNULL(ItemNo, N'') = @ItemSearch OR ISNULL(Barcode, N'') = @ItemSearch)
        UNION
        SELECT ItemId FROM dbo.PriceSettings
        WHERE @ItemSearch <> N'' AND (ISNULL(BarCode, N'') = @ItemSearch OR ISNULL(AliasBarcode, N'') = @ItemSearch)
        UNION
        SELECT ItemId FROM dbo.ItemAlternativeBarcode
        WHERE @ItemSearch <> N'' AND ISNULL(Barcode, N'') = @ItemSearch
    )
    SELECT
        CAST(ISNULL(ItemActivityLogId, 0) AS BIGINT) AS SortNo,
        CreatedOn,
        CASE
            WHEN UPPER(ISNULL(ActivityType, N'')) IN (N'SAVE', N'ADD', N'CREATE', N'CREATED') THEN N'Item Created'
            WHEN UPPER(ISNULL(ActivityType, N'')) IN (N'UPDATE', N'EDIT', N'UPDATED') THEN N'Item Updated'
            ELSE N'Item ' + ISNULL(NULLIF(ActivityType, N''), N'Saved')
        END AS Action,
        CAST(N'Item Master' AS NVARCHAR(150)) AS Source,
        ISNULL(ActivityType, N'') AS ActivityType,
        ISNULL(UserName, N'') AS UserName,
        ISNULL(ItemNo, N'') AS ItemNo,
        ISNULL(ItemName, N'') AS ItemName,
        ISNULL(Barcode, N'') AS Barcode,
        CAST(N'' AS NVARCHAR(50)) AS UOM,
        CAST(ISNULL(Quantity, ISNULL(Available, 0)) AS DECIMAL(18,4)) AS Qty,
        CAST(0 AS DECIMAL(18,4)) AS StockIn,
        CAST(0 AS DECIMAL(18,4)) AS StockOut,
        CAST(0 AS DECIMAL(18,4)) AS QtyDifference,
        CAST(ISNULL(UnitCost, 0) AS DECIMAL(18,4)) AS UnitCost,
        CAST(ISNULL(RetailPrice, 0) AS DECIMAL(18,4)) AS RetailPrice,
        CAST(ISNULL(WalkinPrice, 0) AS DECIMAL(18,4)) AS WalkinPrice,
        CAST(N'' AS NVARCHAR(100)) AS TransactionNo,
        CAST(N'' AS NVARCHAR(100)) AS InvoiceNo,
        CAST(N'' AS NVARCHAR(250)) AS PartyName,
        CAST(ISNULL(ActivityDetails, N'') AS NVARCHAR(MAX)) AS ActivityDetails,
        ISNULL(CounterName, N'') AS CounterName,
        CONVERT(NVARCHAR(100), ISNULL(CounterSessionId, 0)) AS CounterSessionId
    FROM dbo.ItemActivityLog
    WHERE (@FromDate IS NULL OR CreatedOn >= @FromDate)
      AND (@ToDate IS NULL OR CreatedOn < DATEADD(DAY, 1, @ToDate))
      AND (@UserName = N'' OR ISNULL(UserName, N'') = @UserName)
      AND (@Action = N'' OR
            (@Action = N'Item Created' AND UPPER(ISNULL(ActivityType, N'')) IN (N'SAVE', N'ADD', N'CREATE', N'CREATED')) OR
            (@Action = N'Item Updated' AND UPPER(ISNULL(ActivityType, N'')) IN (N'UPDATE', N'EDIT', N'UPDATED')))
      AND (@ItemSearch = N'' OR ItemId IN (SELECT ItemId FROM MatchingItemIds))
      AND (@CompanyId = 0 OR ISNULL(CompanyId, 0) = @CompanyId)
      AND (@BranchId = 0 OR ISNULL(BranchId, 0) = @BranchId)
      AND (@FinYearId = 0 OR ISNULL(FinYearId, 0) = @FinYearId)
    ORDER BY CreatedOn DESC, ItemActivityLogId DESC;
END", (SqlConnection)DataConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private static DataTable CreateCombinedTable()
        {
            var table = new DataTable();
            table.Columns.Add("SortNo", typeof(long));
            table.Columns.Add("CreatedOn", typeof(DateTime));
            table.Columns.Add("Action", typeof(string));
            table.Columns.Add("Source", typeof(string));
            table.Columns.Add("ActivityType", typeof(string));
            table.Columns.Add("UserName", typeof(string));
            table.Columns.Add("ItemNo", typeof(string));
            table.Columns.Add("ItemName", typeof(string));
            table.Columns.Add("Barcode", typeof(string));
            table.Columns.Add("UOM", typeof(string));
            table.Columns.Add("Qty", typeof(decimal));
            table.Columns.Add("StockIn", typeof(decimal));
            table.Columns.Add("StockOut", typeof(decimal));
            table.Columns.Add("QtyDifference", typeof(decimal));
            table.Columns.Add("UnitCost", typeof(decimal));
            table.Columns.Add("RetailPrice", typeof(decimal));
            table.Columns.Add("WalkinPrice", typeof(decimal));
            table.Columns.Add("TransactionNo", typeof(string));
            table.Columns.Add("InvoiceNo", typeof(string));
            table.Columns.Add("PartyName", typeof(string));
            table.Columns.Add("ActivityDetails", typeof(string));
            table.Columns.Add("CounterName", typeof(string));
            table.Columns.Add("CounterSessionId", typeof(string));
            return table;
        }

        private static void AddStockRow(DataTable target, DataRow source)
        {
            DataRow row = target.NewRow();
            string sourceAction = NormalizeStockSource(FirstText(source, "Action"));
            string activityType = FirstText(source, "ActivityType");
            decimal stockIn = FirstDecimal(source, "StockIn", "Stock In");
            decimal stockOut = FirstDecimal(source, "StockOut", "Stock Out");
            decimal movement = FirstDecimal(source, "QtyDifference", "Qty Difference", "MovementQty");
            if (movement == 0m && stockIn != 0m) movement = stockIn;
            if (movement == 0m && stockOut != 0m) movement = 0m - Math.Abs(stockOut);

            row["SortNo"] = FirstLong(source, "ActivityLogId", "SlNo");
            row["CreatedOn"] = FirstDate(source, "CreatedOn");
            row["Action"] = IsUpdate(activityType) ? sourceAction + " Updated" : sourceAction;
            row["Source"] = sourceAction;
            row["ActivityType"] = activityType;
            row["UserName"] = FirstText(source, "UserName");
            row["ItemNo"] = FirstText(source, "ItemNo");
            row["ItemName"] = FirstText(source, "ItemName");
            row["Barcode"] = FirstText(source, "Barcode");
            row["UOM"] = FirstText(source, "UOM");
            row["Qty"] = FirstDecimal(source, "Qty");
            row["StockIn"] = stockIn;
            row["StockOut"] = stockOut;
            row["QtyDifference"] = movement;
            row["UnitCost"] = FirstDecimal(source, "UnitCost", "UnitPrice");
            row["RetailPrice"] = FirstDecimal(source, "RetailPrice", "SellingPrice");
            row["WalkinPrice"] = FirstDecimal(source, "WalkinPrice");
            row["TransactionNo"] = FirstText(source, "TransactionNo", "DocNo", "PurchaseNo", "SalesBillNo");
            row["InvoiceNo"] = FirstText(source, "InvoiceNo");
            row["PartyName"] = FirstText(source, "PartyName", "SupplierName", "CustomerName");
            row["ActivityDetails"] = FirstText(source, "ActivityDetails", "Reason", "Comments", "Remarks");
            row["CounterName"] = FirstText(source, "CounterName");
            row["CounterSessionId"] = FirstText(source, "CounterSessionId");
            target.Rows.Add(row);
        }

        private static IEnumerable<string> GetActionFilterValues()
        {
            return new[]
            {
                "Item Created", "Item Updated", "Purchase", "Purchase Updated", "Sales", "Sales Updated",
                "Purchase Return", "Purchase Return Updated", "Sales Return", "Sales Return Updated",
                "Stock Adjustment", "Stock Adjustment Updated"
            };
        }

        private static void AddUserRows(ISet<string> users, DataTable table)
        {
            if (table == null || !table.Columns.Contains("Value")) return;
            foreach (DataRow row in table.Rows)
            {
                string value = Convert.ToString(row["Value"]);
                if (!string.IsNullOrWhiteSpace(value)) users.Add(value);
            }
        }

        private static string GetStockActionFilter(string actionFilter)
        {
            if (string.IsNullOrWhiteSpace(actionFilter)) return string.Empty;
            if (actionFilter.IndexOf("Purchase Return", StringComparison.OrdinalIgnoreCase) >= 0) return "Purchase Return";
            if (actionFilter.IndexOf("Sales Return", StringComparison.OrdinalIgnoreCase) >= 0) return "Sales Return";
            if (actionFilter.IndexOf("Purchase", StringComparison.OrdinalIgnoreCase) >= 0) return "Purchase";
            if (actionFilter.IndexOf("Sales", StringComparison.OrdinalIgnoreCase) >= 0) return "Sales";
            return string.Empty;
        }

        private static void ApplyActionFilter(DataTable table, string actionFilter)
        {
            if (string.IsNullOrWhiteSpace(actionFilter) || actionFilter == "All Actions") return;
            for (int i = table.Rows.Count - 1; i >= 0; i--)
            {
                if (!string.Equals(Convert.ToString(table.Rows[i]["Action"]), actionFilter, StringComparison.OrdinalIgnoreCase))
                {
                    table.Rows.RemoveAt(i);
                }
            }
        }

        private static string NormalizeStockSource(string action)
        {
            if (string.IsNullOrWhiteSpace(action)) return "Stock Adjustment";
            if (action.IndexOf("Purchase Return", StringComparison.OrdinalIgnoreCase) >= 0) return "Purchase Return";
            if (action.IndexOf("Sales Return", StringComparison.OrdinalIgnoreCase) >= 0) return "Sales Return";
            if (action.IndexOf("Purchase", StringComparison.OrdinalIgnoreCase) >= 0) return "Purchase";
            if (action.IndexOf("Sales", StringComparison.OrdinalIgnoreCase) >= 0) return "Sales";
            if (action.IndexOf("Stock", StringComparison.OrdinalIgnoreCase) >= 0) return "Stock Adjustment";
            return action;
        }

        private static bool IsUpdate(string value)
        {
            return string.Equals(value, "UPDATE", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "EDIT", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "UPDATED", StringComparison.OrdinalIgnoreCase);
        }

        private static string FirstText(DataRow row, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                {
                    string value = Convert.ToString(row[columnName]);
                    if (!string.IsNullOrWhiteSpace(value)) return value;
                }
            }
            return string.Empty;
        }

        private static decimal FirstDecimal(DataRow row, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                {
                    decimal value;
                    if (decimal.TryParse(Convert.ToString(row[columnName]), out value)) return value;
                }
            }
            return 0m;
        }

        private static long FirstLong(DataRow row, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                {
                    long value;
                    if (long.TryParse(Convert.ToString(row[columnName]), out value)) return value;
                }
            }
            return 0L;
        }

        private static DateTime FirstDate(DataRow row, params string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                {
                    DateTime value;
                    if (DateTime.TryParse(Convert.ToString(row[columnName]), out value)) return value;
                }
            }
            return DateTime.MinValue;
        }
    }
}
