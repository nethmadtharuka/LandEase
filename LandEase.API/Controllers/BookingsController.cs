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
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _bookingService.CreateAsync(userId, dto);
        return Ok(ApiResponse<BookingDto>.Ok(
            result, "Booking created successfully."));
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMyBookings()
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _bookingService.GetMyBookingsAsync(userId);
        return Ok(ApiResponse<List<BookingDto>>.Ok(result));
    }

    [HttpGet("incoming")]
    [Authorize(Roles = "Helper")]
    public async Task<IActionResult> GetIncoming()
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _bookingService.GetIncomingBookingsAsync(userId);
        return Ok(ApiResponse<List<BookingDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _bookingService.GetByIdAsync(id, userId);
        return Ok(ApiResponse<BookingDto>.Ok(result));
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id, [FromBody] UpdateBookingStatusDto dto)
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _bookingService.UpdateStatusAsync(id, userId, dto);
        return Ok(ApiResponse<BookingDto>.Ok(
            result, "Booking status updated successfully."));
    }
}