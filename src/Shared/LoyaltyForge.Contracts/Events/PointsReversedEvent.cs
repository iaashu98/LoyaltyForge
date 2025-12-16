namespace LoyaltyForge.Contracts.Events;

/// <summary>
/// Published when points are reversed (e.g., order refund).
/// </summary>
public sealed record PointsReversedEvent : IntegrationEvent
{
    public required Guid CustomerId { get; init; }
    public required int PointsAmount { get; init; }
    public required string Reason { get; init; }
    public required Guid TransactionId { get; init; }
    public required Guid OriginalTransactionId { get; init; }
}
