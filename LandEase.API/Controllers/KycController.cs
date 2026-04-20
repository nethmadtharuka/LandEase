using LandEase.API.Models;
using LandEase.Application.DTOs.Kyc;
using LandEase.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LandEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KycController : ControllerBase
{
    private readonly IKycService _kycService;

    public KycController(IKycService kycService)
    {
        _kycService = kycService;
    }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromForm] KycSubmissionDto dto)
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _kycService.SubmitKycAsync(userId, dto);
        return Ok(ApiResponse<KycStatusDto>.Ok(
            result, "KYC submitted successfully."));
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _kycService.GetMyKycStatusAsync(userId);
        return Ok(ApiResponse<KycStatusDto>.Ok(result));
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Agency")]
    public async Task<IActionResult> GetPending()
    {
        var result = await _kycService.GetPendingKycRecordsAsync();
        return Ok(ApiResponse<List<KycDetailDto>>.Ok(result));
    }

    [HttpPut("{id}/review")]
    [Authorize(Roles = "Agency")]
    public async Task<IActionResult> Review(int id, [FromBody] KycReviewDto dto)
    {
        var reviewerId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _kycService.ReviewKycAsync(id, reviewerId, dto);
        return Ok(ApiResponse<KycStatusDto>.Ok(
            result, "KYC review submitted."));
    }
}