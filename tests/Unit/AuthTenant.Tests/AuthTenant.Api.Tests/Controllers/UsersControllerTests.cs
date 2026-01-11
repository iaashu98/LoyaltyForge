using AuthTenant.Api.Controllers;
using AuthTenant.Application.Interfaces;
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace AuthTenant.Api.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUserTenantRepository> _userTenantRepositoryMock;
    private readonly Mock<ITenantRepository> _tenantRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ILogger<UsersController>> _loggerMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _userTenantRepositoryMock = new Mock<IUserTenantRepository>();
        _tenantRepositoryMock = new Mock<ITenantRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _loggerMock = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(
            _userRepositoryMock.Object,
            _userTenantRepositoryMock.Object,
            _tenantRepositoryMock.Object,
            _passwordHasherMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void Controller_Instantiates_Successfully()
    {
        // Assert
        _controller.Should().NotBeNull();
    }
}
