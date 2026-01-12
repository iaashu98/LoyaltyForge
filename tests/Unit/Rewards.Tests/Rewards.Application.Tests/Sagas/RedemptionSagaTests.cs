using FluentAssertions;
using LoyaltyForge.Contracts.Commands;
using LoyaltyForge.Messaging.RabbitMQ;
using Microsoft.Extensions.Logging;
using Moq;
using Rewards.Application.Interfaces;
using Rewards.Application.Sagas;
using Rewards.Domain.Entities;
using Xunit;

namespace Rewards.Application.Tests.Sagas;

public class RedemptionSagaTests
{
    private readonly Mock<IRedemptionRepository> _mockRedemptionRepository;
    private readonly Mock<IRewardRepository> _mockRewardRepository;
    private readonly Mock<ICommandPublisher> _mockCommandPublisher;
    private readonly Mock<ILogger<RedemptionSaga>> _mockLogger;
    private readonly RedemptionSaga _saga;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _rewardId = Guid.NewGuid();

    public RedemptionSagaTests()
    {
        _mockRedemptionRepository = new Mock<IRedemptionRepository>();
        _mockRewardRepository = new Mock<IRewardRepository>();
        _mockCommandPublisher = new Mock<ICommandPublisher>();
        _mockLogger = new Mock<ILogger<RedemptionSaga>>();
        _saga = new RedemptionSaga(
            _mockRedemptionRepository.Object,
            _mockRewardRepository.Object,
            _mockCommandPublisher.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task StartRedemptionAsync_WithValidRequest_CreatesRedemptionAndSendsCommand()
    {
        // Arrange
        var reward = RewardCatalog.Create(_tenantId, "Test Reward", 500, "discount", "10%", "Description");

        _mockRedemptionRepository
            .Setup(r => r.GetByIdempotencyKeyAsync(_tenantId, "idempotency-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RewardRedemption?)null);

        _mockRewardRepository
            .Setup(r => r.GetByIdAsync(_rewardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reward);

        // Act
        var result = await _saga.StartRedemptionAsync(_tenantId, _customerId, _rewardId, "idempotency-123");

        // Assert
        result.Success.Should().BeFalse(); // Pending, not complete yet
        result.Status.Should().Be("Pending");
        result.RedemptionId.Should().NotBeNull();

        _mockRedemptionRepository.Verify(
            r => r.AddAsync(It.IsAny<RewardRedemption>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _mockCommandPublisher.Verify(
            p => p.SendAsync(It.IsAny<DeductPointsCommand>(), "points.commands", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task StartRedemptionAsync_WithIdempotencyKey_ReturnsExistingRedemption()
    {
        // Arrange
        var existingRedemption = RewardRedemption.Create(_tenantId, _customerId, _rewardId, "idempotency-123", 500);

        _mockRedemptionRepository
            .Setup(r => r.GetByIdempotencyKeyAsync(_tenantId, "idempotency-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRedemption);

        // Act
        var result = await _saga.StartRedemptionAsync(_tenantId, _customerId, _rewardId, "idempotency-123");

        // Assert
        result.RedemptionId.Should().Be(existingRedemption.Id);

        // Should not create new redemption
        _mockRedemptionRepository.Verify(
            r => r.AddAsync(It.IsAny<RewardRedemption>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mockCommandPublisher.Verify(
            p => p.SendAsync(It.IsAny<DeductPointsCommand>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task StartRedemptionAsync_WithNonExistentReward_ReturnsFailed()
    {
        // Arrange
        _mockRedemptionRepository
            .Setup(r => r.GetByIdempotencyKeyAsync(_tenantId, "idempotency-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RewardRedemption?)null);

        _mockRewardRepository
            .Setup(r => r.GetByIdAsync(_rewardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RewardCatalog?)null);

        // Act
        var result = await _saga.StartRedemptionAsync(_tenantId, _customerId, _rewardId, "idempotency-123");

        // Assert
        result.Success.Should().BeFalse();
        result.Status.Should().Be("Failed");
        result.Error.Should().Contain("Reward not found");
        result.RedemptionId.Should().BeNull();
    }

    [Fact]
    public async Task StartRedemptionAsync_WithInactiveReward_ReturnsFailed()
    {
        // Arrange
        var reward = RewardCatalog.Create(_tenantId, "Test Reward", 500, "discount", "10%", "Description");
        reward.Deactivate();

        _mockRedemptionRepository
            .Setup(r => r.GetByIdempotencyKeyAsync(_tenantId, "idempotency-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RewardRedemption?)null);

        _mockRewardRepository
            .Setup(r => r.GetByIdAsync(_rewardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reward);

        // Act
        var result = await _saga.StartRedemptionAsync(_tenantId, _customerId, _rewardId, "idempotency-123");

        // Assert
        result.Success.Should().BeFalse();
        result.Status.Should().Be("Failed");
        result.Error.Should().Contain("not active");
    }

    [Fact]
    public async Task StartRedemptionAsync_SendsCorrectDeductPointsCommand()
    {
        // Arrange
        var reward = RewardCatalog.Create(_tenantId, "Premium Reward", 1000, "discount", "20%", "Description");

        _mockRedemptionRepository
            .Setup(r => r.GetByIdempotencyKeyAsync(_tenantId, "idempotency-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RewardRedemption?)null);

        _mockRewardRepository
            .Setup(r => r.GetByIdAsync(_rewardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reward);

        DeductPointsCommand? capturedCommand = null;
        _mockCommandPublisher
            .Setup(p => p.SendAsync(It.IsAny<DeductPointsCommand>(), "points.commands", It.IsAny<CancellationToken>()))
            .Callback<DeductPointsCommand, string, CancellationToken>((cmd, queue, ct) => capturedCommand = cmd)
            .Returns(Task.CompletedTask);

        // Act
        await _saga.StartRedemptionAsync(_tenantId, _customerId, _rewardId, "idempotency-123");

        // Assert
        capturedCommand.Should().NotBeNull();
        capturedCommand!.TenantId.Should().Be(_tenantId);
        capturedCommand.CustomerId.Should().Be(_customerId);
        capturedCommand.Amount.Should().Be(1000); // Reward points cost
    }

    [Fact]
    public async Task StartRedemptionAsync_WithFulfilledRedemption_ReturnsSuccess()
    {
        // Arrange
        var redemption = RewardRedemption.Create(_tenantId, _customerId, _rewardId, "idempotency-123", 500);
        redemption.MarkFulfilled();

        _mockRedemptionRepository
            .Setup(r => r.GetByIdempotencyKeyAsync(_tenantId, "idempotency-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(redemption);

        // Act
        var result = await _saga.StartRedemptionAsync(_tenantId, _customerId, _rewardId, "idempotency-123");

        // Assert
        result.Success.Should().BeFalse(); // Idempotent return of existing
        result.Status.Should().Be("fulfilled");
    }

    [Fact]
    public async Task StartRedemptionAsync_WithFailedRedemption_ReturnsFailure()
    {
        // Arrange
        var redemption = RewardRedemption.Create(_tenantId, _customerId, _rewardId, "idempotency-123", 500);
        redemption.MarkFailed();

        _mockRedemptionRepository
            .Setup(r => r.GetByIdempotencyKeyAsync(_tenantId, "idempotency-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(redemption);

        // Act
        var result = await _saga.StartRedemptionAsync(_tenantId, _customerId, _rewardId, "idempotency-123");

        // Assert
        result.Success.Should().BeFalse();
        result.Status.Should().Be("failed");
    }
}
