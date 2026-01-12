using FluentAssertions;
using LoyaltyForge.Common.Interfaces;
using Moq;
using PointsEngine.Application.Interfaces;
using PointsEngine.Application.Services;
using PointsEngine.Domain.Entities;
using Xunit;

namespace PointsEngine.Application.Tests.Services;

public class LedgerServiceTests
{
    private readonly Mock<ILedgerRepository> _mockLedgerRepository;
    private readonly Mock<IUserBalanceRepository> _mockBalanceRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly LedgerService _service;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public LedgerServiceTests()
    {
        _mockLedgerRepository = new Mock<ILedgerRepository>();
        _mockBalanceRepository = new Mock<IUserBalanceRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _service = new LedgerService(
            _mockLedgerRepository.Object,
            _mockBalanceRepository.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task EarnPointsAsync_WithValidCommand_CreatesLedgerEntry()
    {
        // Arrange
        var command = new EarnPointsCommand(
            TenantId: _tenantId,
            UserId: _userId,
            PointsAmount: 500,
            SourceType: "order",
            SourceId: Guid.NewGuid(),
            RuleId: Guid.NewGuid(),
            IdempotencyKey: "earn-123",
            Description: "Order purchase");

        var balance = UserBalance.Create(_tenantId, _userId);

        _mockLedgerRepository
            .Setup(r => r.GetByIdempotencyKeyAsync(_tenantId, "earn-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync((LedgerEntry?)null);

        _mockBalanceRepository
            .Setup(r => r.GetOrCreateAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        // Act
        var result = await _service.EarnPointsAsync(command);

        // Assert
        result.Success.Should().BeTrue();
        result.BalanceAfter.Should().Be(500);
        result.LedgerEntryId.Should().NotBeNull();

        _mockLedgerRepository.Verify(r => r.AddAsync(It.IsAny<LedgerEntry>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockBalanceRepository.Verify(r => r.UpdateAsync(It.IsAny<UserBalance>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EarnPointsAsync_WithIdempotencyKey_ReturnsExisting()
    {
        // Arrange
        var command = new EarnPointsCommand(
            TenantId: _tenantId,
            UserId: _userId,
            PointsAmount: 500,
            SourceType: "order",
            SourceId: Guid.NewGuid(),
            RuleId: null,
            IdempotencyKey: "earn-duplicate");

        var existingEntry = LedgerEntry.CreateEarn(
            _tenantId, _userId, "earn-duplicate", 500, 500, "order", Guid.NewGuid(), null, "Existing");

        _mockLedgerRepository
            .Setup(r => r.GetByIdempotencyKeyAsync(_tenantId, "earn-duplicate", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntry);

        // Act
        var result = await _service.EarnPointsAsync(command);

        // Assert
        result.Success.Should().BeTrue();
        result.LedgerEntryId.Should().Be(existingEntry.Id);
        result.BalanceAfter.Should().Be(500);

        // Should not create new entry
        _mockLedgerRepository.Verify(r => r.AddAsync(It.IsAny<LedgerEntry>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeductPointsAsync_WithSufficientBalance_Succeeds()
    {
        // Arrange
        var command = new DeductPointsCommand(
            TenantId: _tenantId,
            UserId: _userId,
            PointsAmount: 300,
            SourceType: "redemption",
            SourceId: Guid.NewGuid(),
            IdempotencyKey: "deduct-123");

        var balance = UserBalance.Create(_tenantId, _userId);
        balance.ApplyEarn(1000, Guid.NewGuid()); // User has 1000 points

        _mockLedgerRepository
            .Setup(r => r.GetByIdempotencyKeyAsync(_tenantId, "deduct-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync((LedgerEntry?)null);

        _mockBalanceRepository
            .Setup(r => r.GetOrCreateAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        // Act
        var result = await _service.DeductPointsAsync(command);

        // Assert
        result.Success.Should().BeTrue();
        result.BalanceAfter.Should().Be(700); // 1000 - 300
        result.LedgerEntryId.Should().NotBeNull();

        _mockLedgerRepository.Verify(r => r.AddAsync(It.IsAny<LedgerEntry>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeductPointsAsync_WithInsufficientBalance_Fails()
    {
        // Arrange
        var command = new DeductPointsCommand(
            TenantId: _tenantId,
            UserId: _userId,
            PointsAmount: 1500,
            SourceType: "redemption",
            SourceId: Guid.NewGuid(),
            IdempotencyKey: "deduct-insufficient");

        var balance = UserBalance.Create(_tenantId, _userId);
        balance.ApplyEarn(1000, Guid.NewGuid()); // User only has 1000 points

        _mockLedgerRepository
            .Setup(r => r.GetByIdempotencyKeyAsync(_tenantId, "deduct-insufficient", It.IsAny<CancellationToken>()))
            .ReturnsAsync((LedgerEntry?)null);

        _mockBalanceRepository
            .Setup(r => r.GetOrCreateAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        // Act
        var result = await _service.DeductPointsAsync(command);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Insufficient balance");
        result.LedgerEntryId.Should().BeNull();

        _mockLedgerRepository.Verify(r => r.AddAsync(It.IsAny<LedgerEntry>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EarnPointsAsync_WithTransactionFailure_RollsBack()
    {
        // Arrange
        var command = new EarnPointsCommand(
            TenantId: _tenantId,
            UserId: _userId,
            PointsAmount: 500,
            SourceType: "order",
            SourceId: Guid.NewGuid(),
            RuleId: null,
            IdempotencyKey: "earn-fail");

        var balance = UserBalance.Create(_tenantId, _userId);

        _mockLedgerRepository
            .Setup(r => r.GetByIdempotencyKeyAsync(_tenantId, "earn-fail", It.IsAny<CancellationToken>()))
            .ReturnsAsync((LedgerEntry?)null);

        _mockBalanceRepository
            .Setup(r => r.GetOrCreateAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        _mockLedgerRepository
            .Setup(r => r.AddAsync(It.IsAny<LedgerEntry>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.EarnPointsAsync(command);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Failed to earn points");
        _mockUnitOfWork.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeductPointsAsync_UpdatesBalanceCorrectly()
    {
        // Arrange
        var command = new DeductPointsCommand(
            TenantId: _tenantId,
            UserId: _userId,
            PointsAmount: 250,
            SourceType: "redemption",
            SourceId: Guid.NewGuid(),
            IdempotencyKey: "deduct-balance");

        var balance = UserBalance.Create(_tenantId, _userId);
        balance.ApplyEarn(1000, Guid.NewGuid());

        _mockLedgerRepository
            .Setup(r => r.GetByIdempotencyKeyAsync(_tenantId, "deduct-balance", It.IsAny<CancellationToken>()))
            .ReturnsAsync((LedgerEntry?)null);

        _mockBalanceRepository
            .Setup(r => r.GetOrCreateAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        // Act
        await _service.DeductPointsAsync(command);

        // Assert
        _mockBalanceRepository.Verify(
            r => r.UpdateAsync(
                It.Is<UserBalance>(b => b.AvailablePoints == 750 && b.LifetimeRedeemed == 250),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EarnPointsAsync_WithExpirationDate_CreatesEntryWithExpiry()
    {
        // Arrange
        var expiryDate = DateTime.UtcNow.AddDays(90);
        var command = new EarnPointsCommand(
            TenantId: _tenantId,
            UserId: _userId,
            PointsAmount: 500,
            SourceType: "promotion",
            SourceId: Guid.NewGuid(),
            RuleId: Guid.NewGuid(),
            IdempotencyKey: "earn-expiry",
            ExpiresAt: expiryDate);

        var balance = UserBalance.Create(_tenantId, _userId);

        _mockLedgerRepository
            .Setup(r => r.GetByIdempotencyKeyAsync(_tenantId, "earn-expiry", It.IsAny<CancellationToken>()))
            .ReturnsAsync((LedgerEntry?)null);

        _mockBalanceRepository
            .Setup(r => r.GetOrCreateAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        // Act
        var result = await _service.EarnPointsAsync(command);

        // Assert
        result.Success.Should().BeTrue();
        _mockLedgerRepository.Verify(
            r => r.AddAsync(
                It.Is<LedgerEntry>(e => e.ExpiresAt == expiryDate),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeductPointsAsync_WithIdempotencyKey_ReturnsExisting()
    {
        // Arrange
        var command = new DeductPointsCommand(
            TenantId: _tenantId,
            UserId: _userId,
            PointsAmount: 300,
            SourceType: "redemption",
            SourceId: Guid.NewGuid(),
            IdempotencyKey: "deduct-duplicate");

        var existingEntry = LedgerEntry.CreateDeduct(
            _tenantId, _userId, "deduct-duplicate", 300, 700, "redemption", Guid.NewGuid(), "Existing");

        _mockLedgerRepository
            .Setup(r => r.GetByIdempotencyKeyAsync(_tenantId, "deduct-duplicate", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntry);

        // Act
        var result = await _service.DeductPointsAsync(command);

        // Assert
        result.Success.Should().BeTrue();
        result.LedgerEntryId.Should().Be(existingEntry.Id);
        result.BalanceAfter.Should().Be(700);

        // Should not create new entry
        _mockLedgerRepository.Verify(r => r.AddAsync(It.IsAny<LedgerEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
