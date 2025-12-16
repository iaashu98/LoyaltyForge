using PointsEngine.Domain.Enums;

namespace PointsEngine.Domain.Entities;

/// <summary>
/// Immutable ledger entry for point transactions.
/// Follows double-entry bookkeeping principles.
/// </summary>
public class PointsLedgerEntry
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid TransactionId { get; private set; }
    public PointsTransactionType TransactionType { get; private set; }
    public int Amount { get; private set; }
    public int BalanceAfter { get; private set; }
    public string Description { get; private set; } = default!;
    public string? ReferenceId { get; private set; }
    public string? ReferenceType { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private PointsLedgerEntry() { } // EF Core constructor

    public static PointsLedgerEntry CreateCredit(
        Guid tenantId,
        Guid customerId,
        int amount,
        int balanceAfter,
        string description,
        string? referenceId = null,
        string? referenceType = null)
    {
        return new PointsLedgerEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CustomerId = customerId,
            TransactionId = Guid.NewGuid(),
            TransactionType = PointsTransactionType.Credit,
            Amount = amount,
            BalanceAfter = balanceAfter,
            Description = description,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static PointsLedgerEntry CreateDebit(
        Guid tenantId,
        Guid customerId,
        int amount,
        int balanceAfter,
        string description,
        string? referenceId = null,
        string? referenceType = null)
    {
        return new PointsLedgerEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CustomerId = customerId,
            TransactionId = Guid.NewGuid(),
            TransactionType = PointsTransactionType.Debit,
            Amount = -amount,
            BalanceAfter = balanceAfter,
            Description = description,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            CreatedAt = DateTime.UtcNow
        };
    }
}
