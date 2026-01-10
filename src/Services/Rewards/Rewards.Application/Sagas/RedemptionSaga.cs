using LoyaltyForge.Contracts.Commands;
using LoyaltyForge.Contracts.Events;
using LoyaltyForge.Messaging.RabbitMQ;
using Rewards.Application.Interfaces;
using Rewards.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Rewards.Application.Sagas;

/// <summary>
/// Saga orchestrator for reward redemption flow.
/// Coordinates between Rewards and Points Engine services.
/// </summary>
public class RedemptionSaga
{
    private readonly IRedemptionRepository _redemptionRepository;
    private readonly IRewardRepository _rewardRepository;
    private readonly ICommandPublisher _commandPublisher;
    private readonly ILogger<RedemptionSaga> _logger;

    public RedemptionSaga(
        IRedemptionRepository redemptionRepository,
        IRewardRepository rewardRepository,
        ICommandPublisher commandPublisher,
        ILogger<RedemptionSaga> logger)
    {
        _redemptionRepository = redemptionRepository;
        _rewardRepository = rewardRepository;
        _commandPublisher = commandPublisher;
        _logger = logger;
    }

    /// <summary>
    /// Initiates the redemption saga.
    /// Step 1: Create pending redemption and send DeductPointsCommand.
    /// </summary>
    public async Task<RedemptionSagaResult> StartRedemptionAsync(
        Guid tenantId,
        Guid customerId,
        Guid rewardId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting redemption saga for customer {CustomerId}, reward {RewardId}, idempotency {IdempotencyKey}",
            customerId,
            rewardId,
            idempotencyKey);

        try
        {
            // 1. Check idempotency - prevent duplicate redemptions
            var existingRedemption = await _redemptionRepository.GetByIdempotencyKeyAsync(
                tenantId,
                idempotencyKey,
                cancellationToken);

            if (existingRedemption != null)
            {
                _logger.LogInformation(
                    "Redemption already exists for idempotency key {IdempotencyKey}",
                    idempotencyKey);

                return new RedemptionSagaResult(
                    RedemptionId: existingRedemption.Id,
                    Status: existingRedemption.Status,
                    Success: existingRedemption.Status == "Fulfilled",
                    Error: existingRedemption.Status == "Failed" ? "Redemption failed" : null);
            }

            // 2. Validate reward exists and is active
            var reward = await _rewardRepository.GetByIdAsync(rewardId, cancellationToken);
            if (reward == null)
            {
                return new RedemptionSagaResult(
                    RedemptionId: null,
                    Status: "Failed",
                    Success: false,
                    Error: "Reward not found");
            }

            if (!reward.IsActive)
            {
                return new RedemptionSagaResult(
                    RedemptionId: null,
                    Status: "Failed",
                    Success: false,
                    Error: "Reward is not active");
            }

            // 3. Create pending redemption using factory method
            var redemption = RewardRedemption.Create(
                tenantId,
                customerId,
                rewardId,
                idempotencyKey,
                reward.PointsCost);

            await _redemptionRepository.AddAsync(redemption, cancellationToken);

            // 4. Send DeductPointsCommand to Points Engine
            var deductCommand = new DeductPointsCommand
            {
                CommandId = Guid.NewGuid(),
                TenantId = tenantId,
                CustomerId = customerId,
                Amount = reward.PointsCost,
                RedemptionId = redemption.Id,
                IdempotencyKey = $"redemption-{redemption.Id}",
                Description = $"Reward redemption: {reward.Name}",
                CreatedAt = DateTime.UtcNow,
                CorrelationId = redemption.Id.ToString()
            };

            await _commandPublisher.SendAsync(deductCommand, "points.commands", cancellationToken);

            _logger.LogInformation(
                "Redemption saga started: RedemptionId {RedemptionId}, sent DeductPointsCommand",
                redemption.Id);

            return new RedemptionSagaResult(
                RedemptionId: redemption.Id,
                Status: "Pending",
                Success: false, // Not complete yet
                Error: null);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error starting redemption saga for customer {CustomerId}, reward {RewardId}",
                customerId,
                rewardId);
            throw;
        }
    }

    /// <summary>
    /// Handles successful points deduction.
    /// Step 2: Complete redemption.
    /// </summary>
    public async Task HandlePointsDeductedAsync(
        PointsDeductedEvent @event,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling PointsDeductedEvent for redemption {RedemptionId}",
            @event.RedemptionId);

        var redemption = await _redemptionRepository.GetByIdAsync(
            @event.RedemptionId,
            cancellationToken);

        if (redemption == null)
        {
            _logger.LogWarning(
                "Redemption {RedemptionId} not found for PointsDeductedEvent",
                @event.RedemptionId);
            return;
        }

        if (redemption.Status != "Pending")
        {
            _logger.LogInformation(
                "Redemption {RedemptionId} already in status {Status}, skipping",
                redemption.Id,
                redemption.Status);
            return;
        }

        // Update redemption to fulfilled
        redemption.MarkFulfilled();
        await _redemptionRepository.UpdateAsync(redemption, cancellationToken);

        _logger.LogInformation(
            "Redemption {RedemptionId} completed successfully",
            redemption.Id);
    }

    /// <summary>
    /// Handles failed points deduction.
    /// Step 2 (Failure): Mark redemption as failed.
    /// </summary>
    public async Task HandlePointsDeductionFailedAsync(
        PointsDeductionFailedEvent @event,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling PointsDeductionFailedEvent for redemption {RedemptionId}: {Reason}",
            @event.RedemptionId,
            @event.FailureReason);

        var redemption = await _redemptionRepository.GetByIdAsync(
            @event.RedemptionId,
            cancellationToken);

        if (redemption == null)
        {
            _logger.LogWarning(
                "Redemption {RedemptionId} not found for PointsDeductionFailedEvent",
                @event.RedemptionId);
            return;
        }

        if (redemption.Status != "Pending")
        {
            _logger.LogInformation(
                "Redemption {RedemptionId} already in status {Status}, skipping",
                redemption.Id,
                redemption.Status);
            return;
        }

        // Update redemption to failed
        redemption.MarkFailed();
        await _redemptionRepository.UpdateAsync(redemption, cancellationToken);

        _logger.LogInformation(
            "Redemption {RedemptionId} marked as failed: {Reason}",
            redemption.Id,
            @event.FailureReason);
    }
}

/// <summary>
/// Result of starting a redemption saga.
/// </summary>
public record RedemptionSagaResult(
    Guid? RedemptionId,
    string Status,
    bool Success,
    string? Error);
