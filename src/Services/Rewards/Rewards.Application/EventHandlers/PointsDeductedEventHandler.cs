using LoyaltyForge.Contracts.Events;
using LoyaltyForge.Messaging.RabbitMQ;
using Rewards.Application.Sagas;
using Microsoft.Extensions.Logging;

namespace Rewards.Application.EventHandlers;

/// <summary>
/// Handles PointsDeductedEvent to complete redemption saga.
/// </summary>
public class PointsDeductedEventHandler : IEventHandler<PointsDeductedEvent>
{
    private readonly RedemptionSaga _redemptionSaga;
    private readonly ILogger<PointsDeductedEventHandler> _logger;

    public PointsDeductedEventHandler(
        RedemptionSaga redemptionSaga,
        ILogger<PointsDeductedEventHandler> logger)
    {
        _redemptionSaga = redemptionSaga;
        _logger = logger;
    }

    public async Task HandleAsync(PointsDeductedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing PointsDeductedEvent for redemption {RedemptionId}",
            @event.RedemptionId);

        await _redemptionSaga.HandlePointsDeductedAsync(@event, cancellationToken);
    }
}
