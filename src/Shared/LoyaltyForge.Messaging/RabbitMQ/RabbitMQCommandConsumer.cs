using System.Text;
using System.Text.Json;
using LoyaltyForge.Contracts.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace LoyaltyForge.Messaging.RabbitMQ;

/// <summary>
/// Consumes integration commands from a specific service queue (point-to-point pattern).
/// </summary>
public class RabbitMQCommandConsumer : IDisposable
{
    private readonly RabbitMQOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMQCommandConsumer> _logger;
    private readonly string _queueName;
    private readonly Dictionary<string, Type> _commandHandlers = new();

    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMQCommandConsumer(
        IOptions<RabbitMQOptions> options,
        IServiceProvider serviceProvider,
        ILogger<RabbitMQCommandConsumer> logger,
        string queueName)
    {
        _options = options.Value;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _queueName = queueName;
    }

    /// <summary>
    /// Registers a command handler for a specific command type.
    /// </summary>
    public void RegisterHandler<TCommand>() where TCommand : IntegrationCommand
    {
        var commandType = typeof(TCommand).Name;
        _commandHandlers[commandType] = typeof(TCommand);
        _logger.LogInformation("Registered handler for command {CommandType}", commandType);
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

        // Declare queue
        _channel.QueueDeclare(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        // Set prefetch count to process one command at a time
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        // Set up consumer
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var commandType = ea.BasicProperties.Type;

                _logger.LogInformation("Received command {CommandType} from queue {QueueName}", commandType, _queueName);

                if (_commandHandlers.TryGetValue(commandType, out var handlerType))
                {
                    var result = await ProcessCommandAsync(message, handlerType, cancellationToken);

                    if (result.Success)
                    {
                        _channel.BasicAck(ea.DeliveryTag, false);
                        _logger.LogInformation("Successfully processed command {CommandType}", commandType);
                    }
                    else
                    {
                        _logger.LogWarning("Command {CommandType} processing failed: {Error}", commandType, result.Error);
                        // Negative acknowledge - will be requeued or sent to DLQ
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                    }
                }
                else
                {
                    _logger.LogWarning("No handler registered for command type {CommandType}", commandType);
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing command");
                _channel.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _channel.BasicConsume(
            queue: _queueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("Started consuming commands from queue {QueueName}", _queueName);

        return Task.CompletedTask;
    }

    private async Task<CommandResult> ProcessCommandAsync(string message, Type commandType, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        // Deserialize command
        var command = JsonSerializer.Deserialize(message, commandType, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }) as IntegrationCommand;

        if (command == null)
        {
            _logger.LogError("Failed to deserialize command of type {CommandType}", commandType.Name);
            return new CommandResult(false, "Deserialization failed");
        }

        // Get handler from DI
        var handlerInterfaceType = typeof(ICommandHandler<>).MakeGenericType(commandType);
        var handler = scope.ServiceProvider.GetService(handlerInterfaceType);

        if (handler == null)
        {
            _logger.LogWarning("No handler implementation found for command type {CommandType}", commandType.Name);
            return new CommandResult(false, "Handler not found");
        }

        // Invoke handler
        var handleMethod = handlerInterfaceType.GetMethod("HandleAsync");
        if (handleMethod != null)
        {
            var result = await (Task<CommandResult>)handleMethod.Invoke(handler, new object[] { command, cancellationToken })!;
            return result;
        }

        return new CommandResult(false, "Handler method not found");
    }

    public Task StopConsumingAsync(CancellationToken cancellationToken = default)
    {
        _channel?.Close();
        _connection?.Close();
        _logger.LogInformation("Stopped consuming commands from queue {QueueName}", _queueName);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
