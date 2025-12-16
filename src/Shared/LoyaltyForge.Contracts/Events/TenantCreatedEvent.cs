namespace LoyaltyForge.Contracts.Events;

/// <summary>
/// Published when a new tenant is created.
/// </summary>
public sealed record TenantCreatedEvent : IntegrationEvent
{
    public required string TenantName { get; init; }
    public required string ContactEmail { get; init; }
}
