using Microsoft.AspNetCore.Mvc;
using PointsEngine.Application.Interfaces;
using PointsEngine.Domain.Entities;

namespace PointsEngine.Api.Controllers;

/// <summary>
/// Controller for managing points earning rules.
/// </summary>
[ApiController]
[Route("api/tenants/{tenantId:guid}/[controller]")]
public class RulesController : ControllerBase
{
    private readonly IRuleService _ruleService;
    private readonly ILogger<RulesController> _logger;

    public RulesController(IRuleService ruleService, ILogger<RulesController> logger)
    {
        _ruleService = ruleService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new points earning rule.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RuleResponse>> CreateRule(
        Guid tenantId,
        [FromBody] CreateRuleRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating rule {RuleName} for tenant {TenantId}", request.Name, tenantId);

        var command = new CreateRuleCommand(
            tenantId,
            request.Name,
            request.EventType,
            request.RuleDefinition,
            request.Priority,
            request.Description,
            request.ValidFrom,
            request.ValidUntil);

        var result = await _ruleService.CreateRuleAsync(command, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Error });
        }

        return CreatedAtAction(
            nameof(GetRule),
            new { tenantId, ruleId = result.RuleId },
            new RuleResponse(result.RuleId, result.Name));
    }

    /// <summary>
    /// Gets all rules for a tenant.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Rule>>> GetRules(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var rules = await _ruleService.GetRulesAsync(tenantId, cancellationToken);
        return Ok(rules);
    }

    /// <summary>
    /// Gets a specific rule by ID.
    /// </summary>
    [HttpGet("{ruleId:guid}")]
    public async Task<ActionResult<Rule>> GetRule(
        Guid tenantId,
        Guid ruleId,
        CancellationToken cancellationToken)
    {
        var rule = await _ruleService.GetRuleByIdAsync(ruleId, tenantId, cancellationToken);

        if (rule == null)
        {
            return NotFound();
        }

        return Ok(rule);
    }

    /// <summary>
    /// Updates an existing rule.
    /// </summary>
    [HttpPut("{ruleId:guid}")]
    public async Task<ActionResult<RuleResponse>> UpdateRule(
        Guid tenantId,
        Guid ruleId,
        [FromBody] UpdateRuleRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating rule {RuleId} for tenant {TenantId}", ruleId, tenantId);

        var command = new UpdateRuleCommand(
            ruleId,
            tenantId,
            request.Name,
            request.Description,
            request.RuleDefinition,
            request.Priority,
            request.ValidUntil);

        var result = await _ruleService.UpdateRuleAsync(command, cancellationToken);

        if (!result.Success)
        {
            return NotFound(new { message = result.Error });
        }

        return Ok(new RuleResponse(result.RuleId, result.Name));
    }

    /// <summary>
    /// Deletes a rule.
    /// </summary>
    [HttpDelete("{ruleId:guid}")]
    public async Task<IActionResult> DeleteRule(
        Guid tenantId,
        Guid ruleId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting rule {RuleId} for tenant {TenantId}", ruleId, tenantId);

        try
        {
            await _ruleService.DeleteRuleAsync(ruleId, tenantId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Activates a rule.
    /// </summary>
    [HttpPost("{ruleId:guid}/activate")]
    public async Task<IActionResult> ActivateRule(
        Guid tenantId,
        Guid ruleId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _ruleService.ActivateRuleAsync(ruleId, tenantId, cancellationToken);
            return Ok(new { message = "Rule activated" });
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Deactivates a rule.
    /// </summary>
    [HttpPost("{ruleId:guid}/deactivate")]
    public async Task<IActionResult> DeactivateRule(
        Guid tenantId,
        Guid ruleId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _ruleService.DeactivateRuleAsync(ruleId, tenantId, cancellationToken);
            return Ok(new { message = "Rule deactivated" });
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}

// Request DTOs
public record CreateRuleRequest(
    string Name,
    string EventType,
    string RuleDefinition,
    int Priority = 0,
    string? Description = null,
    DateTime? ValidFrom = null,
    DateTime? ValidUntil = null);

public record UpdateRuleRequest(
    string? Name = null,
    string? Description = null,
    string? RuleDefinition = null,
    int? Priority = null,
    DateTime? ValidUntil = null);

// Response DTOs
public record RuleResponse(Guid RuleId, string Name);
