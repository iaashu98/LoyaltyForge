using Microsoft.EntityFrameworkCore;
using PointsEngine.Domain.Entities;

namespace PointsEngine.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for Points Engine service.
/// </summary>
public class PointsEngineDbContext : DbContext
{
    public PointsEngineDbContext(DbContextOptions<PointsEngineDbContext> options) : base(options)
    {
    }

    public DbSet<PointsLedgerEntry> LedgerEntries => Set<PointsLedgerEntry>();
    public DbSet<PointsBalance> Balances => Set<PointsBalance>();
    public DbSet<PointsRule> Rules => Set<PointsRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PointsLedgerEntry configuration (immutable)
        modelBuilder.Entity<PointsLedgerEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.CustomerId, e.CreatedAt });
            entity.HasIndex(e => e.TransactionId);
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ReferenceId).HasMaxLength(100);
            entity.Property(e => e.ReferenceType).HasMaxLength(50);
        });

        // PointsBalance configuration
        modelBuilder.Entity<PointsBalance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.CustomerId }).IsUnique();
        });

        // PointsRule configuration
        modelBuilder.Entity<PointsRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.IsActive });
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.RuleType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Configuration).IsRequired();
        });
    }
}
