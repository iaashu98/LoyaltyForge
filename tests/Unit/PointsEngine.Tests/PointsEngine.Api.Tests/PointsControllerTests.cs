using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PointsEngine.Api.Controllers;
using PointsEngine.Application.Interfaces;

namespace PointsEngine.Api.Tests;

public class PointsControllerTests
{
    private readonly Mock<IBalanceService> _balanceServiceMock;
    private readonly Mock<ILedgerService> _ledgerServiceMock;
    private readonly PointsController _controller;

    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();

    public PointsControllerTests()
    {
        _balanceServiceMock = new Mock<IBalanceService>();
        _ledgerServiceMock = new Mock<ILedgerService>();
        var loggerMock = new Mock<ILogger<PointsController>>();
        _controller = new PointsController(_balanceServiceMock.Object, _ledgerServiceMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GetBalance_ReturnsBalanceForCustomer()
    {
        // Arrange
        var balance = new BalanceResult(_customerId, 100, 50, 500, 200, DateTime.UtcNow);
        _balanceServiceMock.Setup(x => x.GetBalanceAsync(_tenantId, _customerId, CancellationToken.None))
            .ReturnsAsync(balance);

        // Act
        var result = await _controller.GetBalance(_tenantId, _customerId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedBalance = okResult!.Value as BalanceResult;
        returnedBalance.Should().NotBeNull();
        returnedBalance!.AvailablePoints.Should().Be(100);
    }

    [Fact]
    public async Task EarnPoints_ReturnsSuccess()
    {
        // Arrange
        var request = new EarnPointsRequest(
            Amount: 100,
            SourceType: "order",
            IdempotencyKey: Guid.NewGuid().ToString()
        );

        var ledgerResult = new LedgerResult(Guid.NewGuid(), 100, true, null);
        _ledgerServiceMock.Setup(x => x.EarnPointsAsync(It.IsAny<EarnPointsCommand>(), CancellationToken.None))
            .ReturnsAsync(ledgerResult);

        // Act
        var result = await _controller.EarnPoints(_tenantId, _customerId, request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeductPoints_ReturnsSuccess()
    {
        // Arrange
        var request = new DeductPointsRequest(
            Amount: 50,
            SourceType: "redemption",
            IdempotencyKey: Guid.NewGuid().ToString()
        );

        var ledgerResult = new LedgerResult(Guid.NewGuid(), 50, true, null);
        _ledgerServiceMock.Setup(x => x.DeductPointsAsync(It.IsAny<DeductPointsCommand>(), CancellationToken.None))
            .ReturnsAsync(ledgerResult);

        // Act
        var result = await _controller.DeductPoints(_tenantId, _customerId, request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CheckSufficientPoints_ReturnsTrue()
    {
        // Arrange
        _balanceServiceMock.Setup(x => x.HasSufficientPointsAsync(_tenantId, _customerId, 100, CancellationToken.None))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CheckSufficientPoints(_tenantId, _customerId, 100, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as SufficientPointsResult;
        response.Should().NotBeNull();
        response!.HasSufficientPoints.Should().BeTrue();
    }
}