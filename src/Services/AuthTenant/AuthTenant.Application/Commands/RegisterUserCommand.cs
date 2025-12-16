namespace AuthTenant.Application.Commands;

/// <summary>
/// Command to register a new user within a tenant.
/// </summary>
public record RegisterUserCommand(
    Guid TenantId,
    string Email,
    string Password,
    string FirstName,
    string LastName);

/// <summary>
/// Result of user registration.
/// </summary>
public record RegisterUserResult(Guid UserId, string Email);
