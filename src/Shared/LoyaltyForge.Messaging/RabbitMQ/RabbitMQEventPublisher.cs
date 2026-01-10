using System.Text.Json;
using RabbitMQ.Client;
using System.Text;
using LoyaltyForge.Contracts.Events;
using Microsoft.Extensions.Options;

namespace LoyaltyForge.Messaging.RabbitMQ;

/// <summary>
/// Publishes integration events to RabbitMQ topic exchange (pub/sub pattern).
/// </summary>
public class RabbitMQEventPublisher : IEventPublisher, IDisposable
{
    private readonly RabbitMQOptions _options;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly object _lock = new();

    public RabbitMQEventPublisher(IOptions<RabbitMQOptions> options)
    {
        _options = options.Value;

        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.Username,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare topic exchange for events
        _channel.ExchangeDeclare(
            exchange: _options.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        if (@event == null) throw new ArgumentNullException(nameof(@event));

        lock (_lock)
        {
            // Generate routing key from event type (e.g., "order.placed", "points.earned")
            var routingKey = GetRoutingKey(@event.EventType);

            // Serialize event to JSON
            var message = JsonSerializer.Serialize(@event, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var body = Encoding.UTF8.GetBytes(message);

            // Set message properties
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true; // Persist messages to disk
            properties.ContentType = "application/json";
            properties.MessageId = @event.EventId.ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Type = @event.EventType;

            // Add correlation ID for distributed tracing
            properties.Headers = new Dictionary<string, object>
            {
                ["tenantId"] = @event.TenantId.ToString(),
                ["eventId"] = @event.EventId.ToString()
            };

            // Publish to exchange
            _channel.BasicPublish(
                exchange: _options.ExchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Converts event type to routing key (e.g., "OrderPlacedEvent" -> "order.placed").
    /// </summary>
    private static string GetRoutingKey(string eventType)
    {
        // Remove "Event" suffix and convert to snake_case with dots
        var name = eventType.Replace("Event", "");

        // Convert PascalCase to dot.separated.lowercase
        var result = new StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]))
            {
                result.Append('.');
            }
            result.Append(char.ToLower(name[i]));
        }

        return result.ToString();
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}
