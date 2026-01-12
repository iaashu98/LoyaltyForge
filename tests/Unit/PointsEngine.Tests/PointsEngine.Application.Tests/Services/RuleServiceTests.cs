using FluentAssertions;
using Moq;
using PointsEngine.Application.Interfaces;
using PointsEngine.Application.Services;
using PointsEngine.Domain.Entities;
using Xunit;

namespace PointsEngine.Application.Tests.Services;

public class RuleServiceTests
{
    private readonly Mock<IRuleRepository> _mockRuleRepository;
    private readonly RuleService _service;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _ruleId = Guid.NewGuid();

    public RuleServiceTests()
    {
        _mockRuleRepository = new Mock<IRuleRepository>();
        _service = new RuleService(_mockRuleRepository.Object);
    }

    [Fact]
    public async Task CreateRuleAsync_WithValidCommand_CreatesRule()
    {
        // Arrange
        var command = new CreateRuleCommand(
            TenantId: _tenantId,
            Name: "Order Completion Rule",
            EventType: "order.completed",
            RuleDefinition: "{\"pointsPerDollar\":10}",
            Priority: 100,
            Description: "Earn 10 points per dollar",
            ValidFrom: DateTime.UtcNow,
            ValidUntil: null,
            CreatedBy: Guid.NewGuid());

        // Act
        var result = await _service.CreateRuleAsync(command);

        // Assert
        result.Success.Should().BeTrue();
        result.Name.Should().Be("Order Completion Rule");
        result.RuleId.Should().NotBeEmpty();

        _mockRuleRepository.Verify(
            r => r.AddRuleAsync(It.IsAny<Rule>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetRulesAsync_ReturnsAllRulesForTenant()
    {
        // Arrange
        var rules = new List<Rule>
        {
            Rule.Create(_tenantId, "Rule 1", "event1", "{}", 100, null, DateTime.UtcNow, null, Guid.NewGuid()),
            Rule.Create(_tenantId, "Rule 2", "event2", "{}", 200, null, DateTime.UtcNow, null, Guid.NewGuid())
        };

        _mockRuleRepository
            .Setup(r => r.GetAllRulesAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rules);

        // Act
        var result = await _service.GetRulesAsync(_tenantId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Name == "Rule 1");
        result.Should().Contain(r => r.Name == "Rule 2");
    }

    [Fact]
    public async Task GetRuleByIdAsync_WithValidTenant_ReturnsRule()
    {
        // Arrange
        var rule = Rule.Create(_tenantId, "Test Rule", "event", "{}", 100, null, DateTime.UtcNow, null, Guid.NewGuid());

        _mockRuleRepository
            .Setup(r => r.GetRuleByIdAsync(rule.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);

        // Act
        var result = await _service.GetRuleByIdAsync(rule.Id, _tenantId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Rule");
    }

    [Fact]
    public async Task GetRuleByIdAsync_WithDifferentTenant_ReturnsNull()
    {
        // Arrange
        var rule = Rule.Create(_tenantId, "Test Rule", "event", "{}", 100, null, DateTime.UtcNow, null, Guid.NewGuid());
        var differentTenantId = Guid.NewGuid();

        _mockRuleRepository
            .Setup(r => r.GetRuleByIdAsync(rule.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);

        // Act
        var result = await _service.GetRuleByIdAsync(rule.Id, differentTenantId);

        // Assert
        result.Should().BeNull(); // Tenant isolation
    }

    [Fact]
    public async Task ActivateRuleAsync_WithValidRule_ActivatesRule()
    {
        // Arrange
        var rule = Rule.Create(_tenantId, "Test Rule", "event", "{}", 100, null, DateTime.UtcNow, null, Guid.NewGuid());
        rule.Deactivate(); // Start as inactive

        _mockRuleRepository
            .Setup(r => r.GetRuleByIdAsync(rule.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);

        // Act
        await _service.ActivateRuleAsync(rule.Id, _tenantId);

        // Assert
        rule.IsActive.Should().BeTrue();
        _mockRuleRepository.Verify(
            r => r.UpdateRuleAsync(It.Is<Rule>(r => r.IsActive), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeactivateRuleAsync_WithValidRule_DeactivatesRule()
    {
        // Arrange
        var rule = Rule.Create(_tenantId, "Test Rule", "event", "{}", 100, null, DateTime.UtcNow, null, Guid.NewGuid());

        _mockRuleRepository
            .Setup(r => r.GetRuleByIdAsync(rule.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);

        // Act
        await _service.DeactivateRuleAsync(rule.Id, _tenantId);

        // Assert
        rule.IsActive.Should().BeFalse();
        _mockRuleRepository.Verify(
            r => r.UpdateRuleAsync(It.Is<Rule>(r => !r.IsActive), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
