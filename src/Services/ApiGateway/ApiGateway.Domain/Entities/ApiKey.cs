namespace ApiGateway.Domain.Entities;

/// <summary>
/// API keys for external service authentication - scoped per tenant.
/// Maps to: gateway.api_keys
/// </summary>
public class ApiKey
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = default!;
    public string KeyPrefix { get; private set; } = default!;
    public string KeyHash { get; private set; } = default!;
    public string Scopes { get; private set; } = default!;  // JSON array
    public int RateLimitPerMinute { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? LastUsedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public Guid? RevokedBy { get; private set; }

    private ApiKey() { } // EF Core constructor

    public static ApiKey Create(
        Guid tenantId,
        string name,
        string keyHash,
        string keyPrefix,
        string[] scopes,
        int rateLimitPerMinute = 60,
        DateTime? expiresAt = null,
        Guid? createdBy = null)
    {
        return new ApiKey
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            KeyHash = keyHash,
            KeyPrefix = keyPrefix,
            Scopes = System.Text.Json.JsonSerializer.Serialize(scopes),
            RateLimitPerMinute = rateLimitPerMinute,
            IsActive = true,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public void UpdateLastUsed()
    {
        LastUsedAt = DateTime.UtcNow;
    }

    public void Revoke(Guid? revokedBy = null)
    {
        IsActive = false;
        RevokedAt = DateTime.UtcNow;
        RevokedBy = revokedBy;
    }

    public string[] GetScopes()
    {
        return System.Text.Json.JsonSerializer.Deserialize<string[]>(Scopes) ?? Array.Empty<string>();
    }
}
