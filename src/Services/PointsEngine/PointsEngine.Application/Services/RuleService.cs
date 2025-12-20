using PointsEngine.Application.Interfaces;
using PointsEngine.Domain.Entities;

namespace PointsEngine.Application.Services;

/// <summary>
/// Application service for managing points earning rules.
/// </summary>
public class RuleService(IRuleRepository ruleRepository) : IRuleService
{
    private readonly IRuleRepository _ruleRepository = ruleRepository;

    public async Task<RuleResult> CreateRuleAsync(CreateRuleCommand command, CancellationToken cancellationToken = default)
    {
        // TODO: Implement business logic
        // 1. Validate rule definition JSON
        // 2. Check for duplicate names within tenant
        // 3. Create and persist rule

        var rule = Rule.Create(
            command.TenantId,
            command.Name,
            command.EventType,
            command.RuleDefinition,
            command.Priority,
            command.Description,
            command.ValidFrom,
            command.ValidUntil,
            command.CreatedBy);

        await _ruleRepository.AddRuleAsync(rule, cancellationToken);

        return new RuleResult(rule.Id, rule.Name, Success: true);
    }

    public async Task<IReadOnlyList<Rule>> GetRulesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _ruleRepository.GetAllRulesAsync(tenantId, cancellationToken);
    }

    public async Task<Rule?> GetRuleByIdAsync(Guid ruleId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var rule = await _ruleRepository.GetRuleByIdAsync(ruleId, cancellationToken);

        // Ensure rule belongs to the tenant
        if (rule != null && rule.TenantId != tenantId)
        {
            return null;
        }

        return rule;
    }

    public async Task<RuleResult> UpdateRuleAsync(UpdateRuleCommand command, CancellationToken cancellationToken = default)
    {
        // TODO: Implement update logic
        // 1. Fetch existing rule
        // 2. Validate ownership (tenant)
        // 3. Apply updates
        // 4. Persist

        var rule = await _ruleRepository.GetRuleByIdAsync(command.RuleId, cancellationToken);

        if (rule == null || rule.TenantId != command.TenantId)
        {
            return new RuleResult(command.RuleId, string.Empty, Success: false, Error: "Rule not found");
        }

        // TODO: Apply updates to rule entity
        if (command.RuleDefinition != null)
        {
            rule.UpdateRuleDefinition(command.RuleDefinition);
        }

        await _ruleRepository.UpdateRuleAsync(rule, cancellationToken);

        return new RuleResult(rule.Id, rule.Name, Success: true);
    }

    public async Task DeleteRuleAsync(Guid ruleId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var rule = await _ruleRepository.GetRuleByIdAsync(ruleId, cancellationToken);

        if (rule == null || rule.TenantId != tenantId)
        {
            throw new InvalidOperationException("Rule not found");
        }

        await _ruleRepository.DeleteRuleAsync(ruleId, cancellationToken);
    }

    public async Task ActivateRuleAsync(Guid ruleId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var rule = await _ruleRepository.GetRuleByIdAsync(ruleId, cancellationToken);

        if (rule == null || rule.TenantId != tenantId)
        {
            throw new InvalidOperationException("Rule not found");
        }

        rule.Activate();
        await _ruleRepository.UpdateRuleAsync(rule, cancellationToken);
    }

    public async Task DeactivateRuleAsync(Guid ruleId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var rule = await _ruleRepository.GetRuleByIdAsync(ruleId, cancellationToken);

        if (rule == null || rule.TenantId != tenantId)
        {
            throw new InvalidOperationException("Rule not found");
        }

        rule.Deactivate();
        await _ruleRepository.UpdateRuleAsync(rule, cancellationToken);
    }
}
