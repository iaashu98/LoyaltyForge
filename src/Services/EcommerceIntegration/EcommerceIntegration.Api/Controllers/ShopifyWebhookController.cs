using Microsoft.AspNetCore.Mvc;
using EcommerceIntegration.Application.Interfaces;
using EcommerceIntegration.Application.DTOs.Shopify;
using LoyaltyForge.Messaging.RabbitMQ;
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
    private readonly ILogger<ShopifyWebhookController> _logger;
    // TODO: Inject IEventPublisher

    public ShopifyWebhookController(
        IWebhookSignatureValidator signatureValidator,
        IEventTransformer<ShopifyOrderPayload> orderTransformer,
        ILogger<ShopifyWebhookController> logger)
    {
        _signatureValidator = signatureValidator;
        _orderTransformer = orderTransformer;
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

        // TODO: Publish to RabbitMQ
        _logger.LogInformation("Transformed order {OrderId} to canonical event", shopifyOrder.Id);

        return Ok(new { received = true });
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
