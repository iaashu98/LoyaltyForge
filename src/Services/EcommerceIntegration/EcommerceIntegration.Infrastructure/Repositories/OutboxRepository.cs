using LoyaltyForge.Common.Outbox;
using Microsoft.EntityFrameworkCore;

namespace EcommerceIntegration.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing outbox messages.
/// </summary>
public class OutboxRepository : IOutboxRepository
{
    private readonly DbContext _dbContext;
    private readonly DbSet<OutboxMessage> _outboxMessages;

    public OutboxRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
        _outboxMessages = dbContext.Set<OutboxMessage>();
    }

    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await _outboxMessages.AddAsync(message, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<OutboxMessage>> GetPendingAsync(
        int batchSize = 100,
        CancellationToken cancellationToken = default)
    {
        return await _outboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var message = await _outboxMessages.FindAsync(new object[] { messageId }, cancellationToken);
        if (message != null)
        {
            message.ProcessedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateRetryAsync(Guid messageId, string error, CancellationToken cancellationToken = default)
    {
        var message = await _outboxMessages.FindAsync(new object[] { messageId }, cancellationToken);
        if (message != null)
        {
            message.RetryCount++;
            message.LastError = error;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
