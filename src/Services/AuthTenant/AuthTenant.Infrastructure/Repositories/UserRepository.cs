using AuthTenant.Application.Interfaces;
using AuthTenant.Domain.Entities;
using AuthTenant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthTenant.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IUserRepository.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AuthTenantDbContext _context;

    public UserRepository(AuthTenantDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.UserTenants)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAndProviderAsync(string email, string provider, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.UserTenants)
            .FirstOrDefaultAsync(u => u.Email == email && u.Provider == provider, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// EF Core implementation of IUserTenantRepository.
/// </summary>
public class UserTenantRepository : IUserTenantRepository
{
    private readonly AuthTenantDbContext _context;

    public UserTenantRepository(AuthTenantDbContext context)
    {
        _context = context;
    }

    public async Task<UserTenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UserTenants
            .Include(ut => ut.User)
            .Include(ut => ut.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(ut => ut.Id == id, cancellationToken);
    }

    public async Task<UserTenant?> GetByUserAndTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.UserTenants
            .Include(ut => ut.User)
            .Include(ut => ut.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TenantId == tenantId, cancellationToken);
    }

    public async Task<IReadOnlyList<UserTenant>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.UserTenants
            .Include(ut => ut.User)
            .Where(ut => ut.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserTenant>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserTenants
            .Include(ut => ut.Tenant)
            .Where(ut => ut.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UserTenant userTenant, CancellationToken cancellationToken = default)
    {
        await _context.UserTenants.AddAsync(userTenant, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UserTenant userTenant, CancellationToken cancellationToken = default)
    {
        _context.UserTenants.Update(userTenant);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
