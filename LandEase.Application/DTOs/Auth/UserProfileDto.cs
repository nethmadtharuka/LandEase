using LandEase.Domain.Enums;

namespace LandEase.Application.DTOs.Auth;

public class UserProfileDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? OriginCountry { get; set; }
    public string? DestinationCountry { get; set; }
    public string? PhoneNumber { get; set; }
    public MigrationStatus MigrationStatus { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsKycVerified { get; set; }
    public decimal AverageRating { get; set; }
    public DateTime CreatedAt { get; set; }
}