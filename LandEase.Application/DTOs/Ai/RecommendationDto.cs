using LandEase.Domain.Enums;

namespace LandEase.Application.DTOs.Ai;

public class RecommendationDto
{
    public int ServiceId { get; set; }
    public string ServiceTitle { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public decimal ProviderRating { get; set; }
    public ServiceCategory Category { get; set; }
    public decimal Price { get; set; }
    public string DestinationCountry { get; set; } = string.Empty;
    public int MatchScore { get; set; }
    public string MatchReason { get; set; } = string.Empty;
}