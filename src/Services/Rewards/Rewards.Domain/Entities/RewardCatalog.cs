namespace Rewards.Domain.Entities;

/// <summary>
/// Available rewards that customers can redeem.
/// Maps to: rewards.catalog
/// </summary>
public class RewardCatalog
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public long PointsCost { get; private set; }
    public string RewardType { get; private set; } = default!;
    public string RewardValue { get; private set; } = default!;  // JSON
    public bool IsLimited { get; private set; }
    public int? TotalQuantity { get; private set; }
    public int? RemainingQuantity { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime? ValidUntil { get; private set; }
    public int? MaxPerUser { get; private set; }
    public string? ImageUrl { get; private set; }
    public int DisplayOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private RewardCatalog() { } // EF Core constructor

    public static RewardCatalog Create(
        Guid tenantId,
        string name,
        long pointsCost,
        string rewardType,
        string rewardValue,
        string? description = null,
        int? totalQuantity = null,
        int? maxPerUser = null,
        string? imageUrl = null)
    {
        var isLimited = totalQuantity.HasValue;

        return new RewardCatalog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Description = description,
            PointsCost = pointsCost,
            RewardType = rewardType,
            RewardValue = rewardValue,
            IsLimited = isLimited,
            TotalQuantity = isLimited ? totalQuantity : null,
            RemainingQuantity = isLimited ? totalQuantity : null,
            IsActive = true,
            ValidFrom = DateTime.UtcNow,
            MaxPerUser = maxPerUser,
            ImageUrl = imageUrl,
            DisplayOrder = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Update reward details
    /// </summary>
    public void Update(
        string? name = null,
        long? pointsCost = null,
        string? rewardType = null,
        string? rewardValue = null,
        string? description = null,
        int? totalQuantity = null,
        int? maxPerUser = null,
        string? imageUrl = null)
    {
        if (name is not null)
            Name = name;
        if (pointsCost is not null)
            PointsCost = pointsCost.Value;
        if (rewardType is not null)
            RewardType = rewardType;
        if (rewardValue is not null)
            RewardValue = rewardValue;
        if (description is not null)
            Description = description;
        if (totalQuantity is not null)
            TotalQuantity = totalQuantity.Value;
        if (maxPerUser is not null)
            MaxPerUser = maxPerUser.Value;
        if (imageUrl is not null)
            ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Decrements inventory. Should be called with row-level lock (SELECT FOR UPDATE).
    /// </summary>
    public bool TryDecrementInventory()
    {
        if (!IsLimited)
            return true;

        if (RemainingQuantity <= 0)
            return false;

        RemainingQuantity--;
        UpdatedAt = DateTime.UtcNow;
        return true;
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
}

/// <summary>
/// Reward type constants matching schema CHECK constraint.
/// </summary>
public static class RewardTypes
{
    public const string Discount = "discount";
    public const string Product = "product";
    public const string GiftCard = "gift_card";
    public const string Custom = "custom";
}
