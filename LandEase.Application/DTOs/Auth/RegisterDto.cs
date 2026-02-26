using LandEase.Domain.Enums;

namespace LandEase.Application.DTOs.Auth;

public class RegisterDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? OriginCountry { get; set; }
    public string? DestinationCountry { get; set; }
    public MigrationStatus MigrationStatus { get; set; }
    public string? PhoneNumber { get; set; }
}