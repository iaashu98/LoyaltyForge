using LoyaltyForge.Contracts.Events;

namespace LoyaltyForge.Messaging.RabbitMQ;

/// <summary>
/// Interface for publishing events to RabbitMQ.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}
