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

            int companyId = filter.CompanyId > 0 ? filter.CompanyId : GetContextValue(SessionContext.CompanyId, DataBase.CompanyId);
            int branchId = filter.BranchId > 0 ? filter.BranchId : GetContextValue(SessionContext.BranchId, DataBase.BranchId);
            int finYearId = filter.FinYearId > 0 ? filter.FinYearId : GetContextValue(SessionContext.FinYearId, DataBase.FinyearId);

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                // 1. Dedicated Stored Procedure Attempt (POS_OutputGSTReport)
                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_OutputGSTReport, (SqlConnection)DataConnection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ReportType", "REGISTER");
                        cmd.Parameters.AddWithValue("@CompanyId", companyId > 0 ? (object)companyId : DBNull.Value);
                        cmd.Parameters.AddWithValue("@BranchId", branchId > 0 ? (object)branchId : DBNull.Value);
                        cmd.Parameters.AddWithValue("@FinYearId", finYearId > 0 ? (object)finYearId : DBNull.Value);
                        cmd.Parameters.AddWithValue("@FromDate", filter.FromDate.Date);
                        cmd.Parameters.AddWithValue("@ToDate", filter.ToDate.Date);
                        cmd.Parameters.AddWithValue("@CustomerLedgerId", filter.CustomerLedgerId > 0 ? (object)filter.CustomerLedgerId : DBNull.Value);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    list.Add(MapSalesRow(row));
                                }
                                return FilterSalesRows(list, filter.SearchText);
                            }
                        }
                    }
                }
                catch (SqlException)
                {
                    list.Clear();
                }

                string companyStateCode = GetCompanyStateCode();

                string masterTable = TableExists("SMaster") ? "SMaster" : (TableExists("SalesMaster") ? "SalesMaster" : "");
                string detailsTable = TableExists("SDetails") ? "SDetails" : (TableExists("SalesDetails") ? "SalesDetails" : "");

                if (string.IsNullOrEmpty(masterTable))
                {
                    return GetSalesMasterFallback(filter, branchId, companyStateCode);
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
                    string companyDetails = GetCol(detailsTable, "d", "CompanyId", "CompanyID", "Company_Id");
                    string branchDetails = GetCol(detailsTable, "d", "BranchId", "BranchID", "Branch_Id");
                    string finYearDetails = GetCol(detailsTable, "d", "FinYearId", "FinYearID", "FinYear_Id");
                    string cancelDetails = GetCol(detailsTable, "d", "CancelFlag", "cancelflag");

                    detailsJoin = $"LEFT JOIN dbo.{detailsTable} d ON {billNoMaster} = {billNoDetails}";
                    if (companyCol != null && companyDetails != null)
                        detailsJoin += $" AND {companyCol} = {companyDetails}";
                    if (branchCol != null && branchDetails != null)
                        detailsJoin += $" AND {branchCol} = {branchDetails}";
                    if (finYearCol != null && finYearDetails != null)
                        detailsJoin += $" AND {finYearCol} = {finYearDetails}";
                    if (cancelDetails != null)
                        detailsJoin += $" AND ISNULL({cancelDetails}, 0) = 0";

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
                        string gstinCol = GetCol(contactTable, "c", "GSTIN", "GSTNo", "GST_NO", "TINNumber");
                        if (gstinCol != null) gstinSelect = $"ISNULL({gstinCol}, '')";

                        string custStateCol = GetCol(contactTable, "c", "StateID", "State", "StateName");
                        if (custStateCol != null) custStateSelect = $"ISNULL(CAST({custStateCol} AS NVARCHAR(100)), '')";
                    }
                }

                foreach (var source in new[] { "LedgerDetails", "LedgerMaster", "Ledger" })
                {
                    if (ledgerIdMaster == null || !TableExists(source))
                        continue;

                    string alias = source == "LedgerDetails" ? "ld" : source == "LedgerMaster" ? "lm" : "l";
                    string ledgerKey = GetCol(source, alias, "LedgerID", "LedgerId");
                    string ledgerGSTIN = GetCol(source, alias, "GSTIN", "GSTNo", "GSTINNo", "GST_NO", "GSTNumber", "TINNo", "GSTIN_NO", "GST");
                    if (ledgerKey == null || ledgerGSTIN == null)
                        continue;

                    ledgerJoin += $" LEFT JOIN dbo.{source} {alias} ON {ledgerIdMaster} = {ledgerKey}";
                    gstinSelect = source == "LedgerDetails"
                        ? $"ISNULL(NULLIF({ledgerGSTIN}, ''), {gstinSelect})"
                        : $"ISNULL(NULLIF({gstinSelect}, ''), ISNULL({ledgerGSTIN}, ''))";
                }

                string masterNetCol = GetCol(masterTable, "m", "NetAmount", "NetAmt", "GrandTotal", "TotalAmount") ?? "0";
                string masterTaxCol = GetCol(masterTable, "m", "TaxAmt", "TaxAmount", "GstAmt") ?? "0";
                string masterSubCol = GetCol(masterTable, "m", "SubTotal", "BaseAmount", "TaxableValue") ?? "0";
                string masterTaxPerCol = GetCol(masterTable, "m", "TaxPer", "TaxPercent", "GstPer") ?? "0";

                DateTime fromDate = filter.FromDate.Date;
                DateTime exclusiveToDate = filter.ToDate.Date.AddDays(1);

                return ExecuteSalesQuery(
                    masterTable, detailsJoin, itemJoin, ledgerJoin,
                    billNoMaster, dateColMaster, custNameCol, gstinSelect, custStateSelect, stateIdMaster,
                    itemNameCol, hsnSelect, qtyCol, unitCol, unitPriceCol, baseAmtCol, taxAmtCol, taxPerCol,
                    cessPerCol, cessAmtCol, totalAmtCol, masterSubCol, masterTaxCol, masterTaxPerCol, masterNetCol, taxTypeCol,
                    companyCol, branchCol, finYearCol, cancelCol, statusCol, ledgerIdMaster,
                    companyId, branchId, finYearId, filter.CustomerLedgerId, fromDate, exclusiveToDate, companyStateCode, filter.SearchText);
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
        }

        private List<SalesGSTRegisterRow> ExecuteSalesQuery(
            string masterTable, string detailsJoin, string itemJoin, string ledgerJoin,
            string billNoMaster, string dateColMaster, string custNameCol, string gstinSelect, string custStateSelect, string stateIdMaster,
            string itemNameCol, string hsnSelect, string qtyCol, string unitCol, string unitPriceCol, string baseAmtCol, string taxAmtCol, string taxPerCol,
            string cessPerCol, string cessAmtCol, string totalAmtCol, string masterSubCol, string masterTaxCol, string masterTaxPerCol, string masterNetCol, string taxTypeCol,
            string companyCol, string branchCol, string finYearCol, string cancelCol, string statusCol, string ledgerIdMaster,
            int companyId, int branchId, int finYearId, int customerLedgerId,
            DateTime fromDate, DateTime exclusiveToDate, string companyStateCode, string searchText)
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

            whereClauses.Add($"{dateColMaster} >= @FromDate AND {dateColMaster} < @ExclusiveTo");
            if (customerLedgerId > 0)
            {
                if (ledgerIdMaster == null)
                    return list;
                whereClauses.Add($"{ledgerIdMaster} = @CustomerLedgerId");
            }

            string whereSql = "WHERE " + string.Join(" AND ", whereClauses);

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
                cmd.Parameters.AddWithValue("@CompanyId", companyId);
                cmd.Parameters.AddWithValue("@BranchId", branchId);
                cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ExclusiveTo", exclusiveToDate);
                if (ledgerIdMaster != null && customerLedgerId > 0)
                {
                    cmd.Parameters.AddWithValue("@CustomerLedgerId", customerLedgerId);
                }

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    foreach (DataRow row in dt.Rows)
                    {
                        var parsedRow = ProcessSalesRow(row, companyStateCode);
                        if (parsedRow != null)
                            list.Add(parsedRow);
                    }
                }
            }

            return FilterSalesRows(list, searchText);
        }

        private List<SalesGSTRegisterRow> GetSalesMasterFallback(OutputGSTReportFilter filter, int branchId, string companyStateCode)
        {
            var rows = new List<SalesGSTRegisterRow>();
            if (filter.CustomerLedgerId > 0)
                return rows;

            try
            {
                using (var cmd = new SqlCommand(STOREDPROCEDURE._POS_Sales_Master_for_Report, (SqlConnection)DataConnection))
                using (var adapter = new SqlDataAdapter(cmd))
                using (var data = new DataTable())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", filter.FromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", filter.ToDate.Date.AddDays(1).AddMilliseconds(-3));
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    adapter.Fill(data);

                    foreach (DataRow row in data.Rows)
                    {
                        decimal net = ToDecimal(row, "NetAmount");
                        decimal tax = ToDecimal(row, "TaxAmt");
                        decimal taxable = ToDecimal(row, "SubTotal");
                        if (taxable == 0m && net > 0m)
                            taxable = net - tax;
                        string gstin = ToString(row, "GSTIN").Trim();
                        string customerName = ToString(row, "customername");
                        if (string.IsNullOrWhiteSpace(customerName))
                            customerName = ToString(row, "CustomerName");
                        string stateCode = gstin.Length >= 2 && char.IsDigit(gstin[0]) && char.IsDigit(gstin[1])
                            ? gstin.Substring(0, 2)
                            : companyStateCode;
                        bool interstate = !string.Equals(stateCode, companyStateCode, StringComparison.Ordinal);
                        decimal cgst = interstate ? 0m : Math.Round(tax / 2m, 2);
                        double rate = taxable > 0m ? (double)Math.Round(tax / taxable * 100m, 2) : 0d;

                        rows.Add(new SalesGSTRegisterRow
                        {
                            InvoiceNo = ToString(row, "BillNo"),
                            DocDate = ToDateTime(row, "BillDate"),
                            CustomerName = string.IsNullOrWhiteSpace(customerName) ? "Walk-in Customer" : customerName,
                            CustomerGSTIN = gstin,
                            SaleType = gstin.Length >= 15 ? "B2B" : "B2C",
                            ItemName = "Sales Transaction",
                            Qty = 1,
                            Unit = "PCS",
                            TaxableValue = taxable,
                            CGSTPer = interstate ? 0d : rate / 2d,
                            SGSTPer = interstate ? 0d : rate / 2d,
                            IGSTPer = interstate ? rate : 0d,
                            CGSTAmt = cgst,
                            SGSTAmt = interstate ? 0m : tax - cgst,
                            IGSTAmt = interstate ? tax : 0m,
                            TotalOutputGST = tax,
                            TotalInvoiceAmount = net,
                            PlaceOfSupply = GetStateNameFromCode(stateCode),
                            SupplyType = interstate ? "INTER-STATE" : "INTRA-STATE",
                            TaxType = "incl"
                        });
                    }
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("Sales master fallback error: " + ex.Message);
            }
            return FilterSalesRows(rows, filter.SearchText);
        }

        private static List<SalesGSTRegisterRow> FilterSalesRows(List<SalesGSTRegisterRow> rows, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return rows;

            string search = searchText.Trim();
            return rows.Where(row => new[] { row.InvoiceNo, row.CustomerName, row.CustomerGSTIN, row.ItemName, row.HSNCode }
                .Any(value => value != null && value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)).ToList();
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
                PlaceOfSupply = ToString(row, "PlaceOfSupply"),
                SupplyType = ToDecimal(row, "IGSTAmt") != 0m || ToDouble(row, "IGSTPer") != 0d ? "INTER-STATE" : "INTRA-STATE",
                TaxType = ToString(row, "TaxType")
            };

            if (r.TaxableValue == 0m && r.TotalInvoiceAmount > 0m)
            {
                double totalTaxPer = r.CGSTPer + r.SGSTPer + r.IGSTPer;
                if (totalTaxPer > 0)
                {
                    r.TaxableValue = Math.Round(r.TotalInvoiceAmount / (decimal)(1.0 + (totalTaxPer / 100.0)), 2);
                    r.TotalOutputGST = r.TotalInvoiceAmount - r.TaxableValue;
                    decimal gst = r.TotalOutputGST - r.CessAmt;
                    if (r.SupplyType == "INTER-STATE")
                    {
                        r.IGSTAmt = gst;
                    }
                    else
                    {
                        r.CGSTAmt = Math.Round(gst / 2m, 2);
                        r.SGSTAmt = gst - r.CGSTAmt;
                    }
                }
                else
                {
                    r.TaxableValue = r.TotalInvoiceAmount;
                }
            }

            return r;
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
                string saleState = ToString(row, "MasterStateId").Trim();
                customerStateCode = string.IsNullOrEmpty(saleState) ? companyStateCode : GetStateCodeFromNameOrId(saleState);
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
            int companyId = filter.CompanyId > 0 ? filter.CompanyId : GetContextValue(SessionContext.CompanyId, DataBase.CompanyId);
            int branchId = filter.BranchId > 0 ? filter.BranchId : GetContextValue(SessionContext.BranchId, DataBase.BranchId);
            int finYearId = filter.FinYearId > 0 ? filter.FinYearId : GetContextValue(SessionContext.FinYearId, DataBase.FinyearId);

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                string companyStateCode = GetCompanyStateCode();

                if (TableExists("SRMaster") && TableExists("SRDetails"))
                {
                    const string detailsJoin = "INNER JOIN dbo.SRDetails d ON m.SReturnNo = d.SReturnNo AND m.FinYearId = d.FinYearId AND m.BranchId = d.BranchId";

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
                        WHERE m.CompanyId = @CompanyId
                          AND m.BranchId = @BranchId
                          AND m.FinYearId = @FinYearId
                          AND m.SReturnDate >= @FromDate
                          AND m.SReturnDate < @ExclusiveTo
                    ";

                    using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                    {
                        cmd.Parameters.AddWithValue("@CompanyId", companyId);
                        cmd.Parameters.AddWithValue("@BranchId", branchId);
                        cmd.Parameters.AddWithValue("@FinYearId", finYearId);
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
