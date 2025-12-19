namespace EcommerceIntegration.Domain.Entities;

/// <summary>
/// Normalized events from external systems - idempotent processing guaranteed.
/// Maps to: integration.external_events
/// </summary>
public class ExternalEvent
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid? WebhookLogId { get; private set; }
    public string IdempotencyKey { get; private set; } = default!;
    public string EventType { get; private set; } = default!;
    public string EventSource { get; private set; } = default!;
    public string SubjectType { get; private set; } = default!;
    public string SubjectId { get; private set; } = default!;
    public string Payload { get; private set; } = default!;  // JSON
    public DateTime OccurredAt { get; private set; }
    public string Status { get; private set; } = default!;
    public DateTime? ProcessedAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation
    public WebhookLog? WebhookLog { get; private set; }

    private ExternalEvent() { } // EF Core constructor

    public static ExternalEvent Create(
        Guid tenantId,
        string idempotencyKey,
        string eventType,
        string eventSource,
        string subjectType,
        string subjectId,
        string payload,
        DateTime occurredAt,
        Guid? webhookLogId = null)
    {
        return new ExternalEvent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            WebhookLogId = webhookLogId,
            IdempotencyKey = idempotencyKey,
            EventType = eventType,
            EventSource = eventSource,
            SubjectType = subjectType,
            SubjectId = subjectId,
            Payload = payload,
            OccurredAt = occurredAt,
            Status = ExternalEventStatus.Pending,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkProcessing()
    {
        Status = ExternalEventStatus.Processing;
    }

    public void MarkProcessed()
    {
        Status = ExternalEventStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string error)
    {
        Status = ExternalEventStatus.Failed;
        ErrorMessage = error;
        ProcessedAt = DateTime.UtcNow;
        RetryCount++;
    }

    public void MarkSkipped()
    {
        Status = ExternalEventStatus.Skipped;
        ProcessedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// External event status constants matching schema CHECK constraint.
/// </summary>
public static class ExternalEventStatus
{
    public const string Pending = "pending";
    public const string Processing = "processing";
    public const string Processed = "processed";
    public const string Failed = "failed";
    public const string Skipped = "skipped";
}
