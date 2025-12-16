namespace AuthTenant.Domain.Entities;

/// <summary>
/// Represents a tenant in the multi-tenant system.
/// </summary>
public class Tenant
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string ContactEmail { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public ICollection<User> Users { get; private set; } = new List<User>();
    public ICollection<ApiKey> ApiKeys { get; private set; } = new List<ApiKey>();

    private Tenant() { } // EF Core constructor

    public static Tenant Create(string name, string contactEmail)
    {
        // TODO: Add validation logic
        return new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name,
            ContactEmail = contactEmail,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}
