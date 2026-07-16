using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Repository.ReportRepository
{
    public class StockAdjustmentReportRepository : BaseRepostitory
    {
        public List<StockAdjustmentReportRow> GetStockAdjustmentReport(StockAdjustmentReportFilter filter)
        {
            var reportData = new List<StockAdjustmentReportRow>();

            const string sql = @"
SELECT
    ROW_NUMBER() OVER (ORDER BY sam.StockAdjustmentDate DESC, sam.StockAdjustmentNo DESC, ISNULL(sad.SlNo, 0)) AS SlNo,
    sam.Id AS StockAdjustmentId,
    ISNULL(sam.StockAdjustmentNo, 0) AS StockAdjustmentNo,
    ISNULL(sam.StockAdjustmentDate, GETDATE()) AS StockAdjustmentDate,
    CASE WHEN ISNULL(sad.QtyDifference, 0) >= 0 THEN 'Stock IN' ELSE 'Stock OUT' END AS AdjustmentType,
    ISNULL(im.BarCode, '') AS Barcode,
    ISNULL(sad.ItemId, 0) AS ItemId,
    ISNULL(im.Description, '') AS ItemName,
    ISNULL(um.UnitName, '') AS UnitName,
    CAST(ISNULL(sad.SystemStock, 0) AS decimal(18, 4)) AS SystemStock,
    CAST(ISNULL(sad.PhysicalStock, 0) AS decimal(18, 4)) AS PhysicalStock,
    CAST(ISNULL(sad.QtyDifference, 0) AS decimal(18, 4)) AS QtyDifference,
    CAST(CASE WHEN ISNULL(sad.QtyDifference, 0) > 0 THEN sad.QtyDifference ELSE 0 END AS decimal(18, 4)) AS StockInQty,
    CAST(CASE WHEN ISNULL(sad.QtyDifference, 0) < 0 THEN ABS(sad.QtyDifference) ELSE 0 END AS decimal(18, 4)) AS StockOutQty,
    CAST(ISNULL(sad.Cost, 0) AS decimal(18, 4)) AS Cost,
    CAST(ISNULL(sad.QtyDifference, 0) * ISNULL(sad.Cost, 0) AS decimal(18, 4)) AS AdjustmentValue,
    COALESCE(NULLIF(sad.Reason, ''), NULLIF(sam.Comments, ''), NULLIF(lm.LedgerName, ''), '') AS Reason,
    ISNULL(lm.LedgerName, '') AS LedgerName,
    COALESCE(NULLIF(u.UserName, ''), NULLIF(CONVERT(nvarchar(50), sam.UserId), '0'), '') AS UserName,
    ISNULL(sam.Comments, '') AS Comments
FROM dbo.StockAdjustmentMaster sam
INNER JOIN dbo.StockAdjustmentDetails sad ON sad.StockAdjustmentMasterId = sam.Id
LEFT JOIN dbo.ItemMaster im ON im.ItemId = sad.ItemId
LEFT JOIN dbo.UnitMaster um ON um.UnitID = sad.UnitId
LEFT JOIN dbo.LedgerMaster lm ON lm.LedgerID = sam.LedgerId
LEFT JOIN dbo.Users u ON u.UserID = sam.UserId
WHERE ISNULL(sam.CancelFlag, 0) = 0
  AND ISNULL(sad.CancelFlag, 0) = 0
  AND sam.StockAdjustmentDate >= @FromDate
  AND sam.StockAdjustmentDate < DATEADD(DAY, 1, @ToDate)
  AND (@CompanyId = 0 OR ISNULL(sam.CompanyId, 0) = @CompanyId)
  AND (@BranchId = 0 OR ISNULL(sam.BranchId, 0) = @BranchId)
  AND (@FinYearId = 0 OR ISNULL(sam.FinYearId, 0) = @FinYearId)
  AND (@AdjustmentType = '' OR @AdjustmentType = CASE WHEN ISNULL(sad.QtyDifference, 0) >= 0 THEN 'Stock IN' ELSE 'Stock OUT' END)
  AND (
        @SearchQuery = ''
        OR CONVERT(nvarchar(50), sam.StockAdjustmentNo) LIKE '%' + @SearchQuery + '%'
        OR ISNULL(im.BarCode, '') LIKE '%' + @SearchQuery + '%'
        OR ISNULL(im.Description, '') LIKE '%' + @SearchQuery + '%'
        OR ISNULL(sad.Reason, '') LIKE '%' + @SearchQuery + '%'
        OR ISNULL(sam.Comments, '') LIKE '%' + @SearchQuery + '%'
        OR ISNULL(lm.LedgerName, '') LIKE '%' + @SearchQuery + '%'
      )
ORDER BY sam.StockAdjustmentDate DESC, sam.StockAdjustmentNo DESC, ISNULL(sad.SlNo, 0);";

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@CompanyId", filter.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", filter.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", filter.FinYearId);
                    cmd.Parameters.AddWithValue("@FromDate", filter.FromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", filter.ToDate.Date);
                    cmd.Parameters.AddWithValue("@AdjustmentType", string.IsNullOrWhiteSpace(filter.AdjustmentType) ? string.Empty : filter.AdjustmentType);
                    cmd.Parameters.AddWithValue("@SearchQuery", string.IsNullOrWhiteSpace(filter.SearchQuery) ? string.Empty : filter.SearchQuery.Trim());

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            reportData.Add(new StockAdjustmentReportRow
                            {
                                SlNo = ReadLong(row, "SlNo"),
                                StockAdjustmentId = ReadInt(row, "StockAdjustmentId"),
                                StockAdjustmentNo = ReadInt(row, "StockAdjustmentNo"),
                                StockAdjustmentDate = ReadDate(row, "StockAdjustmentDate"),
                                AdjustmentType = ReadString(row, "AdjustmentType"),
                                Barcode = ReadString(row, "Barcode"),
                                ItemId = ReadInt(row, "ItemId"),
                                ItemName = ReadString(row, "ItemName"),
                                UnitName = ReadString(row, "UnitName"),
                                SystemStock = ReadDecimal(row, "SystemStock"),
                                PhysicalStock = ReadDecimal(row, "PhysicalStock"),
                                QtyDifference = ReadDecimal(row, "QtyDifference"),
                                StockInQty = ReadDecimal(row, "StockInQty"),
                                StockOutQty = ReadDecimal(row, "StockOutQty"),
                                Cost = ReadDecimal(row, "Cost"),
                                AdjustmentValue = ReadDecimal(row, "AdjustmentValue"),
                                Reason = ReadString(row, "Reason"),
                                LedgerName = ReadString(row, "LedgerName"),
                                UserName = ReadString(row, "UserName"),
                                Comments = ReadString(row, "Comments")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving stock adjustment report data: " + ex.Message, ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return reportData;
        }

        private static string ReadString(DataRow row, string columnName)
        {
            return row[columnName] == DBNull.Value ? string.Empty : row[columnName].ToString();
        }

        private static int ReadInt(DataRow row, string columnName)
        {
            return row[columnName] == DBNull.Value ? 0 : Convert.ToInt32(row[columnName]);
        }

        private static long ReadLong(DataRow row, string columnName)
        {
            return row[columnName] == DBNull.Value ? 0L : Convert.ToInt64(row[columnName]);
        }

        private static decimal ReadDecimal(DataRow row, string columnName)
        {
            return row[columnName] == DBNull.Value ? 0M : Convert.ToDecimal(row[columnName]);
        }

        private static DateTime ReadDate(DataRow row, string columnName)
        {
            return row[columnName] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(row[columnName]);
        }
    }
}
