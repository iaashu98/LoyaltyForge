using FluentAssertions;
using Moq;
using PointsEngine.Application.Interfaces;
using PointsEngine.Application.Services;
using PointsEngine.Domain.Entities;
using Xunit;

namespace PointsEngine.Application.Tests.Services;

public class BalanceServiceTests
{
    private readonly Mock<IUserBalanceRepository> _mockBalanceRepository;
    private readonly Mock<ILedgerRepository> _mockLedgerRepository;
    private readonly BalanceService _service;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public BalanceServiceTests()
    {
        _mockBalanceRepository = new Mock<IUserBalanceRepository>();
        _mockLedgerRepository = new Mock<ILedgerRepository>();
        _service = new BalanceService(_mockBalanceRepository.Object, _mockLedgerRepository.Object);
    }

    [Fact]
    public async Task GetBalanceAsync_WithExistingBalance_ReturnsBalance()
    {
        // Arrange
        var balance = UserBalance.Create(_tenantId, _userId);
        balance.ApplyEarn(1000, Guid.NewGuid());
        balance.ApplyRedeem(300, Guid.NewGuid());

        _mockBalanceRepository
            .Setup(r => r.GetByUserAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        // Act
        var result = await _service.GetBalanceAsync(_tenantId, _userId);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(_userId);
        result.AvailablePoints.Should().Be(700); // 1000 - 300
        result.LifetimeEarned.Should().Be(1000);
        result.LifetimeRedeemed.Should().Be(300);
    }

    [Fact]
    public async Task GetBalanceAsync_WithNoBalance_ReturnsZeroBalance()
    {
        // Arrange
        _mockBalanceRepository
            .Setup(r => r.GetByUserAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserBalance?)null);

        // Act
        var result = await _service.GetBalanceAsync(_tenantId, _userId);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(_userId);
        result.AvailablePoints.Should().Be(0);
        result.PendingPoints.Should().Be(0);
        result.LifetimeEarned.Should().Be(0);
        result.LifetimeRedeemed.Should().Be(0);
    }

    [Fact]
    public async Task HasSufficientPointsAsync_WithSufficientPoints_ReturnsTrue()
    {
        // Arrange
        var balance = UserBalance.Create(_tenantId, _userId);
        balance.ApplyEarn(1000, Guid.NewGuid());

        _mockBalanceRepository
            .Setup(r => r.GetByUserAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        // Act
        var result = await _service.HasSufficientPointsAsync(_tenantId, _userId, 500);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasSufficientPointsAsync_WithInsufficientPoints_ReturnsFalse()
    {
        // Arrange
        var balance = UserBalance.Create(_tenantId, _userId);
        balance.ApplyEarn(300, Guid.NewGuid());

        _mockBalanceRepository
            .Setup(r => r.GetByUserAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        // Act
        var result = await _service.HasSufficientPointsAsync(_tenantId, _userId, 500);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasSufficientPointsAsync_WithExactPoints_ReturnsTrue()
    {
        // Arrange
        var balance = UserBalance.Create(_tenantId, _userId);
        balance.ApplyEarn(500, Guid.NewGuid());

        _mockBalanceRepository
            .Setup(r => r.GetByUserAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        // Act
        var result = await _service.HasSufficientPointsAsync(_tenantId, _userId, 500);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasSufficientPointsAsync_WithNoBalance_ReturnsFalse()
    {
        // Arrange
        _mockBalanceRepository
            .Setup(r => r.GetByUserAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserBalance?)null);

        // Act
        var result = await _service.HasSufficientPointsAsync(_tenantId, _userId, 100);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetBalanceAsync_WithCancellation_ThrowsCancellationException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockBalanceRepository
            .Setup(r => r.GetByUserAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await _service.GetBalanceAsync(_tenantId, _userId, cts.Token));
    }

    [Fact]
    public async Task GetBalanceAsync_VerifiesRepositoryCalledWithCorrectParameters()
    {
        // Arrange
        var balance = UserBalance.Create(_tenantId, _userId);
        _mockBalanceRepository
            .Setup(r => r.GetByUserAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        // Act
        await _service.GetBalanceAsync(_tenantId, _userId);

        // Assert
        _mockBalanceRepository.Verify(
            r => r.GetByUserAsync(_tenantId, _userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
