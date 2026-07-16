using Dapper;
using ModelClass;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Repository.ReportRepository
{
    public class DashboardOverviewRepository : BaseRepostitory
    {
        public DashboardOverview GetOverview(DateTime businessDate)
        {
            return GetOverview(businessDate.Date, businessDate.Date, DashboardOverviewRangeKind.Day);
        }

        public DashboardOverview GetOverview(DateTime fromDate, DateTime toDate, DashboardOverviewRangeKind rangeKind)
        {
            DateTime rangeFromDate = fromDate.Date;
            DateTime rangeToDate = toDate.Date;
            if (rangeToDate < rangeFromDate)
            {
                DateTime swap = rangeFromDate;
                rangeFromDate = rangeToDate;
                rangeToDate = swap;
            }

            DateTime exclusiveToDate = rangeToDate.AddDays(1);
            DateTime comparisonFromDate = GetPreviousRangeStart(rangeFromDate, rangeToDate, rangeKind);

            DashboardOverview overview = new DashboardOverview
            {
                BusinessDate = rangeFromDate,
                FromDate = rangeFromDate,
                ToDate = rangeToDate,
                RangeKind = rangeKind,
                BranchName = DataBase.Branch,
                GeneratedAt = DateTime.Now
            };

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                overview.TotalSales = ReadTotal("SMaster", "BillDate", "NetAmount", "BranchId", rangeFromDate, exclusiveToDate);
                overview.TotalPurchase = ReadTotal("PMaster", "PurchaseDate", "GrandTotal", "BranchID", rangeFromDate, exclusiveToDate);
                overview.TotalSalesReturn = ReadTotal("SReturnMaster", "SReturnDate", "GrandTotal", "BranchId", rangeFromDate, exclusiveToDate);
                overview.TotalPurchaseReturn = ReadTotal("PReturnMaster", "PReturnDate", "GrandTotal", "BranchID", rangeFromDate, exclusiveToDate);
                overview.SalesCount = ReadCount("SMaster", "BillDate", "BranchId", rangeFromDate, exclusiveToDate);
                overview.PurchaseCount = ReadCount("PMaster", "PurchaseDate", "BranchID", rangeFromDate, exclusiveToDate);
                overview.SalesReturnCount = ReadCount("SReturnMaster", "SReturnDate", "BranchId", rangeFromDate, exclusiveToDate);
                overview.PurchaseReturnCount = ReadCount("PReturnMaster", "PReturnDate", "BranchID", rangeFromDate, exclusiveToDate);

                overview.YesterdaySales = ReadTotal("SMaster", "BillDate", "NetAmount", "BranchId", comparisonFromDate, rangeFromDate);
                overview.YesterdayPurchase = ReadTotal("PMaster", "PurchaseDate", "GrandTotal", "BranchID", comparisonFromDate, rangeFromDate);

                // Receipt and payment forms persist their voucher rows with these
                // application voucher types.
                overview.TotalReceipts = ReadVoucherTotal(rangeFromDate, exclusiveToDate, "CUSTRCPT", "Debit");
                overview.TotalPayments = ReadVoucherTotal(rangeFromDate, exclusiveToDate, "VENDPAY", "Credit");
                overview.ReceiptsCount = ReadVoucherCount(rangeFromDate, exclusiveToDate, "CUSTRCPT");
                overview.PaymentsCount = ReadVoucherCount(rangeFromDate, exclusiveToDate, "VENDPAY");
                overview.DueReceivables = ReadSafeDecimal(@"
IF OBJECT_ID('SMaster', 'U') IS NULL SELECT CAST(0 AS decimal(18,2))
ELSE SELECT ISNULL(SUM(ISNULL(NetAmount, 0) - ISNULL(ReceivedAmount, 0)), 0)
FROM SMaster
WHERE BranchId = @BranchId AND CompanyId = @CompanyId AND FinYearId = @FinYearId
  AND ISNULL(CancelFlag, 0) = 0 AND ISNULL(NetAmount, 0) > ISNULL(ReceivedAmount, 0);");
                overview.DuePayables = ReadSafeDecimal(@"
IF OBJECT_ID('PMaster', 'U') IS NULL SELECT CAST(0 AS decimal(18,2))
ELSE SELECT ISNULL(SUM(ISNULL(GrandTotal, 0) - ISNULL(PayedAmount, 0)), 0)
FROM PMaster
WHERE BranchID = @BranchId AND CompanyId = @CompanyId AND FinYearId = @FinYearId
  AND ISNULL(GrandTotal, 0) > ISNULL(PayedAmount, 0);");

                overview.TotalItems = ReadSafeInt(@"
IF OBJECT_ID('ItemMaster', 'U') IS NULL SELECT 0
ELSE SELECT COUNT(1) FROM ItemMaster WHERE CompanyId = @CompanyId;");
                overview.LowStockItems = ReadLowStockItems();
                overview.OutOfStockItems = ReadSafeInt("IF OBJECT_ID('PriceSettings', 'U') IS NULL SELECT 0 ELSE SELECT COUNT(1) FROM PriceSettings WHERE BranchId = @BranchId AND ISNULL(Stock, 0) <= 0;");
                overview.TotalCustomers = ReadLedgerCount(16, "Customer");
                overview.TotalVendors = ReadLedgerCount(17, "Vendor");
                overview.Customers = ReadLedgerRows(16);
                overview.Vendors = ReadLedgerRows(17);

                overview.SalesTrend = ReadSalesTrend(rangeFromDate, exclusiveToDate, rangeKind);
                overview.TopSellingItems = ReadTopSellingItems(rangeFromDate, exclusiveToDate);
            }
            finally
            {
                if (DataConnection != null && DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return overview;
        }

        private DateTime GetPreviousRangeStart(DateTime fromDate, DateTime toDate, DashboardOverviewRangeKind rangeKind)
        {
            switch (rangeKind)
            {
                case DashboardOverviewRangeKind.Month:
                    return fromDate.AddMonths(-1);
                case DashboardOverviewRangeKind.Year:
                    return fromDate.AddYears(-1);
                default:
                    return fromDate.AddDays(-1);
            }
        }

        private decimal ReadTotal(string tableName, string dateColumn, string amountColumn, string branchColumn, DateTime fromDate, DateTime toDate)
        {
            string sql = $@"
IF OBJECT_ID('{tableName}', 'U') IS NULL SELECT CAST(0 AS decimal(18,2))
ELSE SELECT ISNULL(SUM(ISNULL({amountColumn}, 0)), 0)
FROM {tableName}
WHERE {branchColumn} = @BranchId AND CompanyId = @CompanyId AND FinYearId = @FinYearId
  AND {dateColumn} >= @FromDate AND {dateColumn} < @ToDate;";
            return ReadSafeDecimal(sql, fromDate, toDate);
        }

        private decimal ReadVoucherTotal(DateTime fromDate, DateTime toDate, string voucherType, string amountColumn)
        {
            string sql = $@"
IF OBJECT_ID('Vouchers', 'U') IS NULL SELECT CAST(0 AS decimal(18,2))
ELSE SELECT ISNULL(SUM(ISNULL({amountColumn}, 0)), 0)
FROM Vouchers
WHERE BranchID = @BranchId AND CompanyID = @CompanyId AND FinYearID = @FinYearId
  AND VoucherDate >= @FromDate AND VoucherDate < @ToDate
  AND VoucherType = @VoucherType AND ISNULL(CancelFlag, 0) = 0;";
            return ReadSafeDecimal(sql, fromDate, toDate, new { VoucherType = voucherType });
        }

        private int ReadCount(string tableName, string dateColumn, string branchColumn, DateTime fromDate, DateTime toDate)
        {
            string sql = $@"
IF OBJECT_ID('{tableName}', 'U') IS NULL SELECT 0
ELSE SELECT COUNT(1)
FROM {tableName}
WHERE {branchColumn} = @BranchId AND CompanyId = @CompanyId AND FinYearId = @FinYearId
  AND {dateColumn} >= @FromDate AND {dateColumn} < @ToDate;";
            return ReadSafeInt(sql, fromDate, toDate);
        }

        private int ReadVoucherCount(DateTime fromDate, DateTime toDate, string voucherType)
        {
            string sql = @"
IF OBJECT_ID('Vouchers', 'U') IS NULL SELECT 0
ELSE SELECT COUNT(DISTINCT VoucherID)
FROM Vouchers
WHERE BranchID = @BranchId AND CompanyID = @CompanyId AND FinYearID = @FinYearId
  AND VoucherDate >= @FromDate AND VoucherDate < @ToDate
  AND VoucherType = @VoucherType AND ISNULL(CancelFlag, 0) = 0;";
            return ReadSafeInt(sql, fromDate, toDate, new { VoucherType = voucherType });
        }

        private int ReadLedgerCount(int groupId, string fallbackTableName)
        {
            int ledgerCount = ReadSafeInt(@"
IF OBJECT_ID('LedgerMaster', 'U') IS NULL SELECT 0
ELSE SELECT COUNT(1)
FROM LedgerMaster
WHERE BranchID = @BranchId AND CompanyID = @CompanyId AND GroupID = @GroupId;", null, null, new { GroupId = groupId });

            if (ledgerCount > 0)
                return ledgerCount;

            string sql = $@"
IF OBJECT_ID('{fallbackTableName}', 'U') IS NULL SELECT 0
ELSE SELECT COUNT(1) FROM {fallbackTableName} WHERE BranchId = @BranchId;";
            return ReadSafeInt(sql);
        }

        private List<DashboardPartyRow> ReadLedgerRows(int groupId)
        {
            const string sql = @"
IF OBJECT_ID('LedgerMaster', 'U') IS NULL
    SELECT TOP 0 CAST(0 AS int) AS LedgerId, CAST('' AS nvarchar(250)) AS Name
ELSE
    SELECT
        CAST(LedgerID AS int) AS LedgerId,
        ISNULL(NULLIF(LedgerName, ''), 'Unnamed') AS Name
    FROM LedgerMaster
    WHERE BranchID = @BranchId AND CompanyID = @CompanyId AND GroupID = @GroupId
    ORDER BY LedgerName;";

            return DataConnection.Query<DashboardPartyRow>(
                sql,
                BuildParameters(null, null, new { GroupId = groupId })).ToList();
        }

        private List<DashboardTrendPoint> ReadSalesTrend(DateTime fromDate, DateTime toDate, DashboardOverviewRangeKind rangeKind)
        {
            List<DashboardTrendPoint> trend = CreateTrendSkeleton(fromDate, toDate.AddDays(-1), rangeKind);

            try
            {
                string periodExpression = GetTrendPeriodExpression(rangeKind);
                string sql = @"
IF OBJECT_ID('SMaster', 'U') IS NULL SELECT CAST(NULL AS date) AS SaleDate, CAST(0 AS decimal(18,2)) AS Amount WHERE 1 = 0
ELSE SELECT " + periodExpression + @" AS SaleDate, ISNULL(SUM(ISNULL(NetAmount, 0)), 0) AS Amount
FROM SMaster
WHERE BranchId = @BranchId AND CompanyId = @CompanyId AND FinYearId = @FinYearId
  AND BillDate >= @FromDate AND BillDate < @ToDate
GROUP BY " + periodExpression + @";";
                var rows = DataConnection.Query(sql, BuildParameters(fromDate, toDate)).ToList();
                foreach (var row in rows)
                {
                    DateTime saleDate = Convert.ToDateTime(row.SaleDate);
                    var point = trend.FirstOrDefault(x => x.PeriodStart == saleDate.Date);
                    if (point != null) point.Amount = Convert.ToDecimal(row.Amount);
                }
            }
            catch
            {
            }

            return trend;
        }

        private List<DashboardTrendPoint> CreateTrendSkeleton(DateTime fromDate, DateTime toDate, DashboardOverviewRangeKind rangeKind)
        {
            List<DashboardTrendPoint> trend = new List<DashboardTrendPoint>();
            DateTime cursor;

            switch (rangeKind)
            {
                case DashboardOverviewRangeKind.Month:
                    cursor = new DateTime(fromDate.Year, fromDate.Month, 1);
                    DateTime lastMonth = new DateTime(toDate.Year, toDate.Month, 1);
                    while (cursor <= lastMonth)
                    {
                        trend.Add(new DashboardTrendPoint { PeriodStart = cursor, Caption = cursor.ToString("MMM yyyy"), Amount = 0 });
                        cursor = cursor.AddMonths(1);
                    }
                    break;
                case DashboardOverviewRangeKind.Year:
                    cursor = new DateTime(fromDate.Year, 1, 1);
                    DateTime lastYear = new DateTime(toDate.Year, 1, 1);
                    while (cursor <= lastYear)
                    {
                        trend.Add(new DashboardTrendPoint { PeriodStart = cursor, Caption = cursor.ToString("yyyy"), Amount = 0 });
                        cursor = cursor.AddYears(1);
                    }
                    break;
                default:
                    cursor = fromDate.Date;
                    while (cursor <= toDate.Date)
                    {
                        trend.Add(new DashboardTrendPoint { PeriodStart = cursor, Caption = cursor.ToString("dd MMM"), Amount = 0 });
                        cursor = cursor.AddDays(1);
                    }
                    break;
            }

            return trend;
        }

        private string GetTrendPeriodExpression(DashboardOverviewRangeKind rangeKind)
        {
            switch (rangeKind)
            {
                case DashboardOverviewRangeKind.Month:
                    return "CONVERT(date, DATEADD(month, DATEDIFF(month, 0, BillDate), 0))";
                case DashboardOverviewRangeKind.Year:
                    return "CONVERT(date, DATEADD(year, DATEDIFF(year, 0, BillDate), 0))";
                default:
                    return "CAST(BillDate AS date)";
            }
        }

        private List<DashboardTopItem> ReadTopSellingItems(DateTime fromDate, DateTime toDate)
        {
            try
            {
                string sql = @"
IF OBJECT_ID('SDetails', 'U') IS NULL OR OBJECT_ID('SMaster', 'U') IS NULL
    SELECT TOP 0 CAST('' AS nvarchar(200)) AS ItemName, CAST(0 AS decimal(18,2)) AS Qty, CAST(0 AS decimal(18,2)) AS Amount
ELSE
    SELECT TOP 5 sd.ItemName, ISNULL(SUM(ISNULL(sd.Qty, 0)), 0) AS Qty, ISNULL(SUM(ISNULL(sd.TotalAmount, 0)), 0) AS Amount
    FROM SDetails sd
    INNER JOIN SMaster sm ON sm.BillNo = sd.BillNo AND sm.BranchId = sd.BranchId AND sm.CompanyId = sd.CompanyId AND sm.FinYearId = sd.FinYearId
    WHERE sm.BranchId = @BranchId AND sm.CompanyId = @CompanyId AND sm.FinYearId = @FinYearId
      AND sm.BillDate >= @FromDate AND sm.BillDate < @ToDate
    GROUP BY sd.ItemName
    ORDER BY Amount DESC;";

                return DataConnection.Query<DashboardTopItem>(sql, BuildParameters(fromDate, toDate)).ToList();
            }
            catch
            {
                return new List<DashboardTopItem>();
            }
        }

        private decimal ReadSafeDecimal(string sql, DateTime? fromDate = null, DateTime? toDate = null, object extra = null)
        {
            try
            {
                return DataConnection.QueryFirstOrDefault<decimal>(sql, BuildParameters(fromDate, toDate, extra));
            }
            catch
            {
                return 0;
            }
        }

        private int ReadSafeInt(string sql, DateTime? fromDate = null, DateTime? toDate = null, object extra = null)
        {
            try
            {
                return DataConnection.QueryFirstOrDefault<int>(sql, BuildParameters(fromDate, toDate, extra));
            }
            catch
            {
                return 0;
            }
        }

        private int ReadLowStockItems()
        {
            const string tableExistsSql = "SELECT CASE WHEN OBJECT_ID('PriceSettings', 'U') IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END;";
            if (!DataConnection.QueryFirstOrDefault<bool>(tableExistsSql))
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

        private DynamicParameters BuildParameters(DateTime? fromDate = null, DateTime? toDate = null, object extra = null)
        {
            DynamicParameters parameters = new DynamicParameters(extra);
            parameters.Add("@BranchId", SessionContext.BranchId);
            parameters.Add("@CompanyId", SessionContext.CompanyId);
            parameters.Add("@FinYearId", SessionContext.FinYearId);
            if (fromDate.HasValue) parameters.Add("@FromDate", fromDate.Value);
            if (toDate.HasValue) parameters.Add("@ToDate", toDate.Value);
            return parameters;
        }
    }

    public class DashboardOverview
    {
        public DateTime BusinessDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DashboardOverviewRangeKind RangeKind { get; set; }
        public string BranchName { get; set; }
        public DateTime GeneratedAt { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalPurchase { get; set; }
        public decimal TotalSalesReturn { get; set; }
        public decimal TotalPurchaseReturn { get; set; }
        public int SalesCount { get; set; }
        public int PurchaseCount { get; set; }
        public int SalesReturnCount { get; set; }
        public int PurchaseReturnCount { get; set; }
        public decimal YesterdaySales { get; set; }
        public decimal YesterdayPurchase { get; set; }
        public decimal TotalReceipts { get; set; }
        public decimal TotalPayments { get; set; }
        public int ReceiptsCount { get; set; }
        public int PaymentsCount { get; set; }
        public decimal DueReceivables { get; set; }
        public decimal DuePayables { get; set; }
        public int TotalItems { get; set; }
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalVendors { get; set; }
        public List<DashboardPartyRow> Customers { get; set; } = new List<DashboardPartyRow>();
        public List<DashboardPartyRow> Vendors { get; set; } = new List<DashboardPartyRow>();
        public List<DashboardTrendPoint> SalesTrend { get; set; } = new List<DashboardTrendPoint>();
        public List<DashboardTopItem> TopSellingItems { get; set; } = new List<DashboardTopItem>();
    }

    public class DashboardPartyRow
    {
        public int LedgerId { get; set; }
        public string Name { get; set; }
    }

    public class DashboardTrendPoint
    {
        public DateTime PeriodStart { get; set; }
        public string Caption { get; set; }
        public decimal Amount { get; set; }
    }

    public enum DashboardOverviewRangeKind
    {
        Day,
        Month,
        Year
    }

    public class DashboardTopItem
    {
        public string ItemName { get; set; }
        public decimal Qty { get; set; }
        public decimal Amount { get; set; }
    }
}
