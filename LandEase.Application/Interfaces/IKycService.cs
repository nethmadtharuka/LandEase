using LandEase.Application.DTOs.Kyc;

namespace LandEase.Application.Interfaces;

public interface IKycService
{
    Task<KycStatusDto> SubmitKycAsync(int userId, KycSubmissionDto dto);
    Task<KycStatusDto> GetMyKycStatusAsync(int userId);
    Task<List<KycDetailDto>> GetPendingKycRecordsAsync();
    Task<KycStatusDto> ReviewKycAsync(int kycId, int reviewerUserId, KycReviewDto dto);
}