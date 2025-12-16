# LoyaltyForge

A multi-tenant Loyalty & Rewards SaaS platform built with .NET 9, Clean Architecture, PostgreSQL, and RabbitMQ.

## Architecture

- **Auth + Tenant Service** - Tenant management, JWT authentication, API keys, RBAC
- **E-commerce Integration** - Shopify webhooks, event transformation, canonical events
- **Points Engine** - Ledger-based point accounting, rules engine, event consumption
- **Rewards Service** - Reward catalog, redemption flow, point deduction
- **API Gateway** - Token validation, tenant resolution, rate limiting

## Quick Start

```bash
# Start infrastructure
docker-compose up -d

# Restore and build
dotnet restore
dotnet build

# Run all services
dotnet run --project src/Services/AuthTenant/AuthTenant.Api
dotnet run --project src/Services/EcommerceIntegration/EcommerceIntegration.Api
dotnet run --project src/Services/PointsEngine/PointsEngine.Api
dotnet run --project src/Services/Rewards/Rewards.Api
dotnet run --project src/Services/ApiGateway/ApiGateway.Api
```

## Project Structure

```
LoyaltyForge/
├── src/
│   ├── Services/           # Microservices
│   └── Shared/            # Shared libraries
├── contracts/             # Event & API contracts
└── tests/                 # Test projects
```

## Technology Stack

- **.NET 9** - Runtime
- **PostgreSQL** - Database (EF Core)
- **RabbitMQ** - Async messaging
- **Docker** - Containerization
- **Clean Architecture** - Project structure

## License

Proprietary - All rights reserved.
