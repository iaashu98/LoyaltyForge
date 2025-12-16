# E-commerce Integration Service

Receives webhooks from e-commerce platforms and transforms them to canonical events.

## Responsibilities

- Receive Shopify webhooks
- Validate webhook signatures
- Transform platform-specific payloads to canonical events
- Publish events to RabbitMQ

## Supported Platforms

- **Shopify** (primary)
- Future: WooCommerce, BigCommerce, etc.

## Architecture

```
EcommerceIntegration.Api/           → Webhook controllers
EcommerceIntegration.Application/   → Interfaces, DTOs
EcommerceIntegration.Domain/        → Entities
EcommerceIntegration.Infrastructure/→ Platform-specific implementations
```

## API Endpoints

### Shopify Webhooks
- `POST /api/webhooks/shopify/orders/create` - Order created
- `POST /api/webhooks/shopify/orders/paid` - Order paid

## Webhook Configuration

Configure in Shopify Admin:
1. Settings → Notifications → Webhooks
2. Create webhook for `orders/create`
3. Set URL to: `https://your-domain/api/webhooks/shopify/orders/create`

## Running Locally

```bash
cd src/Services/EcommerceIntegration/EcommerceIntegration.Api
dotnet run
```

## Health Check

```bash
curl http://localhost:5002/health
```
