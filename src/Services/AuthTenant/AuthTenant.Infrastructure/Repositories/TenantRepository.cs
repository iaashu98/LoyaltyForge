using AuthTenant.Application.Interfaces;
using AuthTenant.Domain.Entities;
using AuthTenant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthTenant.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ITenantRepository.
/// </summary>
public class TenantRepository : ITenantRepository
{
    private readonly AuthTenantDbContext _context;

    public TenantRepository(AuthTenantDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants.FindAsync([id], cancellationToken);
    }

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants.FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
    }

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tenants.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _context.Tenants.AddAsync(tenant, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
