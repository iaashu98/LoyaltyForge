using Microsoft.EntityFrameworkCore;
using Rewards.Domain.Entities;

namespace Rewards.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for Rewards service.
/// Maps to PostgreSQL schema: rewards
/// </summary>
public class RewardsDbContext : DbContext
{
    public RewardsDbContext(DbContextOptions<RewardsDbContext> options) : base(options)
    {
    }

    public DbSet<CatalogItem> Catalog => Set<CatalogItem>();
    public DbSet<Redemption> Redemptions => Set<Redemption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set schema for all entities
        modelBuilder.HasDefaultSchema("rewards");

        // CatalogItem configuration
        modelBuilder.Entity<CatalogItem>(entity =>
        {
            entity.ToTable("catalog");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.PointsCost).HasColumnName("points_cost");
            entity.Property(e => e.RewardType).HasColumnName("reward_type").HasMaxLength(50).IsRequired();
            entity.Property(e => e.RewardValue).HasColumnName("reward_value").HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.IsLimited).HasColumnName("is_limited");
            entity.Property(e => e.TotalQuantity).HasColumnName("total_quantity");
            entity.Property(e => e.RemainingQuantity).HasColumnName("remaining_quantity");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ValidFrom).HasColumnName("valid_from");
            entity.Property(e => e.ValidUntil).HasColumnName("valid_until");
            entity.Property(e => e.MaxPerUser).HasColumnName("max_per_user");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url").HasMaxLength(500);
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.IsActive, e.DisplayOrder });
        });

        // Redemption configuration
        modelBuilder.Entity<Redemption>(entity =>
        {
            entity.ToTable("redemptions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RewardId).HasColumnName("reward_id");
            entity.Property(e => e.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(255).IsRequired();
            entity.Property(e => e.PointsSpent).HasColumnName("points_spent");
            entity.Property(e => e.LedgerEntryId).HasColumnName("ledger_entry_id");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
            entity.Property(e => e.FulfillmentData).HasColumnName("fulfillment_data").HasColumnType("jsonb");
            entity.Property(e => e.ExternalReference).HasColumnName("external_reference").HasMaxLength(255);
            entity.Property(e => e.FulfilledAt).HasColumnName("fulfilled_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            // Idempotency constraint
            entity.HasIndex(e => new { e.TenantId, e.IdempotencyKey }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.UserId, e.CreatedAt });

            // Same-schema FK is allowed
            entity.HasOne(e => e.Reward)
                .WithMany()
                .HasForeignKey(e => e.RewardId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
