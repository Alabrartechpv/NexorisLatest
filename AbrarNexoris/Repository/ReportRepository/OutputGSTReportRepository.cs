using ModelClass;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Repository.ReportRepository
{
    public class OutputGSTReportRepository : BaseRepostitory
    {
        public List<SalesGSTRegisterRow> GetSalesRegister(OutputGSTReportFilter filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            List<SalesGSTRegisterRow> list = new List<SalesGSTRegisterRow>();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                // 1. Stored Procedure Attempt
                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_OutputGSTReport, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ReportType", "REGISTER");
                        cmd.Parameters.AddWithValue("@CompanyId", filter.CompanyId > 0 ? (object)filter.CompanyId : DBNull.Value);
                        cmd.Parameters.AddWithValue("@BranchId", filter.BranchId > 0 ? (object)filter.BranchId : DBNull.Value);
                        cmd.Parameters.AddWithValue("@FinYearId", filter.FinYearId > 0 ? (object)filter.FinYearId : DBNull.Value);
                        cmd.Parameters.AddWithValue("@FromDate", filter.FromDate.Date);
                        cmd.Parameters.AddWithValue("@ToDate", filter.ToDate.Date);
                        cmd.Parameters.AddWithValue("@CustomerLedgerId", filter.CustomerLedgerId > 0 ? (object)filter.CustomerLedgerId : DBNull.Value);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    list.Add(MapSalesRow(row));
                                }
                                return list;
                            }
                        }
                    }
                }
                catch
                {
                    // Fallback
                }

                // 2. Direct Query Fallback
                if (TableExists("SMaster") && TableExists("SDetails"))
                {
                    bool hasItemMaster = TableExists("ItemMaster");
                    string itemJoin = hasItemMaster ? "LEFT JOIN dbo.ItemMaster i ON d.ItemID = i.ItemID" : "";
                    string hsnSelect = hasItemMaster ? "ISNULL(i.HSNCode, '')" : "''";

                    bool hasLedgerMaster = TableExists("LedgerMaster");
                    bool hasLedger = TableExists("Ledger");
                    string ledgerTable = hasLedgerMaster ? "dbo.LedgerMaster" : (hasLedger ? "dbo.Ledger" : "");
                    string ledgerJoin = !string.IsNullOrEmpty(ledgerTable) ? $"LEFT JOIN {ledgerTable} l ON m.LedgerID = l.LedgerID" : "";
                    string gstinSelect = !string.IsNullOrEmpty(ledgerTable) ? "ISNULL(l.GSTIN, '')" : "''";

                    string sql = $@"
                        SELECT 
                            CAST(ISNULL(m.BillNo, 0) AS NVARCHAR(50)) AS InvoiceNo,
                            m.BillDate AS DocDate,
                            ISNULL(NULLIF(m.CustomerName, ''), 'Walk-in Customer') AS CustomerName,
                            {gstinSelect} AS CustomerGSTIN,
                            CASE WHEN LEN({gstinSelect}) >= 15 THEN 'B2B' ELSE 'B2C' END AS SaleType,
                            ISNULL(d.ItemName, '') AS ItemName,
                            {hsnSelect} AS HSNCode,
                            ISNULL(d.Qty, 0) AS Qty,
                            ISNULL(d.Unit, '') AS Unit,
                            CAST(ISNULL(d.BaseAmount, (ISNULL(d.Qty,0)*ISNULL(d.UnitPrice,0)) - ISNULL(d.TaxAmt,0)) AS DECIMAL(18,2)) AS TaxableValue,
                            CAST((CASE WHEN ISNULL(d.TaxPer, 0) > 100 THEN ISNULL(d.TaxPer, 0)/100.0 ELSE ISNULL(d.TaxPer, 0) END)/2.0 AS FLOAT) AS CGSTPer,
                            CAST(ISNULL(d.TaxAmt, 0)/2.0 AS DECIMAL(18,2)) AS CGSTAmt,
                            CAST((CASE WHEN ISNULL(d.TaxPer, 0) > 100 THEN ISNULL(d.TaxPer, 0)/100.0 ELSE ISNULL(d.TaxPer, 0) END)/2.0 AS FLOAT) AS SGSTPer,
                            CAST(ISNULL(d.TaxAmt, 0)/2.0 AS DECIMAL(18,2)) AS SGSTAmt,
                            CAST(0 AS FLOAT) AS IGSTPer,
                            CAST(0 AS DECIMAL(18,2)) AS IGSTAmt,
                            CAST(0 AS FLOAT) AS CessPer,
                            CAST(0 AS DECIMAL(18,2)) AS CessAmt,
                            CAST(ISNULL(d.TaxAmt, 0) AS DECIMAL(18,2)) AS TotalOutputGST,
                            CAST(ISNULL(d.TotalAmount, (ISNULL(d.Qty,0)*ISNULL(d.UnitPrice,0))) AS DECIMAL(18,2)) AS TotalInvoiceAmount,
                            ISNULL(d.TaxType, 'incl') AS TaxType
                        FROM dbo.SMaster m
                        INNER JOIN dbo.SDetails d ON m.BillNo = d.BillNo
                        {itemJoin}
                        {ledgerJoin}
                        WHERE (@CompanyId IS NULL OR @CompanyId <= 0 OR ISNULL(m.CompanyId, 0) = 0 OR ISNULL(m.CompanyId, 0) = @CompanyId)
                          AND (@BranchId IS NULL OR @BranchId <= 0 OR ISNULL(m.BranchId, 0) = 0 OR ISNULL(m.BranchId, 0) = @BranchId)
                          AND (@FinYearId IS NULL OR @FinYearId <= 0 OR ISNULL(m.FinYearId, 0) = 0 OR ISNULL(m.FinYearId, 0) = @FinYearId)
                          AND (m.BillDate IS NULL OR CAST(m.BillDate AS DATE) BETWEEN @FromDate AND @ToDate)
                          AND (@CustomerLedgerId IS NULL OR @CustomerLedgerId <= 0 OR m.LedgerID = @CustomerLedgerId)
                    ";

                    using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                    {
                        cmd.Parameters.AddWithValue("@CompanyId", filter.CompanyId);
                        cmd.Parameters.AddWithValue("@BranchId", filter.BranchId);
                        cmd.Parameters.AddWithValue("@FinYearId", filter.FinYearId);
                        cmd.Parameters.AddWithValue("@FromDate", filter.FromDate.Date);
                        cmd.Parameters.AddWithValue("@ToDate", filter.ToDate.Date);
                        cmd.Parameters.AddWithValue("@CustomerLedgerId", filter.CustomerLedgerId);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            foreach (DataRow row in dt.Rows)
                            {
                                list.Add(MapSalesRow(row));
                            }
                        }
                    }
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return list;
        }

        public List<OutputGSTSummaryRow> GetOutputSummary(OutputGSTReportFilter filter)
        {
            List<SalesGSTRegisterRow> register = GetSalesRegister(filter);
            var b2b = register.Where(x => string.Equals(x.SaleType, "B2B", StringComparison.OrdinalIgnoreCase)).ToList();
            var b2c = register.Where(x => !string.Equals(x.SaleType, "B2B", StringComparison.OrdinalIgnoreCase)).ToList();

            return new List<OutputGSTSummaryRow>
            {
                new OutputGSTSummaryRow
                {
                    SalesCategory = "B2B Sales",
                    TaxableValue = b2b.Sum(x => x.TaxableValue),
                    CGSTAmt = b2b.Sum(x => x.CGSTAmt),
                    SGSTAmt = b2b.Sum(x => x.SGSTAmt),
                    IGSTAmt = b2b.Sum(x => x.IGSTAmt),
                    CessAmt = b2b.Sum(x => x.CessAmt),
                    TotalOutputGST = b2b.Sum(x => x.TotalOutputGST)
                },
                new OutputGSTSummaryRow
                {
                    SalesCategory = "B2C Sales",
                    TaxableValue = b2c.Sum(x => x.TaxableValue),
                    CGSTAmt = b2c.Sum(x => x.CGSTAmt),
                    SGSTAmt = b2c.Sum(x => x.SGSTAmt),
                    IGSTAmt = b2c.Sum(x => x.IGSTAmt),
                    CessAmt = b2c.Sum(x => x.CessAmt),
                    TotalOutputGST = b2c.Sum(x => x.TotalOutputGST)
                },
                new OutputGSTSummaryRow
                {
                    SalesCategory = "Interstate Sales",
                    TaxableValue = 0m, CGSTAmt = 0m, SGSTAmt = 0m, IGSTAmt = 0m, CessAmt = 0m, TotalOutputGST = 0m
                }
            };
        }

        public List<OutputGSTRateWiseRow> GetRateWiseSummary(OutputGSTReportFilter filter)
        {
            List<SalesGSTRegisterRow> register = GetSalesRegister(filter);
            string[] rates = new[] { "0%", "5%", "12%", "18%", "28%" };

            List<OutputGSTRateWiseRow> rateRows = new List<OutputGSTRateWiseRow>();
            foreach (string r in rates)
            {
                double rateVal = Convert.ToDouble(r.Replace("%", ""));
                var matching = register.Where(x => Math.Abs((x.CGSTPer + x.SGSTPer + x.IGSTPer) - rateVal) < 0.1).ToList();
                decimal tVal = matching.Sum(x => x.TaxableValue);
                decimal cVal = matching.Sum(x => x.CGSTAmt);
                decimal sVal = matching.Sum(x => x.SGSTAmt);
                decimal iVal = matching.Sum(x => x.IGSTAmt);
                decimal cessVal = matching.Sum(x => x.CessAmt);

                rateRows.Add(new OutputGSTRateWiseRow
                {
                    GSTRate = r,
                    TaxableValue = tVal,
                    CGSTAmt = cVal,
                    SGSTAmt = sVal,
                    IGSTAmt = iVal,
                    CessAmt = cessVal,
                    TotalGST = cVal + sVal + iVal + cessVal
                });
            }

            return rateRows;
        }

        public List<B2BSalesRow> GetB2BSales(OutputGSTReportFilter filter)
        {
            List<SalesGSTRegisterRow> register = GetSalesRegister(filter);
            return register.Where(x => string.Equals(x.SaleType, "B2B", StringComparison.OrdinalIgnoreCase))
                .Select(x => new B2BSalesRow
                {
                    CustomerGSTIN = x.CustomerGSTIN,
                    CustomerName = x.CustomerName,
                    InvoiceNo = x.InvoiceNo,
                    DocDate = x.DocDate,
                    TaxableValue = x.TaxableValue,
                    CGSTAmt = x.CGSTAmt,
                    SGSTAmt = x.SGSTAmt,
                    IGSTAmt = x.IGSTAmt,
                    TotalInvoiceAmount = x.TotalInvoiceAmount
                }).ToList();
        }

        public List<HSNOutputGSTRow> GetHSNOutputGST(OutputGSTReportFilter filter)
        {
            List<SalesGSTRegisterRow> register = GetSalesRegister(filter);
            return register.GroupBy(x => string.IsNullOrWhiteSpace(x.HSNCode) ? "N/A" : x.HSNCode)
                .Select(g => new HSNOutputGSTRow
                {
                    HSNCode = g.Key,
                    ItemDescription = g.First().ItemName,
                    UQC = g.First().Unit ?? "PCS",
                    TotalQty = g.Sum(x => x.Qty),
                    TaxableValue = g.Sum(x => x.TaxableValue),
                    GSTRate = $"{g.First().CGSTPer + g.First().SGSTPer + g.First().IGSTPer}%",
                    CGSTAmt = g.Sum(x => x.CGSTAmt),
                    SGSTAmt = g.Sum(x => x.SGSTAmt),
                    IGSTAmt = g.Sum(x => x.IGSTAmt),
                    TotalGST = g.Sum(x => x.TotalOutputGST)
                }).ToList();
        }

        public List<CreditDebitNoteGSTRow> GetCreditDebitNotes(OutputGSTReportFilter filter)
        {
            List<CreditDebitNoteGSTRow> rows = new List<CreditDebitNoteGSTRow>();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                if (TableExists("SRMaster") && TableExists("SRDetails"))
                {
                    string sql = @"
                        SELECT 
                            'Sales Return' AS DocumentType,
                            CAST(ISNULL(m.SReturnNo, 0) AS NVARCHAR(50)) AS NoteNo,
                            m.SReturnDate AS NoteDate,
                            CAST(ISNULL(m.BillNo, 0) AS NVARCHAR(50)) AS RefInvoiceNo,
                            ISNULL(NULLIF(m.CustomerName, ''), 'Walk-in Customer') AS PartyName,
                            CAST((ISNULL(d.Qty,0)*ISNULL(d.Rate,0)) - ISNULL(d.TaxAmt,0) AS DECIMAL(18,2)) AS TaxableAdjustment,
                            CAST(ISNULL(d.TaxAmt, 0)/2.0 AS DECIMAL(18,2)) AS CGSTAdjustment,
                            CAST(ISNULL(d.TaxAmt, 0)/2.0 AS DECIMAL(18,2)) AS SGSTAdjustment,
                            CAST(0 AS DECIMAL(18,2)) AS IGSTAdjustment,
                            CAST((ISNULL(d.Qty,0)*ISNULL(d.Rate,0)) AS DECIMAL(18,2)) AS NetTotalAdjustment
                        FROM dbo.SRMaster m
                        INNER JOIN dbo.SRDetails d ON m.SReturnNo = d.SReturnNo AND m.FinYearId = d.FinYearId AND m.BranchId = d.BranchId
                        WHERE (@CompanyId IS NULL OR m.CompanyId = @CompanyId)
                          AND (@BranchId IS NULL OR m.BranchId = @BranchId)
                          AND (@FinYearId IS NULL OR m.FinYearId = @FinYearId)
                          AND (CAST(m.SReturnDate AS DATE) BETWEEN @FromDate AND @ToDate)
                    ";

                    using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                    {
                        cmd.Parameters.AddWithValue("@CompanyId", filter.CompanyId > 0 ? (object)filter.CompanyId : DBNull.Value);
                        cmd.Parameters.AddWithValue("@BranchId", filter.BranchId > 0 ? (object)filter.BranchId : DBNull.Value);
                        cmd.Parameters.AddWithValue("@FinYearId", filter.FinYearId > 0 ? (object)filter.FinYearId : DBNull.Value);
                        cmd.Parameters.AddWithValue("@FromDate", filter.FromDate.Date);
                        cmd.Parameters.AddWithValue("@ToDate", filter.ToDate.Date);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            foreach (DataRow row in dt.Rows)
                            {
                                rows.Add(new CreditDebitNoteGSTRow
                                {
                                    DocumentType = ToString(row, "DocumentType"),
                                    NoteNo = ToString(row, "NoteNo"),
                                    NoteDate = ToDateTime(row, "NoteDate"),
                                    RefInvoiceNo = ToString(row, "RefInvoiceNo"),
                                    PartyName = ToString(row, "PartyName"),
                                    TaxableAdjustment = ToDecimal(row, "TaxableAdjustment"),
                                    CGSTAdjustment = ToDecimal(row, "CGSTAdjustment"),
                                    SGSTAdjustment = ToDecimal(row, "SGSTAdjustment"),
                                    IGSTAdjustment = ToDecimal(row, "IGSTAdjustment"),
                                    NetTotalAdjustment = ToDecimal(row, "NetTotalAdjustment")
                                });
                            }
                        }
                    }
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return rows;
        }

        private static SalesGSTRegisterRow MapSalesRow(DataRow row)
        {
            return new SalesGSTRegisterRow
            {
                InvoiceNo = ToString(row, "InvoiceNo"),
                DocDate = ToDateTime(row, "DocDate"),
                CustomerName = ToString(row, "CustomerName"),
                CustomerGSTIN = ToString(row, "CustomerGSTIN"),
                SaleType = ToString(row, "SaleType"),
                ItemName = ToString(row, "ItemName"),
                HSNCode = ToString(row, "HSNCode"),
                Qty = ToDouble(row, "Qty"),
                Unit = ToString(row, "Unit"),
                TaxableValue = ToDecimal(row, "TaxableValue"),
                CGSTPer = ToDouble(row, "CGSTPer"),
                CGSTAmt = ToDecimal(row, "CGSTAmt"),
                SGSTPer = ToDouble(row, "SGSTPer"),
                SGSTAmt = ToDecimal(row, "SGSTAmt"),
                IGSTPer = ToDouble(row, "IGSTPer"),
                IGSTAmt = ToDecimal(row, "IGSTAmt"),
                CessPer = ToDouble(row, "CessPer"),
                CessAmt = ToDecimal(row, "CessAmt"),
                TotalOutputGST = ToDecimal(row, "TotalOutputGST"),
                TotalInvoiceAmount = ToDecimal(row, "TotalInvoiceAmount"),
                TaxType = ToString(row, "TaxType")
            };
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

        private static double ToDouble(DataRow row, string col) => row.Table.Columns.Contains(col) && row[col] != DBNull.Value ? Convert.ToDouble(row[col]) : 0.0;
        private static decimal ToDecimal(DataRow row, string col) => row.Table.Columns.Contains(col) && row[col] != DBNull.Value ? Convert.ToDecimal(row[col]) : 0m;
        private static DateTime ToDateTime(DataRow row, string col) => row.Table.Columns.Contains(col) && row[col] != DBNull.Value ? Convert.ToDateTime(row[col]) : DateTime.MinValue;
        private static string ToString(DataRow row, string col) => row.Table.Columns.Contains(col) && row[col] != DBNull.Value ? Convert.ToString(row[col]) : string.Empty;
    }
}
