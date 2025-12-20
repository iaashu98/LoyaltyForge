using Microsoft.AspNetCore.Mvc;
using PointsEngine.Application.Interfaces;
using PointsEngine.Domain.Entities;

namespace PointsEngine.Api.Controllers;

/// <summary>
/// Controller for points balance and transaction operations.
/// Routes: /api/tenants/{tenantId}/customers/{customerId}/points
/// </summary>
[ApiController]
[Route("api/tenants/{tenantId:guid}/customers/{customerId:guid}/points")]
public class PointsController : ControllerBase
{
    private readonly IBalanceService _balanceService;
    private readonly ILedgerService _ledgerService;
    private readonly ILogger<PointsController> _logger;

    public PointsController(
        IBalanceService balanceService,
        ILedgerService ledgerService,
        ILogger<PointsController> logger)
    {
        _balanceService = balanceService;
        _ledgerService = ledgerService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current points balance for a customer.
    /// </summary>
    [HttpGet("balance")]
    public async Task<ActionResult<BalanceResult>> GetBalance(
        Guid tenantId,
        Guid customerId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting balance for customer {CustomerId} in tenant {TenantId}",
            customerId,
            tenantId);

        var balance = await _balanceService.GetBalanceAsync(tenantId, customerId, cancellationToken);
        return Ok(balance);
    }

    /// <summary>
    /// Gets the transaction history (ledger entries) for a customer.
    /// </summary>
    [HttpGet("transactions")]
    public async Task<ActionResult<IEnumerable<LedgerEntry>>> GetTransactions(
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

        var transactions = await _ledgerService.GetTransactionHistoryAsync(
            tenantId, customerId, page, pageSize, cancellationToken);

        return Ok(new
        {
            items = transactions,
            page,
            pageSize,
            totalCount = transactions.Count // TODO: Add proper count query
        });
    }

    /// <summary>
    /// Earns points for a customer (typically called by event handler).
    /// Idempotent - uses idempotency key.
    /// </summary>
    [HttpPost("earn")]
    public async Task<ActionResult<LedgerResult>> EarnPoints(
        Guid tenantId,
        Guid customerId,
        [FromBody] EarnPointsRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Earning {Amount} points for customer {CustomerId} in tenant {TenantId}",
            request.Amount,
            customerId,
            tenantId);

        var command = new EarnPointsCommand(
            tenantId,
            customerId,
            request.Amount,
            request.SourceType,
            request.SourceId,
            request.RuleId,
            request.IdempotencyKey,
            request.Description,
            request.ExpiresAt);

        var result = await _ledgerService.EarnPointsAsync(command, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(result);
    }

    /// <summary>
    /// Deducts points for a redemption (called by Rewards service).
    /// Idempotent - uses idempotency key.
    /// </summary>
    [HttpPost("deduct")]
    public async Task<ActionResult<LedgerResult>> DeductPoints(
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

        var command = new DeductPointsCommand(
            tenantId,
            customerId,
            request.Amount,
            request.SourceType,
            request.SourceId,
            request.IdempotencyKey,
            request.Description);

        var result = await _ledgerService.DeductPointsAsync(command, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(result);
    }

    /// <summary>
    /// Checks if customer has sufficient points for a redemption.
    /// </summary>
    [HttpGet("check/{requiredPoints:long}")]
    public async Task<ActionResult<SufficientPointsResult>> CheckSufficientPoints(
        Guid tenantId,
        Guid customerId,
        long requiredPoints,
        CancellationToken cancellationToken)
    {
        var hasSufficient = await _balanceService.HasSufficientPointsAsync(
            tenantId, customerId, requiredPoints, cancellationToken);

        return Ok(new SufficientPointsResult(hasSufficient, requiredPoints));
    }
}

// Request DTOs
public record EarnPointsRequest(
    long Amount,
    string SourceType,
    string IdempotencyKey,
    Guid? SourceId = null,
    Guid? RuleId = null,
    string? Description = null,
    DateTime? ExpiresAt = null);

public record DeductPointsRequest(
    long Amount,
    string SourceType,
    string IdempotencyKey,
    Guid? SourceId = null,
    string? Description = null);

// Response DTOs
public record SufficientPointsResult(bool HasSufficientPoints, long RequiredPoints);
