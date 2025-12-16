using Microsoft.AspNetCore.Mvc;
using AuthTenant.Application.Commands;

namespace AuthTenant.Api.Controllers;

/// <summary>
/// Controller for API key management operations.
/// </summary>
[ApiController]
[Route("api/tenants/{tenantId:guid}/[controller]")]
public class ApiKeysController : ControllerBase
{
    private readonly ILogger<ApiKeysController> _logger;
    // TODO: Inject IApiKeyRepository

    public ApiKeysController(ILogger<ApiKeysController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generates a new API key for a tenant.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<GenerateApiKeyResult>> GenerateApiKey(
        Guid tenantId,
        [FromBody] GenerateApiKeyCommand command,
        CancellationToken cancellationToken)
    {
        // TODO: Implement API key generation
        // 1. Generate secure random key
        // 2. Hash key for storage
        // 3. Store key with prefix
        // 4. Return raw key (once only)
        
        _logger.LogInformation("Generating API key '{Name}' for tenant {TenantId}", command.Name, tenantId);
        
        // Placeholder response
        return Ok(new GenerateApiKeyResult(
            Guid.NewGuid(),
            command.Name,
            "lf_placeholder_key_12345",
            "lf_",
            command.ExpiresAt));
    }

    /// <summary>
    /// Revokes an API key.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> RevokeApiKey(Guid tenantId, Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement API key revocation
        _logger.LogInformation("Revoking API key {KeyId} for tenant {TenantId}", id, tenantId);
        
        return NoContent();
    }
}
