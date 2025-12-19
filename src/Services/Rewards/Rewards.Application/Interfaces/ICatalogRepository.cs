using Rewards.Domain.Entities;

namespace Rewards.Application.Interfaces;

/// <summary>
/// Repository interface for CatalogItem (reward) operations.
/// </summary>
public interface ICatalogRepository
{
    Task<CatalogItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CatalogItem>> GetByTenantAsync(Guid tenantId, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task AddAsync(CatalogItem item, CancellationToken cancellationToken = default);
    Task UpdateAsync(CatalogItem item, CancellationToken cancellationToken = default);
}
