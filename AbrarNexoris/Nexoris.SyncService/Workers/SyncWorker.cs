using Nexoris.SyncService.Configuration;
using Nexoris.SyncService.Logging;
using Nexoris.SyncService.Models;
using Nexoris.SyncService.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nexoris.SyncService.Workers
{
    public class SyncWorker
    {
        private readonly ILocalDataProvider _dataProvider;
        private readonly ICentralApiClient _apiClient;
        private readonly SyncSettings _settings;
        private bool _isRunning;
        private bool _hasCheckedOnboarding;

        public SyncWorker(
            ILocalDataProvider dataProvider,
            ICentralApiClient apiClient,
            SyncSettings settings)
        {
            _dataProvider = dataProvider;
            _apiClient = apiClient;
            _settings = settings;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            _isRunning = true;
            FileLogger.Info("===============================================================");
            FileLogger.Info("   NEXORIS BRANCH SYNC WORKER STARTED (Branch: {0})", _settings.BranchId);
            FileLogger.Info("   Central API Target: {0}", _settings.CentralApiUrl);
            FileLogger.Info("   Polling: Every {0}s | Batch Size: {1}", _settings.PollIntervalSeconds, _settings.BatchSize);
            FileLogger.Info("===============================================================");

            // Initial automated onboarding check on startup
            await CheckAndPerformInitialOnboardingAsync();

            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (!_hasCheckedOnboarding)
                    {
                        await CheckAndPerformInitialOnboardingAsync();
                    }

                    await PerformSyncCycleAsync();
                }
                catch (Exception ex)
                {
                    FileLogger.Error(ex, "Sync cycle error");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(_settings.PollIntervalSeconds), cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }

            FileLogger.Info("Nexoris Branch Sync Worker STOPPED.");
        }

        private async Task CheckAndPerformInitialOnboardingAsync()
        {
            try
            {
                var status = await _apiClient.GetBranchStatusAsync(_settings.BranchId);
                if (status != null)
                {
                    if (status.InitialSyncRequired)
                    {
                        FileLogger.Info("[ONBOARDING] Central DB has 0 PriceSettings for Branch {0}. Starting Automated Baseline Sync...",
                            _settings.BranchId);

                        var localPrices = await _dataProvider.GetLocalPriceSettingsAsync(_settings.BranchId);
                        if (localPrices != null && localPrices.Any())
                        {
                            FileLogger.Info("[ONBOARDING] Uploading {0} local PriceSettings master records to Head Office...", localPrices.Count);
                            var response = await _apiClient.PushMasterDataAsync(new MasterDataSyncRequest
                            {
                                BranchId = _settings.BranchId,
                                PriceSettings = localPrices
                            });

                            if (response != null && response.Success)
                            {
                                FileLogger.Success("Automated Onboarding Complete! {0} items registered in Central DB.",
                                    response.SyncedItemCount);
                                _hasCheckedOnboarding = true;
                            }
                        }
                    }
                    else
                    {
                        FileLogger.Success("Branch {0} baseline verified ({1} items registered at Head Office).",
                            _settings.BranchId, status.ExistingItemCount);
                        _hasCheckedOnboarding = true;
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.Warn("Onboarding handshake check deferred: {0}", ex.Message);
            }
        }

        public void Stop()
        {
            _isRunning = false;
        }

        private async Task PerformSyncCycleAsync()
        {
            // 1. Fetch pending queue items from local database
            var pendingItems = await _dataProvider.GetPendingQueueItemsAsync(_settings.BatchSize);

            if (pendingItems == null || !pendingItems.Any())
            {
                return;
            }

            FileLogger.Info("Found {0} pending transaction(s) to sync...", pendingItems.Count);

            // 2. Health check to Head Office API
            bool isOnline = await _apiClient.CheckCentralHealthAsync();
            if (!isOnline)
            {
                FileLogger.Warn("Head Office API unreachable. Retrying in {0}s...", _settings.PollIntervalSeconds);
                return;
            }

            // 3. Process ITEM_MASTER items individually if present
            var masterItems = pendingItems.Where(p => p.EntityType.Equals("ITEM_MASTER", StringComparison.OrdinalIgnoreCase)).ToList();
            var transactionItems = pendingItems.Where(p => !p.EntityType.Equals("ITEM_MASTER", StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var m in masterItems)
            {
                if (int.TryParse(m.EntityID, out int itemId))
                {
                    FileLogger.Info("[ITEM SYNC] Syncing ItemId {0} master catalog & units to Head Office...", itemId);

                    var masterReq = await _dataProvider.AssembleMasterDataAsync(itemId, _settings.BranchId);
                    var masterResp = await _apiClient.PushMasterDataAsync(masterReq);

                    if (masterResp != null && masterResp.Success)
                    {
                        FileLogger.Success("ItemId {0} synced ({1} unit price settings updated).",
                            itemId, masterResp.SyncedItemCount);
                        await _dataProvider.UpdateQueueStatusAsync(m.TransactionGuid, "Synced");
                    }
                    else
                    {
                        string err = masterResp != null ? masterResp.Message : "Unknown API error";
                        await _dataProvider.UpdateQueueStatusAsync(m.TransactionGuid, "Failed", err);
                    }
                }
            }

            if (!transactionItems.Any())
            {
                return;
            }

            // 4. Assemble complete transaction payloads (Master + Line Items + Vouchers)
            var batch = await _dataProvider.AssembleBatchAsync(transactionItems, _settings.BranchId);

            if (!batch.Transactions.Any())
            {
                FileLogger.Warn("No transactions could be assembled from pending queue items.");
                return;
            }

            // 5. Send batch to Central API
            FileLogger.Info("Sending batch {0} ({1} items) to Head Office...",
                batch.BatchId, batch.Transactions.Count);

            var response = await _apiClient.SendBatchAsync(batch);

            if (response != null && response.Results != null && response.Results.Any())
            {
                // 5. Update local queue status
                await _dataProvider.ProcessResultsAsync(response.Results);

                int syncedCount = response.Results.Count(r => r.Status.Equals("Synced", StringComparison.OrdinalIgnoreCase) || r.Status.Equals("AlreadySynced", StringComparison.OrdinalIgnoreCase));
                int failedCount = response.Results.Count(r => r.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase));

                if (failedCount == 0)
                {
                    FileLogger.Success("Batch complete: {0} Synced, {1} Failed", syncedCount, failedCount);
                }
                else
                {
                    FileLogger.Warn("Batch complete: {0} Synced, {1} Failed", syncedCount, failedCount);
                }
            }
            else
            {
                FileLogger.Warn("Batch {0} failed or returned empty response.", batch.BatchId);
            }
        }
    }
}
