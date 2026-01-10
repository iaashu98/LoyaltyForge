using Moq;
using Xunit;
using FluentAssertions;
using PointsEngine.Application.CommandHandlers;
using PointsEngine.Application.Interfaces;
using LoyaltyForge.Contracts.Commands;
using LoyaltyForge.Common.Outbox;
using LoyaltyForge.Messaging.RabbitMQ;
using Microsoft.Extensions.Logging;

namespace PointsEngine.Application.Tests.CommandHandlers;

public class DeductPointsCommandHandlerTests
{
    private readonly Mock<IBalanceService> _balanceServiceMock;
    private readonly Mock<ILedgerService> _ledgerServiceMock;
    private readonly Mock<IOutboxRepository> _outboxRepoMock;
    private readonly Mock<ILogger<DeductPointsCommandHandler>> _loggerMock;
    private readonly DeductPointsCommandHandler _handler;

    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _redemptionId = Guid.NewGuid();

    public DeductPointsCommandHandlerTests()
    {
        _balanceServiceMock = new Mock<IBalanceService>();
        _ledgerServiceMock = new Mock<ILedgerService>();
        _outboxRepoMock = new Mock<IOutboxRepository>();
        _loggerMock = new Mock<ILogger<DeductPointsCommandHandler>>();

        _handler = new DeductPointsCommandHandler(
            _balanceServiceMock.Object,
            _ledgerServiceMock.Object,
            _outboxRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithSufficientBalance_DeductsPointsAndPublishesSuccessEvent()
    {
        // Arrange
        var command = CreateDeductPointsCommand(amount: 50);
        var balanceResult = new BalanceResult(_customerId, 100, 0, 100, 0, DateTime.UtcNow);
        var ledgerResult = new LedgerResult(
            Success: true,
            LedgerEntryId: Guid.NewGuid(),
            BalanceAfter: 50,
            Error: null);

        _balanceServiceMock.Setup(x => x.GetBalanceAsync(_tenantId, _customerId, default))
            .ReturnsAsync(balanceResult);
        _ledgerServiceMock.Setup(x => x.DeductPointsAsync(It.IsAny<PointsEngine.Application.Interfaces.DeductPointsCommand>(), default))
            .ReturnsAsync(ledgerResult);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Success.Should().BeTrue();
        result.Error.Should().BeNull();

        _ledgerServiceMock.Verify(x => x.DeductPointsAsync(
            It.Is<PointsEngine.Application.Interfaces.DeductPointsCommand>(cmd =>
                cmd.UserId == _customerId &&
                cmd.PointsAmount == 50),
            default), Times.Once);

        _outboxRepoMock.Verify(x => x.AddAsync(
            It.Is<OutboxMessage>(msg => msg.EventType == "PointsDeductedEvent"),
            default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithInsufficientBalance_PublishesFailureEvent()
    {
        // Arrange
        var command = CreateDeductPointsCommand(amount: 500);
        var balanceResult = new BalanceResult(_customerId, 50, 0, 50, 0, DateTime.UtcNow);

        _balanceServiceMock.Setup(x => x.GetBalanceAsync(_tenantId, _customerId, default))
            .ReturnsAsync(balanceResult);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be("Insufficient balance");

        _ledgerServiceMock.Verify(x => x.DeductPointsAsync(It.IsAny<PointsEngine.Application.Interfaces.DeductPointsCommand>(), default), Times.Never);

        _outboxRepoMock.Verify(x => x.AddAsync(
            It.Is<OutboxMessage>(msg => msg.EventType == "PointsDeductionFailedEvent"),
            default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenLedgerServiceFails_PublishesFailureEvent()
    {
        // Arrange
        var command = CreateDeductPointsCommand(amount: 50);
        var balanceResult = new BalanceResult(_customerId, 100, 0, 100, 0, DateTime.UtcNow);
        var ledgerResult = new LedgerResult(
            Success: false,
            LedgerEntryId: null,
            BalanceAfter: 100,
            Error: "Ledger error");

        _balanceServiceMock.Setup(x => x.GetBalanceAsync(_tenantId, _customerId, default))
            .ReturnsAsync(balanceResult);
        _ledgerServiceMock.Setup(x => x.DeductPointsAsync(It.IsAny<PointsEngine.Application.Interfaces.DeductPointsCommand>(), default))
            .ReturnsAsync(ledgerResult);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be("Ledger error");

        _outboxRepoMock.Verify(x => x.AddAsync(
            It.Is<OutboxMessage>(msg => msg.EventType == "PointsDeductionFailedEvent"),
            default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithException_ReturnsFailureResult()
    {
        // Arrange
        var command = CreateDeductPointsCommand(amount: 50);
        _balanceServiceMock.Setup(x => x.GetBalanceAsync(_tenantId, _customerId, default))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be("Database error");
    }

    private LoyaltyForge.Contracts.Commands.DeductPointsCommand CreateDeductPointsCommand(long amount)
    {
        return new LoyaltyForge.Contracts.Commands.DeductPointsCommand
        {
            CommandId = Guid.NewGuid(),
            TenantId = _tenantId,
            CustomerId = _customerId,
            Amount = amount,
            RedemptionId = _redemptionId,
            IdempotencyKey = $"redemption-{_redemptionId}",
            Description = "Test redemption",
            CreatedAt = DateTime.UtcNow,
            CorrelationId = _redemptionId.ToString()
        };
    }
}
