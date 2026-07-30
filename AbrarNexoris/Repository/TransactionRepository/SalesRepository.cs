using Dapper;
using ModelClass;
using ModelClass.TransactionModels;
using Repository.MasterRepositry;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace Repository.TransactionRepository
{

    public class SalesRepository : BaseRepostitory
    {
        public double Netamount;
        string BillNo;

        // Constants for hardcoded values
        private const string VOUCHER_TYPE_SALES = "Sales";
        private const string OPERATION_GENERATE_NUMBER = "GENERATENUMBER";
        private const string OPERATION_CREATE = "CREATE";
        private const string OPERATION_UPDATE = "UPDATE";
        private const string OPERATION_COMPLETE = "COMPLETE";
        private const string OPERATION_DELETE = "DELETE";
        private const string OPERATION_GETALL = "GETALL";
        private const string OPERATION_GETBYID = "GETBYID";
        private const string OPERATION_PRINT = "Print";
        private const string OPERATION_SOLDITEMHISTORY = "SOLDITEMHISTORY";
        #region SaveSales
        // ============================================================================
        // STOCK MANAGEMENT FLOW (Fixed 31-Dec-2025)
        // ============================================================================
        // CREATE (New Sale):
        //   - _POS_SDetails_Win: Only inserts SDetails record (NO stock update)
        //   - _POS_SalesInvoice_PriceSettings: Deducts stock using (Qty * Packing) formula
        //     Updates PriceSettings.Stock WHERE IsBaseUnit = 'Y'
        //
        // DELETE/UPDATE (Edit or Cancel Sale):
        //   - _POS_SDetails_Win DELETE_BY_BILLNO: Restores stock using (Qty * Packing) formula
        //     Then deletes SDetails records
        //
        // This separation prevents the DOUBLE DEDUCTION bug where stock was reduced twice.
        // ============================================================================
        LedgerRepository ledgerRepository = new LedgerRepository();
        UnitMasterRepository unitMasterRepository = new UnitMasterRepository();

        /// <summary>
        /// Set this BEFORE calling SaveSales/UpdateSales when the customer paid via
        /// FrmSalesCmpt (BankTransfer, UPI, Card, split etc.).
        /// The repository uses this list to post correct per-instrument debit voucher entries.
        /// It is cleared automatically after each save.
        /// </summary>
        public List<SalesPaymentDetail> PendingPaymentDetails { get; set; } = new List<SalesPaymentDetail>();
        public string SaveSales(SalesMaster sales, SalesDetails salesDetails, DataGridView dgvItems)
        {
            ModelClass.TransactionModels.Voucher voucher = new ModelClass.TransactionModels.Voucher();
            DataConnection.Open();
            // Set transaction isolation level to Serializable to prevent race conditions
            var trans = ((SqlConnection)DataConnection).BeginTransaction(IsolationLevel.Serializable);
            try
            {
                // Ensure FinYearId is set from session context
                sales.FinYearId = SessionContext.FinYearId;
                Netamount = sales.NetAmount;
                // Ensure FinYearId is not zero (should not happen but good for safety)
                if (sales.FinYearId == 0)
                {
                    throw new Exception("Cannot save invoice. Financial Year is unexpectedly 0 after hardcoding.");
                }

                NormalizePaymentModeForPersistence(sales);

                //here for getting the last bill no and assing the bill no to the model property
                sales._Operation = OPERATION_GENERATE_NUMBER;

                try
                {
                    List<SalesMaster> getBillNO = DataConnection.Query<SalesMaster>(STOREDPROCEDURE._POS_Sales_Win, sales, trans,
                    commandType: CommandType.StoredProcedure).ToList<SalesMaster>();

                    if (getBillNO.Count > 0)
                    {
                        foreach (SalesMaster master in getBillNO)
                        {
                            sales.BillNo = master.BillNo;
                            BillNo = master.BillNo.ToString();
                        }
                    }
                    else
                    {
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }

                //HERE Genarating Number for Vocuhers - USING THE APPROACH FROM PURCHASE REPOSITORY
                voucher._Operation = "GENERATENUMBER";
                voucher.BranchID = SessionContext.BranchId;
                voucher.CompanyID = SessionContext.CompanyId;
                voucher.VoucherType = VOUCHER_TYPE_SALES;
                voucher.FinYearID = SessionContext.FinYearId;
                List<Voucher> VouchersList = DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, voucher, trans,
                    commandType: CommandType.StoredProcedure).ToList<Voucher>();

                if (VouchersList.Count > 0)
                {
                    foreach (Voucher objVoch in VouchersList)
                    {
                        voucher.VoucherID = objVoch.VoucherID;
                        sales.VoucherID = Convert.ToInt32(objVoch.VoucherID);
                    }
                }
                else
                {
                    throw new Exception("Failed to generate VoucherID. No result returned from database.");
                }

                // Set the operation for creating a new sales record
                sales._Operation = OPERATION_CREATE;
                // VALIDATION: Ensure FinYearId and VoucherID are not zero before proceeding
                if (sales.FinYearId <= 0)
                {
                    throw new Exception("Cannot save invoice. FinYearId is 0, which would cause a primary key violation.");
                }

                if (sales.VoucherID <= 0)
                {
                    throw new Exception("Cannot save invoice. VoucherID is 0, which would cause a primary key violation.");
                }

                // Calculate BillCost from sales details grid (Cost × Qty for all items)
                double billCost = 0;
                for (int i = 0; i < dgvItems.Rows.Count; i++)
                {
                    if (dgvItems.Rows[i].Cells["Cost"] != null && dgvItems.Rows[i].Cells["Cost"].Value != null &&
                        dgvItems.Rows[i].Cells["Qty"] != null && dgvItems.Rows[i].Cells["Qty"].Value != null)
                    {
                        float cost = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Cost"], 0f);
                        float qty = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Qty"], 0f);
                        billCost += (cost * qty);
                    }
                }
                sales.BillCost = billCost;
                // Now directly insert the sales master record using SQL Command
                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_Sales_Win, (SqlConnection)DataConnection, (SqlTransaction)trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Add all the required parameters
                        cmd.Parameters.AddWithValue("CompanyId", sales.CompanyId);
                        cmd.Parameters.AddWithValue("BranchId", sales.BranchId);
                        cmd.Parameters.AddWithValue("FinYearId", sales.FinYearId);
                        cmd.Parameters.AddWithValue("BillNo", sales.BillNo);
                        cmd.Parameters.AddWithValue("BillDate", sales.BillDate);
                        cmd.Parameters.AddWithValue("CustomerName", string.IsNullOrEmpty(sales.CustomerName) ? DBNull.Value : (object)sales.CustomerName);
                        cmd.Parameters.AddWithValue("LedgerID", sales.LedgerID);
                        cmd.Parameters.AddWithValue("EmpID", sales.EmpID);
                        cmd.Parameters.AddWithValue("VoucherID", sales.VoucherID);
                        cmd.Parameters.AddWithValue("StateId", sales.StateId);
                        cmd.Parameters.AddWithValue("PaymodeId", sales.PaymodeId);
                        cmd.Parameters.AddWithValue("PaymodeName", string.IsNullOrEmpty(sales.PaymodeName) ? DBNull.Value : (object)sales.PaymodeName);

                        // Only add CreditDays parameter if it's a credit sale
                        if (IsCreditPaymentMode(sales.PaymodeId))
                        {
                            // Check if the stored procedure has a CreditDays parameter
                            try
                            {
                                cmd.Parameters.AddWithValue("CreditDays", sales.CreditDays);
                            }
                            catch (Exception ex)
                            {
                                // Continue without adding the parameter
                            }
                        }

                        // Add PaymentReference parameter
                        cmd.Parameters.AddWithValue("PaymentReference", string.IsNullOrEmpty(sales.PaymentReference) ? DBNull.Value : (object)sales.PaymentReference);

                        cmd.Parameters.AddWithValue("SubTotal", sales.SubTotal);
                        cmd.Parameters.AddWithValue("NetAmount", sales.NetAmount);
                        cmd.Parameters.AddWithValue("Status", string.IsNullOrEmpty(sales.Status) ? DBNull.Value : (object)sales.Status);
                        cmd.Parameters.AddWithValue("SavedVia", string.IsNullOrEmpty(sales.SavedVia) ? DBNull.Value : (object)sales.SavedVia);
                        cmd.Parameters.AddWithValue("UserId", sales.UserId);
                        cmd.Parameters.AddWithValue("DueDate", sales.DueDate);
                        cmd.Parameters.AddWithValue("_Operation", sales._Operation);

                        // Set default values for required parameters that aren't in our C# model
                        cmd.Parameters.AddWithValue("CounterId", SessionContext.CounterId);
                        cmd.Parameters.AddWithValue("CounterSessionId", SessionContext.CounterSessionId);
                        cmd.Parameters.AddWithValue("Freight", 0);
                        cmd.Parameters.AddWithValue("FreightProfit", 0);
                        cmd.Parameters.AddWithValue("PaymodeLedgerId", sales.PaymodeLedgerId);
                        cmd.Parameters.AddWithValue("TaxPer", sales.TaxPer);
                        cmd.Parameters.AddWithValue("TaxAmt", sales.TaxAmt);
                        cmd.Parameters.AddWithValue("CessPer", 0);
                        cmd.Parameters.AddWithValue("CessAmt", 0);
                        cmd.Parameters.AddWithValue("KFCessPer", 0);
                        cmd.Parameters.AddWithValue("KFCessAmt", 0);
                        cmd.Parameters.AddWithValue("DiscountPer", sales.DiscountPer);
                        cmd.Parameters.AddWithValue("DiscountAmt", sales.DiscountAmt);
                        cmd.Parameters.AddWithValue("RoundOffFlag", sales.RoundOffFlag);
                        cmd.Parameters.AddWithValue("RoundOff", sales.RoundOff);
                        cmd.Parameters.AddWithValue("TenderedAmount", sales.TenderedAmount);
                        cmd.Parameters.AddWithValue("Balance", sales.Balance);
                        cmd.Parameters.AddWithValue("CurrencyId", sales.CurrencyId);
                        cmd.Parameters.AddWithValue("CancelFlag", sales.CancelFlag);
                        cmd.Parameters.AddWithValue("BillCost", sales.BillCost); // Now contains calculated cost
                        cmd.Parameters.AddWithValue("OrderNo", sales.OrderNo > 0 ? (object)sales.OrderNo : DBNull.Value);
                        cmd.Parameters.AddWithValue("VoucherType", "Sales");
                        cmd.Parameters.AddWithValue("TransporterLedgerId", DBNull.Value);
                        cmd.Parameters.AddWithValue("ReceivedAmount", sales.ReceivedAmount);

                        // Add missing parameters for IsPaid and IsSyncd
                        cmd.Parameters.AddWithValue("IsPaid", sales.IsPaid);
                        cmd.Parameters.AddWithValue("IsSyncd", sales.IsSyncd);

                        // Execute and check result
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataSet ds = new DataSet();
                            da.Fill(ds);

                            if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0].Rows.Count > 0))
                            {
                                BillNo = sales.BillNo.ToString(); // Set the BillNo field for return value
                            }
                            else
                            {
                                throw new Exception("Failed to save sales record. No results returned from database.");
                            }
                        }
                    }
                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.InnerException != null)
                    {
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                    }
                    throw;
                }

                //this section save the sales details details 
                salesDetails._Operation = "CREATE";

                // CRITICAL FIX: Explicitly set FinYearId in salesDetails to match sales.FinYearId
                salesDetails.FinYearId = sales.FinYearId;
                // Set common details for all items 
                salesDetails.BillNo = sales.BillNo; // This is already Int64 in both models
                                                    // Note: Don't set VoucherId here, it will be set for each item

                // First check if dgvItems is valid
                if (dgvItems == null || dgvItems.Rows.Count == 0)
                {
                    throw new Exception("No items to save in sales invoice");
                }

                // Proceed with saving details
                for (int i = 0; i < dgvItems.RowCount; i++)
                {
                    try
                    {
                        if (dgvItems.Rows[i].Cells["ItemId"] == null || dgvItems.Rows[i].Cells["ItemId"].Value == null)
                            continue;

                        salesDetails.ItemId = GridParseHelpers.GetIntValue(dgvItems.Rows[i].Cells["ItemId"], 0);
                        salesDetails.Barcode = GridParseHelpers.GetStringValue(dgvItems.Rows[i].Cells["BarCode"], "Unknown");
                        salesDetails.ItemName = GridParseHelpers.GetStringValue(dgvItems.Rows[i].Cells["ItemName"], "Unknown Item");

                        if (dgvItems.Columns.Contains("SlNO") && dgvItems.Rows[i].Cells["SlNO"].Value != null)
                            salesDetails.SlNO = GridParseHelpers.GetIntValue(dgvItems.Rows[i].Cells["SlNO"], i + 1);
                        else if (dgvItems.Columns.Contains("SlNo") && dgvItems.Rows[i].Cells["SlNo"].Value != null) // Legacy check
                            salesDetails.SlNO = GridParseHelpers.GetIntValue(dgvItems.Rows[i].Cells["SlNo"], i + 1);
                        else
                            salesDetails.SlNO = i + 1;

                        // FIXED: Ensure UnitId is correctly set - first try to get from grid, then fallback to lookup
                        int unitId = GridParseHelpers.GetIntValue(dgvItems.Rows[i].Cells["UnitId"], 0);
                        if (unitId <= 0)
                        {
                            // Try to get UnitId from database based on Unit name
                            string unitName = GridParseHelpers.GetStringValue(dgvItems.Rows[i].Cells["Unit"], "");
                            if (!string.IsNullOrEmpty(unitName))
                            {
                                try
                                {
                                    // Use the UnitMasterRepository to get the UnitId by name using stored procedure
                                    unitId = unitMasterRepository.GetUnitIdByName(unitName, trans);
                                    if (unitId > 0)
                                    {
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Continue with default UnitId
                                }
                            }
                        }

                        salesDetails.UnitId = unitId > 0 ? unitId : 1; // Use found ID or default to 1
                        salesDetails.Unit = GridParseHelpers.GetStringValue(dgvItems.Rows[i].Cells["Unit"], "");
                        // FIX: Save actual selling price (S/Price column = Amount in grid) to UnitPrice, not original retail price
                        salesDetails.UnitPrice = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Amount"], 0f);
                        salesDetails.Cost = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Cost"], 0f);
                        salesDetails.DiscountAmount = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["DiscAmt"], 0f);
                        salesDetails.DiscountPer = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["DiscPer"], 0f);
                        salesDetails.MarginAmt = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["MarginAmt"], 0f);
                        salesDetails.MarginPer = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Marginper"], 0f);

                        // Get quantity as float to preserve decimal values
                        float qtyFloat = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Qty"], 0f);
                        // Store the integer value in the model property
                        salesDetails.Qty = Convert.ToInt32(qtyFloat);
                        // FIX: Amount should be line total (Qty × Unit Selling Price) for receipt printing, not unit price
                        // The Amount field in grid stores unit selling price, but for receipt it should be line total
                        float unitSellingPrice = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Amount"], 0f); // This is unit selling price from grid
                        // Calculate Amount as Qty × Unit Selling Price for receipt display
                        salesDetails.Amount = qtyFloat * unitSellingPrice;
                        salesDetails.TotalAmount = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["TotalAmount"], 0f); // Use actual TotalAmount from grid

                        // Get tax values from grid
                        salesDetails.TaxPer = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["TaxPer"], 0f);
                        salesDetails.TaxAmt = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["TaxAmt"], 0f);
                        salesDetails.TaxType = GridParseHelpers.GetStringValue(dgvItems.Rows[i].Cells["TaxType"], "incl");

                        // Get BaseAmount (taxable value) from grid for GST compliance
                        float baseAmount = 0f;
                        if (dgvItems.Columns.Contains("BaseAmount") && dgvItems.Rows[i].Cells["BaseAmount"].Value != null)
                        {
                            baseAmount = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["BaseAmount"], 0f);
                        }
                        else
                        {
                            // Calculate BaseAmount if not in grid
                            string taxType = salesDetails.TaxType?.ToLower() ?? "incl";
                            if (taxType == "incl" && salesDetails.TaxPer > 0)
                            {
                                // For inclusive: BaseAmount = Amount / (1 + TaxPer/100)
                                double divisor = 1.0 + (salesDetails.TaxPer / 100.0);
                                baseAmount = (float)Math.Round(salesDetails.Amount / divisor, 2);
                            }
                            else
                            {
                                // For exclusive: BaseAmount = Amount (before tax)
                                baseAmount = (float)salesDetails.Amount;
                            }
                        }

                        // Common properties from salesMaster or defaults
                        salesDetails.CompanyId = SessionContext.CompanyId;
                        salesDetails.BranchID = SessionContext.BranchId;
                        // Use FinYearId from SessionContext
                        salesDetails.FinYearId = SessionContext.FinYearId;
                        salesDetails.BillDate = DateTime.Now;
                        salesDetails.Expiry = DateTime.Now;
                        // Read MRP from grid - use UnitPrice (selling price) if MRP column not available
                        salesDetails.MRP = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["MRP"],
                            GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["UnitPrice"], 0f));
                        // Read Packing from grid if available
                        salesDetails.Packing = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Packing"], 1f);
                        salesDetails.BaseUnit = "";
                        salesDetails.CounterId = SessionContext.CounterId;

                        if (sales.VoucherID > 0)
                        {
                            salesDetails.VoucherId = Convert.ToInt32(sales.VoucherID);
                        }
                        else
                        {
                            salesDetails.VoucherId = 0; // Or handle as error
                        }

                        var detailParams = new DynamicParameters();

                        // Add all necessary parameters
                        detailParams.Add("CompanyId", salesDetails.CompanyId);
                        detailParams.Add("BranchID", salesDetails.BranchID);
                        // Use FinYearId from SessionContext
                        detailParams.Add("FinYearId", SessionContext.FinYearId);
                        detailParams.Add("CounterId", salesDetails.CounterId);
                        detailParams.Add("BillNo", salesDetails.BillNo);
                        detailParams.Add("BillDate", salesDetails.BillDate);
                        detailParams.Add("SlNO", salesDetails.SlNO);
                        detailParams.Add("ItemId", salesDetails.ItemId);
                        detailParams.Add("ItemName", salesDetails.ItemName);
                        detailParams.Add("UnitId", salesDetails.UnitId);
                        detailParams.Add("Unit", salesDetails.Unit);
                        detailParams.Add("Expiry", salesDetails.Expiry);
                        detailParams.Add("Qty", qtyFloat); // Use the original float value instead of the truncated integer
                        detailParams.Add("Packing", salesDetails.Packing);
                        detailParams.Add("UnitPrice", salesDetails.UnitPrice);
                        detailParams.Add("Amount", salesDetails.Amount);
                        detailParams.Add("DiscountPer", salesDetails.DiscountPer);
                        detailParams.Add("DiscountAmount", salesDetails.DiscountAmount);
                        detailParams.Add("MarginPer", salesDetails.MarginPer);
                        detailParams.Add("MarginAmt", salesDetails.MarginAmt);
                        detailParams.Add("TaxPer", salesDetails.TaxPer);
                        detailParams.Add("TaxAmt", salesDetails.TaxAmt);
                        detailParams.Add("TaxType", salesDetails.TaxType);
                        detailParams.Add("BaseAmount", baseAmount); // Add BaseAmount for GST compliance
                        detailParams.Add("TotalAmount", salesDetails.TotalAmount);
                        detailParams.Add("Cost", salesDetails.Cost);
                        detailParams.Add("BaseUnit", salesDetails.BaseUnit);
                        detailParams.Add("MRP", salesDetails.MRP);
                        detailParams.Add("Barcode", salesDetails.Barcode);
                        detailParams.Add("_Operation", salesDetails._Operation);
                        try
                        {
                            List<SalesDetails> ListSalesDetails = DataConnection.Query<SalesDetails>(
                                STOREDPROCEDURE._POS_SDetails_Win,
                                detailParams,
                                trans,
                                commandType: CommandType.StoredProcedure
                            ).ToList<SalesDetails>();
                            // CRITICAL FIX: Call _POS_SalesInvoice_PriceSettings to update base unit stock
                            try
                            {
                                var priceSettingsParams = new DynamicParameters();
                                priceSettingsParams.Add("CompanyId", salesDetails.CompanyId);
                                priceSettingsParams.Add("BranchId", salesDetails.BranchID);
                                priceSettingsParams.Add("FinYearId", salesDetails.FinYearId);
                                priceSettingsParams.Add("ItemId", salesDetails.ItemId);
                                priceSettingsParams.Add("UnitId", salesDetails.UnitId);
                                priceSettingsParams.Add("Qty", qtyFloat); // Use original float quantity
                                priceSettingsParams.Add("Packing", salesDetails.Packing);
                                priceSettingsParams.Add("BillNo", salesDetails.BillNo);
                                priceSettingsParams.Add("_Operation", "CREATE");

                                List<object> priceSettingsResult = DataConnection.Query<object>(
                                    STOREDPROCEDURE._POS_SalesInvoice_PriceSettings,
                                    priceSettingsParams,
                                    trans,
                                    commandType: CommandType.StoredProcedure
                                ).ToList<object>();
                            }
                            catch (Exception priceEx)
                            {
                                // Don't throw here - the main detail record was saved successfully
                                // Base unit stock update failure shouldn't prevent the sale from completing
                            }
                        }
                        catch (Exception detailEx)
                        {
                            if (detailEx.InnerException != null)
                            {
                            }
                            throw;
                            // NOTE: Stock adjustment is handled automatically by _POS_SDetails_Win CREATE operation
                            // The stored procedure updates both ItemBatch.Qty and PriceSettings.Stock
                        }
                    }
                    catch (Exception)
                    {
                        // Re-throw to ensure full transaction rollback - invoice should be complete or not saved
                        throw;
                    }
                }

                //Here Account Sections - Create voucher entries
                voucher._Operation = "CREATE";

                // Calculate GST amounts from sales details
                Dictionary<double, double> gstTaxAmounts = CalculateGSTAmounts(dgvItems);

                if (IsCashPaymentMode(sales.PaymodeId)) //CASH
                {
                    CreateCashSaleVoucherEntries(sales, voucher, trans, gstTaxAmounts);
                }
                else //CREDIT
                {
                    CreateCreditSaleVoucherEntries(sales, voucher, trans, gstTaxAmounts);
                }

                // SPLIT PAYMENT: Save payment details if provided
                // This is called from FrmSalesCmpt after user enters payment information
                if (sales.PaymentReference != null && sales.PaymentReference.StartsWith("SPLIT_PAYMENT:"))
                {
                    // Payment details will be saved separately by the calling code
                    // The PaymentReference contains a marker to indicate split payment was used
                }
                trans.Commit();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                }
                trans.Rollback();
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return BillNo; //"Success";
        }
        #endregion

        #region UpdateSales
        public string UpdateSales(SalesMaster sales, SalesDetails salesDetails, DataGridView dgvItems)
        {
            ModelClass.TransactionModels.Voucher voucher = new ModelClass.TransactionModels.Voucher();
            DataConnection.Open();
            // Set transaction isolation level to Serializable to prevent race conditions
            var trans = ((SqlConnection)DataConnection).BeginTransaction(IsolationLevel.Serializable);
            try
            {
                // Check if this bill is CURRENTLY a Hold bill in the database
                // This determines whether stock was reduced or not when the bill was created
                // If current status = 'Hold', stock was NOT reduced (so we shouldn't restore it)
                // If current status = 'Complete', stock WAS reduced (so we should restore it before re-reducing)
                bool isCurrentlyHoldBill = false;
                
                // Get original FinYearId if updating, otherwise use session context
                int originalFinYearId = sales.FinYearId;
                if (originalFinYearId <= 0)
                {
                    originalFinYearId = SessionContext.FinYearId;
                    sales.FinYearId = originalFinYearId;
                }
                Netamount = sales.NetAmount;
                // Ensure FinYearId is not zero
                if (sales.FinYearId <= 0)
                {
                    throw new Exception("Cannot update invoice. Financial Year is unexpectedly 0 after hardcoding.");
                }

                NormalizePaymentModeForPersistence(sales);
                try
                {
                    using (SqlCommand statusCmd = new SqlCommand(STOREDPROCEDURE._POS_Sales_Win, (SqlConnection)DataConnection, (SqlTransaction)trans))
                    {
                        statusCmd.CommandType = CommandType.StoredProcedure;
                        statusCmd.Parameters.AddWithValue("@BillNo", sales.BillNo);
                        statusCmd.Parameters.AddWithValue("@BranchId", sales.BranchId);
                        statusCmd.Parameters.AddWithValue("@CompanyId", sales.CompanyId);
                        statusCmd.Parameters.AddWithValue("@FinYearId", sales.FinYearId);
                        statusCmd.Parameters.AddWithValue("@_Operation", "GETSTATUS");

                        using (SqlDataReader reader = statusCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string currentStatus = reader["Status"]?.ToString() ?? "";
                                isCurrentlyHoldBill = (currentStatus == "Hold");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }

                // Check if we need to preserve the VoucherID
                // ALWAYS preserve VoucherID if it exists, regardless of status
                bool preserveVoucherID = (sales.VoucherID > 0);

                if (preserveVoucherID)
                {
                    // Preserve the existing VoucherID
                    int existingVoucherID = (int)sales.VoucherID;
                    voucher.VoucherID = existingVoucherID;
                }
                else
                {
                    // Generate a fresh VoucherID only if we don't have one
                    voucher._Operation = "GENERATENUMBER";
                    voucher.BranchID = SessionContext.BranchId;
                    voucher.CompanyID = SessionContext.CompanyId;
                    voucher.VoucherType = VOUCHER_TYPE_SALES;
                    voucher.FinYearID = SessionContext.FinYearId;
                    List<Voucher> VouchersList = DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, voucher, trans,
                        commandType: CommandType.StoredProcedure).ToList<Voucher>();

                    if (VouchersList.Count > 0)
                    {
                        foreach (Voucher objVoch in VouchersList)
                        {
                            voucher.VoucherID = objVoch.VoucherID;
                            sales.VoucherID = Convert.ToInt32(objVoch.VoucherID);
                        }
                    }
                    else
                    {
                        throw new Exception("Failed to generate VoucherID for updating invoice.");
                    }
                }



                // Set the operation to UPDATE
                sales._Operation = "UPDATE";
                // VALIDATION: Ensure FinYearId and VoucherID are not zero before proceeding
                if (sales.FinYearId <= 0)
                {
                    throw new Exception("Cannot update invoice. FinYearId is 0, which would cause a primary key violation.");
                }

                if (sales.VoucherID <= 0)
                {
                    throw new Exception("Cannot update invoice. VoucherID is 0, which would cause a primary key violation.");
                }

                // Calculate BillCost from sales details grid (Cost × Qty for all items)
                double billCost = 0;
                for (int i = 0; i < dgvItems.Rows.Count; i++)
                {
                    if (dgvItems.Rows[i].Cells["Cost"] != null && dgvItems.Rows[i].Cells["Cost"].Value != null &&
                        dgvItems.Rows[i].Cells["Qty"] != null && dgvItems.Rows[i].Cells["Qty"].Value != null)
                    {
                        float cost = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Cost"], 0f);
                        float qty = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Qty"], 0f);
                        billCost += (cost * qty);
                    }
                }
                sales.BillCost = billCost;
                // Delete all existing sales details
                // IMPORTANT: Check if bill is CURRENTLY a Hold bill in the database
                // If CURRENTLY Hold: stock was NOT reduced when held, so just delete records (no restore)
                // If CURRENTLY Complete: stock WAS reduced, so restore stock first via DELETE_BY_BILLNO
                try
                {
                    if (isCurrentlyHoldBill)
                    {
                        // HOLD BILL BEING COMPLETED: Stock was never reduced, so just delete records without restoring stock
                        // Then the new CREATE operation will reduce stock (correct behavior)
                        using (SqlCommand deleteCmd = new SqlCommand(STOREDPROCEDURE._POS_SDetails_Win_Hold, (SqlConnection)DataConnection, (SqlTransaction)trans))
                        {
                            deleteCmd.CommandType = CommandType.StoredProcedure;
                            deleteCmd.Parameters.AddWithValue("@BillNo", sales.BillNo);
                            deleteCmd.Parameters.AddWithValue("@BranchID", sales.BranchId);
                            deleteCmd.Parameters.AddWithValue("@CompanyId", sales.CompanyId);
                            deleteCmd.Parameters.AddWithValue("@FinYearId", sales.FinYearId);
                            deleteCmd.Parameters.AddWithValue("@_Operation", "DELETE_HOLD");

                            using (SqlDataReader reader = deleteCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    string result = reader["Result"]?.ToString() ?? "";
                                    string message = reader["Message"]?.ToString() ?? "";
                                }
                            }
                        }
                    }
                    else
                    {
                        // COMPLETED SALE UPDATE: Stock was reduced, so use DELETE_BY_BILLNO to restore stock first
                        
                        // Restore base unit stock for old details is handled inside _POS_SDetails_Win 'DELETE_BY_BILLNO' operation
                        // as per the updated stored procedure logic.

                        using (SqlCommand deleteCmd = new SqlCommand(STOREDPROCEDURE._POS_SDetails_Win, (SqlConnection)DataConnection, (SqlTransaction)trans))
                        {
                            deleteCmd.CommandType = CommandType.StoredProcedure;
                            deleteCmd.Parameters.AddWithValue("@BillNo", sales.BillNo);
                            deleteCmd.Parameters.AddWithValue("@BranchID", sales.BranchId);
                            deleteCmd.Parameters.AddWithValue("@CompanyId", sales.CompanyId);
                            deleteCmd.Parameters.AddWithValue("@FinYearId", sales.FinYearId);
                            deleteCmd.Parameters.AddWithValue("@_Operation", "DELETE_BY_BILLNO");

                            using (SqlDataReader reader = deleteCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    string result = reader["Result"].ToString();
                                    string message = reader["Message"].ToString();
                                }
                            }
                        }
                    }

                    // CLEANUP SPLIT PAYMENTS
                    // Ensure orphaned payment details are deleted during update
                    try
                    {
                        DeletePaymentDetailsByBillNo(sales.BillNo, sales.CompanyId, sales.BranchId, sales.FinYearId, trans);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to delete payment details during update: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                    }
                    throw;
                }

                // Now update the sales master record using SQL Command
                try
                {
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_Sales_Win, (SqlConnection)DataConnection, (SqlTransaction)trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Add all the required parameters
                        cmd.Parameters.AddWithValue("CompanyId", sales.CompanyId);
                        cmd.Parameters.AddWithValue("BranchId", sales.BranchId);
                        cmd.Parameters.AddWithValue("FinYearId", sales.FinYearId);
                        cmd.Parameters.AddWithValue("BillNo", sales.BillNo);
                        cmd.Parameters.AddWithValue("BillDate", sales.BillDate);
                        cmd.Parameters.AddWithValue("CustomerName", string.IsNullOrEmpty(sales.CustomerName) ? DBNull.Value : (object)sales.CustomerName);
                        cmd.Parameters.AddWithValue("LedgerID", sales.LedgerID);
                        cmd.Parameters.AddWithValue("EmpID", sales.EmpID);
                        cmd.Parameters.AddWithValue("VoucherID", sales.VoucherID);
                        cmd.Parameters.AddWithValue("StateId", sales.StateId);
                        cmd.Parameters.AddWithValue("PaymodeId", sales.PaymodeId);
                        cmd.Parameters.AddWithValue("PaymodeName", string.IsNullOrEmpty(sales.PaymodeName) ? DBNull.Value : (object)sales.PaymodeName);

                        // Only add CreditDays parameter if it's a credit sale
                        if (IsCreditPaymentMode(sales.PaymodeId))
                        {
                            // Check if the stored procedure has a CreditDays parameter
                            try
                            {
                                cmd.Parameters.AddWithValue("CreditDays", sales.CreditDays);
                            }
                            catch (Exception ex)
                            {
                                // Continue without adding the parameter
                            }
                        }

                        // Add PaymentReference parameter
                        cmd.Parameters.AddWithValue("PaymentReference", string.IsNullOrEmpty(sales.PaymentReference) ? DBNull.Value : (object)sales.PaymentReference);

                        cmd.Parameters.AddWithValue("SubTotal", sales.SubTotal);
                        cmd.Parameters.AddWithValue("NetAmount", sales.NetAmount);
                        cmd.Parameters.AddWithValue("Status", string.IsNullOrEmpty(sales.Status) ? DBNull.Value : (object)sales.Status);
                        cmd.Parameters.AddWithValue("SavedVia", string.IsNullOrEmpty(sales.SavedVia) ? DBNull.Value : (object)sales.SavedVia);
                        cmd.Parameters.AddWithValue("UserId", sales.UserId);
                        cmd.Parameters.AddWithValue("DueDate", sales.DueDate);
                        cmd.Parameters.AddWithValue("_Operation", sales._Operation);

                        // Set default values for required parameters that aren't in our C# model
                        cmd.Parameters.AddWithValue("CounterId", SessionContext.CounterId);
                        cmd.Parameters.AddWithValue("CounterSessionId", SessionContext.CounterSessionId);
                        cmd.Parameters.AddWithValue("Freight", 0);
                        cmd.Parameters.AddWithValue("FreightProfit", 0);
                        cmd.Parameters.AddWithValue("PaymodeLedgerId", sales.PaymodeLedgerId);
                        cmd.Parameters.AddWithValue("TaxPer", sales.TaxPer);
                        cmd.Parameters.AddWithValue("TaxAmt", sales.TaxAmt);
                        cmd.Parameters.AddWithValue("CessPer", 0);
                        cmd.Parameters.AddWithValue("CessAmt", 0);
                        cmd.Parameters.AddWithValue("KFCessPer", 0);
                        cmd.Parameters.AddWithValue("KFCessAmt", 0);
                        cmd.Parameters.AddWithValue("DiscountPer", sales.DiscountPer);
                        cmd.Parameters.AddWithValue("DiscountAmt", sales.DiscountAmt);
                        cmd.Parameters.AddWithValue("RoundOffFlag", sales.RoundOffFlag);
                        cmd.Parameters.AddWithValue("RoundOff", sales.RoundOff);
                        cmd.Parameters.AddWithValue("TenderedAmount", sales.TenderedAmount);
                        cmd.Parameters.AddWithValue("Balance", sales.Balance);
                        cmd.Parameters.AddWithValue("CurrencyId", sales.CurrencyId);
                        cmd.Parameters.AddWithValue("CancelFlag", sales.CancelFlag);
                        cmd.Parameters.AddWithValue("BillCost", sales.BillCost); // Now contains calculated cost
                        cmd.Parameters.AddWithValue("OrderNo", DBNull.Value);
                        cmd.Parameters.AddWithValue("VoucherType", DBNull.Value);
                        cmd.Parameters.AddWithValue("TransporterLedgerId", DBNull.Value);
                        cmd.Parameters.AddWithValue("ReceivedAmount", sales.ReceivedAmount);

                        // Add missing parameters for IsPaid and IsSyncd
                        cmd.Parameters.AddWithValue("IsPaid", sales.IsPaid);
                        cmd.Parameters.AddWithValue("IsSyncd", sales.IsSyncd);

                        // Execute and check result
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataSet ds = new DataSet();
                            da.Fill(ds);

                            if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0].Rows.Count > 0))
                            {
                                BillNo = sales.BillNo.ToString(); // Set the BillNo field for return value
                            }
                            else
                            {
                                throw new Exception("Failed to update sales record. No results returned from database.");
                            }
                        }
                    }
                }
                catch (SqlException)
                {
                    throw;
                }
                catch (Exception)
                {
                    throw;
                }

                // Now process sales details records - always use CREATE operation after deleting old items
                // This ensures ALL items (old + new) are saved correctly
                salesDetails._Operation = "CREATE";
                // CRITICAL FIX: Explicitly set FinYearId in salesDetails to match sales.FinYearId
                salesDetails.FinYearId = sales.FinYearId;
                // Set common details for all items 
                salesDetails.BillNo = sales.BillNo; // This is already Int64 in both models

                // First check if dgvItems is valid
                if (dgvItems == null || dgvItems.Rows.Count == 0)
                {
                    throw new Exception("No items to save in sales invoice");
                }

                // Proceed with saving details
                for (int i = 0; i < dgvItems.RowCount; i++)
                {
                    try
                    {
                        if (dgvItems.Rows[i].Cells["ItemId"] == null || dgvItems.Rows[i].Cells["ItemId"].Value == null)
                            continue;

                        salesDetails.ItemId = GridParseHelpers.GetIntValue(dgvItems.Rows[i].Cells["ItemId"], 0);
                        salesDetails.Barcode = GridParseHelpers.GetStringValue(dgvItems.Rows[i].Cells["BarCode"], "Unknown");
                        salesDetails.ItemName = GridParseHelpers.GetStringValue(dgvItems.Rows[i].Cells["ItemName"], "Unknown Item");

                        if (dgvItems.Columns.Contains("SlNO") && dgvItems.Rows[i].Cells["SlNO"].Value != null)
                            salesDetails.SlNO = GridParseHelpers.GetIntValue(dgvItems.Rows[i].Cells["SlNO"], i + 1);
                        else if (dgvItems.Columns.Contains("SlNo") && dgvItems.Rows[i].Cells["SlNo"].Value != null) // Legacy check
                            salesDetails.SlNO = GridParseHelpers.GetIntValue(dgvItems.Rows[i].Cells["SlNo"], i + 1);
                        else
                            salesDetails.SlNO = i + 1;

                        // FIXED: Ensure UnitId is correctly set - first try to get from grid, then fallback to lookup
                        int unitId = GridParseHelpers.GetIntValue(dgvItems.Rows[i].Cells["UnitId"], 0);
                        if (unitId <= 0)
                        {
                            // Try to get UnitId from database based on Unit name
                            string unitName = GridParseHelpers.GetStringValue(dgvItems.Rows[i].Cells["Unit"], "");
                            if (!string.IsNullOrEmpty(unitName))
                            {
                                try
                                {
                                    // Use the UnitMasterRepository to get the UnitId by name using stored procedure
                                    unitId = unitMasterRepository.GetUnitIdByName(unitName, trans);
                                    if (unitId > 0)
                                    {
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Continue with default UnitId
                                }
                            }
                        }

                        salesDetails.UnitId = unitId > 0 ? unitId : 1; // Use found ID or default to 1
                        salesDetails.Unit = GridParseHelpers.GetStringValue(dgvItems.Rows[i].Cells["Unit"], "");
                        // FIX: Save actual selling price (S/Price column = Amount in grid) to UnitPrice, not original retail price
                        salesDetails.UnitPrice = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Amount"], 0f);
                        salesDetails.Cost = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Cost"], 0f);
                        salesDetails.DiscountAmount = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["DiscAmt"], 0f);
                        salesDetails.DiscountPer = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["DiscPer"], 0f);
                        salesDetails.MarginAmt = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["MarginAmt"], 0f);
                        salesDetails.MarginPer = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Marginper"], 0f);

                        // Get quantity as float to preserve decimal values
                        float qtyFloat = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Qty"], 0f);
                        // Store the integer value in the model property
                        salesDetails.Qty = Convert.ToInt32(qtyFloat);
                        // FIX: Amount should be line total (Qty × Unit Selling Price) for receipt printing, not unit price
                        // The Amount field in grid stores unit selling price, but for receipt it should be line total
                        float unitSellingPrice = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Amount"], 0f); // This is unit selling price from grid
                        // Calculate Amount as Qty × Unit Selling Price for receipt display
                        salesDetails.Amount = qtyFloat * unitSellingPrice;
                        salesDetails.TotalAmount = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["TotalAmount"], 0f); // Use actual TotalAmount from grid

                        // Get tax values from grid
                        salesDetails.TaxPer = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["TaxPer"], 0f);
                        salesDetails.TaxAmt = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["TaxAmt"], 0f);
                        salesDetails.TaxType = GridParseHelpers.GetStringValue(dgvItems.Rows[i].Cells["TaxType"], "incl");

                        // Get BaseAmount (taxable value) from grid for GST compliance
                        float baseAmount = 0f;
                        if (dgvItems.Columns.Contains("BaseAmount") && dgvItems.Rows[i].Cells["BaseAmount"].Value != null)
                        {
                            baseAmount = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["BaseAmount"], 0f);
                        }
                        else
                        {
                            // Calculate BaseAmount if not in grid
                            string taxType = salesDetails.TaxType?.ToLower() ?? "incl";
                            if (taxType == "incl" && salesDetails.TaxPer > 0)
                            {
                                // For inclusive: BaseAmount = Amount / (1 + TaxPer/100)
                                double divisor = 1.0 + (salesDetails.TaxPer / 100.0);
                                baseAmount = (float)Math.Round(salesDetails.Amount / divisor, 2);
                            }
                            else
                            {
                                // For exclusive: BaseAmount = Amount (before tax)
                                baseAmount = (float)salesDetails.Amount;
                            }
                        }

                        // Common properties from salesMaster or defaults
                        salesDetails.CompanyId = SessionContext.CompanyId;
                        salesDetails.BranchID = SessionContext.BranchId;
                        // Use FinYearId from SessionContext
                        salesDetails.FinYearId = SessionContext.FinYearId;
                        salesDetails.BillDate = DateTime.Now;
                        salesDetails.Expiry = DateTime.Now;
                        // Read MRP from grid - use UnitPrice (selling price) if MRP column not available
                        salesDetails.MRP = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["MRP"],
                            GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["UnitPrice"], 0f));
                        // Read Packing from grid if available
                        salesDetails.Packing = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Packing"], 1f);
                        salesDetails.BaseUnit = "";
                        salesDetails.CounterId = SessionContext.CounterId;

                        if (sales.VoucherID > 0)
                        {
                            salesDetails.VoucherId = Convert.ToInt32(sales.VoucherID);
                        }
                        else
                        {
                            salesDetails.VoucherId = 0; // Or handle as error
                        }

                        var detailParams = new DynamicParameters();

                        // Add all necessary parameters
                        detailParams.Add("CompanyId", salesDetails.CompanyId);
                        detailParams.Add("BranchID", salesDetails.BranchID);
                        // Use FinYearId from SessionContext
                        detailParams.Add("FinYearId", SessionContext.FinYearId);
                        detailParams.Add("CounterId", salesDetails.CounterId);
                        detailParams.Add("BillNo", salesDetails.BillNo);
                        detailParams.Add("BillDate", salesDetails.BillDate);
                        detailParams.Add("SlNO", salesDetails.SlNO);
                        detailParams.Add("ItemId", salesDetails.ItemId);
                        detailParams.Add("ItemName", salesDetails.ItemName);
                        detailParams.Add("UnitId", salesDetails.UnitId);
                        detailParams.Add("Unit", salesDetails.Unit);
                        detailParams.Add("Expiry", salesDetails.Expiry);
                        detailParams.Add("Qty", qtyFloat); // Use the original float value instead of the truncated integer
                        detailParams.Add("Packing", salesDetails.Packing);
                        detailParams.Add("UnitPrice", salesDetails.UnitPrice);
                        detailParams.Add("Amount", salesDetails.Amount);
                        detailParams.Add("DiscountPer", salesDetails.DiscountPer);
                        detailParams.Add("DiscountAmount", salesDetails.DiscountAmount);
                        detailParams.Add("MarginPer", salesDetails.MarginPer);
                        detailParams.Add("MarginAmt", salesDetails.MarginAmt);
                        detailParams.Add("TaxPer", salesDetails.TaxPer);
                        detailParams.Add("TaxAmt", salesDetails.TaxAmt);
                        detailParams.Add("TaxType", salesDetails.TaxType);
                        detailParams.Add("BaseAmount", baseAmount); // Add BaseAmount for GST compliance
                        detailParams.Add("TotalAmount", salesDetails.TotalAmount);
                        detailParams.Add("Cost", salesDetails.Cost);
                        detailParams.Add("BaseUnit", salesDetails.BaseUnit);
                        detailParams.Add("MRP", salesDetails.MRP);
                        detailParams.Add("Barcode", salesDetails.Barcode);
                        detailParams.Add("_Operation", salesDetails._Operation);
                        try
                        {
                            List<SalesDetails> ListSalesDetails = DataConnection.Query<SalesDetails>(
                                STOREDPROCEDURE._POS_SDetails_Win,
                                detailParams,
                                trans,
                                commandType: CommandType.StoredProcedure
                            ).ToList<SalesDetails>();
                            // CRITICAL FIX: Call _POS_SalesInvoice_PriceSettings to update base unit stock
                            try
                            {
                                var priceSettingsParams = new DynamicParameters();
                                priceSettingsParams.Add("CompanyId", salesDetails.CompanyId);
                                priceSettingsParams.Add("BranchId", salesDetails.BranchID);
                                priceSettingsParams.Add("FinYearId", salesDetails.FinYearId);
                                priceSettingsParams.Add("ItemId", salesDetails.ItemId);
                                priceSettingsParams.Add("UnitId", salesDetails.UnitId);
                                priceSettingsParams.Add("Qty", qtyFloat); // Use original float quantity
                                priceSettingsParams.Add("Packing", salesDetails.Packing);
                                priceSettingsParams.Add("BillNo", salesDetails.BillNo);
                                priceSettingsParams.Add("_Operation", "CREATE");

                                List<object> priceSettingsResult = DataConnection.Query<object>(
                                    STOREDPROCEDURE._POS_SalesInvoice_PriceSettings,
                                    priceSettingsParams,
                                    trans,
                                    commandType: CommandType.StoredProcedure
                                ).ToList<object>();
                            }
                            catch (Exception priceEx)
                            {
                                // Don't throw here - the main detail record was saved successfully
                                // Base unit stock update failure shouldn't prevent the sale from completing
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                    catch (Exception)
                    {
                        // Re-throw to ensure full transaction rollback - invoice should be complete or not saved
                        throw;
                    }
                }

                // Delete existing voucher entries to recreate them with updated values
                // We do this whether we preserved the ID or generated a new one
                if (sales.VoucherID > 0)
                {
                    try
                    {
                        voucher._Operation = OPERATION_UPDATE;
                        voucher.CompanyID = SessionContext.CompanyId;
                        voucher.BranchID = SessionContext.BranchId;
                        voucher.FinYearID = sales.FinYearId;
                        voucher.VoucherID = sales.VoucherID;
                        voucher.VoucherType = VOUCHER_TYPE_SALES;
                        DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, voucher, trans, commandType: CommandType.StoredProcedure).ToList();
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
                else
                {
                }

                // Create new voucher entries
                // We do this whether we preserved the ID or generated a new one
                if (sales.VoucherID > 0)
                {
                    voucher._Operation = "CREATE";

                    // Calculate GST amounts from sales details
                    Dictionary<double, double> gstTaxAmounts = CalculateGSTAmounts(dgvItems);

                    if (IsCashPaymentMode(sales.PaymodeId)) //CASH
                    {
                        CreateCashSaleVoucherEntries(sales, voucher, trans, gstTaxAmounts);
                    }
                    else //CREDIT
                    {
                        CreateCreditSaleVoucherEntries(sales, voucher, trans, gstTaxAmounts);
                    }
                }
                else
                {
                }
                trans.Commit();
            }
            catch (Exception ex)
            {
                if (trans != null)
                    trans.Rollback();
                if (ex.InnerException != null)
                    throw; // Re-throw without losing stack trace
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            // Return the bill number as a string for consistency with other methods
            return sales.BillNo.ToString();
        }
        #endregion

        #region Hold Sales
        public string HoldSales(SalesMaster sales, SalesDetails salesDetails, DataGridView dgvItems)
        {
            Voucher voucher = new Voucher();
            DataConnection.Open();
            // Set transaction isolation level to Serializable to prevent race conditions
            var trans = ((SqlConnection)DataConnection).BeginTransaction(IsolationLevel.Serializable);
            try
            {
                // Validate input parameters
                if (sales == null)
                {
                    throw new ArgumentNullException("sales", "Sales master object cannot be null");
                }

                if (salesDetails == null)
                {
                    throw new ArgumentNullException("salesDetails", "Sales details object cannot be null");
                }

                if (dgvItems == null)
                {
                    throw new ArgumentNullException("dgvItems", "Items grid cannot be null");
                }

                if (dgvItems.Rows.Count == 0)
                {
                    throw new ArgumentException("Items grid has no rows", "dgvItems");
                }

                // Define SQL Server date limits once at the beginning
                DateTime minSqlDate = new DateTime(1753, 1, 1);
                DateTime maxSqlDate = new DateTime(9999, 12, 31);

                // Ensure FinYearId is set from session context
                sales.FinYearId = SessionContext.FinYearId;
                // Validate and fix BillDate
                if (sales.BillDate < minSqlDate || sales.BillDate > maxSqlDate)
                {
                    sales.BillDate = DateTime.Now;
                    if (sales.BillDate < minSqlDate) sales.BillDate = minSqlDate;
                    if (sales.BillDate > maxSqlDate) sales.BillDate = maxSqlDate;
                }

                // Validate and fix DueDate if set
                if (sales.DueDate < minSqlDate || sales.DueDate > maxSqlDate)
                {
                    sales.DueDate = sales.BillDate;
                }

                NormalizePaymentModeForPersistence(sales);

                Netamount = sales.NetAmount;
                //here for getting the last bill no and assing the bill no to the model property
                sales._Operation = OPERATION_GENERATE_NUMBER;
                // No need to reassign FinYearId here - it's already set to 1 above
                try
                {
                    List<SalesMaster> getBillNO = DataConnection.Query<SalesMaster>(STOREDPROCEDURE._POS_Sales_Win, sales, trans,
                    commandType: CommandType.StoredProcedure).ToList<SalesMaster>();

                    if (getBillNO.Count > 0)
                    {
                        foreach (SalesMaster master in getBillNO)
                        {
                            sales.BillNo = master.BillNo;
                        }
                    }
                    else
                    {
                        throw new Exception("Failed to generate bill number for hold sale");
                    }
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                    }
                    throw new Exception("Failed to generate bill number: " + ex.Message, ex);
                }

                //HERE Genarating Number for Vouchers - USING THE APPROACH FROM PURCHASE REPOSITORY
                voucher._Operation = "GENERATENUMBER";
                voucher.BranchID = SessionContext.BranchId;
                voucher.CompanyID = SessionContext.CompanyId;
                voucher.VoucherType = VOUCHER_TYPE_SALES;
                voucher.FinYearID = SessionContext.FinYearId;
                try
                {
                    List<Voucher> VouchersList = DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, voucher, trans,
                        commandType: CommandType.StoredProcedure).ToList<Voucher>();

                    if (VouchersList.Count > 0)
                    {
                        foreach (Voucher objVoch in VouchersList)
                        {
                            voucher.VoucherID = objVoch.VoucherID;
                            sales.VoucherID = Convert.ToInt32(objVoch.VoucherID);
                        }
                    }
                    else
                    {
                        throw new Exception("Failed to generate VoucherID for hold sales");
                    }
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                    }
                    throw new Exception("Failed to generate voucher ID: " + ex.Message, ex);
                }

                //this section save the sales master section
                sales._Operation = "CREATE";
                // Calculate BillCost from sales details grid (Cost × Qty for all items)
                double billCost = 0;
                for (int i = 0; i < dgvItems.Rows.Count; i++)
                {
                    if (dgvItems.Rows[i].Cells["Cost"] != null && dgvItems.Rows[i].Cells["Cost"].Value != null &&
                        dgvItems.Rows[i].Cells["Qty"] != null && dgvItems.Rows[i].Cells["Qty"].Value != null)
                    {
                        float cost = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Cost"], 0f);
                        float qty = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Qty"], 0f);
                        billCost += (cost * qty);
                    }
                }
                sales.BillCost = billCost;
                // Create DynamicParameters with all required fields
                var parameters = new DynamicParameters();

                // Add all existing parameters
                parameters.Add("CompanyId", sales.CompanyId);
                parameters.Add("BranchId", sales.BranchId);
                parameters.Add("FinYearId", sales.FinYearId);
                parameters.Add("BillNo", sales.BillNo);
                parameters.Add("BillDate", sales.BillDate);
                parameters.Add("LedgerID", sales.LedgerID);
                parameters.Add("CustomerName", sales.CustomerName ?? "");
                parameters.Add("EmpID", sales.EmpID);
                parameters.Add("VoucherID", sales.VoucherID);
                parameters.Add("StateId", sales.StateId);
                parameters.Add("PaymodeId", sales.PaymodeId);
                parameters.Add("PaymodeName", sales.PaymodeName ?? "");

                // Only add CreditDays parameter if it's a credit sale
                if (IsCreditPaymentMode(sales.PaymodeId))
                {
                    try
                    {
                        parameters.Add("CreditDays", sales.CreditDays);
                    }
                    catch (Exception ex)
                    {
                        // Continue without adding the parameter
                    }
                }

                // Add PaymentReference parameter
                parameters.Add("PaymentReference", sales.PaymentReference ?? "");

                parameters.Add("SubTotal", sales.SubTotal);
                parameters.Add("NetAmount", sales.NetAmount);
                parameters.Add("Status", sales.Status ?? "");
                parameters.Add("SavedVia", sales.SavedVia ?? "");
                parameters.Add("UserId", sales.UserId);
                parameters.Add("DueDate", sales.DueDate);
                parameters.Add("_Operation", sales._Operation);

                // Add default values for parameters not in our model
                parameters.Add("CounterId", SessionContext.CounterId);
                parameters.Add("CounterSessionId", SessionContext.CounterSessionId);
                parameters.Add("Freight", 0);
                parameters.Add("FreightProfit", 0);
                parameters.Add("PaymodeLedgerId", sales.PaymodeLedgerId);
                parameters.Add("TaxPer", sales.TaxPer);
                parameters.Add("TaxAmt", sales.TaxAmt);
                parameters.Add("CessPer", 0);
                parameters.Add("CessAmt", 0);
                parameters.Add("KFCessPer", 0);
                parameters.Add("KFCessAmt", 0);
                parameters.Add("DiscountPer", sales.DiscountPer);
                parameters.Add("DiscountAmt", sales.DiscountAmt);
                parameters.Add("RoundOffFlag", sales.RoundOffFlag);
                parameters.Add("RoundOff", sales.RoundOff);
                parameters.Add("TenderedAmount", sales.TenderedAmount);
                parameters.Add("Balance", sales.Balance);
                parameters.Add("CurrencyId", sales.CurrencyId);
                parameters.Add("CancelFlag", sales.CancelFlag);
                parameters.Add("BillCost", sales.BillCost); // Now contains calculated cost
                parameters.Add("OrderNo", null);
                parameters.Add("VoucherType", null);
                parameters.Add("TransporterLedgerId", null);
                parameters.Add("ReceivedAmount", sales.ReceivedAmount);
                try
                {
                    List<SalesMaster> listSales = DataConnection.Query<SalesMaster>(
                        STOREDPROCEDURE._POS_Sales_Win,
                        parameters,
                        trans,
                        commandType: CommandType.StoredProcedure
                    ).ToList<SalesMaster>();

                    if (listSales.Count == 0)
                    {
                        throw new Exception("No results returned from sales master creation");
                    }
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                    }
                    throw new Exception("Failed to create sales master: " + ex.Message, ex);
                }

                // this section for save the sales details details 
                salesDetails._Operation = "CREATE";
                // CRITICALLY IMPORTANT: Ensure salesDetails.FinYearId is set from session context
                salesDetails.FinYearId = SessionContext.FinYearId;
                // Check if dgvItems is valid
                if (dgvItems == null || dgvItems.Rows.Count == 0)
                {
                    throw new Exception("No items to save in the bill");
                }
                // Create a safe date for all detail records
                DateTime safeDate = DateTime.Now;
                if (safeDate < minSqlDate) safeDate = minSqlDate;
                if (safeDate > maxSqlDate) safeDate = maxSqlDate;

                for (int i = 0; i < dgvItems.RowCount; i++)
                {
                    try
                    {
                        // Robustly read and parse values from dgvItems
                        salesDetails.ItemId = GridParseHelpers.GetIntValue(dgvItems.Rows[i].Cells["ItemId"], 0);
                        salesDetails.Barcode = GridParseHelpers.GetStringValue(dgvItems.Rows[i].Cells["BarCode"], "Unknown");
                        salesDetails.ItemName = GridParseHelpers.GetStringValue(dgvItems.Rows[i].Cells["ItemName"], "Unknown Item");

                        if (dgvItems.Columns.Contains("SlNO") && dgvItems.Rows[i].Cells["SlNO"].Value != null)
                            salesDetails.SlNO = GridParseHelpers.GetIntValue(dgvItems.Rows[i].Cells["SlNO"], i + 1);
                        else if (dgvItems.Columns.Contains("SlNo") && dgvItems.Rows[i].Cells["SlNo"].Value != null) // Legacy check
                            salesDetails.SlNO = GridParseHelpers.GetIntValue(dgvItems.Rows[i].Cells["SlNo"], i + 1);
                        else
                            salesDetails.SlNO = i + 1;

                        // FIXED: Ensure UnitId is correctly set - first try to get from grid, then fallback to lookup
                        int unitId = GridParseHelpers.GetIntValue(dgvItems.Rows[i].Cells["UnitId"], 0);
                        if (unitId <= 0)
                        {
                            // Try to get UnitId from database based on Unit name
                            string unitName = GridParseHelpers.GetStringValue(dgvItems.Rows[i].Cells["Unit"], "");
                            if (!string.IsNullOrEmpty(unitName))
                            {
                                try
                                {
                                    // Use the UnitMasterRepository to get the UnitId by name using stored procedure
                                    unitId = unitMasterRepository.GetUnitIdByName(unitName, trans);
                                    if (unitId > 0)
                                    {
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Continue with default UnitId
                                }
                            }
                        }

                        salesDetails.UnitId = unitId > 0 ? unitId : 1; // Use found ID or default to 1
                        salesDetails.Unit = GridParseHelpers.GetStringValue(dgvItems.Rows[i].Cells["Unit"], "");
                        // FIX: Save actual selling price (S/Price column = Amount in grid) to UnitPrice, not original retail price
                        salesDetails.UnitPrice = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Amount"], 0f);
                        salesDetails.Cost = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Cost"], 0f);
                        salesDetails.DiscountAmount = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["DiscAmt"], 0f);
                        salesDetails.DiscountPer = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["DiscPer"], 0f);
                        salesDetails.MarginAmt = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["MarginAmt"], 0f);
                        salesDetails.MarginPer = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Marginper"], 0f);

                        // Get quantity as float to preserve decimal values
                        float qtyFloat = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Qty"], 0f);
                        salesDetails.Qty = Convert.ToInt32(qtyFloat);
                        // FIX: Amount should be line total (Qty × Unit Selling Price) for receipt printing, not unit price
                        // The Amount field in grid stores unit selling price, but for receipt it should be line total
                        float unitSellingPrice = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Amount"], 0f); // This is unit selling price from grid
                        // Calculate Amount as Qty × Unit Selling Price for receipt display
                        salesDetails.Amount = qtyFloat * unitSellingPrice;
                        salesDetails.TotalAmount = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["TotalAmount"], 0f); // Use actual TotalAmount from grid

                        // Get tax values from grid
                        salesDetails.TaxPer = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["TaxPer"], 0f);
                        salesDetails.TaxAmt = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["TaxAmt"], 0f);
                        salesDetails.TaxType = GridParseHelpers.GetStringValue(dgvItems.Rows[i].Cells["TaxType"], "incl");

                        // Get BaseAmount (taxable value) from grid for GST compliance
                        float baseAmount = 0f;
                        if (dgvItems.Columns.Contains("BaseAmount") && dgvItems.Rows[i].Cells["BaseAmount"].Value != null)
                        {
                            baseAmount = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["BaseAmount"], 0f);
                        }
                        else
                        {
                            // Calculate BaseAmount if not in grid
                            string taxType = salesDetails.TaxType?.ToLower() ?? "incl";
                            if (taxType == "incl" && salesDetails.TaxPer > 0)
                            {
                                double divisor = 1.0 + (salesDetails.TaxPer / 100.0);
                                baseAmount = (float)Math.Round(salesDetails.Amount / divisor, 2);
                            }
                            else
                            {
                                baseAmount = (float)salesDetails.Amount;
                            }
                        }

                        // Common properties from salesMaster or defaults
                        salesDetails.CompanyId = SessionContext.CompanyId;
                        salesDetails.BranchID = SessionContext.BranchId;
                        // Use FinYearId from SessionContext
                        salesDetails.FinYearId = SessionContext.FinYearId;
                        // Use the safe date for all date fields
                        salesDetails.BillDate = safeDate;
                        salesDetails.Expiry = safeDate;
                        // Read MRP from grid - use UnitPrice (selling price) if MRP column not available
                        salesDetails.MRP = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["MRP"],
                            GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["UnitPrice"], 0f));
                        // Read Packing from grid if available
                        salesDetails.Packing = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["Packing"], 1f);
                        salesDetails.BaseUnit = "";
                        salesDetails.CounterId = SessionContext.CounterId;
                        salesDetails.BillNo = sales.BillNo; // Set the BillNo from the master

                        if (sales.VoucherID > 0)
                            salesDetails.VoucherId = Convert.ToInt32(sales.VoucherID);
                        else
                        {
                            salesDetails.VoucherId = 0; // Or handle as error
                        }

                        var detailParams = new DynamicParameters();

                        // Add all necessary parameters for _POS_SDetails_Win_Hold
                        detailParams.Add("CompanyId", salesDetails.CompanyId);
                        detailParams.Add("BranchID", salesDetails.BranchID);
                        detailParams.Add("FinYearId", SessionContext.FinYearId);
                        detailParams.Add("CounterId", salesDetails.CounterId);
                        detailParams.Add("BillNo", salesDetails.BillNo);
                        detailParams.Add("BillDate", salesDetails.BillDate);
                        detailParams.Add("SlNO", salesDetails.SlNO);
                        detailParams.Add("ItemId", salesDetails.ItemId);
                        detailParams.Add("ItemName", salesDetails.ItemName);
                        detailParams.Add("UnitId", salesDetails.UnitId);
                        detailParams.Add("Unit", salesDetails.Unit);
                        detailParams.Add("Expiry", salesDetails.Expiry);
                        detailParams.Add("Qty", qtyFloat);
                        detailParams.Add("Packing", salesDetails.Packing);
                        detailParams.Add("UnitPrice", salesDetails.UnitPrice);
                        detailParams.Add("Amount", salesDetails.Amount);
                        detailParams.Add("DiscountPer", salesDetails.DiscountPer);
                        detailParams.Add("DiscountAmount", salesDetails.DiscountAmount);
                        detailParams.Add("MarginPer", salesDetails.MarginPer);
                        detailParams.Add("MarginAmt", salesDetails.MarginAmt);
                        detailParams.Add("TaxPer", salesDetails.TaxPer);
                        detailParams.Add("TaxAmt", salesDetails.TaxAmt);
                        detailParams.Add("TaxType", salesDetails.TaxType);
                        detailParams.Add("BaseAmount", baseAmount); // Add BaseAmount for GST compliance
                        detailParams.Add("TotalAmount", salesDetails.TotalAmount);
                        detailParams.Add("Cost", salesDetails.Cost);
                        detailParams.Add("BaseUnit", salesDetails.BaseUnit);
                        detailParams.Add("VoucherId", salesDetails.VoucherId);
                        detailParams.Add("MRP", salesDetails.MRP);
                        detailParams.Add("Barcode", salesDetails.Barcode);
                        detailParams.Add("_Operation", salesDetails._Operation);
                        try
                        {
                            // Use _POS_SDetails_Win_Hold - does NOT update stock
                            // Stock will only be reduced when the bill is completed/finalized
                            List<SalesDetails> ListSalesDetails = DataConnection.Query<SalesDetails>(
                                STOREDPROCEDURE._POS_SDetails_Win_Hold,
                                detailParams,
                                trans,
                                commandType: CommandType.StoredProcedure
                            ).ToList<SalesDetails>();
                            // NOTE: Stock is NOT updated for hold bills
                            // Stock will be updated when the bill is completed via CompleteSale
                            // This is the correct POS behavior - hold bills are temporary snapshots
                        }
                        catch (Exception detailEx)
                        {
                            if (detailEx.InnerException != null)
                            {
                            }
                            throw new Exception($"Failed to save detail for item {salesDetails.ItemName}: {detailEx.Message}", detailEx);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error processing row {i}: {ex.Message}", ex);
                    }
                }

                // NOTE: Voucher entries are NOT created for hold bills
                // They will be created when the bill is completed/finalized with payment
                // This is the correct accounting practice - uncommitted sales should not create journal entries
                trans.Commit();
                return sales.BillNo.ToString(); // Return the bill number as success
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                }
                trans.Rollback();
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }
        #endregion

        #region Cancel Hold Bill
        /// <summary>
        /// Cancels a held bill and restores stock quantities.
        /// This calls DELETE_BY_BILLNO operation which restores ItemBatch.Qty and PriceSettings.Stock.
        /// </summary>
        /// <param name="billNo">The bill number to cancel</param>
        /// <param name="voucherId">The voucher ID associated with the bill</param>
        /// <returns>"SUCCESS" if successful, error message otherwise</returns>
        public string CancelHoldBill(long billNo, int voucherId)
        {
            SqlTransaction trans = null;
            try
            {
                // Get session context values
                int finYearId = SessionContext.FinYearId;
                int branchId = SessionContext.BranchId;
                int companyId = SessionContext.CompanyId;

                DataConnection.Open();
                trans = (SqlTransaction)DataConnection.BeginTransaction();

                try
                {
                    // Step 1: Delete sales details WITHOUT restoring stock
                    // IMPORTANT: Since hold bills do NOT reduce stock, we should not try to restore it
                    // Use the _POS_SDetails_Win_Hold stored procedure with DELETE_HOLD operation
                    using (SqlCommand deleteCmd = new SqlCommand(STOREDPROCEDURE._POS_SDetails_Win_Hold, (SqlConnection)DataConnection, trans))
                    {
                        deleteCmd.CommandType = CommandType.StoredProcedure;
                        deleteCmd.Parameters.AddWithValue("@BillNo", billNo);
                        deleteCmd.Parameters.AddWithValue("@BranchID", branchId);
                        deleteCmd.Parameters.AddWithValue("@CompanyId", companyId);
                        deleteCmd.Parameters.AddWithValue("@FinYearId", finYearId);
                        deleteCmd.Parameters.AddWithValue("@_Operation", "DELETE_HOLD");

                        using (SqlDataReader reader = deleteCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string result = reader["Result"]?.ToString() ?? "";
                                string message = reader["Message"]?.ToString() ?? "";
                            }
                        }
                    }

                    // Step 2: Delete the master record using DELETE operation
                    var masterParams = new DynamicParameters();
                    masterParams.Add("CompanyId", companyId);
                    masterParams.Add("BranchId", branchId);
                    masterParams.Add("FinYearId", finYearId);
                    masterParams.Add("BillNo", billNo);
                    masterParams.Add("VoucherID", voucherId);
                    masterParams.Add("_Operation", OPERATION_DELETE);
                    var masterResult = DataConnection.Query<dynamic>(
                        STOREDPROCEDURE._POS_Sales_Win,
                        masterParams,
                        trans,
                        commandType: CommandType.StoredProcedure
                    ).ToList();
                    trans.Commit();
                    return "SUCCESS";
                }
                catch (Exception innerEx)
                {
                    trans?.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                }
                return "Error cancelling hold bill: " + ex.Message;
            }
            finally
            {
                if (DataConnection != null && DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }
        #endregion

        #region Complete Sale
        public string CompleteSale(SalesMaster sales)
        {
            try
            {
                // Validate required parameters
                if (sales.BillNo <= 0)
                {
                    return "Invalid bill number";
                }

                // Ensure FinYearId is set from session context
                sales.FinYearId = SessionContext.FinYearId;
                NormalizePaymentModeForPersistence(sales);

                // Open connection
                DataConnection.Open();

                // Use COMPLETE operation to update the held bill with the final payment state.
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_Sales_Win, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@CompanyId", sales.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", sales.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", sales.FinYearId);
                    cmd.Parameters.AddWithValue("@CounterSessionId", SessionContext.CounterSessionId);
                    cmd.Parameters.AddWithValue("@BillNo", sales.BillNo);
                    cmd.Parameters.AddWithValue("@VoucherID", sales.VoucherID);
                    cmd.Parameters.AddWithValue("@PaymodeId", sales.PaymodeId);
                    cmd.Parameters.AddWithValue("@PaymodeName", string.IsNullOrEmpty(sales.PaymodeName) ? DBNull.Value : (object)sales.PaymodeName);
                    cmd.Parameters.AddWithValue("@PaymodeLedgerId", sales.PaymodeLedgerId);
                    cmd.Parameters.AddWithValue("@CreditDays", IsCreditPaymentMode(sales.PaymodeId) ? sales.CreditDays : 0);
                    cmd.Parameters.AddWithValue("@DueDate", sales.DueDate);
                    cmd.Parameters.AddWithValue("@PaymentReference", string.IsNullOrEmpty(sales.PaymentReference) ? DBNull.Value : (object)sales.PaymentReference);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(sales.Status) ? DBNull.Value : (object)sales.Status);

                    // Set payment fields based on payment mode (POS Logic)
                    if (IsCreditPaymentMode(sales.PaymodeId))
                    {
                        // CREDIT SALE LOGIC - Override with correct values
                        cmd.Parameters.AddWithValue("@TenderedAmount", 0); // No immediate payment
                        cmd.Parameters.AddWithValue("@Balance", sales.NetAmount); // Full amount outstanding
                        cmd.Parameters.AddWithValue("@ReceivedAmount", 0); // No payment received
                        cmd.Parameters.AddWithValue("@IsPaid", false); // Not paid
                    }
                    else
                    {
                        // CASH SALE LOGIC - Use actual payment values from UI
                        cmd.Parameters.AddWithValue("@TenderedAmount", sales.TenderedAmount);
                        cmd.Parameters.AddWithValue("@Balance", sales.Balance);
                        cmd.Parameters.AddWithValue("@ReceivedAmount", sales.ReceivedAmount);
                        cmd.Parameters.AddWithValue("@IsPaid", sales.IsPaid);
                    }

                    cmd.Parameters.AddWithValue("@_Operation", OPERATION_COMPLETE);
                    // Execute the command
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        da.Fill(ds);

                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0].Rows.Count > 0))
                        {
                            string billNo = ds.Tables[0].Rows[0][0].ToString();
                            return billNo;
                        }
                        else
                        {
                            return "Failed to complete sale";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                }
                return "Error: " + ex.Message;
            }
            finally
            {
                if (DataConnection != null && DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }
        #endregion

        public GetHoldBillGrid GetHolBill()
        {

            GetHoldBillGrid item = new GetHoldBillGrid();
            DataConnection.Open();

            try
            {
                bool isAdmin = string.Equals(SessionContext.UserLevel, "Administrator", StringComparison.OrdinalIgnoreCase);
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@Operation", "GetHold");
                    cmd.Parameters.AddWithValue("@CounterId", isAdmin ? 0 : SessionContext.CounterId);
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            var allHoldBills = ds.Tables[0].ToListOfObject<GetHoldBill>();

                            // Filter by BranchId and CounterId to ensure data separation between counters/branches (Admins bypass CounterId filter)
                            item.List = allHoldBills
                                .Where(bill => bill.BranchId == SessionContext.BranchId && (isAdmin || bill.CounterId == SessionContext.CounterId))
                                .ToList();
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

        public GetHoldBillGrid GetBill()
        {
            GetHoldBillGrid item = new GetHoldBillGrid();
            item.List = new List<GetHoldBill>(); // Initialize an empty list to avoid null reference

            try
            {
                DataConnection.Open();

                bool isAdmin = string.Equals(SessionContext.UserLevel, "Administrator", StringComparison.OrdinalIgnoreCase);
                // Use _POS_Sales_Win stored procedure with GETALL operation
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_Sales_Win, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@CounterId", isAdmin ? 0 : SessionContext.CounterId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);

                        // Process the result set
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            // Convert rows to GetHoldBill objects
                            List<GetHoldBill> bills = new List<GetHoldBill>();
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                try
                                {
                                    DateTime billDate = DateTime.MinValue;
                                    if (ds.Tables[0].Columns.Contains("BillDate") && row["BillDate"] != DBNull.Value)
                                        billDate = Convert.ToDateTime(row["BillDate"]);

                                    DateTime dueDate = DateTime.MinValue;
                                    if (ds.Tables[0].Columns.Contains("DueDate") && row["DueDate"] != DBNull.Value)
                                        dueDate = Convert.ToDateTime(row["DueDate"]);

                                    string paymodeName = null;
                                    if (ds.Tables[0].Columns.Contains("PaymodeName") && row["PaymodeName"] != DBNull.Value)
                                        paymodeName = row["PaymodeName"].ToString();
                                    else if (ds.Tables[0].Columns.Contains("PayModeName") && row["PayModeName"] != DBNull.Value)
                                        paymodeName = row["PayModeName"].ToString();

                                    int paymodeId = 0;
                                    if (ds.Tables[0].Columns.Contains("PaymodeId") && row["PaymodeId"] != DBNull.Value)
                                        paymodeId = Convert.ToInt32(row["PaymodeId"]);
                                    else if (ds.Tables[0].Columns.Contains("PayModeId") && row["PayModeId"] != DBNull.Value)
                                        paymodeId = Convert.ToInt32(row["PayModeId"]);

                                    string status = null;
                                    if (ds.Tables[0].Columns.Contains("Status") && row["Status"] != DBNull.Value)
                                        status = row["Status"].ToString();

                                    int counterId = 0;
                                    if (ds.Tables[0].Columns.Contains("CounterId") && row["CounterId"] != DBNull.Value)
                                        counterId = Convert.ToInt32(row["CounterId"]);

                                    string saleType;
                                    if (!string.IsNullOrWhiteSpace(paymodeName))
                                    {
                                        if (paymodeName.IndexOf("credit", StringComparison.OrdinalIgnoreCase) >= 0)
                                            saleType = "Credit";
                                        else if (paymodeName.IndexOf("cash", StringComparison.OrdinalIgnoreCase) >= 0)
                                            saleType = "Cash";
                                        else
                                            saleType = paymodeName;
                                    }
                                    else if (!string.IsNullOrWhiteSpace(status) &&
                                        status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                                    {
                                        saleType = "Credit";
                                    }
                                    else if (billDate != DateTime.MinValue && dueDate != DateTime.MinValue &&
                                        dueDate.Date > billDate.Date)
                                    {
                                        saleType = "Credit";
                                    }
                                    else
                                    {
                                        saleType = "Cash";
                                    }

                                    GetHoldBill bill = new GetHoldBill
                                    {
                                        BillNo = Convert.ToInt64(row["BillNo"]),
                                        CustomerName = row["CustomerName"] != DBNull.Value ? row["CustomerName"].ToString() : "Default Customer",
                                        NetAmount = row["NetAmount"] != DBNull.Value ? Convert.ToDouble(row["NetAmount"]) : 0,
                                        SaleType = saleType,
                                        CounterId = counterId,
                                        BillDate = billDate
                                    };
                                    bills.Add(bill);
                                }
                                catch (Exception ex)
                                {
                                    // Continue with next row
                                }
                            }

                            // Filter by CounterId in C# as double-safety (Admins bypass)
                            item.List = bills
                                .Where(bill => isAdmin || bill.CounterId == SessionContext.CounterId)
                                .ToList();
                        }
                        else
                        {
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Error handled silently - this is a read operation
            }
            finally
            {
                if (DataConnection != null && DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return item;
        }

        public ItemPictureGrid GetItemPicture(string Barcode)
        {

            ItemPictureGrid item = new ItemPictureGrid();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@Barcode", Barcode);
                    cmd.Parameters.AddWithValue("@Operation", "GETPICTURE");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            item.list = ds.Tables[0].ToListOfObject<ItemPicture>();
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


        public DataTable GetInvoicePrint(Int64 BillNo)
        {

            DataTable item = new DataTable();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_GetBill, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BillNo", BillNo);
                    cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                    cmd.Parameters.AddWithValue("@_Operations", "GETBILL");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable ds = new DataTable();
                        adapt.Fill(ds);
                        item = ds;

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

        public List<InvoicePrnt> GetBillData(billPara print)
        {
            List<InvoicePrnt> invoice = DataConnection.Query<InvoicePrnt>(STOREDPROCEDURE._POS_GetBill, print,
                commandType: CommandType.StoredProcedure).ToList<InvoicePrnt>();
            return invoice;
        }

        public salesGrid GetById(Int64 BillNo)
        {
            salesGrid sales = new salesGrid();
            SalesMaster sm = new SalesMaster();
            try
            {
                DataConnection.Open();

                // No need for a transaction for a read-only operation
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_Sales_Win, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BillNo", BillNo);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@_Operation", "GetById");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);

                        // Log the results
                        if (ds != null)
                        {
                            for (int i = 0; i < ds.Tables.Count; i++)
                            {
                            }
                        }
                        else
                        {
                        }

                        // Process master data
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            sales.ListSales = ds.Tables[0].ToListOfObject<SalesMaster>();
                        }
                        else
                        {
                            sales.ListSales = new List<SalesMaster>(); // Initialize with empty list to avoid null references
                        }

                        // Process details data
                        if (ds != null && ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                        {
                            sales.ListSDetails = ds.Tables[1].ToListOfObject<SalesDetails>();
                        }
                        else
                        {
                            sales.ListSDetails = new List<SalesDetails>(); // Initialize with empty list to avoid null references
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    throw new Exception($"Error retrieving bill data for Bill #{BillNo}: {ex.Message}", ex);
            }
            finally
            {
                // Always ensure the connection is closed
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return sales;
        }

        public List<SalesMaster> GetBillsByCustomer(int customerId)
        {
            try
            {
                DataConnection.Open();
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@FinyearId", DataBase.FinyearId);
                    cmd.Parameters.AddWithValue("@Operation", "SalesInvoice");
                    cmd.Parameters.AddWithValue("@LedgerId", customerId);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                var result = ds.Tables[0].ToListOfObject<SalesMaster>();
                                return result;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting bills for customer {customerId}: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return new List<SalesMaster>();
        }

        #region Split Payment Methods

        /// <summary>
        /// Saves multiple payment details for a split payment transaction
        /// </summary>
        public void SavePaymentDetails(List<SalesPaymentDetail> paymentDetails, IDbTransaction trans)
        {
            try
            {
                if (paymentDetails == null || paymentDetails.Count == 0)
                {
                    return;
                }
                foreach (var payment in paymentDetails)
                {
                    payment._Operation = "CREATE";

                    var paymentParams = new DynamicParameters();
                    paymentParams.Add("CompanyId", payment.CompanyId);
                    paymentParams.Add("BranchId", payment.BranchId);
                    paymentParams.Add("FinYearId", payment.FinYearId);
                    paymentParams.Add("BillNo", payment.BillNo);
                    paymentParams.Add("PaymodeId", payment.PaymodeId);
                    paymentParams.Add("Amount", payment.Amount);
                    paymentParams.Add("Reference", payment.Reference ?? (object)null);
                    paymentParams.Add("_Operation", payment._Operation);

                    DataConnection.Query<int>(
                        STOREDPROCEDURE._POS_SPaymentDetails,
                        paymentParams,
                        trans,
                        commandType: CommandType.StoredProcedure
                    );
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieves all payment details for a specific bill
        /// </summary>
        public List<SalesPaymentDetail> GetPaymentDetailsByBillNo(long billNo, int companyId, int branchId, int finYearId)
        {
            try
            {
                DataConnection.Open();

                var paymentParams = new DynamicParameters();
                paymentParams.Add("CompanyId", companyId);
                paymentParams.Add("BranchId", branchId);
                paymentParams.Add("FinYearId", finYearId);
                paymentParams.Add("BillNo", billNo);
                paymentParams.Add("_Operation", "GET_BY_BILLNO");

                var paymentDetails = DataConnection.Query<SalesPaymentDetail>(
                    STOREDPROCEDURE._POS_SPaymentDetails,
                    paymentParams,
                    commandType: CommandType.StoredProcedure
                ).ToList();
                return paymentDetails;
            }
            catch (Exception ex)
            {
                return new List<SalesPaymentDetail>();
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        /// <summary>
        /// Deletes all payment details for a specific bill (used during update/delete operations)
        /// </summary>
        public void DeletePaymentDetailsByBillNo(long billNo, int companyId, int branchId, int finYearId, IDbTransaction trans)
        {
            try
            {
                var paymentParams = new DynamicParameters();
                paymentParams.Add("CompanyId", companyId);
                paymentParams.Add("BranchId", branchId);
                paymentParams.Add("FinYearId", finYearId);
                paymentParams.Add("BillNo", billNo);
                paymentParams.Add("_Operation", "DELETE_BY_BILLNO");

                DataConnection.Execute(
                    STOREDPROCEDURE._POS_SPaymentDetails,
                    paymentParams,
                    trans,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Saves payment details after the main sale transaction has completed.
        /// This is used when saving split payment details from the UI layer.
        /// </summary>
        public void SavePaymentDetailsAfterSale(List<SalesPaymentDetail> paymentDetails)
        {
            if (paymentDetails == null || paymentDetails.Count == 0)
            {
                return;
            }

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();
                using (var trans = DataConnection.BeginTransaction())
                {
                    try
                    {
                        // Delete existing payment details for this bill (in case of update)
                        var firstPayment = paymentDetails[0];
                        DeletePaymentDetailsByBillNo(firstPayment.BillNo, firstPayment.CompanyId,
                            firstPayment.BranchId, firstPayment.FinYearId, trans);

                        // Save new payment details
                        SavePaymentDetails(paymentDetails, trans);

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        #endregion


        // Helper methods for robust parsing (add these to your SalesRepository class or a utility class)
        public static class GridParseHelpers
        {
            public static int GetIntValue(DataGridViewCell cell, int defaultValue)
            {
                if (cell != null && cell.Value != null && cell.Value != DBNull.Value)
                {
                    if (int.TryParse(cell.Value.ToString(), out int result))
                        return result;
                }
                return defaultValue;
            }

            public static float GetFloatValue(DataGridViewCell cell, float defaultValue)
            {
                if (cell != null && cell.Value != null && cell.Value != DBNull.Value)
                {
                    if (float.TryParse(cell.Value.ToString(), out float result))
                        return result;
                }
                return defaultValue;
            }

            public static string GetStringValue(DataGridViewCell cell, string defaultValue)
            {
                if (cell != null && cell.Value != null && cell.Value != DBNull.Value)
                {
                    return cell.Value.ToString();
                }
                return defaultValue;
            }
        }

        /// <summary>
        /// Safely parses a double value from an object (e.g., DataRow cell), returning default value if parsing fails
        /// </summary>
        private double ParseDouble(object value, double defaultValue = 0)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;

            if (value is double d)
                return d;

            if (value is float f)
                return f;

            if (value is decimal dec)
                return (double)dec;

            if (value is int i)
                return i;

            if (double.TryParse(value.ToString(), out double result))
                return result;

            return defaultValue;
        }

        #region Utility Methods

        /// <summary>
        /// Ensures a Financial Year is set and available to use
        /// Always returns 1 as the standard FinYearId
        /// </summary>
        /// <returns>Financial Year ID as int (always 1)</returns>
        private int EnsureFinancialYearIsSet()
        {
            // Use FinYearId from SessionContext
            return SessionContext.FinYearId;
        }

        private bool TryParseCreditDaysFromPaymodeLabel(string paymodeName, out int creditDays)
        {
            creditDays = 0;
            if (string.IsNullOrWhiteSpace(paymodeName))
            {
                return false;
            }

            string normalized = paymodeName.Trim().ToUpperInvariant();
            if (!normalized.Contains("DAY"))
            {
                return false;
            }

            string digits = new string(normalized.Where(char.IsDigit).ToArray());
            return !string.IsNullOrWhiteSpace(digits) && int.TryParse(digits, out creditDays);
        }

        private void NormalizePaymentModeForPersistence(SalesMaster sales)
        {
            if (sales == null)
            {
                return;
            }

            int parsedCreditDays = 0;
            bool looksLikeCreditDays = TryParseCreditDaysFromPaymodeLabel(sales.PaymodeName, out parsedCreditDays);

            List<PaymodeDDl> paymodes = null;
            int creditPaymodeId = 0;
            int cashPaymodeId = 2;

            try
            {
                Dropdowns dp = new Dropdowns();
                var grid = dp.PaymodeDDl();
                paymodes = grid?.List?.ToList();
                creditPaymodeId = paymodes?.FirstOrDefault(p =>
                    p.PayModeName.Equals("Credit", StringComparison.OrdinalIgnoreCase))?.PayModeID ?? 0;
                cashPaymodeId = paymodes?.FirstOrDefault(p =>
                    p.PayModeName.Equals("Cash", StringComparison.OrdinalIgnoreCase))?.PayModeID ?? cashPaymodeId;
            }
            catch
            {
                // Fallbacks are already initialized.
            }

            bool isCreditByName = string.Equals(sales.PaymodeName, "Credit", StringComparison.OrdinalIgnoreCase);
            bool isCreditById = creditPaymodeId > 0 && sales.PaymodeId == creditPaymodeId;

            if (looksLikeCreditDays || isCreditByName || isCreditById)
            {
                if (creditPaymodeId > 0)
                {
                    sales.PaymodeId = creditPaymodeId;
                }

                sales.PaymodeName = "Credit";

                if (sales.CreditDays <= 0)
                {
                    sales.CreditDays = parsedCreditDays > 0 ? parsedCreditDays : 30;
                }

                if (sales.DueDate <= sales.BillDate)
                {
                    sales.DueDate = sales.BillDate.AddDays(sales.CreditDays);
                }

                return;
            }

            bool paymodeIdExists = paymodes != null && paymodes.Any(p => p.PayModeID == sales.PaymodeId);
            if (paymodes != null && !paymodeIdExists)
            {
                sales.PaymodeId = cashPaymodeId;
                paymodeIdExists = true;
            }

            if (paymodeIdExists && paymodes != null)
            {
                var mode = paymodes.FirstOrDefault(p => p.PayModeID == sales.PaymodeId);
                if (mode != null && !string.IsNullOrWhiteSpace(mode.PayModeName))
                {
                    sales.PaymodeName = mode.PayModeName;
                }
            }

            if (string.IsNullOrWhiteSpace(sales.PaymodeName))
            {
                sales.PaymodeName = "Cash";
                if (sales.PaymodeId <= 0)
                {
                    sales.PaymodeId = cashPaymodeId;
                }
            }

            sales.CreditDays = 0;
            if (sales.DueDate < sales.BillDate)
            {
                sales.DueDate = sales.BillDate;
            }

            // Ensure PaymodeLedgerId is resolved from PayMode table if not already set
            if (sales.PaymodeLedgerId <= 0 && sales.PaymodeId > 0)
            {
                try
                {
                    string ledgerQuery = "SELECT TOP 1 LedgerID FROM PayMode WHERE PayModeID = @PaymodeID";
                    using (SqlCommand ledgerCmd = new SqlCommand(ledgerQuery, (SqlConnection)DataConnection))
                    {
                        ledgerCmd.Parameters.AddWithValue("@PaymodeID", sales.PaymodeId);
                        object ledgerResult = ledgerCmd.ExecuteScalar();
                        if (ledgerResult != null && ledgerResult != DBNull.Value)
                        {
                            sales.PaymodeLedgerId = Convert.ToInt32(ledgerResult);
                        }
                    }
                }
                catch
                {
                    // Fail gracefully if DataConnection is closed or query fails
                }
            }
        }

        /// <summary>
        /// Checks if the given payment mode ID is a Credit payment mode
        /// </summary>
        /// <param name="paymodeId">Payment mode ID to check</param>
        /// <returns>True if it's a Credit payment mode</returns>
        private bool IsCreditPaymentMode(int paymodeId)
        {
            try
            {
                Dropdowns dp = new Dropdowns();
                var pm = dp.PaymodeDDl();
                var creditMode = pm.List?.FirstOrDefault(p =>
                    p.PayModeName.Equals("Credit", StringComparison.OrdinalIgnoreCase));
                return creditMode != null && paymodeId == creditMode.PayModeID;
            }
            catch (Exception ex)
            {
                return paymodeId == 1; // Fallback to hardcoded check
            }
        }

        /// <summary>
        /// Checks if the given payment mode ID is a Cash payment mode
        /// </summary>
        /// <param name="paymodeId">Payment mode ID to check</param>
        /// <returns>True if it's a Cash payment mode</returns>
        private bool IsCashPaymentMode(int paymodeId)
        {
            try
            {
                Dropdowns dp = new Dropdowns();
                var pm = dp.PaymodeDDl();
                var cashMode = pm.List?.FirstOrDefault(p =>
                    p.PayModeName.Equals("Cash", StringComparison.OrdinalIgnoreCase));
                return cashMode != null && paymodeId == cashMode.PayModeID;
            }
            catch (Exception ex)
            {
                return paymodeId == 2; // Fallback to hardcoded check
            }
        }

        /// <summary>
        /// Validates and ensures the date is within SQL Server's valid range (1753-01-01 to 9999-12-31)
        /// </summary>
        private DateTime GetValidSqlDateTime(DateTime date)
        {
            DateTime minSqlDate = new DateTime(1753, 1, 1);
            DateTime maxSqlDate = new DateTime(9999, 12, 31);

            if (date < minSqlDate) return minSqlDate;
            if (date > maxSqlDate) return maxSqlDate;

            return date;
        }

        /// <summary>
        /// Populates common voucher properties
        /// </summary>
        private void PopulateBaseVoucherProperties(Voucher voucher, SalesMaster sales, DateTime voucherDate)
        {
            voucher.CompanyID = Convert.ToInt32(DataBase.CompanyId);
            voucher.BranchID = Convert.ToInt32(DataBase.BranchId);
            voucher.VoucherID = sales.VoucherID;
            voucher.VoucherSeriesID = 0;
            voucher.VoucherDate = voucherDate;
            voucher.VoucherType = VOUCHER_TYPE_SALES;
            voucher.Narration = $"SALES: #{sales.BillNo}| SALES WORTH:{sales.NetAmount}| REMARKS: ";
            // Store the bill-level payment mode on each voucher entry.
            // Detailed split-payment methods continue to live in SPaymentDetails.
            voucher.Mode = sales.PaymodeName ?? string.Empty;
            voucher.ModeID = sales.PaymodeId;
            voucher.UserDate = voucherDate;
            voucher.UserName = SessionContext.UserName;
            voucher.UserID = SessionContext.UserId;
            voucher.FinYearID = SessionContext.FinYearId;
            voucher.IsSyncd = false;
            voucher._Operation = "CREATE";
        }

        /// <summary>
        /// Creates a voucher entry in the database
        /// </summary>
        private void CreateVoucherEntry(Voucher voucher, IDbTransaction trans, string entryDescription)
        {
            List<Voucher> result = DataConnection.Query<Voucher>(
                STOREDPROCEDURE.POS_Vouchers,
                voucher,
                trans,
                commandType: CommandType.StoredProcedure
            ).ToList<Voucher>();
        }

        /// <summary>
        /// Calculates GST amounts grouped by tax percentage from sales details
        /// Returns a dictionary with tax percentage as key and total tax amount as value
        /// </summary>
        private Dictionary<double, double> CalculateGSTAmounts(DataGridView dgvItems)
        {
            Dictionary<double, double> gstAmounts = new Dictionary<double, double>();

            if (dgvItems == null || dgvItems.Rows.Count == 0)
                return gstAmounts;

            for (int i = 0; i < dgvItems.Rows.Count; i++)
            {
                try
                {
                    if (dgvItems.Rows[i].Cells["TaxPer"] == null || dgvItems.Rows[i].Cells["TaxPer"].Value == null)
                        continue;

                    double taxPer = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["TaxPer"], 0f);
                    double taxAmt = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["TaxAmt"], 0f);

                    if (taxPer > 0 && taxAmt > 0)
                    {
                        if (gstAmounts.ContainsKey(taxPer))
                        {
                            gstAmounts[taxPer] += taxAmt;
                        }
                        else
                        {
                            gstAmounts[taxPer] = taxAmt;
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }

            return gstAmounts;
        }

        /// <summary>
        /// Creates GST voucher entries (CGST and SGST) for each tax rate
        /// </summary>
        private void CreateGSTVoucherEntries(SalesMaster sales, Voucher voucher, IDbTransaction trans,
            Dictionary<double, double> gstTaxAmounts, DateTime voucherDate, ref int slNo)
        {
            const int GST_OUTPUT_GROUP_ID = 23; // Group ID for DUTIES & TAXES (as per POS_Branch stored procedure)

            foreach (var gstEntry in gstTaxAmounts)
            {
                double taxPercentage = gstEntry.Key;
                double totalTaxAmount = gstEntry.Value;

                // Split GST into CGST and SGST (50% each, guaranteeing exact sum equals totalTaxAmount)
                double cgstAmount = Math.Round(totalTaxAmount / 2.0, 2, MidpointRounding.AwayFromZero);
                double sgstAmount = Math.Round(totalTaxAmount - cgstAmount, 2, MidpointRounding.AwayFromZero);
                double cgstPercentage = taxPercentage / 2;
                double sgstPercentage = taxPercentage / 2;

                // Format percentage to match ledger names (remove trailing zeros)
                string cgstPercentageStr = cgstPercentage % 1 == 0 ? cgstPercentage.ToString("0") : cgstPercentage.ToString("0.#");
                string sgstPercentageStr = sgstPercentage % 1 == 0 ? sgstPercentage.ToString("0") : sgstPercentage.ToString("0.#");

                // Create CGST voucher entry
                PopulateBaseVoucherProperties(voucher, sales, voucherDate);
                voucher.GroupID = GST_OUTPUT_GROUP_ID;
                voucher.LedgerName = $"OUTPUT CGST {cgstPercentageStr}%";
                voucher.LedgerID = GetOrCreateGSTLedger(voucher.LedgerName, GST_OUTPUT_GROUP_ID, trans);
                voucher.Debit = 0;
                voucher.Credit = cgstAmount;
                voucher.SlNo = slNo++;

                CreateVoucherEntry(voucher, trans, $"VoucherID={voucher.VoucherID}, Type=CREDIT, Account={voucher.LedgerName}, Amount={cgstAmount}");

                // Create SGST voucher entry
                PopulateBaseVoucherProperties(voucher, sales, voucherDate);
                voucher.GroupID = GST_OUTPUT_GROUP_ID;
                voucher.LedgerName = $"OUTPUT SGST {sgstPercentageStr}%";
                voucher.LedgerID = GetOrCreateGSTLedger(voucher.LedgerName, GST_OUTPUT_GROUP_ID, trans);
                voucher.Debit = 0;
                voucher.Credit = sgstAmount;
                voucher.SlNo = slNo++;

                CreateVoucherEntry(voucher, trans, $"VoucherID={voucher.VoucherID}, Type=CREDIT, Account={voucher.LedgerName}, Amount={sgstAmount}");
            }
        }

        /// <summary>
        /// Gets or creates a GST ledger account
        /// </summary>
        private long GetOrCreateGSTLedger(string ledgerName, int groupId, IDbTransaction trans)
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
                try
                {
                    int nextLedgerId = 1;
                    using (SqlCommand idCmd = new SqlCommand("SELECT ISNULL(MAX(LedgerID), 0) + 1 FROM LedgerMaster", (SqlConnection)DataConnection, (SqlTransaction)trans))
                    {
                        nextLedgerId = Convert.ToInt32(idCmd.ExecuteScalar());
                    }

                    // Create the GST ledger using stored procedure
                    using (SqlCommand cmd = new SqlCommand("POS_Ledger", (SqlConnection)DataConnection, (SqlTransaction)trans))
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
                    }

                    // Now try to get the newly created ledger ID
                    ledgerId = ledgerRepository.GetLedgerId(ledgerName, groupId, Convert.ToInt32(DataBase.BranchId));
                    if (ledgerId > 0)
                    {
                        return ledgerId;
                    }
                }
                catch (Exception createEx)
                {
                }

                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        /// <summary>
        /// Creates voucher entries for cash sales (Debit Cash, Credit Sales)
        /// </summary>r(st pri
        private void CreateCashSaleVoucherEntries(SalesMaster sales, Voucher voucher, IDbTransaction trans, Dictionary<double, double> gstTaxAmounts)
        {
            DateTime voucherDate = GetValidSqlDateTime(DateTime.Now);
            int slNo = 1;

            // -------------------------------------------------------
            // DEBIT ENTRIES:
            // Use PendingPaymentDetails (set on this repository before SaveSales is called)
            // to post per-instrument debit entries.
            // e.g. UPI Rs 500 -> Dr SBI A/C Rs 500; Cash Rs 200 -> Dr CASH-IN-HAND Rs 200
            // SPaymentDetails rows are saved AFTER the transaction, so we cannot read from DB here.
            // Fallback to Cash ledger if no PendingPaymentDetails provided.
            // -------------------------------------------------------
            bool paymentDetailsPosted = false;
            try
            {
                if (PendingPaymentDetails != null && PendingPaymentDetails.Count > 0)
                {
                    foreach (var pd in PendingPaymentDetails)
                    {
                        double amount = (double)pd.Amount;
                        if (amount <= 0) continue;

                        // Resolve the ledger for this payment instrument via PayMode table
                        long ledgerId = 0;
                        string ledgerName = DefaultLedgers.CASH;
                        try
                        {
                            string sql = "SELECT TOP 1 pm.LedgerID, lm.LedgerName FROM PayMode pm LEFT JOIN LedgerMaster lm ON lm.LedgerID = pm.LedgerID WHERE pm.PayModeID = @PayModeID";
                            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection, (SqlTransaction)trans))
                            {
                                cmd.Parameters.AddWithValue("@PayModeID", pd.PaymodeId);
                                using (SqlDataReader rdr = cmd.ExecuteReader())
                                {
                                    if (rdr.Read())
                                    {
                                        if (rdr["LedgerID"] != DBNull.Value)
                                            ledgerId = Convert.ToInt64(rdr["LedgerID"]);
                                        if (rdr["LedgerName"] != DBNull.Value)
                                            ledgerName = rdr["LedgerName"].ToString();
                                    }
                                }
                            }
                        }
                        catch { }

                        if (ledgerId <= 0)
                        {
                            ledgerId = ledgerRepository.GetLedgerId(DefaultLedgers.CASH, (int)AccountGroup.CASH_IN_HAND, SessionContext.BranchId);
                            ledgerName = DefaultLedgers.CASH;
                        }

                        PopulateBaseVoucherProperties(voucher, sales, voucherDate);
                        voucher.GroupID = Convert.ToInt32(AccountGroup.CASH_IN_HAND);
                        voucher.LedgerID = ledgerId;
                        voucher.LedgerName = ledgerName;
                        voucher.Debit = Math.Round(amount, 2);
                        voucher.Credit = 0;
                        voucher.SlNo = slNo++;

                        CreateVoucherEntry(voucher, trans, $"VoucherID={voucher.VoucherID}, Type=DEBIT, Account={ledgerName}, Amount={voucher.Debit}");
                        paymentDetailsPosted = true;
                    } // end foreach
                } // end if PendingPaymentDetails
            }
            finally
            {
                // Null out property reference so it doesn't leak into subsequent saves, without mutating the caller's list
                PendingPaymentDetails = null;
            }

            // Fallback: no SPaymentDetails — debit Cash / mapped paymode ledger for full amount
            if (!paymentDetailsPosted)
            {
                long targetLedgerId = sales.PaymodeLedgerId;
                string targetLedgerName = DefaultLedgers.CASH;

                if (targetLedgerId <= 0 && sales.PaymodeId > 0)
                {
                    try
                    {
                        string sql = "SELECT TOP 1 LedgerID FROM PayMode WHERE PayModeID = @PayModeID";
                        using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection, (SqlTransaction)trans))
                        {
                            cmd.Parameters.AddWithValue("@PayModeID", sales.PaymodeId);
                            object res = cmd.ExecuteScalar();
                            if (res != null && res != DBNull.Value)
                                targetLedgerId = Convert.ToInt64(res);
                        }
                    }
                    catch { }
                }

                if (targetLedgerId > 0)
                {
                    try
                    {
                        string sqlL = "SELECT TOP 1 LedgerName FROM LedgerMaster WHERE LedgerID = @LedgerID";
                        using (SqlCommand cmdL = new SqlCommand(sqlL, (SqlConnection)DataConnection, (SqlTransaction)trans))
                        {
                            cmdL.Parameters.AddWithValue("@LedgerID", targetLedgerId);
                            object resL = cmdL.ExecuteScalar();
                            if (resL != null && resL != DBNull.Value)
                                targetLedgerName = resL.ToString();
                        }
                    }
                    catch { }
                }
                else
                {
                    targetLedgerId = ledgerRepository.GetLedgerId(DefaultLedgers.CASH, (int)AccountGroup.CASH_IN_HAND, SessionContext.BranchId);
                }

                PopulateBaseVoucherProperties(voucher, sales, voucherDate);
                voucher.GroupID = Convert.ToInt32(AccountGroup.CASH_IN_HAND);
                voucher.LedgerID = targetLedgerId;
                voucher.LedgerName = targetLedgerName;
                voucher.Debit = Netamount;
                voucher.Credit = 0;
                voucher.SlNo = slNo++;

                CreateVoucherEntry(voucher, trans, $"VoucherID={voucher.VoucherID}, Type=DEBIT, Account={targetLedgerName}, Amount={Netamount}");
            }

            // -------------------------------------------------------
            // CREDIT ENTRIES: Sales Account + GST
            // -------------------------------------------------------
            double totalGST = gstTaxAmounts.Values.Sum();
            double salesAmountWithoutGST = Netamount - totalGST;

            PopulateBaseVoucherProperties(voucher, sales, voucherDate);
            voucher.GroupID = Convert.ToInt32(AccountGroup.SALES_ACCOUNT);
            voucher.LedgerID = ledgerRepository.GetLedgerId(DefaultLedgers.SALE, (int)AccountGroup.SALES_ACCOUNT, SessionContext.BranchId);
            voucher.LedgerName = DefaultLedgers.SALE;
            voucher.Debit = 0;
            voucher.Credit = salesAmountWithoutGST;
            voucher.SlNo = slNo++;

            CreateVoucherEntry(voucher, trans, $"VoucherID={voucher.VoucherID}, Type=CREDIT, Account=Sales, Amount={salesAmountWithoutGST}");

            // Create GST voucher entries (CGST and SGST)
            CreateGSTVoucherEntries(sales, voucher, trans, gstTaxAmounts, voucherDate, ref slNo);
        }

        /// <summary>
        /// Creates voucher entries for credit sales (Debit Customer, Credit Sales)
        /// </summary>
        private void CreateCreditSaleVoucherEntries(SalesMaster sales, Voucher voucher, IDbTransaction trans, Dictionary<double, double> gstTaxAmounts)
        {
            DateTime voucherDate = GetValidSqlDateTime(DateTime.Now);
            int slNo = 1;

            // First entry - Debit Customer Account
            PopulateBaseVoucherProperties(voucher, sales, voucherDate);
            voucher.LedgerID = sales.LedgerID;
            voucher.LedgerName = sales.CustomerName;
            voucher.Debit = Netamount;
            voucher.Credit = 0;
            voucher.SlNo = slNo++;

            CreateVoucherEntry(voucher, trans, $"VoucherID={voucher.VoucherID}, Type=DEBIT, Customer={voucher.LedgerName}, Amount={Netamount}");

            // Calculate sales amount without GST
            double totalGST = gstTaxAmounts.Values.Sum();
            double salesAmountWithoutGST = Netamount - totalGST;

            // Second entry - Credit Sales Account (without GST)
            PopulateBaseVoucherProperties(voucher, sales, voucherDate);
            voucher.GroupID = Convert.ToInt32(AccountGroup.SALES_ACCOUNT);
            voucher.LedgerID = ledgerRepository.GetLedgerId(DefaultLedgers.SALE, (int)AccountGroup.SALES_ACCOUNT, SessionContext.BranchId);
            voucher.LedgerName = DefaultLedgers.SALE;
            voucher.Debit = 0;
            voucher.Credit = salesAmountWithoutGST;
            voucher.SlNo = slNo++;

            CreateVoucherEntry(voucher, trans, $"VoucherID={voucher.VoucherID}, Type=CREDIT, Account=Sales, Amount={salesAmountWithoutGST}");

            // Create GST voucher entries (CGST and SGST)
            CreateGSTVoucherEntries(sales, voucher, trans, gstTaxAmounts, voucherDate, ref slNo);
        }

        #endregion

        public string DeleteSales(int billNo, int voucherId)
        {
            try
            {
                // Get session context values
                int finYearId = SessionContext.FinYearId;
                int branchId = SessionContext.BranchId;

                DataConnection.Open();
                using (SqlTransaction trans = (SqlTransaction)DataConnection.BeginTransaction())
                {
                    try
                    {
                        // Use the stored procedure for deletion
                        using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_Sales_Win, (SqlConnection)DataConnection, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            // Add all required parameters
                            cmd.Parameters.AddWithValue("@CompanyId", Convert.ToInt32(DataBase.CompanyId));
                            cmd.Parameters.AddWithValue("@BranchId", branchId);
                            cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                            cmd.Parameters.AddWithValue("@BillNo", billNo);
                            cmd.Parameters.AddWithValue("@VoucherID", voucherId);
                            cmd.Parameters.AddWithValue("@VoucherType", "Sales");
                            cmd.Parameters.AddWithValue("@_Operation", "DELETE");
                            cmd.Parameters.AddWithValue("@CancelFlag", 1); // Set to true for deletion
                            // Execute and check result
                            object result = cmd.ExecuteScalar();

                            if (result != null && result.ToString() == "SUCCESS")
                            {
                                trans.Commit();
                                return "Sales invoice deleted successfully!";
                            }
                            else
                            {
                                trans.Rollback();
                                return "Failed to delete sales invoice. Operation returned: " + (result ?? "null");
                            }
                        }
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                return "Error deleting sales invoice: " + ex.Message;
            }
            finally
            {
                if (DataConnection != null && DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        #region Calculation Helpers

        /// <summary>
        /// Calculates discount amount based on percentage or fixed amount
        /// </summary>
        public float CalculateDiscountAmount(float subtotal, string discountText)
        {
            float discountAmount = 0;

            if (!string.IsNullOrEmpty(discountText))
            {
                // Check if it's a percentage discount (ends with %)
                if (discountText.EndsWith("%"))
                {
                    string percentageStr = discountText.TrimEnd('%');
                    if (float.TryParse(percentageStr, out float discountPercentage))
                    {
                        discountAmount = subtotal * (discountPercentage / 100f);
                    }
                }
                // Otherwise assume it's a flat amount
                else if (float.TryParse(discountText, out float flatDiscount))
                {
                    discountAmount = flatDiscount;
                }
            }

            return discountAmount;
        }

        /// <summary>
        /// Applies rounding to the net amount
        /// </summary>
        public float ApplyRounding(float netAmount)
        {
            // Round to nearest whole rupee (standard rounding)
            float roundedValue = (float)Math.Round(netAmount, 0, MidpointRounding.AwayFromZero);
            return roundedValue;
        }

        /// <summary>
        /// Calculates the rounding difference
        /// </summary>
        public float CalculateRoundingAmount(float netAmount)
        {
            float roundedValue = ApplyRounding(netAmount);
            return roundedValue - netAmount;
        }

        /// <summary>
        /// Converts a data table (such as from an UltraGrid) to DataGridView for compatibility with older methods
        /// </summary>
        public DataGridView ConvertDataTableToDataGridView(DataTable sourceTable)
        {
            DataGridView dataGridView = new DataGridView();
            dataGridView.AllowUserToAddRows = false; // Prevent an empty row at the bottom

            try
            {
                // Check if the source table is valid
                if (sourceTable == null)
                    return dataGridView;

                // First add all necessary columns to match what's expected in the repository
                dataGridView.Columns.Add("ItemId", "ItemId");
                dataGridView.Columns.Add("BarCode", "BarCode");
                dataGridView.Columns.Add("ItemName", "ItemName");
                dataGridView.Columns.Add("SlNO", "SlNO");
                dataGridView.Columns.Add("Unit", "Unit");
                dataGridView.Columns.Add("UnitId", "UnitId");
                dataGridView.Columns.Add("IsBaseUnit", "IsBaseUnit");
                dataGridView.Columns.Add("Qty", "Qty");
                dataGridView.Columns.Add("UnitPrice", "UnitPrice");
                dataGridView.Columns.Add("Cost", "Cost");
                dataGridView.Columns.Add("DiscPer", "DiscPer");
                dataGridView.Columns.Add("DiscAmt", "DiscAmt");
                dataGridView.Columns.Add("TaxPer", "TaxPer");
                dataGridView.Columns.Add("TaxAmt", "TaxAmt");
                dataGridView.Columns.Add("TaxType", "TaxType");
                dataGridView.Columns.Add("Amount", "Amount");
                dataGridView.Columns.Add("TotalAmount", "TotalAmount");
                dataGridView.Columns.Add("Marginper", "Marginper");
                dataGridView.Columns.Add("MarginAmt", "MarginAmt");
                dataGridView.Columns.Add("MRP", "MRP");
                dataGridView.Columns.Add("Packing", "Packing");
                dataGridView.Columns.Add("BaseAmount", "BaseAmount");

                // Now copy all rows from the source DataTable to the DataGridView
                foreach (DataRow row in sourceTable.Rows)
                {
                    int rowIndex = dataGridView.Rows.Add();
                    DataGridViewRow dgvRow = dataGridView.Rows[rowIndex];

                    // Copy values to the appropriate columns with safe handling of missing columns
                    foreach (DataGridViewColumn col in dataGridView.Columns)
                    {
                        if (sourceTable.Columns.Contains(col.Name) && row[col.Name] != DBNull.Value)
                        {
                            dgvRow.Cells[col.Name].Value = row[col.Name];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return dataGridView;
        }

        /// <summary>
        /// Calculates total amounts from a DataGridView containing sales items
        /// </summary>
        public float CalculateSubTotal(DataGridView dgvItems)
        {
            float subTotal = 0;

            for (int i = 0; i < dgvItems.Rows.Count; i++)
            {
                // Use BaseAmount for tax-excluded subtotal if available, else fall back to TotalAmount
                if (dgvItems.Columns.Contains("BaseAmount") && dgvItems.Rows[i].Cells["BaseAmount"].Value != null)
                {
                    float baseAmount = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["BaseAmount"], 0f);
                    if (baseAmount > 0)
                    {
                        subTotal += baseAmount;
                        continue;
                    }
                }

                if (dgvItems.Rows[i].Cells["TotalAmount"].Value != null)
                {
                    float totalAmount = GridParseHelpers.GetFloatValue(dgvItems.Rows[i].Cells["TotalAmount"], 0f);
                    subTotal += totalAmount;
                }
            }

            return subTotal;
        }

        /// <summary>
        /// Calculates total amounts from a DataTable containing sales items
        /// </summary>
        public float CalculateSubTotal(DataTable itemsTable)
        {
            float subTotal = 0;

            if (itemsTable != null)
            {
                bool hasBaseAmount = itemsTable.Columns.Contains("BaseAmount");
                bool hasTotalAmount = itemsTable.Columns.Contains("TotalAmount");

                foreach (DataRow row in itemsTable.Rows)
                {
                    if (hasBaseAmount && row["BaseAmount"] != DBNull.Value)
                    {
                        float baseAmount = (float)ParseDouble(row["BaseAmount"], 0);
                        if (baseAmount > 0)
                        {
                            subTotal += baseAmount;
                            continue;
                        }
                    }

                    if (hasTotalAmount && row["TotalAmount"] != DBNull.Value)
                    {
                        subTotal += (float)ParseDouble(row["TotalAmount"], 0);
                    }
                }
            }

            return subTotal;
        }

        /// <summary>
        /// Adds an item to the sales grid
        /// </summary>
        public void AddItemToGrid(DataTable dt, ItemDDl item, float qty, string priceLevel)
        {
            if (item == null || dt == null) return;

            // Check if this item already exists in the grid
            bool itemExists = false;

            // Only check for duplicates if we are NOT in SeparateRows mode
            if (SessionContext.DuplicateItemBehavior != "SeparateRows")
            {
                foreach (DataRow row in dt.Rows)
                {
                    // Use both ItemId and BarCode for comparison to handle items without barcodes
                    string existingItemId = row["ItemId"].ToString();
                    string existingBarcode = row["BarCode"].ToString();

                    if (existingItemId == item.ItemId.ToString() && existingBarcode == item.BarCode)
                    {
                        // Item exists, update quantity
                        float currentQty = float.Parse(row["Qty"].ToString());
                        float newQty = currentQty + qty;
                        row["Qty"] = newQty;

                        float unitPrice = float.Parse(row["UnitPrice"].ToString());
                        float discPer = float.Parse(row["DiscPer"].ToString());
                        float packing = 1; // Default packing
                        if (dt.Columns.Contains("Packing") && row["Packing"] != null && row["Packing"] != DBNull.Value)
                        {
                            float.TryParse(row["Packing"].ToString(), out packing);
                        }

                        // Get current selling price to preserve it
                        float currentSellingPrice = float.Parse(row["Amount"].ToString());

                        // Calculate discount and total using current selling price
                        float discAmt = (newQty * currentSellingPrice) * (discPer / 100);

                        // Get tax information - ALWAYS use inclusive logic for retail mode
                        float taxPer = float.Parse(row["TaxPer"].ToString());
                        // Calculate tax and base amount for the updated quantity
                        float taxAmountPerUnit = 0;
                        float baseAmountPerUnit = currentSellingPrice;
                        if (taxPer > 0 && DataBase.IsTaxEnabled)
                        {
                            // Back-calculate tax from selling price: Base = Price / (1 + TaxRate), Tax = Price - Base
                            double divisor = 1.0 + (taxPer / 100.0);
                            baseAmountPerUnit = divisor > 0 ? (float)(currentSellingPrice / divisor) : currentSellingPrice;
                            taxAmountPerUnit = (float)(currentSellingPrice - baseAmountPerUnit);
                        }

                        // Ensure TaxType is set to inclusive
                        row["TaxType"] = "incl";

                        float totalAmount = (newQty * currentSellingPrice) - discAmt;
                        float baseAmount = (float)Math.Round(newQty * baseAmountPerUnit, 2);
                        float taxAmount = (float)Math.Round(newQty * taxAmountPerUnit, 2);

                        row["DiscAmt"] = discAmt;
                        row["TaxAmt"] = taxAmount;
                        row["BaseAmount"] = baseAmount;
                        row["Amount"] = currentSellingPrice; // Keep existing SellingPrice unchanged
                        row["TotalAmount"] = totalAmount;

                        itemExists = true;
                        break;
                    }
                }
            }

            if (!itemExists)
            {
                // Add as new item
                DataRow newRow = dt.NewRow();
                newRow["SlNO"] = dt.Rows.Count + 1;
                newRow["ItemId"] = item.ItemId;
                newRow["BarCode"] = item.BarCode;
                newRow["ItemName"] = item.Description;
                newRow["UnitId"] = item.UnitId;
                newRow["Unit"] = item.Unit;
                newRow["IsBaseUnit"] = false;
                newRow["Qty"] = qty;
                newRow["Packing"] = item.Packing; // Add packing from item master

                // Get price based on selected price level
                // FIXED: Corrected reversed price mapping - Item Master saves txt_Retail to WholeSalePrice and txt_Walkin to RetailPrice
                float unitPrice = 0;
                if (priceLevel == "RetailPrice")
                    unitPrice = (float)item.WholeSalePrice; // Actual Retail Price is stored in WholeSalePrice field
                else if (priceLevel == "WholesalePrice")
                    unitPrice = (float)item.RetailPrice; // Actual Wholesale (Walking) Price is stored in RetailPrice field
                else if (priceLevel == "CreditPrice")
                    unitPrice = (float)item.CreditPrice;
                else if (priceLevel == "CardPrice")
                    unitPrice = (float)item.CardPrice;
                else if (priceLevel == "MRP")
                    unitPrice = (float)item.MRP;
                else if (priceLevel == "StaffPrice")
                    unitPrice = (float)item.StaffPrice;
                else if (priceLevel == "MinPrice")
                    unitPrice = (float)item.MinPrice;

                newRow["UnitPrice"] = unitPrice;
                newRow["Cost"] = item.Cost;
                newRow["DiscPer"] = 0;
                newRow["DiscAmt"] = 0;
                newRow["TaxPer"] = item.TaxPer;

                // ALWAYS use inclusive tax logic for retail/supermarket mode
                // The displayed price should be the final price the customer pays
                // This ensures that if an item is priced at $10.00, the customer pays exactly $10.00
                float baseAmountPerUnit;
                float taxAmount = 0;

                // Back-calculate base amount from unit price: Base = Price / (1 + TaxRate)
                double divisor = 1.0 + (item.TaxPer / 100.0);
                baseAmountPerUnit = divisor > 0 ? (float)(unitPrice / divisor) : unitPrice;
                if (item.TaxPer > 0 && DataBase.IsTaxEnabled)
                {
                    // Tax = Price - Base
                    taxAmount = (float)Math.Round(unitPrice - baseAmountPerUnit, 2);
                }

                // Force tax type to inclusive for retail mode
                newRow["TaxType"] = "incl";

                // Calculate base amount for the line (rounded to 2 decimals)
                float baseAmount = (float)Math.Round(qty * baseAmountPerUnit, 2);
                newRow["BaseAmount"] = baseAmount; // Store taxable value for GST compliance
                newRow["TaxAmt"] = (float)Math.Round(taxAmount * qty, 2); // Tax for the line

                // Set SellingPrice (Amount) = UnitPrice (Retail Price) by default
                newRow["Amount"] = unitPrice;

                // Calculate total amount (rounded to 2 decimals)
                // Since we're always using inclusive tax, price already includes tax
                float totalAmount = (float)Math.Round(qty * unitPrice, 2);
                newRow["TotalAmount"] = totalAmount;

                // Calculate margin
                // MarginPer is calculated per unit: (UnitPrice - Cost) / UnitPrice × 100
                // MarginAmt is calculated on total: Amount × MarginPer / 100
                float cost = (float)item.Cost;
                float marginPerUnit = unitPrice - cost;
                float marginPer = unitPrice > 0 ? (marginPerUnit / unitPrice) * 100 : 0;
                float lineTotal = qty * unitPrice;
                float marginAmt = lineTotal * (marginPer / 100); // MarginAmt = Amount × MarginPer / 100
                newRow["Marginper"] = marginPer;
                newRow["MarginAmt"] = marginAmt;

                dt.Rows.Add(newRow);
            }
        }

        /// <summary>
        /// Checks if a given barcode exists in the grid and updates quantity if found
        /// For weight item barcodes (starting with $), this method should not be called directly
        /// as they are handled separately in ProcessWeightItemBarcode
        /// </summary>
        public bool CheckBarcodeExists(DataTable dt, string barcode, ref int rowIndex)
        {
            if (dt == null || string.IsNullOrEmpty(barcode)) return false;

            // Skip weight item barcodes (they are handled separately)
            if (barcode.StartsWith("$"))
            {
                return false;
            }

            // Check POS Settings: If DuplicateItemBehavior is "SeparateRows", 
            // don't merge quantities - return false so a new row is added
            if (SessionContext.DuplicateItemBehavior == "SeparateRows")
            {
                rowIndex = -1;
                return false;
            }

            rowIndex = -1;
            bool found = false;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["BarCode"].ToString() == barcode)
                {
                    // Found the barcode, update quantity
                    float currentQty = float.Parse(dt.Rows[i]["Qty"].ToString());
                    float newQty = currentQty + 1;
                    dt.Rows[i]["Qty"] = newQty;

                    float unitPrice = float.Parse(dt.Rows[i]["UnitPrice"].ToString());
                    float discPer = float.Parse(dt.Rows[i]["DiscPer"].ToString());
                    float packing = 1; // Default packing
                    if (dt.Columns.Contains("Packing") && dt.Rows[i]["Packing"] != null && dt.Rows[i]["Packing"] != DBNull.Value)
                    {
                        float.TryParse(dt.Rows[i]["Packing"].ToString(), out packing);
                    }

                    // Get current selling price to preserve it
                    float currentSellingPrice = float.Parse(dt.Rows[i]["Amount"].ToString());

                    // Get tax info for proper GST calculation
                    float taxPer = 0;
                    if (dt.Columns.Contains("TaxPer") && dt.Rows[i]["TaxPer"] != null && dt.Rows[i]["TaxPer"] != DBNull.Value)
                    {
                        float.TryParse(dt.Rows[i]["TaxPer"].ToString(), out taxPer);
                    }

                    // Calculate discount and total using current selling price
                    float lineTotal = newQty * currentSellingPrice;
                    float discAmt = lineTotal * (discPer / 100);
                    float totalAmount = lineTotal - discAmt;

                    // Calculate BaseAmount and TaxAmt (GST-compliant for inclusive tax)
                    float baseAmount = totalAmount;
                    float taxAmt = 0;
                    if (taxPer > 0 && DataBase.IsTaxEnabled)
                    {
                        // Back-calculate: Base = Total / (1 + TaxRate)
                        double divisor = 1.0 + (taxPer / 100.0);
                        baseAmount = divisor > 0 ? (float)(totalAmount / divisor) : totalAmount;
                        taxAmt = totalAmount - baseAmount;
                    }

                    dt.Rows[i]["DiscAmt"] = discAmt;
                    dt.Rows[i]["Amount"] = currentSellingPrice; // Keep existing SellingPrice unchanged
                    dt.Rows[i]["TotalAmount"] = totalAmount;
                    dt.Rows[i]["BaseAmount"] = baseAmount;
                    dt.Rows[i]["TaxAmt"] = taxAmt;

                    rowIndex = i;
                    found = true;
                    break;
                }
            }

            return found;
        }

        #endregion

        #region GetSoldItemHistory
        /// <summary>
        /// Gets the sold item history for a specific item
        /// </summary>
        public List<SoldItemHistory> GetSoldItemHistory(int itemId, int branchId, int companyId, int finYearId)
        {
            try
            {
                DataConnection.Open();

                var parameters = new DynamicParameters();
                parameters.Add("ItemId", itemId);
                parameters.Add("BranchID", branchId);
                parameters.Add("CompanyId", companyId);
                parameters.Add("FinYearId", finYearId);
                parameters.Add("_Operation", OPERATION_SOLDITEMHISTORY);

                List<SoldItemHistory> result = DataConnection.Query<SoldItemHistory>(
                    STOREDPROCEDURE._POS_SDetails_Win,
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }
        #endregion
    }
}
