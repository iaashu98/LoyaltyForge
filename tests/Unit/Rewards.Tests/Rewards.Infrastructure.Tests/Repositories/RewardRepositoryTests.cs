using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Rewards.Domain.Entities;
using Rewards.Infrastructure.Persistence;
using Rewards.Infrastructure.Repositories;
using Xunit;

namespace Rewards.Infrastructure.Tests.Repositories;

public class RewardRepositoryTests : IDisposable
{
    private readonly RewardsDbContext _context;
    private readonly RewardRepository _repository;
    private readonly Guid _tenantId = Guid.NewGuid();

    public RewardRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<RewardsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new RewardsDbContext(options);
        _repository = new RewardRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingReward_ReturnsReward()
    {
        // Arrange
        var reward = RewardCatalog.Create(
            _tenantId,
            "Free Coffee",
            100,
            RewardTypes.Product,
            "{\"productId\":\"coffee-123\"}",
            "Get a free coffee");
        await _context.RewardCatalogs.AddAsync(reward);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(reward.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(reward.Id);
        result.Name.Should().Be("Free Coffee");
        result.PointsCost.Should().Be(100);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllByTenantAsync_ReturnsAllRewardsForTenant()
    {
        // Arrange
        var reward1 = RewardCatalog.Create(_tenantId, "Reward 1", 100, RewardTypes.Discount, "{\"percent\":10}", "Description 1");
        var reward2 = RewardCatalog.Create(_tenantId, "Reward 2", 200, RewardTypes.Product, "{\"productId\":\"p2\"}", "Description 2");
        var otherTenantReward = RewardCatalog.Create(Guid.NewGuid(), "Other", 50, RewardTypes.Custom, "{}", "Other");

        await _context.RewardCatalogs.AddRangeAsync(reward1, reward2, otherTenantReward);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllByTenantAsync(_tenantId, activeOnly: false);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Name == "Reward 1");
        result.Should().Contain(r => r.Name == "Reward 2");
        result.Should().NotContain(r => r.Name == "Other");
    }

    [Fact]
    public async Task GetAllByTenantAsync_WithActiveOnly_ReturnsOnlyActiveRewards()
    {
        // Arrange
        var activeReward = RewardCatalog.Create(_tenantId, "Active", 100, RewardTypes.Product, "{}", "Active reward");
        var inactiveReward = RewardCatalog.Create(_tenantId, "Inactive", 200, RewardTypes.Product, "{}", "Inactive reward");
        inactiveReward.Deactivate();

        await _context.RewardCatalogs.AddRangeAsync(activeReward, inactiveReward);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllByTenantAsync(_tenantId, activeOnly: true);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Active");
        result.First().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task AddAsync_CreatesReward()
    {
        // Arrange
        var reward = RewardCatalog.Create(
            _tenantId,
            "New Reward",
            150,
            RewardTypes.GiftCard,
            "{\"amount\":25}",
            "Brand new reward",
            totalQuantity: 20);

        // Act
        await _repository.AddAsync(reward);
        await _repository.SaveChangesAsync();

        // Assert
        var savedReward = await _context.RewardCatalogs.FindAsync(reward.Id);
        savedReward.Should().NotBeNull();
        savedReward!.Name.Should().Be("New Reward");
        savedReward.PointsCost.Should().Be(150);
        savedReward.TotalQuantity.Should().Be(20);
        savedReward.RemainingQuantity.Should().Be(20);
        savedReward.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Update_ModifiesRewardProperties()
    {
        // Arrange
        var reward = RewardCatalog.Create(_tenantId, "Original", 100, RewardTypes.Product, "{}", "Original desc");
        await _context.RewardCatalogs.AddAsync(reward);
        await _context.SaveChangesAsync();

        // Act - Modify using entity method
        reward.Update(name: "Updated Name", pointsCost: 150);
        _context.RewardCatalogs.Update(reward);
        await _context.SaveChangesAsync();

        // Assert
        var updatedReward = await _context.RewardCatalogs.FindAsync(reward.Id);
        updatedReward!.Name.Should().Be("Updated Name");
        updatedReward.PointsCost.Should().Be(150);
    }

    [Fact]
    public async Task Reward_CanBeActivatedAndDeactivated()
    {
        // Arrange
        var reward = RewardCatalog.Create(_tenantId, "Test", 100, RewardTypes.Product, "{}");
        await _context.RewardCatalogs.AddAsync(reward);
        await _context.SaveChangesAsync();

        // Act - Deactivate
        reward.Deactivate();
        _context.RewardCatalogs.Update(reward);
        await _context.SaveChangesAsync();

        // Assert
        var deactivatedReward = await _context.RewardCatalogs.FindAsync(reward.Id);
        deactivatedReward!.IsActive.Should().BeFalse();

        // Act - Reactivate
        reward.Activate();
        _context.RewardCatalogs.Update(reward);
        await _context.SaveChangesAsync();

        // Assert
        var reactivatedReward = await _context.RewardCatalogs.FindAsync(reward.Id);
        reactivatedReward!.IsActive.Should().BeTrue();
    }

    [Fact]
    public void TryDecrementInventory_WithUnlimitedReward_ReturnsTrue()
    {
        // Arrange
        var reward = RewardCatalog.Create(_tenantId, "Unlimited", 100, RewardTypes.Product, "{}");

        // Act
        var result = reward.TryDecrementInventory();

        // Assert
        result.Should().BeTrue();
        reward.RemainingQuantity.Should().BeNull(); // Unlimited rewards have no quantity
    }

    [Fact]
    public void TryDecrementInventory_WithAvailableStock_DecrementsAndReturnsTrue()
    {
        // Arrange
        var reward = RewardCatalog.Create(_tenantId, "Limited", 100, RewardTypes.Product, "{}", totalQuantity: 10);

        // Act
        var result1 = reward.TryDecrementInventory();
        var result2 = reward.TryDecrementInventory();

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        reward.RemainingQuantity.Should().Be(8);
    }

    [Fact]
    public void TryDecrementInventory_WithNoStock_ReturnsFalse()
    {
        // Arrange
        var reward = RewardCatalog.Create(_tenantId, "OutOfStock", 100, RewardTypes.Product, "{}", totalQuantity: 1);
        reward.TryDecrementInventory(); // Use the only one

        // Act
        var result = reward.TryDecrementInventory();

        // Assert
        result.Should().BeFalse();
        reward.RemainingQuantity.Should().Be(0);
    }

    [Fact]
    public void Create_InitializesWithCorrectDefaults()
    {
        // Act
        var reward = RewardCatalog.Create(_tenantId, "Test Reward", 500, RewardTypes.Discount, "{\"percent\":15}", "Description");

        // Assert
        reward.TenantId.Should().Be(_tenantId);
        reward.Name.Should().Be("Test Reward");
        reward.Description.Should().Be("Description");
        reward.PointsCost.Should().Be(500);
        reward.RewardType.Should().Be(RewardTypes.Discount);
        reward.IsActive.Should().BeTrue();
        reward.IsLimited.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllByTenantAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await _repository.GetAllByTenantAsync(_tenantId, cancellationToken: cts.Token));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
