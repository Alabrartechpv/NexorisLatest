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

                // 1. Stored Procedure Attempt (_POS_Sales_Master_for_Report)
                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_Sales_Master_for_Report, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@FromDate", filter.FromDate.Date);
                        cmd.Parameters.AddWithValue("@ToDate", filter.ToDate.Date.AddDays(1));
                        if (filter.BranchId > 0)
                        {
                            cmd.Parameters.AddWithValue("@BranchId", filter.BranchId);
                        }

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    decimal netAmt = ToDecimal(row, "NetAmount");
                                    decimal taxAmt = ToDecimal(row, "TaxAmt");
                                    decimal subTotal = ToDecimal(row, "SubTotal");
                                    if (subTotal == 0m && netAmt > 0m) subTotal = netAmt - taxAmt;
                                    string gstin = ToString(row, "GSTIN");
                                    string custName = ToString(row, "customername");
                                    if (string.IsNullOrEmpty(custName)) custName = ToString(row, "CustomerName");
                                    if (string.IsNullOrEmpty(custName)) custName = "Walk-in Customer";

                                    list.Add(new SalesGSTRegisterRow
                                    {
                                        InvoiceNo = ToString(row, "BillNo"),
                                        DocDate = ToDateTime(row, "BillDate"),
                                        CustomerName = custName,
                                        CustomerGSTIN = gstin,
                                        SaleType = gstin.Length >= 15 ? "B2B" : "B2C",
                                        ItemName = "Sales Transaction",
                                        HSNCode = "",
                                        Qty = 1,
                                        Unit = "PCS",
                                        TaxableValue = subTotal,
                                        CGSTPer = taxAmt > 0 && subTotal > 0 ? (double)Math.Round((taxAmt / subTotal) * 50m, 2) : 0,
                                        CGSTAmt = Math.Round(taxAmt / 2m, 2),
                                        SGSTPer = taxAmt > 0 && subTotal > 0 ? (double)Math.Round((taxAmt / subTotal) * 50m, 2) : 0,
                                        SGSTAmt = Math.Round(taxAmt / 2m, 2),
                                        IGSTPer = 0,
                                        IGSTAmt = 0m,
                                        CessPer = 0,
                                        CessAmt = 0m,
                                        TotalOutputGST = taxAmt,
                                        TotalInvoiceAmount = netAmt,
                                        TaxType = "incl"
                                    });
                                }
                                return list;
                            }
                        }
                    }
                }
                catch
                {
                    // Fallback to Direct Query
                }

                // 2. Direct Query Fallback
                string masterTable = TableExists("SMaster") ? "SMaster" : (TableExists("SalesMaster") ? "SalesMaster" : "");
                string detailsTable = TableExists("SDetails") ? "SDetails" : (TableExists("SalesDetails") ? "SalesDetails" : "");

                if (!string.IsNullOrEmpty(masterTable))
                {
                    string billNoMaster = GetCol(masterTable, "m", "BillNo", "Billno", "Bill_No") ?? "m.BillNo";
                    string dateColMaster = GetCol(masterTable, "m", "BillDate", "Billdate", "DocDate", "Bill_Date", "CreatedOn") ?? "m.BillDate";
                    string custNameCol = GetCol(masterTable, "m", "CustomerName", "customername", "CustName") ?? "m.CustomerName";
                    string ledgerIdMaster = GetCol(masterTable, "m", "LedgerID", "LedgerId", "CustomerID", "CustomerId");
                    string companyCol = GetCol(masterTable, "m", "CompanyId", "CompanyID", "Company_Id");
                    string branchCol = GetCol(masterTable, "m", "BranchId", "BranchID", "Branch_Id");
                    string finYearCol = GetCol(masterTable, "m", "FinYearId", "FinYearID", "FinYear_Id");
                    string statusCol = GetCol(masterTable, "m", "Status", "status");
                    string cancelCol = GetCol(masterTable, "m", "CancelFlag", "cancelflag");

                    string detailsJoin = "";
                    string itemNameCol = "''";
                    string qtyCol = "0";
                    string unitCol = "''";
                    string unitPriceCol = "0";
                    string baseAmtCol = "0";
                    string taxAmtCol = "0";
                    string taxPerCol = "0";
                    string totalAmtCol = "0";
                    string taxTypeCol = "'incl'";

                    if (!string.IsNullOrEmpty(detailsTable))
                    {
                        string billNoDetails = GetCol(detailsTable, "d", "BillNo", "Billno", "Bill_No") ?? "d.BillNo";
                        detailsJoin = $"LEFT JOIN dbo.{detailsTable} d ON {billNoMaster} = {billNoDetails}";

                        itemNameCol = GetCol(detailsTable, "d", "ItemName", "Item_Name", "itemname") ?? "''";
                        qtyCol = GetCol(detailsTable, "d", "Qty", "qty", "Quantity") ?? "0";
                        unitCol = GetCol(detailsTable, "d", "Unit", "unit", "UOM") ?? "''";
                        unitPriceCol = GetCol(detailsTable, "d", "UnitPrice", "Rate", "Price", "Cost", "unitprice") ?? "0";
                        baseAmtCol = GetCol(detailsTable, "d", "BaseAmount", "BaseAmt", "TaxableValue", "SubTotal") ?? "0";
                        taxAmtCol = GetCol(detailsTable, "d", "TaxAmt", "TaxAmount", "GstAmt", "GSTAmount") ?? "0";
                        taxPerCol = GetCol(detailsTable, "d", "TaxPer", "TaxPercent", "GstPer", "GSTPer") ?? "0";
                        totalAmtCol = GetCol(detailsTable, "d", "TotalAmount", "Amount", "Total", "netamount") ?? "0";
                        taxTypeCol = GetCol(detailsTable, "d", "TaxType", "taxtype") ?? "'incl'";
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
                    if (cancelCol != null)
                        whereClauses.Add($"ISNULL({cancelCol}, 0) = 0");

                    DateTime exclusiveTo = filter.ToDate.Date.AddDays(1);
                    whereClauses.Add($"({dateColMaster} IS NULL OR ({dateColMaster} >= @FromDate AND {dateColMaster} < @ExclusiveTo))");

                    if (ledgerIdMaster != null)
                        whereClauses.Add($"(@CustomerLedgerId <= 0 OR {ledgerIdMaster} = @CustomerLedgerId)");

                    string whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

                    string sql = $@"
                        SELECT 
                            CAST(ISNULL({billNoMaster}, 0) AS NVARCHAR(50)) AS InvoiceNo,
                            {dateColMaster} AS DocDate,
                            ISNULL(NULLIF({custNameCol}, ''), 'Walk-in Customer') AS CustomerName,
                            {gstinSelect} AS CustomerGSTIN,
                            CASE WHEN LEN({gstinSelect}) >= 15 THEN 'B2B' ELSE 'B2C' END AS SaleType,
                            ISNULL({itemNameCol}, 'Sales Item') AS ItemName,
                            {hsnSelect} AS HSNCode,
                            ISNULL({qtyCol}, 1) AS Qty,
                            ISNULL({unitCol}, 'PCS') AS Unit,
                            CAST(
                                CASE 
                                    WHEN ISNULL({baseAmtCol}, 0) > 0 THEN {baseAmtCol}
                                    WHEN ISNULL({masterSubCol}, 0) > 0 THEN {masterSubCol}
                                    WHEN ISNULL({masterNetCol}, 0) > 0 THEN ({masterNetCol} - ISNULL({masterTaxCol}, 0))
                                    ELSE ((ISNULL({qtyCol},0)*ISNULL({unitPriceCol},0)) - ISNULL({taxAmtCol},0))
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
                            CAST(0 AS FLOAT) AS CessPer,
                            CAST(0 AS DECIMAL(18,2)) AS CessAmt,
                            CAST(
                                CASE 
                                    WHEN ISNULL({taxAmtCol}, 0) > 0 THEN {taxAmtCol}
                                    ELSE ISNULL({masterTaxCol}, 0)
                                END AS DECIMAL(18,2)
                            ) AS TotalOutputGST,
                            CAST(
                                CASE 
                                    WHEN ISNULL({totalAmtCol}, 0) > 0 THEN {totalAmtCol}
                                    WHEN (ISNULL({qtyCol},0)*ISNULL({unitPriceCol},0)) > 0 THEN (ISNULL({qtyCol},0)*ISNULL({unitPriceCol},0))
                                    ELSE ISNULL({masterNetCol}, 0)
                                END AS DECIMAL(18,2)
                            ) AS TotalInvoiceAmount,
                            ISNULL({taxTypeCol}, 'incl') AS TaxType
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetSalesRegister error: " + ex.Message);
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
            SalesGSTRegisterRow r = new SalesGSTRegisterRow
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

            if (r.TaxableValue == 0m && r.TotalInvoiceAmount > 0m)
            {
                double totalTaxPer = r.CGSTPer + r.SGSTPer + r.IGSTPer;
                if (totalTaxPer > 0)
                {
                    r.TaxableValue = Math.Round(r.TotalInvoiceAmount / (decimal)(1.0 + (totalTaxPer / 100.0)), 2);
                    r.TotalOutputGST = r.TotalInvoiceAmount - r.TaxableValue;
                    r.CGSTAmt = Math.Round(r.TotalOutputGST / 2m, 2);
                    r.SGSTAmt = r.TotalOutputGST - r.CGSTAmt;
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
