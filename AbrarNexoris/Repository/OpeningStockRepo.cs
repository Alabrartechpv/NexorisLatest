using Dapper;
using ModelClass;
using ModelClass.Master;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class OpeningStockRepo : BaseRepostitory
    {
        /// <summary>
        /// Get all opening stock items using stored procedure
        /// </summary>
        public List<OpeningStockModel> GetOpeningStockItems(int branchId = 0, int itemId = 0, int groupId = 0)
        {
            try
            {
                DataConnection.Open();
                var parameters = new
                {
                    ItemId = itemId,
                    GpId = groupId,
                    _Operation = "GETALL"
                };

                List<OpeningStockModel> items = DataConnection.Query<OpeningStockModel>(
                    STOREDPROCEDURE._POS_Opening_Stock,
                    parameters,
                    commandType: CommandType.StoredProcedure).ToList();

                return items;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching opening stock items: " + ex.Message, ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }

        /// <summary>
        /// Save opening stock to PriceSettings table
        /// Updates existing records (for IsBaseUnit='Y'), only inserts if record doesn't exist
        /// OpnValue = OpnStk * OpeningCost
        /// </summary>
        public string SaveOpeningStock(List<OpeningStockModel> openingStockItems, int branchId, int companyId, int finYearId)
        {
            IDbTransaction transaction = null;
            try
            {
                DataConnection.Open();
                transaction = DataConnection.BeginTransaction();

                foreach (var item in openingStockItems)
                {
                    // Skip items with zero opening stock and zero cost
                    if (item.OpnStk == 0 && item.OpeningCost == 0)
                        continue;

                    // Calculate OpnValue = OpnStk * OpeningCost
                    double opnValue = item.OpnStk * item.OpeningCost;

                    // First, check if record already exists in PriceSettings for this item
                    string checkSql = @"SELECT COUNT(*) FROM PriceSettings 
                                        WHERE ItemId = @ItemId AND BranchId = @BranchId AND IsBaseUnit = 'Y'";

                    int existingCount = 0;
                    using (SqlCommand checkCmd = new SqlCommand(checkSql, (SqlConnection)DataConnection, (SqlTransaction)transaction))
                    {
                        checkCmd.Parameters.AddWithValue("@ItemId", item.ItemId);
                        checkCmd.Parameters.AddWithValue("@BranchId", branchId);
                        existingCount = (int)checkCmd.ExecuteScalar();
                    }

                    if (existingCount > 0)
                    {
                        // Record exists - UPDATE only opening stock fields
                        string updateSql = @"
                            UPDATE PriceSettings 
                            SET OpnStk = @OpnStk,
                                OpeningCost = @OpeningCost,
                                OpnDate = @OpnDate,
                                OpnValue = @OpnValue,
                                Stock = @Stock,
                                StockValue = @StockValue
                            WHERE ItemId = @ItemId 
                                AND BranchId = @BranchId 
                                AND IsBaseUnit = 'Y'";

                        using (SqlCommand updateCmd = new SqlCommand(updateSql, (SqlConnection)DataConnection, (SqlTransaction)transaction))
                        {
                            updateCmd.Parameters.AddWithValue("@OpnStk", item.OpnStk);
                            updateCmd.Parameters.AddWithValue("@OpeningCost", item.OpeningCost);
                            updateCmd.Parameters.AddWithValue("@OpnDate", item.OpnDate.HasValue ? (object)item.OpnDate.Value : DBNull.Value);
                            updateCmd.Parameters.AddWithValue("@OpnValue", opnValue);
                            updateCmd.Parameters.AddWithValue("@Stock", item.OpnStk);
                            updateCmd.Parameters.AddWithValue("@StockValue", opnValue);
                            updateCmd.Parameters.AddWithValue("@ItemId", item.ItemId);
                            updateCmd.Parameters.AddWithValue("@BranchId", branchId);
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // Record doesn't exist - INSERT using stored procedure
                        using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemMasterPriceSettings, (SqlConnection)DataConnection, (SqlTransaction)transaction))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@ItemId", item.ItemId);
                            cmd.Parameters.AddWithValue("@CompanyId", companyId);
                            cmd.Parameters.AddWithValue("@BranchId", branchId);
                            cmd.Parameters.AddWithValue("@BranchName", DBNull.Value);
                            cmd.Parameters.AddWithValue("@FinYearId", finYearId);
                            cmd.Parameters.AddWithValue("@Unit", item.Unit ?? "");
                            cmd.Parameters.AddWithValue("@UnitId", item.UnitId);
                            cmd.Parameters.AddWithValue("@Packing", item.Packing);
                            cmd.Parameters.AddWithValue("@Cost", item.OpeningCost);
                            cmd.Parameters.AddWithValue("@MarginPer", 0);
                            cmd.Parameters.AddWithValue("@MarginAmt", 0);
                            cmd.Parameters.AddWithValue("@RetailPrice", 0);
                            cmd.Parameters.AddWithValue("@WholeSalePrice", 0);
                            cmd.Parameters.AddWithValue("@CreditPrice", 0);
                            cmd.Parameters.AddWithValue("@CardPrice", 0);
                            cmd.Parameters.AddWithValue("@MRP", 0);
                            cmd.Parameters.AddWithValue("@MinPrice", DBNull.Value);
                            cmd.Parameters.AddWithValue("@StaffPrice", DBNull.Value);
                            cmd.Parameters.AddWithValue("@MDWalkinPrice", DBNull.Value);
                            cmd.Parameters.AddWithValue("@MDRetailPrice", DBNull.Value);
                            cmd.Parameters.AddWithValue("@MDCreditPrice", DBNull.Value);
                            cmd.Parameters.AddWithValue("@MDMrpPrice", DBNull.Value);
                            cmd.Parameters.AddWithValue("@MDCardPrice", DBNull.Value);
                            cmd.Parameters.AddWithValue("@MDStaffPrice", DBNull.Value);
                            cmd.Parameters.AddWithValue("@MDMinPrice", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Stock", item.OpnStk);
                            cmd.Parameters.AddWithValue("@OrderedStock", 0);
                            cmd.Parameters.AddWithValue("@StockValue", opnValue);
                            cmd.Parameters.AddWithValue("@ReOrder", 0);
                            cmd.Parameters.AddWithValue("@BarCode", "");
                            cmd.Parameters.AddWithValue("@TaxType", DBNull.Value);
                            cmd.Parameters.AddWithValue("@TaxAmt", 0);
                            cmd.Parameters.AddWithValue("@TaxPer", 0);
                            cmd.Parameters.AddWithValue("@OpnStk", item.OpnStk);
                            cmd.Parameters.AddWithValue("@IsBaseUnit", "Y");
                            cmd.Parameters.AddWithValue("@Costing", DBNull.Value);
                            SqlParameter photoParam = new SqlParameter("@Photo", SqlDbType.VarBinary, -1);
                            photoParam.Value = DBNull.Value;
                            cmd.Parameters.Add(photoParam);
                            SqlParameter photoByteArrayParam = new SqlParameter("@PhotoByteArray", SqlDbType.VarBinary, -1);
                            photoByteArrayParam.Value = DBNull.Value;
                            cmd.Parameters.Add(photoByteArrayParam);
                            cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                            cmd.ExecuteNonQuery();

                            // Update OpeningCost, OpnDate, OpnValue after insert
                            string updateAfterInsertSql = @"
                                UPDATE PriceSettings 
                                SET OpeningCost = @OpeningCost,
                                    OpnDate = @OpnDate,
                                    OpnValue = @OpnValue
                                WHERE ItemId = @ItemId 
                                    AND BranchId = @BranchId 
                                    AND IsBaseUnit = 'Y'";

                            using (SqlCommand updateCmd = new SqlCommand(updateAfterInsertSql, (SqlConnection)DataConnection, (SqlTransaction)transaction))
                            {
                                updateCmd.Parameters.AddWithValue("@OpeningCost", item.OpeningCost);
                                updateCmd.Parameters.AddWithValue("@OpnDate", item.OpnDate.HasValue ? (object)item.OpnDate.Value : DBNull.Value);
                                updateCmd.Parameters.AddWithValue("@OpnValue", opnValue);
                                updateCmd.Parameters.AddWithValue("@ItemId", item.ItemId);
                                updateCmd.Parameters.AddWithValue("@BranchId", branchId);
                                updateCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                transaction.Commit();
                return "Success";
            }
            catch (Exception ex)
            {
                if (transaction != null)
                    transaction.Rollback();
                throw new Exception("Error saving opening stock: " + ex.Message, ex);
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
        }
    }
}
