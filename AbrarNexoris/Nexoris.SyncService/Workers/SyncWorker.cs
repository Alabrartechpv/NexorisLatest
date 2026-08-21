using Nexoris.SyncService.Configuration;
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
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("===============================================================");
            Console.WriteLine(string.Format("   NEXORIS BRANCH SYNC WORKER STARTED (Branch: {0})", _settings.BranchId));
            Console.WriteLine(string.Format("   Central API Target: {0}", _settings.CentralApiUrl));
            Console.WriteLine(string.Format("   Polling: Every {0}s | Batch Size: {1}", _settings.PollIntervalSeconds, _settings.BatchSize));
            Console.WriteLine("===============================================================");
            Console.ResetColor();

            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await PerformSyncCycleAsync();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROR] Sync cycle error: " + ex.Message);
                    Console.ResetColor();
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

            Console.WriteLine("\nNexoris Branch Sync Worker STOPPED.");
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

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(string.Format("[INFO] [{0}] Found {1} pending transaction(s) to sync...",
                DateTime.Now.ToString("HH:mm:ss"), pendingItems.Count));
            Console.ResetColor();

            // 2. Health check to Head Office API
            bool isOnline = await _apiClient.CheckCentralHealthAsync();
            if (!isOnline)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(string.Format("[WARN] [{0}] Head Office API unreachable. Retrying in {1}s...",
                    DateTime.Now.ToString("HH:mm:ss"), _settings.PollIntervalSeconds));
                Console.ResetColor();
                return;
            }

            // 3. Assemble complete transaction payloads (Master + Line Items + Vouchers)
            var batch = await _dataProvider.AssembleBatchAsync(pendingItems, _settings.BranchId);

            if (!batch.Transactions.Any())
            {
                Console.WriteLine("[WARN] No transactions could be assembled from pending queue items.");
                return;
            }

            // 4. Send batch to Central API
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(string.Format("[INFO] [{0}] Sending batch {1} ({2} items) to Head Office...",
                DateTime.Now.ToString("HH:mm:ss"), batch.BatchId, batch.Transactions.Count));
            Console.ResetColor();

            var response = await _apiClient.SendBatchAsync(batch);

            if (response != null && response.Results != null && response.Results.Any())
            {
                // 5. Update local queue status
                await _dataProvider.ProcessResultsAsync(response.Results);

                int syncedCount = response.Results.Count(r => r.Status.Equals("Synced", StringComparison.OrdinalIgnoreCase) || r.Status.Equals("AlreadySynced", StringComparison.OrdinalIgnoreCase));
                int failedCount = response.Results.Count(r => r.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase));

                Console.ForegroundColor = failedCount == 0 ? ConsoleColor.Green : ConsoleColor.Yellow;
                Console.WriteLine(string.Format("[OK]   [{0}] Batch complete: {1} Synced, {2} Failed",
                    DateTime.Now.ToString("HH:mm:ss"), syncedCount, failedCount));
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(string.Format("[WARN] [{0}] Batch {1} failed or returned empty response.",
                    DateTime.Now.ToString("HH:mm:ss"), batch.BatchId));
                Console.ResetColor();
            }
        }
    }
}
