using PointsEngine.Domain.Entities;

namespace PointsEngine.Application.Interfaces;

/// <summary>
/// Repository interface for ledger operations.
/// </summary>
public interface ILedgerRepository
{
    Task<IReadOnlyList<LedgerEntry>> GetByUserAsync(
        Guid tenantId,
        Guid userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<LedgerEntry?> GetByIdempotencyKeyAsync(
        Guid tenantId,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task AddAsync(LedgerEntry entry, CancellationToken cancellationToken = default);
}
