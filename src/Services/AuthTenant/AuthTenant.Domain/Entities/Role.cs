namespace AuthTenant.Domain.Entities;

/// <summary>
/// Role definitions - system-wide or tenant-specific.
/// Permissions stored as JSON array.
/// Maps to: auth.roles
/// </summary>
public class Role
{
    public Guid Id { get; private set; }
    public Guid? TenantId { get; private set; }  // NULL for system roles
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public string Permissions { get; private set; } = default!;  // JSON array of permission strings
    public bool IsSystemRole { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public Tenant? Tenant { get; private set; }
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    private Role() { } // EF Core constructor

    public static Role CreateTenantRole(
        Guid tenantId,
        string name,
        string[] permissions,
        string? description = null)
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Description = description,
            Permissions = System.Text.Json.JsonSerializer.Serialize(permissions),
            IsSystemRole = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Role CreateSystemRole(
        string name,
        string[] permissions,
        string? description = null)
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            TenantId = null,
            Name = name,
            Description = description,
            Permissions = System.Text.Json.JsonSerializer.Serialize(permissions),
            IsSystemRole = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public string[] GetPermissions()
    {
        return System.Text.Json.JsonSerializer.Deserialize<string[]>(Permissions) ?? Array.Empty<string>();
    }

    public void UpdatePermissions(string[] permissions)
    {
        Permissions = System.Text.Json.JsonSerializer.Serialize(permissions);
    }
}
