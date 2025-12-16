namespace AuthTenant.Domain.Entities;

/// <summary>
/// Represents a role for RBAC.
/// </summary>
public class Role
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsSystemRole { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public Tenant Tenant { get; private set; } = default!;
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    private Role() { } // EF Core constructor

    public static Role Create(Guid tenantId, string name, string? description = null, bool isSystemRole = false)
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Description = description,
            IsSystemRole = isSystemRole,
            CreatedAt = DateTime.UtcNow
        };
    }
}
