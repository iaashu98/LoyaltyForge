using LoyaltyForge.Contracts.Events;

namespace LoyaltyForge.Contracts.Events;

/// <summary>
/// Event published when points deduction fails (e.g., insufficient balance).
/// </summary>
public sealed record PointsDeductionFailedEvent : IntegrationEvent
{
    /// <summary>
    /// Customer ID whose points deduction failed.
    /// </summary>
    public required Guid CustomerId { get; init; }

    /// <summary>
    /// Amount of points that was attempted to be deducted.
    /// </summary>
    public required long RequestedAmount { get; init; }

    /// <summary>
    /// Redemption ID that triggered the deduction attempt.
    /// </summary>
    public required Guid RedemptionId { get; init; }

    /// <summary>
    /// Current balance (insufficient for deduction).
    /// </summary>
    public required long CurrentBalance { get; init; }

    /// <summary>
    /// Reason for failure.
    /// </summary>
    public required string FailureReason { get; init; }
}
