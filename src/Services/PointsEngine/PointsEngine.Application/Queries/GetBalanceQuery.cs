namespace PointsEngine.Application.Queries;

/// <summary>
/// Query to get customer points balance.
/// </summary>
public record GetBalanceQuery(Guid TenantId, Guid CustomerId);

/// <summary>
/// Result of balance query.
/// </summary>
public record GetBalanceResult(
    Guid CustomerId,
    int CurrentBalance,
    int LifetimeEarned,
    int LifetimeSpent,
    DateTime LastUpdatedAt);
