using LoyaltyForge.Contracts.Events;
using LoyaltyForge.Messaging.RabbitMQ;
using PointsEngine.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace PointsEngine.Application.EventHandlers;

/// <summary>
/// Handles OrderPlacedEvent to earn points for customers.
/// </summary>
public class OrderPlacedEventHandler : IEventHandler<OrderPlacedEvent>
{
    private readonly IRuleService _ruleService;
    private readonly ILedgerService _ledgerService;
    private readonly ILogger<OrderPlacedEventHandler> _logger;

    public OrderPlacedEventHandler(
        IRuleService ruleService,
        ILedgerService ledgerService,
        ILogger<OrderPlacedEventHandler> logger)
    {
        _ruleService = ruleService;
        _ledgerService = ledgerService;
        _logger = logger;
    }

    public async Task HandleAsync(OrderPlacedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing OrderPlacedEvent for order {OrderId}, customer {CustomerId}, tenant {TenantId}",
            @event.ExternalOrderId,
            @event.CustomerId,
            @event.TenantId);

        try
        {
            // 1. Get applicable earning rules for this tenant
            var rules = await _ruleService.GetRulesAsync(@event.TenantId, cancellationToken);

            if (!rules.Any())
            {
                _logger.LogWarning(
                    "No active earning rules found for tenant {TenantId}",
                    @event.TenantId);
                return;
            }

            // 2. Calculate points based on order total
            // For now, use simple calculation: 1 point per dollar
            // TODO: Apply complex rule engine logic
            var pointsToEarn = (long)Math.Floor(@event.OrderTotal);

            if (pointsToEarn <= 0)
            {
                _logger.LogInformation(
                    "No points to earn for order {OrderId} (amount: {Amount})",
                    @event.ExternalOrderId,
                    @event.OrderTotal);
                return;
            }

            // 3. Earn points via ledger service
            var earnCommand = new EarnPointsCommand(
                TenantId: @event.TenantId,
                UserId: @event.CustomerId,
                PointsAmount: pointsToEarn,
                SourceType: "Order",
                SourceId: Guid.Parse(@event.ExternalOrderId),
                RuleId: null, // TODO: Apply rule ID from rule service
                IdempotencyKey: @event.EventId.ToString(), // Use event ID for idempotency
                Description: $"Order #{@event.ExternalOrderId} - ${@event.OrderTotal:F2}"
            );

            await _ledgerService.EarnPointsAsync(earnCommand, cancellationToken);

            _logger.LogInformation(
                "Successfully earned {Points} points for customer {CustomerId}, order {OrderId}",
                pointsToEarn,
                @event.CustomerId,
                @event.ExternalOrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing OrderPlacedEvent for order {OrderId}, customer {CustomerId}",
                @event.ExternalOrderId,
                @event.CustomerId);
            throw; // Let RabbitMQ retry
        }
    }
}
