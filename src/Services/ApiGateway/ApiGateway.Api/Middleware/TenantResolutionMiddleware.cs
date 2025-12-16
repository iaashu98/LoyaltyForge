namespace ApiGateway.Api.Middleware;

/// <summary>
/// Middleware to extract and validate tenant context from JWT claims.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip tenant resolution for health checks
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        // TODO: Extract tenant from JWT claims or API key
        // 1. Check for JWT token
        // 2. Extract tenant_id claim
        // 3. Set tenant context for downstream services
        
        var tenantId = context.User.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrEmpty(tenantId))
        {
            context.Items["TenantId"] = Guid.Parse(tenantId);
            _logger.LogDebug("Tenant resolved: {TenantId}", tenantId);
        }

        await _next(context);
    }
}
