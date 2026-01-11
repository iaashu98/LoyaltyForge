using PointsEngine.Application.Interfaces;
using PointsEngine.Application.EventHandlers;
using LoyaltyForge.Contracts.Events;
using Moq;
using Xunit;
using FluentAssertions;

namespace PointsEngine.Application.Tests.EventHandlers;

public class OrderPlacedEventHandlerTests
{
    private readonly Mock<IRuleService> _ruleServiceMock;
    private readonly Mock<ILedgerService> _ledgerServiceMock;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<OrderPlacedEventHandler>> _loggerMock;
    private readonly OrderPlacedEventHandler _handler;

    public OrderPlacedEventHandlerTests()
    {
        _ruleServiceMock = new Mock<IRuleService>();
        _ledgerServiceMock = new Mock<ILedgerService>();
        _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<OrderPlacedEventHandler>>();
        _handler = new OrderPlacedEventHandler(
            _ruleServiceMock.Object,
            _ledgerServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidOrder_ProcessesSuccessfully()
    {
        // Arrange
        var orderEvent = new OrderPlacedEvent
        {
            EventId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            ExternalOrderId = "order-123",
            OrderTotal = 100.00m,
            CustomerEmail = "test@example.com",
            Currency = "USD",
            SourcePlatform = "shopify",
            LineItems = Array.Empty<OrderLineItem>()
        };

        _ruleServiceMock.Setup(x => x.GetRulesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PointsEngine.Domain.Entities.Rule>());

        // Act
        await _handler.HandleAsync(orderEvent, default);

        // Assert
        _ruleServiceMock.Verify(x => x.GetRulesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
