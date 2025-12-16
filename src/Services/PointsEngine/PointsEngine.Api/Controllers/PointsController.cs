using Microsoft.AspNetCore.Mvc;
using PointsEngine.Application.Interfaces;
using PointsEngine.Application.Queries;

namespace PointsEngine.Api.Controllers;

/// <summary>
/// Controller for points balance operations.
/// </summary>
[ApiController]
[Route("api/tenants/{tenantId:guid}/customers/{customerId:guid}/[controller]")]
public class PointsController : ControllerBase
{
    private readonly ILogger<PointsController> _logger;
    // TODO: Inject IPointsBalanceRepository, IPointsLedgerRepository

    public PointsController(ILogger<PointsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets the current points balance for a customer.
    /// </summary>
    [HttpGet("balance")]
    public async Task<ActionResult<GetBalanceResult>> GetBalance(
        Guid tenantId,
        Guid customerId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting balance for customer {CustomerId} in tenant {TenantId}", 
            customerId, 
            tenantId);

        // TODO: Implement balance retrieval
        return Ok(new GetBalanceResult(
            customerId,
            CurrentBalance: 0,
            LifetimeEarned: 0,
            LifetimeSpent: 0,
            LastUpdatedAt: DateTime.UtcNow));
    }

    /// <summary>
    /// Gets the transaction history for a customer.
    /// </summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        Guid tenantId,
        Guid customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting transactions for customer {CustomerId} in tenant {TenantId}", 
            customerId, 
            tenantId);

        // TODO: Implement transaction history retrieval
        return Ok(new { items = Array.Empty<object>(), page, pageSize, totalCount = 0 });
    }

    /// <summary>
    /// Deducts points for a redemption (called by Rewards service).
    /// </summary>
    [HttpPost("deduct")]
    public async Task<IActionResult> DeductPoints(
        Guid tenantId,
        Guid customerId,
        [FromBody] DeductPointsRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Deducting {Amount} points from customer {CustomerId} in tenant {TenantId}", 
            request.Amount, 
            customerId, 
            tenantId);

        // TODO: Implement point deduction with idempotency check
        return Ok(new { transactionId = Guid.NewGuid(), newBalance = 0 });
    }
}

public record DeductPointsRequest(int Amount, string Reason, string IdempotencyKey);
