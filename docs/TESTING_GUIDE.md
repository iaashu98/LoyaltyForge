# EDA End-to-End Testing Guide

> **Purpose**: Test the complete Event-Driven Architecture implementation across all services  
> **Last Updated**: 2026-01-11

## Prerequisites

### 1. Start Required Services

```bash
# Start RabbitMQ
docker-compose up -d rabbitmq

# Start PostgreSQL (if not already running)
docker-compose up -d postgres

# Verify RabbitMQ is running
docker ps | grep rabbitmq
```

### 2. Apply Database Migrations

```bash
# E-commerce Integration
cd src/Services/EcommerceIntegration/EcommerceIntegration.Api
dotnet ef database update

# Points Engine
cd ../../../PointsEngine/PointsEngine.Api
dotnet ef database update

# Rewards
cd ../../../Rewards/Rewards.Api
dotnet ef database update
```

### 3. Start All Services

Open 3 terminal windows and run:

```bash
# Terminal 1: E-commerce Integration
cd src/Services/EcommerceIntegration/EcommerceIntegration.Api
dotnet run

# Terminal 2: Points Engine
cd src/Services/PointsEngine/PointsEngine.Api
dotnet run

# Terminal 3: Rewards Service
cd src/Services/Rewards/Rewards.Api
dotnet run
```

**Default Ports:**
- E-commerce Integration: `http://localhost:5001`
- Points Engine: `http://localhost:5003`
- Rewards: `http://localhost:5004`

---

## Test Flow 1: Order → Points Earning

### Step 1: Create Test Data

**Create a Tenant:**
```bash
curl -X POST http://localhost:5003/api/tenants \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Tenant",
    "slug": "test-tenant"
  }'
```

**Create a Customer (User Balance):**
```bash
TENANT_ID="<tenant-id-from-above>"
CUSTOMER_ID="$(uuidgen)"

curl -X POST "http://localhost:5003/api/tenants/$TENANT_ID/balances" \
  -H "Content-Type: application/json" \
  -d "{
    \"userId\": \"$CUSTOMER_ID\"
  }"
```

### Step 2: Simulate Shopify Order

**Send OrderPlacedEvent via E-commerce Integration:**

```bash
curl -X POST http://localhost:5001/api/webhooks/shopify/orders/create \
  -H "Content-Type: application/json" \
  -H "X-Shopify-Hmac-SHA256: test-signature" \
  -d "{
    \"id\": 12345678,
    \"email\": \"customer@example.com\",
    \"total_price\": \"150.00\",
    \"currency\": \"USD\",
    \"customer\": {
      \"id\": \"$CUSTOMER_ID\",
      \"email\": \"customer@example.com\"
    },
    \"line_items\": [
      {
        \"id\": 1,
        \"title\": \"Test Product\",
        \"quantity\": 2,
        \"price\": \"75.00\"
      }
    ]
  }"
```

### Step 3: Verify Points Earned

**Check RabbitMQ Management UI:**
1. Open `http://localhost:15672` (guest/guest)
2. Go to **Queues** tab
3. Verify `points-engine` queue received the message
4. Check message was consumed (should be 0 messages)

**Check Points Engine Database:**
```bash
# Check outbox messages were published
curl "http://localhost:5003/api/tenants/$TENANT_ID/ledger/$CUSTOMER_ID"
```

**Expected Result:**
- Customer should have **150 points** (1 point per dollar)
- Ledger entry created with description: "Order #12345678 - $150.00"

---

## Test Flow 2: Reward Redemption (Full Saga)

### Step 1: Create a Reward

```bash
curl -X POST "http://localhost:5004/api/tenants/$TENANT_ID/rewards" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Free Coffee",
    "description": "Redeem for a free coffee",
    "pointsCost": 50,
    "isActive": true,
    "category": "Food & Beverage"
  }'
```

Save the `rewardId` from the response.

### Step 2: Redeem the Reward

```bash
REWARD_ID="<reward-id-from-above>"
IDEMPOTENCY_KEY="$(uuidgen)"

curl -X POST "http://localhost:5004/api/tenants/$TENANT_ID/redemptions" \
  -H "Content-Type: application/json" \
  -d "{
    \"customerId\": \"$CUSTOMER_ID\",
    \"rewardId\": \"$REWARD_ID\",
    \"idempotencyKey\": \"$IDEMPOTENCY_KEY\"
  }"
```

### Step 3: Verify Saga Execution

**Check RabbitMQ:**
1. Verify `points.commands` queue received `DeductPointsCommand`
2. Verify `rewards-service` queue received `PointsDeductedEvent`

**Check Points Engine:**
```bash
# Verify points were deducted
curl "http://localhost:5003/api/tenants/$TENANT_ID/ledger/$CUSTOMER_ID"
```

**Expected Result:**
- Customer balance: **100 points** (150 - 50)
- New ledger entry with type "Deduction"

**Check Rewards Service:**
```bash
# Get redemption status
curl "http://localhost:5004/api/tenants/$TENANT_ID/redemptions/customers/$CUSTOMER_ID"
```

**Expected Result:**
- Redemption status: `"Fulfilled"`
- Points spent: `50`

---

## Test Flow 3: Insufficient Balance (Failure Path)

### Step 1: Attempt Redemption with Insufficient Points

```bash
# Create expensive reward
curl -X POST "http://localhost:5004/api/tenants/$TENANT_ID/rewards" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Premium Item",
    "description": "Costs more than available balance",
    "pointsCost": 500,
    "isActive": true,
    "category": "Premium"
  }'

EXPENSIVE_REWARD_ID="<reward-id-from-above>"
IDEMPOTENCY_KEY_2="$(uuidgen)"

curl -X POST "http://localhost:5004/api/tenants/$TENANT_ID/redemptions" \
  -H "Content-Type: application/json" \
  -d "{
    \"customerId\": \"$CUSTOMER_ID\",
    \"rewardId\": \"$EXPENSIVE_REWARD_ID\",
    \"idempotencyKey\": \"$IDEMPOTENCY_KEY_2\"
  }"
```

### Step 2: Verify Failure Handling

**Check RabbitMQ:**
- Verify `PointsDeductionFailedEvent` was published

**Check Rewards Service:**
```bash
curl "http://localhost:5004/api/tenants/$TENANT_ID/redemptions/customers/$CUSTOMER_ID"
```

**Expected Result:**
- Redemption status: `"Failed"`
- Failure reason: "Insufficient balance"

**Check Points Engine:**
```bash
curl "http://localhost:5003/api/tenants/$TENANT_ID/ledger/$CUSTOMER_ID"
```

**Expected Result:**
- Balance unchanged: **100 points**
- No new ledger entry created

---

## Test Flow 4: Idempotency

### Step 1: Retry Same Redemption

```bash
# Use the SAME idempotency key from Test Flow 2
curl -X POST "http://localhost:5004/api/tenants/$TENANT_ID/redemptions" \
  -H "Content-Type: application/json" \
  -d "{
    \"customerId\": \"$CUSTOMER_ID\",
    \"rewardId\": \"$REWARD_ID\",
    \"idempotencyKey\": \"$IDEMPOTENCY_KEY\"
  }"
```

**Expected Result:**
- Returns existing redemption
- No duplicate points deduction
- Balance remains: **100 points**

---

## Monitoring & Debugging

### RabbitMQ Management UI

**Access:** `http://localhost:15672` (guest/guest)

**Check:**
- **Exchanges**: `loyaltyforge.events` should exist
- **Queues**: 
  - `ecommerce` (E-commerce Integration)
  - `points-engine` (Points Engine)
  - `points.commands` (Points Engine commands)
  - `rewards-service` (Rewards Service)
- **Messages**: Monitor message rates and consumption

### Database Queries

**Check Outbox Messages:**
```sql
-- E-commerce Integration
SELECT * FROM integration.outbox_messages 
WHERE processed_at IS NULL 
ORDER BY created_at DESC;

-- Points Engine
SELECT * FROM points.outbox_messages 
WHERE processed_at IS NULL 
ORDER BY created_at DESC;
```

**Check Ledger Entries:**
```sql
SELECT * FROM points.ledger_entries 
WHERE user_id = '<customer-id>' 
ORDER BY created_at DESC;
```

**Check Redemptions:**
```sql
SELECT * FROM rewards.reward_redemptions 
WHERE user_id = '<customer-id>' 
ORDER BY created_at DESC;
```

### Application Logs

Monitor console output for each service:
- Look for `"Processing OrderPlacedEvent"`
- Look for `"Processing DeductPointsCommand"`
- Look for `"Handling PointsDeductedEvent"`
- Check for any errors or exceptions

---

## Common Issues & Troubleshooting

### Issue: Messages Not Being Consumed

**Check:**
1. RabbitMQ is running: `docker ps | grep rabbitmq`
2. Services are connected to RabbitMQ (check logs)
3. Queues are bound to exchange correctly
4. Consumer background services are running

**Fix:**
```bash
# Restart RabbitMQ
docker-compose restart rabbitmq

# Restart affected service
# Ctrl+C and dotnet run again
```

### Issue: Outbox Messages Not Publishing

**Check:**
1. `OutboxPublisher` background service is registered
2. Database connection is working
3. Check `outbox_messages` table for pending messages

**Fix:**
```sql
-- Manually mark messages as processed to retry
UPDATE integration.outbox_messages 
SET processed_at = NULL, retry_count = 0 
WHERE id = '<message-id>';
```

### Issue: Points Not Deducted

**Check:**
1. `DeductPointsCommandHandler` is registered
2. Command consumer is subscribed to `points.commands` queue
3. Check Points Engine logs for errors

---

## Success Criteria

✅ **All tests pass if:**

1. **Order → Points Earning:**
   - OrderPlacedEvent published to outbox
   - Event consumed by Points Engine
   - Points added to customer balance
   - Ledger entry created

2. **Reward Redemption:**
   - DeductPointsCommand sent to Points Engine
   - Points deducted successfully
   - PointsDeductedEvent published
   - Redemption marked as "Fulfilled"

3. **Insufficient Balance:**
   - PointsDeductionFailedEvent published
   - Redemption marked as "Failed"
   - No points deducted

4. **Idempotency:**
   - Duplicate requests return existing result
   - No duplicate processing

---

## Next Steps

After successful testing:
1. Update `walkthrough.md` with test results
2. Update `EDA_IMPLEMENTATION_GUIDE.md` with final status
3. Consider adding automated integration tests
4. Document any edge cases discovered during testing
