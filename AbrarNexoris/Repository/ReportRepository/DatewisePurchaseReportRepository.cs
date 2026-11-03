using ModelClass;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Repository.ReportRepository
{
    public class DatewisePurchaseReportRepository : BaseRepostitory
    {
        private static bool _procedureEnsured = false;
        private static readonly object _procedureLock = new object();

        public DatewisePurchaseReportRepository()
        {
            EnsureStoredProcedure();
        }

        /// <summary>
        /// Automatically ensures that the POS_DatewisePurchaseReport stored procedure exists in the database.
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
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[POS_DatewisePurchaseReport]') AND type IN (N'P', N'PC'))
BEGIN
    EXEC dbo.sp_executesql @statement = N'
    CREATE PROCEDURE [dbo].[POS_DatewisePurchaseReport]
        @FromDate        DATE          = NULL,
        @ToDate          DATE          = NULL,
        @PeriodMode      VARCHAR(20)   = ''ALL'',
        @Month           INT           = 0,
        @Year            INT           = 0,
        @CompanyId       INT           = 0,
        @BranchId        INT           = 0,
        @FinYearId       INT           = 0,
        @VendorLedgerId  INT           = 0,
        @ItemSearch      NVARCHAR(200) = NULL,
        @ViewMode        VARCHAR(30)   = ''DETAILED''
    AS
    BEGIN
        SET NOCOUNT ON;
        DECLARE @Mode VARCHAR(20) = UPPER(LTRIM(RTRIM(ISNULL(@PeriodMode, ''ALL''))));
        DECLARE @View VARCHAR(30) = UPPER(LTRIM(RTRIM(ISNULL(@ViewMode, ''DETAILED''))));
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
                M.PurchaseDate,
                M.PurchaseNo,
                ISNULL(M.InvoiceNo, '''') AS InvoiceNo,
                ISNULL(M.InvoiceDate, M.PurchaseDate) AS InvoiceDate,
                ISNULL(NULLIF(M.VendorName, ''''), ISNULL(L.LedgerName, ''Unknown Vendor'')) AS VendorName,
                ISNULL(M.LedgerID, 0) AS VendorLedgerId,
                ISNULL(D.SlNo, 1) AS SlNo,
                ISNULL(D.ItemID, 0) AS ItemID,
                ISNULL(D.BarCode, '''') AS BarCode,
                ISNULL(D.ItemName, '''') AS ItemName,
                ISNULL(D.Unit, '''') AS Unit,
                CAST(ISNULL(D.Packing, ''1'') AS NVARCHAR(50)) AS Packing,
                CAST(ISNULL(D.Qty, 0) AS DECIMAL(18,2)) AS Qty,
                CAST(ISNULL(D.Free, 0) AS DECIMAL(18,2)) AS Free,
                CAST((ISNULL(D.Qty, 0) + ISNULL(D.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
                CAST(ISNULL(D.Cost, 0) AS DECIMAL(18,2)) AS Cost,
                CAST(ISNULL(D.Amount, 0) AS DECIMAL(18,2)) AS Amount,
                CAST(ISNULL(D.TaxAmt, 0) AS DECIMAL(18,2)) AS TaxAmt,
                CAST((ISNULL(D.Amount, 0) + ISNULL(D.TaxAmt, 0)) AS DECIMAL(18,2)) AS NetAmount,
                ISNULL(M.Paymode, '''') AS Paymode,
                ISNULL(M.BilledBy, '''') AS BilledBy
            FROM dbo.PMaster M WITH (NOLOCK)
            INNER JOIN dbo.PDetails D WITH (NOLOCK) ON M.PurchaseNo = D.PurchaseNo
            LEFT JOIN dbo.Ledger L WITH (NOLOCK) ON M.LedgerID = L.LedgerID
            WHERE ISNULL(M.CancelFlag, 0) = 0
              AND (@CompanyId = 0 OR ISNULL(M.CompanyId, 0) = @CompanyId)
              AND (@BranchId = 0 OR ISNULL(M.BranchId, 0) = @BranchId)
              AND (@FinYearId = 0 OR ISNULL(M.FinYearId, 0) = @FinYearId)
              AND (@VendorLedgerId = 0 OR ISNULL(M.LedgerID, 0) = @VendorLedgerId)
              AND (@CleanSearch IS NULL OR D.ItemName LIKE ''%'' + @CleanSearch + ''%'' OR D.BarCode LIKE ''%'' + @CleanSearch + ''%'')
              AND (
                    @Mode = ''ALL''
                    OR (@Mode = ''DATE_RANGE'' AND CAST(M.PurchaseDate AS DATE) >= @StartDate AND CAST(M.PurchaseDate AS DATE) <= @EndDate)
                    OR (@Mode = ''MONTH_WISE'' AND (
                        (@Year = 0 OR YEAR(M.PurchaseDate) = @Year) AND
                        (@Month = 0 OR MONTH(M.PurchaseDate) = @Month)
                    ))
                    OR (@Mode = ''YEAR_WISE'' AND (@Year = 0 OR YEAR(M.PurchaseDate) = @Year))
                  )
            ORDER BY M.PurchaseDate DESC, M.PurchaseNo DESC, D.SlNo ASC;
        END
        ELSE IF @View = ''DATE_ITEM_SUMMARY''
        BEGIN
            SELECT 
                CAST(M.PurchaseDate AS DATE) AS PurchaseDate,
                CAST(0 AS BIGINT) AS PurchaseNo,
                ''Daily Summary'' AS InvoiceNo,
                CAST(M.PurchaseDate AS DATE) AS InvoiceDate,
                ''Multiple Vendors'' AS VendorName,
                0 AS VendorLedgerId,
                ROW_NUMBER() OVER(PARTITION BY CAST(M.PurchaseDate AS DATE) ORDER BY D.ItemName) AS SlNo,
                ISNULL(D.ItemID, 0) AS ItemID,
                ISNULL(D.BarCode, '''') AS BarCode,
                ISNULL(D.ItemName, '''') AS ItemName,
                ISNULL(D.Unit, '''') AS Unit,
                ''1'' AS Packing,
                CAST(SUM(ISNULL(D.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
                CAST(SUM(ISNULL(D.Free, 0)) AS DECIMAL(18,2)) AS Free,
                CAST(SUM(ISNULL(D.Qty, 0) + ISNULL(D.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
                CAST(CASE WHEN SUM(ISNULL(D.Qty, 0)) > 0 
                          THEN SUM(ISNULL(D.Amount, 0)) / SUM(ISNULL(D.Qty, 0)) 
                          ELSE MAX(ISNULL(D.Cost, 0)) 
                     END AS DECIMAL(18,2)) AS Cost,
                CAST(SUM(ISNULL(D.Amount, 0)) AS DECIMAL(18,2)) AS Amount,
                CAST(SUM(ISNULL(D.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
                CAST(SUM(ISNULL(D.Amount, 0) + ISNULL(D.TaxAmt, 0)) AS DECIMAL(18,2)) AS NetAmount,
                '''' AS Paymode,
                '''' AS BilledBy
            FROM dbo.PMaster M WITH (NOLOCK)
            INNER JOIN dbo.PDetails D WITH (NOLOCK) ON M.PurchaseNo = D.PurchaseNo
            LEFT JOIN dbo.Ledger L WITH (NOLOCK) ON M.LedgerID = L.LedgerID
            WHERE ISNULL(M.CancelFlag, 0) = 0
              AND (@CompanyId = 0 OR ISNULL(M.CompanyId, 0) = @CompanyId)
              AND (@BranchId = 0 OR ISNULL(M.BranchId, 0) = @BranchId)
              AND (@FinYearId = 0 OR ISNULL(M.FinYearId, 0) = @FinYearId)
              AND (@VendorLedgerId = 0 OR ISNULL(M.LedgerID, 0) = @VendorLedgerId)
              AND (@CleanSearch IS NULL OR D.ItemName LIKE ''%'' + @CleanSearch + ''%'' OR D.BarCode LIKE ''%'' + @CleanSearch + ''%'')
              AND (
                    @Mode = ''ALL''
                    OR (@Mode = ''DATE_RANGE'' AND CAST(M.PurchaseDate AS DATE) >= @StartDate AND CAST(M.PurchaseDate AS DATE) <= @EndDate)
                    OR (@Mode = ''MONTH_WISE'' AND (
                        (@Year = 0 OR YEAR(M.PurchaseDate) = @Year) AND
                        (@Month = 0 OR MONTH(M.PurchaseDate) = @Month)
                    ))
                    OR (@Mode = ''YEAR_WISE'' AND (@Year = 0 OR YEAR(M.PurchaseDate) = @Year))
                  )
            GROUP BY CAST(M.PurchaseDate AS DATE), D.ItemID, D.BarCode, D.ItemName, D.Unit
            ORDER BY CAST(M.PurchaseDate AS DATE) DESC, D.ItemName ASC;
        END
        ELSE
        BEGIN
            SELECT 
                MAX(M.PurchaseDate) AS PurchaseDate,
                CAST(0 AS BIGINT) AS PurchaseNo,
                ''Item Summary'' AS InvoiceNo,
                MAX(M.PurchaseDate) AS InvoiceDate,
                ''Multiple Vendors'' AS VendorName,
                0 AS VendorLedgerId,
                ROW_NUMBER() OVER(ORDER BY D.ItemName) AS SlNo,
                ISNULL(D.ItemID, 0) AS ItemID,
                ISNULL(D.BarCode, '''') AS BarCode,
                ISNULL(D.ItemName, '''') AS ItemName,
                ISNULL(D.Unit, '''') AS Unit,
                ''1'' AS Packing,
                CAST(SUM(ISNULL(D.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
                CAST(SUM(ISNULL(D.Free, 0)) AS DECIMAL(18,2)) AS Free,
                CAST(SUM(ISNULL(D.Qty, 0) + ISNULL(D.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
                CAST(CASE WHEN SUM(ISNULL(D.Qty, 0)) > 0 
                          THEN SUM(ISNULL(D.Amount, 0)) / SUM(ISNULL(D.Qty, 0)) 
                          ELSE MAX(ISNULL(D.Cost, 0)) 
                     END AS DECIMAL(18,2)) AS Cost,
                CAST(SUM(ISNULL(D.Amount, 0)) AS DECIMAL(18,2)) AS Amount,
                CAST(SUM(ISNULL(D.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
                CAST(SUM(ISNULL(D.Amount, 0) + ISNULL(D.TaxAmt, 0)) AS DECIMAL(18,2)) AS NetAmount,
                '''' AS Paymode,
                '''' AS BilledBy
            FROM dbo.PMaster M WITH (NOLOCK)
            INNER JOIN dbo.PDetails D WITH (NOLOCK) ON M.PurchaseNo = D.PurchaseNo
            LEFT JOIN dbo.Ledger L WITH (NOLOCK) ON M.LedgerID = L.LedgerID
            WHERE ISNULL(M.CancelFlag, 0) = 0
              AND (@CompanyId = 0 OR ISNULL(M.CompanyId, 0) = @CompanyId)
              AND (@BranchId = 0 OR ISNULL(M.BranchId, 0) = @BranchId)
              AND (@FinYearId = 0 OR ISNULL(M.FinYearId, 0) = @FinYearId)
              AND (@VendorLedgerId = 0 OR ISNULL(M.LedgerID, 0) = @VendorLedgerId)
              AND (@CleanSearch IS NULL OR D.ItemName LIKE ''%'' + @CleanSearch + ''%'' OR D.BarCode LIKE ''%'' + @CleanSearch + ''%'')
              AND (
                    @Mode = ''ALL''
                    OR (@Mode = ''DATE_RANGE'' AND CAST(M.PurchaseDate AS DATE) >= @StartDate AND CAST(M.PurchaseDate AS DATE) <= @EndDate)
                    OR (@Mode = ''MONTH_WISE'' AND (
                        (@Year = 0 OR YEAR(M.PurchaseDate) = @Year) AND
                        (@Month = 0 OR MONTH(M.PurchaseDate) = @Month)
                    ))
                    OR (@Mode = ''YEAR_WISE'' AND (@Year = 0 OR YEAR(M.PurchaseDate) = @Year))
                  )
            GROUP BY D.ItemID, D.BarCode, D.ItemName, D.Unit
            ORDER BY D.ItemName ASC;
        END
    END
    ';
END
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
        /// Retrieves the Date-wise purchase report rows based on user filters.
        /// </summary>
        public List<DatewisePurchaseReportRow> GetReport(DatewisePurchaseReportFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            List<DatewisePurchaseReportRow> list = new List<DatewisePurchaseReportRow>();
            bool wasClosed = DataConnection.State == ConnectionState.Closed;

            try
            {
                if (wasClosed) DataConnection.Open();

                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_DatewisePurchaseReport, (SqlConnection)DataConnection))
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
                        cmd.Parameters.Add("@VendorLedgerId", SqlDbType.Int).Value = filter.VendorLedgerId;
                        cmd.Parameters.Add("@ItemSearch", SqlDbType.NVarChar, 200).Value = string.IsNullOrWhiteSpace(filter.ItemSearch) ? (object)DBNull.Value : filter.ItemSearch.Trim();
                        cmd.Parameters.Add("@ViewMode", SqlDbType.VarChar, 30).Value = string.IsNullOrWhiteSpace(filter.ViewMode) ? "DETAILED" : filter.ViewMode;

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable table = new DataTable();
                            adapter.Fill(table);
                            list = MapDataTableToReportRows(table);
                        }
                    }
                }
                catch (Exception spEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Stored procedure execution failed, falling back to direct query: {spEx.Message}");
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

        private List<DatewisePurchaseReportRow> ExecuteFallbackQuery(DatewisePurchaseReportFilter filter)
        {
            List<DatewisePurchaseReportRow> list = new List<DatewisePurchaseReportRow>();

            int companyId = filter.CompanyId > 0 ? filter.CompanyId : SessionContext.CompanyId;
            int branchId = filter.BranchId > 0 ? filter.BranchId : SessionContext.BranchId;
            int finYearId = filter.FinYearId > 0 ? filter.FinYearId : SessionContext.FinYearId;
            string mode = (filter.PeriodMode ?? "ALL").ToUpperInvariant();
            string view = (filter.ViewMode ?? "DETAILED").ToUpperInvariant();

            string sql;
            if (view == "DATE_ITEM_SUMMARY")
            {
                sql = @"
SELECT 
    CAST(M.PurchaseDate AS DATE) AS PurchaseDate,
    CAST(0 AS BIGINT) AS PurchaseNo,
    'Daily Summary' AS InvoiceNo,
    CAST(M.PurchaseDate AS DATE) AS InvoiceDate,
    'Multiple Vendors' AS VendorName,
    0 AS VendorLedgerId,
    1 AS SlNo,
    ISNULL(D.ItemID, 0) AS ItemID,
    ISNULL(D.BarCode, '') AS BarCode,
    ISNULL(D.ItemName, '') AS ItemName,
    ISNULL(D.Unit, '') AS Unit,
    '1' AS Packing,
    CAST(SUM(ISNULL(D.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
    CAST(SUM(ISNULL(D.Free, 0)) AS DECIMAL(18,2)) AS Free,
    CAST(SUM(ISNULL(D.Qty, 0) + ISNULL(D.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
    CAST(CASE WHEN SUM(ISNULL(D.Qty, 0)) > 0 THEN SUM(ISNULL(D.Amount, 0)) / SUM(ISNULL(D.Qty, 0)) ELSE MAX(ISNULL(D.Cost, 0)) END AS DECIMAL(18,2)) AS Cost,
    CAST(SUM(ISNULL(D.Amount, 0)) AS DECIMAL(18,2)) AS Amount,
    CAST(SUM(ISNULL(D.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
    CAST(SUM(ISNULL(D.Amount, 0) + ISNULL(D.TaxAmt, 0)) AS DECIMAL(18,2)) AS NetAmount,
    '' AS Paymode,
    '' AS BilledBy
FROM dbo.PMaster M WITH (NOLOCK)
INNER JOIN dbo.PDetails D WITH (NOLOCK) ON M.PurchaseNo = D.PurchaseNo
LEFT JOIN dbo.Ledger L WITH (NOLOCK) ON M.LedgerID = L.LedgerID
WHERE ISNULL(M.CancelFlag, 0) = 0
  AND (@CompanyId = 0 OR ISNULL(M.CompanyId, 0) = @CompanyId)
  AND (@BranchId = 0 OR ISNULL(M.BranchId, 0) = @BranchId)
  AND (@FinYearId = 0 OR ISNULL(M.FinYearId, 0) = @FinYearId)
  AND (@VendorLedgerId = 0 OR ISNULL(M.LedgerID, 0) = @VendorLedgerId)
  AND (@ItemSearch IS NULL OR D.ItemName LIKE '%' + @ItemSearch + '%' OR D.BarCode LIKE '%' + @ItemSearch + '%')
  AND (
        @Mode = 'ALL'
        OR (@Mode = 'DATE_RANGE' AND CAST(M.PurchaseDate AS DATE) >= @FromDate AND CAST(M.PurchaseDate AS DATE) <= @ToDate)
        OR (@Mode = 'MONTH_WISE' AND ((@Year = 0 OR YEAR(M.PurchaseDate) = @Year) AND (@Month = 0 OR MONTH(M.PurchaseDate) = @Month)))
        OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(M.PurchaseDate) = @Year))
      )
GROUP BY CAST(M.PurchaseDate AS DATE), D.ItemID, D.BarCode, D.ItemName, D.Unit
ORDER BY CAST(M.PurchaseDate AS DATE) DESC, D.ItemName ASC;";
            }
            else if (view == "ITEM_SUMMARY")
            {
                sql = @"
SELECT 
    MAX(M.PurchaseDate) AS PurchaseDate,
    CAST(0 AS BIGINT) AS PurchaseNo,
    'Item Summary' AS InvoiceNo,
    MAX(M.PurchaseDate) AS InvoiceDate,
    'Multiple Vendors' AS VendorName,
    0 AS VendorLedgerId,
    1 AS SlNo,
    ISNULL(D.ItemID, 0) AS ItemID,
    ISNULL(D.BarCode, '') AS BarCode,
    ISNULL(D.ItemName, '') AS ItemName,
    ISNULL(D.Unit, '') AS Unit,
    '1' AS Packing,
    CAST(SUM(ISNULL(D.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
    CAST(SUM(ISNULL(D.Free, 0)) AS DECIMAL(18,2)) AS Free,
    CAST(SUM(ISNULL(D.Qty, 0) + ISNULL(D.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
    CAST(CASE WHEN SUM(ISNULL(D.Qty, 0)) > 0 THEN SUM(ISNULL(D.Amount, 0)) / SUM(ISNULL(D.Qty, 0)) ELSE MAX(ISNULL(D.Cost, 0)) END AS DECIMAL(18,2)) AS Cost,
    CAST(SUM(ISNULL(D.Amount, 0)) AS DECIMAL(18,2)) AS Amount,
    CAST(SUM(ISNULL(D.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
    CAST(SUM(ISNULL(D.Amount, 0) + ISNULL(D.TaxAmt, 0)) AS DECIMAL(18,2)) AS NetAmount,
    '' AS Paymode,
    '' AS BilledBy
FROM dbo.PMaster M WITH (NOLOCK)
INNER JOIN dbo.PDetails D WITH (NOLOCK) ON M.PurchaseNo = D.PurchaseNo
LEFT JOIN dbo.Ledger L WITH (NOLOCK) ON M.LedgerID = L.LedgerID
WHERE ISNULL(M.CancelFlag, 0) = 0
  AND (@CompanyId = 0 OR ISNULL(M.CompanyId, 0) = @CompanyId)
  AND (@BranchId = 0 OR ISNULL(M.BranchId, 0) = @BranchId)
  AND (@FinYearId = 0 OR ISNULL(M.FinYearId, 0) = @FinYearId)
  AND (@VendorLedgerId = 0 OR ISNULL(M.LedgerID, 0) = @VendorLedgerId)
  AND (@ItemSearch IS NULL OR D.ItemName LIKE '%' + @ItemSearch + '%' OR D.BarCode LIKE '%' + @ItemSearch + '%')
  AND (
        @Mode = 'ALL'
        OR (@Mode = 'DATE_RANGE' AND CAST(M.PurchaseDate AS DATE) >= @FromDate AND CAST(M.PurchaseDate AS DATE) <= @ToDate)
        OR (@Mode = 'MONTH_WISE' AND ((@Year = 0 OR YEAR(M.PurchaseDate) = @Year) AND (@Month = 0 OR MONTH(M.PurchaseDate) = @Month)))
        OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(M.PurchaseDate) = @Year))
      )
GROUP BY D.ItemID, D.BarCode, D.ItemName, D.Unit
ORDER BY D.ItemName ASC;";
            }
            else
            {
                sql = @"
SELECT 
    M.PurchaseDate,
    M.PurchaseNo,
    ISNULL(M.InvoiceNo, '') AS InvoiceNo,
    ISNULL(M.InvoiceDate, M.PurchaseDate) AS InvoiceDate,
    ISNULL(NULLIF(M.VendorName, ''), ISNULL(L.LedgerName, 'Unknown Vendor')) AS VendorName,
    ISNULL(M.LedgerID, 0) AS VendorLedgerId,
    ISNULL(D.SlNo, 1) AS SlNo,
    ISNULL(D.ItemID, 0) AS ItemID,
    ISNULL(D.BarCode, '') AS BarCode,
    ISNULL(D.ItemName, '') AS ItemName,
    ISNULL(D.Unit, '') AS Unit,
    CAST(ISNULL(D.Packing, '1') AS NVARCHAR(50)) AS Packing,
    CAST(ISNULL(D.Qty, 0) AS DECIMAL(18,2)) AS Qty,
    CAST(ISNULL(D.Free, 0) AS DECIMAL(18,2)) AS Free,
    CAST((ISNULL(D.Qty, 0) + ISNULL(D.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
    CAST(ISNULL(D.Cost, 0) AS DECIMAL(18,2)) AS Cost,
    CAST(ISNULL(D.Amount, 0) AS DECIMAL(18,2)) AS Amount,
    CAST(ISNULL(D.TaxAmt, 0) AS DECIMAL(18,2)) AS TaxAmt,
    CAST((ISNULL(D.Amount, 0) + ISNULL(D.TaxAmt, 0)) AS DECIMAL(18,2)) AS NetAmount,
    ISNULL(M.Paymode, '') AS Paymode,
    ISNULL(M.BilledBy, '') AS BilledBy
FROM dbo.PMaster M WITH (NOLOCK)
INNER JOIN dbo.PDetails D WITH (NOLOCK) ON M.PurchaseNo = D.PurchaseNo
LEFT JOIN dbo.Ledger L WITH (NOLOCK) ON M.LedgerID = L.LedgerID
WHERE ISNULL(M.CancelFlag, 0) = 0
  AND (@CompanyId = 0 OR ISNULL(M.CompanyId, 0) = @CompanyId)
  AND (@BranchId = 0 OR ISNULL(M.BranchId, 0) = @BranchId)
  AND (@FinYearId = 0 OR ISNULL(M.FinYearId, 0) = @FinYearId)
  AND (@VendorLedgerId = 0 OR ISNULL(M.LedgerID, 0) = @VendorLedgerId)
  AND (@ItemSearch IS NULL OR D.ItemName LIKE '%' + @ItemSearch + '%' OR D.BarCode LIKE '%' + @ItemSearch + '%')
  AND (
        @Mode = 'ALL'
        OR (@Mode = 'DATE_RANGE' AND CAST(M.PurchaseDate AS DATE) >= @FromDate AND CAST(M.PurchaseDate AS DATE) <= @ToDate)
        OR (@Mode = 'MONTH_WISE' AND ((@Year = 0 OR YEAR(M.PurchaseDate) = @Year) AND (@Month = 0 OR MONTH(M.PurchaseDate) = @Month)))
        OR (@Mode = 'YEAR_WISE' AND (@Year = 0 OR YEAR(M.PurchaseDate) = @Year))
      )
ORDER BY M.PurchaseDate DESC, M.PurchaseNo DESC, D.SlNo ASC;";
            }

            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
            {
                cmd.CommandTimeout = 180;
                cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = companyId;
                cmd.Parameters.Add("@BranchId", SqlDbType.Int).Value = branchId;
                cmd.Parameters.Add("@FinYearId", SqlDbType.Int).Value = finYearId;
                cmd.Parameters.Add("@VendorLedgerId", SqlDbType.Int).Value = filter.VendorLedgerId;
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

        private static List<DatewisePurchaseReportRow> MapDataTableToReportRows(DataTable table)
        {
            List<DatewisePurchaseReportRow> rows = new List<DatewisePurchaseReportRow>();
            if (table == null || table.Rows.Count == 0) return rows;

            foreach (DataRow r in table.Rows)
            {
                rows.Add(new DatewisePurchaseReportRow
                {
                    PurchaseDate = ToDateTime(r, "PurchaseDate"),
                    PurchaseNo = ToLong(r, "PurchaseNo"),
                    InvoiceNo = ToString(r, "InvoiceNo"),
                    InvoiceDate = ToDateTime(r, "InvoiceDate"),
                    VendorName = ToString(r, "VendorName"),
                    VendorLedgerId = ToInt(r, "VendorLedgerId"),
                    SlNo = ToInt(r, "SlNo"),
                    ItemID = ToInt(r, "ItemID"),
                    BarCode = ToString(r, "BarCode"),
                    ItemName = ToString(r, "ItemName"),
                    Unit = ToString(r, "Unit"),
                    Packing = ToString(r, "Packing"),
                    Qty = ToDecimal(r, "Qty"),
                    Free = ToDecimal(r, "Free"),
                    TotalQty = ToDecimal(r, "TotalQty"),
                    Cost = ToDecimal(r, "Cost"),
                    Amount = ToDecimal(r, "Amount"),
                    TaxAmt = ToDecimal(r, "TaxAmt"),
                    NetAmount = ToDecimal(r, "NetAmount"),
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
