# LoyaltyForge API Testing Guide

Complete API testing documentation for all services.

---

## 📁 Testing Files

| Service | File | Port | Status |
|---------|------|------|--------|
| Auth+Tenant | [AuthTenant-API-Tests.md](./AuthTenant-API-Tests.md) | 5001 | ✅ Working |
| Points Engine | [PointsEngine-API-Tests.md](./PointsEngine-API-Tests.md) | 5003 | ✅ Working |
| Rewards | [Rewards-API-Tests.md](./Rewards-API-Tests.md) | 5004 | 🔸 Scaffold |
| E-commerce | [EcommerceIntegration-API-Tests.md](./EcommerceIntegration-API-Tests.md) | 5002 | 🔸 Scaffold |
| API Gateway | [ApiGateway-API-Tests.md](./ApiGateway-API-Tests.md) | 5005 | 🔸 Scaffold |

---

## 🚀 Quick Start

### 1. Start Services
```bash
# Start all services
docker compose up --build

# Or start specific services
docker compose up postgres rabbitmq auth-tenant points-engine -d
```

### 2. Verify Health
```bash
curl http://localhost:5001/health  # Auth+Tenant
curl http://localhost:5003/health  # Points Engine
```

### 3. Import to Postman
1. Open Postman
2. Import → File → Select testing markdown files
3. Set up environment variables (see each service guide)

---

## 🧪 End-to-End Test Flow

### Complete User Journey
```
1. Create Tenant (AuthTenant)
   POST /api/tenants
   
2. Register Customer (AuthTenant)
   POST /api/tenants/{id}/users
   
3. Login (AuthTenant)
   POST /api/auth/login
   
4. Create Earning Rule (PointsEngine)
   POST /api/tenants/{id}/rules
   
5. Earn Points (PointsEngine)
   POST /api/tenants/{id}/customers/{cid}/points/earn
   
6. Check Balance (PointsEngine)
   GET /api/tenants/{id}/customers/{cid}/points/balance
   
7. Create Reward (Rewards)
   POST /api/tenants/{id}/rewards
   
8. Redeem Reward (Rewards)
   POST /api/tenants/{id}/customers/{cid}/redemptions
```

---

## 📊 Test Coverage

### Auth+Tenant Service
- [x] Tenant CRUD
- [x] User registration
- [x] Login with password hashing
- [x] Multi-tenancy (same email, different tenants)
- [ ] JWT token generation
- [ ] Role-based access

### Points Engine Service
- [x] Rules CRUD
- [x] Rule activation/deactivation
- [x] Earn points (with idempotency)
- [x] Balance queries
- [x] Transaction history
- [ ] Deduct points
- [ ] Point expiration

### Rewards Service
- [ ] Reward catalog CRUD
- [ ] Redemption flow
- [ ] Point deduction integration

### E-commerce Integration
- [ ] Shopify webhook handling
- [ ] Event transformation
- [ ] Order event publishing

---

## 🔧 Environment Setup

### Global Variables (All Services)
```json
{
  "auth_base_url": "http://localhost:5001",
  "points_base_url": "http://localhost:5003",
  "rewards_base_url": "http://localhost:5004",
  "ecommerce_base_url": "http://localhost:5002",
  "gateway_base_url": "http://localhost:5005"
}
```

### Shared Test Data
```json
{
  "test_tenant_id": "7925045c-01b7-4609-a837-9359cd04b206",
  "test_customer_id": "550e8400-e29b-41d4-a716-446655440000",
  "test_tenant_slug": "acme-store"
}
```

---

## 📝 Testing Best Practices

### 1. Use Idempotency Keys
Always include unique idempotency keys for operations that modify state:
```json
{
  "idempotencyKey": "test-{{$timestamp}}"
}
```

### 2. Chain Requests
Use Postman's test scripts to save IDs for subsequent requests:
```javascript
pm.environment.set("tenant_id", pm.response.json().tenantId);
```

### 3. Verify Responses
Add assertions in Postman tests:
```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Balance is correct", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData.availablePoints).to.eql(100);
});
```

### 4. Test Error Cases
Don't just test happy paths:
- Invalid IDs (404)
- Duplicate operations (409)
- Missing required fields (400)
- Insufficient permissions (403)

---

## 🐛 Common Issues & Solutions

### Issue: Connection Refused
**Solution:** Ensure Docker containers are running
```bash
docker compose ps
docker compose logs auth-tenant
```

### Issue: 404 Not Found
**Solution:** Check service is running on correct port
```bash
curl http://localhost:5001/health
```

### Issue: Database Errors
**Solution:** Verify PostgreSQL schema is initialized
```bash
docker compose exec postgres psql -U postgres -d loyaltyforge -c "\dt auth.*"
```

---

## 📈 Performance Testing

### Load Test Scenarios
1. **Concurrent Earns:** 100 customers earning points simultaneously
2. **Balance Queries:** 1000 requests/second
3. **Rule Evaluation:** Complex rules with multiple conditions

### Tools
- **Artillery:** For load testing
- **k6:** For performance benchmarking
- **Postman Runner:** For basic load tests

---

## 🔐 Security Testing

### Authentication Tests
- [ ] Login with invalid credentials
- [ ] Expired tokens
- [ ] Token tampering
- [ ] Cross-tenant access attempts

### Authorization Tests
- [ ] Customer accessing admin endpoints
- [ ] Accessing other tenant's data
- [ ] Missing authorization headers

---

## 📚 Additional Resources

- [Postman Documentation](https://learning.postman.com/)
- [API Design Best Practices](https://swagger.io/resources/articles/best-practices-in-api-design/)
- [Testing Microservices](https://martinfowler.com/articles/microservice-testing/)
