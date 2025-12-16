using Microsoft.AspNetCore.Mvc;
using AuthTenant.Application.Commands;
using AuthTenant.Application.Interfaces;
using AuthTenant.Domain.Entities;

namespace AuthTenant.Api.Controllers;

/// <summary>
/// Controller for tenant management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(ITenantRepository tenantRepository, ILogger<TenantsController> logger)
    {
        _tenantRepository = tenantRepository;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new tenant.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CreateTenantResult>> CreateTenant(
        [FromBody] CreateTenantCommand command,
        CancellationToken cancellationToken)
    {
        // TODO: Implement business logic
        _logger.LogInformation("Creating tenant: {TenantName}", command.Name);
        
        var tenant = Tenant.Create(command.Name, command.ContactEmail);
        await _tenantRepository.AddAsync(tenant, cancellationToken);
        // TODO: Save changes via UnitOfWork
        
        return CreatedAtAction(
            nameof(GetTenant), 
            new { id = tenant.Id }, 
            new CreateTenantResult(tenant.Id, tenant.Name));
    }

    /// <summary>
    /// Gets a tenant by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Tenant>> GetTenant(Guid id, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id, cancellationToken);
        
        if (tenant is null)
        {
            return NotFound();
        }
        
        return Ok(tenant);
    }

    /// <summary>
    /// Gets all tenants.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tenant>>> GetTenants(CancellationToken cancellationToken)
    {
        var tenants = await _tenantRepository.GetAllAsync(cancellationToken);
        return Ok(tenants);
    }
}
