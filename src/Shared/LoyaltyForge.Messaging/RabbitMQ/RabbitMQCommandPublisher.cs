using System.Text;
using System.Text.Json;
using LoyaltyForge.Contracts.Commands;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace LoyaltyForge.Messaging.RabbitMQ;

/// <summary>
/// Publishes integration commands to specific service queues (point-to-point pattern).
/// </summary>
public class RabbitMQCommandPublisher : ICommandPublisher, IDisposable
{
    private readonly RabbitMQOptions _options;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly object _lock = new();

    public RabbitMQCommandPublisher(IOptions<RabbitMQOptions> options)
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
    }

    public Task SendAsync<TCommand>(TCommand command, string queueName, CancellationToken cancellationToken = default)
        where TCommand : IntegrationCommand
    {
        if (command == null) throw new ArgumentNullException(nameof(command));
        if (string.IsNullOrWhiteSpace(queueName)) throw new ArgumentException("Queue name cannot be empty", nameof(queueName));

        lock (_lock)
        {
            // Declare queue if it doesn't exist (idempotent)
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Serialize command to JSON
            var message = JsonSerializer.Serialize(command, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var body = Encoding.UTF8.GetBytes(message);

            // Set message properties
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.MessageId = command.CommandId.ToString();
            properties.CorrelationId = command.CorrelationId;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Type = command.CommandType;

            properties.Headers = new Dictionary<string, object>
            {
                ["tenantId"] = command.TenantId.ToString(),
                ["commandId"] = command.CommandId.ToString(),
                ["correlationId"] = command.CorrelationId
            };

            // Publish directly to queue (no exchange needed for point-to-point)
            _channel.BasicPublish(
                exchange: string.Empty,
                routingKey: queueName,
                basicProperties: properties,
                body: body);
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}
