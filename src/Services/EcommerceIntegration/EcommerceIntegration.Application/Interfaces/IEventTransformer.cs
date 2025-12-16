using LoyaltyForge.Contracts.Events;

namespace EcommerceIntegration.Application.Interfaces;

/// <summary>
/// Interface for transforming platform-specific events to canonical events.
/// </summary>
public interface IEventTransformer<TSource>
{
    IntegrationEvent Transform(Guid tenantId, TSource sourceEvent);
}
