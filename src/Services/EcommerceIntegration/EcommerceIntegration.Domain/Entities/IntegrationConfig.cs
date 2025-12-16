namespace EcommerceIntegration.Domain.Entities;

/// <summary>
/// Stores integration configuration per tenant and platform.
/// </summary>
public class IntegrationConfig
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Platform { get; private set; } = default!;
    public string WebhookSecret { get; private set; } = default!;
    public string? ShopDomain { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private IntegrationConfig() { } // EF Core constructor

    public static IntegrationConfig Create(
        Guid tenantId,
        string platform,
        string webhookSecret,
        string? shopDomain = null)
    {
        return new IntegrationConfig
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Platform = platform,
            WebhookSecret = webhookSecret,
            ShopDomain = shopDomain,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}
