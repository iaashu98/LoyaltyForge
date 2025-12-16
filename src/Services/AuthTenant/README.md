# Auth + Tenant Service

Multi-tenant authentication and tenant management service.

## Responsibilities

- Tenant creation and management
- Tenant-scoped user management
- JWT token issuance with tenant claims
- API key generation and validation
- Basic RBAC (Role-Based Access Control)

## Architecture

```
AuthTenant.Api/           → API controllers, Program.cs
AuthTenant.Application/   → Commands, queries, interfaces
AuthTenant.Domain/        → Entities, value objects
AuthTenant.Infrastructure/→ EF Core, repositories
```

## API Endpoints

### Tenants
- `POST /api/tenants` - Create tenant
- `GET /api/tenants/{id}` - Get tenant
- `GET /api/tenants` - List tenants

### Authentication
- `POST /api/auth/login` - Authenticate user

### Users (tenant-scoped)
- `POST /api/tenants/{tenantId}/users` - Register user
- `GET /api/tenants/{tenantId}/users/{id}` - Get user
- `GET /api/tenants/{tenantId}/users` - List users

### API Keys (tenant-scoped)
- `POST /api/tenants/{tenantId}/apikeys` - Generate API key
- `DELETE /api/tenants/{tenantId}/apikeys/{id}` - Revoke API key

## Running Locally

```bash
cd src/Services/AuthTenant/AuthTenant.Api
dotnet run
```

## Health Check

```bash
curl http://localhost:5001/health
```
