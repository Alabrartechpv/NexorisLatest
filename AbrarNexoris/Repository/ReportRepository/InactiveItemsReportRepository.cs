using Dapper;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Repository.ReportRepository
{
    public class InactiveItemsReportRepository : BaseRepostitory
    {
        public List<InactiveItemsReportRow> GetInactiveItemsReport(InactiveItemsReportFilter filter)
        {
            if (filter == null)
            {
                filter = new InactiveItemsReportFilter();
            }

            List<InactiveItemsReportRow> list = new List<InactiveItemsReportRow>();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                string sql = @"
SELECT 
    im.ItemId,
    ISNULL(im.ItemNo, '') AS ItemNo,
    ISNULL(im.Barcode, '') AS Barcode,
    ISNULL(im.Description, '') AS ItemName,
    ISNULL(u.UnitName, '') AS Unit,
    ISNULL(c.CategoryName, '') AS CategoryName,
    ISNULL(g.GroupName, '') AS GroupName,
    ISNULL(b.BrandName, '') AS BrandName,
    ISNULL(im.HSNCode, '') AS HSNCode,
    ISNULL(ps.Stock, 0) AS Stock,
    ISNULL(ps.Cost, 0) AS UnitCost,
    ISNULL(ps.WholeSalePrice, 0) AS RetailPrice,
    ISNULL(ps.RetailPrice, 0) AS WalkinPrice,
    ISNULL(sr.StatusName, 'Inactive') AS ItemStatus,
    ISNULL(sr.StatusReason, '') AS StatusReason,
    sr.StatusDate AS StatusDate,
    ISNULL(logInfo.UserName, 'System') AS InactivatedByUser,
    ISNULL(logInfo.UserId, 0) AS UserId,
    ISNULL(logInfo.CounterName, '') AS CounterName,
    ISNULL(logInfo.CounterId, 0) AS CounterId,
    ISNULL(br.BranchName, '') AS BranchName,
    im.BranchId,
    logInfo.CreatedOn AS CreatedOn
FROM dbo.ItemMaster im WITH (NOLOCK)
INNER JOIN dbo.POS_ItemMasterStatusRules sr WITH (NOLOCK) ON im.ItemId = sr.ItemId
LEFT JOIN dbo.CategoryMaster c WITH (NOLOCK) ON im.CategoryId = c.CategoryId
LEFT JOIN dbo.GroupMaster g WITH (NOLOCK) ON im.GroupId = g.GroupId
LEFT JOIN dbo.BrandMaster b WITH (NOLOCK) ON im.BrandId = b.BrandId
LEFT JOIN dbo.UnitMaster u WITH (NOLOCK) ON im.BaseUnitId = u.UnitId
LEFT JOIN dbo.BranchMaster br WITH (NOLOCK) ON im.BranchId = br.BranchId
LEFT JOIN (
    SELECT ItemId, Stock, Cost, WholeSalePrice, RetailPrice
    FROM (
        SELECT ItemId, Stock, Cost, WholeSalePrice, RetailPrice,
               ROW_NUMBER() OVER (PARTITION BY ItemId ORDER BY ISNULL(IsBaseUnit, 'N') DESC, PriceSettingsId ASC) AS rn
        FROM dbo.PriceSettings WITH (NOLOCK)
    ) p WHERE rn = 1
) ps ON im.ItemId = ps.ItemId
LEFT JOIN (
    SELECT ItemId, UserName, UserId, CounterName, CounterId, CreatedOn
    FROM (
        SELECT ItemId, UserName, UserId, CounterName, CounterId, CreatedOn,
               ROW_NUMBER() OVER (PARTITION BY ItemId ORDER BY ItemActivityLogId DESC) AS rn
        FROM dbo.ItemActivityLog WITH (NOLOCK)
        WHERE ItemStatus = 'Inactive' OR ActivityDetails LIKE '%Inactive%' OR ActivityDetails LIKE '%Status%'
    ) logSub WHERE rn = 1
) logInfo ON im.ItemId = logInfo.ItemId
WHERE UPPER(TRIM(sr.StatusName)) = 'INACTIVE'
  AND (@BranchId <= 0 OR im.BranchId = @BranchId)
  AND (@CompanyId <= 0 OR im.CompanyId = @CompanyId)
  AND (@CategoryId <= 0 OR im.CategoryId = @CategoryId)
  AND (@GroupId <= 0 OR im.GroupId = @GroupId)
  AND (
      @DateFilterMode = 'ALL'
      OR (
          @FromDate IS NOT NULL AND @ToDate IS NOT NULL 
          AND (
              (sr.StatusDate >= @FromDate AND sr.StatusDate < DATEADD(DAY, 1, @ToDate))
              OR (logInfo.CreatedOn >= @FromDate AND logInfo.CreatedOn < DATEADD(DAY, 1, @ToDate))
          )
      )
  )
ORDER BY ISNULL(sr.StatusDate, GETDATE()) DESC, im.Description ASC;
";

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@CompanyId", filter.CompanyId);
                parameters.Add("@BranchId", filter.BranchId);
                parameters.Add("@CategoryId", filter.CategoryId);
                parameters.Add("@GroupId", filter.GroupId);
                parameters.Add("@DateFilterMode", filter.DateFilterMode ?? "ALL");
                parameters.Add("@FromDate", filter.FromDate?.Date);
                parameters.Add("@ToDate", filter.ToDate?.Date);

                list = DataConnection.Query<InactiveItemsReportRow>(sql, parameters, commandType: CommandType.Text).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting inactive items report: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return list;
        }
    }
}
