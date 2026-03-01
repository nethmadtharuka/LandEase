using LandEase.Application.DTOs.Admin;
using LandEase.Application.Interfaces;
using LandEase.Domain.Entities;
using LandEase.Infrastructure.Data;
using LandEase.Infrastructure.ExternalServices;
using Microsoft.EntityFrameworkCore;

namespace LandEase.Infrastructure;

public class FraudDetectionService : IFraudDetectionService
{
    private readonly AppDbContext _context;
    private readonly GeminiAiService _gemini;

    public FraudDetectionService(AppDbContext context, GeminiAiService gemini)
    {
        _context = context;
        _gemini = gemini;
    }

    public async Task RunChecksForUserAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.KycRecord)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return;

        // Rule 1 — New user posting service (account < 24 hours old)
        if (user.CreatedAt >= DateTime.UtcNow.AddHours(-24))
        {
            var hasService = await _context.ServiceListings
                .AnyAsync(s => s.ProviderId == userId);

            if (hasService)
                await AddFlagIfNotExistsAsync(userId, "NEW_USER_POSTING",
                    "User posted a service within 24 hours of account creation.");
        }

        // Rule 2 — User receiving 3+ one-star reviews in 7 days
        var recentOneStarReviews = await _context.Reviews
            .Include(r => r.Booking)
                .ThenInclude(b => b.Service)
            .Where(r =>
                r.Booking.Service.ProviderId == userId &&
                r.Rating == 1 &&
                r.CreatedAt >= DateTime.UtcNow.AddDays(-7))
            .CountAsync();

        if (recentOneStarReviews >= 3)
            await AddFlagIfNotExistsAsync(userId, "MULTIPLE_BAD_REVIEWS",
                $"User received {recentOneStarReviews} one-star reviews in the last 7 days.");
    }

    public async Task RunChecksForServiceAsync(int serviceId)
    {
        var service = await _context.ServiceListings
            .Include(s => s.Provider)
            .FirstOrDefaultAsync(s => s.Id == serviceId);

        if (service == null) return;

        // Rule 3 — Service price 3x above category average
        var categoryAvgPrice = await _context.ServiceListings
            .Where(s => s.Category == service.Category &&
                        s.IsActive &&
                        s.Id != serviceId)
            .AverageAsync(s => (double?)s.Price) ?? 0;

        if (categoryAvgPrice > 0 && (double)service.Price > categoryAvgPrice * 3)
            await AddFlagIfNotExistsAsync(service.ProviderId, "PRICE_ANOMALY",
                $"Service '{service.Title}' priced at {service.Price} is 3x above " +
                $"category average of {categoryAvgPrice:F2}.");
    }

    public async Task<ModerationResultDto> ModerateReviewAsync(string reviewContent)
    {
        // Rule-based check first — phone numbers and external links
        var containsPhone = System.Text.RegularExpressions.Regex.IsMatch(
            reviewContent, @"\b\d{9,}\b");
        var containsLink = reviewContent.Contains("http://") ||
                           reviewContent.Contains("https://") ||
                           reviewContent.Contains("www.");

        if (containsPhone)
            return new ModerationResultDto
            {
                IsFlagged = true,
                Reason = "Review contains a phone number which is not allowed."
            };

        if (containsLink)
            return new ModerationResultDto
            {
                IsFlagged = true,
                Reason = "Review contains an external link which is not allowed."
            };

        // AI moderation via Gemini
        try
        {
            var prompt = $@"Analyze this review for inappropriate content. 
Check for: spam, hate speech, harassment, fake reviews, or solicitation.
Review: ""{reviewContent}""
Reply with ONLY: SAFE or FLAGGED: [reason]";

            var response = await _gemini.GenerateResponseAsync(
                "You are a content moderation system. Be strict but fair.",
                new List<(string, string)>(),
                prompt);

            if (response.StartsWith("FLAGGED", StringComparison.OrdinalIgnoreCase))
            {
                var reason = response.Contains(":")
                    ? response.Split(':', 2)[1].Trim()
                    : "Content flagged by AI moderation.";

                return new ModerationResultDto
                {
                    IsFlagged = true,
                    Reason = reason
                };
            }
        }
        catch
        {
            // If AI moderation fails, fall through to safe
        }

        return new ModerationResultDto { IsFlagged = false, Reason = "Content is safe." };
    }

    public async Task<List<FraudFlagDto>> GetActiveFlagsAsync()
    {
        return await _context.FraudFlags
            .Include(f => f.User)
            .Where(f => !f.IsResolved)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FraudFlagDto
            {
                Id = f.Id,
                UserId = f.UserId,
                UserFullName = f.User.FullName,
                UserEmail = f.User.Email,
                FlagType = f.FlagType,
                Description = f.Description,
                IsResolved = f.IsResolved,
                CreatedAt = f.CreatedAt,
                ResolvedAt = f.ResolvedAt
            })
            .ToListAsync();
    }

    public async Task ResolveFlagAsync(int flagId)
    {
        var flag = await _context.FraudFlags.FindAsync(flagId)
            ?? throw new Exception("Fraud flag not found.");

        flag.IsResolved = true;
        flag.ResolvedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private async Task AddFlagIfNotExistsAsync(
        int userId, string flagType, string description)
    {
        var exists = await _context.FraudFlags
            .AnyAsync(f =>
                f.UserId == userId &&
                f.FlagType == flagType &&
                !f.IsResolved);

        if (!exists)
        {
            _context.FraudFlags.Add(new FraudFlag
            {
                UserId = userId,
                FlagType = flagType,
                Description = description,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
    }
}