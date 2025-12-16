namespace PointsEngine.Domain.Entities;

/// <summary>
/// Represents a points calculation rule for a tenant.
/// Business logic to be implemented by human developer.
/// </summary>
public class PointsRule
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public string RuleType { get; private set; } = default!;
    public string Configuration { get; private set; } = default!; // JSON configuration
    public int Priority { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private PointsRule() { } // EF Core constructor

    public static PointsRule Create(
        Guid tenantId,
        string name,
        string ruleType,
        string configuration,
        int priority = 0,
        string? description = null)
    {
        return new PointsRule
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            RuleType = ruleType,
            Configuration = configuration,
            Priority = priority,
            Description = description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}
