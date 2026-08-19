using Microsoft.AspNetCore.Mvc;
using Nexoris.CentralApi.Models.DTOs;
using Nexoris.CentralApi.Services;

namespace Nexoris.CentralApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SyncController : ControllerBase
    {
        private readonly ICentralSyncService _syncService;
        private readonly ILogger<SyncController> _logger;

        public SyncController(ICentralSyncService syncService, ILogger<SyncController> logger)
        {
            _syncService = syncService;
            _logger = logger;
        }

        [HttpPost("transactions")]
        public async Task<IActionResult> IngestTransactions(
            [FromHeader(Name = "X-Branch-Id")] int branchId,
            [FromHeader(Name = "X-Api-Key")] string apiKey,
            [FromBody] BatchSyncRequest request)
        {
            if (request == null || request.Transactions == null || request.Transactions.Count == 0)
            {
                return BadRequest(new { Error = "Batch contains no transactions to process." });
            }

            int authenticatedBranchId = HttpContext.Items.TryGetValue("BranchId", out var bId) ? (int)bId : (branchId > 0 ? branchId : request.BranchId);
            request.BranchId = authenticatedBranchId;

            _logger.LogInformation("Received batch {BatchId} with {Count} transactions from Branch {BranchId}",
                request.BatchId, request.Transactions.Count, authenticatedBranchId);

            var result = await _syncService.ProcessBatchAsync(request);

            return Ok(result);
        }
    }
}
