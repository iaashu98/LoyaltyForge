using AuthTenant.Domain.Entities;

namespace AuthTenant.Application.Interfaces;

/// <summary>
/// Repository interface for User operations.
/// Note: Users are cross-tenant. Use UserTenant for tenant-scoped queries.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAndProviderAsync(string email, string provider, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for UserTenant operations (user-tenant mapping).
/// </summary>
public interface IUserTenantRepository
{
    Task<UserTenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserTenant?> GetByUserAndTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserTenant>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserTenant>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserTenant userTenant, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserTenant userTenant, CancellationToken cancellationToken = default);
}
