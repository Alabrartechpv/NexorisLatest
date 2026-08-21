using Nexoris.SyncService.Configuration;
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
        static void Main(string[] args)
        {
            Console.Title = "Nexoris Branch Sync Service (.NET Framework 4.6.1)";

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
                Console.WriteLine("\nStopping sync worker...");
                worker.Stop();
                cts.Cancel();
            };

            Task.Run(() => worker.RunAsync(cts.Token)).Wait();
        }
    }
}
