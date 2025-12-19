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

    public AuthController(
        IUserRepository userRepository,
        IUserTenantRepository userTenantRepository,
        IPasswordHasher passwordHasher,
        ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
        _userTenantRepository = userTenantRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResult>> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login attempt for {Email} in tenant {TenantId}", command.Email, command.TenantId);

        // Find user by email (local provider) - users are cross-tenant
        var user = await _userRepository.GetByEmailAndProviderAsync(command.Email, AuthProvider.Local, cancellationToken);

        if (user is null)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        // Verify user has access to this tenant
        var userTenant = await _userTenantRepository.GetByUserAndTenantAsync(user.Id, command.TenantId, cancellationToken);

        if (userTenant is null)
        {
            return Unauthorized(new { message = "User not associated with this tenant" });
        }

        // Verify password
        if (user.PasswordHash is null || !_passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        // TODO: Generate JWT token using IJwtService

        return Ok(new LoginResult("placeholder_token", DateTime.UtcNow.AddHours(1)));
    }
}
