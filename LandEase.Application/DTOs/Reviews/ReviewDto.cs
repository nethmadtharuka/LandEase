namespace LandEase.Application.DTOs.Reviews;

public class ReviewDto
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}