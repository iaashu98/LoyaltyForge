namespace LoyaltyForge.Audit.Entities;

/// <summary>
/// Lightweight audit log for system events.
/// Maps to: audit.system_events
/// </summary>
public class SystemEvent
{
    public Guid Id { get; private set; }
    public Guid? TenantId { get; private set; }
    public Guid? UserId { get; private set; }
    public string EventType { get; private set; } = default!;
    public string Source { get; private set; } = default!;
    public string? SubjectType { get; private set; }
    public string? SubjectId { get; private set; }
    public string? Details { get; private set; }  // JSON
    public DateTime CreatedAt { get; private set; }

    private SystemEvent() { } // EF Core constructor

    public static SystemEvent Create(
        string eventType,
        string source,
        Guid? tenantId = null,
        Guid? userId = null,
        string? subjectType = null,
        string? subjectId = null,
        string? details = null)
    {
        return new SystemEvent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            EventType = eventType,
            Source = source,
            SubjectType = subjectType,
            SubjectId = subjectId,
            Details = details,
            CreatedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Event type constants for audit logging.
/// </summary>
public static class AuditEventTypes
{
    public const string TenantCreated = "tenant.created";
    public const string TenantUpdated = "tenant.updated";
    public const string UserCreated = "user.created";
    public const string UserLogin = "user.login";
    public const string PointsEarned = "points.earned";
    public const string PointsRedeemed = "points.redeemed";
    public const string RewardRedeemed = "reward.redeemed";
    public const string ApiKeyCreated = "api_key.created";
    public const string ApiKeyRevoked = "api_key.revoked";
}
