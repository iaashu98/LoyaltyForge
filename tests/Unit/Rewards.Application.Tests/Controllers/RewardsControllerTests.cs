using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rewards.Api.Controllers;
using Rewards.Application.Interfaces;
using Rewards.Domain.Entities;

namespace Rewards.Application.Tests.Controllers;

public class RewardsControllerTests
{
    private readonly Mock<IRewardRepository> _rewardRepoMock;
    private readonly Mock<ILogger<RewardsController>> _loggerMock;
    private readonly RewardsController _controller;

    private readonly Guid _tenantId = Guid.NewGuid();

    public RewardsControllerTests()
    {
        _rewardRepoMock = new Mock<IRewardRepository>();
        _loggerMock = new Mock<ILogger<RewardsController>>();
        _controller = new RewardsController(_loggerMock.Object, _rewardRepoMock.Object);
    }

    [Fact]
    public async Task GetRewards_ReturnsAllActiveRewards()
    {
        // Arrange
        var rewards = new List<RewardCatalog>
        {
            RewardCatalog.Create(_tenantId, "Reward 1", 50, "Discount", "10%"),
            RewardCatalog.Create(_tenantId, "Reward 2", 100, "Discount", "20%")
        };

        _rewardRepoMock.Setup(x => x.GetByTenantIdAsync(_tenantId, default))
            .ReturnsAsync(rewards);

        // Act
        var result = await _controller.GetRewards(_tenantId, default);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var returnedRewards = okResult!.Value as IEnumerable<RewardCatalog>;
        returnedRewards.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetReward_WithValidId_ReturnsReward()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        var reward = RewardCatalog.Create(_tenantId, "Test Reward", 50, "Discount", "10%");

        _rewardRepoMock.Setup(x => x.GetByIdAsync(rewardId, default))
            .ReturnsAsync(reward);

        // Act
        var result = await _controller.GetReward(_tenantId, rewardId, default);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var returnedReward = okResult!.Value as RewardCatalog;
        returnedReward.Should().NotBeNull();
        returnedReward!.Name.Should().Be("Test Reward");
    }

    [Fact]
    public async Task GetReward_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        _rewardRepoMock.Setup(x => x.GetByIdAsync(rewardId, default))
            .ReturnsAsync((RewardCatalog?)null);

        // Act
        var result = await _controller.GetReward(_tenantId, rewardId, default);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
