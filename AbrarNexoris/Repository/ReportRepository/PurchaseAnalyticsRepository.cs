using Dapper;
using ModelClass;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Repository.ReportRepository
{
    public class PurchaseAnalyticsRepository : BaseRepostitory
    {
        public PurchaseAnalyticsOverview GetAnalytics(DateTime fromDate, DateTime toDate)
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

            PurchaseAnalyticsOverview overview = new PurchaseAnalyticsOverview
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
                PurchaseAnalyticsSummary previous = ReadSummary(previousFrom, rangeFrom);
                ApplyChangePercentages(overview.Summary, previous);
                overview.PurchaseTrend = ReadTrend(rangeFrom, exclusiveTo);
                overview.TopByQuantity = ReadTopItems(rangeFrom, exclusiveTo, "Qty");
                overview.TopByAmount = ReadTopItems(rangeFrom, exclusiveTo, "Amount");
                overview.PaymentMethods = ReadPaymentBreakdown(rangeFrom, exclusiveTo);
                overview.Categories = ReadCategoryBreakdown(rangeFrom, exclusiveTo);
                overview.TopVendors = ReadTopVendors(rangeFrom, exclusiveTo);
                overview.Brief = ReadBrief(rangeFrom, exclusiveTo);
                overview.ItemPurchases = ReadItemPurchasesMap(rangeFrom, exclusiveTo);
                overview.ItemPurchaseDetails = ReadItemPurchaseDetails(rangeFrom, exclusiveTo);
            }
            finally
            {
                if (DataConnection != null && DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return overview;
        }

        private PurchaseAnalyticsSummary ReadSummary(DateTime fromDate, DateTime toDate)
        {
            string sql = @"
IF OBJECT_ID('PMaster', 'U') IS NULL
    SELECT CAST(0 AS decimal(18,2)) AS TotalPurchase, CAST(0 AS int) AS TotalOrders,
           CAST(0 AS int) AS TotalVendors, CAST(0 AS decimal(18,2)) AS TotalTax, CAST(0 AS decimal(18,2)) AS TotalDiscount
ELSE
    SELECT
        ISNULL(SUM(ISNULL(GrandTotal, 0)), 0) AS TotalPurchase,
        COUNT(1) AS TotalOrders,
        COUNT(DISTINCT NULLIF(VendorName, '')) AS TotalVendors,
        ISNULL(SUM(ISNULL(TaxAmt, 0)), 0) AS TotalTax,
        ISNULL(SUM(ISNULL(BillDiscountAmt, 0)), 0) AS TotalDiscount
    FROM PMaster
    WHERE BranchID = @BranchId AND CompanyId = @CompanyId AND FinYearId = @FinYearId
      AND ISNULL(CancelFlag, 0) = 0
      AND PurchaseDate >= @FromDate AND PurchaseDate < @ToDate;";

            PurchaseAnalyticsSummary summary = DataConnection.QueryFirstOrDefault<PurchaseAnalyticsSummary>(sql, BuildParameters(fromDate, toDate)) ?? new PurchaseAnalyticsSummary();
            summary.TotalItemsPurchased = ReadTotalItemsPurchased(fromDate, toDate);
            summary.AveragePurchaseValue = summary.TotalOrders > 0 ? summary.TotalPurchase / summary.TotalOrders : 0;
            return summary;
        }

        private decimal ReadTotalItemsPurchased(DateTime fromDate, DateTime toDate)
        {
            string sql = @"
IF OBJECT_ID('PDetails', 'U') IS NULL OR OBJECT_ID('PMaster', 'U') IS NULL
    SELECT CAST(0 AS decimal(18,2))
ELSE
    SELECT ISNULL(SUM(ISNULL(pd.Qty, 0) * ISNULL(NULLIF(pd.Packing, 0), 1)), 0)
    FROM PDetails pd
    INNER JOIN PMaster pm ON pm.PurchaseNo = pd.PurchaseNo AND pm.FinYearId = pd.FinYearId AND pm.BranchID = pd.BranchID AND pm.CompanyId = pd.CompanyId
    WHERE pm.BranchID = @BranchId AND pm.CompanyId = @CompanyId AND pm.FinYearId = @FinYearId
      AND ISNULL(pm.CancelFlag, 0) = 0
      AND pm.PurchaseDate >= @FromDate AND pm.PurchaseDate < @ToDate;";

            return DataConnection.QueryFirstOrDefault<decimal>(sql, BuildParameters(fromDate, toDate));
        }

        private void ApplyChangePercentages(PurchaseAnalyticsSummary current, PurchaseAnalyticsSummary previous)
        {
            current.PurchaseChangePercent = PercentChange(current.TotalPurchase, previous.TotalPurchase);
            current.VendorsChangePercent = PercentChange(current.TotalVendors, previous.TotalVendors);
            current.AveragePurchaseValueChangePercent = PercentChange(current.AveragePurchaseValue, previous.AveragePurchaseValue);
            current.ItemsPurchasedChangePercent = PercentChange(current.TotalItemsPurchased, previous.TotalItemsPurchased);
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

        private List<PurchaseTrendPoint> ReadTrend(DateTime fromDate, DateTime toDate)
        {
            List<PurchaseTrendPoint> trend = CreateTrendSkeleton(fromDate, toDate.AddDays(-1));
            string sql = @"
IF OBJECT_ID('PMaster', 'U') IS NULL
    SELECT CAST(NULL AS date) AS PurchaseDate, CAST(0 AS decimal(18,2)) AS Amount WHERE 1 = 0
ELSE
    SELECT CAST(PurchaseDate AS date) AS PurchaseDate, ISNULL(SUM(ISNULL(GrandTotal, 0)), 0) AS Amount
    FROM PMaster
    WHERE BranchID = @BranchId AND CompanyId = @CompanyId AND FinYearId = @FinYearId
      AND ISNULL(CancelFlag, 0) = 0
      AND PurchaseDate >= @FromDate AND PurchaseDate < @ToDate
    GROUP BY CAST(PurchaseDate AS date);";

            foreach (PurchaseTrendPoint row in DataConnection.Query<PurchaseTrendPoint>(sql, BuildParameters(fromDate, toDate)))
            {
                PurchaseTrendPoint point = trend.FirstOrDefault(x => x.PurchaseDate == row.PurchaseDate.Date);
                if (point != null)
                    point.Amount = row.Amount;
            }

            return trend;
        }

        private List<PurchaseTrendPoint> CreateTrendSkeleton(DateTime fromDate, DateTime toDate)
        {
            List<PurchaseTrendPoint> trend = new List<PurchaseTrendPoint>();
            DateTime cursor = fromDate.Date;
            while (cursor <= toDate.Date)
            {
                trend.Add(new PurchaseTrendPoint { PurchaseDate = cursor, Caption = cursor.ToString("dd MMM"), Amount = 0 });
                cursor = cursor.AddDays(1);
            }
            return trend;
        }

        private List<PurchaseItemMetric> ReadTopItems(DateTime fromDate, DateTime toDate, string orderBy)
        {
            string sortColumn = orderBy == "Qty" ? "QtyPurchased" : "Amount";
            string sql = @"
IF OBJECT_ID('PDetails', 'U') IS NULL OR OBJECT_ID('PMaster', 'U') IS NULL
    SELECT TOP 0 CAST('' AS nvarchar(200)) AS ItemName, CAST(0 AS decimal(18,2)) AS QtyPurchased, CAST(0 AS decimal(18,2)) AS Amount
ELSE
    SELECT TOP 10
        ISNULL(NULLIF(pd.ItemName, ''), 'Unknown Item') AS ItemName,
        ISNULL(SUM(ISNULL(pd.Qty, 0) * ISNULL(NULLIF(pd.Packing, 0), 1)), 0) AS QtyPurchased,
        ISNULL(SUM(ISNULL(pd.Cost, 0) * ISNULL(NULLIF(pd.Packing, 0), 1) * ISNULL(pd.Qty, 0)), 0) AS Amount
    FROM PDetails pd
    INNER JOIN PMaster pm ON pm.PurchaseNo = pd.PurchaseNo AND pm.FinYearId = pd.FinYearId AND pm.BranchID = pd.BranchID AND pm.CompanyId = pd.CompanyId
    WHERE pm.BranchID = @BranchId AND pm.CompanyId = @CompanyId AND pm.FinYearId = @FinYearId
      AND ISNULL(pm.CancelFlag, 0) = 0
      AND pm.PurchaseDate >= @FromDate AND pm.PurchaseDate < @ToDate
    GROUP BY pd.ItemName
    ORDER BY " + sortColumn + @" DESC;";

            return DataConnection.Query<PurchaseItemMetric>(sql, BuildParameters(fromDate, toDate)).ToList();
        }

        private List<PurchaseItemMetric> ReadItemPurchasesMap(DateTime fromDate, DateTime toDate)
        {
            string sql = @"
IF OBJECT_ID('PDetails', 'U') IS NULL OR OBJECT_ID('PMaster', 'U') IS NULL
    SELECT TOP 0 CAST('' AS nvarchar(200)) AS ItemName, CAST(0 AS decimal(18,2)) AS QtyPurchased, CAST(0 AS decimal(18,2)) AS Amount
ELSE
    SELECT
        ISNULL(NULLIF(pd.ItemName, ''), 'Unknown Item') AS ItemName,
        ISNULL(SUM(ISNULL(pd.Qty, 0) * ISNULL(NULLIF(pd.Packing, 0), 1)), 0) AS QtyPurchased,
        ISNULL(SUM(ISNULL(pd.Cost, 0) * ISNULL(NULLIF(pd.Packing, 0), 1) * ISNULL(pd.Qty, 0)), 0) AS Amount
    FROM PDetails pd
    INNER JOIN PMaster pm ON pm.PurchaseNo = pd.PurchaseNo AND pm.FinYearId = pd.FinYearId AND pm.BranchID = pd.BranchID AND pm.CompanyId = pd.CompanyId
    WHERE pm.BranchID = @BranchId AND pm.CompanyId = @CompanyId AND pm.FinYearId = @FinYearId
      AND ISNULL(pm.CancelFlag, 0) = 0
      AND pm.PurchaseDate >= @FromDate AND pm.PurchaseDate < @ToDate
    GROUP BY pd.ItemName
    ORDER BY Amount DESC;";

            return DataConnection.Query<PurchaseItemMetric>(sql, BuildParameters(fromDate, toDate)).ToList();
        }

        private List<PurchaseItemDetail> ReadItemPurchaseDetails(DateTime fromDate, DateTime toDate)
        {
            const string sql = @"
IF OBJECT_ID('PDetails', 'U') IS NULL OR OBJECT_ID('PMaster', 'U') IS NULL
    SELECT TOP 0 CAST(0 AS bigint) AS PurchaseNo, CAST(NULL AS datetime) AS PurchaseDate,
        CAST('' AS nvarchar(200)) AS ItemName, CAST('' AS nvarchar(200)) AS Vendor,
        CAST(0 AS decimal(18,2)) AS Qty, CAST(0 AS decimal(18,2)) AS Cost, CAST(0 AS decimal(18,2)) AS Amount
ELSE
    SELECT
        pd.PurchaseNo,
        pm.PurchaseDate,
        ISNULL(NULLIF(pd.ItemName, ''), 'Unknown Item') AS ItemName,
        ISNULL(NULLIF(pm.VendorName, ''), 'Unknown Vendor') AS Vendor,
        ISNULL(pd.Qty, 0) * ISNULL(NULLIF(pd.Packing, 0), 1) AS Qty,
        ISNULL(pd.Cost, 0) AS Cost,
        ISNULL(pd.Cost, 0) * ISNULL(NULLIF(pd.Packing, 0), 1) * ISNULL(pd.Qty, 0) AS Amount
    FROM PDetails pd
    INNER JOIN PMaster pm ON pm.PurchaseNo = pd.PurchaseNo AND pm.FinYearId = pd.FinYearId
        AND pm.BranchID = pd.BranchID AND pm.CompanyId = pd.CompanyId
    WHERE pm.BranchID = @BranchId AND pm.CompanyId = @CompanyId AND pm.FinYearId = @FinYearId
      AND ISNULL(pm.CancelFlag, 0) = 0
      AND pm.PurchaseDate >= @FromDate AND pm.PurchaseDate < @ToDate
    ORDER BY pm.PurchaseDate DESC, pd.PurchaseNo DESC;";

            return DataConnection.Query<PurchaseItemDetail>(sql, BuildParameters(fromDate, toDate)).ToList();
        }

        private List<PurchaseBreakdown> ReadPaymentBreakdown(DateTime fromDate, DateTime toDate)
        {
            if (!TableExists("PMaster"))
                return new List<PurchaseBreakdown>();

            string payModeNameColumn = TableExists("PayMode") ? GetFirstExistingColumn("PayMode", "PayModeName", "PaymodeName") : null;
            string payModeIdColumn = TableExists("PayMode") ? GetFirstExistingColumn("PayMode", "PayModeID", "PaymodeId", "PayModeId") : null;
            string masterPayModeColumn = GetFirstExistingColumn("PMaster", "Paymode", "PayMode", "PaymodeName", "PayModeName");
            string masterPayModeIdColumn = GetFirstExistingColumn("PMaster", "PaymodeID", "PayModeId", "PayModeID");
            string joinPayMode = !string.IsNullOrEmpty(payModeIdColumn) && !string.IsNullOrEmpty(masterPayModeIdColumn)
                ? $"LEFT JOIN PayMode pay ON pay.[{payModeIdColumn}] = pm.[{masterPayModeIdColumn}]\r\n"
                : string.Empty;
            string nameExpression = BuildPaymentNameExpression("pm", masterPayModeColumn, joinPayMode.Length > 0 ? "pay" : null, payModeNameColumn);

            string sql = $@"
SELECT
    {nameExpression} AS Name,
    ISNULL(SUM(ISNULL(pm.GrandTotal, 0)), 0) AS Amount,
    COUNT(1) AS Count
FROM PMaster pm
{joinPayMode}WHERE pm.BranchID = @BranchId AND pm.CompanyId = @CompanyId AND pm.FinYearId = @FinYearId
  AND ISNULL(pm.CancelFlag, 0) = 0
  AND pm.PurchaseDate >= @FromDate AND pm.PurchaseDate < @ToDate
GROUP BY {nameExpression}
HAVING LOWER(LTRIM(RTRIM({nameExpression}))) IN ('cash', 'credit')
ORDER BY Amount DESC;";

            return NormalizePaymentBreakdown(DataConnection.Query<PurchaseBreakdown>(sql, BuildParameters(fromDate, toDate)).ToList());
        }

        private List<PurchaseBreakdown> ReadCategoryBreakdown(DateTime fromDate, DateTime toDate)
        {
            string itemCategoryColumn = GetFirstExistingColumn("ItemMaster", "CategoryId", "CategoryID");
            string categoryKeyColumn = GetFirstExistingColumn("Category", "Id", "CategoryId", "CategoryID");
            string sql;

            if (string.IsNullOrEmpty(itemCategoryColumn) || string.IsNullOrEmpty(categoryKeyColumn))
            {
                sql = @"
IF OBJECT_ID('PDetails', 'U') IS NULL OR OBJECT_ID('PMaster', 'U') IS NULL
    SELECT TOP 0 CAST('' AS nvarchar(120)) AS Name, CAST(0 AS decimal(18,2)) AS Amount, CAST(0 AS int) AS Count
ELSE
    SELECT
        'Uncategorised' AS Name,
        ISNULL(SUM(ISNULL(pd.Cost, 0) * ISNULL(NULLIF(pd.Packing, 0), 1) * ISNULL(pd.Qty, 0)), 0) AS Amount,
        COUNT(DISTINCT pd.ItemID) AS Count
    FROM PDetails pd
    INNER JOIN PMaster pm ON pm.PurchaseNo = pd.PurchaseNo AND pm.FinYearId = pd.FinYearId AND pm.BranchID = pd.BranchID AND pm.CompanyId = pd.CompanyId
    WHERE pm.BranchID = @BranchId AND pm.CompanyId = @CompanyId AND pm.FinYearId = @FinYearId
      AND ISNULL(pm.CancelFlag, 0) = 0
      AND pm.PurchaseDate >= @FromDate AND pm.PurchaseDate < @ToDate
    ORDER BY Amount DESC;";
            }
            else
            {
                sql = $@"
IF OBJECT_ID('PDetails', 'U') IS NULL OR OBJECT_ID('PMaster', 'U') IS NULL OR OBJECT_ID('ItemMaster', 'U') IS NULL OR OBJECT_ID('Category', 'U') IS NULL
    SELECT TOP 0 CAST('' AS nvarchar(120)) AS Name, CAST(0 AS decimal(18,2)) AS Amount, CAST(0 AS int) AS Count
ELSE
    SELECT
        ISNULL(NULLIF(c.CategoryName, ''), 'Uncategorised') AS Name,
        ISNULL(SUM(ISNULL(pd.Cost, 0) * ISNULL(NULLIF(pd.Packing, 0), 1) * ISNULL(pd.Qty, 0)), 0) AS Amount,
        COUNT(DISTINCT pd.ItemID) AS Count
    FROM PDetails pd
    INNER JOIN PMaster pm ON pm.PurchaseNo = pd.PurchaseNo AND pm.FinYearId = pd.FinYearId AND pm.BranchID = pd.BranchID AND pm.CompanyId = pd.CompanyId
    LEFT JOIN ItemMaster im ON im.ItemId = pd.ItemID
    LEFT JOIN Category c ON c.[{categoryKeyColumn}] = im.[{itemCategoryColumn}]
    WHERE pm.BranchID = @BranchId AND pm.CompanyId = @CompanyId AND pm.FinYearId = @FinYearId
      AND ISNULL(pm.CancelFlag, 0) = 0
      AND pm.PurchaseDate >= @FromDate AND pm.PurchaseDate < @ToDate
    GROUP BY c.CategoryName
    ORDER BY Amount DESC;";
            }

            return DataConnection.Query<PurchaseBreakdown>(sql, BuildParameters(fromDate, toDate)).ToList();
        }

        private List<PurchaseVendorMetric> ReadTopVendors(DateTime fromDate, DateTime toDate)
        {
            string sql = @"
IF OBJECT_ID('PMaster', 'U') IS NULL
    SELECT TOP 0 CAST('' AS nvarchar(200)) AS VendorName, CAST(0 AS decimal(18,2)) AS Amount
ELSE
    SELECT TOP 10
        ISNULL(NULLIF(VendorName, ''), 'Unknown Vendor') AS VendorName,
        ISNULL(SUM(ISNULL(GrandTotal, 0)), 0) AS Amount
    FROM PMaster
    WHERE BranchID = @BranchId AND CompanyId = @CompanyId AND FinYearId = @FinYearId
      AND ISNULL(CancelFlag, 0) = 0
      AND PurchaseDate >= @FromDate AND PurchaseDate < @ToDate
    GROUP BY VendorName
    ORDER BY Amount DESC;";

            return DataConnection.Query<PurchaseVendorMetric>(sql, BuildParameters(fromDate, toDate)).ToList();
        }

        private PurchaseBriefSummary ReadBrief(DateTime fromDate, DateTime toDate)
        {
            PurchaseBriefSummary brief = new PurchaseBriefSummary
            {
                TotalPurchase = ReadSummary(fromDate, toDate).TotalPurchase,
                TotalItemsPurchased = ReadTotalItemsPurchased(fromDate, toDate),
                PurchaseReturn = ReadPurchaseReturn(fromDate, toDate),
                LowStockItems = ReadLowStockItems(),
                OutOfStockItems = ReadSafeInt("IF OBJECT_ID('PriceSettings', 'U') IS NULL SELECT 0 ELSE SELECT COUNT(1) FROM PriceSettings WHERE BranchId = @BranchId AND ISNULL(Stock, 0) <= 0;")
            };
            brief.NetPurchase = brief.TotalPurchase - brief.PurchaseReturn;
            return brief;
        }

        private decimal ReadPurchaseReturn(DateTime fromDate, DateTime toDate)
        {
            string sql = @"
IF OBJECT_ID('PReturnMaster', 'U') IS NULL
    SELECT CAST(0 AS decimal(18,2))
ELSE
    SELECT ISNULL(SUM(ISNULL(GrandTotal, 0)), 0)
    FROM PReturnMaster
    WHERE BranchID = @BranchId AND CompanyId = @CompanyId AND FinYearId = @FinYearId
      AND ISNULL(CancelFlag, 0) = 0
      AND PReturnDate >= @FromDate AND PReturnDate < @ToDate;";

            return DataConnection.QueryFirstOrDefault<decimal>(sql, BuildParameters(fromDate, toDate));
        }

        private int ReadLowStockItems()
        {
            if (!TableExists("PriceSettings"))
                return 0;

            string reorderColumn = GetFirstExistingColumn("PriceSettings", "ReOrder", "MinStock", "ReorderLevel", "Reorder");
            if (string.IsNullOrWhiteSpace(reorderColumn))
                return 0;

            string sql = $@"
SELECT COUNT(1)
FROM PriceSettings
WHERE BranchId = @BranchId
  AND ISNULL(Stock, 0) > 0
  AND ISNULL(Stock, 0) <= ISNULL([{reorderColumn}], 0);";

            return ReadSafeInt(sql);
        }

        private int ReadSafeInt(string sql)
        {
            return DataConnection.QueryFirstOrDefault<int>(sql, BuildParameters(DateTime.Today, DateTime.Today.AddDays(1)));
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

        private List<PurchaseBreakdown> NormalizePaymentBreakdown(List<PurchaseBreakdown> rows)
        {
            string[] standardNames = { "Cash", "Credit" };
            Dictionary<string, PurchaseBreakdown> buckets = standardNames.ToDictionary(
                name => name,
                name => new PurchaseBreakdown { Name = name, Amount = 0, Count = 0 },
                StringComparer.OrdinalIgnoreCase);

            foreach (PurchaseBreakdown row in rows ?? new List<PurchaseBreakdown>())
            {
                string bucketName = GetPaymentBucketName(row.Name);
                if (!buckets.ContainsKey(bucketName))
                    buckets[bucketName] = new PurchaseBreakdown { Name = bucketName, Amount = 0, Count = 0 };

                buckets[bucketName].Amount += row.Amount;
                buckets[bucketName].Count += row.Count;
            }

            return standardNames.Select(name => buckets[name]).ToList();
        }

        private string GetPaymentBucketName(string rawName)
        {
            string compact = (rawName ?? string.Empty).Replace(" ", string.Empty).Replace("-", string.Empty).ToUpperInvariant();

            if (compact.Contains("UPI") || compact.Contains("GPAY") || compact.Contains("PHONEPE") || compact.Contains("PAYTM"))
                return "UPI";
            if (compact.Contains("CARD") || compact.Contains("VISA") || compact.Contains("MASTER"))
                return "Card";
            if (compact.Contains("BANK") || compact.Contains("TRANSFER") || compact.Contains("NEFT") || compact.Contains("RTGS") || compact.Contains("IMPS"))
                return "Bank Transfer";
            if (compact.Contains("CASH"))
                return "Cash";
            if (compact.Contains("CREDIT"))
                return "Credit";

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

    public class PurchaseAnalyticsOverview
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime PreviousFromDate { get; set; }
        public DateTime PreviousToDate { get; set; }
        public PurchaseAnalyticsSummary Summary { get; set; } = new PurchaseAnalyticsSummary();
        public List<PurchaseTrendPoint> PurchaseTrend { get; set; } = new List<PurchaseTrendPoint>();
        public List<PurchaseItemMetric> TopByQuantity { get; set; } = new List<PurchaseItemMetric>();
        public List<PurchaseItemMetric> TopByAmount { get; set; } = new List<PurchaseItemMetric>();
        public List<PurchaseBreakdown> PaymentMethods { get; set; } = new List<PurchaseBreakdown>();
        public List<PurchaseBreakdown> Categories { get; set; } = new List<PurchaseBreakdown>();
        public List<PurchaseVendorMetric> TopVendors { get; set; } = new List<PurchaseVendorMetric>();
        public PurchaseBriefSummary Brief { get; set; } = new PurchaseBriefSummary();
        public List<PurchaseItemMetric> ItemPurchases { get; set; } = new List<PurchaseItemMetric>();
        public List<PurchaseItemDetail> ItemPurchaseDetails { get; set; } = new List<PurchaseItemDetail>();
    }

    public class PurchaseAnalyticsSummary
    {
        public decimal TotalPurchase { get; set; }
        public int TotalOrders { get; set; }
        public int TotalVendors { get; set; }
        public decimal AveragePurchaseValue { get; set; }
        public decimal TotalItemsPurchased { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal PurchaseChangePercent { get; set; }
        public decimal VendorsChangePercent { get; set; }
        public decimal AveragePurchaseValueChangePercent { get; set; }
        public decimal ItemsPurchasedChangePercent { get; set; }
    }

    public class PurchaseTrendPoint
    {
        public DateTime PurchaseDate { get; set; }
        public string Caption { get; set; }
        public decimal Amount { get; set; }
    }

    public class PurchaseItemMetric
    {
        public string ItemName { get; set; }
        public decimal QtyPurchased { get; set; }
        public decimal Amount { get; set; }
    }

    public class PurchaseItemDetail
    {
        public long PurchaseNo { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string ItemName { get; set; }
        public string Vendor { get; set; }
        public decimal Qty { get; set; }
        public decimal Cost { get; set; }
        public decimal Amount { get; set; }
    }

    public class PurchaseBreakdown
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    public class PurchaseVendorMetric
    {
        public string VendorName { get; set; }
        public decimal Amount { get; set; }
    }

    public class PurchaseBriefSummary
    {
        public decimal TotalPurchase { get; set; }
        public decimal TotalItemsPurchased { get; set; }
        public decimal PurchaseReturn { get; set; }
        public decimal NetPurchase { get; set; }
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }
    }
}
