using Microsoft.EntityFrameworkCore;
using Rewards.Application.Interfaces;
using Rewards.Domain.Entities;
using Rewards.Infrastructure.Persistence;

namespace Rewards.Infrastructure.Repositories;

public class RedemptionRepository : IRedemptionRepository
{
    private readonly RewardsDbContext _context;

    public RedemptionRepository(RewardsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RewardRedemption redemption, CancellationToken cancellationToken = default)
    {
        await _context.RewardRedemptions.AddAsync(redemption, cancellationToken);
    }

    public async Task<IReadOnlyList<RewardRedemption>> GetByCustomerAsync(Guid tenantId, Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.RewardRedemptions.Where(x => x.TenantId == tenantId && x.UserId == customerId).ToListAsync(cancellationToken);
    }

    public async Task<RewardRedemption?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.RewardRedemptions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<RewardRedemption?> GetByIdempotencyKeyAsync(Guid tenantId, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return await _context.RewardRedemptions.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public async Task UpdateAsync(RewardRedemption redemption, CancellationToken cancellationToken = default)
    {
        await _context.RewardRedemptions
            .Where(x => x.Id == redemption.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, redemption.Status)
                .SetProperty(x => x.FulfillmentData, redemption.FulfillmentData)
                .SetProperty(x => x.ExternalReference, redemption.ExternalReference)
                .SetProperty(x => x.FulfilledAt, redemption.FulfilledAt)
                .SetProperty(x => x.ExpiresAt, redemption.ExpiresAt)
                .SetProperty(x => x.UpdatedAt, redemption.UpdatedAt),
                cancellationToken);
    }
}