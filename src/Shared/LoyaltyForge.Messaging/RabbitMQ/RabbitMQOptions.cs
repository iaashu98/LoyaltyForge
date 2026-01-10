namespace LoyaltyForge.Messaging.RabbitMQ;

/// <summary>
/// RabbitMQ connection configuration.
/// </summary>
public class RabbitMQOptions
{
    public const string SectionName = "RabbitMQ";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string ExchangeName { get; set; } = "loyaltyforge.events";

    /// <summary>
    /// Name of the service (used for queue naming).
    /// </summary>
    public string ServiceName { get; set; } = "unknown";

    /// <summary>
    /// Prefetch count for consumers (number of messages to prefetch).
    /// </summary>
    public ushort PrefetchCount { get; set; } = 10;
}
