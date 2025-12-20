using LoyaltyForge.Contracts.Events;
using PointsEngine.Domain.Entities;

namespace PointsEngine.Application.Interfaces;

/// <summary>
/// Interface for rules engine that calculates points.
/// Business logic to be implemented by human developer.
/// </summary>
public interface IRuleEngine
{
    /// <summary>
    /// Calculates points to award for an order.
    /// </summary>
    Task<PointsCalculationResult> CalculatePointsAsync(
        Guid tenantId,
        OrderPlacedEvent orderEvent,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of points calculation.
/// </summary>
public record PointsCalculationResult(
    int PointsToAward,
    string Reason,
    IReadOnlyList<string> AppliedRules);
