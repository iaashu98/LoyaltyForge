using EcommerceIntegration.Domain.Entities;

namespace EcommerceIntegration.Application.Interfaces;

/// <summary>
/// Repository interface for IntegrationConfig operations.
/// </summary>
public interface IIntegrationConfigRepository
{
    Task<IntegrationConfig?> GetByTenantAndPlatformAsync(
        Guid tenantId, 
        string platform, 
        CancellationToken cancellationToken = default);
    
    Task AddAsync(IntegrationConfig config, CancellationToken cancellationToken = default);
    Task UpdateAsync(IntegrationConfig config, CancellationToken cancellationToken = default);
}
