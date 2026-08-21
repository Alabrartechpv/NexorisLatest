using Nexoris.SyncService.Models;
using System.Threading.Tasks;

namespace Nexoris.SyncService.Services
{
    public interface ICentralApiClient
    {
        Task<bool> CheckCentralHealthAsync();
        Task<BatchSyncResponse> SendBatchAsync(BatchSyncRequest request);
        Task<BranchStatusResponse> GetBranchStatusAsync(int branchId);
        Task<MasterDataSyncResponse> PushMasterDataAsync(MasterDataSyncRequest request);
    }
}
