using Microsoft.EntityFrameworkCore;
using PointsEngine.Application.Interfaces;
using PointsEngine.Domain.Entities;
using PointsEngine.Infrastructure.Persistence;


namespace PointsEngine.Infrastructure.Repositories;

public class RuleRepository(PointsEngineDbContext context) : IRuleRepository
{
    public Task AddRuleAsync(Rule rule, CancellationToken cancellationToken = default)
    {
        context.Rules.Add(rule);
        return context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRuleAsync(Guid ruleId, CancellationToken cancellationToken = default)
    {
        var rule = await GetRuleByIdAsync(ruleId, cancellationToken);
        if (rule == null)
        {
            throw new ArgumentException("Rule not found");
        }
        context.Rules.Remove(rule);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<List<Rule>> GetActiveByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return context.Rules.Where(r => r.TenantId == tenantId && r.IsActive).ToListAsync(cancellationToken);
    }

    public Task<List<Rule>> GetAllRulesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return context.Rules.Where(r => r.TenantId == tenantId).ToListAsync(cancellationToken);
    }

    public Task<Rule?> GetRuleByIdAsync(Guid ruleId, CancellationToken cancellationToken = default)
    {
        return context.Rules.FirstOrDefaultAsync(r => r.Id == ruleId, cancellationToken);
    }

    public Task UpdateRuleAsync(Rule rule, CancellationToken cancellationToken = default)
    {
        context.Rules.Update(rule);
        return context.SaveChangesAsync(cancellationToken);
    }
}