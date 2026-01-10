using LoyaltyForge.Contracts.Commands;

namespace LoyaltyForge.Contracts.Commands;

/// <summary>
/// Command to request points deduction for a reward redemption.
/// </summary>
public sealed record DeductPointsCommand : IntegrationCommand
{
    /// <summary>
    /// Customer ID whose points should be deducted.
    /// </summary>
    public required Guid CustomerId { get; init; }

    /// <summary>
    /// Amount of points to deduct.
    /// </summary>
    public required long Amount { get; init; }

    /// <summary>
    /// Redemption ID for tracking.
    /// </summary>
    public required Guid RedemptionId { get; init; }

    /// <summary>
    /// Idempotency key to prevent duplicate processing.
    /// </summary>
    public required string IdempotencyKey { get; init; }

    /// <summary>
    /// Description of the deduction.
    /// </summary>
    public required string Description { get; init; }
}
