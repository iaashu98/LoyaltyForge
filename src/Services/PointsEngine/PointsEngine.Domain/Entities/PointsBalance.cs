namespace PointsEngine.Domain.Entities;

/// <summary>
/// Materialized view of customer points balance.
/// Updated after each ledger entry.
/// </summary>
public class PointsBalance
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public int CurrentBalance { get; private set; }
    public int LifetimeEarned { get; private set; }
    public int LifetimeSpent { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }

    private PointsBalance() { } // EF Core constructor

    public static PointsBalance Create(Guid tenantId, Guid customerId)
    {
        return new PointsBalance
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CustomerId = customerId,
            CurrentBalance = 0,
            LifetimeEarned = 0,
            LifetimeSpent = 0,
            LastUpdatedAt = DateTime.UtcNow
        };
    }

    public void Credit(int amount)
    {
        CurrentBalance += amount;
        LifetimeEarned += amount;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void Debit(int amount)
    {
        // TODO: Add validation to prevent negative balance
        CurrentBalance -= amount;
        LifetimeSpent += amount;
        LastUpdatedAt = DateTime.UtcNow;
    }
}
