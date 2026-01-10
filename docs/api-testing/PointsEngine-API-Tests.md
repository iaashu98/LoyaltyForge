# Points Engine Service API Testing Guide

**Base URL:** `http://localhost:5003`

---

## 1. Rules Management

### 1.1 Create Rule
**Endpoint:** `POST /api/tenants/{tenantId}/rules`

**Request Body:**
```json
{
  "name": "10% Back on Orders",
  "eventType": "order.completed",
  "ruleDefinition": "{\"type\":\"percentage\",\"value\":10}",
  "priority": 1,
  "description": "Earn 10% of order value as points"
}
```

**Expected Response (201 Created):**
```json
{
  "ruleId": "92e8b154-8f16-44d3-96e9-b06f351dcbc6",
  "name": "10% Back on Orders"
}
```

**Test Scenarios:**
- ✅ Create percentage-based rule
- ✅ Create fixed-amount rule
- ❌ Duplicate rule name in tenant (409 Conflict)
- ❌ Invalid JSON in ruleDefinition (400 Bad Request)

---

### 1.2 Get All Rules
**Endpoint:** `GET /api/tenants/{tenantId}/rules`

**Example:**
```
GET /api/tenants/7925045c-01b7-4609-a837-9359cd04b206/rules
```

**Expected Response (200 OK):**
```json
[
  {
    "id": "92e8b154-8f16-44d3-96e9-b06f351dcbc6",
    "tenantId": "7925045c-01b7-4609-a837-9359cd04b206",
    "name": "10% Back on Orders",
    "description": "Earn 10% of order value as points",
    "eventType": "order.completed",
    "ruleDefinition": "{\"type\":\"percentage\",\"value\":10}",
    "priority": 1,
    "isActive": true,
    "validFrom": "2026-01-10T10:00:00Z",
    "validUntil": null,
    "createdAt": "2026-01-10T10:00:00Z",
    "updatedAt": "2026-01-10T10:00:00Z"
  }
]
```

---

### 1.3 Get Rule by ID
**Endpoint:** `GET /api/tenants/{tenantId}/rules/{ruleId}`

**Example:**
```
GET /api/tenants/7925045c-01b7-4609-a837-9359cd04b206/rules/92e8b154-8f16-44d3-96e9-b06f351dcbc6
```

**Expected Response (200 OK):**
Same structure as individual rule in Get All Rules

---

### 1.4 Update Rule
**Endpoint:** `PUT /api/tenants/{tenantId}/rules/{ruleId}`

**Request Body:**
```json
{
  "name": "15% Back on Orders",
  "ruleDefinition": "{\"type\":\"percentage\",\"value\":15}",
  "priority": 2
}
```

**Expected Response (200 OK):**
```json
{
  "ruleId": "92e8b154-8f16-44d3-96e9-b06f351dcbc6",
  "name": "15% Back on Orders"
}
```

---

### 1.5 Activate Rule
**Endpoint:** `POST /api/tenants/{tenantId}/rules/{ruleId}/activate`

**Expected Response (200 OK):**
```json
{
  "message": "Rule activated"
}
```

---

### 1.6 Deactivate Rule
**Endpoint:** `POST /api/tenants/{tenantId}/rules/{ruleId}/deactivate`

**Expected Response (200 OK):**
```json
{
  "message": "Rule deactivated"
}
```

---

### 1.7 Delete Rule
**Endpoint:** `DELETE /api/tenants/{tenantId}/rules/{ruleId}`

**Expected Response (204 No Content)**

---

## 2. Points Operations

### 2.1 Earn Points
**Endpoint:** `POST /api/tenants/{tenantId}/customers/{customerId}/points/earn`

**Request Body:**
```json
{
  "amount": 100,
  "sourceType": "order",
  "sourceId": "550e8400-e29b-41d4-a716-446655440001",
  "ruleId": "92e8b154-8f16-44d3-96e9-b06f351dcbc6",
  "idempotencyKey": "order-001-earn",
  "description": "First purchase bonus",
  "expiresAt": "2027-01-10T00:00:00Z"
}
```

**Expected Response (200 OK):**
```json
{
  "ledgerEntryId": "71ea90c9-8de3-41b2-844f-05e59620a310",
  "balanceAfter": 100,
  "success": true,
  "error": null
}
```

**Test Scenarios:**
- ✅ First earn for new customer (creates balance)
- ✅ Second earn for existing customer (accumulates)
- ✅ Duplicate idempotency key (returns existing result)
- ❌ Negative amount (400 Bad Request)

---

### 2.2 Deduct Points ✅ TESTED
**Endpoint:** `POST /api/tenants/{tenantId}/customers/{customerId}/points/deduct`

**Request Body:**
```json
{
  "Amount": 50,
  "SourceType": "redemption",
  "IdempotencyKey": "test-deduct-001",
  "Description": "$5 gift card"
}
```

**Expected Response (200 OK):**
```json
{
  "ledgerEntryId": "fc60b98d-e33d-4fbb-89cd-a85518ff520d",
  "balanceAfter": 100,
  "success": true,
  "error": null
}
```

**Test Scenarios:**
- ✅ Valid deduction with sufficient balance
- ✅ Insufficient balance returns error message
- ✅ Duplicate idempotency key returns existing result
- ❌ Negative amount (400 Bad Request)

**Insufficient Balance Example:**
```json
{
  "Amount": 500,
  "SourceType": "redemption",
  "IdempotencyKey": "test-deduct-002"
}
```

**Response:**
```json
{
  "message": "Insufficient balance. Available: 100, Required: 500"
}
```

---

### 2.3 Get Balance
**Endpoint:** `GET /api/tenants/{tenantId}/customers/{customerId}/points/balance`

**Example:**
```
GET /api/tenants/7925045c-01b7-4609-a837-9359cd04b206/customers/550e8400-e29b-41d4-a716-446655440000/points/balance
```

**Expected Response (200 OK):**
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "availablePoints": 150,
  "pendingPoints": 0,
  "lifetimeEarned": 200,
  "lifetimeRedeemed": 50,
  "lastUpdatedAt": "2026-01-10T10:30:00Z"
}
```

---

### 2.4 Get Transaction History
**Endpoint:** `GET /api/tenants/{tenantId}/customers/{customerId}/points/transactions?page=1&pageSize=20`

**Example:**
```
GET /api/tenants/7925045c-01b7-4609-a837-9359cd04b206/customers/550e8400-e29b-41d4-a716-446655440000/points/transactions?page=1&pageSize=10
```

**Expected Response (200 OK):**
```json
{
  "items": [
    {
      "id": "71ea90c9-8de3-41b2-844f-05e59620a310",
      "tenantId": "7925045c-01b7-4609-a837-9359cd04b206",
      "userId": "550e8400-e29b-41d4-a716-446655440000",
      "idempotencyKey": "order-001-earn",
      "entryType": "earn",
      "pointsAmount": 100,
      "balanceAfter": 100,
      "sourceType": "order",
      "sourceId": "550e8400-e29b-41d4-a716-446655440001",
      "ruleId": "92e8b154-8f16-44d3-96e9-b06f351dcbc6",
      "description": "First purchase bonus",
      "expiresAt": "2027-01-10T00:00:00Z",
      "createdAt": "2026-01-10T10:25:00Z"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1
}
```

---

### 2.5 Check Sufficient Points
**Endpoint:** `GET /api/tenants/{tenantId}/customers/{customerId}/points/check/{requiredPoints}`

**Example:**
```
GET /api/tenants/7925045c-01b7-4609-a837-9359cd04b206/customers/550e8400-e29b-41d4-a716-446655440000/points/check/100
```

**Expected Response (200 OK):**
```json
{
  "hasSufficientPoints": true,
  "requiredPoints": 100
}
```

---

## Postman Collection Setup

### Environment Variables
```json
{
  "base_url": "http://localhost:5003",
  "tenant_id": "7925045c-01b7-4609-a837-9359cd04b206",
  "customer_id": "550e8400-e29b-41d4-a716-446655440000",
  "rule_id": "{{created_rule_id}}"
}
```

### Pre-request Scripts
```javascript
// Generate unique idempotency key
pm.environment.set("idempotency_key", "test-" + Date.now());

// Save rule ID
pm.environment.set("created_rule_id", pm.response.json().ruleId);
```

---

## Test Scenarios & Flows

### Flow 1: Complete Points Lifecycle
1. Create tenant (AuthTenant service)
2. Register customer (AuthTenant service)
3. Create earning rule → `POST /rules`
4. Earn points (first time) → `POST /points/earn`
5. Check balance → `GET /points/balance`
6. Earn more points → `POST /points/earn`
7. Check updated balance → `GET /points/balance`
8. Deduct points → `POST /points/deduct`
9. View transaction history → `GET /points/transactions`

### Flow 2: Idempotency Testing
1. Earn 100 points with key `test-001`
2. Repeat same request with key `test-001` → Should return same ledger entry ID
3. Check balance → Should still be 100 (not 200)

### Flow 3: Multi-Customer Testing
1. Create Customer A
2. Create Customer B
3. Earn 100 points for Customer A
4. Earn 50 points for Customer B
5. Verify Customer A balance = 100
6. Verify Customer B balance = 50

---

## Test Data Sets

### Rule 1: Percentage-based
```json
{
  "name": "10% Back on Orders",
  "eventType": "order.completed",
  "ruleDefinition": "{\"type\":\"percentage\",\"value\":10}",
  "priority": 1
}
```

### Rule 2: Fixed Amount
```json
{
  "name": "Welcome Bonus",
  "eventType": "user.registered",
  "ruleDefinition": "{\"type\":\"fixed\",\"value\":500}",
  "priority": 10
}
```

### Earn Request 1
```json
{
  "amount": 100,
  "sourceType": "order",
  "sourceId": "order-12345",
  "idempotencyKey": "order-12345-earn",
  "description": "Purchase reward"
}
```

### Deduct Request 1
```json
{
  "amount": 50,
  "sourceType": "redemption",
  "sourceId": "redemption-001",
  "idempotencyKey": "redemption-001-deduct",
  "description": "$5 gift card"
}
```
