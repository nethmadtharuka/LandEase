using LandEase.Domain.Enums;

namespace LandEase.Domain.Entities;

public class SosEvent
{
    public int Id { get; set; }
    public int InitiatedByUserId { get; set; }
    public User InitiatedByUser { get; set; } = null!;

    public SosEventType EventType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    public SosStatus Status { get; set; } = SosStatus.Active;
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SosAlert> Alerts { get; set; } = new List<SosAlert>();
}