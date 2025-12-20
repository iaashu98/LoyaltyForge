using Microsoft.EntityFrameworkCore;
using PointsEngine.Application.Interfaces;
using PointsEngine.Domain.Entities;
using PointsEngine.Infrastructure.Persistence;

namespace PointsEngine.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ILedgerRepository.
/// Handles IMMUTABLE ledger entry operations.
/// </summary>
public class LedgerRepository(PointsEngineDbContext context) : ILedgerRepository
{
    public async Task<IReadOnlyList<LedgerEntry>> GetByUserAsync(
        Guid tenantId,
        Guid userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return await context.LedgerEntries
            .Where(e => e.TenantId == tenantId && e.UserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<LedgerEntry?> GetByIdempotencyKeyAsync(
        Guid tenantId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        return await context.LedgerEntries
            .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public async Task AddAsync(LedgerEntry entry, CancellationToken cancellationToken = default)
    {
        await context.LedgerEntries.AddAsync(entry, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
