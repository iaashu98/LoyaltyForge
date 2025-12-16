namespace LoyaltyForge.Contracts.Events;

/// <summary>
/// Base class for all integration events.
/// </summary>
public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
    public Guid TenantId { get; init; }
}
