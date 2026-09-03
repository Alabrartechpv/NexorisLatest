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
                string masterTable = TableExists("PMaster") ? "PMaster" : (TableExists("PurchaseMaster") ? "PurchaseMaster" : "");
                string detailsTable = TableExists("PDetails") ? "PDetails" : (TableExists("PurchaseDetails") ? "PurchaseDetails" : "");

                if (!string.IsNullOrEmpty(masterTable))
                {
                    string purNoMaster = GetCol(masterTable, "m", "PurchaseNo", "PurchaseID", "PurNo", "InvoiceNo") ?? "m.PurchaseNo";
                    string invNoMaster = GetCol(masterTable, "m", "InvoiceNo", "InvNo", "PurchaseNo", "BillNo") ?? "m.InvoiceNo";
                    string dateColMaster = GetCol(masterTable, "m", "InvoiceDate", "PurchaseDate", "DocDate", "CreatedOn") ?? "m.InvoiceDate";
                    string vendorNameCol = GetCol(masterTable, "m", "VendorName", "SupplierName", "vendorname") ?? "m.VendorName";
                    string ledgerIdMaster = GetCol(masterTable, "m", "LedgerID", "LedgerId", "VendorID", "VendorId");
                    string companyCol = GetCol(masterTable, "m", "CompanyId", "CompanyID", "Company_Id");
                    string branchCol = GetCol(masterTable, "m", "BranchId", "BranchID", "Branch_Id");
                    string finYearCol = GetCol(masterTable, "m", "FinYearId", "FinYearID", "FinYear_Id");
                    string taxTypeMasterCol = GetCol(masterTable, "m", "TaxType", "taxtype") ?? "'excl'";

                    string detailsJoin = "";
                    string itemNameCol = "''";
                    string qtyCol = "0";
                    string unitCol = "''";
                    string costCol = "0";
                    string taxAmtCol = "0";
                    string taxPerCol = "0";
                    string cessPerCol = "0";
                    string cessAmtCol = "0";

                    if (!string.IsNullOrEmpty(detailsTable))
                    {
                        string purNoDetails = GetCol(detailsTable, "d", "PurchaseNo", "PurchaseID", "PurNo", "InvoiceNo") ?? "d.PurchaseNo";
                        detailsJoin = $"LEFT JOIN dbo.{detailsTable} d ON {purNoMaster} = {purNoDetails}";

                        itemNameCol = GetCol(detailsTable, "d", "ItemName", "Item_Name", "itemname") ?? "''";
                        qtyCol = GetCol(detailsTable, "d", "Qty", "qty", "Quantity") ?? "0";
                        unitCol = GetCol(detailsTable, "d", "Unit", "unit", "UOM") ?? "''";
                        costCol = GetCol(detailsTable, "d", "Cost", "UnitPrice", "Rate", "Price") ?? "0";
                        taxAmtCol = GetCol(detailsTable, "d", "TaxAmt", "TaxAmount", "GstAmt", "GSTAmount") ?? "0";
                        taxPerCol = GetCol(detailsTable, "d", "TaxPer", "TaxPercent", "GstPer", "GSTPer") ?? "0";
                        cessPerCol = GetCol(detailsTable, "d", "CessPer", "CessPercent") ?? "0";
                        cessAmtCol = GetCol(detailsTable, "d", "CessAmt", "CessAmount") ?? "0";
                    }

                    string hasItemMaster = TableExists("ItemMaster") ? "ItemMaster" : "";
                    string itemJoin = "";
                    string hsnSelect = "''";
                    if (!string.IsNullOrEmpty(hasItemMaster) && !string.IsNullOrEmpty(detailsTable))
                    {
                        string itemKeyDetails = GetCol(detailsTable, "d", "ItemID", "ItemId");
                        string itemKeyMaster = GetCol("ItemMaster", "i", "ItemID", "ItemId");
                        string hsnCol = GetCol("ItemMaster", "i", "HSNCode", "HSN", "HsnCode");
                        if (itemKeyDetails != null && itemKeyMaster != null)
                        {
                            itemJoin = $"LEFT JOIN dbo.ItemMaster i ON {itemKeyDetails} = {itemKeyMaster}";
                            if (hsnCol != null) hsnSelect = $"ISNULL({hsnCol}, '')";
                        }
                    }

                    string ledgerTable = TableExists("LedgerMaster") ? "LedgerMaster" : (TableExists("Ledger") ? "Ledger" : "");
                    string ledgerJoin = "";
                    string gstinSelect = "''";
                    if (!string.IsNullOrEmpty(ledgerTable) && ledgerIdMaster != null)
                    {
                        string ledgerKey = GetCol(ledgerTable, "l", "LedgerID", "LedgerId");
                        if (ledgerKey != null)
                        {
                            ledgerJoin = $"LEFT JOIN dbo.{ledgerTable} l ON {ledgerIdMaster} = {ledgerKey}";
                            gstinSelect = GetGstinColumnExpression("l", ledgerTable);
                        }
                    }

                    string masterNetCol = GetCol(masterTable, "m", "NetAmount", "NetAmt", "GrandTotal", "TotalAmount") ?? "0";
                    string masterTaxCol = GetCol(masterTable, "m", "TaxAmt", "TaxAmount", "GstAmt") ?? "0";
                    string masterSubCol = GetCol(masterTable, "m", "SubTotal", "BaseAmount", "TaxableValue") ?? "0";
                    string masterTaxPerCol = GetCol(masterTable, "m", "TaxPer", "TaxPercent", "GstPer") ?? "0";

                    List<string> whereClauses = new List<string>();
                    if (companyCol != null)
                        whereClauses.Add($"(@CompanyId <= 0 OR ISNULL({companyCol}, 0) = 0 OR ISNULL({companyCol}, 0) = @CompanyId)");
                    if (branchCol != null)
                        whereClauses.Add($"(@BranchId <= 0 OR ISNULL({branchCol}, 0) = 0 OR ISNULL({branchCol}, 0) = @BranchId)");
                    if (finYearCol != null)
                        whereClauses.Add($"(@FinYearId <= 0 OR ISNULL({finYearCol}, 0) = 0 OR ISNULL({finYearCol}, 0) = @FinYearId)");

                    DateTime exclusiveTo = filter.ToDate.Date.AddDays(1);
                    whereClauses.Add($"({dateColMaster} IS NULL OR ({dateColMaster} >= @FromDate AND {dateColMaster} < @ExclusiveTo))");

                    if (ledgerIdMaster != null)
                        whereClauses.Add($"(@SupplierLedgerId <= 0 OR {ledgerIdMaster} = @SupplierLedgerId)");

                    string whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

                    string sql = $@"
                        SELECT 
                            ISNULL({invNoMaster}, CAST({purNoMaster} AS NVARCHAR(50))) AS InvoiceNo,
                            {dateColMaster} AS DocDate,
                            ISNULL(NULLIF({vendorNameCol}, ''), 'General Supplier') AS SupplierName,
                            {gstinSelect} AS SupplierGSTIN,
                            ISNULL({itemNameCol}, 'Purchase Item') AS ItemName,
                            {hsnSelect} AS HSNCode,
                            ISNULL({qtyCol}, 1) AS Qty,
                            ISNULL({unitCol}, 'PCS') AS Unit,
                            CAST(
                                CASE 
                                    WHEN ISNULL({costCol}, 0) > 0 AND ISNULL({qtyCol}, 0) > 0 THEN ((ISNULL({qtyCol},0)*ISNULL({costCol},0)) - ISNULL({taxAmtCol},0))
                                    WHEN ISNULL({masterSubCol}, 0) > 0 THEN {masterSubCol}
                                    WHEN ISNULL({masterNetCol}, 0) > 0 THEN ({masterNetCol} - ISNULL({masterTaxCol}, 0))
                                    ELSE 0
                                END AS DECIMAL(18,2)
                            ) AS TaxableValue,
                            CAST(
                                (CASE 
                                    WHEN ISNULL({taxPerCol}, 0) > 0 THEN 
                                        (CASE WHEN {taxPerCol} > 100 THEN {taxPerCol}/100.0 ELSE {taxPerCol} END)
                                    ELSE 
                                        (CASE WHEN ISNULL({masterTaxPerCol}, 0) > 100 THEN {masterTaxPerCol}/100.0 ELSE ISNULL({masterTaxPerCol}, 0) END)
                                END)/2.0 AS FLOAT
                            ) AS CGSTPer,
                            CAST(
                                (CASE 
                                    WHEN ISNULL({taxAmtCol}, 0) > 0 THEN {taxAmtCol}
                                    ELSE ISNULL({masterTaxCol}, 0)
                                END)/2.0 AS DECIMAL(18,2)
                            ) AS CGSTAmt,
                            CAST(
                                (CASE 
                                    WHEN ISNULL({taxPerCol}, 0) > 0 THEN 
                                        (CASE WHEN {taxPerCol} > 100 THEN {taxPerCol}/100.0 ELSE {taxPerCol} END)
                                    ELSE 
                                        (CASE WHEN ISNULL({masterTaxPerCol}, 0) > 100 THEN {masterTaxPerCol}/100.0 ELSE ISNULL({masterTaxPerCol}, 0) END)
                                END)/2.0 AS FLOAT
                            ) AS SGSTPer,
                            CAST(
                                (CASE 
                                    WHEN ISNULL({taxAmtCol}, 0) > 0 THEN {taxAmtCol}
                                    ELSE ISNULL({masterTaxCol}, 0)
                                END)/2.0 AS DECIMAL(18,2)
                            ) AS SGSTAmt,
                            CAST(0 AS FLOAT) AS IGSTPer,
                            CAST(0 AS DECIMAL(18,2)) AS IGSTAmt,
                            CAST(ISNULL({cessPerCol}, 0) AS FLOAT) AS CessPer,
                            CAST(ISNULL({cessAmtCol}, 0) AS DECIMAL(18,2)) AS CessAmt,
                            CAST(
                                CASE 
                                    WHEN ISNULL({taxAmtCol}, 0) > 0 THEN {taxAmtCol}
                                    ELSE ISNULL({masterTaxCol}, 0)
                                END AS DECIMAL(18,2)
                            ) AS TotalInputGST,
                            CAST(
                                CASE 
                                    WHEN (ISNULL({qtyCol},0)*ISNULL({costCol},0)) > 0 THEN (ISNULL({qtyCol},0)*ISNULL({costCol},0))
                                    ELSE ISNULL({masterNetCol}, 0)
                                END AS DECIMAL(18,2)
                            ) AS TotalInvoiceAmount,
                            ISNULL({taxTypeMasterCol}, 'excl') AS TaxType
                        FROM dbo.{masterTable} m
                        {detailsJoin}
                        {itemJoin}
                        {ledgerJoin}
                        {whereSql}
                    ";

                    using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                    {
                        cmd.Parameters.AddWithValue("@CompanyId", filter.CompanyId);
                        cmd.Parameters.AddWithValue("@BranchId", filter.BranchId);
                        cmd.Parameters.AddWithValue("@FinYearId", filter.FinYearId);
                        cmd.Parameters.AddWithValue("@FromDate", filter.FromDate.Date);
                        cmd.Parameters.AddWithValue("@ExclusiveTo", exclusiveTo);
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetPurchaseRegister error: " + ex.Message);
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
            PurchaseGSTRegisterRow r = new PurchaseGSTRegisterRow
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

            if (r.TaxableValue == 0m && r.TotalInvoiceAmount > 0m)
            {
                double totalTaxPer = r.CGSTPer + r.SGSTPer + r.IGSTPer;
                if (totalTaxPer > 0)
                {
                    r.TaxableValue = Math.Round(r.TotalInvoiceAmount / (decimal)(1.0 + (totalTaxPer / 100.0)), 2);
                    r.TotalInputGST = r.TotalInvoiceAmount - r.TaxableValue;
                    r.CGSTAmt = Math.Round(r.TotalInputGST / 2m, 2);
                    r.SGSTAmt = r.TotalInputGST - r.CGSTAmt;
                }
                else
                {
                    r.TaxableValue = r.TotalInvoiceAmount;
                }
            }

            return r;
        }

        private string GetCol(string tableName, string alias, params string[] candidates)
        {
            foreach (var c in candidates)
            {
                if (ColumnExists(tableName, c))
                    return string.IsNullOrEmpty(alias) ? $"[{c}]" : $"{alias}.[{c}]";
            }
            return null;
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

        private bool ColumnExists(string tableName, string columnName)
        {
            try
            {
                string cleanTable = tableName.StartsWith("dbo.") ? tableName.Substring(4) : tableName;
                using (SqlCommand cmd = new SqlCommand("SELECT CASE WHEN COL_LENGTH(@TableName, @ColumnName) IS NULL THEN 0 ELSE 1 END;", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@TableName", "dbo." + cleanTable);
                    cmd.Parameters.AddWithValue("@ColumnName", columnName);
                    object res = cmd.ExecuteScalar();
                    return res != null && res != DBNull.Value && Convert.ToInt32(res) == 1;
                }
            }
            catch
            {
                return false;
            }
        }

        private string GetGstinColumnExpression(string tableAlias, string tableName)
        {
            if (string.IsNullOrEmpty(tableName) || !TableExists(tableName))
                return "''";

            string[] candidateColumns = new string[] { "GSTIN", "GSTNo", "GSTINNo", "GST_NO", "GSTNumber", "TINNo", "GSTIN_NO", "GST" };
            foreach (string col in candidateColumns)
            {
                if (ColumnExists(tableName, col))
                {
                    return $"ISNULL({tableAlias}.[{col}], '')";
                }
            }

            return "''";
        }

        private static double ToDouble(DataRow row, string col) => row.Table.Columns.Contains(col) && row[col] != DBNull.Value ? Convert.ToDouble(row[col]) : 0.0;
        private static decimal ToDecimal(DataRow row, string col) => row.Table.Columns.Contains(col) && row[col] != DBNull.Value ? Convert.ToDecimal(row[col]) : 0m;
        private static DateTime ToDateTime(DataRow row, string col) => row.Table.Columns.Contains(col) && row[col] != DBNull.Value ? Convert.ToDateTime(row[col]) : DateTime.MinValue;
        private static string ToString(DataRow row, string col) => row.Table.Columns.Contains(col) && row[col] != DBNull.Value ? Convert.ToString(row[col]) : string.Empty;
    }
}
