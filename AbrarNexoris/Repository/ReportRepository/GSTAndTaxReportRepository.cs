using ModelClass;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Repository.ReportRepository
{
    public class GSTAndTaxReportRepository : BaseRepostitory
    {
        public List<GSTAndTaxReportRow> GetReport(GSTAndTaxReportFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            List<GSTAndTaxReportRow> rows = new List<GSTAndTaxReportRow>();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                // 1. Try running Stored Procedure first if present
                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_GSTAndTaxReport, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = filter.CompanyId > 0 ? (object)filter.CompanyId : DBNull.Value;
                        cmd.Parameters.Add("@BranchId", SqlDbType.Int).Value = filter.BranchId > 0 ? (object)filter.BranchId : DBNull.Value;
                        cmd.Parameters.Add("@FinYearId", SqlDbType.Int).Value = filter.FinYearId > 0 ? (object)filter.FinYearId : DBNull.Value;
                        cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = filter.FromDate.Date;
                        cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = filter.ToDate.Date;
                        cmd.Parameters.Add("@TrnsType", SqlDbType.VarChar, 50).Value = string.IsNullOrWhiteSpace(filter.TrnsType) ? (object)DBNull.Value : filter.TrnsType;
                        cmd.Parameters.Add("@TaxType", SqlDbType.VarChar, 20).Value = string.IsNullOrWhiteSpace(filter.TaxType) ? (object)DBNull.Value : filter.TaxType;
                        cmd.Parameters.Add("@SearchText", SqlDbType.VarChar, 100).Value = string.IsNullOrWhiteSpace(filter.SearchText) ? (object)DBNull.Value : filter.SearchText;

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable table = new DataTable();
                            adapter.Fill(table);
                            if (table.Rows.Count > 0)
                            {
                                return MapDataTableToRows(table);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Stored procedure _POS_GSTAndTaxReport failed or missing, executing inline fallback query: {ex.Message}");
                }

                // 2. Direct inline SQL query with table existence guards across SMaster/SDetails, PMaster/PDetails, SRMaster/SRDetails, PRMaster/PRDetails
                List<string> queryBlocks = new List<string>();
                bool hasItemMaster = TableExists("ItemMaster");
                string itemJoin = hasItemMaster ? "LEFT JOIN dbo.ItemMaster i ON d.ItemID = i.ItemID" : "";
                string hsnSelect = hasItemMaster ? "ISNULL(i.HSNCode, '')" : "''";

                if (TableExists("SMaster") && TableExists("SDetails"))
                {
                    queryBlocks.Add($@"
                        SELECT 
                            'Sales Invoice' AS TrnsType,
                            CAST(ISNULL(m.BillNo, 0) AS NVARCHAR(50)) AS InvoiceNo,
                            m.BillDate AS DocDate,
                            ISNULL(NULLIF(m.CustomerName, ''), 'Walk-in Customer') AS PartyName,
                            '' AS PartyGSTIN,
                            ISNULL(d.ItemName, '') AS ItemName,
                            {hsnSelect} AS HSNCode,
                            ISNULL(d.Qty, 0) AS Qty,
                            ISNULL(d.Unit, '') AS Unit,
                            CAST(ISNULL(d.BaseAmount, (ISNULL(d.Qty,0)*ISNULL(d.UnitPrice,0)) - ISNULL(d.TaxAmt,0)) AS DECIMAL(18,2)) AS TaxableAmt,
                            CAST(ISNULL(d.TaxPer, 0) AS FLOAT) AS TaxPer,
                            CAST(ISNULL(d.TaxPer, 0)/2.0 AS FLOAT) AS CGSTPer,
                            CAST(ISNULL(d.TaxAmt, 0)/2.0 AS DECIMAL(18,2)) AS CGSTAmt,
                            CAST(ISNULL(d.TaxPer, 0)/2.0 AS FLOAT) AS SGSTPer,
                            CAST(ISNULL(d.TaxAmt, 0)/2.0 AS DECIMAL(18,2)) AS SGSTAmt,
                            CAST(0 AS FLOAT) AS IGSTPer,
                            CAST(0 AS DECIMAL(18,2)) AS IGSTAmt,
                            CAST(0 AS FLOAT) AS CessPer,
                            CAST(0 AS DECIMAL(18,2)) AS CessAmt,
                            CAST(ISNULL(d.TaxAmt, 0) AS DECIMAL(18,2)) AS TotalTaxAmt,
                            CAST(ISNULL(d.TotalAmount, (ISNULL(d.Qty,0)*ISNULL(d.UnitPrice,0))) AS DECIMAL(18,2)) AS GrandTotal,
                            ISNULL(d.TaxType, 'incl') AS TaxType
                        FROM dbo.SMaster m
                        INNER JOIN dbo.SDetails d ON m.BillNo = d.BillNo AND m.FinYearId = d.FinYearId AND m.BranchId = d.BranchID
                        {itemJoin}
                        WHERE (@CompanyId IS NULL OR m.CompanyId = @CompanyId)
                          AND (@BranchId IS NULL OR m.BranchId = @BranchId)
                          AND (@FinYearId IS NULL OR m.FinYearId = @FinYearId)
                          AND (CAST(m.BillDate AS DATE) BETWEEN @FromDate AND @ToDate)
                          AND (@TrnsType = 'ALL' OR @TrnsType = 'Sales Invoice')
                    ");
                }

                if (TableExists("PMaster") && TableExists("PDetails"))
                {
                    queryBlocks.Add($@"
                        SELECT 
                            'Purchase Invoice' AS TrnsType,
                            ISNULL(m.InvoiceNo, CAST(m.PurchaseNo AS NVARCHAR(50))) AS InvoiceNo,
                            m.InvoiceDate AS DocDate,
                            ISNULL(NULLIF(m.VendorName, ''), 'General Supplier') AS PartyName,
                            '' AS PartyGSTIN,
                            ISNULL(d.ItemName, '') AS ItemName,
                            {hsnSelect} AS HSNCode,
                            ISNULL(d.Qty, 0) AS Qty,
                            ISNULL(d.Unit, '') AS Unit,
                            CAST((ISNULL(d.Qty,0)*ISNULL(d.Cost,0)) - ISNULL(d.TaxAmt,0) AS DECIMAL(18,2)) AS TaxableAmt,
                            CAST(ISNULL(d.TaxPer, 0) AS FLOAT) AS TaxPer,
                            CAST(ISNULL(d.TaxPer, 0)/2.0 AS FLOAT) AS CGSTPer,
                            CAST(ISNULL(d.TaxAmt, 0)/2.0 AS DECIMAL(18,2)) AS CGSTAmt,
                            CAST(ISNULL(d.TaxPer, 0)/2.0 AS FLOAT) AS SGSTPer,
                            CAST(ISNULL(d.TaxAmt, 0)/2.0 AS DECIMAL(18,2)) AS SGSTAmt,
                            CAST(0 AS FLOAT) AS IGSTPer,
                            CAST(0 AS DECIMAL(18,2)) AS IGSTAmt,
                            CAST(ISNULL(d.CessPer, 0) AS FLOAT) AS CessPer,
                            CAST(ISNULL(d.CessAmt, 0) AS DECIMAL(18,2)) AS CessAmt,
                            CAST(ISNULL(d.TaxAmt, 0) AS DECIMAL(18,2)) AS TotalTaxAmt,
                            CAST((ISNULL(d.Qty,0)*ISNULL(d.Cost,0)) AS DECIMAL(18,2)) AS GrandTotal,
                            ISNULL(m.TaxType, 'excl') AS TaxType
                        FROM dbo.PMaster m
                        INNER JOIN dbo.PDetails d ON m.PurchaseNo = d.PurchaseNo AND m.FinYearId = d.FinYearId AND m.BranchId = d.BranchID
                        {itemJoin}
                        WHERE (@CompanyId IS NULL OR m.CompanyId = @CompanyId)
                          AND (@BranchId IS NULL OR m.BranchID = @BranchId)
                          AND (@FinYearId IS NULL OR m.FinYearId = @FinYearId)
                          AND (CAST(m.InvoiceDate AS DATE) BETWEEN @FromDate AND @ToDate)
                          AND (@TrnsType = 'ALL' OR @TrnsType = 'Purchase Invoice')
                    ");
                }

                if (TableExists("SRMaster") && TableExists("SRDetails"))
                {
                    queryBlocks.Add($@"
                        SELECT 
                            'Sales Return' AS TrnsType,
                            CAST(ISNULL(m.SReturnNo, 0) AS NVARCHAR(50)) AS InvoiceNo,
                            m.SReturnDate AS DocDate,
                            ISNULL(NULLIF(m.CustomerName, ''), 'Walk-in Customer') AS PartyName,
                            '' AS PartyGSTIN,
                            ISNULL(d.ItemName, '') AS ItemName,
                            {hsnSelect} AS HSNCode,
                            ISNULL(d.Qty, 0) AS Qty,
                            ISNULL(d.Unit, '') AS Unit,
                            CAST((ISNULL(d.Qty,0)*ISNULL(d.Rate,0)) - ISNULL(d.TaxAmt,0) AS DECIMAL(18,2)) AS TaxableAmt,
                            CAST(ISNULL(d.TaxPer, 0) AS FLOAT) AS TaxPer,
                            CAST(ISNULL(d.TaxPer, 0)/2.0 AS FLOAT) AS CGSTPer,
                            CAST(ISNULL(d.TaxAmt, 0)/2.0 AS DECIMAL(18,2)) AS CGSTAmt,
                            CAST(ISNULL(d.TaxPer, 0)/2.0 AS FLOAT) AS SGSTPer,
                            CAST(ISNULL(d.TaxAmt, 0)/2.0 AS DECIMAL(18,2)) AS SGSTAmt,
                            CAST(0 AS FLOAT) AS IGSTPer,
                            CAST(0 AS DECIMAL(18,2)) AS IGSTAmt,
                            CAST(0 AS FLOAT) AS CessPer,
                            CAST(0 AS DECIMAL(18,2)) AS CessAmt,
                            CAST(ISNULL(d.TaxAmt, 0) AS DECIMAL(18,2)) AS TotalTaxAmt,
                            CAST((ISNULL(d.Qty,0)*ISNULL(d.Rate,0)) AS DECIMAL(18,2)) AS GrandTotal,
                            ISNULL(m.TaxType, 'incl') AS TaxType
                        FROM dbo.SRMaster m
                        INNER JOIN dbo.SRDetails d ON m.SReturnNo = d.SReturnNo AND m.FinYearId = d.FinYearId AND m.BranchId = d.BranchId
                        {itemJoin}
                        WHERE (@CompanyId IS NULL OR m.CompanyId = @CompanyId)
                          AND (@BranchId IS NULL OR m.BranchId = @BranchId)
                          AND (@FinYearId IS NULL OR m.FinYearId = @FinYearId)
                          AND (CAST(m.SReturnDate AS DATE) BETWEEN @FromDate AND @ToDate)
                          AND (@TrnsType = 'ALL' OR @TrnsType = 'Sales Return')
                    ");
                }

                if (TableExists("PRMaster") && TableExists("PRDetails"))
                {
                    queryBlocks.Add($@"
                        SELECT 
                            'Purchase Return' AS TrnsType,
                            CAST(ISNULL(m.PReturnNo, 0) AS NVARCHAR(50)) AS InvoiceNo,
                            m.PReturnDate AS DocDate,
                            ISNULL(NULLIF(m.VendorName, ''), 'General Supplier') AS PartyName,
                            '' AS PartyGSTIN,
                            ISNULL(d.ItemName, '') AS ItemName,
                            {hsnSelect} AS HSNCode,
                            ISNULL(d.Qty, 0) AS Qty,
                            ISNULL(d.Unit, '') AS Unit,
                            CAST((ISNULL(d.Qty,0)*ISNULL(d.Cost,0)) - ISNULL(d.TaxAmt,0) AS DECIMAL(18,2)) AS TaxableAmt,
                            CAST(ISNULL(d.TaxPer, 0) AS FLOAT) AS TaxPer,
                            CAST(ISNULL(d.TaxPer, 0)/2.0 AS FLOAT) AS CGSTPer,
                            CAST(ISNULL(d.TaxAmt, 0)/2.0 AS DECIMAL(18,2)) AS CGSTAmt,
                            CAST(ISNULL(d.TaxPer, 0)/2.0 AS FLOAT) AS SGSTPer,
                            CAST(ISNULL(d.TaxAmt, 0)/2.0 AS DECIMAL(18,2)) AS SGSTAmt,
                            CAST(0 AS FLOAT) AS IGSTPer,
                            CAST(0 AS DECIMAL(18,2)) AS IGSTAmt,
                            CAST(0 AS FLOAT) AS CessPer,
                            CAST(0 AS DECIMAL(18,2)) AS CessAmt,
                            CAST(ISNULL(d.TaxAmt, 0) AS DECIMAL(18,2)) AS TotalTaxAmt,
                            CAST((ISNULL(d.Qty,0)*ISNULL(d.Cost,0)) AS DECIMAL(18,2)) AS GrandTotal,
                            ISNULL(m.TaxType, 'excl') AS TaxType
                        FROM dbo.PRMaster m
                        INNER JOIN dbo.PRDetails d ON m.PReturnNo = d.PReturnNo AND m.FinYearId = d.FinYearId AND m.BranchId = d.BranchId
                        {itemJoin}
                        WHERE (@CompanyId IS NULL OR m.CompanyId = @CompanyId)
                          AND (@BranchId IS NULL OR m.BranchId = @BranchId)
                          AND (@FinYearId IS NULL OR m.FinYearId = @FinYearId)
                          AND (CAST(m.PReturnDate AS DATE) BETWEEN @FromDate AND @ToDate)
                          AND (@TrnsType = 'ALL' OR @TrnsType = 'Purchase Return')
                    ");
                }

                if (queryBlocks.Count == 0)
                {
                    return rows;
                }

                string sqlCombined = string.Join("\nUNION ALL\n", queryBlocks);

                using (SqlCommand cmd = new SqlCommand(sqlCombined, (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", filter.CompanyId > 0 ? (object)filter.CompanyId : DBNull.Value);
                    cmd.Parameters.AddWithValue("@BranchId", filter.BranchId > 0 ? (object)filter.BranchId : DBNull.Value);
                    cmd.Parameters.AddWithValue("@FinYearId", filter.FinYearId > 0 ? (object)filter.FinYearId : DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", filter.FromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", filter.ToDate.Date);
                    cmd.Parameters.AddWithValue("@TrnsType", string.IsNullOrWhiteSpace(filter.TrnsType) ? "ALL" : filter.TrnsType);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);
                        return MapDataTableToRows(table);
                    }
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        private bool TableExists(string tableName)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@TableName", tableName);
                    object res = cmd.ExecuteScalar();
                    return res != null && res != DBNull.Value;
                }
            }
            catch
            {
                return false;
            }
        }

        private List<GSTAndTaxReportRow> MapDataTableToRows(DataTable table)
        {
            List<GSTAndTaxReportRow> rows = new List<GSTAndTaxReportRow>();
            if (table == null) return rows;

            foreach (DataRow row in table.Rows)
            {
                string trnsType = ToString(row, "TrnsType");
                decimal totalTax = ToDecimal(row, "TotalTaxAmt");
                string taxCat = "Other";
                decimal outTax = 0m;
                decimal inTax = 0m;

                if (trnsType.Equals("Sales Invoice", StringComparison.OrdinalIgnoreCase))
                {
                    taxCat = "Output Tax (Sales)";
                    outTax = totalTax;
                }
                else if (trnsType.Equals("Purchase Invoice", StringComparison.OrdinalIgnoreCase))
                {
                    taxCat = "Input Tax (ITC)";
                    inTax = totalTax;
                }
                else if (trnsType.Equals("Sales Return", StringComparison.OrdinalIgnoreCase))
                {
                    taxCat = "Output Tax (Return)";
                    outTax = -totalTax;
                }
                else if (trnsType.Equals("Purchase Return", StringComparison.OrdinalIgnoreCase))
                {
                    taxCat = "Input Tax (Return)";
                    inTax = -totalTax;
                }

                rows.Add(new GSTAndTaxReportRow
                {
                    TrnsType = trnsType,
                    InvoiceNo = ToString(row, "InvoiceNo"),
                    DocDate = ToDateTime(row, "DocDate"),
                    PartyName = ToString(row, "PartyName"),
                    PartyGSTIN = ToString(row, "PartyGSTIN"),
                    ItemName = ToString(row, "ItemName"),
                    HSNCode = ToString(row, "HSNCode"),
                    Qty = ToDouble(row, "Qty"),
                    Unit = ToString(row, "Unit"),
                    TaxableAmt = ToDecimal(row, "TaxableAmt"),
                    TaxPer = ToDouble(row, "TaxPer"),
                    CGSTPer = ToDouble(row, "CGSTPer"),
                    CGSTAmt = ToDecimal(row, "CGSTAmt"),
                    SGSTPer = ToDouble(row, "SGSTPer"),
                    SGSTAmt = ToDecimal(row, "SGSTAmt"),
                    IGSTPer = ToDouble(row, "IGSTPer"),
                    IGSTAmt = ToDecimal(row, "IGSTAmt"),
                    CessPer = ToDouble(row, "CessPer"),
                    CessAmt = ToDecimal(row, "CessAmt"),
                    TotalTaxAmt = totalTax,
                    GrandTotal = ToDecimal(row, "GrandTotal"),
                    TaxType = ToString(row, "TaxType"),
                    TaxCategory = taxCat,
                    OutputTaxAmt = outTax,
                    InputTaxAmt = inTax
                });
            }
            return rows;
        }

        private static double ToDouble(DataRow row, string col) => row.Table.Columns.Contains(col) && row[col] != DBNull.Value ? Convert.ToDouble(row[col]) : 0.0;
        private static decimal ToDecimal(DataRow row, string col) => row.Table.Columns.Contains(col) && row[col] != DBNull.Value ? Convert.ToDecimal(row[col]) : 0m;
        private static DateTime ToDateTime(DataRow row, string col) => row.Table.Columns.Contains(col) && row[col] != DBNull.Value ? Convert.ToDateTime(row[col]) : DateTime.MinValue;
        private static string ToString(DataRow row, string col) => row.Table.Columns.Contains(col) && row[col] != DBNull.Value ? Convert.ToString(row[col]) : string.Empty;
    }
}
