# Rewards Service API Testing Guide

**Base URL:** `http://localhost:5004`

---

## 1. Rewards Catalog Management

### 1.1 Create Reward ✅ TESTED
**Endpoint:** `POST /api/tenants/{tenantId}/rewards`

**Request Body:**
```json
{
  "Name": "$10 Gift Card",
  "Description": "Redeem for $10 off your next purchase",
  "PointsCost": 1000,
  "RewardType": "gift_card",
  "RewardValue": "{\"amount\": 10}"
}
```

**Expected Response (201 Created):**
```json
{
  "id": "57f17f5d-6f17-48a3-a5a9-1139a03d8263",
  "tenantId": "7925045c-01b7-4609-a837-9359cd04b206",
  "name": "$10 Gift Card",
  "description": "Redeem for $10 off your next purchase",
  "pointsCost": 1000,
  "totalQuantity": null,
  "isActive": true,
  "createdAt": "2026-01-10T19:56:41.422127Z",
  "updatedAt": "2026-01-10T19:56:41.422131Z"
}
```

**Test Scenarios:**
- ✅ Valid reward creation
- ✅ Reward persists to database
- ❌ Missing required fields (400 Bad Request)

---

### 1.2 Get All Rewards ✅ TESTED
**Endpoint:** `GET /api/tenants/{tenantId}/rewards?activeOnly=true`

**Example:**
```
GET /api/tenants/7925045c-01b7-4609-a837-9359cd04b206/rewards
```

**Expected Response (200 OK):**
```json
[
  {
    "id": "57f17f5d-6f17-48a3-a5a9-1139a03d8263",
    "tenantId": "7925045c-01b7-4609-a837-9359cd04b206",
    "name": "$10 Gift Card",
    "description": "Redeem for $10 off your next purchase",
    "pointsCost": 1000,
    "totalQuantity": null,
    "isActive": true,
    "createdAt": "2026-01-10T19:56:41.422127Z",
    "updatedAt": "2026-01-10T19:56:41.422131Z"
  }
]
```

**Query Parameters:**
- `activeOnly` (optional, default: true) - Filter for active rewards only

---

### 1.3 Get Reward by ID ✅ TESTED
**Endpoint:** `GET /api/tenants/{tenantId}/rewards/{rewardId}`

**Example:**
```
GET /api/tenants/7925045c-01b7-4609-a837-9359cd04b206/rewards/57f17f5d-6f17-48a3-a5a9-1139a03d8263
```

**Expected Response (200 OK):**
Same structure as individual reward in Get All Rewards

---

### 1.4 Update Reward ✅ TESTED
**Endpoint:** `PUT /api/tenants/{tenantId}/rewards/{rewardId}`

**Request Body:**
```json
{
  "Name": "$15 Gift Card",
  "Description": "Updated description",
  "PointsCost": 1500,
  "RewardType": "gift_card",
  "RewardValue": "{\"amount\": 15}"
}
```

**Expected Response (200 OK):**
```json
{
  "id": "57f17f5d-6f17-48a3-a5a9-1139a03d8263",
  "tenantId": "7925045c-01b7-4609-a837-9359cd04b206",
  "name": "$15 Gift Card",
  "description": "Updated description",
  "pointsCost": 1500,
  "totalQuantity": null,
  "isActive": true,
  "createdAt": "2026-01-10T19:56:41.422127Z",
  "updatedAt": "2026-01-10T19:57:03.112607Z"
}
```

**Test Scenarios:**
- ✅ Valid update
- ✅ UpdatedAt timestamp changes
- ❌ Non-existent reward ID (404 Not Found)

---

### 1.5 Delete Reward ✅ TESTED
**Endpoint:** `DELETE /api/tenants/{tenantId}/rewards/{rewardId}`

**Expected Response (200 OK)**

**Test Scenarios:**
- ✅ Valid deletion
- ❌ Non-existent reward ID (404 Not Found)

---

## 2. Redemptions (Coming Soon)

### 2.1 Redeem Reward
**Endpoint:** `POST /api/tenants/{tenantId}/customers/{customerId}/redemptions`

**Status:** Not yet implemented

---

## Postman Collection Setup

### Environment Variables
```json
{
  "base_url": "http://localhost:5004",
  "tenant_id": "7925045c-01b7-4609-a837-9359cd04b206",
  "reward_id": "{{created_reward_id}}"
}
```

### Pre-request Scripts
```javascript
// Save reward ID from create response
pm.environment.set("created_reward_id", pm.response.json().id);
```

---

## Test Data Sets

### Reward 1: Gift Card
```json
{
  "Name": "$10 Gift Card",
  "Description": "Redeem for $10 off your next purchase",
  "PointsCost": 1000,
  "RewardType": "gift_card",
  "RewardValue": "{\"amount\": 10}"
}
```

### Reward 2: Free Shipping
```json
{
  "Name": "Free Shipping",
  "Description": "Free shipping on your next order",
  "PointsCost": 500,
  "RewardType": "shipping",
  "RewardValue": "{\"type\": \"free_shipping\"}"
}
```

### Reward 3: Discount Coupon
```json
{
  "Name": "20% Off Coupon",
  "Description": "20% discount on your next purchase",
  "PointsCost": 2000,
  "RewardType": "discount",
  "RewardValue": "{\"percentage\": 20}"
}
```

---

## Test Flow

### Complete Rewards Catalog Flow
1. Create reward → `POST /rewards`
2. Get all rewards → `GET /rewards`
3. Get specific reward → `GET /rewards/{id}`
4. Update reward → `PUT /rewards/{id}`
5. Get updated reward → `GET /rewards/{id}` (verify changes)
6. Delete reward → `DELETE /rewards/{id}`
7. Verify deletion → `GET /rewards` (should not appear)

---

## Implementation Status

| Feature | Status |
|---------|--------|
| Create Reward | ✅ Working |
| Get All Rewards | ✅ Working |
| Get Reward by ID | ✅ Working |
| Update Reward | ✅ Working |
| Delete Reward | ✅ Working |
| Redeem Reward | 🔸 Not implemented |
| Get Redemptions | 🔸 Not implemented |
