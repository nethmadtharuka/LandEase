using LandEase.Application.DTOs.Reviews;
using LandEase.Application.Interfaces;
using LandEase.Domain.Entities;
using LandEase.Domain.Enums;
using LandEase.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LandEase.Infrastructure;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;

    public ReviewService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ReviewDto> CreateAsync(int reviewerId, CreateReviewDto dto)
    {
        var booking = await _context.Bookings
            .Include(b => b.Service)
            .Include(b => b.Review)
            .FirstOrDefaultAsync(b => b.Id == dto.BookingId)
            ?? throw new Exception("Booking not found.");

        if (booking.MigrantId != reviewerId)
            throw new Exception("Only the migrant who booked can leave a review.");

        if (booking.Status != BookingStatus.Completed)
            throw new Exception("You can only review a completed booking.");

        if (booking.Review != null)
            throw new Exception("You have already reviewed this booking.");

        var review = new Review
        {
            BookingId = dto.BookingId,
            ReviewerId = reviewerId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        // Recalculate provider average rating
        await RecalculateProviderRatingAsync(booking.Service.ProviderId);

        return await MapToDtoAsync(review.Id);
    }

    public async Task<List<ReviewDto>> GetByProviderAsync(int providerId)
    {
        return await _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Booking)
                .ThenInclude(b => b.Service)
            .Where(r => r.Booking.Service.ProviderId == providerId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                BookingId = r.BookingId,
                ReviewerName = r.Reviewer.FullName,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    private async Task RecalculateProviderRatingAsync(int providerId)
    {
        var ratings = await _context.Reviews
            .Include(r => r.Booking)
                .ThenInclude(b => b.Service)
            .Where(r => r.Booking.Service.ProviderId == providerId)
            .Select(r => r.Rating)
            .ToListAsync();

        if (ratings.Count == 0) return;

        var provider = await _context.Users.FindAsync(providerId);
        if (provider == null) return;

        provider.AverageRating = Math.Round(
            (decimal)ratings.Average(), 2);

        await _context.SaveChangesAsync();
    }

    private async Task<ReviewDto> MapToDtoAsync(int reviewId)
    {
        var review = await _context.Reviews
            .Include(r => r.Reviewer)
            .FirstAsync(r => r.Id == reviewId);

        return new ReviewDto
        {
            Id = review.Id,
            BookingId = review.BookingId,
            ReviewerName = review.Reviewer.FullName,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }
}