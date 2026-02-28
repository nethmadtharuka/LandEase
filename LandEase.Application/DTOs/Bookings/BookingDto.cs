using LandEase.Domain.Enums;

namespace LandEase.Application.DTOs.Bookings;

public class BookingDto
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public string ServiceTitle { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public int ProviderId { get; set; }
    public int MigrantId { get; set; }
    public string MigrantName { get; set; } = string.Empty;
    public BookingStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool HasReview { get; set; }
}