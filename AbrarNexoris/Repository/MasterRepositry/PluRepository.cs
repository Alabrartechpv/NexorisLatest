using Dapper;
using ModelClass.Master;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.MasterRepositry
{
    public class PluRepository : BaseRepostitory
    {
        /// <summary>
        /// Get all weighing items (ItemTypeId = 6) for a specific branch
        /// </summary>
        public List<PluModel> GetAllWeighingItems(int branchId)
        {
            try
            {
                DataConnection.Open();
                var parameters = new
                {
                    ItemId = 0,
                    BarCode = "",
                    Description = "",
                    BranchId = branchId
                };

                List<PluModel> items = DataConnection.Query<PluModel>(
                    STOREDPROCEDURE._POS_GetWeighingItems,
                    parameters,
                    commandType: CommandType.StoredProcedure).ToList();

                return items;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching weighing items: " + ex.Message, ex);
            }
            finally
            {
                DataConnection.Close();
            }
        }

        /// <summary>
        /// Get weighing item by ItemId for a specific branch
        /// </summary>
        public PluModel GetWeighingItemById(int itemId, int branchId)
        {
            try
            {
                DataConnection.Open();
                var parameters = new
                {
                    ItemId = itemId,
                    BarCode = "",
                    Description = "",
                    BranchId = branchId
                };

                PluModel item = DataConnection.Query<PluModel>(
                    STOREDPROCEDURE._POS_GetWeighingItems,
                    parameters,
                    commandType: CommandType.StoredProcedure).FirstOrDefault();

                return item;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching weighing item by ID: " + ex.Message, ex);
            }
            finally
            {
                DataConnection.Close();
            }
        }

        /// <summary>
        /// Get weighing item by BarCode for a specific branch
        /// </summary>
        public PluModel GetWeighingItemByBarCode(string barCode, int branchId)
        {
            try
            {
                DataConnection.Open();
                var parameters = new
                {
                    ItemId = 0,
                    BarCode = barCode ?? "",
                    Description = "",
                    BranchId = branchId
                };

                PluModel item = DataConnection.Query<PluModel>(
                    STOREDPROCEDURE._POS_GetWeighingItems,
                    parameters,
                    commandType: CommandType.StoredProcedure).FirstOrDefault();

                return item;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching weighing item by BarCode: " + ex.Message, ex);
            }
            finally
            {
                DataConnection.Close();
            }
        }

        /// <summary>
        /// Get weighing items by Description (partial match) for a specific branch
        /// </summary>
        public List<PluModel> GetWeighingItemsByDescription(string description, int branchId)
        {
            try
            {
                DataConnection.Open();
                var parameters = new
                {
                    ItemId = 0,
                    BarCode = "",
                    Description = description ?? "",
                    BranchId = branchId
                };

                List<PluModel> items = DataConnection.Query<PluModel>(
                    STOREDPROCEDURE._POS_GetWeighingItems,
                    parameters,
                    commandType: CommandType.StoredProcedure).ToList();

                return items;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching weighing items by Description: " + ex.Message, ex);
            }
            finally
            {
                DataConnection.Close();
            }
        }
    }
}
