namespace Rewards.Domain.Entities;

/// <summary>
/// Represents a reward in the tenant catalog.
/// </summary>
public class Reward
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public int PointsCost { get; private set; }
    public string RewardType { get; private set; } = default!;
    public string? Metadata { get; private set; } // JSON for type-specific data
    public int? StockQuantity { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Reward() { } // EF Core constructor

    public static Reward Create(
        Guid tenantId,
        string name,
        int pointsCost,
        string rewardType,
        string? description = null,
        string? metadata = null,
        int? stockQuantity = null)
    {
        return new Reward
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            PointsCost = pointsCost,
            RewardType = rewardType,
            Description = description,
            Metadata = metadata,
            StockQuantity = stockQuantity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}
