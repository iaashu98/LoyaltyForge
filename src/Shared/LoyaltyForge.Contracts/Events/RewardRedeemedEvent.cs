namespace LoyaltyForge.Contracts.Events;

/// <summary>
/// Published when a reward is redeemed.
/// </summary>
public sealed record RewardRedeemedEvent : IntegrationEvent
{
    public required Guid CustomerId { get; init; }
    public required Guid RewardId { get; init; }
    public required string RewardName { get; init; }
    public required int PointsCost { get; init; }
    public required Guid RedemptionId { get; init; }
    public required string IdempotencyKey { get; init; }
}
