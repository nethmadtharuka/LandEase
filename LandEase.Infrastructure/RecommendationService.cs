using LandEase.Application.DTOs.Ai;
using LandEase.Application.Interfaces;
using LandEase.Domain.Enums;
using LandEase.Infrastructure.Data;
using LandEase.Infrastructure.ExternalServices;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LandEase.Infrastructure;

public class RecommendationService : IRecommendationService
{
    private readonly AppDbContext _context;
    private readonly GeminiAiService _gemini;

    public RecommendationService(AppDbContext context, GeminiAiService gemini)
    {
        _context = context;
        _gemini = gemini;
    }

    public async Task<List<RecommendationDto>> GetRecommendationsAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new Exception("User not found.");

        // Get all active services
        var services = await _context.ServiceListings
            .Include(s => s.Provider)
            .Where(s => s.IsActive && s.ProviderId != userId)
            .ToListAsync();

        if (!services.Any())
            return new List<RecommendationDto>();

        // Get user's recent bookings for category penalty
        var recentBookedCategories = await _context.Bookings
            .Include(b => b.Service)
            .Where(b => b.MigrantId == userId &&
                        b.CreatedAt >= DateTime.UtcNow.AddDays(-30))
            .Select(b => b.Service.Category)
            .ToListAsync();

        // Rule-based scoring
        var scored = services.Select(s =>
        {
            var score = 0;
            var reasons = new List<string>();

            // +30 if service country matches user destination
            if (!string.IsNullOrEmpty(user.DestinationCountry) &&
                s.DestinationCountry.ToLower() ==
                user.DestinationCountry.ToLower())
            {
                score += 30;
                reasons.Add("matches your destination country");
            }

            // +20 for category matching migration status
            if (CategoryMatchesMigrationStatus(s.Category, user.MigrationStatus))
            {
                score += 20;
                reasons.Add("matches your current migration needs");
            }

            // -10 for recently booked categories
            if (recentBookedCategories.Contains(s.Category))
            {
                score -= 10;
                reasons.Add("similar to recently booked");
            }

            // +10 per star above 3.0
            if (s.Provider.AverageRating > 3.0m)
            {
                var bonus = (int)((s.Provider.AverageRating - 3.0m) * 10);
                score += bonus;
                if (bonus > 0)
                    reasons.Add($"highly rated provider ({s.Provider.AverageRating}★)");
            }

            return new
            {
                Service = s,
                Score = score,
                Reason = reasons.Any()
                    ? string.Join(", ", reasons)
                    : "general recommendation"
            };
        })
        .OrderByDescending(x => x.Score)
        .Take(20)
        .ToList();

        // Top 5 after rule-based scoring
        var top5 = scored.Take(5).Select(x => new RecommendationDto
        {
            ServiceId = x.Service.Id,
            ServiceTitle = x.Service.Title,
            ProviderName = x.Service.Provider.FullName,
            ProviderRating = x.Service.Provider.AverageRating,
            Category = x.Service.Category,
            Price = x.Service.Price,
            DestinationCountry = x.Service.DestinationCountry,
            MatchScore = x.Score,
            MatchReason = x.Reason
        }).ToList();

        // Gemini re-ranking enhancement
        try
        {
            var enhanced = await EnhanceWithGeminiAsync(user, top5);
            return enhanced;
        }
        catch
        {
            // If Gemini fails, return rule-based results
            return top5;
        }
    }

    public async Task<RecommendationExplanationDto> GetExplanationAsync(
        int userId, int serviceId)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new Exception("User not found.");

        var service = await _context.ServiceListings
            .Include(s => s.Provider)
            .FirstOrDefaultAsync(s => s.Id == serviceId)
            ?? throw new Exception("Service not found.");

        var prompt = $@"Explain in 2-3 sentences why this service would be helpful 
for this migrant. Be specific and personal.

Migrant Profile:
- From: {user.OriginCountry}
- Moving to: {user.DestinationCountry}  
- Status: {user.MigrationStatus}

Service:
- Title: {service.Title}
- Category: {service.Category}
- Description: {service.Description}
- Provider Rating: {service.Provider.AverageRating}★
- Price: ${service.Price}

Give a warm, helpful explanation of why this service matches their needs.";

        var explanation = await _gemini.GenerateResponseAsync(
            "You are LandEase AI helping migrants find the right services.",
            new List<(string, string)>(),
            prompt);

        return new RecommendationExplanationDto
        {
            ServiceId = serviceId,
            ServiceTitle = service.Title,
            Explanation = explanation
        };
    }

    private async Task<List<RecommendationDto>> EnhanceWithGeminiAsync(
        Domain.Entities.User user,
        List<RecommendationDto> services)
    {
        var servicesJson = JsonSerializer.Serialize(services.Select(s => new
        {
            s.ServiceId,
            s.ServiceTitle,
            s.Category,
            s.Price,
            s.ProviderRating,
            s.MatchScore
        }));

        var prompt = $@"Re-rank these services for a migrant and provide a brief 
match reason for each. Return ONLY a JSON array.

Migrant: from {user.OriginCountry} to {user.DestinationCountry}, 
status: {user.MigrationStatus}

Services: {servicesJson}

Return JSON array with same serviceId values in your preferred order, 
each with a matchReason field. Example format:
[{{""serviceId"": 1, ""matchReason"": ""Perfect for newly arrived migrants""}}]";

        var response = await _gemini.GenerateResponseAsync(
            "You are a migration service recommendation engine. Return only valid JSON.",
            new List<(string, string)>(),
            prompt);

        // Parse Gemini response and update match reasons
        try
        {
            var clean = response
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var reranked = JsonSerializer.Deserialize<List<JsonElement>>(clean);

            if (reranked == null) return services;

            var result = new List<RecommendationDto>();
            foreach (var item in reranked)
            {
                var id = item.GetProperty("serviceId").GetInt32();
                var reason = item.GetProperty("matchReason").GetString() ?? "";
                var original = services.FirstOrDefault(s => s.ServiceId == id);
                if (original != null)
                {
                    original.MatchReason = reason;
                    result.Add(original);
                }
            }

            return result.Any() ? result : services;
        }
        catch
        {
            return services;
        }
    }

    private static bool CategoryMatchesMigrationStatus(
        ServiceCategory category, MigrationStatus status)
    {
        return status switch
        {
            MigrationStatus.Planning =>
                category == ServiceCategory.Legal ||
                category == ServiceCategory.Banking,

            MigrationStatus.NewlyArrived =>
                category == ServiceCategory.Accommodation ||
                category == ServiceCategory.Transport ||
                category == ServiceCategory.CityOrientation ||
                category == ServiceCategory.Food,

            MigrationStatus.Settled =>
                category == ServiceCategory.Jobs ||
                category == ServiceCategory.Education,

            _ => false
        };
    }
}