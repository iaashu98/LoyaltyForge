namespace LoyaltyForge.Common.Interfaces;

/// <summary>
/// Unit of work pattern interface.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
