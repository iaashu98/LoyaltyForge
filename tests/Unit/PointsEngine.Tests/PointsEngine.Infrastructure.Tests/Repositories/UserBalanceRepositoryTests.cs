using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PointsEngine.Domain.Entities;
using PointsEngine.Infrastructure.Persistence;
using PointsEngine.Infrastructure.Repositories;
using Xunit;

namespace PointsEngine.Infrastructure.Tests.Repositories;

public class UserBalanceRepositoryTests : IDisposable
{
    private readonly PointsEngineDbContext _context;
    private readonly UserBalanceRepository _repository;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public UserBalanceRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PointsEngineDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PointsEngineDbContext(options);
        _repository = new UserBalanceRepository(_context);
    }

    [Fact]
    public async Task GetByUserAsync_WithExistingBalance_ReturnsBalance()
    {
        // Arrange
        var balance = UserBalance.Create(_tenantId, _userId);
        await _context.UserBalances.AddAsync(balance);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserAsync(_tenantId, _userId);

        // Assert
        result.Should().NotBeNull();
        result!.TenantId.Should().Be(_tenantId);
        result.UserId.Should().Be(_userId);
        result.AvailablePoints.Should().Be(0);
    }

    [Fact]
    public async Task GetByUserAsync_WithNonExistingBalance_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByUserAsync(_tenantId, _userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserAsync_WithDifferentTenant_ReturnsNull()
    {
        // Arrange
        var balance = UserBalance.Create(_tenantId, _userId);
        await _context.UserBalances.AddAsync(balance);
        await _context.SaveChangesAsync();

        var differentTenantId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByUserAsync(differentTenantId, _userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetOrCreateAsync_WithExistingBalance_ReturnsExisting()
    {
        // Arrange
        var existingBalance = UserBalance.Create(_tenantId, _userId);
        existingBalance.ApplyEarn(100, Guid.NewGuid());
        await _context.UserBalances.AddAsync(existingBalance);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetOrCreateAsync(_tenantId, _userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(existingBalance.Id);
        result.AvailablePoints.Should().Be(100); // Should return existing balance
    }

    [Fact]
    public async Task GetOrCreateAsync_WithNonExistingBalance_CreatesNew()
    {
        // Act
        var result = await _repository.GetOrCreateAsync(_tenantId, _userId);

        // Assert
        result.Should().NotBeNull();
        result.TenantId.Should().Be(_tenantId);
        result.UserId.Should().Be(_userId);
        result.AvailablePoints.Should().Be(0);
        result.LifetimeEarned.Should().Be(0);
        result.LifetimeRedeemed.Should().Be(0);

        // Verify it was saved to database
        var savedBalance = await _context.UserBalances.FindAsync(result.Id);
        savedBalance.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_UpdatesBalance()
    {
        // Arrange
        var balance = UserBalance.Create(_tenantId, _userId);
        await _context.UserBalances.AddAsync(balance);
        await _context.SaveChangesAsync();

        // Detach to simulate new context
        _context.Entry(balance).State = EntityState.Detached;

        // Get fresh instance and modify
        var balanceToUpdate = await _context.UserBalances.FindAsync(balance.Id);
        var ledgerEntryId = Guid.NewGuid();
        balanceToUpdate!.ApplyEarn(500, ledgerEntryId);

        // Act
        await _repository.UpdateAsync(balanceToUpdate);

        // Assert
        var updatedBalance = await _context.UserBalances.FindAsync(balance.Id);
        updatedBalance!.AvailablePoints.Should().Be(500);
        updatedBalance.LifetimeEarned.Should().Be(500);
        updatedBalance.LastLedgerEntryId.Should().Be(ledgerEntryId);
    }

    [Fact]
    public async Task ApplyEarn_IncreasesBalanceAndLifetimeEarned()
    {
        // Arrange
        var balance = UserBalance.Create(_tenantId, _userId);
        var ledgerEntryId = Guid.NewGuid();

        // Act
        balance.ApplyEarn(250, ledgerEntryId);

        // Assert
        balance.AvailablePoints.Should().Be(250);
        balance.LifetimeEarned.Should().Be(250);
        balance.LifetimeRedeemed.Should().Be(0);
        balance.LastLedgerEntryId.Should().Be(ledgerEntryId);
    }

    [Fact]
    public async Task ApplyRedeem_DecreasesBalanceAndIncreasesLifetimeRedeemed()
    {
        // Arrange
        var balance = UserBalance.Create(_tenantId, _userId);
        balance.ApplyEarn(1000, Guid.NewGuid());
        var redeemLedgerEntryId = Guid.NewGuid();

        // Act
        balance.ApplyRedeem(300, redeemLedgerEntryId);

        // Assert
        balance.AvailablePoints.Should().Be(700);
        balance.LifetimeEarned.Should().Be(1000);
        balance.LifetimeRedeemed.Should().Be(300);
        balance.LastLedgerEntryId.Should().Be(redeemLedgerEntryId);
    }

    [Fact]
    public async Task ApplyExpiry_DecreasesAvailablePoints()
    {
        // Arrange
        var balance = UserBalance.Create(_tenantId, _userId);
        balance.ApplyEarn(500, Guid.NewGuid());
        var expiryLedgerEntryId = Guid.NewGuid();

        // Act
        balance.ApplyExpiry(100, expiryLedgerEntryId);

        // Assert
        balance.AvailablePoints.Should().Be(400);
        balance.LifetimeEarned.Should().Be(500); // Lifetime earned doesn't change
        balance.LifetimeRedeemed.Should().Be(0); // Not a redemption
        balance.LastLedgerEntryId.Should().Be(expiryLedgerEntryId);
    }

    [Fact]
    public async Task MultipleOperations_TracksCorrectly()
    {
        // Arrange
        var balance = UserBalance.Create(_tenantId, _userId);

        // Act - Simulate realistic point lifecycle
        balance.ApplyEarn(1000, Guid.NewGuid()); // Earn 1000
        balance.ApplyEarn(500, Guid.NewGuid());  // Earn 500 more
        balance.ApplyRedeem(300, Guid.NewGuid()); // Redeem 300
        balance.ApplyExpiry(50, Guid.NewGuid());  // 50 expire

        // Assert
        balance.AvailablePoints.Should().Be(1150); // 1000 + 500 - 300 - 50
        balance.LifetimeEarned.Should().Be(1500);  // 1000 + 500
        balance.LifetimeRedeemed.Should().Be(300);
    }

    [Fact]
    public async Task Create_InitializesWithZeroBalances()
    {
        // Act
        var balance = UserBalance.Create(_tenantId, _userId);

        // Assert
        balance.TenantId.Should().Be(_tenantId);
        balance.UserId.Should().Be(_userId);
        balance.AvailablePoints.Should().Be(0);
        balance.PendingPoints.Should().Be(0);
        balance.LifetimeEarned.Should().Be(0);
        balance.LifetimeRedeemed.Should().Be(0);
        balance.LastLedgerEntryId.Should().BeNull();
    }

    [Fact]
    public async Task GetOrCreateAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await _repository.GetOrCreateAsync(_tenantId, _userId, cts.Token));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
