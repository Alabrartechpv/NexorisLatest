using Newtonsoft.Json;
using Nexoris.CentralApi.Models.DTOs;
using Nexoris.CentralApi.Services;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nexoris.CentralApi
{
    class Program
    {
        private static HttpListener _listener;
        private static ICentralSyncService _syncService;
        private static bool _isRunning = true;

        static void Main(string[] args)
        {
            Console.Title = "Nexoris Central API (.NET Framework 4.6.1)";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("===============================================================");
            Console.WriteLine("        NEXORIS HEAD OFFICE CENTRAL API SERVER (.NET 4.6.1)   ");
            Console.WriteLine("===============================================================");
            Console.ResetColor();

            _syncService = new CentralSyncService();

            string port = ConfigurationManager.AppSettings["ListeningPort"] ?? "5000";
            _listener = new HttpListener();
            _listener.Prefixes.Add(string.Format("http://localhost:{0}/", port));

            try
            {
                _listener.Start();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(string.Format("[OK] Central API Server listening on http://localhost:{0}/", port));
                Console.WriteLine("[OK] Endpoints available:");
                Console.WriteLine("     - GET  /api/v1/health");
                Console.WriteLine("     - POST /api/v1/sync/transactions");
                Console.ResetColor();
                Console.WriteLine("\nPress Ctrl+C to stop the server...\n");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] Failed to start HttpListener: " + ex.Message);
                Console.ResetColor();
                Console.ReadLine();
                return;
            }

            Task.Run(() => ListenLoop());

            // Keep main thread alive
            var waitHandle = new ManualResetEvent(false);
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                _isRunning = false;
                _listener.Stop();
                waitHandle.Set();
            };

            waitHandle.WaitOne();
        }

        private static async Task ListenLoop()
        {
            while (_isRunning)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    // Process each request concurrently
                    Task.Run(() => HandleRequestAsync(context));
                }
                catch (HttpListenerException)
                {
                    // Server stopped
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[WARN] Listener error: " + ex.Message);
                }
            }
        }

        private static async Task HandleRequestAsync(HttpListenerContext context)
        {
            var req = context.Request;
            var res = context.Response;

            // Add standard CORS headers
            res.Headers.Add("Access-Control-Allow-Origin", "*");
            res.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            res.Headers.Add("Access-Control-Allow-Headers", "Content-Type, X-Branch-Id, X-Api-Key");

            if (req.HttpMethod == "OPTIONS")
            {
                res.StatusCode = 200;
                res.Close();
                return;
            }

            string rawUrl = req.RawUrl?.ToLowerInvariant() ?? "";
            string path = req.Url.AbsolutePath.ToLowerInvariant();

            try
            {
                // ROUTE 1: GET /api/v1/health
                if (req.HttpMethod == "GET" && path.Contains("/health"))
                {
                    bool isDbOk = await _syncService.CheckDatabaseHealthAsync();
                    var healthObj = new
                    {
                        Status = isDbOk ? "Healthy" : "Degraded",
                        Database = isDbOk ? "Connected" : "Disconnected",
                        UtcTimestamp = DateTime.UtcNow
                    };
                    await WriteJsonResponseAsync(res, healthObj, isDbOk ? 200 : 503);
                    return;
                }

                // ROUTE 2: GET /api/v1/sync/branch-status
                if (req.HttpMethod == "GET" && path.Contains("/sync/branch-status"))
                {
                    string branchIdHeader = req.Headers["X-Branch-Id"];
                    string apiKey = req.Headers["X-Api-Key"];

                    if (string.IsNullOrEmpty(branchIdHeader) || !int.TryParse(branchIdHeader, out int branchId) || string.IsNullOrEmpty(apiKey))
                    {
                        await WriteJsonResponseAsync(res, new { Error = "Missing or invalid X-Branch-Id or X-Api-Key headers." }, 401);
                        return;
                    }

                    bool isValidKey = await _syncService.ValidateBranchKeyAsync(branchId, apiKey);
                    if (!isValidKey)
                    {
                        await WriteJsonResponseAsync(res, new { Error = "Forbidden: Invalid API key for branch." }, 403);
                        return;
                    }

                    var statusResult = await _syncService.GetBranchStatusAsync(branchId);
                    await WriteJsonResponseAsync(res, statusResult, 200);
                    return;
                }

                // ROUTE 3: POST /api/v1/sync/master-data (Initial Onboarding & Master Sync)
                if (req.HttpMethod == "POST" && path.Contains("/sync/master-data"))
                {
                    string branchIdHeader = req.Headers["X-Branch-Id"];
                    string apiKey = req.Headers["X-Api-Key"];

                    if (string.IsNullOrEmpty(branchIdHeader) || !int.TryParse(branchIdHeader, out int branchId) || string.IsNullOrEmpty(apiKey))
                    {
                        await WriteJsonResponseAsync(res, new { Error = "Missing or invalid X-Branch-Id or X-Api-Key headers." }, 401);
                        return;
                    }

                    bool isValidKey = await _syncService.ValidateBranchKeyAsync(branchId, apiKey);
                    if (!isValidKey)
                    {
                        await WriteJsonResponseAsync(res, new { Error = "Forbidden: Invalid API key for branch." }, 403);
                        return;
                    }

                    string requestBody;
                    using (var reader = new StreamReader(req.InputStream, req.ContentEncoding))
                    {
                        requestBody = await reader.ReadToEndAsync();
                    }

                    var masterRequest = JsonConvert.DeserializeObject<MasterDataSyncRequest>(requestBody);
                    if (masterRequest == null || masterRequest.PriceSettings == null)
                    {
                        await WriteJsonResponseAsync(res, new { Error = "Invalid JSON payload." }, 400);
                        return;
                    }

                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine(string.Format("[ONBOARDING] [{0}] Received Master Data payload from Branch {1} ({2} items)...",
                        DateTime.Now.ToString("HH:mm:ss"), branchId, masterRequest.PriceSettings.Count));
                    Console.ResetColor();

                    var masterResult = await _syncService.IngestMasterDataAsync(masterRequest);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(string.Format("[OK]          [{0}] Master Data Synced: {1} items saved.",
                        DateTime.Now.ToString("HH:mm:ss"), masterResult.SyncedItemCount));
                    Console.ResetColor();

                    await WriteJsonResponseAsync(res, masterResult, 200);
                    return;
                }

                // ROUTE 4: POST /api/v1/sync/transactions
                if (req.HttpMethod == "POST" && path.Contains("/sync/transactions"))
                {
                    string branchIdHeader = req.Headers["X-Branch-Id"];
                    string apiKey = req.Headers["X-Api-Key"];

                    if (string.IsNullOrEmpty(branchIdHeader) || !int.TryParse(branchIdHeader, out int branchId) || string.IsNullOrEmpty(apiKey))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(string.Format("[WARN] [{0}] Unauthorized attempt - Missing X-Branch-Id or X-Api-Key headers", DateTime.Now.ToString("HH:mm:ss")));
                        Console.ResetColor();
                        await WriteJsonResponseAsync(res, new { Error = "Missing or invalid X-Branch-Id or X-Api-Key headers." }, 401);
                        return;
                    }

                    bool isValidKey = await _syncService.ValidateBranchKeyAsync(branchId, apiKey);
                    if (!isValidKey)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(string.Format("[WARN] [{0}] Forbidden attempt - Invalid API Key for Branch {1}", DateTime.Now.ToString("HH:mm:ss"), branchId));
                        Console.ResetColor();
                        await WriteJsonResponseAsync(res, new { Error = "Forbidden: Invalid API key for branch." }, 403);
                        return;
                    }

                    // Read JSON payload
                    string requestBody;
                    using (var reader = new StreamReader(req.InputStream, req.ContentEncoding))
                    {
                        requestBody = await reader.ReadToEndAsync();
                    }

                    var batchRequest = JsonConvert.DeserializeObject<BatchSyncRequest>(requestBody);
                    if (batchRequest == null || batchRequest.Transactions == null)
                    {
                        await WriteJsonResponseAsync(res, new { Error = "Invalid JSON payload." }, 400);
                        return;
                    }

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(string.Format("[INFO] [{0}] Received Batch {1} from Branch {2} with {3} transactions...",
                        DateTime.Now.ToString("HH:mm:ss"), batchRequest.BatchId, branchId, batchRequest.Transactions.Count));
                    Console.ResetColor();

                    var result = await _syncService.ProcessBatchAsync(batchRequest);

                    int syncedCount = 0;
                    int failedCount = 0;
                    foreach (var r in result.Results)
                    {
                        if (r.Status == "Synced" || r.Status == "AlreadySynced") syncedCount++;
                        else failedCount++;
                    }

                    Console.ForegroundColor = failedCount == 0 ? ConsoleColor.Green : ConsoleColor.Yellow;
                    Console.WriteLine(string.Format("[OK]   [{0}] Batch {1} Processed: {2} Synced, {3} Failed",
                        DateTime.Now.ToString("HH:mm:ss"), batchRequest.BatchId, syncedCount, failedCount));
                    Console.ResetColor();

                    await WriteJsonResponseAsync(res, result, 200);
                    return;
                }

                // ROUTE NOT FOUND
                await WriteJsonResponseAsync(res, new { Error = "Endpoint not found: " + path }, 404);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] Request processing failed: " + ex.ToString());
                Console.ResetColor();
                await WriteJsonResponseAsync(res, new { Error = "Internal server error: " + ex.Message }, 500);
            }
        }

        private static async Task WriteJsonResponseAsync(HttpListenerResponse res, object data, int statusCode)
        {
            res.StatusCode = statusCode;
            res.ContentType = "application/json; charset=utf-8";
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            res.ContentLength64 = bytes.Length;
            await res.OutputStream.WriteAsync(bytes, 0, bytes.Length);
            res.OutputStream.Close();
        }
    }
}
