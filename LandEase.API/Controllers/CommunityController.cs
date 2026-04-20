using LandEase.API.Models;
using LandEase.Application.DTOs.Community;
using LandEase.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LandEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommunityController : ControllerBase
{
    private readonly ICommunityService _communityService;

    public CommunityController(ICommunityService communityService)
    {
        _communityService = communityService;
    }

    private int? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null ? int.Parse(claim) : null;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _communityService.GetAllAsync(
            GetUserId(), page, pageSize);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _communityService.GetByIdAsync(id, GetUserId());
        return Ok(ApiResponse<CommunityDto>.Ok(result));
    }

    [HttpPost("{id}/join")]
    [Authorize]
    public async Task<IActionResult> Join(int id)
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _communityService.JoinAsync(id, userId);
        return Ok(ApiResponse<string>.Ok("", "Joined community successfully."));
    }

    [HttpPost("{id}/leave")]
    [Authorize]
    public async Task<IActionResult> Leave(int id)
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _communityService.LeaveAsync(id, userId);
        return Ok(ApiResponse<string>.Ok("", "Left community successfully."));
    }

    [HttpGet("{id}/members")]
    public async Task<IActionResult> GetMembers(
        int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _communityService.GetMembersAsync(id, page, pageSize);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id}/posts")]
    public async Task<IActionResult> GetPosts(
        int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _communityService.GetPostsAsync(id, page, pageSize);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpPost("{id}/posts")]
    [Authorize]
    public async Task<IActionResult> CreatePost(
        int id, [FromBody] CreatePostDto dto)
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _communityService.CreatePostAsync(id, userId, dto);
        return Ok(ApiResponse<CommunityPostDto>.Ok(
            result, "Post created successfully."));
    }
}