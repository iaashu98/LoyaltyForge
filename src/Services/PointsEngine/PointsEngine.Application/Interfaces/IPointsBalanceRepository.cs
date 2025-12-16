using PointsEngine.Domain.Entities;

namespace PointsEngine.Application.Interfaces;

/// <summary>
/// Repository interface for points balance operations.
/// </summary>
public interface IPointsBalanceRepository
{
    Task<PointsBalance?> GetByCustomerAsync(
        Guid tenantId, 
        Guid customerId, 
        CancellationToken cancellationToken = default);
    
    Task<PointsBalance> GetOrCreateAsync(
        Guid tenantId, 
        Guid customerId, 
        CancellationToken cancellationToken = default);
    
    Task UpdateAsync(PointsBalance balance, CancellationToken cancellationToken = default);
}
