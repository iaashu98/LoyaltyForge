namespace Rewards.Domain.Entities;

/// <summary>
/// Records of reward redemptions - idempotent to prevent double-spend.
/// Maps to: rewards.redemptions
/// </summary>
public class Redemption
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid RewardId { get; private set; }
    public string IdempotencyKey { get; private set; } = default!;
    public long PointsSpent { get; private set; }
    public Guid? LedgerEntryId { get; private set; }
    public string Status { get; private set; } = default!;
    public string? FulfillmentData { get; private set; }  // JSON
    public string? ExternalReference { get; private set; }
    public DateTime? FulfilledAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation
    public CatalogItem Reward { get; private set; } = default!;

    private Redemption() { } // EF Core constructor

    public static Redemption Create(
        Guid tenantId,
        Guid userId,
        Guid rewardId,
        string idempotencyKey,
        long pointsSpent)
    {
        return new Redemption
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            RewardId = rewardId,
            IdempotencyKey = idempotencyKey,
            PointsSpent = pointsSpent,
            Status = RedemptionStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void LinkLedgerEntry(Guid ledgerEntryId)
    {
        LedgerEntryId = ledgerEntryId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFulfilled(string? fulfillmentData = null, string? externalReference = null)
    {
        Status = RedemptionStatus.Fulfilled;
        FulfillmentData = fulfillmentData;
        ExternalReference = externalReference;
        FulfilledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        Status = RedemptionStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkCancelled()
    {
        Status = RedemptionStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkExpired()
    {
        Status = RedemptionStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Redemption status constants matching schema CHECK constraint.
/// </summary>
public static class RedemptionStatus
{
    public const string Pending = "pending";
    public const string Fulfilled = "fulfilled";
    public const string Expired = "expired";
    public const string Cancelled = "cancelled";
    public const string Failed = "failed";
}
