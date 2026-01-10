using LoyaltyForge.Common.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LoyaltyForge.Messaging.Outbox;

/// <summary>
/// Background service that publishes events from the outbox table to RabbitMQ.
/// Implements the Outbox Pattern for reliable event publishing.
/// </summary>
public class OutboxPublisher : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxPublisher> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);

    public OutboxPublisher(
        IServiceProvider serviceProvider,
        ILogger<OutboxPublisher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Publisher started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox Publisher stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var outboxRepository = scope.ServiceProvider.GetService<IOutboxRepository>();
        var eventPublisher = scope.ServiceProvider.GetService<RabbitMQ.IEventPublisher>();

        if (outboxRepository == null || eventPublisher == null)
        {
            _logger.LogWarning("Outbox repository or event publisher not registered");
            return;
        }

        var pendingMessages = await outboxRepository.GetPendingAsync(100, cancellationToken);
        var messagesList = pendingMessages.ToList();

        if (messagesList.Count == 0)
            return;

        _logger.LogInformation("Processing {Count} pending outbox messages", messagesList.Count);

        foreach (var message in messagesList)
        {
            try
            {
                // Deserialize and publish event
                var eventType = Type.GetType(message.EventType);
                if (eventType == null)
                {
                    _logger.LogError("Could not resolve event type {EventType}", message.EventType);
                    await outboxRepository.UpdateRetryAsync(message.Id, $"Could not resolve type: {message.EventType}", cancellationToken);
                    continue;
                }

                var @event = System.Text.Json.JsonSerializer.Deserialize(message.Payload, eventType);
                if (@event == null)
                {
                    _logger.LogError("Could not deserialize event {EventId}", message.Id);
                    await outboxRepository.UpdateRetryAsync(message.Id, "Deserialization failed", cancellationToken);
                    continue;
                }

                // Publish using reflection (since we don't know the exact type at compile time)
                var publishMethod = typeof(RabbitMQ.IEventPublisher).GetMethod("PublishAsync");
                if (publishMethod != null)
                {
                    var genericMethod = publishMethod.MakeGenericMethod(eventType);
                    await (Task)genericMethod.Invoke(eventPublisher, new[] { @event, cancellationToken })!;
                }

                // Mark as processed
                await outboxRepository.MarkAsProcessedAsync(message.Id, cancellationToken);
                _logger.LogInformation("Published outbox message {MessageId} of type {EventType}", message.Id, message.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing outbox message {MessageId}", message.Id);
                await outboxRepository.UpdateRetryAsync(message.Id, ex.Message, cancellationToken);
            }
        }
    }
}
