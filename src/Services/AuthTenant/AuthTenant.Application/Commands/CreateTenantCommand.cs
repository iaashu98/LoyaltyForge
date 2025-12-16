namespace AuthTenant.Application.Commands;

/// <summary>
/// Command to create a new tenant.
/// </summary>
public record CreateTenantCommand(string Name, string ContactEmail);

/// <summary>
/// Result of tenant creation.
/// </summary>
public record CreateTenantResult(Guid TenantId, string Name);
