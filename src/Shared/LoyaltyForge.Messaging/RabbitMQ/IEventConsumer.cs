using LoyaltyForge.Contracts.Events;

namespace LoyaltyForge.Messaging.RabbitMQ;

/// <summary>
/// Interface for consuming events from RabbitMQ.
/// </summary>
public interface IEventConsumer
{
    Task StartConsumingAsync(CancellationToken cancellationToken = default);
    Task StopConsumingAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for handling specific event types.
/// </summary>
public interface IEventHandler<in TEvent> where TEvent : IntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
