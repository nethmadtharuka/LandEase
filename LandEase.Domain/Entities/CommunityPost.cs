namespace LandEase.Domain.Entities;

public class CommunityPost
{
    public int Id { get; set; }
    public int CommunityId { get; set; }
    public Community Community { get; set; } = null!;
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}