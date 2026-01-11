using Rewards.Api.Controllers;
using Rewards.Application.Interfaces;
using Rewards.Domain.Entities;
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Rewards.Api.Tests.Controllers;

public class RedemptionsControllerTests
{
    private readonly Mock<ILogger<RedemptionsController>> _loggerMock;
    private readonly RedemptionsController _controller;

    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();

    public RedemptionsControllerTests()
    {
        _loggerMock = new Mock<ILogger<RedemptionsController>>();
        _controller = new RedemptionsController(_loggerMock.Object);
    }

    [Fact]
    public void Controller_Instantiates_Successfully()
    {
        // Assert
        _controller.Should().NotBeNull();
    }
}
