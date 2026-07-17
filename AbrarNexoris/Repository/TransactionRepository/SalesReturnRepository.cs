using ModelClass;
using ModelClass.TransactionModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using Repository.MasterRepositry;

namespace Repository.TransactionRepository
{
    public class SalesReturnRepository : BaseRepostitory
    {
        LedgerRepository ledgerRepository = new LedgerRepository();


        public SalesReturn GetById(Int64 Number)
        {
            SalesReturn item = new SalesReturn();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BillNo", Number);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            item = ds.Tables[0].Rows[0].ToNullableObject<SalesReturn>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return item;
        }

        // Get Sales Return by SReturnNo
        public SalesReturn GetBySalesReturnId(int sReturnNo)
        {
            SalesReturn item = new SalesReturn();

            // Ensure connection is clean before opening
            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_SalesReturn, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SReturnNo", sReturnNo);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            item = ds.Tables[0].Rows[0].ToNullableObject<SalesReturn>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting sales return: {ex.Message}");
                throw; // Preserve stack trace
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
            return item;
        }

        public DataTable GetSalesReturnDetails(int sReturnNo)
        {
            DataTable dtDetails = new DataTable();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_SReturnDetails, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Only pass the parameters that the stored procedure actually uses for GETALLSRETURNDETAILS
                    cmd.Parameters.AddWithValue("@SReturnNo", sReturnNo);
                    cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", DataBase.FinyearId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALLSRETURNDETAILS");

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dtDetails);

                        // Debug: Log the number of rows returned
                        System.Diagnostics.Debug.WriteLine($"Retrieved {dtDetails.Rows.Count} rows for SReturnNo {sReturnNo}");

                        // Check for duplicates and log details
                        if (dtDetails.Rows.Count > 0)
                        {
                            var duplicateCheck = dtDetails.AsEnumerable()
                                .GroupBy(r => new
                                {
                                    SlNo = r.Field<object>("SlNo"),
                                    ItemName = r.Field<string>("ItemName"),
                                    Unit = r.Field<string>("Unit")
                                })
                                .Where(g => g.Count() > 1)
                                .ToList();

                            if (duplicateCheck.Any())
                            {
                                System.Diagnostics.Debug.WriteLine($"Found {duplicateCheck.Count} duplicate groups in sales return details");
                                foreach (var dup in duplicateCheck)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Duplicate: SlNo={dup.Key.SlNo}, Item={dup.Key.ItemName}, Count={dup.Count()}");
                                }
                            }
                        }
                    }
                }

                // ALWAYS ensure ReturnQty and ReturnedQty columns exist in the result, even if no rows
                if (dtDetails != null)
                {
                    // Remove duplicates if any exist (due to JOIN in stored procedure)
                    if (dtDetails.Rows.Count > 0)
                    {
                        // Create a new DataTable with unique rows based on SlNo and ItemName
                        DataTable uniqueDetails = dtDetails.Clone();
                        var uniqueRows = dtDetails.AsEnumerable()
                            .GroupBy(r => new
                            {
                                SlNo = r.Field<object>("SlNo"),
                                ItemName = r.Field<string>("ItemName")
                            })
                            .Select(g => g.First()) // Take the first occurrence of each group
                            .ToList();

                        foreach (var row in uniqueRows)
                        {
                            uniqueDetails.ImportRow(row);
                        }

                        if (uniqueDetails.Rows.Count != dtDetails.Rows.Count)
                        {
                            System.Diagnostics.Debug.WriteLine($"Removed {dtDetails.Rows.Count - uniqueDetails.Rows.Count} duplicate rows");
                            dtDetails = uniqueDetails;
                        }
                    }

                    if (!dtDetails.Columns.Contains("ReturnQty"))
                    {
                        dtDetails.Columns.Add("ReturnQty", typeof(decimal));
                        System.Diagnostics.Debug.WriteLine("Added ReturnQty column to DataTable");
                    }

                    if (!dtDetails.Columns.Contains("ReturnedQty"))
                    {
                        dtDetails.Columns.Add("ReturnedQty", typeof(decimal));
                        System.Diagnostics.Debug.WriteLine("Added ReturnedQty column to DataTable");
                    }

                    // Normalize nulls and fix Amount calculation for saved returns
                    foreach (DataRow row in dtDetails.Rows)
                    {
                        if (row["ReturnQty"] == DBNull.Value || row["ReturnQty"] == null)
                        {
                            row["ReturnQty"] = 0.0;
                        }
                        if (row["ReturnedQty"] == DBNull.Value || row["ReturnedQty"] == null)
                        {
                            row["ReturnedQty"] = 0.0;
                        }

                        // Fix Amount calculation: for saved returns, show value of ReturnedQty, not ReturnQty
                        if (row["Amount"] != DBNull.Value && row["UnitPrice"] != DBNull.Value)
                        {
                            decimal unitPrice = Convert.ToDecimal(row["UnitPrice"]);
                            decimal returnedQty = Convert.ToDecimal(row["ReturnedQty"]);
                            // For saved returns, Amount should reflect the value of what was actually returned
                            row["Amount"] = unitPrice * returnedQty;
                        }
                    }
                }

                // Check if we have any data - if not, try the direct SQL query approach
                if (dtDetails == null || dtDetails.Rows.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No data returned from stored procedure. Trying direct SQL query approach.");
                    dtDetails = GetSalesReturnDetailsByReturnNo(sReturnNo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error getting sales return details: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Try the direct SQL approach as fallback
                try
                {
                    dtDetails = GetSalesReturnDetailsByReturnNo(sReturnNo);
                }
                catch (Exception innerEx)
                {
                    System.Diagnostics.Debug.WriteLine("Fallback also failed: " + innerEx.Message);
                }
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return dtDetails;
        }

        // Method that uses the updated stored procedure (now includes Packing and Cost columns)
        private DataTable GetSalesReturnDetailsByReturnNo(int sReturnNo)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_SReturnDetails, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Only pass the parameters that the stored procedure actually uses for GETALLSRETURNDETAILS
                    cmd.Parameters.AddWithValue("@SReturnNo", sReturnNo);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALLSRETURNDETAILS");

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable resultTable = new DataTable();
                        adapter.Fill(resultTable);
                        return resultTable;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving sales return details: {ex.Message}");
            }
        }

        public SalesReturnDetailsGrid GetByIdSRDWithAvailableQty(Int64 Number)
        {
            SalesReturnDetailsGrid item = new SalesReturnDetailsGrid();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BillNo", Number);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[1] != null) && (ds.Tables[1].Rows.Count > 0))
                        {
                            // Add ReturnQty and ReturnedQty columns if they don't exist (for backward compatibility)
                            if (!ds.Tables[1].Columns.Contains("ReturnQty"))
                            {
                                ds.Tables[1].Columns.Add("ReturnQty", typeof(double));
                            }

                            if (!ds.Tables[1].Columns.Contains("ReturnedQty"))
                            {
                                ds.Tables[1].Columns.Add("ReturnedQty", typeof(double));
                            }

                            // Add AvailableQty column for easier tracking
                            if (!ds.Tables[1].Columns.Contains("AvailableQty"))
                            {
                                ds.Tables[1].Columns.Add("AvailableQty", typeof(double));
                            }

                            // Calculate previously returned quantities for each item
                            foreach (DataRow row in ds.Tables[1].Rows)
                            {
                                int itemId = Convert.ToInt32(row["ItemId"]);
                                double originalQty = Convert.ToDouble(row["Qty"]);
                                double previouslyReturnedQty = GetPreviouslyReturnedQty(Number, itemId);
                                double availableQty = Math.Max(0, originalQty - previouslyReturnedQty);

                                // Set the calculated values
                                row["ReturnQty"] = 0.0; // Default for new return
                                row["ReturnedQty"] = previouslyReturnedQty;
                                row["AvailableQty"] = availableQty;

                                System.Diagnostics.Debug.WriteLine($"Item {itemId}: Original={originalQty}, Previously Returned={previouslyReturnedQty}, Available={availableQty}");
                            }

                            item.List = ds.Tables[1].ToListOfObject<SalesReturnDetails>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetByIdSRDWithAvailableQty: {ex.Message}");
                throw; // Preserve stack trace
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return item;
        }

        // Helper method to calculate previously returned quantity for an item from a specific bill
        private double GetPreviouslyReturnedQty(Int64 billNo, int itemId)
        {
            double totalReturned = 0.0;

            try
            {
                // Use stored procedure to get total returned quantity for this item from this specific bill
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_SReturnDetails, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@InvoiceNo", billNo.ToString());
                    cmd.Parameters.AddWithValue("@ItemID", itemId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETRETURNEDQTYBYITEM");

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        totalReturned = Convert.ToDouble(result);
                        System.Diagnostics.Debug.WriteLine($"Previously returned qty for Bill {billNo}, Item {itemId}: {totalReturned}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating previously returned qty: {ex.Message}");
                totalReturned = 0.0;
            }

            return totalReturned;
        }

        // Wrapper helper to get total previously returned qty using invoice number string
        private decimal GetTotalReturnedQty(string invoiceNo, int itemId, SqlTransaction trans = null, SqlConnection connection = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(invoiceNo)) return 0m;
                if (!Int64.TryParse(invoiceNo.Trim(), out var billNo)) return 0m;
                var prev = GetPreviouslyReturnedQty(billNo, itemId);
                return Convert.ToDecimal(prev);
            }
            catch
            {
                return 0m;
            }
        }

        public int SReturnNo = 0;
        public int GenerateSReturnNo(SqlTransaction trans = null)
        {
            int SReturnNo = 0;
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_SalesReturn, (SqlConnection)DataConnection, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);

                    // Use FinYearId from session context
                    int finYearId = SessionContext.FinYearId;
                    System.Diagnostics.Debug.WriteLine($"Using FinYearId={finYearId} from SessionContext");

                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                    cmd.Parameters.AddWithValue("@_Operation", "GENERATENUMBER");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            SReturnNo = Convert.ToInt32(ds.Tables[0].Rows[0].ItemArray[0].ToString());
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error in GenerateSReturnNo: {Ex.Message}");
            }
            return SReturnNo;
        }

        public string saveSR(SalesReturn sr, SalesReturnDetails details, DataGridView dgvInvoice)
        {
            Voucher objVoucher = new Voucher();
            SalesReturn SR = new SalesReturn();
            DataConnection.Open();
            // Set transaction isolation level to Serializable to prevent race conditions
            using (SqlTransaction trans = ((SqlConnection)DataConnection).BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    // Emergency fix: validate and set default Unit values if needed
                    for (int i = 0; i < dgvInvoice.Rows.Count; i++)
                    {
                        if (dgvInvoice.Rows[i].Cells["Unit"] != null)
                        {
                            if (dgvInvoice.Rows[i].Cells["Unit"].Value == null ||
                                dgvInvoice.Rows[i].Cells["Unit"].Value == DBNull.Value ||
                                string.IsNullOrWhiteSpace(dgvInvoice.Rows[i].Cells["Unit"].Value?.ToString()))
                            {
                                dgvInvoice.Rows[i].Cells["Unit"].Value = "PCS";
                                System.Diagnostics.Debug.WriteLine($"Emergency Unit fix: Set NULL/empty Unit to 'PCS' for row {i}");
                            }
                        }
                    }

                    // Ensure required fields are set
                    sr.CompanyId = SessionContext.CompanyId;
                    sr.BranchId = SessionContext.BranchId;

                    // Use FinYearId from session context
                    sr.FinYearId = SessionContext.FinYearId;
                    System.Diagnostics.Debug.WriteLine($"Using FinYearId={sr.FinYearId} from SessionContext");

                    sr.SReturnNo = this.GenerateSReturnNo(trans);

                    // Generate the Voucher ID using stored procedure
                    objVoucher._Operation = "GENERATENUMBER";
                    objVoucher.CompanyID = SessionContext.CompanyId;
                    objVoucher.BranchID = SessionContext.BranchId;
                    objVoucher.FinYearID = sr.FinYearId;
                    objVoucher.VoucherType = "Credit Note";

                    int voucherId = 0;
                    using (SqlCommand voucherCmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, (SqlConnection)DataConnection, (SqlTransaction)trans))
                    {
                        voucherCmd.CommandType = CommandType.StoredProcedure;
                        voucherCmd.Parameters.AddWithValue("@CompanyID", objVoucher.CompanyID);
                        voucherCmd.Parameters.AddWithValue("@BranchID", objVoucher.BranchID);
                        voucherCmd.Parameters.AddWithValue("@FinYearID", objVoucher.FinYearID);
                        voucherCmd.Parameters.AddWithValue("@VoucherType", objVoucher.VoucherType);
                        voucherCmd.Parameters.AddWithValue("@_Operation", objVoucher._Operation);

                        object result = voucherCmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value && int.TryParse(result.ToString(), out voucherId))
                        {
                            sr.VoucherID = voucherId;
                            System.Diagnostics.Debug.WriteLine($"Generated Voucher ID (SP): {voucherId} for Sales Return");
                        }
                        else
                        {
                            throw new Exception("Failed to generate VoucherID for Sales Return via stored procedure");
                        }
                    }

                    // Now create the SalesReturn with the voucher ID
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_SalesReturn, (SqlConnection)DataConnection, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CompanyId", sr.CompanyId);
                        cmd.Parameters.AddWithValue("@BranchId", sr.BranchId);
                        cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                        cmd.Parameters.AddWithValue("@SReturnNo", sr.SReturnNo);
                        cmd.Parameters.AddWithValue("@SReturnDate", sr.SReturnDate);
                        cmd.Parameters.AddWithValue("@InvoiceNo", sr.InvoiceNo ?? "");
                        cmd.Parameters.AddWithValue("@InvoiceDate", sr.InvoiceDate);
                        cmd.Parameters.AddWithValue("@LedgerID", sr.LedgerID);
                        cmd.Parameters.AddWithValue("@CustomerName", sr.CustomerName ?? "");
                        cmd.Parameters.AddWithValue("@PaymodeID", sr.PaymodeID);
                        cmd.Parameters.AddWithValue("@Paymode", sr.Paymode ?? "");
                        cmd.Parameters.AddWithValue("@PaymodeLedgerID", sr.PaymodeLedgerID);
                        cmd.Parameters.AddWithValue("@SubTotal", sr.SubTotal);
                        cmd.Parameters.AddWithValue("@SpDisPer", sr.SpDisPer);
                        cmd.Parameters.AddWithValue("@SpDsiAmt", sr.SpDsiAmt);
                        cmd.Parameters.AddWithValue("@BillDiscountPer", sr.BillDiscountPer);
                        cmd.Parameters.AddWithValue("@BillDiscountAmt", sr.BillDiscountAmt);
                        cmd.Parameters.AddWithValue("@TaxPer", sr.TaxPer);
                        cmd.Parameters.AddWithValue("@TaxAmt", sr.TaxAmt);
                        cmd.Parameters.AddWithValue("@Frieght", sr.Frieght);
                        cmd.Parameters.AddWithValue("@GrandTotal", sr.GrandTotal);
                        cmd.Parameters.AddWithValue("@CancelFlag", sr.CancelFlag);
                        cmd.Parameters.AddWithValue("@UserID", sr.UserID);
                        cmd.Parameters.AddWithValue("@UserName", sr.UserName ?? "");
                        cmd.Parameters.AddWithValue("@TaxType", sr.TaxType ?? "");
                        cmd.Parameters.AddWithValue("@RoundOff", sr.RoundOff);
                        cmd.Parameters.AddWithValue("@CessPer", sr.CessPer);
                        cmd.Parameters.AddWithValue("@CessAmt", sr.CessAmt);
                        cmd.Parameters.AddWithValue("@VoucherID", sr.VoucherID);
                        cmd.Parameters.AddWithValue("@BranchName", sr.BranchName ?? "");
                        cmd.Parameters.AddWithValue("@CalAfterTax", sr.CalAfterTax);
                        cmd.Parameters.AddWithValue("@CurSymbol", sr.CurSymbol ?? "");
                        cmd.Parameters.AddWithValue("@SeriesId", sr.SeriesId);
                        cmd.Parameters.AddWithValue("@Status", sr.Status ?? "");
                        cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                        // Add any additional required parameters
                        cmd.Parameters.AddWithValue("@ExpenseAmt", 0);
                        cmd.Parameters.AddWithValue("@OtherExpAmt", 0);
                        cmd.Parameters.AddWithValue("@KFCessPer", 0);
                        cmd.Parameters.AddWithValue("@KFCessAmt", 0);
                        cmd.Parameters.AddWithValue("@Remarks", DBNull.Value);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                    }

                    // Step 3: Iterate over DataGridView and insert SalesReturnDetails for each row
                    for (int i = 0; i < dgvInvoice.Rows.Count; i++)
                    {
                        if (dgvInvoice.Rows[i].Cells["BarCode"].Value == null) continue;

                        // Get packing value from grid data, default to 1 if not available
                        decimal packing = 1;
                        if (dgvInvoice.Rows[i].Cells["Packing"] != null && dgvInvoice.Rows[i].Cells["Packing"].Value != null)
                        {
                            if (decimal.TryParse(dgvInvoice.Rows[i].Cells["Packing"].Value.ToString(), out decimal gridPacking))
                            {
                                packing = gridPacking;
                            }
                        }

                        // Get the UnitId for this item's unit using stored procedure
                        int unitId = 0;
                        try
                        {
                            using (SqlCommand unitIdCmd = new SqlCommand(STOREDPROCEDURE.POS_UnitMaster, (SqlConnection)DataConnection, trans))
                            {
                                unitIdCmd.CommandType = CommandType.StoredProcedure;
                                unitIdCmd.Parameters.AddWithValue("@UnitName", dgvInvoice.Rows[i].Cells["Unit"].Value?.ToString() ?? "");
                                unitIdCmd.Parameters.AddWithValue("@_Operation", "GetByName");

                                // Add required parameters with default values
                                unitIdCmd.Parameters.AddWithValue("@UnitID", DBNull.Value);
                                unitIdCmd.Parameters.AddWithValue("@UnitSymbol", DBNull.Value);
                                unitIdCmd.Parameters.AddWithValue("@UnitQuantityCode", DBNull.Value);
                                unitIdCmd.Parameters.AddWithValue("@Packing", DBNull.Value);
                                unitIdCmd.Parameters.AddWithValue("@NoOfDecimalPlaces", DBNull.Value);
                                unitIdCmd.Parameters.AddWithValue("@UnitNameInBill", DBNull.Value);
                                unitIdCmd.Parameters.AddWithValue("@IsDelete", DBNull.Value);

                                using (SqlDataAdapter adapter = new SqlDataAdapter(unitIdCmd))
                                {
                                    DataSet ds = new DataSet();
                                    adapter.Fill(ds);

                                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                                    {
                                        unitId = Convert.ToInt32(ds.Tables[0].Rows[0]["UnitID"]);
                                        System.Diagnostics.Debug.WriteLine($"Found UnitId {unitId} for Unit {dgvInvoice.Rows[i].Cells["Unit"].Value} via stored procedure");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error retrieving UnitId via stored procedure: {ex.Message}. Using fallback.");
                        }

                        // If UnitId is still 0, try to get it from UnitMaster table using stored procedure
                        if (unitId == 0)
                        {
                            try
                            {
                                using (SqlCommand unitMasterCmd = new SqlCommand(STOREDPROCEDURE.POS_UnitMaster, (SqlConnection)DataConnection, trans))
                                {
                                    unitMasterCmd.CommandType = CommandType.StoredProcedure;
                                    unitMasterCmd.Parameters.AddWithValue("@UnitName", dgvInvoice.Rows[i].Cells["Unit"].Value?.ToString() ?? "");
                                    unitMasterCmd.Parameters.AddWithValue("@_Operation", "GetByName");

                                    // Add required parameters with default values
                                    unitMasterCmd.Parameters.AddWithValue("@UnitID", DBNull.Value);
                                    unitMasterCmd.Parameters.AddWithValue("@UnitSymbol", DBNull.Value);
                                    unitMasterCmd.Parameters.AddWithValue("@UnitQuantityCode", DBNull.Value);
                                    unitMasterCmd.Parameters.AddWithValue("@Packing", DBNull.Value);
                                    unitMasterCmd.Parameters.AddWithValue("@NoOfDecimalPlaces", DBNull.Value);
                                    unitMasterCmd.Parameters.AddWithValue("@UnitNameInBill", DBNull.Value);
                                    unitMasterCmd.Parameters.AddWithValue("@IsDelete", DBNull.Value);

                                    using (SqlDataAdapter adapter = new SqlDataAdapter(unitMasterCmd))
                                    {
                                        DataSet ds = new DataSet();
                                        adapter.Fill(ds);

                                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                                        {
                                            unitId = Convert.ToInt32(ds.Tables[0].Rows[0]["UnitID"]);
                                            System.Diagnostics.Debug.WriteLine($"Found UnitId {unitId} from UnitMaster table via stored procedure for Unit {dgvInvoice.Rows[i].Cells["Unit"].Value}");
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error retrieving UnitId from UnitMaster table via stored procedure: {ex.Message}. Using fallback.");
                            }
                        }

                        // If we still don't have a valid UnitId, get any valid UnitId using stored procedure
                        if (unitId == 0)
                        {
                            try
                            {
                                using (SqlCommand anyUnitCmd = new SqlCommand(STOREDPROCEDURE.POS_UnitMaster, (SqlConnection)DataConnection, trans))
                                {
                                    anyUnitCmd.CommandType = CommandType.StoredProcedure;
                                    anyUnitCmd.Parameters.AddWithValue("@_Operation", "GETALL");

                                    // Add required parameters with default values
                                    anyUnitCmd.Parameters.AddWithValue("@UnitID", DBNull.Value);
                                    anyUnitCmd.Parameters.AddWithValue("@UnitName", DBNull.Value);
                                    anyUnitCmd.Parameters.AddWithValue("@UnitSymbol", DBNull.Value);
                                    anyUnitCmd.Parameters.AddWithValue("@UnitQuantityCode", DBNull.Value);
                                    anyUnitCmd.Parameters.AddWithValue("@Packing", DBNull.Value);
                                    anyUnitCmd.Parameters.AddWithValue("@NoOfDecimalPlaces", DBNull.Value);
                                    anyUnitCmd.Parameters.AddWithValue("@UnitNameInBill", DBNull.Value);
                                    anyUnitCmd.Parameters.AddWithValue("@IsDelete", DBNull.Value);

                                    using (SqlDataAdapter adapter = new SqlDataAdapter(anyUnitCmd))
                                    {
                                        DataSet ds = new DataSet();
                                        adapter.Fill(ds);

                                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                                        {
                                            unitId = Convert.ToInt32(ds.Tables[0].Rows[0]["UnitID"]);
                                            System.Diagnostics.Debug.WriteLine($"Using first available UnitId {unitId} as fallback via stored procedure");
                                        }
                                        else
                                        {
                                            // Last resort - create PCS unit if none exist
                                            try
                                            {
                                                using (SqlCommand createUnitCmd = new SqlCommand(STOREDPROCEDURE.POS_UnitMaster, (SqlConnection)DataConnection, trans))
                                                {
                                                    createUnitCmd.CommandType = CommandType.StoredProcedure;
                                                    createUnitCmd.Parameters.AddWithValue("@UnitName", "PCS");
                                                    createUnitCmd.Parameters.AddWithValue("@UnitSymbol", "PCS");
                                                    createUnitCmd.Parameters.AddWithValue("@_Operation", "CREATE");

                                                    // Add other required parameters
                                                    createUnitCmd.Parameters.AddWithValue("@UnitID", DBNull.Value);
                                                    createUnitCmd.Parameters.AddWithValue("@UnitQuantityCode", DBNull.Value);
                                                    createUnitCmd.Parameters.AddWithValue("@Packing", 1);
                                                    createUnitCmd.Parameters.AddWithValue("@NoOfDecimalPlaces", 0);
                                                    createUnitCmd.Parameters.AddWithValue("@UnitNameInBill", "PCS");
                                                    createUnitCmd.Parameters.AddWithValue("@IsDelete", false);

                                                    using (SqlDataAdapter createAdapter = new SqlDataAdapter(createUnitCmd))
                                                    {
                                                        DataSet createDs = new DataSet();
                                                        createAdapter.Fill(createDs);

                                                        if (createDs != null && createDs.Tables.Count > 0 && createDs.Tables[0].Rows.Count > 0)
                                                        {
                                                            unitId = Convert.ToInt32(createDs.Tables[0].Rows[0]["UnitID"]);
                                                            System.Diagnostics.Debug.WriteLine($"Created PCS unit with ID {unitId} via stored procedure");
                                                        }
                                                        else
                                                        {
                                                            // Absolute last resort
                                                            unitId = 1;
                                                            System.Diagnostics.Debug.WriteLine("Using hard-coded UnitId 1 as absolute last resort");
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception createEx)
                                            {
                                                unitId = 1; // Absolute last resort
                                                System.Diagnostics.Debug.WriteLine($"Error creating PCS unit via stored procedure: {createEx.Message}. Using hard-coded ID 1.");
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                unitId = 1; // Absolute last resort
                                System.Diagnostics.Debug.WriteLine($"Error in fallback unit ID query via stored procedure: {ex.Message}. Using hard-coded ID 1.");
                            }
                        }

                        using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_SReturnDetails, (SqlConnection)DataConnection, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            // Core fields only - reduced parameter set
                            cmd.Parameters.AddWithValue("@CompanyId", sr.CompanyId);
                            cmd.Parameters.AddWithValue("@BranchID", sr.BranchId);
                            cmd.Parameters.AddWithValue("@FinYearId", sr.FinYearId);
                            cmd.Parameters.AddWithValue("@SReturnNo", sr.SReturnNo);
                            cmd.Parameters.AddWithValue("@SReturnDate", sr.SReturnDate);
                            cmd.Parameters.AddWithValue("@InvoiceNo", string.IsNullOrEmpty(sr.InvoiceNo) ? "" : sr.InvoiceNo);
                            cmd.Parameters.AddWithValue("@SlNo", i + 1);
                            // Safe conversions for all parameters to prevent DBNull casting errors
                            cmd.Parameters.AddWithValue("@ItemID", dgvInvoice.Rows[i].Cells["ItemId"].Value != null && dgvInvoice.Rows[i].Cells["ItemId"].Value != DBNull.Value ? Convert.ToInt32(dgvInvoice.Rows[i].Cells["ItemId"].Value) : 0);
                            cmd.Parameters.AddWithValue("@ItemName", dgvInvoice.Rows[i].Cells["ItemName"].Value?.ToString() ?? "");

                            // Ensure Unit value is never null
                            string unitValue = "";
                            if (dgvInvoice.Rows[i].Cells["Unit"].Value != null && dgvInvoice.Rows[i].Cells["Unit"].Value != DBNull.Value)
                            {
                                unitValue = dgvInvoice.Rows[i].Cells["Unit"].Value.ToString().Trim();
                                // If still empty after trimming, use a default value
                                if (string.IsNullOrEmpty(unitValue))
                                {
                                    unitValue = "PCS";
                                    System.Diagnostics.Debug.WriteLine($"Empty unit value for item {dgvInvoice.Rows[i].Cells["ItemName"].Value}, using default 'PCS'");
                                }
                            }
                            else
                            {
                                unitValue = "PCS"; // Default unit if null
                                System.Diagnostics.Debug.WriteLine($"NULL unit value for item {dgvInvoice.Rows[i].Cells["ItemName"].Value}, using default 'PCS'");
                            }

                            cmd.Parameters.AddWithValue("@Unit", unitValue);
                            cmd.Parameters.AddWithValue("@UnitId", unitId); // Add the UnitId parameter

                            // Save only the current return quantity entered by the user
                            decimal currentReturnQty = dgvInvoice.Rows[i].Cells["ReturnQty"].Value != null && dgvInvoice.Rows[i].Cells["ReturnQty"].Value != DBNull.Value ? Convert.ToDecimal(dgvInvoice.Rows[i].Cells["ReturnQty"].Value) : 0m;
                            decimal unitPriceDec = dgvInvoice.Rows[i].Cells["UnitPrice"].Value != null && dgvInvoice.Rows[i].Cells["UnitPrice"].Value != DBNull.Value ? Convert.ToDecimal(dgvInvoice.Rows[i].Cells["UnitPrice"].Value) : 0m;

                            // Safe decimal conversions
                            cmd.Parameters.AddWithValue("@Qty", dgvInvoice.Rows[i].Cells["Qty"].Value != null && dgvInvoice.Rows[i].Cells["Qty"].Value != DBNull.Value ? Convert.ToDecimal(dgvInvoice.Rows[i].Cells["Qty"].Value) : 0m);
                            // Compute cumulative ReturnedQty and reset ReturnQty to 0
                            int itemIdValue = dgvInvoice.Rows[i].Cells["ItemId"].Value != null && dgvInvoice.Rows[i].Cells["ItemId"].Value != DBNull.Value ? Convert.ToInt32(dgvInvoice.Rows[i].Cells["ItemId"].Value) : 0;
                            decimal previouslyReturned = GetTotalReturnedQty(sr.InvoiceNo ?? string.Empty, itemIdValue, trans, null);
                            decimal newReturnedTotal = previouslyReturned + currentReturnQty;
                            cmd.Parameters.AddWithValue("@ReturnQty", currentReturnQty);
                            cmd.Parameters.AddWithValue("@ReturnedQty", newReturnedTotal);
                            cmd.Parameters.AddWithValue("@SalesPrice", unitPriceDec);
                            cmd.Parameters.AddWithValue("@Amount", unitPriceDec * currentReturnQty);
                            cmd.Parameters.AddWithValue("@Reason", dgvInvoice.Rows[i].Cells["Reason"].Value?.ToString() ?? "Select Reason");
                            cmd.Parameters.AddWithValue("@BranchName", sr.BranchName ?? DataBase.Branch ?? "");

                            // Add missing required fields with safe conversions
                            cmd.Parameters.AddWithValue("@Free", 0);
                            cmd.Parameters.AddWithValue("@Cost", dgvInvoice.Rows[i].Cells["Cost"].Value != null && dgvInvoice.Rows[i].Cells["Cost"].Value != DBNull.Value ? Convert.ToDecimal(dgvInvoice.Rows[i].Cells["Cost"].Value) : 0m); // Use actual cost from grid
                            cmd.Parameters.AddWithValue("@DisPer", 0);
                            cmd.Parameters.AddWithValue("@DisAmt", 0);
                            // Get tax values from grid if available, otherwise use 0
                            decimal taxPer = 0;
                            decimal taxAmt = 0;

                            if (dgvInvoice.Rows[i].Cells["TaxPer"].Value != null && dgvInvoice.Rows[i].Cells["TaxPer"].Value != DBNull.Value)
                            {
                                taxPer = Convert.ToDecimal(dgvInvoice.Rows[i].Cells["TaxPer"].Value);
                            }
                            if (dgvInvoice.Rows[i].Cells["TaxAmt"].Value != null && dgvInvoice.Rows[i].Cells["TaxAmt"].Value != DBNull.Value)
                            {
                                taxAmt = Convert.ToDecimal(dgvInvoice.Rows[i].Cells["TaxAmt"].Value);
                            }

                            // Apply global tax setting
                            if (!DataBase.IsTaxEnabled)
                            {
                                taxPer = 0;
                                taxAmt = 0;
                            }

                            cmd.Parameters.AddWithValue("@TaxPer", taxPer);
                            cmd.Parameters.AddWithValue("@TaxAmt", taxAmt);
                            cmd.Parameters.AddWithValue("@TotalSP", dgvInvoice.Rows[i].Cells["Amount"].Value != null && dgvInvoice.Rows[i].Cells["Amount"].Value != DBNull.Value ? Convert.ToDecimal(dgvInvoice.Rows[i].Cells["Amount"].Value) : 0m);
                            cmd.Parameters.AddWithValue("@OriginalSP", 0);
                            cmd.Parameters.AddWithValue("@OriginalCost", 0);
                            cmd.Parameters.AddWithValue("@Packing", packing);
                            cmd.Parameters.AddWithValue("@BaseUnit", "");
                            cmd.Parameters.AddWithValue("@IsExpiry", false);

                            // Get TaxType from grid
                            string taxType = dgvInvoice.Rows[i].Cells["TaxType"].Value != null && dgvInvoice.Rows[i].Cells["TaxType"].Value != DBNull.Value
                                ? dgvInvoice.Rows[i].Cells["TaxType"].Value.ToString() : "incl";
                            cmd.Parameters.AddWithValue("@TaxType", taxType);

                            // Get BaseAmount (taxable value) from grid for GST compliance
                            decimal baseAmount = 0m;
                            if (dgvInvoice.Columns.Contains("BaseAmount") && dgvInvoice.Rows[i].Cells["BaseAmount"].Value != null && dgvInvoice.Rows[i].Cells["BaseAmount"].Value != DBNull.Value)
                            {
                                baseAmount = Convert.ToDecimal(dgvInvoice.Rows[i].Cells["BaseAmount"].Value);
                            }
                            else
                            {
                                // Calculate BaseAmount if not in grid
                                decimal amountVal = dgvInvoice.Rows[i].Cells["Amount"].Value != null && dgvInvoice.Rows[i].Cells["Amount"].Value != DBNull.Value
                                    ? Convert.ToDecimal(dgvInvoice.Rows[i].Cells["Amount"].Value) : 0m;
                                if (taxType.ToLower() == "incl" && taxPer > 0)
                                {
                                    decimal divisor = 1 + (taxPer / 100);
                                    baseAmount = Math.Round(amountVal / divisor, 2);
                                }
                                else
                                {
                                    baseAmount = amountVal;
                                }
                            }
                            cmd.Parameters.AddWithValue("@BaseAmount", baseAmount);

                            cmd.Parameters.AddWithValue("@SeriesID", 0);

                            // Insert the row using CREATE operation in the stored procedure 
                            cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                            cmd.ExecuteNonQuery();
                        }

                        // Update stock using the correct sales return stock adjustment stored procedure
                        try
                        {
                            using (SqlCommand stockCmd = new SqlCommand(STOREDPROCEDURE._SalesReturn_PriceSettings, (SqlConnection)DataConnection, trans))
                            {
                                stockCmd.CommandType = CommandType.StoredProcedure;
                                stockCmd.Parameters.AddWithValue("@CompanyId", sr.CompanyId);
                                stockCmd.Parameters.AddWithValue("@BranchId", sr.BranchId);
                                stockCmd.Parameters.AddWithValue("@FinYearId", sr.FinYearId);
                                stockCmd.Parameters.AddWithValue("@ItemId", dgvInvoice.Rows[i].Cells["ItemId"].Value != null && dgvInvoice.Rows[i].Cells["ItemId"].Value != DBNull.Value ? Convert.ToInt32(dgvInvoice.Rows[i].Cells["ItemId"].Value) : 0);
                                stockCmd.Parameters.AddWithValue("@UnitId", unitId);
                                // Use the current return quantity for stock adjustment, not original sold Qty
                                decimal stockReturnQty = 0m;
                                if (dgvInvoice.Rows[i].Cells["ReturnQty"] != null && dgvInvoice.Rows[i].Cells["ReturnQty"].Value != null && dgvInvoice.Rows[i].Cells["ReturnQty"].Value != DBNull.Value)
                                {
                                    decimal.TryParse(dgvInvoice.Rows[i].Cells["ReturnQty"].Value.ToString(), out stockReturnQty);
                                }
                                stockCmd.Parameters.AddWithValue("@Qty", stockReturnQty);
                                stockCmd.Parameters.AddWithValue("@SingleItemCost", dgvInvoice.Rows[i].Cells["Cost"].Value != null && dgvInvoice.Rows[i].Cells["Cost"].Value != DBNull.Value ? Convert.ToDecimal(dgvInvoice.Rows[i].Cells["Cost"].Value) : 0m);
                                stockCmd.Parameters.AddWithValue("@Packing", packing); // Use actual packing value from grid
                                stockCmd.Parameters.AddWithValue("@SReturnNo", sr.SReturnNo);
                                stockCmd.Parameters.AddWithValue("@_Operation", "CREATE");

                                object result = stockCmd.ExecuteScalar();
                                System.Diagnostics.Debug.WriteLine($"Stock adjustment result for item {dgvInvoice.Rows[i].Cells["ItemName"].Value}: {result}");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error updating stock for item {dgvInvoice.Rows[i].Cells["ItemName"].Value}: {ex.Message}");
                            throw new Exception($"Failed to update stock for returned item '{dgvInvoice.Rows[i].Cells["ItemName"].Value}'. Sales return was not saved.", ex);
                        }
                    }

                    // Now create the actual voucher entries with the generated voucher ID
                    try
                    {
                        // Calculate GST amounts from return details
                        Dictionary<double, double> gstTaxAmounts = CalculateGSTAmounts(dgvInvoice);
                        DateTime voucherDate = sr.SReturnDate;
                        int slNo = 1;

                        // Calculate amounts
                        double totalGST = gstTaxAmounts.Values.Sum();
                        double returnAmountWithoutGST = sr.GrandTotal - totalGST;

                        // Create voucher entries based on payment mode (reverse of Sales)
                        if (sr.PaymodeID == 2) // CASH
                        {
                            // First voucher entry - Credit Cash (cash goes out on return)
                            objVoucher = new Voucher();
                            PopulateBaseVoucherProperties(objVoucher, sr, voucherDate);
                            objVoucher.GroupID = Convert.ToInt32(AccountGroup.CASH_IN_HAND);
                            objVoucher.LedgerID = ledgerRepository.GetLedgerId(DefaultLedgers.CASH, (int)AccountGroup.CASH_IN_HAND, DataBase.BranchId != null ? Convert.ToInt32(DataBase.BranchId) : 0);
                            objVoucher.LedgerName = DefaultLedgers.CASH;
                            objVoucher.Debit = 0;
                            objVoucher.Credit = sr.GrandTotal;
                            objVoucher.SlNo = slNo++;

                            CreateVoucherEntry(objVoucher, trans, $"VoucherID={objVoucher.VoucherID}, Type=CREDIT, Account=Cash, Amount={sr.GrandTotal}");

                            // Second voucher entry - Debit Sales (without GST)
                            PopulateBaseVoucherProperties(objVoucher, sr, voucherDate);
                            objVoucher.GroupID = Convert.ToInt32(AccountGroup.SALES_ACCOUNT);
                            objVoucher.LedgerID = ledgerRepository.GetLedgerId(DefaultLedgers.SALE, (int)AccountGroup.SALES_ACCOUNT, DataBase.BranchId != null ? Convert.ToInt32(DataBase.BranchId) : 0);
                            objVoucher.LedgerName = DefaultLedgers.SALE;
                            objVoucher.Credit = 0;
                            objVoucher.Debit = returnAmountWithoutGST;
                            objVoucher.SlNo = slNo++;

                            CreateVoucherEntry(objVoucher, trans, $"VoucherID={objVoucher.VoucherID}, Type=DEBIT, Account=Sales, Amount={returnAmountWithoutGST}");

                            // Create GST voucher entries (CGST and SGST) - DEBITED for return
                            CreateGSTReturnVoucherEntries(sr, objVoucher, trans, gstTaxAmounts, voucherDate, ref slNo);
                        }
                        else // CREDIT
                        {
                            // First voucher entry - Credit Customer Account (customer gets credit for return)
                            objVoucher = new Voucher();
                            PopulateBaseVoucherProperties(objVoucher, sr, voucherDate);
                            objVoucher.GroupID = Convert.ToInt32(AccountGroup.SUNDRY_DEBTORS);
                            objVoucher.LedgerID = sr.LedgerID;
                            objVoucher.LedgerName = sr.CustomerName;
                            objVoucher.Debit = 0;
                            objVoucher.Credit = sr.GrandTotal;
                            objVoucher.SlNo = slNo++;

                            CreateVoucherEntry(objVoucher, trans, $"VoucherID={objVoucher.VoucherID}, Type=CREDIT, Customer={objVoucher.LedgerName}, Amount={sr.GrandTotal}");

                            // Second voucher entry - Debit Sales (without GST)
                            PopulateBaseVoucherProperties(objVoucher, sr, voucherDate);
                            objVoucher.GroupID = Convert.ToInt32(AccountGroup.SALES_ACCOUNT);
                            objVoucher.LedgerID = ledgerRepository.GetLedgerId(DefaultLedgers.SALE, (int)AccountGroup.SALES_ACCOUNT, DataBase.BranchId != null ? Convert.ToInt32(DataBase.BranchId) : 0);
                            objVoucher.LedgerName = DefaultLedgers.SALE;
                            objVoucher.Credit = 0;
                            objVoucher.Debit = returnAmountWithoutGST;
                            objVoucher.SlNo = slNo++;

                            CreateVoucherEntry(objVoucher, trans, $"VoucherID={objVoucher.VoucherID}, Type=DEBIT, Account=Sales, Amount={returnAmountWithoutGST}");

                            // Create GST voucher entries (CGST and SGST) - DEBITED for return
                            CreateGSTReturnVoucherEntries(sr, objVoucher, trans, gstTaxAmounts, voucherDate, ref slNo);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error creating voucher entries: " + ex.Message);
                        throw new Exception("Failed to create accounting voucher entries for sales return. Sales return was not saved.", ex);
                    }

                    trans.Commit();
                    return $"Sales return saved successfully! #{sr.SReturnNo}";
                }
                catch (Exception ex)
                {
                    if (trans != null) trans.Rollback();
                    return "Error saving sales return: " + ex.Message;
                }
                finally
                {
                    if (DataConnection.State == ConnectionState.Open)
                    {
                        try { DataConnection.Close(); } catch { /* Ignore connection close errors */ }
                    }
                }
            }
        }

        // Update the UpdateSalesReturn method to work with the stored procedure
        public string UpdateSalesReturn(SalesReturn sr, DataGridView dgvInvoice)
        {
            // Ensure connection is clean before opening
            if (DataConnection.State == ConnectionState.Open)
            {
                DataConnection.Close();
            }

            DataConnection.Open();
            // Set transaction isolation level to Serializable to prevent race conditions
            using (SqlTransaction trans = ((SqlConnection)DataConnection).BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    // Emergency fix: validate and set default Unit values if needed
                    for (int i = 0; i < dgvInvoice.Rows.Count; i++)
                    {
                        if (dgvInvoice.Rows[i].Cells["Unit"] != null)
                        {
                            if (dgvInvoice.Rows[i].Cells["Unit"].Value == null ||
                                dgvInvoice.Rows[i].Cells["Unit"].Value == DBNull.Value ||
                                string.IsNullOrWhiteSpace(dgvInvoice.Rows[i].Cells["Unit"].Value?.ToString()))
                            {
                                dgvInvoice.Rows[i].Cells["Unit"].Value = "PCS";
                                System.Diagnostics.Debug.WriteLine($"Emergency Unit fix: Set NULL/empty Unit to 'PCS' for row {i}");
                            }
                        }
                    }

                    // Continue with normal processing
                    // Ensure required fields are set
                    sr.CompanyId = DataBase.CompanyId != null ? Convert.ToInt32(DataBase.CompanyId) : 0;
                    sr.BranchId = DataBase.BranchId != null ? Convert.ToInt32(DataBase.BranchId) : 0;

                    // Use FinYearId from session context if not set
                    if (sr.FinYearId <= 0)
                    {
                        sr.FinYearId = SessionContext.FinYearId;
                        System.Diagnostics.Debug.WriteLine($"Using FinYearId={sr.FinYearId} from SessionContext for update");
                    }

                    // CRITICAL: Make sure we have a valid LedgerID
                    if (sr.LedgerID <= 0)
                    {
                        // If LedgerID is not set but we have a CustomerName, try to find the LedgerID using stored procedure
                        if (!string.IsNullOrEmpty(sr.CustomerName))
                        {
                            try
                            {
                                using (SqlCommand ledgerCmd = new SqlCommand(STOREDPROCEDURE.POS_Customer, (SqlConnection)DataConnection, trans))
                                {
                                    ledgerCmd.CommandType = CommandType.StoredProcedure;
                                    ledgerCmd.Parameters.AddWithValue("@LedgerName", sr.CustomerName);
                                    ledgerCmd.Parameters.AddWithValue("@_Operation", "GETBYLEDGERNAME");

                                    // Add other required parameters with default values
                                    ledgerCmd.Parameters.AddWithValue("@CompanyId", sr.CompanyId);
                                    ledgerCmd.Parameters.AddWithValue("@BranchId", sr.BranchId);
                                    ledgerCmd.Parameters.AddWithValue("@LedgerId", DBNull.Value);

                                    using (SqlDataAdapter adapter = new SqlDataAdapter(ledgerCmd))
                                    {
                                        DataSet ds = new DataSet();
                                        adapter.Fill(ds);

                                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                                        {
                                            sr.LedgerID = Convert.ToInt32(ds.Tables[0].Rows[0]["LedgerID"]);
                                            System.Diagnostics.Debug.WriteLine($"Found LedgerID {sr.LedgerID} for customer {sr.CustomerName} via stored procedure");
                                        }
                                        else
                                        {
                                            // If we still can't find it, we need to use a default customer ID
                                            sr.LedgerID = 1; // Using default ledger ID
                                            System.Diagnostics.Debug.WriteLine($"Could not find LedgerID for {sr.CustomerName} via stored procedure, using default 1");
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                sr.LedgerID = 1;
                                System.Diagnostics.Debug.WriteLine($"Error finding LedgerID via stored procedure: {ex.Message}. Using default 1");
                            }
                        }
                        else
                        {
                            // If no customer name, use default ledger
                            sr.LedgerID = 1;
                            System.Diagnostics.Debug.WriteLine("No CustomerName or LedgerID provided, using default LedgerID 1");
                        }
                    }

                    // Log all values for debugging
                    System.Diagnostics.Debug.WriteLine($"Updating SalesReturn - ID: {sr.SReturnNo}, LedgerID: {sr.LedgerID}");

                    // Set operation to UPDATE
                    sr._Operation = "UPDATE";

                    // Update the master record first using the stored procedure which will handle CustomerName
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_SalesReturn, (SqlConnection)DataConnection, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Add all required parameters
                        cmd.Parameters.AddWithValue("@CompanyId", sr.CompanyId);
                        cmd.Parameters.AddWithValue("@BranchId", sr.BranchId);
                        cmd.Parameters.AddWithValue("@FinYearId", sr.FinYearId);
                        cmd.Parameters.AddWithValue("@SReturnNo", sr.SReturnNo);
                        cmd.Parameters.AddWithValue("@SReturnDate", sr.SReturnDate);
                        cmd.Parameters.AddWithValue("@InvoiceNo", string.IsNullOrEmpty(sr.InvoiceNo) ? "" : sr.InvoiceNo);
                        cmd.Parameters.AddWithValue("@InvoiceDate", sr.InvoiceDate);
                        cmd.Parameters.AddWithValue("@LedgerID", sr.LedgerID); // This is critical - SP uses this to get CustomerName

                        // The stored procedure will get these values based on the IDs
                        // cmd.Parameters.AddWithValue("@CustomerName", sr.CustomerName); // SP gets this from LedgerID
                        cmd.Parameters.AddWithValue("@PaymodeID", sr.PaymodeID);
                        // cmd.Parameters.AddWithValue("@Paymode", sr.Paymode); // SP gets this from PaymodeID
                        cmd.Parameters.AddWithValue("@PaymodeLedgerID", sr.PaymodeLedgerID);

                        // Add all the amount-related parameters
                        cmd.Parameters.AddWithValue("@SubTotal", sr.SubTotal);
                        cmd.Parameters.AddWithValue("@SpDisPer", sr.SpDisPer);
                        cmd.Parameters.AddWithValue("@SpDsiAmt", sr.SpDsiAmt);
                        cmd.Parameters.AddWithValue("@BillDiscountPer", sr.BillDiscountPer);
                        cmd.Parameters.AddWithValue("@BillDiscountAmt", sr.BillDiscountAmt);
                        cmd.Parameters.AddWithValue("@TaxPer", sr.TaxPer);
                        cmd.Parameters.AddWithValue("@TaxAmt", sr.TaxAmt);
                        cmd.Parameters.AddWithValue("@Frieght", sr.Frieght);
                        cmd.Parameters.AddWithValue("@GrandTotal", sr.GrandTotal);
                        cmd.Parameters.AddWithValue("@CancelFlag", sr.CancelFlag);
                        cmd.Parameters.AddWithValue("@UserID", sr.UserID);
                        // cmd.Parameters.AddWithValue("@UserName", sr.UserName); // SP gets this from UserID
                        cmd.Parameters.AddWithValue("@TaxType", string.IsNullOrEmpty(sr.TaxType) ? "" : sr.TaxType);
                        cmd.Parameters.AddWithValue("@RoundOff", sr.RoundOff);
                        cmd.Parameters.AddWithValue("@CessPer", sr.CessPer);
                        cmd.Parameters.AddWithValue("@CessAmt", sr.CessAmt);
                        cmd.Parameters.AddWithValue("@VoucherID", sr.VoucherID);
                        // cmd.Parameters.AddWithValue("@BranchName", sr.BranchName); // SP gets this from BranchId
                        cmd.Parameters.AddWithValue("@CalAfterTax", sr.CalAfterTax);
                        // cmd.Parameters.AddWithValue("@CurSymbol", sr.CurSymbol); // SP gets this from CompanyId
                        cmd.Parameters.AddWithValue("@SeriesId", sr.SeriesId);
                        cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(sr.Status) ? "" : sr.Status);
                        cmd.Parameters.AddWithValue("@_Operation", sr._Operation);

                        // For completeness, add any parameters that might be used in the stored procedure 
                        // but not mapped in our class. These will use null values.
                        cmd.Parameters.AddWithValue("@ExpenseAmt", 0);
                        cmd.Parameters.AddWithValue("@OtherExpAmt", 0);
                        cmd.Parameters.AddWithValue("@KFCessPer", 0);
                        cmd.Parameters.AddWithValue("@KFCessAmt", 0);
                        cmd.Parameters.AddWithValue("@Remarks", DBNull.Value);

                        try
                        {
                            SqlDataAdapter da = new SqlDataAdapter(cmd);
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            System.Diagnostics.Debug.WriteLine("Master record update successful");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error updating master record: {ex.Message}");
                            throw; // Rethrow the exception to be caught by outer try-catch
                        }
                    }

                    // STEP 1: Reverse stock for old return details BEFORE deleting them
                    // Fetch old details using GETALLSRETURNDETAILS
                    try
                    {
                        using (SqlCommand getOldCmd = new SqlCommand(STOREDPROCEDURE.POS_SReturnDetails, (SqlConnection)DataConnection, trans))
                        {
                            getOldCmd.CommandType = CommandType.StoredProcedure;
                            getOldCmd.Parameters.AddWithValue("@SReturnNo", sr.SReturnNo);
                            getOldCmd.Parameters.AddWithValue("@_Operation", "GETALLSRETURNDETAILS");

                            using (SqlDataAdapter adapter = new SqlDataAdapter(getOldCmd))
                            {
                                DataTable oldDetails = new DataTable();
                                adapter.Fill(oldDetails);

                                // Reverse stock for each old detail item
                                foreach (DataRow oldRow in oldDetails.Rows)
                                {
                                    try
                                    {
                                        int oldItemId = oldRow.Table.Columns.Contains("ItemID") && oldRow["ItemID"] != DBNull.Value ? Convert.ToInt32(oldRow["ItemID"]) : 0;
                                        // Use ReturnQty (actual qty for THIS return) for stock reversal, not ReturnedQty (which is cumulative)
                                        decimal oldQty = 0m;
                                        if (oldRow.Table.Columns.Contains("ReturnQty") && oldRow["ReturnQty"] != DBNull.Value)
                                        {
                                            oldQty = Convert.ToDecimal(oldRow["ReturnQty"]);
                                        }
                                        // Fallback: if ReturnQty is 0 (old records), use ReturnedQty as best-effort
                                        if (oldQty <= 0 && oldRow.Table.Columns.Contains("ReturnedQty") && oldRow["ReturnedQty"] != DBNull.Value)
                                        {
                                            oldQty = Convert.ToDecimal(oldRow["ReturnedQty"]);
                                        }
                                        decimal oldPacking = oldRow.Table.Columns.Contains("Packing") && oldRow["Packing"] != DBNull.Value ? Convert.ToDecimal(oldRow["Packing"]) : 1m;
                                        int oldUnitId = oldRow.Table.Columns.Contains("UnitId") && oldRow["UnitId"] != DBNull.Value ? Convert.ToInt32(oldRow["UnitId"]) : 0;
                                        decimal oldCost = oldRow.Table.Columns.Contains("Cost") && oldRow["Cost"] != DBNull.Value ? Convert.ToDecimal(oldRow["Cost"]) : 0m;

                                        if (oldItemId > 0 && oldQty > 0)
                                        {
                                            using (SqlCommand reverseStockCmd = new SqlCommand(STOREDPROCEDURE._SalesReturn_PriceSettings, (SqlConnection)DataConnection, trans))
                                            {
                                                reverseStockCmd.CommandType = CommandType.StoredProcedure;
                                                reverseStockCmd.Parameters.AddWithValue("@CompanyId", sr.CompanyId);
                                                reverseStockCmd.Parameters.AddWithValue("@BranchId", sr.BranchId);
                                                reverseStockCmd.Parameters.AddWithValue("@FinYearId", sr.FinYearId);
                                                reverseStockCmd.Parameters.AddWithValue("@ItemId", oldItemId);
                                                reverseStockCmd.Parameters.AddWithValue("@UnitId", oldUnitId);
                                                reverseStockCmd.Parameters.AddWithValue("@Qty", oldQty);
                                                reverseStockCmd.Parameters.AddWithValue("@SingleItemCost", oldCost);
                                                reverseStockCmd.Parameters.AddWithValue("@Packing", oldPacking);
                                                reverseStockCmd.Parameters.AddWithValue("@SReturnNo", sr.SReturnNo);
                                                reverseStockCmd.Parameters.AddWithValue("@_Operation", "DELETE");

                                                object result = reverseStockCmd.ExecuteScalar();
                                                System.Diagnostics.Debug.WriteLine($"Reversed stock for ItemId {oldItemId}, Qty {oldQty}: {result}");
                                            }
                                        }
                                    }
                                    catch (Exception stockEx)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"Error reversing stock for old detail: {stockEx.Message}");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error fetching old details for stock reversal: {ex.Message}");
                        // Continue - stock reversal failure should not block the update
                    }

                    // STEP 2: Delete all existing details records
                    try
                    {
                        using (SqlCommand deleteCmd = new SqlCommand(STOREDPROCEDURE.POS_SReturnDetails, (SqlConnection)DataConnection, trans))
                        {
                            deleteCmd.CommandType = CommandType.StoredProcedure;
                            deleteCmd.Parameters.AddWithValue("@CompanyId", sr.CompanyId);
                            deleteCmd.Parameters.AddWithValue("@BranchId", sr.BranchId);
                            deleteCmd.Parameters.AddWithValue("@FinYearId", sr.FinYearId);
                            deleteCmd.Parameters.AddWithValue("@SReturnNo", sr.SReturnNo);
                            // Ensure correct voucher type for sales return
                            deleteCmd.Parameters.AddWithValue("@VoucherType", "Credit Note");
                            deleteCmd.Parameters.AddWithValue("@VoucherID", sr.VoucherID);
                            deleteCmd.Parameters.AddWithValue("@_Operation", "DELETE");

                            deleteCmd.ExecuteNonQuery();
                            System.Diagnostics.Debug.WriteLine("Deleted existing detail records successfully");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error deleting existing detail records: {ex.Message}");
                        throw; // Rethrow the exception to be caught by outer try-catch
                    }

                    // Now insert new details for each selected row
                    int selectedRowCount = 0;
                    for (int i = 0; i < dgvInvoice.Rows.Count; i++)
                    {
                        // Skip unselected rows - they shouldn't be in the DataGridView but just in case
                        if (dgvInvoice.Rows[i].Cells["Select"] != null &&
                            dgvInvoice.Rows[i].Cells["Select"].Value != null &&
                            !(bool)dgvInvoice.Rows[i].Cells["Select"].Value)
                            continue;

                        if (dgvInvoice.Rows[i].Cells["BarCode"].Value == null) continue;

                        selectedRowCount++;

                        // Get packing value from grid data, default to 1 if not available
                        decimal packing = 1;
                        if (dgvInvoice.Rows[i].Cells["Packing"] != null && dgvInvoice.Rows[i].Cells["Packing"].Value != null)
                        {
                            if (decimal.TryParse(dgvInvoice.Rows[i].Cells["Packing"].Value.ToString(), out decimal gridPacking))
                            {
                                packing = gridPacking;
                            }
                        }

                        try
                        {
                            // Get the UnitId for this item's unit using stored procedure
                            int unitId = 0;
                            try
                            {
                                using (SqlCommand unitIdCmd = new SqlCommand(STOREDPROCEDURE.POS_UnitMaster, (SqlConnection)DataConnection, trans))
                                {
                                    unitIdCmd.CommandType = CommandType.StoredProcedure;
                                    unitIdCmd.Parameters.AddWithValue("@UnitName", dgvInvoice.Rows[i].Cells["Unit"].Value?.ToString() ?? "");
                                    unitIdCmd.Parameters.AddWithValue("@_Operation", "GetByName");

                                    // Add required parameters with default values
                                    unitIdCmd.Parameters.AddWithValue("@UnitID", DBNull.Value);
                                    unitIdCmd.Parameters.AddWithValue("@UnitSymbol", DBNull.Value);
                                    unitIdCmd.Parameters.AddWithValue("@UnitQuantityCode", DBNull.Value);
                                    unitIdCmd.Parameters.AddWithValue("@Packing", DBNull.Value);
                                    unitIdCmd.Parameters.AddWithValue("@NoOfDecimalPlaces", DBNull.Value);
                                    unitIdCmd.Parameters.AddWithValue("@UnitNameInBill", DBNull.Value);
                                    unitIdCmd.Parameters.AddWithValue("@IsDelete", DBNull.Value);

                                    using (SqlDataAdapter adapter = new SqlDataAdapter(unitIdCmd))
                                    {
                                        DataSet ds = new DataSet();
                                        adapter.Fill(ds);

                                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                                        {
                                            unitId = Convert.ToInt32(ds.Tables[0].Rows[0]["UnitID"]);
                                            System.Diagnostics.Debug.WriteLine($"Found UnitId {unitId} for Unit {dgvInvoice.Rows[i].Cells["Unit"].Value} via stored procedure");
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error retrieving UnitId via stored procedure: {ex.Message}. Using fallback.");
                            }

                            // If UnitId is still 0, use fallback to first available unit
                            if (unitId == 0)
                            {
                                try
                                {
                                    using (SqlCommand anyUnitCmd = new SqlCommand(STOREDPROCEDURE.POS_UnitMaster, (SqlConnection)DataConnection, trans))
                                    {
                                        anyUnitCmd.CommandType = CommandType.StoredProcedure;
                                        anyUnitCmd.Parameters.AddWithValue("@_Operation", "GETALL");

                                        // Add required parameters with default values
                                        anyUnitCmd.Parameters.AddWithValue("@UnitID", DBNull.Value);
                                        anyUnitCmd.Parameters.AddWithValue("@UnitName", DBNull.Value);
                                        anyUnitCmd.Parameters.AddWithValue("@UnitSymbol", DBNull.Value);
                                        anyUnitCmd.Parameters.AddWithValue("@UnitQuantityCode", DBNull.Value);
                                        anyUnitCmd.Parameters.AddWithValue("@Packing", DBNull.Value);
                                        anyUnitCmd.Parameters.AddWithValue("@NoOfDecimalPlaces", DBNull.Value);
                                        anyUnitCmd.Parameters.AddWithValue("@UnitNameInBill", DBNull.Value);
                                        anyUnitCmd.Parameters.AddWithValue("@IsDelete", DBNull.Value);

                                        using (SqlDataAdapter adapter = new SqlDataAdapter(anyUnitCmd))
                                        {
                                            DataSet ds = new DataSet();
                                            adapter.Fill(ds);

                                            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                                            {
                                                unitId = Convert.ToInt32(ds.Tables[0].Rows[0]["UnitID"]);
                                                System.Diagnostics.Debug.WriteLine($"Using first available UnitId {unitId} as fallback via stored procedure");
                                            }
                                            else
                                            {
                                                unitId = 1; // Absolute last resort
                                                System.Diagnostics.Debug.WriteLine("Using hard-coded UnitId 1 as absolute last resort");
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    unitId = 1; // Absolute last resort
                                    System.Diagnostics.Debug.WriteLine($"Error in fallback unit ID query via stored procedure: {ex.Message}. Using hard-coded ID 1.");
                                }
                            }

                            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_SReturnDetails, (SqlConnection)DataConnection, trans))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;

                                // Core fields only - reduced parameter set for insert
                                cmd.Parameters.AddWithValue("@CompanyId", sr.CompanyId);
                                cmd.Parameters.AddWithValue("@BranchID", sr.BranchId);
                                cmd.Parameters.AddWithValue("@FinYearId", sr.FinYearId);
                                cmd.Parameters.AddWithValue("@SReturnNo", sr.SReturnNo);
                                cmd.Parameters.AddWithValue("@SReturnDate", sr.SReturnDate);
                                cmd.Parameters.AddWithValue("@InvoiceNo", string.IsNullOrEmpty(sr.InvoiceNo) ? "" : sr.InvoiceNo);
                                cmd.Parameters.AddWithValue("@SlNo", i + 1);
                                // Safe conversions for all parameters to prevent DBNull casting errors
                                cmd.Parameters.AddWithValue("@ItemID", dgvInvoice.Rows[i].Cells["ItemId"].Value != null && dgvInvoice.Rows[i].Cells["ItemId"].Value != DBNull.Value ? Convert.ToInt32(dgvInvoice.Rows[i].Cells["ItemId"].Value) : 0);
                                cmd.Parameters.AddWithValue("@ItemName", dgvInvoice.Rows[i].Cells["ItemName"].Value?.ToString() ?? "");

                                // Ensure Unit value is never null
                                string unitValue = "";
                                if (dgvInvoice.Rows[i].Cells["Unit"].Value != null && dgvInvoice.Rows[i].Cells["Unit"].Value != DBNull.Value)
                                {
                                    unitValue = dgvInvoice.Rows[i].Cells["Unit"].Value.ToString().Trim();
                                    // If still empty after trimming, use a default value
                                    if (string.IsNullOrEmpty(unitValue))
                                    {
                                        unitValue = "PCS";
                                        System.Diagnostics.Debug.WriteLine($"Empty unit value for item {dgvInvoice.Rows[i].Cells["ItemName"].Value}, using default 'PCS'");
                                    }
                                }
                                else
                                {
                                    unitValue = "PCS"; // Default unit if null
                                    System.Diagnostics.Debug.WriteLine($"NULL unit value for item {dgvInvoice.Rows[i].Cells["ItemName"].Value}, using default 'PCS'");
                                }

                                cmd.Parameters.AddWithValue("@Unit", unitValue);
                                cmd.Parameters.AddWithValue("@UnitId", unitId); // Add the UnitId parameter

                                // Save only the current return quantity entered by the user
                                decimal currentReturnQty = dgvInvoice.Rows[i].Cells["ReturnQty"].Value != null && dgvInvoice.Rows[i].Cells["ReturnQty"].Value != DBNull.Value ? Convert.ToDecimal(dgvInvoice.Rows[i].Cells["ReturnQty"].Value) : 0m;
                                decimal unitPriceDec = dgvInvoice.Rows[i].Cells["UnitPrice"].Value != null && dgvInvoice.Rows[i].Cells["UnitPrice"].Value != DBNull.Value ? Convert.ToDecimal(dgvInvoice.Rows[i].Cells["UnitPrice"].Value) : 0m;

                                // Safe decimal conversions
                                cmd.Parameters.AddWithValue("@Qty", dgvInvoice.Rows[i].Cells["Qty"].Value != null && dgvInvoice.Rows[i].Cells["Qty"].Value != DBNull.Value ? Convert.ToDecimal(dgvInvoice.Rows[i].Cells["Qty"].Value) : 0m);
                                // Compute cumulative ReturnedQty and reset ReturnQty to 0 for persisted record
                                int itemIdValue = dgvInvoice.Rows[i].Cells["ItemId"].Value != null && dgvInvoice.Rows[i].Cells["ItemId"].Value != DBNull.Value ? Convert.ToInt32(dgvInvoice.Rows[i].Cells["ItemId"].Value) : 0;
                                decimal previouslyReturned = GetTotalReturnedQty(sr.InvoiceNo ?? string.Empty, itemIdValue, trans, null);
                                decimal newReturnedTotal = previouslyReturned + currentReturnQty;
                                cmd.Parameters.AddWithValue("@ReturnQty", currentReturnQty);
                                cmd.Parameters.AddWithValue("@ReturnedQty", newReturnedTotal);
                                cmd.Parameters.AddWithValue("@SalesPrice", unitPriceDec);
                                cmd.Parameters.AddWithValue("@Amount", unitPriceDec * currentReturnQty);
                                cmd.Parameters.AddWithValue("@Reason", dgvInvoice.Rows[i].Cells["Reason"].Value?.ToString() ?? "");
                                cmd.Parameters.AddWithValue("@BranchName", string.IsNullOrEmpty(sr.BranchName) ? DataBase.Branch : sr.BranchName);

                                // Add missing required fields with safe conversions
                                cmd.Parameters.AddWithValue("@Free", 0); // Free quantity - required by database
                                cmd.Parameters.AddWithValue("@Cost", dgvInvoice.Rows[i].Cells["Cost"].Value != null && dgvInvoice.Rows[i].Cells["Cost"].Value != DBNull.Value ? Convert.ToDecimal(dgvInvoice.Rows[i].Cells["Cost"].Value) : 0m); // Use actual cost from grid
                                cmd.Parameters.AddWithValue("@DisPer", 0); // Discount percentage - may be required
                                cmd.Parameters.AddWithValue("@DisAmt", 0); // Discount amount - may be required
                                // Get tax values from grid if available, otherwise use 0
                                decimal taxPer = 0;
                                decimal taxAmt = 0;

                                if (dgvInvoice.Rows[i].Cells["TaxPer"].Value != null && dgvInvoice.Rows[i].Cells["TaxPer"].Value != DBNull.Value)
                                {
                                    taxPer = Convert.ToDecimal(dgvInvoice.Rows[i].Cells["TaxPer"].Value);
                                }
                                if (dgvInvoice.Rows[i].Cells["TaxAmt"].Value != null && dgvInvoice.Rows[i].Cells["TaxAmt"].Value != DBNull.Value)
                                {
                                    taxAmt = Convert.ToDecimal(dgvInvoice.Rows[i].Cells["TaxAmt"].Value);
                                }

                                // Apply global tax setting
                                if (!DataBase.IsTaxEnabled)
                                {
                                    taxPer = 0;
                                    taxAmt = 0;
                                }

                                cmd.Parameters.AddWithValue("@TaxPer", taxPer); // Tax percentage - may be required
                                cmd.Parameters.AddWithValue("@TaxAmt", taxAmt); // Tax amount - may be required
                                cmd.Parameters.AddWithValue("@TotalSP", dgvInvoice.Rows[i].Cells["Amount"].Value != null && dgvInvoice.Rows[i].Cells["Amount"].Value != DBNull.Value ? Convert.ToDecimal(dgvInvoice.Rows[i].Cells["Amount"].Value) : 0m); // Total sales price - may be required
                                cmd.Parameters.AddWithValue("@OriginalSP", 0); // Original SP - may be required
                                cmd.Parameters.AddWithValue("@OriginalCost", 0); // Original cost - may be required
                                cmd.Parameters.AddWithValue("@Packing", packing); // Use actual packing value from grid
                                cmd.Parameters.AddWithValue("@BaseUnit", ""); // Base unit - may be required
                                cmd.Parameters.AddWithValue("@IsExpiry", false);

                                // Get TaxType from grid
                                string taxType = dgvInvoice.Rows[i].Cells["TaxType"].Value != null && dgvInvoice.Rows[i].Cells["TaxType"].Value != DBNull.Value
                                    ? dgvInvoice.Rows[i].Cells["TaxType"].Value.ToString() : "incl";
                                cmd.Parameters.AddWithValue("@TaxType", taxType);

                                // Get BaseAmount (taxable value) from grid for GST compliance
                                decimal baseAmount = 0m;
                                if (dgvInvoice.Columns.Contains("BaseAmount") && dgvInvoice.Rows[i].Cells["BaseAmount"].Value != null && dgvInvoice.Rows[i].Cells["BaseAmount"].Value != DBNull.Value)
                                {
                                    baseAmount = Convert.ToDecimal(dgvInvoice.Rows[i].Cells["BaseAmount"].Value);
                                }
                                else
                                {
                                    // Calculate BaseAmount if not in grid
                                    decimal amountVal = dgvInvoice.Rows[i].Cells["Amount"].Value != null && dgvInvoice.Rows[i].Cells["Amount"].Value != DBNull.Value
                                        ? Convert.ToDecimal(dgvInvoice.Rows[i].Cells["Amount"].Value) : 0m;
                                    if (taxType.ToLower() == "incl" && taxPer > 0)
                                    {
                                        decimal divisor = 1 + (taxPer / 100);
                                        baseAmount = Math.Round(amountVal / divisor, 2);
                                    }
                                    else
                                    {
                                        baseAmount = amountVal;
                                    }
                                }
                                cmd.Parameters.AddWithValue("@BaseAmount", baseAmount);

                                cmd.Parameters.AddWithValue("@SeriesID", 0);

                                // Insert the row using CREATE operation in the stored procedure 
                                cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                                cmd.ExecuteNonQuery();
                            }

                            // Update stock using the correct sales return stock adjustment stored procedure
                            try
                            {
                                using (SqlCommand stockCmd = new SqlCommand(STOREDPROCEDURE._SalesReturn_PriceSettings, (SqlConnection)DataConnection, trans))
                                {
                                    stockCmd.CommandType = CommandType.StoredProcedure;
                                    stockCmd.Parameters.AddWithValue("@CompanyId", sr.CompanyId);
                                    stockCmd.Parameters.AddWithValue("@BranchId", sr.BranchId);
                                    stockCmd.Parameters.AddWithValue("@FinYearId", sr.FinYearId);
                                    stockCmd.Parameters.AddWithValue("@ItemId", dgvInvoice.Rows[i].Cells["ItemId"].Value != null && dgvInvoice.Rows[i].Cells["ItemId"].Value != DBNull.Value ? Convert.ToInt32(dgvInvoice.Rows[i].Cells["ItemId"].Value) : 0);
                                    stockCmd.Parameters.AddWithValue("@UnitId", unitId);
                                    // Use current return quantity for stock adjustment during update
                                    decimal stockReturnQty = 0m;
                                    if (dgvInvoice.Rows[i].Cells["ReturnQty"] != null && dgvInvoice.Rows[i].Cells["ReturnQty"].Value != null && dgvInvoice.Rows[i].Cells["ReturnQty"].Value != DBNull.Value)
                                    {
                                        decimal.TryParse(dgvInvoice.Rows[i].Cells["ReturnQty"].Value.ToString(), out stockReturnQty);
                                    }
                                    stockCmd.Parameters.AddWithValue("@Qty", stockReturnQty);
                                    stockCmd.Parameters.AddWithValue("@SingleItemCost", dgvInvoice.Rows[i].Cells["Cost"].Value != null && dgvInvoice.Rows[i].Cells["Cost"].Value != DBNull.Value ? Convert.ToDecimal(dgvInvoice.Rows[i].Cells["Cost"].Value) : 0m);
                                    stockCmd.Parameters.AddWithValue("@Packing", packing); // Use actual packing value from grid
                                    stockCmd.Parameters.AddWithValue("@SReturnNo", sr.SReturnNo);
                                    stockCmd.Parameters.AddWithValue("@_Operation", "CREATE");

                                    object result = stockCmd.ExecuteScalar();
                                    System.Diagnostics.Debug.WriteLine($"Stock adjustment result for item {dgvInvoice.Rows[i].Cells["ItemName"].Value}: {result}");
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error updating stock for item {dgvInvoice.Rows[i].Cells["ItemName"].Value}: {ex.Message}");
                                throw new Exception($"Failed to update stock for returned item '{dgvInvoice.Rows[i].Cells["ItemName"].Value}'. Sales return update was not saved.", ex);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error adding detail row {i + 1}: {ex.Message}");
                            throw; // Rethrow the exception to be caught by outer try-catch
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"Processed {selectedRowCount} selected rows");

                    // Check if we had any selected rows
                    if (selectedRowCount == 0)
                    {
                        throw new Exception("No items were selected for update. Please select at least one item.");
                    }

                    // First delete existing voucher entries
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, (SqlConnection)DataConnection, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("VoucherID", sr.VoucherID);
                        cmd.Parameters.AddWithValue("BranchID", sr.BranchId);
                        cmd.Parameters.AddWithValue("VoucherType", "Credit Note");
                        cmd.Parameters.AddWithValue("_Operation", "UPDATE");

                        cmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Deleted existing voucher entries for VoucherID: {sr.VoucherID}");
                    }

                    // Now create new voucher entries based on payment mode (reverse of Sales)
                    if (sr.PaymodeID == 2) // CASH
                    {
                        // First voucher entry - Debit Cash (reverse of Sales)
                        Voucher objVoucher = new Voucher();
                        objVoucher.CompanyID = DataBase.CompanyId != null ? Convert.ToInt32(DataBase.CompanyId) : 0;
                        objVoucher.BranchID = DataBase.BranchId != null ? Convert.ToInt32(DataBase.BranchId) : 0;
                        objVoucher.VoucherID = sr.VoucherID;
                        objVoucher.VoucherSeriesID = sr.SeriesId;
                        objVoucher.VoucherDate = sr.SReturnDate;
                        objVoucher.VoucherNumber = "SR" + sr.SReturnNo.ToString();
                        objVoucher.GroupID = Convert.ToInt32(AccountGroup.CASH_IN_HAND);
                        objVoucher.LedgerID = ledgerRepository.GetLedgerId(DefaultLedgers.CASH, (int)AccountGroup.CASH_IN_HAND, DataBase.BranchId != null ? Convert.ToInt32(DataBase.BranchId) : 0);
                        objVoucher.LedgerName = DefaultLedgers.CASH;
                        objVoucher.VoucherType = "Credit Note";
                        // Cash goes out on sales return -> Credit cash
                        objVoucher.Debit = 0;
                        objVoucher.Credit = sr.GrandTotal;
                        objVoucher.Narration = "Sales Return #" + sr.SReturnNo;
                        objVoucher.SlNo = 1;
                        objVoucher.Mode = "";
                        objVoucher.ModeID = 0;
                        objVoucher.UserDate = DateTime.Now;
                        objVoucher.UserName = DataBase.UserName;
                        objVoucher.UserID = DataBase.UserId != null ? Convert.ToInt32(DataBase.UserId) : 0;
                        objVoucher.CancelFlag = false;
                        objVoucher.FinYearID = sr.FinYearId;
                        objVoucher.IsSyncd = false;
                        objVoucher._Operation = "CREATE";

                        using (SqlCommand voucherEntryCmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, (SqlConnection)DataConnection, (SqlTransaction)trans))
                        {
                            voucherEntryCmd.CommandType = CommandType.StoredProcedure;
                            voucherEntryCmd.Parameters.AddWithValue("@CompanyID", objVoucher.CompanyID);
                            voucherEntryCmd.Parameters.AddWithValue("@BranchID", objVoucher.BranchID);
                            voucherEntryCmd.Parameters.AddWithValue("@VoucherID", objVoucher.VoucherID);
                            voucherEntryCmd.Parameters.AddWithValue("@VoucherSeriesID", objVoucher.VoucherSeriesID);
                            voucherEntryCmd.Parameters.AddWithValue("@VoucherDate", objVoucher.VoucherDate);
                            voucherEntryCmd.Parameters.AddWithValue("@VoucherNumber", objVoucher.VoucherNumber);
                            voucherEntryCmd.Parameters.AddWithValue("GroupID", objVoucher.GroupID);
                            voucherEntryCmd.Parameters.AddWithValue("LedgerID", objVoucher.LedgerID);
                            voucherEntryCmd.Parameters.AddWithValue("LedgerName", objVoucher.LedgerName);
                            voucherEntryCmd.Parameters.AddWithValue("VoucherType", objVoucher.VoucherType);
                            voucherEntryCmd.Parameters.AddWithValue("Credit", objVoucher.Credit);
                            voucherEntryCmd.Parameters.AddWithValue("Debit", objVoucher.Debit);
                            voucherEntryCmd.Parameters.AddWithValue("Narration", objVoucher.Narration);
                            voucherEntryCmd.Parameters.AddWithValue("SlNo", objVoucher.SlNo);
                            voucherEntryCmd.Parameters.AddWithValue("Mode", objVoucher.Mode);
                            voucherEntryCmd.Parameters.AddWithValue("ModeID", objVoucher.ModeID);
                            voucherEntryCmd.Parameters.AddWithValue("UserID", objVoucher.UserID);
                            voucherEntryCmd.Parameters.AddWithValue("UserName", objVoucher.UserName);
                            voucherEntryCmd.Parameters.AddWithValue("FinYearID", objVoucher.FinYearID);
                            voucherEntryCmd.Parameters.AddWithValue("_Operation", objVoucher._Operation);
                            voucherEntryCmd.Parameters.AddWithValue("UserDate", objVoucher.UserDate);

                            voucherEntryCmd.ExecuteNonQuery();
                            System.Diagnostics.Debug.WriteLine($"Created cash credit voucher entry for VoucherID: {sr.VoucherID}");
                        }

                        // Second voucher entry - Debit Sales (reverse of Sales)
                        // Calculate GST amounts for proper voucher split
                        Dictionary<double, double> gstTaxAmounts = CalculateGSTAmounts(dgvInvoice);
                        double totalGST = gstTaxAmounts.Values.Sum();
                        double returnAmountWithoutGST = sr.GrandTotal - totalGST;
                        int slNo = 2;

                        objVoucher.GroupID = Convert.ToInt32(AccountGroup.SALES_ACCOUNT);
                        objVoucher.LedgerID = ledgerRepository.GetLedgerId(DefaultLedgers.SALE, (int)AccountGroup.SALES_ACCOUNT, Convert.ToInt32(DataBase.BranchId));
                        objVoucher.LedgerName = DefaultLedgers.SALE;
                        // Reverse of sales: Debit Sales (Sales Return) - without GST
                        objVoucher.Credit = 0;
                        objVoucher.Debit = returnAmountWithoutGST;
                        objVoucher.SlNo = slNo++;

                        using (SqlCommand voucherEntryCmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, (SqlConnection)DataConnection, (SqlTransaction)trans))
                        {
                            voucherEntryCmd.CommandType = CommandType.StoredProcedure;
                            voucherEntryCmd.Parameters.AddWithValue("@CompanyID", objVoucher.CompanyID);
                            voucherEntryCmd.Parameters.AddWithValue("@BranchID", objVoucher.BranchID);
                            voucherEntryCmd.Parameters.AddWithValue("@VoucherID", objVoucher.VoucherID);
                            voucherEntryCmd.Parameters.AddWithValue("@VoucherSeriesID", objVoucher.VoucherSeriesID);
                            voucherEntryCmd.Parameters.AddWithValue("@VoucherDate", objVoucher.VoucherDate);
                            voucherEntryCmd.Parameters.AddWithValue("@VoucherNumber", objVoucher.VoucherNumber);
                            voucherEntryCmd.Parameters.AddWithValue("GroupID", objVoucher.GroupID);
                            voucherEntryCmd.Parameters.AddWithValue("LedgerID", objVoucher.LedgerID);
                            voucherEntryCmd.Parameters.AddWithValue("LedgerName", objVoucher.LedgerName);
                            voucherEntryCmd.Parameters.AddWithValue("VoucherType", objVoucher.VoucherType);
                            voucherEntryCmd.Parameters.AddWithValue("Credit", objVoucher.Credit);
                            voucherEntryCmd.Parameters.AddWithValue("Debit", objVoucher.Debit);
                            voucherEntryCmd.Parameters.AddWithValue("Narration", objVoucher.Narration);
                            voucherEntryCmd.Parameters.AddWithValue("SlNo", objVoucher.SlNo);
                            voucherEntryCmd.Parameters.AddWithValue("Mode", objVoucher.Mode);
                            voucherEntryCmd.Parameters.AddWithValue("ModeID", objVoucher.ModeID);
                            voucherEntryCmd.Parameters.AddWithValue("UserID", objVoucher.UserID);
                            voucherEntryCmd.Parameters.AddWithValue("UserName", objVoucher.UserName);
                            voucherEntryCmd.Parameters.AddWithValue("FinYearID", objVoucher.FinYearID);
                            voucherEntryCmd.Parameters.AddWithValue("_Operation", objVoucher._Operation);
                            voucherEntryCmd.Parameters.AddWithValue("UserDate", objVoucher.UserDate);

                            voucherEntryCmd.ExecuteNonQuery();
                            System.Diagnostics.Debug.WriteLine($"Created sales debit voucher entry for VoucherID: {sr.VoucherID}");
                        }

                        // Create GST voucher entries (CGST and SGST) - DEBITED for return
                        CreateGSTReturnVoucherEntries(sr, objVoucher, trans, gstTaxAmounts, sr.SReturnDate, ref slNo);
                    }
                    else // CREDIT
                    {
                        // First voucher entry - Debit Customer Account (reverse of Sales)
                        Voucher objVoucher = new Voucher();
                        objVoucher.CompanyID = DataBase.CompanyId != null ? Convert.ToInt32(DataBase.CompanyId) : 0;
                        objVoucher.BranchID = DataBase.BranchId != null ? Convert.ToInt32(DataBase.BranchId) : 0;
                        objVoucher.VoucherID = sr.VoucherID;
                        objVoucher.VoucherSeriesID = sr.SeriesId;
                        objVoucher.VoucherDate = sr.SReturnDate;
                        objVoucher.VoucherNumber = "SR" + sr.SReturnNo.ToString();
                        objVoucher.GroupID = Convert.ToInt32(AccountGroup.SUNDRY_DEBTORS);
                        objVoucher.LedgerID = sr.LedgerID;
                        objVoucher.LedgerName = sr.CustomerName;
                        objVoucher.VoucherType = "Credit Note";
                        // Credit customer's account for credit note
                        objVoucher.Debit = 0;
                        objVoucher.Credit = sr.GrandTotal;
                        objVoucher.Narration = "Sales Return #" + sr.SReturnNo;
                        objVoucher.SlNo = 1;
                        objVoucher.Mode = "";
                        objVoucher.ModeID = 0;
                        objVoucher.UserDate = DateTime.Now;
                        objVoucher.UserName = DataBase.UserName;
                        objVoucher.UserID = Convert.ToInt32(DataBase.UserId);
                        objVoucher.CancelFlag = false;
                        objVoucher.FinYearID = sr.FinYearId;
                        objVoucher.IsSyncd = false;
                        objVoucher._Operation = "CREATE";

                        using (SqlCommand voucherEntryCmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, (SqlConnection)DataConnection, (SqlTransaction)trans))
                        {
                            voucherEntryCmd.CommandType = CommandType.StoredProcedure;
                            voucherEntryCmd.Parameters.AddWithValue("@CompanyID", objVoucher.CompanyID);
                            voucherEntryCmd.Parameters.AddWithValue("@BranchID", objVoucher.BranchID);
                            voucherEntryCmd.Parameters.AddWithValue("@VoucherID", objVoucher.VoucherID);
                            voucherEntryCmd.Parameters.AddWithValue("@VoucherSeriesID", objVoucher.VoucherSeriesID);
                            voucherEntryCmd.Parameters.AddWithValue("@VoucherDate", objVoucher.VoucherDate);
                            voucherEntryCmd.Parameters.AddWithValue("@VoucherNumber", objVoucher.VoucherNumber);
                            voucherEntryCmd.Parameters.AddWithValue("GroupID", objVoucher.GroupID);
                            voucherEntryCmd.Parameters.AddWithValue("LedgerID", objVoucher.LedgerID);
                            voucherEntryCmd.Parameters.AddWithValue("LedgerName", objVoucher.LedgerName);
                            voucherEntryCmd.Parameters.AddWithValue("VoucherType", objVoucher.VoucherType);
                            voucherEntryCmd.Parameters.AddWithValue("Credit", objVoucher.Credit);
                            voucherEntryCmd.Parameters.AddWithValue("Debit", objVoucher.Debit);
                            voucherEntryCmd.Parameters.AddWithValue("Narration", objVoucher.Narration);
                            voucherEntryCmd.Parameters.AddWithValue("SlNo", objVoucher.SlNo);
                            voucherEntryCmd.Parameters.AddWithValue("Mode", objVoucher.Mode);
                            voucherEntryCmd.Parameters.AddWithValue("ModeID", objVoucher.ModeID);
                            voucherEntryCmd.Parameters.AddWithValue("UserID", objVoucher.UserID);
                            voucherEntryCmd.Parameters.AddWithValue("UserName", objVoucher.UserName);
                            voucherEntryCmd.Parameters.AddWithValue("FinYearID", objVoucher.FinYearID);
                            voucherEntryCmd.Parameters.AddWithValue("_Operation", objVoucher._Operation);
                            voucherEntryCmd.Parameters.AddWithValue("UserDate", objVoucher.UserDate);

                            voucherEntryCmd.ExecuteNonQuery();
                            System.Diagnostics.Debug.WriteLine($"Created customer credit voucher entry for VoucherID: {sr.VoucherID}");
                        }

                        // Second voucher entry - Debit Sales (reverse of Sales)
                        // Calculate GST amounts for proper voucher split
                        Dictionary<double, double> gstTaxAmountsCredit = CalculateGSTAmounts(dgvInvoice);
                        double totalGSTCredit = gstTaxAmountsCredit.Values.Sum();
                        double returnAmountWithoutGSTCredit = sr.GrandTotal - totalGSTCredit;
                        int slNoCredit = 2;

                        objVoucher.GroupID = Convert.ToInt32(AccountGroup.SALES_ACCOUNT);
                        objVoucher.LedgerID = ledgerRepository.GetLedgerId(DefaultLedgers.SALE, (int)AccountGroup.SALES_ACCOUNT, Convert.ToInt32(DataBase.BranchId));
                        objVoucher.LedgerName = DefaultLedgers.SALE;
                        // Debit Sales (Sales Return) - without GST
                        objVoucher.Credit = 0;
                        objVoucher.Debit = returnAmountWithoutGSTCredit;
                        objVoucher.SlNo = slNoCredit++;

                        using (SqlCommand voucherEntryCmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, (SqlConnection)DataConnection, (SqlTransaction)trans))
                        {
                            voucherEntryCmd.CommandType = CommandType.StoredProcedure;
                            voucherEntryCmd.Parameters.AddWithValue("@CompanyID", objVoucher.CompanyID);
                            voucherEntryCmd.Parameters.AddWithValue("@BranchID", objVoucher.BranchID);
                            voucherEntryCmd.Parameters.AddWithValue("@VoucherID", objVoucher.VoucherID);
                            voucherEntryCmd.Parameters.AddWithValue("@VoucherSeriesID", objVoucher.VoucherSeriesID);
                            voucherEntryCmd.Parameters.AddWithValue("@VoucherDate", objVoucher.VoucherDate);
                            voucherEntryCmd.Parameters.AddWithValue("@VoucherNumber", objVoucher.VoucherNumber);
                            voucherEntryCmd.Parameters.AddWithValue("GroupID", objVoucher.GroupID);
                            voucherEntryCmd.Parameters.AddWithValue("LedgerID", objVoucher.LedgerID);
                            voucherEntryCmd.Parameters.AddWithValue("LedgerName", objVoucher.LedgerName);
                            voucherEntryCmd.Parameters.AddWithValue("VoucherType", objVoucher.VoucherType);
                            voucherEntryCmd.Parameters.AddWithValue("Credit", objVoucher.Credit);
                            voucherEntryCmd.Parameters.AddWithValue("Debit", objVoucher.Debit);
                            voucherEntryCmd.Parameters.AddWithValue("Narration", objVoucher.Narration);
                            voucherEntryCmd.Parameters.AddWithValue("SlNo", objVoucher.SlNo);
                            voucherEntryCmd.Parameters.AddWithValue("Mode", objVoucher.Mode);
                            voucherEntryCmd.Parameters.AddWithValue("ModeID", objVoucher.ModeID);
                            voucherEntryCmd.Parameters.AddWithValue("UserID", objVoucher.UserID);
                            voucherEntryCmd.Parameters.AddWithValue("UserName", objVoucher.UserName);
                            voucherEntryCmd.Parameters.AddWithValue("FinYearID", objVoucher.FinYearID);
                            voucherEntryCmd.Parameters.AddWithValue("_Operation", objVoucher._Operation);
                            voucherEntryCmd.Parameters.AddWithValue("UserDate", objVoucher.UserDate);

                            voucherEntryCmd.ExecuteNonQuery();
                            System.Diagnostics.Debug.WriteLine($"Created sales debit voucher entry for VoucherID: {sr.VoucherID}");
                        }

                        // Create GST voucher entries (CGST and SGST) - DEBITED for return
                        CreateGSTReturnVoucherEntries(sr, objVoucher, trans, gstTaxAmountsCredit, sr.SReturnDate, ref slNoCredit);
                    }

                    trans.Commit();
                    return "Sales return updated successfully!";
                }
                catch (Exception ex)
                {
                    if (trans != null) trans.Rollback();
                    return "Error updating sales return: " + ex.Message;
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

        public List<SRgetAll> GetAll()
        {
            List<SRgetAll> srList = new List<SRgetAll>();
            DataConnection.Open();
            System.Diagnostics.Debug.WriteLine("Retrieving sales returns with CancelFlag = 0");

            try
            {
                // Try to use the stored procedure first to get all records
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_SalesReturn, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    cmd.Parameters.AddWithValue("@CancelFlag", 0); // Explicitly pass CancelFlag=0 to get only active records
                    cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId); // Pass current branch
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId > 0 ? SessionContext.FinYearId : (int.TryParse(DataBase.FinyearId, out var id) ? id : 1));
                    cmd.Parameters.AddWithValue("@CompanyId", DataBase.CompanyId); // Pass current company

                    using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                    {
                        DataSet dt = new DataSet();
                        adp.Fill(dt);
                        if (dt.Tables.Count > 0 && dt.Tables[0].Rows.Count > 0)
                        {
                            // Manually map to avoid float/double (Single/Double) casting issues across environments
                            DataTable t = dt.Tables[0];
                            foreach (DataRow r in t.Rows)
                            {
                                SRgetAll item = new SRgetAll();
                                try
                                {
                                    item.SReturnNo = r.Table.Columns.Contains("SReturnNo") && r["SReturnNo"] != DBNull.Value ? Convert.ToInt32(r["SReturnNo"]) : 0;
                                    item.SReturnDate = r.Table.Columns.Contains("SReturnDate") && r["SReturnDate"] != DBNull.Value ? Convert.ToDateTime(r["SReturnDate"]) : DateTime.MinValue;
                                    item.InvoiceNo = r.Table.Columns.Contains("InvoiceNo") && r["InvoiceNo"] != DBNull.Value ? r["InvoiceNo"].ToString() : string.Empty;
                                    item.CustomerName = r.Table.Columns.Contains("CustomerName") && r["CustomerName"] != DBNull.Value ? r["CustomerName"].ToString() : string.Empty;
                                    item.LedgerID = r.Table.Columns.Contains("LedgerID") && r["LedgerID"] != DBNull.Value ? Convert.ToInt64(r["LedgerID"]) : 0;

                                    // Safely handle numeric types that may arrive as Single, Double, Decimal, etc.
                                    if (r.Table.Columns.Contains("GrandTotal") && r["GrandTotal"] != DBNull.Value)
                                    {
                                        object v = r["GrandTotal"];
                                        if (v is float f) item.GrandTotal = Convert.ToDouble(f);
                                        else if (v is double d) item.GrandTotal = d;
                                        else if (v is decimal m) item.GrandTotal = Convert.ToDouble(m);
                                        else item.GrandTotal = Convert.ToDouble(v);
                                    }
                                    else
                                    {
                                        item.GrandTotal = 0d;
                                    }

                                    item.PaymodeID = r.Table.Columns.Contains("PaymodeID") && r["PaymodeID"] != DBNull.Value ? Convert.ToInt32(r["PaymodeID"]) : 0;
                                    item.Paymode = r.Table.Columns.Contains("Paymode") && r["Paymode"] != DBNull.Value ? r["Paymode"].ToString() : string.Empty;
                                }
                                catch (Exception mapEx)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Mapping error in GetAll row: {mapEx.Message}");
                                }
                                srList.Add(item);
                            }
                            System.Diagnostics.Debug.WriteLine($"Retrieved {srList.Count} sales returns via stored procedure");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("No sales returns found via stored procedure or empty result set");
                        }
                    }
                }

                // Add debug output about the returned data
                if (srList != null && srList.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Returning {srList.Count} sales returns");
                    foreach (var sr in srList.Take(5)) // Log the first 5 for debugging
                    {
                        System.Diagnostics.Debug.WriteLine($"SalesReturn #{sr.SReturnNo}, Date: {sr.SReturnDate}, InvoiceNo: {sr.InvoiceNo}, Customer: {sr.CustomerName}");
                    }

                    if (srList.Count > 5)
                    {
                        System.Diagnostics.Debug.WriteLine($"...and {srList.Count - 5} more");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No sales returns found to return");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetAll: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                MessageBox.Show("Error retrieving sales returns: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return srList;
        }

        /// <summary>
        /// Gets pending sales returns for a specific customer that haven't been processed into Credit Notes.
        /// Used by FrmCreditNote to show the "Goods Return List" filtered by customer (like IRS POS).
        /// </summary>
        /// <param name="customerLedgerId">The customer's LedgerID. If 0, returns all pending returns.</param>
        /// <returns>List of pending sales returns for the customer</returns>
        public List<SRgetAll> GetPendingByCustomer(int customerLedgerId)
        {
            List<SRgetAll> srList = new List<SRgetAll>();

            if (DataConnection.State == ConnectionState.Open)
                DataConnection.Close();

            DataConnection.Open();

            try
            {
                // Get all active sales returns for this customer
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_SalesReturn, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    cmd.Parameters.AddWithValue("@CancelFlag", 0);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);

                    using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adp.Fill(ds);

                        if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            DataTable t = ds.Tables[0];
                            foreach (DataRow r in t.Rows)
                            {
                                SRgetAll item = new SRgetAll();
                                try
                                {
                                    item.SReturnNo = r.Table.Columns.Contains("SReturnNo") && r["SReturnNo"] != DBNull.Value ? Convert.ToInt32(r["SReturnNo"]) : 0;
                                    item.SReturnDate = r.Table.Columns.Contains("SReturnDate") && r["SReturnDate"] != DBNull.Value ? Convert.ToDateTime(r["SReturnDate"]) : DateTime.MinValue;
                                    item.InvoiceNo = r.Table.Columns.Contains("InvoiceNo") && r["InvoiceNo"] != DBNull.Value ? r["InvoiceNo"].ToString() : string.Empty;
                                    item.CustomerName = r.Table.Columns.Contains("CustomerName") && r["CustomerName"] != DBNull.Value ? r["CustomerName"].ToString() : string.Empty;
                                    item.LedgerID = r.Table.Columns.Contains("LedgerID") && r["LedgerID"] != DBNull.Value ? Convert.ToInt64(r["LedgerID"]) : 0;

                                    if (r.Table.Columns.Contains("GrandTotal") && r["GrandTotal"] != DBNull.Value)
                                    {
                                        object v = r["GrandTotal"];
                                        if (v is float f) item.GrandTotal = Convert.ToDouble(f);
                                        else if (v is double d) item.GrandTotal = d;
                                        else if (v is decimal m) item.GrandTotal = Convert.ToDouble(m);
                                        else item.GrandTotal = Convert.ToDouble(v);
                                    }
                                    else
                                    {
                                        item.GrandTotal = 0d;
                                    }

                                    item.PaymodeID = r.Table.Columns.Contains("PaymodeID") && r["PaymodeID"] != DBNull.Value ? Convert.ToInt32(r["PaymodeID"]) : 0;
                                    item.Paymode = r.Table.Columns.Contains("Paymode") && r["Paymode"] != DBNull.Value ? r["Paymode"].ToString() : string.Empty;

                                    srList.Add(item);
                                }
                                catch (Exception mapEx)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Mapping error in GetPendingByCustomer row: {mapEx.Message}");
                                }
                            }
                        }
                    }
                }

                // Now filter by customer if specified
                if (customerLedgerId > 0)
                {
                    // Get the LedgerID for each SReturnNo and filter
                    var filteredList = new List<SRgetAll>();
                    foreach (var sr in srList)
                    {
                        var fullReturn = GetBySalesReturnId(sr.SReturnNo);
                        if (fullReturn != null && fullReturn.LedgerID == customerLedgerId)
                        {
                            sr.LedgerID = fullReturn.LedgerID;
                            filteredList.Add(sr);
                        }
                    }
                    srList = filteredList;
                }

                // Exclude returns that already have Credit Notes
                var pendingList = new List<SRgetAll>();
                foreach (var sr in srList)
                {
                    if (!HasCreditNote(sr.SReturnNo))
                    {
                        pendingList.Add(sr);
                    }
                }
                srList = pendingList;

                System.Diagnostics.Debug.WriteLine($"GetPendingByCustomer: Found {srList.Count} pending returns for customer {customerLedgerId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetPendingByCustomer: {ex.Message}");
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return srList;
        }

        /// <summary>
        public bool HasCreditNote(int sReturnNo)
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._CreditNoteMaster, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SReturnNo", sReturnNo);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYSRETURNNO");

                    using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adp.Fill(ds);

                        // If we got any rows, a Credit Note exists for this return
                        return ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking for Credit Note: {ex.Message}");
                return false; // Assume no credit note if error
            }
        }

        // Get invoice number and payment method information
        public void GetInvoiceAndPaymentInfo(int sReturnNo, out string invoiceNo, out int paymodeId, out string paymodeName)
        {
            invoiceNo = "";
            paymodeId = 0;
            paymodeName = "";

            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_SalesReturn, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SReturnNo", sReturnNo);
                    cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId > 0 ? SessionContext.FinYearId : (int.TryParse(DataBase.FinyearId, out var id) ? id : 1));
                    cmd.Parameters.AddWithValue("@_Operation", "GETPAYMENTINFO");

                    System.Diagnostics.Debug.WriteLine($"Retrieving payment info for SReturnNo: {sReturnNo}");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);

                        // Add detailed debugging about the result set
                        if (ds != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Received {ds.Tables.Count} tables in dataset");

                            if (ds.Tables.Count > 0 && ds.Tables[0] != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"Table 0 has {ds.Tables[0].Rows.Count} rows and {ds.Tables[0].Columns.Count} columns");

                                if (ds.Tables[0].Rows.Count > 0)
                                {
                                    DataRow row = ds.Tables[0].Rows[0];

                                    // Log all column names and values for debugging
                                    foreach (DataColumn col in ds.Tables[0].Columns)
                                    {
                                        string colValue = row[col] != DBNull.Value ? row[col].ToString() : "NULL";
                                        System.Diagnostics.Debug.WriteLine($"Column: {col.ColumnName}, Value: {colValue}");
                                    }

                                    // Get invoice number
                                    if (!Convert.IsDBNull(row["InvoiceNo"]))
                                    {
                                        invoiceNo = row["InvoiceNo"].ToString();
                                        System.Diagnostics.Debug.WriteLine($"Retrieved InvoiceNo: {invoiceNo}");
                                    }

                                    // Get payment mode ID
                                    if (!Convert.IsDBNull(row["PaymodeID"]))
                                    {
                                        paymodeId = Convert.ToInt32(row["PaymodeID"]);
                                        System.Diagnostics.Debug.WriteLine($"Retrieved PaymodeID: {paymodeId}");
                                    }

                                    // Get payment mode name
                                    if (!Convert.IsDBNull(row["Paymode"]))
                                    {
                                        paymodeName = row["Paymode"].ToString();
                                        System.Diagnostics.Debug.WriteLine($"Retrieved Paymode: {paymodeName}");
                                    }
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine("No rows returned from GETPAYMENTINFO operation");

                                    // If no data returned from specific operation, try getting full data
                                    GetFullSalesReturnData(sReturnNo, out invoiceNo, out paymodeId, out paymodeName);
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("No tables returned from GETPAYMENTINFO operation");

                                // If no tables returned, try getting full data
                                GetFullSalesReturnData(sReturnNo, out invoiceNo, out paymodeId, out paymodeName);
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Null dataset returned from GETPAYMENTINFO operation");

                            // If null dataset, try getting full data
                            GetFullSalesReturnData(sReturnNo, out invoiceNo, out paymodeId, out paymodeName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting payment info: {ex.Message}");
                // Try fallback method on error
                GetFullSalesReturnData(sReturnNo, out invoiceNo, out paymodeId, out paymodeName);
                throw; // Preserve stack trace
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        // Fallback method to get full sales return data including payment info
        private void GetFullSalesReturnData(int sReturnNo, out string invoiceNo, out int paymodeId, out string paymodeName)
        {
            invoiceNo = "";
            paymodeId = 0;
            paymodeName = "";

            System.Diagnostics.Debug.WriteLine($"Using fallback method to get payment info for SReturnNo: {sReturnNo}");

            try
            {
                // Ensure connection is open
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_SalesReturn, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SReturnNo", sReturnNo);
                    cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId > 0 ? SessionContext.FinYearId : (int.TryParse(DataBase.FinyearId, out var id) ? id : 1));
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);

                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            DataRow row = ds.Tables[0].Rows[0];

                            // Get invoice number
                            if (!Convert.IsDBNull(row["InvoiceNo"]))
                            {
                                invoiceNo = row["InvoiceNo"].ToString();
                                System.Diagnostics.Debug.WriteLine($"Fallback retrieved InvoiceNo: {invoiceNo}");
                            }

                            // Get payment mode ID
                            if (!Convert.IsDBNull(row["PaymodeID"]))
                            {
                                paymodeId = Convert.ToInt32(row["PaymodeID"]);
                                System.Diagnostics.Debug.WriteLine($"Fallback retrieved PaymodeID: {paymodeId}");
                            }

                            // Get payment mode name
                            if (!Convert.IsDBNull(row["Paymode"]))
                            {
                                paymodeName = row["Paymode"].ToString();
                                System.Diagnostics.Debug.WriteLine($"Fallback retrieved Paymode: {paymodeName}");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("No data returned from fallback method");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in fallback method: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        public string DeleteSalesReturn(int sReturnNo, int voucherId)
        {
            // Add detailed debugging at the start
            System.Diagnostics.Debug.WriteLine($"DeleteSalesReturn called with SReturnNo={sReturnNo}, VoucherID={voucherId}");
            System.Diagnostics.Debug.WriteLine($"Current BranchId={DataBase.BranchId}, FinYearId={DataBase.FinyearId}");

            DataConnection.Open();
            using (SqlTransaction trans = (SqlTransaction)DataConnection.BeginTransaction())
            {
                try
                {
                    bool masterUpdated = false;
                    bool detailsUpdated = false;
                    bool voucherUpdated = false;
                    bool storedProcSuccess = false;

                    // Get the actual BranchId and FinYearId values - ensure they're integers
                    int branchId = 0;
                    int finYearId = SessionContext.FinYearId; // Use session context instead of hardcoded value

                    // Try to parse the BranchId from DataBase
                    if (!int.TryParse(DataBase.BranchId, out branchId) || branchId <= 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Invalid BranchId in DataBase, trying to find the record directly");

                        // Try to find the record using stored procedure
                        using (SqlCommand checkCmd = new SqlCommand(STOREDPROCEDURE.POS_SalesReturn, (SqlConnection)DataConnection, trans))
                        {
                            checkCmd.CommandType = CommandType.StoredProcedure;
                            checkCmd.Parameters.Add(new SqlParameter("@SReturnNo", SqlDbType.Int) { Value = sReturnNo });
                            checkCmd.Parameters.Add(new SqlParameter("@_Operation", SqlDbType.VarChar, 50) { Value = "GETBRANCHINFO" });

                            using (SqlDataAdapter adapter = new SqlDataAdapter(checkCmd))
                            {
                                DataSet ds = new DataSet();
                                adapter.Fill(ds);

                                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                                {
                                    DataRow row = ds.Tables[0].Rows[0];
                                    branchId = Convert.ToInt32(row["BranchId"]);
                                    finYearId = SessionContext.FinYearId > 0 ? SessionContext.FinYearId : (int.TryParse(DataBase.FinyearId, out var id) ? id : 1);

                                    System.Diagnostics.Debug.WriteLine($"Found record with BranchId={branchId}, FinYearId={finYearId} (forced to 1)");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine("Record not found with SReturnNo via stored procedure");
                                }
                            }
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Using BranchId from DataBase: {branchId}");
                    }

                    // FinYearId is hardcoded; skip any DB lookup and log
                    System.Diagnostics.Debug.WriteLine("Using hardcoded FinYearId=1 as per requirement");

                    // Final validation of our ID values
                    if (branchId <= 0 || finYearId <= 0)
                    {
                        return "Failed to delete sales return: Could not determine valid Branch ID or Financial Year ID.";
                    }

                    System.Diagnostics.Debug.WriteLine($"Starting delete process for SalesReturn #{sReturnNo} with Branch={branchId}, FinYear={finYearId}");

                    // 1. First check if the record exists using the stored procedure
                    int recordExists = 0;
                    try
                    {
                        using (SqlCommand checkCmd = new SqlCommand(STOREDPROCEDURE.POS_SalesReturn, (SqlConnection)DataConnection, trans))
                        {
                            checkCmd.CommandType = CommandType.StoredProcedure;
                            checkCmd.Parameters.Add(new SqlParameter("@SReturnNo", SqlDbType.Int) { Value = sReturnNo });
                            checkCmd.Parameters.Add(new SqlParameter("@BranchId", SqlDbType.Int) { Value = branchId });
                            checkCmd.Parameters.Add(new SqlParameter("@FinYearId", SqlDbType.Int) { Value = finYearId });
                            checkCmd.Parameters.Add(new SqlParameter("@_Operation", SqlDbType.VarChar, 50) { Value = "GETBYID" });

                            using (SqlDataAdapter adapter = new SqlDataAdapter(checkCmd))
                            {
                                DataSet ds = new DataSet();
                                adapter.Fill(ds);

                                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                                {
                                    recordExists = 1;
                                    System.Diagnostics.Debug.WriteLine($"Found record via stored procedure: SReturnNo={sReturnNo}");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"Record not found via stored procedure: SReturnNo={sReturnNo}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error checking record existence via stored procedure: {ex.Message}");
                        recordExists = 0;
                    }

                    if (recordExists == 0)
                    {
                        trans.Rollback();
                        return $"Failed to delete sales return: Record #{sReturnNo} not found in database.";
                    }

                    // 2. Try to use the stored procedure for deletion first
                    storedProcSuccess = false;
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_SalesReturn, (SqlConnection)DataConnection, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@CompanyId", SqlDbType.Int) { Value = Convert.ToInt32(DataBase.CompanyId) });
                            cmd.Parameters.Add(new SqlParameter("@BranchId", SqlDbType.Int) { Value = branchId });
                            cmd.Parameters.Add(new SqlParameter("@FinYearId", SqlDbType.Int) { Value = finYearId });
                            cmd.Parameters.Add(new SqlParameter("@SReturnNo", SqlDbType.Int) { Value = sReturnNo });
                            cmd.Parameters.Add(new SqlParameter("@VoucherID", SqlDbType.Int) { Value = voucherId });
                            cmd.Parameters.Add(new SqlParameter("@VoucherType", SqlDbType.VarChar, 50) { Value = "Credit Note" });
                            cmd.Parameters.Add(new SqlParameter("@_Operation", SqlDbType.VarChar, 50) { Value = "DELETE" });

                            object result = cmd.ExecuteScalar();
                            if (result != null && result.ToString().Contains("SUCCESS"))
                            {
                                storedProcSuccess = true;
                                System.Diagnostics.Debug.WriteLine($"Stored procedure deletion executed successfully: {result}");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Stored procedure deletion executed but returned: {(result ?? "null")}");
                            }
                        }
                    }
                    catch (Exception spEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error calling stored procedure for deletion: {spEx.Message}");
                        storedProcSuccess = false;
                    }

                    // 3. If stored procedure didn't work, try direct SQL updates as backup
                    masterUpdated = storedProcSuccess;

                    if (!storedProcSuccess)
                    {
                        try
                        {
                            // Use stored procedure to update master record with CancelFlag
                            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_SalesReturn, (SqlConnection)DataConnection, trans))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@CompanyId", SqlDbType.Int) { Value = Convert.ToInt32(DataBase.CompanyId) });
                                cmd.Parameters.Add(new SqlParameter("@BranchId", SqlDbType.Int) { Value = branchId });
                                cmd.Parameters.Add(new SqlParameter("@FinYearId", SqlDbType.Int) { Value = SessionContext.FinYearId > 0 ? SessionContext.FinYearId : (int.TryParse(DataBase.FinyearId, out var id) ? id : 1) });
                                cmd.Parameters.Add(new SqlParameter("@SReturnNo", SqlDbType.Int) { Value = sReturnNo });
                                cmd.Parameters.Add(new SqlParameter("@VoucherId", SqlDbType.Int) { Value = voucherId });
                                cmd.Parameters.Add(new SqlParameter("@VoucherType", SqlDbType.VarChar, 50) { Value = "Credit Note" });
                                cmd.Parameters.Add(new SqlParameter("@_Operation", SqlDbType.VarChar, 50) { Value = "DELETE" });

                                object result = cmd.ExecuteScalar();
                                bool updateSuccess = (result != null && (result.ToString().Contains("SUCCESS") || result.ToString().Contains("UPDATED")));
                                System.Diagnostics.Debug.WriteLine($"Stored procedure update on SReturnMaster result: {(result ?? "null")}");

                                masterUpdated = updateSuccess;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in stored procedure update: {ex.Message}");
                            masterUpdated = false;
                        }
                    }

                    // 4. Handle SReturnDetails table using stored procedure
                    try
                    {
                        // Details are removed by master DELETE operation; no separate CANCEL supported/required
                        detailsUpdated = true;

                        System.Diagnostics.Debug.WriteLine($"SReturnDetails update via stored procedure completed: {detailsUpdated}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error updating SReturnDetails via stored procedure: {ex.Message}");
                        detailsUpdated = true; // Continue with the process even if this part fails
                    }

                    // 5. Determine if we've been successful enough to commit the transaction
                    voucherUpdated = true; // Assume voucher is handled by stored procedure
                    // detailsUpdated is already set above in the try block
                    bool success = masterUpdated || storedProcSuccess;

                    System.Diagnostics.Debug.WriteLine($"Delete operation summary: MasterUpdated={masterUpdated}, DetailsUpdated={detailsUpdated}, VoucherUpdated={voucherUpdated}, StoredProcSuccess={storedProcSuccess}");

                    if (success)
                    {
                        trans.Commit();
                        System.Diagnostics.Debug.WriteLine($"Transaction committed: SalesReturn #{sReturnNo} deleted successfully");
                        return "Sales return deleted successfully!";
                    }
                    else
                    {
                        trans.Rollback();
                        System.Diagnostics.Debug.WriteLine($"Transaction rolled back: Failed to delete SalesReturn #{sReturnNo}");
                        return "Failed to delete sales return. No records were updated.";
                    }
                }
                catch (Exception ex)
                {
                    if (trans != null)
                    {
                        try { trans.Rollback(); } catch { /* Ignore rollback errors */ }
                    }
                    System.Diagnostics.Debug.WriteLine($"Exception in DeleteSalesReturn: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }
                    return "Error deleting sales return: " + ex.Message;
                }
                finally
                {
                    if (DataConnection.State == ConnectionState.Open)
                    {
                        try { DataConnection.Close(); } catch { /* Ignore connection close errors */ }
                    }
                }
            }
        }

        // Helper method to get UnitId from Unit name using stored procedures
        public int GetUnitId(string unitName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(unitName))
                    return 1; // Default unit ID

                // Use the same pattern as other methods in this repository
                if (DataConnection != null)
                {
                    // Try to extract numeric value if the unitName is in format like "1 UNIT"
                    string searchUnitName = unitName.Trim();
                    if (searchUnitName.Contains(" "))
                    {
                        string[] parts = searchUnitName.Split(new char[] { ' ' }, 2);
                        if (int.TryParse(parts[0], out int numericValue) && parts.Length > 1)
                        {
                            // If it's in format "1 UNIT", search for just "UNIT" instead
                            searchUnitName = parts[1].Trim();
                        }
                    }

                    // First attempt: exact match using GetByName operation
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_UnitMaster, (SqlConnection)DataConnection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@UnitName", searchUnitName);
                            cmd.Parameters.AddWithValue("@_Operation", "GetByName");

                            // Add required parameters with default values
                            cmd.Parameters.AddWithValue("@UnitID", DBNull.Value);
                            cmd.Parameters.AddWithValue("@UnitSymbol", DBNull.Value);
                            cmd.Parameters.AddWithValue("@UnitQuantityCode", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Packing", DBNull.Value);
                            cmd.Parameters.AddWithValue("@NoOfDecimalPlaces", DBNull.Value);
                            cmd.Parameters.AddWithValue("@UnitNameInBill", DBNull.Value);
                            cmd.Parameters.AddWithValue("@IsDelete", DBNull.Value);

                            // Open connection if not already open
                            if (DataConnection.State != ConnectionState.Open)
                                DataConnection.Open();

                            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                            {
                                DataSet ds = new DataSet();
                                adapter.Fill(ds);

                                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                                {
                                    return Convert.ToInt32(ds.Tables[0].Rows[0]["UnitID"]);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in GetByName operation: {ex.Message}");
                    }

                    // Second attempt: search using Search operation (LIKE)
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_UnitMaster, (SqlConnection)DataConnection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@UnitName", searchUnitName);
                            cmd.Parameters.AddWithValue("@_Operation", "Search");

                            // Add required parameters with default values
                            cmd.Parameters.AddWithValue("@UnitID", DBNull.Value);
                            cmd.Parameters.AddWithValue("@UnitSymbol", DBNull.Value);
                            cmd.Parameters.AddWithValue("@UnitQuantityCode", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Packing", DBNull.Value);
                            cmd.Parameters.AddWithValue("@NoOfDecimalPlaces", DBNull.Value);
                            cmd.Parameters.AddWithValue("@UnitNameInBill", DBNull.Value);
                            cmd.Parameters.AddWithValue("@IsDelete", DBNull.Value);

                            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                            {
                                DataSet ds = new DataSet();
                                adapter.Fill(ds);

                                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                                {
                                    return Convert.ToInt32(ds.Tables[0].Rows[0]["UnitID"]);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in Search operation: {ex.Message}");
                    }

                    // Third attempt: try to find default units (PCS, EACH, EA)
                    string[] defaultUnits = { "PCS", "EACH", "EA" };
                    foreach (string defaultUnit in defaultUnits)
                    {
                        try
                        {
                            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_UnitMaster, (SqlConnection)DataConnection))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@UnitName", defaultUnit);
                                cmd.Parameters.AddWithValue("@_Operation", "GetByName");

                                // Add required parameters with default values
                                cmd.Parameters.AddWithValue("@UnitID", DBNull.Value);
                                cmd.Parameters.AddWithValue("@UnitSymbol", DBNull.Value);
                                cmd.Parameters.AddWithValue("@UnitQuantityCode", DBNull.Value);
                                cmd.Parameters.AddWithValue("@Packing", DBNull.Value);
                                cmd.Parameters.AddWithValue("@NoOfDecimalPlaces", DBNull.Value);
                                cmd.Parameters.AddWithValue("@UnitNameInBill", DBNull.Value);
                                cmd.Parameters.AddWithValue("@IsDelete", DBNull.Value);

                                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                                {
                                    DataSet ds = new DataSet();
                                    adapter.Fill(ds);

                                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                                    {
                                        return Convert.ToInt32(ds.Tables[0].Rows[0]["UnitID"]);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error searching for default unit {defaultUnit}: {ex.Message}");
                        }
                    }

                    // Fourth attempt: get any available unit using GETALL
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_UnitMaster, (SqlConnection)DataConnection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@_Operation", "GETALL");

                            // Add required parameters with default values
                            cmd.Parameters.AddWithValue("@UnitID", DBNull.Value);
                            cmd.Parameters.AddWithValue("@UnitName", DBNull.Value);
                            cmd.Parameters.AddWithValue("@UnitSymbol", DBNull.Value);
                            cmd.Parameters.AddWithValue("@UnitQuantityCode", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Packing", DBNull.Value);
                            cmd.Parameters.AddWithValue("@NoOfDecimalPlaces", DBNull.Value);
                            cmd.Parameters.AddWithValue("@UnitNameInBill", DBNull.Value);
                            cmd.Parameters.AddWithValue("@IsDelete", DBNull.Value);

                            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                            {
                                DataSet ds = new DataSet();
                                adapter.Fill(ds);

                                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                                {
                                    return Convert.ToInt32(ds.Tables[0].Rows[0]["UnitID"]);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in GETALL operation: {ex.Message}");
                    }

                    // Last resort: try to create a PCS unit if none exist
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_UnitMaster, (SqlConnection)DataConnection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@UnitName", "PCS");
                            cmd.Parameters.AddWithValue("@UnitSymbol", "PCS");
                            cmd.Parameters.AddWithValue("@UnitNameInBill", "PCS");
                            cmd.Parameters.AddWithValue("@Packing", 1);
                            cmd.Parameters.AddWithValue("@NoOfDecimalPlaces", 0);
                            cmd.Parameters.AddWithValue("@UnitQuantityCode", 1);
                            cmd.Parameters.AddWithValue("@IsDelete", false);
                            cmd.Parameters.AddWithValue("@_Operation", "Create");

                            // Add required parameters with default values
                            cmd.Parameters.AddWithValue("@UnitID", DBNull.Value);

                            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                            {
                                DataSet ds = new DataSet();
                                adapter.Fill(ds);

                                // After creating, try to get the newly created unit
                                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                                {
                                    // Try to get the created unit
                                    return GetUnitId("PCS"); // Recursive call to get the newly created unit
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error creating PCS unit: {ex.Message}");
                    }
                }

                // If all attempts fail, return default unit ID
                return 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetUnitId: {ex.Message}");
                return 1;
            }
        }

        // Simplified helper method to delete a voucher using the POS_Vouchers stored procedure
        private bool DeleteVoucherUsingStoredProcedure(int voucherId, int branchId, int finYearId, string voucherType, SqlTransaction trans = null)
        {
            System.Diagnostics.Debug.WriteLine($"Calling POS_Vouchers stored procedure for VoucherID={voucherId}, BranchID={branchId}, VoucherType={voucherType}");

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, (SqlConnection)DataConnection, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = Convert.ToInt32(DataBase.CompanyId) });
                    cmd.Parameters.Add(new SqlParameter("@BranchID", SqlDbType.Int) { Value = branchId });
                    cmd.Parameters.Add(new SqlParameter("@FinYearID", SqlDbType.Int) { Value = finYearId });
                    cmd.Parameters.Add(new SqlParameter("@VoucherID", SqlDbType.Int) { Value = voucherId });
                    cmd.Parameters.Add(new SqlParameter("@VoucherType", SqlDbType.NVarChar, 100) { Value = voucherType });
                    cmd.Parameters.Add(new SqlParameter("@_Operation", SqlDbType.VarChar, 50) { Value = "DELETE" });

                    object result = cmd.ExecuteScalar();
                    bool success = (result != null && result.ToString().Contains("SUCCESS"));
                    System.Diagnostics.Debug.WriteLine($"POS_Vouchers stored procedure returned: {(result ?? "null")}, Success: {success}");
                    return success;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calling POS_Vouchers stored procedure: {ex.Message}");
                return false;
            }
        }

        #region GST Helper Methods

        /// <summary>
        ///  /// Calculates GSunts grouped by tax percentage from sales return details
        /// Returns a dictionary with tax percentage as key and total tax amount as value
        /// </summary>
        private Dictionary<double, double> CalculateGSTAmounts(DataGridView dgvInvoice)
        {
            Dictionary<double, double> gstAmounts = new Dictionary<double, double>();

            if (dgvInvoice == null || dgvInvoice.Rows.Count == 0)
                return gstAmounts;

            for (int i = 0; i < dgvInvoice.Rows.Count; i++)
            {
                try
                {
                    if (dgvInvoice.Rows[i].Cells["TaxPer"] == null || dgvInvoice.Rows[i].Cells["TaxPer"].Value == null)
                        continue;

                    double taxPer = dgvInvoice.Rows[i].Cells["TaxPer"].Value != null && dgvInvoice.Rows[i].Cells["TaxPer"].Value != DBNull.Value ? Convert.ToDouble(dgvInvoice.Rows[i].Cells["TaxPer"].Value) : 0.0;
                    double taxAmt = dgvInvoice.Rows[i].Cells["TaxAmt"].Value != null && dgvInvoice.Rows[i].Cells["TaxAmt"].Value != DBNull.Value ? Convert.ToDouble(dgvInvoice.Rows[i].Cells["TaxAmt"].Value) : 0.0;

                    // Only calculate for items being returned (ReturnQty > 0)
                    double returnQty = dgvInvoice.Rows[i].Cells["ReturnQty"].Value != null && dgvInvoice.Rows[i].Cells["ReturnQty"].Value != DBNull.Value ? Convert.ToDouble(dgvInvoice.Rows[i].Cells["ReturnQty"].Value) : 0.0;

                    if (taxPer > 0 && taxAmt > 0 && returnQty > 0)
                    {
                        // Calculate proportional tax for returned quantity
                        double qty = dgvInvoice.Rows[i].Cells["Qty"].Value != null && dgvInvoice.Rows[i].Cells["Qty"].Value != DBNull.Value ? Convert.ToDouble(dgvInvoice.Rows[i].Cells["Qty"].Value) : 1.0;
                        double proportionalTax = (taxAmt / qty) * returnQty;

                        if (gstAmounts.ContainsKey(taxPer))
                        {
                            gstAmounts[taxPer] += proportionalTax;
                        }
                        else
                        {
                            gstAmounts[taxPer] = proportionalTax;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calculating GST for row {i}: {ex.Message}");
                }
            }

            return gstAmounts;
        }

        /// <summary>
        /// Creates GST voucher entries (CGST and SGST) for sales return - REVERSED from sales
        /// For sales return, GST is debited (opposite of sales where it's credited)
        /// </summary>
        private void CreateGSTReturnVoucherEntries(SalesReturn sr, Voucher voucher, SqlTransaction trans,
            Dictionary<double, double> gstTaxAmounts, DateTime voucherDate, ref int slNo)
        {
            const int GST_OUTPUT_GROUP_ID = 23; // Group ID for DUTIES & TAXES (as per POS_Branch stored procedure)

            foreach (var gstEntry in gstTaxAmounts)
            {
                double taxPercentage = gstEntry.Key;
                double totalTaxAmount = gstEntry.Value;

                // Split GST into CGST and SGST (50% each)
                double cgstAmount = totalTaxAmount / 2;
                double sgstAmount = totalTaxAmount / 2;
                double cgstPercentage = taxPercentage / 2;
                double sgstPercentage = taxPercentage / 2;

                // Format percentage to match ledger names (remove trailing zeros)
                string cgstPercentageStr = cgstPercentage % 1 == 0 ? cgstPercentage.ToString("0") : cgstPercentage.ToString("0.#");
                string sgstPercentageStr = sgstPercentage % 1 == 0 ? sgstPercentage.ToString("0") : sgstPercentage.ToString("0.#");

                // Create CGST voucher entry - DEBIT (reverse of sales)
                PopulateBaseVoucherProperties(voucher, sr, voucherDate);
                voucher.GroupID = GST_OUTPUT_GROUP_ID;
                voucher.LedgerName = $"OUTPUT CGST {cgstPercentageStr}%";
                voucher.LedgerID = GetOrCreateGSTLedger(voucher.LedgerName, GST_OUTPUT_GROUP_ID, trans);
                voucher.Debit = Math.Round(cgstAmount, 2);  // DEBIT for return (opposite of sales)
                voucher.Credit = 0;
                voucher.SlNo = slNo++;

                CreateVoucherEntry(voucher, trans, $"VoucherID={voucher.VoucherID}, Type=DEBIT, Account={voucher.LedgerName}, Amount={cgstAmount}");

                // Create SGST voucher entry - DEBIT (reverse of sales)
                PopulateBaseVoucherProperties(voucher, sr, voucherDate);
                voucher.GroupID = GST_OUTPUT_GROUP_ID;
                voucher.LedgerName = $"OUTPUT SGST {sgstPercentageStr}%";
                voucher.LedgerID = GetOrCreateGSTLedger(voucher.LedgerName, GST_OUTPUT_GROUP_ID, trans);
                voucher.Debit = Math.Round(sgstAmount, 2);  // DEBIT for return (opposite of sales)
                voucher.Credit = 0;
                voucher.SlNo = slNo++;

                CreateVoucherEntry(voucher, trans, $"VoucherID={voucher.VoucherID}, Type=DEBIT, Account={voucher.LedgerName}, Amount={sgstAmount}");

                System.Diagnostics.Debug.WriteLine($"Created GST return entries for {taxPercentage}% tax: CGST={cgstAmount}, SGST={sgstAmount}");
            }
        }

        /// <summary>
        /// Gets or creates a GST ledger account
        /// </summary>
        private long GetOrCreateGSTLedger(string ledgerName, int groupId, SqlTransaction trans)
        {
            try
            {
                // Try to get existing ledger
                long ledgerId = ledgerRepository.GetLedgerId(ledgerName, groupId, Convert.ToInt32(DataBase.BranchId));

                if (ledgerId > 0)
                {
                    return ledgerId;
                }

                // If ledger doesn't exist, create it automatically
                System.Diagnostics.Debug.WriteLine($"GST Ledger '{ledgerName}' not found, creating it now...");

                try
                {
                    int nextLedgerId = 1;
                    using (SqlCommand idCmd = new SqlCommand("SELECT ISNULL(MAX(LedgerID), 0) + 1 FROM LedgerMaster", (SqlConnection)DataConnection, trans))
                    {
                        nextLedgerId = Convert.ToInt32(idCmd.ExecuteScalar());
                    }

                    // Create the GST ledger using stored procedure
                    using (SqlCommand cmd = new SqlCommand("POS_Ledger", (SqlConnection)DataConnection, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@_Operation", "CREATE");
                        cmd.Parameters.AddWithValue("@CompanyID", SessionContext.CompanyId);
                        cmd.Parameters.AddWithValue("@BranchID", SessionContext.BranchId);
                        cmd.Parameters.AddWithValue("@LedgerID", nextLedgerId);
                        cmd.Parameters.AddWithValue("@LedgerName", ledgerName);
                        cmd.Parameters.AddWithValue("@Alias", ledgerName);
                        cmd.Parameters.AddWithValue("@Description", $"GST Output Ledger - {ledgerName}");
                        cmd.Parameters.AddWithValue("@Notes", "Auto-created for GST transactions");
                        cmd.Parameters.AddWithValue("@GroupID", groupId);
                        cmd.Parameters.AddWithValue("@OpnDebit", 0);
                        cmd.Parameters.AddWithValue("@OpnCredit", 0);
                        cmd.Parameters.AddWithValue("@ProvideBankDetails", false);
                        cmd.Parameters.AddWithValue("@GstApplicable", true);
                        cmd.Parameters.AddWithValue("@VatApplicable", false);
                        cmd.Parameters.AddWithValue("@InventoryValuesAffected", false);
                        cmd.Parameters.AddWithValue("@MaintainBillWiseDetails", false);
                        cmd.Parameters.AddWithValue("@PriceLevelApplicable", false);

                        cmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Created GST Ledger '{ledgerName}' successfully");
                    }

                    // Now try to get the newly created ledger ID
                    ledgerId = ledgerRepository.GetLedgerId(ledgerName, groupId, Convert.ToInt32(DataBase.BranchId));
                    if (ledgerId > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Got new LedgerId={ledgerId} for '{ledgerName}'");
                        return ledgerId;
                    }
                }
                catch (Exception createEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating GST ledger '{ledgerName}': {createEx.Message}");
                }

                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting/creating GST ledger '{ledgerName}': {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Populates common voucher properties for sales return
        /// </summary>
        private void PopulateBaseVoucherProperties(Voucher voucher, SalesReturn sr, DateTime voucherDate)
        {
            voucher.CompanyID = Convert.ToInt32(DataBase.CompanyId);
            voucher.BranchID = Convert.ToInt32(DataBase.BranchId);
            voucher.VoucherID = sr.VoucherID;
            voucher.VoucherSeriesID = sr.SeriesId;
            voucher.VoucherDate = voucherDate;
            voucher.VoucherNumber = "SR" + sr.SReturnNo.ToString();
            voucher.VoucherType = "Credit Note";
            voucher.Narration = $"SALES RETURN: #{sr.SReturnNo}| RETURN WORTH:{sr.GrandTotal}| INVOICE: {sr.InvoiceNo}";
            voucher.Mode = "";
            voucher.ModeID = 0;
            voucher.UserDate = voucherDate;
            voucher.UserName = DataBase.UserName;
            voucher.UserID = Convert.ToInt32(DataBase.UserId);
            voucher.FinYearID = sr.FinYearId;
            voucher.IsSyncd = false;
            voucher._Operation = "CREATE";
        }

        /// <summary>
        /// Creates a voucher entry in the database
        /// </summary>
        private void CreateVoucherEntry(Voucher voucher, SqlTransaction trans, string entryDescription)
        {
            System.Diagnostics.Debug.WriteLine($"Creating voucher entry: {entryDescription}");

            using (SqlCommand voucherEntryCmd = new SqlCommand(STOREDPROCEDURE.POS_Vouchers, (SqlConnection)DataConnection, trans))
            {
                voucherEntryCmd.CommandType = CommandType.StoredProcedure;
                voucherEntryCmd.Parameters.AddWithValue("@CompanyID", voucher.CompanyID);
                voucherEntryCmd.Parameters.AddWithValue("@BranchID", voucher.BranchID);
                voucherEntryCmd.Parameters.AddWithValue("@VoucherID", voucher.VoucherID);
                voucherEntryCmd.Parameters.AddWithValue("@VoucherSeriesID", voucher.VoucherSeriesID);
                voucherEntryCmd.Parameters.AddWithValue("@VoucherDate", voucher.VoucherDate);
                voucherEntryCmd.Parameters.AddWithValue("@VoucherNumber", voucher.VoucherNumber);
                voucherEntryCmd.Parameters.AddWithValue("GroupID", voucher.GroupID);
                voucherEntryCmd.Parameters.AddWithValue("LedgerID", voucher.LedgerID);
                voucherEntryCmd.Parameters.AddWithValue("LedgerName", voucher.LedgerName);
                voucherEntryCmd.Parameters.AddWithValue("VoucherType", voucher.VoucherType);
                voucherEntryCmd.Parameters.AddWithValue("Credit", voucher.Credit);
                voucherEntryCmd.Parameters.AddWithValue("Debit", voucher.Debit);
                voucherEntryCmd.Parameters.AddWithValue("Narration", voucher.Narration);
                voucherEntryCmd.Parameters.AddWithValue("SlNo", voucher.SlNo);
                voucherEntryCmd.Parameters.AddWithValue("Mode", voucher.Mode);
                voucherEntryCmd.Parameters.AddWithValue("ModeID", voucher.ModeID);
                voucherEntryCmd.Parameters.AddWithValue("UserID", voucher.UserID);
                voucherEntryCmd.Parameters.AddWithValue("UserName", voucher.UserName);
                voucherEntryCmd.Parameters.AddWithValue("FinYearID", voucher.FinYearID);
                voucherEntryCmd.Parameters.AddWithValue("@_Operation", voucher._Operation);
                voucherEntryCmd.Parameters.AddWithValue("UserDate", voucher.UserDate);

                voucherEntryCmd.ExecuteNonQuery();
            }

            System.Diagnostics.Debug.WriteLine($"Voucher entry created successfully.");
        }

        #endregion
    }

}
