using Rewards.Domain.Entities;

namespace Rewards.Application.Interfaces;

/// <summary>
/// Repository interface for Reward operations.
/// </summary>
public interface IRewardRepository
{
    Task<Reward?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Reward>> GetByTenantAsync(Guid tenantId, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task AddAsync(Reward reward, CancellationToken cancellationToken = default);
    Task UpdateAsync(Reward reward, CancellationToken cancellationToken = default);
    Task DeleteAsync(Reward reward, CancellationToken cancellationToken = default);
}
