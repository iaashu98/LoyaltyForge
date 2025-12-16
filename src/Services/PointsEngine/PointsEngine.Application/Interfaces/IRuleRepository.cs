using PointsEngine.Domain.Entities;

namespace PointsEngine.Application.Interfaces;

/// <summary>
/// Repository interface for points rules.
/// </summary>
public interface IRuleRepository
{
    Task<IReadOnlyList<PointsRule>> GetActiveRulesByTenantAsync(
        Guid tenantId, 
        CancellationToken cancellationToken = default);
    
    Task<PointsRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task AddAsync(PointsRule rule, CancellationToken cancellationToken = default);
    Task UpdateAsync(PointsRule rule, CancellationToken cancellationToken = default);
}
