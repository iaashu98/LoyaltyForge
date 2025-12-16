namespace Rewards.Application.Commands;

/// <summary>
/// Command to redeem a reward.
/// </summary>
public record RedeemRewardCommand(
    Guid TenantId,
    Guid CustomerId,
    Guid RewardId,
    string IdempotencyKey);

/// <summary>
/// Result of redemption attempt.
/// </summary>
public record RedeemRewardResult(
    Guid RedemptionId,
    bool Success,
    string? Error);
