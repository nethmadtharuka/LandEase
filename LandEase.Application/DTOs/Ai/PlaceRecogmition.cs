namespace LandEase.Application.DTOs.Ai;

public class PlaceRecognitionRequestDto
{
    public string ImageBase64 { get; set; } = string.Empty;
    public string MimeType { get; set; } = "image/jpeg";
}

public class PlaceRecognitionResponseDto
{
    public string PlaceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string History { get; set; } = string.Empty;
    public string FamousFor { get; set; } = string.Empty;
    public string TravelTips { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public DateTime RecognizedAt { get; set; } = DateTime.UtcNow;
}