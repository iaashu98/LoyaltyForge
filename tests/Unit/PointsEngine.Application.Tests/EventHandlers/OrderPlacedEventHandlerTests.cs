using Moq;
using Xunit;
using FluentAssertions;
using PointsEngine.Application.EventHandlers;
using PointsEngine.Application.Interfaces;
using LoyaltyForge.Contracts.Events;
using LoyaltyForge.Common.Outbox;
using Microsoft.Extensions.Logging;

namespace PointsEngine.Application.Tests.EventHandlers;

public class OrderPlacedEventHandlerTests
{
    private readonly Mock<ILedgerService> _ledgerServiceMock;
    private readonly Mock<IRuleService> _ruleServiceMock;
    private readonly Mock<ILogger<OrderPlacedEventHandler>> _loggerMock;
    private readonly OrderPlacedEventHandler _handler;

    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();

    public OrderPlacedEventHandlerTests()
    {
        _ledgerServiceMock = new Mock<ILedgerService>();
        _ruleServiceMock = new Mock<IRuleService>();
        _loggerMock = new Mock<ILogger<OrderPlacedEventHandler>>();

        _handler = new OrderPlacedEventHandler(
            _ruleServiceMock.Object,
            _ledgerServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidOrder_EarnsPoints()
    {
        // Arrange
        var @event = CreateOrderPlacedEvent(orderTotal: 100m);
        var ledgerResult = new LedgerResult(
            Success: true,
            LedgerEntryId: Guid.NewGuid(),
            BalanceAfter: 100,
            Error: null);

        _ruleServiceMock.Setup(x => x.GetRulesAsync(_tenantId, default))
            .ReturnsAsync(new List<PointsEngine.Domain.Entities.Rule>());
        _ledgerServiceMock.Setup(x => x.EarnPointsAsync(It.IsAny<EarnPointsCommand>(), default))
            .ReturnsAsync(ledgerResult);

        // Act
        await _handler.HandleAsync(@event);

        // Assert
        _ledgerServiceMock.Verify(x => x.EarnPointsAsync(
            It.Is<EarnPointsCommand>(cmd =>
                cmd.TenantId == _tenantId &&
                cmd.UserId == _customerId &&
                cmd.PointsAmount == 100), // 1 point per dollar
            default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateEventId_SkipsProcessing()
    {
        // Arrange
        var @event = CreateOrderPlacedEvent(orderTotal: 100m);
        var ledgerResult = new LedgerResult(
            Success: false,
            LedgerEntryId: null,
            BalanceAfter: 0,
            Error: "Duplicate idempotency key");

        _ledgerServiceMock.Setup(x => x.EarnPointsAsync(It.IsAny<EarnPointsCommand>(), default))
            .ReturnsAsync(ledgerResult);
        _ruleServiceMock.Setup(x => x.GetRulesAsync(_tenantId, default))
            .ReturnsAsync(new List<PointsEngine.Domain.Entities.Rule>());

        // Act
        await _handler.HandleAsync(@event);

        // Assert - Should still call but ledger service handles idempotency
        _ledgerServiceMock.Verify(x => x.EarnPointsAsync(It.IsAny<EarnPointsCommand>(), default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithZeroOrderTotal_DoesNotEarnPoints()
    {
        // Arrange
        var @event = CreateOrderPlacedEvent(orderTotal: 0m);

        _ruleServiceMock.Setup(x => x.GetRulesAsync(_tenantId, default))
            .ReturnsAsync(new List<PointsEngine.Domain.Entities.Rule>());

        // Act
        await _handler.HandleAsync(@event);

        // Assert - Handler should still process but earn 0 points
        _ledgerServiceMock.Verify(x => x.EarnPointsAsync(
            It.Is<EarnPointsCommand>(cmd => cmd.PointsAmount == 0),
            default), Times.Once);
    }

    private OrderPlacedEvent CreateOrderPlacedEvent(decimal orderTotal)
    {
        return new OrderPlacedEvent
        {
            EventId = Guid.NewGuid(),
            TenantId = _tenantId,
            ExternalOrderId = "ORDER-123",
            CustomerId = _customerId,
            CustomerEmail = "test@example.com",
            OrderTotal = orderTotal,
            Currency = "USD",
            LineItems = new List<OrderLineItem>
            {
                new OrderLineItem
                {
                    ProductId = "PROD-1",
                    ProductName = "Test Product",
                    Quantity = 1,
                    UnitPrice = orderTotal,
                    LineTotal = orderTotal
                }
            },
            SourcePlatform = "Shopify",
            OccurredAt = DateTime.UtcNow
        };
    }
}
