# API Gateway

Entry point for all external API requests. Handles routing, authentication, and rate limiting.

## Responsibilities

- Token validation (JWT)
- Tenant resolution from claims
- Request routing to services via YARP
- Rate limiting per IP/client

## Technology

- **YARP** - Microsoft's reverse proxy library
- **AspNetCoreRateLimit** - Rate limiting middleware

## Routes

| Path Pattern | Destination |
|--------------|-------------|
| `/api/auth/*` | Auth + Tenant Service |
| `/api/tenants/*` | Auth + Tenant Service |
| `/api/webhooks/*` | E-commerce Integration |
| `/api/.../points/*` | Points Engine |
| `/api/.../rewards/*` | Rewards Service |
| `/api/.../redemptions/*` | Rewards Service |

## Rate Limits

Default limits (configurable):
- 100 requests per minute per IP
- 1000 requests per hour per IP

## Running Locally

```bash
cd src/Services/ApiGateway/ApiGateway.Api
dotnet run
```

## Health Check

```bash
curl http://localhost:5005/health
```
