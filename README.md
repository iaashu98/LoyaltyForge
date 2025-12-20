# LoyaltyForge

A multi-tenant Loyalty & Rewards SaaS platform built with **.NET 9**, **Clean Architecture**, **PostgreSQL**, and **RabbitMQ**.

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           API Gateway (:5005)                          │
│               Token validation, tenant resolution, rate limiting        │
└───────────────────────────────┬────────────────────────────────────────┘
                                │
        ┌───────────────────────┼───────────────────────┐
        │                       │                       │
        ▼                       ▼                       ▼
┌───────────────┐      ┌───────────────┐      ┌───────────────┐
│  Auth+Tenant  │      │   Points      │      │   Rewards     │
│    (:5001)    │      │   Engine      │      │   (:5004)     │
│               │      │   (:5003)     │      │               │
│  • Tenants    │      │  • Rules      │      │  • Catalog    │
│  • Users      │      │  • Ledger     │      │  • Redemption │
│  • Roles      │      │  • Balances   │      │               │
└───────────────┘      └───────────────┘      └───────────────┘
                                │
                                │ Events
                                ▼
                    ┌───────────────────────┐
                    │   E-commerce          │
                    │   Integration (:5002) │
                    │   • Shopify webhooks  │
                    │   • Event transform   │
                    └───────────────────────┘
                                │
                        ┌───────┴───────┐
                        ▼               ▼
               ┌─────────────┐  ┌─────────────┐
               │ PostgreSQL  │  │  RabbitMQ   │
               │   (:5432)   │  │   (:5672)   │
               └─────────────┘  └─────────────┘
```

## 🚀 Quick Start

### Using Docker Compose (Recommended)
```bash
# Start everything (infrastructure + all services)
docker compose up --build

# Or start only infrastructure
docker compose up postgres rabbitmq -d
```

### Using the Run Script
```bash
# Start all services
./scripts/run.sh

# Start specific services
./scripts/run.sh -i        # Infrastructure only
./scripts/run.sh -a        # Auth+Tenant only
./scripts/run.sh -p        # Points Engine only

# Stop all services
./scripts/run.sh -s
```

### Manual Development
```bash
dotnet restore
dotnet build

# Run all services
dotnet run --project src/Services/AuthTenant/AuthTenant.Api
dotnet run --project src/Services/EcommerceIntegration/EcommerceIntegration.Api
dotnet run --project src/Services/PointsEngine/PointsEngine.Api
dotnet run --project src/Services/Rewards/Rewards.Api
dotnet run --project src/Services/ApiGateway/ApiGateway.Api
```

## 📁 Project Structure

```
LoyaltyForge/
├── src/
│   ├── Services/
│   │   ├── AuthTenant/              # Auth + Tenant Service
│   │   │   ├── AuthTenant.Api/         • Controllers, Program.cs
│   │   │   ├── AuthTenant.Application/ • Services, Interfaces
│   │   │   ├── AuthTenant.Domain/      • Entities (Tenant, User, Role)
│   │   │   └── AuthTenant.Infrastructure/ • EF Core, Repositories
│   │   │
│   │   ├── PointsEngine/            # Points Engine Service
│   │   │   ├── PointsEngine.Api/       • RulesController, PointsController
│   │   │   ├── PointsEngine.Application/ • Services (Rule, Ledger, Balance)
│   │   │   ├── PointsEngine.Domain/    • Entities (Rule, LedgerEntry, UserBalance)
│   │   │   └── PointsEngine.Infrastructure/ • Repositories, UnitOfWork
│   │   │
│   │   ├── Rewards/                 # Rewards Service
│   │   ├── EcommerceIntegration/    # E-commerce Integration
│   │   └── ApiGateway/              # API Gateway
│   │
│   └── Shared/
│       ├── LoyaltyForge.Common/     # Common interfaces (IUnitOfWork)
│       ├── LoyaltyForge.Contracts/  # Shared DTOs & Events
│       └── LoyaltyForge.Messaging/  # RabbitMQ abstractions
│
├── contracts/events/                # Event schemas (YAML)
├── scripts/
│   ├── schema.sql                   # PostgreSQL schema
│   ├── run.sh                       # Service runner script
│   └── init-databases.sh            # DB initialization
└── docker-compose.yml
```

## 🔌 API Endpoints

### Auth+Tenant Service (:5001)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/tenants` | Create tenant |
| GET | `/api/tenants/{id}` | Get tenant |
| GET | `/api/tenants/by-slug/{slug}` | Get tenant by slug |
| POST | `/api/tenants/{id}/users` | Register user |
| POST | `/api/auth/login` | Login (returns JWT) |

### Points Engine (:5003)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/tenants/{id}/rules` | Create earning rule |
| GET | `/api/tenants/{id}/rules` | List rules |
| PUT | `/api/tenants/{id}/rules/{ruleId}` | Update rule |
| POST | `/api/tenants/{id}/rules/{ruleId}/activate` | Activate rule |
| GET | `/api/tenants/{id}/customers/{cid}/points/balance` | Get balance |
| POST | `/api/tenants/{id}/customers/{cid}/points/earn` | Earn points |
| POST | `/api/tenants/{id}/customers/{cid}/points/deduct` | Deduct points |

## 🗄️ Database Schema

Single database with **schema-per-service** isolation:

| Schema | Service | Tables |
|--------|---------|--------|
| `auth` | AuthTenant | tenants, users, user_tenants, roles, user_roles |
| `points` | PointsEngine | rules, ledger_entries, user_balances, idempotency_keys |
| `rewards` | Rewards | catalog, redemptions |
| `integration` | EcommerceIntegration | webhook_logs, external_events |
| `gateway` | ApiGateway | api_keys, access_logs |
| `audit` | Shared | system_events |

### Key Design Principles
- **Immutable ledger** - `ledger_entries` is append-only (no UPDATE/DELETE)
- **Idempotency** - All event processing uses idempotency keys
- **Multi-tenancy** - All tables include `tenant_id`
- **Soft references** - Cross-schema FKs only to `auth.tenants`

## 🧩 Technology Stack

| Component | Technology |
|-----------|------------|
| Runtime | .NET 9 |
| Database | PostgreSQL 16 (EF Core) |
| Messaging | RabbitMQ 3 |
| Architecture | Clean Architecture |
| Containers | Docker Compose |
| Logging | Serilog |

## 📋 Implementation Status

| Service | Status | Notes |
|---------|--------|-------|
| Auth+Tenant | ✅ Working | Tenant/User CRUD, login, password hashing |
| Points Engine | ✅ Working | Rules CRUD, balance queries, skeleton for ledger |
| Rewards | 🔸 Scaffold | Controllers stubbed |
| E-commerce | 🔸 Scaffold | Webhook endpoint stubbed |
| API Gateway | 🔸 Scaffold | Basic routing |

## 📝 License

Proprietary - All rights reserved.
