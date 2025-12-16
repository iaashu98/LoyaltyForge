namespace AuthTenant.Application.Commands;

/// <summary>
/// Command to generate a new API key.
/// </summary>
public record GenerateApiKeyCommand(Guid TenantId, string Name, DateTime? ExpiresAt);

/// <summary>
/// Result of API key generation. Note: RawKey is only returned once.
/// </summary>
public record GenerateApiKeyResult(Guid ApiKeyId, string Name, string RawKey, string Prefix, DateTime? ExpiresAt);
