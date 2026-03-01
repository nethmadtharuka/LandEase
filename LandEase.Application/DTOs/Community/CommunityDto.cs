namespace LandEase.Application.DTOs.Community;

public class CommunityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OriginCountry { get; set; } = string.Empty;
    public string DestinationCountry { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int PostCount { get; set; }
    public bool IsMember { get; set; }
    public DateTime CreatedAt { get; set; }
}