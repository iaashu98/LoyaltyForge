using Microsoft.AspNetCore.Mvc;
using AuthTenant.Application.Commands;
using AuthTenant.Application.Interfaces;

namespace AuthTenant.Api.Controllers;

/// <summary>
/// Controller for authentication operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthController> _logger;
    // TODO: Inject IJwtService, IPasswordHasher

    public AuthController(IUserRepository userRepository, ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
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
        // TODO: Implement login logic
        // 1. Find user by email and tenant
        // 2. Verify password
        // 3. Generate JWT token
        // 4. Return token
        
        _logger.LogInformation("Login attempt for {Email} in tenant {TenantId}", command.Email, command.TenantId);
        
        var user = await _userRepository.GetByEmailAsync(command.TenantId, command.Email, cancellationToken);
        
        if (user is null)
        {
            return Unauthorized("Invalid credentials");
        }
        
        // TODO: Verify password and generate token
        
        return Ok(new LoginResult("placeholder_token", DateTime.UtcNow.AddHours(1)));
    }
}
