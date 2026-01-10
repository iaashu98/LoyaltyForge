using LoyaltyForge.Contracts.Events;

namespace LoyaltyForge.Contracts.Events;

/// <summary>
/// Event published when points are successfully deducted.
/// </summary>
public sealed record PointsDeductedEvent : IntegrationEvent
{
    /// <summary>
    /// Customer ID whose points were deducted.
    /// </summary>
    public required Guid CustomerId { get; init; }

    /// <summary>
    /// Amount of points deducted.
    /// </summary>
    public required long Amount { get; init; }

    /// <summary>
    /// Redemption ID that triggered the deduction.
    /// </summary>
    public required Guid RedemptionId { get; init; }

    /// <summary>
    /// New balance after deduction.
    /// </summary>
    public required long NewBalance { get; init; }

    /// <summary>
    /// Transaction ID in the ledger.
    /// </summary>
    public required Guid TransactionId { get; init; }
}
