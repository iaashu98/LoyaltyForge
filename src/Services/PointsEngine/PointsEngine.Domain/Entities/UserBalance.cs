namespace PointsEngine.Domain.Entities;

/// <summary>
/// Cached point balances - DERIVED from ledger, not source of truth.
/// Maps to: points.user_balances
/// </summary>
public class UserBalance
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public long AvailablePoints { get; private set; }
    public long PendingPoints { get; private set; }
    public long LifetimeEarned { get; private set; }
    public long LifetimeRedeemed { get; private set; }
    public Guid? LastLedgerEntryId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private UserBalance() { } // EF Core constructor

    public static UserBalance Create(Guid tenantId, Guid userId)
    {
        return new UserBalance
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            AvailablePoints = 0,
            PendingPoints = 0,
            LifetimeEarned = 0,
            LifetimeRedeemed = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void ApplyEarn(long amount, Guid ledgerEntryId)
    {
        AvailablePoints += amount;
        LifetimeEarned += amount;
        LastLedgerEntryId = ledgerEntryId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ApplyRedeem(long amount, Guid ledgerEntryId)
    {
        AvailablePoints -= amount;
        LifetimeRedeemed += amount;
        LastLedgerEntryId = ledgerEntryId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ApplyExpiry(long amount, Guid ledgerEntryId)
    {
        AvailablePoints -= amount;
        LastLedgerEntryId = ledgerEntryId;
        UpdatedAt = DateTime.UtcNow;
    }
}
