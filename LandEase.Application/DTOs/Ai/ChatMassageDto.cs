namespace LandEase.Application.DTOs.Ai;

public class ChatMessageDto
{
    public string Message { get; set; } = string.Empty;
    public string? SessionId { get; set; }
}