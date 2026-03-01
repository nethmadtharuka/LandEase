namespace LandEase.Application.DTOs.Community;

public class CommunityPostDto
{
    public int Id { get; set; }
    public int CommunityId { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public bool AuthorIsKycVerified { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}