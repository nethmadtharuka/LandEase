using LandEase.API.Models;
using LandEase.Application.DTOs.Sos;
using LandEase.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LandEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SosController : ControllerBase
{
    private readonly ISosService _sosService;

    public SosController(ISosService sosService)
    {
        _sosService = sosService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpPost("trigger")]
    [Authorize(Roles = "Migrant")]
    public async Task<IActionResult> Trigger([FromBody] TriggerSosDto dto)
    {
        try
        {
            var result = await _sosService.TriggerAsync(GetUserId(), dto);
            return Ok(ApiResponse<SosEventDto>.Ok(
                result, "SOS alert triggered successfully. Help is on the way."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<SosEventDto>.Fail(ex.Message));
        }
    }

    [HttpGet("active")]
    [Authorize(Roles = "Helper,Agency,Migrant")]
    public async Task<IActionResult> GetActive()
    {
        try
        {
            var result = await _sosService.GetActiveAsync(GetUserId());
            return Ok(ApiResponse<List<SosEventDto>>.Ok(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<SosEventDto>>.Fail(ex.Message));
        }
    }

    [HttpGet("history")]
    [Authorize(Roles = "Migrant")]
    public async Task<IActionResult> GetHistory()
    {
        try
        {
            var result = await _sosService.GetMyHistoryAsync(GetUserId());
            return Ok(ApiResponse<List<SosEventDto>>.Ok(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<SosEventDto>>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}/acknowledge")]
    [Authorize(Roles = "Helper,Agency")]
    public async Task<IActionResult> Acknowledge(int id)
    {
        try
        {
            var result = await _sosService.AcknowledgeAsync(id, GetUserId());
            return Ok(ApiResponse<SosEventDto>.Ok(
                result, "Alert acknowledged. The migrant has been notified."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<SosEventDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}/resolve")]
    public async Task<IActionResult> Resolve(int id)
    {
        try
        {
            var result = await _sosService.ResolveAsync(id, GetUserId());
            return Ok(ApiResponse<SosEventDto>.Ok(
                result, "SOS event resolved successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<SosEventDto>.Fail(ex.Message));
        }
    }
}