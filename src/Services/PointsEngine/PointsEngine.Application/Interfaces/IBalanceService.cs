using PointsEngine.Domain.Entities;

namespace PointsEngine.Application.Interfaces;

/// <summary>
/// Application service for points balance queries.
/// Read-only operations on cached balances.
/// </summary>
public interface IBalanceService
{
    /// <summary>
    /// Gets the current balance for a customer.
    /// </summary>
    Task<BalanceResult> GetBalanceAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a customer has sufficient points for a redemption.
    /// </summary>
    Task<bool> HasSufficientPointsAsync(Guid tenantId, Guid userId, long requiredPoints, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recalculates balance from ledger entries.
    /// Used for consistency checks or recovery.
    /// </summary>
    Task<BalanceResult> RecalculateBalanceAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}

// Results
public record BalanceResult(
    Guid UserId,
    long AvailablePoints,
    long PendingPoints,
    long LifetimeEarned,
    long LifetimeRedeemed,
    DateTime LastUpdatedAt);
