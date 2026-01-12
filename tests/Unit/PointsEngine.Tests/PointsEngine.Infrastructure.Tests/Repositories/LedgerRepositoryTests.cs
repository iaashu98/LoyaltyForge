using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PointsEngine.Domain.Entities;
using PointsEngine.Infrastructure.Persistence;
using PointsEngine.Infrastructure.Repositories;
using Xunit;

namespace PointsEngine.Infrastructure.Tests.Repositories;

public class LedgerRepositoryTests : IDisposable
{
    private readonly PointsEngineDbContext _context;
    private readonly LedgerRepository _repository;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public LedgerRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PointsEngineDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PointsEngineDbContext(options);
        _repository = new LedgerRepository(_context);
    }

    [Fact]
    public async Task AddAsync_CreatesEarnEntry()
    {
        // Arrange
        var entry = LedgerEntry.CreateEarn(
            _tenantId, _userId, "earn-123", 500, 500, SourceTypes.Order, Guid.NewGuid());

        // Act
        await _repository.AddAsync(entry);

        // Assert
        var savedEntry = await _context.LedgerEntries.FindAsync(entry.Id);
        savedEntry.Should().NotBeNull();
        savedEntry!.EntryType.Should().Be(LedgerEntryType.Earn);
        savedEntry.PointsAmount.Should().Be(500);
        savedEntry.IdempotencyKey.Should().Be("earn-123");
    }

    [Fact]
    public async Task AddAsync_CreatesDeductEntry()
    {
        // Arrange
        var entry = LedgerEntry.CreateDeduct(
            _tenantId, _userId, "deduct-123", 300, 200, SourceTypes.Redemption, Guid.NewGuid());

        // Act
        await _repository.AddAsync(entry);

        // Assert
        var savedEntry = await _context.LedgerEntries.FindAsync(entry.Id);
        savedEntry.Should().NotBeNull();
        savedEntry!.EntryType.Should().Be(LedgerEntryType.Redeem);
        savedEntry.PointsAmount.Should().Be(-300); // Negative for deductions
        savedEntry.BalanceAfter.Should().Be(200);
    }

    [Fact]
    public async Task GetByIdempotencyKeyAsync_WithExistingKey_ReturnsEntry()
    {
        // Arrange
        var entry = LedgerEntry.CreateEarn(
            _tenantId, _userId, "unique-key-123", 100, 100, SourceTypes.Order);
        await _context.LedgerEntries.AddAsync(entry);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdempotencyKeyAsync(_tenantId, "unique-key-123");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entry.Id);
        result.IdempotencyKey.Should().Be("unique-key-123");
    }

    [Fact]
    public async Task GetByIdempotencyKeyAsync_WithNonExistingKey_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdempotencyKeyAsync(_tenantId, "non-existing-key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdempotencyKeyAsync_WithDifferentTenant_ReturnsNull()
    {
        // Arrange
        var entry = LedgerEntry.CreateEarn(
            _tenantId, _userId, "key-123", 100, 100, SourceTypes.Order);
        await _context.LedgerEntries.AddAsync(entry);
        await _context.SaveChangesAsync();

        var differentTenantId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdempotencyKeyAsync(differentTenantId, "key-123");

        // Assert
        result.Should().BeNull(); // Tenant isolation
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsEntriesForUser()
    {
        // Arrange
        var entry1 = LedgerEntry.CreateEarn(_tenantId, _userId, "key-1", 100, 100, SourceTypes.Order);
        var entry2 = LedgerEntry.CreateEarn(_tenantId, _userId, "key-2", 200, 300, SourceTypes.Order);
        var otherUserEntry = LedgerEntry.CreateEarn(_tenantId, Guid.NewGuid(), "key-3", 50, 50, SourceTypes.Order);

        await _context.LedgerEntries.AddRangeAsync(entry1, entry2, otherUserEntry);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserAsync(_tenantId, _userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(e => e.IdempotencyKey == "key-1");
        result.Should().Contain(e => e.IdempotencyKey == "key-2");
        result.Should().NotContain(e => e.IdempotencyKey == "key-3");
    }

    [Fact]
    public async Task GetByUserAsync_OrdersByCreatedAtDescending()
    {
        // Arrange
        await Task.Delay(10); // Ensure different timestamps
        var entry1 = LedgerEntry.CreateEarn(_tenantId, _userId, "key-1", 100, 100, SourceTypes.Order);
        await _context.LedgerEntries.AddAsync(entry1);
        await _context.SaveChangesAsync();

        await Task.Delay(10);
        var entry2 = LedgerEntry.CreateEarn(_tenantId, _userId, "key-2", 200, 300, SourceTypes.Order);
        await _context.LedgerEntries.AddAsync(entry2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserAsync(_tenantId, _userId);

        // Assert
        result.Should().HaveCount(2);
        result.First().IdempotencyKey.Should().Be("key-2"); // Most recent first
        result.Last().IdempotencyKey.Should().Be("key-1");
    }

    [Fact]
    public async Task GetByUserAsync_SupportsPagination()
    {
        // Arrange - Create 5 entries
        for (int i = 1; i <= 5; i++)
        {
            var entry = LedgerEntry.CreateEarn(_tenantId, _userId, $"key-{i}", i * 100, i * 100, SourceTypes.Order);
            await _context.LedgerEntries.AddAsync(entry);
            await Task.Delay(5); // Ensure different timestamps
        }
        await _context.SaveChangesAsync();

        // Act - Get page 1 with 2 items
        var page1 = await _repository.GetByUserAsync(_tenantId, _userId, page: 1, pageSize: 2);
        var page2 = await _repository.GetByUserAsync(_tenantId, _userId, page: 2, pageSize: 2);

        // Assert
        page1.Should().HaveCount(2);
        page2.Should().HaveCount(2);
        page1.Should().NotIntersectWith(page2);
    }

    [Fact]
    public async Task CreateExpiry_CreatesExpiryEntry()
    {
        // Arrange
        var entry = LedgerEntry.CreateExpiry(_tenantId, _userId, "expiry-123", 50, 450, "Points expired");

        // Act
        await _repository.AddAsync(entry);

        // Assert
        var savedEntry = await _context.LedgerEntries.FindAsync(entry.Id);
        savedEntry.Should().NotBeNull();
        savedEntry!.EntryType.Should().Be(LedgerEntryType.Expire);
        savedEntry.PointsAmount.Should().Be(-50); // Negative for expiry
        savedEntry.SourceType.Should().Be(SourceTypes.Expiry);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
