using AuthTenant.Domain.Entities;
using AuthTenant.Infrastructure.Persistence;
using AuthTenant.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AuthTenant.Infrastructure.Tests.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly AuthTenantDbContext _context;
    private readonly UserRepository _repository;
    private readonly Guid _tenantId;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AuthTenantDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AuthTenantDbContext(options);
        _repository = new UserRepository(_context);

        // Seed a tenant for testing
        var tenant = Tenant.Create("Test Tenant", "test-tenant");
        _tenantId = tenant.Id;
        _context.Tenants.Add(tenant);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsUser()
    {
        // Arrange
        var user = User.CreateLocal("test@example.com", "hashed_password");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be("test@example.com");
        result.Provider.Should().Be(AuthProvider.Local);
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
    public async Task GetByEmailAndProviderAsync_WithExistingUser_ReturnsUser()
    {
        // Arrange
        var user = User.CreateExternal("john@example.com", "google-123", AuthProvider.Google);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAndProviderAsync("john@example.com", AuthProvider.Google);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("john@example.com");
        result.Provider.Should().Be(AuthProvider.Google);
        result.EmailVerified.Should().BeTrue(); // External providers verify email
    }

    [Fact]
    public async Task GetByEmailAndProviderAsync_WithDifferentProvider_ReturnsNull()
    {
        // Arrange
        var user = User.CreateExternal("john@example.com", "google-123", AuthProvider.Google);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act - Same email but different provider
        var result = await _repository.GetByEmailAndProviderAsync("john@example.com", AuthProvider.Local);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAndProviderAsync_WithNonExistingEmail_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByEmailAndProviderAsync("nonexisting@example.com", AuthProvider.Local);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_CreatesLocalUser()
    {
        // Arrange
        var user = User.CreateLocal("newuser@example.com", "hashed_password_123");

        // Act
        await _repository.AddAsync(user);

        // Assert
        var savedUser = await _context.Users.FindAsync(user.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Email.Should().Be("newuser@example.com");
        savedUser.Provider.Should().Be(AuthProvider.Local);
        savedUser.PasswordHash.Should().Be("hashed_password_123");
        savedUser.EmailVerified.Should().BeFalse(); // Local users start unverified
        savedUser.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public async Task AddAsync_CreatesExternalUser()
    {
        // Arrange
        var user = User.CreateExternal("oauth@example.com", "shopify-456", AuthProvider.Shopify);

        // Act
        await _repository.AddAsync(user);

        // Assert
        var savedUser = await _context.Users.FindAsync(user.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Email.Should().Be("oauth@example.com");
        savedUser.Provider.Should().Be(AuthProvider.Shopify);
        savedUser.ExternalId.Should().Be("shopify-456");
        savedUser.EmailVerified.Should().BeTrue(); // External users are pre-verified
        savedUser.PasswordHash.Should().BeNull(); // No password for external users
    }

    [Fact]
    public async Task UpdateAsync_UpdatesUser()
    {
        // Arrange
        var user = User.CreateLocal("original@example.com", "password");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Detach to simulate a new context
        _context.Entry(user).State = EntityState.Detached;

        // Get fresh instance and modify
        var userToUpdate = await _context.Users.FindAsync(user.Id);
        userToUpdate!.UpdatePassword("new_hashed_password");

        // Act
        await _repository.UpdateAsync(userToUpdate);

        // Assert
        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.PasswordHash.Should().Be("new_hashed_password");
    }

    [Fact]
    public async Task User_CanVerifyEmail()
    {
        // Arrange
        var user = User.CreateLocal("unverified@example.com", "password");
        await _repository.AddAsync(user);

        // Act
        user.VerifyEmail();
        await _repository.UpdateAsync(user);

        // Assert
        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.EmailVerified.Should().BeTrue();
    }

    [Fact]
    public async Task User_CanBeSuspended()
    {
        // Arrange
        var user = User.CreateLocal("active@example.com", "password");
        await _repository.AddAsync(user);

        // Act
        user.Suspend();
        await _repository.UpdateAsync(user);

        // Assert
        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.Status.Should().Be(UserStatus.Suspended);
    }

    [Fact]
    public async Task User_CanBeActivated()
    {
        // Arrange
        var user = User.CreateLocal("suspended@example.com", "password");
        user.Suspend();
        await _repository.AddAsync(user);

        // Act
        user.Activate();
        await _repository.UpdateAsync(user);

        // Assert
        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public async Task AddAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var user = User.CreateLocal("test@example.com", "password");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await _repository.AddAsync(user, cts.Token));
    }

    [Fact]
    public async Task CreateLocal_SetsCorrectDefaults()
    {
        // Arrange & Act
        var user = User.CreateLocal("defaults@example.com", "password");

        // Assert
        user.Provider.Should().Be(AuthProvider.Local);
        user.EmailVerified.Should().BeFalse();
        user.Status.Should().Be(UserStatus.Active);
        user.ExternalId.Should().BeNull();
        user.PasswordHash.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateExternal_SetsCorrectDefaults()
    {
        // Arrange & Act
        var user = User.CreateExternal("external@example.com", "ext-123", AuthProvider.Google);

        // Assert
        user.Provider.Should().Be(AuthProvider.Google);
        user.EmailVerified.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Active);
        user.ExternalId.Should().Be("ext-123");
        user.PasswordHash.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
