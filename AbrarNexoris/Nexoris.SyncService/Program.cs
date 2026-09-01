using Nexoris.SyncService.Configuration;
using Nexoris.SyncService.Logging;
using Nexoris.SyncService.Services;
using Nexoris.SyncService.Workers;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Nexoris.SyncService
{
    class Program
    {
        private static Mutex _appMutex;

        static void Main(string[] args)
        {
            Console.Title = "Nexoris Branch Sync Service (.NET Framework 4.6.1)";

            bool isNewInstance;
            _appMutex = new Mutex(true, "NexorisSyncService_SingleInstanceMutex", out isNewInstance);
            if (!isNewInstance)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n===============================================================");
                Console.WriteLine(" [INFO] Nexoris Branch Sync Service is ALREADY running on this PC.");
                Console.WriteLine(" The sync worker is actively processing queue items in the background.");
                Console.WriteLine("===============================================================");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to exit this duplicate window...");
                Thread.Sleep(3000);
                return;
            }

            var settings = new SyncSettings
            {
                BranchId = int.TryParse(ConfigurationManager.AppSettings["BranchId"], out int bId) ? bId : 1,
                ApiKey = ConfigurationManager.AppSettings["ApiKey"] ?? "MNG_SECURE_API_KEY_2026",
                CentralApiUrl = ConfigurationManager.AppSettings["CentralApiUrl"] ?? "http://localhost:5000",
                PollIntervalSeconds = int.TryParse(ConfigurationManager.AppSettings["PollIntervalSeconds"], out int poll) ? poll : 5,
                BatchSize = int.TryParse(ConfigurationManager.AppSettings["BatchSize"], out int batch) ? batch : 20,
                MaxRetries = int.TryParse(ConfigurationManager.AppSettings["MaxRetries"], out int retries) ? retries : 10
            };

            var dataProvider = new LocalDataProvider();
            var apiClient = new CentralApiClient(settings);
            var worker = new SyncWorker(dataProvider, apiClient, settings);

            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                FileLogger.Info("Stopping sync worker...");
                worker.Stop();
                cts.Cancel();
            };

            Task.Run(() => worker.RunAsync(cts.Token)).Wait();
        }
    }
}
