namespace AuthTenant.Domain.Entities;

/// <summary>
/// Root tenant entity - each business using the platform.
/// Maps to: auth.tenants
/// </summary>
public class Tenant
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string Status { get; private set; } = default!;
    public string? Settings { get; private set; }  // JSON configuration
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation properties
    public ICollection<UserTenant> UserTenants { get; private set; } = new List<UserTenant>();
    public ICollection<Role> Roles { get; private set; } = new List<Role>();

    private Tenant() { } // EF Core constructor

    public static Tenant Create(string name, string slug, string? contactEmail = null)
    {
        var settings = contactEmail != null
            ? $"{{\"contactEmail\":\"{contactEmail}\"}}"
            : null;

        return new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug.ToLowerInvariant().Replace(" ", "-"),
            Status = TenantStatus.Active,
            Settings = settings,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Suspend()
    {
        Status = TenantStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = TenantStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        Status = TenantStatus.Deleted;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSettings(string settings)
    {
        Settings = settings;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Tenant status constants matching schema CHECK constraint.
/// </summary>
public static class TenantStatus
{
    public const string Active = "active";
    public const string Suspended = "suspended";
    public const string Deleted = "deleted";
}
