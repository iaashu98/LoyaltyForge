namespace AuthTenant.Domain.Entities;

/// <summary>
/// Join entity for Role-Permission many-to-many relationship.
/// </summary>
public class RolePermission
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }
    public DateTime AssignedAt { get; private set; }

    // Navigation properties
    public Role Role { get; private set; } = default!;
    public Permission Permission { get; private set; } = default!;

    private RolePermission() { } // EF Core constructor

    public static RolePermission Create(Guid roleId, Guid permissionId)
    {
        return new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            AssignedAt = DateTime.UtcNow
        };
    }
}
