using LandEase.Domain.Enums;

namespace LandEase.Application.DTOs.Services;

public class ServiceListingDto
{
    public int Id { get; set; }
    public int ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public decimal ProviderRating { get; set; }
    public bool ProviderIsKycVerified { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ServiceCategory Category { get; set; }
    public decimal Price { get; set; }
    public string DestinationCountry { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}