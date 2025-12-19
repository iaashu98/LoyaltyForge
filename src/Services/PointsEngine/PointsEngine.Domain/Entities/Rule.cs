namespace PointsEngine.Domain.Entities;

/// <summary>
/// Defines point earning rules - stored as structured JSON for flexibility.
/// Maps to: points.rules
/// </summary>
public class Rule
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public string EventType { get; private set; } = default!;
    public string RuleDefinition { get; private set; } = default!;  // JSON
    public int Priority { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime? ValidUntil { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }

    private Rule() { } // EF Core constructor

    public static Rule Create(
        Guid tenantId,
        string name,
        string eventType,
        string ruleDefinition,
        int priority = 0,
        string? description = null,
        DateTime? validFrom = null,
        DateTime? validUntil = null,
        Guid? createdBy = null)
    {
        return new Rule
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Description = description,
            EventType = eventType,
            RuleDefinition = ruleDefinition,
            Priority = priority,
            IsActive = true,
            ValidFrom = validFrom ?? DateTime.UtcNow,
            ValidUntil = validUntil,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRuleDefinition(string ruleDefinition)
    {
        RuleDefinition = ruleDefinition;
        UpdatedAt = DateTime.UtcNow;
    }
}
