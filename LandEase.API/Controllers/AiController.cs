using LandEase.API.Models;
using LandEase.Application.DTOs.Ai;
using LandEase.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace LandEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAiChatService _aiChatService;
    private readonly IRecommendationService _recommendationService;

    public AiController(
        IAiChatService aiChatService,
        IRecommendationService recommendationService)
    {
        _aiChatService = aiChatService;
        _recommendationService = recommendationService;
    }

    // ── Chat Endpoints ────────────────────────────────────────

    [HttpPost("chat")]
    [EnableRateLimiting("ai")]
    public async Task<IActionResult> Chat([FromBody] ChatMessageDto dto)
    {
        try
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _aiChatService.ChatAsync(userId, dto);
            return Ok(ApiResponse<ChatResponseDto>.Ok(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ChatResponseDto>.Fail(ex.Message));
        }
    }

    [HttpGet("chat/history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string? sessionId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _aiChatService.GetHistoryAsync(
                userId, sessionId, page, pageSize);
            return Ok(ApiResponse<object>.Ok(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpDelete("chat/history")]
    public async Task<IActionResult> ClearHistory()
    {
        try
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _aiChatService.ClearHistoryAsync(userId);
            return Ok(ApiResponse<string>.Ok("", "Chat history cleared successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }

    // ── Translation Endpoint ──────────────────────────────────

    [HttpPost("translate")]
    [EnableRateLimiting("ai")]
    public async Task<IActionResult> Translate([FromBody] TranslateDto dto)
    {
        try
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var reply = await _aiChatService.TranslateAsync(dto);
            return Ok(ApiResponse<object>.Ok(new { reply }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // ── Recommendation Endpoints ──────────────────────────────

    [HttpGet("recommendations")]
    [Authorize(Roles = "Migrant")]
    public async Task<IActionResult> GetRecommendations()
    {
        try
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _recommendationService
                .GetRecommendationsAsync(userId);
            return Ok(ApiResponse<List<RecommendationDto>>.Ok(result));
        }
        catch (Exception ex)
        {
            return BadRequest(
                ApiResponse<List<RecommendationDto>>.Fail(ex.Message));
        }
    }

    [HttpGet("recommendations/explain/{serviceId}")]
    [Authorize(Roles = "Migrant")]
    public async Task<IActionResult> GetExplanation(int serviceId)
    {
        try
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _recommendationService
                .GetExplanationAsync(userId, serviceId);
            return Ok(ApiResponse<RecommendationExplanationDto>.Ok(result));
        }
        catch (Exception ex)
        {
            return BadRequest(
                ApiResponse<RecommendationExplanationDto>.Fail(ex.Message));
        }
    }

    // Rate limiting for chat/translate is handled by ASP.NET Core rate limiting middleware.
}