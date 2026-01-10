namespace AuthTenant.Application.Interfaces;


/// <summary>
/// Service for generating and validating JWT tokens.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a JWT token for authenticated user.
    /// </summary>
    /// <param name="userId">User's unique identifier</param>
    /// <param name="tenantId">Tenant's unique identifier</param>
    /// <param name="email">User's email address</param>
    /// <param name="roles">User's roles (optional)</param>
    /// <returns>JWT token string</returns>
    string GenerateToken(
        Guid userId,
        Guid tenantId,
        string email,
        IEnumerable<string>? roles = null);

    /// <summary>
    /// Validates a JWT token and extracts claims.
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>Validation result with extracted claims</returns>
    Task<TokenValidationResult> ValidateTokenAsync(string token);
}


/// <summary>
/// Result of token validation.
/// </summary>
public record TokenValidationResult(
    bool IsValid,
    Guid? UserId,
    Guid? TenantId,
    string? Email,
    string? Error);