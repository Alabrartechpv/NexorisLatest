using ModelClass;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Repository.ReportRepository
{
    public class DatewiseProfitReportRepository : BaseRepostitory
    {
        private static bool _procedureEnsured = false;
        private static readonly object _procedureLock = new object();

        public DatewiseProfitReportRepository()
        {
            EnsureStoredProcedure();
        }

        /// <summary>
        /// Automatically ensures that the POS_DatewiseProfitReport stored procedure exists and is up to date in the database.
        /// </summary>
        public void EnsureStoredProcedure()
        {
            if (_procedureEnsured) return;

            lock (_procedureLock)
            {
                if (_procedureEnsured) return;

                try
                {
                    bool wasClosed = DataConnection.State == ConnectionState.Closed;
                    if (wasClosed) DataConnection.Open();

                    string ddl = @"
IF OBJECT_ID(N'[dbo].[POS_DatewiseProfitReport]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[POS_DatewiseProfitReport];

EXEC dbo.sp_executesql @statement = N'
CREATE PROCEDURE [dbo].[POS_DatewiseProfitReport]
    @FromDate          DATE          = NULL,
    @ToDate            DATE          = NULL,
    @PeriodMode        VARCHAR(20)   = ''ALL'',
    @Month             INT           = 0,
    @Year              INT           = 0,
    @CompanyId         INT           = 0,
    @BranchId          INT           = 0,
    @FinYearId         INT           = 0,
    @CustomerLedgerId  INT           = 0,
    @ItemSearch        NVARCHAR(200) = NULL,
    @ViewMode          VARCHAR(30)   = ''DATE_MATRIX''
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Mode VARCHAR(20) = UPPER(LTRIM(RTRIM(ISNULL(@PeriodMode, ''ALL''))));
    DECLARE @View VARCHAR(30) = UPPER(LTRIM(RTRIM(ISNULL(@ViewMode, ''DATE_MATRIX''))));
    DECLARE @CleanSearch NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@ItemSearch)), '''');

    DECLARE @StartDate DATE = @FromDate;
    DECLARE @EndDate DATE = @ToDate;

    IF @Mode = ''MONTH_WISE''
    BEGIN
        IF @Year > 0 AND @Month > 0
        BEGIN
            SET @StartDate = DATEFROMPARTS(@Year, @Month, 1);
            SET @EndDate = EOMONTH(@StartDate);
        END
        ELSE IF @Year > 0 AND (@Month = 0 OR @Month IS NULL)
        BEGIN
            SET @StartDate = DATEFROMPARTS(@Year, 1, 1);
            SET @EndDate = DATEFROMPARTS(@Year, 12, 31);
        END
    END
    ELSE IF @Mode = ''YEAR_WISE''
    BEGIN
        IF @Year > 0
        BEGIN
            SET @StartDate = DATEFROMPARTS(@Year, 1, 1);
            SET @EndDate = DATEFROMPARTS(@Year, 12, 31);
        END
    END

    IF @View = ''DETAILED''
    BEGIN
        SELECT 
            CAST(M.BillDate AS DATE) AS BillDate,
            M.BillNo,
            CAST(M.BillNo AS NVARCHAR(50)) AS InvoiceNo,
            CAST(M.BillDate AS DATE) AS InvoiceDate,
            ISNULL(NULLIF(M.CustomerName, ''''), ''Cash Customer'') AS CustomerName,
            ISNULL(M.LedgerID, 0) AS CustomerLedgerId,
            ISNULL(D.SlNO, 1) AS SlNo,
            ISNULL(D.ItemId, 0) AS ItemID,
            ISNULL(NULLIF(D.Barcode, ''''), ISNULL(PS.BarCode, '''')) AS BarCode,
            ISNULL(D.ItemName, '''') AS ItemName,
            ISNULL(D.Unit, '''') AS Unit,
            CAST(ISNULL(D.Packing, ''1'') AS NVARCHAR(50)) AS Packing,
            CAST(ISNULL(D.Qty, 0) AS DECIMAL(18,2)) AS Qty,
            CAST(ISNULL(D.UnitPrice, 0) AS DECIMAL(18,2)) AS Rate,
            CAST(ISNULL(D.Cost, 0) AS DECIMAL(18,2)) AS CostRate,
            CAST(ISNULL(D.Amount, ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0)) AS DECIMAL(18,2)) AS SalesAmount,
            CAST(ISNULL(D.DiscountAmount, 0) AS DECIMAL(18,2)) AS DiscountAmt,
            CAST(ISNULL(D.TaxAmt, 0) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(ISNULL(D.TotalAmount, (ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0)) + ISNULL(D.TaxAmt, 0)) AS DECIMAL(18,2)) AS NetAmount,
            CAST(ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0) AS DECIMAL(18,2)) AS CostAmount,
            CAST(ISNULL(D.MarginAmt, ISNULL(D.TotalAmount, (ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0))) - (ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0))) AS DECIMAL(18,2)) AS Profit,
            CAST(CASE WHEN ISNULL(D.TotalAmount, 0) <> 0 
                      THEN (ISNULL(D.MarginAmt, ISNULL(D.TotalAmount, 0) - (ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0))) / D.TotalAmount) * 100 
                      ELSE 0 
                 END AS DECIMAL(18,2)) AS MarginPercent,
            ISNULL(M.PaymodeName, '''') AS Paymode,
            ISNULL(CAST(M.UserId AS NVARCHAR(50)), '''') AS BilledBy
        FROM dbo.SMaster M WITH (NOLOCK)
        INNER JOIN dbo.SDetails D WITH (NOLOCK) ON M.BillNo = D.BillNo
        OUTER APPLY (
            SELECT TOP 1 ps.BarCode
            FROM dbo.PriceSettings ps WITH (NOLOCK)
            WHERE ps.ItemId = D.ItemId
              AND (@BranchId = 0 OR ps.BranchId = @BranchId)
            ORDER BY CASE WHEN ps.UnitId = D.UnitId THEN 0 WHEN ps.IsBaseUnit = ''Y'' THEN 1 ELSE 2 END
        ) PS
        WHERE ISNULL(M.CancelFlag, 0) = 0
          AND ISNULL(M.Status, '''') NOT IN (''Hold'', ''HOLD'')
          AND (@CompanyId = 0 OR ISNULL(M.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(M.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(M.FinYearId, 0) = @FinYearId)
          AND (@CustomerLedgerId = 0 OR ISNULL(M.LedgerID, 0) = @CustomerLedgerId)
          AND (@CleanSearch IS NULL OR D.ItemName LIKE ''%'' + @CleanSearch + ''%'' OR D.Barcode LIKE ''%'' + @CleanSearch + ''%'' OR PS.BarCode LIKE ''%'' + @CleanSearch + ''%'')
          AND (
                @Mode = ''ALL''
                OR (@Mode = ''DATE_RANGE'' AND CAST(M.BillDate AS DATE) >= @StartDate AND CAST(M.BillDate AS DATE) <= @EndDate)
                OR (@Mode = ''MONTH_WISE'' AND (
                    (@Year = 0 OR YEAR(M.BillDate) = @Year) AND
                    (@Month = 0 OR MONTH(M.BillDate) = @Month)
                ))
                OR (@Mode = ''YEAR_WISE'' AND (@Year = 0 OR YEAR(M.BillDate) = @Year))
              )
        ORDER BY M.BillDate DESC, M.BillNo DESC, D.SlNO ASC;
    END
    ELSE IF @View = ''DATE_ITEM_SUMMARY'' OR @View = ''DATE_MATRIX''
    BEGIN
        SELECT 
            CAST(M.BillDate AS DATE) AS BillDate,
            CAST(0 AS BIGINT) AS BillNo,
            ''Daily Summary'' AS InvoiceNo,
            CAST(M.BillDate AS DATE) AS InvoiceDate,
            ''Multiple Customers'' AS CustomerName,
            0 AS CustomerLedgerId,
            ROW_NUMBER() OVER(PARTITION BY CAST(M.BillDate AS DATE) ORDER BY D.ItemName) AS SlNo,
            ISNULL(D.ItemId, 0) AS ItemID,
            ISNULL(MAX(NULLIF(D.Barcode, '''')), ISNULL(MAX(PS.BarCode), '''')) AS BarCode,
            ISNULL(D.ItemName, '''') AS ItemName,
            ISNULL(D.Unit, '''') AS Unit,
            ''1'' AS Packing,
            CAST(SUM(ISNULL(D.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
            CAST(CASE WHEN SUM(ISNULL(D.Qty, 0)) > 0 
                      THEN SUM(ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0)) / SUM(ISNULL(D.Qty, 0)) 
                      ELSE MAX(ISNULL(D.UnitPrice, 0)) 
                 END AS DECIMAL(18,2)) AS Rate,
            CAST(CASE WHEN SUM(ISNULL(D.Qty, 0)) > 0 
                      THEN SUM(ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)) / SUM(ISNULL(D.Qty, 0)) 
                      ELSE MAX(ISNULL(D.Cost, 0)) 
                 END AS DECIMAL(18,2)) AS CostRate,
            CAST(SUM(ISNULL(D.Amount, ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0))) AS DECIMAL(18,2)) AS SalesAmount,
            CAST(SUM(ISNULL(D.DiscountAmount, 0)) AS DECIMAL(18,2)) AS DiscountAmt,
            CAST(SUM(ISNULL(D.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(SUM(ISNULL(D.TotalAmount, (ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0)) + ISNULL(D.TaxAmt, 0))) AS DECIMAL(18,2)) AS NetAmount,
            CAST(SUM(ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)) AS DECIMAL(18,2)) AS CostAmount,
            CAST(SUM(ISNULL(D.MarginAmt, ISNULL(D.TotalAmount, 0) - (ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)))) AS DECIMAL(18,2)) AS Profit,
            CAST(CASE WHEN SUM(ISNULL(D.TotalAmount, 0)) > 0 
                      THEN (SUM(ISNULL(D.MarginAmt, ISNULL(D.TotalAmount, 0) - (ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)))) / SUM(D.TotalAmount)) * 100 
                      ELSE 0 
                 END AS DECIMAL(18,2)) AS MarginPercent,
            '''' AS Paymode,
            '''' AS BilledBy
        FROM dbo.SMaster M WITH (NOLOCK)
        INNER JOIN dbo.SDetails D WITH (NOLOCK) ON M.BillNo = D.BillNo
        OUTER APPLY (
            SELECT TOP 1 ps.BarCode
            FROM dbo.PriceSettings ps WITH (NOLOCK)
            WHERE ps.ItemId = D.ItemId
              AND (@BranchId = 0 OR ps.BranchId = @BranchId)
            ORDER BY CASE WHEN ps.UnitId = D.UnitId THEN 0 WHEN ps.IsBaseUnit = ''Y'' THEN 1 ELSE 2 END
        ) PS
        WHERE ISNULL(M.CancelFlag, 0) = 0
          AND ISNULL(M.Status, '''') NOT IN (''Hold'', ''HOLD'')
          AND (@CompanyId = 0 OR ISNULL(M.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(M.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(M.FinYearId, 0) = @FinYearId)
          AND (@CustomerLedgerId = 0 OR ISNULL(M.LedgerID, 0) = @CustomerLedgerId)
          AND (@CleanSearch IS NULL OR D.ItemName LIKE ''%'' + @CleanSearch + ''%'' OR D.Barcode LIKE ''%'' + @CleanSearch + ''%'' OR PS.BarCode LIKE ''%'' + @CleanSearch + ''%'')
          AND (
                @Mode = ''ALL''
                OR (@Mode = ''DATE_RANGE'' AND CAST(M.BillDate AS DATE) >= @StartDate AND CAST(M.BillDate AS DATE) <= @EndDate)
                OR (@Mode = ''MONTH_WISE'' AND (
                    (@Year = 0 OR YEAR(M.BillDate) = @Year) AND
                    (@Month = 0 OR MONTH(M.BillDate) = @Month)
                ))
                OR (@Mode = ''YEAR_WISE'' AND (@Year = 0 OR YEAR(M.BillDate) = @Year))
              )
        GROUP BY CAST(M.BillDate AS DATE), D.ItemId, D.ItemName, D.Unit
        ORDER BY CAST(M.BillDate AS DATE) DESC, D.ItemName ASC;
    END
    ELSE
    BEGIN
        SELECT 
            MAX(M.BillDate) AS BillDate,
            CAST(0 AS BIGINT) AS BillNo,
            ''Item Summary'' AS InvoiceNo,
            MAX(M.BillDate) AS InvoiceDate,
            ''Multiple Customers'' AS CustomerName,
            0 AS CustomerLedgerId,
            ROW_NUMBER() OVER(ORDER BY D.ItemName) AS SlNo,
            ISNULL(D.ItemId, 0) AS ItemID,
            ISNULL(MAX(NULLIF(D.Barcode, '''')), ISNULL(MAX(PS.BarCode), '''')) AS BarCode,
            ISNULL(D.ItemName, '''') AS ItemName,
            ISNULL(D.Unit, '''') AS Unit,
            ''1'' AS Packing,
            CAST(SUM(ISNULL(D.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
            CAST(CASE WHEN SUM(ISNULL(D.Qty, 0)) > 0 
                      THEN SUM(ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0)) / SUM(ISNULL(D.Qty, 0)) 
                      ELSE MAX(ISNULL(D.UnitPrice, 0)) 
                 END AS DECIMAL(18,2)) AS Rate,
            CAST(CASE WHEN SUM(ISNULL(D.Qty, 0)) > 0 
                      THEN SUM(ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)) / SUM(ISNULL(D.Qty, 0)) 
                      ELSE MAX(ISNULL(D.Cost, 0)) 
                 END AS DECIMAL(18,2)) AS CostRate,
            CAST(SUM(ISNULL(D.Amount, ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0))) AS DECIMAL(18,2)) AS SalesAmount,
            CAST(SUM(ISNULL(D.DiscountAmount, 0)) AS DECIMAL(18,2)) AS DiscountAmt,
            CAST(SUM(ISNULL(D.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(SUM(ISNULL(D.TotalAmount, (ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0)) + ISNULL(D.TaxAmt, 0))) AS DECIMAL(18,2)) AS NetAmount,
            CAST(SUM(ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)) AS DECIMAL(18,2)) AS CostAmount,
            CAST(SUM(ISNULL(D.MarginAmt, ISNULL(D.TotalAmount, 0) - (ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)))) AS DECIMAL(18,2)) AS Profit,
            CAST(CASE WHEN SUM(ISNULL(D.TotalAmount, 0)) > 0 
                      THEN (SUM(ISNULL(D.MarginAmt, ISNULL(D.TotalAmount, 0) - (ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)))) / SUM(D.TotalAmount)) * 100 
                      ELSE 0 
                 END AS DECIMAL(18,2)) AS MarginPercent,
            '''' AS Paymode,
            '''' AS BilledBy
        FROM dbo.SMaster M WITH (NOLOCK)
        INNER JOIN dbo.SDetails D WITH (NOLOCK) ON M.BillNo = D.BillNo
        OUTER APPLY (
            SELECT TOP 1 ps.BarCode
            FROM dbo.PriceSettings ps WITH (NOLOCK)
            WHERE ps.ItemId = D.ItemId
              AND (@BranchId = 0 OR ps.BranchId = @BranchId)
            ORDER BY CASE WHEN ps.UnitId = D.UnitId THEN 0 WHEN ps.IsBaseUnit = ''Y'' THEN 1 ELSE 2 END
        ) PS
        WHERE ISNULL(M.CancelFlag, 0) = 0
          AND ISNULL(M.Status, '''') NOT IN (''Hold'', ''HOLD'')
          AND (@CompanyId = 0 OR ISNULL(M.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(M.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(M.FinYearId, 0) = @FinYearId)
          AND (@CustomerLedgerId = 0 OR ISNULL(M.LedgerID, 0) = @CustomerLedgerId)
          AND (@CleanSearch IS NULL OR D.ItemName LIKE ''%'' + @CleanSearch + ''%'' OR D.Barcode LIKE ''%'' + @CleanSearch + ''%'' OR PS.BarCode LIKE ''%'' + @CleanSearch + ''%'')
          AND (
                @Mode = ''ALL''
                OR (@Mode = ''DATE_RANGE'' AND CAST(M.BillDate AS DATE) >= @StartDate AND CAST(M.BillDate AS DATE) <= @EndDate)
                OR (@Mode = ''MONTH_WISE'' AND (
                    (@Year = 0 OR YEAR(M.BillDate) = @Year) AND
                    (@Month = 0 OR MONTH(M.BillDate) = @Month)
                ))
                OR (@Mode = ''YEAR_WISE'' AND (@Year = 0 OR YEAR(M.BillDate) = @Year))
              )
        GROUP BY D.ItemId, D.ItemName, D.Unit
        ORDER BY D.ItemName ASC;
    END
END
';
";
                    using (SqlCommand cmd = new SqlCommand(ddl, (SqlConnection)DataConnection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    if (wasClosed && DataConnection.State == ConnectionState.Open)
                    {
                        DataConnection.Close();
                    }

                    _procedureEnsured = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"EnsureStoredProcedure error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Retrieves the Date-wise Profit Report rows based on user filters.
        /// </summary>
        public List<DatewiseProfitReportRow> GetReport(DatewiseProfitReportFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            List<DatewiseProfitReportRow> list = new List<DatewiseProfitReportRow>();
            bool wasClosed = DataConnection.State == ConnectionState.Closed;

            try
            {
                if (wasClosed) DataConnection.Open();

                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_DatewiseProfitReport, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 180;

                        cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = (object)filter.FromDate.Date ?? DBNull.Value;
                        cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = (object)filter.ToDate.Date ?? DBNull.Value;
                        cmd.Parameters.Add("@PeriodMode", SqlDbType.VarChar, 20).Value = string.IsNullOrWhiteSpace(filter.PeriodMode) ? "ALL" : filter.PeriodMode;
                        cmd.Parameters.Add("@Month", SqlDbType.Int).Value = filter.Month;
                        cmd.Parameters.Add("@Year", SqlDbType.Int).Value = filter.Year;
                        cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = filter.CompanyId > 0 ? filter.CompanyId : SessionContext.CompanyId;
                        cmd.Parameters.Add("@BranchId", SqlDbType.Int).Value = filter.BranchId > 0 ? filter.BranchId : SessionContext.BranchId;
                        cmd.Parameters.Add("@FinYearId", SqlDbType.Int).Value = filter.FinYearId > 0 ? filter.FinYearId : SessionContext.FinYearId;
                        cmd.Parameters.Add("@CustomerLedgerId", SqlDbType.Int).Value = filter.CustomerLedgerId;
                        cmd.Parameters.Add("@ItemSearch", SqlDbType.NVarChar, 200).Value = string.IsNullOrWhiteSpace(filter.ItemSearch) ? (object)DBNull.Value : filter.ItemSearch.Trim();
                        cmd.Parameters.Add("@ViewMode", SqlDbType.VarChar, 30).Value = string.IsNullOrWhiteSpace(filter.ViewMode) ? "DATE_MATRIX" : filter.ViewMode;

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable table = new DataTable();
                            adapter.Fill(table);
                            list = MapDataTableToReportRows(table);
                            return list;
                        }
                    }
                }
                catch (SqlException)
                {
                    list = ExecuteFallbackQuery(filter);
                }
            }
            finally
            {
                if (wasClosed && DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return list;
        }

        private List<DatewiseProfitReportRow> ExecuteFallbackQuery(DatewiseProfitReportFilter filter)
        {
            List<DatewiseProfitReportRow> list = new List<DatewiseProfitReportRow>();
            string mode = (filter.PeriodMode ?? "ALL").ToUpperInvariant();
            string view = (filter.ViewMode ?? "DATE_MATRIX").ToUpperInvariant();

            int companyId = filter.CompanyId > 0 ? filter.CompanyId : SessionContext.CompanyId;
            int branchId = filter.BranchId > 0 ? filter.BranchId : SessionContext.BranchId;
            int finYearId = filter.FinYearId > 0 ? filter.FinYearId : SessionContext.FinYearId;

            string sql;
            if (view == "DATE_ITEM_SUMMARY" || view == "DATE_MATRIX")
            {
                sql = @"
SELECT 
    CAST(M.BillDate AS DATE) AS BillDate,
    CAST(0 AS BIGINT) AS BillNo,
    'Daily Summary' AS InvoiceNo,
    CAST(M.BillDate AS DATE) AS InvoiceDate,
    'Multiple Customers' AS CustomerName,
    0 AS CustomerLedgerId,
    ROW_NUMBER() OVER(PARTITION BY CAST(M.BillDate AS DATE) ORDER BY D.ItemName) AS SlNo,
    ISNULL(D.ItemId, 0) AS ItemID,
    ISNULL(MAX(NULLIF(D.Barcode, '')), ISNULL(MAX(PS.BarCode), '')) AS BarCode,
    ISNULL(D.ItemName, '') AS ItemName,
    ISNULL(D.Unit, '') AS Unit,
    '1' AS Packing,
    CAST(SUM(ISNULL(D.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
    CAST(CASE WHEN SUM(ISNULL(D.Qty, 0)) > 0 THEN SUM(ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0)) / SUM(ISNULL(D.Qty, 0)) ELSE MAX(ISNULL(D.UnitPrice, 0)) END AS DECIMAL(18,2)) AS Rate,
    CAST(CASE WHEN SUM(ISNULL(D.Qty, 0)) > 0 THEN SUM(ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)) / SUM(ISNULL(D.Qty, 0)) ELSE MAX(ISNULL(D.Cost, 0)) END AS DECIMAL(18,2)) AS CostRate,
    CAST(SUM(ISNULL(D.Amount, ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0))) AS DECIMAL(18,2)) AS SalesAmount,
    CAST(SUM(ISNULL(D.DiscountAmount, 0)) AS DECIMAL(18,2)) AS DiscountAmt,
    CAST(SUM(ISNULL(D.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
    CAST(SUM(ISNULL(D.TotalAmount, (ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0)) + ISNULL(D.TaxAmt, 0))) AS DECIMAL(18,2)) AS NetAmount,
    CAST(SUM(ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)) AS DECIMAL(18,2)) AS CostAmount,
    CAST(SUM(ISNULL(D.MarginAmt, ISNULL(D.TotalAmount, 0) - (ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)))) AS DECIMAL(18,2)) AS Profit,
    CAST(CASE WHEN SUM(ISNULL(D.TotalAmount, 0)) > 0 THEN (SUM(ISNULL(D.MarginAmt, ISNULL(D.TotalAmount, 0) - (ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)))) / SUM(D.TotalAmount)) * 100 ELSE 0 END AS DECIMAL(18,2)) AS MarginPercent,
    '' AS Paymode,
    '' AS BilledBy
FROM dbo.SMaster M WITH (NOLOCK)
INNER JOIN dbo.SDetails D WITH (NOLOCK) ON M.BillNo = D.BillNo
OUTER APPLY (
    SELECT TOP 1 ps.BarCode
    FROM dbo.PriceSettings ps WITH (NOLOCK)
    WHERE ps.ItemId = D.ItemId
      AND (@BranchId = 0 OR ps.BranchId = @BranchId)
    ORDER BY CASE WHEN ps.UnitId = D.UnitId THEN 0 WHEN ps.IsBaseUnit = 'Y' THEN 1 ELSE 2 END
) PS
WHERE ISNULL(M.CancelFlag, 0) = 0
  AND ISNULL(M.Status, '') NOT IN ('Hold', 'HOLD')
  AND (@CompanyId = 0 OR ISNULL(M.CompanyId, 0) = @CompanyId)
  AND (@BranchId = 0 OR ISNULL(M.BranchId, 0) = @BranchId)
  AND (@FinYearId = 0 OR ISNULL(M.FinYearId, 0) = @FinYearId)
  AND (@CustomerLedgerId = 0 OR ISNULL(M.LedgerID, 0) = @CustomerLedgerId)
  AND (@ItemSearch IS NULL OR D.ItemName LIKE '%' + @ItemSearch + '%' OR D.Barcode LIKE '%' + @ItemSearch + '%' OR PS.BarCode LIKE '%' + @ItemSearch + '%')
  AND (
        @Mode = 'ALL'
        OR (@Mode = 'DATE_RANGE' AND CAST(M.BillDate AS DATE) >= @FromDate AND CAST(M.BillDate AS DATE) <= @ToDate)
        OR (@Mode = 'MONTH_WISE' AND ((@Year = 0 OR YEAR(M.BillDate) = @Year) AND (@Month = 0 OR MONTH(M.BillDate) = @Month)))
        OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(M.BillDate) = @Year))
      )
GROUP BY CAST(M.BillDate AS DATE), D.ItemId, D.ItemName, D.Unit
ORDER BY CAST(M.BillDate AS DATE) DESC, D.ItemName ASC;";
            }
            else if (view == "ITEM_SUMMARY")
            {
                sql = @"
SELECT 
    MAX(M.BillDate) AS BillDate,
    CAST(0 AS BIGINT) AS BillNo,
    'Item Summary' AS InvoiceNo,
    MAX(M.BillDate) AS InvoiceDate,
    'Multiple Customers' AS CustomerName,
    0 AS CustomerLedgerId,
    ROW_NUMBER() OVER(ORDER BY D.ItemName) AS SlNo,
    ISNULL(D.ItemId, 0) AS ItemID,
    ISNULL(MAX(NULLIF(D.Barcode, '')), ISNULL(MAX(PS.BarCode), '')) AS BarCode,
    ISNULL(D.ItemName, '') AS ItemName,
    ISNULL(D.Unit, '') AS Unit,
    '1' AS Packing,
    CAST(SUM(ISNULL(D.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
    CAST(CASE WHEN SUM(ISNULL(D.Qty, 0)) > 0 THEN SUM(ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0)) / SUM(ISNULL(D.Qty, 0)) ELSE MAX(ISNULL(D.UnitPrice, 0)) END AS DECIMAL(18,2)) AS Rate,
    CAST(CASE WHEN SUM(ISNULL(D.Qty, 0)) > 0 THEN SUM(ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)) / SUM(ISNULL(D.Qty, 0)) ELSE MAX(ISNULL(D.Cost, 0)) END AS DECIMAL(18,2)) AS CostRate,
    CAST(SUM(ISNULL(D.Amount, ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0))) AS DECIMAL(18,2)) AS SalesAmount,
    CAST(SUM(ISNULL(D.DiscountAmount, 0)) AS DECIMAL(18,2)) AS DiscountAmt,
    CAST(SUM(ISNULL(D.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
    CAST(SUM(ISNULL(D.TotalAmount, (ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0)) + ISNULL(D.TaxAmt, 0))) AS DECIMAL(18,2)) AS NetAmount,
    CAST(SUM(ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)) AS DECIMAL(18,2)) AS CostAmount,
    CAST(SUM(ISNULL(D.MarginAmt, ISNULL(D.TotalAmount, 0) - (ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)))) AS DECIMAL(18,2)) AS Profit,
    CAST(CASE WHEN SUM(ISNULL(D.TotalAmount, 0)) > 0 THEN (SUM(ISNULL(D.MarginAmt, ISNULL(D.TotalAmount, 0) - (ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0)))) / SUM(D.TotalAmount)) * 100 ELSE 0 END AS DECIMAL(18,2)) AS MarginPercent,
    '' AS Paymode,
    '' AS BilledBy
FROM dbo.SMaster M WITH (NOLOCK)
INNER JOIN dbo.SDetails D WITH (NOLOCK) ON M.BillNo = D.BillNo
OUTER APPLY (
    SELECT TOP 1 ps.BarCode
    FROM dbo.PriceSettings ps WITH (NOLOCK)
    WHERE ps.ItemId = D.ItemId
      AND (@BranchId = 0 OR ps.BranchId = @BranchId)
    ORDER BY CASE WHEN ps.UnitId = D.UnitId THEN 0 WHEN ps.IsBaseUnit = 'Y' THEN 1 ELSE 2 END
) PS
WHERE ISNULL(M.CancelFlag, 0) = 0
  AND ISNULL(M.Status, '') NOT IN ('Hold', 'HOLD')
  AND (@CompanyId = 0 OR ISNULL(M.CompanyId, 0) = @CompanyId)
  AND (@BranchId = 0 OR ISNULL(M.BranchId, 0) = @BranchId)
  AND (@FinYearId = 0 OR ISNULL(M.FinYearId, 0) = @FinYearId)
  AND (@CustomerLedgerId = 0 OR ISNULL(M.LedgerID, 0) = @CustomerLedgerId)
  AND (@ItemSearch IS NULL OR D.ItemName LIKE '%' + @ItemSearch + '%' OR D.Barcode LIKE '%' + @ItemSearch + '%' OR PS.BarCode LIKE '%' + @ItemSearch + '%')
  AND (
        @Mode = 'ALL'
        OR (@Mode = 'DATE_RANGE' AND CAST(M.BillDate AS DATE) >= @FromDate AND CAST(M.BillDate AS DATE) <= @ToDate)
        OR (@Mode = 'MONTH_WISE' AND ((@Year = 0 OR YEAR(M.BillDate) = @Year) AND (@Month = 0 OR MONTH(M.BillDate) = @Month)))
        OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(M.BillDate) = @Year))
      )
GROUP BY D.ItemId, D.ItemName, D.Unit
ORDER BY D.ItemName ASC;";
            }
            else // DETAILED
            {
                sql = @"
SELECT 
    CAST(M.BillDate AS DATE) AS BillDate,
    M.BillNo,
    CAST(M.BillNo AS NVARCHAR(50)) AS InvoiceNo,
    CAST(M.BillDate AS DATE) AS InvoiceDate,
    ISNULL(NULLIF(M.CustomerName, ''), 'Cash Customer') AS CustomerName,
    ISNULL(M.LedgerID, 0) AS CustomerLedgerId,
    ISNULL(D.SlNO, 1) AS SlNo,
    ISNULL(D.ItemId, 0) AS ItemID,
    ISNULL(NULLIF(D.Barcode, ''), ISNULL(PS.BarCode, '')) AS BarCode,
    ISNULL(D.ItemName, '') AS ItemName,
    ISNULL(D.Unit, '') AS Unit,
    CAST(ISNULL(D.Packing, '1') AS NVARCHAR(50)) AS Packing,
    CAST(ISNULL(D.Qty, 0) AS DECIMAL(18,2)) AS Qty,
    CAST(ISNULL(D.UnitPrice, 0) AS DECIMAL(18,2)) AS Rate,
    CAST(ISNULL(D.Cost, 0) AS DECIMAL(18,2)) AS CostRate,
    CAST(ISNULL(D.Amount, ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0)) AS DECIMAL(18,2)) AS SalesAmount,
    CAST(ISNULL(D.DiscountAmount, 0) AS DECIMAL(18,2)) AS DiscountAmt,
    CAST(ISNULL(D.TaxAmt, 0) AS DECIMAL(18,2)) AS TaxAmt,
    CAST(ISNULL(D.TotalAmount, (ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0)) + ISNULL(D.TaxAmt, 0)) AS DECIMAL(18,2)) AS NetAmount,
    CAST(ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0) AS DECIMAL(18,2)) AS CostAmount,
    CAST(ISNULL(D.MarginAmt, ISNULL(D.TotalAmount, (ISNULL(D.Qty, 0) * ISNULL(D.UnitPrice, 0))) - (ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0))) AS DECIMAL(18,2)) AS Profit,
    CAST(CASE WHEN ISNULL(D.TotalAmount, 0) <> 0 
              THEN (ISNULL(D.MarginAmt, ISNULL(D.TotalAmount, 0) - (ISNULL(D.Qty, 0) * ISNULL(D.Cost, 0))) / D.TotalAmount) * 100 
              ELSE 0 
         END AS DECIMAL(18,2)) AS MarginPercent,
    ISNULL(M.PaymodeName, '') AS Paymode,
    ISNULL(CAST(M.UserId AS NVARCHAR(50)), '') AS BilledBy
FROM dbo.SMaster M WITH (NOLOCK)
INNER JOIN dbo.SDetails D WITH (NOLOCK) ON M.BillNo = D.BillNo
OUTER APPLY (
    SELECT TOP 1 ps.BarCode
    FROM dbo.PriceSettings ps WITH (NOLOCK)
    WHERE ps.ItemId = D.ItemId
      AND (@BranchId = 0 OR ps.BranchId = @BranchId)
    ORDER BY CASE WHEN ps.UnitId = D.UnitId THEN 0 WHEN ps.IsBaseUnit = 'Y' THEN 1 ELSE 2 END
) PS
WHERE ISNULL(M.CancelFlag, 0) = 0
  AND ISNULL(M.Status, '') NOT IN ('Hold', 'HOLD')
  AND (@CompanyId = 0 OR ISNULL(M.CompanyId, 0) = @CompanyId)
  AND (@BranchId = 0 OR ISNULL(M.BranchId, 0) = @BranchId)
  AND (@FinYearId = 0 OR ISNULL(M.FinYearId, 0) = @FinYearId)
  AND (@CustomerLedgerId = 0 OR ISNULL(M.LedgerID, 0) = @CustomerLedgerId)
  AND (@ItemSearch IS NULL OR D.ItemName LIKE '%' + @ItemSearch + '%' OR D.Barcode LIKE '%' + @ItemSearch + '%' OR PS.BarCode LIKE '%' + @ItemSearch + '%')
  AND (
        @Mode = 'ALL'
        OR (@Mode = 'DATE_RANGE' AND CAST(M.BillDate AS DATE) >= @FromDate AND CAST(M.BillDate AS DATE) <= @ToDate)
        OR (@Mode = 'MONTH_WISE' AND ((@Year = 0 OR YEAR(M.BillDate) = @Year) AND (@Month = 0 OR MONTH(M.BillDate) = @Month)))
        OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(M.BillDate) = @Year))
      )
ORDER BY M.BillDate DESC, M.BillNo DESC, D.SlNO ASC;";
            }

            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
            {
                cmd.CommandTimeout = 180;
                cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = companyId;
                cmd.Parameters.Add("@BranchId", SqlDbType.Int).Value = branchId;
                cmd.Parameters.Add("@FinYearId", SqlDbType.Int).Value = finYearId;
                cmd.Parameters.Add("@CustomerLedgerId", SqlDbType.Int).Value = filter.CustomerLedgerId;
                cmd.Parameters.Add("@ItemSearch", SqlDbType.NVarChar, 200).Value = string.IsNullOrWhiteSpace(filter.ItemSearch) ? (object)DBNull.Value : filter.ItemSearch.Trim();
                cmd.Parameters.Add("@Mode", SqlDbType.VarChar, 20).Value = mode;
                cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = filter.FromDate.Date;
                cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = filter.ToDate.Date;
                cmd.Parameters.Add("@Month", SqlDbType.Int).Value = filter.Month;
                cmd.Parameters.Add("@Year", SqlDbType.Int).Value = filter.Year;

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    list = MapDataTableToReportRows(table);
                }
            }

            return list;
        }

        private static List<DatewiseProfitReportRow> MapDataTableToReportRows(DataTable table)
        {
            List<DatewiseProfitReportRow> rows = new List<DatewiseProfitReportRow>();
            if (table == null || table.Rows.Count == 0) return rows;

            foreach (DataRow r in table.Rows)
            {
                rows.Add(new DatewiseProfitReportRow
                {
                    BillDate = ToDateTime(r, "BillDate"),
                    BillNo = ToLong(r, "BillNo"),
                    InvoiceNo = ToString(r, "InvoiceNo"),
                    InvoiceDate = ToDateTime(r, "InvoiceDate"),
                    CustomerName = ToString(r, "CustomerName"),
                    CustomerLedgerId = ToInt(r, "CustomerLedgerId"),
                    SlNo = ToInt(r, "SlNo"),
                    ItemID = ToInt(r, "ItemID"),
                    BarCode = ToString(r, "BarCode"),
                    ItemName = ToString(r, "ItemName"),
                    Unit = ToString(r, "Unit"),
                    Packing = ToString(r, "Packing"),
                    Qty = ToDecimal(r, "Qty"),
                    Rate = ToDecimal(r, "Rate"),
                    CostRate = ToDecimal(r, "CostRate"),
                    SalesAmount = ToDecimal(r, "SalesAmount"),
                    DiscountAmt = ToDecimal(r, "DiscountAmt"),
                    TaxAmt = ToDecimal(r, "TaxAmt"),
                    NetAmount = ToDecimal(r, "NetAmount"),
                    CostAmount = ToDecimal(r, "CostAmount"),
                    Profit = ToDecimal(r, "Profit"),
                    MarginPercent = ToDecimal(r, "MarginPercent"),
                    Paymode = ToString(r, "Paymode"),
                    BilledBy = ToString(r, "BilledBy")
                });
            }

            return rows;
        }

        private static string ToString(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value || row[column] == null)
                return string.Empty;
            return row[column].ToString();
        }

        private static int ToInt(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value || row[column] == null)
                return 0;
            return int.TryParse(row[column].ToString(), out int val) ? val : 0;
        }

        private static long ToLong(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value || row[column] == null)
                return 0L;
            return long.TryParse(row[column].ToString(), out long val) ? val : 0L;
        }

        private static decimal ToDecimal(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value || row[column] == null)
                return 0m;
            return decimal.TryParse(row[column].ToString(), out decimal val) ? val : 0m;
        }

        private static DateTime ToDateTime(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value || row[column] == null)
                return DateTime.Today;
            return DateTime.TryParse(row[column].ToString(), out DateTime val) ? val : DateTime.Today;
        }
    }
}
