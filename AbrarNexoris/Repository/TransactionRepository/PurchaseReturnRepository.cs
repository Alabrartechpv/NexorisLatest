using ModelClass.TransactionModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModelClass;
using Repository;
using Repository.MasterRepositry;

namespace Repository.TransactionRepository
{
    public class PurchaseReturnRepository : BaseRepostitory
    {
        // Static field to store the active transaction for UpdatePurchaseReturnDetails
        private static SqlTransaction activeTransaction = null;

        // Add LedgerRepository instance for tax ledger operations
        LedgerRepository objLedgerRepository = new LedgerRepository();

        /// <summary>
        /// Helper method to get purchase return ledger ID
        /// </summary>
        private int GetPurchaseReturnLedgerId(int branchId)
        {
            try
            {
                LedgerRepository ledgerRepo = new LedgerRepository();
                int purchaseReturnLedgerId = ledgerRepo.GetLedgerId(DefaultLedgers.PURCHASERETURN, (int)AccountGroup.PURCHASE_ACCOUNT, branchId > 0 ? branchId : Convert.ToInt32(DataBase.BranchId));
                if (purchaseReturnLedgerId > 0)
                {
                    return purchaseReturnLedgerId;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting purchase return ledger ID: {ex.Message}");
            }
            return 0; // Return 0 if not found, let the calling code handle it
        }

        public PReturnMaster GetById(Int64 Id)
        {
            PReturnMaster item = new PReturnMaster();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_PurchaseReturn, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", Id);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            item = ds.Tables[0].Rows[0].ToNullableObject<PReturnMaster>();
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

        public PReturnDetailsGrid GetByIdPRD(Int64 Id)
        {
            PReturnDetailsGrid item = new PReturnDetailsGrid();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_PurchaseReturn, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", Id);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 1) && (ds.Tables[1] != null) && (ds.Tables[1].Rows.Count > 0))
                        {
                            item.List = ds.Tables[1].ToListOfObject<PReturnDetails>();
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

        public int PReturnNo = 0;
        public int GeneratePReturnNo(SqlTransaction trans = null)
        {
            int generatedNo = 0;

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                // Use the stored procedure which reads from TrackTrans for the
                // specific BranchID + FinYearID — keeping each branch's counter isolated.
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_PurchaseReturn, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Transaction = trans;
                    cmd.Parameters.AddWithValue("@BranchId", Convert.ToInt32(DataBase.BranchId));
                    cmd.Parameters.AddWithValue("@FinYearId", Convert.ToInt32(DataBase.FinyearId));
                    cmd.Parameters.AddWithValue("@_Operation", "GENERATENUMBER");

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        generatedNo = Convert.ToInt32(result);
                        System.Diagnostics.Debug.WriteLine($"GeneratePReturnNo (SP): Branch={DataBase.BranchId}, FinYear={DataBase.FinyearId}, Next={generatedNo}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GeneratePReturnNo: {ex.Message}");
            }
            finally
            {
                if (trans == null && DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            PReturnNo = generatedNo > 0 ? generatedNo : 1;
            return PReturnNo;
        }

        public string savePR(PReturnMaster pr, PReturnDetails details, DataGridView dgvInvoice)
        {
            // Check if this is a direct return (not linked to a purchase invoice)
            bool isDirectReturn = false;
            if (pr.PInvoice == "WITHOUT GR" || pr.InvoiceNo == "WITHOUT GR")
            {
                isDirectReturn = true;
                System.Diagnostics.Debug.WriteLine("Direct return detected (WITHOUT GR)");
            }

            // For direct returns, ensure a default vendor is set if none was selected
            if (isDirectReturn && pr.LedgerID <= 0)
            {
                try
                {
                    LedgerRepository ledgerRepo = new LedgerRepository();
                    int defaultVendorLedgerId = ledgerRepo.GetLedgerId(DefaultLedgers.DEFAULTCUSTOMER, (int)AccountGroup.SUNDRY_CREDITORS, pr.BranchId > 0 ? pr.BranchId : Convert.ToInt32(DataBase.BranchId));
                    if (defaultVendorLedgerId > 0)
                    {
                        pr.LedgerID = defaultVendorLedgerId;
                    }
                    else
                    {
                        // Try to get purchase ledger as fallback
                        defaultVendorLedgerId = ledgerRepo.GetLedgerId(DefaultLedgers.PURCHASE, (int)AccountGroup.PURCHASE_ACCOUNT, pr.BranchId > 0 ? pr.BranchId : Convert.ToInt32(DataBase.BranchId));
                        if (defaultVendorLedgerId > 0)
                        {
                            pr.LedgerID = defaultVendorLedgerId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting default vendor ledger: {ex.Message}");
                }
                pr.VendorName = "Direct Return";
                System.Diagnostics.Debug.WriteLine("Using default vendor for direct return");
            }

            // Validate grid data - check if Reason column is filled for all rows
            bool hasEmptyReason = false;
            string firstEmptyReasonItem = "";

            for (int i = 0; i < dgvInvoice.Rows.Count; i++)
            {
                // Skip rows that are empty or in edit mode
                if (dgvInvoice.Rows[i].IsNewRow || dgvInvoice.Rows[i].Cells["ItemID"].Value == null)
                    continue;

                // Check if the row is selected via the SELECT column
                bool isSelected = false;
                if (dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "SELECT"))
                {
                    var selectValue = dgvInvoice.Rows[i].Cells["SELECT"].Value;
                    isSelected = selectValue != null && selectValue != DBNull.Value && Convert.ToBoolean(selectValue);
                }

                // Only validate reason for selected rows
                if (isSelected)
                {
                    // Check if Reason column exists and is not empty
                    if (dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "Reason"))
                    {
                        var reasonValue = dgvInvoice.Rows[i].Cells["Reason"].Value;
                        if (reasonValue == null || reasonValue == DBNull.Value || string.IsNullOrWhiteSpace(reasonValue.ToString()))
                        {
                            hasEmptyReason = true;

                            // Get the item description for the error message
                            string itemDesc = "";
                            if (dgvInvoice.Rows[i].Cells["Description"].Value != null)
                            {
                                itemDesc = dgvInvoice.Rows[i].Cells["Description"].Value.ToString();
                            }
                            else if (dgvInvoice.Rows[i].Cells["ItemID"].Value != null)
                            {
                                itemDesc = "Item ID: " + dgvInvoice.Rows[i].Cells["ItemID"].Value.ToString();
                            }

                            if (string.IsNullOrEmpty(firstEmptyReasonItem))
                            {
                                firstEmptyReasonItem = itemDesc;
                            }
                        }
                    }
                }
            }

            if (hasEmptyReason)
            {
                System.Diagnostics.Debug.WriteLine("Reason not provided for some selected items");
                return $"Error: Reason is mandatory for all selected items. Please provide a reason for {firstEmptyReasonItem}.";
            }

            // Ensure FinYearId is valid (not 0) before saving to the database
            if (pr.FinYearId <= 0)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("FinYearId is 0 or negative. Attempting to get active FinYear from database");

                    // First try to get the active FinYear from database
                    DataConnection.Open();

                    string finYearQuery = @"
                        SELECT TOP 1 Id 
                        FROM Finyears 
                        WHERE IsActive = 1 
                        ORDER BY Id DESC";

                    using (SqlCommand finYearCmd = new SqlCommand(finYearQuery, (SqlConnection)DataConnection))
                    {
                        object result = finYearCmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            pr.FinYearId = Convert.ToInt32(result);
                            System.Diagnostics.Debug.WriteLine($"Retrieved FinYearId from database: {pr.FinYearId}");
                        }
                    }

                    // If still invalid, try to get from DataBase.FinyearId
                    if (pr.FinYearId <= 0)
                    {
                        try
                        {
                            int dbFinYear = Convert.ToInt32(DataBase.FinyearId);
                            if (dbFinYear > 0)
                            {
                                pr.FinYearId = dbFinYear;
                                System.Diagnostics.Debug.WriteLine($"Using DataBase.FinyearId: {pr.FinYearId}");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error converting DataBase.FinyearId: {ex.Message}");
                        }
                    }

                    // As a last resort, use a dynamic session value or default to 1
                    if (pr.FinYearId <= 0)
                    {
                        pr.FinYearId = SessionContext.FinYearId > 0 ? SessionContext.FinYearId : (int.TryParse(DataBase.FinyearId, out var id) ? id : 1);
                        System.Diagnostics.Debug.WriteLine($"Using default/session FinYearId: {pr.FinYearId}");
                    }

                    DataConnection.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error retrieving FinYearId: {ex.Message}");
                    if (DataConnection.State == ConnectionState.Open)
                        DataConnection.Close();

                    // Set a default value
                    pr.FinYearId = SessionContext.FinYearId > 0 ? SessionContext.FinYearId : (int.TryParse(DataBase.FinyearId, out var id) ? id : 1);
                }
            }

            System.Diagnostics.Debug.WriteLine($"Final FinYearId for master record: {pr.FinYearId}");

            DataConnection.Open();
            using (SqlTransaction trans = (SqlTransaction)DataConnection.BeginTransaction())
            {
                try
                {
                    // Ensure CompanyId is properly set
                    if (pr.CompanyId <= 0)
                    {
                        pr.CompanyId = Convert.ToInt32(DataBase.CompanyId);
                    }

                    // Ensure BranchId is properly set
                    if (pr.BranchId <= 0)
                    {
                        pr.BranchId = Convert.ToInt32(DataBase.BranchId);
                    }

                    // Ensure PaymodeLedgerID is properly set from PayMode table
                    if (pr.PaymodeLedgerID <= 0 && pr.PaymodeID > 0)
                    {
                        try
                        {
                            // Get ledger ID for the selected payment mode from PayMode table
                            string ledgerQuery = @"
                                SELECT TOP 1 LedgerID 
                                FROM PayMode 
                                WHERE PayModeID = @PaymodeID";

                            using (SqlCommand ledgerCmd = new SqlCommand(ledgerQuery, (SqlConnection)DataConnection, trans))
                            {
                                ledgerCmd.Parameters.AddWithValue("@PaymodeID", pr.PaymodeID);
                                object ledgerResult = ledgerCmd.ExecuteScalar();
                                if (ledgerResult != null && ledgerResult != DBNull.Value)
                                {
                                    pr.PaymodeLedgerID = Convert.ToInt32(ledgerResult);
                                    System.Diagnostics.Debug.WriteLine($"Retrieved PaymodeLedgerID: {pr.PaymodeLedgerID} for PaymodeID: {pr.PaymodeID}");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"PaymodeLedgerID not found in PayMode table for PaymodeID: {pr.PaymodeID}");
                                }
                            }
                        }
                        catch (Exception ledgerEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error getting PaymodeLedgerID: {ledgerEx.Message}");
                        }
                    }

                    // Generate the Voucher ID first (similar to SalesReturnRepository approach)
                    int voucherId = 0;
                    using (SqlCommand voucherCmd = new SqlCommand("POS_Vouchers", (SqlConnection)DataConnection, trans))
                    {
                        voucherCmd.CommandType = CommandType.StoredProcedure;
                        voucherCmd.Parameters.AddWithValue("@CompanyID", pr.CompanyId > 0 ? pr.CompanyId : Convert.ToInt32(DataBase.CompanyId));
                        voucherCmd.Parameters.AddWithValue("@BranchID", pr.BranchId > 0 ? pr.BranchId : Convert.ToInt32(DataBase.BranchId));
                        voucherCmd.Parameters.AddWithValue("@FinYearID", pr.FinYearId > 0 ? pr.FinYearId : Convert.ToInt32(DataBase.FinyearId));
                        voucherCmd.Parameters.AddWithValue("@VoucherType", ModelClass.VoucherType.DebitNote);
                        voucherCmd.Parameters.AddWithValue("@_Operation", "GENERATENUMBER");

                        using (SqlDataAdapter da = new SqlDataAdapter(voucherCmd))
                        {
                            DataSet ds = new DataSet();
                            da.Fill(ds);
                            if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                            {
                                voucherId = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                                pr.VoucherID = voucherId;
                                System.Diagnostics.Debug.WriteLine($"Generated voucher ID: {voucherId}");
                            }
                        }
                    }

                    if (voucherId <= 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to generate voucher ID");
                        return "Error: Failed to generate voucher ID";
                    }

                    // Check if there's an existing PR for this purchase number (only if not "WITHOUT GR")
                    int existingPRNo = 0;
                    int purchaseNoForCheck = 0;
                    bool isDirectReturnCheck = (pr.PInvoice == "WITHOUT GR" || pr.InvoiceNo == "WITHOUT GR");

                    if (!isDirectReturnCheck)
                    {
                        // Try to get purchase number from PInvoice first
                        if (!string.IsNullOrEmpty(pr.PInvoice) && int.TryParse(pr.PInvoice, out purchaseNoForCheck) && purchaseNoForCheck > 0)
                        {
                            existingPRNo = GetExistingPRByPurchaseNo(purchaseNoForCheck, pr.CompanyId, pr.FinYearId, pr.BranchId);
                        }
                        // If PInvoice doesn't have a valid purchase number, try InvoiceNo
                        else if (!string.IsNullOrEmpty(pr.InvoiceNo) && int.TryParse(pr.InvoiceNo, out purchaseNoForCheck) && purchaseNoForCheck > 0)
                        {
                            existingPRNo = GetExistingPRByPurchaseNo(purchaseNoForCheck, pr.CompanyId, pr.FinYearId, pr.BranchId);
                        }
                    }

                    int prNo;
                    bool isUpdate = false;

                    if (existingPRNo > 0)
                    {
                        // Use existing PR number - we'll update it instead of creating new
                        prNo = existingPRNo;
                        isUpdate = true;
                        System.Diagnostics.Debug.WriteLine($"Found existing PR#{prNo} for Purchase#{purchaseNoForCheck}, will update instead of creating new");

                        // Get the existing PR master record to preserve important fields
                        int existingPRId = GetIdByPReturnNo(prNo);
                        if (existingPRId > 0)
                        {
                            PReturnMaster existingPR = GetById(existingPRId);
                            if (existingPR != null)
                            {
                                // Preserve existing voucher ID and other important fields
                                pr.VoucherID = existingPR.VoucherID;
                                pr.SeriesID = existingPR.SeriesID > 0 ? existingPR.SeriesID : 1;
                                System.Diagnostics.Debug.WriteLine($"Preserved VoucherID: {pr.VoucherID}, SeriesID: {pr.SeriesID} from existing PR");
                            }
                        }
                    }
                    else
                    {
                        // Generate new PR number
                        prNo = GeneratePReturnNo(trans);
                        if (prNo <= 0)
                        {
                            System.Diagnostics.Debug.WriteLine("Failed to generate PR number from GeneratePReturnNo method");
                            return "Error: Failed to generate PR number";
                        }
                        System.Diagnostics.Debug.WriteLine($"Generated new PR number: {prNo}");

                        // Assign the generated voucher ID to the master object
                        pr.VoucherID = voucherId;
                        System.Diagnostics.Debug.WriteLine($"Assigned voucher ID: {voucherId} to PR# {prNo}");
                    }

                    pr.PReturnNo = prNo; // Set the PR number in the master object

                    // If updating existing PR, skip the duplicate check and use UPDATE operation
                    if (!isUpdate)
                    {
                        // Now check if a record with this combination already exists to avoid primary key violation
                        try
                        {
                            string checkExistingQuery = @"
                                SELECT COUNT(*) 
                                FROM PReturnMaster 
                                WHERE CompanyId = @CompanyId 
                                AND FinYearId = @FinYearId 
                                AND BranchId = @BranchId 
                                AND PReturnNo = @PReturnNo";

                            using (SqlCommand checkCmd = new SqlCommand(checkExistingQuery, (SqlConnection)DataConnection, trans))
                            {
                                checkCmd.Parameters.AddWithValue("@CompanyId", pr.CompanyId);
                                checkCmd.Parameters.AddWithValue("@FinYearId", pr.FinYearId);
                                checkCmd.Parameters.AddWithValue("@BranchId", pr.BranchId);
                                checkCmd.Parameters.AddWithValue("@PReturnNo", pr.PReturnNo);

                                int existingCount = Convert.ToInt32(checkCmd.ExecuteScalar());
                                if (existingCount > 0)
                                {
                                    // Record with this key combination already exists, try incrementing PR number
                                    prNo++;
                                    pr.PReturnNo = prNo;
                                    System.Diagnostics.Debug.WriteLine($"Detected duplicate key, incremented PR number to: {prNo}");
                                }
                            }
                        }
                        catch (Exception checkEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error checking for existing records: {checkEx.Message}");
                            // Continue even if the check fails
                        }
                    }

                    // Now insert or update the PReturnMaster record
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_PurchaseReturn, (SqlConnection)DataConnection, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Always use parameters from the pr object but provide fallbacks for NULL values
                        cmd.Parameters.AddWithValue("@CompanyId", pr.CompanyId);
                        cmd.Parameters.AddWithValue("@FinYearId", pr.FinYearId);
                        cmd.Parameters.AddWithValue("@BranchId", pr.BranchId);
                        cmd.Parameters.AddWithValue("@BranchName", pr.BranchName ?? DataBase.Branch ?? "");
                        cmd.Parameters.AddWithValue("@PReturnNo", pr.PReturnNo);
                        cmd.Parameters.AddWithValue("@PReturnDate", pr.PReturnDate);
                        cmd.Parameters.AddWithValue("@PInvoice", pr.PInvoice ?? "");
                        cmd.Parameters.AddWithValue("@InvoiceNo", pr.InvoiceNo ?? "");
                        cmd.Parameters.AddWithValue("@InvoiceDate", pr.InvoiceDate);
                        cmd.Parameters.AddWithValue("@LedgerID", pr.LedgerID);
                        cmd.Parameters.AddWithValue("@VendorName", pr.VendorName ?? "");
                        cmd.Parameters.AddWithValue("@PaymodeID", pr.PaymodeID);
                        cmd.Parameters.AddWithValue("@Paymode", pr.Paymode ?? string.Empty);
                        cmd.Parameters.AddWithValue("@PaymodeLedgerID", pr.PaymodeLedgerID);
                        cmd.Parameters.AddWithValue("@CreditPeriod", pr.CreditPeriod);
                        cmd.Parameters.AddWithValue("@SubTotal", pr.SubTotal);
                        cmd.Parameters.AddWithValue("@SpDisPer", pr.SpDisPer);
                        cmd.Parameters.AddWithValue("@SpDsiAmt", pr.SpDsiAmt);
                        cmd.Parameters.AddWithValue("@BillDiscountPer", pr.BillDiscountPer);
                        cmd.Parameters.AddWithValue("@BillDiscountAmt", pr.BillDiscountAmt);
                        cmd.Parameters.AddWithValue("@TaxPer", pr.TaxPer);
                        cmd.Parameters.AddWithValue("@TaxAmt", pr.TaxAmt);
                        cmd.Parameters.AddWithValue("@Frieght", pr.Frieght);
                        cmd.Parameters.AddWithValue("@ExpenseAmt", pr.ExpenseAmt);
                        cmd.Parameters.AddWithValue("@OtherExpAmt", pr.OtherExpAmt);
                        cmd.Parameters.AddWithValue("@GrandTotal", pr.GrandTotal);
                        cmd.Parameters.AddWithValue("@CancelFlag", pr.CancelFlag);
                        cmd.Parameters.AddWithValue("@UserID", pr.UserID > 0 ? pr.UserID : Convert.ToInt32(DataBase.UserId));
                        cmd.Parameters.AddWithValue("@UserName", pr.UserName ?? DataBase.UserName ?? "");
                        cmd.Parameters.AddWithValue("@TaxType", pr.TaxType ?? "I");
                        cmd.Parameters.AddWithValue("@Remarks", pr.Remarks ?? "");
                        cmd.Parameters.AddWithValue("@RoundOff", pr.RoundOff);
                        cmd.Parameters.AddWithValue("@CessPer", pr.CessPer);
                        cmd.Parameters.AddWithValue("@CessAmt", pr.CessAmt);
                        cmd.Parameters.AddWithValue("@CalAfterTax", pr.CalAfterTax);
                        cmd.Parameters.AddWithValue("@CurrencyID", pr.CurrencyID);
                        cmd.Parameters.AddWithValue("@CurSymbol", pr.CurSymbol ?? "");
                        cmd.Parameters.AddWithValue("@SeriesID", pr.SeriesID);
                        cmd.Parameters.AddWithValue("@VoucherID", pr.VoucherID);
                        cmd.Parameters.AddWithValue("@_Operation", isUpdate ? "UPDATE" : "CREATE");

                        try
                        {
                            cmd.ExecuteNonQuery();
                            if (isUpdate)
                            {
                                System.Diagnostics.Debug.WriteLine($"Successfully updated PReturnMaster record with PR# {pr.PReturnNo}");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Successfully inserted PReturnMaster record with PR# {pr.PReturnNo}");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error {(isUpdate ? "updating" : "inserting")} PReturnMaster: {ex.Message}");
                            throw new Exception($"Error {(isUpdate ? "updating" : "inserting")} PReturnMaster: {ex.Message}", ex);
                        }
                    }

                    // Iterate over DataGridView and insert PReturnDetails for each row
                    // Skip detail insertion if updating (UpdatePurchaseReturnDetails will handle it)
                    if (!isUpdate)
                    {
                        for (int i = 0; i < dgvInvoice.Rows.Count; i++)
                        {
                            // Skip rows that are empty or in edit mode
                            if (dgvInvoice.Rows[i].IsNewRow || dgvInvoice.Rows[i].Cells["ItemID"].Value == null)
                                continue;

                            details.CompanyId = pr.CompanyId;
                            details.FinYearId = pr.FinYearId;
                            details.BranchID = pr.BranchId;
                            details.PReturnNo = pr.PReturnNo;
                            details.PReturnDate = pr.PReturnDate;
                            details.InvoiceNo = pr.InvoiceNo;
                            details.SlNo = i + 1;

                            // Safely get values with proper null checking
                            details.ItemID = dgvInvoice.Rows[i].Cells["ItemID"].Value != null && dgvInvoice.Rows[i].Cells["ItemID"].Value != DBNull.Value
                                ? Convert.ToInt64(dgvInvoice.Rows[i].Cells["ItemID"].Value)
                                : 0;

                            details.Description = dgvInvoice.Rows[i].Cells["Description"].Value != null && dgvInvoice.Rows[i].Cells["Description"].Value != DBNull.Value
                                ? dgvInvoice.Rows[i].Cells["Description"].Value.ToString()
                                : "";

                            details.UnitId = dgvInvoice.Rows[i].Cells["UnitId"].Value != null && dgvInvoice.Rows[i].Cells["UnitId"].Value != DBNull.Value
                                ? Convert.ToInt32(dgvInvoice.Rows[i].Cells["UnitId"].Value)
                                : 1;

                            // Set BaseUnit as a boolean value using safe conversion
                            if (dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "BaseUnit"))
                            {
                                var baseUnitValue = dgvInvoice.Rows[i].Cells["BaseUnit"].Value;
                                if (baseUnitValue != null && baseUnitValue != DBNull.Value)
                                {
                                    // Handle string value "Y" or "N"
                                    if (baseUnitValue is string)
                                    {
                                        details.BaseUnit = baseUnitValue.ToString().ToUpper() == "Y";
                                    }
                                    else
                                    {
                                        details.BaseUnit = Convert.ToBoolean(baseUnitValue);
                                    }
                                }
                                else
                                {
                                    details.BaseUnit = true; // Default to true if not specified
                                }
                            }
                            else
                            {
                                details.BaseUnit = true; // Default to true if column doesn't exist
                            }

                            // Safe conversion for Packing with default value
                            details.Packing = dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "Packing")
                                            && dgvInvoice.Rows[i].Cells["Packing"].Value != null
                                            && dgvInvoice.Rows[i].Cells["Packing"].Value != DBNull.Value
                                ? Convert.ToDouble(dgvInvoice.Rows[i].Cells["Packing"].Value)
                                : 1.0;

                            // Safe conversion for IsExpiry with default value
                            details.IsExpiry = dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "IsExpiry")
                                             && dgvInvoice.Rows[i].Cells["IsExpiry"].Value != null
                                             && dgvInvoice.Rows[i].Cells["IsExpiry"].Value != DBNull.Value
                                ? Convert.ToBoolean(dgvInvoice.Rows[i].Cells["IsExpiry"].Value)
                                : false;

                            // Safe conversion for BatchNo with default value
                            details.BatchNo = dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "BatchNo")
                                            && dgvInvoice.Rows[i].Cells["BatchNo"].Value != null
                                            && dgvInvoice.Rows[i].Cells["BatchNo"].Value != DBNull.Value
                                ? dgvInvoice.Rows[i].Cells["BatchNo"].Value.ToString()
                                : "";

                            // Safe conversion for Expiry with default value
                            if (dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "Expiry")
                                && dgvInvoice.Rows[i].Cells["Expiry"].Value != null
                                && dgvInvoice.Rows[i].Cells["Expiry"].Value != DBNull.Value)
                            {
                                details.Expiry = Convert.ToDateTime(dgvInvoice.Rows[i].Cells["Expiry"].Value);
                            }
                            else
                            {
                                details.Expiry = null;
                            }

                            // Safe conversion for quantity with default value
                            details.Qty = dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "Qty")
                                        && dgvInvoice.Rows[i].Cells["Qty"].Value != null
                                        && dgvInvoice.Rows[i].Cells["Qty"].Value != DBNull.Value
                                ? Convert.ToDouble(dgvInvoice.Rows[i].Cells["Qty"].Value)
                                : (dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "Quantity")
                                   && dgvInvoice.Rows[i].Cells["Quantity"].Value != null
                                   && dgvInvoice.Rows[i].Cells["Quantity"].Value != DBNull.Value
                                    ? Convert.ToDouble(dgvInvoice.Rows[i].Cells["Quantity"].Value)
                                    : 0);

                            // Safe conversion for TaxPer with default value
                            details.TaxPer = dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "TaxPer")
                                           && dgvInvoice.Rows[i].Cells["TaxPer"].Value != null
                                           && dgvInvoice.Rows[i].Cells["TaxPer"].Value != DBNull.Value
                                ? Convert.ToDouble(dgvInvoice.Rows[i].Cells["TaxPer"].Value)
                                : 0;

                            // Safe conversion for TaxAmt with default value
                            details.TaxAmt = dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "TaxAmt")
                                           && dgvInvoice.Rows[i].Cells["TaxAmt"].Value != null
                                           && dgvInvoice.Rows[i].Cells["TaxAmt"].Value != DBNull.Value
                                ? Convert.ToDouble(dgvInvoice.Rows[i].Cells["TaxAmt"].Value)
                                : 0;

                            // Safe conversion for TaxType with default value
                            // Convert display format back to database format: "Incl" -> "I", "Excl" -> "E"
                            if (dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "TaxType")
                                && dgvInvoice.Rows[i].Cells["TaxType"].Value != null
                                && dgvInvoice.Rows[i].Cells["TaxType"].Value != DBNull.Value)
                            {
                                string taxTypeDisplay = dgvInvoice.Rows[i].Cells["TaxType"].Value.ToString().Trim();
                                // Convert display format to database format
                                if (taxTypeDisplay.Equals("Incl", StringComparison.OrdinalIgnoreCase))
                                    details.TaxType = "I";
                                else if (taxTypeDisplay.Equals("Excl", StringComparison.OrdinalIgnoreCase))
                                    details.TaxType = "E";
                                else if (taxTypeDisplay == "I" || taxTypeDisplay == "E")
                                    details.TaxType = taxTypeDisplay.ToUpper(); // Already in correct format
                                else
                                    details.TaxType = pr.TaxType ?? "I"; // Fallback if unknown format
                            }
                            else
                            {
                                details.TaxType = pr.TaxType ?? "I"; // Fallback to master TaxType or "I"
                            }

                            // Safe conversion for Reason with default value
                            details.Reason = dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "Reason")
                                           && dgvInvoice.Rows[i].Cells["Reason"].Value != null
                                           && dgvInvoice.Rows[i].Cells["Reason"].Value != DBNull.Value
                                ? dgvInvoice.Rows[i].Cells["Reason"].Value.ToString()
                                : "Return";

                            // Safe conversion for Free with default value
                            details.Free = dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "Free")
                                         && dgvInvoice.Rows[i].Cells["Free"].Value != null
                                         && dgvInvoice.Rows[i].Cells["Free"].Value != DBNull.Value
                                ? Convert.ToDouble(dgvInvoice.Rows[i].Cells["Free"].Value)
                                : 0;

                            // Safe conversion for Cost with default value
                            details.Cost = dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "Cost")
                                         && dgvInvoice.Rows[i].Cells["Cost"].Value != null
                                         && dgvInvoice.Rows[i].Cells["Cost"].Value != DBNull.Value
                                ? Convert.ToDouble(dgvInvoice.Rows[i].Cells["Cost"].Value)
                                : 0;

                            // Safe conversion for DisPer with default value
                            details.DisPer = dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "DisPer")
                                           && dgvInvoice.Rows[i].Cells["DisPer"].Value != null
                                           && dgvInvoice.Rows[i].Cells["DisPer"].Value != DBNull.Value
                                ? Convert.ToDouble(dgvInvoice.Rows[i].Cells["DisPer"].Value)
                                : 0;

                            // Safe conversion for DisAmt with default value
                            details.DisAmt = dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "DisAmt")
                                           && dgvInvoice.Rows[i].Cells["DisAmt"].Value != null
                                           && dgvInvoice.Rows[i].Cells["DisAmt"].Value != DBNull.Value
                                ? Convert.ToDouble(dgvInvoice.Rows[i].Cells["DisAmt"].Value)
                                : 0;

                            // Safe conversion for SalesPrice with default value
                            details.SalesPrice = dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "SalesPrice")
                                               && dgvInvoice.Rows[i].Cells["SalesPrice"].Value != null
                                               && dgvInvoice.Rows[i].Cells["SalesPrice"].Value != DBNull.Value
                                ? Convert.ToDouble(dgvInvoice.Rows[i].Cells["SalesPrice"].Value)
                                : details.Cost; // Default to Cost if not available

                            // Safe conversion for OriginalCost with default value
                            details.OriginalCost = dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "OriginalCost")
                                                  && dgvInvoice.Rows[i].Cells["OriginalCost"].Value != null
                                                  && dgvInvoice.Rows[i].Cells["OriginalCost"].Value != DBNull.Value
                                ? Convert.ToDouble(dgvInvoice.Rows[i].Cells["OriginalCost"].Value)
                                : details.Cost; // Default to Cost if not available

                            // Safe conversion for TotalSP with default value
                            details.TotalSP = dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "TotalSP")
                                            && dgvInvoice.Rows[i].Cells["TotalSP"].Value != null
                                            && dgvInvoice.Rows[i].Cells["TotalSP"].Value != DBNull.Value
                                ? Convert.ToDouble(dgvInvoice.Rows[i].Cells["TotalSP"].Value)
                                : details.Cost * details.Qty; // Calculate if not available

                            // Safe conversion for TotalAmount with default value
                            details.TotalAmount = dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "TotalAmount")
                                                && dgvInvoice.Rows[i].Cells["TotalAmount"].Value != null
                                                && dgvInvoice.Rows[i].Cells["TotalAmount"].Value != DBNull.Value
                                ? Convert.ToDouble(dgvInvoice.Rows[i].Cells["TotalAmount"].Value)
                                : (dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "Amount")
                                   && dgvInvoice.Rows[i].Cells["Amount"].Value != null
                                   && dgvInvoice.Rows[i].Cells["Amount"].Value != DBNull.Value
                                    ? Convert.ToDouble(dgvInvoice.Rows[i].Cells["Amount"].Value)
                                    : details.Cost * details.Qty); // Calculate if not available

                            // Safe conversion for CessAmt with default value
                            details.CessAmt = dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "CessAmt")
                                            && dgvInvoice.Rows[i].Cells["CessAmt"].Value != null
                                            && dgvInvoice.Rows[i].Cells["CessAmt"].Value != DBNull.Value
                                ? Convert.ToDouble(dgvInvoice.Rows[i].Cells["CessAmt"].Value)
                                : 0;

                            // Safe conversion for CessPer with default value
                            details.CessPer = dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "CessPer")
                                            && dgvInvoice.Rows[i].Cells["CessPer"].Value != null
                                            && dgvInvoice.Rows[i].Cells["CessPer"].Value != DBNull.Value
                                ? Convert.ToDouble(dgvInvoice.Rows[i].Cells["CessPer"].Value)
                                : 0;

                            // Get return quantity from "Returned qty" column and assign to Returned field
                            // Returnqty must be 0 in database, Returned must get the return qty value
                            double returnQty = 0;
                            if (dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "Returned qty")
                                && dgvInvoice.Rows[i].Cells["Returned qty"].Value != null
                                && dgvInvoice.Rows[i].Cells["Returned qty"].Value != DBNull.Value)
                            {
                                returnQty = Convert.ToDouble(dgvInvoice.Rows[i].Cells["Returned qty"].Value);
                            }
                            // Also check for alternative column name without space (for compatibility)
                            else if (dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "ReturnedQty")
                                     && dgvInvoice.Rows[i].Cells["ReturnedQty"].Value != null
                                     && dgvInvoice.Rows[i].Cells["ReturnedQty"].Value != DBNull.Value)
                            {
                                returnQty = Convert.ToDouble(dgvInvoice.Rows[i].Cells["ReturnedQty"].Value);
                            }
                            // Also check for "ReturnQty" column name (alternative naming)
                            else if (dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "ReturnQty")
                                     && dgvInvoice.Rows[i].Cells["ReturnQty"].Value != null
                                     && dgvInvoice.Rows[i].Cells["ReturnQty"].Value != DBNull.Value)
                            {
                                returnQty = Convert.ToDouble(dgvInvoice.Rows[i].Cells["ReturnQty"].Value);
                            }

                            // Pass return qty to Returned field, and set Returnqty to 0
                            details.Returned = returnQty;
                            details.Returnqty = 0;

                            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_PReturnDetails, (SqlConnection)DataConnection, trans))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("CompanyId", details.CompanyId);
                                cmd.Parameters.AddWithValue("FinYearId", details.FinYearId);
                                cmd.Parameters.AddWithValue("BranchID", details.BranchID);
                                cmd.Parameters.AddWithValue("PReturnNo", details.PReturnNo);
                                cmd.Parameters.AddWithValue("PReturnDate", details.PReturnDate);
                                cmd.Parameters.AddWithValue("InvoiceNo", details.InvoiceNo ?? "");
                                cmd.Parameters.AddWithValue("SlNo", details.SlNo);
                                cmd.Parameters.AddWithValue("ItemID", details.ItemID);
                                cmd.Parameters.AddWithValue("ItemName", details.Description ?? "");
                                cmd.Parameters.AddWithValue("UnitId", details.UnitId);
                                cmd.Parameters.AddWithValue("BaseUnit", details.BaseUnit ? "Y" : "N");
                                cmd.Parameters.AddWithValue("Packing", details.Packing);
                                cmd.Parameters.AddWithValue("IsExpiry", details.IsExpiry);
                                cmd.Parameters.AddWithValue("BatchNo", details.BatchNo ?? "");
                                cmd.Parameters.AddWithValue("Expiry", details.Expiry == null ? DBNull.Value : (object)details.Expiry);
                                cmd.Parameters.AddWithValue("Qty", details.Qty);
                                cmd.Parameters.AddWithValue("TaxPer", details.TaxPer);
                                cmd.Parameters.AddWithValue("TaxAmt", details.TaxAmt);
                                cmd.Parameters.AddWithValue("Reason", details.Reason ?? "Return");
                                cmd.Parameters.AddWithValue("Free", details.Free);
                                cmd.Parameters.AddWithValue("Cost", details.Cost);
                                cmd.Parameters.AddWithValue("DisPer", details.DisPer);
                                cmd.Parameters.AddWithValue("DisAmt", details.DisAmt);
                                cmd.Parameters.AddWithValue("SalesPrice", details.SalesPrice);
                                cmd.Parameters.AddWithValue("OriginalCost", details.OriginalCost);
                                cmd.Parameters.AddWithValue("TotalSP", details.TotalSP);

                                // Add cess parameters
                                cmd.Parameters.AddWithValue("CessAmt", details.CessAmt);
                                cmd.Parameters.AddWithValue("CessPer", details.CessPer);

                                // Add Returnqty and Returned parameters
                                cmd.Parameters.AddWithValue("Returnqty", details.Returnqty);
                                cmd.Parameters.AddWithValue("Returned", details.Returned);

                                // Add TaxType parameter which is required and cannot be NULL
                                // Use the TaxType from the detail record (from grid) or fallback to master or "I" (Inclusive)
                                cmd.Parameters.AddWithValue("TaxType", details.TaxType ?? pr.TaxType ?? "I");
                                System.Diagnostics.Debug.WriteLine($"Using TaxType: {details.TaxType ?? pr.TaxType ?? "I"}");

                                // Add SeriesID parameter which is also required and cannot be NULL
                                cmd.Parameters.AddWithValue("SeriesID", pr.SeriesID > 0 ? pr.SeriesID : 1);
                                System.Diagnostics.Debug.WriteLine($"Using SeriesID: {(pr.SeriesID > 0 ? pr.SeriesID : 1)}");

                                // Add TrnsType parameter which is required by the SP
                                cmd.Parameters.AddWithValue("TrnsType", VoucherType.PurchaseReturn);

                                // Operation parameter
                                cmd.Parameters.AddWithValue("_Operation", "CREATE");

                                try
                                {
                                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                                    DataTable dt = new DataTable();
                                    da.Fill(dt);
                                    System.Diagnostics.Debug.WriteLine($"Inserted detail record for ItemID: {details.ItemID}, Qty: {details.Qty}, Reason: {details.Reason}");
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error inserting detail record: {ex.Message}");

                                    // Print all parameters that were sent to help diagnose issues
                                    System.Diagnostics.Debug.WriteLine("Parameters sent to stored procedure:");
                                    foreach (SqlParameter param in cmd.Parameters)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"{param.ParameterName} = {param.Value}");
                                    }

                                    throw; // Rethrow the original exception
                                }
                            }

                            // Update inventory
                            try
                            {
                                using (SqlCommand invCmd = new SqlCommand(STOREDPROCEDURE.POS_ItemMasterPriceSettings, (SqlConnection)DataConnection, trans))
                                {
                                    invCmd.CommandType = CommandType.StoredProcedure;
                                    invCmd.Parameters.AddWithValue("CompanyId", details.CompanyId);
                                    invCmd.Parameters.AddWithValue("BranchId", details.BranchID);
                                    invCmd.Parameters.AddWithValue("ItemId", details.ItemID);
                                    invCmd.Parameters.AddWithValue("Qty", details.Qty);
                                    invCmd.Parameters.AddWithValue("TransactionType", VoucherType.PurchaseReturn);
                                    invCmd.Parameters.AddWithValue("TransactionNo", details.PReturnNo);
                                    invCmd.Parameters.AddWithValue("ReferenceNo", details.SlNo);
                                    invCmd.Parameters.AddWithValue("_Operation", "CREATETRANSACTION");

                                    invCmd.ExecuteNonQuery();
                                    System.Diagnostics.Debug.WriteLine($"Updated inventory for ItemID: {details.ItemID}");
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Warning: Error updating inventory: {ex.Message}");
                                // Continue despite inventory update error - just log it
                            }

                            // Reduce stock in PriceSettings table for purchase return
                            // This is critical for inventory accuracy
                            ReducePriceSettingsStock(details.ItemID, details.BranchID, details.UnitId, returnQty, trans);

                            // Update PDetails table with returned quantity if this is linked to a purchase invoice
                            // Check both PInvoice and InvoiceNo for the purchase number
                            int purchaseNoForUpdate = 0;
                            bool isValidPurchaseNo = false;

                            // Try to get purchase number from PInvoice first
                            if (!string.IsNullOrEmpty(pr.PInvoice) &&
                                pr.PInvoice != "WITHOUT GR" &&
                                int.TryParse(pr.PInvoice, out purchaseNoForUpdate) &&
                                purchaseNoForUpdate > 0)
                            {
                                isValidPurchaseNo = true;
                            }
                            // If PInvoice doesn't have a valid purchase number, try InvoiceNo
                            else if (!string.IsNullOrEmpty(pr.InvoiceNo) &&
                                     pr.InvoiceNo != "WITHOUT GR" &&
                                     int.TryParse(pr.InvoiceNo, out purchaseNoForUpdate) &&
                                     purchaseNoForUpdate > 0)
                            {
                                isValidPurchaseNo = true;
                            }

                            // Update PDetails if we have a valid purchase number and return quantity
                            if (isValidPurchaseNo && returnQty > 0)
                            {
                                try
                                {
                                    // Update PDetails table: add the returned quantity to the existing Returned value
                                    // Match by PurchaseNo and ItemID (SlNo might differ between PDetails and PReturnDetails)
                                    string updatePDetailsQuery = @"
                                    UPDATE PDetails 
                                    SET Returned = ISNULL(Returned, 0) + @ReturnedQty
                                    WHERE PurchaseNo = @PurchaseNo 
                                    AND ItemID = @ItemID";

                                    using (SqlCommand updateCmd = new SqlCommand(updatePDetailsQuery, (SqlConnection)DataConnection, trans))
                                    {
                                        updateCmd.Parameters.AddWithValue("@ReturnedQty", returnQty);
                                        updateCmd.Parameters.AddWithValue("@PurchaseNo", purchaseNoForUpdate);
                                        updateCmd.Parameters.AddWithValue("@ItemID", details.ItemID);

                                        int rowsAffected = updateCmd.ExecuteNonQuery();
                                        if (rowsAffected > 0)
                                        {
                                            System.Diagnostics.Debug.WriteLine($"Updated PDetails for PurchaseNo: {purchaseNoForUpdate}, ItemID: {details.ItemID}, Added ReturnedQty: {returnQty}");
                                        }
                                        else
                                        {
                                            System.Diagnostics.Debug.WriteLine($"Warning: No rows updated in PDetails for PurchaseNo: {purchaseNoForUpdate}, ItemID: {details.ItemID}");
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Warning: Error updating PDetails with returned quantity: {ex.Message}");
                                    // Continue despite PDetails update error - just log it
                                }
                            }
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Skipping detail insertion in savePR for update - UpdatePurchaseReturnDetails will handle it");
                    }

                    // Create accounting vouchers for the purchase return (only for new PRs, not updates)
                    // For updates, vouchers will be regenerated in UpdatePurchaseReturnDetails
                    if (!isUpdate)
                    {
                        // Determine which ledger to credit based on payment method's PaymodeLedgerID
                        int creditLedgerId;
                        string creditLedgerName;

                        // Use PaymodeLedgerID if available, otherwise fall back to vendor ledger
                        if (pr.PaymodeLedgerID > 0)
                        {
                            // Use the ledger from PayMode table
                            creditLedgerId = pr.PaymodeLedgerID;
                            creditLedgerName = pr.Paymode;
                            System.Diagnostics.Debug.WriteLine($"Using PaymodeLedgerID: {creditLedgerId} for payment method: {pr.Paymode}");
                        }
                        else
                        {
                            // Fallback to vendor ledger if PaymodeLedgerID is not set
                            creditLedgerId = (int)pr.LedgerID;
                            creditLedgerName = pr.VendorName;
                            System.Diagnostics.Debug.WriteLine($"Using Vendor ledger (ID: {creditLedgerId}) as fallback");
                        }

                        // Calculate amount without tax for Purchase Return ledger (SubTotal - TaxAmt)
                        double purchaseAmountWithoutTax = pr.SubTotal - pr.TaxAmt;

                        System.Diagnostics.Debug.WriteLine($"Voucher Calculation: SubTotal={pr.SubTotal}, TaxAmt={pr.TaxAmt}, PurchaseAmtWithoutTax={purchaseAmountWithoutTax}, GrandTotal={pr.GrandTotal}");

                        // Create payment method account voucher (DEBIT for return - money received back)
                        PReturnVoucher vendorVoucher = new PReturnVoucher
                        {
                            CompanyId = pr.CompanyId,
                            FinYearId = pr.FinYearId,
                            BranchID = pr.BranchId,
                            VoucherID = voucherId,
                            VoucherType = ModelClass.VoucherType.DebitNote,
                            VoucherDate = pr.PReturnDate,
                            ReferenceNo = pr.PReturnNo.ToString(),
                            LedgerID = creditLedgerId, // Use payment method's ledger
                            Debit = pr.GrandTotal,
                            Credit = 0,
                            Narration = $"Purchase Return - PR#{pr.PReturnNo}",
                            CancelFlag = false
                        };

                        CreateVoucher(vendorVoucher, trans);

                        // Create purchase return account voucher (CREDIT for return - reducing purchase account)
                        // Amount = SubTotal - TaxAmt (purchase amount without tax)
                        int purchaseReturnLedgerId = GetPurchaseReturnLedgerId(pr.BranchId);
                        if (purchaseReturnLedgerId <= 0)
                        {
                            throw new Exception("Purchase Return ledger not found. Please configure Purchase Return ledger in the system.");
                        }

                        PReturnVoucher prVoucher = new PReturnVoucher
                        {
                            CompanyId = pr.CompanyId,
                            FinYearId = pr.FinYearId,
                            BranchID = pr.BranchId,
                            VoucherID = voucherId,
                            VoucherType = ModelClass.VoucherType.DebitNote,
                            VoucherDate = pr.PReturnDate,
                            ReferenceNo = pr.PReturnNo.ToString(),
                            LedgerID = purchaseReturnLedgerId, // Always use Purchase Return ledger
                            Debit = 0,
                            Credit = purchaseAmountWithoutTax,
                            Narration = $"Purchase Return - PR#{pr.PReturnNo}",
                            CancelFlag = false
                        };

                        CreateVoucher(prVoucher, trans);

                        // Create tax voucher entries for CGST and SGST
                        Dictionary<double, double> taxAmountsByPercentage = AggregateTaxAmountsByPercentage(dgvInvoice);
                        if (taxAmountsByPercentage.Count > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"Creating tax vouchers for {taxAmountsByPercentage.Count} tax percentage(s)");
                            CreateTaxVoucherEntries(voucherId, taxAmountsByPercentage, pr.PReturnNo, pr.Remarks ?? "", trans);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("No tax amounts found, skipping tax voucher creation");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Skipping voucher creation for update - vouchers will be regenerated in UpdatePurchaseReturnDetails");
                    }

                    trans.Commit();

                    // Update TrackTrans table with the latest PR number for future reference
                    try
                    {
                        // Make sure we have a fresh connection after commit
                        if (DataConnection.State != ConnectionState.Open)
                        {
                            DataConnection.Open();
                        }

                        // First check if the TrackTrans record exists
                        string checkQuery = @"
                            SELECT COUNT(*) 
                            FROM TrackTrans 
                            WHERE BranchID = @BranchId AND FinYearID = @FinYearId";

                        int recordCount = 0;
                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, (SqlConnection)DataConnection))
                        {
                            checkCmd.Parameters.AddWithValue("@BranchId", pr.BranchId);
                            checkCmd.Parameters.AddWithValue("@FinYearId", pr.FinYearId);
                            recordCount = Convert.ToInt32(checkCmd.ExecuteScalar());
                        }

                        if (recordCount > 0)
                        {
                            // Update existing record
                            using (SqlCommand cmd = new SqlCommand("UPDATE TrackTrans SET PRBillNo = @PRBillNo WHERE BranchID = @BranchId AND FinYearID = @FinYearId", (SqlConnection)DataConnection))
                            {
                                cmd.Parameters.AddWithValue("@PRBillNo", pr.PReturnNo);
                                cmd.Parameters.AddWithValue("@BranchId", pr.BranchId);
                                cmd.Parameters.AddWithValue("@FinYearId", pr.FinYearId);
                                cmd.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine($"Updated TrackTrans with PRBillNo = {pr.PReturnNo}");
                            }
                        }
                        else
                        {
                            // Insert new record
                            using (SqlCommand cmd = new SqlCommand("INSERT INTO TrackTrans (BranchID, FinYearID, PRBillNo) VALUES (@BranchId, @FinYearId, @PRBillNo)", (SqlConnection)DataConnection))
                            {
                                cmd.Parameters.AddWithValue("@PRBillNo", pr.PReturnNo);
                                cmd.Parameters.AddWithValue("@BranchId", pr.BranchId);
                                cmd.Parameters.AddWithValue("@FinYearId", pr.FinYearId);
                                cmd.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine($"Created new TrackTrans record with PRBillNo = {pr.PReturnNo}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error updating TrackTrans: {ex.Message}");
                        // Not critical, continue
                    }

                    // Return success with the PR number
                    return $"success:{pr.PReturnNo}";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    System.Diagnostics.Debug.WriteLine($"Error in savePR: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                    return "Error: " + ex.Message;
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

        public List<PReturnGetAll> GetAll()
        {
            List<PReturnGetAll> prList = new List<PReturnGetAll>();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_PurchaseReturn, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");

                    using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                    {
                        DataSet dt = new DataSet();
                        adp.Fill(dt);
                        if (dt.Tables[0].Rows.Count > 0)
                        {
                            prList = dt.Tables[0].ToListOfObject<PReturnGetAll>();
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
                {
                    DataConnection.Close();
                }
            }

            return prList;
        }

        public string UpdatePR(PReturnMaster pr)
        {
            DataConnection.Open();

            try
            {
                // Ensure PaymodeLedgerID is set from PayMode table if not already set
                if (pr.PaymodeLedgerID <= 0 && pr.PaymodeID > 0)
                {
                    try
                    {
                        string ledgerQuery = @"
                            SELECT TOP 1 LedgerID 
                            FROM PayMode 
                            WHERE PayModeID = @PaymodeID";

                        using (SqlCommand ledgerCmd = new SqlCommand(ledgerQuery, (SqlConnection)DataConnection))
                        {
                            ledgerCmd.Parameters.AddWithValue("@PaymodeID", pr.PaymodeID);
                            object ledgerResult = ledgerCmd.ExecuteScalar();
                            if (ledgerResult != null && ledgerResult != DBNull.Value)
                            {
                                pr.PaymodeLedgerID = Convert.ToInt32(ledgerResult);
                                System.Diagnostics.Debug.WriteLine($"Retrieved PaymodeLedgerID: {pr.PaymodeLedgerID} for PaymodeID: {pr.PaymodeID}");
                            }
                        }
                    }
                    catch (Exception ledgerEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error getting PaymodeLedgerID: {ledgerEx.Message}");
                    }
                }

                using (SqlCommand cmd = new SqlCommand("_POS_PurchaseReturn", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "UPDATE");
                    cmd.Parameters.AddWithValue("@Id", pr.Id);
                    cmd.Parameters.AddWithValue("@PReturnNo", pr.PReturnNo);
                    cmd.Parameters.AddWithValue("@CompanyId", pr.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", pr.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", pr.FinYearId);
                    cmd.Parameters.AddWithValue("@PReturnDate", pr.PReturnDate);
                    cmd.Parameters.AddWithValue("@LedgerID", pr.LedgerID);
                    cmd.Parameters.AddWithValue("@VendorName", pr.VendorName ?? "");
                    cmd.Parameters.AddWithValue("@PaymodeID", pr.PaymodeID);
                    cmd.Parameters.AddWithValue("@Paymode", pr.Paymode ?? "");
                    cmd.Parameters.AddWithValue("@PaymodeLedgerID", pr.PaymodeLedgerID);
                    cmd.Parameters.AddWithValue("@InvoiceNo", pr.InvoiceNo ?? "");
                    cmd.Parameters.AddWithValue("@InvoiceDate", pr.InvoiceDate != DateTime.MinValue ? (object)pr.InvoiceDate : DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreditPeriod", pr.CreditPeriod);
                    cmd.Parameters.AddWithValue("@SubTotal", pr.SubTotal);
                    cmd.Parameters.AddWithValue("@SpDisPer", pr.SpDisPer);
                    cmd.Parameters.AddWithValue("@SpDsiAmt", pr.SpDsiAmt);
                    cmd.Parameters.AddWithValue("@BillDiscountPer", pr.BillDiscountPer);
                    cmd.Parameters.AddWithValue("@BillDiscountAmt", pr.BillDiscountAmt);
                    cmd.Parameters.AddWithValue("@TaxPer", pr.TaxPer);
                    cmd.Parameters.AddWithValue("@TaxAmt", pr.TaxAmt);
                    cmd.Parameters.AddWithValue("@Frieght", pr.Frieght);
                    cmd.Parameters.AddWithValue("@ExpenseAmt", pr.ExpenseAmt);
                    cmd.Parameters.AddWithValue("@OtherExpAmt", pr.OtherExpAmt);
                    cmd.Parameters.AddWithValue("@GrandTotal", pr.GrandTotal);
                    cmd.Parameters.AddWithValue("@CancelFlag", pr.CancelFlag);
                    cmd.Parameters.AddWithValue("@UserID", pr.UserID > 0 ? pr.UserID : Convert.ToInt32(DataBase.UserId));
                    cmd.Parameters.AddWithValue("@UserName", pr.UserName ?? DataBase.UserName ?? "");
                    cmd.Parameters.AddWithValue("@TaxType", pr.TaxType ?? "I");
                    cmd.Parameters.AddWithValue("@Remarks", pr.Remarks ?? "");
                    cmd.Parameters.AddWithValue("@RoundOff", pr.RoundOff);
                    cmd.Parameters.AddWithValue("@CessPer", pr.CessPer);
                    cmd.Parameters.AddWithValue("@CessAmt", pr.CessAmt);
                    cmd.Parameters.AddWithValue("@CalAfterTax", pr.CalAfterTax);
                    cmd.Parameters.AddWithValue("@CurrencyID", pr.CurrencyID);
                    cmd.Parameters.AddWithValue("@CurSymbol", pr.CurSymbol ?? "");

                    System.Diagnostics.Debug.WriteLine($"UpdatePR - Executing stored procedure with PaymodeID={pr.PaymodeID}, Paymode={pr.Paymode}");
                    System.Diagnostics.Debug.WriteLine($"UpdatePR - Record ID: {pr.Id}, PReturnNo: {pr.PReturnNo}");
                    System.Diagnostics.Debug.WriteLine($"UpdatePR - LedgerID: {pr.LedgerID}, VendorName: {pr.VendorName}");
                    System.Diagnostics.Debug.WriteLine($"UpdatePR - PaymodeLedgerID: {pr.PaymodeLedgerID}");

                    // Log all parameters being sent to stored procedure
                    System.Diagnostics.Debug.WriteLine("=== STORED PROCEDURE PARAMETERS ===");
                    foreach (SqlParameter param in cmd.Parameters)
                    {
                        System.Diagnostics.Debug.WriteLine($"Parameter: {param.ParameterName} = {param.Value} (Type: {param.SqlDbType})");
                    }

                    // Execute the stored procedure and get the result
                    object result = cmd.ExecuteScalar();

                    System.Diagnostics.Debug.WriteLine($"UpdatePR - Stored procedure returned: {result}");

                    if (result != null && result.ToString() == "SUCCESS")
                    {
                        System.Diagnostics.Debug.WriteLine($"UpdatePR - Successfully updated payment method to: {pr.Paymode} (ID: {pr.PaymodeID})");
                        return "success";
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("UpdatePR - No records were updated or update failed");
                        return "No records were updated";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdatePR - Error: {ex.Message}");
                return "Error: " + ex.Message;
            }
            finally
            {
                DataConnection.Close();
            }
        }

        public string DeletePR(int Id, int CompanyId, int FinYearId, int BranchId, int VoucherId, string VoucherType, int PReturnNo = 0)
        {
            DataConnection.Open();

            try
            {
                // If PReturnNo is not provided, try to get it from the database
                if (PReturnNo <= 0)
                {
                    using (SqlCommand selectCmd = new SqlCommand("SELECT PReturnNo FROM PReturnMaster WHERE Id = @Id", (SqlConnection)DataConnection))
                    {
                        selectCmd.Parameters.AddWithValue("@Id", Id);
                        object result = selectCmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            PReturnNo = Convert.ToInt32(result);
                        }
                    }
                }

                if (PReturnNo <= 0)
                {
                    return "Error: Invalid PReturnNo";
                }

                System.Diagnostics.Debug.WriteLine($"Deleting PR #{PReturnNo} with ID {Id}");

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_PurchaseReturn, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", Id);
                    cmd.Parameters.AddWithValue("@CompanyId", CompanyId);
                    cmd.Parameters.AddWithValue("@FinYearId", FinYearId);
                    cmd.Parameters.AddWithValue("@BranchId", BranchId);
                    cmd.Parameters.AddWithValue("@PReturnNo", PReturnNo);
                    cmd.Parameters.AddWithValue("@VoucherID", VoucherId);
                    cmd.Parameters.AddWithValue("@VoucherType", VoucherType ?? ModelClass.VoucherType.DebitNote);
                    cmd.Parameters.AddWithValue("@_Operation", "DELETE");

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                }

                // Delete (mark as cancelled) the associated vouchers
                if (VoucherId > 0)
                {
                    DeleteVoucher(VoucherId, BranchId, ModelClass.VoucherType.DebitNote);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DeletePR: {ex.Message}");
                return "Error: " + ex.Message;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return "success";
        }

        #region Voucher Operations

        /// <summary>
        /// Generates a new voucher number for the specified voucher type
        /// </summary>
        public int GenerateVoucherNo(string voucherType, SqlTransaction trans = null)
        {
            int voucherNo = 0;
            try
            {
                using (SqlCommand cmd = new SqlCommand("POS_Vouchers", (SqlConnection)DataConnection, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", DataBase.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearID", DataBase.FinyearId);
                    cmd.Parameters.AddWithValue("@VoucherType", voucherType);
                    cmd.Parameters.AddWithValue("@_Operation", "GENERATENUMBER");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            // Ensure we're accessing the data correctly
                            if (ds.Tables[0].Columns.Count > 0)
                            {
                                voucherNo = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                                System.Diagnostics.Debug.WriteLine($"Generated voucher number: {voucherNo}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating voucher number: {ex.Message}");
                throw ex;
            }
            return voucherNo;
        }

        /// <summary>
        /// Creates a new voucher entry in the database
        /// </summary>
        public string CreateVoucher(PReturnVoucher voucher, SqlTransaction trans = null)
        {
            try
            {
                bool closeConnection = false;
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                    closeConnection = true;
                }

                using (SqlCommand cmd = new SqlCommand("POS_Vouchers", (SqlConnection)DataConnection, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyID", voucher.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchID", voucher.BranchID);
                    cmd.Parameters.AddWithValue("@VoucherID", voucher.VoucherID);
                    cmd.Parameters.AddWithValue("@VoucherSeriesID", 0); // Default or parameter as needed
                    cmd.Parameters.AddWithValue("@VoucherDate", voucher.VoucherDate);
                    cmd.Parameters.AddWithValue("@VoucherNumber", voucher.ReferenceNo ?? string.Empty);
                    cmd.Parameters.AddWithValue("@LedgerID", voucher.LedgerID);
                    cmd.Parameters.AddWithValue("@VoucherType", voucher.VoucherType);
                    cmd.Parameters.AddWithValue("@Debit", voucher.Debit);
                    cmd.Parameters.AddWithValue("@Credit", voucher.Credit);
                    cmd.Parameters.AddWithValue("@Narration", voucher.Narration ?? string.Empty);
                    cmd.Parameters.AddWithValue("@SlNo", 1); // Default or parameter as needed
                    // Mode and ModeID are optional parameters - using empty values
                    cmd.Parameters.AddWithValue("@Mode", "");
                    cmd.Parameters.AddWithValue("@ModeID", 0);
                    cmd.Parameters.AddWithValue("@UserDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@UserID", DataBase.UserId);
                    cmd.Parameters.AddWithValue("@CancelFlag", voucher.CancelFlag);
                    cmd.Parameters.AddWithValue("@FinYearID", voucher.FinYearId);
                    cmd.Parameters.AddWithValue("@IsSyncd", false);
                    cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows.Count > 0 && dt.Rows[0][0].ToString() == "SUCCESS")
                        {
                            if (closeConnection && trans == null)
                            {
                                DataConnection.Close();
                            }
                            return "success";
                        }
                    }
                }

                if (closeConnection && trans == null)
                {
                    DataConnection.Close();
                }
                return "failed";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets all vouchers of a specific type with optional filtering and paging
        /// </summary>
        public DataSet GetAllVouchers(string voucherType, int pageIndex = 0, int pageSize = 100, string sortBy = "VoucherID", string sortDirection = "DESC")
        {
            DataSet result = new DataSet();
            bool closeConnection = false;

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                    closeConnection = true;
                }

                using (SqlCommand cmd = new SqlCommand("POS_Vouchers", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", DataBase.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                    cmd.Parameters.AddWithValue("@VoucherType", voucherType);
                    cmd.Parameters.AddWithValue("@PageIndex", pageIndex);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);
                    cmd.Parameters.AddWithValue("@SortBy", sortBy);
                    cmd.Parameters.AddWithValue("@SortByDirection", sortDirection);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (closeConnection && DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return result;
        }

        /// <summary>
        /// Gets a voucher by ID and type
        /// </summary>
        public DataTable GetVoucherById(int voucherId, string voucherType)
        {
            DataTable result = new DataTable();
            bool closeConnection = false;

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                    closeConnection = true;
                }

                using (SqlCommand cmd = new SqlCommand("POS_Vouchers", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VoucherID", voucherId);
                    cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                    cmd.Parameters.AddWithValue("@VoucherType", voucherType);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (closeConnection && DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return result;
        }

        /// <summary>
        /// Updates a voucher by first deleting it and then recreating it
        /// </summary>
        public string UpdateVoucher(PReturnVoucher voucher)
        {
            bool closeConnection = false;

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                    closeConnection = true;
                }

                using (SqlCommand cmd = new SqlCommand("POS_Vouchers", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VoucherID", voucher.VoucherID);
                    cmd.Parameters.AddWithValue("@BranchID", voucher.BranchID);
                    cmd.Parameters.AddWithValue("@VoucherType", voucher.VoucherType);
                    cmd.Parameters.AddWithValue("@_Operation", "UPDATE");

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows.Count > 0 && dt.Rows[0][0].ToString() == "SUCCESS")
                        {
                            // After deleting the old voucher, create a new one
                            return CreateVoucher(voucher);
                        }
                    }
                }

                return "failed";
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (closeConnection && DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        /// <summary>
        /// Marks a voucher as cancelled (soft delete)
        /// </summary>
        public string DeleteVoucher(int voucherId, int branchId, string voucherType)
        {
            bool closeConnection = false;

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                    closeConnection = true;
                }

                using (SqlCommand cmd = new SqlCommand("POS_Vouchers", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VoucherID", voucherId);
                    cmd.Parameters.AddWithValue("@BranchID", branchId);
                    cmd.Parameters.AddWithValue("@VoucherType", voucherType);
                    cmd.Parameters.AddWithValue("@_Operation", "DELETE");

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows.Count > 0 && dt.Rows[0][0].ToString() == "SUCCESS")
                        {
                            return "success";
                        }
                    }
                }

                return "failed";
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (closeConnection && DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        /// <summary>
        /// Creates voucher entries for a purchase return transaction
        /// </summary>
        public string CreatePurchaseReturnVouchers(PReturnMaster prMaster, SqlTransaction trans = null)
        {
            try
            {
                // Generate voucher number directly using the stored procedure
                int voucherNo = 0;
                using (SqlCommand voucherCmd = new SqlCommand("POS_Vouchers", (SqlConnection)DataConnection, trans))
                {
                    voucherCmd.CommandType = CommandType.StoredProcedure;
                    voucherCmd.Parameters.AddWithValue("@CompanyID", prMaster.CompanyId > 0 ? prMaster.CompanyId : Convert.ToInt32(DataBase.CompanyId));
                    voucherCmd.Parameters.AddWithValue("@BranchID", prMaster.BranchId > 0 ? prMaster.BranchId : Convert.ToInt32(DataBase.BranchId));
                    voucherCmd.Parameters.AddWithValue("@FinYearID", prMaster.FinYearId > 0 ? prMaster.FinYearId : Convert.ToInt32(DataBase.FinyearId));
                    voucherCmd.Parameters.AddWithValue("@VoucherType", ModelClass.VoucherType.PurchaseReturn);
                    voucherCmd.Parameters.AddWithValue("@_Operation", "GENERATENUMBER");

                    using (SqlDataAdapter da = new SqlDataAdapter(voucherCmd))
                    {
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            voucherNo = Convert.ToInt32(ds.Tables[0].Rows[0][0]);
                        }
                    }
                }

                if (voucherNo <= 0)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to generate voucher number, using fallback");
                    voucherNo = DateTime.Now.Ticks.GetHashCode() % 10000;
                    if (voucherNo < 0) voucherNo *= -1;
                    if (voucherNo == 0) voucherNo = 1;
                }

                // Determine which ledger to credit based on payment method's PaymodeLedgerID
                int creditLedgerId;
                if (prMaster.PaymodeLedgerID > 0)
                {
                    // Use the ledger from PayMode table
                    creditLedgerId = prMaster.PaymodeLedgerID;
                    System.Diagnostics.Debug.WriteLine($"Using PaymodeLedgerID: {creditLedgerId} for payment method: {prMaster.Paymode}");
                }
                else
                {
                    // Fallback to vendor ledger if PaymodeLedgerID is not set
                    creditLedgerId = (int)prMaster.LedgerID;
                    System.Diagnostics.Debug.WriteLine($"Using Vendor ledger (ID: {creditLedgerId}) as fallback");
                }

                // Create payment method account voucher (credit)
                PReturnVoucher vendorVoucher = new PReturnVoucher
                {
                    CompanyId = prMaster.CompanyId,
                    FinYearId = prMaster.FinYearId,
                    BranchID = prMaster.BranchId,
                    VoucherID = voucherNo,
                    VoucherType = ModelClass.VoucherType.DebitNote,
                    VoucherDate = prMaster.PReturnDate,
                    ReferenceNo = prMaster.PReturnNo.ToString(),
                    LedgerID = creditLedgerId, // Use payment method's ledger
                    Debit = 0,
                    Credit = prMaster.GrandTotal,
                    Narration = $"Purchase Return - PR#{prMaster.PReturnNo}",
                    CancelFlag = false
                };

                CreateVoucher(vendorVoucher, trans);

                // Create purchase return account voucher (debit) - Always use Purchase Return ledger
                int purchaseReturnLedgerId = GetPurchaseReturnLedgerId(prMaster.BranchId);
                if (purchaseReturnLedgerId <= 0)
                {
                    throw new Exception("Purchase Return ledger not found. Please configure Purchase Return ledger in the system.");
                }

                PReturnVoucher prVoucher = new PReturnVoucher
                {
                    CompanyId = prMaster.CompanyId,
                    FinYearId = prMaster.FinYearId,
                    BranchID = prMaster.BranchId,
                    VoucherID = voucherNo,
                    VoucherType = ModelClass.VoucherType.DebitNote,
                    VoucherDate = prMaster.PReturnDate,
                    ReferenceNo = prMaster.PReturnNo.ToString(),
                    LedgerID = purchaseReturnLedgerId, // Always use Purchase Return ledger
                    Debit = prMaster.GrandTotal,
                    Credit = 0,
                    Narration = $"Purchase Return - PR#{prMaster.PReturnNo}",
                    CancelFlag = false
                };

                CreateVoucher(prVoucher, trans);

                return "success";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        public void UpdatePurchaseReturnDetails(ModelClass.TransactionModels.PReturnMaster pr, ModelClass.TransactionModels.PReturnDetails details, bool isFirstRecord = true)
        {
            bool closeConnection = false;
            SqlTransaction transaction = null;

            try
            {
                // Validate reason only if the item is selected
                if (details != null && details.ItemID > 0)
                {
                    // We assume the item is selected if it's being passed to this method
                    // The selection filtering is done in the form before calling this method
                    // So we only need to validate the reason is provided
                    if (string.IsNullOrWhiteSpace(details.Reason))
                    {
                        string itemDesc = !string.IsNullOrWhiteSpace(details.Description) ?
                            details.Description : "Item ID: " + details.ItemID;
                        throw new Exception($"Reason is mandatory for selected items. Please provide a reason for {itemDesc}.");
                    }
                }

                // Ensure FinYearId is valid (not 0)
                if (pr.FinYearId <= 0)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("FinYearId is 0 or negative. Attempting to get active FinYear from database");

                        // First try to get the active FinYear from database
                        if (DataConnection.State != ConnectionState.Open)
                        {
                            DataConnection.Open();
                            closeConnection = true;
                        }

                        string finYearQuery = @"
                            SELECT TOP 1 Id 
                            FROM Finyears 
                            WHERE IsActive = 1 
                            ORDER BY Id DESC";

                        using (SqlCommand finYearCmd = new SqlCommand(finYearQuery, (SqlConnection)DataConnection))
                        {
                            object result = finYearCmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                pr.FinYearId = Convert.ToInt32(result);
                                System.Diagnostics.Debug.WriteLine($"Retrieved FinYearId from database: {pr.FinYearId}");
                            }
                        }

                        // If still invalid, try to get from DataBase.FinyearId
                        if (pr.FinYearId <= 0)
                        {
                            try
                            {
                                int dbFinYear = Convert.ToInt32(DataBase.FinyearId);
                                if (dbFinYear > 0)
                                {
                                    pr.FinYearId = dbFinYear;
                                    System.Diagnostics.Debug.WriteLine($"Using DataBase.FinyearId: {pr.FinYearId}");
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error converting DataBase.FinyearId: {ex.Message}");
                            }
                        }

                        // As a last resort, use a dynamic session value or default to 1
                        if (pr.FinYearId <= 0)
                        {
                            pr.FinYearId = SessionContext.FinYearId > 0 ? SessionContext.FinYearId : (int.TryParse(DataBase.FinyearId, out var id) ? id : 1);
                            System.Diagnostics.Debug.WriteLine($"Using default/session FinYearId: {pr.FinYearId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error retrieving FinYearId: {ex.Message}");
                        // Set a default value
                        pr.FinYearId = SessionContext.FinYearId > 0 ? SessionContext.FinYearId : (int.TryParse(DataBase.FinyearId, out var id) ? id : 1);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Final FinYearId for master record: {pr.FinYearId}");

                // Open connection if needed
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                    closeConnection = true;
                }

                // Get or start transaction
                if (isFirstRecord)
                {
                    // If there's an existing active transaction, make sure it's closed/committed first
                    if (activeTransaction != null)
                    {
                        try
                        {
                            // Try to commit any pending transaction
                            activeTransaction.Commit();
                            System.Diagnostics.Debug.WriteLine("Committed existing active transaction before starting new one");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error committing existing transaction: {ex.Message}");
                            // Continue anyway, as we'll create a new transaction below
                        }
                        activeTransaction = null;
                    }

                    // Begin a new transaction for the first record
                    transaction = (SqlTransaction)DataConnection.BeginTransaction();
                    activeTransaction = transaction; // Store for subsequent calls
                    System.Diagnostics.Debug.WriteLine("Started new transaction for first record");

                    // Delete existing PReturnDetails records for this PR number
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM PReturnDetails WHERE PReturnNo = @PReturnNo AND CompanyId = @CompanyId AND FinYearId = @FinYearId AND BranchID = @BranchID", (SqlConnection)DataConnection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@PReturnNo", pr.PReturnNo);
                        cmd.Parameters.AddWithValue("@CompanyId", pr.CompanyId);
                        cmd.Parameters.AddWithValue("@FinYearId", pr.FinYearId);
                        cmd.Parameters.AddWithValue("@BranchID", pr.BranchId);
                        cmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Deleted existing details for PR# {pr.PReturnNo}, FinYearId: {pr.FinYearId}");
                    }
                }
                else
                {
                    // Use the stored transaction from previous call
                    transaction = activeTransaction;
                    System.Diagnostics.Debug.WriteLine($"Using stored transaction: {(transaction != null ? "Valid" : "NULL")}");

                    if (transaction == null)
                    {
                        // If the transaction is null but we need to commit, we'll create a new one
                        // This is a recovery mechanism in case we lost our active transaction
                        System.Diagnostics.Debug.WriteLine("Warning: Active transaction was null when trying to process non-first record. Creating new transaction.");
                        transaction = (SqlTransaction)DataConnection.BeginTransaction();
                        activeTransaction = transaction; // Store for subsequent calls
                    }
                }

                // First, let's get the actual parameter names from the stored procedure
                HashSet<string> actualParameterNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                try
                {
                    string paramQuery = @"
                        SELECT PARAMETER_NAME 
                        FROM INFORMATION_SCHEMA.PARAMETERS 
                        WHERE SPECIFIC_NAME = '_PurchaseReturn_PReturnDetails_ItemBatch'
                        ORDER BY ORDINAL_POSITION";

                    using (SqlCommand paramCmd = new SqlCommand(paramQuery, (SqlConnection)DataConnection, transaction))
                    {
                        using (SqlDataReader reader = paramCmd.ExecuteReader())
                        {
                            System.Diagnostics.Debug.WriteLine("Reading stored procedure parameters:");
                            while (reader.Read())
                            {
                                string paramName = reader.GetString(0).TrimStart('@');
                                actualParameterNames.Add(paramName);
                                System.Diagnostics.Debug.WriteLine($"Found parameter: {paramName}");
                            }
                        }
                    }

                    // If we couldn't find any parameters, try checking the definition directly
                    if (actualParameterNames.Count == 0)
                    {
                        string defQuery = @"
                            SELECT ROUTINE_DEFINITION 
                            FROM INFORMATION_SCHEMA.ROUTINES 
                            WHERE ROUTINE_NAME = '_PurchaseReturn_PReturnDetails_ItemBatch'";

                        using (SqlCommand defCmd = new SqlCommand(defQuery, (SqlConnection)DataConnection, transaction))
                        {
                            object result = defCmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                string definition = result.ToString();
                                System.Diagnostics.Debug.WriteLine("Found procedure definition. Analyzing...");

                                // Look for parameter declarations
                                int paramStart = definition.IndexOf("@");
                                while (paramStart >= 0)
                                {
                                    int paramEnd = definition.IndexOfAny(new[] { ' ', ',', ')', '\r', '\n', '\t' }, paramStart);
                                    if (paramEnd > paramStart)
                                    {
                                        string param = definition.Substring(paramStart + 1, paramEnd - paramStart - 1);
                                        if (!string.IsNullOrWhiteSpace(param))
                                        {
                                            actualParameterNames.Add(param);
                                            System.Diagnostics.Debug.WriteLine($"Found parameter in definition: {param}");
                                        }
                                    }
                                    paramStart = definition.IndexOf("@", paramStart + 1);
                                }
                            }
                        }
                    }
                }
                catch (Exception paramEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Warning: Could not retrieve stored procedure parameters: {paramEx.Message}");
                    // Continue anyway with our best guess at parameter names
                }

                // Insert the detail record if valid
                if (details != null && details.ItemID > 0)
                {
                    // Ensure that Reason is not empty (it's mandatory)
                    if (string.IsNullOrWhiteSpace(details.Reason))
                    {
                        details.Reason = "Purchase Return"; // Default reason
                        System.Diagnostics.Debug.WriteLine("Reason was empty, setting default value: 'Purchase Return'");
                    }

                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_PReturnDetails, (SqlConnection)DataConnection, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Add core parameters - these are almost certainly required
                        cmd.Parameters.AddWithValue("CompanyId", details.CompanyId);
                        cmd.Parameters.AddWithValue("FinYearId", pr.FinYearId); // Use master's FinYearId which we've verified
                        System.Diagnostics.Debug.WriteLine($"Using verified FinYearId: {pr.FinYearId}");
                        cmd.Parameters.AddWithValue("BranchID", details.BranchID);
                        cmd.Parameters.AddWithValue("PReturnNo", details.PReturnNo);
                        cmd.Parameters.AddWithValue("PReturnDate", details.PReturnDate);
                        cmd.Parameters.AddWithValue("InvoiceNo", details.InvoiceNo ?? "");
                        cmd.Parameters.AddWithValue("SlNo", details.SlNo);
                        cmd.Parameters.AddWithValue("ItemID", details.ItemID);

                        // Add item details parameters
                        cmd.Parameters.AddWithValue("ItemName", details.Description ?? "");
                        cmd.Parameters.AddWithValue("UnitId", details.UnitId);
                        cmd.Parameters.AddWithValue("BaseUnit", details.BaseUnit ? "Y" : "N");
                        cmd.Parameters.AddWithValue("Packing", details.Packing);
                        cmd.Parameters.AddWithValue("IsExpiry", details.IsExpiry);
                        cmd.Parameters.AddWithValue("BatchNo", details.BatchNo ?? "");
                        cmd.Parameters.AddWithValue("Expiry", details.Expiry == null ? DBNull.Value : (object)details.Expiry);

                        // Add quantity and financial parameters
                        cmd.Parameters.AddWithValue("Qty", details.Qty);
                        cmd.Parameters.AddWithValue("TaxPer", details.TaxPer);
                        cmd.Parameters.AddWithValue("TaxAmt", details.TaxAmt);
                        cmd.Parameters.AddWithValue("Reason", details.Reason); // Now guaranteed to have a value
                        cmd.Parameters.AddWithValue("Free", details.Free);
                        cmd.Parameters.AddWithValue("Cost", details.Cost);
                        cmd.Parameters.AddWithValue("DisPer", details.DisPer);
                        cmd.Parameters.AddWithValue("DisAmt", details.DisAmt);
                        cmd.Parameters.AddWithValue("SalesPrice", details.SalesPrice);
                        cmd.Parameters.AddWithValue("OriginalCost", details.OriginalCost);
                        cmd.Parameters.AddWithValue("TotalSP", details.TotalSP);

                        // Add cess parameters
                        cmd.Parameters.AddWithValue("CessAmt", details.CessAmt);
                        cmd.Parameters.AddWithValue("CessPer", details.CessPer);

                        // Add Returnqty and Returned parameters
                        cmd.Parameters.AddWithValue("Returnqty", details.Returnqty);
                        cmd.Parameters.AddWithValue("Returned", details.Returned);

                        // Add TaxType parameter which is required and cannot be NULL
                        // Use the TaxType from the master record or default to "I" (Inclusive)
                        cmd.Parameters.AddWithValue("TaxType", pr.TaxType ?? "I");
                        System.Diagnostics.Debug.WriteLine($"Using TaxType: {pr.TaxType ?? "I"}");

                        // Add SeriesID parameter which is also required and cannot be NULL
                        cmd.Parameters.AddWithValue("SeriesID", pr.SeriesID > 0 ? pr.SeriesID : 1);
                        System.Diagnostics.Debug.WriteLine($"Using SeriesID: {(pr.SeriesID > 0 ? pr.SeriesID : 1)}");

                        // Add TrnsType parameter which is required by the SP
                        cmd.Parameters.AddWithValue("TrnsType", "PR");

                        // Operation parameter
                        cmd.Parameters.AddWithValue("_Operation", "CREATE");

                        try
                        {
                            SqlDataAdapter da = new SqlDataAdapter(cmd);
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            System.Diagnostics.Debug.WriteLine($"Inserted detail record for ItemID: {details.ItemID}, Qty: {details.Qty}, Reason: {details.Reason}, Returned: {details.Returned}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error inserting detail record: {ex.Message}");

                            // Print all parameters that were sent to help diagnose issues
                            System.Diagnostics.Debug.WriteLine("Parameters sent to stored procedure:");
                            foreach (SqlParameter param in cmd.Parameters)
                            {
                                System.Diagnostics.Debug.WriteLine($"{param.ParameterName} = {param.Value}");
                            }

                            throw; // Rethrow the original exception
                        }

                        // Reduce stock in PriceSettings table for purchase return update
                        // This is critical for inventory accuracy
                        ReducePriceSettingsStock(details.ItemID, pr.BranchId, details.UnitId, details.Returned, transaction);

                        // Update PDetails table with returned quantity if this is linked to a purchase invoice
                        // Check both PInvoice and InvoiceNo for the purchase number
                        int purchaseNo = 0;
                        bool isValidPurchaseNo = false;

                        // Try to get purchase number from PInvoice first
                        if (!string.IsNullOrEmpty(pr.PInvoice) &&
                            pr.PInvoice != "WITHOUT GR" &&
                            int.TryParse(pr.PInvoice, out purchaseNo) &&
                            purchaseNo > 0)
                        {
                            isValidPurchaseNo = true;
                        }
                        // If PInvoice doesn't have a valid purchase number, try InvoiceNo
                        else if (!string.IsNullOrEmpty(pr.InvoiceNo) &&
                                 pr.InvoiceNo != "WITHOUT GR" &&
                                 int.TryParse(pr.InvoiceNo, out purchaseNo) &&
                                 purchaseNo > 0)
                        {
                            isValidPurchaseNo = true;
                        }

                        // Update PDetails if we have a valid purchase number and return quantity
                        // details.Returned contains only the NEW return qty (not cumulative)
                        if (isValidPurchaseNo && details.Returned > 0)
                        {
                            try
                            {
                                // Update PDetails table: add the returned quantity to the existing Returned value
                                // Match by PurchaseNo and ItemID (SlNo might differ between PDetails and PReturnDetails)
                                string updatePDetailsQuery = @"
                                    UPDATE PDetails 
                                    SET Returned = ISNULL(Returned, 0) + @ReturnedQty
                                    WHERE PurchaseNo = @PurchaseNo 
                                    AND ItemID = @ItemID";

                                using (SqlCommand updateCmd = new SqlCommand(updatePDetailsQuery, (SqlConnection)DataConnection, transaction))
                                {
                                    updateCmd.Parameters.AddWithValue("@ReturnedQty", details.Returned);
                                    updateCmd.Parameters.AddWithValue("@PurchaseNo", purchaseNo);
                                    updateCmd.Parameters.AddWithValue("@ItemID", details.ItemID);

                                    int rowsAffected = updateCmd.ExecuteNonQuery();
                                    if (rowsAffected > 0)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"Updated PDetails for PurchaseNo: {purchaseNo}, ItemID: {details.ItemID}, Added ReturnedQty: {details.Returned}");
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine($"Warning: No rows updated in PDetails for PurchaseNo: {purchaseNo}, ItemID: {details.ItemID}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Warning: Error updating PDetails with returned quantity: {ex.Message}");
                                // Continue despite PDetails update error - just log it
                            }
                        }
                    }
                }
                else if (!isFirstRecord) // This is the final call to commit the transaction
                {
                    System.Diagnostics.Debug.WriteLine("Final call detected (details.ItemID <= 0 and !isFirstRecord). Preparing to commit transaction.");
                }

                // Commit the transaction only if this is the last record
                if (isFirstRecord == false && transaction != null)
                {
                    try
                    {
                        // Before committing, regenerate vouchers for the updated purchase return
                        System.Diagnostics.Debug.WriteLine($"Regenerating vouchers for PR#{pr.PReturnNo}");
                        RegenerateVouchersForUpdate(pr.PReturnNo, pr.CompanyId, pr.FinYearId, pr.BranchId, transaction);

                        transaction.Commit();
                        System.Diagnostics.Debug.WriteLine("Transaction committed successfully with regenerated vouchers");
                    }
                    catch (Exception commitEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error committing transaction: {commitEx.Message}");
                        // If we can't commit, throw to be caught by outer try/catch
                        throw;
                    }
                    finally
                    {
                        // Clear the stored transaction so next update starts fresh
                        activeTransaction = null;
                    }
                }
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    try
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine($"Transaction rolled back due to error: {ex.Message}");
                    }
                    catch (Exception rollbackEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error rolling back transaction: {rollbackEx.Message}");
                    }
                }

                // Clear the stored transaction on error
                activeTransaction = null;
                throw new Exception($"Error in UpdatePurchaseReturnDetails: {ex.Message}", ex);
            }
            finally
            {
                // Always close the connection if we opened it here AND this is the final call
                // (for the first record with subsequent calls pending, we keep it open)
                if (closeConnection && ((isFirstRecord == false) || (details == null || details.ItemID <= 0)))
                {
                    if (DataConnection.State == ConnectionState.Open)
                    {
                        DataConnection.Close();
                        System.Diagnostics.Debug.WriteLine("Connection closed");
                    }
                }
            }
        }

        // Helper method to get previously returned quantity for an item in a purchase return
        private double GetPreviouslyReturnedQty(int prReturnNo, long itemId, SqlTransaction trans = null)
        {
            double totalReturned = 0.0;

            try
            {
                // Use the provided transaction or create a new connection
                SqlConnection connection = trans != null ? trans.Connection : (SqlConnection)DataConnection;
                bool shouldCloseConnection = false;

                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                    shouldCloseConnection = true;
                }

                try
                {
                    // Query to get total returned quantity for this item from this specific purchase return
                    string query = @"
                        SELECT ISNULL(SUM(Returned), 0) AS TotalReturned
                        FROM PReturnDetails
                        WHERE PReturnNo = @PReturnNo
                        AND ItemID = @ItemId
                        AND CompanyId = @CompanyId
                        AND BranchID = @BranchId
                        AND FinYearId = @FinYearId";

                    using (SqlCommand cmd = new SqlCommand(query, connection, trans))
                    {
                        cmd.Parameters.AddWithValue("@PReturnNo", prReturnNo);
                        cmd.Parameters.AddWithValue("@ItemId", itemId);
                        cmd.Parameters.AddWithValue("@CompanyId", Convert.ToInt32(DataBase.CompanyId));
                        cmd.Parameters.AddWithValue("@BranchId", Convert.ToInt32(DataBase.BranchId));
                        cmd.Parameters.AddWithValue("@FinYearId", Convert.ToInt32(DataBase.FinyearId));

                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            totalReturned = Convert.ToDouble(result);
                        }
                    }
                }
                finally
                {
                    if (shouldCloseConnection && connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating previously returned qty: {ex.Message}");
                // Return 0 if there's an error
                totalReturned = 0.0;
            }

            return totalReturned;
        }

        /// <summary>
        /// Gets the ledger name for INPUT CGST or INPUT SGST based on tax percentage
        /// Tax percentage is split equally between CGST and SGST
        /// </summary>
        /// <summary>
        /// Regenerates vouchers for an updated purchase return by deleting old vouchers and creating new ones
        /// </summary>
        private void RegenerateVouchersForUpdate(int prNo, int companyId, int finYearId, int branchId, SqlTransaction trans)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Starting voucher regeneration for PR#{prNo}");

                // First, get the existing master record to get VoucherID and other details
                PReturnMaster existingPR = null;
                string getMasterQuery = @"
                    SELECT * FROM PReturnMaster 
                    WHERE PReturnNo = @PReturnNo 
                    AND CompanyId = @CompanyId 
                    AND FinYearId = @FinYearId 
                    AND BranchId = @BranchId";

                using (SqlCommand cmd = new SqlCommand(getMasterQuery, (SqlConnection)DataConnection, trans))
                {
                    cmd.Parameters.AddWithValue("@PReturnNo", prNo);
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            DataRow row = dt.Rows[0];
                            existingPR = new PReturnMaster
                            {
                                PReturnNo = Convert.ToInt32(row["PReturnNo"]),
                                CompanyId = Convert.ToInt32(row["CompanyId"]),
                                FinYearId = Convert.ToInt32(row["FinYearId"]),
                                BranchId = Convert.ToInt32(row["BranchId"]),
                                VoucherID = row["VoucherID"] != DBNull.Value ? Convert.ToInt64(row["VoucherID"]) : 0,
                                PReturnDate = Convert.ToDateTime(row["PReturnDate"]),
                                LedgerID = Convert.ToInt64(row["LedgerID"]),
                                VendorName = row["VendorName"].ToString(),
                                PaymodeID = Convert.ToInt32(row["PaymodeID"]),
                                Paymode = row["Paymode"].ToString(),
                                PaymodeLedgerID = row["PaymodeLedgerID"] != DBNull.Value ? Convert.ToInt32(row["PaymodeLedgerID"]) : 0,
                                GrandTotal = Convert.ToDouble(row["GrandTotal"]),
                                Remarks = row["Remarks"] != DBNull.Value ? row["Remarks"].ToString() : ""
                            };
                        }
                    }
                }

                if (existingPR == null || existingPR.VoucherID <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Could not find existing PR master record or VoucherID for PR#{prNo}");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Found existing VoucherID: {existingPR.VoucherID} for PR#{prNo}");

                // Delete all existing voucher entries for this VoucherID
                string deleteVouchersQuery = @"
                    DELETE FROM Vouchers 
                    WHERE VoucherID = @VoucherID 
                    AND BranchID = @BranchId 
                    AND VoucherType = @VoucherType";

                using (SqlCommand deleteCmd = new SqlCommand(deleteVouchersQuery, (SqlConnection)DataConnection, trans))
                {
                    deleteCmd.Parameters.AddWithValue("@VoucherID", existingPR.VoucherID);
                    deleteCmd.Parameters.AddWithValue("@BranchId", branchId);
                    deleteCmd.Parameters.AddWithValue("@VoucherType", ModelClass.VoucherType.DebitNote);

                    int deletedRows = deleteCmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"Deleted {deletedRows} existing voucher entries for VoucherID {existingPR.VoucherID}");
                }

                // Now create new voucher entries
                int voucherId = (int)existingPR.VoucherID;

                // Determine which ledger to credit based on payment method's PaymodeLedgerID
                int creditLedgerId;
                string creditLedgerName;

                if (existingPR.PaymodeLedgerID > 0)
                {
                    creditLedgerId = existingPR.PaymodeLedgerID;
                    creditLedgerName = existingPR.Paymode;
                    System.Diagnostics.Debug.WriteLine($"Using PaymodeLedgerID: {creditLedgerId} for payment method: {existingPR.Paymode}");
                }
                else
                {
                    creditLedgerId = (int)existingPR.LedgerID;
                    creditLedgerName = existingPR.VendorName;
                    System.Diagnostics.Debug.WriteLine($"Using Vendor ledger (ID: {creditLedgerId}) as fallback");
                }

                // Get SubTotal and TaxAmt from the master record to calculate purchase amount without tax
                double subTotal = 0;
                double taxAmt = 0;

                string getAmountsQuery = @"
                    SELECT SubTotal, TaxAmt 
                    FROM PReturnMaster 
                    WHERE PReturnNo = @PReturnNo 
                    AND CompanyId = @CompanyId 
                    AND FinYearId = @FinYearId 
                    AND BranchId = @BranchId";

                using (SqlCommand amtCmd = new SqlCommand(getAmountsQuery, (SqlConnection)DataConnection, trans))
                {
                    amtCmd.Parameters.AddWithValue("@PReturnNo", prNo);
                    amtCmd.Parameters.AddWithValue("@CompanyId", companyId);
                    amtCmd.Parameters.AddWithValue("@FinYearId", finYearId);
                    amtCmd.Parameters.AddWithValue("@BranchId", branchId);

                    using (SqlDataReader reader = amtCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            subTotal = reader["SubTotal"] != DBNull.Value ? Convert.ToDouble(reader["SubTotal"]) : 0;
                            taxAmt = reader["TaxAmt"] != DBNull.Value ? Convert.ToDouble(reader["TaxAmt"]) : 0;
                        }
                    }
                }

                // Calculate amount without tax for Purchase Return ledger (SubTotal - TaxAmt)
                double purchaseAmountWithoutTax = subTotal - taxAmt;

                System.Diagnostics.Debug.WriteLine($"Voucher Calculation: SubTotal={subTotal}, TaxAmt={taxAmt}, PurchaseAmtWithoutTax={purchaseAmountWithoutTax}, GrandTotal={existingPR.GrandTotal}");

                // Create payment method account voucher (DEBIT for return - money received back)
                PReturnVoucher vendorVoucher = new PReturnVoucher
                {
                    CompanyId = companyId,
                    FinYearId = finYearId,
                    BranchID = branchId,
                    VoucherID = voucherId,
                    VoucherType = ModelClass.VoucherType.DebitNote,
                    VoucherDate = existingPR.PReturnDate,
                    ReferenceNo = prNo.ToString(),
                    LedgerID = creditLedgerId,
                    Debit = existingPR.GrandTotal,
                    Credit = 0,
                    Narration = $"Purchase Return - PR#{prNo}",
                    CancelFlag = false
                };

                CreateVoucher(vendorVoucher, trans);
                System.Diagnostics.Debug.WriteLine($"Created vendor/payment voucher (Debit: {existingPR.GrandTotal})");

                // Create purchase return account voucher (CREDIT for return - reducing purchase account)
                // Amount = SubTotal - TaxAmt (purchase amount without tax)
                int purchaseReturnLedgerId = GetPurchaseReturnLedgerId(branchId);
                if (purchaseReturnLedgerId <= 0)
                {
                    throw new Exception("Purchase Return ledger not found. Please configure Purchase Return ledger in the system.");
                }

                PReturnVoucher prVoucher = new PReturnVoucher
                {
                    CompanyId = companyId,
                    FinYearId = finYearId,
                    BranchID = branchId,
                    VoucherID = voucherId,
                    VoucherType = ModelClass.VoucherType.DebitNote,
                    VoucherDate = existingPR.PReturnDate,
                    ReferenceNo = prNo.ToString(),
                    LedgerID = purchaseReturnLedgerId,
                    Debit = 0,
                    Credit = purchaseAmountWithoutTax,
                    Narration = $"Purchase Return - PR#{prNo}",
                    CancelFlag = false
                };

                CreateVoucher(prVoucher, trans);
                System.Diagnostics.Debug.WriteLine($"Created purchase voucher (Credit: {purchaseAmountWithoutTax})");

                // Create tax voucher entries for CGST and SGST from updated details
                Dictionary<double, double> taxAmountsByPercentage = AggregateTaxAmountsFromDetails(prNo, companyId, finYearId, branchId, trans);
                if (taxAmountsByPercentage.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Creating tax vouchers for {taxAmountsByPercentage.Count} tax percentage(s)");
                    CreateTaxVoucherEntries(voucherId, taxAmountsByPercentage, prNo, existingPR.Remarks ?? "", trans);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No tax amounts found in updated details, skipping tax voucher creation");
                }

                System.Diagnostics.Debug.WriteLine($"Completed voucher regeneration for PR#{prNo}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error regenerating vouchers for PR#{prNo}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Aggregates tax amounts by tax percentage from purchase return details stored in database
        /// This is used during update operations to recalculate CGST/SGST vouchers
        /// </summary>
        private Dictionary<double, double> AggregateTaxAmountsFromDetails(int prNo, int companyId, int finYearId, int branchId, SqlTransaction trans)
        {
            Dictionary<double, double> taxAmountsByPercentage = new Dictionary<double, double>();

            try
            {
                string query = @"
                    SELECT TaxPer, TaxAmt 
                    FROM PReturnDetails 
                    WHERE PReturnNo = @PReturnNo 
                    AND CompanyId = @CompanyId 
                    AND FinYearId = @FinYearId 
                    AND BranchID = @BranchID
                    AND ItemID IS NOT NULL
                    AND ItemID > 0";

                using (SqlCommand cmd = new SqlCommand(query, (SqlConnection)DataConnection, trans))
                {
                    cmd.Parameters.AddWithValue("@PReturnNo", prNo);
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                    cmd.Parameters.AddWithValue("@BranchID", branchId);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            double taxPer = row["TaxPer"] != DBNull.Value ? Convert.ToDouble(row["TaxPer"]) : 0;
                            double taxAmt = row["TaxAmt"] != DBNull.Value ? Convert.ToDouble(row["TaxAmt"]) : 0;

                            if (taxPer > 0 && taxAmt > 0)
                            {
                                // Round tax percentage to 1 decimal place for grouping
                                double roundedTaxPer = Math.Round(taxPer, 1);

                                // Aggregate tax amounts by percentage
                                if (taxAmountsByPercentage.ContainsKey(roundedTaxPer))
                                {
                                    taxAmountsByPercentage[roundedTaxPer] += taxAmt;
                                }
                                else
                                {
                                    taxAmountsByPercentage[roundedTaxPer] = taxAmt;
                                }

                                System.Diagnostics.Debug.WriteLine($"Found tax: {taxPer}% = {taxAmt}, Aggregated: {taxAmountsByPercentage[roundedTaxPer]}");
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Aggregated {taxAmountsByPercentage.Count} different tax percentages from details");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error aggregating tax amounts from details: {ex.Message}");
            }

            return taxAmountsByPercentage;
        }

        private string GetTaxLedgerName(double taxPercentage, bool isCGST)
        {
            // Calculate half of tax percentage for CGST/SGST
            double halfTaxPer = taxPercentage / 2.0;

            // Round to 1 decimal place to match ledger naming convention
            halfTaxPer = Math.Round(halfTaxPer, 1);

            // Format the ledger name: "INPUT CGST X%" or "INPUT SGST X%"
            string ledgerName = isCGST ? $"INPUT CGST {halfTaxPer}%" : $"INPUT SGST {halfTaxPer}%";

            return ledgerName;
        }

        /// <summary>
        /// Aggregates tax amounts by tax percentage from purchase return items in DataGridView
        /// Returns a dictionary where key is tax percentage and value is total tax amount
        /// </summary>
        private Dictionary<double, double> AggregateTaxAmountsByPercentage(DataGridView dgvInvoice)
        {
            Dictionary<double, double> taxAmountsByPercentage = new Dictionary<double, double>();

            if (dgvInvoice == null || dgvInvoice.Rows.Count == 0)
                return taxAmountsByPercentage;

            for (int i = 0; i < dgvInvoice.Rows.Count; i++)
            {
                try
                {
                    // Skip rows without ItemId or empty rows
                    if (dgvInvoice.Rows[i].IsNewRow ||
                        dgvInvoice.Rows[i].Cells["ItemID"] == null ||
                        dgvInvoice.Rows[i].Cells["ItemID"].Value == null ||
                        string.IsNullOrEmpty(dgvInvoice.Rows[i].Cells["ItemID"].Value.ToString()))
                    {
                        continue;
                    }

                    // Check if the row is selected via the SELECT column
                    bool isSelected = false;
                    if (dgvInvoice.Rows[i].Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "SELECT"))
                    {
                        var selectValue = dgvInvoice.Rows[i].Cells["SELECT"].Value;
                        isSelected = selectValue != null && selectValue != DBNull.Value && Convert.ToBoolean(selectValue);
                    }

                    // Only process selected rows
                    if (!isSelected)
                        continue;

                    // Get tax percentage and tax amount
                    double taxPer = 0;
                    if (dgvInvoice.Rows[i].Cells["TaxPer"] != null && dgvInvoice.Rows[i].Cells["TaxPer"].Value != null)
                    {
                        double.TryParse(dgvInvoice.Rows[i].Cells["TaxPer"].Value.ToString(), out taxPer);
                    }

                    double taxAmt = 0;
                    if (dgvInvoice.Rows[i].Cells["TaxAmt"] != null && dgvInvoice.Rows[i].Cells["TaxAmt"].Value != null)
                    {
                        double.TryParse(dgvInvoice.Rows[i].Cells["TaxAmt"].Value.ToString(), out taxAmt);
                    }

                    // Only process if tax percentage and tax amount are greater than 0
                    if (taxPer > 0 && taxAmt > 0)
                    {
                        // Round tax percentage to 1 decimal place for grouping
                        double roundedTaxPer = Math.Round(taxPer, 1);

                        // Aggregate tax amounts by percentage
                        if (taxAmountsByPercentage.ContainsKey(roundedTaxPer))
                        {
                            taxAmountsByPercentage[roundedTaxPer] += taxAmt;
                        }
                        else
                        {
                            taxAmountsByPercentage[roundedTaxPer] = taxAmt;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error aggregating tax for row {i}: {ex.Message}");
                    continue;
                }
            }

            return taxAmountsByPercentage;
        }

        /// <summary>
        /// Creates voucher entries for tax amounts (CGST and SGST) under DUTIES & TAXES group
        /// For purchase return, tax is credited (reducing tax liability)
        /// </summary>
        private void CreateTaxVoucherEntries(int voucherId, Dictionary<double, double> taxAmountsByPercentage, int prNo, string remarks, SqlTransaction trans)
        {
            try
            {
                // Use the proper AccountGroup enum value for DUTIES & TAXES
                int dutiesAndTaxesGroupId = (int)AccountGroup.DUTIES_AND_TAXES;

                foreach (var taxEntry in taxAmountsByPercentage)
                {
                    double taxPercentage = taxEntry.Key;
                    double totalTaxAmount = taxEntry.Value;

                    // Split tax amount equally between CGST and SGST
                    double cgstAmount = totalTaxAmount / 2.0;
                    double sgstAmount = totalTaxAmount / 2.0;

                    // Get ledger names for CGST and SGST
                    string cgstLedgerName = GetTaxLedgerName(taxPercentage, true);
                    string sgstLedgerName = GetTaxLedgerName(taxPercentage, false);

                    // Get ledger IDs
                    int cgstLedgerId = objLedgerRepository.GetLedgerId(cgstLedgerName, dutiesAndTaxesGroupId, Convert.ToInt32(DataBase.BranchId));
                    int sgstLedgerId = objLedgerRepository.GetLedgerId(sgstLedgerName, dutiesAndTaxesGroupId, Convert.ToInt32(DataBase.BranchId));

                    System.Diagnostics.Debug.WriteLine($"Tax {taxPercentage}%: CGST Ledger='{cgstLedgerName}' (ID:{cgstLedgerId}), SGST Ledger='{sgstLedgerName}' (ID:{sgstLedgerId})");

                    // Create CGST voucher entry (Credit for purchase return - reducing input tax)
                    if (cgstLedgerId > 0 && cgstAmount > 0)
                    {
                        PReturnVoucher cgstVoucher = new PReturnVoucher
                        {
                            CompanyId = Convert.ToInt32(DataBase.CompanyId),
                            FinYearId = Convert.ToInt32(DataBase.FinyearId),
                            BranchID = Convert.ToInt32(DataBase.BranchId),
                            VoucherID = voucherId,
                            VoucherType = ModelClass.VoucherType.DebitNote,
                            VoucherDate = DateTime.Now,
                            ReferenceNo = prNo.ToString(),
                            LedgerID = cgstLedgerId,
                            Debit = 0,
                            Credit = cgstAmount,
                            Narration = $"PURCHASE RETURN: PR#{prNo}| TAX: {cgstLedgerName}| REMARKS: {remarks}",
                            CancelFlag = false,
                            _Operation = "CREATE"
                        };

                        CreateVoucher(cgstVoucher, trans);
                        System.Diagnostics.Debug.WriteLine($"Created CGST voucher: {cgstLedgerName}, Amount: {cgstAmount}");
                    }
                    else if (cgstLedgerId <= 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: CGST Ledger '{cgstLedgerName}' not found in database. Skipping CGST voucher entry.");
                    }

                    // Create SGST voucher entry (Credit for purchase return - reducing input tax)
                    if (sgstLedgerId > 0 && sgstAmount > 0)
                    {
                        PReturnVoucher sgstVoucher = new PReturnVoucher
                        {
                            CompanyId = Convert.ToInt32(DataBase.CompanyId),
                            FinYearId = Convert.ToInt32(DataBase.FinyearId),
                            BranchID = Convert.ToInt32(DataBase.BranchId),
                            VoucherID = voucherId,
                            VoucherType = ModelClass.VoucherType.DebitNote,
                            VoucherDate = DateTime.Now,
                            ReferenceNo = prNo.ToString(),
                            LedgerID = sgstLedgerId,
                            Debit = 0,
                            Credit = sgstAmount,
                            Narration = $"PURCHASE RETURN: PR#{prNo}| TAX: {sgstLedgerName}| REMARKS: {remarks}",
                            CancelFlag = false,
                            _Operation = "CREATE"
                        };

                        CreateVoucher(sgstVoucher, trans);
                        System.Diagnostics.Debug.WriteLine($"Created SGST voucher: {sgstLedgerName}, Amount: {sgstAmount}");
                    }
                    else if (sgstLedgerId <= 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: SGST Ledger '{sgstLedgerName}' not found in database. Skipping SGST voucher entry.");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating tax voucher entries: {ex.Message}");
                throw; // Re-throw to ensure transaction rollback
            }
        }

        // Helper method to generate a new PR number after saving
        public int GenerateNewPRNo()
        {
            // Generate a new PR number and update TrackTrans
            int newPRNo = GeneratePReturnNo();
            System.Diagnostics.Debug.WriteLine($"Generated new PR number after save: {newPRNo}");
            return newPRNo;
        }

        // Method to clear form data - to be called from form after successful save
        public void ClearAfterSave()
        {
            try
            {
                // Close any open connections 
                if (DataConnection.State == ConnectionState.Open)
                {
                    // If there's an active transaction, try to commit it first
                    if (activeTransaction != null)
                    {
                        try
                        {
                            activeTransaction.Commit();
                            System.Diagnostics.Debug.WriteLine("Committed pending transaction in ClearAfterSave");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error committing transaction in ClearAfterSave: {ex.Message}");
                            // Try to roll back instead
                            try
                            {
                                activeTransaction.Rollback();
                                System.Diagnostics.Debug.WriteLine("Rolled back pending transaction in ClearAfterSave");
                            }
                            catch
                            {
                                // Ignore rollback errors
                            }
                        }
                        finally
                        {
                            // Always clear the transaction reference
                            activeTransaction = null;
                        }
                    }

                    // Close the connection
                    DataConnection.Close();
                    System.Diagnostics.Debug.WriteLine("Closed connection in ClearAfterSave");
                }

                // Generate a new PR number for the next transaction
                int newPRNo = GenerateNewPRNo();
                System.Diagnostics.Debug.WriteLine($"Form cleared with new PR number: {newPRNo}");

                // This method just generates the number
                // The form will need to use this number to update its UI
                PReturnNo = newPRNo;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ClearAfterSave: {ex.Message}");
                // Try to ensure the connection is closed anyway
                try
                {
                    if (DataConnection.State == ConnectionState.Open)
                    {
                        DataConnection.Close();
                        System.Diagnostics.Debug.WriteLine("Closed connection after error in ClearAfterSave");
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }

                // Clear the static transaction reference as a safety measure
                activeTransaction = null;
            }
        }

        // Method to get PR ID by PR Number
        public int GetIdByPReturnNo(int prReturnNo)
        {
            try
            {
                DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT Id FROM PReturnMaster WHERE PReturnNo = @PReturnNo", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@PReturnNo", prReturnNo);
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        int id = Convert.ToInt32(result);
                        System.Diagnostics.Debug.WriteLine($"GetIdByPReturnNo: Found ID {id} for PR#{prReturnNo}");
                        return id;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"GetIdByPReturnNo: No record found for PR#{prReturnNo}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetIdByPReturnNo: {ex.Message}");
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return 0;
        }

        /// <summary>
        /// Gets existing Purchase Return record by purchase number/invoice number
        /// Returns the PReturnNo if found, otherwise returns 0
        /// </summary>
        public int GetExistingPRByPurchaseNo(int purchaseNo, int companyId, int finYearId, int branchId)
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                {
                    DataConnection.Open();
                }

                // Check for existing PR by matching PInvoice or InvoiceNo with the purchase number
                string query = @"
                    SELECT TOP 1 PReturnNo 
                    FROM PReturnMaster 
                    WHERE (PInvoice = @PurchaseNo OR PInvoice = @PurchaseNoStr OR InvoiceNo = @PurchaseNoStr)
                        AND CompanyId = @CompanyId
                        AND FinYearId = @FinYearId
                        AND BranchId = @BranchId
                        AND CancelFlag = 0
                    ORDER BY PReturnNo DESC";

                using (SqlCommand cmd = new SqlCommand(query, (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@PurchaseNo", purchaseNo);
                    cmd.Parameters.AddWithValue("@PurchaseNoStr", purchaseNo.ToString());
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        int existingPRNo = Convert.ToInt32(result);
                        System.Diagnostics.Debug.WriteLine($"GetExistingPRByPurchaseNo: Found existing PR#{existingPRNo} for Purchase#{purchaseNo}");
                        return existingPRNo;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"GetExistingPRByPurchaseNo: No existing PR found for Purchase#{purchaseNo}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetExistingPRByPurchaseNo: {ex.Message}");
            }

            return 0;
        }

        // Simplified deletion method that only requires the PR number
        public string DeletePRByNumber(int prReturnNo)
        {
            DataConnection.Open();

            try
            {
                // Verify the PR exists
                int id = 0;
                int companyId = 0;
                int finYearId = 0;
                int branchId = 0;
                int voucherId = 0;

                // First get all the required data for deletion
                using (SqlCommand selectCmd = new SqlCommand("SELECT Id, CompanyId, FinYearId, BranchId, VoucherID FROM PReturnMaster WHERE PReturnNo = @PReturnNo", (SqlConnection)DataConnection))
                {
                    selectCmd.Parameters.AddWithValue("@PReturnNo", prReturnNo);
                    using (SqlDataReader reader = selectCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Use GetValue and safe conversions to avoid casting issues
                            id = Convert.ToInt32(reader.GetValue(0));
                            companyId = Convert.ToInt32(reader.GetValue(1));
                            finYearId = Convert.ToInt32(reader.GetValue(2));
                            branchId = Convert.ToInt32(reader.GetValue(3));

                            // Handle VoucherID safely
                            if (!reader.IsDBNull(4))
                            {
                                try
                                {
                                    // Try direct conversion first
                                    voucherId = Convert.ToInt32(reader.GetValue(4));
                                }
                                catch
                                {
                                    // If direct conversion fails, try through string
                                    voucherId = Convert.ToInt32(reader.GetValue(4).ToString());
                                }
                            }
                        }
                        else
                        {
                            return "Error: Purchase Return record not found";
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Found PR #{prReturnNo} with ID={id}, CompanyId={companyId}, FinYearId={finYearId}, BranchId={branchId}, VoucherId={voucherId}");

                // Instead of using complex SP, use direct SQL commands for simpler deletion
                // First cancel the master record
                using (SqlCommand cmdMaster = new SqlCommand("UPDATE PReturnMaster SET CancelFlag = 1 WHERE Id = @Id", (SqlConnection)DataConnection))
                {
                    cmdMaster.Parameters.AddWithValue("@Id", id);
                    cmdMaster.ExecuteNonQuery();
                }

                // Then delete the details
                using (SqlCommand cmdDetails = new SqlCommand("DELETE FROM PReturnDetails WHERE CompanyId = @CompanyId AND FinYearId = @FinYearId AND BranchID = @BranchId AND PReturnNo = @PReturnNo", (SqlConnection)DataConnection))
                {
                    cmdDetails.Parameters.AddWithValue("@CompanyId", companyId);
                    cmdDetails.Parameters.AddWithValue("@FinYearId", finYearId);
                    cmdDetails.Parameters.AddWithValue("@BranchId", branchId);
                    cmdDetails.Parameters.AddWithValue("@PReturnNo", prReturnNo);
                    cmdDetails.ExecuteNonQuery();
                }

                // Cancel any vouchers if they exist
                if (voucherId > 0)
                {
                    using (SqlCommand cmdVoucher = new SqlCommand("UPDATE Vouchers SET CancelFlag = 1 WHERE CompanyId = @CompanyId AND FinYearId = @FinYearId AND BranchID = @BranchId AND VoucherID = @VoucherId AND VoucherType = @VoucherType", (SqlConnection)DataConnection))
                    {
                        cmdVoucher.Parameters.AddWithValue("@CompanyId", companyId);
                        cmdVoucher.Parameters.AddWithValue("@FinYearId", finYearId);
                        cmdVoucher.Parameters.AddWithValue("@BranchId", branchId);
                        cmdVoucher.Parameters.AddWithValue("@VoucherId", voucherId);
                        cmdVoucher.Parameters.AddWithValue("@VoucherType", ModelClass.VoucherType.DebitNote);
                        cmdVoucher.ExecuteNonQuery();
                    }
                }

                return "success";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DeletePRByNumber: {ex.Message}");
                return "Error: " + ex.Message;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
        }

        // Direct SQL implementation that follows the provided stored procedure exactly
        public string DeleteByPRNumber(int prReturnNo)
        {
            DataConnection.Open();

            try
            {
                // Get the master record ID and other needed values
                int id = 0;
                int companyId = 0;
                int finYearId = 0;
                int branchId = 0;
                long voucherId = 0;

                using (SqlCommand cmd = new SqlCommand("SELECT Id, CompanyId, FinYearId, BranchId, VoucherID FROM PReturnMaster WHERE PReturnNo = @PReturnNo", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@PReturnNo", prReturnNo);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            id = reader.GetInt32(0);
                            companyId = reader.GetInt32(1);
                            finYearId = reader.GetInt32(2);
                            branchId = reader.GetInt32(3);
                            if (!reader.IsDBNull(4))
                                voucherId = reader.GetInt64(4);
                        }
                        else
                        {
                            return "Error: Purchase Return not found";
                        }
                    }
                }

                // Mark master record as cancelled
                using (SqlCommand cmd = new SqlCommand("UPDATE PReturnMaster SET CancelFlag = 1 WHERE Id = @Id", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }

                // Delete detail records (this matches your SQL exactly)
                using (SqlCommand cmd = new SqlCommand("DELETE FROM PReturnDetails WHERE CompanyId = @CompanyId AND FinYearId = @FinYearId AND BranchID = @BranchId AND PReturnNo = @PReturnNo", (SqlConnection)DataConnection))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@PReturnNo", prReturnNo);
                    cmd.ExecuteNonQuery();
                }

                // Mark any vouchers as cancelled (matches your SQL)
                if (voucherId > 0)
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE Vouchers SET CancelFlag = 1 WHERE CompanyId = @CompanyId AND FinYearId = @FinYearId AND BranchID = @BranchId AND VoucherID = @VoucherId AND VoucherType = @VoucherType", (SqlConnection)DataConnection))
                    {
                        cmd.Parameters.AddWithValue("@CompanyId", companyId);
                        cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                        cmd.Parameters.AddWithValue("@BranchId", branchId);
                        cmd.Parameters.AddWithValue("@VoucherId", voucherId);
                        cmd.Parameters.AddWithValue("@VoucherType", ModelClass.VoucherType.DebitNote);
                        cmd.ExecuteNonQuery();
                    }
                }

                return "success";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DeleteByPRNumber: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return "Error: " + ex.Message;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        // This implements the exact SQL provided by the user with a transaction
        public string DeletePRDirectSQL(int prReturnNo)
        {
            SqlTransaction transaction = null;

            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                // Start a transaction to ensure all operations succeed together
                transaction = ((SqlConnection)DataConnection).BeginTransaction();

                // First get the necessary data
                int id = 0;
                int companyId = 0;
                int finYearId = 0;
                int branchId = 0;
                object voucherId = null;

                string selectSql = "SELECT Id, CompanyId, FinYearId, BranchId, VoucherID FROM PReturnMaster WHERE PReturnNo = @PReturnNo";
                using (SqlCommand cmd = new SqlCommand(selectSql, (SqlConnection)DataConnection, transaction))
                {
                    cmd.Parameters.AddWithValue("@PReturnNo", prReturnNo);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Id"));
                            companyId = reader.GetInt32(reader.GetOrdinal("CompanyId"));
                            finYearId = reader.GetInt32(reader.GetOrdinal("FinYearId"));
                            branchId = reader.GetInt32(reader.GetOrdinal("BranchId"));

                            if (!reader.IsDBNull(reader.GetOrdinal("VoucherID")))
                                voucherId = reader.GetValue(reader.GetOrdinal("VoucherID"));
                        }
                        else
                        {
                            return "Error: Purchase return not found";
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Found PR#{prReturnNo} with ID={id}, CompanyId={companyId}, FinYearId={finYearId}, BranchId={branchId}, VoucherId={voucherId}");

                // 1. Update PReturnMaster - using the exact SQL provided
                string updateMasterSql = "UPDATE PReturnMaster SET CancelFlag = 1 WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(updateMasterSql, (SqlConnection)DataConnection, transaction))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"Updated {rowsAffected} rows in PReturnMaster");
                }

                // 2. Delete from PReturnDetails - using the exact SQL provided
                string deleteDetailsSql = "DELETE FROM PReturnDetails WHERE CompanyId = @CompanyId AND FinYearId = @FinYearId AND BranchID = @BranchId AND PReturnNo = @PReturnNo";
                using (SqlCommand cmd = new SqlCommand(deleteDetailsSql, (SqlConnection)DataConnection, transaction))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@PReturnNo", prReturnNo);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"Deleted {rowsAffected} rows from PReturnDetails");
                }

                // 3. Update Vouchers if VoucherID exists - using the exact SQL provided
                if (voucherId != null)
                {
                    string updateVouchersSql = "UPDATE Vouchers SET CancelFlag = 1 WHERE CompanyId = @CompanyId AND FinYearId = @FinYearId AND BranchID = @BranchId AND VoucherID = @VoucherId AND VoucherType = @VoucherType";
                    using (SqlCommand cmd = new SqlCommand(updateVouchersSql, (SqlConnection)DataConnection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@CompanyId", companyId);
                        cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                        cmd.Parameters.AddWithValue("@BranchId", branchId);
                        cmd.Parameters.AddWithValue("@VoucherId", voucherId);
                        cmd.Parameters.AddWithValue("@VoucherType", ModelClass.VoucherType.DebitNote);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Updated {rowsAffected} rows in Vouchers");
                    }
                }

                // Commit the transaction
                transaction.Commit();
                return "success";
            }
            catch (Exception ex)
            {
                // Roll back the transaction if there's an error
                if (transaction != null)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception rollbackEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Rollback error: {rollbackEx.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Error details: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                return $"Error: {ex.Message}";
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        public string UpdatePurchaseReturn(PReturnMaster master)
        {
            try
            {
                if (DataConnection.State != ConnectionState.Open)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand("_POS_PurchaseReturn", (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", master.Id);
                    cmd.Parameters.AddWithValue("@CompanyId", master.CompanyId);
                    cmd.Parameters.AddWithValue("@FinYearId", master.FinYearId);
                    cmd.Parameters.AddWithValue("@BranchId", master.BranchId);
                    cmd.Parameters.AddWithValue("@PReturnNo", master.PReturnNo);
                    cmd.Parameters.AddWithValue("@PReturnDate", master.PReturnDate);
                    cmd.Parameters.AddWithValue("@InvoiceNo", master.InvoiceNo);
                    cmd.Parameters.AddWithValue("@InvoiceDate", master.InvoiceDate);
                    cmd.Parameters.AddWithValue("@LedgerID", master.LedgerID);
                    cmd.Parameters.AddWithValue("@VendorName", master.VendorName);

                    // Ensure PaymodeLedgerID is set from PayMode table if not already set
                    if (master.PaymodeLedgerID <= 0 && master.PaymodeID > 0)
                    {
                        try
                        {
                            string ledgerQuery = @"
                                SELECT TOP 1 LedgerID 
                                FROM PayMode 
                                WHERE PayModeID = @PaymodeID";

                            using (SqlCommand ledgerCmd = new SqlCommand(ledgerQuery, (SqlConnection)DataConnection))
                            {
                                ledgerCmd.Parameters.AddWithValue("@PaymodeID", master.PaymodeID);
                                object ledgerResult = ledgerCmd.ExecuteScalar();
                                if (ledgerResult != null && ledgerResult != DBNull.Value)
                                {
                                    master.PaymodeLedgerID = Convert.ToInt32(ledgerResult);
                                    System.Diagnostics.Debug.WriteLine($"Retrieved PaymodeLedgerID: {master.PaymodeLedgerID} for PaymodeID: {master.PaymodeID}");
                                }
                            }
                        }
                        catch (Exception ledgerEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error getting PaymodeLedgerID: {ledgerEx.Message}");
                        }
                    }

                    // Important: PaymodeID and Paymode must be passed correctly for update
                    cmd.Parameters.AddWithValue("@PaymodeID", master.PaymodeID);
                    cmd.Parameters.AddWithValue("@Paymode", master.Paymode ?? "");
                    cmd.Parameters.AddWithValue("@PaymodeLedgerID", master.PaymodeLedgerID);

                    // Debug information
                    System.Diagnostics.Debug.WriteLine($"Updating purchase return with PaymodeID={master.PaymodeID}, Paymode={master.Paymode}");

                    cmd.Parameters.AddWithValue("@SubTotal", master.SubTotal);
                    cmd.Parameters.AddWithValue("@GrandTotal", master.GrandTotal);
                    cmd.Parameters.AddWithValue("@UserName", master.UserName ?? "");
                    cmd.Parameters.AddWithValue("@UserID", master.UserID > 0 ? master.UserID : 1);
                    cmd.Parameters.AddWithValue("@_Operation", "UPDATE");

                    // Execute the command and capture the result
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        // Check if the update was successful
                        if (dt.Rows.Count > 0 && dt.Rows[0][0].ToString() == "SUCCESS")
                        {
                            return "Purchase return updated successfully";
                        }
                    }

                    return "No records were updated";
                }
            }
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine("SQL Error: " + sqlEx.Message);
                return "Database error: " + sqlEx.Message;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
                return "Error: " + ex.Message;
            }
            finally
            {
                // Ensure connection is closed
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        /// <summary>
        /// Reduces stock in PriceSettings table when a purchase return is processed.
        /// Stock is reduced because items are being returned to the vendor.
        /// </summary>
        /// <param name="itemId">The item ID</param>
        /// <param name="branchId">The branch ID</param>
        /// <param name="unitId">The unit ID</param>
        /// <param name="returnQty">The quantity being returned (to reduce from stock)</param>
        /// <param name="transaction">The SQL transaction to use</param>
        private void ReducePriceSettingsStock(long itemId, int branchId, int unitId, double returnQty, SqlTransaction transaction)
        {
            try
            {
                if (returnQty <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"ReducePriceSettingsStock: Skipping - returnQty is {returnQty}");
                    return;
                }

                // Update PriceSettings to reduce stock by the returned quantity
                string updateStockQuery = @"
                    UPDATE PriceSettings 
                    SET Stock = ISNULL(Stock, 0) - @ReturnQty,
                        StockValue = (ISNULL(Stock, 0) - @ReturnQty) * ISNULL(Cost, 0)
                    WHERE ItemId = @ItemId 
                    AND BranchId = @BranchId 
                    AND UnitId = @UnitId";

                using (SqlCommand updateCmd = new SqlCommand(updateStockQuery, (SqlConnection)DataConnection, transaction))
                {
                    updateCmd.Parameters.AddWithValue("@ReturnQty", returnQty);
                    updateCmd.Parameters.AddWithValue("@ItemId", itemId);
                    updateCmd.Parameters.AddWithValue("@BranchId", branchId);
                    updateCmd.Parameters.AddWithValue("@UnitId", unitId);

                    int rowsAffected = updateCmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"ReducePriceSettingsStock: Reduced stock by {returnQty} for ItemId={itemId}, BranchId={branchId}, UnitId={unitId}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"ReducePriceSettingsStock: No PriceSettings record found for ItemId={itemId}, BranchId={branchId}, UnitId={unitId}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ReducePriceSettingsStock: Error reducing stock - {ex.Message}");
                // Don't throw - this is a secondary update, continue with the transaction
            }
        }
    }
}
