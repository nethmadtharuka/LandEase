using LandEase.Application.DTOs;
using LandEase.Application.DTOs.Services;

namespace LandEase.Application.Interfaces;

public interface IServiceListingService
{
    Task<ServiceListingDto> CreateAsync(int providerId, CreateServiceDto dto);
    Task<ServiceListingDto> GetByIdAsync(int id);
    Task<PagedResultDto<ServiceListingDto>> GetAllAsync(ServiceFilterDto filter);
    Task<ServiceListingDto> UpdateAsync(
        int serviceId, int requestingUserId, UpdateServiceDto dto);
    Task DeleteAsync(int serviceId, int requestingUserId);
    Task<List<ServiceListingDto>> GetByProviderAsync(int providerId);
}