using AuthTenant.Api.Controllers;
using AuthTenant.Application.Interfaces;
using AuthTenant.Domain.Entities;
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthTenant.Api.Tests.Controllers;

public class TenantsControllerTests
{
    private readonly Mock<ITenantRepository> _tenantRepositoryMock;
    private readonly Mock<ILogger<TenantsController>> _loggerMock;
    private readonly TenantsController _controller;

    private readonly Guid _tenantId = Guid.NewGuid();

    public TenantsControllerTests()
    {
        _tenantRepositoryMock = new Mock<ITenantRepository>();
        _loggerMock = new Mock<ILogger<TenantsController>>();
        _controller = new TenantsController(_tenantRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetTenant_ReturnsTenant()
    {
        // Arrange
        var tenant = Tenant.Create("Test Tenant", "test-tenant", "test@example.com");
        _tenantRepositoryMock.Setup(x => x.GetByIdAsync(_tenantId, default))
            .ReturnsAsync(tenant);

        // Act
        var result = await _controller.GetTenant(_tenantId, default);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetTenant_WithInvalidTenantId_ReturnsNotFound()
    {
        // Arrange
        _tenantRepositoryMock.Setup(x => x.GetByIdAsync(_tenantId, default))
            .ReturnsAsync((Tenant?)null);

        // Act
        var result = await _controller.GetTenant(_tenantId, default);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }
}
