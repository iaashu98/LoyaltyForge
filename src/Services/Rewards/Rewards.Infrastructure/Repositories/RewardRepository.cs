using Rewards.Infrastructure.Persistence;
using Rewards.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Rewards.Application.Interfaces;

namespace Rewards.Infrastructure.Repositories;

public class RewardRepository(RewardsDbContext context) : IRewardRepository
{
    private readonly RewardsDbContext _context = context;

    public async Task AddAsync(RewardCatalog rewardCatalog, CancellationToken cancellationToken = default)
    {
        await _context.RewardCatalogs.AddAsync(rewardCatalog, cancellationToken);
    }

    public async Task UpdateAsync(RewardCatalog rewardCatalog, CancellationToken cancellationToken = default)
    {
        await _context.RewardCatalogs.Where(x => x.Id == rewardCatalog.Id).ExecuteUpdateAsync(setters => setters
            .SetProperty(x => x.Name, rewardCatalog.Name)
            .SetProperty(x => x.Description, rewardCatalog.Description)
            .SetProperty(x => x.PointsCost, rewardCatalog.PointsCost)
            .SetProperty(x => x.RewardType, rewardCatalog.RewardType)
            .SetProperty(x => x.RewardValue, rewardCatalog.RewardValue)
            .SetProperty(x => x.IsLimited, rewardCatalog.IsLimited)
            .SetProperty(x => x.TotalQuantity, rewardCatalog.TotalQuantity)
            .SetProperty(x => x.RemainingQuantity, rewardCatalog.RemainingQuantity)
            .SetProperty(x => x.IsActive, rewardCatalog.IsActive)
            .SetProperty(x => x.ValidFrom, rewardCatalog.ValidFrom)
            .SetProperty(x => x.ValidUntil, rewardCatalog.ValidUntil)
            .SetProperty(x => x.MaxPerUser, rewardCatalog.MaxPerUser)
            .SetProperty(x => x.ImageUrl, rewardCatalog.ImageUrl)
            .SetProperty(x => x.DisplayOrder, rewardCatalog.DisplayOrder)
            .SetProperty(x => x.UpdatedAt, rewardCatalog.UpdatedAt),
            cancellationToken);
    }

    public async Task DeleteAsync(RewardCatalog rewardCatalog, CancellationToken cancellationToken = default)
    {
        await _context.RewardCatalogs.Where(x => x.Id == rewardCatalog.Id).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RewardCatalog>> GetAllByTenantAsync(Guid tenantId, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _context.RewardCatalogs.Where(x => x.TenantId == tenantId);

        if (activeOnly)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query.OrderBy(r => r.Name).ToListAsync(cancellationToken);
    }

    public async Task<RewardCatalog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.RewardCatalogs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}