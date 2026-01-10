namespace LoyaltyForge.Contracts.Commands;

/// <summary>
/// Base class for all integration commands.
/// Commands represent requests for actions to be performed by a specific service.
/// </summary>
public abstract record IntegrationCommand
{
    /// <summary>
    /// Unique identifier for this command instance.
    /// </summary>
    public Guid CommandId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when the command was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Type name of the command (derived from class name).
    /// </summary>
    public string CommandType => GetType().Name;

    /// <summary>
    /// Tenant ID for multi-tenancy isolation.
    /// </summary>
    public Guid TenantId { get; init; }

    /// <summary>
    /// Correlation ID for distributed tracing across services.
    /// </summary>
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}
