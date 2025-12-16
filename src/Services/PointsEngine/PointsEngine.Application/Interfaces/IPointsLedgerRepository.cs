using PointsEngine.Domain.Entities;

namespace PointsEngine.Application.Interfaces;

/// <summary>
/// Repository interface for points ledger operations.
/// </summary>
public interface IPointsLedgerRepository
{
    Task<IReadOnlyList<PointsLedgerEntry>> GetByCustomerAsync(
        Guid tenantId, 
        Guid customerId, 
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<PointsLedgerEntry>> GetByTransactionIdAsync(
        Guid transactionId, 
        CancellationToken cancellationToken = default);
    
    Task AddAsync(PointsLedgerEntry entry, CancellationToken cancellationToken = default);
}
