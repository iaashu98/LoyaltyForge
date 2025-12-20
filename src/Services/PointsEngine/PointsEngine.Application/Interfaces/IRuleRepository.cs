using PointsEngine.Domain.Entities;

namespace PointsEngine.Application.Interfaces;

/// <summary>
/// Repository interface for point rules.
/// </summary>
public interface IRuleRepository
{
    Task<List<Rule>> GetActiveByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken);

    Task<Rule?> GetRuleByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Rule>> GetAllRulesAsync(Guid tenantId, CancellationToken cancellationToken);
    Task AddRuleAsync(Rule rule, CancellationToken cancellationToken);
    Task UpdateRuleAsync(Rule rule, CancellationToken cancellationToken);
    Task DeleteRuleAsync(Guid id, CancellationToken cancellationToken);
}
