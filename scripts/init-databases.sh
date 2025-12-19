#!/bin/sh
# LoyaltyForge Database Initialization Script
# This script runs before schema.sql to enable required extensions

set -e
set -u

echo "Initializing LoyaltyForge database..."

# Enable extensions (must be done by superuser before schema.sql runs)
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    -- Extensions are created in schema.sql, this script is for any pre-setup
    -- Currently just a placeholder for future initialization needs
    SELECT 'LoyaltyForge database initialized' AS status;
EOSQL

echo "LoyaltyForge database initialization complete"
