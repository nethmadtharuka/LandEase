using LandEase.API.Models;
using LandEase.Application.DTOs.Reviews;
using LandEase.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LandEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    [Authorize(Roles = "Migrant")]
    public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _reviewService.CreateAsync(userId, dto);
        return Ok(ApiResponse<ReviewDto>.Ok(
            result, "Review submitted successfully."));
    }

    [HttpGet("provider/{providerId}")]
    public async Task<IActionResult> GetByProvider(int providerId)
    {
        var result = await _reviewService.GetByProviderAsync(providerId);
        return Ok(ApiResponse<List<ReviewDto>>.Ok(result));
    }
}