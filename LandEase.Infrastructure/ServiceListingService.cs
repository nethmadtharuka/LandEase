using LandEase.Application.DTOs;
using LandEase.Application.DTOs.Services;
using LandEase.Application.Interfaces;
using LandEase.Domain.Entities;
using LandEase.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LandEase.Infrastructure;

public class ServiceListingService : IServiceListingService
{
    private readonly AppDbContext _context;

    public ServiceListingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceListingDto> CreateAsync(
        int providerId, CreateServiceDto dto)
    {
        var provider = await _context.Users.FindAsync(providerId)
            ?? throw new Exception("Provider not found.");

        if (!provider.IsKycVerified)
            throw new Exception(
                "You must complete KYC verification before posting services.");

        var service = new ServiceListing
        {
            ProviderId = providerId,
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            Price = dto.Price,
            DestinationCountry = dto.DestinationCountry.Trim()
        };

        _context.ServiceListings.Add(service);
        await _context.SaveChangesAsync();

        return MapToDto(service, provider);
    }

    public async Task<ServiceListingDto> GetByIdAsync(int id)
    {
        var service = await _context.ServiceListings
            .Include(s => s.Provider)
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new Exception("Service not found.");

        return MapToDto(service, service.Provider);
    }

    public async Task<PagedResultDto<ServiceListingDto>> GetAllAsync(
        ServiceFilterDto filter)
    {
        var query = _context.ServiceListings
            .Include(s => s.Provider)
            .Where(s => s.IsActive)
            .AsQueryable();

        if (filter.Category.HasValue)
            query = query.Where(s => s.Category == filter.Category.Value);

        if (!string.IsNullOrWhiteSpace(filter.DestinationCountry))
            query = query.Where(s => s.DestinationCountry.ToLower()
                .Contains(filter.DestinationCountry.ToLower()));

        if (filter.MaxPrice.HasValue)
            query = query.Where(s => s.Price <= filter.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            query = query.Where(s =>
                s.Title.ToLower().Contains(filter.SearchTerm.ToLower()) ||
                s.Description.ToLower().Contains(filter.SearchTerm.ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.Provider.AverageRating)
            .ThenByDescending(s => s.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResultDto<ServiceListingDto>
        {
            Items = items.Select(s => MapToDto(s, s.Provider)).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<ServiceListingDto> UpdateAsync(
        int serviceId, int requestingUserId, UpdateServiceDto dto)
    {
        var service = await _context.ServiceListings
            .Include(s => s.Provider)
            .FirstOrDefaultAsync(s => s.Id == serviceId)
            ?? throw new Exception("Service not found.");

        if (service.ProviderId != requestingUserId)
            throw new Exception("You are not authorized to update this service.");

        service.Title = dto.Title;
        service.Description = dto.Description;
        service.Category = dto.Category;
        service.Price = dto.Price;
        service.DestinationCountry = dto.DestinationCountry.Trim();
        service.IsActive = dto.IsActive;
        service.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(service, service.Provider);
    }

    public async Task DeleteAsync(int serviceId, int requestingUserId)
    {
        var service = await _context.ServiceListings
            .FirstOrDefaultAsync(s => s.Id == serviceId)
            ?? throw new Exception("Service not found.");

        if (service.ProviderId != requestingUserId)
            throw new Exception("You are not authorized to delete this service.");

        service.IsActive = false;
        service.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<List<ServiceListingDto>> GetByProviderAsync(int providerId)
    {
        return await _context.ServiceListings
            .Include(s => s.Provider)
            .Where(s => s.ProviderId == providerId && s.IsActive)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => MapToDto(s, s.Provider))
            .ToListAsync();
    }

    private static ServiceListingDto MapToDto(ServiceListing service, User provider) => new()
    {
        Id = service.Id,
        ProviderId = service.ProviderId,
        ProviderName = provider.FullName,
        ProviderRating = provider.AverageRating,
        ProviderIsKycVerified = provider.IsKycVerified,
        Title = service.Title,
        Description = service.Description,
        Category = service.Category,
        Price = service.Price,
        DestinationCountry = service.DestinationCountry,
        IsActive = service.IsActive,
        CreatedAt = service.CreatedAt
    };
}