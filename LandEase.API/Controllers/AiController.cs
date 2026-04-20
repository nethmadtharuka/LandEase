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
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var result = await _aiChatService.ChatAsync(userId, dto);
        return Ok(ApiResponse<ChatResponseDto>.Ok(result));
    }

    [HttpGet("chat/history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string? sessionId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _aiChatService.GetHistoryAsync(
            userId, sessionId, page, pageSize);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpDelete("chat/history")]
    public async Task<IActionResult> ClearHistory()
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _aiChatService.ClearHistoryAsync(userId);
        return Ok(ApiResponse<string>.Ok("", "Chat history cleared successfully."));
    }

    // ── Translation Endpoint ──────────────────────────────────

    [HttpPost("translate")]
    [EnableRateLimiting("ai")]
    public async Task<IActionResult> Translate([FromBody] TranslateDto dto)
    {
        _ = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var reply = await _aiChatService.TranslateAsync(dto);
        return Ok(ApiResponse<object>.Ok(new { reply }));
    }

    // ── Recommendation Endpoints ──────────────────────────────

    [HttpGet("recommendations")]
    [Authorize(Roles = "Migrant")]
    public async Task<IActionResult> GetRecommendations()
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _recommendationService
            .GetRecommendationsAsync(userId);
        return Ok(ApiResponse<List<RecommendationDto>>.Ok(result));
    }

    [HttpGet("recommendations/explain/{serviceId}")]
    [Authorize(Roles = "Migrant")]
    public async Task<IActionResult> GetExplanation(int serviceId)
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _recommendationService
            .GetExplanationAsync(userId, serviceId);
        return Ok(ApiResponse<RecommendationExplanationDto>.Ok(result));
    }

    // Rate limiting for chat/translate is handled by ASP.NET Core rate limiting middleware.
}