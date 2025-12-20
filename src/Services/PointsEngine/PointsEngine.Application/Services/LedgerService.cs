using PointsEngine.Application.Interfaces;
using PointsEngine.Domain.Entities;

namespace PointsEngine.Application.Services;

/// <summary>
/// Application service for points ledger operations.
/// Handles point earning, deduction, and transaction history.
/// </summary>
public class LedgerService : ILedgerService
{
    private readonly ILedgerRepository _ledgerRepository;
    private readonly IUserBalanceRepository _balanceRepository;

    public LedgerService(ILedgerRepository ledgerRepository, IUserBalanceRepository balanceRepository)
    {
        _ledgerRepository = ledgerRepository;
        _balanceRepository = balanceRepository;
    }

    public Task<LedgerResult> EarnPointsAsync(EarnPointsCommand command, CancellationToken cancellationToken = default)
    {
        // TODO: Implement earn points logic
        // 1. Check idempotency key to prevent duplicates
        // 2. Get current balance
        // 3. Create ledger entry with balance_after
        // 4. Update user_balances
        // 5. All in a transaction

        throw new NotImplementedException("Business logic to be implemented");
    }

    public Task<LedgerResult> DeductPointsAsync(DeductPointsCommand command, CancellationToken cancellationToken = default)
    {
        // TODO: Implement deduct points logic
        // 1. Check idempotency key
        // 2. Verify sufficient balance
        // 3. Create ledger entry (negative points_amount)
        // 4. Update user_balances
        // 5. All in a transaction

        throw new NotImplementedException("Business logic to be implemented");
    }

    public Task<LedgerResult> ReversePointsAsync(ReversePointsCommand command, CancellationToken cancellationToken = default)
    {
        // TODO: Implement reversal logic
        // 1. Find original ledger entry
        // 2. Create opposite entry (refund)
        // 3. Update balance
        // Remember: Ledger is immutable - never update existing entries

        throw new NotImplementedException("Business logic to be implemented");
    }

    public async Task<IReadOnlyList<LedgerEntry>> GetTransactionHistoryAsync(
        Guid tenantId,
        Guid userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return await _ledgerRepository.GetByUserAsync(tenantId, userId, page, pageSize, cancellationToken);
    }

    public Task<int> ExpirePointsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement point expiration
        // 1. Find all earn entries with expires_at < now
        // 2. Calculate unexpired balance per entry
        // 3. Create expire ledger entries
        // 4. Update balances
        // This should be called by a background job

        throw new NotImplementedException("Business logic to be implemented");
    }
}
