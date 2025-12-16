namespace Rewards.Application.Interfaces;

/// <summary>
/// HTTP client interface for Points Engine service.
/// </summary>
public interface IPointsServiceClient
{
    Task<PointsDeductionResult> DeductPointsAsync(
        Guid tenantId,
        Guid customerId,
        int amount,
        string reason,
        string idempotencyKey,
        CancellationToken cancellationToken = default);
    
    Task<CustomerBalanceResult> GetBalanceAsync(
        Guid tenantId,
        Guid customerId,
        CancellationToken cancellationToken = default);
}

public record PointsDeductionResult(bool Success, Guid? TransactionId, int? NewBalance, string? Error);
public record CustomerBalanceResult(int CurrentBalance, int LifetimeEarned, int LifetimeSpent);
