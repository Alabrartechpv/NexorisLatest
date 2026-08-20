namespace Nexoris.SyncService.Configuration
{
    public class SyncSettings
    {
        public int BranchId { get; set; } = 1;
        public string ApiKey { get; set; } = "MNG_SECURE_API_KEY_2026";
        public string CentralApiUrl { get; set; } = "http://localhost:5000";
        public int PollIntervalSeconds { get; set; } = 5;
        public int BatchSize { get; set; } = 20;
        public int MaxRetries { get; set; } = 10;
    }
}
