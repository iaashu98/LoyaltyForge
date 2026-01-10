# Auth+Tenant Service API Testing Guide

**Base URL:** `http://localhost:5001`

---

## 1. Tenant Management

### 1.1 Create Tenant
**Endpoint:** `POST /api/tenants`

**Request Body:**
```json
{
  "name": "Acme Store",
  "slug": "acme-store",
  "contactEmail": "admin@acme.com"
}
```

**Expected Response (201 Created):**
```json
{
  "tenantId": "7925045c-01b7-4609-a837-9359cd04b206",
  "name": "Acme Store",
  "slug": "acme-store"
}
```

**Test Scenarios:**
- ✅ Valid tenant creation
- ❌ Duplicate slug (409 Conflict)
- ❌ Missing required fields (400 Bad Request)

---

### 1.2 Get Tenant by ID
**Endpoint:** `GET /api/tenants/{tenantId}`

**Example:**
```
GET /api/tenants/7925045c-01b7-4609-a837-9359cd04b206
```

**Expected Response (200 OK):**
```json
{
  "id": "7925045c-01b7-4609-a837-9359cd04b206",
  "name": "Acme Store",
  "slug": "acme-store",
  "status": "active",
  "settings": "{\"contactEmail\":\"admin@acme.com\"}",
  "createdAt": "2026-01-10T10:00:00Z",
  "updatedAt": "2026-01-10T10:00:00Z"
}
```

---

### 1.3 Get Tenant by Slug
**Endpoint:** `GET /api/tenants/by-slug/{slug}`

**Example:**
```
GET /api/tenants/by-slug/acme-store
```

**Expected Response (200 OK):**
Same as Get Tenant by ID

---

## 2. User Management

### 2.1 Register User
**Endpoint:** `POST /api/tenants/{tenantId}/users`

**Request Body:**
```json
{
  "email": "john@example.com",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe",
  "userType": "customer"
}
```

**Expected Response (201 Created):**
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "john@example.com",
  "tenantId": "7925045c-01b7-4609-a837-9359cd04b206"
}
```

**Test Scenarios:**
- ✅ New user registration
- ❌ Duplicate email in same tenant (409 Conflict)
- ✅ Same email in different tenant (allowed)
- ❌ Weak password (400 Bad Request)
- ❌ Invalid tenant ID (404 Not Found)

---

### 2.2 Get User
**Endpoint:** `GET /api/tenants/{tenantId}/users/{userId}`

**Example:**
```
GET /api/tenants/7925045c-01b7-4609-a837-9359cd04b206/users/550e8400-e29b-41d4-a716-446655440000
```

**Expected Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "john@example.com",
  "emailVerified": false,
  "status": "active",
  "provider": "local",
  "createdAt": "2026-01-10T10:05:00Z"
}
```

---

## 3. Authentication

### 3.1 Login
**Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
  "email": "john@example.com",
  "password": "SecurePass123!",
  "tenantSlug": "acme-store"
}
```

**Expected Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "tenantId": "7925045c-01b7-4609-a837-9359cd04b206",
  "expiresAt": "2026-01-11T10:00:00Z"
}
```

**Test Scenarios:**
- ✅ Valid credentials
- ❌ Wrong password (401 Unauthorized)
- ❌ Non-existent email (401 Unauthorized)
- ❌ Invalid tenant slug (404 Not Found)
- ❌ Suspended user (403 Forbidden)

---

## Postman Collection Setup

### Environment Variables
```json
{
  "base_url": "http://localhost:5001",
  "tenant_id": "{{created_tenant_id}}",
  "user_id": "{{created_user_id}}",
  "auth_token": "{{login_token}}"
}
```

### Pre-request Scripts (for chaining requests)
```javascript
// Save tenant ID from create response
pm.environment.set("created_tenant_id", pm.response.json().tenantId);

// Save user ID from register response
pm.environment.set("created_user_id", pm.response.json().userId);

// Save auth token from login
pm.environment.set("login_token", pm.response.json().token);
```

---

## Test Data Sets

### Tenant 1: Acme Store
```json
{
  "name": "Acme Store",
  "slug": "acme-store",
  "contactEmail": "admin@acme.com"
}
```

### Tenant 2: Beta Shop
```json
{
  "name": "Beta Shop",
  "slug": "beta-shop",
  "contactEmail": "admin@beta.com"
}
```

### User 1: Customer
```json
{
  "email": "customer@example.com",
  "password": "Customer123!",
  "firstName": "Jane",
  "lastName": "Customer",
  "userType": "customer"
}
```

### User 2: Admin
```json
{
  "email": "admin@example.com",
  "password": "Admin123!",
  "firstName": "Admin",
  "lastName": "User",
  "userType": "admin"
}
```
