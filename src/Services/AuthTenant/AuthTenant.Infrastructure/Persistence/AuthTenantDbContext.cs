using AuthTenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthTenant.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for Auth + Tenant service.
/// Maps to PostgreSQL schema: auth
/// </summary>
public class AuthTenantDbContext : DbContext
{
    public AuthTenantDbContext(DbContextOptions<AuthTenantDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserTenant> UserTenants => Set<UserTenant>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set schema for all entities
        modelBuilder.HasDefaultSchema("auth");

        // Tenant configuration
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Settings).HasColumnName("settings").HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.Slug).IsUnique();
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255);
            entity.Property(e => e.ExternalId).HasColumnName("external_id").HasMaxLength(255);
            entity.Property(e => e.Provider).HasColumnName("provider").HasMaxLength(50).IsRequired();
            entity.Property(e => e.EmailVerified).HasColumnName("email_verified");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            // Email unique per provider (not globally)
            entity.HasIndex(e => new { e.Email, e.Provider }).IsUnique();
        });

        // UserTenant configuration (user-tenant mapping)
        modelBuilder.Entity<UserTenant>(entity =>
        {
            entity.ToTable("user_tenants");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.UserType).HasColumnName("user_type").HasMaxLength(50).IsRequired();
            entity.Property(e => e.ExternalCustomerId).HasColumnName("external_customer_id").HasMaxLength(255);
            entity.Property(e => e.Metadata).HasColumnName("metadata").HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => new { e.UserId, e.TenantId }).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserTenants)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.UserTenants)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Permissions).HasColumnName("permissions").HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.IsSystemRole).HasColumnName("is_system_role");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Roles)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserRole configuration
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserTenantId).HasColumnName("user_tenant_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.GrantedAt).HasColumnName("granted_at");
            entity.Property(e => e.GrantedBy).HasColumnName("granted_by");

            entity.HasIndex(e => new { e.UserTenantId, e.RoleId }).IsUnique();

            entity.HasOne(e => e.UserTenant)
                .WithMany(ut => ut.UserRoles)
                .HasForeignKey(e => e.UserTenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
