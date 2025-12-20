using PointsEngine.Domain.Entities;

namespace PointsEngine.Application.Interfaces;

/// <summary>
/// Application service for points ledger operations.
/// Handles point earning, deduction, and transaction history.
/// </summary>
public interface ILedgerService
{
    /// <summary>
    /// Records points earned by a customer.
    /// Idempotent - uses idempotency key to prevent duplicates.
    /// </summary>
    Task<LedgerResult> EarnPointsAsync(EarnPointsCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deducts points for a redemption.
    /// Idempotent - uses idempotency key to prevent duplicates.
    /// </summary>
    Task<LedgerResult> DeductPointsAsync(DeductPointsCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reverses a previous points transaction (refund scenario).
    /// </summary>
    Task<LedgerResult> ReversePointsAsync(ReversePointsCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transaction history for a customer.
    /// </summary>
    Task<IReadOnlyList<LedgerEntry>> GetTransactionHistoryAsync(
        Guid tenantId,
        Guid userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Expires points that have passed their expiration date.
    /// Called by a background job.
    /// </summary>
    Task<int> ExpirePointsAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

// Commands
public record EarnPointsCommand(
    Guid TenantId,
    Guid UserId,
    long PointsAmount,
    string SourceType,
    Guid? SourceId,
    Guid? RuleId,
    string IdempotencyKey,
    string? Description = null,
    DateTime? ExpiresAt = null);

public record DeductPointsCommand(
    Guid TenantId,
    Guid UserId,
    long PointsAmount,
    string SourceType,
    Guid? SourceId,
    string IdempotencyKey,
    string? Description = null);

public record ReversePointsCommand(
    Guid TenantId,
    Guid UserId,
    Guid OriginalLedgerEntryId,
    string IdempotencyKey,
    string? Reason = null);

// Results
public record LedgerResult(
    Guid? LedgerEntryId,
    long BalanceAfter,
    bool Success,
    string? Error = null);
