using System.Text;
using System.Text.Json;
using LoyaltyForge.Contracts.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace LoyaltyForge.Messaging.RabbitMQ;

/// <summary>
/// Consumes integration events from RabbitMQ topic exchange (pub/sub pattern).
/// </summary>
public class RabbitMQEventConsumer : IEventConsumer, IDisposable
{
    private readonly RabbitMQOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMQEventConsumer> _logger;
    private readonly Dictionary<string, Type> _eventHandlers = new();
    private readonly Dictionary<string, string> _routingKeys = new();

    private IConnection? _connection;
    private IModel? _channel;
    private string? _queueName;

    public RabbitMQEventConsumer(
        IOptions<RabbitMQOptions> options,
        IServiceProvider serviceProvider,
        ILogger<RabbitMQEventConsumer> logger)
    {
        _options = options.Value;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Registers an event handler for a specific event type.
    /// </summary>
    public void RegisterHandler<TEvent>(string routingKey) where TEvent : IntegrationEvent
    {
        var eventType = typeof(TEvent).Name;
        _eventHandlers[eventType] = typeof(TEvent);
        _routingKeys[eventType] = routingKey;
        _logger.LogInformation("Registered handler for event {EventType} with routing key {RoutingKey}", eventType, routingKey);
    }

    public Task StartConsumingAsync(CancellationToken cancellationToken = default)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.Username,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare exchange
        _channel.ExchangeDeclare(
            exchange: _options.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        // Create service-specific queue
        _queueName = $"{_options.ServiceName}.events";
        _channel.QueueDeclare(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        // Bind queue to exchange with routing keys
        foreach (var routingKey in _routingKeys.Values.Distinct())
        {
            _channel.QueueBind(
                queue: _queueName,
                exchange: _options.ExchangeName,
                routingKey: routingKey);

            _logger.LogInformation("Bound queue {QueueName} to exchange {ExchangeName} with routing key {RoutingKey}",
                _queueName, _options.ExchangeName, routingKey);
        }

        // Set up consumer
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var eventType = ea.BasicProperties.Type;

                _logger.LogInformation("Received event {EventType} from queue {QueueName}", eventType, _queueName);

                if (_eventHandlers.TryGetValue(eventType, out var handlerType))
                {
                    await ProcessEventAsync(message, handlerType, cancellationToken);
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                else
                {
                    _logger.LogWarning("No handler registered for event type {EventType}", eventType);
                    _channel.BasicAck(ea.DeliveryTag, false); // Ack anyway to prevent redelivery
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event");
                // Negative acknowledge - message will be requeued or sent to DLQ
                _channel.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _channel.BasicConsume(
            queue: _queueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("Started consuming events from queue {QueueName}", _queueName);

        return Task.CompletedTask;
    }

    private async Task ProcessEventAsync(string message, Type eventType, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        // Deserialize event
        var @event = JsonSerializer.Deserialize(message, eventType, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }) as IntegrationEvent;

        if (@event == null)
        {
            _logger.LogError("Failed to deserialize event of type {EventType}", eventType.Name);
            return;
        }

        // Get handler from DI
        var handlerInterfaceType = typeof(IEventHandler<>).MakeGenericType(eventType);
        var handler = scope.ServiceProvider.GetService(handlerInterfaceType);

        if (handler == null)
        {
            _logger.LogWarning("No handler implementation found for event type {EventType}", eventType.Name);
            return;
        }

        // Invoke handler
        var handleMethod = handlerInterfaceType.GetMethod("HandleAsync");
        if (handleMethod != null)
        {
            await (Task)handleMethod.Invoke(handler, new object[] { @event, cancellationToken })!;
        }
    }

    public Task StopConsumingAsync(CancellationToken cancellationToken = default)
    {
        _channel?.Close();
        _connection?.Close();
        _logger.LogInformation("Stopped consuming events");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
