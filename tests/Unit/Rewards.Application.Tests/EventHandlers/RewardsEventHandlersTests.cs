using Moq;
using Xunit;
using FluentAssertions;
using Rewards.Application.EventHandlers;
using Rewards.Application.Sagas;
using LoyaltyForge.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace Rewards.Application.Tests.EventHandlers;

public class PointsDeductedEventHandlerTests
{
    private readonly Mock<RedemptionSaga> _sagaMock;
    private readonly Mock<ILogger<PointsDeductedEventHandler>> _loggerMock;
    private readonly PointsDeductedEventHandler _handler;

    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _redemptionId = Guid.NewGuid();

    public PointsDeductedEventHandlerTests()
    {
        _sagaMock = new Mock<RedemptionSaga>(
            null!, null!, null!, null!);
        _loggerMock = new Mock<ILogger<PointsDeductedEventHandler>>();

        _handler = new PointsDeductedEventHandler(
            _sagaMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_CallsSagaHandlePointsDeducted()
    {
        // Arrange
        var @event = new PointsDeductedEvent
        {
            EventId = Guid.NewGuid(),
            TenantId = _tenantId,
            CustomerId = _customerId,
            Amount = 50,
            RedemptionId = _redemptionId,
            NewBalance = 50,
            TransactionId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow
        };

        // Act
        await _handler.HandleAsync(@event);

        // Assert
        _sagaMock.Verify(x => x.HandlePointsDeductedAsync(@event, default), Times.Once);
    }
}

public class PointsDeductionFailedEventHandlerTests
{
    private readonly Mock<RedemptionSaga> _sagaMock;
    private readonly Mock<ILogger<PointsDeductionFailedEventHandler>> _loggerMock;
    private readonly PointsDeductionFailedEventHandler _handler;

    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _redemptionId = Guid.NewGuid();

    public PointsDeductionFailedEventHandlerTests()
    {
        _sagaMock = new Mock<RedemptionSaga>(
            null!, null!, null!, null!);
        _loggerMock = new Mock<ILogger<PointsDeductionFailedEventHandler>>();

        _handler = new PointsDeductionFailedEventHandler(
            _sagaMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_CallsSagaHandlePointsDeductionFailed()
    {
        // Arrange
        var @event = new PointsDeductionFailedEvent
        {
            EventId = Guid.NewGuid(),
            TenantId = _tenantId,
            CustomerId = _customerId,
            RequestedAmount = 500,
            RedemptionId = _redemptionId,
            CurrentBalance = 50,
            FailureReason = "Insufficient balance",
            OccurredAt = DateTime.UtcNow
        };

        // Act
        await _handler.HandleAsync(@event);

        // Assert
        _sagaMock.Verify(x => x.HandlePointsDeductionFailedAsync(@event, default), Times.Once);
    }
}
