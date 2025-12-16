using AuthTenant.Domain.Entities;

namespace AuthTenant.Application.Interfaces;

/// <summary>
/// Service interface for JWT token operations.
/// </summary>
public interface IJwtService
{
    string GenerateToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
    bool ValidateToken(string token, out Guid tenantId, out Guid userId);
}
