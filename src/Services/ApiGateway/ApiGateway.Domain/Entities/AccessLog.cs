using System.Net;

namespace ApiGateway.Domain.Entities;

/// <summary>
/// Minimal request logging for rate limiting - use external system for analytics.
/// Maps to: gateway.access_logs
/// </summary>
public class AccessLog
{
    public Guid Id { get; private set; }
    public Guid? TenantId { get; private set; }
    public Guid? ApiKeyId { get; private set; }
    public string Method { get; private set; } = default!;
    public string Path { get; private set; } = default!;
    public int StatusCode { get; private set; }
    public int? ResponseTimeMs { get; private set; }
    public IPAddress? ClientIp { get; private set; }
    public string? UserAgent { get; private set; }
    public int? RateLimitRemaining { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private AccessLog() { } // EF Core constructor

    public static AccessLog Create(
        string method,
        string path,
        int statusCode,
        int? responseTimeMs = null,
        Guid? tenantId = null,
        Guid? apiKeyId = null,
        IPAddress? clientIp = null,
        string? userAgent = null,
        int? rateLimitRemaining = null)
    {
        return new AccessLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ApiKeyId = apiKeyId,
            Method = method,
            Path = path,
            StatusCode = statusCode,
            ResponseTimeMs = responseTimeMs,
            ClientIp = clientIp,
            UserAgent = userAgent,
            RateLimitRemaining = rateLimitRemaining,
            CreatedAt = DateTime.UtcNow
        };
    }
}
