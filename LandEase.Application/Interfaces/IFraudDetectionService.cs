using LandEase.Application.DTOs.Admin;

namespace LandEase.Application.Interfaces;

public interface IFraudDetectionService
{
    Task RunChecksForUserAsync(int userId);
    Task RunChecksForServiceAsync(int serviceId);
    Task<ModerationResultDto> ModerateReviewAsync(string reviewContent);
    Task<List<FraudFlagDto>> GetActiveFlagsAsync();
    Task ResolveFlagAsync(int flagId);
}