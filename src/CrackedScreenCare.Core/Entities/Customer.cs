namespace CrackedScreenCare.Core.Entities;

/// <summary>
/// Represents a customer in the repair business.
/// Asset-centric model: Each customer can own multiple devices (assets).
/// </summary>
public class Customer
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property: One customer can own many assets
    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
