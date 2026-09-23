using Dapper;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Repository.TransactionRepository
{
    public class SmartReorderRepository : BaseRepostitory
    {
        public IEnumerable<SmartReorderItemModel> GetSmartReorderSuggestions(
            int? companyId,
            int? branchId,
            int? categoryId = null,
            int? groupId = null,
            string fromBarcode = null,
            string toBarcode = null)
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@CompanyId", companyId);
                parameters.Add("@BranchId", branchId);
                parameters.Add("@CategoryId", categoryId);
                parameters.Add("@GroupId", groupId);
                parameters.Add("@FromBarcode", string.IsNullOrWhiteSpace(fromBarcode) ? null : fromBarcode);
                parameters.Add("@ToBarcode", string.IsNullOrWhiteSpace(toBarcode) ? null : toBarcode);

                List<SmartReorderItemModel> suggestions = DataConnection.Query<SmartReorderItemModel>(
                    STOREDPROCEDURE._POS_GetSmartReorderSuggestions,
                    parameters,
                    commandType: CommandType.StoredProcedure).ToList();

                Dropdowns dropdowns = new Dropdowns();
                InactiveItemLookupInfo inactiveLookup = dropdowns.GetInactiveItemLookup();

                List<SmartReorderItemModel> result = new List<SmartReorderItemModel>();
                foreach (SmartReorderItemModel item in suggestions)
                {
                    if (inactiveLookup != null && inactiveLookup.IsInactive(item.ItemId, item.Barcode, item.ItemName, item.Alert, item.Reason))
                    {
                        continue;
                    }

                    item.FinalQuantity = item.SuggestedQuantity;
                    result.Add(item);
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return new List<SmartReorderItemModel>();
            }
        }

        public void RefreshReorderStats(int calculationDays)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@CalculationDays", calculationDays);
            DataConnection.Execute(
                STOREDPROCEDURE._POS_CalculateReorderStats,
                parameters,
                commandType: CommandType.StoredProcedure);
        }
    }
}
