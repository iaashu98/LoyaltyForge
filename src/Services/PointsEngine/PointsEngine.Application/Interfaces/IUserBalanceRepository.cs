using PointsEngine.Domain.Entities;

namespace PointsEngine.Application.Interfaces;

/// <summary>
/// Repository interface for user balance operations.
/// </summary>
public interface IUserBalanceRepository
{
    Task<UserBalance?> GetByUserAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<UserBalance> GetOrCreateAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(UserBalance balance, CancellationToken cancellationToken = default);
}
