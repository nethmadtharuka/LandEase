using LandEase.Application.DTOs.Immigration;

namespace LandEase.Application.Interfaces;

public interface IImmigrationPredictorService
{
    Task<ScoreResultDto> AnalyzeProfileAsync(
        ProfileSubmitDto dto,
        List<(byte[] fileBytes, string docType)>? uploadedDocuments = null);

    List<string> GetSupportedVisaTypes();
    List<string> GetSupportedCountries();
}