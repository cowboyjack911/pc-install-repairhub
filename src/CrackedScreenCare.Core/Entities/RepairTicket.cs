namespace CrackedScreenCare.Core.Entities;

/// <summary>
/// Represents a repair ticket for a specific asset.
/// All tickets are linked to an Asset, not directly to a Customer,
/// providing a complete service history per device.
/// </summary>
public class RepairTicket
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RepairStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public string? TechnicianNotes { get; set; }
    
    // Navigation property
    public virtual Asset Asset { get; set; } = null!;
}

public enum RepairStatus
{
    Created,
    AwaitingQuoteApproval,
    InProgress,
    AwaitingParts,
    Completed,
    Cancelled
}
