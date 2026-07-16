using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ReportRepository
{
    /// <summary>
    /// Repository for Item Report operations
    /// </summary>
    public class ItemReportRepo : BaseRepostitory
    {
        /// <summary>
        /// Get complete Item Report data with all result sets
        /// </summary>
        /// <param name="finYearId">Financial Year ID</param>
        /// <param name="companyId">Company ID</param>
        /// <param name="branchId">Branch ID (0 for all branches)</param>
        /// <param name="itemId">Item ID</param>
        /// <returns>ItemReportData containing all result sets</returns>
        public ItemReportData GetItemReport(int finYearId, int companyId, int branchId, int itemId)
        {
            ItemReportData reportData = new ItemReportData();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE._POS_ItemReport, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120; // 2 minutes timeout for large data

                    cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);
                    cmd.Parameters.AddWithValue("@BranchId", branchId == 0 ? (object)DBNull.Value : branchId);
                    cmd.Parameters.AddWithValue("@ItemId", itemId);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);

                        // Result Set 1: Transaction History
                        if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            reportData.Transactions = new List<ItemTransactionModel>();
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                reportData.Transactions.Add(new ItemTransactionModel
                                {
                                    Operation = row["Operation"]?.ToString() ?? "",
                                    BranchId = row["BranchId"] != DBNull.Value ? Convert.ToInt32(row["BranchId"]) : 0,
                                    DT = row["DT"] != DBNull.Value ? Convert.ToDateTime(row["DT"]) : DateTime.MinValue,
                                    RefNo = row["RefNo"]?.ToString() ?? "",
                                    UnitId = row["UnitId"] != DBNull.Value ? Convert.ToInt32(row["UnitId"]) : 0,
                                    Qty = row["Qty"] != DBNull.Value ? Convert.ToDecimal(row["Qty"]) : 0,
                                    Packing = row["Packing"] != DBNull.Value ? Convert.ToDecimal(row["Packing"]) : 0,
                                    IsBaseUnit = row["IsBaseUnit"]?.ToString() ?? "N",
                                    Cost = row["Cost"] != DBNull.Value ? Convert.ToDecimal(row["Cost"]) : 0,
                                    Way = row["Way"]?.ToString() ?? "",
                                    UnitPrice = row["UnitPrice"] != DBNull.Value ? Convert.ToDecimal(row["UnitPrice"]) : 0,
                                    Balance = row["Balance"] != DBNull.Value ? Convert.ToDecimal(row["Balance"]) : 0,
                                    BranchName = row["BranchName"]?.ToString() ?? "",
                                    UnitName = row["UnitName"]?.ToString() ?? "",
                                    Account = row["Account"]?.ToString() ?? "",
                                    RefId = row["RefId"] != DBNull.Value ? Convert.ToInt32(row["RefId"]) : 0
                                });
                            }
                        }

                        // Result Set 2: Mobile Orders
                        if (ds.Tables.Count > 1 && ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                        {
                            reportData.MobileOrders = new List<MobileOrderModel>();
                            foreach (DataRow row in ds.Tables[1].Rows)
                            {
                                reportData.MobileOrders.Add(new MobileOrderModel
                                {
                                    BranchId = row["BranchId"] != DBNull.Value ? Convert.ToInt32(row["BranchId"]) : 0,
                                    BranchName = row["BranchName"]?.ToString() ?? "",
                                    OrderDate = row["OrderDate"] != DBNull.Value ? Convert.ToDateTime(row["OrderDate"]) : DateTime.MinValue,
                                    OrderNo = row["OrderNo"]?.ToString() ?? "",
                                    UnitName = row["UnitName"]?.ToString() ?? "",
                                    Qty = row["Qty"] != DBNull.Value ? Convert.ToDecimal(row["Qty"]) : 0,
                                    Packing = row["Packing"] != DBNull.Value ? Convert.ToDecimal(row["Packing"]) : 0,
                                    IsBaseUnit = row["IsBaseUnit"]?.ToString() ?? "N",
                                    OrderStatus = row["OrderStatus"]?.ToString() ?? ""
                                });
                            }
                        }

                        // Result Set 3: Item Details
                        if (ds.Tables.Count > 2 && ds.Tables[2] != null && ds.Tables[2].Rows.Count > 0)
                        {
                            DataRow row = ds.Tables[2].Rows[0];
                            reportData.ItemDetails = new ItemDetailsModel
                            {
                                ItemName = row["ItemName"]?.ToString() ?? "",
                                BrandName = row["BrandName"]?.ToString() ?? "",
                                NameInLocalLanguage = row["NameInLocalLanguage"]?.ToString() ?? "",
                                PhotoByteArray = row["PhotoByteArray"] != DBNull.Value ? (byte[])row["PhotoByteArray"] : null,
                                GroupName = row["GroupName"]?.ToString() ?? "",
                                CategoryName = row["CategoryName"]?.ToString() ?? "",
                                SubCategoryName = row["SubCategoryName"]?.ToString() ?? "",
                                Line = row["Line"]?.ToString() ?? "",
                                RackName = row["RackName"]?.ToString() ?? "",
                                Row = row["Row"]?.ToString() ?? ""
                            };
                        }

                        // Result Set 4: Price Settings
                        if (ds.Tables.Count > 3 && ds.Tables[3] != null && ds.Tables[3].Rows.Count > 0)
                        {
                            reportData.PriceSettings = new List<PriceSettingsModel>();
                            foreach (DataRow row in ds.Tables[3].Rows)
                            {
                                reportData.PriceSettings.Add(new PriceSettingsModel
                                {
                                    BranchName = row["BranchName"]?.ToString() ?? "",
                                    ItemId = row["ItemId"] != DBNull.Value ? Convert.ToInt32(row["ItemId"]) : 0,
                                    UnitId = row["UnitId"] != DBNull.Value ? Convert.ToInt32(row["UnitId"]) : 0,
                                    UnitName = row["UnitName"]?.ToString() ?? "",
                                    IsBaseUnit = row["IsBaseUnit"]?.ToString() ?? "",
                                    Cost = row["Cost"] != DBNull.Value ? Convert.ToDecimal(row["Cost"]) : 0,
                                    TaxP = row["TaxP"] != DBNull.Value ? Convert.ToDecimal(row["TaxP"]) : 0,
                                    CessP = row["CessP"] != DBNull.Value ? Convert.ToDecimal(row["CessP"]) : 0,
                                    KFCP = row["KFCP"] != DBNull.Value ? Convert.ToDecimal(row["KFCP"]) : 0,
                                    MRP = row["MRP"] != DBNull.Value ? Convert.ToDecimal(row["MRP"]) : 0,
                                    Retail = row["Retail"] != DBNull.Value ? Convert.ToDecimal(row["Retail"]) : 0,
                                    WholeSale = row["WholeSale"] != DBNull.Value ? Convert.ToDecimal(row["WholeSale"]) : 0,
                                    Credit = row["Credit"] != DBNull.Value ? Convert.ToDecimal(row["Credit"]) : 0,
                                    Card = row["Card"] != DBNull.Value ? Convert.ToDecimal(row["Card"]) : 0
                                });
                            }
                        }

                        // Result Set 5: Vendor List
                        if (ds.Tables.Count > 4 && ds.Tables[4] != null && ds.Tables[4].Rows.Count > 0)
                        {
                            reportData.Vendors = new List<VendorListModel>();
                            foreach (DataRow row in ds.Tables[4].Rows)
                            {
                                reportData.Vendors.Add(new VendorListModel
                                {
                                    BranchName = row["BranchName"]?.ToString() ?? "",
                                    VendorName = row["VendorName"]?.ToString() ?? ""
                                });
                            }
                        }

                        // Result Set 6: Stock Summary
                        if (ds.Tables.Count > 5 && ds.Tables[5] != null && ds.Tables[5].Rows.Count > 0)
                        {
                            reportData.StockSummary = new List<StockSummaryModel>();
                            foreach (DataRow row in ds.Tables[5].Rows)
                            {
                                reportData.StockSummary.Add(new StockSummaryModel
                                {
                                    BranchName = row["BranchName"]?.ToString() ?? "",
                                    OrderedStock = row["OrderedStock"] != DBNull.Value ? Convert.ToDecimal(row["OrderedStock"]) : 0,
                                    AvailableStock = row["AvailableStock"] != DBNull.Value ? Convert.ToDecimal(row["AvailableStock"]) : 0,
                                    Stock = row["Stock"] != DBNull.Value ? Convert.ToDecimal(row["Stock"]) : 0
                                });
                            }
                        }

                        // Result Set 7: Pending Orders
                        if (ds.Tables.Count > 6 && ds.Tables[6] != null && ds.Tables[6].Rows.Count > 0)
                        {
                            reportData.PendingOrders = new List<PendingOrderModel>();
                            foreach (DataRow row in ds.Tables[6].Rows)
                            {
                                reportData.PendingOrders.Add(new PendingOrderModel
                                {
                                    BranchId = row["BranchId"] != DBNull.Value ? Convert.ToInt32(row["BranchId"]) : 0,
                                    BranchName = row["BranchName"]?.ToString() ?? "",
                                    OrderDate = row["OrderDate"] != DBNull.Value ? Convert.ToDateTime(row["OrderDate"]) : DateTime.MinValue,
                                    OrderNo = row["OrderNo"]?.ToString() ?? "",
                                    UnitName = row["UnitName"]?.ToString() ?? "",
                                    Qty = row["Qty"] != DBNull.Value ? Convert.ToDecimal(row["Qty"]) : 0,
                                    Packing = row["Packing"] != DBNull.Value ? Convert.ToDecimal(row["Packing"]) : 0,
                                    IsBaseUnit = row["IsBaseUnit"]?.ToString() ?? "N",
                                    OrderStatus = row["OrderStatus"]?.ToString() ?? "",
                                    PlatForm = row["PlatForm"]?.ToString() ?? "",
                                    CustomerName = row["CustomerName"]?.ToString() ?? ""
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving Item Report data for ItemId: {itemId}. {ex.Message}", ex);
            }
            finally
            {
                DataConnection.Close();
            }

            return reportData;
        }
    }
}
