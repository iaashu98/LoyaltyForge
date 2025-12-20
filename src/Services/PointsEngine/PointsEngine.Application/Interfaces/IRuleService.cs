using PointsEngine.Domain.Entities;

namespace PointsEngine.Application.Interfaces;

/// <summary>
/// Application service for managing points earning rules.
/// Orchestrates rule CRUD operations.
/// </summary>
public interface IRuleService
{
    /// <summary>
    /// Creates a new rule for a tenant.
    /// </summary>
    Task<RuleResult> CreateRuleAsync(CreateRuleCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all rules for a tenant.
    /// </summary>
    Task<IReadOnlyList<Rule>> GetRulesAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific rule by ID.
    /// </summary>
    Task<Rule?> GetRuleByIdAsync(Guid ruleId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing rule.
    /// </summary>
    Task<RuleResult> UpdateRuleAsync(UpdateRuleCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a rule.
    /// </summary>
    Task DeleteRuleAsync(Guid ruleId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a rule.
    /// </summary>
    Task ActivateRuleAsync(Guid ruleId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a rule.
    /// </summary>
    Task DeactivateRuleAsync(Guid ruleId, Guid tenantId, CancellationToken cancellationToken = default);
}

// Commands
public record CreateRuleCommand(
    Guid TenantId,
    string Name,
    string EventType,
    string RuleDefinition,
    int Priority = 0,
    string? Description = null,
    DateTime? ValidFrom = null,
    DateTime? ValidUntil = null,
    Guid? CreatedBy = null);

public record UpdateRuleCommand(
    Guid RuleId,
    Guid TenantId,
    string? Name = null,
    string? Description = null,
    string? RuleDefinition = null,
    int? Priority = null,
    DateTime? ValidUntil = null);

// Results
public record RuleResult(Guid RuleId, string Name, bool Success, string? Error = null);
