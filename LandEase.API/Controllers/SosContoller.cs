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
        var result = await _sosService.TriggerAsync(GetUserId(), dto);
        return Ok(ApiResponse<SosEventDto>.Ok(
            result, "SOS alert triggered successfully. Help is on the way."));
    }

    [HttpGet("active")]
    [Authorize(Roles = "Helper,Agency,Migrant")]
    public async Task<IActionResult> GetActive()
    {
        var result = await _sosService.GetActiveAsync(GetUserId());
        return Ok(ApiResponse<List<SosEventDto>>.Ok(result));
    }

    [HttpGet("history")]
    [Authorize(Roles = "Migrant")]
    public async Task<IActionResult> GetHistory()
    {
        var result = await _sosService.GetMyHistoryAsync(GetUserId());
        return Ok(ApiResponse<List<SosEventDto>>.Ok(result));
    }

    [HttpPut("{id}/acknowledge")]
    [Authorize(Roles = "Helper,Agency")]
    public async Task<IActionResult> Acknowledge(int id)
    {
        var result = await _sosService.AcknowledgeAsync(id, GetUserId());
        return Ok(ApiResponse<SosEventDto>.Ok(
            result, "Alert acknowledged. The migrant has been notified."));
    }

    [HttpPut("{id}/resolve")]
    public async Task<IActionResult> Resolve(int id)
    {
        var result = await _sosService.ResolveAsync(id, GetUserId());
        return Ok(ApiResponse<SosEventDto>.Ok(
            result, "SOS event resolved successfully."));
    }
}