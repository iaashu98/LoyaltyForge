using AuthTenant.Domain.Entities;
using AuthTenant.Infrastructure.Persistence;
using AuthTenant.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AuthTenant.Infrastructure.Tests.Repositories;

public class TenantRepositoryTests : IDisposable
{
    private readonly AuthTenantDbContext _context;
    private readonly TenantRepository _repository;

    public TenantRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AuthTenantDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AuthTenantDbContext(options);
        _repository = new TenantRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsTenant()
    {
        // Arrange
        var tenant = Tenant.Create("Test Tenant", "test-tenant");
        await _context.Tenants.AddAsync(tenant);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(tenant.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(tenant.Id);
        result.Name.Should().Be("Test Tenant");
        result.Slug.Should().Be("test-tenant");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_WithExistingSlug_ReturnsTenant()
    {
        // Arrange
        var tenant = Tenant.Create("Acme Corp", "acme-corp");
        await _context.Tenants.AddAsync(tenant);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySlugAsync("acme-corp");

        // Assert
        result.Should().NotBeNull();
        result!.Slug.Should().Be("acme-corp");
        result.Name.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task GetBySlugAsync_WithNonExistingSlug_ReturnsNull()
    {
        // Act
        var result = await _repository.GetBySlugAsync("non-existing-slug");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTenants()
    {
        // Arrange
        var tenants = new[]
        {
            Tenant.Create("Tenant 1", "tenant-1"),
            Tenant.Create("Tenant 2", "tenant-2"),
            Tenant.Create("Tenant 3", "tenant-3")
        };
        await _context.Tenants.AddRangeAsync(tenants);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(t => t.Name == "Tenant 1");
        result.Should().Contain(t => t.Name == "Tenant 2");
        result.Should().Contain(t => t.Name == "Tenant 3");
    }

    [Fact]
    public async Task GetAllAsync_WithNoTenants_ReturnsEmptyList()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_CreatesTenant()
    {
        // Arrange
        var tenant = Tenant.Create("New Tenant", "new-tenant", "contact@example.com");

        // Act
        await _repository.AddAsync(tenant);

        // Assert
        var savedTenant = await _context.Tenants.FindAsync(tenant.Id);
        savedTenant.Should().NotBeNull();
        savedTenant!.Name.Should().Be("New Tenant");
        savedTenant.Slug.Should().Be("new-tenant");
        savedTenant.Status.Should().Be(TenantStatus.Active);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesTenant()
    {
        // Arrange
        var tenant = Tenant.Create("Original Name", "original-slug");
        await _context.Tenants.AddAsync(tenant);
        await _context.SaveChangesAsync();

        // Detach to simulate a new context
        _context.Entry(tenant).State = EntityState.Detached;

        // Get fresh instance and modify
        var tenantToUpdate = await _context.Tenants.FindAsync(tenant.Id);
        tenantToUpdate!.UpdateSettings("{\"newSetting\":\"value\"}");

        // Act
        await _repository.UpdateAsync(tenantToUpdate);

        // Assert
        var updatedTenant = await _context.Tenants.FindAsync(tenant.Id);
        updatedTenant.Should().NotBeNull();
        updatedTenant!.Settings.Should().Contain("newSetting");
    }

    [Fact]
    public async Task AddAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var tenant = Tenant.Create("Test Tenant", "test-tenant");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await _repository.AddAsync(tenant, cts.Token));
    }

    [Fact]
    public async Task Create_NormalizesSlug()
    {
        // Arrange & Act
        var tenant = Tenant.Create("My Company", "My Company");
        await _repository.AddAsync(tenant);

        // Assert
        var savedTenant = await _context.Tenants.FindAsync(tenant.Id);
        savedTenant!.Slug.Should().Be("my-company");
    }

    [Fact]
    public async Task Tenant_CanBeSuspended()
    {
        // Arrange
        var tenant = Tenant.Create("Active Tenant", "active-tenant");
        await _repository.AddAsync(tenant);

        // Act
        tenant.Suspend();
        await _repository.UpdateAsync(tenant);

        // Assert
        var updatedTenant = await _context.Tenants.FindAsync(tenant.Id);
        updatedTenant!.Status.Should().Be(TenantStatus.Suspended);
    }

    [Fact]
    public async Task Tenant_CanBeActivated()
    {
        // Arrange
        var tenant = Tenant.Create("Tenant", "tenant");
        tenant.Suspend();
        await _repository.AddAsync(tenant);

        // Act
        tenant.Activate();
        await _repository.UpdateAsync(tenant);

        // Assert
        var updatedTenant = await _context.Tenants.FindAsync(tenant.Id);
        updatedTenant!.Status.Should().Be(TenantStatus.Active);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
