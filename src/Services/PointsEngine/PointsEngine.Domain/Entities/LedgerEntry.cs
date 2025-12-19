namespace PointsEngine.Domain.Entities;

/// <summary>
/// IMMUTABLE append-only ledger - SINGLE SOURCE OF TRUTH for points.
/// Maps to: points.ledger_entries
/// </summary>
public class LedgerEntry
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string IdempotencyKey { get; private set; } = default!;
    public string EntryType { get; private set; } = default!;
    public long PointsAmount { get; private set; }
    public long BalanceAfter { get; private set; }
    public string SourceType { get; private set; } = default!;
    public Guid? SourceId { get; private set; }
    public Guid? RuleId { get; private set; }
    public string? Description { get; private set; }
    public string? Metadata { get; private set; }  // JSON
    public DateTime? ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private LedgerEntry() { } // EF Core constructor

    public static LedgerEntry CreateEarn(
        Guid tenantId,
        Guid userId,
        string idempotencyKey,
        long amount,
        long balanceAfter,
        string sourceType,
        Guid? sourceId = null,
        Guid? ruleId = null,
        string? description = null,
        DateTime? expiresAt = null)
    {
        return new LedgerEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            IdempotencyKey = idempotencyKey,
            EntryType = LedgerEntryType.Earn,
            PointsAmount = amount,
            BalanceAfter = balanceAfter,
            SourceType = sourceType,
            SourceId = sourceId,
            RuleId = ruleId,
            Description = description,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static LedgerEntry CreateRedeem(
        Guid tenantId,
        Guid userId,
        string idempotencyKey,
        long amount,
        long balanceAfter,
        string sourceType,
        Guid? sourceId = null,
        string? description = null)
    {
        return new LedgerEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            IdempotencyKey = idempotencyKey,
            EntryType = LedgerEntryType.Redeem,
            PointsAmount = -amount,  // Negative for debits
            BalanceAfter = balanceAfter,
            SourceType = sourceType,
            SourceId = sourceId,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static LedgerEntry CreateExpiry(
        Guid tenantId,
        Guid userId,
        string idempotencyKey,
        long amount,
        long balanceAfter,
        string? description = null)
    {
        return new LedgerEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            IdempotencyKey = idempotencyKey,
            EntryType = LedgerEntryType.Expire,
            PointsAmount = -amount,  // Negative for expiry
            BalanceAfter = balanceAfter,
            SourceType = SourceTypes.Expiry,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Ledger entry type constants matching schema CHECK constraint.
/// </summary>
public static class LedgerEntryType
{
    public const string Earn = "earn";
    public const string Redeem = "redeem";
    public const string Expire = "expire";
    public const string Adjust = "adjust";
    public const string Refund = "refund";
}

/// <summary>
/// Source type constants for ledger entries.
/// </summary>
public static class SourceTypes
{
    public const string Order = "order";
    public const string Rule = "rule";
    public const string Manual = "manual";
    public const string Redemption = "redemption";
    public const string Expiry = "expiry";
}
