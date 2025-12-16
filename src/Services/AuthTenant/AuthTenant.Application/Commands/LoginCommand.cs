namespace AuthTenant.Application.Commands;

/// <summary>
/// Command to authenticate a user.
/// </summary>
public record LoginCommand(Guid TenantId, string Email, string Password);

/// <summary>
/// Result of successful login.
/// </summary>
public record LoginResult(string AccessToken, DateTime ExpiresAt);
