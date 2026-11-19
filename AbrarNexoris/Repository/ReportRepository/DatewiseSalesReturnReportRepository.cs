using ModelClass;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Repository.ReportRepository
{
    public class DatewiseSalesReturnReportRepository : BaseRepostitory
    {
        private static bool _procedureEnsured = false;
        private static readonly object _procedureLock = new object();

        public DatewiseSalesReturnReportRepository()
        {
            EnsureStoredProcedure();
        }

        /// <summary>
        /// Automatically ensures that the POS_DatewiseSalesReturnReport stored procedure exists and is up to date in the database.
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
IF OBJECT_ID(N'[dbo].[POS_DatewiseSalesReturnReport]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[POS_DatewiseSalesReturnReport];

EXEC dbo.sp_executesql @statement = N'
CREATE PROCEDURE [dbo].[POS_DatewiseSalesReturnReport]
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
    @ViewMode          VARCHAR(30)   = ''DETAILED''
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
            SRM.SReturnDate AS ReturnDate,
            SRM.SReturnNo AS ReturnNo,
            ISNULL(SRM.InvoiceNo, '''') AS InvoiceNo,
            ISNULL(SRM.InvoiceDate, SRM.SReturnDate) AS InvoiceDate,
            ISNULL(NULLIF(LM.LedgerName, ''''), ISNULL(NULLIF(SRM.CustomerName, ''''), ''Cash Customer'')) AS CustomerName,
            ISNULL(SRM.LedgerID, 0) AS CustomerLedgerId,
            ISNULL(SRD.SlNo, 1) AS SlNo,
            ISNULL(SRD.ItemID, 0) AS ItemID,
            ISNULL(PS.BarCode, ISNULL(IM.Barcode, '''')) AS BarCode,
            ISNULL(SRD.ItemName, '''') AS ItemName,
            ISNULL(SRD.Unit, '''') AS Unit,
            CAST(ISNULL(SRD.Packing, 1) AS NVARCHAR(50)) AS Packing,
            CAST(ISNULL(SRD.Qty, 0) AS DECIMAL(18,2)) AS Qty,
            CAST(CASE 
                WHEN ISNULL(SRD.ReturnQty, 0) > 0 THEN SRD.ReturnQty
                WHEN ISNULL(SRD.ReturnedQty, 0) > 0 THEN SRD.ReturnedQty
                ELSE ISNULL(SRD.Qty, 0)
            END AS DECIMAL(18,2)) AS Returned,
            CAST(ISNULL(SRD.Free, 0) AS DECIMAL(18,2)) AS Free,
            CAST((ISNULL(SRD.Qty, 0) + ISNULL(SRD.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
            CAST(ISNULL(SRD.SalesPrice, 0) AS DECIMAL(18,2)) AS Rate,
            CAST((CASE 
                WHEN ISNULL(SRD.ReturnQty, 0) > 0 THEN SRD.ReturnQty
                WHEN ISNULL(SRD.ReturnedQty, 0) > 0 THEN SRD.ReturnedQty
                ELSE ISNULL(SRD.Qty, 0)
            END * ISNULL(SRD.SalesPrice, 0)) AS DECIMAL(18,2)) AS Amount,
            CAST(ISNULL(SRD.DisAmt, 0) AS DECIMAL(18,2)) AS DisAmt,
            CAST(ISNULL(SRD.TaxAmt, 0) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(ISNULL(SRD.TotalSP, ((CASE 
                WHEN ISNULL(SRD.ReturnQty, 0) > 0 THEN SRD.ReturnQty
                WHEN ISNULL(SRD.ReturnedQty, 0) > 0 THEN SRD.ReturnedQty
                ELSE ISNULL(SRD.Qty, 0)
            END * ISNULL(SRD.SalesPrice, 0)) + ISNULL(SRD.TaxAmt, 0) - ISNULL(SRD.DisAmt, 0))) AS DECIMAL(18,2)) AS NetAmount,
            ISNULL(SRD.Reason, '''') AS Reason,
            ISNULL(SRM.Paymode, ''CASH'') AS Paymode,
            ISNULL(SRM.BranchId, 0) AS BranchId,
            ISNULL(B.BranchName, '''') AS BranchName,
            ISNULL(SRM.UserName, '''') AS BilledBy
        FROM dbo.SReturnMaster SRM WITH (NOLOCK)
        INNER JOIN dbo.SReturnDetails SRD WITH (NOLOCK) 
            ON SRM.SReturnNo = SRD.SReturnNo 
           AND SRM.BranchId = SRD.BranchID
        LEFT JOIN dbo.LedgerMaster LM WITH (NOLOCK) ON LM.LedgerID = SRM.LedgerID
        LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemID = SRD.ItemID
        LEFT JOIN dbo.Branches B WITH (NOLOCK) ON B.Id = SRM.BranchId
        OUTER APPLY (
            SELECT TOP 1 ps.BarCode
            FROM dbo.PriceSettings ps WITH (NOLOCK)
            WHERE ps.ItemId = SRD.ItemID
              AND (@BranchId = 0 OR ps.BranchId = @BranchId)
            ORDER BY CASE WHEN ps.UnitId = SRD.UnitId THEN 0 WHEN ps.IsBaseUnit = ''Y'' THEN 1 ELSE 2 END
        ) PS
        WHERE ISNULL(SRM.CancelFlag, 0) = 0
          AND (@CompanyId = 0 OR ISNULL(SRM.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(SRM.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(SRM.FinYearId, 0) = @FinYearId)
          AND (@CustomerLedgerId = 0 OR ISNULL(SRM.LedgerID, 0) = @CustomerLedgerId)
          AND (
                @CleanSearch IS NULL 
                OR SRD.ItemName LIKE ''%'' + @CleanSearch + ''%'' 
                OR PS.BarCode LIKE ''%'' + @CleanSearch + ''%'' 
                OR SRM.InvoiceNo LIKE ''%'' + @CleanSearch + ''%''
                OR CAST(SRM.SReturnNo AS VARCHAR(50)) LIKE ''%'' + @CleanSearch + ''%''
              )
          AND (
                @Mode = ''ALL''
                OR (@Mode = ''DATE_RANGE'' AND CAST(SRM.SReturnDate AS DATE) >= @StartDate AND CAST(SRM.SReturnDate AS DATE) <= @EndDate)
                OR (@Mode = ''MONTH_WISE'' AND (
                    (@Year = 0 OR YEAR(SRM.SReturnDate) = @Year) AND
                    (@Month = 0 OR MONTH(SRM.SReturnDate) = @Month)
                ))
                OR (@Mode = ''YEAR_WISE'' AND (@Year = 0 OR YEAR(SRM.SReturnDate) = @Year))
              )
        ORDER BY SRM.SReturnDate DESC, SRM.SReturnNo DESC, SRD.SlNo ASC;
    END
    ELSE IF @View = ''DATE_ITEM_SUMMARY'' OR @View = ''DATE_MATRIX''
    BEGIN
        SELECT 
            CAST(SRM.SReturnDate AS DATE) AS ReturnDate,
            SRD.ItemID,
            ISNULL(PS.BarCode, ISNULL(IM.Barcode, '''')) AS BarCode,
            SRD.ItemName,
            SRD.Unit,
            COUNT(DISTINCT SRM.SReturnNo) AS ReturnCount,
            CAST(SUM(ISNULL(SRD.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
            CAST(SUM(CASE 
                WHEN ISNULL(SRD.ReturnQty, 0) > 0 THEN SRD.ReturnQty
                WHEN ISNULL(SRD.ReturnedQty, 0) > 0 THEN SRD.ReturnedQty
                ELSE ISNULL(SRD.Qty, 0)
            END) AS DECIMAL(18,2)) AS Returned,
            CAST(SUM(ISNULL(SRD.Free, 0)) AS DECIMAL(18,2)) AS Free,
            CAST(SUM(ISNULL(SRD.Qty, 0) + ISNULL(SRD.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
            CAST(SUM(CASE 
                WHEN ISNULL(SRD.ReturnQty, 0) > 0 THEN SRD.ReturnQty
                WHEN ISNULL(SRD.ReturnedQty, 0) > 0 THEN SRD.ReturnedQty
                ELSE ISNULL(SRD.Qty, 0)
            END * ISNULL(SRD.SalesPrice, 0)) AS DECIMAL(18,2)) AS Amount,
            CAST(SUM(ISNULL(SRD.DisAmt, 0)) AS DECIMAL(18,2)) AS DisAmt,
            CAST(SUM(ISNULL(SRD.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(SUM(ISNULL(SRD.TotalSP, ((CASE 
                WHEN ISNULL(SRD.ReturnQty, 0) > 0 THEN SRD.ReturnQty
                WHEN ISNULL(SRD.ReturnedQty, 0) > 0 THEN SRD.ReturnedQty
                ELSE ISNULL(SRD.Qty, 0)
            END * ISNULL(SRD.SalesPrice, 0)) + ISNULL(SRD.TaxAmt, 0) - ISNULL(SRD.DisAmt, 0)))) AS DECIMAL(18,2)) AS NetAmount
        FROM dbo.SReturnMaster SRM WITH (NOLOCK)
        INNER JOIN dbo.SReturnDetails SRD WITH (NOLOCK) 
            ON SRM.SReturnNo = SRD.SReturnNo 
           AND SRM.BranchId = SRD.BranchID
        LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemID = SRD.ItemID
        OUTER APPLY (
            SELECT TOP 1 ps.BarCode
            FROM dbo.PriceSettings ps WITH (NOLOCK)
            WHERE ps.ItemId = SRD.ItemID
              AND (@BranchId = 0 OR ps.BranchId = @BranchId)
            ORDER BY CASE WHEN ps.UnitId = SRD.UnitId THEN 0 WHEN ps.IsBaseUnit = ''Y'' THEN 1 ELSE 2 END
        ) PS
        WHERE ISNULL(SRM.CancelFlag, 0) = 0
          AND (@CompanyId = 0 OR ISNULL(SRM.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(SRM.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(SRM.FinYearId, 0) = @FinYearId)
          AND (@CustomerLedgerId = 0 OR ISNULL(SRM.LedgerID, 0) = @CustomerLedgerId)
          AND (
                @CleanSearch IS NULL 
                OR SRD.ItemName LIKE ''%'' + @CleanSearch + ''%'' 
                OR PS.BarCode LIKE ''%'' + @CleanSearch + ''%'' 
                OR SRM.InvoiceNo LIKE ''%'' + @CleanSearch + ''%''
                OR CAST(SRM.SReturnNo AS VARCHAR(50)) LIKE ''%'' + @CleanSearch + ''%''
              )
          AND (
                @Mode = ''ALL''
                OR (@Mode = ''DATE_RANGE'' AND CAST(SRM.SReturnDate AS DATE) >= @StartDate AND CAST(SRM.SReturnDate AS DATE) <= @EndDate)
                OR (@Mode = ''MONTH_WISE'' AND (
                    (@Year = 0 OR YEAR(SRM.SReturnDate) = @Year) AND
                    (@Month = 0 OR MONTH(SRM.SReturnDate) = @Month)
                ))
                OR (@Mode = ''YEAR_WISE'' AND (@Year = 0 OR YEAR(SRM.SReturnDate) = @Year))
              )
        GROUP BY 
            CAST(SRM.SReturnDate AS DATE),
            SRD.ItemID,
            ISNULL(PS.BarCode, ISNULL(IM.Barcode, '''')),
            SRD.ItemName,
            SRD.Unit
        ORDER BY ReturnDate DESC, NetAmount DESC;
    END
    ELSE IF @View = ''ITEM_SUMMARY''
    BEGIN
        SELECT 
            SRD.ItemID,
            ISNULL(PS.BarCode, ISNULL(IM.Barcode, '''')) AS BarCode,
            SRD.ItemName,
            SRD.Unit,
            COUNT(DISTINCT SRM.SReturnNo) AS ReturnCount,
            CAST(SUM(ISNULL(SRD.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
            CAST(SUM(CASE 
                WHEN ISNULL(SRD.ReturnQty, 0) > 0 THEN SRD.ReturnQty
                WHEN ISNULL(SRD.ReturnedQty, 0) > 0 THEN SRD.ReturnedQty
                ELSE ISNULL(SRD.Qty, 0)
            END) AS DECIMAL(18,2)) AS Returned,
            CAST(SUM(ISNULL(SRD.Free, 0)) AS DECIMAL(18,2)) AS Free,
            CAST(SUM(ISNULL(SRD.Qty, 0) + ISNULL(SRD.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
            CAST(SUM(CASE 
                WHEN ISNULL(SRD.ReturnQty, 0) > 0 THEN SRD.ReturnQty
                WHEN ISNULL(SRD.ReturnedQty, 0) > 0 THEN SRD.ReturnedQty
                ELSE ISNULL(SRD.Qty, 0)
            END * ISNULL(SRD.SalesPrice, 0)) AS DECIMAL(18,2)) AS Amount,
            CAST(SUM(ISNULL(SRD.DisAmt, 0)) AS DECIMAL(18,2)) AS DisAmt,
            CAST(SUM(ISNULL(SRD.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(SUM(ISNULL(SRD.TotalSP, ((CASE 
                WHEN ISNULL(SRD.ReturnQty, 0) > 0 THEN SRD.ReturnQty
                WHEN ISNULL(SRD.ReturnedQty, 0) > 0 THEN SRD.ReturnedQty
                ELSE ISNULL(SRD.Qty, 0)
            END * ISNULL(SRD.SalesPrice, 0)) + ISNULL(SRD.TaxAmt, 0) - ISNULL(SRD.DisAmt, 0)))) AS DECIMAL(18,2)) AS NetAmount
        FROM dbo.SReturnMaster SRM WITH (NOLOCK)
        INNER JOIN dbo.SReturnDetails SRD WITH (NOLOCK) 
            ON SRM.SReturnNo = SRD.SReturnNo 
           AND SRM.BranchId = SRD.BranchID
        LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemID = SRD.ItemID
        OUTER APPLY (
            SELECT TOP 1 ps.BarCode
            FROM dbo.PriceSettings ps WITH (NOLOCK)
            WHERE ps.ItemId = SRD.ItemID
              AND (@BranchId = 0 OR ps.BranchId = @BranchId)
            ORDER BY CASE WHEN ps.UnitId = SRD.UnitId THEN 0 WHEN ps.IsBaseUnit = ''Y'' THEN 1 ELSE 2 END
        ) PS
        WHERE ISNULL(SRM.CancelFlag, 0) = 0
          AND (@CompanyId = 0 OR ISNULL(SRM.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(SRM.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(SRM.FinYearId, 0) = @FinYearId)
          AND (@CustomerLedgerId = 0 OR ISNULL(SRM.LedgerID, 0) = @CustomerLedgerId)
          AND (
                @CleanSearch IS NULL 
                OR SRD.ItemName LIKE ''%'' + @CleanSearch + ''%'' 
                OR PS.BarCode LIKE ''%'' + @CleanSearch + ''%'' 
                OR SRM.InvoiceNo LIKE ''%'' + @CleanSearch + ''%''
                OR CAST(SRM.SReturnNo AS VARCHAR(50)) LIKE ''%'' + @CleanSearch + ''%''
              )
          AND (
                @Mode = ''ALL''
                OR (@Mode = ''DATE_RANGE'' AND CAST(SRM.SReturnDate AS DATE) >= @StartDate AND CAST(SRM.SReturnDate AS DATE) <= @EndDate)
                OR (@Mode = ''MONTH_WISE'' AND (
                    (@Year = 0 OR YEAR(SRM.SReturnDate) = @Year) AND
                    (@Month = 0 OR MONTH(SRM.SReturnDate) = @Month)
                ))
                OR (@Mode = ''YEAR_WISE'' AND (@Year = 0 OR YEAR(SRM.SReturnDate) = @Year))
              )
        GROUP BY 
            SRD.ItemID,
            ISNULL(PS.BarCode, ISNULL(IM.Barcode, '''')),
            SRD.ItemName,
            SRD.Unit
        ORDER BY NetAmount DESC;
    END
    ELSE IF @View = ''CUSTOMER_SUMMARY''
    BEGIN
        SELECT 
            ISNULL(SRM.LedgerID, 0) AS CustomerLedgerId,
            ISNULL(NULLIF(LM.LedgerName, ''''), ISNULL(NULLIF(SRM.CustomerName, ''''), ''Cash Customer'')) AS CustomerName,
            COUNT(DISTINCT SRM.SReturnNo) AS ReturnCount,
            CAST(SUM(ISNULL(SRD.Qty, 0)) AS DECIMAL(18,2)) AS Qty,
            CAST(SUM(CASE 
                WHEN ISNULL(SRD.ReturnQty, 0) > 0 THEN SRD.ReturnQty
                WHEN ISNULL(SRD.ReturnedQty, 0) > 0 THEN SRD.ReturnedQty
                ELSE ISNULL(SRD.Qty, 0)
            END) AS DECIMAL(18,2)) AS Returned,
            CAST(SUM(ISNULL(SRD.Free, 0)) AS DECIMAL(18,2)) AS Free,
            CAST(SUM(ISNULL(SRD.Qty, 0) + ISNULL(SRD.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
            CAST(SUM(CASE 
                WHEN ISNULL(SRD.ReturnQty, 0) > 0 THEN SRD.ReturnQty
                WHEN ISNULL(SRD.ReturnedQty, 0) > 0 THEN SRD.ReturnedQty
                ELSE ISNULL(SRD.Qty, 0)
            END * ISNULL(SRD.SalesPrice, 0)) AS DECIMAL(18,2)) AS Amount,
            CAST(SUM(ISNULL(SRD.DisAmt, 0)) AS DECIMAL(18,2)) AS DisAmt,
            CAST(SUM(ISNULL(SRD.TaxAmt, 0)) AS DECIMAL(18,2)) AS TaxAmt,
            CAST(SUM(ISNULL(SRD.TotalSP, ((CASE 
                WHEN ISNULL(SRD.ReturnQty, 0) > 0 THEN SRD.ReturnQty
                WHEN ISNULL(SRD.ReturnedQty, 0) > 0 THEN SRD.ReturnedQty
                ELSE ISNULL(SRD.Qty, 0)
            END * ISNULL(SRD.SalesPrice, 0)) + ISNULL(SRD.TaxAmt, 0) - ISNULL(SRD.DisAmt, 0)))) AS DECIMAL(18,2)) AS NetAmount
        FROM dbo.SReturnMaster SRM WITH (NOLOCK)
        INNER JOIN dbo.SReturnDetails SRD WITH (NOLOCK) 
            ON SRM.SReturnNo = SRD.SReturnNo 
           AND SRM.BranchId = SRD.BranchID
        LEFT JOIN dbo.LedgerMaster LM WITH (NOLOCK) ON LM.LedgerID = SRM.LedgerID
        OUTER APPLY (
            SELECT TOP 1 ps.BarCode
            FROM dbo.PriceSettings ps WITH (NOLOCK)
            WHERE ps.ItemId = SRD.ItemID
              AND (@BranchId = 0 OR ps.BranchId = @BranchId)
            ORDER BY CASE WHEN ps.UnitId = SRD.UnitId THEN 0 WHEN ps.IsBaseUnit = ''Y'' THEN 1 ELSE 2 END
        ) PS
        WHERE ISNULL(SRM.CancelFlag, 0) = 0
          AND (@CompanyId = 0 OR ISNULL(SRM.CompanyId, 0) = @CompanyId)
          AND (@BranchId = 0 OR ISNULL(SRM.BranchId, 0) = @BranchId)
          AND (@FinYearId = 0 OR ISNULL(SRM.FinYearId, 0) = @FinYearId)
          AND (@CustomerLedgerId = 0 OR ISNULL(SRM.LedgerID, 0) = @CustomerLedgerId)
          AND (
                @CleanSearch IS NULL 
                OR SRD.ItemName LIKE ''%'' + @CleanSearch + ''%'' 
                OR PS.BarCode LIKE ''%'' + @CleanSearch + ''%'' 
                OR SRM.InvoiceNo LIKE ''%'' + @CleanSearch + ''%''
                OR CAST(SRM.SReturnNo AS VARCHAR(50)) LIKE ''%'' + @CleanSearch + ''%''
              )
          AND (
                @Mode = ''ALL''
                OR (@Mode = ''DATE_RANGE'' AND CAST(SRM.SReturnDate AS DATE) >= @StartDate AND CAST(SRM.SReturnDate AS DATE) <= @EndDate)
                OR (@Mode = ''MONTH_WISE'' AND (
                    (@Year = 0 OR YEAR(SRM.SReturnDate) = @Year) AND
                    (@Month = 0 OR MONTH(SRM.SReturnDate) = @Month)
                ))
                OR (@Mode = ''YEAR_WISE'' AND (@Year = 0 OR YEAR(SRM.SReturnDate) = @Year))
              )
        GROUP BY 
            ISNULL(SRM.LedgerID, 0),
            ISNULL(NULLIF(LM.LedgerName, ''''), ISNULL(NULLIF(SRM.CustomerName, ''''), ''Cash Customer''))
        ORDER BY NetAmount DESC;
    END
END;
';
";
                    using (SqlCommand cmd = new SqlCommand(ddl, (SqlConnection)DataConnection))
                    {
                        cmd.CommandTimeout = 120;
                        cmd.ExecuteNonQuery();
                    }

                    _procedureEnsured = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to ensure POS_DatewiseSalesReturnReport stored procedure: {ex.Message}");
                }
                finally
                {
                    if (DataConnection.State == ConnectionState.Open)
                    {
                        DataConnection.Close();
                    }
                }
            }
        }

        public List<CustomerDDl> GetCustomers()
        {
            try
            {
                Dropdowns dropdowns = new Dropdowns();
                CustomerDDlGrid data = dropdowns.CustomerDDl();
                if (data?.List != null && data.List.Any())
                {
                    return data.List.Where(x => x != null && x.LedgerID > 0).OrderBy(x => x.LedgerName).ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DatewiseSalesReturnReportRepository.GetCustomers: {ex.Message}");
            }
            return new List<CustomerDDl>();
        }

        public List<DatewiseSalesReturnReportRow> GetReport(DatewiseSalesReturnReportFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            List<DatewiseSalesReturnReportRow> list = new List<DatewiseSalesReturnReportRow>();
            bool wasClosed = DataConnection.State == ConnectionState.Closed;

            try
            {
                if (wasClosed) DataConnection.Open();

                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_DatewiseSalesReturnReport, (SqlConnection)DataConnection))
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

        private List<DatewiseSalesReturnReportRow> ExecuteFallbackQuery(DatewiseSalesReturnReportFilter filter)
        {
            List<DatewiseSalesReturnReportRow> list = new List<DatewiseSalesReturnReportRow>();

            int companyId = filter.CompanyId > 0 ? filter.CompanyId : SessionContext.CompanyId;
            int branchId = filter.BranchId > 0 ? filter.BranchId : SessionContext.BranchId;
            int finYearId = filter.FinYearId > 0 ? filter.FinYearId : SessionContext.FinYearId;
            string mode = (filter.PeriodMode ?? "ALL").ToUpperInvariant();

            string sql = @"
SELECT 
    SRM.SReturnDate AS ReturnDate,
    SRM.SReturnNo AS ReturnNo,
    ISNULL(SRM.InvoiceNo, '') AS InvoiceNo,
    ISNULL(SRM.InvoiceDate, SRM.SReturnDate) AS InvoiceDate,
    ISNULL(NULLIF(LM.LedgerName, ''), ISNULL(NULLIF(SRM.CustomerName, ''), 'Cash Customer')) AS CustomerName,
    ISNULL(SRM.LedgerID, 0) AS CustomerLedgerId,
    ISNULL(SRD.SlNo, 1) AS SlNo,
    ISNULL(SRD.ItemID, 0) AS ItemID,
    ISNULL(PS.BarCode, ISNULL(IM.Barcode, '')) AS BarCode,
    ISNULL(SRD.ItemName, '') AS ItemName,
    ISNULL(SRD.Unit, '') AS Unit,
    CAST(ISNULL(SRD.Packing, 1) AS NVARCHAR(50)) AS Packing,
    CAST(ISNULL(SRD.Qty, 0) AS DECIMAL(18,2)) AS Qty,
    CAST(CASE 
        WHEN ISNULL(SRD.ReturnQty, 0) > 0 THEN SRD.ReturnQty
        WHEN ISNULL(SRD.ReturnedQty, 0) > 0 THEN SRD.ReturnedQty
        ELSE ISNULL(SRD.Qty, 0)
    END AS DECIMAL(18,2)) AS Returned,
    CAST(ISNULL(SRD.Free, 0) AS DECIMAL(18,2)) AS Free,
    CAST((ISNULL(SRD.Qty, 0) + ISNULL(SRD.Free, 0)) AS DECIMAL(18,2)) AS TotalQty,
    CAST(ISNULL(SRD.SalesPrice, 0) AS DECIMAL(18,2)) AS Rate,
    CAST((CASE 
        WHEN ISNULL(SRD.ReturnQty, 0) > 0 THEN SRD.ReturnQty
        WHEN ISNULL(SRD.ReturnedQty, 0) > 0 THEN SRD.ReturnedQty
        ELSE ISNULL(SRD.Qty, 0)
    END * ISNULL(SRD.SalesPrice, 0)) AS DECIMAL(18,2)) AS Amount,
    CAST(ISNULL(SRD.DisAmt, 0) AS DECIMAL(18,2)) AS DisAmt,
    CAST(ISNULL(SRD.TaxAmt, 0) AS DECIMAL(18,2)) AS TaxAmt,
    CAST(ISNULL(SRD.TotalSP, ((CASE 
        WHEN ISNULL(SRD.ReturnQty, 0) > 0 THEN SRD.ReturnQty
        WHEN ISNULL(SRD.ReturnedQty, 0) > 0 THEN SRD.ReturnedQty
        ELSE ISNULL(SRD.Qty, 0)
    END * ISNULL(SRD.SalesPrice, 0)) + ISNULL(SRD.TaxAmt, 0) - ISNULL(SRD.DisAmt, 0))) AS DECIMAL(18,2)) AS NetAmount,
    ISNULL(SRD.Reason, '') AS Reason,
    ISNULL(SRM.Paymode, 'CASH') AS Paymode,
    ISNULL(SRM.BranchId, 0) AS BranchId,
    ISNULL(B.BranchName, '') AS BranchName,
    ISNULL(SRM.UserName, '') AS BilledBy
FROM dbo.SReturnMaster SRM WITH (NOLOCK)
INNER JOIN dbo.SReturnDetails SRD WITH (NOLOCK) 
    ON SRM.SReturnNo = SRD.SReturnNo 
   AND SRM.BranchId = SRD.BranchID
LEFT JOIN dbo.LedgerMaster LM WITH (NOLOCK) ON LM.LedgerID = SRM.LedgerID
LEFT JOIN dbo.ItemMaster IM WITH (NOLOCK) ON IM.ItemID = SRD.ItemID
LEFT JOIN dbo.Branches B WITH (NOLOCK) ON B.Id = SRM.BranchId
OUTER APPLY (
    SELECT TOP 1 ps.BarCode
    FROM dbo.PriceSettings ps WITH (NOLOCK)
    WHERE ps.ItemId = SRD.ItemID
      AND (@BranchId = 0 OR ps.BranchId = @BranchId)
    ORDER BY CASE WHEN ps.UnitId = SRD.UnitId THEN 0 WHEN ps.IsBaseUnit = 'Y' THEN 1 ELSE 2 END
) PS
WHERE ISNULL(SRM.CancelFlag, 0) = 0
  AND (@CompanyId = 0 OR ISNULL(SRM.CompanyId, 0) = @CompanyId)
  AND (@BranchId = 0 OR ISNULL(SRM.BranchId, 0) = @BranchId)
  AND (@FinYearId = 0 OR ISNULL(SRM.FinYearId, 0) = @FinYearId)
  AND (@CustomerLedgerId = 0 OR ISNULL(SRM.LedgerID, 0) = @CustomerLedgerId)
  AND (@ItemSearch IS NULL OR SRD.ItemName LIKE '%' + @ItemSearch + '%' OR PS.BarCode LIKE '%' + @ItemSearch + '%' OR SRM.InvoiceNo LIKE '%' + @ItemSearch + '%')
  AND (
        @Mode = 'ALL'
        OR (@Mode = 'DATE_RANGE' AND CAST(SRM.SReturnDate AS DATE) >= @FromDate AND CAST(SRM.SReturnDate AS DATE) <= @ToDate)
      )
ORDER BY SRM.SReturnDate DESC, SRM.SReturnNo DESC, SRD.SlNo ASC;
";

            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
            {
                cmd.Parameters.AddWithValue("@FromDate", filter.FromDate.Date);
                cmd.Parameters.AddWithValue("@ToDate", filter.ToDate.Date);
                cmd.Parameters.AddWithValue("@CompanyId", companyId);
                cmd.Parameters.AddWithValue("@BranchId", branchId);
                cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                cmd.Parameters.AddWithValue("@CustomerLedgerId", filter.CustomerLedgerId);
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

        private List<DatewiseSalesReturnReportRow> MapDataTableToReportRows(DataTable table)
        {
            List<DatewiseSalesReturnReportRow> rows = new List<DatewiseSalesReturnReportRow>();
            if (table == null || table.Rows.Count == 0) return rows;

            foreach (DataRow dr in table.Rows)
            {
                var row = new DatewiseSalesReturnReportRow();

                if (table.Columns.Contains("ReturnDate") && dr["ReturnDate"] != DBNull.Value)
                    row.ReturnDate = Convert.ToDateTime(dr["ReturnDate"]);

                if (table.Columns.Contains("ReturnNo") && dr["ReturnNo"] != DBNull.Value)
                    row.ReturnNo = Convert.ToInt64(dr["ReturnNo"]);

                if (table.Columns.Contains("InvoiceNo") && dr["InvoiceNo"] != DBNull.Value)
                    row.InvoiceNo = Convert.ToString(dr["InvoiceNo"]);

                if (table.Columns.Contains("InvoiceDate") && dr["InvoiceDate"] != DBNull.Value)
                    row.InvoiceDate = Convert.ToDateTime(dr["InvoiceDate"]);

                if (table.Columns.Contains("CustomerName") && dr["CustomerName"] != DBNull.Value)
                    row.CustomerName = Convert.ToString(dr["CustomerName"]);

                if (table.Columns.Contains("CustomerLedgerId") && dr["CustomerLedgerId"] != DBNull.Value)
                    row.CustomerLedgerId = Convert.ToInt32(dr["CustomerLedgerId"]);

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

                if (table.Columns.Contains("Rate") && dr["Rate"] != DBNull.Value)
                    row.Rate = Convert.ToDecimal(dr["Rate"]);

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
