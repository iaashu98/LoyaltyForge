using Microsoft.AspNetCore.Mvc;
using Rewards.Application.Commands;
using Rewards.Application.Interfaces;

namespace Rewards.Api.Controllers;

/// <summary>
/// Controller for reward redemption operations.
/// </summary>
[ApiController]
[Route("api/tenants/{tenantId:guid}/[controller]")]
public class RedemptionsController : ControllerBase
{
    private readonly ILogger<RedemptionsController> _logger;
    // TODO: Inject IRedemptionRepository, IRewardRepository, IPointsServiceClient

    public RedemptionsController(ILogger<RedemptionsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Redeems a reward for a customer.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RedeemRewardResult>> RedeemReward(
        Guid tenantId,
        [FromBody] RedeemRewardRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Redemption request: Customer {CustomerId} redeeming reward {RewardId} with key {IdempotencyKey}",
            request.CustomerId,
            request.RewardId,
            request.IdempotencyKey);

        // TODO: Implement redemption flow:
        // 1. Check idempotency key to prevent duplicates
        // 2. Validate reward exists and is active
        // 3. Check customer has sufficient points
        // 4. Deduct points via Points Engine
        // 5. Create redemption record
        // 6. Publish RewardRedeemed event
        
        return Ok(new RedeemRewardResult(
            RedemptionId: Guid.NewGuid(),
            Success: true,
            Error: null));
    }

    /// <summary>
    /// Gets redemption history for a customer.
    /// </summary>
    [HttpGet("customers/{customerId:guid}")]
    public async Task<IActionResult> GetCustomerRedemptions(
        Guid tenantId,
        Guid customerId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting redemptions for customer {CustomerId} in tenant {TenantId}",
            customerId,
            tenantId);

        // TODO: Implement
        return Ok(Array.Empty<object>());
    }

    /// <summary>
    /// Gets a specific redemption by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRedemption(
        Guid tenantId,
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting redemption {Id} for tenant {TenantId}", id, tenantId);
        
        // TODO: Implement
        return NotFound();
    }
}

public record RedeemRewardRequest(
    Guid CustomerId,
    Guid RewardId,
    string IdempotencyKey);
