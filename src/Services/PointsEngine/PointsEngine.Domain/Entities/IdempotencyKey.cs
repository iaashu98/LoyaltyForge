namespace PointsEngine.Domain.Entities;

/// <summary>
/// General idempotency store - prevents duplicate operations.
/// Maps to: points.idempotency_keys
/// </summary>
public class IdempotencyKey
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Key { get; private set; } = default!;
    public string OperationType { get; private set; } = default!;
    public string Status { get; private set; } = default!;
    public string? Result { get; private set; }  // JSON
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    private IdempotencyKey() { } // EF Core constructor

    public static IdempotencyKey Create(
        Guid tenantId,
        string key,
        string operationType,
        TimeSpan? ttl = null)
    {
        return new IdempotencyKey
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Key = key,
            OperationType = operationType,
            Status = IdempotencyStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(ttl ?? TimeSpan.FromDays(7))
        };
    }

    public void MarkCompleted(string? result = null)
    {
        Status = IdempotencyStatus.Completed;
        Result = result;
    }

    public void MarkFailed()
    {
        Status = IdempotencyStatus.Failed;
    }
}

/// <summary>
/// Idempotency status constants.
/// </summary>
public static class IdempotencyStatus
{
    public const string Pending = "pending";
    public const string Completed = "completed";
    public const string Failed = "failed";
}
