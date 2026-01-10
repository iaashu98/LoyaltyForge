using LoyaltyForge.Contracts.Events;
using LoyaltyForge.Messaging.RabbitMQ;
using Rewards.Application.Sagas;
using Microsoft.Extensions.Logging;

namespace Rewards.Application.EventHandlers;

/// <summary>
/// Handles PointsDeductionFailedEvent to mark redemption as failed.
/// </summary>
public class PointsDeductionFailedEventHandler : IEventHandler<PointsDeductionFailedEvent>
{
    private readonly RedemptionSaga _redemptionSaga;
    private readonly ILogger<PointsDeductionFailedEventHandler> _logger;

    public PointsDeductionFailedEventHandler(
        RedemptionSaga redemptionSaga,
        ILogger<PointsDeductionFailedEventHandler> logger)
    {
        _redemptionSaga = redemptionSaga;
        _logger = logger;
    }

    public async Task HandleAsync(PointsDeductionFailedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing PointsDeductionFailedEvent for redemption {RedemptionId}: {Reason}",
            @event.RedemptionId,
            @event.FailureReason);

        await _redemptionSaga.HandlePointsDeductionFailedAsync(@event, cancellationToken);
    }
}
