namespace LoyaltyForge.Contracts.Events;

/// <summary>
/// Published when points are earned by a customer.
/// </summary>
public sealed record PointsEarnedEvent : IntegrationEvent
{
    public required Guid CustomerId { get; init; }
    public required int PointsAmount { get; init; }
    public required string Reason { get; init; }
    public required Guid TransactionId { get; init; }
    public string? ReferenceId { get; init; }
}
