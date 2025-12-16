namespace AuthTenant.Domain.Entities;

/// <summary>
/// Represents a user within a tenant.
/// </summary>
public class User
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    // Navigation properties
    public Tenant Tenant { get; private set; } = default!;
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    private User() { } // EF Core constructor

    public static User Create(Guid tenantId, string email, string passwordHash, string firstName, string lastName)
    {
        // TODO: Add validation logic
        return new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = email,
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}
