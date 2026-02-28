using LandEase.Domain.Enums;

namespace LandEase.Application.DTOs.Services;

public class ServiceFilterDto
{
    public ServiceCategory? Category { get; set; }
    public string? DestinationCountry { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}