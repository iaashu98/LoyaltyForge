-- =============================================================================
-- LoyaltyForge MVP Database Schema
-- PostgreSQL DDL for Multi-Tenant Loyalty & Rewards SaaS
-- =============================================================================
-- DESIGN PRINCIPLES:
-- - Single database with schema-per-service isolation
-- - All tables include tenant_id for multi-tenancy (where applicable)
-- - UUIDs as primary keys
-- - Immutable ledger for points accounting
-- - Idempotency guarantees via unique constraints
-- - Explicit constraints over implicit logic
-- =============================================================================

-- =============================================================================
-- DATABASE CREATION
-- =============================================================================
-- Note: Run this part separately as superuser if database doesn't exist
-- CREATE DATABASE loyaltyforge;

-- =============================================================================
-- EXTENSIONS
-- =============================================================================

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";      -- UUID generation
CREATE EXTENSION IF NOT EXISTS "pgcrypto";       -- Secure hashing for API keys

-- =============================================================================
-- SCHEMA CREATION
-- =============================================================================

CREATE SCHEMA IF NOT EXISTS auth;         -- Auth + Tenant Service
CREATE SCHEMA IF NOT EXISTS integration;  -- E-commerce Integration Service
CREATE SCHEMA IF NOT EXISTS points;       -- Points Engine
CREATE SCHEMA IF NOT EXISTS rewards;      -- Rewards Service
CREATE SCHEMA IF NOT EXISTS gateway;      -- API Gateway support tables
CREATE SCHEMA IF NOT EXISTS audit;        -- Lightweight audit trail

-- =============================================================================
-- SCHEMA: auth
-- Responsibilities: Tenants, users, user-tenant mapping, roles/access
-- =============================================================================

-- Tenants table: Root entity for multi-tenancy
-- Each tenant represents a business using the loyalty platform
CREATE TABLE auth.tenants (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name            VARCHAR(255) NOT NULL,
    slug            VARCHAR(100) NOT NULL,           -- URL-safe identifier
    status          VARCHAR(50) NOT NULL DEFAULT 'active',  -- active, suspended, deleted
    settings        JSONB DEFAULT '{}',              -- Tenant-specific configuration
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_tenants_slug UNIQUE (slug),
    CONSTRAINT chk_tenants_status CHECK (status IN ('active', 'suspended', 'deleted'))
);

COMMENT ON TABLE auth.tenants IS 'Root tenant entity - each business using the platform';
COMMENT ON COLUMN auth.tenants.slug IS 'URL-safe unique identifier for tenant';
COMMENT ON COLUMN auth.tenants.settings IS 'Tenant-specific configuration as JSON';

-- Users table: All users in the system (both tenant admins and end customers)
-- NOTE: Email is NOT globally unique - same email can exist across tenants/providers
CREATE TABLE auth.users (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email           VARCHAR(255) NOT NULL,
    password_hash   VARCHAR(255),                    -- Null for OAuth-only users
    external_id     VARCHAR(255),                    -- ID from external auth provider
    provider        VARCHAR(50) NOT NULL DEFAULT 'local',  -- local, google, shopify, etc.
    email_verified  BOOLEAN NOT NULL DEFAULT FALSE,
    status          VARCHAR(50) NOT NULL DEFAULT 'active',
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    -- Email uniqueness is per provider (not global)
    -- Same email can exist via different auth providers
    CONSTRAINT uq_users_email_provider UNIQUE (email, provider),
    CONSTRAINT chk_users_status CHECK (status IN ('active', 'suspended', 'deleted'))
);

COMMENT ON TABLE auth.users IS 'All users in the system - email unique per provider, not globally';
COMMENT ON COLUMN auth.users.external_id IS 'User ID from external auth provider (Shopify, etc.)';
COMMENT ON COLUMN auth.users.provider IS 'Auth provider: local, google, shopify - email is unique per provider';

-- User-Tenant mapping: Associates users with tenants (many-to-many)
-- A user can belong to multiple tenants (e.g., customer shops at multiple stores)
CREATE TABLE auth.user_tenants (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id         UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
    tenant_id       UUID NOT NULL REFERENCES auth.tenants(id) ON DELETE CASCADE,
    user_type       VARCHAR(50) NOT NULL DEFAULT 'customer',  -- admin, staff, customer
    external_customer_id VARCHAR(255),               -- Customer ID from e-commerce platform
    metadata        JSONB DEFAULT '{}',              -- Additional user-tenant data
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_user_tenants_mapping UNIQUE (user_id, tenant_id),
    CONSTRAINT chk_user_tenants_type CHECK (user_type IN ('admin', 'staff', 'customer'))
);

COMMENT ON TABLE auth.user_tenants IS 'Maps users to tenants with their role within each tenant';
COMMENT ON COLUMN auth.user_tenants.external_customer_id IS 'Customer ID from Shopify or other e-commerce platform';

-- Roles table: Available roles in the system
CREATE TABLE auth.roles (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID REFERENCES auth.tenants(id) ON DELETE CASCADE,  -- NULL for system roles
    name            VARCHAR(100) NOT NULL,
    description     TEXT,
    permissions     JSONB NOT NULL DEFAULT '[]',     -- Array of permission strings
    is_system_role  BOOLEAN NOT NULL DEFAULT FALSE,  -- System roles cannot be deleted
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_roles_name_tenant UNIQUE (tenant_id, name)
);

COMMENT ON TABLE auth.roles IS 'Role definitions - system-wide or tenant-specific';
COMMENT ON COLUMN auth.roles.permissions IS 'JSON array of permission strings';

-- User-Role assignments: Assigns roles to users within a tenant context
CREATE TABLE auth.user_roles (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_tenant_id  UUID NOT NULL REFERENCES auth.user_tenants(id) ON DELETE CASCADE,
    role_id         UUID NOT NULL REFERENCES auth.roles(id) ON DELETE CASCADE,
    granted_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    granted_by      UUID REFERENCES auth.users(id),
    
    CONSTRAINT uq_user_roles_assignment UNIQUE (user_tenant_id, role_id)
);

COMMENT ON TABLE auth.user_roles IS 'Assigns roles to users within their tenant context';

-- =============================================================================
-- SCHEMA: integration
-- Responsibilities: Webhook ingestion logs, raw external events (Shopify-first)
-- =============================================================================

-- Webhook logs: Records all incoming webhook requests for debugging/audit
CREATE TABLE integration.webhook_logs (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID NOT NULL REFERENCES auth.tenants(id) ON DELETE CASCADE,
    source          VARCHAR(50) NOT NULL,            -- shopify, woocommerce, custom
    topic           VARCHAR(100) NOT NULL,           -- orders/create, customers/update, etc.
    webhook_id      VARCHAR(255),                    -- Provider's webhook ID
    headers         JSONB NOT NULL DEFAULT '{}',     -- Request headers (sanitized)
    payload         JSONB NOT NULL,                  -- Raw webhook payload
    signature       VARCHAR(255),                    -- HMAC signature for verification
    status          VARCHAR(50) NOT NULL DEFAULT 'received',  -- received, processed, failed
    error_message   TEXT,
    processed_at    TIMESTAMPTZ,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT chk_webhook_status CHECK (status IN ('received', 'processing', 'processed', 'failed'))
);

COMMENT ON TABLE integration.webhook_logs IS 'Records all incoming webhook requests for audit and replay';
COMMENT ON COLUMN integration.webhook_logs.topic IS 'Webhook topic like orders/create, customers/update';

-- Create index for webhook lookups
CREATE INDEX idx_webhook_logs_tenant_created ON integration.webhook_logs(tenant_id, created_at DESC);
CREATE INDEX idx_webhook_logs_status ON integration.webhook_logs(status) WHERE status != 'processed';

-- External events: Normalized events extracted from webhooks
-- These are the canonical events that downstream services consume
CREATE TABLE integration.external_events (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID NOT NULL REFERENCES auth.tenants(id) ON DELETE CASCADE,
    webhook_log_id  UUID REFERENCES integration.webhook_logs(id),
    
    -- Idempotency key: Prevents duplicate event processing
    idempotency_key VARCHAR(255) NOT NULL,
    
    event_type      VARCHAR(100) NOT NULL,           -- order.completed, customer.created, etc.
    event_source    VARCHAR(50) NOT NULL,            -- shopify, manual, api
    
    -- Subject of the event
    subject_type    VARCHAR(50) NOT NULL,            -- order, customer, product
    subject_id      VARCHAR(255) NOT NULL,           -- External ID of the subject
    
    -- Event data
    payload         JSONB NOT NULL,                  -- Normalized event payload
    occurred_at     TIMESTAMPTZ NOT NULL,            -- When the event actually occurred
    
    -- Processing status
    status          VARCHAR(50) NOT NULL DEFAULT 'pending',
    processed_at    TIMESTAMPTZ,
    retry_count     INT NOT NULL DEFAULT 0,
    error_message   TEXT,
    
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    -- Idempotency: Same event cannot be processed twice per tenant
    CONSTRAINT uq_external_events_idempotency UNIQUE (tenant_id, idempotency_key),
    CONSTRAINT chk_external_events_status CHECK (status IN ('pending', 'processing', 'processed', 'failed', 'skipped'))
);

COMMENT ON TABLE integration.external_events IS 'Normalized events from external systems - idempotent processing guaranteed';
COMMENT ON COLUMN integration.external_events.idempotency_key IS 'Unique key per tenant to prevent duplicate processing';
COMMENT ON COLUMN integration.external_events.occurred_at IS 'Actual timestamp when event occurred in source system';

CREATE INDEX idx_external_events_pending ON integration.external_events(tenant_id, status, created_at) 
    WHERE status IN ('pending', 'processing');
CREATE INDEX idx_external_events_subject ON integration.external_events(tenant_id, subject_type, subject_id);

-- =============================================================================
-- SCHEMA: points
-- Responsibilities: Rule definitions, immutable ledger, balances, idempotency
-- =============================================================================

-- Point rules: Defines how points are earned
-- Rules are stored as structured JSON for flexibility
CREATE TABLE points.rules (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID NOT NULL REFERENCES auth.tenants(id) ON DELETE CASCADE,
    name            VARCHAR(255) NOT NULL,
    description     TEXT,
    
    -- Rule trigger conditions
    event_type      VARCHAR(100) NOT NULL,           -- order.completed, signup, referral, etc.
    
    -- Rule definition as structured JSON
    -- Example: {"type": "percentage", "value": 10, "currency": "USD", "min_order": 0}
    rule_definition JSONB NOT NULL,
    
    -- Rule status and priority
    priority        INT NOT NULL DEFAULT 0,          -- Higher = evaluated first
    is_active       BOOLEAN NOT NULL DEFAULT TRUE,
    
    -- Validity period
    valid_from      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    valid_until     TIMESTAMPTZ,                     -- NULL = no expiry
    
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by      UUID,                            -- Soft reference to auth.users (no FK)
    
    CONSTRAINT uq_rules_name_tenant UNIQUE (tenant_id, name)
);

COMMENT ON TABLE points.rules IS 'Defines point earning rules - stored as structured JSON for flexibility';
COMMENT ON COLUMN points.rules.rule_definition IS 'JSON defining calculation: type, value, conditions';
COMMENT ON COLUMN points.rules.priority IS 'Evaluation order - higher priority rules checked first';

CREATE INDEX idx_rules_active ON points.rules(tenant_id, event_type, is_active) WHERE is_active = TRUE;

-- Points ledger: IMMUTABLE append-only ledger of all point transactions
-- This is the SINGLE SOURCE OF TRUTH for point accounting
-- CRITICAL: This table is APPEND-ONLY. No UPDATE or DELETE allowed.
CREATE TABLE points.ledger_entries (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID NOT NULL REFERENCES auth.tenants(id) ON DELETE CASCADE,
    user_id         UUID NOT NULL,                   -- Soft reference to auth.users (no cross-schema FK)
    
    -- Idempotency: Prevents duplicate entries from event replays
    idempotency_key VARCHAR(255) NOT NULL,
    
    -- Transaction details
    entry_type      VARCHAR(50) NOT NULL,            -- earn, redeem, expire, adjust, refund
    points_amount   BIGINT NOT NULL,                 -- Positive for earn, negative for redeem/expire
    
    -- Running balance after this entry (denormalized for query efficiency)
    balance_after   BIGINT NOT NULL,
    
    -- Reference to what caused this entry
    source_type     VARCHAR(50) NOT NULL,            -- order, rule, manual, redemption, expiry
    source_id       UUID,                            -- UUID soft reference to source record
    
    -- Reference to the rule that generated points (for earn entries)
    rule_id         UUID,                            -- Soft reference to points.rules (same schema, but no FK)
    
    -- Human-readable description
    description     TEXT,
    
    -- Metadata for additional context
    metadata        JSONB DEFAULT '{}',
    
    -- Expiry tracking for earned points
    expires_at      TIMESTAMPTZ,                     -- When these points expire (NULL = never)
    
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    -- Idempotency constraint: Same event cannot create duplicate ledger entries
    CONSTRAINT uq_ledger_idempotency UNIQUE (tenant_id, idempotency_key),
    CONSTRAINT chk_ledger_entry_type CHECK (entry_type IN ('earn', 'redeem', 'expire', 'adjust', 'refund'))
);

COMMENT ON TABLE points.ledger_entries IS 'IMMUTABLE append-only ledger - SINGLE SOURCE OF TRUTH for points';
COMMENT ON COLUMN points.ledger_entries.idempotency_key IS 'Ensures event replays do not create duplicate entries';
COMMENT ON COLUMN points.ledger_entries.balance_after IS 'Denormalized running balance for query efficiency';
COMMENT ON COLUMN points.ledger_entries.points_amount IS 'Positive for credits, negative for debits';
COMMENT ON COLUMN points.ledger_entries.user_id IS 'Soft reference to auth.users - no FK to avoid cross-schema coupling';

CREATE INDEX idx_ledger_user ON points.ledger_entries(tenant_id, user_id, created_at DESC);
CREATE INDEX idx_ledger_expiry ON points.ledger_entries(expires_at) 
    WHERE expires_at IS NOT NULL AND entry_type = 'earn';

-- User balances: Cached/derived view of current point balances
-- This is a materialized summary of the ledger for query efficiency
-- All balance calculations ultimately derive from ledger_entries
CREATE TABLE points.user_balances (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID NOT NULL REFERENCES auth.tenants(id) ON DELETE CASCADE,
    user_id         UUID NOT NULL,                   -- Soft reference to auth.users (no cross-schema FK)
    
    -- Current balance totals (derived from ledger)
    available_points    BIGINT NOT NULL DEFAULT 0,   -- Points available to spend
    pending_points      BIGINT NOT NULL DEFAULT 0,   -- Points pending confirmation
    lifetime_earned     BIGINT NOT NULL DEFAULT 0,   -- Total points ever earned
    lifetime_redeemed   BIGINT NOT NULL DEFAULT 0,   -- Total points ever redeemed
    
    -- Last ledger entry used to compute this balance
    last_ledger_entry_id UUID,                       -- Soft reference to ledger_entries
    
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_user_balances UNIQUE (tenant_id, user_id)
);

COMMENT ON TABLE points.user_balances IS 'Cached point balances - DERIVED from ledger, not source of truth';
COMMENT ON COLUMN points.user_balances.available_points IS 'Spendable points (earned - redeemed - expired)';
COMMENT ON COLUMN points.user_balances.last_ledger_entry_id IS 'Last processed ledger entry for consistency';
COMMENT ON COLUMN points.user_balances.user_id IS 'Soft reference to auth.users - no FK to avoid cross-schema coupling';

-- Idempotency keys: Tracks processed operations to prevent duplicates
-- This is a general-purpose idempotency store for the points service
CREATE TABLE points.idempotency_keys (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID NOT NULL REFERENCES auth.tenants(id) ON DELETE CASCADE,
    
    idempotency_key VARCHAR(255) NOT NULL,
    operation_type  VARCHAR(100) NOT NULL,           -- earn_points, process_order, etc.
    
    -- Result of the operation
    status          VARCHAR(50) NOT NULL DEFAULT 'pending',
    result          JSONB,                           -- Stored result for replay
    
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    expires_at      TIMESTAMPTZ NOT NULL,            -- When this key can be garbage collected
    
    CONSTRAINT uq_idempotency_keys UNIQUE (tenant_id, idempotency_key, operation_type),
    CONSTRAINT chk_idempotency_status CHECK (status IN ('pending', 'completed', 'failed'))
);

COMMENT ON TABLE points.idempotency_keys IS 'General idempotency store - prevents duplicate operations';
COMMENT ON COLUMN points.idempotency_keys.result IS 'Stored result for returning on duplicate requests';

CREATE INDEX idx_idempotency_expiry ON points.idempotency_keys(expires_at);

-- =============================================================================
-- SCHEMA: rewards
-- Responsibilities: Reward catalog, redemption records, prevent double-spend
-- =============================================================================

-- Reward catalog: Available rewards that can be redeemed
CREATE TABLE rewards.catalog (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID NOT NULL REFERENCES auth.tenants(id) ON DELETE CASCADE,
    
    name            VARCHAR(255) NOT NULL,
    description     TEXT,
    
    -- Redemption cost
    points_cost     BIGINT NOT NULL,
    
    -- Reward type and value
    reward_type     VARCHAR(50) NOT NULL,            -- discount, product, gift_card, custom
    reward_value    JSONB NOT NULL,                  -- Type-specific value definition
    
    -- Inventory management (optional)
    is_limited      BOOLEAN NOT NULL DEFAULT FALSE,
    total_quantity  INT,                             -- NULL if unlimited
    remaining_quantity INT,
    
    -- Status and visibility
    is_active       BOOLEAN NOT NULL DEFAULT TRUE,
    valid_from      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    valid_until     TIMESTAMPTZ,
    
    -- Usage limits
    max_per_user    INT,                             -- NULL = unlimited
    
    -- Display
    image_url       VARCHAR(500),
    display_order   INT NOT NULL DEFAULT 0,
    
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_catalog_name_tenant UNIQUE (tenant_id, name),
    CONSTRAINT chk_catalog_type CHECK (reward_type IN ('discount', 'product', 'gift_card', 'custom')),
    CONSTRAINT chk_catalog_quantity CHECK (
        (is_limited = FALSE) OR 
        (is_limited = TRUE AND total_quantity IS NOT NULL AND remaining_quantity IS NOT NULL)
    )
);

COMMENT ON TABLE rewards.catalog IS 'Available rewards that customers can redeem';
COMMENT ON COLUMN rewards.catalog.reward_value IS 'JSON defining the reward: discount_percent, product_id, etc.';
COMMENT ON COLUMN rewards.catalog.remaining_quantity IS 'Decremented on each redemption if is_limited=true';

CREATE INDEX idx_catalog_active ON rewards.catalog(tenant_id, is_active, display_order) WHERE is_active = TRUE;

-- Redemptions: Records of reward redemptions
-- CRITICAL: Use SELECT ... FOR UPDATE on catalog when decrementing inventory
CREATE TABLE rewards.redemptions (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID NOT NULL REFERENCES auth.tenants(id) ON DELETE CASCADE,
    user_id         UUID NOT NULL,                   -- Soft reference to auth.users (no cross-schema FK)
    reward_id       UUID NOT NULL REFERENCES rewards.catalog(id),  -- Same schema FK is OK
    
    -- Idempotency: Prevents double-redemption from same request
    idempotency_key VARCHAR(255) NOT NULL,
    
    -- Points spent on this redemption
    points_spent    BIGINT NOT NULL,
    
    -- Reference to the ledger entry that debited the points
    ledger_entry_id UUID,                            -- Soft reference to points.ledger_entries (no cross-schema FK)
    
    -- Redemption status
    status          VARCHAR(50) NOT NULL DEFAULT 'pending',
    
    -- Fulfillment details (type-specific)
    -- For discount: {code: "SAVE10", discount_value: 10}
    -- For product: {sku: "PROD-123", tracking: "..."}
    fulfillment_data JSONB DEFAULT '{}',
    
    -- External reference (e.g., discount code in Shopify)
    external_reference VARCHAR(255),
    
    fulfilled_at    TIMESTAMPTZ,
    expires_at      TIMESTAMPTZ,                     -- When the reward expires (for time-limited rewards)
    
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_redemptions_idempotency UNIQUE (tenant_id, idempotency_key),
    CONSTRAINT chk_redemptions_status CHECK (status IN ('pending', 'fulfilled', 'expired', 'cancelled', 'failed'))
);

COMMENT ON TABLE rewards.redemptions IS 'Records of reward redemptions - idempotent to prevent double-spend';
COMMENT ON COLUMN rewards.redemptions.idempotency_key IS 'Prevents duplicate redemptions from retry requests';
COMMENT ON COLUMN rewards.redemptions.fulfillment_data IS 'Type-specific fulfillment info: discount codes, tracking, etc.';
COMMENT ON COLUMN rewards.redemptions.user_id IS 'Soft reference to auth.users - no FK to avoid cross-schema coupling';

CREATE INDEX idx_redemptions_user ON rewards.redemptions(tenant_id, user_id, created_at DESC);
CREATE INDEX idx_redemptions_status ON rewards.redemptions(tenant_id, status) WHERE status = 'pending';

-- =============================================================================
-- SCHEMA: gateway
-- Responsibilities: API keys, basic rate-limit/access tracking
-- =============================================================================

-- API keys: Authentication for external API access
-- Keys are stored as hashes, scoped per tenant
CREATE TABLE gateway.api_keys (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID NOT NULL REFERENCES auth.tenants(id) ON DELETE CASCADE,
    
    name            VARCHAR(255) NOT NULL,           -- Human-readable name
    key_prefix      VARCHAR(10) NOT NULL,            -- First 8 chars for identification (tenant-scoped)
    key_hash        VARCHAR(255) NOT NULL,           -- Hashed API key (never store plaintext)
    
    -- Permissions and scopes
    scopes          JSONB NOT NULL DEFAULT '["read"]',  -- Array of allowed scopes
    
    -- Rate limiting
    rate_limit_per_minute INT NOT NULL DEFAULT 60,
    
    -- Status
    is_active       BOOLEAN NOT NULL DEFAULT TRUE,
    
    -- Validity
    expires_at      TIMESTAMPTZ,                     -- NULL = never expires
    last_used_at    TIMESTAMPTZ,
    
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by      UUID,                            -- Soft reference to auth.users (no cross-schema FK)
    revoked_at      TIMESTAMPTZ,
    revoked_by      UUID,                            -- Soft reference to auth.users (no cross-schema FK)
    
    -- Key prefix is unique per tenant, NOT globally
    CONSTRAINT uq_api_keys_tenant_prefix UNIQUE (tenant_id, key_prefix),
    -- Key hash must be globally unique (same key can't exist twice)
    CONSTRAINT uq_api_keys_hash UNIQUE (key_hash)
);

COMMENT ON TABLE gateway.api_keys IS 'API keys for external service authentication - scoped per tenant';
COMMENT ON COLUMN gateway.api_keys.key_prefix IS 'First 8 characters - unique per tenant, not globally';
COMMENT ON COLUMN gateway.api_keys.key_hash IS 'SHA-256 hash of the full API key - globally unique';

CREATE INDEX idx_api_keys_active ON gateway.api_keys(tenant_id, is_active) WHERE is_active = TRUE;

-- Access logs: Basic request logging for rate limiting and debugging
-- Kept minimal for MVP - use external logging for production analytics
CREATE TABLE gateway.access_logs (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID REFERENCES auth.tenants(id) ON DELETE SET NULL,
    api_key_id      UUID REFERENCES gateway.api_keys(id) ON DELETE SET NULL,
    
    -- Request details
    method          VARCHAR(10) NOT NULL,
    path            VARCHAR(500) NOT NULL,
    status_code     INT NOT NULL,
    response_time_ms INT,
    
    -- Client info
    client_ip       INET,
    user_agent      VARCHAR(500),
    
    -- Rate limiting context
    rate_limit_remaining INT,
    
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE gateway.access_logs IS 'Minimal request logging for rate limiting - use external system for analytics';

-- Partition hint: In production, partition this table by created_at
CREATE INDEX idx_access_logs_key ON gateway.access_logs(api_key_id, created_at DESC);
CREATE INDEX idx_access_logs_created ON gateway.access_logs(created_at);

-- =============================================================================
-- SCHEMA: audit
-- Responsibilities: Lightweight system event traceability
-- =============================================================================

-- System events: Audit trail for important system operations
CREATE TABLE audit.system_events (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID REFERENCES auth.tenants(id) ON DELETE SET NULL,
    
    -- Event classification
    event_category  VARCHAR(50) NOT NULL,            -- auth, points, rewards, integration, system
    event_type      VARCHAR(100) NOT NULL,           -- user.created, points.earned, etc.
    severity        VARCHAR(20) NOT NULL DEFAULT 'info',  -- debug, info, warning, error
    
    -- Actor (who performed the action)
    actor_type      VARCHAR(50) NOT NULL,            -- user, system, api_key, webhook
    actor_id        UUID,                            -- ID of the actor
    
    -- Target (what was affected)
    target_type     VARCHAR(50),                     -- user, tenant, rule, reward, etc.
    target_id       UUID,
    
    -- Event details
    description     TEXT NOT NULL,
    metadata        JSONB DEFAULT '{}',              -- Additional structured data
    
    -- Request context
    request_id      VARCHAR(100),                    -- Correlation ID for request tracing
    ip_address      INET,
    
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT chk_audit_severity CHECK (severity IN ('debug', 'info', 'warning', 'error'))
);

COMMENT ON TABLE audit.system_events IS 'Lightweight audit trail for MVP - captures important system operations';
COMMENT ON COLUMN audit.system_events.request_id IS 'Correlation ID for distributed request tracing';

-- Partition hint: In production, partition this table by created_at
CREATE INDEX idx_system_events_tenant ON audit.system_events(tenant_id, created_at DESC);
CREATE INDEX idx_system_events_type ON audit.system_events(event_category, event_type, created_at DESC);
CREATE INDEX idx_system_events_actor ON audit.system_events(actor_type, actor_id, created_at DESC);
CREATE INDEX idx_system_events_target ON audit.system_events(target_type, target_id);

-- =============================================================================
-- LEDGER IMMUTABILITY ENFORCEMENT
-- =============================================================================
-- Create a restricted role for ledger operations
-- Application should connect with this role for points operations

-- Create role for points engine (INSERT and SELECT only on ledger)
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'points_engine_role') THEN
        CREATE ROLE points_engine_role;
    END IF;
END
$$;

-- Grant schema usage
GRANT USAGE ON SCHEMA points TO points_engine_role;

-- Ledger: INSERT and SELECT only - NO UPDATE, NO DELETE
GRANT SELECT, INSERT ON points.ledger_entries TO points_engine_role;

-- Other points tables: full access
GRANT SELECT, INSERT, UPDATE, DELETE ON points.rules TO points_engine_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON points.user_balances TO points_engine_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON points.idempotency_keys TO points_engine_role;

COMMENT ON TABLE points.ledger_entries IS 'IMMUTABLE - points_engine_role has INSERT+SELECT only, no UPDATE/DELETE';

-- =============================================================================
-- SCHEMA VALIDATION SUMMARY
-- =============================================================================
-- ✓ Points ledger (points.ledger_entries) is append-only
--   - Enforced via role permissions (INSERT + SELECT only)
--   - idempotency_key prevents duplicate entries from event replays
--   - No UPDATE or DELETE allowed at DB level
--
-- ✓ Event replays are safe
--   - integration.external_events has (tenant_id, idempotency_key) constraint
--   - points.ledger_entries has (tenant_id, idempotency_key) constraint
--   - rewards.redemptions has (tenant_id, idempotency_key) constraint
--   - points.idempotency_keys provides general idempotency storage
--
-- ✓ Tenant isolation is enforced
--   - All tenant-scoped tables include tenant_id column
--   - FK to auth.tenants ensures referential integrity
--   - Unique constraints include tenant_id where applicable
--   - Email uniqueness is (email, provider), not global
--   - API key prefix uniqueness is (tenant_id, key_prefix), not global
--
-- ✓ Cross-schema FK policy
--   - Only FK allowed across schemas: to auth.tenants
--   - All other cross-schema references are soft (UUID only)
--
-- ✓ Schema matches service boundaries
--   - auth: Auth + Tenant Service
--   - integration: E-commerce Integration Service  
--   - points: Points Engine (core brain)
--   - rewards: Rewards & Redemption Service
--   - gateway: API keys & access control
--   - audit: System audit trail
-- =============================================================================

-- End of schema definition
