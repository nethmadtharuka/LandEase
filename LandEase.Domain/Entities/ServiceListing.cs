using LandEase.Domain.Enums;

namespace LandEase.Domain.Entities;

public class ServiceListing
{
    public int Id { get; set; }
    public int ProviderId { get; set; }
    public User Provider { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ServiceCategory Category { get; set; }
    public decimal Price { get; set; }
    public string DestinationCountry { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}