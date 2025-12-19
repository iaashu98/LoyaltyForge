using EcommerceIntegration.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceIntegration.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for E-commerce Integration service.
/// Maps to PostgreSQL schema: integration
/// </summary>
public class IntegrationDbContext : DbContext
{
    public IntegrationDbContext(DbContextOptions<IntegrationDbContext> options) : base(options)
    {
    }

    public DbSet<WebhookLog> WebhookLogs => Set<WebhookLog>();
    public DbSet<ExternalEvent> ExternalEvents => Set<ExternalEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set schema for all entities
        modelBuilder.HasDefaultSchema("integration");

        // WebhookLog configuration
        modelBuilder.Entity<WebhookLog>(entity =>
        {
            entity.ToTable("webhook_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.Source).HasColumnName("source").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Topic).HasColumnName("topic").HasMaxLength(100).IsRequired();
            entity.Property(e => e.WebhookId).HasColumnName("webhook_id").HasMaxLength(255);
            entity.Property(e => e.Headers).HasColumnName("headers").HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.Signature).HasColumnName("signature").HasMaxLength(255);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => new { e.TenantId, e.CreatedAt });
            entity.HasIndex(e => e.Status);
        });

        // ExternalEvent configuration
        modelBuilder.Entity<ExternalEvent>(entity =>
        {
            entity.ToTable("external_events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.WebhookLogId).HasColumnName("webhook_log_id");
            entity.Property(e => e.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(255).IsRequired();
            entity.Property(e => e.EventType).HasColumnName("event_type").HasMaxLength(100).IsRequired();
            entity.Property(e => e.EventSource).HasColumnName("event_source").HasMaxLength(50).IsRequired();
            entity.Property(e => e.SubjectType).HasColumnName("subject_type").HasMaxLength(50).IsRequired();
            entity.Property(e => e.SubjectId).HasColumnName("subject_id").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.OccurredAt).HasColumnName("occurred_at");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
            entity.Property(e => e.RetryCount).HasColumnName("retry_count");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            // Idempotency constraint
            entity.HasIndex(e => new { e.TenantId, e.IdempotencyKey }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.SubjectType, e.SubjectId });

            entity.HasOne(e => e.WebhookLog)
                .WithMany(w => w.ExternalEvents)
                .HasForeignKey(e => e.WebhookLogId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
