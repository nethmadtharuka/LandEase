using LandEase.API.Models;
using LandEase.Application.DTOs.Ai;
using LandEase.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LandEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlaceController : ControllerBase
{
    private readonly IPlaceRecognitionService _placeService;

    public PlaceController(IPlaceRecognitionService placeService)
    {
        _placeService = placeService;
    }

    [HttpPost("recognize")]
    public async Task<IActionResult> Recognize(
        [FromBody] PlaceRecognitionRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ImageBase64))
            return BadRequest(ApiResponse<object>.Fail(
                "Image data is required."));

        var result = await _placeService.RecognizePlaceAsync(dto);
        return Ok(ApiResponse<PlaceRecognitionResponseDto>.Ok(result));
    }
}