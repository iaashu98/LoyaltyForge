namespace LoyaltyForge.Common.Interfaces;

/// <summary>
/// Interface for tenant context accessor.
/// </summary>
public interface ITenantContext
{
    Guid TenantId { get; }
    string? TenantName { get; }
    bool IsResolved { get; }
}
