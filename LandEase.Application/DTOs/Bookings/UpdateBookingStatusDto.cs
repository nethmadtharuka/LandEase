using LandEase.Domain.Enums;

namespace LandEase.Application.DTOs.Bookings;

public class UpdateBookingStatusDto
{
    public BookingStatus Status { get; set; }
}