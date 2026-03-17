using LandEase.Domain.Enums;

namespace LandEase.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? OriginCountry { get; set; }
    public string? DestinationCountry { get; set; }
    public MigrationStatus MigrationStatus { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsEmailVerified { get; set; } = false;
    public bool IsKycVerified { get; set; } = false;
    public decimal AverageRating { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public KycRecord? KycRecord { get; set; }

}