using ModelClass.TransactionModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Repository.TransactionRepository
{
    public class PurchaseOrderRepository : BaseRepostitory
    {
        public int GeneratePurchaseOrderNo(int finYearId, int branchId)
        {
            int purchaseOrderNo = 0;
            bool wasClosed = DataConnection.State != ConnectionState.Open;

            try
            {
                if (wasClosed)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_PurchaseOrder, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@_Operation", "GENERATENUMBER");

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        purchaseOrderNo = Convert.ToInt32(result);
                    }
                }
            }
            finally
            {
                if (wasClosed && DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return purchaseOrderNo;
        }

        public List<PurchaseOrderLookupItem> GetPurchaseOrders(int companyId, int branchId, int pageIndex, int pageSize)
        {
            List<PurchaseOrderLookupItem> items = new List<PurchaseOrderLookupItem>();
            bool wasClosed = DataConnection.State != ConnectionState.Open;

            try
            {
                if (wasClosed)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_PurchaseOrder, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@PageIndex", pageIndex);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);
                    cmd.Parameters.AddWithValue("@SortBy", "Poid");
                    cmd.Parameters.AddWithValue("@SortByDirection", "DESC");
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataSet dataSet = new DataSet();
                        adapter.Fill(dataSet);

                        if (dataSet.Tables.Count == 0)
                            return items;

                        foreach (DataRow row in dataSet.Tables[0].Rows)
                        {
                            items.Add(new PurchaseOrderLookupItem
                            {
                                BranchName = Convert.ToString(row["BranchName"]),
                                Poid = SafeInt(row, "Poid"),
                                PONo = SafeInt(row, "PONo"),
                                PODate = SafeDate(row, "PODate"),
                                GrandTotal = SafeDouble(row, "GrandTotal")
                            });
                        }
                    }
                }
            }
            finally
            {
                if (wasClosed && DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return items;
        }

        public PurchaseOrderLoadResult GetPurchaseOrderById(int poid)
        {
            PurchaseOrderLoadResult result = new PurchaseOrderLoadResult
            {
                Master = null,
                Details = new List<PurchaseOrderDetail>()
            };

            bool wasClosed = DataConnection.State != ConnectionState.Open;

            try
            {
                if (wasClosed)
                    DataConnection.Open();

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_PurchaseOrder, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Poid", poid);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataSet dataSet = new DataSet();
                        adapter.Fill(dataSet);

                        if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                        {
                            DataRow row = dataSet.Tables[0].Rows[0];
                            int ledgerId = SafeInt(row, "LedgerID");
                            int paymodeId = SafeInt(row, "PaymodeID");

                            result.Master = new PurchaseOrderMaster
                            {
                                POrderMasterId = SafeInt(row, "Poid"),
                                CompanyId = SafeInt(row, "CompanyId"),
                                FinYearId = SafeInt(row, "FinYearId"),
                                BranchId = SafeInt(row, "BranchId"),
                                PurchaseNo = SafeInt(row, "PONo"),
                                PurchaseDate = SafeDate(row, "PODate"),
                                LedgerID = ledgerId,
                                VendorName = GetLedgerName(ledgerId),
                                PaymodeID = paymodeId,
                                Paymode = GetPaymodeName(paymodeId),
                                SubTotal = SafeDouble(row, "SubTotal"),
                                SpDisPer = SafeDouble(row, "SpDisPer"),
                                SpDsiAmt = SafeDouble(row, "SpDsiAmt"),
                                BillDiscountPer = SafeDouble(row, "OrderDiscountPer"),
                                BillDiscountAmt = SafeDouble(row, "OrderDiscountAmt"),
                                TaxAmt = SafeDouble(row, "TaxAmt"),
                                Frieght = SafeDouble(row, "Frieght"),
                                ExpenseAmt = SafeDouble(row, "ExpenseAmt"),
                                OtherExpAmt = SafeDouble(row, "OtherExpAmt"),
                                GrandTotal = SafeDouble(row, "GrandTotal"),
                                Remarks = Convert.ToString(row["Remarks"]),
                                CreditPeriodTerm = Convert.ToString(row["CreditPeriodTerm"]),
                                ApprovedBy = Convert.ToString(row["ApprovedBy"]),
                                CreatedBy = Convert.ToString(row["CreatedBy"]),
                                ShipVia = Convert.ToString(row["ShipVia"]),
                                FobPoints = Convert.ToString(row["FobPoints"]),
                                TaxType = Convert.ToString(row["TaxType"]),
                                CessAmt = SafeDouble(row, "CessAmt")
                            };
                        }

                        if (dataSet.Tables.Count > 1)
                        {
                            foreach (DataRow row in dataSet.Tables[1].Rows)
                            {
                                int itemId = SafeInt(row, "ItemID");
                                int unitId = SafeInt(row, "UnitId");
                                double cost = SafeDouble(row, "Cost");
                                double qty = SafeDouble(row, "Qty");
                                double taxPer = SafeDouble(row, "TaxPer");
                                double totalAmount = SafeDouble(row, "TotalAmount");

                                result.Details.Add(new PurchaseOrderDetail
                                {
                                    CompanyId = result.Master != null ? result.Master.CompanyId : 0,
                                    FinYearId = result.Master != null ? result.Master.FinYearId : 0,
                                    BranchID = result.Master != null ? result.Master.BranchId : 0,
                                    PurchaseNo = result.Master != null ? result.Master.PurchaseNo : 0,
                                    PurchaseDate = result.Master != null ? result.Master.PurchaseDate : DateTime.Today,
                                    SlNo = result.Details.Count + 1,
                                    ItemID = itemId,
                                    ItemName = Convert.ToString(row["Description"]),
                                    UnitId = unitId,
                                    Unit = GetUnitName(unitId),
                                    BaseUnit = SafeBool(row, "BaseUnit") ? "Y" : "N",
                                    Packing = SafeDouble(row, "Packing"),
                                    Qty = qty,
                                    Free = SafeDouble(row, "Free"),
                                    Cost = cost,
                                    DisPer = SafeDouble(row, "DisPer"),
                                    DisAmt = SafeDouble(row, "DisAmt"),
                                    SalesPrice = SafeDouble(row, "SalesPrice"),
                                    TaxPer = taxPer,
                                    TaxAmt = SafeDouble(row, "TaxAmt"),
                                    TotalSP = SafeDouble(row, "TotalSP"),
                                    OriginalCost = SafeDouble(row, "OriginalCost"),
                                    OriginalSP = totalAmount,
                                    TaxType = string.Empty,
                                    CessAmt = SafeDouble(row, "CessAmt"),
                                    CessPer = SafeDouble(row, "CessPer"),
                                    RetailPrice = SafeDouble(row, "RetailPrice"),
                                    WholeSalePrice = SafeDouble(row, "WholeSalePrice"),
                                    CreditPrice = SafeDouble(row, "CreditPrice"),
                                    Barcode = GetItemCode(itemId),
                                    SingleItemCost = cost,
                                    BaseQty = qty,
                                    BaseQtyReceived = 0,
                                    TotalSst = SafeDouble(row, "TaxAmt")
                                });
                            }
                        }
                    }
                }
            }
            finally
            {
                if (wasClosed && DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return result;
        }

        public string SavePurchaseOrder(PurchaseOrderMaster master, IList<PurchaseOrderDetail> details)
        {
            return SaveInternal(master, details, false);
        }

        public string UpdatePurchaseOrder(PurchaseOrderMaster master, IList<PurchaseOrderDetail> details)
        {
            return SaveInternal(master, details, true);
        }

        /// <summary>
        /// Latest unit cost from purchase invoices (FrmPurchase / PMaster + PDetails) for the given vendor.
        /// Matches Item Master vendor-tab history: same item, vendor (LedgerID), most recent purchase first.
        /// </summary>
        public decimal GetLatestItemPurchaseCost(int itemId, int unitId, string unitName, int companyId, int branchId, int ledgerId)
        {
            if (itemId <= 0 || ledgerId <= 0)
                return 0m;

            if (unitId > 0)
            {
                decimal costByUnitId = QueryLatestPurchaseCost(itemId, unitId, null, companyId, branchId, ledgerId);
                if (costByUnitId > 0)
                    return costByUnitId;
            }

            if (!string.IsNullOrWhiteSpace(unitName))
            {
                decimal costByUnitName = QueryLatestPurchaseCost(itemId, 0, unitName.Trim(), companyId, branchId, ledgerId);
                if (costByUnitName > 0)
                    return costByUnitName;
            }

            return QueryLatestPurchaseCost(itemId, 0, null, companyId, branchId, ledgerId);
        }

        private decimal QueryLatestPurchaseCost(int itemId, int unitId, string unitName, int companyId, int branchId, int ledgerId)
        {
            bool wasClosed = DataConnection.State != ConnectionState.Open;

            try
            {
                if (wasClosed)
                    DataConnection.Open();

                string sql = @"
SELECT TOP 1 ISNULL(pd.Cost, 0)
FROM PDetails pd
INNER JOIN PMaster pm
    ON pm.PurchaseNo = pd.PurchaseNo
    AND pm.FinYearId = pd.FinYearId
    AND pm.BranchId = pd.BranchID
WHERE pd.ItemID = @ItemId
    AND pm.LedgerID = @LedgerId
    AND (@UnitId <= 0 OR pd.UnitId = @UnitId)
    AND (@UnitName IS NULL OR LTRIM(RTRIM(pd.Unit)) = LTRIM(RTRIM(@UnitName)))
    AND (@CompanyId <= 0 OR ISNULL(pm.CompanyId, @CompanyId) = @CompanyId)
    AND (@BranchId <= 0 OR pm.BranchId = @BranchId)
    AND ISNULL(pm.CancelFlag, 0) = 0
    AND ISNULL(pd.Cost, 0) > 0
ORDER BY pm.PurchaseDate DESC, pm.Pid DESC, pd.SlNo DESC";

                using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@ItemId", itemId);
                    cmd.Parameters.AddWithValue("@UnitId", unitId);
                    cmd.Parameters.AddWithValue("@UnitName", string.IsNullOrWhiteSpace(unitName) ? (object)DBNull.Value : unitName);
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId);
                    cmd.Parameters.AddWithValue("@LedgerId", ledgerId);

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        return Convert.ToDecimal(result);
                }
            }
            finally
            {
                if (wasClosed && DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }

            return 0m;
        }

        private string SaveInternal(PurchaseOrderMaster master, IList<PurchaseOrderDetail> details, bool isUpdate)
        {
            if (master == null)
                return "Purchase order header is empty.";

            if (details == null || details.Count == 0)
                return "Purchase order must contain at least one item.";

            bool wasClosed = DataConnection.State != ConnectionState.Open;
            SqlTransaction transaction = null;

            try
            {
                if (wasClosed)
                    DataConnection.Open();

                transaction = ((SqlConnection)DataConnection).BeginTransaction();

                if (isUpdate)
                {
                    string updateResult = ExecuteMasterCommand(master, transaction, "UPDATE");
                    if (!string.IsNullOrWhiteSpace(updateResult) &&
                        !string.Equals(updateResult, "SUCCESS", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(updateResult);
                    }

                    DeleteDetails(master, transaction);
                }
                else
                {
                    string createResult = ExecuteMasterCommand(master, transaction, "CREATE");
                    int poNo;
                    if (!int.TryParse(createResult, out poNo) || poNo <= 0)
                        throw new InvalidOperationException("Purchase order number was not generated.");

                    master.PurchaseNo = poNo;
                }

                foreach (PurchaseOrderDetail detail in details)
                {
                    CreateDetail(master, detail, transaction);
                }

                transaction.Commit();
                return "SUCCESS";
            }
            catch (Exception ex)
            {
                if (transaction != null)
                    transaction.Rollback();

                return "Error: " + ex.Message;
            }
            finally
            {
                if (wasClosed && DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        private string ExecuteMasterCommand(PurchaseOrderMaster master, SqlTransaction transaction, string operation)
        {
            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_PurchaseOrder, (SqlConnection)DataConnection, transaction))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Poid", master.POrderMasterId);
                cmd.Parameters.AddWithValue("@CompanyId", master.CompanyId);
                cmd.Parameters.AddWithValue("@FinYearId", master.FinYearId);
                cmd.Parameters.AddWithValue("@BranchId", master.BranchId);
                cmd.Parameters.AddWithValue("@BranchName", string.IsNullOrWhiteSpace(master.BranchName) ? (object)DBNull.Value : master.BranchName);
                cmd.Parameters.AddWithValue("@PONo", master.PurchaseNo);
                cmd.Parameters.AddWithValue("@PODate", master.PurchaseDate);
                cmd.Parameters.AddWithValue("@LedgerID", master.LedgerID);
                cmd.Parameters.AddWithValue("@VendorName", string.IsNullOrWhiteSpace(master.VendorName) ? (object)DBNull.Value : master.VendorName);
                cmd.Parameters.AddWithValue("@PaymodeID", master.PaymodeID);
                cmd.Parameters.AddWithValue("@Paymode", string.IsNullOrWhiteSpace(master.Paymode) ? (object)DBNull.Value : master.Paymode);
                cmd.Parameters.AddWithValue("@PaymodeLedgerID", master.PaymodeLedgerID);
                cmd.Parameters.AddWithValue("@CreditPeriod", master.CreditPeriod);
                cmd.Parameters.AddWithValue("@SubTotal", master.SubTotal);
                cmd.Parameters.AddWithValue("@SpDisPer", master.SpDisPer);
                cmd.Parameters.AddWithValue("@SpDsiAmt", master.SpDsiAmt);
                cmd.Parameters.AddWithValue("@OrderDiscountPer", master.BillDiscountPer);
                cmd.Parameters.AddWithValue("@OrderDiscountAmt", master.BillDiscountAmt);
                cmd.Parameters.AddWithValue("@TaxPer", master.TaxPer);
                cmd.Parameters.AddWithValue("@TaxAmt", master.TaxAmt);
                cmd.Parameters.AddWithValue("@Frieght", master.Frieght);
                cmd.Parameters.AddWithValue("@ExpenseAmt", master.ExpenseAmt);
                cmd.Parameters.AddWithValue("@OtherExpAmt", master.OtherExpAmt);
                cmd.Parameters.AddWithValue("@GrandTotal", master.GrandTotal);
                cmd.Parameters.AddWithValue("@CancelFlag", master.CancelFlag);
                cmd.Parameters.AddWithValue("@UserID", master.UserID);
                cmd.Parameters.AddWithValue("@UserName", string.IsNullOrWhiteSpace(master.UserName) ? (object)DBNull.Value : master.UserName);
                cmd.Parameters.AddWithValue("@TaxType", string.IsNullOrWhiteSpace(master.TaxType) ? (object)DBNull.Value : master.TaxType);
                cmd.Parameters.AddWithValue("@Remarks", string.IsNullOrWhiteSpace(master.Remarks) ? (object)DBNull.Value : master.Remarks);
                cmd.Parameters.AddWithValue("@RoundOff", master.RoundOff);
                cmd.Parameters.AddWithValue("@CessPer", master.CessPer);
                cmd.Parameters.AddWithValue("@CessAmt", master.CessAmt);
                cmd.Parameters.AddWithValue("@CalAfterTax", master.CalAfterTax);
                cmd.Parameters.AddWithValue("@CreditPeriodTerm", string.IsNullOrWhiteSpace(master.CreditPeriodTerm) ? (object)DBNull.Value : master.CreditPeriodTerm);
                cmd.Parameters.AddWithValue("@ApprovedBy", string.IsNullOrWhiteSpace(master.ApprovedBy) ? (object)DBNull.Value : master.ApprovedBy);
                cmd.Parameters.AddWithValue("@CreatedBy", string.IsNullOrWhiteSpace(master.CreatedBy) ? (object)DBNull.Value : master.CreatedBy);
                cmd.Parameters.AddWithValue("@ShipVia", string.IsNullOrWhiteSpace(master.ShipVia) ? (object)DBNull.Value : master.ShipVia);
                cmd.Parameters.AddWithValue("@FobPoints", string.IsNullOrWhiteSpace(master.FobPoints) ? (object)DBNull.Value : master.FobPoints);
                cmd.Parameters.AddWithValue("@CurrencyID", master.CurrencyID);
                cmd.Parameters.AddWithValue("@CurSymbol", string.IsNullOrWhiteSpace(master.CurSymbol) ? (object)DBNull.Value : master.CurSymbol);
                cmd.Parameters.AddWithValue("@PageIndex", 0);
                cmd.Parameters.AddWithValue("@PageSize", 0);
                cmd.Parameters.AddWithValue("@SortBy", "Poid");
                cmd.Parameters.AddWithValue("@SortByDirection", "DESC");
                cmd.Parameters.AddWithValue("@_Operation", operation);

                object result = cmd.ExecuteScalar();
                return result == null || result == DBNull.Value ? string.Empty : Convert.ToString(result);
            }
        }

        private void DeleteDetails(PurchaseOrderMaster master, SqlTransaction transaction)
        {
            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_PurchaseOrder_Details, (SqlConnection)DataConnection, transaction))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CompanyId", master.CompanyId);
                cmd.Parameters.AddWithValue("@FinYearId", master.FinYearId);
                cmd.Parameters.AddWithValue("@BranchID", master.BranchId);
                cmd.Parameters.AddWithValue("@PONo", master.PurchaseNo);
                cmd.Parameters.AddWithValue("@_Operation", "DELETE");
                cmd.ExecuteScalar();
            }
        }

        private void CreateDetail(PurchaseOrderMaster master, PurchaseOrderDetail detail, SqlTransaction transaction)
        {
            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_PurchaseOrder_Details, (SqlConnection)DataConnection, transaction))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CompanyId", detail.CompanyId);
                cmd.Parameters.AddWithValue("@FinYearId", detail.FinYearId);
                cmd.Parameters.AddWithValue("@BranchID", detail.BranchID);
                cmd.Parameters.AddWithValue("@BranchName", string.IsNullOrWhiteSpace(detail.BranchName) ? (object)DBNull.Value : detail.BranchName);
                cmd.Parameters.AddWithValue("@PONo", master.PurchaseNo);
                cmd.Parameters.AddWithValue("@PODate", master.PurchaseDate);
                cmd.Parameters.AddWithValue("@SlNo", detail.SlNo);
                cmd.Parameters.AddWithValue("@ItemID", detail.ItemID);
                cmd.Parameters.AddWithValue("@ItemName", string.IsNullOrWhiteSpace(detail.ItemName) ? (object)DBNull.Value : detail.ItemName);
                cmd.Parameters.AddWithValue("@UnitId", detail.UnitId);
                cmd.Parameters.AddWithValue("@Unit", string.IsNullOrWhiteSpace(detail.Unit) ? (object)DBNull.Value : detail.Unit);
                cmd.Parameters.AddWithValue("@BaseUnit", string.IsNullOrWhiteSpace(detail.BaseUnit) ? (object)DBNull.Value : detail.BaseUnit);
                cmd.Parameters.AddWithValue("@Packing", detail.Packing);
                cmd.Parameters.AddWithValue("@Qty", detail.Qty);
                cmd.Parameters.AddWithValue("@Free", detail.Free);
                cmd.Parameters.AddWithValue("@Cost", detail.Cost);
                cmd.Parameters.AddWithValue("@DisPer", detail.DisPer);
                cmd.Parameters.AddWithValue("@DisAmt", detail.DisAmt);
                cmd.Parameters.AddWithValue("@SalesPrice", detail.SalesPrice);
                cmd.Parameters.AddWithValue("@TaxPer", detail.TaxPer);
                cmd.Parameters.AddWithValue("@TaxAmt", detail.TaxAmt);
                cmd.Parameters.AddWithValue("@TotalSP", detail.TotalSP);
                cmd.Parameters.AddWithValue("@OriginalCost", detail.OriginalCost);
                cmd.Parameters.AddWithValue("@OriginalSP", detail.OriginalSP);
                cmd.Parameters.AddWithValue("@IsExpiry", detail.IsExpiry);
                cmd.Parameters.AddWithValue("@TaxType", string.IsNullOrWhiteSpace(detail.TaxType) ? (object)DBNull.Value : detail.TaxType);
                cmd.Parameters.AddWithValue("@CessAmt", detail.CessAmt);
                cmd.Parameters.AddWithValue("@CessPer", detail.CessPer);
                cmd.Parameters.AddWithValue("@_Operation", "CREATE");
                cmd.ExecuteScalar();
            }
        }

        private string GetLedgerName(int ledgerId)
        {
            return GetSingleString(
                "SELECT TOP 1 LedgerName FROM LedgerMaster WHERE LedgerID = @Id",
                ledgerId,
                "UNKNOWN VENDOR");
        }

        private string GetPaymodeName(int paymodeId)
        {
            return GetSingleString(
                "SELECT TOP 1 PayModeName FROM PayMode WHERE PayModeID = @Id",
                paymodeId,
                string.Empty);
        }

        private string GetUnitName(int unitId)
        {
            return GetSingleString(
                "SELECT TOP 1 UnitName FROM UnitMaster WHERE UnitID = @Id",
                unitId,
                unitId > 0 ? unitId.ToString() : string.Empty);
        }

        private string GetItemCode(int itemId)
        {
            return GetSingleString(
                "SELECT TOP 1 ISNULL(NULLIF(BarCode, ''), CAST(ItemId AS varchar(50))) FROM ItemMaster WHERE ItemId = @Id",
                itemId,
                itemId > 0 ? itemId.ToString() : string.Empty);
        }

        private string GetSingleString(string sql, int id, string fallbackValue)
        {
            using (SqlCommand cmd = new SqlCommand(sql, (SqlConnection)DataConnection))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@Id", id);
                object value = cmd.ExecuteScalar();
                return value == null || value == DBNull.Value ? fallbackValue : Convert.ToString(value);
            }
        }

        private static int SafeInt(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName))
                return 0;

            int value;
            return int.TryParse(Convert.ToString(row[columnName]), out value) ? value : 0;
        }

        private static double SafeDouble(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName))
                return 0;

            double value;
            return double.TryParse(Convert.ToString(row[columnName]), out value) ? value : 0;
        }

        private static DateTime SafeDate(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName))
                return DateTime.Today;

            DateTime value;
            return DateTime.TryParse(Convert.ToString(row[columnName]), out value) ? value : DateTime.Today;
        }

        private static bool SafeBool(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName))
                return false;

            bool value;
            if (bool.TryParse(Convert.ToString(row[columnName]), out value))
                return value;

            return string.Equals(Convert.ToString(row[columnName]), "1", StringComparison.OrdinalIgnoreCase);
        }
    }
}
