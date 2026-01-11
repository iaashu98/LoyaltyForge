using Rewards.Api.Controllers;
using Rewards.Application.Interfaces;
using Rewards.Domain.Entities;
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Rewards.Api.Tests.Controllers;

public class RewardsControllerTests
{
    private readonly Mock<IRewardRepository> _rewardRepositoryMock;
    private readonly Mock<ILogger<RewardsController>> _loggerMock;
    private readonly RewardsController _controller;

    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _rewardId = Guid.NewGuid();

    public RewardsControllerTests()
    {
        _rewardRepositoryMock = new Mock<IRewardRepository>();
        _loggerMock = new Mock<ILogger<RewardsController>>();
        _controller = new RewardsController(_loggerMock.Object, _rewardRepositoryMock.Object);
    }

    [Fact]
    public async Task GetRewards_ReturnsRewardsForTenant()
    {
        // Arrange
        var rewards = new List<RewardCatalog>
        {
            RewardCatalog.Create(_tenantId, "Reward 1", 100, "Discount", "10%")
        };
        _rewardRepositoryMock.Setup(x => x.GetAllByTenantAsync(_tenantId, true, default))
            .ReturnsAsync(rewards);

        // Act
        var result = await _controller.GetRewards(_tenantId, true, default);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetReward_ReturnsReward()
    {
        // Arrange
        var reward = RewardCatalog.Create(_tenantId, "Test Reward", 100, "Discount", "10%");
        _rewardRepositoryMock.Setup(x => x.GetByIdAsync(_rewardId, default))
            .ReturnsAsync(reward);

        // Act
        var result = await _controller.GetReward(_tenantId, _rewardId, default);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetReward_WithInvalidRewardId_ReturnsNotFound()
    {
        // Arrange
        _rewardRepositoryMock.Setup(x => x.GetByIdAsync(_rewardId, default))
            .ReturnsAsync((RewardCatalog?)null);

        // Act
        var result = await _controller.GetReward(_tenantId, _rewardId, default);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
