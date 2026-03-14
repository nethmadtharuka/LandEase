using LandEase.Application.DTOs.Immigration;

namespace LandEase.Application.Interfaces;

public interface IImmigrationPredictorService
{
    Task<ScoreResultDto> AnalyzeProfileAsync(ProfileSubmitDto dto);
    List<string> GetSupportedVisaTypes();
    List<string> GetSupportedCountries();
}
