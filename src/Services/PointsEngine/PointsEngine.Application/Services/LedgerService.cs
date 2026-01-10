using PointsEngine.Application.Interfaces;
using PointsEngine.Domain.Entities;
using LoyaltyForge.Common.Interfaces;

namespace PointsEngine.Application.Services;

/// <summary>
/// Application service for points ledger operations.
/// Handles point earning, deduction, and transaction history.
/// </summary>
public class LedgerService : ILedgerService
{
    private readonly ILedgerRepository _ledgerRepository;
    private readonly IUserBalanceRepository _balanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LedgerService(ILedgerRepository ledgerRepository, IUserBalanceRepository balanceRepository, IUnitOfWork unitOfWork)
    {
        _ledgerRepository = ledgerRepository;
        _balanceRepository = balanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LedgerResult> EarnPointsAsync(EarnPointsCommand command, CancellationToken cancellationToken = default)
    {
        var existingEntry = await _ledgerRepository.GetByIdempotencyKeyAsync(command.TenantId, command.IdempotencyKey, cancellationToken);
        if (existingEntry != null)
        {
            return new LedgerResult(existingEntry.Id, existingEntry.BalanceAfter, Success: true);
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var balance = await _balanceRepository.GetOrCreateAsync(command.TenantId, command.UserId, cancellationToken);
            var currentBalance = balance.AvailablePoints;
            var newBalance = currentBalance + command.PointsAmount;

            var ledgerEntry = LedgerEntry.CreateEarn(
                tenantId: command.TenantId,
                userId: command.UserId,
                idempotencyKey: command.IdempotencyKey,
                amount: command.PointsAmount,
                balanceAfter: newBalance,
                sourceType: command.SourceType,
                sourceId: command.SourceId,
                ruleId: command.RuleId,
                description: command.Description,
                expiresAt: command.ExpiresAt
                );
            await _ledgerRepository.AddAsync(ledgerEntry, cancellationToken);
            balance.ApplyEarn(command.PointsAmount, ledgerEntry.Id);
            await _balanceRepository.UpdateAsync(balance, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            return new LedgerResult(ledgerEntry.Id, ledgerEntry.BalanceAfter, Success: true);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            return new LedgerResult(null, 0, Success: false, Error: "Failed to earn points");
        }
    }

    public async Task<LedgerResult> DeductPointsAsync(DeductPointsCommand command, CancellationToken cancellationToken = default)
    {
        var existingEntry = await _ledgerRepository.GetByIdempotencyKeyAsync(command.TenantId, command.IdempotencyKey, cancellationToken);
        if (existingEntry != null)
        {
            return new LedgerResult(existingEntry.Id, existingEntry.BalanceAfter, Success: true);
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var balance = await _balanceRepository.GetOrCreateAsync(command.TenantId, command.UserId, cancellationToken);
            var currentBalance = balance.AvailablePoints;

            if (currentBalance - command.PointsAmount < 0)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                return new LedgerResult(null, currentBalance, Success: false,
                    Error: $"Insufficient balance. Available: {currentBalance}, Required: {command.PointsAmount}");
            }

            var ledgerEntry = LedgerEntry.CreateDeduct(
            tenantId: command.TenantId,
            userId: command.UserId,
            idempotencyKey: command.IdempotencyKey,
            amount: command.PointsAmount,
            balanceAfter: currentBalance - command.PointsAmount,
            sourceType: command.SourceType,
            sourceId: command.SourceId,
            description: command.Description
            );

            await _ledgerRepository.AddAsync(ledgerEntry, cancellationToken);
            balance.ApplyRedeem(command.PointsAmount, ledgerEntry.Id);
            await _balanceRepository.UpdateAsync(balance, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            return new LedgerResult(ledgerEntry.Id, ledgerEntry.BalanceAfter, Success: true);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            return new LedgerResult(null, 0, Success: false, Error: "Failed to deduct points");
        }
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
