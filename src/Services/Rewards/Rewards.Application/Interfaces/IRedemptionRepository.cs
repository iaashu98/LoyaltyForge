using Rewards.Domain.Entities;

namespace Rewards.Application.Interfaces;

/// <summary>
/// Repository interface for Redemption operations.
/// </summary>
public interface IRedemptionRepository
{
    Task<RewardRedemption?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RewardRedemption?> GetByIdempotencyKeyAsync(Guid tenantId, string idempotencyKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RewardRedemption>> GetByCustomerAsync(Guid tenantId, Guid customerId, CancellationToken cancellationToken = default);
    Task AddAsync(RewardRedemption redemption, CancellationToken cancellationToken = default);
    Task UpdateAsync(RewardRedemption redemption, CancellationToken cancellationToken = default);
}
