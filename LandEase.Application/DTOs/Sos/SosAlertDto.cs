namespace LandEase.Application.DTOs.Sos;

public class SosAlertDto
{
    public int Id { get; set; }
    public int SosEventId { get; set; }
    public int AlertedUserId { get; set; }
    public string AlertedUserName { get; set; } = string.Empty;
    public bool IsAcknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime SentAt { get; set; }
}