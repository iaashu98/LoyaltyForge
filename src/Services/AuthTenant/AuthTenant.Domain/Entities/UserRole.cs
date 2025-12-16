namespace AuthTenant.Domain.Entities;

/// <summary>
/// Join entity for User-Role many-to-many relationship.
/// </summary>
public class UserRole
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public DateTime AssignedAt { get; private set; }

    // Navigation properties
    public User User { get; private set; } = default!;
    public Role Role { get; private set; } = default!;

    private UserRole() { } // EF Core constructor

    public static UserRole Create(Guid userId, Guid roleId)
    {
        return new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow
        };
    }
}
