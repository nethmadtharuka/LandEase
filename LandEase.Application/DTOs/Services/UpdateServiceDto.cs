using LandEase.Domain.Enums;

namespace LandEase.Application.DTOs.Services;

public class UpdateServiceDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ServiceCategory Category { get; set; }
    public decimal Price { get; set; }
    public string DestinationCountry { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}