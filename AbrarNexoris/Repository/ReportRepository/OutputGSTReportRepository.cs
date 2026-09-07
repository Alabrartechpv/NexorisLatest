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
        private string _cachedCompanyStateCode = null;

        public List<SalesGSTRegisterRow> GetSalesRegister(OutputGSTReportFilter filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            List<SalesGSTRegisterRow> list = new List<SalesGSTRegisterRow>();

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                string companyStateCode = GetCompanyStateCode();

                string masterTable = TableExists("SMaster") ? "SMaster" : (TableExists("SalesMaster") ? "SalesMaster" : "");
                string detailsTable = TableExists("SDetails") ? "SDetails" : (TableExists("SalesDetails") ? "SalesDetails" : "");

                if (string.IsNullOrEmpty(masterTable))
                {
                    return list;
                }

                string billNoMaster = GetCol(masterTable, "m", "BillNo", "Billno", "Bill_No") ?? "m.BillNo";
                string dateColMaster = GetCol(masterTable, "m", "BillDate", "Billdate", "DocDate", "Bill_Date", "CreatedOn") ?? "m.BillDate";
                string custNameCol = GetCol(masterTable, "m", "CustomerName", "customername", "CustName") ?? "m.CustomerName";
                string ledgerIdMaster = GetCol(masterTable, "m", "LedgerID", "LedgerId", "CustomerID", "CustomerId");
                string companyCol = GetCol(masterTable, "m", "CompanyId", "CompanyID", "Company_Id");
                string branchCol = GetCol(masterTable, "m", "BranchId", "BranchID", "Branch_Id");
                string finYearCol = GetCol(masterTable, "m", "FinYearId", "FinYearID", "FinYear_Id");
                string statusCol = GetCol(masterTable, "m", "Status", "status");
                string cancelCol = GetCol(masterTable, "m", "CancelFlag", "cancelflag");
                string stateIdMaster = GetCol(masterTable, "m", "StateId", "StateID", "State_Id", "State");

                string detailsJoin = "";
                string itemNameCol = "''";
                string qtyCol = "0";
                string unitCol = "''";
                string unitPriceCol = "0";
                string baseAmtCol = "0";
                string taxAmtCol = "0";
                string taxPerCol = "0";
                string cessPerCol = "0";
                string cessAmtCol = "0";
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
                    cessPerCol = GetCol(detailsTable, "d", "CessPer", "CessPercent") ?? "0";
                    cessAmtCol = GetCol(detailsTable, "d", "CessAmt", "CessAmount") ?? "0";
                    totalAmtCol = GetCol(detailsTable, "d", "TotalAmount", "Amount", "Total", "netamount") ?? "0";
                    taxTypeCol = GetCol(detailsTable, "d", "TaxType", "taxtype") ?? "'incl'";
                }

                string hasItemMaster = TableExists("ItemMaster") ? "ItemMaster" : "";
                string itemJoin = "";
                string hsnSelect = "''";
                if (!string.IsNullOrEmpty(hasItemMaster) && !string.IsNullOrEmpty(detailsTable))
                {
                    string itemKeyDetails = GetCol(detailsTable, "d", "ItemId", "ItemID");
                    string itemKeyMaster = GetCol("ItemMaster", "i", "ItemId", "ItemID");
                    string hsnCol = GetCol("ItemMaster", "i", "HSNCode", "HSN", "HsnCode");
                    if (itemKeyDetails != null && itemKeyMaster != null)
                    {
                        itemJoin = $"LEFT JOIN dbo.ItemMaster i ON {itemKeyDetails} = {itemKeyMaster}";
                        if (hsnCol != null) hsnSelect = $"ISNULL({hsnCol}, '')";
                    }
                }

                // Check ContactDetails table or LedgerMaster table safely
                string contactTable = TableExists("ContactDetails") ? "ContactDetails" : (TableExists("CustomerMaster") ? "CustomerMaster" : "");
                string ledgerJoin = "";
                string gstinSelect = "''";
                string custStateSelect = "''";
                if (!string.IsNullOrEmpty(contactTable) && ledgerIdMaster != null)
                {
                    string contactKey = GetCol(contactTable, "c", "LedgerID", "LedgerId", "CustomerID", "CustomerId");
                    if (contactKey != null)
                    {
                        ledgerJoin = $"LEFT JOIN dbo.{contactTable} c ON {ledgerIdMaster} = {contactKey}";
                        string gstinCol = GetCol(contactTable, "c", "TINNumber", "GSTIN", "GSTNo", "GST_NO");
                        if (gstinCol != null) gstinSelect = $"ISNULL({gstinCol}, '')";

                        string custStateCol = GetCol(contactTable, "c", "StateID", "State", "StateName");
                        if (custStateCol != null) custStateSelect = $"ISNULL(CAST({custStateCol} AS NVARCHAR(100)), '')";
                    }
                }

                string masterNetCol = GetCol(masterTable, "m", "NetAmount", "NetAmt", "GrandTotal", "TotalAmount") ?? "0";
                string masterTaxCol = GetCol(masterTable, "m", "TaxAmt", "TaxAmount", "GstAmt") ?? "0";
                string masterSubCol = GetCol(masterTable, "m", "SubTotal", "BaseAmount", "TaxableValue") ?? "0";
                string masterTaxPerCol = GetCol(masterTable, "m", "TaxPer", "TaxPercent", "GstPer") ?? "0";

                DateTime fromDate = filter.FromDate.Date;
                DateTime exclusiveToDate = filter.ToDate.Date.AddDays(1);

                // Attempt 1: Date Range Filter
                list = ExecuteSalesQuery(
                    masterTable, detailsJoin, itemJoin, ledgerJoin,
                    billNoMaster, dateColMaster, custNameCol, gstinSelect, custStateSelect, stateIdMaster,
                    itemNameCol, hsnSelect, qtyCol, unitCol, unitPriceCol, baseAmtCol, taxAmtCol, taxPerCol,
                    cessPerCol, cessAmtCol, totalAmtCol, masterSubCol, masterTaxCol, masterTaxPerCol, masterNetCol, taxTypeCol,
                    companyCol, branchCol, finYearCol, cancelCol, statusCol, ledgerIdMaster,
                    filter, fromDate, exclusiveToDate, companyStateCode, useDateFilter: true
                );

                // Attempt 2: If date filter yielded 0 rows, fetch all non-cancelled sales bills in masterTable
                if (list.Count == 0)
                {
                    list = ExecuteSalesQuery(
                        masterTable, detailsJoin, itemJoin, ledgerJoin,
                        billNoMaster, dateColMaster, custNameCol, gstinSelect, custStateSelect, stateIdMaster,
                        itemNameCol, hsnSelect, qtyCol, unitCol, unitPriceCol, baseAmtCol, taxAmtCol, taxPerCol,
                        cessPerCol, cessAmtCol, totalAmtCol, masterSubCol, masterTaxCol, masterTaxPerCol, masterNetCol, taxTypeCol,
                        companyCol, branchCol, finYearCol, cancelCol, statusCol, ledgerIdMaster,
                        filter, fromDate, exclusiveToDate, companyStateCode, useDateFilter: false
                    );
                }

                // Attempt 3: Direct master query fallback
                if (list.Count == 0)
                {
                    list = ExecuteDirectMasterQuery(masterTable, billNoMaster, dateColMaster, custNameCol, masterSubCol, masterTaxCol, masterTaxPerCol, masterNetCol, companyStateCode);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetSalesRegister error: " + ex.Message);
                throw new Exception("Sales Register Query Error: " + ex.Message, ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return list;
        }

        private List<SalesGSTRegisterRow> ExecuteSalesQuery(
            string masterTable, string detailsJoin, string itemJoin, string ledgerJoin,
            string billNoMaster, string dateColMaster, string custNameCol, string gstinSelect, string custStateSelect, string stateIdMaster,
            string itemNameCol, string hsnSelect, string qtyCol, string unitCol, string unitPriceCol, string baseAmtCol, string taxAmtCol, string taxPerCol,
            string cessPerCol, string cessAmtCol, string totalAmtCol, string masterSubCol, string masterTaxCol, string masterTaxPerCol, string masterNetCol, string taxTypeCol,
            string companyCol, string branchCol, string finYearCol, string cancelCol, string statusCol, string ledgerIdMaster,
            OutputGSTReportFilter filter, DateTime fromDate, DateTime exclusiveToDate, string companyStateCode, bool useDateFilter)
        {
            List<SalesGSTRegisterRow> list = new List<SalesGSTRegisterRow>();
            List<string> whereClauses = new List<string>();

            if (companyCol != null)
                whereClauses.Add($"(@CompanyId <= 0 OR ISNULL({companyCol}, 0) = 0 OR ISNULL({companyCol}, 0) = @CompanyId)");
            if (branchCol != null)
                whereClauses.Add($"(@BranchId <= 0 OR ISNULL({branchCol}, 0) = 0 OR ISNULL({branchCol}, 0) = @BranchId)");
            if (finYearCol != null)
                whereClauses.Add($"(@FinYearId <= 0 OR ISNULL({finYearCol}, 0) = 0 OR ISNULL({finYearCol}, 0) = @FinYearId)");

            if (cancelCol != null)
                whereClauses.Add($"({cancelCol} IS NULL OR ISNULL(CAST({cancelCol} AS NVARCHAR(10)), '0') NOT IN ('1', 'Y', 'true'))");
            if (statusCol != null)
                whereClauses.Add($"({statusCol} IS NULL OR LOWER(ISNULL(CAST({statusCol} AS NVARCHAR(50)), '')) NOT IN ('cancel', 'cancelled'))");

            if (useDateFilter)
            {
                whereClauses.Add($"({dateColMaster} IS NULL OR ({dateColMaster} >= @FromDate AND {dateColMaster} < @ExclusiveTo))");
            }

            if (ledgerIdMaster != null && filter.CustomerLedgerId > 0)
                whereClauses.Add($"{ledgerIdMaster} = @CustomerLedgerId");

            string whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

            string sql = $@"
                SELECT 
                    CAST(ISNULL({billNoMaster}, 0) AS NVARCHAR(50)) AS InvoiceNo,
                    ISNULL({dateColMaster}, GETDATE()) AS DocDate,
                    ISNULL(NULLIF({custNameCol}, ''), 'Walk-in Customer') AS CustomerName,
                    {gstinSelect} AS CustomerGSTIN,
                    {custStateSelect} AS CustomerState,
                    {(stateIdMaster != null ? $"ISNULL(CAST({stateIdMaster} AS NVARCHAR(50)), '')" : "''")} AS MasterStateId,
                    ISNULL({itemNameCol}, 'Sales Item') AS ItemName,
                    {hsnSelect} AS HSNCode,
                    ISNULL({qtyCol}, 1) AS Qty,
                    ISNULL({unitCol}, 'PCS') AS Unit,
                    ISNULL({unitPriceCol}, 0) AS UnitPrice,
                    ISNULL({baseAmtCol}, 0) AS BaseAmount,
                    ISNULL({taxAmtCol}, 0) AS DetailTaxAmt,
                    ISNULL({taxPerCol}, 0) AS DetailTaxPer,
                    ISNULL({cessPerCol}, 0) AS CessPer,
                    ISNULL({cessAmtCol}, 0) AS CessAmt,
                    ISNULL({totalAmtCol}, 0) AS DetailTotalAmt,
                    ISNULL({masterSubCol}, 0) AS MasterSubTotal,
                    ISNULL({masterTaxCol}, 0) AS MasterTaxAmt,
                    ISNULL({masterTaxPerCol}, 0) AS MasterTaxPer,
                    ISNULL({masterNetCol}, 0) AS MasterNetAmount,
                    ISNULL({taxTypeCol}, 'incl') AS TaxType
                FROM dbo.{masterTable} m
                {detailsJoin}
                {itemJoin}
                {ledgerJoin}
                {whereSql}
                ORDER BY {dateColMaster} ASC, {billNoMaster} ASC
            ";

            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
            {
                cmd.Parameters.AddWithValue("@CompanyId", filter.CompanyId);
                cmd.Parameters.AddWithValue("@BranchId", filter.BranchId);
                cmd.Parameters.AddWithValue("@FinYearId", filter.FinYearId);
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ExclusiveTo", exclusiveToDate);
                if (ledgerIdMaster != null && filter.CustomerLedgerId > 0)
                {
                    cmd.Parameters.AddWithValue("@CustomerLedgerId", filter.CustomerLedgerId);
                }

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    foreach (DataRow row in dt.Rows)
                    {
                        var parsedRow = ProcessSalesRow(row, companyStateCode);
                        if (parsedRow != null)
                        {
                            if (!string.IsNullOrWhiteSpace(filter.SearchText))
                            {
                                string s = filter.SearchText.Trim().ToLowerInvariant();
                                bool matches = (parsedRow.InvoiceNo ?? "").ToLowerInvariant().Contains(s) ||
                                                (parsedRow.CustomerName ?? "").ToLowerInvariant().Contains(s) ||
                                                (parsedRow.CustomerGSTIN ?? "").ToLowerInvariant().Contains(s) ||
                                                (parsedRow.ItemName ?? "").ToLowerInvariant().Contains(s) ||
                                                (parsedRow.HSNCode ?? "").ToLowerInvariant().Contains(s);
                                if (!matches) continue;
                            }
                            list.Add(parsedRow);
                        }
                    }
                }
            }

            return list;
        }

        private List<SalesGSTRegisterRow> ExecuteDirectMasterQuery(string masterTable, string billNoMaster, string dateColMaster, string custNameCol, string masterSubCol, string masterTaxCol, string masterTaxPerCol, string masterNetCol, string companyStateCode)
        {
            List<SalesGSTRegisterRow> list = new List<SalesGSTRegisterRow>();
            try
            {
                string sql = $@"
                    SELECT 
                        CAST(ISNULL({billNoMaster}, 0) AS NVARCHAR(50)) AS InvoiceNo,
                        ISNULL({dateColMaster}, GETDATE()) AS DocDate,
                        ISNULL(NULLIF({custNameCol}, ''), 'Walk-in Customer') AS CustomerName,
                        '' AS CustomerGSTIN,
                        '' AS CustomerState,
                        '' AS MasterStateId,
                        'Sales Transaction' AS ItemName,
                        '' AS HSNCode,
                        1 AS Qty,
                        'PCS' AS Unit,
                        ISNULL({masterNetCol}, 0) AS UnitPrice,
                        ISNULL({masterSubCol}, 0) AS BaseAmount,
                        ISNULL({masterTaxCol}, 0) AS DetailTaxAmt,
                        ISNULL({masterTaxPerCol}, 0) AS DetailTaxPer,
                        0 AS CessPer,
                        0 AS CessAmt,
                        ISNULL({masterNetCol}, 0) AS DetailTotalAmt,
                        ISNULL({masterSubCol}, 0) AS MasterSubTotal,
                        ISNULL({masterTaxCol}, 0) AS MasterTaxAmt,
                        ISNULL({masterTaxPerCol}, 0) AS MasterTaxPer,
                        ISNULL({masterNetCol}, 0) AS MasterNetAmount,
                        'incl' AS TaxType
                    FROM dbo.{masterTable} m
                    ORDER BY {dateColMaster} ASC, {billNoMaster} ASC
                ";

                using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        foreach (DataRow row in dt.Rows)
                        {
                            var parsedRow = ProcessSalesRow(row, companyStateCode);
                            if (parsedRow != null) list.Add(parsedRow);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ExecuteDirectMasterQuery error: " + ex.Message);
            }
            return list;
        }

        private SalesGSTRegisterRow ProcessSalesRow(DataRow row, string companyStateCode)
        {
            string invoiceNo = ToString(row, "InvoiceNo");
            DateTime docDate = ToDateTime(row, "DocDate");
            string customerName = ToString(row, "CustomerName");
            string gstin = ToString(row, "CustomerGSTIN").Trim().ToUpperInvariant();
            string custState = ToString(row, "CustomerState").Trim();
            string itemName = ToString(row, "ItemName");
            string hsnCode = ToString(row, "HSNCode");
            double qty = ToDouble(row, "Qty");
            string unit = ToString(row, "Unit");
            if (string.IsNullOrWhiteSpace(unit)) unit = "PCS";

            double unitPrice = ToDouble(row, "UnitPrice");
            decimal baseAmt = ToDecimal(row, "BaseAmount");
            decimal detailTaxAmt = ToDecimal(row, "DetailTaxAmt");
            double detailTaxPer = ToDouble(row, "DetailTaxPer");
            double cessPer = ToDouble(row, "CessPer");
            decimal cessAmt = ToDecimal(row, "CessAmt");
            decimal detailTotalAmt = ToDecimal(row, "DetailTotalAmt");

            decimal masterSubTotal = ToDecimal(row, "MasterSubTotal");
            decimal masterTaxAmt = ToDecimal(row, "MasterTaxAmt");
            double masterTaxPer = ToDouble(row, "MasterTaxPer");
            decimal masterNetAmount = ToDecimal(row, "MasterNetAmount");
            string taxType = ToString(row, "TaxType").ToLowerInvariant();

            double rawTaxPer = detailTaxPer > 0 ? detailTaxPer : masterTaxPer;
            double effectiveTaxPer = rawTaxPer > 100 ? rawTaxPer / 100.0 : rawTaxPer;

            decimal lineTotal = detailTotalAmt > 0 ? detailTotalAmt : (decimal)(qty * unitPrice);
            if (lineTotal == 0m && masterNetAmount > 0m) lineTotal = masterNetAmount;

            decimal taxableValue = 0m;
            decimal totalTaxAmt = 0m;

            if (taxType.Contains("excl"))
            {
                taxableValue = baseAmt > 0m ? baseAmt : lineTotal;
                totalTaxAmt = detailTaxAmt > 0m ? detailTaxAmt : Math.Round(taxableValue * (decimal)(effectiveTaxPer / 100.0), 2);
                if (detailTotalAmt == 0m) lineTotal = taxableValue + totalTaxAmt;
            }
            else
            {
                if (baseAmt > 0m && detailTaxAmt > 0m)
                {
                    taxableValue = baseAmt;
                    totalTaxAmt = detailTaxAmt;
                }
                else if (effectiveTaxPer > 0)
                {
                    taxableValue = Math.Round(lineTotal / (decimal)(1.0 + (effectiveTaxPer / 100.0)), 2);
                    totalTaxAmt = lineTotal - taxableValue;
                }
                else
                {
                    taxableValue = lineTotal > 0m ? lineTotal : (masterSubTotal > 0m ? masterSubTotal : masterNetAmount - masterTaxAmt);
                    totalTaxAmt = detailTaxAmt > 0m ? detailTaxAmt : masterTaxAmt;
                }
            }

            if (taxableValue < 0m) taxableValue = 0m;
            if (totalTaxAmt < 0m) totalTaxAmt = 0m;

            bool isB2B = gstin.Length >= 15;
            string customerType = isB2B ? "B2B" : "B2C";

            string customerStateCode = "";
            if (gstin.Length >= 2 && char.IsDigit(gstin[0]) && char.IsDigit(gstin[1]))
            {
                customerStateCode = gstin.Substring(0, 2);
            }
            else if (!string.IsNullOrEmpty(custState))
            {
                customerStateCode = GetStateCodeFromNameOrId(custState);
            }

            if (string.IsNullOrEmpty(customerStateCode))
            {
                customerStateCode = companyStateCode;
            }

            bool isIntraState = string.Equals(companyStateCode, customerStateCode, StringComparison.OrdinalIgnoreCase);
            string supplyType = isIntraState ? "INTRA-STATE" : "INTER-STATE";
            string placeOfSupply = GetStateNameFromCode(customerStateCode);

            double cgstPer = 0, sgstPer = 0, igstPer = 0;
            decimal cgstAmt = 0m, sgstAmt = 0m, igstAmt = 0m;

            if (isIntraState)
            {
                cgstPer = effectiveTaxPer / 2.0;
                sgstPer = effectiveTaxPer / 2.0;
                cgstAmt = Math.Round(totalTaxAmt / 2.0m, 2);
                sgstAmt = totalTaxAmt - cgstAmt;
            }
            else
            {
                igstPer = effectiveTaxPer;
                igstAmt = totalTaxAmt;
            }

            decimal totalOutputGST = cgstAmt + sgstAmt + igstAmt + cessAmt;

            return new SalesGSTRegisterRow
            {
                InvoiceNo = invoiceNo,
                DocDate = docDate,
                CustomerName = customerName,
                CustomerGSTIN = gstin,
                SaleType = customerType,
                ItemName = itemName,
                HSNCode = hsnCode,
                Qty = qty,
                Unit = unit,
                TaxableValue = taxableValue,
                CGSTPer = cgstPer,
                CGSTAmt = cgstAmt,
                SGSTPer = sgstPer,
                SGSTAmt = sgstAmt,
                IGSTPer = igstPer,
                IGSTAmt = igstAmt,
                CessPer = cessPer,
                CessAmt = cessAmt,
                TotalOutputGST = totalOutputGST,
                TotalInvoiceAmount = lineTotal,
                PlaceOfSupply = placeOfSupply,
                SupplyType = supplyType,
                TaxType = taxType
            };
        }

        public List<OutputGSTSummaryRow> GetOutputSummary(OutputGSTReportFilter filter)
        {
            List<SalesGSTRegisterRow> register = GetSalesRegister(filter);

            var taxableRows = register.Where(x => (x.CGSTPer + x.SGSTPer + x.IGSTPer) > 0 || x.TotalOutputGST > 0).ToList();
            var zeroRateRows = register.Where(x => (x.CGSTPer + x.SGSTPer + x.IGSTPer) == 0 && x.TotalOutputGST == 0).ToList();

            return new List<OutputGSTSummaryRow>
            {
                new OutputGSTSummaryRow
                {
                    SalesCategory = "Taxable Sales",
                    TaxableValue = taxableRows.Sum(x => x.TaxableValue),
                    CGSTAmt = taxableRows.Sum(x => x.CGSTAmt),
                    SGSTAmt = taxableRows.Sum(x => x.SGSTAmt),
                    IGSTAmt = taxableRows.Sum(x => x.IGSTAmt),
                    CessAmt = taxableRows.Sum(x => x.CessAmt),
                    TotalOutputGST = taxableRows.Sum(x => x.TotalOutputGST)
                },
                new OutputGSTSummaryRow
                {
                    SalesCategory = "Exempt Sales",
                    TaxableValue = 0m, CGSTAmt = 0m, SGSTAmt = 0m, IGSTAmt = 0m, CessAmt = 0m, TotalOutputGST = 0m
                },
                new OutputGSTSummaryRow
                {
                    SalesCategory = "Nil Rated Sales",
                    TaxableValue = zeroRateRows.Sum(x => x.TaxableValue),
                    CGSTAmt = 0m, SGSTAmt = 0m, IGSTAmt = 0m, CessAmt = 0m, TotalOutputGST = 0m
                },
                new OutputGSTSummaryRow
                {
                    SalesCategory = "Non-GST Sales",
                    TaxableValue = 0m, CGSTAmt = 0m, SGSTAmt = 0m, IGSTAmt = 0m, CessAmt = 0m, TotalOutputGST = 0m
                }
            };
        }

        public List<OutputGSTRateWiseRow> GetRateWiseSummary(OutputGSTReportFilter filter)
        {
            List<SalesGSTRegisterRow> register = GetSalesRegister(filter);
            double[] standardRates = new double[] { 0, 5, 12, 18, 28 };

            var actualRates = register.Select(x => Math.Round(x.CGSTPer + x.SGSTPer + x.IGSTPer, 2)).Distinct().ToList();
            foreach (double r in standardRates)
            {
                if (!actualRates.Contains(r)) actualRates.Add(r);
            }
            actualRates.Sort();

            List<OutputGSTRateWiseRow> rateRows = new List<OutputGSTRateWiseRow>();
            foreach (double rateVal in actualRates)
            {
                var matching = register.Where(x => Math.Abs((x.CGSTPer + x.SGSTPer + x.IGSTPer) - rateVal) < 0.05).ToList();
                decimal tVal = matching.Sum(x => x.TaxableValue);
                decimal cVal = matching.Sum(x => x.CGSTAmt);
                decimal sVal = matching.Sum(x => x.SGSTAmt);
                decimal iVal = matching.Sum(x => x.IGSTAmt);
                decimal cessVal = matching.Sum(x => x.CessAmt);

                rateRows.Add(new OutputGSTRateWiseRow
                {
                    GSTRate = $"{rateVal:0.#}%",
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
            return register.Where(x => string.Equals(x.SaleType, "B2B", StringComparison.OrdinalIgnoreCase) || (!string.IsNullOrWhiteSpace(x.CustomerGSTIN) && x.CustomerGSTIN.Length >= 15))
                .Select(x => new B2BSalesRow
                {
                    CustomerGSTIN = x.CustomerGSTIN,
                    CustomerName = x.CustomerName,
                    InvoiceNo = x.InvoiceNo,
                    DocDate = x.DocDate,
                    TaxableValue = x.TaxableValue,
                    GSTRate = $"{x.CGSTPer + x.SGSTPer + x.IGSTPer:0.#}%",
                    CGSTAmt = x.CGSTAmt,
                    SGSTAmt = x.SGSTAmt,
                    IGSTAmt = x.IGSTAmt,
                    CessAmt = x.CessAmt,
                    TotalInvoiceAmount = x.TotalInvoiceAmount,
                    PlaceOfSupply = x.PlaceOfSupply,
                    ReverseCharge = "N"
                }).ToList();
        }

        public List<HSNOutputGSTRow> GetHSNOutputGST(OutputGSTReportFilter filter)
        {
            List<SalesGSTRegisterRow> register = GetSalesRegister(filter);
            return register.GroupBy(x => new { HSN = string.IsNullOrWhiteSpace(x.HSNCode) ? "N/A" : x.HSNCode, Rate = Math.Round(x.CGSTPer + x.SGSTPer + x.IGSTPer, 2) })
                .Select(g => new HSNOutputGSTRow
                {
                    HSNCode = g.Key.HSN,
                    ItemDescription = g.First().ItemName,
                    Unit = string.IsNullOrWhiteSpace(g.First().Unit) ? "PCS" : g.First().Unit,
                    TotalQty = g.Sum(x => x.Qty),
                    TotalValue = g.Sum(x => x.TotalInvoiceAmount),
                    TaxableValue = g.Sum(x => x.TaxableValue),
                    GSTRate = $"{g.Key.Rate:0.#}%",
                    CGSTAmt = g.Sum(x => x.CGSTAmt),
                    SGSTAmt = g.Sum(x => x.SGSTAmt),
                    IGSTAmt = g.Sum(x => x.IGSTAmt),
                    CessAmt = g.Sum(x => x.CessAmt),
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

                string companyStateCode = GetCompanyStateCode();

                if (TableExists("SRMaster") && TableExists("SRDetails"))
                {
                    string detailsJoin = "INNER JOIN dbo.SRDetails d ON m.SReturnNo = d.SReturnNo";

                    string gstinSelect = "''";
                    string contactTable = TableExists("ContactDetails") ? "ContactDetails" : "";
                    string ledgerJoin = "";
                    if (!string.IsNullOrEmpty(contactTable) && ColumnExists("SRMaster", "LedgerID"))
                    {
                        ledgerJoin = $"LEFT JOIN dbo.{contactTable} c ON m.LedgerID = c.LedgerID";
                        gstinSelect = "ISNULL(c.TINNumber, '')";
                    }

                    DateTime fromDate = filter.FromDate.Date;
                    DateTime exclusiveToDate = filter.ToDate.Date.AddDays(1);

                    string sql = $@"
                        SELECT 
                            'Credit Note' AS DocumentType,
                            CAST(ISNULL(m.SReturnNo, 0) AS NVARCHAR(50)) AS NoteNo,
                            m.SReturnDate AS NoteDate,
                            CAST(ISNULL(m.BillNo, 0) AS NVARCHAR(50)) AS RefInvoiceNo,
                            m.SReturnDate AS OriginalInvoiceDate,
                            ISNULL(NULLIF(m.CustomerName, ''), 'Walk-in Customer') AS PartyName,
                            {gstinSelect} AS CustomerGSTIN,
                            'Sales Return' AS Reason,
                            CAST(ISNULL(d.Qty, 0) * ISNULL(d.Rate, 0) AS DECIMAL(18,2)) AS LineTotal,
                            CAST(ISNULL(d.TaxAmt, 0) AS DECIMAL(18,2)) AS DetailTaxAmt,
                            CAST(ISNULL(d.TaxPer, 0) AS FLOAT) AS DetailTaxPer
                        FROM dbo.SRMaster m
                        {detailsJoin}
                        {ledgerJoin}
                        WHERE (m.SReturnDate >= @FromDate AND m.SReturnDate < @ExclusiveTo OR m.SReturnDate IS NULL)
                    ";

                    using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                    {
                        cmd.Parameters.AddWithValue("@FromDate", fromDate);
                        cmd.Parameters.AddWithValue("@ExclusiveTo", exclusiveToDate);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            foreach (DataRow row in dt.Rows)
                            {
                                string docType = ToString(row, "DocumentType");
                                string noteNo = ToString(row, "NoteNo");
                                DateTime noteDate = ToDateTime(row, "NoteDate");
                                string refInvNo = ToString(row, "RefInvoiceNo");
                                DateTime origInvDate = ToDateTime(row, "OriginalInvoiceDate");
                                string partyName = ToString(row, "PartyName");
                                string gstin = ToString(row, "CustomerGSTIN");
                                string reason = ToString(row, "Reason");

                                decimal lineTotal = ToDecimal(row, "LineTotal");
                                decimal taxAmt = ToDecimal(row, "DetailTaxAmt");
                                double taxPer = ToDouble(row, "DetailTaxPer");
                                if (taxPer > 100) taxPer /= 100.0;

                                decimal taxable = lineTotal > 0m ? (lineTotal - taxAmt) : 0m;
                                if (taxable <= 0m && lineTotal > 0m) taxable = lineTotal;

                                string custStateCode = gstin.Length >= 2 && char.IsDigit(gstin[0]) && char.IsDigit(gstin[1]) ? gstin.Substring(0, 2) : companyStateCode;
                                bool isIntraState = string.Equals(companyStateCode, custStateCode, StringComparison.OrdinalIgnoreCase);

                                decimal cgst = 0m, sgst = 0m, igst = 0m;
                                if (isIntraState)
                                {
                                    cgst = Math.Round(taxAmt / 2m, 2);
                                    sgst = taxAmt - cgst;
                                }
                                else
                                {
                                    igst = taxAmt;
                                }

                                rows.Add(new CreditDebitNoteGSTRow
                                {
                                    DocumentType = docType,
                                    NoteNo = noteNo,
                                    NoteDate = noteDate,
                                    RefInvoiceNo = refInvNo,
                                    OriginalInvoiceDate = origInvDate,
                                    PartyName = partyName,
                                    CustomerGSTIN = gstin,
                                    Reason = reason,
                                    TaxableAdjustment = taxable,
                                    CGSTAdjustment = cgst,
                                    SGSTAdjustment = sgst,
                                    IGSTAdjustment = igst,
                                    CessAmt = 0m,
                                    NetTotalAdjustment = taxable + taxAmt
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetCreditDebitNotes error: " + ex.Message);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return rows;
        }

        private string GetCompanyStateCode()
        {
            if (_cachedCompanyStateCode != null) return _cachedCompanyStateCode;

            try
            {
                string companyTable = TableExists("Company") ? "Company" : (TableExists("CompanyDetails") ? "CompanyDetails" : (TableExists("_CompanyInfo") ? "_CompanyInfo" : ""));
                if (!string.IsNullOrEmpty(companyTable))
                {
                    string gstinCol = GetCol(companyTable, "", "GSTIN", "GSTNo", "GST_NO");
                    string stateCol = GetCol(companyTable, "", "State", "StateId", "StateCode");

                    string sql = $"SELECT TOP 1 {(gstinCol != null ? gstinCol : "''")} AS GSTIN, {(stateCol != null ? stateCol : "''")} AS State FROM dbo.{companyTable}";
                    using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string gstin = reader["GSTIN"] != DBNull.Value ? reader["GSTIN"].ToString().Trim() : "";
                                if (gstin.Length >= 2 && char.IsDigit(gstin[0]) && char.IsDigit(gstin[1]))
                                {
                                    _cachedCompanyStateCode = gstin.Substring(0, 2);
                                    return _cachedCompanyStateCode;
                                }

                                string state = reader["State"] != DBNull.Value ? reader["State"].ToString().Trim() : "";
                                if (!string.IsNullOrEmpty(state))
                                {
                                    _cachedCompanyStateCode = GetStateCodeFromNameOrId(state);
                                    return _cachedCompanyStateCode;
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            _cachedCompanyStateCode = "32";
            return _cachedCompanyStateCode;
        }

        private static string GetStateCodeFromNameOrId(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "32";
            if (input.Length >= 2 && char.IsDigit(input[0]) && char.IsDigit(input[1])) return input.Substring(0, 2);
            if (int.TryParse(input, out int id)) return id < 10 ? "0" + id : id.ToString();
            return "32";
        }

        private static string GetStateNameFromCode(string code)
        {
            switch (code)
            {
                case "32": return "Kerala (32)";
                case "33": return "Tamil Nadu (33)";
                case "29": return "Karnataka (29)";
                case "27": return "Maharashtra (27)";
                case "07": return "Delhi (07)";
                case "09": return "Uttar Pradesh (09)";
                case "19": return "West Bengal (19)";
                case "36": return "Telangana (36)";
                case "37": return "Andhra Pradesh (37)";
                default: return string.IsNullOrEmpty(code) ? "Local State" : $"State ({code})";
            }
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
            if (string.IsNullOrWhiteSpace(tableName)) return false;
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                string fullTable = tableName.StartsWith("dbo.") ? tableName : "dbo." + tableName;
                using (SqlCommand cmd = new SqlCommand("SELECT CASE WHEN OBJECT_ID(@TableName, 'U') IS NOT NULL THEN 1 ELSE 0 END", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@TableName", fullTable);
                    object res = cmd.ExecuteScalar();
                    return res != null && res != DBNull.Value && Convert.ToInt32(res) == 1;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool ColumnExists(string tableName, string columnName)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(columnName)) return false;
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                string fullTable = tableName.StartsWith("dbo.") ? tableName : "dbo." + tableName;
                using (SqlCommand cmd = new SqlCommand("SELECT CASE WHEN COL_LENGTH(@TableName, @ColumnName) IS NOT NULL THEN 1 ELSE 0 END", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@TableName", fullTable);
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

        private static double ToDouble(DataRow row, string col) => row.Table.Columns.Contains(col) && row[col] != DBNull.Value ? Convert.ToDouble(row[col]) : 0.0;
        private static decimal ToDecimal(DataRow row, string col) => row.Table.Columns.Contains(col) && row[col] != DBNull.Value ? Convert.ToDecimal(row[col]) : 0m;
        private static DateTime ToDateTime(DataRow row, string col) => row.Table.Columns.Contains(col) && row[col] != DBNull.Value ? Convert.ToDateTime(row[col]) : DateTime.MinValue;
        private static string ToString(DataRow row, string col) => row.Table.Columns.Contains(col) && row[col] != DBNull.Value ? Convert.ToString(row[col]) : string.Empty;
    }
}
