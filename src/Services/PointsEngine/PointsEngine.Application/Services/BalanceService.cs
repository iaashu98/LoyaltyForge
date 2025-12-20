using PointsEngine.Application.Interfaces;
using PointsEngine.Domain.Entities;

namespace PointsEngine.Application.Services;

/// <summary>
/// Application service for points balance queries.
/// </summary>
public class BalanceService : IBalanceService
{
    private readonly IUserBalanceRepository _balanceRepository;
    private readonly ILedgerRepository _ledgerRepository;

    public BalanceService(IUserBalanceRepository balanceRepository, ILedgerRepository ledgerRepository)
    {
        _balanceRepository = balanceRepository;
        _ledgerRepository = ledgerRepository;
    }

    public async Task<BalanceResult> GetBalanceAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        var balance = await _balanceRepository.GetByUserAsync(tenantId, userId, cancellationToken);

        if (balance == null)
        {
            // Return zero balance for users with no history
            return new BalanceResult(
                userId,
                AvailablePoints: 0,
                PendingPoints: 0,
                LifetimeEarned: 0,
                LifetimeRedeemed: 0,
                LastUpdatedAt: DateTime.UtcNow);
        }

        return new BalanceResult(
            balance.UserId,
            balance.AvailablePoints,
            balance.PendingPoints,
            balance.LifetimeEarned,
            balance.LifetimeRedeemed,
            balance.UpdatedAt);
    }

    public async Task<bool> HasSufficientPointsAsync(Guid tenantId, Guid userId, long requiredPoints, CancellationToken cancellationToken = default)
    {
        var balance = await _balanceRepository.GetByUserAsync(tenantId, userId, cancellationToken);
        return balance != null && balance.AvailablePoints >= requiredPoints;
    }

    public Task<BalanceResult> RecalculateBalanceAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        // TODO: Recalculate balance from ledger entries
        // 1. Sum all ledger entries for user
        // 2. Update user_balances table
        // 3. Return new balance
        // Used for consistency checks or recovery scenarios

        throw new NotImplementedException("Business logic to be implemented");
    }
}
