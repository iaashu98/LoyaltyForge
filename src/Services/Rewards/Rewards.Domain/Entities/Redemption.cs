using Rewards.Domain.Enums;

namespace Rewards.Domain.Entities;

/// <summary>
/// Represents a reward redemption with idempotency support.
/// </summary>
public class Redemption
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid RewardId { get; private set; }
    public string IdempotencyKey { get; private set; } = default!;
    public int PointsSpent { get; private set; }
    public RedemptionStatus Status { get; private set; }
    public string? FulfillmentDetails { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    // Navigation properties
    public Reward Reward { get; private set; } = default!;

    private Redemption() { } // EF Core constructor

    public static Redemption Create(
        Guid tenantId,
        Guid customerId,
        Guid rewardId,
        string idempotencyKey,
        int pointsSpent)
    {
        return new Redemption
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CustomerId = customerId,
            RewardId = rewardId,
            IdempotencyKey = idempotencyKey,
            PointsSpent = pointsSpent,
            Status = RedemptionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkCompleted(string? fulfillmentDetails = null)
    {
        Status = RedemptionStatus.Completed;
        FulfillmentDetails = fulfillmentDetails;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        Status = RedemptionStatus.Failed;
        CompletedAt = DateTime.UtcNow;
    }
}
