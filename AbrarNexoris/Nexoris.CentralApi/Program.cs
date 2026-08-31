using Newtonsoft.Json;
using Nexoris.CentralApi.Logging;
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
            FileLogger.Info("===============================================================");
            FileLogger.Info("        NEXORIS HEAD OFFICE CENTRAL API SERVER (.NET 4.6.1)   ");
            FileLogger.Info("===============================================================");

            _syncService = new CentralSyncService();

            string port = ConfigurationManager.AppSettings["ListeningPort"] ?? "5000";
            _listener = new HttpListener();
            _listener.Prefixes.Add(string.Format("http://localhost:{0}/", port));

            try
            {
                _listener.Start();
                FileLogger.Success("Central API Server listening on http://localhost:{0}/", port);
                FileLogger.Info("Endpoints available:");
                FileLogger.Info("     - GET  /api/v1/health");
                FileLogger.Info("     - GET  /api/v1/sync/branch-status");
                FileLogger.Info("     - POST /api/v1/sync/master-data");
                FileLogger.Info("     - POST /api/v1/sync/transactions");
                Console.WriteLine("\nPress Ctrl+C to stop the server...\n");
            }
            catch (Exception ex)
            {
                FileLogger.Error(ex, "Failed to start HttpListener");
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

        private static long GetMaxRequestBodySize()
        {
            if (long.TryParse(ConfigurationManager.AppSettings["MaxRequestBodySizeBytes"], out long size) && size > 0)
            {
                return size;
            }
            return 20 * 1024 * 1024; // Default: 20 MB
        }

        private static void ApplyCorsHeaders(HttpListenerRequest req, HttpListenerResponse res)
        {
            string origin = req.Headers["Origin"];
            if (string.IsNullOrEmpty(origin))
            {
                return;
            }

            string allowedOriginsSetting = ConfigurationManager.AppSettings["AllowedOrigins"] ?? "http://localhost:5000,http://127.0.0.1:5000";
            var allowedOrigins = allowedOriginsSetting.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            bool isAllowed = false;
            foreach (var rawOrigin in allowedOrigins)
            {
                string allowed = rawOrigin.Trim();
                if (allowed == "*" || string.Equals(origin, allowed, StringComparison.OrdinalIgnoreCase))
                {
                    isAllowed = true;
                    break;
                }
                if (allowed.EndsWith("*") && origin.StartsWith(allowed.TrimEnd('*'), StringComparison.OrdinalIgnoreCase))
                {
                    isAllowed = true;
                    break;
                }
            }

            if (isAllowed)
            {
                res.Headers.Add("Access-Control-Allow-Origin", origin);
                res.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                res.Headers.Add("Access-Control-Allow-Headers", "Content-Type, X-Branch-Id, X-Api-Key");
                res.Headers.Add("Access-Control-Max-Age", "86400");
            }
        }

        private static async Task<string> ReadRequestBodyWithLimitAsync(HttpListenerRequest req, long maxBytes)
        {
            if (req.ContentLength64 > maxBytes)
            {
                return null;
            }

            using (var ms = new MemoryStream())
            {
                var buffer = new byte[81920]; // 80 KB chunk
                long totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = await req.InputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    totalBytesRead += bytesRead;
                    if (totalBytesRead > maxBytes)
                    {
                        return null;
                    }
                    await ms.WriteAsync(buffer, 0, bytesRead);
                }

                var encoding = req.ContentEncoding ?? Encoding.UTF8;
                return encoding.GetString(ms.ToArray());
            }
        }

        private static async Task HandleRequestAsync(HttpListenerContext context)
        {
            var req = context.Request;
            var res = context.Response;

            // Restrict and apply CORS headers safely based on validated Origin
            ApplyCorsHeaders(req, res);

            if (req.HttpMethod == "OPTIONS")
            {
                res.StatusCode = 200;
                res.Close();
                return;
            }

            string rawUrl = req.RawUrl?.ToLowerInvariant() ?? "";
            string path = req.Url.AbsolutePath.ToLowerInvariant();
            long maxBodySize = GetMaxRequestBodySize();

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

                    string requestBody = await ReadRequestBodyWithLimitAsync(req, maxBodySize);
                    if (requestBody == null)
                    {
                        await WriteJsonResponseAsync(res, new { Error = string.Format("Payload too large. Maximum allowed size is {0} MB.", maxBodySize / (1024 * 1024)) }, 413);
                        return;
                    }

                    var masterRequest = JsonConvert.DeserializeObject<MasterDataSyncRequest>(requestBody);
                    if (masterRequest == null || masterRequest.PriceSettings == null)
                    {
                        await WriteJsonResponseAsync(res, new { Error = "Invalid JSON payload." }, 400);
                        return;
                    }

                    FileLogger.Info("[ONBOARDING] Received Master Data payload from Branch {0} ({1} items)...",
                        branchId, masterRequest.PriceSettings.Count);

                    var masterResult = await _syncService.IngestMasterDataAsync(masterRequest);

                    FileLogger.Success("Master Data Synced: {0} items saved for Branch {1}.",
                        masterResult.SyncedItemCount, branchId);

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
                        FileLogger.Warn("Unauthorized attempt - Missing X-Branch-Id or X-Api-Key headers");
                        await WriteJsonResponseAsync(res, new { Error = "Missing or invalid X-Branch-Id or X-Api-Key headers." }, 401);
                        return;
                    }

                    bool isValidKey = await _syncService.ValidateBranchKeyAsync(branchId, apiKey);
                    if (!isValidKey)
                    {
                        FileLogger.Warn("Forbidden attempt - Invalid API Key for Branch {0}", branchId);
                        await WriteJsonResponseAsync(res, new { Error = "Forbidden: Invalid API key for branch." }, 403);
                        return;
                    }

                    // Read JSON payload with size limit check
                    string requestBody = await ReadRequestBodyWithLimitAsync(req, maxBodySize);
                    if (requestBody == null)
                    {
                        await WriteJsonResponseAsync(res, new { Error = string.Format("Payload too large. Maximum allowed size is {0} MB.", maxBodySize / (1024 * 1024)) }, 413);
                        return;
                    }

                    var batchRequest = JsonConvert.DeserializeObject<BatchSyncRequest>(requestBody);
                    if (batchRequest == null || batchRequest.Transactions == null)
                    {
                        await WriteJsonResponseAsync(res, new { Error = "Invalid JSON payload." }, 400);
                        return;
                    }

                    FileLogger.Info("Received Batch {0} from Branch {1} with {2} transactions...",
                        batchRequest.BatchId, branchId, batchRequest.Transactions.Count);

                    var result = await _syncService.ProcessBatchAsync(batchRequest);

                    int syncedCount = 0;
                    int failedCount = 0;
                    foreach (var r in result.Results)
                    {
                        if (string.Equals(r.Status, "Synced", StringComparison.OrdinalIgnoreCase) || 
                            string.Equals(r.Status, "AlreadySynced", StringComparison.OrdinalIgnoreCase)) 
                        {
                            syncedCount++;
                        }
                        else 
                        {
                            failedCount++;
                        }
                    }

                    if (failedCount == 0)
                    {
                        FileLogger.Success("Batch {0} Processed: {1} Synced, {2} Failed",
                            batchRequest.BatchId, syncedCount, failedCount);
                    }
                    else
                    {
                        FileLogger.Warn("Batch {0} Processed: {1} Synced, {2} Failed",
                            batchRequest.BatchId, syncedCount, failedCount);
                    }

                    await WriteJsonResponseAsync(res, result, 200);
                    return;
                }

                // ROUTE NOT FOUND
                await WriteJsonResponseAsync(res, new { Error = "Endpoint not found: " + path }, 404);
            }
            catch (Exception ex)
            {
                // Full internal exception logged safely to file and console on server side
                FileLogger.Error(ex, "Request processing failed ({0})", path);

                // Sanitize user-facing error response to prevent leaking internal database / server details
                await WriteJsonResponseAsync(res, new { Error = "An internal server error occurred while processing the request." }, 500);
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
