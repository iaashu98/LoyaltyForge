using Microsoft.EntityFrameworkCore;
using PointsEngine.Application.Interfaces;
using PointsEngine.Domain.Entities;
using PointsEngine.Infrastructure.Persistence;

namespace PointsEngine.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IUserBalanceRepository.
/// Handles cached balance operations.
/// </summary>
public class UserBalanceRepository(PointsEngineDbContext context) : IUserBalanceRepository
{
    public async Task<UserBalance?> GetByUserAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await context.UserBalances
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.UserId == userId, cancellationToken);
    }

    public async Task<UserBalance> GetOrCreateAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetByUserAsync(tenantId, userId, cancellationToken);

        if (existing != null)
        {
            return existing;
        }

        // Create new balance record
        var balance = UserBalance.Create(tenantId, userId);
        await context.UserBalances.AddAsync(balance, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return balance;
    }

    public async Task UpdateAsync(UserBalance balance, CancellationToken cancellationToken = default)
    {
        context.UserBalances.Update(balance);
        await context.SaveChangesAsync(cancellationToken);
    }
}
