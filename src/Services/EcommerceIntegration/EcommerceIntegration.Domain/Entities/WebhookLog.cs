namespace EcommerceIntegration.Domain.Entities;

/// <summary>
/// Records all incoming webhook requests for audit and replay.
/// Maps to: integration.webhook_logs
/// </summary>
public class WebhookLog
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Source { get; private set; } = default!;
    public string Topic { get; private set; } = default!;
    public string? WebhookId { get; private set; }
    public string Headers { get; private set; } = default!;  // JSON
    public string Payload { get; private set; } = default!;  // JSON
    public string? Signature { get; private set; }
    public string Status { get; private set; } = default!;
    public string? ErrorMessage { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation
    public ICollection<ExternalEvent> ExternalEvents { get; private set; } = new List<ExternalEvent>();

    private WebhookLog() { } // EF Core constructor

    public static WebhookLog Create(
        Guid tenantId,
        string source,
        string topic,
        string headers,
        string payload,
        string? signature = null,
        string? webhookId = null)
    {
        return new WebhookLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Source = source,
            Topic = topic,
            WebhookId = webhookId,
            Headers = headers,
            Payload = payload,
            Signature = signature,
            Status = WebhookStatus.Received,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkProcessing()
    {
        Status = WebhookStatus.Processing;
    }

    public void MarkProcessed()
    {
        Status = WebhookStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string error)
    {
        Status = WebhookStatus.Failed;
        ErrorMessage = error;
        ProcessedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Webhook status constants matching schema CHECK constraint.
/// </summary>
public static class WebhookStatus
{
    public const string Received = "received";
    public const string Processing = "processing";
    public const string Processed = "processed";
    public const string Failed = "failed";
}
