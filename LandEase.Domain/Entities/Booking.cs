using LandEase.Domain.Enums;

namespace LandEase.Domain.Entities;

public class Booking
{
    public int Id { get; set; }

    public int ServiceId { get; set; }
    public ServiceListing Service { get; set; } = null!;

    public int MigrantId { get; set; }
    public User Migrant { get; set; } = null!;

    public BookingStatus Status { get; set; } = BookingStatus.Requested;
    public string? Notes { get; set; }
    public DateTime? ScheduledDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Review? Review { get; set; }
}