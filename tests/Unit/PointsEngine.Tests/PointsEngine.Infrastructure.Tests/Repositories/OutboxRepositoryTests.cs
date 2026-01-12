using FluentAssertions;
using LoyaltyForge.Common.Outbox;
using Microsoft.EntityFrameworkCore;
using PointsEngine.Infrastructure.Persistence;
using PointsEngine.Infrastructure.Repositories;
using Xunit;

namespace PointsEngine.Infrastructure.Tests.Repositories;

public class OutboxRepositoryTests : IDisposable
{
    private readonly PointsEngineDbContext _context;
    private readonly OutboxRepository _repository;

    public OutboxRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PointsEngineDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PointsEngineDbContext(options);
        _repository = new OutboxRepository(_context);
    }

    [Fact]
    public async Task AddAsync_CreatesOutboxMessage()
    {
        // Arrange
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = "PointsEarned",
            Payload = "{\"userId\":\"123\",\"points\":500}",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _repository.AddAsync(message);

        // Assert
        var savedMessage = await _context.Set<OutboxMessage>().FindAsync(message.Id);
        savedMessage.Should().NotBeNull();
        savedMessage!.EventType.Should().Be("PointsEarned");
        savedMessage.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public async Task GetPendingAsync_ReturnsUnprocessedMessages()
    {
        // Arrange
        var pending1 = new OutboxMessage { Id = Guid.NewGuid(), EventType = "Event1", Payload = "{}", CreatedAt = DateTime.UtcNow };
        var pending2 = new OutboxMessage { Id = Guid.NewGuid(), EventType = "Event2", Payload = "{}", CreatedAt = DateTime.UtcNow.AddSeconds(1) };
        var processed = new OutboxMessage { Id = Guid.NewGuid(), EventType = "Event3", Payload = "{}", CreatedAt = DateTime.UtcNow, ProcessedAt = DateTime.UtcNow };

        await _context.Set<OutboxMessage>().AddRangeAsync(pending1, pending2, processed);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPendingAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(m => m.EventType == "Event1");
        result.Should().Contain(m => m.EventType == "Event2");
        result.Should().NotContain(m => m.EventType == "Event3");
    }

    [Fact]
    public async Task MarkAsProcessedAsync_UpdatesProcessedAt()
    {
        // Arrange
        var message = new OutboxMessage { Id = Guid.NewGuid(), EventType = "Event", Payload = "{}", CreatedAt = DateTime.UtcNow };
        await _context.Set<OutboxMessage>().AddAsync(message);
        await _context.SaveChangesAsync();

        // Act
        await _repository.MarkAsProcessedAsync(message.Id);

        // Assert
        var updatedMessage = await _context.Set<OutboxMessage>().FindAsync(message.Id);
        updatedMessage!.ProcessedAt.Should().NotBeNull();
        updatedMessage.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateRetryAsync_IncrementsRetryCountAndSetsError()
    {
        // Arrange
        var message = new OutboxMessage { Id = Guid.NewGuid(), EventType = "Event", Payload = "{}", CreatedAt = DateTime.UtcNow, RetryCount = 0 };
        await _context.Set<OutboxMessage>().AddAsync(message);
        await _context.SaveChangesAsync();

        // Act
        await _repository.UpdateRetryAsync(message.Id, "Connection timeout");

        // Assert
        var updatedMessage = await _context.Set<OutboxMessage>().FindAsync(message.Id);
        updatedMessage!.RetryCount.Should().Be(1);
        updatedMessage.LastError.Should().Be("Connection timeout");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
