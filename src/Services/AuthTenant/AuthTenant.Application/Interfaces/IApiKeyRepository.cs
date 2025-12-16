using AuthTenant.Domain.Entities;

namespace AuthTenant.Application.Interfaces;

/// <summary>
/// Repository interface for ApiKey operations.
/// </summary>
public interface IApiKeyRepository
{
    Task<ApiKey?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiKey?> GetByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApiKey>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(ApiKey apiKey, CancellationToken cancellationToken = default);
    Task UpdateAsync(ApiKey apiKey, CancellationToken cancellationToken = default);
    Task DeleteAsync(ApiKey apiKey, CancellationToken cancellationToken = default);
}
