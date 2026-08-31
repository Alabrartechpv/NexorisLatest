using Newtonsoft.Json;
using Nexoris.SyncService.Configuration;
using Nexoris.SyncService.Logging;
using Nexoris.SyncService.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Nexoris.SyncService.Services
{
    public class CentralApiClient : ICentralApiClient
    {
        private static readonly HttpClient _httpClient;
        private readonly SyncSettings _settings;

        static CentralApiClient()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

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
            catch (TaskCanceledException)
            {
                FileLogger.Warn("Health check timed out (30s limit) to Head Office API ({0})", _settings.CentralApiUrl);
                return false;
            }
            catch (Exception ex)
            {
                FileLogger.Warn("Health check failed to Head Office API ({0}): {1}", _settings.CentralApiUrl, ex.Message);
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
                            FileLogger.Error("Central API returned error ({0}): {1}", response.StatusCode, errContent);
                            return null;
                        }

                        string resultJson = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<BatchSyncResponse>(resultJson);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                FileLogger.Error("Transmission timed out after 30s for Batch {0} to Head Office.", request.BatchId);
                return null;
            }
            catch (Exception ex)
            {
                FileLogger.Error(ex, "Failed to transmit batch {0} to Head Office", request.BatchId);
                return null;
            }
        }

        public async Task<BranchStatusResponse> GetBranchStatusAsync(int branchId)
        {
            try
            {
                string url = _settings.CentralApiUrl.TrimEnd('/') + "/api/v1/sync/branch-status";
                using (var msg = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    msg.Headers.Add("X-Branch-Id", branchId.ToString());
                    msg.Headers.Add("X-Api-Key", _settings.ApiKey);

                    using (var response = await _httpClient.SendAsync(msg))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string json = await response.Content.ReadAsStringAsync();
                            return JsonConvert.DeserializeObject<BranchStatusResponse>(json);
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                FileLogger.Warn("Branch status check timed out after 30s from Central API");
            }
            catch (Exception ex)
            {
                FileLogger.Warn("Failed to fetch branch status from Central API: {0}", ex.Message);
            }
            return null;
        }

        public async Task<MasterDataSyncResponse> PushMasterDataAsync(MasterDataSyncRequest request)
        {
            try
            {
                string url = _settings.CentralApiUrl.TrimEnd('/') + "/api/v1/sync/master-data";
                using (var msg = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    msg.Headers.Add("X-Branch-Id", request.BranchId.ToString());
                    msg.Headers.Add("X-Api-Key", _settings.ApiKey);

                    string json = JsonConvert.SerializeObject(request);
                    msg.Content = new StringContent(json, Encoding.UTF8, "application/json");

                    using (var response = await _httpClient.SendAsync(msg))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string resultJson = await response.Content.ReadAsStringAsync();
                            return JsonConvert.DeserializeObject<MasterDataSyncResponse>(resultJson);
                        }
                        else
                        {
                            string err = await response.Content.ReadAsStringAsync();
                            FileLogger.Error("Master data sync failed ({0}): {1}", response.StatusCode, err);
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                FileLogger.Error("Master data push timed out after 30s to Central API.");
            }
            catch (Exception ex)
            {
                FileLogger.Error(ex, "Failed to push master data to Central API");
            }
            return null;
        }
    }
}
