using Microsoft.AspNetCore.Mvc;
using Nexoris.CentralApi.Services;

namespace Nexoris.CentralApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ICentralSyncService _syncService;

        public HealthController(ICentralSyncService syncService)
        {
            _syncService = syncService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            bool isDbHealthy = await _syncService.CheckDatabaseHealthAsync();

            return Ok(new
            {
                Status = isDbHealthy ? "Healthy" : "Degraded",
                DatabaseConnected = isDbHealthy,
                ServerUtc = DateTime.UtcNow,
                Version = "1.0.0",
                Service = "Nexoris Central Ingestion API"
            });
        }
    }
}
