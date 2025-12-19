namespace AuthTenant.Application.Commands;

/// <summary>
/// Command to register a new user within a tenant.
/// Creates user if not exists, then creates user-tenant mapping.
/// </summary>
public record RegisterUserCommand(
    Guid TenantId,
    string Email,
    string Password,
    string Provider = "local",
    string UserType = "customer");

/// <summary>
/// Result of user registration.
/// </summary>
public record RegisterUserResult(Guid UserId, Guid UserTenantId, string Email);
