using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PointsEngine.Domain.Entities;
using PointsEngine.Infrastructure.Persistence;
using PointsEngine.Infrastructure.Repositories;
using Xunit;

namespace PointsEngine.Infrastructure.Tests.Repositories;

public class RuleRepositoryTests : IDisposable
{
    private readonly PointsEngineDbContext _context;
    private readonly RuleRepository _repository;
    private readonly Guid _tenantId = Guid.NewGuid();

    public RuleRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PointsEngineDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PointsEngineDbContext(options);
        _repository = new RuleRepository(_context);
    }

    [Fact]
    public async Task AddRuleAsync_CreatesRule()
    {
        // Arrange
        var rule = Rule.Create(_tenantId, "Test Rule", "order.completed", "{\"pointsPerDollar\":10}", 100);

        // Act
        await _repository.AddRuleAsync(rule);

        // Assert
        var savedRule = await _context.Rules.FindAsync(rule.Id);
        savedRule.Should().NotBeNull();
        savedRule!.Name.Should().Be("Test Rule");
        savedRule.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetRuleByIdAsync_WithExistingRule_ReturnsRule()
    {
        // Arrange
        var rule = Rule.Create(_tenantId, "Test Rule", "event", "{}", 100);
        await _context.Rules.AddAsync(rule);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetRuleByIdAsync(rule.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(rule.Id);
    }

    [Fact]
    public async Task GetRuleByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetRuleByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllRulesAsync_ReturnsAllRulesForTenant()
    {
        // Arrange
        var rule1 = Rule.Create(_tenantId, "Rule 1", "event1", "{}", 100);
        var rule2 = Rule.Create(_tenantId, "Rule 2", "event2", "{}", 200);
        var otherTenantRule = Rule.Create(Guid.NewGuid(), "Other Rule", "event3", "{}", 300);

        await _context.Rules.AddRangeAsync(rule1, rule2, otherTenantRule);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllRulesAsync(_tenantId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Name == "Rule 1");
        result.Should().Contain(r => r.Name == "Rule 2");
        result.Should().NotContain(r => r.Name == "Other Rule");
    }

    [Fact]
    public async Task GetActiveByTenantAsync_ReturnsOnlyActiveRules()
    {
        // Arrange
        var activeRule = Rule.Create(_tenantId, "Active Rule", "event1", "{}", 100);
        var inactiveRule = Rule.Create(_tenantId, "Inactive Rule", "event2", "{}", 200);
        inactiveRule.Deactivate();

        await _context.Rules.AddRangeAsync(activeRule, inactiveRule);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveByTenantAsync(_tenantId);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(r => r.Name == "Active Rule");
        result.Should().NotContain(r => r.Name == "Inactive Rule");
    }

    [Fact]
    public async Task UpdateRuleAsync_UpdatesRule()
    {
        // Arrange
        var rule = Rule.Create(_tenantId, "Original Name", "event", "{}", 100);
        await _context.Rules.AddAsync(rule);
        await _context.SaveChangesAsync();

        // Act
        rule.UpdateRuleDefinition("{\"updated\":true}");
        await _repository.UpdateRuleAsync(rule);

        // Assert
        var updatedRule = await _context.Rules.FindAsync(rule.Id);
        updatedRule!.RuleDefinition.Should().Be("{\"updated\":true}");
    }

    [Fact]
    public async Task DeleteRuleAsync_RemovesRule()
    {
        // Arrange
        var rule = Rule.Create(_tenantId, "To Delete", "event", "{}", 100);
        await _context.Rules.AddAsync(rule);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteRuleAsync(rule.Id);

        // Assert
        var deletedRule = await _context.Rules.FindAsync(rule.Id);
        deletedRule.Should().BeNull();
    }

    [Fact]
    public async Task DeleteRuleAsync_WithNonExistingId_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _repository.DeleteRuleAsync(Guid.NewGuid()));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
