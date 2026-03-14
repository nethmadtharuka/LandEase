namespace LandEase.Application.DTOs.Ai;

public class ChatResponseDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Reply { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}