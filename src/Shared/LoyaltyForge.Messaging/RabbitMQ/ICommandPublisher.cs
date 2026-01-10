using LoyaltyForge.Contracts.Commands;

namespace LoyaltyForge.Messaging.RabbitMQ;

/// <summary>
/// Interface for publishing commands to specific service queues (point-to-point).
/// </summary>
public interface ICommandPublisher
{
    /// <summary>
    /// Sends a command to a specific service queue.
    /// </summary>
    /// <typeparam name="TCommand">Type of command to send</typeparam>
    /// <param name="command">Command instance</param>
    /// <param name="queueName">Target queue name (e.g., "points.commands")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendAsync<TCommand>(TCommand command, string queueName, CancellationToken cancellationToken = default)
        where TCommand : IntegrationCommand;
}
