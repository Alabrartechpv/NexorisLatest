using Newtonsoft.Json;
using Nexoris.SyncService.Configuration;
using Nexoris.SyncService.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Nexoris.SyncService.Services
{
    public class CentralApiClient : ICentralApiClient
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly SyncSettings _settings;

        public CentralApiClient(SyncSettings settings)
        {
            _settings = settings;
        }

        public async Task<bool> CheckCentralHealthAsync()
        {
            try
            {
                string url = _settings.CentralApiUrl.TrimEnd('/') + "/api/v1/health";
                var response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(string.Format("[WARN] Health check failed to Head Office API ({0}): {1}", _settings.CentralApiUrl, ex.Message));
                Console.ResetColor();
                return false;
            }
        }

        public async Task<BatchSyncResponse> SendBatchAsync(BatchSyncRequest request)
        {
            try
            {
                string url = _settings.CentralApiUrl.TrimEnd('/') + "/api/v1/sync/transactions";
                using (var msg = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    msg.Headers.Add("X-Branch-Id", _settings.BranchId.ToString());
                    msg.Headers.Add("X-Api-Key", _settings.ApiKey);

                    string json = JsonConvert.SerializeObject(request);
                    msg.Content = new StringContent(json, Encoding.UTF8, "application/json");

                    using (var response = await _httpClient.SendAsync(msg))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            string errContent = await response.Content.ReadAsStringAsync();
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(string.Format("[ERROR] Central API returned error ({0}): {1}", response.StatusCode, errContent));
                            Console.ResetColor();
                            return null;
                        }

                        string resultJson = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<BatchSyncResponse>(resultJson);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(string.Format("[ERROR] Failed to transmit batch {0} to Head Office: {1}", request.BatchId, ex.Message));
                Console.ResetColor();
                return null;
            }
        }
    }
}
