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
    private readonly IRewardRepository _rewardRepository;

    public RewardsController(ILogger<RewardsController> logger, IRewardRepository rewardRepository)
    {
        _logger = logger;
        _rewardRepository = rewardRepository;
    }

    /// <summary>
    /// Gets all rewards in the tenant catalog.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRewards(Guid tenantId, [FromQuery] bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting rewards for tenant {TenantId}", tenantId);

        var rewards = await _rewardRepository.GetAllByTenantAsync(tenantId, activeOnly, cancellationToken);
        return Ok(rewards.Select(x => new RewardResponse(x)));
    }

    /// <summary>
    /// Gets a specific reward by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetReward(Guid tenantId, Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting reward {RewardId} for tenant {TenantId}", id, tenantId);

        var reward = await _rewardRepository.GetByIdAsync(id, cancellationToken);
        if (reward is null)
        {
            return NotFound();
        }
        return Ok(new RewardResponse(reward));
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

        var reward = RewardCatalog.Create(
            tenantId,
            request.Name,
            request.PointsCost,
            request.RewardType,
            request.RewardValue,
            request.Description);

        await _rewardRepository.AddAsync(reward, cancellationToken);
        await _rewardRepository.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetReward), new { tenantId, id = reward.Id }, new RewardResponse(reward));
    }

    /// <summary>
    /// Updates an existing reward in the catalog.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<RewardResponse> UpdateReward(
        Guid tenantId,
        Guid id,
        [FromBody] UpdateRewardRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating reward {RewardId} for tenant {TenantId}", id, tenantId);

        var item = await _rewardRepository.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            throw new Exception("Reward not found");
        }

        item.Update(
            request.Name,
            request.PointsCost,
            request.RewardType,
            request.RewardValue,
            request.Description);
        await _rewardRepository.UpdateAsync(item, cancellationToken);
        await _rewardRepository.SaveChangesAsync(cancellationToken);
        return new RewardResponse(item);
    }

    /// <summary>
    /// Deletes a reward from the catalog.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteReward(Guid tenantId, Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting reward {RewardId} for tenant {TenantId}", id, tenantId);

        var reward = await _rewardRepository.GetByIdAsync(id, cancellationToken);
        if (reward is null)
        {
            return NotFound();
        }
        await _rewardRepository.DeleteAsync(reward, cancellationToken);
        await _rewardRepository.SaveChangesAsync(cancellationToken);
        return Ok();
    }


}

public record CreateRewardRequest(
    string Name,
    long PointsCost,
    string RewardType,
    string RewardValue,
    string? Description,
    int? TotalQuantity = null);

public record UpdateRewardRequest(
    string Name,
    long PointsCost,
    string RewardType,
    string RewardValue,
    string? Description,
    int? TotalQuantity = null);

public record RewardResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Description,
    long PointsCost,
    int? TotalQuantity,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public RewardResponse(RewardCatalog reward) : this(
        reward.Id,
        reward.TenantId,
        reward.Name,
        reward.Description,
        reward.PointsCost,
        reward.TotalQuantity,
        reward.IsActive,
        reward.CreatedAt,
        reward.UpdatedAt)
    {
    }
}