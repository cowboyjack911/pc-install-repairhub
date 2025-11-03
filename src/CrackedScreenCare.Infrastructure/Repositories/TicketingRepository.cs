using CrackedScreenCare.Core.Entities;
using CrackedScreenCare.Core.Interfaces;
using CrackedScreenCare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CrackedScreenCare.Infrastructure.Repositories;

/// <summary>
/// Implementation of ITicketingRepository.
/// Provides data access for the Ticketing module while maintaining modular boundaries.
/// </summary>
public class TicketingRepository : ITicketingRepository
{
    private readonly AppDbContext _context;

    public TicketingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RepairTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.RepairTickets
            .Include(t => t.Asset)
                .ThenInclude(a => a.Customer)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<RepairTicket>> GetByAssetIdAsync(Guid assetId, CancellationToken cancellationToken = default)
    {
        return await _context.RepairTickets
            .Where(t => t.AssetId == assetId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<RepairTicket> CreateAsync(RepairTicket ticket, CancellationToken cancellationToken = default)
    {
        ticket.CreatedAt = DateTime.UtcNow;
        _context.RepairTickets.Add(ticket);
        await _context.SaveChangesAsync(cancellationToken);
        return ticket;
    }

    public async Task UpdateAsync(RepairTicket ticket, CancellationToken cancellationToken = default)
    {
        ticket.UpdatedAt = DateTime.UtcNow;
        _context.RepairTickets.Update(ticket);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
