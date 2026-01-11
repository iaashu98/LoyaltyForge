using AuthTenant.Api.Controllers;
using AuthTenant.Application.Interfaces;
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace AuthTenant.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUserTenantRepository> _userTenantRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<ITenantRepository> _tenantRepositoryMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _userTenantRepositoryMock = new Mock<IUserTenantRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _jwtServiceMock = new Mock<IJwtService>();
        _tenantRepositoryMock = new Mock<ITenantRepository>();
        _controller = new AuthController(
            _userRepositoryMock.Object,
            _userTenantRepositoryMock.Object,
            _passwordHasherMock.Object,
            _loggerMock.Object,
            _jwtServiceMock.Object,
            _tenantRepositoryMock.Object);
    }

    [Fact]
    public void Controller_Instantiates_Successfully()
    {
        // Assert
        _controller.Should().NotBeNull();
    }
}
