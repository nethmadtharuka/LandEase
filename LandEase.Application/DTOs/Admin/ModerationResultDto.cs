namespace LandEase.Application.DTOs.Admin;

public class ModerationResultDto
{
    public bool IsFlagged { get; set; }
    public string Reason { get; set; } = string.Empty;
}