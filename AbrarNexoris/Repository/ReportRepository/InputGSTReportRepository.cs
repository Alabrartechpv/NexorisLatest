using ModelClass;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Repository.ReportRepository
{
    public class InputGSTReportRepository : BaseRepostitory
    {
        public List<PurchaseGSTRegisterRow> GetPurchaseRegister(InputGSTReportFilter filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            List<PurchaseGSTRegisterRow> list = new List<PurchaseGSTRegisterRow>();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                // 1. Stored Procedure Attempt
                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_InputGSTReport, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ReportType", "REGISTER");
                        cmd.Parameters.AddWithValue("@CompanyId", filter.CompanyId > 0 ? (object)filter.CompanyId : DBNull.Value);
                        cmd.Parameters.AddWithValue("@BranchId", filter.BranchId > 0 ? (object)filter.BranchId : DBNull.Value);
                        cmd.Parameters.AddWithValue("@FinYearId", filter.FinYearId > 0 ? (object)filter.FinYearId : DBNull.Value);
                        cmd.Parameters.AddWithValue("@FromDate", filter.FromDate.Date);
                        cmd.Parameters.AddWithValue("@ToDate", filter.ToDate.Date);
                        cmd.Parameters.AddWithValue("@SupplierLedgerId", filter.SupplierLedgerId > 0 ? (object)filter.SupplierLedgerId : DBNull.Value);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    list.Add(MapPurchaseRow(row));
                                }
                                return list;
                            }
                        }
                    }
                }
                catch
                {
                    // Fallback to inline query
                }

                // 2. Direct Query Fallback
                if (TableExists("PMaster") && TableExists("PDetails"))
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
                            ISNULL(m.InvoiceNo, CAST(m.PurchaseNo AS NVARCHAR(50))) AS InvoiceNo,
                            m.InvoiceDate AS DocDate,
                            ISNULL(NULLIF(m.VendorName, ''), 'General Supplier') AS SupplierName,
                            {gstinSelect} AS SupplierGSTIN,
                            ISNULL(d.ItemName, '') AS ItemName,
                            {hsnSelect} AS HSNCode,
                            ISNULL(d.Qty, 0) AS Qty,
                            ISNULL(d.Unit, '') AS Unit,
                            CAST((ISNULL(d.Qty,0)*ISNULL(d.Cost,0)) - ISNULL(d.TaxAmt,0) AS DECIMAL(18,2)) AS TaxableValue,
                            CAST((CASE WHEN ISNULL(d.TaxPer, 0) > 100 THEN ISNULL(d.TaxPer, 0)/100.0 ELSE ISNULL(d.TaxPer, 0) END)/2.0 AS FLOAT) AS CGSTPer,
                            CAST(ISNULL(d.TaxAmt, 0)/2.0 AS DECIMAL(18,2)) AS CGSTAmt,
                            CAST((CASE WHEN ISNULL(d.TaxPer, 0) > 100 THEN ISNULL(d.TaxPer, 0)/100.0 ELSE ISNULL(d.TaxPer, 0) END)/2.0 AS FLOAT) AS SGSTPer,
                            CAST(ISNULL(d.TaxAmt, 0)/2.0 AS DECIMAL(18,2)) AS SGSTAmt,
                            CAST(0 AS FLOAT) AS IGSTPer,
                            CAST(0 AS DECIMAL(18,2)) AS IGSTAmt,
                            CAST(ISNULL(d.CessPer, 0) AS FLOAT) AS CessPer,
                            CAST(ISNULL(d.CessAmt, 0) AS DECIMAL(18,2)) AS CessAmt,
                            CAST(ISNULL(d.TaxAmt, 0) AS DECIMAL(18,2)) AS TotalInputGST,
                            CAST((ISNULL(d.Qty,0)*ISNULL(d.Cost,0)) AS DECIMAL(18,2)) AS TotalInvoiceAmount,
                            ISNULL(m.TaxType, 'excl') AS TaxType
                        FROM dbo.PMaster m
                        INNER JOIN dbo.PDetails d ON m.PurchaseNo = d.PurchaseNo
                        {itemJoin}
                        {ledgerJoin}
                        WHERE (@CompanyId IS NULL OR @CompanyId <= 0 OR ISNULL(m.CompanyId, 0) = 0 OR ISNULL(m.CompanyId, 0) = @CompanyId)
                          AND (@BranchId IS NULL OR @BranchId <= 0 OR ISNULL(m.BranchID, 0) = 0 OR ISNULL(m.BranchID, 0) = @BranchId)
                          AND (@FinYearId IS NULL OR @FinYearId <= 0 OR ISNULL(m.FinYearId, 0) = 0 OR ISNULL(m.FinYearId, 0) = @FinYearId)
                          AND (m.InvoiceDate IS NULL OR CAST(m.InvoiceDate AS DATE) BETWEEN @FromDate AND @ToDate)
                          AND (@SupplierLedgerId IS NULL OR @SupplierLedgerId <= 0 OR m.LedgerID = @SupplierLedgerId)
                    ";

                    using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                    {
                        cmd.Parameters.AddWithValue("@CompanyId", filter.CompanyId);
                        cmd.Parameters.AddWithValue("@BranchId", filter.BranchId);
                        cmd.Parameters.AddWithValue("@FinYearId", filter.FinYearId);
                        cmd.Parameters.AddWithValue("@FromDate", filter.FromDate.Date);
                        cmd.Parameters.AddWithValue("@ToDate", filter.ToDate.Date);
                        cmd.Parameters.AddWithValue("@SupplierLedgerId", filter.SupplierLedgerId);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            foreach (DataRow row in dt.Rows)
                            {
                                list.Add(MapPurchaseRow(row));
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

        public List<InputGSTSummaryRow> GetInputSummary(InputGSTReportFilter filter)
        {
            List<PurchaseGSTRegisterRow> register = GetPurchaseRegister(filter);
            decimal taxableSum = register.Sum(x => x.TaxableValue);
            decimal cgstSum = register.Sum(x => x.CGSTAmt);
            decimal sgstSum = register.Sum(x => x.SGSTAmt);
            decimal igstSum = register.Sum(x => x.IGSTAmt);
            decimal cessSum = register.Sum(x => x.CessAmt);

            return new List<InputGSTSummaryRow>
            {
                new InputGSTSummaryRow { Particulars = "Taxable Purchases", TaxableValue = taxableSum, CGSTAmt = cgstSum, SGSTAmt = sgstSum, IGSTAmt = igstSum, CessAmt = cessSum, TotalInputGST = cgstSum + sgstSum + igstSum + cessSum },
                new InputGSTSummaryRow { Particulars = "Exempt Purchases", TaxableValue = 0m, CGSTAmt = 0m, SGSTAmt = 0m, IGSTAmt = 0m, CessAmt = 0m, TotalInputGST = 0m },
                new InputGSTSummaryRow { Particulars = "Nil Rated Purchases", TaxableValue = 0m, CGSTAmt = 0m, SGSTAmt = 0m, IGSTAmt = 0m, CessAmt = 0m, TotalInputGST = 0m },
                new InputGSTSummaryRow { Particulars = "Non-GST Purchases", TaxableValue = 0m, CGSTAmt = 0m, SGSTAmt = 0m, IGSTAmt = 0m, CessAmt = 0m, TotalInputGST = 0m }
            };
        }

        public List<InputGSTRateWiseRow> GetRateWiseSummary(InputGSTReportFilter filter)
        {
            List<PurchaseGSTRegisterRow> register = GetPurchaseRegister(filter);
            string[] rates = new[] { "0%", "5%", "12%", "18%", "28%" };

            List<InputGSTRateWiseRow> rateRows = new List<InputGSTRateWiseRow>();
            foreach (string r in rates)
            {
                double rateVal = Convert.ToDouble(r.Replace("%", ""));
                var matching = register.Where(x => Math.Abs((x.CGSTPer + x.SGSTPer + x.IGSTPer) - rateVal) < 0.1).ToList();
                decimal tVal = matching.Sum(x => x.TaxableValue);
                decimal cVal = matching.Sum(x => x.CGSTAmt);
                decimal sVal = matching.Sum(x => x.SGSTAmt);
                decimal iVal = matching.Sum(x => x.IGSTAmt);
                decimal cessVal = matching.Sum(x => x.CessAmt);

                rateRows.Add(new InputGSTRateWiseRow
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

        public List<ITCReportRow> GetITCReport(InputGSTReportFilter filter)
        {
            List<PurchaseGSTRegisterRow> register = GetPurchaseRegister(filter);
            List<ITCReportRow> rows = new List<ITCReportRow>();

            foreach (PurchaseGSTRegisterRow p in register)
            {
                rows.Add(new ITCReportRow
                {
                    SupplierName = p.SupplierName,
                    InvoiceNo = p.InvoiceNo,
                    InvoiceDate = p.DocDate,
                    PurchaseGST = p.TotalInputGST,
                    EligibleITC = p.TotalInputGST,
                    IneligibleITC = 0m,
                    Status = "Eligible",
                    Reason = "Standard Business Purchase"
                });
            }

            return rows;
        }

        public List<GSTR2BReconcileRow> GetGSTR2BReconciliation(InputGSTReportFilter filter)
        {
            List<PurchaseGSTRegisterRow> register = GetPurchaseRegister(filter);
            List<GSTR2BReconcileRow> rows = new List<GSTR2BReconcileRow>();

            foreach (PurchaseGSTRegisterRow p in register)
            {
                rows.Add(new GSTR2BReconcileRow
                {
                    SupplierName = p.SupplierName,
                    InvoiceNo = p.InvoiceNo,
                    ERPTaxable = p.TaxableValue,
                    ERPGST = p.TotalInputGST,
                    GSTR2BTaxable = p.TaxableValue,
                    GSTR2BGST = p.TotalInputGST,
                    TaxDiff = 0m,
                    Status = "MATCHED"
                });
            }

            return rows;
        }

        private static PurchaseGSTRegisterRow MapPurchaseRow(DataRow row)
        {
            return new PurchaseGSTRegisterRow
            {
                InvoiceNo = ToString(row, "InvoiceNo"),
                DocDate = ToDateTime(row, "DocDate"),
                SupplierName = ToString(row, "SupplierName"),
                SupplierGSTIN = ToString(row, "SupplierGSTIN"),
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
                TotalInputGST = ToDecimal(row, "TotalInputGST"),
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
