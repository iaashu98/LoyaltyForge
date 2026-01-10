using LoyaltyForge.Contracts.Commands;
using LoyaltyForge.Contracts.Events;
using LoyaltyForge.Messaging.RabbitMQ;
using LoyaltyForge.Common.Outbox;
using PointsEngine.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace PointsEngine.Application.CommandHandlers;

/// <summary>
/// Handles DeductPointsCommand from Rewards service.
/// Deducts points and publishes PointsDeductedEvent or PointsDeductionFailedEvent.
/// </summary>
public class DeductPointsCommandHandler : ICommandHandler<LoyaltyForge.Contracts.Commands.DeductPointsCommand>
{
    private readonly IBalanceService _balanceService;
    private readonly ILedgerService _ledgerService;
    private readonly IOutboxRepository _outboxRepository;
    private readonly ILogger<DeductPointsCommandHandler> _logger;

    public DeductPointsCommandHandler(
        IBalanceService balanceService,
        ILedgerService ledgerService,
        IOutboxRepository outboxRepository,
        ILogger<DeductPointsCommandHandler> logger)
    {
        _balanceService = balanceService;
        _ledgerService = ledgerService;
        _outboxRepository = outboxRepository;
        _logger = logger;
    }

    public async Task<CommandResult> HandleAsync(
        LoyaltyForge.Contracts.Commands.DeductPointsCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing DeductPointsCommand: Customer {CustomerId}, Amount {Amount}, Redemption {RedemptionId}",
            command.CustomerId,
            command.Amount,
            command.RedemptionId);

        try
        {
            // 1. Get current balance
            var balanceResult = await _balanceService.GetBalanceAsync(
                command.TenantId,
                command.CustomerId,
                cancellationToken);

            var currentBalance = balanceResult.AvailablePoints;

            // 2. Check if sufficient balance
            if (currentBalance < command.Amount)
            {
                _logger.LogWarning(
                    "Insufficient balance for customer {CustomerId}: Required {Required}, Available {Available}",
                    command.CustomerId,
                    command.Amount,
                    currentBalance);

                // Publish PointsDeductionFailedEvent
                var failedEvent = new PointsDeductionFailedEvent
                {
                    EventId = Guid.NewGuid(),
                    TenantId = command.TenantId,
                    CustomerId = command.CustomerId,
                    RequestedAmount = command.Amount,
                    RedemptionId = command.RedemptionId,
                    CurrentBalance = currentBalance,
                    FailureReason = $"Insufficient balance. Required: {command.Amount}, Available: {currentBalance}",
                    OccurredAt = DateTime.UtcNow
                };

                await _outboxRepository.AddAsync(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    EventType = failedEvent.EventType,
                    Payload = System.Text.Json.JsonSerializer.Serialize(failedEvent),
                    TenantId = command.TenantId,
                    CreatedAt = DateTime.UtcNow
                }, cancellationToken);

                _logger.LogInformation(
                    "Published PointsDeductionFailedEvent for redemption {RedemptionId}",
                    command.RedemptionId);

                return new CommandResult(false, "Insufficient balance");
            }

            // 3. Deduct points via ledger service
            var deductCommand = new PointsEngine.Application.Interfaces.DeductPointsCommand(
                TenantId: command.TenantId,
                UserId: command.CustomerId,
                PointsAmount: command.Amount,
                SourceType: "Redemption",
                SourceId: command.RedemptionId,
                IdempotencyKey: command.IdempotencyKey,
                Description: command.Description
            );

            var ledgerResult = await _ledgerService.DeductPointsAsync(deductCommand, cancellationToken);

            if (!ledgerResult.Success)
            {
                _logger.LogWarning(
                    "Points deduction failed for customer {CustomerId}: {Error}",
                    command.CustomerId,
                    ledgerResult.Error);

                // Publish PointsDeductionFailedEvent
                var failedEvent = new PointsDeductionFailedEvent
                {
                    EventId = Guid.NewGuid(),
                    TenantId = command.TenantId,
                    CustomerId = command.CustomerId,
                    RequestedAmount = command.Amount,
                    RedemptionId = command.RedemptionId,
                    CurrentBalance = currentBalance,
                    FailureReason = ledgerResult.Error ?? "Unknown error",
                    OccurredAt = DateTime.UtcNow
                };

                await _outboxRepository.AddAsync(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    EventType = failedEvent.EventType,
                    Payload = System.Text.Json.JsonSerializer.Serialize(failedEvent),
                    TenantId = command.TenantId,
                    CreatedAt = DateTime.UtcNow
                }, cancellationToken);

                return new CommandResult(false, ledgerResult.Error);
            }

            // 4. Publish PointsDeductedEvent
            var successEvent = new PointsDeductedEvent
            {
                EventId = Guid.NewGuid(),
                TenantId = command.TenantId,
                CustomerId = command.CustomerId,
                Amount = command.Amount,
                RedemptionId = command.RedemptionId,
                NewBalance = ledgerResult.BalanceAfter,
                TransactionId = ledgerResult.LedgerEntryId ?? Guid.Empty,
                OccurredAt = DateTime.UtcNow
            };

            await _outboxRepository.AddAsync(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = successEvent.EventType,
                Payload = System.Text.Json.JsonSerializer.Serialize(successEvent),
                TenantId = command.TenantId,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);

            _logger.LogInformation(
                "Successfully deducted {Amount} points for customer {CustomerId}, redemption {RedemptionId}. New balance: {NewBalance}",
                command.Amount,
                command.CustomerId,
                command.RedemptionId,
                ledgerResult.BalanceAfter);

            return new CommandResult(true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing DeductPointsCommand for customer {CustomerId}, redemption {RedemptionId}",
                command.CustomerId,
                command.RedemptionId);
            return new CommandResult(false, ex.Message);
        }
    }
}
