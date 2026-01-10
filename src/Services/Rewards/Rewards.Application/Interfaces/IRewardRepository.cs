using Rewards.Domain.Entities;

namespace Rewards.Application.Interfaces;

/// <summary>
/// Repository interface for RewardCatalog (reward) operations.
/// </summary>
public interface IRewardRepository
{
    Task<RewardCatalog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RewardCatalog>> GetAllByTenantAsync(Guid tenantId, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task AddAsync(RewardCatalog reward, CancellationToken cancellationToken = default);
    Task UpdateAsync(RewardCatalog reward, CancellationToken cancellationToken = default);
    Task DeleteAsync(RewardCatalog reward, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
