using Microsoft.EntityFrameworkCore;
using Rewards.Domain.Entities;

namespace Rewards.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for Rewards service.
/// </summary>
public class RewardsDbContext : DbContext
{
    public RewardsDbContext(DbContextOptions<RewardsDbContext> options) : base(options)
    {
    }

    public DbSet<Reward> Rewards => Set<Reward>();
    public DbSet<Redemption> Redemptions => Set<Redemption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Reward configuration
        modelBuilder.Entity<Reward>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.IsActive });
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.RewardType).HasMaxLength(50).IsRequired();
        });

        // Redemption configuration
        modelBuilder.Entity<Redemption>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.IdempotencyKey }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.CustomerId });
            entity.Property(e => e.IdempotencyKey).HasMaxLength(100).IsRequired();
            entity.HasOne(e => e.Reward).WithMany().HasForeignKey(e => e.RewardId);
        });
    }
}
