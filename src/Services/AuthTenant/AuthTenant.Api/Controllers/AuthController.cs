using Microsoft.AspNetCore.Mvc;
using AuthTenant.Application.Commands;
using AuthTenant.Application.Interfaces;
using AuthTenant.Domain.Entities;

namespace AuthTenant.Api.Controllers;

/// <summary>
/// Controller for authentication operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IUserTenantRepository _userTenantRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthController> _logger;
    private readonly IJwtService _jwtService;
    private readonly ITenantRepository _tenantRepository;

    public AuthController(
        IUserRepository userRepository,
        IUserTenantRepository userTenantRepository,
        IPasswordHasher passwordHasher,
        ILogger<AuthController> logger,
        IJwtService jwtService,
        ITenantRepository tenantRepository)
    {
        _userRepository = userRepository;
        _userTenantRepository = userTenantRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _jwtService = jwtService;
        _tenantRepository = tenantRepository;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResult>> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Get tenant by tenantId
        var tenant = await _tenantRepository.GetBySlugAsync(command.TenantSlug, cancellationToken);
        if (tenant is null)
        {
            return Unauthorized(new { message = "Tenant not found!" });
        }

        // 2. Get user by email
        var user = await _userRepository.GetByEmailAndProviderAsync(command.Email, AuthProvider.Local, cancellationToken);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid email" });
        }

        if (!_passwordHasher.VerifyPassword(command.Password, user.PasswordHash!))
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }
        // 4. Generate JWT token ← NEW LOGIC
        var token = _jwtService.GenerateToken(
            user.Id,
            tenant.Id,
            user.Email,
            roles: null); // TODO: Add roles when implemented
        var expiresAt = DateTime.UtcNow.AddHours(24);
        // 5. Return token
        return Ok(new LoginResult(
            token,
            user.Id,
            tenant.Id,
            expiresAt));
    }
}
