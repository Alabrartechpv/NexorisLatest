using Nexoris.CentralApi.Services;

namespace Nexoris.CentralApi.Middleware
{
    public class BranchAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BranchAuthMiddleware> _logger;

        public BranchAuthMiddleware(RequestDelegate next, ILogger<BranchAuthMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ICentralSyncService syncService)
        {
            // Allow health check and swagger without authentication
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
            if (path.Contains("/health") || path.Contains("/swagger"))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue("X-Branch-Id", out var branchIdStr) ||
                !int.TryParse(branchIdStr, out int branchId) ||
                !context.Request.Headers.TryGetValue("X-Api-Key", out var apiKey))
            {
                _logger.LogWarning("Unauthorized access attempt missing required headers (X-Branch-Id, X-Api-Key)");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { Error = "Missing or invalid X-Branch-Id or X-Api-Key headers." });
                return;
            }

            bool isValid = await syncService.ValidateBranchKeyAsync(branchId, apiKey.ToString());
            if (!isValid)
            {
                _logger.LogWarning("Invalid credentials for BranchId={BranchId}", branchId);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { Error = "Forbidden: Invalid API key for branch." });
                return;
            }

            context.Items["BranchId"] = branchId;
            await _next(context);
        }
    }
}
