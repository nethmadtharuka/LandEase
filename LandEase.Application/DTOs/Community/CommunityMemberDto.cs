namespace LandEase.Application.DTOs.Community;

public class CommunityMemberDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string OriginCountry { get; set; } = string.Empty;
    public bool IsKycVerified { get; set; }
    public decimal AverageRating { get; set; }
    public DateTime JoinedAt { get; set; }
}