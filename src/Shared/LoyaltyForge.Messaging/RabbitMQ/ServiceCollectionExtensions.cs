using LoyaltyForge.Contracts.Commands;
using LoyaltyForge.Contracts.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LoyaltyForge.Messaging.RabbitMQ;

/// <summary>
/// Extension methods for registering RabbitMQ messaging services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers RabbitMQ event publisher for pub/sub messaging.
    /// </summary>
    public static IServiceCollection AddRabbitMQEventPublisher(this IServiceCollection services)
    {
        services.AddSingleton<IEventPublisher, RabbitMQEventPublisher>();
        return services;
    }

    /// <summary>
    /// Registers RabbitMQ command publisher for point-to-point messaging.
    /// </summary>
    public static IServiceCollection AddRabbitMQCommandPublisher(this IServiceCollection services)
    {
        services.AddSingleton<ICommandPublisher, RabbitMQCommandPublisher>();
        return services;
    }

    /// <summary>
    /// Registers RabbitMQ event consumer for pub/sub messaging.
    /// </summary>
    public static IServiceCollection AddRabbitMQEventConsumer(
        this IServiceCollection services,
        Action<EventConsumerBuilder>? configure = null)
    {
        services.AddSingleton<RabbitMQEventConsumer>();
        services.AddHostedService<EventConsumerHostedService>();

        if (configure != null)
        {
            var builder = new EventConsumerBuilder(services);
            configure(builder);
        }

        return services;
    }

    /// <summary>
    /// Registers RabbitMQ command consumer for point-to-point messaging.
    /// </summary>
    public static IServiceCollection AddRabbitMQCommandConsumer(
        this IServiceCollection services,
        string queueName,
        Action<CommandConsumerBuilder>? configure = null)
    {
        services.AddSingleton(sp => new RabbitMQCommandConsumer(
            sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RabbitMQOptions>>(),
            sp,
            sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RabbitMQCommandConsumer>>(),
            queueName));

        services.AddHostedService<CommandConsumerHostedService>();

        if (configure != null)
        {
            var builder = new CommandConsumerBuilder(services);
            configure(builder);
        }

        return services;
    }

    /// <summary>
    /// Registers an event handler for a specific event type.
    /// </summary>
    public static IServiceCollection AddEventHandler<TEvent, THandler>(this IServiceCollection services)
        where TEvent : IntegrationEvent
        where THandler : class, IEventHandler<TEvent>
    {
        services.AddScoped<IEventHandler<TEvent>, THandler>();
        return services;
    }

    /// <summary>
    /// Registers a command handler for a specific command type.
    /// </summary>
    public static IServiceCollection AddCommandHandler<TCommand, THandler>(this IServiceCollection services)
        where TCommand : IntegrationCommand
        where THandler : class, ICommandHandler<TCommand>
    {
        services.AddScoped<ICommandHandler<TCommand>, THandler>();
        return services;
    }
}

/// <summary>
/// Builder for configuring event consumer.
/// </summary>
public class EventConsumerBuilder
{
    private readonly IServiceCollection _services;
    internal readonly List<(Type EventType, string RoutingKey)> EventSubscriptions = new();

    public EventConsumerBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Subscribes to an event type with a specific routing key.
    /// </summary>
    public EventConsumerBuilder SubscribeToEvent<TEvent>(string routingKey) where TEvent : IntegrationEvent
    {
        EventSubscriptions.Add((typeof(TEvent), routingKey));
        return this;
    }
}

/// <summary>
/// Builder for configuring command consumer.
/// </summary>
public class CommandConsumerBuilder
{
    private readonly IServiceCollection _services;
    internal readonly List<Type> CommandSubscriptions = new();

    public CommandConsumerBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Subscribes to a command type.
    /// </summary>
    public CommandConsumerBuilder SubscribeToCommand<TCommand>() where TCommand : IntegrationCommand
    {
        CommandSubscriptions.Add(typeof(TCommand));
        return this;
    }
}

/// <summary>
/// Hosted service that starts the event consumer.
/// </summary>
internal class EventConsumerHostedService : IHostedService
{
    private readonly RabbitMQEventConsumer _consumer;

    public EventConsumerHostedService(RabbitMQEventConsumer consumer)
    {
        _consumer = consumer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _consumer.StartConsumingAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _consumer.StopConsumingAsync(cancellationToken);
    }
}

/// <summary>
/// Hosted service that starts the command consumer.
/// </summary>
internal class CommandConsumerHostedService : IHostedService
{
    private readonly RabbitMQCommandConsumer _consumer;

    public CommandConsumerHostedService(RabbitMQCommandConsumer consumer)
    {
        _consumer = consumer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _consumer.StartConsumingAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _consumer.StopConsumingAsync(cancellationToken);
    }
}
