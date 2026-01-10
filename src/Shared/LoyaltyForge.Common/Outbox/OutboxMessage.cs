namespace LoyaltyForge.Common.Outbox;

/// <summary>
/// Entity representing an outbox message for reliable event publishing.
/// Implements the Outbox Pattern to ensure atomicity between database writes and message publishing.
/// </summary>
public class OutboxMessage
{
    /// <summary>
    /// Unique identifier for the outbox message.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Type of the event (e.g., "OrderPlacedEvent", "PointsEarnedEvent").
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// JSON serialized payload of the event.
    /// </summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the message was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the message was successfully published (null if not yet published).
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Number of times publishing has been retried.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Tenant ID for multi-tenancy isolation.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Last error message if publishing failed.
    /// </summary>
    public string? LastError { get; set; }
}
