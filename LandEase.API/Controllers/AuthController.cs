using LandEase.API.Models;
using LandEase.Application.DTOs.Auth;
using LandEase.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LandEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(ApiResponse<AuthResponseDto>.Ok(
                result, "Registration successful."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail(ex.Message));
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login successful."));
        }
        catch (Exception ex)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail(ex.Message));
        }
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized(
                    ApiResponse<UserProfileDto>.Fail("Invalid token."));

            var result = await _authService.GetProfileAsync(int.Parse(userIdClaim));
            return Ok(ApiResponse<UserProfileDto>.Ok(result));
        }
        catch (Exception ex)
        {
            return NotFound(ApiResponse<UserProfileDto>.Fail(ex.Message));
        }
    }
}