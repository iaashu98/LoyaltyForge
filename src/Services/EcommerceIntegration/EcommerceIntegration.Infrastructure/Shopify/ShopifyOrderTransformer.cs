using EcommerceIntegration.Application.DTOs.Shopify;
using EcommerceIntegration.Application.Interfaces;
using LoyaltyForge.Contracts.Events;

namespace EcommerceIntegration.Infrastructure.Shopify;

/// <summary>
/// Transforms Shopify order events to canonical OrderPlaced events.
/// </summary>
public class ShopifyOrderTransformer : IEventTransformer<ShopifyOrderPayload>
{
    public IntegrationEvent Transform(Guid tenantId, ShopifyOrderPayload sourceEvent)
    {
        // TODO: Implement transformation logic
        // 1. Map Shopify fields to canonical event
        // 2. Handle missing/optional fields
        // 3. Convert types appropriately
        
        var lineItems = sourceEvent.LineItems?.Select(li => new OrderLineItem
        {
            ProductId = li.ProductId?.ToString() ?? string.Empty,
            ProductName = li.Title ?? string.Empty,
            Quantity = li.Quantity,
            UnitPrice = decimal.TryParse(li.Price, out var price) ? price : 0,
            LineTotal = decimal.TryParse(li.Price, out var p) ? p * li.Quantity : 0
        }).ToList() ?? new List<OrderLineItem>();

        return new OrderPlacedEvent
        {
            TenantId = tenantId,
            ExternalOrderId = sourceEvent.Id.ToString(),
            CustomerId = Guid.NewGuid(), // TODO: Map from customer lookup
            CustomerEmail = sourceEvent.Customer?.Email ?? sourceEvent.Email ?? string.Empty,
            OrderTotal = decimal.TryParse(sourceEvent.TotalPrice, out var total) ? total : 0,
            Currency = sourceEvent.Currency ?? "USD",
            LineItems = lineItems,
            SourcePlatform = "Shopify"
        };
    }
}
