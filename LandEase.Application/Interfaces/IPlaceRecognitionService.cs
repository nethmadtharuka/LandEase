using LandEase.Application.DTOs.Ai;

namespace LandEase.Application.Interfaces;

public interface IPlaceRecognitionService
{
    Task<PlaceRecognitionResponseDto> RecognizePlaceAsync(
        PlaceRecognitionRequestDto dto);
}