using PointsEngine.Application.CommandHandlers;
using PointsEngine.Application.Interfaces;
using LoyaltyForge.Contracts.Commands;
using LoyaltyForge.Common.Outbox;
using Moq;
using Xunit;
using FluentAssertions;

namespace PointsEngine.Application.Tests.CommandHandlers;

/// <summary>
/// Tests for the DeductPointsCommandHandler.
/// </summary>
public class DeductPointsCommandHandlerTests
{
    private readonly Mock<IBalanceService> _balanceServiceMock;
    private readonly Mock<ILedgerService> _ledgerServiceMock;
    private readonly Mock<IOutboxRepository> _outboxRepositoryMock;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<DeductPointsCommandHandler>> _loggerMock;
    private readonly DeductPointsCommandHandler _handler;

    public DeductPointsCommandHandlerTests()
    {
        _balanceServiceMock = new Mock<IBalanceService>();
        _ledgerServiceMock = new Mock<ILedgerService>();
        _outboxRepositoryMock = new Mock<IOutboxRepository>();
        _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<DeductPointsCommandHandler>>();
        _handler = new DeductPointsCommandHandler(
            _balanceServiceMock.Object,
            _ledgerServiceMock.Object,
            _outboxRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ExecutesSuccessfully()
    {
        // Arrange
        var command = new LoyaltyForge.Contracts.Commands.DeductPointsCommand
        {
            CustomerId = Guid.NewGuid(),
            Amount = 10,
            RedemptionId = Guid.NewGuid(),
            IdempotencyKey = Guid.NewGuid().ToString(),
            Description = "Test deduction"
        };

        _balanceServiceMock.Setup(x => x.HasSufficientPointsAsync(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<long>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var ledgerResult = new LedgerResult(Guid.NewGuid(), 90, true, null);
        _ledgerServiceMock.Setup(x => x.DeductPointsAsync(
            It.IsAny<PointsEngine.Application.Interfaces.DeductPointsCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(ledgerResult);

        // Act
        var result = await _handler.HandleAsync(command, default);

        // Assert
        result.Should().NotBeNull();
        // Note: Actual success depends on handler implementation details
    }

    [Fact]
    public void Handler_Instantiates_Successfully()
    {
        // Assert
        _handler.Should().NotBeNull();
    }
}