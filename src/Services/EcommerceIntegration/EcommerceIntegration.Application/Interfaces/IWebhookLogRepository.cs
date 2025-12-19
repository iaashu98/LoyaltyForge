using EcommerceIntegration.Domain.Entities;

namespace EcommerceIntegration.Application.Interfaces;

/// <summary>
/// Repository interface for WebhookLog operations.
/// </summary>
public interface IWebhookLogRepository
{
    Task<WebhookLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(WebhookLog log, CancellationToken cancellationToken = default);
    Task UpdateAsync(WebhookLog log, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for ExternalEvent operations.
/// </summary>
public interface IExternalEventRepository
{
    Task<ExternalEvent?> GetByIdempotencyKeyAsync(
        Guid tenantId,
        string idempotencyKey,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExternalEvent>> GetPendingAsync(
        Guid tenantId,
        int limit = 100,
        CancellationToken cancellationToken = default);
    Task AddAsync(ExternalEvent evt, CancellationToken cancellationToken = default);
    Task UpdateAsync(ExternalEvent evt, CancellationToken cancellationToken = default);
}
