using AuthTenant.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace AuthTenant.Application.Tests.Services;

public class JwtServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly JwtService _jwtService;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _tenantId = Guid.NewGuid();
    private const string Email = "test@example.com";
    private const string Secret = "ThisIsAVeryLongSecretKeyForTestingPurposesOnly12345678";

    public JwtServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(x => x["Jwt:Secret"]).Returns(Secret);
        _configurationMock.Setup(x => x["Jwt:Issuer"]).Returns("LoyaltyForge");
        _configurationMock.Setup(x => x["Jwt:Audience"]).Returns("LoyaltyForge");
        _configurationMock.Setup(x => x["Jwt:ExpiryMinutes"]).Returns("60");

        _jwtService = new JwtService(_configurationMock.Object);
    }

    [Fact]
    public void GenerateToken_WithValidInputs_ReturnsValidToken()
    {
        // Act
        var token = _jwtService.GenerateToken(_userId, _tenantId, Email);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == "userId" && c.Value == _userId.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == "tenantId" && c.Value == _tenantId.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == Email);
    }

    [Fact]
    public void GenerateToken_WithRoles_IncludesRolesInToken()
    {
        // Arrange
        var roles = new[] { "Admin", "User" };

        // Act
        var token = _jwtService.GenerateToken(_userId, _tenantId, Email, roles);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
    }

    [Fact]
    public void GenerateToken_WithoutRoles_GeneratesTokenSuccessfully()
    {
        // Act
        var token = _jwtService.GenerateToken(_userId, _tenantId, Email, null);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().NotContain(c => c.Type == ClaimTypes.Role);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithValidToken_ReturnsSuccessResult()
    {
        // Arrange
        var token = _jwtService.GenerateToken(_userId, _tenantId, Email);

        // Act
        var result = await _jwtService.ValidateTokenAsync(token);

        // Assert
        result.IsValid.Should().BeTrue();
        result.UserId.Should().Be(_userId);
        result.TenantId.Should().Be(_tenantId);
        result.Error.Should().BeNull();
    }

    [Fact]
    public async Task ValidateTokenAsync_WithInvalidToken_ReturnsFailureResult()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var result = await _jwtService.ValidateTokenAsync(invalidToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.UserId.Should().BeNull();
        result.TenantId.Should().BeNull();
        result.Email.Should().BeNull();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ValidateTokenAsync_WithExpiredToken_ReturnsFailureResult()
    {
        // Arrange - Create a configuration with very short expiry
        var shortExpiryConfig = new Mock<IConfiguration>();
        shortExpiryConfig.Setup(x => x["Jwt:Secret"]).Returns(Secret);
        shortExpiryConfig.Setup(x => x["Jwt:Issuer"]).Returns("LoyaltyForge");
        shortExpiryConfig.Setup(x => x["Jwt:Audience"]).Returns("LoyaltyForge");
        shortExpiryConfig.Setup(x => x["Jwt:ExpiryMinutes"]).Returns("-1"); // Expired immediately

        var shortExpiryService = new JwtService(shortExpiryConfig.Object);
        var expiredToken = shortExpiryService.GenerateToken(_userId, _tenantId, Email);

        // Wait a moment to ensure expiration
        await Task.Delay(100);

        // Act
        var result = await _jwtService.ValidateTokenAsync(expiredToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().ContainAny("Lifetime", "lifetime", "expired");
    }

    [Fact]
    public void Constructor_WithMissingSecret_ThrowsInvalidOperationException()
    {
        // Arrange
        var invalidConfig = new Mock<IConfiguration>();
        invalidConfig.Setup(x => x["Jwt:Secret"]).Returns((string)null!);

        // Act & Assert
        var act = () => new JwtService(invalidConfig.Object);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Jwt:Secret*");
    }

    [Fact]
    public void GenerateToken_HasCorrectIssuerAndAudience()
    {
        // Act
        var token = _jwtService.GenerateToken(_userId, _tenantId, Email);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Issuer.Should().Be("LoyaltyForge");
        jwtToken.Audiences.Should().Contain("LoyaltyForge");
    }

    [Fact]
    public void GenerateToken_HasExpirationSet()
    {
        // Act
        var token = _jwtService.GenerateToken(_userId, _tenantId, Email);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.ValidTo.Should().BeAfter(DateTime.UtcNow);
        jwtToken.ValidTo.Should().BeBefore(DateTime.UtcNow.AddHours(2));
    }
}
