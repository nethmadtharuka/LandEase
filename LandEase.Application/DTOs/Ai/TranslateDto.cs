namespace LandEase.Application.DTOs.Ai;

public class TranslateDto
{
    public string Text { get; set; } = string.Empty;
    public string FromLanguage { get; set; } = string.Empty;
    public string ToLanguage { get; set; } = string.Empty;
}