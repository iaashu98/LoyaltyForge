using LoyaltyForge.Contracts.Events;

namespace PointsEngine.Application.Interfaces;

/// <summary>
/// Handler for OrderPlaced events from e-commerce integrations.
/// Applies rules and awards points.
/// </summary>
public interface IOrderPlacedEventHandler
{
    Task HandleAsync(OrderPlacedEvent @event, CancellationToken cancellationToken = default);
}
