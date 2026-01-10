using Moq;
using Xunit;
using FluentAssertions;
using Rewards.Application.Sagas;
using Rewards.Application.Interfaces;
using Rewards.Domain.Entities;
using LoyaltyForge.Messaging.RabbitMQ;
using LoyaltyForge.Contracts.Commands;
using LoyaltyForge.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace Rewards.Application.Tests.Sagas;

public class RedemptionSagaTests
{
    private readonly Mock<IRedemptionRepository> _redemptionRepoMock;
    private readonly Mock<IRewardRepository> _rewardRepoMock;
    private readonly Mock<ICommandPublisher> _commandPublisherMock;
    private readonly Mock<ILogger<RedemptionSaga>> _loggerMock;
    private readonly RedemptionSaga _saga;

    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _rewardId = Guid.NewGuid();

    public RedemptionSagaTests()
    {
        _redemptionRepoMock = new Mock<IRedemptionRepository>();
        _rewardRepoMock = new Mock<IRewardRepository>();
        _commandPublisherMock = new Mock<ICommandPublisher>();
        _loggerMock = new Mock<ILogger<RedemptionSaga>>();

        _saga = new RedemptionSaga(
            _redemptionRepoMock.Object,
            _rewardRepoMock.Object,
            _commandPublisherMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task StartRedemptionAsync_WithValidData_CreatesRedemptionAndSendsCommand()
    {
        // Arrange
        var reward = CreateActiveReward(pointsCost: 50);
        _rewardRepoMock.Setup(x => x.GetByIdAsync(_rewardId, default))
            .ReturnsAsync(reward);
        _redemptionRepoMock.Setup(x => x.GetByIdempotencyKeyAsync(_tenantId, "key-1", default))
            .ReturnsAsync((RewardRedemption?)null);

        // Act
        var result = await _saga.StartRedemptionAsync(_tenantId, _customerId, _rewardId, "key-1");

        // Assert
        result.Should().NotBeNull();
        result.RedemptionId.Should().NotBeNull();
        result.Status.Should().Be("Pending");
        result.Success.Should().BeFalse(); // Not complete yet

        _redemptionRepoMock.Verify(x => x.AddAsync(
            It.Is<RewardRedemption>(r => r.Status == "Pending" && r.UserId == _customerId),
            default), Times.Once);

        _commandPublisherMock.Verify(x => x.SendAsync(
            It.Is<DeductPointsCommand>(cmd =>
                cmd.CustomerId == _customerId &&
                cmd.Amount == 50 &&
                cmd.TenantId == _tenantId),
            "points.commands",
            default), Times.Once);
    }

    [Fact]
    public async Task StartRedemptionAsync_WithDuplicateIdempotencyKey_ReturnsExisting()
    {
        // Arrange
        var existingRedemption = CreateRedemption("Fulfilled");
        _redemptionRepoMock.Setup(x => x.GetByIdempotencyKeyAsync(_tenantId, "key-1", default))
            .ReturnsAsync(existingRedemption);

        // Act
        var result = await _saga.StartRedemptionAsync(_tenantId, _customerId, _rewardId, "key-1");

        // Assert
        result.RedemptionId.Should().Be(existingRedemption.Id);
        result.Status.Should().Be("fulfilled");
        result.Success.Should().BeTrue();

        _redemptionRepoMock.Verify(x => x.AddAsync(It.IsAny<RewardRedemption>(), default), Times.Never);
        _commandPublisherMock.Verify(x => x.SendAsync(It.IsAny<DeductPointsCommand>(), It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task StartRedemptionAsync_WithNonExistentReward_ReturnsFailed()
    {
        // Arrange
        _rewardRepoMock.Setup(x => x.GetByIdAsync(_rewardId, default))
            .ReturnsAsync((RewardCatalog?)null);
        _redemptionRepoMock.Setup(x => x.GetByIdempotencyKeyAsync(_tenantId, "key-1", default))
            .ReturnsAsync((RewardRedemption?)null);

        // Act
        var result = await _saga.StartRedemptionAsync(_tenantId, _customerId, _rewardId, "key-1");

        // Assert
        result.RedemptionId.Should().BeNull();
        result.Status.Should().Be("Failed");
        result.Success.Should().BeFalse();
        result.Error.Should().Be("Reward not found");

        _redemptionRepoMock.Verify(x => x.AddAsync(It.IsAny<RewardRedemption>(), default), Times.Never);
    }

    [Fact]
    public async Task StartRedemptionAsync_WithInactiveReward_ReturnsFailed()
    {
        // Arrange
        var inactiveReward = CreateInactiveReward();
        _rewardRepoMock.Setup(x => x.GetByIdAsync(_rewardId, default))
            .ReturnsAsync(inactiveReward);
        _redemptionRepoMock.Setup(x => x.GetByIdempotencyKeyAsync(_tenantId, "key-1", default))
            .ReturnsAsync((RewardRedemption?)null);

        // Act
        var result = await _saga.StartRedemptionAsync(_tenantId, _customerId, _rewardId, "key-1");

        // Assert
        result.RedemptionId.Should().BeNull();
        result.Status.Should().Be("Failed");
        result.Success.Should().BeFalse();
        result.Error.Should().Be("Reward is not active");
    }

    [Fact]
    public async Task HandlePointsDeductedAsync_UpdatesRedemptionToFulfilled()
    {
        // Arrange
        var redemptionId = Guid.NewGuid();
        var redemption = CreateRedemption("Pending", redemptionId);
        var @event = new PointsDeductedEvent
        {
            EventId = Guid.NewGuid(),
            TenantId = _tenantId,
            CustomerId = _customerId,
            Amount = 50,
            RedemptionId = redemptionId,
            NewBalance = 50,
            TransactionId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow
        };

        _redemptionRepoMock.Setup(x => x.GetByIdAsync(redemptionId, default))
            .ReturnsAsync(redemption);

        // Act
        await _saga.HandlePointsDeductedAsync(@event);

        // Assert
        _redemptionRepoMock.Verify(x => x.UpdateAsync(
            It.Is<RewardRedemption>(r => r.Status == "fulfilled"),
            default), Times.Once);
    }

    [Fact]
    public async Task HandlePointsDeductedAsync_WithNonPendingRedemption_SkipsUpdate()
    {
        // Arrange
        var redemptionId = Guid.NewGuid();
        var redemption = CreateRedemption("Fulfilled", redemptionId);
        var @event = new PointsDeductedEvent
        {
            EventId = Guid.NewGuid(),
            TenantId = _tenantId,
            CustomerId = _customerId,
            Amount = 50,
            RedemptionId = redemptionId,
            NewBalance = 50,
            TransactionId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow
        };

        _redemptionRepoMock.Setup(x => x.GetByIdAsync(redemptionId, default))
            .ReturnsAsync(redemption);

        // Act
        await _saga.HandlePointsDeductedAsync(@event);

        // Assert
        _redemptionRepoMock.Verify(x => x.UpdateAsync(It.IsAny<RewardRedemption>(), default), Times.Never);
    }

    [Fact]
    public async Task HandlePointsDeductionFailedAsync_UpdatesRedemptionToFailed()
    {
        // Arrange
        var redemptionId = Guid.NewGuid();
        var redemption = CreateRedemption("Pending", redemptionId);
        var @event = new PointsDeductionFailedEvent
        {
            EventId = Guid.NewGuid(),
            TenantId = _tenantId,
            CustomerId = _customerId,
            RequestedAmount = 500,
            RedemptionId = redemptionId,
            CurrentBalance = 50,
            FailureReason = "Insufficient balance",
            OccurredAt = DateTime.UtcNow
        };

        _redemptionRepoMock.Setup(x => x.GetByIdAsync(redemptionId, default))
            .ReturnsAsync(redemption);

        // Act
        await _saga.HandlePointsDeductionFailedAsync(@event);

        // Assert
        _redemptionRepoMock.Verify(x => x.UpdateAsync(
            It.Is<RewardRedemption>(r => r.Status == "failed"),
            default), Times.Once);
    }

    // Helper methods
    private RewardCatalog CreateActiveReward(long pointsCost)
    {
        return RewardCatalog.Create(
            _tenantId,
            "Test Reward",
            pointsCost,
            "Discount",
            "50",
            "Test Description");
    }

    private RewardCatalog CreateInactiveReward()
    {
        var reward = CreateActiveReward(50);
        reward.Deactivate();
        return reward;
    }

    private RewardRedemption CreateRedemption(string status, Guid? id = null)
    {
        var redemption = RewardRedemption.Create(
            _tenantId,
            _customerId,
            _rewardId,
            "key-1",
            50);

        if (status == "Fulfilled")
        {
            redemption.MarkFulfilled();
        }
        else if (status == "Failed")
        {
            redemption.MarkFailed();
        }

        return redemption;
    }
}
