using Microsoft.AspNetCore.Http;

namespace LoyaltyForge.Common.Middleware;

/// <summary>
/// Base class for tenant resolution middleware.
/// Implementations should extract tenant from JWT claims or API key.
/// </summary>
public abstract class TenantResolutionMiddlewareBase
{
    protected readonly RequestDelegate _next;

    protected TenantResolutionMiddlewareBase(RequestDelegate next)
    {
        _next = next;
    }

    public abstract Task InvokeAsync(HttpContext context);
}
