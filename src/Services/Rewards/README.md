# Rewards Service

Manages reward catalog and handles reward redemptions.

## Responsibilities

- Tenant-scoped reward catalog CRUD
- Redemption flow with idempotency
- Point deduction via Points Engine
- Emit RewardRedeemed events

## Architecture

```
Rewards.Api/           → API controllers
Rewards.Application/   → Commands, interfaces
Rewards.Domain/        → Entities (Reward, Redemption)
Rewards.Infrastructure/→ EF Core, repositories
```

## API Endpoints

### Reward Catalog (tenant-scoped)
- `GET /api/tenants/{tenantId}/rewards` - List rewards
- `GET /api/tenants/{tenantId}/rewards/{id}` - Get reward
- `POST /api/tenants/{tenantId}/rewards` - Create reward

### Redemptions (tenant-scoped)
- `POST /api/tenants/{tenantId}/redemptions` - Redeem reward
- `GET /api/tenants/{tenantId}/redemptions/customers/{customerId}` - Get customer redemptions
- `GET /api/tenants/{tenantId}/redemptions/{id}` - Get redemption

## Idempotency

All redemption requests require an `IdempotencyKey` to prevent duplicate redemptions.

## Running Locally

```bash
cd src/Services/Rewards/Rewards.Api
dotnet run
```

## Health Check

```bash
curl http://localhost:5004/health
```
