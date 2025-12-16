namespace AuthTenant.Domain.Entities;

/// <summary>
/// Represents a permission in the RBAC system.
/// </summary>
public class Permission
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public string Resource { get; private set; } = default!;
    public string Action { get; private set; } = default!;

    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    private Permission() { } // EF Core constructor

    public static Permission Create(string name, string resource, string action, string? description = null)
    {
        return new Permission
        {
            Id = Guid.NewGuid(),
            Name = name,
            Resource = resource,
            Action = action,
            Description = description
        };
    }
}
