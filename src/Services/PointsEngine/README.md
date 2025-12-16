# Points Engine Service

Core loyalty points calculation and ledger management service.

## Responsibilities

- Consume order events from RabbitMQ
- Apply tenant-specific rules (to be implemented by human)
- Ledger-based point accounting (immutable entries)
- Maintain materialized balance views
- Emit PointsEarned and PointsReversed events
- Provide point deduction API for Rewards service

## Architecture

```
PointsEngine.Api/           → API controllers
PointsEngine.Application/   → Interfaces, queries, event handlers
PointsEngine.Domain/        → Entities (ledger, balance, rules)
PointsEngine.Infrastructure/→ EF Core, repositories, messaging
```

## Key Entities

- **PointsLedgerEntry** - Immutable transaction record
- **PointsBalance** - Materialized view of current balance
- **PointsRule** - Tenant-specific calculation rules

## API Endpoints

### Points (tenant + customer scoped)
- `GET /api/tenants/{tenantId}/customers/{customerId}/points/balance` - Get balance
- `GET /api/tenants/{tenantId}/customers/{customerId}/points/transactions` - Get history
- `POST /api/tenants/{tenantId}/customers/{customerId}/points/deduct` - Deduct points

## Event Consumption

Subscribes to:
- `OrderPlacedEvent` → Calculates and awards points

Publishes:
- `PointsEarnedEvent`
- `PointsReversedEvent`

## Running Locally

```bash
cd src/Services/PointsEngine/PointsEngine.Api
dotnet run
```

## Health Check

```bash
curl http://localhost:5003/health
```
