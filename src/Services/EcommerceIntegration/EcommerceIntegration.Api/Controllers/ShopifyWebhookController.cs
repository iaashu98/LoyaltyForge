using Microsoft.AspNetCore.Mvc;
using EcommerceIntegration.Application.Interfaces;
using EcommerceIntegration.Application.DTOs.Shopify;
using LoyaltyForge.Messaging.RabbitMQ;
using LoyaltyForge.Common.Outbox;
using LoyaltyForge.Contracts.Events;
using System.Text.Json;

namespace EcommerceIntegration.Api.Controllers;

/// <summary>
/// Controller for receiving Shopify webhooks.
/// </summary>
[ApiController]
[Route("api/webhooks/shopify")]
public class ShopifyWebhookController : ControllerBase
{
    private readonly IWebhookSignatureValidator _signatureValidator;
    private readonly IEventTransformer<ShopifyOrderPayload> _orderTransformer;
    private readonly IOutboxRepository _outboxRepository;
    private readonly ILogger<ShopifyWebhookController> _logger;

    public ShopifyWebhookController(
        IWebhookSignatureValidator signatureValidator,
        IEventTransformer<ShopifyOrderPayload> orderTransformer,
        IOutboxRepository outboxRepository,
        ILogger<ShopifyWebhookController> logger)
    {
        _signatureValidator = signatureValidator;
        _orderTransformer = orderTransformer;
        _outboxRepository = outboxRepository;
        _logger = logger;
    }

    /// <summary>
    /// Receives order/create webhook from Shopify.
    /// </summary>
    [HttpPost("orders/create")]
    public async Task<IActionResult> OrderCreated(
        [FromHeader(Name = "X-Shopify-Hmac-Sha256")] string signature,
        [FromHeader(Name = "X-Shopify-Shop-Domain")] string shopDomain,
        CancellationToken cancellationToken)
    {
        // Read raw body for signature validation
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);

        _logger.LogInformation("Received Shopify order webhook from {ShopDomain}", shopDomain);

        // TODO: Lookup tenant by shop domain
        // TODO: Get webhook secret from integration config
        var webhookSecret = "placeholder_secret";
        var tenantId = Guid.NewGuid(); // TODO: Resolve from shop domain

        // Validate signature
        if (!_signatureValidator.ValidateSignature(rawBody, signature, webhookSecret))
        {
            _logger.LogWarning("Invalid webhook signature from {ShopDomain}", shopDomain);
            return Unauthorized("Invalid signature");
        }

        // Deserialize and transform
        var shopifyOrder = JsonSerializer.Deserialize<ShopifyOrderPayload>(rawBody);
        if (shopifyOrder is null)
        {
            return BadRequest("Invalid payload");
        }

        var canonicalEvent = _orderTransformer.Transform(tenantId, shopifyOrder);

        // Save event to outbox for reliable publishing
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = canonicalEvent.EventType,
            Payload = JsonSerializer.Serialize(canonicalEvent),
            CreatedAt = DateTime.UtcNow,
            TenantId = tenantId
        };

        await _outboxRepository.AddAsync(outboxMessage, cancellationToken);

        _logger.LogInformation(
            "Saved OrderPlacedEvent to outbox for order {OrderId}, tenant {TenantId}",
            shopifyOrder.Id,
            tenantId);

        return Ok(new { received = true, eventId = canonicalEvent.EventId });
    }

    /// <summary>
    /// Receives order/paid webhook from Shopify.
    /// </summary>
    [HttpPost("orders/paid")]
    public async Task<IActionResult> OrderPaid(
        [FromHeader(Name = "X-Shopify-Hmac-Sha256")] string signature,
        [FromHeader(Name = "X-Shopify-Shop-Domain")] string shopDomain,
        CancellationToken cancellationToken)
    {
        // TODO: Implement order paid handling
        _logger.LogInformation("Received order/paid webhook from {ShopDomain}", shopDomain);
        return Ok(new { received = true });
    }
}
