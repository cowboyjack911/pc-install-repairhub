using CrackedScreenCare.Core.Entities;

namespace CrackedScreenCare.Core.Interfaces;

/// <summary>
/// Repository interface for managing repair tickets.
/// Enforces modular boundaries for the ticketing module.
/// </summary>
public interface ITicketingRepository
{
    Task<RepairTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<RepairTicket>> GetByAssetIdAsync(Guid assetId, CancellationToken cancellationToken = default);
    Task<RepairTicket> CreateAsync(RepairTicket ticket, CancellationToken cancellationToken = default);
    Task UpdateAsync(RepairTicket ticket, CancellationToken cancellationToken = default);
}
