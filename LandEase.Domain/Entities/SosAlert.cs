namespace LandEase.Domain.Entities;

public class SosAlert
{
    public int Id { get; set; }
    public int SosEventId { get; set; }
    public SosEvent SosEvent { get; set; } = null!;

    public int AlertedUserId { get; set; }
    public User AlertedUser { get; set; } = null!;

    public bool IsAcknowledged { get; set; } = false;
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}