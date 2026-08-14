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
            table.Columns.Add("Vendor", typeof(string));
            table.Columns.Add("Customer", typeof(string));
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
            string partyName = FirstText(source, "PartyName", "SupplierName", "CustomerName", "VendorName", "Vendor", "Customer");
            row["PartyName"] = partyName;

            string actionStr = Convert.ToString(row["Action"]) ?? string.Empty;
            if (actionStr.IndexOf("Purchase", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                row["Vendor"] = partyName;
                row["Customer"] = string.Empty;
            }
            else if (actionStr.IndexOf("Sales", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                row["Customer"] = partyName;
                row["Vendor"] = string.Empty;
            }
            else
            {
                row["Vendor"] = string.Empty;
                row["Customer"] = string.Empty;
            }

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

        public string GetTransactionActivityDetails(
            string action,
            long transNo,
            string itemNo,
            string itemName,
            string barcode,
            string rawDetails,
            string partyName,
            string userName,
            string createdOn,
            string qty,
            string unitCost,
            string retailPrice,
            string walkinPrice,
            string uom)
        {
            action = action ?? string.Empty;
            bool isItemMaster = action.IndexOf("Item", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isPurchase = action.IndexOf("Purchase", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isSales = action.IndexOf("Sales", StringComparison.OrdinalIgnoreCase) >= 0;

            if (isItemMaster)
            {
                string filtered = FilterActivityDetails(rawDetails);
                if (!string.IsNullOrWhiteSpace(filtered) && !IsGenericHeaderOnly(filtered))
                {
                    return filtered;
                }
            }

            if (isPurchase && transNo > 0)
            {
                string purchaseInfo = FetchDetailedPurchaseInfo(action, transNo, itemNo, itemName, barcode, partyName, userName, createdOn, qty, unitCost, retailPrice, uom);
                if (!string.IsNullOrWhiteSpace(purchaseInfo))
                {
                    return purchaseInfo;
                }
            }

            if (isSales && transNo > 0)
            {
                string salesInfo = FetchDetailedSalesInfo(action, transNo, itemNo, itemName, barcode, partyName, userName, createdOn, qty, unitCost, retailPrice, uom);
                if (!string.IsNullOrWhiteSpace(salesInfo))
                {
                    return salesInfo;
                }
            }

            if (!string.IsNullOrWhiteSpace(rawDetails) && !IsGenericHeaderOnly(rawDetails))
            {
                return FilterActivityDetails(rawDetails);
            }

            return BuildComprehensiveFallbackDetails(action, transNo, itemNo, itemName, barcode, partyName, userName, createdOn, qty, unitCost, retailPrice, walkinPrice, uom);
        }

        private static bool IsGenericHeaderOnly(string details)
        {
            if (string.IsNullOrWhiteSpace(details)) return true;
            string clean = details.Trim();
            if (clean.StartsWith("Purchase No:", StringComparison.OrdinalIgnoreCase) && clean.Length < 70 && !clean.Contains("\n")) return true;
            if (clean.StartsWith("Sales invoice #", StringComparison.OrdinalIgnoreCase) && clean.Length < 70 && !clean.Contains("\n")) return true;
            if (clean.Equals("Recovered from saved purchase invoice.", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private static string FilterActivityDetails(string details)
        {
            if (string.IsNullOrWhiteSpace(details)) return details;
            var lines = details.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var filtered = new System.Text.StringBuilder();
            foreach (var line in lines)
            {
                string trimmed = line.TrimStart('-', ' ');
                if (trimmed.StartsWith("Unit '", StringComparison.OrdinalIgnoreCase) &&
                    (trimmed.Contains("Retail Price changed") || trimmed.Contains("Walkin Price changed")))
                {
                    continue;
                }
                filtered.AppendLine(line);
            }
            return filtered.ToString().TrimEnd();
        }

        private string FetchDetailedPurchaseInfo(
            string action,
            long transNo,
            string itemNo,
            string itemName,
            string barcode,
            string partyName,
            string userName,
            string createdOn,
            string qty,
            string unitCost,
            string retailPrice,
            string uom)
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                if (!TableExists("PMaster"))
                {
                    return string.Empty;
                }

                using (SqlCommand cmd = new SqlCommand(@"
SELECT TOP 1
    pm.PurchaseNo,
    ISNULL(pm.InvoiceNo, '') AS InvoiceNo,
    pm.InvoiceDate,
    pm.PurchaseDate,
    ISNULL(pm.VendorName, '') AS VendorName,
    ISNULL(pm.UserName, '') AS UserName,
    ISNULL(pm.Paymode, '') AS Paymode,
    ISNULL(NULLIF(pm.NetTotal, 0), pm.GrandTotal) AS NetTotal,
    ISNULL(pm.TaxAmt, 0) AS HeaderTaxAmt,
    ISNULL(pd.ItemName, @ItemName) AS ItemName,
    ISNULL(pd.Barcode, @Barcode) AS Barcode,
    ISNULL(pd.Unit, '') AS Unit,
    ISNULL(pd.Packing, 1) AS Packing,
    ISNULL(pd.Qty, 0) AS Qty,
    ISNULL(pd.Cost, 0) AS Cost,
    ISNULL(pd.SalesPrice, 0) AS SalesPrice,
    ISNULL(pd.Free, 0) AS Free,
    ISNULL(pd.TaxType, '') AS TaxType,
    ISNULL(pd.TaxPer, 0) AS TaxPer,
    ISNULL(pd.TaxAmt, 0) AS TaxAmt,
    ISNULL(pd.TotalSP, (ISNULL(pd.Cost, 0) * ISNULL(pd.Qty, 0)) + ISNULL(pd.TaxAmt, 0)) AS LineTotal
FROM dbo.PMaster pm
LEFT JOIN dbo.PDetails pd ON pd.PurchaseNo = pm.PurchaseNo AND pd.CompanyId = pm.CompanyId AND pd.BranchID = pm.BranchID AND pd.FinYearId = pm.FinYearId
WHERE pm.PurchaseNo = @TransNo
  AND (@CompanyId = 0 OR ISNULL(pm.CompanyId, 0) = @CompanyId)
  AND (@BranchId = 0 OR ISNULL(pm.BranchId, 0) = @BranchId)
  AND (@FinYearId = 0 OR ISNULL(pm.FinYearId, 0) = @FinYearId)
  AND (
        @Barcode <> '' AND pd.Barcode = @Barcode
        OR @ItemName <> '' AND pd.ItemName = @ItemName
        OR pd.PurchaseNo IS NOT NULL
      )
ORDER BY CASE WHEN pd.Barcode = @Barcode THEN 0 WHEN pd.ItemName = @ItemName THEN 1 ELSE 2 END;", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@TransNo", transNo);
                    cmd.Parameters.AddWithValue("@Barcode", barcode ?? string.Empty);
                    cmd.Parameters.AddWithValue("@ItemName", itemName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var sb = new System.Text.StringBuilder();
                            sb.AppendLine("Action: " + action);
                            sb.AppendLine("GRN No: " + reader["PurchaseNo"]);
                            sb.AppendLine("Vendor: " + FirstNonEmpty(Convert.ToString(reader["VendorName"]), partyName));
                            sb.AppendLine("Invoice Number: " + Convert.ToString(reader["InvoiceNo"]));

                            object invDate = reader["InvoiceDate"];
                            object purDate = reader["PurchaseDate"];
                            if (invDate != null && invDate != DBNull.Value) sb.AppendLine("Invoice Date: " + Convert.ToDateTime(invDate).ToString("dd MMM yyyy"));
                            if (purDate != null && purDate != DBNull.Value) sb.AppendLine("Purchase Date: " + Convert.ToDateTime(purDate).ToString("dd MMM yyyy"));

                            sb.AppendLine("Billed By: " + FirstNonEmpty(Convert.ToString(reader["UserName"]), userName));
                            sb.AppendLine("----------------------------------------");
                            sb.AppendLine("Item: " + FirstNonEmpty(Convert.ToString(reader["ItemName"]), itemName) + (!string.IsNullOrWhiteSpace(itemNo) ? $" (Code: {itemNo})" : "") + (!string.IsNullOrWhiteSpace(barcode) ? $" | Barcode: {barcode}" : ""));
                            sb.AppendLine("Unit: " + FirstNonEmpty(Convert.ToString(reader["Unit"]), uom));
                            sb.AppendLine("Qty: " + FormatDecimalVal(Convert.ToDecimal(reader["Qty"]), qty));
                            sb.AppendLine("Packing: " + FormatDecimalVal(Convert.ToDecimal(reader["Packing"]), "1"));
                            sb.AppendLine("Unit Cost: " + FormatDecimalVal(Convert.ToDecimal(reader["Cost"]), unitCost));
                            sb.AppendLine("Selling Price: " + FormatDecimalVal(Convert.ToDecimal(reader["SalesPrice"]), retailPrice));
                            sb.AppendLine("Free Qty: " + FormatDecimalVal(Convert.ToDecimal(reader["Free"]), "0"));

                            string taxType = Convert.ToString(reader["TaxType"]);
                            if (!string.IsNullOrWhiteSpace(taxType)) sb.AppendLine("Tax Type: " + taxType);

                            sb.AppendLine("Tax %: " + FormatDecimalVal(Convert.ToDecimal(reader["TaxPer"]), "0") + "%");
                            sb.AppendLine("Tax Amt: " + FormatDecimalVal(Convert.ToDecimal(reader["TaxAmt"]), "0"));
                            sb.AppendLine("Line Net Amount: " + FormatDecimalVal(Convert.ToDecimal(reader["LineTotal"]), "0"));
                            sb.AppendLine("Invoice Net Total: " + FormatDecimalVal(Convert.ToDecimal(reader["NetTotal"]), "0"));

                            string paymode = Convert.ToString(reader["Paymode"]);
                            if (!string.IsNullOrWhiteSpace(paymode)) sb.AppendLine("Payment Mode: " + paymode);

                            if (action.IndexOf("Updated", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                sb.AppendLine();
                                sb.AppendLine("Updates:");
                                sb.AppendLine("- Purchase Invoice updated in system.");
                            }

                            return sb.ToString().TrimEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("FetchDetailedPurchaseInfo error: " + ex.Message);
            }

            return string.Empty;
        }

        private string FetchDetailedSalesInfo(
            string action,
            long transNo,
            string itemNo,
            string itemName,
            string barcode,
            string partyName,
            string userName,
            string createdOn,
            string qty,
            string unitCost,
            string retailPrice,
            string uom)
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                if (!TableExists("SMaster"))
                {
                    return string.Empty;
                }

                using (SqlCommand cmd = new SqlCommand(@"
SELECT TOP 1
    sm.BillNo,
    sm.BillDate,
    ISNULL(sm.CustomerName, '') AS CustomerName,
    ISNULL(u.UserName, CASE WHEN ISNULL(sm.UserId, 0) = 0 THEN '' ELSE 'User ' + CONVERT(nvarchar(20), sm.UserId) END) AS UserName,
    ISNULL(sm.PaymodeName, '') AS PaymodeName,
    ISNULL(sm.NetAmount, 0) AS NetAmount,
    ISNULL(sm.TaxAmt, 0) AS HeaderTaxAmt,
    ISNULL(sd.ItemName, @ItemName) AS ItemName,
    ISNULL(sd.Barcode, @Barcode) AS Barcode,
    ISNULL(sd.Unit, '') AS Unit,
    ISNULL(sd.Qty, 0) AS Qty,
    ISNULL(sd.Cost, 0) AS Cost,
    ISNULL(sd.UnitPrice, 0) AS UnitPrice,
    ISNULL(sd.TaxPer, 0) AS TaxPer,
    ISNULL(sd.TaxAmt, 0) AS TaxAmt,
    ISNULL(sd.TotalAmount, (ISNULL(sd.UnitPrice, 0) * ISNULL(sd.Qty, 0)) + ISNULL(sd.TaxAmt, 0)) AS LineTotal
FROM dbo.SMaster sm
LEFT JOIN dbo.Users u ON u.UserID = sm.UserId
LEFT JOIN dbo.SDetails sd ON sd.BillNo = sm.BillNo AND sd.CompanyId = sm.CompanyId AND sd.BranchId = sm.BranchId AND sd.FinYearId = sm.FinYearId
WHERE sm.BillNo = @TransNo
  AND (@CompanyId = 0 OR ISNULL(sm.CompanyId, 0) = @CompanyId)
  AND (@BranchId = 0 OR ISNULL(sm.BranchId, 0) = @BranchId)
  AND (@FinYearId = 0 OR ISNULL(sm.FinYearId, 0) = @FinYearId)
  AND (
        @Barcode <> '' AND sd.Barcode = @Barcode
        OR @ItemName <> '' AND sd.ItemName = @ItemName
        OR sd.BillNo IS NOT NULL
      )
ORDER BY CASE WHEN sd.Barcode = @Barcode THEN 0 WHEN sd.ItemName = @ItemName THEN 1 ELSE 2 END;", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@TransNo", transNo);
                    cmd.Parameters.AddWithValue("@Barcode", barcode ?? string.Empty);
                    cmd.Parameters.AddWithValue("@ItemName", itemName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var sb = new System.Text.StringBuilder();
                            sb.AppendLine("Action: " + action);
                            sb.AppendLine("Bill No / Sales Invoice No: " + reader["BillNo"]);
                            sb.AppendLine("Customer: " + FirstNonEmpty(Convert.ToString(reader["CustomerName"]), partyName));

                            object billDate = reader["BillDate"];
                            if (billDate != null && billDate != DBNull.Value) sb.AppendLine("Invoice Date: " + Convert.ToDateTime(billDate).ToString("dd MMM yyyy hh:mm tt"));

                            sb.AppendLine("Billed By: " + FirstNonEmpty(Convert.ToString(reader["UserName"]), userName));
                            sb.AppendLine("----------------------------------------");
                            sb.AppendLine("Item: " + FirstNonEmpty(Convert.ToString(reader["ItemName"]), itemName) + (!string.IsNullOrWhiteSpace(itemNo) ? $" (Code: {itemNo})" : "") + (!string.IsNullOrWhiteSpace(barcode) ? $" | Barcode: {barcode}" : ""));
                            sb.AppendLine("Unit: " + FirstNonEmpty(Convert.ToString(reader["Unit"]), uom));
                            sb.AppendLine("Qty: " + FormatDecimalVal(Convert.ToDecimal(reader["Qty"]), qty));
                            sb.AppendLine("Selling Price (Unit Price): " + FormatDecimalVal(Convert.ToDecimal(reader["UnitPrice"]), retailPrice));
                            sb.AppendLine("Unit Cost: " + FormatDecimalVal(Convert.ToDecimal(reader["Cost"]), unitCost));
                            sb.AppendLine("Tax %: " + FormatDecimalVal(Convert.ToDecimal(reader["TaxPer"]), "0") + "%");
                            sb.AppendLine("Tax Amt: " + FormatDecimalVal(Convert.ToDecimal(reader["TaxAmt"]), "0"));
                            sb.AppendLine("Line Net Amount: " + FormatDecimalVal(Convert.ToDecimal(reader["LineTotal"]), "0"));
                            sb.AppendLine("Invoice Net Total: " + FormatDecimalVal(Convert.ToDecimal(reader["NetAmount"]), "0"));

                            string paymode = Convert.ToString(reader["PaymodeName"]);
                            if (!string.IsNullOrWhiteSpace(paymode)) sb.AppendLine("Payment Mode: " + paymode);

                            if (action.IndexOf("Updated", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                sb.AppendLine();
                                sb.AppendLine("Updates:");
                                sb.AppendLine("- Sales Invoice updated in system.");
                            }

                            return sb.ToString().TrimEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("FetchDetailedSalesInfo error: " + ex.Message);
            }

            return string.Empty;
        }

        private static string BuildComprehensiveFallbackDetails(
            string action,
            long transNo,
            string itemNo,
            string itemName,
            string barcode,
            string partyName,
            string userName,
            string createdOn,
            string qty,
            string unitCost,
            string retailPrice,
            string walkinPrice,
            string uom)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Action: " + action);

            bool isPurchase = action.IndexOf("Purchase", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isSales = action.IndexOf("Sales", StringComparison.OrdinalIgnoreCase) >= 0;

            if (isPurchase)
            {
                if (transNo > 0) sb.AppendLine("GRN No: " + transNo);
                if (!string.IsNullOrWhiteSpace(partyName)) sb.AppendLine("Vendor: " + partyName);
            }
            else if (isSales)
            {
                if (transNo > 0) sb.AppendLine("Bill No: " + transNo);
                if (!string.IsNullOrWhiteSpace(partyName)) sb.AppendLine("Customer: " + partyName);
            }
            else
            {
                if (transNo > 0) sb.AppendLine("Doc No: " + transNo);
                if (!string.IsNullOrWhiteSpace(partyName)) sb.AppendLine("Party: " + partyName);
            }

            if (!string.IsNullOrWhiteSpace(userName)) sb.AppendLine("Billed By: " + userName);
            if (!string.IsNullOrWhiteSpace(createdOn)) sb.AppendLine("Date: " + createdOn);

            sb.AppendLine("----------------------------------------");
            string itemDesc = FirstNonEmpty(itemName, itemNo, barcode);
            sb.AppendLine("Item: " + itemDesc + (!string.IsNullOrWhiteSpace(itemNo) && itemNo != itemDesc ? $" (Code: {itemNo})" : "") + (!string.IsNullOrWhiteSpace(barcode) && barcode != itemDesc ? $" | Barcode: {barcode}" : ""));
            if (!string.IsNullOrWhiteSpace(uom)) sb.AppendLine("Unit: " + uom);
            if (!string.IsNullOrWhiteSpace(qty)) sb.AppendLine("Qty: " + qty);
            if (!string.IsNullOrWhiteSpace(unitCost)) sb.AppendLine("Unit Cost: " + unitCost);
            if (!string.IsNullOrWhiteSpace(retailPrice)) sb.AppendLine("Selling Price (Retail): " + retailPrice);
            if (!string.IsNullOrWhiteSpace(walkinPrice)) sb.AppendLine("Walkin Price: " + walkinPrice);

            return sb.ToString().Trim();
        }

        private bool TableExists(string tableName)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT CASE WHEN OBJECT_ID(@TableName, 'U') IS NULL THEN 0 ELSE 1 END;", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@TableName", "dbo." + tableName);
                    return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
                }
            }
            catch
            {
                return false;
            }
        }

        private static string FirstNonEmpty(params string[] values)
        {
            foreach (var val in values)
            {
                if (!string.IsNullOrWhiteSpace(val)) return val.Trim();
            }
            return string.Empty;
        }

        private static string FormatDecimalVal(decimal val, string fallback)
        {
            if (val != 0m) return val.ToString("0.####");
            decimal parsed;
            if (!string.IsNullOrWhiteSpace(fallback) && decimal.TryParse(fallback, out parsed))
            {
                return parsed.ToString("0.####");
            }
            return !string.IsNullOrWhiteSpace(fallback) ? fallback : "0";
        }
    }
}
