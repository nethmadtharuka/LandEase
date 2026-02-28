using LandEase.API.Models;
using LandEase.Application.DTOs.Bookings;
using LandEase.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LandEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    [Authorize(Roles = "Migrant")]
    public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
    {
        try
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _bookingService.CreateAsync(userId, dto);
            return Ok(ApiResponse<BookingDto>.Ok(
                result, "Booking created successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<BookingDto>.Fail(ex.Message));
        }
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMyBookings()
    {
        try
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _bookingService.GetMyBookingsAsync(userId);
            return Ok(ApiResponse<List<BookingDto>>.Ok(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<BookingDto>>.Fail(ex.Message));
        }
    }

    [HttpGet("incoming")]
    [Authorize(Roles = "Helper")]
    public async Task<IActionResult> GetIncoming()
    {
        try
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _bookingService.GetIncomingBookingsAsync(userId);
            return Ok(ApiResponse<List<BookingDto>>.Ok(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<BookingDto>>.Fail(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _bookingService.GetByIdAsync(id, userId);
            return Ok(ApiResponse<BookingDto>.Ok(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<BookingDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id, [FromBody] UpdateBookingStatusDto dto)
    {
        try
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _bookingService.UpdateStatusAsync(id, userId, dto);
            return Ok(ApiResponse<BookingDto>.Ok(
                result, "Booking status updated successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<BookingDto>.Fail(ex.Message));
        }
    }
}