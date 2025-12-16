namespace EcommerceIntegration.Domain.Entities;

/// <summary>
/// Stores webhook events for auditing and replay purposes.
/// </summary>
public class WebhookEvent
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Platform { get; private set; } = default!;
    public string EventType { get; private set; } = default!;
    public string ExternalEventId { get; private set; } = default!;
    public string RawPayload { get; private set; } = default!;
    public bool IsProcessed { get; private set; }
    public DateTime ReceivedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? ProcessingError { get; private set; }

    private WebhookEvent() { } // EF Core constructor

    public static WebhookEvent Create(
        Guid tenantId,
        string platform,
        string eventType,
        string externalEventId,
        string rawPayload)
    {
        return new WebhookEvent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Platform = platform,
            EventType = eventType,
            ExternalEventId = externalEventId,
            RawPayload = rawPayload,
            IsProcessed = false,
            ReceivedAt = DateTime.UtcNow
        };
    }

    public void MarkProcessed()
    {
        IsProcessed = true;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string error)
    {
        ProcessingError = error;
        ProcessedAt = DateTime.UtcNow;
    }
}
