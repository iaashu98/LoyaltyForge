namespace AuthTenant.Domain.Entities;

/// <summary>
/// Maps users to tenants (many-to-many) with their role within each tenant.
/// Maps to: auth.user_tenants
/// </summary>
public class UserTenant
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid TenantId { get; private set; }
    public string UserType { get; private set; } = default!;
    public string? ExternalCustomerId { get; private set; }
    public string? Metadata { get; private set; }  // JSON for additional data
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public User User { get; private set; } = default!;
    public Tenant Tenant { get; private set; } = default!;
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    private UserTenant() { } // EF Core constructor

    public static UserTenant Create(
        Guid userId,
        Guid tenantId,
        string userType = UserTypes.Customer,
        string? externalCustomerId = null,
        string? metadata = null)
    {
        return new UserTenant
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            UserType = userType,
            ExternalCustomerId = externalCustomerId,
            Metadata = metadata,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateMetadata(string metadata)
    {
        Metadata = metadata;
    }

    public void SetExternalCustomerId(string externalCustomerId)
    {
        ExternalCustomerId = externalCustomerId;
    }
}

/// <summary>
/// User type constants matching schema CHECK constraint.
/// </summary>
public static class UserTypes
{
    public const string Admin = "admin";
    public const string Staff = "staff";
    public const string Customer = "customer";
}
