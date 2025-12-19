using Microsoft.AspNetCore.Mvc;
using AuthTenant.Application.Commands;
using AuthTenant.Application.Interfaces;
using AuthTenant.Domain.Entities;

namespace AuthTenant.Api.Controllers;

/// <summary>
/// Controller for user management operations.
/// Users are cross-tenant; this controller handles user-tenant mappings.
/// </summary>
[ApiController]
[Route("api/tenants/{tenantId:guid}/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IUserTenantRepository _userTenantRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserRepository userRepository,
        IUserTenantRepository userTenantRepository,
        ITenantRepository tenantRepository,
        IPasswordHasher passwordHasher,
        ILogger<UsersController> logger)
    {
        _userRepository = userRepository;
        _userTenantRepository = userTenantRepository;
        _tenantRepository = tenantRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user within a tenant.
    /// If user exists with same email+provider, creates user-tenant mapping only.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RegisterUserResult>> RegisterUser(
        Guid tenantId,
        [FromBody] RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registering user {Email} for tenant {TenantId}", command.Email, tenantId);

        // Validate tenant exists
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        if (tenant is null)
        {
            return NotFound(new { message = "Tenant not found" });
        }

        // Check if user already exists with this email+provider
        var user = await _userRepository.GetByEmailAndProviderAsync(command.Email, command.Provider, cancellationToken);

        if (user is null)
        {
            // Create new user
            var passwordHash = _passwordHasher.HashPassword(command.Password);
            user = AuthTenant.Domain.Entities.User.CreateLocal(command.Email, passwordHash);
            await _userRepository.AddAsync(user, cancellationToken);
        }

        // Check if user-tenant mapping already exists
        var existingMapping = await _userTenantRepository.GetByUserAndTenantAsync(user.Id, tenantId, cancellationToken);
        if (existingMapping != null)
        {
            return Conflict(new { message = "User is already registered with this tenant" });
        }

        // Create user-tenant mapping
        var userTenant = UserTenant.Create(user.Id, tenantId, command.UserType);
        await _userTenantRepository.AddAsync(userTenant, cancellationToken);

        return CreatedAtAction(
            nameof(GetUser),
            new { tenantId, id = userTenant.Id },
            new RegisterUserResult(user.Id, userTenant.Id, user.Email));
    }

    /// <summary>
    /// Gets a user-tenant mapping by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserTenant>> GetUser(Guid tenantId, Guid id, CancellationToken cancellationToken)
    {
        var userTenant = await _userTenantRepository.GetByIdAsync(id, cancellationToken);

        if (userTenant is null || userTenant.TenantId != tenantId)
        {
            return NotFound();
        }

        return Ok(userTenant);
    }

    /// <summary>
    /// Gets all users for a tenant.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserTenant>>> GetUsers(Guid tenantId, CancellationToken cancellationToken)
    {
        var userTenants = await _userTenantRepository.GetByTenantAsync(tenantId, cancellationToken);
        return Ok(userTenants);
    }
}
