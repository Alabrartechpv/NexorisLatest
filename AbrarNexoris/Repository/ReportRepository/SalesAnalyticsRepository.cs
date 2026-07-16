using Dapper;
using ModelClass;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Repository.ReportRepository
{
    public class SalesAnalyticsRepository : BaseRepostitory
    {
        public SalesAnalyticsOverview GetAnalytics(DateTime fromDate, DateTime toDate)
        {
            DateTime rangeFrom = fromDate.Date;
            DateTime rangeTo = toDate.Date;
            if (rangeTo < rangeFrom)
            {
                DateTime swap = rangeFrom;
                rangeFrom = rangeTo;
                rangeTo = swap;
            }

            DateTime exclusiveTo = rangeTo.AddDays(1);
            int dayCount = Math.Max(1, (rangeTo - rangeFrom).Days + 1);
            DateTime previousFrom = rangeFrom.AddDays(-dayCount);

            SalesAnalyticsOverview overview = new SalesAnalyticsOverview
            {
                FromDate = rangeFrom,
                ToDate = rangeTo,
                PreviousFromDate = previousFrom,
                PreviousToDate = rangeFrom.AddDays(-1)
            };

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                overview.Summary = ReadSummary(rangeFrom, exclusiveTo);
                SalesAnalyticsSummary previous = ReadSummary(previousFrom, rangeFrom);
                ApplyChangePercentages(overview.Summary, previous);
                overview.SalesTrend = ReadTrend(rangeFrom, exclusiveTo);
                overview.TopByQuantity = ReadTopItems(rangeFrom, exclusiveTo, "Qty");
                overview.TopByAmount = ReadTopItems(rangeFrom, exclusiveTo, "Amount");
                overview.ItemSales = ReadItemSalesMap(rangeFrom, exclusiveTo);
                overview.PaymentMethods = ReadBreakdown(rangeFrom, exclusiveTo, SalesBreakdownKind.Payment);
                overview.Categories = ReadBreakdown(rangeFrom, exclusiveTo, SalesBreakdownKind.Category);
                overview.ItemCategories = ReadItemCategoryDetails(rangeFrom, exclusiveTo);
                overview.Customers = ReadBreakdown(rangeFrom, exclusiveTo, SalesBreakdownKind.Customer);
            }
            finally
            {
                if (DataConnection != null && DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return overview;
        }

        private SalesAnalyticsSummary ReadSummary(DateTime fromDate, DateTime toDate)
        {
            string sql = @"
IF OBJECT_ID('SMaster', 'U') IS NULL
    SELECT CAST(0 AS decimal(18,2)) AS TotalSales, CAST(0 AS int) AS TotalOrders,
           CAST(0 AS decimal(18,2)) AS TotalTax, CAST(0 AS decimal(18,2)) AS TotalDiscount, CAST(0 AS decimal(18,2)) AS OutstandingAmount,
           CAST(0 AS decimal(18,2)) AS ReceivedAmount
ELSE
    SELECT
        ISNULL(SUM(ISNULL(NetAmount, 0)), 0) AS TotalSales,
        COUNT(1) AS TotalOrders,
        ISNULL(SUM(ISNULL(TaxAmt, 0)), 0) AS TotalTax,
        ISNULL(SUM(ISNULL(DiscountAmt, 0)), 0) AS TotalDiscount,
        ISNULL(SUM(CASE WHEN ISNULL(NetAmount, 0) > ISNULL(ReceivedAmount, 0) THEN ISNULL(NetAmount, 0) - ISNULL(ReceivedAmount, 0) ELSE 0 END), 0) AS OutstandingAmount,
        ISNULL(SUM(ISNULL(ReceivedAmount, 0)), 0) AS ReceivedAmount
    FROM SMaster
    WHERE BranchId = @BranchId AND CompanyId = @CompanyId AND FinYearId = @FinYearId
      AND ISNULL(CancelFlag, 0) = 0
      AND BillDate >= @FromDate AND BillDate < @ToDate;";

            SalesAnalyticsSummary summary = DataConnection.QueryFirstOrDefault<SalesAnalyticsSummary>(sql, BuildParameters(fromDate, toDate)) ?? new SalesAnalyticsSummary();
            summary.TotalItemsSold = ReadTotalItemsSold(fromDate, toDate);
            summary.TotalProfit = ReadTotalProfit(fromDate, toDate);
            summary.AverageOrderValue = summary.TotalOrders > 0 ? summary.TotalSales / summary.TotalOrders : 0;
            summary.ProfitMarginPercent = summary.TotalSales > 0 ? (summary.TotalProfit / summary.TotalSales) * 100M : 0;
            return summary;
        }

        private decimal ReadTotalProfit(DateTime fromDate, DateTime toDate)
        {
            string sql = @"
IF OBJECT_ID('SDetails', 'U') IS NULL OR OBJECT_ID('SMaster', 'U') IS NULL
    SELECT CAST(0 AS decimal(18,2))
ELSE
    SELECT ISNULL(SUM(ISNULL(sd.MarginAmt, 0)), 0)
    FROM SDetails sd
    INNER JOIN SMaster sm ON sm.BillNo = sd.BillNo AND sm.BranchId = sd.BranchId AND sm.CompanyId = sd.CompanyId AND sm.FinYearId = sd.FinYearId
    WHERE sm.BranchId = @BranchId AND sm.CompanyId = @CompanyId AND sm.FinYearId = @FinYearId
      AND ISNULL(sm.CancelFlag, 0) = 0
      AND sm.BillDate >= @FromDate AND sm.BillDate < @ToDate;";

            return DataConnection.QueryFirstOrDefault<decimal>(sql, BuildParameters(fromDate, toDate));
        }

        private decimal ReadTotalItemsSold(DateTime fromDate, DateTime toDate)
        {
            string sql = @"
IF OBJECT_ID('SDetails', 'U') IS NULL OR OBJECT_ID('SMaster', 'U') IS NULL
    SELECT CAST(0 AS decimal(18,2))
ELSE
    SELECT ISNULL(SUM(ISNULL(sd.Qty, 0)), 0)
    FROM SDetails sd
    INNER JOIN SMaster sm ON sm.BillNo = sd.BillNo AND sm.BranchId = sd.BranchId AND sm.CompanyId = sd.CompanyId AND sm.FinYearId = sd.FinYearId
    WHERE sm.BranchId = @BranchId AND sm.CompanyId = @CompanyId AND sm.FinYearId = @FinYearId
      AND ISNULL(sm.CancelFlag, 0) = 0
      AND sm.BillDate >= @FromDate AND sm.BillDate < @ToDate;";
            return DataConnection.QueryFirstOrDefault<decimal>(sql, BuildParameters(fromDate, toDate));
        }

        private void ApplyChangePercentages(SalesAnalyticsSummary current, SalesAnalyticsSummary previous)
        {
            current.SalesChangePercent = PercentChange(current.TotalSales, previous.TotalSales);
            current.OrdersChangePercent = PercentChange(current.TotalOrders, previous.TotalOrders);
            current.AverageOrderValueChangePercent = PercentChange(current.AverageOrderValue, previous.AverageOrderValue);
            current.ProfitChangePercent = PercentChange(current.TotalProfit, previous.TotalProfit);
            current.ItemsSoldChangePercent = PercentChange(current.TotalItemsSold, previous.TotalItemsSold);
        }

        private decimal PercentChange(decimal current, decimal previous)
        {
            if (previous == 0)
                return current == 0 ? 0 : 100;

            return ((current - previous) / Math.Abs(previous)) * 100M;
        }

        private decimal PercentChange(int current, int previous)
        {
            return PercentChange(Convert.ToDecimal(current), Convert.ToDecimal(previous));
        }

        private List<SalesTrendPoint> ReadTrend(DateTime fromDate, DateTime toDate)
        {
            List<SalesTrendPoint> trend = CreateTrendSkeleton(fromDate, toDate.AddDays(-1));
            string sql = @"
IF OBJECT_ID('SMaster', 'U') IS NULL
    SELECT CAST(NULL AS date) AS SaleDate, CAST(0 AS decimal(18,2)) AS Amount WHERE 1 = 0
ELSE
    SELECT CAST(BillDate AS date) AS SaleDate, ISNULL(SUM(ISNULL(NetAmount, 0)), 0) AS Amount
    FROM SMaster
    WHERE BranchId = @BranchId AND CompanyId = @CompanyId AND FinYearId = @FinYearId
      AND ISNULL(CancelFlag, 0) = 0
      AND BillDate >= @FromDate AND BillDate < @ToDate
    GROUP BY CAST(BillDate AS date);";

            foreach (SalesTrendPoint row in DataConnection.Query<SalesTrendPoint>(sql, BuildParameters(fromDate, toDate)))
            {
                SalesTrendPoint point = trend.FirstOrDefault(x => x.SaleDate == row.SaleDate.Date);
                if (point != null)
                    point.Amount = row.Amount;
            }

            return trend;
        }

        private List<SalesTrendPoint> CreateTrendSkeleton(DateTime fromDate, DateTime toDate)
        {
            List<SalesTrendPoint> trend = new List<SalesTrendPoint>();
            DateTime cursor = fromDate.Date;
            while (cursor <= toDate.Date)
            {
                trend.Add(new SalesTrendPoint { SaleDate = cursor, Caption = cursor.ToString("dd MMM"), Amount = 0 });
                cursor = cursor.AddDays(1);
            }
            return trend;
        }

        private List<SalesItemMetric> ReadTopItems(DateTime fromDate, DateTime toDate, string orderBy)
        {
            string sortColumn = orderBy == "Qty" ? "QtySold" : "Amount";
            string sql = @"
IF OBJECT_ID('SDetails', 'U') IS NULL OR OBJECT_ID('SMaster', 'U') IS NULL
    SELECT TOP 0 CAST('' AS nvarchar(200)) AS ItemName, CAST(0 AS decimal(18,2)) AS QtySold, CAST(0 AS decimal(18,2)) AS Amount, CAST(0 AS decimal(18,2)) AS Profit
ELSE
    SELECT TOP 10
        ISNULL(NULLIF(sd.ItemName, ''), 'Unknown Item') AS ItemName,
        ISNULL(SUM(ISNULL(sd.Qty, 0)), 0) AS QtySold,
        ISNULL(SUM(ISNULL(sd.TotalAmount, 0)), 0) AS Amount,
        ISNULL(SUM(ISNULL(sd.MarginAmt, 0)), 0) AS Profit
    FROM SDetails sd
    INNER JOIN SMaster sm ON sm.BillNo = sd.BillNo AND sm.BranchId = sd.BranchId AND sm.CompanyId = sd.CompanyId AND sm.FinYearId = sd.FinYearId
    WHERE sm.BranchId = @BranchId AND sm.CompanyId = @CompanyId AND sm.FinYearId = @FinYearId
      AND ISNULL(sm.CancelFlag, 0) = 0
      AND sm.BillDate >= @FromDate AND sm.BillDate < @ToDate
    GROUP BY sd.ItemName
    ORDER BY " + sortColumn + @" DESC;";

            return DataConnection.Query<SalesItemMetric>(sql, BuildParameters(fromDate, toDate)).ToList();
        }

        private List<SalesItemMetric> ReadItemSalesMap(DateTime fromDate, DateTime toDate)
        {
            string sql = @"
IF OBJECT_ID('SDetails', 'U') IS NULL OR OBJECT_ID('SMaster', 'U') IS NULL
    SELECT TOP 0 CAST('' AS nvarchar(200)) AS ItemName, CAST(0 AS decimal(18,2)) AS QtySold, CAST(0 AS decimal(18,2)) AS Amount, CAST(0 AS decimal(18,2)) AS Profit
ELSE
    SELECT
        ISNULL(NULLIF(sd.ItemName, ''), 'Unknown Item') AS ItemName,
        ISNULL(SUM(ISNULL(sd.Qty, 0)), 0) AS QtySold,
        ISNULL(SUM(ISNULL(sd.TotalAmount, 0)), 0) AS Amount,
        ISNULL(SUM(ISNULL(sd.MarginAmt, 0)), 0) AS Profit
    FROM SDetails sd
    INNER JOIN SMaster sm ON sm.BillNo = sd.BillNo AND sm.BranchId = sd.BranchId AND sm.CompanyId = sd.CompanyId AND sm.FinYearId = sd.FinYearId
    WHERE sm.BranchId = @BranchId AND sm.CompanyId = @CompanyId AND sm.FinYearId = @FinYearId
      AND ISNULL(sm.CancelFlag, 0) = 0
      AND sm.BillDate >= @FromDate AND sm.BillDate < @ToDate
    GROUP BY sd.ItemName
    ORDER BY Amount DESC;";
            return DataConnection.Query<SalesItemMetric>(sql, BuildParameters(fromDate, toDate)).ToList();
        }

        private List<SalesBreakdown> ReadBreakdown(DateTime fromDate, DateTime toDate, SalesBreakdownKind kind)
        {
            string sql;
            if (kind == SalesBreakdownKind.Payment)
            {
                sql = BuildPaymentBreakdownSql();
            }
            else if (kind == SalesBreakdownKind.Category)
            {
                string itemCategoryColumn = GetFirstExistingColumn("ItemMaster", "CategoryId", "CategoryID");
                string categoryKeyColumn = GetFirstExistingColumn("Category", "Id", "CategoryId", "CategoryID");

                if (string.IsNullOrEmpty(itemCategoryColumn) || string.IsNullOrEmpty(categoryKeyColumn))
                {
                    sql = @"
IF OBJECT_ID('SDetails', 'U') IS NULL OR OBJECT_ID('SMaster', 'U') IS NULL
    SELECT TOP 0 CAST('' AS nvarchar(120)) AS Name, CAST(0 AS decimal(18,2)) AS Amount, CAST(0 AS int) AS Count
ELSE
    SELECT
        'Uncategorised' AS Name,
        ISNULL(SUM(ISNULL(sd.TotalAmount, 0)), 0) AS Amount,
        COUNT(DISTINCT sd.ItemId) AS Count
    FROM SDetails sd
    INNER JOIN SMaster sm ON sm.BillNo = sd.BillNo AND sm.BranchId = sd.BranchId AND sm.CompanyId = sd.CompanyId AND sm.FinYearId = sd.FinYearId
    WHERE sm.BranchId = @BranchId AND sm.CompanyId = @CompanyId AND sm.FinYearId = @FinYearId
      AND ISNULL(sm.CancelFlag, 0) = 0
      AND sm.BillDate >= @FromDate AND sm.BillDate < @ToDate
    ORDER BY Amount DESC;";
                }
                else
                {
                    sql = $@"
IF OBJECT_ID('SDetails', 'U') IS NULL OR OBJECT_ID('SMaster', 'U') IS NULL OR OBJECT_ID('ItemMaster', 'U') IS NULL OR OBJECT_ID('Category', 'U') IS NULL
    SELECT TOP 0 CAST('' AS nvarchar(120)) AS Name, CAST(0 AS decimal(18,2)) AS Amount, CAST(0 AS int) AS Count
ELSE
    SELECT
        ISNULL(NULLIF(c.CategoryName, ''), 'Uncategorised') AS Name,
        ISNULL(SUM(ISNULL(sd.TotalAmount, 0)), 0) AS Amount,
        COUNT(DISTINCT sd.ItemId) AS Count
    FROM SDetails sd
    INNER JOIN SMaster sm ON sm.BillNo = sd.BillNo AND sm.BranchId = sd.BranchId AND sm.CompanyId = sd.CompanyId AND sm.FinYearId = sd.FinYearId
    LEFT JOIN ItemMaster im ON im.ItemId = sd.ItemId
    LEFT JOIN Category c ON c.[{categoryKeyColumn}] = im.[{itemCategoryColumn}]
    WHERE sm.BranchId = @BranchId AND sm.CompanyId = @CompanyId AND sm.FinYearId = @FinYearId
      AND ISNULL(sm.CancelFlag, 0) = 0
      AND sm.BillDate >= @FromDate AND sm.BillDate < @ToDate
    GROUP BY c.CategoryName
    ORDER BY Amount DESC;";
                }
            }
            else
            {
                sql = @"
IF OBJECT_ID('SMaster', 'U') IS NULL
    SELECT TOP 0 CAST('' AS nvarchar(120)) AS Name, CAST(0 AS decimal(18,2)) AS Amount, CAST(0 AS int) AS Count
ELSE
    SELECT TOP 8
        ISNULL(NULLIF(CustomerName, ''), 'Walk-in Customer') AS Name,
        ISNULL(SUM(ISNULL(NetAmount, 0)), 0) AS Amount,
        COUNT(1) AS Count
    FROM SMaster
    WHERE BranchId = @BranchId AND CompanyId = @CompanyId AND FinYearId = @FinYearId
      AND ISNULL(CancelFlag, 0) = 0
      AND BillDate >= @FromDate AND BillDate < @ToDate
    GROUP BY CustomerName
    ORDER BY Amount DESC;";
            }

            List<SalesBreakdown> result = DataConnection.Query<SalesBreakdown>(sql, BuildParameters(fromDate, toDate)).ToList();
            return kind == SalesBreakdownKind.Payment ? NormalizePaymentBreakdown(result) : result;
        }

        private List<SalesItemCategoryDetail> ReadItemCategoryDetails(DateTime fromDate, DateTime toDate)
        {
            string itemCategoryColumn = GetFirstExistingColumn("ItemMaster", "CategoryId", "CategoryID");
            string categoryKeyColumn = GetFirstExistingColumn("Category", "Id", "CategoryId", "CategoryID");
            string categoryJoin = !string.IsNullOrEmpty(itemCategoryColumn) && !string.IsNullOrEmpty(categoryKeyColumn)
                ? $"LEFT JOIN Category c ON c.[{categoryKeyColumn}] = im.[{itemCategoryColumn}]"
                : string.Empty;
            string categoryExpression = categoryJoin.Length > 0
                ? "ISNULL(NULLIF(c.CategoryName, ''), 'Uncategorised')"
                : "'Uncategorised'";

            string sql = $@"
IF OBJECT_ID('SDetails', 'U') IS NULL OR OBJECT_ID('SMaster', 'U') IS NULL
    SELECT TOP 0 CAST('' AS nvarchar(200)) AS ItemName, CAST('' AS nvarchar(120)) AS Category,
        CAST(0 AS decimal(18,2)) AS Qty, CAST(0 AS decimal(18,2)) AS Amount
ELSE
    SELECT
        ISNULL(NULLIF(sd.ItemName, ''), 'Unknown Item') AS ItemName,
        {categoryExpression} AS Category,
        ISNULL(SUM(ISNULL(sd.Qty, 0)), 0) AS Qty,
        ISNULL(SUM(ISNULL(sd.TotalAmount, 0)), 0) AS Amount
    FROM SDetails sd
    INNER JOIN SMaster sm ON sm.BillNo = sd.BillNo AND sm.BranchId = sd.BranchId
        AND sm.CompanyId = sd.CompanyId AND sm.FinYearId = sd.FinYearId
    LEFT JOIN ItemMaster im ON im.ItemId = sd.ItemId
    {categoryJoin}
    WHERE sm.BranchId = @BranchId AND sm.CompanyId = @CompanyId AND sm.FinYearId = @FinYearId
      AND ISNULL(sm.CancelFlag, 0) = 0
      AND sm.BillDate >= @FromDate AND sm.BillDate < @ToDate
    GROUP BY sd.ItemName, {categoryExpression}
    ORDER BY Category, ItemName;";

            return DataConnection.Query<SalesItemCategoryDetail>(sql, BuildParameters(fromDate, toDate)).ToList();
        }

        private string BuildPaymentBreakdownSql()
        {
            if (!TableExists("SMaster"))
                return "SELECT TOP 0 CAST('' AS nvarchar(120)) AS Name, CAST(0 AS decimal(18,2)) AS Amount, CAST(0 AS int) AS Count;";

            bool hasPaymentDetails = TableExists("SPaymentDetails");
            bool hasPayMode = TableExists("PayMode");
            string payModeIdColumn = hasPayMode ? GetFirstExistingColumn("PayMode", "PayModeID", "PaymodeId", "PayModeId") : null;
            string payModeNameColumn = hasPayMode ? GetFirstExistingColumn("PayMode", "PayModeName", "PaymodeName") : null;

            if (hasPaymentDetails)
            {
                string detailAmountColumn = GetFirstExistingColumn("SPaymentDetails", "Amount", "PaidAmount", "ReceivedAmount");
                string detailNameColumn = GetFirstExistingColumn("SPaymentDetails", "PaymodeName", "PayModeName", "PayMode");
                string detailIdColumn = GetFirstExistingColumn("SPaymentDetails", "PaymodeId", "PayModeId", "PayModeID");

                if (!string.IsNullOrEmpty(detailAmountColumn))
                {
                    string joinPayMode = !string.IsNullOrEmpty(detailIdColumn) && !string.IsNullOrEmpty(payModeIdColumn)
                        ? $"    LEFT JOIN PayMode pm ON pm.[{payModeIdColumn}] = sp.[{detailIdColumn}]\r\n"
                        : string.Empty;
                    string nameExpression = BuildPaymentNameExpression("sp", detailNameColumn, joinPayMode.Length > 0 ? "pm" : null, payModeNameColumn);

                    return $@"
SELECT
    {nameExpression} AS Name,
    ISNULL(SUM(ISNULL(sp.[{detailAmountColumn}], 0)), 0) AS Amount,
    COUNT(DISTINCT sp.BillNo) AS Count
FROM SPaymentDetails sp
INNER JOIN SMaster sm ON sm.BillNo = sp.BillNo AND sm.BranchId = sp.BranchId AND sm.CompanyId = sp.CompanyId AND sm.FinYearId = sp.FinYearId
{joinPayMode}WHERE sm.BranchId = @BranchId AND sm.CompanyId = @CompanyId AND sm.FinYearId = @FinYearId
  AND ISNULL(sm.CancelFlag, 0) = 0
  AND sm.BillDate >= @FromDate AND sm.BillDate < @ToDate
GROUP BY {nameExpression}
ORDER BY Amount DESC;";
                }
            }

            string masterNameColumn = GetFirstExistingColumn("SMaster", "PaymodeName", "PayModeName", "PayMode");
            string masterIdColumn = GetFirstExistingColumn("SMaster", "PaymodeId", "PayModeId", "PayModeID");
            string masterAmountColumn = GetFirstExistingColumn("SMaster", "NetAmount", "ReceivedAmount");
            string masterJoinPayMode = !string.IsNullOrEmpty(masterIdColumn) && !string.IsNullOrEmpty(payModeIdColumn)
                ? $"LEFT JOIN PayMode pm ON pm.[{payModeIdColumn}] = sm.[{masterIdColumn}]"
                : string.Empty;
            string masterNameExpression = BuildPaymentNameExpression("sm", masterNameColumn, masterJoinPayMode.Length > 0 ? "pm" : null, payModeNameColumn);
            string amountExpression = string.IsNullOrEmpty(masterAmountColumn) ? "0" : $"ISNULL(sm.[{masterAmountColumn}], 0)";

            return $@"
SELECT
    {masterNameExpression} AS Name,
    ISNULL(SUM({amountExpression}), 0) AS Amount,
    COUNT(1) AS Count
FROM SMaster sm
{masterJoinPayMode}
WHERE sm.BranchId = @BranchId AND sm.CompanyId = @CompanyId AND sm.FinYearId = @FinYearId
  AND ISNULL(sm.CancelFlag, 0) = 0
  AND sm.BillDate >= @FromDate AND sm.BillDate < @ToDate
GROUP BY {masterNameExpression}
ORDER BY Amount DESC;";
        }

        private string BuildPaymentNameExpression(string primaryAlias, string primaryNameColumn, string payModeAlias, string payModeNameColumn)
        {
            string fallback = "'Other'";

            if (!string.IsNullOrEmpty(payModeAlias) && !string.IsNullOrEmpty(payModeNameColumn))
                fallback = $"ISNULL(NULLIF({payModeAlias}.[{payModeNameColumn}], ''), 'Other')";

            if (!string.IsNullOrEmpty(primaryNameColumn))
                return $"ISNULL(NULLIF({primaryAlias}.[{primaryNameColumn}], ''), {fallback})";

            return fallback;
        }

        private List<SalesBreakdown> NormalizePaymentBreakdown(List<SalesBreakdown> rows)
        {
            string[] standardNames = { "Cash", "UPI", "Card", "Bank Transfer", "Other" };
            Dictionary<string, SalesBreakdown> buckets = standardNames.ToDictionary(
                name => name,
                name => new SalesBreakdown { Name = name, Amount = 0, Count = 0 },
                StringComparer.OrdinalIgnoreCase);

            foreach (SalesBreakdown row in rows ?? new List<SalesBreakdown>())
            {
                string bucketName = GetPaymentBucketName(row.Name);
                if (!buckets.ContainsKey(bucketName))
                    buckets[bucketName] = new SalesBreakdown { Name = bucketName, Amount = 0, Count = 0 };

                buckets[bucketName].Amount += row.Amount;
                buckets[bucketName].Count += row.Count;
            }

            return standardNames
                .Select(name => buckets[name])
                .Concat(buckets.Values.Where(x => !standardNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase)).OrderByDescending(x => x.Amount))
                .ToList();
        }

        private string GetPaymentBucketName(string rawName)
        {
            string name = (rawName ?? string.Empty).Trim();
            string compact = name.Replace(" ", string.Empty).Replace("-", string.Empty).ToUpperInvariant();

            if (compact.Contains("UPI") || compact.Contains("GPAY") || compact.Contains("PHONEPE") || compact.Contains("PAYTM"))
                return "UPI";
            if (compact.Contains("CARD") || compact.Contains("VISA") || compact.Contains("MASTER"))
                return "Card";
            if (compact.Contains("BANK") || compact.Contains("TRANSFER") || compact.Contains("NEFT") || compact.Contains("RTGS") || compact.Contains("IMPS"))
                return "Bank Transfer";
            if (compact.Contains("CASH"))
                return "Cash";

            return "Other";
        }

        private string GetFirstExistingColumn(string tableName, params string[] columnNames)
        {
            const string sql = @"
SELECT TOP 1 c.name
FROM sys.columns c
INNER JOIN sys.objects o ON o.object_id = c.object_id
WHERE o.object_id = OBJECT_ID(@TableName, 'U')
  AND c.name IN @ColumnNames
ORDER BY CASE c.name
    WHEN @PreferredColumn THEN 0
    ELSE 1
END;";

            return DataConnection.QueryFirstOrDefault<string>(
                sql,
                new { TableName = tableName, ColumnNames = columnNames, PreferredColumn = columnNames.FirstOrDefault() }
            );
        }

        private bool TableExists(string tableName)
        {
            const string sql = "SELECT CASE WHEN OBJECT_ID(@TableName, 'U') IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END;";
            return DataConnection.QueryFirstOrDefault<bool>(sql, new { TableName = tableName });
        }

        private DynamicParameters BuildParameters(DateTime fromDate, DateTime toDate)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@BranchId", SessionContext.BranchId);
            parameters.Add("@CompanyId", SessionContext.CompanyId);
            parameters.Add("@FinYearId", SessionContext.FinYearId);
            parameters.Add("@FromDate", fromDate);
            parameters.Add("@ToDate", toDate);
            return parameters;
        }
    }

    public class SalesAnalyticsOverview
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime PreviousFromDate { get; set; }
        public DateTime PreviousToDate { get; set; }
        public SalesAnalyticsSummary Summary { get; set; } = new SalesAnalyticsSummary();
        public List<SalesTrendPoint> SalesTrend { get; set; } = new List<SalesTrendPoint>();
        public List<SalesItemMetric> TopByQuantity { get; set; } = new List<SalesItemMetric>();
        public List<SalesItemMetric> TopByAmount { get; set; } = new List<SalesItemMetric>();
        public List<SalesItemMetric> ItemSales { get; set; } = new List<SalesItemMetric>();
        public List<SalesBreakdown> PaymentMethods { get; set; } = new List<SalesBreakdown>();
        public List<SalesBreakdown> Categories { get; set; } = new List<SalesBreakdown>();
        public List<SalesItemCategoryDetail> ItemCategories { get; set; } = new List<SalesItemCategoryDetail>();
        public List<SalesBreakdown> Customers { get; set; } = new List<SalesBreakdown>();
    }

    public class SalesItemCategoryDetail
    {
        public string ItemName { get; set; }
        public string Category { get; set; }
        public decimal Qty { get; set; }
        public decimal Amount { get; set; }
    }

    public class SalesAnalyticsSummary
    {
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal TotalItemsSold { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public decimal ProfitMarginPercent { get; set; }
        public decimal SalesChangePercent { get; set; }
        public decimal OrdersChangePercent { get; set; }
        public decimal AverageOrderValueChangePercent { get; set; }
        public decimal ProfitChangePercent { get; set; }
        public decimal ItemsSoldChangePercent { get; set; }
    }

    public class SalesTrendPoint
    {
        public DateTime SaleDate { get; set; }
        public string Caption { get; set; }
        public decimal Amount { get; set; }
    }

    public class SalesItemMetric
    {
        public string ItemName { get; set; }
        public decimal QtySold { get; set; }
        public decimal Amount { get; set; }
        public decimal Profit { get; set; }
    }

    public class SalesBreakdown
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    internal enum SalesBreakdownKind
    {
        Payment,
        Category,
        Customer
    }
}
