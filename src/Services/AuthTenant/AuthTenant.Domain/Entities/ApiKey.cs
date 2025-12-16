namespace AuthTenant.Domain.Entities;

/// <summary>
/// Represents an API key for tenant access.
/// </summary>
public class ApiKey
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = default!;
    public string KeyHash { get; private set; } = default!;
    public string? KeyPrefix { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? LastUsedAt { get; private set; }

    // Navigation properties
    public Tenant Tenant { get; private set; } = default!;

    private ApiKey() { } // EF Core constructor

    public static ApiKey Create(Guid tenantId, string name, string keyHash, string keyPrefix, DateTime? expiresAt = null)
    {
        return new ApiKey
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            KeyHash = keyHash,
            KeyPrefix = keyPrefix,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt
        };
    }
}
