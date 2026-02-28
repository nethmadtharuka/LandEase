using LandEase.Application.DTOs.Bookings;

namespace LandEase.Application.Interfaces;

public interface IBookingService
{
    Task<BookingDto> CreateAsync(int migrantId, CreateBookingDto dto);
    Task<List<BookingDto>> GetMyBookingsAsync(int userId);
    Task<BookingDto> GetByIdAsync(int bookingId, int requestingUserId);
    Task<BookingDto> UpdateStatusAsync(
        int bookingId, int requestingUserId, UpdateBookingStatusDto dto);
    Task<List<BookingDto>> GetIncomingBookingsAsync(int providerId);
}