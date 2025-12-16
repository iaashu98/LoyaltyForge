using Microsoft.AspNetCore.Mvc;
using AuthTenant.Application.Commands;
using AuthTenant.Application.Interfaces;
using User = AuthTenant.Domain.Entities.User;

namespace AuthTenant.Api.Controllers;

/// <summary>
/// Controller for user management operations.
/// </summary>
[ApiController]
[Route("api/tenants/{tenantId:guid}/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UsersController> _logger;
    // TODO: Inject IPasswordHasher

    public UsersController(IUserRepository userRepository, ILogger<UsersController> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user within a tenant.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RegisterUserResult>> RegisterUser(
        Guid tenantId,
        [FromBody] RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        // TODO: Implement user registration
        // 1. Validate tenant exists
        // 2. Check email uniqueness
        // 3. Hash password
        // 4. Create user
        
        _logger.LogInformation("Registering user {Email} for tenant {TenantId}", command.Email, tenantId);
        
        // TODO: Hash password properly
        var user = AuthTenant.Domain.Entities.User.Create(tenantId, command.Email, "hashed_password", command.FirstName, command.LastName);
        await _userRepository.AddAsync(user, cancellationToken);
        
        return CreatedAtAction(
            nameof(GetUser),
            new { tenantId, id = user.Id },
            new RegisterUserResult(user.Id, user.Email));
    }

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<User>> GetUser(Guid tenantId, Guid id, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        
        if (user is null || user.TenantId != tenantId)
        {
            return NotFound();
        }
        
        return Ok(user);
    }

    /// <summary>
    /// Gets all users for a tenant.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers(Guid tenantId, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetByTenantAsync(tenantId, cancellationToken);
        return Ok(users);
    }
}
