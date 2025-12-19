namespace AuthTenant.Domain.Entities;

/// <summary>
/// Assigns roles to users within their tenant context.
/// Maps to: auth.user_roles
/// </summary>
public class UserRole
{
    public Guid Id { get; private set; }
    public Guid UserTenantId { get; private set; }
    public Guid RoleId { get; private set; }
    public DateTime GrantedAt { get; private set; }
    public Guid? GrantedBy { get; private set; }  // Soft reference to user who granted

    // Navigation properties
    public UserTenant UserTenant { get; private set; } = default!;
    public Role Role { get; private set; } = default!;

    private UserRole() { } // EF Core constructor

    public static UserRole Create(Guid userTenantId, Guid roleId, Guid? grantedBy = null)
    {
        return new UserRole
        {
            Id = Guid.NewGuid(),
            UserTenantId = userTenantId,
            RoleId = roleId,
            GrantedAt = DateTime.UtcNow,
            GrantedBy = grantedBy
        };
    }
}
