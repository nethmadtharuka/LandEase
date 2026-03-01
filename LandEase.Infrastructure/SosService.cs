using LandEase.Application.DTOs.Sos;
using LandEase.Application.Interfaces;
using LandEase.Domain.Entities;
using LandEase.Domain.Enums;
using LandEase.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using LandEase.Infrastructure.Hubs;

namespace LandEase.Infrastructure;

public class SosService : ISosService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<SosHub> _hubContext;

    public SosService(AppDbContext context, IHubContext<SosHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<SosEventDto> TriggerAsync(int userId, TriggerSosDto dto)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new Exception("User not found.");

        // Cancel any existing active SOS from this user
        var existingActive = await _context.SosEvents
            .Where(s => s.InitiatedByUserId == userId &&
                        s.Status == SosStatus.Active)
            .ToListAsync();

        foreach (var existing in existingActive)
        {
            existing.Status = SosStatus.Cancelled;
        }

        // Create new SOS event
        var sosEvent = new SosEvent
        {
            InitiatedByUserId = userId,
            EventType = dto.EventType,
            Description = dto.Description,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Status = SosStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _context.SosEvents.Add(sosEvent);
        await _context.SaveChangesAsync();

        // Find all Helpers and Agencies in the same destination country
        var eligibleUsers = await _context.Users
    .Where(u =>
        u.DestinationCountry == user.DestinationCountry &&
        u.Id != userId &&
        (u.Role == UserRole.Helper ||
         u.Role == UserRole.Agency ||
         (dto.NotifyMigrants && u.Role == UserRole.Migrant)))
    .ToListAsync();
        // Create SosAlert records for each eligible user
        foreach (var eligibleUser in eligibleUsers)
        {
            _context.SosAlerts.Add(new SosAlert
            {
                SosEventId = sosEvent.Id,
                AlertedUserId = eligibleUser.Id,
                SentAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        // Build the DTO for broadcasting
        var sosEventDto = MapToDto(sosEvent, user, eligibleUsers.Count);

        // Broadcast via SignalR to the destination country group
        await _hubContext.Clients
            .Group(user.DestinationCountry!.ToLower().Trim())
            .SendAsync("ReceiveSosAlert", sosEventDto);

        return sosEventDto;
    }

    public async Task<List<SosEventDto>> GetActiveAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new Exception("User not found.");

        return await _context.SosEvents
            .Include(s => s.InitiatedByUser)
            .Include(s => s.Alerts)
            .Where(s =>
                s.Status == SosStatus.Active &&
                s.InitiatedByUser.DestinationCountry == user.DestinationCountry)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => MapToDto(s, s.InitiatedByUser, s.Alerts.Count))
            .ToListAsync();
    }

    public async Task<List<SosEventDto>> GetMyHistoryAsync(int userId)
    {
        return await _context.SosEvents
            .Include(s => s.InitiatedByUser)
            .Include(s => s.Alerts)
            .Where(s => s.InitiatedByUserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => MapToDto(s, s.InitiatedByUser, s.Alerts.Count))
            .ToListAsync();
    }

    public async Task<SosEventDto> AcknowledgeAsync(int sosEventId, int userId)
    {
        var alert = await _context.SosAlerts
            .FirstOrDefaultAsync(a =>
                a.SosEventId == sosEventId && a.AlertedUserId == userId)
            ?? throw new Exception(
                "No alert found for this SOS event for your account.");

        if (alert.IsAcknowledged)
            throw new Exception("You have already acknowledged this alert.");

        alert.IsAcknowledged = true;
        alert.AcknowledgedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var sosEvent = await _context.SosEvents
            .Include(s => s.InitiatedByUser)
            .Include(s => s.Alerts)
            .FirstAsync(s => s.Id == sosEventId);

        var responder = await _context.Users.FindAsync(userId);

        // Notify the migrant that help is coming via SignalR
        await _hubContext.Clients
            .Group(sosEvent.InitiatedByUser.DestinationCountry!.ToLower().Trim())
            .SendAsync("SosAcknowledged", new
            {
                SosEventId = sosEventId,
                ResponderId = userId,
                ResponderName = responder!.FullName,
                AcknowledgedAt = alert.AcknowledgedAt
            });

        return MapToDto(sosEvent, sosEvent.InitiatedByUser, sosEvent.Alerts.Count);
    }

    public async Task<SosEventDto> ResolveAsync(int sosEventId, int userId)
    {
        var sosEvent = await _context.SosEvents
            .Include(s => s.InitiatedByUser)
            .Include(s => s.Alerts)
            .FirstOrDefaultAsync(s => s.Id == sosEventId)
            ?? throw new Exception("SOS event not found.");

        if (sosEvent.Status != SosStatus.Active)
            throw new Exception("This SOS event is no longer active.");

        var isInitiator = sosEvent.InitiatedByUserId == userId;
        var isHelper = await _context.SosAlerts
            .AnyAsync(a => a.SosEventId == sosEventId && a.AlertedUserId == userId);

        if (!isInitiator && !isHelper)
            throw new Exception(
                "You are not authorized to resolve this SOS event.");

        sosEvent.Status = SosStatus.Resolved;
        sosEvent.ResolvedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Broadcast resolution to country group
        await _hubContext.Clients
            .Group(sosEvent.InitiatedByUser.DestinationCountry!.ToLower().Trim())
            .SendAsync("SosResolved", new
            {
                SosEventId = sosEventId,
                ResolvedAt = sosEvent.ResolvedAt
            });

        return MapToDto(sosEvent, sosEvent.InitiatedByUser, sosEvent.Alerts.Count);
    }

    private static SosEventDto MapToDto(
        SosEvent sosEvent, User initiator, int alertCount) => new()
    {
        Id = sosEvent.Id,
        InitiatedByUserId = sosEvent.InitiatedByUserId,
        InitiatedByUserName = initiator.FullName,
        InitiatedByUserPhone = initiator.PhoneNumber ?? string.Empty,
        DestinationCountry = initiator.DestinationCountry ?? string.Empty,
        EventType = sosEvent.EventType,
        Description = sosEvent.Description,
        Latitude = sosEvent.Latitude,
        Longitude = sosEvent.Longitude,
        Status = sosEvent.Status,
        CreatedAt = sosEvent.CreatedAt,
        ResolvedAt = sosEvent.ResolvedAt,
        AlertCount = alertCount
    };
}