using Microsoft.AspNetCore.Mvc;
using Rewards.Application.Interfaces;
using Rewards.Domain.Entities;

namespace Rewards.Api.Controllers;

/// <summary>
/// Controller for reward catalog management.
/// </summary>
[ApiController]
[Route("api/tenants/{tenantId:guid}/[controller]")]
public class RewardsController : ControllerBase
{
    private readonly ILogger<RewardsController> _logger;
    // TODO: Inject IRewardRepository

    public RewardsController(ILogger<RewardsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets all rewards in the tenant catalog.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRewards(Guid tenantId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting rewards for tenant {TenantId}", tenantId);
        
        // TODO: Implement
        return Ok(Array.Empty<Reward>());
    }

    /// <summary>
    /// Gets a specific reward by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetReward(Guid tenantId, Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting reward {RewardId} for tenant {TenantId}", id, tenantId);
        
        // TODO: Implement
        return NotFound();
    }

    /// <summary>
    /// Creates a new reward in the catalog.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateReward(
        Guid tenantId,
        [FromBody] CreateRewardRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating reward '{Name}' for tenant {TenantId}", request.Name, tenantId);
        
        // TODO: Implement
        var reward = Reward.Create(
            tenantId,
            request.Name,
            request.PointsCost,
            request.RewardType,
            request.Description);
        
        return CreatedAtAction(nameof(GetReward), new { tenantId, id = reward.Id }, reward);
    }
}

public record CreateRewardRequest(
    string Name,
    int PointsCost,
    string RewardType,
    string? Description);
