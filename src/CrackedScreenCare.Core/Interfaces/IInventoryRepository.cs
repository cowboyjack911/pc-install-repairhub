using CrackedScreenCare.Core.Entities;

namespace CrackedScreenCare.Core.Interfaces;

/// <summary>
/// Repository interface for managing inventory.
/// This enforces modular boundaries - other modules can only access inventory
/// through this interface, not by directly accessing the database.
/// </summary>
public interface IInventoryRepository
{
    Task<int> GetStockLevelAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<bool> ReserveStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
    Task<bool> ReleaseStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
}
