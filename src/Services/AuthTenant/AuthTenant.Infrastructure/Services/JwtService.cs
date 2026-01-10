using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthTenant.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TokenValidationResult = AuthTenant.Application.Interfaces.TokenValidationResult;

namespace AuthTenant.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryMinutes;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
        _secret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured");
        _issuer = _configuration["Jwt:Issuer"] ?? "LoyaltyForge";
        _audience = _configuration["Jwt:Audience"] ?? "LoyaltyForge";
        _expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "24");
    }

    //Summary
    // 1. Create claims (user data stored in token)
    // 2. Create token
    // 3. Return token
    public string GenerateToken(Guid userId, Guid tenantId, string email, IEnumerable<string>? roles = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("tenantId", tenantId.ToString()),
            new("userId", userId.ToString())
        };

        if (roles != null)
        {
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_expiryMinutes),
            signingCredentials: creds
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }

    public Task<TokenValidationResult> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secret);
            // Validation parameters
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // No tolerance for expiration
            };
            // Validate token
            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            // Extract claims
            var userId = Guid.Parse(principal.FindFirst("userId")?.Value ?? string.Empty);
            var tenantId = Guid.Parse(principal.FindFirst("tenantId")?.Value ?? string.Empty);
            var email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            return Task.FromResult(new TokenValidationResult(
                IsValid: true,
                userId,
                tenantId,
                email,
                Error: null));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new TokenValidationResult(
                IsValid: false,
                UserId: null,
                TenantId: null,
                Email: null,
                Error: ex.Message));
        }
    }
}