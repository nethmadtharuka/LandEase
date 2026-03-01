namespace LandEase.Application.DTOs.Ai;

public class RecommendationExplanationDto
{
    public int ServiceId { get; set; }
    public string ServiceTitle { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}