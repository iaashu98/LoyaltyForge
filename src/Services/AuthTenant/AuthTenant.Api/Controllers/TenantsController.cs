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
        _logger.LogInformation("Creating tenant: {TenantName} with slug {Slug}", command.Name, command.Slug);

        // Check if slug already exists
        var existing = await _tenantRepository.GetBySlugAsync(command.Slug, cancellationToken);
        if (existing != null)
        {
            return Conflict(new { message = $"Tenant with slug '{command.Slug}' already exists" });
        }

        var tenant = Tenant.Create(command.Name, command.Slug, command.ContactEmail);
        await _tenantRepository.AddAsync(tenant, cancellationToken);

        return CreatedAtAction(
            nameof(GetTenant),
            new { id = tenant.Id },
            new CreateTenantResult(tenant.Id, tenant.Name, tenant.Slug));
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
    /// Gets a tenant by slug.
    /// </summary>
    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<Tenant>> GetTenantBySlug(string slug, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetBySlugAsync(slug, cancellationToken);

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
