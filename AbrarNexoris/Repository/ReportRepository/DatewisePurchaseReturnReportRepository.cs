using ModelClass;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Repository.ReportRepository
{
    public class DatewisePurchaseReturnReportRepository : BaseRepostitory
    {
        private static bool _procedureEnsured = false;
        private static readonly object _procedureLock = new object();

        public DatewisePurchaseReturnReportRepository()
        {
            EnsureStoredProcedure();
        }

        /// <summary>
        /// Automatically ensures that the POS_DatewisePurchaseReturnReport stored procedure exists and is up to date in the database.
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
IF OBJECT_ID(N'[dbo].[POS_DatewisePurchaseReturnReport]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[POS_DatewisePurchaseReturnReport];

EXEC dbo.sp_executesql @statement = N'
CREATE PROCEDURE [dbo].[POS_DatewisePurchaseReturnReport]
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
            PRM.PReturnDate AS ReturnDate,
            PRM.PReturnNo AS ReturnNo,
            ISNULL(PRM.InvoiceNo, '''') AS InvoiceNo,
            ISNULL(PRM.InvoiceDate, PRM.PReturnDate) AS InvoiceDate,
            ISNULL(NULLIF(LM.LedgerName, ''''), ISNULL(NULLIF(PRM.VendorName, ''''), ''Unknown Vendor'')) AS VendorName,
            ISNULL(PRM.LedgerID, 0) AS VendorLedgerId,
            ISNULL(PRD.SlNo, 1) AS SlNo,
            ISNULL(PRD.ItemID, 0) AS ItemID,
            ISNULL(PS.BarCode, ISNULL(IM.Barcode, '''')) AS BarCode,
            ISNULL(PRD.ItemName, '''') AS ItemName,
            ISNULL(PRD.Unit, '''') AS Unit,
            CAST(ISNULL(PRD.Packing, 1) AS NVARCHAR(50)) AS Packing,
            CAST(ISNULL(PRD.Qty, 0) AS DECIMAL(18,2)) AS Qty,
            CAST(CASE 
                WHEN ISNULL(PRD.Returned, 0) > 0 THEN PRD.Returned
                WHEN ISNULL(PRD.Returnqty, 0) > 0 THEN PRD.Returnqty
                ELSE ISNULL(PRD.Qty, 0)
            END AS DECIMAL(18,2)) AS Returned,
            CAST(ISNULL(PRD.Free, 0) AS DECIMAL(18,2)) AS Free,
            CAST((ISNULL(PRD.Qty, 0) + ISNULL(PRD.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
            CAST(ISNULL(PRD.Cost, 0) AS DECIMAL(18,2)) AS Cost,
            CAST((CASE 
                WHEN ISNULL(PRD.Returned, 0) > 0 THEN PRD.Returned
                WHEN ISNULL(PRD.Returnqty, 0) > 0 THEN PRD.Returnqty
                ELSE ISNULL(PRD.Qty, 0)
            END * ISNULL(PRD.Cost, 0)) AS DECIMAL(18,2)) AS Amount,
            CAST(ISNULL(PRD.DisAmt, 0) AS DECIMAL(18,2)) AS DisAmt,
            CAST(ISNULL(PRD.TaxAmt, 0) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(ISNULL(PRD.TotalSP, ((CASE 
                WHEN ISNULL(PRD.Returned, 0) > 0 THEN PRD.Returned
                WHEN ISNULL(PRD.Returnqty, 0) > 0 THEN PRD.Returnqty
                ELSE ISNULL(PRD.Qty, 0)
            END * ISNULL(PRD.Cost, 0)) + ISNULL(PRD.TaxAmt, 0) - ISNULL(PRD.DisAmt, 0))) AS DECIMAL(18,2)) AS NetAmount,
            ISNULL(PRD.Reason, '''') AS Reason,
            ISNULL(PRM.Paymode, ''CREDIT'') AS Paymode,
            ISNULL(PRM.BranchId, 0) AS BranchId,
            ISNULL(B.BranchName, '''') AS BranchName,
            ISNULL(PRM.UserName, '''') AS BilledBy
        FROM dbo.PReturnMaster PRM WITH (NOLOCK)
        INNER JOIN dbo.PReturnDetails PRD WITH (NOLOCK) 
            ON PRM.PReturnNo = PRD.PReturnNo 
           AND PRM.BranchId = PRD.BranchID
        LEFT JOIN dbo.LedgerMaster LM WITH (NOLOCK) ON LM.LedgerID = PRM.LedgerID
        LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemID = PRD.ItemID
        LEFT JOIN dbo.Branches B WITH (NOLOCK) ON B.Id = PRM.BranchId
        OUTER APPLY (
            SELECT TOP 1 ps.BarCode
            FROM dbo.PriceSettings ps WITH (NOLOCK)
            WHERE ps.ItemId = PRD.ItemID
              AND (@BranchId = 0 OR ps.BranchId = @BranchId)
            ORDER BY CASE WHEN ps.UnitId = PRD.UnitId THEN 0 WHEN ps.IsBaseUnit = ''Y'' THEN 1 ELSE 2 END
        ) PS
        WHERE ISNULL(PRM.CancelFlag, 0) = 0
          AND (@CompanyId = 0 OR ISNULL(PRM.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(PRM.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(PRM.FinYearId, 0) = @FinYearId)
          AND (@VendorLedgerId = 0 OR ISNULL(PRM.LedgerID, 0) = @VendorLedgerId)
          AND (
                @CleanSearch IS NULL 
                OR PRD.ItemName LIKE ''%'' + @CleanSearch + ''%'' 
                OR PS.BarCode LIKE ''%'' + @CleanSearch + ''%'' 
                OR PRM.InvoiceNo LIKE ''%'' + @CleanSearch + ''%''
                OR CAST(PRM.PReturnNo AS VARCHAR(50)) LIKE ''%'' + @CleanSearch + ''%''
              )
          AND (
                @Mode = ''ALL''
                OR (@Mode = ''DATE_RANGE'' AND CAST(PRM.PReturnDate AS DATE) >= @StartDate AND CAST(PRM.PReturnDate AS DATE) <= @EndDate)
                OR (@Mode = ''MONTH_WISE'' AND (
                    (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year) AND
                    (@Month = 0 OR MONTH(PRM.PReturnDate) = @Month)
                ))
                OR (@Mode = ''YEAR_WISE'' AND (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year))
              )
        ORDER BY PRM.PReturnDate DESC, PRM.PReturnNo DESC, PRD.SlNo ASC;
    END
    ELSE IF @View = ''DATE_ITEM_SUMMARY'' OR @View = ''DATE_MATRIX''
    BEGIN
        SELECT 
            CAST(PRM.PReturnDate AS DATE) AS ReturnDate,
            PRD.ItemID,
            ISNULL(PS.BarCode, ISNULL(IM.Barcode, '''')) AS BarCode,
            PRD.ItemName,
            PRD.Unit,
            COUNT(DISTINCT PRM.PReturnNo) AS ReturnCount,
            CAST(SUM(ISNULL(PRD.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
            CAST(SUM(CASE 
                WHEN ISNULL(PRD.Returned, 0) > 0 THEN PRD.Returned
                WHEN ISNULL(PRD.Returnqty, 0) > 0 THEN PRD.Returnqty
                ELSE ISNULL(PRD.Qty, 0)
            END) AS DECIMAL(18,2)) AS Returned,
            CAST(SUM(ISNULL(PRD.Free, 0)) AS DECIMAL(18,2)) AS Free,
            CAST(SUM(ISNULL(PRD.Qty, 0) + ISNULL(PRD.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
            CAST(SUM(CASE 
                WHEN ISNULL(PRD.Returned, 0) > 0 THEN PRD.Returned
                WHEN ISNULL(PRD.Returnqty, 0) > 0 THEN PRD.Returnqty
                ELSE ISNULL(PRD.Qty, 0)
            END * ISNULL(PRD.Cost, 0)) AS DECIMAL(18,2)) AS Amount,
            CAST(SUM(ISNULL(PRD.DisAmt, 0)) AS DECIMAL(18,2)) AS DisAmt,
            CAST(SUM(ISNULL(PRD.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(SUM(ISNULL(PRD.TotalSP, ((CASE 
                WHEN ISNULL(PRD.Returned, 0) > 0 THEN PRD.Returned
                WHEN ISNULL(PRD.Returnqty, 0) > 0 THEN PRD.Returnqty
                ELSE ISNULL(PRD.Qty, 0)
            END * ISNULL(PRD.Cost, 0)) + ISNULL(PRD.TaxAmt, 0) - ISNULL(PRD.DisAmt, 0)))) AS DECIMAL(18,2)) AS NetAmount
        FROM dbo.PReturnMaster PRM WITH (NOLOCK)
        INNER JOIN dbo.PReturnDetails PRD WITH (NOLOCK) 
            ON PRM.PReturnNo = PRD.PReturnNo 
           AND PRM.BranchId = PRD.BranchID
        LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemID = PRD.ItemID
        OUTER APPLY (
            SELECT TOP 1 ps.BarCode
            FROM dbo.PriceSettings ps WITH (NOLOCK)
            WHERE ps.ItemId = PRD.ItemID
              AND (@BranchId = 0 OR ps.BranchId = @BranchId)
            ORDER BY CASE WHEN ps.UnitId = PRD.UnitId THEN 0 WHEN ps.IsBaseUnit = ''Y'' THEN 1 ELSE 2 END
        ) PS
        WHERE ISNULL(PRM.CancelFlag, 0) = 0
          AND (@CompanyId = 0 OR ISNULL(PRM.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(PRM.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(PRM.FinYearId, 0) = @FinYearId)
          AND (@VendorLedgerId = 0 OR ISNULL(PRM.LedgerID, 0) = @VendorLedgerId)
          AND (
                @CleanSearch IS NULL 
                OR PRD.ItemName LIKE ''%'' + @CleanSearch + ''%'' 
                OR PS.BarCode LIKE ''%'' + @CleanSearch + ''%'' 
                OR PRM.InvoiceNo LIKE ''%'' + @CleanSearch + ''%''
              )
          AND (
                @Mode = ''ALL''
                OR (@Mode = ''DATE_RANGE'' AND CAST(PRM.PReturnDate AS DATE) >= @StartDate AND CAST(PRM.PReturnDate AS DATE) <= @EndDate)
                OR (@Mode = ''MONTH_WISE'' AND (
                    (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year) AND
                    (@Month = 0 OR MONTH(PRM.PReturnDate) = @Month)
                ))
                OR (@Mode = ''YEAR_WISE'' AND (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year))
              )
        GROUP BY 
            CAST(PRM.PReturnDate AS DATE),
            PRD.ItemID,
            ISNULL(PS.BarCode, ISNULL(IM.Barcode, '''')),
            PRD.ItemName,
            PRD.Unit
        ORDER BY ReturnDate DESC, NetAmount DESC;
    END
    ELSE IF @View = ''ITEM_SUMMARY''
    BEGIN
        SELECT 
            PRD.ItemID,
            ISNULL(PS.BarCode, ISNULL(IM.Barcode, '''')) AS BarCode,
            PRD.ItemName,
            PRD.Unit,
            COUNT(DISTINCT PRM.PReturnNo) AS ReturnCount,
            CAST(SUM(ISNULL(PRD.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
            CAST(SUM(CASE 
                WHEN ISNULL(PRD.Returned, 0) > 0 THEN PRD.Returned
                WHEN ISNULL(PRD.Returnqty, 0) > 0 THEN PRD.Returnqty
                ELSE ISNULL(PRD.Qty, 0)
            END) AS DECIMAL(18,2)) AS Returned,
            CAST(SUM(ISNULL(PRD.Free, 0)) AS DECIMAL(18,2)) AS Free,
            CAST(SUM(ISNULL(PRD.Qty, 0) + ISNULL(PRD.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
            CAST(SUM(CASE 
                WHEN ISNULL(PRD.Returned, 0) > 0 THEN PRD.Returned
                WHEN ISNULL(PRD.Returnqty, 0) > 0 THEN PRD.Returnqty
                ELSE ISNULL(PRD.Qty, 0)
            END * ISNULL(PRD.Cost, 0)) AS DECIMAL(18,2)) AS Amount,
            CAST(SUM(ISNULL(PRD.DisAmt, 0)) AS DECIMAL(18,2)) AS DisAmt,
            CAST(SUM(ISNULL(PRD.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(SUM(ISNULL(PRD.TotalSP, ((CASE 
                WHEN ISNULL(PRD.Returned, 0) > 0 THEN PRD.Returned
                WHEN ISNULL(PRD.Returnqty, 0) > 0 THEN PRD.Returnqty
                ELSE ISNULL(PRD.Qty, 0)
            END * ISNULL(PRD.Cost, 0)) + ISNULL(PRD.TaxAmt, 0) - ISNULL(PRD.DisAmt, 0)))) AS DECIMAL(18,2)) AS NetAmount
        FROM dbo.PReturnMaster PRM WITH (NOLOCK)
        INNER JOIN dbo.PReturnDetails PRD WITH (NOLOCK) 
            ON PRM.PReturnNo = PRD.PReturnNo 
           AND PRM.BranchId = PRD.BranchID
        LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemID = PRD.ItemID
        OUTER APPLY (
            SELECT TOP 1 ps.BarCode
            FROM dbo.PriceSettings ps WITH (NOLOCK)
            WHERE ps.ItemId = PRD.ItemID
              AND (@BranchId = 0 OR ps.BranchId = @BranchId)
            ORDER BY CASE WHEN ps.UnitId = PRD.UnitId THEN 0 WHEN ps.IsBaseUnit = ''Y'' THEN 1 ELSE 2 END
        ) PS
        WHERE ISNULL(PRM.CancelFlag, 0) = 0
          AND (@CompanyId = 0 OR ISNULL(PRM.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(PRM.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(PRM.FinYearId, 0) = @FinYearId)
          AND (@VendorLedgerId = 0 OR ISNULL(PRM.LedgerID, 0) = @VendorLedgerId)
          AND (
                @CleanSearch IS NULL 
                OR PRD.ItemName LIKE ''%'' + @CleanSearch + ''%'' 
                OR PS.BarCode LIKE ''%'' + @CleanSearch + ''%'' 
                OR PRM.InvoiceNo LIKE ''%'' + @CleanSearch + ''%''
              )
          AND (
                @Mode = ''ALL''
                OR (@Mode = ''DATE_RANGE'' AND CAST(PRM.PReturnDate AS DATE) >= @StartDate AND CAST(PRM.PReturnDate AS DATE) <= @EndDate)
                OR (@Mode = ''MONTH_WISE'' AND (
                    (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year) AND
                    (@Month = 0 OR MONTH(PRM.PReturnDate) = @Month)
                ))
                OR (@Mode = ''YEAR_WISE'' AND (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year))
              )
        GROUP BY 
            PRD.ItemID,
            ISNULL(PS.BarCode, ISNULL(IM.Barcode, '''')),
            PRD.ItemName,
            PRD.Unit
        ORDER BY NetAmount DESC;
    END
    ELSE IF @View = ''VENDOR_SUMMARY''
    BEGIN
        SELECT 
            ISNULL(PRM.LedgerID, 0) AS VendorLedgerId,
            ISNULL(NULLIF(LM.LedgerName, ''''), ISNULL(NULLIF(PRM.VendorName, ''''), ''Unknown Vendor'')) AS VendorName,
            COUNT(DISTINCT PRM.PReturnNo) AS ReturnCount,
            CAST(SUM(ISNULL(PRD.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
            CAST(SUM(CASE 
                WHEN ISNULL(PRD.Returned, 0) > 0 THEN PRD.Returned
                WHEN ISNULL(PRD.Returnqty, 0) > 0 THEN PRD.Returnqty
                ELSE ISNULL(PRD.Qty, 0)
            END) AS DECIMAL(18,2)) AS Returned,
            CAST(SUM(ISNULL(PRD.Free, 0)) AS DECIMAL(18,2)) AS Free,
            CAST(SUM(ISNULL(PRD.Qty, 0) + ISNULL(PRD.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
            CAST(SUM(CASE 
                WHEN ISNULL(PRD.Returned, 0) > 0 THEN PRD.Returned
                WHEN ISNULL(PRD.Returnqty, 0) > 0 THEN PRD.Returnqty
                ELSE ISNULL(PRD.Qty, 0)
            END * ISNULL(PRD.Cost, 0)) AS DECIMAL(18,2)) AS Amount,
            CAST(SUM(ISNULL(PRD.DisAmt, 0)) AS DECIMAL(18,2)) AS DisAmt,
            CAST(SUM(ISNULL(PRD.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(SUM(ISNULL(PRD.TotalSP, ((CASE 
                WHEN ISNULL(PRD.Returned, 0) > 0 THEN PRD.Returned
                WHEN ISNULL(PRD.Returnqty, 0) > 0 THEN PRD.Returnqty
                ELSE ISNULL(PRD.Qty, 0)
            END * ISNULL(PRD.Cost, 0)) + ISNULL(PRD.TaxAmt, 0) - ISNULL(PRD.DisAmt, 0)))) AS DECIMAL(18,2)) AS NetAmount
        FROM dbo.PReturnMaster PRM WITH (NOLOCK)
        INNER JOIN dbo.PReturnDetails PRD WITH (NOLOCK) 
            ON PRM.PReturnNo = PRD.PReturnNo 
           AND PRM.BranchId = PRD.BranchID
        LEFT JOIN dbo.LedgerMaster LM WITH (NOLOCK) ON LM.LedgerID = PRM.LedgerID
        WHERE ISNULL(PRM.CancelFlag, 0) = 0
          AND (@CompanyId = 0 OR ISNULL(PRM.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(PRM.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(PRM.FinYearId, 0) = @FinYearId)
          AND (@VendorLedgerId = 0 OR ISNULL(PRM.LedgerID, 0) = @VendorLedgerId)
          AND (
                @CleanSearch IS NULL 
                OR PRD.ItemName LIKE ''%'' + @CleanSearch + ''%'' 
                OR PRM.InvoiceNo LIKE ''%'' + @CleanSearch + ''%''
              )
          AND (
                @Mode = ''ALL''
                OR (@Mode = ''DATE_RANGE'' AND CAST(PRM.PReturnDate AS DATE) >= @StartDate AND CAST(PRM.PReturnDate AS DATE) <= @EndDate)
                OR (@Mode = ''MONTH_WISE'' AND (
                    (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year) AND
                    (@Month = 0 OR MONTH(PRM.PReturnDate) = @Month)
                ))
                OR (@Mode = ''YEAR_WISE'' AND (@Year = 0 OR YEAR(PRM.PReturnDate) = @Year))
              )
        GROUP BY 
            ISNULL(PRM.LedgerID, 0),
            ISNULL(NULLIF(LM.LedgerName, ''''), ISNULL(NULLIF(PRM.VendorName, ''''), ''Unknown Vendor''))
        ORDER BY NetAmount DESC;
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
        /// Retrieves the Date-wise purchase return report rows based on user filters.
        /// </summary>
        public List<DatewisePurchaseReturnReportRow> GetReport(DatewisePurchaseReturnReportFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            List<DatewisePurchaseReturnReportRow> list = new List<DatewisePurchaseReturnReportRow>();
            bool wasClosed = DataConnection.State == ConnectionState.Closed;

            try
            {
                if (wasClosed) DataConnection.Open();

                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_DatewisePurchaseReturnReport, (SqlConnection)DataConnection))
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

        private List<DatewisePurchaseReturnReportRow> ExecuteFallbackQuery(DatewisePurchaseReturnReportFilter filter)
        {
            List<DatewisePurchaseReturnReportRow> list = new List<DatewisePurchaseReturnReportRow>();

            int companyId = filter.CompanyId > 0 ? filter.CompanyId : SessionContext.CompanyId;
            int branchId = filter.BranchId > 0 ? filter.BranchId : SessionContext.BranchId;
            int finYearId = filter.FinYearId > 0 ? filter.FinYearId : SessionContext.FinYearId;
            string mode = (filter.PeriodMode ?? "ALL").ToUpperInvariant();
            string view = (filter.ViewMode ?? "DETAILED").ToUpperInvariant();

            string sql = @"
SELECT 
    PRM.PReturnDate AS ReturnDate,
    PRM.PReturnNo AS ReturnNo,
    ISNULL(PRM.InvoiceNo, '') AS InvoiceNo,
    ISNULL(PRM.InvoiceDate, PRM.PReturnDate) AS InvoiceDate,
    ISNULL(NULLIF(LM.LedgerName, ''), ISNULL(NULLIF(PRM.VendorName, ''), 'Unknown Vendor')) AS VendorName,
    ISNULL(PRM.LedgerID, 0) AS VendorLedgerId,
    ISNULL(PRD.SlNo, 1) AS SlNo,
    ISNULL(PRD.ItemID, 0) AS ItemID,
    ISNULL(PS.BarCode, ISNULL(IM.Barcode, '')) AS BarCode,
    ISNULL(PRD.ItemName, '') AS ItemName,
    ISNULL(PRD.Unit, '') AS Unit,
    CAST(ISNULL(PRD.Packing, 1) AS NVARCHAR(50)) AS Packing,
    CAST(ISNULL(PRD.Qty, 0) AS DECIMAL(18,2)) AS Qty,
    CAST(CASE 
        WHEN ISNULL(PRD.Returned, 0) > 0 THEN PRD.Returned
        WHEN ISNULL(PRD.Returnqty, 0) > 0 THEN PRD.Returnqty
        ELSE ISNULL(PRD.Qty, 0)
    END AS DECIMAL(18,2)) AS Returned,
    CAST(ISNULL(PRD.Free, 0) AS DECIMAL(18,2)) AS Free,
    CAST((ISNULL(PRD.Qty, 0) + ISNULL(PRD.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
    CAST(ISNULL(PRD.Cost, 0) AS DECIMAL(18,2)) AS Cost,
    CAST((CASE 
        WHEN ISNULL(PRD.Returned, 0) > 0 THEN PRD.Returned
        WHEN ISNULL(PRD.Returnqty, 0) > 0 THEN PRD.Returnqty
        ELSE ISNULL(PRD.Qty, 0)
    END * ISNULL(PRD.Cost, 0)) AS DECIMAL(18,2)) AS Amount,
    CAST(ISNULL(PRD.DisAmt, 0) AS DECIMAL(18,2)) AS DisAmt,
    CAST(ISNULL(PRD.TaxAmt, 0) AS DECIMAL(18,2)) AS TaxAmt,
    CAST(ISNULL(PRD.TotalSP, ((CASE 
        WHEN ISNULL(PRD.Returned, 0) > 0 THEN PRD.Returned
        WHEN ISNULL(PRD.Returnqty, 0) > 0 THEN PRD.Returnqty
        ELSE ISNULL(PRD.Qty, 0)
    END * ISNULL(PRD.Cost, 0)) + ISNULL(PRD.TaxAmt, 0) - ISNULL(PRD.DisAmt, 0))) AS DECIMAL(18,2)) AS NetAmount,
    ISNULL(PRD.Reason, '') AS Reason,
    ISNULL(PRM.Paymode, 'CREDIT') AS Paymode,
    ISNULL(PRM.BranchId, 0) AS BranchId,
    ISNULL(B.BranchName, '') AS BranchName,
    ISNULL(PRM.UserName, '') AS BilledBy
FROM dbo.PReturnMaster PRM WITH (NOLOCK)
INNER JOIN dbo.PReturnDetails PRD WITH (NOLOCK) 
    ON PRM.PReturnNo = PRD.PReturnNo 
   AND PRM.BranchId = PRD.BranchID
LEFT JOIN dbo.LedgerMaster LM WITH (NOLOCK) ON LM.LedgerID = PRM.LedgerID
LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemID = PRD.ItemID
LEFT JOIN dbo.Branches B WITH (NOLOCK) ON B.Id = PRM.BranchId
OUTER APPLY (
    SELECT TOP 1 ps.BarCode
    FROM dbo.PriceSettings ps WITH (NOLOCK)
    WHERE ps.ItemId = PRD.ItemID
      AND (@BranchId = 0 OR ps.BranchId = @BranchId)
    ORDER BY CASE WHEN ps.UnitId = PRD.UnitId THEN 0 WHEN ps.IsBaseUnit = 'Y' THEN 1 ELSE 2 END
) PS
WHERE ISNULL(PRM.CancelFlag, 0) = 0
  AND (@CompanyId = 0 OR ISNULL(PRM.CompanyId, 0) = @CompanyId)
  AND (@BranchId = 0 OR ISNULL(PRM.BranchId, 0) = @BranchId)
  AND (@FinYearId = 0 OR ISNULL(PRM.FinYearId, 0) = @FinYearId)
  AND (@VendorLedgerId = 0 OR ISNULL(PRM.LedgerID, 0) = @VendorLedgerId)
  AND (@ItemSearch IS NULL OR PRD.ItemName LIKE '%' + @ItemSearch + '%' OR PS.BarCode LIKE '%' + @ItemSearch + '%' OR PRM.InvoiceNo LIKE '%' + @ItemSearch + '%')
  AND (
        @Mode = 'ALL'
        OR (@Mode = 'DATE_RANGE' AND CAST(PRM.PReturnDate AS DATE) >= @FromDate AND CAST(PRM.PReturnDate AS DATE) <= @ToDate)
      )
ORDER BY PRM.PReturnDate DESC, PRM.PReturnNo DESC, PRD.SlNo ASC;
";

            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
            {
                cmd.CommandTimeout = 180;
                cmd.Parameters.AddWithValue("@FromDate", filter.FromDate.Date);
                cmd.Parameters.AddWithValue("@ToDate", filter.ToDate.Date);
                cmd.Parameters.AddWithValue("@CompanyId", companyId);
                cmd.Parameters.AddWithValue("@BranchId", branchId);
                cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                cmd.Parameters.AddWithValue("@VendorLedgerId", filter.VendorLedgerId);
                cmd.Parameters.AddWithValue("@ItemSearch", string.IsNullOrWhiteSpace(filter.ItemSearch) ? (object)DBNull.Value : filter.ItemSearch.Trim());
                cmd.Parameters.AddWithValue("@Mode", mode);

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    list = MapDataTableToReportRows(table);
                }
            }

            return list;
        }

        private List<DatewisePurchaseReturnReportRow> MapDataTableToReportRows(DataTable table)
        {
            List<DatewisePurchaseReturnReportRow> rows = new List<DatewisePurchaseReturnReportRow>();
            if (table == null || table.Rows.Count == 0) return rows;

            foreach (DataRow dr in table.Rows)
            {
                var row = new DatewisePurchaseReturnReportRow();

                if (table.Columns.Contains("ReturnDate") && dr["ReturnDate"] != DBNull.Value)
                    row.ReturnDate = Convert.ToDateTime(dr["ReturnDate"]);

                if (table.Columns.Contains("ReturnNo") && dr["ReturnNo"] != DBNull.Value)
                    row.ReturnNo = Convert.ToInt64(dr["ReturnNo"]);

                if (table.Columns.Contains("InvoiceNo") && dr["InvoiceNo"] != DBNull.Value)
                    row.InvoiceNo = Convert.ToString(dr["InvoiceNo"]);

                if (table.Columns.Contains("InvoiceDate") && dr["InvoiceDate"] != DBNull.Value)
                    row.InvoiceDate = Convert.ToDateTime(dr["InvoiceDate"]);

                if (table.Columns.Contains("VendorName") && dr["VendorName"] != DBNull.Value)
                    row.VendorName = Convert.ToString(dr["VendorName"]);

                if (table.Columns.Contains("VendorLedgerId") && dr["VendorLedgerId"] != DBNull.Value)
                    row.VendorLedgerId = Convert.ToInt32(dr["VendorLedgerId"]);

                if (table.Columns.Contains("SlNo") && dr["SlNo"] != DBNull.Value)
                    row.SlNo = Convert.ToInt32(dr["SlNo"]);

                if (table.Columns.Contains("ItemID") && dr["ItemID"] != DBNull.Value)
                    row.ItemID = Convert.ToInt32(dr["ItemID"]);

                if (table.Columns.Contains("BarCode") && dr["BarCode"] != DBNull.Value)
                    row.BarCode = Convert.ToString(dr["BarCode"]);

                if (table.Columns.Contains("ItemName") && dr["ItemName"] != DBNull.Value)
                    row.ItemName = Convert.ToString(dr["ItemName"]);

                if (table.Columns.Contains("Unit") && dr["Unit"] != DBNull.Value)
                    row.Unit = Convert.ToString(dr["Unit"]);

                if (table.Columns.Contains("Packing") && dr["Packing"] != DBNull.Value)
                    row.Packing = Convert.ToString(dr["Packing"]);

                if (table.Columns.Contains("Qty") && dr["Qty"] != DBNull.Value)
                    row.Qty = Convert.ToDecimal(dr["Qty"]);

                if (table.Columns.Contains("Returned") && dr["Returned"] != DBNull.Value)
                    row.Returned = Convert.ToDecimal(dr["Returned"]);

                if (table.Columns.Contains("Free") && dr["Free"] != DBNull.Value)
                    row.Free = Convert.ToDecimal(dr["Free"]);

                if (table.Columns.Contains("TotalQty") && dr["TotalQty"] != DBNull.Value)
                    row.TotalQty = Convert.ToDecimal(dr["TotalQty"]);

                if (table.Columns.Contains("Cost") && dr["Cost"] != DBNull.Value)
                    row.Cost = Convert.ToDecimal(dr["Cost"]);

                if (table.Columns.Contains("Amount") && dr["Amount"] != DBNull.Value)
                    row.Amount = Convert.ToDecimal(dr["Amount"]);

                if (table.Columns.Contains("DisAmt") && dr["DisAmt"] != DBNull.Value)
                    row.DisAmt = Convert.ToDecimal(dr["DisAmt"]);

                if (table.Columns.Contains("TaxAmt") && dr["TaxAmt"] != DBNull.Value)
                    row.TaxAmt = Convert.ToDecimal(dr["TaxAmt"]);

                if (table.Columns.Contains("NetAmount") && dr["NetAmount"] != DBNull.Value)
                    row.NetAmount = Convert.ToDecimal(dr["NetAmount"]);

                if (table.Columns.Contains("Reason") && dr["Reason"] != DBNull.Value)
                    row.Reason = Convert.ToString(dr["Reason"]);

                if (table.Columns.Contains("Paymode") && dr["Paymode"] != DBNull.Value)
                    row.Paymode = Convert.ToString(dr["Paymode"]);

                if (table.Columns.Contains("BranchId") && dr["BranchId"] != DBNull.Value)
                    row.BranchId = Convert.ToInt32(dr["BranchId"]);

                if (table.Columns.Contains("BranchName") && dr["BranchName"] != DBNull.Value)
                    row.BranchName = Convert.ToString(dr["BranchName"]);

                if (table.Columns.Contains("BilledBy") && dr["BilledBy"] != DBNull.Value)
                    row.BilledBy = Convert.ToString(dr["BilledBy"]);

                rows.Add(row);
            }

            return rows;
        }
    }
}
