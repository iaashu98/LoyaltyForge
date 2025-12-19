using ApiGateway.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiGateway.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for API Gateway service.
/// Maps to PostgreSQL schema: gateway
/// </summary>
public class GatewayDbContext : DbContext
{
    public GatewayDbContext(DbContextOptions<GatewayDbContext> options) : base(options)
    {
    }

    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<AccessLog> AccessLogs => Set<AccessLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set schema for all entities
        modelBuilder.HasDefaultSchema("gateway");

        // ApiKey configuration
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.ToTable("api_keys");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.KeyPrefix).HasColumnName("key_prefix").HasMaxLength(10).IsRequired();
            entity.Property(e => e.KeyHash).HasColumnName("key_hash").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Scopes).HasColumnName("scopes").HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.RateLimitPerMinute).HasColumnName("rate_limit_per_minute");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.LastUsedAt).HasColumnName("last_used_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");
            entity.Property(e => e.RevokedBy).HasColumnName("revoked_by");

            // Key prefix unique per tenant (not globally)
            entity.HasIndex(e => new { e.TenantId, e.KeyPrefix }).IsUnique();
            // Key hash globally unique
            entity.HasIndex(e => e.KeyHash).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.IsActive });
        });

        // AccessLog configuration
        modelBuilder.Entity<AccessLog>(entity =>
        {
            entity.ToTable("access_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.ApiKeyId).HasColumnName("api_key_id");
            entity.Property(e => e.Method).HasColumnName("method").HasMaxLength(10).IsRequired();
            entity.Property(e => e.Path).HasColumnName("path").HasMaxLength(500).IsRequired();
            entity.Property(e => e.StatusCode).HasColumnName("status_code");
            entity.Property(e => e.ResponseTimeMs).HasColumnName("response_time_ms");
            entity.Property(e => e.ClientIp).HasColumnName("client_ip");
            entity.Property(e => e.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
            entity.Property(e => e.RateLimitRemaining).HasColumnName("rate_limit_remaining");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => new { e.ApiKeyId, e.CreatedAt });
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
