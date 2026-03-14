using LandEase.Application.DTOs.Reviews;

namespace LandEase.Application.Interfaces;

public interface IReviewService
{
    Task<ReviewDto> CreateAsync(int reviewerId, CreateReviewDto dto);
    Task<List<ReviewDto>> GetByProviderAsync(int providerId);
}