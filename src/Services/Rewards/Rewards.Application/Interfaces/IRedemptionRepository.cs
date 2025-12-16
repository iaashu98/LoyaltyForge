using Rewards.Domain.Entities;

namespace Rewards.Application.Interfaces;

/// <summary>
/// Repository interface for Redemption operations.
/// </summary>
public interface IRedemptionRepository
{
    Task<Redemption?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Redemption?> GetByIdempotencyKeyAsync(Guid tenantId, string idempotencyKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Redemption>> GetByCustomerAsync(Guid tenantId, Guid customerId, CancellationToken cancellationToken = default);
    Task AddAsync(Redemption redemption, CancellationToken cancellationToken = default);
    Task UpdateAsync(Redemption redemption, CancellationToken cancellationToken = default);
}
