using LandEase.Application.DTOs.Ai;

namespace LandEase.Application.Interfaces;

public interface IRecommendationService
{
    Task<List<RecommendationDto>> GetRecommendationsAsync(int userId);
    Task<RecommendationExplanationDto> GetExplanationAsync(
        int userId, int serviceId);
}