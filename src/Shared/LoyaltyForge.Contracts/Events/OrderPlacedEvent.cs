namespace LoyaltyForge.Contracts.Events;

/// <summary>
/// Canonical order placed event, transformed from e-commerce platform webhooks.
/// </summary>
public sealed record OrderPlacedEvent : IntegrationEvent
{
    public required string ExternalOrderId { get; init; }
    public required Guid CustomerId { get; init; }
    public required string CustomerEmail { get; init; }
    public required decimal OrderTotal { get; init; }
    public required string Currency { get; init; }
    public required IReadOnlyList<OrderLineItem> LineItems { get; init; }
    public required string SourcePlatform { get; init; }
}

public sealed record OrderLineItem
{
    public required string ProductId { get; init; }
    public required string ProductName { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public required decimal LineTotal { get; init; }
}
