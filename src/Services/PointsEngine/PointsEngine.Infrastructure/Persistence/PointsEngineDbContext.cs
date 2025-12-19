using Microsoft.EntityFrameworkCore;
using PointsEngine.Domain.Entities;

namespace PointsEngine.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for Points Engine service.
/// Maps to PostgreSQL schema: points
/// </summary>
public class PointsEngineDbContext : DbContext
{
    public PointsEngineDbContext(DbContextOptions<PointsEngineDbContext> options) : base(options)
    {
    }

    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<UserBalance> UserBalances => Set<UserBalance>();
    public DbSet<Rule> Rules => Set<Rule>();
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set schema for all entities
        modelBuilder.HasDefaultSchema("points");

        // LedgerEntry configuration (IMMUTABLE)
        modelBuilder.Entity<LedgerEntry>(entity =>
        {
            entity.ToTable("ledger_entries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(255).IsRequired();
            entity.Property(e => e.EntryType).HasColumnName("entry_type").HasMaxLength(50).IsRequired();
            entity.Property(e => e.PointsAmount).HasColumnName("points_amount");
            entity.Property(e => e.BalanceAfter).HasColumnName("balance_after");
            entity.Property(e => e.SourceType).HasColumnName("source_type").HasMaxLength(50).IsRequired();
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.RuleId).HasColumnName("rule_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Metadata).HasColumnName("metadata").HasColumnType("jsonb");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            // Idempotency constraint
            entity.HasIndex(e => new { e.TenantId, e.IdempotencyKey }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.UserId, e.CreatedAt });
            entity.HasIndex(e => e.ExpiresAt);
        });

        // UserBalance configuration
        modelBuilder.Entity<UserBalance>(entity =>
        {
            entity.ToTable("user_balances");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AvailablePoints).HasColumnName("available_points");
            entity.Property(e => e.PendingPoints).HasColumnName("pending_points");
            entity.Property(e => e.LifetimeEarned).HasColumnName("lifetime_earned");
            entity.Property(e => e.LifetimeRedeemed).HasColumnName("lifetime_redeemed");
            entity.Property(e => e.LastLedgerEntryId).HasColumnName("last_ledger_entry_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => new { e.TenantId, e.UserId }).IsUnique();
        });

        // Rule configuration
        modelBuilder.Entity<Rule>(entity =>
        {
            entity.ToTable("rules");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EventType).HasColumnName("event_type").HasMaxLength(100).IsRequired();
            entity.Property(e => e.RuleDefinition).HasColumnName("rule_definition").HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.Priority).HasColumnName("priority");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ValidFrom).HasColumnName("valid_from");
            entity.Property(e => e.ValidUntil).HasColumnName("valid_until");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");

            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.EventType, e.IsActive });
        });

        // IdempotencyKey configuration
        modelBuilder.Entity<IdempotencyKey>(entity =>
        {
            entity.ToTable("idempotency_keys");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.Key).HasColumnName("idempotency_key").HasMaxLength(255).IsRequired();
            entity.Property(e => e.OperationType).HasColumnName("operation_type").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Result).HasColumnName("result").HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");

            entity.HasIndex(e => new { e.TenantId, e.Key, e.OperationType }).IsUnique();
            entity.HasIndex(e => e.ExpiresAt);
        });
    }
}
