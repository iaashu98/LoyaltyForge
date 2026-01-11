using PointsEngine.Api.Controllers;
using PointsEngine.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PointsEngine.Domain.Entities;
using Moq;
using Xunit;
using FluentAssertions;

namespace PointsEngine.Api.Tests;

public class RulesControllerTests
{
    private readonly RulesController _controller;
    private readonly Mock<IRuleService> _ruleServiceMock;
    private readonly Mock<ILogger<RulesController>> _loggerMock;

    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _ruleId = Guid.NewGuid();

    public RulesControllerTests()
    {
        _ruleServiceMock = new Mock<IRuleService>();
        _loggerMock = new Mock<ILogger<RulesController>>();
        _controller = new RulesController(_ruleServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateRule_ReturnsCreatedRule()
    {
        // Arrange
        var request = new CreateRuleRequest(
            Name: "Test Rule",
            EventType: "order.created",
            RuleDefinition: "{\"points\": 10}"
        );

        var createResult = new RuleResult(Guid.NewGuid(), "Test Rule", true, null);
        _ruleServiceMock.Setup(x => x.CreateRuleAsync(It.IsAny<CreateRuleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createResult);

        // Act
        var result = await _controller.CreateRule(_tenantId, request, default);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task GetRules_ReturnsRules()
    {
        // Arrange
        var rules = new List<Rule>
        {
            Rule.Create(_tenantId, "Rule 1", "order.created", "{}", 1, null, null, null)
        };

        _ruleServiceMock.Setup(x => x.GetRulesAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rules);

        // Act
        var result = await _controller.GetRules(_tenantId, default);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetRule_ReturnsRule()
    {
        // Arrange
        var rule = Rule.Create(_tenantId, "Test Rule", "order.created", "{}", 1, null, null, null);
        _ruleServiceMock.Setup(x => x.GetRuleByIdAsync(_ruleId, _tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);

        // Act
        var result = await _controller.GetRule(_tenantId, _ruleId, default);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateRule_ReturnsUpdatedRule()
    {
        // Arrange
        var request = new UpdateRuleRequest(Name: "Updated Rule");
        var updateResult = new RuleResult(_ruleId, "Updated Rule", true, null);

        _ruleServiceMock.Setup(x => x.UpdateRuleAsync(It.IsAny<UpdateRuleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updateResult);

        // Act
        var result = await _controller.UpdateRule(_tenantId, _ruleId, request, default);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteRule_ReturnsNoContent()
    {
        // Arrange
        _ruleServiceMock.Setup(x => x.DeleteRuleAsync(_ruleId, _tenantId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteRule(_tenantId, _ruleId, default);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task ActivateRule_ReturnsOk()
    {
        // Arrange
        _ruleServiceMock.Setup(x => x.ActivateRuleAsync(_ruleId, _tenantId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ActivateRule(_tenantId, _ruleId, default);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeactivateRule_ReturnsOk()
    {
        // Arrange
        _ruleServiceMock.Setup(x => x.DeactivateRuleAsync(_ruleId, _tenantId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeactivateRule(_tenantId, _ruleId, default);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}