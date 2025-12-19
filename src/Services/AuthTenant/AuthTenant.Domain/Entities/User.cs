namespace AuthTenant.Domain.Entities;

/// <summary>
/// All users in the system - email unique per provider, not globally.
/// Maps to: auth.users
/// </summary>
public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = default!;
    public string? PasswordHash { get; private set; }
    public string? ExternalId { get; private set; }
    public string Provider { get; private set; } = default!;
    public bool EmailVerified { get; private set; }
    public string Status { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation properties
    public ICollection<UserTenant> UserTenants { get; private set; } = new List<UserTenant>();

    private User() { } // EF Core constructor

    /// <summary>
    /// Create a local user with email/password.
    /// </summary>
    public static User CreateLocal(string email, string passwordHash)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            Provider = AuthProvider.Local,
            EmailVerified = false,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create an external user (OAuth, Shopify, etc).
    /// </summary>
    public static User CreateExternal(string email, string externalId, string provider)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            ExternalId = externalId,
            Provider = provider,
            EmailVerified = true, // External providers typically verify email
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void VerifyEmail()
    {
        EmailVerified = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Suspend()
    {
        Status = UserStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = UserStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// User status constants matching schema CHECK constraint.
/// </summary>
public static class UserStatus
{
    public const string Active = "active";
    public const string Suspended = "suspended";
    public const string Deleted = "deleted";
}

/// <summary>
/// Auth provider constants.
/// </summary>
public static class AuthProvider
{
    public const string Local = "local";
    public const string Google = "google";
    public const string Shopify = "shopify";
}
