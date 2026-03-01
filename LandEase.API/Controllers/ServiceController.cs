using LandEase.API.Models;
using LandEase.Application.DTOs.Services;
using LandEase.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LandEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly IServiceListingService _serviceListingService;

    public ServicesController(IServiceListingService serviceListingService)
    {
        _serviceListingService = serviceListingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ServiceFilterDto filter)
    {
        try
        {
            var result = await _serviceListingService.GetAllAsync(filter);
            return Ok(ApiResponse<object>.Ok(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await _serviceListingService.GetByIdAsync(id);
            return Ok(ApiResponse<ServiceListingDto>.Ok(result));
        }
        catch (Exception ex)
        {
            return NotFound(ApiResponse<ServiceListingDto>.Fail(ex.Message));
        }
    }

    [HttpGet("provider/{providerId}")]
    public async Task<IActionResult> GetByProvider(int providerId)
    {
        try
        {
            var result = await _serviceListingService.GetByProviderAsync(providerId);
            return Ok(ApiResponse<List<ServiceListingDto>>.Ok(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<ServiceListingDto>>.Fail(ex.Message));
        }
    }

    [HttpPost]
    [Authorize(Roles = "Helper")]
    public async Task<IActionResult> Create([FromBody] CreateServiceDto dto)
    {
        try
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _serviceListingService.CreateAsync(userId, dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<ServiceListingDto>.Ok(result, "Service created successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ServiceListingDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Helper")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceDto dto)
    {
        try
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _serviceListingService.UpdateAsync(id, userId, dto);
            return Ok(ApiResponse<ServiceListingDto>.Ok(
                result, "Service updated successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ServiceListingDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Helper,Agency")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _serviceListingService.DeleteAsync(id, userId);
return Ok(ApiResponse<string>.Ok("", "Service deactivated successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }
}