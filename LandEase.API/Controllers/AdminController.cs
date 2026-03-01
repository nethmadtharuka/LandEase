using LandEase.API.Models;
using LandEase.Application.DTOs.Admin;
using LandEase.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LandEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Agency")]
public class AdminController : ControllerBase
{
    private readonly IFraudDetectionService _fraudDetectionService;

    public AdminController(IFraudDetectionService fraudDetectionService)
    {
        _fraudDetectionService = fraudDetectionService;
    }

    [HttpGet("fraud-flags")]
    public async Task<IActionResult> GetFraudFlags()
    {
        try
        {
            var result = await _fraudDetectionService.GetActiveFlagsAsync();
            return Ok(ApiResponse<List<FraudFlagDto>>.Ok(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<FraudFlagDto>>.Fail(ex.Message));
        }
    }

    [HttpPut("fraud-flags/{id}/resolve")]
    public async Task<IActionResult> ResolveFlag(int id)
    {
        try
        {
            await _fraudDetectionService.ResolveFlagAsync(id);
            return Ok(ApiResponse<string>.Ok("", "Flag resolved successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }

    [HttpPost("moderate/review")]
    public async Task<IActionResult> ModerateReview(
        [FromBody] ModerateReviewRequestDto dto)
    {
        try
        {
            var result = await _fraudDetectionService
                .ModerateReviewAsync(dto.Content);
            return Ok(ApiResponse<ModerationResultDto>.Ok(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ModerationResultDto>.Fail(ex.Message));
        }
    }
}