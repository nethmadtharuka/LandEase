using LandEase.Application.DTOs.Bookings;
using LandEase.Application.Exceptions;
using LandEase.Application.Interfaces;
using LandEase.Domain.Entities;
using LandEase.Domain.Enums;
using LandEase.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LandEase.Infrastructure;

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;

    public BookingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BookingDto> CreateAsync(int migrantId, CreateBookingDto dto)
    {
        var service = await _context.ServiceListings
            .Include(s => s.Provider)
            .FirstOrDefaultAsync(s => s.Id == dto.ServiceId)
            ?? throw new NotFoundException("Service not found or is no longer available.");

        if (!service.IsActive)
            throw new ConflictException("This service is no longer available.");

        if (service.ProviderId == migrantId)
            throw new ConflictException("You cannot book your own service.");

        var existingBooking = await _context.Bookings
            .AnyAsync(b =>
                b.ServiceId == dto.ServiceId &&
                b.MigrantId == migrantId &&
                b.Status != BookingStatus.Cancelled &&
                b.Status != BookingStatus.Completed);

        if (existingBooking)
            throw new ConflictException(
                "You already have an active booking for this service.");

        var booking = new Booking
        {
            ServiceId = dto.ServiceId,
            MigrantId = migrantId,
            Notes = dto.Notes,
            ScheduledDate = dto.ScheduledDate,
            Status = BookingStatus.Requested,
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return await MapToDtoAsync(booking.Id);
    }

    public async Task<List<BookingDto>> GetMyBookingsAsync(int userId)
    {
        return await _context.Bookings
            .Include(b => b.Service)
                .ThenInclude(s => s.Provider)
            .Include(b => b.Migrant)
            .Include(b => b.Review)
            .Where(b => b.MigrantId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => MapToDto(b))
            .ToListAsync();
    }

    public async Task<List<BookingDto>> GetIncomingBookingsAsync(int providerId)
    {
        return await _context.Bookings
            .Include(b => b.Service)
                .ThenInclude(s => s.Provider)
            .Include(b => b.Migrant)
            .Include(b => b.Review)
            .Where(b => b.Service.ProviderId == providerId)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => MapToDto(b))
            .ToListAsync();
    }

    public async Task<BookingDto> GetByIdAsync(int bookingId, int requestingUserId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Service)
                .ThenInclude(s => s.Provider)
            .Include(b => b.Migrant)
            .Include(b => b.Review)
            .FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new NotFoundException("Booking not found.");

        if (booking.MigrantId != requestingUserId &&
            booking.Service.ProviderId != requestingUserId)
            throw new ForbiddenException("You are not authorized to view this booking.");

        return MapToDto(booking);
    }

    public async Task<BookingDto> UpdateStatusAsync(
        int bookingId, int requestingUserId, UpdateBookingStatusDto dto)
    {
        var booking = await _context.Bookings
            .Include(b => b.Service)
                .ThenInclude(s => s.Provider)
            .Include(b => b.Migrant)
            .Include(b => b.Review)
            .FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new NotFoundException("Booking not found.");

        var isProvider = booking.Service.ProviderId == requestingUserId;
        var isMigrant = booking.MigrantId == requestingUserId;

        // Business rules for status transitions
        switch (dto.Status)
        {
            case BookingStatus.Accepted:
            case BookingStatus.InProgress:
                if (!isProvider)
                    throw new ForbiddenException(
                        "Only the service provider can accept or start a booking.");
                break;

            case BookingStatus.Completed:
                if (!isProvider)
                    throw new ForbiddenException(
                        "Only the service provider can mark a booking as completed.");
                if (booking.Status != BookingStatus.InProgress)
                    throw new ConflictException(
                        "Only in-progress bookings can be marked as completed.");
                break;

            case BookingStatus.Cancelled:
                if (!isMigrant && !isProvider)
                    throw new ForbiddenException(
                        "Only the migrant or provider can cancel a booking.");
                if (booking.Status == BookingStatus.Completed)
                    throw new ConflictException("Cannot cancel a completed booking.");
                break;

            default:
                throw new ConflictException("Invalid status transition.");
        }

        booking.Status = dto.Status;
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToDto(booking);
    }

    private async Task<BookingDto> MapToDtoAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Service)
                .ThenInclude(s => s.Provider)
            .Include(b => b.Migrant)
            .Include(b => b.Review)
            .FirstAsync(b => b.Id == bookingId);

        return MapToDto(booking);
    }

    private static BookingDto MapToDto(Booking booking) => new()
    {
        Id = booking.Id,
        ServiceId = booking.ServiceId,
        ServiceTitle = booking.Service.Title,
        ProviderName = booking.Service.Provider.FullName,
        ProviderId = booking.Service.ProviderId,
        MigrantId = booking.MigrantId,
        MigrantName = booking.Migrant.FullName,
        Status = booking.Status,
        Notes = booking.Notes,
        ScheduledDate = booking.ScheduledDate,
        CreatedAt = booking.CreatedAt,
        UpdatedAt = booking.UpdatedAt,
        HasReview = booking.Review != null
    };
}