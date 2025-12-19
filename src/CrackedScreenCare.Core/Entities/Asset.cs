namespace CrackedScreenCare.Core.Entities;

/// <summary>
/// Represents a device (asset) owned by a customer.
/// This is the central entity in the asset-centric repair business model.
/// Each asset has its own discrete service history via RepairTickets.
/// </summary>
public class Asset
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string DeviceType { get; set; } = string.Empty; // e.g., "iPhone", "Laptop", "Custom PC"
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual ICollection<RepairTicket> RepairTickets { get; set; } = new List<RepairTicket>();
}
