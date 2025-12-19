using PointsEngine.Domain.Entities;

namespace PointsEngine.Application.Interfaces;

/// <summary>
/// Repository interface for point rules.
/// </summary>
public interface IRuleRepository
{
    Task<IReadOnlyList<Rule>> GetActiveByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<Rule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Rule rule, CancellationToken cancellationToken = default);
    Task UpdateAsync(Rule rule, CancellationToken cancellationToken = default);
}
