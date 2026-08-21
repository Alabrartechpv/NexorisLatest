using Nexoris.SyncService.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nexoris.SyncService.Services
{
    public interface ILocalDataProvider
    {
        Task<List<SyncQueueItem>> GetPendingQueueItemsAsync(int batchSize);
        Task<BatchSyncRequest> AssembleBatchAsync(List<SyncQueueItem> queueItems, int branchId);
        Task UpdateQueueStatusAsync(Guid transactionGuid, string status, string errorMessage = null);
        Task ProcessResultsAsync(List<SyncItemResult> results);
        Task<List<PriceSettingsSyncDto>> GetLocalPriceSettingsAsync(int branchId);
        Task<MasterDataSyncRequest> AssembleMasterDataAsync(int itemId, int branchId);
    }
}
