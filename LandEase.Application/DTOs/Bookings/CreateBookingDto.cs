namespace LandEase.Application.DTOs.Bookings;

public class CreateBookingDto
{
    public int ServiceId { get; set; }
    public string? Notes { get; set; }
    public DateTime? ScheduledDate { get; set; }
}