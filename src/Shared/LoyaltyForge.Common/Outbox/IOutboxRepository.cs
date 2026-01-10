namespace LoyaltyForge.Common.Outbox;

/// <summary>
/// Repository interface for managing outbox messages.
/// </summary>
public interface IOutboxRepository
{
    /// <summary>
    /// Adds a new outbox message.
    /// </summary>
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending (unprocessed) outbox messages.
    /// </summary>
    /// <param name="batchSize">Maximum number of messages to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IEnumerable<OutboxMessage>> GetPendingAsync(int batchSize = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as processed.
    /// </summary>
    Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates retry count and error for a failed message.
    /// </summary>
    Task UpdateRetryAsync(Guid messageId, string error, CancellationToken cancellationToken = default);
}
