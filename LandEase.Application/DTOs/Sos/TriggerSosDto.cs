using LandEase.Domain.Enums;

namespace LandEase.Application.DTOs.Sos;

public class TriggerSosDto
{
    public SosEventType EventType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
     public bool NotifyMigrants { get; set; } = false; // ADD THIS

}