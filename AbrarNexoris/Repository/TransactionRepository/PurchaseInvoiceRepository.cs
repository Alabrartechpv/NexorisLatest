using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelClass;
using ModelClass.TransactionModels;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Windows.Forms;
using Repository.MasterRepositry;


namespace Repository.TransactionRepository
{
    public class PurchaseInvoiceRepository : BaseRepostitory
    {
        private static bool _schemaEnsured = false;
        private static readonly object _schemaLock = new object();

        public PurchaseInvoiceRepository()
        {
            EnsurePMasterSchema();
        }

        public void EnsurePMasterSchema()
        {
            if (_schemaEnsured) return;

            lock (_schemaLock)
            {
                if (_schemaEnsured) return;

                try
                {
                    bool wasClosed = DataConnection.State == ConnectionState.Closed;
                    if (wasClosed) DataConnection.Open();

                    string sql = @"
                        IF EXISTS (
                            SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                            WHERE TABLE_NAME = 'PMaster' AND COLUMN_NAME = 'CurSymbol' AND IS_NULLABLE = 'NO'
                        )
                        BEGIN
                            DECLARE @ConstraintName NVARCHAR(200);
                            SELECT @ConstraintName = name FROM sys.default_constraints 
                            WHERE parent_object_id = OBJECT_ID('PMaster') 
                            AND parent_column_id = COLUMNPROPERTY(OBJECT_ID('PMaster'), 'CurSymbol', 'ColumnId');

                            IF @ConstraintName IS NOT NULL
                            BEGIN
                                EXEC('ALTER TABLE dbo.PMaster DROP CONSTRAINT [' + @ConstraintName + ']');
                            END

                            ALTER TABLE dbo.PMaster ALTER COLUMN CurSymbol NVARCHAR(50) NULL;
                        END

                        IF EXISTS (
                            SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PMaster'
                        )
                        AND NOT EXISTS (
                            SELECT 1 FROM sys.default_constraints 
                            WHERE parent_object_id = OBJECT_ID('PMaster') 
                            AND parent_column_id = COLUMNPROPERTY(OBJECT_ID('PMaster'), 'CurSymbol', 'ColumnId')
                        )
                        BEGIN
                            ALTER TABLE dbo.PMaster ADD CONSTRAINT DF_PMaster_CurSymbol DEFAULT('RM') FOR CurSymbol;
                        END
                    ";

                    using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    if (wasClosed && DataConnection.State == ConnectionState.Open)
                    {
                        DataConnection.Close();
                    }

                    _schemaEnsured = true;
                    System.Diagnostics.Debug.WriteLine("Successfully verified and updated PMaster CurSymbol schema constraint.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("EnsurePMasterSchema error: " + ex.Message);
                }
            }
        }

        public int PurcaseNo = 0;
        LedgerRepository objLedgerRepository = new LedgerRepository();
        public PurchaseStockUpdateOnPricesettings objPricesettingsStock = new PurchaseStockUpdateOnPricesettings();
        public int GeneratePurchaseNO()
        {
            int PurcaseNo = 0;
            bool wasConnectionClosed = DataConnection.State == ConnectionState.Closed;

            try
            {
                // Only open if connection was closed
                if (wasConnectionClosed)
                {
                    DataConnection.Open();
                }

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Purchase, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@_Operation", "GENERATEPURCHASENO");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            PurcaseNo = Convert.ToInt32(ds.Tables[0].Rows[0].ItemArray[0].ToString());
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating purchase number: {Ex.Message}");
            }
            finally
            {
                // Only close if we opened it
                if (wasConnectionClosed && DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
            return PurcaseNo;
        }

        private DynamicParameters GetPurchaseMasterParameters(PurchaseMaster ObjPurchaseMaster)
        {
            var p = new DynamicParameters();
            p.Add("@CompanyId", ObjPurchaseMaster.CompanyId);
            p.Add("@FinYearId", ObjPurchaseMaster.FinYearId);
            p.Add("@BranchId", ObjPurchaseMaster.BranchId);
            p.Add("@BranchName", ObjPurchaseMaster.BranchName ?? "");
            p.Add("@PurchaseNo", ObjPurchaseMaster.PurchaseNo);
            p.Add("@PurchaseDate", ObjPurchaseMaster.PurchaseDate != DateTime.MinValue ? ObjPurchaseMaster.PurchaseDate : DateTime.Now);
            p.Add("@InvoiceNo", ObjPurchaseMaster.InvoiceNo ?? "");
            p.Add("@InvoiceDate", ObjPurchaseMaster.InvoiceDate != DateTime.MinValue ? ObjPurchaseMaster.InvoiceDate : DateTime.Now);
            p.Add("@LedgerID", ObjPurchaseMaster.LedgerID);
            p.Add("@VendorName", ObjPurchaseMaster.VendorName ?? "");
            p.Add("@PaymodeID", ObjPurchaseMaster.PaymodeID);
            p.Add("@Paymode", ObjPurchaseMaster.Paymode ?? "Cash");
            p.Add("@PaymodeLedgerID", ObjPurchaseMaster.PaymodeLedgerID);
            p.Add("@CreditPeriod", ObjPurchaseMaster.CreditPeriod);
            p.Add("@SubTotal", ObjPurchaseMaster.SubTotal);
            p.Add("@SpDisPer", ObjPurchaseMaster.SpDisPer);
            p.Add("@SpDsiAmt", ObjPurchaseMaster.SpDsiAmt);
            p.Add("@BillDiscountPer", ObjPurchaseMaster.BillDiscountPer);
            p.Add("@BillDiscountAmt", ObjPurchaseMaster.BillDiscountAmt);
            p.Add("@TaxPer", ObjPurchaseMaster.TaxPer);
            p.Add("@TaxAmt", ObjPurchaseMaster.TaxAmt);
            p.Add("@Frieght", ObjPurchaseMaster.Frieght);
            p.Add("@ExpenseAmt", ObjPurchaseMaster.ExpenseAmt);
            p.Add("@OtherExpAmt", ObjPurchaseMaster.OtherExpAmt);
            p.Add("@GrandTotal", ObjPurchaseMaster.GrandTotal);
            p.Add("@CancelFlag", ObjPurchaseMaster.CancelFlag);
            p.Add("@UserID", ObjPurchaseMaster.UserID);
            p.Add("@UserName", ObjPurchaseMaster.UserName ?? "");
            p.Add("@TaxType", !string.IsNullOrWhiteSpace(ObjPurchaseMaster.TaxType) ? ObjPurchaseMaster.TaxType : "I");
            p.Add("@Remarks", ObjPurchaseMaster.Remarks ?? "");
            p.Add("@RoundOff", ObjPurchaseMaster.RoundOff);
            p.Add("@CessPer", ObjPurchaseMaster.CessPer);
            p.Add("@CessAmt", ObjPurchaseMaster.CessAmt);
            p.Add("@CalAfterTax", ObjPurchaseMaster.CalAfterTax);
            p.Add("@CurrencyID", ObjPurchaseMaster.CurrencyID > 0 ? ObjPurchaseMaster.CurrencyID : 1);
            p.Add("@CurSymbol", !string.IsNullOrWhiteSpace(ObjPurchaseMaster.CurSymbol) ? ObjPurchaseMaster.CurSymbol : "RM");
            p.Add("@SeriesID", ObjPurchaseMaster.SeriesID);
            p.Add("@VoucherID", ObjPurchaseMaster.VoucherID);
            p.Add("@IsSyncd", ObjPurchaseMaster.IsSyncd);
            p.Add("@Paid", ObjPurchaseMaster.Paid);
            p.Add("@Pid", ObjPurchaseMaster.Pid);
            p.Add("@POrderMasterId", ObjPurchaseMaster.POrderMasterId);
            p.Add("@BilledBy", ObjPurchaseMaster.BilledBy ?? "");
            p.Add("@TrnsType", !string.IsNullOrWhiteSpace(ObjPurchaseMaster.TrnsType) ? ObjPurchaseMaster.TrnsType : "Purchase");
            p.Add("@NetTotal", ObjPurchaseMaster.NetTotal);
            p.Add("@_Operation", ObjPurchaseMaster._Operation ?? "");
            return p;
        }

        public string SavePurchaseInvoice(PurchaseMaster ObjPurchaseMaster, PurchaseDetails objPurchaseDetails, DataGridView dgvItem)
        {
            string result = "";
            Voucher objVoucher = new Voucher();
            EnsurePMasterSchema();
            DataConnection.Open();
            var trans = DataConnection.BeginTransaction();
            try
            {
                if (string.IsNullOrWhiteSpace(ObjPurchaseMaster.CurSymbol))
                {
                    ObjPurchaseMaster.CurSymbol = "RM";
                }
                if (ObjPurchaseMaster.CurrencyID <= 0)
                {
                    ObjPurchaseMaster.CurrencyID = 1;
                }

                ObjPurchaseMaster._Operation = "GENERATEPURCHASENO";
                ObjPurchaseMaster.FinYearId = SessionContext.FinYearId;

                var pGenerate = GetPurchaseMasterParameters(ObjPurchaseMaster);
                List<PurchaseMaster> ObjPurchasNo = DataConnection.Query<PurchaseMaster>(STOREDPROCEDURE.POS_Purchase, pGenerate, trans, commandType: CommandType.StoredProcedure).ToList<PurchaseMaster>();
                if (ObjPurchasNo.Count > 0)
                {
                    foreach (PurchaseMaster ObjPurchasePurchasno in ObjPurchasNo)
                    {
                        ObjPurchaseMaster.PurchaseNo = ObjPurchasePurchasno.PurchaseNo;
                    }
                }

                objVoucher._Operation = "GENERATENUMBER";
                objVoucher.CompanyID = SessionContext.CompanyId;
                objVoucher.BranchID = SessionContext.BranchId;
                objVoucher.FinYearID = ObjPurchaseMaster.FinYearId;
                objVoucher.VoucherType = "Purchase";

                List<Voucher> VouchersList = DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, objVoucher, trans, commandType: CommandType.StoredProcedure).ToList<Voucher>();
                if (VouchersList.Count > 0)
                {
                    foreach (Voucher objVoch in VouchersList)
                    {
                        objVoucher.VoucherID = objVoch.VoucherID;
                        ObjPurchaseMaster.VoucherID = Convert.ToInt32(objVoch.VoucherID);
                    }
                }

                // Calculate and set total tax amount in PurchaseMaster
                float totalTaxAmountForMaster = CalculateTotalTaxAmount(dgvItem);
                ObjPurchaseMaster.TaxAmt = totalTaxAmountForMaster;

                ObjPurchaseMaster._Operation = "CREATE";
                var pCreate = GetPurchaseMasterParameters(ObjPurchaseMaster);
                List<PurchaseMaster> PurchaseMaster = DataConnection.Query<PurchaseMaster>(STOREDPROCEDURE.POS_Purchase, pCreate, trans, commandType: CommandType.StoredProcedure).ToList<PurchaseMaster>();

                if (dgvItem != null && dgvItem.Rows.Count > 0)
                {
                    objPurchaseDetails._Operation = "CREATE";
                    for (int i = 0; i < dgvItem.Rows.Count; i++)
                    {
                        try
                        {
                            if (dgvItem.Rows[i].Cells["ItemId"] == null ||
                                dgvItem.Rows[i].Cells["ItemId"].Value == null ||
                                string.IsNullOrEmpty(dgvItem.Rows[i].Cells["ItemId"].Value.ToString()))
                            {
                                continue;
                            }

                            objPurchaseDetails.CompanyId = SessionContext.CompanyId;
                            objPurchaseDetails.BranchID = SessionContext.BranchId;
                            objPurchaseDetails.FinYearId = ObjPurchaseMaster.FinYearId;
                            objPurchaseDetails.ItemID = Convert.ToInt32(dgvItem.Rows[i].Cells["ItemId"].Value.ToString());
                            objPurchaseDetails.Barcode = dgvItem.Rows[i].Cells["BarCode"].Value?.ToString() ?? "";
                            objPurchaseDetails.ItemName = dgvItem.Rows[i].Cells["Description"].Value?.ToString() ?? "";
                            objPurchaseDetails.UnitId = Convert.ToInt32(dgvItem.Rows[i].Cells["UnitId"].Value.ToString());
                            objPurchaseDetails.Unit = dgvItem.Rows[i].Cells["Unit"].Value?.ToString() ?? "";

                            float packing = 0;
                            float.TryParse(dgvItem.Rows[i].Cells["Packing"].Value?.ToString(), out packing);
                            objPurchaseDetails.Packing = packing;

                            float qty = 0;
                            float.TryParse(dgvItem.Rows[i].Cells["Qty"].Value?.ToString(), out qty);
                            objPurchaseDetails.Qty = qty;

                            // Get BaseCost from grid (tax-excluded cost)
                            float baseCost = 0;
                            float.TryParse(dgvItem.Rows[i].Cells["BaseCost"].Value?.ToString(), out baseCost);

                            float cost = 0;
                            float.TryParse(dgvItem.Rows[i].Cells["Cost"].Value?.ToString(), out cost);
                            objPurchaseDetails.Cost = cost;

                            float free = 0;
                            float.TryParse(dgvItem.Rows[i].Cells["Free"].Value?.ToString(), out free);
                            objPurchaseDetails.Free = free;

                            float taxPer = 0;
                            float.TryParse(dgvItem.Rows[i].Cells["TaxPer"].Value?.ToString(), out taxPer);
                            objPurchaseDetails.TaxPer = taxPer;

                            float taxAmt = 0;
                            float.TryParse(dgvItem.Rows[i].Cells["TaxAmt"].Value?.ToString(), out taxAmt);
                            objPurchaseDetails.TaxAmt = taxAmt;

                            string taxType = dgvItem.Rows[i].Cells["TaxType"].Value?.ToString() ?? "I";
                            objPurchaseDetails.TaxType = taxType;

                            objPurchaseDetails.PurchaseNo = ObjPurchaseMaster.PurchaseNo;
                            objPurchaseDetails.SlNo = i + 1;

                            // Store BaseCost using reflection if property exists in the model
                            try
                            {
                                var baseCostProperty = objPurchaseDetails.GetType().GetProperty("BaseCost");
                                if (baseCostProperty != null && baseCostProperty.CanWrite)
                                {
                                    baseCostProperty.SetValue(objPurchaseDetails, baseCost);
                                }
                            }
                            catch
                            {
                                // If property doesn't exist, continue without error
                            }

                            List<PurchaseDetails> ListPurchaseDetails = DataConnection.Query<PurchaseDetails>(STOREDPROCEDURE.POS_Purchase_Details, objPurchaseDetails, trans, commandType: CommandType.StoredProcedure).ToList<PurchaseDetails>();

                            // Only update stock quantities, not prices - prices should remain unchanged from item master
                            objPricesettingsStock._Operation = "CREATE";
                            objPricesettingsStock.CompanyId = SessionContext.CompanyId;
                            objPricesettingsStock.BranchID = SessionContext.BranchId;
                            objPricesettingsStock.FinYearId = ObjPurchaseMaster.FinYearId;
                            objPricesettingsStock.ItemID = Convert.ToInt32(dgvItem.Rows[i].Cells["ItemId"].Value.ToString());
                            objPricesettingsStock.UnitId = Convert.ToInt32(dgvItem.Rows[i].Cells["UnitId"].Value.ToString());

                            int gridQty = 0;
                            int.TryParse(dgvItem.Rows[i].Cells["Qty"].Value?.ToString(), out gridQty);
                            objPricesettingsStock.Qty = gridQty;

                            // When Free = Qty (free items case), only Qty counts for stock, not Free
                            // So if Free = Qty, set Free to 0 for stock calculation to avoid double counting
                            if (free >= 1 && Math.Abs(free - gridQty) < 0.01f)
                            {
                                objPricesettingsStock.Free = 0; // Don't add Free separately since it's already in Qty
                            }
                            else
                            {
                                objPricesettingsStock.Free = free;
                            }

                            int packingValue = 0;
                            int.TryParse(dgvItem.Rows[i].Cells["Packing"].Value?.ToString(), out packingValue);
                            objPricesettingsStock.Packing = packingValue;

                            objPricesettingsStock.OldQty = 10;

                            // Get existing item prices, markdown values, stock, and cost from database
                            var existingPrices = GetExistingItemPrices(objPricesettingsStock.ItemID, objPricesettingsStock.UnitId, trans);
                            objPricesettingsStock.RetailPrice = existingPrices.RetailPrice;
                            // Use WholeSalePrice from grid if available, otherwise use existing price
                            if (dgvItem.Rows[i].Cells["WholeSalePrice"] != null && dgvItem.Rows[i].Cells["WholeSalePrice"].Value != null && !string.IsNullOrEmpty(dgvItem.Rows[i].Cells["WholeSalePrice"].Value.ToString()))
                            {
                                float wholeSalePrice = 0;
                                if (float.TryParse(dgvItem.Rows[i].Cells["WholeSalePrice"].Value.ToString(), out wholeSalePrice))
                                {
                                    objPricesettingsStock.WholeSalePrice = wholeSalePrice;
                                }
                                else
                                {
                                    objPricesettingsStock.WholeSalePrice = existingPrices.WholeSalePrice;
                                }
                            }
                            else
                            {
                                objPricesettingsStock.WholeSalePrice = existingPrices.WholeSalePrice;
                            }
                            objPricesettingsStock.CreditPrice = existingPrices.CreditPrice;

                            // Preserve existing markdown values
                            objPricesettingsStock.MDRetailPrice = existingPrices.MDRetailPrice;
                            objPricesettingsStock.MDWalkinPrice = existingPrices.MDWalkinPrice;
                            objPricesettingsStock.MDCreditPrice = existingPrices.MDCreditPrice;
                            objPricesettingsStock.MDMrpPrice = existingPrices.MDMrpPrice;
                            objPricesettingsStock.MDCardPrice = existingPrices.MDCardPrice;
                            objPricesettingsStock.MDStaffPrice = existingPrices.MDStaffPrice;
                            objPricesettingsStock.MDMinPrice = existingPrices.MDMinPrice;

                            // Calculate weighted average cost based on existing stock and new purchase
                            float existingCost = (float)existingPrices.Cost;
                            float existingStock = (float)existingPrices.Stock;
                            float purchaseCost = baseCost > 0 ? baseCost : cost;
                            float totalPurchaseQty = gridQty + free;

                            float calculatedAvgCost = CalculateAverageCost(existingCost, existingStock, purchaseCost, totalPurchaseQty);
                            objPricesettingsStock.SingleItemCost = calculatedAvgCost;
                            System.Diagnostics.Debug.WriteLine($"CREATE Purchase - Calculated Weighted Average Cost={calculatedAvgCost} (OldCost={existingCost}, OldStock={existingStock}, PurchCost={purchaseCost}, PurchQty={totalPurchaseQty}) for ItemId={objPricesettingsStock.ItemID}, UnitId={objPricesettingsStock.UnitId}");

                            List<PurchaseStockUpdateOnPricesettings> UpdatePriceSettingsWithStock = DataConnection.Query<PurchaseStockUpdateOnPricesettings>(STOREDPROCEDURE.POS_PurchaseInvoice_PriceSettings, objPricesettingsStock, trans, commandType: CommandType.StoredProcedure).ToList<PurchaseStockUpdateOnPricesettings>();

                            UpdateItemMasterCostDirectly(objPricesettingsStock.ItemID, objPricesettingsStock.UnitId, calculatedAvgCost, packingValue, trans);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Error processing row " + i + ": " + ex.Message);
                            throw new Exception("Failed to save purchase item row " + (i + 1) + ". Transaction rolled back.", ex);
                        }
                    }
                }

                // Calculate total tax amount and subtotal (GrandTotal - TaxAmount)
                float totalTaxAmount = CalculateTotalTaxAmount(dgvItem);
                float subtotalAmount = (float)ObjPurchaseMaster.GrandTotal - totalTaxAmount;

                int targetBranchId = SessionContext.BranchId > 0 ? SessionContext.BranchId : (ObjPurchaseMaster.BranchId > 0 ? ObjPurchaseMaster.BranchId : Convert.ToInt32(DataBase.BranchId));
                int targetCompanyId = SessionContext.CompanyId > 0 ? SessionContext.CompanyId : (ObjPurchaseMaster.CompanyId > 0 ? ObjPurchaseMaster.CompanyId : Convert.ToInt32(DataBase.CompanyId));

                // Check Cash-In-Hand balance if payment mode is Cash
                if (IsCashPaymentMode(ObjPurchaseMaster.PaymodeID, ObjPurchaseMaster.Paymode))
                {
                    double currentCashBalance = GetAvailableCashBalance(targetBranchId, trans);
                    if (currentCashBalance - ObjPurchaseMaster.GrandTotal < 0)
                    {
                        throw new InvalidOperationException($"Insufficient CASH-IN-HAND balance. Current Cash Balance: {currentCashBalance:N2}, Purchase Amount: {ObjPurchaseMaster.GrandTotal:N2}. Transaction cannot cause a negative cash balance.");
                    }
                }

                objVoucher._Operation = "CREATE";
                objVoucher.CompanyID = targetCompanyId;
                objVoucher.BranchID = targetBranchId;
                objVoucher.FinYearID = ObjPurchaseMaster.FinYearId;
                if (IsCashPaymentMode(ObjPurchaseMaster.PaymodeID, ObjPurchaseMaster.Paymode))
                {
                    int cashLedgerId = objLedgerRepository.GetLedgerId(DefaultLedgers.CASH, (int)AccountGroup.CASH_IN_HAND, targetBranchId);
                    if (cashLedgerId <= 0)
                        cashLedgerId = objLedgerRepository.GetLedgerId("CASH", (int)AccountGroup.CASH_IN_HAND, targetBranchId);

                    objVoucher.CompanyID = targetCompanyId;
                    objVoucher.BranchID = targetBranchId;
                    objVoucher.VoucherID = objVoucher.VoucherID;
                    objVoucher.VoucherSeriesID = 0;
                    objVoucher.VoucherDate = DateTime.Now;
                    objVoucher.GroupID = Convert.ToInt32(AccountGroup.CASH_IN_HAND);
                    objVoucher.LedgerID = cashLedgerId;
                    objVoucher.LedgerName = DefaultLedgers.CASH;
                    objVoucher.VoucherType = "Purchase";
                    objVoucher.Debit = 0;
                    objVoucher.Credit = ObjPurchaseMaster.GrandTotal;
                    objVoucher.Narration = "PURCHASE: #" + Convert.ToString(ObjPurchaseMaster.PurchaseNo) + "| PURCHASE WORTH:" + Convert.ToString(ObjPurchaseMaster.GrandTotal) + "| REMARKS: " + ObjPurchaseMaster.Remarks;
                    objVoucher.SlNo = 1;
                    objVoucher.Mode = ObjPurchaseMaster.Paymode ?? "Cash";
                    objVoucher.ModeID = ObjPurchaseMaster.PaymodeID;
                    objVoucher.UserDate = DateTime.Now;
                    objVoucher.UserName = SessionContext.UserName;
                    objVoucher.UserID = SessionContext.UserId;
                    objVoucher.CancelFlag = false;
                    objVoucher.FinYearID = SessionContext.FinYearId;
                    objVoucher.CounterID = SessionContext.CounterId;
                    objVoucher.IsSyncd = false;
                    List<Voucher> ObjSaveCreditVocher = DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, objVoucher, trans, commandType: CommandType.StoredProcedure).ToList<Voucher>();

                    objVoucher.CompanyID = targetCompanyId;
                    objVoucher.BranchID = targetBranchId;
                    objVoucher.VoucherID = objVoucher.VoucherID;
                    objVoucher.VoucherSeriesID = 0;
                    objVoucher.VoucherDate = DateTime.Now;
                    objVoucher.GroupID = Convert.ToInt32(AccountGroup.PURCHASE_ACCOUNT);
                    objVoucher.LedgerID = objLedgerRepository.GetLedgerId(DefaultLedgers.PURCHASE, (int)AccountGroup.PURCHASE_ACCOUNT, targetBranchId);
                    objVoucher.LedgerName = DefaultLedgers.PURCHASE;
                    objVoucher.VoucherType = "Purchase";
                    objVoucher.Credit = 0;
                    objVoucher.Debit = subtotalAmount;
                    objVoucher.Narration = "PURCHASE: #" + Convert.ToString(ObjPurchaseMaster.PurchaseNo) + "| PURCHASE WORTH:" + Convert.ToString(ObjPurchaseMaster.GrandTotal) + "| REMARKS:" + ObjPurchaseMaster.Remarks;
                    objVoucher.SlNo = 2;
                    objVoucher.Mode = ObjPurchaseMaster.Paymode ?? "Cash";
                    objVoucher.ModeID = ObjPurchaseMaster.PaymodeID;
                    objVoucher.UserDate = DateTime.Now;
                    objVoucher.UserName = SessionContext.UserName;
                    objVoucher.UserID = SessionContext.UserId;
                    objVoucher.CancelFlag = false;
                    objVoucher.FinYearID = SessionContext.FinYearId;
                    objVoucher.CounterID = SessionContext.CounterId;
                    objVoucher.IsSyncd = false;
                    List<Voucher> ObjSaveDebitVoucher = DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, objVoucher, trans, commandType: CommandType.StoredProcedure).ToList<Voucher>();
                }
                else
                {
                    objVoucher.CompanyID = targetCompanyId;
                    objVoucher.BranchID = targetBranchId;
                    objVoucher.VoucherID = objVoucher.VoucherID;
                    objVoucher.VoucherSeriesID = 0;
                    objVoucher.VoucherDate = DateTime.Now;
                    objVoucher.GroupID = Convert.ToInt32(AccountGroup.SUNDRY_CREDITORS);
                    objVoucher.LedgerID = ObjPurchaseMaster.LedgerID;
                    objVoucher.LedgerName = ObjPurchaseMaster.VendorName;
                    objVoucher.VoucherType = "Purchase";
                    objVoucher.Debit = 0;
                    objVoucher.Credit = ObjPurchaseMaster.GrandTotal;
                    objVoucher.Narration = "PURCHASE: #" + Convert.ToString(ObjPurchaseMaster.PurchaseNo) + "| PURCHASE WORTH:" + Convert.ToString(ObjPurchaseMaster.GrandTotal) + "| REMARKS:" + ObjPurchaseMaster.Remarks;
                    objVoucher.SlNo = 1;
                    objVoucher.Mode = ObjPurchaseMaster.Paymode ?? "";
                    objVoucher.ModeID = ObjPurchaseMaster.PaymodeID;
                    objVoucher.UserDate = DateTime.Now;
                    objVoucher.UserName = SessionContext.UserName;
                    objVoucher.UserID = SessionContext.UserId;
                    objVoucher.CancelFlag = false;
                    objVoucher.FinYearID = SessionContext.FinYearId;
                    objVoucher.CounterID = SessionContext.CounterId;
                    objVoucher.IsSyncd = false;
                    List<Voucher> ObjSaveDebitVocherCredi = DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, objVoucher, trans, commandType: CommandType.StoredProcedure).ToList<Voucher>();

                    objVoucher.CompanyID = targetCompanyId;
                    objVoucher.BranchID = targetBranchId;
                    objVoucher.VoucherID = objVoucher.VoucherID;
                    objVoucher.VoucherSeriesID = 0;
                    objVoucher.VoucherDate = DateTime.Now;
                    objVoucher.GroupID = Convert.ToInt32(AccountGroup.PURCHASE_ACCOUNT);
                    objVoucher.LedgerID = objLedgerRepository.GetLedgerId(DefaultLedgers.PURCHASE, (int)AccountGroup.PURCHASE_ACCOUNT, targetBranchId);
                    objVoucher.LedgerName = DefaultLedgers.PURCHASE;
                    objVoucher.VoucherType = "Purchase";
                    objVoucher.Debit = subtotalAmount;
                    objVoucher.Credit = 0;
                    objVoucher.Narration = "PURCHASE: #" + Convert.ToString(ObjPurchaseMaster.PurchaseNo) + "| PURCHASE WORTH:" + Convert.ToString(ObjPurchaseMaster.GrandTotal) + "| REMARKS:" + ObjPurchaseMaster.Remarks;
                    objVoucher.SlNo = 2;
                    objVoucher.Mode = ObjPurchaseMaster.Paymode ?? "";
                    objVoucher.ModeID = ObjPurchaseMaster.PaymodeID;
                    objVoucher.UserDate = DateTime.Now;
                    objVoucher.UserName = SessionContext.UserName;
                    objVoucher.UserID = SessionContext.UserId;
                    objVoucher.CancelFlag = false;
                    objVoucher.FinYearID = SessionContext.FinYearId;
                    objVoucher.CounterID = SessionContext.CounterId;
                    objVoucher.IsSyncd = false;
                    List<Voucher> ObjSaveCreditVoucherCredit = DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, objVoucher, trans, commandType: CommandType.StoredProcedure).ToList<Voucher>();
                }

                // Create tax voucher entries for CGST and SGST
                Dictionary<float, float> taxAmountsByPercentage = AggregateTaxAmountsByPercentage(dgvItem);
                if (taxAmountsByPercentage.Count > 0)
                {
                    int nextSlNo = 3; // Start after main voucher entries (SlNo 1 and 2)
                    CreateTaxVoucherEntries(objVoucher, taxAmountsByPercentage, ObjPurchaseMaster.PurchaseNo, ObjPurchaseMaster.Remarks, trans, ref nextSlNo);
                }

                // ATOMIC SYNC QUEUE ENQUEUE (via Stored Procedure POS_SyncQueue)
                try
                {
                    if (ObjPurchaseMaster.TransactionGuid == Guid.Empty)
                    {
                        ObjPurchaseMaster.TransactionGuid = Guid.NewGuid();
                    }

                    DataConnection.Execute(
                        "UPDATE dbo.PMaster SET TransactionGuid = @TransactionGuid WHERE PurchaseNo = @PurchaseNo AND FinYearId = @FinYearId",
                        new { ObjPurchaseMaster.TransactionGuid, ObjPurchaseMaster.PurchaseNo, ObjPurchaseMaster.FinYearId },
                        transaction: trans);

                    DataConnection.Execute(
                        "UPDATE dbo.PDetails SET TransactionGuid = @TransactionGuid WHERE PurchaseNo = @PurchaseNo AND FinYearId = @FinYearId",
                        new { ObjPurchaseMaster.TransactionGuid, ObjPurchaseMaster.PurchaseNo, ObjPurchaseMaster.FinYearId },
                        transaction: trans);

                    SyncQueueRepository.EnqueueTransaction(
                        DataConnection,
                        trans,
                        targetBranchId,
                        "PURCHASE",
                        ObjPurchaseMaster.PurchaseNo.ToString(),
                        ObjPurchaseMaster.TransactionGuid,
                        "CREATE",
                        1
                    );
                }
                catch (Exception syncEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[PurchaseInvoiceRepository.SavePurchaseInvoice] SyncQueue enqueue warning: {syncEx.Message}");
                }

                trans.Commit();


            }
            catch (Exception ex)
            {
                trans.Rollback();
                result = "Error: " + ex.Message;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();

            }

            return result;
        }

        public string UpdatePurchase(PurchaseMaster ObjPmaster, PurchaseDetails objPdetails, DataGridView dgvPurchase)
        {
            string result = "";
            DataConnection.Open();
            Voucher ObjVoucher = new Voucher();
            var trans = DataConnection.BeginTransaction();
            try
            {
                if (string.IsNullOrWhiteSpace(ObjPmaster.CurSymbol))
                {
                    ObjPmaster.CurSymbol = "RM";
                }
                if (ObjPmaster.CurrencyID <= 0)
                {
                    ObjPmaster.CurrencyID = 1;
                }

                // Get the original FinYearId from the database to ensure consistency
                int originalFinYearId = ObjPmaster.FinYearId;

                // IMPORTANT: Get old purchase details BEFORE any updates/deletes happen
                // This is needed to reverse the old purchase's effect on stock and cost
                List<PurchaseDetails> oldPurchaseDetails = GetOldPurchaseDetails(ObjPmaster.PurchaseNo, originalFinYearId, trans);

                // Calculate and set total tax amount in PurchaseMaster
                float totalTaxAmount = CalculateTotalTaxAmount(dgvPurchase);
                ObjPmaster.TaxAmt = totalTaxAmount;

                ObjPmaster._Operation = "UPDATE";
                var pUpdate = GetPurchaseMasterParameters(ObjPmaster);
                List<PurchaseMaster> ObjUpdatePmaster = DataConnection.Query<PurchaseMaster>(STOREDPROCEDURE.POS_Purchase, pUpdate, trans, commandType: CommandType.StoredProcedure).ToList<PurchaseMaster>();
                ObjVoucher.BranchID = SessionContext.BranchId;
                ObjVoucher.VoucherID = ObjPmaster.VoucherID;
                ObjVoucher._Operation = "UPDATE";
                ObjVoucher.VoucherType = "Purchase";
                List<Voucher> ObjDeleteVocher = DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, ObjVoucher, trans, commandType: CommandType.StoredProcedure).ToList<Voucher>();

                if (dgvPurchase != null && dgvPurchase.Rows.Count > 0)
                {
                    objPdetails._Operation = "CREATE";
                    // Process all rows including the last one
                    for (int i = 0; i < dgvPurchase.Rows.Count; i++)
                    {
                        try
                        {
                            // Skip rows that don't have an ItemId cell or value
                            if (dgvPurchase.Rows[i].Cells["ItemId"] == null ||
                                dgvPurchase.Rows[i].Cells["ItemId"].Value == null ||
                                string.IsNullOrEmpty(dgvPurchase.Rows[i].Cells["ItemId"].Value.ToString()))
                            {
                                continue;
                            }

                            objPdetails.CompanyId = SessionContext.CompanyId;
                            objPdetails.BranchID = SessionContext.BranchId;
                            // Ensure we use the original FinYearId rather than default value
                            objPdetails.FinYearId = originalFinYearId;
                            objPdetails.ItemID = Convert.ToInt32(dgvPurchase.Rows[i].Cells["ItemId"].Value.ToString());
                            objPdetails.Barcode = dgvPurchase.Rows[i].Cells["BarCode"].Value?.ToString() ?? "";
                            objPdetails.ItemName = dgvPurchase.Rows[i].Cells["Description"].Value?.ToString() ?? "";
                            objPdetails.UnitId = Convert.ToInt32(dgvPurchase.Rows[i].Cells["UnitId"].Value.ToString());
                            objPdetails.Unit = dgvPurchase.Rows[i].Cells["Unit"].Value?.ToString() ?? "";

                            float packing = 0;
                            float.TryParse(dgvPurchase.Rows[i].Cells["Packing"].Value?.ToString(), out packing);
                            objPdetails.Packing = packing;

                            float qty = 0;
                            float.TryParse(dgvPurchase.Rows[i].Cells["Qty"].Value?.ToString(), out qty);
                            objPdetails.Qty = qty;

                            // Get BaseCost from grid (tax-excluded cost)
                            float baseCost = 0;
                            float.TryParse(dgvPurchase.Rows[i].Cells["BaseCost"].Value?.ToString(), out baseCost);

                            float cost = 0;
                            float.TryParse(dgvPurchase.Rows[i].Cells["Cost"].Value?.ToString(), out cost);
                            objPdetails.Cost = cost;

                            float free = 0;
                            float.TryParse(dgvPurchase.Rows[i].Cells["Free"].Value?.ToString(), out free);
                            objPdetails.Free = free;

                            float taxPer = 0;
                            float.TryParse(dgvPurchase.Rows[i].Cells["TaxPer"].Value?.ToString(), out taxPer);
                            objPdetails.TaxPer = taxPer;

                            float taxAmt = 0;
                            float.TryParse(dgvPurchase.Rows[i].Cells["TaxAmt"].Value?.ToString(), out taxAmt);
                            objPdetails.TaxAmt = taxAmt;

                            string taxType = dgvPurchase.Rows[i].Cells["TaxType"].Value?.ToString() ?? "I";
                            objPdetails.TaxType = taxType;

                            objPdetails.PurchaseNo = ObjPmaster.PurchaseNo;
                            objPdetails.SlNo = i + 1;

                            // Store BaseCost using reflection if property exists in the model
                            try
                            {
                                var baseCostProperty = objPdetails.GetType().GetProperty("BaseCost");
                                if (baseCostProperty != null && baseCostProperty.CanWrite)
                                {
                                    baseCostProperty.SetValue(objPdetails, baseCost);
                                }
                            }
                            catch
                            {
                                // If property doesn't exist, continue without error
                            }

                            List<PurchaseDetails> ListPurchaseDetails = DataConnection.Query<PurchaseDetails>(STOREDPROCEDURE.POS_Purchase_Details, objPdetails, trans, commandType: CommandType.StoredProcedure).ToList<PurchaseDetails>();

                            // Only update stock quantities, not prices - prices should remain unchanged from item master
                            objPricesettingsStock._Operation = "CREATE";
                            objPricesettingsStock.CompanyId = SessionContext.CompanyId;
                            objPricesettingsStock.BranchID = SessionContext.BranchId;
                            // Ensure we use the original FinYearId for price settings too
                            objPricesettingsStock.FinYearId = originalFinYearId;
                            objPricesettingsStock.ItemID = Convert.ToInt32(dgvPurchase.Rows[i].Cells["ItemId"].Value.ToString());
                            objPricesettingsStock.UnitId = Convert.ToInt32(dgvPurchase.Rows[i].Cells["UnitId"].Value.ToString());

                            int gridQty = 0;
                            int.TryParse(dgvPurchase.Rows[i].Cells["Qty"].Value?.ToString(), out gridQty);

                            // Find the old purchase detail for this item/unit combination BEFORE setting Qty
                            // so we can compute the delta for stock update
                            float oldPurchaseCost = 0;
                            float oldPurchaseQty = 0;
                            float oldPurchaseFree = 0;
                            var oldDetail = oldPurchaseDetails.FirstOrDefault(
                                pd => pd.ItemID == objPricesettingsStock.ItemID && pd.UnitId == objPricesettingsStock.UnitId);

                            if (oldDetail != null)
                            {
                                oldPurchaseCost = (float)oldDetail.Cost;
                                oldPurchaseQty = (float)oldDetail.Qty;
                                oldPurchaseFree = (float)oldDetail.Free;
                            }

                            objPricesettingsStock.Qty = gridQty;
                            objPricesettingsStock.Free = (int)free;

                            int packingValue = 0;
                            int.TryParse(dgvPurchase.Rows[i].Cells["Packing"].Value?.ToString(), out packingValue);
                            objPricesettingsStock.Packing = packingValue;

                            // Get existing item prices, markdown values, stock, and cost from database
                            var existingPrices = GetExistingItemPrices(objPricesettingsStock.ItemID, objPricesettingsStock.UnitId, trans);
                            objPricesettingsStock.RetailPrice = existingPrices.RetailPrice;
                            // Use WholeSalePrice from grid if available, otherwise use existing price
                            if (dgvPurchase.Rows[i].Cells["WholeSalePrice"] != null && dgvPurchase.Rows[i].Cells["WholeSalePrice"].Value != null && !string.IsNullOrEmpty(dgvPurchase.Rows[i].Cells["WholeSalePrice"].Value.ToString()))
                            {
                                float wholeSalePrice = 0;
                                if (float.TryParse(dgvPurchase.Rows[i].Cells["WholeSalePrice"].Value.ToString(), out wholeSalePrice))
                                {
                                    objPricesettingsStock.WholeSalePrice = wholeSalePrice;
                                }
                                else
                                {
                                    objPricesettingsStock.WholeSalePrice = existingPrices.WholeSalePrice;
                                }
                            }
                            else
                            {
                                objPricesettingsStock.WholeSalePrice = existingPrices.WholeSalePrice;
                            }
                            objPricesettingsStock.CreditPrice = existingPrices.CreditPrice;

                            // Preserve existing markdown values
                            objPricesettingsStock.MDRetailPrice = existingPrices.MDRetailPrice;
                            objPricesettingsStock.MDWalkinPrice = existingPrices.MDWalkinPrice;
                            objPricesettingsStock.MDCreditPrice = existingPrices.MDCreditPrice;
                            objPricesettingsStock.MDMrpPrice = existingPrices.MDMrpPrice;
                            objPricesettingsStock.MDCardPrice = existingPrices.MDCardPrice;
                            objPricesettingsStock.MDStaffPrice = existingPrices.MDStaffPrice;
                            objPricesettingsStock.MDMinPrice = existingPrices.MDMinPrice;

                            // === PURCHASE UPDATE: DELETE old stock, then CREATE new stock ===
                            // Step 1: If old purchase detail exists, call SP with DELETE to reverse old stock
                            if (oldDetail != null)
                            {
                                var deleteObj = new PurchaseStockUpdateOnPricesettings();
                                deleteObj._Operation = "DELETE";
                                deleteObj.CompanyId = objPricesettingsStock.CompanyId;
                                deleteObj.BranchID = objPricesettingsStock.BranchID;
                                deleteObj.FinYearId = objPricesettingsStock.FinYearId;
                                deleteObj.ItemID = objPricesettingsStock.ItemID;
                                deleteObj.UnitId = objPricesettingsStock.UnitId;
                                deleteObj.Qty = oldPurchaseQty;
                                deleteObj.Free = oldPurchaseFree;
                                deleteObj.OldQty = 0;
                                deleteObj.SingleItemCost = oldPurchaseCost;
                                deleteObj.Packing = oldDetail.Packing > 0 ? oldDetail.Packing : packingValue;
                                deleteObj.RetailPrice = existingPrices.RetailPrice;
                                deleteObj.WholeSalePrice = existingPrices.WholeSalePrice;
                                deleteObj.CreditPrice = existingPrices.CreditPrice;

                                DataConnection.Query<PurchaseStockUpdateOnPricesettings>(
                                    STOREDPROCEDURE.POS_PurchaseInvoice_PriceSettings,
                                    deleteObj, trans, commandType: CommandType.StoredProcedure).ToList();

                                System.Diagnostics.Debug.WriteLine($"UPDATE Purchase STEP1 DELETE - ItemId={objPricesettingsStock.ItemID}, OldQty={oldPurchaseQty}, OldFree={oldPurchaseFree}, OldCost={oldPurchaseCost}, Packing={packingValue}");
                            }

                            // Calculate weighted average cost for UPDATE operation
                            float currentCost = (float)existingPrices.Cost;
                            float currentStock = (float)existingPrices.Stock;
                            float newPurchaseCost = baseCost > 0 ? baseCost : cost;
                            float newPurchaseQty = gridQty + free;

                            float calculatedAvgCost = currentCost;
                            if (oldDetail != null)
                            {
                                calculatedAvgCost = CalculateAverageCostForUpdate(currentCost, currentStock, oldPurchaseCost, oldPurchaseQty + oldPurchaseFree, newPurchaseCost, newPurchaseQty);
                            }
                            else
                            {
                                calculatedAvgCost = CalculateAverageCost(currentCost, currentStock, newPurchaseCost, newPurchaseQty);
                            }

                            objPricesettingsStock.SingleItemCost = calculatedAvgCost;
                            System.Diagnostics.Debug.WriteLine($"UPDATE Purchase - Calculated Weighted Average Cost={calculatedAvgCost} for ItemId={objPricesettingsStock.ItemID}, UnitId={objPricesettingsStock.UnitId}");

                            // Step 2: Call SP with CREATE to add new stock
                            objPricesettingsStock._Operation = "CREATE";
                            List<PurchaseStockUpdateOnPricesettings> UpdatePriceSettingsWithStock = DataConnection.Query<PurchaseStockUpdateOnPricesettings>(STOREDPROCEDURE.POS_PurchaseInvoice_PriceSettings, objPricesettingsStock, trans, commandType: CommandType.StoredProcedure).ToList<PurchaseStockUpdateOnPricesettings>();

                            UpdateItemMasterCostDirectly(objPricesettingsStock.ItemID, objPricesettingsStock.UnitId, calculatedAvgCost, packingValue, trans);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Error processing row " + i + ": " + ex.Message);
                            throw new Exception("Failed to update purchase item row " + (i + 1) + ". Transaction rolled back.", ex);
                        }
                    }
                }

                // Step 3: Handle items that were removed from the purchase bill
                if (oldPurchaseDetails != null && oldPurchaseDetails.Count > 0)
                {
                    foreach (var oldDetail in oldPurchaseDetails)
                    {
                        bool stillPresent = false;
                        if (dgvPurchase != null)
                        {
                            for (int i = 0; i < dgvPurchase.Rows.Count; i++)
                            {
                                if (dgvPurchase.Rows[i].Cells["ItemId"] != null &&
                                    dgvPurchase.Rows[i].Cells["ItemId"].Value != null &&
                                    int.TryParse(dgvPurchase.Rows[i].Cells["ItemId"].Value.ToString(), out int gridItemId) &&
                                    gridItemId == oldDetail.ItemID)
                                {
                                    if (dgvPurchase.Rows[i].Cells["UnitId"] != null &&
                                        dgvPurchase.Rows[i].Cells["UnitId"].Value != null &&
                                        int.TryParse(dgvPurchase.Rows[i].Cells["UnitId"].Value.ToString(), out int gridUnitId) &&
                                        gridUnitId == oldDetail.UnitId)
                                    {
                                        stillPresent = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (!stillPresent)
                        {
                            // Item was removed from purchase bill: call SP with DELETE to reverse its stock
                            var existingPrices = GetExistingItemPrices(oldDetail.ItemID, oldDetail.UnitId, trans);
                            int packingValue = (int)oldDetail.Packing;
                            if (packingValue <= 0) packingValue = 1;

                            var deleteObj = new PurchaseStockUpdateOnPricesettings();
                            deleteObj._Operation = "DELETE";
                            deleteObj.CompanyId = SessionContext.CompanyId;
                            deleteObj.BranchID = SessionContext.BranchId;
                            deleteObj.FinYearId = originalFinYearId;
                            deleteObj.ItemID = oldDetail.ItemID;
                            deleteObj.UnitId = oldDetail.UnitId;
                            deleteObj.Qty = (float)oldDetail.Qty;
                            deleteObj.Free = (float)oldDetail.Free;
                            deleteObj.OldQty = 0;
                            deleteObj.SingleItemCost = (float)oldDetail.Cost;
                            deleteObj.Packing = packingValue;
                            deleteObj.RetailPrice = existingPrices.RetailPrice;
                            deleteObj.WholeSalePrice = existingPrices.WholeSalePrice;
                            deleteObj.CreditPrice = existingPrices.CreditPrice;

                            DataConnection.Query<PurchaseStockUpdateOnPricesettings>(
                                STOREDPROCEDURE.POS_PurchaseInvoice_PriceSettings,
                                deleteObj, trans, commandType: CommandType.StoredProcedure).ToList();

                            System.Diagnostics.Debug.WriteLine($"UPDATE Purchase REMOVED ITEM - ItemId={oldDetail.ItemID}, UnitId={oldDetail.UnitId}, OldQty={oldDetail.Qty}, OldFree={oldDetail.Free} reversed from PriceSettings.");
                        }
                    }
                }

                // Calculate subtotal (GrandTotal - TaxAmount) - reuse totalTaxAmount calculated above
                float subtotalAmount = (float)ObjPmaster.GrandTotal - totalTaxAmount;

                int targetBranchIdUpd = SessionContext.BranchId > 0 ? SessionContext.BranchId : (ObjPmaster.BranchId > 0 ? ObjPmaster.BranchId : Convert.ToInt32(DataBase.BranchId));
                int targetCompanyIdUpd = SessionContext.CompanyId > 0 ? SessionContext.CompanyId : (ObjPmaster.CompanyId > 0 ? ObjPmaster.CompanyId : Convert.ToInt32(DataBase.CompanyId));

                // Check Cash-In-Hand balance if payment mode is Cash
                if (IsCashPaymentMode(ObjPmaster.PaymodeID, ObjPmaster.Paymode))
                {
                    double currentCashBalance = GetAvailableCashBalance(targetBranchIdUpd, trans);
                    double oldCashCredit = GetOldCashVoucherCreditForPurchase(ObjPmaster.VoucherID, targetBranchIdUpd, trans);
                    double effectiveCashBalance = currentCashBalance + oldCashCredit;
                    if (effectiveCashBalance - ObjPmaster.GrandTotal < 0)
                    {
                        throw new InvalidOperationException($"Insufficient CASH-IN-HAND balance. Available Cash Balance (before update): {effectiveCashBalance:N2}, Updated Purchase Amount: {ObjPmaster.GrandTotal:N2}. Transaction cannot cause a negative cash balance.");
                    }
                }

                ObjVoucher._Operation = "CREATE";
                ObjVoucher.CompanyID = targetCompanyIdUpd;
                ObjVoucher.BranchID = targetBranchIdUpd;
                ObjVoucher.FinYearID = originalFinYearId;
                if (IsCashPaymentMode(ObjPmaster.PaymodeID, ObjPmaster.Paymode))
                {
                    int cashLedgerIdUpd = objLedgerRepository.GetLedgerId(DefaultLedgers.CASH, (int)AccountGroup.CASH_IN_HAND, targetBranchIdUpd);
                    if (cashLedgerIdUpd <= 0)
                        cashLedgerIdUpd = objLedgerRepository.GetLedgerId("CASH", (int)AccountGroup.CASH_IN_HAND, targetBranchIdUpd);

                    ObjVoucher.CompanyID = targetCompanyIdUpd;
                    ObjVoucher.BranchID = targetBranchIdUpd;
                    ObjVoucher.VoucherID = ObjVoucher.VoucherID;
                    ObjVoucher.VoucherSeriesID = 0;
                    ObjVoucher.VoucherDate = DateTime.Now;
                    ObjVoucher.GroupID = Convert.ToInt32(AccountGroup.CASH_IN_HAND);
                    ObjVoucher.LedgerID = cashLedgerIdUpd;
                    ObjVoucher.LedgerName = DefaultLedgers.CASH;
                    ObjVoucher.VoucherType = "Purchase";
                    ObjVoucher.Debit = 0;
                    ObjVoucher.Credit = ObjPmaster.GrandTotal;
                    ObjVoucher.Narration = "PURCHASE: #" + Convert.ToString(ObjPmaster.PurchaseNo) + "| PURCHASE WORTH:" + Convert.ToString(ObjPmaster.GrandTotal) + "| REMARKS: " + ObjPmaster.Remarks;
                    ObjVoucher.SlNo = 1;
                    ObjVoucher.Mode = ObjPmaster.Paymode ?? "Cash";
                    ObjVoucher.ModeID = ObjPmaster.PaymodeID;
                    ObjVoucher.UserDate = DateTime.Now;
                    ObjVoucher.UserName = SessionContext.UserName;
                    ObjVoucher.UserID = SessionContext.UserId;
                    ObjVoucher.CancelFlag = false;
                    ObjVoucher.FinYearID = originalFinYearId;
                    ObjVoucher.CounterID = SessionContext.CounterId;
                    ObjVoucher.IsSyncd = false;
                    List<Voucher> ObjSaveCreditVocher = DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, ObjVoucher, trans, commandType: CommandType.StoredProcedure).ToList<Voucher>();

                    ObjVoucher.CompanyID = targetCompanyIdUpd;
                    ObjVoucher.BranchID = targetBranchIdUpd;
                    ObjVoucher.VoucherID = ObjVoucher.VoucherID;
                    ObjVoucher.VoucherSeriesID = 0;
                    ObjVoucher.VoucherDate = DateTime.Now;
                    ObjVoucher.GroupID = Convert.ToInt32(AccountGroup.PURCHASE_ACCOUNT);
                    ObjVoucher.LedgerID = objLedgerRepository.GetLedgerId(DefaultLedgers.PURCHASE, (int)AccountGroup.PURCHASE_ACCOUNT, targetBranchIdUpd);
                    ObjVoucher.LedgerName = DefaultLedgers.PURCHASE;
                    ObjVoucher.VoucherType = "Purchase";
                    ObjVoucher.Credit = 0;
                    ObjVoucher.Debit = subtotalAmount;
                    ObjVoucher.Narration = "PURCHASE: #" + Convert.ToString(ObjPmaster.PurchaseNo) + "| PURCHASE WORTH:" + Convert.ToString(ObjPmaster.GrandTotal) + "| REMARKS:" + ObjPmaster.Remarks;
                    ObjVoucher.SlNo = 2;
                    ObjVoucher.Mode = ObjPmaster.Paymode ?? "Cash";
                    ObjVoucher.ModeID = ObjPmaster.PaymodeID;
                    ObjVoucher.UserDate = DateTime.Now;
                    ObjVoucher.UserName = SessionContext.UserName;
                    ObjVoucher.UserID = SessionContext.UserId;
                    ObjVoucher.CancelFlag = false;
                    ObjVoucher.FinYearID = originalFinYearId;
                    ObjVoucher.CounterID = SessionContext.CounterId;
                    ObjVoucher.IsSyncd = false;
                    List<Voucher> ObjSaveDebitVoucher = DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, ObjVoucher, trans, commandType: CommandType.StoredProcedure).ToList<Voucher>();
                }
                else
                {
                    ObjVoucher.CompanyID = targetCompanyIdUpd;
                    ObjVoucher.BranchID = targetBranchIdUpd;
                    ObjVoucher.VoucherID = ObjVoucher.VoucherID;
                    ObjVoucher.VoucherSeriesID = 0;
                    ObjVoucher.VoucherDate = DateTime.Now;
                    ObjVoucher.GroupID = Convert.ToInt32(AccountGroup.SUNDRY_CREDITORS);
                    ObjVoucher.LedgerID = ObjPmaster.LedgerID;
                    ObjVoucher.LedgerName = ObjPmaster.VendorName;
                    ObjVoucher.VoucherType = "Purchase";
                    ObjVoucher.Debit = 0;
                    ObjVoucher.Credit = ObjPmaster.GrandTotal;
                    ObjVoucher.Narration = "PURCHASE: #" + Convert.ToString(ObjPmaster.PurchaseNo) + "| PURCHASE WORTH:" + Convert.ToString(ObjPmaster.GrandTotal) + "| REMARKS:" + ObjPmaster.Remarks;
                    ObjVoucher.SlNo = 1;
                    ObjVoucher.Mode = ObjPmaster.Paymode ?? "";
                    ObjVoucher.ModeID = ObjPmaster.PaymodeID;
                    ObjVoucher.UserDate = DateTime.Now;
                    ObjVoucher.UserName = SessionContext.UserName;
                    ObjVoucher.UserID = SessionContext.UserId;
                    ObjVoucher.CancelFlag = false;
                    ObjVoucher.FinYearID = originalFinYearId;
                    ObjVoucher.CounterID = SessionContext.CounterId;
                    ObjVoucher.IsSyncd = false;
                    List<Voucher> ObjSaveDebitVocherCredi = DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, ObjVoucher, trans, commandType: CommandType.StoredProcedure).ToList<Voucher>();

                    ObjVoucher.CompanyID = targetCompanyIdUpd;
                    ObjVoucher.BranchID = targetBranchIdUpd;
                    ObjVoucher.VoucherID = ObjVoucher.VoucherID;
                    ObjVoucher.VoucherSeriesID = 0;
                    ObjVoucher.VoucherDate = DateTime.Now;
                    ObjVoucher.GroupID = Convert.ToInt32(AccountGroup.PURCHASE_ACCOUNT);
                    ObjVoucher.LedgerID = objLedgerRepository.GetLedgerId(DefaultLedgers.PURCHASE, (int)AccountGroup.PURCHASE_ACCOUNT, targetBranchIdUpd);
                    ObjVoucher.LedgerName = DefaultLedgers.PURCHASE;
                    ObjVoucher.VoucherType = "Purchase";
                    ObjVoucher.Debit = subtotalAmount;
                    ObjVoucher.Credit = 0;
                    ObjVoucher.Narration = "PURCHASE: #" + Convert.ToString(ObjPmaster.PurchaseNo) + "| PURCHASE WORTH:" + Convert.ToString(ObjPmaster.GrandTotal) + "| REMARKS:" + ObjPmaster.Remarks;
                    ObjVoucher.SlNo = 2;
                    ObjVoucher.Mode = ObjPmaster.Paymode ?? "";
                    ObjVoucher.ModeID = ObjPmaster.PaymodeID;
                    ObjVoucher.UserDate = DateTime.Now;
                    ObjVoucher.UserName = SessionContext.UserName;
                    ObjVoucher.UserID = SessionContext.UserId;
                    ObjVoucher.CancelFlag = false;
                    ObjVoucher.FinYearID = originalFinYearId;
                    ObjVoucher.CounterID = SessionContext.CounterId;
                    ObjVoucher.IsSyncd = false;
                    List<Voucher> ObjSaveCreditVoucherCredit = DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, ObjVoucher, trans, commandType: CommandType.StoredProcedure).ToList<Voucher>();
                }

                // Create tax voucher entries for CGST and SGST
                Dictionary<float, float> taxAmountsByPercentage = AggregateTaxAmountsByPercentage(dgvPurchase);
                if (taxAmountsByPercentage.Count > 0)
                {
                    int nextSlNo = 3; // Start after main voucher entries (SlNo 1 and 2)
                    CreateTaxVoucherEntries(ObjVoucher, taxAmountsByPercentage, ObjPmaster.PurchaseNo, ObjPmaster.Remarks, trans, ref nextSlNo);
                }

                // ATOMIC SYNC QUEUE ENQUEUE (UPDATE via Stored Procedure POS_SyncQueue)
                try
                {
                    int targetBranchId = SessionContext.BranchId > 0 ? SessionContext.BranchId : 1;
                    if (ObjPmaster.TransactionGuid == Guid.Empty)
                    {
                        ObjPmaster.TransactionGuid = DataConnection.QueryFirstOrDefault<Guid?>(
                            "SELECT TransactionGuid FROM dbo.PMaster WHERE PurchaseNo = @PurchaseNo AND BranchId = @BranchId AND FinYearId = @FinYearId",
                            new { ObjPmaster.PurchaseNo, BranchId = targetBranchId, FinYearId = originalFinYearId },
                            transaction: trans) 
                            ?? SyncQueueRepository.GetExistingGuid(DataConnection, trans, targetBranchId, "PURCHASE", ObjPmaster.PurchaseNo.ToString()) 
                            ?? Guid.NewGuid();
                    }

                    DataConnection.Execute(
                        "UPDATE dbo.PMaster SET TransactionGuid = @TransactionGuid WHERE PurchaseNo = @PurchaseNo AND FinYearId = @FinYearId",
                        new { ObjPmaster.TransactionGuid, ObjPmaster.PurchaseNo, FinYearId = originalFinYearId },
                        transaction: trans);

                    DataConnection.Execute(
                        "UPDATE dbo.PDetails SET TransactionGuid = @TransactionGuid WHERE PurchaseNo = @PurchaseNo AND FinYearId = @FinYearId",
                        new { ObjPmaster.TransactionGuid, ObjPmaster.PurchaseNo, FinYearId = originalFinYearId },
                        transaction: trans);

                    SyncQueueRepository.EnqueueTransaction(
                        DataConnection,
                        trans,
                        targetBranchId,
                        "PURCHASE",
                        ObjPmaster.PurchaseNo.ToString(),
                        ObjPmaster.TransactionGuid,
                        "UPDATE",
                        1
                    );
                }
                catch (Exception syncEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[PurchaseInvoiceRepository.UpdatePurchase] SyncQueue enqueue warning: {syncEx.Message}");
                }

                trans.Commit();
            }
            catch (Exception ex)
            {
                trans.Rollback();
                result = "Error: " + ex.Message;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return result;
        }

        public PurchaseInvoiceGrid getPurchaseNumber(int Pid)
        {
            PurchaseInvoiceGrid ObjPurchaseInvoiceGrid = new PurchaseInvoiceGrid();
            bool wasConnectionClosed = DataConnection.State == ConnectionState.Closed;

            try
            {
                // Only open if connection was closed
                if (wasConnectionClosed)
                {
                    DataConnection.Open();
                }

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Purchase, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", DataBase.FinyearId);
                    cmd.Parameters.AddWithValue("@Pid", Pid);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            ObjPurchaseInvoiceGrid.Listpmaster = ds.Tables[0].ToListOfObject<PurchaseMaster>();
                        }

                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[1] != null) && (ds.Tables[1].Rows.Count > 0))
                        {
                            ObjPurchaseInvoiceGrid.Listpdetails = ds.Tables[1].ToListOfObject<PurchaseDetails>();
                        }

                    }

                }

            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            finally
            {
                // Only close if we opened it
                if (wasConnectionClosed && DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }
            return ObjPurchaseInvoiceGrid;

        }

        public string DeletePurchaseInvoice(int purchaseNo, int branchId, int finYearId, int voucherId)
        {
            string result = "";
            bool wasConnectionClosed = DataConnection.State == ConnectionState.Closed;
            IDbTransaction trans = null;

            try
            {
                // Only open if connection was closed
                if (wasConnectionClosed)
                {
                    DataConnection.Open();
                }
                trans = DataConnection.BeginTransaction();

                // Reverse stock in PriceSettings for all items in this purchase invoice BEFORE deleting
                List<PurchaseDetails> oldPurchaseDetails = GetOldPurchaseDetails(purchaseNo, finYearId, trans);
                if (oldPurchaseDetails != null && oldPurchaseDetails.Count > 0)
                {
                    foreach (var oldDetail in oldPurchaseDetails)
                    {
                        var existingPrices = GetExistingItemPrices(oldDetail.ItemID, oldDetail.UnitId, trans);
                        int packingValue = (int)oldDetail.Packing;
                        if (packingValue <= 0) packingValue = 1;

                        var deleteObj = new PurchaseStockUpdateOnPricesettings();
                        deleteObj._Operation = "DELETE";
                        deleteObj.CompanyId = Convert.ToInt32(DataBase.CompanyId);
                        deleteObj.BranchID = branchId;
                        deleteObj.FinYearId = finYearId;
                        deleteObj.ItemID = oldDetail.ItemID;
                        deleteObj.UnitId = oldDetail.UnitId;
                        deleteObj.Qty = (float)oldDetail.Qty;
                        deleteObj.Free = (float)oldDetail.Free;
                        deleteObj.OldQty = 0;
                        deleteObj.SingleItemCost = (float)oldDetail.Cost;
                        deleteObj.Packing = packingValue;
                        deleteObj.RetailPrice = existingPrices.RetailPrice;
                        deleteObj.WholeSalePrice = existingPrices.WholeSalePrice;
                        deleteObj.CreditPrice = existingPrices.CreditPrice;

                        DataConnection.Query<PurchaseStockUpdateOnPricesettings>(
                            STOREDPROCEDURE.POS_PurchaseInvoice_PriceSettings,
                            deleteObj, trans, commandType: CommandType.StoredProcedure).ToList();

                        System.Diagnostics.Debug.WriteLine($"DELETE Purchase Invoice - Reversing stock for ItemId={oldDetail.ItemID}, UnitId={oldDetail.UnitId}, Qty={oldDetail.Qty}, Free={oldDetail.Free} from PriceSettings.");
                    }
                }

                // Create a parameter object with the DELETE operation
                var parameters = new
                {
                    CompanyId = Convert.ToInt32(DataBase.CompanyId),
                    BranchId = branchId,
                    FinYearId = finYearId,
                    PurchaseNo = purchaseNo,
                    VoucherID = voucherId,
                    TrnsType = "Purchase",  // Needed for the stored procedure
                    _Operation = "DELETE"
                };

                // Execute the stored procedure
                List<dynamic> results = DataConnection.Query<dynamic>(
                    STOREDPROCEDURE.POS_Purchase,
                    parameters,
                    trans,
                    commandType: CommandType.StoredProcedure).ToList();

                // Check if the operation was successful
                if (results != null && results.Count > 0)
                {
                    // The stored procedure should return "SUCCESS"
                    result = results[0].ToString();
                }

                // ATOMIC SYNC QUEUE ENQUEUE (CANCEL/DELETE via Stored Procedure POS_SyncQueue)
                try
                {
                    Guid deleteGuid = DataConnection.QueryFirstOrDefault<Guid?>(
                        "SELECT TransactionGuid FROM dbo.PMaster WHERE PurchaseNo = @PurchaseNo AND BranchId = @BranchId AND FinYearId = @FinYearId",
                        new { PurchaseNo = purchaseNo, BranchId = branchId, FinYearId = finYearId },
                        transaction: trans)
                        ?? SyncQueueRepository.GetExistingGuid(DataConnection, trans, branchId, "PURCHASE", purchaseNo.ToString()) 
                        ?? Guid.NewGuid();
                    SyncQueueRepository.EnqueueTransaction(
                        DataConnection,
                        trans,
                        branchId,
                        "PURCHASE",
                        purchaseNo.ToString(),
                        deleteGuid,
                        "CANCEL",
                        1
                    );
                }
                catch (Exception syncEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[PurchaseInvoiceRepository.DeletePurchaseInvoice] SyncQueue enqueue warning: {syncEx.Message}");
                }

                // Commit the transaction
                trans.Commit();
            }
            catch (Exception ex)
            {
                // Rollback transaction in case of error
                if (trans != null)
                {
                    trans.Rollback();
                }
                result = "Error: " + ex.Message;
            }
            finally
            {
                // Only close if we opened it
                if (wasConnectionClosed && DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return result;
        }

        /// <summary>
        /// Gets existing item prices, cost, stock, and markdown values from the database
        /// </summary>
        private dynamic GetExistingItemPrices(int itemId, int unitId, IDbTransaction transaction)
        {
            try
            {
                var query = @"
                    SELECT 
                        ISNULL(Cost, 0) as Cost,
                        ISNULL(Stock, 0) as Stock,
                        ISNULL(RetailPrice, 0) as RetailPrice,
                        ISNULL(WholeSalePrice, 0) as WholeSalePrice,
                        ISNULL(CreditPrice, 0) as CreditPrice,
                        ISNULL(MDRetailPrice, 0) as MDRetailPrice,
                        ISNULL(MDWalkinPrice, 0) as MDWalkinPrice,
                        ISNULL(MDCreditPrice, 0) as MDCreditPrice,
                        ISNULL(MDMrpPrice, 0) as MDMrpPrice,
                        ISNULL(MDCardPrice, 0) as MDCardPrice,
                        ISNULL(MDStaffPrice, 0) as MDStaffPrice,
                        ISNULL(MDMinPrice, 0) as MDMinPrice
                    FROM PriceSettings 
                    WHERE BranchId = @BranchId 
                        AND ItemId = @ItemId 
                        AND UnitId = @UnitId";

                var result = DataConnection.QueryFirstOrDefault(query, new
                {
                    BranchId = Convert.ToInt32(DataBase.BranchId),
                    ItemId = itemId,
                    UnitId = unitId
                }, transaction);

                // Return default values if no record found
                if (result == null)
                {
                    return new
                    {
                        Cost = 0.0,
                        Stock = 0.0,
                        RetailPrice = 0.0,
                        WholeSalePrice = 0.0,
                        CreditPrice = 0.0,
                        MDRetailPrice = 0.0,
                        MDWalkinPrice = 0.0,
                        MDCreditPrice = 0.0,
                        MDMrpPrice = 0.0,
                        MDCardPrice = 0.0,
                        MDStaffPrice = 0.0,
                        MDMinPrice = 0.0
                    };
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting existing item prices: {ex.Message}");
                // Return default values on error
                return new
                {
                    Cost = 0.0,
                    Stock = 0.0,
                    RetailPrice = 0.0,
                    WholeSalePrice = 0.0,
                    CreditPrice = 0.0,
                    MDRetailPrice = 0.0,
                    MDWalkinPrice = 0.0,
                    MDCreditPrice = 0.0,
                    MDMrpPrice = 0.0,
                    MDCardPrice = 0.0,
                    MDStaffPrice = 0.0,
                    MDMinPrice = 0.0
                };
            }
        }

        /// <summary>
        /// Calculates average cost based on existing stock and new purchase
        /// Formula: AvgCost = ((ExistingCost × ExistingStock) + (PurchaseCost × PurchaseQty)) / (ExistingStock + PurchaseQty)
        /// </summary>
        private float CalculateAverageCost(float existingCost, float existingStock, float purchaseCost, float purchaseQty)
        {
            try
            {
                // If there's no existing stock, just use the purchase cost
                if (existingStock <= 0)
                {
                    return purchaseCost;
                }

                // If no new purchase quantity, keep existing cost
                if (purchaseQty <= 0)
                {
                    return existingCost;
                }

                // Calculate weighted average cost
                float totalValue = (existingCost * existingStock) + (purchaseCost * purchaseQty);
                float totalQty = existingStock + purchaseQty;

                // Prevent division by zero
                if (totalQty <= 0)
                {
                    return purchaseCost;
                }

                float avgCost = totalValue / totalQty;

                return avgCost;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating average cost: {ex.Message}");
                // Return existing cost if calculation fails
                return existingCost;
            }
        }

        /// <summary>
        /// Gets old purchase details for a specific purchase number to reverse stock/cost effects
        /// </summary>
        private List<PurchaseDetails> GetOldPurchaseDetails(int purchaseNo, int finYearId, IDbTransaction transaction)
        {
            try
            {
                var query = @"
                    SELECT 
                        ItemID,
                        UnitId,
                        ISNULL(Qty, 0) as Qty,
                        ISNULL(Cost, 0) as Cost,
                        ISNULL(Free, 0) as Free,
                        ISNULL(Packing, 1) as Packing
                    FROM PDetails
                    WHERE PurchaseNo = @PurchaseNo
                        AND FinYearId = @FinYearId
                        AND BranchID = @BranchId
                        AND CompanyId = @CompanyId";

                var results = DataConnection.Query<PurchaseDetails>(query, new
                {
                    PurchaseNo = purchaseNo,
                    FinYearId = finYearId,
                    BranchId = Convert.ToInt32(DataBase.BranchId),
                    CompanyId = Convert.ToInt32(DataBase.CompanyId)
                }, transaction).ToList();

                return results ?? new List<PurchaseDetails>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting old purchase details: {ex.Message}");
                return new List<PurchaseDetails>();
            }
        }

        /// <summary>
        /// Updates ItemMaster cost directly in PriceSettings table
        /// This ensures our calculated average cost is saved even if stored procedure recalculates
        /// </summary>
        private void UpdateItemMasterCostDirectly(int itemId, int unitId, float calculatedCost, int packing, IDbTransaction transaction)
        {
            try
            {
                int packingVal = packing > 0 ? packing : 1;
                float baseUnitCost = calculatedCost / packingVal;

                // Update base unit and all packing units proportional to their packing factor
                var updateQuery = @"
                    UPDATE PriceSettings 
                    SET Cost = CASE 
                        WHEN ISNULL(Packing, 1) > 0 THEN @BaseUnitCost * ISNULL(Packing, 1)
                        ELSE @BaseUnitCost 
                    END
                    WHERE BranchId = @BranchId 
                        AND ItemId = @ItemId";

                DataConnection.Execute(updateQuery, new
                {
                    BaseUnitCost = baseUnitCost,
                    BranchId = Convert.ToInt32(DataBase.BranchId),
                    ItemId = itemId
                }, transaction);

                System.Diagnostics.Debug.WriteLine($"Updated ItemMaster cost for ItemId={itemId} (BaseUnitCost={baseUnitCost}) directly in PriceSettings.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating ItemMaster cost directly: {ex.Message}");
                // Don't throw - this is a secondary update, stored procedure might have already updated it
            }
        }



        /// <summary>
        /// Calculates average cost for UPDATE operation by reversing old purchase effect first
        /// Formula for reverse: OldStock = CurrentStock - OldPurchaseQty
        /// Then: AvgCost = ((OldCost × OldStock) + (NewPurchaseCost × NewPurchaseQty)) / (OldStock + NewPurchaseQty)
        /// </summary>
        private float CalculateAverageCostForUpdate(float currentCost, float currentStock, float oldPurchaseCost, float oldPurchaseQty, float newPurchaseCost, float newPurchaseQty)
        {
            try
            {
                // Reverse the old purchase effect
                // Get the stock before the old purchase was made
                float stockBeforeOldPurchase = currentStock - oldPurchaseQty;

                // If stock would be negative after reversal, assume it was 0
                if (stockBeforeOldPurchase < 0)
                {
                    stockBeforeOldPurchase = 0;
                }

                // Calculate the cost before the old purchase
                // If stock before was 0, we don't have a previous cost, so we'll use 0
                float costBeforeOldPurchase = 0;
                if (stockBeforeOldPurchase > 0)
                {
                    // Reverse the weighted average calculation
                    // CurrentStock × CurrentCost = (OldStock × OldCost) + (OldPurchaseQty × OldPurchaseCost)
                    // So: (OldStock × OldCost) = (CurrentStock × CurrentCost) - (OldPurchaseQty × OldPurchaseCost)
                    // Therefore: OldCost = ((CurrentStock × CurrentCost) - (OldPurchaseQty × OldPurchaseCost)) / OldStock
                    float totalCurrentValue = currentCost * currentStock;
                    float oldPurchaseValue = oldPurchaseCost * oldPurchaseQty;
                    float oldTotalValue = totalCurrentValue - oldPurchaseValue;

                    if (oldTotalValue >= 0 && stockBeforeOldPurchase > 0)
                    {
                        costBeforeOldPurchase = oldTotalValue / stockBeforeOldPurchase;
                    }
                }

                // Now calculate the new average cost with the reversed values
                // This is the same as a fresh purchase calculation
                return CalculateAverageCost(costBeforeOldPurchase, stockBeforeOldPurchase, newPurchaseCost, newPurchaseQty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating average cost for update: {ex.Message}");
                // Fallback to simple calculation if reverse fails
                return CalculateAverageCost(currentCost, currentStock, newPurchaseCost, newPurchaseQty);
            }
        }

        /// <summary>
        /// Gets the ledger name for INPUT CGST or INPUT SGST based on tax percentage
        /// Tax percentage is split equally between CGST and SGST
        /// </summary>
        private string GetTaxLedgerName(float taxPercentage, bool isCGST)
        {
            // Calculate half of tax percentage for CGST/SGST
            float halfTaxPer = taxPercentage / 2.0f;

            // Round to 1 decimal place to match ledger naming convention
            halfTaxPer = (float)Math.Round(halfTaxPer, 1);

            // Format the ledger name: "INPUT CGST X%" or "INPUT SGST X%"
            string ledgerName = isCGST ? $"INPUT CGST {halfTaxPer}%" : $"INPUT SGST {halfTaxPer}%";

            return ledgerName;
        }

        /// <summary>
        /// Calculates the total tax amount from all purchase items
        /// </summary>
        private float CalculateTotalTaxAmount(DataGridView dgvItem)
        {
            float totalTaxAmount = 0;

            if (dgvItem == null || dgvItem.Rows.Count == 0)
                return totalTaxAmount;

            for (int i = 0; i < dgvItem.Rows.Count; i++)
            {
                try
                {
                    // Skip rows without ItemId
                    if (dgvItem.Rows[i].Cells["ItemId"] == null ||
                        dgvItem.Rows[i].Cells["ItemId"].Value == null ||
                        string.IsNullOrEmpty(dgvItem.Rows[i].Cells["ItemId"].Value.ToString()))
                    {
                        continue;
                    }

                    // Get tax amount
                    float taxAmt = 0;
                    if (dgvItem.Rows[i].Cells["TaxAmt"] != null && dgvItem.Rows[i].Cells["TaxAmt"].Value != null)
                    {
                        float.TryParse(dgvItem.Rows[i].Cells["TaxAmt"].Value.ToString(), out taxAmt);
                    }

                    totalTaxAmount += taxAmt;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calculating total tax amount for row {i}: {ex.Message}");
                    continue;
                }
            }

            return totalTaxAmount;
        }

        /// <summary>
        /// Aggregates tax amounts by tax percentage from purchase items in DataGridView
        /// Returns a dictionary where key is tax percentage and value is total tax amount
        /// </summary>
        private Dictionary<float, float> AggregateTaxAmountsByPercentage(DataGridView dgvItem)
        {
            Dictionary<float, float> taxAmountsByPercentage = new Dictionary<float, float>();

            if (dgvItem == null || dgvItem.Rows.Count == 0)
                return taxAmountsByPercentage;

            for (int i = 0; i < dgvItem.Rows.Count; i++)
            {
                try
                {
                    // Skip rows without ItemId
                    if (dgvItem.Rows[i].Cells["ItemId"] == null ||
                        dgvItem.Rows[i].Cells["ItemId"].Value == null ||
                        string.IsNullOrEmpty(dgvItem.Rows[i].Cells["ItemId"].Value.ToString()))
                    {
                        continue;
                    }

                    // Get tax percentage and tax amount
                    float taxPer = 0;
                    if (dgvItem.Rows[i].Cells["TaxPer"] != null && dgvItem.Rows[i].Cells["TaxPer"].Value != null)
                    {
                        float.TryParse(dgvItem.Rows[i].Cells["TaxPer"].Value.ToString(), out taxPer);
                    }

                    float taxAmt = 0;
                    if (dgvItem.Rows[i].Cells["TaxAmt"] != null && dgvItem.Rows[i].Cells["TaxAmt"].Value != null)
                    {
                        float.TryParse(dgvItem.Rows[i].Cells["TaxAmt"].Value.ToString(), out taxAmt);
                    }

                    // Only process if tax percentage and tax amount are greater than 0
                    if (taxPer > 0 && taxAmt > 0)
                    {
                        // Round tax percentage to 1 decimal place for grouping
                        float roundedTaxPer = (float)Math.Round(taxPer, 1);

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
        /// </summary>
        private void CreateTaxVoucherEntries(Voucher objVoucher, Dictionary<float, float> taxAmountsByPercentage, int purchaseNo, string remarks, IDbTransaction trans, ref int slNo)
        {
            try
            {
                // Use the proper AccountGroup enum value for DUTIES & TAXES
                int dutiesAndTaxesGroupId = (int)AccountGroup.DUTIES_AND_TAXES;

                foreach (var taxEntry in taxAmountsByPercentage)
                {
                    float taxPercentage = taxEntry.Key;
                    float totalTaxAmount = taxEntry.Value;

                    // Split tax amount equally between CGST and SGST
                    float cgstAmount = totalTaxAmount / 2.0f;
                    float sgstAmount = totalTaxAmount / 2.0f;

                    // Get ledger names for CGST and SGST
                    string cgstLedgerName = GetTaxLedgerName(taxPercentage, true);
                    string sgstLedgerName = GetTaxLedgerName(taxPercentage, false);

                    // Get ledger IDs
                    int cgstLedgerId = objLedgerRepository.GetLedgerId(cgstLedgerName, dutiesAndTaxesGroupId, Convert.ToInt32(DataBase.BranchId));
                    int sgstLedgerId = objLedgerRepository.GetLedgerId(sgstLedgerName, dutiesAndTaxesGroupId, Convert.ToInt32(DataBase.BranchId));

                    // Create CGST voucher entry (Debit for purchase - input tax)
                    if (cgstLedgerId > 0 && cgstAmount > 0)
                    {
                        objVoucher.CompanyID = Convert.ToInt32(DataBase.CompanyId);
                        objVoucher.BranchID = Convert.ToInt32(DataBase.BranchId);
                        objVoucher.VoucherID = objVoucher.VoucherID;
                        objVoucher.VoucherSeriesID = 0;
                        objVoucher.VoucherDate = DateTime.Now;
                        objVoucher.GroupID = dutiesAndTaxesGroupId;
                        objVoucher.LedgerID = cgstLedgerId;
                        objVoucher.LedgerName = cgstLedgerName;
                        objVoucher.VoucherType = "Purchase";
                        objVoucher.Debit = cgstAmount;
                        objVoucher.Credit = 0;
                        objVoucher.Narration = "PURCHASE: #" + Convert.ToString(purchaseNo) + "| TAX: " + cgstLedgerName + "| REMARKS: " + remarks;
                        objVoucher.SlNo = slNo++;
                        objVoucher.Mode = "";
                        objVoucher.ModeID = 0;
                        objVoucher.UserDate = DateTime.Now;
                        objVoucher.UserName = DataBase.UserName;
                        objVoucher.UserID = Convert.ToInt32(DataBase.UserId);
                        objVoucher.CancelFlag = false;
                        objVoucher.FinYearID = objVoucher.FinYearID;
                        objVoucher.IsSyncd = false;
                        objVoucher._Operation = "CREATE";

                        List<Voucher> ObjSaveCGSTVoucher = DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, objVoucher, trans, commandType: CommandType.StoredProcedure).ToList<Voucher>();
                    }

                    // Create SGST voucher entry (Debit for purchase - input tax)
                    if (sgstLedgerId > 0 && sgstAmount > 0)
                    {
                        objVoucher.CompanyID = Convert.ToInt32(DataBase.CompanyId);
                        objVoucher.BranchID = Convert.ToInt32(DataBase.BranchId);
                        objVoucher.VoucherID = objVoucher.VoucherID;
                        objVoucher.VoucherSeriesID = 0;
                        objVoucher.VoucherDate = DateTime.Now;
                        objVoucher.GroupID = dutiesAndTaxesGroupId;
                        objVoucher.LedgerID = sgstLedgerId;
                        objVoucher.LedgerName = sgstLedgerName;
                        objVoucher.VoucherType = "Purchase";
                        objVoucher.Debit = sgstAmount;
                        objVoucher.Credit = 0;
                        objVoucher.Narration = "PURCHASE: #" + Convert.ToString(purchaseNo) + "| TAX: " + sgstLedgerName + "| REMARKS: " + remarks;
                        objVoucher.SlNo = slNo++;
                        objVoucher.Mode = "";
                        objVoucher.ModeID = 0;
                        objVoucher.UserDate = DateTime.Now;
                        objVoucher.UserName = DataBase.UserName;
                        objVoucher.UserID = Convert.ToInt32(DataBase.UserId);
                        objVoucher.CancelFlag = false;
                        objVoucher.FinYearID = objVoucher.FinYearID;
                        objVoucher.CounterID = SessionContext.CounterId;
                        objVoucher.IsSyncd = false;
                        objVoucher._Operation = "CREATE";

                        List<Voucher> ObjSaveSGSTVoucher = DataConnection.Query<Voucher>(STOREDPROCEDURE.POS_Vouchers, objVoucher, trans, commandType: CommandType.StoredProcedure).ToList<Voucher>();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating tax voucher entries: {ex.Message}");
                throw; // Re-throw to ensure transaction rollback
            }
        }

        /// <summary>Returns true if the given payment mode represents a Cash payment.</summary>
        public bool IsCashPaymentMode(int paymodeId, string paymodeName)
        {
            if (!string.IsNullOrWhiteSpace(paymodeName))
            {
                string pName = paymodeName.Trim();
                if (pName.Equals("Cash", StringComparison.OrdinalIgnoreCase) ||
                    pName.Equals("CASH-IN-HAND", StringComparison.OrdinalIgnoreCase) ||
                    pName.Equals("CASH IN HAND", StringComparison.OrdinalIgnoreCase))
                    return true;

                if (pName.Equals("Credit", StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            try
            {
                Dropdowns dropdowns = new Dropdowns();
                PaymodeDDlGrid paymodes = dropdowns.PaymodeDDl();
                PaymodeDDl paymode = paymodes.List?.FirstOrDefault(p => p.PayModeID == paymodeId);
                return paymode != null &&
                       !string.IsNullOrWhiteSpace(paymode.PayModeName) &&
                       paymode.PayModeName.Trim().Equals("Cash", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IsCashPaymentMode lookup failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>Returns the current CASH-IN-HAND balance (Debit - Credit) for the given branch.</summary>
        public double GetAvailableCashBalance(int branchId, IDbTransaction trans = null)
        {
            try
            {
                int cashLedgerId = objLedgerRepository.GetLedgerId(DefaultLedgers.CASH, (int)AccountGroup.CASH_IN_HAND, branchId);
                if (cashLedgerId <= 0)
                    cashLedgerId = objLedgerRepository.GetLedgerId("CASH", (int)AccountGroup.CASH_IN_HAND, branchId);

                string query = @"
                    SELECT ISNULL(SUM(ISNULL(Debit,0)) - SUM(ISNULL(Credit,0)), 0)
                    FROM Vouchers
                    WHERE BranchID = @BranchId
                      AND ISNULL(CancelFlag,0) = 0
                      AND (GroupID = @GroupId OR LedgerID = @LedgerId
                           OR LedgerName = @LedgerName OR LedgerName = 'CASH')";

                var p = new { BranchId = branchId, GroupId = (int)AccountGroup.CASH_IN_HAND,
                              LedgerId = cashLedgerId, LedgerName = DefaultLedgers.CASH };

                if (trans != null)
                    return DataConnection.ExecuteScalar<double>(query, p, trans);

                bool wasClosed = DataConnection.State == ConnectionState.Closed;
                try
                {
                    if (wasClosed) DataConnection.Open();
                    return DataConnection.ExecuteScalar<double>(query, p);
                }
                finally
                {
                    if (wasClosed && DataConnection.State == ConnectionState.Open) DataConnection.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAvailableCashBalance error: {ex.Message}");
                return 0;
            }
        }

        /// <summary>Returns the cash credit amount recorded for a specific Purchase voucher (used during update to offset existing credit).</summary>
        public double GetOldCashVoucherCreditForPurchase(long voucherId, int branchId, IDbTransaction trans = null)
        {
            if (voucherId <= 0) return 0;
            try
            {
                string query = @"
                    SELECT ISNULL(SUM(ISNULL(Credit,0)), 0)
                    FROM Vouchers
                    WHERE VoucherID = @VoucherId
                      AND BranchID = @BranchId
                      AND VoucherType = 'Purchase'
                      AND ISNULL(CancelFlag,0) = 0
                      AND (GroupID = @GroupId OR LedgerName = 'CASH-IN-HAND' OR LedgerName = 'CASH')";

                var p = new { VoucherId = voucherId, BranchId = branchId, GroupId = (int)AccountGroup.CASH_IN_HAND };

                if (trans != null)
                    return DataConnection.ExecuteScalar<double>(query, p, trans);

                bool wasClosed = DataConnection.State == ConnectionState.Closed;
                try
                {
                    if (wasClosed) DataConnection.Open();
                    return DataConnection.ExecuteScalar<double>(query, p);
                }
                finally
                {
                    if (wasClosed && DataConnection.State == ConnectionState.Open) DataConnection.Close();
                }
            }
            catch
            {
                return 0;
            }
        }

    }
}
