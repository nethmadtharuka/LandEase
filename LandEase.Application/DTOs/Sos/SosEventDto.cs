using LandEase.Domain.Enums;

namespace LandEase.Application.DTOs.Sos;

public class SosEventDto
{
    public int Id { get; set; }
    public int InitiatedByUserId { get; set; }
    public string InitiatedByUserName { get; set; } = string.Empty;
    public string InitiatedByUserPhone { get; set; } = string.Empty;
    public string DestinationCountry { get; set; } = string.Empty;
    public SosEventType EventType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public SosStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int AlertCount { get; set; }
}